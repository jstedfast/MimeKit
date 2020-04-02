﻿//
// CmsSignerTests.cs
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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509KeyUsageFlags = MimeKit.Cryptography.X509KeyUsageFlags;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class CmsSignerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var signer = new CmsSigner (Path.Combine ("..", "..", "TestData", "smime", "smime.p12"), "no.secret");
			var certificate = new X509Certificate2 (signer.Certificate.GetEncoded ());
			var chain = new[] { DotNetUtilities.FromX509Certificate (certificate) };
			AsymmetricCipherKeyPair keyPair;

			using (var stream = new StreamReader (Path.Combine ("..", "..", "TestData", "dkim", "example.pem"))) {
				var reader = new PemReader (stream);

				keyPair = reader.ReadObject () as AsymmetricCipherKeyPair;
			}

			Assert.Throws<ArgumentException> (() => new CmsSigner (certificate));
			Assert.Throws<ArgumentException> (() => new CmsSigner (chain, keyPair.Public));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((IEnumerable<X509Certificate>) null, signer.PrivateKey));
			Assert.Throws<ArgumentException> (() => new CmsSigner (new X509Certificate[0], signer.PrivateKey));
			Assert.Throws<ArgumentException> (() => new CmsSigner (signer.CertificateChain, keyPair.Public));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (signer.CertificateChain, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((X509Certificate) null, signer.PrivateKey));
			Assert.Throws<ArgumentException> (() => new CmsSigner (signer.Certificate, keyPair.Public));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (signer.Certificate, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((X509Certificate2) null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((Stream) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (Stream.Null, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((string) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner ("fileName", null));
		}

		static void LoadPkcs12 (string path, string password, out List<X509Certificate> certificates, out AsymmetricKeyParameter privateKey)
		{
			certificates = null;
			privateKey = null;

			using (var stream = File.OpenRead (path)) {
				var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

				foreach (string alias in pkcs12.Aliases) {
					if (!pkcs12.IsKeyEntry (alias))
						continue;

					var chain = pkcs12.GetCertificateChain (alias);
					var key = pkcs12.GetKey (alias);

					if (!key.Key.IsPrivate || chain.Length == 0)
						continue;

					var flags = chain[0].Certificate.GetKeyUsageFlags ();

					if (flags != X509KeyUsageFlags.None && (flags & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
						continue;

					certificates = new List<X509Certificate> ();
					certificates.Add (chain[0].Certificate);
					privateKey = key.Key;

					foreach (var entry in chain)
						certificates.Add (entry.Certificate);

					return;
				}
			}
		}

		[Test]
		public void TestConstructors ()
		{
			var path = Path.Combine ("..", "..", "TestData", "smime", "smime.p12");
			List<X509Certificate> certificates;
			AsymmetricKeyParameter key;
			var password = "no.secret";
			CmsSigner signer;

			try {
				signer = new CmsSigner (path, password);
			} catch (Exception ex) {
				Assert.Fail (".ctor (string, string): {0}", ex.Message);
			}

			try {
				using (var stream = File.OpenRead (path))
					signer = new CmsSigner (stream, password);
			} catch (Exception ex) {
				Assert.Fail (".ctor (Stream, string): {0}", ex.Message);
			}

			LoadPkcs12 (path, password, out certificates, out key);

			try {
				signer = new CmsSigner (certificates, key);
			} catch (Exception ex) {
				Assert.Fail (".ctor (IEnumerable<X509Certificate>, AsymmetricKeyParameter): {0}", ex.Message);
			}

			try {
				signer = new CmsSigner (certificates[0], key);
			} catch (Exception ex) {
				Assert.Fail (".ctor (X509Certificate, AsymmetricKeyParameter): {0}", ex.Message);
			}

			try {
				signer = new CmsSigner (new X509Certificate2 (path, password, X509KeyStorageFlags.Exportable));
			} catch (Exception ex) {
				Assert.Fail (".ctor (X509Certificate2): {0}", ex);
			}
		}

		[Test]
		public void TestDefaultValues ()
		{
			var path = Path.Combine ("..", "..", "TestData", "smime", "smime.p12");
			List<X509Certificate> certificates;
			AsymmetricKeyParameter key;
			var password = "no.secret";
			CmsSigner signer;

			signer = new CmsSigner (path, password);
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, signer.SignerIdentifierType, "new CmsSigner (string, string)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			using (var stream = File.OpenRead (path))
				signer = new CmsSigner (stream, password);
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, signer.SignerIdentifierType, "new CmsSigner (Stream, string)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			LoadPkcs12 (path, password, out certificates, out key);

			signer = new CmsSigner (certificates, key);
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, signer.SignerIdentifierType, "new CmsSigner (chain, key)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			signer = new CmsSigner (certificates[0], key);
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, signer.SignerIdentifierType, "new CmsSigner (certificate, key)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			signer = new CmsSigner (new X509Certificate2 (path, password, X509KeyStorageFlags.Exportable));
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, signer.SignerIdentifierType, "new CmsSigner (X509Certificate2)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");
		}

		[Test]
		public void TestSignerIdentifierType ()
		{
			var path = Path.Combine ("..", "..", "TestData", "smime", "smime.p12");
			List<X509Certificate> certificates;
			AsymmetricKeyParameter key;
			var password = "no.secret";
			CmsSigner signer;

			signer = new CmsSigner (path, password, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, signer.SignerIdentifierType, "new CmsSigner (string, string)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			using (var stream = File.OpenRead (path))
				signer = new CmsSigner (stream, password, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, signer.SignerIdentifierType, "new CmsSigner (Stream, string)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			LoadPkcs12 (path, password, out certificates, out key);

			signer = new CmsSigner (certificates, key, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, signer.SignerIdentifierType, "new CmsSigner (chain, key)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			signer = new CmsSigner (certificates[0], key, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, signer.SignerIdentifierType, "new CmsSigner (certificate, key)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");

			signer = new CmsSigner (new X509Certificate2 (path, password, X509KeyStorageFlags.Exportable), SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, signer.SignerIdentifierType, "new CmsSigner (X509Certificate2)");
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "RsaSignaturePadding");
		}

		[Test]
		public void TestRsaSignaturePadding ()
		{
			var path = Path.Combine ("..", "..", "TestData", "smime", "smime.p12");
			var signer = new CmsSigner (path, "no.secret");

			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "Default RsaSignaturePaddingScheme");
			Assert.IsNull (signer.RsaSignaturePadding, "Default RsaSignaturePadding");

			Assert.Throws<ArgumentOutOfRangeException> (() => signer.RsaSignaturePaddingScheme = (RsaSignaturePaddingScheme) 555);

			signer.RsaSignaturePaddingScheme = RsaSignaturePaddingScheme.Pkcs1;
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme #1");
			Assert.AreEqual (RsaSignaturePadding.Pkcs1, signer.RsaSignaturePadding, "RsaSignaturePadding #1");

			signer.RsaSignaturePaddingScheme = RsaSignaturePaddingScheme.Pss;
			Assert.AreEqual (RsaSignaturePaddingScheme.Pss, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme #2");
			Assert.AreEqual (RsaSignaturePadding.Pss, signer.RsaSignaturePadding, "RsaSignaturePadding #2");

			signer.RsaSignaturePadding = RsaSignaturePadding.Pkcs1;
			Assert.AreEqual (RsaSignaturePaddingScheme.Pkcs1, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme #3");
			Assert.AreEqual (RsaSignaturePadding.Pkcs1, signer.RsaSignaturePadding, "RsaSignaturePadding #3");

			signer.RsaSignaturePadding = RsaSignaturePadding.Pss;
			Assert.AreEqual (RsaSignaturePaddingScheme.Pss, signer.RsaSignaturePaddingScheme, "RsaSignaturePaddingScheme #4");
			Assert.AreEqual (RsaSignaturePadding.Pss, signer.RsaSignaturePadding, "RsaSignaturePadding #4");
		}
	}
}
