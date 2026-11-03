//
// Utf8.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
// Copyright (c) 2023 SimdUnicode authors
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

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace MimeKit.Utils {
	static class Utf8
	{
		/// <summary>
		/// Validate that the value is well-formed UTF-8.
		/// </summary>
		/// <remarks>
		/// Validates that the value is well-formed UTF-8.
		/// </remarks>
		/// <param name="value">The byte string.</param>
		/// <returns><see langword="true"/> if the value is well-formed UTF-8; otherwise, <see langword="false"/>.</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static bool IsValid (ReadOnlySpan<byte> value)
		{
#if NET8_0_OR_GREATER
			return System.Text.Unicode.Utf8.IsValid (value);
#else
			return IsValidCore (value);
#endif
		}

		/// <summary>
		/// Validate that the value is well-formed UTF-8.
		/// </summary>
		/// <remarks>
		/// <para>Validates that the value is well-formed UTF-8 as defined by Table 3-7 ("Well-Formed UTF-8
		/// Byte Sequences") of the Unicode Standard.</para>
		/// <para>This is the managed fallback used on target frameworks that do not provide
		/// <c>System.Text.Unicode.Utf8.IsValid()</c>. It is compiled on all target frameworks so that it
		/// can be unit tested (and compared against the framework implementation) everywhere.</para>
		/// <note type="note">Overlong encodings, UTF-16 surrogate values (<c>U+D800</c> through <c>U+DFFF</c>)
		/// and scalar values above <c>U+10FFFF</c> are all rejected. This matters because such encodings are
		/// exactly the sort of thing used to smuggle content past scanners that decode them leniently.</note>
		/// </remarks>
		/// <param name="value">The byte string.</param>
		/// <returns><see langword="true"/> if the value is well-formed UTF-8; otherwise, <see langword="false"/>.</returns>
		internal static bool IsValidCore (ReadOnlySpan<byte> value)
		{
			return IndexOfInvalidSequenceCore (value) == -1;
		}

		/// <summary>
		/// Find the start of the first malformed UTF-8 sequence.
		/// </summary>
		/// <param name="value">The byte string.</param>
		/// <returns>The sequence's leading-byte index, or -1 if the value is well-formed UTF-8.</returns>
		public static int IndexOfInvalidSequence (ReadOnlySpan<byte> value)
		{
#if NET8_0_OR_GREATER
			if (value.Length >= 128) {
				if (Avx2.IsSupported)
					return IndexOfInvalidSequenceAvx2 (value);
				if (Ssse3.IsSupported && Sse41.IsSupported)
					return IndexOfInvalidSequenceSse (value);
				if (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian)
					return IndexOfInvalidSequenceArm64 (value);
			}

			if (System.Text.Unicode.Utf8.IsValid (value))
				return -1;
#endif

			return IndexOfInvalidSequenceCore (value);
		}

#if NET8_0_OR_GREATER
		// Lookup algorithm adapted from simdutf/SimdUnicode (MIT),
		// revision d20b63710e8893dede79890f29fd5ae6745d3e2d. No character counts are needed here.
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void GetLookupTables (out Vector128<byte> high, out Vector128<byte> low, out Vector128<byte> current)
		{
			// Intersect the previous byte's two nibble masks with the current byte's high-nibble mask.
			// Bits 0-6 flag invalid byte pairs; bit 7 marks adjacent continuation bytes and is
			// checked against the continuation requirements of the preceding three-/four-byte sequence.
			high = Vector128.Create (
				(byte) 2, 2, 2, 2, 2, 2, 2, 2, 128, 128, 128, 128, 33, 1, 21, 73);
			low = Vector128.Create (
				(byte) 0xe7, 0xa3, 0x83, 0x83, 0x8b, 0xcb, 0xcb, 0xcb,
				0xcb, 0xcb, 0xcb, 0xcb, 0xcb, 0xdb, 0xcb, 0xcb);
			current = Vector128.Create (
				(byte) 1, 1, 1, 1, 1, 1, 1, 1, 0xe6, 0xae, 0xba, 0xba, 1, 1, 1, 1);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static int IndexOfInvalidSequenceTail (ReadOnlySpan<byte> value, int offset, bool incomplete)
		{
			if (incomplete) {
				// The first error or the scalar tail may belong to a sequence starting in the preceding block.
				offset -= 3;

				while (offset > 0 && (value[offset] & 0xc0) == 0x80)
					offset--;
			}

			int invalid = IndexOfInvalidSequenceCore (value.Slice (offset));

			return invalid < 0 ? -1 : offset + invalid;
		}

		internal static unsafe int IndexOfInvalidSequenceAvx2 (ReadOnlySpan<byte> value)
		{
			GetLookupTables (out var highLookup, out var lowLookup, out var currentLookup);
			var highTable = Vector256.Create (highLookup, highLookup);
			var lowTable = Vector256.Create (lowLookup, lowLookup);
			var currentTable = Vector256.Create (currentLookup, currentLookup);
			// Only the last three lanes can carry an unfinished sequence into the next block.
			var maxValue = Vector256.Create (
				(byte) 255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 0xef, 0xdf, 0xbf);
			var lowNibble = Vector256.Create ((byte) 0x0f);
			var highBit = Vector256.Create ((byte) 0x80);
			var thirdByte = Vector256.Create ((byte) 0x60);
			var fourthByte = Vector256.Create ((byte) 0x70);
			var previous = Vector256<byte>.Zero;
			var incomplete = Vector256<byte>.Zero;
			int offset = 0;

			fixed (byte* input = value) {
				while (offset <= value.Length - 32) {
					var current = Vector256.Load (input + offset);

					if (Avx2.MoveMask (current) == 0) {
						if (!Avx2.TestZ (incomplete, incomplete))
							break;

						incomplete = Vector256<byte>.Zero;
					} else {
						var bridge = Avx2.Permute2x128 (previous, current, 0x21);
						var previous1 = Avx2.AlignRight (current, bridge, 15);
						var previous2 = Avx2.AlignRight (current, bridge, 14);
						var previous3 = Avx2.AlignRight (current, bridge, 13);
						var high = Avx2.Shuffle (highTable, Avx2.ShiftRightLogical (previous1.AsUInt16 (), 4).AsByte () & lowNibble);
						var low = Avx2.Shuffle (lowTable, previous1 & lowNibble);
						var next = Avx2.Shuffle (currentTable, Avx2.ShiftRightLogical (current.AsUInt16 (), 4).AsByte () & lowNibble);
						var required = (Avx2.SubtractSaturate (previous2, thirdByte) | Avx2.SubtractSaturate (previous3, fourthByte)) & highBit;
						var errors = (high & low & next) ^ required;

						if (!Avx2.TestZ (errors, errors))
							break;

						incomplete = Avx2.SubtractSaturate (current, maxValue);
					}

					previous = current;
					offset += 32;
				}
			}

			return IndexOfInvalidSequenceTail (value, offset, !Avx2.TestZ (incomplete, incomplete));
		}

		internal static unsafe int IndexOfInvalidSequenceSse (ReadOnlySpan<byte> value)
		{
			GetLookupTables (out var highTable, out var lowTable, out var currentTable);
			var maxValue = Vector128.Create (
				(byte) 255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 0xef, 0xdf, 0xbf);
			var lowNibble = Vector128.Create ((byte) 0x0f);
			var highBit = Vector128.Create ((byte) 0x80);
			var thirdByte = Vector128.Create ((byte) 0x60);
			var fourthByte = Vector128.Create ((byte) 0x70);
			var previous = Vector128<byte>.Zero;
			var incomplete = Vector128<byte>.Zero;
			int offset = 0;

			fixed (byte* input = value) {
				while (offset <= value.Length - 16) {
					var current = Sse2.LoadVector128 (input + offset);

					if (Sse2.MoveMask (current) == 0) {
						if (!Sse41.TestZ (incomplete, incomplete))
							break;

						incomplete = Vector128<byte>.Zero;
					} else {
						var previous1 = Ssse3.AlignRight (current, previous, 15);
						var previous2 = Ssse3.AlignRight (current, previous, 14);
						var previous3 = Ssse3.AlignRight (current, previous, 13);
						var high = Ssse3.Shuffle (highTable, Sse2.ShiftRightLogical (previous1.AsUInt16 (), 4).AsByte () & lowNibble);
						var low = Ssse3.Shuffle (lowTable, previous1 & lowNibble);
						var next = Ssse3.Shuffle (currentTable, Sse2.ShiftRightLogical (current.AsUInt16 (), 4).AsByte () & lowNibble);
						var required = (Sse2.SubtractSaturate (previous2, thirdByte) | Sse2.SubtractSaturate (previous3, fourthByte)) & highBit;
						var errors = (high & low & next) ^ required;

						if (!Sse41.TestZ (errors, errors))
							break;

						incomplete = Sse2.SubtractSaturate (current, maxValue);
					}

					previous = current;
					offset += 16;
				}
			}

			return IndexOfInvalidSequenceTail (value, offset, !Sse41.TestZ (incomplete, incomplete));
		}

		internal static unsafe int IndexOfInvalidSequenceArm64 (ReadOnlySpan<byte> value)
		{
			GetLookupTables (out var highTable, out var lowTable, out var currentTable);
			var maxValue = Vector128.Create (
				(byte) 255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 0xef, 0xdf, 0xbf);
			var lowNibble = Vector128.Create ((byte) 0x0f);
			var highBit = Vector128.Create ((byte) 0x80);
			var thirdByte = Vector128.Create ((byte) 0x60);
			var fourthByte = Vector128.Create ((byte) 0x70);
			var previous = Vector128<byte>.Zero;
			var incomplete = Vector128<byte>.Zero;
			int offset = 0;

			fixed (byte* input = value) {
				while (offset <= value.Length - 16) {
					var current = AdvSimd.LoadVector128 (input + offset);

					if ((current & highBit) == Vector128<byte>.Zero) {
						if (incomplete != Vector128<byte>.Zero)
							break;

						incomplete = Vector128<byte>.Zero;
					} else {
						var previous1 = AdvSimd.ExtractVector128 (previous, current, 15);
						var previous2 = AdvSimd.ExtractVector128 (previous, current, 14);
						var previous3 = AdvSimd.ExtractVector128 (previous, current, 13);
						var high = AdvSimd.Arm64.VectorTableLookup (highTable, AdvSimd.ShiftRightLogical (previous1.AsUInt16 (), 4).AsByte () & lowNibble);
						var low = AdvSimd.Arm64.VectorTableLookup (lowTable, previous1 & lowNibble);
						var next = AdvSimd.Arm64.VectorTableLookup (currentTable, AdvSimd.ShiftRightLogical (current.AsUInt16 (), 4).AsByte () & lowNibble);
						var required = (AdvSimd.SubtractSaturate (previous2, thirdByte) | AdvSimd.SubtractSaturate (previous3, fourthByte)) & highBit;
						var errors = (high & low & next) ^ required;

						if (errors != Vector128<byte>.Zero)
							break;

						incomplete = AdvSimd.SubtractSaturate (current, maxValue);
					}

					previous = current;
					offset += 16;
				}
			}

			return IndexOfInvalidSequenceTail (value, offset, incomplete != Vector128<byte>.Zero);
		}
#endif

		internal static int IndexOfInvalidSequenceCore (ReadOnlySpan<byte> value)
		{
			int index = 0;

			while (index < value.Length) {
				byte b0 = value[index];

				if (b0 < 0x80) {
					// 1-byte sequence: U+0000 -> U+007F
					index++;
					continue;
				}

				byte lower, upper;
				int length;

				if (b0 < 0xC2) {
					// 0x80 -> 0xBF are continuation bytes (which cannot start a sequence) and
					// 0xC0 -> 0xC1 could only ever be the start of an overlong encoding.
					return index;
				} else if (b0 < 0xE0) {
					// 2-byte sequence: U+0080 -> U+07FF
					length = 2;
					lower = 0x80;
					upper = 0xBF;
				} else if (b0 < 0xF0) {
					// 3-byte sequence: U+0800 -> U+FFFF
					length = 3;

					if (b0 == 0xE0) {
						// Exclude overlong encodings of U+0000 -> U+07FF.
						lower = 0xA0;
						upper = 0xBF;
					} else if (b0 == 0xED) {
						// Exclude the UTF-16 surrogate range, U+D800 -> U+DFFF.
						lower = 0x80;
						upper = 0x9F;
					} else {
						lower = 0x80;
						upper = 0xBF;
					}
				} else if (b0 < 0xF5) {
					// 4-byte sequence: U+10000 -> U+10FFFF
					length = 4;

					if (b0 == 0xF0) {
						// Exclude overlong encodings of U+0000 -> U+FFFF.
						lower = 0x90;
						upper = 0xBF;
					} else if (b0 == 0xF4) {
						// Exclude scalar values above U+10FFFF.
						lower = 0x80;
						upper = 0x8F;
					} else {
						lower = 0x80;
						upper = 0xBF;
					}
				} else {
					// 0xF5 -> 0xFF could only ever encode a scalar value above U+10FFFF.
					return index;
				}

				// Make sure the sequence isn't truncated.
				if (value.Length - index < length)
					return index;

				// The range of valid values for the first continuation byte depends on the leading byte.
				byte b1 = value[index + 1];

				if (b1 < lower || b1 > upper)
					return index;

				// Any remaining continuation bytes simply need to be in the range 0x80 -> 0xBF.
				for (int i = 2; i < length; i++) {
					if ((value[index + i] & 0xC0) != 0x80)
						return index;
				}

				index += length;
			}

			return -1;
		}
	}
}
