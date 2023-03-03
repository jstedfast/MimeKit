// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;

namespace MimeKit.Utils {
	internal ref struct ByteArrayBuilder
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
