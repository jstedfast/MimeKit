//
// RsaEncryptionPaddingScheme.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The RSA encryption padding schemes used by S/MIME.
	/// </summary>
	/// <remarks>
	/// The RSA encryption padding schemes used by S/MIME as described in
	/// <a href="https://tools.ietf.org/html/rfc8017">rfc8017</a>.
	/// </remarks>
	public enum RsaEncryptionPaddingScheme
	{
		/// <summary>
		/// The PKCS #1 v1.5 encryption padding scheme.
		/// </summary>
		Pkcs1,

		/// <summary>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme.
		/// </summary>
		Oaep
	}
}
