//
// TnefBuilder.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	/// <summary>
	/// A test-only builder that emits well-formed (and, on request, deliberately
	/// malformed) TNEF streams as described by [MS-OXTNEF].
	/// </summary>
	class TnefBuilder
	{
		public const int TnefSignature = 0x223e9f78;

		readonly MemoryStream stream = new MemoryStream ();

		public TnefBuilder (int signature = TnefSignature, short legacyKey = 0)
		{
			WriteInt32 (signature);
			WriteInt16 (legacyKey);
		}

		void WriteInt16 (short value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
		}

		void WriteInt32 (int value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
			stream.WriteByte ((byte) ((value >> 16) & 0xFF));
			stream.WriteByte ((byte) ((value >> 24) & 0xFF));
		}

		static short Checksum (byte[] payload)
		{
			int checksum = 0;

			for (int i = 0; i < payload.Length; i++)
				checksum = (checksum + payload[i]) & 0xFFFF;

			return (short) checksum;
		}

		/// <summary>
		/// Append a single TNEF attribute.
		/// </summary>
		/// <param name="level">The attribute level.</param>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="payload">The raw attribute value.</param>
		/// <param name="length">An optional length to write instead of the true payload length.</param>
		/// <param name="checksum">An optional checksum to write instead of the true checksum.</param>
		public TnefBuilder WriteAttribute (TnefAttributeLevel level, TnefAttributeTag tag, byte[] payload, int? length = null, short? checksum = null)
		{
			stream.WriteByte ((byte) level);
			WriteInt32 ((int) tag);
			WriteInt32 (length ?? payload.Length);
			stream.Write (payload, 0, payload.Length);
			WriteInt16 (checksum ?? Checksum (payload));

			return this;
		}

		public TnefBuilder WriteOemCodepage (int codepage)
		{
			return WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage, Int32Payload (codepage));
		}

		public TnefBuilder WriteTnefVersion (int version = 0x00010000)
		{
			return WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, Int32Payload (version));
		}

		public TnefBuilder WriteMessageClass (string messageClass)
		{
			var payload = new byte[messageClass.Length + 1];

			Encoding.ASCII.GetBytes (messageClass, 0, messageClass.Length, payload, 0);

			return WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, payload);
		}

		public static byte[] Int32Payload (int value)
		{
			return new byte[] {
				(byte) (value & 0xFF),
				(byte) ((value >> 8) & 0xFF),
				(byte) ((value >> 16) & 0xFF),
				(byte) ((value >> 24) & 0xFF)
			};
		}

		/// <summary>
		/// Append raw bytes directly to the stream, bypassing all framing.
		/// </summary>
		public TnefBuilder WriteRaw (byte[] bytes)
		{
			stream.Write (bytes, 0, bytes.Length);

			return this;
		}

		public byte[] ToArray ()
		{
			return stream.ToArray ();
		}

		/// <summary>
		/// Get a readable stream positioned at the beginning of the TNEF data.
		/// </summary>
		/// <param name="truncateAt">If greater than or equal to zero, truncate the stream to this many bytes.</param>
		public MemoryStream ToStream (int truncateAt = -1)
		{
			var buffer = ToArray ();
			int length = truncateAt < 0 ? buffer.Length : Math.Min (truncateAt, buffer.Length);

			return new MemoryStream (buffer, 0, length, false, true);
		}

		public TnefBuilder WriteMapiProperties (TnefAttributeLevel level, TnefMapiPropertyBuilder properties, int? count = null, int? length = null)
		{
			var tag = level == TnefAttributeLevel.Attachment ? TnefAttributeTag.Attachment : TnefAttributeTag.MapiProperties;

			return WriteAttribute (level, tag, properties.ToArray (count), length);
		}

		/// <summary>
		/// Append an attRecipTable attribute consisting of the specified rows of MAPI properties.
		/// </summary>
		public TnefBuilder WriteRecipientTable (params TnefMapiPropertyBuilder[] rows)
		{
			var payload = new MemoryStream ();

			payload.Write (Int32Payload (rows.Length), 0, 4);

			foreach (var row in rows) {
				var bytes = row.ToArray ();

				payload.Write (bytes, 0, bytes.Length);
			}

			return WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable, payload.ToArray ());
		}
	}

	/// <summary>
	/// A test-only builder for the MAPI property payload of an attMsgProps or attAttachment attribute.
	/// </summary>
	class TnefMapiPropertyBuilder
	{
		readonly MemoryStream stream = new MemoryStream ();
		int count;

		public int Count {
			get { return count; }
		}

		void WriteInt16 (short value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
		}

		void WriteInt32 (int value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
			stream.WriteByte ((byte) ((value >> 16) & 0xFF));
			stream.WriteByte ((byte) ((value >> 24) & 0xFF));
		}

		void WriteInt64 (long value)
		{
			WriteInt32 ((int) (value & 0xFFFFFFFF));
			WriteInt32 ((int) ((value >> 32) & 0xFFFFFFFF));
		}

		void WritePropertyTag (TnefPropertyTag tag)
		{
			WriteInt16 ((short) tag.TnefType);
			WriteInt16 ((short) tag.Id);
		}

		/// <summary>
		/// Write a property tag (and, if the property is named, its name) without any value.
		/// </summary>
		public TnefMapiPropertyBuilder WritePropertyHeader (TnefPropertyTag tag, Guid? guid = null, string name = null, int? nameId = null)
		{
			WritePropertyTag (tag);

			if (tag.IsNamed) {
				var bytes = (guid ?? Guid.Empty).ToByteArray ();

				stream.Write (bytes, 0, bytes.Length);

				if (name != null) {
					WriteInt32 (0); // TnefNameIdKind.Name
					WriteUnicodeValue (name);
				} else {
					WriteInt32 (1); // TnefNameIdKind.Id
					WriteInt32 (nameId ?? 0);
				}
			}

			count++;

			return this;
		}

		/// <summary>
		/// Write a length-prefixed value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="length">An optional length to write instead of the true value length.</param>
		/// <param name="pad">If <c>false</c>, omit the 4-byte alignment padding.</param>
		public TnefMapiPropertyBuilder WriteVariableLengthValue (byte[] value, int? length = null, bool pad = true)
		{
			WriteInt32 (length ?? value.Length);
			stream.Write (value, 0, value.Length);

			if (pad) {
				for (int i = value.Length; (i % 4) != 0; i++)
					stream.WriteByte (0);
			}

			return this;
		}

		/// <summary>
		/// Append raw bytes directly to the property payload, bypassing all framing.
		/// </summary>
		public TnefMapiPropertyBuilder WriteRaw (byte[] bytes)
		{
			stream.Write (bytes, 0, bytes.Length);

			return this;
		}

		public TnefMapiPropertyBuilder WriteUnicodeValue (string value)
		{
			var bytes = Encoding.Unicode.GetBytes (value + "\0");

			return WriteVariableLengthValue (bytes);
		}

		/// <summary>
		/// Write a complete single-valued property with a raw (already encoded) value.
		/// </summary>
		public TnefMapiPropertyBuilder WriteProperty (TnefPropertyTag tag, byte[] rawValue)
		{
			WritePropertyHeader (tag);
			stream.Write (rawValue, 0, rawValue.Length);

			return this;
		}

		public TnefMapiPropertyBuilder WriteInt32Property (TnefPropertyTag tag, int value)
		{
			WritePropertyHeader (tag);
			WriteInt32 (value);

			return this;
		}

		public TnefMapiPropertyBuilder WriteInt64Property (TnefPropertyTag tag, long value)
		{
			WritePropertyHeader (tag);
			WriteInt64 (value);

			return this;
		}

		public TnefMapiPropertyBuilder WriteDoubleProperty (TnefPropertyTag tag, double value)
		{
			WritePropertyHeader (tag);
			WriteInt64 (BitConverter.DoubleToInt64Bits (value));

			return this;
		}

		public TnefMapiPropertyBuilder WriteStringProperty (TnefPropertyTag tag, string value, Encoding encoding = null)
		{
			WritePropertyHeader (tag);

			var bytes = (encoding ?? Encoding.Unicode).GetBytes (value + "\0");

			// Note: string properties are written as a count of values followed by each value.
			WriteInt32 (1);

			return WriteVariableLengthValue (bytes);
		}

		public TnefMapiPropertyBuilder WriteBinaryProperty (TnefPropertyTag tag, byte[] value, int? length = null, bool pad = true)
		{
			WritePropertyHeader (tag);
			WriteInt32 (1);

			return WriteVariableLengthValue (value, length, pad);
		}

		public TnefMapiPropertyBuilder WriteGuidProperty (TnefPropertyTag tag, Guid guid)
		{
			WritePropertyHeader (tag);

			var bytes = guid.ToByteArray ();

			stream.Write (bytes, 0, bytes.Length);

			return this;
		}

		/// <summary>
		/// Write a multi-valued property header followed by an explicit value count.
		/// </summary>
		public TnefMapiPropertyBuilder WriteValueCount (int valueCount)
		{
			WriteInt32 (valueCount);

			return this;
		}

		public byte[] ToArray (int? propertyCount = null)
		{
			var properties = stream.ToArray ();
			var buffer = new byte[4 + properties.Length];
			int n = propertyCount ?? count;

			buffer[0] = (byte) (n & 0xFF);
			buffer[1] = (byte) ((n >> 8) & 0xFF);
			buffer[2] = (byte) ((n >> 16) & 0xFF);
			buffer[3] = (byte) ((n >> 24) & 0xFF);

			Buffer.BlockCopy (properties, 0, buffer, 4, properties.Length);

			return buffer;
		}
	}
}
