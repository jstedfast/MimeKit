//
// Utf8Tests.cs
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

using System.Buffers;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class Utf8Tests
	{
		// Note: these exercise Utf8.IsValidCore() directly (rather than Utf8.IsValid()) because on
		// net8.0 and later, Utf8.IsValid() forwards to System.Text.Unicode.Utf8.IsValid(). The managed
		// fallback is what ships on .NET Framework and netstandard, so it needs coverage of its own.

		static void AssertIsValid (byte[] value, bool expected, string message)
		{
			Assert.That (Utf8.IsValidCore (value), Is.EqualTo (expected), message);
			Assert.That (Utf8.IsValid (value), Is.EqualTo (expected), message);
			AssertInvalidSequenceIndex (value, IndexOfInvalidSequence (value), message);

#if NET8_0_OR_GREATER
			// Make sure the managed fallback agrees with the framework implementation.
			Assert.That (Utf8.IsValidCore (value), Is.EqualTo (System.Text.Unicode.Utf8.IsValid (value)), $"{message} (does not match System.Text.Unicode.Utf8.IsValid)");
#endif
		}

		static int IndexOfInvalidSequence (ReadOnlySpan<byte> value)
		{
			int index = 0;

			while (index < value.Length) {
				if (Rune.DecodeFromUtf8 (value.Slice (index), out _, out int consumed) != OperationStatus.Done)
					return index;

				index += consumed;
			}

			return -1;
		}

		static void AssertInvalidSequenceIndex (ReadOnlySpan<byte> value, int expected, string message)
		{
			Assert.That (Utf8.IndexOfInvalidSequenceCore (value), Is.EqualTo (expected), $"{message} (scalar)");
			Assert.That (Utf8.IndexOfInvalidSequence (value), Is.EqualTo (expected), $"{message} (dispatcher)");
			if (Avx2.IsSupported)
				Assert.That (Utf8.IndexOfInvalidSequenceAvx2 (value), Is.EqualTo (expected), $"{message} (AVX2)");
			if (Ssse3.IsSupported && Sse41.IsSupported)
				Assert.That (Utf8.IndexOfInvalidSequenceSse (value), Is.EqualTo (expected), $"{message} (SSE)");
			if (AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian)
				Assert.That (Utf8.IndexOfInvalidSequenceArm64 (value), Is.EqualTo (expected), $"{message} (ARM64)");
		}

		[Test]
		public void TestRequiredArm64SimdSupport ()
		{
			if (Environment.GetEnvironmentVariable ("MIMEKIT_REQUIRE_ARM64_SIMD") != "1")
				Assert.Ignore ("This run does not require ARM64 SIMD.");

			Assert.That (AdvSimd.Arm64.IsSupported, Is.True, "The ARM64 CI job must execute the AdvSIMD implementations.");
			Assert.That (BitConverter.IsLittleEndian, Is.True, "The ARM64 UTF-8 implementation requires little-endian byte order.");
		}

		[TestCase ("a")]
		[TestCase ("\u00e9")]
		[TestCase ("\u65e5")]
		[TestCase ("\U0001f600")]
		[TestCase ("ascii \u00e9 \u65e5\U0001f600")]
		public void TestIndexOfInvalidSequenceVectorBoundaries (string text)
		{
			var source = Encoding.UTF8.GetBytes (string.Concat (Enumerable.Repeat (text, 1024)));
			var bytes = new byte[1056];
			var substitutions = new byte[] { 0, 0x7f, 0x80, 0xbf, 0xc0, 0xc2, 0xdf, 0xe0, 0xed, 0xef, 0xf0, 0xf4, 0xf5, 0xff };

			for (int alignment = 0; alignment < 32; alignment++) {
				Array.Fill (bytes, (byte) 0xff);
				source.AsSpan (0, 1024).CopyTo (bytes.AsSpan (alignment));
				for (int length = 0; length <= 1024; length++) {
					var value = bytes.AsSpan (alignment, length);
					AssertInvalidSequenceIndex (value, IndexOfInvalidSequence (value), $"alignment={alignment}, length={length}");
				}
			}

			foreach (int length in new[] { 15, 16, 17, 31, 32, 33, 63, 64, 65, 127, 128, 129, 255, 256, 257, 511, 512 }) {
				source.AsSpan (0, length).CopyTo (bytes.AsSpan (1));
				for (int index = 0; index < length; index++) {
					byte saved = bytes[index + 1];
					foreach (byte replacement in substitutions) {
						bytes[index + 1] = replacement;
						var value = bytes.AsSpan (1, length);
						AssertInvalidSequenceIndex (value, IndexOfInvalidSequence (value), $"length={length}, index={index}, byte={replacement:X2}");
					}
					bytes[index + 1] = saved;
				}
			}
		}

		[TestCase (new byte[] { 0x80 })]
		[TestCase (new byte[] { 0xFF })]
		[TestCase (new byte[] { 0xC0, 0xAF })]
		[TestCase (new byte[] { 0xE0, 0x80, 0x80 })]
		[TestCase (new byte[] { 0xED, 0xA0, 0x80 })]
		[TestCase (new byte[] { 0xF0, 0x80, 0x80, 0x80 })]
		[TestCase (new byte[] { 0xF4, 0x90, 0x80, 0x80 })]
		[TestCase (new byte[] { 0xC2, 0x21 })]
		[TestCase (new byte[] { 0xE2, 0x21, 0x80 })]
		[TestCase (new byte[] { 0xE2, 0x82, 0x21 })]
		[TestCase (new byte[] { 0xF0, 0x21, 0x80, 0x80 })]
		[TestCase (new byte[] { 0xF0, 0x90, 0x21, 0x80 })]
		[TestCase (new byte[] { 0xF0, 0x90, 0x80, 0x21 })]
		[TestCase (new byte[] { 0xC2 })]
		[TestCase (new byte[] { 0xE2, 0x82 })]
		[TestCase (new byte[] { 0xF0, 0x90, 0x80 })]
		public void TestIndexOfInvalidSequenceAfterValidPrefix (byte[] malformed)
		{
			foreach (int padding in new[] { 0, 15, 16, 31, 32, 127, 4096 }) {
				var prefix = Encoding.UTF8.GetBytes (new string ('a', padding) + "\0\u00e9\r\n \u65e5\U0001f600");
				var bytes = new byte[prefix.Length + malformed.Length + 2];
				bytes[0] = bytes[bytes.Length - 1] = 0xFF;
				prefix.CopyTo (bytes, 1);
				malformed.CopyTo (bytes, prefix.Length + 1);

				AssertInvalidSequenceIndex (bytes.AsSpan (1, prefix.Length), -1, $"valid prefix, padding={padding}");
				AssertInvalidSequenceIndex (bytes.AsSpan (1, bytes.Length - 2), prefix.Length, $"padding={padding}");

				// Keep malformed sequences inside full vectors, not just in the scalar tail.
				var withSuffix = new byte[bytes.Length + 128];
				Array.Fill (withSuffix, (byte) 'a');
				bytes.AsSpan (0, bytes.Length - 1).CopyTo (withSuffix);
				withSuffix[withSuffix.Length - 1] = 0xff;
				AssertInvalidSequenceIndex (withSuffix.AsSpan (1, withSuffix.Length - 2), prefix.Length, $"padding={padding}, ASCII suffix");
			}

			AssertInvalidSequenceIndex (malformed, 0, "no prefix");
		}

		[Test]
		public void TestIndexOfInvalidSequenceRandomInput ()
		{
			var random = new Random (2083);
			var prefix = Encoding.UTF8.GetBytes ("ascii\0\u00e9\r\n \u65e5\U0001f600");
			var bytes = new byte[256];

			for (int i = 0; i < 4096; i++) {
				random.NextBytes (bytes);
				int offset = random.Next (32);
				prefix.CopyTo (bytes, offset);
				int length = random.Next (bytes.Length - offset + 1);
				var value = bytes.AsSpan (offset, length);

				AssertInvalidSequenceIndex (value, IndexOfInvalidSequence (value), $"iteration={i}");
			}
		}

		[Test]
		public void TestEmpty ()
		{
			AssertIsValid (new byte[0], true, "empty");
		}

		[Test]
		public void TestAscii ()
		{
			AssertIsValid (Encoding.ASCII.GetBytes ("This is a plain old ASCII string."), true, "ascii");

			// Every single ASCII byte, including the control characters and NUL, is well-formed UTF-8.
			for (int i = 0; i < 0x80; i++)
				AssertIsValid (new byte[] { (byte) i }, true, $"ascii 0x{i:X2}");
		}

		[Test]
		public void TestValidMultiByteSequences ()
		{
			AssertIsValid (new byte[] { 0xC2, 0x80 }, true, "U+0080 (shortest 2-byte)");
			AssertIsValid (new byte[] { 0xDF, 0xBF }, true, "U+07FF (longest 2-byte)");
			AssertIsValid (new byte[] { 0xE0, 0xA0, 0x80 }, true, "U+0800 (shortest 3-byte)");
			AssertIsValid (new byte[] { 0xED, 0x9F, 0xBF }, true, "U+D7FF (just below the surrogate range)");
			AssertIsValid (new byte[] { 0xEE, 0x80, 0x80 }, true, "U+E000 (just above the surrogate range)");
			AssertIsValid (new byte[] { 0xEF, 0xBF, 0xBF }, true, "U+FFFF (longest 3-byte)");
			AssertIsValid (new byte[] { 0xF0, 0x90, 0x80, 0x80 }, true, "U+10000 (shortest 4-byte)");
			AssertIsValid (new byte[] { 0xF4, 0x8F, 0xBF, 0xBF }, true, "U+10FFFF (largest scalar value)");

			AssertIsValid (Encoding.UTF8.GetBytes ("Привет, мир!"), true, "cyrillic");
			AssertIsValid (Encoding.UTF8.GetBytes ("日本語のテキスト"), true, "japanese");
			AssertIsValid (Encoding.UTF8.GetBytes ("emoji: \U0001F600\U0001F680"), true, "emoji");
		}

		[Test]
		public void TestBareContinuationBytes ()
		{
			for (int i = 0x80; i < 0xC0; i++)
				AssertIsValid (new byte[] { (byte) i }, false, $"bare continuation byte 0x{i:X2}");

			AssertIsValid (new byte[] { 0x80, 0x80 }, false, "two bare continuation bytes");
			AssertIsValid (new byte[] { (byte) 'a', 0xBF, (byte) 'b' }, false, "bare continuation byte between ascii");
		}

		[Test]
		public void TestOverlongEncodings ()
		{
			AssertIsValid (new byte[] { 0xC0, 0x80 }, false, "overlong U+0000");
			AssertIsValid (new byte[] { 0xC1, 0xBF }, false, "overlong U+007F");
			AssertIsValid (new byte[] { 0xE0, 0x80, 0x80 }, false, "overlong 3-byte U+0000");
			AssertIsValid (new byte[] { 0xE0, 0x9F, 0xBF }, false, "overlong 3-byte U+07FF");
			AssertIsValid (new byte[] { 0xF0, 0x80, 0x80, 0x80 }, false, "overlong 4-byte U+0000");
			AssertIsValid (new byte[] { 0xF0, 0x8F, 0xBF, 0xBF }, false, "overlong 4-byte U+FFFF");
		}

		[Test]
		public void TestSurrogates ()
		{
			AssertIsValid (new byte[] { 0xED, 0xA0, 0x80 }, false, "U+D800 (first high surrogate)");
			AssertIsValid (new byte[] { 0xED, 0xAF, 0xBF }, false, "U+DBFF (last high surrogate)");
			AssertIsValid (new byte[] { 0xED, 0xB0, 0x80 }, false, "U+DC00 (first low surrogate)");
			AssertIsValid (new byte[] { 0xED, 0xBF, 0xBF }, false, "U+DFFF (last low surrogate)");

			// CESU-8 encoding of U+10000 (a surrogate pair encoded as two 3-byte sequences).
			AssertIsValid (new byte[] { 0xED, 0xA0, 0x80, 0xED, 0xB0, 0x80 }, false, "CESU-8 surrogate pair");
		}

		[Test]
		public void TestOutOfRange ()
		{
			AssertIsValid (new byte[] { 0xF4, 0x90, 0x80, 0x80 }, false, "U+110000 (just above the largest scalar value)");

			for (int i = 0xF5; i < 0x100; i++)
				AssertIsValid (new byte[] { (byte) i, 0x80, 0x80, 0x80 }, false, $"invalid leading byte 0x{i:X2}");
		}

		[Test]
		public void TestFiveAndSixByteSequences ()
		{
			AssertIsValid (new byte[] { 0xF8, 0x88, 0x80, 0x80, 0x80 }, false, "5-byte sequence");
			AssertIsValid (new byte[] { 0xFC, 0x84, 0x80, 0x80, 0x80, 0x80 }, false, "6-byte sequence");
			AssertIsValid (new byte[] { 0xFE }, false, "0xFE");
			AssertIsValid (new byte[] { 0xFF }, false, "0xFF");
		}

		[Test]
		public void TestTruncatedSequences ()
		{
			AssertIsValid (new byte[] { 0xC2 }, false, "truncated 2-byte");
			AssertIsValid (new byte[] { 0xE0, 0xA0 }, false, "truncated 3-byte");
			AssertIsValid (new byte[] { 0xE0 }, false, "truncated 3-byte (1 of 3)");
			AssertIsValid (new byte[] { 0xF0, 0x90, 0x80 }, false, "truncated 4-byte");
			AssertIsValid (new byte[] { 0xF0, 0x90 }, false, "truncated 4-byte (2 of 4)");
			AssertIsValid (new byte[] { 0xF0 }, false, "truncated 4-byte (1 of 4)");

			// Truncated sequences at the end of otherwise valid text.
			AssertIsValid (new byte[] { (byte) 'a', (byte) 'b', 0xE0, 0xA0 }, false, "truncated at end of ascii");
		}

		[Test]
		public void TestMissingContinuationBytes ()
		{
			AssertIsValid (new byte[] { 0xC2, (byte) 'a' }, false, "2-byte followed by ascii");
			AssertIsValid (new byte[] { 0xE0, 0xA0, (byte) 'a' }, false, "3-byte followed by ascii");
			AssertIsValid (new byte[] { 0xF0, 0x90, 0x80, (byte) 'a' }, false, "4-byte followed by ascii");
			AssertIsValid (new byte[] { 0xE0, 0xA0, 0xC2, 0x80 }, false, "3-byte interrupted by a new sequence");
		}

		[Test]
		public void TestRawSingleByteCharsetText ()
		{
			// Raw koi8-r text (the sort of thing that should raise Unexpected8BitBytesInHeader).
			AssertIsValid (new byte[] { 0xF0, 0xD2, 0xC9, 0xD7, 0xC5, 0xD4 }, false, "raw koi8-r");

			// Raw iso-8859-1 text.
			AssertIsValid (new byte[] { (byte) 'c', (byte) 'a', (byte) 'f', 0xE9 }, false, "raw iso-8859-1");
		}
	}
}
