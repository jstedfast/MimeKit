//
// TextRfc822Headers.cs
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

namespace MimeKit {
	/// <summary>
	/// A MIME part containing message headers as its content.
	/// </summary>
	/// <remarks>
	/// Represents MIME entities with a Content-Type of text/rfc822-headers.
	/// </remarks>
	public class TextRfc822Headers : MessagePart, ITextRfc822Headers
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
