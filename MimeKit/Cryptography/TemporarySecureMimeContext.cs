//
// TemporarySecureMimeContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Asn1.X509;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME context that does not persist certificates, private keys or CRLs.
	/// </summary>
	/// <remarks>
	/// A <see cref="TemporarySecureMimeContext"/> is a special S/MIME context that
	/// does not use a persistent store for certificates, private keys, or CRLs.
	/// Instead, certificates, private keys, and CRLs are maintained in memory only.
	/// </remarks>
	public class TemporarySecureMimeContext : BouncyCastleSecureMimeContext
	{
		readonly Dictionary<string, EncryptionAlgorithm[]> capabilities;
		internal readonly Dictionary<string, AsymmetricKeyParameter> keys;
		internal readonly List<X509Certificate> certificates;
		readonly HashSet<string> fingerprints;
		readonly List<X509Crl> crls;

		/// <summary>
		/// Initialize a new instance of the <see cref="TemporarySecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TemporarySecureMimeContext"/>.
		/// </remarks>
		public TemporarySecureMimeContext ()
		{
			capabilities = new Dictionary<string, EncryptionAlgorithm[]> (StringComparer.Ordinal);
			keys = new Dictionary<string, AsymmetricKeyParameter> (StringComparer.Ordinal);
			certificates = new List<X509Certificate> ();
			fingerprints = new HashSet<string> ();
			crls = new List<X509Crl> ();
		}

		/// <summary>
		/// Check whether or not a particular mailbox address can be used for signing.
		/// </summary>
		/// <remarks>
		/// Checks whether or not as particular mailbocx address can be used for signing.
		/// </remarks>
		/// <returns><c>true</c> if the mailbox address can be used for signing; otherwise, <c>false</c>.</returns>
		/// <param name="signer">The signer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		public override bool CanSign (MailboxAddress signer)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			AsymmetricKeyParameter key;

			return GetCmsSignerCertificate (signer, out key) != null;
		}

		/// <summary>
		/// Check whether or not the cryptography context can encrypt to a particular recipient.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the cryptography context can be used to encrypt to a particular recipient.
		/// </remarks>
		/// <returns><c>true</c> if the cryptography context can be used to encrypt to the designated recipient; otherwise, <c>false</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public override bool CanEncrypt (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			return GetCmsRecipientCertificate (mailbox) != null;
		}

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			if (selector == null && certificates.Count > 0)
				return certificates[0];

			foreach (var certificate in certificates) {
				if (selector.Match (certificate))
					return certificate;
			}

			return null;
		}

		/// <summary>
		/// Gets the private key for the certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the private key for the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (IX509Selector selector)
		{
			foreach (var certificate in certificates) {
				var fingerprint = certificate.GetFingerprint ();
				AsymmetricKeyParameter key;

				if (!keys.TryGetValue (fingerprint, out key))
					continue;

				if (selector != null && !selector.Match (certificate))
					continue;

				return key;
			}

			return null;
		}

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <remarks>
		/// A trusted anchor is a trusted root-level X.509 certificate,
		/// generally issued by a certificate authority (CA).
		/// </remarks>
		/// <returns>The trusted anchors.</returns>
		protected override Org.BouncyCastle.Utilities.Collections.HashSet GetTrustedAnchors ()
		{
			var anchors = new Org.BouncyCastle.Utilities.Collections.HashSet ();

			foreach (var certificate in certificates) {
				var keyUsage = certificate.GetKeyUsage ();

				if (keyUsage != null && keyUsage[(int) X509KeyUsageBits.KeyCertSign] && certificate.IsSelfSigned ())
					anchors.Add (new TrustAnchor (certificate, null));
			}

			return anchors;
		}

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <remarks>
		/// An intermediate certificate is any certificate that exists between the root
		/// certificate issued by a Certificate Authority (CA) and the certificate at
		/// the end of the chain.
		/// </remarks>
		/// <returns>The intermediate certificates.</returns>
		protected override IX509Store GetIntermediateCertificates ()
		{
			var intermediates = new X509CertificateStore ();

			foreach (var certificate in certificates) {
				var keyUsage = certificate.GetKeyUsage ();

				if (keyUsage != null && keyUsage[(int) X509KeyUsageBits.KeyCertSign] && !certificate.IsSelfSigned ())
					intermediates.Add (certificate);
			}

			return intermediates;
		}

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// A Certificate Revocation List (CRL) is a list of certificate serial numbers issued
		/// by a particular Certificate Authority (CA) that have been revoked, either by the CA
		/// itself or by the owner of the revoked certificate.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected override IX509Store GetCertificateRevocationLists ()
		{
			return X509StoreFactory.Create ("Crl/Collection", new X509CollectionStoreParameters (crls));
		}

		/// <summary>
		/// Get the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </summary>
		/// <remarks>
		/// Gets the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </remarks>
		/// <returns>The date &amp; time for the next update.</returns>
		/// <param name="issuer">The issuer.</param>
		protected override DateTime GetNextCertificateRevocationListUpdate (X509Name issuer)
		{
			var nextUpdate = DateTime.MinValue.ToUniversalTime ();

			foreach (var crl in crls) {
				if (!crl.IssuerDN.Equals (issuer))
					continue;

				nextUpdate = crl.NextUpdate.Value > nextUpdate ? crl.NextUpdate.Value : nextUpdate;
			}

			return nextUpdate;
		}

		X509Certificate GetCmsRecipientCertificate (MailboxAddress mailbox)
		{
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.UtcNow;

			foreach (var certificate in certificates) {
				if (certificate.NotBefore > now || certificate.NotAfter < now)
					continue;

				var keyUsage = certificate.GetKeyUsageFlags ();
				if (keyUsage != 0 && (keyUsage & X509KeyUsageFlags.KeyEncipherment) == 0)
					continue;

				if (secure != null) {
					var fingerprint = certificate.GetFingerprint ();

					if (!fingerprint.Equals (secure.Fingerprint, StringComparison.OrdinalIgnoreCase))
						continue;
				} else {
					var address = certificate.GetSubjectEmailAddress ();

					if (!address.Equals (mailbox.Address, StringComparison.OrdinalIgnoreCase))
						continue;
				}

				return certificate;
			}

			return null;
		}

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsRecipient"/> with the appropriate certificate and
		/// <see cref="CmsRecipient.EncryptionAlgorithms"/> for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			X509Certificate certificate;

			if ((certificate = GetCmsRecipientCertificate (mailbox)) == null)
				throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");

			var recipient = new CmsRecipient (certificate);
			EncryptionAlgorithm[] algorithms;

			if (capabilities.TryGetValue (certificate.GetFingerprint (), out algorithms))
				recipient.EncryptionAlgorithms = algorithms;

			return recipient;
		}

		X509Certificate GetCmsSignerCertificate (MailboxAddress mailbox, out AsymmetricKeyParameter key)
		{
			var secure = mailbox as SecureMailboxAddress;
			var now = DateTime.UtcNow;

			foreach (var certificate in certificates) {
				if (certificate.NotBefore > now || certificate.NotAfter < now)
					continue;

				var keyUsage = certificate.GetKeyUsageFlags ();
				if (keyUsage != 0 && (keyUsage & DigitalSignatureKeyUsageFlags) == 0)
					continue;

				var fingerprint = certificate.GetFingerprint ();

				if (!keys.TryGetValue (fingerprint, out key))
					continue;

				if (secure != null) {
					if (!fingerprint.Equals (secure.Fingerprint, StringComparison.OrdinalIgnoreCase))
						continue;
				} else {
					var address = certificate.GetSubjectEmailAddress ();

					if (!address.Equals (mailbox.Address, StringComparison.OrdinalIgnoreCase))
						continue;
				}

				return certificate;
			}

			key = null;

			return null;
		}

		/// <summary>
		/// Gets the <see cref="CmsSigner"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsSigner"/> with the appropriate signing certificate
		/// for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address for database lookups.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsSigner"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			X509Certificate certificate;
			AsymmetricKeyParameter key;

			if ((certificate = GetCmsSignerCertificate (mailbox, out key)) == null)
				throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");

			return new CmsSigner (BuildCertificateChain (certificate), key) {
				DigestAlgorithm = digestAlgo
			};
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
		protected override void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
		{
			capabilities[certificate.GetFingerprint ()] = algorithms;
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		public override void Import (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

			foreach (string alias in pkcs12.Aliases) {
				if (pkcs12.IsKeyEntry (alias)) {
					var chain = pkcs12.GetCertificateChain (alias);
					var entry = pkcs12.GetKey (alias);

					for (int i = 0; i < chain.Length; i++)
						Import (chain[i].Certificate);

					var fingerprint = chain[0].Certificate.GetFingerprint ();
					if (!keys.ContainsKey (fingerprint))
						keys.Add (fingerprint, entry.Key);
				} else if (pkcs12.IsCertificateEntry (alias)) {
					var entry = pkcs12.GetCertificate (alias);

					Import (entry.Certificate);
				}
			}
		}

		/// <summary>
		/// Imports the specified certificate.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (fingerprints.Add (certificate.GetFingerprint ()))
				certificates.Add (certificate);
		}

		/// <summary>
		/// Imports the specified certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException (nameof (crl));

			crls.Add (crl);
		}

		#endregion
	}
}
