//
// HexEncoderTests.cs
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

using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class HexEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => HexEncoder.Encode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => HexEncoder.Encode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => HexEncoder.Encode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => HexEncoder.Encode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => HexEncoder.Encode (new byte[1], 0, 1, output));
		}

		[Test]
		public void TestEncodeHebrew ()
		{
			const string expected = "%ED%E5%EC%F9%20%EF%E1%20%E9%EC%E8%F4%F0";
			const string input = "םולש ןב ילטפנ";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var buf = encoding.GetBytes (input);
			int n;

			n = HexEncoder.EstimateOutputLength (buf.Length);
			
			Assert.That (n, Is.EqualTo (buf.Length * 3), "EstimateOutputLength");

			var output = new byte[n];
			n = HexEncoder.Encode (buf, 0, buf.Length, output);
			var actual = encoding.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected), "Encode");
		}

		[Test]
		public void TestEncodeAttrSpecials ()
		{
			const string expected = "%20%09%0D%0AABCabc123!%40#$%25^&%2A%28%29_+`-%3D%5B%5D%5C{}|%3B%3A%27%22%2C.%2F%3C%3E%3F";
			const string input = " \t\r\nABCabc123!@#$%^&*()_+`-=[]\\{}|;:'\",./<>?";
			var buf = Encoding.ASCII.GetBytes (input);
			int n;

			n = HexEncoder.EstimateOutputLength (buf.Length);

			Assert.That (n, Is.EqualTo (buf.Length * 3), "EstimateOutputLength");

			var output = new byte[n];
			n = HexEncoder.Encode (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected), "Encode");
		}
	}
}
