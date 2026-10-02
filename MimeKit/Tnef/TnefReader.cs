//
// TnefReader.cs
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
using System.Globalization;
using System.Numerics;
using System.Buffers.Binary;

using MimeKit.Utils;

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

		/// <summary>
		/// The default maximum nesting depth of embedded TNEF messages.
		/// </summary>
		/// <remarks>
		/// The default maximum nesting depth of embedded TNEF messages.
		/// </remarks>
		public const int DefaultMaxNestingDepth = 32;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		long position;
		int checksum;
		int codepage;
		int version;
		int maxNestingDepth = DefaultMaxNestingDepth;
		bool seenAttachmentLevel;
		bool stopped;
		bool closed;
		bool eos;

		/// <summary>
		/// Get the legacy key value from the TNEF stream header.
		/// </summary>
		/// <remarks>
		/// <para>Gets the legacy key value from the TNEF stream header.</para>
		/// <para>This is the <c>LegacyKey</c> field of the TNEF stream header as defined in
		/// <a href="https://learn.microsoft.com/openspecs/exchange_server_protocols/ms-oxtnef/">[MS-OXTNEF]</a>.
		/// It is a value chosen by the producer in order to associate the TNEF stream with the message that
		/// contains it and has no meaning to a consumer, so it is ignored by the reader and exposed only for
		/// diagnostic purposes.</para>
		/// </remarks>
		/// <value>The legacy key value.</value>
		public short AttachmentKey {
			get; private set;
		}

		/// <summary>
		/// Get the current attribute's level.
		/// </summary>
		/// <remarks>
		/// Gets the current attribute's level.
		/// </remarks>
		/// <value>The current attribute's level.</value>
		public TnefAttributeLevel AttributeLevel {
			get; private set;
		}

		/// <summary>
		/// Get the length of the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// Gets the length of the current attribute's raw value.
		/// </remarks>
		/// <value>The length of the current attribute's raw value.</value>
		public int AttributeRawValueLength {
			get; private set;
		}

		/// <summary>
		/// Get the stream offset of the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// Gets the stream offset of the current attribute's raw value.
		/// </remarks>
		/// <value>The stream offset of the current attribute's raw value.</value>
		public long AttributeRawValueStreamOffset {
			get; private set;
		}

		/// <summary>
		/// Get the current attribute's tag.
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
		/// Get the compliance mode.
		/// </summary>
		/// <remarks>
		/// Gets the compliance mode.
		/// </remarks>
		/// <value>The compliance mode.</value>
		public TnefComplianceMode ComplianceMode {
			get; private set;
		}

		/// <summary>
		/// Get the current compliance status of the TNEF stream.
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
		/// Get or set the maximum nesting depth of embedded TNEF messages that the reader should accept.
		/// </summary>
		/// <remarks>
		/// <para>This option exists in order to define the maximum recursive depth of embedded TNEF
		/// messages that <see cref="TnefPropertyReader.GetEmbeddedMessageReader"/> should accept before
		/// assuming that the TNEF stream is maliciously formed. If the value is set too large, then it
		/// is possible that a maliciously formed set of deeply nested embedded messages could cause a
		/// stack overflow.</para>
		/// <para>Once the limit has been exceeded, the <see cref="TnefComplianceStatus.NestingTooDeep"/>
		/// compliance error is recorded and the embedded message reader behaves as if it has already
		/// reached the end of the stream.</para>
		/// </remarks>
		/// <value>The maximum nesting depth. The default value is <see cref="DefaultMaxNestingDepth"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxNestingDepth {
			get { return maxNestingDepth; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxNestingDepth = value;
			}
		}

		// The nesting depth of this reader. A reader created for the top-level TNEF stream has a
		// nesting depth of 0.
		internal int NestingDepth {
			get; set;
		}

		/// <summary>
		/// Get the message codepage.
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
				} catch (Exception ex) {
					ComplianceStatus |= TnefComplianceStatus.InvalidMessageCodepage;
					if (ComplianceMode == TnefComplianceMode.Strict)
						throw new TnefException (TnefComplianceStatus.InvalidMessageCodepage, string.Format (CultureInfo.InvariantCulture, "Invalid message codepage: {0}", value), ex);

					// Note: TnefPropertyReader.DefaultEncoding is windows-1252 if the host can provide it and
					// iso-8859-1 (or us-ascii) if it cannot, so this is always a codepage that we can resolve.
					codepage = TnefPropertyReader.DefaultEncoding.CodePage;
				}
			}
		}

		/// <summary>
		/// Get the TNEF property reader.
		/// </summary>
		/// <remarks>
		/// Gets the TNEF property reader.
		/// </remarks>
		/// <value>The TNEF property reader.</value>
		public TnefPropertyReader TnefPropertyReader {
			get; private set;
		}

		/// <summary>
		/// Get the current stream offset.
		/// </summary>
		/// <remarks>
		/// Gets the current stream offset.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long StreamOffset {
			get { return position - (inputEnd - inputIndex); }
		}

		/// <summary>
		/// Get the TNEF version.
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
						throw new TnefException (TnefComplianceStatus.InvalidTnefVersion, string.Format (CultureInfo.InvariantCulture, "Invalid TNEF version: {0}", value));
				}

				version = value;
			}
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefReader"/> class.
		/// </summary>
		/// <remarks>
		/// <para>When reading a TNEF stream using the <see cref="TnefComplianceMode.Strict"/> mode,
		/// a <see cref="TnefException"/> will be thrown immediately at the first sign of
		/// invalid or corrupted data.</para>
		/// <para>When reading a TNEF stream using the <see cref="TnefComplianceMode.Loose"/> mode,
		/// however, compliance issues are accumulated in the <see cref="ComplianceMode"/>
		/// property, but exceptions are not raised unless the stream is too corrupted to continue.</para>
		/// <para>If <paramref name="defaultMessageCodepage"/> cannot be resolved by the host (which is
		/// common on non-Windows platforms unless the application has registered the
		/// <c>System.Text.Encoding.CodePages</c> provider), a fallback encoding will be used instead.</para>
		/// </remarks>
		/// <param name="inputStream">The input stream.</param>
		/// <param name="defaultMessageCodepage">The default message codepage.</param>
		/// <param name="complianceMode">The compliance mode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputStream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="defaultMessageCodepage"/> is negative.
		/// </exception>
		/// <exception cref="TnefException">
		/// The TNEF stream is corrupted or invalid.
		/// </exception>
		public TnefReader (Stream inputStream, int defaultMessageCodepage, TnefComplianceMode complianceMode)
		{
			if (inputStream is null)
				throw new ArgumentNullException (nameof (inputStream));

			if (defaultMessageCodepage < 0)
				throw new ArgumentOutOfRangeException (nameof (defaultMessageCodepage));

			if (defaultMessageCodepage != 0) {
				// Note: If the host cannot provide the requested codepage (which is common on Linux, where
				// most Windows codepages are unavailable unless the application has registered the
				// System.Text.Encoding.CodePages provider), fall back to the default encoding rather than
				// failing to parse the TNEF stream at all.
				codepage = CharsetUtils.GetEncodingOrDefault (defaultMessageCodepage, TnefPropertyReader.DefaultEncoding).CodePage;
			} else {
				codepage = TnefPropertyReader.DefaultEncoding.CodePage;
			}

			TnefPropertyReader = new TnefPropertyReader (this);
			ComplianceMode = complianceMode;
			InputStream = inputStream;

			DecodeHeader ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefReader"/> for the specified input stream.
		/// </remarks>
		/// <param name="inputStream">The input stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputStream"/> is <see langword="null"/>.
		/// </exception>
		public TnefReader (Stream inputStream) : this (inputStream, 0, TnefComplianceMode.Loose)
		{
		}

		void CheckDisposed ()
		{
			if (closed)
				throw new ObjectDisposedException ("TnefReader");
		}

		internal int ReadAhead (int atleast)
		{
			CheckDisposed ();

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

		internal void SetComplianceError (TnefComplianceStatus error, Exception? innerException = null)
		{
			ComplianceStatus |= error;

			if (ComplianceMode != TnefComplianceMode.Strict)
				return;

			string? message = null;

			switch (error) {
			case TnefComplianceStatus.AttributeOverflow:        message = "Attribute overflow."; break;
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
			case TnefComplianceStatus.Compliant: return;
			}

			if (innerException != null)
				throw new TnefException (error, message, innerException);

			throw new TnefException (error, message);
		}

		void DecodeHeader ()
		{
			try {
				// read the TNEFSignature
				int signature = ReadInt32 ();
				if (signature != TnefSignature) {
					SetComplianceError (TnefComplianceStatus.InvalidTnefSignature);

					// Note: If the signature is wrong, then this is not a TNEF stream and so there
					// is nothing meaningful left to parse. Behave as if we've reached the end of
					// the stream.
					stopped = true;
					return;
				}

				// read the LegacyKey (ignore this value)
				AttachmentKey = ReadInt16 ();
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
			}
		}

		// Prevents the reader from parsing anything further; ReadNextAttribute() will behave as if
		// the end of the stream has been reached.
		internal void Stop ()
		{
			stopped = true;
		}

		void CheckAttributeLevel ()
		{
			switch (AttributeLevel) {
			case TnefAttributeLevel.Attachment:
				// Note: Once the attachment-level attributes have begun, it is no longer
				// legal for the TNEF stream to go back to the message level.
				seenAttachmentLevel = true;
				break;
			case TnefAttributeLevel.Message:
				if (seenAttachmentLevel)
					SetComplianceError (TnefComplianceStatus.InvalidAttributeLevel);
				break;
			default:
				SetComplianceError (TnefComplianceStatus.InvalidAttributeLevel);
				break;
			}
		}

		// Returns true if the specified attribute is allowed to appear at the attachment level.
		static bool IsAttachmentLevelAttribute (TnefAttributeTag tag)
		{
			switch (tag) {
			case TnefAttributeTag.AttachCreateDate:
			case TnefAttributeTag.AttachData:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.AttachMetaFile:
			case TnefAttributeTag.AttachModifyDate:
			case TnefAttributeTag.AttachRenderData:
			case TnefAttributeTag.AttachTitle:
			case TnefAttributeTag.AttachTransportFilename:
				return true;
			default:
				return false;
			}
		}

		// Note: PidTagMessageClass is a dot-delimited ASCII string. Peek at the raw value (without
		// consuming it) in order to verify that it at least looks like one.
		void CheckMessageClass ()
		{
			// Note: [MS-OXCMSG] limits PidTagMessageClass to 255 characters.
			const int MaxMessageClassLength = 255;

			if (AttributeRawValueLength <= 0 || AttributeRawValueLength > MaxMessageClassLength) {
				SetComplianceError (TnefComplianceStatus.InvalidMessageClass);
				return;
			}

			int n = ReadAhead (AttributeRawValueLength);

			while (n < AttributeRawValueLength && !eos)
				n = ReadAhead (AttributeRawValueLength);

			if (n < AttributeRawValueLength) {
				SetComplianceError (TnefComplianceStatus.InvalidMessageClass);
				return;
			}

			for (int i = 0; i < AttributeRawValueLength; i++) {
				byte c = input[inputIndex + i];

				// Note: a single nul-terminator is allowed at the end of the value.
				if (c == 0 && i + 1 == AttributeRawValueLength)
					break;

				if (c < 0x20 || c > 0x7e) {
					SetComplianceError (TnefComplianceStatus.InvalidMessageClass);
					return;
				}
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
			case TnefAttributeTag.MessageId:
			case TnefAttributeTag.MessageStatus:
			case TnefAttributeTag.Null:
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
			case TnefAttributeTag.MessageClass:
			case TnefAttributeTag.OriginalMessageClass:
				CheckMessageClass ();
				break;
			case TnefAttributeTag.OemCodepage:
				if (AttributeRawValueLength >= 4)
					MessageCodepage = PeekInt32 ();
				else
					SetComplianceError (TnefComplianceStatus.InvalidAttributeValue);
				break;
			case TnefAttributeTag.TnefVersion:
				if (AttributeRawValueLength >= 4)
					TnefVersion = PeekInt32 ();
				else
					SetComplianceError (TnefComplianceStatus.InvalidAttributeValue);
				break;
			default:
				SetComplianceError (TnefComplianceStatus.InvalidAttribute);
				return;
			}

			// Note: attNull is allowed at either level.
			if (AttributeTag == TnefAttributeTag.Null)
				return;

			if (IsAttachmentLevelAttribute (AttributeTag)) {
				if (AttributeLevel != TnefAttributeLevel.Attachment)
					SetComplianceError (TnefComplianceStatus.InvalidAttributeLevel);
			} else if (AttributeLevel == TnefAttributeLevel.Attachment) {
				SetComplianceError (TnefComplianceStatus.InvalidAttributeLevel);
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

			var result = BinaryPrimitives.ReadInt16LittleEndian (input.AsSpan (inputIndex));

			inputIndex += 2;

			return result;
		}

		internal int ReadInt32 ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 4);

			var result = BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex));

			inputIndex += 4;

			return result;
		}

		internal int PeekInt32 ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			return BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex));
		}

		internal long ReadInt64 ()
		{
			if (ReadAhead (8) < 8)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 8);

			var result = BinaryPrimitives.ReadInt64LittleEndian (input.AsSpan (inputIndex));

			inputIndex += 8;

			return result;
		}

		internal float ReadSingle ()
		{
			if (ReadAhead (4) < 4)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 4);

			float result;

			if (BitConverter.IsLittleEndian) {
				result = BitConverter.ToSingle (input, inputIndex);
			} else {
				var bytes = new byte[4];

				for (int i = 0; i < 4; i++)
					bytes[i] = input[inputIndex + (3 - i)];

				result = BitConverter.ToSingle (bytes, 0);
			}

			inputIndex += 4;

			return result;
		}

		internal double ReadDouble ()
		{
			if (ReadAhead (8) < 8)
				throw new EndOfStreamException ();

			UpdateChecksum (input, inputIndex, 8);

			double result;

			if (BitConverter.IsLittleEndian) {
				result = BitConverter.ToDouble (input, inputIndex);
			} else {
				var bytes = new byte[8];

				for (int i = 0; i < 8; i++)
					bytes[i] = input[inputIndex + (7 - i)];

				result = BitConverter.ToDouble (bytes, 0);
			}

			inputIndex += 8;

			return result;
		}

		internal bool Skip (long count)
		{
			CheckDisposed ();

			if (count <= 0)
				return true;

			long left = count;

			do {
				int n = (int) Math.Min (inputEnd - inputIndex, left);

				UpdateChecksum (input, inputIndex, n);
				inputIndex += n;
				left -= n;

				if (left == 0)
					break;

				if (ReadAhead ((int) Math.Min (left, int.MaxValue)) == 0) {
					SetComplianceError (TnefComplianceStatus.StreamTruncated);
					return false;
				}
			} while (true);

			return true;
		}

		bool SkipAttributeRawValue ()
		{
			long offset = AttributeRawValueStreamOffset + AttributeRawValueLength;
			int expected, actual;

			if (!Skip (offset - StreamOffset))
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
		/// Advance to the next attribute in the TNEF stream.
		/// </summary>
		/// <remarks>
		/// <para>Advances to the next attribute in the TNEF stream.</para>
		/// <para>If the TNEF stream did not begin with a valid TNEF signature, then there is
		/// nothing meaningful to parse and so this method will always return
		/// <see langword="false" />.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if there is another attribute available to be read; otherwise, <see langword="false" />.</returns>
		/// <exception cref="TnefException">
		/// The TNEF stream is corrupted or invalid.
		/// </exception>
		public bool ReadNextAttribute ()
		{
			CheckDisposed ();

			if (stopped)
				return false;

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

				if (AttributeRawValueLength < 0) {
					SetComplianceError (TnefComplianceStatus.InvalidAttributeLength);
					return false;
				}

				// Note: CheckAttributeTag() peeks at the attribute value for attOemCodepage and
				// attTnefVersion, so it needs to be inside of this try block.
				CheckAttributeTag ();

				TnefPropertyReader.Load ();
			} catch (EndOfStreamException) {
				SetComplianceError (TnefComplianceStatus.StreamTruncated);
				return false;
			}

			return true;
		}

		void UpdateChecksum (byte[] buffer, int offset, int count)
		{
			// Note: the checksum is the sum of the value bytes modulo 65536. Addition modulo
			// 65536 is associative, so the sum can be accumulated at full width and masked
			// once at the end instead of after every byte.
			int end = offset + count;
			long sum = checksum;
			int i = offset;

			if (Vector.IsHardwareAccelerated && count >= Vector<byte>.Count) {
				// Note: each 32-bit lane accumulates at most 255 per iteration, so it would take
				// more than 16 million iterations to overflow. The input buffer is far smaller.
				var vsum = Vector<uint>.Zero;
				int limit = end - Vector<byte>.Count;

				while (i <= limit) {
					Vector.Widen (new Vector<byte> (buffer, i), out Vector<ushort> low, out Vector<ushort> high);
					Vector.Widen (low, out Vector<uint> a, out Vector<uint> b);
					Vector.Widen (high, out Vector<uint> c, out Vector<uint> d);

					vsum += a + b + c + d;
					i += Vector<byte>.Count;
				}

				for (int lane = 0; lane < Vector<uint>.Count; lane++)
					sum += vsum[lane];
			}

			while (i < end)
				sum += buffer[i++];

			checksum = (int) (sum & 0xFFFF);
		}

		/// <summary>
		/// Read the raw attribute value data from the underlying TNEF stream.
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
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException (nameof (count));

			CheckDisposed ();

			long dataEndOffset = AttributeRawValueStreamOffset + AttributeRawValueLength;
			long dataLeft = dataEndOffset - StreamOffset;

			// Note: dataLeft can be negative if something has over-read the attribute value.
			if (dataLeft <= 0)
				return 0;

			int inputLeft = inputEnd - inputIndex;
			int n = (int) Math.Min (dataLeft, count);

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
		/// Reset the compliance status.
		/// </summary>
		/// <remarks>
		/// Resets the compliance status.
		/// </remarks>
		public void ResetComplianceStatus ()
		{
			ComplianceStatus = TnefComplianceStatus.Compliant;
		}

		/// <summary>
		/// Close the TNEF reader and the underlying stream.
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
		/// Release the unmanaged resources used by the <see cref="TnefReader"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="TnefReader"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
		/// <see langword="false" /> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && !closed)
				InputStream.Dispose ();
		}

		/// <summary>
		/// Release all resource used by the <see cref="TnefReader"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="TnefReader"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="TnefReader"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the <see cref="TnefReader"/> so the garbage
		/// collector can reclaim the memory that the <see cref="TnefReader"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
			closed = true;
		}

		#endregion
	}
}
