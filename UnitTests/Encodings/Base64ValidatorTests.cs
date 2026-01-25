//
// Base64ValidatorTests.cs
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
	public class Base64ValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new Base64Validator (dummyReader, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new Base64Validator (dummyReader, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.Base64));
		}

		[TestCase ("VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==")]
		[TestCase ("VGhpcyBpcyB0aGUgcGxhaW4g\t dGV4dCBtZXNzYWdlIQ==")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCBvbmNlLi4=")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCB0d2ljZQ==")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggd2lsbCBub3QgYmUgcGFkZGVk")]
		public void TestValidateValidInput (string text)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new Base64Validator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0));
		}

		public void TestValidateInvalidInput (string text, List<MimeComplianceViolationEventArgs> violations)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var reader = new ComplianceMimeReader ();
			var validator = new Base64Validator (reader, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			AssertInvalidInput (reader, violations);
		}

		[Test]
		public void TestValidateInvalidInput_MultipleInvalidCharacters ()
		{
			const string text = " &% VGhp\r\ncyBp\r\ncyB0aGUgcGxhaW4g  \tdGV4dCBtZ?!XNzY*WdlIQ==";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Character, 1, 1),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Character, 2, 1),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Character, 44, 3),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Character, 45, 3),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Character, 50, 3),
			};

			TestValidateInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding1 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ===";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 44, 1),

				// TODO: should this really get emitted?
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteBase64Quantum, 45, 1)
			};

			TestValidateInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding2 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ====";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 44, 1),
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 45, 1),

				// TODO: should this really get emitted?
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteBase64Quantum, 46, 1)
			};

			TestValidateInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding3 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=====";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 44, 1),
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 45, 1),
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.InvalidBase64Padding, 46, 1),

				// TODO: should this really get emitted?
				//new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteBase64Quantum, 47, 1)
			};

			TestValidateInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidInput_CharactersAfterPadding ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGF5bG9hZCBvZiB0aGUgZmlyc3QgYmFzZTY0LWVuY29kZWQgYmxvY2sgb2Yg\r\ndGV4dC4=\r\nQW5kIHRoaXMgaXMgdGhlIHBheWxvYWQgb2YgdGhlIHNlY29uZCBiYXNlNjQtZW5jb2RlZCBibG9j\r\nayBvZiB0ZXh0Lg==\r\n";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.Base64CharactersAfterPadding, text.IndexOf ('=') + 3, 3),
			};

			TestValidateInvalidInput (text, violations);
		}

		[Test]
		public void TestValidateInvalidInput_IncompleteQuantumAfterPaddingStart ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=\r\n=";
			var violations = new List<MimeComplianceViolationEventArgs> {
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteBase64Quantum, 44, 1),
				new MimeComplianceViolationEventArgs (MimeComplianceViolation.IncompleteBase64Quantum, 46, 2)
			};

			TestValidateInvalidInput (text, violations);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSize (int bufferSize)
		{
			var reader = new ComplianceMimeReader ();

			TestValidator (reader, new Base64Validator (reader, 0, 1), "photo.b64", photo_b64, bufferSize);
		}
	}
}
