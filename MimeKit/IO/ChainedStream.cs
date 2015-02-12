//
// ChainedStream.cs
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

namespace MimeKit.IO {
	/// <summary>
	/// A chained stream.
	/// </summary>
	/// <remarks>
	/// Chains multiple streams together such that reading or writing beyond the end
	/// of one stream spills over into the next stream in the chain. The idea is to
	/// make it appear is if the chain of streams is all one continuous stream.
	/// </remarks>
	public class ChainedStream : Stream
	{
		readonly List<Stream> streams;
		long position;
		bool disposed;
		int current;
		bool eos;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.ChainedStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ChainedStream"/>.
		/// </remarks>
		public ChainedStream ()
		{
			streams = new List<Stream> ();
		}

		/// <summary>
		/// Add the specified stream to the chained stream.
		/// </summary>
		/// <remarks>
		/// Adds the stream to the end of the chain.
		/// </remarks>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void Add (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			streams.Add (stream);
			eos = false;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("stream");
		}

		void CheckCanSeek ()
		{
			if (!CanSeek)
				throw new NotSupportedException ("The stream does not support seeking");
		}

		void CheckCanRead ()
		{
			if (!CanRead)
				throw new NotSupportedException ("The stream does not support reading");
		}

		void CheckCanWrite ()
		{
			if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
		}

		/// <summary>
		/// Checks whether or not the stream supports reading.
		/// </summary>
		/// <remarks>
		/// The <see cref="ChainedStream"/> only supports reading if all of its
		/// streams support it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		public override bool CanRead {
			get {
				foreach (var stream in streams) {
					if (!stream.CanRead)
						return false;
				}

				return streams.Count > 0;
			}
		}

		/// <summary>
		/// Checks whether or not the stream supports writing.
		/// </summary>
		/// <remarks>
		/// The <see cref="ChainedStream"/> only supports writing if all of its
		/// streams support it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		public override bool CanWrite {
			get {
				foreach (var stream in streams) {
					if (!stream.CanWrite)
						return false;
				}

				return streams.Count > 0;
			}
		}

		/// <summary>
		/// Checks whether or not the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// The <see cref="ChainedStream"/> only supports seeking if all of its
		/// streams support it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</value>
		public override bool CanSeek {
			get {
				foreach (var stream in streams) {
					if (!stream.CanSeek)
						return false;
				}

				return streams.Count > 0;
			}
		}

		/// <summary>
		/// Checks whether or not I/O operations can timeout.
		/// </summary>
		/// <remarks>
		/// The <see cref="ChainedStream"/> only supports timeouts if all of its
		/// streams support them.
		/// </remarks>
		/// <value><c>true</c> if I/O operations can timeout; otherwise, <c>false</c>.</value>
		public override bool CanTimeout {
			get { return false; }
		}

		/// <summary>
		/// Gets the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// The length of a <see cref="ChainedStream"/> is the combined lenths of all
		/// of its chained streams.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Length {
			get {
				long length = 0;

				CheckDisposed ();

				foreach (var stream in streams)
					length += stream.Length;

				return length;
			}
		}

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <remarks>
		/// It is always possible to get the position of a <see cref="ChainedStream"/>,
		/// but setting the position is only possible if all of its streams are seekable.
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
		/// Reads up to the requested number of bytes if reading is supported. If the
		/// current child stream does not have enough remaining data to complete the
		/// read, the read will progress into the next stream in the chain in order
		/// to complete the read.
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
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			CheckCanRead ();

			ValidateArguments (buffer, offset, count);

			if (count == 0 || eos)
				return 0;

			int n, nread = 0;

			while (current < streams.Count) {
				if ((n = streams[current].Read (buffer, offset + nread, count - nread)) > 0) {
					nread += n;

					if (nread == count)
						break;
				}

				current++;
			}

			if (nread > 0)
				position += nread;
			else if (nread == 0)
				eos = true;

			return nread;
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Writes the requested number of bytes if writing is supported. If the
		/// current child stream does not have enough remaining space to fit the
		/// complete buffer, the data will spill over into the next stream in the
		/// chain in order to complete the write.
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
			CheckCanWrite ();

			ValidateArguments (buffer, offset, count);

			if (current >= streams.Count)
				current = streams.Count - 1;

			int nwritten = 0;

			while (current < streams.Count && nwritten < count) {
				int n = count - nwritten;

				if (current + 1 < streams.Count) {
					long left = streams[current].Length - streams[current].Position;

					if (left < n)
						n = (int) left;
				}

				streams[current].Write (buffer, offset + nwritten, n);
				position += n;
				nwritten += n;

				if (nwritten < count) {
					streams[current].Flush ();
					current++;
				}
			}
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Seeks to the specified position within the stream if all child streams
		/// support seeking.
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
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin)
		{
			CheckDisposed ();
			CheckCanSeek ();

			long length = -1;
			long real;

			switch (origin) {
			case SeekOrigin.Begin:
				real = offset;
				break;
			case SeekOrigin.Current:
				real = position + offset;
				break;
			case SeekOrigin.End:
				length = Length;
				real = length + offset;
				break;
			default:
				throw new ArgumentOutOfRangeException ("origin", "Invalid SeekOrigin specified");
			}

			// sanity check the resultant offset
			if (real < 0)
				throw new IOException ("Cannot seek to a position before the beginning of the stream");

			// short-cut if we are seeking to our current position
			if (real == position)
				return position;

			if (real > (length < 0 ? Length : length))
				throw new IOException ("Cannot seek beyond the end of the stream");

			if (real > position) {
				while (current < streams.Count && position < real) {
					long left = streams[current].Length - streams[current].Position;
					long n = Math.Min (left, real - position);

					streams[current].Seek (n, SeekOrigin.Current);
					position += n;

					if (position < real)
						current++;
				}

				eos = current >= streams.Count;
			} else {
				int max = Math.Min (streams.Count - 1, current);
				int cur = 0;

				position = 0;
				while (cur <= max) {
					length = streams[cur].Length;

					if (real < position + length) {
						// this is the stream which encompasses our seek offset
						streams[cur].Seek (real - position, SeekOrigin.Begin);
						position = real;
						break;
					}

					position += length;
					cur++;
				}

				current = cur++;

				// reset any streams between our new current stream and our old current stream
				while (cur <= max)
					streams[cur++].Seek (0, SeekOrigin.Begin);

				eos = false;
			}

			return position;
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// If all of the child streams support writing, then the current child stream
		/// will be flushed.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void Flush ()
		{
			CheckDisposed ();
			CheckCanWrite ();

			if (current < streams.Count)
				streams[current].Flush ();
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <remarks>
		/// Setting the length of a <see cref="ChainedStream"/> is not supported.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support setting the length.
		/// </exception>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			throw new NotSupportedException ("Cannot set a length on the stream");
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ChainedStream"/> and
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
