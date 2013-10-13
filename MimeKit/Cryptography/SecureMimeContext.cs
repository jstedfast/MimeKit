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
		/// Gets the certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="flags">Key usage flags.</param>
		protected virtual X509Certificate2 GetCertificate (MailboxAddress mailbox, X509KeyUsageFlags flags)
		{
			var certificates = CertificateStore.Certificates;//.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName, false) == mailbox.Address)
					return certificate;
			}

			if (flags == X509KeyUsageFlags.DigitalSignature)
				throw new ArgumentException ("A valid signing certificate could not be found.", "mailbox");

			throw new ArgumentException ("A valid certificate could not be found.", "mailbox");
		}

		/// <summary>
		/// Gets the cms signer for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The cms signer.</returns>
		/// <param name="mailbox">The mailbox.</param>
		protected virtual CmsSigner GetCmsSigner (MailboxAddress mailbox)
		{
			var signer = new CmsSigner (GetCertificate (mailbox, X509KeyUsageFlags.DigitalSignature));
			signer.IncludeOption = X509IncludeOption.EndCertOnly;
			return signer;
		}

		/// <summary>
		/// Gets the cms recipient for the specified <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The cms recipient.</returns>
		/// <param name="mailbox">The mailbox.</param>
		protected virtual CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			return new CmsRecipient (GetCertificate (mailbox, X509KeyUsageFlags.DataEncipherment));
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
		/// <param name="content">The content.</param>
		/// <param name="digestAlgo">The digest algorithm used.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, byte[] content, out string digestAlgo)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			var cmsSigner = GetCmsSigner (signer);

			digestAlgo = cmsSigner.DigestAlgorithm.FriendlyName;

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

			return new ApplicationPkcs7Signature (new MemoryStream (data));
		}

		/// <summary>
		/// Verify the digital signatures of the specified content using the detached signatureData.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The detached signature data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		public SignerInfoCollection Verify (byte[] content, byte[] signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			var contentInfo = new ContentInfo (content);
			var signed = new SignedCms (contentInfo, true);

			signed.Decode (signatureData);
			signed.CheckSignature (true);

			return signed.SignerInfos;
		}

		/// <summary>
		/// Verify the digital signatures of the specified signedData and extract the original content.
		/// </summary>
		/// <returns>A signer info collection.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="content">The original content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		public SignerInfoCollection Verify (byte[] signedData, out byte[] content)
		{
			if (signedData == null)
				throw new ArgumentNullException ("signedData");

			var signed = new SignedCms ();
			signed.Decode (signedData);
			signed.CheckSignature (true);

			content = signed.ContentInfo.Content;

			return signed.SignerInfos;
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

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, new MemoryStream (data));
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

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, new MemoryStream (data));
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients, optionally
		/// signing the content if the signer provided is not null.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
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
		public override MimePart SignAndEncrypt (MailboxAddress signer, IEnumerable<MailboxAddress> recipients, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return SignAndEncrypt (GetCmsSigner (signer), GetCmsRecipients (recipients), content);
		}

		/// <summary>
		/// Decrypt the specified encryptedData.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="signers">The signers.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		public MimeEntity Decrypt (byte[] encryptedData, out RecipientInfoCollection recipients, out SignerInfoCollection signers)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new EnvelopedCms ();
			enveloped.Decode (encryptedData);
			enveloped.Decrypt (CertificateStore.Certificates);

			recipients = enveloped.RecipientInfos;

			// now that we've decrypted the data, let's see if it is signed...
			var signedData = enveloped.Encode ();
			byte[] content;

			try {
				signers = Verify (signedData, out content);
			} catch (CryptographicException) {
				content = signedData;
				signers = null;
			}

			using (var memory = new MemoryStream (content)) {
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
		public override MimePart ExportKeys (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			var certificates = new X509Certificate2Collection ();
			foreach (var mailbox in mailboxes) {
				var cert = GetCertificate (mailbox, X509KeyUsageFlags.DataEncipherment);
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
		public ApplicationPkcs7Mime ExportKeys (X509Certificate2Collection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			var rawData = certificates.Export (X509ContentType.Pkcs12);
			var content = new MemoryStream (rawData);

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
}
