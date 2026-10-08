//
// UrlScannerTests.cs
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

using System.Globalization;

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

		[TestCase ("See http://example.com/path.", "http://example.com/path")]
		[TestCase ("See http://example.com/path, then", "http://example.com/path")]
		[TestCase ("See http://example.com/path; then", "http://example.com/path")]
		[TestCase ("See http://example.com/path: it", "http://example.com/path")]
		[TestCase ("See http://example.com/path!", "http://example.com/path")]
		[TestCase ("See http://example.com/path?!", "http://example.com/path")]
		[TestCase ("See http://example.com/path...", "http://example.com/path")]
		[TestCase ("See 'http://example.com/path'", "http://example.com/path")]
		[TestCase ("See \"http://example.com/path\"", "http://example.com/path")]
		[TestCase ("See *http://example.com/path*", "http://example.com/path")]
		[TestCase ("See http://example.com/.", "http://example.com/")]
		[TestCase ("See http://example.com/path?q=1.", "http://example.com/path?q=1")]
		[TestCase ("See http://example.com/a.b/c.html.", "http://example.com/a.b/c.html")]
		[TestCase ("(see http://example.com/path).", "http://example.com/path")]
		[TestCase ("see http://example.com/path).", "http://example.com/path")]
		[TestCase ("see http://example.com/path]", "http://example.com/path")]
		[TestCase ("see http://example.com/path}", "http://example.com/path")]
		[TestCase ("see http://en.wikipedia.org/wiki/Foo_(bar) now", "http://en.wikipedia.org/wiki/Foo_(bar)")]
		[TestCase ("see http://en.wikipedia.org/wiki/Foo_(bar)).", "http://en.wikipedia.org/wiki/Foo_(bar)")]
		[TestCase ("see http://example.com/a[0]", "http://example.com/a[0]")]
		[TestCase ("see http://example.com/a{0}", "http://example.com/a{0}")]
		[TestCase ("see www.example.com/path.", "www.example.com/path")]
		[TestCase ("see file:///path/to/file.txt.", "file:///path/to/file.txt")]
		[TestCase ("see file:///path/to/file.txt, ok", "file:///path/to/file.txt")]
		[TestCase ("mail mailto:user@example.com?subject=hi.", "mailto:user@example.com?subject=hi")]
		[TestCase ("mail mailto:user@example.com?", "mailto:user@example.com")]
		[TestCase ("mail mailto:?subject=hi!", "mailto:?subject=hi")]
		[TestCase ("mail mailto:?.", null)]
		public void TestTrailingPunctuation (string input, string expected)
		{
			TestUrlScanner (input, expected);
		}

		[Test]
		public void TestContinueScanningAfterInvalidCandidate ()
		{
			TestUrlScanner ("Email me @ home or visit http://www.example.com", "http://www.example.com");
			TestUrlScanner ("Visit ftp. or www. or http:// or mailto: or file:// or http://example.com", "http://example.com");
		}

		[Test]
		public void TestPatternPrefixedByPartialPattern ()
		{
			TestUrlScanner ("wwww.example.com", "www.example.com");
			TestUrlScanner ("sftp.example.com", "ftp.example.com");
			TestUrlScanner ("hhttp://example.com", "http://example.com");
		}

		[TestCase ("sftp://example.com", "sftp://example.com")]
		[TestCase ("SFTP://example.com", "SFTP://example.com")]
		[TestCase ("https://example.com", "https://example.com")]
		[TestCase ("HtTpS://example.com", "HtTpS://example.com")]
		[TestCase ("WWW.example.com", "WWW.example.com")]
		[TestCase ("ftp:example.com", null)]
		[TestCase ("http:/example.com", null)]
		[TestCase ("http//example.com", null)]
		[TestCase ("http:", null)]
		[TestCase ("file:/", null)]
		[TestCase ("www", null)]
		[TestCase ("ww.example.com", null)]
		public void TestPatternMatching (string input, string expected)
		{
			TestUrlScanner (input, expected);
		}

		[Test]
		public void TestPatternsMustBeWithinRange ()
		{
			const string input = "http://example.com and www.example.org";
			char[] text = input.ToCharArray ();
			UrlMatch match;

			// A pattern that starts before the range must not be matched even though its anchor is within the range.
			int startIndex = 2;
			Assert.That (scanner.Scan (text, startIndex, text.Length - startIndex, out match), Is.True);
			Assert.That (new string (text, match.StartIndex, match.EndIndex - match.StartIndex), Is.EqualTo ("www.example.org"));

			// A pattern that ends after the range must not be matched even though its anchor is within the range.
			Assert.That (scanner.Scan (text, 0, 6, out _), Is.False);

			startIndex = input.IndexOf ("www.", StringComparison.Ordinal) + 1;
			Assert.That (scanner.Scan (text, startIndex, text.Length - startIndex, out _), Is.False);
		}

		[Test]
		public void TestAddPatternWithoutExactlyOneAnchor ()
		{
			var scanner = new UrlScanner ();

			Assert.Throws<ArgumentException> (() => scanner.Add (new UrlPattern (UrlPatternType.Web, "web", "")));
			Assert.Throws<ArgumentException> (() => scanner.Add (new UrlPattern (UrlPatternType.Web, "web.example:", "")));
		}

		[Test]
		public void TestDotAtomAddrspecAtStartOfText ()
		{
			TestUrlScanner ("a.b@example.com", "a.b@example.com");
			TestUrlScanner ("a.b.c@example.com", "a.b.c@example.com");
			TestUrlScanner (".b@example.com", null);
		}

		[Test]
		public void TestCultureInvariantPatternMatching ()
		{
			var culture = CultureInfo.CurrentCulture;

			try {
				CultureInfo.CurrentCulture = new CultureInfo ("tr-TR");

				TestUrlScanner ("FILE://server/share", "FILE://server/share");
				TestUrlScanner ("MAILTO:user@example.com", "MAILTO:user@example.com");
				TestUrlScanner ("f\u0130le://server/share", null);
				TestUrlScanner ("f\u0131le://server/share", null);
			} finally {
				CultureInfo.CurrentCulture = culture;
			}
		}

		[TestCase ("http://example.com\u00A0click here", "http://example.com")]
		[TestCase ("http://example.com\u2028next", "http://example.com")]
		[TestCase ("http://example.com\u3000next", "http://example.com")]
		[TestCase ("http://example.com/\u202Egpj.exe", "http://example.com/")]
		[TestCase ("http://example.com/\u200Bhidden", "http://example.com/")]
		[TestCase ("http://example.com/\u2066x", "http://example.com/")]
		[TestCase ("http://example.com/\uFEFFx", "http://example.com/")]
		[TestCase ("http://example.com/\u0085x", "http://example.com/")]
		[TestCase ("http://ex\u202Eample.com", "http://ex")]
		[TestCase ("user\u202E@example.com", null)]
		[TestCase ("user@example.com\u00A0x", "user@example.com")]
		[TestCase ("http://bücher.example/straße", "http://bücher.example/straße")]
		[TestCase ("josé@bücher.example", "josé@bücher.example")]
		public void TestNonAsciiCharacters (string input, string expected)
		{
			TestUrlScanner (input, expected);
		}

		[TestCase ("Email me at user@localhost please", null)]
		[TestCase ("Email me at user@example please", null)]
		[TestCase ("Email me at user@example.c please", null)]
		[TestCase ("Email me at user@example.c0m please", null)]
		[TestCase ("Email me at user@example.123 please", null)]
		[TestCase ("Email me at user@example.co please", "user@example.co")]
		[TestCase ("Email me at user@example.com please", "user@example.com")]
		[TestCase ("Email me at user@example.info please", "user@example.info")]
		[TestCase ("Email me at user@mail.example.co.uk please", "user@mail.example.co.uk")]
		[TestCase ("Email me at user@пример.рф please", "user@пример.рф")]
		[TestCase ("Email me at mailto:user@localhost please", "mailto:user@localhost")]
		[TestCase ("Email me at user@[127.0.0.1] please", "user@[127.0.0.1]")]
		public void TestAddrspecRequiresFullyQualifiedDomain (string input, string expected)
		{
			TestUrlScanner (input, expected);
		}

		[Test]
		public void TestDomainLengthLimit ()
		{
			var label = new string ('a', 63);
			var maxDomain = $"{label}.{label}.{label}.{new string ('a', 59)}.com"; // 255
			var tooLongDomain = $"{label}.{label}.{label}.{new string ('a', 60)}.com"; // 256

			Assert.That (maxDomain.Length, Is.EqualTo (255));

			TestUrlScanner ($"x user@{maxDomain} x", $"user@{maxDomain}");
			TestUrlScanner ($"x user@{tooLongDomain} x", null);
			TestUrlScanner ($"x user@{maxDomain}. x", $"user@{maxDomain}");

			TestUrlScanner ($"x mailto:user@{maxDomain} x", $"mailto:user@{maxDomain}");
			TestUrlScanner ($"x mailto:user@{tooLongDomain} x", null);

			// a domain that is too long must not be truncated at the maximum length
			var longDomain = $"{label}.{label}.{label}.{label}.com";

			TestUrlScanner ($"x user@{longDomain} x", null);
		}

		[Test]
		public void TestDomainLabelLengthLimit ()
		{
			var maxLabel = new string ('a', 63);
			var tooLongLabel = new string ('a', 64);

			TestUrlScanner ($"x user@{maxLabel}.com x", $"user@{maxLabel}.com");
			TestUrlScanner ($"x user@{tooLongLabel}.com x", null);
			TestUrlScanner ($"x user@example.{tooLongLabel}.com x", null);
			TestUrlScanner ($"x mailto:user@{tooLongLabel}.com x", null);

			TestUrlScanner ($"x http://{maxLabel}.com/path x", $"http://{maxLabel}.com/path");
			TestUrlScanner ($"x http://{tooLongLabel}.com/path x", null);
			TestUrlScanner ($"x http://example.{tooLongLabel}.com/path x", null);
			TestUrlScanner ($"x www.{tooLongLabel}.com x", null);
		}

		[Test]
		public void TestWebHostnameLengthLimit ()
		{
			var label = new string ('a', 63);
			var maxHost = $"{label}.{label}.{label}.{new string ('a', 59)}.com"; // 255
			var tooLongHost = $"{label}.{label}.{label}.{new string ('a', 60)}.com"; // 256

			Assert.That (maxHost.Length, Is.EqualTo (255));

			TestUrlScanner ($"x http://{maxHost}/path x", $"http://{maxHost}/path");
			TestUrlScanner ($"x http://{tooLongHost}/path x", null);
			TestUrlScanner ($"x https://{maxHost}:8080 x", $"https://{maxHost}:8080");

			// the "www." prefix is part of the hostname
			var maxWwwHost = "www." + maxHost.Substring (4);
			var tooLongWwwHost = "www." + tooLongHost.Substring (4);

			TestUrlScanner ($"x {maxWwwHost} x", maxWwwHost);
			TestUrlScanner ($"x {tooLongWwwHost} x", null);
		}

		[Test]
		public void TestLocalPartLengthLimit ()
		{
			var maxLocalPart = new string ('a', 64);
			var tooLongLocalPart = new string ('a', 65);

			TestUrlScanner ($"{maxLocalPart}@example.com", $"{maxLocalPart}@example.com");
			TestUrlScanner ($"x {tooLongLocalPart}@example.com", null);
			TestUrlScanner ($"x {new string ('a', 32)}.{new string ('b', 31)}@example.com", $"{new string ('a', 32)}.{new string ('b', 31)}@example.com");
			TestUrlScanner ($"x {new string ('a', 32)}.{new string ('b', 32)}@example.com", null);
			TestUrlScanner ($"x \"{new string ('a', 62)}\"@example.com", $"\"{new string ('a', 62)}\"@example.com");
			TestUrlScanner ($"x \"{new string ('a', 63)}\"@example.com", null);

			TestUrlScanner ($"mailto:{maxLocalPart}@example.com", $"mailto:{maxLocalPart}@example.com");
			TestUrlScanner ($"mailto:\"{new string ('a', 62)}\"@example.com", $"mailto:\"{new string ('a', 62)}\"@example.com");
			TestUrlScanner ($"mailto:\"{new string ('a', 63)}\"@example.com", null);
		}

		[Test]
		public void TestInvalidIPLiterals ()
		{
			TestUrlScanner ("a@[99999999999999999999.1.1.1]", null);
			TestUrlScanner ("a@[1.2.3.4444]", null);
			TestUrlScanner ("a@[IPv6:fffff::1]", null);
			TestUrlScanner ("a@[IPv6:" + new string ('f', 100000) + "]", null);
		}

		static void AssertLinearTime (Func<int, string> generate)
		{
			var scanner = new UrlScanner ();

			for (int i = 0; i < TextConverter.UrlPatterns.Count; i++)
				scanner.Add (TextConverter.UrlPatterns[i]);

			char[] text = generate (100000).ToCharArray ();
			int startIndex = 0;
			int candidates = 0;

			var stopwatch = System.Diagnostics.Stopwatch.StartNew ();

			while (startIndex < text.Length && scanner.Scan (text, startIndex, text.Length - startIndex, out var match)) {
				startIndex = match.EndIndex;
				candidates++;
			}

			stopwatch.Stop ();

			// Note: A quadratic algorithm would take minutes on these inputs.
			Assert.That (stopwatch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (10)), $"Scanning took {stopwatch.Elapsed} ({candidates} matches)");
		}

		[Test]
		public void TestAdversarialInputsAreLinear ()
		{
			AssertLinearTime (n => "\"" + string.Concat (Enumerable.Repeat ("\\\"@", n)));
			AssertLinearTime (n => string.Concat (Enumerable.Repeat ("a@-", n)));
			AssertLinearTime (n => new string ('a', n) + string.Concat (Enumerable.Repeat ("@-", n)));
			AssertLinearTime (n => string.Concat (Enumerable.Repeat ("mailto:\"x\\\"", n)));
			AssertLinearTime (n => "mailto:" + new string ('a', n) + string.Concat (Enumerable.Repeat ("mailto:", n)));
			AssertLinearTime (n => string.Concat (Enumerable.Repeat ("a.", n)) + string.Concat (Enumerable.Repeat ("@-", n)));
			AssertLinearTime (n => string.Concat (Enumerable.Repeat ("www.", n)));
			AssertLinearTime (n => string.Concat (Enumerable.Repeat ("http:// ", n)));
		}
	}
}
