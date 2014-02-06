//
// SecCertificate.cs
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
	class SecCertificate : CFObject
	{
		const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

		[DllImport (SecurityLibrary)]
		static extern IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr data);

		[DllImport (SecurityLibrary)]
		static extern IntPtr SecCertificateCopyData (IntPtr certificate);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecCertificateCopyCommonName (IntPtr certificate, out IntPtr commonName);

		public SecCertificate (IntPtr handle, bool own) : base (handle, own)
		{
		}

		public SecCertificate (IntPtr handle) : base (handle, false)
		{
		}

		public static SecCertificate Create (CFData data)
		{
			return new SecCertificate (SecCertificateCreateWithData (IntPtr.Zero, data.Handle), true);
		}

		public static SecCertificate Create (byte[] rawData)
		{
			using (var data = new CFData (rawData)) {
				return Create (data);
			}
		}

		public CFData GetData ()
		{
			return new CFData (SecCertificateCopyData (Handle), true);
		}
	}
}
