//
// UUValidatorTests.cs
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
	public class UUValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new UUValidator (nullComplianceLogger, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new UUValidator (nullComplianceLogger, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.UUEncode));
		}

		static void AssertValidInput (string text)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new UUValidator (logger, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (0));

			validator = new UUValidator (logger, 0, 1);

			for (int i = 0; i < rawData.Length; i++)
				validator.Write (rawData, i, 1);

			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (0));
		}

		[TestCase ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n")]
		[TestCase ("   \t\t    \r\n\t\t    \t\r\nbegin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n")]
		public void TestValidateValidInput (string text)
		{
			AssertValidInput (text);
		}

		[TestCase ("begin 644 photo.jpg")]
		[TestCase ("begin 0644 photo.jpg")]
		[TestCase ("\r\nbegin 0644 photo.jpg")]
		[TestCase (" \r\nbegin 0644 photo.jpg")]
		public void TestValidateValidBeginLine (string beginLine)
		{
			string text = beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";

			AssertValidInput (text);
		}

		static void AssertInvalidInput (string text, MimeComplianceIssue[] issues)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new UUValidator (logger, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (issues.Length), "ComplianceViolations");
			for (int i = 0; i < issues.Length; i++) {
				Assert.That (logger.Issues[i].Violation, Is.EqualTo (issues[i].Violation), $"Violation[{i}]");
				Assert.That (logger.Issues[i].StreamOffset, Is.EqualTo (issues[i].StreamOffset), $"StreamOffset[{i}]");
				Assert.That (logger.Issues[i].LineNumber, Is.EqualTo (issues[i].LineNumber), $"LineNumber[{i}]");
			}

			logger.Issues.Clear ();
			validator = new UUValidator (logger, 0, 1);

			for (int i = 0; i < rawData.Length; i++)
				validator.Write (rawData, i, 1);

			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (issues.Length), "1-byte ComplianceViolations");
			for (int i = 0; i < issues.Length; i++) {
				Assert.That (logger.Issues[i].Violation, Is.EqualTo (issues[i].Violation), $"1-byte Violation[{i}]");
				Assert.That (logger.Issues[i].StreamOffset, Is.EqualTo (issues[i].StreamOffset), $"1-byte StreamOffset[{i}]");
				Assert.That (logger.Issues[i].LineNumber, Is.EqualTo (issues[i].LineNumber), $"1-byte LineNumber[{i}]");
			}
		}

		[Test]
		public void TestValidateIncompleteLine ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&E\r\n`\r\nend\r\n";
			// Note: the violation triggers on the '\n' character
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodedLineLength, text.IndexOf ("E\r\n") + 2, 2, 61)
			};

			AssertInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateExtraLineData ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&ENx\r\n`\r\nend\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodedLineLength, text.IndexOf ("x\r\n"), 2, 61)
			};

			AssertInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidDataAfterEnd ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\nmore text...";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf ("more text..."), 5, 1)
			};

			AssertInvalidInput (text, issues);
		}

		[TestCase ("b egin 644 photo.jpg", 1)]
		[TestCase ("be gin 644 photo.jpg", 2)]
		[TestCase ("beg in 644 photo.jpg", 3)]
		[TestCase ("begi n 644 photo.jpg", 4)]
		[TestCase ("beginx 644 photo.jpg", 5)]
		public void TestValidateInvalidBeginLine (string beginLine, int offset)
		{
			string text = beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodePretext, offset, 1, offset + 1),
				new MimeComplianceIssue (MimeComplianceViolation.IncompleteUUEncodedContent, text.Length, 5, 1)
			};

			AssertInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidPretext ()
		{
			string text = "this is some random garbage...\r\nbegin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodePretext, 0, 1, 1)
			};

			AssertInvalidInput (text, issues);
		}

		[TestCase ("begin x644 photo.jpg", 6)]
		[TestCase ("begin 6x44 photo.jpg", 7)]
		[TestCase ("begin 64x4 photo.jpg", 8)]
		[TestCase ("begin 644x photo.jpg", 9)]
		[TestCase ("begin 06440 photo.jpg", 10)]
		public void TestValidateInvalidFileMode (string beginLine, int offset)
		{
			string text = beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodeFileMode, offset, 1, offset + 1)
			};

			AssertInvalidInput (text, issues);
		}

		[TestCase ("xnd", 0)]
		[TestCase ("exd", 1)]
		[TestCase ("enx", 2)]
		[TestCase ("endx", 3)]
		public void TestValidateInvalidEndLine (string endLine, int offset)
		{
			string text = $"begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\n{endLine}\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf (endLine) + offset, 4, offset + 1)
			};

			AssertInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidTerminalLine ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`x\r\nend\r\n";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf ("`x") + 1, 3, 2)
			};

			AssertInvalidInput (text, issues);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSize (int bufferSize)
		{
			var logger = new TestMimeComplianceLogger ();

			TestValidator (logger, new UUValidator (logger, 0, 1), "photo.uu", photo_uu, bufferSize);
		}
	}
}
