//
// ChunkedReadStream.cs
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
	/// Returns at most <c>chunkSize</c> bytes per read (both sync and async) to force lines, line endings
	/// and boundary markers to be split across reads.
	/// </summary>
	class ChunkedReadStream : MemoryStream
	{
		readonly int chunkSize;

		public ChunkedReadStream (byte[] buffer, int chunkSize) : base (buffer, false)
		{
			this.chunkSize = chunkSize;
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return base.Read (buffer, offset, Math.Min (count, chunkSize));
		}

		public override int Read (Span<byte> buffer)
		{
			return base.Read (buffer.Slice (0, Math.Min (buffer.Length, chunkSize)));
		}

		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return base.ReadAsync (buffer, offset, Math.Min (count, chunkSize), cancellationToken);
		}

		public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			return base.ReadAsync (buffer.Slice (0, Math.Min (buffer.Length, chunkSize)), cancellationToken);
		}
	}
}
