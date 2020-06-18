//
// TnefTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Linq;
using System.Globalization;

using MimeKit;
using MimeKit.IO;
using MimeKit.Tnef;
using MimeKit.Utils;
using MimeKit.IO.Filters;

using NUnit.Framework;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var tnef = new TnefPart ();

			Assert.Throws<ArgumentNullException> (() => tnef.Accept (null));
		}

		static void ExtractRecipientTable (TnefReader reader, MimeMessage message)
		{
			var prop = reader.TnefPropertyReader;
			var chars = new char[1024];
			var buf = new byte[1024];

			// Note: The RecipientTable uses rows of properties...
			while (prop.ReadNextRow ()) {
				InternetAddressList list = null;
				string name = null, addr = null;

				while (prop.ReadNextProperty ()) {
					var type = prop.ValueType;
					object value;

					switch (prop.PropertyTag.Id) {
					case TnefPropertyId.RecipientType:
						int recipientType = prop.ReadValueAsInt32 ();
						switch (recipientType) {
						case 1: list = message.To; break;
						case 2: list = message.Cc; break;
						case 3: list = message.Bcc; break;
						default:
							Assert.Fail ("Invalid recipient type.");
							break;
						}
						//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, recipientType);
						break;
					case TnefPropertyId.TransmitableDisplayName:
						if (string.IsNullOrEmpty (name)) {
							name = prop.ReadValueAsString ();
							//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, name);
						} else {
							//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString ());
						}
						break;
					case TnefPropertyId.DisplayName:
						name = prop.ReadValueAsString ();
						//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, name);
						break;
					case TnefPropertyId.EmailAddress:
						if (string.IsNullOrEmpty (addr)) {
							addr = prop.ReadValueAsString ();
							//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, addr);
						} else {
							//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString ());
						}
						break;
					case TnefPropertyId.SmtpAddress:
						// The SmtpAddress, if it exists, should take precedence over the EmailAddress
						// (since the SmtpAddress is meant to be used in the RCPT TO command).
						addr = prop.ReadValueAsString ();
						//Console.WriteLine ("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, addr);
						break;
					case TnefPropertyId.Addrtype:
						Assert.AreEqual (typeof (string), type);
						value = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.Rowid:
						Assert.AreEqual (typeof (int), type);
						value = prop.ReadValueAsInt64 ();
						break;
					case TnefPropertyId.SearchKey:
						Assert.AreEqual (typeof (byte[]), type);
						value = prop.ReadValueAsBytes ();
						break;
					case TnefPropertyId.SendRichInfo:
						Assert.AreEqual (typeof (bool), type);
						value = prop.ReadValueAsBoolean ();
						break;
					case TnefPropertyId.DisplayType:
						Assert.AreEqual (typeof (int), type);
						value = prop.ReadValueAsInt16 ();
						break;
					case TnefPropertyId.SendInternetEncoding:
						Assert.AreEqual (typeof (int), type);
						value = prop.ReadValueAsBoolean ();
						break;
					default:
						Assert.Throws<ArgumentNullException> (() => prop.ReadTextValue (null, 0, chars.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (chars, -1, chars.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (chars, 0, -1));

						Assert.Throws<ArgumentNullException> (() => prop.ReadRawValue (null, 0, buf.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadRawValue (buf, -1, buf.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadRawValue (buf, 0, -1));

						if (type == typeof (int) || type == typeof (long) || type == typeof (bool) || type == typeof (double) || type == typeof (float)) {
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsString ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsGuid ());
						} else if (type == typeof (string)) {
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsBoolean ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsDouble ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsFloat ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt16 ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt32 ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt64 ());
							Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsGuid ());
						}

						value = prop.ReadValue ();
						//Console.WriteLine ("RecipientTable Property (unhandled): {0} = {1}", prop.PropertyTag.Id, value);
						Assert.AreEqual (type, value.GetType (), "Unexpected value type for {0}: {1}", prop.PropertyTag, value.GetType ().Name);
						break;
					}
				}

				Assert.IsNotNull (list, "The recipient type was never specified.");
				Assert.IsNotNull (addr, "The address was never specified.");

				if (list != null)
					list.Add (new MailboxAddress (name, addr));
			}
		}

		static void ExtractMapiProperties (TnefReader reader, MimeMessage message, BodyBuilder builder)
		{
			string normalizedSubject = null, subjectPrefix = null;
			var prop = reader.TnefPropertyReader;
			var chars = new char[1024];
			var buf = new byte[1024];

			while (prop.ReadNextProperty ()) {
				var type = prop.ValueType;
				object value;

				switch (prop.PropertyTag.Id) {
				case TnefPropertyId.InternetMessageId:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.MessageId = prop.ReadValueAsString ();
						//Console.WriteLine ("Message Property: {0} = {1}", prop.PropertyTag.Id, message.MessageId);
					} else {
						Assert.Fail ("Unknown property type for Message-Id: {0}", prop.PropertyTag.ValueTnefType);
					}
					break;
				case TnefPropertyId.Subject:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.Subject = prop.ReadValueAsString ();
						//Console.WriteLine ("Message Property: {0} = {1}", prop.PropertyTag.Id, message.Subject);
					} else {
						Assert.Fail ("Unknown property type for Subject: {0}", prop.PropertyTag.ValueTnefType);
					}
					break;
				case TnefPropertyId.RtfCompressed:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						var rtf = new TextPart ("rtf");
						rtf.ContentType.Name = "body.rtf";

						var converter = new RtfCompressedToRtf ();
						converter.Reset ();

						var content = new MemoryStream ();

						using (var filtered = new FilteredStream (content)) {
							filtered.Add (converter);

							using (var compressed = prop.GetRawValueReadStream ()) {
								compressed.CopyTo (filtered, 4096);
								filtered.Flush ();
							}
						}

						rtf.Content = new MimeContent (content);
						content.Position = 0;

						builder.Attachments.Add (rtf);

						//Console.WriteLine ("Message Property: {0} = <compressed rtf data>", prop.PropertyTag.Id);
					} else {
						Assert.Fail ("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
					}
					break;
				case TnefPropertyId.BodyHtml:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						var html = new TextPart ("html");
						html.ContentType.Name = "body.html";
						html.Text = prop.ReadValueAsString ();

						builder.Attachments.Add (html);

						//Console.WriteLine ("Message Property: {0} = {1}", prop.PropertyTag.Id, html.Text);
					} else {
						Assert.Fail ("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
					}
					break;
				case TnefPropertyId.Body:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						var plain = new TextPart ("plain");
						plain.ContentType.Name = "body.txt";
						plain.Text = prop.ReadValueAsString ();

						builder.Attachments.Add (plain);

						//Console.WriteLine ("Message Property: {0} = {1}", prop.PropertyTag.Id, plain.Text);
					} else {
						Assert.Fail ("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
					}
					break;
				case TnefPropertyId.AlternateRecipientAllowed:
					Assert.AreEqual (typeof (bool), type);
					value = prop.ReadValueAsBoolean ();
					break;
				case TnefPropertyId.MessageClass:
					Assert.AreEqual (typeof (string), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.Importance:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt16 ();
					break;
				case TnefPropertyId.Priority:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt16 ();
					break;
				case TnefPropertyId.Sensitivity:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt16 ();
					break;
				case TnefPropertyId.ClientSubmitTime:
					Assert.AreEqual (typeof (DateTime), type);
					value = prop.ReadValueAsDateTime ();
					break;
				case TnefPropertyId.SubjectPrefix:
					Assert.AreEqual (typeof (string), type);
					subjectPrefix = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.MessageSubmissionId:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.ConversationTopic:
					Assert.AreEqual (typeof (string), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.ConversationIndex:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsBytes ();
					break;
				case TnefPropertyId.SenderName:
					Assert.AreEqual (typeof (string), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.SenderEmailAddress:
					Assert.AreEqual (typeof (string), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.SenderAddrtype:
					Assert.AreEqual (typeof (string), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.SenderSearchKey:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.NormalizedSubject:
					Assert.AreEqual (typeof (string), type);
					normalizedSubject = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.CreationTime:
					Assert.AreEqual (typeof (DateTime), type);
					value = prop.ReadValueAsDateTime ();
					break;
				case TnefPropertyId.LastModificationTime:
					Assert.AreEqual (typeof (DateTime), type);
					value = prop.ReadValueAsDateTime ();
					break;
				case TnefPropertyId.InternetCPID:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt32 ();
					break;
				case TnefPropertyId.MessageCodepage:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt32 ();
					break;
				case TnefPropertyId.INetMailOverrideFormat:
					Assert.AreEqual (typeof (int), type);
					value = prop.ReadValueAsInt32 ();
					break;
				case TnefPropertyId.ReadReceiptRequested:
					Assert.AreEqual (typeof (bool), type);
					value = prop.ReadValueAsBoolean ();
					break;
				case TnefPropertyId.OriginatorDeliveryReportRequested:
					Assert.AreEqual (typeof (bool), type);
					value = prop.ReadValueAsBoolean ();
					break;
				case TnefPropertyId.TnefCorrelationKey:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.DeleteAfterSubmit:
					Assert.AreEqual (typeof (bool), type);
					value = prop.ReadValueAsBoolean ();
					break;
				case TnefPropertyId.MessageDeliveryTime:
					Assert.AreEqual (typeof (DateTime), type);
					value = prop.ReadValueAsDateTime ();
					break;
				case TnefPropertyId.SentmailEntryId:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsString ();
					break;
				case TnefPropertyId.RtfInSync:
					Assert.AreEqual (typeof (bool), type);
					value = prop.ReadValueAsBoolean ();
					break;
				case TnefPropertyId.MappingSignature:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsBytes ();
					break;
				case TnefPropertyId.StoreRecordKey:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsBytes ();
					break;
				case TnefPropertyId.StoreEntryId:
					Assert.AreEqual (typeof (byte[]), type);
					value = prop.ReadValueAsBytes ();
					break;
				default:
					Assert.Throws<ArgumentNullException> (() => prop.ReadTextValue (null, 0, chars.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (chars, -1, chars.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (chars, 0, -1));

					Assert.Throws<ArgumentNullException> (() => prop.ReadRawValue (null, 0, buf.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadRawValue (buf, -1, buf.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadRawValue (buf, 0, -1));

					if (type == typeof (int) || type == typeof (long) || type == typeof (bool) || type == typeof (double) || type == typeof (float)) {
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsString ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsGuid ());
					} else if (type == typeof (string)) {
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsBoolean ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsDouble ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsFloat ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt16 ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt32 ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt64 ());
						Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsGuid ());
					}

					try {
						value = prop.ReadValue ();
					} catch (Exception ex) {
						Console.WriteLine ("Error in prop.ReadValue(): {0}", ex);
						value = null;
					}

					//Console.WriteLine ("Message Property (unhandled): {0} = {1}", prop.PropertyTag.Id, value);
					Assert.AreEqual (type, value.GetType (), "Unexpected value type for {0}: {1}", prop.PropertyTag, value.GetType ().Name);
					break;
				}
			}

			if (string.IsNullOrEmpty (message.Subject) && !string.IsNullOrEmpty (normalizedSubject)) {
				if (!string.IsNullOrEmpty (subjectPrefix))
					message.Subject = subjectPrefix + normalizedSubject;
				else
					message.Subject = normalizedSubject;
			}
		}

		static void ExtractAttachments (TnefReader reader, BodyBuilder builder)
		{
			var attachMethod = TnefAttachMethod.ByValue;
			var filter = new BestEncodingFilter ();
			var prop = reader.TnefPropertyReader;
			MimePart attachment = null;
			int outIndex, outLength;
			TnefAttachFlags flags;
			string[] mimeType;
			byte[] attachData;
			byte[] buffer;
			DateTime time;
			string text;

			//Console.WriteLine ("Extracting attachments...");

			do {
				if (reader.AttributeLevel != TnefAttributeLevel.Attachment) {
					//Assert.Fail ("Expected attachment attribute level: {0}", reader.AttributeLevel);
					break;
				}

				switch (reader.AttributeTag) {
				case TnefAttributeTag.AttachRenderData:
					//Console.WriteLine ("Attachment Attribute: {0}", reader.AttributeTag);
					attachMethod = TnefAttachMethod.ByValue;
					attachment = new MimePart ();
					break;
				case TnefAttributeTag.Attachment:
					//Console.WriteLine ("Attachment Attribute: {0}", reader.AttributeTag);
					if (attachment == null)
						break;

					while (prop.ReadNextProperty ()) {
						switch (prop.PropertyTag.Id) {
						case TnefPropertyId.AttachLongFilename:
							attachment.FileName = prop.ReadValueAsString ();

							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.FileName);
							break;
						case TnefPropertyId.AttachFilename:
							if (attachment.FileName == null) {
								attachment.FileName = prop.ReadValueAsString ();
								//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.FileName);
							} else {
								//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString ());
							}
							break;
						case TnefPropertyId.AttachContentLocation:
							text = prop.ReadValueAsString ();
							if (Uri.IsWellFormedUriString (text, UriKind.Absolute))
								attachment.ContentLocation = new Uri (text, UriKind.Absolute);
							else if (Uri.IsWellFormedUriString (text, UriKind.Relative))
								attachment.ContentLocation = new Uri (text, UriKind.Relative);
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, text);
							break;
						case TnefPropertyId.AttachContentBase:
							text = prop.ReadValueAsString ();
							attachment.ContentBase = new Uri (text, UriKind.Absolute);
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, text);
							break;
						case TnefPropertyId.AttachContentId:
							text = prop.ReadValueAsString ();

							buffer = CharsetUtils.UTF8.GetBytes (text);
							int index = 0;

							if (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out string msgid))
								attachment.ContentId = msgid;
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.ContentId);
							break;
						case TnefPropertyId.AttachDisposition:
							text = prop.ReadValueAsString ();
							if (ContentDisposition.TryParse (text, out ContentDisposition disposition))
								attachment.ContentDisposition = disposition;
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, text);
							break;
						case TnefPropertyId.AttachMethod:
							attachMethod = (TnefAttachMethod) prop.ReadValueAsInt32 ();
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachMethod);
							break;
						case TnefPropertyId.AttachMimeTag:
							text = prop.ReadValueAsString ();
							mimeType = text.Split ('/');
							if (mimeType.Length == 2) {
								attachment.ContentType.MediaType = mimeType[0].Trim ();
								attachment.ContentType.MediaSubtype = mimeType[1].Trim ();
							}
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, text);
							break;
						case TnefPropertyId.AttachFlags:
							flags = (TnefAttachFlags) prop.ReadValueAsInt32 ();
							if ((flags & TnefAttachFlags.RenderedInBody) != 0) {
								if (attachment.ContentDisposition == null)
									attachment.ContentDisposition = new ContentDisposition (ContentDisposition.Inline);
								else
									attachment.ContentDisposition.Disposition = ContentDisposition.Inline;
							}
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, flags);
							break;
						case TnefPropertyId.AttachData:
							var stream = prop.GetRawValueReadStream ();
							var content = new MemoryStream ();

							if (attachMethod == TnefAttachMethod.EmbeddedMessage) {
								var tnef = new TnefPart ();

								foreach (var param in attachment.ContentType.Parameters)
									tnef.ContentType.Parameters[param.Name] = param.Value;

								if (attachment.ContentDisposition != null)
									tnef.ContentDisposition = attachment.ContentDisposition;

								attachment = tnef;
							}

							stream.CopyTo (content, 4096);

							buffer = content.GetBuffer ();
							filter.Flush (buffer, 0, (int) content.Length, out outIndex, out outLength);
							attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
							attachment.Content = new MimeContent (content);
							filter.Reset ();

							//Console.WriteLine ("Attachment Property: {0} has GUID {1}", prop.PropertyTag.Id, new Guid (guid));

							builder.Attachments.Add (attachment);
							break;
						case TnefPropertyId.DisplayName:
							attachment.ContentType.Name = prop.ReadValueAsString ();
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.ContentType.Name);
							break;
						case TnefPropertyId.AttachSize:
							if (attachment.ContentDisposition == null)
								attachment.ContentDisposition = new ContentDisposition ();

							attachment.ContentDisposition.Size = prop.ReadValueAsInt64 ();
							//Console.WriteLine ("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.ContentDisposition.Size.Value);
							break;
						default:
							//Console.WriteLine ("Attachment Property (unhandled): {0} = {1}", prop.PropertyTag.Id, prop.ReadValue ());
							break;
						}
					}
					break;
				case TnefAttributeTag.AttachData:
					//Console.WriteLine ("Attachment Attribute: {0}", reader.AttributeTag);
					if (attachment == null || attachMethod != TnefAttachMethod.ByValue)
						break;

					attachData = prop.ReadValueAsBytes ();
					filter.Flush (attachData, 0, attachData.Length, out outIndex, out outLength);
					attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					attachment.Content = new MimeContent (new MemoryStream (attachData, false));
					filter.Reset ();

					builder.Attachments.Add (attachment);
					break;
				case TnefAttributeTag.AttachCreateDate:
					time = prop.ReadValueAsDateTime ();

					if (attachment != null) {
						if (attachment.ContentDisposition == null)
							attachment.ContentDisposition = new ContentDisposition ();

						attachment.ContentDisposition.CreationDate = time;
					}

					//Console.WriteLine ("Attachment Attribute: {0} = {1}", reader.AttributeTag, time);
					break;
				case TnefAttributeTag.AttachModifyDate:
					time = prop.ReadValueAsDateTime ();

					if (attachment != null) {
						if (attachment.ContentDisposition == null)
							attachment.ContentDisposition = new ContentDisposition ();

						attachment.ContentDisposition.ModificationDate = time;
					}

					//Console.WriteLine ("Attachment Attribute: {0} = {1}", reader.AttributeTag, time);
					break;
				case TnefAttributeTag.AttachTitle:
					text = prop.ReadValueAsString ();

					if (attachment != null && string.IsNullOrEmpty (attachment.FileName))
						attachment.FileName = text;

					//Console.WriteLine ("Attachment Attribute: {0} = {1}", reader.AttributeTag, text);
					break;
				//case TnefAttributeTag.AttachMetaFile:
				//	break;
				default:
					var type = prop.ValueType;
					var value = prop.ReadValue ();
					//Console.WriteLine ("Attachment Attribute (unhandled): {0} = {1}", reader.AttributeTag, value);
					Assert.AreEqual (type, value.GetType (), "Unexpected value type for {0}: {1}", reader.AttributeTag, value.GetType ().Name);
					break;
				}
			} while (reader.ReadNextAttribute ());
		}

		static MimeMessage ExtractTnefMessage (TnefReader reader)
		{
			var builder = new BodyBuilder ();
			var message = new MimeMessage ();

			message.Headers.Remove (HeaderId.Date);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
					break;

				if (reader.AttributeLevel != TnefAttributeLevel.Message)
					Assert.Fail ("Unknown attribute level: {0}", reader.AttributeLevel);

				var prop = reader.TnefPropertyReader;

				switch (reader.AttributeTag) {
				case TnefAttributeTag.RecipientTable:
					ExtractRecipientTable (reader, message);
					break;
				case TnefAttributeTag.MapiProperties:
					ExtractMapiProperties (reader, message, builder);
					break;
				case TnefAttributeTag.DateSent:
					message.Date = prop.ReadValueAsDateTime ();
					//Console.WriteLine ("Message Attribute: {0} = {1}", reader.AttributeTag, message.Date);
					break;
				case TnefAttributeTag.Body:
					builder.TextBody = prop.ReadValueAsString ();
					//Console.WriteLine ("Message Attribute: {0} = {1}", reader.AttributeTag, builder.TextBody);
					break;
				case TnefAttributeTag.TnefVersion:
					//Console.WriteLine ("Message Attribute: {0} = {1}", reader.AttributeTag, prop.ReadValueAsInt32 ());
					var version = prop.ReadValueAsInt32 ();
					Assert.AreEqual (65536, version, "version");
					Assert.AreEqual (65536, reader.TnefVersion, "TnefVersion");
					break;
				case TnefAttributeTag.OemCodepage:
					int codepage = prop.ReadValueAsInt32 ();
					try {
						var encoding = Encoding.GetEncoding (codepage);
						//Console.WriteLine ("Message Attribute: OemCodepage = {0}", encoding.HeaderName);
					} catch {
						//Console.WriteLine ("Message Attribute: OemCodepage = {0}", codepage);
					}
					break;
				default:
					//Console.WriteLine ("Message Attribute (unhandled): {0} = {1}", reader.AttributeTag, prop.ReadValue ());
					break;
				}
			}

			if (reader.AttributeLevel == TnefAttributeLevel.Attachment) {
				ExtractAttachments (reader, builder);
			} else {
				//Console.WriteLine ("no attachments");
			}

			message.Body = builder.ToMessageBody ();

			return message;
		}

		static MimeMessage ParseTnefMessage (string path, TnefComplianceStatus expected)
		{
			using (var reader = new TnefReader (File.OpenRead (path), 0, TnefComplianceMode.Loose)) {
				var message = ExtractTnefMessage (reader);

				Assert.AreEqual (expected, reader.ComplianceStatus, "Unexpected compliance status.");

				return message;
			}
		}

		static byte[] ReadAllBytes (Stream stream, bool text)
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					if (text)
						filtered.Add (new Dos2UnixFilter (true));
					stream.CopyTo (filtered, 4096);
					filtered.Flush ();

					return memory.ToArray ();
				}
			}
		}

		static void TestTnefParser (string baseFileName, TnefComplianceStatus expected = TnefComplianceStatus.Compliant)
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", baseFileName);
			var message = ParseTnefMessage (path + ".tnef", expected);
			var tnefName = Path.GetFileName (path + ".tnef");
			var names = File.ReadAllLines (path + ".list");

			foreach (var name in names) {
				bool found = false;

				foreach (var part in message.BodyParts.OfType<MimePart> ()) {
					if (part.FileName == name) {
						found = true;
						break;
					}
				}

				if (!found)
					Assert.Fail ("Failed to locate attachment: {0}", name);
			}

			// now use TnefPart to do the same thing
			using (var content = File.OpenRead (path + ".tnef")) {
				var tnef = new TnefPart { Content = new MimeContent (content) };
				var attachments = tnef.ExtractAttachments ().ToList ();

				// Step 1: make sure we've extracted the body and all of the attachments
				foreach (var name in names) {
					bool found = false;

					foreach (var part in attachments.OfType<MimePart> ()) {
						if (part is TextPart && string.IsNullOrEmpty (part.FileName)) {
							var basename = Path.GetFileNameWithoutExtension (name);
							var extension = Path.GetExtension (name);
							string subtype;

							switch (extension) {
							case ".html": subtype = "html"; break;
							case ".rtf": subtype = "rtf"; break;
							default: subtype = "plain"; break;
							}

							if (basename == "body" && part.ContentType.IsMimeType ("text", subtype)) {
								found = true;
								break;
							}
						} else if (part.FileName == name) {
							found = true;
							break;
						}
					}

					if (!found)
						Assert.Fail ("Failed to locate attachment in TnefPart: {0}", name);
				}

				// Step 2: verify that the content of the extracted attachments matches up with the expected content
				byte[] expectedData, actualData;
				int untitled = 1;

				foreach (var part in attachments.OfType<MimePart> ()) {
					var isText = false;
					string fileName;

					if (part is TextPart text && string.IsNullOrEmpty (part.FileName)) {
						if (text.IsHtml)
							fileName = "message.html";
						else if (text.IsRichText)
							fileName = "message.rtf";
						else
							fileName = "message.txt";

						isText = true;
					} else if (part.FileName == "Untitled Attachment") {
						// special case for winmail.tnef and christmas.tnef
						fileName = string.Format (CultureInfo.InvariantCulture, "Untitled Attachment.{0}", untitled++);
					} else {
						var extension = Path.GetExtension (part.FileName);

						switch (extension) {
						case ".cfg":
						case ".dat":
						case ".htm":
						case ".ini":
						case ".src":
							isText = true;
							break;
						case "":
							isText = part.FileName == "AUTHORS" || part.FileName == "README";
							break;
						}

						fileName = part.FileName;
					}

					var file = Path.Combine (path, fileName);

					if (!File.Exists (file)) {
						//using (var stream = part.Content.Open ()) {
						//	actualData = ReadAllBytes (stream, isText);
						//	File.WriteAllBytes (file, actualData);
						//}
						continue;
					}

					using (var stream = File.OpenRead (file))
						expectedData = ReadAllBytes (stream, isText);

					using (var stream = part.Content.Open ())
						actualData = ReadAllBytes (stream, isText);

					Assert.AreEqual (expectedData.Length, actualData.Length, $"{tnefName}: {fileName} content length does not match");
					for (int i = 0; i < expectedData.Length; i++)
						Assert.AreEqual (expectedData[i], actualData[i], $"{tnefName}: {fileName} content differs at index {i}");
				}
			}
		}

		[Test]
		public void TestAttachments ()
		{
			TestTnefParser ("attachments");
		}

		[Test]
		public void TestBody ()
		{
			TestTnefParser ("body");
		}

		[Test]
		public void TestChristmas ()
		{
			TestTnefParser ("christmas", TnefComplianceStatus.UnsupportedPropertyType);
		}

		[Test]
		public void TestDataBeforeName ()
		{
			TestTnefParser ("data-before-name");
		}

		[Test]
		public void TestGarbageAtEnd ()
		{
			const TnefComplianceStatus errors = TnefComplianceStatus.InvalidAttributeLevel | TnefComplianceStatus.StreamTruncated;

			TestTnefParser ("garbage-at-end", errors);
		}

		[Test]
		public void TestLongFileName ()
		{
			TestTnefParser ("long-filename");
		}

		[Test]
		public void TestMapiAttachDataObj ()
		{
			TestTnefParser ("MAPI_ATTACH_DATA_OBJ");
		}

		[Test]
		public void TestMapiObject ()
		{
			TestTnefParser ("MAPI_OBJECT");
		}

		[Test]
		public void TestMissingFileNames ()
		{
			TestTnefParser ("missing-filenames");
		}

		[Test]
		public void TestMultiNameProperty ()
		{
			TestTnefParser ("multi-name-property");
		}

		[Test]
		public void TestMultiValueAttribute ()
		{
			TestTnefParser ("multi-value-attribute");
		}

		[Test]
		public void TestOneFile ()
		{
			TestTnefParser ("one-file");
		}

		[Test]
		public void TestPanic ()
		{
			TestTnefParser ("panic", TnefComplianceStatus.InvalidAttribute | TnefComplianceStatus.InvalidAttributeLevel);
		}

		[Test]
		public void TestRtf ()
		{
			TestTnefParser ("rtf");
		}

		[Test]
		public void TestTriples ()
		{
			TestTnefParser ("triples");
		}

		[Test]
		public void TestTwoFiles ()
		{
			TestTnefParser ("two-files");
		}

		[Test]
		public void TestUnicodeMapiAttrName ()
		{
			TestTnefParser ("unicode-mapi-attr-name");
		}

		[Test]
		public void TestUnicodeMapiAttr ()
		{
			TestTnefParser ("unicode-mapi-attr");
		}

		[Test]
		public void TestWinMail ()
		{
			TestTnefParser ("winmail");
		}

		[Test]
		public void TestExtractedCharset ()
		{
			const string expected = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=koi8-r\">\r\n<style type=\"text/css\" style=\"display:none;\"><!-- P {margin-top:0;margin-bottom:0;} --></style>\r\n</head>\r\n<body dir=\"ltr\">\r\n<div id=\"divtagdefaultwrapper\" style=\"font-size:12pt;color:#000000;font-family:Calibri,Helvetica,sans-serif;\" dir=\"ltr\">\r\n<p>ЫПУФЙК</p>\r\n<p><br>\r\n</p>\r\n<p>{EMAILSIGNATURE}</p>\r\n<p><br>\r\n</p>\r\n<div id=\"Signature\"><br>\r\n<font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 10pt;\"><br>\r\n<font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 12pt;\"><b>RR Test 1</b></font>\r\n</font>\r\n<p><font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 10pt;\">&nbsp;</font></p>\r\n</div>\r\n</div>\r\n</body>\r\n</html>\r\n";
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", "ukr.eml"));
			var tnef = message.BodyParts.OfType<TnefPart> ().FirstOrDefault ();

			message = tnef.ConvertToMessage ();

			Assert.IsInstanceOf (typeof (TextPart), message.Body);

			var text = (TextPart) message.Body;

			Assert.IsTrue (text.IsHtml);

			var html = text.Text;

			Assert.AreEqual ("windows-1251", text.ContentType.Charset);
			Assert.AreEqual (expected.Replace ("\r\n", Environment.NewLine), html);
		}

		[Test]
		public void TestRichTextEml ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", "rich-text.eml"));
			var tnef = message.BodyParts.OfType<TnefPart> ().FirstOrDefault ();
			var mtime = new DateTimeOffset (new DateTime (2018, 12, 15, 10, 17, 38));

			message = tnef.ConvertToMessage ();

			Assert.IsEmpty (message.Subject, "Subject");
			Assert.AreEqual (DateTimeOffset.MinValue, message.Date, "Date");
			Assert.AreEqual ("DM5PR21MB0828DA2B8C88048BC03EFFA6CFA20@DM5PR21MB0828.namprd21.prod.outlook.com", message.MessageId, "Message-Id");

			Assert.IsInstanceOf (typeof (Multipart), message.Body);
			var multipart = (Multipart) message.Body;

			Assert.AreEqual (6, multipart.Count);

			Assert.IsInstanceOf (typeof (TextPart), multipart[0]);
			Assert.IsInstanceOf (typeof (MimePart), multipart[1]);
			Assert.IsInstanceOf (typeof (MimePart), multipart[2]);
			Assert.IsInstanceOf (typeof (MimePart), multipart[3]);
			Assert.IsInstanceOf (typeof (MimePart), multipart[4]);
			Assert.IsInstanceOf (typeof (MimePart), multipart[5]);

			var rtf = (TextPart) multipart[0];
			Assert.AreEqual ("text/rtf", rtf.ContentType.MimeType, "MimeType");

			var kitten = (MimePart) multipart[1];
			Assert.AreEqual ("application/octet-stream", kitten.ContentType.MimeType, "MimeType");
			Assert.AreEqual ("kitten-playing-with-a-christmas-tree.jpg", kitten.FileName, "FileName");

			// Note: For some reason, each task and appointment got duplicated. The first copy is attached as a
			// TnefAttribute.AttachData and the second is a TnefPropertyId.AttachData.
			var task1 = (MimePart) multipart[2];
			Assert.AreEqual ("application/octet-stream", task1.ContentType.MimeType, "MimeType");
			Assert.AreEqual ("Build a train table", task1.ContentType.Name, "Name");
			Assert.AreEqual ("attachment", task1.ContentDisposition.Disposition, "Disposition");
			Assert.AreEqual ("Untitled Attachment", task1.ContentDisposition.FileName, "FileName");
			Assert.AreEqual (mtime, task1.ContentDisposition.ModificationDate, "ModificationDate");
			Assert.AreEqual (9217, task1.ContentDisposition.Size, "Size");

			var task2 = (MimePart) multipart[3];
			Assert.AreEqual ("application/vnd.ms-tnef", task2.ContentType.MimeType, "MimeType");
			Assert.AreEqual ("Build a train table", task2.ContentType.Name, "Name");
			Assert.AreEqual ("attachment", task2.ContentDisposition.Disposition, "Disposition");
			Assert.AreEqual ("Untitled Attachment", task2.ContentDisposition.FileName, "FileName");
			Assert.AreEqual (mtime, task2.ContentDisposition.ModificationDate, "ModificationDate");
			Assert.AreEqual (9217, task2.ContentDisposition.Size, "Size");

			var appointment1 = (MimePart) multipart[4];
			Assert.AreEqual ("application/octet-stream", appointment1.ContentType.MimeType, "MimeType");
			Assert.AreEqual ("Christmas Celebration!", appointment1.ContentType.Name, "Name");
			Assert.AreEqual ("attachment", appointment1.ContentDisposition.Disposition, "Disposition");
			Assert.AreEqual ("Untitled Attachment", appointment1.ContentDisposition.FileName, "FileName");
			Assert.AreEqual (mtime, appointment1.ContentDisposition.ModificationDate, "ModificationDate");
			Assert.AreEqual (387453, appointment1.ContentDisposition.Size, "Size");

			var appointment2 = (MimePart) multipart[5];
			Assert.AreEqual ("application/vnd.ms-tnef", appointment2.ContentType.MimeType, "MimeType");
			Assert.AreEqual ("Christmas Celebration!", appointment2.ContentType.Name, "Name");
			Assert.AreEqual ("attachment", appointment2.ContentDisposition.Disposition, "Disposition");
			Assert.AreEqual ("Untitled Attachment", appointment2.ContentDisposition.FileName, "FileName");
			Assert.AreEqual (mtime, appointment2.ContentDisposition.ModificationDate, "ModificationDate");
			Assert.AreEqual (387453, appointment2.ContentDisposition.Size, "Size");
		}

		[Test]
		public void TestTnefNameId ()
		{
			var guid = Guid.NewGuid ();
			var tnef1 = new TnefNameId (guid, 17);
			var tnef2 = new TnefNameId (guid, 17);

			Assert.AreEqual (TnefNameIdKind.Id, tnef1.Kind, "Kind Id");
			Assert.AreEqual (guid, tnef1.PropertySetGuid, "PropertySetGuid Id");
			Assert.AreEqual (17, tnef1.Id, "Id");

			Assert.AreEqual (tnef1.GetHashCode (), tnef2.GetHashCode (), "GetHashCode Id");
			Assert.AreEqual (tnef1, tnef2, "Equal Id");

			tnef1 = new TnefNameId (guid, "name");
			Assert.AreEqual (TnefNameIdKind.Name, tnef1.Kind, "Kind Name");
			Assert.AreEqual (guid, tnef1.PropertySetGuid, "PropertySetGuid");
			Assert.AreEqual ("name", tnef1.Name, "Name");

			Assert.AreNotEqual (tnef1.GetHashCode (), tnef2.GetHashCode (), "GetHashCode Name vs Id");
			Assert.AreNotEqual (tnef1, tnef2, "Equal Name vs Id");

			tnef2 = new TnefNameId (guid, "name");
			Assert.AreEqual (tnef1.GetHashCode (), tnef2.GetHashCode (), "GetHashCode Name");
			Assert.AreEqual (tnef1, tnef2, "Equal Name");

			Assert.IsFalse (tnef1.Equals (new object ()), "Equals (object)");
		}
	}
}
