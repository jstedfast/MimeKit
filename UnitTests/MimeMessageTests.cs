//
// MimeMessageTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
using System.Net.Mail;
using System.Reflection;

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class MimeMessageTests
	{
		[Test]
		public void TestReserialization ()
		{
			const string rawMessageText = @"X-Andrew-Authenticated-As: 4099;greenbush.galaxy;Nathaniel Borenstein
Received: from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41
          via MS.5.6.greenbush.galaxy.sun4_41;
          Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
Message-ID : <UeCBvVq0M2Yt4oUA83@thumper.bellcore.com>
Date: Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
From: Nathaniel Borenstein <nsb>
X-Andrew-Message-Size: 152+1
MIME-Version: 1.0
Content-Type: multipart/alternative; 
	boundary=""Interpart.Boundary.IeCBvV20M2YtEoUA0A""
To: Ned Freed <ned@innosoft.com>,
    ysato@etl.go.jp (Yutaka Sato =?ISO-2022-JP?B?GyRAOjRGI0stGyhK?= )
Subject: MIME & int'l mail

> THIS IS A MESSAGE IN 'MIME' FORMAT.  Your mail reader does not support MIME.
> Please read the first section, which is plain text, and ignore the rest.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII

In honor of the Communications Week error about MIME's ability to handle
international character sets. a screen dump:

[An Andrew ToolKit view (mailobjv) was included here, but could not be
displayed.]
Just for fun....  -- Nathaniel

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-Type: multipart/mixed; 
	boundary=""Alternative.Boundary.IeCBvV20M2Yt4oU=wd""

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

In honor of the <italic>Communications Week</italic> error about MIME's abilit=
y to handle international character sets. a screen dump:<nl>
<nl>

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: image/gif
Content-Description: Some international characters
Content-Transfer-Encoding: base64

R0lGODdhEgLiAKEAAAAAAP///wAA////4CwAAAAAEgLiAAAC/oSPqcvtD6OctNqLs968
...
R+mUIAiVUTmCU0mVJmiVV5mCfaiVQtaUXVlKXwmWZiSWY3lDZWmWIISWaalUWcmW+bWW
b9lAcSmXCUSXdWlKbomX7HWXe4llXOmXQAmYgTmUg0mYRmmYh5mUscGYjemYjwmZkSmZ
k0mZlWmZl4mZqVEAADs=

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

<nl>
<nl>
Just for fun....  -- Nathaniel<nl>

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd--

--Interpart.Boundary.IeCBvV20M2YtEoUA0A--
";
			string result;

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText.Replace ("\r\n", "\n")))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					result = Encoding.UTF8.GetString (serialized.ToArray ());
				}
			}

			Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
		}

		[Test]
		public void TestReserializationEmptyPart ()
		{
			const string rawMessageText = @"Date: Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
From: Nathaniel Borenstein <nsb>
X-Andrew-Message-Size: 152+1
MIME-Version: 1.0
Content-Type: multipart/mixed; 
	boundary=""Interpart.Boundary.IeCBvV20M2YtEoUA0A""
To: Ned Freed <ned@innosoft.com>,
    ysato@etl.go.jp (Yutaka Sato =?ISO-2022-JP?B?GyRAOjRGI0stGyhK?= )
Subject: MIME & int'l mail

> THIS IS A MESSAGE IN 'MIME' FORMAT.  Your mail reader does not support MIME.
> Please read the first section, which is plain text, and ignore the rest.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII

This is the body.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII; name=empty.txt

--Interpart.Boundary.IeCBvV20M2YtEoUA0A--
";
			string result;

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText.Replace ("\r\n", "\n")))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					var options = FormatOptions.Default.Clone ();
					options.NewLineFormat = NewLineFormat.Unix;

					message.WriteTo (options, serialized);

					result = Encoding.UTF8.GetString (serialized.ToArray ());
				}
			}

			Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
		}

		[Test]
		public void TestMailMessageToMimeMessage ()
		{
			var mail = new MailMessage ();
			mail.Body = null;

			var text = new MemoryStream (Encoding.ASCII.GetBytes ("This is plain text."), false);
			mail.AlternateViews.Add (new AlternateView (text, "text/plain"));

			var html = new MemoryStream (Encoding.ASCII.GetBytes ("This is HTML."), false);
			mail.AlternateViews.Add (new AlternateView (html, "text/html"));

			var message = (MimeMessage) mail;

			Assert.IsTrue (message.Body is Multipart, "THe top-level MIME part should be a multipart.");
			Assert.IsTrue (message.Body.ContentType.IsMimeType ("multipart", "alternative"), "The top-level MIME part should be multipart/alternative.");

			var multipart = (Multipart) message.Body;

			Assert.AreEqual (2, multipart.Count, "Expected 2 MIME parts within the multipart/alternative.");
		}

		[Test]
		public void TestIssue135 ()
		{
			var message = new MimeMessage ();
			message.Body = new TextPart ("plain") {
				ContentTransferEncoding = ContentEncoding.Base64,
				ContentObject = new ContentObject (new MemoryStream (new byte[1], false))
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
			var message = new MimeMessage ();

			foreach (var property in message.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				var getter = property.GetGetMethod ();
				var setter = property.GetSetMethod ();
				DateTimeOffset date;
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
			var message = new MimeMessage ();
			var builder = new BodyBuilder ();

			builder.HtmlBody = "<html>This is an <b>html</b> body.</html>";
			builder.TextBody = "This is the text body.";

			builder.LinkedResources.Add ("empty.gif", new byte[0]);
			builder.LinkedResources.Add ("empty.jpg", new byte[0]);
			builder.Attachments.Add ("document.xls", new byte[0]);

			foreach (var resource in builder.LinkedResources)
				resource.ContentId = MimeUtils.GenerateMessageId ();

			message.From.Add (new MailboxAddress ("Example Name", "name@example.com"));
			message.To.Add (new MailboxAddress ("Destination", "dest@example.com"));
			message.Subject = "This is the subject";
			message.Body = builder.ToMessageBody ();

			Assert.AreEqual (builder.HtmlBody, message.HtmlBody, "The HTML bodies do not match.");
			Assert.AreEqual (builder.TextBody, message.TextBody, "The text bodies do not match.");
		}
	}
}
