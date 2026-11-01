//
// EncodedWordValidator.cs
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

using MimeKit.Encodings;
using MimeKit.Utils;

namespace MimeKit {
	enum EncodedWordContext {
		Text,
		Phrase,
		Comment,
		Forbidden
	}

	// A forward-only scanner shared by unstructured headers and finalized address ranges.
	// The RFC's 75-byte word limit bounds lookahead even for overlapping damaged candidates.
	struct EncodedWordValidator
	{
		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		readonly byte[] text;
		readonly long streamOffset;
		readonly int startIndex;
		readonly int endIndex;
		int index;
		int scanIndex;
		int line;
		int column;
		int commentDepth;
		bool quoted;
		bool literal;

		public EncodedWordValidator (IMimeComplianceLogger logger, MimeComplianceContext context, byte[] text, int startIndex, int length, long streamOffset, int line, int column)
		{
			this.logger = logger;
			this.context = context;
			this.text = text;
			this.startIndex = startIndex;
			this.endIndex = startIndex + length;
			this.streamOffset = streamOffset;
			this.line = line;
			this.column = column;
			this.index = startIndex;
			this.scanIndex = startIndex;
			this.commentDepth = 0;
			this.quoted = this.literal = false;
		}

		static bool IsToken (byte c)
		{
			return c.IsToken () && c != (byte) '.';
		}

		static bool IsAlphaNumeric (byte c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
		}

		bool ReadWord (EncodedWordContext location, out int end, out bool complete)
		{
			int limit = index + Math.Min (endIndex - index, 75);
			int at = index + 2;

			complete = false;
			end = at;

			int charset = at;
			while (at < limit && IsToken (text[at]))
				at++;

			if (at == charset || text[charset] == (byte) '*' || at >= limit || text[at++] != (byte) '?')
				return false;

			int encoding = at;
			while (at < limit && IsToken (text[at]))
				at++;

			if (at == encoding || at >= limit || text[at++] != (byte) '?')
				return false;

			byte kind = text[encoding];
			bool base64 = kind == (byte) 'B' || kind == (byte) 'b';
			bool valid = at - encoding == 2 && (base64 || kind == (byte) 'Q' || kind == (byte) 'q');
			int payload = at;
			int padding = 0;
			int hex = 0;

			for (; at < limit; at++) {
				byte c = text[at];

				if (c == (byte) '?' && at + 1 < limit && text[at + 1] == (byte) '=') {
					end = at + 2;
					complete = true;

					if (at == payload || hex != 0 || (base64 && ((at - payload) % 4 != 0 || padding > 2)))
						valid = false;

					if (valid && base64 && padding > 0) {
						int mask = padding == 2 ? 0x0f : 0x03;

						if ((Base64Decoder.GetRank (text[at - padding - 1]) & mask) != 0)
							valid = false;
					}

					return valid;
				}

				// Leave a nested candidate to the next iteration instead of hiding it in this one.
				if (c == (byte) '=' && at + 2 < limit && text[at + 1] == (byte) '?' && text[at + 2] != (byte) '=')
					return false;

				if (c <= 32 || c >= 127 || c == (byte) '?')
					valid = false;

				if (base64) {
					if (c == (byte) '=') {
						padding++;
					} else if (padding != 0 || !(IsAlphaNumeric (c) || c == (byte) '+' || c == (byte) '/')) {
						valid = false;
					}
				} else if (hex > 0) {
					if (!c.IsXDigit ())
						valid = false;
					hex--;
				} else if (c == (byte) '=') {
					hex = 2;
				} else if (location == EncodedWordContext.Phrase) {
					if (!c.IsType (CharType.IsEncodedPhraseSafe) && c != (byte) '_')
						valid = false;
				} else if (location == EncodedWordContext.Comment && (c == (byte) '(' || c == (byte) ')' || c == (byte) '\\')) {
					valid = false;
				}
			}

			return false;
		}

		bool IsBoundary (int at, EncodedWordContext location)
		{
			if (at < startIndex || at >= endIndex || text[at].IsWhitespace ())
				return true;

			return location == EncodedWordContext.Comment && (text[at] == (byte) '(' || text[at] == (byte) ')');
		}

		void Log (MimeComplianceViolation violation)
		{
			while (scanIndex < index) {
				int newline = text.AsSpan (scanIndex, index - scanIndex).IndexOf ((byte) '\n');

				if (newline < 0) {
					column += index - scanIndex;
					scanIndex = index;
					break;
				}

				scanIndex += newline + 1;
				line++;
				column = 1;
			}

			logger.Log (new MimeComplianceIssue (context, violation, streamOffset + index - startIndex, line, column, MimeCompliancePositionKind.ElementStart));
		}

		public void Validate (int end, EncodedWordContext location)
		{
			if (index == startIndex && index < end && text.AsSpan (startIndex, endIndex - startIndex).IndexOf ("=?"u8) < 0) {
				index = endIndex;
				return;
			}

			while (index < end) {
				byte c = text[index];
				var wordContext = location;

				if (location == EncodedWordContext.Text) {
					if (c != (byte) '=') {
						int next = text.AsSpan (index, end - index).IndexOf ((byte) '=');

						if (next < 0) {
							index = end;
							return;
						}

						index += next;
						c = text[index];
					}
				} else {
					if (commentDepth > 0)
						wordContext = EncodedWordContext.Comment;
					else if (quoted || literal)
						wordContext = EncodedWordContext.Forbidden;

					if (c == (byte) '\\' && (commentDepth > 0 || quoted || literal)) {
						index = Math.Min (index + 2, endIndex);
						continue;
					}

					if (c == (byte) '(' && !quoted && !literal)
						commentDepth++;
					else if (c == (byte) ')' && commentDepth > 0)
						commentDepth--;
					else if (commentDepth == 0 && !literal && c == (byte) '"')
						quoted = !quoted;
					else if (commentDepth == 0 && !quoted && c == (byte) '[')
						literal = true;
					else if (literal && c == (byte) ']')
						literal = false;
				}

				if (c == (byte) '=' && index + 1 < endIndex && text[index + 1] == (byte) '?') {
					bool valid = ReadWord (wordContext, out int wordEnd, out bool complete);

					// In an addr-spec, '=' and '?' can be ordinary local-part characters.
					// Only a complete envelope is evidence of misplaced encoded-word syntax.
					if (wordContext != EncodedWordContext.Forbidden || complete) {
						if (wordContext == EncodedWordContext.Forbidden)
							Log (MimeComplianceViolation.InvalidEncodedWordPlacement);
						else if (!IsBoundary (index - 1, wordContext) || !IsBoundary (wordEnd, wordContext))
							valid = false;

						if (!valid)
							Log (MimeComplianceViolation.InvalidEncodedWord);
					}

					if (complete) {
						index = wordEnd;
						continue;
					}
				}

				index++;
			}
		}
	}
}
