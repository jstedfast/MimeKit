//
// Rfc3492.cs
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

namespace MimeKit.Utils {
	/// <summary>
	/// A platform-independent implementation of the Punycode algorithm defined in rfc3492.
	/// </summary>
	/// <remarks>
	/// <para>This only implements the raw Punycode encoding and decoding of a single label. It does not handle
	/// the ACE prefix (<c>xn--</c>) and does not apply any IDNA mapping, normalization or validation rules.</para>
	/// <para>Both encoding and decoding are quadratic in the length of the input, so callers must limit the
	/// length of the input (e.g. to the 63-octet DNS label limit) when it comes from an untrusted source.</para>
	/// </remarks>
	static class Rfc3492
	{
		// RFC 3492, Section 5: Parameter values for Punycode.
		const int Base = 36, TMin = 1, TMax = 26, Skew = 38, Damp = 700, InitialBias = 72, InitialN = 0x80;
		const int MaxCodePoint = 0x10FFFF;

		static int DecodeDigit (char c)
		{
			if (c >= 'a' && c <= 'z')
				return c - 'a';

			if (c >= 'A' && c <= 'Z')
				return c - 'A';

			if (c >= '0' && c <= '9')
				return c - '0' + 26;

			return -1;
		}

		static char EncodeDigit (int digit)
		{
			return (char) (digit < 26 ? 'a' + digit : '0' + digit - 26);
		}

		// RFC 3492, Section 6.1: Bias adaptation function.
		static int Adapt (long delta, int numPoints, bool firstTime)
		{
			int k = 0;

			delta = firstTime ? delta / Damp : delta / 2;
			delta += delta / numPoints;

			while (delta > ((Base - TMin) * TMax) / 2) {
				delta /= Base - TMin;
				k += Base;
			}

			return (int) (k + (Base - TMin + 1) * delta / (delta + Skew));
		}

		/// <summary>
		/// Decode a Punycode string into a sequence of code points.
		/// </summary>
		/// <remarks>
		/// Decodes a Punycode string (without the ACE prefix) as defined in rfc3492, Section 6.2.
		/// </remarks>
		/// <returns><see langword="true" /> if the input was successfully decoded; otherwise, <see langword="false" />
		/// if the input contains a non-ASCII character or an invalid digit, if decoding overflows or produces a
		/// value that is not a Unicode scalar value, or if the output buffer is too small.</returns>
		/// <param name="input">The Punycode-encoded input.</param>
		/// <param name="output">The output buffer for the decoded code points.</param>
		/// <param name="count">The number of code points written to the output buffer.</param>
		public static bool TryDecode (ReadOnlySpan<char> input, Span<int> output, out int count)
		{
			int index = 0;

			count = 0;

			// RFC 3492, Section 6.2: Let b = the number of input code points before the last delimiter, or 0 if
			// there is none, then copy the first b code points to the output (failing on any non-basic code point).
			// Note: Like the sample implementation in Appendix C, decoding only starts after the delimiter if b > 0.
			int delimiter = input.LastIndexOf ('-');

			if (delimiter > 0) {
				if (delimiter > output.Length)
					return false;

				while (index < delimiter) {
					if (input[index] >= 0x80)
						return false;

					output[count++] = input[index++];
				}

				index++;
			}

			int bias = InitialBias;
			int n = InitialN;
			int i = 0;

			while (index < input.Length) {
				int oldi = i;
				int w = 1;

				for (int k = Base; ; k += Base) {
					if (index >= input.Length)
						return false;

					int digit = DecodeDigit (input[index++]);

					if (digit == -1 || digit > (int.MaxValue - i) / w)
						return false;

					i += digit * w;

					int t = k <= bias ? TMin : (k >= bias + TMax ? TMax : k - bias);

					if (digit < t)
						break;

					if (w > int.MaxValue / (Base - t))
						return false;

					w *= Base - t;
				}

				bias = Adapt (i - oldi, count + 1, oldi == 0);

				if (i / (count + 1) > MaxCodePoint - n)
					return false;

				n += i / (count + 1);
				i %= count + 1;

				if (n >= 0xD800 && n <= 0xDFFF)
					return false;

				if (count == output.Length)
					return false;

				output.Slice (i, count - i).CopyTo (output.Slice (i + 1));
				output[i++] = n;
				count++;
			}

			return true;
		}

		static bool Append (Span<char> output, bool countOnly, ref int length, char c)
		{
			if (!countOnly) {
				if (length == output.Length)
					return false;

				output[length] = c;
			}

			length++;

			return true;
		}

		static int Encode (ReadOnlySpan<int> input, Span<char> output, bool countOnly)
		{
			int length = 0;
			int basic = 0;

			for (int i = 0; i < input.Length; i++) {
				if (input[i] < 0x80) {
					if (!Append (output, countOnly, ref length, (char) input[i]))
						return -1;

					basic++;
				}
			}

			if (basic > 0 && !Append (output, countOnly, ref length, '-'))
				return -1;

			int bias = InitialBias;
			int n = InitialN;
			long delta = 0;
			int h = basic;

			while (h < input.Length) {
				int m = int.MaxValue;

				for (int i = 0; i < input.Length; i++) {
					if (input[i] >= n && input[i] < m)
						m = input[i];
				}

				delta += (long) (m - n) * (h + 1);
				n = m;

				for (int i = 0; i < input.Length; i++) {
					if (input[i] < n) {
						delta++;
					} else if (input[i] == n) {
						long q = delta;

						for (int k = Base; ; k += Base) {
							int t = k <= bias ? TMin : (k >= bias + TMax ? TMax : k - bias);

							if (q < t)
								break;

							if (!Append (output, countOnly, ref length, EncodeDigit ((int) (t + (q - t) % (Base - t)))))
								return -1;

							q = (q - t) / (Base - t);
						}

						if (!Append (output, countOnly, ref length, EncodeDigit ((int) q)))
							return -1;

						bias = Adapt (delta, h + 1, h == basic);
						delta = 0;
						h++;
					}
				}

				delta++;
				n++;
			}

			return length;
		}

		/// <summary>
		/// Encode a sequence of code points using Punycode.
		/// </summary>
		/// <remarks>
		/// Encodes a sequence of code points (without adding the ACE prefix) as defined in rfc3492, Section 6.3.
		/// </remarks>
		/// <returns>The number of characters written to the output buffer or <c>-1</c> if the output buffer is too small.</returns>
		/// <param name="input">The code points to encode.</param>
		/// <param name="output">The output buffer.</param>
		public static int Encode (ReadOnlySpan<int> input, Span<char> output)
		{
			return Encode (input, output, false);
		}

		/// <summary>
		/// Get the length of the Punycode encoding of a sequence of code points.
		/// </summary>
		/// <remarks>
		/// Gets the number of characters that <see cref="Encode(ReadOnlySpan{int}, Span{char})"/> would produce
		/// (not including the ACE prefix) without actually encoding the code points.
		/// </remarks>
		/// <returns>The length of the Punycode encoding.</returns>
		/// <param name="input">The code points to encode.</param>
		public static int GetEncodedLength (ReadOnlySpan<int> input)
		{
			return Encode (input, Span<char>.Empty, true);
		}
	}
}
