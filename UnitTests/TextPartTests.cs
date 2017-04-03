//
// TextPartTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Jeffrey Stedfast
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
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.Text;

namespace UnitTests
{
	[TestFixture]
	public class TextPartTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var text = new TextPart (TextFormat.Plain);

			Assert.Throws<ArgumentNullException> (() => new TextPart ("plain", (object[]) null));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", Encoding.UTF8, "blah blah blah", Encoding.UTF8));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", Encoding.UTF8, "blah blah blah", "blah blah"));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TextPart ((TextFormat) 500));

			Assert.Throws<ArgumentNullException> (() => text.Accept (null));
			Assert.Throws<ArgumentNullException> (() => text.GetText ((string) null));
			Assert.Throws<ArgumentNullException> (() => text.GetText ((Encoding) null));
			Assert.Throws<ArgumentNullException> (() => text.SetText ((string) null, "text"));
			Assert.Throws<ArgumentNullException> (() => text.SetText ((Encoding) null, "text"));
			Assert.Throws<ArgumentNullException> (() => text.SetText ("iso-8859-1", null));
			Assert.Throws<ArgumentNullException> (() => text.SetText (Encoding.UTF8, null));
		}

		[Test]
		public void TestFormat ()
		{
			TextPart text;

			text = new TextPart (TextFormat.Html);
			Assert.IsTrue (text.IsHtml, "IsHtml");
			Assert.IsFalse (text.IsPlain, "IsPlain");
			Assert.IsFalse (text.IsFlowed, "IsFlowed");
			Assert.IsFalse (text.IsRichText, "IsRichText");

			text = new TextPart (TextFormat.Plain);
			Assert.IsFalse (text.IsHtml, "IsHtml");
			Assert.IsTrue (text.IsPlain, "IsPlain");
			Assert.IsFalse (text.IsFlowed, "IsFlowed");
			Assert.IsFalse (text.IsRichText, "IsRichText");

			text = new TextPart (TextFormat.Flowed);
			Assert.IsFalse (text.IsHtml, "IsHtml");
			Assert.IsTrue (text.IsPlain, "IsPlain");
			Assert.IsTrue (text.IsFlowed, "IsFlowed");
			Assert.IsFalse (text.IsRichText, "IsRichText");

			text = new TextPart (TextFormat.RichText);
			Assert.IsFalse (text.IsHtml, "IsHtml");
			Assert.IsFalse (text.IsPlain, "IsPlain");
			Assert.IsFalse (text.IsFlowed, "IsFlowed");
			Assert.IsTrue (text.IsRichText, "IsRichText");
		}
	}
}
