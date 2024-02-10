//
// AsyncMimeParser.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	public partial class MimeParser
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

			state = MimeParserState.MessageHeaders;
		}

		async Task StepHeadersAsync (CancellationToken cancellationToken)
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
			headers.Clear ();

			await ReadAheadAsync (Math.Max (ReadAheadSize, left), 0, cancellationToken).ConfigureAwait (false);

			do {
				unsafe {
					fixed (byte *inbuf = input) {
						if (!StepHeaders (inbuf, ref scanningFieldName, ref checkFolded, ref midline, ref blank, ref valid, ref left))
							break;
					}
				}

				var available = await ReadAheadAsync (left + 1, 0, cancellationToken).ConfigureAwait (false);

				if (available == left) {
					// EOF reached before we reached the end of the headers...
					if (toplevel && scanningFieldName && left > 0) {
						// EOF reached right in the middle of a header field name. Throw an error.
						//
						// See private email from Feb 8, 2018 which contained a sample message w/o
						// any breaks between the header and message body. The file also did not
						// end with a newline sequence.
						state = MimeParserState.Error;
					} else {
						// EOF reached somewhere in the middle of the header.
						//
						// Append whatever data we've got left and pretend we found the end
						// of the header (and the header block).
						//
						// For more details, see https://github.com/jstedfast/MimeKit/pull/51
						// and https://github.com/jstedfast/MimeKit/issues/348
						if (left > 0) {
							AppendRawHeaderData (inputIndex, left);
							inputIndex = inputEnd;
						}

						ParseAndAppendHeader ();

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
						if (InnerSkipLine (inbuf, consumeNewLine))
							return true;
					}
				}

				if (await ReadAheadAsync (ReadAheadSize, 1, cancellationToken).ConfigureAwait (false) <= 0)
					return false;
			} while (true);
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

		async Task<ScanContentResult> ScanContentAsync (Stream content, bool trimNewLine, CancellationToken cancellationToken)
		{
			int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			var formats = new bool[2];
			bool midline = false;
			int nleft;

			do {
				nleft = inputEnd - inputIndex;
				if (await ReadAheadAsync (atleast, 2, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					break;
				}

				int contentIndex = inputIndex;

				unsafe {
					fixed (byte* inbuf = input) {
						ScanContent (inbuf, ref nleft, ref midline, ref formats);
					}
				}

				if (contentIndex < inputIndex)
					content.Write (input, contentIndex, inputIndex - contentIndex);
			} while (boundary == BoundaryType.None);

			var isEmpty = content.Length == 0;

			if (boundary != BoundaryType.Eos && trimNewLine) {
				// the last \r\n belongs to the boundary
				if (content.Length > 0) {
					if (input[inputIndex - 2] == (byte) '\r')
						content.SetLength (content.Length - 2);
					else
						content.SetLength (content.Length - 1);
				}
			}

			return new ScanContentResult (formats, isEmpty);
		}

		async Task ConstructMimePartAsync (MimePart part, MimeEntityEndEventArgs args, CancellationToken cancellationToken)
		{
			long endOffset, beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;
			ScanContentResult result;
			Stream content;

			if (persistent) {
				using (var measured = new MeasuringStream ()) {
					result = await ScanContentAsync (measured, true, cancellationToken).ConfigureAwait (false);
					endOffset = beginOffset + measured.Length;
				}

				content = new BoundStream (stream, beginOffset, endOffset, true);
			} else {
				content = new MemoryBlockStream ();

				try {
					result = await ScanContentAsync (content, true, cancellationToken).ConfigureAwait (false);
					content.Seek (0, SeekOrigin.Begin);
				} catch {
					content.Dispose ();
					throw;
				}

				endOffset = beginOffset + content.Length;
			}

			args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);

			if (!result.IsEmpty)
				part.Content = new MimeContent (content, part.ContentTransferEncoding) { NewLineFormat = result.Format };
			else
				content.Dispose ();
		}

		async Task ConstructMessagePartAsync (MessagePart rfc822, MimeEntityEndEventArgs args, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) <= 0) {
					boundary = BoundaryType.Eos;
					return;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						byte* start = inbuf + inputIndex;
						byte* inend = inbuf + inputEnd;
						byte* inptr = start;

						*inend = (byte) '\n';

						while (*inptr != (byte) '\n')
							inptr++;

						// Note: This isn't obvious, but if the "boundary" that was found is an Mbox "From " line, then
						// either the current stream offset is >= contentEnd -or- RespectContentLength is false. It will
						// *never* be an Mbox "From " marker in Entity mode.
						if ((boundary = CheckBoundary (inputIndex, start, (int) (inptr - start))) != BoundaryType.None)
							return;
					}
				}
			}

			// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
			state = MimeParserState.MessageHeaders;
			await StepAsync (cancellationToken).ConfigureAwait (false);

			var message = new MimeMessage (options, headers, RfcComplianceMode.Loose);
			var messageArgs = new MimeMessageEndEventArgs (message, rfc822) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeMessageBegin (messageArgs);

			if (preHeaderLength > 0) {
				message.MboxMarker = new byte[preHeaderLength];
				Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			}

			var type = GetContentType (null);
			var entity = options.CreateEntity (type, headers, true, depth);
			var entityArgs = new MimeEntityEndEventArgs (entity) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeEntityBegin (entityArgs);

			message.Body = entity;

			if (entity is Multipart multipart)
				await ConstructMultipartAsync (multipart, entityArgs, depth + 1, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart child)
				await ConstructMessagePartAsync (child, entityArgs, depth + 1, cancellationToken).ConfigureAwait (false);
			else
				await ConstructMimePartAsync ((MimePart) entity, entityArgs, cancellationToken).ConfigureAwait (false);

			rfc822.Message = message;

			var endOffset = GetEndOffset (inputIndex);
			messageArgs.HeadersEndOffset = entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
			messageArgs.EndOffset = entityArgs.EndOffset = endOffset;

			OnMimeEntityEnd (entityArgs);
			OnMimeMessageEnd (messageArgs);

			args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		async Task MultipartScanPreambleAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				long offset = GetOffset (inputIndex);

				//OnMultipartPreambleBegin (multipart, offset);
				await ScanContentAsync (memory, false, cancellationToken).ConfigureAwait (false);
				multipart.RawPreamble = memory.ToArray ();
				//OnMultipartPreambleEnd (multipart, offset + memory.Length);
			}
		}

		async Task MultipartScanEpilogueAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				long offset = GetOffset (inputIndex);

				//OnMultipartEpilogueBegin (multipart, offset);
				var result = await ScanContentAsync (memory, true, cancellationToken).ConfigureAwait (false);
				multipart.RawEpilogue = result.IsEmpty ? null : memory.ToArray ();
				//OnMultipartEpilogueEnd (multipart, offset + memory.Length);
			}
		}

		async Task MultipartScanSubpartsAsync (Multipart multipart, int depth, CancellationToken cancellationToken)
		{
			//var beginOffset = GetOffset (inputIndex);

			do {
				//OnMultipartBoundaryBegin (multipart, beginOffset);

				// skip over the boundary marker
				if (!await SkipLineAsync (true, cancellationToken).ConfigureAwait (false)) {
					//OnMultipartBoundaryEnd (multipart, GetOffset (inputIndex));
					boundary = BoundaryType.Eos;
					return;
				}

				//OnMultipartBoundaryEnd (multipart, GetOffset (inputIndex));

				var beginLineNumber = lineNumber;

				// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
				state = MimeParserState.Headers;
				await StepAsync (cancellationToken).ConfigureAwait (false);

				if (state == MimeParserState.Boundary) {
					if (headers.Count == 0) {
						if (boundary == BoundaryType.ImmediateBoundary) {
							// FIXME: Should we add an empty TextPart? If we do, update MimeParserTests.TestDoubleMultipartBoundary()
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

				var type = GetContentType (multipart.ContentType);
				var entity = options.CreateEntity (type, headers, false, depth);
				var entityArgs = new MimeEntityEndEventArgs (entity, multipart) {
					HeadersEndOffset = headerBlockEnd,
					BeginOffset = headerBlockBegin,
					LineNumber = beginLineNumber
				};

				OnMimeEntityBegin (entityArgs);

				if (entity is Multipart child)
					await ConstructMultipartAsync (child, entityArgs, depth + 1, cancellationToken).ConfigureAwait (false);
				else if (entity is MessagePart rfc822)
					await ConstructMessagePartAsync (rfc822, entityArgs, depth + 1, cancellationToken).ConfigureAwait (false);
				else
					await ConstructMimePartAsync ((MimePart) entity, entityArgs, cancellationToken).ConfigureAwait (false);

				var endOffset = GetEndOffset (inputIndex);
				entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
				entityArgs.EndOffset = endOffset;

				OnMimeEntityEnd (entityArgs);

				//beginOffset = endOffset;
				multipart.Add (entity);
			} while (boundary == BoundaryType.ImmediateBoundary);
		}

		async Task ConstructMultipartAsync (Multipart multipart, MimeEntityEndEventArgs args, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;
			var marker = multipart.Boundary;
			long endOffset;

			if (marker is null) {
#if DEBUG
				Debug.WriteLine ("Multipart without a boundary encountered!");
#endif

				// Note: this will scan all content into the preamble...
				await MultipartScanPreambleAsync (multipart, cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);
				args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
				return;
			}

			PushBoundary (marker);

			await MultipartScanPreambleAsync (multipart, cancellationToken).ConfigureAwait (false);
			if (boundary == BoundaryType.ImmediateBoundary)
				await MultipartScanSubpartsAsync (multipart, depth, cancellationToken).ConfigureAwait (false);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				//OnMultipartEndBoundaryBegin (multipart, GetEndOffset (inputIndex));

				// consume the end boundary and read the epilogue (if there is one)
				multipart.WriteEndBoundary = true;
				await SkipLineAsync (false, cancellationToken).ConfigureAwait (false);
				PopBoundary ();

				//OnMultipartEndBoundaryEnd (multipart, GetOffset (inputIndex));

				await MultipartScanEpilogueAsync (multipart, cancellationToken).ConfigureAwait (false);

				endOffset = GetEndOffset (inputIndex);
				args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
				return;
			}

			endOffset = GetEndOffset (inputIndex);
			args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);

			multipart.WriteEndBoundary = false;

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
		}

		/// <summary>
		/// Asynchronously parse a list of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses a list of headers from the stream.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task<HeaderList> ParseHeadersAsync (CancellationToken cancellationToken = default)
		{
			state = MimeParserState.Headers;
			toplevel = true;

			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;

			var parsed = new HeaderList (options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
		}

		/// <summary>
		/// Asynchronously parse an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses an entity from the stream.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
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
		public async Task<MimeEntity> ParseEntityAsync (CancellationToken cancellationToken = default)
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != position)
				stream.Seek (position, SeekOrigin.Begin);

			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse entity headers.");

			var type = GetContentType (null);

			// Note: we pass 'false' as the 'toplevel' argument here because
			// we want the entity to consume all of the headers.
			var entity = options.CreateEntity (type, headers, false, 0);
			var entityArgs = new MimeEntityEndEventArgs (entity) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeEntityBegin (entityArgs);

			if (entity is Multipart multipart)
				await ConstructMultipartAsync (multipart, entityArgs, 0, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart rfc822)
				await ConstructMessagePartAsync (rfc822, entityArgs, 0, cancellationToken).ConfigureAwait (false);
			else
				await ConstructMimePartAsync ((MimePart) entity, entityArgs, cancellationToken).ConfigureAwait (false);

			var endOffset = GetEndOffset (inputIndex);
			entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
			entityArgs.EndOffset = endOffset;

			state = MimeParserState.Eos;

			OnMimeEntityEnd (entityArgs);

			return entity;
		}

		/// <summary>
		/// Asynchronously parse a message from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses a message from the stream.
		/// </remarks>
		/// <returns>The parsed message.</returns>
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
		public async Task<MimeMessage> ParseMessageAsync (CancellationToken cancellationToken = default)
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != position)
				stream.Seek (position, SeekOrigin.Begin);

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (await StepAsync (cancellationToken).ConfigureAwait (false)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			toplevel = true;

			// parse the headers
			var beginLineNumber = lineNumber;
			if (state < MimeParserState.Content && await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse message headers.");

			var message = new MimeMessage (options, headers, RfcComplianceMode.Loose);
			var messageArgs = new MimeMessageEndEventArgs (message) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeMessageBegin (messageArgs);

			if (format == MimeFormat.Mbox && options.RespectContentLength) {
				contentEnd = 0;

				for (int i = 0; i < headers.Count; i++) {
					if (headers[i].Id != HeaderId.ContentLength)
						continue;

					var value = headers[i].RawValue;
					int index = 0;

					if (!ParseUtils.SkipWhiteSpace (value, ref index, value.Length))
						continue;

					if (!ParseUtils.TryParseInt32 (value, ref index, value.Length, out int length))
						continue;

					contentEnd = GetOffset (inputIndex) + length;
					break;
				}
			}

			var type = GetContentType (null);
			var entity = options.CreateEntity (type, headers, true, 0);
			var entityArgs = new MimeEntityEndEventArgs (entity) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeEntityBegin (entityArgs);

			message.Body = entity;

			if (entity is Multipart multipart)
				await ConstructMultipartAsync (multipart, entityArgs, 0, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart rfc822)
				await ConstructMessagePartAsync (rfc822, entityArgs, 0, cancellationToken).ConfigureAwait (false);
			else
				await ConstructMimePartAsync ((MimePart) entity, entityArgs, cancellationToken).ConfigureAwait (false);

			var endOffset = GetEndOffset (inputIndex);
			messageArgs.HeadersEndOffset = entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
			messageArgs.EndOffset = entityArgs.EndOffset = endOffset;

			if (boundary != BoundaryType.Eos) {
				if (format == MimeFormat.Mbox)
					state = MimeParserState.MboxMarker;
				else
					state = MimeParserState.Complete;
			} else {
				state = MimeParserState.Eos;
			}

			OnMimeEntityEnd (entityArgs);
			OnMimeMessageEnd (messageArgs);

			return message;
		}
	}
}
