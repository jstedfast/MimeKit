//
// NonSeekableStream.cs
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

namespace UnitTests.IO {
	/// <summary>
	/// A read-only stream wrapper that cannot seek and does not expose its <see cref="Length"/> or
	/// <see cref="Position"/>.
	/// </summary>
	sealed class NonSeekableStream : Stream
	{
		readonly Stream source;

		public NonSeekableStream (Stream source)
		{
			this.source = source;
		}

		public NonSeekableStream (byte[] data) : this (new MemoryStream (data, false))
		{
		}

		public bool IsDisposed { get; private set; }

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException ();
		public override long Position { get => throw new NotSupportedException (); set => throw new NotSupportedException (); }

		public override int Read (byte[] buffer, int offset, int count) => source.Read (buffer, offset, count);
		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken) => source.ReadAsync (buffer, offset, count, cancellationToken);
		public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();
		public override void SetLength (long value) => throw new NotSupportedException ();
		public override void Write (byte[] buffer, int offset, int count) => throw new NotSupportedException ();
		public override void Flush () { }

		protected override void Dispose (bool disposing)
		{
			IsDisposed = true;
			base.Dispose (disposing);
		}
	}
}
