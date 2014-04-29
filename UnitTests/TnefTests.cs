//
// TnefTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

using MimeKit;
using MimeKit.IO;
using MimeKit.Tnef;
using MimeKit.IO.Filters;

using NUnit.Framework;

namespace UnitTests {
	[TestFixture]
	public class TnefTests
	{
		static MimeMessage ExtractTnefMessage (TnefReader reader)
		{
			var builder = new BodyBuilder ();
			var message = new MimeMessage ();
			var buffer = new byte[4096];
			TnefPropertyReader prop;
			int nread;

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
					break;

				if (reader.AttributeLevel != TnefAttributeLevel.Message)
					Assert.Fail ("Unknown attribute level.");

				prop = reader.TnefPropertyReader;

				if (reader.AttributeTag == TnefAttributeTag.RecipientTable) {
					// Note: The RecipientTable is the only attribute that uses rows...
					while (prop.ReadNextRow ()) {
						InternetAddressList list = null;
						string name = null, addr = null;

						while (prop.ReadNextProperty ()) {
							switch (prop.PropertyTag.Id) {
							case TnefPropertyId.RecipientType:
								switch (prop.ReadValueAsInt16 ()) {
								case 1: list = message.To; break;
								case 2: list = message.Cc; break;
								case 3: list = message.Bcc; break;
								default:
									Assert.Fail ("Invalid recipient type.");
									break;
								}
								break;
							case TnefPropertyId.DisplayName:
								name = prop.ReadValueAsString ();
								break;
							case TnefPropertyId.SmtpAddress:
								addr = prop.ReadValueAsString ();
								break;
							default:
								Console.WriteLine ("Unhandled RecipientType property: {0}", prop.PropertyTag.Id);
								break;
							}
						}

						Assert.NotNull (list, "The recipient type was never specified.");
						Assert.NotNull (addr, "The address was never specified.");

						if (list != null)
							list.Add (new MailboxAddress (name, addr));
					}
				} else {
					while (prop.ReadNextProperty ()) {
						switch (prop.PropertyTag.Id) {
						case TnefPropertyId.InternetMessageId:
							if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
								prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
								message.MessageId = prop.ReadValueAsString ();
							} else {
								Assert.Fail ("Unknown property type for Message-Id: {0}", prop.PropertyTag.ValueTnefType);
							}
							break;
						case TnefPropertyId.Subject:
							if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
								prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
								message.Subject = prop.ReadValueAsString ();
							} else {
								Assert.Fail ("Unknown property type for Subject: {0}", prop.PropertyTag.ValueTnefType);
							}
							break;
						case TnefPropertyId.BodyHtml:
							using (var stream = prop.GetRawValueReadStream ()) {
								int codepage = reader.MessageCodepage != 0 ? reader.MessageCodepage : Encoding.UTF8.CodePage;
								var encoding = Encoding.GetEncoding (codepage);

								using (var body = new MemoryStream ()) {
									stream.CopyTo (body, 4096);
									body.Position = 0;

									builder.HtmlBody = encoding.GetString (body.GetBuffer (), 0, (int) body.Length);
								}
							}
							break;
						case TnefPropertyId.Body:
							using (var stream = prop.GetRawValueReadStream ()) {
								int codepage = reader.MessageCodepage != 0 ? reader.MessageCodepage : Encoding.UTF8.CodePage;
								var encoding = Encoding.GetEncoding (codepage);

								using (var body = new MemoryStream ()) {
									stream.CopyTo (body, 4096);
									body.Position = 0;

									builder.TextBody = encoding.GetString (body.GetBuffer (), 0, (int) body.Length);
								}
							}
							break;
						default:
							Console.WriteLine ("Unhandled message property: {0}", prop.PropertyTag.Id);
							break;
						}
					}
				}
			}

			if (reader.AttributeLevel == TnefAttributeLevel.Attachment) {
				bool attachRenderData = false;

				do {
					if (reader.AttributeLevel != TnefAttributeLevel.Attachment)
						Assert.Fail ("Expected attachment attribute level: {0}", reader.AttributeLevel);

					if (reader.AttributeTag == TnefAttributeTag.AttachRenderData)
						attachRenderData = true;

					if (attachRenderData) {
						MimePart attachment = new MimePart ();
						MimeMessage embedded = null;
						string text;

						prop = reader.TnefPropertyReader;

						while (prop.ReadNextProperty ()) {
							switch (prop.PropertyTag.Id) {
							case TnefPropertyId.AttachLongFilename:
								attachment.FileName = prop.ReadValueAsString ();
								break;
							case TnefPropertyId.AttachFilename:
								if (attachment.FileName == null)
									attachment.FileName = prop.ReadValueAsString ();
								break;
							case TnefPropertyId.AttachContentLocation:
								text = prop.ReadValueAsString ();
								if (Uri.IsWellFormedUriString (text, UriKind.Absolute))
									attachment.ContentLocation = new Uri (text, UriKind.Absolute);
								else if (Uri.IsWellFormedUriString (text, UriKind.Relative))
									attachment.ContentLocation = new Uri (text, UriKind.Relative);
								break;
							case TnefPropertyId.AttachContentBase:
								text = prop.ReadValueAsString ();
								attachment.ContentBase = new Uri (text, UriKind.Absolute);
								break;
							case TnefPropertyId.AttachContentId:
								attachment.ContentId = prop.ReadValueAsString ();
								break;
							case TnefPropertyId.AttachDisposition:
								text = prop.ReadValueAsString ();
								if (attachment.ContentDisposition == null)
									attachment.ContentDisposition = new ContentDisposition (text);
								else
									attachment.ContentDisposition.Disposition = text;
								break;
							case TnefPropertyId.AttachData:
								if (prop.IsEmbeddedMessage) {
									using (var er = prop.GetEmbeddedMessageReader ()) {
										embedded = ExtractTnefMessage (er);
									}

									break;
								}

								using (var attachData = prop.GetRawValueReadStream ()) {
									var filter = new BestEncodingFilter ();
									var content = new MemoryBlockStream ();

									using (var filtered = new FilteredStream (content)) {
										filtered.Add (filter);

										attachData.CopyTo (filtered, 4096);
										filtered.Flush ();
									}

									content.Position = 0;

									attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
									attachment.ContentObject = new ContentObject (content, ContentEncoding.Default);
									embedded = null;
								}
								break;
							default:
								Console.WriteLine ("Unhandled attachment property: {0}", prop.PropertyTag.Id);
								break;
							}
						}

						if (embedded != null) {
							var rfc822 = new MessagePart ();
							rfc822.ContentDisposition = attachment.ContentDisposition;
							rfc822.ContentLocation = attachment.ContentLocation;
							rfc822.ContentBase = attachment.ContentBase;
							rfc822.ContentId = attachment.ContentId;

							rfc822.Message = embedded;

							builder.Attachments.Add (rfc822);
						} else {
							builder.Attachments.Add (attachment);
						}
					}
				} while (reader.ReadNextAttribute ());
			}

			message.Body = builder.ToMessageBody ();

			return message;
		}

		static MimeMessage ParseTnefMessage (string path)
		{
			using (var reader = new TnefReader (File.OpenRead (path))) {
				return ExtractTnefMessage (reader);
			}
		}

		static void TestTnefParser (string path)
		{
			var message = ParseTnefMessage (path + ".tnef");
			var names = File.ReadAllLines (path + ".list");

			foreach (var name in names) {
				bool found = false;

				foreach (var part in message.BodyParts) {
					if (part.FileName == name) {
						found = true;
						break;
					}
				}

				if (!found)
					Assert.Fail ("Failed to locate attachment: {0}", name);
			}
		}

		[Test]
		public void TestBody ()
		{
			TestTnefParser ("../../TestData/tnef/body");
		}

		[Test]
		public void TestDataBeforeName ()
		{
			TestTnefParser ("../../TestData/tnef/data-before-name");
		}

		[Test]
		public void TestGarbageAtEnd ()
		{
			TestTnefParser ("../../TestData/tnef/garbage-at-end");
		}

		[Test]
		public void TestLongFileName ()
		{
			TestTnefParser ("../../TestData/tnef/long-filename");
		}

		[Test]
		public void TestMapiAttachDataObj ()
		{
			TestTnefParser ("../../TestData/tnef/MAPI_ATTACH_DATA_OBJ");
		}

		[Test]
		public void TestMapiObj ()
		{
			TestTnefParser ("../../TestData/tnef/MAPI_OBJ");
		}

		[Test]
		public void TestMissingFileNames ()
		{
			TestTnefParser ("../../TestData/tnef/missing-filenames");
		}

		[Test]
		public void TestMultiNameProperty ()
		{
			TestTnefParser ("../../TestData/tnef/multi-name-property");
		}

		[Test]
		public void TestOneFile ()
		{
			TestTnefParser ("../../TestData/tnef/one-file");
		}

		[Test]
		public void TestRtf ()
		{
			TestTnefParser ("../../TestData/tnef/rtf");
		}

		[Test]
		public void TestTriples ()
		{
			TestTnefParser ("../../TestData/tnef/triples");
		}

		[Test]
		public void TestTwoFiles ()
		{
			TestTnefParser ("../../TestData/tnef/two-files");
		}
	}
}
