//
// HtmlOracle.cs
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

using System.Reflection;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using MimeKit.Text;

using HtmlTokenizer = MimeKit.Text.HtmlTokenizer;

namespace MimeKit.Fuzzing.Helpers {
	// Differential oracle for MimeKit's HtmlTokenizer: parses the HTML with AngleSharp (a spec-compliant WHATWG
	// tree builder) and checks that every element that AngleSharp created from a start tag corresponds to a start
	// tag that HtmlTokenizer emitted at the same offset, with the same name and with a superset of its attributes.
	//
	// In other words, any markup that a browser would turn into an element must have been seen by HtmlTokenizer
	// (and therefore by an HtmlToHtml callback). The opposite (HtmlTokenizer emitting a tag that a browser ignores
	// or treats as text) is not checked since the tree builder ignores many start tags (e.g. a stray <tr>).
	//
	// It also checks that HtmlTokenizer agrees with AngleSharp on whether the document is a frameset document, since
	// browsers ignore most start tags after a <frameset> (so they don't switch the tokenizer state).
	//
	// AngleSharp does not record the source position for implied elements, elements cloned by the adoption agency
	// algorithm and the <svg> and <math> root elements, so those are not checked. The content of an <iframe srcdoc>
	// is a separate document, so it is not checked either.
	//
	// AngleSharp has bugs of its own, so a failure is only reported if a real browser confirms it (see BrowserOracle).
	static class HtmlOracle
	{
		readonly struct CreatedElement
		{
			public readonly IDocument? Owner;
			public readonly int Offset;
			public readonly string Name;
			public readonly string[] Attributes;

			public CreatedElement (IDocument? owner, int offset, string name, string[] attributes)
			{
				Owner = owner;
				Offset = offset;
				Name = name;
				Attributes = attributes;
			}
		}

		// Normalizes newlines the same way the HTML input stream preprocessor does so that both parsers agree on offsets.
		//
		// NUL characters are also replaced with U+FFFD because AngleSharp ignores a NUL in the "before html", "before head",
		// "in head" and "after head" insertion modes, whereas the WHATWG spec treats it like any other non-whitespace
		// character (it closes the <head> and implies the <body>), as HtmlTokenizer does. E.g. given "\0<noscript><svg>
		// </noscript><script><t>" with scripting disabled, AngleSharp puts the <noscript> in the <head> and creates a <t>
		// element while a browser puts the <noscript> in the <body> and parses the <script> content as raw text.
		// Both parsers see the same text, and the tokenizer states either emit a NUL as a character token or convert it to
		// U+FFFD, so this only changes how the tree builder treats it (consistently for both parsers).
		public static string Normalize (string html)
		{
			return html.Replace ("\r\n", "\n").Replace ('\r', '\n').Replace ('\0', '\uFFFD');
		}

		public static void Check (string html, bool scriptingEnabled, string what)
		{
			var created = new List<CreatedElement> ();
			var options = new HtmlParserOptions {
				IsKeepingSourceReferences = true,
				IsScripting = scriptingEnabled,
				OnCreated = (element, position) => {
					if (element.LocalName == "style" && element.NamespaceUri == NamespaceNames.SvgUri)
						ClearSpecialFlag (element);

					if (position.Position <= 0)
						return;

					var attributes = new string[element.Attributes.Length];
					for (int i = 0; i < attributes.Length; i++)
						attributes[i] = element.Attributes[i]!.Name.ToLowerInvariant ();

					created.Add (new CreatedElement (element.Owner, position.Position - 1, element.LocalName.ToLowerInvariant (), attributes));
				}
			};

			var document = new HtmlParser (options).ParseDocument (html);
			var tags = Tokenize (html, scriptingEnabled, out bool inFrameset);
			var failure = Compare (document, created, tags, inFrameset);

			if (failure is null)
				return;

			var mode = $"scripting {(scriptingEnabled ? "enabled" : "disabled")}";
			string? confirmed;

			// Note: AngleSharp is not a browser and has bugs of its own, so ask a real browser to confirm the failure.
			try {
				confirmed = BrowserOracle.Check (html, scriptingEnabled, tags, inFrameset);
			} catch (Exception ex) {
				throw new FuzzInvariantException ($"{what} ({mode}): {failure} (could not confirm with a browser: {ex.Message})");
			}

			if (confirmed != null)
				throw new FuzzInvariantException ($"{what}: {confirmed} (AngleSharp: {failure})");

			if (ignoredFailures++ < MaxReportedIgnoredFailures)
				Console.Error.WriteLine ($"HtmlOracle: ignoring an AngleSharp bug that a browser does not share: {what} ({mode}): {failure}");
		}

		const int MaxReportedIgnoredFailures = 20;
		static int ignoredFailures;

		static string? Compare (IDocument document, List<CreatedElement> created, Dictionary<int, HtmlTagToken> tags, bool inFrameset)
		{
			// Note: HtmlTokenizer must agree on whether the <body> got replaced by a <frameset>, since browsers ignore most tags after
			// that (which means that they don't switch the tokenizer state). Otherwise, HtmlTokenizer could emit tags that a
			// browser treats as text (or vice versa).
			bool framesetDocument = document.Body?.LocalName == "frameset";

			if (framesetDocument != inFrameset)
				return $"browser {(framesetDocument ? "creates" : "does not create")} a frameset document but HtmlTokenizer {(inFrameset ? "does" : "does not")} think so";

			foreach (var element in created) {
				// Note: AngleSharp also parses the srcdoc of an <iframe> (with the same options), so skip the elements
				// of nested documents since their offsets are relative to the attribute value.
				if (element.Owner != document)
					continue;

				var where = $"browser creates <{element.Name}> at offset {element.Offset}";

				if (!tags.TryGetValue (element.Offset, out var tag))
					return $"{where} but HtmlTokenizer did not see a start tag there";

				var name = tag.Name.ToLowerInvariant ();

				// The tree builder renames <image> to <img>.
				if (name == "image" && element.Name == "img")
					name = "img";

				if (name != element.Name)
					return $"{where} but HtmlTokenizer saw <{Fuzz.Escape (name)}>";

				foreach (var attribute in element.Attributes) {
					bool found = false;

					foreach (var attr in tag.Attributes) {
						if (attr.Name.Equals (attribute, StringComparison.OrdinalIgnoreCase)) {
							found = true;
							break;
						}
					}

					if (!found)
						return $"{where} with attribute {Fuzz.Escape (attribute)} but HtmlTokenizer did not see that attribute";
				}
			}

			return null;
		}

		// AngleSharp puts the SVG <style> element in the "special" category (which, per the WHATWG spec, only includes the
		// SVG foreignObject, desc and title elements). So for "<span><svg><style></span>", AngleSharp ignores the </span>
		// end tag instead of closing the <svg> element, and processes the following markup as foreign content. Since the
		// element is created before it gets inserted, clearing the flag makes AngleSharp follow the spec.
		static readonly FieldInfo NodeFlagsField = typeof (Node).GetField ("_flags", BindingFlags.Instance | BindingFlags.NonPublic)!;
		static readonly long SpecialNodeFlag = Convert.ToInt64 (Enum.Parse (NodeFlagsField.FieldType, "Special"));

		static void ClearSpecialFlag (IElement element)
		{
			long flags = Convert.ToInt64 (NodeFlagsField.GetValue (element));

			NodeFlagsField.SetValue (element, Enum.ToObject (NodeFlagsField.FieldType, flags & ~SpecialNodeFlag));
		}

		// Returns the start tags that HtmlTokenizer emits, keyed by the offset of their '<'.
		static Dictionary<int, HtmlTagToken> Tokenize (string html, bool scriptingEnabled, out bool inFrameset)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html)) {
				MaxElementDepth = 1 << 20,
				ScriptingEnabled = scriptingEnabled
			};
			var tags = new Dictionary<int, HtmlTagToken> ();
			var lineStarts = new List<int> { 0 };
			int previousEnd = 0;

			for (int i = 0; i < html.Length; i++) {
				if (html[i] == '\n')
					lineStarts.Add (i + 1);
			}

			while (tokenizer.ReadNextToken (out var token)) {
				int end = lineStarts[tokenizer.LineNumber - 1] + tokenizer.LinePosition - 1;

				if (token is HtmlTagToken tag && !tag.IsEndTag) {
					// Note: The tokenizer consumes the '<' before it emits the preceding data token, so the '<' of this tag
					// is either the last character of the previous token or the first character after it.
					int start = html.IndexOf ('<', Math.Max (previousEnd - 1, 0));

					// Skip markup that the tokenizer consumes without emitting a token (e.g. an empty CDATA section or "</>").
					while (start >= 0 && start < end && SkipNoTokenMarkup (html, start, out int next))
						start = html.IndexOf ('<', next);

					if (start >= 0 && start < end)
						tags[start] = tag;
				}

				previousEnd = end;
			}

			inFrameset = tokenizer.OpenElements.IsInFrameset;

			return tags;
		}

		static bool SkipNoTokenMarkup (string html, int index, out int next)
		{
			foreach (var markup in NoTokenMarkup) {
				if (string.CompareOrdinal (html, index, markup, 0, markup.Length) == 0) {
					next = index + markup.Length;
					return true;
				}
			}

			next = index;

			return false;
		}

		static readonly string[] NoTokenMarkup = { "<![CDATA[]]>", "</>" };
	}
}
