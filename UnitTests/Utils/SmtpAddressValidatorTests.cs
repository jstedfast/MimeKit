//
// SmtpAddressValidatorTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class SmtpAddressValidatorTests
	{
		const SmtpAddressValidationOptions Iso2022Jp = SmtpAddressValidationOptions.AllowIso2022JpLocalPart;
		const SmtpAddressValidationOptions LengthLimits = SmtpAddressValidationOptions.EnforceLengthLimits;

		// "テスト用日本語メールテスト123" encoded as ISO-2022-JP within a quoted-string (the '\' of "K\" is quoted)
		const string Iso2022JpAddress = "\"test\u001B$BF|K\\\\8l%a!<%k%F%9%H\u001B(B123\"@iso2022.jp";

		static void AssertValidate (string address, SmtpAddressError expected)
		{
			bool valid = SmtpAddressValidator.TryValidate (address, out var error);

			Assert.That (error, Is.EqualTo (expected), "TryValidate error");
			Assert.That (valid, Is.EqualTo (expected == SmtpAddressError.None), "TryValidate result");
			Assert.That (SmtpAddressValidator.IsValid (address), Is.EqualTo (expected == SmtpAddressError.None), "IsValid");
		}

		static void AssertValidate (string address, SmtpAddressValidationOptions options, SmtpAddressError expected)
		{
			bool valid = SmtpAddressValidator.TryValidate (address, options, out var error);

			Assert.That (error, Is.EqualTo (expected), "TryValidate error");
			Assert.That (valid, Is.EqualTo (expected == SmtpAddressError.None), "TryValidate result");
			Assert.That (SmtpAddressValidator.IsValid (address, options), Is.EqualTo (expected == SmtpAddressError.None), "IsValid");
		}

		static string Repeat (string text, int count)
		{
			var builder = new StringBuilder (text.Length * count);

			for (int i = 0; i < count; i++)
				builder.Append (text);

			return builder.ToString ();
		}

		// Builds an ASCII domain of exactly the specified length (using labels of no more than 62 characters so that
		// tests can append a character to the domain without exceeding the label length limit).
		static string CreateAsciiDomain (int length)
		{
			var builder = new StringBuilder ();

			while (length > 0) {
				int labelLength = length <= 62 ? length : Math.Min (62, length - 2);

				builder.Append ('a', labelLength);
				length -= labelLength;

				if (length > 0) {
					builder.Append ('.');
					length--;
				}
			}

			return builder.ToString ();
		}

		[TestCase ("example@example.com", SmtpAddressError.None, TestName = "TestPlainAscii")]
		[TestCase ("first.last@example.com", SmtpAddressError.None, TestName = "TestDotString")]
		[TestCase ("!#$%&'*+-/=?^_`{|}~@example.com", SmtpAddressError.None, TestName = "TestAtextSpecials")]
		[TestCase ("\"john doe\"@example.com", SmtpAddressError.None, TestName = "TestQuotedString")]
		[TestCase ("\"a\\\"b\\\\c\"@example.com", SmtpAddressError.None, TestName = "TestQuotedPairs")]
		[TestCase ("\"(comment)\"@example.com", SmtpAddressError.None, TestName = "TestQuotedSpecials")]
		[TestCase ("\"\"@example.com", SmtpAddressError.None, TestName = "TestEmptyQuotedString")]
		[TestCase ("user@example", SmtpAddressError.None, TestName = "TestSingleLabelDomain")]
		[TestCase ("user@a-b.example.com", SmtpAddressError.None, TestName = "TestHyphenatedLabel")]
		[TestCase ("user@xn--gr-zia.example.com", SmtpAddressError.None, TestName = "TestALabelDomain")]
		[TestCase ("xn--gr-zia@example.com", SmtpAddressError.None, TestName = "TestALabelLocalPart")]
		[TestCase ("user@[192.168.0.1]", SmtpAddressError.None, TestName = "TestIPv4Literal")]
		[TestCase ("user@[IPv6:2001:db8::1]", SmtpAddressError.None, TestName = "TestIPv6Literal")]
		[TestCase ("user@[IPv6:::ffff:1.2.3.4]", SmtpAddressError.None, TestName = "TestIPv6v4Literal")]
		[TestCase ("user@[x-tag:content]", SmtpAddressError.None, TestName = "TestGeneralLiteral")]
		public void TestValid (string address, SmtpAddressError expected)
		{
			AssertValidate (address, expected);
		}

		[TestCase ("", SmtpAddressError.Syntax, TestName = "TestEmpty")]
		[TestCase ("user", SmtpAddressError.Syntax, TestName = "TestNoDomain")]
		[TestCase ("@example.com", SmtpAddressError.Syntax, TestName = "TestEmptyLocalPart")]
		[TestCase ("user@", SmtpAddressError.Syntax, TestName = "TestEmptyDomain")]
		[TestCase ("us..er@example.com", SmtpAddressError.Syntax, TestName = "TestConsecutiveDots")]
		[TestCase (".user@example.com", SmtpAddressError.Syntax, TestName = "TestLeadingDot")]
		[TestCase ("user.@example.com", SmtpAddressError.Syntax, TestName = "TestTrailingDot")]
		[TestCase ("user@example.com.", SmtpAddressError.Syntax, TestName = "TestTrailingDomainDot")]
		[TestCase ("user@-example.com", SmtpAddressError.Syntax, TestName = "TestLeadingHyphen")]
		[TestCase ("user@example-.com", SmtpAddressError.Syntax, TestName = "TestTrailingHyphen")]
		[TestCase ("us er@example.com", SmtpAddressError.Syntax, TestName = "TestUnquotedSpace")]
		[TestCase ("user@exa mple.com", SmtpAddressError.Syntax, TestName = "TestDomainSpace")]
		[TestCase ("user@@example.com", SmtpAddressError.Syntax, TestName = "TestTwoAtSigns")]
		[TestCase ("<user@example.com>", SmtpAddressError.Syntax, TestName = "TestAngleBrackets")]
		[TestCase ("user(comment)@example.com", SmtpAddressError.Syntax, TestName = "TestComment")]
		[TestCase ("\"user@example.com", SmtpAddressError.Syntax, TestName = "TestUnterminatedQuotedString")]
		[TestCase ("user@[192.168.0.1", SmtpAddressError.Syntax, TestName = "TestUnterminatedLiteral")]
		[TestCase ("user@[1.2.3]", SmtpAddressError.Syntax, TestName = "TestShortIPv4Literal")]
		[TestCase ("user@[IPv6:1::2::3]", SmtpAddressError.Syntax, TestName = "TestBadIPv6Literal")]
		[TestCase ("\"\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestEscapeWithoutOption")]
		[TestCase ("us\u0001er@example.com", SmtpAddressError.Syntax, TestName = "TestControlCharacter")]
		public void TestSyntax (string address, SmtpAddressError expected)
		{
			AssertValidate (address, expected);
		}

		[TestCase ("dømi@example.com", SmtpAddressError.NonAscii, TestName = "TestNonAsciiLocalPart")]
		[TestCase ("user@dømi.fo", SmtpAddressError.NonAscii, TestName = "TestNonAsciiDomain")]
		[TestCase ("dømi@dømi.fo", SmtpAddressError.NonAscii, TestName = "TestNonAsciiBoth")]
		[TestCase ("\"gr å\"@example.com", SmtpAddressError.NonAscii, TestName = "TestNonAsciiQuotedString")]
		[TestCase ("例@例え.jp", SmtpAddressError.NonAscii, TestName = "TestNonAsciiCjk")]
		[TestCase ("\U0001F600@example.com", SmtpAddressError.NonAscii, TestName = "TestNonAsciiSupplementary")]
		[TestCase ("gr å@example.com", SmtpAddressError.Syntax, TestName = "TestSyntaxBeforeNonAscii")]
		[TestCase ("grå..x@example.com", SmtpAddressError.Syntax, TestName = "TestSyntaxBeforeNonAsciiDots")]
		public void TestNonAscii (string address, SmtpAddressError expected)
		{
			AssertValidate (address, expected);
		}

		[TestCase (Iso2022JpAddress, SmtpAddressError.None, TestName = "TestIso2022JpExample")]
		[TestCase ("\"\u001B$BF|K\\\\8l\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpOnly")]
		[TestCase ("\"\u001B$@F|K\\\\8l\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpJisX0208_1978")]
		[TestCase ("\"\u001B(Jtaro\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpJisRoman")]
		[TestCase ("\"\u001B$BF|\u001B(B.\u001B$BK\\\\\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpMultipleRuns")]
		[TestCase ("\"\u001B$BF|\u001B(J\u001B$BK\\\\\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpSwitchWithinRun")]
		[TestCase ("\"\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpRedundantAsciiEscape")]
		[TestCase ("\"\u001B$B$\\\"\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpQuotedPairDquote")]
		[TestCase ("\"\u001B$B%a!&%k\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpKatakanaMiddleDot")]
		[TestCase ("\"\u001B$B/!\u001B(B\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpUnassignedNotDecoded")]
		[TestCase ("\"\u001B$BF|K\\\\8l\u001B(B\"@xn--wgv71a.jp", SmtpAddressError.None, TestName = "TestIso2022JpALabelDomain")]
		[TestCase ("\"plain\"@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpPlainQuotedString")]
		[TestCase ("plain@example.jp", SmtpAddressError.None, TestName = "TestIso2022JpPlainDotString")]
		[TestCase ("\"日本\u001B$B8l\u001B(B\"@example.jp", SmtpAddressError.NonAscii, TestName = "TestIso2022JpMixedWithUtf8")]
		[TestCase ("\"\u001B$BF|K\\\\8l\u001B(B\"@日本.jp", SmtpAddressError.NonAscii, TestName = "TestIso2022JpULabel")]
		[TestCase ("\u001B$BF|\u001B(B@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpUnquoted")]
		[TestCase ("\"\u001B$BF|\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpUnterminated")]
		[TestCase ("\"\u001B(Jtaro\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpUnterminatedJisRoman")]
		[TestCase ("\"\u001B$BF|K\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpOddByteCount")]
		[TestCase ("\"\u001B$BF|K\u001B$B\\\\8l\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpEscapeWithinCharacter")]
		[TestCase ("\"\u001B$B$\"\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpUnescapedDquote")]
		[TestCase ("\"\u001B$AF|\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpUnsupportedEscape")]
		[TestCase ("\"\u001B(I1\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpHalfwidthKatakanaEscape")]
		[TestCase ("\"\u001B$(DF|\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpJisX0212Escape")]
		[TestCase ("\"\u001B(\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpTruncatedEscape")]
		[TestCase ("\"\u001B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpLoneEscape")]
		[TestCase ("\"\u001B$BF \u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpSpaceInJisX0208")]
		[TestCase ("\"\u001B$BF\u0001\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpControlInJisX0208")]
		[TestCase ("\"\u001B$BFé\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpNonAsciiInJisX0208")]
		[TestCase ("\"\u001B(Jtaré\u001B(B\"@example.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpNonAsciiInJisRoman")]
		[TestCase ("\"\u001B$BF|\u001B(B\"@exa\u001Bmple.jp", SmtpAddressError.Syntax, TestName = "TestIso2022JpEscapeInDomain")]
		[TestCase ("\"\u001B$BF|\u001B(B\"@[\u001B(B1.2.3.4]", SmtpAddressError.Syntax, TestName = "TestIso2022JpEscapeInLiteral")]
		public void TestIso2022Jp (string address, SmtpAddressError expected)
		{
			AssertValidate (address, Iso2022Jp, expected);
		}

		[TestCase (Iso2022JpAddress)]
		[TestCase ("\"\u001B(Jtaro\u001B(B\"@example.jp")]
		[TestCase ("\"\u001B(B\"@example.jp")]
		public void TestIso2022JpNotAllowedByDefault (string address)
		{
			AssertValidate (address, SmtpAddressError.Syntax);
			AssertValidate (address, LengthLimits, SmtpAddressError.Syntax);
		}

		[Test]
		public void TestIso2022JpLengthLimit ()
		{
			// the escape sequences and quotes count toward the limit since they are part of the SMTP address
			AssertValidate ("\"\u001B$B" + Repeat ("F|", 28) + "\u001B(B\"@example.jp", Iso2022Jp | LengthLimits, SmtpAddressError.None);
			AssertValidate ("\"\u001B$B" + Repeat ("F|", 29) + "\u001B(B\"@example.jp", Iso2022Jp | LengthLimits, SmtpAddressError.LocalPartTooLong);
			AssertValidate ("\"\u001B$B" + Repeat ("F|", 29) + "\u001B(B\"@example.jp", Iso2022Jp, SmtpAddressError.None);

			// syntax errors take precedence over length limits
			AssertValidate ("\"\u001B$B" + Repeat ("F|", 29) + "F\u001B(B\"@example.jp", Iso2022Jp | LengthLimits, SmtpAddressError.Syntax);
		}

		[Test]
		public void TestLocalPartLengthLimit ()
		{
			AssertValidate (Repeat ("a", 64) + "@example.com", LengthLimits, SmtpAddressError.None);
			AssertValidate (Repeat ("a", 65) + "@example.com", LengthLimits, SmtpAddressError.LocalPartTooLong);

			// the quotes of a quoted-string count toward the limit
			AssertValidate ("\"" + Repeat ("a", 62) + "\"@example.com", LengthLimits, SmtpAddressError.None);
			AssertValidate ("\"" + Repeat ("a", 63) + "\"@example.com", LengthLimits, SmtpAddressError.LocalPartTooLong);

			// the limits are not enforced by default
			AssertValidate (Repeat ("a", 65) + "@example.com", SmtpAddressError.None);
		}

		[Test]
		public void TestLabelLengthLimit ()
		{
			AssertValidate ("u@" + Repeat ("a", 63) + ".com", LengthLimits, SmtpAddressError.None);
			AssertValidate ("u@" + Repeat ("a", 64) + ".com", LengthLimits, SmtpAddressError.LabelTooLong);
			AssertValidate ("u@xn--" + Repeat ("a", 59) + ".com", LengthLimits, SmtpAddressError.None);
			AssertValidate ("u@xn--" + Repeat ("a", 60) + ".com", LengthLimits, SmtpAddressError.LabelTooLong);
			AssertValidate ("u@" + Repeat ("a", 64) + ".com", SmtpAddressError.None);
		}

		[Test]
		public void TestDomainLengthLimit ()
		{
			AssertValidate ("u@" + CreateAsciiDomain (252), LengthLimits, SmtpAddressError.None);
			AssertValidate ("u@" + CreateAsciiDomain (254), LengthLimits, SmtpAddressError.DomainTooLong);
			AssertValidate ("u@" + CreateAsciiDomain (254), SmtpAddressError.None);
		}

		[Test]
		public void TestAddressLengthLimit ()
		{
			AssertValidate ("u@" + CreateAsciiDomain (253), LengthLimits, SmtpAddressError.AddressTooLong);
			AssertValidate (Repeat ("a", 64) + "@" + CreateAsciiDomain (189), LengthLimits, SmtpAddressError.None);
			AssertValidate (Repeat ("a", 64) + "@" + CreateAsciiDomain (190), LengthLimits, SmtpAddressError.AddressTooLong);

			// the address limit applies to address literals
			AssertValidate (Repeat ("a", 64) + "@[IPv6:0000:0000:0000:0000:0000:ffff:192.168.000.001]", LengthLimits, SmtpAddressError.None);

			// the limits are not enforced by default
			AssertValidate ("u@" + CreateAsciiDomain (253), SmtpAddressError.None);
		}

		[Test]
		public void TestLengthLimitPrecedence ()
		{
			// syntax errors take precedence over length limits
			AssertValidate (Repeat ("a", 65) + "@exa..mple.com", LengthLimits, SmtpAddressError.Syntax);

			// non-ASCII errors take precedence over length limits
			AssertValidate (Repeat ("ø", 65) + "@example.com", LengthLimits, SmtpAddressError.NonAscii);

			// the local-part is checked before the domain
			AssertValidate (Repeat ("a", 65) + "@" + Repeat ("a", 64) + ".com", LengthLimits, SmtpAddressError.LocalPartTooLong);

			// labels are checked before the domain
			AssertValidate ("u@" + CreateAsciiDomain (200) + "." + Repeat ("a", 64), LengthLimits, SmtpAddressError.LabelTooLong);
		}

		[Test]
		public void TestMailboxAddress ()
		{
			SmtpAddressError error;

			var mailbox = new MailboxAddress ("Gøril", "goril@example.com");
			Assert.That (SmtpAddressValidator.IsValid (mailbox), Is.True, "display name is not validated");
			Assert.That (SmtpAddressValidator.TryValidate (mailbox, out error), Is.True);
			Assert.That (error, Is.EqualTo (SmtpAddressError.None));

			mailbox = new MailboxAddress ("Gøril", "gøril@example.com");
			Assert.That (SmtpAddressValidator.IsValid (mailbox), Is.False);
			Assert.That (SmtpAddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpAddressError.NonAscii));

			mailbox = new MailboxAddress ("Local", "local");
			Assert.That (SmtpAddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpAddressError.Syntax));

			mailbox = new MailboxAddress ("Long", Repeat ("a", 65) + "@example.com");
			Assert.That (SmtpAddressValidator.IsValid (mailbox), Is.True);
			Assert.That (SmtpAddressValidator.IsValid (mailbox, LengthLimits), Is.False);
			Assert.That (SmtpAddressValidator.TryValidate (mailbox, LengthLimits, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpAddressError.LocalPartTooLong));

			// MimeKit preserves IDN-encoded domains as-is when parsing...
			mailbox = MailboxAddress.Parse ("<user@xn--gr-zia.example.com>");
			Assert.That (mailbox.Domain, Is.EqualTo ("xn--gr-zia.example.com"));
			Assert.That (SmtpAddressValidator.IsValid (mailbox), Is.True);

			// ...but an internationalized domain is not a valid SMTP address...
			mailbox = MailboxAddress.Parse ("<user@grå.example.com>");
			Assert.That (SmtpAddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpAddressError.NonAscii));

			// ...unless the IDN-encoded form of the address is validated instead
			Assert.That (SmtpAddressValidator.IsValid (mailbox.GetAddress (true)), Is.True);
		}
	}
}
