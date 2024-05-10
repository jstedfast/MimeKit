//
// AsymmetricAlgorithmExtensions.cs
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
using System.Security.Cryptography;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Extension methods for System.Security.Cryptography.AsymmetricAlgorithm.
	/// </summary>
	/// <remarks>
	/// Extension methods for System.Security.Cryptography.AsymmetricAlgorithm.
	/// </remarks>
	public static class AsymmetricAlgorithmExtensions
	{
		static void GetAsymmetricKeyParameters (DSA dsa, bool publicOnly, out AsymmetricKeyParameter pub, out AsymmetricKeyParameter key)
		{
			var dp = dsa.ExportParameters (!publicOnly);
			var validationParameters = dp.Seed != null ? new DsaValidationParameters (dp.Seed, dp.Counter) : null;
			var parameters = new DsaParameters (
				new BigInteger (1, dp.P),
				new BigInteger (1, dp.Q),
				new BigInteger (1, dp.G),
				validationParameters);

			pub = new DsaPublicKeyParameters (new BigInteger (1, dp.Y), parameters);
			key = publicOnly ? null : new DsaPrivateKeyParameters (new BigInteger (1, dp.X), parameters);
		}

		static AsymmetricKeyParameter GetAsymmetricKeyParameter (DSACryptoServiceProvider dsa)
		{
			GetAsymmetricKeyParameters (dsa, dsa.PublicOnly, out var pub, out var key);

			return dsa.PublicOnly ? pub : key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (DSACryptoServiceProvider dsa)
		{
			if (dsa.PublicOnly)
				throw new ArgumentException ("DSA key is not a private key.", "key");

			GetAsymmetricKeyParameters (dsa, dsa.PublicOnly, out var pub, out var key);

			return new AsymmetricCipherKeyPair (pub, key);
		}

		static AsymmetricKeyParameter GetAsymmetricKeyParameter (DSA dsa)
		{
			GetAsymmetricKeyParameters (dsa, false, out _, out var key);

			return key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (DSA dsa)
		{
			GetAsymmetricKeyParameters (dsa, false, out var pub, out var key);

			return new AsymmetricCipherKeyPair (pub, key);
		}

		static void GetAsymmetricKeyParameters (RSA rsa, bool publicOnly, out AsymmetricKeyParameter pub, out AsymmetricKeyParameter key)
		{
			var rp = rsa.ExportParameters (!publicOnly);
			var modulus = new BigInteger (1, rp.Modulus);
			var exponent = new BigInteger (1, rp.Exponent);

			pub = new RsaKeyParameters (false, modulus, exponent);
			key = publicOnly ? null : new RsaPrivateCrtKeyParameters (
				modulus,
				exponent,
				new BigInteger (1, rp.D),
				new BigInteger (1, rp.P),
				new BigInteger (1, rp.Q),
				new BigInteger (1, rp.DP),
				new BigInteger (1, rp.DQ),
				new BigInteger (1, rp.InverseQ)
			);
		}

		static AsymmetricKeyParameter GetAsymmetricKeyParameter (RSACryptoServiceProvider rsa)
		{
			GetAsymmetricKeyParameters (rsa, rsa.PublicOnly, out var pub, out var key);

			return rsa.PublicOnly ? pub : key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (RSACryptoServiceProvider rsa)
		{
			if (rsa.PublicOnly)
				throw new ArgumentException ("RSA key is not a private key.", "key");

			GetAsymmetricKeyParameters (rsa, rsa.PublicOnly, out var pub, out var key);

			return new AsymmetricCipherKeyPair (pub, key);
		}

		static AsymmetricKeyParameter GetAsymmetricKeyParameter (RSA rsa)
		{
			GetAsymmetricKeyParameters (rsa, false, out _, out var key);

			return key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (RSA rsa)
		{
			GetAsymmetricKeyParameters (rsa, false, out var pub, out var key);

			return new AsymmetricCipherKeyPair (pub, key);
		}

		/// <summary>
		/// Convert an AsymmetricAlgorithm into a BouncyCastle AsymmetricKeyParameter.
		/// </summary>
		/// <remarks>
		/// <para>Converts an AsymmetricAlgorithm into a BouncyCastle AsymmetricKeyParameter.</para>
		/// <note type="note">Currently, only RSA and DSA keys are supported.</note>
		/// </remarks>
		/// <returns>The Bouncy Castle AsymmetricKeyParameter.</returns>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="key"/> is an unsupported asymmetric algorithm.
		/// </exception>
		public static AsymmetricKeyParameter AsAsymmetricKeyParameter (this AsymmetricAlgorithm key)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key is RSACryptoServiceProvider rsaKey)
				return GetAsymmetricKeyParameter (rsaKey);

			if (key is RSA rsa)
				return GetAsymmetricKeyParameter (rsa);

			if (key is DSACryptoServiceProvider dsaKey)
				return GetAsymmetricKeyParameter (dsaKey);

			if (key is DSA dsa)
				return GetAsymmetricKeyParameter (dsa);

			// TODO: support ECDiffieHellman and ECDsa?

			throw new NotSupportedException (string.Format ("'{0}' is currently not supported.", key.GetType ().Name));
		}

		/// <summary>
		/// Convert an AsymmetricAlgorithm into a BouncyCastle AsymmetricCipherKeyPair.
		/// </summary>
		/// <remarks>
		/// <para>Converts an AsymmetricAlgorithm into a BouncyCastle AsymmetricCipherKeyPair.</para>
		/// <note type="note">Currently, only RSA and DSA keys are supported.</note>
		/// </remarks>
		/// <returns>The Bouncy Castle AsymmetricCipherKeyPair.</returns>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is a public key.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="key"/> is an unsupported asymmetric algorithm.
		/// </exception>
		public static AsymmetricCipherKeyPair AsAsymmetricCipherKeyPair (this AsymmetricAlgorithm key)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key is RSACryptoServiceProvider rsaKey)
				return GetAsymmetricCipherKeyPair (rsaKey);

			if (key is RSA rsa)
				return GetAsymmetricCipherKeyPair (rsa);

			if (key is DSACryptoServiceProvider dsaKey)
				return GetAsymmetricCipherKeyPair (dsaKey);

			if (key is DSA dsa)
				return GetAsymmetricCipherKeyPair (dsa);

			// TODO: support ECDiffieHellman and ECDsa?

			throw new NotSupportedException (string.Format ("'{0}' is currently not supported.", key.GetType ().Name));
		}

		static byte[] GetPaddedByteArray (BigInteger big, int length)
		{
			var bytes = big.ToByteArrayUnsigned ();

			if (bytes.Length >= length)
				return bytes;

			var padded = new byte[length];

			Buffer.BlockCopy (bytes, 0, padded, length - bytes.Length, bytes.Length);

			return padded;
		}

		static DSAParameters GetDSAParameters (DsaKeyParameters key)
		{
			var parameters = new DSAParameters ();

			if (key.Parameters.ValidationParameters != null) {
				parameters.Counter = key.Parameters.ValidationParameters.Counter;
				parameters.Seed = key.Parameters.ValidationParameters.GetSeed ();
			}

			parameters.P = key.Parameters.P.ToByteArrayUnsigned ();
			parameters.Q = key.Parameters.Q.ToByteArrayUnsigned ();
			parameters.G = GetPaddedByteArray (key.Parameters.G, parameters.P.Length);

			return parameters;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPrivateKeyParameters key, DsaPublicKeyParameters pub)
		{
			var parameters = GetDSAParameters (key);
			parameters.X = GetPaddedByteArray (key.X, parameters.Q.Length);

			if (pub != null)
				parameters.Y = GetPaddedByteArray (pub.Y, parameters.P.Length);

			var dsa = new DSACryptoServiceProvider ();

			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPublicKeyParameters key)
		{
			var parameters = GetDSAParameters (key);
			parameters.Y = GetPaddedByteArray (key.Y, parameters.P.Length);

			var dsa = new DSACryptoServiceProvider ();

			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (RsaPrivateCrtKeyParameters key)
		{
			var parameters = new RSAParameters {
				Exponent = key.PublicExponent.ToByteArrayUnsigned (),
				Modulus = key.Modulus.ToByteArrayUnsigned (),
				P = key.P.ToByteArrayUnsigned (),
				Q = key.Q.ToByteArrayUnsigned ()
			};

			parameters.InverseQ = GetPaddedByteArray (key.QInv, parameters.Q.Length);
			parameters.D = GetPaddedByteArray (key.Exponent, parameters.Modulus.Length);
			parameters.DP = GetPaddedByteArray (key.DP, parameters.P.Length);
			parameters.DQ = GetPaddedByteArray (key.DQ, parameters.Q.Length);

			var rsa = new RSACryptoServiceProvider ();

			rsa.ImportParameters (parameters);

			return rsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (RsaKeyParameters key)
		{
			var parameters = new RSAParameters {
				Exponent = key.Exponent.ToByteArrayUnsigned (),
				Modulus = key.Modulus.ToByteArrayUnsigned ()
			};

			var rsa = new RSACryptoServiceProvider ();

			rsa.ImportParameters (parameters);

			return rsa;
		}

		/// <summary>
		/// Convert a BouncyCastle AsymmetricKeyParameter into an AsymmetricAlgorithm.
		/// </summary>
		/// <remarks>
		/// <para>Converts a BouncyCastle AsymmetricKeyParameter into an AsymmetricAlgorithm.</para>
		/// <note type="note">Currently, only RSA and DSA keys are supported.</note>
		/// </remarks>
		/// <returns>The AsymmetricAlgorithm.</returns>
		/// <param name="key">The AsymmetricKeyParameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="key"/> is an unsupported asymmetric key parameter.
		/// </exception>
		public static AsymmetricAlgorithm AsAsymmetricAlgorithm (this AsymmetricKeyParameter key)
		{
			// TODO: Drop this API - it's no longer needed. The WindowsSecureMimeContext now exports the certificate & key into a pkcs12 and then loads that into an X509Certificate2.
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key.IsPrivate) {
				if (key is RsaPrivateCrtKeyParameters rsaPrivateKey)
					return GetAsymmetricAlgorithm (rsaPrivateKey);

				if (key is DsaPrivateKeyParameters dsaPrivateKey)
					return GetAsymmetricAlgorithm (dsaPrivateKey, null);
			} else {
				if (key is RsaKeyParameters rsaPublicKey)
					return GetAsymmetricAlgorithm (rsaPublicKey);

				if (key is DsaPublicKeyParameters dsaPublicKey)
					return GetAsymmetricAlgorithm (dsaPublicKey);
			}

			throw new NotSupportedException (string.Format ("{0} is currently not supported.", key.GetType ().Name));
		}

		/// <summary>
		/// Convert a BouncyCastle AsymmetricCipherKeyPair into an AsymmetricAlgorithm.
		/// </summary>
		/// <remarks>
		/// <para>Converts a BouncyCastle AsymmetricCipherKeyPair into an AsymmetricAlgorithm.</para>
		/// <note type="note">Currently, only RSA and DSA keys are supported.</note>
		/// </remarks>
		/// <returns>The AsymmetricAlgorithm.</returns>
		/// <param name="key">The AsymmetricCipherKeyPair.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="key"/> is an unsupported asymmetric algorithm.
		/// </exception>
		public static AsymmetricAlgorithm AsAsymmetricAlgorithm (this AsymmetricCipherKeyPair key)
		{
			// TODO: Drop this API - it's no longer needed. The WindowsSecureMimeContext now exports the certificate & key into a pkcs12 and then loads that into an X509Certificate2.
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key.Private is RsaPrivateCrtKeyParameters rsaPrivateKey)
				return GetAsymmetricAlgorithm (rsaPrivateKey);

			if (key.Private is DsaPrivateKeyParameters dsaPrivateKey)
				return GetAsymmetricAlgorithm (dsaPrivateKey, (DsaPublicKeyParameters) key.Public);

			throw new NotSupportedException (string.Format ("{0} is currently not supported.", key.GetType ().Name));
		}
	}
}
