// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeEntity"/> is parsed.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeEntity"/> is parsed.
	/// </remarks>
	public class MimeEntityEndEventArgs : MimeEntityBeginEventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityEndEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that was parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		public MimeEntityEndEventArgs (MimeEntity entity) : base (entity)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityEndEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that was parsed.</param>
		/// <param name="parent">The parent multipart.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeEntityEndEventArgs (MimeEntity entity, Multipart parent) : base (entity, parent)
		{
		}

		/// <summary>
		/// Get or set the stream offset that marks the end of the entity's headers.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the entity's headers.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long HeadersEndOffset { get; set; }

		/// <summary>
		/// Get or set the stream offset that marks the end of the entity.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the entity.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long EndOffset { get; set; }

		/// <summary>
		/// Get or set the content length of the entity as measured in lines.
		/// </summary>
		/// <remarks>
		/// <para>Get or set the content length of the entity as measured in lines.</para>
		/// <note type="note">The line count reported by this property is the number of lines in its
		/// content transfer encoding and not the resulting line count after any decoding.</note>
		/// </remarks>
		/// <value>The length of the content in lines.</value>
		public int Lines { get; set; }
	}
}
