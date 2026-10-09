//
// ThrowingReadStream.cs
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
	/// A seekable stream whose reads (sync and async) throw once the stream position reaches a given offset.
	/// By default, an <see cref="IOException"/> with the message "Read failed." is thrown.
	/// </summary>
	/// <remarks>
	/// The default constructor creates a non-empty stream that throws on the very first read.
	/// </remarks>
	class ThrowingReadStream : MemoryStream
	{
		static readonly Func<Exception> DefaultException = () => new IOException ("Read failed.");

		readonly Func<Exception> createException;
		readonly long throwOffset;
		readonly int chunkSize;
		int seeks;

		public ThrowingReadStream () : this (DefaultException)
		{
		}

		public ThrowingReadStream (Func<Exception> createException) : this (new byte[1], 0, int.MaxValue, createException)
		{
		}

		/// <summary>
		/// Create a stream that successfully returns the content of <paramref name="buffer"/> (at most
		/// <paramref name="chunkSize"/> bytes per read) up to <paramref name="throwOffset"/> and then throws.
		/// </summary>
		public ThrowingReadStream (byte[] buffer, long throwOffset, int chunkSize = int.MaxValue, Func<Exception> createException = null) : base (buffer, false)
		{
			this.createException = createException ?? DefaultException;
			this.throwOffset = throwOffset;
			this.chunkSize = chunkSize;
		}

		/// <summary>
		/// The number of calls to <see cref="Seek"/> that succeed before subsequent seeks throw
		/// an <see cref="IOException"/>. Defaults to unlimited.
		/// </summary>
		public int SeekLimit { get; set; } = int.MaxValue;

		public bool IsDisposed { get; private set; }

		int GetCount (int count)
		{
			if (Position >= throwOffset)
				throw createException ();

			return (int) Math.Min (Math.Min (count, chunkSize), throwOffset - Position);
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return base.Read (buffer, offset, GetCount (count));
		}

		public override int Read (Span<byte> buffer)
		{
			return base.Read (buffer.Slice (0, GetCount (buffer.Length)));
		}

		public override int ReadByte ()
		{
			GetCount (1);

			return base.ReadByte ();
		}

		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return base.ReadAsync (buffer, offset, GetCount (count), cancellationToken);
		}

		public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			return base.ReadAsync (buffer.Slice (0, GetCount (buffer.Length)), cancellationToken);
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			if (seeks++ >= SeekLimit)
				throw new IOException ("Seek failed.");

			return base.Seek (offset, loc);
		}

		protected override void Dispose (bool disposing)
		{
			IsDisposed = true;
			base.Dispose (disposing);
		}
	}
}
