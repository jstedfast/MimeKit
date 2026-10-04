//
// UUEncoder.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
#endif

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally encodes content using the Unix-to-Unix encoding.
	/// </summary>
	/// <remarks>
	/// <para>The UUEncoding is an encoding that predates MIME and was used to encode
	/// binary content such as images and other types of multimedia to ensure
	/// that the data remained intact when sent via 7bit transports such as SMTP.</para>
	/// <para>These days, the UUEncoding has largely been deprecated in favour of
	/// the base64 encoding, however, some older mail clients still use it.</para>
	/// </remarks>
	public class UUEncoder : IMimeEncoder
	{
		const int MaxInputPerLine = 45;
		const int MaxOutputPerLine = ((MaxInputPerLine / 3) * 4) + 2;

		readonly byte[] uubuf = new byte[60];
		uint saved;
		byte nsaved;
		byte uulen;

		static UUEncoder ()
		{
#if NET6_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian);
#endif
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="UUEncoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new Unix-to-Unix encoder.
		/// </remarks>
		public UUEncoder ()
		{
		}

		/// <summary>
		/// Clone the <see cref="UUEncoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="UUEncoder"/> with exactly the same state as the current encoder.
		/// </remarks>
		/// <returns>A new <see cref="UUEncoder"/> with identical state.</returns>
		public IMimeEncoder Clone ()
		{
			var encoder = new UUEncoder ();

			Buffer.BlockCopy (uubuf, 0, encoder.uubuf, 0, uubuf.Length);
			encoder.nsaved = nsaved;
			encoder.saved = saved;
			encoder.uulen = uulen;

			return encoder;
		}

#if NET6_0_OR_GREATER
		/// <summary>
		/// Get or set whether the <see cref="UUEncoder"/> should use hardware acceleration when available.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the <see cref="UUEncoder"/> should use hardware acceleration when available.</para>
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
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.UUEncode; }
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
			// Note: up to 44 bytes of input from previous calls may still be buffered (in uubuf and saved), so the
			// worst case needs to account for them being completed into full lines along with the new input.
			return (((inputLength + MaxInputPerLine - 1) / MaxInputPerLine) * MaxOutputPerLine) + MaxOutputPerLine + 2;
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

		static byte Encode (int c)
		{
			return c != 0 ? (byte) (c + 0x20) : (byte) '`';
		}

		unsafe int Encode (byte* input, int length, byte* output, byte* uuptr)
		{
			if (length == 0)
				return 0;

			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte* bufptr;
			byte b0, b1, b2;

			if ((length + nsaved + uulen) < 45) {
				// not enough input to write a full uuencoded line
				bufptr = uuptr + ((uulen / 3) * 4);
			} else {
				bufptr = outptr + 1;

				if (uulen > 0) {
					// copy the previous call's uubuf to output
					int n = (uulen / 3) * 4;

					Buffer.MemoryCopy (uuptr, bufptr, n, n);
					bufptr += n;
				}
			}

			if (nsaved == 2) {
				b0 = (byte) ((saved >> 8) & 0xFF);
				b1 = (byte) (saved & 0xFF);
				b2 = *inptr++;
				nsaved = 0;
				saved = 0;

				// convert 3 input bytes into 4 uuencoded bytes
				*bufptr++ = Encode ((b0 >> 2) & 0x3F);
				*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
				*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
				*bufptr++ = Encode (b2 & 0x3F);

				uulen += 3;
			} else if (nsaved == 1) {
				if ((inptr + 1) < inend) {
					b0 = (byte) (saved & 0xFF);
					b1 = *inptr++;
					b2 = *inptr++;
					nsaved = 0;
					saved = 0;

					// convert 3 input bytes into 4 uuencoded bytes
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr++ = Encode (b2 & 0x3F);

					uulen += 3;
				} else {
					while (inptr < inend) {
						saved = (saved << 8) | *inptr++;
						nsaved++;
					}
				}
			}

			do {
				while (uulen < 45 && (inptr + 2) < inend) {
					b0 = *inptr++;
					b1 = *inptr++;
					b2 = *inptr++;

					// convert 3 input bytes into 4 uuencoded bytes
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr++ = Encode (b2 & 0x3F);

					uulen += 3;
				}

				if (uulen >= 45) {
					// output the uu line length
					*outptr = Encode (uulen);
					outptr += ((uulen / 3) * 4) + 1;
					*outptr++ = (byte) '\n';
					uulen = 0;

					if ((inptr + 45) <= inend) {
						// we have enough input to output another full line
						bufptr = outptr + 1;
					} else {
						bufptr = uuptr;
					}
				} else {
					// not enough input to continue...
					while (inptr < inend) {
						saved = (saved << 8) | *inptr++;
						nsaved++;
					}
				}
			} while (inptr < inend);

			return (int) (outptr - output);
		}

#if NET6_0_OR_GREATER
		unsafe int HwAccelEncode (byte* input, int length, byte* output, byte* uuptr, bool useAvx2)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			if (uulen != 0 || nsaved != 0) {
				// Use the scalar encoder to complete the partial line left over from the previous call.
				int needed = MaxInputPerLine - uulen - nsaved;

				if (length < needed)
					return Encode (input, length, output, uuptr);

				outptr += Encode (inptr, needed, outptr, uuptr);
				inptr += needed;

				Debug.Assert (uulen == 0 && nsaved == 0);
			}

			int lines = (int) (inend - inptr) / MaxInputPerLine;

			if (lines > 0) {
				if (useAvx2)
					outptr = Avx2EncodeLines (inptr, outptr, lines);
				else
					outptr = Vector128EncodeLines (inptr, outptr, lines);

				inptr += lines * MaxInputPerLine;
			}

			if (inptr < inend)
				outptr += Encode (inptr, (int) (inend - inptr), outptr, uuptr);

			return (int) (outptr - output);
		}

		// Encodes complete 45-byte lines (each producing 'M' + 60 encoded characters + '\n').
		//
		// A line is not a multiple of the 12-byte (Vector128) or 24-byte (Vector256) block size,
		// so the final block of each line overlaps the previous one. This means that every load
		// stays within the 45 bytes of input for the line and every store stays within the 62 bytes
		// of output for the line, so no bounds checks against the end of the buffers are needed.
		static unsafe byte* Vector128EncodeLines (byte* inptr, byte* outptr, int lines)
		{
			// The JIT won't hoist these "constants", so help it
			Vector128<byte> shuffleVec = Vector128.Create (0x01020001, 0x04050304, 0x07080607, 0x0A0B090A).AsByte ();

			// The same shuffle as above, but for a 16-byte load done 4 bytes before the 12 bytes of interest.
			Vector128<byte> shuffleTailVec = Vector128.Create (0x05060405, 0x08090708, 0x0B0C0A0B, 0x0E0F0D0E).AsByte ();
			Vector128<byte> maskAC = Vector128.Create (0x0fc0fc00).AsByte ();
			Vector128<byte> maskBB = Vector128.Create (0x003f03f0).AsByte ();
			Vector128<ushort> shiftAC = Vector128.Create (0x04000040).AsUInt16 ();
			Vector128<short> shiftBB = Vector128.Create (0x01000010).AsInt16 ();
			Vector128<byte> const63 = Vector128.Create ((byte) 63);
			Vector128<byte> const33 = Vector128.Create ((byte) 33);
			Vector128<byte> mask8F = Vector128.Create ((byte) 0x8F);

			do {
				*outptr = (byte) 'M';

				// input bytes [0..11] -> output chars [0..15]
				Vector128EncodeBlock (Vector128.LoadUnsafe (ref *inptr), shuffleVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33, mask8F).Store (outptr + 1);

				// input bytes [12..23] -> output chars [16..31]
				Vector128EncodeBlock (Vector128.LoadUnsafe (ref *(inptr + 12)), shuffleVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33, mask8F).Store (outptr + 17);

				// input bytes [24..35] -> output chars [32..47]
				Vector128EncodeBlock (Vector128.LoadUnsafe (ref *(inptr + 24)), shuffleVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33, mask8F).Store (outptr + 33);

				// input bytes [33..44] (loaded from [29..44]) -> output chars [44..59]
				Vector128EncodeBlock (Vector128.LoadUnsafe (ref *(inptr + 29)), shuffleTailVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33, mask8F).Store (outptr + 45);

				outptr[61] = (byte) '\n';

				inptr += MaxInputPerLine;
				outptr += MaxOutputPerLine;
			} while (--lines > 0);

			return outptr;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> Vector128EncodeBlock (Vector128<byte> str, Vector128<byte> shuffleVec, Vector128<byte> maskAC, Vector128<byte> maskBB, Vector128<ushort> shiftAC, Vector128<short> shiftBB, Vector128<byte> const63, Vector128<byte> const33, Vector128<byte> mask8F)
		{
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
			} else if (AdvSimd.Arm64.IsSupported) {
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

			// Translation: 0 => '`' (0x60) and 1..63 => 0x21..0x5F.
			// ((v + 63) & 63) is (v - 1) mod 64, which maps 0 to 63 and 1..63 to 0..62.
			return ((str + const63) & const63) + const33;
		}

		static unsafe byte* Avx2EncodeLines (byte* inptr, byte* outptr, int lines)
		{
			// Each 128-bit lane of the AVX2 shuffle can only access bytes within the same lane, so the
			// lower lane needs its 12 input bytes at offsets [4..15] and the upper lane needs its 12
			// input bytes at offsets [0..11]. The cross-lane permutes below arrange the dwords that way.

			// The JIT won't hoist these "constants", so help it
			Vector256<byte> shuffleVec = Vector256.Create (
				5, 4, 6, 5,
				8, 7, 9, 8,
				11, 10, 12, 11,
				14, 13, 15, 14,
				1, 0, 2, 1,
				4, 3, 5, 4,
				7, 6, 8, 7,
				10, 9, 11, 10).AsByte ();

			// input bytes [0..31] => dwords [x, 0..11] [12..23, x]
			Vector256<int> permuteHead = Vector256.Create (0, 0, 1, 2, 3, 4, 5, 6);

			// input bytes [13..44] => dwords [x, 21..32] [33..44, x]
			Vector256<int> permuteTail = Vector256.Create (0, 2, 3, 4, 5, 6, 7, 0);

			Vector256<byte> maskAC = Vector256.Create (0x0fc0fc00).AsByte ();
			Vector256<byte> maskBB = Vector256.Create (0x003f03f0).AsByte ();
			Vector256<ushort> shiftAC = Vector256.Create (0x04000040).AsUInt16 ();
			Vector256<short> shiftBB = Vector256.Create (0x01000010).AsInt16 ();
			Vector256<byte> const63 = Vector256.Create ((byte) 63);
			Vector256<byte> const33 = Vector256.Create ((byte) 33);

			do {
				*outptr = (byte) 'M';

				// input bytes [0..23] -> output chars [0..31]
				Vector256<byte> str = Avx2.PermuteVar8x32 (Avx.LoadVector256 (inptr).AsInt32 (), permuteHead).AsByte ();
				Avx.Store (outptr + 1, Avx2EncodeBlock (str, shuffleVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33));

				// input bytes [21..44] (loaded from [13..44]) -> output chars [28..59]
				str = Avx2.PermuteVar8x32 (Avx.LoadVector256 (inptr + 13).AsInt32 (), permuteTail).AsByte ();
				Avx.Store (outptr + 29, Avx2EncodeBlock (str, shuffleVec, maskAC, maskBB, shiftAC, shiftBB, const63, const33));

				outptr[61] = (byte) '\n';

				inptr += MaxInputPerLine;
				outptr += MaxOutputPerLine;
			} while (--lines > 0);

			return outptr;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector256<byte> Avx2EncodeBlock (Vector256<byte> str, Vector256<byte> shuffleVec, Vector256<byte> maskAC, Vector256<byte> maskBB, Vector256<ushort> shiftAC, Vector256<short> shiftBB, Vector256<byte> const63, Vector256<byte> const33)
		{
			// See Vector128EncodeBlock for a description of each step.
			str = Avx2.Shuffle (str, shuffleVec);

			Vector256<byte> t0 = Avx2.And (str, maskAC);
			Vector256<byte> t2 = Avx2.And (str, maskBB);
			Vector256<ushort> t1 = Avx2.MultiplyHigh (t0.AsUInt16 (), shiftAC);
			Vector256<short> t3 = Avx2.MultiplyLow (t2.AsInt16 (), shiftBB);

			str = Avx2.Or (t1.AsByte (), t3.AsByte ());

			return Avx2.Add (Avx2.And (Avx2.Add (str, const63), const63), const33);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static Vector128<byte> SimdShuffle (Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask8F)
		{
			Debug.Assert ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian);

			if (Ssse3.IsSupported)
				return Ssse3.Shuffle (left, right);

			return AdvSimd.Arm64.VectorTableLookup (left, right & mask8F);
		}

		internal unsafe int HwAccelEncode (byte* input, int length, byte* output, bool useAvx2)
		{
			fixed (byte* uuptr = uubuf)
				return HwAccelEncode (input, length, output, uuptr, useAvx2);
		}

		unsafe int HwAccelEncode (byte[] input, int startIndex, int length, byte[] output, bool useAvx2, bool flush)
		{
			ValidateArguments (input, startIndex, length, output);

			fixed (byte* inptr = input, outptr = output, uuptr = uubuf) {
				int n = HwAccelEncode (inptr + startIndex, length, outptr, uuptr, useAvx2);

				return flush ? n + Flush (outptr + n, uuptr) : n;
			}
		}

		// The following internal entry points allow the unit tests to exercise each code path directly,
		// regardless of which path the public Encode() and Flush() methods would choose on the host machine.
		internal int Ssse3Encode (byte[] input, int startIndex, int length, byte[] output, bool flush = false)
		{
			if (!Ssse3.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelEncode (input, startIndex, length, output, false, flush);
		}

		internal int Avx2Encode (byte[] input, int startIndex, int length, byte[] output, bool flush = false)
		{
			if (!Avx2.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelEncode (input, startIndex, length, output, true, flush);
		}

		internal int AdvSimdEncode (byte[] input, int startIndex, int length, byte[] output, bool flush = false)
		{
			if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
				throw new PlatformNotSupportedException ();

			return HwAccelEncode (input, startIndex, length, output, false, flush);
		}
#endif

		internal int ScalarEncode (byte[] input, int startIndex, int length, byte[] output, bool flush = false)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output, uuptr = uubuf) {
					int n = Encode (inptr + startIndex, length, outptr, uuptr);

					return flush ? n + Flush (outptr + n, uuptr) : n;
				}
			}
		}

		unsafe int Encode (byte[] input, int startIndex, int length, byte[] output, bool flush)
		{
			ValidateArguments (input, startIndex, length, output);

			fixed (byte* inptr = input, outptr = output, uuptr = uubuf) {
				int n;

#if NET6_0_OR_GREATER
				if ((Ssse3.IsSupported || AdvSimd.Arm64.IsSupported) && BitConverter.IsLittleEndian && EnableHardwareAcceleration)
					n = HwAccelEncode (inptr + startIndex, length, outptr, uuptr, Avx2.IsSupported);
				else
					n = Encode (inptr + startIndex, length, outptr, uuptr);
#else
				n = Encode (inptr + startIndex, length, outptr, uuptr);
#endif

				return flush ? n + Flush (outptr + n, uuptr) : n;
			}
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
			return Encode (input, startIndex, length, output, false);
		}

		// Encodes any remaining buffered input as the final (partial) line, followed by the terminating "`\n" line.
		unsafe int Flush (byte* output, byte* uuptr)
		{
			byte* outptr = output;
			byte* bufptr = uuptr + ((uulen / 3) * 4);
			byte uufill = 0;

			if (nsaved > 0) {
				while (nsaved < 3) {
					saved <<= 8;
					uufill++;
					nsaved++;
				}
				
				if (nsaved == 3) {
					// convert 3 input bytes into 4 uuencoded bytes
					byte b0, b1, b2;
					
					b0 = (byte) ((saved >> 16) & 0xFF);
					b1 = (byte) ((saved >> 8) & 0xFF);
					b2 = (byte) (saved & 0xFF);
					
					*bufptr++ = Encode ((b0 >> 2) & 0x3F);
					*bufptr++ = Encode (((b0 << 4) | ((b1 >> 4) & 0x0F)) & 0x3F);
					*bufptr++ = Encode (((b1 << 2) | ((b2 >> 6) & 0x03)) & 0x3F);
					*bufptr   = Encode (b2 & 0x3F);
					
					uulen += 3;
					nsaved = 0;
					saved = 0;
				}
			}
			
			if (uulen > 0) {
				int n = (uulen / 3) * 4;
				
				*outptr++ = Encode ((uulen - uufill) & 0xFF);
				Buffer.MemoryCopy (uuptr, outptr, n, n);
				outptr += n;

				*outptr++ = (byte) '\n';
				uulen = 0;
			}
			
			*outptr++ = Encode (uulen & 0xFF);
			*outptr++ = (byte) '\n';

			Reset ();

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
			return Encode (input, startIndex, length, output, true);
		}

		/// <summary>
		/// Reset the encoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the encoder.
		/// </remarks>
		public void Reset ()
		{
			nsaved = 0;
			saved = 0;
			uulen = 0;
		}
	}
}
