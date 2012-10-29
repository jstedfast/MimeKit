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

namespace MimeKit
{
	public class Substream : Stream
	{
		long position;
		bool disposed;
		bool eos;

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

		public Stream Source {
			get; private set;
		}

		public long BoundStart {
			get; private set;
		}

		public long BoundEnd {
			get; private set;
		}

		#region Stream implementation

		public override bool CanRead {
			get { return Source.CanRead; }
		}

		public override bool CanWrite {
			get { return Source.CanWrite; }
		}

		public override bool CanSeek {
			get { return Source.CanSeek; }
		}

		public override bool CanTimeout {
			get { return Source.CanTimeout; }
		}

		public override long Length {
			get {
				CheckDisposed ();

				if (BoundEnd != -1)
					return BoundEnd - BoundStart;

				return Source.Length - BoundStart;
			}
		}

		public override long Position {
			get { return position; }
			set {
				CheckDisposed ();
				CheckCanSeek ();

				Seek (value, SeekOrigin.Begin);
			}
		}

		public override int ReadTimeout {
			get { return Source.ReadTimeout; }
			set { Source.ReadTimeout = value; }
		}

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

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckDisposed ();
			CheckCanRead ();

			return base.BeginRead (buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckDisposed ();
			CheckCanWrite ();

			return base.BeginWrite (buffer, offset, count, callback, state);
		}

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

		public override void Flush ()
		{
			CheckDisposed ();
			CheckCanWrite ();

			Source.Flush ();
		}

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
