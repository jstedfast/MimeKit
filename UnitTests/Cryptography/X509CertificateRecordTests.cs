//
// X509CertificateRecordTests.cs
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

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class X509CertificateRecordTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var signer = new CmsSigner (rsa.FileName, "no.secret");
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
			Assert.That (record.BasicConstraints, Is.EqualTo (certificate.GetBasicConstraints ()), "BasicConstraints");
			Assert.That (record.IsAnchor, Is.EqualTo (certificate.IsSelfSigned ()), "IsAnchor");
			Assert.That (record.KeyUsage, Is.EqualTo (certificate.GetKeyUsageFlags ()), "KeyUsage");
			Assert.That (record.NotBefore, Is.EqualTo (certificate.NotBefore.ToUniversalTime ()), "NotBefore");
			Assert.That (record.NotAfter, Is.EqualTo (certificate.NotAfter.ToUniversalTime ()), "NotAfter");
			Assert.That (record.IssuerName, Is.EqualTo (certificate.IssuerDN.ToString ()), "IssuerName");
			Assert.That (record.SerialNumber, Is.EqualTo (certificate.SerialNumber.ToString ()), "SerialNumber");
			Assert.That (record.SubjectName, Is.EqualTo (certificate.SubjectDN.ToString ()), "SubjectName");
			Assert.That (record.SubjectEmail, Is.EqualTo (certificate.GetSubjectEmailAddress ()), "SubjectEmail");
			Assert.That (record.Fingerprint, Is.EqualTo (certificate.GetFingerprint ()), "Fingerprint");
		}

		[Test]
		public void TestDefaultValues ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var signer = new CmsSigner (rsa.FileName, "no.secret");
			AsymmetricCipherKeyPair keyPair;
			X509CertificateRecord record;

			using (var stream = new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				keyPair = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			record = new X509CertificateRecord ();

			Assert.That (record.IsTrusted, Is.False, "IsTrusted #1");
			Assert.That (record.AlgorithmsUpdated, Is.EqualTo (DateTime.MinValue), "AlgorithmsUpdated #1");

			record = new X509CertificateRecord (signer.Certificate);

			Assert.That (record.IsTrusted, Is.False, "IsTrusted #2");
			Assert.That (record.Certificate, Is.EqualTo (signer.Certificate), "Certificate #2");
			Assert.That (record.AlgorithmsUpdated, Is.EqualTo (DateTime.MinValue), "AlgorithmsUpdated #2");
			AssertCertificateProperties (record, signer.Certificate);

			record = new X509CertificateRecord (signer.Certificate, signer.PrivateKey);

			Assert.That (record.IsTrusted, Is.False, "IsTrusted #3");
			Assert.That (record.PrivateKey, Is.EqualTo (signer.PrivateKey), "PrivateKey #3");
			Assert.That (record.Certificate, Is.EqualTo (signer.Certificate), "Certificate #3");
			Assert.That (record.AlgorithmsUpdated, Is.EqualTo (DateTime.MinValue), "AlgorithmsUpdated #3");
			AssertCertificateProperties (record, signer.Certificate);
		}
	}
}
