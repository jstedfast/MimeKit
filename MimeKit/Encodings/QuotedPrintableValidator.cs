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

		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		long lineBeginOffset;
		long streamOffset;
		int lineNumber;
		QpValidatorState state;

		/// <summary>
		/// Initialize a new instance of the <see cref="QuotedPrintableValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new quoted-printable validator.
		/// </remarks>
		/// <param name="logger">The compliance logger.</param>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="streamOffset">The current stream offset.</param>
		/// <param name="lineNumber">The current line number.</param>
		public QuotedPrintableValidator (IMimeComplianceLogger logger, MimeComplianceContext context, long streamOffset, int lineNumber)
		{
			this.logger = logger;
			this.context = context;
			this.lineBeginOffset = streamOffset;
			this.streamOffset = streamOffset;
			this.lineNumber = lineNumber;
		}

		// Note: The validator only ever tracks the offset that the current line begins at. The column
		// is worked out from the offset being reported rather than from the cursor, because not every
		// violation is reported at the exact position that the cursor happens to be sitting at.
		int GetColumnNumber (long offset)
		{
			return (int) (offset - lineBeginOffset) + 1;
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
					// Note: The bulk of quoted-printable content consists of literal characters that are
					// simply passed through, so use a vectorized search to locate the next byte that the
					// validator actually needs to inspect.
					while (inptr < inend) {
						int index = new ReadOnlySpan<byte> (inptr, (int) (inend - inptr)).IndexOfAny ((byte) '=', (byte) '\n');

						if (index == -1) {
							inptr = inend;
							break;
						}

						c = inptr[index];
						inptr += index + 1;

						if (c == '=') {
							state = QpValidatorState.EqualSign;
							break;
						}

						lineBeginOffset = streamOffset + (inptr - input);
						lineNumber++;
					}
					break;
				case QpValidatorState.EqualSign:
					c = *inptr;

					if (c.IsXDigit ()) {
						state = QpValidatorState.DecodeByte;
					} else if (c == '\r') {
						state = QpValidatorState.SoftBreak;
					} else if (c == '\n') {
						state = QpValidatorState.PassThrough;
						lineBeginOffset = streamOffset + (inptr - input) + 1;
						lineNumber++;
					} else {
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset + (inptr - input), lineNumber, GetColumnNumber (streamOffset + (inptr - input))));
						state = QpValidatorState.PassThrough;
					}

					inptr++;
					break;
				case QpValidatorState.SoftBreak:
					if (*inptr == '\n') {
						inptr++;
						lineBeginOffset = streamOffset + (inptr - input);
						lineNumber++;
					} else {
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, streamOffset + (inptr - input), lineNumber, GetColumnNumber (streamOffset + (inptr - input))));
					}

					state = QpValidatorState.PassThrough;
					break;
				case QpValidatorState.DecodeByte:
					c = *inptr;

					if (!c.IsXDigit ()) {
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset + (inptr - input), lineNumber, GetColumnNumber (streamOffset + (inptr - input))));

						if (c == '\n') {
							lineBeginOffset = streamOffset + (inptr - input) + 1;
							lineNumber++;
						}
					}

					state = QpValidatorState.PassThrough;
					inptr++;
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
			ArgumentValidator.Validate (buffer, startIndex, length);

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
				logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidQuotedPrintableEncoding, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
			else if (state == QpValidatorState.SoftBreak)
				logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
		}
	}
}
