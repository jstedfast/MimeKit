//
// RsaEncryptionPaddingTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Reflection;
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class RsaEncryptionPaddingTests
	{
		[Test]
		public void TestEquality ()
		{
			Assert.AreEqual (RsaEncryptionPadding.OaepSha1, RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), "CreateOaep(SHA-1)");
			Assert.AreEqual (RsaEncryptionPadding.OaepSha256, RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha256), "CreateOaep(SHA-256)");
			Assert.AreEqual (RsaEncryptionPadding.OaepSha384, RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha384), "CreateOaep(SHA-384)");
			Assert.AreEqual (RsaEncryptionPadding.OaepSha512, RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha512), "CreateOaep(SHA-512)");

			Assert.AreNotEqual (RsaEncryptionPadding.Pkcs1, RsaEncryptionPadding.OaepSha1, "PKCS1 !Equals SHA-1");
			Assert.AreNotEqual (RsaEncryptionPadding.Pkcs1, RsaEncryptionPadding.OaepSha256, "PKCS1 !Equals SHA-256");
			Assert.AreNotEqual (RsaEncryptionPadding.OaepSha1, RsaEncryptionPadding.OaepSha256, "SHA-1 !Equals SHA-256");

			Assert.AreNotEqual (RsaEncryptionPadding.Pkcs1, new object (), "PKCS1 !Equals object");

			Assert.IsTrue (RsaEncryptionPadding.OaepSha1 == RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), "SHA-1 == SHA-1");
			Assert.IsFalse (RsaEncryptionPadding.OaepSha1 == RsaEncryptionPadding.OaepSha256, "SHA-1 == SHA-256");
			Assert.IsFalse (RsaEncryptionPadding.OaepSha1 == null, "SHA-1 == null");
			Assert.IsFalse (null == RsaEncryptionPadding.OaepSha1, "null == SHA-1");

			Assert.IsFalse (RsaEncryptionPadding.OaepSha1 != RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), "SHA-1 != SHA-1");
			Assert.IsTrue (RsaEncryptionPadding.OaepSha1 != RsaEncryptionPadding.OaepSha256, "SHA-1 != SHA-256");
			Assert.IsTrue (RsaEncryptionPadding.OaepSha1 != null, "SHA-1 != null");
			Assert.IsTrue (null != RsaEncryptionPadding.OaepSha1, "null != SHA-1");
		}

		[Test]
		public void TestGetHashCode ()
		{
			var hashCodes = new Dictionary<int, RsaEncryptionPadding> ();

			foreach (var field in typeof (RsaEncryptionPadding).GetFields (BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType != typeof (RsaEncryptionPadding))
					continue;

				var padding = (RsaEncryptionPadding) field.GetValue (null);
				int hashCode = padding.GetHashCode ();

				if (hashCodes.TryGetValue (hashCode, out var other))
					Assert.Fail ($"{padding.Scheme} shares the same hash code as {other.Scheme}");

				hashCodes.Add (hashCode, padding);
			}
		}

		[Test]
		public void TestNotSupportedException ()
		{
			var supported = new HashSet<DigestAlgorithm> ();

			foreach (var field in typeof (RsaEncryptionPadding).GetFields (BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType != typeof (RsaEncryptionPadding))
					continue;

				var padding = (RsaEncryptionPadding) field.GetValue (null);

				if (padding.Scheme == RsaEncryptionPaddingScheme.Oaep)
					supported.Add (padding.OaepHashAlgorithm);
			}

			foreach (DigestAlgorithm hashAlgorithm in Enum.GetValues (typeof (DigestAlgorithm))) {
				if (!supported.Contains (hashAlgorithm))
					Assert.Throws<NotSupportedException> (() => RsaEncryptionPadding.CreateOaep (hashAlgorithm));
				else
					Assert.DoesNotThrow (() => RsaEncryptionPadding.CreateOaep (hashAlgorithm));
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual ("Pkcs1", RsaEncryptionPadding.Pkcs1.ToString (), "Pkcs1");
			Assert.AreEqual ("OaepSha1", RsaEncryptionPadding.OaepSha1.ToString (), "OaepSha1");
			Assert.AreEqual ("OaepSha256", RsaEncryptionPadding.OaepSha256.ToString (), "OaepSha256");
			Assert.AreEqual ("OaepSha384", RsaEncryptionPadding.OaepSha384.ToString (), "OaepSha384");
			Assert.AreEqual ("OaepSha512", RsaEncryptionPadding.OaepSha512.ToString (), "OaepSha512");
		}

		static void AssertOaepAlgorithmIdentifier (RsaEncryptionPadding padding, DerObjectIdentifier hashAlgorithm)
		{
			var name = $"Oaep{padding.Scheme}";

			var algorithm = padding.GetAlgorithmIdentifier ();
			Assert.IsNotNull (algorithm, $"{name} != null");
			Assert.AreEqual (PkcsObjectIdentifiers.IdRsaesOaep, algorithm.Algorithm, $"{name}.Algorithm == RSAES-OAEP");
			var parameters = (RsaesOaepParameters) algorithm.Parameters;
			Assert.AreEqual (hashAlgorithm, parameters.HashAlgorithm.Algorithm, $"{name}.HashAlgorithm == {padding.OaepHashAlgorithm}");
			Assert.AreEqual (PkcsObjectIdentifiers.IdMgf1, parameters.MaskGenAlgorithm.Algorithm, $"{name}.MaskGenAlgorithm == MGF1");
			var mgf1hash = (AlgorithmIdentifier) parameters.MaskGenAlgorithm.Parameters;
			Assert.AreEqual (hashAlgorithm, mgf1hash.Algorithm, $"{name}.MaskGenHashAlgorithm == {padding.OaepHashAlgorithm}");
		}

		[Test]
		public void TestGetAlgorithmIdentifier ()
		{
			Assert.IsNull (RsaEncryptionPadding.Pkcs1.GetAlgorithmIdentifier (), "Pkcs1");

			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha1, OiwObjectIdentifiers.IdSha1);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha256, NistObjectIdentifiers.IdSha256);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha384, NistObjectIdentifiers.IdSha384);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha512, NistObjectIdentifiers.IdSha512);
		}
	}
}
