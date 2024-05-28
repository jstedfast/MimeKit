//
// SecureMimeTests.cs
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

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;

using MimeKit;
using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	public class SMimeCertificate
	{
		public string FileName { get; private set; }
		public X509Certificate Certificate { get { return Chain[0]; } }
		public X509Certificate[] Chain { get; private set; }
		public DateTime CreationDate { get { return Certificate.NotBefore; } }
		public DateTime ExpirationDate { get { return Certificate.NotAfter; } }
		public string EmailAddress { get { return Certificate.GetSubjectEmailAddress (); } }
		public string Fingerprint { get; private set; }
		public PublicKeyAlgorithm PublicKeyAlgorithm { get { return Certificate.GetPublicKeyAlgorithm (); } }

		public SMimeCertificate (string fileName, X509Certificate[] chain)
		{
			FileName = fileName;
			Chain = chain;
			Fingerprint = chain[0].GetFingerprint ();
		}
	}

	public abstract class SecureMimeTestsBase
	{
		//const string ExpiredCertificateMessage = "A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.\r\n";
#if NET5_0_OR_GREATER
		public const string ExpiredCertificateMessage = "Certificate trust could not be established. The first reported error is: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.";
		public const string UntrustedRootCertificateMessage = "Certificate trust could not be established. The first reported error is: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.";
#else
		public const string ExpiredCertificateMessage = "The certificate is revoked.\r\n";
		public const string UntrustedRootCertificateMessage = "A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.\r\n";
#endif
		public const string ThunderbirdFingerprint = "354ea4dcf98166639b58ec5df06a65de0cd8a95c";
		public const string MimeKitFingerprint = "ba4403cd3d876ae8cd261575820330086cc3cbc8";
		public const string ThunderbirdName = "fejj@gnome.org";

		public static readonly string[] StartComCertificates = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		public static readonly SMimeCertificate[] UnsupportedCertificates;
		public static readonly SMimeCertificate[] SupportedCertificates;
		public static readonly SMimeCertificate[] SMimeCertificates;

		protected virtual bool IsEnabled { get { return true; } }

		protected virtual bool Supports (PublicKeyAlgorithm algorithm)
		{
			switch (algorithm) {
			case PublicKeyAlgorithm.RsaGeneral:
			case PublicKeyAlgorithm.EllipticCurve:
				return true;
			default:
				return false;
			}
		}

		protected abstract SecureMimeContext CreateContext ();

		static SecureMimeTestsBase ()
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var unsupported = new List<SMimeCertificate> ();
			var supported = new List<SMimeCertificate> ();
			var all = new List<SMimeCertificate> ();

			foreach (var cfg in Directory.GetFiles (dataDir, "*.cfg", SearchOption.AllDirectories)) {
				var name = Path.GetFileNameWithoutExtension (cfg);
				var pfx = Path.ChangeExtension (cfg, ".pfx");
				X509Certificate[] chain;

				if (File.Exists (pfx)) {
					chain = LoadPkcs12CertificateChain (pfx, "no.secret");
					var certificate = chain[0];

					if (certificate.NotAfter > DateTime.Now) {
						if (name.Equals ("smime", StringComparison.OrdinalIgnoreCase)) {
							var smime = new SMimeCertificate (pfx, chain);

							switch (smime.PublicKeyAlgorithm) {
							case PublicKeyAlgorithm.RsaGeneral:
							case PublicKeyAlgorithm.EllipticCurve:
								supported.Add (smime);
								break;
							default:
								unsupported.Add (smime);
								break;
							}

							all.Add (smime);
						}
						continue;
					}
				}

				// The pfx file either doesn't exist or it has expired. Time to generate a new one.
				chain = X509CertificateGenerator.Generate (cfg);

				if (name.Equals ("smime", StringComparison.OrdinalIgnoreCase)) {
					var smime = new SMimeCertificate (pfx, chain);

					switch (smime.PublicKeyAlgorithm) {
					case PublicKeyAlgorithm.RsaGeneral:
					case PublicKeyAlgorithm.EllipticCurve:
						supported.Add (smime);
						break;
					default:
						unsupported.Add (smime);
						break;
					}

					all.Add (smime);
				}
			}

			UnsupportedCertificates = unsupported.ToArray ();
			SupportedCertificates = supported.ToArray ();
			SMimeCertificates = all.ToArray ();
		}

		protected void ImportTestCertificates (SecureMimeContext ctx)
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var windows = ctx as WindowsSecureMimeContext;

			if (ctx is TemporarySecureMimeContext)
				CryptographyContext.Register (CreateContext);
			else
				CryptographyContext.Register (ctx.GetType ());

			// Import the StartCom certificates
			if (windows is not null) {
				var parser = new X509CertificateParser ();

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComCertificationAuthority.crt"))) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.AuthRoot, certificate);
				}

				using (var stream = File.OpenRead (Path.Combine (dataDir, "StartComClass1PrimaryIntermediateClientCA.crt"))) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream))
						windows.Import (StoreName.CertificateAuthority, certificate);
				}
			} else {
				foreach (var filename in StartComCertificates) {
					var path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path)) {
						if (ctx is DefaultSecureMimeContext sqlite) {
							sqlite.Import (stream, true);
						} else {
							var parser = new X509CertificateParser ();
							foreach (X509Certificate certificate in parser.ReadCertificates (stream))
								ctx.Import (certificate);
						}
					}
				}
			}

			// Import the smime.pfx certificates
			foreach (var mimekitCertificate in SupportedCertificates) {
				var chain = mimekitCertificate.Chain;

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

				// Import the pfx so that the SecureMimeContext has a copy of the private key as well.
				ctx.Import (mimekitCertificate.FileName, "no.secret");

				// Import a second time to cover the case where the certificate & private key already exist
				Assert.DoesNotThrow (() => ctx.Import (mimekitCertificate.FileName, "no.secret"));
			}
		}

		protected SecureMimeTestsBase ()
		{
			if (!IsEnabled)
				return;

			using (var ctx = CreateContext ())
				ImportTestCertificates (ctx);
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
				var pkcs12 = new Pkcs12StoreBuilder ().Build ();
				pkcs12.Load (stream, password.ToCharArray ());

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

				return Array.Empty<X509Certificate> ();
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

			Assert.Throws<ArgumentNullException> (() => new TemporarySecureMimeContext ((SecureRandom) null));

			using (var ctx = CreateContext ()) {
				var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "rsa", "smime.pfx"), "no.secret");
				var mailbox = new MailboxAddress ("Unit Tests", "example@mimekit.net");
				var recipients = new CmsRecipientCollection ();
				DigitalSignatureCollection signatures;
				MimeEntity entity;

				Assert.That (ctx.Supports ("text/plain"), Is.False, "Should not support text/plain");
				Assert.That (ctx.Supports ("application/octet-stream"), Is.False, "Should not support application/octet-stream");
				Assert.That (ctx.Supports ("application/pkcs7-mime"), Is.True, "Should support application/pkcs7-mime");
				Assert.That (ctx.Supports ("application/x-pkcs7-mime"), Is.True, "Should support application/x-pkcs7-mime");
				Assert.That (ctx.Supports ("application/pkcs7-signature"), Is.True, "Should support application/pkcs7-signature");
				Assert.That (ctx.Supports ("application/x-pkcs7-signature"), Is.True, "Should support application/x-pkcs7-signature");

				Assert.That (ctx.SignatureProtocol, Is.EqualTo ("application/pkcs7-signature"));
				Assert.That (ctx.EncryptionProtocol, Is.EqualTo ("application/pkcs7-mime"));
				Assert.That (ctx.KeyExchangeProtocol, Is.EqualTo ("application/pkcs7-mime"));

				Assert.Throws<ArgumentNullException> (() => ctx.Supports (null));
				Assert.Throws<ArgumentNullException> (() => ctx.CanSign (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.CanSignAsync (null));
				Assert.Throws<ArgumentNullException> (() => ctx.CanEncrypt (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.CanEncryptAsync (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Compress (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Decompress (null));
				Assert.Throws<ArgumentNullException> (() => ctx.DecompressTo (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.DecompressTo (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Decrypt (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.DecryptAsync (null));
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.DecryptTo (stream, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.DecryptToAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.DecryptToAsync (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (signer, null));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (null, DigestAlgorithm.Sha256, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.EncapsulatedSign (mailbox, DigestAlgorithm.Sha256, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncapsulatedSignAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncapsulatedSignAsync (signer, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncapsulatedSignAsync (null, DigestAlgorithm.Sha256, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncapsulatedSignAsync (mailbox, DigestAlgorithm.Sha256, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((CmsRecipientCollection) null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (recipients, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt ((IEnumerable<MailboxAddress>) null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Encrypt (Array.Empty<MailboxAddress> (), null));
				Assert.Throws<ArgumentException> (() => ctx.Encrypt (Array.Empty<MailboxAddress> (), stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync ((CmsRecipientCollection) null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (recipients, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync ((IEnumerable<MailboxAddress>) null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.EncryptAsync (Array.Empty<MailboxAddress> (), null));
				Assert.ThrowsAsync<ArgumentException> (() => ctx.EncryptAsync (Array.Empty<MailboxAddress> (), stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Export (null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ExportAsync (null));
				Assert.Throws<ArgumentNullException> (() => ctx.GetDigestAlgorithm (null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null, "password"));
				Assert.Throws<ArgumentNullException> (() => ctx.Import (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((string) null, "password"));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ("fileName", null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((X509Crl) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((X509Certificate) null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((Stream) null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((Stream) null, "password"));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync (stream, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((string) null, "password"));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ("fileName", null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((X509Crl) null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.ImportAsync ((X509Certificate) null));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (signer, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (null, DigestAlgorithm.Sha256, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Sign (mailbox, DigestAlgorithm.Sha256, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (signer, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (null, DigestAlgorithm.Sha256, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.SignAsync (mailbox, DigestAlgorithm.Sha256, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, stream));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (stream, null));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, out signatures));
				Assert.Throws<ArgumentNullException> (() => ctx.Verify (null, out entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.VerifyAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (() => ctx.VerifyAsync (stream, null));

				entity = new MimePart { Content = new MimeContent (stream) };

				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((SecureMimeContext) null, signer, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, (CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (ctx, signer, null));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create ((CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => MultipartSigned.Create (signer, null));

				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync ((SecureMimeContext) null, signer, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, (CmsSigner) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (ctx, signer, null));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync ((CmsSigner) null, entity));
				Assert.ThrowsAsync<ArgumentNullException> (() => MultipartSigned.CreateAsync (signer, null));
			}
		}

		[Test]
		public virtual void TestCanSignAndEncrypt ()
		{
			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					var valid = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var invalid = new MailboxAddress ("Joe Nobody", "joe@nobody.com");

					Assert.That (ctx.CanSign (invalid), Is.False, $"{invalid} should not be able to sign.");
					Assert.That (ctx.CanEncrypt (invalid), Is.False, $"{invalid} should not be able to encrypt.");

					Assert.That (ctx.CanSign (valid), Is.True, $"{valid} should be able to sign.");
					Assert.That (ctx.CanEncrypt (valid), Is.True, $"{valid} should be able to encrypt.");

					using (var content = new MemoryStream ()) {
						Assert.Throws<CertificateNotFoundException> (() => ctx.Encrypt (new[] { invalid }, content));
						Assert.Throws<CertificateNotFoundException> (() => ctx.Sign (invalid, DigestAlgorithm.Sha1, content));
						Assert.Throws<CertificateNotFoundException> (() => ctx.EncapsulatedSign (invalid, DigestAlgorithm.Sha1, content));
					}
				}
			}
		}

		[Test]
		public virtual async Task TestCanSignAndEncryptAsync ()
		{
			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					var valid = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var invalid = new MailboxAddress ("Joe Nobody", "joe@nobody.com");

					Assert.That (await ctx.CanSignAsync (invalid), Is.False, $"{invalid} should not be able to sign.");
					Assert.That (await ctx.CanEncryptAsync (invalid), Is.False, $"{invalid} should not be able to encrypt.");

					Assert.That (await ctx.CanSignAsync (valid), Is.True, $"{valid} should be able to sign.");
					Assert.That (await ctx.CanEncryptAsync (valid), Is.True, $"{valid} should be able to encrypt.");

					using (var content = new MemoryStream ()) {
						Assert.ThrowsAsync<CertificateNotFoundException> (() => ctx.EncryptAsync (new[] { invalid }, content));
						Assert.ThrowsAsync<CertificateNotFoundException> (() => ctx.SignAsync (invalid, DigestAlgorithm.Sha1, content));
						Assert.ThrowsAsync<CertificateNotFoundException> (() => ctx.EncapsulatedSignAsync (invalid, DigestAlgorithm.Sha1, content));
					}
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
					Assert.That (algorithm, Is.EqualTo (digestAlgo));

					// make sure that the oid and enum values map back and forth correctly
					try {
						var oid = SecureMimeContext.GetDigestOid (digestAlgo);
						SecureMimeContext.TryGetDigestAlgorithm (oid, out algorithm);
						Assert.That (algorithm, Is.EqualTo (digestAlgo));
					} catch (NotSupportedException) {
					}
				}

				Assert.Throws<NotSupportedException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.DoubleSha));
				Assert.Throws<ArgumentOutOfRangeException> (() => ctx.GetDigestAlgorithmName (DigestAlgorithm.None));

				Assert.That (ctx.GetDigestAlgorithm ("blahblahblah"), Is.EqualTo (DigestAlgorithm.None));
				Assert.That (SecureMimeContext.TryGetDigestAlgorithm ("blahblahblah", out DigestAlgorithm algo), Is.False);
			}
		}

		[Test]
		public void TestSecureMimeCompression ()
		{
			var original = new TextPart ("plain") {
				Text = "This is some text that we'll end up compressing..."
			};

			var compressed = ApplicationPkcs7Mime.Compress (original);

			Assert.That (compressed.SecureMimeType, Is.EqualTo (SecureMimeType.CompressedData), "S/MIME type did not match.");

			var decompressed = compressed.Decompress ();

			Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
			Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");
		}

		[Test]
		public async Task TestSecureMimeCompressionAsync ()
		{
			var original = new TextPart ("plain") {
				Text = "This is some text that we'll end up compressing..."
			};

			var compressed = await ApplicationPkcs7Mime.CompressAsync (original);

			Assert.That (compressed.SecureMimeType, Is.EqualTo (SecureMimeType.CompressedData), "S/MIME type did not match.");

			var decompressed = await compressed.DecompressAsync ();

			Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
			Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");
		}

		[Test]
		public void TestSecureMimeCompressionWithContext ()
		{
			var original = new TextPart ("plain") {
				Text = "This is some text that we'll end up compressing..."
			};

			using (var ctx = CreateContext ()) {
				var compressed = ApplicationPkcs7Mime.Compress (ctx, original);

				Assert.That (compressed.SecureMimeType, Is.EqualTo (SecureMimeType.CompressedData), "S/MIME type did not match.");

				var decompressed = compressed.Decompress (ctx);

				Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
				Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");

				using (var stream = new MemoryStream ()) {
					using (var decoded = new MemoryStream ()) {
						compressed.Content.DecodeTo (decoded);
						decoded.Position = 0;
						ctx.DecompressTo (decoded, stream);
					}

					stream.Position = 0;
					decompressed = MimeEntity.Load (stream);

					Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
					Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");
				}
			}
		}

		[Test]
		public async Task TestSecureMimeCompressionWithContextAsync ()
		{
			var original = new TextPart ("plain") {
				Text = "This is some text that we'll end up compressing..."
			};

			using (var ctx = CreateContext ()) {
				var compressed = await ApplicationPkcs7Mime.CompressAsync (ctx, original);

				Assert.That (compressed.SecureMimeType, Is.EqualTo (SecureMimeType.CompressedData), "S/MIME type did not match.");

				var decompressed = await compressed.DecompressAsync (ctx);

				Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
				Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");

				using (var stream = new MemoryStream ()) {
					using (var decoded = new MemoryStream ()) {
						await compressed.Content.DecodeToAsync (decoded);
						decoded.Position = 0;
						await ctx.DecompressToAsync (decoded, stream);
					}

					stream.Position = 0;
					decompressed = await MimeEntity.LoadAsync (stream);

					Assert.That (decompressed, Is.InstanceOf<TextPart> (), "Decompressed part is not the expected type.");
					Assert.That (((TextPart) decompressed).Text, Is.EqualTo (original.Text), "Decompressed content is not the same as the original.");
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

			foreach (var certificate in SupportedCertificates) {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT && certificate.PublicKeyAlgorithm == PublicKeyAlgorithm.EllipticCurve)
					continue;

				var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var signed = ApplicationPkcs7Mime.Sign (self, DigestAlgorithm.Sha1, cleartext);

				Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

				var signatures = signed.Verify (out var extracted);

				Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
				Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

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
					using (var ctx = CreateContext ()) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
			}
		}

		[Test]
		public virtual async Task TestSecureMimeEncapsulatedSigningAsync ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			foreach (var certificate in SupportedCertificates) {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT && certificate.PublicKeyAlgorithm == PublicKeyAlgorithm.EllipticCurve)
					continue;

				var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var signed = await ApplicationPkcs7Mime.SignAsync (self, DigestAlgorithm.Sha1, cleartext);
				MimeEntity extracted;

				Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

				var signatures = signed.Verify (out extracted);

				Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
				Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

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
					using (var ctx = CreateContext ()) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
			}
		}

		void AssertValidSignatures (SecureMimeContext ctx, DigitalSignatureCollection signatures)
		{
			foreach (var signature in signatures) {
				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
				if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
				//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
				//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
				//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
				//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithContext ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var signed = ApplicationPkcs7Mime.Sign (ctx, self, DigestAlgorithm.Sha1, cleartext);
					MimeEntity extracted;

					Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

					var signatures = signed.Verify (ctx, out extracted);

					Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
					Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);

					using (var signedData = signed.Content.Open ()) {
						using (var stream = ctx.Verify (signedData, out signatures))
							extracted = MimeEntity.Load (stream);

						Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
						Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

						Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
						AssertValidSignatures (ctx, signatures);
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeEncapsulatedSigningWithContextAsync ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var signed = await ApplicationPkcs7Mime.SignAsync (ctx, self, DigestAlgorithm.Sha1, cleartext);
					MimeEntity extracted;

					Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

					var signatures = signed.Verify (ctx, out extracted);

					Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
					Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);

					using (var signedData = signed.Content.Open ()) {
						using (var stream = ctx.Verify (signedData, out signatures))
							extracted = await MimeEntity.LoadAsync (stream);

						Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
						Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

						Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
						AssertValidSignatures (ctx, signatures);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithCmsSigner ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var signer = new CmsSigner (certificate.FileName, "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
				var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

				var signed = ApplicationPkcs7Mime.Sign (signer, cleartext);
				MimeEntity extracted;

				Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

				var signatures = signed.Verify (out extracted);

				Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
				Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						using (var ctx = CreateContext ()) {
							if (ctx is WindowsSecureMimeContext) {
								// AppVeyor gets an exception about the root certificate not being trusted
								Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
							} else {
								Assert.Fail ($"Failed to verify signature: {ex}");
							}
						}
					}

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeEncapsulatedSigningWithCmsSignerAsync ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var signer = new CmsSigner (certificate.FileName, "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
				var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

				var signed = await ApplicationPkcs7Mime.SignAsync (signer, cleartext);
				MimeEntity extracted;

				Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

				var signatures = signed.Verify (out extracted);

				Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
				Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
				foreach (var signature in signatures) {
					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						using (var ctx = CreateContext ()) {
							if (ctx is WindowsSecureMimeContext) {
								// AppVeyor gets an exception about the root certificate not being trusted
								Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
							} else {
								Assert.Fail ($"Failed to verify signature: {ex}");
							}
						}
					}

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var signer = new CmsSigner (certificate.FileName, "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
					var signed = ApplicationPkcs7Mime.Sign (ctx, signer, cleartext);
					MimeEntity extracted;

					Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

					var signatures = signed.Verify (ctx, out extracted);

					Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
					Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);

					using (var signedData = signed.Content.Open ()) {
						using (var stream = ctx.Verify (signedData, out signatures))
							extracted = MimeEntity.Load (stream);

						Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
						Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

						Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
						AssertValidSignatures (ctx, signatures);
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeEncapsulatedSigningWithContextAndCmsSignerAsync ()
		{
			var cleartext = new TextPart ("plain") { Text = "This is some text that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var signer = new CmsSigner (certificate.FileName, "no.secret", SubjectIdentifierType.SubjectKeyIdentifier);
					var signed = await ApplicationPkcs7Mime.SignAsync (ctx, signer, cleartext);
					MimeEntity extracted;

					Assert.That (signed.SecureMimeType, Is.EqualTo (SecureMimeType.SignedData), "S/MIME type did not match.");

					var signatures = signed.Verify (ctx, out extracted);

					Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
					Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
					AssertValidSignatures (ctx, signatures);

					using (var signedData = signed.Content.Open ()) {
						using (var stream = ctx.Verify (signedData, out signatures))
							extracted = await MimeEntity.LoadAsync (stream);

						Assert.That (extracted, Is.InstanceOf<TextPart> (), "Extracted part is not the expected type.");
						Assert.That (((TextPart) extracted).Text, Is.EqualTo (cleartext.Text), "Extracted content is not the same as the original.");

						Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");
						AssertValidSignatures (ctx, signatures);
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithCmsSigner ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var signer = new CmsSigner (certificate.FileName, "no.secret");
				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

				var multipart = MultipartSigned.Create (signer, body);

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo ("application/pkcs7-signature"), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				var signatures = multipart.Verify ();
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				using (var ctx = CreateContext ()) {
					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeSigningWithCmsSignerAsync ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var signer = new CmsSigner (certificate.FileName, "no.secret");
				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

				var multipart = await MultipartSigned.CreateAsync (signer, body);

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo ("application/pkcs7-signature"), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				var signatures = await multipart.VerifyAsync ();
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				using (var ctx = CreateContext ()) {
					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithContextAndCmsSigner ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var signer = new CmsSigner (certificate.FileName, "no.secret");
					var multipart = MultipartSigned.Create (ctx, signer, body);

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

					var signatures = multipart.Verify (ctx);
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeSigningWithContextAndCmsSignerAsync ()
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var signer = new CmsSigner (certificate.FileName, "no.secret");
					var multipart = await MultipartSigned.CreateAsync (ctx, signer, body);

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

					var signatures = await multipart.VerifyAsync (ctx);
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (certificate.EmailAddress));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeSigningWithRsaSsaPss ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "rsa", "smime.pfx"), "no.secret") {
				RsaSignaturePadding = RsaSignaturePadding.Pss
			};
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				MultipartSigned multipart;

				try {
					multipart = MultipartSigned.Create (signer, body);
				} catch (NotSupportedException) {
					if (ctx is not WindowsSecureMimeContext)
						Assert.Fail ("RSASSA-PSS should be supported.");
					return;
				}

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo ("application/pkcs7-signature"), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				var signatures = multipart.Verify ();
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (signer.Certificate.GetSubjectEmailAddress ()));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (signer.Certificate.GetFingerprint ()));

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeSigningWithRsaSsaPssAsync ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "rsa", "smime.pfx"), "no.secret") {
				RsaSignaturePadding = RsaSignaturePadding.Pss
			};
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };

			using (var ctx = CreateContext ()) {
				MultipartSigned multipart;

				try {
					multipart = await MultipartSigned.CreateAsync (signer, body);
				} catch (NotSupportedException) {
					if (ctx is not WindowsSecureMimeContext)
						Assert.Fail ("RSASSA-PSS should be supported.");
					return;
				}

				Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

				var protocol = multipart.ContentType.Parameters["protocol"];
				Assert.That (protocol, Is.EqualTo ("application/pkcs7-signature"), "The multipart/signed protocol does not match.");

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				var signatures = await multipart.VerifyAsync ();
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo ("MimeKit UnitTests"));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (signer.Certificate.GetSubjectEmailAddress ()));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (signer.Certificate.GetFingerprint ()));

				var algorithms = GetEncryptionAlgorithms (signature);
				int i = 0;

				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
				Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						// AppVeyor gets an exception about the root certificate not being trusted
						Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeMessageSigning ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
				var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var message = new MimeMessage { Subject = "Test of signing with S/MIME" };

				message.From.Add (self);
				message.Body = body;

				using (var ctx = CreateContext ()) {
					Assert.That (ctx.CanSign (self), Is.True);

					message.Sign (ctx);

					Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "The message body should be a multipart/signed.");

					var multipart = (MultipartSigned) message.Body;

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

					var signatures = multipart.Verify (ctx);
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo (self.Name));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (self.Address));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeMessageSigningAsync ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing..." };
				var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
				var message = new MimeMessage { Subject = "Test of signing with S/MIME" };

				message.From.Add (self);
				message.Body = body;

				using (var ctx = CreateContext ()) {
					Assert.That (await ctx.CanSignAsync (self), Is.True);

					await message.SignAsync (ctx);

					Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "The message body should be a multipart/signed.");

					var multipart = (MultipartSigned) message.Body;

					Assert.That (multipart.Count, Is.EqualTo (2), "The multipart/signed has an unexpected number of children.");

					var protocol = multipart.ContentType.Parameters["protocol"];
					Assert.That (protocol, Is.EqualTo (ctx.SignatureProtocol), "The multipart/signed protocol does not match.");

					Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "The first child is not a text part.");
					Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

					var signatures = await multipart.VerifyAsync (ctx);
					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo (self.Name));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (self.Address));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (certificate.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
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
				Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				var protocol = multipart.ContentType.Parameters["protocol"]?.Trim ();
				Assert.That (ctx.Supports (protocol), Is.True, "The multipart/signed protocol is not supported.");

				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				// Note: this fails in WindowsSecureMimeContext
				var signatures = multipart.Verify (ctx);
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo (ThunderbirdName));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (sender.Address));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (ThunderbirdFingerprint));

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.That (algorithms[0], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[1], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				Assert.That (algorithms[2], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
				Assert.That (algorithms[3], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
				Assert.That (algorithms[4], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
				Assert.That (algorithms[5], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
				Assert.That (algorithms[6], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Environment.OSVersion.Platform != PlatformID.Win32NT)
							Assert.That (ex.InnerException, Is.InstanceOf<ArgumentException> ());
						else
							Assert.That (ex.InnerException.Message, Is.EqualTo (ExpiredCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeVerifyThunderbirdAsync ()
		{
			MimeMessage message;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-signed.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = await parser.ParseMessageAsync ();
			}

			using (var ctx = CreateContext ()) {
				Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "The message body should be a multipart/signed.");

				var multipart = (MultipartSigned) message.Body;

				var protocol = multipart.ContentType.Parameters["protocol"]?.Trim ();
				Assert.That (ctx.Supports (protocol), Is.True, "The multipart/signed protocol is not supported.");

				Assert.That (multipart[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "The second child is not a detached signature.");

				// Note: this fails in WindowsSecureMimeContext
				var signatures = await multipart.VerifyAsync (ctx);
				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo (ThunderbirdName));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (sender.Address));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (ThunderbirdFingerprint));

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.That (algorithms[0], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[1], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				Assert.That (algorithms[2], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
				Assert.That (algorithms[3], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
				Assert.That (algorithms[4], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
				Assert.That (algorithms[5], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
				Assert.That (algorithms[6], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Environment.OSVersion.Platform != PlatformID.Win32NT)
							Assert.That (ex.InnerException, Is.InstanceOf<ArgumentException> ());
						else
							Assert.That (ex.InnerException.Message, Is.EqualTo (ExpiredCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeMessageEncryption ()
		{
			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
					var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var message = new MimeMessage { Subject = "Test of encrypting with S/MIME" };

					message.From.Add (self);
					message.To.Add (self);
					message.Body = body;
					Assert.That (ctx.CanEncrypt (self), Is.True);

					message.Encrypt (ctx);

					Assert.That (message.Body, Is.InstanceOf<ApplicationPkcs7Mime> (), "The message body should be an application/pkcs7-mime part.");

					var encrypted = (ApplicationPkcs7Mime) message.Body;

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

					var decrypted = encrypted.Decrypt (ctx);

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeMessageEncryptionAsync ()
		{
			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
					var self = new MailboxAddress ("MimeKit UnitTests", certificate.EmailAddress);
					var message = new MimeMessage { Subject = "Test of encrypting with S/MIME" };

					message.From.Add (self);
					message.To.Add (self);
					message.Body = body;


					Assert.That (await ctx.CanEncryptAsync (self), Is.True);

					await message.EncryptAsync (ctx);

					Assert.That (message.Body, Is.InstanceOf<ApplicationPkcs7Mime> (), "The message body should be an application/pkcs7-mime part.");

					var encrypted = (ApplicationPkcs7Mime) message.Body;

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

					var decrypted = await encrypted.DecryptAsync (ctx);

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual void TestSecureMimeEncryption (SubjectIdentifierType recipientIdentifierType)
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
				var recipients = new CmsRecipientCollection {
					new CmsRecipient (certificate.Certificate, recipientIdentifierType)
				};

				var encrypted = ApplicationPkcs7Mime.Encrypt (recipients, body);

				Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

				var decrypted = encrypted.Decrypt ();

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual async Task TestSecureMimeEncryptionAsync (SubjectIdentifierType recipientIdentifierType)
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };
				var recipients = new CmsRecipientCollection {
					new CmsRecipient (certificate.Certificate, recipientIdentifierType)
				};

				var encrypted = await ApplicationPkcs7Mime.EncryptAsync (recipients, body);

				Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

				var decrypted = await encrypted.DecryptAsync ();

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual void TestSecureMimeEncryptionWithContext (SubjectIdentifierType recipientIdentifierType)
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var recipients = new CmsRecipientCollection ();

					if (ctx is WindowsSecureMimeContext)
						recipients.Add (new CmsRecipient (certificate.Certificate.AsX509Certificate2 (), recipientIdentifierType));
					else
						recipients.Add (new CmsRecipient (certificate.Certificate, recipientIdentifierType));

					var encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, body);

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

					using (var stream = new MemoryStream ()) {
						ctx.DecryptTo (encrypted.Content.Open (), stream);
						stream.Position = 0;

						var decrypted = MimeEntity.Load (stream);

						Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
						Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
					}
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual async Task TestSecureMimeEncryptionWithContextAsync (SubjectIdentifierType recipientIdentifierType)
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var recipients = new CmsRecipientCollection ();

					if (ctx is WindowsSecureMimeContext)
						recipients.Add (new CmsRecipient (certificate.Certificate.AsX509Certificate2 (), recipientIdentifierType));
					else
						recipients.Add (new CmsRecipient (certificate.Certificate, recipientIdentifierType));

					var encrypted = await ApplicationPkcs7Mime.EncryptAsync (ctx, recipients, body);

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

					using (var stream = new MemoryStream ()) {
						await ctx.DecryptToAsync (encrypted.Content.Open (), stream);
						stream.Position = 0;

						var decrypted = await MimeEntity.LoadAsync (stream);

						Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
						Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
					}
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual void TestSecureMimeEncryptionWithAlgorithm (SubjectIdentifierType recipientIdentifierType)
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var recipients = new CmsRecipientCollection ();

					if (ctx is WindowsSecureMimeContext)
						recipients.Add (new CmsRecipient (certificate.Certificate.AsX509Certificate2 (), recipientIdentifierType));
					else
						recipients.Add (new CmsRecipient (certificate.Certificate, recipientIdentifierType));

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
									Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
									break;
								}
							} else {
								if (algorithm != EncryptionAlgorithm.Twofish)
									Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
							}
							continue;
						} catch (Exception ex) {
							switch (certificate.PublicKeyAlgorithm) {
							case PublicKeyAlgorithm.EllipticCurve:
								if (algorithm == EncryptionAlgorithm.RC240)
									break;
								goto default;
							default:
								Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
								break;
							}
							continue;
						} finally {
							if (!enabled) {
								ctx.Enable (EncryptionAlgorithm.TripleDes);
								ctx.Disable (algorithm);
							}
						}

						Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

						using (var stream = new MemoryStream ()) {
							ctx.DecryptTo (encrypted.Content.Open (), stream);
							stream.Position = 0;

							var decrypted = MimeEntity.Load (stream);

							Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
							Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
						}
					}
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public virtual async Task TestSecureMimeEncryptionWithAlgorithmAsync (SubjectIdentifierType recipientIdentifierType)
		{
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				foreach (var certificate in SupportedCertificates) {
					if (!Supports (certificate.PublicKeyAlgorithm))
						continue;

					var recipients = new CmsRecipientCollection ();

					if (ctx is WindowsSecureMimeContext)
						recipients.Add (new CmsRecipient (certificate.Certificate.AsX509Certificate2 (), recipientIdentifierType));
					else
						recipients.Add (new CmsRecipient (certificate.Certificate, recipientIdentifierType));

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
							encrypted = await ApplicationPkcs7Mime.EncryptAsync (ctx, recipients, body);
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
									Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
									break;
								}
							} else {
								if (algorithm != EncryptionAlgorithm.Twofish)
									Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
							}
							continue;
						} catch (Exception ex) {
							switch (certificate.PublicKeyAlgorithm) {
							case PublicKeyAlgorithm.EllipticCurve:
								if (algorithm == EncryptionAlgorithm.RC240)
									break;
								goto default;
							default:
								Assert.Fail ($"{ctx.GetType ().Name} does not support {algorithm} for {certificate.PublicKeyAlgorithm}: {ex.Message}");
								break;
							}
							continue;
						} finally {
							if (!enabled) {
								ctx.Enable (EncryptionAlgorithm.TripleDes);
								ctx.Disable (algorithm);
							}
						}

						Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

						using (var stream = new MemoryStream ()) {
							await ctx.DecryptToAsync (encrypted.Content.Open (), stream);
							stream.Position = 0;

							var decrypted = await MimeEntity.LoadAsync (stream);

							Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
							Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
						}
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
			var rsa = SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				var recipients = new CmsRecipientCollection ();

				var recipient = new CmsRecipient (rsa.Certificate, SubjectIdentifierType.IssuerAndSerialNumber) {
					EncryptionAlgorithms = new EncryptionAlgorithm[] { EncryptionAlgorithm.Aes128 },
					RsaEncryptionPadding = RsaEncryptionPadding.CreateOaep (hashAlgorithm)
				};
				recipients.Add (recipient);

				ApplicationPkcs7Mime encrypted;

				try {
					encrypted = ApplicationPkcs7Mime.Encrypt (ctx, recipients, body);
				} catch (NotSupportedException) {
					if (ctx is not WindowsSecureMimeContext)
						Assert.Fail ("RSAES-OAEP should be supported.");
					return;
				}

				Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

				using (var stream = new MemoryStream ()) {
					ctx.DecryptTo (encrypted.Content.Open (), stream);
					stream.Position = 0;

					var decrypted = MimeEntity.Load (stream);

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
				}
			}
		}

		[TestCase (DigestAlgorithm.Sha1)]
		[TestCase (DigestAlgorithm.Sha256)]
		[TestCase (DigestAlgorithm.Sha384)]
		[TestCase (DigestAlgorithm.Sha512)]
		public virtual async Task TestSecureMimeEncryptionWithRsaesOaepAsync (DigestAlgorithm hashAlgorithm)
		{
			var rsa = SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				var recipients = new CmsRecipientCollection ();

				var recipient = new CmsRecipient (rsa.Certificate, SubjectIdentifierType.IssuerAndSerialNumber) {
					EncryptionAlgorithms = new EncryptionAlgorithm[] { EncryptionAlgorithm.Aes128 },
					RsaEncryptionPadding = RsaEncryptionPadding.CreateOaep (hashAlgorithm)
				};
				recipients.Add (recipient);

				ApplicationPkcs7Mime encrypted;

				try {
					encrypted = await ApplicationPkcs7Mime.EncryptAsync (ctx, recipients, body);
				} catch (NotSupportedException) {
					if (ctx is not WindowsSecureMimeContext)
						Assert.Fail ("RSAES-OAEP should be supported.");
					return;
				}

				Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

				using (var stream = new MemoryStream ()) {
					await ctx.DecryptToAsync (encrypted.Content.Open (), stream);
					stream.Position = 0;

					var decrypted = await MimeEntity.LoadAsync (stream);

					Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
					Assert.That (((TextPart) decrypted).Text, Is.EqualTo (body.Text), "Decrypted content is not the same as the original.");
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

				ctx.Import (p12, "no.secret");

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.That (type, Is.EqualTo ("enveloped-data"), "Unexpected smime-type parameter.");

				try {
					decrypted = encrypted.Decrypt (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ($"Failed to decrypt thunderbird message: {ex}");
				}

				// The decrypted part should be a multipart/mixed with a text/plain part and an image attachment,
				// very much like the thunderbird-signed.txt message.
				Assert.That (decrypted, Is.InstanceOf<Multipart> (), "Expected the decrypted part to be a Multipart.");
				var multipart = (Multipart) decrypted;

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.That (multipart[1], Is.InstanceOf<MimePart> (), "Expected the second part of the decrypted multipart to be a MimePart.");
			}
		}

		[Test]
		public async Task TestSecureMimeDecryptThunderbirdAsync ()
		{
			var p12 = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-encrypted.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = await parser.ParseMessageAsync ();
			}

			using (var ctx = CreateContext ()) {
				var encrypted = (ApplicationPkcs7Mime) message.Body;
				MimeEntity decrypted = null;

				await ctx.ImportAsync (p12, "no.secret");

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.That (type, Is.EqualTo ("enveloped-data"), "Unexpected smime-type parameter.");

				try {
					decrypted = await encrypted.DecryptAsync (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ($"Failed to decrypt thunderbird message: {ex}");
				}

				// The decrypted part should be a multipart/mixed with a text/plain part and an image attachment,
				// very much like the thunderbird-signed.txt message.
				Assert.That (decrypted, Is.InstanceOf<Multipart> (), "Expected the decrypted part to be a Multipart.");
				var multipart = (Multipart) decrypted;

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.That (multipart[1], Is.InstanceOf<MimePart> (), "Expected the second part of the decrypted multipart to be a MimePart.");
			}
		}

		[Test]
		public virtual void TestSecureMimeSignAndEncrypt ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
				var self = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
				var message = new MimeMessage { Subject = "Test of signing and encrypting with S/MIME" };
				ApplicationPkcs7Mime encrypted;

				message.From.Add (self);
				message.To.Add (self);
				message.Body = body;

				using (var ctx = CreateContext ()) {
					message.SignAndEncrypt (ctx);

					Assert.That (message.Body, Is.InstanceOf<ApplicationPkcs7Mime> (), "The message body should be an application/pkcs7-mime part.");

					encrypted = (ApplicationPkcs7Mime) message.Body;

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");
				}

				using (var ctx = CreateContext ()) {
					var decrypted = encrypted.Decrypt (ctx);

					// The decrypted part should be a multipart/signed
					Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "Expected the decrypted part to be a multipart/signed.");
					var signed = (MultipartSigned) decrypted;

					Assert.That (signed[0], Is.InstanceOf<TextPart> (), "Expected the first part of the multipart/signed to be a multipart.");
					Assert.That (signed[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "Expected second part of the multipart/signed to be a pkcs7-signature.");

					var extracted = (TextPart) signed[0];
					Assert.That (extracted.Text, Is.EqualTo (body.Text), "The decrypted text part's text does not match the original.");

					var signatures = signed.Verify (ctx);

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo (self.Name));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (self.Address));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (self.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
					}
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeSignAndEncryptAsync ()
		{
			foreach (var certificate in SupportedCertificates) {
				if (!Supports (certificate.PublicKeyAlgorithm))
					continue;

				var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up signing and encrypting..." };
				var self = new SecureMailboxAddress ("MimeKit UnitTests", certificate.EmailAddress, certificate.Fingerprint);
				var message = new MimeMessage { Subject = "Test of signing and encrypting with S/MIME" };
				ApplicationPkcs7Mime encrypted;

				message.From.Add (self);
				message.To.Add (self);
				message.Body = body;

				using (var ctx = CreateContext ()) {
					await message.SignAndEncryptAsync (ctx);

					Assert.That (message.Body, Is.InstanceOf<ApplicationPkcs7Mime> (), "The message body should be an application/pkcs7-mime part.");

					encrypted = (ApplicationPkcs7Mime) message.Body;

					Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");
				}

				using (var ctx = CreateContext ()) {
					var decrypted = await encrypted.DecryptAsync (ctx);

					// The decrypted part should be a multipart/signed
					Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "Expected the decrypted part to be a multipart/signed.");
					var signed = (MultipartSigned) decrypted;

					Assert.That (signed[0], Is.InstanceOf<TextPart> (), "Expected the first part of the multipart/signed to be a multipart.");
					Assert.That (signed[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "Expected second part of the multipart/signed to be a pkcs7-signature.");

					var extracted = (TextPart) signed[0];
					Assert.That (extracted.Text, Is.EqualTo (body.Text), "The decrypted text part's text does not match the original.");

					var signatures = await signed.VerifyAsync (ctx);

					Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

					var signature = signatures[0];

					if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
						Assert.That (signature.SignerCertificate.Name, Is.EqualTo (self.Name));
					Assert.That (signature.SignerCertificate.Email, Is.EqualTo (self.Address));
					Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (self.Fingerprint));

					var algorithms = GetEncryptionAlgorithms (signature);
					int i = 0;

					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes192), "Expected AES-192 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Seed))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Seed), "Expected SEED capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia256))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia256), "Expected Camellia-256 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia192))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia192), "Expected Camellia-192 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Camellia128))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Camellia128), "Expected Camellia-128 capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Cast5))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Cast5), "Expected Cast5 capability");
					Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
					if (ctx.IsEnabled (EncryptionAlgorithm.Idea))
						Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Idea), "Expected IDEA capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
					//Assert.That (algorithms[i++], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

					try {
						bool valid = signature.Verify ();

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						if (ctx is WindowsSecureMimeContext) {
							// AppVeyor gets an exception about the root certificate not being trusted
							Assert.That (ex.InnerException.Message, Is.EqualTo (UntrustedRootCertificateMessage));
						} else {
							Assert.Fail ($"Failed to verify signature: {ex}");
						}
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

				ctx.Import (p12, "no.secret");

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.That (type, Is.EqualTo ("enveloped-data"), "Unexpected smime-type parameter.");

				try {
					decrypted = encrypted.Decrypt (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ($"Failed to decrypt thunderbird message: {ex}");
				}

				// The decrypted part should be a multipart/signed
				Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "Expected the decrypted part to be a multipart/signed.");
				var signed = (MultipartSigned) decrypted;

				// The first part of the multipart/signed should be a multipart/mixed with a text/plain part and 2 image attachments,
				// very much like the thunderbird-signed.txt message.
				Assert.That (signed[0], Is.InstanceOf<Multipart> (), "Expected the first part of the multipart/signed to be a multipart.");
				Assert.That (signed[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "Expected second part of the multipart/signed to be a pkcs7-signature.");

				var multipart = (Multipart) signed[0];

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.That (multipart[1], Is.InstanceOf<MimePart> (), "Expected the second part of the decrypted multipart to be a MimePart.");
				Assert.That (multipart[2], Is.InstanceOf<MimePart> (), "Expected the third part of the decrypted multipart to be a MimePart.");

				var signatures = signed.Verify (ctx);

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo (ThunderbirdName));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (sender.Address));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (ThunderbirdFingerprint));

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.That (algorithms[0], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[1], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				Assert.That (algorithms[2], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
				Assert.That (algorithms[3], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
				Assert.That (algorithms[4], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
				Assert.That (algorithms[5], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
				Assert.That (algorithms[6], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Environment.OSVersion.Platform != PlatformID.Win32NT)
							Assert.That (ex.InnerException, Is.InstanceOf<ArgumentException> ());
						else
							Assert.That (ex.InnerException.Message, Is.EqualTo (ExpiredCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public async Task TestSecureMimeDecryptVerifyThunderbirdAsync ()
		{
			var p12 = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "gnome.p12");
			MimeMessage message;

			if (!File.Exists (p12))
				return;

			using (var file = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "thunderbird-signed-encrypted.txt"))) {
				var parser = new MimeParser (file, MimeFormat.Default);
				message = await parser.ParseMessageAsync ();
			}

			using (var ctx = CreateContext ()) {
				var encrypted = (ApplicationPkcs7Mime) message.Body;
				MimeEntity decrypted = null;

				await ctx.ImportAsync (p12, "no.secret");

				var type = encrypted.ContentType.Parameters["smime-type"];
				Assert.That (type, Is.EqualTo ("enveloped-data"), "Unexpected smime-type parameter.");

				try {
					decrypted = await encrypted.DecryptAsync (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
					Assert.Fail ($"Failed to decrypt thunderbird message: {ex}");
				}

				// The decrypted part should be a multipart/signed
				Assert.That (decrypted, Is.InstanceOf<MultipartSigned> (), "Expected the decrypted part to be a multipart/signed.");
				var signed = (MultipartSigned) decrypted;

				// The first part of the multipart/signed should be a multipart/mixed with a text/plain part and 2 image attachments,
				// very much like the thunderbird-signed.txt message.
				Assert.That (signed[0], Is.InstanceOf<Multipart> (), "Expected the first part of the multipart/signed to be a multipart.");
				Assert.That (signed[1], Is.InstanceOf<ApplicationPkcs7Signature> (), "Expected second part of the multipart/signed to be a pkcs7-signature.");

				var multipart = (Multipart) signed[0];

				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected the first part of the decrypted multipart to be a TextPart.");
				Assert.That (multipart[1], Is.InstanceOf<MimePart> (), "Expected the second part of the decrypted multipart to be a MimePart.");
				Assert.That (multipart[2], Is.InstanceOf<MimePart> (), "Expected the third part of the decrypted multipart to be a MimePart.");

				var signatures = await signed.VerifyAsync (ctx);

				Assert.That (signatures.Count, Is.EqualTo (1), "Verify returned an unexpected number of signatures.");

				var sender = message.From.Mailboxes.FirstOrDefault ();
				var signature = signatures[0];

				if (ctx is not WindowsSecureMimeContext || Environment.OSVersion.Platform == PlatformID.Win32NT)
					Assert.That (signature.SignerCertificate.Name, Is.EqualTo (ThunderbirdName));
				Assert.That (signature.SignerCertificate.Email, Is.EqualTo (sender.Address));
				Assert.That (signature.SignerCertificate.Fingerprint.ToLowerInvariant (), Is.EqualTo (ThunderbirdFingerprint));

				var algorithms = GetEncryptionAlgorithms (signature);
				Assert.That (algorithms[0], Is.EqualTo (EncryptionAlgorithm.Aes256), "Expected AES-256 capability");
				Assert.That (algorithms[1], Is.EqualTo (EncryptionAlgorithm.Aes128), "Expected AES-128 capability");
				Assert.That (algorithms[2], Is.EqualTo (EncryptionAlgorithm.TripleDes), "Expected Triple-DES capability");
				Assert.That (algorithms[3], Is.EqualTo (EncryptionAlgorithm.RC2128), "Expected RC2-128 capability");
				Assert.That (algorithms[4], Is.EqualTo (EncryptionAlgorithm.RC264), "Expected RC2-64 capability");
				Assert.That (algorithms[5], Is.EqualTo (EncryptionAlgorithm.Des), "Expected DES capability");
				Assert.That (algorithms[6], Is.EqualTo (EncryptionAlgorithm.RC240), "Expected RC2-40 capability");

				try {
					bool valid = signature.Verify ();

					Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
				} catch (DigitalSignatureVerifyException ex) {
					if (ctx is WindowsSecureMimeContext) {
						if (Environment.OSVersion.Platform != PlatformID.Win32NT)
							Assert.That (ex.InnerException, Is.InstanceOf<ArgumentException> ());
						else
							Assert.That (ex.InnerException.Message, Is.EqualTo (ExpiredCertificateMessage));
					} else {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}

		[Test]
		public virtual void TestSecureMimeImportExport ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var mailboxes = new List<MailboxAddress> {
				// we're going to export our public certificate so that we can email it to someone
				// so that they can then encrypt their emails to us.
				self
			};

			using (var ctx = CreateContext ()) {
				var certsonly = ctx.Export (mailboxes);

				Assert.That (certsonly, Is.InstanceOf<ApplicationPkcs7Mime> (), "The exported mime part is not of the expected type.");

				var pkcs7mime = (ApplicationPkcs7Mime) certsonly;

				Assert.That (pkcs7mime.SecureMimeType, Is.EqualTo (SecureMimeType.CertsOnly), "S/MIME type did not match.");

				using (var imported = new TemporarySecureMimeContext ()) {
					pkcs7mime.Import (imported);

					Assert.That (imported.certificates.Count, Is.EqualTo (1), "Unexpected number of imported certificates.");
					Assert.That (imported.keys.Count > 0, Is.False, "One or more of the certificates included the private key.");
				}
			}
		}

		[Test]
		public virtual async Task TestSecureMimeImportExportAsync ()
		{
			var self = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var mailboxes = new List<MailboxAddress> {
				// we're going to export our public certificate so that we can email it to someone
				// so that they can then encrypt their emails to us.
				self
			};

			using (var ctx = CreateContext ()) {
				var certsonly = await ctx.ExportAsync (mailboxes);

				Assert.That (certsonly, Is.InstanceOf<ApplicationPkcs7Mime> (), "The exported mime part is not of the expected type.");

				var pkcs7mime = (ApplicationPkcs7Mime) certsonly;

				Assert.That (pkcs7mime.SecureMimeType, Is.EqualTo (SecureMimeType.CertsOnly), "S/MIME type did not match.");

				using (var imported = new TemporarySecureMimeContext ()) {
					await pkcs7mime.ImportAsync (imported);

					Assert.That (imported.certificates.Count, Is.EqualTo (1), "Unexpected number of imported certificates.");
					Assert.That (imported.keys.Count > 0, Is.False, "One or more of the certificates included the private key.");
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

			Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "THe message body is not multipart/signed as expected.");

			var signed = (MultipartSigned) message.Body;

			using (var ctx = CreateContext ()) {
				foreach (var signature in signed.Verify (ctx)) {
					try {
						bool valid = signature.Verify (true);

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
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

			Assert.That (message.Body, Is.InstanceOf<MultipartSigned> (), "THe message body is not multipart/signed as expected.");

			var signed = (MultipartSigned) message.Body;

			using (var ctx = CreateContext ()) {
				foreach (var signature in await signed.VerifyAsync (ctx)) {
					try {
						bool valid = signature.Verify (true);

						Assert.That (valid, Is.True, $"Bad signature from {signature.SignerCertificate.Email}");
					} catch (DigitalSignatureVerifyException ex) {
						Assert.Fail ($"Failed to verify signature: {ex}");
					}
				}
			}
		}
	}

	[TestFixture]
	public class SecureMimeTests : SecureMimeTestsBase
	{
		readonly TemporarySecureMimeContext ctx = new TemporarySecureMimeContext (new SecureRandom (new CryptoApiRandomGenerator ())) { CheckCertificateRevocation = true };

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
				return Environment.OSVersion.Platform == PlatformID.Win32NT;
			}
		}

		protected override bool Supports (PublicKeyAlgorithm algorithm)
		{
			switch (algorithm) {
			case PublicKeyAlgorithm.RsaGeneral:
				return true;
			default:
				return false;
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
			if (!IsEnabled)
				return;

			base.TestCanSignAndEncrypt ();
		}

		[Test]
		public override Task TestCanSignAndEncryptAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestCanSignAndEncryptAsync ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigning ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncapsulatedSigning ();
		}

		[Test]
		public override Task TestSecureMimeEncapsulatedSigningAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncapsulatedSigningAsync ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithContext ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncapsulatedSigningWithContext ();
		}

		[Test]
		public override Task TestSecureMimeEncapsulatedSigningWithContextAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncapsulatedSigningWithContextAsync ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithCmsSigner ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncapsulatedSigningWithCmsSigner ();
		}

		[Test]
		public override Task TestSecureMimeEncapsulatedSigningWithCmsSignerAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncapsulatedSigningWithCmsSignerAsync ();
		}

		[Test]
		public override void TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncapsulatedSigningWithContextAndCmsSigner ();
		}

		[Test]
		public override Task TestSecureMimeEncapsulatedSigningWithContextAndCmsSignerAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncapsulatedSigningWithContextAndCmsSignerAsync ();
		}

		[Test]
		public override void TestSecureMimeSigningWithCmsSigner ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeSigningWithCmsSigner ();
		}

		[Test]
		public override Task TestSecureMimeSigningWithCmsSignerAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeSigningWithCmsSignerAsync ();
		}

		[Test]
		public override void TestSecureMimeSigningWithContextAndCmsSigner ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeSigningWithContextAndCmsSigner ();
		}

		[Test]
		public override Task TestSecureMimeSigningWithContextAndCmsSignerAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeSigningWithContextAndCmsSignerAsync ();
		}

		[Test]
		public override void TestSecureMimeSigningWithRsaSsaPss ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeSigningWithRsaSsaPss ();
		}

		[Test]
		public override Task TestSecureMimeSigningWithRsaSsaPssAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeSigningWithRsaSsaPssAsync ();
		}

		[Test]
		public override void TestSecureMimeMessageSigning ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeMessageSigning ();
		}

		[Test]
		public override Task TestSecureMimeMessageSigningAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeMessageSigningAsync ();
		}

		[Test]
		public override void TestSecureMimeVerifyThunderbird ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeVerifyThunderbird ();
		}

		[Test]
		public override Task TestSecureMimeVerifyThunderbirdAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeVerifyThunderbirdAsync ();
		}

		[Test]
		public override void TestSecureMimeMessageEncryption ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeMessageEncryption ();
		}

		[Test]
		public override Task TestSecureMimeMessageEncryptionAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeMessageEncryptionAsync ();
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override void TestSecureMimeEncryption (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncryption (recipientIdentifierType);
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override Task TestSecureMimeEncryptionAsync (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncryptionAsync (recipientIdentifierType);
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override void TestSecureMimeEncryptionWithContext (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncryptionWithContext (recipientIdentifierType);
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override Task TestSecureMimeEncryptionWithContextAsync (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncryptionWithContextAsync (recipientIdentifierType);
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override void TestSecureMimeEncryptionWithAlgorithm (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncryptionWithAlgorithm (recipientIdentifierType);
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public override Task TestSecureMimeEncryptionWithAlgorithmAsync (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncryptionWithAlgorithmAsync (recipientIdentifierType);
		}

		[TestCase (DigestAlgorithm.Sha1)]
		[TestCase (DigestAlgorithm.Sha256)]
		[TestCase (DigestAlgorithm.Sha384)]
		[TestCase (DigestAlgorithm.Sha512)]
		public override void TestSecureMimeEncryptionWithRsaesOaep (DigestAlgorithm hashAlgorithm)
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeEncryptionWithRsaesOaep (hashAlgorithm);
		}

		[TestCase (DigestAlgorithm.Sha1)]
		[TestCase (DigestAlgorithm.Sha256)]
		[TestCase (DigestAlgorithm.Sha384)]
		[TestCase (DigestAlgorithm.Sha512)]
		public override Task TestSecureMimeEncryptionWithRsaesOaepAsync (DigestAlgorithm hashAlgorithm)
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeEncryptionWithRsaesOaepAsync (hashAlgorithm);
		}

		[Test]
		public override void TestSecureMimeSignAndEncrypt ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeSignAndEncrypt ();
		}

		[Test]
		public override Task TestSecureMimeSignAndEncryptAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeSignAndEncryptAsync ();
		}

		[Test]
		public override void TestSecureMimeImportExport ()
		{
			if (!IsEnabled)
				return;

			base.TestSecureMimeImportExport ();
		}

		[Test]
		public override Task TestSecureMimeImportExportAsync ()
		{
			if (!IsEnabled)
				return Task.CompletedTask;

			return base.TestSecureMimeImportExportAsync ();
		}

		static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, SMimeCertificate certificate, SubjectIdentifierType recipientIdentifierType, MimeEntity entity)
		{
			var recipients = new CmsRecipientCollection ();

			if (ctx is WindowsSecureMimeContext)
				recipients.Add (new CmsRecipient (certificate.Certificate.AsX509Certificate2 (), recipientIdentifierType));
			else
				recipients.Add (new CmsRecipient (certificate.Certificate, recipientIdentifierType));

			return ApplicationPkcs7Mime.Encrypt (ctx, recipients, entity);
		}

		static void ValidateCanDecrypt (SecureMimeContext ctx, ApplicationPkcs7Mime encrypted, TextPart expected)
		{
			using (var stream = new MemoryStream ()) {
				ctx.DecryptTo (encrypted.Content.Open (), stream);
				stream.Position = 0;

				var decrypted = MimeEntity.Load (stream);

				Assert.That (decrypted, Is.InstanceOf<TextPart> (), "Decrypted part is not the expected type.");
				Assert.That (((TextPart) decrypted).Text, Is.EqualTo (expected.Text), "Decrypted content is not the same as the original.");
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public void TestBouncyCastleCanDecryptWindows (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return;

			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				using (var bc = new TemporarySecureMimeContext ()) {
					ImportTestCertificates (bc);

					foreach (var certificate in SupportedCertificates) {
						if (!Supports (certificate.PublicKeyAlgorithm))
							continue;

						var encrypted = Encrypt (ctx, certificate, recipientIdentifierType, body);

						Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

						ValidateCanDecrypt (bc, encrypted, body);
					}
				}
			}
		}

		[TestCase (SubjectIdentifierType.IssuerAndSerialNumber)]
		[TestCase (SubjectIdentifierType.SubjectKeyIdentifier)]
		public void TestWindowsCanDecryptBouncyCastle (SubjectIdentifierType recipientIdentifierType)
		{
			if (!IsEnabled)
				return;

			var body = new TextPart ("plain") { Text = "This is some cleartext that we'll end up encrypting..." };

			using (var ctx = CreateContext ()) {
				using (var bc = new TemporarySecureMimeContext ()) {
					ImportTestCertificates (bc);

					foreach (var certificate in SupportedCertificates) {
						if (!Supports (certificate.PublicKeyAlgorithm))
							continue;

						var encrypted = Encrypt (bc, certificate, recipientIdentifierType, body);

						Assert.That (encrypted.SecureMimeType, Is.EqualTo (SecureMimeType.EnvelopedData), "S/MIME type did not match.");

						ValidateCanDecrypt (ctx, encrypted, body);
					}
				}
			}
		}
	}
}
