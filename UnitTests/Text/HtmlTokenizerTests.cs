//
// HtmlTokenizerTests.cs
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
	public class HtmlTokenizerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new HtmlTokenizer ((TextReader) null));

			Assert.Throws<ArgumentNullException> (() => new HtmlTokenizer ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => new HtmlTokenizer ((Stream) null, Encoding.UTF8));

			Assert.Throws<ArgumentNullException> (() => new HtmlTokenizer (Stream.Null, null));

			var nullTokenizer = new HtmlTokenizer (Stream.Null);
			var utf8Tokenizer = new HtmlTokenizer (Stream.Null, Encoding.UTF8);
		}

		static string Quote (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var quoted = new StringBuilder (text.Length + 2, (text.Length * 2) + 2);

			quoted.Append ('\"');
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '\\' || text[i] == '"')
					quoted.Append ('\\');
				else if (text[i] == '\r')
					continue;
				quoted.Append (text[i]);
			}
			quoted.Append ('\"');

			return quoted.ToString ();
		}

		static void GetOutputAndTokenPaths (string path, bool trimCharsetSuffix, out string outpath, out string tokens)
		{
			if (trimCharsetSuffix) {
				var extension = Path.GetExtension (path);
				int charsetExtensionIndex = path.LastIndexOf ('.', path.Length - extension.Length - 1);
				path = path.Substring (0, charsetExtensionIndex) + extension;
			}

			outpath = Path.ChangeExtension (path, ".out.html");
			tokens = Path.ChangeExtension (path, ".tokens");
		}

		static void VerifyHtmlTokenizerOutput (string path, Encoding encoding = null, bool useTextReader = true, bool trimCharsetSuffix = false, bool detectEncodingFromByteOrderMarks = true)
		{
			GetOutputAndTokenPaths (path, trimCharsetSuffix, out var outpath, out var tokens);
			var expectedOutput = File.Exists (outpath) ? File.ReadAllText (outpath) : string.Empty;
			var expected = File.Exists (tokens) ? File.ReadAllText (tokens).Replace ("\r\n", "\n") : string.Empty;
			var output = new StringBuilder ();
			var actual = new StringBuilder ();
			TextReader reader = null;
			Stream stream = null;

			encoding ??= Encoding.GetEncoding (1252);

			if (useTextReader)
				reader = new StreamReader (path, encoding, detectEncodingFromByteOrderMarks);
			else
				stream = File.OpenRead (path);

			try {
				HtmlTokenizer tokenizer;
				HtmlToken token;

				if (useTextReader)
					tokenizer = new HtmlTokenizer (reader);
				else
					tokenizer = new HtmlTokenizer (stream, encoding, detectEncodingFromByteOrderMarks);

				Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));

				while (tokenizer.ReadNextToken (out token)) {
					output.Append (token);

					actual.AppendFormat ("{0}: ", token.Kind);

					switch (token.Kind) {
					case HtmlTokenKind.ScriptData:
					case HtmlTokenKind.CData:
					case HtmlTokenKind.Data:
						var text = (HtmlDataToken) token;

						for (int i = 0; i < text.Data.Length; i++) {
							switch (text.Data[i]) {
							case '\f': actual.Append ("\\f"); break;
							case '\t': actual.Append ("\\t"); break;
							case '\r': break;
							case '\n': actual.Append ("\\n"); break;
							default: actual.Append (text.Data[i]); break;
							}
						}
						actual.Append ('\n');
						break;
					case HtmlTokenKind.Tag:
						var tag = (HtmlTagToken) token;

						actual.AppendFormat ("<{0}{1}", tag.IsEndTag ? "/" : "", tag.Name);

						foreach (var attribute in tag.Attributes) {
							if (attribute.Value != null)
								actual.AppendFormat (" {0}={1}", attribute.Name, Quote (attribute.Value));
							else
								actual.AppendFormat (" {0}", attribute.Name);
						}

						actual.Append (tag.IsEmptyElement ? "/>" : ">");

						actual.Append ('\n');
						break;
					case HtmlTokenKind.Comment:
						var comment = (HtmlCommentToken) token;
						actual.Append (comment.Comment.Replace ("\r\n", "\n"));
						actual.Append ('\n');
						break;
					case HtmlTokenKind.DocType:
						var doctype = (HtmlDocTypeToken) token;

						if (doctype.ForceQuirksMode)
							actual.Append ("<!-- force quirks mode -->");

						actual.Append ("<!DOCTYPE");

						if (doctype.Name != null)
							actual.AppendFormat (" {0}", doctype.Name.ToUpperInvariant ());

						if (doctype.PublicIdentifier != null) {
							actual.AppendFormat (" PUBLIC {0}", Quote (doctype.PublicIdentifier));
							if (doctype.SystemIdentifier != null)
								actual.AppendFormat (" {0}", Quote (doctype.SystemIdentifier));
						} else if (doctype.SystemIdentifier != null) {
							actual.AppendFormat (" SYSTEM {0}", Quote (doctype.SystemIdentifier));
						}

						actual.Append ('>');
						actual.Append ('\n');
						break;
					default:
						Assert.Fail ($"Unhandled token type: {token.Kind}");
						break;
					}
				}

				Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.EndOfFile));
			} finally {
				reader?.Dispose ();
				stream?.Dispose ();
			}

			if (!File.Exists (tokens))
				File.WriteAllText (tokens, actual.ToString ());

			if (!File.Exists (outpath))
				File.WriteAllText (outpath, output.ToString ());

			Assert.That (actual.ToString (), Is.EqualTo (expected), "The token stream does not match the expected tokens.");
			Assert.That (output.ToString (), Is.EqualTo (expectedOutput), "The output stream does not match the expected output.");
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestGoogleSignInAttemptBlocked (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "blocked.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestXamarin3SampleHtml (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "xamarin3.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestPapercut (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "papercut.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestPapercut44 (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "papercut-4.4.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestScriptData (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "script-data.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestCData (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "cdata.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestTokenizer (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "test.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestPlainText (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "plaintext.html"), useTextReader: useTextReader);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestBadlyQuotedAttribute (bool useTextReader)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "badly-quoted-attr.html"), useTextReader: useTextReader);
		}

		[TestCase ("utf-8")]
		[TestCase ("utf-16")]
		[TestCase ("utf-16BE")]
		[TestCase ("utf-32")]
		[TestCase ("utf-32BE")]
		public void TestDetectEncodingFromByteOrderMarks (string charset)
		{
			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", $"Gimhae_Kim_clan.{charset}.html"), useTextReader: false, trimCharsetSuffix: true);
		}

		[TestCase ("utf-8")]
		[TestCase ("utf-16")]
		[TestCase ("utf-16BE")]
		[TestCase ("utf-32")]
		[TestCase ("utf-32BE")]
		public void TestSkipByteOrderMarks (string charset)
		{
			var encoding = Encoding.GetEncoding (charset);

			VerifyHtmlTokenizerOutput (Path.Combine (TestHelper.ProjectDir, "TestData", "html", $"Gimhae_Kim_clan.{charset}.html"), encoding, useTextReader: false, trimCharsetSuffix: true, detectEncodingFromByteOrderMarks: false);
		}

		// Note: The following tests are borrowed from AngleSharp

		static HtmlTokenizer CreateTokenizer (string input)
		{
			return new HtmlTokenizer (new StringReader (input));
		}

		[Test]
		public void TokenizationFinalEOF ()
		{
			var tokenizer = CreateTokenizer ("");

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TokenizationLongerCharacterReference ()
		{
			const string content = "&abcdefghijklmnopqrstvwxyzABCDEFGHIJKLMNOPQRSTV;";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			var cdata = (HtmlDataToken) token;
			Assert.That (cdata.Data, Is.EqualTo (content));
		}

		[Test]
		public void TokenizationStartTagDetection ()
		{
			var tokenizer = CreateTokenizer ("<p>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("p"));
			Assert.That (tag.IsEndTag, Is.False);
			Assert.That (tag.IsEmptyElement, Is.False);
		}

		[Test]
		public void TokenizationBogusCommentEmpty ()
		{
			var tokenizer = CreateTokenizer ("<!>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			var comment = (HtmlCommentToken) token;
			Assert.That (comment.Comment, Is.EqualTo (""));
		}

		[Test]
		public void TokenizationBogusCommentQuestionMark ()
		{
			var tokenizer = CreateTokenizer ("<?>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			var comment = (HtmlCommentToken) token;
			Assert.That (comment.Comment, Is.EqualTo ("?"));
		}

		[Test]
		public void TokenizationBogusCommentClosingTag ()
		{
			var tokenizer = CreateTokenizer ("</ >");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			var comment = (HtmlCommentToken) token;
			Assert.That (comment.Comment, Is.EqualTo (" "));
		}

		[Test]
		public void TokenizationTagNameDetection ()
		{
			var tokenizer = CreateTokenizer ("<span>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).Name, Is.EqualTo ("span"));
		}

		[Test]
		public void TokenizationTagSelfClosingDetected ()
		{
			var tokenizer = CreateTokenizer ("<img />");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).IsEmptyElement, Is.True);
		}

		[Test]
		public void TokenizationAttributesDetected ()
		{
			var tokenizer = CreateTokenizer ("<a target='_blank' href='http://whatever' title='ho'>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).Attributes.Count, Is.EqualTo (3));
		}

		[Test]
		public void TokenizationAttributeNameDetection ()
		{
			var tokenizer = CreateTokenizer ("<input required>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).Attributes[0].Name, Is.EqualTo ("required"));
		}

		[Test]
		public void TokenizationTagMixedCaseHandling ()
		{
			var tokenizer = CreateTokenizer ("<InpUT>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Input));
		}

		[Test]
		public void TokenizationTagSpacesBehind ()
		{
			var tokenizer = CreateTokenizer ("<i   >");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (((HtmlTagToken) token).Name, Is.EqualTo ("i"));
		}

		[Test]
		public void TokenizationCharacterReferenceNotin ()
		{
			var str = string.Empty;
			var src = "I'm &notin; I tell you";
			var tokenizer = CreateTokenizer (src);
			HtmlToken token;

			while (tokenizer.ReadNextToken (out token)) {
				if (token.Kind == HtmlTokenKind.Data)
					str += ((HtmlDataToken) token).Data;
			}

			Assert.That (str, Is.EqualTo ("I'm ∉ I tell you"));
		}

		[Test]
		public void TokenizationCharacterReferenceNotIt ()
		{
			var str = string.Empty;
			var src = "I'm &notit; I tell you";
			var tokenizer = CreateTokenizer (src);
			HtmlToken token;

			while (tokenizer.ReadNextToken (out token)) {
				if (token.Kind == HtmlTokenKind.Data)
					str += ((HtmlDataToken) token).Data;
			}

			Assert.That (str, Is.EqualTo ("I'm ¬it; I tell you"));
		}

		[Test]
		public void TokenizationDoctypeDetected ()
		{
			var tokenizer = CreateTokenizer ("<!doctype html>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
		}

		[Test]
		public void TokenizationCommentDetected ()
		{
			var tokenizer = CreateTokenizer ("<!-- hi my friend -->");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
		}

		[Test]
		public void TokenizationCDataDetected ()
		{
			var tokenizer = CreateTokenizer ("<svg><![CDATA[hi mum how <!-- are you doing />]]>");

			//tokenizer.IsAcceptingCharacterData = true;

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.CData));
		}

		[Test]
		public void TokenizationCDataNotDetectedInHtmlContent ()
		{
			var tokenizer = CreateTokenizer ("<![CDATA[hi mum how <!-- are you doing />]]>");

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("[CDATA[hi mum how <!-- are you doing /"));
		}

		[Test]
		public void TokenizationCDataCorrectCharacters ()
		{
			var sb = new StringBuilder ();
			var tokenizer = CreateTokenizer ("<math><![CDATA[hi mum how <!-- are you doing />]]>");
			HtmlToken token;

			//tokenizer.IsAcceptingCharacterData = true;

			while (tokenizer.ReadNextToken (out token)) {
				if (token.Kind == HtmlTokenKind.CData)
					sb.Append (((HtmlCDataToken) token).Data);
			}

			Assert.That (sb.ToString (), Is.EqualTo ("hi mum how <!-- are you doing />"));
		}

		[Test]
		public void TokenizationUnusualDoctype ()
		{
			var tokenizer = CreateTokenizer ("<!DOCTYPE root_element SYSTEM \"DTD_location\">");
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));

			var d = (HtmlDocTypeToken) token;
			Assert.That (d.Name, Is.Not.Null);
			Assert.That (d.Name, Is.EqualTo ("root_element"));
			Assert.That (d.SystemIdentifier, Is.EqualTo ("DTD_location"));
		}

		[Test]
		public void TokenizationOnlyCarriageReturn ()
		{
			var tokenizer = CreateTokenizer ("\r");
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("\r"));
		}

		[Test]
		public void TokenizationOnlyLineFeed ()
		{
			var tokenizer = CreateTokenizer ("\n");
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("\n"));
		}

		[Test]
		public void TokenizationCarriageReturnLineFeed ()
		{
			var tokenizer = CreateTokenizer ("\r\n");
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("\r\n"));
		}

		[Test]
		public void TokenizationLongestLegalCharacterReference ()
		{
			var content = "&CounterClockwiseContourIntegral;";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("∳"));
		}

		//[Test]
		//public void TokenizationLongestIllegalCharacterReference ()
		//{
		//	var content = "&CounterClockwiseContourIntegralWithWrongName;";
		//	var tokenizer = CreateTokenizer (content);

		//	Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
		//	Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
		//	Assert.That (((HtmlDataToken) token).Data, Is.EqualTo (content));
		//}

		[Test]
		public void TestDataCharacterReferencesNotDecoded ()
		{
			const string content = "<b>check &CounterClockwiseContourIntegral; is not decoded</b>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			tokenizer.DecodeCharacterReferences = false;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.B));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("check &CounterClockwiseContourIntegral; is not decoded"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.B));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);
		}

		[Test]
		public void TestRcDataCharacterReferencesNotDecoded ()
		{
			const string content = "<title>check &CounterClockwiseContourIntegral; is not decoded</title>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			tokenizer.DecodeCharacterReferences = false;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("check &CounterClockwiseContourIntegral; is not decoded"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);
		}

		// The following unit tests are for error conditions

		[Test]
		public void TestTruncatedMarkupDeclarationOpen ()
		{
			foreach (var ignoreTruncatedTags in new[] { false, true }) {
				var tokenizer = CreateTokenizer ("<!");
				tokenizer.IgnoreTruncatedTags = ignoreTruncatedTags;

				Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
				Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
				Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
				Assert.That (tokenizer.ReadNextToken (out _), Is.False);

				tokenizer = CreateTokenizer ("<!-");
				tokenizer.IgnoreTruncatedTags = ignoreTruncatedTags;

				Assert.That (tokenizer.ReadNextToken (out token), Is.True);
				Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
				Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("-"));
				Assert.That (tokenizer.ReadNextToken (out _), Is.False);
			}
		}

		[Test]
		public void TestNumericCharacterReferences ()
		{
			const string content = "&#65&#x42;&#x110000;&#0;&#xD800;&#x80;&#0000000000000000000000000000000000000067;&#99999999999999999999;&#;&#x;";
			var tokenizer = CreateTokenizer (content);
			var text = new StringBuilder ();

			while (tokenizer.ReadNextToken (out var token)) {
				Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
				text.Append (((HtmlDataToken) token).Data);
			}

			Assert.That (text.ToString (), Is.EqualTo ("AB\uFFFD\uFFFD\uFFFD\u20ACC\uFFFD&#;&#x;"));
		}

		[Test]
		public void TestNamedCharacterReferencesInAttributeValues ()
		{
			// 13.2.5.73 Named character reference state: in an attribute value, a legacy reference that isn't
			// terminated by ';' is left as-is if it is followed by '=' or an ASCII alphanumeric character.
			const string content = "<a href=\"?a=1&not=2&noti;&notin;&not;&not!\">&noti;&notin</a>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("?a=1&not=2&noti;\u2209\u00AC\u00AC!"));

			var text = new StringBuilder ();
			while (tokenizer.ReadNextToken (out token) && token.Kind == HtmlTokenKind.Data)
				text.Append (((HtmlDataToken) token).Data);

			Assert.That (text.ToString (), Is.EqualTo ("\u00ACi;\u00ACin"));
		}

		[Test]
		public void TestTruncatedNamedCharacterReference ()
		{
			const string content = "&notin";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			// Note: Only "notin;" is a named reference; without the ';' the longest match is the legacy "not".
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("\u00ACin"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestBogusCommentNul ()
		{
			const string content = "</\0>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("\uFFFD"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestScriptDataEscapedDashDashNul ()
		{
			const string content = "<script><!--a--\0--></script>";
			var tokenizer = CreateTokenizer (content);
			var text = new StringBuilder ();
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));

			while (tokenizer.ReadNextToken (out token) && token.Kind == HtmlTokenKind.ScriptData)
				text.Append (((HtmlScriptDataToken) token).Data);

			Assert.That (text.ToString (), Is.EqualTo ("<!--a--\uFFFD-->"));
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);
		}

		[Test]
		public void TestDocTypeTruncatedKeywordForcesQuirks ()
		{
			foreach (var content in new[] { "<!DOCTYPE html PUB>", "<!DOCTYPE html SYS foo>", "<!DOCTYPE html bogus>" }) {
				var tokenizer = CreateTokenizer (content);

				Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True, content);
				Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType), content);
				var doctype = (HtmlDocTypeToken) token;
				Assert.That (doctype.Name, Is.EqualTo ("html"), content);
				Assert.That (doctype.ForceQuirksMode, Is.True, content);
				Assert.That (tokenizer.ReadNextToken (out _), Is.False, content);
			}
		}

		[Test]
		public void TestTruncatedDocType ()
		{
			const string content = "<!DOCTYPE";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestTruncatedDocTypeSpace ()
		{
			const string content = "<!DOCTYPE ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypeNoName ()
		{
			const string content = "<!DOCTYPE  >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestTruncatedDocTypeName ()
		{
			const string content = "<!DOCTYPE HTML";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypeWithName ()
		{
			const string content = "<!DOCTYPE HTML>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.False);
		}

		[Test]
		public void TestTruncatedAfterDocTypeName ()
		{
			const string content = "<!DOCTYPE HTML ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypeNameParseError ()
		{
			const string content = "<!DOCTYPE HTML\0>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML\uFFFD"));
			Assert.That (doctype.ForceQuirksMode, Is.False);
		}

		[Test]
		public void TestDocTypeNameSpace ()
		{
			const string content = "<!DOCTYPE HTML >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.False);
		}

		[Test]
		public void TestDocTypeNameSpaceBogus ()
		{
			const string content = "<!DOCTYPE HTML BOGUS>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestAfterDocTypeNameShortBogusKeyword ()
		{
			// The After DOCTYPE name state buffers up to 6 characters looking for PUBLIC/SYSTEM; make sure that
			// those characters do not leak into the name of the next tag when the DOCTYPE ends early.
			const string content = "<!DOCTYPE HTML BOG><div class=x></DIV></p>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("div"));
			Assert.That (tag.IsEndTag, Is.False);
			Assert.That (tag.Attributes, Has.Count.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("class"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("DIV"));
			Assert.That (tag.IsEndTag, Is.True);

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("p"));
			Assert.That (tag.IsEndTag, Is.True);

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestAfterDocTypeNameBogusDocType ()
		{
			const string content = "<!DOCTYPE HTML PUBLISH>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestBogusDocTypeAfterName ()
		{
			const string content = "<!DOCTYPE HTML BOGUS >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypeNamePublicX ()
		{
			const string content = "<!DOCTYPE HTML PUBLICX>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypePublicIdentifierQuotedParseError ()
		{
			const string content = "<!DOCTYPE HTML PUBLIC \"public-identifier\0\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PUBLIC"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("public-identifier\uFFFD"));
		}

		[Test]
		public void TestDocTypeSystemIdentifierQuotedParseError ()
		{
			const string content = "<!DOCTYPE HTML SYSTEM \"system-identifier\0\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SYSTEM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("system-identifier\uFFFD"));
		}

		[Test]
		public void TestTruncatedDocTypeAfterPublicKeyword ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.Name, Is.EqualTo ("HTML"));
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
		}

		[Test]
		public void TestTruncatedDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
		}

		[Test]
		public void TestIncompleteDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc  >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
		}

		[Test]
		public void TestInvalidDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc  value>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo (null));
		}

		[Test]
		public void TestIncompleteDocTypePublicIdentifierQuoted ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedDocTypePublicIdentifierQuoted ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedDocTypeAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypePublicWithoutSpace ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc\"value\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeQuoteAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeCharAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"x>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\" ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestInvalidDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"  x>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"  >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeNamePublicClose ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PuBlIc"));
			Assert.That (doctype.PublicIdentifier, Is.EqualTo (null));
		}

		[Test]
		public void TestTruncatedDocTypeAfterSystemKeyword ()
		{
			const string content = "<!DOCTYPE HTML SySTeM";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo (null));
		}

		[Test]
		public void TestDocTypeSystemWithoutSpace ()
		{
			const string content = "<!DOCTYPE HTML SySTeM\"value\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedDocTypeBeforeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
		}

		[Test]
		public void TestDocTypeBeforeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM  >";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
		}

		[Test]
		public void TestDocTypeBeforeSystemIdentifierX ()
		{
			const string content = "<!DOCTYPE HTML SySTeM  x>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
		}

		[Test]
		public void TestTruncatedDocTypeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeQuoteAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\"\">";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeCharAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\"x>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedDocTypeAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\" ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedBogusDocType ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\" x";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;

			// Note: Garbage after the system identifier switches to the bogus DOCTYPE state *without* setting
			// the force-quirks flag and EOF in the bogus DOCTYPE state does not set it either.
			Assert.That (doctype.ForceQuirksMode, Is.False);
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SySTeM"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("value"));
		}

		[Test]
		public void TestDocTypeNameSystemX ()
		{
			const string content = "<!DOCTYPE HTML SYSTEMX>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestDocTypeNameSystem ()
		{
			const string content = "<!DOCTYPE HTML SYSTEM>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			var doctype = (HtmlDocTypeToken) token;
			Assert.That (doctype.ForceQuirksMode, Is.True);
		}

		[Test]
		public void TestTruncatedDocTypeToken ()
		{
			const string content = "<!DOC";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("DOC"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestNotQuiteDocTypeBogusComment ()
		{
			const string content = "<!DOCS>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("DOCS"));
		}

		[Test]
		public void TestTruncatedNotQuiteDocTypeBogusComment ()
		{
			const string content = "<!DOCS";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("DOCS"));
		}

		[Test]
		public void TestNotQuiteCDATABogusComment ()
		{
			const string content = "<![CDAT[>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("[CDAT["));
		}

		[Test]
		public void TestTruncatedNotQuiteCDATABogusComment ()
		{
			const string content = "<![CDAT[";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("[CDAT["));
		}

		[Test]
		public void TestTruncatedCDATA ()
		{
			const string content = "<![CDATA";

			foreach (var ignoreTruncatedTags in new[] { false, true }) {
				var tokenizer = CreateTokenizer (content);
				tokenizer.IgnoreTruncatedTags = ignoreTruncatedTags;

				Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
				Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
				Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("[CDATA"));
				Assert.That (tokenizer.ReadNextToken (out _), Is.False);
			}
		}

		[Test]
		public void TestTruncatedCDATASection ()
		{
			const string content = "<svg><![CDATA[this is some cdata]]";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.CData));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("this is some cdata]]"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.CData));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("this is some cdata]]"));
		}

		[Test]
		public void TestTruncatedComment ()
		{
			const string content = "<!--comment";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment"));
		}

		[Test]
		public void TestTruncatedCommentEndDash ()
		{
			const string content = "<!--comment-";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment"));
		}

		[Test]
		public void TestEmptyComment0 ()
		{
			const string content = "<!-->"; // malformed
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestEmptyComment1 ()
		{
			const string content = "<!--->"; // malformed
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestEmptyComment2 ()
		{
			const string content = "<!---->"; // correct
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestTruncatedEmptyComment0 ()
		{
			const string content = "<!--";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestTruncatedEmptyComment1 ()
		{
			const string content = "<!---";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestTruncatedEmptyComment2 ()
		{
			const string content = "<!----";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestDashComment ()
		{
			const string content = "<!---comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("-comment"));
		}

		[Test]
		public void TestDashDashComment ()
		{
			const string content = "<!----comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("--comment"));
		}

		[Test]
		public void TestCommentDash ()
		{
			const string content = "<!--comment--->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment-"));
		}

		[Test]
		public void TestCommentDashDash ()
		{
			const string content = "<!--comment---->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment--"));
		}

		[Test]
		public void TestCommentDashComment ()
		{
			const string content = "<!--comment-comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment-comment"));
		}

		[Test]
		public void TestCommentDashDashComment ()
		{
			const string content = "<!--comment--comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment--comment"));
		}

		[Test]
		public void TestCommentEndBang ()
		{
			const string content = "<!--comment--!>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment"));
		}

		[Test]
		public void TestTruncatedCommentEndBang ()
		{
			const string content = "<!--comment--!";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment"));
		}

		[Test]
		public void TestCommentDashDashBang ()
		{
			const string content = "<!--comment--!-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment--!"));
		}

		[Test]
		public void TestCommentDashDashBangComment ()
		{
			const string content = "<!--comment--!comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Comment));
			Assert.That (((HtmlCommentToken) token).Comment, Is.EqualTo ("comment--!comment"));
		}

		[Test]
		public void TestTruncatedCharacterReferenceStart ()
		{
			const string content = "&";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("&"));
		}

		[Test]
		public void TestTruncatedCharacterReference ()
		{
			const string content = "&am";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("&am"));
		}

		[Test]
		public void TestTruncatedTagOpen ()
		{
			const string content = "<";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<"));

			// Note: A '<' at EOF is not a truncated tag; it is always emitted as character data.
			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTagOpenDigit ()
		{
			const string content = "<5>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<5>"));
		}

		[Test]
		public void TestTruncatedTagName ()
		{
			const string content = "<nam";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<nam"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedBeforeAttributeName ()
		{
			const string content = "<name ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedAttributeName ()
		{
			const string content = "<name attr";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedAfterAttributeName ()
		{
			const string content = "<name attr  ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr  "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedSelfClosingTag1 ()
		{
			const string content = "<name/";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedSelfClosingTag2 ()
		{
			const string content = "<name /";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name /"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedSelfClosingTagWithAttributeName1 ()
		{
			const string content = "<name attr/";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedSelfClosingTagWithAttributeName2 ()
		{
			const string content = "<name attr /";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr /"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedBeforeAttributeValue1 ()
		{
			const string content = "<name attr =";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr ="));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedBeforeAttributeValue2 ()
		{
			const string content = "<name attr = ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr = "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=\"value"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedAttributeValueQuotedWithAbortedCharacterReference ()
		{
			const string content = "<name attr=\"one & two";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=\"one & two"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedAttributeValueUnquoted ()
		{
			const string content = "<name attr=value";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=value"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedCharacterReferenceInAttributeValue1 ()
		{
			const string content = "<name attr=&";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=&"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedCharacterReferenceInAttributeValue2 ()
		{
			const string content = "<name attr=&am";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=&am"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestUnquotedAmpersandAttributeValue ()
		{
			const string content = "<name attr=&>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("name"));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("attr"));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("&"));
		}

		[Test]
		public void TestTruncatedAfterAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value\"";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=\"value\""));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestAttrbuteNameAfterAttributeValueQuoted ()
		{
			const string content = "<name attr1=\"value\"attr2=value>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("name"));
			Assert.That (tag.Attributes.Count, Is.EqualTo (2));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("attr1"));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("value"));
			Assert.That (tag.Attributes[1].Name, Is.EqualTo ("attr2"));
			Assert.That (tag.Attributes[1].Value, Is.EqualTo ("value"));
		}

		[Test]
		public void TestTruncatedSelfClosingTagAfterAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value\"/";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=\"value\"/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedSelfClosingTagBeforeAttributeValue ()
		{
			const string content = "<name attr=  /";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=  /"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestSelfClosingTagBeforeAttributeValue ()
		{
			const string content = "<name attr=  />";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("name"));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("attr"));

			// Note: per the HTML specification, a '/' in the before attribute value state begins an unquoted attribute value.
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("/"));
			Assert.That (tag.IsEmptyElement, Is.False);
		}

		[Test]
		public void TestMultipleAttributes ()
		{
			const string content = "<name attr1=\"value\"  attr2 =  value  attr3  />";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("name"));
			Assert.That (tag.Attributes.Count, Is.EqualTo (3));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("value"));
			Assert.That (tag.Attributes[1].Value, Is.EqualTo ("value"));
			Assert.That (tag.Attributes[2].Value, Is.Null);
		}

		[Test]
		public void TestTruncatedAfterAttributeValueUnquoted ()
		{
			const string content = "<name attr=value  ";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("<name attr=value  "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedEndTagOpen ()
		{
			const string content = "</";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));

			// Note: A "</" at EOF is not a truncated tag; it is always emitted as character data.
			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedRawText ()
		{
			const string content = "<style>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("a"));
		}

		[Test]
		public void TestTruncatedRawTextEndTagOpen ()
		{
			const string content = "<style></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			// Note: An incomplete end tag at EOF in the RawText state is character data, not a truncated tag.
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedRawTextEndTagOpenNonAsciiLetter ()
		{
			const string content = "<style></ ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</ "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</ "));
		}

		[Test]
		public void TestTruncatedRawTextEndTagName ()
		{
			const string content = "<style></s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</s"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			// Note: An incomplete end tag at EOF in the RawText state is character data, not a truncated tag.
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</s"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagSpace ()
		{
			const string content = "<style></bold ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold "));
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<style></bold/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold/"));
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagGreaterThan ()
		{
			const string content = "<style></bold>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold>"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold>"));
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagNonAsciiLetter ()
		{
			const string content = "<style></bold-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold-"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</bold-"));
		}

		[Test]
		public void TestRawTextEndTagNameSpace ()
		{
			string content = $"<style>a</style >";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("a"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);
		}

		[Test]
		public void TestRawTextEndTagNameSolidus ()
		{
			string content = $"<style>a</style/>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("a"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Style));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);
		}

		[Test]
		public void TestTruncatedRcData ()
		{
			const string content = "<title>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("a"));
		}

		[Test]
		public void TestTruncatedRcDataEndTagOpen ()
		{
			const string content = "<title></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			// Note: An incomplete end tag at EOF in the RcData state is character data, not a truncated tag.
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedRcDataEndTagName ()
		{
			const string content = "<title></t";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</t"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			// Note: An incomplete end tag at EOF in the RcData state is character data, not a truncated tag.
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Title));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RcData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</t"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedScriptData ()
		{
			const string content = "<script>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("a"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDash ()
		{
			const string content = "<script><!-- -";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashDash ()
		{
			const string content = "<script><!--";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagOpen ()
		{
			const string content = "<script><!---</";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagName ()
		{
			const string content = "<script><!---</s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</s"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</s"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameActiveTagSpace ()
		{
			const string content = "<script><!-- -</script ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			// FIXME: Is this correct? Or should it be ScriptData?
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("</script "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameNotActiveTagSpace ()
		{
			const string content = "<script><!-- -</style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style "));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<script><!-- -</style/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style/"));
		}

		[Test]
		public void TestTruncatedScriptDataEscaped ()
		{
			const string content = "<script><!--- ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--- "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--- "));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStart ()
		{
			const string content = "<script><!---<s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<s"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<s"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStartNotActiveTagSpace ()
		{
			const string content = "<script><!---<style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<style "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<style "));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStartNotActiveTagNonAsciiLetter ()
		{
			const string content = "<script><!---<style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<style-"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<style-"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscaped ()
		{
			const string content = "<script><!---<script>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDash ()
		{
			const string content = "<script><!---<script>-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDefault ()
		{
			const string content = "<script><!---<script>-a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-a"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-a"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDash ()
		{
			const string content = "<script><!---<script>--";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>--"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>--"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashDash ()
		{
			const string content = "<script><!---<script>---";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>---"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>---"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashGreaterThan ()
		{
			const string content = "<script><!---<script>-->";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-->"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>-->"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashLetter ()
		{
			const string content = "<script><!---<script>--a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>--a"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!---"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>--a"));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagOpen ()
		{
			const string content = "<script></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</"));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagName ()
		{
			const string content = "<script></s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</s"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</s"));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagSpace ()
		{
			const string content = "<script></style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style "));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<script></style/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style/"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style/"));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagGreaterThan ()
		{
			const string content = "<script></style>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style>"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style>"));
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagNonAsciiLetter ()
		{
			const string content = "<script></style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style-"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</style-"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapeStartNonDash ()
		{
			const string content = "<script><!a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!a"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!a"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapeStartDashNonDash ()
		{
			const string content = "<script><!-a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-a"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-a"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashLessThan ()
		{
			const string content = "<script><!-- -<";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashDefault ()
		{
			const string content = "<script><!-- -a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -a"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- -a"));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagOpenNonAsciiLetter ()
		{
			const string content = "<script><!-- </ ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- "));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</ "));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!-- "));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("</ "));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeEndNotActiveTag ()
		{
			const string content = "<script><!--<--<script>double escaped!-</style>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>double escaped!-</style>"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>double escaped!-</style>"));
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeEndNonAsciiLetter ()
		{
			const string content = "<script><!--<--<script>double escaped!-</style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>double escaped!-</style-"));

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.Script));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.ScriptData));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<!--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<--"));
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.ScriptData));
			Assert.That (((HtmlScriptDataToken) token).Data, Is.EqualTo ("<script>double escaped!-</style-"));
		}

		[Test]
		public void TestBeforeAttributeNameParseError ()
		{
			const string content = "<img \"image.png\">";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("\"image.png\""));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Unknown));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestAfterAttributeNameGreaterThan ()
		{
			const string content = "<img src >";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("src"));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Src));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestAfterAttributeNameParseError ()
		{
			const string content = "<img src \">";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (2));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("src"));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Src));
			Assert.That (tag.Attributes[1].Name, Is.EqualTo ("\""));
			Assert.That (tag.Attributes[1].Id, Is.EqualTo (HtmlAttributeId.Unknown));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestBeforeAttributeValueParseError ()
		{
			const string content = "<img src= =>";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("src"));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Src));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestBeforeAttributeValueGreaterThan ()
		{
			const string content = "<img src= >";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("src"));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Src));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestAttributeValueUnquotedParseError ()
		{
			const string content = "<img src=ab=c>";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			tag = (HtmlTagToken) token;
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("src"));
			Assert.That (tag.Attributes[0].Id, Is.EqualTo (HtmlAttributeId.Src));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("ab=c"));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestIncompleteEndTag ()
		{
			const string content = "</>";
			var tokenizer = CreateTokenizer (content);

			// Per the HTML5 spec, "</>" is a missing-end-tag-name parse error and is dropped.
			Assert.That (tokenizer.ReadNextToken (out HtmlToken _), Is.False);
		}

		[Test]
		public void TestIncompleteEndTagBetweenText ()
		{
			const string content = "a</>b";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("a"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("b"));

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestInvalidSelfClosingStartTag ()
		{
			const string content = "<name/ attr=value>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("name"));
			Assert.That (tag.Attributes.Count, Is.EqualTo (1));
		}

		[Test]
		public void TestNoScript ()
		{
			const string content = "<noscript>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("noscript"));
			Assert.That (tag.Id, Is.EqualTo (HtmlTagId.NoScript));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
		}

		[Test]
		public void TestNoScriptScriptingEnabled ()
		{
			const string content = "<noscript><b>bold</b></noscript>";
			var tokenizer = CreateTokenizer (content);

			Assert.That (tokenizer.ScriptingEnabled, Is.True);

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.NoScript));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));

			// Note: the RawText state may emit the content as multiple data tokens
			var text = string.Empty;

			while (tokenizer.ReadNextToken (out token) && token.Kind == HtmlTokenKind.Data)
				text += ((HtmlDataToken) token).Data;

			Assert.That (text, Is.EqualTo ("<b>bold</b>"));

			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.NoScript));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestNoScriptScriptingDisabled ()
		{
			const string content = "<noscript><b>bold</b></noscript>";
			var tokenizer = CreateTokenizer (content);

			tokenizer.ScriptingEnabled = false;

			Assert.That (tokenizer.ReadNextToken (out HtmlToken token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.NoScript));
			Assert.That (tokenizer.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.B));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.False);

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Data));
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("bold"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.B));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
			Assert.That (((HtmlTagToken) token).Id, Is.EqualTo (HtmlTagId.NoScript));
			Assert.That (((HtmlTagToken) token).IsEndTag, Is.True);

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		static string DescribeTokens (string content)
		{
			return DescribeTokens (CreateTokenizer (content));
		}

		static string DescribeTokens (HtmlTokenizer tokenizer)
		{
			var builder = new StringBuilder ();
			bool data = false;

			while (tokenizer.ReadNextToken (out var token)) {
				// merge adjacent data tokens since the tokenizer is free to split character data
				bool merge = data && token is HtmlDataToken;

				if (merge)
					builder.Length--;
				else if (builder.Length > 0)
					builder.Append (' ');

				data = false;

				switch (token) {
				case HtmlTagToken tag:
					builder.Append (tag.IsEndTag ? "EndTag(" : "Tag(").Append (tag.Name);
					foreach (var attr in tag.Attributes) {
						builder.Append (' ').Append (attr.Name);
						if (attr.Value != null)
							builder.Append ("=\"").Append (attr.Value).Append ('"');
					}
					builder.Append (tag.IsEmptyElement ? "/)" : ")");
					break;
				case HtmlCommentToken comment:
					builder.Append (comment.IsBogusComment ? "Bogus(" : "Comment(").Append (comment.Comment).Append (')');
					break;
				case HtmlDataToken dataToken:
					if (!merge)
						builder.Append ("Data(");
					builder.Append (dataToken.Data).Append (')');
					data = true;
					break;
				default:
					builder.Append (token.Kind);
					break;
				}
			}

			return builder.ToString ();
		}

		// Each of these used to hide the <img> tag from the tokenizer consumer (and therefore from HtmlToHtml's
		// HtmlTagCallback) because the tokenizer consumed a character that the HTML specification says must be
		// reconsumed in a different state.
		[TestCase ("<<img src=x>", "Data(<) Tag(img src=\"x\")")]
		[TestCase ("<script>x</scrip</script><img src=x>", "Tag(script) Data(x</scrip) EndTag(script) Tag(img src=\"x\")")]
		[TestCase ("<style>x</styl</style><img src=x>", "Tag(style) Data(x</styl) EndTag(style) Tag(img src=\"x\")")]
		[TestCase ("<title>x</titl</title><img src=x>", "Tag(title) Data(x</titl) EndTag(title) Tag(img src=\"x\")")]
		[TestCase ("<!DOC><img src=x>", "Bogus(DOC) Tag(img src=\"x\")")]
		[TestCase ("<![CDAT><img src=x>", "Bogus([CDAT) Tag(img src=\"x\")")]
		[TestCase ("<script><!--<script</script><img src=x>", "Tag(script) Data(<!--<script) EndTag(script) Tag(img src=\"x\")")]
		[TestCase ("<script><!--</a <script></script><!--</script><img src=x>", "Tag(script) Data(<!--</a <script></script><!--) EndTag(script) Tag(img src=\"x\")")]
		[TestCase ("<script><!--<script></a></script></script><img src=x>", "Tag(script) Data(<!--<script></a></script>) EndTag(script) Tag(img src=\"x\")")]
		[TestCase ("<script><!--<script>-</script>--></script><img src=x>", "Tag(script) Data(<!--<script>-</script>-->) EndTag(script) Tag(img src=\"x\")")]
		[TestCase ("<img/onerror=x src=y>", "Tag(img onerror=\"x\" src=\"y\")")]
		[TestCase ("<a href=/x>y</a>", "Tag(a href=\"/x\") Data(y) EndTag(a)")]
		[TestCase ("<a b/>", "Tag(a b/)")]
		[TestCase ("<a b/=c>", "Tag(a b =c)")]
		[TestCase ("<<<<", "Data(<<<<)")]
		public void TestReconsume (string content, string expected)
		{
			Assert.That (DescribeTokens (content), Is.EqualTo (expected));
		}

		const string RawTextStyleProbe = "<style><!--</style><img>-->";
		const string RawTextStyleTokens = "Tag(style) Data(<!--) EndTag(style) Tag(img) Data(-->)";
		const string ForeignStyleTokens = "Tag(style) Comment(</style><img>)";

		// Browsers only switch the tokenizer into the RAWTEXT state for <style> when the tag is processed using the
		// rules for HTML content. In SVG and MathML content, <style> is an ordinary element and "<!--" starts a comment.
		[TestCase ("", true)]
		[TestCase ("<svg>", false)]
		[TestCase ("<math>", false)]
		[TestCase ("<svg/>", true)]
		[TestCase ("<svg></svg>", true)]
		[TestCase ("<svg><g/>", false)]
		[TestCase ("<svg><svg></svg>", false)]
		[TestCase ("<svg><foreignObject>", true)]
		[TestCase ("<svg><foreignObject></foreignObject>", false)]
		[TestCase ("<svg><desc>", true)]
		[TestCase ("<svg><title>", true)]
		[TestCase ("<math><mi>", true)]
		[TestCase ("<math><mtext>", true)]
		[TestCase ("<math><mi></mi>", false)]
		[TestCase ("<math><mi><mglyph>", false)]
		[TestCase ("<math><annotation-xml>", false)]
		[TestCase ("<math><annotation-xml encoding=\"text/html\">", true)]
		[TestCase ("<math><annotation-xml encoding=\"APPLICATION/XHTML+XML\">", true)]
		[TestCase ("<math><annotation-xml><svg>", false)]
		[TestCase ("<math><svg>", false)]
		[TestCase ("<math><svg><foreignObject>", false)]
		[TestCase ("<svg><p>", true)]
		[TestCase ("<svg><g><g><div>", true)]
		[TestCase ("<svg></p>", true)]
		[TestCase ("<svg></br>", true)]
		[TestCase ("<svg><font>", false)]
		[TestCase ("<svg><font color=red>", true)]
		[TestCase ("<svg></g>", false)]
		[TestCase ("<div><svg></div>", true)]
		[TestCase ("<foo><svg></foo>", true)]
		[TestCase ("<foo><div><svg></foo>", false)]
		[TestCase ("<b><svg></b>", true)]
		[TestCase ("<table><td><svg></table>", true)]
		[TestCase ("<table><td><svg></td>", true)]
		[TestCase ("<table><td><svg></tr>", true)]
		[TestCase ("<td><svg></td>", false)]
		[TestCase ("<table><tr><td><svg><td>", false)]
		[TestCase ("<ul><li><svg><li>", true)]
		[TestCase ("<ul><li><svg></li>", true)]
		[TestCase ("<svg><foreignObject><div><svg></div>", true)]
		[TestCase ("<svg><foreignObject><svg></foreignObject>", false)]
		[TestCase ("<svg><foreignObject><svg></svg></foreignObject>", false)]
		[TestCase ("<svg><foreignObject><svg></svg></foreignObject></svg>", true)]
		public void TestForeignContentStyle (string prefix, bool rawText)
		{
			var actual = DescribeTokens (prefix + RawTextStyleProbe);

			Assert.That (actual, Does.EndWith (rawText ? RawTextStyleTokens : ForeignStyleTokens));
		}

		// Browsers ignore the self-closing flag on non-void HTML elements, so these still switch the tokenizer state.
		[TestCase ("style")]
		[TestCase ("xmp")]
		[TestCase ("iframe")]
		[TestCase ("noembed")]
		[TestCase ("noframes")]
		[TestCase ("noscript")]
		[TestCase ("title")]
		[TestCase ("textarea")]
		[TestCase ("script")]
		public void TestSelfClosingRawTextElements (string name)
		{
			var content = $"<{name}/><!--</{name}><img>-->";
			var expected = $"Tag({name}/) Data(<!--) EndTag({name}) Tag(img) Data(-->)";

			Assert.That (DescribeTokens (content), Is.EqualTo (expected));
		}

		[Test]
		public void TestSelfClosingPlainText ()
		{
			Assert.That (DescribeTokens ("<plaintext/><img>"), Is.EqualTo ("Tag(plaintext/) Data(<img>)"));
		}

		// CDATA sections are only recognized in SVG and MathML content.
		[TestCase ("<![CDATA[ x ><img>]]>", "Bogus([CDATA[ x ) Tag(img) Data(]]>)")]
		[TestCase ("<svg><![CDATA[ x ><img>]]>", "Tag(svg) Data( x ><img>)")]
		[TestCase ("<math><![CDATA[ x ><img>]]>", "Tag(math) Data( x ><img>)")]
		[TestCase ("<svg><foreignObject><![CDATA[ x ><img>]]>", "Tag(svg) Tag(foreignObject) Data( x ><img>)")]
		[TestCase ("<svg></svg><![CDATA[ x ><img>]]>", "Tag(svg) EndTag(svg) Bogus([CDATA[ x ) Tag(img) Data(]]>)")]
		public void TestCDataSectionContext (string content, string expected)
		{
			Assert.That (DescribeTokens (content), Is.EqualTo (expected));
		}

		static string Repeat (string value, int count)
		{
			var builder = new StringBuilder (value.Length * count);

			for (int i = 0; i < count; i++)
				builder.Append (value);

			return builder.ToString ();
		}

		// The open element stack must not degrade quadratically on deeply nested or unbalanced markup.
		[TestCase ("<g>", "</zz>", false)]
		[TestCase ("<g><a>", "</zz>", false)]
		[TestCase ("<g><a>", "</svg>", true)]
		[TestCase ("<div><span>", "</zz>", true)]
		[TestCase ("<g><foreignObject><div><svg>", "</zz>", false)]
		public void TestForeignContentDeepNesting (string open, string close, bool rawText)
		{
			const int Count = 100000;
			var content = "<svg>" + Repeat (open, Count) + Repeat (close, Count) + RawTextStyleProbe;
			var tokenizer = CreateTokenizer (content);
			tokenizer.MaxElementDepth = int.MaxValue;
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			HtmlToken token, last = null;
			var lastFew = new List<HtmlToken> ();

			while (tokenizer.ReadNextToken (out token)) {
				lastFew.Add (token);
				if (lastFew.Count > 6)
					lastFew.RemoveAt (0);
			}

			watch.Stop ();

			Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (30)));

			last = lastFew[lastFew.Count - 1];

			if (rawText)
				Assert.That (last.Kind, Is.EqualTo (HtmlTokenKind.Data), "Expected <style> to be raw text");
			else
				Assert.That (last.Kind, Is.EqualTo (HtmlTokenKind.Comment), "Expected <style> to be foreign content");
		}

		[Test]
		public void TestMaxElementDepth ()
		{
			var tokenizer = CreateTokenizer (string.Empty);

			Assert.That (tokenizer.MaxElementDepth, Is.EqualTo (4096), "Default");
			Assert.Throws<ArgumentOutOfRangeException> (() => tokenizer.MaxElementDepth = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => tokenizer.MaxElementDepth = -1);

			tokenizer.MaxElementDepth = 1;
			Assert.That (tokenizer.MaxElementDepth, Is.EqualTo (1));

			tokenizer.MaxElementDepth = int.MaxValue;
			Assert.That (tokenizer.MaxElementDepth, Is.EqualTo (int.MaxValue));
		}

		// Once the depth limit is exceeded, the tokenizer can no longer tell how a browser would tokenize the rest of the
		// input, so the start tag that exceeded the limit must be the last tag and the rest of the input must be literal text.
		[TestCase ("<div><span><b><i><img src=x>&amp;", "Tag(div) Tag(span) Tag(b) Tag(i) Data(<img src=x>&amp;)")]
		[TestCase ("<div><span><b></b><i><img src=x>", "Tag(div) Tag(span) Tag(b) EndTag(b) Tag(i) Tag(img src=\"x\")")]
		[TestCase ("<div><div><div><div><div><span><b><img src=x>", "Tag(div) Tag(div) Tag(div) Tag(div) Tag(div) Tag(span) Tag(b) Tag(img src=\"x\")")]
		[TestCase ("<div><span><svg><style><img src=x></style>", "Tag(div) Tag(span) Tag(svg) Tag(style) Data(<img src=x></style>)")]
		[TestCase ("<div><svg><g><foreignObject><style><img src=x></style>", "Tag(div) Tag(svg) Tag(g) Tag(foreignObject) Data(<style><img src=x></style>)")]
		[TestCase ("<div><span><b><svg><![CDATA[<img src=x>]]>", "Tag(div) Tag(span) Tag(b) Tag(svg) Data(<![CDATA[<img src=x>]]>)")]
		[TestCase ("<table><tr><td><img src=x>", "Tag(table) Tag(tr) Tag(td) Data(<img src=x>)")]
		public void TestMaxElementDepthExceeded (string content, string expected)
		{
			var tokenizer = CreateTokenizer (content);
			tokenizer.MaxElementDepth = 3;

			Assert.That (DescribeTokens (tokenizer), Is.EqualTo (expected));
		}

		[Test]
		public void TestMaxElementDepthExceededDataIsEncoded ()
		{
			var tokenizer = new HtmlTokenizer (new StringReader ("<div><span><b><i><img src=x onerror=alert(1)>&amp;")) {
				DecodeCharacterReferences = false,
				MaxElementDepth = 3
			};
			HtmlToken last = null;

			while (tokenizer.ReadNextToken (out var token))
				last = token;

			Assert.That (last, Is.InstanceOf<HtmlDataToken> ());

			using var writer = new StringWriter ();
			last.WriteTo (writer);

			Assert.That (writer.ToString (), Is.EqualTo ("&lt;img src=x onerror=alert(1)&gt;&amp;amp;"));
		}

		[Test]
		public void TestMaxElementDepthNestedTables ()
		{
			const int Count = 100000;
			var content = Repeat ("<table><tr><td>", Count) + "<img src=x>";
			var tokenizer = CreateTokenizer (content);
			int tags = 0;
			HtmlToken last = null;

			while (tokenizer.ReadNextToken (out var token)) {
				if (token.Kind == HtmlTokenKind.Tag)
					tags++;
				last = token;
			}

			// Each <table><tr><td> pushes 4 entries (including the implied <tbody>), so the <table> start tag
			// following the first 1024 repetitions is the last tag.
			Assert.That (tags, Is.EqualTo (3 * 1024 + 1));
			Assert.That (last, Is.InstanceOf<HtmlDataToken> ());
			Assert.That (((HtmlDataToken) last).Data, Does.EndWith ("<img src=x>"));
		}

		// An abruptly terminated DOCTYPE identifier must not leave a stale quote character behind that would cause a
		// character reference in an unquoted attribute value of the next tag to resume in the quoted attribute value state.
		[TestCase ("<!DOCTYPE l PUBLIC\"><k g=&>")]
		[TestCase ("<!DOCTYPE l PUBLIC '><k g=&>")]
		[TestCase ("<!DOCTYPE l SYSTEM \"><k g=&>")]
		[TestCase ("<!DOCTYPE l PUBLIC \"a\" '><k g=&>")]
		public void TestAbruptDocTypeIdentifierQuote (string input)
		{
			var tokenizer = CreateTokenizer (input);

			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.DocType));
			Assert.That (((HtmlDocTypeToken) token).ForceQuirksMode, Is.True);

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));

			var tag = (HtmlTagToken) token;
			Assert.That (tag.Name, Is.EqualTo ("k"));
			Assert.That (tag.Attributes, Has.Count.EqualTo (1));
			Assert.That (tag.Attributes[0].Name, Is.EqualTo ("g"));
			Assert.That (tag.Attributes[0].Value, Is.EqualTo ("&"));

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		static void AssertReusedDataTokensMatch (string html, string label)
		{
			var expected = new HtmlTokenizer (new StringReader (html)) { DecodeCharacterReferences = false };
			var actual = new HtmlTokenizer (new StringReader (html)) { DecodeCharacterReferences = false, ReuseDataTokens = true };
			var expectedOutput = new StringWriter ();
			var actualOutput = new StringWriter ();
			int index = 0;

			while (expected.ReadNextToken (out var expectedToken)) {
				Assert.That (actual.ReadNextToken (out var actualToken), Is.True, $"{label}: token #{index}");
				Assert.That (actualToken.Kind, Is.EqualTo (expectedToken.Kind), $"{label}: token #{index} kind");
				Assert.That (actualToken.GetType (), Is.EqualTo (expectedToken.GetType ()), $"{label}: token #{index} type");

				// Note: Write the token before accessing Data so that the CharBuffer-backed path gets exercised.
				expectedToken.WriteTo (expectedOutput);
				actualToken.WriteTo (actualOutput);
				Assert.That (actualOutput.ToString (), Is.EqualTo (expectedOutput.ToString ()), $"{label}: token #{index} output");

				if (expectedToken is HtmlDataToken expectedData)
					Assert.That (((HtmlDataToken) actualToken).Data, Is.EqualTo (expectedData.Data), $"{label}: token #{index} data");

				index++;
			}

			Assert.That (actual.ReadNextToken (out _), Is.False, label);
		}

		[TestCase ("short<b>" + "a much longer run of character data that forces the reused buffer to grow" + "<i>x</i>" + "mid-length run<br>y")]
		[TestCase ("<script><!--<script>var a = 1 < 2;</script>--></script>after")]
		[TestCase ("<script><!-- a -- < b --> c</script><style>p { a: b < c }</style>")]
		[TestCase ("<svg><![CDATA[a<b]]>text<![CDATA[]]><![CDATA[longer cdata section]]></svg>")]
		[TestCase ("a <</>c> b <<i>x &amp; &lt; & y")]
		[TestCase ("<textarea>a<b>&amp;</textarea><title>x</title><plaintext>tail <b>")]
		public void TestReusedDataTokens (string html)
		{
			AssertReusedDataTokensMatch (html, html);
		}

		[Test]
		public void TestReusedDataTokensCorpus ()
		{
			foreach (var path in Directory.GetFiles (Path.Combine (TestHelper.ProjectDir, "TestData", "html"), "*.html"))
				AssertReusedDataTokensMatch (File.ReadAllText (path), Path.GetFileName (path));
		}

		[Test]
		public void TestReusedDataTokenDataAfterReuse ()
		{
			var tokenizer = new HtmlTokenizer (new StringReader ("first<b>second, but longer<i>3")) { ReuseDataTokens = true };

			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
			var data = (HtmlDataToken) token;
			Assert.That (data.Data, Is.EqualTo ("first"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));

			// Accessing Data converts the reusable token's content into a string, so the next reuse must start afresh.
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token, Is.SameAs (data));
			Assert.That (data.Data, Is.EqualTo ("second, but longer"));

			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (((HtmlDataToken) token).Data, Is.EqualTo ("3"));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		// Tag names, attribute names and short attribute values are shared via a small direct-mapped cache, so
		// make sure that cache collisions and evictions never cause the wrong string to be returned.
		[Test]
		public void TestCachedTagAndAttributeNames ()
		{
			const int count = 4096;
			var builder = new StringBuilder ();

			for (int i = 0; i < count; i++) {
				builder.Append ($"<t{i} a{i}=\"v{i}\" A{i % 7}='{new string ('x', i % 40)}'>");
				builder.Append ($"</t{i}><t{i % 13}/>");
			}

			var tokenizer = CreateTokenizer (builder.ToString ());

			for (int i = 0; i < count; i++) {
				Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
				var tag = (HtmlTagToken) token;
				Assert.That (tag.Name, Is.EqualTo ($"t{i}"));
				Assert.That (tag.IsEndTag, Is.False);
				Assert.That (tag.Attributes, Has.Count.EqualTo (2));
				Assert.That (tag.Attributes[0].Name, Is.EqualTo ($"a{i}"));
				Assert.That (tag.Attributes[0].Value, Is.EqualTo ($"v{i}"));
				Assert.That (tag.Attributes[1].Name, Is.EqualTo ($"A{i % 7}"));
				Assert.That (tag.Attributes[1].Value, Is.EqualTo (new string ('x', i % 40)));

				Assert.That (tokenizer.ReadNextToken (out token), Is.True);
				tag = (HtmlTagToken) token;
				Assert.That (tag.Name, Is.EqualTo ($"t{i}"));
				Assert.That (tag.IsEndTag, Is.True);
				Assert.That (tag.Attributes, Is.Empty);

				Assert.That (tokenizer.ReadNextToken (out token), Is.True);
				tag = (HtmlTagToken) token;
				Assert.That (tag.Name, Is.EqualTo ($"t{i % 13}"));
				Assert.That (tag.IsEmptyElement, Is.True);
				Assert.That (tag.Attributes, Is.Empty);
				Assert.That (tag.Attributes.IndexOf ("a"), Is.EqualTo (-1));
				Assert.That (tag.Attributes.IndexOf (HtmlAttributeId.Href), Is.EqualTo (-1));
				Assert.That (tag.Attributes.Contains ("a"), Is.False);
				Assert.That (tag.Attributes.TryGetValue ("a", out _), Is.False);
				Assert.Throws<ArgumentOutOfRangeException> (() => _ = tag.Attributes[0]);
			}

			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}
	}
}
