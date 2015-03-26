//
// WindowsSecureMimeContext.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

using RealCmsSigner = System.Security.Cryptography.Pkcs.CmsSigner;
using RealCmsRecipient = System.Security.Cryptography.Pkcs.CmsRecipient;
using RealCmsRecipientCollection = System.Security.Cryptography.Pkcs.CmsRecipientCollection;
using RealX509KeyUsageFlags = System.Security.Cryptography.X509Certificates.X509KeyUsageFlags;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME cryptography context that uses Windows' <see cref="System.Security.Cryptography.X509Certificates.X509Store"/>.
	/// </summary>
	/// <remarks>
	/// An S/MIME cryptography context that uses Windows' <see cref="System.Security.Cryptography.X509Certificates.X509Store"/>.
	/// </remarks>
	public class WindowsSecureMimeContext : SecureMimeContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.WindowsSecureMimeContext"/> class.
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

			// ...or CAST5...
			Disable (EncryptionAlgorithm.Cast5);

			// ...or IDEA...
			Disable (EncryptionAlgorithm.Idea);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.WindowsSecureMimeContext"/> class.
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

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate based on the selector.
		/// </remarks>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override Org.BouncyCastle.X509.X509Certificate GetCertificate (IX509Selector selector)
		{
			var storeNames = new [] { StoreName.My, StoreName.AddressBook, StoreName.TrustedPeople, StoreName.Root };

			foreach (var storeName in storeNames) {
				var store = new X509Store (storeName, StoreLocation);

				store.Open (OpenFlags.ReadOnly);

				try {
					foreach (var certificate in store.Certificates) {
						var cert = DotNetUtilities.FromX509Certificate (certificate);
						if (selector == null || selector.Match (cert))
							return cert;
					}
				} finally {
					store.Close ();
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the private key based on the provided selector.
		/// </summary>
		/// <remarks>
		/// Gets the private key based on the provided selector.
		/// </remarks>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (IX509Selector selector)
		{
			var store = new X509Store (StoreName.My, StoreLocation);

			store.Open (OpenFlags.ReadOnly);

			try {
				foreach (var certificate in store.Certificates) {
					if (!certificate.HasPrivateKey)
						continue;

					var cert = DotNetUtilities.FromX509Certificate (certificate);

					if (selector == null || selector.Match (cert)) {
						var pair = DotNetUtilities.GetKeyPair (certificate.PrivateKey);
						return pair.Private;
					}
				}
			} finally {
				store.Close ();
			}

			return null;
		}

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <remarks>
		/// Gets the trusted anchors.
		/// </remarks>
		/// <returns>The trusted anchors.</returns>
		protected override Org.BouncyCastle.Utilities.Collections.HashSet GetTrustedAnchors ()
		{
			var storeNames = new StoreName[] { StoreName.TrustedPeople, StoreName.Root };
			var anchors = new Org.BouncyCastle.Utilities.Collections.HashSet ();

			foreach (var storeName in storeNames) {
				var store = new X509Store (storeName, StoreLocation);

				store.Open (OpenFlags.ReadOnly);

				foreach (var certificate in store.Certificates) {
					var cert = DotNetUtilities.FromX509Certificate (certificate);
					anchors.Add (new TrustAnchor (cert, null));
				}

				store.Close ();
			}

			return anchors;
		}

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <remarks>
		/// Gets the intermediate certificates.
		/// </remarks>
		/// <returns>The intermediate certificates.</returns>
		protected override IX509Store GetIntermediateCertificates ()
		{
			var storeNames = new [] { StoreName.My, StoreName.AddressBook, StoreName.TrustedPeople, StoreName.Root };
			var intermediate = new X509CertificateStore ();

			foreach (var storeName in storeNames) {
				var store = new X509Store (storeName, StoreLocation);

				store.Open (OpenFlags.ReadOnly);

				foreach (var certificate in store.Certificates) {
					var cert = DotNetUtilities.FromX509Certificate (certificate);
					intermediate.Add (cert);
				}

				store.Close ();
			}

			return intermediate;
		}

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// Gets the certificate revocation lists.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected override IX509Store GetCertificateRevocationLists ()
		{
			// TODO: figure out how other Windows apps keep track of CRLs...
			var crls = new List<X509Crl> ();

			return X509StoreFactory.Create ("Crl/Collection", new X509CollectionStoreParameters (crls));
		}

		X509Certificate2 GetCmsRecipientCertificate (MailboxAddress mailbox)
		{
			var store = new X509Store (StoreName.My, StoreLocation);
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.Now;

			store.Open (OpenFlags.ReadOnly);

			try {
				foreach (var certificate in store.Certificates) {
					if (certificate.NotBefore > now || certificate.NotAfter < now)
						continue;

					var usage = certificate.Extensions[X509Extensions.KeyUsage.Id] as X509KeyUsageExtension;
					if (usage != null && (usage.KeyUsages & RealX509KeyUsageFlags.KeyEncipherment) == 0)
						continue;

					if (secure != null) {
						if (certificate.Thumbprint != secure.Fingerprint)
							continue;
					} else {
						if (certificate.GetNameInfo (X509NameType.EmailName, false) != mailbox.Address)
							continue;
					}

					return certificate;
				}
			} finally {
				store.Close ();
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (byte[] rawData)
		{
			using (var memory = new MemoryStream (rawData, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					var algorithms = new List<EncryptionAlgorithm> ();
					var sequence = asn1.ReadObject () as Asn1Sequence;

					if (sequence == null)
						return null;

					for (int i = 0; i < sequence.Count; i++) {
						var identifier = Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier.GetInstance (sequence[i]);
						EncryptionAlgorithm algorithm;

						if (TryGetEncryptionAlgorithm (identifier, out algorithm))
							algorithms.Add (algorithm);
					}

					return algorithms.ToArray ();
				}
			}
		}

		/// <summary>
		/// Gets the X.509 certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </remarks>
		/// <returns>The certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			var certificate = GetCmsRecipientCertificate (mailbox);
			var cert = DotNetUtilities.FromX509Certificate (certificate);
			var recipient = new CmsRecipient (cert);

			foreach (var extension in certificate.Extensions) {
				if (extension.Oid.Value == "1.2.840.113549.1.9.15") {
					var algorithms = DecodeEncryptionAlgorithms (extension.RawData);

					if (algorithms != null)
						recipient.EncryptionAlgorithms = algorithms;

					break;
				}
			}

			return recipient;
		}

		RealCmsRecipient GetRealCmsRecipient (MailboxAddress mailbox)
		{
			return new RealCmsRecipient (GetCmsRecipientCertificate (mailbox));
		}

		RealCmsRecipientCollection GetRealCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			var recipients = new RealCmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetRealCmsRecipient (mailbox));

			return recipients;
		}

		X509Certificate2 GetCmsSignerCertificate (MailboxAddress mailbox)
		{
			var store = new X509Store (StoreName.My, StoreLocation);
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.Now;

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
						if (certificate.Thumbprint != secure.Fingerprint)
							continue;
					} else {
						if (certificate.GetNameInfo (X509NameType.EmailName, false) != mailbox.Address)
							continue;
					}

					return certificate;
				}
			} finally {
				store.Close ();
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

		/// <summary>
		/// Gets the cms signer for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <remarks>
		/// Gets the cms signer for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </remarks>
		/// <returns>The cms signer.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			var certificate = GetCmsSignerCertificate (mailbox);
			var pair = DotNetUtilities.GetKeyPair (certificate.PrivateKey);
			var cert = DotNetUtilities.FromX509Certificate (certificate);
			var signer = new CmsSigner (cert, pair.Private);
			signer.DigestAlgorithm = digestAlgo;
			return signer;
		}

		RealCmsSigner GetRealCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			var signer = new RealCmsSigner (GetCmsSignerCertificate (mailbox));
			signer.DigestAlgorithm = new Oid (GetDigestOid (digestAlgo));
			signer.SignedAttributes.Add (new Pkcs9SigningTime ());
			signer.IncludeOption = X509IncludeOption.ExcludeRoot;
			return signer;
		}

		/// <summary>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </summary>
		/// <remarks>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="algorithms">The encryption algorithm capabilities of the client (in preferred order).</param>
		/// <param name="timestamp">The timestamp.</param>
		protected override void UpdateSecureMimeCapabilities (Org.BouncyCastle.X509.X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
		{
			// TODO: implement this - should we add/update the X509Extension for S/MIME Capabilities?
		}

		static byte[] ReadAllBytes (Stream stream)
		{
			if (stream is MemoryBlockStream)
				return ((MemoryBlockStream) stream).ToArray ();

			if (stream is MemoryStream)
				return ((MemoryStream) stream).ToArray ();

			using (var memory = new MemoryBlockStream ()) {
				stream.CopyTo (memory, 4096);
				return memory.ToArray ();
			}
		}

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Sign and encapsulate the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
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
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var contentInfo = new ContentInfo (ReadAllBytes (content));
			var cmsSigner = GetRealCmsSigner (signer, digestAlgo);
			var signed = new SignedCms (contentInfo, false);
			signed.ComputeSignature (cmsSigner);
			var signedData = signed.Encode ();

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, new MemoryStream (signedData, false));
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Sign the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
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
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var contentInfo = new ContentInfo (ReadAllBytes (content));
			var cmsSigner = GetRealCmsSigner (signer, digestAlgo);
			var signed = new SignedCms (contentInfo, true);
			signed.ComputeSignature (cmsSigner);
			var signedData = signed.Encode ();

			return new ApplicationPkcs7Signature (new MemoryStream (signedData, false));
		}

		/// <summary>
		/// Decrypt the encrypted data.
		/// </summary>
		/// <remarks>
		/// Decrypt the encrypted data.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new EnvelopedCms ();

			enveloped.Decode (ReadAllBytes (encryptedData));
			enveloped.Decrypt ();

			var decryptedData = enveloped.Encode ();

			var memory = new MemoryStream (decryptedData, false);

			return MimeEntity.Load (memory, true);
		}

		/// <summary>
		/// Import the specified certificate.
		/// </summary>
		/// <remarks>
		/// Import the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public override void Import (Org.BouncyCastle.X509.X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			var store = new X509Store (StoreName.AddressBook, StoreLocation);

			store.Open (OpenFlags.ReadWrite);
			store.Add (new X509Certificate2 (certificate.GetEncoded ()));
			store.Close ();
		}

		/// <summary>
		/// Import the specified certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Import the specified certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			// TODO: figure out where to store the CRLs...
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public override void Import (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			var rawData = ReadAllBytes (stream);
			var store = new X509Store (StoreName.My, StoreLocation);
			var certs = new X509Certificate2Collection ();

			store.Open (OpenFlags.ReadWrite);
			certs.Import (rawData, password, X509KeyStorageFlags.UserKeySet);
			store.AddRange (certs);
			store.Close ();
		}

		#endregion
	}
}
