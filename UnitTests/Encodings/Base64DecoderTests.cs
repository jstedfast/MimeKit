//
// Base64DecoderTests.cs
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
	public class Base64DecoderTests : MimeDecoderTestsBase
	{
		static readonly bool DefaultHwAccel = Base64Decoder.EnableHardwareAcceleration;

		static readonly string[] base64EncodedPatterns = {
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCBvbmNlLi4=",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCB0d2ljZQ==",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggd2lsbCBub3QgYmUgcGFkZGVk",
			" &% VGhp\r\ncyBp\r\ncyB0aGUgcGxhaW4g  \tdGV4dCBtZ?!XNzY*WdlIQ==",
		};
		static readonly string[] base64DecodedPatterns = {
			"This is the plain text message!",
			"This is a text which has to be padded once..",
			"This is a text which has to be padded twice",
			"This is a text which will not be padded",
			"This is the plain text message!"
		};
		static readonly string[] base64EncodedLongPatterns = {
			"AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCU" +
			"mJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0" +
			"xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3Bxc" +
			"nN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeY" +
			"mZqbnJ2en6ChoqOkpaanqKmqq6ytrq+wsbKztLW2t7i5uru8vb6" +
			"/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5O" +
			"Xm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/w==",

			"AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSY" +
			"nKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QEFCQ0RFRkdISUpLTE" +
			"1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc" +
			"3R1dnd4eXp7fH1+f4CBgoOEhYaHiImKi4yNjo+QkZKTlJWWl5iZ" +
			"mpucnZ6foKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr/" +
			"AwcLDxMXGx8jJysvMzc7P0NHS09TV1tfY2drb3N3e3+Dh4uPk5e" +
			"bn6Onq6+zt7u/w8fLz9PX29/j5+vv8/f7/AA==",

			"AgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fICEiIyQlJic" +
			"oKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj9AQUJDREVGR0hJSktMTU" +
			"5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzd" +
			"HV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJma" +
			"m5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8D" +
			"BwsPExcbHyMnKy8zNzs/Q0dLT1NXW19jZ2tvc3d7f4OHi4+Tl5u" +
			"fo6err7O3u7/Dx8vP09fb3+Pn6+/z9/v8AAQ=="
		};
		static readonly string[] base64EncodedPatternsExtraPadding = {
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ===",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ====",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=====",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ======",
		};

		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new Base64Decoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new Base64Decoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Base64));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new Base64Decoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new Base64Decoder ());
		}

		public enum CodePath
		{
			Scalar,
			Ssse3,
			Avx2,
			AdvSimd
		}

		delegate int DecodeFunc (Base64Decoder decoder, byte[] input, int startIndex, int length, byte[] output);

		static DecodeFunc GetDecodeFunc (CodePath path)
		{
			switch (path) {
			case CodePath.Ssse3:
				if (!Ssse3.IsSupported)
					Assert.Ignore ("SSSE3 is not supported on this host.");

				return (decoder, input, startIndex, length, output) => decoder.Ssse3Decode (input, startIndex, length, output);
			case CodePath.Avx2:
				if (!Avx2.IsSupported)
					Assert.Ignore ("AVX2 is not supported on this host.");

				return (decoder, input, startIndex, length, output) => decoder.Avx2Decode (input, startIndex, length, output);
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (decoder, input, startIndex, length, output) => decoder.AdvSimdDecode (input, startIndex, length, output);
			default:
				return (decoder, input, startIndex, length, output) => decoder.ScalarDecode (input, startIndex, length, output);
			}
		}

		[Test]
		public void TestDecodePatterns ([Values] CodePath path)
		{
			var decode = GetDecodeFunc (path);
			var decoder = new Base64Decoder ();
			var output = new byte[4096];

			for (int i = 0; i < base64EncodedPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedPatterns[i]);
				int n = decode (decoder, buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (base64DecodedPatterns[i]), $"Failed to decode base64EncodedPatterns[{i}]");
			}

			for (int i = 0; i < base64EncodedLongPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedLongPatterns[i]);
				int n = decode (decoder, buf, 0, buf.Length, output);

				for (int j = 0; j < n; j++)
					Assert.That ((byte) (j + i), Is.EqualTo (output[j]), $"Failed to decode base64EncodedLongPatterns[{i}]");
			}

			for (int i = 0; i < base64EncodedPatternsExtraPadding.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedPatternsExtraPadding[i]);
				int n = decode (decoder, buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (base64DecodedPatterns[0]), $"Failed to decode base64EncodedPatternsExtraPadding[{i}]");
			}
		}

		[Test]
		public void TestDecodeTwoBlocks ([Values] CodePath path)
		{
			const string input = "VGhpcyBpcyB0aGUgcGF5bG9hZCBvZiB0aGUgZmlyc3QgYmFzZTY0LWVuY29kZWQgYmxvY2sgb2Yg\r\ndGV4dC4=\r\nQW5kIHRoaXMgaXMgdGhlIHBheWxvYWQgb2YgdGhlIHNlY29uZCBiYXNlNjQtZW5jb2RlZCBibG9j\r\nayBvZiB0ZXh0Lg==\r\n";
			const string expected = "This is the payload of the first base64-encoded block of text.And this is the payload of the second base64-encoded block of text.";

			var decode = GetDecodeFunc (path);
			var data = Encoding.ASCII.GetBytes (input);
			var decoder = new Base64Decoder ();
			var output = new byte[decoder.EstimateOutputLength (data.Length)];

			int n = decode (decoder, data, 0, data.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected), "Failed to decode two blocks of base64-encoded text.");
		}

		[Test]
		public void TestDecode ([Values] bool hwAccel, [Values (4096, 1024, 16, 1)] int bufferSize)
		{
			Base64Decoder.EnableHardwareAcceleration = hwAccel;

			try {
				TestDecoder (new Base64Decoder (), photo, "photo.b64", bufferSize);
			} finally {
				Base64Decoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[Test]
		public void TestDecodePhoto ([Values] CodePath path, [Values (4096, 1024, 16, 1)] int bufferSize)
		{
			var decode = GetDecodeFunc (path);
			var encoded = File.ReadAllBytes (Path.Combine (dataDir, "photo.b64"));

			Assert.That (DecodeChunked (decode, encoded, bufferSize), Is.EqualTo (photo));
		}

		static byte[] DecodeChunked (DecodeFunc decode, byte[] input, int chunkSize)
		{
			var decoder = new Base64Decoder ();
			var output = new byte[decoder.EstimateOutputLength (input.Length) * 2 + 3];
			int outputLength = 0;

			chunkSize = Math.Max (1, Math.Min (chunkSize, input.Length));

			for (int index = 0; index < input.Length; index += chunkSize) {
				int n = Math.Min (chunkSize, input.Length - index);
				var chunk = new byte[decoder.EstimateOutputLength (n)];

				n = decode (decoder, input, index, n, chunk);
				Buffer.BlockCopy (chunk, 0, output, outputLength, n);
				outputLength += n;
			}

			Array.Resize (ref output, outputLength);

			return output;
		}

		static byte[] DecodeInPlace (DecodeFunc decode, byte[] input)
		{
			var decoder = new Base64Decoder ();
			var buffer = new byte[Math.Max (input.Length, decoder.EstimateOutputLength (input.Length))];

			Buffer.BlockCopy (input, 0, buffer, 0, input.Length);

			int n = decode (decoder, buffer, 0, input.Length, buffer);

			Array.Resize (ref buffer, n);

			return buffer;
		}

		static byte[] Wrap (string base64, int lineLength, string newLine)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < base64.Length; i += lineLength) {
				builder.Append (base64, i, Math.Min (lineLength, base64.Length - i));
				builder.Append (newLine);
			}

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		static readonly object[] LineFormats = {
			new object[] { 1, "\r\n" },
			new object[] { 3, "\n" },
			new object[] { 4, "\r\n" },
			new object[] { 5, "\r\n" },
			new object[] { 12, "\n" },
			new object[] { 16, "\r\n" },
			new object[] { 17, "\r\n" },
			new object[] { 32, "\n" },
			new object[] { 57, "\r\n" },
			new object[] { 64, "\r\n" },
			new object[] { 72, " \t\r\n" },
			new object[] { 75, "\r\n" },
			new object[] { 75, "\n" },
			new object[] { 76, "\r\n" },
			new object[] { 77, "\r\n" },
			new object[] { 76, "\n" },
			new object[] { 1000, "\r\n" },
			new object[] { int.MaxValue, "" },
		};

		[Test]
		public void TestDecodeRandomData ([Values] CodePath path, [ValueSource (nameof (LineFormats))] object[] format)
		{
			int lineLength = (int) format[0];
			var newLine = (string) format[1];
			var random = new Random (lineLength * 31 + newLine.Length);
			int[] chunkSizes = { 1, 3, 15, 16, 17, 31, 33, 64, 77, 4096, int.MaxValue };
			var decode = GetDecodeFunc (path);

			for (int length = 0; length < 1200; length += random.Next (1, 50)) {
				var data = new byte[length];

				random.NextBytes (data);

				var encoded = Wrap (Convert.ToBase64String (data), lineLength, newLine);

				foreach (var chunkSize in chunkSizes)
					Assert.That (DecodeChunked (decode, encoded, chunkSize), Is.EqualTo (data), $"length={length}, chunkSize={chunkSize}");

				Assert.That (DecodeInPlace (decode, encoded), Is.EqualTo (data), $"In-place: length={length}");
			}
		}

		[Test]
		public void TestDecodeRandomDataWithGarbage ([Values] CodePath path)
		{
			const string garbage = " \t\r\n=*!-.~\0\u0080\u00ff@[`{:";
			int[] chunkSizes = { 1, 7, 16, 33, 100, int.MaxValue };
			var scalar = GetDecodeFunc (CodePath.Scalar);
			var decode = GetDecodeFunc (path);
			var random = new Random (1214);

			for (int iteration = 0; iteration < 500; iteration++) {
				var data = new byte[random.Next (0, 600)];

				random.NextBytes (data);

				var builder = new StringBuilder ();
				int insertions = random.Next (0, 20);

				// Split into a few independently-padded segments (concatenated base64 blocks).
				int segments = random.Next (1, 4);
				int offset = 0;

				for (int s = 0; s < segments; s++) {
					int segmentLength = s == segments - 1 ? data.Length - offset : random.Next (0, data.Length - offset + 1);

					builder.Append (Convert.ToBase64String (data, offset, segmentLength));
					offset += segmentLength;
				}

				var base64 = builder.ToString ();
				builder.Clear ();

				var positions = new SortedSet<int> ();
				for (int i = 0; i < insertions; i++)
					positions.Add (random.Next (0, base64.Length + 1));

				int last = 0;
				foreach (var position in positions) {
					builder.Append (base64, last, position - last);
					int count = random.Next (1, 4);
					for (int i = 0; i < count; i++)
						builder.Append (garbage[random.Next (garbage.Length)]);
					last = position;
				}
				builder.Append (base64, last, base64.Length - last);

				var encoded = new byte[builder.Length];
				for (int i = 0; i < builder.Length; i++)
					encoded[i] = (byte) builder[i];

				// The scalar decoder is the reference implementation.
				var expected = DecodeChunked (scalar, encoded, int.MaxValue);

				if (positions.Count == 0)
					Assert.That (expected, Is.EqualTo (data), $"Reference: iteration={iteration}");

				foreach (var chunkSize in chunkSizes)
					Assert.That (DecodeChunked (decode, encoded, chunkSize), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");

				Assert.That (DecodeInPlace (decode, encoded), Is.EqualTo (expected), $"In-place: iteration={iteration}");
			}
		}

		const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		static void AssertMatchesScalar (DecodeFunc decode, byte[] encoded, int[] chunkSizes, string message)
		{
			// The scalar decoder is the reference implementation.
			var expected = DecodeChunked (GetDecodeFunc (CodePath.Scalar), encoded, int.MaxValue);

			foreach (var chunkSize in chunkSizes) {
				var actual = DecodeChunked (decode, encoded, chunkSize);

				if (!actual.AsSpan ().SequenceEqual (expected))
					Assert.That (actual, Is.EqualTo (expected), $"{message}, chunkSize={chunkSize}");
			}

			var inplace = DecodeInPlace (decode, encoded);

			if (!inplace.AsSpan ().SequenceEqual (expected))
				Assert.That (inplace, Is.EqualTo (expected), $"{message}, in-place");
		}

		[Test]
		public void TestDecodeEveryByteAtEveryPosition ([Values] CodePath path, [Values] bool replace)
		{
			// The SIMD kernels classify bytes using nibble lookup tables. Verify that every possible byte value, at every
			// position within (and across) the 16 and 32-byte blocks, is handled exactly like the scalar decoder.
			int[] chunkSizes = { 33, int.MaxValue };
			var decode = GetDecodeFunc (path);
			var data = new byte[72];

			new Random (72).NextBytes (data);

			var base64 = Encoding.ASCII.GetBytes (Convert.ToBase64String (data));

			for (int value = 0; value < 256; value++) {
				for (int position = 0; position < base64.Length; position++) {
					byte[] encoded;

					if (replace) {
						encoded = (byte[]) base64.Clone ();
					} else {
						encoded = new byte[base64.Length + 1];
						Buffer.BlockCopy (base64, 0, encoded, 0, position);
						Buffer.BlockCopy (base64, position, encoded, position + 1, base64.Length - position);
					}

					encoded[position] = (byte) value;

					AssertMatchesScalar (decode, encoded, chunkSizes, $"value=0x{value:X2}, position={position}");
				}
			}
		}

		[Test]
		public void TestDecodeAllLineLengths ([Values] CodePath path, [Values ("\n", "\r\n", " \r\n")] string newLine)
		{
			int[] dataLengths = { 1, 2, 3, 11, 12, 13, 23, 24, 25, 47, 48, 49, 200, 301 };
			int[] chunkSizes = { 1, 15, 16, 17, 32, 33, 77, int.MaxValue };
			var decode = GetDecodeFunc (path);
			var random = new Random (newLine.Length);

			for (int lineLength = 1; lineLength <= 100; lineLength++) {
				foreach (var dataLength in dataLengths) {
					var data = new byte[dataLength];

					random.NextBytes (data);

					var encoded = Wrap (Convert.ToBase64String (data), lineLength, newLine);

					foreach (var chunkSize in chunkSizes) {
						var actual = DecodeChunked (decode, encoded, chunkSize);

						if (!actual.AsSpan ().SequenceEqual (data))
							Assert.That (actual, Is.EqualTo (data), $"lineLength={lineLength}, dataLength={dataLength}, chunkSize={chunkSize}");
					}

					Assert.That (DecodeInPlace (decode, encoded), Is.EqualTo (data), $"In-place: lineLength={lineLength}, dataLength={dataLength}");
				}
			}
		}

		static byte[] GenerateRandomBytes (Random random, int length, double density)
		{
			var encoded = new byte[length];

			for (int i = 0; i < length; i++) {
				if (random.NextDouble () < density)
					encoded[i] = (byte) Base64Alphabet[random.Next (Base64Alphabet.Length)];
				else
					encoded[i] = (byte) random.Next (256);
			}

			return encoded;
		}

		[Test]
		public void TestDecodeRandomBytes ([Values] CodePath path, [Values (0.0, 0.5, 0.9, 0.99, 0.999)] double density)
		{
			// Base64 alphabet characters interspersed with arbitrary bytes (including '=' padding, line breaks and 8-bit
			// bytes) at the specified density.
			int[] chunkSizes = { 1, 7, 16, 31, 33, 100, int.MaxValue };
			var random = new Random ((int) (density * 1000));
			var decode = GetDecodeFunc (path);

			for (int iteration = 0; iteration < 300; iteration++) {
				var encoded = GenerateRandomBytes (random, random.Next (0, 800), density);

				AssertMatchesScalar (decode, encoded, chunkSizes, $"iteration={iteration}");
			}
		}

		static byte[] GenerateEncoded (Random random)
		{
			var data = new byte[random.Next (0, 1000)];

			random.NextBytes (data);

			var newLine = random.Next (3) switch { 0 => "\n", 1 => "\r\n", _ => string.Empty };
			var encoded = Wrap (Convert.ToBase64String (data), random.Next (1, 100), newLine);

			if (encoded.Length > 0 && random.Next (2) == 0) {
				int corruptions = random.Next (1, 10);

				for (int i = 0; i < corruptions; i++)
					encoded[random.Next (encoded.Length)] = (byte) random.Next (256);
			}

			return encoded;
		}

		[Test]
		public void TestDecodeInPlaceAtEveryOffset ([Values] CodePath path)
		{
			// When decoding in-place with the input starting at an offset within the buffer, the output trails the input by a
			// varying distance. Verify that the SIMD stores never clobber input that has not been decoded yet.
			var scalar = GetDecodeFunc (CodePath.Scalar);
			var decode = GetDecodeFunc (path);
			var random = new Random (4242);

			for (int offset = 0; offset <= 40; offset++) {
				for (int iteration = 0; iteration < 50; iteration++) {
					var encoded = GenerateEncoded (random);
					var expected = DecodeChunked (scalar, encoded, int.MaxValue);
					var decoder = new Base64Decoder ();
					var buffer = new byte[Math.Max (offset + encoded.Length, decoder.EstimateOutputLength (encoded.Length))];

					random.NextBytes (buffer);
					Buffer.BlockCopy (encoded, 0, buffer, offset, encoded.Length);

					int n = decode (decoder, buffer, offset, encoded.Length, buffer);

					if (!buffer.AsSpan (0, n).SequenceEqual (expected))
						Assert.That (buffer.AsSpan (0, n).ToArray (), Is.EqualTo (expected), $"offset={offset}, iteration={iteration}");
				}
			}
		}

		unsafe delegate int PointerDecodeFunc (Base64Decoder decoder, byte* input, int length, byte* output, int outputLength);

		static unsafe PointerDecodeFunc GetPointerDecodeFunc (CodePath path)
		{
			switch (path) {
			case CodePath.Ssse3:
				if (!Ssse3.IsSupported)
					Assert.Ignore ("SSSE3 is not supported on this host.");

				return (decoder, input, length, output, outputLength) => decoder.HwAccelDecode (input, length, output, outputLength, false);
			case CodePath.Avx2:
				if (!Avx2.IsSupported)
					Assert.Ignore ("AVX2 is not supported on this host.");

				return (decoder, input, length, output, outputLength) => decoder.HwAccelDecode (input, length, output, outputLength, true);
			case CodePath.AdvSimd:
				if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian || Ssse3.IsSupported)
					Assert.Ignore ("AdvSimd (Arm64) is not supported on this host.");

				return (decoder, input, length, output, outputLength) => decoder.HwAccelDecode (input, length, output, outputLength, false);
			default:
				return (decoder, input, length, output, outputLength) => decoder.Decode (input, length, output);
			}
		}

		[Test]
		public unsafe void TestDecodeStaysWithinBounds ([Values] CodePath path)
		{
			// Surround the input and output with guard bytes to verify that the SIMD kernels never write past the end of
			// the output buffer, and that they never use bytes beyond the end of the input. The input guard bytes are
			// valid base64 so that consuming them would change the decoded output.
			var inputGuard = Encoding.ASCII.GetBytes (Base64Alphabet);
			int[] chunkSizes = { 1, 15, 16, 17, 31, 32, 33, 64, 100, int.MaxValue };
			var scalar = GetDecodeFunc (CodePath.Scalar);
			var decode = GetPointerDecodeFunc (path);
			const int OutputGuardLength = 64;
			const byte Sentinel = 0xA5;
			var random = new Random (1999);

			for (int iteration = 0; iteration < 500; iteration++) {
				var encoded = random.Next (4) == 0 ? GenerateRandomBytes (random, random.Next (0, 800), 0.95) : GenerateEncoded (random);
				var expected = DecodeChunked (scalar, encoded, int.MaxValue);

				foreach (var chunkSize in chunkSizes) {
					var decoder = new Base64Decoder ();
					var actual = new List<byte> ();
					int size = Math.Max (1, Math.Min (chunkSize, encoded.Length));

					for (int index = 0; index < encoded.Length; index += size) {
						int n = Math.Min (size, encoded.Length - index);
						var input = new byte[n + inputGuard.Length];
						int outputLength = decoder.EstimateOutputLength (n);
						var output = new byte[outputLength + OutputGuardLength];

						Buffer.BlockCopy (encoded, index, input, 0, n);
						Buffer.BlockCopy (inputGuard, 0, input, n, inputGuard.Length);
						output.AsSpan ().Fill (Sentinel);

						fixed (byte* inptr = input, outptr = output)
							n = decode (decoder, inptr, n, outptr, outputLength);

						Assert.That (n, Is.LessThanOrEqualTo (outputLength), $"iteration={iteration}, chunkSize={chunkSize}: output length");

						for (int i = outputLength; i < output.Length; i++) {
							if (output[i] != Sentinel)
								Assert.Fail ($"iteration={iteration}, chunkSize={chunkSize}: wrote past the end of the output buffer at offset {i - outputLength}");
						}

						actual.AddRange (output.AsSpan (0, n).ToArray ());
					}

					Assert.That (actual.ToArray (), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}
			}
		}

		[Test]
		public unsafe void TestDecodeDoesNotAccessMemoryOutOfBounds ([Values] CodePath path, [Values] bool alignEnd)
		{
			// Places each chunk of input and the output buffer immediately before (or after) an inaccessible guard page so
			// that reading or writing even a single byte out of bounds crashes rather than going unnoticed.
			int[] chunkSizes = { 1, 15, 16, 17, 31, 32, 33, 47, 48, 49, 64, 100, 4096 };
			var scalar = GetDecodeFunc (CodePath.Scalar);
			var decode = GetPointerDecodeFunc (path);
			var random = new Random (alignEnd ? 2024 : 2025);

			for (int iteration = 0; iteration < 100; iteration++) {
				var encoded = random.Next (4) == 0 ? GenerateRandomBytes (random, random.Next (0, 800), 0.95) : GenerateEncoded (random);
				var expected = DecodeChunked (scalar, encoded, int.MaxValue);

				foreach (var chunkSize in chunkSizes) {
					var decoder = new Base64Decoder ();
					var actual = new List<byte> ();
					int size = Math.Max (1, Math.Min (chunkSize, encoded.Length));

					for (int index = 0; index < encoded.Length; index += size) {
						int n = Math.Min (size, encoded.Length - index);
						int outputLength = decoder.EstimateOutputLength (n);

						using var input = new GuardedMemory (n, alignEnd);
						using var output = new GuardedMemory (outputLength, alignEnd);

						encoded.AsSpan (index, n).CopyTo (new Span<byte> (input.Start, n));

						n = decode (decoder, input.Start, n, output.Start, outputLength);

						Assert.That (n, Is.LessThanOrEqualTo (outputLength), $"iteration={iteration}, chunkSize={chunkSize}: output length");

						actual.AddRange (new ReadOnlySpan<byte> (output.Start, n).ToArray ());
					}

					Assert.That (actual.ToArray (), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}
			}
		}
	}
}
