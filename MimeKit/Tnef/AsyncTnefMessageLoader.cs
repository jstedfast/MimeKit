//
// AsyncTnefMessageLoader.cs
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

using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using MimeKit.IO;

namespace MimeKit.Tnef {
	sealed partial class TnefMessageLoader
	{
		public async Task<TnefMessage> LoadAsync (CancellationToken cancellationToken)
		{
			try {
				while (await reader.ReadAsync (cancellationToken).ConfigureAwait (false)) {
					if (TnefReader.IsAttachmentLevelAttribute (reader.Tag))
						await ProcessAttachmentAttributeAsync (cancellationToken).ConfigureAwait (false);
					else
						await ProcessMessageAttributeAsync (cancellationToken).ConfigureAwait (false);
				}

				return Complete ();
			} catch {
				DisposeAll ();
				throw;
			}
		}

		async Task<MemoryBlockStream?> BufferValueAsync (TnefReaderStream stream, TnefPropertyTag tag, CancellationToken cancellationToken)
		{
			long length = stream.Remaining;

			if (!reader.TryReserveValueBytes (length, tag))
				return null;

			var buffer = ArrayPool<byte>.Shared.Rent (BufferSize);
			var content = new MemoryBlockStream ();
			long copied = 0;

			try {
				int nread;

				while ((nread = await stream.ReadAsync (buffer, 0, BufferSize, cancellationToken).ConfigureAwait (false)) > 0) {
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

		async Task ProcessMessageAttributeAsync (CancellationToken cancellationToken)
		{
			switch (reader.Tag) {
			case TnefAttributeTag.MapiProperties:
				await ReadMessagePropertiesAsync (cancellationToken).ConfigureAwait (false);
				break;
			case TnefAttributeTag.RecipientTable:
				AddRecipients (await reader.GetPropertyReader ().ReadRowsAsPropertySetsAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.Body:
				if (textBody is null) {
					using (var stream = (TnefReaderStream) reader.OpenValueStream ()) {
						var content = await BufferValueAsync (stream, TnefPropertyTag.Null, cancellationToken).ConfigureAwait (false);

						if (content != null)
							textBody = CreateBody (TnefMessageBodyFormat.Text, TnefPropertyTag.BodyA, content);
					}
				}
				break;
			case TnefAttributeTag.Subject:
				AddLegacyString (TnefPropertyTag.SubjectW, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.MessageClass:
				ProcessMessageClass (TnefPropertyTag.MessageClassW, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.OriginalMessageClass:
				ProcessMessageClass (TnefPropertyTag.OrigMessageClassW, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.MessageId:
				ProcessMessageId (await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.DateSent:
				AddLegacyDate (TnefPropertyTag.ClientSubmitTime, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.DateReceived:
				AddLegacyDate (TnefPropertyTag.MessageDeliveryTime, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.DateModified:
				AddLegacyDate (TnefPropertyTag.LastModificationTime, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.DateStart:
				AddLegacyDate (TnefPropertyTag.StartDate, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.DateEnd:
				AddLegacyDate (TnefPropertyTag.EndDate, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.Priority:
				ProcessPriority (await reader.ReadValueAsInt16Async (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.MessageStatus:
				ProcessMessageStatus (await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.AidOwner:
				AddLegacy (TnefPropertyTag.OwnerApptId, await reader.ReadValueAsInt32Async (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.RequestResponse:
				ProcessRequestResponse (await reader.ReadValueAsInt16Async (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.Delegate:
				ProcessDelegate (await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.From:
				ProcessFrom (await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.Owner:
				ProcessOwner (await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			}
		}

		async Task ReadMessagePropertiesAsync (CancellationToken cancellationToken)
		{
			var pr = reader.GetPropertyReader ();

			while (await pr.ReadNextPropertyAsync (cancellationToken).ConfigureAwait (false)) {
				var tag = pr.Tag;

				if (pr.HasValue && TryGetBodyFormat (tag, out var format)) {
					if (!HasMapiBody (format))
						SetMapiBody (format, await ReadBodyAsync (pr, format, cancellationToken).ConfigureAwait (false));
					continue;
				}

				properties.Add (await pr.ReadPropertyAsync (cancellationToken).ConfigureAwait (false));
			}
		}

		async Task<TnefMessageBody?> ReadBodyAsync (TnefPropertyReader pr, TnefMessageBodyFormat format, CancellationToken cancellationToken)
		{
			var tag = pr.Tag;

			using (var stream = (TnefReaderStream) pr.OpenValueStream ()) {
				var content = await BufferValueAsync (stream, tag, cancellationToken).ConfigureAwait (false);

				return content != null ? CreateBody (format, tag, content) : null;
			}
		}

		async Task ProcessAttachmentAttributeAsync (CancellationToken cancellationToken)
		{
			if (reader.Tag == TnefAttributeTag.AttachRenderData) {
				if (BeginAttachment ())
					ProcessRenderData (attachment!, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				return;
			}

			if (attachment is null && !BeginAttachment ())
				return;

			var builder = attachment!;

			switch (reader.Tag) {
			case TnefAttributeTag.Attachment:
				await ReadAttachmentPropertiesAsync (builder, cancellationToken).ConfigureAwait (false);
				break;
			case TnefAttributeTag.AttachData:
				if (builder.Content is null) {
					long offset = reader.AbsoluteOffset;

					using (var stream = (TnefReaderStream) reader.OpenValueStream ()) {
						var content = await BufferValueAsync (stream, TnefPropertyTag.Null, cancellationToken).ConfigureAwait (false);

						if (content != null)
							builder.SetContent (content, offset, 0, false, false);
					}
				}
				break;
			case TnefAttributeTag.AttachTitle:
				ProcessAttachmentString (builder, TnefPropertyTag.AttachLongFilenameW, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.AttachTransportFilename:
				ProcessAttachmentString (builder, TnefPropertyTag.AttachTransportNameW, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.AttachMetaFile:
				ProcessAttachmentBytes (builder, TnefPropertyTag.AttachRendering, await reader.ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.AttachCreateDate:
				ProcessAttachmentDate (builder, TnefPropertyTag.CreationTime, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			case TnefAttributeTag.AttachModifyDate:
				ProcessAttachmentDate (builder, TnefPropertyTag.LastModificationTime, await reader.ReadValueAsDateTimeAsync (cancellationToken).ConfigureAwait (false));
				break;
			}
		}

		async Task ReadAttachmentPropertiesAsync (AttachmentBuilder builder, CancellationToken cancellationToken)
		{
			var pr = reader.GetPropertyReader ();

			while (await pr.ReadNextPropertyAsync (cancellationToken).ConfigureAwait (false)) {
				var tag = pr.Tag;

				if (pr.HasValue && IsAttachData (tag)) {
					if (!builder.ContentFromMapi)
						await ReadAttachDataAsync (builder, pr, cancellationToken).ConfigureAwait (false);
					continue;
				}

				builder.Properties.Add (await pr.ReadPropertyAsync (cancellationToken).ConfigureAwait (false));
			}
		}

		async Task ReadAttachDataAsync (AttachmentBuilder builder, TnefPropertyReader pr, CancellationToken cancellationToken)
		{
			var tag = pr.Tag;
			bool embedded = pr.IsEmbeddedMessage;
			long offset = reader.AbsoluteOffset;

			using (var stream = (TnefReaderStream) pr.OpenValueStream ()) {
				var content = await BufferValueAsync (stream, tag, cancellationToken).ConfigureAwait (false);

				// Note: If the MAPI value was too large to load, the content from the legacy attAttachData attribute is kept.
				if (content != null)
					builder.SetContent (content, offset, GetContentOffset (tag, embedded), true, embedded);
			}
		}
	}
}
