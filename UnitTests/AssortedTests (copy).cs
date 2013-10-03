//
// AssortedTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
			var expected = "This is a part in a (multipart) message generated with the MimeKit\n" +
			               "library.\n\n" +
			               "All of the parts of this message are identical, however they've been\n" +
			               "encoded for transport using different methods.\n";

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
			var expected = "This is a part in a (multipart) message generated with the MimeKit\n" +
			               "library. All of the parts of this message are identical, however\n" +
			               "they've been encoded for transport using different methods.\n";

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
			Assert.AreEqual ("<some.message.id.1@some.domain>", msgid, "The parsed msgid does not match");

			obsolete = "<some.message.id.2@some.domain> as sent on Mon, 17 Jan 1994 11:14:55 -0500";
			msgid = MimeUtils.EnumerateReferences (obsolete).FirstOrDefault ();

			Assert.IsNotNull (msgid, "The parsed msgid token should not be null");
			Assert.AreEqual ("<some.message.id.2@some.domain>", msgid, "The parsed msgid does not match");
		}
	}
}
