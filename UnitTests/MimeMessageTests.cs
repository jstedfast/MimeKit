//
// MimeMessageTests.cs
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
using System.Net.Mail;
using System.Reflection;

using MimeKit;
using MimeKit.Utils;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class MimeMessageTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var unknown = new Cryptography.UnknownCryptographyContext ();
			var message = new MimeMessage ();

			Assert.Throws<ArgumentOutOfRangeException> (() => message.Importance = (MessageImportance) 500);
			Assert.Throws<ArgumentOutOfRangeException> (() => message.Priority = (MessagePriority) 500);
			Assert.Throws<ArgumentOutOfRangeException> (() => message.XPriority = (XMessagePriority) 500);
			Assert.Throws<ArgumentException> (() => message.ResentMessageId = "this is some random text...");
			Assert.Throws<ArgumentException> (() => message.MessageId = "this is some random text...");
			Assert.Throws<ArgumentException> (() => message.InReplyTo = "this is some random text...");
			Assert.Throws<ArgumentNullException> (() => message.ResentMessageId = null);
			Assert.Throws<ArgumentNullException> (() => message.MessageId = null);
			Assert.Throws<ArgumentNullException> (() => message.Subject = null);
			Assert.Throws<ArgumentNullException> (() => message.MimeVersion = null);

			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load ((Stream) null, true));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (ParserOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (null, Stream.Null, true));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (ParserOptions.Default, (Stream) null, true));

			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load ((string) null));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => MimeMessage.Load (ParserOptions.Default, (string) null));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((Stream) null, true));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, Stream.Null, true));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (Stream) null, true));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, "fileName"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (string) null));

			Assert.Throws<ArgumentNullException> (() => message.Accept (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => message.Prepare (EncodingConstraint.None, 10));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo ((string) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo ((Stream) null, true));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (FormatOptions.Default, (string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync ((string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync (null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync ((Stream) null, true));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync (FormatOptions.Default, (Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync (null, "fileName"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await message.WriteToAsync (FormatOptions.Default, (string) null));

			var sender = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			message.From.Add (sender);
			message.To.Add (new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com"));
			var body = new TextPart ("plain") {
				Text = "This is the message body."
			};

			Assert.Throws<ArgumentNullException> (() => message.Sign (null));
			Assert.Throws<ArgumentNullException> (() => message.Sign (null, DigestAlgorithm.Sha1));
			Assert.ThrowsAsync<ArgumentNullException> (() => message.SignAsync (null));
			Assert.ThrowsAsync<ArgumentNullException> (() => message.SignAsync (null, DigestAlgorithm.Sha1));

			message.From.Clear ();
			Assert.Throws<InvalidOperationException> (() => message.Sign (unknown));
			Assert.Throws<InvalidOperationException> (() => message.Sign (unknown, DigestAlgorithm.Sha1));
			Assert.ThrowsAsync<InvalidOperationException> (() => message.SignAsync (unknown));
			Assert.ThrowsAsync<InvalidOperationException> (() => message.SignAsync (unknown, DigestAlgorithm.Sha1));
			message.From.Add (sender);

			Assert.Throws<ArgumentNullException> (() => message.Encrypt (null));
			Assert.ThrowsAsync<ArgumentNullException> (() => message.EncryptAsync (null));

			message.Body = body;
			Assert.Throws<ArgumentException> (() => message.Encrypt (unknown));
			Assert.ThrowsAsync<ArgumentException> (() => message.EncryptAsync (unknown));
			message.Body = null;
			Assert.Throws<InvalidOperationException> (() => message.Encrypt (unknown));
			Assert.ThrowsAsync<InvalidOperationException> (() => message.EncryptAsync (unknown));

			Assert.Throws<ArgumentNullException> (() => message.SignAndEncrypt (null));
			Assert.ThrowsAsync<ArgumentNullException> (() => message.SignAndEncryptAsync (null));

			message.Body = body;
			Assert.Throws<ArgumentException> (() => message.SignAndEncrypt (unknown));
			Assert.ThrowsAsync<ArgumentException> (() => message.SignAndEncryptAsync (unknown));
			message.Body = null;
			Assert.Throws<InvalidOperationException> (() => message.SignAndEncrypt (unknown));
			Assert.ThrowsAsync<InvalidOperationException> (() => message.SignAndEncryptAsync (unknown));

			Assert.Throws<ArgumentNullException> (() => MimeMessage.CreateFromMailMessage (null));
		}

		[Test]
		public void TestGetRecipients ()
		{
			var message = new MimeMessage ();
			message.Sender = new MailboxAddress ("Example Sender", "sender@example.com");
			message.From.Add (new MailboxAddress ("Example From", "from@example.com"));
			message.ReplyTo.Add (new MailboxAddress ("Example Reply-To", "reply-to@example.com"));
			message.To.Add (new MailboxAddress ("Example To", "to@example.com"));
			message.To.Add (new MailboxAddress ("Example To Duplicate", "to@example.com"));
			message.Cc.Add (new MailboxAddress ("Example Cc", "cc@example.com"));
			message.Cc.Add (new MailboxAddress ("Example Cc Duplicate", "cc@example.com"));
			message.Bcc.Add (new MailboxAddress ("Example Bcc", "bcc@example.com"));
			message.Bcc.Add (new MailboxAddress ("Example Bcc Duplicate", "bcc@example.com"));

			var recipients = message.GetRecipients (false);
			Assert.That (recipients.Count, Is.EqualTo (6), "Count");
			Assert.That (recipients[0], Is.EqualTo (message.To[0]), "recipients[0]");
			Assert.That (recipients[1], Is.EqualTo (message.To[1]), "recipients[1]");
			Assert.That (recipients[2], Is.EqualTo (message.Cc[0]), "recipients[2]");
			Assert.That (recipients[3], Is.EqualTo (message.Cc[1]), "recipients[3]");
			Assert.That (recipients[4], Is.EqualTo (message.Bcc[0]), "recipients[4]");
			Assert.That (recipients[5], Is.EqualTo (message.Bcc[1]), "recipients[5]");

			recipients = message.GetRecipients (true);
			Assert.That (recipients.Count, Is.EqualTo (3), "Count (uniqueOnly)");
			Assert.That (recipients[0], Is.EqualTo (message.To.Mailboxes.First ()), "recipients[0] (uniqueOnly)");
			Assert.That (recipients[1], Is.EqualTo (message.Cc.Mailboxes.First ()), "recipients[1] (uniqueOnly)");
			Assert.That (recipients[2], Is.EqualTo (message.Bcc.Mailboxes.First ()), "recipients[2] (uniqueOnly)");

			// Now test the same thing after setting the Resent-* headers...
			message.ResentSender = new MailboxAddress ("Example Resent-Sender", "resent-sender@example.com");
			message.ResentFrom.Add (new MailboxAddress ("Example Resent-From", "resent-from@example.com"));
			message.ResentReplyTo.Add (new MailboxAddress ("Example Resent-Reply-To", "resent-reply-to@example.com"));
			message.ResentTo.Add (new MailboxAddress ("Example Resent-To", "resent-to@example.com"));
			message.ResentTo.Add (new MailboxAddress ("Example Resent-To Duplicate", "resent-to@example.com"));
			message.ResentCc.Add (new MailboxAddress ("Example Resent-Cc", "resent-cc@example.com"));
			message.ResentCc.Add (new MailboxAddress ("Example Resent-Cc Duplicate", "resent-cc@example.com"));
			message.ResentBcc.Add (new MailboxAddress ("Example Resent-Bcc", "resent-bcc@example.com"));
			message.ResentBcc.Add (new MailboxAddress ("Example Resent-Bcc Duplicate", "resent-bcc@example.com"));

			recipients = message.GetRecipients (false);
			Assert.That (recipients.Count, Is.EqualTo (6), "Resent Count");
			Assert.That (recipients[0], Is.EqualTo (message.ResentTo[0]), "Resent recipients[0]");
			Assert.That (recipients[1], Is.EqualTo (message.ResentTo[1]), "Resent recipients[1]");
			Assert.That (recipients[2], Is.EqualTo (message.ResentCc[0]), "Resent recipients[2]");
			Assert.That (recipients[3], Is.EqualTo (message.ResentCc[1]), "Resent recipients[3]");
			Assert.That (recipients[4], Is.EqualTo (message.ResentBcc[0]), "Resent recipients[4]");
			Assert.That (recipients[5], Is.EqualTo (message.ResentBcc[1]), "Resent recipients[5]");

			recipients = message.GetRecipients (true);
			Assert.That (recipients.Count, Is.EqualTo (3), "Resent Count (uniqueOnly)");
			Assert.That (recipients[0], Is.EqualTo (message.ResentTo.Mailboxes.First ()), "Resent recipients[0] (uniqueOnly)");
			Assert.That (recipients[1], Is.EqualTo (message.ResentCc.Mailboxes.First ()), "Resent recipients[1] (uniqueOnly)");
			Assert.That (recipients[2], Is.EqualTo (message.ResentBcc.Mailboxes.First ()), "Resent recipients[2] (uniqueOnly)");
		}

		[Test]
		public void TestSettingCommonInvalidMessageIds ()
		{
			const string msgid = "[d7e8bc604f797c18ba8120250cbd8c04-JFBVALKQOJXWILKCJQZFA7CDNRQXE2LUPF6EIYLUMFGG643TPRCXQ32TNV2HA===@microsoft.com]";
			var message = new MimeMessage ();

			try {
				message.MessageId = msgid;
			} catch (Exception ex) {
				Assert.Fail ($"Failed to set MessageId: {ex.Message}");
			}

			Assert.That (message.MessageId, Is.EqualTo (msgid), "MessageId");

			try {
				message.ResentMessageId = msgid;
			} catch (Exception ex) {
				Assert.Fail ($"Failed to set ResentMessageId: {ex.Message}");
			}

			Assert.That (message.ResentMessageId, Is.EqualTo (msgid), "ResentMessageId");

			try {
				message.InReplyTo = msgid;
			} catch (Exception ex) {
				Assert.Fail ($"Failed to set InReplyTo: {ex.Message}");
			}

			Assert.That (message.InReplyTo, Is.EqualTo (msgid), "InReplyTo");
		}

		[Test]
		public void TestPrependHeader ()
		{
			string rawMessageText = @"Date: Fri, 22 Jan 2016 8:44:05 -0500 (EST)
From: MimeKit Unit Tests <unit.tests@mimekit.org>
To: MimeKit Unit Tests <unit.tests@mimekit.org>
Subject: This is a test off prepending headers.
Message-Id: <id@localhost.com>
MIME-Version: 1.0
Content-Type: text/plain

This is the message body.
".Replace ("\r\n", "\n");
			string expected = "X-Prepended: This is the prepended header\n" + rawMessageText;

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				message.Headers.Insert (0, new Header ("X-Prepended", "This is the prepended header"));

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (expected), "Reserialized message is not identical to the original.");
				}
			}
		}

		[Test]
		public async Task TestReserialization ()
		{
			string rawMessageText = @"X-Andrew-Authenticated-As: 4099;greenbush.galaxy;Nathaniel Borenstein
Received: from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41
          via MS.5.6.greenbush.galaxy.sun4_41;
          Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
Message-ID : <UeCBvVq0M2Yt4oUA83@thumper.bellcore.com>
Date: Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
From: Nathaniel Borenstein <nsb>
X-Andrew-Message-Size: 152+1
MIME-Version: 1.0
Content-Type: multipart/alternative; 
	boundary=""Multipart.Alternative.IeCBvV20M2YtEoUA0A""
To: Ned Freed <ned@innosoft.com>,
    ysato@etl.go.jp (Yutaka Sato =?ISO-2022-JP?B?GyRAOjRGI0stGyhK?= )
Subject: MIME & int'l mail

> THIS IS A MESSAGE IN 'MIME' FORMAT.  Your mail reader does not support MIME.
> Please read the first section, which is plain text, and ignore the rest.

--Multipart.Alternative.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII

In honor of the Communications Week error about MIME's ability to handle
international character sets. a screen dump:

[An Andrew ToolKit view (mailobjv) was included here, but could not be
displayed.]
Just for fun....  -- Nathaniel

--Multipart.Alternative.IeCBvV20M2YtEoUA0A
Content-Type: multipart/mixed; 
	boundary=""Multipart.Mixed.IeCBvV20M2Yt4oU=wd""

--Multipart.Mixed.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

In honor of the <italic>Communications Week</italic> error about MIME's abilit=
y to handle international character sets. a screen dump:<nl>
<nl>

--Multipart.Mixed.IeCBvV20M2Yt4oU=wd
Content-type: image/gif
Content-Transfer-Encoding: base64

R0lGODdhEgLiAKEAAAAAAP///wAA////4CwAAAAAEgLiAAAC/oSPqcvtD6OctNqLs968
...
R+mUIAiVUTmCU0mVJmiVV5mCfaiVQtaUXVlKXwmWZiSWY3lDZWmWIISWaalUWcmW+bWW
b9lAcSmXCUSXdWlKbomX7HWXe4llXOmXQAmYgTmUg0mYRmmYh5mUscGYjemYjwmZkSmZ
k0mZlWmZl4mZqVEAADs=

--Multipart.Mixed.IeCBvV20M2Yt4oU=wd
Content-type: message/rfc822
Content-Description: a message with an mbox marker

From mbox@localhost
Date: Fri, 22 Jan 2016 8:44:05 -0500 (EST)
From: MimeKit Unit Tests <unit.tests@mimekit.org>
To: MimeKit Unit Tests <unit.tests@mimekit.org>
MIME-Version: 1.0
Content-type: text/plain

This is an attached message.

--Multipart.Mixed.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

<nl>
<nl>
Just for fun....  -- Nathaniel<nl>

--Multipart.Alternative.IeCBvV20M2YtEoUA0A--
".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}

				var index = rawMessageText.IndexOf ("\n\n", StringComparison.Ordinal);
				var headersOnly = rawMessageText.Substring (0, index + 2);

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized, true);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (headersOnly), "Reserialized headers are not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized, true);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (headersOnly), "Reserialized headers (async) are not identical to the original.");
				}
			}
		}

		[Test]
		public async Task TestReserializationEmptyParts ()
		{
			string rawMessageText = @"Date: Fri, 22 Jan 2016 8:44:05 -0500 (EST)
From: MimeKit Unit Tests <unit.tests@mimekit.org>
To: MimeKit Unit Tests <unit.tests@mimekit.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; 
	boundary=""Interpart.Boundary.IeCBvV20M2YtEoUA0A""
Subject: Reserialization test of empty mime parts

THIS IS A MESSAGE IN 'MIME' FORMAT.  Your mail reader does not support MIME.
Please read the first section, which is plain text, and ignore the rest.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII

This is the body.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII; name=empty.txt
Content-Description: this part contains no content

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII; name=blank-line.txt
Content-Description: this part contains a single blank line


--Interpart.Boundary.IeCBvV20M2YtEoUA0A--
".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public async Task TestReserializationMessageParts ()
		{
			string rawMessageText = @"Path: flop.mcom.com!news.Stanford.EDU!agate!tcsi.tcs.com!uunet!vixen.cso.uiuc.edu!gateway
From: Internet-Drafts@CNRI.Reston.VA.US
Subject: I-D ACTION:draft-smith-ipatm-bcast-00.txt
Date: 25 Apr 95 15:09:13 GMT
Organization: University of Illinois at Urbana
Lines: 96
Approved: Usenet@ux1.cso.uiuc.edu
Message-ID: <9504251109.aa04587@IETF.CNRI.Reston.VA.US>
Reply-To: Internet-Drafts@CNRI.Reston.VA.US
NNTP-Posting-Host: ux1.cso.uiuc.edu
Mime-Version: 1.0
Content-Type: Multipart/Mixed; Boundary=""NextPart""
Originator: daemon@ux1.cso.uiuc.edu

--NextPart

here are a couple of external bodies:

--NextPart
Content-Type: Multipart/MIXED; Boundary=""OtherAccess""

--OtherAccess
Content-Type:  Message/External-body;
        access-type=""mail-server"";
        server=""mailserv@ds.internic.net""

Content-Type: text/plain
Content-ID: <19950424144009.I-D@CNRI.Reston.VA.US>

ENCODING mime
FILE /internet-drafts/draft-smith-ipatm-bcast-00.txt

--OtherAccess
Content-Type:   Message/External-body;
        name=""draft-smith-ipatm-bcast-00.txt"";
        site=""ds.internic.net"";
        access-type=""anon-ftp"";
        directory=""internet-drafts""

Content-Type: text/plain
Content-ID: <19950424144009.I-D@CNRI.Reston.VA.US>

--OtherAccess
Content-Type: message/external-body;
        access-type=""URL"";
        url=""http://home.netscape.com/
		people/
		jwz/
		index.html""

Content-Type: TEXT/HTML
Content-ID: <spankulate@hubba.hubba.hubba>

--OtherAccess
Content-Type: message/external-body;
        access-type=""local-file"";
        name=""/some/directory/loser.gif""

Content-Type: image/gif
Content-ID: <spankulate3@hubba.hubba.hubba>

--OtherAccess
Content-Type: message/external-body;
        access-type=""afs"";
        name=""/afs/directory/loser.gif""

Content-Type: image/gif
Content-ID: <spankulate4@hubba.hubba.hubba>

--OtherAccess--

--NextPart--
".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public async Task TestReserializationEpilogue ()
		{
			string rawMessageText = @"From: Example Test <test@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed;
   boundary=""simple boundary""

This is the preamble.

--simple boundary
Content-TypeS: text/plain

This is a test.

--simple boundary
Content-Type: text/plain
Content-Disposition: attachment
Content-Transfer-Encoding: 7bit

Another test.

--simple boundary--


This is the epilogue.".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText + "\n"), "Reserialized message is not identical to the original (EnsureNewLine).");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText + "\n"), "Reserialized (async) message is not identical to the original (EnsureNewLine).");
				}
			}
		}

		[Test]
		public async Task TestReserializationMultipartPreambleNoBoundary ()
		{
			string rawMessageText = @"From: Example Test <test@example.com>
Content-Type: multipart/mixed

This is the preamble.
.".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText + "\n"), "Reserialized message is not identical to the original (EnsureNewLine).");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText + "\n"), "Reserialized (async) message is not identical to the original (EnsureNewLine).");
				}
			}
		}

		[Test]
		public async Task TestReserializationInvalidHeaders ()
		{
			string rawMessageText = @"From: Example Test <test@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed;
   boundary=""simple boundary""
Example: test
Test
Test Test
Test:
Test: 
Test: Test
Test Example:

This is the preamble.

--simple boundary
Content-TypeS: text/plain

This is a test.

--simple boundary
Content-Type: text/plain;
Content-Disposition: attachment;
Content-Transfer-Encoding: test;
Content-Transfer-Encoding: binary;
Test Test Test: Test Test
Te$t($)*$= Test Test: Abc def
test test = test
test test :: test
filename=""test.txt""

Another test.

--simple boundary--


This is the epilogue.
".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public async Task TestReserializationDeliveryStatusReportWithEnsureNewLine ()
		{
			string rawMessageText = @"From: est@somwhere.com
Date: Fri, 15 Feb 2019 16:00:08 +0000
Subject: report_with_no_body
To: tom@to.com
MIME-Version: 1.0
Content-Type: multipart/report; report-type=delivery-status; boundary=""A41C7.838631588=_/mm1""


Processing your mail message caused the following errors:

error: err.nosuchuser: newsletter-request@imusic.com

--A41C7.838631588=_/mm1
Content-Type: message/delivery-status

Reporting-MTA: dns; mm1
Arrival-Date: Mon, 29 Jul 1996 02:12:50 -0700

Final-Recipient: RFC822; newsletter-request@imusic.com
Action: failed
Diagnostic-Code: X-LOCAL; 500 (err.nosuchuser)

--A41C7.838631588=_/mm1
Content-Type: message/rfc822

Received: from urchin.netscape.com ([198.95.250.59]) by mm1.sprynet.com with ESMTP id <148217-12799>; Mon, 29 Jul 1996 02:12:50 -0700
Received: from gruntle (gruntle.mcom.com [205.217.230.10]) by urchin.netscape.com (8.7.5/8.7.3) with SMTP id CAA24688 for <newsletter-request@imusic.com>; Mon, 29 Jul 1996 02:04:53 -0700 (PDT)
Sender: jwz@netscape.com
Message-ID: <31FC7EB4.41C6@netscape.com>
Date: Mon, 29 Jul 1996 02:04:52 -0700
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.0b6 (X11; U; IRIX 5.3 IP22)
MIME-Version: 1.0
To: newsletter-request@imusic.com
Subject: unsubscribe
References: <96Jul29.013736-0700pdt.148116-12799+675@mm1.sprynet.com>
Content-Type: text/plain; charset=us-ascii
Content-Transfer-Encoding: 7bit

unsubscribe
--A41C7.838631588=_/mm1--
".Replace ("\r\n", "\n");

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					message.WriteTo (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;
					options.EnsureNewLine = true;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.That (result, Is.EqualTo (rawMessageText), "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public void TestMailMessageToMimeMessage ()
		{
			var mail = new MailMessage ();
			mail.Sender = new MailAddress ("sender@sender.com", "The Real Sender");
			mail.From = new MailAddress ("from@from.com", "From Whence it Came");
			mail.ReplyToList.Add (new MailAddress ("reply-to@reply-to.com"));
			mail.To.Add (new MailAddress ("to@to.com", "The Primary Recipient"));
			mail.CC.Add (new MailAddress ("cc@cc.com", "The Carbon-Copied Recipient"));
			mail.Bcc.Add (new MailAddress ("bcc@bcc.com", "The Blind Carbon-Copied Recipient"));
			mail.Subject = "This is the message subject";
			mail.Priority = MailPriority.High;
			mail.Headers.Add ("X-MimeKit-Test", "does this get copied, too?");
			mail.Body = "This is plain text.";

			//var text = new MemoryStream (Encoding.ASCII.GetBytes ("This is plain text."), false);
			//mail.AlternateViews.Add (new AlternateView (text, "text/plain"));

			var html = new MemoryStream (Encoding.ASCII.GetBytes ("This is HTML."), false);
			var view = new AlternateView (html, "text/html");

			var imageData = new byte[1024];
			var image = new MemoryStream (imageData, false);
			view.LinkedResources.Add (new LinkedResource (image, "image/jpeg") { ContentId = "id@jpeg", ContentLink = new Uri ("link", UriKind.Relative) });
			view.BaseUri = new Uri ("http://example.com");

			mail.AlternateViews.Add (view);

			mail.Attachments.Add (new Attachment (new MemoryStream (imageData, false), "empty.jpeg", "image/jpeg"));

			var message = (MimeMessage) mail;

			Assert.That (message.Sender.Name, Is.EqualTo (mail.Sender.DisplayName), "The sender names do not match.");
			Assert.That (message.Sender.Address, Is.EqualTo (mail.Sender.Address), "The sender addresses do not match.");
			Assert.That (message.From[0].Name, Is.EqualTo (mail.From.DisplayName), "The from names do not match.");
			Assert.That (((MailboxAddress) message.From[0]).Address, Is.EqualTo (mail.From.Address), "The from addresses do not match.");
			Assert.That (message.ReplyTo[0].Name, Is.EqualTo (mail.ReplyToList[0].DisplayName), "The reply-to names do not match.");
			Assert.That (((MailboxAddress) message.ReplyTo[0]).Address, Is.EqualTo (mail.ReplyToList[0].Address), "The reply-to addresses do not match.");
			Assert.That (message.To[0].Name, Is.EqualTo (mail.To[0].DisplayName), "The to names do not match.");
			Assert.That (((MailboxAddress) message.To[0]).Address, Is.EqualTo (mail.To[0].Address), "The to addresses do not match.");
			Assert.That (message.Cc[0].Name, Is.EqualTo (mail.CC[0].DisplayName), "The cc names do not match.");
			Assert.That (((MailboxAddress) message.Cc[0]).Address, Is.EqualTo (mail.CC[0].Address), "The cc addresses do not match.");
			Assert.That (message.Bcc[0].Name, Is.EqualTo (mail.Bcc[0].DisplayName), "The bcc names do not match.");
			Assert.That (((MailboxAddress) message.Bcc[0]).Address, Is.EqualTo (mail.Bcc[0].Address), "The bcc addresses do not match.");
			Assert.That (message.Subject, Is.EqualTo (mail.Subject), "The message subjects do not match.");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Urgent), "The message priority does not match.");
			Assert.That (message.Headers["X-MimeKit-Test"], Is.EqualTo (mail.Headers["X-MimeKit-Test"]), "The X-MimeKit-Test headers do not match");
			Assert.That (message.Body, Is.InstanceOf<Multipart> (), "The top-level MIME part should be a multipart/mixed.");

			var mixed = (Multipart) message.Body;

			Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"), "The top-level MIME part should be a multipart/mixed.");
			Assert.That (mixed.Count, Is.EqualTo (2), "Expected 2 MIME parts within the multipart/mixed");
			Assert.That (mixed[0], Is.InstanceOf<MultipartAlternative> (), "Expected the first part the multipart/mixed to be a multipart/alternative");
			Assert.That (mixed[1], Is.InstanceOf<MimePart> (), "Expected the first part the multipart/mixed to be a MimePart");

			var attachment = (MimePart) mixed[1];
			Assert.That (attachment.FileName, Is.EqualTo ("empty.jpeg"), "Expected the attachment to have a filename");

			var alternative = (MultipartAlternative) mixed[0];

			Assert.That (alternative.Count, Is.EqualTo (2), "Expected 2 MIME parts within the multipart/alternative.");
			Assert.That (alternative[1] is MultipartRelated, Is.True, "The second MIME part should be a multipart/related.");

			var related = (MultipartRelated) alternative[1];

			Assert.That (related.Count, Is.EqualTo (2), "Expected 2 MIME parts within the multipart/related.");
			Assert.That (related.ContentBase.ToString (), Is.EqualTo ("http://example.com/"));
			Assert.That (related[0] is TextPart, Is.True, "The first part of the multipart/related should be the html part");
			Assert.That (((TextPart) related[0]).ContentLocation, Is.Null);
			Assert.That (((TextPart) related[0]).ContentBase, Is.Null);

			var jpeg = (MimePart) related[1];
			Assert.That (jpeg.ContentId, Is.EqualTo ("id@jpeg"));
			Assert.That (jpeg.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (jpeg.ContentLocation.OriginalString, Is.EqualTo ("link"));

			// Test other priorities
			mail.Priority = MailPriority.Low;
			message = (MimeMessage) mail;

			Assert.That (message.Priority, Is.EqualTo (MessagePriority.NonUrgent), "The message priority does not match.");

			mail.Priority = MailPriority.Normal;
			message = (MimeMessage) mail;

			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal), "The message priority does not match.");
		}

		[Test]
		public void TestIssue135 ()
		{
			var message = new MimeMessage {
				Body = new TextPart ("plain") {
					ContentTransferEncoding = ContentEncoding.Base64,
					Content = new MimeContent (new MemoryStream (new byte[1], false))
				}
			};

			try {
				message.ToString ();
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			}
		}

		[Test]
		public void TestImportance ()
		{
			var message = new MimeMessage (new [] { new MailboxAddress ("Example Sender", "sender@example.com") },
				new [] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));

			// Note: setting to normal should not change anything
			message.Importance = MessageImportance.Normal;
			Assert.That (message.Headers.IndexOf (HeaderId.Importance), Is.EqualTo (-1));

			message.Importance = MessageImportance.Low;
			value = message.Headers[HeaderId.Importance];
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Low));
			Assert.That (value, Is.EqualTo ("low"));

			message.Importance = MessageImportance.High;
			value = message.Headers[HeaderId.Importance];
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.High));
			Assert.That (value, Is.EqualTo ("high"));

			message.Importance = MessageImportance.Normal;
			value = message.Headers[HeaderId.Importance];
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));
			Assert.That (value, Is.EqualTo ("normal"));

			message.Headers[HeaderId.Importance] = "high";
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.High));

			message.Headers[HeaderId.Importance] = "low";
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Low));

			message.Headers.Remove (HeaderId.Importance);
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));
		}

		[Test]
		public void TestPriority ()
		{
			var message = new MimeMessage (new [] { new MailboxAddress ("Example Sender", "sender@example.com") },
				new [] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));

			// Note: setting to normal should not change anything
			message.Priority = MessagePriority.Normal;
			Assert.That (message.Headers.IndexOf (HeaderId.Priority), Is.EqualTo (-1));

			message.Priority = MessagePriority.NonUrgent;
			value = message.Headers[HeaderId.Priority];
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.NonUrgent));
			Assert.That (value, Is.EqualTo ("non-urgent"));

			message.Priority = MessagePriority.Urgent;
			value = message.Headers[HeaderId.Priority];
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Urgent));
			Assert.That (value, Is.EqualTo ("urgent"));

			message.Priority = MessagePriority.Normal;
			value = message.Headers[HeaderId.Priority];
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));
			Assert.That (value, Is.EqualTo ("normal"));

			message.Headers[HeaderId.Priority] = "non-urgent";
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.NonUrgent));

			message.Headers[HeaderId.Priority] = "urgent";
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Urgent));

			message.Headers.Remove (HeaderId.Priority);
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));
		}

		[Test]
		public void TestXPriority ()
		{
			var message = new MimeMessage (new[] { new MailboxAddress ("Example Sender", "sender@example.com") },
				new[] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Normal));

			// Note: setting to normal should not change anything
			message.XPriority = XMessagePriority.Normal;
			Assert.That (message.Headers.IndexOf (HeaderId.XPriority), Is.EqualTo (-1));

			message.XPriority = XMessagePriority.Lowest;
			value = message.Headers[HeaderId.XPriority];
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Lowest));
			Assert.That (value, Is.EqualTo ("5 (Lowest)"));

			message.XPriority = XMessagePriority.Low;
			value = message.Headers[HeaderId.XPriority];
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Low));
			Assert.That (value, Is.EqualTo ("4 (Low)"));

			message.XPriority = XMessagePriority.Normal;
			value = message.Headers[HeaderId.XPriority];
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Normal));
			Assert.That (value, Is.EqualTo ("3 (Normal)"));

			message.XPriority = XMessagePriority.High;
			value = message.Headers[HeaderId.XPriority];
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.High));
			Assert.That (value, Is.EqualTo ("2 (High)"));

			message.XPriority = XMessagePriority.Highest;
			value = message.Headers[HeaderId.XPriority];
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Highest));
			Assert.That (value, Is.EqualTo ("1 (Highest)"));

			message.Headers[HeaderId.XPriority] = "5";
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Lowest));

			message.Headers[HeaderId.XPriority] = "4";
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Low));

			message.Headers[HeaderId.XPriority] = "3";
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Normal));

			message.Headers[HeaderId.XPriority] = "2";
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.High));

			message.Headers[HeaderId.XPriority] = "1";
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Highest));

			message.Headers.Remove (HeaderId.XPriority);
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Normal));

			message.Headers.Add (HeaderId.XPriority, "garbage");
			Assert.That (message.XPriority, Is.EqualTo (XMessagePriority.Normal));
		}

		[Test]
		public void TestResend ()
		{
			var message = new MimeMessage (new [] { new MailboxAddress ("Example From", "from@example.com") },
				new [] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			message.Date = new DateTimeOffset (1997, 6, 28, 12, 47, 52, new TimeSpan (-5, 0, 0));
			message.ReplyTo.Add (new MailboxAddress ("Example Reply-To", "reply-to@example.com"));
			message.Sender = new MailboxAddress ("Example Sender", "sender@example.com");
			message.MessageId = MimeUtils.GenerateMessageId ();

			message.ResentSender = new MailboxAddress ("Resent Sender", "resent-sender@example.com");
			message.ResentFrom.Add (new MailboxAddress ("Resent From", "resent-from@example.com"));
			message.ResentReplyTo.Add (new MailboxAddress ("Resent Reply-To", "resent-reply-to@example.com"));
			message.ResentTo.Add (new MailboxAddress ("Resent To", "resent-to@example.com"));
			message.ResentCc.Add (new MailboxAddress ("Resent Cc", "resent-cc@example.com"));
			message.ResentBcc.Add (new MailboxAddress ("Resent Bcc", "resent-bcc@example.com"));
			message.ResentDate = new DateTimeOffset (2007, 6, 28, 12, 47, 52, new TimeSpan (-5, 0, 0));
			message.ResentMessageId = value = MimeUtils.GenerateMessageId ();

			Assert.That (message.Headers[HeaderId.ResentSender], Is.EqualTo ("Resent Sender <resent-sender@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentFrom], Is.EqualTo ("Resent From <resent-from@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentReplyTo], Is.EqualTo ("Resent Reply-To <resent-reply-to@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentTo], Is.EqualTo ("Resent To <resent-to@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentCc], Is.EqualTo ("Resent Cc <resent-cc@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentBcc], Is.EqualTo ("Resent Bcc <resent-bcc@example.com>"));
			Assert.That (message.Headers[HeaderId.ResentDate], Is.EqualTo ("Thu, 28 Jun 2007 12:47:52 -0500"));
			Assert.That (message.Headers[HeaderId.ResentMessageId], Is.EqualTo ("<" + value + ">"));
		}

		[Test]
		public void TestChangeHeaders ()
		{
			const string addressList1 = "\"Example 1\" <example1@example.com>, \"Example 2\" <example2@example.com>";
			const string addressList2 = "\"Example 3\" <example3@example.com>, \"Example 4\" <example4@example.com>";
			const string references1 = "<id1@example.com> <id2@example.com>";
			const string references2 = "<id3@example.com> <id4@example.com>";
			const string mailbox1 = "\"Example 1\" <example1@example.com>";
			const string mailbox2 = "\"Example 2\" <example2@example.com>";
			const string date1 = "Thu, 28 Jun 2007 12:47:52 -0500";
			const string date2 = "Fri, 29 Jun 2007 12:47:52 -0500";
			const string msgid1 = "message-id1@example.com";
			const string msgid2 = "message-id2@example.com";
			const string version1 = "1.0";
			const string version2 = "2.0";
			var message = new MimeMessage ();

			foreach (var property in message.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				var getter = property.GetGetMethod ();
				var setter = property.GetSetMethod ();
				DateTimeOffset date;
				Version version;
				object value;
				HeaderId id;

				if (!Enum.TryParse (property.Name, out id))
					continue;

				switch (property.PropertyType.FullName) {
				case "MimeKit.InternetAddressList":
					message.Headers[id] = addressList1;

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (addressList1), $"Unexpected result when setting {property.Name} to addressList1");

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, addressList2);

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (addressList2), $"Unexpected result when setting {property.Name} to addressList2");
					break;
				case "MimeKit.MailboxAddress":
					message.Headers[id] = mailbox1;

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (mailbox1), $"Unexpected result when setting {property.Name} to mailbox1");

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, mailbox2);

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (mailbox2), $"Unexpected result when setting {property.Name} to mailbox2");

					setter.Invoke (message, new object[] { null });
					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value, Is.Null, $"Expected null value after setting {property.Name} to null.");
					Assert.That (message.Headers.IndexOf (id), Is.EqualTo (-1), $"Expected {property.Name} header to be removed after setting it to null.");
					break;
				case "MimeKit.MessageIdList":
					message.Headers[id] = references1;

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (references1), $"Unexpected result when setting {property.Name} to references1");

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, references2);

					value = getter.Invoke (message, Array.Empty<object> ());
					Assert.That (value.ToString (), Is.EqualTo (references2), $"Unexpected result when setting {property.Name} to references2");
					break;
				case "System.DateTimeOffset":
					message.Headers[id] = date1;

					date = (DateTimeOffset) getter.Invoke (message, Array.Empty<object> ());
					Assert.That (DateUtils.FormatDate (date), Is.EqualTo (date1), $"Unexpected result when setting {property.Name} to date1");

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, date2);

					date = (DateTimeOffset) getter.Invoke (message, Array.Empty<object> ());
					Assert.That (DateUtils.FormatDate (date), Is.EqualTo (date2), $"Unexpected result when setting {property.Name} to date2");

					setter.Invoke (message, new object[] { date });
					break;
				case "System.Version":
					message.Headers[id] = version1;

					version = (Version) getter.Invoke (message, Array.Empty<object> ());
					Assert.That (version.ToString (), Is.EqualTo (version1), $"Unexpected result when setting {property.Name} to version1");

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, version2);

					version = (Version) getter.Invoke (message, Array.Empty<object> ());
					Assert.That (version.ToString (), Is.EqualTo (version2), $"Unexpected result when setting {property.Name} to version2");

					setter.Invoke (message, new object[] { version });
					break;
				case "System.String":
					switch (id) {
					case HeaderId.ResentMessageId:
					case HeaderId.MessageId:
					case HeaderId.InReplyTo:
						message.Headers[id] = "<" + msgid1 + ">";

						value = getter.Invoke (message, Array.Empty<object> ());
						Assert.That (value.ToString (), Is.EqualTo (msgid1), $"Unexpected result when setting {property.Name} to msgid1");

						message.Headers[message.Headers.IndexOf (id)] = new Header (id, "<" + msgid2 + ">");

						value = getter.Invoke (message, Array.Empty<object> ());
						Assert.That (value.ToString (), Is.EqualTo (msgid2), $"Unexpected result when setting {property.Name} to msgid2");

						setter.Invoke (message, new object[] { msgid1 });

						setter.Invoke (message, new object[] { "<" + msgid1 + ">" });
						value = getter.Invoke (message, Array.Empty<object> ());
						Assert.That (value.ToString (), Is.EqualTo (msgid1), $"Unexpected result when setting {property.Name} to msgid1 via the setter.");

						if (id == HeaderId.InReplyTo) {
							setter.Invoke (message, new object[] { null });
							value = getter.Invoke (message, Array.Empty<object> ());
							Assert.That (value, Is.Null, $"Expected null value after setting {property.Name} to null.");
							Assert.That (message.Headers.IndexOf (id), Is.EqualTo (-1), $"Expected {property.Name} header to be removed after setting it to null.");
						}
						break;
					case HeaderId.Subject:
						message.Headers[id] = "Subject #1";

						value = getter.Invoke (message, Array.Empty<object> ());
						Assert.That (value.ToString (), Is.EqualTo ("Subject #1"), $"Unexpected result when setting {property.Name} to subject1");

						message.Headers[message.Headers.IndexOf (id)] = new Header (id, "Subject #2");

						value = getter.Invoke (message, Array.Empty<object> ());
						Assert.That (value.ToString (), Is.EqualTo ("Subject #2"), $"Unexpected result when setting {property.Name} to msgid2");
						break;
					}
					break;
				}
			}
		}

		[Test]
		public void TestImportanceChanged ()
		{
			var message = new MimeMessage ();

			message.Headers.Add (HeaderId.Importance, "high");
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.High));

			message.Headers.Remove (HeaderId.Importance);
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));

			message.Headers.Add (HeaderId.Importance, "low");
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Low));

			message.Headers.Remove (HeaderId.Importance);
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));

			message.Headers.Add (HeaderId.Importance, "normal");
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));

			message.Headers.Remove (HeaderId.Importance);
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));

			message.Headers.Add (HeaderId.Importance, "invalid-value");
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));
		}

		[Test]
		public void TestPriorityChanged ()
		{
			var message = new MimeMessage ();

			message.Headers.Add (HeaderId.Priority, "urgent");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Urgent));

			message.Headers.Remove (HeaderId.Priority);
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));

			message.Headers.Add (HeaderId.Priority, "non-urgent");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.NonUrgent));

			message.Headers.Remove (HeaderId.Priority);
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));

			message.Headers.Add (HeaderId.Priority, "normal");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));

			message.Headers.Remove (HeaderId.Priority);
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));

			message.Headers.Add (HeaderId.Priority, "invalid-value");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));
		}

		[Test]
		public void TestReferencesChanged ()
		{
			var message = new MimeMessage ();
			Header references;

			message.Headers.Add (HeaderId.References, "<id1@localhost> <id2@localhost>");
			Assert.That (message.References.Count, Is.EqualTo (2), "The number of references does not match.");
			Assert.That (message.References[0], Is.EqualTo ("id1@localhost"), "The first references does not match.");
			Assert.That (message.References[1], Is.EqualTo ("id2@localhost"), "The second references does not match.");

			message.References.Add ("id3@localhost");

			Assert.That (message.Headers.TryGetHeader ("References", out references), Is.True, "Failed to get References header.");
			Assert.That (references.Value, Is.EqualTo ("<id1@localhost> <id2@localhost> <id3@localhost>"), "The modified Reference header does not match.");

			message.References.Clear ();

			Assert.That (message.Headers.TryGetHeader ("References", out _), Is.False, "References header should have been removed.");
		}

		[Test]
		public void TestClearHeaders ()
		{
			var message = new MimeMessage ();

			message.Subject = "Clear the headers!";

			message.Sender = new MailboxAddress ("Sender", "sender@sender.com");
			message.ReplyTo.Add (new MailboxAddress ("Reply-To", "reply-to@reply-to.com"));
			message.From.Add (new MailboxAddress ("From", "from@from.com"));
			message.To.Add (new MailboxAddress ("To", "to@to.com"));
			message.Cc.Add (new MailboxAddress ("Cc", "cc@cc.com"));
			message.Bcc.Add (new MailboxAddress ("Bcc", "bcc@bcc.com"));
			message.MessageId = MimeUtils.GenerateMessageId ();
			message.Date = DateTimeOffset.Now;

			message.ResentSender = new MailboxAddress ("Sender", "sender@sender.com");
			message.ResentReplyTo.Add (new MailboxAddress ("Reply-To", "reply-to@reply-to.com"));
			message.ResentFrom.Add (new MailboxAddress ("From", "from@from.com"));
			message.ResentTo.Add (new MailboxAddress ("To", "to@to.com"));
			message.ResentCc.Add (new MailboxAddress ("Cc", "cc@cc.com"));
			message.ResentBcc.Add (new MailboxAddress ("Bcc", "bcc@bcc.com"));
			message.ResentMessageId = MimeUtils.GenerateMessageId ();
			message.ResentDate = DateTimeOffset.Now;

			message.Importance = MessageImportance.High;
			message.Priority = MessagePriority.Urgent;

			message.References.Add ("<id1@localhost>");
			message.InReplyTo = "<id1@localhost>";

			message.MimeVersion = new Version (1, 0);

			message.Headers.Clear ();

			Assert.That (message.Subject, Is.Null, "Subject has not been cleared.");

			Assert.That (message.Sender, Is.Null, "Sender has not been cleared.");
			Assert.That (message.ReplyTo.Count, Is.EqualTo (0), "Reply-To has not been cleared.");
			Assert.That (message.From.Count, Is.EqualTo (0), "From has not been cleared.");
			Assert.That (message.To.Count, Is.EqualTo (0), "To has not been cleared.");
			Assert.That (message.Cc.Count, Is.EqualTo (0), "Cc has not been cleared.");
			Assert.That (message.Bcc.Count, Is.EqualTo (0), "Bcc has not been cleared.");
			Assert.That (message.MessageId, Is.Null, "Message-Id has not been cleared.");
			Assert.That (message.Date, Is.EqualTo (DateTimeOffset.MinValue), "Date has not been cleared.");

			Assert.That (message.ResentSender, Is.Null, "Resent-Sender has not been cleared.");
			Assert.That (message.ResentReplyTo.Count, Is.EqualTo (0), "Resent-Reply-To has not been cleared.");
			Assert.That (message.ResentFrom.Count, Is.EqualTo (0), "Resent-From has not been cleared.");
			Assert.That (message.ResentTo.Count, Is.EqualTo (0), "Resent-To has not been cleared.");
			Assert.That (message.ResentCc.Count, Is.EqualTo (0), "Resent-Cc has not been cleared.");
			Assert.That (message.ResentBcc.Count, Is.EqualTo (0), "Resent-Bcc has not been cleared.");
			Assert.That (message.ResentMessageId, Is.Null, "Resent-Message-Id has not been cleared.");
			Assert.That (message.ResentDate, Is.EqualTo (DateTimeOffset.MinValue), "Resent-Date has not been cleared.");

			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal), "Importance has not been cleared.");
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal), "Priority has not been cleared.");

			Assert.That (message.References.Count, Is.EqualTo (0), "References has not been cleared.");
			Assert.That (message.InReplyTo, Is.Null, "In-Reply-To has not been cleared.");

			Assert.That (message.MimeVersion, Is.Null, "MIME-Version has not been cleared.");
		}

		[Test]
		public void TestHtmlAndTextBodies ()
		{
			const string HtmlBody = "<html>This is an <b>html</b> body.</html>";
			const string TextBody = "This is the text body.";
			MimeMessage message;

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.1.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (null), "The HTML bodies do not match for body.1.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.2.txt"));
			Assert.That (message.TextBody, Is.EqualTo (null), "The text bodies do not match for body.2.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.2.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.3.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.3.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.3.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.4.txt"));
			Assert.That (message.TextBody, Is.EqualTo (null), "The text bodies do not match for body.4.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.4.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.5.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.5.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.5.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.6.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.6.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.6.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.7.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.7.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.7.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.8.txt"));
			Assert.That (message.TextBody, Is.EqualTo (TextBody), "The text bodies do not match for body.8.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (null), "The HTML bodies do not match for body.8.txt.");

			message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.9.txt"));
			Assert.That (message.TextBody, Is.EqualTo (null), "The text bodies do not match for body.9.txt.");
			Assert.That (message.HtmlBody, Is.EqualTo (HtmlBody), "The HTML bodies do not match for body.9.txt.");
		}

		[Test]
		public void TestNoBodyWithTextAttachment ()
		{
			const string rawMessageText = @"From: sender@domain.com
Date: Tue, 29 Aug 2017 09:45:39 +1000
Subject: This has no body, just a text attachment
Message-Id: <75SXBEJJ72U4.5KFFZ6J56L2T2@localhost.localdomain>
MIME-Version: 1.0
Content-Type: text/plain; name=""Plain Text.txt""
Content-Disposition: attachment; filename=""Plain Text.txt""
Content-Transfer-Encoding: 7bit

This is the text attachment";

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				Assert.That (message.TextBody, Is.Null, "Message text should be blank, as no body defined");
				Assert.That (message.Attachments.OfType<TextPart> ().Count (), Is.EqualTo (1), "Message should contain one text attachment");
			}
		}
	}
}
