//
// UUValidatorTests.cs
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
	public class UUValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new UUValidator (dummyReader, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new UUValidator (dummyReader, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.UUEncode));
		}

		static void AssertValidInput (string text)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new UUValidator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0));

			validator = new UUValidator (reader, 0, 1);

			for (int i = 0; i < rawData.Length; i++)
				validator.Write (rawData, i, 1);

			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0));
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

		static void AssertInvalidInput (string text, MimeComplianceViolationEventArgs[] violations)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new UUValidator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (violations.Length), "ComplianceViolations");
			for (int i = 0; i < violations.Length; i++) {
				Assert.That (reader.ComplianceViolations[i].Violation, Is.EqualTo (violations[i].Violation), $"Violation[{i}]");
				Assert.That (reader.ComplianceViolations[i].StreamOffset, Is.EqualTo (violations[i].StreamOffset), $"StreamOffset[{i}]");
				Assert.That (reader.ComplianceViolations[i].LineNumber, Is.EqualTo (violations[i].LineNumber), $"LineNumber[{i}]");
			}

			reader = new ComplianceMimeReader ();
			validator = new UUValidator (reader, 0, 1);

			for (int i = 0; i < rawData.Length; i++)
				validator.Write (rawData, i, 1);

			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (violations.Length), "1-byte ComplianceViolations");
			for (int i = 0; i < violations.Length; i++) {
				Assert.That (reader.ComplianceViolations[i].Violation, Is.EqualTo (violations[i].Violation), $"1-byte Violation[{i}]");
				Assert.That (reader.ComplianceViolations[i].StreamOffset, Is.EqualTo (violations[i].StreamOffset), $"1-byte StreamOffset[{i}]");
				Assert.That (reader.ComplianceViolations[i].LineNumber, Is.EqualTo (violations[i].LineNumber), $"1-byte LineNumber[{i}]");
			}
		}

		[Test]
		public void TestValidateIncompleteLine ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&E\r\n`\r\nend\r\n";
			// Note: the violation triggers on the '\n' character
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodedLineLength, text.IndexOf ("E\r\n") + 2, 2)
			};

			AssertInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateExtraLineData ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&ENx\r\n`\r\nend\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodedLineLength, text.IndexOf ("x\r\n"), 2)
			};

			AssertInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidDataAfterEnd ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\nmore text...";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf ("more text..."), 5)
			};

			AssertInvalidInput (text, violations);
		}

		[TestCase ("b egin 644 photo.jpg", 1)]
		[TestCase ("be gin 644 photo.jpg", 2)]
		[TestCase ("beg in 644 photo.jpg", 3)]
		[TestCase ("begi n 644 photo.jpg", 4)]
		[TestCase ("beginx 644 photo.jpg", 5)]
		public void TestValidateInvalidBeginLine (string beginLine, long offset)
		{
			string text = beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodePretext, offset, 1),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteUUEncodedContent, text.Length, 5)
			};

			AssertInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidPretext ()
		{
			string text = "this is some random garbage...\r\nbegin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodePretext, 0, 1)
			};

			AssertInvalidInput (text, violations);
		}

		[TestCase ("begin x644 photo.jpg", 6)]
		[TestCase ("begin 6x44 photo.jpg", 7)]
		[TestCase ("begin 64x4 photo.jpg", 8)]
		[TestCase ("begin 644x photo.jpg", 9)]
		[TestCase ("begin 06440 photo.jpg", 10)]
		public void TestValidateInvalidFileMode (string beginLine, long offset)
		{
			string text = beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodeFileMode, offset, 1)
			};

			AssertInvalidInput (text, violations);
		}

		[TestCase ("xnd", 0)]
		[TestCase ("exd", 1)]
		[TestCase ("enx", 2)]
		[TestCase ("endx", 3)]
		public void TestValidateInvalidEndLine (string endLine, int offset)
		{
			string text = $"begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\n{endLine}\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf (endLine) + offset, 4)
			};

			AssertInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidTerminalLine ()
		{
			const string text = "begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`x\r\nend\r\n";
			var violations = new MimeComplianceViolationEventArgs[] {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidUUEncodeEndMarker, text.IndexOf ("`x") + 1, 3)
			};

			AssertInvalidInput (text, violations);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSize (int bufferSize)
		{
			var reader = new ComplianceMimeReader ();

			TestValidator (reader, new UUValidator (reader, 0, 1), "photo.uu", photo_uu, bufferSize);
		}
	}
}
