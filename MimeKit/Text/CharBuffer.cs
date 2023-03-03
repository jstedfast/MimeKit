// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
