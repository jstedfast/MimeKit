//
// QuotedPrintableEncoder.cs
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
	/// Incrementally encodes content using the quoted-printable encoding.
	/// </summary>
	/// <remarks>
	/// Quoted-Printable is an encoding often used in MIME to encode textual content
	/// outside of the ASCII range in order to ensure that the text remains intact
	/// when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class QuotedPrintableEncoder : IMimeEncoder
	{
		static ReadOnlySpan<byte> hex_alphabet => "0123456789ABCDEF"u8;

		readonly short tripletsPerLine;
		readonly short maxLineLength;
		short currentLineLength;
		short saved;

		QuotedPrintableEncoder (short tripletsPerLine, short maxLineLength)
		{
			this.tripletsPerLine = tripletsPerLine;
			this.maxLineLength = maxLineLength;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableEncoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable encoder.
		/// </remarks>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		public QuotedPrintableEncoder (int maxLineLength = 76)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			// The quoted-printable specification in rfc2045 require a maximum line length of 76.
			maxLineLength = Math.Min (maxLineLength, 76);

			// normalize the maximum line length
			tripletsPerLine = (short) (maxLineLength / 3);
			this.maxLineLength = (short) maxLineLength;

			Reset ();
		}

		/// <summary>
		/// Clone the <see cref="QuotedPrintableEncoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="QuotedPrintableEncoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="QuotedPrintableEncoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			return new QuotedPrintableEncoder (tripletsPerLine, maxLineLength) {
				currentLineLength = currentLineLength,
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
			get { return ContentEncoding.QuotedPrintable; }
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
			if (saved != -1)
				inputLength++;

			return ((inputLength / tripletsPerLine) * (maxLineLength + 1)) + ((inputLength % tripletsPerLine) * 3) + 2;
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
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			while (inptr < inend) {
				byte c = *inptr++;

				if (c == (byte) '\r') {
					if (saved != -1) {
						byte b = (byte) saved;

						// spaces and tabs must be encoded if they the last character on the line
						if (b.IsBlank () || !b.IsQpSafe ()) {
							*outptr++ = (byte) '=';
							*outptr++ = hex_alphabet[(b >> 4) & 0x0f];
							*outptr++ = hex_alphabet[b & 0x0f];
							currentLineLength += 3;
						} else {
							*outptr++ = b;
							currentLineLength++;
						}
					}

					saved = c;
				} else if (c == (byte) '\n') {
					if (saved != -1 && saved != (byte) '\r') {
						byte b = (byte) saved;

						// spaces and tabs must be encoded if they the last character on the line
						if (b.IsBlank () || !b.IsQpSafe ()) {
							*outptr++ = (byte) '=';
							*outptr++ = hex_alphabet[(b >> 4) & 0x0f];
							*outptr++ = hex_alphabet[b & 0x0f];
						} else {
							*outptr++ = b;
						}
					}

					*outptr++ = (byte) '\n';
					currentLineLength = 0;
					saved = -1;
				} else {
					if (saved != -1) {
						byte b = (byte) saved;

						if (b.IsQpSafe ()) {
							*outptr++ = b;
							currentLineLength++;
						} else {
							*outptr++ = (byte) '=';
							*outptr++ = hex_alphabet[(b >> 4) & 0x0f];
							*outptr++ = hex_alphabet[b & 0x0f];
							currentLineLength += 3;
						}

						if (currentLineLength + 1 >= maxLineLength) {
							*outptr++ = (byte) '=';
							*outptr++ = (byte) '\n';
							currentLineLength = 0;
						}
					}

					saved = c;
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

			if (saved != -1) {
				byte c = (byte) saved;

				// spaces and tabs must be encoded if they the last character on the line
				if (c.IsBlank () || !c.IsQpSafe ()) {
					*outptr++ = (byte) '=';
					*outptr++ = hex_alphabet[(c >> 4) & 0x0f];
					*outptr++ = hex_alphabet[c & 0x0f];
				} else {
					*outptr++ = c;
				}

				// we end with =\n so that the \n isn't interpreted as
				// a real \n when it gets decoded later
				*outptr++ = (byte) '=';
				*outptr++ = (byte) '\n';
			}

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
			currentLineLength = 0;
			saved = -1;
		}
	}
}
