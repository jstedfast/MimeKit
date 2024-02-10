//
// SecureMimeDigitalCertificateTests.cs
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

using Org.BouncyCastle.OpenSsl;

using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class SecureMimeDigitalCertificateTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var signer = new CmsSigner (rsa.FileName, "no.secret");

			Assert.Throws<ArgumentNullException> (() => new SecureMimeDigitalCertificate (null));
			Assert.Throws<ArgumentNullException> (() => new SecureMimeDigitalSignature (null, signer.Certificate));

			Assert.Throws<ArgumentNullException> (() => new WindowsSecureMimeDigitalCertificate (null));
			Assert.Throws<ArgumentNullException> (() => new WindowsSecureMimeDigitalSignature (null));
		}

		static X509Certificate GetCertificate (string fileName)
		{
			using (var stream = File.OpenText (Path.Combine (TestHelper.ProjectDir, "TestData", "smime", fileName))) {
				var reader = new PemReader (stream);
				object item;

				while ((item = reader.ReadObject ()) != null) {
					if (item is X509Certificate certificate)
						return certificate;
				}
			}

			return null;
		}

		[Test]
		public void TestPublicKeyAlgorithmDetection ()
		{
			var dsa = new string[] { "smdsa1.pem", "smdsa2.pem", "smdsa3.pem" };
			var rsa = new string[] { "smrsa1.pem", "smrsa2.pem", "smrsa3.pem" };
			var ec = new string[] { "smec1.pem", "smec2.pem", "smec3.pem" };
			//var dh = new string[] { "smdh.pem" };
			WindowsSecureMimeDigitalCertificate digital2;
			SecureMimeDigitalCertificate digital;
			X509Certificate2 certificate2;
			X509Certificate certificate;

			foreach (var fileName in dsa) {
				certificate = GetCertificate (fileName);
				digital = new SecureMimeDigitalCertificate (certificate);

				Assert.That (digital.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.Dsa), $"PublicKeyAlgorithm: {fileName}");

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.That (digital2.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.Dsa), $"Windows PublicKeyAlgorithm {fileName}");
			}

			foreach (var fileName in rsa) {
				certificate = GetCertificate (fileName);
				digital = new SecureMimeDigitalCertificate (certificate);

				Assert.That (digital.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.RsaGeneral), $"PublicKeyAlgorithm: {fileName}");

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.That (digital2.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.RsaGeneral), $"Windows PublicKeyAlgorithm {fileName}");
			}

			foreach (var fileName in ec) {
				certificate = GetCertificate (fileName);
				digital = new SecureMimeDigitalCertificate (certificate);

				Assert.That (digital.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.EllipticCurve), $"PublicKeyAlgorithm: {fileName}");

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.That (digital2.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.EllipticCurve), $"Windows PublicKeyAlgorithm {fileName}");
			}

			//foreach (var fileName in dh) {
			//	certificate = GetCertificate (fileName);
			//	digital = new SecureMimeDigitalCertificate (certificate);

			//	Assert.That (digital.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.DiffieHellman), $"PublicKeyAlgorithm: {fileName}");

			//	certificate2 = certificate.AsX509Certificate2 ();
			//	digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

			//	Assert.That (digital2.PublicKeyAlgorithm, Is.EqualTo (PublicKeyAlgorithm.DiffieHellman), $"Windows PublicKeyAlgorithm {fileName}");
			//}
		}
	}
}
