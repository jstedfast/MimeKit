//
// Parameter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
#endif

using MimeKit.Encodings;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A header parameter as found in the Content-Type and Content-Disposition headers.
	/// </summary>
	/// <remarks>
	/// Content-Type and Content-Disposition headers often have parameters that specify
	/// further information about how to interpret the content.
	/// </remarks>
	public class Parameter
	{
		string text;

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new parameter with the specified name and value.
		/// </remarks>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="name"/> contains illegal characters.
		/// </exception>
		public Parameter (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("Parameter names are not allowed to be empty.", "name");

			for (int i = 0; i < name.Length; i++) {
				if (name[i] > 127 || !IsAttr ((byte) name[i]))
					throw new ArgumentException ("Illegal characters in parameter name.", "name");
			}

			if (value == null)
				throw new ArgumentNullException ("value");

			Name = name;
			Value = value;
		}

		static bool IsAttr (byte c)
		{
			return c.IsAttr ();
		}

		/// <summary>
		/// Gets the parameter name.
		/// </summary>
		/// <remarks>
		/// Gets the parameter name.
		/// </remarks>
		/// <value>The parameter name.</value>
		public string Name {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the parameter value.
		/// </summary>
		/// <remarks>
		/// Gets or sets the parameter value.
		/// </remarks>
		/// <value>The parameter value.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string Value {
			get { return text; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (text == value)
					return;

				text = value;
				OnChanged ();
			}
		}

		enum EncodeMethod {
			None,
			Quote,
			Rfc2184
		}

		static EncodeMethod GetEncodeMethod (FormatOptions options, string name, string value, out string quoted)
		{
			var method = EncodeMethod.None;

			quoted = null;

			if (name.Length + 1 + value.Length >= options.MaxLineLength)
				return EncodeMethod.Rfc2184;

			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 128) {
					var c = (byte) value[i];

					if (c.IsCtrl ())
						return EncodeMethod.Rfc2184;

					if (!c.IsAttr ())
						method = EncodeMethod.Quote;
				} else if (options.International) {
					method = EncodeMethod.Quote;
				} else {
					return EncodeMethod.Rfc2184;
				}
			}

			if (method == EncodeMethod.Quote) {
				quoted = MimeUtils.Quote (value);

				if (name.Length + 1 + quoted.Length >= options.MaxLineLength)
					return EncodeMethod.Rfc2184;
			}

			return method;
		}

		static EncodeMethod GetEncodeMethod (FormatOptions options, char[] value, int startIndex, int length)
		{
			var method = EncodeMethod.None;

			for (int i = startIndex; i < startIndex + length; i++) {
				if (value[i] < 128) {
					var c = (byte) value[i];

					if (c.IsCtrl ())
						return EncodeMethod.Rfc2184;

					if (!c.IsAttr ())
						method = EncodeMethod.Quote;
				} else if (options.International) {
					method = EncodeMethod.Quote;
				} else {
					return EncodeMethod.Rfc2184;
				}
			}

			return method;
		}

		static EncodeMethod GetEncodeMethod (byte[] value, int length)
		{
			var method = EncodeMethod.None;

			for (int i = 0; i < length; i++) {
				if (value[i] >= 127 || value[i].IsCtrl ())
					return EncodeMethod.Rfc2184;

				if (!value[i].IsAttr ())
					method = EncodeMethod.Quote;
			}

			return method;
		}

		static bool IsCtrl (char c)
		{
			return ((byte) c).IsCtrl ();
		}

		static Encoding GetBestEncoding (string value, Encoding defaultEncoding)
		{
			int encoding = 0; // us-ascii

			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 127) {
					if (IsCtrl (value[i]))
						encoding = Math.Max (encoding, 1);
				} else if (value[i] < 256) {
					encoding = Math.Max (encoding, 1);
				} else {
					encoding = 2;
				}
			}

			switch (encoding) {
			case 0: return Encoding.ASCII;
			case 1: return Encoding.GetEncoding (28591); // iso-8859-1
			default: return defaultEncoding;
			}
		}

		static bool GetNextValue (FormatOptions options, string charset, Encoder encoder, HexEncoder hex, char[] chars, ref int index,
		                          ref byte[] bytes, ref byte[] encoded, int maxLength, out string value)
		{
			int length = chars.Length - index;

			if (length < maxLength) {
				switch (GetEncodeMethod (options, chars, index, length)) {
				case EncodeMethod.Quote:
					value = MimeUtils.Quote (new string (chars, index, length));
					index += length;
					return false;
				case EncodeMethod.None:
					value = new string (chars, index, length);
					index += length;
					return false;
				}
			}

			length = Math.Min (maxLength, length);
			int ratio, count, n;

			do {
				count = encoder.GetByteCount (chars, index, length, true);
				if (count > maxLength && length > 1) {
					if ((ratio = (int) Math.Round ((double) count / (double) length)) > 1)
						length -= Math.Max ((count - maxLength) / ratio, 1);
					else
						length--;
					continue;
				}

				if (bytes.Length < count)
					Array.Resize<byte> (ref bytes, count);

				count = encoder.GetBytes (chars, index, length, bytes, 0, true);

				// Note: the first chunk needs to be encoded in order to declare the charset
				if (index > 0 || charset == "us-ascii") {
					var method = GetEncodeMethod (bytes, count);

					if (method == EncodeMethod.Quote) {
						value = MimeUtils.Quote (Encoding.ASCII.GetString (bytes, 0, count));
						index += length;
						return false;
					}

					if (method == EncodeMethod.None) {
						value = Encoding.ASCII.GetString (bytes, 0, count);
						index += length;
						return false;
					}
				}

				n = hex.EstimateOutputLength (count);
				if (encoded.Length < n)
					Array.Resize<byte> (ref encoded, n);

				// only the first value gets a charset declaration
				int charsetLength = index == 0 ? charset.Length + 2 : 0;

				n = hex.Encode (bytes, 0, count, encoded);
				if (n > 3 && (charsetLength + n) > maxLength) {
					int x = 0;

					for (int i = n - 1; i >= 0 && charsetLength + i >= maxLength; i--) {
						if (encoded[i] == (byte) '%')
							x--;
						else
							x++;
					}

					if ((ratio = (int) Math.Round ((double) count / (double) length)) > 1)
						length -= Math.Max (x / ratio, 1);
					else
						length--;
					continue;
				}

				if (index == 0)
					value = charset + "''" + Encoding.ASCII.GetString (encoded, 0, n);
				else
					value = Encoding.ASCII.GetString (encoded, 0, n);
				index += length;
				return true;
			} while (true);
		}

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength, Encoding encoding)
		{
			string quoted;

			var method = GetEncodeMethod (options, Name, Value, out quoted);
			if (method == EncodeMethod.None)
				quoted = Value;

			if (method != EncodeMethod.Rfc2184) {
				builder.Append (';');
				lineLength++;

				if (lineLength + 1 + Name.Length + 1 + quoted.Length >= options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				lineLength += Name.Length + 1 + quoted.Length;
				builder.Append (Name);
				builder.Append ('=');
				builder.Append (quoted);
				return;
			}

			var bestEncoding = options.International ? Encoding.UTF8 : GetBestEncoding (Value, encoding);
			int maxLength = options.MaxLineLength - (Name.Length + 6);
			var charset = CharsetUtils.GetMimeCharset (bestEncoding);
			var encoder = (Encoder) bestEncoding.GetEncoder ();
			var bytes = new byte[Math.Max (maxLength, 6)];
			var hexbuf = new byte[bytes.Length * 3 + 3];
			var chars = Value.ToCharArray ();
			var hex = new HexEncoder ();
			int index = 0, i = 0;
			string value, id;
			bool encoded;
			int length;

			do {
				builder.Append (';');
				lineLength++;

				encoded = GetNextValue (options, charset, encoder, hex, chars, ref index, ref bytes, ref hexbuf, maxLength, out value);
				length = Name.Length + (encoded ? 1 : 0) + 1 + value.Length;

				if (i == 0 && index == chars.Length) {
					if (lineLength + 1 + length >= options.MaxLineLength) {
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						builder.Append (' ');
						lineLength++;
					}

					builder.Append (Name);
					if (encoded)
						builder.Append ('*');
					builder.Append ('=');
					builder.Append (value);
					lineLength += length;
					return;
				}

				builder.Append (options.NewLine);
				builder.Append ('\t');
				lineLength = 1;

				id = i.ToString ();
				length += id.Length + 1;

				builder.Append (Name);
				builder.Append ('*');
				builder.Append (id);
				if (encoded)
					builder.Append ('*');
				builder.Append ('=');
				builder.Append (value);
				lineLength += length;
				i++;
			} while (index < chars.Length);
		}

		/// <summary>
		/// Returns a string representation of the <see cref="Parameter"/>.
		/// </summary>
		/// <remarks>
		/// Formats the parameter name and value in the form <c>name="value"</c>.
		/// </remarks>
		/// <returns>A string representation of the <see cref="Parameter"/>.</returns>
		public override string ToString ()
		{
			return Name + "=" + MimeUtils.Quote (Value);
		}

		internal event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}
