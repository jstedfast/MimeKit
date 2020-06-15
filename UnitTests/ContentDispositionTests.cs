//
// ContentDispositionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using System;
using System.Text;
using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ContentDispositionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var disposition = new ContentDisposition ();

			Assert.Throws<ArgumentNullException> (() => disposition.Disposition = null, "Setting the disposition to null value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = string.Empty, "Setting the disposition to an empty value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = "žádost", "Setting the disposition to a non-ascii value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = "two atoms", "Setting the disposition to multiple atom tokens should throw.");

			Assert.Throws<ArgumentNullException> (() => disposition.ToString (null, Encoding.UTF8, true));
			Assert.Throws<ArgumentNullException> (() => disposition.ToString (FormatOptions.Default, null, true));
		}

		static void AssertParseResults (ContentDisposition disposition, ContentDisposition expected)
		{
			if (expected == null) {
				Assert.IsNull (disposition);
				return;
			}

			Assert.AreEqual (expected.Disposition, disposition.Disposition, "Disposition");
			Assert.AreEqual (expected.Parameters.Count, disposition.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, disposition.Parameters[i].Name);
				Assert.AreEqual (encoding.EncodingName, disposition.Parameters[i].Encoding.EncodingName);
				Assert.AreEqual (value, disposition.Parameters[i].Value);
				Assert.IsTrue (disposition.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], disposition.Parameters[name]);
			}
		}

		static void AssertParse (string text, ContentDisposition expected, bool result = true, int tokenIndex = -1, int errorIndex = -1)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = ParserOptions.Default;
			ContentDisposition disposition;

			Assert.AreEqual (result, ContentDisposition.TryParse (text, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (options, text, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (buffer, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (options, buffer, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (buffer, 0, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (options, buffer, 0, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			Assert.AreEqual (result, ContentDisposition.TryParse (options, buffer, 0, buffer.Length, out disposition), "Unexpected result for TryParse: {0}", text);
			AssertParseResults (disposition, expected);

			try {
				disposition = ContentDisposition.Parse (text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (options, text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ("Parsing \"{0}\" should have failed.", text);
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "Unexpected token index");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception: {0}", e);
			}
		}

		[Test]
		public void TestEmptyValue ()
		{
			const string text = " ";

			AssertParse (text, null, false, 1, 1);
		}

		[Test]
		public void TestMultipleParametersWithIdenticalNames ()
		{
			const string text1 = "inline;\n filename=\"Filename.doc\";\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc";
			const string text2 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc;\n filename=\"Filename.doc\"";
			const string text3 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename=\"Filename.doc\";\n filename*1*=name.doc";
			var expected = new ContentDisposition ("inline");

			expected.Parameters.Add ("filename", "UnicodeFilename.doc");

			AssertParse (text1, expected);
			AssertParse (text2, expected);
			AssertParse (text3, expected);
		}

		[Test]
		public void TestNonExistentDispositionValueWithParameterValues ()
		{
			const string text = " ; filename=\"test.txt\"";
			const string filename = "test.txt";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected, true, 1, 1);
		}

		[Test]
		public void TestMistakenlyQuotedDispositionValue ()
		{
			const string text = "\"inline\"; filename=\"test.txt\"";
			const string filename = "test.txt";
			var expected = new ContentDisposition ("inline");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected, true, 0, 0);
		}

		[Test]
		public void TestMistakenlyQuotedEncodedParameterValues ()
		{
			const string text = "attachment;\n filename*0*=\"ISO-8859-2''%C8%50%50%20%2D%20%BE%E1%64%6F%73%74%20%6F%20%61%6B%63%65\";\n " +
				"filename*1*=\"%70%74%61%63%69%20%73%6D%6C%6F%75%76%79%20%31%32%2E%31%32%2E\";\n " +
				"filename*2*=\"%64%6F%63\"";
			const string filename = "ČPP - žádost o akceptaci smlouvy 12.12.doc";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add (Encoding.GetEncoding ("ISO-8859-2"), "filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestUnquotedFilenameParameterValues ()
		{
			const string text = " attachment; filename=Partnership Marketing Agreement\n Form - Mega Brands - Easter Toys - Week 11.pdf";
			const string filename = "Partnership Marketing Agreement Form - Mega Brands - Easter Toys - Week 11.pdf";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestInvalidDisposition ()
		{
			const string text = "\attachment";

			AssertParse (text, null, false, 0, 0);
		}

		[Test]
		public void TestInvalidDataAfterMDisposition ()
		{
			var expected = new ContentDisposition ("attachment");
			const string text = "attachment x";

			// TryParse will return false but will have a value to use
			AssertParse (text, expected, false, 11, 11);
		}

		[Test]
		public void TestChineseFilename ()
		{
			const string expected = " attachment;\n\tfilename*=gb18030''%B2%E2%CA%D4%CE%C4%B1%BE.txt\n";
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			disposition.Parameters.Add ("GB18030", "filename", "测试文本.txt");

			var format = FormatOptions.Default.Clone ();
			format.NewLineFormat = NewLineFormat.Unix;

			var encoded = disposition.Encode (format, Encoding.UTF8);
			Parameter param;

			Assert.AreEqual (expected, encoded, "The encoded Chinese filename parameter does not match the expected value.");
			Assert.IsTrue (ContentDisposition.TryParse (encoded, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("测试文本.txt", disposition.FileName, "The decoded Chinese filename does not match.");
			Assert.IsTrue (disposition.Parameters.TryGetValue ("filename", out param), "Failed to locate filename parameter.");
			Assert.AreEqual ("GB18030", param.Encoding.HeaderName, "The filename encoding did not match.");
		}

		[Test]
		public void TestChineseFilename2047 ()
		{
			const string expected = " attachment; filename=\"=?gb18030?b?suLK1M7Esb4udHh0?=\"\n";
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			disposition.Parameters.Add ("GB18030", "filename", "测试文本.txt");

			var format = FormatOptions.Default.Clone ();
			format.ParameterEncodingMethod = ParameterEncodingMethod.Rfc2047;
			format.NewLineFormat = NewLineFormat.Unix;

			var encoded = disposition.Encode (format, Encoding.UTF8);
			Parameter param;

			Assert.AreEqual (expected, encoded, "The encoded Chinese filename parameter does not match the expected value.");
			Assert.IsTrue (ContentDisposition.TryParse (encoded, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("测试文本.txt", disposition.FileName, "The decoded Chinese filename does not match.");
			Assert.IsTrue (disposition.Parameters.TryGetValue ("filename", out param), "Failed to locate filename parameter.");
			Assert.AreEqual ("GB18030", param.Encoding.HeaderName, "The filename encoding did not match.");
		}

		[Test]
		public void TestIssue239 ()
		{
			const string text = " attachment; size=1049971;\n\tfilename*=\"utf-8''SBD%20%C5%A0kodov%C3%A1k%2Ejpg\"";
			const string filename = "SBD Škodovák.jpg";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("size", "1049971");
			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestFormData ()
		{
			const string text = "form-data; filename=\"form.txt\"";
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentDisposition disposition;

			Assert.IsTrue (ContentDisposition.TryParse (text, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("form-data", disposition.Disposition, "The disposition values do not match.");
			Assert.AreEqual ("form.txt", disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("form-data", disposition.Disposition, "The disposition values do not match.");
			Assert.AreEqual ("form.txt", disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("form-data", disposition.Disposition, "The disposition values do not match.");
			Assert.AreEqual ("form.txt", disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual ("form-data", disposition.Disposition, "The disposition values do not match.");
			Assert.AreEqual ("form.txt", disposition.FileName, "The filename value does not match.");
		}

		[Test]
		public void TestDispositionParameters ()
		{
			const string expected = "Content-Disposition: attachment; filename=document.doc;\n" +
				"\tcreation-date=\"Sat, 04 Jan 1997 15:22:17 -0400\";\n" +
				"\tmodification-date=\"Thu, 04 Jan 2007 15:22:17 -0400\";\n" +
				"\tread-date=\"Wed, 04 Jan 2012 15:22:17 -0400\"; size=37001";
			var ctime = new DateTimeOffset (1997, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var mtime = new DateTimeOffset (2007, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var atime = new DateTimeOffset (2012, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var disposition = new ContentDisposition ();
			var format = FormatOptions.Default.Clone ();
			const long size = 37001;
			Parameter param;
			string encoded;

			format.NewLineFormat = NewLineFormat.Unix;

			Assert.AreEqual (ContentDisposition.Attachment, disposition.Disposition, "The disposition should be 'attachment'.");
			Assert.IsTrue (disposition.IsAttachment, "IsAttachment should be true by default.");

			Assert.IsNull (disposition.FileName, "The filename should default to null.");
			Assert.IsNull (disposition.CreationDate, "The creation-date should default to null.");
			Assert.IsNull (disposition.ModificationDate, "The modification-date should default to null.");
			Assert.IsNull (disposition.ReadDate, "The read-date should default to null.");
			Assert.IsNull (disposition.Size, "The size should default to null.");

			disposition.FileName = "document.doc";
			disposition.CreationDate = ctime;
			disposition.ModificationDate = mtime;
			disposition.ReadDate = atime;
			disposition.Size = size;

			encoded = disposition.ToString (format, Encoding.UTF8, true);

			Assert.AreEqual (expected, encoded, "The encoded Content-Disposition does not match.");

			disposition = ContentDisposition.Parse (encoded.Substring ("Content-Disposition:".Length));

			Assert.AreEqual ("document.doc", disposition.FileName, "The filename parameter does not match.");
			Assert.AreEqual (ctime, disposition.CreationDate, "The creation-date parameter does not match.");
			Assert.AreEqual (mtime, disposition.ModificationDate, "The modification-date parameter does not match.");
			Assert.AreEqual (atime, disposition.ReadDate, "The read-date parameter does not match.");
			Assert.AreEqual (size, disposition.Size, "The size parameter does not match.");

			disposition.CreationDate = null;
			Assert.IsFalse (disposition.Parameters.TryGetValue ("creation-date", out param), "The creation-date parameter should have been removed.");

			disposition.ModificationDate = null;
			Assert.IsFalse (disposition.Parameters.TryGetValue ("modification-date", out param), "The modification-date parameter should have been removed.");

			disposition.ReadDate = null;
			Assert.IsFalse (disposition.Parameters.TryGetValue ("read-date", out param), "The read-date parameter should have been removed.");

			disposition.FileName = null;
			Assert.IsFalse (disposition.Parameters.TryGetValue ("filename", out param), "The filename parameter should have been removed.");

			disposition.Size = null;
			Assert.IsFalse (disposition.Parameters.TryGetValue ("size", out param), "The size parameter should have been removed.");

			disposition.IsAttachment = false;
			Assert.AreEqual (ContentDisposition.Inline, disposition.Disposition, "The disposition should be 'inline'.");
			Assert.IsFalse (disposition.IsAttachment, "IsAttachment should be false.");
		}
	}
}
