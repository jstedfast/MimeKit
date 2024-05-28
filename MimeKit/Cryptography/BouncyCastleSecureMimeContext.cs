//
// BouncyCastleSecureMimeContext.cs
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

#if ENABLE_LDAP
using System.DirectoryServices.Protocols;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;
#endif

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Collections;

using AttributeTable = Org.BouncyCastle.Asn1.Cms.AttributeTable;
using IssuerAndSerialNumber = Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// An abstract S/MIME context built around the BouncyCastle API.
	/// </remarks>
	public abstract class BouncyCastleSecureMimeContext : SecureMimeContext
	{
		static readonly string RsassaPssOid = PkcsObjectIdentifiers.IdRsassaPss.Id;

		HttpClient client;

		/// <summary>
		/// Initialize a new instance of the <see cref="SecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleSecureMimeContext"/>
		/// </remarks>
		protected BouncyCastleSecureMimeContext () : this (new SecureRandom ())
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="SecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleSecureMimeContext"/>
		/// </remarks>
		/// <param name="random">A secure pseudo-random number generator.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="random"/> is <c>null</c>.
		/// </exception>
		protected BouncyCastleSecureMimeContext (SecureRandom random)
		{
			if (random == null)
				throw new ArgumentNullException (nameof (random));

			RandomNumberGenerator = random;
			client = new HttpClient ();
		}

		/// <summary>
		/// Get the pseudo-random number generator.
		/// </summary>
		/// <remarks>
		/// Gets the pseudo-random number generator.
		/// </remarks>
		/// <value>The pseudo-random number generator.</value>
		protected SecureRandom RandomNumberGenerator {
			get; private set;
		}

		/// <summary>
		/// Get or set whether or not certificate revocation lists should be downloaded when verifying signatures.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether or not certificate revocation lists should be downloaded when verifying
		/// signatures.</para>
		/// <para>If enabled, the <see cref="BouncyCastleSecureMimeContext"/> will attempt to automatically download
		/// Certificate Revocation Lists (CRLs) from the internet based on the CRL Distribution Point extension on
		/// each certificate.</para>
		/// <note type="security">Enabling this feature opens the client up to potential privacy risks. An attacker
		/// can generate a custom X.509 certificate containing a CRL Distribution Point or OCSP URL pointing to an
		/// attacker-controlled server, thereby getting a notification when the user decrypts the message or verifies
		/// its digital signature.</note>
		/// </remarks>
		/// <value><c>true</c> if CRLs should be downloaded automatically; otherwise, <c>false</c>.</value>
		public bool CheckCertificateRevocation {
			get; set;
		}

		/// <summary>
		/// Get the X.509 certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// <para>Gets the first certificate that matches the specified selector.</para>
		/// <para>This method is used when constructing a certificate chain if the S/MIME
		/// signature does not include a signer's certificate.</para>
		/// </remarks>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected abstract X509Certificate GetCertificate (ISelector<X509Certificate> selector);

		/// <summary>
		/// Get the private key for the certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// <para>Gets the private key for the first certificate that matches the specified selector.</para>
		/// <para>This method is used when signing or decrypting content.</para>
		/// </remarks>
		/// <returns>The private key on success; otherwise, <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected abstract AsymmetricKeyParameter GetPrivateKey (ISelector<X509Certificate> selector);

		/// <summary>
		/// Get the trusted anchors.
		/// </summary>
		/// <remarks>
		/// <para>A trusted anchor is a trusted root-level X.509 certificate,
		/// generally issued by a certificate authority (CA).</para>
		/// <para>This method is used to build a certificate chain while verifying
		/// signed content.</para>
		/// </remarks>
		/// <returns>The trusted anchors.</returns>
		protected abstract ISet<TrustAnchor> GetTrustedAnchors ();

		/// <summary>
		/// Get the intermediate certificates.
		/// </summary>
		/// <remarks>
		/// <para>An intermediate certificate is any certificate that exists between the root
		/// certificate issued by a Certificate Authority (CA) and the certificate at
		/// the end of the chain.</para>
		/// <para>This method is used to build a certificate chain while verifying
		/// signed content.</para>
		/// </remarks>
		/// <returns>The intermediate certificates.</returns>
		protected abstract IStore<X509Certificate> GetIntermediateCertificates ();

		/// <summary>
		/// Get the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// A Certificate Revocation List (CRL) is a list of certificate serial numbers issued
		/// by a particular Certificate Authority (CA) that have been revoked, either by the CA
		/// itself or by the owner of the revoked certificate.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected abstract IStore<X509Crl> GetCertificateRevocationLists ();

		/// <summary>
		/// Get the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </summary>
		/// <remarks>
		/// Gets the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </remarks>
		/// <returns>The date &amp; time for the next update (in UTC).</returns>
		/// <param name="issuer">The issuer.</param>
		protected abstract DateTime GetNextCertificateRevocationListUpdate (X509Name issuer);

		/// <summary>
		/// Get the <see cref="CmsRecipient"/> for the specified mailbox.
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
		protected abstract CmsRecipient GetCmsRecipient (MailboxAddress mailbox);

		/// <summary>
		/// Get a collection of CmsRecipients for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Gets a collection of CmsRecipients for the specified mailboxes.
		/// </remarks>
		/// <returns>A <see cref="CmsRecipientCollection"/>.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the specified <paramref name="mailboxes"/> could not be found.
		/// </exception>
		protected CmsRecipientCollection GetCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var recipients = new CmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetCmsRecipient (mailbox));

			return recipients;
		}

		/// <summary>
		/// Get the <see cref="CmsSigner"/> for the specified mailbox.
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
		protected abstract CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo);

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
		protected abstract void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp);

		CmsAttributeTableGenerator AddSecureMimeCapabilities (AttributeTable signedAttributes)
		{
			var attr = GetSecureMimeCapabilitiesAttribute (true);

			// populate our signed attributes with some S/MIME capabilities
			return new DefaultSignedAttributeTableGenerator (signedAttributes.Add (attr.AttrType, attr.AttrValues[0]));
		}

		CmsSignedDataStreamGenerator CreateSignedDataGenerator (CmsSigner signer)
		{
			var unsignedAttributes = new SimpleAttributeTableGenerator (signer.UnsignedAttributes);
			var signedAttributes = AddSecureMimeCapabilities (signer.SignedAttributes);
			var signedData = new CmsSignedDataStreamGenerator (RandomNumberGenerator);
			var digestOid = GetDigestOid (signer.DigestAlgorithm);
			byte[] subjectKeyId = null;

			if (signer.SignerIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier) {
				var subjectKeyIdentifier = signer.Certificate.GetExtensionValue (X509Extensions.SubjectKeyIdentifier);
				if (subjectKeyIdentifier != null) {
					var id = (Asn1OctetString) Asn1Object.FromByteArray (subjectKeyIdentifier.GetOctets ());
					subjectKeyId = id.GetOctets ();
				}
			}

			if (signer.PrivateKey is RsaKeyParameters && signer.RsaSignaturePadding == RsaSignaturePadding.Pss) {
				if (subjectKeyId == null)
					signedData.AddSigner (signer.PrivateKey, signer.Certificate, RsassaPssOid, digestOid, signedAttributes, unsignedAttributes);
				else
					signedData.AddSigner (signer.PrivateKey, subjectKeyId, RsassaPssOid, digestOid, signedAttributes, unsignedAttributes);
			} else if (subjectKeyId == null) {
				signedData.AddSigner (signer.PrivateKey, signer.Certificate, digestOid, signedAttributes, unsignedAttributes);
			} else {
				signedData.AddSigner (signer.PrivateKey, subjectKeyId, digestOid, signedAttributes, unsignedAttributes);
			}

			signedData.AddCertificates (signer.CertificateChain);

			return signedData;
		}

		Stream Sign (CmsSigner signer, Stream content, bool encapsulate, CancellationToken cancellationToken)
		{
			var signedData = CreateSignedDataGenerator (signer);
			var memory = new MemoryBlockStream ();

			using (var stream = signedData.Open (memory, encapsulate)) {
				content.CopyTo (stream, 4096);
			}

			memory.Position = 0;

			return memory;
		}

		async Task<Stream> SignAsync (CmsSigner signer, Stream content, bool encapsulate, CancellationToken cancellationToken)
		{
			var signedData = CreateSignedDataGenerator (signer);
			var memory = new MemoryBlockStream ();

			using (var stream = signedData.Open (memory, encapsulate)) {
				await content.CopyToAsync (stream, 4096, cancellationToken).ConfigureAwait (false);
			}

			memory.Position = 0;

			return memory;
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var signature = Sign (signer, content, true, cancellationToken);

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signature);
		}

		/// <summary>
		/// Asynchronously signs and encapsulate the content using the specified signer.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var signature = await SignAsync (signer, content, true, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, signature);
		}

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
		public override ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return EncapsulatedSign (cmsSigner, content, cancellationToken);
		}

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
		public override Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return EncapsulatedSignAsync (cmsSigner, content, cancellationToken);
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var signature = Sign (signer, content, false, cancellationToken);

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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Signature> SignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var signature = await SignAsync (signer, content, false, cancellationToken).ConfigureAwait (false);

			return new ApplicationPkcs7Signature (signature);
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return Sign (cmsSigner, content, cancellationToken);
		}

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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return await SignAsync (cmsSigner, content, cancellationToken).ConfigureAwait (false);
		}

		X509Certificate GetCertificate (IStore<X509Certificate> store, SignerID signer)
		{
			var matches = store.EnumerateMatches (signer);

			foreach (X509Certificate certificate in matches)
				return certificate;

			return GetCertificate (signer);
		}

		/// <summary>
		/// Build a certificate chain.
		/// </summary>
		/// <remarks>
		/// <para>Builds a certificate chain for the provided certificate to include when signing.</para>
		/// <para>This method is ideal for use with custom <see cref="GetCmsSigner"/>
		/// implementations when it is desirable to include the certificate chain
		/// in the signature.</para>
		/// </remarks>
		/// <param name="certificate">The certificate to build the chain for.</param>
		/// <returns>The certificate chain, including the specified certificate.</returns>
		protected IList<X509Certificate> BuildCertificateChain (X509Certificate certificate)
		{
			var selector = new X509CertStoreSelector {
				Certificate = certificate
			};

			var intermediates = new X509CertificateStore ();
			intermediates.Add (certificate);

			var parameters = new PkixBuilderParameters (GetTrustedAnchors (), selector) {
				ValidityModel = PkixParameters.PkixValidityModel,
				IsRevocationEnabled = false,
				Date = DateTime.UtcNow
			};
			parameters.AddStoreCert (intermediates);
			parameters.AddStoreCert (GetIntermediateCertificates ());

			var builder = new PkixCertPathBuilder ();
			var result = builder.Build (parameters);

			var chain = new X509Certificate[result.CertPath.Certificates.Count];

			for (int i = 0; i < chain.Length; i++)
				chain[i] = result.CertPath.Certificates[i];

			return chain;
		}

		PkixCertPath BuildCertPath (ISet<TrustAnchor> anchors, IStore<X509Certificate> certificates, IStore<X509Crl> crls, X509Certificate certificate, DateTime signingTime)
		{
			var selector = new X509CertStoreSelector {
				Certificate = certificate
			};

			var intermediates = new X509CertificateStore ();
			intermediates.Add (certificate);

			foreach (X509Certificate cert in certificates.EnumerateMatches (null))
				intermediates.Add (cert);

			var parameters = new PkixBuilderParameters (anchors, selector) {
				ValidityModel = PkixParameters.PkixValidityModel,
				IsRevocationEnabled = false
			};
			parameters.AddStoreCert (intermediates);
			parameters.AddStoreCrl (crls);

			parameters.AddStoreCert (GetIntermediateCertificates ());
			parameters.AddStoreCrl (GetCertificateRevocationLists ());

			if (signingTime != default (DateTime))
				parameters.Date = signingTime;

			var builder = new PkixCertPathBuilder ();
			var result = builder.Build (parameters);

			return result.CertPath;
		}

		/// <summary>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="DigestAlgorithm"/>.
		/// </summary>
		/// <remarks>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="DigestAlgorithm"/>.
		/// </remarks>
		/// <returns><c>true</c> if the algorithm identifier was successfully mapped; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">The algorithm identifier.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="identifier"/> is <c>null</c>.
		/// </exception>
		internal protected static bool TryGetDigestAlgorithm (AlgorithmIdentifier identifier, out DigestAlgorithm algorithm)
		{
			if (identifier == null)
				throw new ArgumentNullException (nameof (identifier));

			return TryGetDigestAlgorithm (identifier.Algorithm.Id, out algorithm);
		}

		/// <summary>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="EncryptionAlgorithm"/>.
		/// </summary>
		/// <remarks>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="EncryptionAlgorithm"/>.
		/// </remarks>
		/// <returns><c>true</c> if the algorithm identifier was successfully mapped; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">The algorithm identifier.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="identifier"/> is <c>null</c>.
		/// </exception>
		internal protected static bool TryGetEncryptionAlgorithm (AlgorithmIdentifier identifier, out EncryptionAlgorithm algorithm)
		{
			if (identifier == null)
				throw new ArgumentNullException (nameof (identifier));

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Aes256Cbc) {
				algorithm = EncryptionAlgorithm.Aes256;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Aes192Cbc) {
				algorithm = EncryptionAlgorithm.Aes192;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Aes128Cbc) {
				algorithm = EncryptionAlgorithm.Aes128;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Camellia256Cbc) {
				algorithm = EncryptionAlgorithm.Camellia256;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Camellia192Cbc) {
				algorithm = EncryptionAlgorithm.Camellia192;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Camellia128Cbc) {
				algorithm = EncryptionAlgorithm.Camellia128;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.Cast5Cbc) {
				algorithm = EncryptionAlgorithm.Cast5;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.DesEde3Cbc) {
				algorithm = EncryptionAlgorithm.TripleDes;
				return true;
			}

			if (identifier.Algorithm.Id == Blowfish.Id) {
				algorithm = EncryptionAlgorithm.Blowfish;
				return true;
			}

			if (identifier.Algorithm.Id == Twofish.Id) {
				algorithm = EncryptionAlgorithm.Twofish;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.SeedCbc) {
				algorithm = EncryptionAlgorithm.Seed;
				return true;
			}

			if (identifier.Algorithm.Id == SmimeCapability.DesCbc.Id) {
				algorithm = EncryptionAlgorithm.Des;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.IdeaCbc) {
				algorithm = EncryptionAlgorithm.Idea;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.RC2Cbc) {
				if (identifier.Parameters is DerSequence derSequence) {
					var version = (DerInteger) derSequence[0];
					int bits = version.Value.IntValue;

					switch (bits) {
					case 58: algorithm = EncryptionAlgorithm.RC2128; return true;
					case 120: algorithm = EncryptionAlgorithm.RC264; return true;
					case 160: algorithm = EncryptionAlgorithm.RC240; return true;
					}
				} else if (identifier.Parameters is DerInteger derInteger) {
					int bits = derInteger.Value.IntValue;

					switch (bits) {
					case 128: algorithm = EncryptionAlgorithm.RC2128; return true;
					case 64: algorithm = EncryptionAlgorithm.RC264; return true;
					case 40: algorithm = EncryptionAlgorithm.RC240; return true;
					}
				}
			}

			algorithm = EncryptionAlgorithm.RC240;

			return false;
		}

		async Task<bool> DownloadCrlsOverHttpAsync (string location, Stream stream, bool doAsync, CancellationToken cancellationToken)
		{
			try {
				if (doAsync) {
					using (var response = await client.GetAsync (location, cancellationToken).ConfigureAwait (false)) {
#if NET6_0_OR_GREATER
						await response.Content.CopyToAsync (stream, cancellationToken).ConfigureAwait (false);
#else
						await response.Content.CopyToAsync (stream).ConfigureAwait (false);
#endif
					}
				} else {
#if NET6_0_OR_GREATER
					using (var response = client.GetAsync (location, cancellationToken).GetAwaiter ().GetResult ())
						response.Content.CopyToAsync (stream, cancellationToken).GetAwaiter ().GetResult ();
#else
					cancellationToken.ThrowIfCancellationRequested ();

					var request = (HttpWebRequest) WebRequest.Create (location);
					using (var response = request.GetResponse ()) {
						var content = response.GetResponseStream ();
						content.CopyTo (stream, 4096);
					}
#endif
				}

				return true;
			} catch {
				return false;
			}
		}

#if ENABLE_LDAP
		// https://msdn.microsoft.com/en-us/library/bb332056.aspx#sdspintro_topic3_lpadconn
		bool DownloadCrlsOverLdap (string location, Stream stream, CancellationToken cancellationToken)
		{
			LdapUri uri;

			cancellationToken.ThrowIfCancellationRequested ();

			if (!LdapUri.TryParse (location, out uri) || string.IsNullOrEmpty (uri.Host) || string.IsNullOrEmpty (uri.DistinguishedName))
				return false;

			try {
				// Note: Mono doesn't support this...
				LdapDirectoryIdentifier identifier;

				if (uri.Port > 0)
					identifier = new LdapDirectoryIdentifier (uri.Host, uri.Port, false, true);
				else
					identifier = new LdapDirectoryIdentifier (uri.Host, false, true);

				using (var ldap = new LdapConnection (identifier)) {
					if (uri.Scheme.Equals ("ldaps", StringComparison.OrdinalIgnoreCase))
						ldap.SessionOptions.SecureSocketLayer = true;

					ldap.Bind ();

					var request = new SearchRequest (uri.DistinguishedName, uri.Filter, uri.Scope, uri.Attributes);
					var response = (SearchResponse) ldap.SendRequest (request);

					foreach (SearchResultEntry entry in response.Entries) {
						foreach (DirectoryAttribute attribute in entry.Attributes) {
							var values = attribute.GetValues (typeof (byte[]));

							for (int i = 0; i < values.Length; i++) {
								var buffer = (byte[]) values[i];

								stream.Write (buffer, 0, buffer.Length);
							}
						}
					}
				}

				return true;
			} catch {
				return false;
			}
		}
#endif

		async Task DownloadCrlsAsync (X509Certificate certificate, bool doAsync, CancellationToken cancellationToken)
		{
			var nextUpdate = GetNextCertificateRevocationListUpdate (certificate.IssuerDN);
			var now = DateTime.UtcNow;
			Asn1OctetString cdp;

			if (nextUpdate > now)
				return;

			if ((cdp = certificate.GetExtensionValue (X509Extensions.CrlDistributionPoints)) == null)
				return;

			var asn1 = Asn1Object.FromByteArray (cdp.GetOctets ());
			var crlDistributionPoint = CrlDistPoint.GetInstance (asn1);
			var points = crlDistributionPoint.GetDistributionPoints ();

			using (var stream = new MemoryBlockStream ()) {
#if ENABLE_LDAP
				var ldapLocations = new List<string> ();
#endif
				bool downloaded = false;

				for (int i = 0; i < points.Length; i++) {
					var generalNames = GeneralNames.GetInstance (points[i].DistributionPointName.Name).GetNames ();
					for (int j = 0; j < generalNames.Length && !downloaded; j++) {
						if (generalNames[j].TagNo != GeneralName.UniformResourceIdentifier)
							continue;

						var location = DerIA5String.GetInstance (generalNames[j].Name).GetString ();
						var colon = location.IndexOf (':');

						if (colon == -1)
							continue;

						var protocol = location.Substring (0, colon).ToLowerInvariant ();

						switch (protocol) {
						case "https": case "http":
							downloaded = await DownloadCrlsOverHttpAsync (location, stream, doAsync, cancellationToken).ConfigureAwait (false);
							break;
#if ENABLE_LDAP
						case "ldaps": case "ldap":
							// Note: delay downloading from LDAP urls in case we find an HTTP url instead since LDAP
							// won't be as reliable on Mono systems which do not implement the LDAP functionality.
							ldapLocations.Add (location);
							break;
#endif
						}
					}
				}

#if ENABLE_LDAP
				for (int i = 0; i < ldapLocations.Count && !downloaded; i++)
					downloaded = DownloadCrlsOverLdap (ldapLocations[i], stream, cancellationToken);
#endif

				if (!downloaded)
					return;

				stream.Position = 0;

				var parser = new X509CrlParser ();
				foreach (X509Crl crl in parser.ReadCrls (stream))
					Import (crl, cancellationToken);
			}
		}

		/// <summary>
		/// Get the list of digital signatures.
		/// </summary>
		/// <remarks>
		/// <para>Gets the list of digital signatures.</para>
		/// <para>This method is useful to call from within any custom
		/// <a href="Overload_MimeKit_Cryptography_SecureMimeContext_Verify.htm">Verify</a>
		/// method that you may implement in your own class.</para>
		/// </remarks>
		/// <returns>The digital signatures.</returns>
		/// <param name="parser">The CMS signed data parser.</param>
		/// <param name="doAsync">Whether or not the operation should be done asynchronously.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		async Task<DigitalSignatureCollection> GetDigitalSignaturesAsync (CmsSignedDataParser parser, bool doAsync, CancellationToken cancellationToken)
		{
			var certificates = parser.GetCertificates ();
			var signatures = new List<IDigitalSignature> ();
			var crls = parser.GetCrls ();
			var store = parser.GetSignerInfos ();

			foreach (var signerInfo in store.GetSigners ()) {
				var certificate = GetCertificate (certificates, signerInfo.SignerID);
				var signature = new SecureMimeDigitalSignature (signerInfo, certificate);

				if (CheckCertificateRevocation && certificate != null)
					await DownloadCrlsAsync (certificate, doAsync, cancellationToken).ConfigureAwait (false);

				if (certificate != null) {
					Import (certificate, cancellationToken);

					if (signature.EncryptionAlgorithms.Length > 0 && signature.CreationDate != default (DateTime))
						UpdateSecureMimeCapabilities (certificate, signature.EncryptionAlgorithms, signature.CreationDate);
				}

				var anchors = GetTrustedAnchors ();

				try {
					signature.Chain = BuildCertPath (anchors, certificates, crls, certificate, signature.CreationDate);
				} catch (Exception ex) {
					signature.ChainException = ex;
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			using (var parser = new CmsSignedDataParser (new CmsTypedStream (content), signatureData)) {
				var signed = parser.GetSignedContent ();

				try {
					signed.ContentStream.CopyTo (Stream.Null, 4096);
				} finally {
					signed.ContentStream.Dispose ();
				}

				return GetDigitalSignaturesAsync (parser, false, cancellationToken).GetAwaiter ().GetResult ();
			}
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override async Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			using (var parser = new CmsSignedDataParser (new CmsTypedStream (content), signatureData)) {
				var signed = parser.GetSignedContent ();

				try {
#if NET6_0_OR_GREATER
					await signed.ContentStream.CopyToAsync (Stream.Null, 4096, cancellationToken).ConfigureAwait (false);
#else
					await signed.ContentStream.CopyToAsync (Stream.Null, 4096).ConfigureAwait (false);
#endif
				} finally {
					signed.ContentStream.Dispose ();
				}

				return await GetDigitalSignaturesAsync (parser, true, cancellationToken).ConfigureAwait (false);
			}
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			using (var parser = new CmsSignedDataParser (signedData)) {
				var signed = parser.GetSignedContent ();

				try {
					entity = MimeEntity.Load (signed.ContentStream, cancellationToken);
					signed.ContentStream.CopyTo (Stream.Null, 4096);
				} finally {
					signed.ContentStream.Dispose ();
				}

				return GetDigitalSignaturesAsync (parser, false, cancellationToken).GetAwaiter ().GetResult ();
			}
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override Stream Verify (Stream signedData, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			using (var parser = new CmsSignedDataParser (signedData)) {
				var signed = parser.GetSignedContent ();
				var content = new MemoryBlockStream ();

				try {
					signed.ContentStream.CopyTo (content, 4096);
					content.Position = 0;
				} catch {
					content.Dispose ();
				} finally {
					signed.ContentStream.Dispose ();
				}

				signatures = GetDigitalSignaturesAsync (parser, false, cancellationToken).GetAwaiter ().GetResult ();

				return content;
			}
		}

		/// <summary>
		/// An RSA-OAEP aware recipient info generator.
		/// </summary>
		class RsaOaepAwareRecipientInfoGenerator : RecipientInfoGenerator
		{
			readonly CmsRecipient recipient;

			public RsaOaepAwareRecipientInfoGenerator (CmsRecipient recipient)
			{
				this.recipient = recipient;
			}

			static IWrapper CreateWrapper (AlgorithmIdentifier keyExchangeAlgorithm)
			{
				string name;

				if (keyExchangeAlgorithm.Algorithm.Id.Equals (PkcsObjectIdentifiers.IdRsaesOaep.Id, StringComparison.Ordinal)) {
					var oaepParameters = RsaesOaepParameters.GetInstance (keyExchangeAlgorithm.Parameters);
					name = "RSA//OAEPWITH" + DigestUtilities.GetAlgorithmName (oaepParameters.HashAlgorithm.Algorithm) + "ANDMGF1Padding";
				} else if (keyExchangeAlgorithm.Algorithm.Id.Equals (PkcsObjectIdentifiers.RsaEncryption.Id, StringComparison.Ordinal)) {
					name = "RSA//PKCS1Padding";
				} else {
					name = keyExchangeAlgorithm.Algorithm.Id;
				}

				return WrapperUtilities.GetWrapper (name);
			}

			static byte[] GenerateWrappedKey (KeyParameter contentEncryptionKey, AlgorithmIdentifier keyEncryptionAlgorithm, AsymmetricKeyParameter publicKey, SecureRandom random)
			{
				var keyWrapper = CreateWrapper (keyEncryptionAlgorithm);
				var keyBytes = contentEncryptionKey.GetKey ();

				keyWrapper.Init (true, new ParametersWithRandom (publicKey, random));

				return keyWrapper.Wrap (keyBytes, 0, keyBytes.Length);
			}

			public RecipientInfo Generate (KeyParameter contentEncryptionKey, SecureRandom random)
			{
				var certificate = recipient.Certificate.TbsCertificate;
				var publicKey = recipient.Certificate.GetPublicKey ();
				var publicKeyInfo = certificate.SubjectPublicKeyInfo;
				AlgorithmIdentifier keyEncryptionAlgorithm;

				// Note: If the recipient explicitly opts in to OAEP encryption (even if the underlying certificate is not tagged with an OAEP OID), choose OAEP instead.
				if (publicKey is RsaKeyParameters && recipient.RsaEncryptionPadding?.Scheme == RsaEncryptionPaddingScheme.Oaep) {
					keyEncryptionAlgorithm = recipient.RsaEncryptionPadding.GetAlgorithmIdentifier ();
				} else {
					keyEncryptionAlgorithm = publicKeyInfo.Algorithm;
				}

				var encryptedKeyBytes = GenerateWrappedKey (contentEncryptionKey, keyEncryptionAlgorithm, publicKey, random);
				RecipientIdentifier recipientIdentifier;

				if (recipient.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier) {
					var subjectKeyIdentifier = (Asn1OctetString) recipient.Certificate.GetExtensionParsedValue (X509Extensions.SubjectKeyIdentifier);
					recipientIdentifier = new RecipientIdentifier (subjectKeyIdentifier);
				} else {
					var issuerAndSerial = new IssuerAndSerialNumber (certificate.Issuer, certificate.SerialNumber.Value);
					recipientIdentifier = new RecipientIdentifier (issuerAndSerial);
				}

				return new RecipientInfo (new KeyTransRecipientInfo (recipientIdentifier, keyEncryptionAlgorithm,
					new DerOctetString (encryptedKeyBytes)));
			}
		}

		void CmsEnvelopeAddEllipticCurve (CmsEnvelopedDataGenerator cms, CmsRecipient recipient, X509Certificate certificate, ECKeyParameters publicKey)
		{
			var keyGenerator = new ECKeyPairGenerator ();

			keyGenerator.Init (new ECKeyGenerationParameters (publicKey.Parameters, RandomNumberGenerator));

			var keyPair = keyGenerator.GenerateKeyPair ();

			// TODO: better handle algorithm selection.
			if (recipient.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier) {
				var subjectKeyIdentifier = (Asn1OctetString) recipient.Certificate.GetExtensionParsedValue (X509Extensions.SubjectKeyIdentifier);
				cms.AddKeyAgreementRecipient (
					CmsEnvelopedGenerator.ECDHSha1Kdf,
					keyPair.Private,
					keyPair.Public,
					subjectKeyIdentifier.GetOctets (),
					publicKey,
					CmsEnvelopedGenerator.Aes128Wrap
				);
			} else {
				cms.AddKeyAgreementRecipient (
					CmsEnvelopedGenerator.ECDHSha1Kdf,
					keyPair.Private,
					keyPair.Public,
					certificate,
					CmsEnvelopedGenerator.Aes128Wrap
				);
			}
		}

		Stream Envelope (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken)
		{
			var cms = new CmsEnvelopedDataGenerator (RandomNumberGenerator);
			var unique = new HashSet<X509Certificate> ();
			int count = 0;

			foreach (var recipient in recipients) {
				if (unique.Add (recipient.Certificate)) {
					var certificate = recipient.Certificate;
					var pub = certificate.GetPublicKey ();

					if (pub is RsaKeyParameters) {
						// Bouncy Castle dispatches OAEP based on the certificate type. However, MimeKit users
						// expect to be able to specify the use of OAEP in S/MIME with certificates that have
						// PKCS#1v1.5 OIDs as these tend to be more broadly compatible across the ecosystem.
						// Thus, build our own RecipientInfoGenerator and register that for this key.
						cms.AddRecipientInfoGenerator (new RsaOaepAwareRecipientInfoGenerator (recipient));
					} else if (pub is ECKeyParameters ellipticCurve) {
						CmsEnvelopeAddEllipticCurve (cms, recipient, certificate, ellipticCurve);
					} else {
						var oid = certificate.SubjectPublicKeyInfo.Algorithm.Algorithm.ToString ();

						throw new NotSupportedException ($"Unsupported type of recipient certificate: {pub.GetType ().Name} (SubjectPublicKeyInfo OID = {oid})");
					}

					count++;
				}
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", nameof (recipients));

			var algorithm = GetPreferredEncryptionAlgorithm (recipients);
			var input = new CmsProcessableInputStream (content);
			CmsEnvelopedData envelopedData;

			switch (algorithm) {
			case EncryptionAlgorithm.Aes128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes128Cbc);
				break;
			case EncryptionAlgorithm.Aes192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes192Cbc);
				break;
			case EncryptionAlgorithm.Aes256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes256Cbc);
				break;
			case EncryptionAlgorithm.Blowfish:
				envelopedData = cms.Generate (input, Blowfish.Id);
				break;
			case EncryptionAlgorithm.Camellia128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia128Cbc);
				break;
			case EncryptionAlgorithm.Camellia192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia192Cbc);
				break;
			case EncryptionAlgorithm.Camellia256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia256Cbc);
				break;
			case EncryptionAlgorithm.Cast5:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Cast5Cbc);
				break;
			case EncryptionAlgorithm.Des:
				envelopedData = cms.Generate (input, SmimeCapability.DesCbc.Id);
				break;
			case EncryptionAlgorithm.Idea:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.IdeaCbc);
				break;
			case EncryptionAlgorithm.RC240:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 40);
				break;
			case EncryptionAlgorithm.RC264:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 64);
				break;
			case EncryptionAlgorithm.RC2128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 128);
				break;
			case EncryptionAlgorithm.Seed:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.SeedCbc);
				break;
			case EncryptionAlgorithm.TripleDes:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.DesEde3Cbc);
				break;
			//case EncryptionAlgorithm.Twofish:
			//	envelopedData = cms.Generate (input, Twofish.Id);
			//	break;
			default:
				throw new NotSupportedException (string.Format ("The {0} encryption algorithm is not supported by the {1}.", algorithm, GetType ().Name));
			}

			return new MemoryStream (envelopedData.GetEncoded (), false);
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var envelopedData = Envelope (recipients, content, cancellationToken);

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

		/// <summary>
		/// Asynchronously encrypt the specified content for the specified recipients.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			Stream envelopedData;

			using (var memory = new MemoryBlockStream ()) {
				await content.CopyToAsync (memory, 4096, cancellationToken).ConfigureAwait (false);

				memory.Position = 0;

				envelopedData = Envelope (recipients, memory, cancellationToken);
			}

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, envelopedData);
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return Encrypt (GetCmsRecipients (recipients), content, cancellationToken);
		}

		/// <summary>
		/// Asynchronously encrypt the specified content for the specified recipients.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return await EncryptAsync (GetCmsRecipients (recipients), content, cancellationToken).ConfigureAwait (false);
		}

		CmsTypedStream GetDecryptedContent (CmsEnvelopedDataParser parser)
		{
			var recipients = parser.GetRecipientInfos ();
			AsymmetricKeyParameter key;

			foreach (var recipient in recipients.GetRecipients ()) {
				if ((key = GetPrivateKey (recipient.RecipientID)) == null)
					continue;

				return recipient.GetContentStream (key);
			}

			throw new CmsException ("A suitable private key could not be found for decrypting.");
		}

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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			using (var parser = new CmsEnvelopedDataParser (encryptedData)) {
				var decrypted = GetDecryptedContent (parser);

				try {
					return MimeEntity.Load (decrypted.ContentStream, false, cancellationToken);
				} finally {
					decrypted.ContentStream.Dispose ();
				}
			}
		}

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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			using (var parser = new CmsEnvelopedDataParser (encryptedData)) {
				var decrypted = GetDecryptedContent (parser);

				try {
					return await MimeEntity.LoadAsync (decrypted.ContentStream, false, cancellationToken).ConfigureAwait (false);
				} finally {
					decrypted.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Decrypt the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void DecryptTo (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			if (decryptedData == null)
				throw new ArgumentNullException (nameof (decryptedData));

			using (var parser = new CmsEnvelopedDataParser (encryptedData)) {
				var decrypted = GetDecryptedContent (parser);

				try {
					decrypted.ContentStream.CopyTo (decryptedData, 4096);
				} finally {
					decrypted.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Asynchronously decrypt the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override async Task DecryptToAsync (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			if (decryptedData == null)
				throw new ArgumentNullException (nameof (decryptedData));

			using (var parser = new CmsEnvelopedDataParser (encryptedData)) {
				var decrypted = GetDecryptedContent (parser);

				try {
					await decrypted.ContentStream.CopyToAsync (decryptedData, 4096, cancellationToken).ConfigureAwait (false);
				} finally {
					decrypted.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Export the certificates for the specified mailboxes.
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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var certificates = new X509CertificateStore ();
			int count = 0;

			foreach (var mailbox in mailboxes) {
				var recipient = GetCmsRecipient (mailbox);
				certificates.Add (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No mailboxes specified.", nameof (mailboxes));

			var cms = new CmsSignedDataStreamGenerator (RandomNumberGenerator);
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (Export (mailboxes, cancellationToken));
		}

		/// <summary>
		/// Releases all resources used by the <see cref="BouncyCastleSecureMimeContext"/> object.
		/// </summary>
		/// <remarks>Call <see cref="CryptographyContext.Dispose()"/> when you are finished using the <see cref="BouncyCastleSecureMimeContext"/>. The
		/// <see cref="CryptographyContext.Dispose()"/> method leaves the <see cref="BouncyCastleSecureMimeContext"/> in an unusable state. After
		/// calling <see cref="CryptographyContext.Dispose()"/>, you must release all references to the <see cref="BouncyCastleSecureMimeContext"/> so
		/// the garbage collector can reclaim the memory that the <see cref="BouncyCastleSecureMimeContext"/> was occupying.</remarks>
		protected override void Dispose (bool disposing)
		{
			if (disposing && client != null) {
				client.Dispose ();
				client = null;
			}

			base.Dispose (disposing);
		}
	}
}
