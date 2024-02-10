//
// PreviewGenerator.cs
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

using MimeKit.Utils;

namespace MimeKit.Text {
	/// <summary>
	/// An abstract class for generating a text preview of a message.
	/// </summary>
	/// <remarks>
	/// An abstract class for generating a text preview of a message.
	/// </remarks>
	public abstract class TextPreviewer
	{
		int maximumPreviewLength;

		/// <summary>
		/// Initialize a new instance of the <see cref="TextPreviewer"/> class.
		/// </summary>
		/// <remarks>
		/// Initialize a new instance of the <see cref="TextPreviewer"/> class.
		/// </remarks>
		protected TextPreviewer ()
		{
			maximumPreviewLength = 230;
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public abstract TextFormat InputFormat {
			get;
		}

		/// <summary>
		/// Get or set the maximum text preview length.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum text preview length.</para>
		/// <para>The default value is <c>230</c> which is what the GMail web API seems to use.</para>
		/// </remarks>
		/// <value>The maximum text preview length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value">is less than <c>1</c> or greater than <c>1024</c>.</paramref>
		/// </exception>
		public int MaximumPreviewLength {
			get { return maximumPreviewLength; }
			set {
				if (value < 1 || value > 1024)
					throw new ArgumentOutOfRangeException (nameof (value));

				maximumPreviewLength = value;
			}
		}

		static TextPreviewer Create (TextFormat format)
		{
			switch (format) {
			case TextFormat.Html: return new HtmlTextPreviewer ();
			default: return new PlainTextPreviewer ();
			}
		}

		/// <summary>
		/// Get a text preview of the text part.
		/// </summary>
		/// <remarks>
		/// Gets a text preview of the text part.
		/// </remarks>
		/// <param name="body">The text part.</param>
		/// <returns>A string representing a shortened preview of the original text.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="body"/> is <c>null</c>.
		/// </exception>
		public static string GetPreviewText (TextPart body)
		{
			if (body is null)
				throw new ArgumentNullException (nameof (body));

			if (body.Content is null)
				return string.Empty;

			var encoding = body.ContentType.CharsetEncoding;
			var buffer = new byte[16 * 1024];
			int nread = 0;

			using (var content = body.Content.Open ()) {
				int n;

				do {
					if ((n = content.Read (buffer, nread, buffer.Length - nread)) <= 0)
						break;

					nread += n;
				} while (nread < buffer.Length);
			}

			if (encoding is null) {
				if (!CharsetUtils.TryGetBomEncoding (buffer, nread, out encoding))
					encoding = CharsetUtils.UTF8;
			}

			using (var stream = new MemoryStream (buffer, 0, nread, false)) {
				var previewer = Create (body.Format);

				try {
					return previewer.GetPreviewText (stream, encoding);
				} catch (DecoderFallbackException) {
					stream.Position = 0;

					return previewer.GetPreviewText (stream, CharsetUtils.Latin1);
				}
			}
		}

		/// <summary>
		/// Get a text preview of a string of text.
		/// </summary>
		/// <remarks>
		/// Gets a text preview of a string of text.
		/// </remarks>
		/// <param name="text">The original text.</param>
		/// <returns>A string representing a shortened preview of the original text.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public virtual string GetPreviewText (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			using (var reader = new StringReader (text))
				return GetPreviewText (reader);
		}

		/// <summary>
		/// Get a text preview of a stream of text in the specified charset.
		/// </summary>
		/// <remarks>
		/// Get a text preview of a stream of text in the specified charset.
		/// </remarks>
		/// <param name="stream">The original text stream.</param>
		/// <param name="charset">The charset encoding of the stream.</param>
		/// <returns>A string representing a shortened preview of the original text.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// </exception>
		public virtual string GetPreviewText (Stream stream, string charset)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			Encoding encoding;

			try {
				encoding = CharsetUtils.GetEncoding (charset);
			} catch (NotSupportedException) {
				encoding = CharsetUtils.UTF8;
			}

			return GetPreviewText (stream, encoding);
		}

		/// <summary>
		/// Get a text preview of a stream of text in the specified encoding.
		/// </summary>
		/// <remarks>
		/// Get a text preview of a stream of text in the specified encoding.
		/// </remarks>
		/// <param name="stream">The original text stream.</param>
		/// <param name="encoding">The encoding of the stream.</param>
		/// <returns>A string representing a shortened preview of the original text.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// </exception>
		public virtual string GetPreviewText (Stream stream, Encoding encoding)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			using (var reader = new StreamReader (stream, encoding, false, 4096, true))
				return GetPreviewText (reader);
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
		/// <paramref name="reader"/> is <c>null</c>.
		/// </exception>
		public abstract string GetPreviewText (TextReader reader);
	}
}
