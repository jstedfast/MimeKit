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
using System.Runtime.InteropServices;
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

		static ReadOnlySpan<byte> FromMarker => new byte[] { (byte) 'F', (byte) 'r', (byte) 'o', (byte) 'm', (byte) ' ' };
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
		/// <paramref name="stream"/> is <see langword="null"/>.
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
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <see langword="null"/>.</para>
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
		/// <paramref name="stream"/> is <see langword="null"/>.
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
		/// <param name="beginOffset">The offset into the stream where the mbox marker begins.</param>
		/// <param name="lineNumber">The line number where the mbox marker exists within the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
		}

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

		/// <summary>
		/// Called when the end of an Mbox marker is encountered in the stream.
		/// </summary>
		/// <remarks>
		/// <para>When the stream is specified to be in <see cref="MimeFormat.Mbox"/> format, this method will be called whenever the parser encounters the end of an Mbox marker.</para>
		/// <para>It is not necessary to override this method unless it is desirable to track the offsets of mbox markers within a stream or to extract the mbox marker itself.</para>
		/// </remarks>
		/// <param name="beginOffset">The offset into the stream where the mbox marker begins.</param>
		/// <param name="lineNumber">The line number where the mbox marker exists within the stream.</param>
		/// <param name="endOffset">The offset into the stream where the mbox marker ends.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected virtual void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
		{
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

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static int FastIndexOf (ReadOnlySpan<byte> span, byte value)
		{
#if NET8_0_OR_GREATER
			return span.IndexOf (value);
#else
			unsafe {
				fixed (byte* start = &MemoryMarshal.GetReference (span)) {
					byte* inptr = start;

					while (*inptr != (byte) '\n')
						inptr++;

					return (int) (inptr - start);
				}
			}
#endif
		}

		bool StepByteOrderMark (ref int bomIndex)
		{
			while (inputIndex < inputEnd && bomIndex < UTF8ByteOrderMark.Length && input[inputIndex] == UTF8ByteOrderMark[bomIndex]) {
				inputIndex++;
				bomIndex++;
			}

			return bomIndex == 0 || bomIndex == UTF8ByteOrderMark.Length;
		}

		bool StepByteOrderMark (CancellationToken cancellationToken)
		{
			int bomIndex = 0;
			bool complete;

			do {
				var available = ReadAhead (ReadAheadSize, 0, cancellationToken);

				if (available <= 0) {
					// failed to read any data... EOF
					inputIndex = inputEnd;
					return false;
				}

				complete = StepByteOrderMark (ref bomIndex);
			} while (!complete && inputIndex == inputEnd);

			return complete;
		}

		static bool IsMboxMarker (ReadOnlySpan<byte> line, bool allowMunged = false)
		{
			int startIndex = allowMunged && line[0] == (byte) '>' ? 1 : 0;

			if (line.Length < FromMarker.Length + startIndex)
				return false;

			var slice = line.Slice (startIndex, FromMarker.Length);

			return slice.SequenceEqual (FromMarker);
		}

		static bool IsMboxMarker (byte[] buffer, int startIndex = 0, bool allowMunged = false)
		{
			return IsMboxMarker (buffer.AsSpan (startIndex), allowMunged);
		}

		bool StepMboxMarkerStart (ref bool midline)
		{
			var span = input.AsSpan ();
			int index = inputIndex;

			input[inputEnd] = (byte) '\n';

			if (midline) {
				// we're in the middle of a line, so we need to scan for the end of the line
				index = FastIndexOf (span.Slice (index), (byte) '\n') + index;

				if (index == inputEnd) {
					// we don't have enough input data
					inputIndex = inputEnd;
					return false;
				}

				// consume the '\n'
				index++;
				inputIndex = index;
				IncrementLineNumber (inputIndex);
				midline = false;
			}

			while (index + 5 <= inputEnd) {
				if (IsMboxMarker (input, index)) {
					// we have found the start of the mbox marker
					return true;
				}

				// scan for the end of the line
				index = FastIndexOf (span.Slice (index), (byte) '\n') + index;

				if (index == inputEnd) {
					// we don't have enough data to check for a From line
					inputIndex = index;
					midline = true;
					break;
				}

				// consume the '\n'
				index++;
				inputIndex = index;
				IncrementLineNumber (inputIndex);
			}

			return false;
		}

		bool StepMboxMarker (out int count)
		{
			input[inputEnd] = (byte) '\n';

			// scan for the end of the line
			count = FastIndexOf (input.AsSpan (inputIndex), (byte) '\n');

			int index = inputIndex + count;

			// make sure not to consume the '\r' if it exists
			if (count > 0 && input[index - 1] == (byte) '\r')
				count--;

			if (index == inputEnd) {
				// we've only consumed a partial mbox marker
				inputIndex += count;
				return false;
			}

			// consume the '\n'
			index++;

			inputIndex = index;
			IncrementLineNumber (inputIndex);

			return true;
		}

		void StepMboxMarker (CancellationToken cancellationToken)
		{
			bool midline = false;
			bool complete;

			// consume data until we find a line that begins with "From "
			do {
				var available = ReadAhead (5, 0, cancellationToken);

				if (available < 5) {
					// failed to find the beginning of the mbox marker; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return;
				}

				complete = StepMboxMarkerStart (ref midline);
			} while (!complete);

			var mboxMarkerOffset = GetOffset (inputIndex);
			var mboxMarkerLineNumber = lineNumber;

			OnMboxMarkerBegin (mboxMarkerOffset, mboxMarkerLineNumber, cancellationToken);

			do {
				if (ReadAhead (ReadAheadSize, 0, cancellationToken) < 1) {
					// failed to find the end of the mbox marker; EOF reached
					state = MimeParserState.Error;
					return;
				}

				int startIndex = inputIndex;

				complete = StepMboxMarker (out int count);

				// TODO: Remove beginOffset and lineNumber arguments from OnMboxMarkerRead() in v5.0
				OnMboxMarkerRead (input, startIndex, count, mboxMarkerOffset, mboxMarkerLineNumber, cancellationToken);
			} while (!complete);

			OnMboxMarkerEnd (mboxMarkerOffset, mboxMarkerLineNumber, GetOffset (inputIndex), cancellationToken);

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

		bool TryDetectInvalidHeader (out bool invalid, out int fieldNameLength, out int headerFieldLength)
		{
			int index = inputIndex;
			bool blanks = false;

			input[inputEnd] = (byte) ':';

			fieldNameLength = 0;

			while (input[index] != (byte) ':') {
				// Blank spaces are allowed between the field name and the ':', but field names themselves are not allowed to contain spaces.
				if (IsBlank (input[index])) {
					if (fieldNameLength == 0)
						fieldNameLength = index - inputIndex;
					blanks = true;
				} else if (blanks || IsControl (input[index])) {
					headerFieldLength = index - inputIndex;
					invalid = true;
					return true;
				}

				index++;
			}

			headerFieldLength = index - inputIndex;

			if (fieldNameLength == 0)
				fieldNameLength = headerFieldLength;

			invalid = false;

			return index < inputEnd;
		}

		void StepHeaderField (int headerFieldLength)
		{
			headerIndex = headerFieldLength + 1;

			EnsureHeaderBufferSize (headerIndex);
			Buffer.BlockCopy (input, inputIndex, headerBuffer, 0, headerIndex);

			// Save input buffer state.
			inputIndex += headerIndex;
		}

		bool StepHeaderValue (ref bool midline)
		{
			int index = inputIndex;
			int nread;

			input[inputEnd] = (byte) '\n';

			while (index < inputEnd && (midline || IsBlank (input[index]))) {
				int count = FastIndexOf (input.AsSpan (index), (byte) '\n');

				index += count;

				if (index == inputEnd) {
					// We've reached the end of the input buffer, and we are currently in the middle of a line.
					midline = true;
					break;
				}

				// Consume the newline and update our line number state.
				index++;

				IncrementLineNumber (index);
				midline = false;
			}

			// At this point, we either reached the end of the buffer or we reached the end of the header value.

			// Calculate the amount of input we've read.
			nread = index - inputIndex;

			// Blit the input data that we've processed into the headerBuffer.
			EnsureHeaderBufferSize (headerIndex + nread);
			Buffer.BlockCopy (input, inputIndex, headerBuffer, headerIndex, nread);
			headerIndex += nread;
			inputIndex = index;

			// If index == inputEnd, then our caller will re-fill the input buffer and call us again.
			return index < inputEnd;
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

		bool TryCheckBoundaryWithinHeaderBlock ()
		{
			input[inputEnd] = (byte) '\n';

			var span = input.AsSpan (inputIndex);
			int length = FastIndexOf (span, (byte) '\n');

			if (inputIndex + length == inputEnd)
				return false;

			var line = span.Slice (0, length);

			if ((boundary = CheckBoundary (inputIndex, line)) != BoundaryType.None)
				state = MimeParserState.Boundary;

			return true;
		}

		bool TryCheckMboxMarkerWithinHeaderBlock ()
		{
			int need = input[inputIndex] == (byte) '>' ? 6 : 5;

			if (inputEnd - inputIndex < need)
				return false;

			if (format == MimeFormat.Mbox && inputIndex >= contentEnd && IsMboxMarker (input, inputIndex)) {
				state = MimeParserState.Complete;
				return true;
			}

			if (headerCount == 0) {
				if (state == MimeParserState.MessageHeaders) {
					// Ignore (munged) From-lines that might appear at the start of a message.
					if (toplevel && !IsMboxMarker (input, inputIndex, true)) {
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

		void StepHeaders (CancellationToken cancellationToken)
		{
			int headersBeginLineNumber = lineNumber;
			var eof = false;

			headerBlockBegin = GetOffset (inputIndex);
			boundary = BoundaryType.None;
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
					// Note: The only way to get here is if this is the first-pass throgh this loop and we're at EOF, so headerCount should ALWAYS be 0.

					if (toplevel && headerCount == 0) {
						// EOF has been reached before any headers have been parsed for Parse[Headers,Entity,Message].
						state = MimeParserState.Eos;
						return;
					}

					// Note: This can happen if a message is truncated immediately after a boundary marker (e.g. where subpart headers would begin).
					state = MimeParserState.Content;
					break;
				}

				// Check for an empty line denoting the end of the header block.
				if (IsEndOfHeaderBlock (left)) {
					state = MimeParserState.Content;
					break;
				}

				// Scan ahead a bit to see if this looks like an invalid header.
				while (!TryDetectInvalidHeader (out invalid, out fieldNameLength, out headerFieldLength)) {
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
						while (!TryCheckBoundaryWithinHeaderBlock ()) {
							int atleast = (inputEnd - inputIndex) + 1;

							if (ReadAhead (atleast, 0, cancellationToken) < atleast)
								break;
						}

						// Note: If a boundary was discovered, then the state will be updated to MimeParserState.Boundary.
						if (state == MimeParserState.Boundary)
							break;

						// Fall through and act as if we're consuming a header.
					} else if (input[inputIndex] == (byte) 'F' || input[inputIndex] == (byte) '>') {
						// Check for an mbox-style From-line. Again, if the message is properly formatted and not truncated, this will NEVER happen.
						while (!TryCheckMboxMarkerWithinHeaderBlock ()) {
							int atleast = (inputEnd - inputIndex) + 1;

							if (ReadAhead (atleast, 0, cancellationToken) < atleast)
								break;
						}

						// state will be one of the following values:
						// 1. Complete: This means that we've found an actual mbox marker
						// 2. Error: Invalid *first* header and it was not a valid mbox marker
						// 3. MessageHeaders or Headers: let it fall through and treat it as an invalid headers
						if (state != MimeParserState.MessageHeaders && state != MimeParserState.Headers)
							break;

						// Fall through and act as if we're consuming a header.
					} else {
						// Fall through and act as if we're consuming a header.
					}

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
				while (!StepHeaderValue (ref midline)) {
					if (ReadAhead (1, 0, cancellationToken) == 0) {
						state = MimeParserState.Content;
						eof = true;
						break;
					}
				}

				if (toplevel && headerCount == 0 && invalid && !IsMboxMarker (headerBuffer)) {
					state = MimeParserState.Error;
					return;
				}

				var header = CreateHeader (beginOffset, fieldNameLength, headerFieldLength, invalid);

				OnHeaderRead (header, beginLineNumber, cancellationToken);
			} while (!eof);

			headerBlockEnd = GetOffset (inputIndex);

			OnHeadersEnd (headerBlockBegin, headersBeginLineNumber, headerBlockEnd, lineNumber, cancellationToken);
		}

		bool InnerSkipLine (bool consumeNewLine)
		{
			input[inputEnd] = (byte) '\n';

			int index = FastIndexOf (input.AsSpan (inputIndex), (byte) '\n') + inputIndex;

			if (index < inputEnd) {
				inputIndex = index;

				if (consumeNewLine) {
					inputIndex++;
					IncrementLineNumber (inputIndex);
				} else if (input[index - 1] == (byte) '\r') {
					inputIndex--;
				}

				return true;
			}

			inputIndex = inputEnd;

			return false;
		}

		bool SkipLine (bool consumeNewLine, CancellationToken cancellationToken)
		{
			do {
				if (InnerSkipLine (consumeNewLine))
					return true;

				if (ReadAhead (ReadAheadSize, 1, cancellationToken) <= 0)
					return false;
			} while (true);
		}

		MimeParserState Step (CancellationToken cancellationToken)
		{
			switch (state) {
			case MimeParserState.Initialized:
				if (!StepByteOrderMark (cancellationToken)) {
					state = MimeParserState.Eos;
					break;
				}

				state = format == MimeFormat.Mbox ? MimeParserState.MboxMarker : MimeParserState.MessageHeaders;
				break;
			case MimeParserState.MboxMarker:
				StepMboxMarker (cancellationToken);
				break;
			case MimeParserState.MessageHeaders:
			case MimeParserState.Headers:
				StepHeaders (cancellationToken);
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

		bool IsPossibleBoundary (ReadOnlySpan<byte> line)
		{
			if (line.Length < 2)
				return false;

			if (line[0] == (byte) '-' && line[1] == (byte) '-')
				return true;

			if (format == MimeFormat.Mbox && IsMboxMarker (line))
				return true;

			return false;
		}

		static bool IsBoundary (ReadOnlySpan<byte> line, byte[] boundary, int boundaryLength)
		{
			if (boundaryLength > line.Length)
				return false;

			var marker = boundary.AsSpan (0, boundaryLength);
			var slice = line.Slice (0, boundaryLength);

			// make sure that the text matches the boundary
			if (!slice.SequenceEqual (marker))
				return false;

			// if this is an mbox marker, we're done
			if (IsMboxMarker (line))
				return true;

			// the boundary may optionally be followed by lwsp
			int index = boundaryLength;
			int endIndex = line.Length;

			while (index < endIndex) {
				if (!line[index].IsWhitespace ())
					return false;

				index++;
			}

			return true;
		}

		BoundaryType CheckBoundary (int startIndex, ReadOnlySpan<byte> line)
		{
			int count = bounds.Count;

			if (!IsPossibleBoundary (line))
				return BoundaryType.None;

			if (contentEnd > 0) {
				// We'll need to special-case checking for the mbox From-marker when respecting Content-Length
				count--;
			}

			for (int i = 0; i < count; i++) {
				var boundary = bounds[i];

				if (IsBoundary (line, boundary.Marker, boundary.FinalLength))
					return i == 0 ? BoundaryType.ImmediateEndBoundary : BoundaryType.ParentEndBoundary;

				if (IsBoundary (line, boundary.Marker, boundary.Length))
					return i == 0 ? BoundaryType.ImmediateBoundary : BoundaryType.ParentBoundary;
			}

			if (contentEnd > 0) {
				// now it is time to check the mbox From-marker for the Content-Length case
				long curOffset = GetOffset (startIndex);
				var boundary = bounds[count];

				if (curOffset >= contentEnd && IsBoundary (line, boundary.Marker, boundary.Length))
					return BoundaryType.ImmediateEndBoundary;
			}

			return BoundaryType.None;
		}

		BoundaryType CheckBoundary ()
		{
			input[inputEnd] = (byte) '\n';

			var span = input.AsSpan (inputIndex);
			int length = FastIndexOf (span, (byte) '\n');
			var line = span.Slice (0, length);

			return CheckBoundary (inputIndex, line);
		}

		bool FoundImmediateBoundary (bool final)
		{
			int boundaryLength = final ? bounds[0].FinalLength : bounds[0].Length;

			input[inputEnd] = (byte) '\n';

			var span = input.AsSpan (inputIndex);
			int length = FastIndexOf (span, (byte) '\n');
			var line = span.Slice (0, length);

			return IsBoundary (line, bounds[0].Marker, boundaryLength);
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

			return contentType.IsMimeType ("text", "rfc822-headers");
		}

#if NET8_0_OR_GREATER
		void ScanContent (ref int nleft, ref bool midline, ref bool[] formats)
		{
			int length = inputEnd - inputIndex;
			int startIndex = inputIndex;
			int index = inputIndex;

			if (midline && length == nleft)
				boundary = BoundaryType.Eos;

			input[inputEnd] = (byte) '\n';

			while (index < inputEnd) {
				var span = input.AsSpan (index);

				length = span.IndexOf ((byte) '\n');
				index += length;

				if (index < inputEnd) {
					var line = span.Slice (0, length);

					if ((boundary = CheckBoundary (startIndex, line)) != BoundaryType.None)
						break;

					if (length > 0 && input[index - 1] == (byte) '\r')
						formats[(int) NewLineFormat.Dos] = true;
					else
						formats[(int) NewLineFormat.Unix] = true;

					// consume the '\n'
					index++;

					IncrementLineNumber (index);
				} else {
					// didn't find the end of the line...
					midline = true;

					if (boundary == BoundaryType.None) {
						// not enough to tell if we found a boundary
						break;
					}

					var line = span.Slice (0, length);

					if ((boundary = CheckBoundary (startIndex, line)) != BoundaryType.None)
						break;
				}

				startIndex = index;
			}

			inputIndex = startIndex;
		}
#else
		unsafe void ScanContent (ref int nleft, ref bool midline, ref bool[] formats)
		{
			fixed (byte* inbuf = input) {
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

					length = (int) (inptr - start);

					if (inptr < inend) {
						var line = new Span<byte> (start, length);

						if ((boundary = CheckBoundary (startIndex, line)) != BoundaryType.None)
							break;

						if (length > 0 && *(inptr - 1) == (byte) '\r')
							formats[(int) NewLineFormat.Dos] = true;
						else
							formats[(int) NewLineFormat.Unix] = true;

						// consume the '\n'
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

						var line = new Span<byte> (start, length);

						if ((boundary = CheckBoundary (startIndex, line)) != BoundaryType.None)
							break;
					}

					startIndex += length;
				}

				inputIndex = startIndex;
			}
		}
#endif

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

		ScanContentResult ScanContent (ScanContentType type, long beginOffset, int beginLineNumber, bool trimNewLine, CancellationToken cancellationToken)
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

				ScanContent (ref nleft, ref midline, ref formats);

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

		int ConstructMimePart (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMimePartContentBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MimeContent, beginOffset, beginLineNumber, true, cancellationToken);
			OnMimePartContentEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, result.Format, cancellationToken);

			return result.Lines;
		}

		int ConstructMessagePart (int depth, CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			if (bounds.Count > 0) {
				int atleast = Math.Max (ReadAheadSize, GetMaxBoundaryLength ());

				if (ReadAhead (atleast, 0, cancellationToken) <= 0) {
					boundary = BoundaryType.Eos;
					return 0;
				}

				// Note: This isn't obvious, but if the "boundary" that was found is an Mbox "From " line, then
				// either the current stream offset is >= contentEnd -or- RespectContentLength is false. It will
				// *never* be an Mbox "From " marker in Entity mode.
				if ((boundary = CheckBoundary ()) != BoundaryType.None)
					return GetLineCount (beginLineNumber, beginOffset, GetEndOffset (inputIndex));
			}

			// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
			state = MimeParserState.MessageHeaders;
			Step (cancellationToken);

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			OnMimeMessageBegin (currentBeginOffset, beginLineNumber, cancellationToken);

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (depth < options.MaxMimeDepth && IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (type, depth + 1, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (depth + 1, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (cancellationToken);
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

		void MultipartScanPreamble (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMultipartPreambleBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MultipartPreamble, beginOffset, beginLineNumber, false, cancellationToken);
			OnMultipartPreambleEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken);
		}

		void MultipartScanEpilogue (CancellationToken cancellationToken)
		{
			var beginOffset = GetOffset (inputIndex);
			var beginLineNumber = lineNumber;

			OnMultipartEpilogueBegin (beginOffset, beginLineNumber, cancellationToken);
			var result = ScanContent (ScanContentType.MultipartEpilogue, beginOffset, beginLineNumber, true, cancellationToken);
			OnMultipartEpilogueEnd (beginOffset, beginLineNumber, beginOffset + result.ContentLength, result.Lines, cancellationToken);
		}

		void MultipartScanSubparts (ContentType multipartContentType, int depth, CancellationToken cancellationToken)
		{
			var boundaryOffset = GetOffset (inputIndex);

			do {
				// skip over the boundary marker
				if (!SkipLine (true, cancellationToken)) {
					OnMultipartBoundary (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber, cancellationToken);
					boundary = BoundaryType.Eos;
					return;
				}

				OnMultipartBoundary (multipartContentType.Boundary, boundaryOffset, GetOffset (inputIndex), lineNumber - 1, cancellationToken);

				var beginLineNumber = lineNumber;

				// Note: When parsing non-toplevel parts, the header parser will never result in the Error state.
				state = MimeParserState.Headers;
				Step (cancellationToken);

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
					lines = ConstructMultipart (type, depth + 1, cancellationToken);
					entityType = MimeEntityType.Multipart;
				} else if (depth < options.MaxMimeDepth && IsMessagePart (type, currentEncoding)) {
					OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMessagePart (depth + 1, cancellationToken);
					entityType = MimeEntityType.MessagePart;
				} else {
					OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
					lines = ConstructMimePart (cancellationToken);
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

		int ConstructMultipart (ContentType contentType, int depth, CancellationToken cancellationToken)
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
				MultipartScanPreamble (cancellationToken);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			PushBoundary (marker);

			MultipartScanPreamble (cancellationToken);
			if (boundary == BoundaryType.ImmediateBoundary)
				MultipartScanSubparts (contentType, depth, cancellationToken);

			if (boundary == BoundaryType.ImmediateEndBoundary) {
				// consume the end boundary and read the epilogue (if there is one)
				var boundaryOffset = GetOffset (inputIndex);
				var boundaryLineNumber = lineNumber;

				SkipLine (false, cancellationToken);

				OnMultipartEndBoundary (marker, boundaryOffset, GetOffset (inputIndex), boundaryLineNumber, cancellationToken);

				PopBoundary ();

				MultipartScanEpilogue (cancellationToken);

				endOffset = GetEndOffset (inputIndex);

				return GetLineCount (beginLineNumber, beginOffset, endOffset);
			}

			// We either found the end of the stream or we found a parent's boundary
			PopBoundary ();

			if (boundary == BoundaryType.ParentEndBoundary && FoundImmediateBoundary (true))
				boundary = BoundaryType.ImmediateEndBoundary;
			else if (boundary == BoundaryType.ParentBoundary && FoundImmediateBoundary (false))
				boundary = BoundaryType.ImmediateBoundary;

			endOffset = GetEndOffset (inputIndex);

			return GetLineCount (beginLineNumber, beginOffset, endOffset);
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
			state = MimeParserState.Headers;
			toplevel = true;

			if (Step (cancellationToken) == MimeParserState.Error)
				throw new FormatException ("Failed to parse headers.");

			state = eos ? MimeParserState.Eos : MimeParserState.Complete;
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
			var beginLineNumber = lineNumber;

			state = MimeParserState.Headers;
			toplevel = true;

			// parse the headers
			switch (Step (cancellationToken)) {
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

			if (IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (type, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (0, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (cancellationToken);
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
			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (Step (cancellationToken)) {
				case MimeParserState.Error:
					throw new FormatException ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new FormatException ("End of stream.");
				}
			}

			var beginLineNumber = lineNumber;
			toplevel = true;

			// parse the headers
			switch (Step (cancellationToken)) {
			case MimeParserState.Error:
				throw new FormatException ("Failed to parse message headers.");
			case MimeParserState.Eos:
				throw new FormatException ("End of stream.");
			}

			var currentHeadersEndOffset = headerBlockEnd;
			var currentBeginOffset = headerBlockBegin;

			OnMimeMessageBegin (currentBeginOffset, beginLineNumber, cancellationToken);

			if (format == MimeFormat.Mbox && options.RespectContentLength && currentContentLength.HasValue)
				contentEnd = GetOffset (inputIndex) + currentContentLength.Value;
			else
				contentEnd = 0;

			var type = GetContentType (null);
			MimeEntityType entityType;
			int lines;

			if (IsMultipart (type)) {
				OnMultipartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMultipart (type, 0, cancellationToken);
				entityType = MimeEntityType.Multipart;
			} else if (IsMessagePart (type, currentEncoding)) {
				OnMessagePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMessagePart (0, cancellationToken);
				entityType = MimeEntityType.MessagePart;
			} else {
				OnMimePartBegin (type, currentBeginOffset, beginLineNumber, cancellationToken);
				lines = ConstructMimePart (cancellationToken);
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
	}
}
