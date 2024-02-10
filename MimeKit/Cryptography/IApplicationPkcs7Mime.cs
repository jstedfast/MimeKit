//
// IApplicationPkcs7Mime.cs
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

using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for an S/MIME part with a Content-Type of application/pkcs7-mime.
	/// </summary>
	/// <remarks>
	/// An application/pkcs7-mime is an S/MIME part and may contain encrypted,
	/// signed or compressed data (or any combination of the above).
	/// </remarks>
	public interface IApplicationPkcs7Mime : IMimePart
	{
		/// <summary>
		/// Gets the value of the "smime-type" parameter.
		/// </summary>
		/// <remarks>
		/// Gets the value of the "smime-type" parameter.
		/// </remarks>
		/// <value>The value of the "smime-type" parameter.</value>
		SecureMimeType SecureMimeType {
			get;
		}

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		MimeEntity Decompress (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<MimeEntity> DecompressAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		MimeEntity Decompress (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<MimeEntity> DecompressAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		MimeEntity Decrypt (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<MimeEntity> DecryptAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		MimeEntity Decrypt (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<MimeEntity> DecryptAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		void Import (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task ImportAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verify the signed-data and return the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="ctx">The S/MIME context to use for verifying the signature.</param>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		DigitalSignatureCollection Verify (SecureMimeContext ctx, out MimeEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data using the default <see cref="SecureMimeContext"/> and returns the
		/// unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		DigitalSignatureCollection Verify (out MimeEntity entity, CancellationToken cancellationToken = default);
	}
}
