//
// RtfToText.cs
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
using System.Threading;

using MimeKit.Utils;

namespace MimeKit.Text {
	/// <summary>
	/// An RTF to plain text converter.
	/// </summary>
	/// <remarks>
	/// <para>Used to convert Rich Text Format (RTF) documents into plain text.</para>
	/// <para>The converter implements the subset of the Rich Text Format (RTF) Specification, version 1.9.1,
	/// that affects the visible text of a document: character sets and code pages, Unicode escapes,
	/// paragraphs, line breaks, tabs, tables and special characters. Destinations that do not contain
	/// document text (such as pictures, embedded objects, style sheets and document properties) are skipped,
	/// as is hidden text. The attachment placeholders (<c>\objattph</c>) that Microsoft Outlook and Exchange write are
	/// reported to the <see cref="AttachmentPlaceholderCallback"/>.</para>
	/// <para>The converter is designed to be resilient against hostile input. Its memory usage does not depend
	/// on the size of the input: font table, color table and group nesting resources are bounded by
	/// <see cref="MaxFontTableEntries"/>, <see cref="MaxColorTableEntries"/> and <see cref="MaxGroupDepth"/>,
	/// and embedded binary data (<c>\binN</c>) is skipped without being buffered.</para>
	/// <note type="note">Since RTF is a 7-bit format in which non-ASCII characters are escaped, the
	/// <see cref="TextConverter.InputEncoding"/> defaults to ISO-8859-1 so that every byte of the input
	/// maps to exactly one character. The code page used to decode escaped characters is determined by the
	/// document itself (<c>\ansicpg</c>, <c>\fcharset</c> and <c>\cpg</c>).</note>
	/// </remarks>
	public class RtfToText : TextConverter
	{
		internal const int DefaultLimit = 4096;

		int maxFontTableEntries = DefaultLimit;
		int maxColorTableEntries = DefaultLimit;
		int maxGroupDepth = DefaultLimit;

		/// <summary>
		/// Initialize a new instance of the <see cref="RtfToText"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new RTF to text converter.
		/// </remarks>
		public RtfToText ()
		{
			InputEncoding = CharsetUtils.Latin1;
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public override TextFormat InputFormat {
			get { return TextFormat.RichText; }
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
		/// Get or set the maximum number of font table entries.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of entries that will be recorded from the document's font table.</para>
		/// <para>Each font table entry may specify the character set used to decode text formatted with that font.
		/// Font table entries beyond this limit are ignored and text formatted with those fonts is decoded using
		/// the document's default code page.</para>
		/// </remarks>
		/// <value>The maximum number of font table entries.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxFontTableEntries {
			get { return maxFontTableEntries; }
			set { maxFontTableEntries = ValidateLimit (value); }
		}

		/// <summary>
		/// Get or set the maximum number of color table entries.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of entries that will be recorded from the document's color table.</para>
		/// <para>Color table entries beyond this limit are ignored.</para>
		/// </remarks>
		/// <value>The maximum number of color table entries.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxColorTableEntries {
			get { return maxColorTableEntries; }
			set { maxColorTableEntries = ValidateLimit (value); }
		}

		/// <summary>
		/// Get or set the maximum group depth.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of distinct nested group states that will be tracked.</para>
		/// <para>RTF groups (<c>{</c> ... <c>}</c>) save and restore formatting state. Consecutive nested
		/// groups that do not change the state between them share a single entry, so arbitrarily deep nesting
		/// of groups that do not change any formatting does not count towards this limit. The content of any
		/// group that would exceed this limit is discarded.</para>
		/// </remarks>
		/// <value>The maximum group depth.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxGroupDepth {
			get { return maxGroupDepth; }
			set { maxGroupDepth = ValidateLimit (value); }
		}

		/// <summary>
		/// Get or set the <see cref="RtfToTextAttachmentPlaceholderCallback"/> method to use for rendering attachment placeholders.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the <see cref="RtfToTextAttachmentPlaceholderCallback"/> method to use for rendering the
		/// attachment placeholders (<c>\objattph</c>) described by [MS-OXRTFEX] section 2.2.3.4.</para>
		/// <para>The callback is invoked once for each placeholder that is rendered, in document order. Placeholders
		/// that are hidden or that are within a destination that is skipped are not rendered.</para>
		/// <para>When the value is <see langword="null" />, the placeholders are not rendered.</para>
		/// </remarks>
		/// <value>The attachment placeholder callback.</value>
		public RtfToTextAttachmentPlaceholderCallback? AttachmentPlaceholderCallback {
			get; set;
		}

		internal static int ValidateLimit (int value)
		{
			if (value < 1)
				throw new ArgumentOutOfRangeException (nameof (value));

			return value;
		}

		sealed class TextHandler : RtfContentHandler
		{
			readonly RtfToTextAttachmentPlaceholderCallback? placeholderCallback;
			readonly TextWriter writer;
			bool pendingCellSeparator;
			int placeholderIndex;

			public TextHandler (TextWriter writer, RtfToTextAttachmentPlaceholderCallback? placeholderCallback)
			{
				this.placeholderCallback = placeholderCallback;
				this.writer = writer;
			}

			void FlushCellSeparator ()
			{
				if (pendingCellSeparator) {
					writer.Write ('\t');
					pendingCellSeparator = false;
				}
			}

			public override void OnText (RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				FlushCellSeparator ();
				writer.Write (buffer, index, count);
			}

			public override void OnParagraph (RtfInterpreter rtf)
			{
				FlushCellSeparator ();
				writer.WriteLine ();
			}

			public override void OnLineBreak (RtfInterpreter rtf)
			{
				FlushCellSeparator ();
				writer.WriteLine ();
			}

			// RTF 1.9.1, "Table Definitions": every cell, including the last cell in a row, is terminated
			// by \cell. Cells are separated by a tab, but the separator is deferred so that a row does not
			// end with a trailing tab.
			public override void OnCell (RtfInterpreter rtf)
			{
				FlushCellSeparator ();
				pendingCellSeparator = true;
			}

			public override void OnRow (RtfInterpreter rtf)
			{
				pendingCellSeparator = false;
				writer.WriteLine ();
			}

			public override void OnObjectPlaceholder (RtfInterpreter rtf)
			{
				// The index is advanced even if the callback is not invoked so that the placeholders stay in lock
				// step with the attachment list ([MS-OXRTFEX] 2.2.3.4).
				int index = placeholderIndex;

				if (placeholderIndex < int.MaxValue)
					placeholderIndex++;

				if (placeholderCallback is null)
					return;

				FlushCellSeparator ();
				placeholderCallback (index, writer);
			}
		}

		/// <summary>
		/// Counts the attachment placeholders that <see cref="Convert(TextReader, TextWriter)"/> would report to the
		/// <see cref="AttachmentPlaceholderCallback"/>.
		/// </summary>
		/// <remarks>
		/// [MS-OXRTFEX] 2.2.3.4: the placeholders are matched with the attachments only if their numbers are equal,
		/// which has to be known before the document is rendered.
		/// </remarks>
		/// <returns>The number of placeholders (saturating at <see cref="int.MaxValue"/>).</returns>
		internal int CountObjectPlaceholders (TextReader reader, CancellationToken cancellationToken)
		{
			var counter = new RtfToHtml.PlaceholderCounter ();
			var interpreter = new RtfInterpreter (reader, counter, MaxGroupDepth, MaxFontTableEntries, MaxColorTableEntries) {
				ExtractHtml = false
			};

			while (interpreter.Step ())
				cancellationToken.ThrowIfCancellationRequested ();

			return counter.Count;
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
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!string.IsNullOrEmpty (Header))
				writer.Write (Header);

			// Encapsulated HTML ([MS-OXRTFEX]) is deliberately not extracted: the RTF portion of such a document
			// (including the \htmlrtf blocks) is a faithful rendering of the HTML, and is much better suited to
			// producing plain text than the HTML markup would be.
			var interpreter = new RtfInterpreter (reader, new TextHandler (writer, AttachmentPlaceholderCallback), MaxGroupDepth, MaxFontTableEntries, MaxColorTableEntries) {
				ExtractHtml = false
			};

			while (interpreter.Step ())
				;

			if (!string.IsNullOrEmpty (Footer))
				writer.Write (Footer);
		}
	}
}
