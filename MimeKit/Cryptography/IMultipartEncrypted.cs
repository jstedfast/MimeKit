//
// IMultipartEncrypted.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a multipart MIME part with a ContentType of multipart/encrypted containing an encrypted MIME part.
	/// </summary>
	/// <remarks>
	/// This mime-type is common when dealing with PGP/MIME but is not used for S/MIME.
	/// </remarks>
	public interface IMultipartEncrypted : IMultipart
	{
		/// <summary>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="IMultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		MimeEntity Decrypt (OpenPgpContext ctx, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		MimeEntity Decrypt (OpenPgpContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="IMultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		MimeEntity Decrypt (out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="IMultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		MimeEntity Decrypt (CancellationToken cancellationToken = default);
	}
}
