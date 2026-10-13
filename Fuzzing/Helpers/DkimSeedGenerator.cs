//
// DkimSeedGenerator.cs
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

using System.Text;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Cryptography;

namespace MimeKit.Fuzzing.Helpers {
	// Generates seeds for the dkim-arc-verifier and dmarc-verifier targets by signing messages with DKIM (RSA and
	// Ed25519) and ARC, and prepending a FuzzDnsZone that publishes the matching keys and a DMARC policy, so that
	// the fuzzer starts from signatures that actually verify. Used by scripts/fuzz-seed-corpora.ps1.
	static class DkimSeedGenerator
	{
		// The Ed25519 private key from RFC 8463, Appendix A.2.
		const string Ed25519PrivateKey = "nWGxne/9WmC6hEr0kuwsxERJxWl7MmkZcDusAxyuf2A=";
		const int MaxSeedLength = 60 * 1024;

		static readonly string[] SignedHeaders = { "From", "To", "Cc", "Subject", "Date", "Message-ID", "MIME-Version", "Content-Type" };

		sealed class SeedArcSigner : ArcSigner
		{
			public SeedArcSigner (AsymmetricKeyParameter key, string domain, string selector) : base (key, domain, selector)
			{
			}

			protected override AuthenticationResults GenerateArcAuthenticationResults (FormatOptions options, MimeMessage message, CancellationToken cancellationToken)
			{
				var results = new AuthenticationResults ("mx.fuzz.example");

				results.Results.Add (new AuthenticationMethodResult ("spf", "pass"));
				results.Results.Add (new AuthenticationMethodResult ("dkim", "pass"));

				return results;
			}

			protected override Task<AuthenticationResults> GenerateArcAuthenticationResultsAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken)
			{
				return Task.FromResult (GenerateArcAuthenticationResults (options, message, cancellationToken));
			}
		}

		public static int Run (string[] args)
		{
			if (args.Length < 4) {
				Console.Error.WriteLine ("Usage: MimeKit.Fuzzing --generate-dkim-seeds <output-dir> <rsa-key.pem> <file|directory>...");
				return 2;
			}

			var output = args[1];
			AsymmetricCipherKeyPair rsa;

			using (var reader = new PemReader (new StreamReader (args[2])))
				rsa = (AsymmetricCipherKeyPair) reader.ReadObject ();

			var ed25519 = new Ed25519PrivateKeyParameters (Convert.FromBase64String (Ed25519PrivateKey), 0);
			var rsaRecord = "v=DKIM1; k=rsa; p=" + Convert.ToBase64String (SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo (rsa.Public).GetDerEncoded ());
			var ed25519Record = "v=DKIM1; k=ed25519; p=" + Convert.ToBase64String (ed25519.GeneratePublicKey ().GetEncoded ());

			Directory.CreateDirectory (output);

			int count = 0;

			for (int i = 3; i < args.Length; i++) {
				IEnumerable<string> files;

				if (Directory.Exists (args[i]))
					files = Directory.EnumerateFiles (args[i], "*", SearchOption.AllDirectories).OrderBy (f => f, StringComparer.Ordinal);
				else
					files = new[] { args[i] };

				foreach (var file in files) {
					var name = Path.GetFileName (file);

					for (int variant = 0; variant < 3; variant++) {
						byte[] seed;

						try {
							seed = Generate (file, variant, rsa.Private, ed25519, rsaRecord, ed25519Record);
						} catch (Exception ex) {
							// Not every corpus file is a message that can be signed (and signing bugs are not what
							// these seeds are for), so just skip it.
							Console.Error.WriteLine ($"warning: skipping {file}: {ex.Message}");
							break;
						}

						if (seed.Length > MaxSeedLength)
							break;

						File.WriteAllBytes (Path.Combine (output, $"{name}.{variant}"), seed);
						count++;
					}
				}
			}

			Console.WriteLine ($"Generated {count} DKIM/ARC seed(s).");

			return 0;
		}

		static string GetDomain (MimeMessage message)
		{
			foreach (var mailbox in message.From.Mailboxes) {
				if (DnsDomainName.TryNormalize (mailbox.Domain, out var domain))
					return domain;
			}

			message.From.Clear ();
			message.From.Add (new MailboxAddress ("Fuzz", "fuzz@example.com"));

			return "example.com";
		}

		static byte[] Generate (string file, int variant, AsymmetricKeyParameter rsa, Ed25519PrivateKeyParameters ed25519, string rsaRecord, string ed25519Record)
		{
			var message = MimeMessage.Load (file);
			var domain = GetDomain (message);
			var zone = new StringBuilder ();

			zone.Append ("fuzz._domainkey.").Append (domain).Append (' ').Append (rsaRecord).Append ('\n');
			zone.Append ("ed._domainkey.").Append (domain).Append (' ').Append (ed25519Record).Append ('\n');
			zone.Append ("_dmarc.").Append (domain).Append (" v=DMARC1; p=reject; adkim=r; aspf=r\n");
			zone.Append ("@spf pass ").Append (domain).Append ('\n');

			switch (variant) {
			case 0:
				new DkimSigner (rsa, domain, "fuzz") {
					HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
					BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
				}.Sign (message, SignedHeaders);
				break;
			case 1:
				new DkimSigner (ed25519, domain, "ed", DkimSignatureAlgorithm.Ed25519Sha256) {
					HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
					BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
				}.Sign (message, SignedHeaders);
				break;
			default:
				new DkimSigner (rsa, domain, "fuzz").Sign (message, SignedHeaders);
				new SeedArcSigner (rsa, domain, "fuzz").Sign (message, SignedHeaders);
				break;
			}

			using var stream = new MemoryStream ();

			var bytes = Fuzz.Utf8.GetBytes (zone.Append ("%%\n").ToString ());
			stream.Write (bytes, 0, bytes.Length);

			message.WriteTo (MimeRoundTrip.CreateFormatOptions (), stream);

			return stream.ToArray ();
		}
	}
}
