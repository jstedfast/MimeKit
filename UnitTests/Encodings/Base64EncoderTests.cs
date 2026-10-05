//
// Base64EncoderTests.cs
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

using System.Text;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class Base64EncoderTests : MimeEncoderTestsBase
	{
		static readonly bool DefaultHwAccel = Base64Encoder.EnableHardwareAcceleration;

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new Base64Encoder (0));

			AssertArgumentExceptions (new Base64Encoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new Base64Encoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.Base64));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new Base64Encoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new Base64Encoder ());
		}

		[TestCase (true, 4096)]
		[TestCase (false, 4096)]
		[TestCase (true, 1024)]
		[TestCase (false, 1024)]
		[TestCase (true, 16)]
		[TestCase (false, 16)]
		[TestCase (true, 1)]
		[TestCase (false, 1)]
		public void TestEncode (bool enableHwAccel, int bufferSize)
		{
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				TestEncoder (new Base64Encoder (), "photo.jpg", photo, "photo.b64", bufferSize);
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestFlush (bool enableHwAccel)
		{
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				TestEncoderFlush (new Base64Encoder (), "photo.jpg", photo, "photo.b64");
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestSuperLongLineLengths (bool enableHwAccel)
		{
			ReadOnlySpan<byte> loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. "u8;
			const int MaxLineLength = 998;
			const int PayloadSize = 20649;
			const int ChunkSize = 4096;

			// 20,649 bytes of Lorem ipsum.
			var payload = new byte[PayloadSize];
			int payloadIndex = 0;

			while (payloadIndex < PayloadSize) {
				int chunk = Math.Min (loremIpsum.Length, PayloadSize - payloadIndex);
				loremIpsum.Slice (0, chunk).CopyTo (payload.AsSpan (payloadIndex, chunk));
				payloadIndex += chunk;
			}

			// Pass Base64Encoder 4 KB chunks.
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				var encoder = new Base64Encoder (MaxLineLength, overrideMaxLineLengthLimits: true);
				var buffer = new byte[encoder.EstimateOutputLength (ChunkSize)];
				using var base64Stream = new MemoryStream ();
				int startIndex = 0;

				while (startIndex < payload.Length) {
					int length = Math.Min (ChunkSize, payload.Length - startIndex);
					int n = encoder.Encode (payload, startIndex, length, buffer);

					base64Stream.Write (buffer, 0, n);
					startIndex += length;
				}

				int flushed = encoder.Flush (payload, startIndex, 0, buffer);
				base64Stream.Write (buffer, 0, flushed);

				// Decode and compare.
				var utf8 = base64Stream.GetBuffer ().AsSpan (0, (int) base64Stream.Length);
				buffer = new byte[Base64.GetMaxDecodedFromUtf8Length (utf8.Length)];
				var result = Base64.DecodeFromUtf8 (utf8, buffer, out int bytesConsumed, out int bytesWritten, true);
				var decoded = buffer.AsSpan (0, bytesWritten);

				Assert.That (result, Is.EqualTo (OperationStatus.Done), "result");
				Assert.That (bytesConsumed, Is.EqualTo (utf8.Length), "bytesConsumed");
				Assert.That (bytesWritten, Is.EqualTo (payload.Length), "bytesWritten");
				Assert.That (decoded.SequenceEqual (payload), Is.True, "decoded");
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		public enum CodePath
		{
			Scalar,
			Ssse3,
			Avx2,
			Avx512,
			AdvSimd
		}

		delegate int EncodeFunc (Base64Encoder encoder, byte[] input, int startIndex, int length, byte[] output, bool flush);

		static EncodeFunc GetEncodeFunc (CodePath path)
		{
			switch (path) {
			case CodePath.Ssse3:
				if (!Ssse3.IsSupported)
					Assert.Ignore ("SSSE3 is not supported on this host.");

				return (encoder, input, startIndex, length, output, flush) => encoder.Ssse3Encode (input, startIndex, length, output, flush);
			case CodePath.Avx2:
				if (!Avx2.IsSupported)
					Assert.Ignore ("AVX2 is not supported on this host.");

				return (encoder, input, startIndex, length, output, flush) => encoder.Avx2Encode (input, startIndex, length, output, flush);
			case CodePath.Avx512:
				if (!Vector512.IsHardwareAccelerated || !Avx512Vbmi.IsSupported || !Avx2.IsSupported)
					Assert.Ignore ("AVX-512 VBMI is not supported on this host.");

				return (encoder, input, startIndex, length, output, flush) => encoder.Avx512Encode (input, startIndex, length, output, flush);
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (encoder, input, startIndex, length, output, flush) => encoder.AdvSimdEncode (input, startIndex, length, output, flush);
			default:
				return (encoder, input, startIndex, length, output, flush) => encoder.ScalarEncode (input, startIndex, length, output, flush);
			}
		}

		// Note: The internal constructor allows line lengths outside of the 60-76 range that the public constructor allows.
		// Since each line holds (maxLineLength / 4) quartets, odd line lengths (e.g. 75) get rounded down.
		static readonly int[] LineLengths = { 4, 12, 16, 48, 64, 72, 75, 76, 100, 256, 998 };

		static Base64Encoder CreateEncoder (int maxLineLength)
		{
			return new Base64Encoder (maxLineLength, overrideMaxLineLengthLimits: true);
		}

		// An independent, deliberately simple, reference implementation of base64 encoding with line wrapping.
		static byte[] ReferenceEncode (byte[] data, int maxLineLength)
		{
			var base64 = Convert.ToBase64String (data);
			int lineLength = (maxLineLength / 4) * 4;
			var builder = new StringBuilder ();

			for (int index = 0; index < base64.Length; index += lineLength) {
				builder.Append (base64, index, Math.Min (lineLength, base64.Length - index));
				builder.Append ('\n');
			}

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		// Encodes the data using the specified chunk sizes (cycling through them), flushing with the final chunk.
		static byte[] EncodeChunked (EncodeFunc encode, int maxLineLength, byte[] data, params int[] chunkSizes)
		{
			var encoder = CreateEncoder (maxLineLength);
			var result = new List<byte> ();
			int index = 0, chunk = 0;

			do {
				int count = Math.Min (chunkSizes[chunk++ % chunkSizes.Length], data.Length - index);
				bool flush = index + count == data.Length;
				var output = new byte[encoder.EstimateOutputLength (count)];

				int n = encode (encoder, data, index, count, output, flush);
				result.AddRange (output.AsSpan (0, n).ToArray ());
				index += count;

				if (flush)
					break;
			} while (true);

			return result.ToArray ();
		}

		static byte[] GenerateRandomBytes (Random random, int length)
		{
			var data = new byte[length];
			random.NextBytes (data);
			return data;
		}

		static readonly int[] ChunkSizes = { 1, 2, 3, 4, 11, 12, 13, 15, 16, 17, 47, 48, 55, 56, 57, 58, 63, 64, 65, 114, 4096, int.MaxValue };

		[Test]
		public void TestEncodeMatchesReference ([Values] CodePath path, [ValueSource (nameof (LineLengths))] int maxLineLength)
		{
			var encode = GetEncodeFunc (path);
			var random = new Random (57);
			var lengths = new List<int> ();

			for (int i = 0; i <= 200; i++)
				lengths.Add (i);
			lengths.AddRange (new[] { 569, 570, 571, 4559, 4560, 4561, 10007 });

			foreach (var length in lengths) {
				var data = GenerateRandomBytes (random, length);
				var expected = ReferenceEncode (data, maxLineLength);

				foreach (var chunkSize in ChunkSizes)
					Assert.That (EncodeChunked (encode, maxLineLength, data, chunkSize), Is.EqualTo (expected), $"length={length}, chunkSize={chunkSize}");
			}
		}

		[Test]
		public void TestEncodeRandomChunks ([Values] CodePath path, [ValueSource (nameof (LineLengths))] int maxLineLength)
		{
			var encode = GetEncodeFunc (path);
			var random = new Random (1999 + maxLineLength);

			for (int iteration = 0; iteration < 500; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 2000));
				var chunkSizes = new int[random.Next (1, 10)];

				for (int i = 0; i < chunkSizes.Length; i++)
					chunkSizes[i] = random.Next (4) == 0 ? random.Next (0, 4) : random.Next (0, 200);

				// Avoid an infinite loop if every chunk size is 0.
				chunkSizes[0] = Math.Max (chunkSizes[0], 1);

				Assert.That (EncodeChunked (encode, maxLineLength, data, chunkSizes), Is.EqualTo (ReferenceEncode (data, maxLineLength)), $"iteration={iteration}, chunks={string.Join (",", chunkSizes)}");
			}
		}

		[Test]
		public void TestEncodePartialSavedBytes ([Values] CodePath path, [Values (76, 998)] int maxLineLength)
		{
			// Leave 0, 1 or 2 saved bytes (and every possible partial line) from a previous call and verify that they
			// get combined correctly with the input of subsequent calls of various lengths.
			var encode = GetEncodeFunc (path);
			int maxInputPerLine = (maxLineLength / 4) * 3;

			for (int prefix = 1; prefix <= maxInputPerLine + 2; prefix++) {
				var data = GenerateRandomBytes (new Random (prefix), 400);
				var expected = ReferenceEncode (data, maxLineLength);

				foreach (var second in new[] { 1, 2, 3, 4, 5, 47, 48, 49, 56, 57, 58, 64, 65, 66 }) {
					Assert.That (EncodeChunked (encode, maxLineLength, data, prefix, second, 3), Is.EqualTo (expected), $"prefix={prefix}, second={second}");
				}
			}
		}

		[Test]
		public void TestEncodeEveryByteAtEveryPosition ([Values] CodePath path, [Values (76, 998)] int maxLineLength, [Values (0x00, 0xFF, -1)] int background)
		{
			// Sets every possible byte value at every position to ensure that every sextet value is translated correctly in
			// every lane of every SIMD block.
			var encode = GetEncodeFunc (path);
			var random = new Random (background);
			var data = new byte[57 * 3 + 7];

			if (background == -1)
				random.NextBytes (data);
			else
				data.AsSpan ().Fill ((byte) background);

			var original = (byte[]) data.Clone ();

			for (int position = 0; position < data.Length; position++) {
				for (int value = 0; value < 256; value++) {
					data[position] = (byte) value;

					var actual = EncodeChunked (encode, maxLineLength, data, int.MaxValue);
					var expected = ReferenceEncode (data, maxLineLength);

					if (!actual.AsSpan ().SequenceEqual (expected))
						Assert.Fail ($"position={position}, value=0x{value:X2}: expected \"{Encoding.ASCII.GetString (expected)}\" but got \"{Encoding.ASCII.GetString (actual)}\"");
				}

				data[position] = original[position];
			}
		}

		[Test]
		public void TestEncodeRoundTrip ([Values] CodePath path, [Values (1, 16, 57, 1024, 4096)] int chunkSize)
		{
			var encode = GetEncodeFunc (path);
			var encoded = EncodeChunked (encode, 76, photo, chunkSize);
			var decoder = new Base64Decoder ();
			var output = new byte[decoder.EstimateOutputLength (encoded.Length)];
			int n = decoder.Decode (encoded, 0, encoded.Length, output);

			Assert.That (output.AsSpan (0, n).ToArray (), Is.EqualTo (photo));
		}

		[Test]
		public void TestHardwareAccelerationToggle ()
		{
			var expected = EncodeChunked (GetEncodeFunc (CodePath.Scalar), 76, photo, 4096);

			try {
				foreach (var value in new[] { true, false }) {
					Base64Encoder.EnableHardwareAcceleration = value;

					var encoder = new Base64Encoder ();
					var output = new byte[encoder.EstimateOutputLength (photo.Length)];
					int n = encoder.Flush (photo, 0, photo.Length, output);

					Assert.That (output.AsSpan (0, n).ToArray (), Is.EqualTo (expected), $"EnableHardwareAcceleration={value}");
				}
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		unsafe delegate int PointerEncodeFunc (Base64Encoder encoder, byte* input, int length, byte* output);

		static unsafe PointerEncodeFunc GetPointerEncodeFunc (CodePath path)
		{
			switch (path) {
			case CodePath.Ssse3:
				if (!Ssse3.IsSupported)
					Assert.Ignore ("SSSE3 is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, false, false);
			case CodePath.Avx2:
				if (!Avx2.IsSupported)
					Assert.Ignore ("AVX2 is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, false, true);
			case CodePath.Avx512:
				if (!Vector512.IsHardwareAccelerated || !Avx512Vbmi.IsSupported || !Avx2.IsSupported)
					Assert.Ignore ("AVX-512 VBMI is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, true, true);
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, false, false);
			default:
				Assert.Ignore ("The scalar encoder is covered by the other tests.");
				return null;
			}
		}

		static readonly int[] BoundsChunkSizes = { 1, 2, 3, 12, 15, 16, 17, 31, 32, 47, 48, 55, 56, 57, 63, 64, 65, 100, 4096 };

		static byte[] Flush (Base64Encoder encoder, List<byte> encoded)
		{
			var output = new byte[encoder.EstimateOutputLength (0)];
			int n = encoder.Flush (Array.Empty<byte> (), 0, 0, output);

			encoded.AddRange (output.AsSpan (0, n).ToArray ());

			return encoded.ToArray ();
		}

		[Test]
		public unsafe void TestEncodeStaysWithinBounds ([Values] CodePath path, [Values (76, 998)] int maxLineLength)
		{
			// Surround the input and output with guard bytes to verify that the SIMD kernels never write past the end of
			// the output buffer, and that they never use bytes beyond the end of the input.
			var encode = GetPointerEncodeFunc (path);
			const int GuardLength = 64;
			const byte Sentinel = 0xA5;
			var random = new Random (2001);

			for (int iteration = 0; iteration < 100; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 1500));
				var expected = ReferenceEncode (data, maxLineLength);

				foreach (var chunkSize in BoundsChunkSizes) {
					var encoder = CreateEncoder (maxLineLength);
					var actual = new List<byte> ();

					for (int index = 0; index < data.Length; index += chunkSize) {
						int n = Math.Min (chunkSize, data.Length - index);
						var input = new byte[n + GuardLength];
						int outputLength = encoder.EstimateOutputLength (n);
						var output = new byte[outputLength + GuardLength];

						Buffer.BlockCopy (data, index, input, 0, n);
						random.NextBytes (input.AsSpan (n));
						output.AsSpan ().Fill (Sentinel);

						fixed (byte* inptr = input, outptr = output)
							n = encode (encoder, inptr, n, outptr);

						Assert.That (n, Is.LessThanOrEqualTo (outputLength), $"iteration={iteration}, chunkSize={chunkSize}: output length");

						for (int i = n; i < output.Length; i++) {
							if (output[i] != Sentinel)
								Assert.Fail ($"iteration={iteration}, chunkSize={chunkSize}: wrote past the end of the encoded output at offset {i - n}");
						}

						actual.AddRange (output.AsSpan (0, n).ToArray ());
					}

					Assert.That (Flush (encoder, actual), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}
			}
		}

		[Test]
		public unsafe void TestEncodeDoesNotAccessMemoryOutOfBounds ([Values] CodePath path, [Values (76, 998)] int maxLineLength, [Values] bool alignEnd)
		{
			// Places each chunk of input and the output buffer immediately before (or after) an inaccessible guard page so
			// that reading or writing even a single byte out of bounds crashes rather than going unnoticed.
			var encode = GetPointerEncodeFunc (path);
			var random = new Random (alignEnd ? 2024 : 2025);

			for (int iteration = 0; iteration < 30; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 1500));
				var expected = ReferenceEncode (data, maxLineLength);

				foreach (var chunkSize in BoundsChunkSizes) {
					var encoder = CreateEncoder (maxLineLength);
					var actual = new List<byte> ();

					for (int index = 0; index < data.Length; index += chunkSize) {
						int n = Math.Min (chunkSize, data.Length - index);
						int outputLength = encoder.EstimateOutputLength (n);

						using var input = new GuardedMemory (n, alignEnd);
						using var output = new GuardedMemory (outputLength, alignEnd);

						data.AsSpan (index, n).CopyTo (new Span<byte> (input.Start, n));

						n = encode (encoder, input.Start, n, output.Start);

						Assert.That (n, Is.LessThanOrEqualTo (outputLength), $"iteration={iteration}, chunkSize={chunkSize}: output length");

						actual.AddRange (new ReadOnlySpan<byte> (output.Start, n).ToArray ());
					}

					Assert.That (Flush (encoder, actual), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}
			}
		}

		[Test]
		public void TestEncodeEmptyInput ([Values] bool enableHwAccel)
		{
			// Regression test: fixing an empty array results in a null pointer, which the hardware accelerated code path
			// used to compute (null - 2) from, causing it to dereference a null pointer.
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				var encoder = new Base64Encoder ();
				var output = new byte[encoder.EstimateOutputLength (0)];

				Assert.That (encoder.Encode (Array.Empty<byte> (), 0, 0, output), Is.EqualTo (0), "Encode");
				Assert.That (encoder.Flush (Array.Empty<byte> (), 0, 0, output), Is.EqualTo (0), "Flush");
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[Test]
		public void TestEstimateOutputLength ([ValueSource (nameof (LineLengths))] int maxLineLength)
		{
			// Verify that the estimate is large enough for every combination of saved bytes and partial line state
			// left over from a previous call, including when flushing.
			int maxInputPerLine = (maxLineLength / 4) * 3;
			var data = new byte[maxInputPerLine * 3 + 3];

			for (int prefix = 0; prefix <= maxInputPerLine + 2; prefix++) {
				for (int length = 0; length < data.Length; length++) {
					foreach (var flush in new[] { false, true }) {
						var encoder = CreateEncoder (maxLineLength);
						var output = new byte[encoder.EstimateOutputLength (prefix)];

						encoder.ScalarEncode (data, 0, prefix, output);

						int estimate = encoder.EstimateOutputLength (length);
						output = new byte[estimate + 16];

						int n = encoder.ScalarEncode (data, 0, length, output, flush);

						Assert.That (n, Is.LessThanOrEqualTo (estimate), $"prefix={prefix}, length={length}, flush={flush}");
					}
				}
			}
		}
	}
}
