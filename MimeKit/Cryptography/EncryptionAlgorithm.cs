//
// EncryptionAlgorithm.cs
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
	/// Encryption algorithms supported by S/MIME and OpenPGP.
	/// </summary>
	/// <remarks>
	/// <para>Represents the available encryption algorithms for use with S/MIME and OpenPGP.</para>
	/// <para>RC-2/40 was required by all S/MIME v2 implementations. However, since the
	/// mid-to-late 1990's, RC-2/40 has been considered to be extremely weak and starting with
	/// S/MIME v3.0 (published in 1999), all S/MIME implementations are required to implement
	/// support for Triple-DES (aka 3DES) and should no longer encrypt using RC-2/40 unless
	/// explicitly requested to do so by the user.</para>
	/// <para>These days, most S/MIME implementations support the AES-128 and AES-256
	/// algorithms which are the recommended algorithms specified in S/MIME v3.2 and
	/// should be preferred over the use of Triple-DES unless the client capabilities
	/// of one or more of the recipients is unknown (or only supports Triple-DES).</para>
	/// </remarks>
	public enum EncryptionAlgorithm {
		/// <summary>
		/// The AES 128-bit encryption algorithm.
		/// </summary>
		Aes128,

		/// <summary>
		/// The AES 192-bit encryption algorithm.
		/// </summary>
		Aes192,

		/// <summary>
		/// The AES 256-bit encryption algorithm.
		/// </summary>
		Aes256,

		/// <summary>
		/// The Camellia 128-bit encryption algorithm.
		/// </summary>
		Camellia128,

		/// <summary>
		/// The Camellia 192-bit encryption algorithm.
		/// </summary>
		Camellia192,

		/// <summary>
		/// The Camellia 256-bit encryption algorithm.
		/// </summary>
		Camellia256,

		/// <summary>
		/// The Cast-5 128-bit encryption algorithm.
		/// </summary>
		Cast5,

		/// <summary>
		/// The DES 56-bit encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is extremely weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		Des,

		/// <summary>
		/// The Triple-DES encryption algorithm.
		/// </summary>
		/// <remarks>
		/// This is the weakest recommended encryption algorithm for use
		/// starting with S/MIME v3 and should only be used as a fallback
		/// if it is unknown what encryption algorithms are supported by
		/// the recipient's mail client.
		/// </remarks>
		TripleDes,

		/// <summary>
		/// The IDEA 128-bit encryption algorithm.
		/// </summary>
		Idea,

		/// <summary>
		/// The blowfish encryption algorithm (OpenPGP only).
		/// </summary>
		Blowfish,

		/// <summary>
		/// The twofish encryption algorithm (OpenPGP only).
		/// </summary>
		Twofish,

		/// <summary>
		/// The RC2 40-bit encryption algorithm (S/MIME only).
		/// </summary>
		/// <remarks>
		/// This is extremely weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		RC240,

		/// <summary>
		/// The RC2 64-bit encryption algorithm (S/MIME only).
		/// </summary>
		/// <remarks>
		/// This is very weak encryption and should not be used
		/// without consent from the user.
		/// </remarks>
		RC264,

		/// <summary>
		/// The RC2 128-bit encryption algorithm (S/MIME only).
		/// </summary>
		RC2128,
	}
}
