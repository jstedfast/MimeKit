//
// Substream.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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

namespace MimeKit {
	public class Substream : Stream
	{
		long position;
		bool disposed;
		bool eos;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Substream"/> class.
		/// </summary>
		/// <param name='source'>
		/// The underlying source stream.
		/// </param>
		/// <param name='bound_start'>
		/// The offset in the source stream that will mark the start of this substream.
		/// </param>
		/// <param name='bound_end'>
		/// The offset in the source stream that will mark the end of this substream.
		/// </param>
		public Substream (Stream source, long bound_start, long bound_end)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (bound_start < 0)
				throw new ArgumentOutOfRangeException ("bound_start");

			if (bound_end >= 0 && bound_end < bound_start)
				throw new ArgumentOutOfRangeException ("bound_end");

			BoundEnd = bound_end < 0 ? -1 : bound_end;
			BoundStart = bound_start;
			Source = source;
			position = 0;
			eos = false;
		}

		/// <summary>
		/// Gets the underlying source stream.
		/// </summary>
		/// <value>
		/// The underlying source stream.
		/// </value>
		public Stream Source {
			get; private set;
		}

		/// <summary>
		/// Gets the start boundary offset of the underlying source stream.
		/// </summary>
		/// <value>
		/// The start boundary offset of the underlying source stream.
		/// </value>
		public long BoundStart {
			get; private set;
		}

		/// <summary>
		/// Gets the end boundary offset of the underlying source stream.
		/// </summary>
		/// <value>
		/// The end boundary offset of the underlying source stream.
		/// </value>
		public long BoundEnd {
			get; private set;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("Substream");
		}

		void CheckCanSeek ()
		{
			if (!Source.CanSeek)
				throw new NotSupportedException ("The stream does not support seeking");
		}

		void CheckCanRead ()
		{
			if (!Source.CanRead)
				throw new NotSupportedException ("The stream does not support reading");
		}

		void CheckCanWrite ()
		{
			if (!Source.CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
		}

		#region Stream implementation

		/// <summary>
		/// Checks whether or not the stream supports reading.
		/// </summary>
		/// <value>
		/// <c>true</c> if the stream supports reading; otherwise, <c>false</c>.
		/// </value>
		public override bool CanRead {
			get { return Source.CanRead; }
		}

		/// <summary>
		/// Checks whether or not the stream supports writing.
		/// </summary>
		/// <value>
		/// <c>true</c> if the stream supports writing; otherwise, <c>false</c>.
		/// </value>
		public override bool CanWrite {
			get { return Source.CanWrite; }
		}

		/// <summary>
		/// Checks whether or not the stream supports seeking.
		/// </summary>
		/// <value>
		/// <c>true</c> if the stream supports seeking; otherwise, <c>false</c>.
		/// </value>
		public override bool CanSeek {
			get { return Source.CanSeek; }
		}

		/// <summary>
		/// Checks whether or not I/O operations can timeout.
		/// </summary>
		/// <value>
		/// <c>true</c> if I/O operations can timeout; otherwise, <c>false</c>.
		/// </value>
		public override bool CanTimeout {
			get { return Source.CanTimeout; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		/// <value>
		/// The length of the stream.
		/// </value>
		public override long Length {
			get {
				CheckDisposed ();

				if (BoundEnd != -1)
					return BoundEnd - BoundStart;

				return Source.Length - BoundStart;
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

		/// <summary>
		/// Gets or sets the read timeout.
		/// </summary>
		/// <value>
		/// The read timeout.
		/// </value>
		public override int ReadTimeout {
			get { return Source.ReadTimeout; }
			set { Source.ReadTimeout = value; }
		}

		/// <summary>
		/// Gets or sets the write timeout.
		/// </summary>
		/// <value>
		/// The write timeout.
		/// </value>
		public override int WriteTimeout {
			get { return Source.WriteTimeout; }
			set { Source.WriteTimeout = value; }
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
		/// Begins an asynchronous read.
		/// </summary>
		/// <returns>
		/// The async result.
		/// </returns>
		/// <param name='buffer'>
		/// The buffer to read data into.
		/// </param>
		/// <param name='offset'>
		/// The buffer offset to start reading into.
		/// </param>
		/// <param name='count'>
		/// The number of bytes to read.
		/// </param>
		/// <param name='callback'>
		/// An async callback.
		/// </param>
		/// <param name='state'>
		/// Custom state to pass to the async callback.
		/// </param>
		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckDisposed ();
			CheckCanRead ();

			return base.BeginRead (buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Begins an asynchronous write.
		/// </summary>
		/// <returns>
		/// The async result.
		/// </returns>
		/// <param name='buffer'>
		/// The buffer containing data to write.
		/// </param>
		/// <param name='offset'>
		/// The beginning offset of the buffer to write.
		/// </param>
		/// <param name='count'>
		/// The number of bytes to write.
		/// </param>
		/// <param name='callback'>
		/// The async callback.
		/// </param>
		/// <param name='state'>
		/// Custom state to pass to the async callback.
		/// </param>
		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckDisposed ();
			CheckCanWrite ();

			return base.BeginWrite (buffer, offset, count, callback, state);
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

			// if we are at the end of the stream, we cannot read anymore data
			if (BoundEnd != -1 && BoundStart + position >= BoundEnd) {
				eos = true;
				return 0;
			}

			// make sure that the source stream is in the expected position
			if (Source.Position != BoundStart + position)
				Source.Seek (BoundStart + position, SeekOrigin.Begin);

			int n = BoundEnd != -1 ? (int) Math.Min (BoundEnd - (BoundStart + position), count) : count;
			int nread = Source.Read (buffer, offset, n);

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

			// if we are at the end of the stream, we cannot write anymore data
			if (BoundEnd != -1 && BoundStart + position + count > BoundEnd) {
				eos = BoundStart + position >= BoundEnd;
				throw new IOException ();
			}

			// make sure that the source stream is in the expected position
			if (Source.Position != BoundStart + position)
				Source.Seek (BoundStart + position, SeekOrigin.Begin);

			Source.Write (buffer, offset, count);
			position += count;

			if (BoundEnd != -1 && BoundStart + position >= BoundEnd)
				eos = true;
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

			long real;

			switch (origin) {
			case SeekOrigin.Begin:
				real = BoundStart + offset;
				break;
			case SeekOrigin.Current:
				real = position + offset;
				break;
			case SeekOrigin.End:
				if (offset >= 0 || (BoundEnd == -1 && !eos)) {
					// We don't know if the underlying stream can seek past the end or not...
					if ((real = Source.Seek (offset, origin)) == -1)
						return -1;
				} else if (BoundEnd == -1) {
					// seeking backwards from eos (which happens to be our current position)
					real = position + offset;
				} else {
					// seeking backwards from a known position
					real = BoundEnd + offset;
				}
				
				break;
			default:
				throw new ArgumentException ("Invalid SeekOrigin specified", "origin");
			}
			
			// sanity check the resultant offset
			if (real < BoundStart)
				throw new IOException ("Cannot seek to a position before the beginning of the stream");
			
			// short-cut if we are seeking to our current position
			if (real == BoundStart + position)
				return position;
			
			if (BoundEnd != -1 && real > BoundEnd)
				throw new IOException ("Cannot seek beyond the end of the stream");
			
			if ((real = Source.Seek (real, SeekOrigin.Begin)) == -1)
				return -1;
			
			// reset eos if appropriate
			if ((BoundEnd != -1 && real < BoundEnd) || (eos && real < BoundStart + position))
				eos = false;
			
			position = real - BoundStart;

			return position;
		}

		/// <summary>
		/// Flushes any internal output buffers.
		/// </summary>
		public override void Flush ()
		{
			CheckDisposed ();
			CheckCanWrite ();

			Source.Flush ();
		}

		/// <summary>
		/// Sets the length.
		/// </summary>
		/// <param name='length'>
		/// The new length.
		/// </param>
		public override void SetLength (long length)
		{
			CheckDisposed ();

			if (BoundEnd == -1 || BoundStart + length > BoundEnd) {
				long end = Source.Length;

				if (BoundStart + length > end)
					Source.SetLength (BoundStart + length);
				
				BoundEnd = BoundStart + length;
			} else {
				BoundEnd = BoundStart + length;
			}
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			disposed = true;
		}
		
		#endregion
	}
}
