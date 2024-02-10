//
// HtmlAttributeTests.cs
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
	public class HtmlAttributeTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlAttribute (HtmlAttributeId.Unknown, string.Empty));
			Assert.Throws<ArgumentNullException> (() => new HtmlAttribute (null, string.Empty));
			Assert.Throws<ArgumentException> (() => new HtmlAttribute (string.Empty, string.Empty));
			Assert.Throws<ArgumentException> (() => new HtmlAttribute ("a b c", string.Empty));
		}

		[Test]
		public void TestToHtmlAttributeId ()
		{
			Assert.That ("".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Unknown), "string.Empty");
			Assert.That ("alt".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Alt), "alt");
			Assert.That ("Alt".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Alt), "Alt");
			Assert.That ("aLt".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Alt), "aLt");
			Assert.That ("ALT".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Alt), "ALT");
			Assert.That ("AlT".ToHtmlAttributeId (), Is.EqualTo (HtmlAttributeId.Alt), "AlT");

			HtmlAttributeId parsed;
			string name;

			foreach (HtmlAttributeId value in Enum.GetValues (typeof (HtmlAttributeId))) {
				if (value == HtmlAttributeId.Unknown)
					continue;

				name = value.ToAttributeName ().ToUpperInvariant ();
				parsed = name.ToHtmlAttributeId ();

				Assert.That (parsed, Is.EqualTo (value), $"Failed to parse the HtmlAttributeId value for {value}");
			}

			name = ((HtmlAttributeId) 1024).ToAttributeName ();
			Assert.That (name, Is.EqualTo ("1024"), "ToAttributeName() for unknown value");
		}
	}
}
