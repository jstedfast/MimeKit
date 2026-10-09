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

using UnitTests.IO;

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
			public string MimeTag;
			public string DisplayName;
			public int? RenderingPosition;
			public bool Hidden;
			public byte[] Content = { 1, 2, 3, 4 };
		}

		static readonly byte[] Png = { 0x89, (byte) 'P', (byte) 'N', (byte) 'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, (byte) 'I', (byte) 'H', (byte) 'D', (byte) 'R' };
		static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 16, (byte) 'J', (byte) 'F', (byte) 'I', (byte) 'F', 0 };

		static byte[] CompressedRtf (string rtf)
		{
			return new RtfCompressedBuilder ().WriteLiterals (Encoding.ASCII.GetBytes (rtf)).WriteEndOfStream ().ToArray ();
		}

		static byte[] CompressedRtf ()
		{
			return CompressedRtf ("{\\rtf1 Hello}");
		}

		static TnefBuilder CreateMessage (Bodies bodies, string html = "<html><body>Hello</body></html>", int? nativeBody = null, bool? rtfInSync = null, byte[] rtf = null)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Inline");

			if ((bodies & Bodies.Plain) != 0)
				properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			if ((bodies & Bodies.Rtf) != 0)
				properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, rtf ?? CompressedRtf ());

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

			if (attachment.MimeTag != null)
				properties.WriteStringProperty (TnefPropertyTag.AttachMimeTagW, attachment.MimeTag);

			if (attachment.DisplayName != null)
				properties.WriteStringProperty (TnefPropertyTag.DisplayNameW, attachment.DisplayName);

			if (attachment.RenderingPosition.HasValue)
				properties.WriteInt32Property (TnefPropertyTag.RenderingPosition, attachment.RenderingPosition.Value);

			if (attachment.Hidden)
				properties.WriteProperty (TnefPropertyTag.AttachmentHidden, new byte[] { 1, 0, 0, 0 });

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
		public void TestRtfBestBodyOleAttachmentsAreNotInline ()
		{
			// [MS-OXCMAIL] 2.1.3.4.1.1: an OLE attachment is rendered in an RTF body as an image of the object. Without
			// a TnefOleObjectConverter, there is no image, so the OLE object is an ordinary attachment.
			var builder = CreateMessage (Bodies.Rtf);

			AddAttachment (builder, new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin" });
			AddAttachment (builder, Flagged ());

			var attachments = Convert (builder);

			Assert.That (attachments, Has.Count.EqualTo (2));
			Assert.That (IsInline (attachments[0]), Is.False, "OLE");
			Assert.That (attachments[0].ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "OLE");
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
			// The image is displayed (and so inline) only when RTF, rather than plain text, is the best body.
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Rtf, rtfInSync: rtfInSync), new Attachment { Content = Png });

			Assert.That (IsInline (entity), Is.EqualTo (inline));
		}

		static byte[] UndecodableRtf ()
		{
			return new RtfCompressedBuilder ().WriteLiterals (Encoding.ASCII.GetBytes ("{\\rtf1 Hello}")).WriteEndOfStream ().ToArray (compressionType: 0x44434241);
		}

		[Test]
		public void TestUndecodableRtfIsNotTheBestBody ()
		{
			// With a valid RTF body that is in sync, the image would be inline (see above).
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Rtf, rtfInSync: true, rtf: UndecodableRtf ()), new Attachment { Content = Png });

			Assert.That (IsInline (entity), Is.False);
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

		[Test]
		public void TestNativeBodyRtfIsIgnoredWhenRtfIsUndecodable ()
		{
			// PidTagNativeBody says RTF, but the RTF body is dropped, so the HTML body is the best body.
			var entity = ConvertSingle (CreateMessage (Bodies.Plain | Bodies.Rtf | Bodies.Html, ReferencingHtml, 2, true, UndecodableRtf ()), Flagged ());

			Assert.That (IsInline (entity), Is.True);
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

		#region multipart/related

		static MimeEntity ConvertBody (TnefBuilder builder, TnefConversionOptions options = null)
		{
			using (var tnef = TnefMessage.Load (builder.ToStream ()))
				return tnef.ConvertToMime (options ?? new TnefConversionOptions ()).Message.Body;
		}

		static void AssertRelated (MimeEntity entity, string rootMimeType, params string[] fileNames)
		{
			Assert.That (entity, Is.InstanceOf<MultipartRelated> ());

			var related = (MultipartRelated) entity;

			Assert.That (related, Has.Count.EqualTo (1 + fileNames.Length));
			Assert.That (related[0].ContentType.MimeType, Is.EqualTo (rootMimeType));
			Assert.That (related.Root, Is.SameAs (related[0]));
			Assert.That (related.ContentType.Parameters["type"], Is.EqualTo (rootMimeType));
			Assert.That (related.ContentType.Parameters.Contains ("start"), Is.False, "the root is the first child");

			for (int i = 0; i < fileNames.Length; i++) {
				Assert.That (related[i + 1], Is.InstanceOf<MimePart> ());
				Assert.That (((MimePart) related[i + 1]).FileName, Is.EqualTo (fileNames[i]));
				Assert.That (IsInline (related[i + 1]), Is.True);
			}
		}

		[Test]
		public void TestHtmlBodyAndInlineAttachmentsAreRelated ()
		{
			const string html = "<html><body><img src=\"cid:image1@example.com\"><img src=\"cid:image2@example.com\"></body></html>";
			var builder = CreateMessage (Bodies.Html, html);

			AddAttachment (builder, Flagged ());
			AddAttachment (builder, new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = "image2@example.com", FileName = "image2.png" });

			AssertRelated (ConvertBody (builder), "text/html", "image.png", "image2.png");
		}

		[Test]
		public void TestOtherAttachmentsArePeersOfTheRelated ()
		{
			var builder = CreateMessage (Bodies.Html, ReferencingHtml);

			// [MS-OXCMAIL] 2.1.3.3.6: an attachment the HTML does not refer to is a peer of the multipart/related,
			// and attachment-table order is kept for the peers.
			AddAttachment (builder, new Attachment { FileName = "first.bin" });
			AddAttachment (builder, Flagged ());
			AddAttachment (builder, new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = "image2@example.com", FileName = "image2.png" });

			var body = ConvertBody (builder);

			Assert.That (body, Is.InstanceOf<Multipart> ());

			var mixed = (Multipart) body;

			Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (mixed, Has.Count.EqualTo (3));
			AssertRelated (mixed[0], "text/html", "image.png");
			Assert.That (((MimePart) mixed[1]).FileName, Is.EqualTo ("first.bin"));
			Assert.That (((MimePart) mixed[2]).FileName, Is.EqualTo ("image2.png"));
			Assert.That (IsInline (mixed[2]), Is.False);
		}

		[Test]
		public void TestAlternativeBodyIsTheRootOfTheRelated ()
		{
			var builder = CreateMessage (Bodies.Plain | Bodies.Html, ReferencingHtml);

			AddAttachment (builder, Flagged ());

			var body = ConvertBody (builder);

			AssertRelated (body, "multipart/alternative", "image.png");

			var alternative = (MultipartAlternative) ((MultipartRelated) body)[0];

			Assert.That (alternative, Has.Count.EqualTo (2));
			Assert.That (alternative[0].ContentType.MimeType, Is.EqualTo ("text/plain"));
			Assert.That (alternative[1].ContentType.MimeType, Is.EqualTo ("text/html"));
		}

		[Test]
		public void TestUnreferencedAttachmentsAreNotRelated ()
		{
			var builder = CreateMessage (Bodies.Html, ReferencingHtml);

			AddAttachment (builder, Flagged ("other@example.com"));

			var body = ConvertBody (builder);

			Assert.That (body, Is.Not.InstanceOf<MultipartRelated> ());
			Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (((Multipart) body)[0].ContentType.MimeType, Is.EqualTo ("text/html"));
		}

		[Test]
		public void TestInlineDispositionAloneIsNotRelated ()
		{
			var builder = CreateMessage (Bodies.Plain);
			var attachment = Flagged ();

			attachment.Disposition = "inline";
			AddAttachment (builder, attachment);

			var body = ConvertBody (builder);

			Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (IsInline (((Multipart) body)[1]), Is.True);
		}

		[Test]
		public void TestRtfBestBodyNonImageAttachmentsAreNotRelated ()
		{
			var builder = CreateMessage (Bodies.Rtf);

			AddAttachment (builder, new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin" });

			var body = ConvertBody (builder);

			// The HTML generated from the RTF can only display images, so there is nothing for a multipart/related to
			// resolve.
			Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (((Multipart) body)[0].ContentType.MimeType, Is.EqualTo ("multipart/alternative"));
			Assert.That (IsInline (((Multipart) body)[1]), Is.False);
		}

		#endregion

		#region RTF attachment placeholders

		sealed class Converted
		{
			public MimeEntity Body;
			public string Text;
			public string Html;
			public List<MimePart> Related = new List<MimePart> ();
			public List<MimePart> Attachments = new List<MimePart> ();
		}

		static Converted ConvertRtf (string rtf, TnefConversionOptions options, params Attachment[] attachments)
		{
			var builder = CreateMessage (Bodies.Rtf, rtf: CompressedRtf (rtf));

			foreach (var attachment in attachments)
				AddAttachment (builder, attachment);

			var result = new Converted { Body = ConvertBody (builder, options) };
			var alternative = result.Body;

			if (alternative is Multipart mixed && mixed.ContentType.IsMimeType ("multipart", "mixed")) {
				alternative = mixed[0];

				for (int i = 1; i < mixed.Count; i++)
					result.Attachments.Add ((MimePart) mixed[i]);
			}

			if (alternative is MultipartRelated related) {
				alternative = related[0];

				for (int i = 1; i < related.Count; i++)
					result.Related.Add ((MimePart) related[i]);
			}

			Assert.That (alternative, Is.InstanceOf<MultipartAlternative> ());

			result.Text = ((MultipartAlternative) alternative).TextBody;
			result.Html = ((MultipartAlternative) alternative).HtmlBody;

			return result;
		}

		static Converted ConvertRtf (string rtf, params Attachment[] attachments)
		{
			return ConvertRtf (rtf, null, attachments);
		}

		static string Img (MimePart image, string alt)
		{
			return "<img src=\"cid:" + image.ContentId + "\" alt=\"" + alt + "\"/>";
		}

		const string TwoPlaceholders = "{\\rtf1 A\\objattph\\'20 B\\objattph\\'20 C}";

		[Test]
		public void TestRtfPlaceholdersAreReplacedByImages ()
		{
			var result = ConvertRtf (TwoPlaceholders,
				new Attachment { FileName = "a.png", Content = Png },
				new Attachment { FileName = "b.bin" });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Attachments, Has.Count.EqualTo (1));

			var image = result.Related[0];

			Assert.That (image.ContentType.MimeType, Is.EqualTo ("image/png"));
			Assert.That (image.ContentId, Is.Not.Null);
			Assert.That (IsInline (image), Is.True);
			Assert.That (result.Attachments[0].FileName, Is.EqualTo ("b.bin"));
			Assert.That (IsInline (result.Attachments[0]), Is.False);

			// The placeholder character that follows each \objattph is not part of the text.
			Assert.That (result.Html, Does.Contain ("A" + Img (image, "a.png") + " B C"));
			Assert.That (result.Text, Does.Contain ("A B C"));
		}

		[Test]
		public void TestRtfPlaceholdersFollowRenderingPosition ()
		{
			var result = ConvertRtf (TwoPlaceholders,
				new Attachment { FileName = "second.png", Content = Png, RenderingPosition = 20 },
				new Attachment { FileName = "first.png", Content = Png, RenderingPosition = 10 });

			Assert.That (result.Related, Has.Count.EqualTo (2));

			var second = result.Related[0];
			var first = result.Related[1];

			Assert.That (second.FileName, Is.EqualTo ("second.png"));
			Assert.That (result.Html, Does.Contain ("A" + Img (first, "first.png") + " B" + Img (second, "second.png") + " C"));
		}

		[Test]
		public void TestRtfPlaceholderCountMismatchAppendsImages ()
		{
			// [MS-OXRTFEX] 2.2.3.4: if the numbers of placeholders and attachments differ, the attachments are appended.
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}",
				new Attachment { FileName = "a.png", Content = Png },
				new Attachment { FileName = "b.jpg", Content = Jpeg });

			Assert.That (result.Related, Has.Count.EqualTo (2));

			var a = result.Related[0];
			var b = result.Related[1];

			Assert.That (result.Html, Does.Contain ("A B"));
			Assert.That (result.Html, Does.Contain ("<div>" + Img (a, "a.png") + "</div><div>" + Img (b, "b.jpg") + "</div>"));
			Assert.That (result.Html.IndexOf ("<img", StringComparison.Ordinal), Is.GreaterThan (result.Html.IndexOf ("A B", StringComparison.Ordinal)));
		}

		[Test]
		public void TestRtfHiddenAndUnrenderedAttachmentsHaveNoPlaceholder ()
		{
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}",
				new Attachment { FileName = "hidden.png", Content = Png, Hidden = true },
				new Attachment { FileName = "unrendered.png", Content = Png, RenderingPosition = -1 },
				new Attachment { FileName = "shown.png", Content = Png });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Related[0].FileName, Is.EqualTo ("shown.png"));
			Assert.That (result.Html, Does.Contain ("A" + Img (result.Related[0], "shown.png") + " B"));

			Assert.That (result.Attachments, Has.Count.EqualTo (2));
			Assert.That (IsInline (result.Attachments[0]), Is.False);
			Assert.That (IsInline (result.Attachments[1]), Is.False);
		}

		[Test]
		public void TestRtfDisplayedImageIsLabeledWithItsActualType ()
		{
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", new Attachment { FileName = "photo", Content = Jpeg, MimeTag = "application/octet-stream" });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Related[0].ContentType.MimeType, Is.EqualTo ("image/jpeg"));
		}

		[TestCase ("application/pdf")]
		[TestCase ("text/html")]
		public void TestRtfImageContentWithOtherDeclaredTypeIsNotDisplayed (string mimeType)
		{
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", new Attachment { FileName = "document", Content = Png, MimeTag = mimeType });

			Assert.That (result.Related, Is.Empty);
			Assert.That (result.Html, Does.Not.Contain ("<img"));
		}

		[Test]
		public void TestRtfDuplicateContentIdsAreReplaced ()
		{
			var result = ConvertRtf (TwoPlaceholders,
				new Attachment { FileName = "a.png", Content = Png, ContentId = "same@example.com" },
				new Attachment { FileName = "b.png", Content = Png, ContentId = "SAME@example.com" });

			Assert.That (result.Related, Has.Count.EqualTo (2));

			var a = result.Related[0];
			var b = result.Related[1];

			Assert.That (a.ContentId, Is.Not.EqualTo (b.ContentId).IgnoreCase);
			Assert.That (result.Html, Does.Contain ("A" + Img (a, "a.png") + " B" + Img (b, "b.png") + " C"));
		}

		[Test]
		public void TestRtfContentIdIsPercentEncoded ()
		{
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", new Attachment { FileName = "a.png", Content = Png, ContentId = "a%b\"c@example.com" });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Html, Does.Contain ("<img src=\"cid:a%25b%22c@example.com\" alt=\"a.png\"/>"));
		}

		[Test]
		public void TestRtfEncapsulatedHtmlIgnoresPlaceholders ()
		{
			// The HTML that is extracted from the RTF refers to its images by itself.
			var result = ConvertRtf ("{\\rtf1\\fromhtml1 {\\*\\htmltag <p>}A\\objattph\\'20 B{\\*\\htmltag </p>}}", new Attachment { FileName = "a.png", Content = Png });

			Assert.That (result.Related, Is.Empty);
			Assert.That (result.Html, Does.Not.Contain ("<img"));
		}

		[Test]
		public void TestRtfEncapsulatedHtmlReferencedAttachmentsAreRelated ()
		{
			// The HTML extracted from the RTF refers to attachments like an HTML best body does ([MS-OXCMAIL] 2.1.3.4.1.2).
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag <p>}A{\\*\\htmltag <img src=\"cid:image1@example.com\">}{\\*\\htmltag <img src=\"cid:image2@example.com\">}B{\\*\\htmltag </p>}}";
			var result = ConvertRtf (rtf,
				new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = "image1@example.com", FileName = "image1.png", Content = Png },
				new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentId = "other@example.com", FileName = "other.png", Content = Png },
				new Attachment { ContentId = "image2@example.com", FileName = "image2.png", Content = Png });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Related[0].FileName, Is.EqualTo ("image1.png"));
			Assert.That (result.Related[0].ContentId, Is.EqualTo ("image1@example.com"));
			Assert.That (IsInline (result.Related[0]), Is.True);

			// Not referenced, and not flagged with afRenderedInBody, respectively.
			Assert.That (result.Attachments, Has.Count.EqualTo (2));
			Assert.That (result.Attachments[0].FileName, Is.EqualTo ("other.png"));
			Assert.That (IsInline (result.Attachments[0]), Is.False);
			Assert.That (result.Attachments[1].FileName, Is.EqualTo ("image2.png"));
			Assert.That (IsInline (result.Attachments[1]), Is.False);

			Assert.That (result.Html, Does.Contain ("<p>A<img src=\"cid:image1@example.com\"/><img src=\"cid:image2@example.com\"/>B</p>"));
			Assert.That (result.Text, Does.Contain ("AB"));
		}

		[Test]
		public void TestRtfEncapsulatedHtmlContentLocationIsRelated ()
		{
			const string rtf = "{\\rtf1\\ansi\\fromhtml1 {\\*\\htmltag <img src=\"http://example.com/image.png\">}}";
			var result = ConvertRtf (rtf,
				new Attachment { Flags = TnefAttachFlags.RenderedInBody, ContentLocation = "http://example.com/image.png", Content = Png });

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Attachments, Is.Empty);
			Assert.That (result.Html, Does.Contain ("<img src=\"http://example.com/image.png\"/>"));
		}

		[Test]
		public void TestRtfWithoutEncapsulatedHtmlIgnoresContentIds ()
		{
			// Rendered RTF does not refer to attachments by Content-Id, even if it contains text that looks like a reference.
			var result = ConvertRtf ("{\\rtf1 <img src=\"cid:image1@example.com\">}", Flagged ());

			Assert.That (result.Related, Is.Empty);
			Assert.That (result.Attachments, Has.Count.EqualTo (1));
			Assert.That (IsInline (result.Attachments[0]), Is.False);
		}

		static string FileNamePlaceholder (TnefAttachment attachment, MimeEntity entity)
		{
			return "<<" + attachment.FileName + ">>";
		}

		[Test]
		public void TestAttachmentPlaceholderCallback ()
		{
			var options = new TnefConversionOptions { AttachmentPlaceholderCallback = FileNamePlaceholder };
			var result = ConvertRtf (TwoPlaceholders, options,
				new Attachment { FileName = "a.png", Content = Png },
				new Attachment { FileName = "b&c.bin" });

			var image = result.Related[0];

			// The text/plain body uses the text for every attachment; the text/html body only for those that it does
			// not display as images, and HTML-encoded.
			Assert.That (result.Text, Does.Contain ("A<<a.png>> B<<b&c.bin>> C"));
			Assert.That (result.Html, Does.Contain ("A" + Img (image, "a.png") + " B&lt;&lt;b&amp;c.bin&gt;&gt; C"));
		}

		[Test]
		public void TestAttachmentPlaceholderCallbackArguments ()
		{
			var calls = new List<(string FileName, MimeEntity Entity)> ();
			var options = new TnefConversionOptions {
				AttachmentPlaceholderCallback = (attachment, entity) => {
					calls.Add ((attachment.FileName, entity));
					return null;
				}
			};
			var result = ConvertRtf (TwoPlaceholders, options,
				new Attachment { FileName = "second.bin", RenderingPosition = 2 },
				new Attachment { FileName = "hidden.bin", Hidden = true },
				new Attachment { FileName = "first.bin", RenderingPosition = 1 });

			// The callback is invoked for the attachments that have placeholders, in placeholder order.
			Assert.That (calls, Has.Count.EqualTo (2));
			Assert.That (calls[0].FileName, Is.EqualTo ("first.bin"));
			Assert.That (calls[0].Entity, Is.SameAs (result.Attachments[2]));
			Assert.That (calls[1].FileName, Is.EqualTo ("second.bin"));
			Assert.That (calls[1].Entity, Is.SameAs (result.Attachments[0]));

			// Returning null writes nothing.
			Assert.That (result.Text, Does.Contain ("A B C"));
			Assert.That (result.Html, Does.Contain ("A B C"));
		}

		[Test]
		public void TestAttachmentPlaceholderCallbackCountMismatchAppendsText ()
		{
			var options = new TnefConversionOptions { AttachmentPlaceholderCallback = FileNamePlaceholder };
			var result = ConvertRtf ("{\\rtf1 A B}", options,
				new Attachment { FileName = "a.png", Content = Png },
				new Attachment { FileName = "b.bin" });

			var image = result.Related[0];

			Assert.That (result.Text, Does.Contain ("A B"));
			Assert.That (result.Text, Does.EndWith (Environment.NewLine + "<<a.png>>" + Environment.NewLine + "<<b.bin>>" + Environment.NewLine));
			Assert.That (result.Html, Does.Contain ("<div>" + Img (image, "a.png") + "</div><div>&lt;&lt;b.bin&gt;&gt;</div>"));
		}

		[Test]
		public void TestAttachmentPlaceholderCallbackWithEncapsulatedHtml ()
		{
			// The extracted HTML does not use the placeholders, but the text/plain body is generated from the RTF.
			var options = new TnefConversionOptions { AttachmentPlaceholderCallback = FileNamePlaceholder };
			var result = ConvertRtf ("{\\rtf1\\fromhtml1 {\\*\\htmltag <p>}A\\objattph\\'20 B{\\*\\htmltag </p>}}", options, new Attachment { FileName = "a.bin" });

			Assert.That (result.Text, Does.Contain ("A<<a.bin>> B"));
			Assert.That (result.Html, Does.Not.Contain ("a.bin"));
		}

		[Test]
		public void TestAttachmentPlaceholderCallbackIsNotUsedForPlainTextBestBody ()
		{
			var options = new TnefConversionOptions { AttachmentPlaceholderCallback = (attachment, entity) => throw new InvalidOperationException () };
			var builder = CreateMessage (Bodies.Plain | Bodies.Rtf, rtfInSync: false);

			AddAttachment (builder, new Attachment { FileName = "a.bin" });

			Assert.That (ConvertBody (builder, options).ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
		}

		#endregion

		#region OLE object converter

		sealed class OleConverter : TnefOleObjectConverter
		{
			readonly Func<TnefAttachment, Stream> convert;

			public readonly List<TnefAttachment> Attachments = new List<TnefAttachment> ();

			public OleConverter (Func<TnefAttachment, Stream> convert)
			{
				this.convert = convert;
			}

			public override Stream Convert (TnefAttachment attachment, CancellationToken cancellationToken = default)
			{
				Attachments.Add (attachment);
				return convert (attachment);
			}
		}

		static Attachment Ole (string displayName = "Chart")
		{
			return new Attachment { Method = TnefAttachMethod.Ole, FileName = "ole.bin", DisplayName = displayName, Content = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } };
		}

		[Test]
		public void TestOleObjectConverterImage ()
		{
			var stream = new NonSeekableStream (Png);
			var converter = new OleConverter (attachment => stream);
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ());

			Assert.That (converter.Attachments, Has.Count.EqualTo (1));
			Assert.That (converter.Attachments[0].Method, Is.EqualTo (TnefAttachMethod.Ole));
			Assert.That (stream.IsDisposed, Is.True, "Disposed");

			// [MS-OXCMAIL] 2.1.3.4.4: the description string is the display name with the image's extension.
			Assert.That (result.Related, Has.Count.EqualTo (1));

			var image = result.Related[0];

			Assert.That (image.ContentType.MimeType, Is.EqualTo ("image/png"));
			Assert.That (image.ContentType.Name, Is.EqualTo ("Chart.png"));
			Assert.That (image.FileName, Is.EqualTo ("Chart.png"));
			Assert.That (image.ContentDescription, Is.EqualTo ("Chart.png"));
			Assert.That (image.ContentDisposition.Size, Is.Null);
			Assert.That (image.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (IsInline (image), Is.True);
			Assert.That (result.Html, Does.Contain ("A" + Img (image, "Chart.png") + " B"));

			using (var memory = new MemoryStream ()) {
				image.Content.DecodeTo (memory);
				Assert.That (memory.ToArray (), Is.EqualTo (Png));
			}
		}

		[Test]
		public void TestOleObjectConverterDescriptionAlreadyHasExtension ()
		{
			var converter = new OleConverter (attachment => new MemoryStream (Jpeg, false));
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ("Photo.JPG"));

			Assert.That (result.Related[0].FileName, Is.EqualTo ("Photo.JPG"));
			Assert.That (result.Related[0].ContentType.MimeType, Is.EqualTo ("image/jpeg"));
		}

		[Test]
		public void TestOleObjectConverterStreamIsReadFromItsCurrentPosition ()
		{
			var data = new byte[3 + Png.Length];

			Png.CopyTo (data, 3);

			var converter = new OleConverter (attachment => new MemoryStream (data, false) { Position = 3 });
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ());

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Related[0].ContentType.MimeType, Is.EqualTo ("image/png"));
		}

		[Test]
		public void TestOleObjectConverterReturnsNull ()
		{
			var converter = new OleConverter (attachment => null);
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ());

			Assert.That (converter.Attachments, Has.Count.EqualTo (1));
			Assert.That (result.Related, Is.Empty);
			Assert.That (result.Attachments, Has.Count.EqualTo (1));
			Assert.That (result.Attachments[0].ContentType.MimeType, Is.EqualTo ("application/octet-stream"));
			Assert.That (result.Attachments[0].FileName, Is.EqualTo ("ole.bin"));
		}

		[Test]
		public void TestOleObjectConverterReturnsNonImage ()
		{
			var stream = new NonSeekableStream (Encoding.ASCII.GetBytes ("<svg xmlns=\"http://www.w3.org/2000/svg\"/>"));
			var converter = new OleConverter (attachment => stream);
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ());

			Assert.That (stream.IsDisposed, Is.True, "Disposed");
			Assert.That (result.Related, Is.Empty);
			Assert.That (result.Attachments, Has.Count.EqualTo (1));
			Assert.That (result.Attachments[0].FileName, Is.EqualTo ("ole.bin"));

			using (var memory = new MemoryStream ()) {
				result.Attachments[0].Content.DecodeTo (memory);
				Assert.That (memory.ToArray (), Is.EqualTo (new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }));
			}
		}

		static TnefConversionLossKind[] ConvertOleLosses (TnefConversionOptions options, Attachment attachment)
		{
			var builder = CreateMessage (Bodies.Rtf, rtf: CompressedRtf ("{\\rtf1 A\\objattph\\'20 B}"));

			AddAttachment (builder, attachment);

			using (var tnef = TnefMessage.Load (builder.ToStream ())) {
				var result = tnef.ConvertToMime (options ?? new TnefConversionOptions ());

				return result.Losses.Select (loss => loss.Kind).ToArray ();
			}
		}

		[Test]
		public void TestOleObjectNotRenderedWithoutConverterIsALoss ()
		{
			Assert.That (ConvertOleLosses (null, Ole ()), Is.EqualTo (new[] { TnefConversionLossKind.OleObjectNotRendered }));
		}

		[Test]
		public void TestOleObjectNotRenderedByConverterIsALoss ()
		{
			var options = new TnefConversionOptions { OleObjectConverter = new OleConverter (attachment => null) };

			Assert.That (ConvertOleLosses (options, Ole ()), Is.EqualTo (new[] { TnefConversionLossKind.OleObjectNotRendered }));

			options = new TnefConversionOptions { OleObjectConverter = new OleConverter (attachment => new MemoryStream (new byte[] { 1, 2, 3 }, false)) };

			Assert.That (ConvertOleLosses (options, Ole ()), Is.EqualTo (new[] { TnefConversionLossKind.OleObjectNotRendered }));
		}

		[Test]
		public void TestOleObjectRenderedIsNotALoss ()
		{
			var options = new TnefConversionOptions { OleObjectConverter = new OleConverter (attachment => new MemoryStream (Png, false)) };

			Assert.That (ConvertOleLosses (options, Ole ()), Is.Empty);
			Assert.That (ConvertOleLosses (null, new Attachment { FileName = "a.bin" }), Is.Empty);
		}

		[Test]
		public void TestOleObjectConverterIsOnlyUsedForOleAttachments ()
		{
			var converter = new OleConverter (attachment => new MemoryStream (Png, false));
			var options = new TnefConversionOptions { OleObjectConverter = converter };

			ConvertRtf ("{\\rtf1 A}", options, new Attachment { FileName = "a.bin" });

			Assert.That (converter.Attachments, Is.Empty);
		}

		[Test]
		public void TestOleObjectConverterWithPlainTextBestBody ()
		{
			// The OLE object is still rendered as an image, but it is not displayed in a body.
			var converter = new OleConverter (attachment => new MemoryStream (Png, false));
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var builder = CreateMessage (Bodies.Plain);

			AddAttachment (builder, Ole ());

			var body = (Multipart) ConvertBody (builder, options);

			Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (body[1].ContentType.MimeType, Is.EqualTo ("image/png"));
			Assert.That (IsInline (body[1]), Is.False);
		}

		[Test]
		public async Task TestOleObjectConverterConvertAsyncDefaultsToConvert ()
		{
			var converter = new OleConverter (attachment => new MemoryStream (Png, false));
			var builder = CreateMessage (Bodies.Plain);

			AddAttachment (builder, Ole ());

			using (var tnef = TnefMessage.Load (builder.ToStream ())) {
				using (var stream = await converter.ConvertAsync (tnef.Attachments[0])) {
					Assert.That (stream, Is.Not.Null);
					Assert.That (converter.Attachments, Has.Count.EqualTo (1));
				}
			}
		}

		static byte[] Bmp (int dibHeaderSize)
		{
			var bmp = new byte[18];

			bmp[0] = (byte) 'B';
			bmp[1] = (byte) 'M';
			BitConverter.GetBytes (dibHeaderSize).CopyTo (bmp, 14);

			return bmp;
		}

		static IEnumerable<TestCaseData> OleImages ()
		{
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("GIF87a"), "image/gif", "Chart.gif");
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("GIF89a"), "image/gif", "Chart.gif");
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("RIFF\0\0\0\0WEBPVP8 "), "image/webp", "Chart.webp");

			foreach (var size in new[] { 12, 40, 52, 56, 64, 108, 124 })
				yield return new TestCaseData (Bmp (size), "image/bmp", "Chart.bmp");

			// Signatures that are close, but not quite.
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("GIF88a"), null, null);
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("GIF89b"), null, null);
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("RIFF\0\0\0\0WAVEfmt "), null, null);
			yield return new TestCaseData (Bmp (41), null, null);
			yield return new TestCaseData (Encoding.ASCII.GetBytes ("BM"), null, null);
			yield return new TestCaseData (new byte[] { 0xFF, 0xD8 }, null, null);
			yield return new TestCaseData (new byte[] { 0x89, (byte) 'P', (byte) 'N', (byte) 'G', 0x0D, 0x0A, 0x1A, 0x00 }, null, null);
		}

		[TestCaseSource (nameof (OleImages))]
		public void TestOleObjectConverterImageFormats (byte[] image, string mimeType, string fileName)
		{
			var converter = new OleConverter (attachment => new MemoryStream (image, false));
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole ());

			if (mimeType is null) {
				Assert.That (result.Related, Is.Empty);
				Assert.That (result.Attachments, Has.Count.EqualTo (1));
				Assert.That (result.Attachments[0].FileName, Is.EqualTo ("ole.bin"));
				return;
			}

			Assert.That (result.Related, Has.Count.EqualTo (1));
			Assert.That (result.Related[0].ContentType.MimeType, Is.EqualTo (mimeType));
			Assert.That (result.Related[0].FileName, Is.EqualTo (fileName));
		}

		[TestCase ("", "ole.bin.png")]
		[TestCase (null, "ole.bin.png")]
		public void TestOleObjectConverterDescriptionFallback (string displayName, string expected)
		{
			var converter = new OleConverter (attachment => new MemoryStream (Png, false));
			var options = new TnefConversionOptions { OleObjectConverter = converter };
			var result = ConvertRtf ("{\\rtf1 A\\objattph\\'20 B}", options, Ole (displayName));

			Assert.That (result.Related[0].FileName, Is.EqualTo (expected));
		}

		#endregion
	}
}
