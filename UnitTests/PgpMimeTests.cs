//
// PgpMimeTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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

using Org.BouncyCastle.Bcpg.OpenPgp;

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

			CryptographyContext.Register (typeof (DummyOpenPgpContext));

			using (var ctx = new DummyOpenPgpContext ()) {
				using (var seckeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.sec")))
					ctx.ImportSecretKeys (seckeys);

				using (var pubkeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.pub")))
					ctx.Import (pubkeys);
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
		public void TestMimeMessageSign ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };

			message.From.Add (self);
			message.Body = body;

			using (var ctx = new DummyOpenPgpContext ()) {
				message.Sign (ctx);

				Assert.IsInstanceOf<MultipartSigned> (message.Body);

				var multipart = (MultipartSigned) message.Body;

				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOf<ApplicationPgpSignature> (multipart[1], "The second child is not a detached signature.");

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
		public void TestMultipartSignedSignUsingKeys ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "44CD48EEC90D8849961F36BA50DCD107AB0821A2");
			PgpSecretKey signer;

			using (var ctx = new DummyOpenPgpContext ()) {
				signer = ctx.GetSigningKey (self);

				foreach (DigestAlgorithm digest in Enum.GetValues (typeof (DigestAlgorithm))) {
					if (digest == DigestAlgorithm.None ||
						digest == DigestAlgorithm.DoubleSha ||
						digest == DigestAlgorithm.Tiger192 ||
						digest == DigestAlgorithm.Haval5160 ||
						digest == DigestAlgorithm.MD4)
						continue;

					var multipart = MultipartSigned.Create (signer, digest, body);

					Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.AreEqual ("application/pgp-signature", protocol, "The multipart/signed protocol does not match.");

					var micalg = multipart.ContentType.Parameters["micalg"];
					var algorithm = ctx.GetDigestAlgorithm (micalg);

					Assert.AreEqual (digest, algorithm, "The multipart/signed micalg does not match.");

					Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
					Assert.IsInstanceOf<ApplicationPgpSignature> (multipart[1], "The second child is not a detached signature.");

					var signatures = multipart.Verify ();
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

		[Test]
		public void TestMimeMessageEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "44CD48EEC90D8849961F36BA50DCD107AB0821A2");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };

			message.From.Add (self);
			message.To.Add (self);
			message.Body = body;

			using (var ctx = new DummyOpenPgpContext ()) {
				message.Encrypt (ctx);

				Assert.IsInstanceOf<MultipartEncrypted> (message.Body);

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt (ctx);

				Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestMultipartEncryptedEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var encrypted = MultipartEncrypted.Encrypt (new [] { self }, body);

			//using (var file = File.Create ("pgp-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt ();

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
		}

		[Test]
		public void TestMultipartEncryptedEncryptUsingKeys ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			IList<PgpPublicKey> recipients;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = ctx.GetPublicKeys (new [] { self });
			}

			var encrypted = MultipartEncrypted.Encrypt (recipients, body);

			//using (var file = File.Create ("pgp-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt ();

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
		}

		[Test]
		public void TestMultipartEncryptedEncryptAlgorithm ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = new DummyOpenPgpContext ()) {
				foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
					if (algorithm == EncryptionAlgorithm.RC240 ||
						algorithm == EncryptionAlgorithm.RC264 || 
						algorithm == EncryptionAlgorithm.RC2128)
						continue;
					
					var encrypted = MultipartEncrypted.Encrypt (algorithm, new [] { self }, body);

					//using (var file = File.Create ("pgp-encrypted.asc"))
					//	encrypted.WriteTo (file);

					var decrypted = encrypted.Decrypt (ctx);

					Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
					Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
				}
			}
		}

		[Test]
		public void TestMultipartEncryptedEncryptAlgorithmUsingKeys ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			IList<PgpPublicKey> recipients;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = ctx.GetPublicKeys (new [] { self });
			}

			foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
				if (algorithm == EncryptionAlgorithm.RC240 ||
					algorithm == EncryptionAlgorithm.RC264 || 
					algorithm == EncryptionAlgorithm.RC2128)
					continue;

				var encrypted = MultipartEncrypted.Encrypt (algorithm, recipients, body);

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt ();

				Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestMimeMessageSignAndEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };
			DigitalSignatureCollection signatures;

			message.From.Add (self);
			message.To.Add (self);
			message.Body = body;

			using (var ctx = new DummyOpenPgpContext ()) {
				message.SignAndEncrypt (ctx);

				Assert.IsInstanceOf<MultipartEncrypted> (message.Body);

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
				Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

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
		public void TestMultipartEncryptedSignAndEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;

			var encrypted = MultipartEncrypted.SignAndEncrypt (self, DigestAlgorithm.Sha1, new [] { self }, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt (out signatures);

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

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

		[Test]
		public void TestMultipartEncryptedSignAndEncryptUsingKeys ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;
			IList<PgpPublicKey> recipients;
			PgpSecretKey signer;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = ctx.GetPublicKeys (new [] { self });
				signer = ctx.GetSigningKey (self);
			}

			var encrypted = MultipartEncrypted.SignAndEncrypt (signer, DigestAlgorithm.Sha1, recipients, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt (out signatures);

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

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

		[Test]
		public void TestMultipartEncryptedSignAndEncryptAlgorithm ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;

			var encrypted = MultipartEncrypted.SignAndEncrypt (self, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, new [] { self }, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt (out signatures);

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

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

		[Test]
		public void TestMultipartEncryptedSignAndEncryptALgorithmUsingKeys ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;
			IList<PgpPublicKey> recipients;
			PgpSecretKey signer;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = ctx.GetPublicKeys (new [] { self });
				signer = ctx.GetSigningKey (self);
			}

			var encrypted = MultipartEncrypted.SignAndEncrypt (signer, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, recipients, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			var decrypted = encrypted.Decrypt (out signatures);

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");

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

		[Test]
		public void TestExport ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = new DummyOpenPgpContext ()) {
				Assert.AreEqual ("application/pgp-keys", ctx.KeyExchangeProtocol, "The key-exchange protocol does not match.");

				var exported = ctx.Export (new [] { self });

				Assert.IsNotNull (exported, "The exported MIME part should not be null.");
				Assert.IsInstanceOf<MimePart> (exported, "The exported MIME part should be a MimePart.");
				Assert.AreEqual ("application/pgp-keys", exported.ContentType.MimeType);
			}
		}

		[Test]
		public void TestDefaultEncryptionAlgorithm ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
					if (algorithm == EncryptionAlgorithm.RC240 ||
						algorithm == EncryptionAlgorithm.RC264 || 
						algorithm == EncryptionAlgorithm.RC2128) {
						Assert.Throws<NotSupportedException> (() => ctx.DefaultEncryptionAlgorithm = algorithm);
						continue;
					}

					ctx.DefaultEncryptionAlgorithm = algorithm;

					Assert.AreEqual (algorithm, ctx.DefaultEncryptionAlgorithm, "Default encryption algorithm does not match.");
				}
			}
		}
	}
}
