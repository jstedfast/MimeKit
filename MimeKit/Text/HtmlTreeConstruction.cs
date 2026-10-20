//
// HtmlTreeConstruction.cs
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

using ElementFlags = MimeKit.Text.HtmlOpenElementStack.ElementFlags;

namespace MimeKit.Text {
	/// <summary>
	/// The tree-construction state used to drive HTML tokenization.
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
	/// implied table structure, the stack updates of the adoption agency algorithm, and the rules for breaking
	/// out of foreign content) but does not model the list of active formatting elements, its reconstruction,
	/// foster parenting, or the select or template insertion modes. Of the document's head/body structure,
	/// it only models whether the tree builder is still before the body and whether it is in the "in head noscript"
	/// insertion mode (since, when scripting is disabled, a <c>&lt;noscript&gt;</c> element within the
	/// <c>&lt;head&gt;</c> gets closed by any tag or character data that is not allowed there, unlike a
	/// <c>&lt;noscript&gt;</c> within the body), as well as the frameset-ok flag and the frameset insertion modes
	/// (since a <c>&lt;frameset&gt;</c> makes browsers ignore start tags such as <c>&lt;style&gt;</c> or
	/// <c>&lt;svg&gt;</c> that would otherwise switch the tokenizer state). It also models the form element
	/// pointer, since (outside of a <c>&lt;template&gt;</c>) a <c>&lt;/form&gt;</c> end tag removes the
	/// <c>&lt;form&gt;</c> element from the stack without closing the elements above it (such as an open
	/// <c>&lt;svg&gt;</c> element).</para>
	/// <para>Element storage and scope queries are provided by <see cref="HtmlOpenElementStack"/>.
	/// The controller owns the parsing rules, and reports lexical transitions and tag dispositions to the
	/// tokenizer. Converters use its structural results separately from their callback contexts.</para>
	/// </remarks>
	sealed class HtmlTreeConstruction
	{
		// Normal means that a tag was neither ignored nor merged, not necessarily that an element was inserted.
		public enum TagDisposition : byte
		{
			Normal,
			Ignored,
			AttributesMerged
		}

		/// <summary>
		/// The lexical transition and structural disposition of a single tag.
		/// </summary>
		public readonly struct TagResult
		{
			public readonly HtmlTokenizerState TokenizerState;
			public readonly TagDisposition Disposition;
			public readonly bool BrokeOutOfForeignContent;

			public TagResult (HtmlTokenizerState tokenizerState, TagDisposition disposition, bool brokeOutOfForeignContent)
			{
				TokenizerState = tokenizerState;
				Disposition = disposition;
				BrokeOutOfForeignContent = brokeOutOfForeignContent;
			}
		}

		// The tree builder's insertion modes, as far as they matter for the document's head. Once the head has been
		// closed, AfterHead covers both the "after head" and the "in body" insertion modes (see bodyCreated).
		enum InsertionMode : byte
		{
			BeforeHead,
			InHead,
			InHeadNoScript,
			AfterHead
		}

		readonly HtmlOpenElementStack stack;
		bool poppedThrough;
		InsertionMode mode;

		// The frameset-ok flag and whether a <frameset> element has replaced the <body>.
		bool framesetOk = true;
		bool inFrameset;

		// The number of open <frameset> elements. Once the outermost one has been closed, the tree builder is in the "after
		// frameset" insertion mode, which ignores <frameset> and <frame> start tags as well.
		int framesetDepth;

		// Whether the <html> element has been inserted (it is never pushed onto the stack).
		bool htmlCreated;

		// Whether the <body> element has been inserted. The "after head" insertion mode (InsertionMode.AfterHead before the body
		// has been inserted) inserts a <frameset> element no matter what the frameset-ok flag is.
		bool bodyCreated;

		// The form element pointer: whether it is set, and the index of the <form> element that it points to (or -1 if that
		// element is no longer on the stack, e.g. because it was popped by a </div> or because it was inserted in a table).
		bool formPointerSet;
		int formPointerIndex = -1;

		public HtmlTreeConstruction (int maxDepth)
		{
			stack = new HtmlOpenElementStack (maxDepth);
		}

		public int MaxDepth {
			get { return stack.MaxDepth; }
			set { stack.MaxDepth = value; }
		}

		public bool DepthExceeded {
			get { return stack.DepthExceeded; }
		}

		public bool TrackClosedElements {
			get { return stack.TrackClosedElements; }
			set { stack.TrackClosedElements = value; }
		}

		public IReadOnlyList<HtmlOpenElementStack.ClosedElement> ClosedElements {
			get { return stack.ClosedElements; }
		}

		public TagResult LastTag {
			get; private set;
		}

		// The namespace declared on <html>, retained for HtmlTokenizer.HtmlNamespace. This is not the
		// adjusted current node's namespace, which is used separately to recognize foreign content.
		public HtmlNamespace HtmlNamespace {
			get; private set;
		}

		/// <summary>
		/// Process a tag and report its disposition and the tokenizer state for the following content.
		/// </summary>
		/// <remarks>
		/// <see cref="ClosedElements"/> describes the implicit closures caused by this tag. The list is
		/// reused and is cleared when the next tag is processed.
		/// </remarks>
		public TagResult ProcessTag (HtmlTagToken tag)
		{
			ClearClosedElements ();

			HtmlTokenizerState state;
			TagDisposition disposition;

			if (tag.IsEndTag) {
				ProcessEndTag (tag);
				disposition = EndTagIgnored ? TagDisposition.Ignored : TagDisposition.Normal;
				state = HtmlTokenizerState.Data;
			} else {
				state = ProcessStartTag (tag);
				disposition = StartTagIgnored ? TagDisposition.Ignored :
					StartTagAttributesMerged ? TagDisposition.AttributesMerged : TagDisposition.Normal;
			}

			// Once structural tracking has failed, the remaining input must be emitted as literal text.
			if (DepthExceeded)
				state = HtmlTokenizerState.PlainText;

			LastTag = new TagResult (state, disposition, BrokeOutOfForeignContent);

			return LastTag;
		}

		HtmlTokenizerState GetTokenizerState (HtmlTagToken tag)
		{
			// The self-closing flag does not affect the content state of non-void HTML elements.
			switch (tag.Id) {
			case HtmlTagId.Style: case HtmlTagId.Xmp: case HtmlTagId.IFrame: case HtmlTagId.NoEmbed: case HtmlTagId.NoFrames:
				return HtmlTokenizerState.RawText;
			case HtmlTagId.Title: case HtmlTagId.TextArea:
				return HtmlTokenizerState.RcData;
			case HtmlTagId.PlainText:
				return HtmlTokenizerState.PlainText;
			case HtmlTagId.Script:
				return HtmlTokenizerState.ScriptData;
			case HtmlTagId.NoScript:
				return ScriptingEnabled ? HtmlTokenizerState.RawText : HtmlTokenizerState.Data;
			case HtmlTagId.Html:
				if (!tag.IsEmptyElement) {
					for (int i = tag.Attributes.Count; i > 0; i--) {
						var attr = tag.Attributes[i - 1];

						if (attr.Id == HtmlAttributeId.XmlNS && attr.Value != null) {
							HtmlNamespace = attr.Value.ToHtmlNamespace ();
							break;
						}
					}
				}
				break;
			}

			return HtmlTokenizerState.Data;
		}

		void PopAbove (int index)
		{
			stack.PopAbove (index);

			if (formPointerIndex >= stack.Count)
				formPointerIndex = -1;
		}

		void PopThrough (int index)
		{
			poppedThrough = true;
			stack.PopThrough (index);

			if (formPointerIndex >= stack.Count)
				formPointerIndex = -1;
		}

		void RemoveEntry (int index)
		{
			poppedThrough = true;
			stack.RemoveEntry (index);
		}

		/// <summary>
		/// Get whether the most recently processed end tag was ignored.
		/// </summary>
		/// <remarks>
		/// An end tag is ignored if it did not close an element even though an element with the same name is open
		/// (e.g. <c>&lt;/mi&gt;</c> when an HTML element is the current node and a special element such as the
		/// MathML <c>&lt;mi&gt;</c> is in the way).
		/// </remarks>
		bool EndTagIgnored {
			get; set;
		}

		/// <summary>
		/// Get whether the most recently processed start tag was ignored.
		/// </summary>
		/// <remarks>
		/// A start tag is ignored if browsers do not insert an element for it, e.g. a <c>&lt;head&gt;</c> start tag after the
		/// <c>&lt;head&gt;</c>, a <c>&lt;frameset&gt;</c> start tag after the frameset-ok flag has been set to "not ok", any
		/// start tag other than <c>&lt;html&gt;</c>, <c>&lt;frameset&gt;</c>, <c>&lt;frame&gt;</c> or <c>&lt;noframes&gt;</c>
		/// in a frameset document, a nested <c>&lt;form&gt;</c>, or a table structure start tag (such as <c>&lt;tr&gt;</c>)
		/// outside of a table.
		/// </remarks>
		bool StartTagIgnored {
			get; set;
		}

		/// <summary>
		/// Get whether the attributes of the most recently processed start tag were added to an existing element.
		/// </summary>
		/// <remarks>
		/// An <c>&lt;html&gt;</c> or <c>&lt;body&gt;</c> start tag that comes after the element has been inserted (explicitly
		/// or implicitly) does not insert another element. Instead, browsers add any of its attributes that the existing element
		/// does not already have to that element.
		/// </remarks>
		bool StartTagAttributesMerged {
			get; set;
		}

		/// <summary>
		/// Get whether the most recently processed tag closed SVG or MathML elements by breaking out of foreign content.
		/// </summary>
		/// <remarks>
		/// Within SVG or MathML content (other than at an integration point), start tags such as <c>&lt;p&gt;</c>,
		/// <c>&lt;b&gt;</c>, <c>&lt;img&gt;</c> or <c>&lt;font color&gt;</c>, as well as the <c>&lt;/p&gt;</c> and
		/// <c>&lt;/br&gt;</c> end tags, close the open SVG and MathML elements and get processed as HTML.
		/// </remarks>
		bool BrokeOutOfForeignContent {
			get; set;
		}

		/// <summary>
		/// Clear the list of implicitly closed elements.
		/// </summary>
		public void ClearClosedElements ()
		{
			stack.ClearClosedElements ();
		}

		/// <summary>
		/// Get whether the adjusted current node is an element in the SVG or MathML namespace.
		/// </summary>
		/// <remarks>
		/// This is the condition under which a <c>&lt;![CDATA[</c> markup declaration starts a CDATA section.
		/// </remarks>
		public bool IsInForeignContent {
			get { return stack.Count > 0 && stack[stack.Count - 1].Namespace != HtmlNamespace.Html; }
		}

		/// <summary>
		/// Get whether an HTML <c>&lt;noscript&gt;</c> element is open.
		/// </summary>
		/// <remarks>
		/// Gets whether an HTML <c>&lt;noscript&gt;</c> element is open.
		/// </remarks>
		public bool IsNoScriptOpen {
			get { return stack.IndexOf (HtmlTagId.NoScript) >= 0; }
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
		/// has inserted the document's <c>&lt;body&gt;</c> and the frameset-ok flag has been set to "not ok".
		/// </remarks>
		public bool TracksCharacterData {
			get { return !inFrameset && (mode != InsertionMode.AfterHead || framesetOk || !bodyCreated); }
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
			htmlCreated = true;

			if (inFrameset)
				return;

			// Note: In the "in template" insertion mode, character data is processed as if in the body without closing the head.
			if (!IsTemplateOpen) {
				if (mode == InsertionMode.InHeadNoScript)
					PopThrough (stack.IndexOf (HtmlTagId.NoScript));

				mode = InsertionMode.AfterHead;
				InsertImpliedBody ();
			}

			if (c != '\0')
				framesetOk = false;
		}

		// Inserts the <body> element for a token that is processed using the "anything else" rules of the "after head" insertion
		// mode, which also set the frameset-ok flag back to "ok" (e.g. after a <template> in the <head> set it to "not ok").
		void InsertImpliedBody ()
		{
			if (!bodyCreated) {
				bodyCreated = true;
				framesetOk = true;
			}
		}

		bool IsTemplateOpen {
			get { return stack.Count > 0 && stack.IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, "template") >= 0; }
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

		// Pop the elements that have implied end tags (dd, dt, li, optgroup, option, p, rb, rp, rt and rtc).
		void GenerateImpliedEndTags ()
		{
			int index = stack.Count - 1;

			while (index >= 0 && stack[index].Namespace == HtmlNamespace.Html && HasImpliedEndTag (stack[index]))
				index--;

			PopAbove (index);
		}

		static bool HasImpliedEndTag (in HtmlOpenElementStack.Entry entry)
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

		// Start tags that, when processed using the rules for the "after head" insertion mode, insert the <body> element.
		static bool InsertsBody (HtmlTagToken tag)
		{
			switch (tag.Id) {
			case HtmlTagId.Html: case HtmlTagId.Head: case HtmlTagId.FrameSet: case HtmlTagId.Base: case HtmlTagId.BaseFont:
			case HtmlTagId.BGSound: case HtmlTagId.Link: case HtmlTagId.Meta: case HtmlTagId.NoFrames: case HtmlTagId.Script:
			case HtmlTagId.Style: case HtmlTagId.Title:
				return false;
			case HtmlTagId.Unknown:
				return !tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase);
			default:
				return true;
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
		/// <returns>The tokenizer state for the content following this tag.</returns>
		/// <param name="tag">The start tag.</param>
		HtmlTokenizerState ProcessStartTag (HtmlTagToken tag)
		{
			bool htmlExisted = htmlCreated;
			var entryMode = mode;

			StartTagAttributesMerged = false;
			BrokeOutOfForeignContent = false;
			StartTagIgnored = false;
			htmlCreated = true;

			// Note: In the frameset insertion modes, every start tag other than <html>, <frameset>, <frame> and <noframes> is
			// ignored, so <noframes> is the only start tag that switches the tokenizer state.
			if (inFrameset) {
				switch (tag.Id) {
				case HtmlTagId.Html:
					StartTagAttributesMerged = tag.Attributes.Count > 0;
					break;
				case HtmlTagId.FrameSet:
					// Note: The self-closing flag is ignored for the <frameset> element.
					if (framesetDepth > 0)
						framesetDepth++;
					else
						StartTagIgnored = true;
					break;
				case HtmlTagId.Frame:
					StartTagIgnored = framesetDepth == 0;
					break;
				case HtmlTagId.NoFrames:
					break;
				default:
					StartTagIgnored = true;
					break;
				}

				return tag.Id == HtmlTagId.NoFrames ? HtmlTokenizerState.RawText : HtmlTokenizerState.Data;
			}

			if (mode != InsertionMode.AfterHead && !ProcessHeadStartTag (tag)) {
				StartTagIgnored = true;
				return GetTokenizerState (tag);
			}

			if (stack.Count > 0) {
				ref readonly var top = ref stack[stack.Count - 1];

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
							PopAbove (stack.TopHtmlOrIntegrationPoint);
							BrokeOutOfForeignContent = true;
						} else {
							// Insert a foreign element for the token in the same namespace as the adjusted current node.
							var ns = top.Namespace;

							if (!tag.IsEmptyElement)
								stack.Push (tag.Name, HtmlTagId.Unknown, ns, GetForeignFlags (ns, tag));

							return HtmlTokenizerState.Data;
						}
					}
				}
			}

			ProcessHtmlStartTag (tag, entryMode, htmlExisted);

			return GetTokenizerState (tag);
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
				PopThrough (stack.IndexOf (HtmlTagId.NoScript));
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
			int p = stack.IndexOf (HtmlTagId.P);

			if (InScope (p, stack.TopButtonScope))
				PopThrough (p);
		}

		void CloseListItem (int item)
		{
			// Walk the stack from the current node: if an item is found before a special element other than
			// address, div or p, close it.
			if (item >= 0 && item >= stack.TopListItemStop)
				PopThrough (item);
		}

		// Returns the index of the current table if the topmost table-scope boundary is a table element; otherwise, -1.
		int CurrentTable ()
		{
			int table = stack.IndexOf (HtmlTagId.Table);

			return table >= 0 && table == stack.TopTableScope ? table : -1;
		}

		void PushHtml (HtmlTagToken tag)
		{
			stack.Push (tag.Name, tag.Id, HtmlNamespace.Html, GetHtmlFlags (tag.Id, tag.Name));
		}

		void PushImplied (HtmlTagId id)
		{
			stack.Push (id.ToHtmlTagName (), id, HtmlNamespace.Html, GetHtmlFlags (id, string.Empty));
		}

		void ProcessHtmlStartTag (HtmlTagToken tag, InsertionMode entryMode, bool htmlExisted)
		{
			bool hadBody = bodyCreated;
			var id = tag.Id;
			int index, table;

			// Note: In the "after head" insertion mode, any start tag that does not belong in the head (other than <body> and
			// <frameset>) inserts the <body> element.
			if (!bodyCreated && mode == InsertionMode.AfterHead && InsertsBody (tag) && !IsTemplateOpen) {
				if (id == HtmlTagId.Body)
					bodyCreated = true;
				else
					InsertImpliedBody ();
			}

			if (framesetOk && ClearsFramesetOk (tag))
				framesetOk = false;

			switch (id) {
			case HtmlTagId.Html:
				// Note: The <html> element is never pushed more than once, so its start tag has no effect on the stack. A
				// subsequent <html> start tag is ignored if there is a <template> element on the stack; otherwise, its
				// attributes get added to the existing <html> element.
				if (IsTemplateOpen)
					StartTagIgnored = true;
				else
					StartTagAttributesMerged = htmlExisted && tag.Attributes.Count > 0;
				return;
			case HtmlTagId.Head:
				// Note: The <head> element is never pushed more than once either, so any <head> start tag that appears after
				// the "before head" insertion mode gets ignored.
				StartTagIgnored = entryMode != InsertionMode.BeforeHead;
				return;
			case HtmlTagId.Body:
				// Note: The <body> element is never pushed more than once either, but a <body> start tag sets the frameset-ok
				// flag to "not ok" (see ClearsFramesetOk). A subsequent <body> start tag is ignored if there is a <template>
				// element on the stack; otherwise, its attributes get added to the existing <body> element.
				if (IsTemplateOpen)
					StartTagIgnored = true;
				else
					StartTagAttributesMerged = hadBody && tag.Attributes.Count > 0;
				return;
			case HtmlTagId.Frame:
				// Note: Outside of a <frameset>, the <frame> start tag is ignored.
				StartTagIgnored = true;
				return;
			case HtmlTagId.FrameSet:
				// Note: Before the <body> has been inserted, the <frameset> element gets inserted no matter what the frameset-ok
				// flag is (e.g. after a <template> in the <head>, which sets it to "not ok"). Otherwise, unless the frameset-ok flag
				// is "not ok", the <frameset> element replaces the <body> (and everything in it). The <frameset> start tag is also
				// ignored if there is a <template> element on the stack.
				if ((framesetOk || !bodyCreated) && !IsTemplateOpen) {
					PopAbove (-1);
					inFrameset = true;
					framesetDepth = 1;
				} else {
					StartTagIgnored = true;
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

				if (stack.Count > 0 && stack[stack.Count - 1].Namespace == HtmlNamespace.Html && IsHeading (stack[stack.Count - 1].Id))
					PopThrough (stack.Top);

				PushHtml (tag);
				return;
			case HtmlTagId.HR:
				ClosePInButtonScope ();
				return;
			case HtmlTagId.LI:
				CloseListItem (stack.IndexOf (HtmlTagId.LI));
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.DD:
			case HtmlTagId.DT:
				CloseListItem (Max (stack.IndexOf (HtmlTagId.DD), stack.IndexOf (HtmlTagId.DT)));
				ClosePInButtonScope ();
				PushHtml (tag);
				return;
			case HtmlTagId.Button:
				index = stack.IndexOf (HtmlTagId.Button);

				if (InScope (index, stack.TopScope))
					PopThrough (index);

				PushHtml (tag);
				return;
			case HtmlTagId.A:
				// Approximates the adoption agency algorithm being run for a nested <a>: when there is no
				// special element above the open <a>, the open <a> gets popped.
				index = stack.IndexOf (HtmlTagId.A);

				if (index >= 0 && stack.TopSpecial < index)
					PopThrough (index);

				PushHtml (tag);
				return;
			case HtmlTagId.Option:
			case HtmlTagId.OptGroup:
				if (stack.IsTopHtml (HtmlTagId.Option))
					PopThrough (stack.Top);

				PushHtml (tag);
				return;
			case HtmlTagId.Table:
				table = CurrentTable ();

				if (table >= 0 && Max (stack.IndexOf (HtmlTagId.TD), stack.IndexOf (HtmlTagId.TH), stack.IndexOf (HtmlTagId.Caption)) < table) {
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
						stack.Push (tag.Name, HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);
					return;
				}

				if (tag.Name.Equals ("math", StringComparison.OrdinalIgnoreCase)) {
					if (!tag.IsEmptyElement)
						stack.Push (tag.Name, HtmlTagId.Unknown, HtmlNamespace.MathML, ElementFlags.None);
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
			if (formPointerSet && !templateOpen) {
				StartTagIgnored = true;
				return;
			}

			// Note: In the "in table", "in table body" and "in row" insertion modes, the <form> element is popped as soon as
			// it gets inserted (or the start tag is ignored if there is a <template> element on the stack).
			if (stack.Count > 0 && stack[stack.Count - 1].Namespace == HtmlNamespace.Html) {
				switch (stack[stack.Count - 1].Id) {
				case HtmlTagId.Table: case HtmlTagId.TBody: case HtmlTagId.Tfoot: case HtmlTagId.THead: case HtmlTagId.TR:
					if (!templateOpen)
						formPointerSet = true;
					else
						StartTagIgnored = true;
					return;
				}
			}

			ClosePInButtonScope ();
			PushHtml (tag);

			if (!templateOpen) {
				formPointerSet = true;
				formPointerIndex = stack.IsTopHtml (HtmlTagId.Form) ? stack.Count - 1 : -1;
			}
		}

		void ProcessTableStructureStartTag (HtmlTagToken tag)
		{
			int table = CurrentTable ();
			int index;

			// Table structure start tags outside of a table are ignored.
			if (table < 0) {
				StartTagIgnored = stack.TopTableScope < 0;
				return;
			}

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
				index = Max (stack.IndexOf (HtmlTagId.TBody), stack.IndexOf (HtmlTagId.THead), stack.IndexOf (HtmlTagId.Tfoot));

				if (index > table) {
					PopAbove (index);
				} else {
					PopAbove (table);
					PushImplied (HtmlTagId.TBody);
				}

				PushHtml (tag);
				break;
			default: // td, th
				index = stack.IndexOf (HtmlTagId.TR);

				if (index > table) {
					PopAbove (index);
				} else {
					index = Max (stack.IndexOf (HtmlTagId.TBody), stack.IndexOf (HtmlTagId.THead), stack.IndexOf (HtmlTagId.Tfoot));

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
		void ProcessEndTag (HtmlTagToken tag)
		{
			BrokeOutOfForeignContent = false;
			poppedThrough = false;

			// Note: In the "before html" insertion mode, only </head>, </body>, </html> and </br> create the <html> element.
			if (tag.Id == HtmlTagId.Head || tag.Id == HtmlTagId.Body || tag.Id == HtmlTagId.Html || tag.Id == HtmlTagId.Br)
				htmlCreated = true;

			// Note: In the frameset insertion modes, end tags never affect anything that matters to the tokenizer.
			if (inFrameset) {
				if (tag.Id == HtmlTagId.FrameSet && framesetDepth > 0)
					framesetDepth--;

				EndTagIgnored = false;
				return;
			}

			if (mode != InsertionMode.AfterHead)
				ProcessHeadEndTag (tag);

			// Note: In the "after head" insertion mode, </body>, </html> and </br> insert the <body> element.
			if (!bodyCreated && mode == InsertionMode.AfterHead && (tag.Id == HtmlTagId.Body || tag.Id == HtmlTagId.Html || tag.Id == HtmlTagId.Br) && !IsTemplateOpen)
				InsertImpliedBody ();

			// Note: In the body, </br> is treated as a <br> start tag.
			if (tag.Id == HtmlTagId.Br)
				framesetOk = false;

			ProcessEndTagInternal (tag);

			// Note: </html>, </head>, </body> and </br> never close an element, so they are not considered to have been ignored.
			EndTagIgnored = !poppedThrough && tag.Id != HtmlTagId.Html && tag.Id != HtmlTagId.Head && tag.Id != HtmlTagId.Body && tag.Id != HtmlTagId.Br &&
				(stack.IndexOf (HtmlNamespace.Html, tag.Id, tag.Name) >= 0 || stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, tag.Name) >= 0);
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
					PopThrough (stack.IndexOf (HtmlTagId.NoScript));

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
			if (stack.Count > 0 && stack[stack.Count - 1].Namespace != HtmlNamespace.Html) {
				if (tag.Id == HtmlTagId.Br || tag.Id == HtmlTagId.P) {
					// </br> and </p> break out of foreign content.
					int n = stack.Count;

					PopAbove (stack.TopHtmlOrIntegrationPoint);
					BrokeOutOfForeignContent = stack.Count < n;
				} else {
					// Walk down the stack looking for a foreign element with a matching name, stopping at the first
					// element in the HTML namespace. If a match is found, pop through it; otherwise, process the
					// token using the rules for HTML content.
					int index = stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, tag.Name);

					if (index > stack.TopHtml) {
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
				index = stack.IndexOf (HtmlTagId.Form);

				if (InScope (index, stack.TopScope))
					PopThrough (index);
				return;
			}

			// Note: Unless there is a <template> element on the stack, the </form> end tag unsets the form element pointer and
			// removes the <form> element that it pointed to (if it is in scope) from the stack *without* popping the elements
			// above it. E.g. in "<form><svg><style></form>", the SVG elements remain open.
			index = formPointerSet ? formPointerIndex : -1;
			formPointerSet = false;
			formPointerIndex = -1;

			if (!InScope (index, stack.TopScope))
				return;

			GenerateImpliedEndTags ();

			if (index == stack.Count - 1)
				PopThrough (index);
			else
				RemoveEntry (index);
		}

		void ProcessFormattingEndTag (HtmlTagId id)
		{
			int index = stack.IndexOf (id);

			if (!InScope (index, stack.TopScope))
				return;

			HtmlOpenElementStack.Entry[]? replacement = null;

			// Model the stack effects, not the DOM reparenting. As elsewhere in this controller, formatting
			// elements are looked up on the stack rather than reconstructed from an active formatting list.
			for (int outer = 0; outer < 8; outer++) {
				if (stack.TopSpecial < index) {
					PopThrough (index);
					return;
				}

				int block = index + 1;

				while ((stack[block].Flags & ElementFlags.Special) == 0)
					block++;

				replacement ??= new HtmlOpenElementStack.Entry[7];
				var formatting = stack[index];
				var furthestBlock = stack[block];
				int count = 0;

				// A folded entry can contain more than one physical node. Only the last formatting node
				// moves, and only the first special node is the furthest block.
				if (formatting.Count > 1) {
					replacement[count] = formatting;
					replacement[count++].Count--;
				}

				int first = count;
				int remaining = 3;

				for (int i = block - 1; i > index && remaining > 0; i--) {
					var node = stack[i];
					int retained = Math.Min (remaining, node.Count);

					remaining -= retained;

					if (node.Namespace == HtmlNamespace.Html && node.Id.IsFormattingElement ()) {
						node.Count = retained;
						replacement[count++] = node;
					}
				}

				// The inner loop visits nodes backwards, but their stack order does not change.
				Array.Reverse (replacement, first, count - first);
				replacement[count] = furthestBlock;
				replacement[count++].Count = 1;
				replacement[count] = formatting;
				replacement[count++].Count = 1;

				if (furthestBlock.Count > 1) {
					replacement[count] = furthestBlock;
					replacement[count++].Count--;
				}

				// Non-formatting inner nodes and formatting nodes beyond the first three are removed
				// together, so a long inner loop does not repeatedly rebuild the same suffix.
				stack.ReplaceRange (index, block - index + 1, replacement, count);
				poppedThrough = true;

				if (formPointerIndex >= index)
					formPointerIndex = stack.IndexOf (HtmlTagId.Form);

				if (DepthExceeded)
					return;

				index = stack.IndexOf (id);
			}
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
				index = stack.IndexOf (HtmlTagId.P);

				if (InScope (index, stack.TopButtonScope))
					PopThrough (index);
				return;
			case HtmlTagId.LI:
				index = stack.IndexOf (HtmlTagId.LI);

				if (InScope (index, stack.TopListItemScope))
					PopThrough (index);
				return;
			case HtmlTagId.H1: case HtmlTagId.H2: case HtmlTagId.H3:
			case HtmlTagId.H4: case HtmlTagId.H5: case HtmlTagId.H6:
				// Any heading end tag closes the topmost heading element.
				index = stack.TopHeading;

				if (InScope (index, stack.TopScope))
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
				index = stack.IndexOf (tag.Id);

				if (InScope (index, stack.TopScope))
					PopThrough (index);
				return;
			case HtmlTagId.Table: case HtmlTagId.TBody: case HtmlTagId.Tfoot: case HtmlTagId.THead:
			case HtmlTagId.TR: case HtmlTagId.TD: case HtmlTagId.TH: case HtmlTagId.Caption:
			case HtmlTagId.ColGroup:
				index = stack.IndexOf (tag.Id);

				if (InScope (index, stack.TopTableScope))
					PopThrough (index);
				return;
			case HtmlTagId.Form:
				ProcessFormEndTag ();
				return;
			case HtmlTagId.A: case HtmlTagId.B: case HtmlTagId.Big: case HtmlTagId.Code:
			case HtmlTagId.EM: case HtmlTagId.Font: case HtmlTagId.I: case HtmlTagId.NoBR:
			case HtmlTagId.S: case HtmlTagId.Small: case HtmlTagId.Strike: case HtmlTagId.Strong:
			case HtmlTagId.TT: case HtmlTagId.U:
				ProcessFormattingEndTag (tag.Id);
				return;
			case HtmlTagId.Unknown:
				if (tag.Name.Equals ("hgroup", StringComparison.OrdinalIgnoreCase) || tag.Name.Equals ("search", StringComparison.OrdinalIgnoreCase)) {
					index = stack.IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, tag.Name);

					if (InScope (index, stack.TopScope))
						PopThrough (index);
					return;
				}

				if (tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase)) {
					index = stack.IndexOf (HtmlNamespace.Html, HtmlTagId.Unknown, tag.Name);

					if (index >= 0)
						PopThrough (index);
					return;
				}
				break;
			}

			// Any other end tag: walk down the stack looking for an HTML element with a matching name,
			// giving up if a special element is found first.
			index = stack.IndexOf (HtmlNamespace.Html, tag.Id, tag.Name);

			if (index >= 0 && index >= stack.TopSpecial)
				PopThrough (index);
		}

		#endregion
	}
}
