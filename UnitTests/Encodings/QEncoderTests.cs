//
// QEncoderTests.cs
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
using System.Buffers;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new QEncoder (QEncodeMode.Text));
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new QEncoder (QEncodeMode.Text);

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new QEncoder (QEncodeMode.Text));
			CloneAndAssert (new QEncoder (QEncodeMode.Phrase));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new QEncoder (QEncodeMode.Text));
			ResetAndAssert (new QEncoder (QEncodeMode.Phrase));
		}

		[Test]
		public void TestEncodeText ()
		{
			const string expected = "_=09=0D=0AABCabc123!=40#$%^&*=28=29=5F+`-=3D=5B=5D\\{}|=3B=3A'=22=2C=2E=2F=3C=3E=3F";
			const string input = " \t\r\nABCabc123!@#$%^&*()_+`-=[]\\{}|;:'\",./<>?";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var output = ArrayPool<byte>.Shared.Rent (256);
			var encoder = new QEncoder (QEncodeMode.Text);

			try {
				Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				// First, test the Encode() method works as expected.
				var buf = encoding.GetBytes (input);
				int n = encoder.Encode (buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);

				Assert.That (actual, Is.EqualTo (expected), "Encode");

				encoder.Reset ();

				// Second, test that the Flush() method, when passed the full input, produces the same output.
				n = encoder.Flush (buf, 0, buf.Length, output);
				actual = Encoding.ASCII.GetString (output, 0, n);

				Assert.That (actual, Is.EqualTo (expected), "Flush");
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestEncodePhrase ()
		{
			const string expected = "_=09=0D=0AABCabc123!=40=23=24=25=5E=26*=28=29=5F+=60-=3D=5B=5D=5C=7B=7D=7C=3B=3A=27=22=2C=2E/=3C=3E=3F";
			const string input = " \t\r\nABCabc123!@#$%^&*()_+`-=[]\\{}|;:'\",./<>?";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QEncoder (QEncodeMode.Phrase);
			var output = ArrayPool<byte>.Shared.Rent (256);

			try {
				Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				// First, test the Encode() method works as expected.
				var buf = encoding.GetBytes (input);
				int n = encoder.Encode (buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);

				Assert.That (actual, Is.EqualTo (expected), "Encode");

				encoder.Reset ();

				// Second, test that the Flush() method, when passed the full input, produces the same output.
				n = encoder.Flush (buf, 0, buf.Length, output);
				actual = Encoding.ASCII.GetString (output, 0, n);

				Assert.That (actual, Is.EqualTo (expected), "Flush");
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}
	}
}

#pragma warning restore CS0618 // Type or member is obsolete
