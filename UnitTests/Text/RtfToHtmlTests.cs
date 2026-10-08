//
// RtfToHtmlTests.cs
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
using MimeKit.Utils;

namespace UnitTests.Text {
	[TestFixture]
	public class RtfToHtmlTests
	{
		static readonly string NewLine = Environment.NewLine;

		[Test]
		public void TestArgumentExceptions ()
		{
			using var reader = new StringReader ("");
			using var writer = new StringWriter ();
			var converter = new RtfToHtml ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxFontTableEntries = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxFontTableEntries = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxColorTableEntries = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxColorTableEntries = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxGroupDepth = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxGroupDepth = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxElementDepth = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxElementDepth = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, writer));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new RtfToHtml ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.ExtractEncapsulatedHtml, Is.True, "ExtractEncapsulatedHtml");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.FooterFormat, Is.EqualTo (HeaderFooterFormat.Text), "FooterFormat");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.HeaderFormat, Is.EqualTo (HeaderFooterFormat.Text), "HeaderFormat");
			Assert.That (converter.HtmlTagCallback, Is.Null, "HtmlTagCallback");
			Assert.That (converter.InputEncoding, Is.EqualTo (CharsetUtils.Latin1), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.RichText), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Html), "OutputFormat");
			Assert.That (converter.OutputHtmlFragment, Is.False, "OutputHtmlFragment");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
			Assert.That (converter.NoScriptHandling, Is.EqualTo (HtmlNoScriptHandling.Unwrap), "NoScriptHandling");
			Assert.That (converter.MaxFontTableEntries, Is.EqualTo (4096), "MaxFontTableEntries");
			Assert.That (converter.MaxColorTableEntries, Is.EqualTo (4096), "MaxColorTableEntries");
			Assert.That (converter.MaxGroupDepth, Is.EqualTo (4096), "MaxGroupDepth");
			Assert.That (converter.MaxElementDepth, Is.EqualTo (4096), "MaxElementDepth");
		}

		static string Convert (string rtf, Action<RtfToHtml> configure = null)
		{
			var converter = new RtfToHtml { OutputHtmlFragment = true };

			configure?.Invoke (converter);

			return converter.Convert (rtf);
		}

		[Test]
		public void TestSimpleDocument ()
		{
			Assert.That (Convert ("{\\rtf1 Hello}", c => c.OutputHtmlFragment = false), Is.EqualTo ("<html><body><div>Hello</div>" + NewLine + "</body></html>"));
			Assert.That (Convert ("{\\rtf1 Hello}"), Is.EqualTo ("<div>Hello</div>" + NewLine));
		}

		[Test]
		public void TestEmptyInput ()
		{
			Assert.That (Convert (string.Empty), Is.EqualTo (string.Empty));
			Assert.That (Convert (string.Empty, c => c.OutputHtmlFragment = false), Is.EqualTo ("<html><body></body></html>"));
		}

		[Test]
		public void TestCharacterFormatting ()
		{
			const string rtf = "{\\rtf1\\ansi{\\colortbl;\\red255\\green0\\blue0;\\red0\\green0\\blue255;}\\pard a {\\b b}{\\i i}{\\ul u}{\\strike s}{\\super sup}{\\sub sub}{\\cf1\\cb2 c}{\\fs36 big}{\\fs25 odd}\\par}";
			var expected = "<div>a <span style=\"font-weight: bold;\">b</span><span style=\"font-style: italic;\">i</span>" +
				"<span style=\"text-decoration: underline;\">u</span><span style=\"text-decoration: line-through;\">s</span>" +
				"<span style=\"vertical-align: super;\">sup</span><span style=\"vertical-align: sub;\">sub</span>" +
				"<span style=\"color: #FF0000; background-color: #0000FF;\">c</span><span style=\"font-size: 18pt;\">big</span>" +
				"<span style=\"font-size: 12.5pt;\">odd</span></div>" + NewLine;

			Assert.That (Convert (rtf), Is.EqualTo (expected));
		}

		[Test]
		public void TestManyDistinctCharacterFormats ()
		{
			// More distinct formats than the CSS cache holds, followed by formats that were evicted and
			// re-used, must still each produce the correct style.
			var rtf = new StringBuilder ("{\\rtf1\\ansi\\pard ");
			var expected = new StringBuilder ("<div>");

			for (int round = 0; round < 2; round++) {
				for (int size = 26; size < 26 + 600; size += 2) {
					rtf.Append ("{\\fs").Append (size).Append (" x}");
					expected.Append ("<span style=\"font-size: ").Append (size / 2).Append ("pt;\">x</span>");
				}
			}

			rtf.Append ("\\par}");
			expected.Append ("</div>").Append (NewLine);

			Assert.That (Convert (rtf.ToString ()), Is.EqualTo (expected.ToString ()));
		}

		[Test]
		public void TestCombinedFormatting ()
		{
			Assert.That (Convert ("{\\rtf1{\\b\\i\\ul\\strike x}}"), Is.EqualTo ("<div><span style=\"font-weight: bold; font-style: italic; text-decoration: underline line-through;\">x</span></div>" + NewLine));
		}

		[Test]
		public void TestAutoColor ()
		{
			// \cf0 refers to the auto color even if the first color table entry is defined
			const string rtf = "{\\rtf1{\\colortbl\\red0\\green0\\blue0;\\red255\\green0\\blue0;}\\cf0\\cb0 a\\cf1 b\\cf99 c}";

			Assert.That (Convert (rtf), Is.EqualTo ("<div>a<span style=\"color: #FF0000;\">b</span>c</div>" + NewLine));
		}

		[Test]
		public void TestZeroFontSize ()
		{
			Assert.That (Convert ("{\\rtf1\\fs0 a}"), Is.EqualTo ("<div>a</div>" + NewLine));
		}

		[Test]
		public void TestParagraphAlignment ()
		{
			const string rtf = "{\\rtf1\\pard left\\par\\pard\\qc center\\par\\pard\\qr right\\par\\qj just\\par\\pard\\ql left\\par}";
			var expected = "<div>left</div>" + NewLine +
				"<div style=\"text-align: center;\">center</div>" + NewLine +
				"<div style=\"text-align: right;\">right</div>" + NewLine +
				"<div style=\"text-align: justify;\">just</div>" + NewLine +
				"<div>left</div>" + NewLine;

			Assert.That (Convert (rtf), Is.EqualTo (expected));
		}

		[Test]
		public void TestWhitespaceAndEscaping ()
		{
			const string rtf = "{\\rtf1 a  b   c\\tab d<&>\"'\\par\\par x\\line y}";
			var expected = "<div>a &#160;b &#160; c&#160;&#160;&#160;&#160;d&lt;&amp;&gt;&quot;&#39;</div>" + NewLine +
				"<div><br/></div>" + NewLine +
				"<div>x<br/>y</div>" + NewLine;

			Assert.That (Convert (rtf), Is.EqualTo (expected));
		}

		[Test]
		public void TestUnicodeAndCodePages ()
		{
			const string rtf = "{\\rtf1\\ansi\\ansicpg1252{\\fonttbl{\\f0 Arial;}{\\f1\\fcharset204 Arial Cyr;}}caf\\'e9 {\\f1 \\'cf\\'f0\\'e8} \\u8364?}";

			Assert.That (Convert (rtf), Is.EqualTo ("<div>caf&#233; &#1055;&#1088;&#1080; &#8364;</div>" + NewLine));
		}

		[Test]
		public void TestHyperlinks ()
		{
			const string rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"http://example.com/?a=1&b=2\"}{\\fldrslt link}} " +
				"{\\field{\\*\\fldinst HYPERLINK \"mailto:user@example.com\"}{\\fldrslt {\\b mail}}}}";
			var expected = "<div><a href=\"http://example.com/?a=1&amp;b=2\">link</a> <a href=\"mailto:user@example.com\"><span style=\"font-weight: bold;\">mail</span></a></div>" + NewLine;

			Assert.That (Convert (rtf), Is.EqualTo (expected));
		}

		[TestCase ("javascript:alert(1)")]
		[TestCase ("JavaScript:alert(1)")]
		[TestCase ("vbscript:msgbox(1)")]
		[TestCase ("data:text/html,<script>alert(1)</script>")]
		[TestCase ("file:///etc/passwd")]
		[TestCase (" javascript:alert(1)")]
		public void TestUnsafeHyperlinks (string url)
		{
			var rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"" + url + "\"}{\\fldrslt bad}}}";
			var html = Convert (rtf);

			Assert.That (html, Is.EqualTo ("<div>bad</div>" + NewLine));
		}

		[Test]
		public void TestTable ()
		{
			const string rtf = "{\\rtf1\\trowd\\cellx1000\\cellx2000\\pard\\intbl a\\cell b\\cell\\row\\pard after\\par}";
			var expected = "<table><tr><td><div>a</div>" + NewLine + "</td><td><div>b</div>" + NewLine + "</td></tr>" + NewLine +
				"</table>" + NewLine + "<div>after</div>" + NewLine;

			Assert.That (Convert (rtf), Is.EqualTo (expected));
		}

		[Test]
		public void TestHeaderAndFooter ()
		{
			var html = Convert ("{\\rtf1 Hello}", c => {
				c.Header = "<p>header</p>";
				c.HeaderFormat = HeaderFooterFormat.Html;
				c.Footer = "<footer>";
				c.FooterFormat = HeaderFooterFormat.Text;
			});

			Assert.That (html, Is.EqualTo ("<p>header</p><div>Hello</div>" + NewLine + "&lt;footer&gt;<br/>"));

			html = Convert ("{\\rtf1 Hello}", c => {
				c.OutputHtmlFragment = false;
				c.Header = "<p>header</p>";
				c.HeaderFormat = HeaderFooterFormat.Html;
				c.Footer = "<p>footer</p>";
				c.FooterFormat = HeaderFooterFormat.Html;
			});

			Assert.That (html, Is.EqualTo ("<html><body><p>header</p><div>Hello</div>" + NewLine + "<p>footer</p></body></html>"));
		}

		[Test]
		public void TestHtmlTagCallback ()
		{
			var html = Convert ("{\\rtf1{\\b bold} plain}", c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Span) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
					} else {
						ctx.WriteTag (writer, true);
					}
				};
			});

			Assert.That (html, Is.EqualTo ("<div>bold plain</div>" + NewLine));
		}

		const string EncapsulatedHtml = "{\\rtf1\\ansi\\ansicpg1252\\fromhtml1 \\deff0{\\fonttbl{\\f0\\fswiss Arial;}}\r\n" +
			"{\\*\\htmltag19 <html>}{\\*\\htmltag34 <head><title>t</title><style>p\\{color:red\\}</style>}{\\*\\htmltag41 </head>}{\\*\\htmltag50 <body>}\\htmlrtf {\\htmlrtf0 \r\n" +
			"{\\*\\htmltag64 <p class=\"caf\\'e9\">}\\htmlrtf {\\htmlrtf0 caf\\'e9 &amp; <b>\\htmlrtf\\par\\htmlrtf0 \r\n" +
			"{\\*\\htmltag72 </p>}\\htmlrtf }\\htmlrtf0 \r\n" +
			"\\htmlrtf }\\htmlrtf0 {\\*\\htmltag58 </body>}{\\*\\htmltag27 </html>}}";

		[Test]
		public void TestExtractEncapsulatedHtml ()
		{
			var html = Convert (EncapsulatedHtml, c => c.OutputHtmlFragment = false);

			Assert.That (html, Is.EqualTo ("<html><head><title>t</title><style>p{color:red}</style></head><body><p class=\"caf&#233;\">café &amp; <b></p></body></html>"));
		}

		[Test]
		public void TestExtractEncapsulatedHtmlFragment ()
		{
			Assert.That (Convert (EncapsulatedHtml), Is.EqualTo ("<p class=\"caf&#233;\">café &amp; <b></p>"));
		}

		[Test]
		public void TestExtractEncapsulatedHtmlCallback ()
		{
			var html = Convert (EncapsulatedHtml, c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.P) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
					} else {
						ctx.WriteTag (writer, true);
					}
				};
			});

			Assert.That (html, Is.EqualTo ("café &amp; <b>"));
		}

		[Test]
		public void TestDisableEncapsulatedHtmlExtraction ()
		{
			// The RTF rendering of the encapsulated HTML is used instead: "&amp; <b>" is literal text in the RTF.
			var html = Convert (EncapsulatedHtml, c => c.ExtractEncapsulatedHtml = false);

			Assert.That (html, Is.EqualTo ("<div>caf&#233; &amp;amp; &lt;b&gt;</div>" + NewLine));
		}

		[Test]
		public void TestFromHtmlOutsideRecognitionWindow ()
		{
			// [MS-OXRTFEX] 2.2.3.1: \fromhtml1 must appear within the first 10 tokens of the document.
			var html = Convert ("{\\rtf1 text\\fromhtml1 {\\*\\htmltag <script>}x}");

			Assert.That (html, Is.EqualTo ("<div>textx</div>" + NewLine));
		}

		[Test]
		public void TestTnefMultiValueAttribute ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", "multi-value-attribute", "message.rtf");
			var converter = new RtfToHtml ();
			string html;

			using (var stream = File.OpenRead (path))
				html = ConvertStream (converter, stream);

			Assert.That (html, Does.StartWith ("<html><head>"));
			Assert.That (html, Does.Contain ("<a style=\"color: #3399ff; \" href=\"tel:208225\">"));
			Assert.That (html, Does.Contain ("Curie Conf Room"));
			Assert.That (html, Does.Not.Contain ("\\htmltag"));
			Assert.That (html, Does.Not.Contain ("\\par"));

			converter = new RtfToHtml { ExtractEncapsulatedHtml = false, OutputHtmlFragment = true };

			using (var stream = File.OpenRead (path))
				html = ConvertStream (converter, stream);

			Assert.That (html, Does.StartWith ("<div>"));
			Assert.That (html, Does.Contain ("<a href=\"tel:208225\"><span style=\"text-decoration: underline; color: #0000FF;\">208225</span></a>"));
		}

		static string ConvertStream (TextConverter converter, Stream stream)
		{
			using var output = new MemoryStream ();

			converter.Convert (stream, output);

			output.Position = 0;

			using var reader = new StreamReader (output, Encoding.UTF8);

			return reader.ReadToEnd ();
		}

		[TestCase ("multi-value-attribute")]
		[TestCase ("winmail")]
		[TestCase ("rtf")]
		[TestCase ("MAPI_OBJECT")]
		[TestCase ("long-filename")]
		public void TestTnefSamples (string name)
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", name, "message.rtf");

			foreach (var extract in new [] { true, false }) {
				var converter = new RtfToHtml { ExtractEncapsulatedHtml = extract };
				string html;

				using (var stream = File.OpenRead (path))
					html = ConvertStream (converter, stream);

				Assert.That (html, Does.StartWith ("<html>"), $"extract={extract}");
				Assert.That (html, Does.EndWith ("</html>").Or.EndWith ("</html>" + NewLine).Or.EndWith ("</html>\r\n").Or.EndWith ("</html>\n"), $"extract={extract}");
				Assert.That (html, Does.Not.Contain ("\\rtf"), $"extract={extract}");
				Assert.That (html, Does.Not.Contain ("\\fonttbl"), $"extract={extract}");
			}

			using (var stream = File.OpenRead (path)) {
				var text = ConvertStream (new RtfToText (), stream);

				Assert.That (text, Does.Not.Contain ("\\rtf"));
				Assert.That (text, Does.Not.Contain ("\\fonttbl"));
			}
		}

		[Test]
		public void TestDeepNesting ()
		{
			const int depth = 1000000;
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1 a");
			builder.Append ('{', depth);
			builder.Append ("{\\b deep}");
			builder.Append ('}', depth);
			builder.Append ("b}");

			Assert.That (Convert (builder.ToString (), c => c.MaxGroupDepth = 8), Is.EqualTo ("<div>a<span style=\"font-weight: bold;\">deep</span>b</div>" + NewLine));
		}

		[Test]
		public void TestMaxGroupDepth ()
		{
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1 a");
			for (int i = 0; i < 100; i++)
				builder.Append ((i & 1) == 0 ? "{\\b " : "{\\b0 ");
			builder.Append ("deep");
			builder.Append ('}', 100);
			builder.Append ("b}");

			Assert.That (Convert (builder.ToString (), c => c.MaxGroupDepth = 8), Is.EqualTo ("<div>ab</div>" + NewLine));
		}

		[Test]
		public void TestLargeColorTable ()
		{
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1{\\colortbl;");
			for (int i = 0; i < 100000; i++)
				builder.Append ("\\red255\\green0\\blue0;");
			builder.Append ("}\\cf5 a\\cf50000 b}");

			Assert.That (Convert (builder.ToString (), c => c.MaxColorTableEntries = 10), Is.EqualTo ("<div><span style=\"color: #FF0000;\">a</span>b</div>" + NewLine));
		}

		[Test]
		public void TestLargeFontTable ()
		{
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1{\\fonttbl");
			for (int i = 0; i < 100000; i++)
				builder.Append ("{\\f").Append (i).Append ("\\fcharset204 Font;}");
			builder.Append ("}\\f5 \\'cf\\f50000 \\'cf}");

			Assert.That (Convert (builder.ToString (), c => c.MaxFontTableEntries = 10), Is.EqualTo ("<div>&#1055;&#207;</div>" + NewLine));
		}

		static string Encapsulate (string html)
		{
			return "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag " + html.Replace ("\\", "\\\\").Replace ("{", "\\{").Replace ("}", "\\}") + "}}";
		}

		[Test]
		public void TestExtractLineBreaksOutsideHtmlTag ()
		{
			// [MS-OXRTFEX] 2.2.3.2: outside of HTMLTAG destinations, \par and \line become CRLF while \cell and \row
			// are dropped.
			const string rtf = "{\\rtf1\\fromhtml1 {\\*\\htmltag <p>}a\\par b\\line c\\cell d\\row{\\*\\htmltag </p>}}";

			Assert.That (Convert (rtf), Is.EqualTo ("<p>a\r\nb\r\ncd</p>"));
		}

		[Test]
		public void TestExtractEndTagCallback ()
		{
			var html = Convert (Encapsulate ("<b>x</b><i>y</i>"), c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.B) {
						if (ctx.IsEndTag) {
							writer.WriteEndTag ("strong");
						} else {
							writer.WriteStartTag ("strong");
							ctx.InvokeCallbackForEndTag = true;
						}
					} else {
						ctx.WriteTag (writer, true);
					}
				};
			});

			Assert.That (html, Is.EqualTo ("<strong>x</strong><i>y</i>"));
		}

		[Test]
		public void TestExtractUnmatchedEndTags ()
		{
			var tags = new List<string> ();
			var html = Convert (Encapsulate ("</i><p>x</b></p></body></html>"), c => {
				c.HtmlTagCallback = (ctx, writer) => {
					tags.Add ((ctx.IsEndTag ? "/" : string.Empty) + ctx.TagName);
					ctx.WriteTag (writer, true);
				};
			});

			// Unmatched end tags are passed to the callback, except for document structure tags in a fragment.
			Assert.That (html, Is.EqualTo ("</i><p>x</b></p>"));
			Assert.That (tags, Is.EqualTo (new [] { "/i", "p", "/b" }));
		}

		[Test]
		public void TestExtractDocumentStructureFragment ()
		{
			var html = Convert (Encapsulate ("<!DOCTYPE html><html><head><title>t</title></head><body/><body>x<br/></body></html>"));

			Assert.That (html, Is.EqualTo ("x<br/>"));
		}

		[Test]
		public void TestExtractSanitizingCallback ()
		{
			// The HtmlTagCallback is the hook for sanitizing extracted HTML, which is attacker-controlled. Content
			// suppressed by the callback must stay suppressed even when its end tag is missing or mismatched.
			var html = Convert (Encapsulate ("<p onclick=\"evil()\">a<script>alert('</p>')</script>b</p><script>c"), c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Script) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						ctx.SuppressInnerContent = true;
						return;
					}

					if (ctx.IsEndTag) {
						ctx.WriteTag (writer, false);
						return;
					}

					ctx.WriteTag (writer, false);
					foreach (var attribute in ctx.Attributes) {
						if (!attribute.Name.StartsWith ("on", StringComparison.OrdinalIgnoreCase))
							writer.WriteAttribute (attribute);
					}
				};
			});

			Assert.That (html, Is.EqualTo ("<p>ab</p>"));
		}

		[TestCase ("<style/>body{background:url(http://example.com/x)}</style><p>x</p>")]
		[TestCase ("<script/>alert(1)</script><p>x</p>")]
		public void TestExtractSelfClosingRawTextSuppressed (string encapsulated)
		{
			// A self-closing <style/> or <script/> still switches the tokenizer into a raw content state, so content
			// suppressed by the callback must include the raw text that follows.
			var html = Convert (Encapsulate (encapsulated), c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Style || ctx.TagId == HtmlTagId.Script) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						ctx.SuppressInnerContent = true;
						return;
					}

					ctx.WriteTag (writer, true);
				};
			});

			Assert.That (html, Is.EqualTo ("<p>x</p>"));
		}

		[Test]
		public void TestExtractMaxElementDepth ()
		{
			// Once the depth limit is exceeded, the rest of the extracted HTML is written as encoded text so that no
			// tags can be hidden from the HtmlTagCallback.
			var html = Convert (Encapsulate ("<div><span><b><i>x<img src=x onerror=alert(1)></i></b>"), c => c.MaxElementDepth = 3);

			Assert.That (html, Is.EqualTo ("<div><span><b><i>x&lt;img src=x onerror=alert(1)&gt;&lt;/i&gt;&lt;/b&gt;"));
		}

		[Test]
		public void TestExtractTruncatedTag ()
		{
			// A tag that is truncated by the end of the input is dropped rather than written as text.
			Assert.That (Convert (Encapsulate ("<p>x</p><img src=\"http://example.com/x\" onerror=")), Is.EqualTo ("<p>x</p>"));
		}

		[Test]
		public void TestExtractBogusDocType ()
		{
			// A DOCTYPE with a truncated PUBLIC/SYSTEM keyword must not corrupt the name of the next tag.
			var html = Convert (Encapsulate ("<!DOCTYPE html </DIV><DIV class=x>y</DIV>"), c => c.OutputHtmlFragment = false);

			Assert.That (html, Is.EqualTo ("<!DOCTYPE html><DIV class=\"x\">y</DIV>"));
		}

		[Test]
		public void TestExtractManyUnclosedTags ()
		{
			// Hostile encapsulated HTML with a huge number of unclosed elements and unmatched end tags must be
			// processed in linear time.
			const int count = 100000;
			var builder = new StringBuilder ();

			for (int i = 0; i < count; i++)
				builder.Append ("<div>");
			for (int i = 0; i < count; i++)
				builder.Append ("</span>");

			var html = Convert (Encapsulate (builder.ToString ()), c => c.HtmlTagCallback = (ctx, writer) => {
				if (!ctx.IsEndTag)
					ctx.WriteTag (writer, true);
			});

			Assert.That (html.Length, Is.EqualTo (count * "<div>".Length));
		}

		[Test]
		public void TestExtractLargeDocument ()
		{
			// The extracted HTML is streamed into the HTML tokenizer rather than buffered.
			var builder = new StringBuilder ("{\\rtf1\\fromhtml1 ");
			for (int i = 0; i < 10000; i++)
				builder.Append ("{\\*\\htmltag <p>}text ").Append (i).Append ("{\\*\\htmltag </p>}\\htmlrtf \\par\\htmlrtf0 ");
			builder.Append ('}');

			var html = Convert (builder.ToString ());

			Assert.That (html, Does.StartWith ("<p>text 0</p><p>text 1</p>"));
			Assert.That (html, Does.EndWith ("<p>text 9999</p>"));
		}

		[Test]
		public void TestTextHeaderAndFooter ()
		{
			var html = Convert ("{\\rtf1 Hello}", c => {
				c.OutputHtmlFragment = false;
				c.Header = "<header>";
				c.HeaderFormat = HeaderFooterFormat.Text;
				c.Footer = "<footer>";
				c.FooterFormat = HeaderFooterFormat.Text;
			});

			Assert.That (html, Is.EqualTo ("<html><body>&lt;header&gt;<br/><div>Hello</div>" + NewLine + "&lt;footer&gt;<br/></body></html>"));
		}

		[Test]
		public void TestExtractHeaderAndFooter ()
		{
			var html = Convert (Encapsulate ("<p>x</p>"), c => {
				c.Header = "<header>";
				c.HeaderFormat = HeaderFooterFormat.Text;
				c.Footer = "<p>footer</p>";
				c.FooterFormat = HeaderFooterFormat.Html;
			});

			Assert.That (html, Is.EqualTo ("&lt;header&gt;<br/><p>x</p><p>footer</p>"));
		}

		[Test]
		public void TestRowWithoutTable ()
		{
			// Unbalanced table structure must still produce well-formed output.
			Assert.That (Convert ("{\\rtf1 a\\row b}"), Is.EqualTo ("<div>a<table><tr></tr>" + NewLine + "</table>" + NewLine + "b</div>" + NewLine));
			Assert.That (Convert ("{\\rtf1\\row\\cell\\row}"), Is.EqualTo ("<table><tr></tr>" + NewLine + "<tr><td></td></tr>" + NewLine + "</table>" + NewLine));
		}

		[Test]
		public void TestHiddenTextIsNotRendered ()
		{
			Assert.That (Convert ("{\\rtf1 a{\\v b\\par c\\line d\\cell e\\row}f}"), Is.EqualTo ("<div>af</div>" + NewLine));
		}

		[Test]
		public void TestHyperlinkWithSwitchBeforeUrl ()
		{
			const string rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \\n \"https://example.com/\"}{\\fldrslt x}}}";

			Assert.That (Convert (rtf), Is.EqualTo ("<div><a href=\"https://example.com/\">x</a></div>" + NewLine));
		}

		[Test]
		public void TestFontPanoseBeforeCharset ()
		{
			// The {\*\panose} group ending must not commit the font entry before \fcharset204 is read.
			const string rtf = "{\\rtf1{\\fonttbl{\\f0\\fswiss{\\*\\panose 020b0604020202020204}\\fcharset204 Arial;}}\\f0 \\'cf}";

			Assert.That (Convert (rtf), Is.EqualTo ("<div>&#1055;</div>" + NewLine));
		}

		[Test]
		public void TestRenderCallbackSuppressAndEndTags ()
		{
			const string rtf = "{\\rtf1 a{\\b secret}b\\par{\\i x}\\par c}";
			var html = Convert (rtf, c => {
				c.HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Span && ctx.Attributes.Count > 0 && ctx.Attributes[0].Value.Contains ("bold")) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						ctx.SuppressInnerContent = true;
					} else if (ctx.TagId == HtmlTagId.Div) {
						if (ctx.IsEndTag) {
							writer.WriteEndTag ("p");
						} else {
							writer.WriteStartTag ("p");
							ctx.InvokeCallbackForEndTag = true;
						}
					} else {
						ctx.WriteTag (writer, true);
					}
				};
			});

			Assert.That (html, Is.EqualTo ("<p>ab</p>" + NewLine + "<p><span style=\"font-style: italic;\">x</span></p>" + NewLine + "<p>c</p>" + NewLine));
		}

		static void RemoveImages (HtmlTagContext ctx, HtmlWriter writer)
		{
			if (ctx.TagId == HtmlTagId.Image) {
				ctx.DeleteTag = true;
				ctx.DeleteEndTag = true;
				return;
			}

			ctx.WriteTag (writer, true);
		}

		[TestCase ("<p>text</p><noscript><img src=\"http://example.com/tracker.png\"></noscript>", "<p>text</p>")]
		[TestCase ("<noscript><style></noscript><img src=x onerror=alert(1)></style></noscript>", "<style></noscript><img src=x onerror=alert(1)></style>")]
		[TestCase ("<noscript><!--</noscript><img src=x onerror=alert(1)>--></noscript>", "<!--</noscript><img src=x onerror=alert(1)>-->")]
		public void TestExtractNoScriptUnwrap (string html, string expected)
		{
			// By default, <noscript> content is tokenized as markup so the callback sees every tag, and the <noscript>
			// tags are removed so that the output means the same thing whether or not scripting is enabled.
			var result = Convert (Encapsulate (html), c => c.HtmlTagCallback = RemoveImages);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestExtractNoScriptScriptingEnabled ()
		{
			// With scripting enabled, <noscript> content is raw text and is not passed to the callback.
			const string html = "<p>text</p><noscript><img src=\"http://example.com/tracker.png\"></noscript>";
			var result = Convert (Encapsulate (html), c => {
				c.HtmlTagCallback = RemoveImages;
				c.NoScriptHandling = HtmlNoScriptHandling.ScriptingEnabled;
			});

			Assert.That (result, Is.EqualTo (html));
		}

		[Test]
		public void TestExtractNoScriptScriptingDisabled ()
		{
			// With scripting disabled, <noscript> content is tokenized as markup and the <noscript> tags are kept.
			const string html = "<p>text</p><noscript><img src=\"http://example.com/tracker.png\"></noscript>";
			var result = Convert (Encapsulate (html), c => {
				c.HtmlTagCallback = RemoveImages;
				c.NoScriptHandling = HtmlNoScriptHandling.ScriptingDisabled;
			});

			Assert.That (result, Is.EqualTo ("<p>text</p><noscript></noscript>"));
		}

		[Test]
		public void TestNoScriptHandlingOutOfRange ()
		{
			var converter = new RtfToHtml ();

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.NoScriptHandling = (HtmlNoScriptHandling) (-1));
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.NoScriptHandling = (HtmlNoScriptHandling) 3);
		}

		[Test]
		public void TestNestedHyperlinks ()
		{
			// A hyperlink field nested in another's result must not produce nested <a> elements.
			const string rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"http://a/\"}{\\fldrslt a{\\field{\\*\\fldinst HYPERLINK \"http://b/\"}{\\fldrslt b}}c}}}";

			Assert.That (Convert (rtf), Is.EqualTo ("<div><a href=\"http://a/\">a</a><a href=\"http://b/\">b</a><a href=\"http://a/\">c</a></div>" + NewLine));
		}

		static void WriteImage (int index, HtmlWriter writer)
		{
			writer.WriteEmptyElementTag ("img");
			writer.WriteAttribute ("src", "cid:" + index);
		}

		static int CountObjectPlaceholders (string rtf)
		{
			using var reader = new StringReader (rtf);

			return new RtfToHtml ().CountObjectPlaceholders (reader, CancellationToken.None);
		}

		[Test]
		public void TestObjectPlaceholders ()
		{
			// [MS-OXRTFEX] 2.2.3.4: each \objattph marks the position of the next attachment. The placeholder
			// character that follows it (written as \'20 or as a literal space) is replaced.
			const string rtf = "{\\rtf1 A\\objattph\\'20 B{\\b\\objattph  C}}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteImage),
				Is.EqualTo ("<div>A<img src=\"cid:0\"/> B<span style=\"font-weight: bold;\"><img src=\"cid:1\"/>C</span></div>" + NewLine));
			Assert.That (Convert (rtf), Is.EqualTo ("<div>A B<span style=\"font-weight: bold;\">C</span></div>" + NewLine));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (2));
		}

		[Test]
		public void TestObjectPlaceholderStartsParagraph ()
		{
			Assert.That (Convert ("{\\rtf1\\objattph\\'20}", c => c.ObjectPlaceholderCallback = WriteImage),
				Is.EqualTo ("<div><img src=\"cid:0\"/></div>" + NewLine));
		}

		[Test]
		public void TestObjectPlaceholderOutputIsNotEncoded ()
		{
			// The callback writes HTML, not text.
			var html = Convert ("{\\rtf1 A\\objattph\\'20}", c => c.ObjectPlaceholderCallback = (index, writer) => writer.WriteText ("<&>"));

			Assert.That (html, Is.EqualTo ("<div>A&lt;&amp;&gt;</div>" + NewLine));
		}

		[Test]
		public void TestObjectPlaceholdersThatAreNotRendered ()
		{
			// Placeholders in skipped destinations or in hidden text are not part of the rendered document, so
			// they are neither reported nor counted.
			const string rtf = "{\\rtf1 A{\\*\\unknown \\objattph}{\\v \\objattph}{\\pict \\objattph}{\\fonttbl \\objattph}" +
				"{\\*\\htmltag \\objattph}B\\objattph\\'20 C}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteImage), Is.EqualTo ("<div>AB<img src=\"cid:0\"/> C</div>" + NewLine));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (1));
		}

		[Test]
		public void TestObjectPlaceholdersInEncapsulatedHtml ()
		{
			// HTML that is extracted from the RTF refers to its attachments itself, so the placeholders in the
			// (suppressed) RTF rendering are ignored and cannot be counted.
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag <p>}A\\objattph\\'20 B{\\*\\htmltag </p>}}";
			int calls = 0;

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = (index, writer) => calls++), Is.EqualTo ("<p>A B</p>"));
			Assert.That (calls, Is.EqualTo (0));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (-1));
		}

		[Test]
		public void TestManyObjectPlaceholders ()
		{
			var builder = new StringBuilder ("{\\rtf1 ");
			int last = -1, count = 0;

			for (int i = 0; i < 100000; i++)
				builder.Append ("\\objattph\\'20");
			builder.Append ('}');

			Convert (builder.ToString (), c => c.ObjectPlaceholderCallback = (index, writer) => {
				Assert.That (index, Is.EqualTo (last + 1));
				last = index;
				count++;
			});

			Assert.That (count, Is.EqualTo (100000));
			Assert.That (CountObjectPlaceholders (builder.ToString ()), Is.EqualTo (100000));
		}
	}
}
