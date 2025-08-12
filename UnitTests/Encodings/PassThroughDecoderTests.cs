//
// PassThroughDecoderTests.cs
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

using System.Buffers;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class PassThroughDecoderTests : MimeDecoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new PassThroughDecoder (ContentEncoding.Default));
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new PassThroughDecoder (ContentEncoding.Default);

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Default));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new PassThroughDecoder (ContentEncoding.Default));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new PassThroughDecoder (ContentEncoding.Default));
		}

		[Test]
		public void TestDecode ()
		{
			const int bufferSize = 1024;
			var decoder = new PassThroughDecoder (ContentEncoding.Default);
			var output = ArrayPool<byte>.Shared.Rent (bufferSize);
			var input = ArrayPool<byte>.Shared.Rent (bufferSize);

			try {
				for (int i = 0; i < input.Length; i++)
					input[i] = (byte) (i & 0xff);

				int n = decoder.Decode (input, 0, input.Length, output);

				Assert.That (n, Is.EqualTo (input.Length));

				for (int i = 0; i < n; i++)
					Assert.That (output[i], Is.EqualTo (input[i]));

				decoder.Reset ();

				unsafe {
					fixed (byte* inbuf = input, outbuf = output) {
						n = decoder.Decode (inbuf, input.Length, outbuf);

						Assert.That (n, Is.EqualTo (input.Length));

						for (int i = 0; i < n; i++)
							Assert.That (output[i], Is.EqualTo (input[i]));
					}
				}
			} finally {
				ArrayPool<byte>.Shared.Return (output);
				ArrayPool<byte>.Shared.Return (input);
			}
		}
	}
}
