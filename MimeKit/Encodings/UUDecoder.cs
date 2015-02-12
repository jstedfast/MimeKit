//
// UUDecoder.cs
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

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the Unix-to-Unix encoding.
	/// </summary>
	/// <remarks>
	/// <para>The UUEncoding is an encoding that predates MIME and was used to encode
	/// binary content such as images and other types of multi-media to ensure
	/// that the data remained intact when sent via 7bit transports such as SMTP.</para>
	/// <para>These days, the UUEncoding has largely been deprecated in favour of
	/// the base64 encoding, however, some older mail clients still use it.</para>
	/// </remarks>
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

		enum UUDecoderState : byte {
			ExpectBegin,
			B,
			Be,
			Beg,
			Begi,
			Begin,
			ExpectPayload,
			Payload,
			Ended,
		}

		readonly UUDecoderState initial;
		UUDecoderState state;
		byte nsaved;
		byte uulen;
		uint saved;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.UUDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix decoder.
		/// </remarks>
		/// <param name="payloadOnly">
		/// If <c>true</c>, decoding begins immediately rather than after finding a begin-line.
		/// </param>
		public UUDecoder (bool payloadOnly)
		{
			initial = payloadOnly ? UUDecoderState.Payload : UUDecoderState.ExpectBegin;
			Reset ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.UUDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix decoder.
		/// </remarks>
		public UUDecoder () : this (false)
		{
		}

		/// <summary>
		/// Clone the <see cref="UUDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="UUDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="UUDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			var decoder = new UUDecoder (initial == UUDecoderState.Payload);

			decoder.state = state;
			decoder.nsaved = nsaved;
			decoder.saved = saved;
			decoder.uulen = uulen;

			return decoder;
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the decoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.UUEncode; }
		}

		/// <summary>
		/// Estimates the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to decode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
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

			if (length < 0 || length > (input.Length - startIndex))
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

		unsafe byte* ScanBeginMarker (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == UUDecoderState.ExpectBegin) {
					if (nsaved != 0 && nsaved != (byte) '\n') {
						while (inptr < inend && *inptr != (byte) '\n')
							inptr++;

						if (inptr == inend) {
							nsaved = *(inptr - 1);
							return inptr;
						}

						nsaved = *inptr++;
						if (inptr == inend)
							return inptr;
					}

					nsaved = *inptr++;
					if (nsaved != (byte) 'b')
						continue;

					state = UUDecoderState.B;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.B) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'e') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Be;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Be) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'g') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Beg;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Beg) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'i') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Begi;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Begi) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'n') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Begin;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Begin) {
					nsaved = *inptr++;
					if (nsaved != (byte) ' ') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.ExpectPayload;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.ExpectPayload) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend)
						return inptr;

					state = UUDecoderState.Payload;
					nsaved = 0;

					return inptr + 1;
				}
			}

			return inptr;
		}

		/// <summary>
		/// Decodes the specified input into the output buffer.
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
			if (state == UUDecoderState.Ended)
				return 0;

			bool last_was_eoln = uulen == 0;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			if (state < UUDecoderState.Payload) {
				if ((inptr = ScanBeginMarker (inptr, inend)) == inend)
					return 0;
			}

			while (inptr < inend) {
				if (*inptr == (byte) '\r') {
					inptr++;
					continue;
				}

				if (*inptr == (byte) '\n') {
					last_was_eoln = true;
					inptr++;
					continue;
				}

				if (uulen == 0 || last_was_eoln) {
					// first octet on a line is the uulen octet
					uulen = uudecode_rank[*inptr];
					last_was_eoln = false;
					if (uulen == 0) {
						state = UUDecoderState.Ended;
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

			return (int) (outptr - output);
		}

		/// <summary>
		/// Decodes the specified input into the output buffer.
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

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Decode (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Resets the decoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the decoder.
		/// </remarks>
		public void Reset ()
		{
			state = initial;
			nsaved = 0;
			saved = 0;
			uulen = 0;
		}
	}
}
