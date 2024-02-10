//
// MimeParser.cs
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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

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
		public static readonly byte[] MboxFrom = "From "u8.ToArray();

		public byte[] Marker { get; private set; }
		public int FinalLength { get { return Marker.Length; } }
		public int Length { get; private set; }
		public int MaxLength { get; private set; }

		public Boundary (string boundary, int currentMaxLength)
		{
			Marker = Encoding.UTF8.GetBytes ("--" + boundary + "--");
			Length = Marker.Length - 2;

			MaxLength = Math.Max (currentMaxLength, Marker.Length);
		}

		Boundary ()
		{
		}

		public static Boundary CreateMboxBoundary ()
		{
			return new Boundary {
				Marker = MboxFrom,
				MaxLength = 5,
				Length = 5
			};
		}

#if DEBUG_PARSER
		public override string ToString ()
		{
			return Encoding.UTF8.GetString (Marker, 0, Marker.Length);
		}
#endif
	}

	enum MimeParserState : sbyte
	{
		Error = -1,
		Initialized,
		MboxMarker,
		MessageHeaders,
		Headers,
		Content,
		Boundary,
		Complete,
		Eos
	}

	/// <summary>
	/// A MIME message and entity parser.
	/// </summary>
	/// <remarks>
	/// A MIME parser is used to parse <see cref="MimeMessage"/> and
	/// <see cref="MimeEntity"/> objects from arbitrary streams.
	/// </remarks>
	public partial class MimeParser : IMimeParser, IEnumerable<MimeMessage>
	{
		static ReadOnlySpan<byte> UTF8ByteOrderMark => new byte[] { 0xEF, 0xBB, 0xBF };
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

		readonly List<Boundary> bounds = new List<Boundary> ();
		readonly List<Header> headers = new List<Header> ();

		MimeParserState state;
		BoundaryType boundary;
		MimeFormat format;
		bool persistent;
		bool toplevel;
		bool eos;

		ParserOptions options; // FIXME: might be better if devs passed ParserOptions into the Parse*() methods rather than .ctor and/or SetStream()
		long headerBlockBegin;
		long headerBlockEnd;
		long contentEnd;

		long prevLineBeginOffset;
		long lineBeginOffset;
		int lineNumber;

		Stream stream;
		long position;

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMessage" />
		/// </example>
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
		/// Initialize a new instance of the <see cref="MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
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
		/// Initialize a new instance of the <see cref="MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
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
		/// Initialize a new instance of the <see cref="MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="MimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
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
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			Options = options;

			SetStream (stream, format, persistent);
		}

		/// <summary>
		/// Get or set the parser options.
		/// </summary>
		/// <remarks>
		/// Gets or sets the parser options.
		/// </remarks>
		/// <value>The parser options.</value>
		public ParserOptions Options {
			get {
				return options;
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (value == ParserOptions.Default)
					options = value.Clone ();
				else
					options = value;
			}
		}

		/// <summary>
		/// Get a value indicating whether the parser has reached the end of the input stream.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the parser has reached the end of the input stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMbox" />
		/// </example>
		/// <value><c>true</c> if this parser has reached the end of the input stream;
		/// otherwise, <c>false</c>.</value>
		public bool IsEndOfStream {
			get { return state == MimeParserState.Eos; }
		}

		/// <summary>
		/// Get the current position of the parser within the stream.
		/// </summary>
		/// <remarks>
		/// Gets the current position of the parser within the stream.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long Position {
			get { return GetOffset (inputIndex); }
		}

		/// <summary>
		/// Get the most recent mbox marker offset.
		/// </summary>
		/// <remarks>
		/// Gets the most recent mbox marker offset.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMbox" />
		/// </example>
		/// <value>The mbox marker offset.</value>
		public long MboxMarkerOffset {
			get { return mboxMarkerOffset; }
		}

		/// <summary>
		/// Get the most recent mbox marker.
		/// </summary>
		/// <remarks>
		/// Gets the most recent mbox marker.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMbox" />
		/// </example>
		/// <value>The mbox marker.</value>
		public string MboxMarker {
			get { return Encoding.UTF8.GetString (mboxMarkerBuffer, 0, mboxMarkerLength); }
		}

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
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
		[Obsolete ("Use SetStream(Stream, MimeFormat) or SetStream(Stream, MimeFormat, bool) instead.")]
		public void SetStream (ParserOptions options, Stream stream, MimeFormat format, bool persistent = false)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			Options = options;

			SetStream (stream, format, persistent);
		}

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		[Obsolete ("Use SetStream(Stream, MimeFormat) or SetStream(Stream, MimeFormat, bool) instead.")]
		public void SetStream (ParserOptions options, Stream stream, bool persistent = false)
		{
			SetStream (options, stream, MimeFormat.Default, persistent);
		}

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void SetStream (Stream stream, MimeFormat format, bool persistent)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			this.persistent = persistent && stream.CanSeek;
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
			headers.Clear ();
			headerOffset = 0;
			headerIndex = 0;
			toplevel = false;
			eos = false;

			bounds.Clear ();
			if (format == MimeFormat.Mbox) {
				bounds.Add (Boundary.CreateMboxBoundary ());

				mboxMarkerBuffer ??= new byte[ReadAheadSize];
			}

			state = MimeParserState.Initialized;
			boundary = BoundaryType.None;
		}

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// Sets the stream to parse.
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void SetStream (Stream stream, MimeFormat format = MimeFormat.Default)
		{
			SetStream (stream, format, false);
		}

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void SetStream (Stream stream, bool persistent)
		{
			SetStream (stream, MimeFormat.Default, persistent);
		}

		/// <summary>
		/// An event signifying the beginning of a new <see cref="MimeMessage"/> has been encountered.
		/// </summary>
		/// <remarks>
		/// An event signifying the beginning of a new <see cref="MimeMessage"/> has been encountered.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="MessageOffsets" />
		/// </example>
		public event EventHandler<MimeMessageBeginEventArgs> MimeMessageBegin;

		/// <summary>
		/// Invoked when the parser begins parsing a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// Invoked when the parser begins parsing a <see cref="MimeMessage"/>.
		/// </remarks>
		/// <param name="args">The parsed state.</param>
		protected virtual void OnMimeMessageBegin (MimeMessageBeginEventArgs args)
		{
			MimeMessageBegin?.Invoke (this, args);
		}

		/// <summary>
		/// An event signifying the end of a <see cref="MimeMessage"/> has been encountered.
		/// </summary>
		/// <remarks>
		/// An event signifying the end of a <see cref="MimeMessage"/> has been encountered.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="MessageOffsets" />
		/// </example>
		public event EventHandler<MimeMessageEndEventArgs> MimeMessageEnd;

		/// <summary>
		/// Invoked when the parser has completed parsing a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// Invoked when the parser has completed parsing a <see cref="MimeMessage"/>.
		/// </remarks>
		/// <param name="args">The parsed state.</param>
		protected virtual void OnMimeMessageEnd (MimeMessageEndEventArgs args)
		{
			MimeMessageEnd?.Invoke (this, args);
		}

		/// <summary>
		/// An event signifying the beginning of a new <see cref="MimeEntity"/> has been encountered.
		/// </summary>
		/// <remarks>
		/// An event signifying the beginning of a new <see cref="MimeEntity"/> has been encountered.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="MessageOffsets" />
		/// </example>
		public event EventHandler<MimeEntityBeginEventArgs> MimeEntityBegin;

		/// <summary>
		/// Invoked when the parser begins parsing a <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Invoked when the parser begins parsing a <see cref="MimeEntity"/>.
		/// </remarks>
		/// <param name="args">The parsed state.</param>
		protected virtual void OnMimeEntityBegin (MimeEntityBeginEventArgs args)
		{
			MimeEntityBegin?.Invoke (this, args);
		}

		/// <summary>
		/// An event signifying the end of a <see cref="MimeEntity"/> has been encountered.
		/// </summary>
		/// <remarks>
		/// An event signifying the end of a <see cref="MimeEntity"/> has been encountered.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="MessageOffsets" />
		/// </example>
		public event EventHandler<MimeEntityEndEventArgs> MimeEntityEnd;

		/// <summary>
		/// Invoked when the parser has completed parsing a <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Invoked when the parser has completed parsing a <see cref="MimeEntity"/>.
		/// </remarks>
		/// <param name="args">The parsed state.</param>
		protected virtual void OnMimeEntityEnd (MimeEntityEndEventArgs args)
		{
			MimeEntityEnd?.Invoke (this, args);
		}

#if DEBUG_PARSER
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
			int nread;

			if (!AlignReadAheadBuffer (atleast, save, out int left, out int start, out int end))
				return left;

			// use the cancellable stream interface if available...
			if (stream is ICancellableStream cancellable) {
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

				if (inptr == inend) {
					// we don't have enough input data
					left = (int) (inptr - start);
					return false;
				}

				var markerLength = (int) (inptr - start);

				if (inptr > start && *(inptr - 1) == (byte) '\r')
					markerLength--;

				// consume the '\n'
				inptr++;

				var lineLength = (int) (inptr - start);

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

		unsafe void ParseAndAppendHeader ()
		{
			if (headerIndex == 0)
				return;

			fixed (byte* buf = headerBuffer) {
				if (Header.TryParse (options, buf, headerIndex, false, out var header)) {
					header.Offset = headerOffset;
					headers.Add (header);
					headerIndex = 0;
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

		unsafe bool StepHeaders (byte* inbuf, ref bool scanningFieldName, ref bool checkFolded, ref bool midline, ref bool blank, ref bool valid, ref int left)
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
					ParseAndAppendHeader ();

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

						if (format == MimeFormat.Mbox && GetOffset ((int) (start - inbuf)) >= contentEnd && length >= 5 && IsMboxMarker (start)) {
							// we've found the start of the next message...
							inputIndex = (int) (start - inbuf);
							state = MimeParserState.Complete;
							headerIndex = 0;
							return false;
						}

						if (headers.Count == 0) {
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

					// Note: if the last byte we got was a '\r', rewind a byte
					if (inptr > start && *(inptr - 1) == (byte) '\r') {
						length--;
						inptr--;
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
					ParseAndAppendHeader ();
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

				if (!valid && headers.Count == 0) {
					if (length > 0 && preHeaderLength == 0) {
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

		unsafe void StepHeaders (byte* inbuf, CancellationToken cancellationToken)
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

			ReadAhead (ReadAheadSize, 0, cancellationToken);

			do {
				if (!StepHeaders (inbuf, ref scanningFieldName, ref checkFolded, ref midline, ref blank, ref valid, ref left))
					break;

				var available = ReadAhead (left + 1, 0, cancellationToken);

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

		unsafe bool InnerSkipLine (byte* inbuf, bool consumeNewLine)
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
				if (InnerSkipLine (inbuf, consumeNewLine))
					return true;

				if (ReadAhead (ReadAheadSize, 1, cancellationToken) <= 0)
					return false;
			} while (true);
		}

		unsafe MimeParserState Step (byte* inbuf, CancellationToken cancellationToken)
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
				StepHeaders (inbuf, cancellationToken);
				toplevel = false;
				break;
			}

			return state;
		}

		ContentType GetContentType (ContentType parent)
		{
			for (int i = 0; i < headers.Count; i++) {
				if (!headers[i].Field.Equals ("Content-Type", StringComparison.OrdinalIgnoreCase))
					continue;

				var rawValue = headers[i].RawValue;
				int index = 0;

				if (!ContentType.TryParse (options, rawValue, ref index, rawValue.Length, false, out var type) && type is null) {
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

				return type;
			}

			if (parent is null || !parent.IsMimeType ("multipart", "digest"))
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

		unsafe void ScanContent (byte* inbuf, ref int nleft, ref bool midline, ref bool[] formats)
		{
			int length = inputEnd - inputIndex;
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			int startIndex = inputIndex;

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

			public ScanContentResult (bool[] formats, bool isEmpty)
			{
				if (formats[(int) NewLineFormat.Unix] && formats[(int) NewLineFormat.Dos])
					Format = NewLineFormat.Mixed;
				else if (formats[(int) NewLineFormat.Unix])
					Format = NewLineFormat.Unix;
				else if (formats[(int) NewLineFormat.Dos])
					Format = NewLineFormat.Dos;
				else
					Format = null;
				IsEmpty = isEmpty;
			}
		}

		unsafe ScanContentResult ScanContent (byte* inbuf, Stream content, bool trimNewLine, CancellationToken cancellationToken)
		{
			int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());
			var formats = new bool[2];
			bool midline = false;
			int nleft;

			do {
				nleft = inputEnd - inputIndex;
				if (ReadAhead (atleast, 2, cancellationToken) <= 0) {
					boundary = BoundaryType.Eos;
					break;
				}

				int contentIndex = inputIndex;

				ScanContent (inbuf, ref nleft, ref midline, ref formats);

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

		unsafe void ConstructMimePart (MimePart part, MimeEntityEndEventArgs args, byte* inbuf, CancellationToken cancellationToken)
		{
			long endOffset, beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;
			ScanContentResult result;
			Stream content;

			if (persistent) {
				using (var measured = new MeasuringStream ()) {
					result = ScanContent (inbuf, measured, true, cancellationToken);
					endOffset = beginOffset + measured.Length;
				}

				content = new BoundStream (stream, beginOffset, endOffset, true);
			} else {
				content = new MemoryBlockStream ();

				try {
					result = ScanContent (inbuf, content, true, cancellationToken);
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

		unsafe void ConstructMessagePart (MessagePart rfc822, MimeEntityEndEventArgs args, byte* inbuf, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (ReadAhead (atleast, 0, cancellationToken) <= 0) {
					boundary = BoundaryType.Eos;
					return;
				}

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

			// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
			state = MimeParserState.MessageHeaders;
			Step (inbuf, cancellationToken);

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
				ConstructMultipart (multipart, entityArgs, inbuf, depth + 1, cancellationToken);
			else if (entity is MessagePart child)
				ConstructMessagePart (child, entityArgs, inbuf, depth + 1, cancellationToken);
			else
				ConstructMimePart ((MimePart) entity, entityArgs, inbuf, cancellationToken);

			rfc822.Message = message;

			var endOffset = GetEndOffset (inputIndex);
			messageArgs.HeadersEndOffset = entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
			messageArgs.EndOffset = entityArgs.EndOffset = endOffset;

			OnMimeEntityEnd (entityArgs);
			OnMimeMessageEnd (messageArgs);

			args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		unsafe void MultipartScanPreamble (Multipart multipart, byte* inbuf, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				long offset = GetOffset (inputIndex);

				//OnMultipartPreambleBegin (multipart, offset);
				ScanContent (inbuf, memory, false, cancellationToken);
				multipart.RawPreamble = memory.ToArray ();
				//OnMultipartPreambleEnd (multipart, offset + memory.Length);
			}
		}

		unsafe void MultipartScanEpilogue (Multipart multipart, byte* inbuf, CancellationToken cancellationToken)
		{
			using (var memory = new MemoryStream ()) {
				long offset = GetOffset (inputIndex);

				//OnMultipartEpilogueBegin (multipart, offset);
				var result = ScanContent (inbuf, memory, true, cancellationToken);
				multipart.RawEpilogue = result.IsEmpty ? null : memory.ToArray ();
				//OnMultipartEpilogueEnd (multipart, offset + memory.Length);
			}
		}

		unsafe void MultipartScanSubparts (Multipart multipart, byte* inbuf, int depth, CancellationToken cancellationToken)
		{
			//var beginOffset = GetOffset (inputIndex);

			do {
				//OnMultipartBoundaryBegin (multipart, beginOffset);

				// skip over the boundary marker
				if (!SkipLine (inbuf, true, cancellationToken)) {
					//OnMultipartBoundaryEnd (multipart, GetOffset (inputIndex));
					boundary = BoundaryType.Eos;
					return;
				}

				//OnMultipartBoundaryEnd (multipart, GetOffset (inputIndex));

				var beginLineNumber = lineNumber;

				// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
				state = MimeParserState.Headers;
				Step (inbuf, cancellationToken);

				if (state == MimeParserState.Boundary) {
					if (headers.Count == 0) {
						if (boundary == BoundaryType.ImmediateBoundary) {
							// FIXME: Should we add an empty TextPart? If we do, update MimeParserTests.TestDoubleMultipartBoundary()
							//beginOffset = GetOffset (inputIndex);
							continue;
						}
						return;
					}

					// This part has no content, but that will be handled in ConstructMultipart()
					// or ConstructMimePart().
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
					ConstructMultipart (child, entityArgs, inbuf, depth + 1, cancellationToken);
				else if (entity is MessagePart rfc822)
					ConstructMessagePart (rfc822, entityArgs, inbuf, depth + 1, cancellationToken);
				else
					ConstructMimePart ((MimePart) entity, entityArgs, inbuf, cancellationToken);

				var endOffset = GetEndOffset (inputIndex);
				entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
				entityArgs.EndOffset = endOffset;

				OnMimeEntityEnd (entityArgs);

				//beginOffset = endOffset;
				multipart.Add (entity);
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

		unsafe void ConstructMultipart (Multipart multipart, MimeEntityEndEventArgs args, byte* inbuf, int depth, CancellationToken cancellationToken)
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
				MultipartScanPreamble (multipart, inbuf, cancellationToken);

				endOffset = GetEndOffset (inputIndex);
				args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
				return;
			}

			PushBoundary (marker);

			MultipartScanPreamble (multipart, inbuf, cancellationToken);
			if (boundary == BoundaryType.ImmediateBoundary)
				MultipartScanSubparts (multipart, inbuf, depth, cancellationToken);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				//OnMultipartEndBoundaryBegin (multipart, GetEndOffset (inputIndex));

				// consume the end boundary and read the epilogue (if there is one)
				multipart.WriteEndBoundary = true;
				SkipLine (inbuf, false, cancellationToken);
				PopBoundary ();

				//OnMultipartEndBoundaryEnd (multipart, GetOffset (inputIndex));

				MultipartScanEpilogue (multipart, inbuf, cancellationToken);

				endOffset = GetEndOffset (inputIndex);
				args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);
				return;
			}

			endOffset = GetEndOffset (inputIndex);
			args.Lines = GetLineCount (beginLineNumber, beginOffset, endOffset);

			multipart.WriteEndBoundary = false;

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			if (boundary == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (inbuf, true))
				boundary = BoundaryType.ImmediateEndBoundary;
			else if (boundary == BoundaryType.ParentBoundary && FoundImmediateBoundary (inbuf, false))
				boundary = BoundaryType.ImmediateBoundary;
		}

		/// <summary>
		/// This is a hack needed by the MessageDeliveryStatus.ParseStatusGroups() logic in order to work around an Office365 bug.
		/// </summary>
		/// <returns>The remainder of the parser's input stream (needed because the input stream may not be seekable).</returns>
		internal Stream ReadToEos ()
		{
			var content = new MemoryBlockStream ();

			try {
				do {
					if (ReadAhead (1, 0, CancellationToken.None) <= 0)
						break;

					content.Write (input, inputIndex, inputEnd - inputIndex);
					inputIndex = inputEnd;
				} while (!eos);

				content.Position = 0;

				return content;
			} catch {
				content.Dispose ();
				throw;
			}
		}

		unsafe HeaderList ParseHeaders (byte* inbuf, CancellationToken cancellationToken)
		{
			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;

			var parsed = new HeaderList (options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
		}

		/// <summary>
		/// Parse a list of headers from the stream.
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
		public HeaderList ParseHeaders (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					return ParseHeaders (inbuf, cancellationToken);
				}
			}
		}

		unsafe bool IsBlankLine (byte* inbuf, CancellationToken cancellationToken)
		{
			if (ReadAhead (ReadAheadSize, 1, cancellationToken) <= 0)
				return false;

			byte* inptr = inbuf + inputIndex;

			return *inptr == (byte) '\r' || *inptr == (byte) '\n';
		}

		unsafe HeaderList ParseStatusGroup (byte* inbuf, CancellationToken cancellationToken)
		{
			while (IsBlankLine (inbuf, cancellationToken)) {
				if (!SkipLine (inbuf, true, cancellationToken))
					break;
			}

			return ParseHeaders (inbuf, cancellationToken);
		}

		/// <summary>
		/// Parse a single message/delivery-status status group from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a single message/delivery-status status group from the stream.
		/// </remarks>
		/// <returns>The parsed status group.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the status group.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		internal HeaderList ParseStatusGroup (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					return ParseStatusGroup (inbuf, cancellationToken);
				}
			}
		}

		unsafe MimeEntity ParseEntity (byte* inbuf, CancellationToken cancellationToken)
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != position)
				stream.Seek (position, SeekOrigin.Begin);

			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (inbuf, cancellationToken) == MimeParserState.Error)
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
				ConstructMultipart (multipart, entityArgs, inbuf, 0, cancellationToken);
			else if (entity is MessagePart rfc822)
				ConstructMessagePart (rfc822, entityArgs, inbuf, 0, cancellationToken);
			else
				ConstructMimePart ((MimePart) entity, entityArgs, inbuf, cancellationToken);

			var endOffset = GetEndOffset (inputIndex);
			entityArgs.HeadersEndOffset = Math.Min (entityArgs.HeadersEndOffset, endOffset);
			entityArgs.EndOffset = endOffset;

			state = MimeParserState.Eos;

			OnMimeEntityEnd (entityArgs);

			return entity;
		}

		/// <summary>
		/// Parse an entity from the stream.
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
		public MimeEntity ParseEntity (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					return ParseEntity (inbuf, cancellationToken);
				}
			}
		}

		unsafe MimeMessage ParseMessage (byte* inbuf, CancellationToken cancellationToken)
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != position)
				stream.Seek (position, SeekOrigin.Begin);

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (Step (inbuf, cancellationToken)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			toplevel = true;

			// parse the headers
			var beginLineNumber = lineNumber;
			if (state < MimeParserState.Content && Step (inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse message headers.");

			var message = new MimeMessage (options, headers, RfcComplianceMode.Loose);
			var messageArgs = new MimeMessageEndEventArgs (message) {
				HeadersEndOffset = headerBlockEnd,
				BeginOffset = headerBlockBegin,
				LineNumber = beginLineNumber
			};

			OnMimeMessageBegin (messageArgs);

			contentEnd = 0;
			if (format == MimeFormat.Mbox && options.RespectContentLength) {
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
				ConstructMultipart (multipart, entityArgs, inbuf, 0, cancellationToken);
			else if (entity is MessagePart rfc822)
				ConstructMessagePart (rfc822, entityArgs, inbuf, 0, cancellationToken);
			else
				ConstructMimePart ((MimePart) entity, entityArgs, inbuf, cancellationToken);

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

		/// <summary>
		/// Parse a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMessage" />
		/// </example>
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
		public MimeMessage ParseMessage (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					return ParseMessage (inbuf, cancellationToken);
				}
			}
		}

		#region IEnumerable implementation

		/// <summary>
		/// Enumerate the messages in the stream.
		/// </summary>
		/// <remarks>
		/// This is mostly useful when parsing mbox-formatted streams.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<MimeMessage> GetEnumerator ()
		{
			while (!IsEndOfStream)
				yield return ParseMessage ();

			yield break;
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Enumerate the messages in the stream.
		/// </summary>
		/// <remarks>
		/// This is mostly useful when parsing mbox-formatted streams.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}
