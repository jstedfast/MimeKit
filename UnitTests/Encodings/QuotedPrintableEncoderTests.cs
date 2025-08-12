//
// QuotedPrintableEncoderTests.cs
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
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new QuotedPrintableEncoder (0));

			AssertArgumentExceptions (new QuotedPrintableEncoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new QuotedPrintableEncoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new QuotedPrintableEncoder (76));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new QuotedPrintableEncoder ());
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestEncodeDos (int bufferSize)
		{
			TestEncoder (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_dos, "wikipedia.qp", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestEncodeUnix (int bufferSize)
		{
			TestEncoder (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_unix, "wikipedia.qp", bufferSize);
		}

		[Test]
		public void TestFlushDos ()
		{
			TestEncoderFlush (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_dos, "wikipedia.qp");
		}

		[Test]
		public void TestFlushUnix ()
		{
			TestEncoderFlush (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_unix, "wikipedia.qp");
		}

		[Test]
		public void TestEncodeSpaceDosLineBreak ()
		{
			const string input = "This line ends with a space \r\nbefore a line break.";
			const string expected = "This line ends with a space=20\nbefore a line break.=\n";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var encoder = new QuotedPrintableEncoder ();

			try {
				var buf = Encoding.ASCII.GetBytes (input);
				int n = encoder.Flush (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestEncodeSpaceUnixLineBreak ()
		{
			const string input = "This line ends with a space \nbefore a line break.";
			const string expected = "This line ends with a space=20\nbefore a line break.=\n";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var encoder = new QuotedPrintableEncoder ();

			try {
				var buf = Encoding.ASCII.GetBytes (input);
				int n = encoder.Flush (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestEncodeEqualSignAt76 ()
		{
			const string expected = "<table style=3D\"width:100%;\" cellpadding=3D\"0\" cellspacing=3D\"0\" border=3D\"=\n0\"><tr><td style=3D\"width:100%;text-align:center;background-color:;\" bgcolo=\nr=3D\"\">Test</td></tr><table>=\n";
			const string text = "<table style=\"width:100%;\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"width:100%;text-align:center;background-color:;\" bgcolor=\"\">Test</td></tr><table>";
			var input = Encoding.ASCII.GetBytes (text);
			var encoder = new QuotedPrintableEncoder (76);
			var output = new byte[encoder.EstimateOutputLength (input.Length)];
			var outputLength = encoder.Flush (input, 0, input.Length, output);
			var encoded = Encoding.ASCII.GetString (output, 0, outputLength);

			Assert.That (encoded, Is.EqualTo (expected));
		}

		[Test]
		public void TestFlush ()
		{
			const string input = "This line ends with a space ";
			const string expected = "This line ends with a space=20=\n";
			var encoder = new QuotedPrintableEncoder ();
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			buf = Encoding.ASCII.GetBytes (input);
			n = encoder.Flush (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));

			buf = Encoding.ASCII.GetBytes (expected);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (input));
		}

		[Test]
		public void TestEncodeHebrew ()
		{
			const string expected = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =\n=E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).\n";
			const string input = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).\n";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QuotedPrintableEncoder (72);
			var output = new byte[1024];

			var buf = encoding.GetBytes (input);
			int n = encoder.Flush (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}
	}
}
