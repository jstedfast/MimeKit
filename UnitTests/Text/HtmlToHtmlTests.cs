//
// HtmlToHtmlTests.cs
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
	public class HtmlToHtmlTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			using var reader = new StringReader ("");
			using var writer = new StringWriter ();
			var converter = new HtmlToHtml ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxElementDepth = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.NoScriptHandling = (HtmlNoScriptHandling) (-1));
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.NoScriptHandling = (HtmlNoScriptHandling) 3);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new HtmlToHtml ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.FilterComments, Is.False, "FilterComments");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.FooterFormat, Is.EqualTo (HeaderFooterFormat.Text), "FooterFormat");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.HeaderFormat, Is.EqualTo (HeaderFooterFormat.Text), "HeaderFormat");
			Assert.That (converter.HtmlTagCallback, Is.Null, "HtmlTagCallback");
			Assert.That (converter.InputEncoding, Is.EqualTo (Encoding.UTF8), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Html), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.MaxElementDepth, Is.EqualTo (4096), "MaxElementDepth");
			Assert.That (converter.NoScriptHandling, Is.EqualTo (HtmlNoScriptHandling.Unwrap), "NoScriptHandling");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Html), "OutputFormat");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
		}

		void ReplaceUrlsWithFileNames (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Image) {
				htmlWriter.WriteEmptyElementTag (ctx.TagName);
				ctx.DeleteEndTag = true;

				for (int i = 0; i < ctx.Attributes.Count; i++) {
					var attr = ctx.Attributes[i];

					if (attr.Id == HtmlAttributeId.Src) {
						var fileName = Path.GetFileName (attr.Value);
						htmlWriter.WriteAttributeName (attr.Name);
						htmlWriter.WriteAttributeValue (fileName);
					} else {
						htmlWriter.WriteAttribute (attr);
					}
				}
			} else {
				ctx.WriteTag (htmlWriter, true);
			}
		}

		[Test]
		public void TestSimpleHtmlToHtml ()
		{
			string expected = File.ReadAllText (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "xamarin3.xhtml"));
			string text = File.ReadAllText (Path.Combine (TestHelper.ProjectDir, "TestData", "html", "xamarin3.html"));
			var converter = new HtmlToHtml { Header = null, Footer = null, HtmlTagCallback = ReplaceUrlsWithFileNames };
			var result = converter.Convert (text);

			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Html), "InputFormat");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Html), "OutputFormat");
			Assert.That (result, Is.EqualTo (expected));
		}

		[TestCase ("foo<img src=x onerror=alert(1)", "foo")]
		[TestCase ("foo <a href=\"javascript:alert(1)", "foo ")]
		[TestCase ("foo<div class=x ", "foo")]
		[TestCase ("foo</div", "foo")]
		[TestCase ("foo</div x", "foo")]
		[TestCase ("foo<", "foo&lt;")]
		[TestCase ("foo</", "foo&lt;/")]
		public void TestTruncatedTagsAreDropped (string html, string expected)
		{
			// A tag that is cut off by the end of the input never reaches the HtmlTagCallback, so it must not be
			// written as-is or it could be completed by whatever markup the output is later combined with.
			var converter = new HtmlToHtml { Header = null, Footer = null };
			var result = converter.Convert (html);

			Assert.That (result, Is.EqualTo (expected));
		}

		[TestCase ("<!DOCTYPE html><html><head><title>t</title><style>p{}</style></head><body><p>x<br/></p></body></html>", "<p>x<br/></p>")]
		[TestCase ("<html><body/><body>x</body></html>", "x")]
		[TestCase ("<head><meta charset=utf-8></head>x</body></html><p>y</p>", "x<p>y</p>")]
		[TestCase ("<p>no structure</p>", "<p>no structure</p>")]
		public void TestOutputHtmlFragment (string html, string expected)
		{
			var tags = new List<string> ();
			var converter = new HtmlToHtml {
				Header = null, Footer = null, OutputHtmlFragment = true,
				HtmlTagCallback = (ctx, writer) => {
					tags.Add (ctx.TagName);
					ctx.WriteTag (writer, true);
				}
			};
			var result = converter.Convert (html);

			// The document structure tags are dropped without being passed to the HtmlTagCallback.
			Assert.That (result, Is.EqualTo (expected));
			Assert.That (tags, Has.None.AnyOf ("html", "head", "body", "title", "meta", "style"));
		}

		[Test]
		public void TestOutputHtmlFragmentFalse ()
		{
			const string html = "<!DOCTYPE html><html><head><title>t</title></head><body>x</body></html>";
			var converter = new HtmlToHtml { Header = null, Footer = null };

			Assert.That (converter.OutputHtmlFragment, Is.False);
			Assert.That (converter.Convert (html), Is.EqualTo (html));
		}

		[TestCase ("<div><span><b><i>x<img src=x onerror=alert(1)>&amp;</i></b>",  "<div><span><b><i>x&lt;img src=x onerror=alert(1)&gt;&amp;amp;&lt;/i&gt;&lt;/b&gt;")]
		[TestCase ("<div><span><svg><style><img src=x onerror=alert(1)></style>", "<div><span><svg><style>&lt;img src=x onerror=alert(1)&gt;&lt;/style&gt;")]
		[TestCase ("<div><div><div><div><span><b>x</b><img src=x>", "<div><div><div><div><span><b>x</b><img src=\"x\"/>")]
		public void TestMaxElementDepthExceeded (string html, string expected)
		{
			// Once the depth limit is exceeded, the remainder of the input must be written as encoded text so that
			// no tags can be hidden from the HtmlTagCallback.
			var converter = new HtmlToHtml { Header = null, Footer = null, MaxElementDepth = 3 };
			var result = converter.Convert (html);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestMaxElementDepthNestedTables ()
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < 100000; i++)
				builder.Append ("<table><tr><td>");
			builder.Append ("<img src=x onerror=alert(1)>");

			var converter = new HtmlToHtml { Header = null, Footer = null };
			var result = converter.Convert (builder.ToString ());

			Assert.That (result, Does.EndWith ("&lt;img src=x onerror=alert(1)&gt;"));
			Assert.That (result, Does.Not.Contain ("<img"));
		}

		void SupressInnerContentCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			ctx.InvokeCallbackForEndTag = true;

			//discard html content from unnecessary tags
			if (ctx.TagId == HtmlTagId.Head || ctx.TagId == HtmlTagId.Script || ctx.TagId == HtmlTagId.Style) {
				ctx.SuppressInnerContent = true;
			} else {
				if (ctx.TagId == HtmlTagId.Image && !ctx.IsEndTag) {
					foreach (var attribute in ctx.Attributes) {
						if (attribute.Id == HtmlAttributeId.Src)
							htmlWriter.WriteText (attribute.Value + " ");
					}
				} else if (ctx.TagId == HtmlTagId.A) {
					foreach (var attribute in ctx.Attributes) {
						if (attribute.Id == HtmlAttributeId.Href)
							htmlWriter.WriteText (" [ " + attribute.Value + " ] ");
					}
				} else {
					//add new line for p, div or br tags
					if (ctx.TagId == HtmlTagId.P || ctx.TagId == HtmlTagId.Div || ctx.TagId == HtmlTagId.Br) {
						htmlWriter.WriteText (Environment.NewLine);
					} else {
						foreach (var attribute in ctx.Attributes) {
							if (attribute.Id == HtmlAttributeId.Src)
								htmlWriter.WriteText (attribute.Value);
						}
					}
				}
			}
		}

		[Test]
		public void TestSupressInnerContent ()
		{
			const string input = "<html xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" xmlns:m=\"http://schemas.microsoft.com/office/2004/12/omml\"xmlns=\"http://www.w3.org/TR/REC-html40\"><head><meta http-equiv=Content-Type content=\"text/html; charset=iso-8859-2\"><meta name=Generator content=\"Microsoft Word 15 (filtered medium)\"><!--[if !mso]><style>v\\:* {behavior:url(#default#VML);}\r\no\\:* {behavior:url(#default#VML);}\r\nw\\:* {behavior:url(#default#VML);}\r\n.shape{behavior:url(#default#VML);}\r\n</style><![endif]--><style><!--\r\n/* Font Definitions */\r\n@font-face\r\n\t{font-family:\"Cambria Math\";\r\n\tpanose-1:2 4 5 3 5 4 6 3 2 4;}\r\n@font-face\r\n\t{font-family:Calibri;\r\n\tpanose-1:2 15 5 2 2 2 4 3 2 4;}\r\n@font-face\r\n\t{font-family:\"Segoe UI\";\r\n\tpanose-1:2 11 5 2 4 2 4 2 2 3;}\r\n@font-face\r\n\t{font-family:Verdana;\r\n\tpanose-1:2 11 6 4 3 5 4 4 2 4;}\r\n/* Style Definitions */\r\np.MsoNormal, li.MsoNormal, div.MsoNormal\r\n\t{margin:0cm;\r\n\tmargin-bottom:.0001pt;\r\n\tfont-size:11.0pt;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tmso-fareast-language:EN-US;}\r\nh3\r\n\t{mso-style-priority:9;\r\n\tmso-style-link:\"Heading 3 Char\";\r\n\tmso-margin-top-alt:auto;\r\n\tmargin-right:0cm;\r\n\tmso-margin-bottom-alt:auto;\r\n\tmargin-left:0cm;\r\n\tfont-size:13.5pt;\r\n\tfont-family:\"Times New Roman\",serif;}\r\na:link, span.MsoHyperlink\r\n\t{mso-style-priority:99;\r\n\tcolor:#0563C1;\r\n\ttext-decoration:underline;}\r\na:visited,span.MsoHyperlinkFollowed\r\n\t{mso-style-priority:99;\r\n\tcolor:#954F72;\r\n\ttext-decoration:underline;}\r\nspan.Heading3Char\r\n\t{mso-style-name:\"Heading 3 Char\";\r\n\tmso-style-priority:9;\r\n\tmso-style-link:\"Heading 3\";\r\n\tfont-family:\"Times New Roman\",serif;\r\n\tmso-fareast-language:FR;\r\n\tfont-weight:bold;}\r\nspan.EmailStyle18\r\n\t{mso-style-type:personal;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tcolor:windowtext;}\r\nspan.EmailStyle19\r\n\t{mso-style-type:personal-reply;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tcolor:#1F497D;}\r\n.MsoChpDefault\r\n\t{mso-style-type:export-only;\r\n\tfont-size:10.0pt;}\r\n@page WordSection1\r\n\t{size:612.0pt 792.0pt;\r\n\tmargin:70.85pt 70.85pt 70.85pt 70.85pt;}\r\ndiv.WordSection1\r\n\t{page:WordSection1;}\r\n--></style><!--[if gte mso 9]><xml>\r\n<o:shapedefaults v:ext=\"edit\" spidmax=\"1026\" />\r\n</xml><![endif]--><!--[if gte mso 9]><xml>\r\n<o:shapelayout v:ext=\"edit\">\r\n<o:idmap v:ext=\"edit\" data=\"1\" />\r\n</o:shapelayout></xml><![endif]--></head><body lang=FR link=\"#0563C1\" vlink=\"#954F72\">Here is the body content which seems fine so far</body></html>";
			const string expected = "Here is the body content which seems fine so far";
			var converter = new HtmlToHtml { HtmlTagCallback = SupressInnerContentCallback };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		static void RemoveImagesCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Image) {
				ctx.DeleteTag = true;
				ctx.DeleteEndTag = true;
			} else {
				ctx.WriteTag (htmlWriter, true);
			}
		}

		static void RemoveImagesAndNoScriptTagsCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Image || ctx.TagId == HtmlTagId.NoScript) {
				ctx.DeleteTag = true;
				ctx.DeleteEndTag = true;
			} else {
				ctx.WriteTag (htmlWriter, true);
			}
		}

		[Test]
		public void TestNoScriptUnwrap ()
		{
			const string input = "<p>text</p><noscript><img src=\"http://example.com/tracker.png\"><p>fallback</p></noscript>";
			const string expected = "<p>text</p><p>fallback</p>";
			var converter = new HtmlToHtml { HtmlTagCallback = RemoveImagesCallback };

			// Note: the content of the <noscript> element is passed to the callback and the <noscript> tags are removed
			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestNoScriptUnwrapNoCallback ()
		{
			const string input = "<head><noscript><link rel=\"stylesheet\" href=\"x.css\"></noscript></head><body><noscript/>a<NOSCRIPT>b</NoScript>c</body>";
			const string expected = "<head><link rel=\"stylesheet\" href=\"x.css\"/></head><body>abc</body>";
			var converter = new HtmlToHtml ();

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		static void RecordTagsCallback (HtmlTagContext ctx, HtmlWriter htmlWriter, List<string> tags)
		{
			tags.Add (ctx.IsEndTag ? "/" + ctx.TagName : ctx.TagName);
			ctx.WriteTag (htmlWriter, true);
		}

		[Test]
		public void TestNoScriptUnwrapCallbackNeverSeesNoScriptTags ()
		{
			const string input = "<div><noscript><b>x</b></noscript></div>";
			var tags = new List<string> ();
			var converter = new HtmlToHtml { HtmlTagCallback = (ctx, writer) => {
				ctx.InvokeCallbackForEndTag = true;
				RecordTagsCallback (ctx, writer, tags);
			} };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo ("<div><b>x</b></div>"));
			Assert.That (tags, Is.EqualTo (new[] { "div", "b", "/b", "/div" }));
		}

		[TestCase ("<noscript><style></noscript><img src=x onerror=alert(1)></style></noscript>", "<style></noscript><img src=x onerror=alert(1)></style>")]
		[TestCase ("<noscript><!--</noscript><img src=x onerror=alert(1)>--></noscript>", "<!--</noscript><img src=x onerror=alert(1)>-->")]
		[TestCase ("<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></p></noscript>", "<p title=\"&lt;/noscript&gt;&lt;img src=x onerror=alert(1)&gt;\"></p>")]
		public void TestNoScriptUnwrapXss (string input, string expected)
		{
			var converter = new HtmlToHtml { HtmlTagCallback = RemoveImagesCallback };

			// Note: without the <noscript> tags, the output is interpreted the same regardless of whether
			// scripting is enabled in the renderer, so the embedded "</noscript>" is harmless.
			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestNoScriptScriptingEnabled ()
		{
			const string input = "<p>text</p><noscript><img src=\"http://example.com/tracker.png\"></noscript>";
			var converter = new HtmlToHtml { HtmlTagCallback = RemoveImagesCallback, NoScriptHandling = HtmlNoScriptHandling.ScriptingEnabled };

			// Note: the content of the <noscript> element is raw text and is not passed to the callback
			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (input));
		}

		[Test]
		public void TestNoScriptScriptingDisabled ()
		{
			const string input = "<p>text</p><noscript><img src=\"http://example.com/tracker.png\"></noscript>";
			const string expected = "<p>text</p><noscript></noscript>";
			var converter = new HtmlToHtml { HtmlTagCallback = RemoveImagesCallback, NoScriptHandling = HtmlNoScriptHandling.ScriptingDisabled };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[TestCase ("<noscript><style></noscript><img src=x onerror=alert(1)></style></noscript>", "<style></noscript><img src=x onerror=alert(1)></style>")]
		[TestCase ("<noscript><!--</noscript><img src=x onerror=alert(1)>--></noscript>", "<!--</noscript><img src=x onerror=alert(1)>-->")]
		[TestCase ("<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></p></noscript>", "<p title=\"&lt;/noscript&gt;&lt;img src=x onerror=alert(1)&gt;\"></p>")]
		public void TestNoScriptScriptingDisabledRemoveNoScriptTags (string input, string expected)
		{
			var converter = new HtmlToHtml { HtmlTagCallback = RemoveImagesAndNoScriptTagsCallback, NoScriptHandling = HtmlNoScriptHandling.ScriptingDisabled };

			// Note: without the <noscript> tags, the output is interpreted the same regardless of whether
			// scripting is enabled in the renderer, so the embedded "</noscript>" is harmless.
			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFilterComments ()
		{
			const string input = "<html><head><!-- this is a comment --></head><body>Here is the body content <!-- this is another comment -->which seems fine so far</body></html>";
			const string expected = "<html><head></head><body>Here is the body content which seems fine so far</body></html>";
			var converter = new HtmlToHtml { FilterComments = true };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestHeaderFooter ()
		{
			const string input = "<body>Here is the body content which seems fine so far</body>";
			const string expected = "<html><head></head><body>Here is the body content which seems fine so far</body></html>";
			var converter = new HtmlToHtml {
				HeaderFormat = HeaderFooterFormat.Html,
				Header = "<html><head></head>",
				FooterFormat = HeaderFooterFormat.Html,
				Footer = "</html>"
			};

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestTextHeaderFooter ()
		{
			const string input = "<body>Here is the body content which seems fine so far</body>";
			const string expected = "&lt;html&gt;&lt;head&gt;&lt;/head&gt;<br/><body>Here is the body content which seems fine so far</body>&lt;/html&gt;<br/>";
			var converter = new HtmlToHtml {
				HeaderFormat = HeaderFooterFormat.Text,
				Header = "<html><head></head>",
				FooterFormat = HeaderFooterFormat.Text,
				Footer = "</html>"
			};

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestIssue808 ()
		{
			const string input = "<html><body>I'm on holiday until&nbsp; June 17, 2022.&#13;</body></html>";
			const string expected = "<html><body>I'm on holiday until&nbsp; June 17, 2022.&#13;</body></html>";
			var converter = new HtmlToHtml ();

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		static void SuppressXCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagName.Equals ("x", StringComparison.OrdinalIgnoreCase))
				ctx.SuppressInnerContent = true;

			ctx.WriteTag (htmlWriter, true);
		}

		[TestCase ("<a><b>1</a>2</b>3", "<a><b>1</a>2</b>3")]
		[TestCase ("<x>1<y>2</x>3</y>4", "<x></x>3</y>4")]
		[TestCase ("<x>1<X>2</x>3</X>4", "<x></X>4")]
		[TestCase ("<y><x>1</y>2</x>3<x>4</X>5", "<y><x></x>3<x></X>5")]
		[TestCase ("<x>1<x>2<x>3</x>4</x>5</x>6", "<x></x>6")]
		[TestCase ("</x>1<x>2</y>3</x>4</x>5", "</x>1<x></x>4</x>5")]
		public void TestOutOfOrderEndTags (string input, string expected)
		{
			var converter = new HtmlToHtml { HtmlTagCallback = SuppressXCallback };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		// The tokenizer used to leak the first few characters following a short bogus DOCTYPE keyword into the
		// name of the next tag, which then caused HtmlWriter to throw an ArgumentException for the invalid tag name.
		[TestCase ("<!DOCTYPE html </DIV><DIV class=x>y</DIV>", "<!DOCTYPE html><DIV class=\"x\">y</DIV>")]
		[TestCase ("<!DOCTYPE html bog><p>y</p>", "<!DOCTYPE html><p>y</p>")]
		public void TestShortBogusDocTypeKeyword (string input, string expected)
		{
			var converter = new HtmlToHtml ();

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));
		}

		static string Repeat (string value, int count)
		{
			var builder = new StringBuilder (value.Length * count);

			for (int i = 0; i < count; i++)
				builder.Append (value);

			return builder.ToString ();
		}

		// Prior to using HtmlTagContextStack, each of these took time quadratic in the number of unclosed tags
		// (100,000 unmatched end tags took over 2 minutes).
		[Test]
		public void TestManyUnclosedTagsFollowedByUnmatchedEndTags ()
		{
			const int count = 100000;
			var input = Repeat ("<b>", count) + "x" + Repeat ("</i>", count);
			var converter = new HtmlToHtml ();

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (input));
		}

		[Test]
		public void TestManyUnclosedTagsFollowedByReopenedOuterElement ()
		{
			const int count = 100000;
			var input = "<a>" + Repeat ("<b>", count) + Repeat ("</a><a>", count);
			var converter = new HtmlToHtml ();

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (input));
		}

		[Test]
		public void TestManyUnclosedTagsInsideSuppressedElement ()
		{
			const int count = 100000;
			var input = "<x>" + Repeat ("<b>y", count) + Repeat ("</i>", count) + "</x>z";
			var converter = new HtmlToHtml { HtmlTagCallback = SuppressXCallback };

			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo ("<x></x>z"));
		}

		// Each of these used to hide the <img> tag from the HtmlTagCallback due to tokenizer reconsume bugs.
		[TestCase ("<<img src=x onerror=alert(1)>")]
		[TestCase ("<script>x</scrip</script><img src=x onerror=alert(1)>")]
		[TestCase ("<style>x</styl</style><img src=x onerror=alert(1)>")]
		[TestCase ("<textarea>x</textare</textarea><img src=x onerror=alert(1)>")]
		[TestCase ("<!DOC><img src=x onerror=alert(1)>")]
		[TestCase ("<![CDAT><img src=x onerror=alert(1)>")]
		[TestCase ("<script><!--<script</script><img src=x onerror=alert(1)>")]
		[TestCase ("<script><!--</a <script></script><!--</script><img src=x onerror=alert(1)>-->")]
		[TestCase ("<script><!--<script></a></script></script><img src=x onerror=alert(1)>")]
		[TestCase ("<style/><!--</style><img src=x onerror=alert(1)>-->")]
		[TestCase ("<script/><!--</script><img src=x onerror=alert(1)>-->")]
		[TestCase ("<textarea/><!--</textarea><img src=x onerror=alert(1)>-->")]
		[TestCase ("<![CDATA[ x ><img src=x onerror=alert(1)>]]>")]
		[TestCase ("<svg><style><img src=x onerror=alert(1)></style></svg>")]
		[TestCase ("<math><style><img src=x onerror=alert(1)></style></math>")]
		[TestCase ("<svg><script><img src=x onerror=alert(1)></script></svg>")]
		[TestCase ("<div><svg></div><style><!--</style><img src=x onerror=alert(1)>-->")]
		[TestCase ("<foo><svg></foo><style><!--</style><img src=x onerror=alert(1)>-->")]
		[TestCase ("<svg><p><style><!--</style><img src=x onerror=alert(1)>-->")]
		public void TestTagCallbackSeesAllTags (string input)
		{
			var converter = new HtmlToHtml {
				HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId != HtmlTagId.Image)
						ctx.WriteTag (writer, true);
					else
						ctx.DeleteTag = true;
				}
			};

			var result = converter.Convert (input);

			Assert.That (result, Does.Not.Contain ("onerror"));
		}

		// Browsers ignore the self-closing flag on <style/> and <script/>, so the content that follows is raw text
		// and must be suppressed along with the element.
		[TestCase ("<style/>body { color: red; }</style><p>x</p>", "<p>x</p>")]
		[TestCase ("<script/>alert(1)</script><p>x</p>", "<p>x</p>")]
		public void TestSuppressSelfClosingRawTextElement (string input, string expected)
		{
			var converter = new HtmlToHtml {
				HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Style || ctx.TagId == HtmlTagId.Script) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						ctx.SuppressInnerContent = true;
					} else {
						ctx.WriteTag (writer, true);
					}
				}
			};

			Assert.That (converter.Convert (input), Is.EqualTo (expected));
		}

		// A literal '<' at the end of a data token must not combine with the data that follows dropped markup to form a new tag.
		[TestCase ("<</>c>", false, "&lt;c>")]
		[TestCase ("<<!-- x -->img src=x onerror=alert(1)>", true, "&lt;img src=x onerror=alert(1)>")]
		[TestCase ("<<!-- x -->img>", false, "&lt;<!-- x -->img>")]
		[TestCase ("a <<b>img onerror=alert(1)>", false, "a &lt;img onerror=alert(1)>")]
		public void TestDroppedMarkupDoesNotFormNewTag (string input, bool filterComments, string expected)
		{
			var converter = new HtmlToHtml {
				FilterComments = filterComments,
				HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.B) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
					} else {
						ctx.WriteTag (writer, true);
					}
				}
			};

			Assert.That (converter.Convert (input), Is.EqualTo (expected));
		}

		// "<a href/=javascript:x>" has a valueless "href" attribute followed by an attribute named "=javascript:x". Make sure
		// that the output does not get reparsed as an href attribute with a value of "javascript:x".
		[Test]
		public void TestAttributeNameStartingWithEquals ()
		{
			const string input = "<a href/=javascript:x>y</a>";

			var converter = new HtmlToHtml ();
			Assert.That (converter.Convert (input), Is.EqualTo ("<a href=\"\" =javascript:x>y</a>"));

			converter.HtmlTagCallback = (ctx, writer) => ctx.WriteTag (writer, true);
			Assert.That (converter.Convert (input), Is.EqualTo ("<a href=\"\" =javascript:x>y</a>"));
		}

		// Bogus comments must be written such that they tokenize as the same bogus comment again. In particular,
		// "</" followed by a non-letter used to be written without the '/' which could turn the comment into a tag.
		[TestCase ("</<script x>", "</<script x>")]
		[TestCase ("</ <img src=x>", "</ <img src=x>")]
		[TestCase ("</<s", "</<s>")]
		[TestCase ("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\"?>")]
		[TestCase ("</?x>", "<?x>")]
		[TestCase ("<!DOC>", "<!DOC>")]
		[TestCase ("<!-x>", "<!-x>")]
		public void TestBogusCommentRoundTrip (string input, string expected)
		{
			var converter = new HtmlToHtml ();
			var result = converter.Convert (input);

			Assert.That (result, Is.EqualTo (expected));

			var tokenizer = new HtmlTokenizer (new StringReader (result));
			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
			Assert.That (token, Is.InstanceOf<HtmlCommentToken> ());
			Assert.That (((HtmlCommentToken) token).IsBogusComment, Is.True);
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}
	}
}
