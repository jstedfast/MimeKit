//
// AsyncMimeParser.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2021 .NET Foundation and Contributors
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
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MimeKit {
	public partial class MimeReader
	{
		async Task<int> ReadAheadAsync (int atleast, int save, CancellationToken cancellationToken)
		{
			int left, start, end;

			if (!AlignReadAheadBuffer (atleast, save, out left, out start, out end))
				return left;

			int nread = await stream.ReadAsync (input, start, end - start, cancellationToken).ConfigureAwait (false);

			if (nread > 0) {
				inputEnd += nread;
				position += nread;
			} else {
				eos = true;
			}

			return inputEnd - inputIndex;
		}

		async Task<bool> StepByteOrderMarkAsync (CancellationToken cancellationToken)
		{
			int bomIndex = 0;

			do {
				var available = await ReadAheadAsync (ReadAheadSize, 0, cancellationToken).ConfigureAwait (false);

				if (available <= 0) {
					// failed to read any data... EOF
					inputIndex = inputEnd;
					return false;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						StepByteOrderMark (inbuf, ref bomIndex);
					}
				}
			} while (inputIndex == inputEnd);

			return bomIndex == 0 || bomIndex == UTF8ByteOrderMark.Length;
		}

		async Task StepMboxMarkerAsync (CancellationToken cancellationToken)
		{
			bool complete;
			int left = 0;

			mboxMarkerLength = 0;

			do {
				var available = await ReadAheadAsync (Math.Max (ReadAheadSize, left), 0, cancellationToken).ConfigureAwait (false);

				if (available <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						complete = StepMboxMarker (inbuf, ref left);
					}
				}
			} while (!complete);

			await OnMboxMarkerReadAsync (mboxMarkerBuffer, 0, mboxMarkerLength, mboxMarkerOffset, lineNumber - 1, cancellationToken).ConfigureAwait (false);

			state = MimeParserState.MessageHeaders;
		}

		async Task StepHeadersAsync (ParserOptions options, CancellationToken cancellationToken)
		{
			bool scanningFieldName = true;
			bool checkFolded = false;
			bool midline = false;
			bool blank = false;
			bool valid = true;
			int left = 0;

			headerBlockBegin = GetOffset (inputIndex);
			boundary = BoundaryType.None;
			ResetRawHeaderData ();
			headerCount = 0;

			currentContentLength = null;
			currentContentType = null;
			currentEncoding = null;

			await ReadAheadAsync (Math.Max (ReadAheadSize, left), 0, cancellationToken).ConfigureAwait (false);

			do {
				unsafe {
					fixed (byte* inbuf = input) {
						if (!StepHeaders (options, inbuf, ref scanningFieldName, ref checkFolded, ref midline, ref blank, ref valid, ref left, cancellationToken))
							break;
					}
				}

				var available = await ReadAheadAsync (left + 1, 0, cancellationToken).ConfigureAwait (false);

				if (available == left) {
					// EOF reached before we reached the end of the headers...
					if (scanningFieldName && left > 0) {
						// EOF reached right in the middle of a header field name. Throw an error.
						//
						// See private email from Feb 8, 2018 which contained a sample message w/o
						// any breaks between the header and message body. The file also did not
						// end with a newline sequence.
						state = MimeParserState.Error;
					} else {
						// EOF reached somewhere in the middle of the value.
						//
						// Append whatever data we've got left and pretend we found the end
						// of the header value (and the header block).
						//
						// For more details, see https://github.com/jstedfast/MimeKit/pull/51
						// and https://github.com/jstedfast/MimeKit/issues/348
						if (left > 0) {
							AppendRawHeaderData (inputIndex, left);
							inputIndex = inputEnd;
						}

						ParseAndAppendHeader (options, cancellationToken);

						state = MimeParserState.Content;
					}
					break;
				}
			} while (true);

			headerBlockEnd = GetOffset (inputIndex);
		}

		async Task<bool> SkipLineAsync (bool consumeNewLine, CancellationToken cancellationToken)
		{
			do {
				unsafe {
					fixed (byte* inbuf = input) {
						if (SkipLine (inbuf, consumeNewLine))
							return true;
					}
				}

				if (await ReadAheadAsync (ReadAheadSize, 1, cancellationToken).ConfigureAwait (false) <= 0)
					return false;
			} while (true);
		}

		async Task<MimeParserState> StepAsync (ParserOptions options, CancellationToken cancellationToken)
		{
			switch (state) {
			case MimeParserState.Initialized:
				if (!await StepByteOrderMarkAsync (cancellationToken).ConfigureAwait (false)) {
					state = MimeParserState.Eos;
					break;
				}

				state = format == MimeFormat.Mbox ? MimeParserState.MboxMarker : MimeParserState.MessageHeaders;
				break;
			case MimeParserState.MboxMarker:
				await StepMboxMarkerAsync (cancellationToken).ConfigureAwait (false);
				break;
			case MimeParserState.MessageHeaders:
			case MimeParserState.Headers:
				await StepHeadersAsync (options, cancellationToken).ConfigureAwait (false);
				toplevel = false;
				break;
			}

			return state;
		}

		async Task<ScanContentResult> ScanContentAsync (ScanContentType type, long beginOffset, int beginLineNumber, bool trimNewLine, CancellationToken cancellationToken)
		{
			int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			int contentIndex = inputIndex;
			var formats = new bool[2];
			int contentLength = 0;
			bool midline = false;
			int nleft;

			do {
				if (contentIndex < inputIndex) {
					switch (type) {
					case ScanContentType.MultipartPreamble:
						await OnMultipartPreambleReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					case ScanContentType.MultipartEpilogue:
						await OnMultipartEpilogueReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					default:
						await OnMimePartContentReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					}

					contentLength += inputIndex - contentIndex;
				}

				nleft = inputEnd - inputIndex;
				if (await ReadAheadAsync (atleast, 2, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					contentIndex = inputIndex;
					break;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						ScanContent (inbuf, ref contentIndex, ref nleft, ref midline, ref formats);
					}
				}
			} while (boundary == BoundaryType.None);

			if (contentIndex < inputIndex) {
				switch (type) {
				case ScanContentType.MultipartPreamble:
					await OnMultipartPreambleReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
					break;
				case ScanContentType.MultipartEpilogue:
					await OnMultipartEpilogueReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
					break;
				default:
					await OnMimePartContentReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
					break;
				}

				contentLength += inputIndex - contentIndex;
			}

			// FIXME: need to redesign the above loop so that we don't consume the last <CR><LF> that belongs to the boundary marker.
			var isEmpty = contentLength == 0;

			if (boundary != BoundaryType.Eos && trimNewLine) {
				// the last \r\n belongs to the boundary
				if (contentLength > 0) {
					if (input[inputIndex - 2] == (byte) '\r')
						contentLength -= 2;
					else
						contentLength--;
				}
			}

			var endOffset = beginOffset + contentLength;
			var lines = GetLineCount (beginLineNumber, beginOffset, endOffset);

			return new ScanContentResult (contentLength, lines, formats, isEmpty);
		}

		async Task<int> ConstructMimePartAsync (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMimePartContentBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MimeContent, beginOffset, beginLineNumber, true, cancellationToken).ConfigureAwait (false);
			await OnMimePartContentEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken).ConfigureAwait (false);

			return result.Lines;
		}

		async Task<int> ConstructMessagePartAsync (ParserOptions options, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					return 0;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						byte* start = inbuf + inputIndex;
						byte* inend = inbuf + inputEnd;
						byte* inptr = start;

						*inend = (byte) '\n';

						while (*inptr != (byte) '\n')
							inptr++;

						boundary = CheckBoundary (inputIndex, start, (int) (inptr - start));

						switch (boundary) {
						case BoundaryType.ImmediateEndBoundary:
						case BoundaryType.ImmediateBoundary:
						case BoundaryType.ParentBoundary:
							return 0;
						case BoundaryType.ParentEndBoundary:
							// ignore "From " boundaries, broken mailers tend to include these...
							if (!IsMboxMarker (start)) {
								return 0;
							}
							break;
						}
					}
				}
			}

			// parse the headers...
			state = MimeParserState.MessageHeaders;
			if (await StepAsync (options, cancellationToken).ConfigureAwait (false) == MimeParserState.Error) {
				// Note: this either means that StepHeaders() found the end of the stream
				// or an invalid header field name at the start of the message headers,
				// which likely means that this is not a valid MIME stream?
				boundary = BoundaryType.Eos;
				return GetLineCount (beginLineNumber, beginOffset, GetEndOffset (inputIndex));
			}

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			await OnMimeMessageBeginAsync (currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			if (preHeaderBuffer.Length > 0) {
				// FIXME: how to solve this?
				//message.MboxMarker = new byte[preHeaderLength];
				//Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			}

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (depth < options.MaxMimeDepth && IsMultipart (type)) {
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (options, type, depth + 1, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.Multipart;
			} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (options, depth + 1, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MessagePart;
			} else {
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			}

			await OnMimeMessageEndAsync (currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		async Task MultipartScanPreambleAsync (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMultipartPreambleBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MultipartPreamble, beginOffset, beginLineNumber, false, cancellationToken).ConfigureAwait (false);
			await OnMultipartPreambleEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken).ConfigureAwait (false);
		}

		async Task MultipartScanEpilogueAsync (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMultipartEpilogueBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MultipartEpilogue, beginOffset, beginLineNumber, true, cancellationToken).ConfigureAwait (false);
			await OnMultipartEpilogueEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken).ConfigureAwait (false);
		}

		async Task MultipartScanSubpartsAsync (ParserOptions options, ContentType multipartContentType, int depth, CancellationToken cancellationToken)
		{
			var boundaryOffset = GetOffset (inputIndex);

			do {
				// skip over the boundary marker
				if (!await SkipLineAsync (true, cancellationToken).ConfigureAwait (false)) {
					await OnMultipartBoundaryAsync (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber, cancellationToken).ConfigureAwait (false);
					boundary = BoundaryType.Eos;
					return;
				}

				await OnMultipartBoundaryAsync (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber - 1, cancellationToken).ConfigureAwait (false);

				var beginLineNumber = lineNumber;

				// parse the headers
				state = MimeParserState.Headers;
				if (await StepAsync (options, cancellationToken).ConfigureAwait (false) == MimeParserState.Error) {
					boundary = BoundaryType.Eos;
					return;
				}

				if (state == MimeParserState.Boundary) {
					if (headerCount == 0) {
						if (boundary == BoundaryType.ImmediateBoundary) {
							//beginOffset = GetOffset (inputIndex);
							continue;
						}
						return;
					}

					// This part has no content, but that will be handled in ConstructMultipartAsync()
					// or ConstructMimePartAsync().
				}

				//if (state == ParserState.Complete && headers.Count == 0)
				//	return BoundaryType.EndBoundary;

				var type = GetContentType (multipartContentType);
				var currentHeadersEndOffset = headerBlockEnd;
				var currentBeginOffset = headerBlockBegin;
				MimeEntityType entityType;
				int lines;

				if (depth < options.MaxMimeDepth && IsMultipart (type)) {
					await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMultipartAsync (options, type, depth + 1, cancellationToken).ConfigureAwait (false);
					entityType = MimeEntityType.Multipart;
				} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
					await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMessagePartAsync (options, depth + 1, cancellationToken).ConfigureAwait (false);
					entityType = MimeEntityType.MessagePart;
				} else {
					await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
					entityType = MimeEntityType.MimePart;
				}

				var endOffset = GetEndOffset (inputIndex);
				currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

				switch (entityType) {
				case MimeEntityType.Multipart:
					await OnMultipartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
					break;
				case MimeEntityType.MessagePart:
					await OnMessagePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
					break;
				default:
					await OnMimePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
					break;
				}

				boundaryOffset = endOffset;
			} while (boundary == BoundaryType.ImmediateBoundary);
		}

		async Task<int> ConstructMultipartAsync (ParserOptions options, ContentType contentType, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var marker = contentType.Boundary;
			var beginLineNumber = lineNumber;
			long endOffset;

			if (marker == null) {
#if DEBUG
				Debug.WriteLine ("Multipart without a boundary encountered!");
#endif

				// Note: this will scan all content into the preamble...
				await MultipartScanPreambleAsync (cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			PushBoundary (marker);

			await MultipartScanPreambleAsync (cancellationToken).ConfigureAwait (false);
			if (boundary == BoundaryType.ImmediateBoundary)
				await MultipartScanSubpartsAsync (options, contentType, depth, cancellationToken).ConfigureAwait (false);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				// FIXME: multipart.WriteEndBoundary = true;

				var boundaryOffset = GetOffset (inputIndex);
				var boundaryLineNumber = lineNumber;

				await SkipLineAsync (false, cancellationToken).ConfigureAwait (false);

				await OnMultipartEndBoundaryAsync (marker, boundaryOffset, GetOffset (inputIndex), boundaryLineNumber, cancellationToken).ConfigureAwait (false);

				PopBoundary ();

				await MultipartScanEpilogueAsync (cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// FIXME: multipart.WriteEndBoundary = false;

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			unsafe {
				fixed (byte* inbuf = input) {
					if (boundary == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (inbuf, true))
						boundary = BoundaryType.ImmediateEndBoundary;
					else if (boundary == BoundaryType.ParentBoundary && FoundImmediateBoundary (inbuf, false))
						boundary = BoundaryType.ImmediateBoundary;
				}
			}

			endOffset = GetEndOffset (inputIndex);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		/// <summary>
		/// Asynchronously parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task ReadEntityAsync (ParserOptions options, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			if (await StepAsync (options, cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse entity headers.");

			var type = GetContentType (null);
			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (options, type, 0, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (options, 0, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MessagePart;
			} else {
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			}

			if (boundary != BoundaryType.Eos)
				state = MimeParserState.Complete;
			else
				state = MimeParserState.Eos;
		}

		/// <summary>
		/// Asynchronously parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task ReadEntityAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			return ReadEntityAsync (ParserOptions.Default, cancellationToken);
		}

		/// <summary>
		/// Asynchronously parses a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the message.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task ReadMessageAsync (ParserOptions options, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (await StepAsync (options, cancellationToken).ConfigureAwait (false)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			toplevel = true;

			// parse the headers
			var beginLineNumber = lineNumber;
			if (state < MimeParserState.Content && await StepAsync (options, cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse message headers.");

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			await OnMimeMessageBeginAsync (currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			if (format == MimeFormat.Mbox && options.RespectContentLength && currentContentLength.HasValue && currentContentLength.Value != -1)
				contentEnd = GetOffset (inputIndex) + currentContentLength.Value;
			else
				contentEnd = 0;

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (options, type, 0, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (options, 0, cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MessagePart;
			} else {
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartEndAsync (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);
				break;
			}

			await OnMimeMessageEndAsync (currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken).ConfigureAwait (false);

			if (boundary != BoundaryType.Eos) {
				if (format == MimeFormat.Mbox)
					state = MimeParserState.MboxMarker;
				else
					state = MimeParserState.Complete;
			} else {
				state = MimeParserState.Eos;
			}
		}

		/// <summary>
		/// Asynchronously parses a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the message.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task ReadMessageAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			return ReadMessageAsync (ParserOptions.Default, cancellationToken);
		}
	}
}
