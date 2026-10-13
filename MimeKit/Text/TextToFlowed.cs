//
// TextToFlowed.cs
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
using System.Text;

using MimeKit.Utils;

namespace MimeKit.Text {
	/// <summary>
	/// A text to flowed text converter.
	/// </summary>
	/// <remarks>
	/// <para>Wraps text to conform with the flowed text format described in rfc3676.</para>
	/// <para>The Content-Type header for the wrapped output text should be set to
	/// <c>text/plain; format=flowed; delsp=yes</c>.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TextConverterExamples.cs" region="TextToFlowed"/>
	/// </example>
	public class TextToFlowed : TextConverter
	{
		const int MaxHardLineLength = 998;
		const int MaxLineLength = 78;

		/// <summary>
		/// Initialize a new instance of the <see cref="TextToFlowed"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new text to flowed text converter.
		/// </remarks>
		public TextToFlowed ()
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

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public override TextFormat OutputFormat {
			get { return TextFormat.Flowed; }
		}

		static ReadOnlySpan<char> Unquote (ReadOnlySpan<char> line, out int quoteDepth)
		{
			int index = 0;

			quoteDepth = 0;

			if (line.Length == 0 || line[0] != '>')
				return line;

			do {
				quoteDepth++;
				index++;

				if (index < line.Length && line[index] == ' ')
					index++;
			} while (index < line.Length && line[index] == '>');

			return line.Slice (index);
		}

		static string GetFlowedLine (StringBuilder flowed, ReadOnlySpan<char> line, ref int index, int quoteDepth)
		{
			flowed.Length = 0;

			if (quoteDepth > 0)
				flowed.Append ('>', quoteDepth);

			// Space-stuff lines which start with a space, "From ", or ">" (rfc3676, section 4.4). Note: Since the
			// quote markers are removed from the line, a '>' can only appear at the start of a soft-broken line.
			if (quoteDepth > 0 || (line.Length > index && (line[index] == ' ' || line[index] == '>')) || line.Slice (index).StartsWith ("From ".AsSpan (), StringComparison.Ordinal))
				flowed.Append (' ');

			int prefixLength = flowed.Length;

			// Note: When the quote depth is very deep, the quote markers alone may leave little or no room for text within
			// MaxLineLength (or even MaxHardLineLength) characters. Always leave at least as much room for text as the
			// quote markers take up, otherwise every line would contain a single word and the size of the output would
			// grow with the quote depth multiplied by the number of words.
			int softLineLength = Math.Max (MaxLineLength, prefixLength * 2);
			int hardLineLength = Math.Max (MaxHardLineLength, prefixLength * 2);

			if (flowed.Length + (line.Length - index) <= softLineLength) {
				flowed.Append (line.Slice (index));
				index = line.Length;

				return flowed.ToString ();
			}

			do {
				int nextSpace = line.Slice (index).IndexOf (' ');
				int wordEnd = nextSpace == -1 ? line.Length : nextSpace + index;
				int softBreak = nextSpace == -1 ? 0 : 2; // 2 = space + soft-break space
				int wordLength = wordEnd - index;

				if (flowed.Length + wordLength + softBreak <= softLineLength) {
					// The entire word will fit on the remainder of the line.
					flowed.Append (line.Slice (index, wordLength));
					index = wordEnd;
				} else if (flowed.Length > prefixLength && prefixLength + wordLength + softBreak <= softLineLength) {
					// Only part of the word will fit on the remainder of this line, but it will easily fit
					// on its own line. Insert a soft-break so that we don't break apart this word.
					break;
				} else if (flowed.Length + wordLength + softBreak <= hardLineLength) {
					// The word will not fit within softLineLength characters even on a line of its own, so send the word as is and exceed MaxLineLength
					// rather than inserting a soft-break into the middle of the word (rfc3676, section 4.2).
					flowed.Append (line.Slice (index, wordLength));
					index = wordEnd;
				} else if (flowed.Length > prefixLength && prefixLength + wordLength + softBreak <= hardLineLength) {
					// The word will fit within hardLineLength characters on its own line.
					break;
				} else {
					// The word is too long to fit within hardLineLength characters even on a line of its own,
					// so there is no choice but to break the word apart. Note: flowed.Length is always less than
					// softLineLength - 1 here and hardLineLength is at least twice prefixLength, so wordLength > 0.
					wordLength = hardLineLength - (flowed.Length + 1);

					flowed.Append (line.Slice (index, wordLength));
					index += wordLength;
					break;
				}

				int maxLineLength = flowed.Length < softLineLength ? softLineLength : hardLineLength;

				while (flowed.Length + 1 < maxLineLength && index < line.Length && line[index] == ' ')
					flowed.Append (line[index++]);
			} while (index < line.Length && flowed.Length + 1 < softLineLength);

			if (index < line.Length)
				flowed.Append (' ');

			return flowed.ToString ();
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
			StringBuilder flowed;
			string? line;

			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			flowed = new StringBuilder (MaxLineLength);

			while ((line = reader.ReadLine ()) != null) {
				// Trim spaces before user-inserted hard line breaks.
				var unquoted = Unquote (line.AsSpan ().TrimEnd (' '), out var quoteDepth);

				// Ensure all lines (fixed and flowed) are 78 characters or fewer in
				// length, counting any trailing space as well as a space added as
				// stuffing, but not counting the CRLF, unless a word by itself
				// exceeds 78 characters.
				int index = 0;

				do {
					var flowedLine = GetFlowedLine (flowed, unquoted, ref index, quoteDepth);
					writer.WriteLine (flowedLine);
				} while (index < unquoted.Length);
			}

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
