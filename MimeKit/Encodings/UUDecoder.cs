//
// UUDecoder.cs
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

#if NET6_0_OR_GREATER
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
#endif

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally decodes content encoded with the Unix-to-Unix encoding.
	/// </summary>
	/// <remarks>
	/// <para>The UUEncoding is an encoding that predates MIME and was used to encode
	/// binary content such as images and other types of multimedia to ensure
	/// that the data remained intact when sent via 7bit transports such as SMTP.</para>
	/// <para>These days, the UUEncoding has largely been deprecated in favour of
	/// the base64 encoding, however, some older mail clients still use it.</para>
	/// </remarks>
	public class UUDecoder : IMimeDecoder
	{
		internal static ReadOnlySpan<byte> uudecode_rank => new byte[256] {
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
			 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
			 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
			  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
		};

		enum UUDecoderState : byte {
			ExpectBegin,
			B,
			Be,
			Beg,
			Begi,
			Begin,
			ExpectPayload,
			Payload,
			Ended,
		}

		readonly UUDecoderState initial;
		UUDecoderState state;
		byte nsaved;
		byte uulen;
		bool eoln;
		uint saved;

		static UUDecoder ()
		{
#if NET6_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian);
#endif
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="UUDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix decoder.
		/// </remarks>
		/// <param name="payloadOnly">
		/// If <see langword="true" />, decoding begins immediately rather than after finding a begin-line.
		/// </param>
		public UUDecoder (bool payloadOnly)
		{
			initial = payloadOnly ? UUDecoderState.Payload : UUDecoderState.ExpectBegin;
			Reset ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="UUDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix decoder.
		/// </remarks>
		public UUDecoder () : this (false)
		{
		}

		/// <summary>
		/// Clone the <see cref="UUDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="UUDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="UUDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			return new UUDecoder (initial == UUDecoderState.Payload) {
				state = state,
				nsaved = nsaved,
				saved = saved,
				uulen = uulen,
				eoln = eoln
			};
		}

#if NET6_0_OR_GREATER
		/// <summary>
		/// Get or set whether the <see cref="UUDecoder"/> should use hardware acceleration when available.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the <see cref="UUDecoder"/> should use hardware acceleration when available.</para>
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
			get { return ContentEncoding.UUEncode; }
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
			// add an extra 3 bytes for the saved input bytes from previous decode step
			return inputLength + 3;
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

		unsafe byte* ScanBeginMarker (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (state == UUDecoderState.ExpectBegin) {
					if (nsaved != 0 && nsaved != (byte) '\n') {
						while (inptr < inend && *inptr != (byte) '\n')
							inptr++;

						if (inptr == inend) {
							nsaved = *(inptr - 1);
							return inptr;
						}

						nsaved = *inptr++;
						if (inptr == inend)
							return inptr;
					}

					nsaved = *inptr++;
					if (nsaved != (byte) 'b')
						continue;

					state = UUDecoderState.B;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.B) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'e') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Be;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Be) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'g') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Beg;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Beg) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'i') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Begi;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Begi) {
					nsaved = *inptr++;
					if (nsaved != (byte) 'n') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.Begin;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.Begin) {
					nsaved = *inptr++;
					if (nsaved != (byte) ' ') {
						state = UUDecoderState.ExpectBegin;
						continue;
					}

					state = UUDecoderState.ExpectPayload;
					if (inptr == inend)
						return inptr;
				}

				if (state == UUDecoderState.ExpectPayload) {
					while (inptr < inend && *inptr != (byte) '\n')
						inptr++;

					if (inptr == inend)
						return inptr;

					state = UUDecoderState.Payload;
					nsaved = 0;

					return inptr + 1;
				}
			}

			return inptr;
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
			if (state == UUDecoderState.Ended)
				return 0;

			bool last_was_eoln = eoln;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			if (state < UUDecoderState.Payload) {
				if ((inptr = ScanBeginMarker (inptr, inend)) == inend)
					return 0;
			}

			while (inptr < inend) {
				if (*inptr == (byte) '\r') {
					inptr++;
					continue;
				}

				if (*inptr == (byte) '\n') {
					last_was_eoln = true;
					inptr++;
					continue;
				}

				if (uulen == 0 || last_was_eoln) {
					// first octet on a line is the uulen octet
					uulen = uudecode_rank[*inptr];
					last_was_eoln = false;
					if (uulen == 0) {
						state = UUDecoderState.Ended;
						break;
					}

					inptr++;
					continue;
				}

				c = *inptr++;

				if (uulen > 0) {
					// save the byte
					saved = (saved << 8) | c;
					nsaved++;

					if (nsaved == 4) {
						byte b0 = (byte) ((saved >> 24) & 0xFF);
						byte b1 = (byte) ((saved >> 16) & 0xFF);
						byte b2 = (byte) ((saved >> 8) & 0xFF);
						byte b3 = (byte) (saved & 0xFF);

						if (uulen >= 3) {
							*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
							*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
							*outptr++ = (byte) (uudecode_rank[b2] << 6 | uudecode_rank[b3]);
							uulen -= 3;
						} else {
							if (uulen >= 1) {
								*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
								uulen--;
							}

							if (uulen >= 1) {
								*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
								uulen--;
							}
						}

						nsaved = 0;
						saved = 0;
					}
				} else {
					break;
				}
			}

			eoln = last_was_eoln;

			return (int) (outptr - output);
		}

#if NET6_0_OR_GREATER
		// Unlike base64, every byte maps to a 6-bit value in the uuencoding: (c - 0x20) & 0x3F. Once the sextets have
		// been computed, packing each quartet of sextets into 3 bytes works exactly like base64 (see Base64Decoder).
		//
		// The primary SIMD kernel, DecodeLines(), decodes whole lines at a time: it reads the length octet, verifies that
		// it is followed by exactly the number of quartets needed to encode that many bytes and then a line break, and
		// decodes the quartets using (potentially overlapping) vector loads.
		//
		// The secondary SIMD kernels are only used in the middle of a line (after the scalar decoder has consumed the length octet)
		// when there are no saved sextets, and they decode at most maxQuartets quartets (the number of quartets that
		// will produce exactly 3 bytes each according to the line's remaining length). Since the scalar decoder ignores
		// '\r' and treats '\n' as the end of the line, the kernels stop at the first quartet that contains either.

		// Returns true if storing a vector of the specified size at outptr will not clobber any unconsumed input (which
		// can only happen when decoding in-place).
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static unsafe bool CanStore (byte* outptr, int size, byte* next, byte* inend)
		{
			return outptr + size <= next || outptr >= inend;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> SimdShuffle (Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask8F)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			if (Ssse3.IsSupported)
				return Ssse3.Shuffle (left, right);

			return AdvSimd.Arm64.VectorTableLookup (left, right & mask8F);
		}

		// Decodes 8 quartets (32 bytes of input) into 24 bytes of output (the remaining 8 bytes are garbage).
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector256<byte> Avx2Decode (Vector256<byte> str, Vector256<sbyte> packBytesInLaneMask, Vector256<int> packLanesControl, Vector256<sbyte> mergeConstant0, Vector256<short> mergeConstant1, Vector256<byte> space, Vector256<byte> mask3F)
		{
			var sextets = (str - space) & mask3F;
			var merged = Avx2.MultiplyAddAdjacent (sextets, mergeConstant0);
			var packed = Avx2.MultiplyAddAdjacent (merged, mergeConstant1);
			packed = Avx2.Shuffle (packed.AsSByte (), packBytesInLaneMask).AsInt32 ();
			packed = Avx2.PermuteVar8x32 (packed, packLanesControl);

			return packed.AsByte ();
		}

		// Decodes 4 quartets (16 bytes of input) into 12 bytes of output (the remaining 4 bytes are garbage).
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> Vector128Decode (Vector128<byte> str, Vector128<byte> packBytesMask, Vector128<sbyte> mergeConstant0, Vector128<short> mergeConstant1, Vector128<byte> space, Vector128<byte> mask3F, Vector128<byte> mask8F, Vector128<byte> one)
		{
			// Each byte now holds a sextet: 00aaaaaa 00bbbbbb 00cccccc 00dddddd (in memory order).
			var sextets = (str - space) & mask3F;

			// Each 16-bit word = (a << 6) | b and (c << 6) | d.
			Vector128<short> merged;
			if (Ssse3.IsSupported) {
				merged = Ssse3.MultiplyAddAdjacent (sextets, mergeConstant0);
			} else {
				var evens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (sextets, one).GetLower (), 6);
				var odds = AdvSimd.Arm64.TransposeOdd (sextets, Vector128<byte>.Zero).AsUInt16 ();
				merged = Vector128.Add (evens, odds).AsInt16 ();
			}

			// Each 32-bit word = (a << 18) | (b << 12) | (c << 6) | d.
			Vector128<int> packed;
			if (Ssse3.IsSupported) {
				packed = Sse2.MultiplyAddAdjacent (merged, mergeConstant1);
			} else {
				var ievens = AdvSimd.ShiftLeftLogicalWideningLower (AdvSimd.Arm64.UnzipEven (merged, one.AsInt16 ()).GetLower (), 12);
				var iodds = AdvSimd.Arm64.TransposeOdd (merged, Vector128<short>.Zero).AsInt32 ();
				packed = Vector128.Add (ievens, iodds);
			}

			// Swap the 3 meaningful bytes of each int into big-endian order and pack them together.
			return SimdShuffle (packed.AsByte (), packBytesMask, mask8F);
		}

		// Decodes up to maxQuartets quartets, 8 quartets (32 bytes of input) at a time. Returns the number of quartets decoded.
		static unsafe int Avx2DecodeQuartets (ref byte* input, ref byte* output, byte* inend, byte* outend, int maxQuartets)
		{
			Debug.Assert (Avx2.IsSupported);

			// See Vector128DecodeQuartets() for an explanation of how this works.
			var packBytesInLaneMask = Vector256.Create (
				2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1,
				2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, (sbyte) -1);
			var packLanesControl = Vector256.Create (0, 1, 2, 4, 5, 6, -1, -1);
			var mergeConstant0 = Vector256.Create (0x01400140).AsSByte ();
			var mergeConstant1 = Vector256.Create (0x00011000).AsInt16 ();
			var space = Vector256.Create ((byte) 0x20);
			var mask3F = Vector256.Create ((byte) 0x3F);
			var cr = Vector256.Create ((byte) '\r');
			var lf = Vector256.Create ((byte) '\n');
			byte* outptr = output;
			byte* inptr = input;
			int decoded = 0;

			while (decoded < maxQuartets && inend - inptr >= 32 && outend - outptr >= 32) {
				var str = Vector256.Load (inptr);
				uint eoln = (Vector256.Equals (str, cr) | Vector256.Equals (str, lf)).ExtractMostSignificantBits ();
				int quartets = Math.Min (BitOperations.TrailingZeroCount (eoln) >> 2, 8);

				quartets = Math.Min (quartets, maxQuartets - decoded);

				if (quartets == 0 || !CanStore (outptr, 32, inptr + (quartets << 2), inend))
					break;

				var packed = Avx2Decode (str, packBytesInLaneMask, packLanesControl, mergeConstant0, mergeConstant1, space, mask3F);

				Avx.Store (outptr, packed);
				inptr += quartets << 2;
				outptr += quartets * 3;
				decoded += quartets;

				if (quartets < 8)
					break;
			}

			output = outptr;
			input = inptr;

			return decoded;
		}

		// Decodes up to maxQuartets quartets, 4 quartets (16 bytes of input) at a time. Returns the number of quartets decoded.
		static unsafe int Vector128DecodeQuartets (ref byte* input, ref byte* output, byte* inend, byte* outend, int maxQuartets)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			var packBytesMask = Vector128.Create (0x06000102, 0x090A0405, 0x0C0D0E08, 0xffffffff).AsByte ();
			var mergeConstant0 = Vector128.Create (0x01400140).AsSByte ();
			var mergeConstant1 = Vector128.Create (0x00011000).AsInt16 ();
			var space = Vector128.Create ((byte) 0x20);
			var mask3F = Vector128.Create ((byte) 0x3F);
			var mask8F = Vector128.Create ((byte) 0x8F);
			var cr = Vector128.Create ((byte) '\r');
			var lf = Vector128.Create ((byte) '\n');
			var one = Vector128.Create ((byte) 1);
			byte* outptr = output;
			byte* inptr = input;
			int decoded = 0;

			while (decoded < maxQuartets && inend - inptr >= 16 && outend - outptr >= 16) {
				var str = Vector128.Load (inptr);
				uint eoln = (Vector128.Equals (str, cr) | Vector128.Equals (str, lf)).ExtractMostSignificantBits ();
				int quartets = Math.Min (BitOperations.TrailingZeroCount (eoln) >> 2, 4);

				quartets = Math.Min (quartets, maxQuartets - decoded);

				if (quartets == 0 || !CanStore (outptr, 16, inptr + (quartets << 2), inend))
					break;

				var result = Vector128Decode (str, packBytesMask, mergeConstant0, mergeConstant1, space, mask3F, mask8F, one);

				result.Store (outptr);
				inptr += quartets << 2;
				outptr += quartets * 3;
				decoded += quartets;

				if (quartets < 4)
					break;
			}

			output = outptr;
			input = inptr;

			return decoded;
		}

		// Decodes as many complete, well-formed lines as possible (a length octet followed by exactly the number of encoded
		// quartets needed for that length and then a "\n" or "\r\n" sequence). Since each line is validated before the
		// output pointer is advanced, the scalar decoder can simply pick up where this method left off for anything else.
		// Returns true if at least one line was decoded.
		static unsafe bool DecodeLines (ref byte* input, ref byte* output, byte* inend, byte* outend, bool useAvx2)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			var packBytesMask = Vector128.Create (0x06000102, 0x090A0405, 0x0C0D0E08, 0xffffffff).AsByte ();
			var mergeConstant0 = Vector128.Create (0x01400140).AsSByte ();
			var mergeConstant1 = Vector128.Create (0x00011000).AsInt16 ();
			var space = Vector128.Create ((byte) 0x20);
			var mask3F = Vector128.Create ((byte) 0x3F);
			var mask8F = Vector128.Create ((byte) 0x8F);
			var cr = Vector128.Create ((byte) '\r');
			var lf = Vector128.Create ((byte) '\n');
			var one = Vector128.Create ((byte) 1);
			byte* outptr = output;
			byte* inptr = input;

			while (inptr < inend && *inptr != (byte) '\r' && *inptr != (byte) '\n') {
				int length = (*inptr - 0x20) & 0x3F;
				int quartets = (length + 2) / 3;
				int count = quartets << 2;

				// Note: Short lines are rare (usually only the last line) and are left for the scalar decoder.
				if (quartets < 4 || inend - inptr <= count + 1)
					break;

				byte* data = inptr + 1;
				byte* eoln = data + count;
				byte* next;

				if (*eoln == (byte) '\n')
					next = eoln + 1;
				else if (*eoln == (byte) '\r' && eoln + 1 < inend && eoln[1] == (byte) '\n')
					next = eoln + 2;
				else
					break;

				// Stores can write up to 8 bytes past the end of the decoded line.
				int maxStore = (quartets * 3) + 8;
				if (outend - outptr < maxStore || !CanStore (outptr, maxStore, inptr, inend))
					break;

				// Note: Each chunk of input is decoded and stored before checking for '\r' or '\n' within the line, but
				// since the outptr is not advanced unless the line is well-formed, any stored garbage will be overwritten.
				bool invalid;

				if (useAvx2 && count >= 32) {
					var packBytesInLaneMask = Vector256.Create (
						2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1,
						2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, (sbyte) -1);
					var packLanesControl = Vector256.Create (0, 1, 2, 4, 5, 6, -1, -1);
					var mergeConstant256_0 = Vector256.Create (0x01400140).AsSByte ();
					var mergeConstant256_1 = Vector256.Create (0x00011000).AsInt16 ();
					var space256 = Vector256.Create ((byte) 0x20);
					var mask3F256 = Vector256.Create ((byte) 0x3F);
					var cr256 = Vector256.Create ((byte) '\r');
					var lf256 = Vector256.Create ((byte) '\n');
					var eolnMask = Vector256<byte>.Zero;
					Vector256<byte> str;
					int index = 0;

					for (; index + 32 < count; index += 32) {
						str = Vector256.Load (data + index);
						eolnMask |= Vector256.Equals (str, cr256) | Vector256.Equals (str, lf256);
						Avx2Decode (str, packBytesInLaneMask, packLanesControl, mergeConstant256_0, mergeConstant256_1, space256, mask3F256).Store (outptr + ((index >> 2) * 3));
					}

					// Decode the last 8 quartets of the line (which may overlap with the previous chunk).
					index = count - 32;
					str = Vector256.Load (data + index);
					eolnMask |= Vector256.Equals (str, cr256) | Vector256.Equals (str, lf256);
					Avx2Decode (str, packBytesInLaneMask, packLanesControl, mergeConstant256_0, mergeConstant256_1, space256, mask3F256).Store (outptr + ((index >> 2) * 3));

					invalid = eolnMask != Vector256<byte>.Zero;
				} else {
					var eolnMask = Vector128<byte>.Zero;
					Vector128<byte> str;
					int index = 0;

					for (; index + 16 < count; index += 16) {
						str = Vector128.Load (data + index);
						eolnMask |= Vector128.Equals (str, cr) | Vector128.Equals (str, lf);
						Vector128Decode (str, packBytesMask, mergeConstant0, mergeConstant1, space, mask3F, mask8F, one).Store (outptr + ((index >> 2) * 3));
					}

					// Decode the last 4 quartets of the line (which may overlap with the previous chunk).
					index = count - 16;
					str = Vector128.Load (data + index);
					eolnMask |= Vector128.Equals (str, cr) | Vector128.Equals (str, lf);
					Vector128Decode (str, packBytesMask, mergeConstant0, mergeConstant1, space, mask3F, mask8F, one).Store (outptr + ((index >> 2) * 3));

					invalid = eolnMask != Vector128<byte>.Zero;
				}

				if (invalid)
					break;

				outptr += length;
				inptr = next;
			}

			bool decoded = inptr != input;

			output = outptr;
			input = inptr;

			return decoded;
		}

		[SkipLocalsInit]
		internal unsafe int HwAccelDecode (byte* input, int length, byte* output, int outputLength, bool useAvx2)
		{
			if (state == UUDecoderState.Ended)
				return 0;

			bool last_was_eoln = eoln;
			byte* outend = output + outputLength;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			if (state < UUDecoderState.Payload) {
				if ((inptr = ScanBeginMarker (inptr, inend)) == inend)
					return 0;
			}

			// Note: This is the scalar Decode() loop with the SIMD kernels inserted at the top.
			while (inptr < inend) {
				if (nsaved == 0 && (uulen == 0 || last_was_eoln) && DecodeLines (ref inptr, ref outptr, inend, outend, useAvx2)) {
					uulen = 0;
					last_was_eoln = true;

					if (inptr == inend)
						break;
				}

				if (nsaved == 0 && uulen >= 3 && !last_was_eoln && inend - inptr >= 16) {
					int maxQuartets = uulen / 3;
					int quartets = 0;

					if (useAvx2)
						quartets = Avx2DecodeQuartets (ref inptr, ref outptr, inend, outend, maxQuartets);

					quartets += Vector128DecodeQuartets (ref inptr, ref outptr, inend, outend, maxQuartets - quartets);
					uulen -= (byte) (quartets * 3);

					if (inptr == inend)
						break;
				}

				if (*inptr == (byte) '\r') {
					inptr++;
					continue;
				}

				if (*inptr == (byte) '\n') {
					last_was_eoln = true;
					inptr++;
					continue;
				}

				if (uulen == 0 || last_was_eoln) {
					// first octet on a line is the uulen octet
					uulen = uudecode_rank[*inptr];
					last_was_eoln = false;
					if (uulen == 0) {
						state = UUDecoderState.Ended;
						break;
					}

					inptr++;
					continue;
				}

				c = *inptr++;

				// save the byte
				saved = (saved << 8) | c;
				nsaved++;

				if (nsaved == 4) {
					byte b0 = (byte) ((saved >> 24) & 0xFF);
					byte b1 = (byte) ((saved >> 16) & 0xFF);
					byte b2 = (byte) ((saved >> 8) & 0xFF);
					byte b3 = (byte) (saved & 0xFF);

					if (uulen >= 3) {
						*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
						*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
						*outptr++ = (byte) (uudecode_rank[b2] << 6 | uudecode_rank[b3]);
						uulen -= 3;
					} else {
						if (uulen >= 1) {
							*outptr++ = (byte) (uudecode_rank[b0] << 2 | uudecode_rank[b1] >> 4);
							uulen--;
						}

						if (uulen >= 1) {
							*outptr++ = (byte) (uudecode_rank[b1] << 4 | uudecode_rank[b2] >> 2);
							uulen--;
						}
					}

					nsaved = 0;
					saved = 0;
				}
			}

			eoln = last_was_eoln;

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
					if ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian && EnableHardwareAcceleration)
						return HwAccelDecode (inptr + startIndex, length, outptr, output.Length, Avx2.IsSupported);
#endif

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
			state = initial;
			nsaved = 0;
			saved = 0;
			uulen = 0;
			eoln = false;
		}
	}
}
