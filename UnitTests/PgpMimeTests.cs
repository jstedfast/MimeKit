//
// PgpMimeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
		static PgpMimeTests ()
		{
			Environment.SetEnvironmentVariable ("GNUPGHOME", Path.GetFullPath ("."));
			var dataDir = Path.Combine ("..", "..", "TestData", "openpgp");

			CryptographyContext.Register (typeof (DummyOpenPgpContext));

			foreach (var name in new [] { "pubring.gpg", "pubring.gpg~", "secring.gpg", "secring.gpg~", "gpg.conf" }) {
				if (File.Exists (name))
					File.Delete (name);
			}

			using (var ctx = new DummyOpenPgpContext ()) {
				using (var seckeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.sec")))
					ctx.ImportSecretKeys (seckeys);

				using (var pubkeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.pub")))
					ctx.Import (pubkeys);
			}

			File.Copy (Path.Combine (dataDir, "gpg.conf"), "gpg.conf", true);
		}

		[Test]
		public void TestMimeMessageSign ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };

			using (var ctx = new DummyOpenPgpContext ()) {
				// throws because no Body is set
				Assert.Throws<InvalidOperationException> (() => message.Sign (ctx));

				message.Body = body;

				// throws because no sender is set
				Assert.Throws<InvalidOperationException> (() => message.Sign (ctx));

				message.From.Add (self);

				// ok, now we can sign
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

			using (var ctx = new DummyOpenPgpContext ()) {
				// throws because no Body has been set
				Assert.Throws<InvalidOperationException> (() => message.Encrypt (ctx));

				message.Body = body;

				// throws because no recipients have been set
				Assert.Throws<InvalidOperationException> (() => message.Encrypt (ctx));

				message.From.Add (self);
				message.To.Add (self);

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

			using (var ctx = new DummyOpenPgpContext ()) {
				// throws because no Body has been set
				Assert.Throws<InvalidOperationException> (() => message.SignAndEncrypt (ctx));

				message.Body = body;

				// throws because no sender has been set
				Assert.Throws<InvalidOperationException> (() => message.SignAndEncrypt (ctx));

				message.From.Add (self);
				message.To.Add (self);

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
		public void TestAutoKeyRetrieve ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "openpgp", "[Announce] GnuPG 2.1.20 released.eml"));
			var multipart = (MultipartSigned) ((Multipart) message.Body)[0];

			Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

			var protocol = multipart.ContentType.Parameters["protocol"];
			Assert.AreEqual ("application/pgp-signature", protocol, "The multipart/signed protocol does not match.");

			var micalg = multipart.ContentType.Parameters["micalg"];

			Assert.AreEqual ("pgp-sha1", micalg, "The multipart/signed micalg does not match.");

			Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
			Assert.IsInstanceOf<ApplicationPgpSignature> (multipart[1], "The second child is not a detached signature.");

			var signatures = multipart.Verify ();
			Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException) {
					// Note: Werner Koch's keyring has an EdDSA subkey which breaks BouncyCastle's
					// PgpPublicKeyRingBundle reader. If/when one of the round-robin keys.gnupg.net
					// key servers returns this particular keyring, we can expect to get an exception
					// about being unable to find Werner's public key.

					//Assert.Fail ("Failed to verify signature: {0}", ex);
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

		[Test]
		public void TestSupports ()
		{
			var supports = new [] { "application/pgp-encrypted", "application/pgp-signature", "application/pgp-keys",
				"application/x-pgp-encrypted", "application/x-pgp-signature", "application/x-pgp-keys" };

			using (var ctx = new DummyOpenPgpContext ()) {
				for (int i = 0; i < supports.Length; i++)
					Assert.IsTrue (ctx.Supports (supports[i]), supports[i]);

				Assert.IsFalse (ctx.Supports ("application/octet-stream"), "application/octet-stream");
				Assert.IsFalse (ctx.Supports ("text/plain"), "text/plain");
			}
		}

		[Test]
		public void TestAlgorithmMappings ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				foreach (DigestAlgorithm digest in Enum.GetValues (typeof (DigestAlgorithm))) {
					if (digest == DigestAlgorithm.None || digest == DigestAlgorithm.DoubleSha)
						continue;

					var name = ctx.GetDigestAlgorithmName (digest);
					var algo = ctx.GetDigestAlgorithm (name);

					Assert.AreEqual (digest, algo);
				}

				Assert.AreEqual (DigestAlgorithm.MD5, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.MD5));
				Assert.AreEqual (DigestAlgorithm.Sha1, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha1));
				Assert.AreEqual (DigestAlgorithm.RipeMD160, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.RipeMD160));
				Assert.AreEqual (DigestAlgorithm.DoubleSha, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.DoubleSha));
				Assert.AreEqual (DigestAlgorithm.MD2, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.MD2));
				Assert.AreEqual (DigestAlgorithm.Tiger192, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Tiger192));
				Assert.AreEqual (DigestAlgorithm.Haval5160, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Haval5pass160));
				Assert.AreEqual (DigestAlgorithm.Sha256, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha256));
				Assert.AreEqual (DigestAlgorithm.Sha384, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha384));
				Assert.AreEqual (DigestAlgorithm.Sha512, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha512));
				Assert.AreEqual (DigestAlgorithm.Sha224, OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha224));

				Assert.AreEqual (PublicKeyAlgorithm.RsaGeneral, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaGeneral));
				Assert.AreEqual (PublicKeyAlgorithm.RsaEncrypt, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaEncrypt));
				Assert.AreEqual (PublicKeyAlgorithm.RsaSign, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaSign));
				Assert.AreEqual (PublicKeyAlgorithm.ElGamalGeneral, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ElGamalGeneral));
				Assert.AreEqual (PublicKeyAlgorithm.ElGamalEncrypt, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ElGamalEncrypt));
				Assert.AreEqual (PublicKeyAlgorithm.Dsa, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.Dsa));
				Assert.AreEqual (PublicKeyAlgorithm.EllipticCurve, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDH));
				Assert.AreEqual (PublicKeyAlgorithm.EllipticCurveDsa, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDsa));
				Assert.AreEqual (PublicKeyAlgorithm.DiffieHellman, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.DiffieHellman));
				//Assert.AreEqual (PublicKeyAlgorithm.EdwardsCurveDsa, OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.EdDSA));
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => CryptographyContext.Create (null));
			Assert.Throws<ArgumentNullException> (() => CryptographyContext.Register ((Type) null));
			Assert.Throws<ArgumentNullException> (() => CryptographyContext.Register ((Func<OpenPgpContext>) null));
			Assert.Throws<ArgumentNullException> (() => CryptographyContext.Register ((Func<SecureMimeContext>) null));

			using (var ctx = new DummyOpenPgpContext ()) {
				var mailboxes = new [] { new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com") };
				var emptyMailboxes = new MailboxAddress[0];
				var pubkeys = ctx.GetPublicKeys (mailboxes);
				var key = ctx.GetSigningKey (mailboxes[0]);
				var emptyPubkeys = new PgpPublicKey[0];
				var stream = new MemoryStream ();

				Assert.Throws<ArgumentException> (() => ctx.KeyServer = new Uri ("relative/uri", UriKind.Relative));

				Assert.Throws<ArgumentNullException> (() => ctx.GetDigestAlgorithm (null));
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.DoubleSha));
				Assert.Throws<NotSupportedException> (() => OpenPgpContext.GetHashAlgorithm (DigestAlgorithm.DoubleSha));
				Assert.Throws<NotSupportedException> (() => OpenPgpContext.GetHashAlgorithm (DigestAlgorithm.Tiger192));
				Assert.Throws<NotSupportedException> (() => OpenPgpContext.GetHashAlgorithm (DigestAlgorithm.Haval5160));
				Assert.Throws<NotSupportedException> (() => OpenPgpContext.GetHashAlgorithm (DigestAlgorithm.MD4));
				Assert.Throws<ArgumentOutOfRangeException> (() => OpenPgpContext.GetDigestAlgorithm ((Org.BouncyCastle.Bcpg.HashAlgorithmTag) 1024));

				Assert.Throws<ArgumentNullException> (() => new ApplicationPgpEncrypted ((MimeEntityConstructorArgs) null));
				Assert.Throws<ArgumentNullException> (() => new ApplicationPgpSignature ((MimeEntityConstructorArgs) null));
				Assert.Throws<ArgumentNullException> (() => new ApplicationPgpSignature ((Stream) null));

				// Accept
				Assert.Throws<ArgumentNullException> (() => new ApplicationPgpEncrypted ().Accept (null));
				Assert.Throws<ArgumentNullException> (() => new ApplicationPgpSignature (stream).Accept (null));

				// Decrypt
				Assert.Throws<ArgumentNullException> (() => ctx.Decrypt (null), "Decrypt");

				// Encrypt
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, stream), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, stream), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((MailboxAddress[]) null, stream), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((PgpPublicKey[]) null, stream), "Encrypt");
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, emptyMailboxes, stream), "Encrypt");
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, emptyPubkeys, stream), "Encrypt");
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (emptyMailboxes, stream), "Encrypt");
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (emptyPubkeys, stream), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, mailboxes, null), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (EncryptionAlgorithm.Cast5, pubkeys, null), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (mailboxes, null), "Encrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (pubkeys, null), "Encrypt");

				// Export
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((PgpPublicKeyRingBundle) null), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((MailboxAddress[]) null), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((PgpPublicKey[]) null), "Export");

				// GetDecryptedStream
				Assert.Throws<ArgumentNullException> (() => ctx.GetDecryptedStream (null), "GetDecryptedStream");

				// GetDigestAlgorithmName
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.None), "GetDigestAlgorithmName");

				// Import
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRingBundle) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRingBundle) null), "Import");

				// ImportSecretKeys
				Assert.Throws<ArgumentNullException> (() => ctx.ImportSecretKeys (null), "ImportSecretKeys");

				// Sign
				Assert.Throws<ArgumentNullException> (() => ctx.Sign ((MailboxAddress) null, DigestAlgorithm.Sha1, stream), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign ((PgpSecretKey) null, DigestAlgorithm.Sha1, stream), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (mailboxes[0], DigestAlgorithm.Sha1, null), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (key, DigestAlgorithm.Sha1, null), "Sign");

				// SignAndEncrypt
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt ((MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt ((PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null), "SignAndEncrypt");

				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt ((MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt ((PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, (MailboxAddress[]) null, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, (PgpPublicKey[]) null, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, emptyPubkeys, stream), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null), "SignAndEncrypt");
				Assert.Throws<ArgumentNullException> (() => ctx.SignAndEncrypt (key, DigestAlgorithm.Sha1, pubkeys, null), "SignAndEncrypt");

				// Supports
				Assert.Throws<ArgumentNullException> (() => ctx.Supports (null), "Supports");

				// Verify
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, stream), "Verify");
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (stream, null), "Verify");
			}
		}
	}
}
