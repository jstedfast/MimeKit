//
// MimeParser.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	enum BoundaryType
	{
		None,
		Eos,
		ImmediateBoundary,
		ImmediateEndBoundary,
		ParentBoundary,
		ParentEndBoundary,
	}

	class Boundary
	{
		public static readonly byte[] MboxFrom = Encoding.ASCII.GetBytes ("From ");

		public byte[] Marker { get; private set; }
		public int FinalLength { get { return Marker.Length; } }
		public int Length { get; private set; }
		public int MaxLength { get; private set; }
		public long ContentEnd { get; set; }

		public Boundary (string boundary, int currentMaxLength)
		{
			Marker = Encoding.UTF8.GetBytes ("--" + boundary + "--");
			Length = Marker.Length - 2;

			MaxLength = Math.Max (currentMaxLength, Marker.Length);
			ContentEnd = -1;
		}

		Boundary ()
		{
		}

		public static Boundary CreateMboxBoundary ()
		{
			var boundary = new Boundary ();
			boundary.Marker = MboxFrom;
			boundary.ContentEnd = -1;
			boundary.MaxLength = 5;
			boundary.Length = 5;
			return boundary;
		}

		public override string ToString ()
		{
			return Encoding.UTF8.GetString (Marker, 0, Marker.Length);
		}
	}

	enum MimeParserState : sbyte
	{
		Error = -1,
		Initialized,
		MboxMarker,
		MessageHeaders,
		Headers,
		Content,
		Complete,
		Eos
	}

	/// <summary>
	/// A MIME message and entity parser.
	/// </summary>
	/// <remarks>
	/// A MIME parser is used to parse <see cref="MimeKit.MimeMessage"/> and
	/// <see cref="MimeKit.MimeEntity"/> objects from arbitrary streams.
	/// </remarks>
	public class MimeParser
	{
		static readonly byte[] UTF8ByteOrderMark = { 0xEF, 0xBB, 0xBF };
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

		// header buffer
		byte[] headerBuffer = new byte[512];
		long headerOffset;
		int headerIndex;

		readonly List<Boundary> bounds;
		readonly List<Header> headers;

		MimeParserState state;
		MimeFormat format;
		bool persistent;
		bool eos;

		CancellationToken token = CancellationToken.None;
		ParserOptions options;
		Stream stream;
		long offset;
		private unsafe byte* _inbuf;
		private GCHandle _inbufHandle;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public MimeParser (Stream stream, MimeFormat format, bool persistent = false) : this (ParserOptions.Default, stream, format, persistent)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public MimeParser (Stream stream, bool persistent = false) : this (ParserOptions.Default, stream, MimeFormat.Default, persistent)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public MimeParser (ParserOptions options, Stream stream, bool persistent = false) : this (options, stream, MimeFormat.Default, persistent)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public MimeParser (ParserOptions options, Stream stream, MimeFormat format, bool persistent = false)
		{
			bounds = new List<Boundary> ();
			headers = new List<Header> ();

			SetStream (options, stream, format, persistent);
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
			get { return GetOffset (-1); }
		}

		/// <summary>
		/// Gets the most recent mbox marker offset.
		/// </summary>
		/// <remarks>
		/// Gets the most recent mbox marker offset.
		/// </remarks>
		/// <value>The mbox marker offset.</value>
		public long MboxMarkerOffset {
			get { return mboxMarkerOffset; }
		}

		/// <summary>
		/// Gets the most recent mbox marker.
		/// </summary>
		/// <remarks>
		/// Gets the most recent mbox marker.
		/// </remarks>
		/// <value>The mbox marker.</value>
		public string MboxMarker {
			get { return Encoding.UTF8.GetString (mboxMarkerBuffer, 0, mboxMarkerLength); }
		}

		/// <summary>
		/// Sets the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public void SetStream (ParserOptions options, Stream stream, MimeFormat format, bool persistent = false)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			this.persistent = persistent && stream.CanSeek;
			this.options = options.Clone ();
			this.format = format;
			this.stream = stream;

			inputIndex = inputStart;
			inputEnd = inputStart;

			mboxMarkerOffset = 0;
			mboxMarkerLength = 0;

			offset = stream.CanSeek ? stream.Position : 0;
			headers.Clear ();
			headerOffset = 0;
			headerIndex = 0;
			eos = false;

			bounds.Clear ();
			if (format == MimeFormat.Mbox) {
				bounds.Add (Boundary.CreateMboxBoundary ());

				if (mboxMarkerBuffer == null)
					mboxMarkerBuffer = new byte[ReadAheadSize];
			}

			state = MimeParserState.Initialized;
		}

		/// <summary>
		/// Sets the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public void SetStream (ParserOptions options, Stream stream, bool persistent = false)
		{
			SetStream (options, stream, MimeFormat.Default, persistent);
		}

		/// <summary>
		/// Sets the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void SetStream (Stream stream, MimeFormat format, bool persistent = false)
		{
			SetStream (ParserOptions.Default, stream, format, persistent);
		}

		/// <summary>
		/// Sets the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="ContentObject"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void SetStream (Stream stream, bool persistent = false)
		{
			SetStream (ParserOptions.Default, stream, MimeFormat.Default, persistent);
		}

#if DEBUG
		static string ConvertToCString (byte[] buffer, int startIndex, int length)
		{
			var cstr = new StringBuilder ();
			cstr.AppendCString (buffer, startIndex, length);
			return cstr.ToString ();
		}
#endif

		static int NextAllocSize (int need)
		{
			return (need + 63) & ~63;
		}

		async Task<int> ReadAhead (int atleast, int save)
		{
			int left = inputEnd - inputIndex;

			if (left >= atleast || eos)
				return left;

			int start = inputStart;
			int end = inputEnd;
			int nread;

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

			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != offset)
				stream.Seek (offset, SeekOrigin.Begin);

			nread = await stream.ReadAsync (input, start, end - start, token);

			if (nread > 0) {
				inputEnd += nread;
				offset += nread;
			} else {
				eos = true;
			}

			return inputEnd - inputIndex;
		}

		long GetOffset (int index)
		{
			if (offset == -1)
				return -1;

			if (index == -1)
				index = inputIndex;

			return offset - (inputEnd - index);
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

		static unsafe bool IsMboxMarker (byte* text)
		{
#if COMPARE_QWORD
			const ulong FromMask = 0x000000FFFFFFFFFF;
			const ulong From     = 0x000000206D6F7246;
			ulong* qword = (ulong*) text;

			return (*qword & FromMask) == From;
#else
			byte* inptr = text;

			return *inptr++ == (byte) 'F' && *inptr++ == (byte) 'r' && *inptr++ == (byte) 'o' && *inptr++ == (byte) 'm' && *inptr == (byte) ' ';
#endif
		}

		async Task StepMboxMarker ()
		{
			bool complete = false;
			bool needInput;
			int left = 0;

			mboxMarkerLength = 0;

			do {
				if (await ReadAhead (Math.Max (ReadAheadSize, left), 0) <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				needInput = false;

				unsafe {
					byte* inptr = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;

					*inend = (byte)'\n';

					while (inptr < inend)
					{
						byte* start = inptr;

						// scan for the end of the line
						while (*inptr != (byte)'\n')
							inptr++;

						long length = inptr - start;

						// consume the '\n'
						inptr++;

						if (inptr >= inend)
						{
							// we don't have enough input data
							inputIndex = (int)(start - _inbuf);
							left = (int)length;
							needInput = true;
							break;
						}

						if (length >= 5 && IsMboxMarker(start))
						{
							int startIndex = (int)(start - _inbuf);

							mboxMarkerOffset = GetOffset(startIndex);
							mboxMarkerLength = (int)length;

							if (mboxMarkerBuffer.Length < mboxMarkerLength)
								Array.Resize(ref mboxMarkerBuffer, mboxMarkerLength);

							Buffer.BlockCopy(input, startIndex, mboxMarkerBuffer, 0, (int)length);
							complete = true;
							break;
						}
					}

					if (!needInput)
					{
						inputIndex = (int)(inptr - _inbuf);
						left = 0;
					}
				}
				
			} while (!complete);

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
			headerIndex = 0;
		}

		unsafe void ParseAndAppendHeader ()
		{
			if (headerIndex == 0)
				return;

			fixed (byte* buf = headerBuffer) {
				Header header;

				if (!Header.TryParse (options, buf, headerIndex, false, out header)) {
#if DEBUG
					Debug.WriteLine (string.Format ("Invalid header at offset {0}: {1}", headerOffset, ConvertToCString (headerBuffer, 0, headerIndex)));
#endif
					headerIndex = 0;
					return;
				}

				header.Offset = headerOffset;
				headers.Add (header);
				headerIndex = 0;
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

		static unsafe bool IsEoln (byte *text)
		{
			if (*text == (byte) '\r')
				text++;

			return *text == (byte) '\n';
		}

		async Task<bool> StepByteOrderMark ()
		{
			int bomIndex = 0;

			do {
				if (await ReadAhead (ReadAheadSize, 0) <= 0) {
					// failed to read any data... EOF
					inputIndex = inputEnd;
					return false;
				}

				unsafe {
					byte* inptr = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;

					while (inptr < inend && bomIndex < UTF8ByteOrderMark.Length && *inptr == UTF8ByteOrderMark[bomIndex])
					{
						bomIndex++;
						inptr++;
					}

					inputIndex = (int)(inptr - _inbuf);
				}
				
			} while (inputIndex == inputEnd);

			return true;
		}

		async Task StepHeaders ()
		{
			bool scanningFieldName = true;
			bool checkFolded = false;
			bool midline = false;
			bool blank = false;
			bool valid = true;
			int left = 0;
			long length;
			bool eoln;

			ResetRawHeaderData ();
			headers.Clear ();

			do {
				if (await ReadAhead (Math.Max (ReadAheadSize, left), 0) <= left) {
					// EOF reached before we reached the end of the headers...
					if (left == 0 && !midline) {
						// the last header we encountered has been parsed cleanly, so try to
						// fail gracefully by pretending we found the end of the headers...
						//
						// For more details, see https://github.com/jstedfast/MimeKit/pull/51
						state = MimeParserState.Content;
					} else {
						// the last header we encountered was truncated - probably best
						// to error out in this case
						if (left > 0)
							AppendRawHeaderData (inputIndex, left);

						state = MimeParserState.Error;
					}

					ParseAndAppendHeader ();
					inputIndex = inputEnd;
					return;
				}

				unsafe {
					byte* inptr = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;
					bool needInput = false;

					*inend = (byte)'\n';

					while (inptr < inend)
					{
						byte* start = inptr;

						// if we are scanning a new line, check for a folded header
						if (!midline && checkFolded && !IsBlank(*inptr))
						{
							ParseAndAppendHeader();

							headerOffset = GetOffset((int)(inptr - _inbuf));
							scanningFieldName = true;
							checkFolded = false;
							blank = false;
							valid = true;
						}

						eoln = IsEoln(inptr);
						if (scanningFieldName && !eoln)
						{
							// scan and validate the field name
							if (*inptr != (byte)':')
							{
								*inend = (byte)':';

								while (*inptr != (byte)':')
								{
									// Blank spaces are allowed between the field name and
									// the ':', but field names themselves are not allowed
									// to contain spaces.
									if (IsBlank(*inptr))
									{
										blank = true;
									}
									else if (blank || IsControl(*inptr))
									{
										valid = false;
										break;
									}

									inptr++;
								}

								if (inptr == inend)
								{
									// we don't have enough input data
									left = (int)(inend - start);
									inputIndex = (int)(start - _inbuf);
									needInput = true;
									break;
								}

								*inend = (byte)'\n';
							}
							else
							{
								valid = false;
							}

							if (!valid)
							{
								length = inptr - start;

								if (format == MimeFormat.Mbox && length == 4 && IsMboxMarker(start))
								{
									// we've found the start of the next message...
									inputIndex = (int)(start - _inbuf);
									state = MimeParserState.Complete;
									headerIndex = 0;
									return;
								}

								if (state == MimeParserState.MessageHeaders && headers.Count == 0)
								{
									// ignore From-lines that might appear at the start of a message
									if (length < 5 || !IsMboxMarker(start))
									{
										inputIndex = (int)(start - _inbuf);
										state = MimeParserState.Error;
										headerIndex = 0;
										return;
									}
								}
							}
						}

						scanningFieldName = false;

						while (*inptr != (byte)'\n')
							inptr++;

						if (inptr == inend)
						{
							// we didn't manage to slurp up a full line, save what we have and refill our input buffer
							length = inptr - start;

							if (inptr > start)
							{
								// Note: if the last byte we got was a '\r', rewind a byte
								inptr--;
								if (*inptr == (byte)'\r')
									length--;
								else
									inptr++;
							}

							if (length > 0)
							{
								AppendRawHeaderData((int)(start - _inbuf), (int)length);
								midline = true;
							}

							inputIndex = (int)(inptr - _inbuf);
							left = (int)(inend - inptr);
							needInput = true;
							break;
						}

						// check to see if we've reached the end of the headers
						if (!midline && IsEoln(start))
						{
							inputIndex = (int)(inptr - _inbuf) + 1;
							state = MimeParserState.Content;
							ParseAndAppendHeader();
							headerIndex = 0;
							return;
						}

						length = (inptr + 1) - start;

						AppendRawHeaderData((int)(start - _inbuf), (int)length);
						checkFolded = true;
						midline = false;
						inptr++;
					}

					if (!needInput)
					{
						inputIndex = (int)(inptr - _inbuf);
						left = (int)(inend - inptr);
					}
				}
				
			} while (true);
		}

		async Task<bool> SkipLine ()
		{
			do {
				unsafe  {
					byte* inptr = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;

					*inend = (byte)'\n';

					while (*inptr != (byte)'\n')
						inptr++;

					if (inptr < inend)
					{
						inputIndex = (int)(inptr - _inbuf) + 1;
						return true;
					}
				}

				inputIndex = inputEnd;
				if (await ReadAhead (ReadAheadSize, 0) <= 0)
					return false;
			} while (true);
		}

		async Task<MimeParserState> Step ()
		{
			switch (state) {
			case MimeParserState.Error:
				break;
			case MimeParserState.Initialized:
				if (!await StepByteOrderMark ()) {
					state = MimeParserState.Eos;
					break;
				}

				state = format == MimeFormat.Mbox ? MimeParserState.MboxMarker : MimeParserState.MessageHeaders;
				break;
			case MimeParserState.MboxMarker:
				await StepMboxMarker ();
				break;
			case MimeParserState.MessageHeaders:
			case MimeParserState.Headers:
				await StepHeaders ();
				break;
			case MimeParserState.Content:
				break;
			case MimeParserState.Complete:
				break;
			case MimeParserState.Eos:
				break;
			default:
				throw new IndexOutOfRangeException ("state");
			}

			return state;
		}

		ContentType GetContentType (ContentType parent)
		{
			ContentType type;

			for (int i = 0; i < headers.Count; i++) {
				if (!headers[i].Field.Equals ("Content-Type", StringComparison.OrdinalIgnoreCase))
					continue;

				if (!ContentType.TryParse (options, headers[i].RawValue, out type) && type == null)
					return new ContentType ("application", "octet-stream");

				return type;
			}

			if (parent == null || !parent.Matches ("multipart", "digest"))
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
			if (!IsPossibleBoundary (start, length))
				return BoundaryType.None;

			long curOffset = GetOffset (startIndex);
			for (int i = 0; i < bounds.Count; i++) {
				var boundary = bounds[i];

				if (curOffset >= boundary.ContentEnd && IsBoundary (start, length, boundary.Marker, boundary.FinalLength))
					return i == 0 ? BoundaryType.ImmediateEndBoundary : BoundaryType.ParentEndBoundary;

				if (IsBoundary (start, length, boundary.Marker, boundary.Length))
					return i == 0 ? BoundaryType.ImmediateBoundary : BoundaryType.ParentBoundary;
			}

			return BoundaryType.None;
		}

		bool FoundImmediateBoundary (bool final)
		{
			unsafe {
				int boundaryLength = final ? bounds[0].FinalLength : bounds[0].Length;
				byte* start = _inbuf + inputIndex;
				byte* inend = _inbuf + inputEnd;
				byte* inptr = start;

				*inend = (byte)'\n';

				while (*inptr != (byte)'\n')
					inptr++;

				return IsBoundary(start, (int)(inptr - start), bounds[0].Marker, boundaryLength);
			}
		}

		int GetMaxBoundaryLength ()
		{
			return bounds.Count > 0 ? bounds[0].MaxLength + 2 : 0;
		}

		async Task<BoundaryType> ScanContent (Stream content, bool trimNewLine)
		{
			int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			BoundaryType found = BoundaryType.None;
			int contentIndex = inputIndex;
			bool midline = false;
			int length, nleft;

			do {
				if (contentIndex < inputIndex)
					content.Write (input, contentIndex, inputIndex - contentIndex);

				nleft = inputEnd - inputIndex;
				if (await ReadAhead (atleast, 2) <= 0) {
					contentIndex = inputIndex;
					found = BoundaryType.Eos;
					break;
				}

				int startIndex = inputIndex;

				unsafe
				{
					byte* inptr = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;

					length = inputEnd - inputIndex;
					contentIndex = inputIndex;

					if (midline && length == nleft)
						found = BoundaryType.Eos;

					*inend = (byte)'\n';

					while (inptr < inend)
					{
						// Note: we can always depend on byte[] arrays being 4-byte aligned on 32bit and 64bit architectures
						int alignment = (startIndex + 3) & ~3;
						byte* aligned = _inbuf + alignment;
						byte* start = inptr;
						byte c = *aligned;
						uint mask;

						*aligned = (byte)'\n';
						while (*inptr != (byte)'\n')
							inptr++;
						*aligned = c;

						if (inptr == aligned && c != (byte)'\n')
						{
							// -funroll-loops, bitches.
							uint* dword = (uint*)inptr;

							do
							{
								mask = *dword++ ^ 0x0A0A0A0A;
								mask = ((mask - 0x01010101) & (~mask & 0x80808080));
							} while (mask == 0);

							inptr = (byte*)(dword - 1);
							while (*inptr != (byte)'\n')
								inptr++;
						}

						length = (int)(inptr - start);

						if (inptr < inend)
						{
							found = CheckBoundary(startIndex, start, length);
							if (found != BoundaryType.None)
								break;

							length++;
							inptr++;
						}
						else
						{
							// didn't find the end of the line...
							midline = true;

							if (found == BoundaryType.None)
							{
								// not enough to tell if we found a boundary
								break;
							}

							found = CheckBoundary(startIndex, start, length);
							if (found != BoundaryType.None)
								break;
						}

						startIndex += length;
					}
				}

				inputIndex = startIndex;
			} while (found == BoundaryType.None);

			if (contentIndex < inputIndex)
				content.Write (input, contentIndex, inputIndex - contentIndex);

			if (found != BoundaryType.Eos && trimNewLine) {
				// the last \r\n belongs to the boundary
				if (content.Length > 0) {
					if (input[inputIndex - 2] == (byte) '\r')
						content.SetLength (content.Length - 2);
					else
						content.SetLength (content.Length - 1);
				}
			}

			return found;
		}

		async Task<BoundaryType> ConstructMimePart (MimePart part)
		{
			BoundaryType found;
			Stream content;

			if (persistent) {
				long begin = GetOffset (inputIndex);
				long end;

				using (var measured = new MeasuringStream ()) {
					found = await ScanContent (measured, true);
					end = begin + measured.Length;
				}

				content = new BoundStream (stream, begin, end, true);
			} else {
				content = new MemoryBlockStream ();
				found = await ScanContent (content, true);
				content.Seek (0, SeekOrigin.Begin);
			}

			part.ContentObject = new ContentObject (content, part.ContentTransferEncoding);

			return found;
		}

		async Task<BoundaryType> ConstructMessagePart (MessagePart part)
		{
			BoundaryType found;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (await ReadAhead (atleast, 0) <= 0)
					return BoundaryType.Eos;

				unsafe {
					byte* start = _inbuf + inputIndex;
					byte* inend = _inbuf + inputEnd;
					byte* inptr = start;

					*inend = (byte)'\n';

					while (*inptr != (byte)'\n')
						inptr++;

					found = CheckBoundary(inputIndex, start, (int)(inptr - start));

					switch (found)
					{
						case BoundaryType.ImmediateEndBoundary:
						case BoundaryType.ImmediateBoundary:
						case BoundaryType.ParentBoundary:
							return found;
						case BoundaryType.ParentEndBoundary:
							// ignore "From " boundaries, broken mailers tend to include these...
							if (!IsMboxMarker(start))
								return found;
							break;
					}
				}
				
			}

			// parse the headers...
			state = MimeParserState.Headers;
			if (await Step () == MimeParserState.Error) {
				// Note: this either means that StepHeaders() found the end of the stream
				// or an invalid header field name at the start of the message headers,
				// which likely means that this is not a valid MIME stream?
				return BoundaryType.Eos;
			}

			var message = new MimeMessage (options, headers);
			var type = GetContentType (null);

			var entity = options.CreateEntity (type, headers, true);
			message.Body = entity;

			if (entity is Multipart)
				found = await ConstructMultipart ((Multipart) entity);
			else if (entity is MessagePart)
				found = await ConstructMessagePart ((MessagePart) entity);
			else
				found = await ConstructMimePart ((MimePart) entity);

			part.Message = message;

			return found;
		}

		async Task<BoundaryType> MultipartScanPreamble (Multipart multipart)
		{
			using (var memory = new MemoryStream ()) {
				var found = await ScanContent (memory, false);
				multipart.RawPreamble = memory.ToArray ();
				return found;
			}
		}

		async Task<BoundaryType> MultipartScanEpilogue (Multipart multipart)
		{
			using (var memory = new MemoryStream ()) {
				var found = await ScanContent (memory, true);
				multipart.RawEpilogue = memory.ToArray ();
				return found;
			}
		}

		async Task<BoundaryType> MultipartScanSubparts (Multipart multipart)
		{
			BoundaryType found;

			do {
				// skip over the boundary marker
				if (!await SkipLine ())
					return BoundaryType.Eos;

				// parse the headers
				state = MimeParserState.Headers;
				if (await Step () == MimeParserState.Error)
					return BoundaryType.Eos;

				//if (state == ParserState.Complete && headers.Count == 0)
				//	return BoundaryType.EndBoundary;

				var type = GetContentType (multipart.ContentType);
				var entity = options.CreateEntity (type, headers, false);

				if (entity is Multipart)
					found = await ConstructMultipart ((Multipart) entity);
				else if (entity is MessagePart)
					found = await ConstructMessagePart ((MessagePart) entity);
				else
					found = await ConstructMimePart ((MimePart) entity);

				multipart.Add (entity);
			} while (found == BoundaryType.ImmediateBoundary);

			return found;
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

		async Task<BoundaryType> ConstructMultipart (Multipart multipart)
		{
			var boundary = multipart.Boundary;

			if (boundary == null) {
#if DEBUG
				Debug.WriteLine ("Multipart without a boundary encountered!");
#endif

				// Note: this will scan all content into the preamble...
				return await MultipartScanPreamble (multipart);
			}

			PushBoundary (boundary);

			var found = await MultipartScanPreamble (multipart);
			if (found == BoundaryType.ImmediateBoundary)
				found = await MultipartScanSubparts (multipart);

			if (found == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				await SkipLine ();
				PopBoundary ();

				return await MultipartScanEpilogue (multipart);
			}

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			if (found == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (true))
				return BoundaryType.ImmediateEndBoundary;

			if (found == BoundaryType.ParentBoundary && FoundImmediateBoundary (false))
				return BoundaryType.ImmediateBoundary;

			return found;
		}

		async Task<HeaderList> ParseHeadersAsync ()
		{
			state = MimeParserState.Headers;
			while (state < MimeParserState.Content) {
				if (await Step () == MimeParserState.Error)
					throw new FormatException ("Failed to parse headers.");
			}

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;

			var parsed = new HeaderList (options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
		}

		/// <summary>
		/// Parses a list of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a list of headers from the stream.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task<HeaderList> ParseHeaders (CancellationToken cancellationToken = default (CancellationToken))
		{
			token = cancellationToken;

            unsafe
            {
                _inbufHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
                _inbuf = (byte*)_inbufHandle.AddrOfPinnedObject().ToPointer();
            }

            try
            {
				return await ParseHeadersAsync ();
			}
			finally
			{
                _inbufHandle.Free();
            }
		}

		async Task<MimeEntity> ParseEntityAsync ()
		{
			state = MimeParserState.Headers;
			while (state < MimeParserState.Content) {
				if (await Step () == MimeParserState.Error)
					throw new FormatException ("Failed to parse entity headers.");
			}

			var type = GetContentType (null);
			BoundaryType found;

			// Note: we pass 'false' as the 'toplevel' argument here because
			// we want the entity to consume all of the headers.
			var entity = options.CreateEntity (type, headers, false);
			if (entity is Multipart)
				found = await ConstructMultipart ((Multipart) entity);
			else if (entity is MessagePart)
				found = await ConstructMessagePart ((MessagePart) entity);
			else
				found = await ConstructMimePart ((MimePart) entity);

			if (found != BoundaryType.Eos)
				state = MimeParserState.Complete;
			else
				state = MimeParserState.Eos;

			return entity;
		}

		/// <summary>
		/// Parses an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task<MimeEntity> ParseEntity (CancellationToken cancellationToken = default (CancellationToken))
		{
			token = cancellationToken;
			return ParseEntityAsync();
		}

		async Task<MimeMessage> ParseMessageAsync ()
		{
			BoundaryType found;

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (await Step ()) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			// parse the headers
			while (state < MimeParserState.Content) {
				if (await Step () == MimeParserState.Error)
					throw new FormatException ("Failed to parse message headers.");
			}

			var message = new MimeMessage (options, headers);

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
				found = await ConstructMultipart ((Multipart) entity);
			else if (entity is MessagePart)
				found = await ConstructMessagePart ((MessagePart) entity);
			else
				found = await ConstructMimePart ((MimePart) entity);

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

		/// <summary>
		/// Parses a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the message.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task<MimeMessage> ParseMessage (CancellationToken cancellationToken = default (CancellationToken))
		{
			token = cancellationToken;
			
			unsafe {
				_inbufHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
				_inbuf = (byte*)_inbufHandle.AddrOfPinnedObject().ToPointer();
			}

			try {
				return await ParseMessageAsync();
			} finally {
				_inbufHandle.Free();
			}
		}
	}
}
