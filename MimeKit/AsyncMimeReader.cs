//
// AsyncMimeParser.cs
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
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using MimeKit.Utils;
using MimeKit.Encodings;

namespace MimeKit {
	public partial class MimeReader
	{
		async Task<int> ReadAheadAsync (int atleast, int save, CancellationToken cancellationToken)
		{
			if (!AlignReadAheadBuffer (atleast, save, out int left, out int start, out int end))
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
			bool complete;

			do {
				var available = await ReadAheadAsync (ReadAheadSize, 0, cancellationToken).ConfigureAwait (false);

				if (available <= 0) {
					// failed to read any data... EOF
					inputIndex = inputEnd;
					return false;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						complete = StepByteOrderMark (inbuf, ref bomIndex);
					}
				}
			} while (!complete && inputIndex == inputEnd);

			return complete;
		}

		async Task StepMboxMarkerAsync (CancellationToken cancellationToken)
		{
			bool midline = false;
			bool complete;

			// consume data until we find a line that begins with "From "
			do {
				var available = await ReadAheadAsync (5, 0, cancellationToken).ConfigureAwait (false);

				if (available < 5) {
					// failed to find the beginning of the mbox marker; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						complete = StepMboxMarkerStart (inbuf, ref midline);
					}
				}
			} while (!complete);

			var mboxMarkerOffset = GetOffset (inputIndex);
			var mboxMarkerLineNumber = lineNumber;

			OnMboxMarkerBegin (mboxMarkerOffset, lineNumber, cancellationToken);

			do {
				if (await ReadAheadAsync (ReadAheadSize, 0, cancellationToken).ConfigureAwait (false) < 1) {
					// failed to find the end of the mbox marker; EOF reached
					state = MimeParserState.Error;
					return;
				}

				int startIndex = inputIndex;
				int count;

				unsafe {
					fixed (byte* inbuf = input) {
						complete = StepMboxMarker (inbuf, out count);
					}
				}

				// TODO: Remove beginOffset and lineNumber arguments from OnMboxMarkerReadAsync() in v5.0
				await OnMboxMarkerReadAsync (input, startIndex, count, mboxMarkerOffset, mboxMarkerLineNumber, cancellationToken).ConfigureAwait (false);
			} while (!complete);

			OnMboxMarkerEnd (mboxMarkerOffset, mboxMarkerLineNumber, GetOffset (inputIndex), cancellationToken);

			state = MimeParserState.MessageHeaders;
		}

		async Task StepHeadersAsync (CancellationToken cancellationToken)
		{
			var options = DetectMimeComplianceViolations ? DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls : DetectionOptions.None;
			int headersBeginLineNumber = lineNumber;
			var eof = false;

			headerBlockBegin = GetOffset (inputIndex);
			boundary = BoundaryType.None;
			currentBoundary = null;
			headerCount = 0;

			currentContentLength = null;

			currentContentType = null;
			currentContentTypeOffset = -1;
			currentContentTypeLineNumber = -1;

			currentEncoding = null;
			currentEncodingOffset = -1;
			currentEncodingLineNumber = -1;

			await OnHeadersBeginAsync (headerBlockBegin, headersBeginLineNumber, cancellationToken).ConfigureAwait (false);

			await ReadAheadAsync (ReadAheadSize, 0, cancellationToken).ConfigureAwait (false);

			do {
				var beginOffset = GetOffset (inputIndex);
				var beginLineNumber = lineNumber;
				int left = inputEnd - inputIndex;
				int headerFieldLength;
				int fieldNameLength;
				bool invalid;

				headerIndex = 0;

				if (left < 2)
					left = await ReadAheadAsync (2, 0, cancellationToken).ConfigureAwait (false);

				if (left == 0) {
					// Note: The only way to get here is if this is the first-pass through this loop and we're at EOF, so headerCount should ALWAYS be 0.

					if (toplevel && headerCount == 0) {
						// EOF has been reached before any headers have been parsed for Parse[Headers,Entity,Message]Async.
						state = MimeParserState.Eos;
						return;
					}

					// Note: This can happen if a message is truncated immediately after a boundary marker (e.g. where subpart headers would begin).
					if (DetectMimeComplianceViolations)
						OnMimeComplianceViolation (MimeComplianceViolation.MissingBodySeparator, beginOffset, beginLineNumber);

					state = MimeParserState.Content;
					break;
				}

				// Check for an empty line denoting the end of the header block.
				if (IsEndOfHeaderBlock (left)) {
					await OnBodySeparatorAsync (beginOffset, beginLineNumber, GetOffset (inputIndex), CancellationToken.None).ConfigureAwait (false);
					state = MimeParserState.Content;
					break;
				}

				// Scan ahead a bit to see if this looks like an invalid header.
				do {
					unsafe {
						fixed (byte* inbuf = input) {
							if (TryDetectInvalidHeader (inbuf, out invalid, out fieldNameLength, out headerFieldLength))
								break;
						}
					}

					int atleast = (inputEnd - inputIndex) + 1;

					if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) < atleast) {
						// Not enough input to even find the ':'... mark as invalid and continue?
						invalid = true;
						break;
					}
				} while (true);

				if (invalid) {
					// Figure out why this is an invalid header.

					if (input[inputIndex] == (byte) '-') {
						// Check for a boundary marker. If the message is properly formatted, this will NEVER happen.
						do {
							unsafe {
								fixed (byte* inbuf = input) {
									if (TryCheckBoundaryWithinHeaderBlock (inbuf))
										break;
								}
							}

							int atleast = (inputEnd - inputIndex) + 1;

							if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) < atleast)
								break;
						} while (true);

						// Note: If a boundary was discovered, then the state will be updated to MimeParserState.Boundary.
						if (state == MimeParserState.Boundary) {
							if (DetectMimeComplianceViolations)
								OnMimeComplianceViolation (MimeComplianceViolation.MissingBodySeparator, beginOffset, beginLineNumber);
							break;
						}

						// Fall through and act as if we're consuming a header.
					} else if (input[inputIndex] == (byte) 'F' || input[inputIndex] == (byte) '>') {
						// Check for an mbox-style From-line. Again, if the message is properly formatted and not truncated, this will NEVER happen.
						do {
							unsafe {
								fixed (byte* inbuf = input) {
									if (TryCheckMboxMarkerWithinHeaderBlock (inbuf))
										break;
								}
							}

							int atleast = (inputEnd - inputIndex) + 1;

							if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) < atleast)
								break;
						} while (true);

						// state will be one of the following values:
						// 1. Complete: This means that we've found an actual mbox marker
						// 2. Error: Invalid *first* header and it was not a valid mbox marker
						// 3. MessageHeaders or Headers: let it fall through and treat it as an invalid headers
						if (state != MimeParserState.MessageHeaders && state != MimeParserState.Headers) {
							if (DetectMimeComplianceViolations)
								OnMimeComplianceViolation (MimeComplianceViolation.MissingBodySeparator, beginOffset, beginLineNumber);
							break;
						}

						// Fall through and act as if we're consuming a header.
					} else {
						// Fall through and act as if we're consuming a header.
					}

					if (DetectMimeComplianceViolations)
						OnMimeComplianceViolation (MimeComplianceViolation.InvalidHeader, beginOffset, beginLineNumber);

					if (toplevel && eos && inputIndex + headerFieldLength >= inputEnd) {
						state = MimeParserState.Error;
						return;
					}

					// Fall through and act as if we're consuming a header.
				} else {
					// Consume the header field name.
					StepHeaderField (headerFieldLength);
				}

				bool midline = true;

				// Consume the header value.
				do {
					unsafe {
						fixed (byte* inbuf = input) {
							if (StepHeaderValue (inbuf, ref options, ref midline))
								break;
						}
					}

					if (await ReadAheadAsync (1, 0, cancellationToken).ConfigureAwait (false) == 0) {
						if (DetectMimeComplianceViolations) {
							if (midline)
								OnMimeComplianceViolation (MimeComplianceViolation.IncompleteHeader, GetOffset (inputIndex), lineNumber);
							else
								OnMimeComplianceViolation (MimeComplianceViolation.MissingBodySeparator, GetOffset (inputIndex), lineNumber);
						}
						state = MimeParserState.Content;
						eof = true;
						break;
					}
				} while (true);

				if (toplevel && headerCount == 0 && invalid && !IsMboxMarker (headerBuffer)) {
					state = MimeParserState.Error;
					return;
				}

				var header = CreateHeader (beginOffset, beginLineNumber, fieldNameLength, headerFieldLength, invalid);

				await OnHeaderReadAsync (header, beginLineNumber, cancellationToken).ConfigureAwait (false);
			} while (!eof);

			headerBlockEnd = GetOffset (inputIndex);

			await OnHeadersEndAsync (headerBlockBegin, headersBeginLineNumber, headerBlockEnd, lineNumber, cancellationToken).ConfigureAwait (false);
		}

		async Task<bool> SkipBoundaryMarkerAsync (string? boundary, bool endBoundary, CancellationToken cancellationToken)
		{
			long beginOffset = GetOffset (inputIndex);
			int beginLineNumber = lineNumber;
			int startIndex = inputIndex;
			bool result;

			if (endBoundary)
				await OnMultipartEndBoundaryBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			else
				await OnMultipartBoundaryBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			unsafe {
				fixed (byte* inbuf = input) {
					result = SkipBoundaryMarkerInternal (inbuf, endBoundary);
				}
			}

			int count = inputIndex - startIndex;

			if (endBoundary)
				await OnMultipartEndBoundaryReadAsync (input, startIndex, count, beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			else
				await OnMultipartBoundaryReadAsync (input, startIndex, count, beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			long endOffset = GetOffset (inputIndex);

			if (endBoundary) {
				await OnMultipartEndBoundaryEndAsync (beginOffset, beginLineNumber, endOffset, cancellationToken).ConfigureAwait (false);

#pragma warning disable 618
				// Obsolete
				await OnMultipartEndBoundaryAsync (boundary, beginOffset, endOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
#pragma warning restore 618
			} else {
				await OnMultipartBoundaryEndAsync (beginOffset, beginLineNumber, endOffset, cancellationToken).ConfigureAwait (false);

#pragma warning disable 618
				// Obsolete
				await OnMultipartBoundaryAsync (boundary, beginOffset, endOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
#pragma warning restore 618
			}

			return result;
		}

		async Task<MimeParserState> StepAsync (CancellationToken cancellationToken)
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
				await StepHeadersAsync (cancellationToken).ConfigureAwait (false);
				toplevel = false;
				break;
			}

			return state;
		}

		async Task<ScanContentResult> ScanContentAsync (ScanContentType type, long beginOffset, int beginLineNumber, bool trimNewLine, DetectionOptions options, CancellationToken cancellationToken)
		{
			int maxBoundaryLength = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			IEncodingValidator? validator = null;
			var formats = new bool[2];
			int contentLength = 0;
			bool incomplete = false;
			bool midline = false;

			if (DetectMimeComplianceViolations && type == ScanContentType.MimeContent && currentEncoding.HasValue)
				validator = GetEncodingValidator (currentEncoding.Value, beginOffset, beginLineNumber);

			do {
				int atleast = incomplete ? Math.Max (maxBoundaryLength, (inputEnd - inputIndex) + 1) : maxBoundaryLength;

				if (await ReadAheadAsync (atleast, 2, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					break;
				}

				int contentIndex = inputIndex;

				unsafe {
					fixed (byte* inbuf = input) {
						incomplete = ScanContent (inbuf, ref options, ref midline, ref formats);
					}
				}

				if (contentIndex < inputIndex) {
					switch (type) {
					case ScanContentType.MultipartPreamble:
						await OnMultipartPreambleReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					case ScanContentType.MultipartEpilogue:
						await OnMultipartEpilogueReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					default:
						validator?.Write (input, contentIndex, inputIndex - contentIndex);
						await OnMimePartContentReadAsync (input, contentIndex, inputIndex - contentIndex, cancellationToken).ConfigureAwait (false);
						break;
					}

					contentLength += inputIndex - contentIndex;
				}
			} while (boundary == BoundaryType.None);

			validator?.Flush ();

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
			var options = DetectMimeComplianceViolations ? GetContentDetectionOptions (currentEncoding) : DetectionOptions.None;
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMimePartContentBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MimeContent, beginOffset, beginLineNumber, true, options, cancellationToken).ConfigureAwait (false);
			await OnMimePartContentEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, result.Format, cancellationToken).ConfigureAwait (false);

			return result.Lines;
		}

		async Task<int> ConstructMessagePartAsync (int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (boundaries != null) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					return 0;
				}

				// Check to see if this first line is a boundary marker.
				unsafe {
					fixed (byte* inbuf = input) {
						var options = DetectMimeComplianceViolations ? GetContentDetectionOptions (currentEncoding) : DetectionOptions.None;
						byte* start = inbuf + inputIndex;
						byte* inend = inbuf + inputEnd;
						byte* inptr;

						*inend = (byte) '\n';

						if (options != DetectionOptions.None) {
							inptr = ParseUtils.EndOfLine (start, inend + 1, options, out var detected);

							if ((detected & DetectionResults.Detected8Bit) != 0)
								OnMimeComplianceViolation (MimeComplianceViolation.Unexpected8BitBytesInBody, beginOffset, beginLineNumber);

							if ((detected & DetectionResults.DetectedNulls) != 0)
								OnMimeComplianceViolation (MimeComplianceViolation.UnexpectedNullBytesInBody, beginOffset, beginLineNumber);
						} else {
							inptr = ParseUtils.EndOfLine (start, inend + 1);
						}

						if (DetectMimeComplianceViolations && (inptr == start || inptr[-1] != (byte) '\r'))
							OnMimeComplianceViolation (MimeComplianceViolation.BareLinefeedInBody, beginOffset + (int) (inptr - start), beginLineNumber);

						// Note: This isn't obvious, but if the "boundary" that was found is an Mbox "From " line, then
						// either the current stream offset is >= contentEnd -or- RespectContentLength is false. It will
						// *never* be an Mbox "From " marker in Entity mode.
						if ((boundary = CheckBoundary (inputIndex, start, (int) (inptr - start))) != BoundaryType.None)
							return GetLineCount (beginLineNumber, beginOffset, GetEndOffset (inputIndex));
					}
				}
			}

			// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
			state = MimeParserState.MessageHeaders;
			await StepAsync (cancellationToken).ConfigureAwait (false);

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			await OnMimeMessageBeginAsync (currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			entityType = GetEntityType (type, currentEncoding, depth);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (type, depth + 1, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (depth + 1, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				break;
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

		async Task MultipartScanPreambleAsync (DetectionOptions options, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMultipartPreambleBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MultipartPreamble, beginOffset, beginLineNumber, false, options, cancellationToken).ConfigureAwait (false);
			await OnMultipartPreambleEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken).ConfigureAwait (false);
		}

		async Task MultipartScanEpilogueAsync (DetectionOptions options, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			await OnMultipartEpilogueBeginAsync (beginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
			var result = await ScanContentAsync (ScanContentType.MultipartEpilogue, beginOffset, beginLineNumber, true, options, cancellationToken).ConfigureAwait (false);
			await OnMultipartEpilogueEndAsync (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken).ConfigureAwait (false);
		}

		async Task MultipartScanSubpartsAsync (ContentType multipartContentType, int depth, CancellationToken cancellationToken)
		{
			do {
				// skip over the boundary marker
				if (!await SkipBoundaryMarkerAsync (multipartContentType.Boundary, endBoundary: false, cancellationToken).ConfigureAwait (false)) {
					boundary = BoundaryType.Eos;
					return;
				}

				var beginLineNumber = lineNumber;

				// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
				state = MimeParserState.Headers;
				await StepAsync (cancellationToken).ConfigureAwait (false);

				var type = GetContentType (multipartContentType);
				var currentHeadersEndOffset = headerBlockEnd;
				var currentBeginOffset = headerBlockBegin;
				MimeEntityType entityType;
				int lines;

				entityType = GetEntityType (type, currentEncoding, depth);

				switch (entityType) {
				case MimeEntityType.Multipart:
					await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMultipartAsync (type, depth + 1, cancellationToken).ConfigureAwait (false);
					break;
				case MimeEntityType.MessagePart:
					await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMessagePartAsync (depth + 1, cancellationToken).ConfigureAwait (false);
					break;
				default:
					await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
					lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
					break;
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
			} while (boundary == BoundaryType.ImmediateBoundary);
		}

		async Task<int> ConstructMultipartAsync (ContentType contentType, int depth, CancellationToken cancellationToken)
		{
			var options = DetectMimeComplianceViolations ? GetContentDetectionOptions (currentEncoding) : DetectionOptions.None;
			var beginOffset = GetOffset (inputIndex);
			var marker = contentType.Boundary;
			var beginLineNumber = lineNumber;
			long endOffset;

			if (marker is null) {
				if (DetectMimeComplianceViolations)
					OnMimeComplianceViolation (MimeComplianceViolation.MissingMultipartBoundaryParameter, currentContentTypeOffset, currentContentTypeLineNumber);

				// Note: this will scan all content into the preamble...
				await MultipartScanPreambleAsync (options, cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// Note: MimeReader can handle invalid boundary markers (but those with '\n' will fail to match, resulting in all content being treated as preamble).
			if (DetectMimeComplianceViolations && !IsValidBoundary (marker))
				OnMimeComplianceViolation (MimeComplianceViolation.InvalidMultipartBoundaryParameter, currentContentTypeOffset, currentContentTypeLineNumber);

			PushBoundary (marker);

			await MultipartScanPreambleAsync (options, cancellationToken).ConfigureAwait (false);
			if (boundary == BoundaryType.ImmediateBoundary)
				await MultipartScanSubpartsAsync (contentType, depth, cancellationToken).ConfigureAwait (false);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				await SkipBoundaryMarkerAsync (marker, endBoundary: true, cancellationToken).ConfigureAwait (false);

				PopBoundary ();

				await MultipartScanEpilogueAsync (options, cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// We either found the end of the stream or we found a parent's boundary
			if (DetectMimeComplianceViolations)
				OnMimeComplianceViolation (MimeComplianceViolation.MissingMultipartBoundary, GetOffset (inputIndex), lineNumber);

			PopBoundary ();

			endOffset = GetEndOffset (inputIndex);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		/// <summary>
		/// Asynchronously read a block of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously reads a block of headers from the stream.
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
		public async Task ReadHeadersAsync (CancellationToken cancellationToken = default)
		{
			state = MimeParserState.Headers;
			toplevel = true;

			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos && inputIndex == inputEnd ? MimeParserState.Eos : MimeParserState.Complete;
		}

		/// <summary>
		/// Asynchronously read an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously reads an entity from the stream.
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
		public async Task ReadEntityAsync (CancellationToken cancellationToken = default)
		{
			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			// parse the headers
			switch (await StepAsync (cancellationToken).ConfigureAwait (false)) {
			case MimeParserState.Error:
				throw new FormatException ("Failed to parse entity headers.");
			case MimeParserState.Eos:
				throw new FormatException ("End of stream.");
			}

			var type = GetContentType (null);
			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;
			MimeEntityType entityType;
			int lines;

			entityType = GetEntityType (type, currentEncoding, 0);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (type, 0, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (0, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				break;
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

			state = MimeParserState.Eos;
		}

		/// <summary>
		/// Asynchronously read a message from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously reads a message from the stream.
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
		public async Task ReadMessageAsync (CancellationToken cancellationToken = default)
		{
			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (await StepAsync (cancellationToken).ConfigureAwait (false)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			var beginLineNumber = lineNumber;
			toplevel = true;

			// parse the headers
			switch (await StepAsync (cancellationToken).ConfigureAwait (false)) {
			case MimeParserState.Error:
				throw new FormatException ("Failed to parse message headers.");
			case MimeParserState.Eos:
				throw new FormatException ("End of stream.");
			}

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			await OnMimeMessageBeginAsync (currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);

			if (format == MimeFormat.Mbox && options.RespectContentLength && currentContentLength.HasValue)
				contentEnd = GetOffset (inputIndex) + currentContentLength.Value;
			else
				contentEnd = 0;

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			entityType = GetEntityType (type, currentEncoding, 0);

			switch (entityType) {
			case MimeEntityType.Multipart:
				await OnMultipartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMultipartAsync (type, 0, cancellationToken).ConfigureAwait (false);
				break;
			case MimeEntityType.MessagePart:
				await OnMessagePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMessagePartAsync (0, cancellationToken).ConfigureAwait (false);
				break;
			default:
				await OnMimePartBeginAsync (type, currentBeginOffset, beginLineNumber, cancellationToken).ConfigureAwait (false);
				lines = await ConstructMimePartAsync (cancellationToken).ConfigureAwait (false);
				break;
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
	}
}
