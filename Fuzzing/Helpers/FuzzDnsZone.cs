//
// FuzzDnsZone.cs
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

using MimeKit.Cryptography;

namespace MimeKit.Fuzzing.Helpers {
	// The input format shared by the DKIM, ARC and DMARC verifier targets: a DNS zone, a line containing only
	// "%%", then the message. Without a "%%" line, the whole input is the message and the zone is empty.
	//
	// Each zone line is "<name> <txt>" and adds one TXT record for <name>. These special <txt> values set the
	// outcome of a query for <name> instead:
	//
	//   !nxdomain   DnsQueryStatus.NonExistentDomain
	//   !tempfail   DnsQueryStatus.TemporaryFailure
	//   !empty      DnsQueryStatus.Success without any records
	//   !throw      QueryTxt throws an exception (which the verifiers must treat as a temporary failure)
	//   !null       QueryTxt returns null (which the verifiers must also tolerate)
	//
	// A name without any line is NXDOMAIN. Lines starting with "#" are ignored, and "@spf <status> <domain>"
	// sets the SPF result that is passed to DmarcVerifier.
	sealed class FuzzDnsZone
	{
		const int MaxRecordsPerName = 16;

		readonly Dictionary<string, List<string>> records = new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, string> outcomes = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

		public SpfCheckResult? SpfResult { get; private set; }

		public static FuzzDnsZone Parse (ReadOnlySpan<byte> data, out ReadOnlySpan<byte> message)
		{
			var zone = new FuzzDnsZone ();
			int index = 0;

			while (index < data.Length) {
				int eol = data.Slice (index).IndexOf ((byte) '\n');
				int next = eol == -1 ? data.Length : index + eol + 1;
				var line = data.Slice (index, (eol == -1 ? data.Length : index + eol) - index);

				if (line.Length > 0 && line[line.Length - 1] == (byte) '\r')
					line = line.Slice (0, line.Length - 1);

				if (line.SequenceEqual ("%%"u8)) {
					message = data.Slice (next);
					return zone;
				}

				zone.AddLine (Fuzz.Latin1.GetString (line));
				index = next;
			}

			// There is no "%%" line, so the whole input is the message.
			message = data;

			return new FuzzDnsZone ();
		}

		static SpfStatus? ParseSpfStatus (string value)
		{
			switch (value.ToLowerInvariant ()) {
			case "none": return SpfStatus.None;
			case "neutral": return SpfStatus.Neutral;
			case "pass": return SpfStatus.Pass;
			case "fail": return SpfStatus.Fail;
			case "softfail": return SpfStatus.SoftFail;
			case "temperror": return SpfStatus.TempError;
			case "permerror": return SpfStatus.PermError;
			default: return null;
			}
		}

		void AddLine (string line)
		{
			if (line.Length == 0 || line[0] == '#')
				return;

			int space = line.IndexOfAny (new[] { ' ', '\t' });
			var name = space == -1 ? line : line.Substring (0, space);
			var value = space == -1 ? string.Empty : line.Substring (space + 1);

			if (name == "@spf") {
				var fields = value.Split (new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

				if (fields.Length == 2 && ParseSpfStatus (fields[0]) is SpfStatus status)
					SpfResult = new SpfCheckResult (status, fields[1]);

				return;
			}

			if (value.Length > 1 && value[0] == '!') {
				outcomes[name] = value;
				return;
			}

			if (!records.TryGetValue (name, out var list))
				records.Add (name, list = new List<string> ());

			if (list.Count < MaxRecordsPerName)
				list.Add (value);
		}

		public DnsTxtResponse? Query (string domain)
		{
			if (outcomes.TryGetValue (domain, out var outcome)) {
				switch (outcome) {
				case "!nxdomain": return new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);
				case "!tempfail": return new DnsTxtResponse (DnsQueryStatus.TemporaryFailure);
				case "!empty": return new DnsTxtResponse (Array.Empty<string> ());
				case "!throw": throw new FuzzDnsException (domain);
				case "!null": return null;
				}
			}

			if (records.TryGetValue (domain, out var list))
				return new DnsTxtResponse (list);

			return new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);
		}
	}

	sealed class FuzzDnsException : Exception
	{
		public FuzzDnsException (string domain) : base ($"Simulated DNS failure for {domain}")
		{
		}
	}

	// An IDnsResolver backed by a FuzzDnsZone that records every query, so that the sync and async verifiers
	// can be required to make exactly the same queries in the same order.
	sealed class FuzzDnsResolver : IDnsResolver
	{
		readonly FuzzDnsZone zone;

		public FuzzDnsResolver (FuzzDnsZone zone)
		{
			this.zone = zone;
		}

		public List<string> Queries { get; } = new List<string> ();

		public DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default)
		{
			Fuzz.Check (domain != null, "IDnsResolver.QueryTxt was called with a null domain");
			Queries.Add (domain!);

			return zone.Query (domain!)!;
		}

		public async Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default)
		{
			Fuzz.Check (domain != null, "IDnsResolver.QueryTxtAsync was called with a null domain");
			Queries.Add (domain!);

			// Complete asynchronously, like a real resolver, so that the async code paths really are exercised.
			await Task.Yield ();

			return zone.Query (domain!)!;
		}
	}
}
