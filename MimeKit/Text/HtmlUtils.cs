//
// HtmlUtils.cs
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
#if NETSTANDARD2_0 || NETFRAMEWORK
		static void Write (this TextWriter writer, ReadOnlySpan<char> value)
		{
			char[] buffer = System.Buffers.ArrayPool<char>.Shared.Rent (value.Length);

			try {
				value.CopyTo (new Span<char> (buffer));
				writer.Write (buffer, 0, value.Length);
			} finally {
				System.Buffers.ArrayPool<char>.Shared.Return (buffer);
			}
		}
#endif

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

		static int IndexOfHtmlEncodeAttributeChar (ReadOnlySpan<char> value, char quote)
		{
			for (int i = 0; i < value.Length; i++) {
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

			return value.Length;
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
		public static void HtmlAttributeEncode (TextWriter output, ReadOnlySpan<char> value, char quote = '"')
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			int index = IndexOfHtmlEncodeAttributeChar (value, quote);

			output.Write (quote);

			if (index > 0)
				output.Write (value.Slice (0, index));

			while (index < value.Length) {
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
						if (index < value.Length && char.IsSurrogatePair (c, value[index])) {
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
		/// <returns>The encoded attribute value.</returns>
		/// <param name="value">The attribute value to encode.</param>
		/// <param name="quote">The character to use for quoting the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="quote"/> is not a valid quote character.
		/// </exception>
		public static string HtmlAttributeEncode (ReadOnlySpan<char> value, char quote = '"')
		{
			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			using (var output = new StringWriter ()) {
				HtmlAttributeEncode (output, value, quote);
				return output.ToString ();
			}
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, value.AsSpan (startIndex, count), quote);
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
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			return HtmlAttributeEncode (value.AsSpan (startIndex, count), quote);
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, value.AsSpan (startIndex, count), quote);
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
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (value.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			return HtmlAttributeEncode (value.AsSpan (startIndex, count), quote);
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			HtmlAttributeEncode (output, value.AsSpan (), quote);
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
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (quote != '"' && quote != '\'')
				throw new ArgumentException ("The quote character must either be '\"' or '\''.", nameof (quote));

			return HtmlAttributeEncode (value.AsSpan (), quote);
		}

		static int IndexOfHtmlEncodeChar (ReadOnlySpan<char> data)
		{
			for (int i = 0; i < data.Length; i++) {
				char c = data[i];

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

			return data.Length;
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
		public static void HtmlEncode (TextWriter output, ReadOnlySpan<char> data)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			int index = IndexOfHtmlEncodeChar (data);

			if (index > 0)
				output.Write (data.Slice (0, index));

			while (index < data.Length) {
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
						if (index < data.Length && char.IsSurrogatePair (c, data[index])) {
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
		/// <returns>The encoded character data.</returns>
		/// <param name="data">The character data to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public static string HtmlEncode (ReadOnlySpan<char> data)
		{
			using (var output = new StringWriter ()) {
				HtmlEncode (output, data);
				return output.ToString ();
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			HtmlEncode (output, data.AsSpan (startIndex, count));
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>The encoded character data.</returns>
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
			if (data is null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			return HtmlEncode (data.AsSpan (startIndex, count));
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			HtmlEncode (output, data.AsSpan (startIndex, count));
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>The encoded character data.</returns>
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
			if (data is null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (data.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));

			return HtmlEncode (data.AsSpan (startIndex, count));
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			HtmlEncode (output, data.AsSpan ());
		}

		/// <summary>
		/// Encode HTML character data.
		/// </summary>
		/// <remarks>
		/// Encodes HTML character data.
		/// </remarks>
		/// <returns>The encoded character data.</returns>
		/// <param name="data">The character data to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public static string HtmlEncode (string data)
		{
			if (data is null)
				throw new ArgumentNullException (nameof (data));

			return HtmlEncode (data.AsSpan ());
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (data is null)
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
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (data is null)
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
			if (data is null)
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
			if (data is null)
				throw new ArgumentNullException (nameof (data));

			using (var output = new StringWriter ()) {
				HtmlDecode (output, data, 0, data.Length);
				return output.ToString ();
			}
		}
	}
}
