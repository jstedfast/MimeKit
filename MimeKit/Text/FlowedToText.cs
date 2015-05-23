﻿//
// FlowedToText.cs
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
using System.Text;

namespace MimeKit.Text {
	/// <summary>
	/// A flowed text to text converter.
	/// </summary>
	/// <remarks>
	/// Unwraps the flowed text format described in rfc3676.
	/// </remarks>
	public class FlowedToText : TextConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.FlowedToText"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new flowed text to text converter.
		/// </remarks>
		public FlowedToText ()
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
			get { return TextFormat.Flowed; }
		}

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public override TextFormat OutputFormat {
			get { return TextFormat.Text; }
		}

		/// <summary>
		/// Get or set whether the trailing space on a wrapped line should be deleted.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the trailing space on a wrapped line should be deleted.</para>
		/// <para>The flowed text format defines a Content-Type parameter called "delsp" which can
		/// have a value of "yes" or "no". If the parameter exists and the value is "yes", then
		/// <see cref="DeleteSpace"/> should be set to <c>true</c>, otherwise <see cref="DeleteSpace"/>
		/// should be set to false.</para>
		/// </remarks>
		/// <value><c>true</c> if the trailing space on a wrapped line should be deleted; otherwise, <c>false</c>.</value>
		public bool DeleteSpace {
			get; set;
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

			if (line.Length == 0)
				return line;

			while (index < line.Length && line[index] == '>') {
				quoteDepth++;
				index++;
			}

			if (index > 0 && index < line.Length && line[index] == ' ')
				index++;

			return index > 0 ? line.Substring (index) : line;
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
			StringBuilder para = new StringBuilder ();
			int paraQuoteDepth = -1;
			int quoteDepth;
			string line;

			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			while ((line = reader.ReadLine ()) != null) {
				line = Unquote (line, out quoteDepth);

				// if there is a leading space, it was stuffed
				if (quoteDepth == 0 && line.Length > 0 && line[0] == ' ')
					line = line.Substring (1);

				if (paraQuoteDepth == -1) {
					paraQuoteDepth = quoteDepth;
				} else if (quoteDepth != paraQuoteDepth) {
					// Note: according to rfc3676, when a folded line has a different quote
					// depth than the previous line, then quote-depth rules win and we need
					// to treat this as a new paragraph.
					if (paraQuoteDepth > 0)
						writer.Write (new string ('>', paraQuoteDepth) + " ");
					writer.WriteLine (para);
					paraQuoteDepth = quoteDepth;
					para.Length = 0;
				}

				para.Append (line);

				if (line.Length == 0 || line[line.Length - 1] != ' ') {
					// when a line does not end with a space, then the paragraph has ended
					if (paraQuoteDepth > 0)
						writer.Write (new string ('>', paraQuoteDepth) + " ");
					writer.WriteLine (para);
					paraQuoteDepth = -1;
					para.Length = 0;
				} else if (DeleteSpace) {
					// Note: lines ending with a space mean that the next line is a continuation
					para.Length--;
				}
			}

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
