//
// Base64Encoder.cs
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

// Note: The SIMD (Avx512, Avx2, AdvSimd, and Ssse3) implementations of the base64 encoder
// were borrowed (and modified) from the .NET Core implementation located at:
// https://github.com/dotnet/runtime/blob/release/9.0/src/libraries/System.Private.CoreLib/src/System/Buffers/Text/Base64Helper/Base64EncoderHelper.cs
//
// The .NET Core implementation was, in turn, inspired by the work done by Wojciech Mula
// in his base64simd project: https://github.com/WojciechMula/base64simd

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NET6_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally encodes content using the base64 encoding.
	/// </summary>
	/// <remarks>
	/// Base64 is an encoding often used in MIME to encode binary content such
	/// as images and other types of multimedia to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	public class Base64Encoder : IMimeEncoder
	{
		internal static ReadOnlySpan<byte> base64_alphabet => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8;

		readonly int quartetsPerLine;
		int quartets;
		byte saved1;
		byte saved2;
		byte saved;

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Encoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 encoder.
		/// </remarks>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		public Base64Encoder (int maxLineLength = 76)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			// The base64 specification in rfc2045 require a maximum line length of 76.
			maxLineLength = Math.Min (maxLineLength, 76);

			quartetsPerLine = maxLineLength / 4;

#if NET6_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || AdvSimd.Arm64.IsSupported;
#endif
		}

		/// <summary>
		/// Clone the <see cref="Base64Encoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Base64Encoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="Base64Encoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			return new Base64Encoder (quartetsPerLine * 4) {
#if NET6_0_OR_GREATER
				EnableHardwareAcceleration = EnableHardwareAcceleration,
#endif
				quartets = quartets,
				saved1 = saved1,
				saved2 = saved2,
				saved = saved
			};
		}

#if NET6_0_OR_GREATER
		/// <summary>
		/// Get or set whether the <see cref="Base64Encoder"/> should use hardware acceleration when available.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the <see cref="Base64Encoder"/> should use hardware acceleration when available.
		/// </remarks>
		/// <value><see langword="true"/> if hardware acceleration should be enabled; otherwise, <see langword="false"/>.</value>
		public bool EnableHardwareAcceleration {
			get; set;
		}
#endif

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Base64; }
		}

		/// <summary>
		/// Estimate the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to encode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			int maxLineLength = (quartetsPerLine * 4) + 1;
			int maxInputPerLine = quartetsPerLine * 3;

			return (((inputLength + 2) / maxInputPerLine) * maxLineLength) + maxLineLength;
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
				throw new ArgumentException ("The output buffer is not large enough to contain the encoded input.", nameof (output));
		}

#if NET6_0_OR_GREATER
		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int HwAccelEncode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			int c1, c2, c3;

			// Flush out any partially read data from a previous iteration.
			if (saved > 0 && length + saved > 2) {
				c1 = saved1;
				c2 = saved < 2 ? *inptr++ : saved2;
				c3 = *inptr++;

				// encode our triplet into a quartet
				*outptr++ = base64_alphabet[c1 >> 2];
				*outptr++ = base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
				*outptr++ = base64_alphabet[((c2 & 0x0f) << 2) | (c3 >> 6)];
				*outptr++ = base64_alphabet[c3 & 0x3f];

				// append a newline if we've reached our target quartet count
				if (++quartets >= quartetsPerLine) {
					*outptr++ = (byte) '\n';
					quartets = 0;
				}

				saved = 0;
			}

			int remainingInput = (int) (inend - inptr);

			// prevent the while-loop from processing incomplete triplets
			inend -= 2;

			while (inptr < inend) {
				int remainingLineInput = Math.Min (remainingInput, (quartetsPerLine - quartets) * 3);
				byte* lineEnd = inptr + remainingLineInput;
				int nread;

				// Note: The standard MIME base64-encoded line length is 76 characters which means that the maximum number
				// of input characters that can be processed per line is 57 characters (3 bytes per quartet).
				//
				// Given that, here's what we would expect for various hardware configurations:
				//
				// Intel/AMD x86/64 CPUs:
				// - AVX512 supported:
				//   - Encodes the first 48 bytes using Avx512Encode() which performs a single loop.
				//   - Encodes the remaining 9 bytes using the "slow" loop that processes 3 input bytes per loop for 3 loops.
				// - AVX2 supported:
				//   - Encodes the first 48 bytes using Avx2Encode() which performs 2 loops.
				//   - Encodes the remaining 9 bytes using the "slow" loop that processes 3 input bytes per loop for 3 loops.
				// - SSSE3 supported:
				//   - Encodes the first 48 bytes using Vector128Encode() which performs 4 loops.
				//   - Encodes the remaining 9 bytes using the "slow" loop that processes 3 input bytes per loop for 3 loops.
				// - No SIMD support:
				//   - Encodes all 57 bytes using the "slow" loop that processes 3 input bytes per loop for 19 loops.
				//
				// ARM64 CPUs:
				// - AdvSIMD supported (>= 48 remaining input bytes):
				//   - Encodes the first 48 bytes using AdvSimdEncode() which performs a single loop.
				//   - Encodes the remaining 9 bytes using the "slow" loop that processes 3 input bytes per loop for 3 loops.
				// - AdvSIMD supported (>= 12 remaining input bytes):
				//   - Encodes the first 12, 24, or 36 bytes using Vector128Encode() which performs 1, 2, or 3 loops.
				//   - Encodes the remaining 9 bytes using the "slow" loop that processes 3 input bytes per loop for 3 loops.

				if (Ssse3.IsSupported) {
					// Hardware accelerated Intel/AMD code-path...
					if (Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported && remainingLineInput >= 48 && remainingInput >= 64) {
						// Avx512Encode processes 48 bytes at a time, but requires at least 64 bytes of input to avoid segfaulting.
						nread = Avx512Encode (ref inptr, ref outptr, lineEnd - 48, ref quartets);
						remainingLineInput -= nread;
						remainingInput -= nread;
					}

					if (Avx2.IsSupported && remainingLineInput >= 48 && remainingInput >= 56) {
						// Avx2Encode processes 24 bytes at a time, but requires at least 32 bytes of input to avoid segfaulting.
						// Note: This is not worth doing for anything less than at least 2 passes.
						nread = Avx2Encode (ref inptr, ref outptr, lineEnd - 24, ref quartets);
						remainingLineInput -= nread;
						remainingInput -= nread;
					}

					if (remainingLineInput >= 12 && remainingInput >= 16) {
						// Vector128Encode (requires SSSE3) processes 12 bytes at a time, but requires at least 16 bytes of input to avoid segfaulting.
						nread = Vector128Encode (ref inptr, ref outptr, lineEnd - 12, ref quartets);
						remainingLineInput -= nread;
						remainingInput -= nread;
					}
				} else if (AdvSimd.Arm64.IsSupported) {
					// Hardware accelerated ARM64 code-path...
#if NET9_0_OR_GREATER
					if (remainingLineInput >= 48) {
						// AdvSimdEncode processes exactly 48 bytes at a time (3x 16-byte chunks).
						nread = AdvSimdEncode (ref inptr, ref outptr, lineEnd - 48, ref quartets);
						remainingLineInput -= nread;
						remainingInput -= nread;
					}
#endif

					if (BitConverter.IsLittleEndian && remainingLineInput >= 12 && remainingInput >= 16) {
						// Vector128Encode (requires AdvSIMD) processes 12 bytes at a time, but requires at least 16 bytes of input to avoid segfaulting.
						nread = Vector128Encode (ref inptr, ref outptr, lineEnd - 12, ref quartets);
						remainingLineInput -= nread;
						remainingInput -= nread;
					}
				}

				// prevent the while-loop from reading beyond the end of the line
				lineEnd -= 2;

				while (inptr < lineEnd) {
					c1 = *inptr++;
					c2 = *inptr++;
					c3 = *inptr++;

					// encode our triplet into a quartet
					*outptr++ = base64_alphabet[c1 >> 2];
					*outptr++ = base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
					*outptr++ = base64_alphabet[((c2 & 0x0f) << 2) | (c3 >> 6)];
					*outptr++ = base64_alphabet[c3 & 0x3f];
					remainingInput -= 3;
					quartets++;
				}

				// append a newline if we've reached our target quartet count
				if (quartets >= quartetsPerLine) {
					*outptr++ = (byte) '\n';
					quartets = 0;
				}
			}

			if (remainingInput > 0) {
				// At this point, saved can only be 0 or 1.
				if (saved == 0) {
					// We can have up to 2 remaining input bytes.
					saved = (byte) remainingInput;
					saved1 = *inptr++;
					if (remainingInput == 2)
						saved2 = *inptr;
					else
						saved2 = 0;
				} else {
					// We have 1 remaining input byte.
					saved2 = *inptr++;
					saved = 2;
				}
			}

			return (int) (outptr - output);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe int Avx512Encode (ref byte* input, ref byte* output, byte* maxOffset, ref int quartets)
		{
			// Reference for VBMI implementation: https://github.com/WojciechMula/base64simd/tree/master/encode
			// If we have AVX512 support, pick off 48 bytes at a time for as long as we can.
			// But because we read 64 bytes at a time, ensure we have enough room to do a
			// full 64-byte read without segfaulting.
			byte* outptr = output;
			byte* inptr = input;

			// The JIT won't hoist these "constants", so help it
			Vector512<sbyte> shuffleVecVbmi = Vector512.Create (
				0x01020001, 0x04050304, 0x07080607, 0x0a0b090a,
				0x0d0e0c0d, 0x10110f10, 0x13141213, 0x16171516,
				0x191a1819, 0x1c1d1b1c, 0x1f201e1f, 0x22232122,
				0x25262425, 0x28292728, 0x2b2c2a2b, 0x2e2f2d2e).AsSByte ();
			Vector512<sbyte> vbmiLookup = Vector512.Create (base64_alphabet).AsSByte ();
			Vector512<ushort> maskAC = Vector512.Create ((uint) 0x0fc0fc00).AsUInt16 ();
			Vector512<uint> maskBB = Vector512.Create ((uint) 0x3f003f00);
			Vector512<ushort> shiftAC = Vector512.Create ((uint) 0x0006000a).AsUInt16 ();
			Vector512<ushort> shiftBB = Vector512.Create ((uint) 0x00080004).AsUInt16 ();

			// This algorithm requires AVX512VBMI support.
			// Vbmi was first introduced in CannonLake and is available from IceLake on.

			do {
				// str = [...|PONM|LKJI|HGFE|DCBA]
				Vector512<sbyte> str = Vector512.Load (inptr).AsSByte ();

				// Step 1 : Split 48 bytes into 64 bytes with each byte using 6-bits from input
				// str = [...|KLJK|HIGH|EFDE|BCAB]
				str = Avx512Vbmi.PermuteVar64x8 (str, shuffleVecVbmi);

				// TODO: This can be achieved faster with multishift
				// Consider the first 4 bytes - BCAB
				// temp1	= [...|0000cccc|cc000000|aaaaaa00|00000000]
				Vector512<ushort> temp1 = (str.AsUInt16 () & maskAC);

				// temp2	= [...|00000000|00cccccc|00000000|00aaaaaa]
				Vector512<ushort> temp2 = Avx512BW.ShiftRightLogicalVariable (temp1, shiftAC).AsUInt16 ();

				// temp3	= [...|ccdddddd|00000000|aabbbbbb|cccc0000]
				Vector512<ushort> temp3 = Avx512BW.ShiftLeftLogicalVariable (str.AsUInt16 (), shiftBB).AsUInt16 ();

				// str	  = [...|00dddddd|00cccccc|00bbbbbb|00aaaaaa]
				str = Vector512.ConditionalSelect (maskBB, temp3.AsUInt32 (), temp2.AsUInt32 ()).AsSByte ();

				// Step 2: Now we have the indices calculated. Next step is to use these indices to translate.
				str = Avx512Vbmi.PermuteVar64x8 (vbmiLookup, str);

				str.Store ((sbyte*) outptr);

				inptr += 48;
				outptr += 64;
				quartets += 16;
			} while (inptr <= maxOffset);

			int nread = (int) (inptr - input);

			output = outptr;
			input = inptr;

			return nread;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe int Avx2Encode (ref byte* input, ref byte* output, byte* maxOffset, ref int quartets)
		{
			// If we have AVX2 support, pick off 24 bytes at a time for as long as we can.
			// But because we read 32 bytes at a time, ensure we have enough room to do a
			// full 32-byte read without segfaulting.

			// translation from SSSE3 into AVX2 of procedure
			// This one works with shifted (4 bytes) input in order to
			// be able to work efficiently in the 2 128-bit lanes

			// input, bytes MSB to LSB:
			// 0 0 0 0 x w v u t s r q p o n m
			// l k j i h g f e d c b a 0 0 0 0

			// The JIT won't hoist these "constants", so help it
			Vector256<sbyte> shuffleVec = Vector256.Create (
				5, 4, 6, 5,
				8, 7, 9, 8,
				11, 10, 12, 11,
				14, 13, 15, 14,
				1, 0, 2, 1,
				4, 3, 5, 4,
				7, 6, 8, 7,
				10, 9, 11, 10);

			Vector256<sbyte> lut = Vector256.Create (
				65, 71, -4, -4,
				-4, -4, -4, -4,
				-4, -4, -4, -4,
				-19, -16, 0, 0,
				65, 71, -4, -4,
				-4, -4, -4, -4,
				-4, -4, -4, -4,
				-19, -16, 0, 0);

			Vector256<sbyte> maskAC = Vector256.Create (0x0fc0fc00).AsSByte ();
			Vector256<sbyte> maskBB = Vector256.Create (0x003f03f0).AsSByte ();
			Vector256<ushort> shiftAC = Vector256.Create (0x04000040).AsUInt16 ();
			Vector256<short> shiftBB = Vector256.Create (0x01000010).AsInt16 ();
			Vector256<byte> const51 = Vector256.Create ((byte) 51);
			Vector256<sbyte> const25 = Vector256.Create ((sbyte) 25);

			byte* outptr = output;
			byte* inptr = input;

			// first load is done at c-0 not to get a segfault
			Vector256<sbyte> str = Avx.LoadVector256 (inptr).AsSByte ();

			// shift by 4 bytes, as required by Reshuffle
			str = Avx2.PermuteVar8x32 (str.AsInt32 (), Vector256.Create (
				0, 0, 0, 0,
				0, 0, 0, 0,
				1, 0, 0, 0,
				2, 0, 0, 0,
				3, 0, 0, 0,
				4, 0, 0, 0,
				5, 0, 0, 0,
				6, 0, 0, 0).AsInt32 ()).AsSByte ();

			// Next loads are done at inptr-4, as required by Reshuffle, so shift it once
			inptr -= 4;

			do {
				// Reshuffle
				str = Avx2.Shuffle (str, shuffleVec);
				// str, bytes MSB to LSB:
				// w x v w
				// t u s t
				// q r p q
				// n o m n
				// k l j k
				// h i g h
				// e f d e
				// b c a b

				Vector256<sbyte> t0 = Avx2.And (str, maskAC);
				// bits, upper case are most significant bits, lower case are least significant bits.
				// 0000wwww XX000000 VVVVVV00 00000000
				// 0000tttt UU000000 SSSSSS00 00000000
				// 0000qqqq RR000000 PPPPPP00 00000000
				// 0000nnnn OO000000 MMMMMM00 00000000
				// 0000kkkk LL000000 JJJJJJ00 00000000
				// 0000hhhh II000000 GGGGGG00 00000000
				// 0000eeee FF000000 DDDDDD00 00000000
				// 0000bbbb CC000000 AAAAAA00 00000000

				Vector256<sbyte> t2 = Avx2.And (str, maskBB);
				// 00000000 00xxxxxx 000000vv WWWW0000
				// 00000000 00uuuuuu 000000ss TTTT0000
				// 00000000 00rrrrrr 000000pp QQQQ0000
				// 00000000 00oooooo 000000mm NNNN0000
				// 00000000 00llllll 000000jj KKKK0000
				// 00000000 00iiiiii 000000gg HHHH0000
				// 00000000 00ffffff 000000dd EEEE0000
				// 00000000 00cccccc 000000aa BBBB0000

				Vector256<ushort> t1 = Avx2.MultiplyHigh (t0.AsUInt16 (), shiftAC);
				// 00000000 00wwwwXX 00000000 00VVVVVV
				// 00000000 00ttttUU 00000000 00SSSSSS
				// 00000000 00qqqqRR 00000000 00PPPPPP
				// 00000000 00nnnnOO 00000000 00MMMMMM
				// 00000000 00kkkkLL 00000000 00JJJJJJ
				// 00000000 00hhhhII 00000000 00GGGGGG
				// 00000000 00eeeeFF 00000000 00DDDDDD
				// 00000000 00bbbbCC 00000000 00AAAAAA

				Vector256<short> t3 = Avx2.MultiplyLow (t2.AsInt16 (), shiftBB);
				// 00xxxxxx 00000000 00vvWWWW 00000000
				// 00uuuuuu 00000000 00ssTTTT 00000000
				// 00rrrrrr 00000000 00ppQQQQ 00000000
				// 00oooooo 00000000 00mmNNNN 00000000
				// 00llllll 00000000 00jjKKKK 00000000
				// 00iiiiii 00000000 00ggHHHH 00000000
				// 00ffffff 00000000 00ddEEEE 00000000
				// 00cccccc 00000000 00aaBBBB 00000000

				str = Avx2.Or (t1.AsSByte (), t3.AsSByte ());
				// 00xxxxxx 00wwwwXX 00vvWWWW 00VVVVVV
				// 00uuuuuu 00ttttUU 00ssTTTT 00SSSSSS
				// 00rrrrrr 00qqqqRR 00ppQQQQ 00PPPPPP
				// 00oooooo 00nnnnOO 00mmNNNN 00MMMMMM
				// 00llllll 00kkkkLL 00jjKKKK 00JJJJJJ
				// 00iiiiii 00hhhhII 00ggHHHH 00GGGGGG
				// 00ffffff 00eeeeFF 00ddEEEE 00DDDDDD
				// 00cccccc 00bbbbCC 00aaBBBB 00AAAAAA

				// Translation
				// LUT contains Absolute offset for all ranges:
				// Translate values 0..63 to the Base64 alphabet. There are five sets:
				// #  From      To         Abs    Index  Characters
				// 0  [0..25]   [65..90]   +65        0  ABCDEFGHIJKLMNOPQRSTUVWXYZ
				// 1  [26..51]  [97..122]  +71        1  abcdefghijklmnopqrstuvwxyz
				// 2  [52..61]  [48..57]    -4  [2..11]  0123456789
				// 3  [62]      [43]       -19       12  +
				// 4  [63]      [47]       -16       13  /

				// Create LUT indices from input:
				// the index for range #0 is right, others are 1 less than expected:
				Vector256<byte> indices = Avx2.SubtractSaturate (str.AsByte (), const51);

				// mask is 0xFF (-1) for range #[1..4] and 0x00 for range #0:
				Vector256<sbyte> mask = Avx2.CompareGreaterThan (str, const25);

				// subtract -1, so add 1 to indices for range #[1..4], All indices are now correct:
				Vector256<sbyte> tmp = Avx2.Subtract (indices.AsSByte (), mask);

				// Add offsets to input values:
				str = Avx2.Add (str, Avx2.Shuffle (lut, tmp));

				Avx.Store (outptr, str.AsByte ());

				inptr += 24;
				outptr += 32;
				quartets += 8;

				if (inptr > maxOffset)
					break;

				// Load at inptr-4, as required by Reshuffle (already shifted by -4)
				str = Avx.LoadVector256 (inptr).AsSByte ();
			} while (true);

			inptr += 4;

			int nread = (int) (inptr - input);

			output = outptr;
			input = inptr;

			return nread;
		}

#if NET9_0_OR_GREATER // Part of the Arm APIs used here added in .NET 9
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe int AdvSimdEncode (ref byte* input, ref byte* output, byte* maxOffset, ref int quartets)
		{
			// C# implementation of https://github.com/aklomp/base64/blob/3a5add8652076612a8407627a42c768736a4263f/lib/arch/neon64/enc_loop.c
			Vector128<byte> str1;
			Vector128<byte> str2;
			Vector128<byte> str3;
			Vector128<byte> res1;
			Vector128<byte> res2;
			Vector128<byte> res3;
			Vector128<byte> res4;
			Vector128<byte> tblEnc1 = Vector128.Create ("ABCDEFGHIJKLMNOP"u8).AsByte ();
			Vector128<byte> tblEnc2 = Vector128.Create ("QRSTUVWXYZabcdef"u8).AsByte ();
			Vector128<byte> tblEnc3 = Vector128.Create ("ghijklmnopqrstuv"u8).AsByte ();
			Vector128<byte> tblEnc4 = Vector128.Create ("wxyz0123456789+/"u8).AsByte ();
			byte* outptr = output;
			byte* inptr = input;

			// If we have Neon support, pick off 48 bytes at a time for as long as we can.
			do {
				// Load 48 bytes and deinterleave:
				(str1, str2, str3) = AdvSimd.Arm64.Load3xVector128AndUnzip (inptr);

				// Divide bits of three input bytes over four output bytes:
				res1 = AdvSimd.ShiftRightLogical (str1, 2);
				res2 = AdvSimd.ShiftRightLogical (str2, 4);
				res3 = AdvSimd.ShiftRightLogical (str3, 6);
				res2 = AdvSimd.ShiftLeftAndInsert (res2, str1, 4);
				res3 = AdvSimd.ShiftLeftAndInsert (res3, str2, 2);

				// Clear top two bits:
				res2 &= AdvSimd.DuplicateToVector128 ((byte) 0x3F);
				res3 &= AdvSimd.DuplicateToVector128 ((byte) 0x3F);
				res4 = str3 & AdvSimd.DuplicateToVector128 ((byte) 0x3F);

				// The bits have now been shifted to the right locations;
				// translate their values 0..63 to the Base64 alphabet.
				// Use a 64-byte table lookup:
				res1 = AdvSimd.Arm64.VectorTableLookup ((tblEnc1, tblEnc2, tblEnc3, tblEnc4), res1);
				res2 = AdvSimd.Arm64.VectorTableLookup ((tblEnc1, tblEnc2, tblEnc3, tblEnc4), res2);
				res3 = AdvSimd.Arm64.VectorTableLookup ((tblEnc1, tblEnc2, tblEnc3, tblEnc4), res3);
				res4 = AdvSimd.Arm64.VectorTableLookup ((tblEnc1, tblEnc2, tblEnc3, tblEnc4), res4);

				// Interleave and store result:
				AdvSimd.Arm64.StoreVectorAndZip (outptr, (res1, res2, res3, res4));

				inptr += 48;
				outptr += 64;
				quartets += 16;
			} while (inptr <= maxOffset);

			int nread = (int) (inptr - input);

			output = outptr;
			input = inptr;

			return nread;
		}
#endif

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe int Vector128Encode (ref byte* input, ref byte* output, byte* maxOffset, ref int quartets)
		{
			// If we have SSSE3 support, pick off 12 bytes at a time for as long as we can.
			// But because we read 16 bytes at a time, ensure we have enough room to do a
			// full 16-byte read without segfaulting.

			// input, bytes MSB to LSB:
			// 0 0 0 0 l k j i h g f e d c b a

			// The JIT won't hoist these "constants", so help it
			Vector128<byte> shuffleVec = Vector128.Create (0x01020001, 0x04050304, 0x07080607, 0x0A0B090A).AsByte ();
			Vector128<byte> lut = Vector128.Create (0xFCFC4741, 0xFCFCFCFC, 0xFCFCFCFC, 0x0000F0ED).AsByte ();
			Vector128<byte> maskAC = Vector128.Create (0x0fc0fc00).AsByte ();
			Vector128<byte> maskBB = Vector128.Create (0x003f03f0).AsByte ();
			Vector128<ushort> shiftAC = Vector128.Create (0x04000040).AsUInt16 ();
			Vector128<short> shiftBB = Vector128.Create (0x01000010).AsInt16 ();
			Vector128<byte> const51 = Vector128.Create ((byte) 51);
			Vector128<sbyte> const25 = Vector128.Create ((sbyte) 25);
			Vector128<byte> mask8F = Vector128.Create ((byte) 0x8F);

			byte* outptr = output;
			byte* inptr = input;

			do {
				Vector128<byte> str = Vector128.LoadUnsafe (ref *inptr);

				// Reshuffle
				str = SimdShuffle (str, shuffleVec, mask8F);
				// str, bytes MSB to LSB:
				// k l j k
				// h i g h
				// e f d e
				// b c a b

				Vector128<byte> t0 = str & maskAC;
				// bits, upper case are most significant bits, lower case are least significant bits
				// 0000kkkk LL000000 JJJJJJ00 00000000
				// 0000hhhh II000000 GGGGGG00 00000000
				// 0000eeee FF000000 DDDDDD00 00000000
				// 0000bbbb CC000000 AAAAAA00 00000000

				Vector128<byte> t2 = str & maskBB;
				// 00000000 00llllll 000000jj KKKK0000
				// 00000000 00iiiiii 000000gg HHHH0000
				// 00000000 00ffffff 000000dd EEEE0000
				// 00000000 00cccccc 000000aa BBBB0000

				Vector128<ushort> t1;
				if (Ssse3.IsSupported) {
					t1 = Sse2.MultiplyHigh (t0.AsUInt16 (), shiftAC);
				} else if (AdvSimd.IsSupported) {
					Vector128<ushort> odd = Vector128.ShiftRightLogical (AdvSimd.Arm64.UnzipOdd (t0.AsUInt16 (), t0.AsUInt16 ()), 6);
					Vector128<ushort> even = Vector128.ShiftRightLogical (AdvSimd.Arm64.UnzipEven (t0.AsUInt16 (), t0.AsUInt16 ()), 10);
					t1 = AdvSimd.Arm64.ZipLow (even, odd);
				} else {
					// explicitly recheck each IsSupported query to ensure that the trimmer can see which paths are live/dead
					t1 = default;
				}
				// 00000000 00kkkkLL 00000000 00JJJJJJ
				// 00000000 00hhhhII 00000000 00GGGGGG
				// 00000000 00eeeeFF 00000000 00DDDDDD
				// 00000000 00bbbbCC 00000000 00AAAAAA

				Vector128<short> t3 = t2.AsInt16 () * shiftBB;
				// 00llllll 00000000 00jjKKKK 00000000
				// 00iiiiii 00000000 00ggHHHH 00000000
				// 00ffffff 00000000 00ddEEEE 00000000
				// 00cccccc 00000000 00aaBBBB 00000000

				str = t1.AsByte () | t3.AsByte ();
				// 00llllll 00kkkkLL 00jjKKKK 00JJJJJJ
				// 00iiiiii 00hhhhII 00ggHHHH 00GGGGGG
				// 00ffffff 00eeeeFF 00ddEEEE 00DDDDDD
				// 00cccccc 00bbbbCC 00aaBBBB 00AAAAAA

				// Translation
				// LUT contains Absolute offset for all ranges:
				// Translate values 0..63 to the Base64 alphabet. There are five sets:
				// #  From      To         Abs    Index  Characters
				// 0  [0..25]   [65..90]   +65        0  ABCDEFGHIJKLMNOPQRSTUVWXYZ
				// 1  [26..51]  [97..122]  +71        1  abcdefghijklmnopqrstuvwxyz
				// 2  [52..61]  [48..57]    -4  [2..11]  0123456789
				// 3  [62]      [43]       -19       12  +
				// 4  [63]      [47]       -16       13  /

				// Create LUT indices from input:
				// the index for range #0 is right, others are 1 less than expected:
				Vector128<byte> indices;
				if (Ssse3.IsSupported) {
					indices = Sse2.SubtractSaturate (str.AsByte (), const51);
				} else if (AdvSimd.IsSupported) {
					indices = AdvSimd.SubtractSaturate (str.AsByte (), const51);
				} else {
					// explicitly recheck each IsSupported query to ensure that the trimmer can see which paths are live/dead
					indices = default;
				}

				// mask is 0xFF (-1) for range #[1..4] and 0x00 for range #0:
				Vector128<sbyte> mask = Vector128.GreaterThan (str.AsSByte (), const25);

				// subtract -1, so add 1 to indices for range #[1..4], All indices are now correct:
				Vector128<sbyte> tmp = indices.AsSByte () - mask;

				// Add offsets to input values:
				str += SimdShuffle (lut, tmp.AsByte (), mask8F);

				str.Store (outptr);

				inptr += 12;
				outptr += 16;
				quartets += 4;
			} while (inptr <= maxOffset);

			int nread = (int) (inptr - input);

			output = outptr;
			input = inptr;

			return nread;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> SimdShuffle (Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask8F)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			if (Ssse3.IsSupported)
				return Ssse3.Shuffle (left, right);

			return AdvSimd.Arm64.VectorTableLookup (left, right & mask8F);
		}
#endif

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int Encode (byte* input, int length, byte* output)
		{
			int remaining = length;
			byte* outptr = output;
			byte* inptr = input;

			if (length + saved > 2) {
				byte* inend = inptr + length - 2;
				int c1, c2, c3;

				c1 = saved < 1 ? *inptr++ : saved1;
				c2 = saved < 2 ? *inptr++ : saved2;
				c3 = *inptr++;

				do {
					// encode our triplet into a quartet
					*outptr++ = base64_alphabet[c1 >> 2];
					*outptr++ = base64_alphabet[(c2 >> 4) | ((c1 & 0x3) << 4)];
					*outptr++ = base64_alphabet[((c2 & 0x0f) << 2) | (c3 >> 6)];
					*outptr++ = base64_alphabet[c3 & 0x3f];

					// append a newline if we've reached our target quartet count
					if (++quartets >= quartetsPerLine) {
						*outptr++ = (byte) '\n';
						quartets = 0;
					}

					if (inptr >= inend)
						break;

					c1 = *inptr++;
					c2 = *inptr++;
					c3 = *inptr++;
				} while (true);

				remaining = 2 - (int) (inptr - inend);
				saved = 0;
			}

			if (remaining > 0) {
				// At this point, saved can only be 0 or 1.
				if (saved == 0) {
					// We can have up to 2 remaining input bytes.
					saved = (byte) remaining;
					saved1 = *inptr++;
					if (remaining == 2)
						saved2 = *inptr;
					else
						saved2 = 0;
				} else {
					// We have 1 remaining input byte.
					saved2 = *inptr++;
					saved = 2;
				}
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encode the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Encodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// encoded input. For estimating the size needed for the output buffer,
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
		public int Encode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
#if NET6_0_OR_GREATER
					if ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && EnableHardwareAcceleration) {
						// If we have hardware acceleration, use it.
						return HwAccelEncode (inptr + startIndex, length, outptr);
					} else {
						return Encode (inptr + startIndex, length, outptr);
					}
#else
					return Encode (inptr + startIndex, length, outptr);
#endif
				}
			}
		}

		unsafe int Flush (byte* input, int length, byte* output)
		{
			byte* outptr = output;

			if (length > 0) {
#if NET6_0_OR_GREATER
				if ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && EnableHardwareAcceleration) {
					// If we have hardware acceleration, use it.
					outptr += HwAccelEncode (input, length, outptr);
				} else {
					outptr += Encode (input, length, outptr);
				}
#else
				outptr += Encode (input, length, outptr);
#endif
			}

			if (saved >= 1) {
				int c1 = saved1;
				int c2 = saved2;

				*outptr++ = base64_alphabet[c1 >> 2];
				*outptr++ = base64_alphabet[c2 >> 4 | ((c1 & 0x3) << 4)];
				if (saved == 2)
					*outptr++ = base64_alphabet[(c2 & 0x0f) << 2];
				else
					*outptr++ = (byte) '=';
				*outptr++ = (byte) '=';
				quartets++;
				saved = 0;
			}

			if (quartets > 0) {
				*outptr++ = (byte) '\n';
				quartets = 0;
			}

			return (int) (outptr - output);
		}

		/// <summary>
		/// Encode the specified input into the output buffer, flushing any internal buffer state as well.
		/// </summary>
		/// <remarks>
		/// <para>Encodes the specified input into the output buffer, flushing any internal state as well.</para>
		/// <para>The output buffer should be large enough to hold all the
		/// encoded input. For estimating the size needed for the output buffer,
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
		public int Flush (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Flush (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Reset the encoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the encoder.
		/// </remarks>
		public void Reset ()
		{
			quartets = 0;
			saved1 = 0;
			saved2 = 0;
			saved = 0;
		}
	}
}
