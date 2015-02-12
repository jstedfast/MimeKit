//
// DigestAlgorithm.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
	/// <remarks>
	/// <para>Digest algorithms are secure hashing algorithms that are used
	/// to generate unique fixed-length signatures for arbitrary data.</para>
	/// <para>The most commonly used digest algorithms are currently MD5
	/// and SHA-1, however, MD5 was successfully broken in 2008 and should
	/// be avoided. In late 2013, Microsoft announced that they would be
	/// retiring their use of SHA-1 in their products by 2016 with the
	/// assumption that its days as an unbroken digest algorithm were
	/// numbered. It is speculated that the SHA-1 digest algorithm will
	/// be vulnerable to collisions, and thus no longer considered secure,
	/// by 2018.</para>
	/// <para>Microsoft and other vendors plan to move to the SHA-2 suite of
	/// digest algorithms which includes the following 4 variants: SHA-224,
	/// SHA-256, SHA-384, and SHA-512.</para>
	/// </remarks>
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
		/// The SHA-1 digest algorithm.
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
		/// The SHA-256 digest algorithm.
		/// </summary>
		Sha256      = 8,

		/// <summary>
		/// The SHA-384 digest algorithm.
		/// </summary>
		Sha384      = 9,

		/// <summary>
		/// The SHA-512 digest algorithm.
		/// </summary>
		Sha512      = 10,

		/// <summary>
		/// The SHA-224 digest algorithm.
		/// </summary>
		Sha224      = 11,

		/// <summary>
		/// The MD4 digest algorithm.
		/// </summary>
		MD4         = 301
	}
}
