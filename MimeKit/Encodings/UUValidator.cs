//
// UUValidator.cs
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
	/// Incrementally validates content encoded with the Unix-to-Unix encoding.
	/// </summary>
	/// <remarks>
	/// <para>The UUEncoding is an encoding that predates MIME and was used to encode
	/// binary content such as images and other types of multimedia to ensure
	/// that the data remained intact when sent via 7bit transports such as SMTP.</para>
	/// <para>These days, the UUEncoding has largely been deprecated in favour of
	/// the base64 encoding, however, some older mail clients still use it.</para>
	/// </remarks>
	class UUValidator : IEncodingValidator
	{
		enum UUValidatorState : byte
		{
			ExpectBegin,
			B,
			Be,
			Beg,
			Begi,
			Begin,
			FileMode,
			FileName,
			Payload,
			Ended,
			EndedNewLine,
			E,
			En,
			End,
			Invalid
		}

		readonly MimeReader reader;
		long streamOffset;
		int lineNumber;
		UUValidatorState state;
		byte nsaved;
		byte uulen;

		/// <summary>
		/// Initialize a new instance of the <see cref="UUValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix validator.
		/// </remarks>
		/// <param name="reader">The mime reader.</param>
		/// <param name="streamOffset">The current stream offset.</param>
		/// <param name="lineNumber">The current line number.</param>
		public UUValidator (MimeReader reader, long streamOffset, int lineNumber)
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
			get { return ContentEncoding.UUEncode; }
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

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe byte ReadByte (ref byte* inptr)
		{
			byte c = *inptr++;

			streamOffset++;

			if (c == (byte) '\n')
				lineNumber++;

			return c;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void SkipByte (ref byte* inptr)
		{
			byte c = *inptr++;

			streamOffset++;

			if (c == (byte) '\n')
				lineNumber++;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe bool ScanBeginMarker (ref byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == UUValidatorState.ExpectBegin) {
					if (nsaved != 0 && nsaved != (byte) '\n') {
						byte* start = inptr;

						// only lines containing whitespace are allowed before the begin marker
						while (inptr < inend && *inptr != (byte) '\n') {
							if (!(*inptr).IsWhitespace ())
								reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);

							inptr++;
						}

						if (inptr == inend) {
							streamOffset += (int) (inptr - start);
							nsaved = *(inptr - 1);
							return false;
						}

						SkipByte (ref inptr);

						streamOffset += (int) (inptr - start);

						if (inptr == inend)
							return false;
					}

					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'b') {
						// only lines containing whitespace are allowed before the begin marker
						if (!nsaved.IsWhitespace ()) {
							reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
							state = UUValidatorState.ExpectBegin;
							continue;
						}

						continue;
					}

					state = UUValidatorState.B;
					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.B) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'e') {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Be;
					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.Be) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'g') {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Beg;
					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.Beg) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'i') {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Begi;
					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.Begi) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'n') {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Begin;
					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.Begin) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) ' ') {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodePretext, streamOffset, lineNumber);
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.FileMode;
					nsaved = 0;

					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.FileMode) {
					// scan file mode
					while (inptr < inend & *inptr >= (byte) '0' && *inptr <= (byte) '9') {
						streamOffset++;
						nsaved++;
						inptr++;
					}

					if (nsaved > 4) {
						// file mode is too long
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset - nsaved + 4, lineNumber);
					}

					if (inptr == inend)
						return false;

					if (nsaved < 3) {
						// file mode is too short
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset, lineNumber);
					}

					if (nsaved == 3 && *inptr != (byte) ' ') {
						// no space after file mode
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset, lineNumber);
					}

					if (*inptr != (byte) '\n') {
						streamOffset++;
						inptr++;
					}

					state = UUValidatorState.FileName;
					nsaved = 0;

					if (inptr == inend)
						return false;
				}

				if (state == UUValidatorState.FileName) {
					byte* start = inptr;

					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend) {
						// need to keep reading until we hit the end of the line
						streamOffset += (int) (inptr - start);
						return false;
					}

					SkipByte (ref inptr);

					streamOffset += (int) (inptr - start);
					state = UUValidatorState.Payload;
					nsaved = 0;

					return true;
				}
			}

			return false;
		}

#if NET6_0_OR_GREATER
		[SkipLocalsInit]
#endif
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void Validate (byte* input, int length)
		{
			bool last_was_eoln = uulen == 0;
			byte* inend = input + length;
			byte* inptr = input;

			if (state < UUValidatorState.Ended) {
				if (state < UUValidatorState.Payload) {
					if (!ScanBeginMarker (ref inptr, inend))
						return;
				}

				while (inptr < inend) {
					if (*inptr == (byte) '\r') {
						SkipByte (ref inptr);
						continue;
					}

					if (*inptr == (byte) '\n') {
						if (uulen > 0) {
							// incomplete line
							reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodedLineLength, streamOffset, lineNumber);
						}

						last_was_eoln = true;
						SkipByte (ref inptr);
						continue;
					}

					if (last_was_eoln) {
						// first octet on a line is the uulen octet
						uulen = UUDecoder.uudecode_rank[*inptr];
						last_was_eoln = false;
						if (uulen == 0) {
							state = UUValidatorState.Ended;
							SkipByte (ref inptr);
							break;
						}

						SkipByte (ref inptr);
						continue;
					}

					byte c = ReadByte (ref inptr);

					if (uulen > 0) {
						nsaved++;

						if (nsaved == 4) {
							if (uulen >= 3) {
								uulen -= 3;
							} else {
								if (uulen >= 1)
									uulen--;

								if (uulen >= 1)
									uulen--;
							}

							nsaved = 0;
						}
					} else {
						// extra data beyond the end of the uuencoded line
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodedLineLength, streamOffset, lineNumber);
					}
				}
			}

			if (state == UUValidatorState.Ended) {
				while (inptr < inend) {
					byte c = *inptr;

					if (!c.IsWhitespace ())
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber);

					SkipByte (ref inptr);

					if (c == (byte) '\n') {
						state = UUValidatorState.EndedNewLine;
						break;
					}
				}
			}

			if (state == UUValidatorState.EndedNewLine && inptr < inend) {
				if (*inptr != (byte) 'e') {
					reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber);
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.E;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.E && inptr < inend) {
				if (*inptr != (byte) 'n') {
					reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber);
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.En;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.En && inptr < inend) {
				if (*inptr != (byte) 'd') {
					reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber);
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.End;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.End) {
				while (inptr < inend) {
					if (!(*inptr).IsWhitespace ()) {
						reader.OnMimeComplianceViolation (MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber);
						state = UUValidatorState.Invalid;
						return;
					}

					SkipByte (ref inptr);
				}
			}
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

			if (state == UUValidatorState.Invalid)
				return;

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
			// Note: the only valid state to end on is the 'end' state.
			// TODO: report any remaining error(s).
		}
	}
}
