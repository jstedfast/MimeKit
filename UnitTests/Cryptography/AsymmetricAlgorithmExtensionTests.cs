//
// AsymmetricAlgorithmExtensionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class AsymmetricAlgorithmExtensionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricKeyParameter (null));
			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricCipherKeyPair (null));
		}

		static void AssertAreEqual (byte[] expected, BigInteger actual, string paramName)
		{
			Assert.That (expected, Is.Not.Null, $"Expected {paramName} is null");
			Assert.That (actual, Is.Not.Null, $"Actual {paramName} is null");
			Assert.That (actual, Is.EqualTo (new BigInteger (1, expected)), $"{paramName} are not equal");
		}

		static void AssertDsaParameters (DSAParameters expected, DsaParameters actual)
		{
			AssertAreEqual (expected.P, actual.P, "P");
			AssertAreEqual (expected.Q, actual.Q, "Q");
			AssertAreEqual (expected.G, actual.G, "G");

			if (expected.Seed != null) {
				Assert.That (actual.ValidationParameters, Is.Not.Null, "ValidationParameters");
				Assert.That (actual.ValidationParameters.Counter, Is.EqualTo (expected.Counter), "Counter");
				Assert.That (actual.ValidationParameters.GetSeed (), Is.EqualTo (expected.Seed), "Seed");
			} else {
				Assert.That (actual.ValidationParameters, Is.Null, "ValidationParameters");
			}
		}

		static void AssertDSA (DSA dsa)
		{
			// first, check private key conversion
			var expected = dsa.ExportParameters (true);
			var privateKey = dsa.AsAsymmetricKeyParameter () as DsaPrivateKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricKeyParameter (private)");
			Assert.That (privateKey.IsPrivate, Is.True, "IsPrivate");
			AssertDsaParameters (expected, privateKey.Parameters);
			AssertAreEqual (expected.X, privateKey.X, "X");

			// test AsymmetricCipherKeyPair conversion
			var keyPair = dsa.AsAsymmetricCipherKeyPair ();
			privateKey = keyPair.Private as DsaPrivateKeyParameters;
			var publicKey = keyPair.Public as DsaPublicKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricCipherKeyPair.Private");
			Assert.That (publicKey, Is.Not.Null, "AsAsymmetricCipherKeyPair.Public");
			AssertDsaParameters (expected, privateKey.Parameters);
			AssertDsaParameters (expected, publicKey.Parameters);
			AssertAreEqual (expected.X, privateKey.X, "X");
			AssertAreEqual (expected.Y, publicKey.Y, "Y");

			// test public key conversion
			expected = dsa.ExportParameters (false);
			using var pubdsa = new DSACryptoServiceProvider ();
			pubdsa.ImportParameters (expected);

			publicKey = pubdsa.AsAsymmetricKeyParameter () as DsaPublicKeyParameters;

			Assert.That (publicKey, Is.Not.Null, "AsAsymmetricKeyParameter (public)");
			Assert.That (publicKey.IsPrivate, Is.False, "IsPrivate");
			AssertDsaParameters (expected, publicKey.Parameters);
			AssertAreEqual (expected.Y, publicKey.Y, "Y");

			Assert.Throws<ArgumentException> (() => pubdsa.AsAsymmetricCipherKeyPair ());
		}

		[Test]
		public void TestDSACryptoServiceProvider ()
		{
#if !MONO
			using (var dsa = new DSACryptoServiceProvider (1024))
				AssertDSA (dsa);
#else
			// System.Security.Cryptography.CryptographicException: Specified key is not a valid size for this algorithm.
			// DSACryptoServiceProvider.set_KeySize = 1024;
			Assert.Ignore ("Mono does not support 1024-bit key sizes for DSACryptoServiceProvider");
#endif
		}

		[Test]
		[SuppressMessage ("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		public void TestDSACng ()
		{
#if !MONO && ENABLE_DSA_CNG
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				using (var dsa = new DSACng (1024))
					AssertDSA (dsa);
			} else {
				Assert.Ignore ("DSACng is only supported on Windows systems.");
			}
#else
			Assert.Ignore ("Mono does not implement DSACng");
#endif
		}

		static void AssertRSA (RSA rsa)
		{
			// first, check private key conversion
			var expected = rsa.ExportParameters (true);
			var privateKey = rsa.AsAsymmetricKeyParameter () as RsaPrivateCrtKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricKeyParameter (private)");
			AssertRsaPrivateKey (expected, privateKey);

			// test AsymmetricCipherKeyPair conversion
			var keyPair = rsa.AsAsymmetricCipherKeyPair ();
			privateKey = keyPair.Private as RsaPrivateCrtKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricCipherKeyPair.Private");
			AssertRsaPrivateKey (expected, privateKey);
			AssertRsaPublicKey (expected, keyPair.Public as RsaKeyParameters);

			// test public key conversion
			expected = rsa.ExportParameters (false);
			using var pubrsa = new RSACryptoServiceProvider ();
			pubrsa.ImportParameters (expected);

			AssertRsaPublicKey (expected, pubrsa.AsAsymmetricKeyParameter () as RsaKeyParameters);

			Assert.Throws<ArgumentException> (() => pubrsa.AsAsymmetricCipherKeyPair ());
		}

		static void AssertRsaPrivateKey (RSAParameters expected, RsaPrivateCrtKeyParameters actual)
		{
			Assert.That (actual.IsPrivate, Is.True, "IsPrivate");
			AssertAreEqual (expected.Modulus, actual.Modulus, "Modulus");
			AssertAreEqual (expected.Exponent, actual.PublicExponent, "Exponent");
			AssertAreEqual (expected.D, actual.Exponent, "D");
			AssertAreEqual (expected.P, actual.P, "P");
			AssertAreEqual (expected.Q, actual.Q, "Q");
			AssertAreEqual (expected.DP, actual.DP, "DP");
			AssertAreEqual (expected.DQ, actual.DQ, "DQ");
			AssertAreEqual (expected.InverseQ, actual.QInv, "InverseQ");
		}

		static void AssertRsaPublicKey (RSAParameters expected, RsaKeyParameters actual)
		{
			Assert.That (actual, Is.Not.Null, "RSA public key");
			Assert.That (actual.IsPrivate, Is.False, "IsPrivate");
			AssertAreEqual (expected.Modulus, actual.Modulus, "Modulus");
			AssertAreEqual (expected.Exponent, actual.Exponent, "Exponent");
		}

		[Test]
		public void TestRSACryptoServiceProvider ()
		{
			using (var rsa = new RSACryptoServiceProvider (2048))
				AssertRSA (rsa);
		}

		[Test]
		[SuppressMessage ("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		public void TestRSACng ()
		{
#if !MONO
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				using (var rsa = new RSACng (2048))
					AssertRSA (rsa);
			} else {
				Assert.Ignore ("RSACng is only supported on Windows systems.");
			}
#else
			Assert.Ignore ("Mono does not implement RSACng");
#endif
		}

		static void AssertECDomain (ECParameters expected, ECDomainParameters actual)
		{
			var namedCurve = ECNamedCurveTable.GetByOid (new DerObjectIdentifier (expected.Curve.Oid.Value));

			Assert.That (namedCurve, Is.Not.Null, "Named curve");
			Assert.That (actual.Curve, Is.EqualTo (namedCurve.Curve), "Curve");
			Assert.That (actual.G, Is.EqualTo (namedCurve.G), "G");
			Assert.That (actual.N, Is.EqualTo (namedCurve.N), "N");
			Assert.That (actual.H, Is.EqualTo (namedCurve.H), "H");
		}

		static void AssertECPoint (ECParameters expected, Org.BouncyCastle.Math.EC.ECPoint actual)
		{
			var q = actual.Normalize ();

			AssertAreEqual (expected.Q.X, q.AffineXCoord.ToBigInteger (), "Q.X");
			AssertAreEqual (expected.Q.Y, q.AffineYCoord.ToBigInteger (), "Q.Y");
		}

		static void AssertECDsa (ECDsa ecdsa)
		{
			// first, check private key conversion
			var expected = ecdsa.ExportParameters (true);
			var privateKey = ecdsa.AsAsymmetricKeyParameter () as ECPrivateKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricKeyParameter (private)");
			Assert.That (privateKey.IsPrivate, Is.True, "IsPrivate");
			AssertECDomain (expected, privateKey.Parameters);
			AssertAreEqual (expected.D, privateKey.D, "D");

			// verify that the private key generates the expected public key point
			AssertECPoint (expected, privateKey.Parameters.G.Multiply (privateKey.D));

			// test AsymmetricCipherKeyPair conversion
			var keyPair = ecdsa.AsAsymmetricCipherKeyPair ();
			privateKey = keyPair.Private as ECPrivateKeyParameters;
			var publicKey = keyPair.Public as ECPublicKeyParameters;

			Assert.That (privateKey, Is.Not.Null, "AsAsymmetricCipherKeyPair.Private");
			Assert.That (publicKey, Is.Not.Null, "AsAsymmetricCipherKeyPair.Public");
			AssertECDomain (expected, privateKey.Parameters);
			AssertECDomain (expected, publicKey.Parameters);
			AssertAreEqual (expected.D, privateKey.D, "D");
			AssertECPoint (expected, publicKey.Q);

			// test public key conversion
			expected = ecdsa.ExportParameters (false);
			using var pubec = ECDsa.Create ();
			pubec.ImportParameters (expected);

			publicKey = pubec.AsAsymmetricKeyParameter () as ECPublicKeyParameters;

			Assert.That (publicKey, Is.Not.Null, "AsAsymmetricKeyParameter (public)");
			Assert.That (publicKey.IsPrivate, Is.False, "IsPrivate");
			AssertECDomain (expected, publicKey.Parameters);
			AssertECPoint (expected, publicKey.Q);
		}

		static ECCurve GetNamedCurve (string name)
		{
			switch (name) {
			case "brainpoolP160r1": return ECCurve.NamedCurves.brainpoolP160r1;
			case "brainpoolP160t1": return ECCurve.NamedCurves.brainpoolP160t1;
			case "brainpoolP192r1": return ECCurve.NamedCurves.brainpoolP192r1;
			case "brainpoolP192t1": return ECCurve.NamedCurves.brainpoolP192t1;
			case "brainpoolP224r1": return ECCurve.NamedCurves.brainpoolP224r1;
			case "brainpoolP224t1": return ECCurve.NamedCurves.brainpoolP224t1;
			case "brainpoolP256r1": return ECCurve.NamedCurves.brainpoolP256r1;
			case "brainpoolP256t1": return ECCurve.NamedCurves.brainpoolP256t1;
			case "brainpoolP320r1": return ECCurve.NamedCurves.brainpoolP320r1;
			case "brainpoolP320t1": return ECCurve.NamedCurves.brainpoolP320t1;
			case "brainpoolP384r1": return ECCurve.NamedCurves.brainpoolP384r1;
			case "brainpoolP384t1": return ECCurve.NamedCurves.brainpoolP384t1;
			case "brainpoolP512r1": return ECCurve.NamedCurves.brainpoolP512r1;
			case "brainpoolP512t1": return ECCurve.NamedCurves.brainpoolP512t1;
			case "nistP256": return ECCurve.NamedCurves.nistP256;
			case "nistP384": return ECCurve.NamedCurves.nistP384;
			case "nistP521": return ECCurve.NamedCurves.nistP521;
			default: throw new NotSupportedException ($"Unknown NamedCurve: {name}");
			}
		}

		[TestCase ("brainpoolP160r1")]
		[TestCase ("brainpoolP160t1")]
		[TestCase ("brainpoolP192r1")]
		[TestCase ("brainpoolP192t1")]
		[TestCase ("brainpoolP224r1")]
		[TestCase ("brainpoolP224t1")]
		[TestCase ("brainpoolP256r1")]
		[TestCase ("brainpoolP256t1")]
		[TestCase ("brainpoolP320r1")]
		[TestCase ("brainpoolP320t1")]
		[TestCase ("brainpoolP384r1")]
		[TestCase ("brainpoolP384t1")]
		[TestCase ("brainpoolP512r1")]
		[TestCase ("brainpoolP512t1")]
		[TestCase ("nistP256")]
		[TestCase ("nistP384")]
		[TestCase ("nistP521")]
		public void TestECDsa (string namedCurve)
		{
			using (var ecdsa = ECDsa.Create (GetNamedCurve (namedCurve)))
				AssertECDsa (ecdsa);
		}

		[TestCase ("brainpoolP160r1")]
		[TestCase ("brainpoolP160t1")]
		[TestCase ("brainpoolP192r1")]
		[TestCase ("brainpoolP192t1")]
		[TestCase ("brainpoolP224r1")]
		[TestCase ("brainpoolP224t1")]
		[TestCase ("brainpoolP256r1")]
		[TestCase ("brainpoolP256t1")]
		[TestCase ("brainpoolP320r1")]
		[TestCase ("brainpoolP320t1")]
		[TestCase ("brainpoolP384r1")]
		[TestCase ("brainpoolP384t1")]
		[TestCase ("brainpoolP512r1")]
		[TestCase ("brainpoolP512t1")]
		[TestCase ("nistP256")]
		[TestCase ("nistP384")]
		[TestCase ("nistP521")]
		[SuppressMessage ("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		public void TestECDsaCng (string namedCurve)
		{
#if !MONO
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				using (var ecdsa = new ECDsaCng (GetNamedCurve (namedCurve)))
					AssertECDsa (ecdsa);
			} else {
				Assert.Ignore ("ECDsaCng is only supported on Windows systems.");
			}
#else
			Assert.Ignore ("Mono does not implement ECDsaCng");
#endif
		}
	}
}
