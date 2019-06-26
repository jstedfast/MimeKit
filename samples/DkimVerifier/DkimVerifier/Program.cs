using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Heijden.DNS;

using Org.BouncyCastle.Crypto;

using MimeKit;
using MimeKit.Cryptography;

namespace DkimVerifierExample
{
	class DkimPublicKeyLocator : DkimPublicKeyLocatorBase
	{
		readonly Dictionary<string, AsymmetricKeyParameter> cache;
		readonly Resolver resolver;

		public DkimPublicKeyLocator ()
		{
			cache = new Dictionary<string, AsymmetricKeyParameter> ();

			resolver = new Resolver ("8.8.8.8") {
				TransportType = TransportType.Udp,
				UseCache = true,
				Retries = 3
			};
		}

		AsymmetricKeyParameter DnsLookup (string domain, string selector, CancellationToken cancellationToken)
		{
			var query = selector + "._domainkey." + domain;
			AsymmetricKeyParameter pubkey;

			// checked if we've already fetched this key
			if (cache.TryGetValue (query, out pubkey))
				return pubkey;

			// make a DNS query
			var response = resolver.Query (query, QType.TXT);
			var builder = new StringBuilder ();

			// combine the TXT records into 1 string buffer
			foreach (var record in response.RecordsTXT) {
				foreach (var text in record.TXT)
					builder.Append (text);
			}

			var txt = builder.ToString ();

			pubkey = GetPublicKey (txt);
			cache.Add (query, pubkey);

			return pubkey;
		}

		public override AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
		{
			var methodList = methods.Split (new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < methodList.Length; i++) {
				if (methodList[i] == "dns/txt")
					return DnsLookup (domain, selector, cancellationToken);
			}

			throw new NotSupportedException (string.Format ("{0} does not include any suported lookup methods.", methods));
		}

		public override Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
		{
			throw new NotImplementedException ("Asynchronous DKIM public key lookup is not implemented in this sample.");
		}
	}

	class Program
	{
		public static void Main (string[] args)
		{
			if (args.Length == 0) {
				Help ();
				return;
			}

			for (int i = 0; i < args.Length; i++) {
				if (args[i] == "--help") {
					Help ();
					return;
				}
			}

			var locator = new DkimPublicKeyLocator ();
			var verifier = new DkimVerifier (locator);

			// RSA-SHA1 is disabled by default starting with MimeKit 2.2.0
			verifier.Enable (DkimSignatureAlgorithm.RsaSha1);

			for (int i = 0; i < args.Length; i++) {
				if (!File.Exists (args[i])) {
					Console.Error.WriteLine ("{0}: No such file.", args[i]);
					continue;
				}

				Console.Write ("{0} -> ", args[i]);

				var message = MimeMessage.Load (args[i]);
				var index = message.Headers.IndexOf (HeaderId.DkimSignature);

				if (index == -1) {
					Console.WriteLine ("NO SIGNATURE");
					continue;
				}

				var dkim = message.Headers[index];

				if (verifier.Verify (message, dkim)) {
					// the DKIM-Signature header is valid!
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine ("VALID");
					Console.ResetColor ();
				} else {
					// the DKIM-Signature is invalid!
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine ("INVALID");
					Console.ResetColor ();
				}
			}
		}

		static void Help ()
		{
			Console.WriteLine ("Usage is: DkimVerifier [options] [messages]");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			Console.WriteLine ("  --help               This help menu.");
		}
	}
}
