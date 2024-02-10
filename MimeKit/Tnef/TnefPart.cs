//
// TnefPart.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Text;
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
	public class TnefPart : MimePart, ITnefPart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefPart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public TnefPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefPart"/> class.
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

		void CheckDisposed ()
		{
			CheckDisposed (nameof (TnefPart));
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="TnefPart"/> nodes
		/// calls <see cref="MimeVisitor.VisitTnefPart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitTnefPart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitTnefPart (this);
		}

		static void ExtractRecipientTable (TnefReader reader, MimeMessage message)
		{
			var prop = reader.TnefPropertyReader;

			// Note: The RecipientTable uses rows of properties...
			while (prop.ReadNextRow ()) {
				string transmitableDisplayName = null;
				string recipientDisplayName = null;
				string displayName = string.Empty;
				InternetAddressList list = null;
				string addr = null;

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
						transmitableDisplayName = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.RecipientDisplayName:
						recipientDisplayName = prop.ReadValueAsString ();
						break;
					case TnefPropertyId.DisplayName:
						displayName = prop.ReadValueAsString ();
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

				if (list != null && !string.IsNullOrEmpty (addr)) {
					var name = recipientDisplayName ?? transmitableDisplayName ?? displayName;

					list.Add (new MailboxAddress (name, addr));
				}
			}
		}

		class EmailAddress
		{
			public string AddrType = "SMTP";
			public string SearchKey;
			public string Name;
			public string Addr;

			bool CanUseSearchKey {
				get {
					return SearchKey != null && SearchKey.Equals ("SMTP", StringComparison.OrdinalIgnoreCase) &&
						SearchKey.Length > AddrType.Length && SearchKey.StartsWith (AddrType, StringComparison.Ordinal) &&
						SearchKey[AddrType.Length] == ':';
				}
			}

			//public bool IsComplete {
			//	get {
			//		return !string.IsNullOrEmpty (Addr) || CanUseSearchKey;
			//	}
			//}

			public bool TryGetMailboxAddress (out MailboxAddress mailbox)
			{
				string addr;

				if (string.IsNullOrEmpty (Addr) && CanUseSearchKey)
					addr = SearchKey.Substring (AddrType.Length + 1);
				else
					addr = Addr;

				if (string.IsNullOrEmpty (addr) || !MailboxAddress.TryParse (addr, out mailbox)) {
					mailbox = null;
					return false;
				}

				mailbox.Name = Name;

				return true;
			}
		}

		static string GetHtmlBody (TnefPropertyReader prop, int codepage, out Encoding encoding)
		{
			var rawValue = prop.ReadValueAsBytes ();
			int rawLength = rawValue.Length;

			while (rawLength > 0 && rawValue[rawLength - 1] == 0)
				rawLength--;

			// Try and extract the charset from the HTML meta Content-Type value.
			using (var stream = new MemoryStream (rawValue, 0, rawLength)) {
				// It should be safe to assume ISO-8859-1 for this purpose. We don't want to risk using UTF-8 (or any other charset) and having it throw an exception...
				using (var reader = new StreamReader (stream, CharsetUtils.Latin1, true)) {
					var tokenizer = new HtmlTokenizer (reader);

					while (tokenizer.ReadNextToken (out var token)) {
						if (token.Kind != HtmlTokenKind.Tag)
							continue;

						var tag = (HtmlTagToken) token;

						if (tag.Id == HtmlTagId.Body || (tag.Id == HtmlTagId.Head && tag.IsEndTag))
							break;

						if (tag.Id != HtmlTagId.Meta || tag.IsEndTag)
							continue;

						string httpEquiv = null;
						string content = null;

						for (int i = 0; i < tag.Attributes.Count; i++) {
							switch (tag.Attributes[i].Id) {
							case HtmlAttributeId.HttpEquiv: httpEquiv ??= tag.Attributes[i].Value; break;
							case HtmlAttributeId.Content: content ??= tag.Attributes[i].Value; break;
							}
						}

						if (httpEquiv is null || content is null || !httpEquiv.Equals ("Content-Type", StringComparison.OrdinalIgnoreCase))
							continue;

						if (!ContentType.TryParse (content, out var contentType) || string.IsNullOrEmpty (contentType.Charset))
							break;

						try {
							encoding = Encoding.GetEncoding (contentType.Charset, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

							// If this converts cleanly, then we're golden.
							return encoding.GetString (rawValue, 0, rawLength);
						} catch {
							// Otherwise, fall back to assuming the TNEF message codepage.
							break;
						}
					}

					encoding = Encoding.GetEncoding (codepage);

					return encoding.GetString (rawValue, 0, rawLength);
				}
			}
		}

		static void ExtractMapiProperties (TnefReader reader, MimeMessage message, MultipartAlternative alternatives)
		{
			var prop = reader.TnefPropertyReader;
			var recipient = new EmailAddress ();
			var sender = new EmailAddress ();
			string normalizedSubject = null;
			string subjectPrefix = null;
			MailboxAddress mailbox;
			var msgid = false;

			while (prop.ReadNextProperty ()) {
				switch (prop.PropertyTag.Id) {
				case TnefPropertyId.InternetMessageId:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.MessageId = prop.ReadValueAsString ();
						msgid = true;
					}
					break;
				case TnefPropertyId.TnefCorrelationKey:
					// According to MSDN, PidTagTnefCorrelationKey is a unique key that is
					// meant to be used to tie the TNEF attachment to the encapsulating
					// message. It can be a string or a binary blob. It seems that most
					// implementations use the Message-Id string, so if this property
					// value looks like a Message-Id, then us it as one (unless we get a
					// InternetMessageId property, in which case we use that instead.
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						if (!msgid) {
							var value = prop.ReadValueAsString ();

							if (value.Length > 5 && value[0] == '<' && value[value.Length - 1] == '>' && value.IndexOf ('@') != -1)
								message.MessageId = value;
						}
					}
					break;
				case TnefPropertyId.Subject:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						message.Subject = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.SubjectPrefix:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						subjectPrefix = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.NormalizedSubject:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						normalizedSubject = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.SenderName:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						sender.Name = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.SenderEmailAddress:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						sender.Addr = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.SenderSearchKey:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						sender.SearchKey = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.SenderAddrtype:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						sender.AddrType = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.ReceivedByName:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						recipient.Name = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.ReceivedByEmailAddress:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						recipient.Addr = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.ReceivedBySearchKey:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						recipient.SearchKey = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.ReceivedByAddrtype:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode) {
						recipient.AddrType = prop.ReadValueAsString ();
					}
					break;
				case TnefPropertyId.RtfCompressed:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						var converter = new RtfCompressedToRtf ();
						var content = new MemoryBlockStream ();

						using (var filtered = new FilteredStream (content)) {
							filtered.Add (converter);

							using (var compressed = prop.GetRawValueReadStream ()) {
								compressed.CopyTo (filtered, 4096);
								filtered.Flush ();
							}
						}

						content.Position = 0;

						var rtf = new TextPart ("rtf") {
							Content = new MimeContent (content)
						};

						alternatives.Add (rtf);
					}
					break;
				case TnefPropertyId.BodyHtml:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						Encoding encoding;
						string text;

						if (prop.PropertyTag.ValueTnefType != TnefPropertyType.Unicode) {
							text = GetHtmlBody (prop, reader.MessageCodepage, out encoding);
						} else {
							text = prop.ReadValueAsString ();
							encoding = CharsetUtils.UTF8;
						}

						var html = new TextPart ("html");
						html.SetText (encoding, text);

						alternatives.Add (html);
					}
					break;
				case TnefPropertyId.Body:
					if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
						prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary) {
						Encoding encoding;

						if (prop.PropertyTag.ValueTnefType != TnefPropertyType.Unicode)
							encoding = Encoding.GetEncoding (reader.MessageCodepage);
						else
							encoding = CharsetUtils.UTF8;

						var text = prop.ReadValueAsString ();
						var plain = new TextPart ("plain");

						plain.SetText (encoding, text);

						alternatives.Add (plain);
					}
					break;
				case TnefPropertyId.Importance:
					// https://msdn.microsoft.com/en-us/library/ee237166(v=exchg.80).aspx
					switch (prop.ReadValueAsInt32 ()) {
					case 2: message.Importance = MessageImportance.High; break;
					case 1: message.Importance = MessageImportance.Normal; break;
					case 0: message.Importance = MessageImportance.Low; break;
					}
					break;
				case TnefPropertyId.Priority:
					// https://msdn.microsoft.com/en-us/library/ee159473(v=exchg.80).aspx
					switch (prop.ReadValueAsInt32 ()) {
					case  1: message.Priority = MessagePriority.Urgent; break;
					case  0: message.Priority = MessagePriority.Normal; break;
					case -1: message.Priority = MessagePriority.NonUrgent; break;
					}
					break;
				case TnefPropertyId.Sensitivity:
					// https://msdn.microsoft.com/en-us/library/ee217353(v=exchg.80).aspx
					// https://tools.ietf.org/html/rfc2156#section-5.3.4
					switch (prop.ReadValueAsInt32 ()) {
					case 1: message.Headers[HeaderId.Sensitivity] = "Personal"; break;
					case 2: message.Headers[HeaderId.Sensitivity] = "Private"; break;
					case 3: message.Headers[HeaderId.Sensitivity] = "Company-Confidential"; break;
					case 0: message.Headers.Remove (HeaderId.Sensitivity); break;
					}
					break;
				}
			}

			if (string.IsNullOrEmpty (message.Subject) && !string.IsNullOrEmpty (normalizedSubject)) {
				if (!string.IsNullOrEmpty (subjectPrefix))
					message.Subject = subjectPrefix + normalizedSubject;
				else
					message.Subject = normalizedSubject;
			}

			if (sender.TryGetMailboxAddress (out mailbox))
				message.From.Add (mailbox);

			if (recipient.TryGetMailboxAddress (out mailbox))
				message.To.Add (mailbox);
		}

		static TnefPart PromoteToTnefPart (MimePart part)
		{
			var tnef = new TnefPart ();

			try {
				foreach (var param in part.ContentType.Parameters)
					tnef.ContentType.Parameters[param.Name] = param.Value;

				if (part.ContentDisposition != null)
					tnef.ContentDisposition = part.ContentDisposition;

				tnef.ContentTransferEncoding = part.ContentTransferEncoding;
			} catch {
				tnef.Dispose ();
				throw;
			}

			return tnef;
		}

		static void ExtractAttachments (TnefReader reader, Multipart attachments)
		{
			var attachMethod = TnefAttachMethod.ByValue;
			var filter = new BestEncodingFilter ();
			var prop = reader.TnefPropertyReader;
			MimePart attachment = null;
			TnefAttachFlags flags;
			bool dispose = false;
			string[] mimeType;
			byte[] attachData;
			string text;

			try {
				do {
					if (reader.AttributeLevel != TnefAttributeLevel.Attachment)
						break;

					switch (reader.AttributeTag) {
					case TnefAttributeTag.AttachRenderData:
						attachMethod = TnefAttachMethod.ByValue;
						if (dispose)
							attachment.Dispose ();
						attachment = new MimePart ();
						dispose = true;
						break;
					case TnefAttributeTag.Attachment:
						if (attachment is null)
							break;

						attachData = null;

						while (prop.ReadNextProperty ()) {
							switch (prop.PropertyTag.Id) {
							case TnefPropertyId.AttachLongFilename:
								attachment.FileName = prop.ReadValueAsString ();
								break;
							case TnefPropertyId.AttachFilename:
								attachment.FileName ??= prop.ReadValueAsString ();
								break;
							case TnefPropertyId.AttachContentLocation:
								attachment.ContentLocation = prop.ReadValueAsUri ();
								break;
							case TnefPropertyId.AttachContentBase:
								attachment.ContentBase = prop.ReadValueAsUri ();
								break;
							case TnefPropertyId.AttachContentId:
								text = prop.ReadValueAsString ();

								var buffer = CharsetUtils.UTF8.GetBytes (text);
								int index = 0;

								if (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out string msgid))
									attachment.ContentId = msgid;
								break;
							case TnefPropertyId.AttachDisposition:
								text = prop.ReadValueAsString ();
								if (ContentDisposition.TryParse (text, out ContentDisposition disposition))
									attachment.ContentDisposition = disposition;
								break;
							case TnefPropertyId.AttachData:
								attachData = prop.ReadValueAsBytes ();
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
									if (attachment.ContentDisposition is null)
										attachment.ContentDisposition = new ContentDisposition (ContentDisposition.Inline);
									else
										attachment.ContentDisposition.Disposition = ContentDisposition.Inline;
								}
								break;
							case TnefPropertyId.AttachSize:
								attachment.ContentDisposition ??= new ContentDisposition ();
								attachment.ContentDisposition.Size = prop.ReadValueAsInt64 ();
								break;
							case TnefPropertyId.DisplayName:
								attachment.ContentType.Name = prop.ReadValueAsString ();
								break;
							}
						}

						if (attachData != null) {
							int count = attachData.Length;
							int index = 0;

							if (attachMethod == TnefAttachMethod.EmbeddedMessage) {
								attachment.ContentTransferEncoding = ContentEncoding.Base64;
								attachment = PromoteToTnefPart (attachment);
								count -= 16;
								index = 16;
							} else if (attachment.ContentType.IsMimeType ("text", "*")) {
								filter.Flush (attachData, index, count, out _, out _);
								attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
								filter.Reset ();
							} else {
								attachment.ContentTransferEncoding = ContentEncoding.Base64;
							}

							attachment.Content = new MimeContent (new MemoryStream (attachData, index, count, false));
							attachments.Add (attachment);
							dispose = false;
						}
						break;
					case TnefAttributeTag.AttachCreateDate:
						if (attachment != null) {
							attachment.ContentDisposition ??= new ContentDisposition ();
							attachment.ContentDisposition.CreationDate = prop.ReadValueAsDateTime ();
						}
						break;
					case TnefAttributeTag.AttachModifyDate:
						if (attachment != null) {
							attachment.ContentDisposition ??= new ContentDisposition ();
							attachment.ContentDisposition.ModificationDate = prop.ReadValueAsDateTime ();
						}
						break;
					case TnefAttributeTag.AttachTitle:
						if (attachment != null && string.IsNullOrEmpty (attachment.FileName))
							attachment.FileName = prop.ReadValueAsString ();
						break;
					case TnefAttributeTag.AttachMetaFile:
						if (attachment is null)
							break;

						// TODO: what to do with the meta data?
						break;
					case TnefAttributeTag.AttachData:
						if (attachment is null || attachMethod != TnefAttachMethod.ByValue)
							break;

						attachData = prop.ReadValueAsBytes ();

						if (attachment.ContentType.IsMimeType ("text", "*")) {
							filter.Flush (attachData, 0, attachData.Length, out _, out _);
							attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
							filter.Reset ();
						} else {
							attachment.ContentTransferEncoding = ContentEncoding.Base64;
						}

						attachment.Content = new MimeContent (new MemoryStream (attachData, false));
						attachments.Add (attachment);
						dispose = false;
						break;
					}
				} while (reader.ReadNextAttribute ());
			} finally {
				if (dispose)
					attachment.Dispose ();
			}
		}

		static MimeMessage ExtractTnefMessage (TnefReader reader)
		{
			var message = new MimeMessage ();
			var alternatives = new MultipartAlternative ();
			MimeEntity body = null;

			try {
				message.Headers.Remove (HeaderId.Date);

				while (reader.ReadNextAttribute ()) {
					if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
						break;

					var prop = reader.TnefPropertyReader;

					switch (reader.AttributeTag) {
					case TnefAttributeTag.RecipientTable:
						ExtractRecipientTable (reader, message);
						break;
					case TnefAttributeTag.MapiProperties:
						ExtractMapiProperties (reader, message, alternatives);
						break;
					case TnefAttributeTag.DateSent:
						message.Date = prop.ReadValueAsDateTime ();
						break;
					case TnefAttributeTag.Body:
						var text = prop.ReadValueAsString ();

						body = new TextPart ("plain") {
							Text = text
						};
						break;
					}
				}
			} catch {
				alternatives.Dispose ();
				message.Dispose ();
				body?.Dispose ();
				throw;
			}

			if (body != null) {
				if (alternatives.Count > 0) {
					alternatives.Add (body);
					body = alternatives;
				} else {
					alternatives.Dispose ();
				}
			} else if (alternatives.Count > 0) {
				if (alternatives.Count == 1) {
					body = alternatives[0];
					alternatives.Clear (false);
					alternatives.Dispose ();
				} else {
					body = alternatives;
				}
			} else {
				alternatives.Dispose ();
			}

			if (reader.AttributeLevel == TnefAttributeLevel.Attachment) {
				var attachments = new Multipart ("mixed");

				if (body != null)
					attachments.Add (body);

				message.Body = attachments;

				try {
					ExtractAttachments (reader, attachments);
				} catch {
					message.Dispose ();
					throw;
				}

				if (attachments.Count == 1) {
					message.Body = attachments[0];
					attachments.Clear (false);
					attachments.Dispose ();
				}
			} else {
				message.Body = body;
			}

			return message;
		}

		/// <summary>
		/// Convert the TNEF content into a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// TNEF data often contains properties that map to <see cref="MimeMessage"/>
		/// headers. TNEF data also often contains file attachments which will be
		/// mapped to MIME parts.
		/// </remarks>
		/// <returns>A message representing the TNEF data in MIME format.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="MimePart.Content"/> property is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
		/// </exception>
		public MimeMessage ConvertToMessage ()
		{
			CheckDisposed ();

			if (Content is null)
				throw new InvalidOperationException ("Cannot parse null TNEF data.");

			int codepage = 0;

			if (!string.IsNullOrEmpty (ContentType.Charset)) {
				if ((codepage = CharsetUtils.GetCodePage (ContentType.Charset)) == -1)
					codepage = 0;
			}

			using (var reader = new TnefReader (Content.Open (), codepage, TnefComplianceMode.Loose))
				return ExtractTnefMessage (reader);
		}

		/// <summary>
		/// Extract the embedded attachments from the TNEF data.
		/// </summary>
		/// <remarks>
		/// Parses the TNEF data and extracts all of the embedded file attachments.
		/// </remarks>
		/// <returns>The attachments.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="MimePart.Content"/> property is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
		/// </exception>
		public IEnumerable<MimeEntity> ExtractAttachments ()
		{
			var message = ConvertToMessage ();
			var body = message.Body;

			// we don't need the message container itself
			message.Body = null;
			message.Dispose ();

			if (body is Multipart multipart) {
				if (multipart.Count > 0 && multipart[0] is MultipartAlternative alternatives) {
					foreach (var alternative in alternatives)
						yield return alternative;

					alternatives.Clear (false);
					alternatives.Dispose ();
					multipart.RemoveAt (0);
				}

				foreach (var part in multipart)
					yield return part;

				multipart.Clear ();
				multipart.Dispose ();
			} else if (body != null) {
				yield return body;
			}

			yield break;
		}
	}
}
