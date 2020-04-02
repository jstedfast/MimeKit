﻿//
// AsymmetricAlgorithmExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
			AsymmetricKeyParameter pub, key;

			GetAsymmetricKeyParameters (dsa, dsa.PublicOnly, out pub, out key);

			return dsa.PublicOnly ? pub : key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (DSACryptoServiceProvider dsa)
		{
			AsymmetricKeyParameter pub, key;

			if (dsa.PublicOnly)
				throw new ArgumentException ("DSA key is not a private key.", "key");

			GetAsymmetricKeyParameters (dsa, dsa.PublicOnly, out pub, out key);

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
			AsymmetricKeyParameter pub, key;

			GetAsymmetricKeyParameters (rsa, rsa.PublicOnly, out pub, out key);

			return rsa.PublicOnly ? pub : key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (RSACryptoServiceProvider rsa)
		{
			AsymmetricKeyParameter pub, key;

			if (rsa.PublicOnly)
				throw new ArgumentException ("RSA key is not a private key.", "key");

			GetAsymmetricKeyParameters (rsa, rsa.PublicOnly, out pub, out key);

			return new AsymmetricCipherKeyPair (pub, key);
		}

#if NET46 || NET47 || NET48 || NETCOREAPP3_0 || __MOBILE__
		static AsymmetricKeyParameter GetAsymmetricKeyParameter (RSACng rsa)
		{
			AsymmetricKeyParameter pub, key;

			GetAsymmetricKeyParameters (rsa, false, out pub, out key);

			return key;
		}

		static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair (RSACng rsa)
		{
			AsymmetricKeyParameter pub, key;

			GetAsymmetricKeyParameters (rsa, false, out pub, out key);

			return new AsymmetricCipherKeyPair (pub, key);
		}
#endif

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

			if (key is DSACryptoServiceProvider)
				return GetAsymmetricKeyParameter ((DSACryptoServiceProvider) key);

			if (key is RSACryptoServiceProvider)
				return GetAsymmetricKeyParameter ((RSACryptoServiceProvider) key);

#if NET46 || NET47 || NET48 || NETCOREAPP3_0 || __MOBILE__
			if (key is RSACng)
				return GetAsymmetricKeyParameter ((RSACng) key);
#endif

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

			if (key is DSACryptoServiceProvider)
				return GetAsymmetricCipherKeyPair ((DSACryptoServiceProvider) key);

			if (key is RSACryptoServiceProvider)
				return GetAsymmetricCipherKeyPair ((RSACryptoServiceProvider) key);

#if NET46 || NET47 || NET48 || NETCOREAPP3_0 || __MOBILE__
			if (key is RSACng)
				return GetAsymmetricCipherKeyPair ((RSACng) key);
#endif

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

			parameters.G = key.Parameters.G.ToByteArrayUnsigned ();
			parameters.P = key.Parameters.P.ToByteArrayUnsigned ();
			parameters.Q = key.Parameters.Q.ToByteArrayUnsigned ();

			return parameters;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPrivateKeyParameters key, DsaPublicKeyParameters pub)
		{
			var parameters = GetDSAParameters (key);
			parameters.X = key.X.ToByteArrayUnsigned ();

			if (pub != null)
				parameters.Y = pub.Y.ToByteArrayUnsigned ();

			var dsa = new DSACryptoServiceProvider ();
			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPublicKeyParameters key)
		{
			var parameters = GetDSAParameters (key);
			parameters.Y = key.Y.ToByteArrayUnsigned ();

			var dsa = new DSACryptoServiceProvider ();
			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (RsaPrivateCrtKeyParameters key)
		{
			var parameters = new RSAParameters ();

			parameters.Exponent = key.PublicExponent.ToByteArrayUnsigned ();
			parameters.Modulus = key.Modulus.ToByteArrayUnsigned ();
			parameters.P = key.P.ToByteArrayUnsigned ();
			parameters.Q = key.Q.ToByteArrayUnsigned ();

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
			var parameters = new RSAParameters ();
			parameters.Exponent = key.Exponent.ToByteArrayUnsigned ();
			parameters.Modulus = key.Modulus.ToByteArrayUnsigned ();

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
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key.IsPrivate) {
				if (key is DsaPrivateKeyParameters)
					return GetAsymmetricAlgorithm ((DsaPrivateKeyParameters) key, null);

				if (key is RsaPrivateCrtKeyParameters)
					return GetAsymmetricAlgorithm ((RsaPrivateCrtKeyParameters) key);
			} else {
				if (key is DsaPublicKeyParameters)
					return GetAsymmetricAlgorithm ((DsaPublicKeyParameters) key);

				if (key is RsaKeyParameters)
					return GetAsymmetricAlgorithm ((RsaKeyParameters) key);
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
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (key.Private is DsaPrivateKeyParameters)
				return GetAsymmetricAlgorithm ((DsaPrivateKeyParameters) key.Private, (DsaPublicKeyParameters) key.Public);

			if (key.Private is RsaPrivateCrtKeyParameters)
				return GetAsymmetricAlgorithm ((RsaPrivateCrtKeyParameters) key.Private);

			throw new NotSupportedException (string.Format ("{0} is currently not supported.", key.GetType ().Name));
		}
	}
}
