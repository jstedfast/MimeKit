//
// TnefWriter.cs
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
using System.IO;
using System.Text;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A forward-only writer for TNEF streams.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefWriter"/> produces a TNEF (Transport Neutral Encapsulation Format) stream as
	/// defined by [MS-OXTNEF].</para>
	/// <para>The stream header, the <see cref="TnefAttributeTag.TnefVersion"/> attribute and the
	/// <see cref="TnefAttributeTag.OemCodepage"/> attribute are written automatically. The level of each
	/// attribute is determined by its tag, and the length and checksum of each attribute are calculated
	/// automatically.</para>
	/// <para>The writer is strict: it throws rather than produce a stream that does not conform to the
	/// grammar defined by [MS-OXTNEF] section 2.1.3.1. All message-level attributes must be written before
	/// any attachments and <see cref="TnefAttributeTag.MapiProperties"/>, if written, must be the last
	/// message-level attribute. Each attachment must begin with a <see cref="TnefAttributeTag.AttachRenderData"/>
	/// attribute and <see cref="TnefAttributeTag.Attachment"/>, if written, must be the last attribute of
	/// the attachment.</para>
	/// <para>Attributes that are written using <see cref="OpenAttributeStream"/> or
	/// <see cref="OpenPropertyWriter"/> are buffered in memory until the stream or property writer is
	/// disposed and are then written to the output stream by the next call to one of the <c>WriteAttribute</c>
	/// methods, <see cref="Flush"/>, or <see cref="Dispose"/>.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="TnefWriter"/>
	/// </example>
	public sealed partial class TnefWriter : IDisposable
	{
		const int AttributeHeaderSize = 9;
		const int MaxMessageClassLength = 255;
		const int MaxNamedPropertyId = 0xFFFE;
		const int MinNamedPropertyId = 0x8000;

		enum WriterState : byte
		{
			// Message-level attributes may be written.
			Message,

			// The attMsgProps attribute has been written, so only attachments may follow.
			MessageProperties,

			// An attAttachRenderData attribute has been written, so attachment-level attributes may follow.
			Attachment,

			// An attAttachment attribute has been written, so only a new attachment may follow.
			AttachmentProperties
		}

		readonly struct BufferedAttribute
		{
			public readonly TnefAttributeLevel Level;
			public readonly TnefAttributeTag Tag;
			public readonly MemoryBlockStream Value;

			public BufferedAttribute (TnefAttributeLevel level, TnefAttributeTag tag, MemoryBlockStream value)
			{
				Level = level;
				Tag = tag;
				Value = value;
			}
		}

		readonly List<BufferedAttribute> queue = new List<BufferedAttribute> ();
		readonly Dictionary<TnefNameId, int> namedIds = new Dictionary<TnefNameId, int> ();
		readonly byte[] header = new byte[AttributeHeaderSize];
		readonly Encoding encoding;
		readonly bool leaveOpen;
		Stream stream;

		// The attribute that is currently being written by a stream or property writer.
		TnefAttributeLevel childLevel;
		TnefAttributeTag childTag;
		MemoryBlockStream? childValue;
		object? child;

		// Set when the embedded message that this writer writes is complete.
		Action<TnefWriter, string?>? closed;
		int nextNamedId = MinNamedPropertyId;
		bool headerWritten;
		WriterState state;
		string? fault;
		bool disposed;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefWriter"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="TnefWriter"/> that writes a TNEF stream to the specified stream.</para>
		/// <para>The <paramref name="codepage"/> is written as the <see cref="TnefAttributeTag.OemCodepage"/> attribute
		/// and is used to encode the 8-bit strings of attributes and of <see cref="TnefPropertyType.String8"/>
		/// properties. [MS-OXTNEF] requires a codepage in which no character contains a zero byte, so the UTF-16
		/// and UTF-32 codepages are not allowed.</para>
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="codepage">The codepage used to encode 8-bit strings.</param>
		/// <param name="legacyKey">The legacy key to write in the stream header.</param>
		/// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="stream"/> open after the
		/// <see cref="TnefWriter"/> is disposed; otherwise, <see langword="false"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="stream"/> is not writable.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="codepage"/> is not a supported codepage, or is a codepage that encodes characters
		/// using zero bytes.
		/// </exception>
		public TnefWriter (Stream stream, int codepage = 1252, ushort legacyKey = 0, bool leaveOpen = false)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (!stream.CanWrite)
				throw new ArgumentException ("The stream is not writable.", nameof (stream));

			encoding = GetEncoding (codepage);
			Codepage = encoding.CodePage;
			LegacyKey = legacyKey;
			this.leaveOpen = leaveOpen;
			this.stream = stream;
		}

		internal TnefWriter (Stream stream, int codepage, Action<TnefWriter, string?> closed) : this (stream, codepage, 0, true)
		{
			this.closed = closed;
		}

		/// <summary>
		/// Get the codepage used to encode 8-bit strings.
		/// </summary>
		/// <remarks>
		/// Gets the codepage used to encode 8-bit strings. This is the value of the
		/// <see cref="TnefAttributeTag.OemCodepage"/> attribute.
		/// </remarks>
		/// <value>The codepage.</value>
		public int Codepage {
			get; private set;
		}

		/// <summary>
		/// Get the legacy key.
		/// </summary>
		/// <remarks>
		/// Gets the legacy key that is written in the stream header.
		/// </remarks>
		/// <value>The legacy key.</value>
		public ushort LegacyKey {
			get; private set;
		}

		internal Encoding Encoding {
			get { return encoding; }
		}

		static Encoding GetEncoding (int codepage)
		{
			Encoding encoding;

			// Note: Codepage 0 (CP_ACP) would select a platform-dependent encoding.
			if (codepage <= 0)
				throw new ArgumentOutOfRangeException (nameof (codepage), "The codepage is not supported.");

			switch (codepage) {
			case 1200: case 1201: case 12000: case 12001:
				throw new ArgumentOutOfRangeException (nameof (codepage), "The codepage must not encode characters using zero bytes.");
			}

			try {
				encoding = CharsetUtils.GetEncoding (codepage);
			} catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) {
				throw new ArgumentOutOfRangeException (nameof (codepage), "The codepage is not supported.");
			}

			return encoding;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefWriter));
		}

		void CheckCanWrite ()
		{
			CheckDisposed ();

			if (fault != null)
				throw new InvalidOperationException (fault);

			if (child != null)
				throw new InvalidOperationException ("The stream or property writer for the previous attribute must be disposed first.");
		}

		// Gets the level of the specified attribute.
		static TnefAttributeLevel GetAttributeLevel (TnefAttributeTag tag, string paramName)
		{
			switch (tag) {
			case TnefAttributeTag.AidOwner:
			case TnefAttributeTag.Body:
			case TnefAttributeTag.ConversationId:
			case TnefAttributeTag.DateEnd:
			case TnefAttributeTag.DateModified:
			case TnefAttributeTag.DateReceived:
			case TnefAttributeTag.DateSent:
			case TnefAttributeTag.DateStart:
			case TnefAttributeTag.Delegate:
			case TnefAttributeTag.From:
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.MessageClass:
			case TnefAttributeTag.MessageId:
			case TnefAttributeTag.MessageStatus:
			case TnefAttributeTag.OriginalMessageClass:
			case TnefAttributeTag.Owner:
			case TnefAttributeTag.ParentId:
			case TnefAttributeTag.Priority:
			case TnefAttributeTag.RecipientTable:
			case TnefAttributeTag.RequestResponse:
			case TnefAttributeTag.SentFor:
			case TnefAttributeTag.Subject:
				return TnefAttributeLevel.Message;
			case TnefAttributeTag.AttachCreateDate:
			case TnefAttributeTag.AttachData:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.AttachMetaFile:
			case TnefAttributeTag.AttachModifyDate:
			case TnefAttributeTag.AttachRenderData:
			case TnefAttributeTag.AttachTitle:
			case TnefAttributeTag.AttachTransportFilename:
				return TnefAttributeLevel.Attachment;
			case TnefAttributeTag.OemCodepage:
			case TnefAttributeTag.TnefVersion:
				throw new ArgumentException (string.Format ("The {0} attribute is written automatically.", tag), paramName);
			default:
				throw new ArgumentException (string.Format ("The {0} attribute cannot be written.", tag), paramName);
			}
		}

		// Validates that the attribute may be written next and advances the state.
		TnefAttributeLevel BeginAttribute (TnefAttributeTag tag, string paramName)
		{
			var level = GetAttributeLevel (tag, paramName);

			switch (tag) {
			case TnefAttributeTag.MapiProperties:
				if (state != WriterState.Message)
					throw new InvalidOperationException ("The MapiProperties attribute must be the last message-level attribute and may only be written once.");
				state = WriterState.MessageProperties;
				break;
			case TnefAttributeTag.AttachRenderData:
				// Note: attAttachRenderData begins a new attachment.
				state = WriterState.Attachment;
				break;
			case TnefAttributeTag.Attachment:
				if (state != WriterState.Attachment)
					throw new InvalidOperationException ("The Attachment attribute must follow an AttachRenderData attribute and may only be written once per attachment.");
				state = WriterState.AttachmentProperties;
				break;
			default:
				if (level == TnefAttributeLevel.Message) {
					if (state != WriterState.Message)
						throw new InvalidOperationException (string.Format ("The {0} attribute is a message-level attribute and must be written before the MapiProperties attribute and before any attachments.", tag));
				} else if (state != WriterState.Attachment) {
					throw new InvalidOperationException (string.Format ("The {0} attribute is an attachment-level attribute and must follow an AttachRenderData attribute and precede the Attachment attribute.", tag));
				}
				break;
			}

			return level;
		}

		static TnefAttributeType GetAttributeType (TnefAttributeTag tag)
		{
			return (TnefAttributeType) ((int) tag & unchecked((int) 0xFFFF0000));
		}

		// Encodes a 32-bit or 16-bit integer attribute value.
		static byte[] EncodeInt32 (TnefAttributeTag tag, int value)
		{
			byte[] buffer;

			switch (GetAttributeType (tag)) {
			case TnefAttributeType.Short:
				if (value < short.MinValue || value > short.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (value), string.Format ("The value of the {0} attribute must be a 16-bit signed integer.", tag));

				buffer = new byte[2];
				BinaryPrimitives.WriteInt16LittleEndian (buffer, (short) value);
				return buffer;
			case TnefAttributeType.Word:
				if (tag == TnefAttributeTag.MessageClass || tag == TnefAttributeTag.OriginalMessageClass)
					break;

				if (value < 0 || value > ushort.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (value), string.Format ("The value of the {0} attribute must be a 16-bit unsigned integer.", tag));

				buffer = new byte[2];
				BinaryPrimitives.WriteUInt16LittleEndian (buffer, (ushort) value);
				return buffer;
			case TnefAttributeType.Long:
			case TnefAttributeType.DWord:
				buffer = new byte[4];
				BinaryPrimitives.WriteInt32LittleEndian (buffer, value);
				return buffer;
			}

			throw new ArgumentException (string.Format ("The value of the {0} attribute is not an integer.", tag), nameof (tag));
		}

		// Encodes a nul-terminated 8-bit string attribute value.
		byte[] EncodeString (TnefAttributeTag tag, string value)
		{
			bool isMessageClass = tag == TnefAttributeTag.MessageClass || tag == TnefAttributeTag.OriginalMessageClass;

			switch (GetAttributeType (tag)) {
			case TnefAttributeType.String:
			case TnefAttributeType.Text:
				break;
			default:
				if (!isMessageClass)
					throw new ArgumentException (string.Format ("The value of the {0} attribute is not a string.", tag), nameof (tag));
				break;
			}

			if (isMessageClass) {
				// Note: [MS-OXOMSG] restricts the message class to printable ASCII characters.
				if (value.Length == 0 || value.Length + 1 > MaxMessageClassLength)
					throw new ArgumentException ("The message class must contain between 1 and 254 characters.", nameof (value));

				for (int i = 0; i < value.Length; i++) {
					if (value[i] < 0x20 || value[i] > 0x7e)
						throw new ArgumentException ("The message class must only contain printable ASCII characters.", nameof (value));
				}
			}

			int count = encoding.GetByteCount (value);
			var buffer = new byte[count + 1];

			encoding.GetBytes (value, 0, value.Length, buffer, 0);

			return buffer;
		}

		// Encodes a 14-byte date attribute value.
		static byte[] EncodeDate (TnefAttributeTag tag, DateTime value)
		{
			if (GetAttributeType (tag) != TnefAttributeType.Date)
				throw new ArgumentException (string.Format ("The value of the {0} attribute is not a date.", tag), nameof (tag));

			var buffer = new byte[14];
			var span = buffer.AsSpan ();

			// Note: The TNEF date structure is 7 16-bit values: year, month, day, hour, minute, second and day-of-week.
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (0, 2), (short) value.Year);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (2, 2), (short) value.Month);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (4, 2), (short) value.Day);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (6, 2), (short) value.Hour);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (8, 2), (short) value.Minute);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (10, 2), (short) value.Second);
			BinaryPrimitives.WriteInt16LittleEndian (span.Slice (12, 2), (short) value.DayOfWeek);

			return buffer;
		}

		static ushort GetChecksum (byte[] buffer, int startIndex, int length)
		{
			return TnefChecksum.Update (0, buffer, startIndex, length);
		}

		byte[] GetStreamHeader ()
		{
			// Note: [MS-OXTNEF] 2.1.3.1: TNEFStream = TNEFHeader TNEFVersion OEMCodePage MessageData *AttachData
			var buffer = new byte[4 + 2 + (AttributeHeaderSize + 4 + 2) + (AttributeHeaderSize + 8 + 2)];
			var span = buffer.AsSpan ();
			int index = 0;

			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (index, 4), TnefReader.TnefSignature);
			index += 4;
			BinaryPrimitives.WriteUInt16LittleEndian (span.Slice (index, 2), LegacyKey);
			index += 2;

			EncodeAttributeHeader (span.Slice (index, AttributeHeaderSize), TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, 4);
			index += AttributeHeaderSize;
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (index, 4), TnefReader.TnefVersion);
			BinaryPrimitives.WriteUInt16LittleEndian (span.Slice (index + 4, 2), GetChecksum (buffer, index, 4));
			index += 6;

			// Note: The secondary codepage is unused and SHOULD be 0.
			EncodeAttributeHeader (span.Slice (index, AttributeHeaderSize), TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage, 8);
			index += AttributeHeaderSize;
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (index, 4), Codepage);
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (index + 4, 4), 0);
			BinaryPrimitives.WriteUInt16LittleEndian (span.Slice (index + 8, 2), GetChecksum (buffer, index, 8));

			return buffer;
		}

		static void EncodeAttributeHeader (Span<byte> span, TnefAttributeLevel level, TnefAttributeTag tag, int length)
		{
			span[0] = (byte) level;
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (1, 4), (int) tag);
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (5, 4), length);
		}

		static byte[] EncodeChecksum (ushort checksum)
		{
			var buffer = new byte[2];

			BinaryPrimitives.WriteUInt16LittleEndian (buffer, checksum);

			return buffer;
		}

		void WriteHeader ()
		{
			if (headerWritten)
				return;

			var buffer = GetStreamHeader ();

			stream.Write (buffer, 0, buffer.Length);
			headerWritten = true;
		}

		void WriteAttributeValue (TnefAttributeLevel level, TnefAttributeTag tag, byte[] buffer, int startIndex, int length)
		{
			EncodeAttributeHeader (header, level, tag, length);
			stream.Write (header, 0, AttributeHeaderSize);
			stream.Write (buffer, startIndex, length);

			var checksum = EncodeChecksum (GetChecksum (buffer, startIndex, length));
			stream.Write (checksum, 0, checksum.Length);
		}

		void WriteBufferedAttribute (BufferedAttribute attribute)
		{
			var value = attribute.Value;
			var buffer = ArrayPool<byte>.Shared.Rent (4096);

			try {
				ushort checksum = 0;
				int nread;

				EncodeAttributeHeader (header, attribute.Level, attribute.Tag, (int) value.Length);
				stream.Write (header, 0, AttributeHeaderSize);

				value.Position = 0;

				while ((nread = value.Read (buffer, 0, buffer.Length)) > 0) {
					checksum = TnefChecksum.Update (checksum, buffer, 0, nread);
					stream.Write (buffer, 0, nread);
				}

				var encoded = EncodeChecksum (checksum);
				stream.Write (encoded, 0, encoded.Length);
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		// Writes the stream header and any buffered attributes to the output stream.
		void WriteQueued ()
		{
			WriteHeader ();

			while (queue.Count > 0) {
				var attribute = queue[0];

				queue.RemoveAt (0);

				try {
					WriteBufferedAttribute (attribute);
				} finally {
					attribute.Value.Dispose ();
				}
			}
		}

		void WriteAttributeCore (TnefAttributeTag tag, byte[] buffer, int startIndex, int length, string paramName)
		{
			CheckCanWrite ();

			var level = BeginAttribute (tag, paramName);

			WriteQueued ();
			WriteAttributeValue (level, tag, buffer, startIndex, length);
		}

		/// <summary>
		/// Write an attribute with an integer value.
		/// </summary>
		/// <remarks>
		/// <para>Writes an attribute with an integer value.</para>
		/// <para>The value of an attribute with a <see cref="TnefAttributeType.Short"/> or
		/// <see cref="TnefAttributeType.Word"/> type is written as a 16-bit integer and the value of an attribute
		/// with a <see cref="TnefAttributeType.Long"/> or <see cref="TnefAttributeType.DWord"/> type is written as
		/// a 32-bit integer.</para>
		/// </remarks>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not an integer.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> does not fit in a 16-bit attribute value.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteAttribute (TnefAttributeTag tag, int value)
		{
			var buffer = EncodeInt32 (tag, value);

			WriteAttributeCore (tag, buffer, 0, buffer.Length, nameof (tag));
		}

		/// <summary>
		/// Write an attribute with a string value.
		/// </summary>
		/// <remarks>
		/// <para>Writes an attribute with a string value, encoded using the <see cref="Codepage"/> and terminated by
		/// a nul character.</para>
		/// <para>The value of an attribute with a <see cref="TnefAttributeType.String"/> or
		/// <see cref="TnefAttributeType.Text"/> type and the value of the <see cref="TnefAttributeTag.MessageClass"/>
		/// and <see cref="TnefAttributeTag.OriginalMessageClass"/> attributes may be written as a string. A message
		/// class must consist of between 1 and 254 printable ASCII characters.</para>
		/// </remarks>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not a string.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is not a valid message class.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteAttribute (TnefAttributeTag tag, string value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			var buffer = EncodeString (tag, value);

			WriteAttributeCore (tag, buffer, 0, buffer.Length, nameof (tag));
		}

		/// <summary>
		/// Write an attribute with a date value.
		/// </summary>
		/// <remarks>
		/// <para>Writes an attribute with a <see cref="TnefAttributeType.Date"/> type.</para>
		/// <para>The date and time components of <paramref name="value"/> are written as-is, without converting
		/// them to another time zone.</para>
		/// </remarks>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not a date.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteAttribute (TnefAttributeTag tag, DateTime value)
		{
			var buffer = EncodeDate (tag, value);

			WriteAttributeCore (tag, buffer, 0, buffer.Length, nameof (tag));
		}

		/// <summary>
		/// Write an attribute with a raw value.
		/// </summary>
		/// <remarks>
		/// Writes an attribute with the specified raw value.
		/// </remarks>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="buffer">The buffer containing the value.</param>
		/// <param name="startIndex">The index of the first byte of the value.</param>
		/// <param name="length">The length of the value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that can be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteAttribute (TnefAttributeTag tag, byte[] buffer, int startIndex, int length)
		{
			ArgumentValidator.Validate (buffer, startIndex, length);

			WriteAttributeCore (tag, buffer, startIndex, length, nameof (tag));
		}

		/// <summary>
		/// Write an attribute with a raw value.
		/// </summary>
		/// <remarks>
		/// Writes an attribute with the specified raw value.
		/// </remarks>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="buffer">The value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that can be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteAttribute (TnefAttributeTag tag, byte[] buffer)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			WriteAttributeCore (tag, buffer, 0, buffer.Length, nameof (tag));
		}

		MemoryBlockStream BeginChild (TnefAttributeTag tag, string paramName)
		{
			CheckCanWrite ();

			childLevel = BeginAttribute (tag, paramName);
			childValue = new MemoryBlockStream ();
			childTag = tag;

			return childValue;
		}

		void SetChild (object value)
		{
			child = value;
		}

		/// <summary>
		/// Open a stream for writing the raw value of an attribute.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for writing the raw value of an attribute, such as the content of an
		/// <see cref="TnefAttributeTag.AttachData"/> attribute.</para>
		/// <para>The value is buffered in memory until the stream is disposed. No other attribute may be written
		/// until the stream has been disposed.</para>
		/// </remarks>
		/// <returns>The value stream.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that can be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		public Stream OpenAttributeStream (TnefAttributeTag tag)
		{
			var value = BeginChild (tag, nameof (tag));
			var stream = new TnefValueWriteStream (value, s => OnChildClosed (s, null));

			SetChild (stream);

			return stream;
		}

		/// <summary>
		/// Open a property writer for an attribute that contains MAPI properties.
		/// </summary>
		/// <remarks>
		/// <para>Opens a <see cref="TnefPropertyWriter"/> for writing the properties of a
		/// <see cref="TnefAttributeTag.MapiProperties"/>, <see cref="TnefAttributeTag.RecipientTable"/> or
		/// <see cref="TnefAttributeTag.Attachment"/> attribute.</para>
		/// <para>The properties are buffered in memory until the property writer is disposed. No other attribute
		/// may be written until the property writer has been disposed.</para>
		/// </remarks>
		/// <returns>The property writer.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that contains MAPI properties.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		public TnefPropertyWriter OpenPropertyWriter (TnefAttributeTag tag)
		{
			switch (tag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.RecipientTable:
			case TnefAttributeTag.Attachment:
				break;
			default:
				throw new ArgumentException (string.Format ("The {0} attribute does not contain MAPI properties.", tag), nameof (tag));
			}

			var value = BeginChild (tag, nameof (tag));
			var writer = new TnefPropertyWriter (this, value, tag == TnefAttributeTag.RecipientTable);

			SetChild (writer);

			return writer;
		}

		internal void OnChildClosed (object closedChild, string? error)
		{
			if (child != closedChild || childValue is null)
				return;

			if (error != null) {
				fault = error;
				childValue.Dispose ();
			} else if (childValue.Length > int.MaxValue) {
				fault = string.Format ("The value of the {0} attribute is too large.", childTag);
				childValue.Dispose ();
			} else {
				queue.Add (new BufferedAttribute (childLevel, childTag, childValue));
			}

			childValue = null;
			child = null;
		}

		// Gets the property id that is used for the specified named property within this stream.
		internal int GetNamedPropertyId (TnefNameId name)
		{
			if (namedIds.TryGetValue (name, out int id))
				return id;

			if (nextNamedId > MaxNamedPropertyId)
				throw new InvalidOperationException ("The TNEF stream contains too many named properties.");

			id = nextNamedId++;
			namedIds.Add (name, id);

			return id;
		}

		/// <summary>
		/// Flush the writer.
		/// </summary>
		/// <remarks>
		/// Writes the stream header and any buffered attributes to the output stream and then flushes the output
		/// stream.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The stream or property writer for the previous attribute has not been disposed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Flush ()
		{
			CheckCanWrite ();
			WriteQueued ();
			stream.Flush ();
		}

		void ReleaseQueue ()
		{
			foreach (var attribute in queue)
				attribute.Value.Dispose ();

			queue.Clear ();
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefWriter"/> object.
		/// </summary>
		/// <remarks>
		/// <para>Writes the stream header and any buffered attributes to the output stream, flushes it and, unless
		/// the writer was created with <c>leaveOpen</c> set to <see langword="true"/>, disposes it.</para>
		/// <para>If the stream or property writer of an attribute has not been disposed, that attribute is
		/// discarded.</para>
		/// <para>Call <see cref="Flush"/> or <see cref="FlushAsync"/> before disposing the writer in order to
		/// observe errors.</para>
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			string? error = fault;

			try {
				if (child != null) {
					error ??= "The embedded message contains an attribute that was not completed.";
					childValue?.Dispose ();
					childValue = null;
					child = null;
				}

				WriteQueued ();
				stream.Flush ();
			} catch (Exception ex) {
				// Note: Dispose must not throw. The error is reported to the owner of an embedded message.
				error ??= ex.Message;
			} finally {
				ReleaseQueue ();
				disposed = true;

				if (!leaveOpen)
					stream.Dispose ();
			}

			closed?.Invoke (this, error);
			closed = null;
		}
	}
}
