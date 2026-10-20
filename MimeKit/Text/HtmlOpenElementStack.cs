//
// HtmlOpenElementStack.cs
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

using MimeKit.Utils;

namespace MimeKit.Text {
	/// <summary>
	/// Storage and scope queries for the stack of open HTML elements.
	/// </summary>
	/// <remarks>
	/// <para><see cref="HtmlTreeConstruction"/> supplies element classifications and decides which entries to
	/// push, pop or remove. This class does not choose insertion modes or tokenizer states.</para>
	/// <para>Consecutive identical entries are folded into a single entry with a repeat count. Cached indexes
	/// support scope and category queries without walking the stack, and entries are allocated in small chunks.</para>
	/// </remarks>
	sealed class HtmlOpenElementStack
	{
		[Flags]
		public enum ElementFlags : ushort
		{
			None = 0,

			// The element is in the "special" category.
			Special = 1 << 0,

			// The element terminates the "has an element in scope" walk.
			Scope = 1 << 1,

			// The element additionally terminates the "has an element in list item scope" walk (ol, ul).
			ListItemScope = 1 << 2,

			// The element additionally terminates the "has an element in button scope" walk (button).
			ButtonScope = 1 << 3,

			// The element terminates the "has an element in table scope" walk (html, table, template).
			TableScope = 1 << 4,

			// The element is an HTML h1-h6 element.
			Heading = 1 << 5,

			// The element terminates the walk performed for li, dd and dt start tags
			// (special elements other than address, div and p).
			ListItemStop = 1 << 6,

			// The element is a MathML text integration point (mi, mo, mn, ms, mtext).
			MathMLTextIntegrationPoint = 1 << 7,

			// The element is an HTML integration point (SVG foreignObject, desc, title and
			// MathML annotation-xml with an HTML encoding).
			HtmlIntegrationPoint = 1 << 8,
		}

		public struct Entry
		{
			public string Name;
			public HtmlTagId Id;
			public HtmlNamespace Namespace;
			public ElementFlags Flags;

			// The number of consecutive identical elements represented by this entry.
			public int Count;

			// The index of the next lower entry with the same name in the same lookup table (or -1).
			public int PrevSameName;

			// The index of the nearest entry at or below this one that satisfies each predicate (or -1).
			public int PrevSpecial;
			public int PrevScope;
			public int PrevListItemScope;
			public int PrevButtonScope;
			public int PrevTableScope;
			public int PrevHeading;
			public int PrevListItemStop;
			public int PrevHtml;
			public int PrevHtmlOrIntegrationPoint;
		}

		readonly Dictionary<string, int> foreignNames = new Dictionary<string, int> (MimeUtils.OrdinalIgnoreCase);
		readonly Dictionary<string, int> htmlNames = new Dictionary<string, int> (MimeUtils.OrdinalIgnoreCase);
		readonly int[] htmlIds = new int[(int) HtmlTagId.Xmp + 1];

		// Note: Entries are stored in fixed-size chunks rather than in a single array that gets resized as the stack
		// grows. This avoids copying the entire stack each time it grows and keeps every allocation well below the
		// large object heap threshold, no matter how deep the stack gets.
		const int ChunkShift = 6;
		const int ChunkSize = 1 << ChunkShift;
		const int ChunkMask = ChunkSize - 1;

		Entry[]?[] chunks = new Entry[]?[4];
		int maxDepth;
		int count;

		public HtmlOpenElementStack (int maxDepth)
		{
			for (int i = 0; i < htmlIds.Length; i++)
				htmlIds[i] = -1;

			this.maxDepth = maxDepth;
		}

		/// <summary>
		/// Get or set the maximum number of entries that the stack can hold.
		/// </summary>
		/// <remarks>
		/// Consecutive identical elements share a single entry and so only count once towards this limit.
		/// </remarks>
		public int MaxDepth {
			get { return maxDepth; }
			set { maxDepth = value; }
		}

		/// <summary>
		/// Get whether an element could not be pushed because the stack had reached its maximum depth.
		/// </summary>
		/// <remarks>
		/// Once this happens, the stack no longer reflects the document's structure and can no longer be trusted.
		/// </remarks>
		public bool DepthExceeded {
			get; private set;
		}

		/// <summary>
		/// An element (or a run of consecutive identical elements) that was implicitly closed.
		/// </summary>
		public readonly struct ClosedElement
		{
			public readonly string Name;
			public readonly int Count;
			public readonly bool IsForeign;

			public ClosedElement (string name, int count, bool isForeign)
			{
				Name = name;
				Count = count;
				IsForeign = isForeign;
			}
		}

		List<ClosedElement>? closedElements;

		/// <summary>
		/// Get or set whether to track the elements that get implicitly closed.
		/// </summary>
		/// <remarks>
		/// <para>Tags such as <c>&lt;body&gt;</c>, <c>&lt;img&gt;</c> or <c>&lt;/div&gt;</c> implicitly close the open SVG
		/// and MathML elements. If a consumer such as <see cref="HtmlToHtml"/> does not write the tag that caused this
		/// (e.g. because it was removed by an <see cref="HtmlToHtml.HtmlTagCallback"/>), then the output would remain in
		/// foreign content and a browser would interpret the rest of it differently than the tokenizer did.</para>
		/// <para>When enabled, <see cref="ClosedElements"/> gets populated with the elements that were implicitly
		/// closed (innermost first) so that the consumer can write explicit end tags for them.</para>
		/// </remarks>
		public bool TrackClosedElements {
			get { return closedElements != null; }
			set { closedElements = value ? closedElements ?? new List<ClosedElement> () : null; }
		}

		/// <summary>
		/// Get the elements removed by pops since the last call to <see cref="ClearClosedElements"/> (innermost first).
		/// </summary>
		/// <remarks>
		/// The element that is explicitly closed by an end tag is not included.
		/// </remarks>
		public IReadOnlyList<ClosedElement> ClosedElements {
			get { return (IReadOnlyList<ClosedElement>?) closedElements ?? Array.Empty<ClosedElement> (); }
		}

		/// <summary>
		/// Clear the list of implicitly closed elements.
		/// </summary>
		public void ClearClosedElements ()
		{
			closedElements?.Clear ();
		}

		public int Count {
			get { return count; }
		}

		public ref readonly Entry this[int index] {
			get { return ref EntryAt (index); }
		}

		#region Stack primitives

		ref Entry EntryAt (int index)
		{
			return ref chunks[index >> ChunkShift]![index & ChunkMask];
		}

		public int Top {
			get { return count - 1; }
		}

		public int TopSpecial {
			get { return count > 0 ? EntryAt (count - 1).PrevSpecial : -1; }
		}

		public int TopScope {
			get { return count > 0 ? EntryAt (count - 1).PrevScope : -1; }
		}

		public int TopListItemScope {
			get { return count > 0 ? EntryAt (count - 1).PrevListItemScope : -1; }
		}

		public int TopButtonScope {
			get { return count > 0 ? EntryAt (count - 1).PrevButtonScope : -1; }
		}

		public int TopTableScope {
			get { return count > 0 ? EntryAt (count - 1).PrevTableScope : -1; }
		}

		public int TopHeading {
			get { return count > 0 ? EntryAt (count - 1).PrevHeading : -1; }
		}

		public int TopListItemStop {
			get { return count > 0 ? EntryAt (count - 1).PrevListItemStop : -1; }
		}

		public int TopHtml {
			get { return count > 0 ? EntryAt (count - 1).PrevHtml : -1; }
		}

		public int TopHtmlOrIntegrationPoint {
			get { return count > 0 ? EntryAt (count - 1).PrevHtmlOrIntegrationPoint : -1; }
		}

		public bool IsTopHtml (HtmlTagId id)
		{
			return count > 0 && EntryAt (count - 1).Namespace == HtmlNamespace.Html && EntryAt (count - 1).Id == id;
		}

		public int IndexOf (HtmlTagId id)
		{
			return htmlIds[(int) id];
		}

		public int IndexOf (HtmlNamespace ns, HtmlTagId id, string name)
		{
			if (ns == HtmlNamespace.Html && id != HtmlTagId.Unknown)
				return htmlIds[(int) id];

			var table = ns == HtmlNamespace.Html ? htmlNames : foreignNames;

			return table.TryGetValue (name, out int index) ? index : -1;
		}

		void SetIndexOf (HtmlNamespace ns, HtmlTagId id, string name, int index)
		{
			if (ns == HtmlNamespace.Html && id != HtmlTagId.Unknown) {
				htmlIds[(int) id] = index;
				return;
			}

			var table = ns == HtmlNamespace.Html ? htmlNames : foreignNames;

			if (index == -1)
				table.Remove (name);
			else
				table[name] = index;
		}

		public void Push (string name, HtmlTagId id, HtmlNamespace ns, ElementFlags flags)
		{
			int prev = count - 1;

			if (prev >= 0) {
				ref var top = ref EntryAt (prev);

				// Fold consecutive identical elements into a single entry.
				if (top.Namespace == ns && top.Id == id && top.Flags == flags && (id != HtmlTagId.Unknown || string.Equals (top.Name, name, StringComparison.OrdinalIgnoreCase))) {
					if (top.Count < int.MaxValue)
						top.Count++;
					return;
				}
			}

			if (count >= maxDepth) {
				DepthExceeded = true;
				return;
			}

			int index = count;
			int chunk = index >> ChunkShift;

			if (chunk == chunks.Length)
				Array.Resize (ref chunks, chunks.Length * 2);

			chunks[chunk] ??= new Entry[ChunkSize];

			ref var entry = ref EntryAt (index);

			entry.Name = name;
			entry.Id = id;
			entry.Namespace = ns;
			entry.Flags = flags;
			entry.Count = 1;
			entry.PrevSameName = IndexOf (ns, id, name);

			if (prev >= 0) {
				ref var below = ref EntryAt (prev);

				entry.PrevSpecial = below.PrevSpecial;
				entry.PrevScope = below.PrevScope;
				entry.PrevListItemScope = below.PrevListItemScope;
				entry.PrevButtonScope = below.PrevButtonScope;
				entry.PrevTableScope = below.PrevTableScope;
				entry.PrevHeading = below.PrevHeading;
				entry.PrevListItemStop = below.PrevListItemStop;
				entry.PrevHtml = below.PrevHtml;
				entry.PrevHtmlOrIntegrationPoint = below.PrevHtmlOrIntegrationPoint;
			} else {
				entry.PrevSpecial = -1;
				entry.PrevScope = -1;
				entry.PrevListItemScope = -1;
				entry.PrevButtonScope = -1;
				entry.PrevTableScope = -1;
				entry.PrevHeading = -1;
				entry.PrevListItemStop = -1;
				entry.PrevHtml = -1;
				entry.PrevHtmlOrIntegrationPoint = -1;
			}

			if ((flags & ElementFlags.Special) != 0)
				entry.PrevSpecial = index;

			if ((flags & ElementFlags.Scope) != 0) {
				entry.PrevScope = index;
				entry.PrevListItemScope = index;
				entry.PrevButtonScope = index;
			}

			if ((flags & ElementFlags.ListItemScope) != 0)
				entry.PrevListItemScope = index;

			if ((flags & ElementFlags.ButtonScope) != 0)
				entry.PrevButtonScope = index;

			if ((flags & ElementFlags.TableScope) != 0)
				entry.PrevTableScope = index;

			if ((flags & ElementFlags.Heading) != 0)
				entry.PrevHeading = index;

			if ((flags & ElementFlags.ListItemStop) != 0)
				entry.PrevListItemStop = index;

			if (ns == HtmlNamespace.Html) {
				entry.PrevHtml = index;
				entry.PrevHtmlOrIntegrationPoint = index;
			} else if ((flags & (ElementFlags.MathMLTextIntegrationPoint | ElementFlags.HtmlIntegrationPoint)) != 0) {
				entry.PrevHtmlOrIntegrationPoint = index;
			}

			SetIndexOf (ns, id, name, index);
			count++;
		}

		void RemoveTopEntry ()
		{
			ref var entry = ref EntryAt (count - 1);

			SetIndexOf (entry.Namespace, entry.Id, entry.Name, entry.PrevSameName);
			entry = default;
			count--;
		}

		// Pop elements until the element at the specified index is the current node (or the stack is empty if index is -1).
		public void PopAbove (int index)
		{
			while (count - 1 > index) {
				if (closedElements != null) {
					ref var entry = ref EntryAt (count - 1);

					closedElements.Add (new ClosedElement (entry.Name, entry.Count, entry.Namespace != HtmlNamespace.Html));
				}

				RemoveTopEntry ();
			}
		}

		// Pop elements until (one instance of) the element at the specified index has been popped.
		public void PopThrough (int index)
		{
			PopAbove (index);

			if (EntryAt (index).Count > 1)
				EntryAt (index).Count--;
			else
				RemoveTopEntry ();
		}

		// Remove (one instance of) the element at the specified index without popping the elements above it.
		//
		// Note: The entries above it cache the indexes of the entries below them, so they get removed and pushed again.
		public void RemoveEntry (int index)
		{
			if (EntryAt (index).Count > 1) {
				EntryAt (index).Count--;
				return;
			}

			var above = new Entry[count - index - 1];

			for (int i = 0; i < above.Length; i++)
				above[i] = EntryAt (index + 1 + i);

			while (count > index)
				RemoveTopEntry ();

			for (int i = 0; i < above.Length; i++) {
				ref var saved = ref above[i];

				// Note: The entry might get folded into the entry below it (e.g. <b><form><b></form>).
				Push (saved.Name, saved.Id, saved.Namespace, saved.Flags);

				ref var top = ref EntryAt (count - 1);

				top.Count = (int) Math.Min ((long) top.Count + saved.Count - 1, int.MaxValue);
			}
		}

		// Replace a range without reporting implicit closures, rebuilding the indexes of the retained entries.
		public void ReplaceRange (int index, int length, Entry[] replacement, int replacementCount)
		{
			var above = new Entry[count - index - length];

			for (int i = 0; i < above.Length; i++)
				above[i] = EntryAt (index + length + i);

			while (count > index)
				RemoveTopEntry ();

			for (int i = 0; i < replacementCount; i++)
				PushEntry (replacement[i]);

			for (int i = 0; i < above.Length; i++)
				PushEntry (above[i]);
		}

		void PushEntry (Entry entry)
		{
			if (DepthExceeded)
				return;

			Push (entry.Name, entry.Id, entry.Namespace, entry.Flags);

			if (!DepthExceeded) {
				ref var top = ref EntryAt (count - 1);

				top.Count = (int) Math.Min ((long) top.Count + entry.Count - 1, int.MaxValue);
			}
		}

		#endregion
	}
}
