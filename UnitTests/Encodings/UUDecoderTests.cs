//
// UUDecoderTests.cs
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
	public class UUDecoderTests : MimeDecoderTestsBase
	{
		static readonly bool DefaultHwAccel = UUDecoder.EnableHardwareAcceleration;

		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new UUDecoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new UUDecoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.UUEncode));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new UUDecoder (true));
			CloneAndAssert (new UUDecoder (false));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new UUDecoder (true));
			ResetAndAssert (new UUDecoder (false));
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestDecode (int bufferSize)
		{
			TestDecoder (new UUDecoder (), photo, "photo.uu", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestDecodeBeginStateChanges (int bufferSize)
		{
			TestDecoder (new UUDecoder (), photo, "photo.uu-states", bufferSize);
		}

		[Test]
		public void TestDecodeEndOfLineStateAcrossChunks ([Values] CodePath path)
		{
			// The first line's length octet claims 4 bytes but only 1 quartet (3 bytes) follows, so the decoder
			// must remember that it just saw a line break in order to treat the '#' as the next line's length octet.
			var input = Encoding.ASCII.GetBytes ("$86)C\r\n#86)C\r\n`\r\n");
			var decode = GetDecodeFunc (path);
			var expected = DecodeChunked (decode, input, int.MaxValue, true);

			Assert.That (expected, Has.Length.EqualTo (6));
			Assert.That (expected.AsSpan (3).ToArray (), Is.EqualTo (expected.AsSpan (0, 3).ToArray ()));

			for (int split = 1; split < input.Length; split++) {
				var decoder = new UUDecoder (true);
				var output = new byte[decoder.EstimateOutputLength (input.Length) * 2];
				var rest = new byte[output.Length];

				int n = decode (decoder, input, 0, split, output);
				int m = decode (decoder, input, split, input.Length - split, rest);
				Array.Copy (rest, 0, output, n, m);

				Assert.That (output.AsSpan (0, n + m).ToArray (), Is.EqualTo (expected), $"split={split}");
			}
		}

		[Test]
		public void TestDecodeWithoutHardwareAcceleration ([Values (4096, 1024,  16, 1)] int bufferSize)
		{
			UUDecoder.EnableHardwareAcceleration = false;

			try {
				TestDecoder (new UUDecoder (), photo, "photo.uu", bufferSize);
			} finally {
				UUDecoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		public enum CodePath
		{
			Scalar,
			Ssse3,
			Avx2,
			AdvSimd
		}

		delegate int DecodeFunc (UUDecoder decoder, byte[] input, int startIndex, int length, byte[] output);

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

		static byte[] DecodeChunked (DecodeFunc decode, byte[] input, int chunkSize, bool payloadOnly = false)
		{
			var decoder = new UUDecoder (payloadOnly);
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

		static byte[] DecodeInPlace (DecodeFunc decode, byte[] input, bool payloadOnly = false)
		{
			var decoder = new UUDecoder (payloadOnly);
			var buffer = new byte[Math.Max (input.Length, decoder.EstimateOutputLength (input.Length))];

			Buffer.BlockCopy (input, 0, buffer, 0, input.Length);

			int n = decode (decoder, buffer, 0, input.Length, buffer);

			Array.Resize (ref buffer, n);

			return buffer;
		}

		static char EncodeSextet (int value, bool backtick)
		{
			return value == 0 && backtick ? '`' : (char) (value + 0x20);
		}

		// Uuencodes the data using the specified number of bytes per line (normally 45).
		static string UUEncode (byte[] data, int bytesPerLine, string newLine, bool backtick, bool header = true)
		{
			var builder = new StringBuilder ();

			if (header)
				builder.Append ("begin 644 file.bin").Append (newLine);

			for (int index = 0; index < data.Length; index += bytesPerLine) {
				int n = Math.Min (bytesPerLine, data.Length - index);

				builder.Append (EncodeSextet (n, backtick));

				for (int i = 0; i < n; i += 3) {
					int b0 = data[index + i];
					int b1 = i + 1 < n ? data[index + i + 1] : 0;
					int b2 = i + 2 < n ? data[index + i + 2] : 0;

					builder.Append (EncodeSextet (b0 >> 2, backtick));
					builder.Append (EncodeSextet (((b0 << 4) | (b1 >> 4)) & 0x3F, backtick));
					builder.Append (EncodeSextet (((b1 << 2) | (b2 >> 6)) & 0x3F, backtick));
					builder.Append (EncodeSextet (b2 & 0x3F, backtick));
				}

				builder.Append (newLine);
			}

			builder.Append (EncodeSextet (0, backtick)).Append (newLine);
			builder.Append ("end").Append (newLine);

			return builder.ToString ();
		}

		static byte[] GetBytes (string text)
		{
			var bytes = new byte[text.Length];

			for (int i = 0; i < text.Length; i++)
				bytes[i] = (byte) text[i];

			return bytes;
		}

		[Test]
		public void TestDecodePhoto ([Values] CodePath path, [Values (4096, 1024, 61, 33, 16, 1)] int bufferSize)
		{
			var decode = GetDecodeFunc (path);
			var encoded = File.ReadAllBytes (Path.Combine (dataDir, "photo.uu"));

			Assert.That (DecodeChunked (decode, encoded, bufferSize), Is.EqualTo (photo));
		}

		static readonly object[] LineFormats = {
			new object[] { 1, "\n" },
			new object[] { 2, "\r\n" },
			new object[] { 3, "\r\n" },
			new object[] { 5, "\n" },
			new object[] { 10, "\r\n" },
			new object[] { 11, "\r\n" },
			new object[] { 12, "\n" },
			new object[] { 13, "\r\n" },
			new object[] { 23, "\r\n" },
			new object[] { 24, "\n" },
			new object[] { 25, "\r\n" },
			new object[] { 44, "\r\n" },
			new object[] { 45, "\r\n" },
			new object[] { 45, "\n" },
			new object[] { 46, "\r\n" },
			new object[] { 63, "\n" },
		};

		[Test]
		public void TestDecodeRandomData ([Values] CodePath path, [ValueSource (nameof (LineFormats))] object[] format, [Values] bool backtick)
		{
			int bytesPerLine = (int) format[0];
			var newLine = (string) format[1];
			var random = new Random (bytesPerLine * 31 + newLine.Length);
			int[] chunkSizes = { 1, 3, 15, 16, 17, 31, 33, 61, 62, 64, 4096, int.MaxValue };
			var decode = GetDecodeFunc (path);

			for (int length = 0; length < 1200; length += random.Next (1, 50)) {
				var data = new byte[length];

				random.NextBytes (data);

				var encoded = GetBytes (UUEncode (data, bytesPerLine, newLine, backtick));

				foreach (var chunkSize in chunkSizes)
					Assert.That (DecodeChunked (decode, encoded, chunkSize), Is.EqualTo (data), $"length={length}, chunkSize={chunkSize}");

				Assert.That (DecodeInPlace (decode, encoded), Is.EqualTo (data), $"In-place: length={length}");

				var payload = GetBytes (UUEncode (data, bytesPerLine, newLine, backtick, false));

				Assert.That (DecodeChunked (decode, payload, int.MaxValue, true), Is.EqualTo (data), $"Payload-only: length={length}");
				Assert.That (DecodeInPlace (decode, payload, true), Is.EqualTo (data), $"Payload-only in-place: length={length}");
			}
		}

		[Test]
		public void TestDecodeRandomDataWithGarbage ([Values] CodePath path)
		{
			// Note: The scalar decoder ignores '\r' anywhere, treats '\n' as the end of a line, and maps every other byte to
			// a sextet, so inserting any of these characters (or deleting characters) changes the decoded output in interesting
			// ways. The SIMD code paths must produce exactly the same output as the scalar decoder.
			const string garbage = "\r\n\r\n \t`!~\0\u0080\u00ff@MZa";
			int[] chunkSizes = { 1, 7, 16, 33, 61, 100, int.MaxValue };
			var scalar = GetDecodeFunc (CodePath.Scalar);
			var decode = GetDecodeFunc (path);
			var random = new Random (1214);

			for (int iteration = 0; iteration < 500; iteration++) {
				var data = new byte[random.Next (0, 600)];

				random.NextBytes (data);

				var text = UUEncode (data, random.Next (1, 64), random.Next (2) == 0 ? "\n" : "\r\n", random.Next (2) == 0);
				var builder = new StringBuilder ();
				var positions = new SortedSet<int> ();
				int insertions = random.Next (0, 20);

				for (int i = 0; i < insertions; i++)
					positions.Add (random.Next (0, text.Length + 1));

				int last = 0;
				foreach (var position in positions) {
					if (position < last)
						continue;

					builder.Append (text, last, position - last);

					if (random.Next (4) == 0 && position < text.Length) {
						// delete a character instead of inserting garbage
						last = position + 1;
						continue;
					}

					int count = random.Next (1, 4);
					for (int i = 0; i < count; i++)
						builder.Append (garbage[random.Next (garbage.Length)]);
					last = position;
				}
				builder.Append (text, last, text.Length - last);

				var encoded = GetBytes (builder.ToString ());
				bool payloadOnly = random.Next (4) == 0;

				// The scalar decoder is the reference implementation.
				var expected = DecodeChunked (scalar, encoded, int.MaxValue, payloadOnly);

				if (positions.Count == 0 && !payloadOnly)
					Assert.That (expected, Is.EqualTo (data), $"iteration={iteration}: reference decode");

				foreach (var chunkSize in chunkSizes) {
					Assert.That (DecodeChunked (scalar, encoded, chunkSize, payloadOnly), Is.EqualTo (expected), $"Scalar: iteration={iteration}, chunkSize={chunkSize}");
					Assert.That (DecodeChunked (decode, encoded, chunkSize, payloadOnly), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}

				Assert.That (DecodeInPlace (decode, encoded, payloadOnly), Is.EqualTo (expected), $"In-place: iteration={iteration}");
			}
		}
	}
}
