//
// X509CertificateStoreTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;

using NUnit.Framework;

using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class X509CertificateStoreTests
	{
		static readonly string[] CertificateAuthorities = new string[] {
			"certificate-authority.crt", "StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		static string GetTestDataPath (string relative)
		{
			return Path.Combine ("..", "..", "TestData", "smime", relative);
		}

		[Test]
		public void TestImportSingleCertificate ()
		{
			var store = new X509CertificateStore ();

			store.Import (GetTestDataPath (CertificateAuthorities[0]));
			var certificate = store.Certificates.FirstOrDefault ();
			var count = store.Certificates.Count ();

			Assert.AreEqual (1, count, "Unexpected number of certificates imported.");
			Assert.AreEqual ("bruce.wayne@example.com", certificate.GetSubjectEmailAddress (), "Unexpected email address for certificate.");
		}

		[Test]
		public void TestImportExportMultipleCertificates ()
		{
			var store = new X509CertificateStore ();

			foreach (var authority in CertificateAuthorities)
				store.Import (GetTestDataPath (authority));

			var count = store.Certificates.Count ();

			Assert.AreEqual (CertificateAuthorities.Length, count, "Unexpected number of certificates imported.");

			store.Export ("exported.crt");

			var imported = new X509CertificateStore ();
			imported.Import ("exported.crt");

			count = imported.Certificates.Count ();

			Assert.AreEqual (CertificateAuthorities.Length, count, "Unexpected number of certificates re-imported.");
		}

		[Test]
		public void TestImportExportPkcs12 ()
		{
			var store = new X509CertificateStore ();

			store.Import (GetTestDataPath ("smime.p12"), "no.secret");
			var certificate = store.Certificates.FirstOrDefault ();
			var count = store.Certificates.Count ();

			Assert.AreEqual (1, count, "Unexpected number of certificates imported.");
			Assert.IsNotNull (store.GetPrivateKey (certificate), "Failed to get private key.");

			foreach (var authority in CertificateAuthorities)
				store.Import (GetTestDataPath (authority));

			store.Export ("exported.p12", "no.secret");

			var imported = new X509CertificateStore ();
			imported.Import ("exported.p12", "no.secret");

			count = imported.Certificates.Count ();

			Assert.AreEqual (store.Certificates.Count (), count, "Unexpected number of certificates re-imported.");
			Assert.IsNotNull (imported.GetPrivateKey (certificate), "Failed to get private key after re-importing.");
		}
	}
}
