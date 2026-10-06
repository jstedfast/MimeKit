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

			handler.TextCallback = rtf => links.Add (rtf.State.Hyperlink ? rtf.FieldHyperlink : null);

			while (interpreter.Step ())
				;

			Assert.That (handler.Output.ToString (), Is.EqualTo ("[begin]link after"));
			Assert.That (links, Is.EqualTo (new [] { "http://www.example.com/", null }));
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
	}
}
