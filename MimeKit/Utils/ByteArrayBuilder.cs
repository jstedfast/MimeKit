//
// ByteArrayBuilder.cs
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

using System;
using System.Buffers;

namespace MimeKit.Utils {
	ref struct ByteArrayBuilder
	{
		byte[] buffer;
		int length;

		public ByteArrayBuilder (int initialCapacity)
		{
			buffer = ArrayPool<byte>.Shared.Rent (initialCapacity);
			length = 0;
		}

		void EnsureCapacity (int capacity)
		{
			if (capacity > buffer.Length) {
				var resized = ArrayPool<byte>.Shared.Rent (capacity);
				Buffer.BlockCopy (buffer, 0, resized, 0, length);
				ArrayPool<byte>.Shared.Return (buffer);
				buffer = resized;
			}
		}

		public void Append (byte c)
		{
			EnsureCapacity (length + 1);

			buffer[length++] = c;
		}

		public void Append (byte[] text, int startIndex, int count)
		{
			EnsureCapacity (length + count);

			Buffer.BlockCopy (text, startIndex, buffer, length, count);
			length += count;
		}

		public byte[] ToArray ()
		{
			var array = new byte[length];

			Buffer.BlockCopy (buffer, 0, array, 0, length);

			return array;
		}

		public void Dispose ()
		{
			if (buffer != null) {
				ArrayPool<byte>.Shared.Return (buffer);
				buffer = null;
			}
		}
	}
}
