//
// SmtpUtf8AddressValidatorTests.cs
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
	public class SmtpUtf8AddressValidatorTests
	{
		static void AssertValidate (string address, SmtpUtf8AddressError expected)
		{
			bool valid = SmtpUtf8AddressValidator.TryValidate (address, out var error);

			Assert.That (error, Is.EqualTo (expected), "TryValidate error");
			Assert.That (valid, Is.EqualTo (expected == SmtpUtf8AddressError.None), "TryValidate result");
			Assert.That (SmtpUtf8AddressValidator.IsValid (address), Is.EqualTo (expected == SmtpUtf8AddressError.None), "IsValid");
		}

		// The following test vectors are from https://github.com/arnt/mailmaint-smtputf8 (tests.json),
		// published by Arnt Gulbrandsen alongside draft-ietf-mailmaint-smtputf8-syntax.
		[TestCase ("example@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt01PlainAscii")]
		[TestCase ("Example@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt02CasePreserved")]
		[TestCase ("dømi@dømi.fo", SmtpUtf8AddressError.None, TestName = "TestArnt03LatinULabel")]
		[TestCase ("example@xn--gr-zia.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestArnt04ALabelInDomain")]
		[TestCase ("xn--gr-zia@example.com", SmtpUtf8AddressError.ALabel, TestName = "TestArnt05ALabelInLocalPart")]
		[TestCase ("com.xn--gr-zia@example.com", SmtpUtf8AddressError.ALabel, TestName = "TestArnt06ALabelInSecondAtom")]
		[TestCase ("XN--GR-ZIA@example.com", SmtpUtf8AddressError.ALabel, TestName = "TestArnt07UpperCaseALabel")]
		[TestCase ("xn-gr@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt08SingleHyphen")]
		[TestCase ("\u200E4@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt09LeftToRightMark")]
		[TestCase ("\u061C@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt10ArabicLetterMark")]
		[TestCase ("\u200E@\u200F.\u200E", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt11BidiControls")]
		[TestCase ("ali\u202Emoc.elpmaxe@kravdraa.ec", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt12RightToLeftOverride")]
		[TestCase ("🐪@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt13Symbol")]
		[TestCase ("admin\u3164@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt14HangulFiller")]
		[TestCase ("\uFF21\uFF24\uFF2D\uFF29\uFF2E@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt15Fullwidth")]
		[TestCase ("名字\uFE0F@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt16VariationSelector")]
		[TestCase ("م\u0640حمد@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt17Tatweel")]
		[TestCase ("\u1100\u1161\u11A8@example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestArnt18ConjoiningJamo")]
		[TestCase ("각@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt19PrecomposedHangul")]
		[TestCase ("IВM@dømi.fo", SmtpUtf8AddressError.MixedScripts, TestName = "TestArnt20CyrillicAndLatin")]
		[TestCase ("阿Q正传@dømi.fo", SmtpUtf8AddressError.MixedScripts, TestName = "TestArnt21HanAndLatin")]
		[TestCase ("Толстой@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt22Cyrillic")]
		[TestCase ("名字@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt23Han")]
		[TestCase ("阿Q正传@阿Q正传.example", SmtpUtf8AddressError.None, TestName = "TestArnt24HanAndAscii")]
		[TestCase ("名字@例子.中国", SmtpUtf8AddressError.None, TestName = "TestArnt25AllHan")]
		[TestCase ("info@例子.中国", SmtpUtf8AddressError.None, TestName = "TestArnt26AsciiLocalPartHanDomain")]
		[TestCase ("उदाहरण@उदाहरण.भारत", SmtpUtf8AddressError.None, TestName = "TestArnt27Devanagari")]
		[TestCase ("gøril@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt28LatinLocalPart")]
		[TestCase ("\u0627@2\u0627.\u0627", SmtpUtf8AddressError.None, TestName = "TestArnt29ArabicWithDigits")]
		[TestCase ("\u0627@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt30ArabicWithAscii")]
		[TestCase ("café@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt31Nfc")]
		[TestCase ("cafe\u0301@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt32Nfd")]
		// Note: tests.json considers this valid, but the empty atoms are not permitted by the rfc5321 Dot-string syntax.
		[TestCase ("..@example.com", SmtpUtf8AddressError.Syntax, TestName = "TestArnt33EmptyAtoms")]
		[TestCase ("\"john.doe\"@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt34QuotedString")]
		[TestCase ("\"john doe\"@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt35QuotedStringWithSpace")]
		// Note: the TAB is not permitted by the rfc5321 Atom syntax, so this fails the syntax check before the code point check.
		[TestCase ("grå\u0009\u200B\uFEFF\u00AD\u00A0\u2003\u3000@grå.org", SmtpUtf8AddressError.Syntax, TestName = "TestArnt36InvisibleCharacters")]
		[TestCase ("cel·la@example.com", SmtpUtf8AddressError.None, TestName = "TestArnt37MiddleDotBetweenL")]
		[TestCase ("cex·xa@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestArnt38MiddleDotNotBetweenL")]
		[TestCase ("\u0660\u06F0@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestArnt39MixedArabicIndicDigits")]
		public void TestArntTestVectors (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, expected);
		}

		[TestCase ("")]
		[TestCase ("user")]
		[TestCase ("@example.com")]
		[TestCase ("user@")]
		[TestCase ("user@@example.com")]
		[TestCase (".user@example.com")]
		[TestCase ("user.@example.com")]
		[TestCase ("us..er@example.com")]
		[TestCase ("us er@example.com")]
		[TestCase ("us\ter@example.com")]
		[TestCase ("us(comment)er@example.com")]
		[TestCase ("user@exa mple.com")]
		[TestCase ("user@.example.com")]
		[TestCase ("user@example..com")]
		[TestCase ("user@example.com.")]
		[TestCase ("user@-example.com")]
		[TestCase ("user@example-.com")]
		[TestCase ("user@ex_ample.com")]
		[TestCase ("user@example.com ")]
		[TestCase (" user@example.com")]
		[TestCase ("<user@example.com>")]
		[TestCase ("User <user@example.com>")]
		[TestCase ("\"unterminated@example.com")]
		[TestCase ("\"a\"b@example.com")]
		[TestCase ("a\"b\"@example.com")]
		[TestCase ("\"a\tb\"@example.com")]
		[TestCase ("\"a\u007Fb\"@example.com")]
		[TestCase ("\"a\\\u0001b\"@example.com")]
		[TestCase ("\"a\\")]
		[TestCase ("\"a\"")]
		[TestCase ("xn--a@-example.com")]
		public void TestSyntaxErrors (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.Syntax);
		}

		[TestCase ("a@b")]
		[TestCase ("a.b.c@example.com")]
		[TestCase ("!#$%&'*+-/=?^_`{|}~@example.com")]
		[TestCase ("user@1example.com")]
		[TestCase ("user@ex-am--ple.com")]
		[TestCase ("\"\"@example.com")]
		[TestCase ("\"a\\\"b\"@example.com")]
		[TestCase ("\"a\\\\b\"@example.com")]
		[TestCase ("\"a@b\"@example.com")]
		[TestCase ("\"名字 x\"@example.com")]
		[TestCase ("\"xn--gr-zia\"@example.com")]
		[TestCase ("xn-@example.com")]
		[TestCase ("x-n--@example.com")]
		[TestCase ("straße@example.com")]
		[TestCase ("ς@example.com")]
		[TestCase ("〇@example.com")]
		[TestCase ("𠀀@example.com")]
		[TestCase ("Ωμέγα@παράδειγμα.ελ")]
		public void TestValidAddresses (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.None);
		}

		[TestCase ("xn--@example.com")]
		[TestCase ("Xn--abc@example.com")]
		[TestCase ("xN--abc@example.com")]
		[TestCase ("user@xn--abc")]
		[TestCase ("user@example.XN--abc")]
		[TestCase ("a.b.xn--c@example.com")]
		public void TestALabels (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.ALabel);
		}

		[TestCase ("user@[192.168.0.1]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[0.0.0.0]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[255.255.255.255]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[001.002.003.004]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[256.0.0.1]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[1.2.3]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[1.2.3.4.5]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[1234.1.1.1]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[1..2.3]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[1.2.3.4.]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[192.168.0.1", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[192.168.0.1]x", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[192.168.0.1].com", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:::]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:::1]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1::]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:2001:db8::1]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[ipv6:2001:DB8::1]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:7:8]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1:2:3::4:5:6]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:1.2.3.4]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:::ffff:1.2.3.4]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1:2::3:4:1.2.3.4]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:7]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:7:8:9]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1::2:3:4:5:6:7]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:7::]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1::2::3]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:::2]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:12345::1]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:2:3:4:5:1.2.3.4]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:2:3::4:5:1.2.3.4]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1.2.3.4::]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:1:2:3:4:5:6:7:]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6::1:2:3:4:5:6:7]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:2001:db8::g]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:2001:db8::1%eth0]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[IPv6:]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[x-tag:content]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[tag:a:b!~]", SmtpUtf8AddressError.None)]
		[TestCase ("user@[:content]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag-:content]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[t_g:content]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag:]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag:a\\b]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag:a[b]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag:a b]", SmtpUtf8AddressError.Syntax)]
		[TestCase ("user@[tag:ø]", SmtpUtf8AddressError.Syntax)]
		public void TestAddressLiterals (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, expected);
		}

		[TestCase ("a\uD800@example.com", TestName = "TestLoneHighSurrogate")]
		[TestCase ("a\uDC00@example.com", TestName = "TestLoneLowSurrogate")]
		[TestCase ("a\uDC00\uD800@example.com", TestName = "TestReversedSurrogates")]
		[TestCase ("user@example.com\uD800", TestName = "TestTrailingHighSurrogate")]
		[TestCase ("\"a\u00A0b\"@example.com", TestName = "TestNoBreakSpaceInQuotedString")]
		[TestCase ("\uFB01@example.com", TestName = "TestLigature")]
		[TestCase ("\u01C5@example.com", TestName = "TestTitlecase")]
		[TestCase ("a\u00B2@example.com", TestName = "TestSuperscript")]
		[TestCase ("\u2160@example.com", TestName = "TestRomanNumeral")]
		[TestCase ("\u3031@example.com", TestName = "TestVerticalKanaRepeatMark")]
		[TestCase ("\u0378@example.com", TestName = "TestUnassigned")]
		[TestCase ("a\uFDD0@example.com", TestName = "TestNoncharacter")]
		[TestCase ("a\uE000@example.com", TestName = "TestPrivateUse")]
		[TestCase ("a\u0085@example.com", TestName = "TestC1Control")]
		[TestCase ("a\u2028@example.com", TestName = "TestLineSeparator")]
		[TestCase ("a\u00A9@example.com", TestName = "TestCopyrightSign")]
		public void TestDisallowedCodePoints (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.DisallowedCodePoint);
		}

		// RFC 5892, Appendix A.1 (ZERO WIDTH NON-JOINER)
		[TestCase ("\u0915\u094D\u200C\u0937@example.com", SmtpUtf8AddressError.None, TestName = "TestZwnjAfterVirama")]
		[TestCase ("\u0628\u200C\u0628@example.com", SmtpUtf8AddressError.None, TestName = "TestZwnjBetweenDualJoining")]
		[TestCase ("\u0628\u064E\u200C\u064E\u0627@example.com", SmtpUtf8AddressError.None, TestName = "TestZwnjWithTransparent")]
		[TestCase ("\u0627\u200C\u0628@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwnjAfterRightJoining")]
		[TestCase ("\u0628\u200C\u0627\u200C\u0628@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestSecondZwnjAfterRightJoining")]
		[TestCase ("\u200C\u0628@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwnjAtStart")]
		[TestCase ("\u0628\u200C@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwnjBeforeAt")]
		[TestCase ("\u0628\u064E\u200C@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwnjNoRightJoining")]
		[TestCase ("a\u200Cb@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwnjBetweenLatin")]
		[TestCase ("user@\u0628\u200C\u0628.example", SmtpUtf8AddressError.None, TestName = "TestZwnjInDomain")]
		// RFC 5892, Appendix A.2 (ZERO WIDTH JOINER)
		[TestCase ("\u0915\u094D\u200D\u0937@example.com", SmtpUtf8AddressError.None, TestName = "TestZwjAfterVirama")]
		[TestCase ("\u0628\u200D\u0628@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwjBetweenDualJoining")]
		[TestCase ("a\u200Db@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwjBetweenLatin")]
		[TestCase ("\u200D@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestZwjAtStart")]
		// RFC 5892, Appendix A.3 (MIDDLE DOT)
		[TestCase ("l\u00B7l@example.com", SmtpUtf8AddressError.None, TestName = "TestMiddleDotBetweenL")]
		[TestCase ("user@col\u00B7legi.cat", SmtpUtf8AddressError.None, TestName = "TestMiddleDotInDomain")]
		[TestCase ("ceL\u00B7La@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestMiddleDotBetweenUpperCaseL")]
		[TestCase ("\u00B7l@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestMiddleDotAtStart")]
		[TestCase ("l\u00B7@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestMiddleDotBeforeAt")]
		// RFC 5892, Appendix A.4 (GREEK LOWER NUMERAL SIGN)
		[TestCase ("\u0375\u03B1@example.com", SmtpUtf8AddressError.None, TestName = "TestKeraiaBeforeGreek")]
		[TestCase ("\u0375a@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestKeraiaBeforeLatin")]
		[TestCase ("\u03B1\u0375@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestKeraiaAtEnd")]
		// RFC 5892, Appendix A.5 and A.6 (HEBREW PUNCTUATION GERESH and GERSHAYIM)
		[TestCase ("\u05D0\u05F3@example.com", SmtpUtf8AddressError.None, TestName = "TestGereshAfterHebrew")]
		[TestCase ("\u05D0\u05F4\u05D1@example.com", SmtpUtf8AddressError.None, TestName = "TestGershayimAfterHebrew")]
		[TestCase ("a\u05F3@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestGereshAfterLatin")]
		[TestCase ("\u05F4\u05D0@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestGershayimAtStart")]
		// RFC 5892, Appendix A.7 (KATAKANA MIDDLE DOT)
		[TestCase ("\u30A2\u30FB\u30A4@example.com", SmtpUtf8AddressError.None, TestName = "TestKatakanaMiddleDotWithKatakana")]
		[TestCase ("\u30FB@\u4F8B.example", SmtpUtf8AddressError.None, TestName = "TestKatakanaMiddleDotWithHanElsewhere")]
		[TestCase ("a\u30FBb@example.com", SmtpUtf8AddressError.ContextRule, TestName = "TestKatakanaMiddleDotWithLatin")]
		// RFC 5892, Appendix A.8 and A.9 (ARABIC-INDIC DIGITS and EXTENDED ARABIC-INDIC DIGITS)
		[TestCase ("\u0660\u0669@example.com", SmtpUtf8AddressError.None, TestName = "TestArabicIndicDigits")]
		[TestCase ("\u06F0\u06F9@example.com", SmtpUtf8AddressError.None, TestName = "TestExtendedArabicIndicDigits")]
		[TestCase ("\u0660@\u06F0.example", SmtpUtf8AddressError.ContextRule, TestName = "TestMixedArabicIndicDigitsAcrossAt")]
		public void TestContextRules (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, expected);
		}

		// Note: draft-ietf-mailmaint-smtputf8-syntax counts each script separately, so even Japanese text that
		// mixes Han with Hiragana or Katakana is considered to use more than one script.
		[TestCase ("名前ひらがな@example.com", TestName = "TestHanAndHiragana")]
		[TestCase ("ひらがなカタカナ@example.com", TestName = "TestHiraganaAndKatakana")]
		[TestCase ("Ωmega@ø.example", TestName = "TestGreekAndLatin")]
		[TestCase ("user@\u0627.\u05D0", TestName = "TestArabicAndHebrewInDomain")]
		[TestCase ("\"ø Ω\"@example.com", TestName = "TestMixedScriptsInQuotedString")]
		public void TestMixedScripts (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.MixedScripts);
		}

		[TestCase ("cafe\u0301\u00E9@example.com", TestName = "TestInheritedIsNeutral")]
		[TestCase ("\u30FC\u30A2@example.com", TestName = "TestCommonIsNeutral")]
		[TestCase ("ø@ø.example", TestName = "TestSameScriptInLocalPartAndDomain")]
		public void TestNeutralScripts (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.None);
		}

		[Test]
		public void TestErrorPrecedence ()
		{
			// Syntax is reported before an A-label
			AssertValidate ("xn--abc@example..com", SmtpUtf8AddressError.Syntax);

			// An A-label is reported before a disallowed code point
			AssertValidate ("xn--abc\u200E@example.com", SmtpUtf8AddressError.ALabel);

			// A disallowed code point is reported before a context rule violation or mixed scripts
			AssertValidate ("IВM\u200D\u3164@dømi.fo", SmtpUtf8AddressError.DisallowedCodePoint);

			// A context rule violation is reported before mixed scripts
			AssertValidate ("IВM\u200D@dømi.fo", SmtpUtf8AddressError.ContextRule);
		}

		[Test]
		public void TestLargeInputs ()
		{
			// These inputs would take quadratic time if the context rules were evaluated naively.
			var builder = new StringBuilder ();

			for (int i = 0; i < 50000; i++)
				builder.Append ('\u30FB');
			builder.Append ("@example.com");

			AssertValidate (builder.ToString (), SmtpUtf8AddressError.ContextRule);

			builder.Clear ();
			for (int i = 0; i < 10000; i++) {
				builder.Append ('\u0628');
				builder.Append ('\u064E', 10);
				builder.Append ('\u200C');
				builder.Append ('\u064E', 10);
			}
			builder.Append ('\u0628');
			builder.Append ("@example.com");

			AssertValidate (builder.ToString (), SmtpUtf8AddressError.None);

			builder.Clear ();
			builder.Append ('\u0628');
			builder.Append ('\u064E', 50000);
			builder.Append ('\u200C', 50000);
			builder.Append ('\u064E', 50000);
			builder.Append ('\u0628');
			builder.Append ("@example.com");

			AssertValidate (builder.ToString (), SmtpUtf8AddressError.ContextRule);

			builder.Clear ();
			for (int i = 0; i < 50000; i++)
				builder.Append ("ab.");
			builder.Append ("c@");
			for (int i = 0; i < 50000; i++)
				builder.Append ("ø.");
			builder.Append ("example");

			AssertValidate (builder.ToString (), SmtpUtf8AddressError.None);
		}

		[Test]
		public void TestMailboxAddress ()
		{
			SmtpUtf8AddressError error;

			var mailbox = new MailboxAddress ("Gøril", "gøril@example.com");
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox), Is.True);
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.True);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.None));

			// The display name is not validated
			mailbox = new MailboxAddress ("\u202E\U0001F42A", "user@example.com");
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox), Is.True);

			mailbox = MailboxAddress.Parse ("IBM <IВM@dømi.fo>");
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox), Is.False);
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.MixedScripts));

			mailbox = new MailboxAddress ("Local", "local");
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.Syntax));

			mailbox = new MailboxAddress ("ALabel", "xn--gr-zia@example.com");
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.ALabel));

			// MimeKit decodes IDN-encoded domains when parsing, so the Unicode form of the domain is validated instead.
			mailbox = MailboxAddress.Parse ("<user@xn--gr-zia.example.com>");
			Assert.That (mailbox.Domain, Is.EqualTo ("grå.example.com"));
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox), Is.True);

			// ...which means that the decoded domain is still subject to all of the rules (xn--wxa9v is Cyrillic + Greek)
			mailbox = MailboxAddress.Parse ("<user@xn--wxa9v.example.com>");
			Assert.That (mailbox.Domain, Is.EqualTo ("жλ.example.com"));
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.MixedScripts));

			// A domain that cannot be decoded is left in its encoded form and is rejected as an A-label.
			mailbox = MailboxAddress.Parse ("<user@xn--a.example.com>");
			Assert.That (mailbox.Domain, Is.EqualTo ("xn--a.example.com"));
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.ALabel));
		}

		static void AssertValidate (string address, SmtpUtf8AddressValidationOptions options, SmtpUtf8AddressError expected)
		{
			bool valid = SmtpUtf8AddressValidator.TryValidate (address, options, out var error);

			Assert.That (error, Is.EqualTo (expected), "TryValidate error");
			Assert.That (valid, Is.EqualTo (expected == SmtpUtf8AddressError.None), "TryValidate result");
			Assert.That (SmtpUtf8AddressValidator.IsValid (address, options), Is.EqualTo (expected == SmtpUtf8AddressError.None), "IsValid");
		}

		[TestCase ("user@xn--gr-zia.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnLatin")]
		[TestCase ("user@XN--GR-ZIA.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnUpperCasePrefix")]
		[TestCase ("user@grå.xn--e1afmkfd.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestDecodeIdnMixedWithULabel")]
		[TestCase ("user@xn--e1afmkfd.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnCyrillic")]
		[TestCase ("user@xn--wxa9v.example.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestDecodeIdnMixedScripts")]
		[TestCase ("user@xn--g6h.example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestDecodeIdnSymbol")]
		[TestCase ("user@xn--i-7iq.example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestDecodeIdnAsciiAndSymbol")]
		[TestCase ("user@xn--aypal-uye.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnHomographIgnoringAscii")]
		[TestCase ("user@xn--a.example.com", SmtpUtf8AddressError.DisallowedCodePoint, TestName = "TestDecodeIdnControlCharacter")]
		[TestCase ("user@xn--ihqwcrb4cv8a8dqg056pqjye.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnRfc3492SimplifiedChinese")]
		[TestCase ("user@xn--ihqwctvzc91f659drss3x8bo0yb.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnRfc3492TraditionalChinese")]
		[TestCase ("user@xn--b1abfaaepdrnnbgefbaDotcwatmq2g4l.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnRfc3492Russian")]
		[TestCase ("user@xn--Proprostnemluvesky-uyb24dma41a.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnRfc3492Czech")]
		[TestCase ("user@xn--j50ic.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnSupplementaryPlane")]
		[TestCase ("user@XN--TDA.example.com", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnUppercase")]
		[TestCase ("user@xn---tda.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnNonCanonical")]
		[TestCase ("user@xn----eha.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnLeadingHyphen")]
		[TestCase ("user@xn----dha.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnTrailingHyphenInULabel")]
		[TestCase ("user@xn--ab---3ra.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnHyphensInThirdAndFourthPositions")]
		[TestCase ("user@xn--bb0c.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnSurrogate")]
		[TestCase ("user@xn--tdaü.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnNonAscii")]
		[TestCase ("user@xn--99999999999.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnOverflowALabel")]
		[TestCase ("user@xn--abc-.example.com", SmtpUtf8AddressError.Syntax, TestName = "TestDecodeIdnTrailingHyphen")]
		[TestCase ("xn--gr-zia@example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnLocalPartNotDecoded")]
		[TestCase ("xn--gr-zia@xn--gr-zia.example.com", SmtpUtf8AddressError.ALabel, TestName = "TestDecodeIdnLocalPartPrecedence")]
		[TestCase ("user@[192.168.0.1]", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnAddressLiteral")]
		[TestCase ("user@xn--gr-zia", SmtpUtf8AddressError.None, TestName = "TestDecodeIdnSingleLabel")]
		[TestCase ("us..er@xn--gr-zia.example.com", SmtpUtf8AddressError.Syntax, TestName = "TestDecodeIdnSyntaxPrecedence")]
		public void TestDecodeIdn (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, SmtpUtf8AddressValidationOptions.DecodeIdn, expected);
		}

		[Test]
		public void TestDecodeIdnLongALabel ()
		{
			// an A-label is limited to 63 octets (rfc5890), even when length limits are not being enforced
			AssertValidate ("user@xn--" + Repeat ("a", 60) + ".example.com", SmtpUtf8AddressValidationOptions.DecodeIdn, SmtpUtf8AddressError.ALabel);
		}

		[TestCase ("paypal@рaypal.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinHomographDomain")]
		[TestCase ("рaypal@example.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinHomographLocalPart")]
		[TestCase ("info@москва-shop.ru", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinMixedLabel")]
		[TestCase ("иван@example.com", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinCyrillicLocalPart")]
		[TestCase ("user@пример.com", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinCyrillicLabel")]
		[TestCase ("иван@пример.com", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinCyrillicLocalPartAndLabel")]
		[TestCase ("dømi@dømi.fo", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinLatin")]
		[TestCase ("иван.ivan@example.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinMixedLocalPartAtoms")]
		[TestCase ("иван123@example.com", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinDigits")]
		[TestCase ("\"иван@ivan\"@example.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinQuotedLocalPart")]
		[TestCase ("user@[IPv6:::1]", SmtpUtf8AddressError.None, TestName = "TestAsciiIsLatinAddressLiteral")]
		[TestCase ("user@жλ.example.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestAsciiIsLatinNonAsciiMixedScripts")]
		public void TestAsciiIsLatin (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, SmtpUtf8AddressValidationOptions.AsciiIsLatin, expected);
		}

		[TestCase ("paypal@рaypal.com", TestName = "TestAsciiIgnoredHomographDomain")]
		[TestCase ("info@москва-shop.ru", TestName = "TestAsciiIgnoredMixedLabel")]
		[TestCase ("иван.ivan@example.com", TestName = "TestAsciiIgnoredMixedLocalPartAtoms")]
		public void TestAsciiIgnoredByDefault (string address)
		{
			AssertValidate (address, SmtpUtf8AddressError.None);
		}

		[TestCase ("user@xn--aypal-uye.com", SmtpUtf8AddressError.MixedScripts, TestName = "TestSpoofHomographALabel")]
		[TestCase ("user@xn---shop-3ven7dru7a.ru", SmtpUtf8AddressError.MixedScripts, TestName = "TestSpoofMixedALabel")]
		[TestCase ("user@xn--e1afmkfd.com", SmtpUtf8AddressError.None, TestName = "TestSpoofCyrillicALabel")]
		[TestCase ("иван@xn--e1afmkfd.com", SmtpUtf8AddressError.None, TestName = "TestSpoofCyrillicLocalPartAndALabel")]
		[TestCase ("user@example.com", SmtpUtf8AddressError.None, TestName = "TestSpoofAscii")]
		public void TestSpoofDetection (string address, SmtpUtf8AddressError expected)
		{
			AssertValidate (address, SmtpUtf8AddressValidationOptions.DecodeIdn | SmtpUtf8AddressValidationOptions.AsciiIsLatin, expected);
		}

		[Test]
		public void TestMailboxAddressWithOptions ()
		{
			const SmtpUtf8AddressValidationOptions options = SmtpUtf8AddressValidationOptions.DecodeIdn | SmtpUtf8AddressValidationOptions.AsciiIsLatin;
			SmtpUtf8AddressError error;
			MailboxAddress mailbox;

			mailbox = MailboxAddress.Parse ("PayPal <service@xn--aypal-uye.com>");
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox), Is.True);
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox, options), Is.False);
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, options, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.MixedScripts));

			mailbox = MailboxAddress.Parse ("<user@xn--99999999999.example.com>");
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, options, out error), Is.False);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.ALabel));

			mailbox = MailboxAddress.Parse ("<иван@xn--e1afmkfd.com>");
			Assert.That (SmtpUtf8AddressValidator.IsValid (mailbox, options), Is.True);
			Assert.That (SmtpUtf8AddressValidator.TryValidate (mailbox, options, out error), Is.True);
			Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.None));
		}

		const SmtpUtf8AddressValidationOptions LengthLimits = SmtpUtf8AddressValidationOptions.EnforceLengthLimits;

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

		[Test]
		public void TestLocalPartLengthLimit ()
		{
			AssertValidate (Repeat ("a", 64) + "@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("a", 65) + "@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);

			// the limit is in UTF-8 octets, not characters
			AssertValidate (Repeat ("ø", 32) + "@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("ø", 32) + "a@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);
			AssertValidate (Repeat ("ж", 21) + "a@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("例", 21) + "a@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("例", 21) + "ab@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);
			AssertValidate (Repeat ("\U00020000", 16) + "@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("\U00020000", 16) + "a@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);

			// the quotes of a quoted-string count toward the limit
			AssertValidate ("\"" + Repeat ("a", 62) + "\"@example.com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate ("\"" + Repeat ("a", 63) + "\"@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);

			// the limits are not enforced by default
			AssertValidate (Repeat ("a", 65) + "@example.com", SmtpUtf8AddressError.None);
		}

		[Test]
		public void TestLabelLengthLimit ()
		{
			AssertValidate ("user@" + Repeat ("a", 63) + ".com", LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate ("user@" + Repeat ("a", 64) + ".com", LengthLimits, SmtpUtf8AddressError.LabelTooLong);
			AssertValidate ("user@example." + Repeat ("a", 64), LengthLimits, SmtpUtf8AddressError.LabelTooLong);

			// an A-label is measured as written
			AssertValidate ("user@xn--" + Repeat ("a", 59) + ".com", LengthLimits, SmtpUtf8AddressError.ALabel);
			AssertValidate ("user@xn--" + Repeat ("a", 60) + ".com", LengthLimits, SmtpUtf8AddressError.LabelTooLong);

			// a label with more code points than the limit is too long regardless of how it encodes
			AssertValidate ("user@" + Repeat ("ж", 64) + ".com", LengthLimits, SmtpUtf8AddressError.LabelTooLong);
			AssertValidate ("user@" + Repeat ("ж", 100000) + ".com", LengthLimits, SmtpUtf8AddressError.LabelTooLong);

			// the label limit does not apply to address literals
			AssertValidate ("user@[IPv6:0000:0000:0000:0000:0000:ffff:192.168.000.001]", LengthLimits, SmtpUtf8AddressError.None);

			// the limits are not enforced by default
			AssertValidate ("user@" + Repeat ("a", 64) + ".com", SmtpUtf8AddressError.None);
		}

		// Verifies that the A-label length of a U-label is computed exactly by growing labels until
		// IdnMapping.GetAscii() fails because the A-label would exceed 63 octets.
		[TestCase ("ж", "", TestName = "TestLabelLengthBoundaryCyrillic")]
		[TestCase ("ж", "a", TestName = "TestLabelLengthBoundaryCyrillicAndAscii")]
		[TestCase ("ø", "a", TestName = "TestLabelLengthBoundaryLatin")]
		[TestCase ("例", "", TestName = "TestLabelLengthBoundaryHan")]
		[TestCase ("例", "え", TestName = "TestLabelLengthBoundaryHanAndHiragana")]
		[TestCase ("\U00020000", "", TestName = "TestLabelLengthBoundarySupplementary")]
		[TestCase ("ä", "zz9", TestName = "TestLabelLengthBoundaryMixed")]
		public void TestLabelLengthBoundary (string first, string second)
		{
			var idn = new System.Globalization.IdnMapping ();
			var builder = new StringBuilder ();
			string previous = null;

			for (int i = 0; i < 64; i++) {
				builder.Append (i % 2 == 0 || second.Length == 0 ? first : second);

				var label = builder.ToString ();

				try {
					var ascii = idn.GetAscii (label);

					Assert.That (ascii.Length, Is.LessThanOrEqualTo (63));
					previous = label;
				} catch (ArgumentException) {
					Assert.That (previous, Is.Not.Null);

					// Note: only check the length errors since the label may not satisfy the other rules
					SmtpUtf8AddressValidator.TryValidate ("user@" + previous + ".com", LengthLimits, out var error);
					Assert.That (error, Is.Not.EqualTo (SmtpUtf8AddressError.LabelTooLong), previous);

					SmtpUtf8AddressValidator.TryValidate ("user@" + label + ".com", LengthLimits, out error);
					Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.LabelTooLong), label);
					return;
				}
			}

			Assert.Fail ("Expected IdnMapping.GetAscii() to fail before the label reached 64 code points.");
		}

		// Verifies that the A-label length of random U-labels is computed exactly by padding the domain to exactly
		// the domain length limit using the length of the A-label produced by IdnMapping.GetAscii().
		[Test]
		public void TestDomainLengthMatchesIdnMapping ()
		{
			string[] alphabet = { "a", "z", "0", "-", "ø", "é", "ж", "λ", "例", "え", "한", "ا", "क", "\U00020000", "\U0001F600" };
			var idn = new System.Globalization.IdnMapping ();
			var random = new Random (6531);
			int tested = 0;

			while (tested < 500) {
				var builder = new StringBuilder ();
				int length = random.Next (1, 30);

				for (int i = 0; i < length; i++)
					builder.Append (alphabet[random.Next (alphabet.Length)]);

				var label = builder.ToString ();
				string ascii;

				if (label[0] == '-' || label[label.Length - 1] == '-')
					continue;

				// IdnMapping may map the label (e.g. NFC), in which case its A-label is not of the label as written
				try {
					ascii = idn.GetAscii (label);

					if (!ascii.StartsWith ("xn--", StringComparison.Ordinal) || idn.GetUnicode (ascii) != label)
						continue;
				} catch (ArgumentException) {
					continue;
				}

				var filler = CreateAsciiDomain (253 - ascii.Length - 1);

				SmtpUtf8AddressValidator.TryValidate ("u@" + label + "." + filler, LengthLimits, out var error);
				Assert.That (error, Is.Not.EqualTo (SmtpUtf8AddressError.DomainTooLong).And.Not.EqualTo (SmtpUtf8AddressError.LabelTooLong), label);

				SmtpUtf8AddressValidator.TryValidate ("u@" + label + "." + filler + "a", LengthLimits, out error);
				Assert.That (error, Is.EqualTo (SmtpUtf8AddressError.DomainTooLong), label);

				tested++;
			}
		}

		[Test]
		public void TestDomainLengthLimit ()
		{
			// the domain limit is measured in A-label form, so a domain that is short in UTF-8 can still be too long
			var domain = Repeat ("é.", 32) + "com";

			Assert.That (Encoding.UTF8.GetByteCount ("u@" + domain), Is.LessThanOrEqualTo (254));
			AssertValidate ("u@" + domain, LengthLimits, SmtpUtf8AddressError.DomainTooLong);

			AssertValidate ("u@" + CreateAsciiDomain (254), LengthLimits, SmtpUtf8AddressError.DomainTooLong);
			AssertValidate ("u@" + CreateAsciiDomain (254), SmtpUtf8AddressError.None);
		}

		[Test]
		public void TestAddressLengthLimit ()
		{
			AssertValidate ("u@" + CreateAsciiDomain (252), LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate ("u@" + CreateAsciiDomain (253), LengthLimits, SmtpUtf8AddressError.AddressTooLong);
			AssertValidate (Repeat ("a", 64) + "@" + CreateAsciiDomain (189), LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("a", 64) + "@" + CreateAsciiDomain (190), LengthLimits, SmtpUtf8AddressError.AddressTooLong);

			// the address limit is measured in UTF-8 octets
			AssertValidate (Repeat ("ø", 32) + "@" + CreateAsciiDomain (189), LengthLimits, SmtpUtf8AddressError.None);
			AssertValidate (Repeat ("ø", 32) + "@" + CreateAsciiDomain (190), LengthLimits, SmtpUtf8AddressError.AddressTooLong);

			// the address limit applies to address literals
			AssertValidate (Repeat ("a", 64) + "@[IPv6:0000:0000:0000:0000:0000:ffff:192.168.000.001]", LengthLimits, SmtpUtf8AddressError.None);

			// the limits are not enforced by default
			AssertValidate ("u@" + CreateAsciiDomain (253), SmtpUtf8AddressError.None);
		}

		[Test]
		public void TestLengthLimitPrecedence ()
		{
			// syntax errors take precedence over length limits
			AssertValidate (Repeat ("a", 65) + "@exa..mple.com", LengthLimits, SmtpUtf8AddressError.Syntax);

			// length limits take precedence over the other rules
			AssertValidate ("xn--" + Repeat ("a", 61) + "@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);
			AssertValidate (Repeat ("\u200E", 22) + "@example.com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);

			// the local-part is checked before the domain
			AssertValidate (Repeat ("a", 65) + "@" + Repeat ("a", 64) + ".com", LengthLimits, SmtpUtf8AddressError.LocalPartTooLong);

			// labels are checked before the domain
			AssertValidate ("u@" + CreateAsciiDomain (200) + "." + Repeat ("a", 64), LengthLimits, SmtpUtf8AddressError.LabelTooLong);
		}
	}
}
