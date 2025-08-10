//
// Rfc2047Base64EncoderTests.cs
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

using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class Rfc2047Base64EncoderTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var encoder = new Rfc2047Base64Encoder ();
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => encoder.Encode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => encoder.Encode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => encoder.Encode (new byte[1], 0, 1, output));
		}
	}
}
