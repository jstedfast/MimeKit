//
// WindowsSecureMimeContext.cs
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
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;

using MimeKit.IO;

using RealCmsSigner = System.Security.Cryptography.Pkcs.CmsSigner;
using RealCmsRecipient = System.Security.Cryptography.Pkcs.CmsRecipient;
using RealAlgorithmIdentifier = System.Security.Cryptography.Pkcs.AlgorithmIdentifier;
using RealSubjectIdentifierType = System.Security.Cryptography.Pkcs.SubjectIdentifierType;
using RealCmsRecipientCollection = System.Security.Cryptography.Pkcs.CmsRecipientCollection;
using RealX509KeyUsageFlags = System.Security.Cryptography.X509Certificates.X509KeyUsageFlags;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// An S/MIME cryptography context that uses <see cref="System.Security.Cryptography.X509Certificates.X509Store"/>
	/// for certificate storage and retrieval.
	/// </remarks>
	public class WindowsSecureMimeContext : SecureMimeContext
	{
		const X509KeyStorageFlags DefaultKeyStorageFlags = X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;

		/// <summary>
		/// Initialize a new instance of the <see cref="WindowsSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="WindowsSecureMimeContext"/>.
		/// </remarks>
		/// <param name="location">The X.509 store location.</param>
		public WindowsSecureMimeContext (StoreLocation location)
		{
			StoreLocation = location;

			// System.Security does not support Camellia...
			Disable (EncryptionAlgorithm.Camellia256);
			Disable (EncryptionAlgorithm.Camellia192);
			Disable (EncryptionAlgorithm.Camellia192);

			// ... or Blowfish/Twofish...
			Disable (EncryptionAlgorithm.Blowfish);
			Disable (EncryptionAlgorithm.Twofish);

			// ...or CAST5...
			Disable (EncryptionAlgorithm.Cast5);

			// ...or IDEA...
			Disable (EncryptionAlgorithm.Idea);

			// ...or SEED...
			Disable (EncryptionAlgorithm.Seed);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="WindowsSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Constructs an S/MIME context using the current user's X.509 store location.
		/// </remarks>
		public WindowsSecureMimeContext () : this (StoreLocation.CurrentUser)
		{
		}

		/// <summary>
		/// Gets the X.509 store location.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 store location.
		/// </remarks>
		/// <value>The store location.</value>
		public StoreLocation StoreLocation {
			get; private set;
		}

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
		public override bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			return GetSignerCertificate (signer) != null;
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
		public override bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			return GetRecipientCertificate (mailbox) != null;
		}

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Get the certificate for the specified recipient.
		/// </summary>
		/// <remarks>
		/// <para>Gets the certificate for the specified recipient.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address.</para>
		/// </remarks>
		/// <returns>The certificate to use for the recipient; otherwise, or <c>null</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		protected virtual X509Certificate2 GetRecipientCertificate (MailboxAddress mailbox)
		{
			var storeNames = new [] { StoreName.AddressBook, StoreName.My, StoreName.TrustedPeople };
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.UtcNow;

			foreach (var storeName in storeNames) {
				var store = new X509Store (storeName, StoreLocation);

				store.Open (OpenFlags.ReadOnly);

				try {
					foreach (var certificate in store.Certificates) {
						if (certificate.NotBefore > now || certificate.NotAfter < now)
							continue;

						var usage = certificate.Extensions[X509Extensions.KeyUsage.Id] as X509KeyUsageExtension;
						if (usage != null && (usage.KeyUsages & RealX509KeyUsageFlags.KeyEncipherment) == 0)
							continue;

						if (secure != null) {
							if (!certificate.Thumbprint.Equals (secure.Fingerprint, StringComparison.OrdinalIgnoreCase))
								continue;
						} else {
							var address = certificate.GetNameInfo (X509NameType.EmailName, false);

							if (!address.Equals (mailbox.Address, StringComparison.InvariantCultureIgnoreCase))
								continue;
						}

						return certificate;
					}
				} finally {
					store.Close ();
				}
			}

			return null;
		}

		/// <summary>
		/// Get the <see cref="System.Security.Cryptography.Pkcs.CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="System.Security.Cryptography.Pkcs.CmsRecipient"/> with
		/// the appropriate certificate for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual RealCmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			X509Certificate2 certificate;

			if ((certificate = GetRecipientCertificate (mailbox)) == null)
				throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");

			return new RealCmsRecipient (certificate);
		}

		/// <summary>
		/// Get a collection of <see cref="System.Security.Cryptography.Pkcs.CmsRecipient"/> for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Gets a collection of <see cref="System.Security.Cryptography.Pkcs.CmsRecipient"/> for the specified mailboxes.
		/// </remarks>
		/// <returns>A <see cref="CmsRecipientCollection"/>.</returns>
		/// <param name="mailboxes">The recipient mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the specified <paramref name="mailboxes"/> could not be found.
		/// </exception>
		RealCmsRecipientCollection GetCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			var collection = new RealCmsRecipientCollection ();

			foreach (var recipient in mailboxes)
				collection.Add (GetCmsRecipient (recipient));

			if (collection.Count == 0)
				throw new ArgumentException ("No recipients specified.", nameof (mailboxes));

			return collection;
		}

		static RealCmsRecipientCollection GetCmsRecipients (CmsRecipientCollection recipients)
		{
			var collection = new RealCmsRecipientCollection ();

			try {
				foreach (var recipient in recipients) {
					var certificate = new X509Certificate2 (recipient.Certificate.GetEncoded ());
					RealSubjectIdentifierType type;
					RealCmsRecipient real;

					if (recipient.RecipientIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
						type = RealSubjectIdentifierType.IssuerAndSerialNumber;
					else
						type = RealSubjectIdentifierType.SubjectKeyIdentifier;

#if NETCOREAPP3_0
					var padding = recipient.RsaEncryptionPadding?.AsRSAEncryptionPadding ();

					if (padding != null)
						real = new RealCmsRecipient (type, certificate, padding);
					else
						real = new RealCmsRecipient (type, certificate);
#else
					if (recipient.RsaEncryptionPadding?.Scheme == RsaEncryptionPaddingScheme.Oaep) {
						certificate.Dispose ();
						throw new NotSupportedException ("The RSAES-OAEP encryption padding scheme is not supported by the WindowsSecureMimeContext. You must use a subclass of BouncyCastleSecureMimeContext to get this feature.");
					}

					real = new RealCmsRecipient (type, certificate);
#endif

					collection.Add (real);
				}

				return collection;
			} catch {
				foreach (var recipient in collection)
					recipient.Certificate.Dispose ();
				throw;
			}
		}

		/// <summary>
		/// Get the certificate for the specified signer.
		/// </summary>
		/// <remarks>
		/// <para>Gets the certificate for the specified signer.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address.</para>
		/// </remarks>
		/// <returns>The certificate to use for the signer; otherwise, or <c>null</c>.</returns>
		/// <param name="mailbox">The signer's mailbox address.</param>
		protected virtual X509Certificate2 GetSignerCertificate (MailboxAddress mailbox)
		{
			var store = new X509Store (StoreName.My, StoreLocation);
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.UtcNow;

			store.Open (OpenFlags.ReadOnly);

			try {
				foreach (var certificate in store.Certificates) {
					if (certificate.NotBefore > now || certificate.NotAfter < now)
						continue;

					var usage = certificate.Extensions[X509Extensions.KeyUsage.Id] as X509KeyUsageExtension;
					if (usage != null && (usage.KeyUsages & (RealX509KeyUsageFlags.DigitalSignature | RealX509KeyUsageFlags.NonRepudiation)) == 0)
						continue;

					if (!certificate.HasPrivateKey)
						continue;

					if (secure != null) {
						if (!certificate.Thumbprint.Equals (secure.Fingerprint, StringComparison.OrdinalIgnoreCase))
							continue;
					} else {
						var address = certificate.GetNameInfo (X509NameType.EmailName, false);

						if (!address.Equals (mailbox.Address, StringComparison.InvariantCultureIgnoreCase))
							continue;
					}

					return certificate;
				}
			} finally {
				store.Close ();
			}

			return null;
		}

		AsnEncodedData GetSecureMimeCapabilities ()
		{
			var attr = GetSecureMimeCapabilitiesAttribute (false);

			return new AsnEncodedData (attr.AttrType.Id, attr.AttrValues[0].GetEncoded ());
		}

		RealCmsSigner GetRealCmsSigner (RealSubjectIdentifierType type, X509Certificate2 certificate, DigestAlgorithm digestAlgo)
		{
			var signer = new RealCmsSigner (type, certificate) {
				DigestAlgorithm = new Oid (GetDigestOid (digestAlgo)),
				IncludeOption = X509IncludeOption.ExcludeRoot
			};
			signer.SignedAttributes.Add (GetSecureMimeCapabilities ());
			signer.SignedAttributes.Add (new Pkcs9SigningTime ());
			return signer;
		}

		RealCmsSigner GetRealCmsSigner (CmsSigner signer)
		{
			if (signer.RsaSignaturePadding == RsaSignaturePadding.Pss)
				throw new NotSupportedException ("The RSASSA-PSS signature padding scheme is not supported by the WindowsSecureMimeContext. You must use a subclass of BouncyCastleSecureMimeContext to get this feature.");

			RealSubjectIdentifierType type;
			X509Certificate2 certificate;

			if (signer.SignerIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
				type = RealSubjectIdentifierType.IssuerAndSerialNumber;
			else
				type = RealSubjectIdentifierType.SubjectKeyIdentifier;

			if (signer.WindowsCertificate != null) {
				certificate = signer.WindowsCertificate;
			} else {
				using (var stream = new MemoryStream ()) {
					var chain = new X509CertificateEntry[signer.CertificateChain.Count];
					var key = new AsymmetricKeyEntry (signer.PrivateKey);
					var password = Guid.NewGuid ().ToString ();
					var random = new SecureRandom ();
					var pkcs12 = new Pkcs12StoreBuilder ().Build ();

					for (int i = 0; i < chain.Length; i++)
						chain[i] = new X509CertificateEntry (signer.CertificateChain[i]);

					pkcs12.SetKeyEntry ("signer", key, chain);
					pkcs12.Save (stream, password.ToCharArray (), random);

					var rawData = stream.ToArray ();

					certificate = new X509Certificate2 (rawData, password);
				}
			}

			return GetRealCmsSigner (type, certificate, signer.DigestAlgorithm);
		}

		/// <summary>
		/// Get the <see cref="System.Security.Cryptography.Pkcs.CmsSigner"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="System.Security.Cryptography.Pkcs.CmsSigner"/> with
		/// the appropriate signing certificate for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address for database lookups.</para>
		/// </remarks>
		/// <returns>A <see cref="System.Security.Cryptography.Pkcs.CmsSigner"/>.</returns>
		/// <param name="mailbox">The signer's mailbox address.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual RealCmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			X509Certificate2 certificate;

			if ((certificate = GetSignerCertificate (mailbox)) == null)
				throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");

			return GetRealCmsSigner (RealSubjectIdentifierType.IssuerAndSerialNumber, certificate, digestAlgo);
		}

		/// <summary>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </summary>
		/// <remarks>
		/// <para>Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.</para>
		/// <para>This method is called when decoding digital signatures that include S/MIME capabilities in the metadata, allowing custom
		/// implementations to update the X.509 certificate records with the list of preferred encryption algorithms specified by the
		/// sending client.</para>
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="algorithms">The encryption algorithm capabilities of the client (in preferred order).</param>
		/// <param name="timestamp">The timestamp.</param>
		protected virtual void UpdateSecureMimeCapabilities (X509Certificate2 certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
		{
			// TODO: implement this - should we add/update the X509Extension for S/MIME Capabilities?
		}

		static byte[] ReadAllBytes (Stream stream)
		{
			if (stream is MemoryBlockStream block)
				return block.ToArray ();

			if (stream is MemoryStream mem)
				return mem.ToArray ();

			using (var memory = new MemoryBlockStream ()) {
				stream.CopyTo (memory, 4096);
				return memory.ToArray ();
			}
		}

		static async Task<byte[]> ReadAllBytesAsync (Stream stream, CancellationToken cancellationToken)
		{
			if (stream is MemoryBlockStream block)
				return block.ToArray ();

			if (stream is MemoryStream mem)
				return mem.ToArray ();

			using (var memory = new MemoryBlockStream ()) {
				await stream.CopyToAsync (memory, 4096, cancellationToken).ConfigureAwait (false);
				return memory.ToArray ();
			}
		}

		static async Task<Stream> SignAsync (RealCmsSigner signer, Stream content, bool detach, bool doAsync, CancellationToken cancellationToken = default)
		{
			ContentInfo contentInfo;

			if (doAsync)
				contentInfo = new ContentInfo (await ReadAllBytesAsync (content, cancellationToken).ConfigureAwait (false));
			else
				contentInfo = new ContentInfo (ReadAllBytes (content));

			var signed = new SignedCms (contentInfo, detach);

			try {
				signed.ComputeSignature (signer, false);
			} catch (CryptographicException) {
				signer.IncludeOption = X509IncludeOption.EndCertOnly;
				signed.ComputeSignature (signer, false);
			}

			var signedData = signed.Encode ();

			return new MemoryStream (signedData, false);
		}

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetRealCmsSigner (signer);
			var signedData = SignAsync (real, content, false, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signedData);
		}

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetRealCmsSigner (signer);
			var signedData = await SignAsync (real, content, false, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signedData);
		}

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs and encapsulates the content using the specified signer.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetCmsSigner (signer, digestAlgo);
			var signedData = SignAsync (real, content, false, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signedData);
		}

		/// <summary>
		/// Asynchronously sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs and encapsulates the content using the specified signer.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetCmsSigner (signer, digestAlgo);
			var signedData = await SignAsync (real, content, false, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signedData);
		}

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetRealCmsSigner (signer);
			var signature = SignAsync (real, content, true, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Signature (signature);
		}

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Signature> SignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetRealCmsSigner (signer);
			var signature = await SignAsync (real, content, true, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Signature (signature);
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs the content using the specified signer.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);
			var signature = SignAsync (cmsSigner, content, true, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Signature (signature);
		}

		/// <summary>
		/// Asynchronously sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs the content using the specified signer.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);
			var signature = await SignAsync (cmsSigner, content, true, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Signature (signature);
		}

		/// <summary>
		/// Attempts to map a <see cref="System.Security.Cryptography.Oid"/>
		/// to a <see cref="DigestAlgorithm"/>.
		/// </summary>
		/// <remarks>
		/// Attempts to map a <see cref="System.Security.Cryptography.Oid"/>
		/// to a <see cref="DigestAlgorithm"/>.
		/// </remarks>
		/// <returns><c>true</c> if the algorithm identifier was successfully mapped; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">The algorithm identifier.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="identifier"/> is <c>null</c>.
		/// </exception>
		internal protected static bool TryGetDigestAlgorithm (Oid identifier, out DigestAlgorithm algorithm)
		{
			if (identifier == null)
				throw new ArgumentNullException (nameof (identifier));

			return TryGetDigestAlgorithm (identifier.Value, out algorithm);
		}

		DigitalSignatureCollection GetDigitalSignatures (SignedCms signed)
		{
			var signatures = new List<IDigitalSignature> ();

			foreach (var signerInfo in signed.SignerInfos) {
				var signature = new WindowsSecureMimeDigitalSignature (signerInfo);

				if (signerInfo.Certificate != null) {
					if (signature.EncryptionAlgorithms.Length > 0 && signature.CreationDate.Ticks != 0) {
						UpdateSecureMimeCapabilities (signerInfo.Certificate, signature.EncryptionAlgorithms, signature.CreationDate);
					} else {
						try {
							Import (signerInfo.Certificate);
						} catch {
						}
					}
				}

				signatures.Add (signature);
			}

			return new DigitalSignatureCollection (signatures);
		}

		/// <summary>
		/// Verify the specified content using the detached signature data.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signature data.
		/// </remarks>
		/// <returns>A list of the digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The detached signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			var contentInfo = new ContentInfo (ReadAllBytes (content));
			var signed = new SignedCms (contentInfo, true);

			signed.Decode (ReadAllBytes (signatureData));

			return GetDigitalSignatures (signed);
		}

		/// <summary>
		/// Asynchronously verify the specified content using the detached signature data.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signature data.
		/// </remarks>
		/// <returns>A list of the digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The detached signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			var contentInfo = new ContentInfo (await ReadAllBytesAsync (content, cancellationToken).ConfigureAwait (false));
			var signed = new SignedCms (contentInfo, true);

			signed.Decode (await ReadAllBytesAsync (signatureData, cancellationToken).ConfigureAwait (false));

			return GetDigitalSignatures (signed);
		}

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			var content = ReadAllBytes (signedData);
			var signed = new SignedCms ();

			signed.Decode (content);

			var memory = new MemoryStream (signed.ContentInfo.Content, false);

			try {
				entity = MimeEntity.Load (memory, true, cancellationToken);
			} catch {
				memory.Dispose ();
				throw;
			}

			return GetDigitalSignatures (signed);
		}

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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Stream Verify (Stream signedData, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			var content = ReadAllBytes (signedData);
			var signed = new SignedCms ();

			signed.Decode (content);

			signatures = GetDigitalSignatures (signed);

			return new MemoryStream (signed.ContentInfo.Content, false);
		}

		/// <summary>
		/// Gets the preferred encryption algorithm to use for encrypting to the specified recipients.
		/// </summary>
		/// <remarks>
		/// <para>Gets the preferred encryption algorithm to use for encrypting to the specified recipients
		/// based on the encryption algorithms supported by each of the recipients, the
		/// <see cref="CryptographyContext.EnabledEncryptionAlgorithms"/>, and the
		/// <see cref="CryptographyContext.EncryptionAlgorithmRank"/>.</para>
		/// <para>If the supported encryption algorithms are unknown for any recipient, it is assumed that
		/// the recipient supports at least the Triple-DES encryption algorithm.</para>
		/// </remarks>
		/// <returns>The preferred encryption algorithm.</returns>
		/// <param name="recipients">The recipients.</param>
		protected virtual EncryptionAlgorithm GetPreferredEncryptionAlgorithm (RealCmsRecipientCollection recipients)
		{
			var votes = new int[EncryptionAlgorithmCount];
			int need = recipients.Count;

			foreach (var recipient in recipients) {
				var supported = recipient.Certificate.GetEncryptionAlgorithms ();

				foreach (var algorithm in supported)
					votes[(int) algorithm]++;
			}

			// Starting with S/MIME v3 (published in 1999), Triple-DES is a REQUIRED algorithm.
			// S/MIME v2.x and older only required RC2/40, but SUGGESTED Triple-DES.
			// Considering the fact that Bruce Schneier was able to write a
			// screensaver that could crack RC2/40 back in the late 90's, let's
			// not default to anything weaker than Triple-DES...
			EncryptionAlgorithm chosen = EncryptionAlgorithm.TripleDes;
			int nvotes = 0;

			votes[(int) EncryptionAlgorithm.TripleDes] = need;

			// iterate through the algorithms, from strongest to weakest, keeping track
			// of the algorithm with the most amount of votes (between algorithms with
			// the same number of votes, choose the strongest of the 2 - i.e. the one
			// that we arrive at first).
			var algorithms = EncryptionAlgorithmRank;
			for (int i = 0; i < algorithms.Length; i++) {
				var algorithm = algorithms[i];

				if (!IsEnabled (algorithm))
					continue;

				if (votes[(int) algorithm] > nvotes) {
					nvotes = votes[(int) algorithm];
					chosen = algorithm;
				}
			}

			return chosen;
		}

		internal RealAlgorithmIdentifier GetAlgorithmIdentifier (EncryptionAlgorithm algorithm)
		{
			switch (algorithm) {
			case EncryptionAlgorithm.Aes256:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.Aes256Cbc));
			case EncryptionAlgorithm.Aes192:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.Aes192Cbc));
			case EncryptionAlgorithm.Aes128:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.Aes128Cbc));
			case EncryptionAlgorithm.TripleDes:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.DesEde3Cbc));
			case EncryptionAlgorithm.Des:
				return new RealAlgorithmIdentifier (new Oid (SmimeCapability.DesCbc.Id));
			case EncryptionAlgorithm.RC2128:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.RC2Cbc), 128);
			case EncryptionAlgorithm.RC264:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.RC2Cbc), 64);
			case EncryptionAlgorithm.RC240:
				return new RealAlgorithmIdentifier (new Oid (CmsEnvelopedGenerator.RC2Cbc), 40);
			default:
				throw new NotSupportedException (string.Format ("The {0} encryption algorithm is not supported by the {1}.", algorithm, GetType ().Name));
			}
		}

		async Task<Stream> EnvelopeAsync (RealCmsRecipientCollection recipients, Stream content, EncryptionAlgorithm encryptionAlgorithm, bool doAsync, CancellationToken cancellationToken)
		{
			ContentInfo contentInfo;

			if (doAsync)
				contentInfo = new ContentInfo (await ReadAllBytesAsync (content, cancellationToken).ConfigureAwait (false));
			else
				contentInfo = new ContentInfo (ReadAllBytes (content));

			var algorithm = GetAlgorithmIdentifier (encryptionAlgorithm);
			var envelopedData = new EnvelopedCms (contentInfo, algorithm);

			envelopedData.Encrypt (recipients);

			return new MemoryStream (envelopedData.Encode (), false);
		}

		Task<Stream> EnvelopeAsync (RealCmsRecipientCollection recipients, Stream content, bool doAsync, CancellationToken cancellationToken)
		{
			var algorithm = GetPreferredEncryptionAlgorithm (recipients);

			return EnvelopeAsync (recipients, content, algorithm, doAsync, cancellationToken);
		}

		Task<Stream> EnvelopeAsync (CmsRecipientCollection recipients, Stream content, bool doAsync, CancellationToken cancellationToken)
		{
			var algorithm = GetPreferredEncryptionAlgorithm (recipients);

			return EnvelopeAsync (GetCmsRecipients (recipients), content, algorithm, doAsync, cancellationToken);
		}

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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var envelopedData = EnvelopeAsync (recipients, content, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var envelopedData = await EnvelopeAsync (recipients, content, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A certificate for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetCmsRecipients (recipients);
			var envelopedData = EnvelopeAsync (real, content, false, cancellationToken).GetAwaiter ().GetResult ();

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

		/// <summary>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A certificate for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var real = GetCmsRecipients (recipients);
			var envelopedData = await EnvelopeAsync (real, content, true, cancellationToken);

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

		static async Task<MimeEntity> DecryptAsync (Stream encryptedData, bool doAsync, CancellationToken cancellationToken)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			var enveloped = new EnvelopedCms ();
			CryptographicException ce = null;
			byte[] content;

			if (doAsync)
				content = await ReadAllBytesAsync (encryptedData, cancellationToken).ConfigureAwait (false);
			else
				content = ReadAllBytes (encryptedData);

			enveloped.Decode (content);

			foreach (var recipient in enveloped.RecipientInfos) {
				try {
					enveloped.Decrypt (recipient);
					ce = null;
					break;
				} catch (CryptographicException ex) {
					ce = ex;
				}
			}

			if (ce != null)
				throw ce;

			var decryptedData = enveloped.Encode ();

			var memory = new MemoryStream (decryptedData, false);

			return MimeEntity.Load (memory, true, cancellationToken);
		}

		/// <summary>
		/// Decrypt the encrypted data.
		/// </summary>
		/// <remarks>
		/// Decrypt the encrypted data.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			return DecryptAsync (encryptedData, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously decrypt the encrypted data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypt the encrypted data.
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			return DecryptAsync (encryptedData, true, cancellationToken);
		}

		static async Task DecryptToAsync (Stream encryptedData, Stream decryptedData, bool doAsync, CancellationToken cancellationToken)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			if (decryptedData == null)
				throw new ArgumentNullException (nameof (decryptedData));

			var enveloped = new EnvelopedCms ();
			byte[] content;

			if (doAsync)
				content = await ReadAllBytesAsync (encryptedData, cancellationToken).ConfigureAwait (false);
			else
				content = ReadAllBytes (encryptedData);

			enveloped.Decode (content);
			enveloped.Decrypt ();

			var encoded = enveloped.Encode ();

			if (doAsync)
				await decryptedData.WriteAsync (encoded, 0, encoded.Length, cancellationToken).ConfigureAwait (false);
			else
				decryptedData.Write (encoded, 0, encoded.Length);
		}

		/// <summary>
		/// Decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The decrypted data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void DecryptTo (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default)
		{
			DecryptToAsync (encryptedData, decryptedData, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The decrypted data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task DecryptToAsync (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default)
		{
			return DecryptToAsync (encryptedData, decryptedData, false, cancellationToken);
		}

		/// <summary>
		/// Import the specified certificate.
		/// </summary>
		/// <remarks>
		/// Import the specified certificate.
		/// </remarks>
		/// <param name="storeName">The store to import the certificate into.</param>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public void Import (StoreName storeName, X509Certificate2 certificate, CancellationToken cancellationToken = default)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			cancellationToken.ThrowIfCancellationRequested ();

			var store = new X509Store (storeName, StoreLocation);

			store.Open (OpenFlags.ReadWrite);
			store.Add (certificate);
			store.Close ();
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate into the <see cref="StoreName.AddressBook"/> store.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override void Import (X509Certificate2 certificate, CancellationToken cancellationToken = default)
		{
			Import (StoreName.AddressBook, certificate, cancellationToken);
		}

		/// <summary>
		/// Import the specified certificate.
		/// </summary>
		/// <remarks>
		/// Import the specified certificate.
		/// </remarks>
		/// <param name="storeName">The store to import the certificate into.</param>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public void Import (StoreName storeName, Org.BouncyCastle.X509.X509Certificate certificate, CancellationToken cancellationToken = default)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			using (var certificate2 = new X509Certificate2 (certificate.GetEncoded ()))
				Import (storeName, certificate2, cancellationToken);
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate into the <see cref="StoreName.AddressBook"/> store.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override void Import (Org.BouncyCastle.X509.X509Certificate certificate, CancellationToken cancellationToken = default)
		{
			Import (StoreName.AddressBook, certificate, cancellationToken);
		}

		static string GetSerialNumber (X509CrlEntry crlEntry)
		{
			return crlEntry.SerialNumber.ToString ();
		}

		static X500DistinguishedName GetIssuerName (X509CrlEntry crlEntry)
		{
			var issuer = crlEntry.GetCertificateIssuer ();
			var rawData = issuer.GetEncoded ();

			return new X500DistinguishedName (rawData);
		}

		/// <summary>
		/// Import a certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports a certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override void Import (X509Crl crl, CancellationToken cancellationToken = default)
		{
			if (crl == null)
				throw new ArgumentNullException (nameof (crl));

			cancellationToken.ThrowIfCancellationRequested ();

			var revoked = crl.GetRevokedCertificates ();

			if (revoked == null)
				return;

			var addressbook = new X509Store (StoreName.AddressBook, StoreLocation);
			var disallowed = new X509Store (StoreName.Disallowed, StoreLocation);

			addressbook.Open (OpenFlags.ReadWrite);
			disallowed.Open (OpenFlags.ReadWrite);

			try {
				foreach (var crlEntry in revoked) {
					var serialNumber = GetSerialNumber (crlEntry);
					var issuer = GetIssuerName (crlEntry);

					var matching = addressbook.Certificates.Find (X509FindType.FindBySerialNumber, serialNumber, false);
					matching = matching.Find (X509FindType.FindByIssuerDistinguishedName, issuer, false);

					for (int i = 0; i < matching.Count; i++) {
						addressbook.Remove (matching[i]);
						disallowed.Add (matching[i]);
					}
				}
			} finally {
				addressbook.Close ();
				disallowed.Close ();
			}
		}

		/// <summary>
		/// Import certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="flags">The storage flags to use when importing the certificate and private key.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public void Import (Stream stream, string password, X509KeyStorageFlags flags, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			cancellationToken.ThrowIfCancellationRequested ();

			var rawData = ReadAllBytes (stream);
			var store = new X509Store (StoreName.My, StoreLocation);
			var certs = new X509Certificate2Collection ();

			store.Open (OpenFlags.ReadWrite);
			certs.Import (rawData, password, flags);
			store.AddRange (certs);
			store.Close ();
		}

		/// <summary>
		/// Asynchronously import certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="flags">The storage flags to use when importing the certificate and private key.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public async Task ImportAsync (Stream stream, string password, X509KeyStorageFlags flags, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			cancellationToken.ThrowIfCancellationRequested ();

			var rawData = await ReadAllBytesAsync (stream, cancellationToken).ConfigureAwait (false);
			var store = new X509Store (StoreName.My, StoreLocation);
			var certs = new X509Certificate2Collection ();

			store.Open (OpenFlags.ReadWrite);
			certs.Import (rawData, password, flags);
			store.AddRange (certs);
			store.Close ();
		}

		/// <summary>
		/// Import certificates and keys from a pkcs12-encoded stream.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override void Import (Stream stream, string password, CancellationToken cancellationToken = default)
		{
			Import (stream, password, DefaultKeyStorageFlags, cancellationToken);
		}

		/// <summary>
		/// Asynchronously import certificates and keys from a pkcs12-encoded stream.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override Task ImportAsync (Stream stream, string password, CancellationToken cancellationToken = default)
		{
			return ImportAsync (stream, password, DefaultKeyStorageFlags, cancellationToken);
		}

		/// <summary>
		/// Exports the certificates for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the certificates for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// No mailboxes were specified.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			cancellationToken.ThrowIfCancellationRequested ();

			var certificates = new X509CertificateStore ();
			int count = 0;

			foreach (var mailbox in mailboxes) {
				var certificate = GetRecipientCertificate (mailbox);

				if (certificate != null)
					certificates.Add (certificate.AsBouncyCastleCertificate ());

				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No mailboxes specified.", nameof (mailboxes));

			var cms = new CmsSignedDataStreamGenerator ();
			cms.AddCertificates (certificates);

			var memory = new MemoryBlockStream ();
			cms.Open (memory).Dispose ();
			memory.Position = 0;

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, memory);
		}

		/// <summary>
		/// Asynchronously export the certificates for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Asynchronously exports the certificates for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// No mailboxes were specified.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (Export (mailboxes, cancellationToken));
		}
		#endregion
	}
}
