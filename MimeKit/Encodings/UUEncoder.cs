//
// UUEncoder.cs
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
	/// Incrementally encodes content using the Unix-to-Unix encoding.
	/// </summary>
	/// <remarks>
	/// <para>The UUEncoding is an encoding that predates MIME and was used to encode
	/// binary content such as images and other types of multi-media to ensure
	/// that the data remained intact when sent via 7bit transports such as SMTP.</para>
	/// <para>These days, the UUEncoding has largely been deprecated in favour of
	/// the base64 encoding, however, some older mail clients still use it.</para>
	/// </remarks>
	public class UUEncoder : IMimeEncoder
	{
		const int MaxInputPerLine = 45;
		const int MaxOutputPerLine = ((MaxInputPerLine / 3) * 4) + 2;

		readonly byte[] uubuf = new byte[60];
		uint saved;
		byte nsaved;
		byte uulen;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.UUEncoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix encoder.
		/// </remarks>
		public UUEncoder ()
		{
			Reset ();
		}

		/// <summary>
		/// Clone the <see cref="UUEncoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="UUEncoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="UUEncoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			var encoder = new UUEncoder ();

			Buffer.BlockCopy (uubuf, 0, encoder.uubuf, 0, uubuf.Length);
			encoder.nsaved = nsaved;
			encoder.saved = saved;
			encoder.uulen = uulen;

			return encoder;
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.UUEncode; }
		}

		/// <summary>
		/// Estimates the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to encode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			return (((inputLength + 2) / MaxInputPerLine) * MaxOutputPerLine) + MaxOutputPerLine + 2;
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
				throw new ArgumentException ("The output buffer is not large enough to contain the encoded input.", "output");
		}

		static byte Encode (int c)
		{
			return c != 0 ? (byte) (c + 0x20) : (byte) '`';
		}

		unsafe int Encode (byte* input, int length, byte[] outbuf, byte* output, byte *uuptr)
		{
			if (length == 0)
				return 0;

			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte* bufptr;
			byte b0, b1, b2;

			if ((length + uulen) < 45) {
				// not enough input to write a full uuencoded line
				bufptr = uuptr + ((uulen / 3) * 4);
			} else {
				bufptr = outptr + 1;
				
				if (uulen > 0) {
					// copy the previous call's uubuf to output
					int n = (uulen / 3) * 4;

					Buffer.BlockCopy (uubuf, 0, outbuf, (int) (bufptr - output), n);
					bufptr += n;
				}
			}

			if (nsaved == 2) {
				b0 = (byte) ((saved >> 8) & 0xFF);
				b1 = (byte) (saved & 0xFF);
				b2 = *inptr++;
				nsaved = 0;
				saved = 0;

				// convert 3 input bytes into 4 uuencoded bytes
				*bufptr++ = Encode ((b0 >> 2) & 0x3F);
				*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
				*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
				*bufptr++ = Encode (b2 & 0x3F);

				uulen += 3;
			} else if (nsaved == 1) {
				if ((inptr + 2) < inend) {
					b0 = (byte) (saved & 0xFF);
					b1 = *inptr++;
					b2 = *inptr++;
					nsaved = 0;
					saved = 0;

					// convert 3 input bytes into 4 uuencoded bytes
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr++ = Encode (b2 & 0x3F);

					uulen += 3;
				} else {
					while (inptr < inend) {
						saved = (saved << 8) | *inptr++;
						nsaved++;
					}
				}
			}

			while (inptr < inend) {
				while (uulen < 45 && (inptr + 3) <= inend) {
					b0 = *inptr++;
					b1 = *inptr++;
					b2 = *inptr++;

					// convert 3 input bytes into 4 uuencoded bytes
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr++ = Encode (b2 & 0x3F);

					uulen += 3;
				}

				if (uulen >= 45) {
					// output the uu line length
					*outptr = Encode (uulen);
					outptr += ((uulen / 3) * 4) + 1;
					*outptr++ = (byte) '\n';
					uulen = 0;

					if ((inptr + 45) <= inend) {
						// we have enough input to output another full line
						bufptr = outptr + 1;
					} else {
						bufptr = uuptr;
					}
				} else {
					// not enough input to continue...
					for (nsaved = 0, saved = 0; inptr < inend; nsaved++)
						saved = (saved << 8) | *inptr++;
				}
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encodes the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Encodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// encoded input. For estimating the size needed for the output buffer,
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
		public int Encode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output, uuptr = uubuf) {
					return Encode (inptr + startIndex, length, output, outptr, uuptr);
				}
			}
		}

		unsafe int Flush (byte* input, int length, byte[] outbuf, byte* output, byte* uuptr)
		{
			byte* outptr = output;

			if (length > 0)
				outptr += Encode (input, length, outbuf, output, uuptr);

			byte* bufptr = uuptr + ((uulen / 3) * 4);
			byte uufill = 0;

			if (nsaved > 0) {
				while (nsaved < 3) {
					saved <<= 8;
					uufill++;
					nsaved++;
				}
				
				if (nsaved == 3) {
					// convert 3 input bytes into 4 uuencoded bytes
					byte b0, b1, b2;
					
					b0 = (byte) ((saved >> 16) & 0xFF);
					b1 = (byte) ((saved >> 8) & 0xFF);
					b2 = (byte) (saved & 0xFF);
					
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr++ = Encode (b2 & 0x3F);
					
					uulen += 3;
					nsaved = 0;
					saved = 0;
				}
			}
			
			if (uulen > 0) {
				int n = (uulen / 3) * 4;
				
				*outptr++ = Encode ((uulen - uufill) & 0xFF);
				Buffer.BlockCopy (uubuf, 0, outbuf, (int) (outptr - output), n);
				outptr += n;

				*outptr++ = (byte) '\n';
				uulen = 0;
			}
			
			*outptr++ = Encode (uulen & 0xFF);
			*outptr++ = (byte) '\n';

			Reset ();

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encodes the specified input into the output buffer, flushing any internal buffer state as well.
		/// </summary>
		/// <remarks>
		/// <para>Encodes the specified input into the output buffer, flusing any internal state as well.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// encoded input. For estimating the size needed for the output buffer,
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
		public int Flush (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output, uuptr = uubuf) {
					return Flush (inptr + startIndex, length, output, outptr, uuptr);
				}
			}
		}

		/// <summary>
		/// Resets the encoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the encoder.
		/// </remarks>
		public void Reset ()
		{
			nsaved = 0;
			saved = 0;
			uulen = 0;
		}
	}
}
