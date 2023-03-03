// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
