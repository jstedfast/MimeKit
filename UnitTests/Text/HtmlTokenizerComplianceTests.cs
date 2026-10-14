//
// HtmlTokenizerComplianceTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlTokenizerComplianceTests
	{
		// Returns at most one character per read so that every token straddles a buffer boundary.
		sealed class TrickleReader : StringReader
		{
			public TrickleReader (string text) : base (text)
			{
			}

			public override int Read (char[] buffer, int index, int count)
			{
				return base.Read (buffer, index, Math.Min (count, 1));
			}

			public override int Read (Span<char> buffer)
			{
				return base.Read (buffer.Slice (0, Math.Min (buffer.Length, 1)));
			}
		}

		static HtmlTokenizer CreateTokenizer (string html, bool trickle, IHtmlComplianceLogger logger)
		{
			var reader = trickle ? new TrickleReader (html) : new StringReader (html);

			return new HtmlTokenizer (reader) {
				ComplianceLogger = logger
			};
		}

		static string[] Tokenize (string html, bool trickle = false, int maxComplianceIssuesPerViolation = 0, int maxElementDepth = HtmlTokenizer.DefaultMaxElementDepth)
		{
			var logger = new TestHtmlComplianceLogger ();
			var tokenizer = CreateTokenizer (html, trickle, logger);

			tokenizer.ComplianceOptions.MaxIssuesPerViolation = maxComplianceIssuesPerViolation;
			tokenizer.MaxElementDepth = maxElementDepth;

			while (tokenizer.ReadNextToken (out _))
				;

			return logger.Issues.Select (x => x.ToString ()).ToArray ();
		}

		static readonly object[][] ViolationCases = {
			new object[] { "<p>hello</p>", Array.Empty<string> () },
			new object[] { "<!DOCTYPE html><html><body a=\"b\" c=d e=f>&amp;&#65;&#x41;&copy;</body></html>", Array.Empty<string> () },
			new object[] { "<", new[] { "EofBeforeTagName at line 1, position 2 (offset 1)" } },
			new object[] { "</", new[] { "EofBeforeTagName at line 1, position 3 (offset 2)" } },
			new object[] { "<42>", new[] { "InvalidFirstCharacterOfTagName at line 1, position 2 (offset 1)" } },
			new object[] { "a < b", new[] { "InvalidFirstCharacterOfTagName at line 1, position 4 (offset 3)" } },
			new object[] { "<?xml?>", new[] { "UnexpectedQuestionMarkInsteadOfTagName at line 1, position 2 (offset 1)" } },
			new object[] { "</>", new[] { "MissingEndTagName at line 1, position 3 (offset 2)" } },
			new object[] { "<div", new[] { "EofInTag at line 1, position 5 (offset 4)" } },
			new object[] { "<div / id=x>", new[] { "UnexpectedSolidusInTag at line 1, position 6 (offset 5)" } },
			new object[] { "</div id=x>", new[] { "EndTagWithAttributes at line 1, position 11 (offset 10)" } },
			new object[] { "</div/>", new[] { "EndTagWithTrailingSolidus at line 1, position 7 (offset 6)" } },
			new object[] { "<div id=a ID=b>", new[] { "DuplicateAttribute at line 1, position 11 (offset 10)" } },
			new object[] { "<div\nid=a\nid=b>", new[] { "DuplicateAttribute at line 3, position 1 (offset 10)" } },
			new object[] { "<div id=>", new[] { "MissingAttributeValue at line 1, position 9 (offset 8)" } },
			new object[] { "<div id='a'class='b'>", new[] { "MissingWhitespaceBetweenAttributes at line 1, position 12 (offset 11)" } },
			new object[] { "<div a\"b>", new[] { "UnexpectedCharacterInAttributeName at line 1, position 7 (offset 6)" } },
			new object[] { "<div a=b\"c>", new[] { "UnexpectedCharacterInUnquotedAttributeValue at line 1, position 9 (offset 8)" } },
			new object[] { "<div =a>", new[] { "UnexpectedEqualsSignBeforeAttributeName at line 1, position 6 (offset 5)" } },
			new object[] { "<!-->", new[] { "AbruptClosingOfEmptyComment at line 1, position 5 (offset 4)" } },
			new object[] { "<!--->", new[] { "AbruptClosingOfEmptyComment at line 1, position 6 (offset 5)" } },
			new object[] { "<!-- x", new[] { "EofInComment at line 1, position 7 (offset 6)" } },
			new object[] { "<!-- x --!>", new[] { "IncorrectlyClosedComment at line 1, position 11 (offset 10)" } },
			new object[] { "<!x>", new[] { "IncorrectlyOpenedComment at line 1, position 3 (offset 2)" } },
			new object[] { "<!-- <!-- x -->", new[] { "NestedComment at line 1, position 10 (offset 9)" } },
			new object[] { "<!DOCTYPE html PUBLIC \"foo>", new[] { "AbruptDocTypePublicIdentifier at line 1, position 27 (offset 26)" } },
			new object[] { "<!DOCTYPE html SYSTEM \"foo>", new[] { "AbruptDocTypeSystemIdentifier at line 1, position 27 (offset 26)" } },
			new object[] { "<!DOCTYPE html", new[] { "EofInDocType at line 1, position 15 (offset 14)" } },
			new object[] { "<!DOCTYPE html foo>", new[] { "InvalidCharacterSequenceAfterDocTypeName at line 1, position 16 (offset 15)" } },
			new object[] { "<!DOCTYPE>", new[] { "MissingDocTypeName at line 1, position 10 (offset 9)" } },
			new object[] { "<!DOCTYPE html PUBLIC>", new[] { "MissingDocTypePublicIdentifier at line 1, position 22 (offset 21)" } },
			new object[] { "<!DOCTYPE html SYSTEM>", new[] { "MissingDocTypeSystemIdentifier at line 1, position 22 (offset 21)" } },
			new object[] { "<!DOCTYPE html PUBLIC foo>", new[] { "MissingQuoteBeforeDocTypePublicIdentifier at line 1, position 23 (offset 22)" } },
			new object[] { "<!DOCTYPE html SYSTEM foo>", new[] { "MissingQuoteBeforeDocTypeSystemIdentifier at line 1, position 23 (offset 22)" } },
			new object[] { "<!DOCTYPE html PUBLIC\"foo\">", new[] { "MissingWhitespaceAfterDocTypePublicKeyword at line 1, position 22 (offset 21)" } },
			new object[] { "<!DOCTYPE html SYSTEM\"foo\">", new[] { "MissingWhitespaceAfterDocTypeSystemKeyword at line 1, position 22 (offset 21)" } },
			new object[] { "<!DOCTYPEhtml>", new[] { "MissingWhitespaceBeforeDocTypeName at line 1, position 10 (offset 9)" } },
			new object[] { "<!DOCTYPE html PUBLIC \"a\"\"b\">", new[] { "MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers at line 1, position 26 (offset 25)" } },
			new object[] { "<!DOCTYPE html SYSTEM \"a\" x>", new[] { "UnexpectedCharacterAfterDocTypeSystemIdentifier at line 1, position 27 (offset 26)" } },
			new object[] { "&#;", new[] { "AbsenceOfDigitsInNumericCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#x;", new[] { "AbsenceOfDigitsInNumericCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#x110000;", new[] { "CharacterReferenceOutsideUnicodeRange at line 1, position 1 (offset 0)" } },
			new object[] { "&#x80;", new[] { "ControlCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#13;", new[] { "ControlCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#1;", new[] { "ControlCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#9;", Array.Empty<string> () },
			new object[] { "&amp x", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#65 x", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#65", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&amp", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#xFFFE;", new[] { "NoncharacterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#xFDD0;", new[] { "NoncharacterCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#0;", new[] { "NullCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&#xD800;", new[] { "SurrogateCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&foo;", new[] { "UnknownNamedCharacterReference at line 1, position 1 (offset 0)" } },
			new object[] { "&foo bar", Array.Empty<string> () },
			new object[] { "&", Array.Empty<string> () },
			new object[] { "& x", Array.Empty<string> () },
			new object[] { "x &#x110000", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 3 (offset 2)", "CharacterReferenceOutsideUnicodeRange at line 1, position 3 (offset 2)" } },
			new object[] { "<a title='&copy x'>", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 11 (offset 10)" } },
			new object[] { "<a href='?a=1&copy=2'>", Array.Empty<string> () },
			new object[] { "<a title='&foo;'>", new[] { "UnknownNamedCharacterReference at line 1, position 11 (offset 10)" } },
			new object[] { "<a title='&#0;'>", new[] { "NullCharacterReference at line 1, position 11 (offset 10)" } },
			new object[] { "<title>&#0;&foo;</title>", new[] { "NullCharacterReference at line 1, position 8 (offset 7)", "UnknownNamedCharacterReference at line 1, position 12 (offset 11)" } },
			new object[] { "<textarea>&amp x</textarea>", new[] { "MissingSemicolonAfterCharacterReference at line 1, position 11 (offset 10)" } },
			new object[] { "<![CDATA[x]]>", new[] { "CDataInHtmlContent at line 1, position 3 (offset 2)" } },
			new object[] { "<svg><![CDATA[x", new[] { "EofInCData at line 1, position 16 (offset 15)" } },
			new object[] { "<svg><![CDATA[x]]></svg>", Array.Empty<string> () },
			new object[] { "<script><!--", new[] { "EofInScriptHtmlCommentLikeText at line 1, position 13 (offset 12)" } },
			new object[] { "<script><!--<script>", new[] { "EofInScriptHtmlCommentLikeText at line 1, position 21 (offset 20)" } },
			new object[] { "a\0b", new[] { "UnexpectedNullCharacter at line 1, position 2 (offset 1)" } },
			new object[] { "<a\0>", new[] { "UnexpectedNullCharacter at line 1, position 3 (offset 2)" } },
			new object[] { "<a b\0=c>", new[] { "UnexpectedNullCharacter at line 1, position 5 (offset 4)" } },
			new object[] { "<a b=\0>", new[] { "UnexpectedNullCharacter at line 1, position 6 (offset 5)" } },
			new object[] { "<!--\0-->", new[] { "UnexpectedNullCharacter at line 1, position 5 (offset 4)" } },
			new object[] { "<title>\0</title>", new[] { "UnexpectedNullCharacter at line 1, position 8 (offset 7)" } },
			new object[] { "line1\nline2 &#0;", new[] { "NullCharacterReference at line 2, position 7 (offset 12)" } },
		};

		[TestCaseSource (nameof (ViolationCases))]
		public void TestViolations (string html, string[] expected)
		{
			Assert.That (Tokenize (html), Is.EqualTo (expected));
		}

		[TestCaseSource (nameof (ViolationCases))]
		public void TestViolationsAcrossBufferBoundaries (string html, string[] expected)
		{
			Assert.That (Tokenize (html, true), Is.EqualTo (expected));
		}

		static string[] GetTokens (string html, bool decodeCharacterReferences, IHtmlComplianceLogger logger)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html)) {
				DecodeCharacterReferences = decodeCharacterReferences,
				ComplianceLogger = logger
			};
			var tokens = new List<string> ();

			while (tokenizer.ReadNextToken (out var token)) {
				var text = token is HtmlDataToken data ? data.Data : token.ToString ();

				tokens.Add (token.Kind + ": " + text);
			}

			return tokens.ToArray ();
		}

		[TestCaseSource (nameof (ViolationCases))]
		public void TestLoggingDoesNotChangeTheTokens (string html, string[] expected)
		{
			foreach (var decode in new[] { true, false }) {
				var logger = new TestHtmlComplianceLogger ();
				var tokens = GetTokens (html, decode, logger);

				Assert.That (tokens, Is.EqualTo (GetTokens (html, decode, null)), $"DecodeCharacterReferences = {decode}");
				Assert.That (logger.Issues.Select (x => x.ToString ()), Is.EqualTo (expected), $"DecodeCharacterReferences = {decode}");
			}
		}

		[Test]
		public void TestDuplicateAttributes ()
		{
			var issues = Tokenize ("<a x=1 X=2 x=3 y=4><b x=1 y=2><c X=1 x=2>");

			Assert.That (issues, Is.EqualTo (new[] {
				"DuplicateAttribute at line 1, position 8 (offset 7)",
				"DuplicateAttribute at line 1, position 12 (offset 11)",
				"DuplicateAttribute at line 1, position 38 (offset 37)"
			}));
		}

		[Test]
		public void TestDuplicateAttributesAreStillEmitted ()
		{
			var tokenizer = new HtmlTokenizer (new StringReader ("<a x=1 X=2>")) {
				ComplianceLogger = new TestHtmlComplianceLogger ()
			};

			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);

			var tag = (HtmlTagToken) token;

			Assert.That (tag.Attributes, Has.Count.EqualTo (2));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("1"));
			Assert.That (tag.Attributes[1].Value, Is.EqualTo ("2"));
		}

		[Test]
		public void TestManyDistinctAttributes ()
		{
			var builder = new StringBuilder ("<a");

			for (int i = 0; i < 1000; i++)
				builder.Append (" a").Append (i).Append ("=x");

			builder.Append (" a999=y>");

			Assert.That (Tokenize (builder.ToString ()), Has.Length.EqualTo (1));
		}

		[Test]
		public void TestElementDepthLimitExceededIsReportedOnce ()
		{
			var issues = Tokenize ("<a><b><c><d><e>", maxElementDepth: 2);

			Assert.That (issues, Is.EqualTo (new[] { "ElementDepthLimitExceeded at line 1, position 9 (offset 8)" }));
		}

		[Test]
		public void TestMaxComplianceIssuesPerViolation ()
		{
			var issues = Tokenize ("&#0;&#0;&#0;&#0;<a\0>", maxComplianceIssuesPerViolation: 2);

			Assert.That (issues, Is.EqualTo (new[] {
				"NullCharacterReference at line 1, position 1 (offset 0)",
				"NullCharacterReference at line 1, position 5 (offset 4)",
				"TooManyComplianceIssues at line 1, position 9 (offset 8)",
				"UnexpectedNullCharacter at line 1, position 19 (offset 18)"
			}));
		}

		[Test]
		public void TestComplianceLoggerProperties ()
		{
			var logger = new TestHtmlComplianceLogger ();
			var tokenizer = new HtmlTokenizer (new StringReader ("&#0;&#0;&#0;"));

			Assert.That (tokenizer.ComplianceLogger, Is.Null, "Default ComplianceLogger");
			Assert.That (tokenizer.ComplianceOptions, Is.Not.Null, "Default ComplianceOptions");
			Assert.That (tokenizer.ComplianceOptions, Is.Not.SameAs (HtmlComplianceOptions.Default), "Default ComplianceOptions should be a copy");
			Assert.That (tokenizer.ComplianceOptions.MaxIssuesPerViolation, Is.EqualTo (0), "Default MaxIssuesPerViolation");

			var options = new HtmlComplianceOptions { MaxIssuesPerViolation = 1 };
			tokenizer.ComplianceOptions = options;
			tokenizer.ComplianceLogger = logger;

			Assert.That (tokenizer.ComplianceLogger, Is.SameAs (logger));
			Assert.That (tokenizer.ComplianceOptions, Is.SameAs (options));

			tokenizer.ComplianceOptions = HtmlComplianceOptions.Default;
			Assert.That (tokenizer.ComplianceOptions, Is.Not.SameAs (HtmlComplianceOptions.Default), "Setting Default should store a copy");
			tokenizer.ComplianceOptions = options;

			while (tokenizer.ReadNextToken (out _))
				;

			Assert.That (logger.Issues.Select (x => x.Violation), Is.EqualTo (new[] { HtmlComplianceViolation.NullCharacterReference, HtmlComplianceViolation.TooManyComplianceIssues }));
		}

		[Test]
		public void TestChangingComplianceOptionsAfterSettingTheLogger ()
		{
			var logger = new TestHtmlComplianceLogger ();
			var tokenizer = new HtmlTokenizer (new StringReader ("&#0;&#0;&#0;<a>&#0;&#0;&#0;")) {
				ComplianceLogger = logger
			};

			tokenizer.ComplianceOptions.MaxIssuesPerViolation = 1;

			// Note: The first data token stops at the start tag, so the first three references have been
			// reported by the time it is returned.
			Assert.That (tokenizer.ReadNextToken (out _), Is.True);
			Assert.That (logger.Issues.Select (x => x.Violation), Is.EqualTo (new[] {
				HtmlComplianceViolation.NullCharacterReference,
				HtmlComplianceViolation.TooManyComplianceIssues
			}));

			// Note: Changing the limit restarts the count.
			tokenizer.ComplianceOptions.MaxIssuesPerViolation = 2;

			while (tokenizer.ReadNextToken (out _))
				;

			Assert.That (logger.Issues.Select (x => x.Violation), Is.EqualTo (new[] {
				HtmlComplianceViolation.NullCharacterReference,
				HtmlComplianceViolation.TooManyComplianceIssues,
				HtmlComplianceViolation.NullCharacterReference,
				HtmlComplianceViolation.NullCharacterReference,
				HtmlComplianceViolation.TooManyComplianceIssues
			}));
		}

		[Test]
		public void TestComplianceOptionsArguments ()
		{
			var tokenizer = new HtmlTokenizer (new StringReader (string.Empty));
			var converter = new HtmlToHtml ();

			Assert.Throws<ArgumentNullException> (() => tokenizer.ComplianceOptions = null);
			Assert.Throws<ArgumentNullException> (() => converter.ComplianceOptions = null);
			Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceOptions { MaxIssuesPerViolation = -1 });
		}

		[Test]
		public void TestComplianceOptionsClone ()
		{
			var options = new HtmlComplianceOptions { MaxIssuesPerViolation = 5 };
			var clone = options.Clone ();

			Assert.That (clone, Is.Not.SameAs (options));
			Assert.That (clone.MaxIssuesPerViolation, Is.EqualTo (5));
			Assert.That (HtmlComplianceOptions.Default.MaxIssuesPerViolation, Is.EqualTo (0));
		}

		[Test]
		public void TestRemovingTheComplianceLogger ()
		{
			var logger = new TestHtmlComplianceLogger ();
			var tokenizer = new HtmlTokenizer (new StringReader ("<a x=1 x=2><b x=1 x=2>")) {
				ComplianceLogger = logger
			};

			Assert.That (tokenizer.ReadNextToken (out _), Is.True);
			Assert.That (logger.Issues, Has.Count.EqualTo (1));

			tokenizer.ComplianceLogger = null;

			while (tokenizer.ReadNextToken (out _))
				;

			Assert.That (logger.Issues, Has.Count.EqualTo (1));
		}

		[Test]
		public void TestHtmlToHtml ()
		{
			const string html = "<p title='&copy x'>&amp x &#0; &foo;</p><div id=a id=b>";
			var logger = new TestHtmlComplianceLogger ();
			var converter = new HtmlToHtml { OutputHtmlFragment = true };
			var expected = converter.Convert (html);

			Assert.That (converter.ComplianceLogger, Is.Null, "Default ComplianceLogger");
			Assert.That (converter.ComplianceOptions, Is.Not.SameAs (HtmlComplianceOptions.Default), "Default ComplianceOptions should be a copy");
			Assert.That (converter.ComplianceOptions.MaxIssuesPerViolation, Is.EqualTo (0), "Default MaxIssuesPerViolation");

			converter.ComplianceLogger = logger;

			Assert.That (converter.Convert (html), Is.EqualTo (expected), "Output");
			Assert.That (logger.Issues.Select (x => x.ToString ()), Is.EqualTo (new[] {
				"MissingSemicolonAfterCharacterReference at line 1, position 11 (offset 10)",
				"MissingSemicolonAfterCharacterReference at line 1, position 20 (offset 19)",
				"NullCharacterReference at line 1, position 27 (offset 26)",
				"UnknownNamedCharacterReference at line 1, position 32 (offset 31)",
				"DuplicateAttribute at line 1, position 51 (offset 50)"
			}));
		}

		[Test]
		public void TestHtmlToHtmlMaxIssuesPerViolation ()
		{
			var options = new HtmlComplianceOptions { MaxIssuesPerViolation = 1 };
			var logger = new TestHtmlComplianceLogger ();
			var converter = new HtmlToHtml {
				ComplianceOptions = options,
				ComplianceLogger = logger
			};

			Assert.That (converter.ComplianceOptions, Is.SameAs (options));

			converter.Convert ("&#0;&#0;&#0;");
			converter.Convert ("&#0;");

			// Note: Each conversion gets its own budget.
			Assert.That (logger.Issues.Select (x => x.Violation), Is.EqualTo (new[] {
				HtmlComplianceViolation.NullCharacterReference,
				HtmlComplianceViolation.TooManyComplianceIssues,
				HtmlComplianceViolation.NullCharacterReference
			}));
		}
	}
}
