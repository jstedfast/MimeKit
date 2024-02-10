//
// ArcSignerTests.cs
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

using System.Text;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class ArcSignerTests
	{
		[Test]
		public void TestArcSignerCtors ()
		{
			Assert.DoesNotThrow (() => {
				var signer = new DummyArcSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"), "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256
				};
			});

			AsymmetricCipherKeyPair keys;

			using (var stream = new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				keys = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			Assert.DoesNotThrow (() => {
				var signer = new DummyArcSigner (keys.Private, "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256
				};
			});
		}

		[Test]
		public void TestArcSignerDefaults ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem");
			AsymmetricCipherKeyPair keys;
			ArcSigner signer;

			using (var stream = new StreamReader (path)) {
				var reader = new PemReader (stream);

				keys = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			signer = new DummyArcSigner (keys.Private, "example.com", "1433868189.example");
			Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #1");

			signer = new DummyArcSigner (path, "example.com", "1433868189.example");
			Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #2");

			using (var stream = File.OpenRead (path)) {
				signer = new DummyArcSigner (stream, "example.com", "1433868189.example");
				Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #3");
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem");
			var locator = new DkimPublicKeyLocator ();
			var verifier = new DkimVerifier (locator);
			var dkimHeader = new Header (HeaderId.DkimSignature, "value");
			var arcHeader = new Header (HeaderId.ArcMessageSignature, "value");
			var options = FormatOptions.Default;
			var message = new MimeMessage ();
			AsymmetricCipherKeyPair keys;
			ArcSigner signer;

			using (var stream = new StreamReader (path)) {
				var reader = new PemReader (stream);

				keys = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner ((AsymmetricKeyParameter) null, "domain", "selector"));
			Assert.Throws<ArgumentException> (() => new DummyArcSigner (keys.Public, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner (keys.Private, null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner (keys.Private, "domain", null));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner ((string) null, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner ("fileName", null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner ("fileName", "domain", null));
			Assert.Throws<ArgumentException> (() => new DummyArcSigner (string.Empty, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DummyArcSigner ((Stream) null, "domain", "selector"));
			using (var stream = File.OpenRead (path)) {
				Assert.Throws<ArgumentNullException> (() => new DummyArcSigner (stream, null, "selector"));
				Assert.Throws<ArgumentNullException> (() => new DummyArcSigner (stream, "domain", null));

				signer = new DummyArcSigner (stream, "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1
				};
			}

			// Sign

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (message, (IList<HeaderId>) null));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.Unknown, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.Received, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.ContentType }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (message, (IList<string>) null));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "", "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { null, "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "Received", "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "Content-Type" }));

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, message, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (options, message, new HeaderId[] { HeaderId.From, HeaderId.Unknown }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, message, (IList<HeaderId>) null));

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, message, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, null, new string[] { "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (options, message, new string[] { "From", null }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, message, (IList<string>) null));

			// SignAsync

			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (null, new HeaderId[] { HeaderId.From }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (message, (IList<HeaderId>) null));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new HeaderId[] { HeaderId.Unknown, HeaderId.From }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new HeaderId[] { HeaderId.Received, HeaderId.From }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new HeaderId[] { HeaderId.ContentType }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (null, new string[] { "From" }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (message, (IList<string>) null));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new string[] { "", "From" }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new string[] { null, "From" }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new string[] { "Received", "From" }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (message, new string[] { "Content-Type" }));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (null, message, new HeaderId[] { HeaderId.From }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (options, null, new HeaderId[] { HeaderId.From }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (options, message, new HeaderId[] { HeaderId.From, HeaderId.Unknown }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (options, message, (IList<HeaderId>) null));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (null, message, new string[] { "From" }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (options, null, new string[] { "From" }));
			Assert.ThrowsAsync<ArgumentException> (async () => await signer.SignAsync (options, message, new string[] { "From", null }));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await signer.SignAsync (options, message, (IList<string>) null));
		}

		static void AssertHeadersEqual (string description, HeaderId id, string expected, string actual)
		{
			var expectedTags = DkimVerifierBase.ParseParameterTags (id, expected);
			var actualTags = DkimVerifierBase.ParseParameterTags (id, actual);

			Assert.That (actualTags.Count, Is.EqualTo (expectedTags.Count), $"{id.ToHeaderName ()} parameter counts do not match");
			foreach (var tag in expectedTags) {
				string value;

				if (tag.Key == "b") {
					// Note: these values are affected by tag order, so MimeKit's results *will* differ
					// from the results produced by the library that the tests are based on...
					//
					// We'll validate that these values are correct by using the ArcVerifier.
					continue;
				}

				Assert.That (actualTags.TryGetValue (tag.Key, out value), Is.True, tag.Key);
				Assert.That (value, Is.EqualTo (tag.Value), $"{id.ToHeaderName ()} {tag.Key}= values do not match");
			}
		}

		static void AssertSignResults (string description, MimeMessage message, DkimPublicKeyLocator locator, DkimSignatureAlgorithm algorithm, string aar, string ams, string seal)
		{
			Header header;
			int index;

			if (string.IsNullOrEmpty (seal)) {
				index = message.Headers.IndexOf (HeaderId.ArcSeal);

				Assert.That (index, Is.Not.EqualTo (0), "Message should not have been signed.");
			} else {
				index = message.Headers.IndexOf (HeaderId.ArcAuthenticationResults);
				Assert.That (index, Is.EqualTo (2), "IndexOf ARC-Authentication-Results header");
				header = message.Headers[index];
				Assert.That (header.Value, Is.EqualTo (aar), "ARC-Authentication-Results headers do not match");

				index = message.Headers.IndexOf (HeaderId.ArcMessageSignature);
				Assert.That (index, Is.EqualTo (1), "IndexOf ARC-Message-Signature header");
				header = message.Headers[index];
				AssertHeadersEqual (description, HeaderId.ArcMessageSignature, ams, header.Value);

				index = message.Headers.IndexOf (HeaderId.ArcSeal);
				Assert.That (index, Is.EqualTo (0), "IndexOf ARC-Seal header");
				header = message.Headers[index];
				AssertHeadersEqual (description, HeaderId.ArcSeal, seal, header.Value);

				var expected = ArcSignatureValidationResult.Pass;
				if (header.Value.Contains ("cv=fail;"))
					expected = ArcSignatureValidationResult.Fail;
				var verifier = new ArcVerifier (locator);
				ArcValidationResult result;

				if (!verifier.IsEnabled (algorithm)) {
					result = verifier.Verify (message);

					Assert.That (result.Chain, Is.EqualTo (ArcSignatureValidationResult.Fail), "Disabled algorithm should fail");

					verifier.Enable (algorithm);
				}

				result = verifier.Verify (message);

				Assert.That (result.Chain, Is.EqualTo (expected), "ArcSigner validation failed");
			}
		}

		static void Sign (string description, string input, DkimPublicKeyLocator locator, string srvid, string domain, string selector,
			string privateKey, long t, string[] hdrs, string aar, string ams, string seal,
			DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256,
			DkimCanonicalizationAlgorithm headerAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
			DkimCanonicalizationAlgorithm bodyAlgorithm = DkimCanonicalizationAlgorithm.Relaxed)
		{
			DummyArcSigner signer;

			if (algorithm == DkimSignatureAlgorithm.Ed25519Sha256) {
				var rawData = Convert.FromBase64String (privateKey);
				var key = new Ed25519PrivateKeyParameters (rawData, 0);

				signer = new DummyArcSigner (key, domain, selector, algorithm);
			} else {
				using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (privateKey), false)) {
					signer = new DummyArcSigner (stream, domain, selector, algorithm);
				}
			}

			signer.HeaderCanonicalizationAlgorithm = headerAlgorithm;
			signer.BodyCanonicalizationAlgorithm = bodyAlgorithm;
			signer.PublicKeyLocator = locator;
			signer.Timestamp = t;
			signer.SrvId = srvid;

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (input), false)) {
				var message = MimeMessage.Load (stream);

				// Test Sign(..., string[] headers, ...)
				signer.Sign (message, hdrs);
				AssertSignResults (description, message, locator, algorithm, aar, ams, seal);

				stream.Position = 0;
				message = MimeMessage.Load (stream);

				// Test SignAsync(..., string[] headers, ...)
				signer.SignAsync (message, hdrs).GetAwaiter ().GetResult ();
				AssertSignResults (description, message, locator, algorithm, aar, ams, seal);

				var ids = new HeaderId[hdrs.Length];
				for (int i = 0; i < hdrs.Length; i++)
					ids[i] = hdrs[i].ToHeaderId ();

				stream.Position = 0;
				message = MimeMessage.Load (stream);

				// Test Sign(..., HeaderId[] headers, ...)
				signer.Sign (message, ids);
				AssertSignResults (description, message, locator, algorithm, aar, ams, seal);

				stream.Position = 0;
				message = MimeMessage.Load (stream);

				// Test SignAsync(..., HeaderId[] headers, ...)
				signer.SignAsync (message, ids).GetAwaiter ().GetResult ();
				AssertSignResults (description, message, locator, algorithm, aar, ams, seal);
			}
		}

		#pragma warning disable IDE1006 // Naming Styles

		[Test]
		public void sig_alg_rsa_sha1 ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "i=1; a=rsa-sha1; cv=none; d=example.org; s=dummy; t=12345; b=eF3i/65gJXfQu4kMAiK+EusLS0irqv6gIyJYEcpJTaASwWZd+vt6+nN7pjP7kDZYwno8gVFS5vFrkc959FBjJr1oWVVbVdrJenYqCvBBnFmrYyhG137vV+7RziHerQCJ44VkrF1VEaj7TQ+rp5rmwUMZQlpxdqUYWTIT9DpYAFs=";
			const string ams = "i=1; a=rsa-sha1; d=example.org; s=dummy; c=relaxed/relaxed; t=12345; h=mime-version:date:from:to:subject; bh=bIxxaeIQvmOBdTAitYfSNFgzPP4=; b=qh2z8AFVMdUI4pG3EBTn9+3Af3KU0tmlLUOeUnCsWuigUdE7TTGLirz2ZOeFUAjAeHuKFxJjEPbtYtQh9ptuxZ8Mn9sE/AfIBd85q2yHWu3WOGQFwRrEuLekiH/zXG3d7VUhf8PNveHEXx/NI40qBpPLjXF4o8qxoApGEQZ6CjA=";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("a=rsa-sha1", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal, DkimSignatureAlgorithm.RsaSha1);
		}

		[Test]
		public void sig_alg_ed25519_sha256 ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			const string keyblock = "nWGxne/9WmC6hEr0kuwsxERJxWl7MmkZcDusAxyuf2A=";
			const string seal = "i=1; a=ed25519-sha256; cv=none; d=example.org; s=dummy; t=12345; b=1VlNt2DJiRslGH64VEByTy/rviHk9vVjzaFb6Jd4C5V01Uy9pyrZwa6vwPdUZgrnymXzbz2qgtGyHKd3oYHABg==";
			const string ams = "i=1; a=ed25519-sha256; d=example.org; s=dummy; c=relaxed/relaxed; t=12345; h=mime-version:date:from:to:subject; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; b=f3EEIg6djos7u74HzwTwQaZmCt3jhoQe/PnDsLfnOd5i6slE0MJdoR4lgBOAZMh+nTLF7YfsHF/qopJwoPBQDg==";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");

			Sign ("a=ed25519-sha256", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal, DkimSignatureAlgorithm.Ed25519Sha256);
		}

		[Test]
		public void c_simple_simple ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "i=1; a=rsa-sha256; cv=none; d=example.org; s=dummy; t=12345; b=utRLvF0lNhc2EUk27iMDUrE7jKP4eGWRy9iUKvmgcZtXY8f4x7r5zz8WzLzOUKo8KyUSgnzVWyyxtMqoAcGzzb/1HtwsEiT+B79pCkZDNqcH2E5dHHq7z8/QBwjScZWu3qFaFDtnMktVs13AMc6W1vgAueOyFjH3mdHmAbQBQaY=";
			const string ams = "i=1; a=rsa-sha256; d=example.org; s=dummy; c=simple/simple; t=12345; h=mime-version:date:from:to:subject; bh=hhFbTjokraRYc/Af+8v4zyKm/9ApHGkBSLO129NtPbo=; b=GTpRE34Ud8TOECxNCRNw4jSncvc1nTlLNr6OJC6AjxmumsW2dvQglYIYJK5NO7zPKwzTVUlVB6ZznpIzaKJzsy5oBxV2frl5p5ZBFKGeiU94O0byPiATdwHe4lQiYUK5hBQV0Ei2sxSSlbQPXqjDymnZDcMPp/fLhDQETrjE6AI=";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("a=rsa-sha256", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal, DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm.Simple);
		}

		#region Canonicalization

		[Test]
		public void message_body_eol_wsp ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("message body eol whitespace ignored", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void message_body_inl_wsp ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey   gang,
This is a   test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("message body inline whitespace reduced", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void message_body_end_lines ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("message body ignore trailing empty lines", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void message_body_trail_crlf ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("message body add crlf to end if na", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void headers_field_name_case ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
FROM: John Q Doe <jqd@d1.example.org>
to: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("header field names case insensitive", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void headers_field_unfold ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe
  <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("header", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void headers_eol_wsp ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0   
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("headers ignore eol whitespace", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void headers_inl_wsp ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan   2015   15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("header reduce inline whitespace", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void headers_col_wsp ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From:   John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("headers whitespace surrounding colon ignored", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		#endregion
		#region Existant Seal Headers

		[Test]
		public void i0_base ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=0 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void i1_base ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=pass;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
   b=KalMXVkx0O4PZIytFfe3i6B/c64408LkuF6rYR9HzBsTazolbsFg/nTah+zh9xmVnylcbg
   gZnvu+Rte97HXb9fCK6/rAJQJ97NvYVgzwnEiAzCDts/3dS3SO+PyoAV2HxGMQlUgNeqidAc
   mpm7kE3NbSpgq8Z/rsK5FZ7ADceXw=; cv=none; d=example.org; i=1; s=dummy;
   t=12345          
ARC-Message-Signature: a=rsa-sha256;
   b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm
   5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v
   rCB4wHD9GADeUKVfHzmpZhFuYOa88=;
   bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
   d=example.org; h=mime-version:date:from:to:subject;
   i=1; s=dummy; t=12345          
ARC-Authentication-Results: i=1; lists.example.org; arc=none;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=KiRMwS+rbu4ScgsYQGrZdW72DMVPKRnmkXigPU2FpNPTRViMIRIclMAa48kvbOJ/APWJuG eX3uW2PfI3u2EKtDitHFLvlU2LlCkHhyp8HSO5VJFr0aAk9Z3aQhcoE5hWJ061NXe8C1nafG 4ctcT8p0xkTjVrL9EVsz26o0mRlXY=; cv=pass; d=example.org; i=2; s=dummy; t=12346";
			const string ams = "a=rsa-sha256; b=UaNJhLFAa56Gpc+wKk0SL2Jq/LJgT9CYSZl59wcGYkpG0D5bjhDdj3qers6hD+3BpljNgn mFxq8zWssoPon3ydvTSCSjVwPRNgLol9zBP+FZo/QGQQbj74ZcGv04jOVe8TKDTFSaVe41L7 mH16ZdoLgRdSa2Ys+p9f0+DVFYTO4=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=2; s=dummy; t=12346";
			const string aar = "i=2; lists.example.org; arc=pass; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=1 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12346, hdrs, aar, ams, seal);
		}

		[Test]
		public void i2_base ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=pass;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=I8bdOhGPwqIRyhSYZysZdwFJmD/gRxZR3Dn8BQdKkv3fOsWG8A2aftWwnAHKsNreVi6MUF
    W4M3tVxsG+pF52qzl3zQGn3bkQIS1N700fbu0z0Lg8IW/gcxziGJlLgK5Bk70uN1egGgdLwn
    SiywwvouD7BX1ZlkxFk9i84SDf8/w=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=UaNJhLFAa56Gpc+wKk0SL2Jq/LJgT9CYSZl59wcGYkpG0D5bjhDdj3qers6hD+3BpljNgn
    mFxq8zWssoPon3ydvTSCSjVwPRNgLol9zBP+FZo/QGQQbj74ZcGv04jOVe8TKDTFSaVe41L7
    mH16ZdoLgRdSa2Ys+p9f0+DVFYTO4=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=mime-version:date:from:to:subject;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org; arc=pass;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;               
   b=KalMXVkx0O4PZIytFfe3i6B/c64408LkuF6rYR9HzBsTazolbsFg/nTah+zh9xmVnylcbg
   gZnvu+Rte97HXb9fCK6/rAJQJ97NvYVgzwnEiAzCDts/3dS3SO+PyoAV2HxGMQlUgNeqidAc
   mpm7kE3NbSpgq8Z/rsK5FZ7ADceXw=; cv=none; d=example.org; i=1; s=dummy;
   t=12345
ARC-Message-Signature: a=rsa-sha256;
   b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm
   5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v
   rCB4wHD9GADeUKVfHzmpZhFuYOa88=;
   bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
   d=example.org; h=mime-version:date:from:to:subject;
   i=1; s=dummy; t=12345                    
ARC-Authentication-Results: i=1; lists.example.org; arc=none;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=amYobvirySPk39Y45uHWIsJOGQ0pDhn3D8ncaOew7h+9cddITDFGght2qHYE0GLdpDtLUG J1CwEoM6hdVhG6BkJ80vHzy09Wj2id7B3DMpytPm9MjU7K6Le9VGdewFItwhmG+c15l8krp6 sKw7wUlgM60lSZT0EYTC2x8NXjNDI=; cv=pass; d=example.org; i=3; s=dummy; t=12347";
			const string ams = "a=rsa-sha256; b=QmCd8uJdwnr6wMmniYA/VHCuWButAIlcPZSpNWvk8KHgTuFMZlCPQToT2qVpf2BUfdNpnC mSCED02aLfV6Grc6caqO4PIaxyu3Z+/HNxh0NugIW2JVHT1cZicWkwlgZa4V9i+CYFBAYmzb L0n4ibTxSX8XPxR9ffZdknwiLmYsA=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=3; s=dummy; t=12347";
			const string aar = "i=3; lists.example.org; arc=pass; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=2 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12347, hdrs, aar, ams, seal);
		}

		[Test]
		public void i1_base_fail ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=fail;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
   b=kalMXVkx0O4PZIytFfe3i6B/c64408LkuF6rYR9HzBsTazolbsFg/nTah+zh9xmVnylcbg
   gZnvu+Rte97HXb9fCK6/rAJQJ97NvYVgzwnEiAzCDts/3dS3SO+PyoAV2HxGMQlUgNeqidAc
   mpm7kE3NbSpgq8Z/rsK5FZ7ADceXw=; cv=none; d=example.org; i=1; s=dummy;
   t=12345          
ARC-Message-Signature: a=rsa-sha256;
   b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm
   5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v
   rCB4wHD9GADeUKVfHzmpZhFuYOa88=;
   bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
   d=example.org; h=mime-version:date:from:to:subject;
   i=1; s=dummy; t=12345          
ARC-Authentication-Results: i=1; lists.example.org; arc=none;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=1NUXYB6dkzvHNNuAxkPWkze6te3YkN29XbS1WtqXGPKmwZujBYH8Au3eMW+pKUnCFSK4Bj tyh0/cTU4jKwxE7sVnGV7BbwW8FdRsYSOgT5RCq3GBuWq5SAW5jDzTIoSMU5joN+jU55xw8a mcpcAZse7+iQbftRJflGDEyHZH8s4=; cv=fail; d=example.org; i=2; s=dummy; t=12346";
			const string ams = "a=rsa-sha256; b=UaNJhLFAa56Gpc+wKk0SL2Jq/LJgT9CYSZl59wcGYkpG0D5bjhDdj3qers6hD+3BpljNgn mFxq8zWssoPon3ydvTSCSjVwPRNgLol9zBP+FZo/QGQQbj74ZcGv04jOVe8TKDTFSaVe41L7 mH16ZdoLgRdSa2Ys+p9f0+DVFYTO4=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=2; s=dummy; t=12346";
			const string aar = "i=2; lists.example.org; arc=fail; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=1 basic test with failing arc set", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12346, hdrs, aar, ams, seal);
		}

		[Test]
		public void i2_base_fail ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=fail;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass        
MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=i8bdOhGPwqIRyhSYZysZdwFJmD/gRxZR3Dn8BQdKkv3fOsWG8A2aftWwnAHKsNreVi6MUF
    W4M3tVxsG+pF52qzl3zQGn3bkQIS1N700fbu0z0Lg8IW/gcxziGJlLgK5Bk70uN1egGgdLwn
    SiywwvouD7BX1ZlkxFk9i84SDf8/w=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=UaNJhLFAa56Gpc+wKk0SL2Jq/LJgT9CYSZl59wcGYkpG0D5bjhDdj3qers6hD+3BpljNgn
    mFxq8zWssoPon3ydvTSCSjVwPRNgLol9zBP+FZo/QGQQbj74ZcGv04jOVe8TKDTFSaVe41L7
    mH16ZdoLgRdSa2Ys+p9f0+DVFYTO4=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=mime-version:date:from:to:subject;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org; arc=pass;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;               
   b=KalMXVkx0O4PZIytFfe3i6B/c64408LkuF6rYR9HzBsTazolbsFg/nTah+zh9xmVnylcbg
   gZnvu+Rte97HXb9fCK6/rAJQJ97NvYVgzwnEiAzCDts/3dS3SO+PyoAV2HxGMQlUgNeqidAc
   mpm7kE3NbSpgq8Z/rsK5FZ7ADceXw=; cv=none; d=example.org; i=1; s=dummy;
   t=12345
ARC-Message-Signature: a=rsa-sha256;
   b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm
   5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v
   rCB4wHD9GADeUKVfHzmpZhFuYOa88=;
   bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
   d=example.org; h=mime-version:date:from:to:subject;
   i=1; s=dummy; t=12345                    
ARC-Authentication-Results: i=1; lists.example.org; arc=none;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass      
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.organ>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=P3oIsF0qE5VDD1XPP0oH5XkvpG20k9jmkREcWvi1I9uy6P4UP9Y7mVYTAsNdi8XOg+AMiG CT1CUTmR5+MyYC4mqFW6943PIyzDrDvhZb8DLoy5/tM2cztpSzS0SItqM2XRh0YGp0yMA1sz obc7WTpkgtqFz5beCQC/PjnQ3ZkRw=; cv=fail; d=example.org; i=3; s=dummy; t=12347";
			const string ams = "a=rsa-sha256; b=QmCd8uJdwnr6wMmniYA/VHCuWButAIlcPZSpNWvk8KHgTuFMZlCPQToT2qVpf2BUfdNpnC mSCED02aLfV6Grc6caqO4PIaxyu3Z+/HNxh0NugIW2JVHT1cZicWkwlgZa4V9i+CYFBAYmzb L0n4ibTxSX8XPxR9ffZdknwiLmYsA=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=3; s=dummy; t=12347";
			const string aar = "i=3; lists.example.org; arc=fail; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=1 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12347, hdrs, aar, ams, seal);
		}

		[Test]
		public void no_additional_sig ()
		{
			const string input = @"Authentication-Results: lists.example.org;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass      
MIME-Version: 1.0
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=OrYKWzAKrroSe2lCeF+/5QJOSzJi/RSTggVcdINMmJ8TO8wfkRLaJkAnhLhNts5lnJIDI7
    ZFUmsbtZ6ZhBK5l6WzaE5+iDofcUTjKMFw7keblIE6Frp8Evsb2ShKQZDIseXZxcNHr/Oz0t
    pSKS2JwAriD3rkXm6WVR0Jv+wDFQo=; cv=fail; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=UaNJhLFAa56Gpc+wKk0SL2Jq/LJgT9CYSZl59wcGYkpG0D5bjhDdj3qers6hD+3BpljNgn
    mFxq8zWssoPon3ydvTSCSjVwPRNgLol9zBP+FZo/QGQQbj74ZcGv04jOVe8TKDTFSaVe41L7
    mH16ZdoLgRdSa2Ys+p9f0+DVFYTO4=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=mime-version:date:from:to:subject;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass          
ARC-Seal: a=rsa-sha256;
    b=fOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=mime-version:date:from:to:subject;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "";
			const string ams = "";
			const string aar = "";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("if a chain is failing, dont add another set", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12346, hdrs, aar, ams, seal);
		}

		[Test]
		public void ar_merged1 ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none
Authentication-Results: lists.example.org; spf=pass smtp.mfrom=jqd@d1.example
Authentication-Results: lists.example.org; dkim=pass (1024-bit key) header.i=@d1.example
Authentication-Results: lists.example.org; dmarc=pass
Authentication-Results: nobody.example.org; something=ignored      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=0 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		[Test]
		public void ar_merged2 ()
		{
			const string input = @"Authentication-Results: lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example
Authentication-Results: lists.example.org; dkim=pass (1024-bit key) header.i=@d1.example
Authentication-Results: lists.example.org; dmarc=pass
Authentication-Results: nobody.example.org; something=ignored      
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			const string keyblock = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQi
Y/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqM
KrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB
AoGAH0cxOhFZDgzXWhDhnAJDw5s4roOXN4OhjiXa8W7Y3rhX3FJqmJSPuC8N9vQm
6SVbaLAE4SG5mLMueHlh4KXffEpuLEiNp9Ss3O4YfLiQpbRqE7Tm5SxKjvvQoZZe
zHorimOaChRL2it47iuWxzxSiRMv4c+j70GiWdxXnxe4UoECQQDzJB/0U58W7RZy
6enGVj2kWF732CoWFZWzi1FicudrBFoy63QwcowpoCazKtvZGMNlPWnC7x/6o8Gc
uSe0ga2xAkEA8C7PipPm1/1fTRQvj1o/dDmZp243044ZNyxjg+/OPN0oWCbXIGxy
WvmZbXriOWoSALJTjExEgraHEgnXssuk7QJBALl5ICsYMu6hMxO73gnfNayNgPxd
WFV6Z7ULnKyV7HSVYF0hgYOHjeYe9gaMtiJYoo0zGN+L3AAtNP9huqkWlzECQE1a
licIeVlo1e+qJ6Mgqr0Q7Aa7falZ448ccbSFYEPD6oFxiOl9Y9se9iYHZKKfIcst
o7DUw1/hz2Ck4N5JrgUCQQCyKveNvjzkkd8HjYs0SwM0fPjK16//5qDZ2UiDGnOe
uEzxBDAr518Z8VFbR41in3W4Y3yCDgQlLlcETrS+zYcL
-----END RSA PRIVATE KEY-----
";
			const string seal = "a=rsa-sha256; b=Pg8Yyk1AgYy2l+kb6iy+mY106AXm5EdgDwJhLP7+XyT6yaS38ZUho+bmgSDorV+LyARH4A 967A/oWMX3coyC7pAGyI+hA3+JifL7P3/aIVP4ooRJ/WUgT79snPuulxE15jg6FgQE68ObA1 /hy77BxdbD9EQxFGNcr/wCKQoeKJ8=; cv=none; d=example.org; i=1; s=dummy; t=12345";
			const string ams = "a=rsa-sha256; b=XWeK9DxQ8MUm+Me5GLZ5lQ3L49RdoFv7m7VlrAkKb3/C7jjw33TrTY0KYI5lkowvEGnAtm 5lAqLz67FxA/VrJc2JiYFQR/mBoJLLz/hh9y77byYmSO9tLfIDe2A83+6QsXHO3K6PxTz7+v rCB4wHD9GADeUKVfHzmpZhFuYOa88=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed; d=example.org; h=mime-version:date:from:to:subject; i=1; s=dummy; t=12345";
			const string aar = "i=1; lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass";
			var hdrs = new string[] { "mime-version", "date", "from", "to", "subject" };
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Sign ("i=0 basic test", input, locator, "lists.example.org", "example.org", "dummy", keyblock, 12345, hdrs, aar, ams, seal);
		}

		#endregion

		#pragma warning restore IDE1006 // Naming Styles
	}
}
