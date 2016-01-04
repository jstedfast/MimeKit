//
// ContentDispositionTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
		public void TestMultipleParametersWithIdenticalNames ()
		{
			ContentDisposition disposition;

			const string text1 = "inline;\n filename=\"Filename.doc\";\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc";
			Assert.IsTrue (ContentDisposition.TryParse (text1, out disposition), "Failed to parse first Content-Disposition");
			Assert.AreEqual ("UnicodeFilename.doc", disposition.FileName, "The first filename value does not match.");

			const string text2 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc;\n filename=\"Filename.doc\"";
			Assert.IsTrue (ContentDisposition.TryParse (text2, out disposition), "Failed to parse second Content-Disposition");
			Assert.AreEqual ("UnicodeFilename.doc", disposition.FileName, "The second filename value does not match.");

			const string text3 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename=\"Filename.doc\";\n filename*1*=name.doc";
			Assert.IsTrue (ContentDisposition.TryParse (text3, out disposition), "Failed to parse third Content-Disposition");
			Assert.AreEqual ("UnicodeFilename.doc", disposition.FileName, "The third filename value does not match.");
		}

		[Test]
		public void TestMistakenlyQuotedEncodedParameterValues ()
		{
			const string text = "attachment;\n filename*0*=\"ISO-8859-2''%C8%50%50%20%2D%20%BE%E1%64%6F%73%74%20%6F%20%61%6B%63%65\";\n " +
				"filename*1*=\"%70%74%61%63%69%20%73%6D%6C%6F%75%76%79%20%31%32%2E%31%32%2E\";\n " +
				"filename*2*=\"%64%6F%63\"";
			const string expected = "ČPP - žádost o akceptaci smlouvy 12.12.doc";
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentDisposition disposition;

			Assert.IsTrue (ContentDisposition.TryParse (text, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");
		}

		[Test]
		public void TestUnquotedFilenameParameterValues ()
		{
			const string text = " attachment; filename=Partnership Marketing Agreement\n Form - Mega Brands - Easter Toys - Week 11.pdf";
			const string expected = "Partnership Marketing Agreement Form - Mega Brands - Easter Toys - Week 11.pdf";
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentDisposition disposition;

			Assert.IsTrue (ContentDisposition.TryParse (text, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, 0, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");

			Assert.IsTrue (ContentDisposition.TryParse (buffer, out disposition), "Failed to parse Content-Disposition");
			Assert.AreEqual (expected, disposition.FileName, "The filename value does not match.");
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
	}
}
