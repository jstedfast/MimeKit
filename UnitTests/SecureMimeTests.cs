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
using System.Collections.Generic;

using NUnit.Framework;

using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;

namespace UnitTests {
	[TestFixture]
	public class SecureMimeTests
	{
		static readonly string[] CertificateAuthorities = new string[] {
			"certificate-authority.crt", "StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		static SecureMimeContext CreateContext ()
		{
			var dataDir = Path.Combine ("..", "..", "TestData", "smime");
			var parser = new X509CertificateParser ();
			var ctx = new DummySecureMimeContext ();
			string path;

			foreach (var filename in CertificateAuthorities) {
				path = Path.Combine (dataDir, filename);
				var certificate = parser.ReadCertificate (File.ReadAllBytes (path));
				ctx.certificates.Add (certificate);
			}

			path = Path.Combine (dataDir, "smime.p12");
			ctx.ImportPkcs12 (File.ReadAllBytes (path), "no.secret");

			return ctx;
		}

		[Test]
		public void TestSecureMimeSigning ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up signing...";

			using (var ctx = CreateContext ()) {
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

		//[Test]
		public void TestRawThunderbirdData ()
		{
			var cleartext = File.ReadAllBytes (Path.Combine ("..", "..", "TestData", "smime", "parsed-content.txt"));
			var signatureData = File.ReadAllBytes (Path.Combine ("..", "..", "TestData", "smime", "signature-data.p7s"));

			using (var ctx = CreateContext ()) {
				var signatures = ctx.Verify (cleartext, signatureData);
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

		//[Test]
		public void TestSecureMimeVerifyThunderbird ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine ("..", "..", "TestData", "smime", "thunderbird-signed.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			using (var ctx = CreateContext ()) {
				var multipart = (MultipartSigned) message.Body;

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.IsTrue (ctx.Supports (protocol), "The multipart/signed protocol is not supported.");

				Assert.IsInstanceOfType (typeof (ApplicationPkcs7Signature), multipart[1], "The second child is not a detached signature.");

				using (var file = File.OpenWrite (Path.Combine ("..", "..", "TestData", "smime", "parsed-content.txt"))) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Dos;

					multipart[0].WriteTo (options, file);
					file.Write (new byte[] { 0x0A, 0x10 }, 0, 2);
					file.Flush ();
				}

				using (var file = File.OpenWrite (Path.Combine ("..", "..", "TestData", "smime", "signature-data.p7s"))) {
					((MimePart) multipart[1]).ContentObject.DecodeTo (file);
				}

				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an eunexpected number of signatures.");
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
		public void TestSecureMimeEncryption ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var recipients = new List<MailboxAddress> ();

			// encrypt to ourselves...
			recipients.Add (self);

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up encrypting...";

			using (var ctx = CreateContext ()) {
				var encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, cleartext);

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

				var decrypted = encrypted.Decrypt (ctx);

				Assert.IsInstanceOfType (typeof (TextPart), decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestSecureMimeSignAndEncrypt ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var recipients = new List<MailboxAddress> ();

			// encrypt to ourselves...
			recipients.Add (self);

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some cleartext that we'll end up encrypting...";

			ApplicationPkcs7Mime encrypted;

			using (var ctx = CreateContext ()) {
				encrypted = ApplicationPkcs7Mime.SignAndEncrypt (ctx, self, DigestAlgorithm.Sha1, recipients, cleartext);

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");
			}

			using (var ctx = CreateContext ()) {
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

		[Test]
		public void TestSecureMimeImportExport ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var mailboxes = new List<MailboxAddress> ();

			// we're going to export our public certificate so that we can email it to someone
			// so that they can then encrypt their emails to us.
			mailboxes.Add (self);

			using (var ctx = CreateContext ()) {
				var certsonly = ctx.ExportKeys (mailboxes);

				Assert.IsInstanceOfType (typeof (ApplicationPkcs7Mime), certsonly, "The exported mime part is not of the expected type.");

				var pkcs7mime = (ApplicationPkcs7Mime) certsonly;

				Assert.AreEqual (SecureMimeType.CertsOnly, pkcs7mime.SecureMimeType, "S/MIME type did not match.");

				using (var imported = new DummySecureMimeContext ()) {
					try {
						pkcs7mime.Import (imported);
					} catch (Exception ex) {
						Assert.Fail ("Failed to import certificates: {0}", ex);
					}

					Assert.AreEqual (1, imported.certificates.Count, "Unexpected number of imported certificates.");
					Assert.IsFalse (imported.keys.Count > 0, "One or more of the certificates included the private key.");
				}
			}
		}
	}
}
