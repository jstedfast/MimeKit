//
// ChainedStream.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

namespace MimeKit {
	public class ChainedStream : Stream
	{
		readonly List<Stream> streams = new List<Stream> ();
		long position;
		bool disposed;
		int current;
		bool eos;

		public ChainedStream ()
		{
		}

		/// <summary>
		/// Add the specified stream to the chained stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
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
		/// <value>
		/// <c>true</c> if the stream supports reading; otherwise, <c>false</c>.
		/// </value>
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
		/// <value>
		/// <c>true</c> if the stream supports writing; otherwise, <c>false</c>.
		/// </value>
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
		/// <value>
		/// <c>true</c> if the stream supports seeking; otherwise, <c>false</c>.
		/// </value>
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
		/// <value>
		/// <c>true</c> if I/O operations can timeout; otherwise, <c>false</c>.
		/// </value>
		public override bool CanTimeout {
			get { return false; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		/// <value>
		/// The length of the stream.
		/// </value>
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
		/// Gets or sets the position of the stream.
		/// </summary>
		/// <value>
		/// The position of the stream.
		/// </value>
		public override long Position {
			get { return position; }
			set {
				CheckDisposed ();
				CheckCanSeek ();

				Seek (value, SeekOrigin.Begin);
			}
		}

		void ValidateArguments (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		/// <summary>
		/// Reads data into the specified buffer.
		/// </summary>
		/// <param name='buffer'>
		/// The buffer to read data into.
		/// </param>
		/// <param name='offset'>
		/// The offset into the buffer to start reading data.
		/// </param>
		/// <param name='count'>
		/// The number of bytes to read.
		/// </param>
		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			CheckCanRead ();

			ValidateArguments (buffer, offset, count);

			if (count == 0 || eos)
				return 0;

			int n, nread = 0;

			while (current < streams.Count && nread < count) {
				if ((n = streams[current].Read (buffer, offset + nread, count - nread)) > 0)
					nread += n;
				else
					current++;
			}

			if (nread > 0)
				position += nread;
			else if (nread == 0)
				eos = true;

			return nread;
		}

		/// <summary>
		/// Writes the specified buffer.
		/// </summary>
		/// <param name='buffer'>
		/// The buffer to write.
		/// </param>
		/// <param name='offset'>
		/// The offset of the first byte to write.
		/// </param>
		/// <param name='count'>
		/// The number of bytes to write.
		/// </param>
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
		/// Seeks to the specified offset.
		/// </summary>
		/// <param name='offset'>
		/// The offset from the specified origin.
		/// </param>
		/// <param name='origin'>
		/// The origin from which to seek.
		/// </param>
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
			} else {
				int max = Math.Min (streams.Count - 1, current);
				int cur = 0;

				position = 0;
				while (cur <= max && position < real) {
					length = streams[current].Length;

					if (real < position + length) {
						// this is the stream which encompasses our seek offset
						streams[cur].Seek (real - position, SeekOrigin.Begin);
						break;
					}

					position += length;
					cur++;
				}

				current = cur++;

				// reset any streams between our new current stream and our old current stream
				while (cur <= max)
					streams[cur++].Seek (0, SeekOrigin.Begin);
			}

			return position;
		}

		/// <summary>
		/// Flushes any internal output buffers.
		/// </summary>
		public override void Flush ()
		{
			CheckDisposed ();
			CheckCanWrite ();

			if (current < streams.Count)
				streams[current].Flush ();
		}

		/// <summary>
		/// Sets the length.
		/// </summary>
		/// <param name='value'>
		/// The new length.
		/// </param>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			throw new NotSupportedException ("Cannot set a length on the stream");
		}

		protected override void Dispose (bool disposing)
		{
			// FIXME: should we dispose our streams?
			base.Dispose (disposing);
			disposed = true;
		}
	}
}

