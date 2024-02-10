//
// DefaultSecureMimeContextTests.cs
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

using System.Data.Common;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;

using MimeKit;
using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DefaultSecureMimeContextTests
	{
		static readonly string[] CertificateAuthorities = {
			"StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt"
		};

		[Test]
		public void TestArgumentExceptions ()
		{
			var connection = SqliteCertificateDatabase.CreateConnection ("smime.db");
			var database = new SqliteCertificateDatabase (connection, "no.secret");
			var random = new SecureRandom ();

			// password
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((string) null));

			// password, random
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((string) null, random));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ("password", (SecureRandom) null));

			// fileName, password
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext (null, "password"));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ("fileName", (string) null));

			// fileName, password, random
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext (null, "password", random));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ("fileName", null, random));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ("fileName", "password", null));

			// IX509CertificateDatabase [, random]
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((IX509CertificateDatabase) null));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((IX509CertificateDatabase) null, random));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext (database, null));

			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((string) null, "password"));
			Assert.Throws<ArgumentException> (() => new SqliteCertificateDatabase (string.Empty, "password"));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ("smime.db", null));

			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((DbConnection) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase (connection, null));

			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((string) null, "password", random));
			Assert.Throws<ArgumentException> (() => new SqliteCertificateDatabase (string.Empty, "password", random));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ("smime.db", null, random));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ("smime.db", "password", null));

			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((DbConnection) null, "password", random));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase (connection, null, random));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase (connection, "password", null));

			Assert.Throws<ArgumentNullException> (() => database.Add ((X509CrlRecord) null));
			Assert.Throws<ArgumentNullException> (() => database.Remove ((X509CrlRecord) null));
			Assert.Throws<ArgumentNullException> (() => database.Update ((X509CrlRecord) null));
			Assert.Throws<ArgumentNullException> (() => database.Add ((X509CertificateRecord) null));
			Assert.Throws<ArgumentNullException> (() => database.Remove ((X509CertificateRecord) null));
			Assert.Throws<ArgumentNullException> (() => database.Update ((X509CertificateRecord) null, X509CertificateRecordFields.Algorithms));
			Assert.Throws<ArgumentNullException> (() => database.Find ((X509Crl) null, X509CrlRecordFields.IsDelta));
			Assert.Throws<ArgumentNullException> (() => database.Find ((X509Name) null, X509CrlRecordFields.IsDelta).FirstOrDefault ());
			Assert.Throws<ArgumentNullException> (() => database.Find ((X509Certificate) null, X509CertificateRecordFields.Id));
			Assert.Throws<ArgumentNullException> (() => database.Find ((MailboxAddress) null, DateTime.Now, true, X509CertificateRecordFields.PrivateKey).FirstOrDefault ());

			using (var ctx = new DefaultSecureMimeContext (database)) {
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((Stream) null, true));
				Assert.Throws<ArgumentNullException> (() => ctx.Import ((X509Certificate) null, true));
			}
		}

		[Test]
		public void TestImportCertificates ()
		{
			try {
				var database = new SqliteCertificateDatabase ("smime.db", "no.secret");
				var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
				var certificates = new List<X509Certificate> ();

				using (var ctx = new DefaultSecureMimeContext (database)) {
					foreach (var filename in CertificateAuthorities) {
						var path = Path.Combine (dataDir, filename);

						using (var stream = File.OpenRead (path)) {
							var parser = new X509CertificateParser ();

							foreach (X509Certificate certificate in parser.ReadCertificates (stream)) {
								certificates.Add (certificate);
								ctx.Import (certificate);
							}
						}
					}

					// make sure each certificate is there and then delete them...
					foreach (var certificate in certificates) {
						var record = database.Find (certificate, X509CertificateRecordFields.Id);

						Assert.That (record, Is.Not.Null, "Find");

						database.Remove (record);
					}
				}
			} finally {
				if (File.Exists ("smime.db"))
					File.Delete ("smime.db");
			}
		}

		[Test]
		public void TestImportX509Certificate2 ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);

			try {
				using (var ctx = new DefaultSecureMimeContext ("smime.db", "no.secret")) {
					var certificate = new X509Certificate2 (rsa.FileName, "no.secret", X509KeyStorageFlags.Exportable);
					var secure = new SecureMailboxAddress ("MimeKit UnitTests", rsa.EmailAddress, certificate.Thumbprint);
					var mailbox = new MailboxAddress ("MimeKit UnitTests", rsa.EmailAddress);

					ctx.Import (certificate);

					// Check that the certificate exists in the context
					Assert.That (ctx.CanSign (mailbox), Is.True, "CanSign(MailboxAddress)");
					Assert.That (ctx.CanEncrypt (mailbox), Is.True, "CanEncrypt(MailboxAddress)");
					Assert.That (ctx.CanSign (secure), Is.True, "CanSign(SecureMailboxAddress)");
					Assert.That (ctx.CanEncrypt (secure), Is.True, "CanEncrypt(SecureMailboxAddress)");
				}
			} finally {
				if (File.Exists ("smime.db"))
					File.Delete ("smime.db");
			}
		}

		[Test]
		public async Task TestImportX509Certificate2Async ()
		{
			var rsa = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.PublicKeyAlgorithm == PublicKeyAlgorithm.RsaGeneral);

			try {
				using (var ctx = new DefaultSecureMimeContext ("smime.db", "no.secret")) {
					var certificate = new X509Certificate2 (rsa.FileName, "no.secret", X509KeyStorageFlags.Exportable);
					var secure = new SecureMailboxAddress ("MimeKit UnitTests", rsa.EmailAddress, certificate.Thumbprint);
					var mailbox = new MailboxAddress ("MimeKit UnitTests", rsa.EmailAddress);

					await ctx.ImportAsync (certificate);

					// Check that the certificate exists in the context
					Assert.That (await ctx.CanSignAsync (mailbox), Is.True, "CanSign(MailboxAddress)");
					Assert.That (await ctx.CanEncryptAsync (mailbox), Is.True, "CanEncrypt(MailboxAddress)");
					Assert.That (await ctx.CanSignAsync (secure), Is.True, "CanSign(SecureMailboxAddress)");
					Assert.That (await ctx.CanEncryptAsync (secure), Is.True, "CanEncrypt(SecureMailboxAddress)");
				}
			} finally {
				if (File.Exists ("smime.db"))
					File.Delete ("smime.db");
			}
		}
	}
}
