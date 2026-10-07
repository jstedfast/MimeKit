//
// RtfToTextTests.cs
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
using System.Diagnostics;

using MimeKit.Text;
using MimeKit.Utils;

namespace UnitTests.Text {
	[TestFixture]
	public class RtfToTextTests
	{
		static readonly string NewLine = Environment.NewLine;

		[Test]
		public void TestArgumentExceptions ()
		{
			using var reader = new StringReader ("");
			using var writer = new StringWriter ();
			var converter = new RtfToText ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxFontTableEntries = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxFontTableEntries = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxColorTableEntries = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxColorTableEntries = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxGroupDepth = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.MaxGroupDepth = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, writer));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new RtfToText ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.InputEncoding, Is.EqualTo (CharsetUtils.Latin1), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.RichText), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Plain), "OutputFormat");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
			Assert.That (converter.MaxFontTableEntries, Is.EqualTo (4096), "MaxFontTableEntries");
			Assert.That (converter.MaxColorTableEntries, Is.EqualTo (4096), "MaxColorTableEntries");
			Assert.That (converter.MaxGroupDepth, Is.EqualTo (4096), "MaxGroupDepth");
		}

		static string Convert (string rtf, Action<RtfToText> configure = null)
		{
			var converter = new RtfToText ();

			configure?.Invoke (converter);

			return converter.Convert (rtf);
		}

		[Test]
		public void TestSimpleDocument ()
		{
			const string rtf = "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fswiss Arial;}}{\\colortbl;\\red255\\green0\\blue0;}" +
				"{\\*\\generator Riched20 10.0;}\\viewkind4\\uc1\\pard\\f0\\fs20 Hello \\b World\\b0 !\\par\r\n" +
				"Second\\line line\\tab x\\par\r\n}";

			Assert.That (Convert (rtf), Is.EqualTo ("Hello World!" + NewLine + "Second" + NewLine + "line\tx" + NewLine));
		}

		[Test]
		public void TestHeaderAndFooter ()
		{
			var text = Convert ("{\\rtf1 body}", c => { c.Header = "header "; c.Footer = " footer"; });

			Assert.That (text, Is.EqualTo ("header body footer"));
		}

		[Test]
		public void TestEmptyInput ()
		{
			Assert.That (Convert (string.Empty), Is.EqualTo (string.Empty));
			Assert.That (Convert ("{\\rtf1}"), Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestRawLineBreaksAreIgnored ()
		{
			// RTF 1.9.1, "Conventions of an RTF Reader": CR and LF in the input are not part of the text.
			Assert.That (Convert ("{\\rtf1 one\r\ntwo\rthree\nfour}"), Is.EqualTo ("onetwothreefour"));
		}

		[Test]
		public void TestEscapedLineBreakIsParagraph ()
		{
			// RTF 1.9.1, "Control Symbols": a backslash followed by CR or LF is equivalent to \par.
			Assert.That (Convert ("{\\rtf1 one\\\r\ntwo}"), Is.EqualTo ("one" + NewLine + "two"));
		}

		[Test]
		public void TestSpecialCharacters ()
		{
			const string rtf = "{\\rtf1 \\{\\}\\\\\\~\\_\\-\\emdash\\endash\\bullet\\lquote\\rquote\\ldblquote\\rdblquote\\emspace\\enspace\\qmspace\\zwj\\zwnj\\ltrmark\\rtlmark}";

			Assert.That (Convert (rtf), Is.EqualTo ("{}\\\u00A0\u2011\u2014\u2013\u2022\u2018\u2019\u201C\u201D\u2003\u2002\u2005\u200D\u200C\u200E\u200F"));
		}

		[Test]
		public void TestUnicode ()
		{
			// RTF 1.9.1, "Unicode RTF": \uN is followed by \ucN (default 1) fallback characters that must be skipped.
			Assert.That (Convert ("{\\rtf1\\ansi caf\\u233?!}"), Is.EqualTo ("caf\u00e9!"));
			Assert.That (Convert ("{\\rtf1\\ansi\\uc2\\u8364 XX!}"), Is.EqualTo ("\u20ac!"));
			Assert.That (Convert ("{\\rtf1\\ansi\\uc0\\u8364!}"), Is.EqualTo ("\u20ac!"));

			// a \'hh escape counts as a single fallback character
			Assert.That (Convert ("{\\rtf1\\ansi\\u8364\\'80!}"), Is.EqualTo ("\u20ac!"));

			// \ucN is group-scoped
			Assert.That (Convert ("{\\rtf1\\ansi{\\uc2\\u8364 XX}\\u8364 X!}"), Is.EqualTo ("\u20ac\u20ac!"));

			// "N is a signed 16-bit integer": negative values represent code points above 32767
			Assert.That (Convert ("{\\rtf1\\ansi\\u-200?!}"), Is.EqualTo ("\uFF38!"));
		}

		[Test]
		public void TestUnicodeSurrogatePair ()
		{
			// U+1F600 encoded as a UTF-16 surrogate pair (0xD83D = -10179, 0xDE00 = -8704)
			Assert.That (Convert ("{\\rtf1\\ansi\\u-10179?\\u-8704?}"), Is.EqualTo ("\U0001F600"));
			Assert.That (Convert ("{\\rtf1\\ansi\\uc0\\u-10179\\u-8704}"), Is.EqualTo ("\U0001F600"));
		}

		[Test]
		public void TestUnpairedSurrogates ()
		{
			// Unpaired surrogates are malformed UTF-16 and would make a strict encoder throw, so they become U+FFFD.
			Assert.That (Convert ("{\\rtf1\\ansi a\\u-10240?b}"), Is.EqualTo ("a\uFFFDb"));
			Assert.That (Convert ("{\\rtf1\\ansi a\\u-8704?b}"), Is.EqualTo ("a\uFFFDb"));
			Assert.That (Convert ("{\\rtf1\\ansi a\\u-10240?}"), Is.EqualTo ("a\uFFFD"));
			Assert.That (Convert ("{\\rtf1\\ansi a\\u-10240?\\par b}"), Is.EqualTo ("a\uFFFD" + Environment.NewLine + "b"));
			Assert.That (Convert ("{\\rtf1\\ansi {a\\u-10240?}b}"), Is.EqualTo ("a\uFFFDb"));
			Assert.That (Convert ("{\\rtf1\\ansi \\u-10240?\\u-10179?\\u-8704?}"), Is.EqualTo ("\uFFFD\U0001F600"));
			Assert.That (Convert ("{\\rtf1\\ansi \\u-10240?\\u65?}"), Is.EqualTo ("\uFFFDA"));
		}

		[Test]
		public void TestNestedTableProperties ()
		{
			// RTF 1.9.1, "Nested Tables": the \nestrow that ends a nested row lives inside {\*\nesttableprops ...},
			// and {\*\nonesttables ...} holds a duplicate rendering for readers without nested table support.
			const string rtf = "{\\rtf1\\ansi \\intbl\\itap2 a\\nestcell b\\nestcell{\\*\\nesttableprops\\trowd\\cellx100\\cellx200\\nestrow}" +
				"{\\nonesttables\\par a b}\\cell\\row}";
			var text = Convert (rtf);

			Assert.That (text, Does.Contain ("a"));
			Assert.That (text, Does.Contain ("b"));
			Assert.That (text, Does.Not.Contain ("a b"));

			// Some writers put the nested cell content inside the nesttableprops group itself.
			Assert.That (Convert ("{\\rtf1\\ansi {\\*\\nesttableprops\\trowd\\cellx100 Hello\\nestcell\\nestrow}}").Trim (), Is.EqualTo ("Hello"));
		}

		[Test]
		public void TestObjectResult ()
		{
			// RTF 1.9.1, "Objects": the \result destination holds the object's last rendered result for readers
			// that cannot display the object, while the object data itself is never rendered.
			const string rtf = "{\\rtf1\\ansi a{\\object\\objemb\\objw100\\objh100{\\*\\objclass Package}{\\*\\objdata 0105000002}" +
				"{\\objdata 0105000002}{\\result b}}c}";

			Assert.That (Convert (rtf), Is.EqualTo ("abc"));
		}

		[Test]
		public void TestDestinationsAfterPrematureRootGroupEnd ()
		{
			// An extra '}' closes the {\rtf1 ...} group early. The content that follows is still rendered, but groups
			// there are no longer the root group, so ignorable and skipped destinations must still be skipped.
			const string rtf = "{\\rtf1\\ansi a}}{\\*\\htmltag0 <x>}{\\*\\unknown y}{\\info z}b}";

			Assert.That (Convert (rtf), Is.EqualTo ("ab"));
		}

		[Test]
		public void TestAnsiCodePage ()
		{
			// RTF 1.9.1, "Character Set": \ansicpgN specifies the default code page used to decode \'hh escapes.
			Assert.That (Convert ("{\\rtf1\\ansi\\ansicpg1251 \\'cf\\'f0\\'e8\\'e2\\'e5\\'f2}"), Is.EqualTo ("Привет"));
		}

		[Test]
		public void TestFontCharset ()
		{
			// RTF 1.9.1, "Font Table": \fcharsetN determines the code page of text formatted with that font.
			const string rtf = "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fcharset0 Arial;}{\\f1\\fcharset204 Arial Cyr;}{\\f2\\fcharset161 Arial Greek;}}" +
				"\\'e9{\\f1 \\'cf\\'f0\\'e8}{\\f2 \\'e1}\\'e9}";

			Assert.That (Convert (rtf), Is.EqualTo ("éПриαé"));
		}

		[Test]
		public void TestFontCodePage ()
		{
			// RTF 1.9.1, "Font Table": \cpgN overrides the code page implied by \fcharsetN.
			const string rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0\\fcharset0\\cpg1251 Arial;}}\\f0 \\'cf}";

			Assert.That (Convert (rtf), Is.EqualTo ("П"));
		}

		[Test]
		public void TestDoubleByteCharacters ()
		{
			// Shift-JIS: 0x82 0xA0 is HIRAGANA LETTER A; the lead and trail bytes arrive as separate \'hh escapes.
			Assert.That (Convert ("{\\rtf1\\ansi\\ansicpg932 \\'82\\'a0}"), Is.EqualTo ("\u3042"));

			// 0x95 0x5C is a Shift-JIS character whose trail byte is a backslash, which writers escape as \\.
			Assert.That (Convert ("{\\rtf1\\ansi\\ansicpg932 \\'95\\\\}"), Is.EqualTo ("\u8868"));
		}

		[Test]
		public void TestRaw8BitBytes ()
		{
			// Real-world writers often emit raw 8-bit bytes; they are decoded using the current code page.
			Assert.That (Convert ("{\\rtf1\\ansi\\ansicpg1251 \u00cf}"), Is.EqualTo ("П"));
		}

		[Test]
		public void TestHiddenText ()
		{
			Assert.That (Convert ("{\\rtf1 a{\\v hidden}b\\v c\\v0 d}"), Is.EqualTo ("abd"));
		}

		[Test]
		public void TestSkippedDestinations ()
		{
			const string rtf = "{\\rtf1{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\stylesheet{\\s0 Normal;}}" +
				"{\\info{\\title Title}{\\author Author}}{\\*\\unknowndest junk}{\\pict\\wmetafile8 0102030405}" +
				"{\\object{\\*\\objdata 0102}}{\\header Header}{\\footnote Footnote}a{\\*\\bkmkstart b1}{\\*\\bkmkend b1}b}";

			Assert.That (Convert (rtf), Is.EqualTo ("ab"));
		}

		[Test]
		public void TestUnknownControlWordsAreIgnored ()
		{
			// RTF 1.9.1, "Conventions of an RTF Reader": unknown control words are ignored, but the text is kept.
			Assert.That (Convert ("{\\rtf1 a\\unknownword123 b{\\anotherunknown c}}"), Is.EqualTo ("abc"));
		}

		[Test]
		public void TestBinaryData ()
		{
			// RTF 1.9.1, "\binN": the binary data must be skipped even if it contains RTF syntax.
			Assert.That (Convert ("{\\rtf1 a{\\pict\\bin3 {{{}b}"), Is.EqualTo ("ab"));
		}

		[Test]
		public void TestHyperlinkField ()
		{
			// RTF 1.9.1, "Fields": only the field result is displayed.
			const string rtf = "{\\rtf1 see {\\field{\\*\\fldinst{HYPERLINK \"http://www.example.com/\"}}{\\fldrslt{link}}} now}";

			Assert.That (Convert (rtf), Is.EqualTo ("see link now"));
		}

		[Test]
		public void TestTable ()
		{
			const string rtf = "{\\rtf1\\trowd\\cellx1000\\cellx2000\\pard\\intbl a\\cell b\\cell\\row\\trowd\\cellx1000\\cellx2000\\pard\\intbl c\\cell d\\cell\\row\\pard after\\par}";

			Assert.That (Convert (rtf), Is.EqualTo ("a\tb" + NewLine + "c\td" + NewLine + "after" + NewLine));
		}

		[Test]
		public void TestEncapsulatedHtmlRendersRtf ()
		{
			// RtfToText deliberately renders the RTF portion of encapsulated HTML rather than the HTML markup.
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag1 <html>}{\\*\\htmltag <b>}\\htmlrtf {\\b \\htmlrtf0 bold\\htmlrtf }\\htmlrtf0 {\\*\\htmltag </b>}\\htmlrtf \\par\\htmlrtf0 }";

			Assert.That (Convert (rtf), Is.EqualTo ("bold" + NewLine));
		}

		[Test]
		public void TestStrayCloseBraces ()
		{
			Assert.That (Convert ("}}}{\\rtf1 a}}}}b"), Does.StartWith ("a"));
		}

		[Test]
		public void TestUnterminatedInput ()
		{
			Assert.That (Convert ("{\\rtf1 abc{\\b def\\'"), Is.EqualTo ("abcdef"));
			Assert.That (Convert ("{\\rtf1 abc{\\b def\\par"), Is.EqualTo ("abcdef" + NewLine));

			// a lone hex digit at the end of the input is treated leniently, like any other single-digit \'h escape
			Assert.That (Convert ("{\\rtf1 abc\\'4"), Is.EqualTo ("abc\u0004"));
		}

		[Test]
		public void TestDeepNestingIsFolded ()
		{
			// Groups that do not change the formatting state share a single stack entry, so a million nested
			// groups neither exhausts memory nor hits MaxGroupDepth.
			const int depth = 1000000;
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1 a");
			builder.Append ('{', depth);
			builder.Append ("deep");
			builder.Append ('}', depth);
			builder.Append ("b}");

			var stopwatch = Stopwatch.StartNew ();
			var text = Convert (builder.ToString (), c => c.MaxGroupDepth = 8);
			stopwatch.Stop ();

			Assert.That (text, Is.EqualTo ("adeepb"));
			Assert.That (stopwatch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (30)));
		}

		static string CreateAlternatingNesting (int depth, string content)
		{
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1 a");
			for (int i = 0; i < depth; i++)
				builder.Append ((i & 1) == 0 ? "{\\b " : "{\\b0 ");
			builder.Append (content);
			builder.Append ('}', depth);
			builder.Append ("b}");

			return builder.ToString ();
		}

		[Test]
		public void TestMaxGroupDepth ()
		{
			// Groups that each change the formatting state cannot be folded; once there are more than
			// MaxGroupDepth distinct states, the content of the deeper groups is discarded.
			var rtf = CreateAlternatingNesting (100, "deep");

			Assert.That (Convert (rtf), Is.EqualTo ("adeepb"));
			Assert.That (Convert (rtf, c => c.MaxGroupDepth = 8), Is.EqualTo ("ab"));
		}

		[Test]
		public void TestLargeFontTable ()
		{
			var builder = new StringBuilder ();

			builder.Append ("{\\rtf1\\ansi{\\fonttbl");
			for (int i = 0; i < 100000; i++)
				builder.Append ("{\\f").Append (i).Append ("\\fcharset204 Font;}");
			builder.Append ("}\\f99999 \\'cf\\f1 \\'cf}");

			// font 99999 was dropped so its text is decoded using the document code page, while font 1 is still known
			Assert.That (Convert (builder.ToString (), c => c.MaxFontTableEntries = 10), Is.EqualTo ("ÏП"));
		}

		static void WriteIndex (int index, TextWriter writer)
		{
			writer.Write ('[');
			writer.Write (index);
			writer.Write (']');
		}

		static int CountObjectPlaceholders (string rtf)
		{
			using var reader = new StringReader (rtf);

			return new RtfToText ().CountObjectPlaceholders (reader, CancellationToken.None);
		}

		[Test]
		public void TestObjectPlaceholders ()
		{
			// [MS-OXRTFEX] 2.2.3.4: each \objattph marks the position of the next attachment. The placeholder
			// character that follows it (written as \'20 or as a literal space) is replaced.
			const string rtf = "{\\rtf1 A\\objattph\\'20 B\\objattph  C}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteIndex), Is.EqualTo ("A[0] B[1]C"));
			Assert.That (Convert (rtf), Is.EqualTo ("A BC"));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (2));
		}

		[TestCase ("{\\rtf1 A\\objattph\\'41B}", "A[0]AB")]
		[TestCase ("{\\rtf1 A\\objattph xB}", "A[0]xB")]
		[TestCase ("{\\rtf1 A\\objattph\\par B}", "A[0]\r\nB")]
		[TestCase ("{\\rtf1 A\\objattph}", "A[0]")]
		public void TestObjectPlaceholderCharacterIsOptional (string rtf, string expected)
		{
			// Only a space that immediately follows \objattph is consumed; other content is never lost.
			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteIndex), Is.EqualTo (expected.Replace ("\r\n", NewLine)));
		}

		[Test]
		public void TestObjectPlaceholdersThatAreNotRendered ()
		{
			// Placeholders in skipped destinations or in hidden text are not part of the rendered document, so
			// they are neither reported nor counted.
			const string rtf = "{\\rtf1 A{\\*\\unknown \\objattph}{\\v \\objattph}{\\pict \\objattph}{\\fonttbl \\objattph}" +
				"{\\*\\htmltag \\objattph}B\\objattph\\'20 C}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteIndex), Is.EqualTo ("AB[0] C"));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (1));
		}

		[Test]
		public void TestObjectPlaceholderInTableCell ()
		{
			const string rtf = "{\\rtf1\\trowd\\cellx1000\\cellx2000\\pard\\intbl a\\cell\\objattph\\'20\\cell\\row}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteIndex), Is.EqualTo ("a\t[0]" + NewLine));
		}

		[Test]
		public void TestObjectPlaceholdersInEncapsulatedHtml ()
		{
			// RtfToText always renders the RTF, so the placeholders are reported even if the RTF encapsulates HTML.
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag <p>}A\\objattph\\'20 B{\\*\\htmltag </p>}}";

			Assert.That (Convert (rtf, c => c.ObjectPlaceholderCallback = WriteIndex), Is.EqualTo ("A[0] B"));
			Assert.That (CountObjectPlaceholders (rtf), Is.EqualTo (1));
		}

		[Test]
		public void TestManyObjectPlaceholders ()
		{
			var builder = new StringBuilder ("{\\rtf1 ");
			int last = -1, count = 0;

			for (int i = 0; i < 100000; i++)
				builder.Append ("\\objattph\\'20");
			builder.Append ('}');

			var text = Convert (builder.ToString (), c => c.ObjectPlaceholderCallback = (index, writer) => {
				Assert.That (index, Is.EqualTo (last + 1));
				last = index;
				count++;
			});

			Assert.That (text, Is.Empty);
			Assert.That (count, Is.EqualTo (100000));
			Assert.That (CountObjectPlaceholders (builder.ToString ()), Is.EqualTo (100000));
		}
	}
}
