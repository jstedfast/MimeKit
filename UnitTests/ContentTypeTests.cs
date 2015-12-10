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
			const string text = "text/plain";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "text", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "plain", "Media subtype does not match: {0}", text);
		}

		[Test]
		public void TestSimpleContentTypeWithVendorExtension ()
		{
			const string text = "application/x-vnd.msdoc";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-vnd.msdoc", "Media subtype does not match: {0}", text);
		}

		[Test]
		public void TestSimpleContentTypeWithParameter ()
		{
			const string text = "multipart/mixed; boundary=\"boundary-text\"";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "multipart", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "mixed", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("boundary"), "Parameter list does not contain boundary param: {0}", text);
			Assert.AreEqual (type.Parameters["boundary"], "boundary-text", "boundary values do not match: {0}", text);
		}

		[Test]
		public void TestMultipartParameterExampleFromRfc2231 ()
		{
			const string text = "message/external-body; access-type=URL;\n      URL*0=\"ftp://\";\n      URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
			ContentType type;

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
			const string text = "multipart/mixed;;\n                Boundary=\"===========================_ _= 1212158(26598)\"";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "multipart", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "mixed", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("boundary"), "Parameter list does not contain boundary param: {0}", text);
			Assert.AreEqual (type.Parameters["boundary"], "===========================_ _= 1212158(26598)", "boundary values do not match: {0}", text);
		}

		[Test]
		public void TestContentTypeAndContentTrafserEncodingOnOneLine ()
		{
			const string text = "text/plain; charset = \"iso-8859-1\" Content-Transfer-Encoding: 8bit";
			ContentType type;

			Assert.IsFalse (ContentType.TryParse (text, out type), "Content-Type should have failed to parse");
			Assert.IsNotNull (type, "ContentType should not be null");
			Assert.IsTrue (type.IsMimeType ("text", "plain"), "ContenType should match text/plain");
		}

		[Test]
		public void TestEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n      title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "x-stuff", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("title"), "Parameter list does not contain title param: {0}", text);
			Assert.AreEqual (type.Parameters["title"], "This is ***fun***", "title values do not match: {0}", text);
		}

		[Test]
		public void TestMultipartEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n    title*1*=us-ascii'en'This%20is%20even%20more%20;\n    title*2*=%2A%2A%2Afun%2A%2A%2A%20;\n    title*3=\"isn't it!\"";
			ContentType type;

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
			const string text = "application/x-stuff;\n    title=\"some chinese characters =?utf-8?q?=E4=B8=AD=E6=96=87?= and stuff\"\n";
			ContentType type;

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
			const string expected = " text/plain; charset=iso-8859-1;\n\tname*0=\"this is a really really long filename that should force MimeKit to b\";\n\tname*1=\"reak it apart - yay!.html\"\n";
			var format = FormatOptions.Default.Clone ();
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "this is a really really long filename that should force MimeKit to break it apart - yay!.html");

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestBreakingOfLongParamValues2047 ()
		{
			const string expected = " text/plain; charset=iso-8859-1; name=\"=?us-ascii?q?this_is_?=\n\t=?us-ascii?q?a_really_really_long_filename_that_should_force_MimeKit_to_?=\n\t=?us-ascii?q?break_it_apart_-_yay!=2Ehtml?=\"\n";
			var format = FormatOptions.Default.Clone ();
			format.ParameterEncodingMethod = ParameterEncodingMethod.Rfc2047;
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "this is a really really long filename that should force MimeKit to break it apart - yay!.html");

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfParamValues ()
		{
			const string expected = " text/plain; charset=iso-8859-1;\n\tname*=iso-8859-1''Kristoffer%20Br%E5nemyr\n";
			var format = FormatOptions.Default.Clone ();
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "Kristoffer Brånemyr");

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfParamValues2047 ()
		{
			const string expected = " text/plain; charset=iso-8859-1;\n\tname=\"=?iso-8859-1?q?Kristoffer_Br=E5nemyr?=\"\n";
			var format = FormatOptions.Default.Clone ();
			format.ParameterEncodingMethod = ParameterEncodingMethod.Rfc2047;
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "iso-8859-1");
			type.Parameters.Add ("name", "Kristoffer Brånemyr");

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfLongParamValues ()
		{
			const string expected = " text/plain; charset=utf-8;\n\tname*0*=iso-8859-1''%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5;\n\tname*1*=%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5%E5\n";
			var format = FormatOptions.Default.Clone ();
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "utf-8");
			type.Parameters.Add ("name", new string ('å', 40));

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestEncodingOfLongParamValues2047 ()
		{
			const string expected = " text/plain; charset=utf-8; name=\"=?iso-8859-1?b?5eXl5eXl?=\n\t=?iso-8859-1?b?5eXl5eXl5eXl5eXl5eXl5eXl5eXl5eXl5eXl5eXl5eXl5Q==?=\"\n";
			var format = FormatOptions.Default.Clone ();
			format.ParameterEncodingMethod = ParameterEncodingMethod.Rfc2047;
			format.NewLineFormat = NewLineFormat.Unix;

			var type = new ContentType ("text", "plain");
			type.Parameters.Add ("charset", "utf-8");
			type.Parameters.Add ("name", new string ('å', 40));

			var encoded = type.Encode (format, Encoding.UTF8);

			Assert.AreEqual (expected, encoded, "Encoded Content-Type does not match: {0}", expected);
		}

		[Test]
		public void TestUnquotedParameter ()
		{
			const string text = "application/octet-stream; name=Test;";
			ContentType type;

			Assert.IsTrue (ContentType.TryParse (text, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "octet-stream", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("name"), "Parameter list does not contain name param: {0}", text);
			Assert.AreEqual (type.Parameters["name"], "Test", "name values do not match: {0}", text);
		}

		[Test]
		public void TestUnquotedParameterWithSpaces ()
		{
			const string text = "application/octet-stream; name=Test Name.pdf;";
			var options = ParserOptions.Default.Clone ();
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentType type;

			// it should fail using the strict parser...
			options.ParameterComplianceMode = RfcComplianceMode.Strict;
			Assert.IsFalse (ContentType.TryParse (options, buffer, out type), "Should not have parsed (strict mode): {0}", text);
			// however, it should preserve at least the type/subtype info... (I call this a feature!)
			Assert.IsNotNull (type, "Even though parsing failed, the content type should not be null.");
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "octet-stream", "Media subtype does not match: {0}", text);

			// it *should* pass with the loose parser
			options.ParameterComplianceMode = RfcComplianceMode.Loose;
			Assert.IsTrue (ContentType.TryParse (options, buffer, out type), "Failed to parse: {0}", text);
			Assert.AreEqual (type.MediaType, "application", "Media type does not match: {0}", text);
			Assert.AreEqual (type.MediaSubtype, "octet-stream", "Media subtype does not match: {0}", text);
			Assert.IsNotNull (type.Parameters, "Parameter list is null: {0}", text);
			Assert.IsTrue (type.Parameters.Contains ("name"), "Parameter list does not contain name param: {0}", text);
			Assert.AreEqual (type.Parameters["name"], "Test Name.pdf", "name values do not match: {0}", text);
		}
	}
}
