//
// Base64Encoder.cs
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

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally encodes content using the base64 encoding.
	/// </summary>
	/// <remarks>
	/// Base64 is an encoding often used in MIME to encode binary content such
	/// as images and other types of multi-media to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class Base64Encoder : IMimeEncoder
	{
		static ReadOnlySpan<byte> base64_alphabet => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8;

		readonly int quartetsPerLine;
		readonly bool rfc2047;
		int quartets;
		byte saved1;
		byte saved2;
		byte saved;

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Encoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 encoder.
		/// </remarks>
		/// <param name="rfc2047"><c>true</c> if this encoder will be used to encode rfc2047 encoded-word payloads; <c>false</c> otherwise.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		internal Base64Encoder (bool rfc2047, int maxLineLength = 76)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			// The base64 specification in rfc2045 require a maximum line length of 76.
			maxLineLength = Math.Min (maxLineLength, 76);

			quartetsPerLine = maxLineLength / 4;
			this.rfc2047 = rfc2047;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Encoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 encoder.
		/// </remarks>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		public Base64Encoder (int maxLineLength = 76) : this (false, maxLineLength)
		{
		}

		/// <summary>
		/// Clone the <see cref="Base64Encoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Base64Encoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="Base64Encoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			return new Base64Encoder (rfc2047, quartetsPerLine * 4) {
				quartets = quartets,
				saved1 = saved1,
				saved2 = saved2,
				saved = saved
			};
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Base64; }
		}

		/// <summary>
		/// Estimate the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to encode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			if (rfc2047)
				return ((inputLength + 2) / 3) * 4;

			int maxLineLength = (quartetsPerLine * 4) + 1;
			int maxInputPerLine = quartetsPerLine * 3;

			return (((inputLength + 2) / maxInputPerLine) * maxLineLength) + maxLineLength;
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
				throw new ArgumentException ("The output buffer is not large enough to contain the encoded input.", nameof (output));
		}

		unsafe int Encode (byte* input, int length, byte* output)
		{
			int remaining = length;
			byte* outptr = output;
			byte* inptr = input;

			if (length + saved > 2) {
				byte* inend = inptr + length - 2;
				int c1, c2, c3;

				c1 = saved < 1 ? *inptr++ : saved1;
				c2 = saved < 2 ? *inptr++ : saved2;
				c3 = *inptr++;

				do {
					// encode our triplet into a quartet
					*outptr++ = base64_alphabet[c1 >> 2];
					*outptr++ = base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
					*outptr++ = base64_alphabet[((c2 & 0x0f) << 2) | (c3 >> 6)];
					*outptr++ = base64_alphabet[c3 & 0x3f];

					// encode 18 quartets per line
					if (!rfc2047 && (++quartets) >= quartetsPerLine) {
						*outptr++ = (byte) '\n';
						quartets = 0;
					}

					if (inptr >= inend)
						break;

					c1 = *inptr++;
					c2 = *inptr++;
					c3 = *inptr++;
				} while (true);

				remaining = 2 - (int) (inptr - inend);
				saved = 0;
			}

			if (remaining > 0) {
				// At this point, saved can only be 0 or 1.
				if (saved == 0) {
					// We can have up to 2 remaining input bytes.
					saved = (byte) remaining;
					saved1 = *inptr++;
					if (remaining == 2)
						saved2 = *inptr;
					else
						saved2 = 0;
				} else {
					// We have 1 remaining input byte.
					saved2 = *inptr++;
					saved = 2;
				}
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encode the specified input into the output buffer.
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

			if (saved >= 1) {
				int c1 = saved1;
				int c2 = saved2;

				*outptr++ = base64_alphabet[c1 >> 2];
				*outptr++ = base64_alphabet[c2 >> 4 | ((c1 & 0x3) << 4)];
				if (saved == 2)
					*outptr++ = base64_alphabet[(c2 & 0x0f) << 2];
				else
					*outptr++ = (byte) '=';
				*outptr++ = (byte) '=';
				quartets++;
			}

			if (!rfc2047 && quartets > 0)
				*outptr++ = (byte) '\n';

			Reset ();

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encode the specified input into the output buffer, flushing any internal buffer state as well.
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
		/// Reset the encoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the encoder.
		/// </remarks>
		public void Reset ()
		{
			quartets = 0;
			saved1 = 0;
			saved2 = 0;
			saved = 0;
		}
	}
}
