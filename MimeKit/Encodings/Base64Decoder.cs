//
// Base64Decoder.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2022 .NET Foundation and Contributors
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

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the base64 encoding.
	/// </summary>
	/// <remarks>
	/// Base64 is an encoding often used in MIME to encode binary content such
	/// as images and other types of multi-media to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class Base64Decoder : IMimeDecoder
	{
		static ReadOnlySpan<byte> base64_rank => new byte[256] {
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255, 62,255,255,255, 63,
			 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,255,255,255,255,255,255,
			255,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
			 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,255,255,255,255,255,
			255, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
			 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
		};

		uint saved;
		byte bytes;
		bool eof;

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Decoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 decoder.
		/// </remarks>
		public Base64Decoder ()
		{
		}

		/// <summary>
		/// Clone the <see cref="Base64Decoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Base64Decoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="Base64Decoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			var decoder = new Base64Decoder ();
			decoder.saved = saved;
			decoder.bytes = bytes;
			decoder.eof = eof;

			return decoder;
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the decoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Base64; }
		}

		/// <summary>
		/// Estimate the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to decode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			if (eof)
				return 0;

			int quartets = (inputLength + bytes) / 4;

			return quartets * 3;
		}

		void ValidateArguments (byte[] input, int startIndex, int length, byte[] output)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));

			if (output == null)
				throw new ArgumentNullException (nameof (output));

			if (output.Length < EstimateOutputLength (length))
				throw new ArgumentException ("The output buffer is not large enough to contain the decoded input.", nameof (output));
		}

		/// <summary>
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// decoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">A pointer to the beginning of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">A pointer to the beginning of the output buffer.</param>
		public unsafe int Decode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			if (eof)
				return 0;

			// decode every quartet into a triplet
			while (inptr < inend) {
				byte rank = base64_rank[(c = *inptr++)];

				if (rank != 0xFF) {
					saved = (saved << 6) | rank;
					bytes++;

					if (bytes == 4) {
						// flush our decoded quartet
						*outptr++ = (byte) ((saved >> 16) & 0xFF);
						*outptr++ = (byte) ((saved >> 8) & 0xFF);
						*outptr++ = (byte) (saved & 0xFF);
						saved = 0;
						bytes = 0;
					}
				} else if (c == (byte) '=') {
					// Note: this marks the end of the base64 stream. The only octet that can
					// appear after this is another '=' (and possibly mailing-list junk).
					eof = true;
					break;
				}
			}

			// Note: there shouldn't be more than 2 '=' octets at the end of a quartet,
			// so if bytes < 2, then it means the encoder is broken.
			if (eof && bytes >= 2) {
				// at this point, `bytes` should be either 2 or 3
				int eq = 4 - bytes;

				saved <<= (6 * eq);

				*outptr++ = (byte) ((saved >> 16) & 0xFF);
				if (bytes > 2)
					*outptr++ = (byte) ((saved >> 8) & 0xFF);
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// decoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">The output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="input"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
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
		public int Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			if (eof)
				return 0;

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Decode (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Reset the decoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the decoder.
		/// </remarks>
		public void Reset ()
		{
			saved = 0;
			bytes = 0;
			eof = false;
		}
	}
}
