//
// QuotedPrintableDecoder.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
	/// Incrementally decodes content encoded with the quoted-printable encoding.
	/// </summary>
	/// <remarks>
	/// Quoted-Printable is an encoding often used in MIME to textual content outside
	/// of the ASCII range in order to ensure that the text remains intact when sent
	/// via 7bit transports such as SMTP.
	/// </remarks>
	public class QuotedPrintableDecoder : IMimeDecoder
	{
		enum QpDecoderState : byte {
			PassThrough,
			EqualSign,
			SoftBreak,
			DecodeByte
		}

		readonly bool rfc2047;
		QpDecoderState state;
		byte saved;

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable decoder.
		/// </remarks>
		/// <param name="rfc2047"><c>true</c> if this decoder will be used to decode rfc2047 encoded-word tokens; otherwise, <c>false</c>.</param>
		public QuotedPrintableDecoder (bool rfc2047)
		{
			this.rfc2047 = rfc2047;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable decoder.
		/// </remarks>
		public QuotedPrintableDecoder () : this (false)
		{
		}

		/// <summary>
		/// Clone the <see cref="QuotedPrintableDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="QuotedPrintableDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="QuotedPrintableDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			return new QuotedPrintableDecoder (rfc2047) {
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
			get { return ContentEncoding.QuotedPrintable; }
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
			switch (state) {
			case QpDecoderState.PassThrough: return inputLength;
			case QpDecoderState.EqualSign: return inputLength + 1; // add an extra byte in case the '=' character is not the start of a valid hex sequence
			default: return inputLength + 2; // add an extra 2 bytes in case the =X sequence is not the start of a valid hex sequence
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

			while (inptr < inend) {
				switch (state) {
				case QpDecoderState.PassThrough:
					while (inptr < inend) {
						c = *inptr++;

						if (c == '=') {
							state = QpDecoderState.EqualSign;
							break;
						} else if (rfc2047 && c == '_') {
							*outptr++ = (byte) ' ';
						} else {
							*outptr++ = c;
						}
					}
					break;
				case QpDecoderState.EqualSign:
					c = *inptr++;

					if (c.IsXDigit ()) {
						state = QpDecoderState.DecodeByte;
						saved = c;
					} else if (c == '=') {
						// invalid encoded sequence - pass it through undecoded
						*outptr++ = (byte) '=';
					} else if (c == '\r') {
						state = QpDecoderState.SoftBreak;
					} else if (c == '\n') {
						state = QpDecoderState.PassThrough;
					} else {
						// invalid encoded sequence - pass it through undecoded
						state = QpDecoderState.PassThrough;
						*outptr++ = (byte) '=';
						*outptr++ = c;
					}
					break;
				case QpDecoderState.SoftBreak:
					state = QpDecoderState.PassThrough;
					c = *inptr++;

					if (c != '\n') {
						// invalid encoded sequence - pass it through undecoded
						*outptr++ = (byte) '=';
						*outptr++ = (byte) '\r';
						*outptr++ = c;
					}
					break;
				case QpDecoderState.DecodeByte:
					c = *inptr++;
					if (c.IsXDigit ()) {
						saved = saved.ToXDigit ();
						c = c.ToXDigit ();

						*outptr++ = (byte) ((saved << 4) | c);
					} else {
						// invalid encoded sequence - pass it through undecoded
						*outptr++ = (byte) '=';
						*outptr++ = saved;
						*outptr++ = c;
					}

					state = QpDecoderState.PassThrough;
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
		/// Reset the decoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the decoder.
		/// </remarks>
		public void Reset ()
		{
			state = QpDecoderState.PassThrough;
			saved = 0;
		}
	}
}
