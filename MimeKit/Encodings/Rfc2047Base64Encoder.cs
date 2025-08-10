//
// Rfc2047Base64Encoder.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
#if NET6_0_OR_GREATER
using System.Buffers.Text;
#else
using System.Runtime.CompilerServices;
#endif

namespace MimeKit.Encodings {
	/// <summary>
	/// A base64 encoder that is specifically meant to be used for rfc2047 encoded-word tokens.
	/// </summary>
	/// <remarks>
	/// The rfc2047 "B" encoding is an encoding often used in MIME to encode textual content outside
	/// the ASCII range within an rfc2047 encoded-word token in order to ensure that the text remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	class Rfc2047Base64Encoder : IRfc2047Encoder
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="Rfc2047Base64Encoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new rfc2047 base64 encoder.
		/// </remarks>
		public Rfc2047Base64Encoder ()
		{
		}

		/// <summary>
		/// Get the rfc2047 encoding method.
		/// </summary>
		/// <remarks>
		/// <para>Gets the rfc2047 encoding method.</para>
		/// <para>Rfc2047 encoded-word tokens support two methods of encoding: base64 and quoted-printable.
		/// These encoding methods are represented by <c>b</c> (or <c>B</c>) and <c>q</c> (or <c>Q</c>),
		/// respectively.</para>
		/// </remarks>
		/// <value>The character representing the rfc2047 encoding.</value>
		public char Encoding {
			get { return 'b'; }
		}

		/// <summary>
		/// Estimate the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to encode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			return ((inputLength + 2) / 3) * 4;
		}

		void ValidateArguments (byte[] input, int startIndex, int length, byte[] output)
		{
			if (input is null)
				throw new ArgumentNullException (nameof (input));

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));

			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (output.Length < EstimateOutputLength (length))
				throw new ArgumentException ("The output buffer is not large enough to contain the encoded input.", nameof (output));
		}

#if !NET6_0_OR_GREATER // aka NETFRAMEWORK || NETSTANDARD
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int Encode (byte* input, int length, byte* output)
		{
			int remaining = length;
			byte* outptr = output;
			byte* inptr = input;
			int c1, c2, c3;

			if (length > 2) {
				byte* inend = inptr + length - 2;

				c1 = *inptr++;
				c2 = *inptr++;
				c3 = *inptr++;

				do {
					// encode our triplet into a quartet
					*outptr++ = Base64Encoder.base64_alphabet[c1 >> 2];
					*outptr++ = Base64Encoder.base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
					*outptr++ = Base64Encoder.base64_alphabet[((c2 & 0x0f) << 2) | (c3 >> 6)];
					*outptr++ = Base64Encoder.base64_alphabet[c3 & 0x3f];

					if (inptr >= inend)
						break;

					c1 = *inptr++;
					c2 = *inptr++;
					c3 = *inptr++;
				} while (true);

				remaining = 2 - (int) (inptr - inend);
			}

			if (remaining == 2) {
				c1 = *inptr++;
				c2 = *inptr++;

				*outptr++ = Base64Encoder.base64_alphabet[c1 >> 2];
				*outptr++ = Base64Encoder.base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
				*outptr++ = Base64Encoder.base64_alphabet[(c2 & 0x0f) << 2];
				*outptr++ = (byte) '=';
			} else if (remaining == 1) {
				c1 = *inptr++;
				c2 = 0;

				*outptr++ = Base64Encoder.base64_alphabet[c1 >> 2];
				*outptr++ = Base64Encoder.base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
				*outptr++ = (byte) '=';
				*outptr++ = (byte) '=';
			}

			return (int) (outptr - output);
		}
#endif

		/// <summary>
		/// Encode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Encodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// encoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">The output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="input"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="output"/> is not large enough to contain the encoded content.</para>
		/// <para>Use the <see cref="EstimateOutputLength"/> method to properly determine the 
		/// necessary length of the <paramref name="output"/> byte array.</para>
		/// </exception>
		public int Encode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

#if NET6_0_OR_GREATER
			Base64.EncodeToUtf8 (input.AsSpan (startIndex, length), output.AsSpan (), out _, out int outputLength, true);

			return outputLength;
#else
			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Encode (inptr + startIndex, length, outptr);
				}
			}
#endif
		}
	}
}
