//
// TextConvertersTarget.cs
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
	// Converts the input with TextToText, TextToFlowed, FlowedToText, TextToHtml and FlowedToHtml, checking that:
	//
	// * the string, TextReader (one char at a time) and Stream overloads produce identical output;
	// * the output size is bounded by the input size;
	// * the HTML output only uses the elements and attributes that the converters generate, with balanced
	//   end tags, and honours the HtmlTagCallback's DeleteTag/DeleteEndTag/SuppressInnerContent requests;
	// * hyperlinks only use the schemes that UrlScanner recognizes;
	// * FlowedToText (DelSp=yes) of the TextToFlowed output reproduces the input lines.
	sealed class TextConvertersTarget : IFuzzTarget
	{
		static readonly string[] AllowedSchemes = {
			"mailto", "http", "https", "ftp", "sftp", "file", "news", "nntp", "telnet", "webcal", "callto", "h323", "sip"
		};

		enum CallbackMode
		{
			Default,
			DeleteBlockQuote,
			SuppressBlockQuote,
			InvokeForEndTags
		}

		public string Name => "text-converters";

		public int MaxLength => 256 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			var text = Fuzz.Latin1.GetString (input);
			uint selector = Fuzz.Selector (data);
			bool deleteSpace = (selector & 1) != 0;
			bool fragment = (selector & 2) != 0;
			var mode = (CallbackMode) ((selector >> 2) & 3);

			CheckConverter (new TextToText (), input, text, 2, 64);
			CheckConverter (new TextToFlowed (), input, text, 4, 256);
			CheckConverter (new FlowedToText { DeleteSpace = deleteSpace }, input, text, 2, 64);

			var textToHtml = new TextToHtml { OutputHtmlFragment = fragment, HtmlTagCallback = CreateCallback (mode) };
			CheckHtml (CheckConverter (textToHtml, input, text, 64, 256), fragment, mode, "TextToHtml");

			var flowedToHtml = new FlowedToHtml { OutputHtmlFragment = fragment, DeleteSpace = deleteSpace, HtmlTagCallback = CreateCallback (mode) };
			CheckHtml (CheckConverter (flowedToHtml, input, text, 64, 256), fragment, mode, "FlowedToHtml");

			CheckFlowedRoundTrip (text);
		}

		static string CheckConverter (TextConverter converter, byte[] input, string text, int factor, int slack)
		{
			var name = converter.GetType ().Name;

			converter.InputEncoding = Fuzz.Latin1;
			converter.OutputEncoding = Fuzz.Utf8;
			converter.DetectEncodingFromByteOrderMark = false;

			var output = converter.Convert (text);

			var oneChar = Convert (converter, new OneCharTextReader (text));
			Fuzz.Equal (output, oneChar, $"{name} string vs one char at a time");

			var stream = Convert (converter, input);
			Fuzz.Equal (output, stream, $"{name} string vs Stream");

			Fuzz.Check (output.Length <= (long) factor * text.Length + slack, $"{name} produced {output.Length} chars from {text.Length} chars");

			return output;
		}

		static HtmlTagCallback? CreateCallback (CallbackMode mode)
		{
			switch (mode) {
			case CallbackMode.DeleteBlockQuote:
				return (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.BlockQuote) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						return;
					}

					ctx.WriteTag (writer, true);
				};
			case CallbackMode.SuppressBlockQuote:
				return (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.BlockQuote && !ctx.IsEndTag)
						ctx.SuppressInnerContent = true;

					ctx.WriteTag (writer, true);
				};
			case CallbackMode.InvokeForEndTags:
				return (ctx, writer) => {
					if (!ctx.IsEndTag)
						ctx.InvokeCallbackForEndTag = true;

					ctx.WriteTag (writer, true);
				};
			default:
				return null;
			}
		}

		static void CheckHtml (string html, bool fragment, CallbackMode mode, string name)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html)) { MaxElementDepth = 1 << 20 };
			var stack = new List<string> ();
			int suppressed = -1;

			while (tokenizer.ReadNextToken (out var token)) {
				if (token.Kind == HtmlTokenKind.Data) {
					Fuzz.Check (suppressed == -1, $"{name} wrote content inside a <blockquote> whose inner content was suppressed");
					continue;
				}

				Fuzz.Check (token.Kind == HtmlTokenKind.Tag, $"{name} output contains a {token.Kind} token");

				var tag = (HtmlTagToken) token;
				var tagName = tag.Name;

				if (tag.IsEndTag) {
					Fuzz.Check (tag.Attributes.Count == 0, $"{name} end tag </{tagName}> has attributes");
					Fuzz.Check (stack.Count > 0 && stack[stack.Count - 1] == tagName, $"{name} wrote an unbalanced end tag </{tagName}> (open: {string.Join (">", stack)})");
					stack.RemoveAt (stack.Count - 1);

					if (stack.Count == suppressed)
						suppressed = -1;
					continue;
				}

				if (tagName is "html" or "body") {
					Fuzz.Check (!fragment, $"{name} wrote <{tagName}> in an HTML fragment");
					Fuzz.Check (stack.Count == (tagName == "html" ? 0 : 1), $"{name} wrote <{tagName}> at depth {stack.Count}");
				} else {
					Fuzz.Check (suppressed == -1, $"{name} wrote <{tagName}> inside a <blockquote> whose inner content was suppressed");
					Fuzz.Check (fragment || stack.Count >= 2, $"{name} wrote <{tagName}> outside of <body>");
				}

				switch (tagName) {
				case "html":
				case "body":
				case "p":
					CheckNoAttributes (tag, name);
					break;
				case "blockquote":
					CheckNoAttributes (tag, name);
					Fuzz.Check (mode != CallbackMode.DeleteBlockQuote, $"{name} wrote a <blockquote> that the callback deleted");

					if (mode == CallbackMode.SuppressBlockQuote)
						suppressed = stack.Count;
					break;
				case "br":
					CheckNoAttributes (tag, name);
					continue;
				case "a":
					Fuzz.Check (tag.Attributes.Count == 1 && tag.Attributes[0].Name == "href", $"{name} wrote an <a> without exactly one href attribute");
					CheckHref (tag.Attributes[0].Value, name);
					break;
				default:
					throw new FuzzInvariantException ($"{name} wrote an unexpected <{Fuzz.Escape (tagName)}> element");
				}

				Fuzz.Check (!tag.IsEmptyElement, $"{name} wrote a self-closing <{tagName}/>");
				stack.Add (tagName);
			}

			Fuzz.Check (stack.Count == 0, $"{name} left unclosed elements: {string.Join (">", stack)}");
		}

		static void CheckNoAttributes (HtmlTagToken tag, string name)
		{
			Fuzz.Check (tag.Attributes.Count == 0, $"{name} wrote <{tag.Name}> with attributes");
		}

		static void CheckHref (string? href, string name)
		{
			Fuzz.Check (!string.IsNullOrEmpty (href), $"{name} wrote an <a> with an empty href");

			int colon = href!.IndexOf (':');

			Fuzz.Check (colon > 0, $"{name} wrote an href without a scheme: {Fuzz.Escape (href)}");

			var scheme = href.Substring (0, colon);

			Fuzz.Check (AllowedSchemes.Any (s => s.Equals (scheme, StringComparison.OrdinalIgnoreCase)), $"{name} wrote an href with an unexpected scheme: {Fuzz.Escape (href)}");
		}

		// TextToFlowed soft-breaks long lines with an extra trailing space (i.e. DelSp=yes), trims trailing spaces
		// (RFC 3676 section 4.1) and normalizes quote markers to ">>> ", so FlowedToText with DeleteSpace should
		// reproduce the input lines with those normalizations applied.
		static void CheckFlowedRoundTrip (string text)
		{
			var flowed = new TextToFlowed ().Convert (text);
			var output = new FlowedToText { DeleteSpace = true }.Convert (flowed);
			var expected = new List<string> ();
			var actual = ReadLines (output);

			foreach (var line in ReadLines (text)) {
				var trimmed = line.TrimEnd (' ');
				int index = 0, depth = 0;

				while (index < trimmed.Length && trimmed[index] == '>') {
					depth++;
					index++;

					if (index < trimmed.Length && trimmed[index] == ' ')
						index++;
				}

				expected.Add (depth > 0 ? new string ('>', depth) + " " + trimmed.Substring (index) : trimmed);
			}

			Fuzz.Check (actual.Count == expected.Count, $"FlowedToText (TextToFlowed (text)) has {actual.Count} lines, expected {expected.Count}");

			for (int i = 0; i < expected.Count; i++)
				Fuzz.Equal (expected[i], actual[i], $"FlowedToText (TextToFlowed (text)) line {i}");
		}

		static List<string> ReadLines (string text)
		{
			var lines = new List<string> ();
			using var reader = new StringReader (text);
			string? line;

			while ((line = reader.ReadLine ()) != null)
				lines.Add (line);

			return lines;
		}

		static string Convert (TextConverter converter, TextReader reader)
		{
			using var writer = new StringWriter ();

			converter.Convert (reader, writer);

			return writer.ToString ();
		}

		static string Convert (TextConverter converter, byte[] input)
		{
			using var output = new MemoryStream ();

			converter.Convert (new MemoryStream (input, false), output);

			return Fuzz.Utf8.GetString (output.ToArray ());
		}
	}
}
