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

using Org.BouncyCastle;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
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
		public override string GetMicAlgName (DigestAlgorithm micalg)
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
		/// Gets the X509 certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The X509 certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected abstract X509Certificate GetCertificate (MailboxAddress mailbox);

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

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			return new CmsRecipient (GetCertificate (mailbox));
		}

		/// <summary>
		/// Gets the cms recipients for the specified <see cref="MimeKit.MailboxAddress"/>es.
		/// </summary>
		/// <returns>The cms recipients.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the specified <paramref name="mailboxes"/> could not be found.
		/// </exception>
		protected CmsRecipientCollection GetRecipientCertificates (IEnumerable<MailboxAddress> mailboxes)
		{
			var recipients = new CmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetCmsRecipient (mailbox));

			return recipients;
		}

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="recipient">The recipient.</param>
		protected abstract AsymmetricKeyParameter GetPrivateKey (RecipientID recipient);

		protected static string GetOid (DigestAlgorithm digestAlgo)
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while signing.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return Sign (cmsSigner, content);
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while signing.
		/// </exception>
		public ApplicationPkcs7Signature Sign (CmsSigner signer, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", "signer");

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var generator = new CmsSignedDataStreamGenerator ();
			generator.AddSigner (signer.PrivateKey, signer.Certificate, GetOid (signer.DigestAlgorithm),
				signer.SignedAttributes, signer.UnsignedAttributes);

			var memory = new MemoryStream ();

			using (var stream = generator.Open (memory)) {
				stream.Write (content, 0, content.Length);
			}

			memory.Position = 0;

			return new ApplicationPkcs7Signature (memory);
		}

		X509Certificate GetCertificate (IX509Store store, IX509Selector selector)
		{
			var matches = store.GetMatches (selector);

			foreach (X509Certificate certificate in matches) {
				return certificate;
			}

			return null;
		}

		IList<IDigitalSignature> GetDigitalSignatures (SignerInformationStore store, IX509Store certificates)
		{
			var signatures = new List<IDigitalSignature> ();

			foreach (SignerInformation signerInfo in store.GetSigners ()) {
				var cert = GetCertificate (certificates, signerInfo.SignerID);
				var signature = new SecureMimeDigitalSignature (signerInfo);
				var certificate = new SecureMimeDigitalCertificate (cert);

				signature.SignerCertificate = certificate;

				// Verify that the signature is good vs bad
				if (!signerInfo.Verify (cert)) {
					signature.Status = DigitalSignatureStatus.Bad;
				}

				if (DateTime.Now > certificate.ExpirationDate) {
					signature.Errors |= DigitalSignatureError.CertificateExpired;
					signature.Status = DigitalSignatureStatus.Error;
				}

				// FIXME: verify the certificate chain with what we have in our local store

//				var chain = new X509Chain ();
//				chain.ChainPolicy.UrlRetrievalTimeout = OnlineCertificateRetrievalTimeout;
//				chain.ChainPolicy.RevocationMode = AllowOnlineCertificateRetrieval ? X509RevocationMode.Online : X509RevocationMode.Offline;
//				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
//				//if (AllowSelfSignedCertificates)
//				//	chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
//				chain.ChainPolicy.VerificationTime = DateTime.Now;
//
//				if (!chain.Build (signerInfo.Certificate)) {
//					for (int i = 0; i < chain.ChainStatus.Length; i++) {
//						if (chain.ChainStatus[i].Status.HasFlag (X509ChainStatusFlags.Revoked)) {
//							signature.Errors |= DigitalSignatureError.CertificateRevoked;
//							signature.Status = DigitalSignatureStatus.Error;
//						}
//
//						certificate.ChainStatus |= chain.ChainStatus[i].Status;
//					}
//				}

				// FIXME: how do I get the creation/expiration timestamps?
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while verifying the signature.
		/// </exception>
		public override IList<IDigitalSignature> Verify (byte[] content, byte[] signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			var contentStream = new MemoryStream (content, false);
			var signed = new CmsSignedDataParser (new CmsTypedStream (contentStream), signatureData);
			var certificates = signed.GetCertificates ("Collection");
			var signers = signed.GetSignerInfos ();

			return GetDigitalSignatures (signers, certificates);
		}

		/// <summary>
		/// Verify the digital signatures of the specified signedData and extract the original content.
		/// </summary>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="content">The original content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while verifying the signature.
		/// </exception>
		public IList<IDigitalSignature> Verify (byte[] signedData, out byte[] content)
		{
			if (signedData == null)
				throw new ArgumentNullException ("signedData");

			var signed = new CmsSignedDataParser (signedData);
			var certificates = signed.GetCertificates ("Collection");
			var signers = signed.GetSignerInfos ();

			var signedContent = signed.GetSignedContent ();
			using (var memory = new MemoryStream ()) {
				signedContent.ContentStream.CopyTo (memory, 4096);
				content = memory.ToArray ();
			}

			return GetDigitalSignatures (signers, certificates);
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while encrypting.
		/// </exception>
		public ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, byte[] content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var generator = new CmsEnvelopedDataGenerator ();
			int count = 0;

			foreach (var recipient in recipients) {
				generator.AddKeyTransRecipient (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			// FIXME: how to decide which algorithm to use?
			var envelope = generator.Generate (new CmsProcessableByteArray (content), CmsEnvelopedGenerator.DesEde3Cbc);
			var data = envelope.GetEncoded ();

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, new MemoryStream (data, false));
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while encrypting.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, byte[] content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return Encrypt (GetRecipientCertificates (recipients), content);
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while signing or encrypting.
		/// </exception>
		public ApplicationPkcs7Mime SignAndEncrypt (CmsSigner signer, CmsRecipientCollection recipients, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var generator = new CmsSignedDataGenerator ();
			generator.AddSigner (signer.PrivateKey, signer.Certificate, GetOid (signer.DigestAlgorithm),
				signer.SignedAttributes, signer.UnsignedAttributes);
			var signedData = generator.Generate (new CmsProcessableByteArray (content), true);

			var cms = new CmsEnvelopedDataGenerator ();
			int count = 0;

			foreach (var recipient in recipients) {
				cms.AddKeyTransRecipient (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			var envelopedData = cms.Generate (signedData.SignedContent, CmsEnvelopedGenerator.DesEde3Cbc);
			var data = envelopedData.GetEncoded ();

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, new MemoryStream (data, false));
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while signing or encrypting.
		/// </exception>
		public override MimePart SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return SignAndEncrypt (GetCmsSigner (signer, digestAlgo), GetRecipientCertificates (recipients), content);
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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while decrypting.
		/// </exception>
		public override MimeEntity Decrypt (byte[] encryptedData, out IList<IDigitalSignature> signatures)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new CmsEnvelopedDataParser (encryptedData);
			var recipients = enveloped.GetRecipientInfos ();
			var algorithm = enveloped.EncryptionAlgorithmID;

			foreach (RecipientInformation recipient in recipients.GetRecipients ()) {
				var key = GetPrivateKey (recipient.RecipientID);
				if (key == null)
					continue;

				var content = recipient.GetContent (key);

				try {
					var signed = new CmsSignedDataParser (content);
					var certificates = signed.GetCertificates ("Collection");
					var signers = signed.GetSignerInfos ();

					var signedContent = signed.GetSignedContent ();
					using (var memory = new MemoryStream ()) {
						signedContent.ContentStream.CopyTo (memory, 4096);
						content = memory.ToArray ();
					}

					signatures = GetDigitalSignatures (signers, certificates);
				} catch {
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
		/// Imports the pkcs12-encoded certificate and key data.
		/// </summary>
		/// <param name="rawData">The raw certificate data.</param>
		/// <param name="password">The password to unlock the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="rawData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public virtual void ImportPkcs12 (byte[] rawData, string password)
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
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while exporting.
		/// </exception>
		public override MimePart ExportKeys (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			var certificates = new List<X509Certificate> ();
			foreach (var mailbox in mailboxes) {
				var cert = GetCertificate (mailbox);
				certificates.Add (cert);
			}

			if (certificates.Count == 0)
				throw new ArgumentException ("No mailboxes specified.", "mailboxes");

			// FIXME:
			return null;
		}
	}
}
