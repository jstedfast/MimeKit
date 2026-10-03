//
// TnefMessageConverterTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefMessageConverterTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");

		static TnefPropertyTag NamedTag (int index, TnefPropertyType type = TnefPropertyType.Unicode)
		{
			return new TnefPropertyTag ((TnefPropertyId) (0x8000 + index), type);
		}

		static void WriteInternetHeader (TnefMapiPropertyBuilder properties, int index, string field, params string[] values)
		{
			if (values.Length == 1) {
				properties.WritePropertyHeader (NamedTag (index), TnefPropertySetGuid.InternetHeaders, field);
				properties.WriteValueCount (1);
				properties.WriteUnicodeValue (values[0]);
			} else {
				properties.WritePropertyHeader (NamedTag (index, TnefPropertyType.Unicode | TnefPropertyType.MultiValued), TnefPropertySetGuid.InternetHeaders, field);
				properties.WriteValueCount (values.Length);

				foreach (var value in values)
					properties.WriteUnicodeValue (value);
			}
		}

		static TnefBuilder CreateMessage (TnefMapiPropertyBuilder properties)
		{
			return new TnefBuilder ().WriteTnefVersion ().WriteMapiProperties (TnefAttributeLevel.Message, properties);
		}

		static void AddAttachment (TnefBuilder builder, TnefMapiPropertyBuilder properties, byte[] content)
		{
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);

			if (content != null)
				builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, content);

			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
		}

		static TnefMapiPropertyBuilder AttachmentProperties (string fileName, string mimeType = null, string contentId = null)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.ByValue);
			properties.WriteStringProperty (TnefPropertyTag.AttachLongFilenameW, fileName);

			if (mimeType != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachMimeTagW, mimeType);

			if (contentId != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachContentIdW, contentId);

			return properties;
		}

		static void AddEmbeddedMessage (TnefBuilder builder, string subject)
		{
			var embeddedProperties = new TnefMapiPropertyBuilder ();
			embeddedProperties.WriteStringProperty (TnefPropertyTag.SubjectW, subject);

			var embedded = CreateMessage (embeddedProperties).ToArray ();
			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteStringProperty (TnefPropertyTag.DisplayNameW, subject);
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);

			AddAttachment (builder, properties, null);
		}

		static TnefConversionResult Convert (TnefBuilder builder, TnefConversionOptions options = null)
		{
			using (var tnef = TnefMessage.Load (builder.ToStream ()))
				return tnef.ConvertToMime (options);
		}

		static TnefMapiPropertyBuilder Recipient (TnefRecipientType type, string name, string address, string addrType = "SMTP", string smtpAddress = null)
		{
			var row = new TnefMapiPropertyBuilder ();

			row.WriteInt32Property (TnefPropertyTag.RecipientType, (int) type);
			row.WriteStringProperty (TnefPropertyTag.DisplayNameW, name);
			row.WriteStringProperty (TnefPropertyTag.AddrtypeW, addrType);
			row.WriteStringProperty (TnefPropertyTag.EmailAddressW, address);

			if (smtpAddress != null)
				row.WriteStringProperty (TnefPropertyTag.SmtpAddressW, smtpAddress);

			return row;
		}

		static string ReadText (MimePart part)
		{
			using (var memory = new MemoryStream ()) {
				part.Content.DecodeTo (memory);

				return Encoding.UTF8.GetString (memory.ToArray ());
			}
		}

		// Attachments are always wrapped in a multipart/mixed, even when there is no body.
		static MimeEntity SingleAttachment (MimeMessage message)
		{
			var mixed = (Multipart) message.Body;

			Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (mixed.Count, Is.EqualTo (1));

			return mixed[0];
		}

		#region Headers

		[Test]
		public void TestReceivedHeadersComeFirstAndOnlyReceivedHeadersAreCopied ()
		{
			const string transportHeaders = "Received: from a.example.com by b.example.com; Mon, 1 Jan 2024 00:00:02 +0000\r\n" +
				"Subject: Transport subject\r\n" +
				"X-Transport: dropped\r\n" +
				"Received: from c.example.com\r\n\tby a.example.com; Mon, 1 Jan 2024 00:00:01 +0000\r\n";
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Subject");
			properties.WriteStringProperty (TnefPropertyTag.TransportMessageHeadersW, transportHeaders);

			using (var result = Convert (CreateMessage (properties))) {
				var headers = result.Message.Headers;

				Assert.That (headers.Count, Is.GreaterThanOrEqualTo (3));
				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.Received));
				Assert.That (headers[0].Value, Does.StartWith ("from a.example.com"));
				Assert.That (headers[1].Id, Is.EqualTo (HeaderId.Received));
				Assert.That (headers[1].Value, Does.StartWith ("from c.example.com"));
				Assert.That (headers.Count (h => h.Id == HeaderId.Received), Is.EqualTo (2));
				Assert.That (headers.Contains ("X-Transport"), Is.False);
				Assert.That (result.Message.Subject, Is.EqualTo ("Subject"));
			}
		}

		[Test]
		public void TestInternetHeadersAreCopied ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Subject");
			WriteInternetHeader (properties, 1, "X-Custom", "custom value");
			WriteInternetHeader (properties, 2, "X-Multi", "one", "two");
			WriteInternetHeader (properties, 3, "Subject", "Duplicate subject");
			WriteInternetHeader (properties, 4, "Content-Type", "multipart/mixed; boundary=x");
			WriteInternetHeader (properties, 5, "MIME-Version", "1.0");
			WriteInternetHeader (properties, 6, "X-MS-Exchange-Organization-SCL", "1");
			WriteInternetHeader (properties, 7, "X-Microsoft-Exchange-Forest-Thing", "1");
			WriteInternetHeader (properties, 8, "Bad Field", "value");

			using (var result = Convert (CreateMessage (properties))) {
				var headers = result.Message.Headers;

				Assert.That (headers["X-Custom"], Is.EqualTo ("custom value"));
				Assert.That (headers.Where (h => h.Field == "X-Multi").Select (h => h.Value), Is.EqualTo (new[] { "one", "two" }));

				// Writers MUST NOT duplicate a header that another property already produced.
				Assert.That (headers.Count (h => h.Id == HeaderId.Subject), Is.EqualTo (1));
				Assert.That (result.Message.Subject, Is.EqualTo ("Subject"));

				// MIME structure headers and Exchange-internal headers are excluded.
				Assert.That (headers.Contains (HeaderId.ContentType), Is.False);
				Assert.That (headers.Contains (HeaderId.MimeVersion), Is.False);
				Assert.That (headers.Contains ("X-MS-Exchange-Organization-SCL"), Is.False);
				Assert.That (headers.Contains ("X-Microsoft-Exchange-Forest-Thing"), Is.False);

				Assert.That (headers.Contains ("Bad Field"), Is.False);
				Assert.That (result.Losses.Any (loss => loss.Kind == TnefConversionLossKind.InvalidHeader), Is.True);
			}
		}

		[Test]
		public void TestInternetHeadersAreAddedAfterReceivedHeaders ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			WriteInternetHeader (properties, 1, "X-Custom", "custom value");
			properties.WriteStringProperty (TnefPropertyTag.TransportMessageHeadersW, "Received: from a.example.com; Mon, 1 Jan 2024 00:00:00 +0000\r\n");

			using (var result = Convert (CreateMessage (properties))) {
				var headers = result.Message.Headers;

				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.Received));
				Assert.That (headers.IndexOf ("X-Custom"), Is.GreaterThan (0));
			}
		}

		[Test]
		public void TestImportancePrioritySensitivityAndMessageFlag ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 0);
			properties.WriteInt32Property (TnefPropertyTag.Priority, -1);
			properties.WriteInt32Property (TnefPropertyTag.Sensitivity, 2);

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Message.Importance, Is.EqualTo (MessageImportance.Low));
				Assert.That (result.Message.Priority, Is.EqualTo (MessagePriority.NonUrgent));
				Assert.That (result.Message.Headers[HeaderId.Sensitivity], Is.EqualTo ("Private"));
			}
		}

		[Test]
		public void TestThreadingHeaders ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.InReplyToIdW, "<parent@example.com>");
			properties.WriteStringProperty (TnefPropertyTag.InternetReferencesW, "<root@example.com> <parent@example.com>");

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Message.InReplyTo, Is.EqualTo ("parent@example.com"));
				Assert.That (result.Message.References, Is.EqualTo (new[] { "root@example.com", "parent@example.com" }));
			}
		}

		[Test]
		public void TestDateFromClientSubmitTime ()
		{
			var date = new DateTime (2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (TnefPropertyTag.ClientSubmitTime, date.ToFileTimeUtc ());

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Message.Date, Is.EqualTo (new DateTimeOffset (date)));
				Assert.That (result.Message.Date.Offset, Is.EqualTo (TimeSpan.Zero));
			}
		}

		[Test]
		public void TestNoHeadersAreFabricated ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			using (var result = Convert (CreateMessage (properties))) {
				var headers = result.Message.Headers;

				Assert.That (headers.Contains (HeaderId.From), Is.False);
				Assert.That (headers.Contains (HeaderId.Subject), Is.False);
				Assert.That (headers.Contains (HeaderId.Date), Is.False);
				Assert.That (headers.Contains (HeaderId.MessageId), Is.False);
			}
		}

		#endregion

		#region Addresses

		[Test]
		public void TestSentRepresentingIsFromAndDistinctSenderIsSender ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingNameW, "Boss");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingAddrtypeW, "SMTP");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingEmailAddressW, "boss@example.com");
			properties.WriteStringProperty (TnefPropertyTag.SenderNameW, "Assistant");
			properties.WriteStringProperty (TnefPropertyTag.SenderAddrtypeW, "SMTP");
			properties.WriteStringProperty (TnefPropertyTag.SenderEmailAddressW, "assistant@example.com");

			using (var result = Convert (CreateMessage (properties))) {
				var from = result.Message.From.Mailboxes.Single ();

				Assert.That (from.Name, Is.EqualTo ("Boss"));
				Assert.That (from.Address, Is.EqualTo ("boss@example.com"));
				Assert.That (result.Message.Sender, Is.Not.Null);
				Assert.That (result.Message.Sender.Name, Is.EqualTo ("Assistant"));
				Assert.That (result.Message.Sender.Address, Is.EqualTo ("assistant@example.com"));
			}
		}

		[Test]
		public void TestSenderIsOmittedWhenItMatchesSentRepresenting ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingNameW, "Alice");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingEmailAddressW, "alice@example.com");
			properties.WriteStringProperty (TnefPropertyTag.SenderNameW, "Alice Smith");
			properties.WriteStringProperty (TnefPropertyTag.SenderEmailAddressW, "ALICE@example.com");

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Message.From.Mailboxes.Single ().Address, Is.EqualTo ("alice@example.com"));
				Assert.That (result.Message.Sender, Is.Null);
			}
		}

		[Test]
		public void TestSenderIsFromWithoutSentRepresenting ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SenderNameW, "Alice");
			properties.WriteStringProperty (TnefPropertyTag.SenderAddrtypeW, "EX");
			properties.WriteStringProperty (TnefPropertyTag.SenderEmailAddressW, "/O=EXAMPLE/OU=FIRST ADMINISTRATIVE GROUP/CN=RECIPIENTS/CN=ALICE");
			properties.WriteStringProperty (TnefPropertyTag.SenderSmtpAddressW, "alice@example.com");

			using (var result = Convert (CreateMessage (properties))) {
				var from = result.Message.From.Mailboxes.Single ();

				// A non-SMTP address type uses PidTagSenderSmtpAddress instead of PidTagSenderEmailAddress.
				Assert.That (from.Name, Is.EqualTo ("Alice"));
				Assert.That (from.Address, Is.EqualTo ("alice@example.com"));
				Assert.That (result.Message.Sender, Is.Null);
			}
		}

		[Test]
		public void TestRecipientTypes ()
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			builder.WriteRecipientTable (
				Recipient (TnefRecipientType.To, "To", "to@example.com"),
				Recipient (TnefRecipientType.Cc, "Cc", "cc@example.com"),
				Recipient (TnefRecipientType.Bcc, "Bcc", "bcc@example.com"),
				Recipient ((TnefRecipientType) 0, "Originator", "originator@example.com"),
				Recipient (TnefRecipientType.To, "Exchange", "/O=EXAMPLE/CN=RECIPIENTS/CN=EXCHANGE", "EX", "exchange@example.com"),
				Recipient (TnefRecipientType.To, "Unresolvable", "/O=EXAMPLE/CN=RECIPIENTS/CN=UNRESOLVABLE", "EX"));

			using (var result = Convert (builder)) {
				var message = result.Message;

				Assert.That (message.To.Mailboxes.Select (m => m.Address), Is.EqualTo (new[] { "to@example.com", "exchange@example.com" }));
				Assert.That (message.Cc.Mailboxes.Single ().Address, Is.EqualTo ("cc@example.com"));
				Assert.That (message.Bcc.Mailboxes.Single ().Address, Is.EqualTo ("bcc@example.com"));
				Assert.That (result.Losses.Count (loss => loss.Kind == TnefConversionLossKind.UnparsableRecipient), Is.EqualTo (1));
			}
		}

		#endregion

		#region Attachments

		[TestCase ("image/png", "image/png")]
		[TestCase ("text/plain", "text/plain")]
		[TestCase ("multipart/mixed; boundary=x", "application/octet-stream")]
		[TestCase ("message/rfc822", "application/octet-stream")]
		[TestCase ("message/partial", "application/octet-stream")]
		[TestCase ("application/applefile", "application/octet-stream")]
		[TestCase ("application/mac-binhex40", "application/octet-stream")]
		[TestCase ("not a mime type", "application/octet-stream")]
		public void TestAttachmentContentTypeIsSanitized (string mimeTag, string expected)
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddAttachment (builder, AttachmentProperties ("file.bin", mimeTag), new byte[] { 0x41, 0x42, 0x43 });

			using (var result = Convert (builder)) {
				var part = (MimePart) SingleAttachment (result.Message);

				Assert.That (part.GetType (), Is.EqualTo (typeof (MimePart)));
				Assert.That (part.ContentType.MimeType, Is.EqualTo (expected));
				Assert.That (part.FileName, Is.EqualTo ("file.bin"));
			}
		}

		[Test]
		public void TestAttachmentWithoutContentIsReported ()
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddAttachment (builder, AttachmentProperties ("missing.bin"), null);

			using (var result = Convert (builder)) {
				Assert.That (result.Message.Body, Is.Null);
				Assert.That (result.Losses.Single ().Kind, Is.EqualTo (TnefConversionLossKind.AttachmentWithoutContent));
			}
		}

		[Test]
		public void TestBodyIsFirstPartOfMultipartMixed ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			var builder = CreateMessage (properties);

			AddAttachment (builder, AttachmentProperties ("a.txt", "text/plain"), Encoding.ASCII.GetBytes ("a"));
			AddAttachment (builder, AttachmentProperties ("b.txt", "text/plain"), Encoding.ASCII.GetBytes ("b"));

			using (var result = Convert (builder)) {
				var mixed = (Multipart) result.Message.Body;

				Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
				Assert.That (mixed.Count, Is.EqualTo (3));
				Assert.That (((TextPart) mixed[0]).Text, Is.EqualTo ("Hello"));
				Assert.That (((MimePart) mixed[1]).FileName, Is.EqualTo ("a.txt"));
				Assert.That (((MimePart) mixed[2]).FileName, Is.EqualTo ("b.txt"));
			}
		}

		[Test]
		public void TestEmbeddedMessagesArePassedThroughByDefault ()
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddEmbeddedMessage (builder, "Embedded");

			using (var result = Convert (builder)) {
				var part = SingleAttachment (result.Message) as TnefPart;

				Assert.That (part, Is.Not.Null);
				Assert.That (part.ContentType.Name, Is.EqualTo ("Embedded"));

				using (var embedded = part.LoadTnefMessage ())
					Assert.That (embedded.Subject, Is.EqualTo ("Embedded"));
			}
		}

		[Test]
		public void TestEmbeddedMessagesAreConvertedOnRequest ()
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddEmbeddedMessage (builder, "Embedded");

			using (var result = Convert (builder, new TnefConversionOptions { ConvertEmbeddedMessages = true })) {
				var part = SingleAttachment (result.Message) as MessagePart;

				Assert.That (part, Is.Not.Null);
				Assert.That (part.Message.Subject, Is.EqualTo ("Embedded"));
			}
		}

		#endregion

		#region MIME skeleton

		static TnefBuilder CreateSkeletonMessage (string skeleton, bool includeAttachment = true, string attachmentContentId = "image@example.com")
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Property subject");
			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello from the body property");
			properties.WriteInt32Property (TnefPropertyTag.Importance, 2);
			properties.WriteBinaryProperty (TnefPropertyTag.MimeSkeleton, Encoding.ASCII.GetBytes (skeleton.Replace ("\r\n", "\n").Replace ("\n", "\r\n")));
			properties.WriteStringProperty (TnefPropertyTag.TransportMessageHeadersW, "Received: from transport.example.com; Mon, 1 Jan 2024 00:00:00 +0000\r\n");
			WriteInternetHeader (properties, 1, "X-Property-Header", "value");

			var builder = CreateMessage (properties);

			if (includeAttachment)
				AddAttachment (builder, AttachmentProperties ("image.png", "image/png", attachmentContentId), new byte[] { 0x89, 0x50, 0x4E, 0x47 });

			return builder;
		}

		const string Skeleton = @"Received: from skeleton.example.com; Mon, 1 Jan 2024 00:00:00 +0000
From: Skeleton <skeleton@example.com>
Subject: Skeleton subject
Message-Id: <skeleton@example.com>
X-Skeleton-Header: kept
MIME-Version: 1.0
Content-Type: multipart/related; boundary=""boundary""

--boundary
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: quoted-printable

--boundary
Content-Type: image/png; name=image.png
Content-Transfer-Encoding: base64
X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>

--boundary--
";

		[Test]
		public void TestMimeSkeletonIsFilled ()
		{
			using (var result = Convert (CreateSkeletonMessage (Skeleton))) {
				var message = result.Message;

				Assert.That (result.Losses, Is.Empty);

				// The skeleton's headers are used verbatim; the property headers are not consulted.
				Assert.That (message.Subject, Is.EqualTo ("Skeleton subject"));
				Assert.That (message.MessageId, Is.EqualTo ("skeleton@example.com"));
				Assert.That (message.Headers["X-Skeleton-Header"], Is.EqualTo ("kept"));
				Assert.That (message.Headers.Contains ("X-Property-Header"), Is.False);
				Assert.That (message.Headers.Where (h => h.Id == HeaderId.Received).Select (h => h.Value.Trim ()), Is.EqualTo (new[] { "from skeleton.example.com; Mon, 1 Jan 2024 00:00:00 +0000" }));

				// PidTagImportance replaces the skeleton's value because it can change after delivery.
				Assert.That (message.Importance, Is.EqualTo (MessageImportance.High));

				var related = (MultipartRelated) message.Body;

				Assert.That (related.Count, Is.EqualTo (2));

				var text = (TextPart) related[0];
				Assert.That (text.Text, Is.EqualTo ("Hello from the body property"));
				Assert.That (text.ContentTransferEncoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				var image = (MimePart) related[1];
				Assert.That (image.ContentType.MimeType, Is.EqualTo ("image/png"));
				Assert.That (image.Headers.Contains ("X-Exchange-MIME-Skeleton-Content-Id"), Is.False);
				Assert.That (image.ContentId, Is.Null);

				using (var memory = new MemoryStream ()) {
					image.Content.DecodeTo (memory);
					Assert.That (memory.ToArray (), Is.EqualTo (new byte[] { 0x89, 0x50, 0x4E, 0x47 }));
				}
			}
		}

		[Test]
		public void TestMimeSkeletonMatchesAttachmentsByContentId ()
		{
			var skeleton = Skeleton.Replace ("X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>", "Content-Id: <IMAGE-ID@example.com>");

			using (var result = Convert (CreateSkeletonMessage (skeleton, attachmentContentId: "IMAGE-ID@example.com"))) {
				var image = (MimePart) ((Multipart) result.Message.Body)[1];

				Assert.That (result.Losses, Is.Empty);
				Assert.That (image.ContentId, Is.EqualTo ("IMAGE-ID@example.com"));
				Assert.That (image.Content.Stream.Length, Is.GreaterThan (0));
			}
		}

		static void AssertSkeletonFallback (TnefConversionResult result)
		{
			var message = result.Message;

			Assert.That (result.Losses.Any (loss => loss.Kind == TnefConversionLossKind.InvalidMimeSkeleton), Is.True);
			Assert.That (message.Subject, Is.EqualTo ("Property subject"));
			Assert.That (message.Headers["X-Property-Header"], Is.EqualTo ("value"));
			Assert.That (message.Headers[0].Id, Is.EqualTo (HeaderId.Received));
			Assert.That (message.Headers[0].Value, Does.Contain ("transport.example.com"));
		}

		[Test]
		public void TestMimeSkeletonMissingAnAttachmentFallsBackToProperties ()
		{
			var skeleton = Skeleton.Replace ("X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>", "Content-Id: <other@example.com>");

			using (var result = Convert (CreateSkeletonMessage (skeleton)))
				AssertSkeletonFallback (result);
		}

		[Test]
		public void TestMimeSkeletonWithUnknownBodyFallsBackToProperties ()
		{
			var skeleton = Skeleton.Replace ("text/plain; charset=utf-8", "text/html; charset=utf-8");

			using (var result = Convert (CreateSkeletonMessage (skeleton)))
				AssertSkeletonFallback (result);
		}

		[Test]
		public void TestHeadersOnlyMimeSkeletonIsFilledWithTextBody ()
		{
			// A skeleton without a Content-Type has an implicit text/plain body (RFC 2045 5.2).
			using (var result = Convert (CreateSkeletonMessage ("Subject: Headers only\r\n", false))) {
				Assert.That (result.Losses, Is.Empty);
				Assert.That (result.Message.Subject, Is.EqualTo ("Headers only"));
				Assert.That (((TextPart) result.Message.Body).Text, Is.EqualTo ("Hello from the body property"));
			}
		}

		[Test]
		public void TestEmptyMimeSkeletonIsIgnored ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Property subject");
			properties.WriteBinaryProperty (TnefPropertyTag.MimeSkeleton, Array.Empty<byte> ());

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Losses, Is.Empty);
				Assert.That (result.Message.Subject, Is.EqualTo ("Property subject"));
			}
		}

		#endregion

		#region API

		static TnefPart CreateTnefPart (string subject)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, subject);

			return new TnefPart {
				Content = new MimeContent (CreateMessage (properties).ToStream ())
			};
		}

		[Test]
		public void TestLoadTnefMessage ()
		{
			using (var part = CreateTnefPart ("Loaded")) {
				using (var tnef = part.LoadTnefMessage ())
					Assert.That (tnef.Subject, Is.EqualTo ("Loaded"));

				// The part can be loaded more than once.
				using (var tnef = part.LoadTnefMessage ())
					Assert.That (tnef.Subject, Is.EqualTo ("Loaded"));
			}
		}

		[Test]
		public async Task TestLoadTnefMessageAsync ()
		{
			using (var part = CreateTnefPart ("Loaded")) {
				using (var tnef = await part.LoadTnefMessageAsync ())
					Assert.That (tnef.Subject, Is.EqualTo ("Loaded"));

				using (var tnef = await part.LoadTnefMessageAsync ())
					Assert.That (tnef.Subject, Is.EqualTo ("Loaded"));
			}
		}

		[Test]
		public void TestLoadTnefMessageWithoutContent ()
		{
			using (var part = new TnefPart ()) {
				Assert.Throws<InvalidOperationException> (() => part.LoadTnefMessage ());
				Assert.ThrowsAsync<InvalidOperationException> (() => part.LoadTnefMessageAsync ());
			}
		}

		[Test]
		public void TestLoadTnefMessageAfterDispose ()
		{
			var part = CreateTnefPart ("Disposed");

			part.Dispose ();

			Assert.Throws<ObjectDisposedException> (() => part.LoadTnefMessage ());
			Assert.ThrowsAsync<ObjectDisposedException> (() => part.LoadTnefMessageAsync ());
		}

		[Test]
		public void TestConvertToMimeAfterDispose ()
		{
			var tnef = TnefMessage.Load (CreateMessage (new TnefMapiPropertyBuilder ()).ToStream ());

			tnef.Dispose ();

			Assert.Throws<ObjectDisposedException> (() => tnef.ConvertToMime ());
		}

		[Test]
		public void TestConvertToMimeCanceled ()
		{
			using (var tnef = TnefMessage.Load (CreateMessage (new TnefMapiPropertyBuilder ()).ToStream ())) {
				using var cts = new CancellationTokenSource ();

				cts.Cancel ();

				Assert.Throws<OperationCanceledException> (() => tnef.ConvertToMime (null, cts.Token));
			}
		}

		[Test]
		public void TestConvertedMessageOutlivesTnefMessage ()
		{
			MimeMessage message;
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddAttachment (builder, AttachmentProperties ("a.txt", "text/plain"), Encoding.ASCII.GetBytes ("content"));

			using (var tnef = TnefMessage.Load (builder.ToStream ()))
				message = tnef.ConvertToMime ().Message;

			using (message)
				Assert.That (ReadText ((MimePart) SingleAttachment (message)), Is.EqualTo ("content"));
		}

		[Test]
		public void TestConversionLoss ()
		{
			Assert.Throws<ArgumentNullException> (() => new TnefConversionLoss (TnefConversionLossKind.InvalidHeader, null));

			var loss = new TnefConversionLoss (TnefConversionLossKind.InvalidHeader, "description");

			Assert.That (loss.Kind, Is.EqualTo (TnefConversionLossKind.InvalidHeader));
			Assert.That (loss.Description, Is.EqualTo ("description"));
			Assert.That (loss.ToString (), Is.EqualTo ("InvalidHeader: description"));
		}

		[Test]
		public void TestConversionOptionsClone ()
		{
			var options = new TnefConversionOptions { ConvertEmbeddedMessages = true };
			var clone = options.Clone ();

			Assert.That (clone, Is.Not.SameAs (options));
			Assert.That (clone.ConvertEmbeddedMessages, Is.True);
			Assert.That (TnefConversionOptions.Default.ConvertEmbeddedMessages, Is.False);
		}

		#endregion
	}
}
