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
			AssertArgumentExceptions (new UUValidator ());
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new UUValidator ();

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.UUEncode));
		}

		[TestCase ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n")]
		[TestCase ("   \t\t    \r\n\t\t    \t\r\nbegin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n")]
		public void TestValidateValidInput (string text)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.True);
		}

		[TestCase ("begin 644 photo.jpg")]
		[TestCase ("begin 0644 photo.jpg")]
		[TestCase ("\r\nbegin 0644 photo.jpg")]
		[TestCase (" \r\nbegin 0644 photo.jpg")]
		public void TestValidateValidBeginLine (string beginLine)
		{
			var rawData = Encoding.ASCII.GetBytes (beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.True);
		}

		[Test]
		public void TestValidateIncompleteLine ()
		{
			var rawData = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&E\r\n`\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[Test]
		public void TestValidateExtraLineData ()
		{
			var rawData = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&ENx\r\n`\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[Test]
		public void TestValidateInvalidDataAfterEnd ()
		{
			var rawData = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\nmore text...");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[TestCase ("b egin 644 photo.jpg")]
		[TestCase ("be gin 644 photo.jpg")]
		[TestCase ("beg in 644 photo.jpg")]
		[TestCase ("begi n 644 photo.jpg")]
		[TestCase ("beginx 644 photo.jpg")]
		[TestCase ("begin x644 photo.jpg")]
		[TestCase ("begin x644 photo.jpg")]
		[TestCase ("begin 6x44 photo.jpg")]
		[TestCase ("begin 64x4 photo.jpg")]
		[TestCase ("begin 644x photo.jpg")]
		[TestCase ("begin 06440 photo.jpg")]
		public void TestValidateInvalidBeginLine (string beginLine)
		{
			var rawData = Encoding.ASCII.GetBytes (beginLine + "\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[TestCase ("xnd")]
		[TestCase ("exd")]
		[TestCase ("enx")]
		[TestCase ("endx")]
		public void TestValidateInvalidEndLine (string endLine)
		{
			var rawData = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\n" + endLine);
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[Test]
		public void TestValidateInvalidTerminalLine ()
		{
			var rawData = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`x\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[TestCase ("abc")]
		public void TestValidateInvalidPrefix (string prefix)
		{
			var rawData = Encoding.ASCII.GetBytes (prefix + "\r\nbegin 644 photo.jpg\r\nM_]C_X``02D9)1@`!`0$`2`!(``#_X@Q824-#7U!23T9)3$4``0$```Q(3&EN\r\n`\r\nend\r\n");
			var validator = new UUValidator ();

			validator.Validate (rawData, 0, rawData.Length);

			Assert.That (validator.Complete (), Is.False);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSize (int bufferSize)
		{
			TestValidator (new UUValidator (), "photo.uu", photo_uu, bufferSize);
		}
	}
}
