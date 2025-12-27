//
// QuotedPrintableValidator.cs
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
using System.Runtime.CompilerServices;

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally validates content encoded with the quoted-printable encoding.
	/// </summary>
	/// <remarks>
	/// Quoted-Printable is an encoding often used in MIME to textual content outside
	/// the ASCII range in order to ensure that the text remains intact when sent
	/// via 7bit transports such as SMTP.
	/// </remarks>
	class QuotedPrintableValidator : IEncodingValidator
	{
		enum QpValidatorState : byte
		{
			PassThrough,
			EqualSign,
			SoftBreak,
			DecodeByte,
			Invalid
		}

		QpValidatorState state;

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable validator.
		/// </remarks>
		public QuotedPrintableValidator ()
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
			get { return ContentEncoding.QuotedPrintable; }
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
		unsafe bool Validate (byte* input, int length)
		{
			byte* inend = input + length;
			byte* inptr = input;
			byte c;

			while (inptr < inend) {
				switch (state) {
				case QpValidatorState.PassThrough:
					while (inptr < inend) {
						c = *inptr++;

						if (c == '=') {
							state = QpValidatorState.EqualSign;
							break;
						}
					}
					break;
				case QpValidatorState.EqualSign:
					c = *inptr++;

					if (c.IsXDigit ()) {
						state = QpValidatorState.DecodeByte;
					} else if (c == '\r') {
						state = QpValidatorState.SoftBreak;
					} else if (c == '\n') {
						state = QpValidatorState.PassThrough;
					} else {
						// invalid encoded sequence
						state = QpValidatorState.Invalid;
						return false;
					}
					break;
				case QpValidatorState.SoftBreak:
					state = QpValidatorState.PassThrough;
					c = *inptr++;

					if (c != '\n') {
						// invalid encoded sequence
						state = QpValidatorState.Invalid;
						return false;
					}
					break;
				case QpValidatorState.DecodeByte:
					c = *inptr++;

					if (!c.IsXDigit ()) {
						// invalid encoded sequence
						state = QpValidatorState.Invalid;
						return false;
					}

					state = QpValidatorState.PassThrough;
					break;
				}
			}

			return true;
		}

		/// <summary>
		/// Validate that a buffer contains only valid quoted-printable encoded content.
		/// </summary>
		/// <remarks>
		/// Validates that the input buffer contains only valid quoted-printable encoded content.
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

			if (state == QpValidatorState.Invalid)
				return false;

			fixed (byte* inbuf = input) {
				return Validate (inbuf + startIndex, length);
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
			// Note: the only valid state to end on is the pass-through state.
			return state == QpValidatorState.PassThrough;
		}
	}
}
