//
// SqliteCertificateDatabaseTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class SqliteCertificateDatabaseTests : IDisposable
	{
		class TestSqliteCertificateDatabase : SqliteCertificateDatabase
		{
			public TestSqliteCertificateDatabase (string fileName, string password, SecureRandom random) : base (fileName, password, random)
			{
			}

			public object GetCertificateValue (X509CertificateRecord record, string columnName)
			{
				return GetValue (record, columnName);
			}

			public object GetCrlValue (X509CrlRecord record, string columnName)
			{
				return GetValue (record, columnName);
			}

			public void ExecuteFailingTransaction ()
			{
				ExecuteWithinTransaction (() => {
					using (var command = CreateCommand ()) {
						command.CommandText = "CREATE TABLE ROLLBACK_TEST (ID INTEGER)";
						ExecuteNonQuery (command);
					}

					throw new InvalidOperationException ("Rollback this transaction.");
				});
			}

			public bool TableExists (string name)
			{
				using (var command = CreateCommand ()) {
					command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '" + name + "'";
					return Convert.ToInt64 (command.ExecuteScalar ()) != 0;
				}
			}
		}

		static readonly string[] StartComCertificates = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		static readonly string TempDir = Path.Combine (Path.GetTempPath (), "MimeKit-" + Guid.NewGuid ().ToString ("N"));
		static readonly string DatabasePath = Path.Combine (TempDir, "sqlite.db");
		readonly X509Certificate[] chain;
		readonly string dataDir;

		public SqliteCertificateDatabaseTests ()
		{
			var rsa = SecureMimeTestsBase.RsaCertificate;
			dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");

			chain = rsa.Chain;

			using (var ctx = new DefaultSecureMimeContext (DatabasePath, "no.secret")) {
				foreach (var filename in StartComCertificates) {
					var path = Path.Combine (dataDir, filename);
					using (var stream = File.OpenRead (path))
						ctx.Import (stream, true);
				}

				ctx.Import (rsa.FileName, "no.secret");

				foreach (var crl in SecureMimeTestsBase.ObsoleteCrls)
					ctx.Import (crl);
			}
		}

		public void Dispose ()
		{
			if (Directory.Exists (TempDir))
				Directory.Delete (TempDir, true);

			GC.SuppressFinalize (this);
		}

		static string GetUniqueDatabasePath ()
		{
			return Path.Combine (TempDir, Guid.NewGuid ().ToString ("N"), "sqlite.db");
		}

		static void DeleteDatabaseDirectory (string path)
		{
			var directory = Path.GetDirectoryName (path);

			if (!string.IsNullOrEmpty (directory) && Directory.Exists (directory))
				Directory.Delete (directory, true);
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var nullPasswordPath = GetUniqueDatabasePath ();
			var nullRandomPath = GetUniqueDatabasePath ();

			try {
				Assert.Throws<ArgumentNullException> (() => SqliteCertificateDatabase.CreateConnection (null));
				Assert.Throws<ArgumentException> (() => SqliteCertificateDatabase.CreateConnection (string.Empty));
				Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase (nullPasswordPath, null));
				Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase (nullRandomPath, "no.secret", null));

				// Invalid arguments should not leave behind an empty database file.
				Assert.That (File.Exists (nullPasswordPath), Is.False, "null password created a database file");
				Assert.That (File.Exists (nullRandomPath), Is.False, "null random created a database file");
			} finally {
				DeleteDatabaseDirectory (nullPasswordPath);
				DeleteDatabaseDirectory (nullRandomPath);
			}
		}

		[Test]
		public void TestCreateConnectionCreatesMissingDirectories ()
		{
			var path = GetUniqueDatabasePath ();

			try {
				using (var connection = SqliteCertificateDatabase.CreateConnection (path))
					Assert.That (connection.ConnectionString, Does.Contain ("sqlite.db"));

				Assert.That (File.Exists (path), Is.True, "Database file");
			} finally {
				DeleteDatabaseDirectory (path);
			}
		}

		[Test]
		public void TestProtectedHelpers ()
		{
			var path = GetUniqueDatabasePath ();

			try {
				using (var dbase = new TestSqliteCertificateDatabase (path, "no.secret", new SecureRandom ())) {
					var certificateException = Assert.Throws<ArgumentException> (() => dbase.GetCertificateValue (new X509CertificateRecord (), "UNKNOWN"));
					var crlException = Assert.Throws<ArgumentException> (() => dbase.GetCrlValue (new X509CrlRecord (), "UNKNOWN"));
					var rollbackException = Assert.Throws<InvalidOperationException> (() => dbase.ExecuteFailingTransaction ());

					Assert.That (certificateException.ParamName, Is.EqualTo ("columnName"), "Certificate ParamName");
					Assert.That (certificateException.Message, Is.EqualTo ("Unknown column name: UNKNOWN (Parameter 'columnName')"), "Certificate Message");
					Assert.That (crlException.ParamName, Is.EqualTo ("columnName"), "CRL ParamName");
					Assert.That (crlException.Message, Is.EqualTo ("Unknown column name: UNKNOWN (Parameter 'columnName')"), "CRL Message");
					Assert.That (rollbackException.Message, Is.EqualTo ("Rollback this transaction."));
					Assert.That (dbase.TableExists ("ROLLBACK_TEST"), Is.False, "ROLLBACK_TEST table");
				}
			} finally {
				DeleteDatabaseDirectory (path);
			}
		}

		[Test]
		public void TestObjectDisposedExceptions ()
		{
			var path = GetUniqueDatabasePath ();

			try {
				var dbase = new SqliteCertificateDatabase (path, "no.secret");

				dbase.Dispose ();

				var exception = Assert.Throws<ObjectDisposedException> (() => dbase.Find (null, false, X509CertificateRecordFields.Certificate).ToList ());

				Assert.That (exception.ObjectName, Is.EqualTo ("SqliteCertificateDatabase"));
			} finally {
				DeleteDatabaseDirectory (path);
			}
		}

		[Test]
		public void TestAutoUpgradeVersion0 ()
		{
			var path = Path.Combine (dataDir, "smimev0.db");
			var tmp = Path.Combine (TempDir, "smimev0-tmp.db");

			Directory.CreateDirectory (TempDir);
			File.Copy (path, tmp, true);

			using (var dbase = new SqliteCertificateDatabase (tmp, "no.secret")) {
				// Verify that we can select the Root Certificate
				bool trustedAnchor = false;
				foreach (var record in dbase.Find (null, true, X509CertificateRecordFields.Certificate)) {
					var fingerprint = record.Certificate.GetFingerprint ();

					if (fingerprint == "943471ff1ca3fb2dd843f515df261756cad58673") {
						trustedAnchor = true;
						break;
					}
				}

				Assert.That (trustedAnchor, Is.True, "Did not find the MimeKit UnitTests trusted anchor");
			}
		}

		[Test]
		public void TestAutoUpgradeVersion1 ()
		{
			var path = Path.Combine (dataDir, "smimev1.db");
			var tmp = Path.Combine (TempDir, "smimev1-tmp.db");

			Directory.CreateDirectory (TempDir);
			File.Copy (path, tmp, true);

			using (var dbase = new SqliteCertificateDatabase (tmp, "no.secret")) {
				// Verify that we can select the Root Certificate
				bool trustedAnchor = false;
				foreach (var record in dbase.Find (null, true, X509CertificateRecordFields.Certificate)) {
					var fingerprint = record.Certificate.GetFingerprint ();

					if (fingerprint == "943471ff1ca3fb2dd843f515df261756cad58673") {
						trustedAnchor = true;
						break;
					}
				}

				Assert.That (trustedAnchor, Is.True, "Did not find the MimeKit UnitTests trusted anchor");
			}
		}

		[Test]
		public void TestEnumerateMatches ()
		{
			using (var dbase = new SqliteCertificateDatabase (DatabasePath, "no.secret")) {
				var certificates = ((IStore<X509Certificate>) dbase).EnumerateMatches (null).ToList ();

				Assert.That (certificates, Has.Count.EqualTo (6), "Did not find the expected # of certificate");
			}
		}

		static void AssertFindBy (ISelector<X509Certificate> selector, X509Certificate expected)
		{
			using (var dbase = new SqliteCertificateDatabase (DatabasePath, "no.secret")) {
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

		[Test]
		public void TestFindPrivateKeys ()
		{
			using (var dbase = new SqliteCertificateDatabase (DatabasePath, "no.secret")) {
				var privateKeys = dbase.FindPrivateKeys (null).ToList ();

				Assert.That (privateKeys, Has.Count.EqualTo (1), "Did not find the expected # of private keys");
			}
		}

		[Test]
		public void TestFindCrl ()
		{
			using (var dbase = new SqliteCertificateDatabase (DatabasePath, "no.secret")) {
				foreach (var crl in SecureMimeTestsBase.ObsoleteCrls) {
					var record = dbase.Find (crl, X509CrlRecordFields.Id | X509CrlRecordFields.IsDelta | X509CrlRecordFields.IssuerName | X509CrlRecordFields.ThisUpdate | X509CrlRecordFields.NextUpdate | X509CrlRecordFields.Crl);

					Assert.That (record, Is.Not.Null, $"Did not find the expected CRL for {crl.IssuerDN}");
					Assert.That (record.IsDelta, Is.EqualTo (crl.IsDelta ()), $"IsDelta for {crl.IssuerDN}");
					Assert.That (record.IssuerName, Is.EqualTo (crl.IssuerDN.ToString ()), $"IssuerName for {crl.IssuerDN}");
					Assert.That (record.ThisUpdate, Is.EqualTo (crl.ThisUpdate), $"ThisUpdate for {crl.IssuerDN}");
					Assert.That (record.NextUpdate, Is.EqualTo (crl.NextUpdate), $"NextUpdate for {crl.IssuerDN}");
				}
			}
		}
	}
}
