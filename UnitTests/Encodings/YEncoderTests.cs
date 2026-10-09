//
// YEncoderTests.cs
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

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class YEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new YEncoder (59));

			AssertArgumentExceptions (new YEncoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new YEncoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.Default));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new YEncoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new YEncoder ());
		}

		[Test]
		public void TestEncodeEscapesSpecialOctets ()
		{
			var encoder = new YEncoder ();
			byte[] input = { 214, 224, 227, 19, 4, 223, 23 };
			var output = new byte[encoder.EstimateOutputLength (input.Length)];

			int n = encoder.Flush (input, 0, input.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo ("=@=J=M=}=n=IA\n"));
		}

		[Test]
		public void TestEncodeWrapsAtConfiguredLineLength ()
		{
			var encoder = new YEncoder (60);
			var input = new byte[61];
			input.AsSpan ().Fill (23);
			var output = new byte[encoder.EstimateOutputLength (input.Length)];

			int n = encoder.Flush (input, 0, input.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (new string ('A', 60) + "\nA\n"));
		}

		static void AssertEstimateIsSufficient (YEncoder encoder, byte[] input, string message)
		{
			int estimate = encoder.EstimateOutputLength (input.Length);
			var output = new byte[estimate + 1024];

			int n = encoder.Flush (input, 0, input.Length, output);

			Assert.That (n, Is.LessThanOrEqualTo (estimate), message);
		}

		[TestCase (60)]
		[TestCase (128)]
		[TestCase (998)]
		public void TestEstimateOutputLengthWorstCase (int lineLength)
		{
			// 214 + 42 wraps to 0x00, which must always be escaped (2 octets per input byte)
			for (int length = 0; length <= (lineLength * 3) + 1; length++) {
				var input = new byte[length];
				input.AsSpan ().Fill (214);

				AssertEstimateIsSufficient (new YEncoder (lineLength), input, $"lineLength={lineLength} length={length}");

				// start with a partially filled line from a previous Encode() call
				for (int prefix = 1; prefix < lineLength; prefix += lineLength - 2) {
					var encoder = new YEncoder (lineLength);
					var unescaped = new byte[prefix];
					unescaped.AsSpan ().Fill (23);

					encoder.Encode (unescaped, 0, unescaped.Length, new byte[encoder.EstimateOutputLength (unescaped.Length)]);

					AssertEstimateIsSufficient (encoder, input, $"lineLength={lineLength} prefix={prefix} length={length}");
				}
			}
		}

		[Test]
		public void TestEncodeIntoExactlyEstimatedBuffer ()
		{
			var encoder = new YEncoder (60);
			var input = new byte[59];
			input.AsSpan ().Fill (214);
			var output = new byte[encoder.EstimateOutputLength (input.Length)];

			int n = encoder.Flush (input, 0, input.Length, output);
			var expected = string.Concat (Enumerable.Repeat ("=@", 30)) + "\n" + string.Concat (Enumerable.Repeat ("=@", 29)) + "\n";

			Assert.That (Encoding.ASCII.GetString (output, 0, n), Is.EqualTo (expected));
		}
	}
}
