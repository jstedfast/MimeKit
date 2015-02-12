//
// YEncoder.cs
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
	/// Incrementally encodes content using the yEnc encoding.
	/// </summary>
	/// <remarks>
	/// <para>The yEncoding is an encoding that is most commonly used with Usenet and
	/// is a binary encoding that includes a 32-bit cyclic redundancy check.</para>
	/// <para>For more information, see http://www.yenc.org/</para>
	/// </remarks>
	public class YEncoder : IMimeEncoder
	{
		int lineLength;
		byte octets;
		Crc32 crc;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.YEncoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new yEnc encoder.
		/// </remarks>
		/// <param name="maxLineLength">The line length to use.</param>
		public YEncoder (int maxLineLength = 128)
		{
			if (maxLineLength < 0 || maxLineLength > 254)
				throw new ArgumentOutOfRangeException ("maxLineLength");

			lineLength = maxLineLength;
			crc = new Crc32 ();

			Reset ();
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
		/// Clone the <see cref="YEncoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="YEncoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="YEncoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			var encoder = new YEncoder ();

			encoder.lineLength = lineLength;
			encoder.crc = crc.Clone ();
			encoder.octets = octets;

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
			get { return ContentEncoding.Default; }
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
			return (inputLength * 2) + (inputLength / lineLength) + 1;
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

		unsafe int Encode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			while (inptr < inend) {
				byte c = *inptr++;

				crc.Update (c);

				c += (byte) 42;

				if (c == 0 || c == (byte) '\t' || c == (byte) '\r' || c == (byte) '\n' || c == (byte) '=' || c == (byte) '.') {
					*outptr++ = (byte) '=';
					*outptr++ = (byte) (c + 64);
					octets += 2;
				} else {
					*outptr++ = c;
					octets++;
				}

				if (octets >= lineLength) {
					*outptr++ = (byte) '\n';
					octets = 0;
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
				fixed (byte* inptr = input, outptr = output) {
					return Encode (inptr + startIndex, length, outptr);
				}
			}
		}

		unsafe int Flush (byte* input, int length, byte* output)
		{
			byte* outptr = output;

			if (length > 0)
				outptr += Encode (input, length, output);

			if (octets > 0) {
				*outptr++ = (byte) '\n';
				octets = 0;
			}

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
				fixed (byte* inptr = input, outptr = output) {
					return Flush (inptr + startIndex, length, outptr);
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
			crc.Reset ();
			octets = 0;
		}
	}
}
