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
			AssertArgumentExceptions (new QuotedPrintableValidator (dummyReader, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new QuotedPrintableValidator (dummyReader, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeUnix (int bufferSize)
		{
			var reader = new ComplianceMimeReader ();

			TestValidator (reader, new QuotedPrintableValidator (reader, 0, 1), "wikipedia.qp", wikipedia_unix, bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeDos (int bufferSize)
		{
			var reader = new ComplianceMimeReader ();

			TestValidator (reader, new QuotedPrintableValidator (reader, 0, 1), "wikipedia.qp", wikipedia_dos, bufferSize);
		}

		[TestCase ("=XA", 1)]
		[TestCase ("=AX", 2)]
		public void TestValidateInvalidHexSequence (string hex, int offset)
		{
			string text = $"This is some quoted printable text with an invalid {hex} sequence.";
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new QuotedPrintableValidator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1));
			Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
			Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.IndexOf ('=') + offset));
			Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (1));
		}

		[Test]
		public void TestValidateInvalidSoftBreak ()
		{
			const string text = "This is some quoted printable text with an invalid =\rsoft break";
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new QuotedPrintableValidator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1));
			Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak));
			Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.LastIndexOf ('s')));
			Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (1));
		}
	}
}
