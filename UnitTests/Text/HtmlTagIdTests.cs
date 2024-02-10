//
// HtmlTagIdTests.cs
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
	public class HtmlTagIdTests
	{
		[Test]
		public void TestToHtmlTagId ()
		{
			Assert.That ("".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Unknown), "string.Empty");
			Assert.That ("!".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Comment), "!");
			Assert.That ("!blah".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Comment), "!blah");
			Assert.That ("a".ToHtmlTagId (), Is.EqualTo (HtmlTagId.A), "a");
			Assert.That ("A".ToHtmlTagId (), Is.EqualTo (HtmlTagId.A), "A");
			Assert.That ("font".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Font), "font");
			Assert.That ("FONT".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Font), "FONT");
			Assert.That ("FoNt".ToHtmlTagId (), Is.EqualTo (HtmlTagId.Font), "FoNt");

			HtmlTagId parsed;
			string name;

			foreach (HtmlTagId value in Enum.GetValues (typeof (HtmlTagId))) {
				if (value == HtmlTagId.Unknown)
					continue;

				name = value.ToHtmlTagName ().ToUpperInvariant ();
				parsed = name.ToHtmlTagId ();

				Assert.That (parsed, Is.EqualTo (value), $"Failed to parse the HtmlTagId value for {value}");
			}

			name = ((HtmlTagId) 1024).ToHtmlTagName ();
			Assert.That (name, Is.EqualTo ("1024"), "ToHtmlTagName() for unknown value");
		}

		[Test]
		public void TestIsFormattingElement ()
		{
			var formattingElements = new[] { "a", "b", "big", "code", "em", "font", "i", "nobr", "s", "small", "strike", "strong", "tt", "u" };

			foreach (var element in formattingElements) {
				var tag = element.ToHtmlTagId ();

				Assert.That (tag.IsFormattingElement (), Is.True, element);
			}

			Assert.That ("body".ToHtmlTagId ().IsFormattingElement (), Is.False, "body");
		}

		[Test]
		public void TestIsEmptyElement ()
		{
			var emptyElements = new[] { "area", "base", "br", "col", "command", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr" };

			foreach (var element in emptyElements) {
				var tag = element.ToHtmlTagId ();

				Assert.That (tag.IsEmptyElement (), Is.True, element);
			}

			Assert.That ("body".ToHtmlTagId ().IsEmptyElement (), Is.False, "body");
		}
	}
}
