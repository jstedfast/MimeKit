//
// TnefReader.cs
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
using System.Text;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF reader.
	/// </summary>
	/// <remarks>
	/// A TNEF reader.
	/// </remarks>
	public class TnefReader : IDisposable
	{
		internal const int TnefSignature = 0x223e9f78;

		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 0;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		long position;
		int checksum;
		int codepage;
		int version;
		bool closed;
		bool eos;

		/// <summary>
		/// Gets the attachment key value.
		/// </summary>
		/// <remarks>
		/// Gets the attachment key value.
		/// </remarks>
		/// <value>The attachment key value.</value>
		public short AttachmentKey {
			get; private set;
		}

		/// <summary>
		/// Gets the current attribute's level.
		/// </summary>
		/// <remarks>
		/// Gets the current attribute's level.
		/// </remarks>
		/// <value>The current attribute's level.</value>
		public TnefAttributeLevel AttributeLevel {
			get; private set;
		}

		/// <summary>
		/// Gets the length of the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// Gets the length of the current attribute's raw value.
		/// </remarks>
		/// <value>The length of the current attribute's raw value.</value>
		public int AttributeRawValueLength {
			get; private set;
		}

		/// <summary>
		/// Gets the stream offset of the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// Gets the stream offset of the current attribute's raw value.
		/// </remarks>
		/// <value>The stream offset of the current attribute's raw value.</value>
		public int AttributeRawValueStreamOffset {
			get; private set;
		}

		/// <summary>
		/// Gets the current attribute's tag.
		/// </summary>
		/// <remarks>
		/// Gets the current attribute's tag.
		/// </remarks>
		/// <value>The current attribute's tag.</value>
		public TnefAttributeTag AttributeTag {
			get; private set;
		}

		internal TnefAttributeType AttributeType {
			get { return (TnefAttributeType) ((int) AttributeTag & 0xF0000); }
		}

		/// <summary>
		/// Gets the compliance mode.
		/// </summary>
		/// <remarks>
		/// Gets the compliance mode.
		/// </remarks>
		/// <value>The compliance mode.</value>
		public TnefComplianceMode ComplianceMode {
			get; private set;
		}

		/// <summary>
		/// Gets the current compliance status of the TNEF stream.
		/// </summary>
		/// <remarks>
		/// <para>Gets the current compliance status of the TNEF stream.</para>
		/// <para>As the reader progresses, this value may change if errors are encountered.</para>
		/// </remarks>
		/// <value>The compliance status.</value>
		public TnefComplianceStatus ComplianceStatus {
			get; internal set;
		}

		internal Stream InputStream {
			get; private set;
		}

		/// <summary>
		/// Gets the message codepage.
		/// </summary>
		/// <remarks>
		/// Gets the message codepage.
		/// </remarks>
		/// <value>The message codepage.</value>
		public int MessageCodepage {
			get { return codepage; }
			private set {
				if (value == codepage)
					return;

				try {
					var encoding = Encoding.GetEncoding (value);
					codepage = encoding.CodePage;
				} catch (ArgumentOutOfRangeException) {
					ComplianceStatus |= TnefComplianceStatus.InvalidMessageCodepage;
					if (ComplianceMode == TnefComplianceMode.Strict)
						throw new TnefException (string.Format ("Invalid message codepage: {0}", value));
					codepage = 1252;
				} catch (NotSupportedException) {
					ComplianceStatus |= TnefComplianceStatus.InvalidMessageCodepage;
					if (ComplianceMode == TnefComplianceMode.Strict)
						throw new TnefException (string.Format ("Unsupported message codepage: {0}", value));
					codepage = 1252;
				}
			}
		}

		/// <summary>
		/// Gets the TNEF property reader.
		/// </summary>
		/// <remarks>
		/// Gets the TNEF property reader.
		/// </remarks>
		/// <value>The TNEF property reader.</value>
		public TnefPropertyReader TnefPropertyReader {
			get; private set;
		}

		/// <summary>
		/// Gets the current stream offset.
		/// </summary>
		/// <remarks>
		/// Gets the current stream offset.
		/// </remarks>
		/// <value>The stream offset.</value>
		public int StreamOffset {
			get { return (int) (position - (inputEnd - inputIndex)); }
		}

		/// <summary>
		/// Gets the TNEF version.
		/// </summary>
		/// <remarks>
		/// Gets the TNEF version.
		/// </remarks>
		/// <value>The TNEF version.</value>
		public int TnefVersion {
			get { return version; }
			private set {
				if (value != 0x00010000) {
					ComplianceStatus |= TnefComplianceStatus.InvalidTnefVersion;
					if (ComplianceMode == TnefComplianceMode.Strict)
						throw new TnefException (string.Format ("Invalid TNEF version: {0}", value));
				}

				version = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefReader"/> class.
		/// </summary>
		/// <remarks>
		/// <para>When reading a TNEF stream using the <see cref="TnefComplianceMode.Strict"/> mode,
		/// a <see cref="TnefException"/> will be thrown immediately at the first sign of
		/// invalid or corrupted data.</para>
		/// <para>When reading a TNEF stream using the <see cref="TnefComplianceMode.Loose"/> mode,
		/// however, compliance issues are accumulated in the <see cref="ComplianceMode"/>
		/// property, but exceptions are not raised unless the stream is too corrupted to continue.</para>
		/// </remarks>
		/// <param name="inputStream">The input stream.</param>
		/// <param name="defaultMessageCodepage">The default message codepage.</param>
		/// <param name="complianceMode">The compliance mode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputStream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="defaultMessageCodepage"/> is not a valid codepage.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="defaultMessageCodepage"/> is not a supported codepage.
		/// </exception>
		/// <exception cref="TnefException">
		/// The TNEF stream is corrupted or invalid.
		/// </exception>
		public TnefReader (Stream inputStream, int defaultMessageCodepage, TnefComplianceMode complianceMode)
		{
			if (inputStream == null)
				throw new ArgumentNullException ("inputStream");

			if (defaultMessageCodepage < 0)
				throw new ArgumentOutOfRangeException ("defaultMessageCodepage");

			if (defaultMessageCodepage != 0) {
				// make sure that this codepage is valid...
				var encoding = Encoding.GetEncoding (defaultMessageCodepage);
				codepage = encoding.CodePage;
			} else {
				codepage = 1252;
			}

			TnefPropertyReader = new TnefPropertyReader (this);
			ComplianceMode = complianceMode;
			InputStream = inputStream;

			DecodeHeader ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefReader"/> for the specified input stream.
		/// </remarks>
		/// <param name="inputStream">The input stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputStream"/> is <c>null</c>.
		/// </exception>
		public TnefReader (Stream inputStream) : this (inputStream, 0, TnefComplianceMode.Loose)
		{
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Tnef.TnefReader"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Tnef.TnefReader"/> is reclaimed by garbage collection.
		/// </remarks>
		~TnefReader ()
		{
			Dispose (false);
		}

		internal int ReadAhead (int atleast)
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
				Buffer.BlockCopy (input, index, input, start, left);
				index = start;
				start += left;
			} else if (index > 0) {
				int shift = Math.Min (index, end - start);
				Buffer.BlockCopy (input, index, input, index - shift, left);
				index -= shift;
				start = index + left;
			} else {
				// we can't shift...
				start = end;
			}

			inputIndex = index;
			inputEnd = start;

			end = input.Length - PadSize;

			if ((nread = InputStream.Read (input, start, end - start)) > 0) {
				inputEnd += nread;
				position += nread;
			} else {
				eos = true;
			}

			return inputEnd - inputIndex;
		}

		internal void SetComplianceError (TnefComplianceStatus error, Exception innerException = null)
		{
			ComplianceStatus |= error;

			if (ComplianceMode != TnefComplianceMode.Strict)
				return;

			string message = null;

			switch (error) {
			case TnefComplianceStatus.AttributeOverflow:        message = "Too many attributes."; break;
			case TnefComplianceStatus.InvalidAttribute:         message = "Invalid attribute."; break;
			case TnefComplianceStatus.InvalidAttributeChecksum: message = "Invalid attribute checksum."; break;
			case TnefComplianceStatus.InvalidAttributeLength:   message = "Invalid attribute length."; break;
			case TnefComplianceStatus.InvalidAttributeLevel:    message = "Invalid attribute level."; break;
			case TnefComplianceStatus.InvalidAttributeValue:    message = "Invalid attribute value."; break;
			case TnefComplianceStatus.InvalidDate:              message = "Invalid date."; break;
			case TnefComplianceStatus.InvalidMessageClass:      message = "Invalid message class."; break;
			case TnefComplianceStatus.InvalidMessageCodepage:   message = "Invalid message codepage."; break;
			case TnefComplianceStatus.InvalidPropertyLength:    message = "Invalid property length."; break;
			case TnefComplianceStatus.InvalidRowCount:          message = "Invalid row count."; break;
			case TnefComplianceStatus.InvalidTnefSignature:     message = "Invalid TNEF signature."; break;
			case TnefComplianceStatus.InvalidTnefVersion:       message = "Invalid TNEF version."; break;
			case TnefComplianceStatus.NestingTooDeep:           message = "Nesting too deep."; break;
			case TnefComplianceStatus.StreamTruncated:          message = "Truncated TNEF stream."; break;
			case TnefComplianceStatus.UnsupportedPropertyType:  message = "Unsupported property type."; break;
			}

			if (innerException != null)
				throw new TnefException (message, innerException);

			throw new TnefException (message);
		}

		void DecodeHeader ()
		{
			try {
				// read the TNEFSignature
				int signature = ReadInt32 ();
				if (signature != TnefSignature)
					SetComplianceError (TnefComplianceStatus.InvalidTnefSignature);

				// read the LegacyKey (ignore this value)
				AttachmentKey = ReadInt16 ();
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
				throw;
			}
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
				SetComplianceError (TnefComplianceStatus.InvalidAttributeLevel);
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
			case TnefAttributeTag.OriginalMessageClass:
			case TnefAttributeTag.Owner:
			case TnefAttributeTag.ParentId:
			case TnefAttributeTag.Priority:
			case TnefAttributeTag.RecipientTable:
			case TnefAttributeTag.RequestResponse:
			case TnefAttributeTag.SentFor:
			case TnefAttributeTag.Subject:
				break;
			case TnefAttributeTag.AttachRenderData:
				TnefPropertyReader.AttachMethod = TnefAttachMethod.ByValue;
				break;
			case TnefAttributeTag.OemCodepage:
				MessageCodepage = PeekInt32 ();
				break;
			case TnefAttributeTag.TnefVersion:
				TnefVersion = PeekInt32 ();
				break;
			default:
				SetComplianceError (TnefComplianceStatus.InvalidAttribute);
				break;
			}
		}

		static unsafe void Load32BitValue (byte *dest, byte[] src, int startIndex)
		{
			if (BitConverter.IsLittleEndian) {
				dest[0] = src[startIndex];
				dest[1] = src[startIndex + 1];
				dest[2] = src[startIndex + 2];
				dest[3] = src[startIndex + 3];
			} else {
				dest[0] = src[startIndex + 3];
				dest[1] = src[startIndex + 2];
				dest[2] = src[startIndex + 1];
				dest[3] = src[startIndex];
			}
		}

		static unsafe void Load64BitValue (byte *dest, byte[] src, int startIndex)
		{
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < 8; i++)
					dest[i] = src[startIndex + i];
			} else {
				for (int i = 0; i < 8; i++)
					dest[i] = src[startIndex + (7 - i)];
			}
		}

		internal byte ReadByte ()
		{
			if (ReadAhead (1) < 1)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 1);

			return input[inputIndex++];
		}

		internal short ReadInt16 ()
		{
			if (ReadAhead (2) < 2)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 2);

			return (short) (input[inputIndex++] | (input[inputIndex++] << 8));
		}

		internal int ReadInt32 ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 4);

			return input[inputIndex++] | (input[inputIndex++] << 8) |
				(input[inputIndex++] << 16) | (input[inputIndex++] << 24);
		}

		internal int PeekInt32 ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			return input[inputIndex] | (input[inputIndex + 1] << 8) |
				(input[inputIndex + 2] << 16) | (input[inputIndex + 3] << 24);
		}

		internal unsafe long ReadInt64 ()
		{
			if (ReadAhead (8) < 8)
				throw new EndOfStreamException ();

			long value;

			Load32BitValue ((byte *) &value, input, inputIndex);
			UpdateChecksum (input, inputIndex, 8);
			inputIndex += 8;

			return value;
		}

		internal unsafe float ReadSingle ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			float value;

			Load32BitValue ((byte *) &value, input, inputIndex);
			UpdateChecksum (input, inputIndex, 4);
			inputIndex += 4;

			return value;
		}

		internal unsafe double ReadDouble ()
		{
			if (ReadAhead (8) < 8)
				throw new EndOfStreamException ();

			double value;

			Load64BitValue ((byte *) &value, input, inputIndex);
			UpdateChecksum (input, inputIndex, 8);
			inputIndex += 8;

			return value;
		}

		internal bool Seek (int offset)
		{
			int left = offset - StreamOffset;

			if (left <= 0)
				return true;

			do {
				int n = Math.Min (inputEnd - inputIndex, left);

				UpdateChecksum (input, inputIndex, n);
				inputIndex += n;
				left -= n;

				if (left == 0)
					break;

				if (ReadAhead (left) == 0) {
					SetComplianceError (TnefComplianceStatus.StreamTruncated);
					return false;
				}
			} while (true);

			return true;
		}

		bool SkipAttributeRawValue ()
		{
			int offset = AttributeRawValueStreamOffset + AttributeRawValueLength;
			int expected, actual;

			if (!Seek (offset))
				return false;

			// Note: ReadInt16() will update the checksum, so we need to capture it here
			expected = checksum;

			try {
				actual = (ushort) ReadInt16 ();
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
				return false;
			}

			if (actual != expected)
				SetComplianceError (TnefComplianceStatus.InvalidAttributeChecksum);

			return true;
		}

		/// <summary>
		/// Advances to the next attribute in the TNEF stream.
		/// </summary>
		/// <remarks>
		/// Advances to the next attribute in the TNEF stream.
		/// </remarks>
		/// <returns><c>true</c> if there is another attribute available to be read; otherwise <c>false</c>.</returns>
		/// <exception cref="TnefException">
		/// The TNEF stream is corrupted or invalid.
		/// </exception>
		public bool ReadNextAttribute ()
		{
			if (AttributeRawValueStreamOffset != 0 && !SkipAttributeRawValue ())
				return false;

			try {
				AttributeLevel = (TnefAttributeLevel) ReadByte ();
			} catch (EndOfStreamException) {
				return false;
			}

			CheckAttributeLevel ();

			try {
				AttributeTag = (TnefAttributeTag) ReadInt32 ();
				AttributeRawValueLength = ReadInt32 ();
				AttributeRawValueStreamOffset = StreamOffset;
				checksum = 0;
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
				return false;
			}

			CheckAttributeTag ();

			if (AttributeRawValueLength < 0)
				SetComplianceError (TnefComplianceStatus.InvalidAttributeLength);

			try {
				TnefPropertyReader.Load ();
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
				return false;
			}

			return true;
		}

		void UpdateChecksum (byte[] buffer, int offset, int count)
		{
			for (int i = offset; i < offset + count; i++)
				checksum = (checksum + buffer[i]) & 0xFFFF;
		}

		/// <summary>
		/// Reads the raw attribute value data from the underlying TNEF stream.
		/// </summary>
		/// <remarks>
		/// Reads the raw attribute value data from the underlying TNEF stream.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number
		/// of bytes requested if that many bytes are not available, or zero (0) if the end of the
		/// value has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="offset"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
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
					SetComplianceError (TnefComplianceStatus.StreamTruncated);
					return 0;
				}
			} else {
				n = Math.Min (inputLeft, n);
			}

			Buffer.BlockCopy (input, inputIndex, buffer, offset, n);
			UpdateChecksum (buffer, offset, n);
			inputIndex += n;

			return n;
		}

		/// <summary>
		/// Resets the compliance status.
		/// </summary>
		/// <remarks>
		/// Resets the compliance status.
		/// </remarks>
		public void ResetComplianceStatus ()
		{
			ComplianceStatus = TnefComplianceStatus.Compliant;
		}

		/// <summary>
		/// Closes the TNEF reader and the underlying stream.
		/// </summary>
		/// <remarks>
		/// Closes the TNEF reader and the underlying stream.
		/// </remarks>
		public void Close ()
		{
			Dispose ();
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="TnefReader"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="TnefReader"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			InputStream.Dispose ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Tnef.TnefReader"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeKit.Tnef.TnefReader"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeKit.Tnef.TnefReader"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the <see cref="MimeKit.Tnef.TnefReader"/> so the garbage
		/// collector can reclaim the memory that the <see cref="MimeKit.Tnef.TnefReader"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
			closed = true;
		}

		#endregion
	}
}
