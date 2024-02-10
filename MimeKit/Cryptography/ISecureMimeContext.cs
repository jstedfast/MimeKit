//
// SecureMimeContext.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="ISecureMimeContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public interface ISecureMimeContext : ICryptographyContext
	{
		/// <summary>
		/// Compress the specified stream.
		/// </summary>
		/// <remarks>
		/// Compresses the specified stream.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		ApplicationPkcs7Mime Compress (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously compress the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously compresses the specified stream.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<ApplicationPkcs7Mime> CompressAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Decompress the specified stream.
		/// </summary>
		/// <remarks>
		/// Decompresses the specified stream.
		/// </remarks>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		MimeEntity Decompress (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously decompress the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the specified stream.
		/// </remarks>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<MimeEntity> DecompressAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Decompress the specified stream to an output stream.
		/// </summary>
		/// <remarks>
		/// Decompresses the specified stream to an output stream.
		/// </remarks>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="output">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		void DecompressTo (Stream stream, Stream output, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously decompress the specified stream to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the specified stream to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="output">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task DecompressToAsync (Stream stream, Stream output, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		void DecryptTo (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task DecryptToAsync (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Sign and encapsulate the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Signs and encapsulates the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously sign and encapsulate the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs and encapsulates the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<ApplicationPkcs7Signature> SignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Verify the digital signatures of the specified signed data and extract the original content.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signatures of the specified signed data and extracts the original content.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="entity">The extracted MIME entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Verify the digital signatures of the specified signed data and extract the original content.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signatures of the specified signed data and extracts the original content.
		/// </remarks>
		/// <returns>The extracted content stream.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="signatures">The digital signatures.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Stream Verify (Stream signedData, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		void Import (Stream stream, string password, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task ImportAsync (Stream stream, string password, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Imports certificates and keys from a pkcs12 file.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12 file.
		/// </remarks>
		/// <param name="fileName">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a private key.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a certificate that could be used for signing.</para>
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		void Import (string fileName, string password, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously imports certificates and keys from a pkcs12 file.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates and keys from a pkcs12 file.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="fileName">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a private key.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a certificate that could be used for signing.</para>
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task ImportAsync (string fileName, string password, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		void Import (X509Certificate2 certificate, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously import a certificate.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports a certificate.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task ImportAsync (X509Certificate2 certificate, CancellationToken cancellationToken = default (CancellationToken));
	}
}
