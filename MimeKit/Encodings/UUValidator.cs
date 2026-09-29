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

		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		long lineBeginOffset;
		long streamOffset;
		int lineNumber;
		UUValidatorState state;
		bool invalidPretext;
		bool invalidFileMode;
		// Note: These latch their corresponding violation so that it is reported at most once per
		// uuencoded line. The number of malformed octets on a line is attacker-controlled, so
		// reporting every one of them would let a crafted part emit an issue per byte of content,
		// swamping every other finding in the report. They are instance fields rather than locals
		// because a malformed line can span multiple Write() calls.
		bool reportedInvalidContent;
		bool reportedExtraData;
		bool eoln;
		byte nsaved;
		byte uulen;

		/// <summary>
		/// Initialize a new instance of the <see cref="UUValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix validator.
		/// </remarks>
		/// <param name="logger">The compliance logger.</param>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="streamOffset">The current stream offset.</param>
		/// <param name="lineNumber">The current line number.</param>
		public UUValidator (IMimeComplianceLogger logger, MimeComplianceContext context, long streamOffset, int lineNumber)
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
			get { return ContentEncoding.UUEncode; }
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe byte ReadByte (ref byte* inptr)
		{
			byte c = *inptr++;

			streamOffset++;

			if (c == (byte) '\n') {
				lineBeginOffset = streamOffset;
				lineNumber++;
			}

			return c;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void SkipByte (ref byte* inptr)
		{
			byte c = *inptr++;

			streamOffset++;

			if (c == (byte) '\n') {
				lineBeginOffset = streamOffset;
				lineNumber++;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void SkipToLineFeed (ref byte* inptr, byte* inend)
		{
			int index = new ReadOnlySpan<byte> (inptr, (int) (inend - inptr)).IndexOf ((byte) '\n');
			int count = index < 0 ? (int) (inend - inptr) : index;

			streamOffset += count;
			inptr += count;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe bool ScanBeginMarker (ref byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == UUValidatorState.ExpectBegin) {
					if (nsaved != 0 && nsaved != (byte) '\n') {
						// skip ahead to the next line...
						SkipToLineFeed (ref inptr, inend);

						if (inptr == inend) {
							nsaved = *(inptr - 1);
							break;
						}

						nsaved = ReadByte (ref inptr);

						if (inptr == inend)
							break;
					}

					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'b') {
						// only lines containing whitespace are allowed before the begin marker
						if (!nsaved.IsWhitespace ()) {
							if (!invalidPretext) {
								logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
								invalidPretext = true;
							}
							state = UUValidatorState.ExpectBegin;
							continue;
						}

						continue;
					}

					state = UUValidatorState.B;
					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.B) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'e') {
						if (!invalidPretext) {
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
							invalidPretext = true;
						}
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Be;
					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.Be) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'g') {
						if (!invalidPretext) {
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
							invalidPretext = true;
						}
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Beg;
					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.Beg) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'i') {
						if (!invalidPretext) {
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
							invalidPretext = true;
						}
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Begi;
					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.Begi) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) 'n') {
						if (!invalidPretext) {
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
							invalidPretext = true;
						}
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.Begin;
					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.Begin) {
					nsaved = ReadByte (ref inptr);
					if (nsaved != (byte) ' ') {
						if (!invalidPretext) {
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodePretext, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
							invalidPretext = true;
						}
						state = UUValidatorState.ExpectBegin;
						continue;
					}

					state = UUValidatorState.FileMode;
					nsaved = 0;

					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.FileMode) {
					// scan file mode
					while (inptr < inend && *inptr >= (byte) '0' && *inptr <= (byte) '9') {
						streamOffset++;
						nsaved++;
						inptr++;
					}

					if (!invalidFileMode && nsaved > 4) {
						// file mode is too long
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset - nsaved + 4, lineNumber, GetColumnNumber (streamOffset - nsaved + 4)));
						invalidFileMode = true;
					}

					if (inptr == inend)
						break;

					if (!invalidFileMode && nsaved < 3) {
						// file mode is too short
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
						invalidFileMode = true;
					}

					// scan ahead for the space after the file mode
					while (inptr < inend && *inptr != (byte) ' ') {
						if (!invalidFileMode) {
							// invalid character in file mode
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeFileMode, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
							invalidFileMode = true;
						}

						if (*inptr == (byte) '\n')
							break;

						streamOffset++;
						inptr++;
					}

					if (inptr == inend)
						break;

					if (*inptr != (byte) '\n') {
						streamOffset++;
						inptr++;
					}

					state = UUValidatorState.FileName;
					nsaved = 0;

					if (inptr == inend)
						break;
				}

				if (state == UUValidatorState.FileName) {
					SkipToLineFeed (ref inptr, inend);

					if (inptr == inend) {
						// need to keep reading until we hit the end of the line
						break;
					}

					SkipByte (ref inptr);

					state = UUValidatorState.Payload;
					eoln = true;
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
			byte* inend = input + length;
			byte* inptr = input;

			if (state < UUValidatorState.Ended) {
				if (state < UUValidatorState.Payload) {
					if (!ScanBeginMarker (ref inptr, inend))
						return;
				}

				if (state == UUValidatorState.Payload) {
					while (inptr < inend) {
						// Note: Every byte in the 33..96 range is a valid payload character and the uulen
						// octet states exactly how many of them the line is supposed to contain, so a run
						// of them can be consumed in bulk. This avoids having to do the range check and
						// the quantum bookkeeping one byte at a time.
						// Note: When uulen is 0, the rest of the line is data beyond the declared length.
						// Unless that has already been reported, each of those octets has to go through the
						// per-octet path, so there is nothing to gain from scanning ahead here.
						if (!eoln && (uulen > 0 || reportedExtraData)) {
							int remaining = (int) (inend - inptr);
							int count;

							// Note: Once the content violation has been reported for this line, the octets
							// that follow no longer need to be classified: the quantum bookkeeping below is
							// the same whether or not an octet is a valid payload character, so the scan can
							// run to the end of the line instead of stopping on every invalid octet. Without
							// this, a line of invalid octets re-enters the scan once per byte having made no
							// progress.
#if NET8_0_OR_GREATER
							var span = new ReadOnlySpan<byte> (inptr, remaining);
							int index = reportedInvalidContent
								? span.IndexOfAny ((byte) '\r', (byte) '\n')
								: span.IndexOfAnyExceptInRange ((byte) 33, (byte) 96);

							count = index < 0 ? remaining : index;
#else
							count = 0;

							if (reportedInvalidContent) {
								while (count < remaining && inptr[count] != (byte) '\r' && inptr[count] != (byte) '\n')
									count++;
							} else {
								while (count < remaining && inptr[count] >= 33 && inptr[count] <= 96)
									count++;
							}
#endif

							if (uulen > 0) {
								// Each group of 4 characters encodes 3 octets, so this is the number of
								// characters still needed in order to complete the current line.
								int needed = (4 * ((uulen + 2) / 3)) - nsaved;

								if (count > needed)
									count = needed;

								if (count > 0) {
									int groups = (nsaved + count) / 4;

									nsaved = (byte) ((nsaved + count) % 4);
									uulen = (byte) (uulen >= 3 * groups ? uulen - 3 * groups : 0);
									streamOffset += count;
									inptr += count;
									continue;
								}
							} else if (count > 0) {
								// Note: Everything left on this line is data beyond the declared length and
								// that has already been reported, so it can be skipped outright. The per-octet
								// path leaves nsaved alone once uulen reaches 0, so there is no bookkeeping
								// to preserve here.
								streamOffset += count;
								inptr += count;
								continue;
							}
						}

						if (*inptr == (byte) '\r') {
							SkipByte (ref inptr);
							continue;
						}

						if (*inptr == (byte) '\n') {
							if (uulen > 0) {
								// incomplete line
								logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.IncompleteUUEncodedLine, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
							}

							SkipByte (ref inptr);
							eoln = true;
							continue;
						}

						if (eoln) {
							// first octet on a line is the uulen octet
							eoln = false;
							reportedInvalidContent = false;
							reportedExtraData = false;

							if (*inptr == (byte) '`') {
								state = UUValidatorState.Ended;
								SkipByte (ref inptr);
								break;
							}

							uulen = UUDecoder.uudecode_rank[*inptr];

							if (uulen > 45) {
								// invalid line length
								logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodedLineLength, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
							}

							SkipByte (ref inptr);
							continue;
						}

						byte c = ReadByte (ref inptr);

						if ((c < 33 || c > 96) && !reportedInvalidContent) {
							reportedInvalidContent = true;

							// invalid character in uuencoded payload
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodedContent, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
						}

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
						} else if (!reportedExtraData) {
							reportedExtraData = true;

							// extra data beyond the end of the uuencoded line
							logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodedLineExtraData, streamOffset - 1, lineNumber, GetColumnNumber (streamOffset - 1)));
						}
					}
				}
			}

			if (state == UUValidatorState.Ended) {
				while (inptr < inend) {
					byte c = *inptr;

					if (!c.IsWhitespace ())
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber, GetColumnNumber (streamOffset)));

					SkipByte (ref inptr);

					if (c == (byte) '\n') {
						state = UUValidatorState.EndedNewLine;
						break;
					}
				}
			}

			if (state == UUValidatorState.EndedNewLine && inptr < inend) {
				if (*inptr != (byte) 'e') {
					logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.E;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.E && inptr < inend) {
				if (*inptr != (byte) 'n') {
					logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.En;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.En && inptr < inend) {
				if (*inptr != (byte) 'd') {
					logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
					state = UUValidatorState.Invalid;
					return;
				}

				state = UUValidatorState.End;
				SkipByte (ref inptr);
			}

			if (state == UUValidatorState.End) {
				while (inptr < inend) {
					if (!(*inptr).IsWhitespace ()) {
						logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.InvalidUUEncodeEndMarker, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
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
			ArgumentValidator.Validate (buffer, startIndex, length);

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
			if (state < UUValidatorState.End)
				logger.Log (new MimeComplianceIssue (context, MimeComplianceViolation.IncompleteUUEncodedContent, streamOffset, lineNumber, GetColumnNumber (streamOffset)));
		}
	}
}
