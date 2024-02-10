//
// ICryptographyContext.cs
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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract cryptography context interface.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="ICryptographyContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>,
	/// <see cref="MultipartEncrypted"/> and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public interface ICryptographyContext : IDisposable
	{
		/// <summary>
		/// Get or set whether a <see cref="MimeEntity"/> should be prepared before signing.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether a <see cref="MimeEntity"/> should be prepared before signing.
		/// </remarks>
		/// <value><c>true</c> if a MimeEntity should be prepared before signing; otherwise, <c>false</c>.</value>
		bool PrepareBeforeSigning { get; set; }

		/// <summary>
		/// Get the signature protocol.
		/// </summary>
		/// <remarks>
		/// <para>The signature protocol is used by <see cref="MultipartSigned"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The signature protocol.</value>
		string SignatureProtocol { get; }

		/// <summary>
		/// Get the encryption protocol.
		/// </summary>
		/// <remarks>
		/// <para>The encryption protocol is used by <see cref="MultipartEncrypted"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The encryption protocol.</value>
		string EncryptionProtocol { get; }

		/// <summary>
		/// Get the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// <para>The key exchange protocol is really only used for OpenPGP.</para>
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		string KeyExchangeProtocol { get; }

		/// <summary>
		/// Check whether or not the specified protocol is supported by the <see cref="ICryptographyContext"/>.
		/// </summary>
		/// <remarks>
		/// Used in order to make sure that the protocol parameter value specified in either a multipart/signed
		/// or multipart/encrypted part is supported by the supplied cryptography context.
		/// </remarks>
		/// <returns><c>true</c> if the protocol is supported; otherwise <c>false</c></returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		bool Supports (string protocol);

		/// <summary>
		/// Get the enabled digest algorithms in ranked order.
		/// </summary>
		/// <remarks>
		/// Gets the enabled digest algorithms in ranked order.
		/// </remarks>
		/// <value>The enabled encryption algorithms.</value>
		DigestAlgorithm[] EnabledDigestAlgorithms { get; }

		/// <summary>
		/// Enable the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Enables the digest algorithm.
		/// </remarks>
		/// <param name="algorithm">The digest algorithm.</param>
		void Enable (DigestAlgorithm algorithm);

		/// <summary>
		/// Disable the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Disables the digest algorithm.
		/// </remarks>
		/// <param name="algorithm">The digest algorithm.</param>
		void Disable (DigestAlgorithm algorithm);

		/// <summary>
		/// Get the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the micalg parameter value string back to the appropriate <see cref="DigestAlgorithm"/>.
		/// </remarks>
		/// <returns>The digest algorithm.</returns>
		/// <param name="micalg">The micalg parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="micalg"/> is <c>null</c>.
		/// </exception>
		DigestAlgorithm GetDigestAlgorithm (string micalg);

		/// <summary>
		/// Get the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the <see cref="DigestAlgorithm"/> to the appropriate string identifier
		/// as used by the micalg parameter value of a multipart/signed Content-Type
		/// header.
		/// </remarks>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		string GetDigestAlgorithmName (DigestAlgorithm micalg);

		/// <summary>
		/// Check whether the specified digest algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified digest algorithm is enabled.
		/// </remarks>
		/// <returns><c>true</c> if the specified digest algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">The digest algorithm.</param>
		bool IsEnabled (DigestAlgorithm algorithm);

		/// <summary>
		/// Get the enabled encryption algorithms in ranked order.
		/// </summary>
		/// <remarks>
		/// Gets the enabled encryption algorithms in ranked order.
		/// </remarks>
		/// <value>The enabled encryption algorithms.</value>
		EncryptionAlgorithm[] EnabledEncryptionAlgorithms { get; }

		/// <summary>
		/// Enable the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Enables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		void Enable (EncryptionAlgorithm algorithm);

		/// <summary>
		/// Disable the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Disables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		void Disable (EncryptionAlgorithm algorithm);

		/// <summary>
		/// Check whether the specified encryption algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified encryption algorithm is enabled.
		/// </remarks>
		/// <returns><c>true</c> if the specified encryption algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		bool IsEnabled (EncryptionAlgorithm algorithm);

		/// <summary>
		/// Check whether or not the cryptography context can encrypt to a particular recipient.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the cryptography context can be used to encrypt to a particular recipient.
		/// </remarks>
		/// <returns><c>true</c> if the cryptography context can be used to encrypt to the designated recipient; otherwise, <c>false</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously check whether or not the cryptography context can encrypt to a particular recipient.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the cryptography context can be used to encrypt to a particular recipient.
		/// </remarks>
		/// <returns><c>true</c> if the cryptography context can be used to encrypt to the designated recipient; otherwise, <c>false</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<bool> CanEncryptAsync (MailboxAddress mailbox, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Decrypt the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously decrypt the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Check whether or not a particular mailbox address can be used for signing.
		/// </summary>
		/// <remarks>
		/// Checks whether or not as particular mailbocx address can be used for signing.
		/// </remarks>
		/// <returns><c>true</c> if the mailbox address can be used for signing; otherwise, <c>false</c>.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously check whether or not a particular mailbox address can be used for signing.
		/// </summary>
		/// <remarks>
		/// Checks whether or not as particular mailbocx address can be used for signing.
		/// </remarks>
		/// <returns><c>true</c> if the mailbox address can be used for signing; otherwise, <c>false</c>.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<bool> CanSignAsync (MailboxAddress signer, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Sign the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
		MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously sign the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
		Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Verify the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signatureData.
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously verify the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signatureData.
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Imports the public certificates or keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports the public certificates or keys from the specified stream.
		/// </remarks>
		/// <param name="stream">The raw certificate or key data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		void Import (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously imports the public certificates or keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports the public certificates or keys from the specified stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate or key data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task ImportAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Exports the keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Asynchronously exports the keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Asynchronously exports the keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default (CancellationToken));
	}
}
