//
// BouncyCastleSecureMimeContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Asn1.X509;

using AttributeTable = Org.BouncyCastle.Asn1.Cms.AttributeTable;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography
{
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// An abstract S/MIME context built around the BouncyCastle API.
	/// </remarks>
	public abstract class BouncyCastleSecureMimeContext : SecureMimeContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleSecureMimeContext"/>
		/// </remarks>
		protected BouncyCastleSecureMimeContext ()
		{
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
		protected abstract X509Certificate GetCertificate (IX509Selector selector);

		/// <summary>
		/// Get the private key for the certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// <para>Gets the private key for the first certificate that matches the specified selector.</para>
		/// <para>This method is used when signing or decrypting content.</para>
		/// </remarks>
		/// <returns>The private key on success; otherwise, <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected abstract AsymmetricKeyParameter GetPrivateKey (IX509Selector selector);

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
		protected abstract HashSet GetTrustedAnchors ();

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
		protected abstract IX509Store GetIntermediateCertificates ();

		/// <summary>
		/// Get the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// A Certificate Revocation List (CRL) is a list of certificate serial numbers issued
		/// by a particular Certificate Authority (CA) that have been revoked, either by the CA
		/// itself or by the owner of the revoked certificate.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected abstract IX509Store GetCertificateRevocationLists ();

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
		/// <para>This method is called from <see cref="GetDigitalSignatures"/>, allowing custom implementations
		/// to update the X.509 certificate records with the list of preferred encryption algorithms specified by the sending client.</para>
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="algorithms">The encryption algorithm capabilities of the client (in preferred order).</param>
		/// <param name="timestamp">The timestamp.</param>
		protected abstract void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp);

		AttributeTable AddSecureMimeCapabilities (AttributeTable signedAttributes)
		{
			var attr = GetSecureMimeCapabilitiesAttribute ();

			// populate our signed attributes with some S/MIME capabilities
			return signedAttributes.Add (attr.AttrType, attr.AttrValues[0]);
		}

		Stream Sign (CmsSigner signer, Stream content, bool encapsulate)
		{
			var signedData = new CmsSignedDataStreamGenerator ();

			signedData.AddSigner (signer.PrivateKey, signer.Certificate, GetDigestOid (signer.DigestAlgorithm),
			                      AddSecureMimeCapabilities (signer.SignedAttributes), signer.UnsignedAttributes);

			signedData.AddCertificates (signer.CertificateChain);

			var memory = new MemoryBlockStream ();

			using (var stream = signedData.Open (memory, encapsulate))
				content.CopyTo (stream, 4096);

			memory.Position = 0;

			return memory;
		}

		/// <summary>
		/// Cryptographically signs and encapsulates the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", nameof (signer));

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, Sign (signer, content, true));
		}

		/// <summary>
		/// Cryptographically signs and encapsulates the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encapsulates the content using the specified signer and digest algorithm.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return EncapsulatedSign (cmsSigner, content);
		}

		/// <summary>
		/// Cryptographically signs the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", nameof (signer));

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return new ApplicationPkcs7Signature (Sign (signer, content, false));
		}

		/// <summary>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return Sign (cmsSigner, content);
		}

		X509Certificate GetCertificate (IX509Store store, SignerID signer)
		{
			var matches = store.GetMatches (signer);

			foreach (X509Certificate certificate in matches)
				return certificate;

			return GetCertificate (signer);
		}

		PkixCertPath BuildCertPath (HashSet anchors, IX509Store certificates, IX509Store crls, X509Certificate certificate, DateTime? signingTime)
		{
			var intermediate = new X509CertificateStore ();
			foreach (X509Certificate cert in certificates.GetMatches (null))
				intermediate.Add (cert);

			var selector = new X509CertStoreSelector ();
			selector.Certificate = certificate;

			var parameters = new PkixBuilderParameters (anchors, selector);
			parameters.AddStore (GetIntermediateCertificates ());
			parameters.AddStore (intermediate);

			var localCrls = GetCertificateRevocationLists ();
			parameters.AddStore (localCrls);
			parameters.AddStore (crls);

			parameters.ValidityModel = PkixParameters.ChainValidityModel;
			parameters.IsRevocationEnabled = false;

			if (signingTime.HasValue)
				parameters.Date = new DateTimeObject (signingTime.Value);

			var result = new PkixCertPathBuilder ().Build (parameters);

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
		protected static bool TryGetDigestAlgorithm (AlgorithmIdentifier identifier, out DigestAlgorithm algorithm)
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

			if (identifier.Algorithm.Id == SmimeCapability.DesCbc.Id) {
				algorithm = EncryptionAlgorithm.Des;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.IdeaCbc) {
				algorithm = EncryptionAlgorithm.Idea;
				return true;
			}

			if (identifier.Algorithm.Id == CmsEnvelopedGenerator.RC2Cbc) {
				if (identifier.Parameters is DerSequence) {
					var param = (DerSequence) identifier.Parameters;
					var version = (DerInteger) param[0];
					int bits = version.Value.IntValue;

					switch (bits) {
					case 58: algorithm = EncryptionAlgorithm.RC2128; return true;
					case 120: algorithm = EncryptionAlgorithm.RC264; return true;
					case 160: algorithm = EncryptionAlgorithm.RC240; return true;
					}
				} else {
					var param = (DerInteger) identifier.Parameters;
					int bits = param.Value.IntValue;

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

		static DateTime ToAdjustedDateTime (DerUtcTime time)
		{
			//try {
			//	return time.ToAdjustedDateTime ();
			//} catch {
			return DateUtils.Parse (time.AdjustedTimeString, "yyyyMMddHHmmsszzz");
			//}
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
		protected DigitalSignatureCollection GetDigitalSignatures (CmsSignedDataParser parser)
		{
			var certificates = parser.GetCertificates ("Collection");
			var signatures = new List<IDigitalSignature> ();
			var crls = parser.GetCrls ("Collection");
			var store = parser.GetSignerInfos ();

			try {
				// FIXME: we might not want to import these...
				foreach (X509Crl crl in crls.GetMatches (null))
					Import (crl);
			} catch {
			}

			foreach (SignerInformation signerInfo in store.GetSigners ()) {
				var certificate = GetCertificate (certificates, signerInfo.SignerID);
				var signature = new SecureMimeDigitalSignature (signerInfo);
				var algorithms = new List<EncryptionAlgorithm> ();
				DateTime? signedDate = null;
				DigestAlgorithm digestAlgo;

				if (signerInfo.SignedAttributes != null) {
					Asn1EncodableVector vector = signerInfo.SignedAttributes.GetAll (CmsAttributes.SigningTime);
					foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
						var signingTime = (DerUtcTime) ((DerSet) attr.AttrValues)[0];
						signature.CreationDate = ToAdjustedDateTime (signingTime);
						signedDate = signature.CreationDate;
						break;
					}

					vector = signerInfo.SignedAttributes.GetAll (SmimeAttributes.SmimeCapabilities);
					foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
						foreach (Asn1Sequence sequence in attr.AttrValues) {
							for (int i = 0; i < sequence.Count; i++) {
								var identifier = AlgorithmIdentifier.GetInstance (sequence[i]);
								EncryptionAlgorithm algorithm;

								if (TryGetEncryptionAlgorithm (identifier, out algorithm))
									algorithms.Add (algorithm);
							}
						}
					}

					signature.EncryptionAlgorithms = algorithms.ToArray ();
				}

				if (TryGetDigestAlgorithm (signerInfo.DigestAlgorithmID, out digestAlgo))
					signature.DigestAlgorithm = digestAlgo;

				if (certificate != null) {
					signature.SignerCertificate = new SecureMimeDigitalCertificate (certificate);
					if (algorithms.Count > 0 && signedDate != null) {
						UpdateSecureMimeCapabilities (certificate, signature.EncryptionAlgorithms, signedDate.Value);
					} else {
						try {
							Import (certificate);
						} catch {
						}
					}
				}

				var anchors = GetTrustedAnchors ();

				try {
					signature.Chain = BuildCertPath (anchors, certificates, crls, certificate, signedDate);
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
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			var parser = new CmsSignedDataParser (new CmsTypedStream (content), signatureData);
			var signed = parser.GetSignedContent ();

			signed.Drain ();

			return GetDigitalSignatures (parser);
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
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			var parser = new CmsSignedDataParser (signedData);
			var signed = parser.GetSignedContent ();

			entity = MimeEntity.Load (signed.ContentStream);
			signed.Drain ();

			return GetDigitalSignatures (parser);
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
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Stream Verify (Stream signedData, out DigitalSignatureCollection signatures)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));

			var parser = new CmsSignedDataParser (signedData);
			var signed = parser.GetSignedContent ();
			var content = new MemoryBlockStream ();

			signed.ContentStream.CopyTo (content, 4096);
			content.Position = 0;
			signed.Drain ();

			signatures = GetDigitalSignatures (parser);

			return content;
		}

		class VoteComparer : IComparer<int>
		{
			public int Compare (int x, int y)
			{
				return y - x;
			}
		}

		/// <summary>
		/// Get the preferred encryption algorithm to use for encrypting to the specified recipients.
		/// </summary>
		/// <remarks>
		/// <para>Gets the preferred encryption algorithm to use for encrypting to the specified recipients
		/// based on the encryption algorithms supported by each of the recipients, the
		/// <see cref="EnabledEncryptionAlgorithms"/>, and the <see cref="EncryptionAlgorithmRank"/>.</para>
		/// <para>If the supported encryption algorithms are unknown for any recipient, it is assumed that
		/// the recipient supports at least the Triple-DES encryption algorithm.</para>
		/// </remarks>
		/// <returns>The preferred encryption algorithm.</returns>
		/// <param name="recipients">The recipients.</param>
		protected virtual EncryptionAlgorithm GetPreferredEncryptionAlgorithm (CmsRecipientCollection recipients)
		{
			var votes = new int[EncryptionAlgorithmCount];

			foreach (var recipient in recipients) {
				int cast = EncryptionAlgorithmCount;

				foreach (var algorithm in recipient.EncryptionAlgorithms) {
					votes[(int) algorithm] += cast;
					cast--;
				}
			}

			// Starting with S/MIME v3 (published in 1999), Triple-DES is a REQUIRED algorithm.
			// S/MIME v2.x and older only required RC2/40, but SUGGESTED Triple-DES.
			// Considering the fact that Bruce Schneier was able to write a
			// screensaver that could crack RC2/40 back in the late 90's, let's
			// not default to anything weaker than Triple-DES...
			EncryptionAlgorithm chosen = EncryptionAlgorithm.TripleDes;
			int nvotes = 0;

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

		Stream Envelope (CmsRecipientCollection recipients, Stream content)
		{
			var unique = new HashSet<X509Certificate> ();
			var cms = new CmsEnvelopedDataGenerator ();
			int count = 0;

			foreach (var recipient in recipients) {
				if (unique.Add (recipient.Certificate)) {
					cms.AddKeyTransRecipient (recipient.Certificate);
					count++;
				}
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", nameof (recipients));

			var input = new CmsProcessableInputStream (content);
			CmsEnvelopedData envelopedData;

			switch (GetPreferredEncryptionAlgorithm (recipients)) {
			case EncryptionAlgorithm.Camellia256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia256Cbc);
				break;
			case EncryptionAlgorithm.Camellia192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia192Cbc);
				break;
			case EncryptionAlgorithm.Camellia128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia128Cbc);
				break;
			case EncryptionAlgorithm.Aes256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes256Cbc);
				break;
			case EncryptionAlgorithm.Aes192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes192Cbc);
				break;
			case EncryptionAlgorithm.Aes128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes128Cbc);
				break;
			case EncryptionAlgorithm.Cast5:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Cast5Cbc);
				break;
			case EncryptionAlgorithm.Idea:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.IdeaCbc);
				break;
			case EncryptionAlgorithm.RC2128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 128);
				break;
			case EncryptionAlgorithm.RC264:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 64);
				break;
			case EncryptionAlgorithm.RC240:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 40);
				break;
			default:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.DesEde3Cbc);
				break;
			}

			return new MemoryStream (envelopedData.GetEncoded (), false);
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, Envelope (recipients, content));
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A certificate for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			return Encrypt (GetCmsRecipients (recipients), content);
		}

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			var parser = new CmsEnvelopedDataParser (encryptedData);
			var recipients = parser.GetRecipientInfos ();
			var algorithm = parser.EncryptionAlgorithmID;
			AsymmetricKeyParameter key;

			foreach (RecipientInformation recipient in recipients.GetRecipients ()) {
				if ((key = GetPrivateKey (recipient.RecipientID)) == null)
					continue;

				var content = recipient.GetContent (key);
				var memory = new MemoryStream (content, false);

				return MimeEntity.Load (memory, true);
			}

			throw new CmsException ("A suitable private key could not be found for decrypting.");
		}

		/// <summary>
		/// Decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="output">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void DecryptTo (Stream encryptedData, Stream output)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			if (output == null)
				throw new ArgumentNullException (nameof (output));

			var parser = new CmsEnvelopedDataParser (encryptedData);
			var recipients = parser.GetRecipientInfos ();
			var algorithm = parser.EncryptionAlgorithmID;
			AsymmetricKeyParameter key;

			foreach (RecipientInformation recipient in recipients.GetRecipients ()) {
				if ((key = GetPrivateKey (recipient.RecipientID)) == null)
					continue;

				var content = recipient.GetContentStream (key);

				content.ContentStream.CopyTo (output, 4096);
				return;
			}

			throw new CmsException ("A suitable private key could not be found for decrypting.");
		}

		/// <summary>
		/// Exports the certificates for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the certificates for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// No mailboxes were specified.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes)
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

			var cms = new CmsSignedDataStreamGenerator ();
			var memory = new MemoryBlockStream ();

			cms.AddCertificates (certificates);
			cms.Open (memory).Dispose ();
			memory.Position = 0;

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, memory);
		}
	}
}
