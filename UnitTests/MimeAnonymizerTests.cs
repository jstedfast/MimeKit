//
// MimeAnonymizerTests.cs
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

using System;
using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeAnonymizerTests
	{
		static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string TextDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "text");

		[Test]
		public void TestArgumentExceptions ()
		{
			var anonymizer = new MimeAnonymizer ();
			using var message = new MimeMessage ();
			using var entity = new MimePart ();

			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize ((MimeMessage) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (message, null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (null, message, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (FormatOptions.Default, (MimeMessage) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (FormatOptions.Default, message, null));

			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize ((MimeEntity) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (entity, null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (null, entity, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (FormatOptions.Default, (MimeEntity) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => anonymizer.Anonymize (FormatOptions.Default, entity, null));
		}

		[TestCase (
			" (qmail 21619 invoked from network); 15 Nov 2017 14:16:18 -0000\r\n",
			" (xxxxx xxxxx xxxxxxx xxxx xxxxxxx); 15 Nov 2017 14:16:18 -0000\r\n")]
		[TestCase (
			" from unknown (HELO EUR01-HE1-obe.outbound.protection.outlook.com) (80.68.177.35)\r\n  by  with SMTP; 15 Nov 2017 14:16:18 -0000\r\n",
			" from xxxxxxx (xxxx xxxxxxxxxxxxx.xxxxxxxx.xxxxxxxxxx.xxxxxxx.xxx) (xx.xx.xxx.xx)\r\n  by  with xxxx; 15 Nov 2017 14:16:18 -0000\r\n")]
		[TestCase (
			" from mail-he1eur01on0133.outbound.protection.outlook.com\r\n\t([104.47.0.133] helo=EUR01-HE1-obe.outbound.protection.outlook.com) by\r\n\tmyassp01.mynet.it with SMTP (2.5.5); 15 Nov 2017 15:16:20 +0100\r\n",
			" from xxxxxxxxxxxxxxxxxxx.xxxxxxxx.xxxxxxxxxx.xxxxxxx.xxx\r\n\t([xxx.xx.x.xxx] xxxxxxxxxxxxxxxxxx.xxxxxxxx.xxxxxxxxxx.xxxxxxx.xxx) by\r\n\txxxxxxxx.xxxxx.xx with xxxx (x.x.x); 15 Nov 2017 15:16:20 +0100\r\n")]
		[TestCase (
			" from AM4PR01MB1444.eurprd01.prod.exchangelabs.com (10.164.76.26) by\r\n AM4PR01MB1442.eurprd01.prod.exchangelabs.com (10.164.76.24) with Microsoft\r\n SMTP Server (version=TLS1_2,\r\n cipher=TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384_P256) id 15.20.218.12; Wed, 15\r\n Nov 2017 14:16:14 +0000\r\n",
			" from xxxxxxxxxxxxx.xxxxxxxx.xxxx.xxxxxxxxxxxx.xxx (xx.xxx.xx.xx) by\r\n xxxxxxxxxxxxx.xxxxxxxx.xxxx.xxxxxxxxxxxx.xxx (xx.xxx.xx.xx) with xxxxxxxxx\r\n xxxx xxxxxx (xxxxxxxxxxxxxx,\r\n xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx) id xx.xx.xxx.xx; Wed, 15\r\n Nov 2017 14:16:14 +0000\r\n")]
		[TestCase (
			" from AM4PR01MB1444.eurprd01.prod.exchangelabs.com\r\n ([fe80::7830:c66f:eaa8:e3dd]) by AM4PR01MB1444.eurprd01.prod.exchangelabs.com\r\n ([fe80::7830:c66f:eaa8:e3dd%14]) with mapi id 15.20.0218.015; Wed, 15 Nov\r\n 2017 14:16:14 +0000\r\n",
			" from xxxxxxxxxxxxx.xxxxxxxx.xxxx.xxxxxxxxxxxx.xxx\r\n ([xxxx::xxxx:xxxx:xxxx:xxxx]) by xxxxxxxxxxxxx.xxxxxxxx.xxxx.xxxxxxxxxxxx.xxx\r\n ([xxxx::xxxx:xxxx:xxxx:xxxxxxx]) with xxxx id xx.xx.xxxx.xxx; Wed, 15 Nov\r\n 2017 14:16:14 +0000\r\n")]
		[TestCase (
			" from unknown (this is a (nested comment) with an \\\"escaped quoted\")\r\n by AM4PR01MB1442.eurprd01.prod.exchangelabs.com (10.164.76.24) \r\n",
			" from xxxxxxx (xxxx xx x (xxxxxx xxxxxxx) xxxx xx \\xxxxxxxx xxxxxxx)\r\n by xxxxxxxxxxxxx.xxxxxxxx.xxxx.xxxxxxxxxxxx.xxx (xx.xxx.xx.xx) \r\n")]
		public void TestAnonymizeReceivedHeaderValue (string value, string expected)
		{
			var rawValue = Encoding.UTF8.GetBytes (value);
			var anonymizedValue = MimeAnonymizer.AnonymizeReceivedHeaderValue (rawValue);
			var anonymized = Encoding.UTF8.GetString (anonymizedValue);

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized value does not match expected value.");
		}

		[TestCase (
			" \":sysmail\"@  Some-Group. Some-Org,\r\n Muhammed.(I am  the greatest) Ali @(the)Vegas.WBA\r\n",
			" \"xxxxxxxx\"@  xxxxxxxxxx. xxxxxxxx,\r\n xxxxxxxx.(x xx  xxx xxxxxxxx) xxx @(xxx)xxxxx.xxx\r\n")]
		[TestCase (
			" Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>\r\n",
			" xxxx(x xxxx \\) xxxx) <xxxx(xxx xxxxxxx)@xxxxx.xxxx(xxx xxxx)>\r\n")]
		[TestCase (
			" GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington\r\n\t<hp@redhat.com>;, fejj@helixcode.com\r\n",
			" xxxxx xxxxxxx: xxxxxx xx xxxxx <xxxxxx@xxxxx.xxx>, xxxxx xxxxxxxxxx\r\n\t<xx@xxxxxx.xxx>;, xxxx@xxxxxxxxx.xxx\r\n")]
		[TestCase (
			" A Group(Some people):Chris Jones <c@(Chris's host.)public.example>, joe@example.org,\r\n John <jdoe@one.test> (my dear friend); (the end of the group)\r\n",
			" x xxxxx(xxxx xxxxxx):xxxxx xxxxx <x@(xxxxxxx xxxx.)xxxxxx.xxxxxxx>, xxx@xxxxxxx.xxx,\r\n xxxx <xxxx@xxx.xxxx> (xx xxxx xxxxxx); (xxx xxx xx xxx xxxxx)\r\n")]
		[TestCase (
			" \"Nathaniel S. Borenstein\" <nsb@thumper.bellcore.com>\r\n",
			" \"xxxxxxxxxxxxxxxxxxxxxxx\" <xxx@xxxxxxx.xxxxxxxx.xxx>\r\n")]
		[TestCase (
			" \"Nathaniel\r\nS. Borenstein\" <nsb@thumper.bellcore.com>\r\n",
			" \"xxxxxxxxx\r\nxxxxxxxxxxxxx\" <xxx@xxxxxxx.xxxxxxxx.xxx>\r\n")]
		[TestCase (
			" =?utf-8?b?2YfZhCDYqtiq2YPZhNmFINin2YTZhNi62Kkg2KfZhNil2YbYrNmE2YrYstmK2Kk=?=\r\n =?utf-8?b?IC/Yp9mE2LnYsdio2YrYqdif?= <do.you.speak@arabic.com>\r\n",
			" =?utf-8?b?xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx?=\r\n =?utf-8?b?xxxxxxxxxxxxxxxxxxxxxxxx?= <xx.xxx.xxxxx@xxxxxx.xxx>\r\n")]
		[TestCase (
			" =?utf-8?b?54uC44Gj44Gf44GT44Gu5LiW44Gn54u\r\nC44GG44Gq44KJ5rCX44Gv56K644GL44Gg?=\r\n =?utf-8?b?44CC?= <famous@quotes.ja>\r\n",
			" =?utf-8?b?xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r\nxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx?=\r\n =?utf-8?b?xxxx?= <xxxxxx@xxxxxx.xx>\r\n")]
		[TestCase (
			" 伊昭傑@郵件.商務, राम@मोहन.ईन्फो,\r\n юзер@екзампл.ком, θσερ@εχαμπλε.ψομ\r\n",
			" xxxxxxxxx@xxxxxx.xxxxxx, xxxxxxxxx@xxxxxxxxxxxx.xxxxxxxxxxxxxxx,\r\n xxxxxxxx@xxxxxxxxxxxxxx.xxxxxx, xxxxxxxx@xxxxxxxxxxxxxx.xxxxxx\r\n")]
		[TestCase (
			" <<<user2@example.org>>>, <another@example.net, second@example.org>\r\n",
			" <<<xxxxx@xxxxxxx.xxx>>>, <xxxxxxx@xxxxxxx.xxx, xxxxxx@xxxxxxx.xxx>\r\n")]
		[TestCase (
			" <user@[domain.com\r\n <img src=x onerror=alert()>]>\r\n",
			" <xxxx@[xxxxxx.xxx\r\n <xxx xxx=x xxxxxxx=xxxxx()>]>\r\n")]
		[TestCase (
			" <user@[domain.com]\0\r\n]>\r\n",
			" <xxxx@[xxxxxx.xxx]x\r\n]>\r\n")]
		[TestCase (
			" \"User Name\" <user@example.com>, \"Unterminated qstring token\r\n",
			" \"xxxxxxxxx\" <xxxx@xxxxxxx.xxx>, \"xxxxxxxxxxxxxxxxxxxxxxxxxx\r\n")]
		[TestCase (
			" \"User Name\" <user@example.com>, (Unterminated comment\r\n",
			" \"xxxxxxxxx\" <xxxx@xxxxxxx.xxx>, (xxxxxxxxxxxx xxxxxxx\r\n")]
		[TestCase (
			" Display Name <escaped\\\"quoted@example.com>\r\n",
			" xxxxxxx xxxx <xxxxxxx\\\"xxxxxx@xxxxxxx.xxx>\r\n")]
		public void TestAnonymizeAddressHeaderValue (string value, string expected)
		{
			var rawValue = Encoding.UTF8.GetBytes (value);
			var anonymizedValue = MimeAnonymizer.AnonymizeAddressHeaderValue (rawValue);
			var anonymized = Encoding.UTF8.GetString (anonymizedValue);

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized value does not match expected value.");
		}

		[TestCase (
			" attachment\r\n",
			" attachment\r\n")]
		[TestCase (
			" attachment; \r\n",
			" attachment; \r\n")]
		[TestCase (
			" attachment; filename=winmail.dat\r\n",
			" attachment; filename=xxxxxxxxxxx\r\n")]
		[TestCase (
			" attachment; filename=\"escaped \\\"quotes\\\".doc\";;\r\n",
			" attachment; filename=\"xxxxxxxx\\\"xxxxxx\\\"xxxx\";;\r\n")]
		[TestCase (
			" attachment;\r\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc\r\n",
			" attachment;\r\n filename*0*=xxxxxxxxxxxxxxxxxx;\n filename*1*=xxxxxxxx\r\n")]
		[TestCase (
			" attachment;\r\n filename*0*=UTF-8''UnicodeFile;\n filename*1=\"name.doc\";\r\n",
			" attachment;\r\n filename*0*=xxxxxxxxxxxxxxxxxx;\n filename*1=\"xxxxxxxx\";\r\n")]
		[TestCase (
			" inline; filename; size=32767;\r\n",
			" inline; filename; size=xxxxx;\r\n")]
		[TestCase (
			" inline; filename*; filename*0; filename*1*; filename*2*=;\r\n",
			" inline; filename*; filename*0; filename*1*; filename*2*=;\r\n")]
		public void TestAnonymizeContentDispositionValue (string value, string expected)
		{
			var rawValue = Encoding.UTF8.GetBytes (value);
			var anonymizedValue = MimeAnonymizer.AnonymizeContentDispositionValue (rawValue);
			var anonymized = Encoding.UTF8.GetString (anonymizedValue);

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized value does not match expected value.");
		}

		[TestCase (
			" application/octet-stream\r\n",
			" application/octet-stream\r\n")]
		[TestCase (
			" application/octet-stream; \r\n",
			" application/octet-stream; \r\n")]
		[TestCase (
			" text/plain; charset=us-ascii\r\n",
			" text/plain; charset=us-ascii\r\n")]
		[TestCase (
			" text/plain; charset=\"us-ascii\"\r\n",
			" text/plain; charset=\"us-ascii\"\r\n")]
		[TestCase (
			" text/plain; charset=us-ascii; format=flowed\r\n deslsp=yes; name=anonymize.txt\r\n",
			" text/plain; charset=us-ascii; format=flowed\r\n deslsp=yes; name=xxxxxxxxxxxxx\r\n")]
		[TestCase (
			" multipart/mixed;\r\n\tboundary=\"----=_NextPart_000_0031_01D36222.8A648550\"\r\n",
			" multipart/mixed;\r\n\tboundary=\"----=_NextPart_000_0031_01D36222.8A648550\"\r\n")]
		[TestCase (
			" multipart/mixed;\r\n\tboundary*=\"----=_NextPart_000_0031_01D36222.8A648550\"\r\n",
			" multipart/mixed;\r\n\tboundary*=\"----=_NextPart_000_0031_01D36222.8A648550\"\r\n")]
		[TestCase (
			" multipart/mixed;\r\n\tboundary*0*=US-ASCII''----=3D_NextPart_000_;\r\n\tboundary*1*=0031_01D36222.8A648550;\r\n",
			" multipart/mixed;\r\n\tboundary*0*=US-ASCII''----=3D_NextPart_000_;\r\n\tboundary*1*=0031_01D36222.8A648550;\r\n")]
		[TestCase (
			" application/octet-stream;\r\n name*0*=UTF-8''anonymize;\n name*1*=this.doc\r\n",
			" application/octet-stream;\r\n name*0*=xxxxxxxxxxxxxxxx;\n name*1*=xxxxxxxx\r\n")]
		[TestCase (
			" application/octet-stream;\r\n name*0*=UTF-8''UnicodeFile;\n name*1=\"name.doc\";\r\n",
			" application/octet-stream;\r\n name*0*=xxxxxxxxxxxxxxxxxx;\n name*1=\"xxxxxxxx\";\r\n")]
		[TestCase (
			" application/octet-stream;\r\n name=\"unterminated qstring value;\r\n",
			" application/octet-stream;\r\n name=\"xxxxxxxxxxxxxxxxxxxxxxxxxxx\r\n")]
		public void TestAnonymizeContentTypeValue (string value, string expected)
		{
			var rawValue = Encoding.UTF8.GetBytes (value);
			var anonymizedValue = MimeAnonymizer.AnonymizeContentTypeValue (rawValue);
			var anonymized = Encoding.UTF8.GetString (anonymizedValue);

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized value does not match expected value.");
		}

		[TestCase (
			" This is a simple subject...",
			" xxxx xx x xxxxxx xxxxxxxxxx")]
		[TestCase (
			" blurdy bloop =??q?no_charset?= beep boop\r\n",
			" xxxxxx xxxxx =??x?xxxxxxxxxx?= xxxx xxxx\r\n")]
		[TestCase (
			" blurdy bloop =?iso-8859-1?q?this_is_english?= beep boop\r\n",
			" xxxxxx xxxxx =?iso-8859-1?q?xxxxxxxxxxxxxxx?= xxxx xxxx\r\n")]
		[TestCase (
			" I'm so happy! =?utf-8?b?8J+YgA==?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?\r\n",
			" xxx xx xxxxxx =?utf-8?b?xxxxxxxx?= x xxxx xxxx xx xxxx =?utf-8?b?xxxxxxxxxxxxxxxxxxxx?= xxxxx xx xxxxx?\r\n")]
		[TestCase (
			" blurdy bloop =?=?q?this_is_english?= beep boop\r\n",
			" xxxxxx xxxxx =?=?x?xxxxxxxxxxxxxxx?= xxxx xxxx\r\n")]
		[TestCase (
			" blurdy bloop =?iso-8859-1??this_is_english?= beep boop\r\n",
			" xxxxxx xxxxx =?iso-8859-1??xxxxxxxxxxxxxxx?= xxxx xxxx\r\n")]
		[TestCase (
			" blurdy bloop =?iso-8859-1?=?this_is_english?= beep boop\r\n",
			" xxxxxx xxxxx =?iso-8859-1?=?xxxxxxxxxxxxxxx?= xxxx xxxx\r\n")]
		public void TestAnonymizeUnstructuredHeaderValue (string value, string expected)
		{
			var rawValue = Encoding.UTF8.GetBytes (value);
			var anonymizedValue = MimeAnonymizer.AnonymizeUnstructuredHeaderValue (rawValue);
			var anonymized = Encoding.UTF8.GetString (anonymizedValue);

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized value does not match expected value.");
		}

		static void AssertAnonymizeMessage (string fileName)
		{
			var path = Path.Combine (MessagesDataDir, fileName);
			var anon = Path.ChangeExtension (path, ".anonymized.eml");

			using (var stream = File.OpenRead (path)) {
				var parser = new ExperimentalMimeParser (stream, MimeFormat.Entity);

				using (var message = parser.ParseMessage ()) {
					var anonymizer = new MimeAnonymizer ();
					string anonymized;

					using (var memory = new MemoryStream ()) {
						anonymizer.Anonymize (message, memory);

						var buffer = memory.GetBuffer ();

						if (!File.Exists (anon))
							File.WriteAllBytes (anon, memory.ToArray ());

						anonymized = Encoding.UTF8.GetString (buffer, 0, (int) memory.Length).ReplaceLineEndings ();
					}

					var expected = File.ReadAllText (anon).ReplaceLineEndings ();

					Assert.That (anonymized, Is.EqualTo (expected), "Anonymized data does not match expected data.");
				}
			}
		}

		static void AssertAnonymizeEntity (string fileName)
		{
			var path = Path.Combine (MessagesDataDir, fileName);
			var anon = Path.ChangeExtension (path, ".anonymized.eml");

			using (var stream = File.OpenRead (path)) {
				var parser = new ExperimentalMimeParser (stream, MimeFormat.Entity);

				using (var entity = parser.ParseEntity ()) {
					var anonymizer = new MimeAnonymizer ();
					string anonymized;

					using (var memory = new MemoryStream ()) {
						anonymizer.Anonymize (entity, memory);

						var buffer = memory.GetBuffer ();

						if (!File.Exists (anon))
							File.WriteAllBytes (anon, memory.ToArray ());

						anonymized = Encoding.UTF8.GetString (buffer, 0, (int) memory.Length).ReplaceLineEndings ();
					}

					var expected = File.ReadAllText (anon).ReplaceLineEndings ();

					Assert.That (anonymized, Is.EqualTo (expected), "Anonymized data does not match expected data.");
				}
			}
		}

		[Test]
		public void TestAnonymizeSimpleEmbeddedMessage ()
		{
			AssertAnonymizeMessage ("simple-embedded-message.eml");
		}

		[Test]
		public void TestAnonymizeSimpleMultipartMessage ()
		{
			AssertAnonymizeMessage ("simple-multipart.eml");
		}

		[Test]
		public void TestAnonymizeSimpleMultipartEntity ()
		{
			AssertAnonymizeEntity ("simple-multipart.eml");
		}

		[Test]
		public void TestAnonymizeGeneratedMessage ()
		{
			string expected = @"Received: from xxxxxxxxxx.xxxxxxx.xxx by xxxxxxxxx via xxxx;
	Sun, 6 Nov 2025 13:22:23 -0400
From: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
Date: Sun, 06 Apr 2025 13:22:18 -0400
Subject: xxxx xx x xxxx xxxxxxx
Message-Id: <xx.x@xxxxxxx.xxx>
To: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
References: <xx.x@xxxxxxx.xxx>
In-Reply-To: <xx.x@xxxxxxx.xxx>
MIME-Version: 1.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

xxxx xx x xxxx xxxxxxx

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; name=xxxxxxxxxxxxxxx; charset=utf-8
Content-Disposition: attachment; filename=xxxxxxxxxxxxxxx
Content-Transfer-Encoding: base64

xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxx

------=_NextPart_000_003F_01CE98CE.6E826F90--
";

			using var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.To.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.Date = new DateTimeOffset (2025, 4, 6, 13, 22, 18, TimeSpan.FromHours (-4));
			message.Subject = "This is a test subject";
			message.References.Add ("id.1@mimekit.net");
			message.InReplyTo = "id.1@mimekit.net";
			message.MessageId = "id.2@mimekit.net";

			message.Headers.Insert (0, "Received", "from unit-tests.mimekit.net by localhost via SMTP; Sun, 6 Nov 2025 13:22:23 -0400");

			var multipart = new Multipart ("mixed") {
				Boundary = "----=_NextPart_000_003F_01CE98CE.6E826F90"
			};

			multipart.Add (new TextPart ("plain") {
				Text = "This is a test message\r\n"
			});

			multipart.Add (new TextPart ("plain") {
				FileName = "lorem-ipsum.txt",
				ContentTransferEncoding = ContentEncoding.Base64,
				Text = File.ReadAllText (Path.Combine (TextDataDir, "lorem-ipsum.txt")),
			});

			message.Body = multipart;

			var anonymizer = new MimeAnonymizer ();
			string anonymized;

			using (var memory = new MemoryStream ()) {
				anonymizer.Anonymize (message, memory);

				var buffer = memory.GetBuffer ();
				anonymized = Encoding.UTF8.GetString (buffer, 0, (int) memory.Length).ReplaceLineEndings ();
			}

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized data does not match expected data.");
		}

		[Test]
		public void TestAnonymizeGeneratedMessageWithoutBody ()
		{
			string expected = @"Received: from xxxxxxxxxx.xxxxxxx.xxx by xxxxxxxxx via xxxx;
	Sun, 6 Nov 2025 13:22:23 -0400
From: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
Date: Sun, 06 Apr 2025 13:22:18 -0400
Subject: xxxx xx x xxxx xxxxxxx
Message-Id: <xx.x@xxxxxxx.xxx>
To: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
References: <xx.x@xxxxxxx.xxx>
In-Reply-To: <xx.x@xxxxxxx.xxx>
xxxx xx xx xxxxxxx xxxxxxxxx

";

			using var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.To.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.Date = new DateTimeOffset (2025, 4, 6, 13, 22, 18, TimeSpan.FromHours (-4));
			message.Subject = "This is a test subject";
			message.References.Add ("id.1@mimekit.net");
			message.InReplyTo = "id.1@mimekit.net";
			message.MessageId = "id.2@mimekit.net";

			message.Headers.Insert (0, "Received", "from unit-tests.mimekit.net by localhost via SMTP; Sun, 6 Nov 2025 13:22:23 -0400");

			// add an invalid header
			message.Headers.Add (new Header (ParserOptions.Default, Encoding.ASCII.GetBytes ("This is an invalid header...\r\n"), Array.Empty<byte> (), true));

			var anonymizer = new MimeAnonymizer ();
			string anonymized;

			using (var memory = new MemoryStream ()) {
				anonymizer.Anonymize (message, memory);

				var buffer = memory.GetBuffer ();
				anonymized = Encoding.UTF8.GetString (buffer, 0, (int) memory.Length).ReplaceLineEndings ();
			}

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized data does not match expected data.");
		}

		[Test]
		public void TestPreserveHeaders ()
		{
			string expected = @"Received: from unit-tests.mimekit.net by localhost via SMTP;
	Sun, 6 Nov 2025 13:22:23 -0400
From: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
Date: Sun, 06 Apr 2025 13:22:18 -0400
Subject: xxxx xx x xxxx xxxxxxx
Message-Id: <id.2@mimekit.net>
To: ""xxxxxxxxxxxxxxxxxxx"" <xxxxxxxxxx@xxxxxxx.xxx>
References: <xx.x@xxxxxxx.xxx>
In-Reply-To: <xx.x@xxxxxxx.xxx>

";

			using var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.To.Add (new MailboxAddress ("LastName, FirstName", "unit-tests@mimekit.net"));
			message.Date = new DateTimeOffset (2025, 4, 6, 13, 22, 18, TimeSpan.FromHours (-4));
			message.Subject = "This is a test subject";
			message.References.Add ("id.1@mimekit.net");
			message.InReplyTo = "id.1@mimekit.net";
			message.MessageId = "id.2@mimekit.net";

			message.Headers.Insert (0, "Received", "from unit-tests.mimekit.net by localhost via SMTP; Sun, 6 Nov 2025 13:22:23 -0400");

			var anonymizer = new MimeAnonymizer ();
			string anonymized;

			// preserve some headers
			anonymizer.PreserveHeaders.Add ("received");
			anonymizer.PreserveHeaders.Add ("message-id");

			using (var memory = new MemoryStream ()) {
				anonymizer.Anonymize (message, memory);

				var buffer = memory.GetBuffer ();
				anonymized = Encoding.UTF8.GetString (buffer, 0, (int) memory.Length).ReplaceLineEndings ();
			}

			Assert.That (anonymized, Is.EqualTo (expected), "Anonymized data does not match expected data.");
		}
	}
}
