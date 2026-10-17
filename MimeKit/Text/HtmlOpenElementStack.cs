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
	/// the list of active formatting elements, the adoption agency algorithm, foster parenting, or the
	/// select or template insertion modes. Of the document's head/body structure, it only models whether the
	/// tree builder is still before the body and whether it is in the "in head noscript" insertion mode (since,
	/// when scripting is disabled, a <c>&lt;noscript&gt;</c> element within the <c>&lt;head&gt;</c> gets closed by
	/// any tag or character data that is not allowed there, unlike a <c>&lt;noscript&gt;</c> within the body), as
	/// well as the frameset-ok flag and the frameset insertion modes (since a <c>&lt;frameset&gt;</c> makes browsers
	/// ignore start tags such as <c>&lt;style&gt;</c> or <c>&lt;svg&gt;</c> that would otherwise switch the tokenizer
	/// state). It also models the form element pointer, since (outside of a <c>&lt;template&gt;</c>) a
	/// <c>&lt;/form&gt;</c> end tag removes the <c>&lt;form&gt;</c> element from the stack without closing the elements
	/// above it (such as an open <c>&lt;svg&gt;</c> element).</para>
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

		// The tree builder's insertion modes, as far as they matter for the document's head. Once the head has been
		// closed, the "after head" insertion mode processes everything that matters to this class as if in the body.
		enum InsertionMode : byte
		{
			BeforeHead,
			InHead,
			InHeadNoScript,
			AfterHead
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
		bool poppedThrough;
		InsertionMode mode;

		// The frameset-ok flag and whether a <frameset> element has replaced the <body>.
		bool framesetOk = true;
		bool inFrameset;

		// The form element pointer: whether it is set, and the index of the <form> element that it points to (or -1 if that
		// element is no longer on the stack, e.g. because it was popped by a </div> or because it was inserted in a table).
		bool formPointerSet;
		int formPointerIndex = -1;
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
		/// Get the elements that were implicitly closed by the most recently processed tag (innermost first).
		/// </summary>
		/// <remarks>
		/// The element that is explicitly closed by an end tag is not included.
		/// </remarks>
		public IReadOnlyList<ClosedElement> ClosedElements {
			get { return (IReadOnlyList<ClosedElement>?) closedElements ?? Array.Empty<ClosedElement> (); }
		}

		/// <summary>
		/// Get whether the most recently processed end tag was ignored.
		/// </summary>
		/// <remarks>
		/// An end tag is ignored if it did not close an element even though an element with the same name is open
		/// (e.g. <c>&lt;/mi&gt;</c> when an HTML element is the current node and a special element such as the
		/// MathML <c>&lt;mi&gt;</c> is in the way).
		/// </remarks>
		public bool EndTagIgnored {
			get; private set;
		}

		/// <summary>
		/// Clear the list of implicitly closed elements.
		/// </summary>
		public void ClearClosedElements ()
		{
			closedElements?.Clear ();
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

		/// <summary>
		/// Get whether an HTML <c>&lt;noscript&gt;</c> element is open.
		/// </summary>
		/// <remarks>
		/// Gets whether an HTML <c>&lt;noscript&gt;</c> element is open.
		/// </remarks>
		public bool IsNoScriptOpen {
			get { return IndexOf (HtmlTagId.NoScript) >= 0; }
		}

		/// <summary>
		/// Get or set whether the tree builder should behave as if scripting is enabled.
		/// </summary>
		/// <remarks>
		/// When scripting is disabled, a <c>&lt;noscript&gt;</c> element within the <c>&lt;head&gt;</c> gets processed
		/// using the "in head noscript" insertion mode rather than as a normal element.
		/// </remarks>
		public bool ScriptingEnabled {
			get; set;
		} = true;

		/// <summary>
		/// Get whether the tree builder needs to be told about character data.
		/// </summary>
		/// <remarks>
		/// While this is <see langword="true" />, any character in the data state (or in a CDATA section) that is not
		/// whitespace must be reported using <see cref="ProcessCharacterData"/>. This is the case until the tree builder
		/// has moved past the document's <c>&lt;head&gt;</c> and the frameset-ok flag has been set to "not ok".
		/// </remarks>
		public bool TracksCharacterData {
			get { return !inFrameset && (mode != InsertionMode.AfterHead || framesetOk); }
		}

		/// <summary>
		/// Get whether the tree builder is in the "in frameset", "after frameset" or "after after frameset" insertion mode.
		/// </summary>
		/// <remarks>
		/// <para>In these insertion modes, browsers ignore every start tag other than <c>&lt;html&gt;</c>, <c>&lt;frameset&gt;</c>,
		/// <c>&lt;frame&gt;</c> and <c>&lt;noframes&gt;</c> (as well as every end tag other than <c>&lt;/frameset&gt;</c>,
		/// <c>&lt;/noframes&gt;</c> and <c>&lt;/html&gt;</c>) and never leave them.</para>
		/// <para>Since the ignored tags do not switch the tokenizer state (e.g. <c>&lt;style&gt;</c> or <c>&lt;svg&gt;</c>), a
		/// consumer such as <see cref="HtmlToHtml"/> must not write them, since they would no longer be ignored if the
		/// <c>&lt;frameset&gt;</c> start tag itself does not get written (or if the output gets embedded in another document).</para>
		/// </remarks>
		public bool IsInFrameset {
			get { return inFrameset; }
		}

		/// <summary>
		/// Update the stack of open elements for a character in the data state that is not whitespace.
		/// </summary>
		/// <remarks>
		/// <para>Character data that is not whitespace implicitly closes the <c>&lt;head&gt;</c> (and any <c>&lt;noscript&gt;</c>
		/// element within it) and inserts the <c>&lt;body&gt;</c>.</para>
		/// <para>Any character other than U+0000 also sets the frameset-ok flag to "not ok" (browsers ignore a U+0000 in the
		/// body, but it is inserted as U+FFFD in SVG or MathML content without changing the frameset-ok flag either).</para>
		/// </remarks>
		/// <param name="c">The character.</param>
		public void ProcessCharacterData (char c)
		{
			if (inFrameset)
				return;

			if (c != '\0')
				framesetOk = false;

			// Note: In the "in template" insertion mode, character data is processed as if in the body without closing the head.
			if (IsTemplateOpen)
				return;

			if (mode == InsertionMode.InHeadNoScript)
				PopThrough (IndexOf (HtmlTagId.NoScript));

			mode = InsertionMode.AfterHead;
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

		bool IsTemplateOpen {
			get { return htmlNames.Count > 0 && IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, "template") >= 0; }
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

			if (count - 1 == formPointerIndex)
				formPointerIndex = -1;

			SetIndexOf (entry.Namespace, entry.Id, entry.Name, entry.PrevSameName);
			entry = default;
			count--;
		}

		// Pop elements until the element at the specified index is the current node (or the stack is empty if index is -1).
		void PopAbove (int index)
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
		void PopThrough (int index)
		{
			poppedThrough = true;

			PopAbove (index);

			if (EntryAt (index).Count > 1)
				EntryAt (index).Count--;
			else
				RemoveTopEntry ();
		}

		// Remove (one instance of) the element at the specified index without popping the elements above it.
		//
		// Note: The entries above it cache the indexes of the entries below them, so they get removed and pushed again.
		void RemoveEntry (int index)
		{
			poppedThrough = true;

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

		// Pop the elements that have implied end tags (dd, dt, li, optgroup, option, p, rb, rp, rt and rtc).
		void GenerateImpliedEndTags ()
		{
			int index = count - 1;

			while (index >= 0 && EntryAt (index).Namespace == HtmlNamespace.Html && HasImpliedEndTag (EntryAt (index)))
				index--;

			PopAbove (index);
		}

		static bool HasImpliedEndTag (in Entry entry)
		{
			switch (entry.Id) {
			case HtmlTagId.DD: case HtmlTagId.DT: case HtmlTagId.LI: case HtmlTagId.OptGroup:
			case HtmlTagId.Option: case HtmlTagId.P: case HtmlTagId.RP: case HtmlTagId.RT:
				return true;
			case HtmlTagId.Unknown:
				return entry.Name.Equals ("rb", StringComparison.OrdinalIgnoreCase) || entry.Name.Equals ("rtc", StringComparison.OrdinalIgnoreCase);
			default:
				return false;
			}
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

		// Start tags that, when processed using the rules for the "in body" insertion mode, set the frameset-ok flag to "not ok".
		static bool ClearsFramesetOk (HtmlTagToken tag)
		{
			switch (tag.Id) {
			case HtmlTagId.Pre: case HtmlTagId.Listing: case HtmlTagId.LI: case HtmlTagId.DD: case HtmlTagId.DT:
			case HtmlTagId.Button: case HtmlTagId.Applet: case HtmlTagId.Marquee: case HtmlTagId.Object:
			case HtmlTagId.Table: case HtmlTagId.Area: case HtmlTagId.Br: case HtmlTagId.Embed: case HtmlTagId.Image:
			case HtmlTagId.Keygen: case HtmlTagId.Wbr: case HtmlTagId.HR: case HtmlTagId.TextArea: case HtmlTagId.Xmp:
			case HtmlTagId.IFrame: case HtmlTagId.Select: case HtmlTagId.Body:
				return true;
			case HtmlTagId.Input:
				// Note: Only the first attribute with a given name counts, since browsers drop any duplicates.
				for (int i = 0; i < tag.Attributes.Count; i++) {
					var attr = tag.Attributes[i];

					if (attr.Id == HtmlAttributeId.Type)
						return !IsAsciiCaseInsensitiveMatch (attr.Value, "hidden");
				}

				return true;
			case HtmlTagId.Unknown:
				// Note: The tree builder treats an <image> start tag as an <img> start tag. A <template> start tag also sets
				// the frameset-ok flag to "not ok" (even in the <head>).
				return tag.Name.Equals ("image", StringComparison.OrdinalIgnoreCase) || tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase);
			default:
				return false;
			}
		}

		// Note: Unlike StringComparison.OrdinalIgnoreCase, this only folds the ASCII letters (e.g. "hıdden" is not "hidden").
		static bool IsAsciiCaseInsensitiveMatch (string? value, string lowercase)
		{
			if (value == null || value.Length != lowercase.Length)
				return false;

			for (int i = 0; i < value.Length; i++) {
				char c = value[i];

				if (c >= 'A' && c <= 'Z')
					c = (char) (c + 0x20);

				if (c != lowercase[i])
					return false;
			}

			return true;
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
			// Note: In the frameset insertion modes, every start tag other than <html>, <frameset>, <frame> and <noframes> is
			// ignored, so <noframes> is the only start tag that switches the tokenizer state.
			if (inFrameset)
				return tag.Id == HtmlTagId.NoFrames;

			if (mode != InsertionMode.AfterHead && !ProcessHeadStartTag (tag))
				return true;

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

		// Updates the insertion mode for a start tag that is processed before the body. Returns false if the start tag
		// should be ignored.
		bool ProcessHeadStartTag (HtmlTagToken tag)
		{
			// Note: In the "in template" insertion mode, any start tag that does not belong in the head is processed as
			// if in the body without closing the head (and a <noscript> start tag does not belong in the head).
			if (IsTemplateOpen)
				return true;

			switch (mode) {
			case InsertionMode.BeforeHead:
				// Note: Any start tag other than <html> implies a <head> start tag (if it isn't one) and gets reprocessed
				// using the "in head" insertion mode.
				if (tag.Id == HtmlTagId.Html)
					return true;

				mode = InsertionMode.InHead;
				break;
			case InsertionMode.InHeadNoScript:
				switch (tag.Id) {
				case HtmlTagId.Html:
				case HtmlTagId.BaseFont:
				case HtmlTagId.BGSound:
				case HtmlTagId.Link:
				case HtmlTagId.Meta:
				case HtmlTagId.NoFrames:
				case HtmlTagId.Style:
					return true;
				case HtmlTagId.Head:
				case HtmlTagId.NoScript:
					return false;
				}

				// Any other start tag closes the <noscript> element and gets reprocessed using the "in head" insertion mode.
				PopThrough (IndexOf (HtmlTagId.NoScript));
				mode = InsertionMode.InHead;
				break;
			}

			switch (tag.Id) {
			case HtmlTagId.Html:
			case HtmlTagId.Head:
			case HtmlTagId.Base:
			case HtmlTagId.BaseFont:
			case HtmlTagId.BGSound:
			case HtmlTagId.Link:
			case HtmlTagId.Meta:
			case HtmlTagId.Title:
			case HtmlTagId.NoFrames:
			case HtmlTagId.Style:
			case HtmlTagId.Script:
				return true;
			case HtmlTagId.NoScript:
				// Note: When scripting is enabled, the content of the <noscript> element is raw text.
				if (!ScriptingEnabled)
					mode = InsertionMode.InHeadNoScript;
				return true;
			case HtmlTagId.Unknown:
				if (tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase))
					return true;
				break;
			}

			// Any other start tag closes the <head> element.
			mode = InsertionMode.AfterHead;

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

			if (framesetOk && ClearsFramesetOk (tag))
				framesetOk = false;

			switch (id) {
			case HtmlTagId.Html:
			case HtmlTagId.Head:
				// These elements are never pushed more than once, so their start tags have no effect on the stack.
				return;
			case HtmlTagId.Body:
				// Note: The <body> element is never pushed more than once either, but a <body> start tag sets the frameset-ok
				// flag to "not ok" (see ClearsFramesetOk).
				return;
			case HtmlTagId.FrameSet:
				// Note: Unless the frameset-ok flag is "not ok", the <frameset> element replaces the <body> (and everything in it).
				// The <frameset> start tag is also ignored if the second element on the stack of open elements is not the <body>,
				// but that can only happen in the fragment case or if there is a <template> element on the stack, and a <template>
				// start tag sets the frameset-ok flag to "not ok".
				if (framesetOk) {
					PopAbove (-1);
					inFrameset = true;
				}
				return;
			case HtmlTagId.Address: case HtmlTagId.Article: case HtmlTagId.Aside: case HtmlTagId.BlockQuote:
			case HtmlTagId.Center: case HtmlTagId.Details: case HtmlTagId.Dialog: case HtmlTagId.Dir:
			case HtmlTagId.Div: case HtmlTagId.DL: case HtmlTagId.FieldSet: case HtmlTagId.FigCaption:
			case HtmlTagId.Figure: case HtmlTagId.Footer: case HtmlTagId.Header: case HtmlTagId.Main:
			case HtmlTagId.Menu: case HtmlTagId.Nav: case HtmlTagId.OL: case HtmlTagId.P:
			case HtmlTagId.Section: case HtmlTagId.Summary: case HtmlTagId.UL: case HtmlTagId.Pre:
			case HtmlTagId.Listing: case HtmlTagId.PlainText: case HtmlTagId.Xmp:
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.Form:
				ProcessFormStartTag (tag);
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

		void ProcessFormStartTag (HtmlTagToken tag)
		{
			bool templateOpen = IsTemplateOpen;

			// Note: Unless there is a <template> element on the stack, a <form> start tag is ignored if the form element
			// pointer is set (even if the <form> element that it points to has already been closed).
			if (formPointerSet && !templateOpen)
				return;

			// Note: In the "in table", "in table body" and "in row" insertion modes, the <form> element is popped as soon as
			// it gets inserted (or the start tag is ignored if there is a <template> element on the stack).
			if (count > 0 && EntryAt (count - 1).Namespace == HtmlNamespace.Html) {
				switch (EntryAt (count - 1).Id) {
				case HtmlTagId.Table: case HtmlTagId.TBody: case HtmlTagId.Tfoot: case HtmlTagId.THead: case HtmlTagId.TR:
					if (!templateOpen)
						formPointerSet = true;
					return;
				}
			}

			ClosePInButtonScope ();
			PushHtml (tag);

			if (!templateOpen) {
				formPointerSet = true;
				formPointerIndex = IsTopHtml (HtmlTagId.Form) ? count - 1 : -1;
			}
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
			poppedThrough = false;

			// Note: In the frameset insertion modes, end tags never affect anything that matters to the tokenizer.
			if (inFrameset) {
				EndTagIgnored = false;
				return;
			}

			if (mode != InsertionMode.AfterHead)
				ProcessHeadEndTag (tag);

			// Note: In the body, </br> is treated as a <br> start tag.
			if (tag.Id == HtmlTagId.Br)
				framesetOk = false;

			ProcessEndTagInternal (tag);

			// Note: </html>, </head>, </body> and </br> never close an element, so they are not considered to have been ignored.
			EndTagIgnored = !poppedThrough && tag.Id != HtmlTagId.Html && tag.Id != HtmlTagId.Head && tag.Id != HtmlTagId.Body && tag.Id != HtmlTagId.Br &&
				(IndexOf (HtmlNamespace.Html, tag.Id, tag.Name) >= 0 || IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, tag.Name) >= 0);
		}

		// Updates the insertion mode for an end tag that is processed before the body.
		//
		// Note: The end tags that get ignored before the body do not need to be special-cased since the only element that
		// can be open at this point (outside of a <template>) is the <noscript> element.
		void ProcessHeadEndTag (HtmlTagToken tag)
		{
			if (IsTemplateOpen)
				return;

			switch (tag.Id) {
			case HtmlTagId.NoScript:
				// Note: The </noscript> end tag itself pops the <noscript> element.
				if (mode == InsertionMode.InHeadNoScript)
					mode = InsertionMode.InHead;
				break;
			case HtmlTagId.Br:
				if (mode == InsertionMode.InHeadNoScript)
					PopThrough (IndexOf (HtmlTagId.NoScript));

				mode = InsertionMode.AfterHead;
				break;
			case HtmlTagId.Head:
			case HtmlTagId.Body:
			case HtmlTagId.Html:
				// Note: These end tags are ignored in the "in head noscript" insertion mode.
				if (mode != InsertionMode.InHeadNoScript)
					mode = InsertionMode.AfterHead;
				break;
			}
		}

		void ProcessEndTagInternal (HtmlTagToken tag)
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

		void ProcessFormEndTag ()
		{
			int index;

			if (IsTemplateOpen) {
				index = IndexOf (HtmlTagId.Form);

				if (InScope (index, TopScope))
					PopThrough (index);
				return;
			}

			// Note: Unless there is a <template> element on the stack, the </form> end tag unsets the form element pointer and
			// removes the <form> element that it pointed to (if it is in scope) from the stack *without* popping the elements
			// above it. E.g. in "<form><svg><style></form>", the SVG elements remain open.
			index = formPointerSet ? formPointerIndex : -1;
			formPointerSet = false;
			formPointerIndex = -1;

			if (!InScope (index, TopScope))
				return;

			GenerateImpliedEndTags ();

			if (index == count - 1)
				PopThrough (index);
			else
				RemoveEntry (index);
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
			case HtmlTagId.UL: case HtmlTagId.Applet: case HtmlTagId.Marquee:
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
			case HtmlTagId.Form:
				ProcessFormEndTag ();
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
