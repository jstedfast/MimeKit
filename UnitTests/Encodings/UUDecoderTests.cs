//
// UUDecoderTests.cs
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

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class UUDecoderTests : MimeDecoderTestsBase
	{
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
	}
}
