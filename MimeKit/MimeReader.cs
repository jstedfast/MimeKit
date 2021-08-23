//
// MimeReader.cs
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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A MIME message and entity reader.
	/// </summary>
	/// <remarks>
	/// <para><see cref="MimeReader"/> provides forward-only, read-only access to MIME data in a stream.</para>
	/// <para><see cref="MimeReader"/> methods let you move through MIME data and read the contents of a node.</para>
	/// <para><see cref="MimeReader"/> uses a pull model to retrieve data.</para>
	/// </remarks>
	public partial class MimeReader
	{
		enum MimeEntityType
		{
			MimePart,
			MessagePart,
			Multipart
		}

		static readonly byte[] UTF8ByteOrderMark = { 0xEF, 0xBB, 0xBF };
		static readonly Task CompletedTask;
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 4;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// mbox From-line state
		byte[] mboxMarkerBuffer;
		long mboxMarkerOffset;
		int mboxMarkerLength;

		// message/rfc822 mbox markers (shouldn't exist, but sometimes do)
		byte[] preHeaderBuffer = new byte[128];
		int preHeaderLength;

		// header buffer
		byte[] headerBuffer = new byte[512];
		long headerOffset;
		int headerIndex;
		int headerCount;

		readonly List<Boundary> bounds = new List<Boundary> ();

		ContentEncoding? currentEncoding;
		ContentType currentContentType;
		long? currentContentLength;

		MimeParserState state;
		BoundaryType boundary;
		MimeFormat format;
		bool toplevel;
		bool eos;

		long headerBlockBegin;
		long headerBlockEnd;
		long contentEnd;

		long prevLineBeginOffset;
		long lineBeginOffset;
		int lineNumber;

		Stream stream;
		long position;

		static MimeReader ()
		{
#if NET45
			CompletedTask = Task.FromResult (true);
#else
			CompletedTask = Task.CompletedTask;
#endif
		}

		public MimeReader (Stream stream, MimeFormat format = MimeFormat.Default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			this.format = format;
			this.stream = stream;

			inputIndex = inputStart;
			inputEnd = inputStart;

			mboxMarkerOffset = 0;
			mboxMarkerLength = 0;
			headerBlockBegin = 0;
			headerBlockEnd = 0;
			lineNumber = 1;
			contentEnd = 0;

			position = stream.CanSeek ? stream.Position : 0;
			prevLineBeginOffset = position;
			lineBeginOffset = position;
			preHeaderLength = 0;
			headerOffset = 0;
			headerIndex = 0;
			toplevel = false;
			eos = false;

			if (format == MimeFormat.Mbox) {
				bounds.Add (Boundary.CreateMboxBoundary ());

				if (mboxMarkerBuffer == null)
					mboxMarkerBuffer = new byte[ReadAheadSize];
			}

			state = MimeParserState.Initialized;
			boundary = BoundaryType.None;
		}

		/// <summary>
		/// Gets a value indicating whether the parser has reached the end of the input stream.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the parser has reached the end of the input stream.
		/// </remarks>
		/// <value><c>true</c> if this parser has reached the end of the input stream;
		/// otherwise, <c>false</c>.</value>
		public bool IsEndOfStream {
			get { return state == MimeParserState.Eos; }
		}

		/// <summary>
		/// Gets the current position of the parser within the stream.
		/// </summary>
		/// <remarks>
		/// Gets the current position of the parser within the stream.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long Position {
			get { return GetOffset (inputIndex); }
		}

		protected virtual void OnMboxMarkerRead (byte[] marker, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMboxMarkerReadAsync (byte[] marker, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		// FIXME: make use of this
		protected virtual Task OnHeaderReadAsync (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnHeadersEnd (long offset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnHeadersEndAsync (long offset, int lineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		#region MimeMessage Events

		protected virtual void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimeMessageBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimeMessageEndAsync (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

#endregion MimeMessage Events

#region MimePart Events

		protected virtual void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimePartContentBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMimePartContentRead (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimePartContentReadAsync (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimePartContentEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMimePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

#endregion MimePart Events

#region MessagePart Events

		protected virtual void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMessagePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMessagePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

#endregion MessagePart Events

#region Multipart Events

		protected virtual void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartBoundaryAsync (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartEndBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartEndBoundaryAsync (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartPreambleBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartPreambleRead (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartPreambleReadAsync (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartPreambleEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartEpilogueBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartEpilogueBeginAsync (long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartEpilogueRead (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartEpilogueReadAsync (byte[] content, int startIndex, int count, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartEpilogueEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

		protected virtual void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		protected virtual Task OnMultipartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}

#endregion Multipart Events

		static int NextAllocSize (int need)
		{
			return (need + 63) & ~63;
		}

		bool AlignReadAheadBuffer (int atleast, int save, out int left, out int start, out int end)
		{
			left = inputEnd - inputIndex;
			start = inputStart;
			end = inputEnd;

			if (left >= atleast || eos)
				return false;

			left += save;

			if (left > 0) {
				int index = inputIndex - save;

				// attempt to align the end of the remaining input with ReadAheadSize
				if (index >= start) {
					start -= Math.Min (ReadAheadSize, left);
					Buffer.BlockCopy (input, index, input, start, left);
					index = start;
					start += left;
				} else if (index > 0) {
					int shift = Math.Min (index, end - start);
					Buffer.BlockCopy (input, index, input, index - shift, left);
					index -= shift;
					start = index + left;
				} else {
					// we can't shift...
					start = end;
				}

				inputIndex = index + save;
				inputEnd = start;
			} else {
				inputIndex = start;
				inputEnd = start;
			}

			end = input.Length - PadSize;

			return true;
		}

		int ReadAhead (int atleast, int save, CancellationToken cancellationToken)
		{
			int nread, left, start, end;

			if (!AlignReadAheadBuffer (atleast, save, out left, out start, out end))
				return left;

			// use the cancellable stream interface if available...
			var cancellable = stream as ICancellableStream;
			if (cancellable != null) {
				nread = cancellable.Read (input, start, end - start, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				nread = stream.Read (input, start, end - start);
			}

			if (nread > 0) {
				inputEnd += nread;
				position += nread;
			} else {
				eos = true;
			}

			return inputEnd - inputIndex;
		}

		long GetOffset (int index)
		{
			if (position == -1)
				return -1;

			return position - (inputEnd - index);
		}

		long GetEndOffset (int index)
		{
			if (boundary != BoundaryType.Eos && index > 1 && input[index - 1] == (byte) '\n') {
				index--;

				if (index > 1 && input[index - 1] == (byte) '\r')
					index--;
			}

			return GetOffset (index);
		}

		int GetLineCount (int beginLineNumber, long beginOffset, long endOffset)
		{
			var lines = lineNumber - beginLineNumber;

			if (lineBeginOffset >= beginOffset && endOffset > lineBeginOffset)
				lines++;

			if (boundary != BoundaryType.Eos && endOffset == prevLineBeginOffset)
				lines--;

			return lines;
		}

		static unsafe bool CStringsEqual (byte* str1, byte* str2, int length)
		{
			byte* se = str1 + length;
			byte* s1 = str1;
			byte* s2 = str2;

			while (s1 < se) {
				if (*s1++ != *s2++)
					return false;
			}

			return true;
		}

		unsafe void StepByteOrderMark (byte* inbuf, ref int bomIndex)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;

			while (inptr < inend && bomIndex < UTF8ByteOrderMark.Length && *inptr == UTF8ByteOrderMark[bomIndex]) {
				bomIndex++;
				inptr++;
			}

			inputIndex = (int) (inptr - inbuf);
		}

		unsafe bool StepByteOrderMark (byte* inbuf, CancellationToken cancellationToken)
		{
			int bomIndex = 0;

			do {
				var available = ReadAhead (ReadAheadSize, 0, cancellationToken);

				if (available <= 0) {
					// failed to read any data... EOF
					inputIndex = inputEnd;
					return false;
				}

				StepByteOrderMark (inbuf, ref bomIndex);
			} while (inputIndex == inputEnd);

			return bomIndex == 0 || bomIndex == UTF8ByteOrderMark.Length;
		}

		static unsafe bool IsMboxMarker (byte* text, bool allowMunged = false)
		{
#if COMPARE_QWORD
			const ulong FromMask = 0x000000FFFFFFFFFF;
			const ulong From     = 0x000000206D6F7246;
			ulong* qword = (ulong*) text;

			return (*qword & FromMask) == From;
#else
			byte* inptr = text;

			if (allowMunged && *inptr == (byte) '>')
				inptr++;

			return *inptr++ == (byte) 'F' && *inptr++ == (byte) 'r' && *inptr++ == (byte) 'o' && *inptr++ == (byte) 'm' && *inptr == (byte) ' ';
#endif
		}

		unsafe bool StepMboxMarker (byte* inbuf, ref int left)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;

			*inend = (byte) '\n';

			while (inptr < inend) {
				int startIndex = inputIndex;
				byte* start = inptr;

				// scan for the end of the line
				while (*inptr != (byte) '\n')
					inptr++;

				var markerLength = (int) (inptr - start);

				if (inptr > start && *(inptr - 1) == (byte) '\r')
					markerLength--;

				// consume the '\n'
				inptr++;

				var lineLength = (int) (inptr - start);

				if (inptr >= inend) {
					// we don't have enough input data
					left = lineLength;
					return false;
				}

				inputIndex += lineLength;
				prevLineBeginOffset = lineBeginOffset;
				lineBeginOffset = GetOffset (inputIndex);
				lineNumber++;

				if (markerLength >= 5 && IsMboxMarker (start)) {
					mboxMarkerOffset = GetOffset (startIndex);
					mboxMarkerLength = markerLength;

					if (mboxMarkerBuffer.Length < mboxMarkerLength)
						Array.Resize (ref mboxMarkerBuffer, mboxMarkerLength);

					Buffer.BlockCopy (input, startIndex, mboxMarkerBuffer, 0, markerLength);

					return true;
				}
			}

			left = 0;

			return false;
		}

		unsafe void StepMboxMarker (byte* inbuf, CancellationToken cancellationToken)
		{
			bool complete;
			int left = 0;

			mboxMarkerLength = 0;

			do {
				var available = ReadAhead (Math.Max (ReadAheadSize, left), 0, cancellationToken);

				if (available <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				complete = StepMboxMarker (inbuf, ref left);
			} while (!complete);

			OnMboxMarkerRead (mboxMarkerBuffer, 0, mboxMarkerLength, mboxMarkerOffset, lineNumber - 1, cancellationToken);

			state = MimeParserState.MessageHeaders;
		}

		void AppendRawHeaderData (int startIndex, int length)
		{
			int left = headerBuffer.Length - headerIndex;

			if (left < length)
				Array.Resize (ref headerBuffer, NextAllocSize (headerIndex + length));

			Buffer.BlockCopy (input, startIndex, headerBuffer, headerIndex, length);
			headerIndex += length;
		}

		void ResetRawHeaderData ()
		{
			preHeaderLength = 0;
			headerIndex = 0;
		}

		unsafe void ParseAndAppendHeader (ParserOptions options, CancellationToken cancellationToken)
		{
			if (headerIndex == 0)
				return;

			fixed (byte* buf = headerBuffer) {
				if (Header.TryParse (options, buf, headerIndex, false, out var header)) {
					var rawValue = header.RawValue;
					int index = 0;

					header.Offset = headerOffset;

					switch (header.Id) {
					case HeaderId.ContentTransferEncoding:
						if (!currentEncoding.HasValue) {
							MimeUtils.TryParse (header.Value, out ContentEncoding encoding);
							currentEncoding = encoding;
						}
						break;
					case HeaderId.ContentLength:
						if (!currentContentLength.HasValue) {
							if (ParseUtils.SkipWhiteSpace (rawValue, ref index, rawValue.Length) && ParseUtils.TryParseInt32 (rawValue, ref index, rawValue.Length, out int length))
								currentContentLength = length;
							else
								currentContentLength = -1;
						}
						break;
					case HeaderId.ContentType:
						if (currentContentType == null) {
							// FIXME: do we really need all this fallback stuff for parameters? I doubt it.
							if (!ContentType.TryParse (options, rawValue, ref index, rawValue.Length, false, out var type) && type == null) {
								// if 'type' is null, then it means that even the mime-type was unintelligible
								type = new ContentType ("application", "octet-stream");

								// attempt to recover any parameters...
								while (index < rawValue.Length && rawValue[index] != ';')
									index++;

								if (++index < rawValue.Length) {
									if (ParameterList.TryParse (options, rawValue, ref index, rawValue.Length, false, out var parameters))
										type.Parameters = parameters;
								}
							}

							currentContentType = type;
						}
						break;
					}

					OnHeaderRead (header, -1, cancellationToken); // FIXME: track the line number that the header starts on?
					headerIndex = 0;
					headerCount++;
				}
			}
		}

		static bool IsControl (byte c)
		{
			return c.IsCtrl ();
		}

		static bool IsBlank (byte c)
		{
			return c.IsBlank ();
		}

		static unsafe bool IsEoln (byte* text)
		{
			if (*text == (byte) '\r')
				text++;

			return *text == (byte) '\n';
		}

		unsafe bool StepHeaders (ParserOptions options, byte* inbuf, ref bool scanningFieldName, ref bool checkFolded, ref bool midline, ref bool blank, ref bool valid, ref int left, CancellationToken cancellationToken)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			bool needInput = false;
			long length;
			bool eoln;

			*inend = (byte) '\n';

			while (inptr < inend) {
				byte* start = inptr;

				// if we are scanning a new line, check for a folded header
				if (!midline && checkFolded && !IsBlank (*inptr)) {
					ParseAndAppendHeader (options, cancellationToken);

					headerOffset = GetOffset ((int) (inptr - inbuf));
					scanningFieldName = true;
					checkFolded = false;
					blank = false;
					valid = true;
				}

				eoln = IsEoln (inptr);
				if (scanningFieldName && !eoln) {
					// scan and validate the field name
					if (*inptr != (byte) ':') {
						*inend = (byte) ':';

						while (*inptr != (byte) ':') {
							// Blank spaces are allowed between the field name and
							// the ':', but field names themselves are not allowed
							// to contain spaces.
							if (IsBlank (*inptr)) {
								blank = true;
							} else if (blank || IsControl (*inptr)) {
								valid = false;
								break;
							}

							inptr++;
						}

						if (inptr == inend) {
							// we don't have enough input data; restore state back to the beginning of the line
							left = (int) (inend - start);
							inputIndex = (int) (start - inbuf);
							needInput = true;
							break;
						}

						*inend = (byte) '\n';
					} else {
						valid = false;
					}

					if (!valid) {
						length = inptr - start;

						if (format == MimeFormat.Mbox && inputIndex >= contentEnd && length >= 5 && IsMboxMarker (start)) {
							// we've found the start of the next message...
							inputIndex = (int) (start - inbuf);
							state = MimeParserState.Complete;
							headerIndex = 0;
							return false;
						}

						if (headerCount == 0) {
							if (state == MimeParserState.MessageHeaders) {
								// ignore From-lines that might appear at the start of a message
								if (toplevel && (length < 5 || !IsMboxMarker (start, true))) {
									// not a From-line...
									inputIndex = (int) (start - inbuf);
									state = MimeParserState.Error;
									headerIndex = 0;
									return false;
								}
							} else if (toplevel && state == MimeParserState.Headers) {
								inputIndex = (int) (start - inbuf);
								state = MimeParserState.Error;
								headerIndex = 0;
								return false;
							}
						}
					}
				}

				scanningFieldName = false;

				while (*inptr != (byte) '\n')
					inptr++;

				if (inptr == inend) {
					// we didn't manage to slurp up a full line, save what we have and refill our input buffer
					length = inptr - start;

					if (inptr > start) {
						// Note: if the last byte we got was a '\r', rewind a byte
						inptr--;
						if (*inptr == (byte) '\r')
							length--;
						else
							inptr++;
					}

					if (length > 0) {
						AppendRawHeaderData ((int) (start - inbuf), (int) length);
						midline = true;
					}

					inputIndex = (int) (inptr - inbuf);
					left = (int) (inend - inptr);
					needInput = true;
					break;
				}

				prevLineBeginOffset = lineBeginOffset;
				lineBeginOffset = GetOffset ((int) (inptr - inbuf) + 1);
				lineNumber++;

				// check to see if we've reached the end of the headers
				if (!midline && IsEoln (start)) {
					inputIndex = (int) (inptr - inbuf) + 1;
					state = MimeParserState.Content;
					ParseAndAppendHeader (options, cancellationToken);
					headerIndex = 0;
					return false;
				}

				length = (inptr + 1) - start;

				if ((boundary = CheckBoundary ((int) (start - inbuf), start, (int) length)) != BoundaryType.None) {
					inputIndex = (int) (start - inbuf);
					state = MimeParserState.Boundary;
					headerIndex = 0;
					return false;
				}

				if (!valid && headerCount == 0) {
					if (length > 0 && preHeaderLength == 0) {
						if (inptr[-1] == (byte) '\r')
							length--;
						length--;

						preHeaderLength = (int) length;

						if (preHeaderLength > preHeaderBuffer.Length)
							Array.Resize (ref preHeaderBuffer, NextAllocSize (preHeaderLength));

						Buffer.BlockCopy (input, (int) (start - inbuf), preHeaderBuffer, 0, preHeaderLength);
					}
					scanningFieldName = true;
					checkFolded = false;
					blank = false;
					valid = true;
				} else {
					AppendRawHeaderData ((int) (start - inbuf), (int) length);
					checkFolded = true;
				}

				midline = false;
				inptr++;
			}

			if (!needInput) {
				inputIndex = (int) (inptr - inbuf);
				left = (int) (inend - inptr);
			}

			return true;
		}

		unsafe void StepHeaders (ParserOptions options, byte* inbuf, CancellationToken cancellationToken)
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

			ReadAhead (ReadAheadSize, 0, cancellationToken);

			do {
				if (!StepHeaders (options, inbuf, ref scanningFieldName, ref checkFolded, ref midline, ref blank, ref valid, ref left, cancellationToken))
					break;

				var available = ReadAhead (left + 1, 0, cancellationToken);

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

		unsafe bool SkipLine (byte* inbuf, bool consumeNewLine)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;

			*inend = (byte) '\n';

			while (*inptr != (byte) '\n')
				inptr++;

			if (inptr < inend) {
				inputIndex = (int) (inptr - inbuf);

				if (consumeNewLine) {
					inputIndex++;
					lineNumber++;
					prevLineBeginOffset = lineBeginOffset;
					lineBeginOffset = GetOffset (inputIndex);
				} else if (*(inptr - 1) == (byte) '\r') {
					inputIndex--;
				}

				return true;
			}

			inputIndex = inputEnd;

			return false;
		}

		unsafe bool SkipLine (byte* inbuf, bool consumeNewLine, CancellationToken cancellationToken)
		{
			do {
				if (SkipLine (inbuf, consumeNewLine))
					return true;

				if (ReadAhead (ReadAheadSize, 1, cancellationToken) <= 0)
					return false;
			} while (true);
		}

		unsafe MimeParserState Step (ParserOptions options, byte* inbuf, CancellationToken cancellationToken)
		{
			switch (state) {
			case MimeParserState.Initialized:
				if (!StepByteOrderMark (inbuf, cancellationToken)) {
					state = MimeParserState.Eos;
					break;
				}

				state = format == MimeFormat.Mbox ? MimeParserState.MboxMarker : MimeParserState.MessageHeaders;
				break;
			case MimeParserState.MboxMarker:
				StepMboxMarker (inbuf, cancellationToken);
				break;
			case MimeParserState.MessageHeaders:
			case MimeParserState.Headers:
				StepHeaders (options, inbuf, cancellationToken);
				toplevel = false;
				break;
			}

			return state;
		}

		ContentType GetContentType (ContentType parent)
		{
			if (currentContentType != null)
				return currentContentType;

			if (parent == null || !parent.IsMimeType ("multipart", "digest"))
				return new ContentType ("text", "plain");

			return new ContentType ("message", "rfc822");
		}

		unsafe bool IsPossibleBoundary (byte* text, int length)
		{
			if (length < 2)
				return false;

			if (*text == (byte) '-' && *(text + 1) == (byte) '-')
				return true;

			if (format == MimeFormat.Mbox && length >= 5 && IsMboxMarker (text))
				return true;

			return false;
		}

		static unsafe bool IsBoundary (byte* text, int length, byte[] boundary, int boundaryLength)
		{
			if (boundaryLength > length)
				return false;

			fixed (byte* boundaryptr = boundary) {
				// make sure that the text matches the boundary
				if (!CStringsEqual (text, boundaryptr, boundaryLength))
					return false;

				// if this is an mbox marker, we're done
				if (IsMboxMarker (text))
					return true;

				// the boundary may optionally be followed by lwsp
				byte* inptr = text + boundaryLength;
				byte* inend = text + length;

				while (inptr < inend) {
					if (!(*inptr).IsWhitespace ())
						return false;

					inptr++;
				}
			}

			return true;
		}

		unsafe BoundaryType CheckBoundary (int startIndex, byte* start, int length)
		{
			int count = bounds.Count;

			if (!IsPossibleBoundary (start, length))
				return BoundaryType.None;

			if (contentEnd > 0) {
				// We'll need to special-case checking for the mbox From-marker when respecting Content-Length
				count--;
			}

			for (int i = 0; i < count; i++) {
				var boundary = bounds[i];

				if (IsBoundary (start, length, boundary.Marker, boundary.FinalLength))
					return i == 0 ? BoundaryType.ImmediateEndBoundary : BoundaryType.ParentEndBoundary;

				if (IsBoundary (start, length, boundary.Marker, boundary.Length))
					return i == 0 ? BoundaryType.ImmediateBoundary : BoundaryType.ParentBoundary;
			}

			if (contentEnd > 0) {
				// now it is time to check the mbox From-marker for the Content-Length case
				long curOffset = GetOffset (startIndex);
				var boundary = bounds[count];

				if (curOffset >= contentEnd && IsBoundary (start, length, boundary.Marker, boundary.Length))
					return BoundaryType.ImmediateEndBoundary;
			}

			return BoundaryType.None;
		}

		unsafe bool FoundImmediateBoundary (byte* inbuf, bool final)
		{
			int boundaryLength = final ? bounds[0].FinalLength : bounds[0].Length;
			byte* start = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			byte* inptr = start;

			*inend = (byte) '\n';

			while (*inptr != (byte) '\n')
				inptr++;

			return IsBoundary (start, (int) (inptr - start), bounds[0].Marker, boundaryLength);
		}

		int GetMaxBoundaryLength ()
		{
			return bounds.Count > 0 ? bounds[0].MaxLength + 2 : 0;
		}

		static bool IsMultipart (ContentType contentType)
		{
			return contentType.MediaType.Equals ("multipart", StringComparison.OrdinalIgnoreCase);
		}

		static readonly string[] MessageMediaSubtypes = { "rfc822", "news", "global", "global-headers", "external-body", "rfc2822" };

		static bool IsMessagePart (ContentType contentType, ContentEncoding? encoding)
		{
			if (encoding.HasValue && ParserOptions.IsEncoded (encoding.Value))
				return false;

			if (contentType.MediaType.Equals ("message", StringComparison.OrdinalIgnoreCase)) {
				for (int i = 0; i < MessageMediaSubtypes.Length; i++) {
					if (contentType.MediaSubtype.Equals (MessageMediaSubtypes[i], StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			if (contentType.IsMimeType ("text", "rfc822-headers"))
				return true;

			return false;
		}

		unsafe void ScanContent (byte* inbuf, ref int contentIndex, ref int nleft, ref bool midline, ref bool[] formats)
		{
			int length = inputEnd - inputIndex;
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			int startIndex = inputIndex;

			contentIndex = inputIndex;

			if (midline && length == nleft)
				boundary = BoundaryType.Eos;

			*inend = (byte) '\n';

			while (inptr < inend) {
				// Note: we can always depend on byte[] arrays being 4-byte aligned on 32bit and 64bit architectures
				int alignment = (startIndex + 3) & ~3;
				byte* aligned = inbuf + alignment;
				byte* start = inptr;
				byte c = *aligned;
				uint mask;

				*aligned = (byte) '\n';
				while (*inptr != (byte) '\n')
					inptr++;
				*aligned = c;

				if (inptr == aligned && c != (byte) '\n') {
					// -funroll-loops, yippee ki-yay.
					uint* dword = (uint*) inptr;

					do {
						mask = *dword++ ^ 0x0A0A0A0A;
						mask = ((mask - 0x01010101) & (~mask & 0x80808080));
					} while (mask == 0);

					inptr = (byte*) (dword - 1);
					while (*inptr != (byte) '\n')
						inptr++;
				}

				length = (int) (inptr - start);

				if (inptr < inend) {
					if ((boundary = CheckBoundary (startIndex, start, length)) != BoundaryType.None)
						break;

					if (length > 0 && *(inptr - 1) == (byte) '\r')
						formats[(int) NewLineFormat.Dos] = true;
					else
						formats[(int) NewLineFormat.Unix] = true;

					lineNumber++;
					length++;
					inptr++;

					prevLineBeginOffset = lineBeginOffset;
					lineBeginOffset = GetOffset ((int) (inptr - inbuf));
				} else {
					// didn't find the end of the line...
					midline = true;

					if (boundary == BoundaryType.None) {
						// not enough to tell if we found a boundary
						break;
					}

					if ((boundary = CheckBoundary (startIndex, start, length)) != BoundaryType.None)
						break;
				}

				startIndex += length;
			}

			inputIndex = startIndex;
		}

		class ScanContentResult
		{
			public readonly NewLineFormat? Format;
			public readonly bool IsEmpty;
			public readonly int ContentLength;
			public readonly int Lines;

			public ScanContentResult (int contentLength, int lines, bool[] formats, bool isEmpty)
			{
				ContentLength = contentLength;
				if (formats[(int) NewLineFormat.Unix] && formats[(int) NewLineFormat.Dos])
					Format = NewLineFormat.Mixed;
				else if (formats[(int) NewLineFormat.Unix])
					Format = NewLineFormat.Unix;
				else if (formats[(int) NewLineFormat.Dos])
					Format = NewLineFormat.Dos;
				else
					Format = null;
				IsEmpty = isEmpty;
				Lines = lines;
			}
		}

		enum ScanContentType
		{
			MimeContent,
			MultipartPreamble,
			MultipartEpilogue
		}

		unsafe ScanContentResult ScanContent (ScanContentType type, byte* inbuf, long beginOffset, int beginLineNumber, bool trimNewLine, CancellationToken cancellationToken)
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
						OnMultipartPreambleRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
						break;
					case ScanContentType.MultipartEpilogue:
						OnMultipartEpilogueRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
						break;
					default:
						OnMimePartContentRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
						break;
					}

					contentLength += inputIndex - contentIndex;
				}

				nleft = inputEnd - inputIndex;
				if (ReadAhead (atleast, 2, cancellationToken) <= 0) {
					boundary = BoundaryType.Eos;
					contentIndex = inputIndex;
					break;
				}

				ScanContent (inbuf, ref contentIndex, ref nleft, ref midline, ref formats);
			} while (boundary == BoundaryType.None);

			if (contentIndex < inputIndex) {
				switch (type) {
				case ScanContentType.MultipartPreamble:
					OnMultipartPreambleRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
					break;
				case ScanContentType.MultipartEpilogue:
					OnMultipartEpilogueRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
					break;
				default:
					OnMimePartContentRead (input, contentIndex, inputIndex - contentIndex, cancellationToken);
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

		unsafe int ConstructMimePart (byte* inbuf, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMimePartContentBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MimeContent, inbuf, beginOffset, beginLineNumber, true, cancellationToken);
			OnMimePartContentEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken);

			return result.Lines;
		}

		unsafe int ConstructMessagePart (ParserOptions options, byte* inbuf, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (ReadAhead (atleast, 0, cancellationToken) <= 0) {
					boundary = BoundaryType.Eos;
					return 0;
				}

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

			// parse the headers...
			state = MimeParserState.MessageHeaders;
			if (Step (options, inbuf, cancellationToken) == MimeParserState.Error) {
				// Note: this either means that StepHeaders() found the end of the stream
				// or an invalid header field name at the start of the message headers,
				// which likely means that this is not a valid MIME stream?
				boundary = BoundaryType.Eos;
				return GetLineCount (beginLineNumber, beginOffset, GetEndOffset (inputIndex));
			}

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			OnMimeMessageBegin (currentBeginOffset, beginLineNumber, cancellationToken);

			if (preHeaderBuffer.Length > 0) {
				// FIXME: how to solve this?
				//message.MboxMarker = new byte[preHeaderLength];
				//Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			}

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (depth < options.MaxMimeDepth && IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (options, type, inbuf, depth + 1, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (options, inbuf, depth + 1, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (inbuf, cancellationToken);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				OnMultipartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			case MimeEntityType.MessagePart:
				OnMessagePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			default:
				OnMimePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			}

			OnMimeMessageEnd (currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		unsafe void MultipartScanPreamble (byte* inbuf, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMultipartPreambleBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MultipartPreamble, inbuf, beginOffset, beginLineNumber, false, cancellationToken);
			OnMultipartPreambleEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken);
		}

		unsafe void MultipartScanEpilogue (byte* inbuf, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMultipartEpilogueBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MultipartEpilogue, inbuf, beginOffset, beginLineNumber, true, cancellationToken);
			OnMultipartEpilogueEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken);
		}

		unsafe void MultipartScanSubparts (ParserOptions options, ContentType multipartContentType, byte* inbuf, int depth, CancellationToken cancellationToken)
		{
			var boundaryOffset = GetOffset (inputIndex);

			do {
				// skip over the boundary marker
				if (!SkipLine (inbuf, true, cancellationToken)) {
					OnMultipartBoundary (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber, cancellationToken);
					boundary = BoundaryType.Eos;
					return;
				}

				OnMultipartBoundary (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber - 1, cancellationToken);

				var beginLineNumber = lineNumber;

				// parse the headers
				state = MimeParserState.Headers;
				if (Step (options, inbuf, cancellationToken) == MimeParserState.Error) {
					boundary = BoundaryType.Eos;
					return;
				}

				if (state == MimeParserState.Boundary) {
					if (headerCount == 0) {
						if (boundary == BoundaryType.ImmediateBoundary) {
							boundaryOffset = GetOffset (inputIndex);
							continue;
						}
						return;
					}

					// This part has no content, but that will be handled in ConstructMultipart()
					// or ConstructMimePart().
				}

				//if (state == ParserState.Complete && headers.Count == 0)
				//	return BoundaryType.EndBoundary;

				var type = GetContentType (multipartContentType);
				var currentHeadersEndOffset = headerBlockEnd;
				var currentBeginOffset = headerBlockBegin;
				MimeEntityType entityType;
				int lines;

				if (depth < options.MaxMimeDepth && IsMultipart (type)) {
					OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMultipart (options, type, inbuf, depth + 1, cancellationToken);
					entityType = MimeEntityType.Multipart;
				} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
					OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMessagePart (options, inbuf, depth + 1, cancellationToken);
					entityType = MimeEntityType.MessagePart;
				} else {
					OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMimePart (inbuf, cancellationToken);
					entityType = MimeEntityType.MimePart;
				}

				var endOffset = GetEndOffset (inputIndex);
				currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

				switch (entityType) {
				case MimeEntityType.Multipart:
					OnMultipartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
					break;
				case MimeEntityType.MessagePart:
					OnMessagePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
					break;
				default:
					OnMimePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
					break;
				}

				boundaryOffset = endOffset;
			} while (boundary == BoundaryType.ImmediateBoundary);
		}

		void PushBoundary (string boundary)
		{
			if (bounds.Count > 0)
				bounds.Insert (0, new Boundary (boundary, bounds[0].MaxLength));
			else
				bounds.Add (new Boundary (boundary, 0));
		}

		void PopBoundary ()
		{
			bounds.RemoveAt (0);
		}

		unsafe int ConstructMultipart (ParserOptions options, ContentType contentType, byte* inbuf, int depth, CancellationToken cancellationToken)
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
				MultipartScanPreamble (inbuf, cancellationToken);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			PushBoundary (marker);

			MultipartScanPreamble (inbuf, cancellationToken);
			if (boundary == BoundaryType.ImmediateBoundary)
				MultipartScanSubparts (options, contentType, inbuf, depth, cancellationToken);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				// FIXME: multipart.WriteEndBoundary = true;

				var boundaryOffset = GetOffset (inputIndex);
				var boundaryLineNumber = lineNumber;

				SkipLine (inbuf, false, cancellationToken);

				OnMultipartEndBoundary (marker, boundaryOffset, GetOffset (inputIndex), boundaryLineNumber, cancellationToken);

				PopBoundary ();

				MultipartScanEpilogue (inbuf, cancellationToken);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// FIXME: multipart.WriteEndBoundary = false;

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			if (boundary == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (inbuf, true))
				boundary = BoundaryType.ImmediateEndBoundary;
			else if (boundary == BoundaryType.ParentBoundary && FoundImmediateBoundary (inbuf, false))
				boundary = BoundaryType.ImmediateBoundary;

			endOffset = GetEndOffset (inputIndex);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		unsafe void ReadEntity (ParserOptions options, byte* inbuf, CancellationToken cancellationToken)
		{
			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (options, inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse entity headers.");

			var type = GetContentType (null);
			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (options, type, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (options, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (inbuf, cancellationToken);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				OnMultipartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			case MimeEntityType.MessagePart:
				OnMessagePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			default:
				OnMimePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			}

			if (boundary != BoundaryType.Eos)
				state = MimeParserState.Complete;
			else
				state = MimeParserState.Eos;
		}

		/// <summary>
		/// Parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
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
		public void ReadEntity (ParserOptions options, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			unsafe {
				fixed (byte* inbuf = input) {
					ReadEntity (options, inbuf, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
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
		public void ReadEntity (CancellationToken cancellationToken = default (CancellationToken))
		{
			ReadEntity (ParserOptions.Default, cancellationToken);
		}

		unsafe void ReadMessage (ParserOptions options, byte* inbuf, CancellationToken cancellationToken)
		{
			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (Step (options, inbuf, cancellationToken)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			toplevel = true;

			// parse the headers
			var beginLineNumber = lineNumber;
			if (state < MimeParserState.Content && Step (options, inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse message headers.");

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			OnMimeMessageBegin (currentBeginOffset, beginLineNumber, cancellationToken);

			if (format == MimeFormat.Mbox && options.RespectContentLength && currentContentLength.HasValue && currentContentLength.Value != -1)
				contentEnd = GetOffset (inputIndex) + currentContentLength.Value;
			else
				contentEnd = 0;

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (options, type, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (options, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (inbuf, cancellationToken);
				entityType = MimeEntityType.MimePart;
			}

			var endOffset = GetEndOffset (inputIndex);
			currentHeadersEndOffset = Math.Min (currentHeadersEndOffset, endOffset);

			switch (entityType) {
			case MimeEntityType.Multipart:
				OnMultipartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			case MimeEntityType.MessagePart:
				OnMessagePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			default:
				OnMimePartEnd (type, currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);
				break;
			}

			OnMimeMessageEnd (currentBeginOffset, beginLineNumber, currentHeadersEndOffset, endOffset, lines, cancellationToken);

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
		/// Parses a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <returns>The parsed message.</returns>
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
		public void ReadMessage (ParserOptions options, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			unsafe {
				fixed (byte* inbuf = input) {
					ReadMessage (options, inbuf, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Parses a message from the stream.
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
		public void ReadMessage (CancellationToken cancellationToken = default (CancellationToken))
		{
			ReadMessage (ParserOptions.Default, cancellationToken);
		}
	}
}
