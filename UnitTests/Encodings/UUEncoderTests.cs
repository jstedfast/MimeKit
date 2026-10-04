//
// UUEncoderTests.cs
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
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class UUEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new UUEncoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new UUEncoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.UUEncode));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new UUEncoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new UUEncoder ());
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestEncode (int bufferSize)
		{
			TestEncoder (new UUEncoder (), "photo.jpg", photo, "photo.uu", bufferSize);
		}

		[Test]
		public void TestFlush ()
		{
			TestEncoderFlush (new UUEncoder (), "photo.jpg", photo, "photo.uu");
		}

		public enum CodePath
		{
			Scalar,
			Ssse3,
			Avx2,
			AdvSimd
		}

		delegate int EncodeFunc (UUEncoder encoder, byte[] input, int startIndex, int length, byte[] output, bool flush);

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
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (encoder, input, startIndex, length, output, flush) => encoder.AdvSimdEncode (input, startIndex, length, output, flush);
			default:
				return (encoder, input, startIndex, length, output, flush) => encoder.ScalarEncode (input, startIndex, length, output, flush);
			}
		}

		// An independent, deliberately simple, reference implementation of uuencoding (without the begin/end lines).
		static byte[] ReferenceEncode (byte[] data)
		{
			static char Encode (int c) => c != 0 ? (char) (c + 0x20) : '`';
			var builder = new StringBuilder ();

			for (int index = 0; index < data.Length; index += 45) {
				int n = Math.Min (45, data.Length - index);

				builder.Append (Encode (n));

				for (int i = 0; i < n; i += 3) {
					int b0 = data[index + i];
					int b1 = i + 1 < n ? data[index + i + 1] : 0;
					int b2 = i + 2 < n ? data[index + i + 2] : 0;

					builder.Append (Encode (b0 >> 2));
					builder.Append (Encode (((b0 << 4) | (b1 >> 4)) & 0x3F));
					builder.Append (Encode (((b1 << 2) | (b2 >> 6)) & 0x3F));
					builder.Append (Encode (b2 & 0x3F));
				}

				builder.Append ('\n');
			}

			builder.Append ("`\n");

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		// Encodes the data using the specified chunk sizes (cycling through them), flushing with the final chunk.
		static byte[] EncodeChunked (EncodeFunc encode, byte[] data, params int[] chunkSizes)
		{
			var encoder = new UUEncoder ();
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

		static readonly int[] ChunkSizes = { 1, 2, 3, 4, 11, 12, 13, 44, 45, 46, 47, 89, 90, 91, 135, 136, 4096, int.MaxValue };

		[Test]
		public void TestEncodeMatchesReference ([Values] CodePath path)
		{
			var encode = GetEncodeFunc (path);
			var random = new Random (45);
			var lengths = new List<int> ();

			for (int i = 0; i <= 300; i++)
				lengths.Add (i);
			lengths.AddRange (new[] { 449, 450, 451, 4499, 4500, 4501, 10007 });

			foreach (var length in lengths) {
				var data = GenerateRandomBytes (random, length);
				var expected = ReferenceEncode (data);

				foreach (var chunkSize in ChunkSizes)
					Assert.That (EncodeChunked (encode, data, chunkSize), Is.EqualTo (expected), $"length={length}, chunkSize={chunkSize}");
			}
		}

		[Test]
		public void TestEncodeRandomChunks ([Values] CodePath path)
		{
			var encode = GetEncodeFunc (path);
			var random = new Random (1999);

			for (int iteration = 0; iteration < 2000; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 2000));
				var chunkSizes = new int[random.Next (1, 10)];

				for (int i = 0; i < chunkSizes.Length; i++)
					chunkSizes[i] = random.Next (4) == 0 ? random.Next (0, 4) : random.Next (0, 200);

				// Avoid an infinite loop if every chunk size is 0.
				chunkSizes[0] = Math.Max (chunkSizes[0], 1);

				Assert.That (EncodeChunked (encode, data, chunkSizes), Is.EqualTo (ReferenceEncode (data)), $"iteration={iteration}, chunks={string.Join (",", chunkSizes)}");
			}
		}

		[Test]
		public void TestEncodePartialSavedBytes ([Values] CodePath path)
		{
			// Regression test: with 1 saved byte, a subsequent call with exactly 2 bytes would save all 3 bytes instead
			// of encoding them, and the next call would then encode its own input *before* the saved bytes.
			var encode = GetEncodeFunc (path);
			var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			Assert.That (EncodeChunked (encode, data, 1, 2, 3), Is.EqualTo (ReferenceEncode (data)), "1,2,3");
			Assert.That (EncodeChunked (encode, data, 1, 2), Is.EqualTo (ReferenceEncode (data)), "1,2");
			Assert.That (EncodeChunked (encode, data, 4, 2, 4), Is.EqualTo (ReferenceEncode (data)), "4,2,4");

			for (int prefix = 0; prefix < 45; prefix++) {
				var line = GenerateRandomBytes (new Random (prefix), 200);

				foreach (var second in new[] { 1, 2, 3, 4, 45, 46, 47 }) {
					Assert.That (EncodeChunked (encode, line, Math.Max (prefix, 1), second, 3), Is.EqualTo (ReferenceEncode (line)), $"prefix={prefix}, second={second}");
				}
			}
		}

		[Test]
		public void TestEncodeEveryByteAtEveryPosition ([Values] CodePath path, [Values (0x00, 0xFF, -1)] int background)
		{
			// Sets every possible byte value at every position within 2 full lines plus a partial line to ensure that every
			// sextet value is translated correctly (including 0 => '`') in every lane of every SIMD block (including the
			// overlapping tail block of each line).
			var encode = GetEncodeFunc (path);
			var random = new Random (background);
			var data = new byte[45 * 2 + 7];

			if (background == -1)
				random.NextBytes (data);
			else
				data.AsSpan ().Fill ((byte) background);

			var original = (byte[]) data.Clone ();

			for (int position = 0; position < data.Length; position++) {
				for (int value = 0; value < 256; value++) {
					data[position] = (byte) value;

					var actual = EncodeChunked (encode, data, int.MaxValue);
					var expected = ReferenceEncode (data);

					if (!actual.AsSpan ().SequenceEqual (expected))
						Assert.Fail ($"position={position}, value=0x{value:X2}: expected \"{Encoding.ASCII.GetString (expected)}\" but got \"{Encoding.ASCII.GetString (actual)}\"");
				}

				data[position] = original[position];
			}
		}

		[Test]
		public void TestEncodeRoundTrip ([Values] CodePath path, [Values (1, 16, 45, 1024, 4096)] int chunkSize)
		{
			var encode = GetEncodeFunc (path);
			var encoded = EncodeChunked (encode, photo, chunkSize);
			var decoder = new UUDecoder (true);
			var output = new byte[decoder.EstimateOutputLength (encoded.Length)];
			int n = decoder.Decode (encoded, 0, encoded.Length, output);

			Assert.That (output.AsSpan (0, n).ToArray (), Is.EqualTo (photo));
		}

		[Test]
		public void TestHardwareAccelerationToggle ()
		{
			var expected = EncodeChunked (GetEncodeFunc (CodePath.Scalar), photo, 4096);
			bool enabled = UUEncoder.EnableHardwareAcceleration;

			try {
				foreach (var value in new[] { true, false }) {
					UUEncoder.EnableHardwareAcceleration = value;

					var encoder = new UUEncoder ();
					var output = new byte[encoder.EstimateOutputLength (photo.Length)];
					int n = encoder.Flush (photo, 0, photo.Length, output);

					Assert.That (output.AsSpan (0, n).ToArray (), Is.EqualTo (expected), $"EnableHardwareAcceleration={value}");
				}
			} finally {
				UUEncoder.EnableHardwareAcceleration = enabled;
			}
		}

		unsafe delegate int PointerEncodeFunc (UUEncoder encoder, byte* input, int length, byte* output);

		static unsafe PointerEncodeFunc GetPointerEncodeFunc (CodePath path)
		{
			switch (path) {
			case CodePath.Ssse3:
				if (!Ssse3.IsSupported)
					Assert.Ignore ("SSSE3 is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, false);
			case CodePath.Avx2:
				if (!Avx2.IsSupported)
					Assert.Ignore ("AVX2 is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, true);
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (encoder, input, length, output) => encoder.HwAccelEncode (input, length, output, false);
			default:
				Assert.Ignore ("The scalar encoder is covered by the other tests.");
				return null;
			}
		}

		static readonly int[] BoundsChunkSizes = { 1, 2, 3, 12, 15, 16, 31, 32, 44, 45, 46, 61, 89, 90, 91, 100, 4096 };

		static byte[] Flush (UUEncoder encoder, List<byte> encoded)
		{
			var output = new byte[encoder.EstimateOutputLength (0)];
			int n = encoder.Flush (Array.Empty<byte> (), 0, 0, output);

			encoded.AddRange (output.AsSpan (0, n).ToArray ());

			return encoded.ToArray ();
		}

		[Test]
		public unsafe void TestEncodeStaysWithinBounds ([Values] CodePath path)
		{
			// Surround the input and output with guard bytes to verify that the SIMD kernels never write past the end of
			// the output buffer, and that they never use bytes beyond the end of the input.
			var encode = GetPointerEncodeFunc (path);
			const int GuardLength = 64;
			const byte Sentinel = 0xA5;
			var random = new Random (2001);

			for (int iteration = 0; iteration < 200; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 1000));
				var expected = ReferenceEncode (data);

				foreach (var chunkSize in BoundsChunkSizes) {
					var encoder = new UUEncoder ();
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
		public unsafe void TestEncodeDoesNotAccessMemoryOutOfBounds ([Values] CodePath path, [Values] bool alignEnd)
		{
			// Places each chunk of input and the output buffer immediately before (or after) an inaccessible guard page so
			// that reading or writing even a single byte out of bounds crashes rather than going unnoticed.
			var encode = GetPointerEncodeFunc (path);
			var random = new Random (alignEnd ? 2024 : 2025);

			for (int iteration = 0; iteration < 50; iteration++) {
				var data = GenerateRandomBytes (random, random.Next (0, 1000));
				var expected = ReferenceEncode (data);

				foreach (var chunkSize in BoundsChunkSizes) {
					var encoder = new UUEncoder ();
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
	}
}
