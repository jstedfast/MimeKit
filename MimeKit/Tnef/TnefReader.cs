//
// TnefReader.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

namespace MimeKit.Tnef {
	public class TnefReader : IDisposable
	{
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 0;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		short checksum;
		long position;
		bool closed;
		bool eos;

		public short AttachmentKey {
			get; private set;
		}

		public TnefAttributeLevel AttributeLevel {
			get; private set;
		}

		public int AttributeRawValueLength {
			get; private set;
		}

		public int AttributeRawValueStreamOffset {
			get; private set;
		}

		public TnefAttributeTag AttributeTag {
			get; private set;
		}

		public TnefComplianceMode ComplianceMode {
			get; private set;
		}

		public TnefComplianceStatus ComplianceStatus {
			get; internal set;
		}

		internal Stream InputStream {
			get; private set;
		}

		public int MessageCodepage {
			get; private set;
		}

		public TnefPropertyReader TnefPropertyReader {
			get; private set;
		}

		public int StreamOffset {
			get { return (int) (position - (inputEnd - inputIndex)); }
		}

		public int TnefVersion {
			get; private set;
		}

		public TnefReader (Stream inputStream, int defaultMessageCodepage, TnefComplianceMode complianceMode)
		{
			TnefPropertyReader = new TnefPropertyReader (this);
			MessageCodepage = defaultMessageCodepage;
			ComplianceMode = complianceMode;
			InputStream = inputStream;

			DecodeHeader ();
		}

		public TnefReader (Stream inputStream) : this (inputStream, 1252, TnefComplianceMode.Loose)
		{
		}

		~TnefReader ()
		{
			Dispose (false);
		}

		static void MemMove (byte[] buf, int sourceIndex, int destIndex, int length)
		{
			if (sourceIndex + length > destIndex) {
				int src = sourceIndex + length - 1;
				int dest = destIndex + length - 1;
				int start = sourceIndex;

				while (src >= start)
					buf[dest--] = buf[src--];
			} else {
				int src = sourceIndex;
				int dest = destIndex;
				int end = length;

				while (src < end)
					buf[dest++] = buf[src++];
			}
		}

		int ReadAhead (int atleast)
		{
			int left = inputEnd - inputIndex;

			if (left >= atleast || eos)
				return left;

			int index = inputIndex;
			int start = inputStart;
			int end = inputEnd;
			int nread;

			// attempt to align the end of the remaining input with ReadAheadSize
			if (index >= start) {
				start -= Math.Min (ReadAheadSize, left);
				MemMove (input, index, start, left);
				index = start;
				start += left;
			} else if (index > 0) {
				int shift = Math.Min (index, end - start);
				MemMove (input, index, index - shift, left);
				index -= shift;
				start = index + left;
			} else {
				// we can't shift...
				start = end;
			}

			inputIndex = index;
			inputEnd = start;

			end = input.Length - PadSize;

			// Note: if a perviously parsed MimePart's content has been read,
			// then the stream position will have moved and will need to be
			// reset.
			if (InputStream.Position != position)
				InputStream.Seek (position, SeekOrigin.Begin);

			if ((nread = InputStream.Read (input, start, end - start)) > 0) {
				inputEnd += nread;
				position += nread;
			} else {
				eos = true;
			}

			return inputEnd - inputIndex;
		}

		void DecodeHeader ()
		{
			// read the TNEFSignature
			int signature = ReadInt32 ();
			if (signature != 0x223e9f78) {
				ComplianceStatus |= TnefComplianceStatus.InvalidTnefSignature;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid TnefSignature.");
			}

			// read the LegacyKey (ignore this value)
			ReadInt16 ();
		}

		void CheckDisposed ()
		{
			if (closed)
				throw new ObjectDisposedException ("TnefReader");
		}

		void CheckAttributeLevel ()
		{
			switch (AttributeLevel) {
			case TnefAttributeLevel.Attachment:
			case TnefAttributeLevel.Message:
				break;
			default:
				ComplianceStatus |= TnefComplianceStatus.InvalidAttributeLevel;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute level.");
				break;
			}
		}

		void CheckAttributeTag ()
		{
			switch (AttributeTag) {
			case TnefAttributeTag.AidOwner:
			case TnefAttributeTag.AttachCreateDate:
			case TnefAttributeTag.AttachData:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.AttachMetaFile:
			case TnefAttributeTag.AttachModifyDate:
			case TnefAttributeTag.AttachRenderData:
			case TnefAttributeTag.AttachTitle:
			case TnefAttributeTag.AttachTransportFilename:
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
			case TnefAttributeTag.Null:
			case TnefAttributeTag.OemCodepage:
			case TnefAttributeTag.OriginalMessageClass:
			case TnefAttributeTag.Owner:
			case TnefAttributeTag.ParentId:
			case TnefAttributeTag.Priority:
			case TnefAttributeTag.RecipientTable:
			case TnefAttributeTag.RequestResponse:
			case TnefAttributeTag.SentFor:
			case TnefAttributeTag.Subject:
			case TnefAttributeTag.TnefVersion:
				break;
			default:
				ComplianceStatus = TnefComplianceStatus.InvalidAttribute;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute.");
				break;
			}
		}

		internal bool ReadBoolean ()
		{
			return ReadByte () != 0;
		}

		internal byte ReadByte ()
		{
			if (ReadAhead (1) < 1)
				throw new EndOfStreamException ();

			return input[inputIndex++];
		}

		internal short ReadInt16 ()
		{
			if (ReadAhead (2) < 2)
				throw new EndOfStreamException ();

			return (short) (input[inputIndex++] | (input[inputIndex++] << 8));
		}

		internal int ReadInt32 ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			return input[inputIndex++] | (input[inputIndex++] << 8) |
				(input[inputIndex++] << 16) | (input[inputIndex++] << 24);
		}

		internal long ReadInt64 ()
		{
			long lo = ReadInt32 ();
			long hi = ReadInt32 ();

			return (hi << 32) | lo;
		}

		void SkipAttributeRawValue ()
		{
			int dataEndOffset = AttributeRawValueStreamOffset + AttributeRawValueLength;
			int dataLeft = dataEndOffset - StreamOffset;
			short crc16;

			if (dataLeft > 0) {
				var buffer = new byte[Math.Min (4096, dataLeft)];
				int nread;

				do {
					nread = ReadAttributeRawValue (buffer, 0, buffer.Length);
				} while (nread > 0);
			}

			try {
				crc16 = ReadInt16 ();
			} catch (EndOfStreamException) {
				ComplianceStatus |= TnefComplianceStatus.StreamTruncated;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Truncated TNEF stream.");
				throw;
			}

			if (crc16 != checksum) {
				ComplianceStatus |= TnefComplianceStatus.InvalidAttributeChecksum;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute checksum.");
			}
		}

		public bool ReadNextAttribute ()
		{
			if (AttributeRawValueStreamOffset != 0)
				SkipAttributeRawValue ();

			try {
				AttributeLevel = (TnefAttributeLevel) ReadByte ();
			} catch (EndOfStreamException) {
				return false;
			}

			try {
				AttributeTag = (TnefAttributeTag) ReadInt32 ();
				AttributeRawValueLength = ReadInt32 ();
				AttributeRawValueStreamOffset = StreamOffset;
				checksum = 0;
			} catch (EndOfStreamException) {
				ComplianceStatus |= TnefComplianceStatus.StreamTruncated;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Truncated TNEF stream.");

				return false;
			}

			CheckAttributeLevel ();
			CheckAttributeTag ();

			if (AttributeRawValueLength < 0) {
				ComplianceStatus = TnefComplianceStatus.InvalidAttributeLength;
				if (ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute length.");
			}

			TnefPropertyReader.Load ();

			return true;
		}

		void UpdateChecksum (byte[] buffer, int offset, int count)
		{
			for (int i = offset; i < offset + count; i++)
				checksum += (short) (buffer[i] & 0xff);
		}

		public int ReadAttributeRawValue (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			int dataEndOffset = AttributeRawValueStreamOffset + AttributeRawValueLength;
			int dataLeft = dataEndOffset - StreamOffset;

			if (dataLeft == 0)
				return 0;

			int inputLeft = inputEnd - inputIndex;
			int n = Math.Min (dataLeft, count);

			if (n > inputLeft && inputLeft < ReadAheadSize) {
				if ((n = Math.Min (ReadAhead (n), n)) == 0) {
					ComplianceStatus |= TnefComplianceStatus.StreamTruncated;
					throw new TnefException ("Truncated TNEF stream.");
				}
			}

			Buffer.BlockCopy (input, inputIndex, buffer, offset, n);
			UpdateChecksum (buffer, offset, n);
			inputIndex += n;

			return n;
		}

		public void ResetComplianceStatus ()
		{
			ComplianceStatus = TnefComplianceStatus.Compliant;
		}

		public void Close ()
		{
			Dispose ();
		}

		#region IDisposable implementation

		protected virtual void Dispose (bool disposing)
		{
			InputStream.Dispose ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
			closed = true;
		}

		#endregion
	}
}
