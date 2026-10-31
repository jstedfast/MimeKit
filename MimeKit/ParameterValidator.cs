//
// ParameterValidator.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Text;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	// Inspect raw declarations independently of parameter decoding and precedence.
	sealed class ParameterValidator
	{
		sealed class ParameterState
		{
			// Unlike HashSet, a balanced tree avoids attacker-controlled integer hash
			// collisions making insertion quadratic.
			public SortedSet<int>? Segments;
			public MimeComplianceIssue MissingSegmentIssue;
			public bool Extended;
			public bool Plain;

			int highestSegment = -1;

			// Distinct nonnegative indices cover 0..n-1 iff their maximum is n-1.
			public bool HasMissingSegments => Segments != null && highestSegment != Segments.Count - 1;

			public bool Add (int segment, bool encoded, bool allowFallback)
			{
				bool repeated;

				if (segment >= 0) {
					repeated = false;

					if (Segments is null) {
						Segments = new SortedSet<int> ();
						repeated = Extended || (Plain && !allowFallback);
					}

					highestSegment = Math.Max (highestSegment, segment);

					return !Segments.Add (segment) || repeated;
				}

				if (encoded) {
					repeated = Extended || Segments != null || (Plain && !allowFallback);
					Extended = true;
				} else {
					repeated = Plain || (!allowFallback && (Extended || Segments != null));
					Plain = true;
				}

				return repeated;
			}
		}

		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		readonly HeaderId header;
		readonly long streamOffset;
		readonly int lineNumber;
		readonly int columnNumber;

		byte[] text = Array.Empty<byte> ();
		int startIndex;
		int endIndex;
		int index;
		int scanIndex;
		int scanLine;
		int scanColumn;

		public ParameterValidator (IMimeComplianceLogger logger, MimeComplianceContext context, HeaderId header, long streamOffset, int lineNumber, int columnNumber)
		{
			this.logger = logger;
			this.context = context;
			this.header = header;
			this.streamOffset = streamOffset;
			this.lineNumber = lineNumber;
			this.columnNumber = columnNumber;
		}

		MimeComplianceIssue CreateIssue (MimeComplianceViolation violation, int at, MimeCompliancePositionKind positionKind = MimeCompliancePositionKind.Exact)
		{
			while (scanIndex < at) {
				if (text[scanIndex++] == (byte) '\n') {
					scanLine++;
					scanColumn = 1;
				} else {
					scanColumn++;
				}
			}

			return new MimeComplianceIssue (context, violation, streamOffset + at - startIndex, scanLine, scanColumn, positionKind);
		}

		bool SkipCommentsAndWhiteSpace ()
		{
			return ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, false);
		}

		bool SkipToNextParameter ()
		{
			while (index < endIndex) {
				switch (text[index]) {
				case (byte) ';':
					index++;
					return true;
				case (byte) '"':
					if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false))
						return false;
					break;
				case (byte) '(':
					if (!ParseUtils.SkipComment (text, ref index, endIndex))
						return false;
					break;
				default:
					index++;
					break;
				}
			}

			return false;
		}

		bool TryParseParameter (out string name, out int segment, out bool encoded, out int start)
		{
			name = string.Empty;
			segment = -1;
			encoded = false;
			start = index;

			while (index < endIndex && text[index].IsAttr ())
				index++;

			int length = index - start;

			if (length == 0 || !SkipCommentsAndWhiteSpace () || index == endIndex)
				return false;

			if (text[index] == (byte) '*') {
				index++;

				if (!SkipCommentsAndWhiteSpace () || index == endIndex)
					return false;

				if (text[index] >= (byte) '0' && text[index] <= (byte) '9') {
					if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out segment))
						return false;

					if (!SkipCommentsAndWhiteSpace () || index == endIndex)
						return false;

					if (text[index] == (byte) '*') {
						index++;
						encoded = true;
					}
				} else {
					encoded = true;
				}

				if (!SkipCommentsAndWhiteSpace () || index == endIndex)
					return false;
			}

			if (text[index] != (byte) '=')
				return false;

			index++;

			name = Encoding.ASCII.GetString (text, start, length);

			return true;
		}

		bool SkipValue (bool allowEmpty, out bool invalid, out int valueStart, out int valueEnd)
		{
			invalid = true;
			valueStart = valueEnd = index;

			if (!SkipCommentsAndWhiteSpace ())
				return false;

			valueStart = valueEnd = index;

			if (index == endIndex) {
				invalid = !allowEmpty;
				return false;
			}

			if (text[index] == (byte) '"') {
				if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false))
					return false;

				valueEnd = index;
				invalid = false;
			} else {
				while (index < endIndex && text[index] != (byte) ';' && text[index] != (byte) '(')
					index++;

				valueEnd = index;
				while (valueEnd > valueStart && text[valueEnd - 1].IsWhitespace ())
					valueEnd--;

				invalid = valueEnd == valueStart && !allowEmpty;
			}

			if (!SkipCommentsAndWhiteSpace ()) {
				invalid = true;
				return false;
			}

			if (index == endIndex)
				return false;

			// Loose parameter parsing also accepts a missing separator after a quote or comment.
			if (text[index] == (byte) ';')
				index++;

			return index < endIndex;
		}

		bool ValidateEncodedValue (int start, int end, bool initial)
		{
			// Match the decoder's tolerance for quoted extended values.
			if (end - start >= 2 && text[start] == (byte) '"' && text[end - 1] == (byte) '"') {
				start++;
				end--;
			}

			if (initial) {
				for (int field = 0; field < 2; field++) {
					while (start < end && text[start] != (byte) '\'') {
						if (text[start] <= 32 || text[start] >= 127)
							return false;
						start++;
					}

					if (start == end)
						return false;

					start++;
				}
			}

			// RFC 2231 section 7 requires complete ext-octets in each segment,
			// even though a multibyte character may span several segments.
			while (start < end) {
				if (text[start] == (byte) '%') {
					if (end - start < 3 || !text[start + 1].IsXDigit () || !text[start + 2].IsXDigit ())
						return false;
					start += 3;
				} else {
					if (!text[start].IsAttr ())
						return false;
					start++;
				}
			}

			return true;
		}

		public void Validate (byte[] buffer, int startIndex, int length)
		{
			text = buffer;
			this.startIndex = startIndex;
			endIndex = startIndex + length;
			index = startIndex;
			scanIndex = startIndex;
			scanLine = lineNumber;
			scanColumn = columnNumber;

			if (!SkipToNextParameter ())
				return;

			// The reader's MaxHeaderLength bounds the number and size of declarations.
			var parameters = new Dictionary<string, ParameterState> (MimeUtils.OrdinalIgnoreCase);

			while (index < endIndex) {
				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

				int start = index;

				if (!SkipCommentsAndWhiteSpace ()) {
					logger.Log (CreateIssue (MimeComplianceViolation.InvalidParameter, start, MimeCompliancePositionKind.ElementStart));
					break;
				}

				if (index == endIndex)
					break;

				// Empty parameter slots are also ignored by ParameterList.
				if (text[index] == (byte) ';') {
					index++;
					continue;
				}

				if (!TryParseParameter (out var name, out int segment, out bool encoded, out start)) {
					logger.Log (CreateIssue (MimeComplianceViolation.InvalidParameter, start, MimeCompliancePositionKind.ElementStart));
					if (!SkipToNextParameter ())
						break;
					continue;
				}

				bool filename = header == HeaderId.ContentType ?
					name.Equals ("name", StringComparison.OrdinalIgnoreCase) :
					name.Equals ("filename", StringComparison.OrdinalIgnoreCase);

				if (!parameters.TryGetValue (name, out var state)) {
					state = new ParameterState ();
					parameters.Add (name, state);
				}

				// Capture the first segment's position before later issues advance the scan cursor.
				if (segment >= 0 && state.Segments is null)
					state.MissingSegmentIssue = CreateIssue (MimeComplianceViolation.MissingParameterSegment, start, MimeCompliancePositionKind.ElementStart);

				if (state.Add (segment, encoded, filename)) {
					var violation = MimeComplianceViolation.RepeatedParameter;

					if (header == HeaderId.ContentType && name.Equals ("boundary", StringComparison.OrdinalIgnoreCase))
						violation = MimeComplianceViolation.RepeatedBoundaryParameter;
					else if (filename)
						violation = MimeComplianceViolation.RepeatedFilenameParameter;

					logger.Log (CreateIssue (violation, start));
				}

				bool more = SkipValue (encoded && segment > 0, out bool invalid, out int valueStart, out int valueEnd);

				if (invalid)
					logger.Log (CreateIssue (MimeComplianceViolation.InvalidParameter, start, MimeCompliancePositionKind.ElementStart));
				else if (encoded && !ValidateEncodedValue (valueStart, valueEnd, segment <= 0))
					logger.Log (CreateIssue (MimeComplianceViolation.InvalidRfc2231Parameter, start, MimeCompliancePositionKind.ElementStart));

				if (!more)
					break;
			}

			foreach (var state in parameters.Values) {
				if (state.HasMissingSegments)
					logger.Log (state.MissingSegmentIssue);
			}
		}
	}
}
