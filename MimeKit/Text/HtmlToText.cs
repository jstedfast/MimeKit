//
// HtmlToText.cs
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
using System.Globalization;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// An HTML to plain text converter.
	/// </summary>
	/// <remarks>
	/// <para>Converts HTML documents or fragments into plain text, preserving paragraphs, line breaks,
	/// headings, lists, blockquotes and preformatted text. Headings use hash prefixes, lists use
	/// bullets or decimal numbers, and quoted lines use greater-than prefixes.</para>
	/// <para>Scripts, styles, titles, templates and elements with a hidden attribute are omitted.
	/// CSS is not evaluated, inline emphasis is reduced to its text, and tables are flattened without
	/// borders or column alignment. The output is plain text, not a Markdown serialization.</para>
	/// <para>Output is not word-wrapped. Generated line endings use <see cref="TextWriter.NewLine"/>.
	/// Structural spacing at the end of the document is omitted, while explicit line breaks and
	/// preformatted whitespace are retained. The inherited header and footer are literal text.</para>
	/// </remarks>
	public class HtmlToText : TextConverter
	{
		int maxElementDepth = HtmlTokenizer.DefaultMaxElementDepth;
		int maxInputLength = 16 * 1024 * 1024;
		int maxOutputLength = 64 * 1024 * 1024;

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlToText"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new HTML to plain text converter.
		/// </remarks>
		public HtmlToText ()
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

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public override TextFormat OutputFormat {
			get { return TextFormat.Plain; }
		}

		/// <summary>
		/// Get or set whether link destinations should be included in the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether link destinations are appended in parentheses after their text.
		/// A destination that is identical to the link text is not repeated. Links without text output
		/// their destination without parentheses. Relative destinations are not resolved.</para>
		/// <para>The default value is <see langword="true"/>.</para>
		/// </remarks>
		/// <value><see langword="true"/> to include link destinations; otherwise, <see langword="false"/>.</value>
		public bool OutputLinkUrls {
			get; set;
		} = true;

		/// <summary>
		/// Get or set whether image alternative text should be included in the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether image alternative text should be included in the output.
		/// Images without alternative text are omitted. No images or other external resources are fetched.</para>
		/// <para>The default value is <see langword="true"/>.</para>
		/// </remarks>
		/// <value><see langword="true"/> to include image alternative text; otherwise, <see langword="false"/>.</value>
		public bool OutputImageAltText {
			get; set;
		} = true;

		/// <summary>
		/// Get or set the maximum element depth.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of open elements tracked by the converter.
		/// Unlike the tokenizer, the converter counts repeated nested elements separately.
		/// Exceeding this limit throws a <see cref="FormatException"/>.</para>
		/// <para>The default value is <c>4096</c>.</para>
		/// </remarks>
		/// <value>The maximum element depth.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxElementDepth {
			get { return maxElementDepth; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxElementDepth = value;
			}
		}

		/// <summary>
		/// Get or set the maximum input length.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of UTF-16 characters read from the input, including
		/// markup and suppressed content. This also bounds individual tokens and retained attributes.
		/// Exceeding this limit throws a <see cref="FormatException"/>.</para>
		/// <para>The default value is <c>16777216</c>. The header and footer are not included in this limit.</para>
		/// </remarks>
		/// <value>The maximum input length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxInputLength {
			get { return maxInputLength; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxInputLength = value;
			}
		}

		/// <summary>
		/// Get or set the maximum output length.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of UTF-16 characters written by the converter, including
		/// generated indentation, link destinations and line endings. This bounds expansion caused by
		/// deeply nested lists or blockquotes. Exceeding this limit throws a <see cref="FormatException"/>.</para>
		/// <para>The default value is <c>67108864</c>. The literal header and footer are not included in this limit.</para>
		/// </remarks>
		/// <value>The maximum output length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxOutputLength {
			get { return maxOutputLength; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxOutputLength = value;
			}
		}

		sealed class LimitedReader : TextReader
		{
			readonly TextReader source;
			int remaining;

			public LimitedReader (TextReader source, int limit)
			{
				this.source = source;
				remaining = limit;
			}

			public override int Read ()
			{
				int c = source.Read ();

				if (c != -1 && remaining-- == 0)
					throw new FormatException ("The HTML input length limit has been exceeded.");

				return c;
			}

			public override int Read (char[] buffer, int index, int count)
			{
				if (count == 0)
					return 0;

				if (remaining == 0) {
					Read ();
					return 0;
				}

				int nread = source.Read (buffer, index, Math.Min (remaining, count));
				remaining -= nread;
				return nread;
			}
		}

		sealed class Element : HtmlTagContext
		{
			readonly HtmlTagToken tag;

			public Element (HtmlTagToken tag, int index) : base (tag.Id)
			{
				this.tag = tag;
				Index = index;
			}

			public override HtmlAttributeCollection Attributes { get { return tag.Attributes; } }
			public override string TagName { get { return tag.Name; } }
			public override bool IsEmptyElementTag { get { return false; } }
			public override bool IsEndTag { get { return false; } }

			public readonly int Index;
			public int Indent, QuoteDepth, PreDepth;
			public Element? List, Item, Link, Heading;
			public bool Visible, LinkMatches = true;
			public string? Url;
			public int LinkLength;
			public long NextNumber;
		}

		sealed class Converter
		{
			static readonly string[] ParagraphScopes = {
				"button", "table", "td", "th", "caption", "applet", "marquee", "object", "template"
			};
			readonly HtmlTagContextStack<Element> contexts = new HtmlTagContextStack<Element> ();
			readonly List<Element> elements = new List<Element> ();
			readonly HtmlToText options;
			readonly TextWriter writer;
			Element? list, item, link, heading;
			int indent, quoteDepth, preDepth;
			int pendingBreaks, newLines, outputLength;
			bool lineStart = true, hasOutput, pendingSpace;
			bool skipPreNewLine, previousCR;
			string? marker;

			public Converter (HtmlToText options, TextWriter writer)
			{
				this.options = options;
				this.writer = writer;
			}

			static bool IsWhiteSpace (char c)
			{
				return c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f';
			}

			static bool IsHeading (HtmlTagId id)
			{
				return id >= HtmlTagId.H1 && id <= HtmlTagId.H6;
			}

			static bool IsList (HtmlTagId id)
			{
				return id == HtmlTagId.OL || id == HtmlTagId.UL || id == HtmlTagId.Menu || id == HtmlTagId.Dir;
			}

			static int Spacing (HtmlTagId id)
			{
				switch (id) {
				case HtmlTagId.P: case HtmlTagId.Pre: case HtmlTagId.Listing: case HtmlTagId.Xmp:
				case HtmlTagId.PlainText:
				case HtmlTagId.BlockQuote: case HtmlTagId.Table: case HtmlTagId.DL:
				case HtmlTagId.H1: case HtmlTagId.H2: case HtmlTagId.H3:
				case HtmlTagId.H4: case HtmlTagId.H5: case HtmlTagId.H6:
					return 2;
				case HtmlTagId.Address: case HtmlTagId.Article: case HtmlTagId.Aside:
				case HtmlTagId.Center: case HtmlTagId.Details: case HtmlTagId.Dialog:
				case HtmlTagId.Div: case HtmlTagId.FieldSet: case HtmlTagId.FigCaption:
				case HtmlTagId.Figure: case HtmlTagId.Footer: case HtmlTagId.Form:
				case HtmlTagId.Header: case HtmlTagId.Main: case HtmlTagId.Nav:
				case HtmlTagId.Section: case HtmlTagId.Summary: case HtmlTagId.TR:
				case HtmlTagId.Caption: case HtmlTagId.DD: case HtmlTagId.DT:
				case HtmlTagId.TextArea:
				case HtmlTagId.LI: case HtmlTagId.OL: case HtmlTagId.UL:
				case HtmlTagId.Menu: case HtmlTagId.Dir:
					return 1;
				default:
					return 0;
				}
			}

			static bool SuppressesContent (HtmlTagToken tag)
			{
				switch (tag.Id) {
				case HtmlTagId.Script: case HtmlTagId.Style: case HtmlTagId.Title:
				case HtmlTagId.IFrame: case HtmlTagId.NoFrames:
					return true;
				default:
					return tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase) ||
						tag.Attributes.TryGetValue ("hidden", out _);
				}
			}

			void Boundary (int count)
			{
				if (marker == null)
					pendingBreaks = Math.Max (pendingBreaks, count);
				pendingSpace = false;
			}

			void ReserveOutput (int count)
			{
				if (count > options.MaxOutputLength - outputLength)
					throw new FormatException ("The HTML output length limit has been exceeded.");

				outputLength += count;
			}

			void WriteOutput (char c)
			{
				ReserveOutput (1);
				writer.Write (c);
			}

			void WriteOutput (string text)
			{
				ReserveOutput (text.Length);
				writer.Write (text);
			}

			void WriteNewLine ()
			{
				ReserveOutput (writer.NewLine.Length);
				writer.WriteLine ();
			}

			void FlushBreaks ()
			{
				if (hasOutput && pendingBreaks > newLines) {
					for (int i = newLines; i < pendingBreaks; i++)
						WriteNewLine ();
					lineStart = true;
					newLines = pendingBreaks;
					pendingSpace = false;
				}

				pendingBreaks = 0;
			}

			void WriteCharacter (char c)
			{
				FlushBreaks ();

				if (pendingSpace && !lineStart) {
					WriteOutput (' ');
					TrackLinkCharacter (' ');
				}
				pendingSpace = false;

				if (lineStart) {
					for (int i = 0; i < quoteDepth; i++)
						WriteOutput ('>');

					if (quoteDepth > 0)
						WriteOutput (' ');

					int padding = marker != null ? indent - marker.Length : indent;

					for (int i = 0; i < padding; i++)
						WriteOutput (' ');

					if (marker != null) {
						WriteOutput (marker);
						marker = null;
					}

					lineStart = false;
				}

				WriteOutput (c);
				TrackLinkCharacter (c);
				hasOutput = true;
				newLines = 0;
			}

			void TrackLinkCharacter (char c)
			{
				if (link?.Url != null) {
					if (link.LinkLength == 0 && c == ' ')
						return;

					if (link.LinkLength < link.Url.Length && link.Url[link.LinkLength] == c) {
						link.LinkLength++;
					} else {
						link.LinkMatches = false;
						link.LinkLength = link.Url.Length + 1;
					}
				}
			}

			void WriteText (ReadOnlySpan<char> text)
			{
				foreach (char c in text) {
					if (IsWhiteSpace (c))
						pendingSpace = true;
					else
						WriteCharacter (c);
				}
			}

			void LineBreak ()
			{
				FlushBreaks ();

				if (marker != null) {
					int continuationIndent = indent;
					FinishMarker ();
					indent = continuationIndent;
				}

				WriteNewLine ();

				hasOutput = true;
				lineStart = true;
				newLines = Math.Min (newLines + 1, 2);
				pendingSpace = false;

				TrackLinkCharacter ('\n');
			}

			void WritePreformatted (ReadOnlySpan<char> text)
			{
				foreach (char c in text) {
					if (previousCR && c == '\n') {
						previousCR = false;
						continue;
					}

					previousCR = c == '\r';

					if (skipPreNewLine) {
						skipPreNewLine = false;

						if (c == '\r' || c == '\n')
							continue;
					}

					if (c == '\r' || c == '\n')
						LineBreak ();
					else
						WriteCharacter (c);
				}
			}

			void FinishMarker ()
			{
				if (marker == null)
					return;

				string text = marker.TrimEnd ();
				indent -= marker.Length;
				marker = null;

				WriteText (text.AsSpan ());
			}

			void CloseAt (int index)
			{
				while (elements.Count > index) {
					var element = elements[elements.Count - 1];

					if (element.Visible) {
						if (element.TagId == HtmlTagId.A && element.Url != null) {
							link = null;
							int breaks = pendingBreaks;
							pendingBreaks = 0;

							if (element.LinkLength == 0) {
								WriteText (element.Url.AsSpan ());
							} else if (!element.LinkMatches || element.LinkLength != element.Url.Length) {
								WriteText (" (".AsSpan ());
								WriteText (element.Url.AsSpan ());
								WriteCharacter (')');
							}

							pendingBreaks = Math.Max (pendingBreaks, breaks);
						}

						if (element.TagId == HtmlTagId.LI) {
							FinishMarker ();
							pendingBreaks = 1;
						}

						int spacing = Spacing (element.TagId);

						if (IsList (element.TagId))
							spacing = element.List == null ? 2 : 1;

						if (spacing > 0)
							Boundary (spacing);

						if (element.TagId == HtmlTagId.TD || element.TagId == HtmlTagId.TH)
							pendingSpace = true;
					}

					indent = element.Indent;
					quoteDepth = element.QuoteDepth;
					preDepth = element.PreDepth;
					list = element.List;
					item = element.Item;
					link = element.Link;
					heading = element.Heading;
					contexts.Pop (element.TagName);
					elements.RemoveAt (elements.Count - 1);
					previousCR = skipPreNewLine = false;
				}
			}

			void Close (string name)
			{
				var element = contexts.Peek (name);

				if (element != null)
					CloseAt (element.Index);
			}

			void CloseInScope (string name, string scope)
			{
				var element = contexts.Peek (name);
				var boundary = contexts.Peek (scope);

				if (element != null && IsInTemplateScope (element) && (boundary == null || element.Index > boundary.Index))
					CloseAt (element.Index);
			}

			bool IsInTemplateScope (Element element)
			{
				var template = contexts.Peek ("template");

				return template == null || element.Index > template.Index;
			}

			void CloseParagraph ()
			{
				var paragraph = contexts.Peek ("p");

				if (paragraph == null)
					return;

				foreach (var name in ParagraphScopes) {
					var boundary = contexts.Peek (name);

					if (boundary != null && boundary.Index > paragraph.Index)
						return;
				}

				CloseAt (paragraph.Index);
			}

			static long GetNumber (HtmlTagToken tag, string name, long defaultValue)
			{
				if (tag.Attributes.TryGetValue (name, out var attribute) &&
					int.TryParse (attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
					return value;

				return defaultValue;
			}

			void StartTag (HtmlTagToken tag)
			{
				if (tag.Id == HtmlTagId.Head)
					return;

				if ((tag.Id == HtmlTagId.Html || tag.Id == HtmlTagId.Body) &&
					!contexts.SuppressContent && !tag.Attributes.TryGetValue ("hidden", out _))
					return;

				if (Spacing (tag.Id) > 0)
					CloseParagraph ();

				if (tag.Id == HtmlTagId.LI && item != null && item.List == list && IsInTemplateScope (item))
					CloseAt (item.Index);

				if (IsHeading (tag.Id) && heading != null && IsInTemplateScope (heading))
					CloseAt (heading.Index);

				if (tag.Id == HtmlTagId.A)
					CloseInScope ("a", "template");

				if (tag.Id == HtmlTagId.DD || tag.Id == HtmlTagId.DT) {
					CloseInScope ("dd", "dl");
					CloseInScope ("dt", "dl");
				}

				bool suppressed = contexts.SuppressContent || SuppressesContent (tag);

				if (tag.Id.IsEmptyElement ()) {
					if (!suppressed) {
						switch (tag.Id) {
						case HtmlTagId.Br:
							LineBreak ();
							break;
						case HtmlTagId.HR:
							Boundary (2);
							WriteText ("---".AsSpan ());
							Boundary (2);
							break;
						case HtmlTagId.Image:
							if (options.OutputImageAltText && tag.Attributes.TryGetValue (HtmlAttributeId.Alt, out var alt))
								WriteText (alt.Value.AsSpan ());
							break;
						}
					}
					return;
				}

				if (elements.Count >= options.MaxElementDepth)
					throw new FormatException ("The HTML element depth limit has been exceeded.");

				var element = new Element (tag, elements.Count) {
					Indent = indent, QuoteDepth = quoteDepth, PreDepth = preDepth,
					List = list, Item = item, Link = link, Heading = heading,
					SuppressInnerContent = suppressed,
					Visible = !suppressed
				};
				contexts.Push (element);
				elements.Add (element);

				if (suppressed) {
					if (IsList (tag.Id))
						list = element;

					if (tag.Id == HtmlTagId.LI)
						item = element;

					if (IsHeading (tag.Id))
						heading = element;
				} else {
					int spacing = Spacing (tag.Id);
					if (spacing > 0)
						Boundary (spacing);

					switch (tag.Id) {
					case HtmlTagId.BlockQuote:
						quoteDepth++;
						break;
					case HtmlTagId.Pre: case HtmlTagId.Listing: case HtmlTagId.Xmp:
					case HtmlTagId.TextArea: case HtmlTagId.PlainText:
						preDepth++;
						skipPreNewLine = tag.Id != HtmlTagId.Xmp && tag.Id != HtmlTagId.PlainText;
						previousCR = false;
						break;
					case HtmlTagId.OL: case HtmlTagId.UL: case HtmlTagId.Menu: case HtmlTagId.Dir:
						FinishMarker ();
						Boundary (list == null ? 2 : 1);
						element.NextNumber = GetNumber (tag, "start", 1);
						list = element;
						break;
					case HtmlTagId.LI:
						item = element;
						indent = list?.Indent ?? indent;

						if (list?.TagId == HtmlTagId.OL) {
							list.NextNumber = GetNumber (tag, "value", list.NextNumber);
							marker = list.NextNumber.ToString (CultureInfo.InvariantCulture) + ". ";
							list.NextNumber++;
						} else {
							marker = "- ";
						}

						indent += marker.Length;
						break;
					case HtmlTagId.DD:
						indent += 2;
						break;
					case HtmlTagId.A:
						if (options.OutputLinkUrls && tag.Attributes.TryGetValue (HtmlAttributeId.Href, out var href)) {
							element.Url = href.Value?.Trim ();

							if (!string.IsNullOrEmpty (element.Url))
								link = element;
						}
						break;
					case HtmlTagId.TD: case HtmlTagId.TH:
						pendingSpace = true;
						break;
					default:
						if (IsHeading (tag.Id)) {
							heading = element;
							WriteText ((new string ('#', tag.Id - HtmlTagId.H1 + 1) + " ").AsSpan ());
						}
						break;
					}
				}

				// HTML ignores self-closing syntax on non-void elements, including scripts and templates.
				if (tag.IsEmptyElement && tag.Id == HtmlTagId.Unknown &&
					!tag.Name.Equals ("template", StringComparison.OrdinalIgnoreCase))
					CloseAt (element.Index);
			}

			public void Convert (TextReader reader)
			{
				var tokenizer = new HtmlTokenizer (reader) {
					DecodeCharacterReferences = true,
					ScriptingEnabled = false,
					IgnoreTruncatedTags = true,
					ReuseDataTokens = true,
					MaxElementDepth = options.MaxElementDepth
				};
				var tree = tokenizer.TreeConstruction;
				tree.TrackClosedElements = true;

				while (tokenizer.ReadNextToken (out var token)) {
					if (tree.DepthExceeded)
						throw new FormatException ("The HTML element depth limit has been exceeded.");

					foreach (var closed in tree.ClosedElements) {
						for (int i = 0; i < closed.Count; i++)
							Close (closed.Name);
					}

					tree.ClearClosedElements ();

					if (token.Kind != HtmlTokenKind.Data && token.Kind != HtmlTokenKind.CData)
						previousCR = skipPreNewLine = false;

					if (token is HtmlTagToken tag) {
						if (tree.LastTag.Disposition == HtmlTreeConstruction.TagDisposition.Ignored)
							continue;

						if (tag.IsEndTag) {
							if (IsHeading (tag.Id) && heading != null)
								CloseAt (heading.Index);
							else if (tag.Id == HtmlTagId.Br && !contexts.SuppressContent)
								LineBreak ();
							else
								Close (tag.Name);
						} else if (tree.LastTag.Disposition != HtmlTreeConstruction.TagDisposition.AttributesMerged) {
							StartTag (tag);
						}
					} else if (!contexts.SuppressContent && !tree.IsInFrameset &&
						(token.Kind == HtmlTokenKind.Data || token.Kind == HtmlTokenKind.CData)) {
						var data = ((HtmlDataToken) token).DataSpan;

						if (preDepth > 0)
							WritePreformatted (data);
						else
							WriteText (data);
					}
				}

				CloseAt (0);
			}
		}

		/// <summary>
		/// Convert HTML to plain text.
		/// </summary>
		/// <remarks>
		/// Converts HTML to plain text without closing the reader or writer. If a conversion limit
		/// is exceeded or an I/O error occurs, the writer may already contain partial output.
		/// </remarks>
		/// <param name="reader">The HTML reader.</param>
		/// <param name="writer">The plain text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// An input length, output length or element depth limit was exceeded.
		/// </exception>
		public override void Convert (TextReader reader, TextWriter writer)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			using (var limited = new LimitedReader (reader, maxInputLength))
				new Converter (this, writer).Convert (limited);

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
