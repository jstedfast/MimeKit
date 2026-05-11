//
// HtmlToHtml.cs
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
		/// Get or set whether the converter should remove HTML comments from the output.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the converter should remove HTML comments from the output.
		/// </remarks>
		/// <value><see langword="true" /> if the converter should remove comments; otherwise, <see langword="false" />.</value>
		public bool FilterComments {
			get; set;
		}

		/// <summary>
		/// Get or set whether undesirable tags should be stripped from the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether undesirable tags should be stripped from the output.</para>
		/// <note type="warning">
		/// <para>This is an incomplete solution for protecting against Cross-Site Scripting (XSS) attacks
		/// and should not be relied upon as a comprehensive security measure. This filter only removes
		/// certain known dangerous or undesirable HTML tags (such as <c>&lt;applet&gt;</c>, <c>&lt;audio&gt;</c>,
		/// <c>&lt;base&gt;</c>, <c>&lt;dialog&gt;</c>, <c>&lt;embed&gt;</c>, <c>&lt;form&gt;</c>, <c>&lt;frame&gt;</c>,
		/// <c>&lt;frameset&gt;</c>, <c>&lt;iframe&gt;</c>, <c>&lt;input&gt;</c>, <c>&lt;link&gt;</c>, <c>&lt;object&gt;</c>,
		/// <c>&lt;script&gt;</c>, <c>&lt;select&gt;</c>, <c>&lt;source&gt;</c>, <c>&lt;style&gt;</c>, <c>&lt;textarea&gt;</c>,
		/// <c>&lt;video&gt;</c>, and any tags without an enum mapping) but does not:</para>
		/// <list type="bullet">
		/// <item><description>Validate or sanitize attribute values (e.g., <c>javascript:</c>, <c>data:</c>,
		/// or <c>vbscript:</c> URI schemes in <c>href</c>, <c>src</c>, or other URL attributes)</description></item>
		/// <item><description>Filter event handler attributes (e.g., <c>onclick</c>, <c>onerror</c>, <c>onload</c>, etc.)</description></item>
		/// <item><description>Sanitize inline CSS that may contain expressions or imports</description></item>
		/// <item><description>Protect against newly discovered XSS attack vectors or techniques</description></item>
		/// </list>
		/// <para>For robust XSS protection, it is strongly recommended that applications pass the HTML output through
		/// a dedicated HTML sanitizer library (such as HtmlSanitizer, Ganss.XSS, or similar) that is actively maintained
		/// and updated to address emerging security threats.</para>
		/// </note>
		/// </remarks>
		/// <value><see langword="true" /> if undesirable tags should be filtered; otherwise, <see langword="false" />.</value>
		[Obsolete ("This is an incomplete solution for protecting against Cross-Site Scripting (XSS) attacks and should not be relied upon as a comprehensive security measure. For robust XSS protection, it is strongly recommended that applications pass the HTML output through a dedicated HTML sanitizer library that is actively maintained and updated to address emerging security threats.")]
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
		public HtmlTagCallback? HtmlTagCallback {
			get; set;
		}

#if false
		/// <summary>
		/// Get or set whether the converter should collapse white space,
		/// balance tags, and fix other problems in the source HTML.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the converter should collapse white space,
		/// balance tags, and fix other problems in the source HTML.
		/// </remarks>
		/// <value><see langword="true" /> if the output html should be normalized; otherwise, <see langword="false" />.</value>
		public bool NormalizeHtml {
			get; set;
		}
#endif

#if false
		/// <summary>
		/// Get or set whether the converter should only output an HTML fragment.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the converter should only output an HTML fragment.
		/// </remarks>
		/// <value><see langword="true" /> if the converter should only output an HTML fragment; otherwise, <see langword="false" />.</value>
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

		static HtmlToHtmlTagContext? Pop (IList<HtmlToHtmlTagContext> stack, string name)
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
		/// Determines whether the HTML tag is considered unsafe or undesirable in email contexts.
		/// </summary>
		/// <remarks>
		/// <para>Determines whether the HTML tag is considered unsafe or undesirable in email contexts.</para>
		/// <para>Some of these tags are known to be abused for Cross-Site Scripting attacks while others are,
		/// at best, questionable for use with HTML email due to the fact that they are interactive.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the HTML tag is a known XSS attack vector; otherwise, <see langword="false" />.</returns>
		/// <param name="id">The HTML tag identifier.</param>
		static bool IsUnsafe (HtmlTagId id)
		{
			switch (id) {
			case HtmlTagId.Unknown:     // Unknown tags could potentially be dangerous
			case HtmlTagId.Applet:      // Can execute Java applets
			case HtmlTagId.Audio:       // Can embed audio with potentially malicious content
			case HtmlTagId.Base:        // Can hijack relative URLs
			case HtmlTagId.Embed:       // Can embed executable content
			case HtmlTagId.Form:        // Can submit data to an attacker's server
			case HtmlTagId.Frame:       // Embeds external (and thus unsafe) content
			case HtmlTagId.FrameSet:    // Container for frames
			case HtmlTagId.IFrame:      // Embeds external (and thus unsafe) content
			case HtmlTagId.Input:       // Can be used to steal user input or trigger actions
			case HtmlTagId.Link:        // Can load external stylesheets that execute in certain contexts
			case HtmlTagId.Object:      // Can embed executable content
			case HtmlTagId.Script:      // Direct script execution
			case HtmlTagId.Select:      // Can be used to steal user input or trigger actions
			case HtmlTagId.Source:      // Can be used to define alternative audio or video sources
			case HtmlTagId.Style:       // Can contain CSS with expression() or import of malicious content
			case HtmlTagId.TextArea:    // Can be used to steal user input or trigger actions
			case HtmlTagId.Video:       // Can embed video with potentially malicious content
				return true;
			default:
				return false;
			}
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
		/// <para><paramref name="reader"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <see langword="null"/>.</para>
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

			using (var htmlWriter = new HtmlWriter (writer, true)) {
				var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
				var stack = new List<HtmlToHtmlTagContext> ();
				var tokenizer = new HtmlTokenizer (reader) {
					DecodeCharacterReferences = false
				};
				HtmlToHtmlTagContext? ctx;

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

								if (FilterHtml && IsUnsafe (ctx.TagId)) {
									ctx.SuppressInnerContent = true;
									ctx.DeleteEndTag = true;
									ctx.DeleteTag = true;
								} else if (!SuppressContent (stack)) {
									callback (ctx, htmlWriter);
								}

								stack.Add (ctx);
							} else if (!SuppressContent (stack)) {
								ctx = new HtmlToHtmlTagContext (tag);

								if (!FilterHtml || !IsUnsafe (ctx.TagId))
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
