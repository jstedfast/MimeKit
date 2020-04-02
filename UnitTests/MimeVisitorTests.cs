﻿//
// MimeVisitorTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

using NUnit.Framework;

using MimeKit;
using MimeKit.Tnef;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class MimeVisitorTests
	{
		[Test]
		public void TestMimeVisitor ()
		{
			var dataDir = Path.Combine ("..", "..", "TestData", "mbox");
			var visitor = new HtmlPreviewVisitor ();
			int index = 0;

			using (var stream = File.OpenRead (Path.Combine (dataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var filename = string.Format ("jwz.body.{0}.html", index);
					var path = Path.Combine (dataDir, filename);
					var message = parser.ParseMessage ();
					string expected, actual;

					visitor.Visit (message);

					actual = visitor.HtmlBody;

					if (!string.IsNullOrEmpty (actual))
						actual = actual.Replace ("\r\n", "\n");

					if (!File.Exists (path) && actual != null)
						File.WriteAllText (path, actual);

					if (File.Exists (path))
						expected = File.ReadAllText (path, Encoding.UTF8).Replace ("\r\n", "\n");
					else
						expected = null;

					if (index != 6 && index != 13 && index != 31) {
						// message 6, 13 and 31 contain some japanese text that is broken in Mono
						Assert.AreEqual (expected, actual, "The bodies do not match for message {0}", index);
					}

					visitor.Reset ();
					index++;
				}
			}
		}

		class MimeVisitorTester : MimeVisitor
		{
			public int ApplicationPgpEncrypted;
			public int ApplicationPgpSignature;
			public int ApplicationPkcs7Mime;
			public int ApplicationPkcs7Signature;
			public int Message;
			public int MessageDeliveryStatus;
			public int MessageDispositionNotification;
			public int MessagePart;
			public int MessagePartial;
			public int MimeEntity;
			public int MimeMessage;
			public int MimePart;
			public int Multipart;
			public int MultipartAlternative;
			public int MultipartEncrypted;
			public int MultipartRelated;
			public int MultipartReport;
			public int MultipartSigned;
			public int TextPart;
			public int TextRfc822Headers;
			public int TnefPart;

			protected internal override void VisitApplicationPgpEncrypted (ApplicationPgpEncrypted entity)
			{
				ApplicationPgpEncrypted++;
				base.VisitApplicationPgpEncrypted (entity);
			}

			protected internal override void VisitApplicationPgpSignature (ApplicationPgpSignature entity)
			{
				ApplicationPgpSignature++;
				base.VisitApplicationPgpSignature (entity);
			}

			protected internal override void VisitApplicationPkcs7Mime (ApplicationPkcs7Mime entity)
			{
				ApplicationPkcs7Mime++;
				base.VisitApplicationPkcs7Mime (entity);
			}

			protected internal override void VisitApplicationPkcs7Signature (ApplicationPkcs7Signature entity)
			{
				ApplicationPkcs7Signature++;
				base.VisitApplicationPkcs7Signature (entity);
			}

			protected override void VisitMessage (MessagePart entity)
			{
				Message++;
				base.VisitMessage (entity);
			}

			protected internal override void VisitMessageDeliveryStatus (MessageDeliveryStatus entity)
			{
				MessageDeliveryStatus++;
				base.VisitMessageDeliveryStatus (entity);
			}

			protected internal override void VisitMessageDispositionNotification (MessageDispositionNotification entity)
			{
				MessageDispositionNotification++;
				base.VisitMessageDispositionNotification (entity);
			}

			protected internal override void VisitMessagePart (MessagePart entity)
			{
				MessagePart++;
				base.VisitMessagePart (entity);
			}

			protected internal override void VisitMessagePartial (MessagePartial entity)
			{
				MessagePartial++;
				base.VisitMessagePartial (entity);
			}

			protected internal override void VisitMimeEntity (MimeEntity entity)
			{
				MimeEntity++;
				base.VisitMimeEntity (entity);
			}

			protected internal override void VisitMimeMessage (MimeMessage message)
			{
				MimeMessage++;
				base.VisitMimeMessage (message);
			}

			protected internal override void VisitMimePart (MimePart entity)
			{
				MimePart++;
				base.VisitMimePart (entity);
			}

			protected internal override void VisitMultipart (Multipart multipart)
			{
				Multipart++;
				base.VisitMultipart (multipart);
			}

			protected internal override void VisitMultipartAlternative (MultipartAlternative alternative)
			{
				MultipartAlternative++;
				base.VisitMultipartAlternative (alternative);
			}

			protected internal override void VisitMultipartEncrypted (MultipartEncrypted encrypted)
			{
				MultipartEncrypted++;
				base.VisitMultipartEncrypted (encrypted);
			}

			protected internal override void VisitMultipartRelated (MultipartRelated related)
			{
				MultipartRelated++;
				base.VisitMultipartRelated (related);
			}

			protected internal override void VisitMultipartReport (MultipartReport report)
			{
				MultipartReport++;
				base.VisitMultipartReport (report);
			}

			protected internal override void VisitMultipartSigned (MultipartSigned signed)
			{
				MultipartSigned++;
				base.VisitMultipartSigned (signed);
			}

			protected internal override void VisitTextPart (TextPart entity)
			{
				TextPart++;
				base.VisitTextPart (entity);
			}

			protected internal override void VisitTextRfc822Headers (TextRfc822Headers entity)
			{
				TextRfc822Headers++;
				base.VisitTextRfc822Headers (entity);
			}

			protected internal override void VisitTnefPart (TnefPart entity)
			{
				TnefPart++;
				base.VisitTnefPart (entity);
			}
		}

		[Test]
		public void TestVisitorMethods ()
		{
			var message = new MimeMessage ();
			message.Body = new MultipartSigned () {
				new Multipart ("mixed") {
					new MultipartAlternative () {
						new TextPart ("plain"),
						new MultipartRelated () {
							new TextPart ("html"),
							new MimePart ("image", "jpeg")
						}
					},
					new TnefPart (),
					new MessagePart () {
						Message = new MimeMessage () {
							Body = new MultipartReport ("delivery-status") {
								new MessageDeliveryStatus (),
								new TextRfc822Headers ()
							}
						}
					},
					new MessagePart () {
						Message = new MimeMessage () {
							Body = new MultipartReport ("disposition-notification") {
								new MessageDispositionNotification ()
							}
						}
					},
					new MessagePartial ("id", 1, 1),
					new ApplicationPkcs7Mime (SecureMimeType.SignedData, Stream.Null),
					new ApplicationPkcs7Signature (Stream.Null),
					new MultipartEncrypted () {
						new MimePart ("application", "octet-stream"),
						new ApplicationPgpEncrypted ()
					}
				},
				new ApplicationPgpSignature (Stream.Null)
			};
			var visitor = new MimeVisitorTester ();

			visitor.Visit (message);
			Assert.AreEqual (1, visitor.ApplicationPgpEncrypted);
			Assert.AreEqual (1, visitor.ApplicationPgpSignature);
			Assert.AreEqual (1, visitor.ApplicationPkcs7Mime);
			Assert.AreEqual (1, visitor.ApplicationPkcs7Signature);
			Assert.AreEqual (3, visitor.Message);
			Assert.AreEqual (1, visitor.MessageDeliveryStatus);
			Assert.AreEqual (1, visitor.MessageDispositionNotification);
			Assert.AreEqual (3, visitor.MessagePart);
			Assert.AreEqual (1, visitor.MessagePartial);
			Assert.AreEqual (22, visitor.MimeEntity);
			Assert.AreEqual (3, visitor.MimeMessage);
			Assert.AreEqual (12, visitor.MimePart);
			Assert.AreEqual (7, visitor.Multipart);
			Assert.AreEqual (1, visitor.MultipartAlternative);
			Assert.AreEqual (1, visitor.MultipartEncrypted);
			Assert.AreEqual (1, visitor.MultipartRelated);
			Assert.AreEqual (2, visitor.MultipartReport);
			Assert.AreEqual (1, visitor.MultipartSigned);
			Assert.AreEqual (2, visitor.TextPart);
			Assert.AreEqual (1, visitor.TextRfc822Headers);
			Assert.AreEqual (1, visitor.TnefPart);

			visitor = new MimeVisitorTester ();

			visitor.Visit (message.Body);
			Assert.AreEqual (1, visitor.ApplicationPgpEncrypted);
			Assert.AreEqual (1, visitor.ApplicationPgpSignature);
			Assert.AreEqual (1, visitor.ApplicationPkcs7Mime);
			Assert.AreEqual (1, visitor.ApplicationPkcs7Signature);
			Assert.AreEqual (3, visitor.Message);
			Assert.AreEqual (1, visitor.MessageDeliveryStatus);
			Assert.AreEqual (1, visitor.MessageDispositionNotification);
			Assert.AreEqual (3, visitor.MessagePart);
			Assert.AreEqual (1, visitor.MessagePartial);
			Assert.AreEqual (22, visitor.MimeEntity);
			Assert.AreEqual (2, visitor.MimeMessage);
			Assert.AreEqual (12, visitor.MimePart);
			Assert.AreEqual (7, visitor.Multipart);
			Assert.AreEqual (1, visitor.MultipartAlternative);
			Assert.AreEqual (1, visitor.MultipartEncrypted);
			Assert.AreEqual (1, visitor.MultipartRelated);
			Assert.AreEqual (2, visitor.MultipartReport);
			Assert.AreEqual (1, visitor.MultipartSigned);
			Assert.AreEqual (2, visitor.TextPart);
			Assert.AreEqual (1, visitor.TextRfc822Headers);
			Assert.AreEqual (1, visitor.TnefPart);
		}
	}
}
