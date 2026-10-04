//
// Base64Validator.cs
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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
#endif

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
		// Classifications for each possible input byte. Combining the base64 alphabet, the padding
		// character, the line breaks and the whitespace into a single table means that the inner loop
		// only needs one table lookup per byte instead of a lookup plus a chain of comparisons.
		const byte Alphabet = 0;
		const byte Padding = 1;
		const byte LineFeed = 2;
		const byte Whitespace = 3;
		const byte Comment = 4;

		static ReadOnlySpan<byte> base64_class => new byte[256] {
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  3,  2,  5,  5,  3,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  3,  5,  5,  5,  5,  5,  5,  5,  5,  5,  4,  0,  5,  5,  5,  0,
			  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  5,  5,  5,  1,  5,  5,
			  5,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
			  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  5,  5,  5,  5,  5,
			  5,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
			  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,
			  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  5
		};

		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		long lineBeginOffset;
		long streamOffset;
		int lineNumber;
		int padding;
		uint total;
		bool invalid;
		// Note: These latch their corresponding violation so that it is reported at most once per
		// line. The number of invalid octets on a line is attacker-controlled, so reporting every one
		// of them would let a crafted part emit an issue per byte of content, swamping every other
		// finding in the report. They are instance fields rather than locals because a malformed line
		// can span multiple Write() calls.
		bool reportedInvalidCharacter;
		bool reportedComment;

#if NET8_0_OR_GREATER
		// The widest vector (in bytes) that Validate() is allowed to use. This allows the unit tests to
		// exercise the narrower (and scalar) code paths on hosts that support wider vectors.
		internal int MaxVectorSize = Vector256.IsHardwareAccelerated ? 32 : Vector128.IsHardwareAccelerated ? 16 : 0;
#endif

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Validator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 validator.
		/// </remarks>
		/// <param name="logger">The compliance logger.</param>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="streamOffset">The current stream offset.</param>
		/// <param name="lineNumber">The current line number.</param>
		public Base64Validator (IMimeComplianceLogger logger, MimeComplianceContext context, long streamOffset, int lineNumber)
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
			get { return ContentEncoding.Base64; }
		}

#if NET8_0_OR_GREATER
		// Update the line tracking state for the line feeds found by one of the SIMD kernels.
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void OnLineFeeds (byte* input, byte* block, uint lineFeeds)
		{
			// Note: For both 16 and 32-byte vectors, the index of the last line feed in the block is
			// 31 - LeadingZeroCount(), so the next line begins at 32 - LeadingZeroCount().
			lineBeginOffset = streamOffset + (block - input) + (32 - BitOperations.LeadingZeroCount (lineFeeds));
			lineNumber += BitOperations.PopCount (lineFeeds);
			reportedInvalidCharacter = false;
			reportedComment = false;
		}

		// Returns a mask of the bytes that are base64 alphabet characters.
		//
		// Note: x86 has no unsigned byte comparisons, so each range check is performed by adding a bias that maps the
		// start of the range to sbyte.MinValue and then doing a single signed comparison against the end of the range.
		// OR'ing with 0x20 maps 'A'-'Z' to 'a'-'z' without mapping any other byte into that range.
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector256<byte> IsAlphabet (Vector256<byte> block)
		{
			var letters = Vector256.LessThan (((block | Vector256.Create ((byte) 0x20)) + Vector256.Create (unchecked ((byte) (0x80 - 'a')))).AsSByte (), Vector256.Create ((sbyte) (-128 + 26)));
			var digits = Vector256.LessThan ((block + Vector256.Create (unchecked ((byte) (0x80 - '0')))).AsSByte (), Vector256.Create ((sbyte) (-128 + 10)));

			return letters.AsByte () | digits.AsByte ()
				| Vector256.Equals (block, Vector256.Create ((byte) '+'))
				| Vector256.Equals (block, Vector256.Create ((byte) '/'));
		}

		// See IsAlphabet(Vector256<byte>) for an explanation of how this works.
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> IsAlphabet (Vector128<byte> block)
		{
			var letters = Vector128.LessThan (((block | Vector128.Create ((byte) 0x20)) + Vector128.Create (unchecked ((byte) (0x80 - 'a')))).AsSByte (), Vector128.Create ((sbyte) (-128 + 26)));
			var digits = Vector128.LessThan ((block + Vector128.Create (unchecked ((byte) (0x80 - '0')))).AsSByte (), Vector128.Create ((sbyte) (-128 + 10)));

			return letters.AsByte () | digits.AsByte ()
				| Vector128.Equals (block, Vector128.Create ((byte) '+'))
				| Vector128.Equals (block, Vector128.Create ((byte) '/'));
		}

		// Validates 32-byte blocks consisting of base64 alphabet characters, line feeds and whitespace. Stops at
		// the first byte that requires special handling ('=', '*' or an invalid character) or when there are
		// fewer than 32 bytes remaining. Returns a pointer to the first byte that was not consumed.
		unsafe byte* ValidateVector256 (byte* input, byte* inptr, byte* inend, ref uint total)
		{
			// Note: Accumulate into a local rather than updating the ref parameter in every iteration of the loop.
			uint n = total;

			do {
				var block = Vector256.Load (inptr);

				var alphabet = IsAlphabet (block);
				uint alphabetMask = alphabet.ExtractMostSignificantBits ();

				// Fast path: blocks consisting entirely of base64 alphabet characters only need to be counted.
				if (alphabetMask == uint.MaxValue) {
					n += 32;
					inptr += 32;
					continue;
				}

				var lineFeed = Vector256.Equals (block, Vector256.Create ((byte) '\n'));
				var whitespace = Vector256.Equals (block, Vector256.Create ((byte) '\r'))
					| Vector256.Equals (block, Vector256.Create ((byte) ' '))
					| Vector256.Equals (block, Vector256.Create ((byte) '\t'));

				uint valid = (alphabet | lineFeed | whitespace).ExtractMostSignificantBits ();
				uint lineFeedMask = lineFeed.ExtractMostSignificantBits ();
				int count = 32;

				if (valid != uint.MaxValue) {
					// Only consume the bytes that precede the first byte that requires special handling.
					count = BitOperations.TrailingZeroCount (~valid);

					uint mask = (1u << count) - 1;

					alphabetMask &= mask;
					lineFeedMask &= mask;
				}

				n += (uint) BitOperations.PopCount (alphabetMask);

				if (lineFeedMask != 0)
					OnLineFeeds (input, inptr, lineFeedMask);

				inptr += count;

				if (count < 32)
					break;
			} while (inend - inptr >= 32);

			total = n;

			return inptr;
		}

		// See ValidateVector256() for an explanation of how this works.
		unsafe byte* ValidateVector128 (byte* input, byte* inptr, byte* inend, ref uint total)
		{
			// Note: Accumulate into a local rather than updating the ref parameter in every iteration of the loop.
			uint n = total;

			do {
				var block = Vector128.Load (inptr);

				var alphabet = IsAlphabet (block);
				uint alphabetMask = alphabet.ExtractMostSignificantBits ();

				if (alphabetMask == 0xFFFF) {
					n += 16;
					inptr += 16;
					continue;
				}

				var lineFeed = Vector128.Equals (block, Vector128.Create ((byte) '\n'));
				var whitespace = Vector128.Equals (block, Vector128.Create ((byte) '\r'))
					| Vector128.Equals (block, Vector128.Create ((byte) ' '))
					| Vector128.Equals (block, Vector128.Create ((byte) '\t'));

				uint valid = (alphabet | lineFeed | whitespace).ExtractMostSignificantBits ();
				uint lineFeedMask = lineFeed.ExtractMostSignificantBits ();
				int count = 16;

				if (valid != 0xFFFF) {
					count = BitOperations.TrailingZeroCount (~valid);

					uint mask = (1u << count) - 1;

					alphabetMask &= mask;
					lineFeedMask &= mask;
				}

				n += (uint) BitOperations.PopCount (alphabetMask);

				if (lineFeedMask != 0)
					OnLineFeeds (input, inptr, lineFeedMask);

				inptr += count;

				if (count < 16)
					break;
			} while (inend - inptr >= 16);

			total = n;

			return inptr;
		}
#endif

#if NET6_0_OR_GREATER
		[SkipLocalsInit]
#endif
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe void Validate (ref byte table, byte* input, int length)
		{
			byte* inend = input + length;
			byte* inptr = input;
			uint n = total;

			if (padding == 0) {
				while (inptr < inend) {
#if NET8_0_OR_GREATER
					// The overwhelming majority of the content consists of base64 alphabet characters and
					// line breaks, so use SIMD to validate them in bulk. The SIMD kernels stop at the first
					// byte that requires special handling, which is then handled by the scalar code below.
					if (MaxVectorSize >= 32 && inend - inptr >= 32)
						inptr = ValidateVector256 (input, inptr, inend, ref n);

					if (MaxVectorSize >= 16 && inend - inptr >= 16)
						inptr = ValidateVector128 (input, inptr, inend, ref n);

					if (inptr == inend)
						break;
#endif
					byte c = *inptr++;
					byte category = Unsafe.Add (ref table, c);

					if (category == Alphabet) {
						n++;
					} else if (category == LineFeed) {
						lineBeginOffset = streamOffset + (inptr - input);
						lineNumber++;
						reportedInvalidCharacter = false;
						reportedComment = false;
					} else if (category == Whitespace) {
						// Whitespace is not part of the encoding, but is harmless.
					} else if (category == Padding) {
						// An '=' char is a valid base64 character, but is special and indicates the end of the content (other than additional padding).
						if (n % 4 < 2) {
							// Padding is only valid in the last 2 positions of the final quantum.
							Log (MimeComplianceViolation.InvalidBase64Padding, streamOffset + (inptr - input) - 1);
							invalid = true;
						} else {
							padding = 1;
							n++;
						}
						break;
					} else if (category == Comment) {
						// RFC 1113 (a Privacy Enhanced Mail specification) allowed for comments in what later became known as "base64 encoding".
						// This was obsoleted in RFC 1421 (which replaced RFC 1113) and RFC 1341 (the first MIME specification) explicitly
						// disallowed it, but some mailers may generate such content. Detect it and report it as a compliance violation.
						if (!reportedComment) {
							reportedComment = true;

							Log (MimeComplianceViolation.ObsoleteBase64Comment, streamOffset + (inptr - input) - 1);
						}
					} else {
						// This is an invalid base64 character.
						if (!reportedInvalidCharacter) {
							reportedInvalidCharacter = true;

							Log (MimeComplianceViolation.InvalidBase64Character, streamOffset + (inptr - input) - 1);
						}
					}
				}
			}

			while (!invalid && inptr < inend) {
				byte c = *inptr++;
				byte category = Unsafe.Add (ref table, c);

				if (category == LineFeed) {
					lineBeginOffset = streamOffset + (inptr - input);
					lineNumber++;
				} else if (category == Padding) {
					n++;

					if (++padding > 2) {
						Log (MimeComplianceViolation.InvalidBase64Padding, streamOffset + (inptr - input) - 1);
						invalid = true;
					}
				} else if (category != Whitespace) {
					Log (MimeComplianceViolation.Base64CharactersAfterPadding, streamOffset + (inptr - input) - 1);
					invalid = true;
				}
			}

			streamOffset += length;
			total = n;
		}

		void Log (MimeComplianceViolation violation, long offset)
		{
			logger.Log (new MimeComplianceIssue (context, violation, offset, lineNumber, GetColumnNumber (offset)));
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

			if (invalid)
				return;

			fixed (byte* inbuf = buffer) {
				ref byte table = ref MemoryMarshal.GetReference (base64_class);

				Validate (ref table, inbuf + startIndex, length);
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
			if (!invalid && total % 4 != 0)
				Log (MimeComplianceViolation.IncompleteBase64Quantum, streamOffset);
		}
	}
}
