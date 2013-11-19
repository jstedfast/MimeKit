//
// EncryptionAlgorithm.cs
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
	/// Encryption algorithms supported by S/MIME.
	/// </summary>
	public enum EncryptionAlgorithm {
		/// <summary>
		/// The client supports the AES 128-bit encryption algorithm.
		/// </summary>
		Aes128,

		/// <summary>
		/// The client supports the AES 192-bit encryption algorithm.
		/// </summary>
		Aes192,

		/// <summary>
		/// The client supports the AES 256-bit encryption algorithm.
		/// </summary>
		Aes256,

		/// <summary>
		/// The client supports the Camellia 128-bit encryption algorithm.
		/// </summary>
		Camellia128,

		/// <summary>
		/// The client supports the Camellia 192-bit encryption algorithm.
		/// </summary>
		Camellia192,

		/// <summary>
		/// The client supports the Camellia 256-bit encryption algorithm.
		/// </summary>
		Camellia256,

		/// <summary>
		/// The client supports the Cast-5 128-bit encryption algorithm.
		/// </summary>
		Cast5,

		/// <summary>
		/// The client supports the DES 56-bit encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is extremely weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		Des,

		/// <summary>
		/// The client supports the Triple-DES encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is the weakest recommended encryption algorithm for use
		/// starting with S/MIME v3 and should only be used as a fallback
		/// if it is unknown what encryption algorithms are supported by
		/// the recipient's mail client.
		/// </remarks>
		TripleDes,

		/// <summary>
		/// The client supports the IDEA 128-bit encryption algorithm.
		/// </summary>
		Idea,

		/// <summary>
		/// The client supports the RC2 40-bit encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is extremely weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		RC240,

		/// <summary>
		/// The client supports the RC2 64-bit encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is very weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		RC264,

		/// <summary>
		/// The client supports the RC2 128-bit encryption algorithm.
		/// </summary>
		RC2128,
	}
}
