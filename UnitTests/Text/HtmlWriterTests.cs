//
// HtmlWriterTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute ("name", Array.Empty<char> (), -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute ("name", Array.Empty<char> (), 0, 1));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute (HtmlAttributeId.Unknown, new char[1], 0, 1));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (HtmlAttributeId.Alt, null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute (HtmlAttributeId.Alt, Array.Empty<char> (), -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttribute (HtmlAttributeId.Alt, Array.Empty<char> (), 0, 1));
				Assert.Throws<ArgumentException> (() => html.WriteAttribute (HtmlAttributeId.Unknown, "value"));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttribute (HtmlAttributeId.Alt, null));

				Assert.Throws<ArgumentException> (() => html.WriteAttributeName (HtmlAttributeId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeName (null));

				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeValue (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteAttributeValue (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttributeValue (Array.Empty<char> (), -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteAttributeValue (Array.Empty<char> (), 0, 1));

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
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteMarkupText (Array.Empty<char> (), -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteMarkupText (Array.Empty<char> (), 0, 1));

				Assert.Throws<ArgumentException> (() => html.WriteStartTag (HtmlTagId.Unknown));
				Assert.Throws<ArgumentNullException> (() => html.WriteStartTag (null));
				Assert.Throws<ArgumentException> (() => html.WriteStartTag (string.Empty));
				Assert.Throws<ArgumentException> (() => html.WriteStartTag ("a b c"));

				Assert.Throws<ArgumentNullException> (() => html.WriteText (null));
				Assert.Throws<ArgumentNullException> (() => html.WriteText (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteText (Array.Empty<char> (), -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => html.WriteText (Array.Empty<char> (), 0, 1));

				Assert.Throws<ArgumentNullException> (() => html.WriteToken (null));
			}
		}

		static void TestHtmlWriter (HtmlWriter html, object output)
		{
			const string expected = "<html ltr=\"true\"><head/><body>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters in this text should get encoded: &lt;&gt;&#39;&amp;\n" +
				"and this is a formatted string with a few args: 1 apple<br/><br/></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters should not get encoded: &lt;&gt;" +
				"</p><p></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters in this text should get encoded: &lt;&gt;&#39;&amp;\n<br/><br/></p>" +
				"<p class=\"paragraph\" style=\"font: arial; color: red\" align=\"left\">" +
				"special characters should not get encoded: &lt;&gt;" +
				"</p></body></html>";
			const string format = "and this is a formatted string with a few args: {0} {1}";
			const string text = "special characters in this text should get encoded: <>'&\n";
			const string markup = "special characters should not get encoded: &lt;&gt;";
			const string style = "font: arial; color: red";

			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

			// make sure we can't start by writing an attribute since we are in the wrong state
			Assert.Throws<InvalidOperationException> (() => html.WriteAttribute (new HtmlAttribute (HtmlAttributeId.Action, "invalid state")));
			Assert.Throws<InvalidOperationException> (() => html.WriteAttribute (HtmlAttributeId.Action, "invalid state"));
			Assert.Throws<InvalidOperationException> (() => html.WriteAttribute ("action", "invalid state"));

			// write a tag
			html.WriteStartTag (HtmlTagId.Html);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			// *now* we should be able to write an attribute
			html.WriteAttribute (new HtmlAttribute ("ltr", "true"));

			// write en empty element tag, this should change the state to Default
			html.WriteEmptyElementTag (HtmlTagId.Head);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteStartTag ("body");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteStartTag (HtmlTagId.P);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			// make sure that we can't write an attribute value yet
			Assert.Throws<InvalidOperationException> (() => html.WriteAttributeValue ("attrValue"));
			Assert.Throws<InvalidOperationException> (() => html.WriteAttributeValue ("attrValue".ToCharArray (), 0, 9));

			html.WriteAttributeName (HtmlAttributeId.Class);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Attribute));

			html.WriteAttributeValue ("paragraph");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteAttributeName ("style");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Attribute));

			html.WriteAttributeValue (style.ToCharArray (), 0, style.Length);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteAttribute (HtmlAttributeId.Align, "left");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteText (text);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

			html.WriteText (format, 1, "apple");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

			html.WriteEmptyElementTag ("br");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));
			html.WriteEmptyElementTag ("br");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteEndTag ("p");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

			Assert.Throws<InvalidOperationException> (() => html.WriteAttributeName ("style"));
			Assert.Throws<InvalidOperationException> (() => html.WriteAttributeName (HtmlAttributeId.Style));

			html.WriteStartTag ("p");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteAttribute (HtmlAttributeId.Class, "paragraph".ToCharArray (), 0, "paragraph".Length);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteAttribute ("style", style.ToCharArray (), 0, style.Length);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteAttribute ("align", "left");
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Tag));

			html.WriteMarkupText (markup);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

			html.WriteEndTag (HtmlTagId.P);
			Assert.That (html.WriterState, Is.EqualTo (HtmlWriterState.Default));

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

			var paraEndTag = new HtmlTagToken ("p", true);
			html.WriteToken (paraEndTag);

			html.WriteEndTag (HtmlTagId.Body);
			html.WriteEndTag ("html");
			html.Flush ();

			string actual;

			if (output is MemoryStream memory) {
				actual = Encoding.UTF8.GetString (memory.GetBuffer (), 0, (int) memory.Length);
			} else if (output is StringBuilder sb) {
				actual = sb.ToString ();
			} else {
				throw new NotImplementedException ();
			}

			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestHtmlWriterToStringBuilder ()
		{
			var sb = new StringBuilder ();

			using (var html = new HtmlWriter (new StringWriter (sb)))
				TestHtmlWriter (html, sb);
		}

		[Test]
		public void TestHtmlWriterToStream ()
		{
			var memory = new MemoryStream ();

			using (var html = new HtmlWriter (memory, new UTF8Encoding (false)))
				TestHtmlWriter (html, memory);
		}
	}
}
