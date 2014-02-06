//
// CFData.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Runtime.InteropServices;

namespace MimeKit.MacInterop {
	class CFData : CFObject
	{
		byte[] cached;

		[DllImport (CoreFoundationLibrary)]
		extern static int CFDataGetLength (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static void CFDataGetBytes (IntPtr handle, CFRange range, IntPtr buffer);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDataCreate (IntPtr allocator, byte[] buffer, int length);

		static byte[] CFDataGetBytes (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			int length = CFDataGetLength (handle);
			if (length < 1)
				return null;

			var buffer = new byte[length];
			unsafe {
				fixed (byte *bufptr = buffer) {
					CFDataGetBytes (handle, new CFRange (0, length), (IntPtr) bufptr);
				}
			}

			return buffer;
		}

		public CFData (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public CFData (IntPtr handle) : base (handle, false)
		{
		}

		public CFData (byte[] buffer)
		{
			Handle = CFDataCreate (IntPtr.Zero, buffer, buffer.Length);
			cached = buffer;
		}

		public byte[] GetBuffer ()
		{
			if (cached == null)
				cached = CFDataGetBytes (Handle);

			return cached;
		}
	}
}
