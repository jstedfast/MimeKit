//
// Rfc2231Decoder.cs
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
	/// Incrementally decodes RFC 2231 percent-encoded parameter values.
	/// </summary>
	/// <remarks>
	/// This is mostly meant for decoding parameter values encoded using
	/// the rules specified by rfc2184 and rfc2231.
	/// </remarks>
	class Rfc2231Decoder : IMimeDecoder
	{
		enum Rfc2231DecoderState : byte {
			PassThrough,
			Percent,
			DecodeByte
		}

		Rfc2231DecoderState state;
		byte saved;

		/// <summary>
		/// Initialize a new instance of the <see cref="Rfc2231Decoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new RFC 2231 decoder.
		/// </remarks>
		public Rfc2231Decoder ()
		{
		}

		/// <summary>
		/// Clone the <see cref="Rfc2231Decoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Rfc2231Decoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="Rfc2231Decoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			return new Rfc2231Decoder {
				state = state,
				saved = saved
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
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The estimated output length is too large to be represented as an <see cref="System.Int32"/>.
		/// </exception>
		public int EstimateOutputLength (int inputLength)
		{
			switch (state) {
			case Rfc2231DecoderState.PassThrough: return inputLength;
			case Rfc2231DecoderState.Percent: return ArgumentValidator.ValidateOutputLength ((long) inputLength + 1); // add an extra byte in case the '%' character is not the start of a valid hex sequence
			default: return ArgumentValidator.ValidateOutputLength ((long) inputLength + 2); // add an extra 2 bytes in case the %X sequence is not the start of a valid hex sequence
			}
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
			byte c;

			while (inptr < inend) {
				switch (state) {
				case Rfc2231DecoderState.PassThrough:
					while (inptr < inend) {
						c = *inptr++;

						if (c == '%') {
							state = Rfc2231DecoderState.Percent;
							break;
						}

						*outptr++ = c;
					}
					break;
				case Rfc2231DecoderState.Percent:
					c = *inptr++;
					state = Rfc2231DecoderState.DecodeByte;
					saved = c;
					break;
				case Rfc2231DecoderState.DecodeByte:
					c = *inptr++;
					if (c.IsXDigit () && saved.IsXDigit ()) {
						saved = saved.ToXDigit ();
						c = c.ToXDigit ();

						*outptr++ = (byte) ((saved << 4) | c);
					} else {
						// invalid encoded sequence - pass it through undecoded
						*outptr++ = (byte) '%';
						*outptr++ = saved;
						*outptr++ = c;
					}

					state = Rfc2231DecoderState.PassThrough;
					break;
				}
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
			state = Rfc2231DecoderState.PassThrough;
			saved = 0;
		}
	}
}
