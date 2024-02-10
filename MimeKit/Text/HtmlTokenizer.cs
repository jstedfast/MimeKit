//
// HtmlTokenizer.cs
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
using System.Text;
using System.Runtime.CompilerServices;

namespace MimeKit.Text {
	/// <summary>
	/// An HTML tokenizer.
	/// </summary>
	/// <remarks>
	/// Tokenizes HTML text, emitting an <see cref="HtmlToken"/> for each token it encounters.
	/// </remarks>
	public class HtmlTokenizer
	{
		// Specification: https://dev.w3.org/html5/spec-LC/tokenization.html
		const string DocType = "doctype";
		const string CData = "[CDATA[";

		const int MinimumBufferSize = 1024;

		readonly HtmlEntityDecoder entity = new HtmlEntityDecoder ();
		readonly CharBuffer data = new CharBuffer (2048);
		readonly CharBuffer name = new CharBuffer (32);

		readonly TextReader textReader;
		readonly Stream stream;
		Encoding encoding;
		Decoder decoder;

		readonly byte[] input;
		int inputEnd;

		char[] buffer;
		int bufferIndex, bufferEnd;

		readonly char[] cdata = new char[3];
		int cdataIndex;

		HtmlDocTypeToken doctype;
		HtmlAttribute attribute;
		string activeTagName;
		HtmlTagToken tag;
		char quote;

		bool detectEncodingFromByteOrderMarks;
		bool detectByteOrderMark;
		bool isEndTag;
		bool bang;
		bool eof;

		HtmlTokenizer ()
		{
			DecodeCharacterReferences = true;
			LinePosition = 1;
			LineNumber = 1;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTokenizer"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="HtmlTokenizer"/>.</para>
		/// <para>This constructor will attempt to auto-detect the appropriate encoding to use by examining the first four bytes of the stream
		/// and, if a unicode byte-order-mark is detected, use the appropriate unicode encoding. If no byte order mark is detected, then it will
		/// default to UTF-8.</para>
		/// </remarks>
		/// <param name="stream">The input stream.</param>
		public HtmlTokenizer (Stream stream) : this (stream, Encoding.UTF8)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTokenizer"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="HtmlTokenizer"/>.</para>
		/// <para>This constructor will attempt to auto-detect the appropriate encoding to use by examining the first four bytes of the stream
		/// and, if a unicode byte-order-mark is detected, use the appropriate unicode encoding. If no byte order mark is detected, then it will
		/// default to the user-supplied encoding.</para>
		/// </remarks>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The charset encoding of the stream.</param>
		public HtmlTokenizer (Stream stream, Encoding encoding) : this (stream, encoding, true)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTokenizer"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="HtmlTokenizer"/>.</para>
		/// <para>This constructor allows you to change the encoding the first time you read from the <see cref="HtmlTokenizer"/>. The
		/// <paramref name="detectEncodingFromByteOrderMarks"/> parameter detects the encoding by looking at the first four bytes of the stream.
		/// It will automatically recognize UTF-8, little-endian UTF-16, big-endian UTF-16, little-endian UTF-32, and big-endian UTF-32 text if
		/// the stream starts with the appropriate byte order marks. Otherwise, the user-provided encoding is used.</para>
		/// </remarks>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The charset encoding of the stream.</param>
		/// <param name="detectEncodingFromByteOrderMarks"><c>true</c> if byte order marks should be detected and used to override the <paramref name="encoding"/>; otherwise, <c>false</c>.</param>
		/// <param name="bufferSize">The minimum buffer size to use for reading.</param>
		public HtmlTokenizer (Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize = 4096) : this ()
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			input = new byte[Math.Max (MinimumBufferSize, bufferSize)];
			if (!detectEncodingFromByteOrderMarks) {
				buffer = new char[encoding.GetMaxCharCount (input.Length)];
				decoder = encoding.GetDecoder ();
			}

			this.detectEncodingFromByteOrderMarks = detectEncodingFromByteOrderMarks;
			this.detectByteOrderMark = !detectEncodingFromByteOrderMarks;
			this.encoding = encoding;
			this.stream = stream;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTokenizer"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlTokenizer"/>.
		/// </remarks>
		/// <param name="reader">The <see cref="TextReader"/>.</param>
		public HtmlTokenizer (TextReader reader) : this ()
		{
			if (reader == null)
				throw new ArgumentNullException (nameof (reader));

			buffer = new char[2048];
			textReader = reader;
		}

		/// <summary>
		/// Get or set whether or not the tokenizer should decode character references.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether or not the tokenizer should decode character references.</para>
		/// <note type="warning">Character references in attribute values will still be decoded
		/// even if this value is set to <c>false</c>.</note>
		/// </remarks>
		/// <value><c>true</c> if character references should be decoded; otherwise, <c>false</c>.</value>
		public bool DecodeCharacterReferences {
			get; set;
		}

		/// <summary>
		/// Get the current HTML namespace detected by the tokenizer.
		/// </summary>
		/// <remarks>
		/// Gets the current HTML namespace detected by the tokenizer.
		/// </remarks>
		/// <value>The html namespace.</value>
		public HtmlNamespace HtmlNamespace {
			get; private set;
		}

		/// <summary>
		/// Get or set whether or not the tokenizer should ignore truncated tags.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether or not the tokenizer should ignore truncated tags.</para>
		/// <para>If <c>false</c> and the stream abruptly ends in the middle of an HTML tag, it will be
		/// treated as an <see cref="HtmlDataToken"/> instead.</para>
		/// </remarks>
		/// <value><c>true</c> if truncated tags should be ignored; otherwise, <c>false</c>.</value>
		public bool IgnoreTruncatedTags {
			get; set;
		}

		/// <summary>
		/// Get the current line number.
		/// </summary>
		/// <remarks>
		/// <para>This property is most commonly used for error reporting, but can be called
		/// at any time. The starting value for this property is <c>1</c>.</para>
		/// <para>Combined with <see cref="LinePosition"/>, a value of <c>1,1</c> indicates
		/// the start of the document.</para>
		/// </remarks>
		/// <value>The current line number.</value>
		public int LineNumber {
			get; private set;
		}

		/// <summary>
		/// Get the current line position.
		/// </summary>
		/// <remarks>
		/// <para>This property is most commonly used for error reporting, but can be called
		/// at any time. The starting value for this property is <c>1</c>.</para>
		/// <para>Combined with <see cref="LineNumber"/>, a value of <c>1,1</c> indicates
		/// the start of the document.</para>
		/// </remarks>
		/// <value>The column position of the current line.</value>
		public int LinePosition {
			get; private set;
		}

		/// <summary>
		/// Get the current state of the tokenizer.
		/// </summary>
		/// <remarks>
		/// Gets the current state of the tokenizer.
		/// </remarks>
		/// <value>The current state of the tokenizer.</value>
		public HtmlTokenizerState TokenizerState {
			get; private set;
		}

		/// <summary>
		/// Create a DOCTYPE token.
		/// </summary>
		/// <remarks>
		/// Creates a DOCTYPE token.
		/// </remarks>
		/// <returns>The DOCTYPE token.</returns>
		protected virtual HtmlDocTypeToken CreateDocType ()
		{
			return new HtmlDocTypeToken ();
		}

		HtmlDocTypeToken CreateDocTypeToken (string rawTagName)
		{
			var token = CreateDocType ();
			token.RawTagName = rawTagName;
			return token;
		}

		/// <summary>
		/// Create an HTML comment token.
		/// </summary>
		/// <remarks>
		/// Creates an HTML comment token.
		/// </remarks>
		/// <returns>The HTML comment token.</returns>
		/// <param name="comment">The comment.</param>
		/// <param name="bogus"><c>true</c> if the comment is bogus; otherwise, <c>false</c>.</param>
		protected virtual HtmlCommentToken CreateCommentToken (string comment, bool bogus = false)
		{
			return new HtmlCommentToken (comment, bogus);
		}

		/// <summary>
		/// Create an HTML character data token.
		/// </summary>
		/// <remarks>
		/// Creates an HTML character data token.
		/// </remarks>
		/// <returns>The HTML character data token.</returns>
		/// <param name="data">The character data.</param>
		protected virtual HtmlDataToken CreateDataToken (string data)
		{
			return new HtmlDataToken (data);
		}

		/// <summary>
		/// Create an HTML character data token.
		/// </summary>
		/// <remarks>
		/// Creates an HTML character data token.
		/// </remarks>
		/// <returns>The HTML character data token.</returns>
		/// <param name="data">The character data.</param>
		protected virtual HtmlCDataToken CreateCDataToken (string data)
		{
			return new HtmlCDataToken (data);
		}

		/// <summary>
		/// Create an HTML script data token.
		/// </summary>
		/// <remarks>
		/// Creates an HTML script data token.
		/// </remarks>
		/// <returns>The HTML script data token.</returns>
		/// <param name="data">The script data.</param>
		protected virtual HtmlScriptDataToken CreateScriptDataToken (string data)
		{
			return new HtmlScriptDataToken (data);
		}

		/// <summary>
		/// Create an HTML tag token.
		/// </summary>
		/// <remarks>
		/// Creates an HTML tag token.
		/// </remarks>
		/// <returns>The HTML tag token.</returns>
		/// <param name="name">The tag name.</param>
		/// <param name="isEndTag"><c>true</c> if the tag is an end tag; otherwise, <c>false</c>.</param>
		protected virtual HtmlTagToken CreateTagToken (string name, bool isEndTag = false)
		{
			return new HtmlTagToken (name, isEndTag);
		}

		/// <summary>
		/// Create an attribute.
		/// </summary>
		/// <remarks>
		/// Creates an attribute.
		/// </remarks>
		/// <returns>The attribute.</returns>
		/// <param name="name">The attribute name.</param>
		protected virtual HtmlAttribute CreateAttribute (string name)
		{
			return new HtmlAttribute (name);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsAlphaNumeric (int c)
		{
			return ((uint) (c - 'A') <= 'Z' - 'A') || ((uint) (c - 'a') <= 'z' - 'a') || ((uint) (c - '0') <= '9' - '0');
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsAsciiLetter (int c)
		{
			return ((uint) (c - 'A') <= 'Z' - 'A') || ((uint) (c - 'a') <= 'z' - 'a');
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static char ToLower (int c)
		{
			// check if the char is within the uppercase range
			if ((uint) (c - 'A') <= 'Z' - 'A')
				return (char) (c + 0x20);

			return (char) c;
		}

		int SkipByteOrderMark (ReadOnlySpan<byte> preamble)
		{
			for (int i = 0; i < preamble.Length; i++) {
				if (input[i] != preamble[i])
					return 0;
			}

			return preamble.Length;
		}

		int DetectByteOrderMark ()
		{
#if NET6_0_OR_GREATER
			var preamble = encoding.Preamble;
#else
			var preamble = encoding.GetPreamble ();
#endif

			detectByteOrderMark = false;

			if (preamble.Length == 0)
				return 0;

			do {
				int nread = stream.Read (input, inputEnd, input.Length - inputEnd);

				if (nread == 0)
					break;

				inputEnd += nread;
			} while (inputEnd < preamble.Length);

			return SkipByteOrderMark (preamble);
		}

		int DetectEncodingFromByteOrderMarks ()
		{
			detectEncodingFromByteOrderMarks = false;

			do {
				int nread = stream.Read (input, inputEnd, input.Length - inputEnd);

				if (nread == 0)
					break;

				inputEnd += nread;
			} while (inputEnd < 4);

			int first2Bytes = inputEnd >= 2 ? input[0] << 8 | input[1] : 0;
			int next2Bytes = inputEnd >= 4 ? (input[2] << 8 |input[3]) : 0;
			const int UTF32BE = 12001;

			switch (first2Bytes) {
			case 0x0000:
				if (next2Bytes == 0xFEFF)
					encoding = Encoding.GetEncoding (UTF32BE);
				break;
			case 0xFEFF:
				encoding = Encoding.BigEndianUnicode;
				break;
			case 0xFFFE:
				if (next2Bytes == 0x0000)
					encoding = Encoding.UTF32;
				else
					encoding = Encoding.Unicode;
				break;
			case 0xEFBB:
				if ((next2Bytes & 0xFF00) == 0xBF00)
					encoding = new UTF8Encoding (true, true);
				break;
			}

			decoder = encoding.GetDecoder ();
			buffer = new char[encoding.GetMaxCharCount (input.Length)];

#if NET6_0_OR_GREATER
			var preamble = encoding.Preamble;
#else
			var preamble = encoding.GetPreamble ();
#endif

			return SkipByteOrderMark (preamble);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void FillBuffer ()
		{
			if (bufferIndex == bufferEnd && !eof) {
				if (stream != null) {
					int inputIndex;

					if (detectEncodingFromByteOrderMarks)
						inputIndex = DetectEncodingFromByteOrderMarks ();
					else if (detectByteOrderMark)
						inputIndex = DetectByteOrderMark ();
					else
						inputIndex = 0;

					bufferIndex = 0;
					bufferEnd = 0;

					do {
						if (inputIndex == inputEnd) {
							inputEnd = stream.Read (input, 0, input.Length);
							inputIndex = 0;
						}

						bufferEnd = decoder.GetChars (input, inputIndex, inputEnd - inputIndex, buffer, 0, inputEnd == 0);
						inputIndex = inputEnd;
					} while (bufferEnd == 0 && inputEnd > 0);

					inputEnd = 0;
				} else {
					bufferEnd = textReader.Read (buffer, 0, buffer.Length);
					bufferIndex = 0;
				}

				eof = bufferEnd == 0;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		bool TryPeek (out char c)
		{
			FillBuffer ();

			if (bufferIndex < bufferEnd) {
				c = buffer[bufferIndex];
				return true;
			}

			c = '\0';

			return false;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void IncrementLineNumber ()
		{
			LinePosition = 1;
			LineNumber++;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void ConsumeCharacter (char c)
		{
			if (c == '\n') {
				IncrementLineNumber ();
			} else {
				LinePosition++;
			}

			bufferIndex++;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		bool TryRead (out char c)
		{
			FillBuffer ();

			if (bufferIndex < bufferEnd) {
				c = buffer[bufferIndex++];

				if (c == '\n') {
					IncrementLineNumber ();
				} else {
					LinePosition++;
				}

				return true;
			}

			c = '\0';

			return false;
		}

		bool NameIs (string value)
		{
			if (name.Length != value.Length)
				return false;

			for (int i = 0; i < name.Length; i++) {
				if (ToLower (name[i]) != ToLower (value[i]))
					return false;
			}

			return true;
		}

		void EmitTagAttribute ()
		{
			attribute = CreateAttribute (name.ToString ());
			tag.Attributes.Add (attribute);
			name.Length = 0;
		}

		HtmlToken EmitCommentToken (string comment, bool bogus = false)
		{
			var token = CreateCommentToken (comment, bogus);
			token.IsBangComment = bang;
			data.Length = 0;
			name.Length = 0;
			bang = false;
			return token;
		}

		HtmlToken EmitCommentToken (CharBuffer comment, bool bogus = false)
		{
			return EmitCommentToken (comment.ToString (), bogus);
		}

		HtmlToken EmitDocType ()
		{
			var token = doctype;
			data.Length = 0;
			doctype = null;
			return token;
		}

		HtmlToken EmitDataToken (bool encodeEntities, bool truncated)
		{
			if (data.Length == 0)
				return null;

			if (truncated && IgnoreTruncatedTags) {
				data.Length = 0;
				return null;
			}

			var token = CreateDataToken (data.ToString ());
			token.EncodeEntities = encodeEntities;
			data.Length = 0;

			return token;
		}

		HtmlToken EmitCDataToken ()
		{
			if (data.Length == 0)
				return null;

			var token = CreateCDataToken (data.ToString ());
			data.Length = 0;

			return token;
		}

		HtmlToken EmitScriptDataToken ()
		{
			if (data.Length == 0)
				return null;

			var token = CreateScriptDataToken (data.ToString ());
			data.Length = 0;

			return token;
		}

		HtmlToken EmitTagToken ()
		{
			if (!tag.IsEndTag && !tag.IsEmptyElement) {
				switch (tag.Id) {
				case HtmlTagId.Style: case HtmlTagId.Xmp: case HtmlTagId.IFrame: case HtmlTagId.NoEmbed: case HtmlTagId.NoFrames:
					TokenizerState = HtmlTokenizerState.RawText;
					activeTagName = tag.Name;
					break;
				case HtmlTagId.Title: case HtmlTagId.TextArea:
					TokenizerState = HtmlTokenizerState.RcData;
					activeTagName = tag.Name;
					break;
				case HtmlTagId.PlainText:
					TokenizerState = HtmlTokenizerState.PlainText;
					break;
				case HtmlTagId.Script:
					TokenizerState = HtmlTokenizerState.ScriptData;
					break;
				case HtmlTagId.NoScript:
					// TODO: only switch into the RawText state if scripting is enabled
					TokenizerState = HtmlTokenizerState.RawText;
					activeTagName = tag.Name;
					break;
				case HtmlTagId.Html:
					TokenizerState = HtmlTokenizerState.Data;

					for (int i = tag.Attributes.Count; i > 0; i--) {
						var attr = tag.Attributes[i - 1];

						if (attr.Id == HtmlAttributeId.XmlNS && attr.Value != null) {
							HtmlNamespace = attr.Value.ToHtmlNamespace ();
							break;
						}
					}
					break;
				default:
					TokenizerState = HtmlTokenizerState.Data;
					break;
				}
			} else {
				TokenizerState = HtmlTokenizerState.Data;
			}

			var token = tag;
			data.Length = 0;
			tag = null;

			return token;
		}

		// 8.2.4.69 Tokenizing character references
		HtmlToken ReadCharacterReference (HtmlTokenizerState next)
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				data.Append ('&');

				return EmitDataToken (true, false);
			}

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ': case '<': case '&':
				// no character is consumed, emit '&'
				TokenizerState = next;
				data.Append ('&');
				return null;
			}

			entity.Push ('&');

			while (entity.Push (c)) {
				ConsumeCharacter (c);

				if (c == ';')
					break;

				if (!TryPeek (out c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					data.Append (entity.GetPushedInput ());
					entity.Reset ();

					return EmitDataToken (true, false);
				}
			}

			TokenizerState = next;

			data.Append (entity.GetValue ());
			entity.Reset ();

			return null;
		}

		HtmlToken ReadGenericRawTextLessThan (HtmlTokenizerState rawText, HtmlTokenizerState rawTextEndTagOpen)
		{
			data.Append ('<');

			if (TryPeek (out char c) && c == '/') {
				TokenizerState = rawTextEndTagOpen;
				ConsumeCharacter (c);
				data.Append ('/');
				name.Length = 0;
			} else {
				TokenizerState = rawText;
			}

			return null;
		}

		HtmlToken ReadGenericRawTextEndTagOpen (bool decoded, HtmlTokenizerState rawText, HtmlTokenizerState rawTextEndTagName)
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitDataToken (decoded, true);
			}

			if (IsAsciiLetter (c)) {
				TokenizerState = rawTextEndTagName;
				ConsumeCharacter (c);
				name.Append (c);
				data.Append (c);
			} else {
				TokenizerState = rawText;
			}

			return null;
		}

		HtmlToken ReadGenericRawTextEndTagName (bool decoded, HtmlTokenizerState rawText)
		{
			var current = TokenizerState;

			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitDataToken (decoded, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					if (NameIs (activeTagName)) {
						TokenizerState = HtmlTokenizerState.BeforeAttributeName;
						break;
					}

					goto default;
				case '/':
					if (NameIs (activeTagName)) {
						TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
						break;
					}
					goto default;
				case '>':
					if (NameIs (activeTagName)) {
						var token = CreateTagToken (name.ToString (), true);
						TokenizerState = HtmlTokenizerState.Data;
						data.Length = 0;
						name.Length = 0;
						return token;
					}
					goto default;
				default:
					if (!IsAsciiLetter (c)) {
						TokenizerState = rawText;
						return null;
					}

					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == current);

			tag = CreateTagToken (name.ToString (), true);
			name.Length = 0;

			return null;
		}

		// 8.2.4.1 Data state
		HtmlToken ReadData ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				switch (c) {
				case '&':
					if (DecodeCharacterReferences) {
						TokenizerState = HtmlTokenizerState.CharacterReferenceInData;
						return null;
					}

					goto default;
				case '<':
					TokenizerState = HtmlTokenizerState.TagOpen;
					break;
				//case 0: // parse error, but emit it anyway
				default:
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.Data);

			return EmitDataToken (DecodeCharacterReferences, false);
		}

		// 8.2.4.2 Character reference in data state
		HtmlToken ReadCharacterReferenceInData ()
		{
			return ReadCharacterReference (HtmlTokenizerState.Data);
		}

		// 8.2.4.3 RCDATA state
		HtmlToken ReadRcData ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				switch (c) {
				case '&':
					if (DecodeCharacterReferences) {
						TokenizerState = HtmlTokenizerState.CharacterReferenceInRcData;
						return null;
					}

					goto default;
				case '<':
					TokenizerState = HtmlTokenizerState.RcDataLessThan;
					return EmitDataToken (DecodeCharacterReferences, false);
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.RcData);

			return EmitDataToken (DecodeCharacterReferences, false);
		}

		// 8.2.4.4 Character reference in RCDATA state
		HtmlToken ReadCharacterReferenceInRcData ()
		{
			return ReadCharacterReference (HtmlTokenizerState.RcData);
		}

		// 8.2.4.5 RAWTEXT state
		HtmlToken ReadRawText ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				switch (c) {
				case '<':
					TokenizerState = HtmlTokenizerState.RawTextLessThan;
					return EmitDataToken (false, false);
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.RawText);

			return EmitDataToken (false, false);
		}

		// 8.2.4.6 Script data state
		HtmlToken ReadScriptData ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				switch (c) {
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataLessThan;
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptData);

			return EmitScriptDataToken ();
		}

		// 8.2.4.7 PLAINTEXT state
		HtmlToken ReadPlainText ()
		{
			do {
				while (bufferIndex < bufferEnd) {
					char c = buffer[bufferIndex++];

					LinePosition++;

					switch (c) {
					case '\0':
						data.Append ('\uFFFD');
						break;
					case '\n':
						IncrementLineNumber ();
						goto default;
					default:
						data.Append (c);
						break;
					}
				}

				FillBuffer ();
			} while (!eof);

			TokenizerState = HtmlTokenizerState.EndOfFile;

			return EmitDataToken (false, false);
		}

		// 8.2.4.8 Tag open state
		HtmlToken ReadTagOpen ()
		{
			if (!TryRead (out char c)) {
				var token = IgnoreTruncatedTags ? null : CreateDataToken ("<");
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return token;
			}

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append ('<');
			data.Append (c);

			switch (c) {
			case '!':
				TokenizerState = HtmlTokenizerState.MarkupDeclarationOpen;
				break;
			case '?':
				TokenizerState = HtmlTokenizerState.BogusComment;
				data.Length = 1;
				data[0] = c;
				break;
			case '/':
				TokenizerState = HtmlTokenizerState.EndTagOpen;
				break;
			default:
				if (IsAsciiLetter (c)) {
					TokenizerState = HtmlTokenizerState.TagName;
					isEndTag = false;
					name.Append (c);
				} else {
					TokenizerState = HtmlTokenizerState.Data;
				}
				break;
			}

			return null;
		}

		// 8.2.4.9 End tag open state
		HtmlToken ReadEndTagOpen ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitDataToken (false, true);
			}

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				data.Length = 0; // FIXME: this is probably wrong
				break;
			default:
				if (IsAsciiLetter (c)) {
					TokenizerState = HtmlTokenizerState.TagName;
					isEndTag = true;
					name.Append (c);
				} else {
					TokenizerState = HtmlTokenizerState.BogusComment;
					data.Length = 1;
					data[0] = c;
				}
				break;
			}

			return null;
		}

		// 8.2.4.10 Tag name state
		HtmlToken ReadTagName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.BeforeAttributeName;
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					break;
				case '>':
					tag = CreateTagToken (name.ToString (), isEndTag);
					data.Length = 0;
					name.Length = 0;

					return EmitTagToken ();
				default:
					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.TagName);

			tag = CreateTagToken (name.ToString (), isEndTag);
			name.Length = 0;

			return null;
		}

		// 8.2.4.11 RCDATA less-than sign state
		HtmlToken ReadRcDataLessThan ()
		{
			return ReadGenericRawTextLessThan (HtmlTokenizerState.RcData, HtmlTokenizerState.RcDataEndTagOpen);
		}

		// 8.2.4.12 RCDATA end tag open state
		HtmlToken ReadRcDataEndTagOpen ()
		{
			return ReadGenericRawTextEndTagOpen (DecodeCharacterReferences, HtmlTokenizerState.RcData, HtmlTokenizerState.RcDataEndTagName);
		}

		// 8.2.4.13 RCDATA end tag name state
		HtmlToken ReadRcDataEndTagName ()
		{
			return ReadGenericRawTextEndTagName (DecodeCharacterReferences, HtmlTokenizerState.RcData);
		}

		// 8.2.4.14 RAWTEXT less-than sign state
		HtmlToken ReadRawTextLessThan ()
		{
			return ReadGenericRawTextLessThan (HtmlTokenizerState.RawText, HtmlTokenizerState.RawTextEndTagOpen);
		}

		// 8.2.4.15 RAWTEXT end tag open state
		HtmlToken ReadRawTextEndTagOpen ()
		{
			return ReadGenericRawTextEndTagOpen (false, HtmlTokenizerState.RawText, HtmlTokenizerState.RawTextEndTagName);
		}

		// 8.2.4.16 RAWTEXT end tag name state
		HtmlToken ReadRawTextEndTagName ()
		{
			return ReadGenericRawTextEndTagName (false, HtmlTokenizerState.RawText);
		}

		// 8.2.4.17 Script data less-than sign state
		HtmlToken ReadScriptDataLessThan ()
		{
			data.Append ('<');

			if (TryPeek (out char c) && c == '/') {
				TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
				ConsumeCharacter (c);
				data.Append ('/');
				name.Length = 0;
			} else if (c == '!') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapeStart;
				ConsumeCharacter (c);
				data.Append ('!');
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			return null;
		}

		// 8.2.4.18 Script data end tag open state
		HtmlToken ReadScriptDataEndTagOpen ()
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitScriptDataToken ();
			}

			if (c == 'S' || c == 's') {
				TokenizerState = HtmlTokenizerState.ScriptDataEndTagName;
				ConsumeCharacter (c);
				name.Append ('s');
				data.Append (c);
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			return null;
		}

		// 8.2.4.19 Script data end tag name state
		HtmlToken ReadScriptDataEndTagName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitScriptDataToken ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					if (NameIs ("script")) {
						TokenizerState = HtmlTokenizerState.BeforeAttributeName;
						break;
					}
					goto default;
				case '/':
					if (NameIs ("script")) {
						TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
						break;
					}
					goto default;
				case '>':
					if (NameIs ("script")) {
						var token = CreateTagToken (name.ToString (), true);
						TokenizerState = HtmlTokenizerState.Data;
						data.Length = 0;
						name.Length = 0;
						return token;
					}
					goto default;
				default:
					if (!IsAsciiLetter (c)) {
						TokenizerState = HtmlTokenizerState.ScriptData;
						name.Length = 0;
						return null;
					}

					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEndTagName);

			tag = CreateTagToken (name.ToString (), true);
			name.Length = 0;

			return null;
		}

		// 8.2.4.20 Script data escape start state
		HtmlToken ReadScriptDataEscapeStart ()
		{
			if (TryPeek (out char c) && c == '-') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapeStartDash;
				ConsumeCharacter (c);
				data.Append ('-');
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			return null;
		}

		// 8.2.4.21 Script data escape start dash state
		HtmlToken ReadScriptDataEscapeStartDash ()
		{
			if (TryPeek (out char c) && c == '-') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
				ConsumeCharacter (c);
				data.Append ('-');
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			return null;
		}

		// 8.2.4.22 Script data escaped state
		HtmlToken ReadScriptDataEscaped ()
		{
			HtmlToken token = null;

			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitScriptDataToken ();
				}

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
					token = EmitScriptDataToken ();
					data.Append ('<');
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			return token;
		}

		// 8.2.4.23 Script data escaped dash state
		HtmlToken ReadScriptDataEscapedDash ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitScriptDataToken ();
			}

			HtmlToken token = null;

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
				data.Append ('-');
				break;
			case '<':
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
				token = EmitScriptDataToken ();
				data.Append ('<');
				break;
			default:
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
				data.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return token;
		}

		// 8.2.4.24 Script data escaped dash dash state
		HtmlToken ReadScriptDataEscapedDashDash ()
		{
			HtmlToken token = null;

			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitScriptDataToken ();
				}

				switch (c) {
				case '-':
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
					token = EmitScriptDataToken ();
					data.Append ('<');
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.ScriptData;
					data.Append ('>');
					break;
				default:
					TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedDashDash);

			return token;
		}

		// 8.2.4.25 Script data escaped less-than sign state
		HtmlToken ReadScriptDataEscapedLessThan ()
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
				return null;
			}

			if (c == '/') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagOpen;
				ConsumeCharacter (c);
				data.Append (c);
				name.Length = 0;
			} else if (IsAsciiLetter (c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapeStart;
				ConsumeCharacter (c);
				data.Append (c);
				name.Append (c);
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
			}

			return null;
		}

		// 8.2.4.26 Script data escaped end tag open state
		HtmlToken ReadScriptDataEscapedEndTagOpen ()
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitScriptDataToken ();
			}

			if (IsAsciiLetter (c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagName;
				ConsumeCharacter (c);
				data.Append (c);
				name.Append (c);
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
			}

			return null;
		}

		// 8.2.4.27 Script data escaped end tag name state
		HtmlToken ReadScriptDataEscapedEndTagName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitScriptDataToken ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					if (NameIs ("script")) {
						TokenizerState = HtmlTokenizerState.BeforeAttributeName;
						break;
					}

					goto default;
				case '/':
					if (NameIs ("script")) {
						TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
						break;
					}
					goto default;
				case '>':
					if (NameIs ("script")) {
						var token = CreateTagToken (name.ToString (), true);
						TokenizerState = HtmlTokenizerState.Data;
						data.Length = 0;
						name.Length = 0;
						return token;
					}
					goto default;
				default:
					if (!IsAsciiLetter (c)) {
						TokenizerState = HtmlTokenizerState.ScriptData;
						return null;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedEndTagName);

			tag = CreateTagToken (name.ToString (), true);
			name.Length = 0;

			return null;
		}

		// 8.2.4.28 Script data double escape start state
		HtmlToken ReadScriptDataDoubleEscapeStart ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitScriptDataToken ();
				}

				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ': case '/': case '>':
					if (NameIs ("script"))
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					else
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					name.Length = 0;
					break;
				default:
					if (!IsAsciiLetter (c))
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					else
						name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeStart);

			return null;
		}

		// 8.2.4.29 Script data double escaped state
		HtmlToken ReadScriptDataDoubleEscaped ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitScriptDataToken ();
				}

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDash;
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
					data.Append ('<');
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			return null;
		}

		// 8.2.4.30 Script data double escaped dash state
		HtmlToken ReadScriptDataDoubleEscapedDash ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitScriptDataToken ();
			}

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDashDash;
				data.Append ('-');
				break;
			case '<':
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
				data.Append ('<');
				break;
			default:
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
				data.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return null;
		}

		// 8.2.4.31 Script data double escaped dash dash state
		HtmlToken ReadScriptDataDoubleEscapedDashDash ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitScriptDataToken ();
				}

				switch (c) {
				case '-':
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
					data.Append ('<');
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.ScriptData;
					data.Append ('>');
					break;
				default:
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedDashDash);

			return null;
		}

		// 8.2.4.32 Script data double escaped less-than sign state
		HtmlToken ReadScriptDataDoubleEscapedLessThan ()
		{
			if (TryPeek (out char c) && c == '/') {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapeEnd;
				ConsumeCharacter (c);
				data.Append ('/');
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
			}

			return null;
		}

		// 8.2.4.33 Script data double escape end state
		HtmlToken ReadScriptDataDoubleEscapeEnd ()
		{
			do {
				TryPeek (out char c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ': case '/': case '>':
					if (NameIs ("script"))
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					else
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					ConsumeCharacter (c);
					data.Append (c);
					break;
				default:
					if (!IsAsciiLetter (c)) {
						// Note: EOF also hits this case.
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					} else {
						ConsumeCharacter (c);
						name.Append (c);
						data.Append (c);
					}
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeEnd);

			return null;
		}

		// 8.2.4.34 Before attribute name state
		HtmlToken ReadBeforeAttributeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					tag = null;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return null;
				case '>':
					return EmitTagToken ();
				case '"': case '\'': case '<': case '=':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeName;
					name.Append (c == '\0' ? '\uFFFD' : c);
					return null;
				}
			} while (true);
		}

		// 8.2.4.35 Attribute name state
		HtmlToken ReadAttributeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;
					tag = null;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.AfterAttributeName;
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					break;
				case '=':
					TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
					break;
				case '>':
					EmitTagAttribute ();

					return EmitTagToken ();
				default:
					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeName);

			EmitTagAttribute ();

			return null;
		}

		// 8.2.4.36 After attribute name state
		HtmlToken ReadAfterAttributeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					tag = null;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return null;
				case '=':
					TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
					return null;
				case '>':
					return EmitTagToken ();
				case '"': case '\'': case '<':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeName;
					name.Append (c == '\0' ? '\uFFFD' : c);
					return null;
				}
			} while (true);
		}

		// 8.2.4.37 Before attribute value state
		HtmlToken ReadBeforeAttributeValue ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					tag = null;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.AttributeValueQuoted;
					quote = c;
					return null;
				case '&':
					TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
					return null;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return null;
				case '>':
					return EmitTagToken ();
				case '<': case '=': case '`':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeValueUnquoted;
					name.Append (c == '\0' ? '\uFFFD' : c);
					return null;
				}
			} while (true);
		}

		// 8.2.4.38 Attribute value (double-quoted) state
		HtmlToken ReadAttributeValueQuoted ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '&':
					TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
					return null;
				case '\0':
					name.Append ('\uFFFD');
					break;
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
						quote = '\0';
						break;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeValueQuoted);

			attribute.Value = name.ToString ();
			name.Length = 0;

			return null;
		}

		// 8.2.4.40 Attribute value (unquoted) state
		HtmlToken ReadAttributeValueUnquoted ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					name.Length = 0;

					return EmitDataToken (false, true);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.BeforeAttributeName;
					break;
				case '&':
					TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
					return null;
				case '>':
					attribute.Value = name.ToString ();
					name.Length = 0;

					return EmitTagToken ();
				case '\'': case '<': case '=': case '`':
					// parse error
					goto default;
				default:
					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeValueUnquoted);

			attribute.Value = name.ToString ();
			name.Length = 0;

			return null;
		}

		// 8.2.4.41 Character reference in attribute value state
		HtmlToken ReadCharacterReferenceInAttributeValue ()
		{
			char additionalAllowedCharacter = quote == '\0' ? '>' : quote;

			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				name.Length = 0;

				return EmitDataToken (false, true);
			}

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ': case '<': case '&':
				// no character is consumed, emit '&'
				name.Append ('&');
				break;
			default:
				if (c == additionalAllowedCharacter) {
					// this is not a character reference, nothing is consumed
					name.Append ('&');
					break;
				}

				entity.Push ('&');

				while (entity.Push (c)) {
					ConsumeCharacter (c);

					if (c == ';')
						break;

					if (!TryPeek (out c)) {
						TokenizerState = HtmlTokenizerState.EndOfFile;
						data.Length--;
						data.Append (entity.GetPushedInput ());
						entity.Reset ();

						return EmitDataToken (false, true);
					}
				}

				var pushed = entity.GetPushedInput ();
				string value;

				if (c == '=' || IsAlphaNumeric (c))
					value = pushed;
				else
					value = entity.GetValue ();

				data.Length--;
				data.Append (pushed);
				name.Append (value);
				entity.Reset ();
				break;
			}

			if (quote == '\0')
				TokenizerState = HtmlTokenizerState.AttributeValueUnquoted;
			else
				TokenizerState = HtmlTokenizerState.AttributeValueQuoted;

			return null;
		}

		// 8.2.4.42 After attribute value (quoted) state
		HtmlToken ReadAfterAttributeValueQuoted ()
		{
			HtmlToken token = null;

			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitDataToken (false, true);
			}

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeAttributeName;
				ConsumeCharacter (c);
				data.Append (c);
				break;
			case '/':
				TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
				ConsumeCharacter (c);
				data.Append (c);
				break;
			case '>':
				ConsumeCharacter (c);
				token = EmitTagToken ();
				break;
			default:
				TokenizerState = HtmlTokenizerState.BeforeAttributeName;
				break;
			}

			return token;
		}

		// 8.2.4.43 Self-closing start tag state
		HtmlToken ReadSelfClosingStartTag ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitDataToken (false, true);
			}

			if (c == '>') {
				tag.IsEmptyElement = true;

				return EmitTagToken ();
			}

			// parse error
			TokenizerState = HtmlTokenizerState.BeforeAttributeName;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			return null;
		}

		// 8.2.4.44 Bogus comment state
		HtmlToken ReadBogusComment ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				if (c == '>')
					break;

				data.Append (c == '\0' ? '\uFFFD' : c);
			} while (true);

			TokenizerState = HtmlTokenizerState.Data;

			return EmitCommentToken (data, true);
		}

		// 8.2.4.45 Markup declaration open state
		HtmlToken ReadMarkupDeclarationOpen ()
		{
			int count = 0;
			char c = '\0';

			while (count < 2) {
				if (!TryPeek (out c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitDataToken (false, true);
				}

				if (c != '-')
					break;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				ConsumeCharacter (c);
				data.Append (c);
				count++;
			}

			if (count == 2) {
				// "<!--"
				TokenizerState = HtmlTokenizerState.CommentStart;
				name.Length = 0;
				return null;
			}

			if (count == 0) {
				// Check for "<!DOCTYPE " or "<![CDATA["
				if (c == 'D' || c == 'd') {
					// Note: we save the data in case we hit a parse error and have to emit a data token
					ConsumeCharacter (c);
					data.Append (c);
					name.Append (c);
					count = 1;

					while (count < 7) {
						if (!TryRead (out c)) {
							TokenizerState = HtmlTokenizerState.EndOfFile;
							return EmitDataToken (false, true);
						}

						// Note: we save the data in case we hit a parse error and have to emit a data token
						data.Append (c);
						name.Append (c);

						if (ToLower (c) != DocType[count])
							break;

						count++;
					}

					if (count == 7) {
						doctype = CreateDocTypeToken (name.ToString ());
						TokenizerState = HtmlTokenizerState.DocType;
						name.Length = 0;
						return null;
					}

					name.Length = 0;
				} else if (c == '[') {
					// Note: we save the data in case we hit a parse error and have to emit a data token
					ConsumeCharacter (c);
					data.Append (c);
					count = 1;

					while (count < 7) {
						if (!TryRead (out c)) {
							TokenizerState = HtmlTokenizerState.EndOfFile;
							return EmitDataToken (false, true);
						}

						// Note: we save the data in case we hit a parse error and have to emit a data token
						data.Append (c);

						if (c != CData[count])
							break;

						count++;
					}

					if (count == 7) {
						TokenizerState = HtmlTokenizerState.CDataSection;
						data.Length = 0;
						return null;
					}
				}
			}

			// parse error
			TokenizerState = HtmlTokenizerState.BogusComment;

			// trim the leading "<!"
			for (int i = 0; i < data.Length - 2; i++)
				data[i] = data[i + 2];
			data.Length -= 2;
			bang = true;

			return null;
		}

		// 8.2.4.46 Comment start state
		HtmlToken ReadCommentStart ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.Data;

				return EmitCommentToken (string.Empty);
			}

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentStartDash;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				return EmitCommentToken (string.Empty);
			default:
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return null;
		}

		// 8.2.4.47 Comment start dash state
		HtmlToken ReadCommentStartDash ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.Data;
				return EmitCommentToken (name);
			}

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEnd;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				return EmitCommentToken (name);
			default:
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ('-');
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return null;
		}

		// 8.2.4.48 Comment state
		HtmlToken ReadComment ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitCommentToken (name);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.CommentEndDash;
					return null;
				default:
					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (true);
		}

		// 8.2.4.49 Comment end dash state
		HtmlToken ReadCommentEndDash ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.Data;
				return EmitCommentToken (name);
			}

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEnd;
				break;
			default:
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ('-');
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return null;
		}

		// 8.2.4.50 Comment end state
		HtmlToken ReadCommentEnd ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					return EmitCommentToken (name);
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					return EmitCommentToken (name);
				case '!': // parse error
					TokenizerState = HtmlTokenizerState.CommentEndBang;
					return null;
				case '-':
					name.Append ('-');
					break;
				default:
					TokenizerState = HtmlTokenizerState.Comment;
					name.Append ("--");
					name.Append (c == '\0' ? '\uFFFD' : c);
					return null;
				}
			} while (true);
		}

		// 8.2.4.51 Comment end bang state
		HtmlToken ReadCommentEndBang ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				return EmitCommentToken (name);
			}

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEndDash;
				name.Append ("--!");
				break;
			case '>':
				TokenizerState = HtmlTokenizerState.Data;
				return EmitCommentToken (name);
			default: // parse error
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ("--!");
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			return null;
		}

		// 8.2.4.52 DOCTYPE state
		HtmlToken ReadDocType ()
		{
			if (!TryPeek (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				name.Length = 0;

				return EmitDocType ();
			}

			TokenizerState = HtmlTokenizerState.BeforeDocTypeName;

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ':
				ConsumeCharacter (c);
				data.Append (c);
				break;
			}

			return null;
		}

		// 8.2.4.53 Before DOCTYPE name state
		HtmlToken ReadBeforeDocTypeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				default:
					TokenizerState = HtmlTokenizerState.DocTypeName;
					name.Append (c == '\0' ? '\uFFFD' : c);
					return null;
				}
			} while (true);
		}

		// 8.2.4.54 DOCTYPE name state
		HtmlToken ReadDocTypeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.Name = name.ToString ();
					doctype.ForceQuirksMode = true;
					name.Length = 0;

					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.AfterDocTypeName;
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					doctype.Name = name.ToString ();
					name.Length = 0;

					return EmitDocType ();
				case '\0':
					name.Append ('\uFFFD');
					break;
				default:
					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypeName);

			doctype.Name = name.ToString ();
			name.Length = 0;

			return null;
		}

		// 8.2.4.55 After DOCTYPE name state
		HtmlToken ReadAfterDocTypeName ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					return EmitDocType ();
				default:
					name.Append (c);
					if (name.Length < 6)
						break;

					if (NameIs ("public")) {
						TokenizerState = HtmlTokenizerState.AfterDocTypePublicKeyword;
						doctype.PublicKeyword = name.ToString ();
					} else if (NameIs ("system")) {
						TokenizerState = HtmlTokenizerState.AfterDocTypeSystemKeyword;
						doctype.SystemKeyword = name.ToString ();
					} else {
						TokenizerState = HtmlTokenizerState.BogusDocType;
					}

					name.Length = 0;
					return null;
				}
			} while (true);
		}

		// 8.2.4.56 After DOCTYPE public keyword state
		HtmlToken ReadAfterDocTypePublicKeyword ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				return EmitDocType ();
			}

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeDocTypePublicIdentifier;
				break;
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
				doctype.PublicIdentifier = string.Empty;
				quote = c;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				doctype.ForceQuirksMode = true;
				return EmitDocType ();
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			return null;
		}

		// 8.2.4.57 Before DOCTYPE public identifier state
		HtmlToken ReadBeforeDocTypePublicIdentifier ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
					doctype.PublicIdentifier = string.Empty;
					quote = c;
					return null;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return null;
				}
			} while (true);
		}

		// 8.2.4.58 DOCTYPE public identifier (double-quoted) state
		HtmlToken ReadDocTypePublicIdentifierQuoted ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.PublicIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					name.Length = 0;

					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\0': // parse error
					name.Append ('\uFFFD');
					break;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.PublicIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					name.Length = 0;

					return EmitDocType ();
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterDocTypePublicIdentifier;
						quote = '\0';
						break;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypePublicIdentifierQuoted);

			doctype.PublicIdentifier = name.ToString ();
			name.Length = 0;

			return null;
		}

		// 8.2.4.60 After DOCTYPE public identifier state
		HtmlToken ReadAfterDocTypePublicIdentifier ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				return EmitDocType ();
			}

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers;
				break;
			case '>':
				TokenizerState = HtmlTokenizerState.Data;
				return EmitDocType ();
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
				doctype.SystemIdentifier = string.Empty;
				quote = c;
				break;
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			return null;
		}

		// 8.2.4.61 Between DOCTYPE public and system identifiers state
		HtmlToken ReadBetweenDocTypePublicAndSystemIdentifiers ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					return EmitDocType ();
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
					doctype.SystemIdentifier = string.Empty;
					quote = c;
					return null;
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return null;
				}
			} while (true);
		}

		// 8.2.4.62 After DOCTYPE system keyword state
		HtmlToken ReadAfterDocTypeSystemKeyword ()
		{
			if (!TryRead (out char c)) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				return EmitDocType ();
			}

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\r': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeDocTypeSystemIdentifier;
				break;
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
				doctype.SystemIdentifier = string.Empty;
				quote = c;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				doctype.ForceQuirksMode = true;
				return EmitDocType ();
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			return null;
		}

		// 8.2.4.63 Before DOCTYPE system identifier state
		HtmlToken ReadBeforeDocTypeSystemIdentifier ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
					doctype.SystemIdentifier = string.Empty;
					quote = c;
					return null;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return null;
				}
			} while (true);
		}

		// 8.2.4.64 DOCTYPE system identifier (double-quoted) state
		HtmlToken ReadDocTypeSystemIdentifierQuoted ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.SystemIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					name.Length = 0;

					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\0': // parse error
					name.Append ('\uFFFD');
					break;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.SystemIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					name.Length = 0;

					return EmitDocType ();
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterDocTypeSystemIdentifier;
						quote = '\0';
						break;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypeSystemIdentifierQuoted);

			doctype.SystemIdentifier = name.ToString ();
			name.Length = 0;

			return null;
		}

		// 8.2.4.66 After DOCTYPE system identifier state
		HtmlToken ReadAfterDocTypeSystemIdentifier ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					return EmitDocType ();
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					return null;
				}
			} while (true);
		}

		// 8.2.4.67 Bogus DOCTYPE state
		HtmlToken ReadBogusDocType ()
		{
			do {
				if (!TryRead (out char c)) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					return EmitDocType ();
				}

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				if (c == '>') {
					TokenizerState = HtmlTokenizerState.Data;
					return EmitDocType ();
				}
			} while (true);
		}

		// 8.2.4.68 CDATA section state
		HtmlToken ReadCDataSection ()
		{
			do {
				while (bufferIndex < bufferEnd) {
					char c = buffer[bufferIndex++];

					if (c == '\n') {
						IncrementLineNumber ();
					} else {
						LinePosition++;
					}

					if (cdataIndex >= 3) {
						data.Append (cdata[0]);
						cdata[0] = cdata[1];
						cdata[1] = cdata[2];
						cdata[2] = c;

						if (cdata[0] == ']' && cdata[1] == ']' && cdata[2] == '>') {
							TokenizerState = HtmlTokenizerState.Data;
							cdataIndex = 0;

							return EmitCDataToken ();
						}
					} else {
						cdata[cdataIndex++] = c;
					}
				}

				FillBuffer ();
			} while (!eof);

			TokenizerState = HtmlTokenizerState.EndOfFile;

			for (int i = 0; i < cdataIndex; i++)
				data.Append (cdata[i]);

			cdataIndex = 0;

			return EmitCDataToken ();
		}

		/// <summary>
		/// Read the next token.
		/// </summary>
		/// <remarks>
		/// Reads the next token.
		/// </remarks>
		/// <returns><c>true</c> if the next token was read; otherwise, <c>false</c>.</returns>
		/// <param name="token">The token that was read.</param>
		public bool ReadNextToken (out HtmlToken token)
		{
			do {
				switch (TokenizerState) {
				case HtmlTokenizerState.Data:
					token = ReadData ();
					break;
				case HtmlTokenizerState.CharacterReferenceInData:
					token = ReadCharacterReferenceInData ();
					break;
				case HtmlTokenizerState.RcData:
					token = ReadRcData ();
					break;
				case HtmlTokenizerState.CharacterReferenceInRcData:
					token = ReadCharacterReferenceInRcData ();
					break;
				case HtmlTokenizerState.RawText:
					token = ReadRawText ();
					break;
				case HtmlTokenizerState.ScriptData:
					token = ReadScriptData ();
					break;
				case HtmlTokenizerState.PlainText:
					token = ReadPlainText ();
					break;
				case HtmlTokenizerState.TagOpen:
					token = ReadTagOpen ();
					break;
				case HtmlTokenizerState.EndTagOpen:
					token = ReadEndTagOpen ();
					break;
				case HtmlTokenizerState.TagName:
					token = ReadTagName ();
					break;
				case HtmlTokenizerState.RcDataLessThan:
					token = ReadRcDataLessThan ();
					break;
				case HtmlTokenizerState.RcDataEndTagOpen:
					token = ReadRcDataEndTagOpen ();
					break;
				case HtmlTokenizerState.RcDataEndTagName:
					token = ReadRcDataEndTagName ();
					break;
				case HtmlTokenizerState.RawTextLessThan:
					token = ReadRawTextLessThan ();
					break;
				case HtmlTokenizerState.RawTextEndTagOpen:
					token = ReadRawTextEndTagOpen ();
					break;
				case HtmlTokenizerState.RawTextEndTagName:
					token = ReadRawTextEndTagName ();
					break;
				case HtmlTokenizerState.ScriptDataLessThan:
					token = ReadScriptDataLessThan ();
					break;
				case HtmlTokenizerState.ScriptDataEndTagOpen:
					token = ReadScriptDataEndTagOpen ();
					break;
				case HtmlTokenizerState.ScriptDataEndTagName:
					token = ReadScriptDataEndTagName ();
					break;
				case HtmlTokenizerState.ScriptDataEscapeStart:
					token = ReadScriptDataEscapeStart ();
					break;
				case HtmlTokenizerState.ScriptDataEscapeStartDash:
					token = ReadScriptDataEscapeStartDash ();
					break;
				case HtmlTokenizerState.ScriptDataEscaped:
					token = ReadScriptDataEscaped ();
					break;
				case HtmlTokenizerState.ScriptDataEscapedDash:
					token = ReadScriptDataEscapedDash ();
					break;
				case HtmlTokenizerState.ScriptDataEscapedDashDash:
					token = ReadScriptDataEscapedDashDash ();
					break;
				case HtmlTokenizerState.ScriptDataEscapedLessThan:
					token = ReadScriptDataEscapedLessThan ();
					break;
				case HtmlTokenizerState.ScriptDataEscapedEndTagOpen:
					token = ReadScriptDataEscapedEndTagOpen ();
					break;
				case HtmlTokenizerState.ScriptDataEscapedEndTagName:
					token = ReadScriptDataEscapedEndTagName ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapeStart:
					token = ReadScriptDataDoubleEscapeStart ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscaped:
					token = ReadScriptDataDoubleEscaped ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedDash:
					token = ReadScriptDataDoubleEscapedDash ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedDashDash:
					token = ReadScriptDataDoubleEscapedDashDash ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedLessThan:
					token = ReadScriptDataDoubleEscapedLessThan ();
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapeEnd:
					token = ReadScriptDataDoubleEscapeEnd ();
					break;
				case HtmlTokenizerState.BeforeAttributeName:
					token = ReadBeforeAttributeName ();
					break;
				case HtmlTokenizerState.AttributeName:
					token = ReadAttributeName ();
					break;
				case HtmlTokenizerState.AfterAttributeName:
					token = ReadAfterAttributeName ();
					break;
				case HtmlTokenizerState.BeforeAttributeValue:
					token = ReadBeforeAttributeValue ();
					break;
				case HtmlTokenizerState.AttributeValueQuoted:
					token = ReadAttributeValueQuoted ();
					break;
				case HtmlTokenizerState.AttributeValueUnquoted:
					token = ReadAttributeValueUnquoted ();
					break;
				case HtmlTokenizerState.CharacterReferenceInAttributeValue:
					token = ReadCharacterReferenceInAttributeValue ();
					break;
				case HtmlTokenizerState.AfterAttributeValueQuoted:
					token = ReadAfterAttributeValueQuoted ();
					break;
				case HtmlTokenizerState.SelfClosingStartTag:
					token = ReadSelfClosingStartTag ();
					break;
				case HtmlTokenizerState.BogusComment:
					token = ReadBogusComment ();
					break;
				case HtmlTokenizerState.MarkupDeclarationOpen:
					token = ReadMarkupDeclarationOpen ();
					break;
				case HtmlTokenizerState.CommentStart:
					token = ReadCommentStart ();
					break;
				case HtmlTokenizerState.CommentStartDash:
					token = ReadCommentStartDash ();
					break;
				case HtmlTokenizerState.Comment:
					token = ReadComment ();
					break;
				case HtmlTokenizerState.CommentEndDash:
					token = ReadCommentEndDash ();
					break;
				case HtmlTokenizerState.CommentEnd:
					token = ReadCommentEnd ();
					break;
				case HtmlTokenizerState.CommentEndBang:
					token = ReadCommentEndBang ();
					break;
				case HtmlTokenizerState.DocType:
					token = ReadDocType ();
					break;
				case HtmlTokenizerState.BeforeDocTypeName:
					token = ReadBeforeDocTypeName ();
					break;
				case HtmlTokenizerState.DocTypeName:
					token = ReadDocTypeName ();
					break;
				case HtmlTokenizerState.AfterDocTypeName:
					token = ReadAfterDocTypeName ();
					break;
				case HtmlTokenizerState.AfterDocTypePublicKeyword:
					token = ReadAfterDocTypePublicKeyword ();
					break;
				case HtmlTokenizerState.BeforeDocTypePublicIdentifier:
					token = ReadBeforeDocTypePublicIdentifier ();
					break;
				case HtmlTokenizerState.DocTypePublicIdentifierQuoted:
					token = ReadDocTypePublicIdentifierQuoted ();
					break;
				case HtmlTokenizerState.AfterDocTypePublicIdentifier:
					token = ReadAfterDocTypePublicIdentifier ();
					break;
				case HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers:
					token = ReadBetweenDocTypePublicAndSystemIdentifiers ();
					break;
				case HtmlTokenizerState.AfterDocTypeSystemKeyword:
					token = ReadAfterDocTypeSystemKeyword ();
					break;
				case HtmlTokenizerState.BeforeDocTypeSystemIdentifier:
					token = ReadBeforeDocTypeSystemIdentifier ();
					break;
				case HtmlTokenizerState.DocTypeSystemIdentifierQuoted:
					token = ReadDocTypeSystemIdentifierQuoted ();
					break;
				case HtmlTokenizerState.AfterDocTypeSystemIdentifier:
					token = ReadAfterDocTypeSystemIdentifier ();
					break;
				case HtmlTokenizerState.BogusDocType:
					token = ReadBogusDocType ();
					break;
				case HtmlTokenizerState.CDataSection:
					token = ReadCDataSection ();
					break;
				case HtmlTokenizerState.EndOfFile:
				default:
					token = null;
					return false;
				}
			} while (token is null);

			return true;
		}
	}
}
