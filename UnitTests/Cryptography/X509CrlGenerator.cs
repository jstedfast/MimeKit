//
// X509CrlGenerator.cs
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

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace UnitTests.Cryptography {
	class X509CrlGenerator
	{
		static BigInteger CrlNumberValue = BigInteger.One;

		public static X509Crl Generate (X509Certificate authority, AsymmetricKeyParameter signingKey, DateTime thisUpdate, DateTime nextUpdate, params X509Certificate[] certificates)
		{
			var randomGenerator = new CryptoApiRandomGenerator ();
			var random = new SecureRandom (randomGenerator);
			var crlGenerator = new X509V2CrlGenerator ();
			crlGenerator.SetIssuerDN (authority.SubjectDN);
			crlGenerator.SetThisUpdate (thisUpdate);
			crlGenerator.SetNextUpdate (nextUpdate);

			crlGenerator.AddExtension (X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure (authority.GetPublicKey ()));
			crlGenerator.AddExtension (X509Extensions.CrlNumber, false, new CrlNumber (CrlNumberValue));
			CrlNumberValue = CrlNumberValue.Add (BigInteger.One);

			foreach (var certificate in certificates)
				crlGenerator.AddCrlEntry (certificate.SerialNumber, thisUpdate, CrlReason.KeyCompromise);

			string signatureAlgorithm;

			if (signingKey is RsaPrivateCrtKeyParameters) {
				signatureAlgorithm = "SHA256WithRSA";
			} else if (signingKey is ECPrivateKeyParameters ec) {
				if (ec.AlgorithmName == "ECGOST3410") {
					signatureAlgorithm = "GOST3411WithECGOST3410";
				} else {
					signatureAlgorithm = "SHA256withECDSA";
				}
			} else {
				signatureAlgorithm = "SHA256WithRSA";
			}

			var crl = crlGenerator.Generate (new Asn1SignatureFactory (signatureAlgorithm, signingKey, random));

			return crl;
		}
	}
}
