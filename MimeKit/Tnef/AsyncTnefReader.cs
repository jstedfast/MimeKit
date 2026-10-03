//
// AsyncTnefReader.cs
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
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MimeKit.Tnef {
	public sealed partial class TnefReader
	{
		Task<int> ReadStreamAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return stream.ReadAsync (buffer, offset, count, cancellationToken);
		}

		internal async Task<bool> FillAsync (int count, CancellationToken cancellationToken)
		{
			if (inputEnd - inputIndex >= count)
				return true;

			CompactBuffer ();

			while (inputEnd < count) {
				if (eos)
					return false;

				int nread = await ReadStreamAsync (input, inputEnd, input.Length - inputEnd, cancellationToken).ConfigureAwait (false);

				if (nread <= 0) {
					eos = true;
					return false;
				}

				inputEnd += nread;
				position += nread;
			}

			return true;
		}

		internal async Task<bool> SkipAsync (long count, CancellationToken cancellationToken)
		{
			while (count > 0) {
				if (inputIndex == inputEnd && !await FillAsync (1, cancellationToken).ConfigureAwait (false))
					return false;

				int n = (int) Math.Min (inputEnd - inputIndex, count);

				UpdateChecksum (input, inputIndex, n);
				inputIndex += n;
				count -= n;
			}

			return true;
		}

		internal async Task<int> ReadValueDataAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (count == 0)
				return 0;

			int n;

			if (inputIndex == inputEnd) {
				if (eos)
					return 0;

				if (count >= BufferSize) {
					// Note: Large reads bypass the buffer and go straight into the caller's buffer.
					if ((n = await ReadStreamAsync (buffer, offset, count, cancellationToken).ConfigureAwait (false)) <= 0) {
						eos = true;
						return 0;
					}

					position += n;
					UpdateChecksum (buffer, offset, n);

					return n;
				}

				if (!await FillAsync (1, cancellationToken).ConfigureAwait (false) && inputIndex == inputEnd)
					return 0;
			}

			n = Math.Min (inputEnd - inputIndex, count);
			TakeBytes (buffer, offset, n);

			return n;
		}

		internal async Task<int> ReadValueDataFullyAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			int nread = 0;
			int n;

			while (nread < count && (n = await ReadValueDataAsync (buffer, offset + nread, count - nread, cancellationToken).ConfigureAwait (false)) > 0)
				nread += n;

			return nread;
		}

		async Task<byte[]> ReadUnverifiedValueBytesAsync (int count, CancellationToken cancellationToken)
		{
			var chunks = new List<byte[]> ();
			int nread = 0;

			try {
				while (nread < count) {
					int requested = Math.Min (MaxUnverifiedAllocation, count - nread);
					var chunk = ArrayPool<byte>.Shared.Rent (MaxUnverifiedAllocation);

					chunks.Add (chunk);

					int n = await ReadValueDataFullyAsync (chunk, 0, requested, cancellationToken).ConfigureAwait (false);

					nread += n;

					if (n < requested)
						break;
				}

				return JoinChunks (chunks, nread);
			} finally {
				ReturnChunks (chunks);
			}
		}

		internal async Task<byte[]?> ReadValueBytesAsync (int count, TnefPropertyTag propertyTag, CancellationToken cancellationToken)
		{
			if (count == 0)
				return Array.Empty<byte> ();

			if (!TryReserveValueBytes (count, propertyTag))
				return null;

			byte[] buffer;
			int nread;

			if (CanAllocate (count)) {
				buffer = new byte[count];
				nread = await ReadValueDataFullyAsync (buffer, 0, count, cancellationToken).ConfigureAwait (false);

				if (nread < count)
					Array.Resize (ref buffer, nread);
			} else {
				buffer = await ReadUnverifiedValueBytesAsync (count, cancellationToken).ConfigureAwait (false);
				nread = buffer.Length;
			}

			if (nread < count) {
				root.dataBytesRemaining += count - nread;
				SetTruncated (propertyTag);
			}

			return buffer;
		}

		async Task<bool> ReadHeaderAsync (CancellationToken cancellationToken)
		{
			if (skipIidPrefix) {
				if (!await FillAsync (16, cancellationToken).ConfigureAwait (false)) {
					SetTruncated ();
					return false;
				}

				inputIndex += 16;
			}

			if (!await FillAsync (4, cancellationToken).ConfigureAwait (false)) {
				SetTruncated ();
				return false;
			}

			if (!ProcessHeader ())
				return false;

			if (!await FillAsync (2, cancellationToken).ConfigureAwait (false)) {
				SetTruncated ();
				return false;
			}

			LegacyKey = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian (input.AsSpan (inputIndex, 2));
			inputIndex += 2;

			return true;
		}

		async Task<bool> ReadAttributeHeaderAsync (CancellationToken cancellationToken)
		{
			attributeOffset = LocalOffset;

			if (!await FillAsync (1, cancellationToken).ConfigureAwait (false))
				return false;

			if (!await FillAsync (AttributeHeaderSize, cancellationToken).ConfigureAwait (false)) {
				SetTruncated ();
				return false;
			}

			if (!ProcessAttributeHeader ())
				return false;

			int peek = GetPeekLength ();
			bool peeked = peek > 0 && await FillAsync (peek, cancellationToken).ConfigureAwait (false);

			CheckAttributeTag (peeked);

			return true;
		}

		async Task<bool> FinishAttributeAsync (CancellationToken cancellationToken)
		{
			if (truncated)
				return false;

			long remaining = valueEnd - LocalOffset;

			if (remaining > 0 && !await SkipAsync (remaining, cancellationToken).ConfigureAwait (false)) {
				SetTruncated ();
				return false;
			}

			if (!await FillAsync (2, cancellationToken).ConfigureAwait (false)) {
				SetTruncated ();
				return false;
			}

			return ProcessChecksum ();
		}

		/// <summary>
		/// Asynchronously advance to the next attribute.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously advances the reader to the next attribute in the TNEF stream.</para>
		/// <para>The first call reads the TNEF stream header. If the stream does not begin with the TNEF signature,
		/// a <see cref="TnefComplianceViolation.InvalidSignature"/> issue is reported and the method returns
		/// <see langword="false"/>.</para>
		/// <para>Any part of the current attribute's value that has not been consumed is skipped, and the
		/// attribute's checksum is verified.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="TnefReaderAsync"/>
		/// </example>
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
		public async Task<bool> ReadAsync (CancellationToken cancellationToken = default)
		{
			if (!BeginRead ())
				return false;

			if (state == ReaderState.Initial) {
				ResetComplianceBudget ();

				if (!await ReadHeaderAsync (cancellationToken).ConfigureAwait (false))
					return EndRead (false);
			} else if (!await FinishAttributeAsync (cancellationToken).ConfigureAwait (false)) {
				return EndRead (false);
			}

			ResetAttribute ();

			return EndRead (await ReadAttributeHeaderAsync (cancellationToken).ConfigureAwait (false));
		}

		async Task EnsureScalarAsync (int width, CancellationToken cancellationToken)
		{
			ClaimValue (ValueClaim.Scalar);

			if (scalarLength >= 0)
				return;

			int count = GetScalarReadLength (width);

			await FillAsync (count, cancellationToken).ConfigureAwait (false);
			LoadScalar (width, count);
		}

		/// <summary>
		/// Asynchronously read the current attribute's value as a 16-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously reads the current attribute's value as a 16-bit integer.</para>
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
		public async Task<short> ReadValueAsInt16Async (CancellationToken cancellationToken = default)
		{
			return (short) await ReadValueAsInt32Async (cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Asynchronously read the current attribute's value as a 32-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously reads the current attribute's value as a 32-bit integer.</para>
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
		public async Task<int> ReadValueAsInt32Async (CancellationToken cancellationToken = default)
		{
			CheckAttribute ();

			int width = GetScalarWidth (false);

			await EnsureScalarAsync (width, cancellationToken).ConfigureAwait (false);

			return GetScalarInt32 (width);
		}

		/// <summary>
		/// Asynchronously read the current attribute's value as a date and time.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously reads the current attribute's value as a date and time.</para>
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
		public async Task<DateTime> ReadValueAsDateTimeAsync (CancellationToken cancellationToken = default)
		{
			CheckDateAttribute ();
			await EnsureScalarAsync (14, cancellationToken).ConfigureAwait (false);

			return dateValue;
		}

		/// <summary>
		/// Asynchronously read the current attribute's value as a string.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously reads the current attribute's value as a string, decoded using the current <see cref="Codepage"/>.</para>
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
		public async Task<string> ReadValueAsStringAsync (CancellationToken cancellationToken = default)
		{
			CheckStringAttribute ();

			return DecodeString (encoding, await ReadValueAsBytesAsync (cancellationToken).ConfigureAwait (false));
		}

		/// <summary>
		/// Asynchronously read the current attribute's raw value.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously reads the current attribute's raw value.</para>
		/// <para>The value may only be read once.</para>
		/// <para>If the value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/>
		/// issue is reported and an empty array is returned. Use <see cref="OpenValueStream"/> to read large values.</para>
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
		public async Task<byte[]> ReadValueAsBytesAsync (CancellationToken cancellationToken = default)
		{
			ClaimValue (ValueClaim.Raw);

			return await ReadValueBytesAsync ((int) ValueRemaining, TnefPropertyTag.Null, cancellationToken).ConfigureAwait (false) ?? Array.Empty<byte> ();
		}
	}
}