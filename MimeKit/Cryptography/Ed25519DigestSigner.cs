//
// Ed25519DigestSigner.cs
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

using System;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	class Ed25519DigestSigner : ISigner
	{
		Ed25519PrivateKeyParameters privateKey;
		Ed25519PublicKeyParameters publicKey;
		readonly IDigest digest;

		public Ed25519DigestSigner (IDigest digest)
		{
			this.digest = digest;
		}

		public int GetMaxSignatureSize ()
		{
			return Ed25519PrivateKeyParameters.SignatureSize;
		}

		public string AlgorithmName {
			get { return digest.AlgorithmName + "withEd25519"; }
		}

		public void Init (bool forSigning, ICipherParameters parameters)
		{
			if (forSigning) {
				privateKey = (Ed25519PrivateKeyParameters) parameters;
				publicKey = privateKey.GeneratePublicKey ();
			} else {
				publicKey = (Ed25519PublicKeyParameters) parameters;
				privateKey = null;
			}

			Reset ();
		}

		public void Update (byte input)
		{
			digest.Update (input);
		}

		public void BlockUpdate (byte[] input, int inOff, int length)
		{
			digest.BlockUpdate (input, inOff, length);
		}

#if NET6_0_OR_GREATER
		/// <summary>Update the signer with a span of bytes.</summary>
		/// <param name="input">the span containing the data.</param>
		public void BlockUpdate (ReadOnlySpan<byte> input)
		{
			digest.BlockUpdate (input);
		}
#endif

		public byte[] GenerateSignature ()
		{
			if (privateKey == null)
				throw new InvalidOperationException ("Ed25519DigestSigner not initialised for signature generation.");

			var hash = new byte[digest.GetDigestSize ()];

			digest.DoFinal (hash, 0);

			var signature = new byte[Ed25519PrivateKeyParameters.SignatureSize];
			privateKey.Sign (Ed25519.Algorithm.Ed25519, null, hash, 0, hash.Length, signature, 0);

			Reset ();

			return signature;
		}

		public bool VerifySignature (byte[] signature)
		{
			if (privateKey != null)
				throw new InvalidOperationException ("Ed25519DigestSigner not initialised for verification");

			if (Ed25519.SignatureSize != signature.Length)
				return false;

			var hash = new byte[digest.GetDigestSize ()];

			digest.DoFinal (hash, 0);

			var pk = publicKey.GetEncoded ();
			var result = Ed25519.Verify (signature, 0, pk, 0, hash, 0, hash.Length);

			Reset ();

			return result;
		}

		public void Reset ()
		{
			digest.Reset ();
		}
	}
}
