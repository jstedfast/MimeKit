//
// CFString.cs
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
	class CFString : CFObject
	{
		string cached;

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, int count);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static int CFStringGetLength (IntPtr handle);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);

		public CFString (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			Handle = CFStringCreateWithCharacters (IntPtr.Zero, str, str.Length);
			this.cached = str;
		}

		public CFString (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public CFString (IntPtr handle) : base (handle, false)
		{
		}

		static string CFStringGetString (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			string str;

			int length = CFStringGetLength (handle);
			IntPtr unicode = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;

			if (unicode == IntPtr.Zero) {
				CFRange range = new CFRange (0, length);
				buffer = Marshal.AllocCoTaskMem (length * 2);
				CFStringGetCharacters (handle, range, buffer);
				unicode = buffer;
			}

			unsafe {
				str = new string ((char *) unicode, 0, length);
			}

			if (buffer != IntPtr.Zero)
				Marshal.FreeCoTaskMem (buffer);

			return str;
		}

		public static implicit operator string (CFString str)
		{
			return str.ToString ();
		}

		public static implicit operator CFString (string str)
		{
			return new CFString (str);
		}

		public int Length {
			get {
				if (cached != null)
					return cached.Length;

				return CFStringGetLength (Handle);
			}
		}

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static char CFStringGetCharacterAtIndex (IntPtr handle, int p);

		public char this [int index] {
			get {
				if (cached != null)
					return cached[index];

				return CFStringGetCharacterAtIndex (Handle, index);
			}
		}

		public override string ToString ()
		{
			if (cached == null)
				cached = CFStringGetString (Handle);

			return cached;
		}
	}
}
