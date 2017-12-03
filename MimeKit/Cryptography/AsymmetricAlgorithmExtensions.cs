//
// AsymmetricAlgorithmExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corp.
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

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography
{
	/// <summary>
	/// Extension methods for System.Security.Cryptography.AsymmetricAlgorithm.
	/// </summary>
	/// <remarks>
	/// Extension methods for System.Security.Cryptography.AsymmetricAlgorithm.
	/// </remarks>
	public static class AsymmetricAlgorithmExtensions
	{
		static AsymmetricCipherKeyPair GetBouncyCastleKeyPair (DSA dsa)
		{
			var dp = dsa.ExportParameters (true);
			var validationParameters = dp.Seed != null ? new DsaValidationParameters (dp.Seed, dp.Counter) : null;
			var parameters = new DsaParameters (
				new BigInteger (1, dp.P),
				new BigInteger (1, dp.Q),
				new BigInteger (1, dp.G),
				validationParameters);
			var pubKey = new DsaPublicKeyParameters (new BigInteger (1, dp.Y), parameters);
			var privKey = new DsaPrivateKeyParameters (new BigInteger (1, dp.X), parameters);

			return new AsymmetricCipherKeyPair (pubKey, privKey);
		}

		static AsymmetricCipherKeyPair GetBouncyCastleKeyPair (RSA rsa)
		{
			var rp = rsa.ExportParameters (true);
			var modulus = new BigInteger (1, rp.Modulus);
			var exponent = new BigInteger (1, rp.Exponent);
			var pubKey = new RsaKeyParameters (false, modulus, exponent);
			var privKey = new RsaPrivateCrtKeyParameters (
				modulus,
				exponent,
				new BigInteger (1, rp.D),
				new BigInteger (1, rp.P),
				new BigInteger (1, rp.Q),
				new BigInteger (1, rp.DP),
				new BigInteger (1, rp.DQ),
				new BigInteger (1, rp.InverseQ)
			);

			return new AsymmetricCipherKeyPair (pubKey, privKey);
		}

		/// <summary>
		/// Convert an AsymmetricAlgorithm into a BouncyCastle AsymmetricCipherKeyPair.
		/// </summary>
		/// <remarks>
		/// <para>Converts an AsymmetricAlgorithm into a BouncyCastle AsymmetricCipherKeyPair.</para>
		/// </remarks>
		/// <returns>The bouncy castle key pair.</returns>
		/// <param name="privateKey">The private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="privateKey"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="privateKey"/> is an unsupported asymmetric algorithm.
		/// </exception>
		public static AsymmetricCipherKeyPair AsBouncyCastleKeyPair (this AsymmetricAlgorithm privateKey)
		{
			if (privateKey == null)
				throw new ArgumentNullException (nameof (privateKey));

			if (privateKey is DSA)
				return GetBouncyCastleKeyPair ((DSA) privateKey);

			if (privateKey is RSA)
				return GetBouncyCastleKeyPair ((RSA) privateKey);

			// TODO: support ECDiffieHellman and ECDsa?

			throw new ArgumentException (string.Format ("'{0}' is currently not supported.", privateKey.GetType ().Name), nameof (privateKey));
		}
	}
}
