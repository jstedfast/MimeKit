//
// YEncWriter.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Encodings {
	/// <summary>
	/// A streaming writer for single-part and multipart yEnc blocks.
	/// </summary>
	/// <remarks>
	/// <para>Writes complete blocks without buffering an entire file. Each source must contain exactly
	/// the declared block size, starting at its current position. Multipart sources contain only the
	/// indicated range; this writer does not seek into a whole-file source or assemble parts.</para>
	/// <para>A failed write may leave incomplete output and invalidates the writer.
	/// Instances are not thread-safe. This writer does not generate MIME headers or NNTP transport framing.</para>
	/// </remarks>
	public sealed class YEncWriter : IDisposable
	{
		static readonly byte[] IllegalFilenameCharacters = new byte[] { 0, 13, 10 };
		readonly YEncOptions options;
		readonly Stream stream;
		readonly bool leaveOpen;
		readonly byte[] newLine;
		bool disposed, faulted;

		/// <summary>
		/// Initialize a yEnc writer.
		/// </summary>
		/// <remarks>
		/// The options are copied; the destination need not support seeking.
		/// </remarks>
		/// <param name="stream">The writable destination.</param>
		/// <param name="options">The options, or <see langword="null"/> for defaults.</param>
		/// <param name="leaveOpen">Whether to leave the destination open when disposed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="stream"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The destination is not writable.
		/// </exception>
		public YEncWriter (Stream stream, YEncOptions? options = null, bool leaveOpen = false)
		{
			YEncFormat.ValidateStream (stream, true, nameof (stream));

			this.options = (options ?? new YEncOptions ()).Snapshot ();
			this.stream = stream;
			this.leaveOpen = leaveOpen;

			newLine = this.options.NewLineFormat == NewLineFormat.Dos ? new byte[] { 13, 10 } : new byte[] { 10 };
		}

		byte[] ControlLine (string text)
		{
			if (text.Length > options.MaxLineLength)
				throw new ArgumentException ("The yEnc control line exceeds the configured maximum length.");

			var bytes = new byte[text.Length + newLine.Length];

			Encoding.ASCII.GetBytes (text, 0, text.Length, bytes, 0);
			Buffer.BlockCopy (newLine, 0, bytes, text.Length, newLine.Length);

			return bytes;
		}

		byte[] CreateHeader (YEncHeader header, Stream source, uint? fileChecksum, CancellationToken cancellationToken)
		{
			if (header is null)
				throw new ArgumentNullException (nameof (header));

			YEncFormat.ValidateStream (source, false, nameof (source));

			if (ReferenceEquals (source, stream))
				throw new ArgumentException ("The source must differ from the destination.", nameof (source));

			if (disposed)
				throw new ObjectDisposedException (nameof (YEncWriter));

			if (faulted)
				throw new InvalidOperationException ("The yEnc writer cannot be reused after a failed write.");

			if (header.LineLength < 60 || header.LineLength > 998 || header.LineLength + 1 > options.MaxLineLength || header.DecodedSize > options.MaxDecodedSize)
				throw new ArgumentOutOfRangeException (nameof (header), "The block size or line length is outside the supported limits.");

			cancellationToken.ThrowIfCancellationRequested ();

			if (GetTrailerText (header, 0, fileChecksum).Length > options.MaxLineLength)
				throw new ArgumentException ("The yEnc trailer exceeds the configured maximum line length.", nameof (header));

			var prefix = new StringBuilder ("=ybegin ");

			if (header.PartNumber != 0) {
				prefix.Append ("part=").Append (header.PartNumber.ToString (CultureInfo.InvariantCulture)).Append (' ');

				if (header.TotalParts != 0)
					prefix.Append ("total=").Append (header.TotalParts.ToString (CultureInfo.InvariantCulture)).Append (' ');
			}

			prefix.Append ("line=").Append (header.LineLength.ToString (CultureInfo.InvariantCulture));
			prefix.Append (" size=").Append (header.Size.ToString (CultureInfo.InvariantCulture)).Append (" name=");

			int nameLength = options.FileNameEncoding.GetByteCount (header.FileName);

			if (nameLength > options.MaxLineLength - prefix.Length)
				throw new ArgumentException ("The yEnc header exceeds the configured maximum line length.", nameof (header));

			var name = options.FileNameEncoding.GetBytes (header.FileName);

			if (name.AsSpan ().IndexOfAny (IllegalFilenameCharacters) != -1)
				throw new ArgumentException ("The encoded filename contains NUL, CR or LF.", nameof (header));

			byte[] part = header.PartNumber == 0 ? Array.Empty<byte> () : ControlLine (
				"=ypart begin=" + header.Begin.ToString (CultureInfo.InvariantCulture) + " end=" + header.End.ToString (CultureInfo.InvariantCulture));

			long length = (long) prefix.Length + name.Length + newLine.Length + part.Length;

			if (length > int.MaxValue)
				throw new ArgumentException ("The combined yEnc headers are too long.", nameof (header));

			var bytes = new byte[(int) length];
			var text = prefix.ToString ();

			Encoding.ASCII.GetBytes (text, 0, text.Length, bytes, 0);
			Buffer.BlockCopy (name, 0, bytes, text.Length, name.Length);
			Buffer.BlockCopy (newLine, 0, bytes, text.Length + name.Length, newLine.Length);
			Buffer.BlockCopy (part, 0, bytes, text.Length + name.Length + newLine.Length, part.Length);

			return bytes;
		}

		static string GetTrailerText (YEncHeader header, uint checksum, uint? fileChecksum)
		{
			string text = "=yend size=" + header.DecodedSize.ToString (CultureInfo.InvariantCulture);

			if (header.PartNumber != 0) {
				text += " part=" + header.PartNumber.ToString (CultureInfo.InvariantCulture);

				if (header.TotalParts != 0)
					text += " total=" + header.TotalParts.ToString (CultureInfo.InvariantCulture);

				text += " pcrc32=" + checksum.ToString ("x8", CultureInfo.InvariantCulture);

				if (fileChecksum.HasValue)
					text += " crc32=" + fileChecksum.Value.ToString ("x8", CultureInfo.InvariantCulture);
			} else {
				text += " crc32=" + checksum.ToString ("x8", CultureInfo.InvariantCulture);
			}

			return text;
		}

		byte[] CreateTrailer (YEncHeader header, uint checksum, uint? fileChecksum)
		{
			if (fileChecksum.HasValue && (header.PartNumber == 0 || (header.Begin == 1 && header.End == header.Size)) && fileChecksum.Value != checksum)
				throw new FormatException ("The supplied whole-file checksum does not match the source.");

			return ControlLine (GetTrailerText (header, checksum, fileChecksum));
		}

		/// <summary>
		/// Write a complete yEnc block.
		/// </summary>
		/// <remarks>
		/// Writes framing, payload and a checksum trailer. Leaves the source open and does not flush the
		/// destination. The source must end at exactly the declared decoded size; size errors invalidate
		/// the output. A supplied whole-file checksum for a partial range is emitted without verification.
		/// </remarks>
		/// <returns>The finalized CRC32 of this block.</returns>
		/// <param name="header">The block metadata.</param>
		/// <param name="source">The source of this block's decoded bytes.</param>
		/// <param name="fileChecksum">An optional finalized whole-file CRC32.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="header"/> or <paramref name="source"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A stream, filename encoding or header is invalid.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The header exceeds the configured limits or the writer's line-length range of 60 through 998.
		/// </exception>
		/// <exception cref="FormatException">
		/// The source size or supplied checksum does not match.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The writer has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The writer is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public uint Write (YEncHeader header, Stream source, uint? fileChecksum = null, CancellationToken cancellationToken = default)
		{
			var begin = CreateHeader (header, source, fileChecksum, cancellationToken);
			bool success = false;

			try {
				var encoder = new YEncEncoder (options.NewLineFormat, header.LineLength);
				var input = new byte[4096];
				var output = new byte[encoder.EstimateOutputLength (input.Length)];
				long remaining = header.DecodedSize;

				YEncFormat.Write (stream, begin, begin.Length, cancellationToken);

				while (true) {
					cancellationToken.ThrowIfCancellationRequested ();

					int count = remaining >= input.Length ? input.Length : (int) remaining + 1;
					int n = YEncFormat.Read (source, input, count, cancellationToken);

					if (n == 0) {
						if (remaining != 0)
							throw new FormatException ("The yEnc source is shorter than its declared size.");

						break;
					}

					if (n > remaining)
						throw new FormatException ("The yEnc source is longer than its declared size.");

					remaining -= n;
					n = encoder.Encode (input, 0, n, output);

					YEncFormat.Write (stream, output, n, cancellationToken);
				}

				int flushed = encoder.Flush (Array.Empty<byte> (), 0, 0, output);

				YEncFormat.Write (stream, output, flushed, cancellationToken);

				uint checksum = unchecked ((uint) encoder.Checksum) ^ 0xffffffff;
				var end = CreateTrailer (header, checksum, fileChecksum);

				YEncFormat.Write (stream, end, end.Length, cancellationToken);

				success = true;

				return checksum;
			} finally {
				if (!success)
					faulted = true;
			}
		}

		/// <summary>
		/// Asynchronously write a complete yEnc block.
		/// </summary>
		/// <remarks>
		/// Leaves the source open and does not flush the destination. The source must end at exactly the
		/// declared decoded size. A supplied whole-file checksum for a partial range is emitted
		/// without verification.
		/// </remarks>
		/// <returns>The finalized CRC32 of this block.</returns>
		/// <param name="header">The block metadata.</param>
		/// <param name="source">The source of this block's decoded bytes.</param>
		/// <param name="fileChecksum">An optional finalized whole-file CRC32.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="header"/> or <paramref name="source"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A stream, filename encoding or header is invalid.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The header exceeds the configured limits or the writer's line-length range of 60 through 998.
		/// </exception>
		/// <exception cref="FormatException">
		/// The source size or supplied checksum does not match.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The writer has faulted.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// The writer is disposed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task<uint> WriteAsync (YEncHeader header, Stream source, uint? fileChecksum = null, CancellationToken cancellationToken = default)
		{
			var begin = CreateHeader (header, source, fileChecksum, cancellationToken);
			bool success = false;

			try {
				var encoder = new YEncEncoder (options.NewLineFormat, header.LineLength);
				var input = new byte[4096];
				var output = new byte[encoder.EstimateOutputLength (input.Length)];
				long remaining = header.DecodedSize;

				await stream.WriteAsync (begin, 0, begin.Length, cancellationToken).ConfigureAwait (false);

				while (true) {
					cancellationToken.ThrowIfCancellationRequested ();

					int count = remaining >= input.Length ? input.Length : (int) remaining + 1;
					int n = await source.ReadAsync (input, 0, count, cancellationToken).ConfigureAwait (false);

					if (n == 0) {
						if (remaining != 0)
							throw new FormatException ("The yEnc source is shorter than its declared size.");

						break;
					}

					if (n > remaining)
						throw new FormatException ("The yEnc source is longer than its declared size.");

					remaining -= n;
					n = encoder.Encode (input, 0, n, output);

					await stream.WriteAsync (output, 0, n, cancellationToken).ConfigureAwait (false);
				}

				int flushed = encoder.Flush (Array.Empty<byte> (), 0, 0, output);

				await stream.WriteAsync (output, 0, flushed, cancellationToken).ConfigureAwait (false);

				uint checksum = unchecked ((uint) encoder.Checksum) ^ 0xffffffff;
				var end = CreateTrailer (header, checksum, fileChecksum);

				await stream.WriteAsync (end, 0, end.Length, cancellationToken).ConfigureAwait (false);

				success = true;

				return checksum;
			} finally {
				if (!success)
					faulted = true;
			}
		}

		/// <summary>
		/// Dispose the writer.
		/// </summary>
		/// <remarks>
		/// Closes the destination unless leaveOpen was specified.
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
