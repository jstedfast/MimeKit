//
// Base64Validator.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally validates content encoded with the base64 encoding.
	/// </summary>
	/// <remarks>
	/// Base64 is an encoding often used in MIME to encode binary content such
	/// as images and other types of multimedia to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	class Base64Validator : IEncodingValidator
	{
		bool invalid;
		int padding;
		int octets;

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Validator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 validator.
		/// </remarks>
		public Base64Validator ()
		{
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the validator supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Base64; }
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void ValidateArguments (byte[] input, int startIndex, int length)
		{
			if (input is null)
				throw new ArgumentNullException (nameof (input));

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
		}

#if NET6_0_OR_GREATER
		[SkipLocalsInit]
#endif
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe bool Validate (ref byte table, byte* input, int length)
		{
			byte* inend = input + length;
			byte* inptr = input;

			if (padding == 0) {
				while (inptr < inend) {
					byte c = *inptr++;
					byte rank = Unsafe.Add (ref table, c);

					if (rank == 0xFF) {
						if (c == (byte) '\n') {
							if (octets % 4 != 0) {
								invalid = true;
								return false;
							}

							octets = 0;
						} else if (!c.IsWhitespace ()) {
							invalid = true;
							return false;
						}
					} else if (c == (byte) '=') {
						padding = 1;
						octets++;
						break;
					} else {
						octets++;
					}
				}
			}

			while (inptr < inend) {
				byte c = *inptr++;

				if (c == (byte) '\n') {
					if (octets % 4 != 0) {
						invalid = true;
						return false;
					}

					octets = 0;
				} else if (c == (byte) '=') {
					padding++;
					octets++;

					if (octets % 4 != 0 || padding > 2) {
						invalid = true;
						return false;
					}
				} else if (!c.IsWhitespace ()) {
					invalid = true;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Validate that a buffer contains only valid base64 encoded content.
		/// </summary>
		/// <remarks>
		/// Validates that the input buffer contains only valid base64 encoded content.
		/// </remarks>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <returns><see langword="true"/> if the content is valid; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="input"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		public unsafe bool Validate (byte[] input, int startIndex, int length)
		{
			ValidateArguments (input, startIndex, length);

			if (invalid)
				return false;

			fixed (byte* inbuf = input) {
				ref byte table = ref MemoryMarshal.GetReference (Base64Decoder.base64_rank);

				return Validate (ref table, inbuf + startIndex, length);
			}
		}

		/// <summary>
		/// Complete the validation process.
		/// </summary>
		/// <remarks>
		/// Completes the validation process.
		/// </remarks>
		/// <returns><see langword="true"/> if the content was valid; otherwise, <see langword="false"/>.</returns>
		public bool Complete ()
		{
			return invalid || (octets % 4 == 0 && padding <= 2);
		}
	}
}
