//
// CFDictionary.cs
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
	class CFDictionary : CFObject
	{
		public static IntPtr KeyCallbacks;
		public static IntPtr ValueCallbacks;

		static CFDictionary ()
		{
			var lib = Dlfcn.dlopen (CoreFoundationLibrary, 0);

			try {
				KeyCallbacks = Dlfcn.GetIndirect (lib, "kCFTypeDictionaryKeyCallBacks");
				ValueCallbacks = Dlfcn.GetIndirect (lib, "kCFTypeDictionaryValueCallBacks");
			} finally {
				Dlfcn.dlclose (lib);
			}
		}

		public CFDictionary (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public CFDictionary (IntPtr handle) : base (handle, false)
		{
		}

		public static CFDictionary FromObjectAndKey (CFObject obj, CFObject key)
		{
			return new CFDictionary (CFDictionaryCreate (IntPtr.Zero, new IntPtr[] { key.Handle }, new IntPtr [] { obj.Handle }, 1, KeyCallbacks, ValueCallbacks), true);
		}

		public static CFDictionary FromObjectsAndKeys (CFObject[] objects, CFObject[] keys)
		{
			if (objects == null)
				throw new ArgumentNullException ("objects");

			if (keys == null)
				throw new ArgumentNullException ("keys");

			if (objects.Length != keys.Length)
				throw new ArgumentException ("The length of both arrays must be the same");

			IntPtr [] k = new IntPtr [keys.Length];
			IntPtr [] v = new IntPtr [keys.Length];

			for (int i = 0; i < k.Length; i++) {
				k [i] = keys [i].Handle;
				v [i] = objects [i].Handle;
			}

			return new CFDictionary (CFDictionaryCreate (IntPtr.Zero, k, v, k.Length, KeyCallbacks, ValueCallbacks), true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryCreate (IntPtr allocator, IntPtr[] keys, IntPtr[] vals, int len, IntPtr keyCallbacks, IntPtr valCallbacks);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryGetValue (IntPtr theDict, IntPtr key);
		public static IntPtr GetValue (IntPtr theDict, IntPtr key)
		{
			return CFDictionaryGetValue (theDict, key);
		}

//		public static bool GetBooleanValue (IntPtr theDict, IntPtr key)
//		{
//			var value = GetValue (theDict, key);
//			if (value == IntPtr.Zero)
//				return false;
//			return CFBoolean.GetValue (value);
//		}

		public string GetStringValue (string key)
		{
			using (var str = new CFString (key)) {
				using (var value = new CFString (CFDictionaryGetValue (Handle, str.Handle))) {
					return value.ToString ();
				}
			}
		}

		public int GetInt32Value (string key)
		{
			int value = 0;
			using (var str = new CFString (key)) {
				if (!CFNumberGetValue (CFDictionaryGetValue (Handle, str.Handle), /* kCFNumberSInt32Type */ 3, out value))
					throw new System.Collections.Generic.KeyNotFoundException (string.Format ("Key {0} not found", key));
				return value;
			}
		}

		public IntPtr GetIntPtrValue (string key)
		{
			using (var str = new CFString (key)) {
				return CFDictionaryGetValue (Handle, str.Handle);
			}
		}

		public CFDictionary GetDictionaryValue (string key)
		{
			using (var str = new CFString (key)) {
				var ptr = CFDictionaryGetValue (Handle, str.Handle);
				return ptr == IntPtr.Zero ? null : new CFDictionary (ptr);
			}
		}

		public bool ContainsKey (string key)
		{
			using (var str = new CFString (key)) {
				return CFDictionaryContainsKey (Handle, str.Handle);
			}
		}

		[DllImport (CoreFoundationLibrary)]
		static extern bool CFNumberGetValue (IntPtr number, int theType, out int value);

		[DllImport (CoreFoundationLibrary)]
		extern static bool CFDictionaryContainsKey (IntPtr theDict, IntPtr key);
	}

	class CFMutableDictionary : CFDictionary
	{
		[DllImport (CoreFoundationLibrary)]
		static extern IntPtr CFDictionaryCreateMutable (IntPtr allocator, IntPtr capacity, IntPtr keyCallBacks, IntPtr valueCallBacks);

		// void CFDictionaryAddValue (CFMutableDictionaryRef theDict, const void *key, const void *value);

		[DllImport (CoreFoundationLibrary)]
		extern static void CFDictionarySetValue (IntPtr theDict, IntPtr key, IntPtr value);

		public CFMutableDictionary (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public CFMutableDictionary (IntPtr handle) : base (handle, false)
		{
		}

		public void SetValue (IntPtr key, IntPtr value)
		{
			CFDictionarySetValue (Handle, key, value);
		}

//		public void SetValue (IntPtr key, bool value)
//		{
//			SetValue (key, value ? CFBoolean.True.Handle : CFBoolean.False.Handle);
//		}
	}
}
