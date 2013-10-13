//
// SecureMimeTests.cs
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class SecureMimeTests
	{
		MailboxAddress signerMailbox;
		X509Store store;

		[TestFixtureSetUp]
		public void Setup ()
		{
			var dataDir = Path.Combine ("..", "..", "TestData", "smime");
			X509Certificate2Collection certs;
			string path;

			store = new X509Store ("MimeKitUnitTests", StoreLocation.CurrentUser);
			store.Open (OpenFlags.ReadWrite);

			path = Path.Combine (dataDir, "certificate-authority.cert");
			store.Add (new X509Certificate2 (path));

			signerMailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			path = Path.Combine (dataDir, "smime.p12");
			certs = new X509Certificate2Collection ();
			certs.Import (path, "no.secret", X509KeyStorageFlags.UserKeySet);
			store.AddRange (certs);
		}

		// Note: if I uncomment this, it seems to get run after running TestSecureMimeEncryption()
		// (which passes) but before TestSecureMimeSigning() is run, thus causing it to fail.
//		[TestFixtureTearDown]
//		public void TearDown ()
//		{
//			store.Close ();
//		}

		[Test]
		public void TestSecureMimeSigning ()
		{
			var ctx = new SecureMimeContext (store);

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up signing...";

			var multipart = MultipartSigned.Create (ctx, signerMailbox, cleartext);
			Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

			var protocol = multipart.ContentType.Parameters["protocol"];
			Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

			Assert.IsInstanceOfType (typeof (TextPart), multipart[0], "The first child is not a text part.");
			Assert.IsInstanceOfType (typeof (ApplicationPkcs7Signature), multipart[1], "The second child is not a detached signature.");

			var signers = multipart.Verify ();
			Assert.AreEqual (1, signers.Count, "The signer info collection contains an unexpected number of signers.");
			foreach (var signer in signers) {
				try {
					// don't validate the signer against a CA since we're using a self-signed certificate
					signer.CheckSignature (true);
				} catch (Exception) {
					Assert.Fail ("Checking the signature of {0} failed.", signer);
				}
			}
		}

		[Test]
		public void TestSecureMimeEncryption ()
		{
			var recipients = new List<MailboxAddress> ();
			var ctx = new SecureMimeContext (store);

			// encrypt to ourselves...
			recipients.Add (new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com"));

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up encrypting...";

			var encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, cleartext);
			var decrypted = encrypted.Decrypt (ctx);

			Assert.IsInstanceOfType (typeof (TextPart), decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (cleartext.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
		}
	}
}
