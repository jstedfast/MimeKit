//
// ITextPart.cs
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
using System.Text;

using MimeKit.Text;

namespace MimeKit {
	/// <summary>
	/// An interface for a textual MIME part.
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
	public interface ITextPart : IMimePart
	{
		/// <summary>
		/// Get the text format of the content.
		/// </summary>
		/// <remarks>
		/// Gets the text format of the content.
		/// </remarks>
		/// <value>The text format of the content.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		TextFormat Format {
			get;
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool IsEnriched {
			get;
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool IsFlowed {
			get;
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool IsHtml {
			get;
		}

		/// <summary>
		/// Get whether or not this text part contains plain text.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/plain</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is html; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool IsPlain {
			get;
		}

		/// <summary>
		/// Get whether or not this text part contains RTF.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the text part's Content-Type is <c>text/rtf</c>.
		/// </remarks>
		/// <value><c>true</c> if the text is RTF; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool IsRichText {
			get;
		}

		/// <summary>
		/// Get the decoded text content.
		/// </summary>
		/// <remarks>
		/// <para>If the charset parameter on the <see cref="IMimeEntity.ContentType"/>
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		string Text {
			get; set;
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		bool TryDetectEncoding (out Encoding encoding, out TextEncodingConfidence confidence);

		/// <summary>
		/// Get the decoded text and the encoding used to convert it into unicode.
		/// </summary>
		/// <remarks>
		/// <para>If the charset parameter on the <see cref="IMimeEntity.ContentType"/>
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		string GetText (out Encoding encoding);

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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		string GetText (Encoding encoding);

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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		string GetText (string charset);

		/// <summary>
		/// Set the text content and the charset parameter in the Content-Type header.
		/// </summary>
		/// <remarks>
		/// This method is similar to setting the <see cref="ITextPart.Text"/> property,
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		void SetText (Encoding encoding, string text);

		/// <summary>
		/// Set the text content and the charset parameter in the Content-Type header.
		/// </summary>
		/// <remarks>
		/// This method is similar to setting the <see cref="ITextPart.Text"/> property,
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
		/// The <see cref="ITextPart"/> has been disposed.
		/// </exception>
		void SetText (string charset, string text);
	}
}
