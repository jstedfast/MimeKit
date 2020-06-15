//
// HtmlUtils.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Globalization;

namespace MimeKit.Text {
	/// <summary>
	/// A collection of HTML-related utility methods.
	/// </summary>
	/// <remarks>
	/// A collection of HTML-related utility methods.
	/// </remarks>
	public static class HtmlUtils
	{
		internal static bool IsValidTokenName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;

			for (int i = 0; i < name.Length; i++) {
				switch (name[i]) {
				case '\t': case '\r': case '\n': case '\f': case ' ':
				case '<': case '>': case '\'': case '"':
				case '/': case '=':
					return false;
				}
			}

			return true;
		}

		static int IndexOfHtmlEncodeAttributeChar (ICharArray value, int startIndex, int endIndex, char quote)
		{
			for (int i = startIndex; i < endIndex; i++) {
				char c = value[i];

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': break;
				case '&': case '<': case '>':
					return i;
				default:
					if (c == quote || c < 32 || c >= 127)
						return i;
					break;
				}
			}

			return endIndex;
		}

		static void HtmlAttributeEncode (TextWriter output, ICharArray value, int startIndex, int count, char quote = '"')
		{
			int endIndex = startIndex + count;
			int index;

			index = IndexOfHtmlEncodeAttributeChar (value, startIndex, endIndex, quote);

			output.Write (quote);

			if (index == endIndex) {
				value.Write (output, startIndex, count);
				output.Write (quote);
				return;
			}

			if (index > startIndex)
				value.Write (output, startIndex, index - startIndex);

			while (index < endIndex) {
				char c = value[index++];
				int unichar;

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': output.Write (c); break;
				case '\'': output.Write (c == quote ? "&#39;" : "'"); break;
				case '"': output.Write (c == quote ? "&quot;" : "\""); break;
				case '&': output.Write ("&amp;"); break;
				case '<': output.Write ("&lt;"); break;
				case '>': output.Write ("&gt;"); break;
				default:
					if (c < 32 || (c >= 127 && c < 160)) {
						// illegal control character
						break;
					}

					if (c > 255 && char.IsSurrogate (c)) {
						if (index < endIndex && char.IsSurrogatePair (c, value[index])) {
							unichar = char.ConvertToUtf32 (c, value[index]);
							index++;
						} else {
							unichar = c;
						}
					} else if (c >= 160) {
						// 160-255 and non-surrogates
						unichar = c;
					} else {
						// SPACE and other printable (safe) ASCII
						output.Write (c);
						break;
					}

					output.Write (string.Format (CultureInfo.InvariantCulture, "&#{0};", unichar));
					break;
				}
			}

			output.Write (quote);
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="startIndex">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static void HtmlAttributeEncode (TextWriter output, char[] value, int startIndex, int count, char quote = '"')
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, new CharArray (value), startIndex, count, quote);
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <returns>The encoded attribute value.</returns>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="startIndex">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static string HtmlAttributeEncode (char[] value, int startIndex, int count, char quote = '"')
		{
			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			var encoded = new StringBuilder ();

			using (var output = new StringWriter (encoded))
				HtmlAttributeEncode (output, new CharArray (value), startIndex, count, quote);

			return encoded.ToString ();
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="startIndex">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static void HtmlAttributeEncode (TextWriter output, string value, int startIndex, int count, char quote = '"')
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, new CharString (value), startIndex, count, quote);
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static void HtmlAttributeEncode (TextWriter output, string value, char quote = '"')
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, new CharString (value), 0, value.Length, quote);
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <returns>The encoded attribute value.</returns>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="startIndex">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static string HtmlAttributeEncode (string value, int startIndex, int count, char quote = '"')
		{
			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			var encoded = new StringBuilder ();

			using (var output = new StringWriter (encoded))
				HtmlAttributeEncode (output, new CharString (value), startIndex, count, quote);

			return encoded.ToString ();
		}

		/// <summary>
		/// Encode an HTML attribute value.
		/// </summary>
		/// <remarks>
		/// Encodes an HTML attribute value.
		/// </remarks>
		/// <returns>The encoded attribute value.</returns>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static string HtmlAttributeEncode (string value, char quote = '"')
		{
			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			var encoded = new StringBuilder ();

			using (var output = new StringWriter (encoded))
				HtmlAttributeEncode (output, new CharString (value), 0, value.Length, quote);

			return encoded.ToString ();
		}

		static int IndexOfHtmlEncodeChar (ICharArray value, int startIndex, int endIndex)
		{
			for (int i = startIndex; i < endIndex; i++) {
				char c = value[i];

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': break;
				case '\'': case '"': case '&': case '<': case '>':
					return i;
				default:
					if (c < 32 || c >= 127)
						return i;
					break;
				}
			}

			return endIndex;
		}

		static void HtmlEncode (TextWriter output, ICharArray data, int startIndex, int count)
		{
			int endIndex = startIndex + count;
			int index;

			index = IndexOfHtmlEncodeChar (data, startIndex, endIndex);

			if (index > startIndex)
				data.Write (output, startIndex, index - startIndex);

			while (index < endIndex) {
				char c = data[index++];
				int unichar;

				switch (c) {
				case '\t': case '\r': case '\n': case '\f': output.Write (c); break;
				case '\'': output.Write ("&#39;"); break;
				case '"': output.Write ("&quot;"); break;
				case '&': output.Write ("&amp;"); break;
				case '<': output.Write ("&lt;"); break;
				case '>': output.Write ("&gt;"); break;
				default:
					if (c < 32 || (c >= 127 && c < 160)) {
						// illegal control character
						break;
					}

					if (c > 255 && char.IsSurrogate (c)) {
						if (index < endIndex && char.IsSurrogatePair (c, data[index])) {
							unichar = char.ConvertToUtf32 (c, data[index]);
							index++;
						} else {
							unichar = c;
						}
					} else if (c >= 160) {
						// 160-255 and non-surrogates
						unichar = c;
					} else {
						// SPACE and other printable (safe) ASCII
						output.Write (c);
						break;
					}

					output.Write (string.Format (CultureInfo.InvariantCulture, "&#{0};", unichar));
					break;
				}
			}
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="data">The character data to encode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static void HtmlEncode (TextWriter output, char[] data, int startIndex, int count)
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			HtmlEncode (output, new CharArray (data), startIndex, count);
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>THe encoded character data.</returns>
		/// <param name="data">The character data to encode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static string HtmlEncode (char[] data, int startIndex, int count)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			var encoded = new StringBuilder ();

			using (var output = new StringWriter (encoded))
				HtmlEncode (output, new CharArray (data), startIndex, count);

			return encoded.ToString ();
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="data">The character data to encode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static void HtmlEncode (TextWriter output, string data, int startIndex, int count)
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			HtmlEncode (output, new CharString (data), startIndex, count);
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="data">The character data to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		public static void HtmlEncode (TextWriter output, string data)
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (data == null)
				throw new ArgumentNullException (nameof (data));

			HtmlEncode (output, new CharString (data), 0, data.Length);
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>THe encoded character data.</returns>
		/// <param name="data">The character data to encode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static string HtmlEncode (string data, int startIndex, int count)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			using (var output = new StringWriter ()) {
				HtmlEncode (output, new CharString (data), startIndex, count);
				return output.ToString ();
			}
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>THe encoded character data.</returns>
		/// <param name="data">The character data to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public static string HtmlEncode (string data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var output = new StringWriter ()) {
				HtmlEncode (output, new CharString (data), 0, data.Length);
				return output.ToString ();
			}
		}

		/// <summary>
		/// Decode HTML character data.
		/// </summary>
		/// <remarks>
		/// Decodes HTML character data.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="data">The character data to decode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static void HtmlDecode (TextWriter output, string data, int startIndex, int count)
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			var entity = new HtmlEntityDecoder ();
			int endIndex = startIndex + count;
			int index = startIndex;

			while (index < endIndex) {
				if (data[index] == '&') {
					while (index < endIndex && entity.Push (data[index]))
						index++;

					output.Write (entity.GetValue ());
					entity.Reset ();
				} else {
					output.Write (data[index++]);
				}
			}
		}

		/// <summary>
		/// Decode HTML character data.
		/// </summary>
		/// <remarks>
		/// Decodes HTML character data.
		/// </remarks>
		/// <param name="output">The <see cref="System.IO.TextWriter"/> to output the result.</param>
		/// <param name="data">The character data to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		public static void HtmlDecode (TextWriter output, string data)
		{
			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (data == null)
				throw new ArgumentNullException (nameof (data));

			HtmlDecode (output, data, 0, data.Length);
		}

		/// <summary>
		/// Decode HTML character data.
		/// </summary>
		/// <remarks>
		/// Decodes HTML character data.
		/// </remarks>
		/// <returns>The decoded character data.</returns>
		/// <param name="data">The character data to decode.</param>
		/// <param name="startIndex">The starting index of the character data.</param>
		/// <param name="count">The number of characters in the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the data.</para>
		/// </exception>
		public static string HtmlDecode (string data, int startIndex, int count)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			using (var output = new StringWriter ()) {
				HtmlDecode (output, data, startIndex, count);
				return output.ToString ();
			}
		}

		/// <summary>
		/// Decode HTML character data.
		/// </summary>
		/// <remarks>
		/// Decodes HTML character data.
		/// </remarks>
		/// <returns>The decoded character data.</returns>
		/// <param name="data">The character data to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public static string HtmlDecode (string data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var output = new StringWriter ()) {
				HtmlDecode (output, data, 0, data.Length);
				return output.ToString ();
			}
		}
	}
}
