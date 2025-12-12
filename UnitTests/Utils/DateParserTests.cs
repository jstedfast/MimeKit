//
// DateParserTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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

using MimeKit.Utils;

namespace UnitTests.Utils
{
	[TestFixture]
	public class DateParserTests
	{
		static readonly string[] dates = {
			"Sun, 08 Dec 91 09:11:00 +0000", // 2-digit year
			"8 Dec 1991 09:11 (Sunday)", // missing seconds and timezone
			"26 Dec 1991 20:45 (Thursday)", // missing seconds and timezone
			"Tue, 9 Jun 92 03:45:24 JST", // 2-digit year & JST is a non-standard timezone name
			"Mon, 17 Jan 1994 11:14:55 -0500", // valid date format
			"Mon, 17 Jan 01 11:14:55 -0500", // 2-digit year (01)
			"Tue, 30 Mar 2004 13:01:38 +0000",
			"Sat Mar 24 21:23:03 EDT 2007", // missing comma and year is in wrong place
			"Sat, 24 Mar 2007 21:23:03 EDT", // valid date format w/ named EDT timezone
			"Sat, 24 Mar 2007 21:23:03 GMT", // valid date format w/ named GMT timezone
			"17-6-2008 17:10:08", // completely wrong date format
			"FRI, 30 NOV 2012 02:09:10 +0100", // capitalized weekday/month
			"Tue, 11 Feb 2014 22:27:10 +0100 (CET)",
			"Wed, 6 Aug 2014 01:53:48 -2200", // large timezone offset
			"Tue, 21 Apr 15 14:44:51 GMT",
			"Tue, 21 April 15 14:44:51 GMT", // full month name and 2-digit year
			"Thu, 1 Oct 2015 14:40:57 +0200 (Mitteleuropäische Sommerzeit)", // nonsensical comment
			"Tue, 12 Jun 2012 19:22:28 0200", // missing '+' before timezone
			"Fri, 8 May 2015", // missing time & timezone info
			"Fri, 8 May 2015 12", // missing minutes, seconds, timezone
			"Fri, 8 May 2015 12:05", // missing seconds & timezone
			"Fri, 8 May 2015 12:05:01", // missing timezone
			"Fri, 8 May 2015 12:05:01 400", // timezone in incorrect format
			"Sat, 9 May 2015 24:00:00 -0400", // 24:00 is not a valid time
			"Sat, 9 May 2015 25:00:00 -0400", // 25:00 is not a valid time
			"May 9 2015 25:00:00 -0400", // token order & 25:00 is not a valid time
			"2015 May 9 25:00:00 -0400", // token order & 25:00 is not a valid time
			"2015 May 9 25:99:78 -0400", // token order & 25:99:78 is not a valid time
			"25 Sep 81 06:03:27 -0400", // 2-digit year (and missing optional weekday)
			"Sat, 10 Sep 2022 12:59:19 -1234567890123456789", // crazy timezone offset
			"Sat, 10 Sep 2022 12:59:19 1234567890123456789", // crazy timezone offset
			"Sat, 10 Sep 2022 12:59:19 04+00", // invalid timezone offset format
			"Sat, 10 Sep 2022 12:59:19 ECST", // unknown timezone name
			"Sat, Sep 10 2022 12:59:19 0400", // missing + or - for timezone offset
			"Sat, Sep 10 77 12:59:19 0400", // 2-digit year and missing + or - for timezone offset
			"Sat, 01 Mar 2025 09:00:00 XYZ", // invalid timezone name
			"Mon, 28 Feb 2025 09:00:00 -0500", // wrong weekday
			"Fri, 28 Feb 2025 12:15:00 AM -0500", // 12:15 AM but otherwise standard format
			"Fri, 28 Feb 2025 11:30:00 PM -0500", // 11:30 PM but otherwise standard format
			"Feb 28 2025 12:15:00 AM -0500", // 12:15 AM with non-standard format
			"Feb 28 2025 11:30:00 PM -0500", // 11:30 PM with non-standard format
			"Fri, 28 Feb 2025 23:59:60 -0500", // leap second
			"Fri, 28 Feb 2025 23:59:61 -0500", // too many seconds
			"Fri, 28 Feb 2025 22:59:60 -0500", // leap second only valid if time is 23:59:60
			"Fri, 28 Feb 2025 23:60:59 -0500", // too many minutes
			"31 Dec 2024 10:15:00 AM", // missing timezone info after AM time
			"31 Dec 2024 10:15:00 PM" // missing timezone info after PM time
		};

		static readonly string[] expected = {
			"Sun, 08 Dec 1991 09:11:00 +0000",
			"Sun, 08 Dec 1991 09:11:00 +0000",
			"Thu, 26 Dec 1991 20:45:00 +0000",
			"Tue, 09 Jun 1992 03:45:24 +0900",
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
			"Tue, 12 Jun 2012 19:22:28 +0200",
			"Fri, 08 May 2015 00:00:00 +0000",
			"Fri, 08 May 2015 00:00:00 +0000",
			"Fri, 08 May 2015 12:05:00 +0000",
			"Fri, 08 May 2015 12:05:01 +0000",
			"Fri, 08 May 2015 12:05:01 +0400",
			"Sat, 09 May 2015 00:00:00 -0400",
			"Sat, 09 May 2015 00:00:00 -0400",
			"Sat, 09 May 2015 00:00:00 -0400",
			"Sat, 09 May 2015 00:00:00 -0400",
			"Sat, 09 May 2015 00:00:00 -0400",
			"Fri, 25 Sep 1981 06:03:27 -0400",
			"Sat, 10 Sep 2022 12:59:19 +0000",
			"Sat, 10 Sep 2022 12:59:19 +0000",
			"Sat, 10 Sep 2022 12:59:19 +0000",
			"Sat, 10 Sep 2022 12:59:19 +0000",
			"Sat, 10 Sep 2022 12:59:19 +0400",
			"Sat, 10 Sep 1977 12:59:19 +0400",
			"Sat, 01 Mar 2025 09:00:00 +0000",
			"Fri, 28 Feb 2025 09:00:00 -0500",
			"Fri, 28 Feb 2025 00:15:00 -0500",
			"Fri, 28 Feb 2025 23:30:00 -0500",
			"Fri, 28 Feb 2025 00:15:00 -0500",
			"Fri, 28 Feb 2025 23:30:00 -0500",
			"Fri, 28 Feb 2025 23:59:59 -0500",
			"Fri, 28 Feb 2025 00:00:00 -0500",
			"Fri, 28 Feb 2025 00:00:00 -0500",
			"Fri, 28 Feb 2025 00:00:00 -0500",
			"Tue, 31 Dec 2024 10:15:00 +0000",
			"Tue, 31 Dec 2024 22:15:00 +0000"
		};

		[Test]
		public void TestDateParser ()
		{
			for (int i = 0; i < dates.Length; i++) {
				var text = Encoding.UTF8.GetBytes (dates[i]);
				DateTimeOffset date;
				string parsed;

				Assert.That (DateUtils.TryParse (text, 0, text.Length, out date), Is.True, $"Failed to parse date: {dates[i]}");
				parsed = DateUtils.FormatDate (date);
				Assert.That (parsed, Is.EqualTo (expected[i]), $"Parsed date does not match: '{parsed}' vs '{expected[i]}'");

				Assert.That (DateUtils.TryParse (text, 0, out date), Is.True, $"Failed to parse date: {dates[i]}");
				parsed = DateUtils.FormatDate (date);
				Assert.That (parsed, Is.EqualTo (expected[i]), $"Parsed date does not match: '{parsed}' vs '{expected[i]}'");

				Assert.That (DateUtils.TryParse (text, out date), Is.True, $"Failed to parse date: {dates[i]}");
				parsed = DateUtils.FormatDate (date);
				Assert.That (parsed, Is.EqualTo (expected[i]), $"Parsed date does not match: '{parsed}' vs '{expected[i]}'");

				Assert.That (DateUtils.TryParse (dates[i], out date), Is.True, $"Failed to parse date: {dates[i]}");
				parsed = DateUtils.FormatDate (date);
				Assert.That (parsed, Is.EqualTo (expected[i]), $"Parsed date does not match: '{parsed}' vs '{expected[i]}'");
			}
		}

		static readonly string[] invalidDates = {
			"this is pure junk",
			"Sunday is the day of our Lord",
			"Sun is so bright, I gotta wear shades",
			"Sat, 8 dogs did while 8 cats hid",
			"Sat, 9 May flies bit my arms"
		};

		[Test]
		public void TestParseInvalidDates ()
		{
			for (int i = 0; i < invalidDates.Length; i++) {
				var text = Encoding.UTF8.GetBytes (invalidDates[i]);

				Assert.That (DateUtils.TryParse (text, 0, text.Length, out _), Is.False, $"Should not have parsed '{invalidDates[i]}'");
				Assert.That (DateUtils.TryParse (text, 0, out _), Is.False, $"Should not have parsed '{invalidDates[i]}'");
				Assert.That (DateUtils.TryParse (text, out _), Is.False, $"Should not have parsed '{invalidDates[i]}'");
				Assert.That (DateUtils.TryParse (invalidDates[i], out _), Is.False, $"Should not have parsed '{invalidDates[i]}'");
			}
		}

		[TestCase ("Wed, 1 Jan 2025 09:00:00 -0500", 1)]
		[TestCase ("Sat, 1 Feb 2025 09:00:00 -0500", 2)]
		[TestCase ("Sat, 1 Mar 2025 09:00:00 -0500", 3)]
		[TestCase ("Tue, 1 Apr 2025 09:00:00 -0500", 4)]
		[TestCase ("Thu, 1 May 2025 09:00:00 -0500", 5)]
		[TestCase ("Sun, 1 Jun 2025 09:00:00 -0500", 6)]
		[TestCase ("Tue, 1 Jul 2025 09:00:00 -0500", 7)]
		[TestCase ("Fri, 1 Aug 2025 09:00:00 -0500", 8)]
		[TestCase ("Mon, 1 Sep 2025 09:00:00 -0500", 9)]
		[TestCase ("Wed, 1 Oct 2025 09:00:00 -0500", 10)]
		[TestCase ("Sat, 1 Nov 2025 09:00:00 -0500", 11)]
		[TestCase ("Mon, 1 Dec 2025 09:00:00 -0500", 12)]
		public void TestMonths (string value, int month)
		{
			Assert.That (DateUtils.TryParse (value, out var date), Is.True, $"Failed to parse date: {value}");
			Assert.That (date.Month, Is.EqualTo (month));
		}

		[TestCase ("GMT", 0)]
		[TestCase ("UTC", 0)]
		[TestCase ("EDT", -400)]
		[TestCase ("EST", -500)]
		[TestCase ("CDT", -500)]
		[TestCase ("CST", -600)]
		[TestCase ("MDT", -600)]
		[TestCase ("MST", -700)]
		[TestCase ("PDT", -700)]
		[TestCase ("PST", -800)]
		[TestCase ("A", 100)]
		[TestCase ("B", 200)]
		[TestCase ("C", 300)]
		[TestCase ("D", 400)]
		[TestCase ("E", 500)]
		[TestCase ("F", 600)]
		[TestCase ("G", 700)]
		[TestCase ("H", 800)]
		[TestCase ("I", 900)]
		[TestCase ("K", 1000)]
		[TestCase ("L", 1100)]
		[TestCase ("M", 1200)]
		[TestCase ("N", -100)]
		[TestCase ("O", -200)]
		[TestCase ("P", -300)]
		[TestCase ("Q", -400)]
		[TestCase ("R", -500)]
		[TestCase ("S", -600)]
		[TestCase ("T", -700)]
		[TestCase ("U", -800)]
		[TestCase ("V", -900)]
		[TestCase ("W", -1000)]
		[TestCase ("X", -1100)]
		[TestCase ("Y", -1200)]
		[TestCase ("Z", 0)]
		[TestCase ("JST", 900)]
		[TestCase ("KST", 900)]
		public void TestTimezones (string zone, int expectedOffset)
		{
			TimeSpan tzone = new TimeSpan (expectedOffset / 100, Math.Abs (expectedOffset % 100), 0);
			var value = string.Format ("Fri, 28 Feb 2025 09:00:00 {0}", zone);

			Assert.That (DateUtils.TryParse (value, out var date), Is.True, $"Failed to parse date: {value}");
			Assert.That (date.Offset, Is.EqualTo (tzone), $"Parsed date offset does not match: '{date.Offset}' vs '{expectedOffset}'");
		}
	}
}
