//
// MimeParserBenchmarks.cs
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

using System;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit;

namespace Benchmarks {
    public class MimeParserBenchmarks
    {
		static readonly string MessagesDataDir = Path.Combine (BenchmarkHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "mbox");
		const string MessageHeaderStressTest = @"From - 
Return-Path: <info@someserver>
Received: from maleman.mcom.com (maleman.mcom.com [198.93.92.3]) by urchin.netscape.com (8.6.12/8.6.9) with ESMTP id EAA18301; Thu, 25 Apr 1996 04:30:51 -0700
Received: from ns.netscape.com (ns.netscape.com.mcom.com [198.95.251.10]) by maleman.mcom.com (8.6.9/8.6.9) with ESMTP id EAA01168; Thu, 25 Apr 1996 04:29:58 -0700
Received: from RSA.COM (RSA.COM [192.80.211.33]) by ns.netscape.com (8.7.3/8.7.3) with SMTP id EAA17575; Thu, 25 Apr 1996 04:29:05 -0700 (PDT)
Received: from callisto.HIP.Berkeley.EDU by RSA.COM with SMTP
	id AA26475; Thu, 25 Apr 96 04:25:34 PDT
Received: (from raph@localhost) by callisto.hip.berkeley.edu (8.6.12/8.6.12) id DAA00979 for smime-dev@rsa.com; Thu, 25 Apr 1996 03:26:57 -0700
X-Antivirus: Avast (VPS 210729-0, 7/29/2021), Outbound message
X-Antivirus-Status: Clean
X-EOPAttributedMessage: 0
X-EOPTenantAttributedMessage: 44d81603-6a98-429d-8ab9-bb947b669639:0
X-MS-PublicTrafficType: Email
X-MS-Office365-Filtering-Correlation-Id: eea4ffa7-7fd9-42d0-4e7b-08d9524bc48c
X-MS-TrafficTypeDiagnostic: FRYP281MB0583:
X-LD-Processed: 44d81603-6a98-429d-8ab9-bb947b669639,ExtFwd
X-MS-Exchange-Transport-Forked: True
X-Microsoft-Antispam-PRVS: =?utf-8?q?=3CFRYP281MB058343080804D92271C3A1D8E6EB9=40FRYP281MB0583=2EDEUP281?=
 =?utf-8?q?=2EPROD=2EOUTLOOK=2ECOM=3E?=
X-MS-Oob-TLC-OOBClassifiers: OLM:6108;
X-MS-Exchange-SenderADCheck: 0
X-MS-Exchange-AntiSpam-Relay: 0
X-Microsoft-Antispam: BCL:0;
X-Microsoft-Antispam-Message-Info: =?utf-8?q?JOmZ=2FsCm2X+QgnzHElzIVUYuE71YCZKgZrH90cQGqenMs1zwCDY0k4V7mHNii?=
 =?utf-8?q?tAMIfrCv4eluiE1mqaS82GDEkhlGuFB18RFNecBMs6JyqJ+nofe4e19AFCnm3?=
 =?utf-8?q?PwruBn3VdUmpXB0vnlgu4u6wy1jpcrHz8H64gTSIhDjWquqem9+SWjqZ0QvCO?=
 =?utf-8?q?cOiM9s4jBLn4TKw1SYlb97MpihwAYuD6rIDdH3jQG5mtRxj9YNCgNFTo6KumD?=
 =?utf-8?q?13alldCfqkaJ25FEnNdlvDwal7pUz7CjA9AIiyD0SYjs4L9tpC48C=2F7zW+9mH?=
 =?utf-8?q?s+MJUyi5gYGdiueE1T8KKW73lLDAUmXY4vR+WP0c1KGmhxOVEahyORh29H6vp?=
 =?utf-8?q?i28USMBVQ5h0QdigEzrjI0goTORAOqA8RE6VNcH9F+ZhVNH4vmTsLT4FtvSbf?=
 =?utf-8?q?mlFCDxqPv2v9aiKwaf2DzOzp3+uoKxYiKfA3IKTKw7brVDz2WAAgL+HodRSGb?=
 =?utf-8?q?ULwDAf8Zb0GBtBWvZwclibsS8fzOACTe8+IXdW8X58WJ5fbeUIOXDzS9lbpJM?=
 =?utf-8?q?g1zIvb+9eBeSdeHKtm5P45giWOXO=2F41p=2FpfAjpATtmV9wp0NvdLfCMFP6=2FXJj?=
 =?utf-8?q?ZSzYu19GUJOP5DusRK0p7ejlBjdYmZeYbP6vD6glAzjTQGNquwgJBAwVr+Hek?=
 =?utf-8?q?DdYT84E4qnaU4WQuCe7jc47+vng9CmQpi?=
X-Forefront-Antispam-Report: =?utf-8?q?CIP=3A82=2E102=2E144=2E78=3BCTRY=3AIL=3BLANG=3Aen=3BSCL=3A1=3BSRV=3A=3BIPV=3ANLI=3BSFV=3ANSPM?=
 =?utf-8?q?=3BH=3Amtaout66=2E012=2Enet=2Eil=3BPTR=3Amtaout66=2E012=2Enet=2Eil=3BCAT=3ANONE=3BSFS=3A=28?=
 =?utf-8?q?396003=29=28136003=29=2839830400003=29=28376002=29=28346002=29=2853546011=29=28863620?=
 =?utf-8?q?01=29=282616005=29=2868406010=29=28956004=29=2870586007=29=285660300002=29=287596003=29?=
 =?utf-8?q?=282906002=29=288676002=29=28498600001=29=28356005=29=2844736005=29=28166002=29=2834206?=
 =?utf-8?q?002=29=2836756003=29=2866574015=29=2826005=29=28316002=29=28966005=29=2883380400001=29=28?=
 =?utf-8?q?1420700001=29=28336012=29=3BDIR=3AOUT=3BSFP=3A1102=3B?=
X-ExternalRecipientOutboundConnectors: 44d81603-6a98-429d-8ab9-bb947b669639
X-MS-Exchange-ForwardingLoop: info@doe.com;44d81603-6a98-429d-8ab9-bb947b669639
X-OriginatorOrg: foobar.de
X-MS-Exchange-CrossTenant-OriginalArrivalTime: 26 Aug 2021 13:46:02.1598 (UTC)
X-MS-Exchange-CrossTenant-Network-Message-Id: eea4ffa7-7fd9-42d0-4e7b-08d9524bc48c
X-MS-Exchange-CrossTenant-Id: 44d81603-6a98-429d-8ab9-bb947b669639
X-MS-Exchange-CrossTenant-AuthSource: FR2DEU01FT016.eop-deu01.prod.protection.outlook.com
X-MS-Exchange-CrossTenant-AuthAs: Anonymous
X-MS-Exchange-CrossTenant-FromEntityHeader: Internet
X-MS-Exchange-Transport-CrossTenantHeadersStamped: FRYP281MB0583
From: John Doe <john@doe.com>
To: Jane Doe <jane@doe.com>
Date: Thu, 26 Aug 2021 09:46:01 -0400
Subject: Lets stress test the header parser!
Message-Id: <0123456789.5932184607@doe.com>
In-Reply-To: 
References:
X-Mailer: Microsoft Outlook 16.0
Thread-index: Add41XPhHzNfrvGkRJ2TYsozzI9LOQLXuPZA
MIME-version: 1.0
Content-type: text/plain; charset=""utf-8""
Content-language: en-us
X-Spam-Score: 0.000

Hello, there!

This is a short message body that is meant to make sure that the parser spends most of its time in the
header parser.

";
		static readonly byte[] MessageHeaderStressTestData = Encoding.ASCII.GetBytes (MessageHeaderStressTest);

		#region MimeParser

		static void MimeParserSingleMessage (string fileName, bool persistent = false)
		{
			var path = Path.Combine (MessagesDataDir, fileName);
			using var stream = File.OpenRead (path);
			var parser = new MimeParser (stream, MimeFormat.Entity, persistent);

			for (int i = 0; i < 1000; i++) {
				parser.ParseMessage ();

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity, persistent);
			}
		}

		[Benchmark]
		public void MimeParser_StarTrekMessage ()
		{
			MimeParserSingleMessage ("startrek.eml");
		}

		[Benchmark]
		public void MimeParser_StarTrekMessagePersistent ()
		{
			MimeParserSingleMessage ("startrek.eml", true);
		}

		static void MimeParserMboxFile (string fileName, bool persistent = false)
		{
			var path = Path.Combine (MboxDataDir, fileName);

			using var stream = File.OpenRead (path);
			using var looped = new LoopedInputStream (stream, 10);
			var parser = new MimeParser (looped, MimeFormat.Mbox, persistent);

			while (!parser.IsEndOfStream) {
				parser.ParseMessage ();
			}
		}

		[Benchmark]
		public void MimeParser_ContentLengthMbox ()
		{
			MimeParserMboxFile ("content-length.mbox.txt");
		}

		[Benchmark]
		public void MimeParser_ContentLengthMboxPersistent ()
		{
			MimeParserMboxFile ("content-length.mbox.txt", true);
		}

		[Benchmark]
		public void MimeParser_JwzMbox ()
		{
			MimeParserMboxFile ("jwz.mbox.txt");
		}

		[Benchmark]
		public void MimeParser_JwzMboxPersistent ()
		{
			MimeParserMboxFile ("jwz.mbox.txt", true);
		}

		[Benchmark]
		public void MimeParser_HeaderStressTest ()
		{
			using var stream = new MemoryStream (MessageHeaderStressTestData, false);
			using var looped = new LoopedInputStream (stream, 1000);
			var parser = new MimeParser (looped, MimeFormat.Mbox, true);

			while (!parser.IsEndOfStream) {
				parser.ParseMessage ();
			}
		}

		#endregion MimeParser

		#region ExperimentalMimeParser

		static void ExperimentalMimeParserSingleMessage (string fileName, bool persistent = false)
		{
			var path = Path.Combine (MessagesDataDir, fileName);
			using var stream = File.OpenRead (path);
			var parser = new ExperimentalMimeParser (stream, MimeFormat.Entity, persistent);

			for (int i = 0; i < 1000; i++) {
				parser.ParseMessage ();

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity, persistent);
			}
		}

		[Benchmark]
		public void ExperimentalMimeParser_StarTrekMessage ()
		{
			ExperimentalMimeParserSingleMessage ("startrek.eml");
		}

		[Benchmark]
		public void ExperimentalMimeParser_StarTrekMessagePersistent ()
		{
			ExperimentalMimeParserSingleMessage ("startrek.eml", true);
		}

		static void ExperimentalMimeParserMboxFile (string fileName, bool persistent = false)
		{
			var path = Path.Combine (MboxDataDir, fileName);
			using var stream = File.OpenRead (path);
			using var looped = new LoopedInputStream (stream, 10);
			var parser = new ExperimentalMimeParser (looped, MimeFormat.Mbox, persistent);

			while (!parser.IsEndOfStream) {
				parser.ParseMessage ();
			}
		}

		[Benchmark]
		public void ExperimentalMimeParser_ContentLengthMbox ()
		{
			ExperimentalMimeParserMboxFile ("content-length.mbox.txt");
		}

		[Benchmark]
		public void ExperimentalMimeParser_ContentLengthMboxPersistent ()
		{
			ExperimentalMimeParserMboxFile ("content-length.mbox.txt", true);
		}

		[Benchmark]
		public void ExperimentalMimeParser_JwzMbox ()
		{
			ExperimentalMimeParserMboxFile ("jwz.mbox.txt");
		}

		[Benchmark]
		public void ExperimentalMimeParser_JwzMboxPersistent ()
		{
			ExperimentalMimeParserMboxFile ("jwz.mbox.txt", true);
		}

		[Benchmark]
		public void ExperimentalMimeParser_HeaderStressTest ()
		{
			using var stream = new MemoryStream (MessageHeaderStressTestData, false);
			using var looped = new LoopedInputStream (stream, 1000);
			var parser = new ExperimentalMimeParser (looped, MimeFormat.Mbox, true);

			while (!parser.IsEndOfStream) {
				parser.ParseMessage ();
			}
		}

		#endregion ExperimentalMimeParser

		#region MimeReader

		static void MimeReaderSingleMessage (string fileName)
		{
			var path = Path.Combine (MessagesDataDir, fileName);
			using var stream = File.OpenRead (path);
			var reader = new MimeReader (stream, MimeFormat.Entity);

			for (int i = 0; i < 1000; i++) {
				reader.ReadMessage ();

				stream.Position = 0;
				reader.SetStream (stream, MimeFormat.Entity);
			}
		}

		[Benchmark]
		public void MimeReader_StarTrekMessage ()
		{
			MimeReaderSingleMessage ("startrek.eml");
		}

		static void MimeReaderMboxFile (string fileName)
		{
			var path = Path.Combine (MboxDataDir, fileName);
			using var stream = File.OpenRead (path);
			using var looped = new LoopedInputStream (stream, 10);
			var reader = new MimeReader (looped, MimeFormat.Mbox);

			while (!reader.IsEndOfStream) {
				reader.ReadMessage ();
			}
		}

		[Benchmark]
		public void MimeReader_ContentLengthMbox ()
		{
			MimeReaderMboxFile ("content-length.mbox.txt");
		}

		[Benchmark]
		public void MimeReader_JwzMbox ()
		{
			MimeReaderMboxFile ("jwz.mbox.txt");
		}

		[Benchmark]
		public void MimeReader_HeaderStressTest ()
		{
			using var stream = new MemoryStream (MessageHeaderStressTestData, false);
			using var looped = new LoopedInputStream (stream, 1000);
			var reader = new MimeReader (looped, MimeFormat.Mbox);

			while (!reader.IsEndOfStream) {
				reader.ReadMessage ();
			}
		}

		#endregion
	}
}
