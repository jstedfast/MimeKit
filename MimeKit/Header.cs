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
		byte[] rawValue;

		internal Header (string field, byte[] value)
		{
			Field = field;
			RawValue = value;
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

		static unsafe bool TryParse (byte* input, int length, out Header header)
		{
			StringBuilder field = new StringBuilder ();
			byte* inend = input + length;
			byte* inptr = input;
			byte[] value;
			int count;

			// find the end of the field name
			while (inptr < inend && IsAtom (*inptr))
				field.Append ((char) *inptr++);

			if (inptr == inend || *inptr != ':') {
				header = null;
				return false;
			}

			inptr++;

			count = length - (int) (inptr - input);
			value = new byte[count];

			for (int i = 0; i < count; i++)
				value[i] = *inptr++;

			header = new Header (field.ToString (), value);

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
					return TryParse (inptr + startIndex, length, out header);
				}
			}
		}

		public string Field {
			get; private set;
		}

		public byte[] RawValue {
			get { return rawValue; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				rawValue = value;
				Offset = null;

				OnChanged ();
			}
		}

		public string Value {
			get {
				if (textValue == null)
					textValue = Rfc2047.DecodeText (RawValue);

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

			RawValue = Rfc2047.EncodeText (encoding, textValue);
		}

		public long? Offset {
			get; private set;
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}
