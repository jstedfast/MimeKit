﻿//
// SecureMimeDigitalCertificateTests.cs
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
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.OpenSsl;

using NUnit.Framework;

using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class SecureMimeDigitalCertificateTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var signer = new CmsSigner (Path.Combine ("..", "..", "TestData", "smime", "smime.p12"), "no.secret");

			Assert.Throws<ArgumentNullException> (() => new SecureMimeDigitalCertificate (null));
			Assert.Throws<ArgumentNullException> (() => new SecureMimeDigitalSignature (null, signer.Certificate));

			Assert.Throws<ArgumentNullException> (() => new WindowsSecureMimeDigitalCertificate (null));
			Assert.Throws<ArgumentNullException> (() => new WindowsSecureMimeDigitalSignature (null));
		}

		static X509Certificate GetCertificate (string fileName)
		{
			using (var stream = File.OpenText (Path.Combine ("..", "..", "TestData", "smime", fileName))) {
				var reader = new PemReader (stream);
				object item;

				while ((item = reader.ReadObject ()) != null) {
					var certificate = item as X509Certificate;

					if (certificate != null)
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

				Assert.AreEqual (PublicKeyAlgorithm.Dsa, digital.PublicKeyAlgorithm, "PublicKeyAlgorithm: {0}", fileName);

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.AreEqual (PublicKeyAlgorithm.Dsa, digital2.PublicKeyAlgorithm, "Windows PublicKeyAlgorithm {0}", fileName);
			}

			foreach (var fileName in rsa) {
				certificate = GetCertificate (fileName);
				digital = new SecureMimeDigitalCertificate (certificate);

				Assert.AreEqual (PublicKeyAlgorithm.RsaGeneral, digital.PublicKeyAlgorithm, "PublicKeyAlgorithm: {0}", fileName);

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.AreEqual (PublicKeyAlgorithm.RsaGeneral, digital2.PublicKeyAlgorithm, "Windows PublicKeyAlgorithm {0}", fileName);
			}

			foreach (var fileName in ec) {
				certificate = GetCertificate (fileName);
				digital = new SecureMimeDigitalCertificate (certificate);

				Assert.AreEqual (PublicKeyAlgorithm.EllipticCurve, digital.PublicKeyAlgorithm, "PublicKeyAlgorithm: {0}", fileName);

				certificate2 = certificate.AsX509Certificate2 ();
				digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

				Assert.AreEqual (PublicKeyAlgorithm.EllipticCurve, digital2.PublicKeyAlgorithm, "Windows PublicKeyAlgorithm {0}", fileName);
			}

			//foreach (var fileName in dh) {
			//	certificate = GetCertificate (fileName);
			//	digital = new SecureMimeDigitalCertificate (certificate);

			//	Assert.AreEqual (PublicKeyAlgorithm.DiffieHellman, digital.PublicKeyAlgorithm, "PublicKeyAlgorithm: {0}", fileName);

			//	certificate2 = certificate.AsX509Certificate2 ();
			//	digital2 = new WindowsSecureMimeDigitalCertificate (certificate2);

			//	Assert.AreEqual (PublicKeyAlgorithm.DiffieHellman, digital2.PublicKeyAlgorithm, "Windows PublicKeyAlgorithm {0}", fileName);
			//}
		}
	}
}
