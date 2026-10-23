//
// YEncDecoder.cs
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

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the yEnc encoding.
	/// </summary>
	/// <remarks>
	/// <para>The yEncoding is an encoding that is most commonly used with Usenet and
	/// is a binary encoding that includes a 32-bit cyclic redundancy check.</para>
	/// <para>For more information, see <a href="http://www.yenc.org">www.yenc.org</a>.</para>
	/// </remarks>
	public class YEncDecoder : IMimeDecoder
	{
		enum YEncDecoderState : byte {
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

		readonly YEncDecoderState initial;
		YEncDecoderState state;
		bool escaped;
		byte octet;
		bool eoln;
		Crc32 crc;

		/// <summary>
		/// Initialize a new instance of the <see cref="YEncDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new yEnc decoder.
		/// </remarks>
		/// <param name="payloadOnly">
		/// If <see langword="true" />, decoding begins immediately rather than after finding an =ybegin line.
		/// </param>
		public YEncDecoder (bool payloadOnly)
		{
			initial = payloadOnly ? YEncDecoderState.Payload : YEncDecoderState.ExpectYBegin;
			crc = new Crc32 (-1);
			Reset ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="YEncDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new yEnc decoder.
		/// </remarks>
		public YEncDecoder () : this (false)
		{
		}

		/// <summary>
		/// Get the checksum.
		/// </summary>
		/// <remarks>
		/// Gets the checksum.
		/// </remarks>
		/// <value>The checksum.</value>
		public int Checksum {
			get { return crc.Checksum; }
		}

		/// <summary>
		/// Clone the <see cref="YEncDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="YEncDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="YEncDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			return new YEncDecoder (initial == YEncDecoderState.Payload) {
				crc = crc.Clone (),
				escaped = escaped,
				state = state,
				octet = octet,
				eoln = eoln
			};
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the decoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Unspecified; }
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
			return inputLength;
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
				throw new ArgumentException ("The output buffer is not large enough to contain the decoded input.", nameof (output));
		}

		unsafe byte* ScanYBeginMarker (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == YEncDecoderState.ExpectYBegin) {
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

					state = YEncDecoderState.YBeginEqual;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqual) {
					octet = *inptr++;
					if (octet != (byte) 'y') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualY;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualY) {
					octet = *inptr++;
					if (octet != (byte) 'b') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualYB;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualYB) {
					octet = *inptr++;
					if (octet != (byte) 'e') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualYBe;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualYBe) {
					octet = *inptr++;
					if (octet != (byte) 'g') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualYBeg;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualYBeg) {
					octet = *inptr++;
					if (octet != (byte) 'i') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualYBegi;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualYBegi) {
					octet = *inptr++;
					if (octet != (byte) 'n') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.YBeginEqualYBegin;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YBeginEqualYBegin) {
					octet = *inptr++;
					if (octet != (byte) ' ') {
						state = YEncDecoderState.ExpectYBegin;
						continue;
					}

					state = YEncDecoderState.ExpectYBeginNewLine;
					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.ExpectYBeginNewLine) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend) {
						octet = *(inptr - 1);
						break;
					}

					state = YEncDecoderState.ExpectYPartOrPayload;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.ExpectYPartOrPayload) {
					if (*inptr != (byte) '=') {
						state = YEncDecoderState.Payload;
						escaped = false;
						eoln = true;
						break;
					}

					state = YEncDecoderState.YPartEqual;
					octet = *inptr++;
					escaped = true;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqual) {
					if (*inptr != (byte) 'y') {
						state = YEncDecoderState.Payload;
						escaped = true;
						eoln = true;
						return inptr;
					}

					state = YEncDecoderState.YPartEqualY;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqualY) {
					if (*inptr == (byte) 'e') {
						// we got an "=ye" which can only be an "=yend"
						state = YEncDecoderState.Ended;
						return inptr;
					}

					if (*inptr != (byte) 'p') {
						state = YEncDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YEncDecoderState.YPartEqualYP;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqualYP) {
					if (*inptr != (byte) 'a') {
						state = YEncDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YEncDecoderState.YPartEqualYPa;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqualYPa) {
					if (*inptr != (byte) 'r') {
						state = YEncDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YEncDecoderState.YPartEqualYPar;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqualYPar) {
					if (*inptr != (byte) 't') {
						state = YEncDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YEncDecoderState.YPartEqualYPart;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.YPartEqualYPart) {
					if (*inptr != (byte) ' ') {
						state = YEncDecoderState.ExpectYBeginNewLine;
						continue;
					}

					state = YEncDecoderState.ExpectYPartNewLine;
					octet = *inptr++;

					if (inptr == inend)
						break;
				}

				if (state == YEncDecoderState.ExpectYPartNewLine) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend) {
						octet = *(inptr - 1);
						break;
					}

					state = YEncDecoderState.Payload;
					octet = *inptr++;
					escaped = false;
					eoln = true;
					break;
				}
			}

			return inptr;
		}

		/// <summary>
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
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

			if (state < YEncDecoderState.Payload) {
				if ((inptr = ScanYBeginMarker (inptr, inend)) == inend)
					return 0;
			}

			if (state == YEncDecoderState.Ended)
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
						state = YEncDecoderState.Ended;
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
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// decoded input. For estimating the size needed for the output buffer,
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
		/// Reset the decoder.
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
