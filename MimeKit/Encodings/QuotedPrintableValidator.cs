//
// QuotedPrintableValidator.cs
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
			DecodeByte
		}

		readonly MimeReader reader;
		long streamOffset;
		int lineNumber;
		QpValidatorState state;

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable validator.
		/// </remarks>
		/// <param name="reader">The mime reader.</param>
		/// <param name="streamOffset">The current stream offset.</param>
		/// <param name="lineNumber">The current line number.</param>
		public QuotedPrintableValidator (MimeReader reader, long streamOffset, int lineNumber)
		{
			this.reader = reader;
			this.streamOffset = streamOffset;
			this.lineNumber = lineNumber;
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
		unsafe void Validate (byte* input, int length)
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
						} else if (c == '\n') {
							lineNumber++;
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
						lineNumber++;
					} else {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset + (inptr - input), lineNumber);
						state = QpValidatorState.PassThrough;
					}
					break;
				case QpValidatorState.SoftBreak:
					c = *inptr++;

					if (c == '\n') {
						lineNumber++;
					} else {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, streamOffset + (inptr - input), lineNumber);
					}

					state = QpValidatorState.PassThrough;
					break;
				case QpValidatorState.DecodeByte:
					c = *inptr++;

					if (!c.IsXDigit ()) {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset + (inptr - input), lineNumber);

						if (c == '\n')
							lineNumber++;
					}

					state = QpValidatorState.PassThrough;
					break;
				}
			}

			streamOffset += length;
		}

		/// <summary>
		/// Write a sequence of bytes to the validator.
		/// </summary>
		/// <remarks>
		/// Writes a sequence of bytes to the validator.
		/// </remarks>
		/// <param name="buffer">The buffer.</param>
		/// <param name="startIndex">The starting index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="buffer"/> byte array.
		/// </exception>
		public unsafe void Write (byte[] buffer, int startIndex, int length)
		{
			ValidateArguments (buffer, startIndex, length);

			fixed (byte* inbuf = buffer) {
				Validate (inbuf + startIndex, length);
			}
		}

		/// <summary>
		/// Flush the validator state.
		/// </summary>
		/// <remarks>
		/// Flushes the validator state.
		/// </remarks>
		public void Flush ()
		{
			// Note: the only valid state to end on is the pass-through state.
			if (state == QpValidatorState.EqualSign || state == QpValidatorState.DecodeByte)
				reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset, lineNumber);
			else if (state == QpValidatorState.SoftBreak)
				reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, streamOffset, lineNumber);
		}
	}
}
