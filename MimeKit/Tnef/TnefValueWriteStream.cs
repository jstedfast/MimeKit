//
// TnefValueWriteStream.cs
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
using System.Threading;
using System.Threading.Tasks;

using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A write-only stream that appends an attribute or property value to an in-memory buffer.
	/// </summary>
	/// <remarks>
	/// The owner is notified when the stream is disposed so that it can complete the value.
	/// </remarks>
	sealed class TnefValueWriteStream : Stream
	{
		readonly Action<TnefValueWriteStream>? closed;
		readonly Stream buffer;
		long length;
		bool disposed;

		public TnefValueWriteStream (Stream buffer, Action<TnefValueWriteStream>? closed)
		{
			this.buffer = buffer;
			this.closed = closed;
		}

		public override bool CanRead {
			get { return false; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return !disposed; }
		}

		public override long Length {
			get { return length; }
		}

		public override long Position {
			get { return length; }
			set { throw new NotSupportedException (); }
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefValueWriteStream));
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
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
		public override void Write (byte[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);
			CheckDisposed ();

			this.buffer.Write (buffer, offset, count);
			length += count;
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			// Note: The buffer is in memory, so there is nothing to wait for.
			Write (buffer, offset, count);

			return Task.CompletedTask;
		}

		public override void Flush ()
		{
			CheckDisposed ();
		}

		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();
			CheckDisposed ();

			return Task.CompletedTask;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;
				closed?.Invoke (this);
			}

			base.Dispose (disposing);
		}
	}
}
