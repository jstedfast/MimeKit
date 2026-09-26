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

using System.Text;

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

#if NET8_0_OR_GREATER
			// Make sure the managed fallback agrees with the framework implementation.
			Assert.That (Utf8.IsValidCore (value), Is.EqualTo (System.Text.Unicode.Utf8.IsValid (value)), $"{message} (does not match System.Text.Unicode.Utf8.IsValid)");
#endif
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
