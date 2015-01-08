//
// PackedByteArray.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Utils {
	class PackedByteArray : IList<byte>
	{
		const int InitialBufferSize = 64;

		ushort[] buffer;
		int length;
		int cursor;

		public PackedByteArray ()
		{
			buffer = new ushort[InitialBufferSize];
			Clear ();
		}

		#region ICollection implementation

		public int Count {
			get { return length; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		void EnsureBufferSize (int size)
		{
			if (buffer.Length > size)
				return;

			int ideal = (size + 63) & ~63;

			Array.Resize<ushort> (ref buffer, ideal);
		}

		public void Add (byte item)
		{
			if (cursor < 0 || item != (byte) (buffer[cursor] & 0xFF) || (buffer[cursor] & 0xFF00) == 0xFF00) {
				EnsureBufferSize (cursor + 2);
				buffer[++cursor] = (ushort) ((1 << 8) | item);
			} else {
				buffer[cursor] += (1 << 8);
			}

			length++;
		}
		
		public void Clear ()
		{
			cursor = -1;
			length = 0;
		}
		
		public bool Contains (byte item)
		{
			for (int i = 0; i <= cursor; i++) {
				if (item == (byte) (buffer[i] & 0xFF))
					return true;
			}

			return false;
		}
		
		public void CopyTo (byte[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (arrayIndex < 0 || arrayIndex + length > array.Length)
				throw new ArgumentOutOfRangeException ("arrayIndex");

			int index = arrayIndex;
			int count;
			byte c;

			for (int i = 0; i <= cursor; i++) {
				count = (buffer[i] >> 8) & 0xFF;
				c = (byte) (buffer[i] & 0xFF);

				for (int n = 0; n < count; n++)
					array[index++] = c;
			}
		}
		
		public bool Remove (byte item)
		{
			int count = 0;
			int i;
			
			// find the index of the element we need to remove
			for (i = 0; i <= cursor; i++) {
				if (item == (byte) (buffer[i] & 0xFF)) {
					count = ((buffer[i] >> 8) & 0xFF);
					break;
				}
			}

			if (i > cursor)
				return false;

			if (count > 1) {
				// this byte was repeated more than once, so just decrement the count
				buffer[i] = (ushort) (((count - 1) << 8) | item);
			} else if (i < cursor) {
				// to remove the element at position i, we need to shift the
				// remaining data one item to the left
				Array.Copy (buffer, i + 1, buffer, i, cursor - i);
				cursor--;
			} else {
				// removing the last byte added
				cursor--;
			}
			
			length--;

			return true;
		}

		#endregion

		#region IList implementation

		public int IndexOf (byte item)
		{
			int offset = 0;

			for (int i = 0; i <= cursor; i++) {
				if (item == (byte) (buffer[i] & 0xFF))
					return offset;

				offset += ((buffer[i] >> 8) & 0xFF);
			}

			return -1;
		}

		public void Insert (int index, byte item)
		{
			throw new NotSupportedException ();
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index > length)
				throw new ArgumentOutOfRangeException ("index");

			int offset = 0;
			int count = 0;
			int i;

			// find the index of the element we need to remove
			for (i = 0; i <= cursor; i++) {
				count = ((buffer[i] >> 8) & 0xFF);
				if (offset + count > index)
					break;

				offset += count;
			}

			if (count > 1) {
				// this byte was repeated more than once, so just decrement the count
				byte c = (byte) (buffer[i] & 0xFF);
				buffer[i] = (ushort) (((count - 1) << 8) | c);
			} else if (i < cursor) {
				// to remove the element at position i, we need to shift the
				// remaining data one item to the left
				Array.Copy (buffer, i + 1, buffer, i, cursor - i);
				cursor--;
			} else {
				// removing the last byte added
				cursor--;
			}

			length--;
		}

		public byte this [int index] {
			get {
				if (index < 0 || index > length)
					throw new ArgumentOutOfRangeException ("index");

				int offset = 0;
				int count, i;

				for (i = 0; i <= cursor; i++) {
					count = ((buffer[i] >> 8) & 0xFF);
					if (offset + count > index)
						break;
					
					offset += count;
				}

				return (byte) (buffer[i] & 0xFF);
			}
			set {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<byte> GetEnumerator ()
		{
			int count;
			byte c;
			
			for (int i = 0; i <= cursor; i++) {
				count = (buffer[i] >> 8) & 0xFF;
				c = (byte) (buffer[i] & 0xFF);
				
				for (int n = 0; n < count; n++)
					yield return c;
			}

			yield break;
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}

