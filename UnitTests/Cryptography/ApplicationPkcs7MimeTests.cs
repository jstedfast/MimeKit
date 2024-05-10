//
// ApplicationPkcs7MimeTests.cs
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

using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;

using MimeKit;
using MimeKit.Cryptography;

using BCX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	[TestFixture]
	public abstract class ApplicationPkcs7MimeTestsBase
	{
		protected abstract SecureMimeContext CreateContext ();

		protected virtual EncryptionAlgorithm[] GetEncryptionAlgorithms (IDigitalSignature signature)
		{
			return ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var entity = new TextPart ("plain") { Text = "This is some text..." };
			var mailbox = new MailboxAddress ("MimeKit UnitTests", rsa.EmailAddress);
			var signer = new CmsSigner (rsa.FileName, "no.secret");
			var recipients = new CmsRecipientCollection ();
			var mailboxes = new [] { mailbox };

			recipients.Add (new CmsRecipient (signer.Certificate));

			using (var ctx = CreateContext ()) {
				ctx.Import (rsa.FileName, "no.secret");

				// Compress
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (ctx, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (null));

				// CompressAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.CompressAsync (null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.CompressAsync (ctx, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.CompressAsync (null));

				// Encrypt
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (null, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, (IEnumerable<MailboxAddress>) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, (CmsRecipientCollection) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt ((IEnumerable<MailboxAddress>) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt ((CmsRecipientCollection) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (mailboxes, null));

				// EncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (null, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (null, recipients, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (ctx, (IEnumerable<MailboxAddress>) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (ctx, (CmsRecipientCollection) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (ctx, recipients, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (ctx, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync ((IEnumerable<MailboxAddress>) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync ((CmsRecipientCollection) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (recipients, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.EncryptAsync (mailboxes, null));

				// Sign
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (null, mailbox, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (null, signer, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, (CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, mailbox, DigestAlgorithm.Sha1, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, signer, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign ((MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign ((CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (mailbox, DigestAlgorithm.Sha1, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (signer, null));

				// SignAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (null, mailbox, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (null, signer, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (ctx, (CmsSigner) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (ctx, mailbox, DigestAlgorithm.Sha1, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (ctx, signer, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync ((MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync ((CmsSigner) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (mailbox, DigestAlgorithm.Sha1, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAsync (signer, null));

				// SignAndEncrypt
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, mailbox, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, mailbox, DigestAlgorithm.Sha1, mailboxes, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (mailbox, DigestAlgorithm.Sha1, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, signer, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, signer, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, signer, recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (signer, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (signer, recipients, null));

				// SignAndEncryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (null, mailbox, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, mailbox, DigestAlgorithm.Sha1, mailboxes, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (mailbox, DigestAlgorithm.Sha1, mailboxes, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (null, signer, recipients, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, null, recipients, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, signer, null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (ctx, signer, recipients, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (null, recipients, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (signer, null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncryptAsync (signer, recipients, null));

				var compressed = ApplicationPkcs7Mime.Compress (ctx, entity);
				var encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, entity);
				var signed = ApplicationPkcs7Mime.Sign (ctx, signer, entity);

				// Decompress
				Assert.Throws<ArgumentNullException> (() => compressed.Decompress (null));
				Assert.Throws<InvalidOperationException> (() => encrypted.Decompress ());
				Assert.Throws<InvalidOperationException> (() => encrypted.Decompress (ctx));
				Assert.Throws<InvalidOperationException> (() => signed.Decompress ());
				Assert.Throws<InvalidOperationException> (() => signed.Decompress (ctx));

				// DecompressAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => compressed.DecompressAsync (null));
				Assert.ThrowsAsync<InvalidOperationException> (() => encrypted.DecompressAsync ());
				Assert.ThrowsAsync<InvalidOperationException> (() => encrypted.DecompressAsync (ctx));
				Assert.ThrowsAsync<InvalidOperationException> (() => signed.DecompressAsync ());
				Assert.ThrowsAsync<InvalidOperationException> (() => signed.DecompressAsync (ctx));

				// Decrypt
				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null));
				Assert.Throws<InvalidOperationException> (() => compressed.Decrypt (ctx));
				Assert.Throws<InvalidOperationException> (() => signed.Decrypt (ctx));

				// DecryptAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => encrypted.DecryptAsync (null));
				Assert.ThrowsAsync<InvalidOperationException> (() => compressed.DecryptAsync (ctx));
				Assert.ThrowsAsync<InvalidOperationException> (() => signed.DecryptAsync (ctx));

				// Import
				Assert.Throws<ArgumentNullException> (() => encrypted.Import (null));
				Assert.Throws<InvalidOperationException> (() => compressed.Import (ctx));
				Assert.Throws<InvalidOperationException> (() => signed.Import (ctx));

				// ImportAsync
				Assert.ThrowsAsync<ArgumentNullException> (() => encrypted.ImportAsync (null));
				Assert.ThrowsAsync<InvalidOperationException> (() => compressed.ImportAsync (ctx));
				Assert.ThrowsAsync<InvalidOperationException> (() => signed.ImportAsync (ctx));

				// Verify
				Assert.Throws<ArgumentNullException> (() => {
					signed.Verify (null, out _);
				});
				Assert.Throws<InvalidOperationException> (() => {
					compressed.Verify (ctx, out _);
				});
				Assert.Throws<InvalidOperationException> (() => {
					encrypted.Verify (ctx, out _);
				});
			}
		}

		[Test]
		public void TestSecureMimeTypes ()
		{
			using (var stream = new MemoryStream (Array.Empty<byte> (), false)) {
				ApplicationPkcs7Mime pkcs7;

				Assert.Throws<ArgumentOutOfRangeException> (() => new ApplicationPkcs7Mime (SecureMimeType.Unknown, stream));

				pkcs7 = new ApplicationPkcs7Mime (SecureMimeType.AuthEnvelopedData, stream);
				Assert.That (pkcs7.ContentType.Parameters["smime-type"], Is.EqualTo ("authenveloped-data"));
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.AuthEnvelopedData));

				pkcs7 = new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, stream);
				Assert.That (pkcs7.ContentType.Parameters["smime-type"], Is.EqualTo ("enveloped-data"));
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData));

				pkcs7 = new ApplicationPkcs7Mime (SecureMimeType.CompressedData, stream);
				Assert.That (pkcs7.ContentType.Parameters["smime-type"], Is.EqualTo ("compressed-data"));
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.CompressedData));

				pkcs7 = new ApplicationPkcs7Mime (SecureMimeType.SignedData, stream);
				Assert.That (pkcs7.ContentType.Parameters["smime-type"], Is.EqualTo ("signed-data"));
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData));

				pkcs7 = new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, stream);
				Assert.That (pkcs7.ContentType.Parameters["smime-type"], Is.EqualTo ("certs-only"));
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.CertsOnly));

				pkcs7.ContentType.Parameters["smime-type"] = "x-unknown-data";
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.Unknown));

				pkcs7.ContentType.Parameters.Remove ("smime-type");
				Assert.That (pkcs7.SecureMimeType, Is.EqualTo (SecureMimeType.Unknown));
			}
		}

		static void ImportAll (SecureMimeContext ctx)
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var windows = ctx as WindowsSecureMimeContext;

			// Import the StartCom certificates
			if (windows is not null) {
				var parser = new X509CertificateParser ();

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComCertificationAuthority.crt"))) {
					foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.AuthRoot, certificate);
				}

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComClass1PrimaryIntermediateClientCA.crt"))) {
					foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.CertificateAuthority, certificate);
				}
			} else {
				foreach (var filename in SecureMimeTestsBase.StartComCertificates) {
					var path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path)) {
						if (ctx is DefaultSecureMimeContext sqlite) {
							sqlite.Import (stream, true);
						} else {
							var parser = new X509CertificateParser ();
							foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
								ctx.Import (certificate);
						}
					}
				}
			}

			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var chain = certificate.Chain;

				// Import the root & intermediate certificates from the smime.pfx file
				if (windows is not null) {
					var store = StoreName.AuthRoot;
					for (int i = chain.Length - 1; i > 0; i--) {
						windows.Import (store, chain[i]);
						store = StoreName.CertificateAuthority;
					}
				} else {
					for (int i = chain.Length - 1; i > 0; i--) {
						if (ctx is DefaultSecureMimeContext sqlite) {
							sqlite.Import (chain[i], true);
						} else {
							ctx.Import (chain[i]);
						}
					}
				}

				// Import the pfx file directly in order to get the private key
				ctx.Import (certificate.FileName, "no.secret");
			}
		}

		static async Task ImportAllAsync (SecureMimeContext ctx)
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var windows = ctx as WindowsSecureMimeContext;

			// Import the StartCom certificates
			if (windows is not null) {
				var parser = new X509CertificateParser ();

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComCertificationAuthority.crt"))) {
					foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.AuthRoot, certificate);
				}

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComClass1PrimaryIntermediateClientCA.crt"))) {
					foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.CertificateAuthority, certificate);
				}
			} else {
				foreach (var filename in SecureMimeTestsBase.StartComCertificates) {
					var path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path)) {
						if (ctx is DefaultSecureMimeContext sqlite) {
							await sqlite.ImportAsync (stream, true).ConfigureAwait (false);
						} else {
							var parser = new X509CertificateParser ();
							foreach (BCX509Certificate certificate in parser.ReadCertificates (stream))
								await ctx.ImportAsync (certificate).ConfigureAwait (false);
						}
					}
				}
			}

			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var chain = certificate.Chain;

				// Import the root & intermediate certificates from the smime.pfx file
				if (windows is not null) {
					var store = StoreName.AuthRoot;
					for (int i = chain.Length - 1; i > 0; i--) {
						windows.Import (store, chain[i]);
						store = StoreName.CertificateAuthority;
					}
				} else {
					for (int i = chain.Length - 1; i > 0; i--) {
						if (ctx is DefaultSecureMimeContext sqlite) {
							await sqlite.ImportAsync (chain[i], true).ConfigureAwait (false);
						} else {
							await ctx.ImportAsync (chain[i]).ConfigureAwait (false);
						}
					}
				}

				// Import the pfx file directly in order to get the private key
				await ctx.ImportAsync (certificate.FileName, "no.secret").ConfigureAwait (false);
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public void TestEncryptCmsRecipients (SubjectIdentifierType recipientIdentifierType)
		{
			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var entity = new TextPart ("plain") { Text = "This is some text..." };
				var signer = new CmsSigner (certificate.FileName, "no.secret");
				var recipients = new CmsRecipientCollection {
					new CmsRecipient (signer.Certificate, recipientIdentifierType)
				};

				var encrypted = ApplicationPkcs7Mime.Encrypt (recipients, entity);

				using (var ctx = CreateContext ()) {
					ctx.Import (certificate.FileName, "no.secret");

					var decrypted = encrypted.Decrypt (ctx);
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public async Task TestEncryptCmsRecipientsAsync (SubjectIdentifierType recipientIdentifierType)
		{
			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var entity = new TextPart ("plain") { Text = "This is some text..." };
				var signer = new CmsSigner (certificate.FileName, "no.secret");
				var recipients = new CmsRecipientCollection {
					new CmsRecipient (signer.Certificate, recipientIdentifierType)
				};

				var encrypted = await ApplicationPkcs7Mime.EncryptAsync (recipients, entity).ConfigureAwait (false);

				using (var ctx = CreateContext ()) {
					await ctx.ImportAsync (certificate.FileName, "no.secret").ConfigureAwait (false);

					var decrypted = await encrypted.DecryptAsync (ctx).ConfigureAwait (false);
				}
			}
		}

		[Test]
		public void TestEncryptMailboxes ()
		{
			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var entity = new TextPart ("plain") { Text = "This is some text..." };
				var mailbox = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var mailboxes = new[] { mailbox };

				using (var ctx = CreateContext ()) {
					ApplicationPkcs7Mime encrypted;
					MimeEntity decrypted;
					TextPart text;

					ctx.Import (certificate.FileName, "no.secret");

					encrypted = ApplicationPkcs7Mime.Encrypt (mailboxes, entity);
					decrypted = encrypted.Decrypt (ctx);
					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted from Encrypt(mailboxes, entity)");
					text = (TextPart) decrypted;
					Assert.That (text.Text, Is.EqualTo (entity.Text), "Decrypted text");

					encrypted = ApplicationPkcs7Mime.Encrypt (ctx, mailboxes, entity);
					decrypted = encrypted.Decrypt (ctx);
					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Encrypt(ctx, mailboxes, entity)");
					text = (TextPart) decrypted;
					Assert.That (text.Text, Is.EqualTo (entity.Text), "Decrypted text");
				}
			}
		}

		[Test]
		public async Task TestEncryptMailboxesAsync ()
		{
			foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
				var entity = new TextPart ("plain") { Text = "This is some text..." };
				var mailbox = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var mailboxes = new[] { mailbox };

				using (var ctx = CreateContext ()) {
					ApplicationPkcs7Mime encrypted;
					MimeEntity decrypted;
					TextPart text;

					await ctx.ImportAsync (certificate.FileName, "no.secret").ConfigureAwait (false);

					encrypted = await ApplicationPkcs7Mime.EncryptAsync (mailboxes, entity).ConfigureAwait (false);
					decrypted = await encrypted.DecryptAsync (ctx).ConfigureAwait (false);
					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted from EncryptAsync(mailboxes, entity)");
					text = (TextPart) decrypted;
					Assert.That (text.Text, Is.EqualTo (entity.Text), "Decrypted text");

					encrypted = await ApplicationPkcs7Mime.EncryptAsync (ctx, mailboxes, entity).ConfigureAwait (false);
					decrypted = await encrypted.DecryptAsync (ctx).ConfigureAwait (false);
					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "EncryptAsync(ctx, mailboxes, entity)");
					text = (TextPart) decrypted;
					Assert.That (text.Text, Is.EqualTo (entity.Text), "Decrypted text");
				}
			}
		}

		void AssertSignResults (SMimeCertificate certificate, SecureMimeContext ctx, ApplicationPkcs7Mime signed, TextPart entity)
		{
			var signatures = signed.Verify (ctx, out var encapsulated);

			Assert.That (encapsulated, Is.InstanceOf<TextPart> (), "TextPart");
			Assert.That (((TextPart) encapsulated).Text, Is.EqualTo (entity.Text), "Text");

			Assert.That (signatures.Count, Is.EqualTo (1), "Signature count");

			var signature = signatures[0];

			Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
			Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
			Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));
			Assert.That (signature.SignerCertificate.CreationDate, Is.EqualTo (certificate.CreationDate), "CreationDate");
			Assert.That (signature.SignerCertificate.ExpirationDate, Is.EqualTo (certificate.ExpirationDate), "ExpirationDate");
			Assert.That (signature.SignerCertificate.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));
			Assert.That (signature.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));

			try {
				bool valid = signature.Verify ();

				Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
			} catch (DigitalSignatureVerifyException ex) {
				if (ctx is WindowsSecureMimeContext) {
					// AppVeyor gets an exception about the root certificate not being trusted
					Assert.That (ex.InnerException.Message, Is.EqualTo (SecureMimeTestsBase.UntrustedRootCertificateMessage));
				} else {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}

			var algorithms = GetEncryptionAlgorithms (signature);
			int i = 0;

			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
		}

		[Test]
		public void TestSignCmsSigner ()
		{
			using (var ctx = CreateContext ()) {
				ImportAll (ctx);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var signer = new CmsSigner (certificate.FileName, "no.secret");

					var signed = ApplicationPkcs7Mime.Sign (ctx, signer, entity);
					AssertSignResults (certificate, ctx, signed, entity);
				}
			}
		}

		[Test]
		public async Task TestSignCmsSignerAsync ()
		{
			using (var ctx = CreateContext ()) {
				await ImportAllAsync (ctx).ConfigureAwait (false);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var signer = new CmsSigner (certificate.FileName, "no.secret");

					var signed = await ApplicationPkcs7Mime.SignAsync (ctx, signer, entity).ConfigureAwait (false);
					AssertSignResults (certificate, ctx, signed, entity);
				}
			}
		}

		[Test]
		public void TestSignMailbox ()
		{
			using (var ctx = CreateContext ()) {
				ImportAll (ctx);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var mailbox = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
					var entity = new TextPart ("plain") { Text = "This is some text..." };

					var signed = ApplicationPkcs7Mime.Sign (ctx, mailbox, DigestAlgorithm.Sha224, entity);
					AssertSignResults (certificate, ctx, signed, entity);
				}
			}
		}

		[Test]
		public async Task TestSignMailboxAsync ()
		{
			using (var ctx = CreateContext ()) {
				await ImportAllAsync (ctx).ConfigureAwait (false);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var mailbox = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
					var entity = new TextPart ("plain") { Text = "This is some text..." };

					var signed = await ApplicationPkcs7Mime.SignAsync (ctx, mailbox, DigestAlgorithm.Sha224, entity).ConfigureAwait (false);
					AssertSignResults (certificate, ctx, signed, entity);
				}
			}
		}

		void AssertSignAndEncryptResults (SMimeCertificate certificate, SecureMimeContext ctx, ApplicationPkcs7Mime encrypted, TextPart entity)
		{
			var decrypted = encrypted.Decrypt (ctx);

			Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "MultipartSigned");

			var signed = (MultipartSigned) decrypted;
			Assert.That (signed.Count, Is.EqualTo (2), "MultipartSigned count");

			var encapsulated = signed[0];

			var signatures = signed.Verify (ctx);

			Assert.That (encapsulated, Is.InstanceOf<TextPart> (), "TextPart");
			Assert.That (((TextPart) encapsulated).Text, Is.EqualTo (entity.Text), "Text");

			Assert.That (signatures.Count, Is.EqualTo (1), "Signature count");

			var signature = signatures[0];

			Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
			Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
			Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));
			Assert.That (signature.SignerCertificate.CreationDate, Is.EqualTo (certificate.CreationDate), "CreationDate");
			Assert.That (signature.SignerCertificate.ExpirationDate, Is.EqualTo (certificate.ExpirationDate), "ExpirationDate");
			Assert.That (signature.SignerCertificate.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));
			Assert.That (signature.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));

			try {
				bool valid = signature.Verify ();

				Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
			} catch (DigitalSignatureVerifyException ex) {
				if (ctx is WindowsSecureMimeContext) {
					// AppVeyor gets an exception about the root certificate not being trusted
					Assert.That (ex.InnerException.Message, Is.EqualTo (SecureMimeTestsBase.UntrustedRootCertificateMessage));
				} else {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}

			var algorithms = GetEncryptionAlgorithms (signature);
			int i = 0;

			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
		}

		async Task AssertSignAndEncryptResultsAsync (SMimeCertificate certificate, SecureMimeContext ctx, ApplicationPkcs7Mime encrypted, TextPart entity)
		{
			var decrypted = await encrypted.DecryptAsync (ctx).ConfigureAwait (false);

			Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "MultipartSigned");

			var signed = (MultipartSigned) decrypted;
			Assert.That (signed.Count, Is.EqualTo (2), "MultipartSigned count");

			var encapsulated = signed[0];

			var signatures = await signed.VerifyAsync (ctx).ConfigureAwait (false);

			Assert.That (encapsulated, Is.InstanceOf<TextPart> (), "TextPart");
			Assert.That (((TextPart) encapsulated).Text, Is.EqualTo (entity.Text), "Text");

			Assert.That (signatures.Count, Is.EqualTo (1), "Signature count");

			var signature = signatures[0];

			Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
			Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
			Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));
			Assert.That (signature.SignerCertificate.CreationDate, Is.EqualTo (certificate.CreationDate), "CreationDate");
			Assert.That (signature.SignerCertificate.ExpirationDate, Is.EqualTo (certificate.ExpirationDate), "ExpirationDate");
			Assert.That (signature.SignerCertificate.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));
			Assert.That (signature.PublicKeyAlgorithm, Is.EqualTo (certificate.PublicKeyAlgorithm));

			try {
				bool valid = signature.Verify ();

				Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
			} catch (DigitalSignatureVerifyException ex) {
				if (ctx is WindowsSecureMimeContext) {
					// AppVeyor gets an exception about the root certificate not being trusted
					Assert.That (ex.InnerException.Message, Is.EqualTo (SecureMimeTestsBase.UntrustedRootCertificateMessage));
				} else {
					Assert.Fail ($"Failed to verify signature: {ex}");
				}
			}

			var algorithms = GetEncryptionAlgorithms (signature);
			int i = 0;

			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
			Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
		}

		[Test]
		public void TestSignAndEncryptCms ()
		{
			using (var ctx = CreateContext ()) {
				ImportAll (ctx);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var signer = new CmsSigner (certificate.FileName, "no.secret");
					var recipients = new CmsRecipientCollection {
						new CmsRecipient (signer.Certificate)
					};

					var encrypted = ApplicationPkcs7Mime.SignAndEncrypt (signer, recipients, entity);
					AssertSignAndEncryptResults (certificate, ctx, encrypted, entity);
				}
			}
		}

		[Test]
		public async Task TestSignAndEncryptCmsAsync ()
		{
			using (var ctx = CreateContext ()) {
				await ImportAllAsync (ctx).ConfigureAwait (false);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var signer = new CmsSigner (certificate.FileName, "no.secret");
					var recipients = new CmsRecipientCollection {
						new CmsRecipient (signer.Certificate)
					};

					var encrypted = await ApplicationPkcs7Mime.SignAndEncryptAsync (signer, recipients, entity).ConfigureAwait (false);
					await AssertSignAndEncryptResultsAsync (certificate, ctx, encrypted, entity).ConfigureAwait (false);
				}
			}
		}

		[Test]
		public void TestSignAndEncryptMailboxes ()
		{
			using (var ctx = CreateContext ()) {
				ImportAll (ctx);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var mailbox = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var recipients = new MailboxAddress[] { mailbox };

					var encrypted = ApplicationPkcs7Mime.SignAndEncrypt (mailbox, DigestAlgorithm.Sha224, recipients, entity);
					AssertSignAndEncryptResults (certificate, ctx, encrypted, entity);
				}
			}
		}

		[Test]
		public async Task TestSignAndEncryptMailboxesAsync ()
		{
			using (var ctx = CreateContext ()) {
				await ImportAllAsync (ctx).ConfigureAwait (false);

				foreach (var certificate in SecureMimeTestsBase.SupportedCertificates) {
					var mailbox = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
					var entity = new TextPart ("plain") { Text = "This is some text..." };
					var recipients = new MailboxAddress[] { mailbox };

					var encrypted = await ApplicationPkcs7Mime.SignAndEncryptAsync (mailbox, DigestAlgorithm.Sha224, recipients, entity).ConfigureAwait (false);
					await AssertSignAndEncryptResultsAsync (certificate, ctx, encrypted, entity).ConfigureAwait (false);
				}
			}
		}
	}

	[TestFixture]
	public class ApplicationPkcs7MimeTests : ApplicationPkcs7MimeTestsBase
	{
		readonly TemporarySecureMimeContext ctx = new TemporarySecureMimeContext (new SecureRandom (new CryptoApiRandomGenerator ())) { CheckCertificateRevocation = true };

		public ApplicationPkcs7MimeTests ()
		{
			CryptographyContext.Register (CreateContext);
		}

		protected override SecureMimeContext CreateContext ()
		{
			return ctx;
		}
	}

	[TestFixture]
	public class ApplicationPkcs7MimeSqliteTests : ApplicationPkcs7MimeTestsBase
	{
		class MySecureMimeContext : DefaultSecureMimeContext
		{
			public MySecureMimeContext () : base ("pkcs7.db", "no.secret")
			{
				CheckCertificateRevocation = true;
			}
		}

		public ApplicationPkcs7MimeSqliteTests ()
		{
			CryptographyContext.Register (typeof (MySecureMimeContext));
		}

		protected override SecureMimeContext CreateContext ()
		{
			return new MySecureMimeContext ();
		}

		static ApplicationPkcs7MimeSqliteTests ()
		{
			if (File.Exists ("pkcs7.db"))
				File.Delete ("pkcs7.db");
		}
	}
}
