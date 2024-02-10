//
// HtmlToHtml.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// An HTML to HTML converter.
	/// </summary>
	/// <remarks>
	/// Used to convert HTML into HTML.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class HtmlToHtml : TextConverter
	{
		//static readonly HashSet<string> AutoClosingTags;

		//static HtmlToHtml ()
		//{
		//	// Note: These are tags that auto-close when an identical tag is encountered and/or when a parent node is closed.
		//	AutoClosingTags = new HashSet<string> (new [] {
		//		"li",
		//		"p",
		//		"td",
		//		"tr"
		//	}, MimeUtils.OrdinalIgnoreCase);
		//}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlToHtml"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new HTML to HTML converter.
		/// </remarks>
		public HtmlToHtml ()
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
			get { return TextFormat.Html; }
		}

		/// <summary>
		/// Get or set whether or not the converter should remove HTML comments from the output.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the converter should remove HTML comments from the output.
		/// </remarks>
		/// <value><c>true</c> if the converter should remove comments; otherwise, <c>false</c>.</value>
		public bool FilterComments {
			get; set;
		}

		/// <summary>
		/// Get or set whether or not executable scripts should be stripped from the output.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not executable scripts should be stripped from the output.
		/// </remarks>
		/// <value><c>true</c> if executable scripts should be filtered; otherwise, <c>false</c>.</value>
		public bool FilterHtml {
			get; set;
		}

		/// <summary>
		/// Get or set the footer format.
		/// </summary>
		/// <remarks>
		/// Gets or sets the footer format.
		/// </remarks>
		/// <value>The footer format.</value>
		public HeaderFooterFormat FooterFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the header format.
		/// </summary>
		/// <remarks>
		/// Gets or sets the header format.
		/// </remarks>
		/// <value>The header format.</value>
		public HeaderFooterFormat HeaderFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the <see cref="HtmlTagCallback"/> method to use for custom
		/// filtering of HTML tags and content.
		/// </summary>
		/// <remarks>
		/// Get or set the <see cref="HtmlTagCallback"/> method to use for custom
		/// filtering of HTML tags and content.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The html tag callback.</value>
		public HtmlTagCallback HtmlTagCallback {
			get; set;
		}

#if false
		/// <summary>
		/// Get or set whether or not the converter should collapse white space,
		/// balance tags, and fix other problems in the source HTML.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the converter should collapse white space,
		/// balance tags, and fix other problems in the source HTML.
		/// </remarks>
		/// <value><c>true</c> if the output html should be normalized; otherwise, <c>false</c>.</value>
		public bool NormalizeHtml {
			get; set;
		}
#endif

#if false
		/// <summary>
		/// Get or set whether or not the converter should only output an HTML fragment.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the converter should only output an HTML fragment.
		/// </remarks>
		/// <value><c>true</c> if the converter should only output an HTML fragment; otherwise, <c>false</c>.</value>
		public bool OutputHtmlFragment {
			get; set;
		}
#endif

		class HtmlToHtmlTagContext : HtmlTagContext
		{
			readonly HtmlTagToken tag;

			public HtmlToHtmlTagContext (HtmlTagToken htmlTag) : base (htmlTag.Id)
			{
				tag = htmlTag;
			}

			public override string TagName {
				get { return tag.Name; }
			}

			public override HtmlAttributeCollection Attributes {
				get { return tag.Attributes; }
			}

			public override bool IsEmptyElementTag {
				get { return tag.IsEmptyElement || tag.Id.IsEmptyElement (); }
			}

			public override bool IsEndTag {
				get { return tag.IsEndTag; }
			}
		}

		static void DefaultHtmlTagCallback (HtmlTagContext tagContext, HtmlWriter htmlWriter)
		{
			tagContext.WriteTag (htmlWriter, true);
		}

		static bool SuppressContent (IList<HtmlToHtmlTagContext> stack)
		{
			for (int i = stack.Count; i > 0; i--) {
				if (stack[i - 1].SuppressInnerContent)
					return true;
			}

			return false;
		}

		static HtmlToHtmlTagContext Pop (IList<HtmlToHtmlTagContext> stack, string name)
		{
			for (int i = stack.Count; i > 0; i--) {
				if (stack[i - 1].TagName.Equals (name, StringComparison.OrdinalIgnoreCase)) {
					var ctx = stack[i - 1];
					stack.RemoveAt (i - 1);
					return ctx;
				}
			}

			return null;
		}

		/// <summary>
		/// Convert the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </remarks>
		/// <param name="reader">The text reader.</param>
		/// <param name="writer">The text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <c>null</c>.</para>
		/// </exception>
		public override void Convert (TextReader reader, TextWriter writer)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header)) {
				if (HeaderFormat == HeaderFooterFormat.Text) {
					var converter = new TextToHtml { OutputHtmlFragment = true };

					using (var sr = new StringReader (Header))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Header);
				}
			}

			using (var htmlWriter = new HtmlWriter (writer)) {
				var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
				var stack = new List<HtmlToHtmlTagContext> ();
				var tokenizer = new HtmlTokenizer (reader) {
					DecodeCharacterReferences = false
				};
				HtmlToHtmlTagContext ctx;

				while (tokenizer.ReadNextToken (out var token)) {
					switch (token.Kind) {
					default:
						if (!SuppressContent (stack))
							htmlWriter.WriteToken (token);
						break;
					case HtmlTokenKind.Comment:
						if (!FilterComments && !SuppressContent (stack))
							htmlWriter.WriteToken (token);
						break;
					case HtmlTokenKind.Tag:
						var tag = (HtmlTagToken) token;

						if (!tag.IsEndTag) {
							//if (NormalizeHtml && AutoClosingTags.Contains (startTag.TagName) &&
							//	(ctx = Pop (stack, startTag.TagName)) != null &&
							//	ctx.InvokeCallbackForEndTag && !SuppressContent (stack)) {
							//	var value = $"</{ctx.TagName}>";
							//	var name = ctx.TagName;
							//
							//	ctx = new HtmlToHtmlTagContext (new HtmlTokenTag (HtmlTokenKind.EndTag, name, value)) {
							//		InvokeCallbackForEndTag = ctx.InvokeCallbackForEndTag,
							//		SuppressInnerContent = ctx.SuppressInnerContent,
							//		DeleteEndTag = ctx.DeleteEndTag,
							//		DeleteTag = ctx.DeleteTag
							//	};
							//	callback (ctx, htmlWriter);
							//}

							if (!tag.IsEmptyElement) {
								ctx = new HtmlToHtmlTagContext (tag);

								if (FilterHtml && ctx.TagId == HtmlTagId.Script) {
									ctx.SuppressInnerContent = true;
									ctx.DeleteEndTag = true;
									ctx.DeleteTag = true;
								} else if (!SuppressContent (stack)) {
									callback (ctx, htmlWriter);
								}

								stack.Add (ctx);
							} else if (!SuppressContent (stack)) {
								ctx = new HtmlToHtmlTagContext (tag);

								if (!FilterHtml || ctx.TagId != HtmlTagId.Script)
									callback (ctx, htmlWriter);
							}
						} else {
							if ((ctx = Pop (stack, tag.Name)) != null) {
								if (!SuppressContent (stack)) {
									if (ctx.InvokeCallbackForEndTag) {
										ctx = new HtmlToHtmlTagContext (tag) {
											InvokeCallbackForEndTag = ctx.InvokeCallbackForEndTag,
											SuppressInnerContent = ctx.SuppressInnerContent,
											DeleteEndTag = ctx.DeleteEndTag,
											DeleteTag = ctx.DeleteTag
										};
										callback (ctx, htmlWriter);
									} else if (!ctx.DeleteEndTag) {
										htmlWriter.WriteEndTag (tag.Name);
									}
								}
							} else if (!SuppressContent (stack)) {
								ctx = new HtmlToHtmlTagContext (tag);
								callback (ctx, htmlWriter);
							}
						}
						break;
					}
				}

				htmlWriter.Flush ();
			}

			if (!string.IsNullOrEmpty (Footer)) {
				if (FooterFormat == HeaderFooterFormat.Text) {
					var converter = new TextToHtml { OutputHtmlFragment = true };

					using (var sr = new StringReader (Footer))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Footer);
				}
			}
		}
	}
}
