//
// MemoryBlockStream.cs
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
using System.Collections.Generic;

namespace MimeKit.IO
{
	/// <summary>
	/// An efficient memory stream implementation that sacrifices the ability to
	/// get access to the internal byte buffer in order to drastically improve
	/// performance.
	/// </summary>
	/// <remarks>
	/// Instead of resizing an internal byte array, the <see cref="MemoryBlockStream"/>
	/// chains blocks of non-contiguous memory. This helps improve performance by avoiding
	/// unneeded copying of data from the old array to the newly allocated array as well
	/// as the zeroing of the newly allocated array.
	/// </remarks>
	public class MemoryBlockStream : Stream
	{
		const long MaxCapacity = int.MaxValue * BlockSize;
		const long BlockSize = 2048;

		readonly List<byte[]> blocks = new List<byte[]> ();
		long position, length;
		bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.MemoryBlockStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MemoryBlockStream"/> with an initial memory block
		/// of 2048 bytes.
		/// </remarks>
		public MemoryBlockStream ()
		{
			blocks.Add (new byte[BlockSize]);
		}

		/// <summary>
		/// Copies the memory stream into a byte array.
		/// </summary>
		/// <remarks>
		/// Copies all of the stream data into a newly allocated byte array.
		/// </remarks>
		/// <returns>The array.</returns>
		public byte[] ToArray ()
		{
			var array = new byte[length];
			int need = (int) length;
			int arrayIndex = 0;
			int nread = 0;
			int block = 0;

			while (nread < length) {
				int n = Math.Min ((int) BlockSize, need);
				Buffer.BlockCopy (blocks[block], 0, array, arrayIndex, n);
				arrayIndex += n;
				nread += n;
				need -= n;
				block++;
			}

			return array;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("stream");
		}

		#region implemented abstract members of Stream

		/// <summary>
		/// Checks whether or not the stream supports reading.
		/// </summary>
		/// <remarks>
		/// The <see cref="MemoryBlockStream"/> is always readable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		public override bool CanRead {
			get { return true; }
		}

		/// <summary>
		/// Checks whether or not the stream supports writing.
		/// </summary>
		/// <remarks>
		/// The <see cref="MemoryBlockStream"/> is always writable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		public override bool CanWrite {
			get { return true; }
		}

		/// <summary>
		/// Checks whether or not the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// The <see cref="MemoryBlockStream"/> is always seekable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</value>
		public override bool CanSeek {
			get { return true; }
		}

		/// <summary>
		/// Checks whether or not reading and writing to the stream can timeout.
		/// </summary>
		/// <remarks>
		/// The <see cref="MemoryBlockStream"/> does not support timing out.
		/// </remarks>
		/// <value><c>true</c> if reading and writing to the stream can timeout; otherwise, <c>false</c>.</value>
		public override bool CanTimeout {
			get { return false; }
		}

		/// <summary>
		/// Gets the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// Gets the length of the stream, in bytes.
		/// </remarks>
		/// <value>The length of the stream, in bytes.</value>
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
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <remarks>
		/// Gets or sets the current position within the stream.
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
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position
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
			CheckDisposed ();

			ValidateArguments (buffer, offset, count);

			if (position == MaxCapacity)
				return 0;

			int max = Math.Min ((int) (length - position), count);
			int startIndex = (int) (position % BlockSize);
			int block = (int) (position / BlockSize);
			int nread = 0;

			while (nread < max && block < blocks.Count) {
				int n = Math.Min ((int) BlockSize - startIndex, max - nread);
				Buffer.BlockCopy (blocks[block], startIndex, buffer, offset + nread, n);
				startIndex = 0;
				nread += n;
				block++;
			}

			position += nread;

			return nread;
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Writes the entire buffer to the stream and advances the current position
		/// within the stream by the number of bytes written, adding memory blocks as
		/// needed in order to contain the newly written bytes.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
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

			if (position + count >= MaxCapacity)
				throw new IOException (string.Format ("Cannot exceed {0} bytes", MaxCapacity));

			int startIndex = (int) (position % BlockSize);
			long capacity = blocks.Count * BlockSize;
			int block = (int) (position / BlockSize);
			int nwritten = 0;

			while (capacity < position + count) {
				blocks.Add (new byte[BlockSize]);
				capacity += BlockSize;
			}

			while (nwritten < count) {
				int n = Math.Min ((int) BlockSize - startIndex, count - nwritten);
				Buffer.BlockCopy (buffer, offset + nwritten, blocks[block], startIndex, n);
				startIndex = 0;
				nwritten += n;
				block++;
			}

			position += nwritten;

			length = Math.Max (length, position);
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Sets the position within the current stream.
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
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
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
				throw new ArgumentOutOfRangeException ("origin", "Invalid SeekOrigin specified");
			}

			// sanity check the resultant offset
			if (real < 0)
				throw new IOException ("Cannot seek to a position before the beginning of the stream");

			if (real > MaxCapacity)
				throw new IOException (string.Format ("Cannot exceed {0} bytes", MaxCapacity));

			// short-cut if we are seeking to our current position
			if (real == position)
				return position;

			if (real > length)
				throw new IOException ("Cannot seek beyond the end of the stream");

			position = real;

			return position;
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// This method does not do anything.
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
		/// Sets the length of the stream.
		/// </summary>
		/// <remarks>
		/// Sets the length of the stream.
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

			if (value < 0 || value > MaxCapacity)
				throw new ArgumentOutOfRangeException ("value");

			long capacity = blocks.Count * BlockSize;

			if (value > capacity) {
				do {
					blocks.Add (new byte[BlockSize]);
					capacity += BlockSize;
				} while (capacity < value);
			} else if (value < length) {
				// shed any blocks that are no longer needed
				while (capacity - value > BlockSize) {
					blocks.RemoveAt (blocks.Count - 1);
					capacity -= BlockSize;
				}

				// reset the range of bytes between the new length and the old length to 0
				int count = (int) (Math.Min (length, capacity) - value);
				int startIndex = (int) (value % BlockSize);
				int block = (int) (value / BlockSize);

				Array.Clear (blocks[block], startIndex, count);
			}

			position = Math.Min (position, value);
			length = value;
		}

		#endregion

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="MemoryBlockStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			disposed = true;
		}
	}
}
