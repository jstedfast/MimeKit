//
// YDecoder.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the yEnc encoding.
	/// </summary>
	/// <remarks>
	/// <para>The yEncoding is an encoding that is most commonly used with Usenet and
	/// is a binary encoding that includes a 32-bit cyclic redundancy check.</para>
	/// <para>For more information, see http://www.yenc.org/</para>
	/// </remarks>
	public class YDecoder : IMimeDecoder
	{
		enum YDecoderState : byte {
			ExpectYBegin,
			YBeginEqual,
			YBeginEqualY,
			YBeginEqualYB,
			YBeginEqualYBe,
			YBeginEqualYBeg,
			YBeginEqualYBegi,
			YBeginEqualYBegin,
			ExpectYBeginNewLine,

			ExpectYPartOrPayload,

			YPartEqual,
			YPartEqualY,
			YPartEqualYP,
			YPartEqualYPa,
			YPartEqualYPar,
			YPartEqualYPart,
			ExpectYPartNewLine,

			Payload,
			Ended,
		}

		readonly YDecoderState initial;
		YDecoderState state;
		bool escaped;
		byte octet;
		bool eoln;
		Crc32 crc;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.YDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new yEnc decoder.
		/// </remarks>
		/// <param name="payloadOnly">
		/// If <c>true</c>, decoding begins immediately rather than after finding an =ybegin line.
		/// </param>
		public YDecoder (bool payloadOnly)
		{
			initial = payloadOnly ? YDecoderState.Payload : YDecoderState.ExpectYBegin;
			crc = new Crc32 (-1);
			Reset ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.YDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new yEnc decoder.
		/// </remarks>
		public YDecoder () : this (false)
		{
		}

		/// <summary>
		/// Gets the checksum.
		/// </summary>
		/// <remarks>
		/// Gets the checksum.
		/// </remarks>
		/// <value>The checksum.</value>
		public int Checksum {
			get { return crc.Checksum; }
		}

		/// <summary>
		/// Clone the <see cref="YDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="YDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="YDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			var decoder = new YDecoder (initial == YDecoderState.Payload);

			decoder.crc = crc.Clone ();
			decoder.state = state;
			decoder.octet = octet;
			decoder.eoln = eoln;

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
			get { return ContentEncoding.Default; }
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
			return inputLength;
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

		unsafe byte* ScanYBeginMarker (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == YDecoderState.ExpectYBegin) {
					if (octet != (byte) '\n') {
						while (inptr < inend && *inptr != (byte) '\n')
							inptr++;

						if (inptr == inend) {
							octet = *(inptr - 1);
							break;
						}

						octet = *inptr++;
						if (inptr == inend)
							break;
					}

					octet = *inptr++;
					if (octet != (byte) '=')
						continue;

					state = YDecoderState.YBeginEqual;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqual) {
					octet = *inptr++;
					if (octet != (byte) 'y') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualY;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualY) {
					octet = *inptr++;
					if (octet != (byte) 'b') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualYB;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualYB) {
					octet = *inptr++;
					if (octet != (byte) 'e') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualYBe;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualYBe) {
					octet = *inptr++;
					if (octet != (byte) 'g') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualYBeg;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualYBeg) {
					octet = *inptr++;
					if (octet != (byte) 'i') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualYBegi;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualYBegi) {
					octet = *inptr++;
					if (octet != (byte) 'n') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.YBeginEqualYBegin;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YBeginEqualYBegin) {
					octet = *inptr++;
					if (octet != (byte) ' ') {
						state = YDecoderState.ExpectYBegin;
						continue;
					}

					state = YDecoderState.ExpectYBeginNewLine;
					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.ExpectYBeginNewLine) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend) {
						octet = *(inptr - 1);
						break;
					}

					state = YDecoderState.ExpectYPartOrPayload;
					octet = *inptr++;
					break;
				}

				if (state == YDecoderState.ExpectYPartOrPayload) {
					if (*inptr != (byte) '=') {
						state = YDecoderState.Payload;
						break;
					}

					state = YDecoderState.YPartEqual;
					octet = *inptr++;
					escaped = true;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqual) {
					if (*inptr != (byte) 'y') {
						state = YDecoderState.Payload;
						return inptr;
					}

					state = YDecoderState.YPartEqualY;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqualY) {
					if (*inptr == (byte) 'e') {
						// we got an "=ye" which can only be an "=yend"
						state = YDecoderState.Ended;
						return inptr;
					}

					if (*inptr != (byte) 'p') {
						state = YDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YDecoderState.YPartEqualYP;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqualYP) {
					if (*inptr != (byte) 'a') {
						state = YDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YDecoderState.YPartEqualYPa;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqualYPa) {
					if (*inptr != (byte) 'r') {
						state = YDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YDecoderState.YPartEqualYPar;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqualYPar) {
					if (*inptr != (byte) 't') {
						state = YDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YDecoderState.YPartEqualYPart;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.YPartEqualYPart) {
					if (*inptr != (byte) ' ') {
						state = YDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YDecoderState.ExpectYPartNewLine;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YDecoderState.ExpectYPartNewLine) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend) {
						octet = *(inptr - 1);
						break;
					}

					state = YDecoderState.Payload;
					octet = *inptr++;
					break;
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
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			if (state < YDecoderState.Payload) {
				if ((inptr = ScanYBeginMarker (inptr, inend)) == inend)
					return 0;

				eoln = true;
			}

			if (state == YDecoderState.Ended)
				return 0;

			while (inptr < inend) {
				octet = *inptr++;

				if (octet == (byte) '\r') {
					escaped = false;
					continue;
				}

				if (octet == (byte) '\n') {
					escaped = false;
					eoln = true;
					continue;
				}

				if (escaped) {
					if (eoln && octet == (byte) 'y') {
						// this can only be =yend
						state = YDecoderState.Ended;
						break;
					}

					escaped = false;
					eoln = false;
					octet -= 64;
				} else if (octet == (byte) '=') {
					escaped = true;
					continue;
				} else {
					eoln = false;
				}

				octet -= 42;

				crc.Update (octet);
				*outptr++ = octet;
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
			octet = (byte) '\n';
			state = initial;
			escaped = false;
			eoln = true;

			crc.Reset ();
		}
	}
}
