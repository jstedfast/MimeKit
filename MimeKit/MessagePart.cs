//
// MessagePart.cs
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
using System.Threading.Tasks;

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A MIME part containing a <see cref="MimeMessage"/> as its content.
	/// </summary>
	/// <remarks>
	/// Represents MIME entities such as those with a Content-Type of message/rfc822 or message/news.
	/// </remarks>
	public class MessagePart : MimeEntity, IMessagePart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MessagePart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MessagePart"/>.
		/// </remarks>
		/// <param name="subtype">The message subtype.</param>
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="subtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeMessage"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public MessagePart (string subtype, params object[] args) : this (subtype)
		{
			if (args is null)
				throw new ArgumentNullException (nameof (args));

			MimeMessage message = null;

			foreach (object obj in args) {
				if (obj is null || TryInit (obj))
					continue;

				if (obj is MimeMessage mesg) {
					if (message != null)
						throw new ArgumentException ("MimeMessage should not be specified more than once.");

					message = mesg;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (message != null)
				Message = message;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="ContentType"/> based on the provided media type and subtype.
		/// </remarks>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		protected MessagePart (string mediaType, string mediaSubtype) : base (mediaType, mediaSubtype)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME message entity with the specified subtype.
		/// </remarks>
		/// <param name="subtype">The message subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="subtype"/> is <c>null</c>.
		/// </exception>
		public MessagePart (string subtype) : this ("message", subtype)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message/rfc822 MIME entity.
		/// </remarks>
		public MessagePart () : this ("rfc822")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MessagePart));
		}

		/// <summary>
		/// Gets or sets the message content.
		/// </summary>
		/// <remarks>
		/// Gets or sets the message content.
		/// </remarks>
		/// <value>The message content.</value>
		public MimeMessage Message {
			get; set;
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MessagePart"/> nodes
		/// calls <see cref="MimeVisitor.VisitMessagePart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMessagePart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MessagePart"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMessagePart (this);
		}

		/// <summary>
		/// Prepare the MIME entity for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the MIME entity for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MessagePart"/> has been disposed.
		/// </exception>
		public override void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			CheckDisposed ();

			Message?.Prepare (constraint, maxLineLength);
		}

		/// <summary>
		/// Write the <see cref="MessagePart"/> to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the MIME entity and its message to the output stream.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MessagePart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void WriteTo (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default)
		{
			base.WriteTo (options, stream, contentOnly, cancellationToken);

			if (Message is null)
				return;

			if (Message.MboxMarker != null && Message.MboxMarker.Length != 0) {
				if (stream is ICancellableStream cancellable) {
					cancellable.Write (Message.MboxMarker, 0, Message.MboxMarker.Length, cancellationToken);
				} else {
					stream.Write (Message.MboxMarker, 0, Message.MboxMarker.Length);
				}
			}

			if (options.EnsureNewLine) {
				options = options.Clone ();
				options.EnsureNewLine = false;
			}

			Message.WriteTo (options, stream, cancellationToken);
		}

		/// <summary>
		/// Asynchronously write the <see cref="MessagePart"/> to the output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the MIME entity and its message to the output stream.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MessagePart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override async Task WriteToAsync (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default)
		{
			await base.WriteToAsync (options, stream, contentOnly, cancellationToken).ConfigureAwait (false);

			if (Message is null)
				return;

			if (Message.MboxMarker != null && Message.MboxMarker.Length != 0)
				await stream.WriteAsync (Message.MboxMarker, 0, Message.MboxMarker.Length, cancellationToken).ConfigureAwait (false);

			if (options.EnsureNewLine) {
				options = options.Clone ();
				options.EnsureNewLine = false;
			}

			await Message.WriteToAsync (options, stream, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="MessagePart"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="MessagePart"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && Message != null)
				Message.Dispose ();

			base.Dispose (disposing);
		}
	}
}
