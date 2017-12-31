//
// HtmlWriterTests.cs
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

using NUnit.Framework;

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlWriterTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new HtmlWriter (null, Encoding.UTF8));
			Assert.Throws<ArgumentNullException> (() => new HtmlWriter (new MemoryStream (), null));
			Assert.Throws<ArgumentNullException> (() => new HtmlWriter (null));

			using (var html = new HtmlWriter (new StringWriter ())) {
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (null, string.Empty));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute ("name", null));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute (string.Empty, null));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute ("a b c", null));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (null, new char[1], 0, 1));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute ("name", null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute ("name", new char[0], -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute ("name", new char[0], 0, 1));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute (HtmlAttributeId.Unknown, new char[1], 0, 1));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (HtmlAttributeId.Alt, null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute (HtmlAttributeId.Alt, new char[0], -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute (HtmlAttributeId.Alt, new char[0], 0, 1));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute (HtmlAttributeId.Unknown, "value"));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (HtmlAttributeId.Alt, null));

				Assert.Throws<ArgumentException> (() => html.WriteAttributeName (HtmlAttributeId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeName (null));

				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeValue (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeValue (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttributeValue (new char[0], -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttributeValue (new char[0], 0, 1));

				Assert.Throws<ArgumentException> (() => html.WriteEmptyElementTag (HtmlTagId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteEmptyElementTag (null));
				Assert.Throws<ArgumentException> (() => html.WriteEmptyElementTag (string.Empty));
				Assert.Throws<ArgumentException> (() => html.WriteEmptyElementTag ("a b c"));

				Assert.Throws<ArgumentException> (() => html.WriteEndTag (HtmlTagId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteEndTag (null));
				Assert.Throws<ArgumentException> (() => html.WriteEndTag (string.Empty));
				Assert.Throws<ArgumentException> (() => html.WriteEndTag ("a b c"));

				Assert.Throws<ArgumentNullException> (() => html.WriteMarkupText (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteMarkupText (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteMarkupText (new char[0], -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteMarkupText (new char[0], 0, 1));

				Assert.Throws<ArgumentException> (() => html.WriteStartTag (HtmlTagId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteStartTag (null));
				Assert.Throws<ArgumentException> (() => html.WriteStartTag (string.Empty));
				Assert.Throws<ArgumentException> (() => html.WriteStartTag ("a b c"));

				Assert.Throws<ArgumentNullException> (() => html.WriteText (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteText (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteText (new char[0], -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteText (new char[0], 0, 1));

				Assert.Throws<ArgumentNullException> (() => html.WriteToken (null));
			}
		}

		[Test]
		public void TestHtmlWriter ()
		{
			const string expected = "<html ltr=\"true\"><head/><body>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters in this text should get encoded: &lt;&gt;&#39;&amp;\n<br/><br/></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters should not get encoded: &lt;&gt;" +
				"</p><p></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters in this text should get encoded: &lt;&gt;&#39;&amp;\n<br/><br/></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters should not get encoded: &lt;&gt;" +
				"</p></body></html>";
			var text = "special characters in this text should get encoded: <>'&\n";
			var markup = "special characters should not get encoded: &lt;&gt;";
			var style = "font: arial; color: red";
			var actual = new StringBuilder ();

			using (var html = new HtmlWriter (new StringWriter (actual))) {
				Assert.AreEqual (HtmlWriterState.Default, html.WriterState);

				// make sure we can't start by writing an attribute since we are in the wrong state
				Assert.Throws<InvalidOperationException> (() => html.WriteAttribute (new HtmlAttribute (HtmlAttributeId.Action, "invalid state")));
				Assert.Throws<InvalidOperationException> (() => html.WriteAttribute (HtmlAttributeId.Action, "invalid state"));
				Assert.Throws<InvalidOperationException> (() => html.WriteAttribute ("action", "invalid state"));

				// write a tag
				html.WriteStartTag (HtmlTagId.Html);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				// *now* we should be able to write an attribute
				html.WriteAttribute (new HtmlAttribute ("ltr", "true"));

				// write en empty element tag, this should change the state to Default
				html.WriteEmptyElementTag (HtmlTagId.Head);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteStartTag ("body");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteStartTag (HtmlTagId.P);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				// make sure that we can't write an attribute value yet
				Assert.Throws<InvalidOperationException> (() => html.WriteAttributeValue ("attrValue"));
				Assert.Throws<InvalidOperationException> (() => html.WriteAttributeValue ("attrValue".ToCharArray (), 0, 9));

				html.WriteAttributeName (HtmlAttributeId.Class);
				Assert.AreEqual (HtmlWriterState.Attribute, html.WriterState);

				html.WriteAttributeValue ("paragraph");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteAttributeName ("style");
				Assert.AreEqual (HtmlWriterState.Attribute, html.WriterState);

				html.WriteAttributeValue (style.ToCharArray (), 0, style.Length);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteAttribute (HtmlAttributeId.Align, "left");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteText (text);
				Assert.AreEqual (HtmlWriterState.Default, html.WriterState);

				html.WriteEmptyElementTag ("br");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);
				html.WriteEmptyElementTag ("br");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteEndTag ("p");
				Assert.AreEqual (HtmlWriterState.Default, html.WriterState);

				Assert.Throws<InvalidOperationException> (() => html.WriteAttributeName ("style"));
				Assert.Throws<InvalidOperationException> (() => html.WriteAttributeName (HtmlAttributeId.Style));

				html.WriteStartTag ("p");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteAttribute (HtmlAttributeId.Class, "paragraph".ToCharArray (), 0, "paragraph".Length);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteAttribute ("style", style.ToCharArray (), 0, style.Length);
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteAttribute ("align", "left");
				Assert.AreEqual (HtmlWriterState.Tag, html.WriterState);

				html.WriteMarkupText (markup);
				Assert.AreEqual (HtmlWriterState.Default, html.WriterState);

				html.WriteEndTag (HtmlTagId.P);
				Assert.AreEqual (HtmlWriterState.Default, html.WriterState);

				html.WriteStartTag (HtmlTagId.P);
				html.WriteEndTag (HtmlTagId.P);

				html.WriteStartTag ("p");
				html.WriteAttribute ("class", "paragraph");
				html.WriteAttribute ("style", style);
				html.WriteAttribute ("align", "left");
				html.WriteText (text.ToCharArray (), 0, text.Length);
				html.WriteEmptyElementTag ("br");
				html.WriteEmptyElementTag ("br");
				html.WriteEndTag ("p");

				html.WriteStartTag ("p");
				html.WriteAttribute ("class", "paragraph");
				html.WriteAttribute ("style", style);
				html.WriteAttribute ("align", "left");
				html.WriteMarkupText (markup.ToCharArray (), 0, markup.Length);
				html.WriteEndTag ("p");

				html.WriteEndTag (HtmlTagId.Body);
				html.WriteEndTag ("html");
				html.Flush ();
			}

			Assert.AreEqual (expected, actual.ToString ());
		}
	}
}
