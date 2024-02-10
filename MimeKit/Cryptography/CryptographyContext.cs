//
// CryptographyContext.cs
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
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="CryptographyContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>,
	/// <see cref="MultipartEncrypted"/> and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public abstract class CryptographyContext : ICryptographyContext
	{
		const string SubclassAndRegisterFormat = "You need to subclass {0} and then register it with MimeKit.Cryptography.CryptographyContext.Register().";
		static Func<SecureMimeContext> SecureMimeContextFactory;
		static Func<OpenPgpContext> PgpContextFactory;
		static readonly object mutex = new object ();

		EncryptionAlgorithm[] encryptionAlgorithmRank;
		DigestAlgorithm[] digestAlgorithmRank;

		int enabledEncryptionAlgorithms;
		int enabledDigestAlgorithms;

		/// <summary>
		/// Initialize a new instance of the <see cref="CryptographyContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CryptographyContext"/>.</para>
		/// <para>By default, only the 3DES encryption algorithm and the SHA-1 digest algorithm are enabled.</para>
		/// </remarks>
		protected CryptographyContext ()
		{
			encryptionAlgorithmRank = new[] {
				EncryptionAlgorithm.TripleDes
			};

			Enable (EncryptionAlgorithm.TripleDes);

			digestAlgorithmRank = new[] {
				DigestAlgorithm.Sha1
			};

			Enable (DigestAlgorithm.Sha1);

			PrepareBeforeSigning = true;
		}

		/// <summary>
		/// Get or set whether a <see cref="MimeEntity"/> should be prepared before signing.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether a <see cref="MimeEntity"/> should be prepared before signing.
		/// </remarks>
		/// <value><c>true</c> if a MimeEntity should be prepared before signing; otherwise, <c>false</c>.</value>
		public bool PrepareBeforeSigning {
			get; set;
		}

		/// <summary>
		/// Get the signature protocol.
		/// </summary>
		/// <remarks>
		/// <para>The signature protocol is used by <see cref="MultipartSigned"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The signature protocol.</value>
		public abstract string SignatureProtocol { get; }

		/// <summary>
		/// Get the encryption protocol.
		/// </summary>
		/// <remarks>
		/// <para>The encryption protocol is used by <see cref="MultipartEncrypted"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The encryption protocol.</value>
		public abstract string EncryptionProtocol { get; }

		/// <summary>
		/// Get the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// <para>The key exchange protocol is really only used for OpenPGP.</para>
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public abstract string KeyExchangeProtocol { get; }

#if NOT_YET
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CryptographyContext"/> allows online
		/// certificate retrieval.
		/// </summary>
		/// <value><c>true</c> if online certificate retrieval should be allowed; otherwise, <c>false</c>.</value>
		public bool AllowOnlineCertificateRetrieval { get; set; }

		/// <summary>
		/// Gets or sets the online certificate retrieval timeout.
		/// </summary>
		/// <value>The online certificate retrieval timeout.</value>
		public TimeSpan OnlineCertificateRetrievalTimeout { get; set; }
#endif

		/// <summary>
		/// Get the preferred rank order for the encryption algorithms; from the most preferred to the least.
		/// </summary>
		/// <remarks>
		/// Gets the preferred rank order for the encryption algorithms; from the most preferred to the least.
		/// </remarks>
		/// <value>The preferred encryption algorithm ranking.</value>
		protected EncryptionAlgorithm[] EncryptionAlgorithmRank {
			get { return encryptionAlgorithmRank; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (value.Length == 0)
					throw new ArgumentException ("The array of encryption algorithms cannot be empty.", nameof (value));

				encryptionAlgorithmRank = value;
			}
		}

		/// <summary>
		/// Get the enabled encryption algorithms in ranked order.
		/// </summary>
		/// <remarks>
		/// Gets the enabled encryption algorithms in ranked order.
		/// </remarks>
		/// <value>The enabled encryption algorithms.</value>
		public EncryptionAlgorithm[] EnabledEncryptionAlgorithms {
			get {
				var algorithms = new List<EncryptionAlgorithm> ();

				foreach (var algorithm in EncryptionAlgorithmRank) {
					if (IsEnabled (algorithm))
						algorithms.Add (algorithm);
				}

				return algorithms.ToArray ();
			}
		}

		/// <summary>
		/// Enable the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Enables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		public void Enable (EncryptionAlgorithm algorithm)
		{
			enabledEncryptionAlgorithms |= 1 << (int) algorithm;
		}

		/// <summary>
		/// Disable the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Disables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		public void Disable (EncryptionAlgorithm algorithm)
		{
			enabledEncryptionAlgorithms &= ~(1 << (int) algorithm);
		}

		/// <summary>
		/// Check whether the specified encryption algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified encryption algorithm is enabled.
		/// </remarks>
		/// <returns><c>true</c> if the specified encryption algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		public bool IsEnabled (EncryptionAlgorithm algorithm)
		{
			return (enabledEncryptionAlgorithms & (1 << (int) algorithm)) != 0;
		}

		/// <summary>
		/// Get the preferred rank order for the digest algorithms; from the most preferred to the least.
		/// </summary>
		/// <remarks>
		/// Gets the preferred rank order for the digest algorithms; from the most preferred to the least.
		/// </remarks>
		/// <value>The preferred encryption algorithm ranking.</value>
		protected DigestAlgorithm[] DigestAlgorithmRank {
			get { return digestAlgorithmRank; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (value.Length == 0)
					throw new ArgumentException ("The array of digest algorithms cannot be empty.", nameof (value));

				digestAlgorithmRank = value;
			}
		}

		/// <summary>
		/// Get the enabled digest algorithms in ranked order.
		/// </summary>
		/// <remarks>
		/// Gets the enabled digest algorithms in ranked order.
		/// </remarks>
		/// <value>The enabled encryption algorithms.</value>
		public DigestAlgorithm[] EnabledDigestAlgorithms {
			get {
				var algorithms = new List<DigestAlgorithm> ();

				foreach (var algorithm in DigestAlgorithmRank) {
					if (IsEnabled (algorithm))
						algorithms.Add (algorithm);
				}

				return algorithms.ToArray ();
			}
		}

		/// <summary>
		/// Enable the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Enables the digest algorithm.
		/// </remarks>
		/// <param name="algorithm">The digest algorithm.</param>
		public void Enable (DigestAlgorithm algorithm)
		{
			enabledDigestAlgorithms |= 1 << (int) algorithm;
		}

		/// <summary>
		/// Disable the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Disables the digest algorithm.
		/// </remarks>
		/// <param name="algorithm">The digest algorithm.</param>
		public void Disable (DigestAlgorithm algorithm)
		{
			enabledDigestAlgorithms &= ~(1 << (int) algorithm);
		}

		/// <summary>
		/// Check whether the specified digest algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified digest algorithm is enabled.
		/// </remarks>
		/// <returns><c>true</c> if the specified digest algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">The digest algorithm.</param>
		public bool IsEnabled (DigestAlgorithm algorithm)
		{
			return (enabledDigestAlgorithms & (1 << (int) algorithm)) != 0;
		}

		/// <summary>
		/// Check whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
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
		public abstract bool Supports (string protocol);

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
		public abstract string GetDigestAlgorithmName (DigestAlgorithm micalg);

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
		public abstract DigestAlgorithm GetDigestAlgorithm (string micalg);

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
		public abstract bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default);

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
		public virtual Task<bool> CanSignAsync (MailboxAddress signer, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (CanSign (signer, cancellationToken));
		}

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
		public abstract bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default);

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
		public virtual Task<bool> CanEncryptAsync (MailboxAddress mailbox, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (CanEncrypt (mailbox, cancellationToken));
		}

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
		public abstract MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default);

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
		public abstract Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default);

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
		public abstract DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default);

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
		public abstract Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default);

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
		public abstract MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default);

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
		public abstract Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default);

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
		public abstract MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default);

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
		public abstract Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default);

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
		public abstract void Import (Stream stream, CancellationToken cancellationToken = default);

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
		public abstract Task ImportAsync (Stream stream, CancellationToken cancellationToken = default);

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
		public abstract MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default);

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
		public abstract Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default);

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="CryptographyContext"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="CryptographyContext"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
		/// Releases all resources used by the <see cref="CryptographyContext"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="CryptographyContext"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="CryptographyContext"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="CryptographyContext"/> so
		/// the garbage collector can reclaim the memory that the <see cref="CryptographyContext"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Creates a new <see cref="CryptographyContext"/> for the specified protocol.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CryptographyContext"/> for the specified protocol.</para>
		/// <para>The default <see cref="CryptographyContext"/> types can over overridden by calling
		/// the <see cref="Register(Type)"/> method with the preferred type.</para>
		/// </remarks>
		/// <returns>The <see cref="CryptographyContext"/> for the protocol.</returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// There are no supported <see cref="CryptographyContext"/>s that support 
		/// the specified <paramref name="protocol"/>.
		/// </exception>
		public static CryptographyContext Create (string protocol)
		{
			if (protocol == null)
				throw new ArgumentNullException (nameof (protocol));

			protocol = protocol.ToLowerInvariant ();

			lock (mutex) {
				switch (protocol) {
				case "application/x-pkcs7-signature":
				case "application/pkcs7-signature":
				case "application/x-pkcs7-mime":
				case "application/pkcs7-mime":
				case "application/x-pkcs7-keys":
				case "application/pkcs7-keys":
					if (SecureMimeContextFactory != null)
						return SecureMimeContextFactory ();

					return new DefaultSecureMimeContext ();
				case "application/x-pgp-signature":
				case "application/pgp-signature":
				case "application/x-pgp-encrypted":
				case "application/pgp-encrypted":
				case "application/x-pgp-keys":
				case "application/pgp-keys":
					if (PgpContextFactory != null)
						return PgpContextFactory ();

					throw new NotSupportedException (string.Format (SubclassAndRegisterFormat, "MimeKit.Cryptography.OpenPgpContext or MimeKit.Cryptography.GnuPGContext"));
				default:
					throw new NotSupportedException ();
				}
			}
		}

		/// <summary>
		/// Registers a default <see cref="SecureMimeContext"/> or <see cref="OpenPgpContext"/>.
		/// </summary>
		/// <remarks>
		/// Registers the specified type as the default <see cref="SecureMimeContext"/> or
		/// <see cref="OpenPgpContext"/>.
		/// </remarks>
		/// <param name="type">A custom subclass of <see cref="SecureMimeContext"/> or
		/// <see cref="OpenPgpContext"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="type"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="type"/> is not a subclass of
		/// <see cref="SecureMimeContext"/> or <see cref="OpenPgpContext"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="type"/> does not have a parameterless constructor.</para>
		/// </exception>
		public static void Register (Type type)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));

			var ctor = type.GetConstructor (Array.Empty<Type> ());
			var args = Array.Empty<object> ();

			if (ctor == null)
				throw new ArgumentException ("The specified type must have a parameterless constructor.", nameof (type));

			if (type.IsSubclassOf (typeof (SecureMimeContext))) {
				lock (mutex) {
					SecureMimeContextFactory = () => (SecureMimeContext) ctor.Invoke (args);
				}
			} else if (type.IsSubclassOf (typeof (OpenPgpContext))) {
				lock (mutex) {
					PgpContextFactory = () => (OpenPgpContext) ctor.Invoke (args);
				}
			} else {
				throw new ArgumentException ("The specified type must be a subclass of SecureMimeContext or OpenPgpContext.", nameof (type));
			}
		}

		/// <summary>
		/// Registers a default <see cref="SecureMimeContext"/> factory.
		/// </summary>
		/// <remarks>
		/// Registers a factory that will return a new instance of the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <param name="factory">A factory that creates a new instance of <see cref="SecureMimeContext"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="factory"/> is <c>null</c>.
		/// </exception>
		public static void Register (Func<SecureMimeContext> factory)
		{
			if (factory == null)
				throw new ArgumentNullException (nameof (factory));

			lock (mutex) {
				SecureMimeContextFactory = factory;
			}
		}

		/// <summary>
		/// Registers a default <see cref="OpenPgpContext"/> factory.
		/// </summary>
		/// <remarks>
		/// Registers a factory that will return a new instance of the default <see cref="OpenPgpContext"/>.
		/// </remarks>
		/// <param name="factory">A factory that creates a new instance of <see cref="OpenPgpContext"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="factory"/> is <c>null</c>.
		/// </exception>
		public static void Register (Func<OpenPgpContext> factory)
		{
			if (factory == null)
				throw new ArgumentNullException (nameof (factory));

			lock (mutex) {
				PgpContextFactory = factory;
			}
		}
	}
}
