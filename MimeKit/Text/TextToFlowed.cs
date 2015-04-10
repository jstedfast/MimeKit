//
// TextToFlowed.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
			get { return TextFormat.Text; }
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

		static bool LineRequiresSpaceStuffing (string line, int startIndex = 0)
		{
			if (line.Length <= startIndex || (line[startIndex] != ' ' && line[startIndex] != '>' && line[startIndex] != 'F'))
				return false;

			if (line[startIndex] == 'F') {
				int index = startIndex + 1;

				if (index >= line.Length || line[index++] != 'r')
					return false;

				if (index >= line.Length || line[index++] != 'o')
					return false;

				if (index >= line.Length || line[index++] != 'm')
					return false;

				if (index >= line.Length || line[index] != ' ')
					return false;
			}

			return true;
		}

		static string GetFlowedLine (char[] flowed, string line, ref int index)
		{
			int length = 0;

			// Space-stuff lines which start with a space, "From ", or ">".
			if (LineRequiresSpaceStuffing (line, index))
				flowed[length++] = ' ';

			do {
				while (length < flowed.Length && index < line.Length && line[index] != ' ')
					flowed[length++] = line[index++];

				if (length == flowed.Length) {
					flowed[length - 1] = ' ';
					index--;
					break;
				}

				if (length >= OptimalLineLength) {
					flowed[length++] = ' ';
					break;
				}

				while (length < flowed.Length && index < line.Length && line[index] == ' ')
					flowed[length++] = line[index++];

				if (length == flowed.Length) {
					index--;
					break;
				}
			} while (index < line.Length && length < MaxLineLength);

			return new string (flowed, 0, length);
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
			char[] flowed;
			string line;

			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			flowed = new char[MaxLineLength];

			while ((line = reader.ReadLine ()) != null) {
				int index = 0;

				// Trim spaces before user-inserted hard line breaks.
				line.TrimEnd (' ');

				// Ensure all lines (fixed and flowed) are 78 characters or fewer in
				// length, counting any trailing space as well as a space added as
				// stuffing, but not counting the CRLF, unless a word by itself
				// exceeds 78 characters.
				do {
					var flowedLine = GetFlowedLine (flowed, line, ref index);

					if (index >= line.Length) {
						writer.WriteLine (flowedLine);
						break;
					}

					writer.Write (flowedLine);
				} while (true);
			}

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
