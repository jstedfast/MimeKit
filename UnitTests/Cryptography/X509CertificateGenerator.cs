//
// X509CertificateGenerator.cs
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

using System.Text;
using System.Globalization;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

using MimeKit;

namespace UnitTests.Cryptography {
	class X509CertificateGenerator
	{
		static readonly Dictionary<string, DerObjectIdentifier> SMimeCapabilityMapping;
		static readonly Dictionary<string, DerObjectIdentifier> X509NameOidMapping;
		static readonly Dictionary<string, int> X509SubjectAlternativeTagMapping;
		static readonly char[] EqualSign = new char[] { '=' };

		static X509CertificateGenerator ()
		{
			X509NameOidMapping = new Dictionary<string, DerObjectIdentifier> (StringComparer.OrdinalIgnoreCase) {
				{ "BusinessCategory", X509Name.BusinessCategory },
				{ "C", X509Name.C },
				{ "CN", X509Name.CN },
				{ "CommonName", X509Name.CN },
				{ "CountryCode", X509Name.C },
				{ "CountryName", X509Name.C },
				{ "CountryOfCitizenship", X509Name.CountryOfCitizenship },
				{ "CountryOfResidence", X509Name.CountryOfResidence },
				{ "DateOfBirth", X509Name.DateOfBirth },
				{ "DC", X509Name.DC },
				{ "DN", X509Name.DnQualifier },
				{ "DnQualifier", X509Name.DnQualifier },
				{ "E", X509Name.E },
				{ "EmailAddress", X509Name.EmailAddress },
				{ "Gender", X509Name.Gender },
				{ "Generation", X509Name.Generation },
				{ "GivenName", X509Name.GivenName },
				{ "Initials", X509Name.Initials },
				{ "L", X509Name.L },
				{ "LocalityName", X509Name.L },
				{ "NameAtBirth", X509Name.NameAtBirth },
				{ "O", X509Name.O },
				{ "OrganizationName", X509Name.O },
				{ "OrganizationalUnitName", X509Name.OU },
				{ "OU", X509Name.OU },
				{ "PlaceOfBirth", X509Name.PlaceOfBirth },
				{ "PostalAddress", X509Name.PostalAddress },
				{ "PostalCode", X509Name.PostalCode },
				{ "Pseudonym", X509Name.Pseudonym },
				{ "SerialNumber", X509Name.SerialNumber },
				{ "ST", X509Name.ST },
				{ "StateOrProvinceName", X509Name.ST },
				{ "Street", X509Name.Street },
				{ "Surname", X509Name.Surname },
				{ "T", X509Name.T },
				{ "TelephoneNumber", X509Name.TelephoneNumber },
				{ "Title", X509Name.T },
				{ "UID", X509Name.UID },
				{ "UnstructuredAddress", X509Name.UnstructuredAddress },
				{ "UnstructuredName", X509Name.UnstructuredName },
				{ "UniqueIdentifier", X509Name.UniqueIdentifier },
			};

			X509SubjectAlternativeTagMapping = new Dictionary<string, int> (StringComparer.OrdinalIgnoreCase) {
				{ "Rfc822Name", GeneralName.Rfc822Name },
				{ "DnsName", GeneralName.DnsName },
			};

			SMimeCapabilityMapping = new Dictionary<string, DerObjectIdentifier> (StringComparer.OrdinalIgnoreCase) {
				{ "AES128", NistObjectIdentifiers.IdAes128Cbc },
				{ "AES192", NistObjectIdentifiers.IdAes192Cbc },
				{ "AES256", NistObjectIdentifiers.IdAes256Cbc },
				{ "CAMELLIA128", NttObjectIdentifiers.IdCamellia128Cbc },
				{ "CAMELLIA192", NttObjectIdentifiers.IdCamellia192Cbc },
				{ "CAMELLIA256", NttObjectIdentifiers.IdCamellia256Cbc },
				{ "CAST5", MiscObjectIdentifiers.cast5CBC },
				{ "DES", OiwObjectIdentifiers.DesCbc },
				{ "3DES", PkcsObjectIdentifiers.DesEde3Cbc },
				{ "IDEA", MiscObjectIdentifiers.as_sys_sec_alg_ideaCBC },
				{ "RC2", PkcsObjectIdentifiers.RC2Cbc },
				{ "SEED", KisaObjectIdentifiers.IdSeedCbc }
			};
		}

		static AsymmetricCipherKeyPair LoadAsymmetricCipherKeyPair (string fileName)
		{
			using (var stream = File.OpenRead (fileName)) {
				using (var reader = new StreamReader (stream)) {
					var pem = new PemReader (reader);
					var item = pem.ReadObject ();

					if (item is AsymmetricCipherKeyPair keyPair)
						return keyPair;

					if (item is AsymmetricKeyParameter key && key.IsPrivate) {
						if (key is DsaPrivateKeyParameters dsa) {
							var y = dsa.Parameters.G.ModPow (dsa.X, dsa.Parameters.P);
							var pub = new DsaPublicKeyParameters (y, dsa.Parameters);

							return new AsymmetricCipherKeyPair (pub, key);
						} else if (key is RsaPrivateCrtKeyParameters rsa) {
							var pub = new RsaKeyParameters (false, rsa.Modulus, rsa.Exponent);

							return new AsymmetricCipherKeyPair (pub, key);
						} else if (key is ECPrivateKeyParameters ec) {
							var q = ec.Parameters.G.Multiply (ec.D);
							var pub = new ECPublicKeyParameters (ec.AlgorithmName, q, ec.Parameters);

							return new AsymmetricCipherKeyPair (pub, key);
						}
					}

					throw new Exception ("Invalid asymmetric key pair.");
				}
			}
		}

		static X509Certificate[] LoadPkcs12CertificateChain (string fileName, string password, out AsymmetricKeyParameter key)
		{
			using (var stream = File.OpenRead (fileName)) {
				var pkcs12 = new Pkcs12StoreBuilder ().Build ();

				pkcs12.Load (stream, password.ToCharArray ());

				foreach (string alias in pkcs12.Aliases) {
					if (pkcs12.IsKeyEntry (alias)) {
						var chain = pkcs12.GetCertificateChain (alias);
						var entry = pkcs12.GetKey (alias);

						if (!entry.Key.IsPrivate)
							continue;

						key = entry.Key;

						var certificates = new X509Certificate[chain.Length];
						for (int i = 0; i < chain.Length; i++)
							certificates[i] = chain[i].Certificate;

						return certificates;
					}
				}
			}

			throw new Exception ("Failed to locate private key entry.");
		}

		static string GetFingerprint (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var encoded = certificate.GetEncoded ();
			var fingerprint = new StringBuilder ();
			var sha1 = new Sha1Digest ();
			var data = new byte[20];

			sha1.BlockUpdate (encoded, 0, encoded.Length);
			sha1.DoFinal (data, 0);

			for (int i = 0; i < data.Length; i++)
				fingerprint.Append (data[i].ToString ("x2"));

			return fingerprint.ToString ();
		}

		public sealed class PrivateKeyOptions
		{
			public PrivateKeyOptions ()
			{
				Algorithm = "RSA";
				BitLength = 2048;
			}

			public string Algorithm { get; set; }

			public int BitLength { get; set; }

			public string CurveName { get; set; }

			public string FileName { get; set; }
		}

		public sealed class CertificateOptions
		{
			public CertificateOptions ()
			{
				Oids = new List<DerObjectIdentifier> ();
				Values = new List<string> ();
			}

			public string Alias { get; set; }

			internal IList<DerObjectIdentifier> Oids { get; }

			internal IList<string> Values { get; }

			internal string RevocationUrl { get; set; }

			internal List<GeneralName> SubjectAlternativeNames { get; private set; }

			internal SmimeCapabilityVector SMimeCapabilities { get; private set; }

			public void Add (DerObjectIdentifier oid, string value)
			{
				if (oid == X509Name.CN || oid == X509Name.E)
					Alias ??= value;

				Values.Add (value);
				Oids.Add (oid);
			}

			public void Add (string property, string value)
			{
				if (!X509NameOidMapping.TryGetValue (property, out DerObjectIdentifier oid))
					throw new ArgumentException ($"Unknown property: {property}", nameof (property));

				Add (oid, value);
			}

			public void AddSubjectAlternativeName (string property, string value)
			{
				if (!X509SubjectAlternativeTagMapping.TryGetValue (property, out int tagNo))
					throw new ArgumentException ($"Unknown property: {property}", nameof (property));

				SubjectAlternativeNames ??= new List<GeneralName> ();
				switch (tagNo) {
				case GeneralName.Rfc822Name:
					SubjectAlternativeNames.Add (new GeneralName (tagNo, MailboxAddress.EncodeAddrspec (value)));
					break;
				case GeneralName.DnsName:
					SubjectAlternativeNames.Add (new GeneralName (tagNo, MailboxAddress.IdnMapping.Encode (value)));
					break;
				default:
					SubjectAlternativeNames.Add (new GeneralName (tagNo, value));
					break;
				}
			}

			public void AddSMimeCapability (string capability)
			{
				var components = capability.Split (new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
				DerObjectIdentifier oid;

				if (SMimeCapabilityMapping.TryGetValue (components[0], out oid) || DerObjectIdentifier.TryFromID (components[0], out oid)) {
					SMimeCapabilities ??= new SmimeCapabilityVector ();

					if (components.Length > 1) {
						int value = int.Parse (components[1], NumberStyles.None, CultureInfo.InvariantCulture);

						SMimeCapabilities.AddCapability (oid, value);
					} else {
						SMimeCapabilities.AddCapability (oid);
					}
				} else {
					throw new ArgumentException ($"Unknown S/MIME capability: {capability}", nameof (capability));
				}
			}
		}

		public sealed class GeneratorOptions
		{
			public GeneratorOptions ()
			{
				IssuerPassword = string.Empty;
				Password = string.Empty;
			}

			public string BasicConstraints { get; set; }

			public int DaysValid { get; set; }

			public string Issuer { get; set; }

			public string IssuerPassword { get; set; }

			public string KeyUsage { get; set; }

			public string Output { get; set; }

			public string Password { get; set; }

			public string SignatureAlgorithm { get; set; }

			public bool IncludeChain { get; set; } = true;
		}

		public static X509Certificate[] Generate (GeneratorOptions options, PrivateKeyOptions privateKeyOptions, CertificateOptions certificateOptions, out AsymmetricKeyParameter privateKey)
		{
			// Sanity Checks
			if (!string.IsNullOrEmpty (privateKeyOptions.FileName) && !File.Exists (privateKeyOptions.FileName))
				throw new FormatException ($"[PrivateKey] FileName `{privateKeyOptions.FileName}' does not exist!");

			if (certificateOptions.Oids.Count == 0)
				throw new FormatException ("No [Subject] specified.");

			if (string.IsNullOrEmpty (options.Issuer))
				throw new FormatException ($"[Generator] Issuer property cannot be empty!");

			if (options.Issuer != "this" && !File.Exists (options.Issuer))
				throw new FormatException ($"[Generator] Issuer `{options.Issuer}' does not exist!");

			if (string.IsNullOrEmpty (options.Output))
				throw new FormatException ($"[Generator] Output property cannot be empty!");

			var subject = new X509Name (certificateOptions.Oids, certificateOptions.Values);
			var randomGenerator = new CryptoApiRandomGenerator ();
			var random = new SecureRandom (randomGenerator);
			AsymmetricCipherKeyPair key;

			if (string.IsNullOrEmpty (privateKeyOptions.FileName)) {
				IAsymmetricCipherKeyPairGenerator keyPairGenerator;

				switch (privateKeyOptions.Algorithm.ToLowerInvariant ()) {
				case "dsa":
					var dsaParameterGenerator = new DsaParametersGenerator ();
					dsaParameterGenerator.Init (privateKeyOptions.BitLength, 80, random);
					var dsaParameters = dsaParameterGenerator.GenerateParameters ();
					var dsaKeyGenParameters = new DsaKeyGenerationParameters (random, dsaParameters);
					keyPairGenerator = new DsaKeyPairGenerator ();
					keyPairGenerator.Init (dsaKeyGenParameters);
					break;
				case "rsa":
					var rsaKeyGenParameters = new KeyGenerationParameters (random, privateKeyOptions.BitLength);
					keyPairGenerator = new RsaKeyPairGenerator ();
					keyPairGenerator.Init (rsaKeyGenParameters);
					break;
				case "ec":
					var eccDomainParameters = new ECDomainParameters (ECNamedCurveTable.GetByName (privateKeyOptions.CurveName));
					var eccKeyGenParameters = new ECKeyGenerationParameters (eccDomainParameters, random);
					keyPairGenerator = new ECKeyPairGenerator ();
					keyPairGenerator.Init (eccKeyGenParameters);
					break;
				default:
					throw new ArgumentException ($"Unsupported PrivateKey algorithm: {privateKeyOptions.Algorithm}");
				}

				key = keyPairGenerator.GenerateKeyPair ();
			} else {
				try {
					key = LoadAsymmetricCipherKeyPair (privateKeyOptions.FileName);
				} catch (Exception ex) {
					throw new FormatException ($"[PrivateKey] Failed to load `{privateKeyOptions.FileName}': {ex.Message}", ex);
				}
			}

			AsymmetricKeyParameter signingKey;
			X509Certificate issuerCertificate;
			X509Certificate[] chain;
			X509Name issuer;

			if (options.Issuer != "this") {
				try {
					chain = LoadPkcs12CertificateChain (options.Issuer, options.IssuerPassword, out signingKey);
					issuerCertificate = chain[0];
					issuer = chain[0].SubjectDN;
				} catch (Exception ex) {
					throw new FormatException ("[Generator] failed to load `{options.Issuer}': {ex.Message}", ex);
				}
			} else {
				chain = Array.Empty<X509Certificate> ();
				issuerCertificate = null;
				signingKey = key.Private;
				issuer = subject;
			}

			string signatureAlgorithm;

			if (string.IsNullOrEmpty (options.SignatureAlgorithm)) {
				if (signingKey is RsaPrivateCrtKeyParameters) {
					signatureAlgorithm = "SHA256WithRSA";
				} else if (signingKey is ECPrivateKeyParameters ec) {
					if (ec.AlgorithmName == "ECGOST3410") {
						signatureAlgorithm = "GOST3411WithECGOST3410";
					} else {
						signatureAlgorithm = "SHA256withECDSA";
					}
				} else {
					signatureAlgorithm = "GOST3411WithGOST3410";
				}
			} else {
				signatureAlgorithm = options.SignatureAlgorithm;
			}

			int serialNumberIndex = certificateOptions.Oids.IndexOf (X509Name.SerialNumber);
			BigInteger serialNumber;

			if (serialNumberIndex == -1) {
				serialNumber = BigIntegers.CreateRandomInRange (BigInteger.One, BigInteger.ValueOf (long.MaxValue), random);
			} else {
				try {
					serialNumber = new BigInteger (certificateOptions.Values[serialNumberIndex]);
				} catch {
					throw new FormatException ($"Invalid [Subject] SerialNumber: {certificateOptions.Values[serialNumberIndex]}");
				}
			}

			var notBefore = DateTime.UtcNow;
			var notAfter = notBefore.AddDays (options.DaysValid);

			var signatureFactory = new Asn1SignatureFactory (signatureAlgorithm, signingKey, random);
			var generator = new X509V3CertificateGenerator ();
			generator.SetSerialNumber (serialNumber);
			generator.SetPublicKey (key.Public);
			generator.SetNotBefore (notBefore);
			generator.SetNotAfter (notAfter);
			generator.SetSubjectDN (subject);
			generator.SetIssuerDN (issuer);

			generator.AddExtension (X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure (key.Public));

			if (certificateOptions.SubjectAlternativeNames != null) {
				var altNames = new GeneralNames (certificateOptions.SubjectAlternativeNames.ToArray ());
				generator.AddExtension (X509Extensions.SubjectAlternativeName, false, altNames);
			}

			if (certificateOptions.SMimeCapabilities != null) {
				var capabilities = certificateOptions.SMimeCapabilities.ToAsn1EncodableVector ();
				generator.AddExtension (SmimeAttributes.SmimeCapabilities, false, new DerSequence (capabilities));
			}

			if (certificateOptions.RevocationUrl != null) {
				var fullName = new GeneralNames (new GeneralName (GeneralName.UniformResourceIdentifier, certificateOptions.RevocationUrl));
				var crlDistPoint = new CrlDistPoint (new [] {
					new DistributionPoint (new DistributionPointName (DistributionPointName.FullName, fullName), null, null)
				});

				generator.AddExtension (X509Extensions.CrlDistributionPoints, false, crlDistPoint);
			}

			if (issuerCertificate != null)
				generator.AddExtension (X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure (issuerCertificate));

			if (!string.IsNullOrEmpty (options.BasicConstraints)) {
				var basicConstraints = options.BasicConstraints.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				bool critical = false;
				bool ca = false;

				foreach (var constraint in basicConstraints) {
					switch (constraint.Trim ().ToLowerInvariant ()) {
					case "critical": critical = true; break;
					case "ca:false": ca = false; break;
					case "ca:true": ca = true; break;
					}
				}

				generator.AddExtension (X509Extensions.BasicConstraints, critical, new BasicConstraints (ca));
			}

			if (!string.IsNullOrEmpty (options.KeyUsage)) {
				var keyUsages = options.KeyUsage.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				bool critical = false;
				int keyUsage = 0;

				foreach (var usage in keyUsages) {
					switch (usage.Trim ().ToLowerInvariant ()) {
					case "critical": critical = true; break;
					case "digitalsignature": keyUsage |= X509KeyUsage.DigitalSignature; break;
					case "nonrepudiation": keyUsage |= X509KeyUsage.NonRepudiation; break;
					case "keyencipherment": keyUsage |= X509KeyUsage.KeyEncipherment; break;
					case "dataencipherment": keyUsage |= X509KeyUsage.DataEncipherment; break;
					case "keyagreement": keyUsage |= X509KeyUsage.KeyAgreement; break;
					case "keycertsign": keyUsage |= X509KeyUsage.KeyCertSign; break;
					case "crlsign": keyUsage |= X509KeyUsage.CrlSign; break;
					case "encipheronly": keyUsage |= X509KeyUsage.EncipherOnly; break;
					case "decipheronly": keyUsage |= X509KeyUsage.DecipherOnly; break;
					}
				}

				generator.AddExtension (X509Extensions.KeyUsage, critical, new KeyUsage (keyUsage));
			}

			privateKey = key.Private;

			var certificate = generator.Generate (signatureFactory);
			var keyEntry = new AsymmetricKeyEntry (privateKey);

			var chainLength = options.IncludeChain ? chain.Length + 1 : 1;
			var chainEntries = new X509CertificateEntry[chainLength];
			var certificates = new X509Certificate[chain.Length + 1];

			chainEntries[0] = new X509CertificateEntry (certificate);
			certificates[0] = certificate;

			
			for (int i = 0; i < chain.Length; i++) {
				if(options.IncludeChain)
					chainEntries[i + 1] = new X509CertificateEntry (chain[i]);
				certificates[i + 1] = chain[i];
			}

			var pkcs12 = new Pkcs12StoreBuilder ().Build ();
			pkcs12.SetKeyEntry (certificateOptions.Alias ?? string.Empty, keyEntry, chainEntries);

			using (var stream = File.Create (options.Output))
				pkcs12.Save (stream, options.Password.ToCharArray (), random);

			return certificates;
		}

		static string GetFileName (string baseDirectory, string value)
		{
			if (string.IsNullOrEmpty (value))
				return value;

			if (Path.IsPathRooted (value))
				return value;

			return Path.Combine (baseDirectory, value);
		}

		public static X509Certificate[] Generate (string cfg, out AsymmetricKeyParameter privateKey)
		{
			var baseDirectory = Path.GetDirectoryName (cfg);
			var certificate = new CertificateOptions ();
			var privateKeyOptions = new PrivateKeyOptions ();
			var options = new GeneratorOptions ();
			string section = null;

			// Default the output filename to the same as the input filename, but with a .pfx extension.
			options.Output = Path.ChangeExtension (cfg, ".pfx");

			using (var reader = File.OpenText (cfg)) {
				string line;

				while ((line = reader.ReadLine ()) != null) {
					if (line.Length == 0 || line[0] == '#')
						continue;

					if (line[0] == '[') {
						int endIndex = line.IndexOf (']');

						if (endIndex == -1)
							throw new FormatException ($"Incomplete section: {line}");

						section = line.Substring (1, endIndex - 1);
						continue;
					}

					var kvp = line.Split (EqualSign, 2);
					var property = kvp[0].Trim ();
					var value = kvp[1].Trim ();

					switch (section.ToLowerInvariant ()) {
					case "privatekey":
						switch (property.ToLowerInvariant ()) {
						case "algorithm":
							privateKeyOptions.Algorithm = value;
							break;
						case "bitlength":
							if (int.TryParse (value, out int bitLength)) {
								privateKeyOptions.BitLength = bitLength;
							} else {
								throw new FormatException ($"Invalid [PrivateKey] BitLength: {value}");
							}
							break;
						case "curvename":
							privateKeyOptions.CurveName = value;
							break;
						case "filename":
							privateKeyOptions.FileName = GetFileName (baseDirectory, value);
							break;
						default:
							throw new FormatException ($"Unknown [PrivateKey] property: {property}");
						}
						break;
					case "revocation":
						switch (property.ToLowerInvariant ()) {
						case "distributionpoint":
							certificate.RevocationUrl = value;
							break;
						default:
							throw new FormatException ($"Unknown [Revocation] property: {property}");
						}
						break;
					case "subject":
						try {
							certificate.Add (property, value);
						} catch (ArgumentException) {
							throw new FormatException ($"Unknown [Subject] property: {property}");
						}
						break;
					case "subjectalternativenames":
						try {
							certificate.AddSubjectAlternativeName (property, value);
						} catch (ArgumentException) {
							throw new FormatException ($"Unknown [SubjectAlternativeNames] property: {property}");
						}
						break;
					case "smimecapabilities":
						try {
							certificate.AddSMimeCapability (value);
						} catch (ArgumentException) {
							throw new FormatException ($"Unknown [SMimeCapabilities] value: {value}");
						}
						break;
					case "generator":
						switch (property.ToLowerInvariant ()) {
						case "basicconstraints":
							options.BasicConstraints = value;
							break;
						case "daysvalid":
							if (int.TryParse (value, out int days)) {
								options.DaysValid = days;
							} else {
								throw new FormatException ($"Invalid [Generator] DaysValid: {value}");
							}
							break;
						case "issuer":
							if (!string.IsNullOrEmpty (value) && value != "this")
								options.Issuer = GetFileName (baseDirectory, value);
							else
								options.Issuer = value;
							break;
						case "issuerpassword":
							options.IssuerPassword = value;
							break;
						case "keyusage":
							options.KeyUsage = value;
							break;
						case "output":
							options.Output = GetFileName (baseDirectory, value);
							break;
						case "password":
							options.Password = value;
							break;
						case "signaturealgorithm":
							options.SignatureAlgorithm = value;
							break;
						case "includechain":
							options.IncludeChain = bool.Parse (value);
							break;

						default:
							throw new FormatException ($"Unknown [Generator] property: {property}");
						}
						break;
					default:
						throw new FormatException ($"Unknown section: {section}");
					}
				}
			}

			return Generate (options, privateKeyOptions, certificate, out privateKey);
		}
	}
}
