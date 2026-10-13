//
// RtfInterpreterComplianceTests.cs
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
	public class RtfInterpreterComplianceTests
	{
		sealed class NullHandler : RtfContentHandler
		{
		}

		// Returns at most one character per read so that every token straddles a buffer boundary.
		sealed class TrickleReader : StringReader
		{
			public TrickleReader (string text) : base (text)
			{
			}

			public override int Read (char[] buffer, int index, int count)
			{
				return base.Read (buffer, index, Math.Min (count, 1));
			}

			public override Task<int> ReadAsync (char[] buffer, int index, int count)
			{
				return Task.FromResult (Read (buffer, index, count));
			}

			public override ValueTask<int> ReadAsync (Memory<char> buffer, CancellationToken cancellationToken = default)
			{
				return new ValueTask<int> (base.Read (buffer.Span.Slice (0, Math.Min (buffer.Length, 1))));
			}
		}

		const int Limit = 4096;

		static RtfInterpreter Create (string rtf, bool trickle, IRtfComplianceLogger logger, int maxGroupDepth, int maxFonts, int maxColors)
		{
			var reader = trickle ? new TrickleReader (rtf) : new StringReader (rtf);

			return new RtfInterpreter (reader, new NullHandler (), maxGroupDepth, maxFonts, maxColors) {
				ComplianceLogger = logger
			};
		}

		static string[] Interpret (string rtf, bool trickle = false, int maxGroupDepth = Limit, int maxFonts = Limit, int maxColors = Limit)
		{
			var logger = new TestRtfComplianceLogger ();
			var interpreter = Create (rtf, trickle, logger, maxGroupDepth, maxFonts, maxColors);

			while (interpreter.Step ())
				;

			return logger.Issues.Select (x => x.ToString ()).ToArray ();
		}

		static async Task<string[]> InterpretAsync (string rtf, bool trickle = false, int maxGroupDepth = Limit, int maxFonts = Limit, int maxColors = Limit)
		{
			var logger = new TestRtfComplianceLogger ();
			var interpreter = Create (rtf, trickle, logger, maxGroupDepth, maxFonts, maxColors);

			while (await interpreter.StepAsync ())
				;

			return logger.Issues.Select (x => x.ToString ()).ToArray ();
		}

		static readonly string LongFieldInstruction = "{\\rtf1{\\field{\\*\\fldinst HYPERLINK \"" + new string ('a', 3000) + "\"}{\\fldrslt link}}{\\field{\\*\\fldinst HYPERLINK \"" + new string ('b', 3000) + "\"}{\\fldrslt link}}}";

		static readonly object[][] ViolationCases = {
			// well-formed documents
			new object[] { "{\\rtf1 hello}", Array.Empty<string> () },
			new object[] { "{\\rtf1{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red255\\green0\\blue0;}\\f0\\fs24\\cf1\\cf0\\cb1 x\\u8364?\\u-10179?\\u-8704?}", Array.Empty<string> () },
			new object[] { "{\\rtf1 x} \t\0\r\n", Array.Empty<string> () },

			// document structure
			new object[] { "", new[] { "MissingRtfHeader at line 1, position 1 (offset 0)" } },
			new object[] { "hello", new[] { "MissingRtfHeader at line 1, position 1 (offset 0)" } },
			new object[] { "{hello}", new[] { "MissingRtfHeader at line 1, position 2 (offset 1) (depth 1)" } },
			new object[] { "{", new[] { "TruncatedStream at line 1, position 2 (offset 1) (depth 1)", "MissingRtfHeader at line 1, position 2 (offset 1) (depth 1)" } },
			new object[] { "{\\rtf1 x}}", new[] { "UnbalancedGroupEnd at line 1, position 10 (offset 9)" } },
			new object[] { "{\\rtf1 x}junk{more}", new[] { "ContentAfterDocument at line 1, position 10 (offset 9)" } },
			new object[] { "{\\rtf1 x}\n\\par", new[] { "ContentAfterDocument at line 2, position 1 (offset 10)" } },
			new object[] { "{\\rtf1{\\b x}", new[] { "TruncatedStream at line 1, position 13 (offset 12) (depth 1)" } },
			new object[] { "{\\rtf1{\\pict \\bin100 abc}", new[] { "TruncatedStream at line 1, position 26 (offset 25) (depth 2)" } },

			// lexical
			new object[] { "{\\rtf1 \\abcdefghijklmnopqrstuvwxyzabcdefghij x}", new[] { "KeywordTooLong at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\li99999999999 x}", new[] { "InvalidParameter at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\li-99999999999 x}", new[] { "InvalidParameter at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\u70000?}", new[] { "InvalidParameter at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\u99999999999?}", new[] { "InvalidParameter at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\'zz}", new[] { "InvalidHexEscape at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\'4z}", new[] { "InvalidHexEscape at line 1, position 8 (offset 7) (depth 1)" } },
			new object[] { "{\\rtf1 \\'", new[] { "InvalidHexEscape at line 1, position 8 (offset 7) (depth 1)", "TruncatedStream at line 1, position 10 (offset 9) (depth 1)" } },
			new object[] { "{\\rtf1 \\'4", new[] { "InvalidHexEscape at line 1, position 8 (offset 7) (depth 1)", "TruncatedStream at line 1, position 11 (offset 10) (depth 1)" } },
			new object[] { "{\\rtf1 caf\u00e9 \u00e9}", new[] { "UnescapedEightBitCharacter at line 1, position 11 (offset 10) (depth 1)" } },
			new object[] { "{\\rtf1 caf\u00e9\\par \u00e9}", new[] { "UnescapedEightBitCharacter at line 1, position 11 (offset 10) (depth 1)", "UnescapedEightBitCharacter at line 1, position 17 (offset 16) (depth 1)" } },
			new object[] { "{\\rtf1{\\*\\generator caf\u00e9;} x}", Array.Empty<string> () },
			new object[] { "{\\rtf1 \\u-10179?}", new[] { "UnpairedSurrogate at line 1, position 17 (offset 16)" } },
			new object[] { "{\\rtf1 \\u-8704?}", new[] { "UnpairedSurrogate at line 1, position 8 (offset 7) (depth 1)" } },

			// character formatting
			new object[] { "{\\rtf1\\ansicpg99999 \\'e9 \\'e9}", new[] { "UnsupportedCodePage at line 1, position 30 (offset 29)" } },
			new object[] { "{\\rtf1{\\fonttbl{\\f0 Arial;}}\\f1 x\\f2 y}", new[] { "UndefinedFont at line 1, position 29 (offset 28) (depth 1)", "UndefinedFont at line 1, position 34 (offset 33) (depth 1)" } },
			new object[] { "{\\rtf1{\\colortbl;\\red255\\green0\\blue0;}\\cf2 x\\cb-1 y}", new[] { "UndefinedColor at line 1, position 40 (offset 39) (depth 1)", "UndefinedColor at line 1, position 46 (offset 45) (depth 1)" } },
			new object[] { "{\\rtf1 \\fs0 x\\fs-2 y\\fs3277 z\\fs3276 w}", new[] { "InvalidFontSize at line 1, position 8 (offset 7) (depth 1)", "InvalidFontSize at line 1, position 14 (offset 13) (depth 1)", "InvalidFontSize at line 1, position 21 (offset 20) (depth 1)" } },
			new object[] { LongFieldInstruction, new[] { "FieldInstructionTooLong at line 1, position 3038 (offset 3037) (depth 2)", "FieldInstructionTooLong at line 1, position 6086 (offset 6085) (depth 2)" } },

			// line tracking
			new object[] { "{\\rtf1\r\n\\par\n\\'zz}", new[] { "InvalidHexEscape at line 3, position 1 (offset 13) (depth 1)" } },
			new object[] { "{\\rtf1{\\pict\\bin5 a\n\nb\n}\\'zz}", new[] { "InvalidHexEscape at line 4, position 2 (offset 24) (depth 1)" } },
		};

		[TestCaseSource (nameof (ViolationCases))]
		public void TestViolations (string rtf, string[] expected)
		{
			Assert.That (Interpret (rtf), Is.EqualTo (expected), "buffered");
			Assert.That (Interpret (rtf, trickle: true), Is.EqualTo (expected), "trickle");
		}

		[TestCaseSource (nameof (ViolationCases))]
		public async Task TestViolationsAsync (string rtf, string[] expected)
		{
			Assert.That (await InterpretAsync (rtf), Is.EqualTo (expected), "buffered");
			Assert.That (await InterpretAsync (rtf, trickle: true), Is.EqualTo (expected), "trickle");
		}

		static readonly object[][] LimitCases = {
			new object[] { "{\\rtf1{\\fonttbl{\\f0 A;}{\\f1 B;}{\\f2 C;}}\\f2 x\\f9 y}", Limit, 1, Limit, new[] { "FontTableLimitExceeded at line 1, position 30 (offset 29) (depth 3)" } },
			new object[] { "{\\rtf1{\\colortbl;\\red1;\\red2;\\red3;}\\cf3 x\\cf9 y}", Limit, Limit, 1, new[] { "ColorTableLimitExceeded at line 1, position 23 (offset 22) (depth 2)" } },
			new object[] { "{\\rtf1\\b{\\i{\\ul x}}{\\i{\\ul y}}}", 2, Limit, Limit, new[] { "GroupDepthLimitExceeded at line 1, position 12 (offset 11) (depth 2)" } },
			new object[] { "{\\rtf1" + string.Concat (Enumerable.Repeat ("{", 10000)) + "x" + string.Concat (Enumerable.Repeat ("}", 10000)) + "}", 2, Limit, Limit, Array.Empty<string> () },
		};

		[TestCaseSource (nameof (LimitCases))]
		public void TestLimits (string rtf, int maxGroupDepth, int maxFonts, int maxColors, string[] expected)
		{
			Assert.That (Interpret (rtf, false, maxGroupDepth, maxFonts, maxColors), Is.EqualTo (expected), "buffered");
			Assert.That (Interpret (rtf, true, maxGroupDepth, maxFonts, maxColors), Is.EqualTo (expected), "trickle");
		}

		[TestCaseSource (nameof (LimitCases))]
		public async Task TestLimitsAsync (string rtf, int maxGroupDepth, int maxFonts, int maxColors, string[] expected)
		{
			Assert.That (await InterpretAsync (rtf, false, maxGroupDepth, maxFonts, maxColors), Is.EqualTo (expected), "buffered");
			Assert.That (await InterpretAsync (rtf, true, maxGroupDepth, maxFonts, maxColors), Is.EqualTo (expected), "trickle");
		}

		[Test]
		public void TestNoLoggerProducesTheSameContent ()
		{
			const string rtf = "{\\rtf1{\\fonttbl{\\f0 A;}}\\f5\\fs0\\cf9 caf\u00e9 \\'zz\\u-10179?}}junk";
			var withLogger = new RtfToText { ComplianceLogger = new TestRtfComplianceLogger () };
			var withoutLogger = new RtfToText ();

			Assert.That (withLogger.Convert (rtf), Is.EqualTo (withoutLogger.Convert (rtf)));
			Assert.That (((TestRtfComplianceLogger) withLogger.ComplianceLogger).Issues, Is.Not.Empty);
		}
	}
}