//
// UrlScannerTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class UrlScannerTests
	{
		readonly UrlScanner scanner;

		public UrlScannerTests ()
		{
			scanner = new UrlScanner ();

			for (int i = 0; i < TextConverter.UrlPatterns.Count; i++)
				scanner.Add (TextConverter.UrlPatterns[i]);
		}

		[Test]
		public void TestNoMatch ()
		{
			char[] text = "This is some text with nothing to match...".ToCharArray ();
			UrlMatch match;

			Assert.That (scanner.Scan (text, 0, text.Length, out match), Is.False, "Should not have found a match");
		}

		void TestUrlScanner (string input, string expected)
		{
			char[] text = input.ToCharArray ();
			UrlMatch match;
			string url;

			if (expected == null) {
				Assert.That (scanner.Scan (text, 0, text.Length, out match), Is.False, "Should not have found a match.");
				return;
			}

			Assert.That (scanner.Scan (text, 0, text.Length, out match), Is.True, "Failed to find match.");

			url = new string (text, match.StartIndex, match.EndIndex - match.StartIndex);

			Assert.That (url, Is.EqualTo (expected), "Did not match the expected substring.");
		}

		[Test]
		public void TestSimpleAddrspec ()
		{
			TestUrlScanner ("This is some text with a simple.addrspec@example.com in the text...", "simple.addrspec@example.com");
		}

		[Test]
		public void TestSimpleAddrspecPeriod ()
		{
			TestUrlScanner ("This is some text with a simple.addrspec@example.com. Did it work?", "simple.addrspec@example.com");
		}

		[Test]
		public void TestSimpleQuotedLocalpartAddrspec ()
		{
			TestUrlScanner ("This is some text with a \"quoted local part\"@example.com in the text...", "\"quoted local part\"@example.com");
		}

		[Test]
		public void TestComplexQuotedLocalpartAddrspec ()
		{
			TestUrlScanner ("This is some text with a \"quoted \\\"local\\\" part\"@example.com in the text...", "\"quoted \\\"local\\\" part\"@example.com");
		}

		[Test]
		public void TestIPv4Addrspec ()
		{
			TestUrlScanner ("This is some text with a ipv4@[127.0.0.1] in the text...", "ipv4@[127.0.0.1]");
		}

		[Test]
		public void TestIPv6LoopbackAddrspec ()
		{
			TestUrlScanner ("This is some text with a ipv6@[IPv6:::1] in the text...", "ipv6@[IPv6:::1]");
		}

		[Test]
		public void TestIPv6v4Addrspec ()
		{
			TestUrlScanner ("This is some text with a ipv6@[IPv6:::ffff:10.0.0.1] in the text...", "ipv6@[IPv6:::ffff:10.0.0.1]");
		}

		[Test]
		public void TestIPv6Addrspec ()
		{
			TestUrlScanner ("This is some text with a ipv6@[IPv6:FE80:0000:0000:0000:0202:B3FF:FE1E:8329] in the text...", "ipv6@[IPv6:FE80:0000:0000:0000:0202:B3FF:FE1E:8329]");
		}

		[Test]
		public void TestSimpleMailToUrl ()
		{
			TestUrlScanner ("This is some text with a mailto:simple.addrspec@example.com in the text...", "mailto:simple.addrspec@example.com");
		}

		[Test]
		public void TestMailToWithSimpleQuotedLocalpartAddrspec ()
		{
			TestUrlScanner ("This is some text with a mailto:\"quoted local part\"@example.com in the text...", "mailto:\"quoted local part\"@example.com");
		}

		[Test]
		public void TestMailToWithComplexQuotedLocalpartAddrspec ()
		{
			TestUrlScanner ("This is some text with a mailto:\"quoted \\\"local\\\" part\"@example.com in the text...", "mailto:\"quoted \\\"local\\\" part\"@example.com");
		}

		[Test]
		public void TestMailToIPv4Addrspec ()
		{
			TestUrlScanner ("This is some text with a mailto:ipv4@[127.0.0.1] in the text...", "mailto:ipv4@[127.0.0.1]");
		}

		[Test]
		public void TestMailToIPv6Addrspec ()
		{
			TestUrlScanner ("This is some text with a mailto:ipv6@[IPv6:::1] in the text...", "mailto:ipv6@[IPv6:::1]");
		}

		[Test]
		public void TestMailToUrlWithoutAddrspec ()
		{
			TestUrlScanner ("This is some text with a mailto:?subject=Shake%20it%20off,%20shake%20it%20off in the text...", "mailto:?subject=Shake%20it%20off,%20shake%20it%20off");
		}

		[Test]
		public void TestMailToUrlWithAddrspecAndSubject ()
		{
			TestUrlScanner ("This is some text with a mailto:taylor.swift@mtv.com?subject=Shake%20it%20off,%20shake%20it%20off in the text...", "mailto:taylor.swift@mtv.com?subject=Shake%20it%20off,%20shake%20it%20off");
		}

		[Test]
		public void TestFileUrl ()
		{
			TestUrlScanner ("This is some text with a file:///path/to/some/filename.txt url in it...", "file:///path/to/some/filename.txt");
		}

		[Test]
		public void TestSimpleWebUrl ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com url in it...", "http://www.xamarin.com");
		}

		[Test]
		public void TestSimpleWebUrlWithPath ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com/logo.png url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPathEnclosedInParens ()
		{
			TestUrlScanner ("This is some text with an (http://www.xamarin.com/logo.png) url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPathEnclosedInCurlyBraces ()
		{
			TestUrlScanner ("This is some text with an {http://www.xamarin.com/logo.png} url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPathEnclosedInAngleBrackets ()
		{
			TestUrlScanner ("This is some text with an <http://www.xamarin.com/logo.png> url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPathEnclosedInSquareBrackets ()
		{
			TestUrlScanner ("This is some text with an [http://www.xamarin.com/logo.png] url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPathEnclosedInPipes ()
		{
			TestUrlScanner ("This is some text with an |http://www.xamarin.com/logo.png| url in it...", "http://www.xamarin.com/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithPort ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com:80 url in it...", "http://www.xamarin.com:80");
		}

		[Test]
		public void TestSimpleWebUrlWithPortAndPath ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com:80/logo.png url in it...", "http://www.xamarin.com:80/logo.png");
		}

		[Test]
		public void TestSimpleWebUrlWithQuery ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com?query url in it...", "http://www.xamarin.com?query");
		}

		[Test]
		public void TestSimpleWebUrlWithPortAndQuery ()
		{
			TestUrlScanner ("This is some text with an http://www.xamarin.com:80?query url in it...", "http://www.xamarin.com:80?query");
		}

		[Test]
		public void TestSimpleWebUrlWithQuestionMark ()
		{
			TestUrlScanner ("Have you seen this website: http://www.xamarin.com? Wow!", "http://www.xamarin.com");
		}

		[Test]
		public void TestSimpleWebUrlWithTwoQuestionMarks ()
		{
			TestUrlScanner ("Have you seen this website: http://www.xamarin.com?? Wow!", "http://www.xamarin.com");
		}

		[Test]
		public void TestWebUrlWithLeadingNumericDomain ()
		{
			TestUrlScanner ("Have you seen this website: https://23andme.com?? Now you can check if you really are 1/1024 native american!", "https://23andme.com");
		}

		[Test]
		public void TestTextEndingWithFtpDot ()
		{
			TestUrlScanner ("This is some text with that ends with ftp.", null);
		}
	}
}
