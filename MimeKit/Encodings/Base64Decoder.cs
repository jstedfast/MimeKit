//
// Base64Decoder.cs
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

#if NET6_0_OR_GREATER
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the base64 encoding.
	/// </summary>
	/// <remarks>
	/// Base64 is an encoding often used in MIME to encode binary content such
	/// as images and other types of multimedia to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class Base64Decoder : IMimeDecoder
	{
		static ReadOnlySpan<byte> base64_rank => new byte[256] {
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255, 62,255,255,255, 63,
			 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,255,255,255,  0,255,255,
			255,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
			 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,255,255,255,255,255,
			255, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
			 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
		};

#if NET6_0_OR_GREATER
		static ReadOnlySpan<sbyte> DecodingMap => new sbyte[256] {
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1, -1, 63,
			52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
			-1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
			15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
			-1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
			41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		};

		static ReadOnlySpan<uint> VbmiLookup0 => new uint[] {
			0x80808080, 0x80808080, 0x80808080, 0x80808080,
			0x80808080, 0x80808080, 0x80808080, 0x80808080,
			0x80808080, 0x80808080, 0x3e808080, 0x3f808080,
			0x37363534, 0x3b3a3938, 0x80803d3c, 0x80808080
		};

		static ReadOnlySpan<uint> VbmiLookup1 => new uint[] {
			0x02010080, 0x06050403, 0x0a090807, 0x0e0d0c0b,
			0x1211100f, 0x16151413, 0x80191817, 0x80808080,
			0x1c1b1a80, 0x201f1e1d, 0x24232221, 0x28272625,
			0x2c2b2a29, 0x302f2e2d, 0x80333231, 0x80808080
		};

		static ReadOnlySpan<sbyte> Avx2LutHigh => new sbyte[] {
			0x10, 0x10, 0x01, 0x02,
			0x04, 0x08, 0x04, 0x08,
			0x10, 0x10, 0x10, 0x10,
			0x10, 0x10, 0x10, 0x10,
			0x10, 0x10, 0x01, 0x02,
			0x04, 0x08, 0x04, 0x08,
			0x10, 0x10, 0x10, 0x10,
			0x10, 0x10, 0x10, 0x10
		};

		static ReadOnlySpan<sbyte> Avx2LutLow => new sbyte[] {
			0x15, 0x11, 0x11, 0x11,
			0x11, 0x11, 0x11, 0x11,
			0x11, 0x11, 0x13, 0x1A,
			0x1B, 0x1B, 0x1B, 0x1A,
			0x15, 0x11, 0x11, 0x11,
			0x11, 0x11, 0x11, 0x11,
			0x11, 0x11, 0x13, 0x1A,
			0x1B, 0x1B, 0x1B, 0x1A
		};

		static ReadOnlySpan<sbyte> Avx2LutShift => new sbyte[] {
			0, 16, 19, 4,
			-65, -65, -71, -71,
			0, 0, 0, 0,
			0, 0, 0, 0,
			0, 16, 19, 4,
			-65, -65, -71, -71,
			0, 0, 0, 0,
			0, 0, 0, 0
		};

		static ReadOnlySpan<int> Vector128LutHigh => new int[] { 0x02011010, 0x08040804, 0x10101010, 0x10101010 };

		static ReadOnlySpan<int> Vector128LutLow => new int[] { 0x11111115, 0x11111111, 0x1A131111, 0x1A1B1B1B };

		static ReadOnlySpan<uint> Vector128LutShift => new uint[] { 0x04131000, 0xb9b9bfbf, 0x00000000, 0x00000000 };

		static ReadOnlySpan<uint> AdvSimdLutOne3 => new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0x3EFFFFFF, 0x3FFFFFFF };

		const uint AdvSimdLutTwo3Uint1 = 0x1B1AFFFF;
#endif

		int previous;
		uint saved;
		byte bytes;

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Decoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 decoder.
		/// </remarks>
		public Base64Decoder ()
		{
		}

		/// <summary>
		/// Clone the <see cref="Base64Decoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Base64Decoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="Base64Decoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			return new Base64Decoder {
				previous = previous,
				saved = saved,
				bytes = bytes
			};
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the decoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Base64; }
		}

		/// <summary>
		/// Estimate the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to decode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			// may require up to 3 padding bytes
			return (inputLength / 4) * 3 + 3;
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
				throw new ArgumentException ("The output buffer is not large enough to contain the decoded input.", nameof (output));
		}

#if NET6_0_OR_GREATER
		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int HwAccelDecode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			int remainingInput;
			byte rank, c;

			if (bytes > 0) {
				// Flush out any partially read data from a previous iteration.
				while (inptr < inend && bytes < 4) {
					rank = base64_rank[(c = *inptr++)];

					if (rank != 0xFF) {
						previous = (previous << 8) | c;
						saved = (saved << 6) | rank;
						bytes++;
					}
				}

				if (bytes < 4) {
					// Not enough bytes to decode anything...
					return 0;
				}

				if ((previous & 0xFF0000) != ((byte) '=') << 16) {
					*outptr++ = (byte) ((saved >> 16) & 0xFF);
					if ((previous & 0xFF00) != ((byte) '=') << 8) {
						*outptr++ = (byte) ((saved >> 8) & 0xFF);
						if ((previous & 0xFF) != (byte) '=')
							*outptr++ = (byte) (saved & 0xFF);
					}
				}

				saved = 0;
				bytes = 0;

				remainingInput = (int) (inend - inptr);
			} else {
				// No previous data, so we can just start decoding.
				remainingInput = length;
			}

			// decode every quartet into a triplet
			while (inptr < inend) {
				Vector128Decode
			}

			return (int) (outptr - output);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe void Avx512Decode (ref byte* input, ref byte* output, byte* maxOffset)
		{
			// Reference for VBMI implementation : https://github.com/WojciechMula/base64simd/tree/master/decode
			// If we have AVX512 support, pick off 64 bytes at a time for as long as we can,
			// but make sure that we quit before seeing any == markers at the end of the
			// string. Also, because we write 16 zeroes at the end of the output, ensure
			// that there are at least 22 valid bytes of input data remaining to close the
			// gap. 64 + 2 + 22 = 88 bytes.
			byte* outptr = output;
			byte* inptr = input;

			// The JIT won't hoist these "constants", so help it
			Vector512<sbyte> vbmiLookup0 = Vector512.Create (VbmiLookup0).AsSByte ();
			Vector512<sbyte> vbmiLookup1 = Vector512.Create (VbmiLookup1).AsSByte ();
			Vector512<byte> vbmiPackedLanesControl = Vector512.Create (
				0x06000102, 0x090a0405, 0x0c0d0e08, 0x16101112,
				0x191a1415, 0x1c1d1e18, 0x26202122, 0x292a2425,
				0x2c2d2e28, 0x36303132, 0x393a3435, 0x3c3d3e38,
				0x00000000, 0x00000000, 0x00000000, 0x00000000).AsByte ();

			Vector512<sbyte> mergeConstant0 = Vector512.Create (0x01400140).AsSByte ();
			Vector512<short> mergeConstant1 = Vector512.Create (0x00011000).AsInt16 ();

			// This algorithm requires AVX512VBMI support.
			// Vbmi was first introduced in CannonLake and is available from IceLake on.
			do {
				Vector512<sbyte> str = Vector512.Load (inptr).AsSByte ();

				// Step 1: Translate encoded Base64 input to their original indices
				// This step also checks for invalid inputs and exits.
				// After this, we have indices which are verified to have upper 2 bits set to 0 in each byte.
				// origIndex      = [...|00dddddd|00cccccc|00bbbbbb|00aaaaaa]
				Vector512<sbyte> origIndex = Avx512Vbmi.PermuteVar64x8x2 (vbmiLookup0, str, vbmiLookup1);
				Vector512<sbyte> errorVec = (origIndex.AsInt32 () | str.AsInt32 ()).AsSByte ();
				if (errorVec.ExtractMostSignificantBits () != 0)
					break;

				// Step 2: Now we need to reshuffle bits to remove the 0 bits.
				// multiAdd1: [...|0000cccc|ccdddddd|0000aaaa|aabbbbbb]
				Vector512<short> multiAdd1 = Avx512BW.MultiplyAddAdjacent (origIndex.AsByte (), mergeConstant0);
				// multiAdd1: [...|00000000|aaaaaabb|bbbbcccc|ccdddddd]
				Vector512<int> multiAdd2 = Avx512BW.MultiplyAddAdjacent (multiAdd1, mergeConstant1);

				// Step 3: Pack 48 bytes
				str = Avx512Vbmi.PermuteVar64x8 (multiAdd2.AsByte (), vbmiPackedLanesControl).AsSByte ();
				str.Store ((sbyte*) outptr);
				inptr += 64;
				outptr += 48;
			} while (inptr <= maxOffset);

			input = inptr;
			output = outptr;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe void Avx2Decode (ref byte* srcBytes, ref byte* destBytes, byte* srcEnd, int sourceLength, int destLength, byte* srcStart, byte* destStart)
		{
			// If we have AVX2 support, pick off 32 bytes at a time for as long as we can,
			// but make sure that we quit before seeing any == markers at the end of the
			// string. Also, because we write 8 zeroes at the end of the output, ensure
			// that there are at least 11 valid bytes of input data remaining to close the
			// gap. 32 + 2 + 11 = 45 bytes.

			// See SSSE3-version below for an explanation of how the code works.

			// The JIT won't hoist these "constants", so help it
			Vector256<sbyte> lutHi = Vector256.Create (Avx2LutHigh);
			Vector256<sbyte> lutLo = Vector256.Create (Avx2LutLow);
			Vector256<sbyte> lutShift = Vector256.Create (Avx2LutShift);
			Vector256<sbyte> packBytesInLaneMask = Vector256.Create (
				2, 1, 0, 6,
				5, 4, 10, 9,
				8, 14, 13, 12,
				-1, -1, -1, -1,
				2, 1, 0, 6,
				5, 4, 10, 9,
				8, 14, 13, 12,
				-1, -1, -1, -1);
			Vector256<int> packLanesControl = Vector256.Create (
				 0, 0, 0, 0,
				1, 0, 0, 0,
				2, 0, 0, 0,
				4, 0, 0, 0,
				5, 0, 0, 0,
				6, 0, 0, 0,
				-1, -1, -1, -1,
				-1, -1, -1, -1).AsInt32 ();
			Vector256<sbyte> maskSlashOrUnderscore = Vector256.Create ((sbyte) '/');
			Vector256<sbyte> mergeConstant0 = Vector256.Create (0x01400140).AsSByte ();
			Vector256<short> mergeConstant1 = Vector256.Create (0x00011000).AsInt16 ();
			byte* dest = destBytes;
			byte* src = srcBytes;

			//while (remaining >= 45)
			do {
				Vector256<sbyte> str = Avx.LoadVector256 (src).AsSByte ();
				Vector256<sbyte> hiNibbles = Avx2.And (Avx2.ShiftRightLogical (str.AsInt32 (), 4).AsSByte (), maskSlashOrUnderscore);

				if (!TryDecode256Core (str, hiNibbles, maskSlashOrUnderscore, lutLo, lutHi, lutShift, out str))
					break;

				// in, lower lane, bits, upper case are most significant bits, lower case are least significant bits:
				// 00llllll 00kkkkLL 00jjKKKK 00JJJJJJ
				// 00iiiiii 00hhhhII 00ggHHHH 00GGGGGG
				// 00ffffff 00eeeeFF 00ddEEEE 00DDDDDD
				// 00cccccc 00bbbbCC 00aaBBBB 00AAAAAA

				Vector256<short> merge_ab_and_bc = Avx2.MultiplyAddAdjacent (str.AsByte (), mergeConstant0);
				// 0000kkkk LLllllll 0000JJJJ JJjjKKKK
				// 0000hhhh IIiiiiii 0000GGGG GGggHHHH
				// 0000eeee FFffffff 0000DDDD DDddEEEE
				// 0000bbbb CCcccccc 0000AAAA AAaaBBBB

				Vector256<int> output = Avx2.MultiplyAddAdjacent (merge_ab_and_bc, mergeConstant1);
				// 00000000 JJJJJJjj KKKKkkkk LLllllll
				// 00000000 GGGGGGgg HHHHhhhh IIiiiiii
				// 00000000 DDDDDDdd EEEEeeee FFffffff
				// 00000000 AAAAAAaa BBBBbbbb CCcccccc

				// Pack bytes together in each lane:
				output = Avx2.Shuffle (output.AsSByte (), packBytesInLaneMask).AsInt32 ();
				// 00000000 00000000 00000000 00000000
				// LLllllll KKKKkkkk JJJJJJjj IIiiiiii
				// HHHHhhhh GGGGGGgg FFffffff EEEEeeee
				// DDDDDDdd CCcccccc BBBBbbbb AAAAAAaa

				// Pack lanes
				str = Avx2.PermuteVar8x32 (output, packLanesControl).AsSByte ();
				Avx.Store (dest, str.AsByte ());
				src += 32;
				dest += 24;
			} while (src <= srcEnd);

			srcBytes = src;
			destBytes = dest;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> SimdShuffle (Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask8F)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			if (Ssse3.IsSupported) {
				return Ssse3.Shuffle (left, right);
			} else {
				return AdvSimd.Arm64.VectorTableLookup (left, right & mask8F);
			}
		}

#if NET9_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void AdvSimdDecode (ref byte* srcBytes, ref byte* destBytes, byte* srcEnd, int sourceLength, int destLength, byte* srcStart, byte* destStart)
        {
            // C# implementation of https://github.com/aklomp/base64/blob/3a5add8652076612a8407627a42c768736a4263f/lib/arch/neon64/dec_loop.c
            // If we have AdvSimd support, pick off 64 bytes at a time for as long as we can,
            // but make sure that we quit before seeing any == markers at the end of the
            // string. 64 + 2 = 66 bytes.

            // In the decoding process, we want to map each byte, representing a Base64 value, to its 6-bit (0-63) representation.
            // It uses the following mapping. Values outside the following groups are invalid and, we abort decoding when encounter one.
            //
            // #    From       To         Char
            // 1    [43]       [62]       +
            // 2    [47]       [63]       /
            // 3    [48..57]   [52..61]   0..9
            // 4    [65..90]   [0..25]    A..Z
            // 5    [97..122]  [26..51]   a..z
            //
            // To map an input value to its Base64 representation, we use look-up tables 'decLutOne' and 'decLutTwo'.
            // 'decLutOne' helps to map groups 1, 2 and 3 while 'decLutTwo' maps groups 4 and 5 in the above list.
            // After mapping, each value falls between 0-63. Consequently, the last six bits of each byte now hold a valid value.
            // We then compress four such bytes (with valid 4 * 6 = 24 bits) to three UTF8 bytes (3 * 8 = 24 bits).
            // For faster decoding, we use SIMD operations that allow the processing of multiple bytes together.
            // However, the compress operation on adjacent values of a vector could be slower. Thus, we de-interleave while reading
            // the input bytes that store adjacent bytes in separate vectors. This later simplifies the compress step with the help
            // of logical operations. This requires interleaving while storing the decoded result.

            // Values in 'decLutOne' maps input values from 0 to 63.
            //   255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
            //   255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
            //   255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,  62, 255, 255, 255,  63
            //    52,  53,  54,  55,  56,  57,  58,  59,  60,  61, 255, 255, 255, 255, 255, 255
            var decLutOne = (Vector128<byte>.AllBitsSet,
                             Vector128<byte>.AllBitsSet,
                             Vector128.Create (AdvSimdLutOne3).AsByte(),
                             Vector128.Create (0x37363534, 0x3B3A3938, 0xFFFF3D3C, 0xFFFFFFFF).AsByte ());

            // Values in 'decLutTwo' maps input values from 63 to 127.
            //    0, 255,   0,   1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13
            //   14,  15,  16,  17,  18,  19,  20,  21,  22,  23,  24,  25, 255, 255, 255, 255
            //  255, 255,  26,  27,  28,  29,  30,  31,  32,  33,  34,  35,  36,  37,  38,  39
            //   40,  41,  42,  43,  44,  45,  46,  47,  48,  49,  50,  51, 255, 255, 255, 255
            var decLutTwo = (Vector128.Create (0x0100FF00, 0x05040302, 0x09080706, 0x0D0C0B0A).AsByte (),
                             Vector128.Create (0x11100F0E, 0x15141312, 0x19181716, 0xFFFFFFFF).AsByte (),
                             Vector128.Create (AdvSimdLutTwo3Uint1, 0x1F1E1D1C, 0x23222120, 0x27262524).AsByte (),
                             Vector128.Create (0x2B2A2928, 0x2F2E2D2C, 0x33323130, 0xFFFFFFFF).AsByte ());

            byte* src = srcBytes;
            byte* dest = destBytes;
            Vector128<byte> offset = Vector128.Create<byte> (63);

            do {
                // Step 1: Load 64 bytes and de-interleave.
                if (!this.TryLoadArmVector128x4(src, srcStart, sourceLength,
                    out Vector128<byte> str1, out Vector128<byte> str2, out Vector128<byte> str3, out Vector128<byte> str4))
                {
                    break;
                }

                // Step 2: Map each valid input to its Base64 value.
                // We use two look-ups to compute partial results and combine them later.

                // Step 2.1: Detect valid Base64 values from the first three groups. Maps input as,
                //  0 to  63 (Invalid) => 255
                //  0 to  63 (Valid)   => Their Base64 equivalent
                // 64 to 255           => 0

                // Each input value acts as an index in the look-up table 'decLutOne'.
                // e.g., for group 1: index 43 maps to 62 (Base64 '+').
                // Group 4 and 5 values are out-of-range (>64), so they are mapped to zero.
                // Other valid indices but invalid values are mapped to 255.
                Vector128<byte> decOne1 = AdvSimd.Arm64.VectorTableLookup (decLutOne, str1);
                Vector128<byte> decOne2 = AdvSimd.Arm64.VectorTableLookup (decLutOne, str2);
                Vector128<byte> decOne3 = AdvSimd.Arm64.VectorTableLookup (decLutOne, str3);
                Vector128<byte> decOne4 = AdvSimd.Arm64.VectorTableLookup (decLutOne, str4);

                // Step 2.2: Detect valid Base64 values from groups 4 and 5. Maps input as,
                //   0 to  63           => 0
                //  64 to 122 (Valid)   => Their Base64 equivalent
                //  64 to 122 (Invalid) => 255
                // 123 to 255           => Remains unchanged

                // Subtract/offset each input value by 63 so that it can be used as a valid offset.
                // Subtract saturate makes values from the first three groups set to zero that are
                // then mapped to zero in the subsequent look-up.
                Vector128<byte> decTwo1 = AdvSimd.SubtractSaturate (str1, offset);
                Vector128<byte> decTwo2 = AdvSimd.SubtractSaturate (str2, offset);
                Vector128<byte> decTwo3 = AdvSimd.SubtractSaturate (str3, offset);
                Vector128<byte> decTwo4 = AdvSimd.SubtractSaturate (str4, offset);

                // We use VTBX to map values where out-of-range indices are unchanged.
                decTwo1 = AdvSimd.Arm64.VectorTableLookupExtension(decTwo1, decLutTwo, decTwo1);
                decTwo2 = AdvSimd.Arm64.VectorTableLookupExtension(decTwo2, decLutTwo, decTwo2);
                decTwo3 = AdvSimd.Arm64.VectorTableLookupExtension(decTwo3, decLutTwo, decTwo3);
                decTwo4 = AdvSimd.Arm64.VectorTableLookupExtension(decTwo4, decLutTwo, decTwo4);

                // Step 3: Combine the partial result.
                // Each look-up above maps valid values to their Base64 equivalent or zero.
                // Thus the intermediate results 'decOne' and 'decTwo' could be OR-ed to get final values.
                str1 = (decOne1 | decTwo1);
                str2 = (decOne2 | decTwo2);
                str3 = (decOne3 | decTwo3);
                str4 = (decOne4 | decTwo4);

                // Step 4: Detect an invalid input value.
                // Invalid values < 122 are set to 255 while the ones above 122 are unchanged.
                // Check for invalid input, any value larger than 63.
                Vector128<byte> classified = (Vector128.GreaterThan (str1, offset)
                                            | Vector128.GreaterThan (str2, offset)
                                            | Vector128.GreaterThan (str3, offset)
                                            | Vector128.GreaterThan (str4, offset));

                // Check that all bits are zero.
                if (classified != Vector128<byte>.Zero)
                    break;

                // Step 5: Compress four bytes into three.
                Vector128<byte> res1 = ((str1 << 2) | (str2 >> 4));
                Vector128<byte> res2 = ((str2 << 4) | (str3 >> 2));
                Vector128<byte> res3 = ((str3 << 6) | str4);

                // Step 6: Interleave and store decoded results.
                AdvSimd.Arm64.StoreVectorAndZip (dest, (res1, res2, res3));

                src += 64;
                dest += 48;
            } while (src <= srcEnd);

            srcBytes = src;
            destBytes = dest;
        }

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe bool TryLoadArmVector128x4 (byte* src, byte* srcStart, int sourceLength,
			out Vector128<byte> str1, out Vector128<byte> str2, out Vector128<byte> str3, out Vector128<byte> str4)
		{
			(str1, str2, str3, str4) = AdvSimd.Arm64.Load4xVector128AndUnzip (src);

			return true;
		}
#endif

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe void Vector128Decode (ref byte* srcBytes, ref byte* destBytes, byte* srcEnd)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			// If we have Vector128 support, pick off 16 bytes at a time for as long as we can,
			// but make sure that we quit before seeing any == markers at the end of the
			// string. Also, because we write four zeroes at the end of the output, ensure
			// that there are at least 6 valid bytes of input data remaining to close the
			// gap. 16 + 2 + 6 = 24 bytes.

			// The input consists of six character sets in the Base64 alphabet,
			// which we need to map back to the 6-bit values they represent.
			// There are three ranges, two singles, and then there's the rest.
			//
			//  #  From       To        Add  Characters
			//  1  [43]       [62]      +19  +
			//  2  [47]       [63]      +16  /
			//  3  [48..57]   [52..61]   +4  0..9
			//  4  [65..90]   [0..25]   -65  A..Z
			//  5  [97..122]  [26..51]  -71  a..z
			// (6) Everything else => invalid input

			// We will use LUTS for character validation & offset computation
			// Remember that 0x2X and 0x0X are the same index for _mm_shuffle_epi8,
			// this allows to mask with 0x2F instead of 0x0F and thus save one constant declaration (register and/or memory access)

			// For offsets:
			// Perfect hash for lut = ((inptr>>4)&0x2F)+((inptr==0x2F)?0xFF:0x00)
			// 0000 = garbage
			// 0001 = /
			// 0010 = +
			// 0011 = 0-9
			// 0100 = A-Z
			// 0101 = A-Z
			// 0110 = a-z
			// 0111 = a-z
			// 1000 >= garbage

			// For validation, here's the table.
			// A character is valid if and only if the AND of the 2 lookups equals 0:

			// hi \ lo              0000 0001 0010 0011 0100 0101 0110 0111 1000 1001 1010 1011 1100 1101 1110 1111
			//      LUT             0x15 0x11 0x11 0x11 0x11 0x11 0x11 0x11 0x11 0x11 0x13 0x1A 0x1B 0x1B 0x1B 0x1A

			// 0000 0X10 char        NUL  SOH  STX  ETX  EOT  ENQ  ACK  BEL   BS   HT   LF   VT   FF   CR   SO   SI
			//           andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10

			// 0001 0x10 char        DLE  DC1  DC2  DC3  DC4  NAK  SYN  ETB  CAN   EM  SUB  ESC   FS   GS   RS   US
			//           andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10

			// 0010 0x01 char               !    "    #    $    %    &    '    (    )    *    +    ,    -    .    /
			//           andlut     0x01 0x01 0x01 0x01 0x01 0x01 0x01 0x01 0x01 0x01 0x01 0x00 0x01 0x01 0x01 0x00

			// 0011 0x02 char          0    1    2    3    4    5    6    7    8    9    :    ;    <    =    >    ?
			//           andlut     0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x02 0x02 0x02 0x02 0x02 0x02

			// 0100 0x04 char          @    A    B    C    D    E    F    G    H    I    J    K    L    M    N    0
			//           andlut     0x04 0x00 0x00 0x00 0X00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00

			// 0101 0x08 char          P    Q    R    S    T    U    V    W    X    Y    Z    [    \    ]    ^    _
			//           andlut     0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x08 0x08 0x08 0x08 0x08

			// 0110 0x04 char          `    a    b    c    d    e    f    g    h    i    j    k    l    m    n    o
			//           andlut     0x04 0x00 0x00 0x00 0X00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00
			// 0111 0X08 char          p    q    r    s    t    u    v    w    x    y    z    {    |    }    ~
			//           andlut     0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x08 0x08 0x08 0x08 0x08

			// 1000 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1001 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1010 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1011 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1100 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1101 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1110 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10
			// 1111 0x10 andlut     0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10 0x10

			// The JIT won't hoist these "constants", so help it
			Vector128<byte> lutHi = Vector128.Create (Vector128LutHigh).AsByte ();
			Vector128<byte> lutLo = Vector128.Create (Vector128LutLow).AsByte ();
			Vector128<sbyte> lutShift = Vector128.Create (Vector128LutShift).AsSByte ();
			Vector128<sbyte> packBytesMask = Vector128.Create (0x06000102, 0x090A0405, 0x0C0D0E08, 0xffffffff).AsSByte ();
			Vector128<byte> mergeConstant0 = Vector128.Create (0x01400140).AsByte ();
			Vector128<short> mergeConstant1 = Vector128.Create (0x00011000).AsInt16 ();
			Vector128<byte> one = Vector128.Create ((byte) 1);
			Vector128<byte> mask2F = Vector128.Create ((byte) '/');
			Vector128<byte> mask8F = Vector128.Create ((byte) 0x8F);
			byte* dest = destBytes;
			byte* src = srcBytes;

			//while (remaining >= 24)
			do {
				Vector128<byte> str = Vector128.LoadUnsafe (ref *src);

				// lookup
				Vector128<byte> hiNibbles = Vector128.ShiftRightLogical (str.AsInt32 (), 4).AsByte () & mask2F;

				if (!TryDecode128Core (str, hiNibbles, mask2F, mask8F, lutLo, lutHi, lutShift, out str))
					break;

				// in, bits, upper case are most significant bits, lower case are least significant bits
				// 00llllll 00kkkkLL 00jjKKKK 00JJJJJJ
				// 00iiiiii 00hhhhII 00ggHHHH 00GGGGGG
				// 00ffffff 00eeeeFF 00ddEEEE 00DDDDDD
				// 00cccccc 00bbbbCC 00aaBBBB 00AAAAAA

				Vector128<short> merge_ab_and_bc;
				if (Ssse3.IsSupported) {
					merge_ab_and_bc = Ssse3.MultiplyAddAdjacent (str.AsByte (), mergeConstant0.AsSByte ());
				} else if (AdvSimd.Arm64.IsSupported) {
					Vector128<ushort> evens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (str, one).GetLower (), 6);
					Vector128<ushort> odds = AdvSimd.Arm64.TransposeOdd (str, Vector128<byte>.Zero).AsUInt16 ();
					merge_ab_and_bc = Vector128.Add (evens, odds).AsInt16 ();
				} else {
					// We explicitly recheck each IsSupported query to ensure that the trimmer can see which paths are live/dead
					ThrowUnreachableException ();
					merge_ab_and_bc = default;
				}
				// 0000kkkk LLllllll 0000JJJJ JJjjKKKK
				// 0000hhhh IIiiiiii 0000GGGG GGggHHHH
				// 0000eeee FFffffff 0000DDDD DDddEEEE
				// 0000bbbb CCcccccc 0000AAAA AAaaBBBB

				Vector128<int> output;
				if (Ssse3.IsSupported) {
					output = Sse2.MultiplyAddAdjacent (merge_ab_and_bc, mergeConstant1);
				} else if (AdvSimd.Arm64.IsSupported) {
					Vector128<int> ievens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (merge_ab_and_bc, one.AsInt16 ()).GetLower (), 12);
					Vector128<int> iodds = AdvSimd.Arm64.TransposeOdd (merge_ab_and_bc, Vector128<short>.Zero).AsInt32 ();
					output = Vector128.Add (ievens, iodds).AsInt32 ();
				} else {
					// We explicitly recheck each IsSupported query to ensure that the trimmer can see which paths are live/dead
					ThrowUnreachableException ();
					output = default;
				}
				// 00000000 JJJJJJjj KKKKkkkk LLllllll
				// 00000000 GGGGGGgg HHHHhhhh IIiiiiii
				// 00000000 DDDDDDdd EEEEeeee FFffffff
				// 00000000 AAAAAAaa BBBBbbbb CCcccccc

				// Pack bytes together:
				str = SimdShuffle (output.AsByte (), packBytesMask.AsByte (), mask8F);
				// 00000000 00000000 00000000 00000000
				// LLllllll KKKKkkkk JJJJJJjj IIiiiiii
				// HHHHhhhh GGGGGGgg FFffffff EEEEeeee
				// DDDDDDdd CCcccccc BBBBbbbb AAAAAAaa

				str.Store (dest);

				src += 16;
				dest += 12;
			} while (src <= srcEnd);

			srcBytes = src;
			destBytes = dest;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static unsafe void WriteThreeLowOrderBytes (byte* destination, int value)
		{
			destination[0] = (byte) (value >> 16);
			destination[1] = (byte) (value >> 8);
			destination[2] = (byte) value;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		internal static bool IsWhiteSpace (int value)
		{
			Debug.Assert (value >= 0 && value <= ushort.MaxValue);
			uint charMinusLowUInt32;
			return (int) ((0xC8000100U << (short) (charMinusLowUInt32 = (ushort) (value - '\t'))) & (charMinusLowUInt32 - 32)) < 0;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool TryDecode128Core (
				Vector128<byte> str,
				Vector128<byte> hiNibbles,
				Vector128<byte> maskSlashOrUnderscore,
				Vector128<byte> mask8F,
				Vector128<byte> lutLow,
				Vector128<byte> lutHigh,
				Vector128<sbyte> lutShift,
				out Vector128<byte> result)
		{
			Vector128<byte> loNibbles = str & maskSlashOrUnderscore;
			Vector128<byte> hi = SimdShuffle (lutHigh, hiNibbles, mask8F);
			Vector128<byte> lo = SimdShuffle (lutLow, loNibbles, mask8F);

			// Check for invalid input: if any "and" values from lo and hi are not zero,
			// fall back on bytewise code to do error checking and reporting:
			if ((lo & hi) != Vector128<byte>.Zero) {
				result = default;
				return false;
			}

			Vector128<byte> eq2F = Vector128.Equals (str, maskSlashOrUnderscore);
			Vector128<byte> shift = SimdShuffle (lutShift.AsByte (), (eq2F + hiNibbles), mask8F);

			// Now simply add the delta values to the input:
			result = str + shift;

			return true;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool TryDecode256Core (
			Vector256<sbyte> str,
			Vector256<sbyte> hiNibbles,
			Vector256<sbyte> maskSlashOrUnderscore,
			Vector256<sbyte> lutLow,
			Vector256<sbyte> lutHigh,
			Vector256<sbyte> lutShift,
			out Vector256<sbyte> result)
		{
			Vector256<sbyte> loNibbles = Avx2.And (str, maskSlashOrUnderscore);
			Vector256<sbyte> hi = Avx2.Shuffle (lutHigh, hiNibbles);
			Vector256<sbyte> lo = Avx2.Shuffle (lutLow, loNibbles);

			if ((lo & hi) != Vector256<sbyte>.Zero) {
				result = default;
				return false;
			}

			Vector256<sbyte> eq2F = Avx2.CompareEqual (str, maskSlashOrUnderscore);
			Vector256<sbyte> shift = Avx2.Shuffle (lutShift, Avx2.Add (eq2F, hiNibbles));

			result = Avx2.Add (str, shift);

			return true;
		}
#endif

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int SlowDecode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			// decode every quartet into a triplet
			while (inptr < inend) {
				byte rank = base64_rank[(c = *inptr++)];

				if (rank != 0xFF) {
					previous = (previous << 8) | c;
					saved = (saved << 6) | rank;
					bytes++;

					if (bytes == 4) {
						if ((previous & 0xFF0000) != ((byte) '=') << 16) {
							*outptr++ = (byte) ((saved >> 16) & 0xFF);
							if ((previous & 0xFF00) != ((byte) '=') << 8) {
								*outptr++ = (byte) ((saved >> 8) & 0xFF);
								if ((previous & 0xFF) != (byte) '=')
									*outptr++ = (byte) (saved & 0xFF);
							}
						}
						saved = 0;
						bytes = 0;
					}
				}
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// decoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">A pointer to the beginning of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">A pointer to the beginning of the output buffer.</param>
		public unsafe int Decode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			// decode every quartet into a triplet
			while (inptr < inend) {
				byte rank = base64_rank[(c = *inptr++)];

				if (rank != 0xFF) {
					previous = (previous << 8) | c;
					saved = (saved << 6) | rank;
					bytes++;

					if (bytes == 4) {
						if ((previous & 0xFF0000) != ((byte) '=') << 16) {
							*outptr++ = (byte) ((saved >> 16) & 0xFF);
							if ((previous & 0xFF00) != ((byte) '=') << 8) {
								*outptr++ = (byte) ((saved >> 8) & 0xFF);
								if ((previous & 0xFF) != (byte) '=')
									*outptr++ = (byte) (saved & 0xFF);
							}
						}
						saved = 0;
						bytes = 0;
					}
				}
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Decode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// decoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">The output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="input"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <see langword="null"/>.</para>
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
		public int Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Decode (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Reset the decoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the decoder.
		/// </remarks>
		public void Reset ()
		{
			previous = 0;
			saved = 0;
			bytes = 0;
		}
	}
}
