//
// RtfConverterBenchmarks.cs
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

using System;
using System.IO;
using System.Text;

using MimeKit.Text;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.Text {
	// The documents are generated rather than loaded from the RTF test corpus because the corpus documents are all
	// small (a few KB). Each document is roughly 1 MB so that per-document overhead does not dominate.
	[MemoryDiagnoser]
	public class RtfConverterBenchmarks
	{
		const string Sentence = "The quick brown fox jumps over the lazy dog. ";
		const int TargetSize = 1024 * 1024;

		internal static readonly string WordDocument, EncapsulatedHtml, ExtractedHtml, UnicodeDocument;
		internal static readonly string DeepNesting, DeepFormattedNesting, LargeFontTable, LargeBinary;

		static RtfConverterBenchmarks ()
		{
			WordDocument = CreateWordDocument ();
			EncapsulatedHtml = CreateEncapsulatedHtml (out ExtractedHtml);
			UnicodeDocument = CreateUnicodeDocument ();
			DeepNesting = CreateDeepNesting ();
			DeepFormattedNesting = CreateDeepFormattedNesting ();
			LargeFontTable = CreateLargeFontTable ();
			LargeBinary = CreateLargeBinary ();
		}

		const string WordHeader = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033" +
			"{\\fonttbl{\\f0\\froman\\fprq2\\fcharset0 Times New Roman;}{\\f1\\fswiss\\fprq2\\fcharset0 Arial;}" +
			"{\\f2\\fmodern\\fprq1\\fcharset0 Courier New;}{\\f3\\fswiss\\fprq2{\\*\\panose 020b0604020202020204}\\fcharset204 Arial Cyr;}}" +
			"{\\colortbl ;\\red255\\green0\\blue0;\\red0\\green0\\blue255;\\red0\\green128\\blue0;}" +
			"{\\stylesheet{ Normal;}{\\s1 heading 1;}}" +
			"{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1\r\n";

		// A document similar to what Word or WordPad produces: paragraphs with mixed character formatting, a field,
		// \'XX escapes, \uN characters and a table.
		static string CreateWordDocument ()
		{
			var builder = new StringBuilder (TargetSize + 4096);
			int n = 0;

			builder.Append (WordHeader);

			while (builder.Length < TargetSize) {
				builder.Append ("\\pard\\sa200\\sl276\\slmult1\\f1\\fs22 ");
				builder.Append (Sentence).Append ("{\\b bold ").Append (n).Append ("} and {\\i italic} and ");
				builder.Append ("{\\cf1\\ul colored underline}. Caf\\'e9 na\\'efve \\u8364? r\\'e9sum\\'e9. ");
				builder.Append ("{\\f3\\'cf\\'f0\\'e8\\'e2\\'e5\\'f2} ");
				builder.Append (Sentence).Append (Sentence).Append ("\\par\r\n");

				if ((n % 10) == 0) {
					builder.Append ("{\\field{\\*\\fldinst{HYPERLINK \"https://www.example.com/page/").Append (n).Append ("\"}}");
					builder.Append ("{\\fldrslt{\\ul\\cf2 link text}}}\\par\r\n");
				}

				if ((n % 25) == 0) {
					builder.Append ("\\trowd\\cellx3000\\cellx6000\\cellx9000\r\n");
					builder.Append ("\\pard\\intbl cell one\\cell {\\b cell two}\\cell cell three\\cell\\row\r\n");
					builder.Append ("\\pard\\intbl cell four\\cell cell five\\cell cell six\\cell\\row\r\n");
					builder.Append ("\\pard ");
				}

				n++;
			}

			builder.Append ('}');

			return builder.ToString ();
		}

		// A document similar to what Outlook and Exchange produce for HTML bodies ([MS-OXRTFEX]): the original HTML
		// is encapsulated in \*\htmltag destinations and the RTF rendering is wrapped in \htmlrtf ... \htmlrtf0.
		static string CreateEncapsulatedHtml (out string html)
		{
			var htmlBuilder = new StringBuilder (TargetSize);
			var builder = new StringBuilder (TargetSize * 2);
			int n = 0;

			builder.Append ("{\\rtf1\\ansi\\ansicpg1252\\fromhtml1 \\fbidis \\deff0{\\fonttbl{\\f0\\fswiss Arial;}" +
				"{\\f1\\fmodern Courier New;}{\\f2\\fnil\\fcharset2 Symbol;}}{\\colortbl\\red0\\green0\\blue0;\\red0\\green0\\blue255;}\r\n");
			builder.Append ("{\\*\\htmltag19 <html>}{\\*\\htmltag34 <head>}{\\*\\htmltag1 <meta charset=\"utf-8\">}" +
				"{\\*\\htmltag41 </head>}{\\*\\htmltag50 <body style=\"font-family: Arial\">}\\htmlrtf \\lang1033 \\htmlrtf0\r\n");
			htmlBuilder.Append ("<html><head><meta charset=\"utf-8\"></head><body style=\"font-family: Arial\">");

			while (builder.Length < TargetSize) {
				builder.Append ("{\\*\\htmltag64 <p class=\"MsoNormal\">}\\htmlrtf {\\htmlrtf0 ");
				builder.Append (Sentence).Append ("{\\*\\htmltag84 <b>}\\htmlrtf {\\b \\htmlrtf0 bold ").Append (n);
				builder.Append ("\\htmlrtf }\\htmlrtf0 {\\*\\htmltag92 </b>} caf\\'e9 ");
				builder.Append ("{\\*\\htmltag84 <a href=\"https://www.example.com/page/").Append (n).Append ("\">}");
				builder.Append ("\\htmlrtf {\\field{\\*\\fldinst{HYPERLINK \"https://www.example.com/\"}}{\\fldrslt\\cf1\\ul \\htmlrtf0 link");
				builder.Append ("\\htmlrtf }\\htmlrtf0 \\htmlrtf }}\\htmlrtf0 {\\*\\htmltag92 </a>} ");
				builder.Append (Sentence).Append ("\\htmlrtf\\par}\\htmlrtf0\r\n{\\*\\htmltag72 </p>}\r\n");

				htmlBuilder.Append ("<p class=\"MsoNormal\">").Append (Sentence).Append ("<b>bold ").Append (n);
				htmlBuilder.Append ("</b> caf\u00e9 <a href=\"https://www.example.com/page/").Append (n).Append ("\">link</a> ");
				htmlBuilder.Append (Sentence).Append ("</p>");

				n++;
			}

			builder.Append ("{\\*\\htmltag58 </body>}{\\*\\htmltag27 </html>}}");
			htmlBuilder.Append ("</body></html>");

			html = htmlBuilder.ToString ();

			return builder.ToString ();
		}

		// Text that is entirely non-Latin: DBCS \'XX pairs decoded via a code page, and \uN with ANSI fallbacks.
		static string CreateUnicodeDocument ()
		{
			var builder = new StringBuilder (TargetSize + 4096);

			builder.Append ("{\\rtf1\\ansi\\ansicpg932\\deff0{\\fonttbl{\\f0\\fnil\\fcharset128 MS Gothic;}{\\f1\\fnil\\fcharset0 Arial;}}\\uc1\\f0 ");

			while (builder.Length < TargetSize) {
				builder.Append ("\\'93\\'fa\\'96\\'7b\\'8c\\'ea\\'82\\'cc\\'83\\'65\\'83\\'4c\\'83\\'58\\'83\\'67 ");
				builder.Append ("{\\f1\\u1055?\\u1088?\\u1080?\\u1074?\\u1077?\\u1090? \\u20320?\\u22909?\\u19990?\\u30028?}\\par\r\n");
			}

			builder.Append ('}');

			return builder.ToString ();
		}

		// Hostile: groups nested ~512K levels deep that do not change any formatting. These are folded into a
		// single group stack entry.
		static string CreateDeepNesting ()
		{
			const int depth = TargetSize / 2;
			var builder = new StringBuilder (TargetSize + 16);

			builder.Append ("{\\rtf1 ");
			builder.Append ('{', depth);
			builder.Append ('x');
			builder.Append ('}', depth);
			builder.Append ('}');

			return builder.ToString ();
		}

		// Hostile: nested groups that each change formatting so that they cannot be folded. Nesting beyond
		// MaxGroupDepth is skipped.
		static string CreateDeepFormattedNesting ()
		{
			var builder = new StringBuilder (TargetSize + 16);
			int depth = 0;

			builder.Append ("{\\rtf1 ");

			while (builder.Length < TargetSize / 2) {
				builder.Append ((depth & 1) == 0 ? "{\\b x" : "{\\b0 y");
				depth++;
			}

			builder.Append ('}', depth);
			builder.Append ('}');

			return builder.ToString ();
		}

		// Hostile: a font table with far more entries than MaxFontTableEntries, followed by text that references
		// fonts beyond the limit.
		static string CreateLargeFontTable ()
		{
			var builder = new StringBuilder (TargetSize + 4096);
			int n = 0;

			builder.Append ("{\\rtf1\\ansi{\\fonttbl");

			while (builder.Length < TargetSize - 4096) {
				builder.Append ("{\\f").Append (n).Append ("\\fcharset").Append ((n & 1) == 0 ? "204" : "0").Append (" Font").Append (n).Append (";}");
				n++;
			}

			builder.Append ('}');

			for (int i = 0; i < n; i += n / 100)
				builder.Append ("\\f").Append (i).Append (" \\'cf\\'f0 ");

			builder.Append ('}');

			return builder.ToString ();
		}

		// Embedded binary data (\binN), which is skipped without being buffered.
		static string CreateLargeBinary ()
		{
			const int length = TargetSize - 64;
			var builder = new StringBuilder (TargetSize);

			builder.Append ("{\\rtf1 before {\\*\\blipuid x}{\\pict\\bin").Append (length).Append (' ');
			builder.Append ('\u00ff', length);
			builder.Append ("} after}");

			return builder.ToString ();
		}

		static void Convert (TextConverter converter, string text)
		{
			using var reader = new StringReader (text);

			converter.Convert (reader, TextWriter.Null);
		}

		[Benchmark]
		public void RtfToText_WordDocument ()
		{
			Convert (new RtfToText (), WordDocument);
		}

		[Benchmark]
		public void RtfToHtml_WordDocument ()
		{
			Convert (new RtfToHtml (), WordDocument);
		}

		[Benchmark]
		public void RtfToText_UnicodeDocument ()
		{
			Convert (new RtfToText (), UnicodeDocument);
		}

		[Benchmark]
		public void RtfToHtml_UnicodeDocument ()
		{
			Convert (new RtfToHtml (), UnicodeDocument);
		}

		[Benchmark]
		public void RtfToText_EncapsulatedHtml ()
		{
			Convert (new RtfToText (), EncapsulatedHtml);
		}

		[Benchmark]
		public void RtfToHtml_EncapsulatedHtml_Extract ()
		{
			Convert (new RtfToHtml (), EncapsulatedHtml);
		}

		[Benchmark]
		public void RtfToHtml_EncapsulatedHtml_Render ()
		{
			Convert (new RtfToHtml { ExtractEncapsulatedHtml = false }, EncapsulatedHtml);
		}

		// Baseline for RtfToHtml_EncapsulatedHtml_Extract: the cost of passing the extracted HTML through HtmlToHtml.
		[Benchmark]
		public void HtmlToHtml_ExtractedHtml ()
		{
			Convert (new HtmlToHtml (), ExtractedHtml);
		}

		[Benchmark]
		public void RtfToText_DeepNesting ()
		{
			Convert (new RtfToText (), DeepNesting);
		}

		[Benchmark]
		public void RtfToText_DeepFormattedNesting ()
		{
			Convert (new RtfToText (), DeepFormattedNesting);
		}

		[Benchmark]
		public void RtfToHtml_DeepFormattedNesting ()
		{
			Convert (new RtfToHtml (), DeepFormattedNesting);
		}

		[Benchmark]
		public void RtfToText_LargeFontTable ()
		{
			Convert (new RtfToText (), LargeFontTable);
		}

		[Benchmark]
		public void RtfToText_LargeBinary ()
		{
			Convert (new RtfToText (), LargeBinary);
		}
	}
}
