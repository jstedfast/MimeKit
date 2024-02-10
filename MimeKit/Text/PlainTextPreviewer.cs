//
// PlainTextPreviewer.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// A text previewer for plain text.
	/// </summary>
	/// <remarks>
	/// A text previewer for plain text.
	/// </remarks>
	public class PlainTextPreviewer : TextPreviewer
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="PlainTextPreviewer"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new previewer for plain text.
		/// </remarks>
		public PlainTextPreviewer ()
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
			get { return TextFormat.Plain; }
		}

		static bool IsWhiteSpace (char c)
		{
			return char.IsWhiteSpace (c) || (c >= 0x200B && c <= 0x200D);
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
		public override string GetPreviewText (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			if (text.Length == 0)
				return string.Empty;

			var preview = new char[Math.Min (MaximumPreviewLength, text.Length)];
			int previewLength = 0;
			var lwsp = true;
			int i;

			for (i = 0; i < text.Length && previewLength < preview.Length; i++) {
				if (IsWhiteSpace (text[i])) {
					if (!lwsp) {
						preview[previewLength++] = ' ';
						lwsp = true;
					}
				} else {
					preview[previewLength++] = text[i];
					lwsp = false;
				}
			}

			if (lwsp && previewLength > 0)
				previewLength--;

			if (i < text.Length)
				preview[previewLength - 1] = '\u2026';

			return new string (preview, 0, previewLength);
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
		public override string GetPreviewText (TextReader reader)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			var preview = new char[MaximumPreviewLength];
			var buffer = new char[4096];
			int previewLength = 0;
			var lwsp = true;
			int nread, i;

			while ((nread = reader.ReadBlock (buffer, 0, buffer.Length)) > 0) {
				for (i = 0; i < nread && previewLength < preview.Length; i++) {
					if (char.IsWhiteSpace (buffer[i])) {
						if (!lwsp) {
							preview[previewLength++] = ' ';
							lwsp = true;
						}
					} else {
						preview[previewLength++] = buffer[i];
						lwsp = false;
					}
				}

				if (i < nread) {
					preview[previewLength - 1] = '\u2026';
					lwsp = false;
					break;
				}
			}

			if (lwsp && previewLength > 0)
				previewLength--;

			return new string (preview, 0, previewLength);
		}
	}
}
