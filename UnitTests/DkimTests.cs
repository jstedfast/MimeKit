//
// DkimTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests
{
	[TestFixture]
	public class DkimTests
	{
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
		}

		static DkimTests ()
		{
			using (var stream = new StreamReader (Path.Combine ("..", "..", "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				DkimKeys = reader.ReadObject () as AsymmetricCipherKeyPair;
			}
		}

		static DkimSigner CreateSigner (DkimSignatureAlgorithm algorithm)
		{
			return new DkimSigner (DkimKeys.Private, "example.com", "1433868189.example") {
				SignatureAlgorithm = algorithm,
				AgentOrUserIdentifier = "@eng.example.com",
				QueryMethod = "dns/txt",
			};
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

			message.Sign (signer, headers, DkimCanonicalizationAlgorithm.Simple, bodyAlgorithm);

			VerifyDkimBodyHash (message, signatureAlgorithm, expectedHash);

			var dkim = message.Headers[0];

			Assert.IsTrue (message.Verify (dkim, new DummyPublicKeyLocator (DkimKeys.Public)), "Failed to verify DKIM-Signature.");
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

		[Test]
		public void TestVerifyGoogleMailDkimSignature ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "dkim", "gmail.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			AsymmetricKeyParameter key;

			// Note: you can use http://dkimcore.org/tools/dkimrecordcheck.html to get public keys manually
			using (var stream = new StreamReader (Path.Combine ("..", "..", "TestData", "dkim", "gmail.pub"))) {
				var reader = new PemReader (stream);

				key = reader.ReadObject () as AsymmetricKeyParameter;
			}

			message.Verify (message.Headers[index], new DummyPublicKeyLocator (key));
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
		public void TestDkimSignVerifyJwzMbox ()
		{
			using (var stream = File.OpenRead ("../../TestData/mbox/jwz.mbox.txt")) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
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
				}
			}
		}
	}
}
