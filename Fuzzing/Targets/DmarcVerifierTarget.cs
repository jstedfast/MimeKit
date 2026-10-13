//
// DmarcVerifierTarget.cs
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

using MimeKit.Cryptography;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Evaluates DMARC for a message against a fuzzed DNS zone and SPF result (see FuzzDnsZone for the input
	// format). The sync and async verifiers must give identical results after making identical DNS queries,
	// verifying with precomputed DKIM results must reach the same decision as letting the verifier compute them,
	// the result must be self-consistent, and it must survive a round-trip through an Authentication-Results header.
	sealed class DmarcVerifierTarget : IFuzzTarget
	{
		public string Name => "dmarc-verifier";

		public int MaxLength => 64 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var zone = FuzzDnsZone.Parse (data, out var content);
			var message = VerifierChecks.ParseMessage (content);

			if (message is null)
				return;

			int maxAuthorDomains = (Fuzz.Selector (data) & 1) != 0 ? 4 : 1;
			var options = VerifierChecks.CreateFormatOptions (NewLineFormat.Dos);
			var spf = zone.SpfResult;

			var resolver = new FuzzDnsResolver (zone);
			var result = CreateVerifier (resolver, maxAuthorDomains).Verify (options, message, spf);

			CheckResult (result, spf, true);

			var expected = Dump (result);

			var async = new FuzzDnsResolver (zone);
			var asyncResult = Fuzz.Wait (CreateVerifier (async, maxAuthorDomains).VerifyAsync (options, message, spf));

			Fuzz.Equal (expected, Dump (asyncResult), "DmarcVerifier.Verify vs VerifyAsync");
			Fuzz.Equal (string.Join ("\n", resolver.Queries), string.Join ("\n", async.Queries), "DmarcVerifier.Verify vs VerifyAsync DNS queries");

			// Verifying with DKIM results that were computed up front must reach the same decision.
			var dkimResults = new DkimVerifier (new FuzzDnsResolver (zone)).Verify (options, message);
			var precomputed = CreateVerifier (new FuzzDnsResolver (zone), maxAuthorDomains).Verify (message, dkimResults, spf);

			CheckResult (precomputed, spf, true);
			Fuzz.Equal (DumpDecision (result), DumpDecision (precomputed), "DmarcVerifier.Verify with computed vs precomputed DKIM results");

			var asyncPrecomputed = Fuzz.Wait (CreateVerifier (new FuzzDnsResolver (zone), maxAuthorDomains).VerifyAsync (message, dkimResults, spf));

			Fuzz.Equal (Dump (precomputed), Dump (asyncPrecomputed), "DmarcVerifier.Verify vs VerifyAsync with precomputed DKIM results");

			var methods = new List<AuthenticationMethodResult> { result.ToAuthenticationMethodResult () };

			if (result.AuthorDomainResults != null) {
				foreach (var domainResult in result.AuthorDomainResults)
					methods.Add (domainResult.ToAuthenticationMethodResult ());
			}

			VerifierChecks.CheckAuthenticationResults (methods, "DmarcValidationResult.ToAuthenticationMethodResult");
		}

		static DmarcVerifier CreateVerifier (FuzzDnsResolver resolver, int maxAuthorDomains)
		{
			return new DmarcVerifier (resolver) {
				MaxAuthorDomains = maxAuthorDomains
			};
		}

		static void CheckResult (DmarcValidationResult result, SpfCheckResult? spf, bool top)
		{
			Fuzz.Check (Enum.IsDefined (result.Status), $"DmarcValidationResult has an undefined Status: {result.Status}");
			Fuzz.Check (result.SpfResult == spf, "DmarcValidationResult.SpfResult is not the SPF result that was passed in");

			switch (result.Status) {
			case DmarcStatus.Pass:
				Fuzz.Check (result.DkimAligned || result.SpfAligned, "DmarcValidationResult passed without an aligned identifier");
				Fuzz.Check (result.Record != null && result.PolicyDomain != null, "DmarcValidationResult passed without a policy record");
				break;
			case DmarcStatus.Fail:
				Fuzz.Check (!result.DkimAligned && !result.SpfAligned, "DmarcValidationResult failed with an aligned identifier");
				Fuzz.Check (result.Policy.HasValue, "DmarcValidationResult failed without a policy");
				break;
			}

			if (result.DkimAligned) {
				Fuzz.Check (result.AlignedDkimResult != null && result.AlignedDkimResult.Status == DkimSignatureStatus.Pass, "DmarcValidationResult.DkimAligned is set without a passing AlignedDkimResult");
			} else {
				Fuzz.Check (result.AlignedDkimResult is null, "DmarcValidationResult.AlignedDkimResult is set but DkimAligned is not");
			}

			if (result.SpfAligned)
				Fuzz.Check (spf != null && spf.Status == SpfStatus.Pass, "DmarcValidationResult.SpfAligned is set without a passing SPF result");

			if (result.Policy.HasValue && result.RequestedPolicy.HasValue && result.IsTesting)
				Fuzz.Check (result.Policy.Value <= result.RequestedPolicy.Value || result.Policy.Value == DmarcPolicy.None, "DmarcValidationResult testing mode made the policy stricter");

			foreach (var dkim in result.DkimResults)
				VerifierChecks.CheckResult (dkim);

			if (result.AuthorDomainResults != null) {
				Fuzz.Check (top, "A per-Author-Domain DmarcValidationResult has its own AuthorDomainResults");
				Fuzz.Check (result.AuthorDomainResults.Count > 1, "DmarcValidationResult.AuthorDomainResults has fewer than 2 results");

				bool found = false;

				foreach (var domainResult in result.AuthorDomainResults) {
					found |= ReferenceEquals (domainResult, result);

					if (!ReferenceEquals (domainResult, result))
						CheckResult (domainResult, spf, false);
				}

				Fuzz.Check (found, "DmarcValidationResult is not one of its own AuthorDomainResults");
			}
		}

		// The fields that make up the DMARC decision, which must not depend on how the DKIM results were obtained.
		static string DumpDecision (DmarcValidationResult result)
		{
			var builder = new StringBuilder ();

			builder.Append ("status=").Append (result.Status);
			builder.Append (" errors=").Append (result.Errors);
			builder.Append (" author=").Append (result.AuthorDomain ?? "(null)");
			builder.Append (" policy-domain=").Append (result.PolicyDomain ?? "(null)");
			builder.Append (" source=").Append (result.PolicyDomainSource?.ToString () ?? "(null)");
			builder.Append (" org=").Append (result.OrganizationalDomain ?? "(null)");
			builder.Append (" requested=").Append (result.RequestedPolicy?.ToString () ?? "(null)");
			builder.Append (" policy=").Append (result.Policy?.ToString () ?? "(null)");
			builder.Append (" testing=").Append (result.IsTesting);
			builder.Append (" dkim-aligned=").Append (result.DkimAligned);
			builder.Append (" spf-aligned=").Append (result.SpfAligned);

			if (result.AlignedDkimResult != null)
				builder.Append (" aligned=").Append (result.AlignedDkimResult.Domain).Append ('/').Append (result.AlignedDkimResult.Selector);

			builder.Append ('\n');

			return builder.ToString ();
		}

		static void Dump (StringBuilder builder, DmarcValidationResult result)
		{
			builder.Append (DumpDecision (result));

			foreach (var dkim in result.DkimResults) {
				builder.Append ("  ");
				VerifierChecks.Dump (builder, dkim);
			}
		}

		static string Dump (DmarcValidationResult result)
		{
			var builder = new StringBuilder ();

			Dump (builder, result);

			if (result.AuthorDomainResults != null) {
				foreach (var domainResult in result.AuthorDomainResults) {
					builder.Append ("author-domain: ");
					Dump (builder, domainResult);
				}
			}

			return builder.ToString ();
		}
	}
}
