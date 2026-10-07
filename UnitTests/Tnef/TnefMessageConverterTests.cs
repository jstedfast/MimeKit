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

		static void AddEmbeddedMessage (TnefBuilder builder, string subject, string contentId = null, byte[] embedded = null)
		{
			if (embedded is null) {
				var embeddedProperties = new TnefMapiPropertyBuilder ();
				embeddedProperties.WriteStringProperty (TnefPropertyTag.SubjectW, subject);

				embedded = CreateMessage (embeddedProperties).ToArray ();
			}

			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteStringProperty (TnefPropertyTag.DisplayNameW, subject);
			if (contentId != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachContentIdW, contentId);
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
		public void TestNonStringInternetHeadersAreReported ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (NamedTag (1, TnefPropertyType.Long), TnefPropertySetGuid.InternetHeaders, "X-Single");
			properties.WriteRaw (BitConverter.GetBytes (1));
			properties.WritePropertyHeader (NamedTag (2, TnefPropertyType.Long | TnefPropertyType.MultiValued), TnefPropertySetGuid.InternetHeaders, "X-Multi");
			properties.WriteValueCount (2);
			properties.WriteRaw (BitConverter.GetBytes (1));
			properties.WriteRaw (BitConverter.GetBytes (2));

			using (var result = Convert (CreateMessage (properties))) {
				Assert.That (result.Message.Headers.Contains ("X-Single"), Is.False);
				Assert.That (result.Message.Headers.Contains ("X-Multi"), Is.False);
				Assert.That (result.Losses.Count (loss => loss.Kind == TnefConversionLossKind.InvalidHeader), Is.EqualTo (2));
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

		[Test]
		public void TestInvalidEmbeddedMessageIsPassedThrough ()
		{
			var builder = new TnefBuilder ().WriteTnefVersion ();

			AddEmbeddedMessage (builder, "Corrupt", embedded: new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

			using (var result = Convert (builder, new TnefConversionOptions { ConvertEmbeddedMessages = true })) {
				var part = SingleAttachment (result.Message) as TnefPart;

				Assert.That (part, Is.Not.Null);
				Assert.That (result.Losses.Single ().Kind, Is.EqualTo (TnefConversionLossKind.InvalidEmbeddedMessage));
			}
		}

		#endregion

		#region HTML body charset

		static TnefConversionResult ConvertHtml (byte[] html, int internetCodepage)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.InternetCodepage, internetCodepage);
			properties.WriteBinaryProperty (TnefPropertyTag.BodyHtmlB, html);

			return Convert (CreateMessage (properties));
		}

		const string Cyrillic = "\u041f\u0440\u0438\u0432\u0435\u0442";

		[TestCase ("<html><head><meta charset=\"utf-8\"></head><body>{0}</body></html>", 65001)]
		[TestCase ("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=koi8-r\"></head><body>{0}</body></html>", 20866)]
		[TestCase ("<html><head><meta name=\"viewport\" content=\"width=device-width\"><meta http-equiv=\"refresh\" content=\"30\"><meta charset=\" \"></head><body>{0}</body></html>", 1251)]
		public void TestHtmlMetaCharsetIsUsed (string template, int codepage)
		{
			var html = Encoding.GetEncoding (codepage).GetBytes (string.Format (template, Cyrillic));

			// PidTagInternetCodepage is only used when the document does not declare a usable charset.
			using (var result = ConvertHtml (html, 1251))
				Assert.That (result.Message.HtmlBody, Does.Contain (Cyrillic));
		}

		[TestCase ("<html><head><meta charset=\"utf-8\"></head><body>{0}</body></html>")]
		[TestCase ("<html><head><meta charset=\"x-unknown-charset\"></head><body>{0}</body></html>")]
		[TestCase ("<html><head></head><body><meta charset=\"koi8-r\">{0}</body></html>")]
		[TestCase ("<html><head><title>t</title></head><meta charset=\"koi8-r\"><body>{0}</body></html>")]
		public void TestUnusableHtmlMetaCharsetIsIgnored (string template)
		{
			// The windows-1251 bytes are not valid UTF-8, the charset is unknown, or the <meta> element is not in the
			// document's head, so PidTagInternetCodepage is used instead.
			var html = Encoding.GetEncoding (1251).GetBytes (string.Format (template, Cyrillic));

			using (var result = ConvertHtml (html, 1251))
				Assert.That (result.Message.HtmlBody, Does.Contain (Cyrillic));
		}

		#endregion

		#region Compressed RTF

		const string RtfText = "{\\rtf1 Hello from RTF}";
		const string RtfPlainText = "Hello from RTF";

		static byte[] CompressedRtf (int? crc = null, int? compressionType = null, string rtf = RtfText)
		{
			return new RtfCompressedBuilder ().WriteLiterals (Encoding.ASCII.GetBytes (rtf)).WriteEndOfStream ().ToArray (crc: crc, compressionType: compressionType);
		}

		static TnefBuilder CreateRtfMessage (byte[] rtf, bool rtfInSync = true, string html = null, int? nativeBody = null, string plain = "Hello")
		{
			var properties = new TnefMapiPropertyBuilder ();

			if (plain != null)
				properties.WriteStringProperty (TnefPropertyTag.BodyW, plain);
			if (html != null)
				properties.WriteBinaryProperty (TnefPropertyTag.BodyHtmlB, Encoding.ASCII.GetBytes (html));
			if (nativeBody.HasValue)
				properties.WriteInt32Property (TnefPropertyTag.NativeBody, nativeBody.Value);
			properties.WriteInt32Property (TnefPropertyTag.RtfInSync, rtfInSync ? 1 : 0);
			properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, rtf);

			return CreateMessage (properties);
		}

		// [MS-OXCMAIL] 2.1.3.3.5: when the best body is RTF, the text/plain and text/html alternatives are generated
		// from the RTF (as UTF-8 without a byte order mark), and no text/rtf part is emitted.
		static MultipartAlternative AssertGeneratedFromRtf (MimeMessage message, string plain = RtfPlainText, string html = RtfPlainText)
		{
			var alternative = (MultipartAlternative) message.Body;

			Assert.That (alternative.Count, Is.EqualTo (2));
			Assert.That (alternative[0].ContentType.MimeType, Is.EqualTo ("text/plain"));
			Assert.That (alternative[0].ContentType.Charset, Is.EqualTo ("utf-8"));
			Assert.That (alternative[1].ContentType.MimeType, Is.EqualTo ("text/html"));
			Assert.That (alternative[1].ContentType.Charset, Is.EqualTo ("utf-8"));
			Assert.That (ReadText ((MimePart) alternative[0]).Trim (), Is.EqualTo (plain));
			Assert.That (ReadText ((MimePart) alternative[1]), Does.StartWith ("<").And.Contain (html));
			Assert.That (message.BodyParts.Any (part => part.ContentType.IsMimeType ("text", "rtf")), Is.False);

			return alternative;
		}

		[Test]
		public void TestCompressedRtfBody ()
		{
			using (var result = Convert (CreateRtfMessage (CompressedRtf ()))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message);
			}
		}

		[Test]
		public void TestUncompressedRtfBody ()
		{
			var rtf = new byte[16 + RtfText.Length];

			BitConverter.GetBytes (RtfText.Length + 12).CopyTo (rtf, 0);
			BitConverter.GetBytes (RtfText.Length).CopyTo (rtf, 4);
			BitConverter.GetBytes ((int) RtfCompressionMode.Uncompressed).CopyTo (rtf, 8);
			Encoding.ASCII.GetBytes (RtfText).CopyTo (rtf, 16);

			using (var result = Convert (CreateRtfMessage (rtf))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message);
			}
		}

		[Test]
		public void TestRtfNotInSyncWithPlainTextIsNotUsed ()
		{
			// [MS-OXBBODY] 2.1.3.1 step 3, row 9.1: plain text and RTF that is not in sync makes plain text the best body.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (), rtfInSync: false))) {
				var text = (TextPart) result.Message.Body;

				Assert.That (result.Losses, Is.Empty);
				Assert.That (text.ContentType.MimeType, Is.EqualTo ("text/plain"));
				Assert.That (text.Text, Is.EqualTo ("Hello"));
			}
		}

		[Test]
		public void TestRtfInSyncWithHtmlIsUsedForBothBodies ()
		{
			// [MS-OXBBODY] 2.1.3.1 step 3, row 6: RTF that is in sync is the best body even when there is an HTML body, and
			// [MS-OXCMAIL] 2.1.3.3.5 says the text/html SHOULD be generated from the RTF rather than copied from PidTagHtml.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (), html: "<html><body>Stale HTML</body></html>"))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message);
				Assert.That (result.Message.HtmlBody, Does.Not.Contain ("Stale HTML"));
			}
		}

		[Test]
		public void TestRtfNotInSyncWithHtmlIsNotUsed ()
		{
			// [MS-OXBBODY] 2.1.3.1 step 3, row 7.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (), rtfInSync: false, html: "<html><body>Current HTML</body></html>"))) {
				var alternative = (MultipartAlternative) result.Message.Body;

				Assert.That (alternative.Select (part => part.ContentType.MimeType), Is.EqualTo (new[] { "text/plain", "text/html" }));
				Assert.That (result.Message.TextBody, Is.EqualTo ("Hello"));
				Assert.That (result.Message.HtmlBody, Does.Contain ("Current HTML"));
			}
		}

		[Test]
		public void TestNativeBodyRtfIsUsed ()
		{
			// [MS-OXBBODY] 2.1.3.1 step 1: PidTagNativeBody takes precedence over PidTagRtfInSync.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (), rtfInSync: false, nativeBody: 2))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message);
			}
		}

		[Test]
		public void TestNativeBodyNamingAMissingBodyFallsBackToRtf ()
		{
			// PidTagNativeBody says plain text, but the RTF is the only body there is.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (), rtfInSync: false, nativeBody: 1, plain: null))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message);
			}
		}

		[Test]
		public void TestEncapsulatedHtmlIsRecovered ()
		{
			// [MS-OXRTFEX] 2.1.3.1.2: RTF with \fromhtml1 encapsulates the original HTML, which RtfToHtml recovers.
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag <html><body><p>}{\\htmlrtf \\par }Hello {\\*\\htmltag <b>}encapsulated{\\*\\htmltag </b>}{\\*\\htmltag </p></body></html>}}";

			using (var result = Convert (CreateRtfMessage (CompressedRtf (rtf: rtf)))) {
				Assert.That (result.Losses, Is.Empty);
				AssertGeneratedFromRtf (result.Message, "Hello encapsulated", "<p>Hello <b>encapsulated</b></p>");
			}
		}

		[Test]
		public void TestRtfBodyWithLoneSurrogate ()
		{
			// A lone \uN surrogate must not make the UTF-8 encoder throw.
			using (var result = Convert (CreateRtfMessage (CompressedRtf (rtf: "{\\rtf1 A\\u-10240?B}")))) {
				var alternative = (MultipartAlternative) result.Message.Body;

				Assert.That (result.Losses, Is.Empty);
				Assert.That (ReadText ((MimePart) alternative[0]), Does.StartWith ("A").And.Contain ("B"));
				Assert.That (ReadText ((MimePart) alternative[1]), Does.Contain ("B"));
			}
		}

		[TestCase (0x44434241)]
		[TestCase (0)]
		public void TestUnknownRtfCompressionTypeDropsRtfBody (int compressionType)
		{
			using (var result = Convert (CreateRtfMessage (CompressedRtf (compressionType: compressionType)))) {
				var text = result.Message.Body as TextPart;

				Assert.That (text, Is.Not.Null, "the RTF alternative is dropped, leaving only the plain text body");
				Assert.That (text.ContentType.MimeType, Is.EqualTo ("text/plain"));
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidRtfBody }));
				Assert.That (result.Losses[0].Description, Does.Contain (((uint) compressionType).ToString ("X8")));
			}
		}

		[Test]
		public void TestRtfChecksumMismatchIsReported ()
		{
			using (var result = Convert (CreateRtfMessage (CompressedRtf (crc: 0x12345678)))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.RtfChecksumMismatch }));
				AssertGeneratedFromRtf (result.Message);
			}
		}

		[Test]
		public void TestUnknownRtfCompressionTypeInEmbeddedMessage ()
		{
			var embeddedProperties = new TnefMapiPropertyBuilder ();
			embeddedProperties.WriteStringProperty (TnefPropertyTag.SubjectW, "Embedded");
			embeddedProperties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, CompressedRtf (compressionType: 0x44434241));

			var embedded = CreateMessage (embeddedProperties).ToArray ();
			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);

			var builder = new TnefBuilder ().WriteTnefVersion ();
			AddAttachment (builder, properties, null);

			using (var result = Convert (builder, new TnefConversionOptions { ConvertEmbeddedMessages = true })) {
				var part = (MessagePart) SingleAttachment (result.Message);

				Assert.That (part.Message.BodyParts.Any (entity => entity.ContentType.IsMimeType ("text", "rtf")), Is.False);
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidRtfBody }));
			}
		}

		#endregion

		#region MIME skeleton

		static TnefBuilder CreateSkeletonMessage (string skeleton, bool includeAttachment = true, string attachmentContentId = "image@example.com", byte[] rtf = null)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Property subject");
			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello from the body property");
			properties.WriteInt32Property (TnefPropertyTag.Importance, 2);

			if (rtf != null) {
				properties.WriteInt32Property (TnefPropertyTag.RtfInSync, 1);
				properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, rtf);
			}
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

		const string RtfSkeleton = @"Subject: Skeleton subject
MIME-Version: 1.0
Content-Type: multipart/related; boundary=""boundary""

--boundary
Content-Type: text/rtf

--boundary
Content-Type: image/png; name=image.png
Content-Transfer-Encoding: base64
X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>

--boundary--
";

		[Test]
		public void TestMimeSkeletonRtfBodyIsFilled ()
		{
			using (var result = Convert (CreateSkeletonMessage (RtfSkeleton, rtf: CompressedRtf ()))) {
				var related = (MultipartRelated) result.Message.Body;

				Assert.That (result.Losses, Is.Empty);
				Assert.That (ReadText ((MimePart) related[0]), Is.EqualTo (RtfText));
			}
		}

		[Test]
		public void TestMimeSkeletonFallbackRedecodesRtfBody ()
		{
			// The skeleton consumes the decoded RTF before the missing attachment makes it fall back, so the RTF has to
			// be decoded again to generate the text/plain and text/html bodies.
			using (var result = Convert (CreateSkeletonMessage (RtfSkeleton, attachmentContentId: "other@example.com", rtf: CompressedRtf ()))) {
				AssertSkeletonFallback (result);
				Assert.That (result.Message.TextBody.TrimEnd (), Is.EqualTo (RtfPlainText));
				Assert.That (result.Message.HtmlBody, Does.Contain (RtfPlainText));
				Assert.That (result.Message.BodyParts.Any (part => part.ContentType.IsMimeType ("text", "rtf")), Is.False);
			}
		}

		[Test]
		public void TestMimeSkeletonFallbackKeepsRtfLosses ()
		{
			using (var result = Convert (CreateSkeletonMessage (RtfSkeleton, rtf: CompressedRtf (compressionType: 0x44434241)))) {
				AssertSkeletonFallback (result);
				Assert.That (result.Losses.Any (loss => loss.Kind == TnefConversionLossKind.InvalidRtfBody), Is.True);
				Assert.That (result.Message.BodyParts.Any (part => part.ContentType.IsMimeType ("text", "rtf")), Is.False);
			}
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

		const string EmbeddedSkeleton = @"Subject: Skeleton subject
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary""

--boundary
Content-Type: text/plain; charset=utf-8

--boundary
Content-Type: message/rfc822
X-Exchange-MIME-Skeleton-Content-Id: <embedded@example.com>

--boundary--
";

		[Test]
		public void TestMimeSkeletonEmbeddedMessageIsFilled ()
		{
			var builder = CreateSkeletonMessage (EmbeddedSkeleton, false);

			AddEmbeddedMessage (builder, "Embedded", "embedded@example.com");

			// The skeleton's structure decides that the embedded message is converted, whatever the options say.
			using (var result = Convert (builder, new TnefConversionOptions { ConvertEmbeddedMessages = false })) {
				var mixed = (Multipart) result.Message.Body;

				Assert.That (result.Losses, Is.Empty);
				Assert.That (result.Message.Subject, Is.EqualTo ("Skeleton subject"));
				Assert.That (((TextPart) mixed[0]).Text, Is.EqualTo ("Hello from the body property"));
				Assert.That (((MessagePart) mixed[1]).Message.Subject, Is.EqualTo ("Embedded"));
			}
		}

		[Test]
		public void TestMimeSkeletonEmbeddedMessageThatIsNotAnEmbeddedMessageFallsBackToProperties ()
		{
			var skeleton = EmbeddedSkeleton.Replace ("embedded@example.com", "image@example.com");

			using (var result = Convert (CreateSkeletonMessage (skeleton)))
				AssertSkeletonFallback (result);
		}

		[Test]
		public void TestMimeSkeletonInvalidEmbeddedMessageFallsBackToProperties ()
		{
			var builder = CreateSkeletonMessage (EmbeddedSkeleton, false);

			AddEmbeddedMessage (builder, "Corrupt", "embedded@example.com", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

			using (var result = Convert (builder)) {
				AssertSkeletonFallback (result);

				// The loss recorded while trying the skeleton is discarded along with the skeleton.
				Assert.That (result.Losses.Any (loss => loss.Kind == TnefConversionLossKind.InvalidEmbeddedMessage), Is.False);
				Assert.That (result.Message.BodyParts.OfType<TnefPart> ().Count (), Is.EqualTo (1));
			}
		}

		[Test]
		public void TestMimeSkeletonWithoutAnAttachmentFallsBackToProperties ()
		{
			// The message/rfc822 part does not correspond to an attachment, so the image attachment is not in the skeleton.
			var skeleton = EmbeddedSkeleton.Replace ("\r\n", "\n").Replace ("X-Exchange-MIME-Skeleton-Content-Id: <embedded@example.com>\n", string.Empty);

			using (var result = Convert (CreateSkeletonMessage (skeleton)))
				AssertSkeletonFallback (result);
		}

		[Test]
		public void TestMimeSkeletonAttachmentWithoutContentFallsBackToProperties ()
		{
			var builder = CreateSkeletonMessage (Skeleton, false);

			AddAttachment (builder, AttachmentProperties ("image.png", "image/png", "image@example.com"), null);

			using (var result = Convert (builder))
				AssertSkeletonFallback (result);
		}

		[Test]
		public void TestMimeSkeletonReportsUnreferencedAttachmentWithoutContent ()
		{
			var builder = CreateSkeletonMessage (Skeleton);

			AddAttachment (builder, AttachmentProperties ("missing.bin"), null);

			using (var result = Convert (builder)) {
				Assert.That (result.Message.Subject, Is.EqualTo ("Skeleton subject"));
				Assert.That (result.Losses.Single ().Kind, Is.EqualTo (TnefConversionLossKind.AttachmentWithoutContent));
			}
		}

		[Test]
		public void TestMimeSkeletonKeepsPartsThatHaveContent ()
		{
			// Parts that already have content in the skeleton are left alone, even if they match an attachment.
			var skeleton = Skeleton.Replace ("\r\n", "\n")
				.Replace ("Content-Transfer-Encoding: quoted-printable\n\n", "Content-Transfer-Encoding: quoted-printable\n\nSkeleton text\n")
				.Replace ("X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>\n\n", "X-Exchange-MIME-Skeleton-Content-Id: <image@example.com>\n\nAQID\n");

			using (var result = Convert (CreateSkeletonMessage (skeleton))) {
				var related = (MultipartRelated) result.Message.Body;

				Assert.That (result.Losses, Is.Empty);
				Assert.That (((TextPart) related[0]).Text.TrimEnd (), Is.EqualTo ("Skeleton text"));

				using (var memory = new MemoryStream ()) {
					((MimePart) related[1]).Content.DecodeTo (memory);
					Assert.That (memory.ToArray (), Is.EqualTo (new byte[] { 1, 2, 3 }));
				}
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
			var options = new TnefConversionOptions { ConvertEmbeddedMessages = true, GenerateCalendar = false, MaxCalendarExceptions = 7, MaxCalendarExceptionsSize = 4096 };
			var clone = options.Clone ();

			Assert.That (clone, Is.Not.SameAs (options));
			Assert.That (clone.ConvertEmbeddedMessages, Is.True);
			Assert.That (clone.GenerateCalendar, Is.False);
			Assert.That (clone.MaxCalendarExceptions, Is.EqualTo (7));
			Assert.That (clone.MaxCalendarExceptionsSize, Is.EqualTo (4096));
			Assert.That (TnefConversionOptions.Default.ConvertEmbeddedMessages, Is.False);
			Assert.That (TnefConversionOptions.Default.GenerateCalendar, Is.True);
		}

		#endregion
	}
}
