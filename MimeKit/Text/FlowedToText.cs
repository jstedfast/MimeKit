// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;

using MimeKit.Utils;

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
		/// Initialize a new instance of the <see cref="FlowedToText"/> class.
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
			get { return TextFormat.Plain; }
		}

		/// <summary>
		/// Get or set whether the trailing space on a wrapped line should be deleted.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the trailing space on a wrapped line should be deleted.</para>
		/// <para>The flowed text format defines a Content-Type parameter called "delsp" which can
		/// have a value of "yes" or "no". If the parameter exists and the value is "yes", then
		/// <see cref="DeleteSpace"/> should be set to <c>true</c>, otherwise <see cref="DeleteSpace"/>
		/// should be set to <c>false</c>.</para>
		/// </remarks>
		/// <value><c>true</c> if the trailing space on a wrapped line should be deleted; otherwise, <c>false</c>.</value>
		public bool DeleteSpace {
			get; set;
		}

		static ReadOnlySpan<char> Unquote (ReadOnlySpan<char> line, out int quoteDepth)
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

			return index > 0 ? line.Slice (index) : line;
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
			var para = new StringBuilder ();
			int paraQuoteDepth = -1;
			string line;

			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			while ((line = reader.ReadLine ()) != null) {
				var unquoted = Unquote (line.AsSpan (), out int quoteDepth);

				// if there is a leading space, it was stuffed
				if (quoteDepth == 0 && unquoted.Length > 0 && unquoted[0] == ' ')
					unquoted = unquoted.Slice (1);

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

				para.Append (unquoted);

				if (unquoted.Length == 0 || unquoted[unquoted.Length - 1] != ' ') {
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
