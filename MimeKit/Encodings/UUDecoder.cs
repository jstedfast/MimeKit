//
// UuDecoder.cs
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

namespace MimeKit {
	public class UUDecoder : IMimeDecoder
	{
		static readonly byte[] uudecode_rank = new byte[256] {
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
		};

		bool ended;
		byte nsaved;
		byte uulen;
		uint saved;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.UuDecoder"/> class.
		/// </summary>
		public UUDecoder ()
		{
			Reset ();
		}

		/// <summary>
		/// Clones the decoder.
		/// </summary>
		public object Clone ()
		{
			return MemberwiseClone ();
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <value>
		/// The encoding.
		/// </value>
		public ContentEncoding Encoding
		{
			get { return ContentEncoding.UUEncode; }
		}

		/// <summary>
		/// Estimates the length of the output.
		/// </summary>
		/// <returns>
		/// The estimated output length.
		/// </returns>
		/// <param name='inputLength'>
		/// The input length.
		/// </param>
		public int EstimateOutputLength (int inputLength)
		{
			// add an extra 3 bytes for the saved input bytes from previous decode step
			return inputLength + 3;
		}

		void ValidateArguments (byte[] input, int startIndex, int length, byte[] output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > input.Length)
				throw new ArgumentOutOfRangeException ("length");

			if (output == null)
				throw new ArgumentNullException ("output");

			if (output.Length < EstimateOutputLength (length))
				throw new ArgumentException ("The output buffer is not large enough to contain the decoded input.", "output");
		}

		static byte Decode (int c)
		{
			return (byte) ((c - 0x20) & 0x3F);
		}

		unsafe int Decode (byte* input, int length, byte* output)
		{
			if (ended)
				return 0;

			bool last_was_eoln = uulen == 0;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			while (inptr < inend) {
				if (*inptr == (byte) '\n') {
					last_was_eoln = true;
					inptr++;
					continue;
				} else if (uulen == 0 || last_was_eoln) {
					// first octet on a line is the uulen octet
					uulen = uudecode_rank[*inptr];
					last_was_eoln = false;
					if (uulen == 0) {
						ended = true;
						break;
					}

					inptr++;
					continue;
				}

				c = *inptr++;

				if (uulen > 0) {
					// save the byte
					saved = (saved << 8) | c;
					nsaved++;

					if (nsaved == 4) {
						byte b0 = (byte) ((saved >> 24) & 0xFF);
						byte b1 = (byte) ((saved >> 16) & 0xFF);
						byte b2 = (byte) ((saved >> 8) & 0xFF);
						byte b3 = (byte) (saved & 0xFF);

						if (uulen >= 3) {
							*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
							*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
							*outptr++ = (byte) (uudecode_rank[b2] << 6 | uudecode_rank[b3]);
							uulen -= 3;
						} else {
							if (uulen >= 1) {
								*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
								uulen--;
							}

							if (uulen >= 1) {
								*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
								uulen--;
							}
						}

						nsaved = 0;
						saved = 0;
					}
				} else {
					break;
				}
			}

			return (int) (inptr - output);
		}

		/// <summary>
		/// Decodes the specified input into the output buffer.
		/// </summary>
		/// <returns>
		/// The number of bytes written to the output buffer.
		/// </returns>
		/// <param name='input'>
		/// The input buffer.
		/// </param>
		/// <param name='startIndex'>
		/// The starting index of the input buffer.
		/// </param>
		/// <param name='length'>
		/// The length of the input buffer.
		/// </param>
		/// <param name='output'>
		/// The output buffer.
		/// </param>
		public int Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Decode (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Resets the decoder.
		/// </summary>
		public void Reset ()
		{
			ended = false;
			nsaved = 0;
			saved = 0;
			uulen = 0;
		}
	}
}
