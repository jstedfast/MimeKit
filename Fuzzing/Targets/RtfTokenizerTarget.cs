//
// RtfTokenizerTarget.cs
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

using MimeKit.Fuzzing.Helpers;
using MimeKit.Text;

namespace MimeKit.Fuzzing.Targets {
	// Tokenizes the input (as Latin-1) using the sync API with the default and a tiny buffer size and the async
	// API over a reader that returns one character at a time, checking that all three agree. Then runs the
	// interpreter sync and async, checking that the content reported to the handler agrees and that the group
	// stack and the font and color tables never exceed their limits.
	sealed class RtfTokenizerTarget : IFuzzTarget
	{
		public string Name => "rtf-tokenizer";

		public int MaxLength => 1024 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var text = Fuzz.Latin1.GetString (data);

			var tokens = Tokenize (new RtfTokenizer (new StringReader (text)), false, text.Length);
			var small = Tokenize (new RtfTokenizer (new StringReader (text), 16), false, text.Length);
			var async = Tokenize (new RtfTokenizer (new OneCharTextReader (text), 1), true, text.Length);

			Fuzz.Equal (tokens, small, "RtfTokenizer default vs 16-char buffer");
			Fuzz.Equal (tokens, async, "RtfTokenizer sync vs async (one char at a time)");

			uint selector = Fuzz.Selector (data);
			bool small_limits = (selector & 1) != 0;
			int maxGroupDepth = small_limits ? 16 : 4096;
			int maxFonts = small_limits ? 8 : 4096;
			int maxColors = small_limits ? 8 : 4096;
			bool extractHtml = (selector & 2) != 0;

			var syncTrace = Interpret (new StringReader (text), false, text.Length, maxGroupDepth, maxFonts, maxColors, extractHtml);
			var asyncTrace = Interpret (new OneCharTextReader (text), true, text.Length, maxGroupDepth, maxFonts, maxColors, extractHtml);

			Fuzz.Equal (syncTrace, asyncTrace, "RtfInterpreter Step vs StepAsync");
		}

		static string Tokenize (RtfTokenizer tokenizer, bool async, int length)
		{
			var trace = new StringBuilder ();
			var pendingText = new StringBuilder ();
			int count = 0;

			while (async ? Fuzz.Wait (tokenizer.ReadNextTokenAsync ()) : tokenizer.ReadNextToken ()) {
				Fuzz.Check (++count <= length + 1, $"RtfTokenizer produced more than {length + 1} tokens from {length} chars");

				var kind = tokenizer.Kind;

				// Note: Runs of text may legitimately be split differently depending on how the input was buffered.
				if (kind == RtfTokenKind.Text) {
					Fuzz.Check (tokenizer.TextLength > 0, "RtfTokenizer produced an empty Text token");
					Fuzz.Check (tokenizer.TextIndex >= 0 && tokenizer.TextIndex <= tokenizer.TextBuffer.Length - tokenizer.TextLength, "RtfTokenizer produced an invalid text range");
					pendingText.Append (tokenizer.Text);
					continue;
				}

				FlushText (trace, pendingText);

				trace.Append (kind);

				switch (kind) {
				case RtfTokenKind.ControlWord:
					Fuzz.Check (tokenizer.Keyword.Length > 0 && tokenizer.Keyword.Length <= RtfTokenizer.MaxKeywordLength, $"RtfTokenizer produced a keyword of {tokenizer.Keyword.Length} chars");
					trace.Append (' ').Append (tokenizer.Keyword).Append (' ').Append (tokenizer.KeywordId).Append (' ').Append (tokenizer.IsKeywordTruncated);
					if (tokenizer.HasParameter)
						trace.Append (' ').Append (tokenizer.Parameter);
					break;
				case RtfTokenKind.ControlSymbol:
					trace.Append (' ').Append ((int) tokenizer.Symbol);
					break;
				case RtfTokenKind.HexChar:
					trace.Append (' ').Append (tokenizer.HexValue);
					break;
				}

				trace.AppendLine ();
			}

			FlushText (trace, pendingText);

			return trace.ToString ();
		}

		static void FlushText (StringBuilder trace, StringBuilder pendingText)
		{
			if (pendingText.Length == 0)
				return;

			trace.Append ("Text ").Append (pendingText.Length).Append (' ').AppendLine (Fuzz.HashString (pendingText.ToString ()));
			pendingText.Clear ();
		}

		static string Interpret (TextReader reader, bool async, int length, int maxGroupDepth, int maxFonts, int maxColors, bool extractHtml)
		{
			var handler = new RecordingHandler ();
			var rtf = new RtfInterpreter (reader, handler, maxGroupDepth, maxFonts, maxColors) {
				ExtractHtml = extractHtml
			};
			int count = 0;

			while (async ? Fuzz.Wait (rtf.StepAsync ()) : rtf.Step ()) {
				Fuzz.Check (++count <= length + 1, $"RtfInterpreter processed more than {length + 1} tokens from {length} chars");
				Fuzz.Check (rtf.StackEntries <= maxGroupDepth, $"RtfInterpreter used {rtf.StackEntries} stack entries (limit {maxGroupDepth})");
				Fuzz.Check (rtf.FontCount <= maxFonts, $"RtfInterpreter font table has {rtf.FontCount} entries (limit {maxFonts})");
				Fuzz.Check (rtf.ColorCount <= maxColors, $"RtfInterpreter color table has {rtf.ColorCount} entries (limit {maxColors})");
				Fuzz.Check (rtf.Depth >= 0, $"RtfInterpreter depth is negative ({rtf.Depth})");
			}

			return handler.Trace;
		}

		sealed class RecordingHandler : RtfContentHandler
		{
			readonly StringBuilder trace = new StringBuilder ();
			readonly StringBuilder pendingText = new StringBuilder ();
			string? pendingKind, pendingState;

			public string Trace {
				get {
					Flush ();
					return trace.ToString ();
				}
			}

			static string Snapshot (RtfInterpreter rtf)
			{
				ref readonly var state = ref rtf.State;

				return string.Join (",", state.Destination, state.Alignment, state.VerticalAlignment, state.Flags, state.Font, state.FontSize,
					rtf.GetColor (state.ForegroundColor), rtf.GetColor (state.BackgroundColor), state.Hyperlink, rtf.FromHtml);
			}

			void Flush ()
			{
				if (pendingKind == null)
					return;

				trace.Append (pendingKind).Append (' ').Append (pendingState).Append (' ').Append (pendingText.Length).Append (' ').AppendLine (Fuzz.HashString (pendingText.ToString ()));
				pendingText.Clear ();
				pendingKind = null;
			}

			void Text (string kind, RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				Fuzz.Check (buffer != null && index >= 0 && count >= 0 && index <= buffer.Length - count, $"{kind}: invalid buffer range ({index}, {count})");

				var state = Snapshot (rtf);

				if (pendingKind != kind || pendingState != state) {
					Flush ();
					pendingKind = kind;
					pendingState = state;
				}

				pendingText.Append (buffer, index, count);
			}

			void Event (string kind, RtfInterpreter rtf)
			{
				Flush ();
				trace.Append (kind).Append (' ').AppendLine (Snapshot (rtf));
			}

			public override void OnBegin (RtfInterpreter rtf) => Event ("Begin", rtf);
			public override void OnText (RtfInterpreter rtf, char[] buffer, int index, int count) => Text ("Text", rtf, buffer, index, count);
			public override void OnHtmlTag (RtfInterpreter rtf, char[] buffer, int index, int count) => Text ("HtmlTag", rtf, buffer, index, count);
			public override void OnParagraph (RtfInterpreter rtf) => Event ("Paragraph", rtf);
			public override void OnLineBreak (RtfInterpreter rtf) => Event ("LineBreak", rtf);
			public override void OnCell (RtfInterpreter rtf) => Event ("Cell", rtf);
			public override void OnRow (RtfInterpreter rtf) => Event ("Row", rtf);
			public override void OnObjectPlaceholder (RtfInterpreter rtf) => Event ("ObjectPlaceholder", rtf);
		}
	}
}
