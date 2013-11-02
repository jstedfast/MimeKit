//
// WindowsSecureMimeContext.cs
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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Cms;

namespace MimeKit.Cryptography {
	public class WindowsSecureMimeContext : SecureMimeContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.WindowsSecureMimeContext"/> class.
		/// </summary>
		/// <param name="store">The X509 certificate store.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="store"/> is <c>null</c>.
		/// </exception>
		public WindowsSecureMimeContext (X509Store store)
		{
			if (store == null)
				throw new ArgumentNullException ("store");

			CertificateStore = store;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.WindowsSecureMimeContext"/> class.
		/// </summary>
		public WindowsSecureMimeContext ()
		{
			CertificateStore = new X509Store (StoreName.My, StoreLocation.CurrentUser);
			CertificateStore.Open (OpenFlags.ReadWrite);
		}

		/// <summary>
		/// Gets or sets the X509 certificate store.
		/// </summary>
		/// <value>The X509 certificate store.</value>
		public X509Store CertificateStore {
			get; protected set;
		}

		/// <summary>
		/// Gets the X509 certificate associated with the <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override Org.BouncyCastle.X509.X509Certificate GetCertificate (MailboxAddress mailbox)
		{
			var certificates = CertificateStore.Certificates;//.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName, false) == mailbox.Address)
					return DotNetUtilities.FromX509Certificate (certificate);
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
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
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			var certificates = CertificateStore.Certificates;//.Find (X509FindType.FindByKeyUsage, flags, true);

			foreach (var certificate in certificates) {
				if (certificate.GetNameInfo (X509NameType.EmailName, false) != mailbox.Address)
					continue;

				if (!certificate.HasPrivateKey)
					continue;

				var pair = DotNetUtilities.GetKeyPair (certificate.PrivateKey);
				var cert = DotNetUtilities.FromX509Certificate (certificate);
				var signer = new CmsSigner (cert, pair.Private);
				signer.DigestAlgorithm = digestAlgo;

				return signer;
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

		static string ToHexString (byte[] array)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < array.Length; i++)
				builder.Append (array[i].ToString ("X2"));

			return builder.ToString ();
		}

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="recipient">The recipient.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (RecipientID recipient)
		{
			X509FindType type;
			object token;

			if (recipient.SubjectKeyIdentifier != null) {
				type = X509FindType.FindBySubjectKeyIdentifier;
				token = ToHexString (recipient.SubjectKeyIdentifier);
			} else if (recipient.Subject != null) {
				type = X509FindType.FindBySubjectDistinguishedName;
				token = recipient.SubjectAsString;
			} else if (recipient.Issuer != null) {
				type = X509FindType.FindByIssuerDistinguishedName;
				token = recipient.IssuerAsString;
			} else if (recipient.SerialNumber != null) {
				type = X509FindType.FindBySerialNumber;
				token = recipient.SerialNumber.ToString ();
			} else {
				Debug.WriteLine ("Attempting to lookup recipient based on unexpected parameters.");
				return null;
			}

			var matches = CertificateStore.Certificates.Find (type, token, false);
			AsymmetricKeyParameter key = null;
			DateTime now = DateTime.Now;

			foreach (var certificate in matches) {
				if (!certificate.HasPrivateKey)
					continue;

				var pair = DotNetUtilities.GetKeyPair (certificate.PrivateKey);
				key = pair.Private;

				if (now > certificate.NotAfter || now < certificate.NotBefore)
					continue;

				return key;
			}

			return key;
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
		public override void ImportPkcs12 (byte[] rawData, string password)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			if (password == null)
				throw new ArgumentNullException ("password");

			var certs = new X509Certificate2Collection ();
			certs.Import (rawData, password, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

			CertificateStore.AddRange (certs);
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

			if (certificates.Count == 0)
				throw new ArgumentException ("No certificates specified.", "certificates");

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
}
