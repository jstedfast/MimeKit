//
// YEncReader.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// A streaming reader for single-part and multipart yEnc blocks.
	/// </summary>
	/// <remarks>
	/// <para>Call <see cref="ReadNext"/> and then <see cref="DecodeTo"/> for each block.
	/// To discard a block while still validating it, decode to <see cref="Stream.Null"/>.</para>
	/// <para>This reader does not assemble multipart files, interpret MIME, or undo NNTP dot-stuffing.
	/// Supply the article body after transport decoding. Do not access the underlying stream while reading:
	/// the reader buffers input beyond the current block.</para>
	/// <para>Content written to the destination is provisional until decoding completes successfully.
	/// A failed operation invalidates the reader. Instances are not thread-safe.</para>
	/// </remarks>
	public sealed class YEncReader : IDisposable
	{
		readonly byte[] input = new byte[4096];
		readonly YEncOptions options;
		readonly Stream stream;
		readonly bool leaveOpen;
		byte[] line;
		byte[] output = Array.Empty<byte> ();
		int inputIndex, inputLength, lineLength;
		bool pending, faulted, disposed, eof;

		/// <summary>
		/// Initialize a yEnc reader.
		/// </summary>
		/// <remarks>
		/// The options are copied; the source need not support seeking.
		/// </remarks>
		/// <param name="stream">The readable source.</param>
		/// <param name="options">The options, or <see langword="null"/> for defaults.</param>
		/// <param name="leaveOpen">Whether to leave the source open when disposed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The source is not readable.
		/// </exception>
		public YEncReader (Stream stream, YEncOptions? options = null, bool leaveOpen = false)
		{
			YEncFormat.ValidateStream (stream, false, nameof (stream));

			this.options = (options ?? new YEncOptions ()).Snapshot ();
			this.stream = stream;
			this.leaveOpen = leaveOpen;
			line = new byte[Math.Min (256, this.options.MaxLineLength + 2)];
		}

		/// <summary>
		/// Get the current block's header.
		/// </summary>
		/// <remarks>
		/// Available after a successful ReadNext; null before reading or after reaching end of input.
		/// </remarks>
		/// <value>The current header, or <see langword="null"/>.</value>
		public YEncHeader? Header { get; private set; }

		/// <summary>
		/// Get the number of decoded bytes in the current block.
		/// </summary>
		/// <remarks>
		/// This count does not imply successful validation.
		/// </remarks>
		/// <value>The decoded byte count.</value>
		public long DecodedSize { get; private set; }

		/// <summary>
		/// Get the computed, finalized CRC32 of the decoded block.
		/// </summary>
		/// <remarks>
		/// Only available after DecodeTo completes successfully.
		/// For multipart files this covers only the current part.
		/// </remarks>
		/// <value>The checksum, or <see langword="null"/>.</value>
		public uint? Checksum { get; private set; }

		/// <summary>
		/// Get the whole-file checksum advertised by the trailer.
		/// </summary>
		/// <remarks>
		/// Available after successful decoding, if present. For a partial byte range this value cannot
		/// be verified without assembling the entire file and is returned as unverified metadata.
		/// </remarks>
		/// <value>The advertised checksum, or <see langword="null"/>.</value>
		public uint? ExpectedChecksum { get; private set; }

		/// <summary>
		/// Get the part checksum advertised by the trailer.
		/// </summary>
		/// <remarks>
		/// Available after successful decoding, if present, and verified against the decoded bytes.
		/// </remarks>
		/// <value>The advertised part checksum, or <see langword="null"/>.</value>
		public uint? ExpectedPartChecksum { get; private set; }

		void CheckState (CancellationToken cancellationToken)
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (YEncReader));

			if (faulted)
				throw new InvalidOperationException ("The yEnc reader cannot be reused after a failed operation.");

			cancellationToken.ThrowIfCancellationRequested ();
		}

		void EnsureLineCapacity (int needed)
		{
			if (needed > line.Length)
				Array.Resize (ref line, (int) Math.Min (Math.Max ((long) line.Length * 2, needed), options.MaxLineLength + 2L));
		}

		bool ScanLine ()
		{
			while (inputIndex < inputLength) {
				byte c = input[inputIndex++];

				if (c == (byte) '\n') {
					FinishLine ();
					return true;
				}

				if (lineLength == options.MaxLineLength && c != (byte) '\r' || lineLength > options.MaxLineLength)
					throw new FormatException ("The yEnc input exceeds the maximum line length.");

				EnsureLineCapacity (lineLength + 1);
				line[lineLength++] = c;
			}

			return false;
		}

		void FinishLine ()
		{
			if (lineLength > 0 && line[lineLength - 1] == (byte) '\r')
				lineLength--;

			if (lineLength > options.MaxLineLength)
				throw new FormatException ("The yEnc input exceeds the maximum line length.");
		}

		bool ReadLine (CancellationToken cancellationToken)
		{
			lineLength = 0;

			while (true) {
				cancellationToken.ThrowIfCancellationRequested ();

				if (ScanLine ())
					return true;

				if (eof) {
					FinishLine ();
					return lineLength > 0;
				}

				inputLength = YEncFormat.Read (stream, input, input.Length, cancellationToken);
				inputIndex = 0;
				eof = inputLength == 0;
			}
		}

		async Task<bool> ReadLineAsync (CancellationToken cancellationToken)
		{
			lineLength = 0;

			while (true) {
				cancellationToken.ThrowIfCancellationRequested ();

				if (ScanLine ())
					return true;

				if (eof) {
					FinishLine ();
					return lineLength > 0;
				}

				inputLength = await stream.ReadAsync (input, 0, input.Length, cancellationToken).ConfigureAwait (false);
				inputIndex = 0;
				eof = inputLength == 0;
			}
		}

		bool IsMarker (ReadOnlySpan<byte> marker)
		{
			return YEncFormat.StartsWith (line, lineLength, marker);
		}

		Dictionary<string, string> ParseFields (int start, bool isHeader = false)
		{
			return YEncFormat.ParseFields (CharsetUtils.Latin1.GetString (line, 0, lineLength), start, isHeader);
		}

		void SetHeader (Dictionary<string, string> fields, Dictionary<string, string>? part)
		{
			if (!fields.TryGetValue ("name", out var name))
				throw new FormatException ("Missing yEnc filename.");

			long size = YEncFormat.Number (fields, "size");
			int length = YEncFormat.Integer (fields, "line");
			int number = YEncFormat.Integer (fields, "part", false);
			int total = YEncFormat.Integer (fields, "total", false);
			long begin = part != null ? YEncFormat.Number (part, "begin") : 0;
			long end = part != null ? YEncFormat.Number (part, "end") : 0;

			if (fields.ContainsKey ("part") && number == 0)
				throw new FormatException ("Invalid yEnc part number.");

			try {
				var bytes = CharsetUtils.Latin1.GetBytes (name);

				name = options.FileNameEncoding.GetString (bytes, 0, bytes.Length);
				Header = new YEncHeader (name, size, length, number, total, begin, end);
			} catch (ArgumentException ex) {
				throw new FormatException ("Invalid yEnc header.", ex);
			}

			if (Header.DecodedSize > options.MaxDecodedSize || Header.LineLength > options.MaxLineLength)
				throw new FormatException ("The yEnc header exceeds the configured limits.");

			pending = true;
		}

		void BeginRead (CancellationToken cancellationToken)
		{
			CheckState (cancellationToken);

			if (pending)
				throw new InvalidOperationException ("Decode the current yEnc block before reading another.");

			Header = null;
			DecodedSize = 0;
			Checksum = ExpectedChecksum = ExpectedPartChecksum = null;
		}

		/// <summary>
		/// Read the next yEnc header.
		/// </summary>
		/// <remarks>
		/// Skips surrounding text. Decode the current block before calling again.
		/// </remarks>
		/// <returns>True if a block was found; false at end of input.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="FormatException">
		/// A header is malformed, incomplete, or exceeds a configured limit.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The current block has not been decoded, or the reader has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The reader is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public bool ReadNext (CancellationToken cancellationToken = default)
		{
			BeginRead (cancellationToken);

			bool success = false;

			try {
				while (ReadLine (cancellationToken)) {
					if (!IsMarker ("=ybegin "u8))
						continue;

					var fields = ParseFields (8, true);
					Dictionary<string, string>? part = null;

					if (fields.ContainsKey ("part")) {
						if (!ReadLine (cancellationToken) || !IsMarker ("=ypart "u8))
							throw new FormatException ("Missing yEnc part header.");

						part = ParseFields (7);
					}

					SetHeader (fields, part);
					success = true;

					return true;
				}

				success = true;
				return false;
			} finally {
				if (!success)
					faulted = true;
			}
		}

		/// <summary>
		/// Asynchronously read the next yEnc header.
		/// </summary>
		/// <remarks>
		/// Skips surrounding text. Decode the current block before calling again.
		/// </remarks>
		/// <returns>True if a block was found; false at end of input.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="FormatException">
		/// A header is malformed, incomplete, or exceeds a configured limit.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The current block has not been decoded, or the reader has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The reader is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task<bool> ReadNextAsync (CancellationToken cancellationToken = default)
		{
			BeginRead (cancellationToken);

			bool success = false;

			try {
				while (await ReadLineAsync (cancellationToken).ConfigureAwait (false)) {
					if (!IsMarker ("=ybegin "u8))
						continue;

					var fields = ParseFields (8, true);
					Dictionary<string, string>? part = null;

					if (fields.ContainsKey ("part")) {
						if (!await ReadLineAsync (cancellationToken).ConfigureAwait (false) || !IsMarker ("=ypart "u8))
							throw new FormatException ("Missing yEnc part header.");

						part = ParseFields (7);
					}

					SetHeader (fields, part);
					success = true;

					return true;
				}

				success = true;
				return false;
			} finally {
				if (!success)
					faulted = true;
			}
		}

		int DecodeLine (YEncDecoder decoder)
		{
			var header = Header!;

			if (IsMarker ("=y"u8))
				throw new FormatException ("Unexpected yEnc control line.");

			if (lineLength > header.LineLength + 1)
				throw new FormatException ("The yEnc payload exceeds its declared line length.");

			bool finalEscape = false;

			for (int i = 0; i < lineLength; i++) {
				if (line[i] == 0 || line[i] == (byte) '\r')
					throw new FormatException ("Unescaped critical byte in yEnc payload.");

				if (line[i] == (byte) '=') {
					finalEscape = i == lineLength - 2;

					if (++i == lineLength || line[i] == 0 || line[i] == (byte) '\r')
						throw new FormatException ("Invalid or incomplete yEnc escape.");
				}
			}

			if (lineLength > header.LineLength && !finalEscape)
				throw new FormatException ("The yEnc payload exceeds its declared line length.");

			EnsureLineCapacity (lineLength + 1);
			line[lineLength] = (byte) '\n';

			if (output.Length < lineLength + 1)
				output = new byte[lineLength + 1];

			int count = decoder.Decode (line, 0, lineLength + 1, output);

			if (count > header.DecodedSize - DecodedSize)
				throw new FormatException ("The yEnc payload exceeds its declared size.");

			DecodedSize += count;

			return count;
		}

		void ValidateTrailer (YEncDecoder decoder)
		{
			var fields = ParseFields (6);
			var header = Header!;

			if (YEncFormat.Number (fields, "size") != DecodedSize || DecodedSize != header.DecodedSize)
				throw new FormatException ("The yEnc decoded size does not match its header or trailer.");

			if ((header.PartNumber != 0 || fields.ContainsKey ("part")) && YEncFormat.Integer (fields, "part") != header.PartNumber)
				throw new FormatException ("The yEnc trailer part number does not match its header.");

			if (fields.ContainsKey ("total") && YEncFormat.Integer (fields, "total") != header.TotalParts)
				throw new FormatException ("The yEnc trailer part count does not match its header.");

			uint actual = unchecked ((uint) decoder.Checksum) ^ 0xffffffff;
			uint? expected = YEncFormat.Checksum (fields, "crc32");
			uint? part = YEncFormat.Checksum (fields, "pcrc32");
			bool wholeFile = header.PartNumber == 0 || (header.Begin == 1 && header.End == header.Size);

			if (part.HasValue && part.Value != actual || wholeFile && expected.HasValue && expected.Value != actual)
				throw new FormatException ("The yEnc checksum does not match the decoded content.");

			Checksum = actual;
			ExpectedChecksum = expected;
			ExpectedPartChecksum = part;
			pending = false;
		}

		void BeginDecode (Stream destination, CancellationToken cancellationToken)
		{
			YEncFormat.ValidateStream (destination, true, nameof (destination));

			if (ReferenceEquals (destination, stream))
				throw new ArgumentException ("The destination must differ from the source.", nameof (destination));

			CheckState (cancellationToken);

			if (!pending)
				throw new InvalidOperationException ("Read a yEnc header before decoding a block.");
		}

		/// <summary>
		/// Decode and validate the current block.
		/// </summary>
		/// <remarks>
		/// Leaves the destination open. It may contain partial or corrupt output if the operation fails;
		/// callers must discard that output. Absent checksums are accepted; present checksums are verified
		/// when they cover this block. Whole-file checksums of partial ranges remain unverified metadata.
		/// </remarks>
		/// <param name="destination">The writable destination.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="destination"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The destination is not writable or is the source stream.
		/// </exception>
		/// <exception cref="FormatException">
		/// The block is malformed, truncated, or fails size or checksum validation.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// There is no pending block or the reader has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The reader is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public void DecodeTo (Stream destination, CancellationToken cancellationToken = default)
		{
			BeginDecode (destination, cancellationToken);

			bool success = false;

			try {
				var decoder = new YEncDecoder (true);

				while (ReadLine (cancellationToken)) {
					if (IsMarker ("=yend "u8)) {
						ValidateTrailer (decoder);
						success = true;
						return;
					}

					int count = DecodeLine (decoder);

					YEncFormat.Write (destination, output, count, cancellationToken);
				}

				throw new FormatException ("Missing yEnc trailer.");
			} finally {
				if (!success)
					faulted = true;
			}
		}

		/// <summary>
		/// Asynchronously decode and validate the current block.
		/// </summary>
		/// <remarks>
		/// Leaves the destination open. Discard its output if the operation fails.
		/// Absent checksums are accepted; whole-file checksums of partial ranges remain unverified metadata.
		/// </remarks>
		/// <param name="destination">The writable destination.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="destination"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The destination is not writable or is the source stream.
		/// </exception>
		/// <exception cref="FormatException">
		/// The block is malformed, truncated, or fails size or checksum validation.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// There is no pending block or the reader has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The reader is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task DecodeToAsync (Stream destination, CancellationToken cancellationToken = default)
		{
			BeginDecode (destination, cancellationToken);

			bool success = false;

			try {
				var decoder = new YEncDecoder (true);

				while (await ReadLineAsync (cancellationToken).ConfigureAwait (false)) {
					if (IsMarker ("=yend "u8)) {
						ValidateTrailer (decoder);
						success = true;
						return;
					}

					int count = DecodeLine (decoder);

					await destination.WriteAsync (output, 0, count, cancellationToken).ConfigureAwait (false);
				}

				throw new FormatException ("Missing yEnc trailer.");
			} finally {
				if (!success)
					faulted = true;
			}
		}

		/// <summary>
		/// Dispose the reader.
		/// </summary>
		/// <remarks>
		/// Closes the source unless leaveOpen was specified.
		/// Does not decode or validate a pending block.
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;

			if (!leaveOpen)
				stream.Dispose ();
		}
	}
}
