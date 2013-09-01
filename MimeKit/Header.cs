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

		internal Header (string field, byte[] value)
		{
			RawValue = value;
			Field = field;
		}

		public Header (string field, string value)
		{
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

			SetValue (Encoding.UTF8, value);
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

		public string Field {
			get; private set;
		}

		public byte[] RawValue {
			get; private set;
		}

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

		public void SetValue (Encoding encoding, string value)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (value == null)
				throw new ArgumentNullException ("value");

			textValue = value.Trim ();

			// FIXME: fold & end in newline?
			RawValue = Rfc2047.EncodeText (encoding, " " + textValue);
			Offset = null;
			OnChanged ();
		}

		public long? Offset {
			get; internal set;
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
	}
}
