//
// TextPart.cs
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
using System.Text;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Text;
using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace MimeKit {
	/// <summary>
	/// A textual MIME part.
	/// </summary>
	/// <remarks>
	/// <para>Unless overridden, all textual parts parsed by the <see cref="MimeParser"/>,
	/// such as text/plain or text/html, will be represented by a <see cref="TextPart"/>.</para>
	/// <para>For more information about text media types, see section 4.1 of
	/// <a href="https://tools.ietf.org/html/rfc2046#section-4.1">rfc2046</a>.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class TextPart : MimePart, ITextPart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TextPart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public TextPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TextPart"/>
		/// class with the specified text subtype.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="TextPart"/> with the specified subtype.</para>
		/// <note type="note"><para>Typically the <paramref name="subtype"/> should either be
		/// <c>"plain"</c> for plain text content or <c>"html"</c> for HTML content.</para>
		/// <para>For more options, check the MIME-type registry at
		/// <a href="http://www.iana.org/assignments/media-types/media-types.xhtml#text">
		/// http://www.iana.org/assignments/media-types/media-types.xhtml#text
		/// </a></para></note>
		/// </remarks>
		/// <param name="subtype">The media subtype.</param>
		/// <param name="args">An array of initialization parameters: headers, charset encoding and text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="subtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="System.Text.Encoding"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains more than one <see cref="System.String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public TextPart (string subtype, params object[] args) : this (subtype)
		{
			if (args is null)
				throw new ArgumentNullException (nameof (args));

			// Default to UTF8 if not given.
			Encoding encoding = null;
			string text = null;

			foreach (object obj in args) {
				if (obj is null || TryInit (obj))
					continue;

				if (obj is Encoding enc) {
					if (encoding != null)
						throw new ArgumentException ("An encoding should not be specified more than once.");

					encoding = enc;
					continue;
				}

				if (obj is string str) {
					if (text != null)
						throw new ArgumentException ("The text should not be specified more than once.");

					text = str;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (text != null) {
				encoding ??= Encoding.UTF8;
				SetText (encoding, text);
			}
		}

		internal TextPart (ContentType contentType) : base (contentType)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TextPart"/>
		/// class with the specified text subtype.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="TextPart"/> with the specified subtype.</para>
		/// <note type="note"><para>Typically the <paramref name="subtype"/> should either be
		/// <c>"plain"</c> for plain text content or <c>"html"</c> for HTML content.</para>
		/// <para>For more options, check the MIME-type registry at
		/// <a href="http://www.iana.org/assignments/media-types/media-types.xhtml#text">
		/// http://www.iana.org/assignments/media-types/media-types.xhtml#text
		/// </a></para></note>
		/// </remarks>
		/// <param name="subtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="subtype"/> is <c>null</c>.
		/// </exception>
		public TextPart (string subtype) : base ("text", subtype)
		{
		}

		static string GetMediaSubtype (TextFormat format)
		{
			switch (format) {
			case TextFormat.CompressedRichText:
			case TextFormat.RichText:
				return "rtf";
			case TextFormat.Enriched:
				return "enriched";
			case TextFormat.Flowed:
			case TextFormat.Plain:
				return "plain";
			case TextFormat.Html:
				return "html";
			default:
				throw new ArgumentOutOfRangeException (nameof (format));
			}
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TextPart"/>
		/// class with the specified text format.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TextPart"/> with the specified text format.
		/// </remarks>
		/// <param name="format">The text format.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="format"/> is out of range.
		/// </exception>
		public TextPart (TextFormat format) : base ("text", GetMediaSubtype (format))
		{
			if (format == TextFormat.Flowed)
				ContentType.Format = "flowed";
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TextPart"/>
		/// class with a Content-Type of text/plain.
		/// </summary>
		/// <remarks>
		/// Creates a default <see cref="TextPart"/> with a mime-type of <c>text/plain</c>.
		/// </remarks>
		public TextPart () : base ("text", "plain")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (TextPart));
		}

		/// <summary>
		/// Get the text format of the content.
		/// </summary>
		/// <remarks>
		/// Gets the text format of the content.
		/// </remarks>
		/// <value>The text format of the content.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public TextFormat Format {
			get {
				CheckDisposed ();

				if (ContentType.MediaType.Equals ("text", StringComparison.OrdinalIgnoreCase)) {
					if (ContentType.MediaSubtype.Equals ("plain")) {
						if (ContentType.Parameters.TryGetValue ("format", out string format)) {
							format = format.Trim ();

							if (format.Equals ("flowed", StringComparison.OrdinalIgnoreCase))
								return TextFormat.Flowed;
						}
					} else if (ContentType.MediaSubtype.Equals ("html", StringComparison.OrdinalIgnoreCase)) {
						return TextFormat.Html;
					} else if (ContentType.MediaSubtype.Equals ("rtf", StringComparison.OrdinalIgnoreCase)) {
						return TextFormat.RichText;
					} else if (ContentType.MediaSubtype.Equals ("enriched", StringComparison.OrdinalIgnoreCase)) {
						return TextFormat.Enriched;
					} else if (ContentType.MediaSubtype.Equals ("richtext", StringComparison.OrdinalIgnoreCase)) {
						return TextFormat.Enriched;
					}
				} else if (ContentType.IsMimeType ("application", "rtf")) {
					return TextFormat.RichText;
				}

				return TextFormat.Plain;
			}
		}

		/// <summary>
		/// Get whether or not this text part contains enriched text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/enriched</c> or its
		/// predecessor, <c>text/richtext</c> (not to be confused with <c>text/rtf</c>).
		/// </remarks>
		/// <value><c>true</c> if the text is enriched; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool IsEnriched {
			get {
				CheckDisposed ();

				return ContentType.IsMimeType ("text", "enriched") || ContentType.IsMimeType ("text", "richtext");
			}
		}

		/// <summary>
		/// Get whether or not this text part contains flowed text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/plain</c> and
		/// has a format parameter with a value of <c>flowed</c>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the text is flowed; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool IsFlowed {
			get {
				if (!IsPlain || !ContentType.Parameters.TryGetValue ("format", out string format))
					return false;

				format = format.Trim ();

				return format.Equals ("flowed", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Get whether or not this text part contains HTML.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/html</c>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the text is html; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool IsHtml {
			get {
				CheckDisposed ();

				return ContentType.IsMimeType ("text", "html");
			}
		}

		/// <summary>
		/// Get whether or not this text part contains plain text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/plain</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is html; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool IsPlain {
			get {
				CheckDisposed ();
				
				return ContentType.IsMimeType ("text", "plain");
			}
		}

		/// <summary>
		/// Get whether or not this text part contains RTF.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/rtf</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is RTF; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool IsRichText {
			get {
				CheckDisposed ();

				return ContentType.IsMimeType ("text", "rtf") || ContentType.IsMimeType ("application", "rtf");
			}
		}

		/// <summary>
		/// Get the decoded text content.
		/// </summary>
		/// <remarks>
		/// <para>If the charset parameter on the <see cref="MimeEntity.ContentType"/>
		/// is set, it will be used in order to convert the raw content into unicode.
		/// If that fails or if the charset parameter is not set, the first 2 bytes of
		/// the content will be checked for a unicode BOM. If a BOM exists, then that
		/// will be used for conversion. If no BOM is found, then UTF-8 is attempted.
		/// If conversion fails, then iso-8859-1 will be used as the final fallback.</para>
		/// <para>For more control, use <see cref="GetText(Encoding)"/>
		/// or <see cref="GetText(String)"/>.</para>
		/// </remarks>
		/// <value>The decocded text.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public string Text {
			get {
				return GetText (out _);
			}
			set {
				SetText (Encoding.UTF8, value);
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="TextPart"/> nodes
		/// calls <see cref="MimeVisitor.VisitTextPart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitTextPart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitTextPart (this);
		}

		/// <summary>
		/// Determine whether or not the text is in the specified format.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the text is in the specified format.
		/// </remarks>
		/// <returns><c>true</c> if the text is in the specified format; otherwise, <c>false</c>.</returns>
		/// <param name="format">The text format.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		internal bool IsFormat (TextFormat format)
		{
			switch (format) {
			case TextFormat.Plain:    return IsPlain;
			case TextFormat.Flowed:   return IsFlowed;
			case TextFormat.Html:     return IsHtml;
			case TextFormat.Enriched: return IsEnriched;
			case TextFormat.RichText: return IsRichText;
			default: return false;
			}
		}

		enum HtmlTagState
		{
			None,
			Html,
			Head,
			Stop
		}

		static bool TryDetectHtmlEncoding (TextReader reader, out Encoding encoding, out TextEncodingConfidence confidence)
		{
			var tokenizer = new HtmlTokenizer (reader);
			var state = HtmlTagState.None;

			// https://html.spec.whatwg.org/multipage/parsing.html#prescan-a-byte-stream-to-determine-its-encoding
			while (tokenizer.ReadNextToken (out var token)) {
				if (token.Kind != HtmlTokenKind.Tag)
					continue;

				var tag = (HtmlTagToken) token;

				switch (tag.Id) {
				case HtmlTagId.Html:
					if (state == HtmlTagState.None) {
						if (tag.IsEndTag || tag.IsEmptyElement)
							state = HtmlTagState.Stop;
						else
							state = HtmlTagState.Html;
					}
					break;
				case HtmlTagId.Head:
					if (state == HtmlTagState.Html) {
						if (tag.IsEndTag || tag.IsEmptyElement)
							state = HtmlTagState.Stop;
						else
							state = HtmlTagState.Head;
					}
					break;
				case HtmlTagId.Meta:
					if (state == HtmlTagState.Head && !tag.IsEndTag) {
						var attributes = new HashSet<HtmlAttributeId> ();
						bool? need_pragma = null;
						var got_pragma = false;
						string charset = null;

						foreach (var attribute in tag.Attributes) {
							if (attribute.Value is null || !attributes.Add (attribute.Id))
								continue;

							switch (attribute.Id) {
							case HtmlAttributeId.HttpEquiv:
								if (attribute.Value.Equals ("Content-Type", StringComparison.OrdinalIgnoreCase))
									got_pragma = true;
								break;
							case HtmlAttributeId.Content:
								if (charset is null && ContentType.TryParse (attribute.Value, out var contentType) && !string.IsNullOrEmpty (contentType.Charset)) {
									charset = contentType.Charset.Trim ();
									need_pragma = true;
								}
								break;
							case HtmlAttributeId.Charset:
								if (!string.IsNullOrEmpty (attribute.Value)) {
									charset = attribute.Value.Trim ();
									need_pragma = false;
								}
								break;
							}
						}

						if (need_pragma.HasValue && charset != null && (!need_pragma.Value || got_pragma)) {
							if (charset.Equals ("x-user-defined", StringComparison.OrdinalIgnoreCase))
								charset = "windows-1252";

							try {
								encoding = CharsetUtils.GetEncoding (charset);
								if (encoding.CodePage == Encoding.Unicode.CodePage || encoding.CodePage == Encoding.BigEndianUnicode.CodePage)
									encoding = CharsetUtils.UTF8;

								confidence = TextEncodingConfidence.Tentative;
								return true;
							} catch {
								encoding = null;
							}
						}
					}
					break;
				case HtmlTagId.Body:
					state = HtmlTagState.Stop;
					break;
				}

				if (state == HtmlTagState.Stop)
					break;
			}

			confidence = TextEncodingConfidence.Undefined;
			encoding = null;

			return false;
		}

		bool TryDetectHtmlEncoding (out Encoding encoding, out TextEncodingConfidence confidence)
		{
			using (var content = Content.Open ()) {
				// limit processing to first 1024 bytes as per https://html.spec.whatwg.org/multipage/parsing.html#the-input-byte-stream
				using (var bounded = new BoundStream (content, 0, 1024, true)) {
					using (var reader = new StreamReader (bounded, CharsetUtils.Latin1, false, 1024, true))
						return TryDetectHtmlEncoding (reader, out encoding, out confidence);
				}
			}
		}

		/// <summary>
		/// Try to detect the encoding of the text content.
		/// </summary>
		/// <remarks>
		/// <para>Attempts to detect the encoding of the text content.</para>
		/// <para>If no content is defined, then <paramref name="encoding"/> is set to ASCII and <paramref name="confidence"/>
		/// is set to <see cref="TextEncodingConfidence.Irrelevant"/>.</para>
		/// <para>If a charset is specified on the <c>Content-Type</c> header and it is a supported encoding, then
		/// <paramref name="encoding"/> is set to the encoding for the specified charset and <paramref name="confidence"/>
		/// is set to <see cref="TextEncodingConfidence.Certain"/>.</para>
		/// <para>If a Byte-Order-Mark (BOM) is found, then <paramref name="encoding"/> is set to the corresponding unicode
		/// encoding and <paramref name="confidence"/> is set to <see cref="TextEncodingConfidence.Certain"/>.</para>
		/// <para>If the content is in HTML format, then the first 1024 bytes are processed for <c>&lt;meta&gt;</c> tags
		/// containing charset information. If charset information is found and the charset is a supported encoding, then
		/// <paramref name="encoding"/> is set to the encoding for the specified charset and <paramref name="confidence"/> is
		/// set to <see cref="TextEncodingConfidence.Tentative"/>.</para>
		/// </remarks>
		/// <returns><c>true</c> if an encoding was detected; otherwise, <c>false</c>.</returns>
		/// <param name="encoding">The detected encoding; otherwise, <c>null</c>.</param>
		/// <param name="confidence">The confidence in the detected encoding being correct.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public bool TryDetectEncoding (out Encoding encoding, out TextEncodingConfidence confidence)
		{
			CheckDisposed ();

			if (Content is null) {
				confidence = TextEncodingConfidence.Irrelevant;
				encoding = Encoding.ASCII;
				return true;
			}

			// If the transport layer specifies a character encoding, and it is supported, return that encoding with the confidence certain.
			encoding = ContentType.CharsetEncoding;
			if (encoding != null) {
				confidence = TextEncodingConfidence.Certain;
				return true;
			}

			using (var content = Content.Open ()) {
				if (CharsetUtils.TryGetBomEncoding (content, out encoding)) {
					confidence = TextEncodingConfidence.Certain;
					return true;
				}
			}

			if (IsHtml)
				return TryDetectHtmlEncoding (out encoding, out confidence);

			confidence = TextEncodingConfidence.Undefined;
			encoding = null;

			return false;
		}

		/// <summary>
		/// Get the decoded text and the encoding used to convert it into unicode.
		/// </summary>
		/// <remarks>
		/// <para>If the charset parameter on the <see cref="MimeEntity.ContentType"/>
		/// is set, it will be used in order to convert the raw content into unicode.
		/// If that fails or if the charset parameter is not set, the first 3 bytes of
		/// the content will be checked for a unicode BOM. If a BOM exists, then that
		/// will be used for conversion. If no BOM is found, then UTF-8 is attempted.
		/// If conversion fails, then iso-8859-1 will be used as the final fallback.</para>
		/// <para>For more control, use <see cref="GetText(Encoding)"/>
		/// or <see cref="GetText(String)"/>.</para>
		/// </remarks>
		/// <param name="encoding">The encoding used to convert the text into unicode.</param>
		/// <returns>The decoded text.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public string GetText (out Encoding encoding)
		{
			CheckDisposed ();

			if (!TryDetectEncoding (out encoding, out _))
				encoding = CharsetUtils.UTF8;

			try {
				return GetText (encoding);
			} catch (DecoderFallbackException) {
				// fall back to iso-8859-1
				encoding = CharsetUtils.Latin1;
			}

			return GetText (encoding);
		}

		/// <summary>
		/// Get the decoded text content using the provided charset encoding to
		/// override the charset specified in the Content-Type parameters.
		/// </summary>
		/// <remarks>
		/// Uses the provided charset encoding to convert the raw text content
		/// into a unicode string, overriding any charset specified in the
		/// Content-Type header.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="encoding">The charset encoding to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoding"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public string GetText (Encoding encoding)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			CheckDisposed ();

			if (Content is null)
				return string.Empty;

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new CharsetFilter (encoding, CharsetUtils.UTF8));
					filtered.Add (FormatOptions.Default.CreateNewLineFilter ());
					Content.DecodeTo (filtered);
					filtered.Flush ();
				}

				var buffer = memory.GetBuffer ();

				return CharsetUtils.UTF8.GetString (buffer, 0, (int) memory.Length);
			}
		}

		/// <summary>
		/// Get the decoded text content using the provided charset to override
		/// the charset specified in the Content-Type parameters.
		/// </summary>
		/// <remarks>
		/// Uses the provided charset encoding to convert the raw text content
		/// into a unicode string, overriding any charset specified in the
		/// Content-Type header.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="charset">The charset encoding to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="charset"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="charset"/> is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public string GetText (string charset)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			return GetText (CharsetUtils.GetEncoding (charset));
		}

		/// <summary>
		/// Set the text content and the charset parameter in the Content-Type header.
		/// </summary>
		/// <remarks>
		/// This method is similar to setting the <see cref="TextPart.Text"/> property,
		/// but allows specifying a charset encoding to use. Also updates the
		/// <see cref="ContentType.Charset"/> property.
		/// </remarks>
		/// <param name="encoding">The charset encoding.</param>
		/// <param name="text">The text content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public void SetText (Encoding encoding, string text)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			CheckDisposed ();

			var content = new MemoryStream (encoding.GetBytes (text));
			ContentType.CharsetEncoding = encoding;
			Content = new MimeContent (content);
		}

		/// <summary>
		/// Set the text content and the charset parameter in the Content-Type header.
		/// </summary>
		/// <remarks>
		/// This method is similar to setting the <see cref="TextPart.Text"/> property,
		/// but allows specifying a charset encoding to use. Also updates the
		/// <see cref="ContentType.Charset"/> property.
		/// </remarks>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="charset"/> is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TextPart"/> has been disposed.
		/// </exception>
		public void SetText (string charset, string text)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			SetText (CharsetUtils.GetEncoding (charset), text);
		}
	}
}
