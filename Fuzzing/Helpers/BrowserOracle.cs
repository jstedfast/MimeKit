//
// BrowserOracle.cs
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
using System.Text.Json;

using Microsoft.Playwright;

using MimeKit.Text;

namespace MimeKit.Fuzzing.Helpers {
	// Confirms a failure reported by HtmlOracle using a real browser (Chrome, Edge or Playwright's Chromium, in that order),
	// since AngleSharp has bugs of its own (e.g. it matches "any other end tag" by local name alone, so "</mi>" closes a MathML
	// <mi> element that browsers leave open). Browsers don't record source offsets, so instead every start tag that HtmlTokenizer
	// emitted gets a unique marker attribute inserted after its name. Any element that the browser creates without a marker
	// (other than the attribute-less elements implied by the tree builder) must have come from markup that HtmlTokenizer did not
	// see as a start tag.
	//
	// The browser is only launched the first time that it is needed, which keeps the fuzzing loop fast.
	static class BrowserOracle
	{
		const string MarkerPrefix = "mkoracle";
		const string Url = "http://oracle.invalid/";

		// Collects every element (including those in <template> contents) in document order.
		const string DumpScript = @"() => {
	const elements = [];
	const walk = node => {
		for (let child = node.firstChild; child; child = child.nextSibling) {
			if (child.nodeType !== 1)
				continue;
			elements.push({ ns: child.namespaceURI, name: child.localName, attributes: [...child.attributes].map(a => a.name) });
			walk(child instanceof HTMLTemplateElement ? child.content : child);
		}
	};
	walk(document);
	return JSON.stringify({ frameset: document.body !== null && document.body.localName === 'frameset', elements });
}";

		// The elements that the tree builder creates without a start tag (e.g. "</p>" creates a <p> and "</br>" creates a <br>).
		static readonly HashSet<string> ImpliedElements = new (StringComparer.Ordinal) {
			"html", "head", "body", "tbody", "tr", "colgroup", "p", "br"
		};

		static readonly object SyncRoot = new ();
		static IPlaywright? playwright;
		static IBrowser? browser;
		static string? unavailable;

		static IBrowser? GetBrowser (out string? error)
		{
			lock (SyncRoot) {
				if (browser is null && unavailable is null) {
					try {
						playwright = Playwright.CreateAsync ().GetAwaiter ().GetResult ();

						foreach (var channel in new[] { "chrome", "msedge", null }) {
							try {
								browser = playwright.Chromium.LaunchAsync (new BrowserTypeLaunchOptions { Channel = channel }).GetAwaiter ().GetResult ();
								break;
							} catch (PlaywrightException ex) {
								unavailable = ex.Message.Split ('\n')[0];
							}
						}

						if (browser != null) {
							unavailable = null;
							AppDomain.CurrentDomain.ProcessExit += (s, e) => {
								try {
									browser.CloseAsync ().GetAwaiter ().GetResult ();
									playwright.Dispose ();
								} catch {
								}
							};
						}
					} catch (Exception ex) {
						unavailable = ex.Message.Split ('\n')[0];
					}
				}

				error = unavailable;

				return browser;
			}
		}

		/// <summary>
		/// Check the start tags that HtmlTokenizer emitted against the document that the browser creates.
		/// </summary>
		/// <returns><see langword="null"/> if the browser agrees with HtmlTokenizer; otherwise, a description of the disagreement.</returns>
		/// <exception cref="InvalidOperationException">The check could not be performed.</exception>
		public static string? Check (string html, bool scriptingEnabled, IReadOnlyDictionary<int, HtmlTagToken> tags, bool inFrameset)
		{
			if (html.Contains (MarkerPrefix, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException ($"the input contains the marker prefix ({MarkerPrefix})");

			var marked = Mark (html, tags, out var markers);
			var browser = GetBrowser (out var error) ?? throw new InvalidOperationException ($"no browser is available ({error})");
			var json = Parse (browser, marked, scriptingEnabled);
			var mode = $"scripting {(scriptingEnabled ? "enabled" : "disabled")}";

			using var result = JsonDocument.Parse (json);
			bool frameset = result.RootElement.GetProperty ("frameset").GetBoolean ();

			if (frameset != inFrameset)
				return $"browser {(frameset ? "creates" : "does not create")} a frameset document ({mode}) but HtmlTokenizer {(inFrameset ? "does" : "does not")} think so";

			foreach (var element in result.RootElement.GetProperty ("elements").EnumerateArray ()) {
				var ns = element.GetProperty ("ns").GetString ();
				var name = element.GetProperty ("name").GetString ()!;
				var attributes = new List<string> ();
				var seen = new List<HtmlTagToken> ();

				foreach (var attribute in element.GetProperty ("attributes").EnumerateArray ()) {
					var attr = attribute.GetString ()!;

					if (TryGetMarkerIndex (attr, out int index) && index < markers.Count)
						seen.Add (markers[index]);
					else
						attributes.Add (attr);
				}

				var where = $"browser creates <{Fuzz.Escape (name)}> ({mode})";

				if (seen.Count == 0) {
					if (ns == "http://www.w3.org/1999/xhtml" && ImpliedElements.Contains (name) && attributes.Count == 0)
						continue;

					return $"{where} but HtmlTokenizer did not see its start tag";
				}

				foreach (var tag in seen) {
					var tagName = tag.Name.ToLowerInvariant ();

					// The tree builder renames <image> to <img>.
					if (tagName == "image" && name == "img")
						tagName = "img";

					// Note: The tree builder adjusts the case of some SVG element names (e.g. <foreignObject>).
					if (!tagName.Equals (name, StringComparison.OrdinalIgnoreCase))
						return $"{where} but HtmlTokenizer saw <{Fuzz.Escape (tagName)}>";
				}

				// Note: The attributes of a duplicate <html> or <body> start tag are added to the existing element, so check that
				// each attribute came from one of the start tags that contributed to the element.
				foreach (var attribute in attributes) {
					bool found = false;

					foreach (var tag in seen) {
						foreach (var attr in tag.Attributes) {
							if (attr.Name.Equals (attribute, StringComparison.OrdinalIgnoreCase)) {
								found = true;
								break;
							}
						}
					}

					if (!found)
						return $"{where} with attribute {Fuzz.Escape (attribute)} but HtmlTokenizer did not see that attribute";
				}
			}

			return null;
		}

		// Parses the index of a marker attribute (whose name might be followed by the trailing characters of the tag name).
		static bool TryGetMarkerIndex (string attribute, out int index)
		{
			index = 0;

			if (!attribute.StartsWith (MarkerPrefix, StringComparison.Ordinal))
				return false;

			int end = MarkerPrefix.Length;

			while (end < attribute.Length && char.IsAsciiDigit (attribute[end]))
				end++;

			for (int i = end; i < attribute.Length; i++) {
				if (attribute[i] is not ('-' or ']' or '!'))
					return false;
			}

			return int.TryParse (attribute.AsSpan (MarkerPrefix.Length, end - MarkerPrefix.Length), out index);
		}

		// Inserts a marker attribute after the name of each start tag that HtmlTokenizer emitted.
		static string Mark (string html, IReadOnlyDictionary<int, HtmlTagToken> tags, out List<HtmlTagToken> markers)
		{
			var offsets = new List<int> (tags.Keys);
			var builder = new StringBuilder (html.Length + tags.Count * 16);
			int index = 0;

			markers = new List<HtmlTagToken> (tags.Count);
			offsets.Sort ();

			foreach (var offset in offsets) {
				int end = offset + 1;

				while (end < html.Length && html[end] is not ('\t' or '\n' or '\f' or ' ' or '/' or '>'))
					end++;

				// Note: Inserting an attribute between "--" and '>' or "]]" and '>' would change where the browser ends a comment
				// or CDATA section if it treats this markup as text (which is what HtmlTokenizer might have gotten wrong), so repeat
				// the trailing '-', ']' and '!' characters of the tag name after the marker (e.g. "<x--" becomes "<x-- mkoracle0--").
				// The marker ends with a digit, so the browser would still end the comment or CDATA section at the same '>'.
				int suffix = end;

				if (end < html.Length && html[end] == '>') {
					while (html[suffix - 1] is '-' or ']' or '!')
						suffix--;
				}

				builder.Append (html, index, end - index);
				builder.Append (' ').Append (MarkerPrefix).Append (markers.Count);
				builder.Append (html, suffix, end - suffix);
				markers.Add (tags[offset]);
				index = end;
			}

			builder.Append (html, index, html.Length - index);

			return builder.ToString ();
		}

		static string Parse (IBrowser browser, string html, bool scriptingEnabled)
		{
			// Note: Disabling JavaScript also clears the parser's scripting flag. Otherwise, the Content-Security-Policy keeps the
			// scripts and event handlers in the input from changing the document while leaving the scripting flag set.
			var context = browser.NewContextAsync (new BrowserNewContextOptions { JavaScriptEnabled = scriptingEnabled }).GetAwaiter ().GetResult ();

			try {
				var page = context.NewPageAsync ().GetAwaiter ().GetResult ();
				var body = Encoding.UTF8.GetBytes (html);

				page.RouteAsync ("**/*", async route => {
					if (route.Request.Url == Url && route.Request.IsNavigationRequest && route.Request.Frame == page.MainFrame) {
						await route.FulfillAsync (new RouteFulfillOptions {
							Status = 200,
							BodyBytes = body,
							Headers = new Dictionary<string, string> {
								["Content-Type"] = "text/html; charset=utf-8",
								["Content-Security-Policy"] = "default-src 'none'; script-src 'none'"
							}
						}).ConfigureAwait (false);
					} else {
						await route.AbortAsync ().ConfigureAwait (false);
					}
				}).GetAwaiter ().GetResult ();

				page.GotoAsync (Url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }).GetAwaiter ().GetResult ();

				return page.EvaluateAsync<string> (DumpScript).GetAwaiter ().GetResult ();
			} finally {
				context.CloseAsync ().GetAwaiter ().GetResult ();
			}
		}
	}
}