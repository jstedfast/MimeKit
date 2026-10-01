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
	}
}
