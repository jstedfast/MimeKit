//
// TemporarySecureMimeContextTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

using MimeKit;
using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class TemporarySecureMimeContextTests
	{
		class TestableTemporarySecureMimeContext : TemporarySecureMimeContext
		{
			public int TrustedAnchorCount {
				get { return GetTrustedAnchors ().Count; }
			}

			public X509Certificate GetMatchingCertificate (ISelector<X509Certificate> selector)
			{
				return GetCertificate (selector);
			}

			public AsymmetricKeyParameter GetMatchingPrivateKey (ISelector<X509Certificate> selector)
			{
				return GetPrivateKey (selector);
			}
		}

		[Test]
		public void TestImportX509Certificate2 ()
		{
			var rsa = SecureMimeTestsBase.RsaCertificate;
			using var certificate = new X509Certificate2 (rsa.FileName, "no.secret", X509KeyStorageFlags.Exportable);

			using (var ctx = new TemporarySecureMimeContext ()) {
				var secure = new SecureMailboxAddress ("MimeKit UnitTests", rsa.EmailAddress, certificate.Thumbprint);
				var mailbox = new MailboxAddress ("MimeKit UnitTests", rsa.EmailAddress);

				ctx.Import (certificate);

				// Check that the certificate exists in the context
				Assert.That (ctx.CanSign (mailbox), Is.True, "CanSign(MailboxAddress)");
				Assert.That (ctx.CanEncrypt (mailbox), Is.True, "CanEncrypt(MailboxAddress)");
				Assert.That (ctx.CanSign (secure), Is.True, "CanSign(SecureMailboxAddress)");
				Assert.That (ctx.CanEncrypt (secure), Is.True, "CanEncrypt(SecureMailboxAddress)");
			}

		}

		static async Task AssertExplicitTrustAsync (bool doAsync)
			{
				using var ctx = new TestableTemporarySecureMimeContext ();
				var root = SecureMimeTestsBase.RootCertificate;
				using var certificate = new X509Certificate2 (root.GetEncoded ());
				ctx.Import (certificate);
				Assert.That (ctx.TrustedAnchorCount, Is.Zero, "X509Certificate2 import must not grant trust.");
				ctx.Import (SecureMimeTestsBase.RsaCertificate.FileName, "no.secret");
				Assert.That (ctx.TrustedAnchorCount, Is.Zero, "PKCS#12 import must not grant trust.");

				if (doAsync)
					await ctx.ImportAsync (root);
				else
					ctx.Import (root);
				Assert.That (ctx.TrustedAnchorCount, Is.Zero);

				if (doAsync)
					await ctx.ImportAsync (root, true);
				else
					ctx.Import (root, true);
				Assert.That (ctx.TrustedAnchorCount, Is.EqualTo (1));
				ctx.Import (root);
				ctx.Import (certificate);
				Assert.That (ctx.TrustedAnchorCount, Is.EqualTo (1), "Ordinary imports must preserve explicit trust.");

				if (doAsync)
					await ctx.ImportAsync (root, false);
				else
					ctx.Import (root, false);
				Assert.That (ctx.TrustedAnchorCount, Is.Zero);
			}

			[Test]
			public void TestExplicitTrust ()
			{
				AssertExplicitTrustAsync (false).GetAwaiter ().GetResult ();
			}

			[Test]
			public Task TestExplicitTrustAsync ()
			{
				return AssertExplicitTrustAsync (true);
			}

			static async Task AssertMessageCertificateIsUntrustedAsync (bool doAsync, bool trusted)
			{
				var root = SecureMimeTestsBase.RootCertificate;
				var generator = new CmsSignedDataGenerator ();
				generator.AddSigner (SecureMimeTestsBase.RootKey, root, CmsSignedGenerator.DigestSha256);
				generator.AddCertificates (CollectionUtilities.CreateStore (new[] { root }));
				var payload = System.Text.Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\nsigned data");
				var signed = generator.Generate (new CmsProcessableByteArray (payload), false).GetEncoded ();
				using var ctx = new TestableTemporarySecureMimeContext ();
				if (trusted)
					ctx.Import (root, true);
				using var content = new MemoryStream (payload);
				using var signatureData = new MemoryStream (signed);
				var signatures = doAsync ? await ctx.VerifyAsync (content, signatureData) : ctx.Verify (content, signatureData);
				var signature = (SecureMimeDigitalSignature) signatures[0];

				Assert.That (ctx.TrustedAnchorCount, Is.EqualTo (trusted ? 1 : 0));
				Assert.That (signature.Verify (true), Is.True);
				if (trusted) {
					Assert.That (signature.ChainException, Is.Null);
					Assert.That (signature.Verify (), Is.True);
				} else {
					Assert.That (signature.Chain, Is.Null);
					Assert.That (signature.ChainException, Is.Not.Null);
					Assert.Throws<DigitalSignatureVerifyException> (() => signature.Verify ());
				}
			}

			[Test]
			public void TestMessageCertificateIsUntrusted ([Values (false, true)] bool trusted)
			{
				AssertMessageCertificateIsUntrustedAsync (false, trusted).GetAwaiter ().GetResult ();
			}

			[Test]
			public Task TestMessageCertificateIsUntrustedAsync ([Values (false, true)] bool trusted)
			{
				return AssertMessageCertificateIsUntrustedAsync (true, trusted);
			}

		[Test]
		public async Task TestImportX509Certificate2Async ()
		{
			var rsa = SecureMimeTestsBase.RsaCertificate;
			using var certificate = new X509Certificate2 (rsa.FileName, "no.secret", X509KeyStorageFlags.Exportable);

			using (var ctx = new TemporarySecureMimeContext ()) {
				var secure = new SecureMailboxAddress ("MimeKit UnitTests", rsa.EmailAddress, certificate.Thumbprint);
				var mailbox = new MailboxAddress ("MimeKit UnitTests", rsa.EmailAddress);

				await ctx.ImportAsync (certificate);

				// Check that the certificate exists in the context
				Assert.That (await ctx.CanSignAsync (mailbox), Is.True, "CanSign(MailboxAddress)");
				Assert.That (await ctx.CanEncryptAsync (mailbox), Is.True, "CanEncrypt(MailboxAddress)");
				Assert.That (await ctx.CanSignAsync (secure), Is.True, "CanSign(SecureMailboxAddress)");
				Assert.That (await ctx.CanEncryptAsync (secure), Is.True, "CanEncrypt(SecureMailboxAddress)");
			}
		}

		[Test]
		public void TestProtectedCertificateAndPrivateKeyLookup ()
		{
			var rsa = SecureMimeTestsBase.RsaCertificate;
			var selector = new X509CertStoreSelector {
				Subject = rsa.Chain[0].SubjectDN
			};

			using (var ctx = new TestableTemporarySecureMimeContext ()) {
				Assert.That (ctx.GetMatchingCertificate (null), Is.Null);
				Assert.That (ctx.GetMatchingPrivateKey (null), Is.Null);

				ctx.Import (rsa.FileName, "no.secret");

				Assert.That (ctx.GetMatchingCertificate (null).GetFingerprint (), Is.EqualTo (rsa.Fingerprint));
				Assert.That (ctx.GetMatchingCertificate (selector).GetFingerprint (), Is.EqualTo (rsa.Fingerprint));
				Assert.That (ctx.GetMatchingPrivateKey (selector).IsPrivate, Is.True);
			}
		}
	}
}
