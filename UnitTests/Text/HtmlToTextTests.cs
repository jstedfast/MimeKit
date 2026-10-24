//
// HtmlToTextTests.cs
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

using System.Text;

using MimeKit.Text;

using UnitTests.IO;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlToTextTests
	{
		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new HtmlToText ();

			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Html));
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Plain));
			Assert.That (converter.OutputLinkUrls, Is.True);
			Assert.That (converter.OutputImageAltText, Is.True);
			Assert.That (converter.MaxElementDepth, Is.EqualTo (4096));
			Assert.That (converter.MaxInputLength, Is.EqualTo (16777216));
			Assert.That (converter.MaxOutputLength, Is.EqualTo (67108864));
		}

		static readonly object[] FormattingCases = {
			new object[] { "", "" },
			new object[] { " \t\r\n ", "" },
			new object[] { "one \t\r\n two", "one two" },
			new object[] { "<p>Hello <b>bold</b> and <em>italic</em>.</p><p>Next.</p>", "Hello bold and italic.\n\nNext." },
			new object[] { "<div>one</div><div>two<br>three</div>", "one\ntwo\nthree" },
			new object[] { "one<br><br>two<br>", "one\n\ntwo\n" },
			new object[] { "<br>one</br>two", "\none\ntwo" },
			new object[] { "<p></p><div></div><p>one</p><p></p>", "one" },
			new object[] { "<h1>Title</h1><h2>Subtitle</h2><p>Body.</p><hr><p>End.</p>", "# Title\n\n## Subtitle\n\nBody.\n\n---\n\nEnd." },
			new object[] { "<h3>Three</h3><h4>Four</h4><h5>Five</h5><h6>Six</h6>", "### Three\n\n#### Four\n\n##### Five\n\n###### Six" },
			new object[] { "<h1>Title</h2><p>Text", "# Title\n\nText" },
			new object[] { "<p>one<p>two<div>three</div>four", "one\n\ntwo\n\nthree\nfour" },
			new object[] { "one<!-- comment -->two &amp; &lt;three&gt; &#x1f600; &nbsp; end", "onetwo & <three> \U0001f600 \u00a0 end" },
			new object[] { "<html><head><title>Hidden</title><style>p { color:red }</style><script>x()</script></head><body><p>Visible</p></body></html>", "Visible" },
			new object[] { "<head><title>Hidden</title><p>Visible", "Visible" },
			new object[] { "<title>Hidden</title>Visible", "Visible" },
			new object[] { "a<script/>bad <b>stuff</b></script>b<style/>bad</style>c", "abc" },
			new object[] { "<div hidden><p>Hidden</p></div><p>Visible</p>", "Visible" },
			new object[] { "<body hidden><p>Hidden</p></body>", "" },
			new object[] { "<html hidden><body>Hidden</body></html>", "" },
			new object[] { "<p hidden>Hidden<p>Visible", "Visible" },
			new object[] { "<ul><li hidden>Hidden<li>Visible</ul>", "- Visible" },
			new object[] { "<h1 hidden>Hidden<h2>Visible", "## Visible" },
			new object[] { "<ul hidden><li>Hidden<li>Still hidden</ul>Visible", "Visible" },
			new object[] { "<template><p>Hidden</p><template>Also hidden</template></template>Visible", "Visible" },
			new object[] { "<noscript><p>Fallback</p></noscript>", "Fallback" },
			new object[] { "<p>x<template><div>Hidden</div></template>y</p>", "xy" },
			new object[] { "<a href='url'>x<template><a href='hidden'>Hidden</a></template>y</a>", "xy (url)" },
			new object[] { "<p>x<div hidden><p>Hidden</p></div>y", "x\n\ny" },
			new object[] { "<svg><text>SVG &amp; text</text><![CDATA[ <raw> ]]></svg>", "SVG & text <raw>" },
			new object[] { "<ul><li>one</li><li>two</li></ul><p>End</p>", "- one\n- two\n\nEnd" },
			new object[] { "<ol start='3'><li>three<li value='7'>seven<li>eight</ol>", "3. three\n7. seven\n8. eight" },
			new object[] { "<ol start='-2'><li>a<li>b</ol>", "-2. a\n-1. b" },
			new object[] { "<ol start='bad'><li value='bad'>a<li>b</ol>", "1. a\n2. b" },
			new object[] { "<ul><li>outer<ol><li>inner<br>continued</li><li>second</li></ol>tail</li><li>last</li></ul>", "- outer\n  1. inner\n     continued\n  2. second\n  tail\n- last" },
			new object[] { "<ul><li><p>First.</p><p>Second.</p></li><li>Next.</li></ul>", "- First.\n\n  Second.\n- Next." },
			new object[] { "<ul><li></li><li>x</li></ul>", "-\n- x" },
			new object[] { "<ul><li><br>next</li><li>end</li></ul>", "-\n  next\n- end" },
			new object[] { "<blockquote>one<br>two<blockquote>inner</blockquote>tail</blockquote>end", "> one\n> two\n\n>> inner\n\n> tail\n\nend" },
			new object[] { "<blockquote><ul><li>one<li>two</ul></blockquote>", "> - one\n> - two" },
			new object[] { "<dl><dt>Term</dt><dd>Meaning</dd><dt>Other</dt><dd>Definition</dd></dl>", "Term\n  Meaning\nOther\n  Definition" },
			new object[] { "<dl><dt>Term<dd>Meaning<dt>Other<dd>Definition</dl>", "Term\n  Meaning\nOther\n  Definition" },
			new object[] { "<pre>\n  one\t two\n\nthree  </pre><p>End</p>", "  one\t two\n\nthree  \n\nEnd" },
			new object[] { "<pre>\r\none\r\ntwo\rthree\n</pre>", "one\ntwo\nthree\n" },
			new object[] { "<pre>  &lt;b&gt; <b>bold</b>\t end</pre>", "  <b> bold\t end" },
			new object[] { "<pre><!-- comment -->\nkeep</pre>", "\nkeep" },
			new object[] { "<pre><b>\nkeep</b></pre>", "\nkeep" },
			new object[] { "<pre>\n\nkeep</pre>", "\nkeep" },
			new object[] { "<pre>one<br>two</pre>", "one\ntwo" },
			new object[] { "<plaintext>\n  <b>literal</b>", "\n  <b>literal</b>" },
			new object[] { "<blockquote><pre>\na\n b\n</pre></blockquote>", "> a\n>  b\n" },
			new object[] { "<textarea>\n a &amp; b\n</textarea>", " a & b\n" },
			new object[] { "<xmp><b>literal</b></xmp>", "<b>literal</b>" },
			new object[] { "Visit <a href='https://example.com/?a=1&amp;b=2'>Example</a>.", "Visit Example (https://example.com/?a=1&b=2)." },
			new object[] { "See <a href='https://example.com'>https://example.com</a>.", "See https://example.com." },
			new object[] { "<a href='/relative'><b>Label</b></a>", "Label (/relative)" },
			new object[] { "<a href='url'></a>", "url" },
			new object[] { "<a href='url'>Label", "Label (url)" },
			new object[] { "<a href='url'><p>Label</p></a>", "Label (url)" },
			new object[] { "<a href='outer'>one<a href='inner'>two</a>", "one (outer)two (inner)" },
			new object[] { "<a href='  '>Label</a><a>No URL</a>", "LabelNo URL" },
			new object[] { "<a href='url'> url </a>", "url" },
			new object[] { "<a href='url'>Label <script>Hidden</script> text</a>", "Label text (url)" },
			new object[] { "An <img alt='A &amp; B'> image<img src='x'>.", "An A & B image." },
			new object[] { "<a href='url'><img alt='Image'></a>", "Image (url)" },
			new object[] { "<table><tr><th>Name</th><th>Value</th></tr><tr><td>A</td><td>B</td></tr></table>", "Name Value\nA B" },
			new object[] { "<table><tr><td>A<td>B<tr><td>C<td>D</table>", "A B\nC D" },
			new object[] { "<div><b>one</div>two</b>", "one\ntwo" },
			new object[] { "<p>one</unknown>two</p>", "onetwo" },
			new object[] { "one<img alt='truncated", "one" },
			new object[] { "<div/>one</div>two", "one\ntwo" }
		};

		[TestCaseSource (nameof (FormattingCases))]
		public void TestFormatting (string html, string expected)
		{
			var converter = new HtmlToText ();

			Assert.That (converter.Convert (html), Is.EqualTo (expected.Replace ("\n", Environment.NewLine)));
			using var chunked = new ChunkedReadStream (Encoding.UTF8.GetBytes (html), 1);
			using var writer = new StringWriter { NewLine = "\n" };
			converter.Convert (chunked, writer);
			Assert.That (writer.ToString (), Is.EqualTo (expected), "chunked UTF-8 input");
		}

		[Test]
		public void TestOptionsAndReuse ()
		{
			var converter = new HtmlToText {
				OutputLinkUrls = false, OutputImageAltText = false,
				Header = "[header]", Footer = "[footer]"
			};
			const string html = "<p><a href='url'>Label</a><img alt='Image'></p>";

			Assert.That (converter.Convert (html), Is.EqualTo ("[header]Label[footer]"));
			converter.OutputLinkUrls = converter.OutputImageAltText = true;
			converter.Header = converter.Footer = null;
			Assert.That (converter.Convert (html), Is.EqualTo ("Label (url)Image"));
			Assert.That (converter.Convert ("plain"), Is.EqualTo ("plain"));
		}

		[Test]
		public void TestStreamOverloads ()
		{
			var converter = new HtmlToText { InputEncoding = Encoding.Unicode, OutputEncoding = new UTF8Encoding (false) };
			using var input = new MemoryStream (Encoding.Unicode.GetBytes ("<p>caf\u00e9</p>"));
			using var output = new MemoryStream ();

			converter.Convert (input, output);
			Assert.That (Encoding.UTF8.GetString (output.ToArray ()), Is.EqualTo ("caf\u00e9"));
			Assert.That (input.CanRead, Is.True);
			Assert.That (output.CanWrite, Is.True);

			using var reader = new StringReader ("<h1>Title</h1>");
			output.SetLength (0);
			converter.Convert (reader, output);
			Assert.That (Encoding.UTF8.GetString (output.ToArray ()), Is.EqualTo ("# Title"));
			Assert.That (reader.Peek (), Is.EqualTo (-1));
		}

		[TestCase ("<b>", "</b>")]
		[TestCase ("<blockquote>", "</blockquote>")]
		[TestCase ("<template>", "</template>")]
		public void TestDepthLimit (string start, string end)
		{
			var converter = new HtmlToText { MaxElementDepth = 8 };
			string valid = string.Concat (Enumerable.Repeat (start, 8)) + "x" + string.Concat (Enumerable.Repeat (end, 8));

			Assert.DoesNotThrow (() => converter.Convert (valid));
			Assert.Throws<FormatException> (() => converter.Convert (start + valid + end));
			Assert.That (converter.Convert ("ok"), Is.EqualTo ("ok"));
		}

		[TestCase ("plain text")]
		[TestCase ("<script>hidden</script>")]
		[TestCase ("<img alt='attribute'>")]
		[TestCase ("<!-- comment -->")]
		public void TestInputLengthLimit (string html)
		{
			var converter = new HtmlToText { MaxInputLength = html.Length };

			Assert.DoesNotThrow (() => converter.Convert (html));
			converter.MaxInputLength--;
			Assert.Throws<FormatException> (() => converter.Convert (html));
			Assert.That (converter.Convert ("ok"), Is.EqualTo ("ok"));
		}

		[Test]
		public void TestManySequentialElements ()
		{
			var converter = new HtmlToText { MaxElementDepth = 16 };
			string html = "<ol>" + string.Concat (Enumerable.Repeat ("<li>item", 10000)) + "</ol>";
			string expected = string.Join (Environment.NewLine, Enumerable.Range (1, 10000).Select (n => $"{n}. item"));

			Assert.That (converter.Convert (html), Is.EqualTo (expected));
			html = "<table>" + string.Concat (Enumerable.Repeat ("<tr><td>a<td>b", 10000)) + "</table>";
			expected = string.Join (Environment.NewLine, Enumerable.Repeat ("a b", 10000));
			Assert.That (converter.Convert (html), Is.EqualTo (expected));
		}

		[Test]
		public void TestTokenBoundaries ()
		{
			var converter = new HtmlToText ();
			string word = new string ('a', 20000);

			Assert.That (converter.Convert ("<pre>\n" + word + "\r\n  end</pre>"), Is.EqualTo (word + Environment.NewLine + "  end"));
			Assert.That (converter.Convert (word + " \t\r\n <b>end</b>"), Is.EqualTo (word + " end"));
			Assert.That (converter.Convert ("<a href='" + word + "'>" + word + "</a>"), Is.EqualTo (word));
		}

		[TestCase ("<blockquote><ol><li>one<li>two</ol></blockquote>")]
		[TestCase ("<p>one</p><p>two</p>")]
		[TestCase ("<a href='destination'>Label</a>")]
		[TestCase ("<pre>\n a\tb\n</pre>")]
		public void TestOutputLengthLimit (string html)
		{
			var converter = new HtmlToText ();
			string expected = converter.Convert (html);
			converter.MaxOutputLength = expected.Length;
			Assert.That (converter.Convert (html), Is.EqualTo (expected));
			converter.MaxOutputLength--;
			using var reader = new StringReader (html);
			using var writer = new StringWriter ();

			Assert.Throws<FormatException> (() => converter.Convert (reader, writer));
			Assert.That (writer.ToString ().Length, Is.LessThanOrEqualTo (converter.MaxOutputLength));
			Assert.That (converter.Convert ("ok"), Is.EqualTo ("ok"));
		}

		[Test]
		public void TestInputLimitIncludesTruncatedTokens ()
		{
			var converter = new HtmlToText { MaxInputLength = 64 };

			foreach (string prefix in new[] { "<img alt='", "<script>", "<!--", "&", "<" }) {
				string html = prefix + new string ('a', 10000);
				using var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (html), 1);
				using var writer = new StringWriter ();
				Assert.Throws<FormatException> (() => converter.Convert (input, writer));
				Assert.That (input.Position, Is.LessThan (html.Length), prefix);
			}
		}

		[Test]
		public void TestDepthLimitFailsBeforeEndOfInput ()
		{
			var converter = new HtmlToText { MaxElementDepth = 4 };
			string html = "<b><i><u><s><span>" + new string ('x', 10000);
			using var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (html), 1);
			using var writer = new StringWriter ();

			Assert.Throws<FormatException> (() => converter.Convert (input, writer));
			Assert.That (input.Position, Is.LessThan (html.Length));
		}

		[Test]
		public void TestUnmatchedEndTags ()
		{
			var converter = new HtmlToText { MaxElementDepth = 32 };
			string html = string.Concat (Enumerable.Repeat ("<b>", 16)) +
				string.Concat (Enumerable.Repeat ("</unknown>", 10000)) + "text";

			Assert.That (converter.Convert (html), Is.EqualTo ("text"));
		}

		[Test]
		public void TestIOExceptions ()
		{
			var converter = new HtmlToText ();
			using var input = new ThrowingReadStream ();
			using var output = new ThrowingWriteStream ();
			using var writer = new StringWriter ();
			using var reader = new StringReader ("text");

			Assert.Throws<IOException> (() => converter.Convert (input, writer));
			Assert.That (input.IsDisposed, Is.False);
			Assert.Throws<IOException> (() => converter.Convert (reader, output));
			Assert.That (output.IsDisposed, Is.False);
			Assert.That (converter.Convert ("ok"), Is.EqualTo ("ok"));
		}
	}
}
