//
// HtmlTagContextStack.cs
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

using System;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// The set of currently-open elements tracked while filtering untrusted HTML.
	/// </summary>
	/// <remarks>
	/// <para>Converters that pass HTML through an <see cref="HtmlTagCallback"/> need to answer two questions for
	/// every token: "is any open element suppressing its inner content?" and "which open element (if any) does
	/// this end tag close?". Answering them by scanning a flat list of open elements costs O(depth) per token,
	/// which allows a small, maliciously-crafted document (e.g. 100,000 unclosed <c>&lt;b&gt;</c> tags
	/// followed by 100,000 unmatched end tags) to consume minutes of CPU time.</para>
	/// <para>This class answers both questions in O(1): it keeps a count of the open elements that suppress
	/// their inner content, and it chains the open elements by (case-insensitive) tag name so that the most
	/// recently opened element with a given name can be found and removed without scanning.</para>
	/// <para>Elements are not implicitly closed when an end tag for an outer element is encountered; just
	/// like the list-based implementation that this replaces, only the innermost element whose name matches
	/// the end tag is removed.</para>
	/// </remarks>
	sealed class HtmlTagContextStack<T> where T : HtmlTagContext
	{
		sealed class Node
		{
			public readonly T Context;
			public readonly bool SuppressInnerContent;
			public readonly Node? Next;

			public Node (T context, Node? next)
			{
				// Snapshot SuppressInnerContent when the element is opened so that the suppression count
				// remains consistent even if the context is modified after it has been pushed.
				SuppressInnerContent = context.SuppressInnerContent;
				Context = context;
				Next = next;
			}
		}

		// Maps a tag name to the innermost open element with that name. Each node links to the next
		// (outer) open element with the same name. HTML tag names are ASCII case-insensitive.
		readonly Dictionary<string, Node> open = new Dictionary<string, Node> (StringComparer.OrdinalIgnoreCase);
		int suppressed;

		/// <summary>
		/// Get whether any open element is suppressing its inner content.
		/// </summary>
		public bool SuppressContent {
			get { return suppressed > 0; }
		}

		/// <summary>
		/// Push a newly-opened element.
		/// </summary>
		/// <param name="context">The tag context of the opened element.</param>
		public void Push (T context)
		{
			var name = context.TagName;

			open.TryGetValue (name, out var next);

			var node = new Node (context, next);

			if (node.SuppressInnerContent)
				suppressed++;

			open[name] = node;
		}

		/// <summary>
		/// Pop the innermost open element with the specified tag name.
		/// </summary>
		/// <param name="name">The name of the end tag.</param>
		/// <returns>The tag context of the closed element, or <see langword="null"/> if no element with that name is open.</returns>
		public T? Pop (string name)
		{
			if (!open.TryGetValue (name, out var node))
				return null;

			if (node.Next != null)
				open[name] = node.Next;
			else
				open.Remove (name);

			if (node.SuppressInnerContent)
				suppressed--;

			return node.Context;
		}
	}
}
