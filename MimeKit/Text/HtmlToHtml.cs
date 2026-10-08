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
		HtmlNoScriptHandling noScriptHandling = HtmlNoScriptHandling.Unwrap;
		int maxElementDepth = HtmlTokenizer.DefaultMaxElementDepth;

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
		/// Get or set the maximum element depth.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of distinct nested elements that will be tracked while
		/// tokenizing the HTML (see <see cref="HtmlTokenizer.MaxElementDepth"/>).</para>
		/// <para>Consecutive nested elements that are identical share a single entry, so arbitrarily deep nesting
		/// of the same element does not count towards this limit. If a start tag exceeds this limit, then it is the
		/// last tag passed to the <see cref="HtmlTagCallback"/> and the remainder of the input is written to the
		/// output as encoded text.</para>
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
		/// Get or set how <c>&lt;noscript&gt;</c> elements should be handled.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets how <c>&lt;noscript&gt;</c> elements should be handled.</para>
		/// <para>The HTML5 specification tokenizes the content of a <c>&lt;noscript&gt;</c> element differently
		/// depending on whether scripting is enabled in the application that renders the HTML (see
		/// <see cref="HtmlTokenizer.ScriptingEnabled"/>). When scripting is enabled, the content is raw text;
		/// when scripting is disabled, the content is normal markup.</para>
		/// <note type="security">
		/// <para>If the <see cref="HtmlTagCallback"/> is being used to filter the HTML (for example, to remove
		/// remote images or event handler attributes), then the output is only filtered correctly if the
		/// <c>&lt;noscript&gt;</c> content is tokenized the same way that the application that eventually renders
		/// the output will interpret it. A mismatch in either direction allows content to bypass the callback:</para>
		/// <list type="bullet">
		/// <item><description>When using <see cref="HtmlNoScriptHandling.ScriptingEnabled"/>, if the output is
		/// rendered with scripting disabled (as is typical for email clients), markup inside <c>&lt;noscript&gt;</c>
		/// elements, such as tracking images, remote content and forms, is never seen by the callback but
		/// <em>is</em> rendered.</description></item>
		/// <item><description>When using <see cref="HtmlNoScriptHandling.ScriptingDisabled"/>, if the output is
		/// rendered with scripting enabled, a <c>&lt;/noscript&gt;</c> hidden inside a comment or a raw text element
		/// such as <c>&lt;style&gt;</c> within a <c>&lt;noscript&gt;</c> element will close the
		/// <c>&lt;noscript&gt;</c> element in the renderer, allowing markup that the callback never saw
		/// (including script event handlers) to be rendered. This is a cross-site scripting (XSS)
		/// vulnerability.</description></item>
		/// </list>
		/// <para>The default value is <see cref="HtmlNoScriptHandling.Unwrap"/>, which tokenizes the content of
		/// <c>&lt;noscript&gt;</c> elements as normal markup (so that it is passed to the <see cref="HtmlTagCallback"/>)
		/// and removes the <c>&lt;noscript&gt;</c> start and end tags from the output. Since the output then contains no
		/// <c>&lt;noscript&gt;</c> elements, it is interpreted the same way regardless of whether scripting is enabled
		/// in the renderer. The trade-off is that the fallback content of <c>&lt;noscript&gt;</c> elements is always
		/// displayed, even by renderers that have scripting enabled.</para>
		/// <para>Note that <see cref="HtmlToHtml"/> is not an HTML sanitizer. For displaying untrusted HTML,
		/// use a dedicated HTML sanitizer library.</para>
		/// </note>
		/// </remarks>
		/// <value>The method for handling <c>&lt;noscript&gt;</c> elements.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid <see cref="HtmlNoScriptHandling"/> value.
		/// </exception>
		public HtmlNoScriptHandling NoScriptHandling {
			get { return noScriptHandling; }
			set {
				if (value < HtmlNoScriptHandling.Unwrap || value > HtmlNoScriptHandling.ScriptingDisabled)
					throw new ArgumentOutOfRangeException (nameof (value));

				noScriptHandling = value;
			}
		}

		/// <summary>
		/// Get or set whether the converter should only output an HTML fragment.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the converter should output an entire HTML document or just a fragment
		/// of the HTML body content.</para>
		/// <para>When <see langword="true" />, the <c>&lt;!DOCTYPE&gt;</c>, <c>&lt;html&gt;</c>, <c>&lt;head&gt;</c>
		/// (including its content) and <c>&lt;body&gt;</c> tags are removed without being passed to the
		/// <see cref="HtmlTagCallback"/>.</para>
		/// </remarks>
		/// <value><see langword="true" /> if the converter should only output an HTML fragment; otherwise, <see langword="false" />.</value>
		public bool OutputHtmlFragment {
			get; set;
		}

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

			ConvertHtml (reader, writer);

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

		static bool IsDocumentStructureTag (HtmlTagId id)
		{
			return id == HtmlTagId.Html || id == HtmlTagId.Head || id == HtmlTagId.Body;
		}

		// Note: This is also used by RtfToHtml to convert the HTML encapsulated within RTF (which is just as untrusted
		// as any other HTML), so that both converters share the same filtering logic.
		internal void ConvertHtml (TextReader reader, TextWriter writer)
		{
			using (var htmlWriter = new HtmlWriter (writer, true)) {
				var unwrapNoScript = noScriptHandling == HtmlNoScriptHandling.Unwrap;
				var fragment = OutputHtmlFragment;
				var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
				var stack = new HtmlTagContextStack<HtmlToHtmlTagContext> ();
				var tokenizer = new HtmlTokenizer (reader) {
					DecodeCharacterReferences = false,
					ScriptingEnabled = noScriptHandling == HtmlNoScriptHandling.ScriptingEnabled,
					IgnoreTruncatedTags = true,
					MaxElementDepth = maxElementDepth,
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
					case HtmlTokenKind.DocType:
						if (!fragment && !stack.SuppressContent)
							htmlWriter.WriteToken (token);
						break;
					case HtmlTokenKind.Tag:
						var tag = (HtmlTagToken) token;

						// Note: The content of <noscript> elements has been tokenized as markup (as if scripting is disabled), so
						// remove the <noscript> tags themselves so that renderers with scripting enabled interpret it the same way.
						if (unwrapNoScript && tag.Id == HtmlTagId.NoScript)
							break;

						if (!tag.IsEndTag) {
							if (fragment && IsDocumentStructureTag (tag.Id)) {
								// Drop the tag (and the content of <head>) without consulting the callback.
								if (!tag.IsEmptyElement) {
									ctx = new HtmlToHtmlTagContext (tag) {
										SuppressInnerContent = tag.Id == HtmlTagId.Head,
										DeleteEndTag = true,
										DeleteTag = true
									};

									stack.Push (ctx);
								}
							} else if (!tag.IsEmptyElement || tokenizer.TokenizerState != HtmlTokenizerState.Data) {
								// Note: A self-closing tag such as <style/> or <script/> still switches the tokenizer into a raw content
								// state (browsers ignore the self-closing flag on non-void HTML elements), so treat it as an open element
								// so that SuppressInnerContent applies to the raw content that follows.
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
							} else if (!stack.SuppressContent && !(fragment && IsDocumentStructureTag (tag.Id))) {
								ctx = new HtmlToHtmlTagContext (tag);
								callback (ctx, htmlWriter);
							}
						}
						break;
					}
				}

				htmlWriter.Flush ();
			}
		}
	}
}
