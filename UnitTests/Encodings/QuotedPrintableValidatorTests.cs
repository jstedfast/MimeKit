//
// QuotedPrintableValidatorTests.cs
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

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new QuotedPrintableValidator (nullComplianceLogger, MimeComplianceContext.Transport, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new QuotedPrintableValidator (nullComplianceLogger, MimeComplianceContext.Transport, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeUnix (int bufferSize)
		{
			var logger = new TestMimeComplianceLogger ();

			TestValidator (logger, new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1), "wikipedia.qp", wikipedia_unix, bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSizeDos (int bufferSize)
		{
			var logger = new TestMimeComplianceLogger ();

			TestValidator (logger, new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1), "wikipedia.qp", wikipedia_dos, bufferSize);
		}

		[TestCase ("=XA", 1)]
		[TestCase ("=AX", 2)]
		[TestCase ("=A\n", 2)]
		public void TestValidateInvalidHexSequence (string hex, int offset)
		{
			string text = $"This is some quoted printable text with an invalid {hex} sequence.";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (1));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.IndexOf ('=') + offset));
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (1));
		}

		[Test]
		public void TestValidateInvalidSoftBreak ()
		{
			const string text = "This is some quoted printable text with an invalid =\rsoft break";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (1));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.LastIndexOf ('s')));
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (1));
		}

		// Note: These violations are reported at most once per line so that a crafted part cannot emit
		// an issue for every couple of bytes of content. "=~" is the densest form the abuse can take:
		// each pair logs an issue in the EqualSign state and then returns to the pass-through state.
		[Test]
		public void TestValidateInvalidHexSequenceReportedOncePerLine ()
		{
			const string text = "=~=~=~=~\r\n=~=~=~=~\r\n";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (2));

			for (int i = 0; i < logger.Issues.Count; i++) {
				Assert.That (logger.Issues[i].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
				Assert.That (logger.Issues[i].StreamOffset, Is.EqualTo (i * 10 + 1));
				Assert.That (logger.Issues[i].LineNumber, Is.EqualTo (i + 1));
				Assert.That (logger.Issues[i].ColumnNumber, Is.EqualTo (2));
			}
		}

		// Note: This covers the same throttling for the DecodeByte state, where the octet following a
		// valid hex digit turns out not to be one.
		[Test]
		public void TestValidateInvalidHexDigitReportedOncePerLine ()
		{
			const string text = "=A~=A~=A~\r\n=A~=A~=A~\r\n";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (2));

			for (int i = 0; i < logger.Issues.Count; i++) {
				Assert.That (logger.Issues[i].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
				Assert.That (logger.Issues[i].StreamOffset, Is.EqualTo (i * 11 + 2));
				Assert.That (logger.Issues[i].LineNumber, Is.EqualTo (i + 1));
				Assert.That (logger.Issues[i].ColumnNumber, Is.EqualTo (3));
			}
		}

		[Test]
		public void TestValidateInvalidSoftBreakReportedOncePerLine ()
		{
			const string text = "=\r=\r=\r\r\n=\r=\r=\r\r\n";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (2));

			for (int i = 0; i < logger.Issues.Count; i++) {
				Assert.That (logger.Issues[i].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak));
				Assert.That (logger.Issues[i].StreamOffset, Is.EqualTo (i * 8 + 2));
				Assert.That (logger.Issues[i].LineNumber, Is.EqualTo (i + 1));
				Assert.That (logger.Issues[i].ColumnNumber, Is.EqualTo (3));
			}
		}

		// Note: The two violations latch independently, so a line that trips both still reports both.
		[Test]
		public void TestValidateInvalidHexSequenceAndSoftBreakOnSameLine ()
		{
			const string text = "=~=\r~\r\n";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (2));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (1));
			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableSoftBreak));
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (4));
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (1));
		}

		[TestCase ("invalid trailing =", MimeComplianceViolation.InvalidQuotedPrintableEncoding)]
		[TestCase ("invalid trailing =A", MimeComplianceViolation.InvalidQuotedPrintableEncoding)]
		[TestCase ("invalid trailing =\r", MimeComplianceViolation.InvalidQuotedPrintableSoftBreak)]
		public void TestValidateInvalidFlush (string text, MimeComplianceViolation violation)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (1));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (violation));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.Length));
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (1));
		}

		static List<(MimeComplianceViolation Violation, long StreamOffset, int LineNumber, int ColumnNumber)> ValidateInChunks (byte[] input, int chunkSize)
		{
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			for (int index = 0; index < input.Length; index += chunkSize)
				validator.Write (input, index, Math.Min (chunkSize, input.Length - index));

			validator.Flush ();

			return logger.Issues.Select (issue => (issue.Violation, issue.StreamOffset, issue.LineNumber, issue.ColumnNumber)).ToList ();
		}

		[Test]
		public void TestValidatePassThroughRunLengths ()
		{
			// Place line breaks, valid and invalid '=' sequences at, before and after the scalar scan length (16)
			// as well as much further into the input so that both the scalar and IndexOfAny() paths are used.
			foreach (var runLength in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 100, 1000 }) {
				var builder = new StringBuilder ();

				for (int i = 0; i < 4; i++) {
					builder.Append ('x', runLength);
					builder.Append ("=C3=A9");
					builder.Append ('y', runLength);
					builder.Append ("=\r\n");
					builder.Append ('z', runLength);
					builder.Append ("\r\n");
					builder.Append ('w', runLength);
					builder.Append ("=~=A~\n");
					builder.Append ('u', runLength);
					builder.Append ("=\rX\r\n");
				}

				builder.Append ('v', runLength);
				builder.Append ('=');

				var input = Encoding.ASCII.GetBytes (builder.ToString ());

				// Feeding the validator 1 byte at a time only ever uses the byte-by-byte scan, so it serves as the reference.
				var expected = ValidateInChunks (input, 1);

				Assert.That (expected, Has.Count.EqualTo (9), $"run length = {runLength}");

				foreach (var chunkSize in new[] { 2, 3, 15, 16, 17, 33, 77, input.Length }) {
					var actual = ValidateInChunks (input, chunkSize);

					Assert.That (actual, Is.EqualTo (expected), $"run length = {runLength}, chunk size = {chunkSize}");
				}
			}
		}

		[Test]
		public void TestValidateLineTrackingAcrossShortAndLongLines ()
		{
			// Short lines are handled entirely by the scalar scan while the long line requires IndexOfAny().
			var longLine = new string ('a', 100);
			var text = "short\nline\n" + longLine + "\n=~\n";
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (1));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidQuotedPrintableEncoding));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.LastIndexOf ('~')));
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (4));
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (2));
		}
	}
}
