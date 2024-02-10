//
// CharBuffer.cs
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
using System.Runtime.CompilerServices;

namespace MimeKit.Text {
	class CharBuffer
	{
		char[] buffer;

		public CharBuffer (int capacity)
		{
			buffer = new char[capacity];
		}

		public int Length {
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			set;
		}

		public char this[int index] {
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			get { return buffer[index]; }
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			set { buffer[index] = value; }
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		void EnsureCapacity (int length)
		{
			if (length < buffer.Length)
				return;

			int capacity = buffer.Length << 1;
			while (capacity <= length)
				capacity <<= 1;

			Array.Resize (ref buffer, capacity);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public void Append (char c)
		{
			EnsureCapacity (Length + 1);
			buffer[Length++] = c;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public void Append (string str)
		{
			EnsureCapacity (Length + str.Length);
			str.CopyTo (0, buffer, Length, str.Length);
			Length += str.Length;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public override string ToString ()
		{
			return new string (buffer, 0, Length);
		}

		//public static implicit operator string (CharBuffer buffer)
		//{
		//	return buffer.ToString ();
		//}
	}
}
