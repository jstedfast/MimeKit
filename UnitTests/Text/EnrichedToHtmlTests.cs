//
// EnrichedToHtmlTests.cs
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
	public class EnrichedToHtmlTests
	{
		const string Prefix = "<div style=\"white-space: pre-wrap;\">";
		const string Suffix = "</div>";

		static readonly object[][] FormattingCases = {
			new object[] { "", "" },
			new object[] { "plain", "<div>plain</div>" },
			new object[] { "a  b\tc", "<div>a  b\tc</div>" },
			new object[] { "a & b << c > d", "<div>a &amp; b &lt; c &gt; d</div>" },
			new object[] { "&lt;b&gt;", "<div>&amp;lt;b&amp;gt;</div>" },
			new object[] { "<<script>alert(1)<</script>", "<div>&lt;script&gt;alert(1)&lt;/script&gt;</div>" },
			new object[] { "a<bold>b</bold>c", "<div>a<span style=\"font-weight: bold;\">b</span>c</div>" },
			new object[] { "<BoLd><ITALIC>x</italic></bOlD>", "<div><span style=\"font-weight: bold; font-style: italic;\">x</span></div>" },
			new object[] { "<underline>x</underline>", "<div><span style=\"text-decoration: underline;\">x</span></div>" },
			new object[] { "<fixed>x</fixed>", "<div><span style=\"font-family: monospace;\">x</span></div>" },
			new object[] { "<bold>a<bold>b</bold>c</bold>", "<div><span style=\"font-weight: bold;\">a</span><span style=\"font-weight: bold;\">b</span><span style=\"font-weight: bold;\">c</span></div>" },
			new object[] { "<bigger>x</bigger>", "<div><span style=\"font-size: 1.167em;\">x</span></div>" },
			new object[] { "<smaller>x</smaller>", "<div><span style=\"font-size: 0.833em;\">x</span></div>" },
			new object[] { "<bigger><smaller>x</smaller></bigger>", "<div>x</div>" },
			new object[] { "<bigger><bigger>x</bigger></bigger>", "<div><span style=\"font-size: 1.333em;\">x</span></div>" },
			new object[] { "a\nb", "<div>a b</div>" },
			new object[] { "a\r\nb", "<div>a b</div>" },
			new object[] { "a\rb", "<div>a b</div>" },
			new object[] { "a\n\nb", "<div>a</div><div>b</div>" },
			new object[] { "a\r\n\r\nb", "<div>a</div><div>b</div>" },
			new object[] { "a\n\n\nb", "<div>a</div><div><br/></div><div>b</div>" },
			new object[] { "a\r\r\nb", "<div>a</div><div>b</div>" },
			new object[] { "\n\nx", "<div><br/></div><div>x</div>" },
			new object[] { "x\n", "<div>x </div>" },
			new object[] { "x\n\n", "<div>x</div><div><br/></div>" },
			new object[] { "\n", "<div> </div>" },
			new object[] { "a\n \nb", "<div>a   b</div>" },
			new object[] { "<nofill>a\nb</nofill>", "<div style=\"white-space: pre;\">a</div><div style=\"white-space: pre;\">b</div>" },
			new object[] { "<nofill>a\r\n\r\nb</nofill>", "<div style=\"white-space: pre;\">a</div><div style=\"white-space: pre;\"><br/></div><div style=\"white-space: pre;\">b</div>" },
			new object[] { "a<nofill>b</nofill>c", "<div>a</div><div style=\"white-space: pre;\">b</div><div>c</div>" },
			new object[] { "a<center>b</center>c", "<div>a</div><div style=\"text-align: center;\">b</div><div>c</div>" },
			new object[] { "<center>a</center>\n\nb", "<div style=\"text-align: center;\">a</div><div>b</div>" },
			new object[] { "a<center>\n\nb</center>c", "<div>a</div><div style=\"text-align: center;\">b</div><div>c</div>" },
			new object[] { "<center>a</center>\n\n\nb", "<div style=\"text-align: center;\">a</div><div><br/></div><div>b</div>" },
			new object[] { "<center>a</center><bold></bold>\n\nb", "<div style=\"text-align: center;\">a</div><div>b</div>" },
			new object[] { "<center>\n\nx</center>", "<div style=\"text-align: center;\"><br/></div><div style=\"text-align: center;\">x</div>" },
			new object[] { "<nofill>a\n</nofill>", "<div style=\"white-space: pre;\">a</div><div style=\"white-space: pre;\"><br/></div>" },
			new object[] { "<nofill>a\n\n</nofill>", "<div style=\"white-space: pre;\">a</div><div style=\"white-space: pre;\"><br/></div><div style=\"white-space: pre;\"><br/></div>" },
			new object[] { "<flushboth><nofill>x</nofill></flushboth>", "<div style=\"white-space: pre; text-align: left;\">x</div>" },
			new object[] { "<flushleft>x</flushleft>", "<div style=\"text-align: left;\">x</div>" },
			new object[] { "<flushright>x</flushright>", "<div style=\"text-align: right;\">x</div>" },
			new object[] { "<flushboth>x</flushboth>", "<div style=\"text-align: justify;\">x</div>" },
			new object[] { "<center>a<flushright>b</flushright>c</center>", "<div style=\"text-align: center;\">a</div><div style=\"text-align: right;\">b</div><div style=\"text-align: center;\">c</div>" },
			new object[] { "<bold>a<center>b</center>c</bold>", "<div><span style=\"font-weight: bold;\">a</span></div><div style=\"text-align: center;\"><span style=\"font-weight: bold;\">b</span></div><div><span style=\"font-weight: bold;\">c</span></div>" },
			new object[] { "<excerpt>a<excerpt>b</excerpt>c</excerpt>", "<div style=\"margin-left: 4ch;\">a</div><div style=\"margin-left: 8ch;\">b</div><div style=\"margin-left: 4ch;\">c</div>" },
			new object[] { "<excerpt><param>source</param>x</excerpt>", "<div style=\"margin-left: 4ch;\">x</div>" },
			new object[] { "<paraindent><param>Left, RIGHT, in</param>x\n\ny</paraindent>", "<div style=\"margin-left: 4ch; margin-right: 4ch; text-indent: 4ch;\">x</div><div style=\"margin-left: 4ch; margin-right: 4ch; text-indent: 4ch;\">y</div>" },
			new object[] { "<paraindent><param>left,left,out</param>x</paraindent>", "<div style=\"margin-left: 12ch; text-indent: -4ch;\">x</div>" },
			new object[] { "<paraindent><param>left</param><paraindent><param>right</param>x</paraindent></paraindent>", "<div style=\"margin-left: 4ch; margin-right: 4ch;\">x</div>" },
			new object[] { "<paraindent><param>in,out</param>x</paraindent>", "<div>x</div>" },
			new object[] { "<paraindent><param>left,evil</param>x</paraindent>", "<div>x</div>" },
			new object[] { "<color><param> ReD </param>x</color>", "<div><span style=\"color: red;\">x</span></div>" },
			new object[] { "<color><param>FFFF,8000,0000</param>x</color>", "<div><span style=\"color: #ff8000;\">x</span></div>" },
			new object[] { "<color><param>blue</param>a<color><param>white</param>b</color>c</color>", "<div><span style=\"color: blue;\">a</span><span style=\"color: white;\">b</span><span style=\"color: blue;\">c</span></div>" },
			new object[] { "<color>x</color>", "<div>x</div>" },
			new object[] { "<fontfamily><param>Times New Roman</param>x</fontfamily>", "<div><span style=\"font-family: 'Times New Roman';\">x</span></div>" },
			new object[] { "<fixed>a<fontfamily><param>Arial</param>b</fontfamily>c</fixed>", "<div><span style=\"font-family: monospace;\">a</span><span style=\"font-family: 'Arial';\">b</span><span style=\"font-family: monospace;\">c</span></div>" },
			new object[] { "<lang><param>en-US</param>x</lang>", "<div><span lang=\"en-US\">x</span></div>" },
			new object[] { "<lang><param>zh-Hant-TW</param>x</lang>", "<div><span lang=\"zh-Hant-TW\">x</span></div>" },
			new object[] { "<lang><param>es-419</param>x</lang>", "<div><span lang=\"es-419\">x</span></div>" },
			new object[] { "<color><param>ffff,ffff,ffff</param>x</color>", "<div><span style=\"color: #ffffff;\">x</span></div>" },
			new object[] { "<color><param>0000,0000,0000</param>x</color>", "<div><span style=\"color: #000000;\">x</span></div>" },
			new object[] { "<x-new><param>hidden</param>visible</x-new>", "<div>visible</div>" },
			new object[] { "<indent>a</indent><indentright>b</indentright>", "<div>ab</div>" },
			new object[] { "<bold><italic>x</bold>y</italic>", "<div><span style=\"font-weight: bold; font-style: italic;\">x</span>y</div>" },
			new object[] { "a</unknown>b", "<div>ab</div>" },
			new object[] { "<bold>x", "<div><span style=\"font-weight: bold;\">x</span></div>" },
			new object[] { "<param>hidden", "" },
			new object[] { "a<param>hidden</param>b", "<div>ab</div>" },
			new object[] { "<color><param>red<param>hidden</param>more</param>x</color>", "<div>x</div>" },
			new object[] { "<param>hidden</bold>still hidden</param>x", "<div>x</div>" },
			new object[] { "<color><param><bold>red</bold></param>x</color>", "<div>x</div>" },
			new object[] { "<color><param>blue</param><color><param>invalid</param>x</color></color>", "<div><span style=\"color: blue;\">x</span></div>" },
			new object[] { "<color> <param>red</param>x</color>", "<div> x</div>" },
			new object[] { "<bold", "<div>&lt;bold</div>" },
			new object[] { "<", "<div>&lt;</div>" },
			new object[] { "</", "<div>&lt;/</div>" },
			new object[] { "<>", "<div>&lt;&gt;</div>" },
			new object[] { "<bold attr=x>visible", "<div>&lt;bold attr=x&gt;visible</div>" }
		};

		[TestCaseSource (nameof (FormattingCases))]
		public void TestFormatting (string enriched, string expected)
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true };

			Assert.That (converter.Convert (enriched), Is.EqualTo (Prefix + expected + Suffix));
			using var input = new ChunkedReadStream (Encoding.UTF8.GetBytes (enriched), 1);
			using var writer = new StringWriter ();
			converter.Convert (input, writer);
			Assert.That (writer.ToString (), Is.EqualTo (Prefix + expected + Suffix), "chunked UTF-8");
		}

		[Test]
		public void TestRfc1896Example ()
		{
			const string input = "<bold>Now</bold> is the time for <italic>all</italic>\n" +
				"good men\n<smaller>(and <<women>)</smaller> to\n<ignoreme>come</ignoreme>\n\n" +
				"to the aid of their\n\n\n<color><param>red</param>beloved</color>\ncountry.\n\n" +
				"By the way,\nI think that <paraindent><param>left</param><<smaller>\n\n" +
				"</paraindent>should REALLY be called\n\n" +
				"<paraindent><param>left</param><<tinier></paraindent>\n\n" +
				"and that I am always right.\n\n-- the end";
			const string expected = "<div><span style=\"font-weight: bold;\">Now</span> is the time for " +
				"<span style=\"font-style: italic;\">all</span> good men " +
				"<span style=\"font-size: 0.833em;\">(and &lt;women&gt;)</span> to come</div>" +
				"<div>to the aid of their</div><div><br/></div>" +
				"<div><span style=\"color: red;\">beloved</span> country.</div>" +
				"<div>By the way, I think that </div>" +
				"<div style=\"margin-left: 4ch;\">&lt;smaller&gt;</div>" +
				"<div>should REALLY be called</div>" +
				"<div style=\"margin-left: 4ch;\">&lt;tinier&gt;</div>" +
				"<div>and that I am always right.</div><div>-- the end</div>";

			TestFormatting (input, expected);
		}

		[TestCase ("color", "red; background: url(https://example.com)")]
		[TestCase ("color", "ffff,0000,000g")]
		[TestCase ("color", "#ff0000")]
		[TestCase ("fontfamily", "Arial';color:red;/*")]
		[TestCase ("fontfamily", "Arial&quot; onclick=&quot;evil")]
		[TestCase ("fontfamily", "url(https://example.com)")]
		[TestCase ("fontfamily", "Arial\\27")]
		[TestCase ("lang", "en\" onclick=\"evil")]
		[TestCase ("lang", "-en")]
		[TestCase ("lang", "en-")]
		[TestCase ("lang", "en--US")]
		[TestCase ("lang", "123")]
		[TestCase ("lang", "toolonglanguage")]
		[TestCase ("paraindent", "left; color:red")]
		public void TestInvalidParameters (string command, string value)
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true };
			var input = $"<{command}><param>{value}</param>x</{command}>";
			Assert.That (converter.Convert (input), Is.EqualTo (Prefix + "<div>x</div>" + Suffix));
		}

		[Test]
		public void TestDefaultsAndHeaderFooter ()
		{
			var converter = new EnrichedToHtml ();
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Enriched));
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Html));
			Assert.That (converter.OutputHtmlFragment, Is.False);
			Assert.That (converter.MaxElementDepth, Is.EqualTo (4096));
			Assert.That (converter.MaxParameterLength, Is.EqualTo (4096));
			Assert.That (converter.MaxInputLength, Is.EqualTo (16777216));
			Assert.That (converter.MaxOutputLength, Is.EqualTo (67108864));
			Assert.That (converter.HeaderFormat, Is.EqualTo (HeaderFooterFormat.Text));
			Assert.That (converter.FooterFormat, Is.EqualTo (HeaderFooterFormat.Text));
			Assert.That (converter.Convert ("x"), Is.EqualTo ("<html><body>" + Prefix + "<div>x</div>" + Suffix + "</body></html>"));

			converter.Header = "<header>";
			converter.Footer = "<footer>";
			converter.OutputHtmlFragment = true;
			Assert.That (converter.Convert ("x"), Is.EqualTo ("&lt;header&gt;<br/>" + Prefix + "<div>x</div>" + Suffix + "&lt;footer&gt;<br/>"));
			converter.HeaderFormat = converter.FooterFormat = HeaderFooterFormat.Html;
			Assert.That (converter.Convert ("x"), Is.EqualTo ("<header>" + Prefix + "<div>x</div>" + Suffix + "<footer>"));
		}

		[Test]
		public void TestStreamOverloadsAndUnicode ()
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true, InputEncoding = Encoding.Unicode, OutputEncoding = new UTF8Encoding (false) };
			const string content = "caf\u00e9 \u65e5\u672c\u8a9e \ud83d\ude00";
			string expected = converter.Convert (content);
			using var input = new MemoryStream (Encoding.Unicode.GetBytes (content));
			using var output = new MemoryStream ();

			converter.Convert (input, output);
			Assert.That (Encoding.UTF8.GetString (output.ToArray ()), Is.EqualTo (expected));
			Assert.That (input.CanRead, Is.True);
			Assert.That (output.CanWrite, Is.True);

			using var reader = new StringReader (content);
			output.SetLength (0);
			converter.Convert (reader, output);
			Assert.That (Encoding.UTF8.GetString (output.ToArray ()), Is.EqualTo (expected));
			Assert.That (reader.Peek (), Is.EqualTo (-1));
		}

		[TestCase ("bold")]
		[TestCase ("x-unknown")]
		[TestCase ("param")]
		public void TestDepthLimit (string command)
		{
			var converter = new EnrichedToHtml { MaxElementDepth = 8 };
			string start = "<" + command + ">";
			string end = "</" + command + ">";
			string valid = string.Concat (Enumerable.Repeat (start, 8)) + "x" + string.Concat (Enumerable.Repeat (end, 8));

			Assert.DoesNotThrow (() => converter.Convert (valid));
			Assert.Throws<FormatException> (() => converter.Convert (start + valid + end));
			Assert.That (converter.Convert ("ok"), Does.Contain ("<div>ok</div>"));
		}

		[TestCase ("x\r\ny")]
		[TestCase ("x\r")]
		[TestCase ("x\r\n")]
		[TestCase ("x\r\ry")]
		[TestCase ("<bold>text</bold>")]
		[TestCase ("<color><param>red</param>x</color>")]
		[TestCase ("<param>hidden")]
		[TestCase ("<incomplete")]
		public void TestInputLengthLimit (string input)
		{
			var converter = new EnrichedToHtml { MaxInputLength = input.Length };
			Assert.DoesNotThrow (() => converter.Convert (input));
			converter.MaxInputLength--;
			Assert.Throws<FormatException> (() => converter.Convert (input));
			Assert.That (converter.Convert ("x"), Does.Contain ("<div>x</div>"));
		}

		[TestCase ("fontfamily", "Arial")]
		[TestCase ("color", "red")]
		[TestCase ("x-unknown", "ignored")]
		[TestCase ("x-unknown", "<<")]
		public void TestParameterLengthLimit (string command, string value)
		{
			var converter = new EnrichedToHtml { MaxParameterLength = value.Length };
			string input = $"<{command}><param>{value}</param>x</{command}>";
			Assert.DoesNotThrow (() => converter.Convert (input));
			converter.MaxParameterLength--;
			Assert.Throws<FormatException> (() => converter.Convert (input));
		}

		[Test]
		public void TestNestedAndNormalizedParameterLimits ()
		{
			var converter = new EnrichedToHtml { MaxParameterLength = 3, MaxElementDepth = 2 };
			Assert.DoesNotThrow (() => converter.Convert ("<color><param>r\r\nb</param>x</color>"));
			Assert.Throws<FormatException> (() => converter.Convert ("<color><param>r\r\ned</param>x</color>"));
			Assert.Throws<FormatException> (() => converter.Convert ("<color><param><param>x</param></param>x</color>"));
			converter.MaxElementDepth = 3;
			converter.MaxParameterLength = 16;
			Assert.DoesNotThrow (() => converter.Convert ("<color><param><param>x</param></param>x</color>"));
			converter.MaxParameterLength = 15;
			Assert.Throws<FormatException> (() => converter.Convert ("<color><param><param>x</param></param>x</color>"));
		}

		[TestCase ("plain text")]
		[TestCase ("<bold>& <<</bold>")]
		[TestCase ("<paraindent><param>left,left</param>text</paraindent>")]
		[TestCase ("<color><param>FFFF,0000,FFFF</param>text</color>")]
		public void TestOutputLengthLimit (string input)
		{
			var converter = new EnrichedToHtml { Header = "header", Footer = "footer" };
			string expected = converter.Convert (input);
			converter.MaxOutputLength = expected.Length;
			Assert.That (converter.Convert (input), Is.EqualTo (expected));
			converter.MaxOutputLength--;
			using var reader = new StringReader (input);
			using var writer = new StringWriter ();
			Assert.Throws<FormatException> (() => converter.Convert (reader, writer));
			Assert.That (writer.ToString ().Length, Is.LessThanOrEqualTo (converter.MaxOutputLength));
			Assert.DoesNotThrow (() => converter.Convert ("x"));
		}

		[Test]
		public void TestEarlyLimits ()
		{
			var converter = new EnrichedToHtml { MaxInputLength = 64, MaxParameterLength = 32, MaxElementDepth = 4 };
			foreach (string prefix in new[] { "", "<param>", "<bold><italic><fixed><underline><bigger>" }) {
				string content = prefix + new string ('x', 20000);
				using var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (content), 1);
				using var writer = new StringWriter ();
				Assert.Throws<FormatException> (() => converter.Convert (input, writer));
				Assert.That (input.Position, Is.LessThan (content.Length));
			}
		}

		[Test]
		public void TestEarlyOutputLimit ()
		{
			var converter = new EnrichedToHtml { MaxOutputLength = 64 };
			string content = new string ('&', 20000);
			using var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (content), 1);
			using var writer = new StringWriter ();

			Assert.Throws<FormatException> (() => converter.Convert (input, writer));
			Assert.That (writer.ToString ().Length, Is.LessThanOrEqualTo (64));
			Assert.That (input.Position, Is.LessThan (content.Length));
		}

		[Test]
		public void TestCommandLengthAndBufferBoundaries ()
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true };
			var command = new string ('x', 60);
			Assert.That (converter.Convert ($"<{command}>text</{command}>"), Is.EqualTo (Prefix + "<div>text</div>" + Suffix));
			command += "x";
			Assert.That (converter.Convert ($"<{command}>"), Is.EqualTo (Prefix + "<div>&lt;" + command + "&gt;</div>" + Suffix));
			foreach (int length in new[] { 1023, 1024, 4095, 4096, 20000 }) {
				string text = new string ('a', length);
				Assert.That (converter.Convert (text + "\r\n<bold>x</bold>"), Is.EqualTo (Prefix + "<div>" + text + " <span style=\"font-weight: bold;\">x</span></div>" + Suffix));
			}
		}

		[TestCase (1022)]
		[TestCase (1023)]
		[TestCase (1024)]
		[TestCase (4095)]
		public void TestSurrogatePairAtBufferBoundary (int length)
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true };
			string padding = new string ('a', length);
			string input = padding + "\ud83d\ude00z";
			string expected = Prefix + "<div>" + padding + "&#128512;z</div>" + Suffix;

			Assert.That (converter.Convert (input), Is.EqualTo (expected));
			using var stream = new ChunkedReadStream (Encoding.UTF8.GetBytes (input), 1);
			using var writer = new StringWriter ();
			converter.Convert (stream, writer);
			Assert.That (writer.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestManyCommandsAndSizeBounds ()
		{
			var converter = new EnrichedToHtml { OutputHtmlFragment = true };
			string input = string.Concat (Enumerable.Repeat ("<x-unknown>x</x-unknown></unmatched>", 10000));
			Assert.That (converter.Convert (input), Is.EqualTo (Prefix + "<div>" + new string ('x', 10000) + "</div>" + Suffix));
			input = string.Concat (Enumerable.Repeat ("<bold>", 1000)) + string.Concat (Enumerable.Repeat ("</unknown>", 10000)) + "x";
			Assert.That (converter.Convert (input), Is.EqualTo (Prefix + "<div><span style=\"font-weight: bold;\">x</span></div>" + Suffix));
			Assert.That (converter.Convert (string.Concat (Enumerable.Repeat ("<bigger>", 1000)) + "x"), Does.Contain ("font-size: 12em;"));
			Assert.That (converter.Convert (string.Concat (Enumerable.Repeat ("<smaller>", 1000)) + "x"), Does.Contain ("font-size: 0.167em;"));
		}

		[Test]
		public void TestIOErrorsAndReuse ()
		{
			var converter = new EnrichedToHtml ();
			using var input = new ThrowingReadStream ();
			using var output = new ThrowingWriteStream ();
			using var reader = new StringReader ("text");
			using var writer = new StringWriter ();

			Assert.Throws<IOException> (() => converter.Convert (input, writer));
			Assert.That (input.IsDisposed, Is.False);
			Assert.Throws<IOException> (() => converter.Convert (reader, output));
			Assert.That (output.IsDisposed, Is.False);
			Assert.That (converter.Convert ("ok"), Does.Contain ("<div>ok</div>"));
		}
	}
}
