//
// QEncoder.cs
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

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Q-Encoding mode.
	/// </summary>
	/// <remarks>
	/// The encoding mode for the 'Q' encoding used in rfc2047.
	/// </remarks>
	public enum QEncodeMode : byte {
		/// <summary>
		/// A mode for encoding phrases, as defined by rfc822.
		/// </summary>
		Phrase,

		/// <summary>
		/// A mode for encoding text.
		/// </summary>
		Text
	}

	/// <summary>
	/// Incrementally encodes content using a variation of the quoted-printable encoding
	/// that is specifically meant to be used for rfc2047 encoded-word tokens.
	/// </summary>
	/// <remarks>
	/// The Q-Encoding is an encoding often used in MIME to encode textual content outside
	/// of the ASCII range within an rfc2047 encoded-word token in order to ensure that
	/// the text remains intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class QEncoder : IMimeEncoder
	{
		static readonly byte[] hex_alphabet = new byte[16] {
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, // '0' -> '7'
			0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, // '8' -> 'F'
		};

		readonly CharType mask;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.QEncoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new rfc2047 quoted-printable encoder.
		/// </remarks>
		/// <param name="mode">The rfc2047 encoding mode.</param>
		public QEncoder (QEncodeMode mode)
		{
			mask = mode == QEncodeMode.Phrase ? CharType.IsEncodedPhraseSafe : CharType.IsEncodedWordSafe;
			Reset ();
		}

		/// <summary>
		/// Clone the <see cref="QEncoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="QEncoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="QEncoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			return new QEncoder (mask == CharType.IsEncodedPhraseSafe ? QEncodeMode.Phrase : QEncodeMode.Text);
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.QuotedPrintable; }
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
			return inputLength * 3;
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
			if (length == 0)
				return 0;

			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			while (inptr < inend) {
				byte c = *inptr++;

				if (c == ' ') {
					*outptr++ = (byte) '_';
				} else if (c != '_' && c.IsType (mask)) {
					*outptr++ = c;
				} else {
					*outptr++ = (byte) '=';
					*outptr++ = hex_alphabet[(c >> 4) & 0x0f];
					*outptr++ = hex_alphabet[c & 0x0f];
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
		}
	}
}
