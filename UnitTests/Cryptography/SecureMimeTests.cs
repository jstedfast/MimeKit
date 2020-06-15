//
// SecureMimeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;

using MimeKit;
using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	public abstract class SecureMimeTestsBase
	{
		//const string ExpiredCertificateMessage = "A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.\r\n";
		const string ExpiredCertificateMessage = "The certificate is revoked.\r\n";
		const string UntrustedRootCertificateMessage = "A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.\r\n";
		const string ThunderbirdFingerprint = "354ea4dcf98166639b58ec5df06a65de0cd8a95c";
		const string MimeKitFingerprint = "ba4403cd3d876ae8cd261575820330086cc3cbc8";
		const string ThunderbirdName = "fejj@gnome.org";

		static readonly DateTime MimeKitCreationDate = new DateTime (2019, 11, 05, 03, 00, 15);
		static readonly DateTime MimeKitExpirationDate = new DateTime (2029, 11, 02, 03, 00, 15);
		readonly X509Certificate MimeKitCertificate;

		static readonly string[] StartComCertificates = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		protected virtual bool IsEnabled { get { return true; } }
		protected abstract SecureMimeContext CreateContext ();

		protected SecureMimeTestsBase ()
		{
			if (!IsEnabled)
				return;

			using (var ctx = CreateContext ()) {
				var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
				string path;

				if (ctx is TemporarySecureMimeContext)
					CryptographyContext.Register (() => CreateContext ());
				else
					CryptographyContext.Register (ctx.GetType ());

				var chain = LoadPkcs12CertificateChain (Path.Combine (dataDir, "smime.pfx"), "no.secret");
				MimeKitCertificate = chain[0];

				if (ctx is WindowsSecureMimeContext) {
					var windows = (WindowsSecureMimeContext) ctx;
					var parser = new X509CertificateParser ();

					using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComCertificationAuthority.crt"))) {
						foreach (X509Certificate certificate in parser.ReadCertificates (stream))
							windows.Import (StoreName.AuthRoot, certificate);
					}

					using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComClass1PrimaryIntermediateClientCA.crt"))) {
						foreach (X509Certificate certificate in parser.ReadCertificates (stream))
							windows.Import (StoreName.CertificateAuthority, certificate);
					}

					// import the root & intermediate certificates from the smime.pfx file
					var store = StoreName.AuthRoot;
					for (int i = chain.Length - 1; i > 0; i--) {
						windows.Import (store, chain[i]);
						store = StoreName.CertificateAuthority;
					}
				} else {
					foreach (var filename in StartComCertificates) {
						path = Path.Combine (dataDir, filename);
						using (var stream = File.OpenRead (path)) {
							if (ctx is DefaultSecureMimeContext) {
								((DefaultSecureMimeContext) ctx).Import (stream, true);
							} else {
								var parser = new X509CertificateParser ();
								foreach (X509Certificate certificate in parser.ReadCertificates (stream))
									ctx.Import (certificate);
							}
						}
					}

					// import the root & intermediate certificates from the smime.pfx file
					for (int i = chain.Length - 1; i > 0; i--) {
						if (ctx is DefaultSecureMimeContext) {
							((DefaultSecureMimeContext) ctx).Import (chain[i], true);
						} else {
							ctx.Import (chain[i]);
						}
					}
				}

				path = Path.Combine (dataDir, "smime.pfx");
				ctx.Import (path, "no.secret");

				// import a second time to cover the case where the certificate & private key already exist
				Assert.DoesNotThrow (() => ctx.Import (path, "no.secret"));
			}
		}

		public static X509Certificate LoadCertificate (string path)
		{
			using (var stream = File.OpenRead (path)) {
				var parser = new X509CertificateParser ();

				return parser.ReadCertificate (stream);
			}
		}

		public static X509Certificate[] LoadPkcs12CertificateChain (string fileName, string password)
		{
			using (var stream = File.OpenRead (fileName)) {
				var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

				foreach (string alias in pkcs12.Aliases) {
					if (pkcs12.IsKeyEntry (alias)) {
						var chain = pkcs12.GetCertificateChain (alias);
						var entry = pkcs12.GetKey (alias);

						if (!entry.Key.IsPrivate)
							continue;

						var certificates = new X509Certificate[chain.Length];
						for (int i = 0; i < chain.Length; i++)
							certificates[i] = chain[i].Certificate;

						return certificates;
					}
				}

				return new X509Certificate[0];
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var stream = new MemoryStream ();

			Assert.Throws<ArgumentNullException> (() => new ApplicationPkcs7Signature ((MimeEntityConstructorArgs) null));
			Assert.Throws<ArgumentNullException> (() => new ApplicationPkcs7Mime ((MimeEntityConstructorArgs) null));
			Assert.Throws<ArgumentNullException> (() => new ApplicationPkcs7Signature ((Stream) null));

			// Accept
			Assert.Throws<ArgumentNullException> (() => new ApplicationPkcs7Mime (SecureMimeType.SignedData, stream).Accept (null));
			Assert.Throws<ArgumentNullException> (() => new ApplicationPkcs7Signature (stream).Accept (null));

			Assert.Throws<ArgumentOutOfRangeException> (() => SecureMimeContext.GetDigestOid (DigestAlgorithm.None));
			Assert.Throws<NotSupportedException> (() => SecureMimeContext.GetDigestOid (DigestAlgorithm.DoubleSha));
			Assert.Throws<NotSupportedException> (() => SecureMimeContext.GetDigestOid (DigestAlgorithm.Haval5160));
			Assert.Throws<NotSupportedException> (() => SecureMimeContext.GetDigestOid (DigestAlgorithm.Tiger192));

			using (var ctx = CreateContext ()) {
				var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret");
				var mailbox = new MailboxAddress ("Unit Tests", "example@mimekit.net");
				var recipients = new CmsRecipientCollection ();
				DigitalSignatureCollection signatures;
				MimeEntity entity;

				Assert.IsFalse (ctx.Supports ("text/plain"), "Should not support text/plain");
				Assert.IsFalse (ctx.Supports ("application/octet-stream"), "Should not support application/octet-stream");
				Assert.IsTrue (ctx.Supports ("application/pkcs7-mime"), "Should support application/pkcs7-mime");
				Assert.IsTrue (ctx.Supports ("application/x-pkcs7-mime"), "Should support application/x-pkcs7-mime");
				Assert.IsTrue (ctx.Supports ("application/pkcs7-signature"), "Should support application/pkcs7-signature");
				Assert.IsTrue (ctx.Supports ("application/x-pkcs7-signature"), "Should support application/x-pkcs7-signature");

				Assert.AreEqual ("application/pkcs7-signature", ctx.SignatureProtocol);
				Assert.AreEqual ("application/pkcs7-mime", ctx.EncryptionProtocol);
				Assert.AreEqual ("application/pkcs7-mime", ctx.KeyExchangeProtocol);

				Assert.Throws<ArgumentNullException> (() => ctx.Supports (null));
				Assert.Throws<ArgumentNullException> (() => ctx.CanSign (null));
				Assert.Throws<ArgumentNullException> (() => ctx.CanEncrypt (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Compress (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Decompress (null));
				Assert.Throws<ArgumentNullException> (() => ctx.DecompressTo (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.DecompressTo (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Decrypt (null));
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (signer, null));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (null, DigestAlgorithm.Sha256, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (mailbox, DigestAlgorithm.Sha256, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((CmsRecipientCollection) null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (recipients, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((IEnumerable<MailboxAddress>) null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (new MailboxAddress[0], null));
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (new MailboxAddress[0], stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Export (null));
				Assert.Throws<ArgumentNullException> (() => ctx.GetDigestAlgorithm (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null, "password"));
				Assert.Throws<ArgumentNullException> (() => ctx.Import (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((string) null, "password"));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ("fileName", null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((X509Crl) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((X509Certificate) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (signer, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (null, DigestAlgorithm.Sha256, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (mailbox, DigestAlgorithm.Sha256, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, out signatures));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, out entity));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await ctx.VerifyAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await ctx.VerifyAsync (stream, null));

				entity = new MimePart { Content = new MimeContent (stream) };

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((SecureMimeContext) null, signer, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, signer, null));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (signer, null));
			}
		}

		[Test]
		public virtual void TestCanSignAndEncrypt ()
		{
			var valid = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var invalid = new MailboxAddress ("Joe Nobody", "joe@nobody.com");

			using (var ctx = CreateContext ()) {
				Assert.IsFalse (ctx.CanSign (invalid), "{0} should not be able to sign.", invalid);
				Assert.IsFalse (ctx.CanEncrypt (invalid), "{0} should not be able to encrypt.", invalid);

				Assert.IsTrue (ctx.CanSign (valid), "{0} should be able to sign.", valid);
				Assert.IsTrue (ctx.CanEncrypt (valid), "{0} should be able to encrypt.", valid);

				using (var content = new MemoryStream ()) {
					Assert.Throws<CertificateNotFoundException> (() => ctx.Encrypt (new[] { invalid }, content));
					Assert.Throws<CertificateNotFoundException> (() => ctx.Sign (invalid, DigestAlgorithm.Sha1, content));
					Assert.Throws<CertificateNotFoundException> (() => ctx.EncapsulatedSign (invalid, DigestAlgorithm.Sha1, content));
				}
			}
		}

		[Test]
		public void TestDigestAlgorithmMappings ()
		{
			using (var ctx = CreateContext ()) {
				foreach (DigestAlgorithm digestAlgo in Enum.GetValues (typeof (DigestAlgorithm))) {
					if (digestAlgo == DigestAlgorithm.None ||
					    digestAlgo == DigestAlgorithm.DoubleSha)
						continue;

					// make sure that the name & enum values map back and forth correctly
					var micalg = ctx.GetDigestAlgorithmName (digestAlgo);
					var algorithm = ctx.GetDigestAlgorithm (micalg);
					Assert.AreEqual (digestAlgo, algorithm);

					// make sure that the oid and enum values map back and forth correctly
					try {
						var oid = SecureMimeContext.GetDigestOid (digestAlgo);
						SecureMimeContext.TryGetDigestAlgorithm (oid, out algorithm);
						Assert.AreEqual (digestAlgo, algorithm);
					} catch (NotSupportedException) {
					}
				}

				Assert.Throws<NotSupportedException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.DoubleSha));
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.None));

				Assert.AreEqual (DigestAlgorithm.None, ctx.GetDigestAlgorithm ("blahblahblah"));
				Assert.IsFalse (SecureMimeContext.TryGetDigestAlgorithm ("blahblahblah", out DigestAlgorithm algo));
			}
		}

		[Test]
		public void TestSecureMimeCompression ()
		{
			var original = new TextPart ("plain");
			original.Text = "This is some text that we'll end up compressing...";

			var compressed = ApplicationPkcs7Mime.Compress (original);

			Assert.AreEqual (SecureMimeType.CompressedData, compressed.SecureMimeType, "S/MIME type did not match.");

			var decompressed = compressed.Decompress ();

			Assert.IsInstanceOf<TextPart> (decompressed, "Decompressed part is not the expected type.");
			Assert.AreEqual (original.Text, ((TextPart) decompressed).Text, "Decompressed content is not the same as the original.");
		}

		[Test]
		public void TestSecureMimeCompressionWithContext ()
		{
			var original = new TextPart ("plain");
			original.Text = "This is some text that we'll end up compressing...";

			using (var ctx = CreateContext ()) {
				var compressed = ApplicationPkcs7Mime.Compress (ctx, original);

				Assert.AreEqual (SecureMimeType.CompressedData, compressed.SecureMimeType, "S/MIME type did not match.");

				var decompressed = compressed.Decompress (ctx);

				Assert.IsInstanceOf<TextPart> (decompressed, "Decompressed part is not the expected type.");
				Assert.AreEqual (original.Text, ((TextPart) decompressed).Text, "Decompressed content is not the same as the original.");

				using (var stream = new MemoryStream ()) {
					using (var decoded = new MemoryStream ()) {
						compressed.Content.DecodeTo (decoded);
						decoded.Position = 0;
						ctx.DecompressTo (decoded, stream);
					}

					stream.Position = 0;
					decompressed = MimeEntity.Load (stream);

					Assert.IsInstanceOf<TextPart> (decompressed, "Decompressed part is not the expected type.");
					Assert.AreEqual (original.Text, ((TextPart) decompressed).Text, "Decompressed content is not the same as the original.");
				}
			}
		}

		protected virtual EncryptionAlgorithm[] GetEncryptionAlgorithms (IDigitalSignature signature)
		{
			return ((SecureMimeDigitalSignature) signature).EncryptionAlgorithms;
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigning ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			var signed = ApplicationPkcs7Mime.Sign (self, DigestAlgorithm.Sha1, cleartext);
			MimeEntity extracted;

			Assert.AreEqual (SecureMimeType.SignedData, signed.SecureMimeType, "S/MIME type did not match.");

			var signatures = signed.Verify (out extracted);

			Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
			Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

			Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

			var signature = signatures[0];

			Assert.AreEqual ("MimeKit UnitTests", signature.SignerCertificate.Name);
			Assert.AreEqual ("mimekit@example.com", signature.SignerCertificate.Email);
			Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());
			Assert.AreEqual (MimeKitCreationDate, signature.SignerCertificate.CreationDate, "CreationDate");
			Assert.AreEqual (MimeKitExpirationDate, signature.SignerCertificate.ExpirationDate, "ExpirationDate");
			Assert.AreEqual (PublicKeyAlgorithm.RsaGeneral, signature.SignerCertificate.PublicKeyAlgorithm);
			Assert.AreEqual (PublicKeyAlgorithm.RsaGeneral, signature.PublicKeyAlgorithm);

			try {
				bool valid = signature.Verify ();

				Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
			} catch (DigitalSignatureVerifyException ex) {
				using (var ctx = CreateContext ()) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}

			var algorithms = GetEncryptionAlgorithms (signature);
			int i = 0;

			Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
			Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
			Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
		}

		void AssertValidSignatures (SecureMimeContext ctx, DigitalSignatureCollection signatures)
		{
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.AreEqual (EncryptionAlgorithm.Seed, algorithms[i++], "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[i++], "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[i++], "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[i++], "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[i++], "Expected Cast5 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[i++], "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[i++], "Expected IDEA capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[i++], "Expected RC2-128 capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[i++], "Expected RC2-64 capability");
				//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[i++], "Expected DES capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[i++], "Expected RC2-40 capability");
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithContext ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

			using (var ctx = CreateContext ()) {
				var signed = ApplicationPkcs7Mime.Sign (ctx, self, DigestAlgorithm.Sha1, cleartext);
				MimeEntity extracted;

				Assert.AreEqual (SecureMimeType.SignedData, signed.SecureMimeType, "S/MIME type did not match.");

				var signatures = signed.Verify (ctx, out extracted);

				Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				AssertValidSignatures (ctx, signatures);

				using (var signedData = signed.Content.Open ()) {
					using (var stream = ctx.Verify (signedData, out signatures))
						extracted = MimeEntity.Load (stream);

					Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
					Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

					Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithCmsSigner ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			var signed = ApplicationPkcs7Mime.Sign (signer, cleartext);
			MimeEntity extracted;

			Assert.AreEqual (SecureMimeType.SignedData, signed.SecureMimeType, "S/MIME type did not match.");

			var signatures = signed.Verify (out extracted);

			Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
			Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

			Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					using (var ctx = CreateContext ()) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
						} else {
							Assert.Fail ("Failed to verify signature: {0}", ex);
						}
					}
				}

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				var signed = ApplicationPkcs7Mime.Sign (ctx, signer, cleartext);
				MimeEntity extracted;

				Assert.AreEqual (SecureMimeType.SignedData, signed.SecureMimeType, "S/MIME type did not match.");

				var signatures = signed.Verify (ctx, out extracted);

				Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
				Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
				AssertValidSignatures (ctx, signatures);

				using (var signedData = signed.Content.Open ()) {
					using (var stream = ctx.Verify (signedData, out signatures))
						extracted = MimeEntity.Load (stream);

					Assert.IsInstanceOf<TextPart> (extracted, "Extracted part is not the expected type.");
					Assert.AreEqual (cleartext.Text, ((TextPart) extracted).Text, "Extracted content is not the same as the original.");

					Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithCmsSigner ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret");
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			var multipart = MultipartSigned.Create (signer, body);

			Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

			var protocol = multipart.ContentType.Parameters["protocol"];
			Assert.AreEqual ("application/pkcs7-signature", protocol, "The multipart/signed protocol does not match.");

			Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
			Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

			var signatures = multipart.Verify ();
			Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

			var signature = signatures[0];

			using (var ctx = CreateContext ()) {
				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual ("MimeKit UnitTests", signature.SignerCertificate.Name);
				Assert.AreEqual ("mimekit@example.com", signature.SignerCertificate.Email);
				Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithContextAndCmsSigner ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret");
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				var multipart = MultipartSigned.Create (ctx, signer, body);

				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual ("MimeKit UnitTests", signature.SignerCertificate.Name);
				Assert.AreEqual ("mimekit@example.com", signature.SignerCertificate.Email);
				Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.AreEqual (EncryptionAlgorithm.Seed, algorithms[i++], "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[i++], "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[i++], "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[i++], "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[i++], "Expected Cast5 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[i++], "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[i++], "Expected IDEA capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[i++], "Expected RC2-128 capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[i++], "Expected RC2-64 capability");
				//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[i++], "Expected DES capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[i++], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithRsaSsaPss ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret") {
				RsaSignaturePadding = RsaSignaturePadding.Pss
			};
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				MultipartSigned multipart;

				try {
					multipart = MultipartSigned.Create (signer, body);
				} catch (NotSupportedException) {
					if (!(ctx is WindowsSecureMimeContext))
						Assert.Fail ("RSASSA-PSS should be supported.");
					return;
				}

				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual ("application/pkcs7-signature", protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				var signatures = multipart.Verify ();
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual ("MimeKit UnitTests", signature.SignerCertificate.Name);
				Assert.AreEqual ("mimekit@example.com", signature.SignerCertificate.Email);
				Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeMessageSigning ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of signing with S/MIME" };

			message.From.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				Assert.IsTrue (ctx.CanSign (self));

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

				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual (self.Name, signature.SignerCertificate.Name);
				Assert.AreEqual (self.Address, signature.SignerCertificate.Email);
				Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.AreEqual (EncryptionAlgorithm.Seed, algorithms[i++], "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[i++], "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[i++], "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[i++], "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[i++], "Expected Cast5 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[i++], "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[i++], "Expected IDEA capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[i++], "Expected RC2-128 capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[i++], "Expected RC2-64 capability");
				//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[i++], "Expected DES capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[i++], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeMessageSigningAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of signing with S/MIME" };

			message.From.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				Assert.IsTrue (ctx.CanSign (self));

				message.Sign (ctx);

				Assert.IsInstanceOf<MultipartSigned> (message.Body, "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				Assert.AreEqual (2, multipart.Count, "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.AreEqual (ctx.SignatureProtocol, protocol, "The multipart/signed protocol does not match.");

				Assert.IsInstanceOf<TextPart> (multipart[0], "The first child is not a text part.");
				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				var signatures = await multipart.VerifyAsync (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual (self.Name, signature.SignerCertificate.Name);
				Assert.AreEqual (self.Address, signature.SignerCertificate.Email);
				Assert.AreEqual (MimeKitFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.AreEqual (EncryptionAlgorithm.Seed, algorithms[i++], "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[i++], "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[i++], "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[i++], "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[i++], "Expected Cast5 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[i++], "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[i++], "Expected IDEA capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[i++], "Expected RC2-128 capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[i++], "Expected RC2-64 capability");
				//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[i++], "Expected DES capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[i++], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeVerifyThunderbird ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-signed.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			using (var ctx = CreateContext ()) {
				Assert.IsInstanceOf<MultipartSigned> (message.Body, "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				var protocol = multipart.ContentType.Parameters["protocol"]?.Trim ();
				Assert.IsTrue (ctx.Supports (protocol), "The multipart/signed protocol is not supported.");

				Assert.IsInstanceOf<ApplicationPkcs7Signature> (multipart[1], "The second child is not a detached signature.");

				// Note: this fails in WindowsSecureMimeContext
				var signatures = multipart.Verify (ctx);
				Assert.AreEqual (1, signatures.Count, "Verify returned an unexpected number of signatures.");

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual (ThunderbirdName, signature.SignerCertificate.Name);
				Assert.AreEqual (sender.Address, signature.SignerCertificate.Email);
				Assert.AreEqual (ThunderbirdFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[0], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[1], "Expected AES-128 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[2], "Expected Triple-DES capability");
				Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[3], "Expected RC2-128 capability");
				Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[4], "Expected RC2-64 capability");
				Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[5], "Expected DES capability");
				Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[6], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Path.DirectorySeparatorChar == '/')
							Assert.IsInstanceOf<ArgumentException> (ex.InnerException);
						else
							Assert.AreEqual (ExpiredCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeMessageEncryption ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var message = new MimeMessage { Subject = "Test of encrypting with S/MIME" };

			message.From.Add (self);
			message.To.Add (self);
			message.Body = body;

			using (var ctx = CreateContext ()) {
				Assert.IsTrue (ctx.CanEncrypt (self));

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
		public virtual void TestSecureMimeEncryption ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
			var recipients = new CmsRecipientCollection ();

			recipients.Add (new CmsRecipient (MimeKitCertificate, SubjectIdentifierType.SubjectKeyIdentifier));

			var encrypted = ApplicationPkcs7Mime.Encrypt (recipients, body);

			Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

			var decrypted = encrypted.Decrypt ();

			Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
			Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
		}

		[Test]
		public virtual void TestSecureMimeEncryptionWithContext ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				var recipients = new CmsRecipientCollection ();

				if (ctx is WindowsSecureMimeContext)
					recipients.Add (new CmsRecipient (MimeKitCertificate.AsX509Certificate2 (), SubjectIdentifierType.SubjectKeyIdentifier));
				else
					recipients.Add (new CmsRecipient (MimeKitCertificate, SubjectIdentifierType.SubjectKeyIdentifier));

				var encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, body);

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

				using (var stream = new MemoryStream ()) {
					ctx.DecryptTo (encrypted.Content.Open (), stream);
					stream.Position = 0;

					var decrypted = MimeEntity.Load (stream);

					Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
					Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeEncryptionWithAlgorithm ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				var recipients = new CmsRecipientCollection ();

				if (ctx is WindowsSecureMimeContext)
					recipients.Add (new CmsRecipient (MimeKitCertificate.AsX509Certificate2 (), SubjectIdentifierType.SubjectKeyIdentifier));
				else
					recipients.Add (new CmsRecipient (MimeKitCertificate, SubjectIdentifierType.SubjectKeyIdentifier));

				foreach (EncryptionAlgorithm algorithm in Enum.GetValues (typeof (EncryptionAlgorithm))) {
					foreach (var recipient in recipients)
						recipient.EncryptionAlgorithms = new EncryptionAlgorithm[] { algorithm };

					var enabled = ctx.IsEnabled (algorithm);
					ApplicationPkcs7Mime encrypted;

					// Note: these are considered weaker than 3DES and so we need to disable 3DES to use them
					switch (algorithm) {
					case EncryptionAlgorithm.Idea:
					case EncryptionAlgorithm.Des:
					case EncryptionAlgorithm.RC2128:
					case EncryptionAlgorithm.RC264:
					case EncryptionAlgorithm.RC240:
						ctx.Disable (EncryptionAlgorithm.TripleDes);
						break;
					}

					if (!enabled) {
						// make sure the algorithm is enabled
						ctx.Enable (algorithm);
					}

					try {
						encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, body);
					} catch (NotSupportedException ex) {
						if (ctx is WindowsSecureMimeContext) {
							switch (algorithm) {
							case EncryptionAlgorithm.Camellia128:
							case EncryptionAlgorithm.Camellia192:
							case EncryptionAlgorithm.Camellia256:
							case EncryptionAlgorithm.Blowfish:
							case EncryptionAlgorithm.Twofish:
							case EncryptionAlgorithm.Cast5:
							case EncryptionAlgorithm.Idea:
							case EncryptionAlgorithm.Seed:
								break;
							default:
								Assert.Fail ("{0} does not support {1}: {2}", ctx.GetType ().Name, algorithm, ex.Message);
								break;
							}
						} else {
							if (algorithm != EncryptionAlgorithm.Twofish)
								Assert.Fail ("{0} does not support {1}: {2}", ctx.GetType ().Name, algorithm, ex.Message);
						}
						continue;
					} catch (Exception ex) {
						Assert.Fail ("{0} does not support {1}: {2}", ctx.GetType ().Name, algorithm, ex.Message);
						continue;
					} finally {
						if (!enabled) {
							ctx.Enable (EncryptionAlgorithm.TripleDes);
							ctx.Disable (algorithm);
						}
					}

					Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

					using (var stream = new MemoryStream ()) {
						ctx.DecryptTo (encrypted.Content.Open (), stream);
						stream.Position = 0;

						var decrypted = MimeEntity.Load (stream);

						Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
						Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
					}
				}
			}
		}

		[TestCase (DigestAlgorithm.Sha1)]
		[TestCase (DigestAlgorithm.Sha256)]
		[TestCase (DigestAlgorithm.Sha384)]
		[TestCase (DigestAlgorithm.Sha512)]
		public virtual void TestSecureMimeEncryptionWithRsaesOaep (DigestAlgorithm hashAlgorithm)
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				var recipients = new CmsRecipientCollection ();

				var recipient = new CmsRecipient (MimeKitCertificate, SubjectIdentifierType.IssuerAndSerialNumber);
				recipient.EncryptionAlgorithms = new EncryptionAlgorithm[] { EncryptionAlgorithm.Aes128 };
				recipient.RsaEncryptionPadding = RsaEncryptionPadding.CreateOaep (hashAlgorithm);
				recipients.Add (recipient);

				ApplicationPkcs7Mime encrypted;

				try {
					encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, body);
				} catch (NotSupportedException) {
					if (!(ctx is WindowsSecureMimeContext))
						Assert.Fail ("RSAES-OAEP should be supported.");
					return;
				}

				Assert.AreEqual (SecureMimeType.EnvelopedData, encrypted.SecureMimeType, "S/MIME type did not match.");

				using (var stream = new MemoryStream ()) {
					ctx.DecryptTo (encrypted.Content.Open (), stream);
					stream.Position = 0;

					var decrypted = MimeEntity.Load (stream);

					Assert.IsInstanceOf<TextPart> (decrypted, "Decrypted part is not the expected type.");
					Assert.AreEqual (body.Text, ((TextPart) decrypted).Text, "Decrypted content is not the same as the original.");
				}
			}
		}

		[Test]
		public void TestSecureMimeDecryptThunderbird ()
		{
			var p12 = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-encrypted.txt"))) {
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
		public virtual void TestSecureMimeSignAndEncrypt ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
			var self = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", MimeKitFingerprint);
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

				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual (self.Name, signature.SignerCertificate.Name);
				Assert.AreEqual (self.Address, signature.SignerCertificate.Email);
				Assert.AreEqual (self.Fingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[i++], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes192, algorithms[i++], "Expected AES-192 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[i++], "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.AreEqual (EncryptionAlgorithm.Seed, algorithms[i++], "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.AreEqual (EncryptionAlgorithm.Camellia256, algorithms[i++], "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.AreEqual (EncryptionAlgorithm.Camellia192, algorithms[i++], "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.AreEqual (EncryptionAlgorithm.Camellia128, algorithms[i++], "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.AreEqual (EncryptionAlgorithm.Cast5, algorithms[i++], "Expected Cast5 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[i++], "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.AreEqual (EncryptionAlgorithm.Idea, algorithms[i++], "Expected IDEA capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[i++], "Expected RC2-128 capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[i++], "Expected RC2-64 capability");
				//Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[i++], "Expected DES capability");
				//Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[i++], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.AreEqual (UntrustedRootCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public void TestSecureMimeDecryptVerifyThunderbird ()
		{
			var p12 = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-signed-encrypted.txt"))) {
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

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (!(ctx is WindowsSecureMimeContext) || Path.DirectorySeparatorChar == '\\')
					Assert.AreEqual (ThunderbirdName, signature.SignerCertificate.Name);
				Assert.AreEqual (sender.Address, signature.SignerCertificate.Email);
				Assert.AreEqual (ThunderbirdFingerprint, signature.SignerCertificate.Fingerprint.ToLowerInvariant ());

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.AreEqual (EncryptionAlgorithm.Aes256, algorithms[0], "Expected AES-256 capability");
				Assert.AreEqual (EncryptionAlgorithm.Aes128, algorithms[1], "Expected AES-128 capability");
				Assert.AreEqual (EncryptionAlgorithm.TripleDes, algorithms[2], "Expected Triple-DES capability");
				Assert.AreEqual (EncryptionAlgorithm.RC2128, algorithms[3], "Expected RC2-128 capability");
				Assert.AreEqual (EncryptionAlgorithm.RC264, algorithms[4], "Expected RC2-64 capability");
				Assert.AreEqual (EncryptionAlgorithm.Des, algorithms[5], "Expected DES capability");
				Assert.AreEqual (EncryptionAlgorithm.RC240, algorithms[6], "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Path.DirectorySeparatorChar == '/')
							Assert.IsInstanceOf<ArgumentException> (ex.InnerException);
						else
							Assert.AreEqual (ExpiredCertificateMessage, ex.InnerException.Message);
					} else {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeImportExport ()
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

				using (var imported = new TemporarySecureMimeContext ()) {
					pkcs7mime.Import (imported);

					Assert.AreEqual (1, imported.certificates.Count, "Unexpected number of imported certificates.");
					Assert.IsFalse (imported.keys.Count > 0, "One or more of the certificates included the private key.");
				}
			}
		}

		[Test]
		public void TestSecureMimeVerifyMixedLineEndings ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "octet-stream-with-mixed-line-endings.dat"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = parser.ParseMessage ();
			}

			Assert.IsInstanceOf<MultipartSigned> (message.Body, "THe message body is not multipart/signed as expected.");

			var signed = (MultipartSigned) message.Body;

			using (var ctx = CreateContext ()) {
				foreach (var signature in signed.Verify (ctx)) {
					try {
						bool valid = signature.Verify (true);

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}

		[Test]
		public async Task TestSecureMimeVerifyMixedLineEndingsAsync ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "octet-stream-with-mixed-line-endings.dat"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = await parser.ParseMessageAsync ();
			}

			Assert.IsInstanceOf<MultipartSigned> (message.Body, "THe message body is not multipart/signed as expected.");

			var signed = (MultipartSigned) message.Body;

			using (var ctx = CreateContext ()) {
				foreach (var signature in await signed.VerifyAsync (ctx)) {
					try {
						bool valid = signature.Verify (true);

						Assert.IsTrue (valid, "Bad signature from {0}", signature.SignerCertificate.Email);
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ("Failed to verify signature: {0}", ex);
					}
				}
			}
		}
	}

	[TestFixture]
	public class SecureMimeTests : SecureMimeTestsBase
	{
		readonly TemporarySecureMimeContext ctx = new TemporarySecureMimeContext { CheckCertificateRevocation = true };

		protected override SecureMimeContext CreateContext ()
		{
			return ctx;
		}
	}

	[TestFixture]
	public class SecureMimeSqliteTests : SecureMimeTestsBase
	{
		class MySecureMimeContext : DefaultSecureMimeContext
		{
			public MySecureMimeContext () : base ("smime.db", "no.secret")
			{
				CheckCertificateRevocation = true;
			}
		}

		protected override SecureMimeContext CreateContext ()
		{
			return new MySecureMimeContext ();
		}

		static SecureMimeSqliteTests ()
		{
			if (File.Exists ("smime.db"))
				File.Delete ("smime.db");
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

	[TestFixture]
	public class WindowsSecureMimeTests : SecureMimeTestsBase
	{
		protected override bool IsEnabled {
			get {
				return Path.DirectorySeparatorChar == '\\';
			}
		}

		protected override SecureMimeContext CreateContext ()
		{
			return new WindowsSecureMimeContext ();
		}

		protected override EncryptionAlgorithm[] GetEncryptionAlgorithms (IDigitalSignature signature)
		{
			return ((WindowsSecureMimeDigitalSignature) signature).EncryptionAlgorithms;
		}

		[Test]
		public override void TestCanSignAndEncrypt ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestCanSignAndEncrypt ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigning ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncapsulatedSigning ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithContext ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncapsulatedSigningWithContext ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithCmsSigner ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncapsulatedSigningWithCmsSigner ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ();
		}

		[Test]
		public override void TestSecureMimeSigningWithCmsSigner ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeSigningWithCmsSigner ();
		}

		[Test]
		public override void TestSecureMimeSigningWithContextAndCmsSigner ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeSigningWithContextAndCmsSigner ();
		}

		[Test]
		public override void TestSecureMimeSigningWithRsaSsaPss ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeSigningWithRsaSsaPss ();
		}

		[Test]
		public override void TestSecureMimeMessageSigning ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeMessageSigning ();
		}

		[Test]
		public override async Task TestSecureMimeMessageSigningAsync ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			await base.TestSecureMimeMessageSigningAsync ();
		}

		[Test]
		public override void TestSecureMimeVerifyThunderbird ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeVerifyThunderbird ();
		}

		[Test]
		public override void TestSecureMimeMessageEncryption ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeMessageEncryption ();
		}

		[Test]
		public override void TestSecureMimeEncryption ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncryption ();
		}

		[Test]
		public override void TestSecureMimeEncryptionWithContext ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncryptionWithContext ();
		}

		[Test]
		public override void TestSecureMimeEncryptionWithAlgorithm ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncryptionWithAlgorithm ();
		}

		[TestCase (DigestAlgorithm.Sha1)]
		[TestCase (DigestAlgorithm.Sha256)]
		[TestCase (DigestAlgorithm.Sha384)]
		[TestCase (DigestAlgorithm.Sha512)]
		public override void TestSecureMimeEncryptionWithRsaesOaep (DigestAlgorithm hashAlgorithm)
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeEncryptionWithRsaesOaep (hashAlgorithm);
		}

		[Test]
		public override void TestSecureMimeSignAndEncrypt ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeSignAndEncrypt ();
		}

		[Test]
		public override void TestSecureMimeImportExport ()
		{
			if (Path.DirectorySeparatorChar != '\\')
				return;

			base.TestSecureMimeImportExport ();
		}
	}
}
