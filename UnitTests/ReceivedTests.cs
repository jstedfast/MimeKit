//
// ReceivedTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using System.Net;
using System.Text;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class ReceivedTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var received = new Received ();

			Assert.Throws<ArgumentNullException> (() => new Received (null, IPAddress.Loopback, "by.host.com", IPAddress.Loopback, DateTimeOffset.Now));
			Assert.Throws<ArgumentNullException> (() => new Received ("from.host.com", null, "by.host.com", IPAddress.Loopback, DateTimeOffset.Now));
			Assert.Throws<ArgumentNullException> (() => new Received ("from.host.com", IPAddress.Loopback, null, IPAddress.Loopback, DateTimeOffset.Now));
			Assert.Throws<ArgumentNullException> (() => new Received ("from.host.com", IPAddress.Loopback, "by.host.com", null, DateTimeOffset.Now));

			Assert.Throws<ArgumentNullException> (() => Received.Parse (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Received.Parse (Array.Empty<byte> (), 0, 1));

			Assert.Throws<ArgumentNullException> (() => Received.Parse ((ParserOptions) null, Array.Empty<byte> (), 0, 0));
			Assert.Throws<ArgumentNullException> (() => Received.Parse (ParserOptions.Default, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Received.Parse (ParserOptions.Default, Array.Empty<byte> (), 0, 1));

			Assert.Throws<ArgumentException> (() => received.From = string.Empty);
			Assert.Throws<ArgumentException> (() => received.From = "[127.");
			Assert.Throws<ArgumentException> (() => received.From = "[xyz.!@#.$%^.&*()]");
			Assert.Throws<ArgumentException> (() => received.From = "smtp .gmail.com");
			Assert.Throws<ArgumentException> (() => received.From = "smtp\a.gmail.com");
			Assert.Throws<ArgumentException> (() => received.From = "abc[def");
			Assert.Throws<ArgumentException> (() => received.From = "smtp..server.com");
			Assert.Throws<ArgumentException> (() => received.From = "smtp.-server.com");

			Assert.Throws<ArgumentException> (() => received.By = string.Empty);
			Assert.Throws<ArgumentException> (() => received.By = "[127.");
			Assert.Throws<ArgumentException> (() => received.By = "[xyz.!@#.$%^.&*()]");
			Assert.Throws<ArgumentException> (() => received.By = "smtp\t.gmail.com");
			Assert.Throws<ArgumentException> (() => received.By = "smtp\x7f.gmail.com");
			Assert.Throws<ArgumentException> (() => received.By = "abc\\def");
			Assert.Throws<ArgumentException> (() => received.By = "smtp..server.com");
			Assert.Throws<ArgumentException> (() => received.By = "smtp.-server.com");

			Assert.Throws<ArgumentException> (() => received.Via = string.Empty);

			Assert.Throws<ArgumentException> (() => received.With = string.Empty);

			Assert.Throws<ArgumentException> (() => received.Id = string.Empty);
			Assert.Throws<ArgumentException> (() => received.Id = "this is invalid...");

			Assert.Throws<ArgumentException> (() => received.For = string.Empty);
			Assert.Throws<ArgumentException> (() => received.For = "this is invalid...");

			Assert.Throws<ArgumentNullException> (() => received.ToString (null));

			// TryParse
			Assert.That (Received.TryParse (null, out _), Is.False, "TryParse(null)");
			Assert.That (Received.TryParse (null, 0, 0, out _), Is.False, "TryParse(null, 0, 0)");
			Assert.That (Received.TryParse (Array.Empty<byte> (), -1, 1, out _), Is.False, "TryParse(empty, -1, 1)");

			Assert.That (Received.TryParse (null, Array.Empty<byte> (), out _), Is.False, "TryParse(null, empty)");
			Assert.That (Received.TryParse (null, Array.Empty<byte> (), 0, 0, out _), Is.False, "TryParse(null, empty, 0, 0)");
			Assert.That (Received.TryParse (ParserOptions.Default, null, 0, 0, out _), Is.False, "TryParse(options, null, 0, 0)");
			Assert.That (Received.TryParse (ParserOptions.Default, Array.Empty<byte> (), -1, 1, out _), Is.False, "TryParse(options, empty, -1, 1)");

			Assert.Throws<ArgumentNullException> (() => new ReceivedClause (null, "value"));
			Assert.Throws<ArgumentNullException> (() => new ReceivedClause ("keyword", null));
			Assert.Throws<ArgumentNullException> (() => new ReceivedClause ("keyword", "value", null));

			Assert.Throws<ArgumentException> (() => new ReceivedClause (string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => new ReceivedClause ("keyword with spaces", "value"));
			Assert.Throws<ArgumentException> (() => new ReceivedClause ("-keyword-starting-with-dash", "value"));
			Assert.Throws<ArgumentException> (() => new ReceivedClause ("key!word", "value"));

			Assert.Throws<ArgumentException> (() => new ReceivedClause ("keyword", string.Empty));
		}

		class ReceivedResults
		{
			public readonly string HeaderValue;
			public string From;
			public string FromTcpInfo;
			public string By;
			public string ByTcpInfo;
			public string Via;
			public string With;
			public string Id;
			public string For;
			public string DateTime;
			public string Reformatted;

			public ReceivedResults (string headerValue)
			{
				HeaderValue = headerValue;
			}
		}

		[Test]
		public void TestConstructors ()
		{
			const string expected = " from smtp.source.com ([192.168.1.1])\r\n\tby smtp.target.com ([127.0.0.1]); Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var received = new Received ("smtp.source.com", IPAddress.Parse ("192.168.1.1"), "smtp.target.com", IPAddress.Parse ("127.0.0.1"), dateTime);
			var encoded = received.ToString ();

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("[192.168.1.1]"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo ("[127.0.0.1]"), "ByTcpInfo");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");
			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestCommentSpecials ()
		{
			const string expected = " from smtp.source.com (\\\\escaped\\\\) by smtp.target.com;\r\n\tSat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var buffer = Encoding.UTF8.GetBytes (expected);
			var received = Received.Parse (buffer);
			var encoded = received.ToString ();

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("\\escaped\\"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");
			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestCommentsGetUnfolded ()
		{
			const string expected = " from smtp.source.com (HELO [192.168.1.1])\r\n\tby smtp.target.com ([127.0.0.1]); Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var received = new Received {
				From = "smtp.source.com",
				FromTcpInfo = "HELO\t[192.\r\n168.1.1]",
				By = "smtp.target.com",
				ByTcpInfo = "[127.0.\r\n0.1]",
				DateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4))
			};
			var encoded = received.ToString ();

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("HELO [192.168.1.1]"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo ("[127.0.0.1]"), "ByTcpInfo");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");
			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestParsedCommentsGetUnfolded ()
		{
			const string input = " from smtp.source.com (HELO\r\n\t[192.168.1.1])\r\n\tby smtp.target.com (\r\n\t[127.0.0.1]\r\n\t); Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			const string expected = " from smtp.source.com (HELO [192.168.1.1])\r\n\tby smtp.target.com ( [127.0.0.1] ); Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var buffer = Encoding.UTF8.GetBytes (input);
			var received = Received.Parse (buffer);
			var encoded = received.ToString ();

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("HELO [192.168.1.1]"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo (" [127.0.0.1] "), "ByTcpInfo");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");
			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestRemoveComments ()
		{
			const string input = " from smtp.source.com (TcpInfo for 'from' clause)\r\n\tby smtp.target.com (TcpInfo for 'by' clause); Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			const string expected = " from smtp.source.com by smtp.target.com;\r\n\tSat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var buffer = Encoding.UTF8.GetBytes (input);
			var received = Received.Parse (buffer);

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("TcpInfo for 'from' clause"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo ("TcpInfo for 'by' clause"), "ByTcpInfo");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");

			// this should remove the FromTcpInfo
			received.FromTcpInfo = null;

			Assert.That (received.FromTcpInfo, Is.Null, "FromTcpInfo");

			// this should remove the ByTcpInfo
			received.ByTcpInfo = null;

			Assert.That (received.FromTcpInfo, Is.Null, "ByTcpInfo");

			var encoded = received.ToString ();

			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestRemoveClause ()
		{
			const string input = " from smtp.source.com by smtp.target.com via TCP with ESMTPS; Sat, 18 Apr 2026 20:05:38 -0400\r\n";
			const string expected = " from smtp.source.com by smtp.target.com with ESMTPS;\r\n\tSat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var buffer = Encoding.UTF8.GetBytes (input);
			var received = Received.Parse (buffer);

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.Null, "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.Null, "ByTcpInfo");
			Assert.That (received.Via, Is.EqualTo ("TCP"), "Via");
			Assert.That (received.With, Is.EqualTo ("ESMTPS"), "With");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");

			// this should remove the 'via' clause
			received.Via = null;

			Assert.That (received.Via, Is.Null, "Via");

			var encoded = received.ToString ();

			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		[Test]
		public void TestSettingAllProperties ()
		{
			const string expected = " from smtp.source.com ([192.168.1.1])\r\n\tby smtp.target.com ([127.0.0.1]) via TCP with ESMTPS\r\n\tid <VAD7UBNO1TU4.JCMO6CD121AX1@office365.com> for unit-tests@mimekit.net;\r\n\tSat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var received = new Received {
				From = "smtp.source.com",
				FromTcpInfo = "[192.168.1.1]",
				By = "smtp.target.com",
				ByTcpInfo = "[127.0.0.1]",
				Via = "TCP",
				With = "ESMTPS",
				Id = "<VAD7UBNO1TU4.JCMO6CD121AX1@office365.com>",
				For = "unit-tests@mimekit.net",
				DateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4))
			};
			var buffer = Encoding.ASCII.GetBytes (expected);
			var encoded = received.ToString ();

			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("[192.168.1.1]"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo ("[127.0.0.1]"), "ByTcpInfo");
			Assert.That (received.Via, Is.EqualTo ("TCP"), "Via");
			Assert.That (received.With, Is.EqualTo ("ESMTPS"), "With");
			Assert.That (received.Id, Is.EqualTo ("<VAD7UBNO1TU4.JCMO6CD121AX1@office365.com>"), "Id");
			Assert.That (received.For, Is.EqualTo ("unit-tests@mimekit.net"), "For");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");
			Assert.That (encoded, Is.EqualTo (expected), "ToString");

			Assert.That (Received.TryParse (buffer, out received), Is.True, "TryParse");
		}

		[Test]
		public void TestParsingAllProperties ()
		{
			const string expected = " from smtp.source.com ([192.168.1.1])\r\n\tby smtp.target.com ([127.0.0.1]) via TCP with ESMTPS\r\n\tid <VAD7UBNO1TU4.JCMO6CD121AX1@office365.com> for unit-tests@mimekit.net;\r\n\tSat, 18 Apr 2026 20:05:38 -0400\r\n";
			var dateTime = new DateTimeOffset (2026, 4, 18, 20, 05, 38, TimeSpan.FromHours (-4));
			var buffer = Encoding.ASCII.GetBytes (expected);

			Assert.That (Received.TryParse (buffer, out var received), Is.True, "TryParse");
			Assert.That (received.From, Is.EqualTo ("smtp.source.com"), "From");
			Assert.That (received.FromTcpInfo, Is.EqualTo ("[192.168.1.1]"), "FromTcpInfo");
			Assert.That (received.By, Is.EqualTo ("smtp.target.com"), "By");
			Assert.That (received.ByTcpInfo, Is.EqualTo ("[127.0.0.1]"), "ByTcpInfo");
			Assert.That (received.Via, Is.EqualTo ("TCP"), "Via");
			Assert.That (received.With, Is.EqualTo ("ESMTPS"), "With");
			Assert.That (received.Id, Is.EqualTo ("<VAD7UBNO1TU4.JCMO6CD121AX1@office365.com>"), "Id");
			Assert.That (received.For, Is.EqualTo ("unit-tests@mimekit.net"), "For");
			Assert.That (received.DateTime, Is.EqualTo (dateTime), "DateTime");

			var encoded = received.ToString ();

			Assert.That (encoded, Is.EqualTo (expected), "ToString");
		}

		static readonly ReceivedResults[] ValidTestCases = {
			// Syntactically normal Received headers

			// Example from RFC 5321
			new ReceivedResults (" from bar.com by foo.com ; Thu, 21 May 1998\r\n\t05:33:29 -0700\r\n") {
				From = "bar.com",
				By = "foo.com",
				DateTime = "Thu, 21 May 1998 05:33:29 -0700",
				Reformatted = " from bar.com by foo.com; Thu, 21 May 1998 05:33:29 -0700\r\n"
			},

			new ReceivedResults (" from thumper.bellcore.com by greenbush.bellcore.com (4.1/4.7)\r\n\tid <AA01648> for nsb; Fri, 29 Nov 91 07:13:33 EST\r\n") {
				From = "thumper.bellcore.com",
				By = "greenbush.bellcore.com",
				ByTcpInfo = "4.1/4.7",
				Id = "<AA01648>",
				For = "nsb",
				DateTime = "Fri, 29 Nov 1991 07:13:33 -0500",
				Reformatted = " from thumper.bellcore.com by greenbush.bellcore.com (4.1/4.7)\r\n\tid <AA01648> for nsb; Fri, 29 Nov 91 07:13:33 EST\r\n"
			},
			new ReceivedResults (" from joyce.cs.su.oz.au by thumper.bellcore.com (4.1/4.7)\r\n\tid <AA11898> for nsb@greenbush; Fri, 29 Nov 91 07:11:57 EST\r\n") {
				From = "joyce.cs.su.oz.au",
				By = "thumper.bellcore.com",
				ByTcpInfo = "4.1/4.7",
				Id = "<AA11898>",
				For = "nsb@greenbush",
				DateTime = "Fri, 29 Nov 1991 07:11:57 -0500",
				Reformatted = " from joyce.cs.su.oz.au by thumper.bellcore.com (4.1/4.7)\r\n\tid <AA11898> for nsb@greenbush; Fri, 29 Nov 91 07:11:57 EST\r\n"
			},
			new ReceivedResults (" from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41\r\n\tvia MS.5.6.greenbush.galaxy.sun4_41; Fri, 12 Jun 1992 13:29:05 -0400 (EDT)\r\n") {
				From = "Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41",
				Via = "MS.5.6.greenbush.galaxy.sun4_41",
				DateTime = "Fri, 12 Jun 1992 13:29:05 -0400",
				Reformatted = "\r\n\tfrom Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41\r\n\tvia MS.5.6.greenbush.galaxy.sun4_41; Fri, 12 Jun 1992 13:29:05 -0400 (EDT)\r\n"
			},
			new ReceivedResults (" from sqhilton.pc.cs.cmu.edu by po3.andrew.cmu.edu (5.54/3.15)\r\n\tid <AA21478> for beatty@cosmos.vlsi.cs.cmu.edu; Wed, 26 Aug 92 22:14:07 EDT\r\n") {
				From = "sqhilton.pc.cs.cmu.edu",
				By = "po3.andrew.cmu.edu",
				ByTcpInfo = "5.54/3.15",
				Id = "<AA21478>",
				For = "beatty@cosmos.vlsi.cs.cmu.edu",
				DateTime = "Wed, 26 Aug 1992 22:14:07 -0400"
			},
			new ReceivedResults (" from [127.0.0.1] by [127.0.0.1] id <AA21478> with sendmail (v1.8)\r\n\tfor <beatty@cosmos.vlsi.cs.cmu.edu>; Wed, 26 Aug 92 22:14:07 EDT\r\n") {
				From = "[127.0.0.1]",
				By = "[127.0.0.1]",
				Id = "<AA21478>",
				With = "sendmail",
				For = "<beatty@cosmos.vlsi.cs.cmu.edu>",
				DateTime = "Wed, 26 Aug 1992 22:14:07 -0400"
			},
			new ReceivedResults (" from smtp.domain.com (smtp.domain.com. [207.54.68.120])\r\n        by mx.google.com with ESMTPS id 4fb4d7f45d1cf-659329877a1si67605a12.45.2026.02.02.09.25.54\r\n        for <user@gmail.com>\r\n        (version=TLS1_2 cipher=ECDHE-ECDSA-CHACHA20-POLY1305 bits=256/256);\r\n        Mon, 02 Feb 2026 09:25:55 -0800 (PST)\r\n") {
				From = "smtp.domain.com",
				FromTcpInfo = "smtp.domain.com. [207.54.68.120]",
				By = "mx.google.com",
				With = "ESMTPS",
				Id = "4fb4d7f45d1cf-659329877a1si67605a12.45.2026.02.02.09.25.54",
				For = "<user@gmail.com>",
				DateTime = "Mon, 02 Feb 2026 09:25:55 -0800",
				Reformatted = " from smtp.domain.com (smtp.domain.com. [207.54.68.120])\r\n\tby mx.google.com with ESMTPS\r\n\tid 4fb4d7f45d1cf-659329877a1si67605a12.45.2026.02.02.09.25.54\r\n\tfor <user@gmail.com>\r\n\t(version=TLS1_2 cipher=ECDHE-ECDSA-CHACHA20-POLY1305 bits=256/256);\r\n\tMon, 02 Feb 2026 09:25:55 -0800 (PST)\r\n"
			},

			// Non-compliant Received headers that actually exist in the wild

			// Starts with 'by' clause
			new ReceivedResults (" by 2002:aa6:d946:0:b0:32a:1941:595b with SMTP id w6csp1900485lkc;\r\n        Mon, 2 Feb 2026 09:25:55 -0800 (PST)\r\n") {
				By = "2002:aa6:d946:0:b0:32a:1941:595b",
				With = "SMTP",
				Id = "w6csp1900485lkc",
				DateTime = "Mon, 02 Feb 2026 09:25:55 -0800",
				Reformatted = " by 2002:aa6:d946:0:b0:32a:1941:595b with SMTP id w6csp1900485lkc;\r\n\tMon, 2 Feb 2026 09:25:55 -0800 (PST)\r\n"
			},

			// "Microsoft SMTP Server" and "Frontend Transport" are not single-word values but according to rfc5321, they should be...
			new ReceivedResults (" from us-smtp-delivery-105.mimecast.com (216.205.24.105)\r\n\tby BN3NAM04FT018.mail.protection.outlook.com (10.152.92.162) with Microsoft\r\n\tSMTP Server (version=TLS1_2, cipher=TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384)\r\n\tid 15.20.1835.13 via Frontend Transport; Tue, 30 Apr 2019 19:10:19 +0000\r\n") {
				From = "us-smtp-delivery-105.mimecast.com",
				FromTcpInfo = "216.205.24.105",
				By = "BN3NAM04FT018.mail.protection.outlook.com",
				ByTcpInfo = "10.152.92.162",
				With = "Microsoft SMTP Server",
				Id = "15.20.1835.13",
				Via = "Frontend Transport",
				DateTime = "Tue, 30 Apr 2019 19:10:19 +0000",
				Reformatted = " from us-smtp-delivery-105.mimecast.com (216.205.24.105)\r\n\tby BN3NAM04FT018.mail.protection.outlook.com (10.152.92.162)\r\n\twith Microsoft SMTP Server\r\n\t(version=TLS1_2, cipher=TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384)\r\n\tid 15.20.1835.13 via Frontend Transport; Tue, 30 Apr 2019 19:10:19 +0000\r\n"
			},

			// Nested comments
			new ReceivedResults (" from [67.219.246.196] (using TLSv1.2 with cipher DHE-RSA-AES256-GCM-SHA384 (256 bits))\r\n\tby server-2.bemta.az-c.us-east-1.aws.symcld.net id 11/DD-19573-41C55BC5;\r\n\tTue, 16 Apr 2019 04:37:40 +0000\r\n") {
				From = "[67.219.246.196]",
				FromTcpInfo = "using TLSv1.2 with cipher DHE-RSA-AES256-GCM-SHA384 (256 bits)",
				By = "server-2.bemta.az-c.us-east-1.aws.symcld.net",
				Id = "11/DD-19573-41C55BC5",
				DateTime = "Tue, 16 Apr 2019 04:37:40 +0000",
				Reformatted = " from [67.219.246.196]\r\n\t(using TLSv1.2 with cipher DHE-RSA-AES256-GCM-SHA384 (256 bits))\r\n\tby server-2.bemta.az-c.us-east-1.aws.symcld.net id 11/DD-19573-41C55BC5;\r\n\tTue, 16 Apr 2019 04:37:40 +0000\r\n"
			},

			// No clauses
			new ReceivedResults (" (qmail 16244 invoked from network); 16 Apr 2019 04:37:38 -0000\r\n") {
				DateTime = "Tue, 16 Apr 2019 04:37:38 +0000"
			},

			// Multiple comments in the 'from' clause
			new ReceivedResults (" from smtp.source.com (HELO localhost) ([160.46.252.39])\r\n\tby smtp.destination.com with ESMTP/TLS; 02 Feb 2026 18:25:54 +0100\r\n") {
				From = "smtp.source.com",
				FromTcpInfo = "HELO localhost",
				By = "smtp.destination.com",
				With = "ESMTP/TLS",
				DateTime = "Mon, 02 Feb 2026 18:25:54 +0100"
			},
			new ReceivedResults (" from relay301.mycloudmailbox.com (unknown [207.126.101.249])\r\n\t(using TLSv1.2 with cipher ECDHE-RSA-AES128-SHA256 (128/128 bits))\r\n\t(No client certificate requested)\r\n\tby S15-GW103.mycloudmailbox.com (Postfix) with ESMTPS id 44th580QHjz2SnDr\r\n\tfor <unit-tests@mimekit.net>; Tue, 30 Apr 2019 08:42:52 -0400 (EDT)\r\n") {
				From = "relay301.mycloudmailbox.com",
				FromTcpInfo = "unknown [207.126.101.249]",
				By = "S15-GW103.mycloudmailbox.com",
				ByTcpInfo = "Postfix",
				With = "ESMTPS",
				Id = "44th580QHjz2SnDr",
				For = "<unit-tests@mimekit.net>",
				DateTime = "Tue, 30 Apr 2019 08:42:52 -0400",
				Reformatted = " from relay301.mycloudmailbox.com (unknown [207.126.101.249])\r\n\t(using TLSv1.2 with cipher ECDHE-RSA-AES128-SHA256 (128/128 bits))\r\n\t(No client certificate requested) by S15-GW103.mycloudmailbox.com (Postfix)\r\n\twith ESMTPS id 44th580QHjz2SnDr for <unit-tests@mimekit.net>;\r\n\tTue, 30 Apr 2019 08:42:52 -0400 (EDT)\r\n"
			},

#if false
			// Absolute yikes
			new ReceivedResults (" from  [()] by  () (MDaemon PRO v18.5.1) id md50900000001.msg;\r\n\tTue, 30 Apr 2019 11:19:43 -0400\r\n") {
				From = "[()]",
				By = "",
				ByTcpInfo = " MDaemon PRO v18.5.1",
				Id = "md50900000001.msg",
				DateTime = "Tue, 30 Apr 2019 11:19:43 -0400"
			},
#endif
		};

		static void AssertReceived (Received received, ReceivedResults expected)
		{
			Assert.That (received.From, Is.EqualTo (expected.From), "from");
			Assert.That (received.FromTcpInfo, Is.EqualTo (expected.FromTcpInfo), "fromTcpInfo");
			Assert.That (received.By, Is.EqualTo (expected.By), "by");
			Assert.That (received.ByTcpInfo, Is.EqualTo (expected.ByTcpInfo), "byTcpInfo");
			Assert.That (received.Via, Is.EqualTo (expected.Via), "via");
			Assert.That (received.With, Is.EqualTo (expected.With), "with");
			Assert.That (received.Id, Is.EqualTo (expected.Id), "id");
			Assert.That (received.For, Is.EqualTo (expected.For), "for");

			if (expected.DateTime == null) {
				Assert.That (received.DateTime, Is.Null, "dateTime");
			} else {
				string value = DateUtils.FormatDate (received.DateTime.Value);

				Assert.That (value, Is.EqualTo (expected.DateTime), "dateTime");
			}
		}

		[Test]
		public void TestParseValidHeaderValues ()
		{
			foreach (var testCase in ValidTestCases) {
				var buffer = Encoding.UTF8.GetBytes (testCase.HeaderValue);
				Received received;

				try {
					received = Received.Parse (buffer);
					AssertReceived (received, testCase);
				} catch (ParseException ex) {
					Assert.Fail ($"Parse(byte[]): {ex.Message}:\r\n{testCase.HeaderValue}");
					throw;
				}

				try {
					received = Received.Parse (buffer, 0, buffer.Length);
					AssertReceived (received, testCase);
				} catch (ParseException ex) {
					Assert.Fail ($"Parse(byte[], int, int): {ex.Message}:\r\n{testCase.HeaderValue}");
					throw;
				}

				var expected = testCase.Reformatted ?? testCase.HeaderValue;
				var reformatted = received.ToString ();

				if (FormatOptions.Default.NewLineFormat == NewLineFormat.Unix)
					expected = expected.Replace ("\r\n", "\n");

				Assert.That (reformatted, Is.EqualTo (expected), "ToString");
			}
		}

		[Test]
		public void TestTryParseValidHeaderValues ()
		{
			foreach (var testCase in ValidTestCases) {
				var buffer = Encoding.UTF8.GetBytes (testCase.HeaderValue);
				Received received;

				Assert.That (Received.TryParse (buffer, out received), Is.True, "TryParse(byte[])");
				AssertReceived (received, testCase);

				Assert.That (Received.TryParse (buffer, 0, buffer.Length, out received), Is.True, "TryParse(byte[], int, int)");
				AssertReceived (received, testCase);

				var expected = testCase.Reformatted ?? testCase.HeaderValue;
				var reformatted = received.ToString ();

				if (FormatOptions.Default.NewLineFormat == NewLineFormat.Unix)
					expected = expected.Replace ("\r\n", "\n");

				Assert.That (reformatted, Is.EqualTo (expected), "ToString");
			}
		}

		// unterminated comments
		[TestCase (" (this is an unterminated comment...", "Incomplete comment token at offset 1", 1, -1)]
		[TestCase (" from (this is an unterminated comment...", "Incomplete comment token at offset 6", 6, -1)]
		[TestCase (" from remote-host.com (this is an unterminated comment...", "Incomplete comment token at offset 22", 22, -1)]
		[TestCase (" from remote-host.com by smtp.local-host.com (this is an unterminated comment...", "Incomplete comment token at offset 45", 45, -1)]
		// duplicate clauses
		[TestCase (" from remote-host.com from remote-host.com", "Duplicate 'from' clause at offset 22", 22, 26)]
		[TestCase (" from remote-host.com by smtp.local-host.com by duplicate-host.com", "Duplicate 'by' clause at offset 45", 45, 47)]
		[TestCase (" from remote-host.com by smtp.local-host.com via Frontend Transport via Backend Transport", "Duplicate 'via' clause at offset 68", 68, 71)]
		[TestCase (" from remote-host.com by smtp.local-host.com with SMTP with ESMTP", "Duplicate 'with' clause at offset 55", 55, 59)]
		[TestCase (" from remote-host.com by smtp.local-host.com id <123@localhost> id <456@localhost>", "Duplicate 'id' clause at offset 64", 64, 66)]
		[TestCase (" from remote-host.com by smtp.local-host.com for user@domain.com for user@domain.com", "Duplicate 'for' clause at offset 65", 65, 68)]
		public void TestParseInvalidHeaderValues (string value, string reason, int tokenIndex, int errorIndex)
		{
			var header = new Header (HeaderId.Received, value);
			var buffer = Encoding.UTF8.GetBytes (value);

			if (errorIndex == -1)
				errorIndex = buffer.Length;

			Assert.That (Received.TryParse (buffer, out _), Is.False, $"TryParse(byte[]) should have failed: {reason}");
			Assert.That (Received.TryParse (buffer, 0, buffer.Length, out _), Is.False, $"TryParse(byte[], int, int) should have failed: {reason}");

			try {
				var received = Received.Parse (buffer);
				Assert.Fail ($"Parse(byte[]) should have thrown a ParseException: {reason}");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ErrorIndex");
				Assert.That (ex.Message, Is.EqualTo (reason), "Parse(byte[]) should have thrown a ParseException with the expected reason");
			} catch (Exception ex) {
				Assert.Fail ($"Parse(byte[]) should have thrown a ParseException, but threw {ex.GetType ().Name} instead.");
			}

			try {
				var received = Received.Parse (buffer, 0, buffer.Length);
				Assert.Fail ($"Parse(byte[], int, int) should have thrown a ParseException: {reason}");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ErrorIndex");
				Assert.That (ex.Message, Is.EqualTo (reason), "Parse(byte[], int, int) should have thrown a ParseException with the expected reason");
			} catch (Exception ex) {
				Assert.Fail ($"Parse(byte[], int, int) should have thrown a ParseException, but threw {ex.GetType ().Name} instead.");
			}
		}

		[TestCase ("smtp.gmail.com", "[192.168.1.1]", "smtp.office365.com", "[10.3.0.1]", "Frontend Transport", "Microsoft SMTP Server", "<123@gmail.com>", "user@office365.com",
			" from smtp.gmail.com ([192.168.1.1])\r\n\tby smtp.office365.com ([10.3.0.1]) via Frontend Transport\r\n\twith Microsoft SMTP Server id <123@gmail.com> for user@office365.com;\r\n\tWed, 15 Apr 2026 20:39:57 -0400\r\n")]
		public void TestToString (string from, string fromTcpInfo, string by, string byTcpInfo, string via, string with, string id, string @for, string expected)
		{
			var received = new Received {
				From = from,
				FromTcpInfo = fromTcpInfo,
				By = by,
				ByTcpInfo = byTcpInfo,
				Via = via,
				With = with,
				Id = id,
				For = @for,
				DateTime = new DateTimeOffset (2026, 4, 15, 20, 39, 57, TimeSpan.FromHours (-4))
			};
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			var value = received.ToString ();

			Assert.That (value, Is.EqualTo (expected));
		}
	}
}
