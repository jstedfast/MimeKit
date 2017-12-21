//
// DkimTests.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DkimTests
	{
		static readonly AsymmetricKeyParameter GMailDkimPublicKey;
		static readonly AsymmetricCipherKeyPair DkimKeys;

		class DummyPublicKeyLocator : IDkimPublicKeyLocator
		{
			readonly AsymmetricKeyParameter key;

			public DummyPublicKeyLocator (AsymmetricKeyParameter publicKey)
			{
				key = publicKey;
			}

			public AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
			{
				return key;
			}

			public Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
			{
				return Task.FromResult (key);
			}
		}

		static DkimTests ()
		{
			using (var stream = new StreamReader (Path.Combine ("..", "..", "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				DkimKeys = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			// Note: you can use http://dkimcore.org/tools/dkimrecordcheck.html to get public keys manually
			using (var stream = new StreamReader (Path.Combine ("..", "..", "TestData", "dkim", "gmail.pub"))) {
				var reader = new PemReader (stream);

				GMailDkimPublicKey = reader.ReadObject () as AsymmetricKeyParameter;
			}
		}

		static DkimSigner CreateSigner (DkimSignatureAlgorithm algorithm)
		{
			return new DkimSigner (Path.Combine ("..", "..", "TestData", "dkim", "example.pem"), "example.com", "1433868189.example") {
				SignatureAlgorithm = algorithm,
				AgentOrUserIdentifier = "@eng.example.com",
				QueryMethod = "dns/txt",
			};
		}

		[Test]
		public void TestDkimSignatureStream ()
		{
			var signer = CreateSigner (DkimSignatureAlgorithm.RsaSha1);
			var buffer = new byte[128];

			Assert.Throws<ArgumentNullException> (() => new DkimSignatureStream (null));

			using (var stream = new DkimSignatureStream (signer.DigestSigner)) {
				Assert.IsFalse (stream.CanRead);
				Assert.IsTrue (stream.CanWrite);
				Assert.IsFalse (stream.CanSeek);
				Assert.IsFalse (stream.CanTimeout);

				Assert.Throws<NotSupportedException> (() => stream.Read (buffer, 0, buffer.Length));

				Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1));

				Assert.AreEqual (0, stream.Length);

				Assert.Throws<NotSupportedException> (() => stream.Position = 64);

				Assert.Throws<NotSupportedException> (() => stream.Seek (64, SeekOrigin.Begin));
				Assert.Throws<NotSupportedException> (() => stream.SetLength (256));

				Assert.Throws<ArgumentNullException> (() => stream.VerifySignature (null));
			}
		}

		static void VerifyDkimBodyHash (MimeMessage message, DkimSignatureAlgorithm algorithm, string expectedHash)
		{
			var value = message.Headers[HeaderId.DkimSignature];
			var parameters = value.Split (';');
			string hash = null;

			for (int i = 0; i < parameters.Length; i++) {
				var param = parameters[i].Trim ();

				if (param.StartsWith ("bh=", StringComparison.Ordinal)) {
					hash = param.Substring (3);
					break;
				}
			}

			Assert.AreEqual (expectedHash, hash, "The {0} hash does not match the expected value.", algorithm.ToString ().ToUpperInvariant ().Substring (3));
		}

		static void TestEmptyBody (DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm, string expectedHash)
		{
			var headers = new [] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date };
			var signer = CreateSigner (signatureAlgorithm);
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "This is an empty message";
			message.Date = DateTimeOffset.Now;

			message.Body = new TextPart ("plain") { Text = "" };

			message.Body.Prepare (EncodingConstraint.SevenBit);

			message.Sign (signer, headers, DkimCanonicalizationAlgorithm.Simple, bodyAlgorithm);

			VerifyDkimBodyHash (message, signatureAlgorithm, expectedHash);

			var dkim = message.Headers[0];

			Assert.IsTrue (message.Verify (dkim, new DummyPublicKeyLocator (DkimKeys.Public)), "Failed to verify DKIM-Signature.");
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var locator = new DummyPublicKeyLocator (DkimKeys.Public);
			var dkimHeader = new Header (HeaderId.DkimSignature, "value");
			var options = FormatOptions.Default;
			var message = new MimeMessage ();
			DkimSigner signer;

			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((AsymmetricKeyParameter) null, "domain", "selector"));
			Assert.Throws<ArgumentException> (() => new DkimSigner (DkimKeys.Public, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner (DkimKeys.Private, null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner (DkimKeys.Private, "domain", null));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((string) null, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ("fileName", null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ("fileName", "domain", null));
			Assert.Throws<ArgumentException> (() => new DkimSigner (string.Empty, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((Stream) null, "domain", "selector"));
			using (var stream = File.OpenRead (Path.Combine ("..", "..", "TestData", "dkim", "example.pem"))) {
				Assert.Throws<ArgumentNullException> (() => new DkimSigner (stream, null, "selector"));
				Assert.Throws<ArgumentNullException> (() => new DkimSigner (stream, "domain", null));

				signer = new DkimSigner (stream, "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1,
					AgentOrUserIdentifier = "@eng.example.com",
					QueryMethod = "dns/txt",
				};
			}

			Assert.Throws<ArgumentNullException> (() => message.Sign (null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (signer, (IList<HeaderId>) null));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new HeaderId[] { HeaderId.Unknown, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new HeaderId[] { HeaderId.Received, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new HeaderId[] { HeaderId.ContentType }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (null, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (signer, (IList<string>) null));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new string[] { "", "From" }));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new string[] { null, "From" }));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new string[] { "Received", "From" }));
			Assert.Throws<ArgumentException> (() => message.Sign (signer, new string[] { "Content-Type" }));

			Assert.Throws<ArgumentNullException> (() => message.Sign (null, signer, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (options, null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentException> (() => message.Sign (options, signer, new HeaderId[] { HeaderId.From, HeaderId.Unknown }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (options, signer, (IList<HeaderId>) null));

			Assert.Throws<ArgumentNullException> (() => message.Sign (null, signer, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (options, null, new string[] { "From" }));
			Assert.Throws<ArgumentException> (() => message.Sign (options, signer, new string[] { "From", null }));
			Assert.Throws<ArgumentNullException> (() => message.Sign (options, signer, (IList<string>) null));

			Assert.Throws<ArgumentNullException> (() => message.Verify (null, locator));
			Assert.Throws<ArgumentNullException> (() => message.Verify (dkimHeader, null));
			Assert.Throws<ArgumentNullException> (() => message.Verify (null, dkimHeader, locator));
			Assert.Throws<ArgumentNullException> (() => message.Verify (FormatOptions.Default, null, locator));
			Assert.Throws<ArgumentNullException> (() => message.Verify (FormatOptions.Default, dkimHeader, null));
		}

		[Test]
		public void TestEmptySimpleBodySha1 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, "uoq1oCgLlTqpdDX/iUbLy7J1Wic=");
		}

		[Test]
		public void TestEmptySimpleBodySha256 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Simple, "frcCV1k9oG9oKj3dpUqdJg1PxRT2RSN/XKdLCPjaYaY=");
		}

		[Test]
		public void TestEmptyRelaxedBodySha1 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Relaxed, "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
		}

		[Test]
		public void TestEmptyRelaxedBodySha256 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Relaxed, "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
		}

		static void TestUnicode (DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm, string expectedHash)
		{
			var headers = new [] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date };
			var signer = CreateSigner (signatureAlgorithm);
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "This is a unicode message";
			message.Date = DateTimeOffset.Now;

			var builder = new BodyBuilder ();
			builder.TextBody = " تست  ";
			builder.HtmlBody = "  <div> تست </div> ";
			message.Body = builder.ToMessageBody ();

			((Multipart) message.Body).Boundary = "=-MultipartAlternativeBoundary";
			((Multipart) message.Body)[1].ContentId = null;

			message.Body.Prepare (EncodingConstraint.EightBit);

			message.Sign (signer, headers, DkimCanonicalizationAlgorithm.Simple, bodyAlgorithm);

			var dkim = message.Headers[0];

			VerifyDkimBodyHash (message, signatureAlgorithm, expectedHash);

			Assert.IsTrue (message.Verify (dkim, new DummyPublicKeyLocator (DkimKeys.Public)), "Failed to verify DKIM-Signature.");
		}

		[Test]
		public void TestUnicodeSimpleBodySha1 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, "6GV1ZoyaprYbwRLXsr5+8zY5Jh0=");
		}

		[Test]
		public void TestUnicodeSimpleBodySha256 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Simple, "BuW/GpCA9rAVDfStp0Dc2duuFhmwcxhy5jOeL+Xn+ew=");
		}

		[Test]
		public void TestUnicodeRelaxedBodySha1 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Relaxed, "bbT6nP0aAiAP5OMguA+mHgpzgh4=");
		}

		[Test]
		public void TestUnicodeRelaxedBodySha256 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Relaxed, "PEaN3fYH5NdIg4QzgaSS+ceYlSMRnYbqCPMxncx6gy0=");
		}

		[Test]
		public void TestVerifyGoogleMailDkimSignature ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "gmail.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (message.Verify (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		[Test]
		public async void TestVerifyGoogleMailDkimSignatureAsync ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "gmail.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (await message.VerifyAsync (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		[Test]
		public void TestVerifyGoogleMultipartRelatedDkimSignature ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "related.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (message.Verify (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		[Test]
		public async void TestVerifyGoogleMultipartRelatedDkimSignatureAsync ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "related.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (await message.VerifyAsync (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		[Test]
		public void TestVerifyGoogleMultipartWithoutEndBoundaryDkimSignature ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "multipart-no-end-boundary.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (message.Verify (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		[Test]
		public async void TestVerifyGoogleMultipartWithoutEndBoundaryDkimSignatureAsync ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "multipart-no-end-boundary.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var locator = new DummyPublicKeyLocator (GMailDkimPublicKey);

			Assert.IsTrue (await message.VerifyAsync (message.Headers[index], locator), "Failed to verify GMail signature.");
		}

		static void TestDkimSignVerify (MimeMessage message, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm headerAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm)
		{
			var headers = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
			var signer = CreateSigner (signatureAlgorithm);

			message.Sign (signer, headers, headerAlgorithm, bodyAlgorithm);

			var dkim = message.Headers[0];

			Assert.IsTrue (message.Verify (dkim, new DummyPublicKeyLocator (DkimKeys.Public)), "Failed to verify DKIM-Signature.");

			message.Headers.RemoveAt (0);
		}

		[Test]
		//[Ignore]
		public void TestDkimSignVerifyJwzMbox ()
		{
			using (var stream = File.OpenRead ("../../TestData/mbox/jwz.mbox.txt")) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream && i < 10) {
					var message = parser.ParseMessage ();

					TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha1,
						DkimCanonicalizationAlgorithm.Relaxed,
						DkimCanonicalizationAlgorithm.Relaxed);

					TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha256,
						DkimCanonicalizationAlgorithm.Relaxed,
						DkimCanonicalizationAlgorithm.Simple);

					TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha1,
						DkimCanonicalizationAlgorithm.Simple,
						DkimCanonicalizationAlgorithm.Relaxed);

					TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha256,
						DkimCanonicalizationAlgorithm.Simple,
						DkimCanonicalizationAlgorithm.Simple);

					i++;
				}
			}
		}
	}
}
