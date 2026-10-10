//
// MimeParserReader.cs
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit {
	public partial class MimeParser
	{
		sealed class Reader : MimeReader
		{
			readonly MimeParser parser;

			public Reader (MimeParser parser, ParserOptions options, Stream stream, MimeFormat format) : base (options, stream, format)
			{
				this.parser = parser;
			}

			protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				parser.OnMboxMarkerBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMboxMarkerBeginAsync (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMboxMarkerBeginAsync (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				parser.OnMboxMarkerRead (buffer, startIndex, count, cancellationToken);
			}

			protected override Task OnMboxMarkerReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				return parser.OnMboxMarkerReadAsync (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				parser.OnMboxMarkerEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override Task OnMboxMarkerEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				return parser.OnMboxMarkerEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnHeadersBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnHeadersBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnHeadersBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnHeaderRead (header, beginLineNumber, cancellationToken);
			}

			protected override Task OnHeaderReadAsync (Header header, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnHeaderReadAsync (header, beginLineNumber, cancellationToken);
			}

			protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
			{
				parser.OnHeadersEnd (beginOffset, beginLineNumber, endOffset, endLineNumber, cancellationToken);
			}

			protected override Task OnHeadersEndAsync (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnHeadersEndAsync (beginOffset, beginLineNumber, endOffset, endLineNumber, cancellationToken);
			}

			protected override void OnBodySeparator (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				parser.OnBodySeparator (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override Task OnBodySeparatorAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				return parser.OnBodySeparatorAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMimeMessageBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMimeMessageBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMimeMessageBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMimeMessageEnd (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override Task OnMimeMessageEndAsync (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMimeMessageEndAsync (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMimePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMimePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMimePartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMimePartContentBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMimePartContentBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMimePartContentBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				parser.OnMimePartContentRead (buffer, startIndex, count, cancellationToken);
			}

			protected override Task OnMimePartContentReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				return parser.OnMimePartContentReadAsync (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
			{
				parser.OnMimePartContentEnd (beginOffset, beginLineNumber, endOffset, lines, newLineFormat, cancellationToken);
			}

			protected override Task OnMimePartContentEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
			{
				return parser.OnMimePartContentEndAsync (beginOffset, beginLineNumber, endOffset, lines, newLineFormat, cancellationToken);
			}

			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMimePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override Task OnMimePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMimePartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMessagePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMessagePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMessagePartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMessagePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override Task OnMessagePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMessagePartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMultipartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartBoundaryBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMultipartBoundaryBeginAsync (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartBoundaryBeginAsync (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartBoundaryRead (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMultipartBoundaryReadAsync (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartBoundaryReadAsync (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				parser.OnMultipartBoundaryEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override Task OnMultipartBoundaryEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				return parser.OnMultipartBoundaryEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartEndBoundaryBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMultipartEndBoundaryBeginAsync (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEndBoundaryBeginAsync (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartEndBoundaryRead (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMultipartEndBoundaryReadAsync (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEndBoundaryReadAsync (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				parser.OnMultipartEndBoundaryEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override Task OnMultipartEndBoundaryEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEndBoundaryEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartPreambleBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMultipartPreambleBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartPreambleBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				parser.OnMultipartPreambleRead (buffer, startIndex, count, cancellationToken);
			}

			protected override Task OnMultipartPreambleReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				return parser.OnMultipartPreambleReadAsync (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMultipartPreambleEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override Task OnMultipartPreambleEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMultipartPreambleEndAsync (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				parser.OnMultipartEpilogueBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override Task OnMultipartEpilogueBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEpilogueBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				parser.OnMultipartEpilogueRead (buffer, startIndex, count, cancellationToken);
			}

			protected override Task OnMultipartEpilogueReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEpilogueReadAsync (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMultipartEpilogueEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override Task OnMultipartEpilogueEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEpilogueEndAsync (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				parser.OnMultipartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override Task OnMultipartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				return parser.OnMultipartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}
		}
	}
}
