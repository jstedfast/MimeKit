// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeMessage"/> is parsed.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeMessage"/> is parsed.
	/// </remarks>
	public class MimeMessageEndEventArgs : MimeMessageBeginEventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageEndEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public MimeMessageEndEventArgs (MimeMessage message) : base (message)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageEndEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <param name="parent">The parent message part.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeMessageEndEventArgs (MimeMessage message, MessagePart parent) : base (message, parent)
		{
		}

		/// <summary>
		/// Get or set the stream offset that marks the end of the message headers.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the message headers.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long HeadersEndOffset { get; set; }

		/// <summary>
		/// Get or set the stream offset that marks the end of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the message.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long EndOffset { get; set; }
	}
}
