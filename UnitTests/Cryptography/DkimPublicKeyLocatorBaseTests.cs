//
// DkimPublicKeyLocatorBaseTests.cs
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

using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DkimPublicKeyLocatorBaseTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", null);

			Assert.Throws<ArgumentNullException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "dummy"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await locator.LocatePublicKeyAsync ("dns/txt", "example.org", "dummy"));
		}

		[Test]
		public void TestParseExceptions ()
		{
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("empty._domainkey.example.org", string.Empty);
			locator.Add ("whitespace._domainkey.example.org", "     ");
			locator.Add ("no-k-or-p-params._domainkey.example.org", "v=DKIM1; x=abc; y=def");
			//locator.Add ("no-k-param._domainkey.example.org", "v=DKIM1; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("no-p-param._domainkey.example.org", "v=DKIM1; k=rsa");
			locator.Add ("unknown-algorithm._domainkey.example.org", "v=DKIM1; k=dummy; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id");

			Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "empty"));
			Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "whitespace"));
			Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "no-k-or-p-params"));
			//Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "no-k-param"));
			Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "no-p-param"));
			Assert.Throws<ParseException> (() => locator.LocatePublicKey ("dns/txt", "example.org", "unknown-algorithm"));
		}

		[Test]
		public void TestParseMissingKParamDefaultsToRsa ()
		{
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("no-k-param._domainkey.example.org", "v=DKIM1; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			var key = locator.LocatePublicKey ("dns/txt", "example.org", "no-k-param");

			Assert.That (key, Is.InstanceOf<RsaKeyParameters> ());
		}
	}
}
