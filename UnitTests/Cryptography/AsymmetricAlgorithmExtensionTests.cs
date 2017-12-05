//
// AsymmetricAlgorithmExtensionTests.cs
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

using NUnit.Framework;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography
{
	[TestFixture]
	public class AsymmetricAlgorithmExtensionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricKeyParameter (null));
			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricCipherKeyPair (null));

			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricAlgorithm ((AsymmetricKeyParameter) null));
			Assert.Throws<ArgumentNullException> (() => AsymmetricAlgorithmExtensions.AsAsymmetricAlgorithm ((AsymmetricCipherKeyPair) null));
		}

		static void AssertAreEqual (byte[] expected, byte[] actual, string paramName)
		{
			if (expected == null) {
				Assert.IsNull (actual, paramName);
				return;
			}

			Assert.IsNotNull (actual, paramName);
			Assert.AreEqual (expected.Length, actual.Length, "Lengths do not match: {0}", paramName);

			var expectedBigInteger = new BigInteger (1, expected);
			var actualBigInteger = new BigInteger (1, actual);

			Assert.AreEqual (expectedBigInteger, actualBigInteger, "{0} are not equal", paramName);
		}

		[Test]
		public void TestDsa ()
		{
			using (var dsa = new DSACryptoServiceProvider (1024)) {
				// first, check private key conversion
				var expected = dsa.ExportParameters (true);
				var keyParameter = dsa.AsAsymmetricKeyParameter ();
				var windows = keyParameter.AsAsymmetricAlgorithm () as DSACryptoServiceProvider;
				var actual = windows.ExportParameters (true);

				Assert.AreEqual (expected.Counter, actual.Counter, "Counter");
				AssertAreEqual (expected.Seed, actual.Seed, "Seed");
				AssertAreEqual (expected.G, actual.G, "G");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.X, actual.X, "X");
				AssertAreEqual (expected.Y, actual.Y, "Y");

				// test AsymmetricCipherKeyPair conversion
				var keyPair = dsa.AsAsymmetricCipherKeyPair ();
				windows = keyPair.AsAsymmetricAlgorithm () as DSACryptoServiceProvider;
				actual = windows.ExportParameters (true);

				Assert.AreEqual (expected.Counter, actual.Counter, "Counter");
				AssertAreEqual (expected.Seed, actual.Seed, "Seed");
				AssertAreEqual (expected.G, actual.G, "G");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.X, actual.X, "X");
				AssertAreEqual (expected.Y, actual.Y, "Y");

				// test public key conversion
				expected = dsa.ExportParameters (false);
				var pubdsa = new DSACryptoServiceProvider ();
				pubdsa.ImportParameters (expected);

				keyParameter = pubdsa.AsAsymmetricKeyParameter ();
				windows = keyParameter.AsAsymmetricAlgorithm () as DSACryptoServiceProvider;
				actual = windows.ExportParameters (false);

				Assert.AreEqual (expected.Counter, actual.Counter, "Counter");
				AssertAreEqual (expected.Seed, actual.Seed, "Seed");
				AssertAreEqual (expected.G, actual.G, "G");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.X, actual.X, "X");
				AssertAreEqual (expected.Y, actual.Y, "Y");
			}
		}

		[Test]
		public void TestRsa ()
		{
			using (var rsa = new RSACryptoServiceProvider (1024)) {
				// first, check private key conversion
				var expected = rsa.ExportParameters (true);
				var keyParameter = rsa.AsAsymmetricKeyParameter ();
				var windows = keyParameter.AsAsymmetricAlgorithm () as RSACryptoServiceProvider;
				var actual = windows.ExportParameters (true);

				AssertAreEqual (expected.D, actual.D, "D");
				AssertAreEqual (expected.DP, actual.DP, "DP");
				AssertAreEqual (expected.DQ, actual.DQ, "DQ");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.Exponent, actual.Exponent, "Exponent");
				AssertAreEqual (expected.InverseQ, actual.InverseQ, "InverseQ");
				AssertAreEqual (expected.Modulus, actual.Modulus, "Modulus");

				// test AsymmetricCipherKeyPair conversion
				var keyPair = rsa.AsAsymmetricCipherKeyPair ();
				windows = keyPair.AsAsymmetricAlgorithm () as RSACryptoServiceProvider;
				actual = windows.ExportParameters (true);

				AssertAreEqual (expected.D, actual.D, "D");
				AssertAreEqual (expected.DP, actual.DP, "DP");
				AssertAreEqual (expected.DQ, actual.DQ, "DQ");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.Exponent, actual.Exponent, "Exponent");
				AssertAreEqual (expected.InverseQ, actual.InverseQ, "InverseQ");
				AssertAreEqual (expected.Modulus, actual.Modulus, "Modulus");

				// test public key conversion
				expected = rsa.ExportParameters (false);
				var pubrsa = new RSACryptoServiceProvider ();
				pubrsa.ImportParameters (expected);

				keyParameter = pubrsa.AsAsymmetricKeyParameter ();
				windows = keyParameter.AsAsymmetricAlgorithm () as RSACryptoServiceProvider;
				actual = windows.ExportParameters (false);

				AssertAreEqual (expected.D, actual.D, "D");
				AssertAreEqual (expected.DP, actual.DP, "DP");
				AssertAreEqual (expected.DQ, actual.DQ, "DQ");
				AssertAreEqual (expected.P, actual.P, "P");
				AssertAreEqual (expected.Q, actual.Q, "Q");
				AssertAreEqual (expected.Exponent, actual.Exponent, "Exponent");
				AssertAreEqual (expected.InverseQ, actual.InverseQ, "InverseQ");
				AssertAreEqual (expected.Modulus, actual.Modulus, "Modulus");
			}
		}
	}
}
