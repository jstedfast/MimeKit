//
// TnefPart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#else
using Encoding = System.Text.Encoding;
#endif

using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace MimeKit.Tnef {
	/// <summary>
	/// A MIME part containing Microsoft TNEF data.
	/// </summary>
	/// <remarks>
	/// <para>Represents an application/ms-tnef or application/vnd.ms-tnef part.</para>
	/// <para>TNEF (Transport Neutral Encapsulation Format) attachments are most often
	/// sent by Microsoft Outlook clients.</para>
	/// </remarks>
	public class TnefPart : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefPart"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public TnefPart (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefPart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefPart"/> with a Content-Type of application/vnd.ms-tnef
		/// and a Content-Disposition value of "attachment" and a filename paremeter with a
		/// value of "winmail.dat".
		/// </remarks>
		public TnefPart () : base ("application", "vnd.ms-tnef")
		{
			FileName = "winmail.dat";
		}

		static void ExtractRecipientTable (TnefReader reader, MimeMessage message)
		{
			var prop = reader.TnefPropertyReader;

			// Note: The RecipientTable uses rows of properties...
			while (prop.ReadNextRow ()) {
				InternetAddressList list = null;
				string name = null, addr = null;

				while (prop.ReadNextProperty ()) {
					switch (prop.PropertyTag.Id) {
					case TnefPropertyId.RecipientType:
						int recipientType = prop.ReadValueAsInt32 ();
						switch (recipientType) {
						case 1: list = message.To; break;
						case 2: list = message.Cc; break;
						case 3: list = message.Bcc; break;
						}
						break;
					case TnefPropertyId.TransmitableDisplayName:
						if (string.IsNullOrEmpty (name))
							name = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.DisplayName:
						name = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.EmailAddress:
						if (string.IsNullOrEmpty (addr))
							addr = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.SmtpAddress:
						// The SmtpAddress, if it exists, should take precedence over the EmailAddress
						// (since the SmtpAddress is meant to be used in the RCPT TO command).
						addr = prop.ReadValueAsString ();
						break;
					}
				}

				if (list != null && !string.IsNullOrEmpty (addr))
					list.Add (new MailboxAddress (name, addr));
			}
		}

		static void ExtractMapiProperties (TnefReader reader, MimeMessage message, BodyBuilder builder)
		{
			var prop = reader.TnefPropertyReader;

			while (prop.ReadNextProperty ()) {
				switch (prop.PropertyTag.Id) {
				case TnefPropertyId.InternetMessageId:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.MessageId = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.Subject:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.Subject = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.RtfCompressed:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						var rtf = new TextPart ("rtf");
						rtf.ContentType.Name = "body.rtf";

						var converter = new RtfCompressedToRtf ();
						var content = new MemoryBlockStream ();

						using (var filtered = new FilteredStream (content)) {
							filtered.Add (converter);

							using (var compressed = prop.GetRawValueReadStream ()) {
								compressed.CopyTo (filtered, 4096);
								filtered.Flush ();
							}
						}

						rtf.ContentObject = new ContentObject (content);
						content.Position = 0;

						builder.Attachments.Add (rtf);
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
					}
					break;
				}
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
			string text;

			do {
				if (reader.AttributeLevel != TnefAttributeLevel.Attachment)
					break;

				switch (reader.AttributeTag) {
				case TnefAttributeTag.AttachRenderData:
					attachMethod = TnefAttachMethod.ByValue;
					attachment = new MimePart ();
					break;
				case TnefAttributeTag.Attachment:
					if (attachment == null)
						break;

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
							var stream = prop.GetRawValueReadStream ();
							var content = new MemoryStream ();
							var guid = new byte[16];

							if (attachMethod == TnefAttachMethod.EmbeddedMessage) {
								var tnef = new TnefPart ();

								foreach (var param in attachment.ContentType.Parameters)
									tnef.ContentType.Parameters[param.Name] = param.Value;

								if (attachment.ContentDisposition != null)
									tnef.ContentDisposition = attachment.ContentDisposition;

								attachment = tnef;
							}

							// read the GUID
							stream.Read (guid, 0, 16);

							// the rest is content
							using (var filtered = new FilteredStream (content)) {
								filtered.Add (filter);
								stream.CopyTo (filtered, 4096);
								filtered.Flush ();
							}

							content.Position = 0;

							attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
							attachment.ContentObject = new ContentObject (content);
							filter.Reset ();

							builder.Attachments.Add (attachment);
							break;
						case TnefPropertyId.AttachMethod:
							attachMethod = (TnefAttachMethod) prop.ReadValueAsInt32 ();
							break;
						case TnefPropertyId.AttachMimeTag:
							mimeType = prop.ReadValueAsString ().Split ('/');
							if (mimeType.Length == 2) {
								attachment.ContentType.MediaType = mimeType[0].Trim ();
								attachment.ContentType.MediaSubtype = mimeType[1].Trim ();
							}
							break;
						case TnefPropertyId.AttachFlags:
							flags = (TnefAttachFlags) prop.ReadValueAsInt32 ();
							if ((flags & TnefAttachFlags.RenderedInBody) != 0) {
								if (attachment.ContentDisposition == null)
									attachment.ContentDisposition = new ContentDisposition (ContentDisposition.Inline);
								else
									attachment.ContentDisposition.Disposition = ContentDisposition.Inline;
							}
							break;
						case TnefPropertyId.AttachSize:
							if (attachment.ContentDisposition == null)
								attachment.ContentDisposition = new ContentDisposition ();

							attachment.ContentDisposition.Size = prop.ReadValueAsInt64 ();
							break;
						case TnefPropertyId.DisplayName:
							attachment.ContentType.Name = prop.ReadValueAsString ();
							break;
						}
					}
					break;
				case TnefAttributeTag.AttachCreateDate:
					if (attachment != null) {
						if (attachment.ContentDisposition == null)
							attachment.ContentDisposition = new ContentDisposition ();

						attachment.ContentDisposition.CreationDate = prop.ReadValueAsDateTime ();
					}
					break;
				case TnefAttributeTag.AttachModifyDate:
					if (attachment != null) {
						if (attachment.ContentDisposition == null)
							attachment.ContentDisposition = new ContentDisposition ();

						attachment.ContentDisposition.ModificationDate = prop.ReadValueAsDateTime ();
					}
					break;
				case TnefAttributeTag.AttachTitle:
					if (attachment != null && string.IsNullOrEmpty (attachment.FileName))
						attachment.FileName = prop.ReadValueAsString ();
					break;
				case TnefAttributeTag.AttachMetaFile:
					if (attachment == null)
						break;

					// TODO: what to do with the meta data?
					break;
				case TnefAttributeTag.AttachData:
					if (attachment == null || attachMethod != TnefAttachMethod.ByValue)
						break;

					attachData = prop.ReadValueAsBytes ();
					filter.Flush (attachData, 0, attachData.Length, out outIndex, out outLength);
					attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					attachment.ContentObject = new ContentObject (new MemoryStream (attachData, false));
					filter.Reset ();

					builder.Attachments.Add (attachment);
					break;
				}
			} while (reader.ReadNextAttribute ());
		}

		static MimeMessage ExtractTnefMessage (TnefReader reader)
		{
			var builder = new BodyBuilder ();
			var message = new MimeMessage ();

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
					break;

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
					break;
				case TnefAttributeTag.Body:
					builder.TextBody = prop.ReadValueAsString ();
					break;
				}
			}

			if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
				ExtractAttachments (reader, builder);

			message.Body = builder.ToMessageBody ();

			return message;
		}

		/// <summary>
		/// Converts the TNEF content into a <see cref="MimeKit.MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// TNEF data often contains properties that map to <see cref="MimeKit.MimeMessage"/>
		/// headers. TNEF data also often contains file attachments which will be
		/// mapped to MIME parts.
		/// </remarks>
		/// <returns>A message representing the TNEF data in MIME format.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="MimeKit.MimePart.ContentObject"/> property is <c>null</c>.
		/// </exception>
		public MimeMessage ConvertToMessage ()
		{
			if (ContentObject == null)
				throw new InvalidOperationException ("Cannot parse TNEF data without a ContentObject.");

			int codepage = 0;

			if (!string.IsNullOrEmpty (ContentType.Charset)) {
				if ((codepage = CharsetUtils.GetCodePage (ContentType.Charset)) == -1)
					codepage = 0;
			}

			using (var reader = new TnefReader (ContentObject.Open (), codepage, TnefComplianceMode.Loose)) {
				return ExtractTnefMessage (reader);
			}
		}

		/// <summary>
		/// Extracts the embedded attachments from the TNEF data.
		/// </summary>
		/// <remarks>
		/// Parses the TNEF data and extracts all of the embedded file attachments.
		/// </remarks>
		/// <returns>The attachments.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="MimeKit.MimePart.ContentObject"/> property is <c>null</c>.
		/// </exception>
		public IEnumerable<MimePart> ExtractAttachments ()
		{
			var message = ConvertToMessage ();

			foreach (var attachment in message.BodyParts)
				yield return attachment;

			yield break;
		}
	}
}
