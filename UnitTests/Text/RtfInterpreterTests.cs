//
// RtfInterpreterTests.cs
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

namespace UnitTests.Text {
	[TestFixture]
	public class RtfInterpreterTests
	{
		sealed class RecordingHandler : RtfContentHandler
		{
			public readonly StringBuilder Output = new StringBuilder ();
			public Action<RtfInterpreter> TextCallback;
			public int BeginCount;
			bool lastWasTag;

			public override void OnBegin (RtfInterpreter rtf)
			{
				BeginCount++;
				lastWasTag = false;
				Output.Append (rtf.FromHtml ? "[begin:html]" : "[begin]");
			}

			public override void OnText (RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				TextCallback?.Invoke (rtf);
				lastWasTag = false;
				Output.Append (buffer, index, count);
			}

			public override void OnParagraph (RtfInterpreter rtf)
			{
				lastWasTag = false;
				Output.Append ("[par]");
			}

			public override void OnLineBreak (RtfInterpreter rtf)
			{
				lastWasTag = false;
				Output.Append ("[line]");
			}

			public override void OnCell (RtfInterpreter rtf)
			{
				lastWasTag = false;
				Output.Append ("[cell]");
			}

			public override void OnRow (RtfInterpreter rtf)
			{
				lastWasTag = false;
				Output.Append ("[row]");
			}

			public override void OnHtmlTag (RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				// htmltag content may be reported in several chunks, so merge adjacent chunks.
				if (lastWasTag)
					Output.Length -= 2;
				else
					Output.Append ("<<");

				Output.Append (buffer, index, count).Append (">>");
				lastWasTag = true;
			}
		}

		static RtfInterpreter Create (string rtf, RecordingHandler handler, bool extractHtml = false, int maxGroupDepth = 4096, int maxFonts = 4096, int maxColors = 4096)
		{
			return new RtfInterpreter (new StringReader (rtf), handler, maxGroupDepth, maxFonts, maxColors) {
				ExtractHtml = extractHtml
			};
		}

		static string Run (string rtf, bool extractHtml = false)
		{
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler, extractHtml);

			while (interpreter.Step ())
				;

			Assert.That (handler.BeginCount, Is.EqualTo (1), "OnBegin should be called exactly once");

			return handler.Output.ToString ();
		}

		static async Task<string> RunAsync (string rtf, bool extractHtml = false)
		{
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler, extractHtml);

			while (await interpreter.StepAsync ())
				;

			Assert.That (handler.BeginCount, Is.EqualTo (1), "OnBegin should be called exactly once");

			return handler.Output.ToString ();
		}

		const string EncapsulatedHtml = "{\\rtf1\\ansi\\ansicpg1252\\fromhtml1 \\deff0{\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fcharset204 Arial Cyr;}}\r\n" +
			"{\\*\\htmltag19 <html>}{\\*\\htmltag34 <head>}{\\*\\htmltag41 </head>}{\\*\\htmltag50 <body>}\\htmlrtf {\\htmlrtf0 \r\n" +
			"{\\*\\htmltag64 <p class=\"x\\'e9\">}\\htmlrtf {\\htmlrtf0 caf\\'e9 {\\f1 \\'cf}\\htmlrtf\\par\\htmlrtf0 \r\n" +
			"{\\*\\htmltag72 </p>}\\htmlrtf }\\htmlrtf0 {\\*\\htmltag a\\_b\\par\\tab c}\r\n" +
			"\\htmlrtf }\\htmlrtf0 {\\*\\htmltag58 </body>}{\\*\\htmltag27 </html>}}";

		[Test]
		public void TestExtractHtml ()
		{
			// [MS-OXRTFEX] 2.2.3.2: htmltag content is decoded with the document code page, text outside of htmltag
			// with the current font's code page, \htmlrtf content is suppressed and \par/\tab become CRLF/TAB.
			var expected = "[begin:html]<<<html><head></head><body><p class=\"xé\">>>café П<<</p>a\u00ADb\r\n\tc</body></html>>>";

			Assert.That (Run (EncapsulatedHtml, true), Is.EqualTo (expected));
		}

		[Test]
		public async Task TestExtractHtmlAsync ()
		{
			var expected = "[begin:html]<<<html><head></head><body><p class=\"xé\">>>café П<<</p>a\u00ADb\r\n\tc</body></html>>>";

			Assert.That (await RunAsync (EncapsulatedHtml, true), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncapsulatedHtmlWithoutExtraction ()
		{
			Assert.That (Run (EncapsulatedHtml, false), Is.EqualTo ("[begin:html]café П[par]"));
		}

		[Test]
		public void TestHtmlRtfIsGroupScoped ()
		{
			// [MS-OXRTFEX] 2.1.3.1.3: the \htmlrtf state is restored at the end of the group.
			const string rtf = "{\\rtf1\\fromhtml1 a{\\htmlrtf b}c}";

			Assert.That (Run (rtf, true), Is.EqualTo ("[begin:html]ac"));
		}

		[Test]
		public void TestFromHtmlRecognitionWindow ()
		{
			// [MS-OXRTFEX] 2.2.3.1: \fromhtml must appear within the first 10 tokens, which must be '{' or control words.
			Assert.That (Run ("{\\rtf1 text\\fromhtml1 {\\*\\htmltag <b>}x}", true), Is.EqualTo ("[begin]textx"));

			var builder = new StringBuilder ("{\\rtf1");
			for (int i = 0; i < 10; i++)
				builder.Append ("\\ansi");
			builder.Append ("\\fromhtml1 {\\*\\htmltag <b>}x}");

			Assert.That (Run (builder.ToString (), true), Is.EqualTo ("[begin]x"));
		}

		[Test]
		public void TestBeginBeforeFirstContent ()
		{
			Assert.That (Run ("{\\rtf1 a}"), Is.EqualTo ("[begin]a"));
			Assert.That (Run (string.Empty), Is.EqualTo ("[begin]"));
			Assert.That (Run ("{\\rtf1\\ansi\\deff0"), Is.EqualTo ("[begin]"));
		}

		[Test]
		public void TestCellsAndRows ()
		{
			Assert.That (Run ("{\\rtf1\\trowd\\cellx100\\intbl a\\cell b\\cell\\row x\\line y\\par}"), Is.EqualTo ("[begin]a[cell]b[cell][row]x[line]y[par]"));
		}

		[Test]
		public void TestFormattingState ()
		{
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1{\\colortbl;\\red255\\green0\\blue0;\\red0\\green0\\blue255;}{\\b\\i\\ul\\strike\\cf1\\cb2\\fs30\\qc\\super x}", handler);
			RtfGroupState state = default;
			int foreground = 0, background = 0;

			handler.TextCallback = rtf => {
				state = rtf.State;
				foreground = rtf.GetColor (state.ForegroundColor);
				background = rtf.GetColor (state.BackgroundColor);
			};

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]x"));
			Assert.That (state.Bold, Is.True, "Bold");
			Assert.That (state.Italic, Is.True, "Italic");
			Assert.That (state.Underline, Is.True, "Underline");
			Assert.That (state.Strike, Is.True, "Strike");
			Assert.That (state.FontSize, Is.EqualTo (30), "FontSize");
			Assert.That (state.Alignment, Is.EqualTo (RtfAlignment.Center), "Alignment");
			Assert.That (state.VerticalAlignment, Is.EqualTo (RtfVerticalAlignment.Superscript), "VerticalAlignment");
			Assert.That (foreground, Is.EqualTo (0xFF0000), "ForegroundColor");
			Assert.That (background, Is.EqualTo (0x0000FF), "BackgroundColor");
			Assert.That (interpreter.GetColor (-1), Is.EqualTo (-1));
			Assert.That (interpreter.GetColor (100), Is.EqualTo (-1));
			Assert.That (interpreter.State.Bold, Is.False, "Bold should be restored at the end of the group");
		}

		[Test]
		public void TestHyperlinkFieldState ()
		{
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"http://www.example.com/\" }{\\fldrslt link}} after}", handler);
			var links = new List<string> ();

			handler.TextCallback = rtf => links.Add (rtf.State.Hyperlink);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]link after"));
			Assert.That (links, Is.EqualTo (new [] { "http://www.example.com/", null }));
		}

		[Test]
		public void TestNestedHyperlinkFieldState ()
		{
			// RTF 1.9.1, "Fields": a field may be nested within another field's result. The link target is scoped
			// to the \fldrslt group, so text after the nested field belongs to the outer link again.
			const string rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"http://a/\"}{\\fldrslt a{\\field{\\*\\fldinst HYPERLINK \"http://b/\"}{\\fldrslt b}}{\\field{\\*\\fldinst PAGE}{\\fldrslt 1}}c}}d}";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler);
			var links = new List<string> ();

			handler.TextCallback = r => links.Add (r.State.Hyperlink);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]ab1cd"));
			Assert.That (links, Is.EqualTo (new [] { "http://a/", "http://b/", null, "http://a/", null }));
		}

		[TestCase ("HYPERLINK \"http://www.example.com/\"", "http://www.example.com/")]
		[TestCase ("  HYPERLINK  http://www.example.com/ ", "http://www.example.com/")]
		[TestCase ("HYPERLINK \\l \"bookmark\"", null)]
		[TestCase ("HYPERLINK \"mailto:user@example.com\" \\o \"tooltip\"", "mailto:user@example.com")]
		[TestCase ("PAGE", null)]
		[TestCase ("HYPERLINK", null)]
		public void TestParseHyperlink (string instruction, string expected)
		{
			Assert.That (RtfInterpreter.ParseHyperlink (new StringBuilder (instruction)), Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldedStack ()
		{
			const int depth = 1000000;
			var rtf = "{\\rtf1 " + new string ('{', depth) + "x";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler, maxGroupDepth: 4);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]x"));
			Assert.That (interpreter.Depth, Is.EqualTo (depth + 1));
			Assert.That (interpreter.StackEntries, Is.LessThanOrEqualTo (2));
		}

		[Test]
		public async Task TestFoldedStackAsync ()
		{
			const int depth = 1000000;
			var rtf = "{\\rtf1 " + new string ('{', depth) + "x";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler, maxGroupDepth: 4);

			while (await interpreter.StepAsync ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]x"));
			Assert.That (interpreter.Depth, Is.EqualTo (depth + 1));
			Assert.That (interpreter.StackEntries, Is.LessThanOrEqualTo (2));
		}

		[Test]
		public void TestStackEntriesAreBounded ()
		{
			var builder = new StringBuilder ("{\\rtf1 ");
			for (int i = 0; i < 100000; i++)
				builder.Append ((i & 1) == 0 ? "{\\b " : "{\\b0 ");
			builder.Append ('x');

			var handler = new RecordingHandler ();
			var interpreter = Create (builder.ToString (), handler, maxGroupDepth: 16);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.StackEntries, Is.LessThanOrEqualTo (16));
			Assert.That (interpreter.Depth, Is.EqualTo (100001));
			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]"));
		}

		[Test]
		public void TestFontTableLimit ()
		{
			var builder = new StringBuilder ("{\\rtf1{\\fonttbl");
			for (int i = 0; i < 100000; i++)
				builder.Append ("{\\f").Append (i).Append ("\\fcharset204 F;}");
			builder.Append ("}}");

			var handler = new RecordingHandler ();
			var interpreter = Create (builder.ToString (), handler, maxFonts: 10);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.FontCount, Is.EqualTo (10));

			interpreter = Create (builder.ToString (), new RecordingHandler ());

			while (interpreter.Step ())
				;

			Assert.That (interpreter.FontCount, Is.EqualTo (4096));
		}

		[Test]
		public void TestFontTableWithoutGroups ()
		{
			// RTF 1.9.1, "Font Table": font entries are not required to be enclosed in their own groups.
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1{\\fonttbl\\f0\\fcharset0 A;\\f1\\fcharset204 B;}\\f1 \\'cf}", handler);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.FontCount, Is.EqualTo (2));
			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]П"));
		}

		[Test]
		public void TestColorTableLimit ()
		{
			var builder = new StringBuilder ("{\\rtf1{\\colortbl;");
			for (int i = 0; i < 100000; i++)
				builder.Append ("\\red1\\green2\\blue3;");
			builder.Append ("}}");

			var interpreter = Create (builder.ToString (), new RecordingHandler (), maxColors: 10);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.ColorCount, Is.EqualTo (10));
			Assert.That (interpreter.GetColor (0), Is.EqualTo (-1), "auto");
			Assert.That (interpreter.GetColor (1), Is.EqualTo (0x010203));

			interpreter = Create (builder.ToString (), new RecordingHandler ());

			while (interpreter.Step ())
				;

			Assert.That (interpreter.ColorCount, Is.EqualTo (4096));
		}

		[Test]
		public void TestHugeFieldInstruction ()
		{
			var rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"http://" + new string ('a', 1000000) + "\"}{\\fldrslt x}}}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]x"));
		}

		// RTF 1.9.1, "Font Table": \fcharsetN identifies the character set (and therefore the code page) of a font.
		[TestCase (254, "\\'9b", "\u00A2")] // PC-437
		[TestCase (255, "\\'9b", "\u00F8")] // PC-850 (OEM)
		[TestCase (77, "\\'8e", "\u00E9")] // Mac Roman
		[TestCase (130, "abc", "abc")] // Johab (code page 1361)
		[TestCase (999, "\\'e9", "\u00E9")] // unknown charset: the document code page
		public void TestFontCharsets (int charset, string content, string expected)
		{
			var rtf = "{\\rtf1{\\fonttbl{\\f0\\fcharset" + charset + " A;}}\\f0 " + content + "}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]" + expected));
		}

		// RTF 1.9.1, "Character Set": \ansi, \mac, \pc and \pca select the document's character set.
		[TestCase ("\\ansi", "\\'80", "\u20AC")]
		[TestCase ("\\mac", "\\'8e", "\u00E9")]
		[TestCase ("\\pc", "\\'9b", "\u00A2")]
		[TestCase ("\\pca", "\\'9b", "\u00F8")]
		[TestCase ("\\ansicpg1251", "\\'cf", "\u041F")]
		[TestCase ("\\ansicpg0", "\\'e9", "\u00E9")] // ignored
		[TestCase ("\\ansicpg-5", "\\'e9", "\u00E9")] // ignored
		[TestCase ("\\ansicpg99999", "\\'e9", "\u00E9")] // unknown code page: the fallback encoding is used
		[TestCase ("\\ansicpg50220", "\\'1b$B0!\\'1b(B", "\u4E9C")] // a stateful (ISO-2022-JP) code page
		public void TestDocumentCharacterSets (string charset, string content, string expected)
		{
			Assert.That (Run ("{\\rtf1" + charset + " " + content + "}"), Is.EqualTo ("[begin]" + expected));
		}

		[Test]
		public void TestDecoderCacheIsBounded ()
		{
			// Every font may name a different code page; a hostile document must not be able to make us create an
			// unbounded number of decoders (or repeatedly throw and catch for unknown code pages).
			var builder = new StringBuilder ("{\\rtf1{\\fonttbl");
			for (int i = 0; i < 100; i++)
				builder.Append ("{\\f").Append (i).Append ("\\cpg").Append (70000 + i).Append (" F;}");
			builder.Append ('}');
			for (int i = 0; i < 100; i++)
				builder.Append ("\\f").Append (i).Append ("\\'e9");
			builder.Append ('}');

			Assert.That (Run (builder.ToString ()), Is.EqualTo ("[begin]" + new string ('\u00E9', 100)));
		}

		[Test]
		public void TestManyBytesInOneCodePage ()
		{
			// More escaped bytes than fit in the byte buffer, including a DBCS character split across the boundary.
			var builder = new StringBuilder ("{\\rtf1{\\fonttbl{\\f0\\fcharset128 MS Gothic;}}\\f0 ");
			builder.Append ('a');
			for (int i = 0; i < 2000; i++)
				builder.Append ("\\'82\\'a0"); // Shift-JIS HIRAGANA LETTER A
			builder.Append ('}');

			Assert.That (Run (builder.ToString ()), Is.EqualTo ("[begin]a" + new string ('\u3042', 2000)));
		}

		[Test]
		public void TestNonLatin1InputCharacters ()
		{
			// When the caller decodes the RTF with an encoding other than Latin-1, text may already contain
			// characters above U+00FF; those must be passed through rather than truncated to a byte.
			Assert.That (Run ("{\\rtf1 a\u4E9Cb\u0001c}"), Is.EqualTo ("[begin]a\u4E9Cbc"));
		}

		// RTF 1.9.1, "Unicode RTF": the N (\ucN) fallback "characters" following \uM are skipped. A control word, a
		// control symbol or a \'hh escape each count as a single character, and a group boundary ends the run.
		[TestCase ("\\uc3\\u8364\\'80\\{\\b x", "\u20ACx")]
		[TestCase ("\\uc5\\u8364{a}bc", "\u20ACabc")]
		[TestCase ("\\uc2147483647\\u65 abc{x}y", "Axy")]
		[TestCase ("\\uc-5\\u65 B", "AB")]
		[TestCase ("\\uc0\\u65 B", "AB")]
		[TestCase ("\\u65\\u66 xy", "Axy")]
		[TestCase ("\\u-3913?", "\uF0B7")]
		[TestCase ("\\u70000?", "\uFFFD")]
		[TestCase ("{\\uc2 \\u65 xy}z", "Az")]
		public void TestUnicodeSkip (string content, string expected)
		{
			Assert.That (Run ("{\\rtf1 " + content + "}"), Is.EqualTo ("[begin]" + expected));
		}

		[Test]
		public void TestUnicodePairDestination ()
		{
			// RTF 1.9.1, "Unicode RTF": {\upr{ansi version}{\*\ud{unicode version}}}. Readers that understand \ud
			// ignore the ANSI version, including any escapes and symbols within it.
			const string rtf = "{\\rtf1 a{\\upr{\\'e9\\~\\{x\\u66 y}{\\*\\ud{\\'e9\\u66 y}}}b}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]a\u00E9Bb"));
		}

		[Test]
		public void TestSpecialControlSymbols ()
		{
			// RTF 1.9.1, "Special Characters".
			const string rtf = "{\\rtf1 a\\~b\\_c\\-d\\\te\\:f\\|g\\\nh}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]a\u00A0b\u2011cd\tefg[par]h"));
		}

		[Test]
		public void TestSpecialControlWords ()
		{
			const string rtf = "{\\rtf1\\emdash\\endash\\bullet\\lquote\\rquote\\ldblquote\\rdblquote\\emspace\\enspace\\qmspace\\zwj\\zwnj\\ltrmark\\rtlmark\\tab}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]\u2014\u2013\u2022\u2018\u2019\u201C\u201D\u2003\u2002\u2005\u200D\u200C\u200E\u200F\t"));
		}

		[Test]
		public void TestFontTableNestedDestinations ()
		{
			// RTF 1.9.1, "Font Table": an entry may contain nested destinations such as {\*\panose ...}, {\*\falt ...}
			// or {\*\fname ...}. These are skipped, any ';' inside them does not terminate the entry, and the end of
			// such a nested group does not terminate the entry either (properties may follow it).
			const string rtf = "{\\rtf1{\\fonttbl{\\f0{\\*\\panose 02020603050405020304}\\fcharset204{\\*\\falt Foo;}{\\pict x;}{\\*\\unknown y;}Cyr;}" +
				"{\\f1{\\fcharset204}A;}}\\f0\\'cf\\f1\\'cf}";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.FontCount, Is.EqualTo (2));
			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]\u041F\u041F"));
		}

		[Test]
		public void TestFontTableEntryTerminatedByGroupEnd ()
		{
			const string rtf = "{\\rtf1{\\fonttbl{\\f0\\fcharset204 A}{\\f1\\fcharset161 B}}\\f0\\'cf\\f1\\'e1}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]\u041F\u03B1"));
		}

		[Test]
		public void TestFontTableUpdatesAfterLimit ()
		{
			// Once the font table is full, existing entries may still be redefined since that does not grow the table.
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1{\\fonttbl{\\f0 A;}{\\f1 B;}{\\f0\\fcharset204 C;}}\\f0\\'cf}", handler, maxFonts: 1);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.FontCount, Is.EqualTo (1));
			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]\u041F"));
		}

		[Test]
		public void TestColorTableNestedDestinations ()
		{
			// RTF 1.9.1, "Color Table": component values are clamped to 0-255 and ignorable destinations are skipped.
			const string rtf = "{\\rtf1{\\colortbl;\\red300\\green-5\\blue16{\\*\\unknown ;;;};\\cfoo\\red1;}x}";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler);

			while (interpreter.Step ())
				;

			Assert.That (interpreter.ColorCount, Is.EqualTo (3));
			Assert.That (interpreter.GetColor (0), Is.EqualTo (-1));
			Assert.That (interpreter.GetColor (1), Is.EqualTo (0xFF0010));
			Assert.That (interpreter.GetColor (2), Is.EqualTo (0x010000));
			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]x"));
		}

		[Test]
		public void TestTextInTablesIsNotRendered ()
		{
			Assert.That (Run ("{\\rtf1{\\fonttbl\\'e9\\~\\{{\\f0 A;}}{\\colortbl\\'e9\\~;}x}"), Is.EqualTo ("[begin]x"));
		}

		[Test]
		public void TestNestedFieldInstructions ()
		{
			// The instruction is only complete once the outermost \fldinst group ends.
			const string rtf = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK {\\b \"http://example.com/\"}}{\\fldrslt link}}}";
			var handler = new RecordingHandler ();
			var interpreter = Create (rtf, handler);
			string link = null;

			handler.TextCallback = r => link = r.State.Hyperlink;

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]link"));
			Assert.That (link, Is.EqualTo ("http://example.com/"));
		}

		[TestCase ("HYPERLINK \\o \"tip\" \"http://example.com/\"", "http://example.com/")]
		[TestCase ("HYPERLINK \\o tip http://example.com/", "http://example.com/")]
		[TestCase ("HYPERLINK \\t \"_blank\" \"https://example.com/\"", "https://example.com/")]
		[TestCase ("HYPERLINK \\n \"https://example.com/\"", "https://example.com/")]
		[TestCase ("HYPERLINK \\m \"https://example.com/\"", "https://example.com/")]
		[TestCase ("HYPERLINK \\o \"unterminated", null)]
		[TestCase ("HYPERLINK \\o", null)]
		[TestCase ("HYPERLINK \"http://example.com/", "http://example.com/")]
		[TestCase ("HYPERLINK \" http://example.com/ \"", "http://example.com/")]
		[TestCase ("HYPERLINK \"java\tscript:alert(1)\"", null)]
		[TestCase ("HYPERLINK \"http://exa\u0001mple.com/\"", null)]
		[TestCase ("HYPERLINK \"\"", null)]
		[TestCase ("HYPERLINK \":foo\"", null)]
		[TestCase ("HYPERLINK \"example.com\"", null)]
		[TestCase ("HYPERLINK \"FTP://example.com/\"", "FTP://example.com/")]
		[TestCase ("HYPERLINK \"tel:+15555551234\"", "tel:+15555551234")]
		[TestCase ("hyperlink \"https://example.com/\"", "https://example.com/")]
		[TestCase ("HYPERLINKX", null)]
		[TestCase ("HYPER", null)]
		[TestCase ("", null)]
		public void TestParseHyperlinkEdgeCases (string instruction, string expected)
		{
			Assert.That (RtfInterpreter.ParseHyperlink (new StringBuilder (instruction)), Is.EqualTo (expected));
		}

		[Test]
		public void TestGroupStateEquality ()
		{
			var a = new RtfGroupState { Font = 1, FontSize = 24 };
			var b = new RtfGroupState { Font = 1, FontSize = 24 };

			Assert.That (a.Equals ((object) b), Is.True);
			Assert.That (a.Equals ((object) "x"), Is.False);
			Assert.That (a.GetHashCode (), Is.EqualTo (b.GetHashCode ()));

			b.UnicodeSkip = 2;
			Assert.That (a.Equals (b), Is.False);

			b = a;
			b.SetFlag (RtfStateFlags.Bold, true);
			Assert.That (a.Equals (b), Is.False);
			b.SetFlag (RtfStateFlags.Bold, false);
			Assert.That (a.Equals (b), Is.True);
		}

		sealed class NullHandler : RtfContentHandler
		{
		}

		[Test]
		public void TestDefaultContentHandler ()
		{
			// The base handler ignores everything.
			const string rtf = "{\\rtf1\\fromhtml1 a\\par b\\line c\\cell d\\row{\\*\\htmltag <b>}}";

			foreach (var extract in new [] { false, true }) {
				var interpreter = new RtfInterpreter (new StringReader (rtf), new NullHandler (), 4096, 4096, 4096) {
					ExtractHtml = extract
				};

				while (interpreter.Step ())
					;

				Assert.That (interpreter.Step (), Is.False, "Step after the end of the input");
			}
		}

		[Test]
		public async Task TestDefaultContentHandlerAsync ()
		{
			const string rtf = "{\\rtf1\\fromhtml1 a\\par b\\line c\\cell d\\row{\\*\\htmltag <b>}}";

			foreach (var extract in new [] { false, true }) {
				var interpreter = new RtfInterpreter (new StringReader (rtf), new NullHandler (), 4096, 4096, 4096) {
					ExtractHtml = extract
				};

				while (await interpreter.StepAsync ())
					;

				Assert.That (await interpreter.StepAsync (), Is.False, "Step after the end of the input");
			}
		}

		[Test]
		public void TestHtmlTagLineBreaks ()
		{
			// [MS-OXRTFEX] 2.2.3.2: \par and \line within an HTMLTAG destination become CRLF, while \cell and \row
			// have no meaning there.
			const string rtf = "{\\rtf1\\fromhtml1 {\\*\\htmltag <p>\\par x\\line y\\cell\\row}}";

			Assert.That (Run (rtf, true), Is.EqualTo ("[begin:html]<<<p>\r\nx\r\ny>>"));
		}

		[Test]
		public void TestSuppressedContent ()
		{
			// RTF 1.9.1, "Font (Character) Formatting Properties": \v is hidden text, which includes paragraph and
			// table structure produced within it.
			Assert.That (Run ("{\\rtf1 a{\\v b\\par c\\line d\\cell e\\row}f}"), Is.EqualTo ("[begin]af"));
		}

		[Test]
		public void TestCharacterFormattingReset ()
		{
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1\\deff3\\f5\\b\\i\\ul\\strike\\v\\super\\fs40\\cf1\\cb2\\plain x\\ulnone\\sub\\nosupersub\\fs y}", handler);
			var states = new List<RtfGroupState> ();

			handler.TextCallback = r => states.Add (r.State);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]xy"));
			Assert.That (states[0].Font, Is.EqualTo (3));
			Assert.That (states[0].Flags, Is.EqualTo (RtfStateFlags.None));
			Assert.That (states[0].FontSize, Is.EqualTo (RtfInterpreter.DefaultFontSize));
			Assert.That (states[0].VerticalAlignment, Is.EqualTo (RtfVerticalAlignment.Baseline));
			Assert.That (states[1].FontSize, Is.EqualTo (RtfInterpreter.DefaultFontSize));
		}

		[TestCase ("\\fs-10", 0)]
		[TestCase ("\\fs2147483647", 3276)]
		[TestCase ("\\fs", RtfInterpreter.DefaultFontSize)]
		[TestCase ("\\fs13", 13)]
		public void TestFontSizeIsClamped (string control, int expected)
		{
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1" + control + " x}", handler);
			int fontSize = -1;

			handler.TextCallback = r => fontSize = r.State.FontSize;

			while (interpreter.Step ())
				;

			Assert.That (fontSize, Is.EqualTo (expected));
		}

		[Test]
		public void TestNegativeColorIndices ()
		{
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1{\\colortbl;\\red1\\green2\\blue3;}\\cf-1\\cb-2147483648 x}", handler);
			RtfGroupState state = default;

			handler.TextCallback = r => state = r.State;

			while (interpreter.Step ())
				;

			Assert.That (state.ForegroundColor, Is.EqualTo (0));
			Assert.That (state.BackgroundColor, Is.EqualTo (0));
		}

		[Test]
		public void TestMaxGroupDepthOfOne ()
		{
			// With a single stack entry, only the root group's state is tracked: nested groups that save that same
			// state are folded into it, while any nested group that saves a different state is skipped.
			var handler = new RecordingHandler ();
			var interpreter = Create ("{\\rtf1 a{b}\\b c{\\i d}{e}f}", handler, maxGroupDepth: 1);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]abcf"));
			Assert.That (interpreter.Depth, Is.EqualTo (0));
		}

		[Test]
		public void TestSkippedGroupNestingIsCounted ()
		{
			// A skipped destination containing a huge number of nested groups is skipped using a counter only.
			const int depth = 1000000;
			var rtf = "{\\rtf1 a{\\*\\unknown " + new string ('{', depth) + new string ('}', depth) + "}b}";

			Assert.That (Run (rtf), Is.EqualTo ("[begin]ab"));
		}
	}
}
