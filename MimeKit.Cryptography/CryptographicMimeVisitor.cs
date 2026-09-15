//
// CryptographicMimeVisitor.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// Represents a visitor for cryptographic MIME trees.
	/// </summary>
	/// <remarks>
	/// This class is designed to be inherited to create more specialized classes whose
	/// functionality requires traversing, examining or copying a MIME tree.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public abstract class CryptographicMimeVisitor : MimeVisitor
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CryptographicMimeVisitor"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CryptographicMimeVisitor"/>.
		/// </remarks>
		protected CryptographicMimeVisitor ()
		{
		}

		/// <summary>
		/// Visit the application/pgp-encrypted MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pgp-encrypted MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartEncrypted"/>
		/// <param name="entity">The application/pgp-encrypted MIME entity.</param>
		protected internal virtual void VisitApplicationPgpEncrypted (ApplicationPgpEncrypted entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pgp-signature MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pgp-signature MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartSigned"/>
		/// <param name="entity">The application/pgp-signature MIME entity.</param>
		protected internal virtual void VisitApplicationPgpSignature (ApplicationPgpSignature entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pkcs7-mime MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pkcs7-mime MIME entity.
		/// </remarks>
		/// <param name="entity">The application/pkcs7-mime MIME entity.</param>
		protected internal virtual void VisitApplicationPkcs7Mime (ApplicationPkcs7Mime entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pkcs7-signature MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pkcs7-signature MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartSigned"/>
		/// <param name="entity">The application/pkcs7-signature MIME entity.</param>
		protected internal virtual void VisitApplicationPkcs7Signature (ApplicationPkcs7Signature entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the multipart/encrypted MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/encrypted MIME entity.
		/// </remarks>
		/// <param name="encrypted">The multipart/encrypted MIME entity.</param>
		protected internal virtual void VisitMultipartEncrypted (MultipartEncrypted encrypted)
		{
			VisitMultipart (encrypted);
		}

		/// <summary>
		/// Visit the multipart/signed MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/signed MIME entity.
		/// </remarks>
		/// <param name="signed">The multipart/signed MIME entity.</param>
		protected internal virtual void VisitMultipartSigned (MultipartSigned signed)
		{
			VisitMultipart (signed);
		}
	}
}
