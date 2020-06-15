//
// DateParserTests.cs
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

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class DateParserTests
	{
		static readonly string[] dates = {
			"Sun, 08 Dec 91 09:11:00 +0000",
			"8 Dec 1991 09:11 (Sunday)",
			"26 Dec 1991 20:45 (Thursday)",
			"Tue, 9 Jun 92 03:45:24 JST",
			"Mon, 17 Jan 1994 11:14:55 -0500",
			"Mon, 17 Jan 01 11:14:55 -0500",
			"Tue, 30 Mar 2004 13:01:38 +0000",
			"Sat Mar 24 21:23:03 EDT 2007",
			"Sat, 24 Mar 2007 21:23:03 EDT",
			"Sat, 24 Mar 2007 21:23:03 GMT",
			"17-6-2008 17:10:08",
			"FRI, 30 NOV 2012 02:09:10 +0100",
			"Tue, 11 Feb 2014 22:27:10 +0100 (CET)",
			"Wed, 6 Aug 2014 01:53:48 -2200",
			"Tue, 21 Apr 15 14:44:51 GMT",
			"Tue, 21 April 15 14:44:51 GMT",
			"Thu, 1 Oct 2015 14:40:57 +0200 (Mitteleuropäische Sommerzeit)",
			"Tue, 12 Jun 2012 19:22:28 0200"
		};

		static readonly string[] expected = {
			"Sun, 08 Dec 1991 09:11:00 +0000",
			"Sun, 08 Dec 1991 09:11:00 +0000",
			"Thu, 26 Dec 1991 20:45:00 +0000",
			"Tue, 09 Jun 1992 03:45:24 +0000",
			"Mon, 17 Jan 1994 11:14:55 -0500",
			"Wed, 17 Jan 2001 11:14:55 -0500",
			"Tue, 30 Mar 2004 13:01:38 +0000",
			"Sat, 24 Mar 2007 21:23:03 -0400",
			"Sat, 24 Mar 2007 21:23:03 -0400",
			"Sat, 24 Mar 2007 21:23:03 +0000",
			"Tue, 17 Jun 2008 17:10:08 +0000",
			"Fri, 30 Nov 2012 02:09:10 +0100",
			"Tue, 11 Feb 2014 22:27:10 +0100",
			"Wed, 06 Aug 2014 01:53:48 +0000",
			"Tue, 21 Apr 2015 14:44:51 +0000",
			"Tue, 21 Apr 2015 14:44:51 +0000",
			"Thu, 01 Oct 2015 14:40:57 +0200",
			"Tue, 12 Jun 2012 19:22:28 +0200"
		};

		[Test]
		public void TestDateParser ()
		{
			DateTimeOffset date;
			string parsed;
			byte[] text;

			for (int i = 0; i < dates.Length; i++) {
				text = Encoding.UTF8.GetBytes (dates[i]);

				Assert.IsTrue (DateUtils.TryParse (text, 0, text.Length, out date), "Failed to parse date: {0}", dates[i]);
				parsed = DateUtils.FormatDate (date);
				Assert.AreEqual (expected[i], parsed, "Parsed date does not match: '{0}' vs '{1}'", parsed, expected[i]);

				Assert.IsTrue (DateUtils.TryParse (text, 0, out date), "Failed to parse date: {0}", dates[i]);
				parsed = DateUtils.FormatDate (date);
				Assert.AreEqual (expected[i], parsed, "Parsed date does not match: '{0}' vs '{1}'", parsed, expected[i]);

				Assert.IsTrue (DateUtils.TryParse (text, out date), "Failed to parse date: {0}", dates[i]);
				parsed = DateUtils.FormatDate (date);
				Assert.AreEqual (expected[i], parsed, "Parsed date does not match: '{0}' vs '{1}'", parsed, expected[i]);

				Assert.IsTrue (DateUtils.TryParse (dates[i], out date), "Failed to parse date: {0}", dates[i]);
				parsed = DateUtils.FormatDate (date);
				Assert.AreEqual (expected[i], parsed, "Parsed date does not match: '{0}' vs '{1}'", parsed, expected[i]);
			}

			text = Encoding.ASCII.GetBytes ("this is pure junk");

			Assert.IsFalse (DateUtils.TryParse (text, 0, text.Length, out date), "Should not have parsed junk.");
			Assert.IsFalse (DateUtils.TryParse (text, 0, out date), "Should not have parsed junk.");
			Assert.IsFalse (DateUtils.TryParse (text, out date), "Should not have parsed junk.");
			Assert.IsFalse (DateUtils.TryParse ("this is pure junk", out date), "Should not have parsed junk.");
		}
	}
}
