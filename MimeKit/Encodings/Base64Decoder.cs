//
// Base64Decoder.cs
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

// Note: The SIMD (Avx2, AdvSimd, and Ssse3) block decoding kernels of the base64 decoder were
// borrowed (and modified) from the .NET Core implementation located at:
// https://github.com/dotnet/runtime/blob/release/9.0/src/libraries/System.Private.CoreLib/src/System/Buffers/Text/Base64Helper/Base64DecoderHelper.cs
//
// The .NET Core implementation was, in turn, inspired by the work done by Wojciech Mula
// in his base64simd project: https://github.com/WojciechMula/base64simd
//
// Unlike Base64.DecodeFromUtf8(), the loop driving these kernels is designed for MIME's lenient
// base64 decoding rules: line breaks, whitespace, garbage and (possibly mid-stream) padding are
// all handed off to the scalar decoder, which also preserves the decoder state across calls.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET6_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
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

		int previous;
		uint saved;
		byte bytes;

		static Base64Decoder ()
		{
#if NET6_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian);
#endif
		}

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

#if NET6_0_OR_GREATER
		/// <summary>
		/// Get or set whether the <see cref="Base64Decoder"/> should use hardware acceleration when available.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the <see cref="Base64Decoder"/> should use hardware acceleration when available.</para>
		/// <para>Hardware acceleration defaults to <see langword="true"/> on systems that support SSSE3 (x86/x64) or
		/// AdvSimd (Arm64). The hardware accelerated code path produces identical results to the scalar implementation.</para>
		/// </remarks>
		/// <value><see langword="true"/> if hardware acceleration should be enabled; otherwise, <see langword="false"/>.</value>
		public static bool EnableHardwareAcceleration {
			get; set;
		}
#endif

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
			// decoding base64 converts 4 bytes of input into 3 bytes of output
			return ((inputLength / 4) * 3) + 3;
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
#endif
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int Decode (ref byte table, byte* input, byte* inend, byte* output)
		{
			byte* outptr = output;
			byte* inptr = input;

			// decode every quartet into a triplet
			while (inptr < inend) {
				byte c = *inptr++;
				byte rank = Unsafe.Add (ref table, c);

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
			ref byte table = ref MemoryMarshal.GetReference (base64_rank);

			return Decode (ref table, input, input + length, output);
		}

#if NET6_0_OR_GREATER
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> SimdShuffle (Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask8F)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			if (Ssse3.IsSupported)
				return Ssse3.Shuffle (left, right);

			return AdvSimd.Arm64.VectorTableLookup (left, right & mask8F);
		}

		// Decodes as many complete 32-byte blocks as possible, stopping at the first block that contains
		// anything other than the 64 base64 alphabet characters (e.g. whitespace, garbage, or '=' padding).
		// Each block writes 32 bytes of output (of which only the first 24 are meaningful).
		static unsafe void Avx2DecodeBlocks (ref byte* input, ref byte* output, byte* inend, byte* outend)
		{
			Debug.Assert (Avx2.IsSupported);

			// See Vector128DecodeBlocks() for an explanation of how this works.
			var lutHi = Vector256.Create (
				0x10, 0x10, 0x01, 0x02, 0x04, 0x08, 0x04, 0x08, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10,
				0x10, 0x10, 0x01, 0x02, 0x04, 0x08, 0x04, 0x08, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, (sbyte) 0x10);
			var lutLo = Vector256.Create (
				0x15, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x13, 0x1A, 0x1B, 0x1B, 0x1B, 0x1A,
				0x15, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x13, 0x1A, 0x1B, 0x1B, 0x1B, (sbyte) 0x1A);
			var lutShift = Vector256.Create (
				0, 16, 19, 4, -65, -65, -71, -71, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 16, 19, 4, -65, -65, -71, -71, 0, 0, 0, 0, 0, 0, 0, (sbyte) 0);
			var packBytesInLaneMask = Vector256.Create (
				2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1,
				2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, (sbyte) -1);
			var packLanesControl = Vector256.Create (0, 1, 2, 4, 5, 6, -1, -1);
			var mask2F = Vector256.Create ((sbyte) '/');
			var mergeConstant0 = Vector256.Create (0x01400140).AsSByte ();
			var mergeConstant1 = Vector256.Create (0x00011000).AsInt16 ();
			byte* outptr = output;
			byte* inptr = input;

			while (inend - inptr >= 32 && outend - outptr >= 32) {
				var str = Avx.LoadVector256 (inptr).AsSByte ();
				var hiNibbles = Avx2.And (Avx2.ShiftRightLogical (str.AsInt32 (), 4).AsSByte (), mask2F);
				var loNibbles = Avx2.And (str, mask2F);
				var hi = Avx2.Shuffle (lutHi, hiNibbles);
				var lo = Avx2.Shuffle (lutLo, loNibbles);

				if (!Avx.TestZ (lo, hi))
					break;

				var eq2F = Avx2.CompareEqual (str, mask2F);
				var shift = Avx2.Shuffle (lutShift, Avx2.Add (eq2F, hiNibbles));
				str = Avx2.Add (str, shift);

				var merged = Avx2.MultiplyAddAdjacent (str.AsByte (), mergeConstant0);
				var packed = Avx2.MultiplyAddAdjacent (merged, mergeConstant1);
				packed = Avx2.Shuffle (packed.AsSByte (), packBytesInLaneMask).AsInt32 ();
				packed = Avx2.PermuteVar8x32 (packed, packLanesControl);

				Avx.Store (outptr, packed.AsByte ());
				inptr += 32;
				outptr += 24;
			}

			output = outptr;
			input = inptr;
		}

		// Decodes as many 16-byte blocks as possible. Each block writes 16 bytes of output (of which only the
		// first 12 are meaningful).
		//
		// If a block contains anything other than the 64 base64 alphabet characters (e.g. whitespace, garbage,
		// or '=' padding), the complete quartets that precede the first invalid character are still decoded
		// (quartets are independent of each other) and the return value is the offset (relative to the updated
		// input pointer) of that invalid character. Everything from the updated input pointer up to and
		// including that offset must be handled by the scalar decoder.
		//
		// Returns -1 if decoding stopped because there was not enough input or output space remaining.
		static unsafe int Vector128DecodeBlocks (ref byte* input, ref byte* output, byte* inend, byte* outend)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

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
			// (6) Everything else => invalid input (including '=')
			//
			// A character is valid if and only if the AND of the hi-nibble and lo-nibble lookups is 0.
			// The offset to add is looked up using a perfect hash: ((c >> 4) & 0x2F) + ((c == '/') ? 0xFF : 0x00).
			var lutHi = Vector128.Create (0x02011010, 0x08040804, 0x10101010, 0x10101010).AsByte ();
			var lutLo = Vector128.Create (0x11111115, 0x11111111, 0x1A131111, 0x1A1B1B1B).AsByte ();
			var lutShift = Vector128.Create (0x04131000, 0xb9b9bfbf, 0x00000000, 0x00000000).AsByte ();
			var packBytesMask = Vector128.Create (0x06000102, 0x090A0405, 0x0C0D0E08, 0xffffffff).AsByte ();
			var mergeConstant0 = Vector128.Create (0x01400140).AsSByte ();
			var mergeConstant1 = Vector128.Create (0x00011000).AsInt16 ();
			var one = Vector128.Create ((byte) 1);
			var mask2F = Vector128.Create ((byte) '/');
			var mask8F = Vector128.Create ((byte) 0x8F);
			byte* outptr = output;
			byte* inptr = input;
			int offset = -1;

			while (inend - inptr >= 16 && outend - outptr >= 16) {
				var str = Vector128.Load (inptr);
				var hiNibbles = Vector128.ShiftRightLogical (str.AsInt32 (), 4).AsByte () & mask2F;
				var loNibbles = str & mask2F;
				var hi = SimdShuffle (lutHi, hiNibbles, mask8F);
				var lo = SimdShuffle (lutLo, loNibbles, mask8F);
				var invalid = lo & hi;
				int valid = 16;

				if (invalid != Vector128<byte>.Zero) {
					uint mask = Vector128.ExtractMostSignificantBits (~Vector128.Equals (invalid, Vector128<byte>.Zero));

					valid = BitOperations.TrailingZeroCount (mask);

					if (valid < 4) {
						offset = valid;
						break;
					}
				}

				var eq2F = Vector128.Equals (str, mask2F);
				var shift = SimdShuffle (lutShift, eq2F + hiNibbles, mask8F);
				str += shift;

				// in, bits, upper case are most significant bits, lower case are least significant bits
				// 00llllll 00kkkkLL 00jjKKKK 00JJJJJJ
				// 00iiiiii 00hhhhII 00ggHHHH 00GGGGGG
				// 00ffffff 00eeeeFF 00ddEEEE 00DDDDDD
				// 00cccccc 00bbbbCC 00aaBBBB 00AAAAAA
				Vector128<short> merged;
				if (Ssse3.IsSupported) {
					merged = Ssse3.MultiplyAddAdjacent (str, mergeConstant0);
				} else {
					var evens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (str, one).GetLower (), 6);
					var odds = AdvSimd.Arm64.TransposeOdd (str, Vector128<byte>.Zero).AsUInt16 ();
					merged = Vector128.Add (evens, odds).AsInt16 ();
				}

				// 0000kkkk LLllllll 0000JJJJ JJjjKKKK
				// 0000hhhh IIiiiiii 0000GGGG GGggHHHH
				// 0000eeee FFffffff 0000DDDD DDddEEEE
				// 0000bbbb CCcccccc 0000AAAA AAaaBBBB
				Vector128<int> packed;
				if (Ssse3.IsSupported) {
					packed = Sse2.MultiplyAddAdjacent (merged, mergeConstant1);
				} else {
					var ievens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (merged, one.AsInt16 ()).GetLower (), 12);
					var iodds = AdvSimd.Arm64.TransposeOdd (merged, Vector128<short>.Zero).AsInt32 ();
					packed = Vector128.Add (ievens, iodds);
				}

				// 00000000 JJJJJJjj KKKKkkkk LLllllll
				// 00000000 GGGGGGgg HHHHhhhh IIiiiiii
				// 00000000 DDDDDDdd EEEEeeee FFffffff
				// 00000000 AAAAAAaa BBBBbbbb CCcccccc
				var result = SimdShuffle (packed.AsByte (), packBytesMask, mask8F);

				// 00000000 00000000 00000000 00000000
				// LLllllll KKKKkkkk JJJJJJjj IIiiiiii
				// HHHHhhhh GGGGGGgg FFffffff EEEEeeee
				// DDDDDDdd CCcccccc BBBBbbbb AAAAAAaa
				if (valid < 16) {
					int quartets = valid >> 2;
					byte* next = inptr + (quartets << 2);

					// The 16-byte store must not clobber any unconsumed input when decoding in-place. If it
					// would, let the scalar decoder handle everything up to and including the invalid character.
					if (outptr + 16 > next && outptr < inend) {
						offset = valid;
						break;
					}

					result.Store (outptr);
					outptr += quartets * 3;
					inptr = next;
					offset = valid & 3;
					break;
				}

				result.Store (outptr);
				inptr += 16;
				outptr += 12;
			}

			output = outptr;
			input = inptr;

			return offset;
		}

		[SkipLocalsInit]
		unsafe int HwAccelDecode (byte* input, int length, byte* output, int outputLength, bool useAvx2)
		{
			ref byte table = ref MemoryMarshal.GetReference (base64_rank);
			byte* outend = output + outputLength;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte* stop = null;

			while (true) {
				// Use the scalar decoder until we are aligned on a quartet boundary, the next character is a valid
				// (non-padding) base64 character, and we are past any invalid character that the SIMD kernel stopped on.
				byte* aligned = inptr;
				int n = bytes;

				while (aligned < inend) {
					byte c = *aligned;

					if (Unsafe.Add (ref table, c) != 0xFF) {
						if (n == 0 && c != (byte) '=' && aligned > stop)
							break;

						n = (n + 1) & 3;
					}

					aligned++;
				}

				if (aligned > inptr) {
					outptr += Decode (ref table, inptr, aligned, outptr);
					inptr = aligned;
				}

				if (inend - inptr < 16 || outend - outptr < 16) {
					// Not enough input (or output space) remaining for the SIMD kernels.
					outptr += Decode (ref table, inptr, inend, outptr);
					break;
				}

				if (useAvx2)
					Avx2DecodeBlocks (ref inptr, ref outptr, inend, outend);

				int offset = Vector128DecodeBlocks (ref inptr, ref outptr, inend, outend);

				stop = offset >= 0 ? inptr + offset : null;
			}

			return (int) (outptr - output);
		}

		unsafe int HwAccelDecode (byte[] input, int startIndex, int length, byte[] output, bool useAvx2)
		{
			ValidateArguments (input, startIndex, length, output);

			fixed (byte* inptr = input, outptr = output)
				return HwAccelDecode (inptr + startIndex, length, outptr, output.Length, useAvx2);
		}

		// The following internal entry points allow the unit tests to exercise each code path directly,
		// regardless of which path the public Decode() method would choose on the host machine.
		internal int Ssse3Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			if (!Ssse3.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelDecode (input, startIndex, length, output, false);
		}

		internal int Avx2Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			if (!Avx2.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelDecode (input, startIndex, length, output, true);
		}

		internal int AdvSimdDecode (byte[] input, int startIndex, int length, byte[] output)
		{
			if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelDecode (input, startIndex, length, output, false);
		}
#endif

		internal int ScalarDecode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output)
					return Decode (inptr + startIndex, length, outptr);
			}
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
#if NET6_0_OR_GREATER
					if ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && EnableHardwareAcceleration) {
						return HwAccelDecode (inptr + startIndex, length, outptr, output.Length, Avx2.IsSupported);
					} else {
						return Decode (inptr + startIndex, length, outptr);
					}
#else
					return Decode (inptr + startIndex, length, outptr);
#endif
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
