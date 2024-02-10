//
// ApplicationPgpEncrypted.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A MIME part with a Content-Type of application/pgp-encrypted.
	/// </summary>
	/// <remarks>
	/// An application/pgp-encrypted part will typically be the first child of
	/// a <see cref="MultipartEncrypted"/> part and contains only a Version
	/// header.
	/// </remarks>
	public class ApplicationPgpEncrypted : MimePart, IApplicationPgpEncrypted
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPgpEncrypted"/>
		/// class based on the <see cref="MimeEntityConstructorArgs"/>.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public ApplicationPgpEncrypted (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPgpEncrypted"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME part with a Content-Type of application/pgp-encrypted
		/// and content matching <c>"Version: 1\n"</c>.
		/// </remarks>
		public ApplicationPgpEncrypted () : base ("application", "pgp-encrypted")
		{
			ContentDisposition = new ContentDisposition ("attachment");
			ContentTransferEncoding = ContentEncoding.SevenBit;

			var content = new MemoryStream (Encoding.UTF8.GetBytes ("Version: 1\n"), false);

			Content = new MimeContent (content);
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (ApplicationPgpEncrypted));
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="ApplicationPgpEncrypted"/> nodes
		/// calls <see cref="MimeVisitor.VisitApplicationPgpEncrypted"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitApplicationPgpEncrypted"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPgpEncrypted"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitApplicationPgpEncrypted (this);
		}
	}
}
