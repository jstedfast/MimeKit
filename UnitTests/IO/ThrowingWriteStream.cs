//
// ThrowingWriteStream.cs
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
	/// A stream whose writes (sync and async) always throw. By default an <see cref="IOException"/>
	/// with the message "Write failed." is thrown.
	/// </summary>
	class ThrowingWriteStream : MemoryStream
	{
		readonly Func<Exception> createException;

		public ThrowingWriteStream () : this (() => new IOException ("Write failed."))
		{
		}

		public ThrowingWriteStream (Func<Exception> createException)
		{
			this.createException = createException;
		}

		public bool IsDisposed { get; private set; }

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw createException ();
		}

		public override void Write (ReadOnlySpan<byte> buffer)
		{
			throw createException ();
		}

		public override void WriteByte (byte value)
		{
			throw createException ();
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			throw createException ();
		}

		public override ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			throw createException ();
		}

		protected override void Dispose (bool disposing)
		{
			IsDisposed = true;
			base.Dispose (disposing);
		}
	}
}
