//
// BrowserOracleTests.cs
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

using MimeKit.Fuzzing.Helpers;
using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class BrowserOracleTests
	{
		static HtmlTagToken ReadTag (string html)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html));

			Assert.That (tokenizer.ReadNextToken (out var token), Is.True);
			Assert.That (token, Is.TypeOf<HtmlTagToken> ());

			return (HtmlTagToken) token;
		}

		[TestCase ("<x = 'v';>")]
		[TestCase ("<x = >")]
		[TestCase ("<x/=>")]
		[TestCase ("<x/a=b>")]
		[TestCase ("<x/>")]
		[TestCase ("<x a='b'>")]
		[TestCase ("<x a=foo/>")]
		[TestCase ("<x-->")]
		[TestCase ("<x]]>")]
		[TestCase ("<x--!>")]
		public void TestMarkPreservesAttributes (string html)
		{
			var original = ReadTag (html);
			var tags = new Dictionary<int, HtmlTagToken> { { 0, original } };
			var marked = BrowserOracle.Mark (html, tags, out var markers);
			var actual = ReadTag (marked);

			Assert.That (markers, Is.EqualTo (new[] { original }));
			Assert.That (actual.Name, Is.EqualTo (original.Name));
			Assert.That (actual.IsEmptyElement, Is.EqualTo (original.IsEmptyElement));
			Assert.That (actual.Attributes.Count, Is.EqualTo (original.Attributes.Count + 1));
			Assert.That (actual.Attributes[0].Name, Is.EqualTo ("mkoracle0"));

			for (int i = 0; i < original.Attributes.Count; i++) {
				Assert.That (actual.Attributes[i + 1].Name, Is.EqualTo (original.Attributes[i].Name));
				Assert.That (actual.Attributes[i + 1].Value, Is.EqualTo (original.Attributes[i].Value));
			}
		}

		[TestCase ("<!--<x--><p>")]
		[TestCase ("<!--<x--!><p>")]
		[TestCase ("<svg><![CDATA[<x]]><p>")]
		public void TestMarkPreservesTextTerminators (string html)
		{
			var tags = new Dictionary<int, HtmlTagToken> { { html.IndexOf ("<x", StringComparison.Ordinal), new HtmlTagToken ("x", false) } };
			var marked = BrowserOracle.Mark (html, tags, out _);
			var tokenizer = new HtmlTokenizer (new StringReader (marked));
			var names = new List<string> ();

			while (tokenizer.ReadNextToken (out var token)) {
				if (token is HtmlTagToken tag)
					names.Add (tag.Name);
			}

			Assert.That (names, Is.EqualTo (html.StartsWith ("<svg>", StringComparison.Ordinal) ? new[] { "svg", "p" } : new[] { "p" }));
		}
	}
}
