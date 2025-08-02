//
// QuotedPrintableDecoderTests.cs
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

using System.Text;
using System.Buffers;

using MimeKit;
using MimeKit.Utils;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableDecoderTests : MimeDecoderTestsBase
	{
		static readonly string[] qpEncodedPatterns = {
			"=e1=e2=E3=E4\r\n",
			"=e1=g2=E3=E4\r\n",
			"=e1=eg=E3=E4\r\n",
			"   =e1 =e2  =E3\t=E4  \t \t    \r\n",
			"Soft line=\r\n\tHard line\r\n",
			"width==\r\n340 height=3d200\r\n",

		};
		static readonly string[] qpDecodedPatterns = {
			"\u00e1\u00e2\u00e3\u00e4\r\n",
			"\u00e1=g2\u00e3\u00e4\r\n",
			"\u00e1=eg\u00e3\u00e4\r\n",
			"   \u00e1 \u00e2  \u00e3\t\u00e4  \t \t    \r\n",
			"Soft line\tHard line\r\n",
			"width=340 height=200\r\n"
		};

		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new QuotedPrintableDecoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new QuotedPrintableDecoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new QuotedPrintableDecoder (true));
			CloneAndAssert (new QuotedPrintableDecoder (false));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new QuotedPrintableDecoder (true));
			ResetAndAssert (new QuotedPrintableDecoder (false));
		}

		[Test]
		public void TestDecodePatterns ()
		{
			var output = ArrayPool<byte>.Shared.Rent (4096);
			var decoder = new QuotedPrintableDecoder ();
			var encoding = CharsetUtils.Latin1;

			try {
				for (int i = 0; i < qpEncodedPatterns.Length; i++) {
					decoder.Reset ();

					var buf = encoding.GetBytes (qpEncodedPatterns[i]);
					int n = decoder.Decode (buf, 0, buf.Length, output);

					var actual = encoding.GetString (output, 0, n);
					Assert.That (actual, Is.EqualTo (qpDecodedPatterns[i]), $"Failed to decode qpEncodedPatterns[{i}]");
				}
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestDecode (int bufferSize)
		{
			TestDecoder (new QuotedPrintableDecoder (), wikipedia_unix, "wikipedia.qp", bufferSize, true);
		}

		[Test]
		public void TestDecodeEqualSignAt76 ()
		{
			const string encoded = "<table style=3D\"width:100%;\" cellpadding=3D\"0\" cellspacing=3D\"0\" border=3D\"=\n0\"><tr><td style=3D\"width:100%;text-align:center;background-color:;\" bgcolo=\nr=3D\"\">Test</td></tr><table>=\n";
			const string expected = "<table style=\"width:100%;\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"width:100%;text-align:center;background-color:;\" bgcolor=\"\">Test</td></tr><table>";
			var input = Encoding.ASCII.GetBytes (encoded);
			var decoder = new QuotedPrintableDecoder ();

			var output = new byte[decoder.EstimateOutputLength (input.Length)];
			var decodedLength = decoder.Decode (input, 0, input.Length, output);
			var decoded = Encoding.ASCII.GetString (output, 0, decodedLength);

			Assert.That (decoded, Is.EqualTo (expected));
		}

		[Test]
		public void TestDecodeInvalidSoftBreak ()
		{
			const string input = "This is an invalid=\rsoft break.";
			const string expected = "This is an invalid=\rsoft break.";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var decoder = new QuotedPrintableDecoder ();

			try {
				Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				var buf = Encoding.ASCII.GetBytes (input);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestDecodeHebrew ()
		{
			const string input = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).";
			const string expected = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var output = ArrayPool<byte>.Shared.Rent (4096);
			var decoder = new QuotedPrintableDecoder ();

			try {
				Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				var buf = Encoding.ASCII.GetBytes (input);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				var actual = encoding.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}
	}
}
