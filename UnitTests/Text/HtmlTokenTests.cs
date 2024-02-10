//
// HtmlTokenTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlTokenTests
	{
		class BrokenHtmlDataToken : HtmlDataToken
		{
			public BrokenHtmlDataToken (string data) : base (HtmlTokenKind.Comment, data)
			{
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var comment = new HtmlCommentToken ("This is a comment.");
			var cdata = new HtmlCDataToken ("This is some CDATA.");
			var data = new HtmlDataToken ("This is some character data.");
			var script = new HtmlScriptDataToken ("This is some script data.");
			var doc = new HtmlDocTypeToken ();
			var tag = new HtmlTagToken ("name", false);
			var attributes = Array.Empty<HtmlAttribute> ();

			Assert.Throws<ArgumentNullException> (() => new HtmlCommentToken (null));
			Assert.Throws<ArgumentNullException> (() => comment.WriteTo (null));

			Assert.Throws<ArgumentNullException> (() => new HtmlCDataToken (null));
			Assert.Throws<ArgumentNullException> (() => cdata.WriteTo (null));

			Assert.Throws<ArgumentNullException> (() => new HtmlDataToken (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => new BrokenHtmlDataToken ("This is some character data."));
			Assert.Throws<ArgumentNullException> (() => data.WriteTo (null));

			Assert.Throws<ArgumentNullException> (() => doc.WriteTo (null));

			Assert.Throws<ArgumentNullException> (() => new HtmlTagToken (null, attributes, false));
			Assert.Throws<ArgumentNullException> (() => new HtmlTagToken ("name", null, false));
			Assert.Throws<ArgumentNullException> (() => new HtmlTagToken (null, false));
			Assert.Throws<ArgumentNullException> (() => tag.WriteTo (null));

			Assert.Throws<ArgumentNullException> (() => new HtmlScriptDataToken (null));
			Assert.Throws<ArgumentNullException> (() => script.WriteTo (null));
		}

		[Test]
		public void TestHtmlTagTokenCtor ()
		{
			var attrs = new HtmlAttribute[] { new HtmlAttribute ("src", "image.png"), new HtmlAttribute ("alt", "[image]") };
			var token = new HtmlTagToken ("img", attrs, true);

			Assert.That (token.Id, Is.EqualTo (HtmlTagId.Image));
			Assert.That (token.IsEmptyElement, Is.True);
			Assert.That (token.IsEndTag, Is.False);
			Assert.That (token.Attributes.Count, Is.EqualTo (2));
		}

		[Test]
		public void TestHtmlDocTypePublicIdentifier ()
		{
			var doctype = new HtmlDocTypeToken ();

			doctype.PublicIdentifier = "public-identifier";
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("public-identifier"), "PublicIdentifier");
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PUBLIC"), "PublicKeyword");
			Assert.That (doctype.SystemKeyword, Is.Null, "SystemKeyword");

			doctype.PublicIdentifier = null;
			Assert.That (doctype.PublicIdentifier, Is.Null, "PublicIdentifier");
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PUBLIC"), "PublicKeyword");
			Assert.That (doctype.SystemKeyword, Is.Null, "SystemKeyword");

			doctype.PublicIdentifier = "public-identifier";
			doctype.SystemIdentifier = "system-identifier";
			doctype.PublicIdentifier = null;
			Assert.That (doctype.PublicIdentifier, Is.Null, "PublicIdentifier");
			Assert.That (doctype.PublicKeyword, Is.EqualTo ("PUBLIC"), "PublicKeyword");
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SYSTEM"), "SystemKeyword");
		}

		[Test]
		public void TestHtmlDocTypeSystemIdentifier ()
		{
			var doctype = new HtmlDocTypeToken ();

			doctype.SystemIdentifier = "system-identifier";
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("system-identifier"), "SystemIdentifier");
			Assert.That (doctype.SystemKeyword, Is.EqualTo ("SYSTEM"), "SystemKeyword");

			doctype.SystemIdentifier = null;
			Assert.That (doctype.SystemIdentifier, Is.Null, "SystemIdentifier");
			Assert.That (doctype.SystemKeyword, Is.Null, "SystemKeyword");
		}
	}
}
