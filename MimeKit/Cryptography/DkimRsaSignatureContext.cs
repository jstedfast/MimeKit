//
// DkimRsaSignatureContext.cs
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
using System.Security.Cryptography;

namespace MimeKit.Cryptography {
	class DkimRsaSignatureContext : IDkimSignatureContext
	{
		readonly HashAlgorithmName hashAlgo;
		readonly HashAlgorithm hash;
		readonly RSA rsa;

		public DkimRsaSignatureContext (RSA rsa, DkimSignatureAlgorithm algorithm)
		{
			this.rsa = rsa;

			switch (algorithm) {
			case DkimSignatureAlgorithm.RsaSha1:
				hashAlgo = HashAlgorithmName.SHA1;
				hash = SHA1.Create ();
				break;
			case DkimSignatureAlgorithm.RsaSha256:
				hashAlgo = HashAlgorithmName.SHA256;
				hash = SHA256.Create ();
				break;
			default:
				throw new NotSupportedException ($"{algorithm} is not supported.");
			}
		}

		public void Update (byte[] buffer, int offset, int length)
		{
			hash.TransformBlock (buffer, offset, length, null, 0);
		}

		public byte[] GenerateSignature ()
		{
			hash.TransformFinalBlock (Array.Empty<byte> (), 0, 0);

			return rsa.SignHash (hash.Hash, hashAlgo, RSASignaturePadding.Pkcs1);
		}

		public bool VerifySignature (byte[] signature)
		{
			hash.TransformFinalBlock (Array.Empty<byte> (), 0, 0);

			return rsa.VerifyHash (hash.Hash, signature, hashAlgo, RSASignaturePadding.Pkcs1);
		}

		public void Dispose ()
		{
			hash.Dispose ();
		}
	}
}
