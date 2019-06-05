//
// ArcSignerTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class ArcSignerTests
	{
		static void AssertHeadersEqual (string description, HeaderId id, string expected, string actual)
		{
			var expectedTags = DkimVerifierBase.ParseParameterTags (id, expected);
			var actualTags = DkimVerifierBase.ParseParameterTags (id, actual);

			Assert.AreEqual (expectedTags.Count, actualTags.Count, "{0} parameter counts do not match", id.ToHeaderName ());
			foreach (var tag in expectedTags) {
				string value;

				if (tag.Key == "b" || tag.Key == "bh") {
					// Note: these values are affected by tag order, so MimeKit's results *will* differ
					// from the results produced by the library that the tests are based on...
					//
					// We'll validate that these values are correct by using the ArcVerifier.
					continue;
				}

				Assert.IsTrue (actualTags.TryGetValue (tag.Key, out value), tag.Key);
				Assert.AreEqual (tag.Value, value, "{0} {1}= values do not match", id.ToHeaderName (), tag.Key);
			}
		}

		static void Sign (string description, string input, DkimPublicKeyLocator locator, string srvid, string domain, string selector, string privateKey, long t, string[] hdrs, string aar, string ams, string seal)
		{
			ArcSigner signer;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (privateKey), false)) {
				signer = new DummyArcSigner (stream, domain, selector, DkimSignatureAlgorithm.RsaSha256) {
					HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
					BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
					PublicKeyLocator = locator,
					Timestamp = t,
					SrvId = srvid
				};
			}

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (input), false)) {
				var message = MimeMessage.Load (stream);
				Header header;
				int index;

				signer.Sign (message, hdrs);

				if (string.IsNullOrEmpty (seal)) {
					index = message.Headers.IndexOf (HeaderId.ArcSeal);

					Assert.AreNotEqual (0, index, "Message should not have been signed.");
				} else {
					index = message.Headers.IndexOf (HeaderId.ArcAuthenticationResults);
					Assert.AreEqual (2, index, "IndexOf ARC-Authentication-Results header");
					header = message.Headers[index];
					Assert.AreEqual (aar, header.Value, "ARC-Authentication-Results headers do not match");

					index = message.Headers.IndexOf (HeaderId.ArcMessageSignature);
					Assert.AreEqual (1, index, "IndexOf ARC-Message-Signature header");
					header = message.Headers[index];
					AssertHeadersEqual (description, HeaderId.ArcMessageSignature, ams, header.Value);

					index = message.Headers.IndexOf (HeaderId.ArcSeal);
					Assert.AreEqual (0, index, "IndexOf ARC-Seal header");
					header = message.Headers[index];
					AssertHeadersEqual (description, HeaderId.ArcSeal, seal, header.Value);
				}
			}
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
	}
}
