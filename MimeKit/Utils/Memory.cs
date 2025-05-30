//
// Memory.cs
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

using System.Runtime.CompilerServices;

#if NETCOREAPP
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace MimeKit.Utils {
	static class Memory
	{
#if NETCOREAPP
		const ulong XorPowerOfTwoToHighByte = (0x07ul | 0x06ul << 8 | 0x05ul << 16 | 0x04ul << 24 | 0x03ul << 32 | 0x02ul << 40 | 0x01ul << 48) + 1;
		static readonly Vector256<byte> Avx2HighBits = Vector256.Create ((byte) 0x80);
		static readonly Vector128<byte> Sse2HighBits = Vector128.Create ((byte) 0x80);
		static readonly Vector<byte> VectorHighBits = new Vector<byte> (0x80);
		static readonly Vector256<byte> Avx2Zero = Vector256<byte>.Zero;
		static readonly Vector128<byte> Sse2Zero = Vector128<byte>.Zero;
		static readonly Vector<byte> VectorZero = Vector<byte>.Zero;
#endif

		/// <summary>
		/// A very fast implementation of an IndexOf()-style method that can also detect bytes with a high bit set or null-bytes along the way.
		/// </summary>
		/// <remarks>
		/// A very fast implementation of an IndexOf()-style method that can also detect bytes with a high bit set or null-bytes along the way.
		/// </remarks>
		/// <param name="searchSpace">The search space.</param>
		/// <param name="length">The length of the search space.</param>
		/// <param name="value">The byte value to search for.</param>
		/// <param name="options">A bitwise set of options specifying classes of bytes to detect along the way.</param>
		/// <param name="detected">A bitwise set of results indicating which classes of bytes were detected.</param>
		/// <returns>The index of the first matching byte; otherwise, <c>-1</c>.</returns>
		public unsafe static int IndexOf (byte* searchSpace, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			DetectionResults mask = (DetectionResults) ((int) options);
			int index;

#if NETCOREAPP
			if (Vector.IsHardwareAccelerated) {
				if (Avx2.IsSupported) {
					// Use the AVX2-specific hardware accelerated vector IndexOf() implementation (faster than SSE2 and generic vector implementations).
					index = Avx2IndexOf (searchSpace, length, value, options, out detected);
				} else if (Sse2.IsSupported) {
					// Use the SSE2-specific hardware accelerated vector IndexOf() implementation (faster than generic vector implementation).
					index = Sse2IndexOf (searchSpace, length, value, options, out detected);
				} else {
					// Use the generic hardware accelerated vector IndexOf() implementation (e.g. ARM64).
					index = VectorIndexOf (searchSpace, length, value, options, out detected);
				}
			} else {
				index = SwAccelIndexOf (searchSpace, length, value, options, out detected);
			}
#else // NETFRAMEWORK
			index = SwAccelIndexOf (searchSpace, length, value, options, out detected);
#endif

			detected &= mask;

			return index;
		}

#if NETCOREAPP
		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		internal static unsafe int Avx2IndexOf (byte* searchSpace, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			nint lengthToExamine = length, offset = 0;
			uint uValue = value;

			detected = DetectionResults.Nothing;

			// Note: We use Vector128 instead of Vector256 for this check because this AVX2 implementation will
			// also use SSE2 if we end up not having enough data available to use the AVX2 vector instructions.
			if (length >= Vector128<byte>.Count * 2) {
				// Calculate the number of bytes to examine before we start processing SSE2 (128-bit) and/or AVX2 (256-bit) vectors.
				lengthToExamine = UnalignedCountVector128 (searchSpace);
			}

			// Use an optimized sequential scan on the first segment of the search space. This will either result in a match -or- offset
			// will be aligned to a 128-bit address boundary.
			int index = PreSequentialScan (searchSpace, ref offset, lengthToExamine, uValue, options, ref detected);

			if (index != -1)
				return index;

			// For improved performance, disable detection options that we have already detected.
			if ((detected & DetectionResults.Detected8Bit) != 0)
				options &= ~DetectionOptions.Detect8Bit;

			if ((detected & DetectionResults.DetectedNulls) != 0)
				options &= ~DetectionOptions.DetectNulls;

			// We only get past the PreSequentialScan if the remaining length is greater than the length of at least 1 Vector128<byte>.
			// After processing Vector128<byte> lengths, we return to PostSequentialScan to finish scanning over any remaining bytes.
			if (offset < (nint) length) {
				// Check if we are aligned to a 256-bit address boundary.
				if ((((nint) (searchSpace + offset)) & ((nint) (Vector256<byte>.Count - 1))) != 0) {
					// Unfortunately, we are not yet aligned to a 256-bit address boundary, so we will need to process
					// 1 Vector128<byte>'s worth of data before we can switch to processing Vector256<byte>'s.
					Vector128<byte> values = Vector128.Create (value);
					Vector128<byte> search = Sse2.LoadAlignedVector128 (searchSpace + offset);
					Vector128<byte> result = Sse2.CompareEqual (values, search);

					// Note that MoveMask will convert the equal vector elements into a set of bit flags.
					// This means that the bit position in 'matches' corresponds to the element offset.
					int matches = Sse2.MoveMask (result);
					if (matches == 0) {
						// No matches found, so check for nulls and 8-bit characters.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Sse2.CompareEqual (search, Sse2Zero);
							if (Sse2.MoveMask (result) != 0) {
								// We found some null bytes, so set the detected flag and disable further null detection.
								detected |= DetectionResults.DetectedNulls;
								options &= ~DetectionOptions.DetectNulls;
							}
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Sse2.And (search, Sse2HighBits);
							if (Sse2.MoveMask (result) != 0) {
								// We found some 8-bit characters, so set the detected flag and disable further 8-bit detection.
								detected |= DetectionResults.Detected8Bit;
								options &= ~DetectionOptions.Detect8Bit;
							}
						}

						// Advance the offset by the size of a Vector128<byte> (16 bytes).
						offset += Vector128<byte>.Count;
					} else {
						// We have matches, so find the index of the first match.
						index = BitOperations.TrailingZeroCount (matches);

						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Sse2.CompareEqual (search, Sse2Zero);
							matches = Sse2.MoveMask (result);

							// Find the index of first zero-byte character.
							int zeroIndex = BitOperations.TrailingZeroCount (matches);

							// We detected a zero-byte only if it occurs before the matched first matched byte.
							if (zeroIndex < index)
								detected |= DetectionResults.DetectedNulls;
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Sse2.And (search, Sse2HighBits);
							matches = Sse2.MoveMask (result);

							// Find the index of first byte with the high bit set.
							int bitIndex = BitOperations.TrailingZeroCount (matches);

							// We detected an 8bit character only if it occurs before the first matched byte.
							if (bitIndex < index)
								detected |= DetectionResults.Detected8Bit;
						}

						return (int) offset + index;
					}
				}

				// Now that we are aligned to a 256-bit address boundary, we can start processing Vector256<byte> vectors.
				lengthToExamine = GetByteVector256SpanLength (offset, length);
				if (offset < lengthToExamine) {
					Vector256<byte> values = Vector256.Create (value);

					do {
						Vector256<byte> search = Avx2.LoadAlignedVector256 (searchSpace + offset);
						Vector256<byte> result = Avx2.CompareEqual (values, search);
						int matches = Avx2.MoveMask (result);

						// Note that MoveMask has converted the matched vector elements into a set of bit flags.
						// This means that the bit position in 'matches' corresponds to the element offset.
						if (matches == 0) {
							// No matches found, so check for nulls and 8-bit characters.
							if ((options & DetectionOptions.DetectNulls) != 0) {
								// Check if any of the bytes in the vector match against 0.
								result = Avx2.CompareEqual (search, Avx2Zero);
								if (Avx2.MoveMask (result) != 0) {
									// We found some null bytes, so set the detected flag and disable further null detection.
									detected |= DetectionResults.DetectedNulls;
									options &= ~DetectionOptions.DetectNulls;
								}
							}

							if ((options & DetectionOptions.Detect8Bit) != 0) {
								// Check if any of the bytes in the vector have their high bit set.
								result = Avx2.And (search, Avx2HighBits);
								if (Avx2.MoveMask (result) != 0) {
									// We found some 8-bit characters, so set the detected flag and disable further 8-bit detection.
									detected |= DetectionResults.Detected8Bit;
									options &= ~DetectionOptions.Detect8Bit;
								}
							}

							// Advance the offset by the size of a Vector256<byte> (32 bytes).
							offset += Vector256<byte>.Count;
							continue;
						}

						// We have matches, so find the index of the first match.
						index = BitOperations.TrailingZeroCount (matches);

						// Now check for nulls and 8-bit characters that occur *before* the matched index.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Avx2.CompareEqual (search, Avx2Zero);
							matches = Avx2.MoveMask (result);

							// Find the index of first zero-byte character.
							int zeroIndex = BitOperations.TrailingZeroCount (matches);

							// We detected a zero-byte only if it occurs before the matched first matched byte.
							if (zeroIndex < index)
								detected |= DetectionResults.DetectedNulls;
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Avx2.And (search, Avx2HighBits);
							matches = Avx2.MoveMask (result);

							// Find the index of first byte with the high bit set.
							int bitIndex = BitOperations.TrailingZeroCount (matches);

							// We detected an 8bit character only if it occurs before the first matched byte.
							if (bitIndex < index)
								detected |= DetectionResults.Detected8Bit;
						}

						return (int) offset + index;
					} while (offset < lengthToExamine);
				}

				// We have run out of room to process Vector256<byte> vectors. Check if we can process a Vector128<byte> vector.
				lengthToExamine = GetByteVector128SpanLength (offset, length);
				if (offset < lengthToExamine) {
					Vector128<byte> values = Vector128.Create (value);
					Vector128<byte> search = Sse2.LoadAlignedVector128 (searchSpace + offset);
					Vector128<byte> result = Sse2.CompareEqual (values, search);

					// Note that MoveMask will convert the matched vector elements into a set of bit flags.
					// This means that the bit position in 'matches' corresponds to the element offset.
					int matches = Sse2.MoveMask (result);
					if (matches == 0) {
						// No matches found, so check for nulls and 8-bit characters.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Sse2.CompareEqual (search, Sse2Zero);
							if (Sse2.MoveMask (result) != 0) {
								// We found some null bytes, so set the detected flag and disable further null detection.
								detected |= DetectionResults.DetectedNulls;
								options &= ~DetectionOptions.DetectNulls;
							}
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Sse2.And (search, Sse2HighBits);
							if (Sse2.MoveMask (result) != 0) {
								// We found some 8-bit characters, so set the detected flag and disable further 8-bit detection.
								detected |= DetectionResults.Detected8Bit;
								options &= ~DetectionOptions.Detect8Bit;
							}
						}

						// Zero flags set so no matches
						offset += Vector128<byte>.Count;
					} else {
						// We have matches, so find the index of the first match.
						index = BitOperations.TrailingZeroCount (matches);

						// Now check for nulls and 8-bit characters that occur *before* the matched index.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Sse2.CompareEqual (search, Sse2Zero);
							matches = Sse2.MoveMask (result);

							// Find the index of first zero-byte character.
							int zeroIndex = BitOperations.TrailingZeroCount (matches);

							// We detected a zero-byte only if it occurs before the matched first matched byte.
							if (zeroIndex < index)
								detected |= DetectionResults.DetectedNulls;
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Sse2.And (search, Sse2HighBits);
							matches = Sse2.MoveMask (result);

							// Find the index of first byte with the high bit set.
							int bitIndex = BitOperations.TrailingZeroCount (matches);

							// We detected an 8bit character only if it occurs before the first matched byte.
							if (bitIndex < index)
								detected |= DetectionResults.Detected8Bit;
						}

						return (int) offset + index;
					}
				}

				if (offset < (nint) length) {
					lengthToExamine = (nint) length - offset;
					return PostSequentialScan (searchSpace, offset, lengthToExamine, uValue, options, ref detected);
				}
			}

			return -1;
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		internal static unsafe int Sse2IndexOf (byte* searchSpace, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			nint lengthToExamine = length, offset = 0;
			uint uValue = value;

			detected = DetectionResults.Nothing;

			if (length >= Vector128<byte>.Count * 2) {
				// Calculate the number of bytes to examine before we start processing SSE2 (128-bit) vectors.
				lengthToExamine = UnalignedCountVector128 (searchSpace);
			}

			// Use an optimized sequential scan on the first segment of the search space. This will either result in a match -or- offset
			// will be aligned to a 128-bit address boundary.
			int index = PreSequentialScan (searchSpace, ref offset, lengthToExamine, uValue, options, ref detected);

			if (index != -1)
				return index;

			// For improved performance, disable detection options that we have already detected.
			if ((detected & DetectionResults.Detected8Bit) != 0)
				options &= ~DetectionOptions.Detect8Bit;

			if ((detected & DetectionResults.DetectedNulls) != 0)
				options &= ~DetectionOptions.DetectNulls;

			// We only get past the PreSequentialScan if the remaining length is greater than the length of at least 1 Vector128<byte>.
			// After processing Vector128<byte> lengths, we return to PostSequentialScan to finish scanning over any remaining bytes.
			if (offset < (nint) length) {
				Vector128<byte> values = Vector128.Create (value);

				lengthToExamine = GetByteVector128SpanLength (offset, length);

				while ((byte*) lengthToExamine > (byte*) offset) {
					Vector128<byte> search = Sse2.LoadAlignedVector128 (searchSpace + offset);
					Vector128<byte> result = Sse2.CompareEqual (values, search);

					// Note that MoveMask will convert the matched vector elements into a set of bit flags.
					// This means that the bit position in 'matches' corresponds to the element offset.
					int matches = Sse2.MoveMask (result);
					if (matches == 0) {
						// No matches found, so check for nulls and 8-bit characters.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							result = Sse2.CompareEqual (search, Sse2Zero);
							if (Sse2.MoveMask (result) != 0) {
								// We found some null bytes, so set the detected flag and disable further null detection.
								detected |= DetectionResults.DetectedNulls;
								options &= ~DetectionOptions.DetectNulls;
							}
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							result = Sse2.And (search, Sse2HighBits);
							if (Sse2.MoveMask (result) != 0) {
								// We found some 8-bit characters, so set the detected flag and disable further 8-bit detection.
								detected |= DetectionResults.Detected8Bit;
								options &= ~DetectionOptions.Detect8Bit;
							}
						}

						// Advance the offset by the size of a Vector128<byte> (16 bytes).
						offset += Vector128<byte>.Count;
						continue;
					}

					// We have matches, so find the index of the first match.
					index = BitOperations.TrailingZeroCount (matches);

					// Now check for nulls and 8-bit characters that occur *before* the matched index.
					if ((options & DetectionOptions.DetectNulls) != 0) {
						// Check if any of the bytes in the vector match against 0.
						result = Sse2.CompareEqual (search, Sse2Zero);
						matches = Sse2.MoveMask (result);

						// Find the index of first zero-byte character.
						int zeroIndex = BitOperations.TrailingZeroCount (matches);

						// We detected a zero-byte only if it occurs before the matched first matched byte.
						if (zeroIndex < index)
							detected |= DetectionResults.DetectedNulls;
					}

					if ((options & DetectionOptions.Detect8Bit) != 0) {
						// Check if any of the bytes in the vector have their high bit set.
						result = Sse2.And (search, Sse2HighBits);
						matches = Sse2.MoveMask (result);

						// Find the index of first byte with the high bit set.
						int bitIndex = BitOperations.TrailingZeroCount (matches);

						// We detected an 8bit character only if it occurs before the first matched byte.
						if (bitIndex < index)
							detected |= DetectionResults.Detected8Bit;
					}

					return (int) offset + index;
				}

				if (offset < (nint) length) {
					lengthToExamine = (nint) length - offset;
					return PostSequentialScan (searchSpace, offset, lengthToExamine, uValue, options, ref detected);
				}
			}

			return -1;
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		internal static unsafe int VectorIndexOf (byte* searchSpace, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			nint lengthToExamine = length;
			nint offset = 0;

			detected = DetectionResults.Nothing;

			if (length >= Vector<byte>.Count * 2) {
				// Calculate the number of bytes to examine before we start processing Vector<byte> vectors (size is hardware dependent).
				lengthToExamine = UnalignedCountVector (searchSpace);
			}

			// Use an optimized sequential scan on the first segment of the search space. This will either result in a match -or- offset
			// will be aligned to an address boundary that is a multiple of the hardware's vector length.
			int index = PreSequentialScan (searchSpace, ref offset, lengthToExamine, value, options, ref detected);

			if (index != -1)
				return index;

			// For improved performance, disable detection options that we have already detected.
			if ((detected & DetectionResults.Detected8Bit) != 0)
				options &= ~DetectionOptions.Detect8Bit;

			if ((detected & DetectionResults.DetectedNulls) != 0)
				options &= ~DetectionOptions.DetectNulls;

			// We only get past the PreSequentialScan if the remaining length is greater than Vector length.
			// After processing Vector lengths we return to PostSequentialScan to finish scanning over any remaining bytes.
			if (offset < (nint) length) {
				Vector<byte> values = new Vector<byte> (value);

				lengthToExamine = GetByteVectorSpanLength (offset, length);

				while ((byte*) lengthToExamine > (byte*) offset) {
					Vector<byte> search = Unsafe.Read<Vector<byte>> (searchSpace + offset);
					Vector<byte> matches = Vector.Equals (search, values);

					// At this point, 'matches' contains non-Zero elements for each matched byte.
					// This means that if 'matches' is all Zeroes, then we have no matches.
					if (Vector<byte>.Zero.Equals (matches)) {
						// No matches found, so check for nulls and 8-bit characters.
						if ((options & DetectionOptions.DetectNulls) != 0) {
							// Check if any of the bytes in the vector match against 0.
							matches = Vector.Equals (search, VectorZero);
							if (!Vector<byte>.Zero.Equals (matches)) {
								// We found some null bytes, so set the detected flag and disable further null detection.
								detected |= DetectionResults.DetectedNulls;
								options &= ~DetectionOptions.DetectNulls;
							}
						}

						if ((options & DetectionOptions.Detect8Bit) != 0) {
							// Check if any of the bytes in the vector have their high bit set.
							matches = Vector.BitwiseAnd (search, VectorHighBits);
							if (!Vector<byte>.Zero.Equals (matches)) {
								// We found some 8-bit characters, so set the detected flag and disable further 8-bit detection.
								detected |= DetectionResults.Detected8Bit;
								options &= ~DetectionOptions.Detect8Bit;
							}
						}

						// Advance the offset by the size of a Vector<byte> (hardware dependent).
						offset += Vector<byte>.Count;
						continue;
					}

					// We have matches, so find the index of the first match.
					index = IndexOfFirstMatch (matches);

					// Now check for nulls and 8-bit characters that occur *before* the matched index.
					if ((options & DetectionOptions.DetectNulls) != 0) {
						// Check if any of the bytes in the vector match against 0.
						matches = Vector.Equals (search, VectorZero);

						// Find the index of first zero-byte character.
						int zeroIndex = IndexOfFirstMatch (matches);

						// We detected a zero-byte only if it occurs before the matched first matched byte.
						if (zeroIndex < index)
							detected |= DetectionResults.DetectedNulls;
					}

					if ((options & DetectionOptions.Detect8Bit) != 0) {
						// Check if any of the bytes in the vector have their high bit set.
						matches = Vector.BitwiseAnd (search, VectorHighBits);

						// Find the index of first byte with the high bit set.
						int bitIndex = IndexOfFirstMatch (matches);

						// We detected an 8bit character only if it occurs before the first matched byte.
						if (bitIndex < index)
							detected |= DetectionResults.Detected8Bit;
					}

					return (int) offset + index;
				}

				if (offset < (nint) length) {
					lengthToExamine = (nint) length - offset;
					return PostSequentialScan (searchSpace, offset, lengthToExamine, value, options, ref detected);
				}
			}

			return -1;
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		static unsafe int PreSequentialScan (byte* searchSpace, ref nint offset, nint length, uint value, DetectionOptions options, ref DetectionResults detected)
		{
			byte* inptr = searchSpace + offset;
			byte* inend = inptr + length;

			// Track the "end" of our search space where we can read 4 bytes at a time. We subtract 3 from the true end of our search space
			// because once we get to that address location, we can no longer read 4 bytes at a time without going out of bounds.
			byte* vend = inend - 3;

			// Calculate our alignment by figuring out the modulus of our pointer address divided by 4.
			// E.g., if inptr is 0x00000001, then alignment will be 1, meaning we are 3 bytes misaligned.
			// ...or if inptr is 0x00000003, then alignment will be 3, meaning we are 1 byte misaligned.
			// ...and if inptr is 0x00000000 or 0x00000004, then alignment will be 0, meaning we are perfectly aligned.
			nint alignment = ((nint) inptr) & 3;

			// Make sure we have enough data left to align to a 4-byte address, otherwise default to the while-loop after this if-statement.
			if ((4 - alignment) < length) {
				// Align to a 4-byte address.
				switch (alignment) {
				case 1: // Advance up to 3 bytes, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) offset;

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					offset++;
					goto case 2;
				case 2: // Advance up to 2 bytes, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) offset;

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					offset++;
					goto case 3;
				case 3: // Advance up to 1 byte, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) offset;

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					offset++;
					break;
				}

				// If inptr is still less than vend, then it means we can start reading 4 bytes at a time for increased performance.
				if (inptr < vend) {
					// Create a bitmask for the value we are searching for that is repeated 4 times (once per byte).
					uint mask = value | (value << 8);
					mask |= mask << 16;

					do {
						// Read 4 bytes into 'dword'.
						uint dword = *(uint*) inptr;

						// XOR the 4 bytes that we just read with the mask we calculated earlier which will result in 0 for each matched byte.
						uint xor = dword ^ mask;

						// We then take the XOR result and check if it contains any bytes that are zero using some bit twiddling:
						//
						// #define haszero(v) (((v) - 0x01010101UL) & ~(v) & 0x80808080UL)
						//
						// The subexpression(v - 0x01010101UL), evaluates to a high bit set in any byte whenever the corresponding byte in v is
						// zero or greater than 0x80. The sub-expression ~v & 0x80808080UL evaluates to high bits set in bytes where the byte of
						// v doesn't have its high bit set (so the byte was less than 0x80). Finally, by ANDing these two sub-expressions the
						// result is the high bits set where the bytes in v were zero, since the high bits set due to a value greater than 0x80
						// in the first sub-expression are masked off by the second.
						//
						// See also: https://graphics.stanford.edu/~seander/bithacks.html#ZeroInWord
						if (((xor - 0x01010101) & (~xor & 0x80808080)) != 0) {
							// The value we are searching for was found in the next 4 bytes. Break out of this loop and then check
							// each individual byte to find the exact location of our search value in the next while-loop.
							break;
						}

						// The value we are searching for was not found in the next 4 bytes, so we can safely use bit masking to check
						// the same next 4 bytes for 8-bit characters and null bytes.
						detected |= (dword & 0x80808080) != 0 ? DetectionResults.Detected8Bit : DetectionResults.Nothing;
						detected |= ((dword - 0x01010101) & (~dword & 0x80808080)) != 0 ? DetectionResults.DetectedNulls : DetectionResults.Nothing;
						offset += 4;
						inptr += 4;
					} while (inptr < vend);
				}
			}

			// Continue searching the remainder of our search space 1 byte at a time until we find our match (if it exists).
			while (inptr < inend) {
				if (*inptr == value)
					return (int) offset;

				if (*inptr > 127)
					detected |= DetectionResults.Detected8Bit;
				else if (*inptr == 0)
					detected |= DetectionResults.DetectedNulls;

				offset++;
				inptr++;
			}

			return -1;
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		static unsafe int PostSequentialScan (byte* searchSpace, nint offset, nint length, uint value, DetectionOptions options, ref DetectionResults detected)
		{
			// Note that in the post-sequential-scan, we do not need to worry about alignment - we are guaranteed to be 4 (or 16 or 32)
			// byte aligned because the AVX2, SSE2 and Vector code-paths have already aligned us to a 4-byte address boundary.
			byte* inptr = searchSpace + offset;
			byte* inend = inptr + length;

			// Track the "end" of our search space where we can read 4 bytes at a time. We subtract 3 from the true end of our search space
			// because once we get to that address location, we can no longer read 4 bytes at a time without going out of bounds.
			byte* vend = inend - 3;

			// Create a bitmask for the value we are searching for that is repeated 4 times (once per byte).
			uint mask = value | (value << 8);
			mask |= mask << 16;

			while (inptr < vend) {
				// Read 4 bytes into 'dword'.
				uint dword = *(uint*) inptr;

				// XOR the 4 bytes that we just read with the mask we calculated earlier which will result in 0 for each matched byte.
				uint xor = dword ^ mask;

				// We then take the XOR result and check if it contains any bytes that are zero using some bit twiddling:
				//
				// #define haszero(v) (((v) - 0x01010101UL) & ~(v) & 0x80808080UL)
				//
				// The subexpression(v - 0x01010101UL), evaluates to a high bit set in any byte whenever the corresponding byte in v is
				// zero or greater than 0x80. The sub-expression ~v & 0x80808080UL evaluates to high bits set in bytes where the byte of
				// v doesn't have its high bit set (so the byte was less than 0x80). Finally, by ANDing these two sub-expressions the
				// result is the high bits set where the bytes in v were zero, since the high bits set due to a value greater than 0x80
				// in the first sub-expression are masked off by the second.
				//
				// See also: https://graphics.stanford.edu/~seander/bithacks.html#ZeroInWord
				if (((xor - 0x01010101) & (~xor & 0x80808080)) != 0) {
					// The value we are searching for was found in the next 4 bytes. Break out of this loop and then check
					// each individual byte to find the exact location of our search value in the next while-loop.
					break;
				}

				// The value we are searching for was not found in the next 4 bytes, so we can safely use bit masking to check
				// the same next 4 bytes for 8-bit characters and null bytes.
				detected |= (dword & 0x80808080) != 0 ? DetectionResults.Detected8Bit : DetectionResults.Nothing;
				detected |= ((dword - 0x01010101) & (~dword & 0x80808080)) != 0 ? DetectionResults.DetectedNulls : DetectionResults.Nothing;
				inptr += 4;
			}

			// Continue searching the remainder of our search space 1 byte at a time until we find our match (if it exists).
			while (inptr < inend) {
				if (*inptr == value)
					return (int) (inptr - searchSpace);

				if (*inptr > 127)
					detected |= DetectionResults.Detected8Bit;
				else if (*inptr == 0)
					detected |= DetectionResults.DetectedNulls;

				inptr++;
			}

			return -1;
		}

		// Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static int IndexOfFirstMatch (Vector<byte> match)
		{
			Vector<ulong> vector64 = Vector.AsVectorUInt64 (match);
			ulong candidate = 0;
			int i = 0;

			// Pattern unrolled by JIT https://github.com/dotnet/coreclr/pull/8001
			for ( ; i < Vector<ulong>.Count; i++) {
				candidate = vector64[i];
				if (candidate != 0)
					break;
			}

			// Single LEA instruction with JIT'd const (using function result)
			return (i * 8) + IndexOfFirstMatch (candidate);
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static int IndexOfFirstMatch (ulong match)
		{
			if (Bmi1.X64.IsSupported)
				return (int) (Bmi1.X64.TrailingZeroCount (match) >> 3);

			// Flag least significant power of two bit
			ulong powerOfTwoFlag = match ^ (match - 1);

			// Shift all powers of two into the high byte and extract
			return (int) ((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe nint GetByteVectorSpanLength (nint offset, int length)
		{
			return (nint) ((length - offset) & ~(Vector<byte>.Count - 1));
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe nint GetByteVector128SpanLength (nint offset, int length)
		{
			return (nint) ((length - offset) & ~(Vector128<byte>.Count - 1));
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe nint GetByteVector256SpanLength (nint offset, int length)
		{
			return (nint) ((length - offset) & ~(Vector256<byte>.Count - 1));
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe nint UnalignedCountVector (byte* searchSpace)
		{
			nint unaligned = ((nint) searchSpace) & (Vector<byte>.Count - 1);
			return (nint) ((Vector<byte>.Count - unaligned) & (Vector<byte>.Count - 1));
		}

		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe nint UnalignedCountVector128 (byte* searchSpace)
		{
			nint unaligned = ((nint) searchSpace) & (Vector128<byte>.Count - 1);
			return (nint) ((Vector128<byte>.Count - unaligned) & (Vector128<byte>.Count - 1));
		}
#endif // NETCOREAPP

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		internal unsafe static int SwAccelIndexOf (byte* searchSpace, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			DetectionResults detectedMask = (DetectionResults) ((int) options);
			byte* inend = searchSpace + length;
			byte* inptr = searchSpace;

			// Track the "end" of our search space where we can read 4 bytes at a time. We subtract 3 from the true end of our search space
			// because once we get to that address location, we can no longer read 4 bytes at a time without going out of bounds.
			byte* vend = inend - 3;

			// Calculate our alignment by figuring out the modulus of our pointer address divided by 4.
			// E.g., if inptr is 0x00000001, then alignment will be 1, meaning we are 3 bytes misaligned.
			// ...or if inptr is 0x00000003, then alignment will be 3, meaning we are 1 byte misaligned.
			// ...and if inptr is 0x00000000 or 0x00000004, then alignment will be 0, meaning we are perfectly aligned.
			nint alignment = ((nint) inptr) & 3;

			detected = DetectionResults.Nothing;

			// Make sure we have enough data left to align to a 4-byte address, otherwise default to the while-loop after this if-statement.
			if ((4 - alignment) < length) {
				// Align to a 4-byte address.
				switch (alignment) {
				case 1: // Advance up to 3 bytes, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) (inptr - searchSpace);

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					goto case 2;
				case 2: // Advance up to 2 bytes, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) (inptr - searchSpace);

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					goto case 3;
				case 3: // Advance up to 1 byte, checking for 'uValue', 8-bit bytes and 0 along the way.
					if (*inptr == value)
						return (int) (inptr - searchSpace);

					if (*inptr > 127)
						detected |= DetectionResults.Detected8Bit;
					else if (*inptr == 0)
						detected |= DetectionResults.DetectedNulls;

					inptr++;
					break;
				}

				// If inptr is still less than vend, then it means we can start reading 4 bytes at a time for increased performance.
				if (inptr < vend) {
					// Create a bitmask for the value we are searching for that is repeated 4 times (once per byte).
					uint mask = value;
					mask |= mask << 8;
					mask |= mask << 16;

					do {
						// Read 4 bytes into 'dword'.
						uint dword = *(uint*) inptr;

						// XOR the 4 bytes that we just read with the mask we calculated earlier which will result in 0 for each matched byte.
						uint xor = dword ^ mask;

						// We then take the XOR result and check if it contains any bytes that are zero using some bit twiddling:
						//
						// #define haszero(v) (((v) - 0x01010101UL) & ~(v) & 0x80808080UL)
						//
						// The subexpression(v - 0x01010101UL), evaluates to a high bit set in any byte whenever the corresponding byte in v is
						// zero or greater than 0x80. The sub-expression ~v & 0x80808080UL evaluates to high bits set in bytes where the byte of
						// v doesn't have its high bit set (so the byte was less than 0x80). Finally, by ANDing these two sub-expressions the
						// result is the high bits set where the bytes in v were zero, since the high bits set due to a value greater than 0x80
						// in the first sub-expression are masked off by the second.
						//
						// See also: https://graphics.stanford.edu/~seander/bithacks.html#ZeroInWord
						if (((xor - 0x01010101) & (~xor & 0x80808080)) != 0) {
							// The value we are searching for was found in the next 4 bytes. Break out of this loop and then check
							// each individual byte to find the exact location of our search value in the next while-loop.
							break;
						}

						// The value we are searching for was not found in the next 4 bytes, so we can safely use bit masking to check
						// the same next 4 bytes for 8-bit characters and null bytes.
						detected |= (dword & 0x80808080) != 0 ? DetectionResults.Detected8Bit : DetectionResults.Nothing;
						detected |= ((dword - 0x01010101) & (~dword & 0x80808080)) != 0 ? DetectionResults.DetectedNulls : DetectionResults.Nothing;
						inptr += 4;
					} while (inptr < vend);
				}
			}

			// Continue searching the remainder of our search space 1 byte at a time until we find our match (if it exists).
			while (inptr < inend) {
				if (*inptr == value)
					return (int) (inptr - searchSpace);

				if (*inptr > 127)
					detected |= DetectionResults.Detected8Bit;
				else if (*inptr == 0)
					detected |= DetectionResults.DetectedNulls;

				inptr++;
			}

			return -1;
		}
	}
}
