//
// Parameter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc.
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
		ParameterEncodingMethod encodingMethod;
		Encoding encoding;
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
		/// <paramref name="name"/> contains illegal characters.
		/// </exception>
		public Parameter (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("Parameter names are not allowed to be empty.", nameof (name));

			for (int i = 0; i < name.Length; i++) {
				if (name[i] > 127 || !IsAttr ((byte) name[i]))
					throw new ArgumentException ("Illegal characters in parameter name.", nameof (name));
			}

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			Value = value;
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new parameter with the specified name and value.
		/// </remarks>
		/// <param name="encoding">The character encoding.</param>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> contains illegal characters.
		/// </exception>
		public Parameter (Encoding encoding, string name, string value)
		{
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("Parameter names are not allowed to be empty.", nameof (name));

			for (int i = 0; i < name.Length; i++) {
				if (name[i] > 127 || !IsAttr ((byte) name[i]))
					throw new ArgumentException ("Illegal characters in parameter name.", nameof (name));
			}

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			Encoding = encoding;
			Value = value;
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new parameter with the specified name and value.
		/// </remarks>
		/// <param name="charset">The character encoding.</param>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public Parameter (string charset, string name, string value)
		{
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("Parameter names are not allowed to be empty.", nameof (name));

			for (int i = 0; i < name.Length; i++) {
				if (name[i] > 127 || !IsAttr ((byte) name[i]))
					throw new ArgumentException ("Illegal characters in parameter name.", nameof (name));
			}

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			Encoding = CharsetUtils.GetEncoding (charset);
			Value = value;
			Name = name;
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
		/// Gets or sets the parameter value character encoding.
		/// </summary>
		/// <remarks>
		/// Gets or sets the parameter value character encoding.
		/// </remarks>
		/// <value>The character encoding.</value>
		public Encoding Encoding {
			get { return encoding ?? CharsetUtils.UTF8; }
			set {
				if (encoding == value)
					return;

				encoding = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the parameter encoding method to use.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the parameter encoding method to use.</para>
		/// <para>The MIME specifications specify that the proper method for encoding Content-Type
		/// and Content-Disposition parameter values is the method described in
		/// <a href="https://tools.ietf.org/html/rfc2231">rfc2231</a>. However, it is common for
		/// some older email clients to improperly encode using the method described in
		/// <a href="https://tools.ietf.org/html/rfc2047">rfc2047</a> instead.</para>
		/// <para>If set to <see cref="ParameterEncodingMethod.Default"/>, the encoding
		/// method used will default to the value set on the <see cref="FormatOptions"/>.</para>
		/// </remarks>
		/// <value>The encoding method.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid value.
		/// </exception>
		public ParameterEncodingMethod EncodingMethod {
			get { return encodingMethod; }
			set {
				if (encodingMethod == value)
					return;

				switch (value) {
				case ParameterEncodingMethod.Default:
				case ParameterEncodingMethod.Rfc2047:
				case ParameterEncodingMethod.Rfc2231:
					encodingMethod = value;
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				OnChanged ();
			}
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
					throw new ArgumentNullException (nameof (value));

				if (text == value)
					return;

				text = value;
				OnChanged ();
			}
		}

		static bool IsAttr (byte c)
		{
			return c.IsAttr ();
		}

		static bool IsCtrl (char c)
		{
			return ((byte) c).IsCtrl ();
		}

		enum EncodeMethod {
			None,
			Quote,
			Rfc2047,
			Rfc2231
		}

		EncodeMethod GetEncodeMethod (FormatOptions options, string name, string value, out string quoted)
		{
			var method = EncodeMethod.None;
			EncodeMethod encode;

			switch (encodingMethod) {
			default:
				if (options.ParameterEncodingMethod == ParameterEncodingMethod.Rfc2231)
					encode = EncodeMethod.Rfc2231;
				else
					encode = EncodeMethod.Rfc2047;
				break;
			case ParameterEncodingMethod.Rfc2231:
				encode = EncodeMethod.Rfc2231;
				break;
			case ParameterEncodingMethod.Rfc2047:
				encode = EncodeMethod.Rfc2047;
				break;
			}

			quoted = null;

			if (name.Length + 1 + value.Length >= options.MaxLineLength)
				return encode;

			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 128) {
					var c = (byte) value[i];

					if (c.IsCtrl ())
						return encode;

					if (!c.IsAttr ())
						method = EncodeMethod.Quote;
				} else if (options.International) {
					method = EncodeMethod.Quote;
				} else {
					return encode;
				}
			}

			if (method == EncodeMethod.Quote) {
				quoted = MimeUtils.Quote (value);

				if (name.Length + 1 + quoted.Length >= options.MaxLineLength)
					return encode;
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
						return EncodeMethod.Rfc2231;

					if (!c.IsAttr ())
						method = EncodeMethod.Quote;
				} else if (options.International) {
					method = EncodeMethod.Quote;
				} else {
					return EncodeMethod.Rfc2231;
				}
			}

			return method;
		}

		static EncodeMethod GetEncodeMethod (byte[] value, int length)
		{
			var method = EncodeMethod.None;

			for (int i = 0; i < length; i++) {
				if (value[i] >= 127 || value[i].IsCtrl ())
					return EncodeMethod.Rfc2231;

				if (!value[i].IsAttr ())
					method = EncodeMethod.Quote;
			}

			return method;
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

		static bool Rfc2231GetNextValue (FormatOptions options, string charset, Encoder encoder, HexEncoder hex, char[] chars, ref int index, ref byte[] bytes, ref byte[] encoded, int maxLength, out string value)
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

		void EncodeRfc2231 (FormatOptions options, StringBuilder builder, ref int lineLength, Encoding headerEncoding)
		{
			var bestEncoding = options.International ? CharsetUtils.UTF8 : GetBestEncoding (Value, encoding ?? headerEncoding);
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

				encoded = Rfc2231GetNextValue (options, charset, encoder, hex, chars, ref index, ref bytes, ref hexbuf, maxLength, out value);
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

		static int EstimateEncodedWordLength (string charset, int byteCount, int encodeCount)
		{
			int length = charset.Length + 7;

			if ((double) encodeCount < (byteCount * 0.17)) {
				// quoted-printable encoding
				return length + (byteCount - encodeCount) + (encodeCount * 3);
			}

			// base64 encoding
			return length + ((byteCount + 2) / 3) * 4;
		}

		static bool ExceedsMaxWordLength (string charset, int byteCount, int encodeCount, int maxLength)
		{
			int length = EstimateEncodedWordLength (charset, byteCount, encodeCount);

			return length + 1 >= maxLength;
		}

		static int Rfc2047EncodeNextChunk (StringBuilder str, string text, ref int index, Encoding encoding, string charset, Encoder encoder, int maxLength)
		{
			int byteCount = 0, charCount = 0, encodeCount = 0;
			var buffer = new char[2];
			int startIndex = index;
			int nchars, n;
			char c;

			while (index < text.Length) {
				c = text[index++];

				if (c < 127) {
					if (IsCtrl (c) || c == '"' || c == '\\')
						encodeCount++;

					byteCount++;
					charCount++;
					nchars = 1;
					n = 1;
				} else if (c < 256) {
					// iso-8859-1
					encodeCount++;
					byteCount++;
					charCount++;
					nchars = 1;
					n = 1;
				} else {
					if (char.IsSurrogatePair (text, index - 1)) {
						buffer[1] = text[index++];
						nchars = 2;
					} else {
						nchars = 1;
					}

					buffer[0] = c;

					try {
						n = encoder.GetByteCount (buffer, 0, nchars, true);
					} catch {
						n = 3;
					}

					charCount += nchars;
					encodeCount += n;
					byteCount += n;
				}

				if (ExceedsMaxWordLength (charset, byteCount, encodeCount, maxLength)) {
					// restore our previous state
					charCount -= nchars;
					index -= nchars;
					byteCount -= n;
					break;
				}
			}

			return Rfc2047.AppendEncodedWord (str, encoding, text, startIndex, charCount, QEncodeMode.Text);
		}

		void EncodeRfc2047 (FormatOptions options, StringBuilder builder, ref int lineLength, Encoding headerEncoding)
		{
			var bestEncoding = options.International ? CharsetUtils.UTF8 : GetBestEncoding (Value, encoding ?? headerEncoding);
			var charset = CharsetUtils.GetMimeCharset (bestEncoding);
			var encoder = (Encoder) bestEncoding.GetEncoder ();
			int index = 0;
			int length;

			builder.Append (';');
			lineLength++;

			// account for: <SPACE> + <NAME> + "=\"=?<CHARSET>?b?<10 chars>?=\""
			if (lineLength + Name.Length + charset.Length + 10 + Math.Min (Value.Length, 10) >= options.MaxLineLength) {
				builder.Append (options.NewLine);
				builder.Append ('\t');
				lineLength = 1;
			} else {
				builder.Append (' ');
				lineLength++;
			}

			builder.AppendFormat ("{0}=\"", Name);
			lineLength += Name.Length + 2;

			do {
				length = Rfc2047EncodeNextChunk (builder, Value, ref index, bestEncoding, charset, encoder, (options.MaxLineLength - lineLength) - 1);
				lineLength += length;

				if (index >= Value.Length)
					break;

				builder.Append (options.NewLine);
				builder.Append ('\t');
				lineLength = 1;
			} while (true);

			builder.Append ('\"');
			lineLength++;
		}

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength, Encoding headerEncoding)
		{
			string quoted;

			switch (GetEncodeMethod (options, Name, Value, out quoted)) {
			case EncodeMethod.Rfc2231:
				EncodeRfc2231 (options, builder, ref lineLength, headerEncoding);
				break;
			case EncodeMethod.Rfc2047:
				EncodeRfc2047 (options, builder, ref lineLength, headerEncoding);
				break;
			case EncodeMethod.None:
				quoted = Value;
				goto default;
			default:
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
				break;
			}
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
