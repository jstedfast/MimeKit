//
// X509CertificateStoreTests.cs
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

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class X509CertificateStoreTests
	{
		static readonly string[] CertificateAuthorities = new string[] {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		static string GetTestDataPath (string relative)
		{
			return Path.Combine (TestHelper.ProjectDir, "TestData", "smime", relative);
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var store = new X509CertificateStore ();

			Assert.Throws<ArgumentNullException> (() => store.Add (null));
			Assert.Throws<ArgumentNullException> (() => store.AddRange (null));
			Assert.Throws<ArgumentNullException> (() => store.Export ((Stream) null, "password"));
			Assert.Throws<ArgumentNullException> (() => store.Export ((string) null, "password"));
			Assert.Throws<ArgumentNullException> (() => store.Export (Stream.Null, null));
			Assert.Throws<ArgumentNullException> (() => store.Export ("fileName", null));
			Assert.Throws<ArgumentNullException> (() => store.Export ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => store.Export ((string) null));
			Assert.Throws<ArgumentNullException> (() => store.GetPrivateKey (null));
			Assert.Throws<ArgumentNullException> (() => store.Import ((Stream) null, "password"));
			Assert.Throws<ArgumentNullException> (() => store.Import ((string) null, "password"));
			Assert.Throws<ArgumentNullException> (() => store.Import ((byte[]) null, "password"));
			Assert.Throws<ArgumentNullException> (() => store.Import (Stream.Null, null));
			Assert.Throws<ArgumentNullException> (() => store.Import (rsa.FileName, null));
			Assert.Throws<ArgumentNullException> (() => store.Import (Array.Empty<byte> (), null));
			Assert.Throws<ArgumentNullException> (() => store.Import ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => store.Import ((string) null));
			Assert.Throws<ArgumentNullException> (() => store.Import ((byte[]) null));
			Assert.Throws<ArgumentNullException> (() => store.Remove (null));
			Assert.Throws<ArgumentNullException> (() => store.RemoveRange (null));
		}

		[Test]
		public void TestAddRemove ()
		{
			var certificates = new List<X509Certificate> ();
			var parser = new X509CertificateParser ();
			var store = new X509CertificateStore ();

			foreach (var authority in CertificateAuthorities) {
				var path = GetTestDataPath (authority);

				using (var stream = File.OpenRead (path)) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream))
						certificates.Add (certificate);
				}
			}

			foreach (var certificate in certificates)
				store.Add (certificate);

			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (CertificateAuthorities.Length), "Unexpected number of certificates after Add.");

			foreach (var certificate in certificates) {
				var key = store.GetPrivateKey (certificate);
				Assert.That (key, Is.Null, "GetPrivateKey");
				store.Remove (certificate);
			}

			count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (0), "Unexpected number of certificates after Remove.");
		}

		[Test]
		public void TestAddRemoveRange ()
		{
			var certificates = new List<X509Certificate> ();
			var parser = new X509CertificateParser ();
			var store = new X509CertificateStore ();

			foreach (var authority in CertificateAuthorities) {
				var path = GetTestDataPath (authority);

				using (var stream = File.OpenRead (path)) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream))
						certificates.Add (certificate);
				}
			}

			store.AddRange (certificates);

			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (CertificateAuthorities.Length), "Unexpected number of certificates after AddRange.");

			foreach (var certificate in certificates) {
				var key = store.GetPrivateKey (certificate);
				Assert.That (key, Is.Null, "GetPrivateKey");
			}

			store.RemoveRange (certificates);

			count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (0), "Unexpected number of certificates after RemoveRange.");
		}

		[Test]
		public void TestImportData ()
		{
			var store = new X509CertificateStore ();

			store.Import (File.ReadAllBytes (GetTestDataPath (CertificateAuthorities[0])));
			var certificate = store.Certificates.FirstOrDefault ();
			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (1), "Unexpected number of certificates imported.");
			Assert.That (certificate.GetCommonName (), Is.EqualTo ("StartCom Certification Authority"), "Unexpected CN for certificate.");
		}

		[Test]
		public void TestImportSingleCertificate ()
		{
			var store = new X509CertificateStore ();

			store.Import (GetTestDataPath (CertificateAuthorities[0]));
			var certificate = store.Certificates.FirstOrDefault ();
			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (1), "Unexpected number of certificates imported.");
			Assert.That (certificate.GetCommonName (), Is.EqualTo ("StartCom Certification Authority"), "Unexpected CN for certificate.");
		}

		[Test]
		public void TestImportExportMultipleCertificates ()
		{
			var store = new X509CertificateStore ();

			foreach (var authority in CertificateAuthorities)
				store.Import (GetTestDataPath (authority));

			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (CertificateAuthorities.Length), "Unexpected number of certificates imported.");

			try {
				store.Export ("exported.crt");

				var imported = new X509CertificateStore ();
				imported.Import ("exported.crt");

				count = imported.Certificates.Count ();

				Assert.That (count, Is.EqualTo (CertificateAuthorities.Length), "Unexpected number of certificates re-imported.");
			} finally {
				if (File.Exists ("exported.crt"))
					File.Delete ("exported.crt");
			}
		}

		[Test]
		public void TestImportExportPkcs12 ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			var store = new X509CertificateStore ();

			store.Import (rsa.FileName, "no.secret");
			var certificate = store.Certificates.FirstOrDefault ();
			var count = store.Certificates.Count ();

			Assert.That (count, Is.EqualTo (4), "Unexpected number of certificates imported.");
			Assert.That (store.GetPrivateKey (certificate), Is.Not.Null, "Failed to get private key.");

			foreach (var authority in CertificateAuthorities)
				store.Import (GetTestDataPath (authority));

			try {
				store.Export ("exported.p12", "no.secret");

				var imported = new X509CertificateStore ();
				imported.Import ("exported.p12", "no.secret");

				count = imported.Certificates.Count ();

				Assert.That (count, Is.EqualTo (store.Certificates.Count ()), "Unexpected number of certificates re-imported.");
				Assert.That (imported.GetPrivateKey (certificate), Is.Not.Null, "Failed to get private key after re-importing.");
			} finally {
				if (File.Exists ("exported.p12"))
					File.Delete ("exported.p12");
			}
		}
	}
}
