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

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPartConversionTests
	{
		static MimeMessage ConvertToMessage (TnefBuilder builder)
		{
			var part = new TnefPart {
				Content = new MimeContent (builder.ToStream ())
			};

			return part.ConvertToMessage ();
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
	}
}
