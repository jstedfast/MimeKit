//
// HtmlReader.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Text {
	class HtmlReader : IDisposable
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly HashSet<string> AutoCloseTags;
		const string PlainTextEndTag = "</plaintext>";
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 1;

		enum HtmlParserState {
			Initial,
			Error,
			EndOfFile,
			Closed,

			ReadText,
			ReadTagName,
			ReadAttributeName,
			ReadAttributeValue,
		}

		// I/O buffering
		readonly char[] input = new char[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// attribute buffer
		char[] tokenBuffer = new char[512];
		char attributeValueQuote;
		int tokenIndex;

		readonly IList<HtmlAttribute> attributes = new List<HtmlAttribute> ();
		readonly IList<string> openTags = new List<string> ();

		HtmlParserState state;
		TextReader reader;
		int position;
		bool eof;

		static HtmlReader ()
		{
			// Note: These are tags that auto-close when an identical tag is encountered and/or when a parent node is closed.
			AutoCloseTags = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			AutoCloseTags.Add ("li");
			AutoCloseTags.Add ("p");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlReader"/>.
		/// </remarks>
		/// <param name="input">The input.</param>
		public HtmlReader (TextReader input)
		{
			reader = input;
		}

		/// <summary>
		/// Releas unmanaged resources and perform other cleanup operations before the
		/// <see cref="MimeKit.Text.HtmlReader"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Text.HtmlReader"/> is reclaimed by garbage collection.
		/// </remarks>
		~HtmlReader ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (state == HtmlParserState.Closed)
				throw new ObjectDisposedException ("HtmlReader");
		}

		/// <summary>
		/// Get an attribute reader for the current HTML tag.
		/// </summary>
		/// <remarks>
		/// Gets an attribute reader for the current HTML tag.
		/// </remarks>
		/// <value>The attribute reader.</value>
		public HtmlAttributeReader AttributeReader {
			// FIXME: this should probably throw an exception if not in the proper state
			get; private set;
		}

		/// <summary>
		/// Get the current character offset in the HTML document.
		/// </summary>
		/// <remarks>
		/// Gets the current character offset in the HTML document.
		/// </remarks>
		/// <value>The current character offset.</value>
		public int CurrentOffset {
			get { return GetOffset (inputIndex); }
		}

		/// <summary>
		/// Get the current nesting depth of the HTML document.
		/// </summary>
		/// <remarks>
		/// Gets the current nesting depth of the HTML document.
		/// </remarks>
		/// <value>The depth.</value>
		public int Depth {
			get; private set;
		}

		/// <summary>
		/// Get the identifier for the current HTNL tag.
		/// </summary>
		/// <remarks>
		/// Gets the identifier for the current HTNL tag.
		/// </remarks>
		/// <value>The tag identifier.</value>
		public HtmlTagId TagId {
			get; private set;
		}

		/// <summary>
		/// Get the kind of the token currently being processed.
		/// </summary>
		/// <remarks>
		/// Gets the kind of the token currently being processed.
		/// </remarks>
		/// <value>The kind of the token.</value>
		public HtmlTokenKind TokenKind {
			get; private set;
		}

		int ReadAhead (int save = 0)
		{
			int left = inputEnd - inputIndex;

			if (eof)
				return left;

			int start = inputStart;
			int end = inputEnd;
			int nread;

			left += save;

			if (left > 0) {
				int index = inputIndex - save;

				// attempt to align the end of the remaining input with ReadAheadSize
				if (index >= start) {
					start -= Math.Min (ReadAheadSize, left);
					Buffer.BlockCopy (input, index, input, start, left);
					index = start;
					start += left;
				} else if (index > 0) {
					int shift = Math.Min (index, end - start);
					Buffer.BlockCopy (input, index, input, index - shift, left);
					index -= shift;
					start = index + left;
				} else {
					// we can't shift...
					start = end;
				}

				inputIndex = index + save;
				inputEnd = start;
			} else {
				inputIndex = start;
				inputEnd = start;
			}

			end = input.Length - PadSize;

			if ((nread = reader.Read (input, start, end - start)) > 0) {
				inputEnd += nread;
				position += nread;
			} else {
				eof = true;
			}

			return inputEnd - inputIndex;
		}

		int GetOffset (int index)
		{
			return position - (inputEnd - index);
		}

		bool CheckAtPlainTextEndTag ()
		{
			int left = inputEnd - inputIndex;
			int index;

			if (left < PlainTextEndTag.Length && ReadAhead () < PlainTextEndTag.Length)
				return false;
			
			index = inputIndex;

			for (int i = 0; i < PlainTextEndTag.Length; i++) {
				if (PlainTextEndTag[i] != char.ToLowerInvariant (input[index]))
					return false;
				index++;
			}

			return true;
		}

		/// <summary>
		/// Advance the reader to the next HTML token.
		/// </summary>
		/// <remarks>
		/// Advances the reader to the next HTML token.
		/// </remarks>
		/// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
		public bool ReadNextToken ()
		{
			CheckDisposed ();

			switch (state) {
			case HtmlParserState.EndOfFile:
			case HtmlParserState.Closed:
			case HtmlParserState.Error:
				return false;
			}

			return true;
		}

		static void ValidateArguments (char[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		/// <summary>
		/// Read markup text directly from the underlying stream without parsing.
		/// </summary>
		/// <remarks>
		/// Reads markup text directly from the underlying stream without parsing.
		/// </remarks>
		/// <returns>The number of characters read.</returns>
		/// <param name="buffer">The buffer to read the HTML markup text into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="offset"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlReader"/> has been disposed.
		/// </exception>
		public int ReadMarkupText (char[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);
			CheckDisposed ();

			int end = offset + count;
			int index = offset;
			int n;

			if (inputEnd > inputIndex) {
				n = Math.Min (inputEnd - inputIndex, count);
				Buffer.BlockCopy (input, inputIndex, buffer, index, n);
				inputIndex += n;
				index += n;
			}

			if (index < end && !eof) {
				if ((n = reader.Read (buffer, index, end - index)) > 0) {
					state = HtmlParserState.Initial;
					position += n;
					index += n;
				} else {
					state = HtmlParserState.EndOfFile;
					eof = true;
				}
			}

			return index - offset;
		}

		/// <summary>
		/// Read the text between an HTML start tag and an HTML end tag.
		/// </summary>
		/// <remarks>
		/// Reads the text between an HTML start tag and an HTML end tag.
		/// </remarks>
		/// <returns>The number of characters read.</returns>
		/// <param name="buffer">The buffer to read the text into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="offset"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlReader"/> is not currently at a text token.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlReader"/> has been disposed.
		/// </exception>
		public int ReadText (char[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);
			CheckDisposed ();

			if (TokenKind != HtmlTokenKind.Text)
				throw new InvalidOperationException ("The HtmlReader is not currently at a text token.");

			if (state == HtmlParserState.Initial)
				throw new InvalidOperationException ("You must call HtmlReader.ReadNextToken() before you can read text.");
			
			int end = offset + count;
			int index = offset;

			do {
				input[inputEnd] = '<';
				while (index < end && input[inputIndex] != '<')
					buffer[index++] = input[inputIndex++];

				if (index == end)
					break;

				if (inputIndex < inputEnd) {
					if (TagId != HtmlTagId.PlainText || CheckAtPlainTextEndTag ())
						break;
				}

				if (ReadAhead () == 0)
					break;
			} while (true);

			return index - offset;
		}

		/// <summary>
		/// Read the name of the current HTML tag.
		/// </summary>
		/// <remarks>
		/// Reads the name of the current HTML tag.
		/// </remarks>
		/// <returns>The name of the HTML tag.</returns>
		public string ReadTagName ()
		{
			CheckDisposed ();

			if (TokenKind == HtmlTokenKind.Text)
				throw new InvalidOperationException ("The HtmlReader is not currently at an HTML tag token.");

			return null;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="HtmlReader"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases any unmanaged resources used by the <see cref="HtmlReader"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				state = HtmlParserState.Closed;
				reader = null;
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Text.HtmlReader"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="MimeKit.Text.HtmlReader"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="MimeKit.Text.HtmlReader"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="MimeKit.Text.HtmlReader"/> so the garbage
		/// collector can reclaim the memory that the <see cref="MimeKit.Text.HtmlReader"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);

			GC.SuppressFinalize (this);
		}
	}
}
