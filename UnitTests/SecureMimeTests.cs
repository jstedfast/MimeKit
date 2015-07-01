//
// SecureMimeTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
	public abstract class SecureMimeTestsBase
	{
		static readonly string[] CertificateAuthorities = {
			"certificate-authority.crt", "StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		protected abstract SecureMimeContext CreateContext ();

		[TestFixtureSetUp]
		public void SetUp ()
		{
			using (var ctx = (DefaultSecureMimeContext) CreateContext ()) {
				var dataDir = Path.Combine ("..", "..", "TestData", "smime");
				string path;

				foreach (var filename in CertificateAuthorities) {
					path = Path.Combine (dataDir, filename);
					using (var file = File.OpenRead (path)) {
						ctx.Import (file, true);
					}
				}

				path = Path.Combine (dataDir, "smime.p12");

				using (var file = File.OpenRead (path)) {
					ctx.Import (file, "no.secret");
				}
			}
		}

		[Test]
		public void TestSecureMimeCompression ()
		{
			var original = new TextPart ("plain");
			original.Text = "This is some text that we'll end up compressing...";

			using (var ctx = CreateContext ()) {
				var compressed = ApplicationPkcs7Mime.Compress (ctx, original);

				Assert.AreEqual (SecureMimeType.CompressedData, compressed.SecureMimeType, "S/MIME type did not match.");

				var decompressed = compressed.Decompress (ctx);

				Assert.IsInstanceOf<TextPart> (decompressed, "Decompressed part is not the expected type.");
				Assert.AreEqual (original.Text, ((TextPart) decompressed).Text, "Decompressed content is not the same as the original.");
			}
		}

		[Test]
		public void TestSecureMimeEncapsulatedSigning ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var cleartext = new TextPart ("plain");
			cleartext.Text = "This is some text that we'll end up signing...";

			using (var ctx = CreateContext ()) {
				var signed = ApplicationPkcs7Mime.Sign (ctx, self, DigestAlgorithm.Sha1, cleartext);
				MimeEntity extracted;

				Assert.AreEqual (SecureMimeType.SignedData, signed.SecureMimeType, "S/MIME type did not match.");

				var signatures = signed.Verify (ctx, out extracted);

				Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}

					var algorithms = ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[0], "Expected Camellia-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[1], "Expected AES-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[2], "Expected Camellia-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[3], "Expected AES-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[4], "Expected Camellia-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[5], "Expected AES-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[6], "Expected IDEA capability");
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[7], "Expected Cast5 capability");
					Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[8], "Expected Triple-DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[9], "Expected RC2-128 capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[10], "Expected RC2-64 capability");
					//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[11], "Expected DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[12], "Expected RC2-40 capability");
				}
			}
		}

		[Test]
		public void TestSecureMimeSigning ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of signing with S/MIME" };

			message.From.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				message.Sign (ctx);

				Assert.IsInstanceOf<MultipartSigned> (message.Body, "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}

					var algorithms = ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[0], "Expected Camellia-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[1], "Expected AES-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[2], "Expected Camellia-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[3], "Expected AES-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[4], "Expected Camellia-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[5], "Expected AES-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[6], "Expected IDEA capability");
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[7], "Expected Cast5 capability");
					Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[8], "Expected Triple-DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[9], "Expected RC2-128 capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[10], "Expected RC2-64 capability");
					//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[11], "Expected DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[12], "Expected RC2-40 capability");
				}
			}
		}

		[Test]
		public void TestSecureMimeVerifyThunderbird ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine ("..", "..", "TestData", "smime", "thunderbird-signed.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			using (var ctx = CreateContext ()) {
				Assert.IsInstanceOf<MultipartSigned> (message.Body, "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.IsTrue (ctx.Supports (protocol), "The multipart/signed protocol is not supported.");

				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}

					var algorithms = ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
					Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[0], "Expected AES-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[1], "Expected AES-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[2], "Expected Triple-DES capability");
					Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[3], "Expected RC2-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[4], "Expected RC2-64 capability");
					Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[5], "Expected DES capability");
					Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[6], "Expected RC2-40 capability");
				}
			}
		}

		[Test]
		public void TestSecureMimeEncryption ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of encrypting with S/MIME" };

			message.From.Add (self);
			message.To.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				message.Encrypt (ctx);

				Assert.IsInstanceOf<ApplicationPkcs7Mime> (message.Body, "The message body should be an application/pkcs7-mime part.");

				var encrypted = (ApplicationPkcs7Mime) message.Body;

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

				var decrypted = encrypted.Decrypt (ctx);

				Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestSecureMimeDecryptThunderbird ()
		{
			var p12 = Path.Combine ("..", "..", "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine ("..", "..", "TestData", "smime", "thunderbird-encrypted.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			using (var ctx = CreateContext ()) {
				var encrypted = (ApplicationPkcs7Mime) message.Body;
				MimeEntity decrypted = null;

				using (var file = File.OpenRead (p12)) {
					ctx.Import (file, "no.secret");
				}

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.AreEqual ("enveloped-data", type, "Unexpected smime-type parameter.");

				try {
					decrypted = encrypted.Decrypt (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ("Failed to decrypt thunderbird message: {0}", ex);
				}

				// The decrypted part should be a multipart/mixed with a text/plain part and an image attachment,
				// very much like the thunderbird-signed.txt message.
				Assert.IsInstanceOf<Multipart> (decrypted, "Expected the decrypted part to be a Multipart.");
				var multipart = (Multipart) decrypted;

				Assert.IsInstanceOf<TextPart> (multipart[0], "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.IsInstanceOf<MimePart> (multipart[1], "Expected the second part of the decrypted multipart to be a MimePart.");
			}
		}

		[Test]
		public void TestSecureMimeSignAndEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "b7dd33847c3308dd9e12b4c3c94b545d76ab5e41");
			var message = new MimeMessage { Subject = "Test of signing and encrypting with S/MIME" };
			ApplicationPkcs7Mime encrypted;

			message.From.Add (self);
			message.To.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				message.SignAndEncrypt (ctx);

				Assert.IsInstanceOf<ApplicationPkcs7Mime> (message.Body, "The message body should be an application/pkcs7-mime part.");

				encrypted = (ApplicationPkcs7Mime) message.Body;

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");
			}

			using (var ctx = CreateContext ()) {
				var decrypted = encrypted.Decrypt (ctx);

				// The decrypted part should be a multipart/signed
				Assert.IsInstanceOf<MultipartSigned> (decrypted, "Expected the decrypted part to be a multipart/signed.");
				var signed = (MultipartSigned) decrypted;

				Assert.IsInstanceOf<TextPart> (signed[0], "Expected the first part of the multipart/signed to be a multipart.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (signed[1], "Expected second part of the multipart/signed to be a pkcs7-signature.");

				var extracted = (TextPart) signed[0];
				Assert.AreEqual (body.Text, extracted.Text, "The decrypted text part's text does not match the original.");

				var signatures = signed.Verify (ctx);

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}

					var algorithms = ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[0], "Expected Camellia-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[1], "Expected AES-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[2], "Expected Camellia-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[3], "Expected AES-192 capability");
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[4], "Expected Camellia-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[5], "Expected AES-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[6], "Expected IDEA capability");
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[7], "Expected Cast5 capability");
					Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[8], "Expected Triple-DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[9], "Expected RC2-128 capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[10], "Expected RC2-64 capability");
					//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[11], "Expected DES capability");
					//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[12], "Expected RC2-40 capability");
				}
			}
		}

		[Test]
		public void TestSecureMimeDecryptVerifyThunderbird ()
		{
			var p12 = Path.Combine ("..", "..", "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine ("..", "..", "TestData", "smime", "thunderbird-signed-encrypted.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			using (var ctx = CreateContext ()) {
				var encrypted = (ApplicationPkcs7Mime) message.Body;
				MimeEntity decrypted = null;

				using (var file = File.OpenRead (p12)) {
					ctx.Import (file, "no.secret");
				}

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.AreEqual ("enveloped-data", type, "Unexpected smime-type parameter.");

				try {
					decrypted = encrypted.Decrypt (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ("Failed to decrypt thunderbird message: {0}", ex);
				}

				// The decrypted part should be a multipart/signed
				Assert.IsInstanceOf<MultipartSigned> (decrypted, "Expected the decrypted part to be a multipart/signed.");
				var signed = (MultipartSigned) decrypted;

				// The first part of the multipart/signed should be a multipart/mixed with a text/plain part and 2 image attachments,
				// very much like the thunderbird-signed.txt message.
				Assert.IsInstanceOf<Multipart> (signed[0], "Expected the first part of the multipart/signed to be a multipart.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (signed[1], "Expected second part of the multipart/signed to be a pkcs7-signature.");

				var multipart = (Multipart) signed[0];

				Assert.IsInstanceOf<TextPart> (multipart[0], "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.IsInstanceOf<MimePart> (multipart[1], "Expected the second part of the decrypted multipart to be a MimePart.");
				Assert.IsInstanceOf<MimePart> (multipart[2], "Expected the third part of the decrypted multipart to be a MimePart.");

				var signatures = signed.Verify (ctx);

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}

					var algorithms = ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
					Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[0], "Expected AES-256 capability");
					Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[1], "Expected AES-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[2], "Expected Triple-DES capability");
					Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[3], "Expected RC2-128 capability");
					Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[4], "Expected RC2-64 capability");
					Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[5], "Expected DES capability");
					Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[6], "Expected RC2-40 capability");
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
				var certsonly = ctx.Export (mailboxes);

				Assert.IsInstanceOf<ApplicationPkcs7Mime> (certsonly, "The exported mime part is not of the expected type.");

				var pkcs7mime = (ApplicationPkcs7Mime) certsonly;

				Assert.AreEqual (SecureMimeType.CertsOnly, pkcs7mime.SecureMimeType, "S/MIME type did not match.");

				using (var imported = new DummySecureMimeContext ()) {
					pkcs7mime.Import (imported);

					Assert.AreEqual (1, imported.certificates.Count, "Unexpected number of imported certificates.");
					Assert.IsFalse (imported.keys.Count > 0, "One or more of the certificates included the private key.");
				}
			}
		}
	}

	[TestFixture]
	public class SecureMimeSqliteTests : SecureMimeTestsBase
	{
		protected override SecureMimeContext CreateContext ()
		{
			return new DefaultSecureMimeContext ("smime.db", "no.secret");
		}
	}

	#if false
	[TestFixture, Explicit]
	public class SecureMimeNpgsqlTests : SecureMimeTestsBase
	{
		protected override SecureMimeContext CreateContext ()
		{
			var user = Environment.GetEnvironmentVariable ("USER");
			var builder = new StringBuilder ();

			builder.Append ("server=localhost;");
			builder.Append ("database=smime.npg;");
			builder.AppendFormat ("user id={0}", user);

			var db = new NpgsqlCertificateDatabase (builder.ToString (), "no.secret");

			return new DefaultSecureMimeContext (db);
		}
	}
	#endif
}
