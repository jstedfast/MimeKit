//
// SecureMimeContext.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities.Collections;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="SecureMimeContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public abstract class SecureMimeContext : CryptographyContext
	{
		/// <summary>
		/// Gets the signature protocol.
		/// </summary>
		/// <value>The signature protocol.</value>
		public override string SignatureProtocol {
			get { return "application/pkcs7-signature"; }
		}

		/// <summary>
		/// Gets the encryption protocol.
		/// </summary>
		/// <value>The encryption protocol.</value>
		public override string EncryptionProtocol {
			get { return "application/pkcs7-mime"; }
		}

		/// <summary>
		/// Gets the key exchange protocol.
		/// </summary>
		/// <value>The key exchange protocol.</value>
		public override string KeyExchangeProtocol {
			get { return "application/pkcs7-keys"; }
		}

		/// <summary>
		/// Checks whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
		/// </summary>
		/// <returns><c>true</c> if the protocol is supported; otherwise <c>false</c></returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public override bool Supports (string protocol)
		{
			if (protocol == null)
				throw new ArgumentNullException ("protocol");

			var type = protocol.ToLowerInvariant ().Split (new char[] { '/' });
			if (type.Length != 2 || type[0] != "application")
				return false;

			if (type[1].StartsWith ("x-", StringComparison.Ordinal))
				type[1] = type[1].Substring (2);

			return type[1] == "pkcs7-signature" || type[1] == "pkcs7-mime" || type[1] == "pkcs7-keys";
		}

		/// <summary>
		/// Gets the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
		/// </summary>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		public override string GetMicAlgorithmName (DigestAlgorithm micalg)
		{
			switch (micalg) {
			case DigestAlgorithm.MD5:        return "md5";
			case DigestAlgorithm.Sha1:       return "sha1";
			case DigestAlgorithm.RipeMD160:  return "ripemd160";
			case DigestAlgorithm.MD2:        return "md2";
			case DigestAlgorithm.Tiger192:   return "tiger192";
			case DigestAlgorithm.Haval5160:  return "haval-5-160";
			case DigestAlgorithm.Sha256:     return "sha256";
			case DigestAlgorithm.Sha384:     return "sha384";
			case DigestAlgorithm.Sha512:     return "sha512";
			case DigestAlgorithm.Sha224:     return "sha224";
			case DigestAlgorithm.MD4:        return "md4";
			default: throw new ArgumentOutOfRangeException ("micalg");
			}
		}

		/// <summary>
		/// Gets the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <returns>The digest algorithm.</returns>
		/// <param name="micalg">The micalg parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="micalg"/> is <c>null</c>.
		/// </exception>
		public override DigestAlgorithm GetDigestAlgorithm (string micalg)
		{
			if (micalg == null)
				throw new ArgumentNullException ("micalg");

			switch (micalg.ToLowerInvariant ()) {
			case "md5":         return DigestAlgorithm.MD5;
			case "sha1":        return DigestAlgorithm.Sha1;
			case "ripemd160":   return DigestAlgorithm.RipeMD160;
			case "md2":         return DigestAlgorithm.MD2;
			case "tiger192":    return DigestAlgorithm.Tiger192;
			case "haval-5-160": return DigestAlgorithm.Haval5160;
			case "sha256":      return DigestAlgorithm.Sha256;
			case "sha384":      return DigestAlgorithm.Sha384;
			case "sha512":      return DigestAlgorithm.Sha512;
			case "sha224":      return DigestAlgorithm.Sha224;
			case "md4":         return DigestAlgorithm.MD4;
			default:            return DigestAlgorithm.None;
			}
		}

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected abstract X509Certificate GetCertificate (IX509Selector selector);

		/// <summary>
		/// Gets the private key based on the provided selector.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected abstract AsymmetricKeyParameter GetPrivateKey (IX509Selector selector);

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <returns>The trusted anchors.</returns>
		protected abstract HashSet GetTrustedAnchors ();

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <returns>The intermediate certificates.</returns>
		protected abstract IX509Store GetIntermediateCertificates ();

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <returns>The certificate revocation lists.</returns>
		protected abstract IX509Store GetCertificateRevocationLists ();

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected abstract CmsRecipient GetCmsRecipient (MailboxAddress mailbox);

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/>s for the specified <see cref="MimeKit.MailboxAddress"/>es.
		/// </summary>
		/// <returns>The <see cref="CmsRecipient"/>s.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the specified <paramref name="mailboxes"/> could not be found.
		/// </exception>
		protected CmsRecipientCollection GetCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			var recipients = new CmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetCmsRecipient (mailbox));

			return recipients;
		}

		/// <summary>
		/// Gets the <see cref="CmsSigner"/> for the specified mailbox.
		/// </summary>
		/// <returns>A <see cref="CmsSigner"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected abstract CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo);

		static string GetOid (DigestAlgorithm digestAlgo)
		{
			switch (digestAlgo) {
			case DigestAlgorithm.MD5:        return PkcsObjectIdentifiers.MD5.Id;
			case DigestAlgorithm.Sha1:       return PkcsObjectIdentifiers.Sha1WithRsaEncryption.Id;
			case DigestAlgorithm.MD2:        return PkcsObjectIdentifiers.MD2.Id;
			case DigestAlgorithm.Sha256:     return PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id;
			case DigestAlgorithm.Sha384:     return PkcsObjectIdentifiers.Sha384WithRsaEncryption.Id;
			case DigestAlgorithm.Sha512:     return PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id;
			case DigestAlgorithm.Sha224:     return PkcsObjectIdentifiers.Sha224WithRsaEncryption.Id;
			case DigestAlgorithm.MD4:        return PkcsObjectIdentifiers.MD4.Id;
			case DigestAlgorithm.RipeMD160:
			case DigestAlgorithm.DoubleSha:
			case DigestAlgorithm.Tiger192:
			case DigestAlgorithm.Haval5160:
				throw new NotSupportedException ();
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// Compress the specified stream.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Mime Compress (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var compresser = new CmsCompressedDataGenerator ();
			var processable = new CmsProcessableInputStream (stream);
			var compressed = compresser.Generate (processable, CmsCompressedDataGenerator.ZLib);
			var encoded = compressed.GetEncoded ();

			return new ApplicationPkcs7Mime (SecureMimeType.CompressedData, new MemoryStream (encoded, false));
		}

		/// <summary>
		/// Decompress the specified stream.
		/// </summary>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var decompresser = new CmsCompressedDataParser (stream);
			var content = decompresser.GetContent ();
			var parser = new MimeParser (content.ContentStream, MimeFormat.Entity);

			return parser.ParseEntity ();
		}

		Stream Sign (CmsSigner signer, Stream content, bool encapsulate)
		{
			var cms = new CmsSignedDataStreamGenerator ();

			cms.AddSigner (signer.PrivateKey, signer.Certificate, GetOid (signer.DigestAlgorithm),
				signer.SignedAttributes, signer.UnsignedAttributes);

			var memory = new MemoryStream ();

			using (var stream = cms.Open (memory, encapsulate)) {
				content.CopyTo (stream, 4096);
			}

			memory.Position = 0;

			return memory;
		}

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
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
		public ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", "signer");

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, Sign (signer, content, true));
		}

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
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
		public ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return EncapsulatedSign (cmsSigner, content);
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
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
		public ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", "signer");

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Signature (Sign (signer, content, false));
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
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
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return Sign (cmsSigner, content);
		}

		X509Certificate GetCertificate (IX509Store store, SignerID signer)
		{
			var matches = store.GetMatches (signer);

			foreach (X509Certificate certificate in matches) {
				return certificate;
			}

			return GetCertificate (signer);
		}

		PkixCertPath BuildCertPath (HashSet anchors, IX509Store certificates, IX509Store crls, X509Certificate certificate)
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

			// Note: we disable revocation unless we actually have non-empty revocation lists
			parameters.IsRevocationEnabled = localCrls.GetMatches (null).Count > 0;

			var result = new PkixCertPathBuilder ().Build (parameters);

			return result.CertPath;
		}

		IList<IDigitalSignature> GetDigitalSignatures (CmsSignedDataParser parser)
		{
			var certificates = parser.GetCertificates ("Collection");
			var signatures = new List<IDigitalSignature> ();
			var crls = parser.GetCrls ("Collection");
			var store = parser.GetSignerInfos ();

			foreach (SignerInformation signerInfo in store.GetSigners ()) {
				var certificate = GetCertificate (certificates, signerInfo.SignerID);
				var signature = new SecureMimeDigitalSignature (signerInfo);

				if (signerInfo.SignedAttributes != null) {
					Asn1EncodableVector vector = signerInfo.SignedAttributes.GetAll (CmsAttributes.SigningTime);
					foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
						var signingTime = (DerUtcTime) ((DerSet) attr.AttrValues)[0];
						signature.CreationDate = signingTime.ToAdjustedDateTime ();
						break;
					}
				}

				if (certificate != null)
					signature.SignerCertificate = new SecureMimeDigitalCertificate (certificate);

				var anchors = GetTrustedAnchors ();

				var chain = BuildCertPath (anchors, certificates, crls, certificate);

//				var parameters = new PkixParameters (anchors);
//				var validator = new PkixCertPathValidator ();
//				var result = validator.Validate (chain, parameters);

				signature.Chain = chain;

				Console.WriteLine ("Chain:");
				foreach (X509Certificate cert in chain.Certificates) {
					Console.WriteLine ("{0}", cert.GetCommonName ());
				}

				signatures.Add (signature);
			}

			return signatures;
		}

		/// <summary>
		/// Verify the digital signatures of the specified content using the detached signatureData.
		/// </summary>
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
		public override IList<IDigitalSignature> Verify (Stream content, Stream signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			var parser = new CmsSignedDataParser (new CmsTypedStream (content), signatureData);

			parser.GetSignedContent ().Drain ();

			return GetDigitalSignatures (parser);
		}

		/// <summary>
		/// Verify the digital signatures of the specified signedData and extract the original content.
		/// </summary>
		/// <returns>The unencapsulated <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="signatures">The digital signatures.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Verify (Stream signedData, out IList<IDigitalSignature> signatures)
		{
			if (signedData == null)
				throw new ArgumentNullException ("signedData");

			var signedDataParser = new CmsSignedDataParser (signedData);
			var signed = signedDataParser.GetSignedContent ();

			var parser = new MimeParser (signed.ContentStream, MimeFormat.Entity);
			var entity = parser.ParseEntity ();

			signatures = GetDigitalSignatures (signedDataParser);

			return entity;
		}

		Stream Envelope (CmsRecipientCollection recipients, Stream content)
		{
			var cms = new CmsEnvelopedDataGenerator ();
			int count = 0;

			foreach (var recipient in recipients) {
				cms.AddKeyTransRecipient (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			// FIXME: how to decide which algorithm to use?
			var input = new CmsProcessableInputStream (content);
			var envelopedData = cms.Generate (input, CmsEnvelopedGenerator.DesEde3Cbc);

			return new MemoryStream (envelopedData.GetEncoded (), false);
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
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
		public ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, Envelope (recipients, content));
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
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
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return Encrypt (GetCmsRecipients (recipients), content);
		}

		/// <summary>
		/// Signs and encrypts the specified content for the specified recipients.
		/// </summary>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Mime SignAndEncrypt (CmsSigner signer, CmsRecipientCollection recipients, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			using (var signed = Sign (signer, content, true)) {
				return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, Envelope (recipients, signed));
			}
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients, optionally
		/// signing the content if the signer provided is not null.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
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
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return SignAndEncrypt (GetCmsSigner (signer, digestAlgo), GetCmsRecipients (recipients), content);
		}

		/// <summary>
		/// Decrypt the specified encryptedData.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData, out IList<IDigitalSignature> signatures)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new CmsEnvelopedDataParser (encryptedData);
			var recipients = enveloped.GetRecipientInfos ();
			var algorithm = enveloped.EncryptionAlgorithmID;
			AsymmetricKeyParameter key;

			foreach (RecipientInformation recipient in recipients.GetRecipients ()) {
				if ((key = GetPrivateKey (recipient.RecipientID)) == null)
					continue;

				var content = recipient.GetContent (key);

				try {
					var parser = new CmsSignedDataParser (content);
					var signed = parser.GetSignedContent ();

					using (var memory = new MemoryStream ()) {
						signed.ContentStream.CopyTo (memory, 4096);
						content = memory.ToArray ();
					}

					signatures = GetDigitalSignatures (parser);
				} catch (Exception ex) {
					Console.WriteLine ("Failed to verify signed data: {0}", ex);
					signatures = null;
				}

				using (var memory = new MemoryStream (content, false)) {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					return parser.ParseEntity ();
				}
			}

			throw new CmsException ("Can't decrypt.");
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
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
		public virtual void Import (Stream stream, string password)
		{
			throw new NotSupportedException ();
		}

		/// <summary>
		/// Exports the certificates for the specified mailboxes.
		/// </summary>
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
				throw new ArgumentNullException ("mailboxes");

			var certificates = new X509CertificateStore ();
			int count = 0;

			foreach (var mailbox in mailboxes) {
				var recipient = GetCmsRecipient (mailbox);
				certificates.Add (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No mailboxes specified.", "mailboxes");

			var cms = new CmsSignedDataStreamGenerator ();
			var memory = new MemoryStream ();

			cms.AddCertificates (certificates);
			cms.Open (memory).Close ();
			memory.Position = 0;

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, memory);
		}
	}
}
