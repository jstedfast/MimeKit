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
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An experimental replacement for <see cref="MimeParser"/>.
	/// </summary>
	/// <remarks>
	/// An experimental replacement for <see cref="MimeParser"/>.
	/// </remarks>
	public class ExperimentalMimeParser : MimeReader, IMimeParser
	{
		readonly Stack<object> stack = new Stack<object> ();

		// Mbox state
		byte[] mboxMarkerBuffer;
		long mboxMarkerOffset;
		int mboxMarkerLength;

		// Current MimeMessage/MimeEntity Header state
		readonly List<Header> headers = new List<Header> ();
		byte[] preHeaderBuffer;
		int preHeaderLength;

		// MimePart content and Multipart preamble/epilogue state
		Stream content;

		bool parsingMessageHeaders;
		int depth;

		bool persistent;

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="ExperimentalMimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="ExperimentalMimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="ExperimentalMimeParser"/>
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
		public ExperimentalMimeParser (Stream stream, MimeFormat format, bool persistent = false) : this (ParserOptions.Default, stream, format, persistent)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ExperimentalMimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="ExperimentalMimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="ExperimentalMimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="ExperimentalMimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public ExperimentalMimeParser (Stream stream, bool persistent = false) : this (ParserOptions.Default, stream, MimeFormat.Default, persistent)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ExperimentalMimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="ExperimentalMimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="ExperimentalMimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="ExperimentalMimeParser"/>
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
		public ExperimentalMimeParser (ParserOptions options, Stream stream, bool persistent = false) : this (options, stream, MimeFormat.Default, persistent)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ExperimentalMimeParser"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="ExperimentalMimeParser"/> that will parse the specified stream.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="ExperimentalMimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="ExperimentalMimeParser"/>
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
		public ExperimentalMimeParser (ParserOptions options, Stream stream, MimeFormat format, bool persistent = false) : base (options, stream, format)
		{
			this.persistent = persistent && stream.CanSeek;
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
			base.SetStream (stream, format);

			this.persistent = persistent && stream.CanSeek;
			if (format == MimeFormat.Mbox && mboxMarkerBuffer is null)
				mboxMarkerBuffer = new byte[64];
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
		public override void SetStream (Stream stream, MimeFormat format = MimeFormat.Default)
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

		void PopEntity ()
		{
			if (stack.Count > 1) {
				var entity = (MimeEntity) stack.Pop ();
				var parent = stack.Peek ();

				if (parent is MimeMessage message)
					message.Body = entity;
				else
					((Multipart) parent).InternalAdd (entity);
			}
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
		protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
		{
			if (mboxMarkerBuffer.Length < count)
				Array.Resize (ref mboxMarkerBuffer, count);

			Buffer.BlockCopy (buffer, startIndex, mboxMarkerBuffer, 0, count);
			mboxMarkerOffset = beginOffset;
			mboxMarkerLength = count;
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
		protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			headers.Clear ();
			preHeaderLength = 0;
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
		protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
		{
			if (parsingMessageHeaders && header.IsInvalid && headers.Count == 0) {
				if (preHeaderBuffer is null)
					preHeaderBuffer = new byte[header.RawField.Length];
				else if (header.RawField.Length + preHeaderLength > preHeaderBuffer.Length)
					Array.Resize (ref preHeaderBuffer, header.RawField.Length + preHeaderLength);

				Buffer.BlockCopy (header.RawField, 0, preHeaderBuffer, preHeaderLength, header.RawField.Length);
				preHeaderLength += header.RawField.Length;
			} else {
				headers.Add (header);
			}
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
		protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
		{
			parsingMessageHeaders = false;
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
		protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			var message = new MimeMessage (Options, headers, RfcComplianceMode.Loose);

			if (preHeaderLength > 0) {
				message.MboxMarker = new byte[preHeaderLength];
				Buffer.BlockCopy (preHeaderBuffer, 0, message.MboxMarker, 0, preHeaderLength);
			}

			stack.Push (message);
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
		protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			if (stack.Count > 1) {
				var message = (MimeMessage) stack.Pop ();
				var rfc822 = (MessagePart) stack.Peek ();

				rfc822.Message = message;
			}
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
		protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			var toplevel = stack.Count > 0 && stack.Peek () is MimeMessage;
			var part = Options.CreateEntity (contentType, headers, toplevel, depth);

			stack.Push (part);
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
		protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			content = persistent ? null : new MemoryBlockStream ();
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
		protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			if (!persistent)
				content.Write (buffer, startIndex, count);
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
		protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
		{
			if (endOffset <= beginOffset && !newLineFormat.HasValue) {
				// Note: This is a hack that makes Multipart.WriteTo() work properly.
				content?.Dispose ();
				content = null;
				return;
			}

			var part = (MimePart) stack.Peek ();

			if (persistent) {
				content = new BoundStream (stream, beginOffset, endOffset, true);
			} else {
				content.SetLength (endOffset - beginOffset);
				content.Position = 0;
			}

			part.Content = new MimeContent (content, part.ContentTransferEncoding) { NewLineFormat = newLineFormat };
			content = null;
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
		protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			PopEntity ();
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
		protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			var toplevel = stack.Count > 0 && stack.Peek () is MimeMessage;
			var rfc822 = Options.CreateEntity (contentType, headers, toplevel, depth);

			parsingMessageHeaders = true;
			stack.Push (rfc822);
			depth++;
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
		protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			PopEntity ();
			depth--;
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
		protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			var toplevel = stack.Count > 0 && stack.Peek () is MimeMessage;
			var multipart = Options.CreateEntity (contentType, headers, toplevel, depth);

			stack.Push (multipart);
			depth++;
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
		protected override void OnMultipartBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
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
		protected override void OnMultipartEndBoundary (string boundary, long beginOffset, long endOffset, int lineNumber, CancellationToken cancellationToken)
		{
			var multipart = (Multipart) stack.Peek ();

			multipart.WriteEndBoundary = true;
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
		protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			content = new MemoryStream ();
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
		protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			content.Write (buffer, startIndex, count);
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
		protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			var multipart = (Multipart) stack.Peek ();

			content.SetLength (endOffset - beginOffset);

			multipart.RawPreamble = ((MemoryStream) content).ToArray ();
			content.Dispose ();
			content = null;
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
		protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
		{
			content = new MemoryStream ();
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
		protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
		{
			content.Write (buffer, startIndex, count);
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
		protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
		{
			var multipart = (Multipart) stack.Peek ();

			content.SetLength (endOffset - beginOffset);

			multipart.RawEpilogue = ((MemoryStream) content).ToArray ();
			content.Dispose ();
			content = null;
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
		protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
		{
			PopEntity ();
			depth--;
		}

		#endregion Multipart Events

		void Reset ()
		{
			while (stack.Count > 0) {
				var item = (IDisposable) stack.Pop ();
				item.Dispose ();
			}

			content?.Dispose ();
			content = null;
		}

		void Initialize (bool parsingMessageHeaders)
		{
			// Note: if a previously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (persistent && stream.Position != position)
				stream.Seek (position, SeekOrigin.Begin);

			this.parsingMessageHeaders = parsingMessageHeaders;
			stack.Clear ();
			depth = 0;
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
			Initialize (false);

			try {
				ReadHeaders (cancellationToken);
			} catch {
				Reset ();
				throw;
			}

			var parsed = new HeaderList (Options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
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
			Initialize (false);

			try {
				await ReadHeadersAsync (cancellationToken).ConfigureAwait (false);
			} catch {
				Reset ();
				throw;
			}

			var parsed = new HeaderList (Options);
			foreach (var header in headers)
				parsed.Add (header);

			return parsed;
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
			Initialize (false);

			try {
				ReadEntity (cancellationToken);
			} catch {
				Reset ();
				throw;
			}

			return (MimeEntity) stack.Pop ();
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
			Initialize (false);

			try {
				await ReadEntityAsync (cancellationToken).ConfigureAwait (false);
			} catch {
				Reset ();
				throw;
			}

			return (MimeEntity) stack.Pop ();
		}

		/// <summary>
		/// Parse a message from the stream.
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
		public MimeMessage ParseMessage (CancellationToken cancellationToken = default)
		{
			Initialize (true);

			try {
				ReadMessage (cancellationToken);
			} catch {
				Reset ();
				throw;
			}

			return (MimeMessage) stack.Pop ();
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
			Initialize (true);

			try {
				await ReadMessageAsync (cancellationToken).ConfigureAwait (false);
			} catch {
				Reset ();
				throw;
			}

			return (MimeMessage) stack.Pop ();
		}
	}
}
