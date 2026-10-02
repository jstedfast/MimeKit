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
using System.Numerics;
using System.Threading;
using System.Buffers.Binary;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A forward-only reader for TNEF streams.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefReader"/> provides forward-only, pull-style access to the attributes of a
	/// TNEF (Transport Neutral Encapsulation Format) stream as defined by [MS-OXTNEF].</para>
	/// <para>Each call to <see cref="Read(CancellationToken)"/> advances the reader to the next attribute.
	/// The value of the current attribute may be consumed using one of the <c>ReadValueAs*</c> methods,
	/// <see cref="OpenValueStream"/>, or, for attributes that contain MAPI properties,
	/// <see cref="GetPropertyReader"/>. Any part of the value that is not consumed is skipped
	/// automatically by the next call to <see cref="Read(CancellationToken)"/>.</para>
	/// <para>The reader never throws because of malformed data. Instead, each problem that is detected is
	/// reported to the <see cref="ComplianceLogger"/> (if any) and the reader recovers as best it can.
	/// To stop at the first problem, use a logger that throws.</para>
	/// </remarks>
	public sealed partial class TnefReader : IDisposable
	{
		internal const int TnefSignature = 0x223e9f78;
		internal const int TnefVersion = 0x00010000;

		// Note: [MS-OXCMSG] limits PidTagMessageClass to 255 characters.
		const int MaxMessageClassLength = 255;
		const int AttributeHeaderSize = 9;
		const int BufferSize = 4096;

		// Note: Values up to this size are always allocated up front. Larger values are only allocated up
		// front if the stream is known to contain enough data. Otherwise, they are read in chunks so that a
		// bogus length cannot force a huge allocation.
		const int MaxUnverifiedAllocation = 64 * 1024;

		internal static readonly Encoding DefaultEncoding = CharsetUtils.GetEncodingOrDefault (1252, CharsetUtils.Latin1);

		enum ReaderState : byte
		{
			Initial,
			Attribute,
			Done
		}

		enum ValueClaim : byte
		{
			None,
			Scalar,
			Raw,
			Properties
		}

		readonly byte[] input = new byte[BufferSize];
		readonly byte[] scalar = new byte[16];
		readonly TnefOptions options;
		readonly bool skipIidPrefix;
		readonly long baseOffset;
		readonly bool leaveOpen;
		readonly Stream stream;
		readonly int depth;

		ITnefComplianceLogger? userComplianceLogger;
		ITnefComplianceLogger? complianceLogger;
		int maxComplianceIssuesPerViolation;
		bool inheritedComplianceLogger;

		int inputIndex, inputEnd;
		long position;
		bool eos;

		ReaderState state;
		bool seenAttachmentLevel;
		bool truncated;
		bool disposed;
		Encoding encoding;
		int codepage;

		// The state of the current attribute.
		TnefPropertyReader? propertyReader;
		long attributeOffset;
		long valueEnd;
		int checksum;
		ValueClaim claim;
		int scalarLength;
		DateTime dateValue;
		int generation;
		int valueGeneration;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefReader"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="TnefReader"/> for the specified stream.</para>
		/// <para>The constructor does not read anything from the stream. The TNEF stream header is read by
		/// the first call to <see cref="Read(CancellationToken)"/> or <see cref="ReadAsync(CancellationToken)"/>.</para>
		/// </remarks>
		/// <param name="stream">The TNEF stream.</param>
		/// <param name="options">The options to use, or <see langword="null"/> to use <see cref="TnefOptions.Default"/>.</param>
		/// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="stream"/> open when the reader is disposed;
		/// otherwise, <see langword="false"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		public TnefReader (Stream stream, TnefOptions? options = null, bool leaveOpen = false)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			this.options = options ?? TnefOptions.Default;
			this.leaveOpen = leaveOpen;
			this.stream = stream;

			if (this.options.DefaultCodepage != 0) {
				// Note: If the host cannot provide the requested codepage (which is common on Linux, where
				// most Windows codepages are unavailable unless the application has registered the
				// System.Text.Encoding.CodePages provider), fall back to the default encoding rather than
				// failing to read the TNEF stream at all.
				encoding = CharsetUtils.GetEncodingOrDefault (this.options.DefaultCodepage, DefaultEncoding);
			} else {
				encoding = DefaultEncoding;
			}

			codepage = encoding.CodePage;
			ResetAttribute ();
		}

		// Creates a reader for a TNEF message that is embedded within the value of a MAPI property of the parent.
		TnefReader (TnefReader parent, Stream stream, long baseOffset)
		{
			options = parent.options;
			depth = parent.depth + 1;
			encoding = parent.encoding;
			codepage = parent.codepage;
			userComplianceLogger = parent.userComplianceLogger;
			complianceLogger = parent.complianceLogger;
			maxComplianceIssuesPerViolation = parent.maxComplianceIssuesPerViolation;
			inheritedComplianceLogger = true;
			this.baseOffset = baseOffset;
			this.stream = stream;
			skipIidPrefix = true;
			leaveOpen = true;
			ResetAttribute ();
		}

		/// <summary>
		/// Get or set the logger to use for reporting TNEF compliance violations.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the logger to use for reporting TNEF compliance violations.</para>
		/// <para>Readers created by <see cref="TnefPropertyReader.OpenEmbeddedMessage"/> report to the same
		/// logger as the reader that created them.</para>
		/// </remarks>
		/// <value>The TNEF compliance logger.</value>
		public ITnefComplianceLogger? ComplianceLogger {
			get { return userComplianceLogger; }
			set {
				userComplianceLogger = value;
				inheritedComplianceLogger = false;
				UpdateComplianceLogger ();
			}
		}

		/// <summary>
		/// Get or set the maximum number of times that each compliance violation may be reported.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of times that each compliance violation may be
		/// reported while reading a single TNEF stream. The default is <c>0</c>, which means that no limit
		/// is applied.</para>
		/// <para>The number of issues a TNEF stream can produce is bounded only by its size, and the
		/// densest forms cost only a few bytes per issue. Setting a limit is recommended when reading
		/// untrusted streams.</para>
		/// <para>When some violation reaches the limit, a single
		/// <see cref="TnefComplianceViolation.TooManyComplianceIssues"/> issue is reported so that the
		/// report is never silently incomplete.</para>
		/// </remarks>
		/// <value>The maximum number of times that each violation may be reported, or <c>0</c> for no limit.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxComplianceIssuesPerViolation {
			get { return maxComplianceIssuesPerViolation; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxComplianceIssuesPerViolation = value;
				inheritedComplianceLogger = false;
				UpdateComplianceLogger ();
			}
		}

		/// <summary>
		/// Get the options used by the reader.
		/// </summary>
		/// <remarks>
		/// Gets the options used by the reader.
		/// </remarks>
		/// <value>The options.</value>
		public TnefOptions Options {
			get { return options; }
		}

		/// <summary>
		/// Get the nesting depth of the reader.
		/// </summary>
		/// <remarks>
		/// <para>Gets the nesting depth of the reader.</para>
		/// <para>A reader for a top-level TNEF stream has a depth of <c>0</c>. A reader created by
		/// <see cref="TnefPropertyReader.OpenEmbeddedMessage"/> has a depth one greater than the reader
		/// that created it.</para>
		/// </remarks>
		/// <value>The nesting depth.</value>
		public int Depth {
			get { return depth; }
		}

		/// <summary>
		/// Get the legacy key from the TNEF stream header.
		/// </summary>
		/// <remarks>
		/// <para>Gets the legacy key from the TNEF stream header.</para>
		/// <para>The legacy key is only meaningful once the stream header has been read by the first call to
		/// <see cref="Read(CancellationToken)"/>.</para>
		/// </remarks>
		/// <value>The legacy key.</value>
		public ushort LegacyKey {
			get; private set;
		}

		/// <summary>
		/// Get the codepage used to decode 8-bit strings.
		/// </summary>
		/// <remarks>
		/// <para>Gets the codepage used to decode 8-bit strings.</para>
		/// <para>The codepage is initially determined by <see cref="TnefOptions.DefaultCodepage"/> and is updated
		/// when the reader reads a <see cref="TnefAttributeTag.OemCodepage"/> attribute.</para>
		/// </remarks>
		/// <value>The codepage.</value>
		public int Codepage {
			get { return codepage; }
		}

		/// <summary>
		/// Get the level of the current attribute.
		/// </summary>
		/// <remarks>
		/// Gets the level of the current attribute.
		/// </remarks>
		/// <value>The attribute level.</value>
		public TnefAttributeLevel Level {
			get; private set;
		}

		/// <summary>
		/// Get the tag of the current attribute.
		/// </summary>
		/// <remarks>
		/// Gets the tag of the current attribute.
		/// </remarks>
		/// <value>The attribute tag.</value>
		public TnefAttributeTag Tag {
			get; private set;
		}

		/// <summary>
		/// Get the type of the current attribute's value.
		/// </summary>
		/// <remarks>
		/// Gets the type of the current attribute's value, as encoded in the attribute's <see cref="Tag"/>.
		/// </remarks>
		/// <value>The attribute type.</value>
		public TnefAttributeType AttributeType {
			get { return (TnefAttributeType) ((int) Tag & unchecked ((int) 0xFFFF0000)); }
		}

		/// <summary>
		/// Get the length of the current attribute's value.
		/// </summary>
		/// <remarks>
		/// Gets the length of the current attribute's value, in bytes.
		/// </remarks>
		/// <value>The length of the attribute value.</value>
		public int Length {
			get; private set;
		}

		/// <summary>
		/// Get the stream offset of the current attribute.
		/// </summary>
		/// <remarks>
		/// <para>Gets the offset of the current attribute's header within the outermost TNEF stream.</para>
		/// <para>For readers created by <see cref="TnefPropertyReader.OpenEmbeddedMessage"/>, the offset is
		/// relative to the start of the outermost TNEF stream rather than to the start of the embedded
		/// message.</para>
		/// </remarks>
		/// <value>The stream offset.</value>
		public long StreamOffset {
			get { return baseOffset + attributeOffset; }
		}

		// Whether the reader has been advanced (i.e. Read() or ReadAsync() has been called at least once).
		internal bool HasStarted {
			get { return state != ReaderState.Initial; }
		}

		internal Encoding Encoding {
			get { return encoding; }
		}

		internal TnefAttachMethod AttachMethod {
			get; set;
		}

		// The current offset relative to the start of this reader's stream.
		internal long LocalOffset {
			get { return position - (inputEnd - inputIndex); }
		}

		// The absolute offset of the current position within the outermost TNEF stream.
		internal long AbsoluteOffset {
			get { return baseOffset + LocalOffset; }
		}

		// The end of the current attribute's value, relative to the start of this reader's stream.
		internal long ValueEnd {
			get { return valueEnd; }
		}

		// The number of bytes of the current attribute's value that have not been consumed.
		internal long ValueRemaining {
			get { return Math.Max (valueEnd - LocalOffset, 0); }
		}

		internal int Generation {
			get { return generation; }
		}

		internal int ValueGeneration {
			get { return valueGeneration; }
		}

		internal bool IsTruncated {
			get { return truncated; }
		}

		internal int BufferedCount {
			get { return inputEnd - inputIndex; }
		}

		void UpdateComplianceLogger ()
		{
			if (userComplianceLogger != null && maxComplianceIssuesPerViolation > 0)
				complianceLogger = new TnefCappedComplianceLogger (userComplianceLogger, maxComplianceIssuesPerViolation);
			else
				complianceLogger = userComplianceLogger;
		}

		// Note: A reader for an embedded message shares the budget of the reader that created it, so only a
		// reader that owns its logger resets the budget.
		void ResetComplianceBudget ()
		{
			if (!inheritedComplianceLogger)
				(complianceLogger as TnefCappedComplianceLogger)?.Reset ();
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefReader));
		}

		internal void Log (TnefComplianceViolation violation, long localOffset, TnefPropertyTag propertyTag)
		{
			var logger = complianceLogger;

			if (logger is null)
				return;

			var attributeTag = state == ReaderState.Attribute ? Tag : TnefAttributeTag.Null;
			var issue = new TnefComplianceIssue (violation, baseOffset + localOffset, depth, attributeTag, propertyTag);

			logger.Log (issue);
		}

		internal void Log (TnefComplianceViolation violation, long localOffset)
		{
			Log (violation, localOffset, TnefPropertyTag.Null);
		}

		void LogAttribute (TnefComplianceViolation violation)
		{
			Log (violation, attributeOffset, TnefPropertyTag.Null);
		}

		// Note: Once the stream has been found to be truncated, nothing further can be read.
		internal void SetTruncated ()
		{
			if (truncated)
				return;

			truncated = true;
			Log (TnefComplianceViolation.TruncatedStream, LocalOffset);
		}

		internal void SetTruncated (TnefPropertyTag propertyTag)
		{
			if (truncated)
				return;

			truncated = true;
			Log (TnefComplianceViolation.TruncatedStream, LocalOffset, propertyTag);
		}

		internal void IncrementValueGeneration ()
		{
			valueGeneration++;
		}

		void ResetAttribute ()
		{
			Level = 0;
			Tag = TnefAttributeTag.Null;
			Length = 0;
			attributeOffset = LocalOffset;
			valueEnd = attributeOffset;
			propertyReader = null;
			claim = ValueClaim.None;
			scalarLength = -1;
			checksum = 0;
		}

		int ReadStream (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (stream is ICancellableStream cancellable)
				return cancellable.Read (buffer, offset, count, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested ();

			return stream.Read (buffer, offset, count);
		}

		// Moves any buffered data to the start of the buffer so that there is room to read more.
		void CompactBuffer ()
		{
			int left = inputEnd - inputIndex;

			if (inputIndex > 0) {
				if (left > 0)
					Buffer.BlockCopy (input, inputIndex, input, 0, left);

				inputIndex = 0;
				inputEnd = left;
			}
		}

		// Ensures that at least the specified number of bytes (which must not exceed the buffer size) are
		// buffered. Returns false if the end of the stream is reached first.
		internal bool Fill (int count, CancellationToken cancellationToken)
		{
			if (inputEnd - inputIndex >= count)
				return true;

			CompactBuffer ();

			while (inputEnd < count) {
				if (eos)
					return false;

				int nread = ReadStream (input, inputEnd, input.Length - inputEnd, cancellationToken);

				if (nread <= 0) {
					eos = true;
					return false;
				}

				inputEnd += nread;
				position += nread;
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
				// Note: each 32-bit lane accumulates at most 4 * 255 per iteration, so the lanes are
				// flushed into the 64-bit sum every BlockIterations iterations to rule out overflow no
				// matter how large the buffer is.
				const int BlockIterations = 1 << 20;
				int limit = end - Vector<byte>.Count;

				while (i <= limit) {
					var vsum = Vector<uint>.Zero;
					int n = 0;

					while (i <= limit && n < BlockIterations) {
						Vector.Widen (new Vector<byte> (buffer, i), out Vector<ushort> low, out Vector<ushort> high);
						Vector.Widen (low, out Vector<uint> a, out Vector<uint> b);
						Vector.Widen (high, out Vector<uint> c, out Vector<uint> d);

						vsum += a + b + c + d;
						i += Vector<byte>.Count;
						n++;
					}

					for (int lane = 0; lane < Vector<uint>.Count; lane++)
						sum += vsum[lane];
				}
			}

			while (i < end)
				sum += buffer[i++];

			checksum = (int) (sum & 0xFFFF);
		}

		// Consumes the specified number of buffered value bytes, copying them into the destination buffer.
		internal void TakeBytes (byte[] buffer, int offset, int count)
		{
			Buffer.BlockCopy (input, inputIndex, buffer, offset, count);
			UpdateChecksum (input, inputIndex, count);
			inputIndex += count;
		}

		// Copies the specified number of buffered bytes into the destination buffer without consuming them.
		internal void PeekBytes (byte[] buffer, int offset, int count)
		{
			Buffer.BlockCopy (input, inputIndex, buffer, offset, count);
		}

		// Consumes a buffered 16-bit little-endian value.
		internal short TakeInt16 ()
		{
			var value = BinaryPrimitives.ReadInt16LittleEndian (input.AsSpan (inputIndex, 2));
			UpdateChecksum (input, inputIndex, 2);
			inputIndex += 2;
			return value;
		}

		// Consumes a buffered 32-bit little-endian value.
		internal int TakeInt32 ()
		{
			var value = BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex, 4));
			UpdateChecksum (input, inputIndex, 4);
			inputIndex += 4;
			return value;
		}

		// Skips the specified number of value bytes, updating the checksum as it goes.
		internal bool Skip (long count, CancellationToken cancellationToken)
		{
			while (count > 0) {
				if (inputIndex == inputEnd && !Fill (1, cancellationToken))
					return false;

				int n = (int) Math.Min (inputEnd - inputIndex, count);

				UpdateChecksum (input, inputIndex, n);
				inputIndex += n;
				count -= n;
			}

			return true;
		}

		// Reads up to count bytes of value data (the caller is responsible for not reading past the end of
		// the value). Returns 0 only at the end of the stream.
		internal int ReadValueData (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (count == 0)
				return 0;

			int n;

			if (inputIndex == inputEnd) {
				if (eos)
					return 0;

				if (count >= BufferSize) {
					// Note: Large reads bypass the buffer and go straight into the caller's buffer.
					if ((n = ReadStream (buffer, offset, count, cancellationToken)) <= 0) {
						eos = true;
						return 0;
					}

					position += n;
					UpdateChecksum (buffer, offset, n);

					return n;
				}

				if (!Fill (1, cancellationToken) && inputIndex == inputEnd)
					return 0;
			}

			n = Math.Min (inputEnd - inputIndex, count);
			TakeBytes (buffer, offset, n);

			return n;
		}

		// Reads value data until the requested number of bytes have been read or the stream ends.
		internal int ReadValueDataFully (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			int nread = 0;
			int n;

			while (nread < count && (n = ReadValueData (buffer, offset + nread, count - nread, cancellationToken)) > 0)
				nread += n;

			return nread;
		}

		// Returns true if an allocation of the specified size can be made up front without trusting a length
		// that might be bogus.
		internal bool CanAllocate (long count)
		{
			if (count <= MaxUnverifiedAllocation)
				return true;

			long buffered = inputEnd - inputIndex;

			if (count <= buffered)
				return true;

			count -= buffered;

			try {
				if (stream is TnefReaderStream bounded)
					return count <= bounded.Remaining && bounded.CanAllocate (count);

				return stream.CanSeek && count <= stream.Length - stream.Position;
			} catch (NotSupportedException) {
				return false;
			}
		}

		// Reads the specified number of value bytes. If the stream ends first, the truncation is reported
		// and the bytes that were available are returned.
		internal byte[] ReadValueBytes (int count, TnefPropertyTag propertyTag, CancellationToken cancellationToken)
		{
			if (count == 0)
				return Array.Empty<byte> ();

			byte[] buffer;
			int nread;

			if (CanAllocate (count)) {
				buffer = new byte[count];
				nread = ReadValueDataFully (buffer, 0, count, cancellationToken);
			} else {
				buffer = new byte[MaxUnverifiedAllocation];
				nread = 0;

				while (nread < count) {
					if (nread == buffer.Length)
						Array.Resize (ref buffer, (int) Math.Min ((long) buffer.Length * 2, count));

					int n = ReadValueData (buffer, nread, buffer.Length - nread, cancellationToken);

					if (n == 0)
						break;

					nread += n;
				}
			}

			if (nread < count) {
				SetTruncated (propertyTag);
				Array.Resize (ref buffer, nread);
			} else if (buffer.Length > nread) {
				Array.Resize (ref buffer, nread);
			}

			return buffer;
		}

		internal static string DecodeString (Encoding encoding, byte[] bytes)
		{
			int length = bytes.Length;

			// Note: 8-bit strings are usually nul-terminated.
			while (length > 0 && bytes[length - 1] == 0)
				length--;

			if (length == 0)
				return string.Empty;

			try {
				return encoding.GetString (bytes, 0, length);
			} catch (DecoderFallbackException) {
				return DefaultEncoding.GetString (bytes, 0, length);
			}
		}

		bool ProcessHeader ()
		{
			int signature = BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex, 4));

			if (signature != TnefSignature) {
				// Note: If the signature is wrong, then this is not a TNEF stream and so there is nothing
				// meaningful left to read.
				Log (TnefComplianceViolation.InvalidSignature, LocalOffset);
				return false;
			}

			inputIndex += 4;

			return true;
		}

		bool ReadHeader (CancellationToken cancellationToken)
		{
			if (skipIidPrefix) {
				// Note: An embedded message's data begins with the 16-byte IID of the IMessage interface.
				if (!Fill (16, cancellationToken)) {
					SetTruncated ();
					return false;
				}

				inputIndex += 16;
			}

			if (!Fill (4, cancellationToken)) {
				SetTruncated ();
				return false;
			}

			if (!ProcessHeader ())
				return false;

			if (!Fill (2, cancellationToken)) {
				SetTruncated ();
				return false;
			}

			LegacyKey = BinaryPrimitives.ReadUInt16LittleEndian (input.AsSpan (inputIndex, 2));
			inputIndex += 2;

			return true;
		}

		// Parses the buffered attribute header. Returns false if the attribute cannot be read.
		bool ProcessAttributeHeader ()
		{
			var level = (TnefAttributeLevel) input[inputIndex];
			int tag = BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex + 1, 4));
			int length = BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex + 5, 4));

			inputIndex += AttributeHeaderSize;

			Level = level;
			Tag = (TnefAttributeTag) tag;
			Length = Math.Max (length, 0);
			valueEnd = LocalOffset + Length;
			state = ReaderState.Attribute;

			switch (level) {
			case TnefAttributeLevel.Attachment:
				// Note: Once the attachment-level attributes have begun, it is no longer legal for the
				// TNEF stream to go back to the message level.
				seenAttachmentLevel = true;
				break;
			case TnefAttributeLevel.Message:
				if (seenAttachmentLevel)
					LogAttribute (TnefComplianceViolation.MessageAttributeAfterAttachment);
				break;
			default:
				LogAttribute (TnefComplianceViolation.InvalidAttributeLevel);
				break;
			}

			if (length < 0) {
				// Note: The length is a DWORD, so a negative value is a length that is larger than
				// int.MaxValue. There is no way to know where the next attribute begins.
				LogAttribute (TnefComplianceViolation.InvalidAttributeLength);
				return false;
			}

			return true;
		}

		// Gets the number of bytes of the value that need to be peeked at in order to validate the attribute.
		int GetPeekLength ()
		{
			switch (Tag) {
			case TnefAttributeTag.MessageClass:
			case TnefAttributeTag.OriginalMessageClass:
				return Length > 0 && Length <= MaxMessageClassLength ? Length : 0;
			case TnefAttributeTag.OemCodepage:
			case TnefAttributeTag.TnefVersion:
				return Length >= 4 ? 4 : 0;
			default:
				return 0;
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

		void CheckMessageClass (bool peeked)
		{
			if (Length <= 0 || Length > MaxMessageClassLength) {
				LogAttribute (TnefComplianceViolation.InvalidMessageClass);
				return;
			}

			// Note: If the stream ended before the value did, that is a truncated stream rather than an
			// invalid message class, and it will be reported as such when the value is consumed.
			if (!peeked)
				return;

			for (int i = 0; i < Length; i++) {
				byte c = input[inputIndex + i];

				// Note: a single nul-terminator is allowed at the end of the value.
				if (c == 0 && i + 1 == Length)
					break;

				if (c < 0x20 || c > 0x7e) {
					LogAttribute (TnefComplianceViolation.InvalidMessageClass);
					return;
				}
			}
		}

		void SetCodepage (int value)
		{
			if (value == codepage)
				return;

			try {
				encoding = CharsetUtils.GetEncoding (value);
				codepage = encoding.CodePage;
			} catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) {
				LogAttribute (TnefComplianceViolation.InvalidMessageCodepage);

				// Note: DefaultEncoding is windows-1252 if the host can provide it and iso-8859-1 if it
				// cannot, so this is always an encoding that can be used.
				encoding = DefaultEncoding;
				codepage = encoding.CodePage;
			}
		}

		void CheckAttributeTag (bool peeked)
		{
			switch (Tag) {
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
				// Note: attAttachRenderData marks the start of a new attachment.
				AttachMethod = TnefAttachMethod.ByValue;
				break;
			case TnefAttributeTag.MessageClass:
			case TnefAttributeTag.OriginalMessageClass:
				CheckMessageClass (peeked);
				break;
			case TnefAttributeTag.OemCodepage:
				if (Length < 4)
					LogAttribute (TnefComplianceViolation.InvalidAttributeValue);
				else if (peeked)
					SetCodepage (BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex, 4)));
				break;
			case TnefAttributeTag.TnefVersion:
				if (Length < 4)
					LogAttribute (TnefComplianceViolation.InvalidAttributeValue);
				else if (peeked && BinaryPrimitives.ReadInt32LittleEndian (input.AsSpan (inputIndex, 4)) != TnefVersion)
					LogAttribute (TnefComplianceViolation.UnsupportedVersion);
				break;
			default:
				LogAttribute (TnefComplianceViolation.UnknownAttribute);
				return;
			}

			// Note: attNull is allowed at either level, and an invalid level has already been reported.
			if (Tag == TnefAttributeTag.Null || (Level != TnefAttributeLevel.Message && Level != TnefAttributeLevel.Attachment))
				return;

			if (IsAttachmentLevelAttribute (Tag) != (Level == TnefAttributeLevel.Attachment))
				LogAttribute (TnefComplianceViolation.AttributeLevelMismatch);
		}

		bool ReadAttributeHeader (CancellationToken cancellationToken)
		{
			attributeOffset = LocalOffset;

			// Note: Reaching the end of the stream between attributes is the normal way for a TNEF stream to end.
			if (!Fill (1, cancellationToken))
				return false;

			if (!Fill (AttributeHeaderSize, cancellationToken)) {
				SetTruncated ();
				return false;
			}

			if (!ProcessAttributeHeader ())
				return false;

			int peek = GetPeekLength ();
			bool peeked = peek > 0 && Fill (peek, cancellationToken);

			CheckAttributeTag (peeked);

			return true;
		}

		bool ProcessChecksum ()
		{
			ushort expected = BinaryPrimitives.ReadUInt16LittleEndian (input.AsSpan (inputIndex, 2));

			inputIndex += 2;

			if (expected != checksum)
				LogAttribute (TnefComplianceViolation.AttributeChecksumMismatch);

			return true;
		}

		// Skips whatever remains of the current attribute's value and verifies its checksum.
		bool FinishAttribute (CancellationToken cancellationToken)
		{
			if (truncated)
				return false;

			long remaining = valueEnd - LocalOffset;

			if (remaining > 0 && !Skip (remaining, cancellationToken)) {
				SetTruncated ();
				return false;
			}

			if (!Fill (2, cancellationToken)) {
				SetTruncated ();
				return false;
			}

			return ProcessChecksum ();
		}

		bool BeginRead ()
		{
			CheckDisposed ();

			generation++;
			valueGeneration++;
			propertyReader = null;

			return state != ReaderState.Done;
		}

		bool EndRead (bool success)
		{
			if (!success) {
				state = ReaderState.Done;
				ResetAttribute ();
			}

			return success;
		}

		/// <summary>
		/// Advance to the next attribute.
		/// </summary>
		/// <remarks>
		/// <para>Advances the reader to the next attribute in the TNEF stream.</para>
		/// <para>The first call reads the TNEF stream header. If the stream does not begin with the TNEF signature,
		/// a <see cref="TnefComplianceViolation.InvalidSignature"/> issue is reported and the method returns
		/// <see langword="false"/>.</para>
		/// <para>Any part of the current attribute's value that has not been consumed is skipped, and the
		/// attribute's checksum is verified.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the reader was advanced to the next attribute; otherwise,
		/// <see langword="false"/> if there are no more attributes.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public bool Read (CancellationToken cancellationToken = default)
		{
			if (!BeginRead ())
				return false;

			if (state == ReaderState.Initial) {
				ResetComplianceBudget ();

				if (!ReadHeader (cancellationToken))
					return EndRead (false);
			} else if (!FinishAttribute (cancellationToken)) {
				return EndRead (false);
			}

			ResetAttribute ();

			return EndRead (ReadAttributeHeader (cancellationToken));
		}

		void CheckAttribute ()
		{
			CheckDisposed ();

			if (state != ReaderState.Attribute)
				throw new InvalidOperationException ("The reader is not positioned on an attribute.");
		}

		void ClaimValue (ValueClaim value)
		{
			CheckAttribute ();

			if (claim != ValueClaim.None && (claim != ValueClaim.Scalar || value != ValueClaim.Scalar))
				throw new InvalidOperationException ("The attribute value has already been read.");

			claim = value;
		}

		int GetScalarWidth (bool allowDate)
		{
			switch (AttributeType) {
			case TnefAttributeType.Short:
			case TnefAttributeType.Word:
				return 2;
			case TnefAttributeType.Long:
			case TnefAttributeType.DWord:
				return 4;
			case TnefAttributeType.Date:
				if (allowDate)
					return 14;
				break;
			}

			throw new InvalidOperationException (string.Format ("The {0} attribute cannot be read as the requested type.", Tag));
		}

		int GetScalarReadLength (int width)
		{
			return (int) Math.Min (Length, width);
		}

		// Called once the scalar value bytes have been buffered (or the stream has ended).
		void LoadScalar (int width, int count)
		{
			int n = Math.Min (inputEnd - inputIndex, count);

			TakeBytes (scalar, 0, n);
			scalarLength = n;

			if (n < count)
				SetTruncated ();

			if (Length < width) {
				LogAttribute (TnefComplianceViolation.InvalidAttributeValue);
			} else if (n == width && AttributeType == TnefAttributeType.Date) {
				dateValue = DecodeDate ();
			}
		}

		DateTime DecodeDate ()
		{
			// Note: The TNEF date structure is 7 16-bit values: year, month, day, hour, minute, second and
			// day-of-week.
			var span = scalar.AsSpan ();
			int year = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (0, 2));
			int month = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (2, 2));
			int day = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (4, 2));
			int hour = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (6, 2));
			int minute = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (8, 2));
			int second = BinaryPrimitives.ReadInt16LittleEndian (span.Slice (10, 2));

			try {
				return new DateTime (year, month, day, hour, minute, second);
			} catch (ArgumentOutOfRangeException) {
				LogAttribute (TnefComplianceViolation.InvalidDate);
				return default;
			}
		}

		int GetScalarInt32 (int width)
		{
			if (scalarLength < width)
				return 0;

			switch (AttributeType) {
			case TnefAttributeType.Short: return BinaryPrimitives.ReadInt16LittleEndian (scalar.AsSpan (0, 2));
			case TnefAttributeType.Word: return BinaryPrimitives.ReadUInt16LittleEndian (scalar.AsSpan (0, 2));
			default: return BinaryPrimitives.ReadInt32LittleEndian (scalar.AsSpan (0, 4));
			}
		}

		bool EnsureScalar (int width, CancellationToken cancellationToken)
		{
			ClaimValue (ValueClaim.Scalar);

			if (scalarLength >= 0)
				return false;

			int count = GetScalarReadLength (width);

			Fill (count, cancellationToken);
			LoadScalar (width, count);

			return true;
		}

		/// <summary>
		/// Read the current attribute's value as a 16-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current attribute's value as a 16-bit integer.</para>
		/// <para>The value may be read any number of times.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value cannot be read as a 16-bit integer.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been consumed in some other way.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public short ReadValueAsInt16 (CancellationToken cancellationToken = default)
		{
			return (short) ReadValueAsInt32 (cancellationToken);
		}

		/// <summary>
		/// Read the current attribute's value as a 32-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current attribute's value as a 32-bit integer.</para>
		/// <para>The value may be read any number of times.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value cannot be read as a 32-bit integer.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been consumed in some other way.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public int ReadValueAsInt32 (CancellationToken cancellationToken = default)
		{
			CheckAttribute ();

			int width = GetScalarWidth (false);

			EnsureScalar (width, cancellationToken);

			return GetScalarInt32 (width);
		}

		/// <summary>
		/// Read the current attribute's value as a date and time.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current attribute's value as a date and time.</para>
		/// <para>If the value is not a valid date, a <see cref="TnefComplianceViolation.InvalidDate"/> issue is
		/// reported and <c>default (DateTime)</c> is returned.</para>
		/// <para>The value may be read any number of times.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value cannot be read as a date.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been consumed in some other way.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public DateTime ReadValueAsDateTime (CancellationToken cancellationToken = default)
		{
			CheckDateAttribute ();
			EnsureScalar (14, cancellationToken);

			return dateValue;
		}

		void CheckDateAttribute ()
		{
			CheckAttribute ();

			if (AttributeType != TnefAttributeType.Date)
				throw new InvalidOperationException (string.Format ("The {0} attribute cannot be read as a date.", Tag));
		}

		void CheckStringAttribute ()
		{
			CheckAttribute ();

			switch (AttributeType) {
			case TnefAttributeType.Triples:
			case TnefAttributeType.String:
			case TnefAttributeType.Text:
			case TnefAttributeType.Byte:
				break;
			default:
				throw new InvalidOperationException (string.Format ("The {0} attribute cannot be read as a string.", Tag));
			}
		}

		/// <summary>
		/// Read the current attribute's value as a string.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current attribute's value as a string, decoded using the current <see cref="Codepage"/>.</para>
		/// <para>The value may only be read once.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value cannot be read as a string.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public string ReadValueAsString (CancellationToken cancellationToken = default)
		{
			CheckStringAttribute ();

			return DecodeString (encoding, ReadValueAsBytes (cancellationToken));
		}

		/// <summary>
		/// Read the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current attribute's raw value.</para>
		/// <para>The value may only be read once.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public byte[] ReadValueAsBytes (CancellationToken cancellationToken = default)
		{
			ClaimValue (ValueClaim.Raw);

			return ReadValueBytes ((int) ValueRemaining, TnefPropertyTag.Null, cancellationToken);
		}

		/// <summary>
		/// Open a stream for reading the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for reading the current attribute's raw value.</para>
		/// <para>The stream is only valid until the reader is advanced to the next attribute.</para>
		/// </remarks>
		/// <returns>The value stream.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been read.</para>
		/// </exception>
		public Stream OpenValueStream ()
		{
			ClaimValue (ValueClaim.Raw);

			return new TnefReaderStream (this, valueEnd, TnefPropertyTag.Null);
		}

		/// <summary>
		/// Get a reader for the MAPI properties contained within the current attribute.
		/// </summary>
		/// <remarks>
		/// <para>Gets a reader for the MAPI properties contained within the current attribute.</para>
		/// <para>Only the <see cref="TnefAttributeTag.MapiProperties"/>, <see cref="TnefAttributeTag.Attachment"/>
		/// and <see cref="TnefAttributeTag.RecipientTable"/> attributes contain MAPI properties.</para>
		/// <para>The property reader is only valid until the reader is advanced to the next attribute. Repeated
		/// calls for the same attribute return the same property reader.</para>
		/// </remarks>
		/// <returns>The property reader.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The reader has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The reader is not positioned on an attribute.</para>
		/// <para>-or-</para>
		/// <para>The current attribute does not contain MAPI properties.</para>
		/// <para>-or-</para>
		/// <para>The attribute's value has already been consumed in some other way.</para>
		/// </exception>
		public TnefPropertyReader GetPropertyReader ()
		{
			CheckAttribute ();

			switch (Tag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.RecipientTable:
				break;
			default:
				throw new InvalidOperationException (string.Format ("The {0} attribute does not contain MAPI properties.", Tag));
			}

			if (claim == ValueClaim.Properties)
				return propertyReader!;

			ClaimValue (ValueClaim.Properties);

			return propertyReader = new TnefPropertyReader (this, Tag == TnefAttributeTag.RecipientTable);
		}

		internal void CheckGeneration (int expected)
		{
			CheckDisposed ();

			if (generation != expected)
				throw new InvalidOperationException ("The reader has been advanced to another attribute.");
		}

		internal TnefReader CreateEmbeddedReader (Stream stream, long localOffset, TnefPropertyTag propertyTag)
		{
			var child = new TnefReader (this, stream, baseOffset + localOffset);

			if (child.depth > options.MaxNestingDepth) {
				Log (TnefComplianceViolation.NestingTooDeep, localOffset, propertyTag);
				child.state = ReaderState.Done;
			}

			return child;
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefReader"/> object.
		/// </summary>
		/// <remarks>
		/// Releases all resources used by the reader. Unless the reader was created with <c>leaveOpen</c>
		/// set to <see langword="true"/>, the underlying stream is also disposed.
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			propertyReader = null;

			if (!leaveOpen)
				stream.Dispose ();
		}
	}
}
