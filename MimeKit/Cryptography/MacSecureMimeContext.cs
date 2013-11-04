//
// MacSecureMimeContext.cs
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

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;

using MimeKit.MacInterop;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	public class MacSecureMimeContext : SecureMimeContext
	{
		SecKeychain keychain;

		public MacSecureMimeContext (string path, string password)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (password == null)
				throw new ArgumentNullException ("password");

			keychain = SecKeychain.Create (path, password);
		}

		public MacSecureMimeContext ()
		{
			keychain = SecKeychain.Default;
		}

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			foreach (var certificate in keychain.GetCertificates ((CssmKeyUse) 0)) {
				if (selector.Match (certificate))
					return certificate;
			}

			return null;
		}

		/// <summary>
		/// Gets the private key based on the provided selector.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (IX509Selector selector)
		{
			foreach (var signer in keychain.GetAllCmsSigners ()) {
				if (selector.Match (signer.Certificate))
					return signer.PrivateKey;
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
			foreach (var certificate in keychain.GetCertificates (CssmKeyUse.Encrypt)) {
				if (certificate.GetSubjectEmail () == mailbox.Address)
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
			foreach (var signer in keychain.GetAllCmsSigners ()) {
				if (signer.Certificate.GetSubjectEmail () == mailbox.Address) {
					signer.DigestAlgorithm = digestAlgo;
					return signer;
				}
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

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
			if (stream == null)
				throw new ArgumentNullException ("stream");

			// FIXME: implement this
		}

		/// <summary>
		/// Imports the pkcs12-encoded certificate and key data.
		/// </summary>
		/// <param name="stream">The raw certificate data.</param>
		/// <param name="password">The password to unlock the stream.</param>
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
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());
			foreach (string alias in pkcs12.Aliases) {
				if (pkcs12.IsKeyEntry (alias)) {
					var chain = pkcs12.GetCertificateChain (alias);
					var entry = pkcs12.GetKey (alias);

					for (int i = 0; i < chain.Length; i++)
						keychain.Add (chain[i].Certificate);

					keychain.Add (entry.Key);
				} else if (pkcs12.IsCertificateEntry (alias)) {
					var entry = pkcs12.GetCertificate (alias);
					keychain.Add (entry.Certificate);
				}
			}
		}

		/// <summary>
		/// Releases all resources used by the <see cref="MimeKit.Cryptography.MacSecureMimeContext"/> object.
		/// </summary>
		/// <param name="disposing">If <c>true</c>, this method is being called by
		/// <see cref="MimeKit.Cryptography.CryptographyContext.Dispose"/>;
		/// otherwise it is being called by the finalizer.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && keychain != SecKeychain.Default) {
				keychain.Dispose ();
				keychain = null;
			}

			base.Dispose (disposing);
		}
	}
}
