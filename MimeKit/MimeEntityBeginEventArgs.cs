// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// Event args emitted by the <see cref="MimeParser"/> when it begins parsing a <see cref="MimeEntity"/>.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when it begins parsing a <see cref="MimeEntity"/>.
	/// </remarks>
	public class MimeEntityBeginEventArgs : EventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityBeginEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityBeginEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that is being parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		public MimeEntityBeginEventArgs (MimeEntity entity)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			Entity = entity;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityBeginEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityBeginEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that is being parsed.</param>
		/// <param name="parent">The parent multipart.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeEntityBeginEventArgs (MimeEntity entity, Multipart parent)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			if (parent is null)
				throw new ArgumentNullException (nameof (parent));

			Entity = entity;
			Parent = parent;
		}

		/// <summary>
		/// Get the MIME entity.
		/// </summary>
		/// <remarks>
		/// Gets the MIME entity.
		/// </remarks>
		/// <value>The MIME entity.</value>
		public MimeEntity Entity { get; }

		/// <summary>
		/// Get the parent <see cref="Multipart"/> if this entity is a child.
		/// </summary>
		/// <remarks>
		/// Gets the parent <see cref="Multipart"/> if this entity is a child.
		/// </remarks>
		/// <value>The parent <see cref="Multipart"/>.</value>
		public Multipart Parent { get; }

		/// <summary>
		/// Get or set the stream offset that marks the beginning of the entity.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the beginning of the entity.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long BeginOffset { get; set; }

		/// <summary>
		/// Get or set the line number of the beginning of the entity.
		/// </summary>
		/// <remarks>
		/// Gets or sets the line number of the beginning of the entity.
		/// </remarks>
		/// <value>The line number.</value>
		public int LineNumber { get; set; }
	}
}
