//
// RtfCompressedWriteStream.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;

using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A write-only stream that encodes RTF as compressed RTF.
	/// </summary>
	/// <remarks>
	/// <para>Encodes RTF using the LZFu compression ("compressed") or the uncompressed format defined by [MS-OXRTFCP]
	/// and appends the result to a seekable output stream. The 16-byte header is written when the stream is
	/// disposed.</para>
	/// <para>The encoder uses a dictionary that is identical to the dictionary maintained by the decoder
	/// ([MS-OXRTFCP] section 2.1.3.1.3) and finds the longest match (of at most 17 bytes) by following hash chains
	/// of the dictionary positions that begin with the same 2 bytes.</para>
	/// </remarks>
	sealed class RtfCompressedWriteStream : Stream
	{
		const int HeaderSize = 16;
		const int DictionarySize = 4096;
		const int DictionaryMask = DictionarySize - 1;
		const int MinMatchLength = 2;
		const int MaxMatchLength = 17;
		const int MaxChainLength = 64;

		readonly Action<RtfCompressedWriteStream, string?>? closed;
		readonly byte[] group = new byte[1 + 8 * 2];
		readonly Crc32 crc32 = new Crc32 ();
		readonly Stream output;
		readonly long headerOffset;
		readonly bool compress;

		// The dictionary and its hash chains.
		readonly byte[] dict = new byte[DictionarySize];
		readonly short[] head = new short[DictionarySize];
		readonly short[] prev = new short[DictionarySize];
		int writeOffset;
		bool full;

		// The input that has not yet been encoded.
		byte[] input = new byte[4096];
		int inputIndex, inputEnd;

		// The current group of up to 8 tokens.
		int groupLength = 1, tokenCount;

		long compressedSize, rawSize;
		bool disposed;

		public RtfCompressedWriteStream (Stream output, bool compress, Action<RtfCompressedWriteStream, string?>? closed)
		{
			this.compress = compress;
			this.closed = closed;
			this.output = output;

			headerOffset = output.Position;
			output.Write (new byte[HeaderSize], 0, HeaderSize);

			if (compress) {
				var initializer = RtfCompressedToRtf.DictionaryInitializer;

				initializer.CopyTo (dict);
				writeOffset = initializer.Length;

				head.AsSpan ().Fill (-1);
				prev.AsSpan ().Fill (-1);

				// Note: Every position within the initializer that is followed by another initializer byte can begin a match.
				for (int i = 0; i < writeOffset - 1; i++)
					Insert (i);
			}
		}

		public override bool CanRead {
			get { return false; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return !disposed; }
		}

		public override long Length {
			get { return rawSize; }
		}

		public override long Position {
			get { return rawSize; }
			set { throw new NotSupportedException (); }
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (RtfCompressedWriteStream));
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		static int Hash (byte a, byte b)
		{
			return ((a << 4) ^ b) & DictionaryMask;
		}

		// Adds the dictionary position to the hash chain of the 2 bytes that begin at that position.
		void Insert (int position)
		{
			int hash = Hash (dict[position], dict[(position + 1) & DictionaryMask]);

			prev[position] = head[hash];
			head[hash] = (short) position;
		}

		// Adds a byte to the dictionary exactly as the decoder does.
		void AddToDictionary (byte value)
		{
			dict[writeOffset] = value;

			// Note: Now that the byte that follows it is known, the previous position can begin a match.
			Insert ((writeOffset - 1) & DictionaryMask);

			writeOffset = (writeOffset + 1) & DictionaryMask;

			if (writeOffset == 0)
				full = true;
		}

		// Gets the length of the match between the dictionary at the specified offset and the input. When the
		// decoder copies a match, the bytes that it copies are added to the dictionary one at a time, so a match
		// may overlap the bytes that it produces.
		int GetMatchLength (int offset, int maxLength)
		{
			int length = 0;

			while (length < maxLength) {
				int position = (offset + length) & DictionaryMask;
				int distance = (position - writeOffset) & DictionaryMask;
				byte value = distance < length ? input[inputIndex + distance] : dict[position];

				if (value != input[inputIndex + length])
					break;

				length++;
			}

			return length;
		}

		void FindMatch (int available, out int matchOffset, out int matchLength)
		{
			int maxLength = Math.Min (available, MaxMatchLength);
			int candidate = head[Hash (input[inputIndex], input[inputIndex + 1])];
			int chain = MaxChainLength;

			matchOffset = 0;
			matchLength = 0;

			// Note: [MS-OXRTFCP] 2.3.3.2.1 scans the dictionary from its oldest position and keeps the first of the
			// longest matches, so prefer the oldest candidate when matches have the same length.
			int start = full ? (writeOffset + 1) & DictionaryMask : 0;
			int matchAge = int.MaxValue;

			while (candidate >= 0 && chain-- > 0) {
				// Note: A reference to the current write offset marks the end of the stream.
				if (candidate != writeOffset) {
					int length = GetMatchLength (candidate, maxLength);
					int age = (candidate - start) & DictionaryMask;

					if (length > matchLength || (length == matchLength && length > 0 && age < matchAge)) {
						matchOffset = candidate;
						matchLength = length;
						matchAge = age;
					}
				}

				candidate = prev[candidate];
			}
		}

		void WriteGroup ()
		{
			crc32.Update (group, 0, groupLength);
			output.Write (group, 0, groupLength);
			compressedSize += groupLength;
			groupLength = 1;
			tokenCount = 0;
			group[0] = 0;
		}

		void WriteToken (bool reference, byte value0, byte value1)
		{
			if (reference) {
				group[0] |= (byte) (1 << tokenCount);
				group[groupLength++] = value0;
				group[groupLength++] = value1;
			} else {
				group[groupLength++] = value0;
			}

			if (++tokenCount == 8)
				WriteGroup ();
		}

		void WriteReference (int offset, int length)
		{
			// Note: A dictionary reference is a 12-bit offset followed by a 4-bit length (minus 2), big-endian.
			WriteToken (true, (byte) (offset >> 4), (byte) (((offset & 0x0F) << 4) | (length - MinMatchLength)));
		}

		// Encodes the input, leaving at least MaxMatchLength bytes unencoded unless the input is complete.
		void Encode (bool final)
		{
			int minimum = final ? 0 : MaxMatchLength;

			while (inputEnd - inputIndex > minimum) {
				int available = inputEnd - inputIndex;
				int matchOffset = 0, matchLength = 0;

				if (available >= MinMatchLength)
					FindMatch (available, out matchOffset, out matchLength);

				if (matchLength >= MinMatchLength) {
					WriteReference (matchOffset, matchLength);

					for (int i = 0; i < matchLength; i++)
						AddToDictionary (input[inputIndex++]);
				} else {
					byte value = input[inputIndex++];

					WriteToken (false, value, 0);
					AddToDictionary (value);
				}
			}

			// Note: Move the remaining input to the start of the buffer.
			int remaining = inputEnd - inputIndex;

			if (remaining > 0 && inputIndex > 0)
				Buffer.BlockCopy (input, inputIndex, input, 0, remaining);

			inputIndex = 0;
			inputEnd = remaining;
		}

		static void ValidateArguments (byte[] buffer, int offset, int count)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException (nameof (count));
		}
		public override void Write (byte[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);
			CheckDisposed ();

			if (!compress) {
				output.Write (buffer, offset, count);
				compressedSize += count;
				rawSize += count;
				return;
			}

			rawSize += count;

			while (count > 0) {
				int n = Math.Min (count, input.Length - inputEnd);

				Buffer.BlockCopy (buffer, offset, input, inputEnd, n);
				inputEnd += n;
				offset += n;
				count -= n;

				if (inputEnd == input.Length)
					Encode (false);
			}
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			// Note: The output is an in-memory buffer, so there is nothing to wait for.
			Write (buffer, offset, count);

			return Task.CompletedTask;
		}

		public override void Flush ()
		{
			CheckDisposed ();
		}

		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();
			CheckDisposed ();

			return Task.CompletedTask;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		void Complete ()
		{
			if (compress) {
				Encode (true);

				// Note: [MS-OXRTFCP] 2.1.3.1.4: The end of the stream is marked by a reference to the dictionary's
				// current write offset.
				WriteReference (writeOffset, MinMatchLength);

				if (tokenCount > 0)
					WriteGroup ();
			}

			if (compressedSize > int.MaxValue - 12 || rawSize > int.MaxValue)
				throw new InvalidOperationException ("The compressed RTF is too large.");

			var header = new byte[HeaderSize];
			var span = header.AsSpan ();

			// Note: COMPSIZE includes the 12 bytes of the header that follow it.
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (0, 4), (int) compressedSize + 12);
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (4, 4), (int) rawSize);
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (8, 4), (int) (compress ? RtfCompressionMode.Compressed : RtfCompressionMode.Uncompressed));
			BinaryPrimitives.WriteInt32LittleEndian (span.Slice (12, 4), compress ? crc32.Checksum : 0);

			long end = output.Position;

			output.Position = headerOffset;
			output.Write (header, 0, HeaderSize);
			output.Position = end;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;

				string? error = null;

				// Note: Dispose must not throw, so errors are reported to the owner.
				try {
					Complete ();
				} catch (Exception ex) {
					error = ex.Message;
				}

				input = Array.Empty<byte> ();
				closed?.Invoke (this, error);
			}

			base.Dispose (disposing);
		}
	}
}
