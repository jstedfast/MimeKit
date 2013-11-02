//
// DummySecureMimeContext.cs
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.X509;

using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.Cms;

namespace UnitTests {
	public class DummySecureMimeContext : SecureMimeContext
	{
		internal readonly Dictionary<X509Certificate, AsymmetricKeyParameter> keys = new Dictionary<X509Certificate, AsymmetricKeyParameter> ();
		internal readonly List<X509Certificate> certificates = new List<X509Certificate> ();

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate based on the selector.
		/// </summary>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			foreach (var certificate in certificates) {
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
			foreach (var certificate in certificates) {
				AsymmetricKeyParameter key;

				if (!keys.TryGetValue (certificate, out key))
					continue;

				if (!selector.Match (certificate))
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
			foreach (var certificate in certificates) {
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
			foreach (var certificate in certificates) {
				AsymmetricKeyParameter key;

				if (!keys.TryGetValue (certificate, out key))
					continue;

				if (certificate.GetSubjectEmail () == mailbox.Address) {
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
			using (var memory = new MemoryStream (rawData, false)) {
				var pkcs12 = new Pkcs12Store (memory, password.ToCharArray ());

				foreach (string alias in pkcs12.Aliases) {
					if (pkcs12.IsKeyEntry (alias)) {
						var chain = pkcs12.GetCertificateChain (alias);
						var entry = pkcs12.GetKey (alias);

						for (int i = 0; i < chain.Length; i++)
							certificates.Add (chain[i].Certificate);

						keys.Add (chain[0].Certificate, entry.Key);
					} else if (pkcs12.IsCertificateEntry (alias)) {
						var entry = pkcs12.GetCertificate (alias);
						certificates.Add (entry.Certificate);
					}
				}
			}
		}

		#endregion

		#region implemented abstract members of CryptographyContext

		public override void ImportKeys (byte[] rawData)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			var signed = new CmsSignedDataParser (rawData);
			var certs = signed.GetCertificates ("Collection");
			var signers = signed.GetSignerInfos ();

			foreach (SignerInformation signerInfo in signers.GetSigners ()) {
				var matches = certs.GetMatches (signerInfo.SignerID);

				foreach (X509Certificate certificate in matches) {
					certificates.Add (certificate);
				}
			}
		}

		#endregion
	}
}
