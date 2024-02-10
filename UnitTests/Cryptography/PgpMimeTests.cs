//
// PgpMimeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class PgpMimeTests : IDisposable
	{
		readonly string GnuPGDir;

		public PgpMimeTests ()
		{
			GnuPGDir = Path.Combine (TestHelper.ProjectDir, "Temp", ".gnupg");
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp");

			Directory.CreateDirectory (GnuPGDir);

			Environment.SetEnvironmentVariable ("GNUPGHOME", GnuPGDir);
			CryptographyContext.Register (typeof (DummyOpenPgpContext));

			foreach (var name in new [] { "pubring.gpg", "pubring.gpg~", "secring.gpg", "secring.gpg~", "gpg.conf" }) {
				var path = Path.Combine (GnuPGDir, name);

				if (File.Exists (path))
					File.Delete (path);
			}

			using (var ctx = new DummyOpenPgpContext ()) {
				using (var seckeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.sec"))) {
					using (var armored = new ArmoredInputStream (seckeys))
						ctx.Import (new PgpSecretKeyRingBundle (armored));
				}

				using (var pubkeys = File.OpenRead (Path.Combine (dataDir, "mimekit.gpg.pub")))
					ctx.Import (pubkeys);
			}

			File.Copy (Path.Combine (dataDir, "gpg.conf"), Path.Combine (GnuPGDir, "gpg.conf"), true);
		}

		public void Dispose ()
		{
			if (Directory.Exists (GnuPGDir)) {
				Directory.Delete (GnuPGDir, true);
			}

			GC.SuppressFinalize (this);
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

				Assert.That (encryptionAlgorithms.Length, Is.EqualTo (4));
				Assert.That (encryptionAlgorithms[0], Is.EqualTo (EncryptionAlgorithm.Aes256));
				Assert.That (encryptionAlgorithms[1], Is.EqualTo (EncryptionAlgorithm.Aes192));
				Assert.That (encryptionAlgorithms[2], Is.EqualTo (EncryptionAlgorithm.Aes128));
				Assert.That (encryptionAlgorithms[3], Is.EqualTo (EncryptionAlgorithm.TripleDes));

				var digestAlgorithms = ctx.EnabledDigestAlgorithms;

				Assert.That (digestAlgorithms.Length, Is.EqualTo (3));
				Assert.That (digestAlgorithms[0], Is.EqualTo (DigestAlgorithm.Sha256));
				Assert.That (digestAlgorithms[1], Is.EqualTo (DigestAlgorithm.Sha512));
				Assert.That (digestAlgorithms[2], Is.EqualTo (DigestAlgorithm.Sha1));
			}
		}

		[Test]
		public void TestKeyEnumeration ()
		{
			using (var ctx = new DummyOpenPgpContext ()) {
				var unknownMailbox = new MailboxAddress ("Snarky McSnarkypants", "snarky@snarkypants.net");
				var knownMailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
				var expectedKeyIds = new long[] { 5826761848774992290, -3218736509825932358 };
				var keyIds = new List<long> ();

				foreach (var key in ctx.EnumeratePublicKeys ())
					keyIds.Add (key.KeyId);

				foreach (var keyId in expectedKeyIds)
					Assert.That (keyIds.Contains (keyId), Is.True, $"Expected keyId: {keyId}");

				Assert.That (ctx.EnumeratePublicKeys (unknownMailbox).Count (), Is.EqualTo (0), "Unexpected number of public keys for an unknown mailbox");
				Assert.That (ctx.EnumeratePublicKeys (knownMailbox).Count (), Is.EqualTo (2), "Unexpected number of public keys for a known mailbox");

				Assert.That (ctx.EnumerateSecretKeys ().Count (), Is.EqualTo (2), "Unexpected number of secret keys");
				Assert.That (ctx.EnumerateSecretKeys (unknownMailbox).Count (), Is.EqualTo (0), "Unexpected number of secret keys for an unknown mailbox");
				Assert.That (ctx.EnumerateSecretKeys (knownMailbox).Count (), Is.EqualTo (2), "Unexpected number of secret keys for a known mailbox");

				Assert.That (ctx.CanSign (knownMailbox), Is.True);
				Assert.That (ctx.CanSign (unknownMailbox), Is.False);

				Assert.That (ctx.CanEncrypt (knownMailbox), Is.True);
				Assert.That (ctx.CanEncrypt (unknownMailbox), Is.False);
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
				Assert.That (pubring, Is.Not.Null, "Expected to find the generated public keyring");

				ctx.Delete (pubring);
				Assert.That (ctx.EnumeratePublicKeyRings ().Count (), Is.EqualTo (publicKeyRings), "Unexpected number of public keyrings");

				var secring = ctx.EnumerateSecretKeyRings (mailbox).FirstOrDefault ();
				Assert.That (secring, Is.Not.Null, "Expected to find the generated secret keyring");

				ctx.Delete (secring);
				Assert.That (ctx.EnumerateSecretKeyRings ().Count (), Is.EqualTo (secretKeyRings), "Unexpected number of secret keyrings");
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

				Assert.That (sigCount, Is.EqualTo (0));

				ctx.SignKey (seckey, pubkey, DigestAlgorithm.Sha256, OpenPgpKeyCertification.CasualCertification);

				pubring = ctx.EnumeratePublicKeyRings (mailbox).FirstOrDefault ();
				pubkey = pubring.GetPublicKey ();

				sigCount = 0;

				foreach (PgpSignature sig in pubkey.GetKeySignatures ()) {
					Assert.That (sig.KeyId, Is.EqualTo (seckey.KeyId));
					Assert.That (sig.HashAlgorithm, Is.EqualTo (HashAlgorithmTag.Sha256));
					Assert.That (sig.SignatureType, Is.EqualTo ((int) OpenPgpKeyCertification.CasualCertification));
					sigCount++;
				}

				Assert.That (sigCount, Is.EqualTo (1));

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

				Assert.That (message.Body, Is.InstanceOf<MultipartSigned> ());

				var multipart = (MultipartSigned) message.Body;

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPgpSignature>(), "The second child is not a detached signature.");

				var signatures = multipart.Verify (ctx);
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo ("mimekit@example.com"));
				Assert.That (signature.SignerCertificate.Fingerprint, Is.EqualTo ("44CD48EEC90D8849961F36BA50DCD107AB0821A2"));
				Assert.That (signature.SignerCertificate.CreationDate, Is.EqualTo (new DateTime (2013, 11, 3, 18, 32, 27)), "CreationDate");
				Assert.That (signature.SignerCertificate.ExpirationDate, Is.EqualTo (DateTime.MaxValue), "ExpirationDate");
				Assert.That (signature.SignerCertificate.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.RsaGeneral));

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public async Task TestMimeMessageSignAsync ()
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

				Assert.That (message.Body, Is.InstanceOf<MultipartSigned> ());

				var multipart = (MultipartSigned) message.Body;

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPgpSignature> (), "The second child is not a detached signature.");

				var signatures = await multipart.VerifyAsync (ctx);
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo ("mimekit@example.com"));
				Assert.That (signature.SignerCertificate.Fingerprint, Is.EqualTo ("44CD48EEC90D8849961F36BA50DCD107AB0821A2"));
				Assert.That (signature.SignerCertificate.CreationDate, Is.EqualTo (new DateTime (2013, 11, 3, 18, 32, 27)), "CreationDate");
				Assert.That (signature.SignerCertificate.ExpirationDate, Is.EqualTo (DateTime.MaxValue), "ExpirationDate");
				Assert.That (signature.SignerCertificate.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.RsaGeneral));

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public void TestMultipartSignedVerifyExceptions ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = new DummyOpenPgpContext ()) {
				var signed = MultipartSigned.Create (ctx, self, DigestAlgorithm.Sha256, body);

				var protocol = signed.ContentType.Parameters["protocol"];
				signed.ContentType.Parameters.Remove ("protocol");

				Assert.Throws<FormatException> (() => signed.Verify (), "Verify() w/o protocol parameter");
				Assert.Throws<FormatException> (() => signed.Verify (ctx), "Verify(ctx) w/o protocol parameter");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (), "VerifyAsync() w/o protocol parameter");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (ctx), "VerifyAsync(ctx) w/o protocol parameter");

				signed.ContentType.Parameters.Add ("protocol", "invalid-protocol");
				Assert.Throws<NotSupportedException> (() => signed.Verify (), "Verify() w/ invalid protocol parameter");
				Assert.Throws<NotSupportedException> (() => signed.Verify (ctx), "Verify(ctx) w/ invalid protocol parameter");
				Assert.ThrowsAsync<NotSupportedException> (() => signed.VerifyAsync (), "VerifyAsync() w/ invalid protocol parameter");
				Assert.ThrowsAsync<NotSupportedException> (() => signed.VerifyAsync (ctx), "VerifyAsync(ctx) w/ invalid protocol parameter");

				signed.ContentType.Parameters["protocol"] = protocol;
				var signature = signed[1];
				signed.RemoveAt (1);
				Assert.Throws<FormatException> (() => signed.Verify (), "Verify() w/ < 2 parts");
				Assert.Throws<FormatException> (() => signed.Verify (ctx), "Verify(ctx) w/ < 2 parts");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (), "VerifyAsync() w/ < 2 parts");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (ctx), "VerifyAsync(ctx) w/ < 2 parts");

				var emptySignature = new MimePart ("application", "octet-stream");
				signed.Add (emptySignature);
				Assert.Throws<FormatException> (() => signed.Verify (), "Verify() w/ invalid signature part");
				Assert.Throws<FormatException> (() => signed.Verify (ctx), "Verify(ctx) w/ invalid signature part");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (), "VerifyAsync() w/ invalid signature part");
				Assert.ThrowsAsync<FormatException> (() => signed.VerifyAsync (ctx), "VerifyAsync(ctx) w/ invalid signature part");

				var invalidContent = new MimePart ("image", "jpeg") {
					Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
				};
				signed[1] = invalidContent;
				Assert.Throws<NotSupportedException> (() => signed.Verify (), "Verify() w/ invalid content part");
				Assert.Throws<NotSupportedException> (() => signed.Verify (ctx), "Verify(ctx) w/ invalid content part");
				Assert.ThrowsAsync<NotSupportedException> (() => signed.VerifyAsync (), "VerifyAsync() w/ invalid content part");
				Assert.ThrowsAsync<NotSupportedException> (() => signed.VerifyAsync (ctx), "VerifyAsync(ctx) w/ invalid content part");
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

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo ("application/pgp-signature"), "The multipart/signed protocol does not match.");

					var micalg = multipart.ContentType.Parameters["micalg"];
					var algorithm = ctx.GetDigestAlgorithm (micalg);

					Assert.That (algorithm, Is.EqualTo (digest), "The multipart/signed micalg does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPgpSignature> (), "The second child is not a detached signature.");

					var signatures = multipart.Verify ();
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					foreach (var signature in signatures) {
						try {
							bool valid = signature.Verify ();

							Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
						} catch (DigitalSignatureVerifyException ex) {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public async Task TestMultipartSignedSignUsingKeysAsync ()
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

					var multipart = await MultipartSigned.CreateAsync (signer, digest, body);

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo ("application/pgp-signature"), "The multipart/signed protocol does not match.");

					var micalg = multipart.ContentType.Parameters["micalg"];
					var algorithm = ctx.GetDigestAlgorithm (micalg);

					Assert.That (algorithm, Is.EqualTo (digest), "The multipart/signed micalg does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPgpSignature> (), "The second child is not a detached signature.");

					var signatures = await multipart.VerifyAsync ();
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					foreach (var signature in signatures) {
						try {
							bool valid = signature.Verify ();

							Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
						} catch (DigitalSignatureVerifyException ex) {
							Assert.Fail ($"Failed to verify signature: {ex}");
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

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt (ctx);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				// now do the same thing, but encrypt to the Resent-To headers
				message.From.Clear ();
				message.To.Clear ();

				message.From.Add (new MailboxAddress ("Dummy Sender", "dummy@sender.com"));
				message.To.Add (new MailboxAddress ("Dummy Recipient", "dummy@recipient.com"));

				message.ResentFrom.Add (self);
				message.ResentTo.Add (self);
				message.Body = body;

				message.Encrypt (ctx);

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				decrypted = encrypted.Decrypt (ctx);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public async Task TestMimeMessageEncryptAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "44CD48EEC90D8849961F36BA50DCD107AB0821A2");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };

			using (var ctx = new DummyOpenPgpContext ()) {
				// throws because no Body has been set
				Assert.ThrowsAsync<InvalidOperationException> (() => message.EncryptAsync (ctx));

				message.Body = body;

				// throws because no recipients have been set
				Assert.ThrowsAsync<InvalidOperationException> (() => message.EncryptAsync (ctx));

				message.From.Add (self);
				message.To.Add (self);

				await message.EncryptAsync (ctx);

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				// TODO: implement DecryptAsync
				var decrypted = encrypted.Decrypt (ctx);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				// now do the same thing, but encrypt to the Resent-To headers
				message.From.Clear ();
				message.To.Clear ();

				message.From.Add (new MailboxAddress ("Dummy Sender", "dummy@sender.com"));
				message.To.Add (new MailboxAddress ("Dummy Recipient", "dummy@recipient.com"));

				message.ResentFrom.Add (self);
				message.ResentTo.Add (self);
				message.Body = body;

				await message.EncryptAsync (ctx);

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				// TODO: implement DecryptAsync
				decrypted = encrypted.Decrypt (ctx);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public void TestMultipartEncryptedDecryptExceptions ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var encrypted = MultipartEncrypted.Encrypt (new[] { self }, body);

			using (var ctx = new DummyOpenPgpContext ()) {
				var protocol = encrypted.ContentType.Parameters["protocol"];
				encrypted.ContentType.Parameters.Remove ("protocol");

				Assert.Throws<FormatException> (() => encrypted.Decrypt (), "Decrypt() w/o protocol parameter");
				Assert.Throws<FormatException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/o protocol parameter");

				encrypted.ContentType.Parameters.Add ("protocol", "invalid-protocol");
				//Assert.Throws<NotSupportedException> (() => encrypted.Decrypt (), "Decrypt() w/ invalid protocol parameter");
				Assert.Throws<NotSupportedException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/ invalid protocol parameter");

				encrypted.ContentType.Parameters["protocol"] = protocol;
				var version = encrypted[0];
				encrypted.RemoveAt (0);
				Assert.Throws<FormatException> (() => encrypted.Decrypt (), "Decrypt() w/ < 2 parts");
				Assert.Throws<FormatException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/ < 2 parts");

				var invalidVersion = new MimePart ("application", "octet-stream") {
					Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
				};
				encrypted.Insert (0, invalidVersion);
				Assert.Throws<FormatException> (() => encrypted.Decrypt (), "Decrypt() w/ invalid version part");
				Assert.Throws<FormatException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/ invalid version part");

				var emptyContent = new MimePart ("application", "octet-stream");
				var content = encrypted[1];
				encrypted[1] = emptyContent;
				encrypted[0] = version;
				Assert.Throws<FormatException> (() => encrypted.Decrypt (), "Decrypt() w/ empty content part");
				Assert.Throws<FormatException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/ empty content part");

				var invalidContent = new MimePart ("image", "jpeg") {
					Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
				};
				encrypted[1] = invalidContent;
				Assert.Throws<FormatException> (() => encrypted.Decrypt (), "Decrypt() w/ invalid content part");
				Assert.Throws<FormatException> (() => encrypted.Decrypt (ctx), "Decrypt(ctx) w/ invalid content part");
			}
		}

		[Test]
		public void TestMultipartEncryptedEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var encrypted = MultipartEncrypted.Encrypt (new [] { self }, body);

			using (var stream = new MemoryStream ()) {
				encrypted.WriteTo (stream);
				stream.Position = 0;

				var entity = MimeEntity.Load (stream);

				Assert.That (entity, Is.InstanceOf<MultipartEncrypted> (), "Encrypted part is not the expected type");

				encrypted = (MultipartEncrypted) entity;

				Assert.That (encrypted[0], Is.InstanceOf<ApplicationPgpEncrypted> (), "First child of multipart/encrypted is not the expected type");
				Assert.That (encrypted[1], Is.InstanceOf<MimePart> (), "Second child of multipart/encrypted is not the expected type");
				Assert.That (encrypted[1].ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Second child of multipart/encrypted is not the expected mime-type");
			}

			var decrypted = encrypted.Decrypt ();

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
		}

		[Test]
		public async Task TestMultipartEncryptedEncryptAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var encrypted = await MultipartEncrypted.EncryptAsync (new[] { self }, body);

			using (var stream = new MemoryStream ()) {
				await encrypted.WriteToAsync (stream);
				stream.Position = 0;

				var entity = await MimeEntity.LoadAsync (stream);

				Assert.That (entity, Is.InstanceOf<MultipartEncrypted> (), "Encrypted part is not the expected type");

				encrypted = (MultipartEncrypted) entity;

				Assert.That (encrypted[0], Is.InstanceOf<ApplicationPgpEncrypted> (), "First child of multipart/encrypted is not the expected type");
				Assert.That (encrypted[1], Is.InstanceOf<MimePart> (), "Second child of multipart/encrypted is not the expected type");
				Assert.That (encrypted[1].ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Second child of multipart/encrypted is not the expected mime-type");
			}

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt ();

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
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

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
		}

		[Test]
		public async Task TestMultipartEncryptedEncryptUsingKeysAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			IList<PgpPublicKey> recipients;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = await ctx.GetPublicKeysAsync (new[] { self });
			}

			var encrypted = await MultipartEncrypted.EncryptAsync (recipients, body);

			//using (var file = File.Create ("pgp-encrypted.asc"))
			//	encrypted.WriteTo (file);

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt ();

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
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

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
				}
			}
		}

		[Test]
		public async Task TestMultipartEncryptedEncryptAlgorithmAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = new DummyOpenPgpContext ()) {
				foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
					if (!IsSupported (algorithm))
						continue;

					var encrypted = await MultipartEncrypted.EncryptAsync (algorithm, new[] { self }, body);

					//using (var file = File.Create ("pgp-encrypted.asc"))
					//	encrypted.WriteTo (file);

					// TODO: implement DecryptAsync
					var decrypted = encrypted.Decrypt (ctx);

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
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

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
			}
		}

		[Test]
		public async Task TestMultipartEncryptedEncryptAlgorithmUsingKeysAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			IList<PgpPublicKey> recipients;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = await ctx.GetPublicKeysAsync (new[] { self });
			}

			foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
				if (!IsSupported (algorithm))
					continue;

				var encrypted = await MultipartEncrypted.EncryptAsync (algorithm, recipients, body);

				//using (var file = File.Create ("pgp-encrypted.asc"))
				//	encrypted.WriteTo (file);

				// TODO: implement DecryptAsync
				var decrypted = encrypted.Decrypt ();

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
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

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				var decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
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

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public async Task TestMimeMessageSignAndEncryptAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			var message = new MimeMessage { Subject = "Test of signing with OpenPGP" };
			DigitalSignatureCollection signatures;

			using (var ctx = new DummyOpenPgpContext ()) {
				// throws because no Body has been set
				Assert.ThrowsAsync<InvalidOperationException> (() => message.SignAndEncryptAsync (ctx));

				message.Body = body;

				// throws because no sender has been set
				Assert.ThrowsAsync<InvalidOperationException> (() => message.SignAndEncryptAsync (ctx));

				message.From.Add (self);
				message.To.Add (self);

				await message.SignAndEncryptAsync (ctx);

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				var encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				// TODO: implement DecryptAsync
				var decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
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

				await message.SignAndEncryptAsync (ctx);

				Assert.That (message.Body, Is.InstanceOf<MultipartEncrypted> ());

				encrypted = (MultipartEncrypted) message.Body;

				//using (var file = File.Create ("pgp-signed-encrypted.asc"))
				//	encrypted.WriteTo (file);

				// TODO: implement DecryptAsync
				decrypted = encrypted.Decrypt (ctx, out signatures);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
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

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public async Task TestMultipartEncryptedSignAndEncryptAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;

			var encrypted = await MultipartEncrypted.SignAndEncryptAsync (self, DigestAlgorithm.Sha1, new[] { self }, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt (out signatures);

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
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

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public async Task TestMultipartEncryptedSignAndEncryptUsingKeysAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;
			IList<PgpPublicKey> recipients;
			PgpSecretKey signer;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = await ctx.GetPublicKeysAsync (new[] { self });
				signer = await ctx.GetSigningKeyAsync (self);
			}

			var encrypted = await MultipartEncrypted.SignAndEncryptAsync (signer, DigestAlgorithm.Sha1, recipients, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt (out signatures);

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
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

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public async Task TestMultipartEncryptedSignAndEncryptAlgorithmAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;

			var encrypted = await MultipartEncrypted.SignAndEncryptAsync (self, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, new[] { self }, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt (out signatures);

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public void TestMultipartEncryptedSignAndEncryptAlgorithmUsingKeys ()
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

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public async Task TestMultipartEncryptedSignAndEncryptAlgorithmUsingKeysAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", "AB0821A2");
			DigitalSignatureCollection signatures;
			IList<PgpPublicKey> recipients;
			PgpSecretKey signer;

			using (var ctx = new DummyOpenPgpContext ()) {
				recipients = await ctx.GetPublicKeysAsync (new[] { self });
				signer = await ctx.GetSigningKeyAsync (self);
			}

			var encrypted = await MultipartEncrypted.SignAndEncryptAsync (signer, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, recipients, body);

			//using (var file = File.Create ("pgp-signed-encrypted.asc"))
			//	encrypted.WriteTo (file);

			// TODO: implement DecryptAsync
			var decrypted = encrypted.Decrypt (out signatures);

			Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
			Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}
		}

		[Test]
		public void TestAutoKeyRetrieve ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp", "[Announce] GnuPG 2.1.20 released.eml"));
			var multipart = (MultipartSigned) ((Multipart) message.Body)[0];

			Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

			var protocol = multipart.ContentType.Parameters["protocol"];
			Assert.That (protocol, Is.EqualTo ("application/pgp-signature"), "The multipart/signed protocol does not match.");

			var micalg = multipart.ContentType.Parameters["micalg"];

			Assert.That (micalg, Is.EqualTo ("pgp-sha1"), "The multipart/signed micalg does not match.");

			Assert.That(multipart[0], Is.InstanceOf<TextPart>(), "The first child is not a text part.");
			Assert.That(multipart[1], Is.InstanceOf<ApplicationPgpSignature>(), "The second child is not a detached signature.");

			DigitalSignatureCollection signatures;

			try {
				signatures = multipart.Verify ();
			} catch (IOException ex) {
				if (ex.Message == "unknown PGP public key algorithm encountered") {
					Assert.Ignore ($"Known issue: {ex.Message}");
					return;
				}

				throw;
			}

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
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
		public async Task TestAutoKeyRetrieveAsync ()
		{
			var message = await MimeMessage.LoadAsync (Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp", "[Announce] GnuPG 2.1.20 released.eml"));
			var multipart = (MultipartSigned) ((Multipart) message.Body)[0];

			Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

			var protocol = multipart.ContentType.Parameters["protocol"];
			Assert.That (protocol, Is.EqualTo ("application/pgp-signature"), "The multipart/signed protocol does not match.");

			var micalg = multipart.ContentType.Parameters["micalg"];

			Assert.That (micalg, Is.EqualTo ("pgp-sha1"), "The multipart/signed micalg does not match.");

			Assert.That(multipart[0], Is.InstanceOf<TextPart>(), "The first child is not a text part.");
			Assert.That(multipart[1], Is.InstanceOf<ApplicationPgpSignature>(), "The second child is not a detached signature.");

			DigitalSignatureCollection signatures;

			try {
				signatures = await multipart.VerifyAsync ();
			} catch (IOException ex) {
				if (ex.Message == "unknown PGP public key algorithm encountered") {
					Assert.Ignore ($"Known issue: {ex.Message}");
					return;
				}

				throw;
			}

			Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
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
				Assert.That (ctx.KeyExchangeProtocol, Is.EqualTo ("application/pgp-keys"), "The key-exchange protocol does not match.");

				var keys = ctx.EnumeratePublicKeys (self).ToList ();
				var exported = ctx.Export (new [] { self });

				Assert.That (exported, Is.Not.Null, "The exported MIME part should not be null.");
				Assert.That(exported, Is.InstanceOf<MimePart>(), "The exported MIME part should be a MimePart.");
				Assert.That (exported.ContentType.MimeType, Is.EqualTo ("application/pgp-keys"));

				exported = ctx.Export (keys);

				Assert.That (exported, Is.Not.Null, "The exported MIME part should not be null.");
				Assert.That(exported, Is.InstanceOf<MimePart>(), "The exported MIME part should be a MimePart.");
				Assert.That (exported.ContentType.MimeType, Is.EqualTo ("application/pgp-keys"));

				using (var stream = new MemoryStream ()) {
					ctx.Export (new[] { self }, stream, true);

					Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
				}

				foreach (var keyring in ctx.EnumeratePublicKeyRings (self)) {
					using (var stream = new MemoryStream ()) {
						ctx.Export (new[] { self }, stream, true);

						Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
					}
				}

				using (var stream = new MemoryStream ()) {
					ctx.Export (keys, stream, true);

					Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
				}
			}
		}

		[Test]
		public async Task TestExportAsync ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = new DummyOpenPgpContext ()) {
				Assert.That (ctx.KeyExchangeProtocol, Is.EqualTo ("application/pgp-keys"), "The key-exchange protocol does not match.");

				var keys = ctx.EnumeratePublicKeys (self).ToList ();
				var exported = await ctx.ExportAsync (new[] { self });

				Assert.That (exported, Is.Not.Null, "The exported MIME part should not be null.");
				Assert.That(exported, Is.InstanceOf<MimePart>(), "The exported MIME part should be a MimePart.");
				Assert.That (exported.ContentType.MimeType, Is.EqualTo ("application/pgp-keys"));

				exported = await ctx.ExportAsync (keys);

				Assert.That (exported, Is.Not.Null, "The exported MIME part should not be null.");
				Assert.That(exported, Is.InstanceOf<MimePart>(), "The exported MIME part should be a MimePart.");
				Assert.That (exported.ContentType.MimeType, Is.EqualTo ("application/pgp-keys"));

				using (var stream = new MemoryStream ()) {
					await ctx.ExportAsync (new[] { self }, stream, true);

					Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
				}

				foreach (var keyring in ctx.EnumeratePublicKeyRings (self)) {
					using (var stream = new MemoryStream ()) {
						await ctx.ExportAsync (new[] { self }, stream, true);

						Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
					}
				}

				using (var stream = new MemoryStream ()) {
					await ctx.ExportAsync (keys, stream, true);

					Assert.That (stream.Length, Is.EqualTo (exported.Content.Stream.Length));
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

					Assert.That (ctx.DefaultEncryptionAlgorithm, Is.EqualTo (algorithm), "Default encryption algorithm does not match.");
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
					Assert.That (ctx.Supports (supports[i]), Is.True, supports[i]);

				Assert.That (ctx.Supports ("application/octet-stream"), Is.False, "application/octet-stream");
				Assert.That (ctx.Supports ("text/plain"), Is.False, "text/plain");
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

					Assert.That (algo, Is.EqualTo (digest));
				}

				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.MD5), Is.EqualTo (DigestAlgorithm.MD5));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha1), Is.EqualTo (DigestAlgorithm.Sha1));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.RipeMD160), Is.EqualTo (DigestAlgorithm.RipeMD160));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.DoubleSha), Is.EqualTo (DigestAlgorithm.DoubleSha));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.MD2), Is.EqualTo (DigestAlgorithm.MD2));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Tiger192), Is.EqualTo (DigestAlgorithm.Tiger192));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Haval5pass160), Is.EqualTo (DigestAlgorithm.Haval5160));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha256), Is.EqualTo (DigestAlgorithm.Sha256));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha384), Is.EqualTo (DigestAlgorithm.Sha384));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha512), Is.EqualTo (DigestAlgorithm.Sha512));
				Assert.That (OpenPgpContext.GetDigestAlgorithm (Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha224), Is.EqualTo (DigestAlgorithm.Sha224));

				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaGeneral), Is.EqualTo (PublicKeyAlgorithm.RsaGeneral));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaEncrypt), Is.EqualTo (PublicKeyAlgorithm.RsaEncrypt));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.RsaSign), Is.EqualTo (PublicKeyAlgorithm.RsaSign));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ElGamalGeneral), Is.EqualTo (PublicKeyAlgorithm.ElGamalGeneral));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ElGamalEncrypt), Is.EqualTo (PublicKeyAlgorithm.ElGamalEncrypt));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.Dsa), Is.EqualTo (PublicKeyAlgorithm.Dsa));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDH), Is.EqualTo (PublicKeyAlgorithm.EllipticCurve));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDsa), Is.EqualTo (PublicKeyAlgorithm.EllipticCurveDsa));
				Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.DiffieHellman), Is.EqualTo (PublicKeyAlgorithm.DiffieHellman));
				//Assert.That (OpenPgpContext.GetPublicKeyAlgorithm (Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.EdDSA), Is.EqualTo (PublicKeyAlgorithm.EdwardsCurveDsa));
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
				var clientEastwood = new MailboxAddress ("Man with No Name", "client.eastwood@fistfullofdollars.com");
				var mailboxes = new [] { new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com") };
				var emptyMailboxes = Array.Empty<MailboxAddress> ();
				var pubkeys = ctx.GetPublicKeys (mailboxes);
				var key = ctx.GetSigningKey (mailboxes[0]);
				var emptyPubkeys = Array.Empty<PgpPublicKey> ();
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
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.CanSignAsync (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.CanEncryptAsync (null));

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

				// EncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync ((MailboxAddress[]) null, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync ((PgpPublicKey[]) null, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, emptyMailboxes, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, emptyPubkeys, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.EncryptAsync (emptyMailboxes, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.EncryptAsync (emptyPubkeys, stream), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, mailboxes, null), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (EncryptionAlgorithm.Cast5, pubkeys, null), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (mailboxes, null), "EncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (pubkeys, null), "EncryptAsync");

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

				// ExportAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((PgpPublicKeyRingBundle) null), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((MailboxAddress[]) null), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((PgpPublicKey[]) null), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((PgpPublicKeyRingBundle) null, stream, true), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((MailboxAddress[]) null, stream, true), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync ((PgpPublicKey[]) null, stream, true), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync (ctx.PublicKeyRingBundle, null, true), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync (mailboxes, null, true), "ExportAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync (pubkeys, null, true), "ExportAsync");

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
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.DecryptToAsync (null, stream), "DecryptToAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.DecryptToAsync (stream, null), "DecryptToAsync");

				// GetDigestAlgorithmName
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.None), "GetDigestAlgorithmName");

				// GetPublicKeys
				Assert.Throws<ArgumentNullException> (() => ctx.GetPublicKeys (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.GetPublicKeysAsync (null));
				Assert.Throws<PublicKeyNotFoundException> (() => ctx.GetPublicKeys (new MailboxAddress[] { clientEastwood }));
				Assert.ThrowsAsync<PublicKeyNotFoundException> (() => ctx.GetPublicKeysAsync (new MailboxAddress[] { clientEastwood }));

				// GetSigningKey
				Assert.Throws<ArgumentNullException> (() => ctx.GetSigningKey (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.GetSigningKeyAsync (null));
				Assert.Throws<PrivateKeyNotFoundException> (() => ctx.GetSigningKey (clientEastwood));
				Assert.ThrowsAsync<PrivateKeyNotFoundException> (() => ctx.GetSigningKeyAsync (clientEastwood));

				// Import
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpPublicKeyRingBundle) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRing) null), "Import");
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((PgpSecretKeyRingBundle) null), "Import");

				// ImportAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((Stream) null), "ImportAsync");
				//Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((PgpPublicKeyRing) null), "ImportAsync");
				//Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((PgpPublicKeyRingBundle) null), "ImportAsync");
				//Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((PgpSecretKeyRing) null), "ImportAsync");
				//Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((PgpSecretKeyRingBundle) null), "ImportAsync");

				// Sign
				Assert.Throws<ArgumentNullException> (() => ctx.Sign ((MailboxAddress) null, DigestAlgorithm.Sha1, stream), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign ((PgpSecretKey) null, DigestAlgorithm.Sha1, stream), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (mailboxes[0], DigestAlgorithm.Sha1, null), "Sign");
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (key, DigestAlgorithm.Sha1, null), "Sign");

				// SignAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, stream), "SignAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, stream), "SignAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (mailboxes[0], DigestAlgorithm.Sha1, null), "SignAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (key, DigestAlgorithm.Sha1, null), "SignAsync");

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

				// SignAndEncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null), "SignAndEncryptAsync");

				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, (MailboxAddress[]) null, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, (PgpPublicKey[]) null, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, emptyPubkeys, stream), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null), "SignAndEncryptAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, pubkeys, null), "SignAndEncryptAsync");

				// SignKey
				Assert.Throws<ArgumentNullException> (() => ctx.SignKey (null, pubkeys[0]), "SignKey");
				Assert.Throws<ArgumentNullException> (() => ctx.SignKey (key, null), "SignKey");

				// Supports
				Assert.Throws<ArgumentNullException> (() => ctx.Supports (null), "Supports");

				// Verify
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, stream), "Verify");
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (stream, null), "Verify");

				// VerifyAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.VerifyAsync (null, stream), "VerifyAsync");
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.VerifyAsync (stream, null), "VerifyAsync");

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

				// EncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync ((MailboxAddress[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync ((PgpPublicKey[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (null, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, (MailboxAddress[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (ctx, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (null, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, (PgpPublicKey[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (ctx, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (EncryptionAlgorithm.Cast5, pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (null, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, (MailboxAddress[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (null, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, (PgpPublicKey[]) null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.EncryptAsync (ctx, EncryptionAlgorithm.Cast5, pubkeys, null));

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

				// SignAndEncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (null, mailboxes[0], DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (null, key, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (null, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyMailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (null, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, null, entity));
				Assert.ThrowsAsync<ArgumentException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, emptyPubkeys, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartEncrypted.SignAndEncryptAsync (ctx, key, DigestAlgorithm.Sha1, EncryptionAlgorithm.Cast5, pubkeys, null));

				var encrypted = new MultipartEncrypted ();

				Assert.Throws<ArgumentNullException> (() => encrypted.Accept (null));

				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null));
				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null, out signatures));

				// MultipartSigned.Create
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (null, mailboxes[0], DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, mailboxes[0], DigestAlgorithm.Sha1, null));

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (null, key, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, key, DigestAlgorithm.Sha1, null));

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (key, DigestAlgorithm.Sha1, null));

				// MultipartSigned.CreateAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (null, mailboxes[0], DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, mailboxes[0], DigestAlgorithm.Sha1, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (null, key, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, (PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, key, DigestAlgorithm.Sha1, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync ((PgpSecretKey) null, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (key, DigestAlgorithm.Sha1, null));

				var signed = MultipartSigned.Create (key, DigestAlgorithm.Sha1, entity);

				Assert.Throws<ArgumentNullException> (() => signed.Accept (null));
				Assert.Throws<ArgumentOutOfRangeException> (() => signed.Prepare (EncodingConstraint.SevenBit, 0));

				Assert.Throws<ArgumentNullException> (() => signed.Verify (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => signed.VerifyAsync (null));
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

			PumpDataThroughFilter (filter, Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp", "mimekit.gpg.pub"), true);
			Assert.That (filter.DataType, Is.EqualTo (OpenPgpDataType.PublicKey));
			Assert.That (filter.BeginOffset, Is.EqualTo (0));
			Assert.That (filter.EndOffset, Is.EqualTo (1754));

			filter.Reset ();

			PumpDataThroughFilter (filter, Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp", "mimekit.gpg.sec"), true);
			Assert.That (filter.DataType, Is.EqualTo (OpenPgpDataType.PrivateKey));
			Assert.That (filter.BeginOffset, Is.EqualTo (0));
			Assert.That (filter.EndOffset, Is.EqualTo (3650));
		}
	}
}
