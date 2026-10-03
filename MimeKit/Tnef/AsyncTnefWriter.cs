//
// AsyncTnefWriter.cs
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

using MimeKit.Utils;

namespace MimeKit.Tnef {
	public sealed partial class TnefWriter
	{
		async Task WriteHeaderAsync (CancellationToken cancellationToken)
		{
			if (headerWritten)
				return;

			var buffer = GetStreamHeader ();

			await stream.WriteAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
			headerWritten = true;
		}

		async Task WriteAttributeValueAsync (TnefAttributeLevel level, TnefAttributeTag tag, byte[] buffer, int startIndex, int length, CancellationToken cancellationToken)
		{
			EncodeAttributeHeader (header, level, tag, length);
			await stream.WriteAsync (header, 0, AttributeHeaderSize, cancellationToken).ConfigureAwait (false);
			await stream.WriteAsync (buffer, startIndex, length, cancellationToken).ConfigureAwait (false);

			var checksum = EncodeChecksum (GetChecksum (buffer, startIndex, length));
			await stream.WriteAsync (checksum, 0, checksum.Length, cancellationToken).ConfigureAwait (false);
		}

		async Task WriteBufferedAttributeAsync (BufferedAttribute attribute, CancellationToken cancellationToken)
		{
			var value = attribute.Value;
			var buffer = ArrayPool<byte>.Shared.Rent (4096);

			try {
				ushort checksum = 0;
				int nread;

				EncodeAttributeHeader (header, attribute.Level, attribute.Tag, (int) value.Length);
				await stream.WriteAsync (header, 0, AttributeHeaderSize, cancellationToken).ConfigureAwait (false);

				value.Position = 0;

				while ((nread = value.Read (buffer, 0, buffer.Length)) > 0) {
					checksum = TnefChecksum.Update (checksum, buffer, 0, nread);
					await stream.WriteAsync (buffer, 0, nread, cancellationToken).ConfigureAwait (false);
				}

				var encoded = EncodeChecksum (checksum);
				await stream.WriteAsync (encoded, 0, encoded.Length, cancellationToken).ConfigureAwait (false);
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		async Task WriteQueuedAsync (CancellationToken cancellationToken)
		{
			await WriteHeaderAsync (cancellationToken).ConfigureAwait (false);

			while (queue.Count > 0) {
				var attribute = queue[0];

				queue.RemoveAt (0);

				try {
					await WriteBufferedAttributeAsync (attribute, cancellationToken).ConfigureAwait (false);
				} finally {
					attribute.Value.Dispose ();
				}
			}
		}

		async Task WriteAttributeCoreAsync (TnefAttributeTag tag, byte[] buffer, int startIndex, int length, string paramName, CancellationToken cancellationToken)
		{
			CheckCanWrite ();

			var level = BeginAttribute (tag, paramName);

			await WriteQueuedAsync (cancellationToken).ConfigureAwait (false);
			await WriteAttributeValueAsync (level, tag, buffer, startIndex, length, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Asynchronously write an attribute with an integer value.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously writes an attribute with an integer value.</para>
		/// <para>The value of an attribute with a <see cref="TnefAttributeType.Short"/> or
		/// <see cref="TnefAttributeType.Word"/> type is written as a 16-bit integer and the value of an attribute
		/// with a <see cref="TnefAttributeType.Long"/> or <see cref="TnefAttributeType.DWord"/> type is written as
		/// a 32-bit integer.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not an integer.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> does not fit in a 16-bit attribute value.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteAttributeAsync (TnefAttributeTag tag, int value, CancellationToken cancellationToken = default)
		{
			var buffer = EncodeInt32 (tag, value);

			return WriteAttributeCoreAsync (tag, buffer, 0, buffer.Length, nameof (tag), cancellationToken);
		}

		/// <summary>
		/// Asynchronously write an attribute with a string value.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously writes an attribute with a string value, encoded using the <see cref="Codepage"/> and
		/// terminated by a nul character.</para>
		/// <para>The value of an attribute with a <see cref="TnefAttributeType.String"/> or
		/// <see cref="TnefAttributeType.Text"/> type and the value of the <see cref="TnefAttributeTag.MessageClass"/>
		/// and <see cref="TnefAttributeTag.OriginalMessageClass"/> attributes may be written as a string. A message
		/// class must consist of between 1 and 254 printable ASCII characters.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not a string.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is not a valid message class.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteAttributeAsync (TnefAttributeTag tag, string value, CancellationToken cancellationToken = default)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			var buffer = EncodeString (tag, value);

			return WriteAttributeCoreAsync (tag, buffer, 0, buffer.Length, nameof (tag), cancellationToken);
		}

		/// <summary>
		/// Asynchronously write an attribute with a date value.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously writes an attribute with a <see cref="TnefAttributeType.Date"/> type.</para>
		/// <para>The date and time components of <paramref name="value"/> are written as-is, without converting
		/// them to another time zone.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="value">The value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is not an attribute that can be written.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="tag"/> is not a date.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteAttributeAsync (TnefAttributeTag tag, DateTime value, CancellationToken cancellationToken = default)
		{
			var buffer = EncodeDate (tag, value);

			return WriteAttributeCoreAsync (tag, buffer, 0, buffer.Length, nameof (tag), cancellationToken);
		}

		/// <summary>
		/// Asynchronously write an attribute with a raw value.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes an attribute with the specified raw value.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="buffer">The buffer containing the value.</param>
		/// <param name="startIndex">The index of the first byte of the value.</param>
		/// <param name="length">The length of the value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that can be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteAttributeAsync (TnefAttributeTag tag, byte[] buffer, int startIndex, int length, CancellationToken cancellationToken = default)
		{
			ArgumentValidator.Validate (buffer, startIndex, length);

			return WriteAttributeCoreAsync (tag, buffer, startIndex, length, nameof (tag), cancellationToken);
		}

		/// <summary>
		/// Asynchronously write an attribute with a raw value.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes an attribute with the specified raw value.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="tag">The attribute tag.</param>
		/// <param name="buffer">The value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="tag"/> is not an attribute that can be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute cannot be written at this point in the stream.</para>
		/// <para>-or-</para>
		/// <para>The stream or property writer for the previous attribute has not been disposed.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteAttributeAsync (TnefAttributeTag tag, byte[] buffer, CancellationToken cancellationToken = default)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			return WriteAttributeCoreAsync (tag, buffer, 0, buffer.Length, nameof (tag), cancellationToken);
		}

		/// <summary>
		/// Asynchronously flush the writer.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the stream header and any buffered attributes to the output stream and then flushes
		/// the output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The stream or property writer for the previous attribute has not been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task FlushAsync (CancellationToken cancellationToken = default)
		{
			CheckCanWrite ();
			await WriteQueuedAsync (cancellationToken).ConfigureAwait (false);
			await stream.FlushAsync (cancellationToken).ConfigureAwait (false);
		}
	}
}
