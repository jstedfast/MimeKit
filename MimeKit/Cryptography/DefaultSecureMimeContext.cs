//
// DefaultSecureMimeContext.cs
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

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A default <see cref="SecureMimeContext"/> implementation that uses a pkcs12 file as a certificate and private key store.
	/// </summary>
	public class DefaultSecureMimeContext : SecureMimeContext
	{
		/// <summary>
		/// The default path for the Certificate Revocation Lists (CRLs).
		/// </summary>
		/// <remarks>
		/// <para>On Microsoft Windows-based systems, this path will be something like <c>C:\Users\UserName\AppData\Roaming\mimekit\revoked.crl</c>.</para>
		/// <para>On Unix systems such as Linux and Mac OS X, this path will be <c>~/.mimekit/revoked.crl</c>.</para>
		/// </remarks>
		protected static readonly string DefaultCertificateRevocationListsPath;

		/// <summary>
		/// The default store path for certificates belonging to people in the user's addressbook.
		/// </summary>
		/// <remarks>
		/// <para>On Microsoft Windows-based systems, this path will be something like <c>C:\Users\UserName\AppData\Roaming\mimekit\addressbook.crt</c>.</para>
		/// <para>On Unix systems such as Linux and Mac OS X, this path will be <c>~/.mimekit/addressbook.crt</c>.</para>
		/// </remarks>
		protected static readonly string DefaultAddressBookCertificatesPath;

		/// <summary>
		/// The default store path for root (CA) certificates.
		/// </summary>
		/// <remarks>
		/// <para>On Microsoft Windows-based systems, this path will be something like <c>C:\Users\UserName\AppData\Roaming\mimekit\root.crt</c>.</para>
		/// <para>On Unix systems such as Linux and Mac OS X, this path will be <c>~/.mimekit/root.crt</c>.</para>
		/// </remarks>
		protected static readonly string DefaultRootCertificatesPath;

		/// <summary>
		/// The default store path for user certificates and keys.
		/// </summary>
		/// <remarks>
		/// <para>On Microsoft Windows-based systems, this path will be something like <c>C:\Users\UserName\AppData\Roaming\mimekit\user.p12</c>.</para>
		/// <para>On Unix systems such as Linux and Mac OS X, this path will be <c>~/.mimekit/user.p12</c>.</para>
		/// </remarks>
		protected static readonly string DefaultUserCertificatesPath;

		readonly X509CertificateStore addressbook;
		readonly X509CertificateStore store;
		readonly X509CertificateStore root;
		readonly HashSet<X509Crl> crls;

		readonly string addressbookFileName;
		readonly string revokedFileName;
		readonly string userFileName;
		readonly string password;

		static DefaultSecureMimeContext ()
		{
			string path;

			if (Path.DirectorySeparatorChar == '\\') {
				var appData = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				path = Path.Combine (appData, "Roaming", "mimekit");
			} else {
				var home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				path = Path.Combine (home, ".mimekit");
			}

			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);

			DefaultCertificateRevocationListsPath = Path.Combine (path, "revoked.crl");
			DefaultAddressBookCertificatesPath = Path.Combine (path, "addressbook.crt");
			DefaultRootCertificatesPath = Path.Combine (path, "root.crt");
			DefaultUserCertificatesPath = Path.Combine (path, "user.p12");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <param name="revokedFileName">The path to the revoked certificate lists.</param>
		/// <param name="addressbookFileName">The path to the addressbook certificates.</param>
		/// <param name="rootFileName">The path to the root certificates.</param>
		/// <param name="userFileName">The path to the pkcs12-formatted user certificates.</param>
		/// <param name="password">The password for the pkcs12 user certificates file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="addressbookFileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="rootFileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="userFileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while reading the file.
		/// </exception>
		protected DefaultSecureMimeContext (string revokedFileName, string addressbookFileName, string rootFileName, string userFileName, string password)
		{
			addressbook = new X509CertificateStore ();
			store = new X509CertificateStore ();
			root = new X509CertificateStore ();
			crls = new HashSet<X509Crl> ();

			try {
				using (var file = File.OpenRead (revokedFileName)) {
					var parser = new X509CrlParser ();
					foreach (X509Crl crl in parser.ReadCrls (file))
						crls.Add (crl);
				}
			} catch (FileNotFoundException) {
			}

			try {
				addressbook.Import (addressbookFileName);
			} catch (FileNotFoundException) {
			}

			try {
				store.Import (userFileName, password);
			} catch (FileNotFoundException) {
			}

			try {
				root.Import (rootFileName);
			} catch (FileNotFoundException) {
			}

			this.addressbookFileName = addressbookFileName;
			this.revokedFileName = revokedFileName;
			this.userFileName = userFileName;
			this.password = password;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while reading one of the certificate stores.
		/// </exception>
		public DefaultSecureMimeContext () : this (DefaultCertificateRevocationListsPath, DefaultAddressBookCertificatesPath, DefaultRootCertificatesPath, DefaultUserCertificatesPath, "no.secret")
		{
		}

		void SaveRevocationLists ()
		{
			var filename = Path.GetFileName (revokedFileName) + "~";
			var dirname = Path.GetDirectoryName (revokedFileName);
			var tmp = Path.Combine (dirname, "." + filename);
			var bak = Path.Combine (dirname, filename);

			if (!Directory.Exists (dirname))
				Directory.CreateDirectory (dirname);

			using (var file = File.Create (revokedFileName)) {
				foreach (var crl in crls) {
					var encoded = crl.GetEncoded ();
					file.Write (encoded, 0, encoded.Length);
				}
			}

			if (File.Exists (addressbookFileName))
				File.Replace (tmp, addressbookFileName, bak);
			else
				File.Move (tmp, addressbookFileName);
		}

		void SaveAddressBookCerts ()
		{
			var filename = Path.GetFileName (addressbookFileName) + "~";
			var dirname = Path.GetDirectoryName (addressbookFileName);
			var tmp = Path.Combine (dirname, "." + filename);
			var bak = Path.Combine (dirname, filename);

			if (!Directory.Exists (dirname))
				Directory.CreateDirectory (dirname);

			addressbook.Export (tmp);

			if (File.Exists (addressbookFileName))
				File.Replace (tmp, addressbookFileName, bak);
			else
				File.Move (tmp, addressbookFileName);
		}

		void SaveUserCerts ()
		{
			var filename = Path.GetFileName (userFileName) + "~";
			var dirname = Path.GetDirectoryName (userFileName);
			var tmp = Path.Combine (dirname, "." + filename);
			var bak = Path.Combine (dirname, filename);

			if (!Directory.Exists (dirname))
				Directory.CreateDirectory (dirname);

			store.Export (tmp, password);

			if (File.Exists (userFileName))
				File.Replace (tmp, userFileName, bak);
			else
				File.Move (tmp, userFileName);
		}

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			var certificate = store.GetMatches (selector).FirstOrDefault ();
			if (certificate != null)
				return certificate;

			certificate = addressbook.GetMatches (selector).FirstOrDefault ();
			if (certificate != null)
				return certificate;

			return root.GetMatches (selector).FirstOrDefault ();
		}

		/// <summary>
		/// Gets the private key based on the provided selector.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (IX509Selector selector)
		{
			foreach (var certificate in store.GetMatches (selector)) {
				var key = store.GetPrivateKey (certificate);
				if (key == null)
					continue;

				return key;
			}

			return null;
		}

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <returns>The trusted anchors.</returns>
		protected override Org.BouncyCastle.Utilities.Collections.HashSet GetTrustedAnchors ()
		{
			var anchors = new Org.BouncyCastle.Utilities.Collections.HashSet ();

			foreach (var certificate in root.Certificates) {
				anchors.Add (new TrustAnchor (certificate, null));
			}

			return anchors;
		}

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <returns>The intermediate certificates.</returns>
		protected override IX509Store GetIntermediateCertificates ()
		{
			return addressbook;
		}

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <returns>The certificate revocation lists.</returns>
		protected override IX509Store GetCertificateRevocationLists ()
		{
			return X509StoreFactory.Create ("Crl/Collection", new X509CollectionStoreParameters (crls.ToList ()));
		}

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			foreach (var certificate in store.Certificates) {
				if (certificate.GetSubjectEmailAddress () == mailbox.Address)
					return new CmsRecipient (certificate);
			}

			foreach (var certificate in addressbook.Certificates) {
				if (certificate.GetSubjectEmailAddress () == mailbox.Address)
					return new CmsRecipient (certificate);
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
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
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			foreach (var certificate in store.Certificates) {
				var key = store.GetPrivateKey (certificate);

				if (key != null && certificate.GetSubjectEmailAddress () == mailbox.Address) {
					var signer = new CmsSigner (certificate, key);
					signer.DigestAlgorithm = digestAlgo;
					return signer;
				}
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

		/// <summary>
		/// Import the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			addressbook.Add (certificate);
			SaveAddressBookCerts ();
		}

		/// <summary>
		/// Import the specified certificate revocation list.
		/// </summary>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			crls.Add (crl);
			SaveRevocationLists ();
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void Import (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			store.Import (stream, password);
			SaveUserCerts ();
		}

		#endregion
	}
}
