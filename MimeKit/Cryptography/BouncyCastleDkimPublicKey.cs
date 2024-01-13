//
// BouncyCastleDkimPublicKey.cs
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

using System;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM public key implemented using BouncyCastle.
	/// </summary>
	/// <remarks>
	/// A DKIM public key implemented using BouncyCastle.
	/// </remarks>
	public class BouncyCastleDkimPublicKey : BouncyCastleDkimKey, IDkimPublicKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BouncyCastleDkimPublicKey"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleDkimPublicKey"/>.
		/// </remarks>
		/// <param name="key">The public key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a public key.
		/// </exception>
		public BouncyCastleDkimPublicKey (AsymmetricKeyParameter key)
		{
			if (key is null)
				throw new ArgumentNullException (nameof (key));

			if (key.IsPrivate)
				throw new ArgumentException ("The key must be a public key.", nameof (key));

			Key = key;
		}

		/// <summary>
		/// Get the size, in bits, of the key modulus used by the asymmetric algorithm.
		/// </summary>
		/// <remarks>
		/// Gets the size, in bits, of the key modulus used by the asymmetric algorithm.
		/// </remarks>
		/// <value>The size, in bits, of the key modulus.</value>
		public int KeySize {
			get {
				if (Key is RsaKeyParameters rsa)
					return rsa.Modulus.BitLength;

				return 0;
			}
		}

		/// <summary>
		/// Get the public key algorithm.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm.
		/// </remarks>
		/// <value>The public key algorithm.</value>
		public DkimPublicKeyAlgorithm Algorithm {
			get {
				if (Key is RsaKeyParameters)
					return DkimPublicKeyAlgorithm.Rsa;

				if (Key is Ed25519PublicKeyParameters)
					return DkimPublicKeyAlgorithm.Ed25519;

				return DkimPublicKeyAlgorithm.Unknown;
			}
		}

		/// <summary>
		/// Create a DKIM signature context suitable for verifying a signature.
		/// </summary>
		/// <remarks>
		/// Creates a DKIM signature context suitable for verifying a signature.
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		/// <returns>The DKIM signature context.</returns>
		/// <exception cref="System.NotSupportedException">
		/// The specified <paramref name="algorithm"/> is not supported.
		/// </exception>
		public IDkimSignatureContext CreateVerifyContext (DkimSignatureAlgorithm algorithm)
		{
			return CreateSignatureContext (algorithm, false);
		}
	}
}
