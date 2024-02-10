//
// TnefReaderStream.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

namespace MimeKit.Tnef {
	/// <summary>
	/// A stream for reading raw values from a <see cref="TnefReader"/> or <see cref="TnefPropertyReader"/>.
	/// </summary>
	/// <remarks>
	/// A stream for reading raw values from a <see cref="TnefReader"/> or <see cref="TnefPropertyReader"/>.
	/// </remarks>
	class TnefReaderStream : Stream
	{
		readonly int valueEndOffset, dataEndOffset;
		readonly TnefReader reader;
		bool disposed;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefReaderStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a stream for reading a raw value from the <see cref="TnefReader"/>.
		/// </remarks>
		/// <param name="tnefReader">The <see cref="TnefReader"/>.</param>
		/// <param name="dataEndOffset">The end offset of the data.</param>
		/// <param name="valueEndOffset">The end offset of the container value.</param>
		public TnefReaderStream (TnefReader tnefReader, int dataEndOffset, int valueEndOffset)
		{
			this.valueEndOffset = valueEndOffset;
			this.dataEndOffset = dataEndOffset;
			reader = tnefReader;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefReaderStream));
		}

		/// <summary>
		/// Check whether or not the stream supports reading.
		/// </summary>
		/// <remarks>
		/// The <see cref="TnefReaderStream"/> is always readable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		public override bool CanRead {
			get { return true; }
		}

		/// <summary>
		/// Check whether or not the stream supports writing.
		/// </summary>
		/// <remarks>
		/// Writing to a <see cref="TnefReaderStream"/> is not supported.
		/// </remarks>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		public override bool CanWrite {
			get { return false; }
		}

		/// <summary>
		/// Check whether or not the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// Seeking within a <see cref="TnefReaderStream"/> is not supported.
		/// </remarks>
		/// <value><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</value>
		public override bool CanSeek {
			get { return false; }
		}

		/// <summary>
		/// Get the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// Getting the length of a <see cref="TnefReaderStream"/> is not supported.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Length {
			get { throw new NotSupportedException ("Cannot get the length of the stream."); }
		}

		/// <summary>
		/// Get or sets the current position within the stream.
		/// </summary>
		/// <remarks>
		/// Getting and setting the position of a <see cref="TnefReaderStream"/> is not supported.
		/// </remarks>
		/// <value>The position of the stream.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Position {
			get { throw new NotSupportedException ("The stream does not support seeking."); }
			set { throw new NotSupportedException ("The stream does not support seeking."); }
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
		/// Reads a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
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
		public override int Read (byte[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);

			CheckDisposed ();

			int dataLeft = dataEndOffset - reader.StreamOffset;
			int n = Math.Min (dataLeft, count);

			int nread = n > 0 ? reader.ReadAttributeRawValue (buffer, offset, n) : 0;

			dataLeft -= nread;

			if (dataLeft == 0 && valueEndOffset > reader.StreamOffset) {
				int valueLeft = valueEndOffset - reader.StreamOffset;
				var buf = new byte[valueLeft];

				reader.ReadAttributeRawValue (buf, 0, valueLeft);
			}

			return nread;
		}

		/// <summary>
		/// Write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// The <see cref="TnefReaderStream"/> does not support writing.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ("The stream does not support writing.");
		}

		/// <summary>
		/// Set the position within the current stream.
		/// </summary>
		/// <remarks>
		/// The <see cref="TnefReaderStream"/> does not support seeking.
		/// </remarks>
		/// <returns>The new position within the stream.</returns>
		/// <param name="offset">The offset into the stream relative to the <paramref name="origin"/>.</param>
		/// <param name="origin">The origin to seek from.</param>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ("The stream does not support seeking.");
		}

		/// <summary>
		/// Clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// The <see cref="TnefReaderStream"/> does not support writing.
		/// </remarks>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		public override void Flush ()
		{
			throw new NotSupportedException ("The stream does not support writing.");
		}

		/// <summary>
		/// Set the length of the stream.
		/// </summary>
		/// <remarks>
		/// The <see cref="TnefReaderStream"/> does not support setting the length.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support setting the length.
		/// </exception>
		public override void SetLength (long value)
		{
			throw new NotSupportedException ("The stream does not support setting the length.");
		}

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="TnefReaderStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// The underlying <see cref="TnefReader"/> is not disposed.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			disposed = true;
		}
	}
}
