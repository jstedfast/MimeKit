//
// ContentTypeTests.cs
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
	public class ContentTypeTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var type = new ContentType ("text", "plain");

			Assert.Throws<ArgumentNullException> (() => type.MediaType = null);
			Assert.Throws<ArgumentNullException> (() => type.MediaSubtype = null);

			Assert.Throws<ArgumentNullException> (() => type.IsMimeType (null, "plain"));
			Assert.Throws<ArgumentNullException> (() => type.IsMimeType ("text", null));

			Assert.Throws<ArgumentNullException> (() => type.ToString (null, true));
			Assert.Throws<ArgumentNullException> (() => type.ToString (null, Encoding.UTF8, true));
			Assert.Throws<ArgumentNullException> (() => type.ToString (FormatOptions.Default, null, true));
		}

		static void AssertTryParse (string text, ContentType expected, bool result = true)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = ParserOptions.Default;
			ContentType type;

			Assert.AreEqual (result, ContentType.TryParse (text, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (options, text, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (buffer, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (options, buffer, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (buffer, 0, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (options, buffer, 0, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (buffer, 0, buffer.Length, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}

			Assert.AreEqual (result, ContentType.TryParse (options, buffer, 0, buffer.Length, out type), "Unexpected result for TryParse: {0}", text);
			Assert.AreEqual (expected.MediaType, type.MediaType, "MediaType");
			Assert.AreEqual (expected.MediaSubtype, type.MediaSubtype, "MediaSubtype");
			Assert.AreEqual (expected.Parameters.Count, type.Parameters.Count, "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.AreEqual (name, type.Parameters[i].Name);
				Assert.AreEqual (encoding, type.Parameters[i].Encoding);
				Assert.AreEqual (value, type.Parameters[i].Value);
				Assert.IsTrue (type.Parameters.Contains (name));
				Assert.AreEqual (expected.Parameters[name], type.Parameters[name]);
			}
		}

		[Test]
		public void TestSimpleContentType ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain";

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestSimpleContentTypeWithVendorExtension ()
		{
			var expected = new ContentType ("application", "x-vnd.msdoc");
			const string text = "application/x-vnd.msdoc";

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestSimpleContentTypeWithParameter ()
		{
			var expected = new ContentType ("multipart", "mixed") { Boundary = "boundary-text" };
			const string text = "multipart/mixed; boundary=\"boundary-text\"";

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestMultipartParameterExampleFromRfc2231 ()
		{
			const string text = "message/external-body; access-type=URL;\n      URL*0=\"ftp://\";\n      URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
			var expected = new ContentType ("message", "external-body");

			expected.Parameters.Add ("access-type", "URL");
			expected.Parameters.Add ("URL", "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar");

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestContentTypeWithEmptyParameter ()
		{
			const string text = "multipart/mixed;;\n                Boundary=\"===========================_ _= 1212158(26598)\"";
			var expected = new ContentType ("multipart", "mixed");

			expected.Parameters.Add ("Boundary", "===========================_ _= 1212158(26598)");

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestContentTypeAndContentTrafserEncodingOnOneLine ()
		{
			const string text = "text/plain; charset = \"iso-8859-1\" Content-Transfer-Encoding: 8bit";
			var expected = new ContentType ("text", "plain");

			// TryParse should "fail", but still produce a usable ContentType
			AssertTryParse (text, expected, false);
		}

		[Test]
		public void TestEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n      title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add (Encoding.ASCII, "title", "This is ***fun***");

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestMultipartEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n    title*1*=us-ascii'en'This%20is%20even%20more%20;\n    title*2*=%2A%2A%2Afun%2A%2A%2A%20;\n    title*3=\"isn't it!\"";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add (Encoding.ASCII, "title", "This is even more ***fun*** isn't it!");

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestRfc2047EncodedParameter ()
		{
			const string text = "application/x-stuff;\n    title=\"some chinese characters =?utf-8?q?=E4=B8=AD=E6=96=87?= and stuff\"\n";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add ("title", "some chinese characters 中文 and stuff");

			AssertTryParse (text, expected);
		}

		[Test]
		public void TestRfc2047EncodedParameterBig5 ()
		{
			const string text = "application/x-stuff;\n    title=\"some chinese characters =?big5?b?pKSk5Q==?= and stuff\"\n";
			var expected = new ContentType ("application", "x-stuff");
			var big5 = Encoding.GetEncoding ("big5");

			expected.Parameters.Add (big5, "title", "some chinese characters 中文 and stuff");

			AssertTryParse (text, expected);
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

		[Test]
		public void TestMimeTypeWithoutSubtype ()
		{
			const string text = "application-x-gzip; name=document.xml.gz";
			ContentType type;

			// this should fail due to the missing subtype
			Assert.IsFalse (ContentType.TryParse (text, out type), "Should not have parsed: {0}", text);
			Assert.IsNull (type, "The content type should be null.");
		}
	}
}
