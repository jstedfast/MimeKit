//
// DkimArcVerifierTarget.cs
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
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Verifies the DKIM signatures and the ARC chain of a message against a fuzzed DNS zone (see FuzzDnsZone for
	// the input format). The verifiers must report every problem in their results rather than throw, the sync and
	// async verifiers must give identical results after making identical DNS queries, the results must not
	// depend on the newline format of the FormatOptions, and the results must survive a round-trip through an
	// Authentication-Results header.
	sealed class DkimArcVerifierTarget : IFuzzTarget
	{
		public string Name => "dkim-arc-verifier";

		public int MaxLength => 64 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var zone = FuzzDnsZone.Parse (data, out var content);
			var message = VerifierChecks.ParseMessage (content);

			if (message is null)
				return;

			bool sha1 = (Fuzz.Selector (data) & 1) != 0;

			CheckDkim (zone, message, sha1);
			CheckArc (zone, message, sha1);
		}

		static DkimVerifier CreateDkimVerifier (FuzzDnsResolver resolver, bool sha1)
		{
			var verifier = new DkimVerifier (resolver);

			if (sha1)
				verifier.Enable (DkimSignatureAlgorithm.RsaSha1);

			return verifier;
		}

		static ArcVerifier CreateArcVerifier (FuzzDnsResolver resolver, bool sha1)
		{
			var verifier = new ArcVerifier (resolver);

			if (sha1)
				verifier.Enable (DkimSignatureAlgorithm.RsaSha1);

			return verifier;
		}

		static void CheckDkim (FuzzDnsZone zone, MimeMessage message, bool sha1)
		{
			var dos = new FuzzDnsResolver (zone);
			var results = CreateDkimVerifier (dos, sha1).Verify (VerifierChecks.CreateFormatOptions (NewLineFormat.Dos), message);

			foreach (var result in results)
				VerifierChecks.CheckResult (result);

			var expected = VerifierChecks.Dump (results);

			var async = new FuzzDnsResolver (zone);
			var asyncResults = Fuzz.Wait (CreateDkimVerifier (async, sha1).VerifyAsync (VerifierChecks.CreateFormatOptions (NewLineFormat.Dos), message));

			Fuzz.Equal (expected, VerifierChecks.Dump (asyncResults), "DkimVerifier.Verify vs VerifyAsync");
			Fuzz.Equal (string.Join ("\n", dos.Queries), string.Join ("\n", async.Queries), "DkimVerifier.Verify vs VerifyAsync DNS queries");

			// DKIM signatures are always computed over CRLF line endings, whatever the caller's FormatOptions say.
			var unix = new FuzzDnsResolver (zone);
			var unixResults = CreateDkimVerifier (unix, sha1).Verify (VerifierChecks.CreateFormatOptions (NewLineFormat.Unix), message);

			Fuzz.Equal (expected, VerifierChecks.Dump (unixResults), "DkimVerifier.Verify with Dos vs Unix FormatOptions");

			VerifierChecks.CheckAuthenticationResults (results.Select (r => r.ToAuthenticationMethodResult ()), "DkimSignatureValidationResult.ToAuthenticationMethodResult");
		}

		static void CheckArc (FuzzDnsZone zone, MimeMessage message, bool sha1)
		{
			var dos = new FuzzDnsResolver (zone);
			var result = CreateArcVerifier (dos, sha1).Verify (VerifierChecks.CreateFormatOptions (NewLineFormat.Dos), message);

			VerifierChecks.CheckResult (result);

			var expected = VerifierChecks.Dump (result);

			var async = new FuzzDnsResolver (zone);
			var asyncResult = Fuzz.Wait (CreateArcVerifier (async, sha1).VerifyAsync (VerifierChecks.CreateFormatOptions (NewLineFormat.Dos), message));

			Fuzz.Equal (expected, VerifierChecks.Dump (asyncResult), "ArcVerifier.Verify vs VerifyAsync");
			Fuzz.Equal (string.Join ("\n", dos.Queries), string.Join ("\n", async.Queries), "ArcVerifier.Verify vs VerifyAsync DNS queries");

			var unix = new FuzzDnsResolver (zone);
			var unixResult = CreateArcVerifier (unix, sha1).Verify (VerifierChecks.CreateFormatOptions (NewLineFormat.Unix), message);

			Fuzz.Equal (expected, VerifierChecks.Dump (unixResult), "ArcVerifier.Verify with Dos vs Unix FormatOptions");
		}
	}
}
