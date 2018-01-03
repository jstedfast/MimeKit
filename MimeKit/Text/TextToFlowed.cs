//
// TextToFlowed.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// A text to flowed text converter.
	/// </summary>
	/// <remarks>
	/// <para>Wraps text to conform with the flowed text format described in rfc3676.</para>
	/// <para>The Content-Type header for the wrapped output text should be set to
	/// <c>text/plain; format=flowed; delsp=yes</c>.</para>
	/// </remarks>
	public class TextToFlowed : TextConverter
	{
		const int OptimalLineLength = 66;
		const int MaxLineLength = 78;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.TextToFlowed"/> class.
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

		/// <summary>
		/// Get or set the text that will be appended to the end of the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the text that will be appended to the end of the output.</para>
		/// <para>The footer must be set before conversion begins.</para>
		/// </remarks>
		/// <value>The footer.</value>
		public string Footer {
			get; set;
		}

		/// <summary>
		/// Get or set text that will be prepended to the beginning of the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the text that will be prepended to the beginning of the output.</para>
		/// <para>The header must be set before conversion begins.</para>
		/// </remarks>
		/// <value>The header.</value>
		public string Header {
			get; set;
		}

		static string Unquote (string line, out int quoteDepth)
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

			return line.Substring (index);
		}

		static bool StartsWith (string text, int startIndex, string value)
		{
			if (startIndex + value.Length > text.Length)
				return false;

			for (int i = 0; i < value.Length; i++) {
				if (text[startIndex + i] != value[i])
					return false;
			}

			return true;
		}

		static string GetFlowedLine (StringBuilder flowed, string line, ref int index, int quoteDepth)
		{
			flowed.Length = 0;

			if (quoteDepth > 0)
				flowed.Append ('>', quoteDepth);

			// Space-stuffed lines which start with a space, "From ", or ">".
			if (quoteDepth > 0 || StartsWith (line, index, "From "))
				flowed.Append (' ');

			if (flowed.Length + (line.Length - index) <= MaxLineLength) {
				flowed.Append (line.Substring (index));
				index = line.Length;

				return flowed.ToString ();
			}

			do {
				do {
					flowed.Append (line[index++]);
				} while (flowed.Length + 1 < MaxLineLength && index < line.Length && line[index] != ' ');

				if (flowed.Length >= OptimalLineLength) {
					flowed.Append (' ');
					break;
				}

				while (flowed.Length + 1 < MaxLineLength && index < line.Length && line[index] == ' ')
					flowed.Append (line[index++]);
			} while (index < line.Length && flowed.Length < OptimalLineLength);

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
		/// <para><paramref name="reader"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <c>null</c>.</para>
		/// </exception>
		public override void Convert (TextReader reader, TextWriter writer)
		{
			StringBuilder flowed;
			string line;

			if (reader == null)
				throw new ArgumentNullException (nameof (reader));

			if (writer == null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			flowed = new StringBuilder (MaxLineLength);

			while ((line = reader.ReadLine ()) != null) {
				int quoteDepth;
				int index = 0;

				// Trim spaces before user-inserted hard line breaks.
				line = line.TrimEnd (' ');

				line = Unquote (line, out quoteDepth);

				// Ensure all lines (fixed and flowed) are 78 characters or fewer in
				// length, counting any trailing space as well as a space added as
				// stuffing, but not counting the CRLF, unless a word by itself
				// exceeds 78 characters.
				do {
					var flowedLine = GetFlowedLine (flowed, line, ref index, quoteDepth);
					writer.WriteLine (flowedLine);
				} while (index < line.Length);
			}

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
