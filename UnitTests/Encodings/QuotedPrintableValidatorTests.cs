//
// QuotedPrintableValidatorTests.cs
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

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new QuotedPrintableValidator ());
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new QuotedPrintableValidator ();

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeUnix (int bufferSize)
		{
			TestValidator (new QuotedPrintableValidator (), "wikipedia.qp", wikipedia_unix, bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeDos (int bufferSize)
		{
			TestValidator (new QuotedPrintableValidator (), "wikipedia.qp", wikipedia_dos, bufferSize);
		}

		[TestCase ("=XA")]
		[TestCase ("=AX")]
		public void TestValidateInvalidHexSequence (string hex)
		{
			var rawData = Encoding.ASCII.GetBytes ($"This is some quoted printable text with an invalid {hex} sequence.");
			var validator = new QuotedPrintableValidator ();

			validator.Write (rawData, 0, rawData.Length);

			Assert.That (validator.Validate (), Is.False);
		}

		[Test]
		public void TestValidateInvalidSoftBreak ()
		{
			var rawData = Encoding.ASCII.GetBytes ("This is some quoted printable text with an invalid =\rsoft break");
			var validator = new QuotedPrintableValidator ();

			validator.Write (rawData, 0, rawData.Length);

			Assert.That (validator.Validate (), Is.False);
		}
	}
}
