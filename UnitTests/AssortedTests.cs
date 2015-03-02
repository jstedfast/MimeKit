//
// AssortedTests.cs
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
using System.Linq;
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class AssortedTests
	{
		[Test]
		public void TestMultiLinePreamble ()
		{
			var multipart = new Multipart ("alternative");
			var multiline = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" + 
			                "All of the parts of this message are identical, however they've been encoded " +
			                "for transport using different methods.\n";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" +
			               "All of the parts of this message are identical, however they've been encoded\n" +
			               "for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Preamble = multiline;

			Assert.AreEqual (expected, multipart.Preamble);
		}

		[Test]
		public void TestLongPreamble ()
		{
			var multipart = new Multipart ("alternative");
			var multiline = "This is a part in a (multipart) message generated with the MimeKit library. " + 
			                "All of the parts of this message are identical, however they've been encoded " +
			                "for transport using different methods.";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n" +
			               "All of the parts of this message are identical, however they've been encoded\n" +
			               "for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Preamble = multiline;

			Assert.AreEqual (expected, multipart.Preamble);
		}

		[Test]
		public void TestParsingObsoleteInReplyToSyntax ()
		{
			var obsolete = "Joe Sixpack's message sent on Mon, 17 Jan 1994 11:14:55 -0500 <some.message.id.1@some.domain>";
			var msgid = MimeUtils.EnumerateReferences (obsolete).FirstOrDefault ();

			Assert.IsNotNull (msgid, "The parsed msgid token should not be null");
			Assert.AreEqual ("some.message.id.1@some.domain", msgid, "The parsed msgid does not match");

			obsolete = "<some.message.id.2@some.domain> as sent on Mon, 17 Jan 1994 11:14:55 -0500";
			msgid = MimeUtils.EnumerateReferences (obsolete).FirstOrDefault ();

			Assert.IsNotNull (msgid, "The parsed msgid token should not be null");
			Assert.AreEqual ("some.message.id.2@some.domain", msgid, "The parsed msgid does not match");
		}

		[Test]
		public void TestSimpleRfc2047QEncodedPhrase ()
		{
			var options = ParserOptions.Default.Clone ();
			var input = "=?iso-8859-1?q?hola?=";
			string actual;

			options.Rfc2047ComplianceMode = RfcComplianceMode.Strict;
			actual = Rfc2047.DecodePhrase (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual);

			options.Rfc2047ComplianceMode = RfcComplianceMode.Loose;
			actual = Rfc2047.DecodePhrase (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual, "Unexpected result when workarounds enabled.");
		}

		[Test]
		public void TestSimpleRfc2047BEcnodedPhrase ()
		{
			var options = ParserOptions.Default.Clone ();
			var input = "=?iso-8859-1?B?aG9sYQ==?=";
			string actual;

			options.Rfc2047ComplianceMode = RfcComplianceMode.Strict;
			actual = Rfc2047.DecodePhrase (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual);

			options.Rfc2047ComplianceMode = RfcComplianceMode.Loose;
			actual = Rfc2047.DecodePhrase (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual, "Unexpected result when workarounds enabled.");
		}

		[Test]
		public void TestSimpleRfc2047QEncodedText ()
		{
			var options = ParserOptions.Default.Clone ();
			var input = "=?iso-8859-1?q?hola?=";
			string actual;

			options.Rfc2047ComplianceMode = RfcComplianceMode.Strict;
			actual = Rfc2047.DecodeText (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual);

			options.Rfc2047ComplianceMode = RfcComplianceMode.Loose;
			actual = Rfc2047.DecodeText (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual, "Unexpected result when workarounds enabled.");
		}

		[Test]
		public void TestSimpleRfc2047BEcnodedText ()
		{
			var options = ParserOptions.Default.Clone ();
			var input = "=?iso-8859-1?B?aG9sYQ==?=";
			string actual;

			options.Rfc2047ComplianceMode = RfcComplianceMode.Strict;
			actual = Rfc2047.DecodeText (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual);

			options.Rfc2047ComplianceMode = RfcComplianceMode.Loose;
			actual = Rfc2047.DecodeText (options, Encoding.ASCII.GetBytes (input));
			Assert.AreEqual ("hola", actual, "Unexpected result when workarounds enabled.");
		}

		[Test]
		public void TestISO885913Rfc2047BEcnodedText ()
		{
			var options = ParserOptions.Default.Clone ();
			var input = "=?ISO-8859-13?Q?jaut=E2jums_=EE=EE=EE=FB=FB=EC=F0?=";
			string actual;

			options.Rfc2047ComplianceMode = RfcComplianceMode.Strict;
			actual = Rfc2047.DecodeText (options, Encoding.ASCII.GetBytes (input));


		}


		[Test]
		public void TestRfc2047DecodeInvalidMultibyteBreak ()
		{
			const string japanese = "狂ったこの世で狂うなら気は確かだ。";
			var utf8 = Encoding.UTF8.GetBytes (japanese);
			var builder = new StringBuilder ();

			for (int wordBreak = (utf8.Length / 2) - 5; wordBreak < (utf8.Length / 2) + 5; wordBreak++) {
				builder.Append ("=?utf-8?b?").Append (Convert.ToBase64String (utf8, 0, wordBreak)).Append ("?= ");
				builder.Append ("=?utf-8?b?").Append (Convert.ToBase64String (utf8, wordBreak, utf8.Length - wordBreak)).Append ("?=");

				var decoded = Rfc2047.DecodeText (Encoding.ASCII.GetBytes (builder.ToString ()));

				Assert.AreEqual (japanese, decoded, "Decoded text did not match the original.");

				builder.Clear ();
			}
		}

		[Test]
		public void TestRfc2047DecodeInvalidPayloadBreak ()
		{
			const string japanese = "狂ったこの世で狂うなら気は確かだ。";
			var utf8 = Encoding.UTF8.GetBytes (japanese);
			var base64 = Convert.ToBase64String (utf8);
			var builder = new StringBuilder ();

			builder.Append ("=?utf-8?b?").Append (base64.Substring (0, base64.Length - 6)).Append ("?= ");
			builder.Append ("=?utf-8?b?").Append (base64.Substring (base64.Length - 6)).Append ("?=");

			var decoded = Rfc2047.DecodeText (Encoding.ASCII.GetBytes (builder.ToString ()));

			Assert.AreEqual (japanese, decoded, "Decoded text did not match the original.");
		}

		[Test]
		public void TestDecodeInvalidCharset ()
		{
			const string xunknown = "=?x-unknown?B?aG9sYQ==?=";
			const string cp1260 = "=?cp1260?B?aG9sYQ==?=";
			string actual;

			// Note: we won't be able to get a codepage for x-unknown.
			actual = Rfc2047.DecodeText (Encoding.ASCII.GetBytes (xunknown));
			Assert.AreEqual ("hola", actual, "Unexpected decoding of x-unknown.");

			// Note: cp-1260 doesn't exist, but will make CharsetUtils parse the codepage as 1260.
			actual = Rfc2047.DecodeText (Encoding.ASCII.GetBytes (cp1260));
			Assert.AreEqual ("hola", actual, "Unexpected decoding of cp1260.");
		}

		[Test]
		public void TestInvalidDoubleDomainMessageId ()
		{
			var msgid = MimeUtils.EnumerateReferences ("<local-part@domain1@domain2>").FirstOrDefault ();

			Assert.AreEqual ("local-part@domain1@domain2", msgid);
		}

		[Test]
		public void TestInvalidAtNoDomainMessageId ()
		{
			// https://github.com/jstedfast/MimeKit/issues/102
			var msgid = MimeUtils.EnumerateReferences ("<local-part@>").FirstOrDefault ();

			Assert.AreEqual ("local-part@", msgid);
		}

		[Test]
		public void TestInvalidNoDomainMessageId ()
		{
			var msgid = MimeUtils.EnumerateReferences ("<local-part>").FirstOrDefault ();

			Assert.AreEqual ("local-part", msgid);
		}
	}
}
