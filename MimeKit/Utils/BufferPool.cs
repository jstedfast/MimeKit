//
// BufferPool.cs
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

// Code based on Microsoft's System.Buffer.DefaultArrayPoolBucket class located at
// https://github.com/dotnet/corefx/blob/master/src/System.Buffers/src/System/Buffers/DefaultArrayPoolBucket.cs

using System;
using System.Threading;
using System.Diagnostics;

namespace MimeKit.Utils
{
	/// <summary>
	/// Provides a pool of reusable buffers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Renting and returning buffers with a <see cref="BufferPool"/> can increase performance
	/// in situations where buffers are created and destroyed frequently, resulting in significant
	/// memory pressure on the garbage collector.
	/// </para>
	/// <para>
	/// This class is thread-safe.  All members may be used by multiple threads concurrently.
	/// </para>
	/// </remarks>
	class BufferPool
	{
		readonly byte[][] buffers;
		SpinLock spinLock;
		int index;

		/// <summary>
		/// Initialize a new instance of the <see cref="BufferPool"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new buffer pool.
		/// </remarks>
		/// <param name="bufferSize">The buffer size.</param>
		/// <param name="maxBufferCount">The maximum number of buffers that should be retained by the pool.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="bufferSize"/> is less than <c>1</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="maxBufferCount"/> is less than <c>1</c>.</para>
		/// </exception>
		public BufferPool (int bufferSize, int maxBufferCount)
		{
			if (bufferSize < 1)
				throw new ArgumentOutOfRangeException (nameof (bufferSize));

			if (maxBufferCount < 1)
				throw new ArgumentOutOfRangeException (nameof (maxBufferCount));

			buffers = new byte[maxBufferCount][];
			MaxBufferCount = maxBufferCount;
			BufferSize = bufferSize;

			spinLock = new SpinLock (Debugger.IsAttached);
		}

		/// <summary>
		/// Get the size of the buffers returned and/or retained by the pool.
		/// </summary>
		/// <remarks>
		/// Gets the size of the buffers returned and/or retained by the pool.
		/// </remarks>
		/// <value>The size of the buffer.</value>
		public int BufferSize {
			get; private set;
		}

		/// <summary>
		/// Get the maximum number of buffers that the pool should retain.
		/// </summary>
		/// <remarks>
		/// Gets the maximum number of buffers that the pool should retain.
		/// </remarks>
		/// <value>The max buffer count.</value>
		public int MaxBufferCount {
			get; private set;
		}

		/// <summary>
		/// Rent a buffer from the pool.
		/// </summary>
		/// <remarks>
		/// Returns a buffer from the pool. This buffer should later be returned back to the pool using
		/// <see cref="Return(byte[])"/> when the caller is finished using it so that it may be reused
		/// in subsequent uses of <see cref="Rent(bool)"/>.
		/// </remarks>
		/// <returns>The rented buffer.</returns>
		/// <param name="clear"><c>true if the buffer should be cleared; otherwise, <c>false</c></c>.</param>
		public byte[] Rent (bool clear = false)
		{
			byte[] buffer = null;
			bool locked = false;

			try {
				spinLock.Enter (ref locked);

				if (index < buffers.Length) {
					buffer = buffers[index];
					buffers[index] = null;
					index++;
				}
			} finally {
				if (locked)
					spinLock.Exit (false);
			}

			if (buffer is null)
				buffer = new byte[BufferSize];
			else if (clear)
				Array.Clear (buffer, 0, BufferSize);

			return buffer;
		}

		/// <summary>
		/// Return the specified buffer to the pool.
		/// </summary>
		/// <remarks>
		/// Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer
		/// and must not use it. The reference returned from a given call to <see cref="Rent(bool)"/> must
		/// only be returned via <see cref="Return(byte[])"/> once. The default <see cref="BufferPool"/>
		/// may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
		/// if it is determined that the pool already contains the maximum number of buffers as specified by
		/// <see cref="MaxBufferCount"/>.
		/// </remarks>
		/// <param name="buffer">The buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The size of the <paramref name="buffer"/> does not match <see cref="BufferSize"/>.
		/// </exception>
		public void Return (byte[] buffer)
		{
			bool locked = false;

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (buffer.Length != BufferSize)
				throw new ArgumentException ("The size of the buffer does not match the size used by the pool.", nameof (buffer));

			try {
				spinLock.Enter (ref locked);

				if (index > 0)
					buffers[--index] = buffer;
			} finally {
				if (locked)
					spinLock.Exit (false);
			}
		}
	}
}
