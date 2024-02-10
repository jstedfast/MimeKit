//
// ReadOneByteStream.cs
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

namespace UnitTests.IO {
	class ReadOneByteStream : Stream
	{
		readonly Stream source;

		public ReadOneByteStream (Stream source)
		{
			this.source = source;
		}

		public override bool CanRead {
			get { return source.CanRead; }
		}

		public override bool CanWrite {
			get { return source.CanWrite; }
		}

		public override bool CanSeek {
			get { return source.CanSeek; }
		}

		public override long Length {
			get { return source.Length; }
		}

		public override long Position {
			get { return source.Position; }
			set { source.Position = value; }
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return source.Read (buffer, offset, 1);
		}

		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return source.ReadAsync (buffer, offset, count, cancellationToken);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			source.Write (buffer, offset, count);
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return source.WriteAsync (buffer, offset, count, cancellationToken);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return source.Seek (offset, origin);
		}

		public override void Flush ()
		{
			source.Flush ();
		}

		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			return source.FlushAsync (cancellationToken);
		}

		public override void SetLength (long value)
		{
			source.SetLength (value);
		}
	}
}
