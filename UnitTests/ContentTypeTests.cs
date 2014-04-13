//
// ContentTypeTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
	public class ContentTypeTests
	{
		[Test]
		public void TestSimpleContentType ()
		{
			string text = "text/plain";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "text", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "plain", "Media subtype does not match: {0}", text);
		}

		[Test]
		public void TestSimpleContentTypeWithVendorExtension ()
		{
			string text = "application/x-vnd.msdoc";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-vnd.msdoc", "Media subtype does not match: {0}", text);
		}

		[Test]
		public void TestSimpleContentTypeWithParameter ()
		{
			string text = "multipart/mixed; boundary=\"boundary-text\"";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "multipart", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "mixed", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("boundary"), "Parameter list does not contain boundary param: {0}", text);
			Assert.AreEqual (type.Parameters["boundary"], "boundary-text", "boundary values do not match: {0}", text);
		}

		[Test]
		public void TestMultipartParameterExampleFromRfc2184 ()
		{
			ContentType type;
			string text;

			text = "message/external-body; access-type=URL;\n      URL*0=\"ftp://\";\n      URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "message", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "external-body", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("access-type"), "Parameter list does not contain access-type param: {0}", text);
			Assert.AreEqual (type.Parameters["access-type"], "URL", "access-type values do not match: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("URL"), "Parameter list does not contain URL param: {0}", text);
			Assert.AreEqual (type.Parameters["URL"], "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", "access-type values do not match: {0}", text);
		}

		[Test]
		public void TestContentTypeWithEmptyParameter ()
		{
			ContentType type;
			string text;

			text = "multipart/mixed;;\n                Boundary=\"===========================_ _= 1212158(26598)\"";
			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "multipart", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "mixed", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("boundary"), "Parameter list does not contain boundary param: {0}", text);
			Assert.AreEqual (type.Parameters["boundary"], "===========================_ _= 1212158(26598)", "boundary values do not match: {0}", text);
		}

		[Test]
		public void TestEncodedParameterExampleFromRfc2184 ()
		{
			ContentType type;
			string text;

			text = "application/x-stuff;\n      title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A";
			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-stuff", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("title"), "Parameter list does not contain title param: {0}", text);
			Assert.AreEqual (type.Parameters["title"], "This is ***fun***", "title values do not match: {0}", text);
		}

		[Test]
		public void TestMultipartEncodedParameterExampleFromRfc2184 ()
		{
			ContentType type;
			string text;

			text = "application/x-stuff;\n    title*1*=us-ascii'en'This%20is%20even%20more%20;\n    title*2*=%2A%2A%2Afun%2A%2A%2A%20;\n    title*3=\"isn't it!\"";
			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-stuff", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("title"), "Parameter list does not contain title param: {0}", text);
			Assert.AreEqual (type.Parameters["title"], "This is even more ***fun*** isn't it!", "title values do not match: {0}", text);
		}

		[Test]
		public void TestRfc2047EncodedParameter ()
		{
			ContentType type;
			string text;

			text = "application/x-stuff;\n    title=\"some chinese characters =?utf-8?q?=E4=B8=AD=E6=96=87?= and stuff\"\n";
			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-stuff", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("title"), "Parameter list does not contain title param: {0}", text);
			Assert.AreEqual (type.Parameters["title"], "some chinese characters 中文 and stuff", "title values do not match: {0}", text);
		}

		[Test]
		public void TestBreakingOfLongParamValues ()
		{
			string encoded, expected;
			ContentType type;

			expected = "Content-Type: text/plain; charset=iso-8859-1;\n\tname*0=\"this is a really really long filename that should force MimeKi\";\n\tname*1=\"t to break it apart - yay!.html\"";
			type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "this is a really really long filename that should force MimeKit to break it apart - yay!.html");
			encoded = type.ToString (Encoding.UTF8, true);
			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfParamValues ()
		{
			string encoded, expected;
			ContentType type;

			expected = "Content-Type: text/plain; charset=iso-8859-1;\n\tname*=iso-8859-1''Kristoffer%20Br%E5nemyr";
			type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "Kristoffer Brånemyr");
			encoded = type.ToString (Encoding.UTF8, true);
			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfLongParamValues ()
		{
			string encoded, expected;
			ContentType type;

			expected = "Content-Type: text/plain; charset=utf-8;\n\tname*0*=iso-8859-1''%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5;\n\tname*1*=%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5;\n\tname*2*=%E5%E5%E5%E5";
			type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "utf-8");
			type.Parameters.Add ("name", new string ('å', 40));
			encoded = type.ToString (Encoding.UTF8, true);
			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

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
			ContentDisposition disposition;

			Assert.IsTrue (ContentDisposition.TryParse (text, out disposition), "Failed to parse fourth Content-Disposition");
			Assert.AreEqual ("ČPP - žádost o akceptaci smlouvy 12.12.doc", disposition.FileName, "The fourth filename value does not match.");
		}
	}
}
