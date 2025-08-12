﻿//
// IApplicationPkcs7Signature.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for an S/MIME part with a Content-Type of application/pkcs7-signature.
	/// </summary>
	/// <remarks>
	/// <para>An application/pkcs7-signature part contains detatched pkcs7 signature data
	/// and is typically contained within a <see cref="MultipartSigned"/> part.</para>
	/// <para>To verify the signature, use one of the
	/// <a href="Overload_MimeKit_Cryptography_MultipartSigned_Verify.htm">Verify</a>
	/// methods on the parent multipart/signed part.</para>
	/// </remarks>
	public interface IApplicationPkcs7Signature : IMimePart
	{
	}
}
