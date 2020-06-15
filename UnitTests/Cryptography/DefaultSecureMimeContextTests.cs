//
// DefaultSecureMimeContextTests.cs
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
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;

using NUnit.Framework;

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

		static DefaultSecureMimeContextTests ()
		{
			if (File.Exists ("smime.db"))
				File.Delete ("smime.db");
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((string) null));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ((IX509CertificateDatabase) null));

			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext (null, "password"));
			Assert.Throws<ArgumentNullException> (() => new DefaultSecureMimeContext ("fileName", null));

			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((DbConnection) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ((string) null, "password"));
			Assert.Throws<ArgumentException> (() => new SqliteCertificateDatabase (string.Empty, "password"));
			Assert.Throws<ArgumentNullException> (() => new SqliteCertificateDatabase ("smime.db", null));

			var database = new SqliteCertificateDatabase ("smime.db", "no.secret");

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
			}
		}

		[Test]
		public void TestImportCertificates ()
		{
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

					Assert.IsNotNull (record, "Find");

					database.Remove (record);
				}
			}
		}
	}
}
