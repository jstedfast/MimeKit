//
// HtmlTokenizerTarget.cs
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
	// Tokenizes the input with HtmlTokenizer and converts it with HtmlToHtml and HtmlTextPreviewer, checking that:
	//
	// * the TextReader (whole string and one char at a time) and Stream inputs produce identical tokens;
	// * the number of tokens is bounded by the input length;
	// * re-tokenizing the HtmlToHtml output yields exactly the start tags (and attributes) that the
	//   HtmlTagCallback wrote, so that no markup can bypass the callback;
	// * converting the HtmlToHtml output a second time does not change it;
	// * the preview text is no longer than MaximumPreviewLength and does not depend on how the input is chunked.
	sealed class HtmlTokenizerTarget : IFuzzTarget
	{
		// Elements that the filtering callback removes. The content of the non-void elements is suppressed as well,
		// since removing only the tag would change how a renderer tokenizes the content (e.g. the content of <svg>).
		static readonly HashSet<string> BlockedVoidElements = new (StringComparer.Ordinal) {
			"img", "embed", "link", "meta", "base", "input"
		};
		static readonly HashSet<string> BlockedElements = new (StringComparer.Ordinal) {
			"script", "iframe", "object", "svg", "math", "template", "frameset", "frame"
		};

		public string Name => "html-tokenizer";

		public int MaxLength => 256 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			Run (data, Fuzz.Selector (data));
		}

		// Note: Used for minimizing crashes, since the selector is derived from the whole input.
		internal static void Run (ReadOnlySpan<byte> data, uint selector)
		{
			var input = data.ToArray ();
			var text = Fuzz.Latin1.GetString (input);

			CheckTokenizer (input, text, selector);
			CheckHtmlToHtml (text, selector);
			CheckPreviewer (text, selector);

			// Note: The element depth limit makes HtmlTokenizer stop modeling the tree, so it cannot match a browser.
			if ((selector & 8) == 0)
				HtmlOracle.Check (HtmlOracle.Normalize (text), (selector & 1) != 0, "Input");
		}

		static HtmlTokenizer CreateTokenizer (HtmlTokenizer tokenizer, uint selector)
		{
			tokenizer.ScriptingEnabled = (selector & 1) != 0;
			tokenizer.DecodeCharacterReferences = (selector & 2) != 0;
			tokenizer.IgnoreTruncatedTags = (selector & 4) != 0;

			if ((selector & 8) != 0)
				tokenizer.MaxElementDepth = 4;

			return tokenizer;
		}

		static void CheckTokenizer (byte[] input, string text, uint selector)
		{
			var expected = Tokenize (CreateTokenizer (new HtmlTokenizer (new StringReader (text)), selector), text.Length);
			var oneChar = Tokenize (CreateTokenizer (new HtmlTokenizer (new OneCharTextReader (text)), selector), text.Length);
			Fuzz.Equal (expected, oneChar, "HtmlTokenizer string vs one char at a time");

			var stream = new MemoryStream (input, false);
			var streamed = Tokenize (CreateTokenizer (new HtmlTokenizer (stream, Fuzz.Latin1, false, 16), selector), text.Length);
			Fuzz.Equal (expected, streamed, "HtmlTokenizer string vs Stream");
		}

		static string Tokenize (HtmlTokenizer tokenizer, int length)
		{
			var trace = new StringBuilder ();
			int count = 0;

			while (tokenizer.ReadNextToken (out var token)) {
				count++;
				Fuzz.Check (count <= length + 1, $"HtmlTokenizer returned more than {length + 1} tokens for {length} chars");

				trace.Append (token.Kind).Append (' ');

				switch (token) {
				case HtmlTagToken tag:
					trace.Append (tag.IsEndTag ? "/" : "").Append (tag.Name).Append (' ').Append (tag.Id);
					if (tag.IsEmptyElement)
						trace.Append (" empty");
					foreach (var attribute in tag.Attributes)
						trace.Append (' ').Append (attribute.Name).Append ('=').Append (attribute.Value is null ? "null" : Fuzz.Escape (attribute.Value));
					break;
				case HtmlCommentToken comment:
					trace.Append (comment.IsBogusComment ? "bogus " : "").Append (Fuzz.Escape (comment.Comment));
					break;
				case HtmlDocTypeToken doctype:
					trace.Append (doctype.Name).Append ('|').Append (doctype.PublicKeyword).Append ('|').Append (doctype.PublicIdentifier);
					trace.Append ('|').Append (doctype.SystemKeyword).Append ('|').Append (doctype.SystemIdentifier).Append ('|').Append (doctype.ForceQuirksMode);
					break;
				case HtmlDataToken text:
					trace.Append (Fuzz.Escape (text.Data));
					break;
				}

				trace.Append ('|').Append (token.ToString () is { } s ? Fuzz.Escape (s) : "null");
				trace.Append (" @").Append (tokenizer.TokenizerState).Append ('\n');
			}

			return trace.ToString ();
		}

		sealed class RecordingCallback
		{
			public readonly List<string> StartTags = new ();
			public bool Filter;

			public void Invoke (HtmlTagContext ctx, HtmlWriter writer)
			{
				var name = ctx.TagName;

				if (ctx.IsEndTag) {
					if (!ctx.DeleteEndTag)
						ctx.WriteTag (writer);
					return;
				}

				if (Filter) {
					if (BlockedVoidElements.Contains (name)) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						return;
					}

					if (BlockedElements.Contains (name)) {
						ctx.DeleteTag = true;
						ctx.DeleteEndTag = true;
						ctx.SuppressInnerContent = true;
						return;
					}
				}

				var record = new StringBuilder (name);

				if (ctx.IsEmptyElementTag)
					writer.WriteEmptyElementTag (name);
				else
					writer.WriteStartTag (name);

				foreach (var attribute in ctx.Attributes) {
					if (Filter && (attribute.Name.StartsWith ("on", StringComparison.Ordinal) || attribute.Name == "srcdoc"))
						continue;

					writer.WriteAttribute (attribute);
					record.Append (' ').Append (attribute.Name);
				}

				StartTags.Add (record.ToString ());
			}
		}

		static HtmlToHtml CreateConverter (uint selector, RecordingCallback callback)
		{
			return new HtmlToHtml {
				MaxElementDepth = (selector & 8) != 0 ? 4 : 4096,
				NoScriptHandling = (HtmlNoScriptHandling) ((selector >> 4) % 3),
				FilterComments = (selector & 64) != 0,
				OutputHtmlFragment = (selector & 128) != 0,
				HtmlTagCallback = callback.Invoke,
			};
		}

		static void CheckHtmlToHtml (string text, uint selector)
		{
			var callback = new RecordingCallback { Filter = (selector & 256) != 0 };
			var converter = CreateConverter (selector, callback);
			var output = converter.Convert (text);
			var written = callback.StartTags.ToArray ();

			Fuzz.Check (output.Length <= 16L * text.Length + 256, $"HtmlToHtml produced {output.Length} chars from {text.Length} chars");

			callback.StartTags.Clear ();
			var oneChar = Convert (converter, new OneCharTextReader (text));
			Fuzz.Equal (output, oneChar, "HtmlToHtml string vs one char at a time");

			switch (converter.NoScriptHandling) {
			case HtmlNoScriptHandling.Unwrap:
				// The output contains no <noscript> elements, so it must be interpreted the same way either way.
				CheckStartTags (output, written, false);
				CheckStartTags (output, written, true);
				HtmlOracle.Check (HtmlOracle.Normalize (output), false, "HtmlToHtml output");
				HtmlOracle.Check (HtmlOracle.Normalize (output), true, "HtmlToHtml output");
				break;
			case HtmlNoScriptHandling.ScriptingEnabled:
				CheckStartTags (output, written, true);
				HtmlOracle.Check (HtmlOracle.Normalize (output), true, "HtmlToHtml output");
				break;
			case HtmlNoScriptHandling.ScriptingDisabled:
				CheckStartTags (output, written, false);
				HtmlOracle.Check (HtmlOracle.Normalize (output), false, "HtmlToHtml output");
				break;
			}

			// Note: Once the element depth limit is exceeded, the remainder of the input is written as encoded literal text,
			// which would be encoded again if the output was converted with the same limit, so the output is converted
			// again without the limit.
			callback.StartTags.Clear ();
			converter.MaxElementDepth = 1 << 20;
			var again = converter.Convert (output);
			Fuzz.Equal (output, again, "HtmlToHtml output converted again");
		}

		static void CheckStartTags (string html, string[] written, bool scriptingEnabled)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html)) {
				MaxElementDepth = 1 << 20,
				ScriptingEnabled = scriptingEnabled
			};
			var record = new StringBuilder ();
			int index = 0;

			while (tokenizer.ReadNextToken (out var token)) {
				if (token is not HtmlTagToken tag || tag.IsEndTag)
					continue;

				record.Clear ().Append (tag.Name);
				foreach (var attribute in tag.Attributes)
					record.Append (' ').Append (attribute.Name);

				var actual = record.ToString ();

				Fuzz.Check (index < written.Length, $"HtmlToHtml output (scripting {(scriptingEnabled ? "enabled" : "disabled")}) contains <{Fuzz.Escape (actual)}> which the callback never wrote");
				Fuzz.Check (actual == written[index], $"HtmlToHtml output (scripting {(scriptingEnabled ? "enabled" : "disabled")}) start tag #{index} is <{Fuzz.Escape (actual)}> but the callback wrote <{Fuzz.Escape (written[index])}>");
				index++;
			}

			Fuzz.Check (index == written.Length, $"HtmlToHtml output (scripting {(scriptingEnabled ? "enabled" : "disabled")}) contains {index} start tags but the callback wrote {written.Length}");
		}

		static void CheckPreviewer (string text, uint selector)
		{
			var previewer = new HtmlTextPreviewer {
				MaximumPreviewLength = 1 + (int) ((selector >> 9) % 300)
			};

			var preview = previewer.GetPreviewText (text);
			Fuzz.Check (preview.Length <= previewer.MaximumPreviewLength, $"HtmlTextPreviewer returned {preview.Length} chars (max {previewer.MaximumPreviewLength})");

			var oneChar = previewer.GetPreviewText (new OneCharTextReader (text));
			Fuzz.Equal (preview, oneChar, "HtmlTextPreviewer string vs one char at a time");
		}

		static string Convert (TextConverter converter, TextReader reader)
		{
			using var writer = new StringWriter ();

			converter.Convert (reader, writer);

			return writer.ToString ();
		}
	}
}
