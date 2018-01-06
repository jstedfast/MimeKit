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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
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
				using (var seckeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.sec"))) {
					using (var armored = new ArmoredInputStream (seckeys))
						ctx.Import (new PgpSecretKeyRingBundle (armored));
				}

				using (var pubkeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.pub")))
					ctx.Import (pubkeys);
			}

			File.Copy (Path.Combine (dataDir, "gpg.conf"), "gpg.conf", true);
		}

		static bool IsSupported (EncryptionAlgorithm algorithm)
		{
			switch (algorithm) {
			case EncryptionAlgorithm.RC2128:
			case EncryptionAlgorithm.RC264:
			case EncryptionAlgorithm.RC240:
			case EncryptionAlgorithm.Seed:
				return false;
			default:
				return true;
			}
		}

		[Test]
		public void TestPreferredAlgorithms ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				var encryptionAlgorithms = ctx.EnabledEncryptionAlgorithms;

				Assert.AreEqual (4, encryptionAlgorithms.Length);
				Assert.AreEqual (EncryptionAlgorithm.Aes256, encryptionAlgorithms[0]);
				Assert.AreEqual (EncryptionAlgorithm.Aes192, encryptionAlgorithms[1]);
				Assert.AreEqual (EncryptionAlgorithm.Aes128, encryptionAlgorithms[2]);
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, encryptionAlgorithms[3]);

				var digestAlgorithms = ctx.EnabledDigestAlgorithms;

				Assert.AreEqual (3, digestAlgorithms.Length);
				Assert.AreEqual (DigestAlgorithm.Sha256, digestAlgorithms[0]);
				Assert.AreEqual (DigestAlgorithm.Sha512, digestAlgorithms[1]);
				Assert.AreEqual (DigestAlgorithm.Sha1, digestAlgorithms[2]);
			}
		}

		[Test]
		public void TestKeyEnumeration ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				var unknownMailbox = new MailboxAddress ("Snarky McSnarkypants", "snarky@snarkypants.net");
				var knownMailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

				int count = ctx.EnumeratePublicKeys ().Count ();

				// Note: the count will be 8 if run as a complete unit test or 2 if run individually
				Assert.IsTrue (count == 8 || count == 2, "Unexpected number of public keys");
				Assert.AreEqual (0, ctx.EnumeratePublicKeys (unknownMailbox).Count (), "Unexpected number of public keys for an unknown mailbox");
				Assert.AreEqual (2, ctx.EnumeratePublicKeys (knownMailbox).Count (), "Unexpected number of public keys for a known mailbox");

				Assert.AreEqual (2, ctx.EnumerateSecretKeys ().Count (), "Unexpected number of secret keys");
				Assert.AreEqual (0, ctx.EnumerateSecretKeys (unknownMailbox).Count (), "Unexpected number of secret keys for an unknown mailbox");
				Assert.AreEqual (2, ctx.EnumerateSecretKeys (knownMailbox).Count (), "Unexpected number of secret keys for a known mailbox");

				Assert.IsTrue (ctx.CanSign (knownMailbox));
				Assert.IsFalse (ctx.CanSign (unknownMailbox));

				Assert.IsTrue (ctx.CanEncrypt (knownMailbox));
				Assert.IsFalse (ctx.CanEncrypt (unknownMailbox));
			}
		}

		[Test]
		public void TestKeyGeneration ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				var mailbox = new MailboxAddress ("Snarky McSnarkypants", "snarky@snarkypants.net");
				int publicKeyRings = ctx.EnumeratePublicKeyRings ().Count ();
				int secretKeyRings = ctx.EnumerateSecretKeyRings ().Count ();

				ctx.GenerateKeyPair (mailbox, "password", DateTime.Now.AddYears (1), EncryptionAlgorithm.Cast5);

				var pubring = ctx.EnumeratePublicKeyRings (mailbox).FirstOrDefault ();
				Assert.IsNotNull (pubring, "Expected to find the generated public keyring");

				ctx.Delete (pubring);
				Assert.AreEqual (publicKeyRings, ctx.EnumeratePublicKeyRings ().Count (), "Unexpected number of public keyrings");

				var secring = ctx.EnumerateSecretKeyRings (mailbox).FirstOrDefault ();
				Assert.IsNotNull (secring, "Expected to find the generated secret keyring");

				ctx.Delete (secring);
				Assert.AreEqual (secretKeyRings, ctx.EnumerateSecretKeyRings ().Count (), "Unexpected number of secret keyrings");
			}
		}

		[Test]
		public void TestKeySigning ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				var seckey = ctx.EnumerateSecretKeys (new MailboxAddress ("", "mimekit@example.com")).FirstOrDefault ();
				var mailbox = new MailboxAddress ("Snarky McSnarkypants", "snarky@snarkypants.net");

				ctx.GenerateKeyPair (mailbox, "password", DateTime.Now.AddYears (1), EncryptionAlgorithm.Cast5);

				// delete the secret keyring, we don't need it
				var secring = ctx.EnumerateSecretKeyRings (mailbox).FirstOrDefault ();
				ctx.Delete (secring);

				var pubring = ctx.EnumeratePublicKeyRings (mailbox).FirstOrDefault ();
				var pubkey = pubring.GetPublicKey ();
				int sigCount = 0;

				foreach (PgpSignature sig in pubkey.GetKeySignatures ())
					sigCount++;

				Assert.AreEqual (0, sigCount);

				ctx.SignKey (seckey, pubkey, DigestAlgorithm.Sha256, OpenPgpKeyCertification.CasualCertification);

				pubring = ctx.EnumeratePublicKeyRings (mailbox).FirstOrDefault ();
				pubkey = pubring.GetPublicKey ();

				sigCount = 0;

				foreach (PgpSignature sig in pubkey.GetKeySignatures ()) {
					Assert.AreEqual (seckey.KeyId, sig.KeyId);
					Assert.AreEqual (HashAlgorithmTag.Sha256, sig.HashAlgorithm);
					Assert.AreEqual ((int) OpenPgpKeyCertification.CasualCertification, sig.SignatureType);
					sigCount++;
				}

				Assert.AreEqual (1, sigCount);

				ctx.Delete (pubring);
			}
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
		public async void TestMimeMessageSignAsync ()
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

				var signatures = await multipart.VerifyAsync (ctx);
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
		public async void TestMultipartSignedSignUsingKeysAsync ()
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

					var signatures = await multipart.VerifyAsync ();
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

				// now do the same thing, but encrypt to the Resent-To headers
				message.From.Clear ();
				message.To.Clear ();

				message.From.Add (new MailboxAddress ("Dummy Sender", "dummy@sender.com"));
				message.To.Add (new MailboxAddress ("Dummy Recipient", "dummy@recipient.com"));

				message.ResentFrom.Add (self);
				message.ResentTo.Add (self);
				message.Body = body;

				message.Encrypt (ctx);

				Assert.IsInstanceOf<MultipartEncrypted> (message.Body);

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				decrypted = encrypted.Decrypt (ctx);

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
					if (!IsSupported (algorithm))
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
				if (!IsSupported (algorithm))
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

				// now do the same thing, but encrypt to the Resent-To headers
				message.From.Clear ();
				message.To.Clear ();

				message.From.Add (new MailboxAddress ("Dummy Sender", "dummy@sender.com"));
				message.To.Add (new MailboxAddress ("Dummy Recipient", "dummy@recipient.com"));

				message.ResentFrom.Add (self);
				message.ResentTo.Add (self);
				message.Body = body;

				message.SignAndEncrypt (ctx);

				Assert.IsInstanceOf<MultipartEncrypted> (message.Body);

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				decrypted = encrypted.Decrypt (ctx, out signatures);

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

				var keys = ctx.EnumeratePublicKeys (self).ToList ();
				var exported = ctx.Export (new [] { self });

				Assert.IsNotNull (exported, "The exported MIME part should not be null.");
				Assert.IsInstanceOf<MimePart> (exported, "The exported MIME part should be a MimePart.");
				Assert.AreEqual ("application/pgp-keys", exported.ContentType.MimeType);

				exported = ctx.Export (keys);

				Assert.IsNotNull (exported, "The exported MIME part should not be null.");
				Assert.IsInstanceOf<MimePart> (exported, "The exported MIME part should be a MimePart.");
				Assert.AreEqual ("application/pgp-keys", exported.ContentType.MimeType);

				using (var stream = new MemoryStream ()) {
					ctx.Export (new[] { self }, stream, true);

					Assert.AreEqual (exported.Content.Stream.Length, stream.Length);
				}

				foreach (var keyring in ctx.EnumeratePublicKeyRings (self)) {
					using (var stream = new MemoryStream ()) {
						ctx.Export (new[] { self }, stream, true);

						Assert.AreEqual (exported.Content.Stream.Length, stream.Length);
					}
				}

				using (var stream = new MemoryStream ()) {
					ctx.Export (keys, stream, true);

					Assert.AreEqual (exported.Content.Stream.Length, stream.Length);
				}
			}
		}

		[Test]
		public void TestDefaultEncryptionAlgorithm ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
					if (!IsSupported (algorithm)) {
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
				DigitalSignatureCollection signatures;
				var stream = new MemoryStream ();
				var entity = new MimePart ();

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

				Assert.Throws<ArgumentNullException> (() => ctx.CanSign (null));
				Assert.Throws<ArgumentNullException> (() => ctx.CanEncrypt (null));

				// Delete
				Assert.Throws<ArgumentNullException> (() => ctx.Delete ((PgpPublicKeyRing) null), "Delete");
				Assert.Throws<ArgumentNullException> (() => ctx.Delete ((PgpSecretKeyRing) null), "Delete");

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
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((PgpPublicKeyRingBundle) null, stream, true), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((MailboxAddress[]) null, stream, true), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export ((PgpPublicKey[]) null, stream, true), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export (ctx.PublicKeyRingBundle, null, true), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export (mailboxes, null, true), "Export");
				Assert.Throws<ArgumentNullException> (() => ctx.Export (pubkeys, null, true), "Export");

				// EnumeratePublicKey[Ring]s
				Assert.Throws<ArgumentNullException> (() => ctx.EnumeratePublicKeyRings (null).FirstOrDefault ());
				Assert.Throws<ArgumentNullException> (() => ctx.EnumeratePublicKeys (null).FirstOrDefault ());
				Assert.Throws<ArgumentNullException> (() => ctx.EnumerateSecretKeyRings (null).FirstOrDefault ());
				Assert.Throws<ArgumentNullException> (() => ctx.EnumerateSecretKeys (null).FirstOrDefault ());

				// GenerateKeyPair
				Assert.Throws<ArgumentNullException> (() => ctx.GenerateKeyPair (null, "password"));
				Assert.Throws<ArgumentNullException> (() => ctx.GenerateKeyPair (mailboxes[0], null));
				Assert.Throws<ArgumentException> (() => ctx.GenerateKeyPair (mailboxes[0], "password", DateTime.Now));

				// DecryptTo
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (null, stream), "DecryptTo");
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (stream, null), "DecryptTo");

				// DecryptToAsync
				Assert.Throws<ArgumentNullException> (async () => await ctx.DecryptToAsync (null, stream), "DecryptToAsync");
				Assert.Throws<ArgumentNullException> (async () => await ctx.DecryptToAsync (stream, null), "DecryptToAsync");

				// GetDigestAlgorithmName
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.None), "GetDigestAlgorithmName");

				// Import
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRingBundle) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRingBundle) null), "Import");

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

				// SignKey
				Assert.Throws<ArgumentNullException> (() => ctx.SignKey (null, pubkeys[0]), "SignKey");
				Assert.Throws<ArgumentNullException> (() => ctx.SignKey (key, null), "SignKey");

				// Supports
				Assert.Throws<ArgumentNullException> (() => ctx.Supports (null), "Supports");

				// Verify
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, stream), "Verify");
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (stream, null), "Verify");

				// Verify
				Assert.Throws<ArgumentNullException> (async () => await ctx.VerifyAsync (null, stream), "VerifyAsync");
				Assert.Throws<ArgumentNullException> (async () => await ctx.VerifyAsync (stream, null), "VerifyAsync");

				// MultipartEncrypted

				// Encrypt
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt ((MailboxAddress[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt ((PgpPublicKey[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (null, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, (MailboxAddress[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (ctx, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (null, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, (PgpPublicKey[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (ctx, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (EncryptionAlgorithm.Cast5, pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (null, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (null, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.Encrypt (ctx, EncryptionAlgorithm.Cast5, pubkeys, null));

				// SignAndEncrypt
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt ((MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt ((PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (null, mailboxes[0], DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (null, key, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt ((MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt ((PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (null, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (null, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.Throws<ArgumentException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartEncrypted.SignAndEncrypt (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null));

				var encrypted = new MultipartEncrypted ();

				Assert.Throws<ArgumentNullException> (() => encrypted.Accept (null));

				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null));
				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null, out signatures));

				// MultipartSigned
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (null, mailboxes[0], DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, mailboxes[0], DigestAlgorithm.Sha1, null));

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (null, key, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, key, DigestAlgorithm.Sha1, null));

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (key, DigestAlgorithm.Sha1, null));

				var signed = MultipartSigned.Create (key, DigestAlgorithm.Sha1, entity);

				Assert.Throws<ArgumentNullException> (() => signed.Accept (null));

				Assert.Throws<ArgumentNullException> (() => signed.Verify (null));
				Assert.Throws<ArgumentNullException> (async () => await signed.VerifyAsync (null));
			}
		}

		static void PumpDataThroughFilter (IMimeFilter filter, string fileName, bool isText)
		{
			using (var stream = File.OpenRead (fileName)) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = new byte[1];
					int outputLength;
					int outputIndex;
					int n;

					if (isText)
						filtered.Add (new Unix2DosFilter ());

					while ((n = filtered.Read (buffer, 0, buffer.Length)) > 0)
						filter.Filter (buffer, 0, n, out outputIndex, out outputLength);

					filter.Flush (buffer, 0, 0, out outputIndex, out outputLength);
				}
			}
		}

		[Test]
		public void TestOpenPgpDetectionFilter ()
		{
			var filter = new OpenPgpDetectionFilter ();

			PumpDataThroughFilter (filter, Path.Combine ("..", "..", "TestData", "openpgp", "mimekit.gpg.pub"), true);
			Assert.AreEqual (OpenPgpDataType.PublicKey, filter.DataType);
			Assert.AreEqual (0, filter.BeginOffset);
			Assert.AreEqual (1754, filter.EndOffset);

			filter.Reset ();

			PumpDataThroughFilter (filter, Path.Combine ("..", "..", "TestData", "openpgp", "mimekit.gpg.sec"), true);
			Assert.AreEqual (OpenPgpDataType.PrivateKey, filter.DataType);
			Assert.AreEqual (0, filter.BeginOffset);
			Assert.AreEqual (3650, filter.EndOffset);
		}
	}
}
