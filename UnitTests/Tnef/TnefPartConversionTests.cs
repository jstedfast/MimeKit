//
// TnefPartConversionTests.cs
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
	[Ignore ("Re-enabled in step 6 when TnefMessage.ToMimeMessage lands")]
	public class TnefPartConversionTests
	{
		static MimeMessage ConvertToMessage (TnefBuilder builder)
		{
			var part = new TnefPart {
				Content = new MimeContent (builder.ToStream ())
			};

			return ConvertToMessage (part);
		}

		static MimeMessage ConvertToMessage (TnefPart part)
		{
			throw new NotImplementedException ();
		}

		static MimeMessage ExtractTnefMessage (TnefReader reader)
		{
			throw new NotImplementedException ();
		}

		[Test]
		public void TestSenderSearchKeyUsedWhenEmailAddressIsMissing ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unicode), "Sender Name");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.Unicode), "SMTP");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Unicode), "SMTP:sender@example.com");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var message = ConvertToMessage (builder);

			Assert.That (message.From.Mailboxes.Count (), Is.EqualTo (1), "From");
			Assert.That (message.From.Mailboxes.First ().Address, Is.EqualTo ("sender@example.com"));
			Assert.That (message.From.Mailboxes.First ().Name, Is.EqualTo ("Sender Name"));
		}

		[Test]
		public void TestReceivedBySearchKeyUsedWhenEmailAddressIsMissing ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.Unicode), "Recipient Name");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.Unicode), "smtp");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.ReceivedBySearchKey, TnefPropertyType.Unicode), "SMTP:recipient@example.com");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var message = ConvertToMessage (builder);

			Assert.That (message.To.Mailboxes.Count (), Is.EqualTo (1), "To");
			Assert.That (message.To.Mailboxes.First ().Address, Is.EqualTo ("recipient@example.com"));
		}

		[Test]
		public void TestSearchKeyIgnoredForNonSmtpAddrType ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unicode), "Sender Name");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.Unicode), "EX");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Unicode), "EX:/O=EXAMPLE/CN=SENDER");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var message = ConvertToMessage (builder);

			Assert.That (message.From.Mailboxes.Count (), Is.EqualTo (0), "From");
		}

		[Test]
		public void TestSenderEmailAddressTakesPrecedenceOverSearchKey ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unicode), "Sender Name");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.Unicode), "real@example.com");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Unicode), "SMTP:other@example.com");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var message = ConvertToMessage (builder);

			Assert.That (message.From.Mailboxes.Count (), Is.EqualTo (1), "From");
			Assert.That (message.From.Mailboxes.First ().Address, Is.EqualTo ("real@example.com"));
		}

		[Test]
		public void TestEmbeddedMessageAttachmentShorterThanTheObjectHeader ()
		{
			// Note: an attachMethod of EmbeddedMessage means that the first 16 bytes of the
			// attachment data are an OLE object header, but nothing guarantees that the
			// attachment data is actually at least 16 bytes long.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.Unicode), "embedded.msg");
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long), (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteBinaryProperty (new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object), new byte[] { 1, 2, 3, 4 });

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			Assert.DoesNotThrow (() => ConvertToMessage (builder));
		}

		[Test]
		public void TestMessagePropertiesWithUnexpectedTypesDoNotThrow ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Unicode), "high");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Priority, TnefPropertyType.Unicode), "urgent");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Sensitivity, TnefPropertyType.Unicode), "private");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			Assert.DoesNotThrow (() => ConvertToMessage (builder));
		}

		[Test]
		public void TestRecipientTableWithUnexpectedTypesDoNotThrow ()
		{
			var row = new TnefMapiPropertyBuilder ();

			row.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Unicode), "to");
			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Long), 1234);
			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.Long), 1234);
			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.RecipientDisplayName, TnefPropertyType.Long), 1234);
			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.Long), 1234);
			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Long), 1234);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteRecipientTable (row);

			Assert.DoesNotThrow (() => ConvertToMessage (builder));
		}

		[Test]
		public void TestAttachmentPropertiesWithUnexpectedTypesDoNotThrow ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachFilename, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachContentLocation, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachContentBase, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachContentId, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachDisposition, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachMimeTag, TnefPropertyType.Long), 1234);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Long), 1234);
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Unicode), "by-value");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.AttachFlags, TnefPropertyType.Unicode), "none");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.AttachSize, TnefPropertyType.Unicode), "1234");
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Long), 1234);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			Assert.DoesNotThrow (() => ConvertToMessage (builder));
		}

		// A corrupt or hostile TNEF stream can carry anything at all in PidTagInternetMessageId, and
		// MimeMessage.MessageId rejects a value it cannot parse. Converting a message must not propagate
		// that rejection to the caller as an ArgumentException.
		[TestCase ("this is not a message id")]
		[TestCase ("")]
		[TestCase ("<>")]
		[TestCase ("<@>")]
		[TestCase ("@@@@@")]
		[TestCase ("<unterminated@example.com")]
		[TestCase ("no-angle-brackets@example.com")]
		public void TestInvalidInternetMessageIdDoesNotThrow (string messageId)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unicode), messageId);
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "Subject");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			MimeMessage message = null;

			Assert.DoesNotThrow (() => message = ConvertToMessage (builder), "ConvertToMessage");

			// The rest of the message still has to survive.
			Assert.That (message.Subject, Is.EqualTo ("Subject"), "Subject");
		}

		[Test]
		public void TestValidInternetMessageIdIsStillUsed ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unicode), "<valid.id@example.com>");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var message = ConvertToMessage (builder);

			Assert.That (message.MessageId, Is.EqualTo ("valid.id@example.com"), "MessageId");
		}

		// PidTagTnefCorrelationKey is only used as a Message-Id when it looks like one, but "looks like
		// one" is a three character heuristic that plenty of invalid values satisfy.
		[TestCase ("<@@@@@>")]
		[TestCase ("<   @   >")]
		[TestCase ("<a@b c@d>")]
		public void TestInvalidTnefCorrelationKeyDoesNotThrow (string correlationKey)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.TnefCorrelationKey, TnefPropertyType.Unicode), correlationKey);
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "Subject");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			MimeMessage message = null;

			Assert.DoesNotThrow (() => message = ConvertToMessage (builder), "ConvertToMessage");

			Assert.That (message.Subject, Is.EqualTo ("Subject"), "Subject");
		}

		// A TNEF stream that is cut short mid-value must not fail the whole conversion. ConvertToMessage ()
		// is a best effort API with no documented exception for a truncated stream, so it has to return
		// whatever was successfully extracted up to the point the bytes ran out.
		[Test]
		public void TestTruncatedStreamsConvertWithoutThrowing ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "The Subject");
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unicode), "<id@example.com>");
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Long), 2);
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.Priority, TnefPropertyType.Long), 1);
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.Unicode), "The body of the message.");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var complete = builder.ToArray ();

			// Every truncation offset is a distinct "the stream ended in the middle of this field" case.
			for (int length = 0; length < complete.Length; length++) {
				var truncated = new byte[length];

				Buffer.BlockCopy (complete, 0, truncated, 0, length);

				using var part = new TnefPart { Content = new MimeContent (new MemoryStream (truncated, false)) };

				Assert.DoesNotThrow (() => ConvertToMessage (part).Dispose (), $"truncated to {length}");
			}

			// And the untruncated stream still has to produce everything.
			using var whole = new TnefPart { Content = new MimeContent (new MemoryStream (complete, false)) };
			using var message = ConvertToMessage (whole);

			Assert.That (message.Subject, Is.EqualTo ("The Subject"), "Subject");
			Assert.That (message.MessageId, Is.EqualTo ("id@example.com"), "MessageId");
		}

		// The recipient table carries attacker controlled strings, and MailboxAddress rejects an address
		// it cannot parse. Converting a message has to drop an unparsable recipient rather than throwing.
		[TestCase ("not an address")]
		[TestCase ("@")]
		[TestCase ("<")]
		[TestCase ("a b c@example.com")]
		[TestCase ("user@")]
		[TestCase ("user@@example.com")]
		[TestCase (" ")]
		public void TestUnparsableRecipientAddressDoesNotThrow (string address)
		{
			var row = new TnefMapiPropertyBuilder ();

			row.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Long), 1);
			row.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Unicode), "Display Name");
			row.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Unicode), address);

			var valid = new TnefMapiPropertyBuilder ();

			valid.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Long), 1);
			valid.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Unicode), "Valid Recipient");
			valid.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Unicode), "valid@example.com");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteRecipientTable (row, valid);

			MimeMessage message = null;

			Assert.DoesNotThrow (() => message = ConvertToMessage (builder), "ConvertToMessage");

			// The unparsable recipient is dropped, but the valid one that follows it is not.
			Assert.That (message.To.Mailboxes.Count (), Is.EqualTo (1), "To");
			Assert.That (message.To.Mailboxes.First ().Address, Is.EqualTo ("valid@example.com"), "Address");
			Assert.That (message.To.Mailboxes.First ().Name, Is.EqualTo ("Valid Recipient"), "Name");
		}

		static byte[] BuildTruncationTestStream (out HashSet<int> boundaries)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "The Subject");
			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Long), 2);
			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.Unicode), "The body of the message.");

			var attachment = new TnefMapiPropertyBuilder ();

			attachment.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.Unicode), "file.txt");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, Encoding.ASCII.GetBytes ("attachment content"));
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, attachment);

			var complete = builder.ToArray ();

			// TNEF has no end marker, so a stream that ends exactly between two attributes is indistinguishable
			// from a complete stream. Only a cut that lands inside the header or inside an attribute is detectable.
			boundaries = new HashSet<int> { complete.Length };

			using (var reader = new TnefReader (new MemoryStream (complete, false))) {
				boundaries.Add ((int) reader.StreamOffset);

				while (reader.Read ())
					boundaries.Add ((int) (reader.StreamOffset + 9 + reader.Length + 2));
			}

			return complete;
		}

		// A truncated stream must still convert, but it must not be reported as compliant: a pipeline needs to
		// know that content may have been lost.
		[Test]
		public void TestTruncatedStreamsAreReportedAsTruncated ()
		{
			var complete = BuildTruncationTestStream (out var boundaries);
			int checkedCount = 0;

			for (int length = 0; length < complete.Length; length++) {
				if (boundaries.Contains (length))
					continue;

				var logger = new TestTnefComplianceLogger ();
				using var reader = new TnefReader (new MemoryStream (complete, 0, length, false)) { ComplianceLogger = logger };
				using var message = ExtractTnefMessage (reader);

				Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream), $"truncated to {length}");
				checkedCount++;
			}

			Assert.That (checkedCount, Is.GreaterThan (100), "too few truncation offsets were exercised");

			var completeLogger = new TestTnefComplianceLogger ();

			using (var reader = new TnefReader (new MemoryStream (complete, false)) { ComplianceLogger = completeLogger }) {
				using var message = ExtractTnefMessage (reader);

				Assert.That (completeLogger.Issues, Is.Empty, "complete stream");
			}
		}

		// In Strict mode a truncated stream must surface as a TnefException, not as a raw EndOfStreamException.
		[Test]
		public void TestTruncatedStreamsThrowTnefExceptionInStrictMode ()
		{
			var complete = BuildTruncationTestStream (out var boundaries);

			for (int length = 0; length < complete.Length; length++) {
				if (boundaries.Contains (length))
					continue;

				var ex = Assert.Throws<TnefException> (() => {
					using var reader = new TnefReader (new MemoryStream (complete, 0, length, false));
					ExtractTnefMessage (reader).Dispose ();
				}, $"truncated to {length}");

				Assert.That (ex!.Violation, Is.EqualTo (TnefComplianceViolation.TruncatedStream), $"truncated to {length}");
			}
		}
	}
}
