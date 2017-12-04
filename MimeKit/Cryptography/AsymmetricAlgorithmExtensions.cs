//
// AsymmetricAlgorithmExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
		static AsymmetricKeyParameter GetAsymmetricKeyParameter (DSACryptoServiceProvider dsa)
		{
			var dp = dsa.ExportParameters (!dsa.PublicOnly);
			var validationParameters = dp.Seed != null ? new DsaValidationParameters (dp.Seed, dp.Counter) : null;
			var parameters = new DsaParameters (
				new BigInteger (1, dp.P),
				new BigInteger (1, dp.Q),
				new BigInteger (1, dp.G),
				validationParameters);

			if (dsa.PublicOnly)
				return new DsaPublicKeyParameters (new BigInteger (1, dp.Y), parameters);

			return new DsaPrivateKeyParameters (new BigInteger (1, dp.X), parameters);
		}

		static AsymmetricKeyParameter GetAsymmetricKeyParameter (RSACryptoServiceProvider rsa)
		{
			var rp = rsa.ExportParameters (!rsa.PublicOnly);
			var modulus = new BigInteger (1, rp.Modulus);
			var exponent = new BigInteger (1, rp.Exponent);

			if (rsa.PublicOnly)
				return new RsaKeyParameters (false, modulus, exponent);

			return new RsaPrivateCrtKeyParameters (
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

			// TODO: support ECDiffieHellman and ECDsa?

			throw new NotSupportedException (string.Format ("'{0}' is currently not supported.", key.GetType ().Name));
		}

		static DSAParameters GetDSAParameters (DsaKeyParameters key)
		{
			var parameters = new DSAParameters ();

			if (key.Parameters.ValidationParameters != null) {
				parameters.Counter = key.Parameters.ValidationParameters.Counter;
				parameters.Seed = key.Parameters.ValidationParameters.GetSeed ();
			}

			parameters.G = key.Parameters.G.ToByteArray ();
			parameters.P = key.Parameters.P.ToByteArray ();
			parameters.Q = key.Parameters.Q.ToByteArray ();

			return parameters;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPrivateKeyParameters key)
		{
			var parameters = GetDSAParameters (key);
			parameters.X = key.X.ToByteArray ();

			var dsa = new DSACryptoServiceProvider ();
			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (DsaPublicKeyParameters key)
		{
			var parameters = GetDSAParameters (key);
			parameters.Y = key.Y.ToByteArray ();

			var dsa = new DSACryptoServiceProvider ();
			dsa.ImportParameters (parameters);

			return dsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (RsaPrivateCrtKeyParameters key)
		{
			var parameters = new RSAParameters ();

			parameters.Exponent = key.PublicExponent.ToByteArray ();
			parameters.Modulus = key.Modulus.ToByteArray ();
			parameters.InverseQ = key.QInv.ToByteArray ();
			parameters.D = key.Exponent.ToByteArray ();
			parameters.DP = key.DP.ToByteArray ();
			parameters.DQ = key.DQ.ToByteArray ();
			parameters.P = key.P.ToByteArray ();
			parameters.Q = key.Q.ToByteArray ();

			var rsa = new RSACryptoServiceProvider ();
			rsa.ImportParameters (parameters);

			return rsa;
		}

		static AsymmetricAlgorithm GetAsymmetricAlgorithm (RsaKeyParameters key)
		{
			var parameters = new RSAParameters ();
			parameters.Exponent = key.Exponent.ToByteArray ();
			parameters.Modulus = key.Modulus.ToByteArray ();

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
					return GetAsymmetricAlgorithm ((DsaPrivateKeyParameters) key);

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
	}
}
