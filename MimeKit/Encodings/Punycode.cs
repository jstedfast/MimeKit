//
// Punycode.cs
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
using System.Globalization;

namespace MimeKit.Encodings {
	/// <summary>
	/// A class for encoding and decoding international domain names.
	/// </summary>
	/// <remarks>
	/// A class for encoding and decoding international domain names.
	/// </remarks>
	public class Punycode : IPunycode
	{
		readonly IdnMapping idn;

		/// <summary>
		/// Initialize a new instance of the <see cref="Punycode"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new instance of <see cref="Punycode"/>.
		/// </remarks>
		public Punycode ()
		{
			idn = new IdnMapping ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Punycode"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new instance of <see cref="Punycode"/>.
		/// </remarks>
		/// <param name="idnMapping">The <see cref="IdnMapping"/> to use for encoding and decoding international domain names.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="idnMapping"/> is <c>null</c>.
		/// </exception>
		public Punycode (IdnMapping idnMapping)
		{
			if (idnMapping is null)
				throw new ArgumentNullException (nameof (idnMapping));

			idn = idnMapping;
		}

		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="unicode">The Unicode domain name.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		public string Encode (string unicode)
		{
			try {
				return idn.GetAscii (unicode);
			} catch {
				return unicode;
			}
		}

		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="unicode">The Unicode domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="unicode"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		public string Encode (string unicode, int index)
		{
			try {
				return idn.GetAscii (unicode, index);
			} catch {
				return unicode.Substring (index);
			}
		}

		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="unicode">The Unicode domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="unicode"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <param name="count">The number of characters to convert in the substring that starts
		/// at the position specified by <paramref name="index"/> in the <paramref name="unicode"/>
		/// string.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		public string Encode (string unicode, int index, int count)
		{
			try {
				return idn.GetAscii (unicode, index, count);
			} catch {
				return unicode.Substring (index, count);
			}
		}

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="ascii">The ASCII-encoded domain name.</param>
		/// <returns>The Unicode domain name.</returns>
		public string Decode (string ascii)
		{
			try {
				return idn.GetUnicode (ascii);
			} catch {
				return ascii;
			}
		}

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="ascii">The ASCII-encoded domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="ascii"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <returns>The Unicode domain name.</returns>
		public string Decode (string ascii, int index)
		{
			try {
				return idn.GetUnicode (ascii, index);
			} catch {
				return ascii.Substring (index);
			}
		}

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="ascii">The ASCII-encoded domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="ascii"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <param name="count">The number of characters to convert in the substring that starts
		/// at the position specified by <paramref name="index"/> in the <paramref name="ascii"/>
		/// string.</param>
		/// <returns>The Unicode domain name.</returns>
		public string Decode (string ascii, int index, int count)
		{
			try {
				return idn.GetUnicode (ascii, index, count);
			} catch {
				return ascii.Substring (index, count);
			}
		}
	}
}
