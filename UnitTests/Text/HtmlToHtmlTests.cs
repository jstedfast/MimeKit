//
// HtmlToHtmlTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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

using System;
using System.IO;
using System.Text;

using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlToHtmlTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new HtmlToHtml ();
			var reader = new StringReader ("");
			var writer = new StringWriter ();

			Assert.AreEqual (TextFormat.Html, converter.InputFormat);
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat);
			Assert.IsFalse (converter.DetectEncodingFromByteOrderMark);
			Assert.IsFalse (converter.FilterComments);
			Assert.IsFalse (converter.FilterHtml);
			Assert.IsNull (converter.Footer);
			Assert.IsNull (converter.Header);
			Assert.AreEqual (HeaderFooterFormat.Text, converter.FooterFormat);
			Assert.AreEqual (HeaderFooterFormat.Text, converter.HeaderFormat);

			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
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

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestFilterComments ()
		{
			const string input = "<html><head><!-- this is a comment --></head><body>Here is the body content <!-- this is another comment -->which seems fine so far</body></html>";
			const string expected = "<html><head></head><body>Here is the body content which seems fine so far</body></html>";
			var converter = new HtmlToHtml { FilterComments = true };

			var result = converter.Convert (input);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestFilterHtml ()
		{
			const string input = "<html><head><script>/* this is a script */</script></head><body>Here is the body content which seems fine so far</body></html>";
			const string expected = "<html><head></head><body>Here is the body content which seems fine so far</body></html>";
			var converter = new HtmlToHtml { FilterHtml = true };

			var result = converter.Convert (input);

			Assert.AreEqual (expected, result);
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

			Assert.AreEqual (expected, result);
		}
	}
}
