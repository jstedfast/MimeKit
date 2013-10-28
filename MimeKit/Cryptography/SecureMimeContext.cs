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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	public class SecureMimeContext : CryptographyContext
	{
		/// <summary>
		/// Gets the certificate store.
		/// </summary>
		/// <value>The certificate store.</value>
		public X509Store CertificateStore {
			get; protected set;
		}

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
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SecureMimeContext"/> class.
		/// </summary>
		/// <param name="store">The certificate store.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="store"/> is <c>null</c>.
		/// </exception>
		public SecureMimeContext (X509Store store)
		{
			if (store == null)
				throw new ArgumentNullException ("store");

			CertificateStore = store;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SecureMimeContext"/> class.
		/// </summary>
		public SecureMimeContext ()
		{
			CertificateStore = new X509Store (StoreLocation.CurrentUser);
			CertificateStore.Open (OpenFlags.ReadWrite);
		}

		/// <summary>
		/// Gets the encryption certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="exporting"><c>true</c> if the certificate will be exported; otherwise <c>false</c>.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual X509Certificate2 GetEncryptionCertificate (MailboxAddress mailbox, bool exporting)
		{
			var certificates = CertificateStore.Certificates;//.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName, false) == mailbox.Address) {
					if (!exporting || !certificate.HasPrivateKey)
						return certificate;

					// Note: this is to get rid of the private key
					return new X509Certificate2 (certificate.RawData);
				}
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
		}

		/// <summary>
		/// Gets the certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual X509Certificate2 GetSigningCertificate (MailboxAddress mailbox)
		{
			var certificates = CertificateStore.Certificates;//.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName, false) == mailbox.Address) {

					return certificate;
				}
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

		/// <summary>
		/// Gets the cms signer for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The cms signer.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			var signer = new CmsSigner (GetSigningCertificate (mailbox));
			signer.DigestAlgorithm.FriendlyName = digestAlgo.ToString ().ToUpperInvariant ();
			signer.IncludeOption = X509IncludeOption.EndCertOnly;
			return signer;
		}

		/// <summary>
		/// Gets the cms recipient for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The cms recipient.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			return new CmsRecipient (GetEncryptionCertificate (mailbox, false));
		}

		/// <summary>
		/// Gets the cms recipients for the specified <see cref="MimeKit.MailboxAddress"/>es.
		/// </summary>
		/// <returns>The cms recipients.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		protected virtual CmsRecipientCollection GetCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			var recipients = new CmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetCmsRecipient (mailbox));

			return recipients;
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

			if (content == null)
				throw new ArgumentNullException ("content");

			var contentInfo = new ContentInfo (content);
			var signed = new SignedCms (contentInfo, true);

			signed.ComputeSignature (signer, false);
			var data = signed.Encode ();

			return new ApplicationPkcs7Signature (new MemoryStream (data, false));
		}

		IList<IDigitalSignature> GetDigitalSignatures (SignerInfoCollection signerInfos)
		{
			var signatures = new List<IDigitalSignature> ();

			foreach (var signerInfo in signerInfos) {
				var signature = new SecureMimeDigitalSignature (signerInfo);
				var certificate = (SecureMimeDigitalCertificate) signature.SignerCertificate;

				if (certificate.ExpirationDate < DateTime.Now) {
					signature.Errors |= DigitalSignatureError.CertificateExpired;
					signature.Status = DigitalSignatureStatus.Error;
				}

				var chain = new X509Chain ();
				chain.ChainPolicy.UrlRetrievalTimeout = OnlineCertificateRetrievalTimeout;
				chain.ChainPolicy.RevocationMode = AllowOnlineCertificateRetrieval ? X509RevocationMode.Online : X509RevocationMode.Offline;
				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				//if (AllowSelfSignedCertificates)
				//	chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
				chain.ChainPolicy.VerificationTime = DateTime.Now;

				if (!chain.Build (signerInfo.Certificate)) {
					for (int i = 0; i < chain.ChainStatus.Length; i++) {
						if (chain.ChainStatus[i].Status.HasFlag (X509ChainStatusFlags.Revoked)) {
							signature.Errors |= DigitalSignatureError.CertificateRevoked;
							signature.Status = DigitalSignatureStatus.Error;
						}

						certificate.ChainStatus |= chain.ChainStatus[i].Status;
					}
				}

				if (signature.Status != DigitalSignatureStatus.Error) {
					try {
						signerInfo.CheckSignature (true);
					} catch (CryptographicException) {
						signature.Status = DigitalSignatureStatus.Bad;
					}
				}

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

			var contentInfo = new ContentInfo (content);
			var signed = new SignedCms (contentInfo, true);

			signed.Decode (signatureData);

			return GetDigitalSignatures (signed.SignerInfos);
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

			var signed = new SignedCms ();
			signed.Decode (signedData);

			content = signed.ContentInfo.Content;

			return GetDigitalSignatures (signed.SignerInfos);
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

			var contentInfo = new ContentInfo (content);
			var enveloped = new EnvelopedCms (contentInfo);
			enveloped.Encrypt (recipients);
			var data = enveloped.Encode ();

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

			var contentInfo = new ContentInfo (content);
			var signed = new SignedCms (contentInfo, false);
			signed.ComputeSignature (signer, false);
			contentInfo = new ContentInfo (signed.Encode ());
			var enveloped = new EnvelopedCms (contentInfo);
			enveloped.Encrypt (recipients);
			var data = enveloped.Encode ();

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
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while decrypting.
		/// </exception>
		public override MimeEntity Decrypt (byte[] encryptedData, out IList<IDigitalSignature> signatures)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new EnvelopedCms ();
			enveloped.Decode (encryptedData);
			enveloped.Decrypt (CertificateStore.Certificates);

			// now that we've decrypted the data, let's see if it is signed...
			var signedData = enveloped.Encode ();
			byte[] content;

			try {
				signatures = Verify (signedData, out content);
			} catch (CryptographicException) {
				content = signedData;
				signatures = null;
			}

			using (var memory = new MemoryStream (content, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				return parser.ParseEntity ();
			}
		}

		/// <summary>
		/// Imports keys (or certificates).
		/// </summary>
		/// <param name="rawData">The raw key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="rawData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while importing.
		/// </exception>
		public override void ImportKeys (byte[] rawData)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			var certs = new X509Certificate2Collection ();
			certs.Import (rawData);

			CertificateStore.AddRange (certs);
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

			var certificates = new X509Certificate2Collection ();
			foreach (var mailbox in mailboxes) {
				var cert = GetEncryptionCertificate (mailbox, true);
				certificates.Add (cert);
			}

			if (certificates.Count == 0)
				throw new ArgumentException ();

			return ExportKeys (certificates);
		}

		/// <summary>
		/// Exports the specified certificates.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="certificates">The certificates.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificates"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while exporting.
		/// </exception>
		public ApplicationPkcs7Mime ExportKeys (X509Certificate2Collection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			var rawData = certificates.Export (X509ContentType.Pkcs12);
			var content = new MemoryStream (rawData, false);

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, content);
		}

		/// <summary>
		/// Exports the key.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">
		/// An error occurred while exporting.
		/// </exception>
		public ApplicationPkcs7Mime ExportKey (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return ExportKeys (new X509Certificate2Collection (certificate));
		}

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && CertificateStore != null) {
				CertificateStore.Close ();
				CertificateStore = null;
			}

			base.Dispose (disposing);
		}
	}

	/// <summary>
	/// An S/MIME digital certificate.
	/// </summary>
	public class SecureMimeDigitalCertificate : IDigitalCertificate
	{
		internal SecureMimeDigitalCertificate (SignerInfo signerInfo)
		{
			Certificate = signerInfo.Certificate;
			TrustLevel = TrustLevel.Undefined;
		}

		SecureMimeDigitalCertificate ()
		{
		}

		/// <summary>
		/// Gets the <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2" />.
		/// </summary>
		/// <value>The certificate.</value>
		public X509Certificate2 Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the chain status.
		/// </summary>
		/// <value>The chain status.</value>
		public X509ChainStatusFlags ChainStatus {
			get; internal set;
		}

		#region IDigitalCertificate implementation

		/// <summary>
		/// Gets the public key algorithm.
		/// </summary>
		/// <value>The public key algorithm.</value>
		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get; private set;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get { return Certificate.GetNameInfo (X509NameType.SimpleName, false); }
		}

		/// <summary>
		/// Gets the email address.
		/// </summary>
		/// <value>The email address.</value>
		public string Email {
			get { return Certificate.GetNameInfo (X509NameType.EmailName, false); }
		}

		/// <summary>
		/// Gets the fingerprint.
		/// </summary>
		/// <value>The fingerprint.</value>
		public string Fingerprint {
			get { return Certificate.Thumbprint; }
		}

		/// <summary>
		/// Gets the creation date.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get { return Certificate.NotBefore; }
		}

		/// <summary>
		/// Gets the expiration date.
		/// </summary>
		/// <value>The expiration date.</value>
		public DateTime ExpirationDate {
			get { return Certificate.NotAfter; }
		}

		/// <summary>
		/// Gets the trust level.
		/// </summary>
		/// <value>The trust level.</value>
		public TrustLevel TrustLevel {
			get; internal set;
		}

		#endregion
	}

	/// <summary>
	/// An S/MIME digital signature.
	/// </summary>
	public class SecureMimeDigitalSignature : IDigitalSignature
	{
		internal SecureMimeDigitalSignature (SignerInfo signerInfo)
		{
			SignerCertificate = new SecureMimeDigitalCertificate (signerInfo);
			SignerInfo = signerInfo;
		}

		SecureMimeDigitalSignature ()
		{
		}

		public SignerInfo SignerInfo {
			get; private set;
		}

		#region IDigitalSignature implementation

		public IDigitalCertificate SignerCertificate {
			get; private set;
		}

		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get; internal set;
		}

		public DigestAlgorithm DigestAlgorithm {
			get; internal set;
		}

		public DigitalSignatureStatus Status {
			get; internal set;
		}

		public DigitalSignatureError Errors {
			get; internal set;
		}

		public DateTime CreationDate {
			get; internal set;
		}

		public DateTime ExpirationDate {
			get; internal set;
		}

		#endregion
	}
}
