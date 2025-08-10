//
// HexDecoderTests.cs
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

#pragma warning disable CS0618 // Type or member is obsolete

using System.Text;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class HexDecoderTests : MimeDecoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new HexDecoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new HexDecoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Default));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new HexDecoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new HexDecoder ());
		}

		[Test]
		public void TestDecodeHebrew ()
		{
			const string input = "This should decode: (%ED%E5%EC%F9 %EF%E1 %E9%EC%E8%F4%F0) while %X1%S1%Z1 should not";
			const string expected = "This should decode: (םולש ןב ילטפנ) while %X1%S1%Z1 should not";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var decoder = new HexDecoder ();
			var output = new byte[1024];

			var buf = Encoding.ASCII.GetBytes (input);
			int n = decoder.Decode (buf, 0, buf.Length, output);
			var actual = encoding.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeAttrSpecials ()
		{
			const string input = "%20%09%0D%0AABCabc123!%40#$%25^&%2A%28%29_+`-%3D%5B%5D%5C{}|%3B%3A%27%22%2C.%2F%3C%3E%3F";
			const string expected = " \t\r\nABCabc123!@#$%^&*()_+`-=[]\\{}|;:'\",./<>?";
			var decoder = new HexDecoder ();
			var output = new byte[1024];

			var buf = Encoding.ASCII.GetBytes (input);
			int n = decoder.Decode (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}
	}
}

#pragma warning restore CS0618 // Type or member is obsolete
