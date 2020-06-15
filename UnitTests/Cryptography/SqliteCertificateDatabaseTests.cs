//
// SqliteCertificateDatabaseTests.cs
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
using System.IO;

using NUnit.Framework;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities.Date;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class SqliteCertificateDatabaseTests
	{
		static readonly string[] StartComCertificates = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		readonly X509Certificate[] chain;
		readonly string dataDir;

		public SqliteCertificateDatabaseTests ()
		{
			dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var path = Path.Combine (dataDir, "smime.pfx");

			if (File.Exists ("sqlite.db"))
				File.Delete ("sqlite.db");

			chain = SecureMimeTestsBase.LoadPkcs12CertificateChain (path, "no.secret");

			using (var ctx = new DefaultSecureMimeContext ("sqlite.db", "no.secret")) {
				foreach (var filename in StartComCertificates) {
					path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path))
						ctx.Import (stream, true);
				}

				path = Path.Combine (dataDir, "smime.pfx");
				ctx.Import (path, "no.secret");
			}
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

				Assert.IsTrue (trustedAnchor, "Did not find the MimeKit UnitTests trusted anchor");
			}
		}

		void AssertFindBy (IX509Selector selector, X509Certificate expected)
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

				Assert.IsTrue (found, "Did not find the expected certificate");
			}
		}

		[Test]
		public void TestFindByBasicConstraints ()
		{
			foreach (var certificate in chain) {
				var basicConstraints = certificate.GetBasicConstraints ();
				if (basicConstraints == -1)
					basicConstraints = -2;

				var selector = new X509CertStoreSelector ();
				selector.BasicConstraints = basicConstraints;

				AssertFindBy (selector, certificate);
			}
		}

		[Test]
		public void TestFindByCertificateValid ()
		{
			var selector = new X509CertStoreSelector ();
			selector.CertificateValid = new DateTimeObject (chain[0].NotBefore.AddDays (10));

			AssertFindBy (selector, chain[0]);
		}

		[Test]
		public void TestFindBySubjectName ()
		{
			var selector = new X509CertStoreSelector ();
			selector.Subject = chain[0].SubjectDN;

			AssertFindBy (selector, chain[0]);
		}

		[Test]
		public void TestFindBySubjectKeyIdentifier ()
		{
			var subjectKeyIdentifier = chain[0].GetExtensionValue (X509Extensions.SubjectKeyIdentifier);
			var selector = new X509CertStoreSelector ();
			selector.SubjectKeyIdentifier = subjectKeyIdentifier.GetOctets ();

			AssertFindBy (selector, chain[0]);
		}
	}
}
