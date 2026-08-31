//
// ApplicationPkcs7Mime.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Collections.Generic;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME part with a Content-Type of application/pkcs7-mime.
	/// </summary>
	/// <remarks>
	/// An application/pkcs7-mime is an S/MIME part and may contain encrypted,
	/// signed or compressed data (or any combination of the above).
	/// </remarks>
	public class ApplicationPkcs7Mime : MimePart, IApplicationPkcs7Mime
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <see langword="null"/>.
		/// </exception>
		public ApplicationPkcs7Mime (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new MIME part with a Content-Type of application/pkcs7-mime
		/// and the <paramref name="stream"/> as its content.</para>
		/// <para>Unless you are writing your own pkcs7 implementation, you'll probably
		/// want to use the <c>SecureMime.Compress</c>, <c>SecureMime.Encrypt</c>, and/or
		/// <c>SecureMime.EncapsulatedSign</c> methods (in the MimeKit.Cryptography package) to create new instances
		/// of this class.</para>
		/// </remarks>
		/// <param name="type">The S/MIME type.</param>
		/// <param name="stream">The content stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="type"/> is not a valid value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="stream"/> does not support reading.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> does not support seeking.</para>
		/// </exception>
		public ApplicationPkcs7Mime (SecureMimeType type, Stream stream) : base ("application", "pkcs7-mime")
		{
			ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
			ContentTransferEncoding = ContentEncoding.Base64;
			Content = new MimeContent (stream);

			switch (type) {
			case SecureMimeType.CompressedData:
				ContentType.Parameters["smime-type"] = "compressed-data";
				ContentDisposition.FileName = "smime.p7z";
				ContentType.Name = "smime.p7z";
				break;
			case SecureMimeType.EnvelopedData:
				ContentType.Parameters["smime-type"] = "enveloped-data";
				ContentDisposition.FileName = "smime.p7m";
				ContentType.Name = "smime.p7m";
				break;
			case SecureMimeType.SignedData:
				ContentType.Parameters["smime-type"] = "signed-data";
				ContentDisposition.FileName = "smime.p7m";
				ContentType.Name = "smime.p7m";
				break;
			case SecureMimeType.AuthEnvelopedData:
				ContentType.Parameters["smime-type"] = "authenveloped-data";
				ContentDisposition.FileName = "smime.p7m";
				ContentType.Name = "smime.p7m";
				break;
			case SecureMimeType.CertsOnly:
				ContentType.Parameters["smime-type"] = "certs-only";
				ContentDisposition.FileName = "smime.p7c";
				ContentType.Name = "smime.p7c";
				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (type));
			}
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (ApplicationPkcs7Mime));
		}

		/// <summary>
		/// Gets the value of the "smime-type" parameter.
		/// </summary>
		/// <remarks>
		/// Gets the value of the "smime-type" parameter.
		/// </remarks>
		/// <value>The value of the "smime-type" parameter.</value>
		public SecureMimeType SecureMimeType {
			get {
				var type = ContentType.Parameters["smime-type"];

				if (type == null)
					return SecureMimeType.Unknown;

				if (type.Equals ("authenveloped-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.AuthEnvelopedData;
				if (type.Equals ("compressed-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.CompressedData;
				if (type.Equals ("enveloped-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.EnvelopedData;
				if (type.Equals ("signed-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.SignedData;
				if (type.Equals ("certs-only", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.CertsOnly;

				return SecureMimeType.Unknown;
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="ApplicationPkcs7Mime"/> nodes
		/// calls <see cref="MimeVisitor.VisitApplicationPkcs7Mime"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitApplicationPkcs7Mime"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitApplicationPkcs7Mime (this);
		}
	}
}
