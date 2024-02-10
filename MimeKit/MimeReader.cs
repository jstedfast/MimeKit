//
// MimeReader.cs
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A MIME message and entity reader.
	/// </summary>
	/// <remarks>
	/// <para><see cref="MimeReader"/> provides forward-only, read-only access to MIME data in a stream.</para>
	/// </remarks>
	public partial class MimeReader
	{
		enum MimeEntityType
		{
			MimePart,
			MessagePart,
			Multipart
		}

		static ReadOnlySpan<byte> UTF8ByteOrderMark => new byte[] { 0xEF, 0xBB, 0xBF };

		const int HeaderBufferGrowSize = 64;
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 4;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// header buffer
		byte[] headerBuffer = new byte[512];
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

		ParserOptions options;
		internal Stream stream;
		internal long position;

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeReader"/> that will parse the specified stream.
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public MimeReader (Stream stream, MimeFormat format = MimeFormat.Default) : this (ParserOptions.Default, stream, format)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeReader"/> that will parse the specified stream.
		/// </remarks>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public MimeReader (ParserOptions options, Stream stream, MimeFormat format = MimeFormat.Default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			Options = options;

			SetStream (stream, format);
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
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public virtual void SetStream (Stream stream, MimeFormat format = MimeFormat.Default)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			this.format = format;
			this.stream = stream;

			inputIndex = inputStart;
			inputEnd = inputStart;

			headerBlockBegin = 0;
			headerBlockEnd = 0;
			lineNumber = 1;
			contentEnd = 0;

			position = stream.CanSeek ? stream.Position : 0;
			prevLineBeginOffset = position;
			lineBeginOffset = position;
			toplevel = false;
			eos = false;

			bounds.Clear ();
			if (format == MimeFormat.Mbox)
				bounds.Add (Boundary.CreateMboxBoundary ());

			state = MimeParserState.Initialized;
			boundary = BoundaryType.None;
		}

		#region Mbox Events

		/// <summary>
		/// Called when an Mbox marker is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>When the stream is specified to be in <see cref="MimeFormat.Mbox"/> format, this method will be called whenever the parser encounters an Mbox marker.</para>
		/// <para>It is not necessary to override this method unless it is desirable to track the offsets of mbox markers within a stream or to extract the mbox marker itself.</para>
		/// </remarks>
		/// <param name="buffer">The buffer containing the mbox marker.</param>
		/// <param name="startIndex">The index denoting the starting position of the mbox marker within the buffer.</param>
		/// <param name="count">The length of the mbox marker within the buffer, in bytes.</param>
		/// <param name="beginOffset">The offset into the stream where the mbox marker begins.</param>
		/// <param name="lineNumber">The line number where the mbox marker exists within the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when an Mbox marker is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>When the stream is specified to be in <see cref="MimeFormat.Mbox"/> format, this method will be called whenever the parser encounters an Mbox marker.</para>
		/// <para>It is not necessary to override this method unless it is desirable to track the offsets of mbox markers within a stream or to extract the mbox marker itself.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="buffer">The buffer containing the mbox marker.</param>
		/// <param name="startIndex">The index denoting the starting position of the mbox marker within the buffer.</param>
		/// <param name="count">The length of the mbox marker within the buffer, in bytes.</param>
		/// <param name="beginOffset">The offset into the stream where the mbox marker begins.</param>
		/// <param name="lineNumber">The line number where the mbox marker exists within the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMboxMarkerReadAsync (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			OnMboxMarkerRead (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion Mbox Events

		#region Header Events

		/// <summary>
		/// Called when the beginning of a list of headers is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a list of headers is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnHeadersEnd"/> when the end of the list of headers are found.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the headers begin.</param>
		/// <param name="beginLineNumber">The line number where the list of headers begin.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a list of headers is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a list of headers is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnHeadersEndAsync"/> when the end of the list of headers are found.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the headers begin.</param>
		/// <param name="beginLineNumber">The line number where the list of headers begin.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnHeadersBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnHeadersBegin (beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when a message or MIME part header is read from the stream.
		/// </summary>
		/// <remarks>
		/// This method will be called whenever a message or MIME part header is encountered within the stream.
		/// </remarks>
		/// <param name="header">The header that was read from the stream.</param>
		/// <param name="beginLineNumber">The line number where the header exists within the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when a message or MIME part header is read from the stream.
		/// </summary>
		/// <remarks>
		/// This method will be called whenever a message or MIME part header is encountered within the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="header">The header that was read from the stream.</param>
		/// <param name="beginLineNumber">The line number where the header exists within the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnHeaderReadAsync (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnHeaderRead (header, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a list of headers is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a list of headers is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnHeadersBegin"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the headers began.</param>
		/// <param name="beginLineNumber">The line number where the list of headers began.</param>
		/// <param name="endOffset">The offset into the stream where the list of headers ended.</param>
		/// <param name="endLineNumber">The line number headers where the list of headers ended.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a list of headers is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a list of headers is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnHeadersBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the headers began.</param>
		/// <param name="beginLineNumber">The line number where the list of headers began.</param>
		/// <param name="endOffset">The offset into the stream where the list of headers ended.</param>
		/// <param name="endLineNumber">The line number headers where the list of headers ended.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnHeadersEndAsync (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
		{
			OnHeadersEnd (beginOffset, beginLineNumber, endOffset, endLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion Header Events

		#region MimeMessage Events

		/// <summary>
		/// Called when the beginning of a message is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a message is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimeMessageEnd"/> when the end of the message is found.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the message begins.</param>
		/// <param name="beginLineNumber">The line number where the message begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a message is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a message is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimeMessageEndAsync"/> when the end of the message is found.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the message begins.</param>
		/// <param name="beginLineNumber">The line number where the message begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimeMessageBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMimeMessageBegin (beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a message is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a message is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimeMessageBegin"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the message began.</param>
		/// <param name="beginLineNumber">The line number where the message began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the message headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the message ended.</param>
		/// <param name="lines">The length of the message as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a message is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a message is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimeMessageBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the message began.</param>
		/// <param name="beginLineNumber">The line number where the message began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the message headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the message ended.</param>
		/// <param name="lines">The length of the message as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimeMessageEndAsync (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMimeMessageEnd (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion MimeMessage Events

		#region MimePart Events

		/// <summary>
		/// Called when the beginning of a MIME part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a MIME part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartEnd"/> when the end of the MIME part is found.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the MIME part begins.</param>
		/// <param name="beginLineNumber">The line number where the MIME part begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a MIME part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a MIME part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartEndAsync"/> when the end of the MIME part is found.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the MIME part begins.</param>
		/// <param name="beginLineNumber">The line number where the MIME part begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMimePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the beginning of a MIME part's content is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a MIME part's content is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartContentEnd"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the MIME part content began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part content began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a MIME part's content is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a MIME part's content is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartContentEndAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the MIME part content began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part content began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimePartContentBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMimePartContentBegin (beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when MIME part content is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when MIME part content is read from the stream.</para>
		/// </remarks>
		/// <param name="buffer">A buffer containing the MIME part content.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when MIME part content is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when MIME part content is read from the stream.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="buffer">A buffer containing the MIME part content.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimePartContentReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			OnMimePartContentRead (buffer, startIndex, count, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a MIME part's content is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a MIME part's content is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartContentBegin"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the MIME part content began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part content ended.</param>
		/// <param name="lines">The length of the MIME part content as measured in lines.</param>
		/// <param name="newLineFormat">The new-line format of the content, if known.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a MIME part's content is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a MIME part's content is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartContentBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the MIME part content began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part content ended.</param>
		/// <param name="lines">The length of the MIME part content as measured in lines.</param>
		/// <param name="newLineFormat">The new-line format of the content, if known.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimePartContentEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
		{
			OnMimePartContentEnd (beginOffset, beginLineNumber, endOffset, lines, newLineFormat, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a MIME part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a MIME part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartBegin"/>.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the MIME part began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the MIME part headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part ends.</param>
		/// <param name="lines">The length of the MIME part as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a MIME part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a MIME part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMimePartBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the MIME part began.</param>
		/// <param name="beginLineNumber">The line number where the MIME part began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the MIME part headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part ends.</param>
		/// <param name="lines">The length of the MIME part as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMimePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMimePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion MimePart Events

		#region MessagePart Events

		/// <summary>
		/// Called when the beginning of a message part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a message part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMessagePartEnd"/> when the end of the message part is found.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the message part begins.</param>
		/// <param name="beginLineNumber">The line number where the message part begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a message part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a message part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMessagePartEndAsync"/> when the end of the message part is found.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the message part begins.</param>
		/// <param name="beginLineNumber">The line number where the message part begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMessagePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMessagePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a message part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a message part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMessagePartBegin"/>.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the message part began.</param>
		/// <param name="beginLineNumber">The line number where the message part began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the MIME part headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part ends.</param>
		/// <param name="lines">The length of the MIME part as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a message part is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a message part is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMessagePartBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the MIME part.</param>
		/// <param name="beginOffset">The offset into the stream where the message part began.</param>
		/// <param name="beginLineNumber">The line number where the message part began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the MIME part headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the MIME part ends.</param>
		/// <param name="lines">The length of the MIME part as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMessagePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMessagePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion MessagePart Events

		#region Multipart Events

		/// <summary>
		/// Called when the beginning of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEnd"/> when the end of the multipart is found.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the multipart.</param>
		/// <param name="beginOffset">The offset into the stream where the multipart begins.</param>
		/// <param name="beginLineNumber">The line number where the multipart begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEndAsync"/> when the end of the multipart is found.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the multipart.</param>
		/// <param name="beginOffset">The offset into the stream where the multipart begins.</param>
		/// <param name="beginLineNumber">The line number where the multipart begins.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMultipartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when a multipart boundary is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// Called when a multipart boundary is encountered in the stream.
		/// </remarks>
		/// <param name="boundary">The multipart boundary string.</param>
		/// <param name="beginOffset">The offset into the stream where the boundary marker began.</param>
		/// <param name="endOffset">The offset into the stream where the boundary marker ended.</param>
		/// <param name="lineNumber">The line number where the boundary marker was found in the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when a multipart boundary is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// Called when a multipart boundary is encountered in the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="boundary">The multipart boundary string.</param>
		/// <param name="beginOffset">The offset into the stream where the boundary marker began.</param>
		/// <param name="endOffset">The offset into the stream where the boundary marker ended.</param>
		/// <param name="lineNumber">The line number where the boundary marker was found in the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartBoundaryAsync (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
			OnMultipartBoundary (boundary, beginOffset, endOffset, lineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when a multipart end boundary is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// Called when a multipart end boundary is encountered in the stream.
		/// </remarks>
		/// <param name="boundary">The multipart boundary string.</param>
		/// <param name="beginOffset">The offset into the stream where the boundary marker began.</param>
		/// <param name="endOffset">The offset into the stream where the boundary marker ended.</param>
		/// <param name="lineNumber">The line number where the boundary marker was found in the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartEndBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when a multipart end boundary is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// Called when a multipart end boundary is encountered in the stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="boundary">The multipart boundary string.</param>
		/// <param name="beginOffset">The offset into the stream where the boundary marker began.</param>
		/// <param name="endOffset">The offset into the stream where the boundary marker ended.</param>
		/// <param name="lineNumber">The line number where the boundary marker was found in the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartEndBoundaryAsync (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
			OnMultipartEndBoundary (boundary, beginOffset, endOffset, lineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the beginning of the preamble of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of the preamble of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartPreambleEnd"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the preamble began.</param>
		/// <param name="beginLineNumber">The line number where the preamble began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of the preamble of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of the preamble of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartPreambleEndAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the preamble began.</param>
		/// <param name="beginLineNumber">The line number where the preamble began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartPreambleBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMultipartPreambleBegin (beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when multipart preamble text is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when multipart preamble text is read from the stream.</para>
		/// </remarks>
		/// <param name="buffer">A buffer containing the multipart preamble text.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when multipart preamble text is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when multipart preamble text is read from the stream.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="buffer">A buffer containing the multipart preamble text.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartPreambleReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			OnMultipartPreambleRead (buffer, startIndex, count, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of the preamble of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of the preamble of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartPreambleBegin"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the multipart preamble began.</param>
		/// <param name="beginLineNumber">The line number where the multipart preamble began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart preamble ended.</param>
		/// <param name="lines">The length of the multipart preamble as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of the preamble of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of the preamble of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartPreambleBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the multipart preamble began.</param>
		/// <param name="beginLineNumber">The line number where the multipart preamble began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart preamble ended.</param>
		/// <param name="lines">The length of the multipart preamble as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartPreambleEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMultipartPreambleEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the beginning of the epilogue of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of the epilogue of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEpilogueEnd"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the epilogue began.</param>
		/// <param name="beginLineNumber">The line number where the epilogue began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the beginning of the epilogue of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the beginning of the epilogue of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEpilogueEndAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the epilogue began.</param>
		/// <param name="beginLineNumber">The line number where the epilogue began.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartEpilogueBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			OnMultipartEpilogueBegin (beginOffset, beginLineNumber, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when multipart epilogue text is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when multipart epilogue text is read from the stream.</para>
		/// </remarks>
		/// <param name="buffer">A buffer containing the multipart epilogue text.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when multipart epilogue text is read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when multipart epilogue text is read from the stream.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="buffer">A buffer containing the multipart epilogue text.</param>
		/// <param name="startIndex">The index denoting the starting position of the content within the buffer.</param>
		/// <param name="count">The length of the content within the buffer, in bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartEpilogueReadAsync (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			OnMultipartEpilogueRead (buffer, startIndex, count, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of the epilogue of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of the epilogue of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEpilogueBegin"/>.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the multipart epilogue began.</param>
		/// <param name="beginLineNumber">The line number where the multipart epilogue began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart epilogue ended.</param>
		/// <param name="lines">The length of the multipart epilogue as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of the epilogue of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of the epilogue of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartEpilogueBeginAsync"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="beginOffset">The offset into the stream where the multipart epilogue began.</param>
		/// <param name="beginLineNumber">The line number where the multipart epilogue began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart epilogue ended.</param>
		/// <param name="lines">The length of the multipart epilogue as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartEpilogueEndAsync (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMultipartEpilogueEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when the end of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartBegin"/>.</para>
		/// </remarks>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the multipart.</param>
		/// <param name="beginOffset">The offset into the stream where the multipart began.</param>
		/// <param name="beginLineNumber">The line number where the multipart began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the multipart headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart ends.</param>
		/// <param name="lines">The length of the multipart as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
		}

		/// <summary>
		/// Called when the end of a multipart is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>Called when the end of a multipart is encountered in the stream.</para>
		/// <para>This method is always paired with a corresponding call to <see cref="OnMultipartBegin"/>.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="contentType">The parsed <c>Content-Type</c> header of the multipart.</param>
		/// <param name="beginOffset">The offset into the stream where the multipart began.</param>
		/// <param name="beginLineNumber">The line number where the multipart began.</param>
		/// <param name="headersEndOffset">The offset into the stream where the multipart headers ended and the content began.</param>
		/// <param name="endOffset">The offset into the stream where the multipart ends.</param>
		/// <param name="lines">The length of the multipart as measured in lines.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual Task OnMultipartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			OnMultipartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			return Task.CompletedTask;
		}

		#endregion Multipart Events

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static int NextAllocSize (int need)
		{
			return (need + 63) & ~63;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void IncrementLineNumber (int index)
		{
			prevLineBeginOffset = lineBeginOffset;
			lineBeginOffset = GetOffset (index);
			lineNumber++;
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

		static unsafe bool IsMboxMarker (byte[] text, bool allowMunged = false)
		{
			fixed (byte* buf = text) {
				return IsMboxMarker (buf, allowMunged);
			}
		}

		unsafe bool StepMboxMarker (byte* inbuf, ref int left, out int mboxMarkerIndex, out int mboxMarkerLength, out long mboxMarkerOffset)
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
					mboxMarkerOffset = 0;
					mboxMarkerLength = 0;
					mboxMarkerIndex = 0;
					return false;
				}

				var markerLength = (int) (inptr - start);

				if (inptr > start && *(inptr - 1) == (byte) '\r')
					markerLength--;

				// consume the '\n'
				inptr++;

				var lineLength = (int) (inptr - start);

				inputIndex += lineLength;
				IncrementLineNumber (inputIndex);

				if (markerLength >= 5 && IsMboxMarker (start)) {
					mboxMarkerOffset = GetOffset (startIndex);
					mboxMarkerLength = markerLength;
					mboxMarkerIndex = startIndex;
					return true;
				}
			}

			mboxMarkerOffset = 0;
			mboxMarkerLength = 0;
			mboxMarkerIndex = 0;
			left = 0;

			return false;
		}

		unsafe void StepMboxMarker (byte* inbuf, CancellationToken cancellationToken)
		{
			int mboxMarkerIndex, mboxMarkerLength;
			long mboxMarkerOffset;
			bool complete;
			int left = 0;

			do {
				var available = ReadAhead (Math.Max (ReadAheadSize, left), 0, cancellationToken);

				if (available <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				complete = StepMboxMarker (inbuf, ref left, out mboxMarkerIndex, out mboxMarkerLength, out mboxMarkerOffset);
			} while (!complete);

			OnMboxMarkerRead (input, mboxMarkerIndex, mboxMarkerLength, mboxMarkerOffset, lineNumber - 1, cancellationToken);

			state = MimeParserState.MessageHeaders;
		}

		void UpdateHeaderState (Header header)
		{
			var rawValue = header.RawValue;
			int index = 0;

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
				if (currentContentType is null) {
					// FIXME: do we really need all this fallback stuff for parameters? I doubt it.
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

					currentContentType = type;
				}
				break;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsControl (byte c)
		{
			return c.IsCtrl ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsBlank (byte c)
		{
			return c.IsBlank ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void EnsureHeaderBufferSize (int size)
		{
			if (size >= headerBuffer.Length)
				Array.Resize (ref headerBuffer, NextAllocSize (size));
		}

		unsafe bool TryDetectInvalidHeader (byte* inbuf, out bool invalid, out int fieldNameLength, out int headerFieldLength)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			byte* start = inptr;
			bool blanks = false;

			*inend = (byte) ':';

			fieldNameLength = 0;

			while (*inptr != (byte) ':') {
				// Blank spaces are allowed between the field name and the ':', but field names themselves are not allowed to contain spaces.
				if (IsBlank (*inptr)) {
					if (fieldNameLength == 0)
						fieldNameLength = (int) (inptr - start);
					blanks = true;
				} else if (blanks || IsControl (*inptr)) {
					headerFieldLength = (int) (inptr - start);
					invalid = true;
					return true;
				}

				inptr++;
			}

			headerFieldLength = (int) (inptr - start);

			if (fieldNameLength == 0)
				fieldNameLength = headerFieldLength;

			invalid = false;

			return inptr < inend;
		}

		void StepHeaderField (int headerFieldLength)
		{
			headerIndex = headerFieldLength + 1;

			EnsureHeaderBufferSize (headerIndex);
			Buffer.BlockCopy (input, inputIndex, headerBuffer, 0, headerIndex);

			// Save input buffer state.
			inputIndex += headerIndex;
		}

		unsafe bool StepHeaderValue (byte* inbuf, ref bool midline)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			int index = inputIndex;
			int nread;

			*inend = (byte) '\n';

#if false
			if (midline) {
				while (*inptr != (byte) '\n')
					inptr++;

				if (inptr < inend) {
					// Consume the newline and update our parse state.
					inptr++;

					index = (int) (inptr - inbuf);
					IncrementLineNumber (index);

					midline = false;
				} else {
					// We've reached the end of the input buffer.
				}
			}
#endif

			while (inptr < inend && (midline || IsBlank (*inptr))) {
#if FUNROLL_LOOPS
				// Note: we can always depend on byte[] arrays being 4-byte aligned on 32bit and 64bit architectures
				int alignment = (index + 3) & ~3;
				byte* aligned = inbuf + alignment;
				byte c = *aligned;

				*aligned = (byte) '\n';
				while (*inptr != (byte) '\n')
					inptr++;
				*aligned = c;

				if (inptr == aligned && c != (byte) '\n') {
					// -funroll-loops, yippee ki-yay.
					uint* dword = (uint*) inptr;
					uint mask;

					do {
						mask = *dword++ ^ 0x0A0A0A0A;
						mask = ((mask - 0x01010101) & (~mask & 0x80808080));
					} while (mask == 0);

					inptr = (byte*) (dword - 1);
					while (*inptr != (byte) '\n')
						inptr++;
				}
#else
				while (*inptr != (byte) '\n')
					inptr++;
#endif

				if (inptr == inend) {
					// We've reached the end of the input buffer and we are currently in the middle of a line.
					midline = true;
					break;
				}

				// Consume the newline and update our line number state.
				inptr++;

				index = (int) (inptr - inbuf);
				IncrementLineNumber (index);
				midline = false;
			}

			// At this point, we either reached the end of the buffer or we reached the end of the header value.

			// Calculate the new inputIndex and the amount of input we've read.
			index = (int) (inptr - inbuf);
			nread = index - inputIndex;

			// Blit the input data that we've processed into the headerBuffer.
			EnsureHeaderBufferSize (headerIndex + nread);
			Buffer.BlockCopy (input, inputIndex, headerBuffer, headerIndex, nread);
			headerIndex += nread;
			inputIndex = index;

			// If inptr == inend, then our caller will re-fill the input buffer and call us again.
			return inptr < inend;
		}

		bool IsEndOfHeaderBlock (int left)
		{
			if (input[inputIndex] == (byte) '\n') {
				state = MimeParserState.Content;
				inputIndex++;
				IncrementLineNumber (inputIndex);
				return true;
			}

			if (input[inputIndex] == (byte) '\r' && (left == 1 || input[inputIndex + 1] == (byte) '\n')) {
				state = MimeParserState.Content;
				if (left > 1)
					inputIndex += 2;
				else
					inputIndex++;
				IncrementLineNumber (inputIndex);
				return true;
			}

			return false;
		}

		unsafe bool TryCheckBoundaryWithinHeaderBlock (byte* inbuf)
		{
			byte* inptr = inbuf + inputIndex;
			byte* inend = inbuf + inputEnd;
			byte* start = inptr;

			*inend = (byte) '\n';

			while (*inptr != (byte) '\n')
				inptr++;

			if (inptr == inend)
				return false;

			int length = (int) (inptr - start);

			if ((boundary = CheckBoundary (inputIndex, start, length)) != BoundaryType.None)
				state = MimeParserState.Boundary;

			return true;
		}

		unsafe bool TryCheckMboxMarkerWithinHeaderBlock (byte* inbuf)
		{
			byte* inptr = inbuf + inputIndex;
			int need = *inptr == '>' ? 6 : 5;

			if (inputEnd - inputIndex < need)
				return false;

			if (format == MimeFormat.Mbox && inputIndex >= contentEnd && IsMboxMarker (inptr)) {
				state = MimeParserState.Complete;
				return true;
			}

			if (headerCount == 0) {
				if (state == MimeParserState.MessageHeaders) {
					// Ignore (munged) From-lines that might appear at the start of a message.
					if (toplevel && !IsMboxMarker (inptr, true)) {
						// This line was not a (munged) mbox From-line.
						state = MimeParserState.Error;
						return true;
					}
				} else if (toplevel && state == MimeParserState.Headers) {
					state = MimeParserState.Error;
					return true;
				}
			}

			// At this point, it may still be what looks like a From-line, but we'll treat it as an invalid header.

			return true;
		}

		Header CreateHeader (long beginOffset, int fieldNameLength, int headerFieldLength, bool invalid)
		{
			byte[] field, value;

			if (invalid) {
				field = new byte[headerIndex];
				Buffer.BlockCopy (headerBuffer, 0, field, 0, headerIndex);
				fieldNameLength = headerIndex;
				value = Array.Empty<byte> ();
			} else {
				field = new byte[headerFieldLength];
				Buffer.BlockCopy (headerBuffer, 0, field, 0, headerFieldLength);
				value = new byte[headerIndex - (headerFieldLength + 1)];
				Buffer.BlockCopy (headerBuffer, headerFieldLength + 1, value, 0, value.Length);
			}

			var header = new Header (options, field, fieldNameLength, value, invalid) {
				Offset = beginOffset
			};

			UpdateHeaderState (header);
			headerCount++;

			return header;
		}

		unsafe void StepHeaders (byte* inbuf, CancellationToken cancellationToken)
		{
			int headersBeginLineNumber = lineNumber;

			headerBlockBegin = GetOffset (inputIndex);
			boundary = BoundaryType.None;
			//preHeaderLength = 0;
			headerCount = 0;

			currentContentLength = null;
			currentContentType = null;
			currentEncoding = null;

			OnHeadersBegin (headerBlockBegin, headersBeginLineNumber, cancellationToken);

			ReadAhead (ReadAheadSize, 0, cancellationToken);

			do {
				var beginOffset = GetOffset (inputIndex);
				var beginLineNumber = lineNumber;
				int left = inputEnd - inputIndex;
				int headerFieldLength;
				int fieldNameLength;
				bool invalid;

				headerIndex = 0;

				if (left < 2)
					left = ReadAhead (2, 0, cancellationToken);

				if (left == 0) {
					state = MimeParserState.Content;
					break;
				}

				// Check for an empty line denoting the end of the header block.
				if (IsEndOfHeaderBlock (left))
					break;

				// Scan ahead a bit to see if this looks like an invalid header.
				while (!TryDetectInvalidHeader (inbuf, out invalid, out fieldNameLength, out headerFieldLength)) {
					int atleast = (inputEnd - inputIndex) + 1;

					if (ReadAhead (atleast, 0, cancellationToken) < atleast) {
						// Not enough input to even find the ':'... mark as invalid and continue?
						invalid = true;
						break;
					}
				}

				if (invalid) {
					// Figure out why this is an invalid header.

					if (input[inputIndex] == (byte) '-') {
						// Check for a boundary marker. If the message is properly formatted, this will NEVER happen.
						while (!TryCheckBoundaryWithinHeaderBlock (inbuf)) {
							int atleast = (inputEnd - inputIndex) + 1;

							if (ReadAhead (atleast, 0, cancellationToken) < atleast)
								break;
						}
					} else if (input[inputIndex] == (byte) 'F' || input[inputIndex] == (byte) '>') {
						// Check for an mbox-style From-line. Again, if the message is properly formatted and not truncated, this will NEVER happen.
						while (!TryCheckMboxMarkerWithinHeaderBlock (inbuf)) {
							int atleast = (inputEnd - inputIndex) + 1;

							if (ReadAhead (atleast, 0, cancellationToken) < atleast)
								break;
						}
					}

					if (state != MimeParserState.MessageHeaders && state != MimeParserState.Headers)
						break;

					if (toplevel && eos && inputIndex + headerFieldLength >= inputEnd) {
						state = MimeParserState.Error;
						return;
					}

					// Fall through and act is if we're consuming a header.
				} else {
					// Consume the header field name.
					StepHeaderField (headerFieldLength);
				}

				bool midline = true;

				// Consume the header value.
				while (!StepHeaderValue (inbuf, ref midline)) {
					if (ReadAhead (1, 0, cancellationToken) == 0)
						break;
				}

				if (toplevel && headerCount == 0 && invalid && !IsMboxMarker (headerBuffer)) {
					state = MimeParserState.Error;
					return;
				}

				var header = CreateHeader (beginOffset, fieldNameLength, headerFieldLength, invalid);

				OnHeaderRead (header, beginLineNumber, cancellationToken);
			} while (true);

			headerBlockEnd = GetOffset (inputIndex);

			OnHeadersEnd (headerBlockBegin, headersBeginLineNumber, headerBlockEnd, lineNumber, cancellationToken);
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
					IncrementLineNumber (inputIndex);
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
			if (currentContentType != null)
				return currentContentType;

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

					length++;
					inptr++;

					IncrementLineNumber ((int) (inptr - inbuf));
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
			var formats = new bool[2];
			int contentLength = 0;
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
			} while (boundary == BoundaryType.None);

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
			OnMimePartContentEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, result.Format, cancellationToken);

			return result.Lines;
		}

		unsafe int ConstructMessagePart (byte* inbuf, int depth, CancellationToken cancellationToken)
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

				// Note: This isn't obvious, but if the "boundary" that was found is an Mbox "From " line, then
				// either the current stream offset is >= contentEnd -or- RespectContentLength is false. It will
				// *never* be an Mbox "From " marker in Entity mode.
				if ((boundary = CheckBoundary (inputIndex, start, (int) (inptr - start))) != BoundaryType.None)
					return GetLineCount (beginLineNumber, beginOffset, GetEndOffset (inputIndex));
			}

			// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
			state = MimeParserState.MessageHeaders;
			Step (inbuf, cancellationToken);

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			OnMimeMessageBegin (currentBeginOffset, beginLineNumber, cancellationToken);

			//if (preHeaderLength > 0) {
				// FIXME: how to solve this?
				//message.MboxMarker = new byte[preHeaderLength];
				//Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			//}

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (depth < options.MaxMimeDepth && IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (type, inbuf, depth + 1, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (inbuf, depth + 1, cancellationToken);
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

		unsafe void MultipartScanSubparts (ContentType multipartContentType, byte* inbuf, int depth, CancellationToken cancellationToken)
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

				// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
				state = MimeParserState.Headers;
				Step (inbuf, cancellationToken);

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
					lines = ConstructMultipart (type, inbuf, depth + 1, cancellationToken);
					entityType = MimeEntityType.Multipart;
				} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
					OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMessagePart (inbuf, depth + 1, cancellationToken);
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

		unsafe int ConstructMultipart (ContentType contentType, byte* inbuf, int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var marker = contentType.Boundary;
			var beginLineNumber = lineNumber;
			long endOffset;

			if (marker is null) {
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
				MultipartScanSubparts (contentType, inbuf, depth, cancellationToken);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				var boundaryOffset = GetOffset (inputIndex);
				var boundaryLineNumber = lineNumber;

				SkipLine (inbuf, false, cancellationToken);

				OnMultipartEndBoundary (marker, boundaryOffset, GetOffset (inputIndex), boundaryLineNumber, cancellationToken);

				PopBoundary ();

				MultipartScanEpilogue (inbuf, cancellationToken);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			if (boundary == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (inbuf, true))
				boundary = BoundaryType.ImmediateEndBoundary;
			else if (boundary == BoundaryType.ParentBoundary && FoundImmediateBoundary (inbuf, false))
				boundary = BoundaryType.ImmediateBoundary;

			endOffset = GetEndOffset (inputIndex);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
		}

		unsafe void ReadHeaders (byte* inbuf, CancellationToken cancellationToken)
		{
			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;
		}

		/// <summary>
		/// Read a block of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Reads a block of headers from the stream.
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
		public void ReadHeaders (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					ReadHeaders (inbuf, cancellationToken);
				}
			}
		}

		unsafe void ReadEntity (byte* inbuf, CancellationToken cancellationToken)
		{
			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (inbuf, cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse entity headers.");

			var type = GetContentType (null);
			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (type, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (inbuf, 0, cancellationToken);
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

			state = MimeParserState.Eos;
		}

		/// <summary>
		/// Read an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Reads an entity from the stream.
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
		public void ReadEntity (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					ReadEntity (inbuf, cancellationToken);
				}
			}
		}

		unsafe void ReadMessage (byte* inbuf, CancellationToken cancellationToken)
		{
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
				lines = ConstructMultipart (type, inbuf, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (inbuf, 0, cancellationToken);
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
		/// Read a message from the stream.
		/// </summary>
		/// <remarks>
		/// Reads a message from the stream.
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
		public void ReadMessage (CancellationToken cancellationToken = default)
		{
			unsafe {
				fixed (byte* inbuf = input) {
					ReadMessage (inbuf, cancellationToken);
				}
			}
		}
	}
}
