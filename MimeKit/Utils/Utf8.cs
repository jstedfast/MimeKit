//
// Utf8.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Runtime.CompilerServices;

namespace MimeKit.Utils {
	static class Utf8
	{
		/// <summary>
		/// Validate that the value is well-formed UTF-8.
		/// </summary>
		/// <remarks>
		/// Validates that the value is well-formed UTF-8.
		/// </remarks>
		/// <param name="value">The byte string.</param>
		/// <returns><see langword="true"/> if the value is well-formed UTF-8; otherwise, <see langword="false"/>.</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static bool IsValid (ReadOnlySpan<byte> value)
		{
#if NET8_0_OR_GREATER
			return System.Text.Unicode.Utf8.IsValid (value);
#else
			return IsValidCore (value);
#endif
		}

		/// <summary>
		/// Validate that the value is well-formed UTF-8.
		/// </summary>
		/// <remarks>
		/// <para>Validates that the value is well-formed UTF-8 as defined by Table 3-7 ("Well-Formed UTF-8
		/// Byte Sequences") of the Unicode Standard.</para>
		/// <para>This is the managed fallback used on target frameworks that do not provide
		/// <c>System.Text.Unicode.Utf8.IsValid()</c>. It is compiled on all target frameworks so that it
		/// can be unit tested (and compared against the framework implementation) everywhere.</para>
		/// <note type="note">Overlong encodings, UTF-16 surrogate values (<c>U+D800</c> through <c>U+DFFF</c>)
		/// and scalar values above <c>U+10FFFF</c> are all rejected. This matters because such encodings are
		/// exactly the sort of thing used to smuggle content past scanners that decode them leniently.</note>
		/// </remarks>
		/// <param name="value">The byte string.</param>
		/// <returns><see langword="true"/> if the value is well-formed UTF-8; otherwise, <see langword="false"/>.</returns>
		internal static bool IsValidCore (ReadOnlySpan<byte> value)
		{
			int index = 0;

			while (index < value.Length) {
				byte b0 = value[index];

				if (b0 < 0x80) {
					// 1-byte sequence: U+0000 -> U+007F
					index++;
					continue;
				}

				byte lower, upper;
				int length;

				if (b0 < 0xC2) {
					// 0x80 -> 0xBF are continuation bytes (which cannot start a sequence) and
					// 0xC0 -> 0xC1 could only ever be the start of an overlong encoding.
					return false;
				} else if (b0 < 0xE0) {
					// 2-byte sequence: U+0080 -> U+07FF
					length = 2;
					lower = 0x80;
					upper = 0xBF;
				} else if (b0 < 0xF0) {
					// 3-byte sequence: U+0800 -> U+FFFF
					length = 3;

					if (b0 == 0xE0) {
						// Exclude overlong encodings of U+0000 -> U+07FF.
						lower = 0xA0;
						upper = 0xBF;
					} else if (b0 == 0xED) {
						// Exclude the UTF-16 surrogate range, U+D800 -> U+DFFF.
						lower = 0x80;
						upper = 0x9F;
					} else {
						lower = 0x80;
						upper = 0xBF;
					}
				} else if (b0 < 0xF5) {
					// 4-byte sequence: U+10000 -> U+10FFFF
					length = 4;

					if (b0 == 0xF0) {
						// Exclude overlong encodings of U+0000 -> U+FFFF.
						lower = 0x90;
						upper = 0xBF;
					} else if (b0 == 0xF4) {
						// Exclude scalar values above U+10FFFF.
						lower = 0x80;
						upper = 0x8F;
					} else {
						lower = 0x80;
						upper = 0xBF;
					}
				} else {
					// 0xF5 -> 0xFF could only ever encode a scalar value above U+10FFFF.
					return false;
				}

				// Make sure the sequence isn't truncated.
				if (value.Length - index < length)
					return false;

				// The range of valid values for the first continuation byte depends on the leading byte.
				byte b1 = value[index + 1];

				if (b1 < lower || b1 > upper)
					return false;

				// Any remaining continuation bytes simply need to be in the range 0x80 -> 0xBF.
				for (int i = 2; i < length; i++) {
					if ((value[index + i] & 0xC0) != 0x80)
						return false;
				}

				index += length;
			}

			return true;
		}
	}
}
