//
// MimeMessageTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Reflection;

using NUnit.Framework;

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

			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((Stream) null, true));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, Stream.Null, true));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (Stream) null, true));

			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync ((string) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (null, "fileName"));
			Assert.Throws<ArgumentNullException> (async () => await MimeMessage.LoadAsync (ParserOptions.Default, (string) null));

			Assert.Throws<ArgumentNullException> (() => message.Accept (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => message.Prepare (EncodingConstraint.None, 10));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo ((string) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => message.WriteTo (FormatOptions.Default, (string) null));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync ((string) null));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync ((Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync (null, "fileName"));
			Assert.Throws<ArgumentNullException> (async () => await message.WriteToAsync (FormatOptions.Default, (string) null));
			Assert.Throws<ArgumentNullException> (() => message.Sign (null));
			Assert.Throws<ArgumentNullException> (() => message.Sign (null, DigestAlgorithm.Sha1));
			Assert.Throws<ArgumentNullException> (() => message.Encrypt (null));
			Assert.Throws<ArgumentNullException> (() => message.SignAndEncrypt (null));
		}

		[Test]
		public async void TestReserialization ()
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

					Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.AreEqual (rawMessageText, result, "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public async void TestReserializationEmptyParts ()
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

					Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.AreEqual (rawMessageText, result, "Reserialized (async) message is not identical to the original.");
				}
			}
		}

		[Test]
		public async void TestReserializationMessageParts ()
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

					Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
				}

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					await message.WriteToAsync (options, serialized);

					var result = Encoding.UTF8.GetString (serialized.ToArray ());

					Assert.AreEqual (rawMessageText, result, "Reserialized (async) message is not identical to the original.");
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

			Assert.AreEqual (mail.Sender.DisplayName, message.Sender.Name, "The sender names do not match.");
			Assert.AreEqual (mail.Sender.Address, message.Sender.Address, "The sender addresses do not match.");
			Assert.AreEqual (mail.From.DisplayName, message.From[0].Name, "The from names do not match.");
			Assert.AreEqual (mail.From.Address, ((MailboxAddress) message.From[0]).Address, "The from addresses do not match.");
			Assert.AreEqual (mail.ReplyToList[0].DisplayName, message.ReplyTo[0].Name, "The reply-to names do not match.");
			Assert.AreEqual (mail.ReplyToList[0].Address, ((MailboxAddress) message.ReplyTo[0]).Address, "The reply-to addresses do not match.");
			Assert.AreEqual (mail.To[0].DisplayName, message.To[0].Name, "The to names do not match.");
			Assert.AreEqual (mail.To[0].Address, ((MailboxAddress) message.To[0]).Address, "The to addresses do not match.");
			Assert.AreEqual (mail.CC[0].DisplayName, message.Cc[0].Name, "The cc names do not match.");
			Assert.AreEqual (mail.CC[0].Address, ((MailboxAddress) message.Cc[0]).Address, "The cc addresses do not match.");
			Assert.AreEqual (mail.Bcc[0].DisplayName, message.Bcc[0].Name, "The bcc names do not match.");
			Assert.AreEqual (mail.Bcc[0].Address, ((MailboxAddress) message.Bcc[0]).Address, "The bcc addresses do not match.");
			Assert.AreEqual (mail.Subject, message.Subject, "The message subjects do not match.");
			Assert.AreEqual (MessagePriority.Urgent, message.Priority, "The message priority does not match.");
			Assert.IsInstanceOf<Multipart> (message.Body, "The top-level MIME part should be a multipart/mixed.");

			var mixed = (Multipart) message.Body;

			Assert.AreEqual ("multipart/mixed", mixed.ContentType.MimeType, "The top-level MIME part should be a multipart/mixed.");
			Assert.AreEqual (2, mixed.Count, "Expected 2 MIME parts within the multipart/mixed");
			Assert.IsInstanceOf<MultipartAlternative> (mixed[0], "Expected the first part the multipart/mixed to be a multipart/alternative");
			Assert.IsInstanceOf<MimePart> (mixed[1], "Expected the first part the multipart/mixed to be a MimePart");

			var attachment = (MimePart) mixed[1];
			Assert.AreEqual ("empty.jpeg", attachment.FileName, "Expected the attachment to have a filename");

			var alternative = (MultipartAlternative) mixed[0];

			Assert.AreEqual (2, alternative.Count, "Expected 2 MIME parts within the multipart/alternative.");
			Assert.IsTrue (alternative[1] is MultipartRelated, "The second MIME part should be a multipart/related.");

			var related = (MultipartRelated) alternative[1];

			Assert.AreEqual (2, related.Count, "Expected 2 MIME parts within the multipart/related.");
			Assert.AreEqual ("http://example.com/", related.ContentBase.ToString ());
			Assert.IsTrue (related[0] is TextPart, "The first part of the multipart/related should be the html part");
			Assert.IsNull (((TextPart) related[0]).ContentLocation);
			Assert.IsNull (((TextPart) related[0]).ContentBase);

			var jpeg = (MimePart) related[1];
			Assert.AreEqual ("id@jpeg", jpeg.ContentId);
			Assert.AreEqual ("image/jpeg", jpeg.ContentType.MimeType);
			Assert.AreEqual ("link", jpeg.ContentLocation.OriginalString);
		}

		[Test]
		public void TestIssue135 ()
		{
			var message = new MimeMessage ();
			message.Body = new TextPart ("plain") {
				ContentTransferEncoding = ContentEncoding.Base64,
				Content = new MimeContent (new MemoryStream (new byte[1], false))
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

			Assert.AreEqual (MessageImportance.Normal, message.Importance);

			// Note: setting to normal should not change anything
			message.Importance = MessageImportance.Normal;
			Assert.AreEqual (-1, message.Headers.IndexOf (HeaderId.Importance));

			message.Importance = MessageImportance.Low;
			value = message.Headers[HeaderId.Importance];
			Assert.AreEqual (MessageImportance.Low, message.Importance);
			Assert.AreEqual ("low", value);

			message.Importance = MessageImportance.High;
			value = message.Headers[HeaderId.Importance];
			Assert.AreEqual (MessageImportance.High, message.Importance);
			Assert.AreEqual ("high", value);

			message.Importance = MessageImportance.Normal;
			value = message.Headers[HeaderId.Importance];
			Assert.AreEqual (MessageImportance.Normal, message.Importance);
			Assert.AreEqual ("normal", value);

			message.Headers[HeaderId.Importance] = "high";
			Assert.AreEqual (MessageImportance.High, message.Importance);

			message.Headers[HeaderId.Importance] = "low";
			Assert.AreEqual (MessageImportance.Low, message.Importance);

			message.Headers.Remove (HeaderId.Importance);
			Assert.AreEqual (MessageImportance.Normal, message.Importance);
		}

		[Test]
		public void TestPriority ()
		{
			var message = new MimeMessage (new [] { new MailboxAddress ("Example Sender", "sender@example.com") },
				new [] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			Assert.AreEqual (MessagePriority.Normal, message.Priority);

			// Note: setting to normal should not change anything
			message.Priority = MessagePriority.Normal;
			Assert.AreEqual (-1, message.Headers.IndexOf (HeaderId.Priority));

			message.Priority = MessagePriority.NonUrgent;
			value = message.Headers[HeaderId.Priority];
			Assert.AreEqual (MessagePriority.NonUrgent, message.Priority);
			Assert.AreEqual ("non-urgent", value);

			message.Priority = MessagePriority.Urgent;
			value = message.Headers[HeaderId.Priority];
			Assert.AreEqual (MessagePriority.Urgent, message.Priority);
			Assert.AreEqual ("urgent", value);

			message.Priority = MessagePriority.Normal;
			value = message.Headers[HeaderId.Priority];
			Assert.AreEqual (MessagePriority.Normal, message.Priority);
			Assert.AreEqual ("normal", value);

			message.Headers[HeaderId.Priority] = "non-urgent";
			Assert.AreEqual (MessagePriority.NonUrgent, message.Priority);

			message.Headers[HeaderId.Priority] = "urgent";
			Assert.AreEqual (MessagePriority.Urgent, message.Priority);

			message.Headers.Remove (HeaderId.Priority);
			Assert.AreEqual (MessagePriority.Normal, message.Priority);
		}

		[Test]
		public void TestXPriority ()
		{
			var message = new MimeMessage (new[] { new MailboxAddress ("Example Sender", "sender@example.com") },
				new[] { new MailboxAddress ("Example Recipient", "recipient@example.com") },
				"Yo dawg, what up?",
				new TextPart { Text = "Hey! What's happenin'?" });
			string value;

			Assert.AreEqual (XMessagePriority.Normal, message.XPriority);

			// Note: setting to normal should not change anything
			message.XPriority = XMessagePriority.Normal;
			Assert.AreEqual (-1, message.Headers.IndexOf (HeaderId.XPriority));

			message.XPriority = XMessagePriority.Lowest;
			value = message.Headers[HeaderId.XPriority];
			Assert.AreEqual (XMessagePriority.Lowest, message.XPriority);
			Assert.AreEqual ("5 (Lowest)", value);

			message.XPriority = XMessagePriority.Low;
			value = message.Headers[HeaderId.XPriority];
			Assert.AreEqual (XMessagePriority.Low, message.XPriority);
			Assert.AreEqual ("4 (Low)", value);

			message.XPriority = XMessagePriority.Normal;
			value = message.Headers[HeaderId.XPriority];
			Assert.AreEqual (XMessagePriority.Normal, message.XPriority);
			Assert.AreEqual ("3 (Normal)", value);

			message.XPriority = XMessagePriority.High;
			value = message.Headers[HeaderId.XPriority];
			Assert.AreEqual (XMessagePriority.High, message.XPriority);
			Assert.AreEqual ("2 (High)", value);

			message.XPriority = XMessagePriority.Highest;
			value = message.Headers[HeaderId.XPriority];
			Assert.AreEqual (XMessagePriority.Highest, message.XPriority);
			Assert.AreEqual ("1 (Highest)", value);

			message.Headers[HeaderId.XPriority] = "5";
			Assert.AreEqual (XMessagePriority.Lowest, message.XPriority);

			message.Headers[HeaderId.XPriority] = "4";
			Assert.AreEqual (XMessagePriority.Low, message.XPriority);

			message.Headers[HeaderId.XPriority] = "3";
			Assert.AreEqual (XMessagePriority.Normal, message.XPriority);

			message.Headers[HeaderId.XPriority] = "2";
			Assert.AreEqual (XMessagePriority.High, message.XPriority);

			message.Headers[HeaderId.XPriority] = "1";
			Assert.AreEqual (XMessagePriority.Highest, message.XPriority);

			message.Headers.Remove (HeaderId.XPriority);
			Assert.AreEqual (XMessagePriority.Normal, message.XPriority);
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

			Assert.AreEqual ("Resent Sender <resent-sender@example.com>", message.Headers[HeaderId.ResentSender]);
			Assert.AreEqual ("Resent From <resent-from@example.com>", message.Headers[HeaderId.ResentFrom]);
			Assert.AreEqual ("Resent Reply-To <resent-reply-to@example.com>", message.Headers[HeaderId.ResentReplyTo]);
			Assert.AreEqual ("Resent To <resent-to@example.com>", message.Headers[HeaderId.ResentTo]);
			Assert.AreEqual ("Resent Cc <resent-cc@example.com>", message.Headers[HeaderId.ResentCc]);
			Assert.AreEqual ("Resent Bcc <resent-bcc@example.com>", message.Headers[HeaderId.ResentBcc]);
			Assert.AreEqual ("Thu, 28 Jun 2007 12:47:52 -0500", message.Headers[HeaderId.ResentDate]);
			Assert.AreEqual ("<" + value + ">", message.Headers[HeaderId.ResentMessageId]);
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

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (addressList1, value.ToString (), "Unexpected result when setting {0} to addressList1", property.Name);

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, addressList2);

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (addressList2, value.ToString (), "Unexpected result when setting {0} to addressList2", property.Name);
					break;
				case "MimeKit.MailboxAddress":
					message.Headers[id] = mailbox1;

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (mailbox1, value.ToString (), "Unexpected result when setting {0} to mailbox1", property.Name);

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, mailbox2);

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (mailbox2, value.ToString (), "Unexpected result when setting {0} to mailbox2", property.Name);

					setter.Invoke (message, new object[] { null });
					value = getter.Invoke (message, new object[0]);
					Assert.IsNull (value, "Expected null value after setting {0} to null.", property.Name);
					Assert.AreEqual (-1, message.Headers.IndexOf (id), "Expected {0} header to be removed after setting it to null.", property.Name);
					break;
				case "MimeKit.MessageIdList":
					message.Headers[id] = references1;

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (references1, value.ToString (), "Unexpected result when setting {0} to references1", property.Name);

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, references2);

					value = getter.Invoke (message, new object[0]);
					Assert.AreEqual (references2, value.ToString (), "Unexpected result when setting {0} to references2", property.Name);
					break;
				case "System.DateTimeOffset":
					message.Headers[id] = date1;

					date = (DateTimeOffset) getter.Invoke (message, new object[0]);
					Assert.AreEqual (date1, DateUtils.FormatDate (date), "Unexpected result when setting {0} to date1", property.Name);

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, date2);

					date = (DateTimeOffset) getter.Invoke (message, new object[0]);
					Assert.AreEqual (date2, DateUtils.FormatDate (date), "Unexpected result when setting {0} to date2", property.Name);

					setter.Invoke (message, new object[] { date });
					break;
				case "System.Version":
					message.Headers[id] = version1;

					version = (Version) getter.Invoke (message, new object[0]);
					Assert.AreEqual (version1, version.ToString (), "Unexpected result when setting {0} to version1", property.Name);

					message.Headers[message.Headers.IndexOf (id)] = new Header (id, version2);

					version = (Version) getter.Invoke (message, new object[0]);
					Assert.AreEqual (version2, version.ToString (), "Unexpected result when setting {0} to version2", property.Name);

					setter.Invoke (message, new object[] { version });
					break;
				case "System.String":
					switch (id) {
					case HeaderId.ResentMessageId:
					case HeaderId.MessageId:
					case HeaderId.InReplyTo:
						message.Headers[id] = "<" + msgid1 + ">";

						value = getter.Invoke (message, new object[0]);
						Assert.AreEqual (msgid1, value.ToString (), "Unexpected result when setting {0} to msgid1", property.Name);

						message.Headers[message.Headers.IndexOf (id)] = new Header (id, "<" + msgid2 + ">");

						value = getter.Invoke (message, new object[0]);
						Assert.AreEqual (msgid2, value.ToString (), "Unexpected result when setting {0} to msgid2", property.Name);

						setter.Invoke (message, new object[] { msgid1 });

						setter.Invoke (message, new object[] { "<" + msgid1 + ">" });
						value = getter.Invoke (message, new object[0]);
						Assert.AreEqual (msgid1, value.ToString (), "Unexpected result when setting {0} to msgid1 via the setter.", property.Name);

						if (id == HeaderId.InReplyTo) {
							setter.Invoke (message, new object[] { null });
							value = getter.Invoke (message, new object[0]);
							Assert.IsNull (value, "Expected null value after setting {0} to null.", property.Name);
							Assert.AreEqual (-1, message.Headers.IndexOf (id), "Expected {0} header to be removed after setting it to null.", property.Name);
						}
						break;
					case HeaderId.Subject:
						message.Headers[id] = "Subject #1";

						value = getter.Invoke (message, new object[0]);
						Assert.AreEqual ("Subject #1", value.ToString (), "Unexpected result when setting {0} to subject1", property.Name);

						message.Headers[message.Headers.IndexOf (id)] = new Header (id, "Subject #2");

						value = getter.Invoke (message, new object[0]);
						Assert.AreEqual ("Subject #2", value.ToString (), "Unexpected result when setting {0} to msgid2", property.Name);
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
			Assert.AreEqual (MessageImportance.High, message.Importance);

			message.Headers.Remove (HeaderId.Importance);
			Assert.AreEqual (MessageImportance.Normal, message.Importance);

			message.Headers.Add (HeaderId.Importance, "low");
			Assert.AreEqual (MessageImportance.Low, message.Importance);

			message.Headers.Remove (HeaderId.Importance);
			Assert.AreEqual (MessageImportance.Normal, message.Importance);

			message.Headers.Add (HeaderId.Importance, "normal");
			Assert.AreEqual (MessageImportance.Normal, message.Importance);

			message.Headers.Remove (HeaderId.Importance);
			Assert.AreEqual (MessageImportance.Normal, message.Importance);

			message.Headers.Add (HeaderId.Importance, "invalid-value");
			Assert.AreEqual (MessageImportance.Normal, message.Importance);
		}

		[Test]
		public void TestPriorityChanged ()
		{
			var message = new MimeMessage ();

			message.Headers.Add (HeaderId.Priority, "urgent");
			Assert.AreEqual (MessagePriority.Urgent, message.Priority);

			message.Headers.Remove (HeaderId.Priority);
			Assert.AreEqual (MessagePriority.Normal, message.Priority);

			message.Headers.Add (HeaderId.Priority, "non-urgent");
			Assert.AreEqual (MessagePriority.NonUrgent, message.Priority);

			message.Headers.Remove (HeaderId.Priority);
			Assert.AreEqual (MessagePriority.Normal, message.Priority);

			message.Headers.Add (HeaderId.Priority, "normal");
			Assert.AreEqual (MessagePriority.Normal, message.Priority);

			message.Headers.Remove (HeaderId.Priority);
			Assert.AreEqual (MessagePriority.Normal, message.Priority);

			message.Headers.Add (HeaderId.Priority, "invalid-value");
			Assert.AreEqual (MessagePriority.Normal, message.Priority);
		}

		[Test]
		public void TestReferencesChanged ()
		{
			var message = new MimeMessage ();
			Header references;

			message.Headers.Add (HeaderId.References, "<id1@localhost> <id2@localhost>");
			Assert.AreEqual (2, message.References.Count, "The number of references does not match.");
			Assert.AreEqual ("id1@localhost", message.References[0], "The first references does not match.");
			Assert.AreEqual ("id2@localhost", message.References[1], "The second references does not match.");

			message.References.Add ("id3@localhost");

			Assert.IsTrue (message.Headers.TryGetHeader ("References", out references), "Failed to get References header.");
			Assert.AreEqual ("<id1@localhost> <id2@localhost> <id3@localhost>", references.Value, "The modified Reference header does not match.");

			message.References.Clear ();

			Assert.IsFalse (message.Headers.TryGetHeader ("References", out references), "References header should have been removed.");
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

			Assert.IsNull (message.Subject, "Subject has not been cleared.");

			Assert.IsNull (message.Sender, "Sender has not been cleared.");
			Assert.AreEqual (0, message.ReplyTo.Count, "Reply-To has not been cleared.");
			Assert.AreEqual (0, message.From.Count, "From has not been cleared.");
			Assert.AreEqual (0, message.To.Count, "To has not been cleared.");
			Assert.AreEqual (0, message.Cc.Count, "Cc has not been cleared.");
			Assert.AreEqual (0, message.Bcc.Count, "Bcc has not been cleared.");
			Assert.IsNull (message.MessageId, "Message-Id has not been cleared.");
			Assert.AreEqual (DateTimeOffset.MinValue, message.Date, "Date has not been cleared.");

			Assert.IsNull (message.ResentSender, "Resent-Sender has not been cleared.");
			Assert.AreEqual (0, message.ResentReplyTo.Count, "Resent-Reply-To has not been cleared.");
			Assert.AreEqual (0, message.ResentFrom.Count, "Resent-From has not been cleared.");
			Assert.AreEqual (0, message.ResentTo.Count, "Resent-To has not been cleared.");
			Assert.AreEqual (0, message.ResentCc.Count, "Resent-Cc has not been cleared.");
			Assert.AreEqual (0, message.ResentBcc.Count, "Resent-Bcc has not been cleared.");
			Assert.IsNull (message.ResentMessageId, "Resent-Message-Id has not been cleared.");
			Assert.AreEqual (DateTimeOffset.MinValue, message.ResentDate, "Resent-Date has not been cleared.");

			Assert.AreEqual (MessageImportance.Normal, message.Importance, "Importance has not been cleared.");
			Assert.AreEqual (MessagePriority.Normal, message.Priority, "Priority has not been cleared.");

			Assert.AreEqual (0, message.References.Count, "References has not been cleared.");
			Assert.IsNull (message.InReplyTo, "In-Reply-To has not been cleared.");

			Assert.IsNull (message.MimeVersion, "MIME-Version has not been cleared.");
		}

		[Test]
		public void TestHtmlAndTextBodies ()
		{
			const string HtmlBody = "<html>This is an <b>html</b> body.</html>";
			const string TextBody = "This is the text body.";
			MimeMessage message;

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.1.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.1.txt.");
			Assert.AreEqual (null, message.HtmlBody, "The HTML bodies do not match for body.1.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.2.txt"));
			Assert.AreEqual (null, message.TextBody, "The text bodies do not match for body.2.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.2.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.3.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.3.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.3.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.4.txt"));
			Assert.AreEqual (null, message.TextBody, "The text bodies do not match for body.4.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.4.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.5.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.5.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.5.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.6.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.6.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.6.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.7.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.7.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.7.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.8.txt"));
			Assert.AreEqual (TextBody, message.TextBody, "The text bodies do not match for body.8.txt.");
			Assert.AreEqual (null, message.HtmlBody, "The HTML bodies do not match for body.8.txt.");

			message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "body.9.txt"));
			Assert.AreEqual (null, message.TextBody, "The text bodies do not match for body.9.txt.");
			Assert.AreEqual (HtmlBody, message.HtmlBody, "The HTML bodies do not match for body.9.txt.");
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

			using (var source = new MemoryStream(Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				Assert.IsNull (message.TextBody, "Message text should be blank, as no body defined");
				Assert.AreEqual (1, message.Attachments.OfType<TextPart> ().Count (), "Message should contain one text attachment");
			}
		}
	}
}
