//
// Header.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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
using System.Text;

namespace MimeKit {
	public sealed class Header
	{
		string textValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
		/// </summary>
		/// <param name="charset">The charset that should be used to encode the
		/// header value.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The value of the header.</param>
		public Header (Encoding charset, string field, string value)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (field == null)
				throw new ArgumentNullException ("field");

			if (field.Length == 0)
				throw new ArgumentException ("Header field names are not allowed to be empty.", "field");

			for (int i = 0; i < field.Length; i++) {
				if (field[i] > 127 || !IsAtom ((byte) field[i]))
					throw new ArgumentException ("Illegal characters in header field name.", "field");
			}

			if (value == null)
				throw new ArgumentNullException ("value");

			Field = field;

			SetValue (charset, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
		/// </summary>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The value of the header.</param>
		public Header (string field, string value) : this (Encoding.UTF8, field, value)
		{
		}

		// Note: this ctor is only used by the parser
		internal Header (string field, byte[] value)
		{
			RawValue = value;
			Field = field;
		}

		/// <summary>
		/// Gets the stream offset of the beginning of the header.
		/// 
		/// Note: This will only be set if the header was parsed from a stream.
		/// </summary>
		/// <value>The stream offset.</value>
		public long? Offset {
			get; internal set;
		}

		/// <summary>
		/// Gets the name of the header field.
		/// </summary>
		/// <value>The name of the header field.</value>
		public string Field {
			get; private set;
		}

		/// <summary>
		/// Gets the raw value of the header.
		/// </summary>
		/// <value>The raw value of the header.</value>
		public byte[] RawValue {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the header value.
		/// </summary>
		/// <value>The header value.</value>
		public string Value {
			get {
				if (textValue == null)
					textValue = Unfold (Rfc2047.DecodeText (RawValue));

				return textValue;
			}
			set {
				SetValue (Encoding.UTF8, value);
			}
		}

		/// <summary>
		/// Sets the header value using the specified charset.
		/// </summary>
		/// <param name="charset">A charset encoding.</param>
		/// <param name="value">The header value.</param>
		public void SetValue (Encoding charset, string value)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (value == null)
				throw new ArgumentNullException ("value");

			textValue = value.Trim ();

			// FIXME: fold & end in newline?
			RawValue = Rfc2047.EncodeText (charset, " " + textValue);
			Offset = null;
			OnChanged ();
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		static unsafe string Unfold (string text)
		{
			int startIndex;
			int endIndex;
			int i = 0;

			if (text == null)
				return string.Empty;

			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;

			if (i == text.Length)
				return string.Empty;

			startIndex = i;
			endIndex = i;

			while (i < text.Length) {
				if (!char.IsWhiteSpace (text[i]))
					endIndex = i;
			}

			endIndex++;

			int count = endIndex - startIndex;
			char[] chars = new char[count];

			fixed (char* outbuf = chars) {
				char* outptr = outbuf;

				for (i = startIndex; i < endIndex; i++) {
					if (text[i] != '\r' && text[i] != '\n')
						*outptr++ = text[i];
				}

				count = (int) (outptr - outbuf);
			}

			return new string (chars, 0, count);
		}

		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		static bool IsBlankOrControl (byte c)
		{
			return c.IsType (CharType.IsBlank | CharType.IsControl);
		}

		internal static unsafe bool TryParse (byte* input, int length, bool strict, out Header header)
		{
			byte* inend = input + length;
			byte* start = input;
			byte* inptr = input;

			// find the end of the field name
			if (strict) {
				while (inptr < inend && IsAtom (*inptr))
					inptr++;
			} else {
				while (inptr < inend && *inptr != (byte) ':' && !IsBlankOrControl (*inptr))
					inptr++;
			}

			if (inptr == inend || *inptr != ':') {
				header = null;
				return false;
			}

			var chars = new char[(int) (inptr - start)];
			fixed (char* outbuf = chars) {
				char* outptr = outbuf;

				while (start < inptr)
					*outptr++ = (char) *start++;
			}

			var field = new string (chars);

			inptr++;

			int count = (int) (inend - inptr);
			var value = new byte[count];

			fixed (byte *outbuf = value) {
				byte* outptr = outbuf;

				while (inptr < inend)
					*outptr++ = *inptr++;
			}

			header = new Header (field, value);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="header">The parsed header.</param>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out Header header)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			unsafe {
				fixed (byte* inptr = buffer) {
					return TryParse (inptr + startIndex, length, true, out header);
				}
			}
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="header">The parsed header.</param>
		public static bool TryParse (byte[] buffer, int startIndex, out Header header)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			unsafe {
				fixed (byte* inptr = buffer) {
					return TryParse (inptr + startIndex, buffer.Length - startIndex, true, out header);
				}
			}
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.Header"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="header">The parsed header.</param>
		public static bool TryParse (string text, out Header header)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);

			unsafe {
				fixed (byte *inptr = buffer) {
					return TryParse (inptr, buffer.Length, true, out header);
				}
			}
		}
	}
}
