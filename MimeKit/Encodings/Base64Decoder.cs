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
using System.Runtime.InteropServices;


#if NET6_0_OR_GREATER
using System.Buffers.Text;
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

		/// <summary>
		/// Initialize a new instance of the <see cref="Base64Decoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new base64 decoder.
		/// </remarks>
		public Base64Decoder ()
		{
#if NET9_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || AdvSimd.Arm64.IsSupported;
#elif NET6_0_OR_GREATER
			EnableHardwareAcceleration = Ssse3.IsSupported || (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian);
#endif
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
#if NET6_0_OR_GREATER
				EnableHardwareAcceleration = EnableHardwareAcceleration,
#endif
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
		/// Gets or sets whether the <see cref="Base64Decoder"/> should use hardware acceleration when available.
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
		[SkipLocalsInit]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe int HwAccelDecode (byte* input, int length, byte* output, int outputLength)
		{
			ref byte table = ref MemoryMarshal.GetReference (base64_rank);
			int remainingInput, remainingOutput;
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;

			if (bytes > 0) {
				// Flush out any partially read data from a previous iteration.
				while (inptr < inend && bytes < 4) {
					byte c = *inptr++;
					byte rank = Unsafe.Add (ref table, c);

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

				remainingOutput = outputLength - (int) (outptr - output);
				remainingInput = (int) (inend - inptr);
				saved = 0;
				bytes = 0;
			} else {
				// No previous data, so we can just start decoding.
				remainingOutput = outputLength;
				remainingInput = length;
			}

			var utf8 = new ReadOnlySpan<byte> (inptr, remainingInput);
			var decoded = new Span<byte> (outptr, remainingOutput);

			var status = Base64.DecodeFromUtf8 (utf8, decoded, out int bytesRead, out int bytesWritten, false);
			outptr += bytesWritten;
			inptr += bytesRead;

			if (inptr < inend) {
				// Note: There are 2 scenarios where we might end up here:
				// 1. Base64.DecodeFromUtf8() will only consume/decode complete quartets so if we have < 4 bytes remaining, we will end up here.
				// 2. Base64.DecodeFromUtf8() encountered invalid bytes (OperationStatus.InvalidData), so fall back to decoding manually.
				outptr += Decode (ref table, inptr, inend, outptr);
			}

			return (int) (outptr - output);
		}
#endif

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
						return HwAccelDecode (inptr + startIndex, length, outptr, output.Length);
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
