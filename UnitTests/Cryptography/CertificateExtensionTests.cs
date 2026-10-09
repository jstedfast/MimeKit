//
// BouncyCastleCertificateExtensionTests.cs
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

using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;
using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509KeyUsageFlags = MimeKit.Cryptography.X509KeyUsageFlags;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class CertificateExtensionTests
	{
		static X509Certificate GenerateCertificate (bool invalidSmimeCapabilities)
		{
			using var randomGenerator = new CryptoApiRandomGenerator ();
			var random = new SecureRandom (randomGenerator);
			var keyGenerator = new RsaKeyPairGenerator ();
			keyGenerator.Init (new KeyGenerationParameters (random, 2048));
			var keyPair = keyGenerator.GenerateKeyPair ();
			var generator = new X509V3CertificateGenerator ();
			var oids = new[] { X509Name.CN, X509Name.Name, X509Name.EmailAddress };
			var values = new[] { "Extension Test", "Display Name", "User@Example.COM" };
			var subject = new X509Name (oids, values);
			var altNames = new GeneralNames (new[] {
				new GeneralName (GeneralName.DnsName, MailboxAddress.IdnMapping.Encode ("bücher.example")),
				new GeneralName (GeneralName.Rfc822Name, "alt@example.com"),
				new GeneralName (GeneralName.DnsName, "www.example.com")
			});

			generator.SetSerialNumber (BigInteger.One);
			generator.SetIssuerDN (subject);
			generator.SetSubjectDN (subject);
			generator.SetNotBefore (DateTime.UtcNow.AddDays (-1));
			generator.SetNotAfter (DateTime.UtcNow.AddDays (1));
			generator.SetPublicKey (keyPair.Public);
			generator.AddExtension (X509Extensions.SubjectAlternativeName, false, altNames);

			if (invalidSmimeCapabilities)
				generator.AddExtension (SmimeAttributes.SmimeCapabilities, false, DerInteger.ValueOf (5));

			return generator.Generate (new Asn1SignatureFactory ("SHA256WithRSA", keyPair.Private, random));
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.AsX509Certificate2 (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetIssuerNameInfo (null, X509Name.CN));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectNameInfo (null, X509Name.CN));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetCommonName (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectName (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectEmailAddress (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetSubjectDnsNames (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetFingerprint (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetKeyUsageFlags ((X509Certificate) null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetEncryptionAlgorithms (null));
			Assert.Throws<ArgumentNullException> (() => BouncyCastleCertificateExtensions.GetPublicKeyAlgorithm (null));

			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetPrivateKeyAsAsymmetricKeyParameter (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.AsBouncyCastleCertificate (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetEncryptionAlgorithms (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetPublicKeyAlgorithm (null));
			Assert.Throws<ArgumentNullException> (() => X509Certificate2Extensions.GetSubjectDnsNames (null));

			var certificate = GenerateCertificate (false).AsX509Certificate2 ();
			certificate.Dispose ();

			var exception = Assert.Throws<ArgumentException> (() => certificate.AsBouncyCastleCertificate ());
			Assert.That (exception.ParamName, Is.EqualTo ("certificate"));
		}

		static X509KeyUsageFlags GetX509Certificate2KeyUsageFlags (X509Certificate2 certificate)
		{
			if (certificate.Extensions[X509Extensions.KeyUsage.Id] is X509KeyUsageExtension usage)
				return (X509KeyUsageFlags) usage.KeyUsages;

			return BouncyCastleCertificateExtensions.GetKeyUsageFlags ((bool[]) null);
		}

		[Test]
		public void TestCertificateConversion ()
		{
			var fileNames = new string[] { "StartComCertificationAuthority.crt", "StartComClass1PrimaryIntermediateClientCA.crt" };
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var parser = new X509CertificateParser ();

			foreach (var fileName in fileNames) {
				using (var stream = File.OpenRead (Path.Combine (dataDir, fileName))) {
					foreach (X509Certificate certificate in parser.ReadCertificates (stream)) {
						var certificate2 = certificate.AsX509Certificate2 ();
						var certificate1 = certificate2.AsBouncyCastleCertificate ();

						Assert.That (certificate1.GetFingerprint ().ToUpperInvariant (), Is.EqualTo (certificate2.Thumbprint), "Fingerprint");
						Assert.That (certificate1.GetIssuerNameInfo (X509Name.EmailAddress), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, true)), "Issuer Email");
						Assert.That (certificate1.GetSubjectEmailAddress (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, false)), "Subject Email");
						Assert.That (certificate1.GetCommonName (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.SimpleName, false)), "Common Name");

						var usage2 = GetX509Certificate2KeyUsageFlags (certificate2);
						var usage1 = certificate1.GetKeyUsageFlags ();

						Assert.That (usage1, Is.EqualTo (usage2), "KeyUsageFlags");
					}
				}
			}
		}

		[Test]
		public void TestGetSubjectDnsNames ()
		{
			var certificate = SecureMimeTestsBase.SupportedCertificates.FirstOrDefault (c => c.DnsNames.Length > 0);
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "dnsnames", "smime.pfx");
			var parser = new X509CertificateParser ();

			using (var stream = File.OpenRead (path)) {
				var certificate2 = certificate.Certificate.AsX509Certificate2 ();
				var certificate1 = certificate2.AsBouncyCastleCertificate ();

				Assert.That (certificate1.GetFingerprint ().ToUpperInvariant (), Is.EqualTo (certificate2.Thumbprint), "Fingerprint");
				Assert.That (certificate1.GetIssuerNameInfo (X509Name.EmailAddress), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, true)), "Issuer Email");
				Assert.That (certificate1.GetSubjectEmailAddress (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.EmailName, false)), "Subject Email");
				Assert.That (certificate1.GetCommonName (), Is.EqualTo (certificate2.GetNameInfo (X509NameType.SimpleName, false)), "Common Name");

				var usage2 = GetX509Certificate2KeyUsageFlags (certificate2);
				var usage1 = certificate1.GetKeyUsageFlags ();

				Assert.That (usage1, Is.EqualTo (usage2), "KeyUsageFlags");

				var dnsNames = certificate1.GetSubjectDnsNames ();
				var expectedDnsNames = certificate.DnsNames;

				Assert.That (dnsNames.Length, Is.EqualTo (expectedDnsNames.Length), "SubjectDnsNames.Length");
				for (int i = 0; i < dnsNames.Length; i++)
					Assert.That (dnsNames[i], Is.EqualTo (expectedDnsNames[i]), $"SubjectDnsNames[{i}]");

				dnsNames = certificate2.GetSubjectDnsNames ();

				Assert.That (dnsNames.Length, Is.EqualTo (expectedDnsNames.Length), "SubjectDnsNames.Length #2");
				for (int i = 0; i < dnsNames.Length; i++)
					Assert.That (dnsNames[i], Is.EqualTo (expectedDnsNames[i]), $"SubjectDnsNames[{i}] #2");
			}
		}

		[Test]
		public void TestGeneratedCertificateExtensions ()
		{
			var certificate = GenerateCertificate (false);
			using var certificate2 = certificate.AsX509Certificate2 ();

			Assert.That (certificate.GetIssuerNameInfo (X509Name.CN), Is.EqualTo ("Extension Test"));
			Assert.That (certificate.GetSubjectName (), Is.EqualTo ("Display Name"));
			Assert.That (certificate.GetSubjectEmailAddress (true), Is.EqualTo ("User@Example.COM"));

			var dnsNames = certificate.GetSubjectDnsNames ();
			Assert.That (dnsNames, Is.EqualTo (new[] { "bücher.example", "www.example.com" }));

			dnsNames = certificate.GetSubjectDnsNames (true);
			Assert.That (dnsNames, Is.EqualTo (new[] { "xn--bcher-kva.example", "www.example.com" }));

			dnsNames = certificate2.GetSubjectDnsNames ();
			Assert.That (dnsNames, Is.EqualTo (new[] { "bücher.example", "www.example.com" }));

			dnsNames = certificate2.GetSubjectDnsNames (true);
			Assert.That (dnsNames, Is.EqualTo (new[] { "xn--bcher-kva.example", "www.example.com" }));
		}

		[Test]
		public void TestInvalidSmimeCapabilitiesFallBackToTripleDes ()
		{
			var certificate = GenerateCertificate (true);
			using var certificate2 = certificate.AsX509Certificate2 ();

			Assert.That (certificate.GetEncryptionAlgorithms (), Is.EqualTo (new[] { EncryptionAlgorithm.TripleDes }));
			Assert.That (certificate2.GetEncryptionAlgorithms (), Is.EqualTo (new[] { EncryptionAlgorithm.TripleDes }));
		}
	}
}
