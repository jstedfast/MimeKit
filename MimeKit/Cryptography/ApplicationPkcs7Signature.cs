//
// ApplicationPkcs7Signature.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME part with a Content-Type of application/pkcs7-signature.
	/// </summary>
	/// <remarks>
	/// <para>An application/pkcs7-signature part contains detatched pkcs7 signature data
	/// and is typically contained within a <see cref="MultipartSigned"/> part.</para>
	/// <para>To verify the signature, use one of the
	/// <a href="Overload_MimeKit_Cryptography_MultipartSigned_Verify.htm">Verify</a>
	/// methods on the parent multipart/signed part.</para>
	/// </remarks>
	public class ApplicationPkcs7Signature : MimePart, IApplicationPkcs7Signature
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Signature"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public ApplicationPkcs7Signature (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Signature"/>
		/// class with a Content-Type of application/pkcs7-signature.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME part with a Content-Type of application/pkcs7-signature
		/// and the <paramref name="stream"/> as its content.
		/// </remarks>
		/// <param name="stream">The content stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="stream"/> does not support reading.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> does not support seeking.</para>
		/// </exception>
		public ApplicationPkcs7Signature (Stream stream) : base ("application", "pkcs7-signature")
		{
			ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
			ContentTransferEncoding = ContentEncoding.Base64;
			Content = new MimeContent (stream);
			FileName = "smime.p7s";
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (ApplicationPkcs7Signature));
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="ApplicationPkcs7Signature"/> nodes
		/// calls <see cref="MimeVisitor.VisitApplicationPkcs7Signature"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitApplicationPkcs7Signature"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Signature"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitApplicationPkcs7Signature (this);
		}
	}
}
