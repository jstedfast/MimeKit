//
// SqliteCertificateDatabaseTests.cs
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
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class SqliteCertificateDatabaseTests : IDisposable
	{
		static readonly string[] StartComCertificates = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		readonly X509Certificate[] chain;
		readonly string dataDir;

		public SqliteCertificateDatabaseTests ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);
			dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");

			if (File.Exists ("sqlite.db"))
				File.Delete ("sqlite.db");

			chain = rsa.Chain;

			using (var ctx = new DefaultSecureMimeContext ("sqlite.db", "no.secret")) {
				foreach (var filename in StartComCertificates) {
					var path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path))
						ctx.Import (stream, true);
				}

				ctx.Import (rsa.FileName, "no.secret");
			}
		}

		public void Dispose ()
		{
			if (File.Exists ("sqlite.db"))
				File.Delete ("sqlite.db");

			GC.SuppressFinalize (this);
		}

		[Test]
		public void TestAutoUpgrade ()
		{
			var path = Path.Combine (dataDir, "smimev0.db");
			const string tmp = "smimev0-tmp.db";

			if (File.Exists (tmp))
				File.Delete (tmp);

			File.Copy (path, tmp);

			using (var dbase = new SqliteCertificateDatabase (tmp, "no.secret")) {
				var root = chain[chain.Length - 1];

				// Verify that we can select the Root Certificate
				bool trustedAnchor = false;
				foreach (var record in dbase.Find (null, true, X509CertificateRecordFields.Certificate)) {
					if (record.Certificate.Equals (root)) {
						trustedAnchor = true;
						break;
					}
				}

				Assert.That (trustedAnchor, Is.True, "Did not find the MimeKit UnitTests trusted anchor");
			}
		}

		static void AssertFindBy (Org.BouncyCastle.Utilities.Collections.ISelector<X509Certificate> selector, X509Certificate expected)
		{
			using (var dbase = new SqliteCertificateDatabase ("sqlite.db", "no.secret")) {
				// Verify that we can select the Root Certificate
				bool found = false;
				foreach (var record in dbase.Find (selector, false, X509CertificateRecordFields.Certificate)) {
					if (record.Certificate.Equals (expected)) {
						found = true;
						break;
					}
				}

				Assert.That (found, Is.True, "Did not find the expected certificate");
			}
		}

		[Test]
		public void TestFindByBasicConstraints ()
		{
			foreach (var certificate in chain) {
				var basicConstraints = certificate.GetBasicConstraints ();
				if (basicConstraints == -1)
					basicConstraints = -2;

				var selector = new X509CertStoreSelector {
					BasicConstraints = basicConstraints
				};

				AssertFindBy (selector, certificate);
			}
		}

		[Test]
		public void TestFindByCertificateValid ()
		{
			var selector = new X509CertStoreSelector {
				CertificateValid = chain[0].NotBefore.AddDays (10)
			};

			AssertFindBy (selector, chain[0]);
		}

		[Test]
		public void TestFindBySubjectName ()
		{
			var selector = new X509CertStoreSelector {
				Subject = chain[0].SubjectDN
			};

			AssertFindBy (selector, chain[0]);
		}

		[Test]
		public void TestFindBySubjectKeyIdentifier ()
		{
			var subjectKeyIdentifier = chain[0].GetExtensionValue (X509Extensions.SubjectKeyIdentifier);
			var selector = new X509CertStoreSelector {
				SubjectKeyIdentifier = subjectKeyIdentifier.GetOctets ()
			};

			AssertFindBy (selector, chain[0]);
		}
	}
}
