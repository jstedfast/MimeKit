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
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit {
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
		/// Initializes a new instance of the <see cref="MimeKit.SecureMimeContext"/> class.
		/// </summary>
		/// <param name="store">The certificate store.</param>
		public SecureMimeContext (X509Store store)
		{
			CertificateStore = store;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.SecureMimeContext"/> class.
		/// </summary>
		public SecureMimeContext ()
		{
			CertificateStore = new X509Store (StoreLocation.CurrentUser);
			CertificateStore.Open (OpenFlags.ReadWrite);
		}

		protected virtual X509Certificate2 GetCertificate (string emailName, X509KeyUsageFlags flags)
		{
			var certificates = CertificateStore.Certificates.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName) == emailName)
					return certificate;
			}

			throw new ArgumentException ("A valid certificate could not be found.", "emailName");
		}

		protected virtual CmsSigner GetCmsSigner (string emailName)
		{
			return new CmsSigner (GetCertificate (emailName, X509KeyUsageFlags.DigitalSignature));
		}

		protected virtual CmsRecipient GetCmsRecipient (string emailName)
		{
			return new CmsRecipient (GetCertificate (emailName, X509KeyUsageFlags.DataEncipherment));
		}

		protected virtual CmsRecipientCollection GetCmsRecipients (IList<string> emailNames)
		{
			var recipients = new CmsRecipientCollection ();

			foreach (var emailName in emailNames)
				recipients.Add (GetCmsRecipient (emailName));

			return recipients;
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		public override MimePart Sign (string signer, byte[] content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			var certificates = CertificateStore.Certificates.Find (X509FindType.FindBySubjectName, signer, true);
			if (certificates.Count == 0)
				throw new ArgumentException ("A valid certificate for the specified signer could not be found.", "signer");

			return Sign (GetCmsSigner (signer), content);
		}

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
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
		/// Verify the specified content and signatureData.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The detached signature data.</param>
		public SignerInfoCollection Verify (byte[] content, byte[] signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			var contentInfo = new ContentInfo (content);
			var signed = new SignedCms (contentInfo, true);

			signed.Decode (signatureData);
			signed.CheckSignature (false);

			return signed.SignerInfos;
		}

		/// <summary>
		/// Verify the specified signedData and extract the original content.
		/// </summary>
		/// <returns>A signer info collection.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="content">The original content.</param>
		public SignerInfoCollection Verify (byte[] signedData, out byte[] content)
		{
			if (signedData == null)
				throw new ArgumentNullException ("signedData");

			var signed = new SignedCms ();
			signed.Decode (signedData);
			signed.CheckSignature (false);

			content = signed.ContentInfo.Content;

			return signed.SignerInfos;
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients, optionally
		/// signing the content if the signer provided is not null.
		/// </summary>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		public ApplicationPkcs7Mime Encrypt (CmsSigner signer, CmsRecipientCollection recipients, byte[] content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var contentInfo = new ContentInfo (content);

			if (signer != null) {
				var signed = new SignedCms (contentInfo, false);
				signed.ComputeSignature (signer, false);
				contentInfo = new ContentInfo (signed.Encode ());
			}

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
		public override MimePart Encrypt (string signer, IList<string> recipients, byte[] content)
		{
			if (signer == null)
				return Encrypt (null, GetCmsRecipients (recipients), content);

			return Encrypt (GetCmsSigner (signer), GetCmsRecipients (recipients), content);
		}

		/// <summary>
		/// Decrypt the specified encryptedData.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="signers">The signers.</param>
		public MimeEntity Decrypt (byte[] encryptedData, out RecipientInfoCollection recipients, out SignerInfoCollection signers)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var enveloped = new EnvelopedCms ();
			enveloped.Decode (encryptedData);
			enveloped.Decrypt ();

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
		/// Imports the keys.
		/// </summary>
		/// <param name="keyData">The key data.</param>
		public override void ImportKeys (byte[] keyData)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Exports the keys.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="keys">The keys.</param>
		public override MimePart ExportKeys (IList<string> keys)
		{
			return ExportKeys (keys.Select (emailName => GetCertificate (emailName, X509KeyUsageFlags.DataEncipherment)));
		}

		/// <summary>
		/// Exports the keys.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="certificates">The certificates.</param>
		public ApplicationPkcs7Mime ExportKeys (IEnumerable<X509Certificate2> certificates)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Exports the keys.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="certificate">The certificate.</param>
		public ApplicationPkcs7Mime ExportKeys (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, new MemoryStream (certificate.RawData));
		}
	}
}
