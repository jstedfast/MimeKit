//
// PublicKeyAlgorithm.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An enumeration of public key algorithms.
	/// </summary>
	/// <remarks>
	/// An enumeration of public key algorithms.
	/// </remarks>
	public enum PublicKeyAlgorithm {
		/// <summary>
		/// No public key algorithm specified.
		/// </summary>
		None             = 0,

		/// <summary>
		/// The RSA algorithm.
		/// </summary>
		RsaGeneral       = 1,

		/// <summary>
		/// The RSA encryption-only algorithm.
		/// </summary>
		RsaEncrypt       = 2,

		/// <summary>
		/// The RSA sign-only algorithm.
		/// </summary>
		RsaSign          = 3,

		/// <summary>
		/// The El-Gamal encryption-only algorithm.
		/// </summary>
		ElGamalEncrypt   = 16,

		/// <summary>
		/// The DSA algorithm.
		/// </summary>
		Dsa              = 17,

		/// <summary>
		/// The elliptic curve algorithm.
		/// </summary>
		EllipticCurve    = 18,

		/// <summary>
		/// The elliptic curve DSA algorithm.
		/// </summary>
		EllipticCurveDsa = 19,

		/// <summary>
		/// The El-Gamal algorithm.
		/// </summary>
		ElGamalGeneral   = 20,

		/// <summary>
		/// The Diffie-Hellman algorithm.
		/// </summary>
		DiffieHellman    = 21,
	}
}
