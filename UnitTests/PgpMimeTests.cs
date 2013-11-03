//
// PgpMimeTests.cs
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

using NUnit.Framework;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class PgpMimeTests
	{
		[SetUp]
		public void SetUp ()
		{
			Environment.SetEnvironmentVariable ("GNUPGHOME", Path.GetFullPath ("."));
			var dataDir = Path.Combine ("..", "..", "TestData", "openpgp");

			using (var ctx = new GnuPGContext ()) {
				using (var seckeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.sec")))
					ctx.ImportSecretKeys (seckeys);

				using (var pubkeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.pub")))
					ctx.ImportKeys (pubkeys);
			}
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				File.Delete ("pubring.gpg");
			} catch {
			}

			try {
				File.Delete ("secring.gpg");
			} catch {
			}
		}

		[Test]
		public void TestPgpMimeSigning ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up signing...";

			using (var ctx = new GnuPGContext ()) {
				var multipart = MultipartSigned.Create (ctx, self, DigestAlgorithm.Sha1, cleartext);
				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOfType (typeof (TextPart), multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOfType (typeof (ApplicationPkcs7Signature), multipart[1], "The second child is not a detached signature.");

				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public void TestPgpMimeEncryption ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var recipients = new List<MailboxAddress> ();

			// encrypt to ourselves...
			recipients.Add (self);

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up encrypting...";

			using (var ctx = new GnuPGContext ()) {
				var encrypted = MultipartEncrypted.Create (ctx, recipients, cleartext);

				using (var file = File.OpenWrite ("pgp-encrypted.txt"))
					encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt (ctx);

				Assert.IsInstanceOfType (typeof (TextPart), decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestPgpMimeSignAndEncrypt ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var recipients = new List<MailboxAddress> ();

			// encrypt to ourselves...
			recipients.Add (self);

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up encrypting...";

			using (var ctx = new GnuPGContext ()) {
				var encrypted = MultipartEncrypted.Create (ctx, self, DigestAlgorithm.Sha1, recipients, cleartext);

				IList<IDigitalSignature> signatures;
				var decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.IsInstanceOfType (typeof (TextPart), decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}
	}
}
