//
// BouncyCastleCertificateExtensionTests.cs
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

using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;

using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509KeyUsageFlags = MimeKit.Cryptography.X509KeyUsageFlags;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class CertificateExtensionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.AsX509Certificate2 (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetIssuerNameInfo (null, X509Name.CN));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectNameInfo (null, X509Name.CN));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetCommonName (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectName (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectEmailAddress (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetFingerprint (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetKeyUsageFlags ((X509Certificate) null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetEncryptionAlgorithms (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetPublicKeyAlgorithm (null));

			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.AsBouncyCastleCertificate (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetEncryptionAlgorithms (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetPublicKeyAlgorithm (null));
		}

		static X509KeyUsageFlags GetX509Certificate2KeyUsageFlags (X509Certificate2 certificate)
		{
			if (certificate.Extensions[X509Extensions.KeyUsage.Id] is X509KeyUsageExtension usage)
				return (X509KeyUsageFlags) usage.KeyUsages;

			return BouncyCastleCertificateExtensions.GetKeyUsageFlags ((bool[]) null);
		}

		[Test]
		public void TestCertificateConversion ()
		{
			var fileNames = new string[] { "StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt" };
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var parser = new X509CertificateParser ();

			foreach (var fileName in fileNames) {
				using (var stream = File.OpenRead (Path.Combine (dataDir, fileName))) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream)) {
						var certificate2 = certificate.AsX509Certificate2 ();
						var certificate1 = certificate2.AsBouncyCastleCertificate ();

						Assert.That (certificate1.GetFingerprint ().ToUpperInvariant (), Is.EqualTo (certificate2.Thumbprint), "Fingerprint");
						Assert.That (certificate1.GetIssuerNameInfo (X509Name.EmailAddress), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, true)), "Issuer Email");
						Assert.That (certificate1.GetSubjectEmailAddress (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, false)), "Subject Email");
						Assert.That (certificate1.GetCommonName (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.SimpleName, false)), "Common Name");

						var usage2 = GetX509Certificate2KeyUsageFlags (certificate2);
						var usage1 = certificate1.GetKeyUsageFlags ();

						Assert.That (usage1, Is.EqualTo (usage2), "KeyUsageFlags");
					}
				}
			}
		}
	}
}
