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
	/// An approximation of the HTML tree-construction stack of open elements.
	/// </summary>
	/// <remarks>
	/// <para>The HTML tokenizer cannot decide which state to switch to after a start tag without knowing how
	/// the tree builder would handle that tag. For example, <c>&lt;style&gt;</c> switches the tokenizer into the
	/// RAWTEXT state in HTML content but not in SVG or MathML content, and <c>&lt;![CDATA[</c> only starts a
	/// CDATA section in SVG or MathML content. If the tokenizer gets this wrong, markup that a browser would
	/// treat as a tag can be hidden inside what the tokenizer thinks is a comment or raw text (and vice versa),
	/// which would allow it to bypass an <see cref="HtmlToHtml.HtmlTagCallback"/>.</para>
	/// <para>This class tracks just enough of the tree-construction algorithm (see the "tree construction"
	/// section of the WHATWG HTML Living Standard) to determine whether a start tag is processed using the
	/// rules for HTML content or the rules for foreign content. It models the stack of open elements
	/// (including implied end tags, the various "has an element in scope" checks, the "special" category,
	/// implied table structure, and the rules for breaking out of foreign content) but does not model
	/// the list of active formatting elements, the adoption agency algorithm, foster parenting, the
	/// select or template insertion modes, or the document's head/body structure.</para>
	/// <para>To guard against resource exhaustion, consecutive identical entries are folded into a single
	/// entry with a repeat count, and every query is O(1): each entry caches the index of the nearest entry
	/// at or below it that satisfies each scope/category predicate, and the index of the next lower entry with
	/// the same name.</para>
	/// <para>The number of entries is also limited (see <see cref="MaxDepth"/>). Simply ignoring elements beyond
	/// the limit is not safe: an untracked <c>&lt;svg&gt;</c> or <c>&lt;math&gt;</c> element would cause the
	/// tokenizer to disagree with a browser about how the rest of the document should be tokenized. Instead,
	/// <see cref="DepthExceeded"/> gets set so that the tokenizer can fail closed.</para>
	/// </remarks>
	sealed class HtmlOpenElementStack
	{
		[Flags]
		enum ElementFlags : ushort
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

		struct Entry
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
		/// Get whether the adjusted current node is an element in the SVG or MathML namespace.
		/// </summary>
		/// <remarks>
		/// This is the condition under which a <c>&lt;![CDATA[</c> markup declaration starts a CDATA section.
		/// </remarks>
		public bool IsInForeignContent {
			get { return count > 0 && EntryAt (count - 1).Namespace != HtmlNamespace.Html; }
		}

		#region Stack primitives

		ref Entry EntryAt (int index)
		{
			return ref chunks[index >> ChunkShift]![index & ChunkMask];
		}

		int Top {
			get { return count - 1; }
		}

		int TopSpecial {
			get { return count > 0 ? EntryAt (count - 1).PrevSpecial : -1; }
		}

		int TopScope {
			get { return count > 0 ? EntryAt (count - 1).PrevScope : -1; }
		}

		int TopListItemScope {
			get { return count > 0 ? EntryAt (count - 1).PrevListItemScope : -1; }
		}

		int TopButtonScope {
			get { return count > 0 ? EntryAt (count - 1).PrevButtonScope : -1; }
		}

		int TopTableScope {
			get { return count > 0 ? EntryAt (count - 1).PrevTableScope : -1; }
		}

		int TopHeading {
			get { return count > 0 ? EntryAt (count - 1).PrevHeading : -1; }
		}

		int TopListItemStop {
			get { return count > 0 ? EntryAt (count - 1).PrevListItemStop : -1; }
		}

		int TopHtml {
			get { return count > 0 ? EntryAt (count - 1).PrevHtml : -1; }
		}

		int TopHtmlOrIntegrationPoint {
			get { return count > 0 ? EntryAt (count - 1).PrevHtmlOrIntegrationPoint : -1; }
		}

		bool IsTopHtml (HtmlTagId id)
		{
			return count > 0 && EntryAt (count - 1).Namespace == HtmlNamespace.Html && EntryAt (count - 1).Id == id;
		}

		int IndexOf (HtmlTagId id)
		{
			return htmlIds[(int) id];
		}

		int IndexOf (HtmlNamespace ns, HtmlTagId id, string name)
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

		static int Max (int a, int b)
		{
			return a > b ? a : b;
		}

		static int Max (int a, int b, int c)
		{
			return Max (Max (a, b), c);
		}

		// Returns whether the element at the given index is "in scope" given the index of the topmost scope boundary.
		static bool InScope (int index, int boundary)
		{
			return index >= 0 && index >= boundary;
		}

		void Push (string name, HtmlTagId id, HtmlNamespace ns, ElementFlags flags)
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
		void PopAbove (int index)
		{
			while (count - 1 > index)
				RemoveTopEntry ();
		}

		// Pop elements until (one instance of) the element at the specified index has been popped.
		void PopThrough (int index)
		{
			PopAbove (index);

			if (EntryAt (index).Count > 1)
				EntryAt (index).Count--;
			else
				RemoveTopEntry ();
		}

		#endregion

		#region Element classification

		static bool IsHeading (HtmlTagId id)
		{
			return id >= HtmlTagId.H1 && id <= HtmlTagId.H6;
		}

		static ElementFlags GetHtmlFlags (HtmlTagId id, string name)
		{
			switch (id) {
			case HtmlTagId.Applet:
			case HtmlTagId.Caption:
			case HtmlTagId.Marquee:
			case HtmlTagId.Object:
			case HtmlTagId.TD:
			case HtmlTagId.TH:
				return ElementFlags.Special | ElementFlags.Scope | ElementFlags.ListItemStop;
			case HtmlTagId.Html:
			case HtmlTagId.Table:
				return ElementFlags.Special | ElementFlags.Scope | ElementFlags.TableScope | ElementFlags.ListItemStop;
			case HtmlTagId.OL:
			case HtmlTagId.UL:
				return ElementFlags.Special | ElementFlags.ListItemScope | ElementFlags.ListItemStop;
			case HtmlTagId.Button:
				return ElementFlags.Special | ElementFlags.ButtonScope | ElementFlags.ListItemStop;
			case HtmlTagId.H1:
			case HtmlTagId.H2:
			case HtmlTagId.H3:
			case HtmlTagId.H4:
			case HtmlTagId.H5:
			case HtmlTagId.H6:
				return ElementFlags.Special | ElementFlags.Heading | ElementFlags.ListItemStop;
			case HtmlTagId.Address:
			case HtmlTagId.Div:
			case HtmlTagId.P:
				return ElementFlags.Special;
			case HtmlTagId.Area:
			case HtmlTagId.Article:
			case HtmlTagId.Aside:
			case HtmlTagId.Base:
			case HtmlTagId.BaseFont:
			case HtmlTagId.BGSound:
			case HtmlTagId.BlockQuote:
			case HtmlTagId.Body:
			case HtmlTagId.Br:
			case HtmlTagId.Center:
			case HtmlTagId.Col:
			case HtmlTagId.ColGroup:
			case HtmlTagId.DD:
			case HtmlTagId.Details:
			case HtmlTagId.Dir:
			case HtmlTagId.DL:
			case HtmlTagId.DT:
			case HtmlTagId.Embed:
			case HtmlTagId.FieldSet:
			case HtmlTagId.FigCaption:
			case HtmlTagId.Figure:
			case HtmlTagId.Footer:
			case HtmlTagId.Form:
			case HtmlTagId.Frame:
			case HtmlTagId.FrameSet:
			case HtmlTagId.Head:
			case HtmlTagId.Header:
			case HtmlTagId.HR:
			case HtmlTagId.IFrame:
			case HtmlTagId.Image:
			case HtmlTagId.Input:
			case HtmlTagId.Keygen:
			case HtmlTagId.LI:
			case HtmlTagId.Link:
			case HtmlTagId.Listing:
			case HtmlTagId.Main:
			case HtmlTagId.Menu:
			case HtmlTagId.Meta:
			case HtmlTagId.Nav:
			case HtmlTagId.NoEmbed:
			case HtmlTagId.NoFrames:
			case HtmlTagId.NoScript:
			case HtmlTagId.Param:
			case HtmlTagId.PlainText:
			case HtmlTagId.Pre:
			case HtmlTagId.Script:
			case HtmlTagId.Section:
			case HtmlTagId.Select:
			case HtmlTagId.Source:
			case HtmlTagId.Style:
			case HtmlTagId.Summary:
			case HtmlTagId.TBody:
			case HtmlTagId.TextArea:
			case HtmlTagId.Tfoot:
			case HtmlTagId.THead:
			case HtmlTagId.Title:
			case HtmlTagId.TR:
			case HtmlTagId.Track:
			case HtmlTagId.Wbr:
			case HtmlTagId.Xmp:
				return ElementFlags.Special | ElementFlags.ListItemStop;
			case HtmlTagId.Unknown:
				if (name.Equals ("template", StringComparison.OrdinalIgnoreCase))
					return ElementFlags.Special | ElementFlags.Scope | ElementFlags.TableScope | ElementFlags.ListItemStop;

				if (name.Equals ("hgroup", StringComparison.OrdinalIgnoreCase) || name.Equals ("search", StringComparison.OrdinalIgnoreCase))
					return ElementFlags.Special | ElementFlags.ListItemStop;

				return ElementFlags.None;
			default:
				return ElementFlags.None;
			}
		}

		static ElementFlags GetForeignFlags (HtmlNamespace ns, HtmlTagToken tag)
		{
			const ElementFlags IntegrationPoint = ElementFlags.Special | ElementFlags.Scope | ElementFlags.ListItemStop;
			var name = tag.Name;

			if (ns == HtmlNamespace.MathML) {
				switch (name.Length) {
				case 2:
					if (name.Equals ("mi", StringComparison.OrdinalIgnoreCase) || name.Equals ("mo", StringComparison.OrdinalIgnoreCase) ||
						name.Equals ("mn", StringComparison.OrdinalIgnoreCase) || name.Equals ("ms", StringComparison.OrdinalIgnoreCase))
						return IntegrationPoint | ElementFlags.MathMLTextIntegrationPoint;
					break;
				case 5:
					if (name.Equals ("mtext", StringComparison.OrdinalIgnoreCase))
						return IntegrationPoint | ElementFlags.MathMLTextIntegrationPoint;
					break;
				case 14:
					if (name.Equals ("annotation-xml", StringComparison.OrdinalIgnoreCase)) {
						for (int i = 0; i < tag.Attributes.Count; i++) {
							var attr = tag.Attributes[i];

							if (attr.Name.Equals ("encoding", StringComparison.OrdinalIgnoreCase)) {
								if (attr.Value != null && (attr.Value.Equals ("text/html", StringComparison.OrdinalIgnoreCase) ||
									attr.Value.Equals ("application/xhtml+xml", StringComparison.OrdinalIgnoreCase)))
									return IntegrationPoint | ElementFlags.HtmlIntegrationPoint;
								break;
							}
						}

						return IntegrationPoint;
					}
					break;
				}
			} else if (ns == HtmlNamespace.Svg) {
				if (name.Equals ("foreignObject", StringComparison.OrdinalIgnoreCase) || name.Equals ("desc", StringComparison.OrdinalIgnoreCase) ||
					name.Equals ("title", StringComparison.OrdinalIgnoreCase))
					return IntegrationPoint | ElementFlags.HtmlIntegrationPoint;
			}

			return ElementFlags.None;
		}

		// Start tags that cause the parser to break out of foreign content.
		static bool IsForeignContentBreakout (HtmlTagToken tag)
		{
			switch (tag.Id) {
			case HtmlTagId.B: case HtmlTagId.Big: case HtmlTagId.BlockQuote: case HtmlTagId.Body: case HtmlTagId.Br:
			case HtmlTagId.Center: case HtmlTagId.Code: case HtmlTagId.DD: case HtmlTagId.Div: case HtmlTagId.DL:
			case HtmlTagId.DT: case HtmlTagId.EM: case HtmlTagId.Embed: case HtmlTagId.H1: case HtmlTagId.H2:
			case HtmlTagId.H3: case HtmlTagId.H4: case HtmlTagId.H5: case HtmlTagId.H6: case HtmlTagId.Head:
			case HtmlTagId.HR: case HtmlTagId.I: case HtmlTagId.Image: case HtmlTagId.LI: case HtmlTagId.Listing:
			case HtmlTagId.Menu: case HtmlTagId.Meta: case HtmlTagId.NoBR: case HtmlTagId.OL: case HtmlTagId.P:
			case HtmlTagId.Pre: case HtmlTagId.Ruby: case HtmlTagId.S: case HtmlTagId.Small: case HtmlTagId.Span:
			case HtmlTagId.Strong: case HtmlTagId.Strike: case HtmlTagId.Sub: case HtmlTagId.Sup: case HtmlTagId.Table:
			case HtmlTagId.TT: case HtmlTagId.U: case HtmlTagId.UL: case HtmlTagId.Var:
				return true;
			case HtmlTagId.Font:
				for (int i = 0; i < tag.Attributes.Count; i++) {
					switch (tag.Attributes[i].Id) {
					case HtmlAttributeId.Color:
					case HtmlAttributeId.Face:
					case HtmlAttributeId.Size:
						return true;
					}
				}
				return false;
			default:
				return false;
			}
		}

		static bool IsVoidElement (HtmlTagId id, string name)
		{
			switch (id) {
			case HtmlTagId.BaseFont:
			case HtmlTagId.BGSound:
			case HtmlTagId.Frame:
				return true;
			case HtmlTagId.Unknown:
				return name.Equals ("image", StringComparison.OrdinalIgnoreCase);
			default:
				return id.IsEmptyElement ();
			}
		}

		#endregion

		#region Start tags

		/// <summary>
		/// Update the stack of open elements for a start tag.
		/// </summary>
		/// <remarks>
		/// Updates the stack of open elements for a start tag.
		/// </remarks>
		/// <returns><see langword="true" /> if the start tag was processed using the rules for HTML content (in which case
		/// the tokenizer should switch to the appropriate RCDATA/RAWTEXT/script/PLAINTEXT state for the tag); otherwise,
		/// <see langword="false" /> if it was processed using the rules for foreign content.</returns>
		/// <param name="tag">The start tag.</param>
		public bool ProcessStartTag (HtmlTagToken tag)
		{
			if (count > 0) {
				ref var top = ref EntryAt (count - 1);

				if (top.Namespace != HtmlNamespace.Html) {
					bool html;

					if ((top.Flags & ElementFlags.HtmlIntegrationPoint) != 0) {
						html = true;
					} else if ((top.Flags & ElementFlags.MathMLTextIntegrationPoint) != 0) {
						html = !tag.Name.Equals ("mglyph", StringComparison.OrdinalIgnoreCase) && !tag.Name.Equals ("malignmark", StringComparison.OrdinalIgnoreCase);
					} else if (top.Namespace == HtmlNamespace.MathML && top.Name.Equals ("annotation-xml", StringComparison.OrdinalIgnoreCase)) {
						html = tag.Name.Equals ("svg", StringComparison.OrdinalIgnoreCase);
					} else {
						html = false;
					}

					if (!html) {
						if (IsForeignContentBreakout (tag)) {
							// Pop until the current node is a MathML text integration point, an HTML integration point,
							// or an element in the HTML namespace, then reprocess the token using the HTML rules.
							PopAbove (TopHtmlOrIntegrationPoint);
						} else {
							// Insert a foreign element for the token in the same namespace as the adjusted current node.
							var ns = top.Namespace;

							if (!tag.IsEmptyElement)
								Push (tag.Name, HtmlTagId.Unknown, ns, GetForeignFlags (ns, tag));

							return false;
						}
					}
				}
			}

			ProcessHtmlStartTag (tag);

			return true;
		}

		void ClosePInButtonScope ()
		{
			int p = IndexOf (HtmlTagId.P);

			if (InScope (p, TopButtonScope))
				PopThrough (p);
		}

		void CloseListItem (int item)
		{
			// Walk the stack from the current node: if an item is found before a special element other than
			// address, div or p, close it.
			if (item >= 0 && item >= TopListItemStop)
				PopThrough (item);
		}

		// Returns the index of the current table if the topmost table-scope boundary is a table element; otherwise, -1.
		int CurrentTable ()
		{
			int table = IndexOf (HtmlTagId.Table);

			return table >= 0 && table == TopTableScope ? table : -1;
		}

		void PushHtml (HtmlTagToken tag)
		{
			Push (tag.Name, tag.Id, HtmlNamespace.Html, GetHtmlFlags (tag.Id, tag.Name));
		}

		void PushImplied (HtmlTagId id)
		{
			Push (id.ToHtmlTagName (), id, HtmlNamespace.Html, GetHtmlFlags (id, string.Empty));
		}

		void ProcessHtmlStartTag (HtmlTagToken tag)
		{
			var id = tag.Id;
			int index, table;

			switch (id) {
			case HtmlTagId.Html:
			case HtmlTagId.Head:
			case HtmlTagId.Body:
				// These elements are never pushed more than once, so their start tags have no effect on the stack.
				return;
			case HtmlTagId.Address: case HtmlTagId.Article: case HtmlTagId.Aside: case HtmlTagId.BlockQuote:
			case HtmlTagId.Center: case HtmlTagId.Details: case HtmlTagId.Dialog: case HtmlTagId.Dir:
			case HtmlTagId.Div: case HtmlTagId.DL: case HtmlTagId.FieldSet: case HtmlTagId.FigCaption:
			case HtmlTagId.Figure: case HtmlTagId.Footer: case HtmlTagId.Header: case HtmlTagId.Main:
			case HtmlTagId.Menu: case HtmlTagId.Nav: case HtmlTagId.OL: case HtmlTagId.P:
			case HtmlTagId.Section: case HtmlTagId.Summary: case HtmlTagId.UL: case HtmlTagId.Pre:
			case HtmlTagId.Listing: case HtmlTagId.Form: case HtmlTagId.PlainText: case HtmlTagId.Xmp:
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.H1: case HtmlTagId.H2: case HtmlTagId.H3:
			case HtmlTagId.H4: case HtmlTagId.H5: case HtmlTagId.H6:
				ClosePInButtonScope ();

				if (count > 0 && EntryAt (count - 1).Namespace == HtmlNamespace.Html && IsHeading (EntryAt (count - 1).Id))
					PopThrough (Top);

				PushHtml (tag);
				return;
			case HtmlTagId.HR:
				ClosePInButtonScope ();
				return;
			case HtmlTagId.LI:
				CloseListItem (IndexOf (HtmlTagId.LI));
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.DD:
			case HtmlTagId.DT:
				CloseListItem (Max (IndexOf (HtmlTagId.DD), IndexOf (HtmlTagId.DT)));
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.Button:
				index = IndexOf (HtmlTagId.Button);

				if (InScope (index, TopScope))
					PopThrough (index);

				PushHtml (tag);
				return;
			case HtmlTagId.A:
				// Approximates the adoption agency algorithm being run for a nested <a>: when there is no
				// special element above the open <a>, the open <a> gets popped.
				index = IndexOf (HtmlTagId.A);

				if (index >= 0 && TopSpecial < index)
					PopThrough (index);

				PushHtml (tag);
				return;
			case HtmlTagId.Option:
			case HtmlTagId.OptGroup:
				if (IsTopHtml (HtmlTagId.Option))
					PopThrough (Top);

				PushHtml (tag);
				return;
			case HtmlTagId.Table:
				table = CurrentTable ();

				if (table >= 0 && Max (IndexOf (HtmlTagId.TD), IndexOf (HtmlTagId.TH), IndexOf (HtmlTagId.Caption)) < table) {
					// In the "in table", "in table body" or "in row" insertion modes, a <table> start tag closes the current table.
					PopThrough (table);
				} else {
					ClosePInButtonScope ();
				}

				PushHtml (tag);
				return;
			case HtmlTagId.Caption:
			case HtmlTagId.ColGroup:
			case HtmlTagId.Col:
			case HtmlTagId.TBody:
			case HtmlTagId.THead:
			case HtmlTagId.Tfoot:
			case HtmlTagId.TR:
			case HtmlTagId.TD:
			case HtmlTagId.TH:
				ProcessTableStructureStartTag (tag);
				return;
			case HtmlTagId.Unknown:
				if (tag.Name.Equals ("svg", StringComparison.OrdinalIgnoreCase)) {
					if (!tag.IsEmptyElement)
						Push (tag.Name, HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);
					return;
				}

				if (tag.Name.Equals ("math", StringComparison.OrdinalIgnoreCase)) {
					if (!tag.IsEmptyElement)
						Push (tag.Name, HtmlTagId.Unknown, HtmlNamespace.MathML, ElementFlags.None);
					return;
				}

				if (tag.Name.Equals ("hgroup", StringComparison.OrdinalIgnoreCase) || tag.Name.Equals ("search", StringComparison.OrdinalIgnoreCase)) {
					ClosePInButtonScope ();
					PushHtml (tag);
					return;
				}
				break;
			}

			// Note: The self-closing flag is ignored for HTML elements that are not void elements.
			if (!IsVoidElement (id, tag.Name))
				PushHtml (tag);
		}

		void ProcessTableStructureStartTag (HtmlTagToken tag)
		{
			int table = CurrentTable ();
			int index;

			// Table structure start tags outside of a table are ignored.
			if (table < 0)
				return;

			switch (tag.Id) {
			case HtmlTagId.Caption:
			case HtmlTagId.ColGroup:
				PopAbove (table);
				PushHtml (tag);
				break;
			case HtmlTagId.Col:
				PopAbove (table);
				PushImplied (HtmlTagId.ColGroup);
				break;
			case HtmlTagId.TBody:
			case HtmlTagId.THead:
			case HtmlTagId.Tfoot:
				PopAbove (table);
				PushHtml (tag);
				break;
			case HtmlTagId.TR:
				index = Max (IndexOf (HtmlTagId.TBody), IndexOf (HtmlTagId.THead), IndexOf (HtmlTagId.Tfoot));

				if (index > table) {
					PopAbove (index);
				} else {
					PopAbove (table);
					PushImplied (HtmlTagId.TBody);
				}

				PushHtml (tag);
				break;
			default: // td, th
				index = IndexOf (HtmlTagId.TR);

				if (index > table) {
					PopAbove (index);
				} else {
					index = Max (IndexOf (HtmlTagId.TBody), IndexOf (HtmlTagId.THead), IndexOf (HtmlTagId.Tfoot));

					if (index > table) {
						PopAbove (index);
					} else {
						PopAbove (table);
						PushImplied (HtmlTagId.TBody);
					}

					PushImplied (HtmlTagId.TR);
				}

				PushHtml (tag);
				break;
			}
		}

		#endregion

		#region End tags

		/// <summary>
		/// Update the stack of open elements for an end tag.
		/// </summary>
		/// <remarks>
		/// Updates the stack of open elements for an end tag.
		/// </remarks>
		/// <param name="tag">The end tag.</param>
		public void ProcessEndTag (HtmlTagToken tag)
		{
			if (count > 0 && EntryAt (count - 1).Namespace != HtmlNamespace.Html) {
				if (tag.Id == HtmlTagId.Br || tag.Id == HtmlTagId.P) {
					// </br> and </p> break out of foreign content.
					PopAbove (TopHtmlOrIntegrationPoint);
				} else {
					// Walk down the stack looking for a foreign element with a matching name, stopping at the first
					// element in the HTML namespace. If a match is found, pop through it; otherwise, process the
					// token using the rules for HTML content.
					int index = IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, tag.Name);

					if (index > TopHtml) {
						PopThrough (index);
						return;
					}
				}
			}

			ProcessHtmlEndTag (tag);
		}

		void ProcessHtmlEndTag (HtmlTagToken tag)
		{
			int index;

			switch (tag.Id) {
			case HtmlTagId.Html:
			case HtmlTagId.Head:
			case HtmlTagId.Body:
			case HtmlTagId.Br:
				return;
			case HtmlTagId.P:
				index = IndexOf (HtmlTagId.P);

				if (InScope (index, TopButtonScope))
					PopThrough (index);
				return;
			case HtmlTagId.LI:
				index = IndexOf (HtmlTagId.LI);

				if (InScope (index, TopListItemScope))
					PopThrough (index);
				return;
			case HtmlTagId.H1: case HtmlTagId.H2: case HtmlTagId.H3:
			case HtmlTagId.H4: case HtmlTagId.H5: case HtmlTagId.H6:
				// Any heading end tag closes the topmost heading element.
				index = TopHeading;

				if (InScope (index, TopScope))
					PopThrough (index);
				return;
			case HtmlTagId.Address: case HtmlTagId.Article: case HtmlTagId.Aside: case HtmlTagId.BlockQuote:
			case HtmlTagId.Button: case HtmlTagId.Center: case HtmlTagId.Details: case HtmlTagId.Dialog:
			case HtmlTagId.Dir: case HtmlTagId.Div: case HtmlTagId.DL: case HtmlTagId.FieldSet:
			case HtmlTagId.FigCaption: case HtmlTagId.Figure: case HtmlTagId.Footer: case HtmlTagId.Header:
			case HtmlTagId.Listing: case HtmlTagId.Main: case HtmlTagId.Menu: case HtmlTagId.Nav:
			case HtmlTagId.OL: case HtmlTagId.Pre: case HtmlTagId.Section: case HtmlTagId.Summary:
			case HtmlTagId.UL: case HtmlTagId.Form: case HtmlTagId.Applet: case HtmlTagId.Marquee:
			case HtmlTagId.Object: case HtmlTagId.DD: case HtmlTagId.DT:
				index = IndexOf (tag.Id);

				if (InScope (index, TopScope))
					PopThrough (index);
				return;
			case HtmlTagId.Table: case HtmlTagId.TBody: case HtmlTagId.Tfoot: case HtmlTagId.THead:
			case HtmlTagId.TR: case HtmlTagId.TD: case HtmlTagId.TH: case HtmlTagId.Caption:
			case HtmlTagId.ColGroup:
				index = IndexOf (tag.Id);

				if (InScope (index, TopTableScope))
					PopThrough (index);
				return;
			case HtmlTagId.A: case HtmlTagId.B: case HtmlTagId.Big: case HtmlTagId.Code:
			case HtmlTagId.EM: case HtmlTagId.Font: case HtmlTagId.I: case HtmlTagId.NoBR:
			case HtmlTagId.S: case HtmlTagId.Small: case HtmlTagId.Strike: case HtmlTagId.Strong:
			case HtmlTagId.TT: case HtmlTagId.U:
				// Approximates the adoption agency algorithm: when the formatting element is in scope and there is
				// no "furthest block" (a special element above it), the formatting element and everything above it
				// gets popped. Otherwise, the stack is left as-is.
				index = IndexOf (tag.Id);

				if (InScope (index, TopScope) && TopSpecial < index)
					PopThrough (index);
				return;
			case HtmlTagId.Unknown:
				if (tag.Name.Equals ("hgroup", StringComparison.OrdinalIgnoreCase) || tag.Name.Equals ("search", StringComparison.OrdinalIgnoreCase)) {
					index = IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, tag.Name);

					if (InScope (index, TopScope))
						PopThrough (index);
					return;
				}

				if (tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase)) {
					index = IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, tag.Name);

					if (index >= 0)
						PopThrough (index);
					return;
				}
				break;
			}

			// Any other end tag: walk down the stack looking for an HTML element with a matching name,
			// giving up if a special element is found first.
			index = IndexOf (HtmlNamespace.Html, tag.Id, tag.Name);

			if (index >= 0 && index >= TopSpecial)
				PopThrough (index);
		}

		#endregion
	}
}
