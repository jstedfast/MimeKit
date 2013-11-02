//
// DigestAlgorithm.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A digest algorithm.
	/// </summary>
	public enum DigestAlgorithm {
		/// <summary>
		/// No digest algorithm specified.
		/// </summary>
		None        = 0,

		/// <summary>
		/// The MD5 digest algorithm.
		/// </summary>
		MD5         = 1,

		/// <summary>
		/// The SHA1 digest algorithm.
		/// </summary>
		Sha1        = 2,

		/// <summary>
		/// The Ripe-MD/160 digest algorithm.
		/// </summary>
		RipeMD160   = 3,

		/// <summary>
		/// The double-SHA digest algorithm.
		/// </summary>
		DoubleSha   = 4,

		/// <summary>
		/// The MD2 digest algorithm.
		/// </summary>
		MD2         = 5,

		/// <summary>
		/// The TIGER/192 digest algorithm.
		/// </summary>
		Tiger192    = 6,

		/// <summary>
		/// The HAVAL 5-pass 160-bit digest algorithm.
		/// </summary>
		Haval5160   = 7,

		/// <summary>
		/// The SHA256 digest algorithm.
		/// </summary>
		Sha256      = 8,

		/// <summary>
		/// The SHA384 digest algorithm.
		/// </summary>
		Sha384      = 9,

		/// <summary>
		/// The SHA512 digest algorithm.
		/// </summary>
		Sha512      = 10,

		/// <summary>
		/// The SHA224 digest algorithm.
		/// </summary>
		Sha224      = 11,

		/// <summary>
		/// The MD4 digest algorithm.
		/// </summary>
		MD4         = 301
	}
}
