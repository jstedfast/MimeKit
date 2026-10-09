//
// TnefMessageLoader.cs
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

using System;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Buffers.Binary;
using System.Collections.Generic;

using MimeKit.IO;

namespace MimeKit.Tnef {
	// Builds a TnefPropertySet from MAPI properties and from the properties that were decoded from legacy attributes.
	// When both are present, the MAPI property takes precedence ([MS-OXTNEF] section 2.1.3.4).
	sealed class TnefPropertySetBuilder
	{
		readonly List<TnefProperty> properties = new List<TnefProperty> ();
		HashSet<TnefPropertyId>? legacy;

		int IndexOf (TnefPropertyId id)
		{
			for (int i = 0; i < properties.Count; i++) {
				if (properties[i].Tag.Id == id)
					return i;
			}

			return -1;
		}

		public void Add (TnefProperty property)
		{
			// Note: A MAPI property that has a value replaces the property that was decoded from a legacy attribute.
			if (legacy != null && property.Name is null && property.Count > 0 && legacy.Remove (property.Tag.Id)) {
				int index = IndexOf (property.Tag.Id);

				if (index != -1)
					properties.RemoveAt (index);
			}

			properties.Add (property);
		}

		public void AddLegacy (TnefPropertyTag tag, object value, Encoding encoding)
		{
			if (IndexOf (tag.Id) != -1)
				return;

			legacy ??= new HashSet<TnefPropertyId> ();
			legacy.Add (tag.Id);

			properties.Add (new TnefProperty (tag, null, 1, value, encoding));
		}

		public string? GetString (TnefPropertyId id)
		{
			int index = IndexOf (id);

			return index != -1 && properties[index].TryGetString (out var value) ? value : null;
		}

		public TnefPropertySet ToPropertySet ()
		{
			var set = new TnefPropertySet ();

			for (int i = 0; i < properties.Count; i++)
				set.Add (properties[i]);

			return set;
		}
	}

	// Loads a TnefMessage from a TnefReader.
	//
	// Note: The I/O is done by the methods in this file and mirrored by AsyncTnefMessageLoader.cs. Everything that does not
	// require I/O is shared.
	sealed partial class TnefMessageLoader
	{
		sealed class AttachmentBuilder
		{
			public readonly TnefPropertySetBuilder Properties = new TnefPropertySetBuilder ();
			public MemoryBlockStream? Content;
			public long ContentAbsoluteOffset;
			public long ContentOffset;
			public bool ContentFromMapi;
			public bool Embedded;

			public void SetContent (MemoryBlockStream content, long absoluteOffset, long offset, bool fromMapi, bool embedded)
			{
				Content?.Dispose ();
				Content = content;
				ContentAbsoluteOffset = absoluteOffset;
				ContentOffset = offset;
				ContentFromMapi = fromMapi;
				Embedded = embedded;
			}
		}

		const int BufferSize = 4096;

		// Note: An Object value begins with the 16-byte IID of the object's interface ([MS-OXTNEF] section 2.1.3.5.1).
		const int IidLength = 16;

		const string MeetingResponsePrefix = "IPM.Schedule.Meeting.Resp.";

		// PidTagAttachTag value for OLE attachments ([MS-OXCMSG] section 2.2.2.15): OID_TNEF.
		static readonly byte[] OleAttachTag = { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x14, 0x03, 0x0A, 0x03, 0x02, 0x01 };

		// PidTagAttachEncoding value for MacBinary attachments ([MS-OXCMSG] section 2.2.2.4).
		static readonly byte[] MacBinaryAttachEncoding = { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x14, 0x03, 0x0B, 0x01 };

		// The ProviderUID of a One-Off EntryID ([MS-OXCDATA] section 2.2.5.1).
		static readonly byte[] OneOffProviderUid = { 0x81, 0x2B, 0x1F, 0xA4, 0xBE, 0xA3, 0x10, 0x19, 0x9D, 0x6E, 0x00, 0xDD, 0x01, 0x0F, 0x54, 0x02 };

		readonly TnefPropertySetBuilder properties = new TnefPropertySetBuilder ();
		readonly List<TnefAttachment> attachments = new List<TnefAttachment> ();
		readonly List<TnefRecipient> recipients = new List<TnefRecipient> ();
		readonly TnefReader reader;

		AttachmentBuilder? attachment;
		bool attachmentLimitExceeded;

		TnefMessageBody? textBody, htmlBody, rtfBody;
		bool mapiTextBody, mapiHtmlBody, mapiRtfBody;

		string? ownerName, ownerAddressType, ownerAddress;
		bool hasOwner;

		public TnefMessageLoader (TnefReader reader)
		{
			this.reader = reader;
		}

		public TnefMessage Load (CancellationToken cancellationToken)
		{
			try {
				while (reader.Read (cancellationToken)) {
					if (TnefReader.IsAttachmentLevelAttribute (reader.Tag))
						ProcessAttachmentAttribute (cancellationToken);
					else
						ProcessMessageAttribute (cancellationToken);
				}

				return Complete ();
			} catch {
				DisposeAll ();
				throw;
			}
		}

		#region Shared logic

		void DisposeAll ()
		{
			attachment?.Content?.Dispose ();
			attachment = null;

			for (int i = 0; i < attachments.Count; i++)
				attachments[i].Dispose ();

			textBody?.Dispose ();
			htmlBody?.Dispose ();
			rtfBody?.Dispose ();
		}

		TnefMessage Complete ()
		{
			FinishAttachment ();

			if (reader.HasInvalidSignature)
				throw new TnefException (TnefComplianceViolation.InvalidSignature, "The stream does not begin with the TNEF signature.");

			ApplyOwner ();

			var set = properties.ToPropertySet ();

			// Note: The codepage may have changed after a body was read if the stream did not specify an
			// attOemCodepage and the PidTagInternetCodepage property followed the body. See [MS-OXTNEF] 2.3.3.2.
			textBody?.SetMessageEncoding (reader.Encoding);
			htmlBody?.SetMessageEncoding (reader.Encoding);
			rtfBody?.SetMessageEncoding (reader.Encoding);

			if (htmlBody != null && htmlBody.Tag.ValueTnefType == TnefPropertyType.Binary)
				htmlBody.Encoding = GetInternetEncoding (set);

			return new TnefMessage (reader.Codepage, reader.LegacyKey, set, recipients, attachments, textBody, htmlBody, rtfBody);
		}

		static Encoding? GetInternetEncoding (TnefPropertySet set)
		{
			var codepage = set.GetInt32 (TnefPropertyTag.InternetCodepage);

			if (!codepage.HasValue || codepage.Value <= 0)
				return null;

			try {
				return Encoding.GetEncoding (codepage.Value);
			} catch {
				return null;
			}
		}

		void AddLegacy (TnefPropertyTag tag, object value)
		{
			properties.AddLegacy (tag, value, reader.Encoding);
		}

		void AddLegacy (AttachmentBuilder builder, TnefPropertyTag tag, object value)
		{
			builder.Properties.AddLegacy (tag, value, reader.Encoding);
		}

		void LogInvalidAttributeValue ()
		{
			reader.Log (TnefComplianceViolation.InvalidAttributeValue, reader.AttributeOffset);
		}

		// Decodes an 8-bit string, ignoring any trailing null characters.
		static string DecodeString (Encoding encoding, byte[] bytes, int index, int count)
		{
			while (count > 0 && bytes[index + count - 1] == 0)
				count--;

			return count > 0 ? encoding.GetString (bytes, index, count) : string.Empty;
		}

		string DecodeString (byte[] bytes)
		{
			return DecodeString (reader.Encoding, bytes, 0, bytes.Length);
		}

		void AddLegacyString (TnefPropertyTag tag, byte[] value)
		{
			var text = DecodeString (value);

			if (text.Length > 0)
				AddLegacy (tag, text);
		}

		void AddLegacyDate (TnefPropertyTag tag, DateTime value)
		{
			if (value != default)
				AddLegacy (tag, value);
		}

		// Translates the legacy message classes that are listed in [MS-OXTNEF] section 2.1.3.4.1.
		internal static string TranslateMessageClass (string messageClass)
		{
			const string MicrosoftMailV3 = "Microsoft Mail v3.0 ";
			var value = messageClass;

			if (value.StartsWith (MicrosoftMailV3, StringComparison.Ordinal))
				value = value.Substring (MicrosoftMailV3.Length);

			switch (value) {
			case "IPM.Microsoft Mail.Note": return "IPM.Note";
			case "IPM.Microsoft Mail.Read Receipt": return "Report.IPM.Note.IPNRN";
			case "IPM.Microsoft Mail.Non-Delivery": return "Report.IPM.Note.NDR";
			case "IPM.Microsoft Schedule.MtgRespP": return "IPM.Schedule.Meeting.Resp.Pos";
			case "IPM.Microsoft Schedule.MtgRespN": return "IPM.Schedule.Meeting.Resp.Neg";
			case "IPM.Microsoft Schedule.MtgRespA": return "IPM.Schedule.Meeting.Resp.Tent";
			case "IPM.Microsoft Schedule.MtgReq": return "IPM.Schedule.Meeting.Request";
			case "IPM.Microsoft Schedule.MtgCncl": return "IPM.Schedule.Meeting.Canceled";
			default: return messageClass;
			}
		}

		void ProcessMessageClass (TnefPropertyTag tag, byte[] value)
		{
			var messageClass = DecodeString (value).Trim ();

			if (messageClass.Length > 0)
				AddLegacy (tag, TranslateMessageClass (messageClass));
		}

		static int HexValue (byte c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			return -1;
		}

		// The attMessageID attribute is the PidTagSearchKey encoded as a string of hexadecimal digits.
		void ProcessMessageId (byte[] value)
		{
			int length = value.Length;

			while (length > 0 && value[length - 1] == 0)
				length--;

			if (length == 0)
				return;

			if ((length & 1) != 0) {
				LogInvalidAttributeValue ();
				return;
			}

			var searchKey = new byte[length / 2];

			for (int i = 0; i < length; i += 2) {
				int high = HexValue (value[i]);
				int low = HexValue (value[i + 1]);

				if (high == -1 || low == -1) {
					LogInvalidAttributeValue ();
					return;
				}

				searchKey[i / 2] = (byte) ((high << 4) | low);
			}

			AddLegacy (TnefPropertyTag.SearchKey, searchKey);
		}

		// Maps attPriority to PidTagImportance ([MS-OXTNEF] section 2.1.3.4).
		void ProcessPriority (short value)
		{
			switch (value) {
			case 1: AddLegacy (TnefPropertyTag.Importance, 2); break;
			case 2: AddLegacy (TnefPropertyTag.Importance, 1); break;
			case 3: AddLegacy (TnefPropertyTag.Importance, 0); break;
			default: LogInvalidAttributeValue (); break;
			}
		}

		// Maps the attMessageStatus flags to PidTagMessageFlags ([MS-OXTNEF] section 2.1.3.4).
		void ProcessMessageStatus (byte[] value)
		{
			if (value.Length == 0) {
				LogInvalidAttributeValue ();
				return;
			}

			int status = value[0];
			int flags = 0;

			if ((status & 0x20) != 0) // fmsRead
				flags |= 0x01; // MSGFLAG_READ
			if ((status & 0x01) == 0) // fmsModified
				flags |= 0x02; // MSGFLAG_UNMODIFIED
			if ((status & 0x04) != 0) // fmsSubmitted
				flags |= 0x04; // MSGFLAG_SUBMIT
			if ((status & 0x02) != 0) // fmsLocal
				flags |= 0x08; // MSGFLAG_UNSENT
			if ((status & 0x80) != 0) // fmsHasAttach
				flags |= 0x10; // MSGFLAG_HASATTACH

			AddLegacy (TnefPropertyTag.MessageFlags, flags);
		}

		void ProcessRequestResponse (short value)
		{
			AddLegacy (TnefPropertyTag.ResponseRequested, (value & 0xFF) != 0);
		}

		void ProcessDelegate (byte[] value)
		{
			if (value.Length > 0)
				AddLegacy (TnefPropertyTag.RcvdRepresentingEntryId, value);
		}

		// Splits an address of the form "TYPE:address".
		static void SplitAddress (string value, out string? addressType, out string address)
		{
			int index = value.IndexOf (':');

			if (index > 0) {
				addressType = value.Substring (0, index);
				address = value.Substring (index + 1);
			} else {
				addressType = null;
				address = value;
			}
		}

		// Creates a One-Off EntryID ([MS-OXCDATA] section 2.2.5.1) with Unicode strings.
		internal static byte[] CreateOneOffEntryId (string displayName, string addressType, string address)
		{
			int length = 4 + OneOffProviderUid.Length + 4 + (displayName.Length + addressType.Length + address.Length + 3) * 2;
			var entryId = new byte[length];
			int index = 4;

			Buffer.BlockCopy (OneOffProviderUid, 0, entryId, index, OneOffProviderUid.Length);
			index += OneOffProviderUid.Length;

			// Note: The version is 0 and the flags are MAPI_ONE_OFF_UNICODE (0x8000), little-endian.
			entryId[index + 3] = 0x80;
			index += 4;

			index += Encoding.Unicode.GetBytes (displayName, 0, displayName.Length, entryId, index) + 2;
			index += Encoding.Unicode.GetBytes (addressType, 0, addressType.Length, entryId, index) + 2;
			Encoding.Unicode.GetBytes (address, 0, address.Length, entryId, index);

			return entryId;
		}

		// Decodes the attFrom attribute, which is a TRP structure followed by the display name and the address
		// ([MS-OXTNEF] section 2.1.3.3.1).
		void ProcessFrom (byte[] value)
		{
			if (value.Length < 8) {
				LogInvalidAttributeValue ();
				return;
			}

			int trpid = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (0, 2));
			int nameLength = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (4, 2));
			int addressLength = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (6, 2));

			// Note: The only TRP type that is used is trpidOneOff.
			if (trpid != 0x0004 || 8 + nameLength + addressLength > value.Length) {
				LogInvalidAttributeValue ();
				return;
			}

			var name = DecodeString (reader.Encoding, value, 8, nameLength);
			var fullAddress = DecodeString (reader.Encoding, value, 8 + nameLength, addressLength);

			SplitAddress (fullAddress, out var addressType, out var address);

			if (name.Length > 0)
				AddLegacy (TnefPropertyTag.SenderNameW, name);

			if (!string.IsNullOrEmpty (addressType))
				AddLegacy (TnefPropertyTag.SenderAddrtypeW, addressType!);

			if (address.Length > 0)
				AddLegacy (TnefPropertyTag.SenderEmailAddressW, address);

			AddLegacy (TnefPropertyTag.SenderEntryId, CreateOneOffEntryId (name, addressType ?? string.Empty, address));
		}

		// Decodes the attOwner attribute, which is a length-prefixed display name followed by a length-prefixed address
		// ([MS-OXTNEF] section 2.1.3.3.1). The properties that it maps to depend on the message class, so they are not
		// added until the entire message has been read.
		void ProcessOwner (byte[] value)
		{
			if (value.Length < 2) {
				LogInvalidAttributeValue ();
				return;
			}

			int nameLength = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (0, 2));
			int index = 2 + nameLength;

			if (index + 2 > value.Length) {
				LogInvalidAttributeValue ();
				return;
			}

			int addressLength = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (index, 2));
			index += 2;

			if (index + addressLength > value.Length) {
				LogInvalidAttributeValue ();
				return;
			}

			ownerName = DecodeString (reader.Encoding, value, 2, nameLength);
			SplitAddress (DecodeString (reader.Encoding, value, index, addressLength), out ownerAddressType, out var address);
			ownerAddress = address;
			hasOwner = true;
		}

		void ApplyOwner ()
		{
			if (!hasOwner)
				return;

			var messageClass = properties.GetString (TnefPropertyId.MessageClass)?.Trim ();
			TnefPropertyTag nameTag, addressTypeTag, addressTag, entryIdTag;

			// Note: For meeting responses, attOwner identifies the delegator rather than the sender.
			if (messageClass != null && messageClass.StartsWith (MeetingResponsePrefix, StringComparison.OrdinalIgnoreCase)) {
				nameTag = TnefPropertyTag.RcvdRepresentingNameW;
				addressTypeTag = TnefPropertyTag.RcvdRepresentingAddrtypeW;
				addressTag = TnefPropertyTag.RcvdRepresentingEmailAddressW;
				entryIdTag = TnefPropertyTag.RcvdRepresentingEntryId;
			} else {
				nameTag = TnefPropertyTag.SentRepresentingNameW;
				addressTypeTag = TnefPropertyTag.SentRepresentingAddrtypeW;
				addressTag = TnefPropertyTag.SentRepresentingEmailAddressW;
				entryIdTag = TnefPropertyTag.SentRepresentingEntryId;
			}

			var name = ownerName ?? string.Empty;
			var address = ownerAddress ?? string.Empty;

			if (name.Length > 0)
				AddLegacy (nameTag, name);

			if (!string.IsNullOrEmpty (ownerAddressType))
				AddLegacy (addressTypeTag, ownerAddressType!);

			if (address.Length > 0)
				AddLegacy (addressTag, address);

			AddLegacy (entryIdTag, CreateOneOffEntryId (name, ownerAddressType ?? string.Empty, address));
		}

		void AddRecipients (IReadOnlyList<TnefPropertySet> rows)
		{
			for (int i = 0; i < rows.Count; i++)
				recipients.Add (new TnefRecipient (rows[i]));
		}

		static bool TryGetBodyFormat (TnefPropertyTag tag, out TnefMessageBodyFormat format)
		{
			if (!tag.IsMultiValued) {
				var type = tag.ValueTnefType;

				switch (tag.Id) {
				case TnefPropertyId.Body:
					if (type == TnefPropertyType.String8 || type == TnefPropertyType.Unicode) {
						format = TnefMessageBodyFormat.Text;
						return true;
					}
					break;
				case TnefPropertyId.BodyHtml:
					if (type == TnefPropertyType.String8 || type == TnefPropertyType.Unicode || type == TnefPropertyType.Binary) {
						format = TnefMessageBodyFormat.Html;
						return true;
					}
					break;
				case TnefPropertyId.RtfCompressed:
					if (type == TnefPropertyType.Binary) {
						format = TnefMessageBodyFormat.CompressedRtf;
						return true;
					}
					break;
				}
			}

			format = default;

			return false;
		}

		// The first MAPI body of each format wins.
		bool HasMapiBody (TnefMessageBodyFormat format)
		{
			switch (format) {
			case TnefMessageBodyFormat.Text: return mapiTextBody;
			case TnefMessageBodyFormat.Html: return mapiHtmlBody;
			default: return mapiRtfBody;
			}
		}

		void SetMapiBody (TnefMessageBodyFormat format, TnefMessageBody? body)
		{
			switch (format) {
			case TnefMessageBodyFormat.Text:
				mapiTextBody = true;

				// Note: The MAPI body replaces the body from the legacy attBody attribute, unless it was too large to load.
				if (body != null) {
					textBody?.Dispose ();
					textBody = body;
				}
				break;
			case TnefMessageBodyFormat.Html:
				mapiHtmlBody = true;
				htmlBody = body;
				break;
			default:
				mapiRtfBody = true;
				rtfBody = body;
				break;
			}
		}

		static bool IsNul (byte[] buffer, int index, int unit)
		{
			for (int i = 0; i < unit; i++) {
				if (buffer[index + i] != 0)
					return false;
			}

			return true;
		}

		// Removes the trailing null characters from a text body.
		static void TrimTrailingNuls (MemoryBlockStream content, int unit)
		{
			long length = content.Length;

			if ((length % unit) != 0)
				return;

			var buffer = ArrayPool<byte>.Shared.Rent (BufferSize);
			long end = length;

			try {
				while (end > 0) {
					int n = (int) Math.Min (BufferSize, end);
					int nread = 0;

					content.Position = end - n;

					while (nread < n) {
						int count = content.Read (buffer, nread, n - nread);

						if (count == 0)
							break;

						nread += count;
					}

					int index = nread;

					while (index >= unit && IsNul (buffer, index - unit, unit))
						index -= unit;

					end -= nread - index;

					if (index > 0 || nread < n)
						break;
				}

				if (end < length)
					content.SetLength (end);
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
				content.Position = 0;
			}
		}

		TnefMessageBody CreateBody (TnefMessageBodyFormat format, TnefPropertyTag tag, MemoryBlockStream content)
		{
			Encoding? encoding;

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Unicode:
				TrimTrailingNuls (content, 2);
				encoding = Encoding.Unicode;
				break;
			case TnefPropertyType.String8:
				TrimTrailingNuls (content, 1);
				encoding = reader.Encoding;
				break;
			default:
				encoding = null;
				break;
			}

			return new TnefMessageBody (format, tag, content, encoding, reader.Encoding);
		}

		static bool IsAttachData (TnefPropertyTag tag)
		{
			if (tag.Id != TnefPropertyId.AttachData || tag.IsMultiValued)
				return false;

			return tag.ValueTnefType == TnefPropertyType.Binary || tag.ValueTnefType == TnefPropertyType.Object;
		}

		// Starts a new attachment, unless the maximum number of attachments has been reached.
		bool BeginAttachment ()
		{
			FinishAttachment ();

			if (attachmentLimitExceeded)
				return false;

			if (attachments.Count >= reader.Options.MaxAttachments) {
				reader.Log (TnefComplianceViolation.TooManyAttachments, reader.AttributeOffset);
				attachmentLimitExceeded = true;
				return false;
			}

			attachment = new AttachmentBuilder ();

			return true;
		}

		void FinishAttachment ()
		{
			if (attachment is null)
				return;

			var builder = attachment;
			attachment = null;

			var set = builder.Properties.ToPropertySet ();
			TnefEmbeddedMessageContext? embedded = null;

			if (builder.Content != null) {
				// Note: PidTagAttachMethod usually follows PidTagAttachDataObject, so a Binary value of an embedded
				// message can only be recognized once all of the attachment's properties have been read.
				if (!builder.Embedded && builder.ContentFromMapi && set.GetInt32 (TnefPropertyTag.AttachMethod) == (int) TnefAttachMethod.EmbeddedMessage) {
					builder.ContentOffset = IidLength;
					builder.Embedded = true;
				}

				if (builder.Embedded)
					embedded = new TnefEmbeddedMessageContext (reader, builder.ContentAbsoluteOffset + builder.ContentOffset);
			}

			attachments.Add (new TnefAttachment (set, builder.Content, builder.ContentOffset, embedded));
		}

		// Decodes the attAttachRendData attribute ([MS-OXTNEF] section 2.1.3.3.11).
		void ProcessRenderData (AttachmentBuilder builder, byte[] value)
		{
			if (value.Length < 14) {
				LogInvalidAttributeValue ();
				return;
			}

			int type = BinaryPrimitives.ReadUInt16LittleEndian (value.AsSpan (0, 2));
			int position = BinaryPrimitives.ReadInt32LittleEndian (value.AsSpan (2, 4));
			uint flags = BinaryPrimitives.ReadUInt32LittleEndian (value.AsSpan (10, 4));

			AddLegacy (builder, TnefPropertyTag.RenderingPosition, position);

			// Note: attachment type 2 is an OLE object.
			if (type == 2)
				AddLegacy (builder, TnefPropertyTag.AttachTag, OleAttachTag);

			// Note: flag 0x00000001 indicates that the attachment is MacBinary encoded.
			if ((flags & 0x00000001) != 0)
				AddLegacy (builder, TnefPropertyTag.AttachEncoding, MacBinaryAttachEncoding);
		}

		void ProcessAttachmentBytes (AttachmentBuilder builder, TnefPropertyTag tag, byte[] value)
		{
			if (value.Length > 0)
				AddLegacy (builder, tag, value);
		}

		void ProcessAttachmentString (AttachmentBuilder builder, TnefPropertyTag tag, byte[] value)
		{
			var text = DecodeString (value);

			if (text.Length > 0)
				AddLegacy (builder, tag, text);
		}

		void ProcessAttachmentDate (AttachmentBuilder builder, TnefPropertyTag tag, DateTime value)
		{
			if (value != default)
				AddLegacy (builder, tag, value);
		}

		#endregion

		// Reads the remainder of a value into memory, subject to the data limits.
		MemoryBlockStream? BufferValue (TnefReaderStream stream, TnefPropertyTag tag, CancellationToken cancellationToken)
		{
			long length = stream.Remaining;

			if (!reader.TryReserveValueBytes (length, tag))
				return null;

			var buffer = ArrayPool<byte>.Shared.Rent (BufferSize);
			var content = new MemoryBlockStream ();
			long copied = 0;

			try {
				int nread;

				while ((nread = stream.Read (buffer, 0, BufferSize, cancellationToken)) > 0) {
					content.Write (buffer, 0, nread);
					copied += nread;
				}
			} catch {
				content.Dispose ();
				throw;
			} finally {
				reader.ReleaseValueBytes (length - copied);
				ArrayPool<byte>.Shared.Return (buffer);
			}

			content.Position = 0;

			return content;
		}

		void ProcessMessageAttribute (CancellationToken cancellationToken)
		{
			switch (reader.Tag) {
			case TnefAttributeTag.MapiProperties:
				ReadMessageProperties (cancellationToken);
				break;
			case TnefAttributeTag.RecipientTable:
				AddRecipients (reader.GetPropertyReader ().ReadRowsAsPropertySets (cancellationToken));
				break;
			case TnefAttributeTag.Body:
				if (textBody is null) {
					using (var stream = (TnefReaderStream) reader.OpenValueStream ()) {
						var content = BufferValue (stream, TnefPropertyTag.Null, cancellationToken);

						if (content != null)
							textBody = CreateBody (TnefMessageBodyFormat.Text, TnefPropertyTag.BodyA, content);
					}
				}
				break;
			case TnefAttributeTag.Subject:
				AddLegacyString (TnefPropertyTag.SubjectW, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.MessageClass:
				ProcessMessageClass (TnefPropertyTag.MessageClassW, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.OriginalMessageClass:
				ProcessMessageClass (TnefPropertyTag.OrigMessageClassW, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.MessageId:
				ProcessMessageId (reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.DateSent:
				AddLegacyDate (TnefPropertyTag.ClientSubmitTime, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.DateReceived:
				AddLegacyDate (TnefPropertyTag.MessageDeliveryTime, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.DateModified:
				AddLegacyDate (TnefPropertyTag.LastModificationTime, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.DateStart:
				AddLegacyDate (TnefPropertyTag.StartDate, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.DateEnd:
				AddLegacyDate (TnefPropertyTag.EndDate, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.Priority:
				ProcessPriority (reader.ReadValueAsInt16 (cancellationToken));
				break;
			case TnefAttributeTag.MessageStatus:
				ProcessMessageStatus (reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.AidOwner:
				AddLegacy (TnefPropertyTag.OwnerApptId, reader.ReadValueAsInt32 (cancellationToken));
				break;
			case TnefAttributeTag.RequestResponse:
				ProcessRequestResponse (reader.ReadValueAsInt16 (cancellationToken));
				break;
			case TnefAttributeTag.Delegate:
				ProcessDelegate (reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.From:
				ProcessFrom (reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.Owner:
				ProcessOwner (reader.ReadValueAsBytes (cancellationToken));
				break;
			}
		}

		void ReadMessageProperties (CancellationToken cancellationToken)
		{
			var pr = reader.GetPropertyReader ();

			while (pr.ReadNextProperty (cancellationToken)) {
				var tag = pr.Tag;

				if (pr.HasValue && TryGetBodyFormat (tag, out var format)) {
					if (!HasMapiBody (format))
						SetMapiBody (format, ReadBody (pr, format, cancellationToken));
					continue;
				}

				properties.Add (pr.ReadProperty (cancellationToken));
			}
		}

		TnefMessageBody? ReadBody (TnefPropertyReader pr, TnefMessageBodyFormat format, CancellationToken cancellationToken)
		{
			var tag = pr.Tag;

			using (var stream = (TnefReaderStream) pr.OpenValueStream ()) {
				var content = BufferValue (stream, tag, cancellationToken);

				return content != null ? CreateBody (format, tag, content) : null;
			}
		}

		void ProcessAttachmentAttribute (CancellationToken cancellationToken)
		{
			if (reader.Tag == TnefAttributeTag.AttachRenderData) {
				if (BeginAttachment ())
					ProcessRenderData (attachment!, reader.ReadValueAsBytes (cancellationToken));
				return;
			}

			if (attachment is null && !BeginAttachment ())
				return;

			var builder = attachment!;

			switch (reader.Tag) {
			case TnefAttributeTag.Attachment:
				ReadAttachmentProperties (builder, cancellationToken);
				break;
			case TnefAttributeTag.AttachData:
				if (builder.Content is null) {
					long offset = reader.AbsoluteOffset;

					using (var stream = (TnefReaderStream) reader.OpenValueStream ()) {
						var content = BufferValue (stream, TnefPropertyTag.Null, cancellationToken);

						if (content != null)
							builder.SetContent (content, offset, 0, false, false);
					}
				}
				break;
			case TnefAttributeTag.AttachTitle:
				ProcessAttachmentString (builder, TnefPropertyTag.AttachLongFilenameW, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.AttachTransportFilename:
				ProcessAttachmentString (builder, TnefPropertyTag.AttachTransportNameW, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.AttachMetaFile:
				ProcessAttachmentBytes (builder, TnefPropertyTag.AttachRendering, reader.ReadValueAsBytes (cancellationToken));
				break;
			case TnefAttributeTag.AttachCreateDate:
				ProcessAttachmentDate (builder, TnefPropertyTag.CreationTime, reader.ReadValueAsDateTime (cancellationToken));
				break;
			case TnefAttributeTag.AttachModifyDate:
				ProcessAttachmentDate (builder, TnefPropertyTag.LastModificationTime, reader.ReadValueAsDateTime (cancellationToken));
				break;
			}
		}

		void ReadAttachmentProperties (AttachmentBuilder builder, CancellationToken cancellationToken)
		{
			var pr = reader.GetPropertyReader ();

			while (pr.ReadNextProperty (cancellationToken)) {
				var tag = pr.Tag;

				if (pr.HasValue && IsAttachData (tag)) {
					if (!builder.ContentFromMapi)
						ReadAttachData (builder, pr, cancellationToken);
					continue;
				}

				builder.Properties.Add (pr.ReadProperty (cancellationToken));
			}
		}

		void ReadAttachData (AttachmentBuilder builder, TnefPropertyReader pr, CancellationToken cancellationToken)
		{
			var tag = pr.Tag;
			bool embedded = pr.IsEmbeddedMessage;
			long offset = reader.AbsoluteOffset;

			using (var stream = (TnefReaderStream) pr.OpenValueStream ()) {
				var content = BufferValue (stream, tag, cancellationToken);

				// Note: If the MAPI value was too large to load, the content from the legacy attAttachData attribute is kept.
				if (content != null)
					builder.SetContent (content, offset, GetContentOffset (tag, embedded), true, embedded);
			}
		}

		static long GetContentOffset (TnefPropertyTag tag, bool embedded)
		{
			return embedded || tag.ValueTnefType == TnefPropertyType.Object ? IidLength : 0;
		}
	}
}
