//
// GeneratedHtmlChecker.cs
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

using System.Text.RegularExpressions;

using MimeKit.Text;

namespace MimeKit.Fuzzing.Helpers {
	// Validates the HTML that RtfToHtml generates when it renders RTF (as opposed to de-encapsulating HTML
	// that was embedded in the RTF): only a fixed vocabulary of elements and attributes may appear, every
	// element must be closed in order, the nesting depth is bounded and hyperlinks may only use the schemes
	// that RtfInterpreter allows.
	static class GeneratedHtmlChecker
	{
		// html > body > table > tr > td > div > a > span
		const int MaxDepth = 8;

		static readonly Regex SafeCss = new Regex ("^[a-z\\-]+:[ a-z\\-#0-9A-F.]+;( [a-z\\-]+:[ a-z\\-#0-9A-F.]+;)*$", RegexOptions.CultureInvariant);
		static readonly string[] AllowedSchemes = { "http", "https", "mailto", "ftp", "tel" };

		public static void Check (string html, bool fragment)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html));
			var stack = new List<string> ();
			int tagCount = 0;

			while (tokenizer.ReadNextToken (out var token)) {
				if (token.Kind == HtmlTokenKind.Data)
					continue;

				Fuzz.Check (token.Kind == HtmlTokenKind.Tag, $"Generated HTML contains a {token.Kind} token");

				var tag = (HtmlTagToken) token;
				var name = tag.Name.ToLowerInvariant ();

				if (!fragment && tagCount < 2 && !tag.IsEndTag)
					Fuzz.Check (name == (tagCount == 0 ? "html" : "body"), $"Generated HTML document starts with <{name}>");

				tagCount++;

				if (tag.IsEndTag) {
					Fuzz.Check (tag.Attributes.Count == 0, $"End tag </{name}> has attributes");
					Fuzz.Check (stack.Count > 0 && stack[stack.Count - 1] == name, $"Unbalanced end tag </{name}> (open: {string.Join (">", stack)})");
					stack.RemoveAt (stack.Count - 1);
					continue;
				}

				switch (name) {
				case "html":
				case "body":
					Fuzz.Check (!fragment, $"<{name}> in an HTML fragment");
					CheckNoAttributes (tag);
					break;
				case "table":
				case "tr":
				case "td":
					CheckNoAttributes (tag);
					break;
				case "div":
					foreach (var attribute in tag.Attributes) {
						Fuzz.Check (attribute.Name == "style", $"<div> has a {attribute.Name} attribute");
						Fuzz.Check (attribute.Value is "text-align: center;" or "text-align: right;" or "text-align: justify;", $"<div> has an unexpected style: {attribute.Value}");
					}
					break;
				case "span":
					foreach (var attribute in tag.Attributes) {
						Fuzz.Check (attribute.Name == "style", $"<span> has a {attribute.Name} attribute");
						Fuzz.Check (attribute.Value != null && SafeCss.IsMatch (attribute.Value), $"<span> has an unexpected style: {Fuzz.Escape (attribute.Value ?? "null")}");
					}
					break;
				case "a":
					foreach (var attribute in tag.Attributes) {
						Fuzz.Check (attribute.Name == "href", $"<a> has a {attribute.Name} attribute");
						CheckHref (attribute.Value);
					}
					break;
				case "br":
					CheckNoAttributes (tag);
					continue;
				case "img":
					// Written by the harness's attachment placeholder callback.
					foreach (var attribute in tag.Attributes)
						Fuzz.Check (attribute.Name is "src" or "alt", $"<img> has a {attribute.Name} attribute");
					continue;
				default:
					throw new FuzzInvariantException ($"Generated HTML contains an unexpected <{name}> element");
				}

				if (tag.IsEmptyElement)
					continue;

				stack.Add (name);
				Fuzz.Check (stack.Count <= MaxDepth, $"Generated HTML nests deeper than {MaxDepth}: {string.Join (">", stack)}");
			}

			Fuzz.Check (stack.Count == 0, $"Generated HTML has unclosed elements: {string.Join (">", stack)}");
		}

		static void CheckNoAttributes (HtmlTagToken tag)
		{
			Fuzz.Check (tag.Attributes.Count == 0, $"<{tag.Name}> has attributes");
		}

		static void CheckHref (string? href)
		{
			Fuzz.Check (!string.IsNullOrEmpty (href), "<a> has an empty href");

			int colon = href!.IndexOf (':');

			Fuzz.Check (colon > 0, $"<a> href has no scheme: {Fuzz.Escape (href)}");

			var scheme = href.Substring (0, colon);

			Fuzz.Check (AllowedSchemes.Any (s => s.Equals (scheme, StringComparison.OrdinalIgnoreCase)), $"<a> href uses a disallowed scheme: {Fuzz.Escape (href)}");
		}
	}
}
