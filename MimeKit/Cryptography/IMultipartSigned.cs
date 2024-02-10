//
// IMultipartSigned.cs
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
	/// An interface for a signed multipart, as used by both S/MIME and PGP/MIME protocols.
	/// </summary>
	/// <remarks>
	/// The first child of a multipart/signed is the content while the second child
	/// is the detached signature data. Any other children are not defined and could
	/// be anything.
	/// </remarks>
	public interface IMultipartSigned : IMultipart
	{
		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the supplied cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="ctx">The cryptography context to use for verifying the signature.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The multipart is malformed in some way.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="ctx"/> does not support verifying the signature part.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartSigned"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		DigitalSignatureCollection Verify (CryptographyContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the supplied cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="ctx">The cryptography context to use for verifying the signature.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The multipart is malformed in some way.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="ctx"/> does not support verifying the signature part.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartSigned"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<DigitalSignatureCollection> VerifyAsync (CryptographyContext ctx, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the default cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for verifying the signature could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartSigned"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		DigitalSignatureCollection Verify (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the default cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for verifying the signature could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartSigned"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<DigitalSignatureCollection> VerifyAsync (CancellationToken cancellationToken = default);
	}
}
