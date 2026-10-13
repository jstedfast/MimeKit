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
	// AngleSharp does not record the source position for implied elements, elements cloned by the adoption agency
	// algorithm and the <svg> and <math> root elements, so those are not checked. The content of an <iframe srcdoc>
	// is a separate document, so it is not checked either.
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
		public static string Normalize (string html)
		{
			return html.Replace ("\r\n", "\n").Replace ('\r', '\n');
		}

		public static void Check (string html, bool scriptingEnabled, string what)
		{
			var created = new List<CreatedElement> ();
			var options = new HtmlParserOptions {
				IsKeepingSourceReferences = true,
				IsScripting = scriptingEnabled,
				OnCreated = (element, position) => {
					if (position.Position <= 0)
						return;

					var attributes = new string[element.Attributes.Length];
					for (int i = 0; i < attributes.Length; i++)
						attributes[i] = element.Attributes[i]!.Name.ToLowerInvariant ();

					created.Add (new CreatedElement (element.Owner, position.Position - 1, element.LocalName.ToLowerInvariant (), attributes));
				}
			};

			var document = new HtmlParser (options).ParseDocument (html);

			if (created.Count == 0)
				return;

			var tags = Tokenize (html, scriptingEnabled);

			foreach (var element in created) {
				// Note: AngleSharp also parses the srcdoc of an <iframe> (with the same options), so skip the elements
				// of nested documents since their offsets are relative to the attribute value.
				if (element.Owner != document)
					continue;

				var where = $"{what} (scripting {(scriptingEnabled ? "enabled" : "disabled")}): browser creates <{element.Name}> at offset {element.Offset}";

				Fuzz.Check (tags.TryGetValue (element.Offset, out var tag), $"{where} but HtmlTokenizer did not see a start tag there");

				var name = tag!.Name.ToLowerInvariant ();

				// The tree builder renames <image> to <img>.
				if (name == "image" && element.Name == "img")
					name = "img";

				Fuzz.Check (name == element.Name, $"{where} but HtmlTokenizer saw <{Fuzz.Escape (name)}>");

				foreach (var attribute in element.Attributes) {
					bool found = false;

					foreach (var attr in tag.Attributes) {
						if (attr.Name.Equals (attribute, StringComparison.OrdinalIgnoreCase)) {
							found = true;
							break;
						}
					}

					Fuzz.Check (found, $"{where} with attribute {Fuzz.Escape (attribute)} but HtmlTokenizer did not see that attribute");
				}
			}
		}

		// Returns the start tags that HtmlTokenizer emits, keyed by the offset of their '<'.
		static Dictionary<int, HtmlTagToken> Tokenize (string html, bool scriptingEnabled)
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

					if (start >= 0 && start < end)
						tags[start] = tag;
				}

				previousEnd = end;
			}

			return tags;
		}
	}
}
