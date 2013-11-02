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

		protected override X509Certificate GetCertificate (MailboxAddress mailbox)
		{
			foreach (var certificate in keychain.GetAllCertificates (CssmKeyUse.Encrypt)) {
				if (certificate.GetSubjectEmail () == mailbox.Address)
					return certificate;
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
		}

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

		protected override AsymmetricKeyParameter GetPrivateKey (RecipientID recipient)
		{
			throw new NotImplementedException ();
		}

		public override void ImportKeys (byte[] rawData)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");


		}

		public override void ImportPkcs12 (byte[] rawData, string password)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			if (password == null)
				throw new ArgumentNullException ("password");

			using (var memory = new MemoryStream (rawData, false)) {
				var pkcs12 = new Pkcs12Store (memory, password.ToCharArray ());
				foreach (string alias in pkcs12.Aliases) {
					if (pkcs12.IsKeyEntry (alias)) {
						var chain = pkcs12.GetCertificateChain (alias);
						var entry = pkcs12.GetKey (alias);

						for (int i = 0; i < chain.Length; i++)
							keychain.Add (chain[i].Certificate);


					} else if (pkcs12.IsCertificateEntry (alias)) {
						var entry = pkcs12.GetCertificate (alias);
						keychain.Add (entry.Certificate);
					}
				}
			}
		}

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
