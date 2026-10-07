//
// HtmlTextPreviewer.cs
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
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// A text previewer for HTML content.
	/// </summary>
	/// <remarks>
	/// A text previewer for HTML content.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TextConverterExamples.cs" region="GetPreviewText"/>
	/// </example>
	public class HtmlTextPreviewer : TextPreviewer
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTextPreviewer"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new previewer for HTML.
		/// </remarks>
		public HtmlTextPreviewer ()
		{
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public override TextFormat InputFormat {
			get { return TextFormat.Html; }
		}

		static bool IsWhiteSpace (char c)
		{
			return char.IsWhiteSpace (c) || (c >= 0x200B && c <= 0x200D);
		}

		static bool Append (char[] preview, ref int previewLength, string value, ref bool lwsp)
		{
			int i;

			for (i = 0; i < value.Length && previewLength < preview.Length; i++) {
				if (IsWhiteSpace (value[i])) {
					if (!lwsp) {
						preview[previewLength++] = ' ';
						lwsp = true;
					}
				} else {
					preview[previewLength++] = value[i];
					lwsp = false;
				}
			}

			if (i < value.Length) {
				if (lwsp)
					previewLength--;

				preview[previewLength - 1] = '\u2026';
				lwsp = false;
				return true;
			}

			return false;
		}

		sealed class HtmlTagContext
		{
			public HtmlTagContext (HtmlTagId id)
			{
				TagId = id;
			}

			public HtmlTagId TagId {
				get;
			}

			public int ListIndex {
				get; set;
			}

			public bool SuppressInnerContent {
				get; set;
			}

			// Links in the document-order list of open elements.
			public HtmlTagContext? Outer, Inner;

			// The next (outer) open element with the same TagId.
			public HtmlTagContext? OuterSameId;

			// Monotonically increasing push order, used to determine which of two elements is innermost.
			public long Sequence;
		}

		/// <summary>
		/// The set of currently-open elements.
		/// </summary>
		/// <remarks>
		/// <para>Every operation is O(1). A flat list is not used because closing an element by tag id and
		/// locating the innermost list element would each require an O(depth) scan, which allows a small,
		/// maliciously-crafted document (e.g. many unclosed tags followed by many unmatched end tags or
		/// <c>&lt;li&gt;</c> tags) to consume an amount of CPU time that is quadratic in its size.</para>
		/// <para>Open elements are kept in a doubly-linked list (so that an element can be removed from the
		/// middle) and are also chained by <see cref="HtmlTagId"/> (so that the innermost element with a given
		/// id can be found without scanning).</para>
		/// </remarks>
		sealed class OpenElementStack
		{
			readonly Dictionary<HtmlTagId, HtmlTagContext> innermostById = new Dictionary<HtmlTagId, HtmlTagContext> ();
			HtmlTagContext? innermost;
			long sequence;

			public bool SuppressContent {
				get { return innermost != null && innermost.SuppressInnerContent; }
			}

			public void Push (HtmlTagContext ctx)
			{
				innermostById.TryGetValue (ctx.TagId, out ctx.OuterSameId);
				innermostById[ctx.TagId] = ctx;
				ctx.Sequence = sequence++;

				ctx.Outer = innermost;
				if (innermost != null)
					innermost.Inner = ctx;
				innermost = ctx;
			}

			public void Pop (HtmlTagId id)
			{
				if (!innermostById.TryGetValue (id, out var ctx))
					return;

				if (ctx.OuterSameId != null)
					innermostById[id] = ctx.OuterSameId;
				else
					innermostById.Remove (id);

				if (ctx.Inner != null)
					ctx.Inner.Outer = ctx.Outer;
				else
					innermost = ctx.Outer;

				if (ctx.Outer != null)
					ctx.Outer.Inner = ctx.Inner;

				ctx.Outer = ctx.Inner = ctx.OuterSameId = null;
			}

			public HtmlTagContext? GetListItemContext ()
			{
				innermostById.TryGetValue (HtmlTagId.OL, out var ol);
				innermostById.TryGetValue (HtmlTagId.UL, out var ul);

				if (ol is null)
					return ul;

				if (ul is null)
					return ol;

				return ol.Sequence > ul.Sequence ? ol : ul;
			}

			public void Clear ()
			{
				innermostById.Clear ();
				innermost = null;
			}
		}

		static bool ShouldSuppressInnerContent (HtmlTagId id)
		{
			switch (id) {
			case HtmlTagId.OL:
			case HtmlTagId.Script:
			case HtmlTagId.Style:
			case HtmlTagId.Table:
			case HtmlTagId.TBody:
			case HtmlTagId.THead:
			case HtmlTagId.TR:
			case HtmlTagId.UL:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Get a text preview of a stream of text.
		/// </summary>
		/// <remarks>
		/// Gets a text preview of a stream of text.
		/// </remarks>
		/// <param name="reader">The original text stream.</param>
		/// <returns>A string representing a shortened preview of the original text.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="reader"/> is <see langword="null"/>.
		/// </exception>
		public override string GetPreviewText (TextReader reader)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			var tokenizer = new HtmlTokenizer (reader) { IgnoreTruncatedTags = true };
			var preview = new char[MaximumPreviewLength];
			var stack = new OpenElementStack ();
			var prefix = string.Empty;
			int previewLength = 0;
			HtmlTagContext? ctx;
			HtmlAttribute? attr;
			bool body = false;
			bool full = false;
			bool lwsp = true;

			while (!full && tokenizer.ReadNextToken (out var token)) {
				switch (token.Kind) {
				case HtmlTokenKind.Tag:
					var tag = (HtmlTagToken) token;

					if (!tag.IsEndTag) {
						if (body) {
							switch (tag.Id) {
							case HtmlTagId.Image:
								if ((attr = tag.Attributes.FirstOrDefault (x => x.Id == HtmlAttributeId.Alt)) != null) {
									full = Append (preview, ref previewLength, prefix + attr.Value, ref lwsp);
									prefix = string.Empty;
								}
								break;
							case HtmlTagId.LI:
								if ((ctx = stack.GetListItemContext ()) != null) {
									if (ctx.TagId == HtmlTagId.OL) {
										full = Append (preview, ref previewLength, $" {++ctx.ListIndex}. ", ref lwsp);
										prefix = string.Empty;
									} else {
										//full = Append (preview, ref previewLength, " \u2022 ", ref lwsp);
										prefix = " ";
									}
								}
								break;
							case HtmlTagId.Br:
							case HtmlTagId.P:
								prefix = " ";
								break;
							}

							if (!tag.IsEmptyElement || tokenizer.TokenizerState != HtmlTokenizerState.Data) {
								ctx = new HtmlTagContext (tag.Id) {
									SuppressInnerContent = ShouldSuppressInnerContent (tag.Id)
								};
								stack.Push (ctx);
							}
						} else if (tag.Id == HtmlTagId.Body && !tag.IsEmptyElement) {
							body = true;
						}
					} else if (tag.Id == HtmlTagId.Body) {
						stack.Clear ();
						body = false;
					} else {
						stack.Pop (tag.Id);
					}
					break;
				case HtmlTokenKind.Data:
					if (body && !stack.SuppressContent) {
						var data = (HtmlDataToken) token;

						full = Append (preview, ref previewLength, prefix + data.Data, ref lwsp);
						prefix = string.Empty;
					}
					break;
				}
			}

			if (lwsp && previewLength > 0)
				previewLength--;

			return new string (preview, 0, previewLength);
		}
	}
}
