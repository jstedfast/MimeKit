//
// Rfc3492Tests.cs
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

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class Rfc3492Tests
	{
		static int[] GetCodePoints (string text)
		{
			var codePoints = new List<int> ();

			for (int i = 0; i < text.Length; i++) {
				codePoints.Add (char.ConvertToUtf32 (text, i));

				if (char.IsHighSurrogate (text[i]))
					i++;
			}

			return codePoints.ToArray ();
		}

		static string Encode (int[] codePoints)
		{
			var output = new char[256];
			int length = Rfc3492.Encode (codePoints, output);

			Assert.That (length, Is.GreaterThanOrEqualTo (0), "Encode");
			Assert.That (Rfc3492.GetEncodedLength (codePoints), Is.EqualTo (length), "GetEncodedLength");

			return new string (output, 0, length);
		}

		// RFC 3492, Section 7.1: Sample strings (plus a few common domain labels).
		[TestCase ("\u4ED6\u4EEC\u4E3A\u4EC0\u4E48\u4E0D\u8BF4\u4E2D\u6587", "ihqwcrb4cv8a8dqg056pqjye")]
		[TestCase ("3\u5E74B\u7D44\u91D1\u516B\u5148\u751F", "3B-ww4c5e180e575a65lsy2b")]
		[TestCase ("\u5B89\u5BA4\u5948\u7F8E\u6075-with-SUPER-MONKEYS", "-with-SUPER-MONKEYS-pc58ag80a8qai00g7n9n")]
		[TestCase ("\u305D\u306E\u30B9\u30D4\u30FC\u30C9\u3067", "d9juau41awczczp")]
		[TestCase ("-> $1.00 <-", "-> $1.00 <--")]
		[TestCase ("b\u00FCcher", "bcher-kva")]
		[TestCase ("\u043C\u043E\u0439\u0434\u043E\u043C\u0435\u043D", "d1acklchcc")]
		[TestCase ("\U0001F600", "e28h")]
		public void TestEncodeDecode (string unicode, string punycode)
		{
			var codePoints = GetCodePoints (unicode);

			Assert.That (Encode (codePoints), Is.EqualTo (punycode), "Encode");

			var decoded = new int[256];

			Assert.That (Rfc3492.TryDecode (punycode.AsSpan (), decoded, out int count), Is.True, "TryDecode");
			Assert.That (decoded.AsSpan (0, count).ToArray (), Is.EqualTo (codePoints), "TryDecode");

			// Decoding is case-insensitive for the encoded digits.
			Assert.That (Rfc3492.TryDecode (punycode.ToUpperInvariant ().AsSpan (), decoded, out count), Is.True, "TryDecode (uppercase)");
			Assert.That (count, Is.EqualTo (codePoints.Length), "TryDecode (uppercase)");
		}

		[Test]
		public void TestDecodeEmpty ()
		{
			Assert.That (Rfc3492.TryDecode (ReadOnlySpan<char>.Empty, new int[1], out int count), Is.True);
			Assert.That (count, Is.EqualTo (0));
		}

		[TestCase ("b\u00FCcher-kva", TestName = "TestDecodeNonAsciiBasicCodePoint")]
		[TestCase ("bcher-k\u00FCa", TestName = "TestDecodeNonAsciiDigit")]
		[TestCase ("bcher-k_a", TestName = "TestDecodeInvalidDigit")]
		[TestCase ("bcher-z", TestName = "TestDecodeTruncated")]
		[TestCase ("-kva", TestName = "TestDecodeLeadingDelimiter")]
		[TestCase ("99999999999", TestName = "TestDecodeOverflow")]
		[TestCase ("zzzzzzzzz9", TestName = "TestDecodeBeyondMaxCodePoint")]
		public void TestDecodeInvalid (string punycode)
		{
			Assert.That (Rfc3492.TryDecode (punycode.AsSpan (), new int[256], out _), Is.False);
		}

		[Test]
		public void TestDecodeSurrogate ()
		{
			var punycode = Encode (new int[] { 'a', 0xD800 });

			Assert.That (Rfc3492.TryDecode (punycode.AsSpan (), new int[256], out _), Is.False);
		}

		[Test]
		public void TestDecodeOutputTooSmall ()
		{
			// The basic code points do not fit.
			Assert.That (Rfc3492.TryDecode ("bcher-kva".AsSpan (), new int[4], out _), Is.False, "basic");

			// The basic code points fit, but the decoded code point does not.
			Assert.That (Rfc3492.TryDecode ("bcher-kva".AsSpan (), new int[5], out _), Is.False, "extended");
			Assert.That (Rfc3492.TryDecode ("bcher-kva".AsSpan (), new int[6], out int count), Is.True, "exact");
			Assert.That (count, Is.EqualTo (6));
		}

		[Test]
		public void TestEncodeOutputTooSmall ()
		{
			var codePoints = GetCodePoints ("b\u00FCcher");

			// "bcher-kva" is 9 characters long; fail while writing the basic code points, the delimiter and the digits.
			Assert.That (Rfc3492.Encode (codePoints, new char[3]), Is.EqualTo (-1), "basic");
			Assert.That (Rfc3492.Encode (codePoints, new char[5]), Is.EqualTo (-1), "delimiter");
			Assert.That (Rfc3492.Encode (codePoints, new char[7]), Is.EqualTo (-1), "digits");
			Assert.That (Rfc3492.Encode (codePoints, new char[8]), Is.EqualTo (-1), "final digit");
			Assert.That (Rfc3492.Encode (codePoints, new char[9]), Is.EqualTo (9), "exact");
		}
	}
}
