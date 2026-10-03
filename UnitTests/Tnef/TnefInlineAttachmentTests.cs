//
// TnefInlineAttachmentTests.cs
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
	// [MS-OXCMAIL] 2.1.3.4.1: which attachments are rendered inline depends on the best body ([MS-OXBBODY] 2.1.3.1).
	[TestFixture]
	public class TnefInlineAttachmentTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");

		[Flags]
		enum Bodies {
			None = 0,
			Plain = 1,
			Rtf = 2,
			Html = 4
		}

		sealed class Attachment
		{
			public TnefAttachMethod Method = TnefAttachMethod.ByValue;
			public TnefAttachFlags Flags;
			public string FileName = "image.png";
			public string ContentId;
			public string ContentLocation;
			public string ContentBase;
			public string Disposition;
			public byte[] Content = { 1, 2, 3, 4 };
		}

		static byte[] CompressedRtf ()
		{
			return new RtfCompressedBuilder ().WriteLiterals (Encoding.ASCII.GetBytes ("{\\rtf1 Hello}")).WriteEndOfStream ().ToArray ();
		}

		static TnefBuilder CreateMessage (Bodies bodies, string html = "<html><body>Hello</body></html>", int? nativeBody = null, bool? rtfInSync = null)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Inline");

			if ((bodies & Bodies.Plain) != 0)
				properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			if ((bodies & Bodies.Rtf) != 0)
				properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, CompressedRtf ());

			if ((bodies & Bodies.Html) != 0)
				properties.WriteStringProperty (TnefPropertyTag.BodyHtmlW, html);

			if (nativeBody.HasValue)
				properties.WriteInt32Property (TnefPropertyTag.NativeBody, nativeBody.Value);

			if (rtfInSync.HasValue)
				properties.WriteInt32Property (TnefPropertyTag.RtfInSync, rtfInSync.Value ? 1 : 0);

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder;
		}

		static void AddAttachment (TnefBuilder builder, Attachment attachment)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) attachment.Method);
			properties.WriteStringProperty (TnefPropertyTag.AttachLongFilenameW, attachment.FileName);

			if (attachment.Flags != TnefAttachFlags.None)
				properties.WriteInt32Property (TnefPropertyTag.AttachFlags, (int) attachment.Flags);

			if (attachment.ContentId != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachContentIdW, attachment.ContentId);

			if (attachment.ContentLocation != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachContentLocationW, attachment.ContentLocation);

			if (attachment.ContentBase != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachContentBaseW, attachment.ContentBase);

			if (attachment.Disposition != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachDispositionW, attachment.Disposition);

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);

			if (attachment.Method == TnefAttachMethod.EmbeddedMessage) {
				var embedded = new TnefBuilder ().WriteTnefVersion ().ToArray ();
				var value = new byte[16 + embedded.Length];

				IID_IMessage.ToByteArray ().CopyTo (value, 0);
				embedded.CopyTo (value, 16);

				properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);
			} else {
				builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, attachment.Content);
			}

			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
		}

		static IList<MimeEntity> Convert (TnefBuilder builder)
		{
			using (var tnef = TnefMessage.Load (builder.ToStream ())) {
				var message = tnef.ConvertToMime (new TnefConversionOptions { ConvertEmbeddedMessages = true }).Message;

				return message.BodyParts.Where (part => !(part is TextPart text) || text.ContentDisposition != null).ToList ();
			}
		}

		static MimeEntity ConvertSingle (TnefBuilder builder, Attachment attachment)
		{
			AddAttachment (builder, attachment);

			var attachments = Convert (builder);

			Assert.That (attachments, Has.Count.EqualTo (1));

			return attachments[0];
		}

		static bool IsInline (MimeEntity entity)
		{
			return entity.ContentDisposition != null && entity.ContentDisposition.Disposition == ContentDisposition.Inline;
		}

		static Attachment Flagged (string contentId = "image1@example.com")
		{
			return new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = contentId };
		}

		const string ReferencingHtml = "<html><body><img src=\"cid:image1@example.com\"></body></html>";

		#region Plain text best body

		[Test]
		public void TestPlainTextBestBodyIgnoresInlineIndications ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Plain), Flagged ());

			Assert.That (IsInline (entity), Is.False);
			Assert.That (entity.ContentDisposition.Disposition, Is.EqualTo (ContentDisposition.Attachment));
			Assert.That (entity.ContentId, Is.EqualTo ("image1@example.com"), "Content-Id is still emitted");
		}

		[Test]
		public void TestNoBodyIgnoresInlineIndications ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.None), Flagged ());

			Assert.That (IsInline (entity), Is.False);
		}

		[Test]
		public void TestAttachmentDispositionIsPreserved ()
		{
			var attachment = Flagged ();

			attachment.Disposition = "inline";

			var entity = ConvertSingle (CreateMessage (Bodies.Plain), attachment);

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestPlainTextAndHtmlWithoutRtfReferencedIsInline ()
		{
			// Deliberate deviation from [MS-OXCMAIL] 2.1.3.4.1: the plain text best body ([MS-OXBBODY] 2.1.3.1 row 10)
			// does not stop the HTML body's references from making the attachment inline.
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Html, ReferencingHtml), Flagged ());

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestPlainTextAndHtmlWithoutRtfUnreferencedIsNotInline ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Html), Flagged ());

			Assert.That (IsInline (entity), Is.False);
		}

		#endregion

		#region HTML best body

		[Test]
		public void TestHtmlBestBodyReferencedByContentId ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Html, ReferencingHtml), Flagged ());

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestHtmlBestBodyReferencedByEncodedContentId ()
		{
			const string html = "<html><body><table background=\"CID:Image%31@Example.com\"></table></body></html>";

			var entity = ConvertSingle (CreateMessage (Bodies.Html, html), Flagged ("<image1@example.com>"));

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestHtmlBestBodyUnreferenced ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Html, ReferencingHtml), Flagged ("other@example.com"));

			Assert.That (IsInline (entity), Is.False);
		}

		[Test]
		public void TestHtmlBestBodyRequiresRenderedInBody ()
		{
			var attachment = Flagged ();

			attachment.Flags = TnefAttachFlags.None;

			var entity = ConvertSingle (CreateMessage (Bodies.Html, ReferencingHtml), attachment);

			Assert.That (IsInline (entity), Is.False);
		}

		[Test]
		public void TestHtmlBestBodyRequiresContentIdOrLocation ()
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Html, ReferencingHtml), Flagged (null));

			Assert.That (IsInline (entity), Is.False);
		}

		[Test]
		public void TestHtmlBestBodyReferencedByContentLocation ()
		{
			const string html = "<html><body><img src=\"http://www.example.com/images/logo.png\"></body></html>";
			var attachment = Flagged (null);

			attachment.ContentLocation = "http://www.example.com/images/logo.png";

			var entity = ConvertSingle (CreateMessage (Bodies.Html, html), attachment);

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestHtmlBestBodyReferencedByRelativeContentLocation ()
		{
			const string html = "<html><body><img src=\"http://www.example.com/images/logo.png\"></body></html>";
			var attachment = Flagged (null);

			attachment.ContentBase = "http://www.example.com/images/";
			attachment.ContentLocation = "logo.png";

			var entity = ConvertSingle (CreateMessage (Bodies.Html, html), attachment);

			Assert.That (IsInline (entity), Is.True);
		}

		[Test]
		public void TestHtmlBestBodyOnlyReferencedAttachmentsAreInline ()
		{
			var builder = CreateMessage (Bodies.Html, ReferencingHtml);

			AddAttachment (builder, Flagged ());
			AddAttachment (builder, new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = "image2@example.com", FileName = "image2.png" });
			AddAttachment (builder, new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin" });

			var attachments = Convert (builder);

			Assert.That (attachments, Has.Count.EqualTo (3));
			Assert.That (IsInline (attachments[0]), Is.True);
			Assert.That (IsInline (attachments[1]), Is.False);
			Assert.That (IsInline (attachments[2]), Is.False);
		}

		#endregion

		#region RTF best body

		[Test]
		public void TestRtfBestBodyOleAttachmentsAreInline ()
		{
			var builder = CreateMessage (Bodies.Rtf);

			AddAttachment (builder, new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin" });
			AddAttachment (builder, Flagged ());

			var attachments = Convert (builder);

			Assert.That (attachments, Has.Count.EqualTo (2));
			Assert.That (IsInline (attachments[0]), Is.True, "OLE");
			Assert.That (IsInline (attachments[1]), Is.False, "afRenderedInBody by value");
		}

		[TestCase (true, false)]
		[TestCase (false, true)]
		public void TestRtfAndHtmlBestBodyDependsOnRtfInSync (bool rtfInSync, bool inline)
		{
			var entity = ConvertSingle (CreateMessage (Bodies.Rtf | Bodies.Html, ReferencingHtml, rtfInSync: rtfInSync), Flagged ());

			Assert.That (IsInline (entity), Is.EqualTo (inline));
		}

		[TestCase (true, true)]
		[TestCase (false, false)]
		public void TestPlainTextAndRtfBestBodyDependsOnRtfInSync (bool rtfInSync, bool inline)
		{
			// The OLE attachment is inline only when RTF, rather than plain text, is the best body.
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Rtf, rtfInSync: rtfInSync), new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin" });

			Assert.That (IsInline (entity), Is.EqualTo (inline));
		}

		#endregion

		#region PidTagNativeBody

		[TestCase (1, true)]
		[TestCase (2, false)]
		[TestCase (3, true)]
		[TestCase (0, false)]
		public void TestNativeBodyTakesPrecedence (int nativeBody, bool inline)
		{
			// Without PidTagNativeBody, RTF and HTML in sync would make RTF the best body; value 0 is not in the table
			// and so falls through to that. With value 1 (plain text), the HTML body still references the attachment,
			// so it is inline (a deliberate deviation from [MS-OXCMAIL] 2.1.3.4.1).
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Rtf | Bodies.Html, ReferencingHtml, nativeBody, true), Flagged ());

			Assert.That (IsInline (entity), Is.EqualTo (inline));
		}

		#endregion

		#region Embedded messages

		[Test]
		public void TestEmbeddedMessageIsNeverInline ()
		{
			var attachment = Flagged ();

			attachment.Method = TnefAttachMethod.EmbeddedMessage;
			attachment.FileName = "message.msg";

			var entity = ConvertSingle (CreateMessage (Bodies.Html, ReferencingHtml), attachment);

			Assert.That (entity, Is.InstanceOf<MessagePart> ());
			Assert.That (IsInline (entity), Is.False);
		}

		[Test]
		public void TestEmbeddedMessageAsTnefIsNeverInline ()
		{
			var attachment = Flagged ();
			var builder = CreateMessage (Bodies.Html, ReferencingHtml);

			attachment.Method = TnefAttachMethod.EmbeddedMessage;
			attachment.FileName = "message.msg";
			AddAttachment (builder, attachment);

			using (var tnef = TnefMessage.Load (builder.ToStream ())) {
				var message = tnef.ConvertToMime (new TnefConversionOptions { ConvertEmbeddedMessages = false }).Message;
				var entity = message.BodyParts.OfType<TnefPart> ().Single ();

				Assert.That (IsInline (entity), Is.False);
			}
		}

		[Test]
		public void TestInvalidEmbeddedMessageIsNeverInline ()
		{
			var attachment = Flagged ();

			attachment.Method = TnefAttachMethod.EmbeddedMessage;
			attachment.FileName = "message.msg";

			var builder = CreateMessage (Bodies.Html, ReferencingHtml);
			var properties = new TnefMapiPropertyBuilder ();

			// An afEmbeddedMessage attachment whose content is not an embedded message is converted to an ordinary
			// attachment, but it is still an attached Message object as far as [MS-OXCMAIL] 2.1.3.4.1 is concerned.
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteStringProperty (TnefPropertyTag.AttachLongFilenameW, attachment.FileName);
			properties.WriteInt32Property (TnefPropertyTag.AttachFlags, (int) attachment.Flags);
			properties.WriteStringProperty (TnefPropertyTag.AttachContentIdW, attachment.ContentId);

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 });
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			var attachments = Convert (builder);

			Assert.That (attachments, Has.Count.EqualTo (1));
			Assert.That (attachments[0], Is.InstanceOf<MimePart> ());
			Assert.That (IsInline (attachments[0]), Is.False);
		}

		#endregion
	}
}
