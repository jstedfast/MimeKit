// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// A MIME part containing message headers as its content.
	/// </summary>
	/// <remarks>
	/// Represents MIME entities with a Content-Type of text/rfc822-headers.
	/// </remarks>
	public class TextRfc822Headers : MessagePart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TextRfc822Headers"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public TextRfc822Headers (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TextRfc822Headers"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TextRfc822Headers"/>.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeMessage"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public TextRfc822Headers (params object[] args) : this ()
		{
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
		/// Initialize a new instance of the <see cref="TextRfc822Headers"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new text/rfc822-headers MIME entity.
		/// </remarks>
		public TextRfc822Headers () : base ("text", "rfc822-headers")
		{
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="TextRfc822Headers"/> nodes
		/// calls <see cref="MimeVisitor.VisitTextRfc822Headers"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitTextRfc822Headers"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitTextRfc822Headers (this);
		}
	}
}
