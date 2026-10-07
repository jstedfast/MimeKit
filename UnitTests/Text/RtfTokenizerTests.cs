//
// RtfTokenizerTests.cs
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
	public class RtfTokenizerTests
	{
		static void Describe (RtfTokenizer tokenizer, List<string> tokens, StringBuilder text)
		{
			// Text tokens are split at buffer boundaries, so merge adjacent text tokens before comparing.
			if (tokenizer.Kind == RtfTokenKind.Text) {
				text.Append (tokenizer.Text.ToString ());
				return;
			}

			if (text.Length > 0) {
				tokens.Add ("text:" + text);
				text.Clear ();
			}

			switch (tokenizer.Kind) {
			case RtfTokenKind.GroupStart: tokens.Add ("{"); break;
			case RtfTokenKind.GroupEnd: tokens.Add ("}"); break;
			case RtfTokenKind.ControlSymbol: tokens.Add ("symbol:" + tokenizer.Symbol); break;
			case RtfTokenKind.HexChar: tokens.Add ("hex:" + tokenizer.HexValue.ToString ("x2")); break;
			case RtfTokenKind.ControlWord:
				var word = "word:" + tokenizer.Keyword.ToString ();
				if (tokenizer.HasParameter)
					word += "=" + tokenizer.Parameter;
				if (tokenizer.IsKeywordTruncated)
					word += " (truncated)";
				word += " [" + tokenizer.KeywordId + "]";
				tokens.Add (word);
				break;
			}
		}

		static List<string> Tokenize (string rtf, int bufferSize)
		{
			var tokenizer = new RtfTokenizer (new StringReader (rtf), bufferSize);
			var tokens = new List<string> ();
			var text = new StringBuilder ();

			while (tokenizer.ReadNextToken ())
				Describe (tokenizer, tokens, text);

			if (text.Length > 0)
				tokens.Add ("text:" + text);

			return tokens;
		}

		static async Task<List<string>> TokenizeAsync (string rtf, int bufferSize)
		{
			var tokenizer = new RtfTokenizer (new StringReader (rtf), bufferSize);
			var tokens = new List<string> ();
			var text = new StringBuilder ();

			while (await tokenizer.ReadNextTokenAsync ())
				Describe (tokenizer, tokens, text);

			if (text.Length > 0)
				tokens.Add ("text:" + text);

			return tokens;
		}

		static readonly string[] BasicExpected = {
			"{",
			"word:rtf=1 [Rtf]",
			"word:ansi [Ansi]",
			"text:Hello ",
			"hex:e9",
			"symbol:\\",
			"symbol:{",
			"symbol:}",
			"word:fs=-20 [Fs]",
			"word:foo [Unknown]",
			"text:x",
			"symbol:*",
			"symbol:\n",
			"word:par [Par]",
			"}"
		};

		// RTF 1.9.1, "Control Words": a space following a control word is part of the control word, any other
		// non-letter/digit terminates it and is processed normally. Raw CR/LF in the text are ignored, while
		// a backslash followed by CR or LF is equivalent to \par.
		const string BasicRtf = "{\\rtf1\\ansi Hello\r\n \\'E9\\\\\\{\\}\\fs-20\\foo x\\*\\\n\\par}";

		[TestCase (1)]
		[TestCase (3)]
		[TestCase (4096)]
		public void TestBasicTokens (int bufferSize)
		{
			Assert.That (Tokenize (BasicRtf, bufferSize), Is.EqualTo (BasicExpected));
		}

		[TestCase (1)]
		[TestCase (3)]
		[TestCase (4096)]
		public async Task TestBasicTokensAsync (int bufferSize)
		{
			Assert.That (await TokenizeAsync (BasicRtf, bufferSize), Is.EqualTo (BasicExpected));
		}

		// RTF 1.9.1, "\binN": the N bytes that follow the control word (and its single delimiting space) are
		// binary data and must not be interpreted, even if they look like RTF syntax.
		const string BinaryRtf = "{\\bin5 {}\\}{x}";
		static readonly string[] BinaryExpected = { "{", "word:bin=5 [Bin]", "text:x", "}" };

		[TestCase (1)]
		[TestCase (4096)]
		public void TestBinarySkipped (int bufferSize)
		{
			Assert.That (Tokenize (BinaryRtf, bufferSize), Is.EqualTo (BinaryExpected));
		}

		[TestCase (1)]
		[TestCase (4096)]
		public async Task TestBinarySkippedAsync (int bufferSize)
		{
			Assert.That (await TokenizeAsync (BinaryRtf, bufferSize), Is.EqualTo (BinaryExpected));
		}

		[Test]
		public void TestHugeBinaryIsStreamed ()
		{
			// A \bin length larger than the input must not cause any allocation proportional to N.
			var tokens = Tokenize ("{\\bin2147483647 abc", 16);

			Assert.That (tokens, Is.EqualTo (new [] { "{", "word:bin=2147483647 [Bin]" }));
		}

		[Test]
		public void TestOverlongKeyword ()
		{
			var keyword = new string ('a', 1000);
			var tokens = Tokenize ("\\" + keyword + "123 x", 7);

			Assert.That (tokens, Has.Count.EqualTo (2));
			Assert.That (tokens[0], Does.StartWith ("word:" + new string ('a', RtfTokenizer.MaxKeywordLength) + "=123 (truncated) [Unknown]"));
			Assert.That (tokens[1], Is.EqualTo ("text:x"));
		}

		[Test]
		public void TestOverlongParameterSaturates ()
		{
			var tokens = Tokenize ("\\fs" + new string ('9', 1000) + " \\fs-" + new string ('9', 1000) + " x", 5);

			Assert.That (tokens, Is.EqualTo (new [] {
				"word:fs=" + int.MaxValue + " [Fs]",
				"word:fs=" + int.MinValue + " [Fs]",
				"text:x"
			}));
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new RtfTokenizer (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => new RtfTokenizer (new StringReader (string.Empty), 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new RtfTokenizer (new StringReader (string.Empty), -1));
		}

		// Malformed escapes must never throw or desynchronize the tokenizer, regardless of where the buffer
		// boundaries fall.
		static readonly object[] MalformedCases = {
			new object[] { "\\fs-x", new [] { "word:fs [Fs]", "text:x" } },
			new object[] { "\\fs- x", new [] { "word:fs [Fs]", "text: x" } },
			new object[] { "\\'zz", new [] { "text:zz" } },
			new object[] { "\\'a}", new [] { "hex:0a", "}" } },
			new object[] { "\\'A9\\'Fe", new [] { "hex:a9", "hex:fe" } },
			new object[] { "\\'\\b", new [] { "word:b [B]" } },
			new object[] { "\\'4", new [] { "hex:04" } },
			new object[] { "x\\'", new [] { "text:x" } },
			new object[] { "x\\", new [] { "text:x" } },
			new object[] { "\\fs", new [] { "word:fs [Fs]" } },
			new object[] { "\\fs-", new [] { "word:fs [Fs]" } },
			new object[] { "\\fs-12", new [] { "word:fs=-12 [Fs]" } },
			new object[] { "\\\r\\\t\\~", new [] { "symbol:\n", "symbol:\t", "symbol:~" } },
			new object[] { "\\bin-5 x", new [] { "word:bin=-5 [Bin]", "text:x" } },
			new object[] { "\\bin0 x", new [] { "word:bin=0 [Bin]", "text:x" } },
			new object[] { "a\r\n\r\nb", new [] { "text:ab" } },
		};

		[TestCaseSource (nameof (MalformedCases))]
		public void TestMalformedEscapes (string rtf, string[] expected)
		{
			foreach (var bufferSize in new [] { 1, 2, 3, 4096 })
				Assert.That (Tokenize (rtf, bufferSize), Is.EqualTo (expected), $"bufferSize={bufferSize}");
		}

		[TestCaseSource (nameof (MalformedCases))]
		public async Task TestMalformedEscapesAsync (string rtf, string[] expected)
		{
			foreach (var bufferSize in new [] { 1, 2, 3, 4096 })
				Assert.That (await TokenizeAsync (rtf, bufferSize), Is.EqualTo (expected), $"bufferSize={bufferSize}");
		}

		[Test]
		public void TestEndOfFileIsSticky ()
		{
			var tokenizer = new RtfTokenizer (new StringReader ("x"), 1);

			Assert.That (tokenizer.ReadNextToken (), Is.True);
			Assert.That (tokenizer.Kind, Is.EqualTo (RtfTokenKind.Text));

			for (int i = 0; i < 3; i++) {
				Assert.That (tokenizer.ReadNextToken (), Is.False);
				Assert.That (tokenizer.Kind, Is.EqualTo (RtfTokenKind.EndOfFile));
			}
		}

		[Test]
		public async Task TestEndOfFileIsStickyAsync ()
		{
			var tokenizer = new RtfTokenizer (new StringReader ("x"), 1);

			Assert.That (await tokenizer.ReadNextTokenAsync (), Is.True);
			Assert.That (tokenizer.Kind, Is.EqualTo (RtfTokenKind.Text));

			for (int i = 0; i < 3; i++) {
				Assert.That (await tokenizer.ReadNextTokenAsync (), Is.False);
				Assert.That (tokenizer.Kind, Is.EqualTo (RtfTokenKind.EndOfFile));
			}
		}

		[Test]
		public async Task TestCancellation ()
		{
			var tokenizer = new RtfTokenizer (new StringReader ("{\\rtf1 hello}"), 4);
			using var cts = new CancellationTokenSource ();

			cts.Cancel ();

			Assert.That (await tokenizer.ReadNextTokenAsync (CancellationToken.None), Is.True);
			Assert.That (async () => {
				while (await tokenizer.ReadNextTokenAsync (cts.Token))
					;
			}, Throws.InstanceOf<OperationCanceledException> ());
		}
	}
}
