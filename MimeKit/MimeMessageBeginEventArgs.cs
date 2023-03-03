// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// Event args emitted by the <see cref="MimeParser"/> when it begins parsing a <see cref="MimeMessage"/>.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when it begins parsing a <see cref="MimeMessage"/>.
	/// </remarks>
	public class MimeMessageBeginEventArgs : EventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageBeginEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageBeginEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public MimeMessageBeginEventArgs (MimeMessage message)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			Message = message;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageBeginEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageBeginEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <param name="parent">The parent message part.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeMessageBeginEventArgs (MimeMessage message, MessagePart parent)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (parent is null)
				throw new ArgumentNullException (nameof (parent));

			Message = message;
			Parent = parent;
		}

		/// <summary>
		/// Get the message that was parsed.
		/// </summary>
		/// <remarks>
		/// Gets the message that was parsed.
		/// </remarks>
		/// <value>The message.</value>
		public MimeMessage Message { get; }

		/// <summary>
		/// Get the parent <see cref="MessagePart"/> if this message is an attachment.
		/// </summary>
		/// <remarks>
		/// Gets the parent <see cref="MessagePart"/> if this message is an attachment.
		/// </remarks>
		/// <value>The parent <see cref="MessagePart"/>.</value>
		public MessagePart Parent { get; }

		/// <summary>
		/// Get or set the stream offset that marks the beginning of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the beginning of the message.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long BeginOffset { get; set; }

		/// <summary>
		/// Get or set the line number of the beginning of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the line number of the beginning of the message.
		/// </remarks>
		/// <value>The line number.</value>
		public int LineNumber { get; set; }
	}
}
