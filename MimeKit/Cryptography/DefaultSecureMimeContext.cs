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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	public class DefaultSecureMimeContext : SecureMimeContext
	{
		protected static readonly string DefaultCertificateStorePath;

		readonly X509CertificateStore store;
		readonly string password;
		readonly string path;

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

			DefaultCertificateStorePath = Path.Combine (path, "smime.p12");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <param name="path">The path to the pkcs12 backing file.</param>
		/// <param name="password">The password for the pkcs12 file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="path"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while reading the file.
		/// </exception>
		protected DefaultSecureMimeContext (string path, string password)
		{
			store = new X509CertificateStore ();

			try {
				store.Import (path, password);
			} catch (FileNotFoundException) {
			}

			this.password = password;
			this.path = path;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while reading the backing pkcs12 file.
		/// </exception>
		public DefaultSecureMimeContext () : this (DefaultCertificateStorePath, "no.secret")
		{
		}

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			return store.GetMatches (selector).FirstOrDefault ();
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
		/// Imports the pkcs12-encoded certificate and key data.
		/// </summary>
		/// <param name="stream">The raw certificate data.</param>
		/// <param name="password">The password to unlock the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public override void ImportPkcs12 (Stream stream, string password)
		{
			store.Import (stream, password);

			// FIXME: save the certificates
		}

		#endregion

		#region implemented abstract members of CryptographyContext

		/// <summary>
		/// Imports certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </summary>
		/// <param name="stream">The raw key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public override void Import (Stream stream)
		{
			store.Import (stream);

			// FIXME: save the certificates
		}

		#endregion
	}
}
