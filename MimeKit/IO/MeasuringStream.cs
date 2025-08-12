﻿//
// MeasuringStream.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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

namespace MimeKit.IO {
	/// <summary>
	/// A stream useful for measuring the amount of data written.
	/// </summary>
	/// <remarks>
	/// A <see cref="MeasuringStream"/> keeps track of the number of bytes
	/// that have been written to it. This is useful, for example, when you
	/// need to know how large a <see cref="MimeMessage"/> is without
	/// actually writing it to disk or into a memory buffer.
	/// </remarks>
	public class MeasuringStream : Stream
	{
		bool disposed;
		long position;
		long length;

		/// <summary>
		/// Initialize a new instance of the <see cref="MeasuringStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MeasuringStream"/>.
		/// </remarks>
		public MeasuringStream ()
		{
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (MeasuringStream));
		}

		#region implemented abstract members of Stream

		/// <summary>
		/// Check whether the stream supports reading.
		/// </summary>
		/// <remarks>
		/// A <see cref="MeasuringStream"/> is not readable.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports reading; otherwise, <see langword="false" />.</value>
		public override bool CanRead {
			get { return false; }
		}

		/// <summary>
		/// Check whether the stream supports writing.
		/// </summary>
		/// <remarks>
		/// A <see cref="MeasuringStream"/> is always writable.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports writing; otherwise, <see langword="false" />.</value>
		public override bool CanWrite {
			get { return true; }
		}

		/// <summary>
		/// Check whether the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// A <see cref="MeasuringStream"/> is always seekable.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports seeking; otherwise, <see langword="false" />.</value>
		public override bool CanSeek {
			get { return true; }
		}

		/// <summary>
		/// Check whether reading and writing to the stream can time out.
		/// </summary>
		/// <remarks>
		/// Writing to a <see cref="MeasuringStream"/> cannot timeout.
		/// </remarks>
		/// <value><see langword="true" /> if reading and writing to the stream can time out; otherwise, <see langword="false" />.</value>
		public override bool CanTimeout {
			get { return false; }
		}

		/// <summary>
		/// Get the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// The length of a <see cref="MeasuringStream"/> indicates the
		/// number of bytes that have been written to it.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Length {
			get {
				CheckDisposed ();

				return length;
			}
		}

		/// <summary>
		/// Get or set the current position within the stream.
		/// </summary>
		/// <remarks>
		/// Since it is possible to seek within a <see cref="MeasuringStream"/>,
		/// it is possible that the position will not always be identical to the
		/// length of the stream, but typically it will be.
		/// </remarks>
		/// <value>The position of the stream.</value>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Position {
			get { return position; }
			set { Seek (value, SeekOrigin.Begin); }
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

		/// <summary>
		/// Read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reading from a <see cref="MeasuringStream"/> is not supported.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();

			throw new NotSupportedException ("The stream does not support reading");
		}

		/// <summary>
		/// Asynchronously read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reading from a <see cref="MeasuringStream"/> is not supported.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			CheckDisposed ();

			throw new NotSupportedException ("The stream does not support reading");
		}

		/// <summary>
		/// Write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Increments the <see cref="Position"/> property by the number of bytes written.
		/// If the updated position is greater than the current length of the stream, then
		/// the <see cref="Length"/> property will be updated to be identical to the
		/// position.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
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
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();

			ValidateArguments (buffer, offset, count);

			position += count;

			length = Math.Max (length, position);
		}

		/// <summary>
		/// Asynchronously write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Increments the <see cref="Position"/> property by the number of bytes written.
		/// If the updated position is greater than the current length of the stream, then
		/// the <see cref="Length"/> property will be updated to be identical to the
		/// position.
		/// </remarks>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			Write (buffer, offset, count);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Set the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Updates the <see cref="Position"/> within the stream.
		/// </remarks>
		/// <returns>The new position within the stream.</returns>
		/// <param name="offset">The offset into the stream relative to the <paramref name="origin"/>.</param>
		/// <param name="origin">The origin to seek from.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="origin"/> is not a valid <see cref="System.IO.SeekOrigin"/>. 
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin)
		{
			long real;

			CheckDisposed ();

			switch (origin) {
			case SeekOrigin.Begin:
				real = offset;
				break;
			case SeekOrigin.Current:
				real = position + offset;
				break;
			case SeekOrigin.End:
				real = length + offset;
				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (origin), "Invalid SeekOrigin specified");
			}

			// sanity check the resultant offset
			if (real < 0)
				throw new IOException ("Cannot seek to a position before the beginning of the stream");

			if (real > length)
				throw new IOException ("Cannot seek beyond the end of the stream");

			position = real;

			return position;
		}

		/// <summary>
		/// Clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Since a <see cref="MeasuringStream"/> does not actually do anything other than
		/// count bytes, this method is a no-op.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override void Flush ()
		{
			CheckDisposed ();

			// nothing to do...
		}

		/// <summary>
		/// Asynchronously clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Since a <see cref="MeasuringStream"/> does not actually do anything other than
		/// count bytes, this method is a no-op.
		/// </remarks>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			CheckDisposed ();

			// nothing to do...
			return Task.CompletedTask;
		}

		/// <summary>
		/// Set the length of the stream.
		/// </summary>
		/// <remarks>
		/// Sets the <see cref="Length"/> to the specified value and updates
		/// <see cref="Position"/> to the specified value if (and only if)
		/// the current position is greater than the new length value.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			if (value < 0)
				throw new ArgumentOutOfRangeException (nameof (value));

			position = Math.Min (position, value);
			length = value;
		}

		#endregion

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="MeasuringStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="MeasuringStream"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
		/// <see langword="false" /> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			disposed = true;
		}
	}
}
