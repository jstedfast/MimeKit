//
// X509CertificateChainTests.cs
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
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.X509;

using NUnit.Framework;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class X509CertificateChainTests
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
			var path = GetTestDataPath ("smime.p12");
			var chain = new X509CertificateChain ();
			CmsSigner signer;

			using (var stream = File.OpenRead (path))
				signer = new CmsSigner (stream, "no.secret");

			Assert.Throws<ArgumentNullException> (() => new X509CertificateChain (null));
			Assert.Throws<ArgumentNullException> (() => chain.Add (null));
			Assert.Throws<ArgumentNullException> (() => chain.AddRange (null));
			Assert.Throws<ArgumentNullException> (() => chain.Contains (null));
			Assert.Throws<ArgumentNullException> (() => chain.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => chain.CopyTo (new X509Certificate[0], -1));
			Assert.Throws<ArgumentNullException> (() => chain.IndexOf (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => chain.Insert (-1, signer.Certificate));
			Assert.Throws<ArgumentNullException> (() => chain.Insert (0, null));
			Assert.Throws<ArgumentNullException> (() => chain[0] = null);
			Assert.Throws<ArgumentNullException> (() => chain.Remove (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => chain.RemoveAt (-1));
		}

		[Test]
		public void TestAddRemoveRange ()
		{
			var certificates = new List<X509Certificate> ();
			var chain = new X509CertificateChain ();

			foreach (var authority in CertificateAuthorities) {
				var certificate = SecureMimeTestsBase.LoadCertificate (GetTestDataPath (authority));

				certificates.Add (certificate);
			}

			Assert.Throws<ArgumentNullException> (() => chain.AddRange (null));

			chain.AddRange (certificates);

			Assert.AreEqual (CertificateAuthorities.Length, chain.Count, "Unexpected number of certificates after AddRange.");

			int index = 0;
			foreach (var certificate in chain)
				Assert.AreEqual (certificates[index++], certificate, "GetEnumerator");

			index = 0;
			foreach (X509Certificate certificate in ((IEnumerable) chain))
				Assert.AreEqual (certificates[index++], certificate, "GetEnumerator");

			Assert.Throws<ArgumentNullException> (() => chain.RemoveRange (null));

			chain.RemoveRange (certificates);

			Assert.AreEqual (0, chain.Count, "Unexpected number of certificates after RemoveRange.");
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var certs = SecureMimeTestsBase.LoadPkcs12CertificateChain (GetTestDataPath ("smime.pfx"), "no.secret");
			var chain = new X509CertificateChain ();

			Assert.IsFalse (chain.IsReadOnly);
			Assert.AreEqual (0, chain.Count, "Initial count");

			chain.Add (certs[2]);

			Assert.AreEqual (1, chain.Count);
			Assert.AreEqual (certs[2], chain[0]);

			chain.Insert (0, certs[0]);
			chain.Insert (1, certs[1]);

			Assert.AreEqual (3, chain.Count);
			Assert.AreEqual (certs[0], chain[0]);
			Assert.AreEqual (certs[1], chain[1]);
			Assert.AreEqual (certs[2], chain[2]);

			Assert.IsTrue (chain.Contains (certs[1]), "Contains");
			Assert.AreEqual (1, chain.IndexOf (certs[1]), "IndexOf");

			var array = new X509Certificate[chain.Count];
			chain.CopyTo (array, 0);
			chain.Clear ();

			Assert.AreEqual (0, chain.Count);

			foreach (var cert in array)
				chain.Add (cert);

			Assert.AreEqual (array.Length, chain.Count);

			Assert.IsTrue (chain.Remove (certs[2]));
			Assert.AreEqual (2, chain.Count);
			Assert.AreEqual (certs[0], chain[0]);
			Assert.AreEqual (certs[1], chain[1]);

			chain.RemoveAt (0);

			Assert.AreEqual (1, chain.Count);
			Assert.AreEqual (certs[1], chain[0]);

			chain[0] = certs[2];

			Assert.AreEqual (1, chain.Count);
			Assert.AreEqual (certs[2], chain[0]);
		}
	}
}
