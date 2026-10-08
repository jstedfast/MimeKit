//
// Program.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2014-2024 Jeffrey Stedfast
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
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using DnsClient;

using MimeKit;
using MimeKit.Cryptography;

namespace DkimVerifierExample
{
	class DnsResolver : IDnsResolver
	{
		readonly ConcurrentDictionary<string, DnsTxtResponse> cache;
		readonly LookupClient dnsClient;

		public DnsResolver ()
		{
			cache = new ConcurrentDictionary<string, DnsTxtResponse> (StringComparer.OrdinalIgnoreCase);

			var options = new LookupClientOptions (IPAddress.Parse ("8.8.8.8")) {
				UseCache = true,
				Retries = 3
			};

			dnsClient = new LookupClient (options);
		}

		DnsTxtResponse GetTxtResponse (string domain, IDnsQueryResponse response)
		{
			DnsTxtResponse result;

			if (response.HasError) {
				if (response.Header.ResponseCode == DnsHeaderResponseCode.NotExistentDomain)
					result = new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);
				else
					return new DnsTxtResponse (DnsQueryStatus.TemporaryFailure);
			} else {
				// Each TXT record may be split into multiple character-strings which must be concatenated.
				// Note: separate TXT records must *not* be concatenated together.
				var records = response.Answers.TxtRecords ().Select (record => string.Concat (record.Text));

				result = new DnsTxtResponse (records);
			}

			// only cache definitive answers
			cache[domain] = result;

			return result;
		}

		public DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default)
		{
			// check if we've already fetched this record
			if (cache.TryGetValue (domain, out var cached))
				return cached;

			try {
				var response = dnsClient.Query (domain, QueryType.TXT, QueryClass.IN);

				return GetTxtResponse (domain, response);
			} catch (DnsResponseException) {
				return new DnsTxtResponse (DnsQueryStatus.TemporaryFailure);
			}
		}

		public async Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default)
		{
			// check if we've already fetched this record
			if (cache.TryGetValue (domain, out var cached))
				return cached;

			try {
				var response = await dnsClient.QueryAsync (domain, QueryType.TXT, QueryClass.IN, cancellationToken).ConfigureAwait (false);

				return GetTxtResponse (domain, response);
			} catch (DnsResponseException) {
				return new DnsTxtResponse (DnsQueryStatus.TemporaryFailure);
			}
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

			var resolver = new DnsResolver ();
			var verifier = new DkimVerifier (resolver);

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
