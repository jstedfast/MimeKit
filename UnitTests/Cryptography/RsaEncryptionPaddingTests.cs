//
// RsaEncryptionPaddingTests.cs
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

using System.Reflection;

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
			Assert.That (RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), Is.EqualTo (RsaEncryptionPadding.OaepSha1), "CreateOaep(SHA-1)");
			Assert.That (RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha256), Is.EqualTo (RsaEncryptionPadding.OaepSha256), "CreateOaep(SHA-256)");
			Assert.That (RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha384), Is.EqualTo (RsaEncryptionPadding.OaepSha384), "CreateOaep(SHA-384)");
			Assert.That (RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha512), Is.EqualTo (RsaEncryptionPadding.OaepSha512), "CreateOaep(SHA-512)");

			Assert.That (RsaEncryptionPadding.OaepSha1, Is.Not.EqualTo (RsaEncryptionPadding.Pkcs1), "PKCS1 !Equals SHA-1");
			Assert.That (RsaEncryptionPadding.OaepSha256, Is.Not.EqualTo (RsaEncryptionPadding.Pkcs1), "PKCS1 !Equals SHA-256");
			Assert.That (RsaEncryptionPadding.OaepSha256, Is.Not.EqualTo (RsaEncryptionPadding.OaepSha1), "SHA-1 !Equals SHA-256");

			Assert.That (new object (), Is.Not.EqualTo (RsaEncryptionPadding.Pkcs1), "PKCS1 !Equals object");

			Assert.That (RsaEncryptionPadding.OaepSha1 == RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), Is.True, "SHA-1 == SHA-1");
			Assert.That (RsaEncryptionPadding.OaepSha1 == RsaEncryptionPadding.OaepSha256, Is.False, "SHA-1 == SHA-256");
			Assert.That (RsaEncryptionPadding.OaepSha1 == null, Is.False, "SHA-1 == null");
			Assert.That (null == RsaEncryptionPadding.OaepSha1, Is.False, "null == SHA-1");

			Assert.That (RsaEncryptionPadding.OaepSha1 != RsaEncryptionPadding.CreateOaep (DigestAlgorithm.Sha1), Is.False, "SHA-1 != SHA-1");
			Assert.That (RsaEncryptionPadding.OaepSha1 != RsaEncryptionPadding.OaepSha256, Is.True, "SHA-1 != SHA-256");
			Assert.That (RsaEncryptionPadding.OaepSha1 != null, Is.True, "SHA-1 != null");
			Assert.That (null != RsaEncryptionPadding.OaepSha1, Is.True, "null != SHA-1");
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
			Assert.That (RsaEncryptionPadding.Pkcs1.ToString (), Is.EqualTo ("Pkcs1"), "Pkcs1");
			Assert.That (RsaEncryptionPadding.OaepSha1.ToString (), Is.EqualTo ("OaepSha1"), "OaepSha1");
			Assert.That (RsaEncryptionPadding.OaepSha256.ToString (), Is.EqualTo ("OaepSha256"), "OaepSha256");
			Assert.That (RsaEncryptionPadding.OaepSha384.ToString (), Is.EqualTo ("OaepSha384"), "OaepSha384");
			Assert.That (RsaEncryptionPadding.OaepSha512.ToString (), Is.EqualTo ("OaepSha512"), "OaepSha512");
		}

		static void AssertOaepAlgorithmIdentifier (RsaEncryptionPadding padding, DerObjectIdentifier hashAlgorithm)
		{
			var name = $"Oaep{padding.Scheme}";

			var algorithm = padding.GetAlgorithmIdentifier ();
			Assert.That (algorithm, Is.Not.Null, $"{name} != null");
			Assert.That (algorithm.Algorithm, Is.EqualTo (PkcsObjectIdentifiers.IdRsaesOaep), $"{name}.Algorithm == RSAES-OAEP");
			var parameters = (RsaesOaepParameters) algorithm.Parameters;
			Assert.That (parameters.HashAlgorithm.Algorithm, Is.EqualTo (hashAlgorithm), $"{name}.HashAlgorithm == {padding.OaepHashAlgorithm}");
			Assert.That (parameters.MaskGenAlgorithm.Algorithm, Is.EqualTo (PkcsObjectIdentifiers.IdMgf1), $"{name}.MaskGenAlgorithm == MGF1");
			var mgf1hash = (AlgorithmIdentifier) parameters.MaskGenAlgorithm.Parameters;
			Assert.That (mgf1hash.Algorithm, Is.EqualTo (hashAlgorithm), $"{name}.MaskGenHashAlgorithm == {padding.OaepHashAlgorithm}");
		}

		[Test]
		public void TestGetAlgorithmIdentifier ()
		{
			Assert.That (RsaEncryptionPadding.Pkcs1.GetAlgorithmIdentifier (), Is.Null, "Pkcs1");

			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha1, OiwObjectIdentifiers.IdSha1);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha256, NistObjectIdentifiers.IdSha256);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha384, NistObjectIdentifiers.IdSha384);
			AssertOaepAlgorithmIdentifier (RsaEncryptionPadding.OaepSha512, NistObjectIdentifiers.IdSha512);
		}
	}
}
