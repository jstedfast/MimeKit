//
// X509CertificateRecordTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2021 .NET Foundation and Contributors
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
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

using NUnit.Framework;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class X509CertificateRecordTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret");
			AsymmetricCipherKeyPair keyPair;

			using (var stream = new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				keyPair = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			Assert.Throws<ArgumentNullException> (() => new X509CrlRecord (null));
			Assert.Throws<ArgumentNullException> (() => new X509CertificateRecord (null));
			Assert.Throws<ArgumentNullException> (() => new X509CertificateRecord (null, signer.PrivateKey));
			Assert.Throws<ArgumentNullException> (() => new X509CertificateRecord (signer.Certificate, null));
			Assert.Throws<ArgumentException> (() => new X509CertificateRecord (signer.Certificate, keyPair.Public));
		}

		static void AssertCertificateProperties (X509CertificateRecord record, X509Certificate certificate)
		{
			Assert.AreEqual (certificate.GetBasicConstraints (), record.BasicConstraints, "BasicConstraints");
			Assert.AreEqual (certificate.IsSelfSigned (), record.IsAnchor, "IsAnchor");
			Assert.AreEqual (certificate.GetKeyUsageFlags (), record.KeyUsage, "KeyUsage");
			Assert.AreEqual (certificate.NotBefore.ToUniversalTime (), record.NotBefore, "NotBefore");
			Assert.AreEqual (certificate.NotAfter.ToUniversalTime (), record.NotAfter, "NotAfter");
			Assert.AreEqual (certificate.IssuerDN.ToString (), record.IssuerName, "IssuerName");
			Assert.AreEqual (certificate.SerialNumber.ToString (), record.SerialNumber, "SerialNumber");
			Assert.AreEqual (certificate.SubjectDN.ToString (), record.SubjectName, "SubjectName");
			Assert.AreEqual (certificate.GetSubjectEmailAddress (), record.SubjectEmail, "SubjectEmail");
			Assert.AreEqual (certificate.GetFingerprint (), record.Fingerprint, "Fingerprint");
		}

		[Test]
		public void TestDefaultValues ()
		{
			var signer = new CmsSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.pfx"), "no.secret");
			AsymmetricCipherKeyPair keyPair;
			X509CertificateRecord record;

			using (var stream = new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				keyPair = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			record = new X509CertificateRecord ();

			Assert.IsFalse (record.IsTrusted, "IsTrusted #1");
			Assert.AreEqual (DateTime.MinValue, record.AlgorithmsUpdated, "AlgorithmsUpdated #1");

			record = new X509CertificateRecord (signer.Certificate);

			Assert.IsFalse (record.IsTrusted, "IsTrusted #2");
			Assert.AreEqual (signer.Certificate, record.Certificate, "Certificate #2");
			Assert.AreEqual (DateTime.MinValue, record.AlgorithmsUpdated, "AlgorithmsUpdated #2");
			AssertCertificateProperties (record, signer.Certificate);

			record = new X509CertificateRecord (signer.Certificate, signer.PrivateKey);

			Assert.IsFalse (record.IsTrusted, "IsTrusted #3");
			Assert.AreEqual (signer.PrivateKey, record.PrivateKey, "PrivateKey #3");
			Assert.AreEqual (signer.Certificate, record.Certificate, "Certificate #3");
			Assert.AreEqual (DateTime.MinValue, record.AlgorithmsUpdated, "AlgorithmsUpdated #3");
			AssertCertificateProperties (record, signer.Certificate);
		}
	}
}
