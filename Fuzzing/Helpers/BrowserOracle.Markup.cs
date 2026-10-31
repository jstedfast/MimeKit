//
// BrowserOracle.Markup.cs
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

namespace MimeKit.Fuzzing.Helpers {
	static partial class BrowserOracle
	{
		const string MarkerPrefix = "mkoracle";

		// Kept separate from Playwright so the marker transformation can be tested without launching a browser.
		internal static string Mark (string html, IReadOnlyDictionary<int, HtmlTagToken> tags, out List<HtmlTagToken> markers)
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

				// Preserve comment and CDATA terminators if a browser treats this apparent tag as text.
				int suffix = end;

				if (end < html.Length && html[end] == '>') {
					while (html[suffix - 1] is '-' or ']' or '!')
						suffix--;
				}

				builder.Append (html, index, end - index);
				// A boolean marker would consume a following '=' attribute as its value. Use an unquoted
				// value to avoid introducing quotes that could end an enclosing attribute in a browser.
				builder.Append (' ').Append (MarkerPrefix).Append (markers.Count).Append ("=0");
				builder.Append (html, suffix, end - suffix);
				// Terminate the value before a slash so it cannot absorb a self-closing flag or attributes.
				if (end < html.Length && html[end] != '>')
					builder.Append (' ');
				markers.Add (tags[offset]);
				index = end;
			}

			builder.Append (html, index, html.Length - index);

			return builder.ToString ();
		}
	}
}
