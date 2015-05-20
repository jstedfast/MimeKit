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
using System.Text;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit.Text {
	enum HtmlReaderState {
		Initial,
		Error,
		EndOfFile,
		Closed,

		ReadTag,
		ReadText
	}

	class HtmlReader : IDisposable
	{
		static readonly HashSet<string> SelfClosingTags;
		const string PlainTextEndTag = "</plaintext>";
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 1;

		// I/O buffering
		readonly char[] input = new char[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// other buffers
		readonly StringBuilder value = new StringBuilder ();
		readonly StringBuilder name = new StringBuilder ();

		TextReader reader;
		int position;
		bool plaintext;
		bool eof;

		static HtmlReader ()
		{
			// Note: These tags are considered to be Empty Elements even if they do not end with "/>".
			SelfClosingTags = new HashSet<string> (new [] {
				"area",
				"base",
				"br",
				"col",
				"command",
				"embed",
				"hr",
				"img",
				"input",
				"keygen",
				"link",
				"meta",
				"param",
				"source",
				"track",
				"wbr"
			}, StringComparer.OrdinalIgnoreCase);
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
			if (State == HtmlReaderState.Closed)
				throw new ObjectDisposedException ("HtmlReader");
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
		/// Get the current state of the HTML reader.
		/// </summary>
		/// <remarks>
		/// Gets the current state of the HTML reader.
		/// </remarks>
		/// <value>The state of the reader.</value>
		public HtmlReaderState State {
			get; internal set;
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

			for (int i = 0; i < PlainTextEndTag.Length - 1; i++) {
				if (PlainTextEndTag[i] != char.ToLowerInvariant (input[index]))
					return false;
				index++;
			}

			return input[index] == '>' || IsWhiteSpace (input[index]);
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';
		}

		bool SkipWhiteSpace ()
		{
			do {
				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				input[inputEnd] = '>';
				while (IsWhiteSpace (input[inputIndex]))
					inputIndex++;
			} while (inputIndex >= inputEnd);

			return true;
		}

		bool ReadAttributeName ()
		{
			if (inputIndex >= inputEnd && ReadAhead () == 0) {
				State = HtmlReaderState.EndOfFile;
				return false;
			}

			if (!HtmlUtils.IsValidStartCharacter (input[inputIndex])) {
				State = HtmlReaderState.Error;
				return false;
			}

			value.Append (input[inputIndex++]);

			do {
				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				input[inputEnd] = ' ';
				while (HtmlUtils.IsValidNameCharacter (input[inputIndex]))
					value.Append (input[inputIndex++]);
			} while (inputIndex >= inputEnd);

			return true;
		}

		bool ReadAttributeValue ()
		{
			if (inputIndex >= inputEnd && ReadAhead () == 0) {
				State = HtmlReaderState.EndOfFile;
				return false;
			}

			if (input[inputIndex] == '\'' || input[inputIndex] == '"') {
				char quote = input[inputIndex++];

				value.Append (quote);

				do {
					if (inputIndex >= inputEnd && ReadAhead () == 0) {
						State = HtmlReaderState.EndOfFile;
						return false;
					}

					input[inputEnd] = quote;
					while (input[inputIndex] != quote)
						value.Append (input[inputIndex++]);
				} while (inputIndex >= inputEnd);

				value.Append (quote);
				inputIndex++;
			} else {
				do {
					if (inputIndex >= inputEnd && ReadAhead () == 0) {
						State = HtmlReaderState.EndOfFile;
						return false;
					}

					input[inputEnd] = ' ';
					while (!IsWhiteSpace (input[inputIndex]) && input[inputIndex] != '>')
						value.Append (input[inputIndex++]);
				} while (inputIndex >= inputEnd);
			}

			return true;
		}

		bool ReadTagName ()
		{
			if (inputIndex >= inputEnd && ReadAhead () == 0) {
				State = HtmlReaderState.EndOfFile;
				return false;
			}

			if (!HtmlUtils.IsValidStartCharacter (input[inputIndex])) {
				State = HtmlReaderState.Error;
				return false;
			}

			value.Append (input[inputIndex]);
			name.Append (input[inputIndex]);
			inputIndex++;

			do {
				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				input[inputEnd] = ' ';
				while (HtmlUtils.IsValidNameCharacter (input[inputIndex])) {
					value.Append (input[inputIndex]);
					name.Append (input[inputIndex]);
					inputIndex++;
				}
			} while (inputIndex >= inputEnd);

			return true;
		}

		bool ReadComment (out HtmlToken token)
		{
			token = null;

			value.Append ('!');
			inputIndex++;

			if (inputIndex == inputEnd && ReadAhead () == 0) {
				State = HtmlReaderState.EndOfFile;
				return false;
			}

			value.Append (input[inputIndex]);
			name.Append (input[inputIndex]);
			inputIndex++;

			if (inputIndex == inputEnd && ReadAhead () == 0) {
				State = HtmlReaderState.EndOfFile;
				return false;
			}

			value.Append (input[inputIndex]);
			name.Append (input[inputIndex]);
			inputIndex++;

			if (name[0] == '-' && name[1] == '-') {
				do {
					if (inputIndex == inputEnd && ReadAhead () == 0) {
						State = HtmlReaderState.EndOfFile;
						return false;
					}

					while (inputIndex < inputEnd) {
						if (input[inputIndex] == '>' && value.Length > 6 &&
							value[value.Length - 2] == '-' && value[value.Length - 1] == '-') {
							value.Append (input[inputIndex++]);
							break;
						}

						value.Append (input[inputIndex++]);
					}
				} while (inputIndex >= inputEnd);

				token = new HtmlTokenComment (HtmlTokenKind.Comment, value.ToString ());
				State = GetNextState ();

				return true;
			}

			do {
				if (inputIndex == inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				input[inputEnd] = '>';
				while (input[inputIndex] != '>')
					value.Append (input[inputIndex++]);
			} while (inputIndex >= inputEnd);

			value.Append ('>');
			inputIndex++;

			token = new HtmlTokenDocType (HtmlTokenKind.DocType, value.ToString ());
			State = GetNextState ();

			return true;
		}

		internal HtmlReaderState GetNextState ()
		{
			if (inputIndex >= inputEnd && ReadAhead () == 0)
				return HtmlReaderState.EndOfFile;

			if (input[inputIndex] == '<')
				return HtmlReaderState.ReadTag;

			return HtmlReaderState.ReadText;
		}

		/// <summary>
		/// Advance the reader to the next HTML token.
		/// </summary>
		/// <remarks>
		/// Advances the reader to the next HTML token.
		/// </remarks>
		/// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
		public bool ReadNextToken (out HtmlToken token)
		{
			CheckDisposed ();

			switch (State) {
			case HtmlReaderState.EndOfFile:
			case HtmlReaderState.Closed:
			case HtmlReaderState.Error:
				token = null;
				return false;
			default:
				if (ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					token = null;
					return false;
				}

				if (input[inputIndex] == '<')
					goto case HtmlReaderState.ReadTag;

				goto case HtmlReaderState.ReadText;
			case HtmlReaderState.ReadText:
				token = new HtmlTokenText (HtmlTokenKind.Text, this);
				break;
			case HtmlReaderState.ReadTag:
				var kind = HtmlTokenKind.StartTag;

				value.Length = 0;
				name.Length = 0;
				token = null;

				// read the '<'
				value.Append (input[inputIndex++]);

				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				if (input[inputIndex] == '!')
					return ReadComment (out token);

				if (input[inputIndex] == '/') {
					kind = HtmlTokenKind.EndTag;
					value.Append ('/');
					inputIndex++;
				}

				// read the tag name
				if (!ReadTagName ())
					return false;

				// skip white space after the tag name
				if (!SkipWhiteSpace ())
					return false;

				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				// read attributes
				while (input[inputIndex] != '/' && input[inputIndex] != '>') {
					value.Append (' ');

					if (!ReadAttributeName ())
						return false;
					
					if (!SkipWhiteSpace ())
						return false;
					
					if (inputIndex >= inputEnd && ReadAhead () == 0) {
						State = HtmlReaderState.EndOfFile;
						return false;
					}

					if (input[inputIndex] != '=') {
						if (!SkipWhiteSpace ())
							return false;
						
						continue;
					}

					value.Append ('=');
					inputIndex++;

					if (!SkipWhiteSpace ())
						return false;
					
					if (!ReadAttributeValue ())
						return false;
					
					if (!SkipWhiteSpace ())
						return false;
				}

				if (input[inputIndex] == '/') {
					kind = HtmlTokenKind.EmptyElementTag;
					value.Append ('/');
					inputIndex++;
				}

				if (inputIndex >= inputEnd && ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					return false;
				}

				if (input[inputIndex] != '>') {
					State = HtmlReaderState.Error;
					return false;
				}

				value.Append ('>');
				inputIndex++;

				var tag = name.ToString ();

				if (kind == HtmlTokenKind.StartTag && SelfClosingTags.Contains (tag))
					kind = HtmlTokenKind.EmptyElementTag;
				
				token = new HtmlTokenTag (kind, tag, value.ToString ());

				if (kind == HtmlTokenKind.StartTag) {
					plaintext = tag.ToHtmlTagId () == HtmlTagId.PlainText;
				} else if (kind == HtmlTokenKind.EndTag && plaintext) {
					plaintext = false;
				}

				State = GetNextState ();
				break;
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
					State = HtmlReaderState.Initial;
					position += n;
					index += n;
				} else {
					State = HtmlReaderState.EndOfFile;
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

			if (State == HtmlReaderState.Initial)
				throw new InvalidOperationException ("You must call HtmlReader.ReadNextToken() before you can read text.");

			if (State != HtmlReaderState.ReadText)
				return 0;
			
			int end = offset + count;
			int index = offset;

			do {
				input[inputEnd] = '<';
				while (index < end && input[inputIndex] != '<')
					buffer[index++] = input[inputIndex++];

				if (index == end)
					break;

				if (inputIndex < inputEnd) {
					if (!plaintext || CheckAtPlainTextEndTag ()) {
						State = HtmlReaderState.ReadTag;
						break;
					}
				} else if (ReadAhead () == 0) {
					State = HtmlReaderState.EndOfFile;
					break;
				}
			} while (true);

			return index - offset;
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
				State = HtmlReaderState.Closed;
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
