//
// BouncyCastleDkimKey.cs
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
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A base class for <see cref="BouncyCastleDkimPublicKey"/> and <see cref="BouncyCastleDkimPrivateKey" />.
	/// </summary>
	/// <remarks>
	/// A base class for <see cref="BouncyCastleDkimPublicKey"/> and <see cref="BouncyCastleDkimPrivateKey" />.
	/// </remarks>
	public abstract class BouncyCastleDkimKey
	{
		/// <summary>
		/// Get the private key.
		/// </summary>
		/// <remarks>
		/// Gets the private key.
		/// </remarks>
		public AsymmetricKeyParameter Key {
			get; protected set;
		}

		/// <summary>
		/// Create a DKIM signature context.
		/// </summary>
		/// <remarks>
		/// Creates a DKIM signature context.
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		/// <param name="sign">If set to <c>true</c>, the context will be used for signing; otherwise, it will be used for verifying.</param>
		/// <returns>The DKIM signature context.</returns>
		/// <exception cref="NotSupportedException">
		/// The specified <paramref name="algorithm"/> is not supported.
		/// </exception>
		protected IDkimSignatureContext CreateSignatureContext (DkimSignatureAlgorithm algorithm, bool sign)
		{
			ISigner signer;

			switch (algorithm) {
			case DkimSignatureAlgorithm.RsaSha1:
				signer = new RsaDigestSigner (new Sha1Digest ());
				break;
			case DkimSignatureAlgorithm.RsaSha256:
				signer = new RsaDigestSigner (new Sha256Digest ());
				break;
			case DkimSignatureAlgorithm.Ed25519Sha256:
				signer = new Ed25519DigestSigner (new Sha256Digest ());
				break;
			default:
				throw new NotSupportedException ($"{algorithm} is not supported.");
			}

			signer.Init (sign, Key);

			return new BouncyCastleDkimSignatureContext (signer);
		}
	}
}
