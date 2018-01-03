//
// AsyncMimeParser.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	public partial class MimeParser
	{
		async Task<int> ReadAheadAsync (int atleast, int save, CancellationToken cancellationToken)
		{
			int left, start, end;

			if (!AlignReadAheadBuffer (atleast, save, out left, out start, out end))
				return left;

			int nread = await stream.ReadAsync (input, start, end - start, cancellationToken).ConfigureAwait (false);

			if (nread > 0) {
				inputEnd += nread;
				offset += nread;
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
			bool complete = false;
			bool needInput;
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

				needInput = false;

				unsafe {
					fixed (byte* inbuf = input) {
						StepMboxMarker (inbuf, ref needInput, ref complete, ref left);
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

			ResetRawHeaderData ();
			headers.Clear ();

			await ReadAheadAsync (Math.Max (ReadAheadSize, left), 0, cancellationToken).ConfigureAwait (false);

			do {
				unsafe {
					fixed (byte *inbuf = input) {
						if (!StepHeaders (inbuf, ref scanningFieldName, ref checkFolded, ref midline, ref blank, ref valid, ref left))
							return;
					}
				}

				var available = await ReadAheadAsync (left + 1, 0, cancellationToken).ConfigureAwait (false);

				if (available == 0) {
					// EOF reached before we reached the end of the headers...
					if (left > 0) {
						AppendRawHeaderData (inputIndex, left);
						inputIndex = inputEnd;
					}

					ParseAndAppendHeader ();

					// fail gracefully by pretending we found the end of the headers...
					//
					// For more details, see https://github.com/jstedfast/MimeKit/pull/51
					// and https://github.com/jstedfast/MimeKit/issues/348
					state = MimeParserState.Content;
					return;
				}
			} while (true);
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
				break;
			}

			return state;
		}

		struct ScanContentResults
		{
			public readonly BoundaryType Boundary;
			public readonly bool IsEmpty;

			public ScanContentResults (BoundaryType boundary, bool empty)
			{
				Boundary = boundary;
				IsEmpty = empty;
			}
		}

		async Task<ScanContentResults> ScanContentAsync (Stream content, bool trimNewLine, CancellationToken cancellationToken)
		{
			int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			BoundaryType found = BoundaryType.None;
			int contentIndex = inputIndex;
			bool midline = false;
			int nleft;

			do {
				if (contentIndex < inputIndex)
					content.Write (input, contentIndex, inputIndex - contentIndex);

				nleft = inputEnd - inputIndex;
				if (await ReadAheadAsync (atleast, 2, cancellationToken).ConfigureAwait (false) <= 0) {
					contentIndex = inputIndex;
					found = BoundaryType.Eos;
					break;
				}

				unsafe {
					fixed (byte* inbuf = input) {
						ScanContent (inbuf, ref contentIndex, ref nleft, ref midline, ref found);
					}
				}
			} while (found == BoundaryType.None);

			if (contentIndex < inputIndex)
				content.Write (input, contentIndex, inputIndex - contentIndex);

			var empty = content.Length == 0;

			if (found != BoundaryType.Eos && trimNewLine) {
				// the last \r\n belongs to the boundary
				if (content.Length > 0) {
					if (input[inputIndex - 2] == (byte) '\r')
						content.SetLength (content.Length - 2);
					else
						content.SetLength (content.Length - 1);
				}
			}

			return new ScanContentResults (found, empty);
		}

		async Task<BoundaryType> ConstructMimePartAsync (MimePart part, CancellationToken cancellationToken)
		{
			ScanContentResults results;
			Stream content;

			if (persistent) {
				long begin = GetOffset (inputIndex);
				long end;

				using (var measured = new MeasuringStream ()) {
					results = await ScanContentAsync (measured, true, cancellationToken).ConfigureAwait (false);
					end = begin + measured.Length;
				}

				content = new BoundStream (stream, begin, end, true);
			} else {
				content = new MemoryBlockStream ();
				results = await ScanContentAsync (content, true, cancellationToken).ConfigureAwait (false);
				content.Seek (0, SeekOrigin.Begin);
			}

			if (!results.IsEmpty)
				part.Content = new MimeContent (content, part.ContentTransferEncoding);

			return results.Boundary;
		}

		async Task<BoundaryType> ConstructMessagePartAsync (MessagePart part, CancellationToken cancellationToken)
		{
			BoundaryType found;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (await ReadAheadAsync (atleast, 0, cancellationToken).ConfigureAwait (false) <= 0)
					return BoundaryType.Eos;

				unsafe {
					fixed (byte* inbuf = input) {
						byte* start = inbuf + inputIndex;
						byte* inend = inbuf + inputEnd;
						byte* inptr = start;

						*inend = (byte) '\n';

						while (*inptr != (byte) '\n')
							inptr++;

						found = CheckBoundary (inputIndex, start, (int) (inptr - start));

						switch (found) {
						case BoundaryType.ImmediateEndBoundary:
						case BoundaryType.ImmediateBoundary:
						case BoundaryType.ParentBoundary:
							return found;
						case BoundaryType.ParentEndBoundary:
							// ignore "From " boundaries, broken mailers tend to include these...
							if (!IsMboxMarker (start))
								return found;
							break;
						}
					}
				}
			}

			// parse the headers...
			state = MimeParserState.Headers;
			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error) {
				// Note: this either means that StepHeaders() found the end of the stream
				// or an invalid header field name at the start of the message headers,
				// which likely means that this is not a valid MIME stream?
				return BoundaryType.Eos;
			}

			var message = new MimeMessage (options, headers, RfcComplianceMode.Loose);
			var type = GetContentType (null);

			if (preHeaderBuffer.Length > 0) {
				message.MboxMarker = new byte[preHeaderLength];
				Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			}

			var entity = options.CreateEntity (type, headers, true);
			message.Body = entity;

			if (entity is Multipart)
				found = await ConstructMultipartAsync ((Multipart) entity, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart)
				found = await ConstructMessagePartAsync ((MessagePart) entity, cancellationToken).ConfigureAwait (false);
			else
				found = await ConstructMimePartAsync ((MimePart) entity, cancellationToken).ConfigureAwait (false);

			part.Message = message;

			return found;
		}

		async Task<BoundaryType> MultipartScanPreambleAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				var found = await ScanContentAsync (memory, false, cancellationToken).ConfigureAwait (false);
				multipart.RawPreamble = memory.ToArray ();
				return found.Boundary;
			}
		}

		async Task<BoundaryType> MultipartScanEpilogueAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				var found = await ScanContentAsync (memory, true, cancellationToken).ConfigureAwait (false);
				multipart.RawEpilogue = found.IsEmpty ? null : memory.ToArray ();
				return found.Boundary;
			}
		}

		async Task<BoundaryType> MultipartScanSubpartsAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			BoundaryType found;

			do {
				// skip over the boundary marker
				if (!await SkipLineAsync (true, cancellationToken).ConfigureAwait (false))
					return BoundaryType.Eos;

				// parse the headers
				state = MimeParserState.Headers;
				if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
					return BoundaryType.Eos;

				//if (state == ParserState.Complete && headers.Count == 0)
				//	return BoundaryType.EndBoundary;

				var type = GetContentType (multipart.ContentType);
				var entity = options.CreateEntity (type, headers, false);

				if (entity is Multipart)
					found = await ConstructMultipartAsync ((Multipart) entity, cancellationToken).ConfigureAwait (false);
				else if (entity is MessagePart)
					found = await ConstructMessagePartAsync ((MessagePart) entity, cancellationToken).ConfigureAwait (false);
				else
					found = await ConstructMimePartAsync ((MimePart) entity, cancellationToken).ConfigureAwait (false);

				multipart.Add (entity);
			} while (found == BoundaryType.ImmediateBoundary);

			return found;
		}

		async Task<BoundaryType> ConstructMultipartAsync (Multipart multipart, CancellationToken cancellationToken)
		{
			var boundary = multipart.Boundary;

			if (boundary == null) {
#if DEBUG
				Debug.WriteLine ("Multipart without a boundary encountered!");
#endif

				// Note: this will scan all content into the preamble...
				return await MultipartScanPreambleAsync (multipart, cancellationToken).ConfigureAwait (false);
			}

			PushBoundary (boundary);

			var found = await MultipartScanPreambleAsync (multipart, cancellationToken).ConfigureAwait (false);
			if (found == BoundaryType.ImmediateBoundary)
				found = await MultipartScanSubpartsAsync (multipart, cancellationToken).ConfigureAwait (false);

			if (found == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				multipart.WriteEndBoundary = true;
				await SkipLineAsync (false, cancellationToken).ConfigureAwait (false);
				PopBoundary ();

				return await MultipartScanEpilogueAsync (multipart, cancellationToken).ConfigureAwait (false);
			}

			multipart.WriteEndBoundary = false;

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			unsafe {
				fixed (byte* inbuf = input) {
					if (found == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (inbuf, true))
						return BoundaryType.ImmediateEndBoundary;

					if (found == BoundaryType.ParentBoundary && FoundImmediateBoundary (inbuf, false))
						return BoundaryType.ImmediateBoundary;
				}
			}

			return found;
		}

		/// <summary>
		/// Asynchronously parses a list of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a list of headers from the stream.
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
		public async Task<HeaderList> ParseHeadersAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			state = MimeParserState.Headers;
			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;

			var parsed = new HeaderList (options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
		}

		/// <summary>
		/// Asynchronously parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
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
		public async Task<MimeEntity> ParseEntityAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != offset)
				stream.Seek (offset, SeekOrigin.Begin);

			state = MimeParserState.Headers;
			if (await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse entity headers.");

			var type = GetContentType (null);
			BoundaryType found;

			// Note: we pass 'false' as the 'toplevel' argument here because
			// we want the entity to consume all of the headers.
			var entity = options.CreateEntity (type, headers, false);
			if (entity is Multipart)
				found = await ConstructMultipartAsync ((Multipart) entity, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart)
				found = await ConstructMessagePartAsync ((MessagePart) entity, cancellationToken).ConfigureAwait (false);
			else
				found = await ConstructMimePartAsync ((MimePart) entity, cancellationToken).ConfigureAwait (false);

			if (found != BoundaryType.Eos)
				state = MimeParserState.Complete;
			else
				state = MimeParserState.Eos;

			return entity;
		}

		/// <summary>
		/// Asynchronously parses a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
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
		public async Task<MimeMessage> ParseMessageAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			BoundaryType found;

			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != offset)
				stream.Seek (offset, SeekOrigin.Begin);

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (await StepAsync (cancellationToken).ConfigureAwait (false)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			// parse the headers
			if (state < MimeParserState.Content && await StepAsync (cancellationToken).ConfigureAwait (false) == MimeParserState.Error)
				throw new FormatException ("Failed to parse message headers.");

			var message = new MimeMessage (options, headers, RfcComplianceMode.Loose);

			if (format == MimeFormat.Mbox && options.RespectContentLength) {
				bounds[0].ContentEnd = -1;

				for (int i = 0; i < headers.Count; i++) {
					if (!headers[i].Field.Equals ("Content-Length", StringComparison.OrdinalIgnoreCase))
						continue;

					var value = headers[i].RawValue;
					int length, index = 0;

					if (!ParseUtils.TryParseInt32 (value, ref index, value.Length, out length))
						continue;

					long endOffset = GetOffset (inputIndex) + length;

					bounds[0].ContentEnd = endOffset;
					break;
				}
			}

			var type = GetContentType (null);
			var entity = options.CreateEntity (type, headers, true);
			message.Body = entity;

			if (entity is Multipart)
				found = await ConstructMultipartAsync ((Multipart) entity, cancellationToken).ConfigureAwait (false);
			else if (entity is MessagePart)
				found = await ConstructMessagePartAsync ((MessagePart) entity, cancellationToken).ConfigureAwait (false);
			else
				found = await ConstructMimePartAsync ((MimePart) entity, cancellationToken).ConfigureAwait (false);

			if (found != BoundaryType.Eos) {
				if (format == MimeFormat.Mbox)
					state = MimeParserState.MboxMarker;
				else
					state = MimeParserState.Complete;
			} else {
				state = MimeParserState.Eos;
			}

			return message;
		}
	}
}
