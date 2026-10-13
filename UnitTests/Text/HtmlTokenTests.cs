//
// HtmlTokenTests.cs
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

		// Values that cannot be written as a well-formed DOCTYPE used to be accepted, which allowed markup to be
		// injected into (or after) the DOCTYPE, e.g. Name = "html><script>".
		[TestCase ("")]
		[TestCase ("html><script>alert(1)</script")]
		[TestCase ("html PUBLIC")]
		[TestCase ("a\tb")]
		[TestCase ("a\nb")]
		[TestCase ("a\fb")]
		[TestCase ("a\rb")]
		[TestCase ("a\0b")]
		public void TestHtmlDocTypeInvalidName (string name)
		{
			var doctype = new HtmlDocTypeToken { Name = "html" };

			Assert.Throws<ArgumentException> (() => doctype.Name = name);
			Assert.That (doctype.Name, Is.EqualTo ("html"));
		}

		[TestCase ("x\"><script>alert(1)</script>")]
		[TestCase ("a>b")]
		[TestCase ("a\0b")]
		[TestCase ("a'b\"c")]
		public void TestHtmlDocTypeInvalidIdentifier (string identifier)
		{
			var doctype = new HtmlDocTypeToken { Name = "html", PublicIdentifier = "p", SystemIdentifier = "s" };

			Assert.Throws<ArgumentException> (() => doctype.PublicIdentifier = identifier);
			Assert.Throws<ArgumentException> (() => doctype.SystemIdentifier = identifier);
			Assert.That (doctype.PublicIdentifier, Is.EqualTo ("p"));
			Assert.That (doctype.SystemIdentifier, Is.EqualTo ("s"));
		}

		[TestCase ("html", "-//W3C//DTD HTML 4.01//EN", "http://www.w3.org/TR/html4/strict.dtd")]
		[TestCase ("<x/\"'", "it's", "say \"hi\"")]
		[TestCase ("html", "", "")]
		[TestCase ("html", null, "s")]
		[TestCase ("html", "p", null)]
		public void TestHtmlDocTypeSettersRoundTrip (string name, string publicIdentifier, string systemIdentifier)
		{
			var doctype = new HtmlDocTypeToken { Name = name, PublicIdentifier = publicIdentifier, SystemIdentifier = systemIdentifier };
			var tokenizer = new HtmlTokenizer (new StringReader (doctype.ToString () + "<p>"));

			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
			Assert.That (token, Is.InstanceOf<HtmlDocTypeToken> ());

			var parsed = (HtmlDocTypeToken) token;

			Assert.That (parsed.Name, Is.EqualTo (name), "Name");
			Assert.That (parsed.PublicIdentifier, Is.EqualTo (publicIdentifier), "PublicIdentifier");
			Assert.That (parsed.SystemIdentifier, Is.EqualTo (systemIdentifier), "SystemIdentifier");
			Assert.That (parsed.ForceQuirksMode, Is.False, "ForceQuirksMode");
			Assert.That (tokenizer.ReadNextToken (out token), Is.True);
			Assert.That (token.Kind, Is.EqualTo (HtmlTokenKind.Tag));
		}

		// Without a name, the PUBLIC or SYSTEM keyword would be parsed as the name, so the identifiers are omitted.
		[Test]
		public void TestHtmlDocTypeWithoutName ()
		{
			var doctype = new HtmlDocTypeToken { PublicIdentifier = "p", SystemIdentifier = "s" };

			Assert.That (doctype.ToString (), Is.EqualTo ("<!DOCTYPE>"));

			doctype = new HtmlDocTypeToken { SystemIdentifier = "s" };

			Assert.That (doctype.ToString (), Is.EqualTo ("<!DOCTYPE>"));
		}
	}
}
