//
// ContentTypeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

		[Test]
		public void TestClone ()
		{
			var original = new ContentType ("text", "plain") {
				Charset = "iso-8859-1",
				Name = "clone-me.txt",
			};
			var clone = original.Clone ();

			Assert.That (clone.MediaType, Is.EqualTo (original.MediaType), "MediaType");
			Assert.That (clone.MediaSubtype, Is.EqualTo (original.MediaSubtype), "MediaSubtype");
			Assert.That (clone.Parameters.Count, Is.EqualTo (original.Parameters.Count), "Parameters.Count");
			Assert.That (clone.Charset, Is.EqualTo (original.Charset), "Charset");
			Assert.That (clone.Name, Is.EqualTo (original.Name), "Name");
		}

		[Test]
		public void TestChangedEvents ()
		{
			var timestamp = new DateTimeOffset (2022, 9, 9, 7, 41, 23, new TimeSpan (-4, 0, 0));
			var contentType = new ContentType ("text", "plain");
			int changed = 0;

			contentType.Changed += (sender, args) => { changed++; };

			contentType.Name = "filename.txt";
			Assert.That (changed, Is.EqualTo (1), "Setting an initial Name value SHOULD emit the Changed event");
			changed = 0;

			contentType.Name = "filename.txt";
			Assert.That (changed, Is.EqualTo (0), "Setting the same Name value should not emit the Changed event");

			contentType.Name = "filename.pdf";
			Assert.That (changed, Is.EqualTo (1), "Setting a different Name value SHOULD emit the Changed event");
			changed = 0;

			contentType.Name = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the Name SHOULD emit the Changed event");
			changed = 0;

			contentType.Boundary = "=-boundary-marker--";
			Assert.That (changed, Is.EqualTo (1), "Setting an initial Boundary value SHOULD emit the Changed event");
			changed = 0;

			contentType.Boundary = "=-boundary-marker--";
			Assert.That (changed, Is.EqualTo (0), "Setting the same Boundary value should not emit the Changed event");

			contentType.Boundary = "=-boundary-marker-123--";
			Assert.That (changed, Is.EqualTo (1), "Setting a different Boundary value SHOULD emit the Changed event");
			changed = 0;

			contentType.Boundary = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the Boundary SHOULD emit the Changed event");
			changed = 0;

			contentType.Charset = "utf-8";
			Assert.That (changed, Is.EqualTo (1), "Setting an initial Charset value SHOULD emit the Changed event");
			changed = 0;

			contentType.Charset = "utf-8";
			Assert.That (changed, Is.EqualTo (0), "Setting the same Charset value should not emit the Changed event");

			contentType.Charset = "iso-8859-1";
			Assert.That (changed, Is.EqualTo (1), "Setting a different Charset value SHOULD emit the Changed event");
			changed = 0;

			contentType.Charset = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the Charset SHOULD emit the Changed event");
			changed = 0;

			contentType.CharsetEncoding = Encoding.UTF8;
			Assert.That (changed, Is.EqualTo (1), "Setting an initial CharsetEncoding value SHOULD emit the Changed event");
			changed = 0;

			contentType.CharsetEncoding = Encoding.UTF8;
			Assert.That (changed, Is.EqualTo (0), "Setting the same CharsetEncoding value should not emit the Changed event");

			contentType.CharsetEncoding = Encoding.ASCII;
			Assert.That (changed, Is.EqualTo (1), "Setting a different CharsetEncoding value SHOULD emit the Changed event");
			changed = 0;

			contentType.CharsetEncoding = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the CharsetEncoding SHOULD emit the Changed event");
			changed = 0;

			contentType.Format = "flowed";
			Assert.That (changed, Is.EqualTo (1), "Setting an initial Format value SHOULD emit the Changed event");
			changed = 0;

			contentType.Format = "flowed";
			Assert.That (changed, Is.EqualTo (0), "Setting the same Format value should not emit the Changed event");

			contentType.Format = "unknown";
			Assert.That (changed, Is.EqualTo (1), "Setting a different Format value SHOULD emit the Changed event");
			changed = 0;

			contentType.Format = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the Format SHOULD emit the Changed event");
			changed = 0;
		}

		static void AssertParseResults (ContentType type, ContentType expected)
		{
			if (expected == null) {
				Assert.That (type, Is.Null);
				return;
			}

			Assert.That (type.MediaType, Is.EqualTo (expected.MediaType), "MediaType");
			Assert.That (type.MediaSubtype, Is.EqualTo (expected.MediaSubtype), "MediaSubtype");
			Assert.That (type.Parameters.Count, Is.EqualTo (expected.Parameters.Count), "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.That (type.Parameters[i].Name, Is.EqualTo (name));
				Assert.That (type.Parameters[i].Encoding, Is.EqualTo (encoding));
				Assert.That (type.Parameters[i].Value, Is.EqualTo (value));
				Assert.That (type.Parameters.Contains (name), Is.True);
				Assert.That (type.Parameters[name], Is.EqualTo (expected.Parameters[name]));
			}
		}

		static void AssertParse (string text, ContentType expected, bool result = true, int tokenIndex = -1, int errorIndex = -1)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = ParserOptions.Default;
			ContentType type;

			Assert.That (ContentType.TryParse (text, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (options, text, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (buffer, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (options, buffer, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (buffer, 0, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (options, buffer, 0, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (buffer, 0, buffer.Length, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			Assert.That (ContentType.TryParse (options, buffer, 0, buffer.Length, out type), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (type, expected);

			try {
				type = ContentType.Parse (text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (options, text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (options, buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (options, buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				type = ContentType.Parse (options, buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (type, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}
		}

		[Test]
		public void TestSimpleContentType ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain";

			AssertParse (text, expected);
		}

		[Test]
		public void TestSimpleContentTypeWithVendorExtension ()
		{
			var expected = new ContentType ("application", "x-vnd.msdoc");
			const string text = "application/x-vnd.msdoc";

			AssertParse (text, expected);
		}

		[Test]
		public void TestSimpleContentTypeWithParameter ()
		{
			var expected = new ContentType ("multipart", "mixed") { Boundary = "boundary-text" };
			const string text = "multipart/mixed; boundary=\"boundary-text\"";

			AssertParse (text, expected);
		}

		[Test]
		public void TestMultipartParameterExampleFromRfc2231 ()
		{
			const string text = "message/external-body; access-type=URL;\n      URL*0=\"ftp://\";\n      URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
			var expected = new ContentType ("message", "external-body");

			expected.Parameters.Add ("access-type", "URL");
			expected.Parameters.Add ("URL", "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar");

			AssertParse (text, expected);
		}

		[Test]
		public void TestContentTypeWithEmptyParameter ()
		{
			const string text = "multipart/mixed;;\n                Boundary=\"===========================_ _= 1212158(26598)\"";
			var expected = new ContentType ("multipart", "mixed");

			expected.Parameters.Add ("Boundary", "===========================_ _= 1212158(26598)");

			AssertParse (text, expected);
		}

		// Tests the work-around for issue #595
		[Test]
		public void TestContentTypeWithoutSemicolonBetweenParameters ()
		{
			const string text = "application/x-pkcs7-mime;\n name=\"smime.p7m\"\n smime-type=enveloped-data";
			var expected = new ContentType ("application", "x-pkcs7-mime") { Name = "smime.p7m" };
			expected.Parameters.Add ("smime-type", "enveloped-data");

			AssertParse (text, expected, true);
		}

		[Test]
		public void TestContentTypeAndContentTrafserEncodingOnOneLine ()
		{
			const string text = "text/plain; charset = \"iso-8859-1\" Content-Transfer-Encoding: 8bit";
			var expected = new ContentType ("text", "plain");

			// TryParse should "fail", but still produce a usable ContentType.
			// Parse will throw ParseException.
			AssertParse (text, expected, false, 35, 60);
		}

		[Test]
		public void TestEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n      title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add (Encoding.ASCII, "title", "This is ***fun***");

			AssertParse (text, expected);
		}

		[Test]
		public void TestMultipartEncodedParameterExampleFromRfc2231 ()
		{
			const string text = "application/x-stuff;\n    title*1*=us-ascii'en'This%20is%20even%20more%20;\n    title*2*=%2A%2A%2Afun%2A%2A%2A%20;\n    title*3=\"isn't it!\"";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add (Encoding.ASCII, "title", "This is even more ***fun*** isn't it!");

			AssertParse (text, expected);
		}

		[Test]
		public void TestRfc2047EncodedParameter ()
		{
			const string text = "application/x-stuff;\n    title=\"some chinese characters =?utf-8?q?=E4=B8=AD=E6=96=87?= and stuff\"\n";
			var expected = new ContentType ("application", "x-stuff");

			expected.Parameters.Add ("title", "some chinese characters 中文 and stuff");

			AssertParse (text, expected);
		}

		[Test]
		public void TestRfc2047EncodedParameterBig5 ()
		{
			const string text = "application/x-stuff;\n    title=\"some chinese characters =?big5?b?pKSk5Q==?= and stuff\"\n";
			var expected = new ContentType ("application", "x-stuff");
			var big5 = Encoding.GetEncoding ("big5");

			expected.Parameters.Add (big5, "title", "some chinese characters 中文 and stuff");

			AssertParse (text, expected);
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
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

			Assert.That (encoded, Is.EqualTo (expected), $"Encoded Content-Type does not match: {expected}");
		}

		[Test]
		public void TestUnquotedParameter ()
		{
			var expected = new ContentType ("application", "octet-stream");
			const string text = "application/octet-stream; name=Test;";

			expected.Parameters.Add ("name", "Test");

			AssertParse (text, expected);
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
			Assert.That (ContentType.TryParse (options, buffer, out type), Is.False, $"Should not have parsed (strict mode): {text}");
			// however, it should preserve at least the type/subtype info... (I call this a feature!)
			Assert.That (type, Is.Not.Null, "Even though parsing failed, the content type should not be null.");
			Assert.That (type.MediaType, Is.EqualTo ("application"), $"Media type does not match: {text}");
			Assert.That (type.MediaSubtype, Is.EqualTo ("octet-stream"), $"Media subtype does not match: {text}");

			// it *should* pass with the loose parser
			options.ParameterComplianceMode = RfcComplianceMode.Loose;
			Assert.That (ContentType.TryParse (options, buffer, out type), Is.True, $"Failed to parse: {text}");
			Assert.That (type.MediaType, Is.EqualTo ("application"), $"Media type does not match: {text}");
			Assert.That (type.MediaSubtype, Is.EqualTo ("octet-stream"), $"Media subtype does not match: {text}");
			Assert.That (type.Parameters, Is.Not.Null, $"Parameter list is null: {text}");
			Assert.That (type.Parameters.Contains ("name"), Is.True, $"Parameter list does not contain name param: {text}");
			Assert.That (type.Parameters["name"], Is.EqualTo ("Test Name.pdf"), $"name values do not match: {text}");
		}

		[Test]
		public void TestUnquotedBoundaryWithTrailingNewLineAndSpace ()
		{
			const string text = "multipart/mixed;\n boundary=--boundary_0_8ab0e518-760f-4a94-acc0-66f7cdea5c9f\n ";
			var options = ParserOptions.Default.Clone ();
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentType type;

			options.ParameterComplianceMode = RfcComplianceMode.Strict;
			Assert.That (ContentType.TryParse (options, buffer, out type), Is.True, $"Failed to parse: {text}");
			Assert.That (type.MediaType, Is.EqualTo ("multipart"), $"Media type does not match: {text}");
			Assert.That (type.MediaSubtype, Is.EqualTo ("mixed"), $"Media subtype does not match: {text}");
			Assert.That (type.Boundary, Is.EqualTo ("--boundary_0_8ab0e518-760f-4a94-acc0-66f7cdea5c9f"), $"The boundary parameter does not match: {text}");

			options.ParameterComplianceMode = RfcComplianceMode.Loose;
			Assert.That (ContentType.TryParse (options, buffer, out type), Is.True, $"Failed to parse: {text}");
			Assert.That (type.MediaType, Is.EqualTo ("multipart"), $"Media type does not match: {text}");
			Assert.That (type.MediaSubtype, Is.EqualTo ("mixed"), $"Media subtype does not match: {text}");
			Assert.That (type.Boundary, Is.EqualTo ("--boundary_0_8ab0e518-760f-4a94-acc0-66f7cdea5c9f"), $"The boundary parameter does not match: {text}");
		}

		[Test]
		public void TestInternationalParameterValue ()
		{
			const string text = " text/plain; format=flowed; x-eai-please-do-not=\"abstürzen\"";
			ContentType type;

			Assert.That (ContentType.TryParse (text, out type), Is.True);
			Assert.That (type.Parameters["x-eai-please-do-not"], Is.EqualTo ("abstürzen"));
		}

		[Test]
		public void TestMimeTypeWithoutSubtype ()
		{
			const string text = "application-x-gzip; name=document.xml.gz";

			AssertParse (text, null, false, 18, 18);
		}

		[Test]
		public void TestInvalidType ()
		{
			const string text = "åpplication/octet-stream";

			AssertParse (text, null, false, 0, 0);
		}

		[Test]
		public void TestInvalidSubtype ()
		{
			const string text = "application/åtom";

			AssertParse (text, null, false, 12, 12);
		}

		[Test]
		public void TestInvalidDataAfterMimeType ()
		{
			var expected = new ContentType ("application", "octet-stream");
			const string text = "application/octet-stream x";

			AssertParse (text, expected, false, 25, 25);
		}

		[Test]
		public void TestEmptyParameterName ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; =";

			AssertParse (text, expected, false, 12, 12);
		}

		[Test]
		public void TestIncompleteParameterName ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name";

			AssertParse (text, expected, false, 12, 16);
		}

		[Test]
		public void TestIncompleteParameterNameWithStar ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name*";

			AssertParse (text, expected, false, 12, 17);
		}

		[Test]
		public void TestIncompleteParameterNameWithPartId ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name*0";

			AssertParse (text, expected, false, 12, 18);
		}

		[Test]
		public void TestIncompleteParameterNameWithPartIdStar ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name*0*";

			AssertParse (text, expected, false, 12, 19);
		}

		[Test]
		public void TestInvalidParameterNameWithPartId ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name*0*x";

			AssertParse (text, expected, false, 12, 19);
		}

		[Test]
		public void TestInncompleteParameterNameWithPartIdStarEqual ()
		{
			var expected = new ContentType ("text", "plain");
			const string text = "text/plain; name*0*=";

			AssertParse (text, expected, false, 12, 20);
		}

		[Test]
		public void TestProperties ()
		{
			var type = new ContentType ("application", "octet-stream") {
				MediaType = "text"
			};
			Assert.That (type.MediaType, Is.EqualTo ("text"));

			type.MediaSubtype = "plain";
			Assert.That (type.MediaSubtype, Is.EqualTo ("plain"));

			type.Boundary = "--=Boundary=--";
			Assert.That (type.Boundary, Is.EqualTo ("--=Boundary=--"));
			type.Boundary = null;
			Assert.That (type.Boundary, Is.Null);

			type.Format = "flowed";
			Assert.That (type.Format, Is.EqualTo ("flowed"));
			type.Format = null;
			Assert.That (type.Format, Is.Null);

			type.Charset = "iso-8859-1";
			Assert.That (type.Charset, Is.EqualTo ("iso-8859-1"));
			type.Charset = null;
			Assert.That (type.Charset, Is.Null);

			type.Name = "filename.txt";
			Assert.That (type.Name, Is.EqualTo ("filename.txt"));
			type.Name = null;
			Assert.That (type.Name, Is.Null);
		}

		[Test]
		public void TestToString ()
		{
			const string expected = "Content-Type: text/plain; format=\"flowed\"; charset=\"iso-8859-1\"; name=\"filename.txt\"";
			var type = new ContentType ("text", "plain") { Format = "flowed", Charset = "iso-8859-1", Name = "filename.txt" };
			var value = type.ToString ().Replace ("\r\n", "\n");

			Assert.That (value, Is.EqualTo (expected));
		}

		[Test]
		public void TestToStringEncode ()
		{
			const string rfc2231 = "Content-Type: text/plain; format=flowed; charset=utf-8;\n\tname*0*=utf-8''%D0%AD%D1%82%D0%BE%20%D1%80%D1%83%D1%81%D1%81%D0%BA%D0%BE;\n\tname*1*=%D0%B5%20%D0%B8%D0%BC%D1%8F%20%D1%84%D0%B0%D0%B9%D0%BB%D0%B0.txt";
			const string rfc2047 = "Content-Type: text/plain; format=flowed; charset=utf-8;\n\tname=\"=?utf-8?b?0K3RgtC+INGA0YPRgdGB0LrQvtC1INC40LzRjyDRhNCw0LnQu9CwLnR4?=\n\t=?utf-8?q?t?=\"";
			var type = new ContentType ("text", "plain") { Format = "flowed", Charset = "utf-8", Name = "Это русское имя файла.txt" };
			string value;

			value = type.ToString (Encoding.UTF8, true).Replace ("\r\n", "\n");
			Assert.That (value, Is.EqualTo (rfc2231), "Default");

			foreach (var parameter in type.Parameters)
				parameter.EncodingMethod = ParameterEncodingMethod.Rfc2231;

			value = type.ToString (Encoding.UTF8, true).Replace ("\r\n", "\n");
			Assert.That (value, Is.EqualTo (rfc2231), "Rfc2231");

			foreach (var parameter in type.Parameters)
				parameter.EncodingMethod = ParameterEncodingMethod.Rfc2047;

			value = type.ToString (Encoding.UTF8, true).Replace ("\r\n", "\n");
			Assert.That (value, Is.EqualTo (rfc2047), "Rfc2047");
		}

		[Test]
		public void TestParseMultipartMultipartMixed ()
		{
			const string input = "multipart/multipart/mixed; boundary=\"boundary-marker\"\r\n";
			ContentType contentType;

			Assert.That (ContentType.TryParse (input, out contentType), Is.True, "Expected TryParse to succeed.");
			Assert.That (contentType.MediaType, Is.EqualTo ("multipart"), "MediaType");
			Assert.That (contentType.MediaSubtype, Is.EqualTo ("multipart/mixed"), "MediaSubtype");
			Assert.That (contentType.Boundary, Is.EqualTo ("boundary-marker"), "Boundary");
		}
	}
}
