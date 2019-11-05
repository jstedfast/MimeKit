using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace CertificateGenerator
{
	class Program
	{
		public static void Main (string[] args)
		{
			var x509NameOids = CreateX509NameOidMapping ();
			var oids = new List<DerObjectIdentifier> ();
			var values = new List<string> ();
			var privateKey = new PrivateKeyOptions ();
			var options = new GeneratorOptions ();
			AsymmetricCipherKeyPair key;
			string section = null;
			string alias = null;

			options.Output = Path.ChangeExtension (args[0], ".pfx");

			using (var reader = File.OpenText (args[0])) {
				string line;

				while ((line = reader.ReadLine ()) != null) {
					if (line.Length == 0 || line[0] == '#')
						continue;

					if (line[0] == '[') {
						int endIndex = line.IndexOf (']');

						if (endIndex == -1) {
							Console.Error.WriteLine ("Incomplete section: ", line);
							return;
						}

						section = line.Substring (1, endIndex - 1);
						continue;
					}

					var kvp = line.Split (new char[] { '=' }, 2);
					var property = kvp[0].ToLowerInvariant ().Trim ();
					var value = kvp[1].Trim ();

					switch (section.ToLowerInvariant ()) {
					case "privatekey":
						switch (property) {
						case "algorithm":
							privateKey.Algorithm = value;
							break;
						case "bitlength":
							if (int.TryParse (value, out int bitLength)) {
								privateKey.BitLength = bitLength;
							} else {
								Console.Error.WriteLine ("Invalid [PrivateKey] BitLength: {0}", value);
								return;
							}
							break;
						case "filename":
							privateKey.FileName = value;
							break;
						default:
							Console.Error.WriteLine ("Unknown [PrivateKey] property: {0}", kvp[0]);
							return;
						}
						break;
					case "subject":
						if (x509NameOids.TryGetValue (property, out DerObjectIdentifier oid)) {
							if (oid == X509Name.CN)
								alias = value;
							else if (alias == null && oid == X509Name.E)
								alias = value;

							values.Add (value);
							oids.Add (oid);
						} else {
							Console.Error.WriteLine ("Unknown [Subject] property: {0}", kvp[0]);
							return;
						}
						break;
					case "generator":
						switch (property) {
						case "basicconstraints":
							options.BasicConstraints = value;
							break;
						case "daysvalid":
							if (int.TryParse (value, out int days)) {
								options.DaysValid = days;
							} else {
								Console.Error.WriteLine ("Invalid [Generator] DaysValid: {0}", value);
								return;
							}
							break;
						case "issuer":
							options.Issuer = value;
							break;
						case "issuerpassword":
							options.IssuerPassword = value;
							break;
						case "keyusage":
							options.KeyUsage = value;
							break;
						case "output":
							options.Output = value;
							break;
						case "password":
							options.Password = value;
							break;
						case "signaturealgorithm":
							options.SignatureAlgorithm = value;
							break;
						default:
							Console.Error.WriteLine ("Unknown [Generator] property: {0}", kvp[0]);
							return;
						}
						break;
					default:
						Console.Error.WriteLine ("Unknown section: {0}", section);
						break;
					}
				}
			}

			// Sanity Checks
			if (!string.IsNullOrEmpty (privateKey.FileName) && !File.Exists (privateKey.FileName)) {
				Console.Error.WriteLine ("[PrivateKey] FileName `{0}' does not exist!", privateKey.FileName);
				return;
			}

			if (oids.Count == 0) {
				Console.Error.WriteLine ("No [Subject] specified.");
				return;
			}

			if (string.IsNullOrEmpty (options.Issuer)) {
				Console.Error.WriteLine ("[Generator] Issuer property cannot be empty!");
				return;
			} else if (options.Issuer != "this" && !File.Exists (options.Issuer)) {
				Console.Error.WriteLine ("[Generator] Issuer `{0}' does not exist!", options.Issuer);
				return;
			}

			if (string.IsNullOrEmpty (options.Output)) {
				Console.Error.WriteLine ("[Generator] Output property cannot be empty!");
				return;
			}

			var randomGenerator = new CryptoApiRandomGenerator ();
			var random = new SecureRandom (randomGenerator);
			var subject = new X509Name (oids, values);

			if (string.IsNullOrEmpty (privateKey.FileName)) {
				var keyGenerationParameters = new KeyGenerationParameters (random, privateKey.BitLength);
				IAsymmetricCipherKeyPairGenerator keyPairGenerator;

				switch (privateKey.Algorithm.ToLowerInvariant ()) {
				case "rsa": keyPairGenerator = new RsaKeyPairGenerator (); break;
				case "ecdsa": keyPairGenerator = new ECKeyPairGenerator ("ECDSA"); break;
				default: Console.Error.WriteLine ("Unsupported PrivateKey algorithm: {0}", privateKey.Algorithm); return;
				}
				keyPairGenerator.Init (keyGenerationParameters);
				key = keyPairGenerator.GenerateKeyPair ();
			} else {
				try {
					key = LoadAsymmetricCipherKeyPair (privateKey.FileName);
				} catch (Exception ex) {
					Console.Error.WriteLine ("[PrivateKey] Failed to load `{0}': {1}", privateKey.FileName, ex.Message);
					return;
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
					Console.Error.WriteLine ("[Generator] failed to load `{0}': {1}", options.Issuer, ex.Message);
					return;
				}
			} else {
				chain = new X509Certificate[0];
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

			int serialNumberIndex = oids.IndexOf (X509Name.SerialNumber);
			BigInteger serialNumber;

			if (serialNumberIndex == -1) {
				serialNumber = BigIntegers.CreateRandomInRange (BigInteger.One, BigInteger.ValueOf (long.MaxValue), random);
			} else {
				try {
					serialNumber = new BigInteger (values[serialNumberIndex]);
				} catch {
					Console.Error.WriteLine ("Invalid [Subject] SerialNumber: {0}", values[serialNumberIndex]);
					return;
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

			var certificate = generator.Generate (signatureFactory);
			var keyEntry = new AsymmetricKeyEntry (key.Private);

			var chainEntries = new X509CertificateEntry[chain.Length + 1];
			chainEntries[0] = new X509CertificateEntry (certificate);
			for (int i = 0; i < chain.Length; i++)
				chainEntries[i + 1] = new X509CertificateEntry (chain[i]);

			var pkcs12 = new Pkcs12Store ();
			pkcs12.SetKeyEntry (alias ?? string.Empty, keyEntry, chainEntries);

			using (var stream = File.Create (options.Output))
				pkcs12.Save (stream, options.Password.ToCharArray (), random);

			Console.WriteLine ("{0} {1}", options.Output, GetFingerprint (certificate));
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
						if (key is RsaPrivateCrtKeyParameters rsa) {
							var pub = new RsaKeyParameters (false, rsa.Modulus, rsa.Exponent);

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
				var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

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

		static Dictionary<string, DerObjectIdentifier> CreateX509NameOidMapping ()
		{
			var mapping = new Dictionary<string, DerObjectIdentifier> ();
			mapping.Add ("c", X509Name.C);
			mapping.Add ("countrycode", X509Name.C);
			mapping.Add ("countryname", X509Name.C);
			mapping.Add ("st", X509Name.ST);
			mapping.Add ("stateorprovincename", X509Name.ST);
			mapping.Add ("l", X509Name.L);
			mapping.Add ("localityname", X509Name.L);
			mapping.Add ("street", X509Name.Street);
			mapping.Add ("postaladdress", X509Name.PostalAddress);
			mapping.Add ("postalcode", X509Name.PostalCode);
			mapping.Add ("o", X509Name.O);
			mapping.Add ("organizationname", X509Name.O);
			mapping.Add ("ou", X509Name.OU);
			mapping.Add ("organizationalunitname", X509Name.OU);
			mapping.Add ("cn", X509Name.CN);
			mapping.Add ("commonname", X509Name.CN);
			mapping.Add ("e", X509Name.E);
			mapping.Add ("emailaddress", X509Name.E);
			mapping.Add ("serialnumber", X509Name.SerialNumber);
			mapping.Add ("t", X509Name.T);
			mapping.Add ("title", X509Name.T);
			mapping.Add ("dc", X509Name.DC);
			mapping.Add ("uid", X509Name.UID);
			mapping.Add ("surname", X509Name.Surname);
			mapping.Add ("givenname", X509Name.GivenName);
			mapping.Add ("initials", X509Name.Initials);
			mapping.Add ("generation", X509Name.Generation);
			mapping.Add ("unstructuredaddress", X509Name.UnstructuredAddress);
			mapping.Add ("unstructuredname", X509Name.UnstructuredName);
			mapping.Add ("uniqueidentifier", X509Name.UniqueIdentifier);
			mapping.Add ("dn", X509Name.DnQualifier);
			mapping.Add ("pseudonym", X509Name.Pseudonym);
			mapping.Add ("nameofbirth", X509Name.NameAtBirth);
			mapping.Add ("countryofcitizenship", X509Name.CountryOfCitizenship);
			mapping.Add ("countryofresidence", X509Name.CountryOfResidence);
			mapping.Add ("gender", X509Name.Gender);
			mapping.Add ("placeofbirth", X509Name.PlaceOfBirth);
			mapping.Add ("dateofbirth", X509Name.DateOfBirth);
			mapping.Add ("businesscategory", X509Name.BusinessCategory);
			mapping.Add ("telephonenumber", X509Name.TelephoneNumber);
			return mapping;
		}
	}

	sealed class PrivateKeyOptions
	{
		public PrivateKeyOptions ()
		{
			Algorithm = "RSA";
			BitLength = 2048;
		}

		public string Algorithm {
			get; set;
		}

		public int BitLength {
			get; set;
		}

		public string FileName {
			get; set;
		}
	}

	sealed class GeneratorOptions
	{
		public GeneratorOptions ()
		{
			IssuerPassword = string.Empty;
			Password = string.Empty;
		}

		public string BasicConstraints {
			get; set;
		}

		public int DaysValid {
			get; set;
		}

		public string Issuer {
			get; set;
		}

		public string IssuerPassword {
			get; set;
		}

		public string KeyUsage {
			get; set;
		}

		public string Output {
			get; set;
		}

		public string Password {
			get; set;
		}

		public string SignatureAlgorithm {
			get; set;
		}
	}
}
