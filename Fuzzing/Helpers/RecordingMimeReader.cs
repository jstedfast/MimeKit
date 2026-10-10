//
// RecordingMimeReader.cs
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

using System.Text;

namespace MimeKit.Fuzzing.Helpers {
	// A MimeReader that records every callback into a textual trace and validates the callback arguments.
	//
	// The *Read callbacks may be delivered in arbitrarily sized chunks (and the chunking legitimately differs
	// between the sync and async code paths), so consecutive reads of the same kind are folded into a single
	// running hash + length that is flushed when the next non-read callback arrives. This makes the trace
	// independent of buffering.
	sealed class RecordingMimeReader : MimeReader
	{
		readonly StringBuilder trace = new StringBuilder ();
		readonly long streamLength;
		string? pendingKind;
		ulong pendingHash;
		long pendingLength;

		public RecordingMimeReader (ParserOptions options, Stream stream, MimeFormat format) : base (options, stream, format)
		{
			streamLength = stream.Length;
		}

		public string Trace {
			get {
				Flush ();
				return trace.ToString ();
			}
		}

		public void Append (string text)
		{
			Flush ();
			trace.AppendLine (text);
		}

		void Flush ()
		{
			if (pendingKind == null)
				return;

			trace.Append (pendingKind).Append (' ').Append (pendingLength).Append (' ').Append (pendingHash.ToString ("x16")).AppendLine ();
			pendingKind = null;
		}

		void Read (string kind, byte[] buffer, int startIndex, int count)
		{
			Fuzz.Check (buffer != null, $"{kind}: buffer is null");
			Fuzz.Check (startIndex >= 0 && count >= 0 && startIndex <= buffer!.Length - count, $"{kind}: invalid buffer range ({startIndex}, {count}) for a buffer of {buffer!.Length} bytes");

			if (pendingKind != kind) {
				Flush ();
				pendingKind = kind;
				pendingHash = 14695981039346656037UL;
				pendingLength = 0;
			}

			pendingHash = Fuzz.Hash (buffer.AsSpan (startIndex, count), pendingHash);
			pendingLength += count;
		}

		void Offset (string kind, long offset)
		{
			Fuzz.Check (offset >= 0 && offset <= streamLength, $"{kind}: offset {offset} is outside of the stream [0, {streamLength}]");
		}

		void Line (string kind, int lineNumber)
		{
			Fuzz.Check (lineNumber >= 1, $"{kind}: invalid line number {lineNumber}");
		}

		void Range (string kind, long beginOffset, long endOffset)
		{
			Offset (kind, beginOffset);
			Offset (kind, endOffset);
			Fuzz.Check (beginOffset <= endOffset, $"{kind}: begin offset {beginOffset} is after end offset {endOffset}");
		}

		void Begin (string kind, long beginOffset, int lineNumber)
		{
			Offset (kind, beginOffset);
			Line (kind, lineNumber);
			Append ($"{kind} {beginOffset} {lineNumber}");
		}

		void End (string kind, long beginOffset, int lineNumber, long endOffset)
		{
			Range (kind, beginOffset, endOffset);
			Line (kind, lineNumber);
			Append ($"{kind} {beginOffset} {lineNumber} {endOffset}");
		}

		void EndWithLines (string kind, long beginOffset, int lineNumber, long endOffset, int lines)
		{
			Range (kind, beginOffset, endOffset);
			Line (kind, lineNumber);
			Fuzz.Check (lines >= 0, $"{kind}: negative line count {lines}");
			Append ($"{kind} {beginOffset} {lineNumber} {endOffset} {lines}");
		}

		void EntityEnd (string kind, ContentType? contentType, long beginOffset, int lineNumber, long headersEndOffset, long endOffset, int lines)
		{
			Range (kind, beginOffset, endOffset);
			Range (kind, beginOffset, headersEndOffset);
			Fuzz.Check (headersEndOffset <= endOffset, $"{kind}: headers end offset {headersEndOffset} is after end offset {endOffset}");
			Line (kind, lineNumber);
			Fuzz.Check (lines >= 0, $"{kind}: negative line count {lines} (begin {beginOffset}, line {lineNumber}, headers end {headersEndOffset}, end {endOffset})");
			Append ($"{kind} {contentType?.MimeType} {beginOffset} {lineNumber} {headersEndOffset} {endOffset} {lines}");
		}

		void EntityBegin (string kind, ContentType contentType, long beginOffset, int lineNumber)
		{
			Fuzz.Check (contentType != null, $"{kind}: content type is null");
			Offset (kind, beginOffset);
			Line (kind, lineNumber);
			Append ($"{kind} {contentType!.MimeType} {beginOffset} {lineNumber}");
		}

		protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			=> Begin ("MboxMarkerBegin", beginOffset, lineNumber);

		protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			=> Read ("MboxMarkerRead", buffer, startIndex, count);

		protected override void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			=> End ("MboxMarkerEnd", beginOffset, lineNumber, endOffset);

		protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> Begin ("HeadersBegin", beginOffset, beginLineNumber);

		protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
			Fuzz.Check (header != null, "HeaderRead: header is null");
			Line ("HeaderRead", beginLineNumber);

			if (header!.Offset.HasValue)
				Offset ("HeaderRead", header.Offset.Value);

			Append ($"HeaderRead {Fuzz.Escape (header.Field)} {header.Offset} {beginLineNumber} {Fuzz.Hash (header.RawField):x16} {header.RawValue.Length} {Fuzz.Hash (header.RawValue):x16}");
		}

		protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
		{
			Range ("HeadersEnd", beginOffset, endOffset);
			Line ("HeadersEnd", beginLineNumber);
			Line ("HeadersEnd", endLineNumber);
			Fuzz.Check (beginLineNumber <= endLineNumber, $"HeadersEnd: begin line {beginLineNumber} is after end line {endLineNumber}");
			Append ($"HeadersEnd {beginOffset} {beginLineNumber} {endOffset} {endLineNumber}");
		}

		protected override void OnBodySeparator (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			=> End ("BodySeparator", beginOffset, lineNumber, endOffset);

		protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> Begin ("MimeMessageBegin", beginOffset, beginLineNumber);

		protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			=> EntityEnd ("MimeMessageEnd", null, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);

		protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> EntityBegin ("MimePartBegin", contentType, beginOffset, beginLineNumber);

		protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> Begin ("MimePartContentBegin", beginOffset, beginLineNumber);

		protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			=> Read ("MimePartContentRead", buffer, startIndex, count);

		protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
		{
			EndWithLines ("MimePartContentEnd", beginOffset, beginLineNumber, endOffset, lines);
			Append ($"  newline {newLineFormat}");
		}

		protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			=> EntityEnd ("MimePartEnd", contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);

		protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> EntityBegin ("MessagePartBegin", contentType, beginOffset, beginLineNumber);

		protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			=> EntityEnd ("MessagePartEnd", contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);

		protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> EntityBegin ("MultipartBegin", contentType, beginOffset, beginLineNumber);

		protected override void OnMultipartBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			=> Begin ("MultipartBoundaryBegin", beginOffset, lineNumber);

		protected override void OnMultipartBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			Offset ("MultipartBoundaryRead", beginOffset);
			Line ("MultipartBoundaryRead", lineNumber);
			Read ("MultipartBoundaryRead", buffer, startIndex, count);
		}

		protected override void OnMultipartBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			=> End ("MultipartBoundaryEnd", beginOffset, lineNumber, endOffset);

		protected override void OnMultipartEndBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			=> Begin ("MultipartEndBoundaryBegin", beginOffset, lineNumber);

		protected override void OnMultipartEndBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			Offset ("MultipartEndBoundaryRead", beginOffset);
			Line ("MultipartEndBoundaryRead", lineNumber);
			Read ("MultipartEndBoundaryRead", buffer, startIndex, count);
		}

		protected override void OnMultipartEndBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			=> End ("MultipartEndBoundaryEnd", beginOffset, lineNumber, endOffset);

		protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> Begin ("MultipartPreambleBegin", beginOffset, beginLineNumber);

		protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			=> Read ("MultipartPreambleRead", buffer, startIndex, count);

		protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			=> EndWithLines ("MultipartPreambleEnd", beginOffset, beginLineNumber, endOffset, lines);

		protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			=> Begin ("MultipartEpilogueBegin", beginOffset, beginLineNumber);

		protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			=> Read ("MultipartEpilogueRead", buffer, startIndex, count);

		protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			=> EndWithLines ("MultipartEpilogueEnd", beginOffset, beginLineNumber, endOffset, lines);

		protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			=> EntityEnd ("MultipartEnd", contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
	}
}
