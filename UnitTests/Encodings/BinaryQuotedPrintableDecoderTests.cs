//
// BinaryQuotedPrintableDecoderTests.cs
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
    public class BinaryQuotedPrintableDecoderTests : MimeDecoderTestsBase
    {
        static readonly byte[] pdf;

        static BinaryQuotedPrintableDecoderTests ()
        {
            var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "encoders");

            pdf = File.ReadAllBytes (Path.Combine (dataDir, "mimekit.net.pdf"));
        }

        [Test]
        public void TestArgumentExceptions ()
        {
            AssertArgumentExceptions (new BinaryQuotedPrintableDecoder ());
        }

        [Test]
        public void TestEncoding ()
        {
            var decoder = new BinaryQuotedPrintableDecoder ();

            Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
        }

        [Test]
        public void TestClone ()
        {
            CloneAndAssert (new BinaryQuotedPrintableDecoder ());
        }

        [Test]
        public void TestReset ()
        {
            ResetAndAssert (new BinaryQuotedPrintableDecoder ());
        }

        [TestCase (NewLineFormat.Dos)]
		[TestCase (NewLineFormat.Unix)]
		public void TestDecode (NewLineFormat format)
		{
			// First, we need to encode some binary content...
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new EncoderFilter (new BinaryQuotedPrintableEncoder ()));
					if (format == NewLineFormat.Unix)
						filtered.Add (new Dos2UnixFilter ());

					filtered.Write (pdf, 0, pdf.Length);
					filtered.Flush ();
				}

				var content = new MimeContent (stream, ContentEncoding.QuotedPrintable) { IsText = true };

				using (var decoded = new MemoryStream ()) {
					content.DecodeTo (decoded);

					var buf = decoded.GetBuffer ();
					int n = pdf.Length;

					Assert.AreEqual (pdf.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (pdf[i], buf[i], "The byte at offset {0} does not match.", i);
				}
			}
		}
	}
}
