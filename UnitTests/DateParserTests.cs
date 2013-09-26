//
// DateParserTests.cs
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
using System.Text;
using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class DateParserTests
	{
		static readonly string[] dates = new string[] {
			"Mon, 17 Jan 1994 11:14:55 -0500",
			"Mon, 17 Jan 01 11:14:55 -0500",
			"Tue, 30 Mar 2004 13:01:38 +0000",
			"Sat Mar 24 21:23:03 EDT 2007",
			"Sat, 24 Mar 2007 21:23:03 EDT",
			"Sat, 24 Mar 2007 21:23:03 GMT",
			"17-6-2008 17:10:08",
			"26 Dec 1991 20:45 (Thursday)",
			"Tue, 9 Jun 92 03:45:24 JST"
		};

		static readonly string[] expected = new string[] {
			"Mon, 17 Jan 1994 11:14:55 -0500",
			"Wed, 17 Jan 2001 11:14:55 -0500",
			"Tue, 30 Mar 2004 13:01:38 +0000",
			"Sat, 24 Mar 2007 21:23:03 -0400",
			"Sat, 24 Mar 2007 21:23:03 -0400",
			"Sat, 24 Mar 2007 21:23:03 +0000",
			"Tue, 17 Jun 2008 17:10:08 +0000",
			"Thu, 26 Dec 1991 20:45:00 +0000",
			"Tue, 09 Jun 1992 03:45:24 +0000"
		};

		[Test]
		public void TestDateParser ()
		{
			for (int i = 0; i < dates.Length; i++) {
				var text = Encoding.UTF8.GetBytes (dates[i]);
				DateTimeOffset date;

				Assert.IsTrue (DateUtils.TryParseDateTime (text, 0, text.Length, out date), "Failed to parse date: {0}", dates[i]);
				var parsed = DateUtils.ToString (date);

				Assert.AreEqual (expected[i], parsed, "Parsed date does not match: '{0}' vs '{1}'", parsed, expected[i]);
			}
		}
	}
}
