//
// TextPart.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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

#if PORTABLE
using EncoderReplacementFallback = Portable.Text.EncoderReplacementFallback;
using DecoderReplacementFallback = Portable.Text.DecoderReplacementFallback;
using EncoderExceptionFallback = Portable.Text.EncoderExceptionFallback;
using DecoderExceptionFallback = Portable.Text.DecoderExceptionFallback;
using EncoderFallbackException = Portable.Text.EncoderFallbackException;
using DecoderFallbackException = Portable.Text.DecoderFallbackException;
using DecoderFallbackBuffer = Portable.Text.DecoderFallbackBuffer;
using DecoderFallback = Portable.Text.DecoderFallback;
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

using MimeKit.Text;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A Textual MIME part.
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
	public class TextPart : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public TextPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
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
			if (args == null)
				throw new ArgumentNullException (nameof (args));

			// Default to UTF8 if not given.
			Encoding encoding = null;
			string text = null;

			foreach (object obj in args) {
				if (obj == null || TryInit (obj))
					continue;

				var enc = obj as Encoding;
				if (enc != null) {
					if (encoding != null)
						throw new ArgumentException ("An encoding should not be specified more than once.");

					encoding = enc;
					continue;
				}

				var str = obj as string;
				if (str != null) {
					if (text != null)
						throw new ArgumentException ("The text should not be specified more than once.");

					text = str;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (text != null) {
				encoding = encoding ?? Encoding.UTF8;
				SetText (encoding, text);
			}
		}

		internal TextPart (ContentType contentType) : base (contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
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
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
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
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
		/// class with a Content-Type of text/plain.
		/// </summary>
		/// <remarks>
		/// Creates a default <see cref="TextPart"/> with a mime-type of <c>text/plain</c>.
		/// </remarks>
		public TextPart () : base ("text", "plain")
		{
		}

		/// <summary>
		/// Gets whether or not this text part contains enriched text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/enriched</c> or its
		/// predecessor, <c>text/richtext</c> (not to be confused with <c>text/rtf</c>).
		/// </remarks>
		/// <value><c>true</c> if the text is enriched; otherwise, <c>false</c>.</value>
		bool IsEnriched {
			get { return ContentType.IsMimeType ("text", "enriched") || ContentType.IsMimeType ("text", "richtext"); }
		}

		/// <summary>
		/// Gets whether or not this text part contains flowed text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/plain</c> and
		/// has a format parameter with a value of <c>flowed</c>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the text is flowed; otherwise, <c>false</c>.</value>
		public bool IsFlowed {
			get {
				string format;

				if (!IsPlain || !ContentType.Parameters.TryGetValue ("format", out format))
					return false;

				format = format.Trim ();

				return format.ToLowerInvariant () == "flowed";
			}
		}

		/// <summary>
		/// Gets whether or not this text part contains HTML.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/html</c>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the text is html; otherwise, <c>false</c>.</value>
		public bool IsHtml {
			get { return ContentType.IsMimeType ("text", "html"); }
		}

		/// <summary>
		/// Gets whether or not this text part contains plain text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/plain</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is html; otherwise, <c>false</c>.</value>
		public bool IsPlain {
			get { return ContentType.IsMimeType ("text", "plain"); }
		}

		/// <summary>
		/// Gets whether or not this text part contains RTF.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/rtf</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is RTF; otherwise, <c>false</c>.</value>
		public bool IsRichText {
			get { return ContentType.IsMimeType ("text", "rtf") || ContentType.IsMimeType ("application", "rtf"); }
		}

		/// <summary>
		/// Gets the decoded text content.
		/// </summary>
		/// <remarks>
		/// <para>If the charset parameter on the <see cref="MimeEntity.ContentType"/>
		/// is set, it will be used in order to convert the raw content into unicode.
		/// If that fails or if the charset parameter is not set, iso-8859-1 will be
		/// used instead.</para>
		/// <para>For more control, use <see cref="GetText(Encoding)"/>
		/// or <see cref="GetText(String)"/>.</para>
		/// </remarks>
		/// <value>The text.</value>
		public string Text {
			get {
				if (Content == null)
					return string.Empty;

				var charset = ContentType.Parameters["charset"];

				using (var memory = new MemoryStream ()) {
					Content.DecodeTo (memory);

#if !PORTABLE && !NETSTANDARD
					var content = memory.GetBuffer ();
#else
					var content = memory.ToArray ();
#endif
					Encoding encoding = null;

					if (charset != null) {
						try {
							encoding = CharsetUtils.GetEncoding (charset);
						} catch (NotSupportedException) {
						}
					}

					if (encoding == null) {
						try {
							return CharsetUtils.UTF8.GetString (content, 0, (int) memory.Length);
						} catch (DecoderFallbackException) {
							// fall back to iso-8859-1
							encoding = CharsetUtils.Latin1;
						}
					}

					return encoding.GetString (content, 0, (int) memory.Length);
				}
			}
			set {
				SetText (Encoding.UTF8, value);
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.TextPart"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitTextPart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitTextPart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitTextPart (this);
		}

		/// <summary>
		/// Determines whether or not the text is in the specified format.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the text is in the specified format.
		/// </remarks>
		/// <returns><c>true</c> if the text is in the specified format; otherwise, <c>false</c>.</returns>
		/// <param name="format">The text format.</param>
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

		/// <summary>
		/// Gets the decoded text content using the provided charset encoding to
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
		public string GetText (Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (Content == null)
				return string.Empty;

			using (var memory = new MemoryStream ()) {
				Content.DecodeTo (memory);

#if !PORTABLE && !NETSTANDARD
				var buffer = memory.GetBuffer ();
#else
				var buffer = memory.ToArray ();
#endif

				return encoding.GetString (buffer, 0, (int) memory.Length);
			}
		}

		/// <summary>
		/// Gets the decoded text content using the provided charset to override
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
		public string GetText (string charset)
		{
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			return GetText (CharsetUtils.GetEncoding (charset));
		}

		/// <summary>
		/// Sets the text content and the charset parameter in the Content-Type header.
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
		public void SetText (Encoding encoding, string text)
		{
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (text == null)
				throw new ArgumentNullException (nameof (text));

			ContentType.Parameters["charset"] = CharsetUtils.GetMimeCharset (encoding);
			var content = new MemoryStream (encoding.GetBytes (text));
			Content = new MimeContent (content);
		}

		/// <summary>
		/// Sets the text content and the charset parameter in the Content-Type header.
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
		public void SetText (string charset, string text)
		{
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			if (text == null)
				throw new ArgumentNullException (nameof (text));

			SetText (CharsetUtils.GetEncoding (charset), text);
		}
	}
}
