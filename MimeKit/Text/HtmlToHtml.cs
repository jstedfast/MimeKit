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

namespace MimeKit.Text {
	/// <summary>
	/// An HTML to HTML converter.
	/// </summary>
	/// <remarks>
	/// <para>Used to convert HTML into HTML.</para>
	/// <para>A tag that is truncated by the end of the input (e.g. <c>&lt;img src=x</c>) is dropped rather than
	/// being written to the output.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class HtmlToHtml : TextConverter
	{
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

		/// <summary>
		/// Get or set whether the HTML should be tokenized as if scripting is enabled.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the HTML should be tokenized as if scripting is enabled.</para>
		/// <para>This corresponds to the scripting flag described in the HTML5 specification and controls
		/// how the content of <c>&lt;noscript&gt;</c> elements is tokenized (see
		/// <see cref="HtmlTokenizer.ScriptingEnabled"/>). When <see langword="true" />, the content of a
		/// <c>&lt;noscript&gt;</c> element is treated as raw text and is written to the output verbatim
		/// <em>without</em> being passed to the <see cref="HtmlTagCallback"/>. When <see langword="false" />,
		/// the content is tokenized as normal markup and each tag is passed to the <see cref="HtmlTagCallback"/>.</para>
		/// <note type="security">
		/// <para>If the <see cref="HtmlTagCallback"/> is being used to filter the HTML (for example, to remove
		/// remote images or event handler attributes), then the output is only filtered correctly if this
		/// value matches whether scripting is enabled in the application that will eventually render the
		/// output. A mismatch in either direction allows content to bypass the callback:</para>
		/// <list type="bullet">
		/// <item><description>If this value is <see langword="true" /> but the output is rendered with
		/// scripting disabled (as is typical for email clients), markup inside <c>&lt;noscript&gt;</c>
		/// elements, such as tracking images, remote content and forms, is never seen by the callback but
		/// <em>is</em> rendered.</description></item>
		/// <item><description>If this value is <see langword="false" /> but the output is rendered with
		/// scripting enabled, a <c>&lt;/noscript&gt;</c> hidden inside a comment or a raw text element such as
		/// <c>&lt;style&gt;</c> within a <c>&lt;noscript&gt;</c> element will close the
		/// <c>&lt;noscript&gt;</c> element in the renderer, allowing markup that the callback never saw
		/// (including script event handlers) to be rendered. This is a cross-site scripting (XSS)
		/// vulnerability.</description></item>
		/// </list>
		/// <para>The default value is <see langword="true" /> because a mismatch then cannot lead to script
		/// execution, since any markup that bypasses the callback is only rendered when scripting is disabled.
		/// Applications that render with scripting disabled should set this value to <see langword="false" />
		/// so that the content of <c>&lt;noscript&gt;</c> elements is filtered. To produce output that is
		/// interpreted the same way regardless of whether scripting is enabled, set this value to
		/// <see langword="false" /> and remove the <c>&lt;noscript&gt;</c> start and end tags (but not their
		/// content) in the <see cref="HtmlTagCallback"/> by setting <see cref="HtmlTagContext.DeleteTag"/> and
		/// <see cref="HtmlTagContext.DeleteEndTag"/>.</para>
		/// <para>Note that <see cref="HtmlToHtml"/> is not an HTML sanitizer. For displaying untrusted HTML,
		/// use a dedicated HTML sanitizer library.</para>
		/// </note>
		/// </remarks>
		/// <value><see langword="true" /> if the HTML should be tokenized as if scripting is enabled; otherwise, <see langword="false" />.</value>
		public bool ScriptingEnabled {
			get; set;
		} = true;

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
				var stack = new HtmlTagContextStack<HtmlToHtmlTagContext> ();
				var tokenizer = new HtmlTokenizer (reader) {
					DecodeCharacterReferences = false,
					ScriptingEnabled = ScriptingEnabled,
					IgnoreTruncatedTags = true,
					ReuseDataTokens = true
				};
				HtmlToHtmlTagContext? ctx;

				while (tokenizer.ReadNextToken (out var token)) {
					switch (token.Kind) {
					default:
						if (!stack.SuppressContent)
							htmlWriter.WriteToken (token);
						break;
					case HtmlTokenKind.Comment:
						if (!FilterComments && !stack.SuppressContent)
							htmlWriter.WriteToken (token);
						break;
					case HtmlTokenKind.Tag:
						var tag = (HtmlTagToken) token;

						if (!tag.IsEndTag) {
							// Note: A self-closing tag such as <style/> or <script/> still switches the tokenizer into a raw content
							// state (browsers ignore the self-closing flag on non-void HTML elements), so treat it as an open element
							// so that SuppressInnerContent applies to the raw content that follows.
							if (!tag.IsEmptyElement || tokenizer.TokenizerState != HtmlTokenizerState.Data) {
								ctx = new HtmlToHtmlTagContext (tag);

								if (!stack.SuppressContent)
									callback (ctx, htmlWriter);

								stack.Push (ctx);
							} else if (!stack.SuppressContent) {
								ctx = new HtmlToHtmlTagContext (tag);
								callback (ctx, htmlWriter);
							}
						} else {
							if ((ctx = stack.Pop (tag.Name)) != null) {
								if (!stack.SuppressContent) {
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
							} else if (!stack.SuppressContent) {
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
