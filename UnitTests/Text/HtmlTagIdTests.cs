//
// HtmlTagIdTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using NUnit.Framework;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlTagIdTests
	{
		[Test]
		public void TestToHtmlTagId ()
		{
			Assert.AreEqual (HtmlTagId.Unknown, "".ToHtmlTagId (), "string.Empty");
			Assert.AreEqual (HtmlTagId.Comment, "!".ToHtmlTagId (), "!");
			Assert.AreEqual (HtmlTagId.Comment, "!blah".ToHtmlTagId (), "!blah");
			Assert.AreEqual (HtmlTagId.A, "a".ToHtmlTagId (), "a");
			Assert.AreEqual (HtmlTagId.A, "A".ToHtmlTagId (), "A");
			Assert.AreEqual (HtmlTagId.Font, "font".ToHtmlTagId (), "font");
			Assert.AreEqual (HtmlTagId.Font, "FONT".ToHtmlTagId (), "FONT");
			Assert.AreEqual (HtmlTagId.Font, "FoNt".ToHtmlTagId (), "FoNt");
		}

		[Test]
		public void TestIsFormattingElement ()
		{
			var formattingElements = new[] { "a", "b", "big", "code", "em", "font", "i", "nobr", "s", "small", "strike", "strong", "tt", "u" };

			foreach (var element in formattingElements) {
				var tag = element.ToHtmlTagId ();

				Assert.IsTrue (tag.IsFormattingElement (), element);
			}

			Assert.IsFalse ("body".ToHtmlTagId ().IsFormattingElement (), "body");
		}
	}
}
