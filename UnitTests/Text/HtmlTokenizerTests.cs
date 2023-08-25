//
// HtmlTokenizerTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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

			encoding = encoding ?? Encoding.GetEncoding (1252);

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

				Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);

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
						Assert.Fail ("Unhandled token type: {0}", token.Kind);
						break;
					}
				}

				Assert.AreEqual (HtmlTokenizerState.EndOfFile, tokenizer.TokenizerState);
			} finally {
				reader?.Dispose ();
				stream?.Dispose ();
			}

			if (!File.Exists (tokens))
				File.WriteAllText (tokens, actual.ToString ());

			if (!File.Exists (outpath))
				File.WriteAllText (outpath, output.ToString ());

			Assert.AreEqual (expected, actual.ToString (), "The token stream does not match the expected tokens.");
			Assert.AreEqual (expectedOutput, output.ToString (), "The output stream does not match the expected output.");
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

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TokenizationLongerCharacterReference ()
		{
			const string content = "&abcdefghijklmnopqrstvwxyzABCDEFGHIJKLMNOPQRSTV;";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			var cdata = (HtmlDataToken) token;
			Assert.AreEqual (content, cdata.Data);
		}

		[Test]
		public void TokenizationStartTagDetection ()
		{
			var tokenizer = CreateTokenizer ("<p>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("p", tag.Name);
			Assert.IsFalse (tag.IsEndTag);
			Assert.IsFalse (tag.IsEmptyElement);
		}

		[Test]
		public void TokenizationBogusCommentEmpty ()
		{
			var tokenizer = CreateTokenizer ("<!>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			var comment = (HtmlCommentToken) token;
			Assert.AreEqual ("", comment.Comment);
		}

		[Test]
		public void TokenizationBogusCommentQuestionMark ()
		{
			var tokenizer = CreateTokenizer ("<?>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			var comment = (HtmlCommentToken) token;
			Assert.AreEqual ("?", comment.Comment);
		}

		[Test]
		public void TokenizationBogusCommentClosingTag ()
		{
			var tokenizer = CreateTokenizer ("</ >");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			var comment = (HtmlCommentToken) token;
			Assert.AreEqual (" ", comment.Comment);
		}

		[Test]
		public void TokenizationTagNameDetection ()
		{
			var tokenizer = CreateTokenizer ("<span>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual ("span", ((HtmlTagToken) token).Name);
		}

		[Test]
		public void TokenizationTagSelfClosingDetected ()
		{
			var tokenizer = CreateTokenizer ("<img />");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.IsTrue (((HtmlTagToken) token).IsEmptyElement);
		}

		[Test]
		public void TokenizationAttributesDetected ()
		{
			var tokenizer = CreateTokenizer ("<a target='_blank' href='http://whatever' title='ho'>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (3, ((HtmlTagToken) token).Attributes.Count);
		}

		[Test]
		public void TokenizationAttributeNameDetection ()
		{
			var tokenizer = CreateTokenizer ("<input required>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual ("required", ((HtmlTagToken) token).Attributes[0].Name);
		}

		[Test]
		public void TokenizationTagMixedCaseHandling ()
		{
			var tokenizer = CreateTokenizer ("<InpUT>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTagId.Input, ((HtmlTagToken) token).Id);
		}

		[Test]
		public void TokenizationTagSpacesBehind ()
		{
			var tokenizer = CreateTokenizer ("<i   >");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual ("i", ((HtmlTagToken) token).Name);
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

			Assert.AreEqual ("I'm ∉ I tell you", str);
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

			Assert.AreEqual ("I'm ¬it; I tell you", str);
		}

		[Test]
		public void TokenizationDoctypeDetected ()
		{
			var tokenizer = CreateTokenizer ("<!doctype html>");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
		}

		[Test]
		public void TokenizationCommentDetected ()
		{
			var tokenizer = CreateTokenizer ("<!-- hi my friend -->");

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
		}

		[Test]
		public void TokenizationCDataDetected ()
		{
			var tokenizer = CreateTokenizer ("<![CDATA[hi mum how <!-- are you doing />]]>");

			//tokenizer.IsAcceptingCharacterData = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.CData, token.Kind);
		}

		[Test]
		public void TokenizationCDataCorrectCharacters ()
		{
			var sb = new StringBuilder ();
			var tokenizer = CreateTokenizer ("<![CDATA[hi mum how <!-- are you doing />]]>");
			HtmlToken token;

			//tokenizer.IsAcceptingCharacterData = true;

			while (tokenizer.ReadNextToken (out token)) {
				if (token.Kind == HtmlTokenKind.CData)
					sb.Append (((HtmlCDataToken) token).Data);
			}

			Assert.AreEqual ("hi mum how <!-- are you doing />", sb.ToString ());
		}

		[Test]
		public void TokenizationUnusualDoctype ()
		{
			var tokenizer = CreateTokenizer ("<!DOCTYPE root_element SYSTEM \"DTD_location\">");
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);

			var d = (HtmlDocTypeToken) token;
			Assert.IsNotNull (d.Name);
			Assert.AreEqual ("root_element", d.Name);
			Assert.AreEqual ("DTD_location", d.SystemIdentifier);
		}

		[Test]
		public void TokenizationOnlyCarriageReturn ()
		{
			var tokenizer = CreateTokenizer ("\r");
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("\r", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TokenizationOnlyLineFeed ()
		{
			var tokenizer = CreateTokenizer ("\n");
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("\n", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TokenizationCarriageReturnLineFeed ()
		{
			var tokenizer = CreateTokenizer ("\r\n");
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("\r\n", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TokenizationLongestLegalCharacterReference ()
		{
			var content = "&CounterClockwiseContourIntegral;";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("∳", ((HtmlDataToken) token).Data);
		}

		//[Test]
		//public void TokenizationLongestIllegalCharacterReference ()
		//{
		//	var content = "&CounterClockwiseContourIntegralWithWrongName;";
		//	var tokenizer = CreateTokenizer (content);

		//	Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
		//	Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
		//	Assert.AreEqual (content, ((HtmlDataToken) token).Data);
		//}

		[Test]
		public void TestDataCharacterReferencesNotDecoded ()
		{
			const string content = "<b>check &CounterClockwiseContourIntegral; is not decoded</b>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			tokenizer.DecodeCharacterReferences = false;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.B, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("check &CounterClockwiseContourIntegral; is not decoded", ((HtmlDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.B, ((HtmlTagToken) token).Id);
			Assert.IsTrue (((HtmlTagToken) token).IsEndTag);
		}

		[Test]
		public void TestRcDataCharacterReferencesNotDecoded ()
		{
			const string content = "<title>check &CounterClockwiseContourIntegral; is not decoded</title>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			tokenizer.DecodeCharacterReferences = false;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("check &CounterClockwiseContourIntegral; is not decoded", ((HtmlDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.IsTrue (((HtmlTagToken) token).IsEndTag);
		}

		// The following unit tests are for error conditions

		[Test]
		public void TestTruncatedMarkupDeclarationOpen ()
		{
			const string content = "<!-";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<!-", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedDocType ()
		{
			const string content = "<!DOCTYPE";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsTrue (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestTruncatedDocTypeSpace ()
		{
			const string content = "<!DOCTYPE ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsTrue (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNoName ()
		{
			const string content = "<!DOCTYPE  >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsTrue (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestTruncatedDocTypeName ()
		{
			const string content = "<!DOCTYPE HTML";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsTrue (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeWithName ()
		{
			const string content = "<!DOCTYPE HTML>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsFalse (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestTruncatedAfterDocTypeName ()
		{
			const string content = "<!DOCTYPE HTML ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			Assert.IsTrue (((HtmlDocTypeToken) token).ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNameParseError ()
		{
			const string content = "<!DOCTYPE HTML\0>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML\uFFFD", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNameSpace ()
		{
			const string content = "<!DOCTYPE HTML >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNameSpaceBogus ()
		{
			const string content = "<!DOCTYPE HTML BOGUS>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestAfterDocTypeNameBogusDocType ()
		{
			const string content = "<!DOCTYPE HTML PUBLISH>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestBogusDocTypeAfterName ()
		{
			const string content = "<!DOCTYPE HTML BOGUS >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNamePublicX ()
		{
			const string content = "<!DOCTYPE HTML PUBLICX>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsTrue (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestDocTypePublicIdentifierQuotedParseError ()
		{
			const string content = "<!DOCTYPE HTML PUBLIC \"public-identifier\0\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("PUBLIC", doctype.PublicKeyword);
			Assert.AreEqual ("public-identifier\uFFFD", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypeSystemIdentifierQuotedParseError ()
		{
			const string content = "<!DOCTYPE HTML SYSTEM \"system-identifier\0\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("SYSTEM", doctype.SystemKeyword);
			Assert.AreEqual ("system-identifier\uFFFD", doctype.SystemIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeAfterPublicKeyword ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.AreEqual ("HTML", doctype.Name);
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
		}

		[Test]
		public void TestTruncatedDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
		}

		[Test]
		public void TestIncompleteDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc  >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
		}

		[Test]
		public void TestInvalidDocTypeBeforePublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc  value>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual (null, doctype.PublicIdentifier);
		}

		[Test]
		public void TestIncompleteDocTypePublicIdentifierQuoted ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypePublicIdentifierQuoted ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypePublicWithoutSpace ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc\"value\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypeQuoteAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypeCharAfterPublicIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"x>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\" ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestInvalidDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"  x>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypeBetweenPublicAndSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc \"value\"  >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual ("value", doctype.PublicIdentifier);
		}

		[Test]
		public void TestDocTypeNamePublicClose ()
		{
			const string content = "<!DOCTYPE HTML PuBlIc>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("PuBlIc", doctype.PublicKeyword);
			Assert.AreEqual (null, doctype.PublicIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeAfterSystemKeyword ()
		{
			const string content = "<!DOCTYPE HTML SySTeM";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual (null, doctype.SystemIdentifier);
		}

		[Test]
		public void TestDocTypeSystemWithoutSpace ()
		{
			const string content = "<!DOCTYPE HTML SySTeM\"value\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeBeforeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
		}

		[Test]
		public void TestDocTypeBeforeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM  >";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
		}

		[Test]
		public void TestDocTypeBeforeSystemIdentifierX ()
		{
			const string content = "<!DOCTYPE HTML SySTeM  x>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
		}

		[Test]
		public void TestTruncatedDocTypeSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestDocTypeQuoteAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\"\">";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestDocTypeCharAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\"x>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsFalse (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestTruncatedDocTypeAfterSystemIdentifier ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\" ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestTruncatedBogusDocType ()
		{
			const string content = "<!DOCTYPE HTML SySTeM \"value\" x";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
			Assert.AreEqual ("SySTeM", doctype.SystemKeyword);
			Assert.AreEqual ("value", doctype.SystemIdentifier);
		}

		[Test]
		public void TestDocTypeNameSystemX ()
		{
			const string content = "<!DOCTYPE HTML SYSTEMX>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestDocTypeNameSystem ()
		{
			const string content = "<!DOCTYPE HTML SYSTEM>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.DocType, token.Kind);
			var doctype = (HtmlDocTypeToken) token;
			Assert.IsTrue (doctype.ForceQuirksMode);
		}

		[Test]
		public void TestTruncatedDocTypeToken ()
		{
			const string content = "<!DOC";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<!DOC", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestNotQuiteDocTypeBogusComment ()
		{
			const string content = "<!DOCS>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("DOCS", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedNotQuiteDocTypeBogusComment ()
		{
			const string content = "<!DOCS";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("DOCS", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestNotQuiteCDATABogusComment ()
		{
			const string content = "<![CDAT[>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("[CDAT[", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedNotQuiteCDATABogusComment ()
		{
			const string content = "<![CDAT[";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("[CDAT[", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedCDATA ()
		{
			const string content = "<![CDATA";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<![CDATA", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedCDATASection ()
		{
			const string content = "<![CDATA[this is some cdata]]";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.CData, token.Kind);
			Assert.AreEqual ("this is some cdata]]", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.CData, token.Kind);
			Assert.AreEqual ("this is some cdata]]", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedComment ()
		{
			const string content = "<!--comment";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedCommentEndDash ()
		{
			const string content = "<!--comment-";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestEmptyComment0 ()
		{
			const string content = "<!-->"; // malformed
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestEmptyComment1 ()
		{
			const string content = "<!--->"; // malformed
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestEmptyComment2 ()
		{
			const string content = "<!---->"; // correct
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedEmptyComment0 ()
		{
			const string content = "<!--";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedEmptyComment1 ()
		{
			const string content = "<!---";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedEmptyComment2 ()
		{
			const string content = "<!----";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual (string.Empty, ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestDashComment ()
		{
			const string content = "<!---comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("-comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestDashDashComment ()
		{
			const string content = "<!----comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("--comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDash ()
		{
			const string content = "<!--comment--->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment-", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDashDash ()
		{
			const string content = "<!--comment---->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment--", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDashComment ()
		{
			const string content = "<!--comment-comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment-comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDashDashComment ()
		{
			const string content = "<!--comment--comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment--comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentEndBang ()
		{
			const string content = "<!--comment--!>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedCommentEndBang ()
		{
			const string content = "<!--comment--!";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDashDashBang ()
		{
			const string content = "<!--comment--!-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment--!", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestCommentDashDashBangComment ()
		{
			const string content = "<!--comment--!comment-->";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Comment, token.Kind);
			Assert.AreEqual ("comment--!comment", ((HtmlCommentToken) token).Comment);
		}

		[Test]
		public void TestTruncatedCharacterReferenceStart ()
		{
			const string content = "&";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("&", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedCharacterReference ()
		{
			const string content = "&am";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("&am", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedTagOpen ()
		{
			const string content = "<";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTagOpenDigit ()
		{
			const string content = "<5>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<5>", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedTagName ()
		{
			const string content = "<nam";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<nam", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedBeforeAttributeName ()
		{
			const string content = "<name ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedAttributeName ()
		{
			const string content = "<name attr";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedAfterAttributeName ()
		{
			const string content = "<name attr  ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr  ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedSelfClosingTag1 ()
		{
			const string content = "<name/";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name/", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedSelfClosingTag2 ()
		{
			const string content = "<name /";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name /", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedSelfClosingTagWithAttributeName1 ()
		{
			const string content = "<name attr/";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr/", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedSelfClosingTagWithAttributeName2 ()
		{
			const string content = "<name attr /";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr /", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedBeforeAttributeValue1 ()
		{
			const string content = "<name attr =";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr =", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedBeforeAttributeValue2 ()
		{
			const string content = "<name attr = ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr = ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=\"value", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedAttributeValueQuotedWithAbortedCharacterReference ()
		{
			const string content = "<name attr=\"one & two";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=\"one & two", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedAttributeValueUnquoted ()
		{
			const string content = "<name attr=value";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=value", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedCharacterReferenceInAttributeValue1 ()
		{
			const string content = "<name attr=&";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=&", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedCharacterReferenceInAttributeValue2 ()
		{
			const string content = "<name attr=&am";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=&am", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestUnquotedAmpersandAttributeValue ()
		{
			const string content = "<name attr=&>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("name", tag.Name);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("attr", tag.Attributes[0].Name);
			Assert.AreEqual ("&", tag.Attributes[0].Value);
		}

		[Test]
		public void TestTruncatedAfterAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value\"";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=\"value\"", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestAttrbuteNameAfterAttributeValueQuoted ()
		{
			const string content = "<name attr1=\"value\"attr2=value>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("name", tag.Name);
			Assert.AreEqual (2, tag.Attributes.Count);
			Assert.AreEqual ("attr1", tag.Attributes[0].Name);
			Assert.AreEqual ("value", tag.Attributes[0].Value);
			Assert.AreEqual ("attr2", tag.Attributes[1].Name);
			Assert.AreEqual ("value", tag.Attributes[1].Value);
		}

		[Test]
		public void TestTruncatedSelfClosingTagAfterAttributeValueQuoted ()
		{
			const string content = "<name attr=\"value\"/";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=\"value\"/", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedSelfClosingTagBeforeAttributeValue ()
		{
			const string content = "<name attr=  /";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=  /", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestSelfClosingTagBeforeAttributeValue ()
		{
			const string content = "<name attr=  />";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("name", tag.Name);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("attr", tag.Attributes[0].Name);
			Assert.AreEqual (null, tag.Attributes[0].Value);
		}

		[Test]
		public void TestMultipleAttributes ()
		{
			const string content = "<name attr1=\"value\"  attr2 =  value  attr3  />";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("name", tag.Name);
			Assert.AreEqual (3, tag.Attributes.Count);
			Assert.AreEqual ("value", tag.Attributes[0].Value);
			Assert.AreEqual ("value", tag.Attributes[1].Value);
			Assert.IsNull (tag.Attributes[2].Value);
		}

		[Test]
		public void TestTruncatedAfterAttributeValueUnquoted ()
		{
			const string content = "<name attr=value  ";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("<name attr=value  ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedEndTagOpen ()
		{
			const string content = "</";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedRawText ()
		{
			const string content = "<style>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("a", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRawTextEndTagOpen ()
		{
			const string content = "<style></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedRawTextEndTagOpenNonAsciiLetter ()
		{
			const string content = "<style></ ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</ ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</ ", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRawTextEndTagName ()
		{
			const string content = "<style></s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</s", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagSpace ()
		{
			const string content = "<style></bold ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold ", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<style></bold/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold/", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold/", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagGreaterThan ()
		{
			const string content = "<style></bold>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold>", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold>", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRawTextEndTagNameNotActiveTagNonAsciiLetter ()
		{
			const string content = "<style></bold-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold-", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</bold-", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestRawTextEndTagNameSpace ()
		{
			string content = $"<style>a</style >";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("a", ((HtmlDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.IsTrue (((HtmlTagToken) token).IsEndTag);
		}

		[Test]
		public void TestRawTextEndTagNameSolidus ()
		{
			string content = $"<style>a</style/>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("a", ((HtmlDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Style, ((HtmlTagToken) token).Id);
			Assert.IsTrue (((HtmlTagToken) token).IsEndTag);
		}

		[Test]
		public void TestTruncatedRcData ()
		{
			const string content = "<title>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("a", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedRcDataEndTagOpen ()
		{
			const string content = "<title></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedRcDataEndTagName ()
		{
			const string content = "<title></t";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</t", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Title, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.RcData, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedScriptData ()
		{
			const string content = "<script>a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDash ()
		{
			const string content = "<script><!-- -";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashDash ()
		{
			const string content = "<script><!--";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagOpen ()
		{
			const string content = "<script><!---</";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagName ()
		{
			const string content = "<script><!---</s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</s", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</s", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameActiveTagSpace ()
		{
			const string content = "<script><!-- -</script ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			// FIXME: Is this correct? Or should it be ScriptData?
			Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			Assert.AreEqual ("</script ", ((HtmlDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameNotActiveTagSpace ()
		{
			const string content = "<script><!-- -</style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style ", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style ", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<script><!-- -</style/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style/", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style/", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscaped ()
		{
			const string content = "<script><!--- ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--- ", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--- ", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStart ()
		{
			const string content = "<script><!---<s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<s", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<s", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStartNotActiveTagSpace ()
		{
			const string content = "<script><!---<style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<style ", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<style ", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeStartNotActiveTagNonAsciiLetter ()
		{
			const string content = "<script><!---<style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<style-", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<style-", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscaped ()
		{
			const string content = "<script><!---<script>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDash ()
		{
			const string content = "<script><!---<script>-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDefault ()
		{
			const string content = "<script><!---<script>-a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-a", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDash ()
		{
			const string content = "<script><!---<script>--";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>--", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>--", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashDash ()
		{
			const string content = "<script><!---<script>---";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>---", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>---", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashGreaterThan ()
		{
			const string content = "<script><!---<script>-->";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-->", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>-->", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapedDashDashLetter ()
		{
			const string content = "<script><!---<script>--a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>--a", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!---", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>--a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagOpen ()
		{
			const string content = "<script></";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagName ()
		{
			const string content = "<script></s";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</s", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</s", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagSpace ()
		{
			const string content = "<script></style ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style ", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style ", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagSolidus ()
		{
			const string content = "<script></style/";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style/", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style/", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagGreaterThan ()
		{
			const string content = "<script></style>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style>", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style>", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEndTagNameNotActiveTagNonAsciiLetter ()
		{
			const string content = "<script></style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style-", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</style-", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapeStartNonDash ()
		{
			const string content = "<script><!a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!a", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapeStartDashNonDash ()
		{
			const string content = "<script><!-a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-a", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashLessThan ()
		{
			const string content = "<script><!-- -<";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual ("<", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedDashDefault ()
		{
			const string content = "<script><!-- -a";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -a", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- -a", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataEscapedEndTagOpenNonAsciiLetter ()
		{
			const string content = "<script><!-- </ ";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- ", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</ ", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!-- ", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("</ ", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeEndNotActiveTag ()
		{
			const string content = "<script><!--<--<script>double escaped!-</style>";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>double escaped!-</style>", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>double escaped!-</style>", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestTruncatedScriptDataDoubleEscapeEndNonAsciiLetter ()
		{
			const string content = "<script><!--<--<script>double escaped!-</style-";
			var tokenizer = CreateTokenizer (content);
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>double escaped!-</style-", ((HtmlScriptDataToken) token).Data);

			tokenizer = CreateTokenizer (content);
			tokenizer.IgnoreTruncatedTags = true;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			Assert.AreEqual (HtmlTagId.Script, ((HtmlTagToken) token).Id);
			Assert.AreEqual (HtmlTokenizerState.ScriptData, tokenizer.TokenizerState);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<!--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<--", ((HtmlScriptDataToken) token).Data);
			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.ScriptData, token.Kind);
			Assert.AreEqual ("<script>double escaped!-</style-", ((HtmlScriptDataToken) token).Data);
		}

		[Test]
		public void TestBeforeAttributeNameParseError ()
		{
			const string content = "<img \"image.png\">";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("\"image.png\"", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Unknown, tag.Attributes[0].Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestAfterAttributeNameGreaterThan ()
		{
			const string content = "<img src >";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("src", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Src, tag.Attributes[0].Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestAfterAttributeNameParseError ()
		{
			const string content = "<img src \">";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (2, tag.Attributes.Count);
			Assert.AreEqual ("src", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Src, tag.Attributes[0].Id);
			Assert.AreEqual ("\"", tag.Attributes[1].Name);
			Assert.AreEqual (HtmlAttributeId.Unknown, tag.Attributes[1].Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestBeforeAttributeValueParseError ()
		{
			const string content = "<img src= =>";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("src", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Src, tag.Attributes[0].Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestBeforeAttributeValueGreaterThan ()
		{
			const string content = "<img src= >";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("src", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Src, tag.Attributes[0].Id);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestAttributeValueUnquotedParseError ()
		{
			const string content = "<img src=ab=c>";
			var tokenizer = CreateTokenizer (content);
			HtmlTagToken tag;
			HtmlToken token;

			Assert.IsTrue (tokenizer.ReadNextToken (out token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			tag = (HtmlTagToken) token;
			Assert.AreEqual (HtmlTagId.Image, tag.Id);
			Assert.AreEqual (1, tag.Attributes.Count);
			Assert.AreEqual ("src", tag.Attributes[0].Name);
			Assert.AreEqual (HtmlAttributeId.Src, tag.Attributes[0].Id);
			Assert.AreEqual ("ab=c", tag.Attributes[0].Value);
			Assert.AreEqual (HtmlTokenizerState.Data, tokenizer.TokenizerState);
			Assert.IsFalse (tokenizer.ReadNextToken (out _));
		}

		[Test]
		public void TestIncompleteEndTag ()
		{
			const string content = "</>";
			var tokenizer = CreateTokenizer (content);

			// TODO: is this the expected behavior?
			Assert.IsFalse (tokenizer.ReadNextToken (out HtmlToken _));
			//Assert.AreEqual (HtmlTokenKind.Data, token.Kind);
			//Assert.AreEqual ("</>", ((HtmlDataToken) token).Data);
		}

		[Test]
		public void TestInvalidSelfClosingStartTag ()
		{
			const string content = "<name/ attr=value>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("name", tag.Name);
			Assert.AreEqual (1, tag.Attributes.Count);
		}

		[Test]
		public void TestNoScript ()
		{
			const string content = "<noscript>";
			var tokenizer = CreateTokenizer (content);

			Assert.IsTrue (tokenizer.ReadNextToken (out HtmlToken token));
			Assert.AreEqual (HtmlTokenKind.Tag, token.Kind);
			var tag = (HtmlTagToken) token;
			Assert.AreEqual ("noscript", tag.Name);
			Assert.AreEqual (HtmlTagId.NoScript, tag.Id);
			Assert.AreEqual (HtmlTokenizerState.RawText, tokenizer.TokenizerState);
		}
	}
}
