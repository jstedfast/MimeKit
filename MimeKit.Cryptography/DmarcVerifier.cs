//
// DmarcVerifier.cs
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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DMARC verifier.
	/// </summary>
	/// <remarks>
	/// <para>Validates the use of the Author Domain of a message, as specified by
	/// <a href="https://www.rfc-editor.org/rfc/rfc9989">RFC 9989</a> (Domain-based Message Authentication,
	/// Reporting, and Conformance).</para>
	/// <para>The verifier extracts the Author Domain from the From header, discovers the applicable DMARC Policy Record
	/// using the DNS Tree Walk (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.10">RFC 9989, Section 4.10</a>),
	/// and checks whether any DKIM or SPF Authenticated Identifier is aligned with the Author Domain.</para>
	/// <para>The verifier does not decide what to do with a message that fails validation; see
	/// <see cref="DmarcValidationResult"/> for details.</para>
	/// <para>The DNS Tree Walk makes at most 8 DNS queries per domain, and queries are cached for the duration
	/// of each call to <c>Verify</c>. The number of domains that are evaluated is bounded by
	/// <see cref="MaxAuthorDomains"/> and <see cref="MaxDkimSignatures"/>.</para>
	/// </remarks>
	public class DmarcVerifier
	{
		const int DefaultMaxDkimSignatures = 10;
		const int MaxTreeWalkLabels = 7;
		const string PolicyRecordPrefix = "_dmarc";

		const DmarcRecordErrors InvalidPolicyErrors = DmarcRecordErrors.InvalidPolicy | DmarcRecordErrors.InvalidSubdomainPolicy | DmarcRecordErrors.InvalidNonExistentSubdomainPolicy;

		int maxDkimSignatures = DefaultMaxDkimSignatures;
		int maxAuthorDomains = 1;

		/// <summary>
		/// Initialize a new instance of the <see cref="DmarcVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DmarcVerifier"/> that uses a new <see cref="Cryptography.DkimVerifier"/> with
		/// default settings to verify DKIM signatures.
		/// </remarks>
		/// <param name="resolver">The DNS resolver.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="resolver"/> is <see langword="null"/>.
		/// </exception>
		public DmarcVerifier (IDnsResolver resolver)
		{
			if (resolver is null)
				throw new ArgumentNullException (nameof (resolver));

			DkimVerifier = new DkimVerifier (resolver);
			DnsResolver = resolver;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DmarcVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DmarcVerifier"/> that uses the specified <see cref="Cryptography.DkimVerifier"/>
		/// to verify DKIM signatures.
		/// </remarks>
		/// <param name="resolver">The DNS resolver.</param>
		/// <param name="dkimVerifier">The DKIM verifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="resolver"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimVerifier"/> is <see langword="null"/>.</para>
		/// </exception>
		public DmarcVerifier (IDnsResolver resolver, DkimVerifier dkimVerifier)
		{
			if (resolver is null)
				throw new ArgumentNullException (nameof (resolver));

			if (dkimVerifier is null)
				throw new ArgumentNullException (nameof (dkimVerifier));

			DkimVerifier = dkimVerifier;
			DnsResolver = resolver;
		}

		/// <summary>
		/// Get the DNS resolver.
		/// </summary>
		/// <remarks>
		/// Gets the DNS resolver used to look up DMARC Policy Records.
		/// </remarks>
		/// <value>The DNS resolver.</value>
		public IDnsResolver DnsResolver {
			get;
		}

		/// <summary>
		/// Get the DKIM verifier.
		/// </summary>
		/// <remarks>
		/// Gets the DKIM verifier used by the <c>Verify</c> methods that verify the DKIM signatures of the message.
		/// </remarks>
		/// <value>The DKIM verifier.</value>
		public DkimVerifier DkimVerifier {
			get;
		}

		/// <summary>
		/// Get or set the maximum number of Author Domains to evaluate.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of distinct Author Domains that will be evaluated when the From header
		/// contains more than one mailbox.</para>
		/// <para>According to <a href="https://www.rfc-editor.org/rfc/rfc9989#section-5.3.1">RFC 9989, Section 5.3.1</a>,
		/// DMARC validation is not possible for a message with more than one Author Domain, but a Mail Receiver may
		/// choose to evaluate each of them. Since this can be abused as a denial-of-service attack
		/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-11.5">RFC 9989, Section 11.5</a>), messages with
		/// more Author Domains than this limit result in <see cref="DmarcStatus.PermError"/> with
		/// <see cref="DmarcErrors.MultipleAuthorDomains"/>.</para>
		/// <para>The default value is <c>1</c>.</para>
		/// </remarks>
		/// <value>The maximum number of Author Domains.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxAuthorDomains {
			get { return maxAuthorDomains; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxAuthorDomains = value;
			}
		}

		/// <summary>
		/// Get or set the maximum number of DKIM results to evaluate.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of DKIM results that will be checked for alignment with the
		/// Author Domain. DKIM results beyond the limit are ignored.</para>
		/// <para>Checking a DKIM-Authenticated Identifier for relaxed alignment may require a DNS Tree Walk, so this
		/// limit bounds the number of DNS queries made for messages with a large number of signatures.</para>
		/// <para>The default value is <c>10</c>.</para>
		/// </remarks>
		/// <value>The maximum number of DKIM results.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxDkimSignatures {
			get { return maxDkimSignatures; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxDkimSignatures = value;
			}
		}

		static int CountLabels (string domain)
		{
			int count = 1;

			for (int i = 0; i < domain.Length; i++) {
				if (domain[i] == '.')
					count++;
			}

			return count;
		}

		static string GetLastLabels (string domain, int count)
		{
			int index = domain.Length;

			while (count > 0) {
				index = domain.LastIndexOf ('.', index - 1);

				if (index == -1)
					return domain;

				count--;
			}

			return domain.Substring (index + 1);
		}

		static bool IsSubdomainOf (string domain, string parent)
		{
			return domain.Length > parent.Length + 1 && domain[domain.Length - parent.Length - 1] == '.' && domain.EndsWith (parent, StringComparison.Ordinal);
		}

		static DmarcPolicy ApplyTestMode (DmarcPolicy policy)
		{
			return policy == DmarcPolicy.Reject ? DmarcPolicy.Quarantine : DmarcPolicy.None;
		}

		static PolicyQueryResult GetPolicyQueryResult (DnsTxtResponse? response)
		{
			if (response is null || response.Status == DnsQueryStatus.TemporaryFailure)
				return PolicyQueryResult.TemporaryFailure;

			if (response.Status != DnsQueryStatus.Success)
				return PolicyQueryResult.NotFound;

			DmarcRecord? found = null;
			int count = 0;

			// Records that do not start with "v=DMARC1" are discarded. If multiple DMARC Policy Records are returned for
			// a single target, they are all discarded (RFC 9989, Section 4.10).
			for (int i = 0; i < response.Records.Count; i++) {
				if (DmarcRecord.TryParse (response.Records[i], out var record)) {
					found = record;
					count++;
				}
			}

			if (count == 0)
				return PolicyQueryResult.NotFound;

			if (count > 1)
				return PolicyQueryResult.MultipleRecords;

			return new PolicyQueryResult (PolicyQueryStatus.Found, found);
		}

		static DomainExistence GetDomainExistence (DnsTxtResponse? response)
		{
			if (response is null || response.Status == DnsQueryStatus.TemporaryFailure)
				return DomainExistence.Unknown;

			return response.Status == DnsQueryStatus.NonExistentDomain ? DomainExistence.NonExistent : DomainExistence.Exists;
		}

		PolicyQueryResult QueryPolicyRecord (VerificationContext context, string domain, CancellationToken cancellationToken)
		{
			if (context.PolicyQueries.TryGetValue (domain, out var result))
				return result;

			if (DnsDomainName.TryCombine (PolicyRecordPrefix, domain, out var name)) {
				try {
					result = GetPolicyQueryResult (DnsResolver.QueryTxt (name, cancellationToken));
				} catch (OperationCanceledException) {
					throw;
				} catch (Exception) {
					result = PolicyQueryResult.TemporaryFailure;
				}
			} else {
				result = PolicyQueryResult.NotFound;
			}

			context.PolicyQueries.Add (domain, result);

			return result;
		}

		async Task<PolicyQueryResult> QueryPolicyRecordAsync (VerificationContext context, string domain, CancellationToken cancellationToken)
		{
			if (context.PolicyQueries.TryGetValue (domain, out var result))
				return result;

			if (DnsDomainName.TryCombine (PolicyRecordPrefix, domain, out var name)) {
				try {
					result = GetPolicyQueryResult (await DnsResolver.QueryTxtAsync (name, cancellationToken).ConfigureAwait (false));
				} catch (OperationCanceledException) {
					throw;
				} catch (Exception) {
					result = PolicyQueryResult.TemporaryFailure;
				}
			} else {
				result = PolicyQueryResult.NotFound;
			}

			context.PolicyQueries.Add (domain, result);

			return result;
		}

		// RFC 9989, Section 4.10: query the domain itself, then (if it has 8 or more labels) skip to the 7 right-most
		// labels and remove one label at a time until a record with a psd tag is found or no labels remain. This
		// bounds the walk to at most 8 queries.
		TreeWalk GetTreeWalk (VerificationContext context, string domain, CancellationToken cancellationToken)
		{
			if (context.TreeWalks.TryGetValue (domain, out var walk))
				return walk;

			int labels = CountLabels (domain);
			string target = domain;

			walk = new TreeWalk (domain);

			while (walk.Add (target, QueryPolicyRecord (context, target, cancellationToken)) && labels > 1) {
				labels = Math.Min (labels - 1, MaxTreeWalkLabels);
				target = GetLastLabels (domain, labels);
			}

			walk.SelectOrganizationalDomain ();
			context.TreeWalks.Add (domain, walk);

			return walk;
		}

		async Task<TreeWalk> GetTreeWalkAsync (VerificationContext context, string domain, CancellationToken cancellationToken)
		{
			if (context.TreeWalks.TryGetValue (domain, out var walk))
				return walk;

			int labels = CountLabels (domain);
			string target = domain;

			walk = new TreeWalk (domain);

			while (walk.Add (target, await QueryPolicyRecordAsync (context, target, cancellationToken).ConfigureAwait (false)) && labels > 1) {
				labels = Math.Min (labels - 1, MaxTreeWalkLabels);
				target = GetLastLabels (domain, labels);
			}

			walk.SelectOrganizationalDomain ();
			context.TreeWalks.Add (domain, walk);

			return walk;
		}

		// RFC 9989, Section 3.2.13: a domain does not exist if a query for it results in NXDOMAIN.
		DomainExistence GetDomainExistence (string domain, CancellationToken cancellationToken)
		{
			try {
				return GetDomainExistence (DnsResolver.QueryTxt (domain, cancellationToken));
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception) {
				return DomainExistence.Unknown;
			}
		}

		async Task<DomainExistence> GetDomainExistenceAsync (string domain, CancellationToken cancellationToken)
		{
			try {
				return GetDomainExistence (await DnsResolver.QueryTxtAsync (domain, cancellationToken).ConfigureAwait (false));
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception) {
				return DomainExistence.Unknown;
			}
		}

		DmarcValidationResult Evaluate (VerificationContext context, string authorDomain, CancellationToken cancellationToken)
		{
			var walk = GetTreeWalk (context, authorDomain, cancellationToken);
			var evaluation = new AuthorDomainEvaluation (context, authorDomain, walk, MaxDkimSignatures);

			if (evaluation.IsComplete)
				return evaluation.Result;

			evaluation.SetDkimResults (context.GetDkimResults (cancellationToken));

			foreach (var domain in evaluation.GetIdentifierDomainsToWalk ())
				GetTreeWalk (context, domain, cancellationToken);

			if (evaluation.NeedsExistenceCheck)
				evaluation.SetAuthorDomainExistence (GetDomainExistence (authorDomain, cancellationToken));

			return evaluation.Complete ();
		}

		async Task<DmarcValidationResult> EvaluateAsync (VerificationContext context, string authorDomain, CancellationToken cancellationToken)
		{
			var walk = await GetTreeWalkAsync (context, authorDomain, cancellationToken).ConfigureAwait (false);
			var evaluation = new AuthorDomainEvaluation (context, authorDomain, walk, MaxDkimSignatures);

			if (evaluation.IsComplete)
				return evaluation.Result;

			evaluation.SetDkimResults (await context.GetDkimResultsAsync (cancellationToken).ConfigureAwait (false));

			foreach (var domain in evaluation.GetIdentifierDomainsToWalk ())
				await GetTreeWalkAsync (context, domain, cancellationToken).ConfigureAwait (false);

			if (evaluation.NeedsExistenceCheck)
				evaluation.SetAuthorDomainExistence (await GetDomainExistenceAsync (authorDomain, cancellationToken).ConfigureAwait (false));

			return evaluation.Complete ();
		}

		// RFC 9989, Section 5.3.1: extract the Author Domain(s) from the From header.
		DmarcValidationResult? GetAuthorDomains (MimeMessage message, List<string> domains)
		{
			int count = 0;

			foreach (var header in message.Headers) {
				if (header.Id == HeaderId.From)
					count++;
			}

			if (count == 0)
				return new DmarcValidationResult (DmarcStatus.PermError, DmarcErrors.NoFromHeader);

			if (count > 1)
				return new DmarcValidationResult (DmarcStatus.PermError, DmarcErrors.MultipleFromHeaders);

			foreach (var mailbox in message.From.Mailboxes) {
				if (string.IsNullOrEmpty (mailbox.Domain))
					continue;

				if (!DnsDomainName.TryNormalize (mailbox.Domain, out var domain))
					return new DmarcValidationResult (DmarcStatus.PermError, DmarcErrors.InvalidAuthorDomain);

				if (domains.Contains (domain))
					continue;

				if (domains.Count == MaxAuthorDomains)
					return new DmarcValidationResult (DmarcStatus.PermError, DmarcErrors.MultipleAuthorDomains);

				domains.Add (domain);
			}

			if (domains.Count == 0)
				return new DmarcValidationResult (DmarcStatus.PermError, DmarcErrors.NoAuthorDomain);

			return null;
		}

		static int GetSeverity (DmarcValidationResult result)
		{
			switch (result.Status) {
			case DmarcStatus.Fail: return 10 + (int) (result.Policy ?? DmarcPolicy.None);
			case DmarcStatus.TempError: return 3;
			case DmarcStatus.PermError: return 2;
			case DmarcStatus.Pass: return 1;
			default: return 0;
			}
		}

		// RFC 9989, Section 11.5: apply the DMARC mechanism to each Author Domain and apply the strictest policy
		// among the checks that fail.
		static DmarcValidationResult Combine (DmarcValidationResult[] results)
		{
			var result = results[0];
			int severity = GetSeverity (result);

			for (int i = 1; i < results.Length; i++) {
				int value = GetSeverity (results[i]);

				if (value > severity) {
					result = results[i];
					severity = value;
				}
			}

			result.AuthorDomainResults = results;

			return result;
		}

		DmarcValidationResult Verify (VerificationContext context, CancellationToken cancellationToken)
		{
			var domains = new List<string> ();
			var error = GetAuthorDomains (context.Message, domains);

			if (error != null) {
				error.SpfResult = context.SpfResult;
				return error;
			}

			if (domains.Count == 1)
				return Evaluate (context, domains[0], cancellationToken);

			var results = new DmarcValidationResult[domains.Count];

			for (int i = 0; i < results.Length; i++)
				results[i] = Evaluate (context, domains[i], cancellationToken);

			return Combine (results);
		}

		async Task<DmarcValidationResult> VerifyAsync (VerificationContext context, CancellationToken cancellationToken)
		{
			var domains = new List<string> ();
			var error = GetAuthorDomains (context.Message, domains);

			if (error != null) {
				error.SpfResult = context.SpfResult;
				return error;
			}

			if (domains.Count == 1)
				return await EvaluateAsync (context, domains[0], cancellationToken).ConfigureAwait (false);

			var results = new DmarcValidationResult[domains.Count];

			for (int i = 0; i < results.Length; i++)
				results[i] = await EvaluateAsync (context, domains[i], cancellationToken).ConfigureAwait (false);

			return Combine (results);
		}

		VerificationContext CreateContext (MimeMessage message, IEnumerable<DkimSignatureValidationResult> dkimResults, SpfCheckResult? spfResult)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (dkimResults is null)
				throw new ArgumentNullException (nameof (dkimResults));

			var results = new List<DkimSignatureValidationResult> (dkimResults);

			for (int i = 0; i < results.Count; i++) {
				if (results[i] is null)
					throw new ArgumentException ("One or more of the DKIM results is null.", nameof (dkimResults));
			}

			return new VerificationContext (DkimVerifier, FormatOptions.Default, message, results.ToArray (), spfResult);
		}

		VerificationContext CreateContext (FormatOptions options, MimeMessage message, SpfCheckResult? spfResult)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (message is null)
				throw new ArgumentNullException (nameof (message));

			return new VerificationContext (DkimVerifier, options, message, null, spfResult);
		}

		/// <summary>
		/// Validate the message using the specified DKIM and SPF results.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message using DKIM results that were obtained separately, such as by calling
		/// <see cref="Cryptography.DkimVerifier.Verify(FormatOptions, MimeMessage, CancellationToken)"/> or from an
		/// upstream MTA.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="message">The message to validate.</param>
		/// <param name="dkimResults">The results of verifying the DKIM signatures of the message.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimResults"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the <paramref name="dkimResults"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DmarcValidationResult Verify (MimeMessage message, IEnumerable<DkimSignatureValidationResult> dkimResults, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			var context = CreateContext (message, dkimResults, spfResult);

			return Verify (context, cancellationToken);
		}

		/// <summary>
		/// Asynchronously validate the message using the specified DKIM and SPF results.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message using DKIM results that were obtained separately, such as by calling
		/// <see cref="Cryptography.DkimVerifier.VerifyAsync(FormatOptions, MimeMessage, CancellationToken)"/> or from an
		/// upstream MTA.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="message">The message to validate.</param>
		/// <param name="dkimResults">The results of verifying the DKIM signatures of the message.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimResults"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the <paramref name="dkimResults"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<DmarcValidationResult> VerifyAsync (MimeMessage message, IEnumerable<DkimSignatureValidationResult> dkimResults, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			var context = CreateContext (message, dkimResults, spfResult);

			return VerifyAsync (context, cancellationToken);
		}

		/// <summary>
		/// Validate the message.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message, using the <see cref="DkimVerifier"/> to verify the DKIM signatures of the
		/// message. The signatures are only verified if a DMARC Policy Record applies to the message.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="options">The formatting options used to verify the DKIM signatures.</param>
		/// <param name="message">The message to validate.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DmarcValidationResult Verify (FormatOptions options, MimeMessage message, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			var context = CreateContext (options, message, spfResult);

			return Verify (context, cancellationToken);
		}

		/// <summary>
		/// Asynchronously validate the message.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message, using the <see cref="DkimVerifier"/> to verify the DKIM signatures of the
		/// message. The signatures are only verified if a DMARC Policy Record applies to the message.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="options">The formatting options used to verify the DKIM signatures.</param>
		/// <param name="message">The message to validate.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<DmarcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			var context = CreateContext (options, message, spfResult);

			return VerifyAsync (context, cancellationToken);
		}

		/// <summary>
		/// Validate the message.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message, using the <see cref="DkimVerifier"/> to verify the DKIM signatures of the
		/// message. The signatures are only verified if a DMARC Policy Record applies to the message.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="message">The message to validate.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DmarcValidationResult Verify (MimeMessage message, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			return Verify (FormatOptions.Default, message, spfResult, cancellationToken);
		}

		/// <summary>
		/// Asynchronously validate the message.
		/// </summary>
		/// <remarks>
		/// <para>Validates the message, using the <see cref="DkimVerifier"/> to verify the DKIM signatures of the
		/// message. The signatures are only verified if a DMARC Policy Record applies to the message.</para>
		/// <para>DNS failures do not throw exceptions; they are reported via the
		/// <see cref="DmarcValidationResult.Status"/> and <see cref="DmarcValidationResult.Errors"/> of the result.</para>
		/// </remarks>
		/// <returns>The result of DMARC validation.</returns>
		/// <param name="message">The message to validate.</param>
		/// <param name="spfResult">The result of the SPF check of the MAIL FROM identity, if available.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<DmarcValidationResult> VerifyAsync (MimeMessage message, SpfCheckResult? spfResult, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (FormatOptions.Default, message, spfResult, cancellationToken);
		}

		enum PolicyQueryStatus
		{
			NotFound,
			Found,
			MultipleRecords,
			TemporaryFailure
		}

		enum DomainExistence
		{
			Exists,
			NonExistent,
			Unknown
		}

		enum Alignment
		{
			NotAligned,
			Aligned,
			Unknown
		}

		sealed class PolicyQueryResult
		{
			public static readonly PolicyQueryResult NotFound = new PolicyQueryResult (PolicyQueryStatus.NotFound, null);
			public static readonly PolicyQueryResult MultipleRecords = new PolicyQueryResult (PolicyQueryStatus.MultipleRecords, null);
			public static readonly PolicyQueryResult TemporaryFailure = new PolicyQueryResult (PolicyQueryStatus.TemporaryFailure, null);

			public PolicyQueryResult (PolicyQueryStatus status, DmarcRecord? record)
			{
				Status = status;
				Record = record;
			}

			public PolicyQueryStatus Status { get; }

			public DmarcRecord? Record { get; }
		}

		sealed class PolicyRecord
		{
			public PolicyRecord (string domain, DmarcRecord record)
			{
				Domain = domain;
				Record = record;
			}

			public string Domain { get; }

			public DmarcRecord Record { get; }
		}

		sealed class TreeWalk
		{
			public TreeWalk (string domain)
			{
				Records = new List<PolicyRecord> ();
				OrganizationalDomain = domain;
				Domain = domain;
			}

			public string Domain { get; }

			// The DMARC Policy Records that were found, from the longest name to the shortest.
			public List<PolicyRecord> Records { get; }

			public string OrganizationalDomain { get; private set; }

			// The walk was stopped by a temporary DNS failure, so the Organizational Domain cannot be determined.
			public bool Incomplete { get; private set; }

			public bool MultipleRecords { get; private set; }

			// Returns false if the walk should stop.
			public bool Add (string domain, PolicyQueryResult result)
			{
				switch (result.Status) {
				case PolicyQueryStatus.TemporaryFailure:
					Incomplete = true;
					return false;
				case PolicyQueryStatus.MultipleRecords:
					MultipleRecords = true;
					return true;
				case PolicyQueryStatus.Found:
					Records.Add (new PolicyRecord (domain, result.Record!));

					// If a single record remains and it contains a "psd=n" or "psd=y" tag, stop.
					return result.Record!.PublicSuffixDomain == DmarcPublicSuffixDomain.Unspecified;
				default:
					return true;
				}
			}

			// RFC 9989, Section 4.10.2
			public void SelectOrganizationalDomain ()
			{
				for (int i = 0; i < Records.Count; i++) {
					var record = Records[i];

					switch (record.Record.PublicSuffixDomain) {
					case DmarcPublicSuffixDomain.No:
						OrganizationalDomain = record.Domain;
						return;
					case DmarcPublicSuffixDomain.Yes:
						if (record.Domain != Domain) {
							// The Organizational Domain is the domain one label below the PSD.
							OrganizationalDomain = GetLastLabels (Domain, CountLabels (record.Domain) + 1);
							return;
						}
						break;
					}
				}

				// Otherwise, select the record found at the name with the fewest number of labels. If no records were
				// found, the initial target domain is the Organizational Domain.
				if (Records.Count > 0)
					OrganizationalDomain = Records[Records.Count - 1].Domain;
			}
		}

		sealed class VerificationContext
		{
			readonly DkimVerifier dkimVerifier;
			readonly FormatOptions options;
			DkimSignatureValidationResult[]? dkimResults;

			public VerificationContext (DkimVerifier dkimVerifier, FormatOptions options, MimeMessage message, DkimSignatureValidationResult[]? dkimResults, SpfCheckResult? spfResult)
			{
				PolicyQueries = new Dictionary<string, PolicyQueryResult> (StringComparer.Ordinal);
				TreeWalks = new Dictionary<string, TreeWalk> (StringComparer.Ordinal);
				this.dkimVerifier = dkimVerifier;
				this.dkimResults = dkimResults;
				this.options = options;
				SpfResult = spfResult;
				Message = message;
			}

			public MimeMessage Message { get; }

			public SpfCheckResult? SpfResult { get; }

			public Dictionary<string, PolicyQueryResult> PolicyQueries { get; }

			public Dictionary<string, TreeWalk> TreeWalks { get; }

			// The DKIM results, if they were provided by the caller or have already been verified.
			public DkimSignatureValidationResult[]? DkimResults {
				get { return dkimResults; }
			}

			public DkimSignatureValidationResult[] GetDkimResults (CancellationToken cancellationToken)
			{
				dkimResults ??= dkimVerifier.Verify (options, Message, cancellationToken);

				return dkimResults;
			}

			public async Task<DkimSignatureValidationResult[]> GetDkimResultsAsync (CancellationToken cancellationToken)
			{
				dkimResults ??= await dkimVerifier.VerifyAsync (options, Message, cancellationToken).ConfigureAwait (false);

				return dkimResults;
			}
		}

		// The DNS-independent logic for evaluating a single Author Domain. The DNS queries are made by the caller
		// (synchronously or asynchronously) in between the steps.
		sealed class AuthorDomainEvaluation
		{
			readonly Dictionary<string, TreeWalk> treeWalks;
			readonly string? organizationalDomain;
			readonly string authorDomain;
			readonly int maxDkimSignatures;
			DkimSignatureValidationResult[] dkimResults;
			DomainExistence existence;
			DmarcPolicy nonExistentSubdomainPolicy;
			DmarcPolicy policy;

			public AuthorDomainEvaluation (VerificationContext context, string authorDomain, TreeWalk walk, int maxDkimSignatures)
			{
				dkimResults = context.DkimResults ?? Array.Empty<DkimSignatureValidationResult> ();
				this.maxDkimSignatures = maxDkimSignatures;
				treeWalks = context.TreeWalks;
				this.authorDomain = authorDomain;

				Result = new DmarcValidationResult (DmarcStatus.None) {
					SpfResult = context.SpfResult,
					AuthorDomain = authorDomain,
					DkimResults = dkimResults
				};

				if (walk.MultipleRecords)
					Result.Errors |= DmarcErrors.MultipleRecords;

				if (walk.Incomplete)
					Result.Errors |= DmarcErrors.DnsTemporaryFailure;
				else
					organizationalDomain = walk.OrganizationalDomain;

				IsComplete = !DiscoverPolicy (walk);
			}

			public DmarcValidationResult Result { get; }

			public bool IsComplete { get; }

			public bool NeedsExistenceCheck { get; private set; }

			DmarcRecord Record {
				get { return Result.Record!; }
			}

			// RFC 9989, Section 4.10.1
			bool DiscoverPolicy (TreeWalk walk)
			{
				var records = walk.Records;
				PolicyRecord? publicSuffixRecord = null;
				PolicyRecord? record = null;

				if (records.Count > 0 && records[0].Domain == authorDomain) {
					Result.PolicyDomainSource = DmarcPolicyDomainSource.AuthorDomain;
					record = records[0];
				} else if (walk.Incomplete) {
					// Without the full Tree Walk, we cannot know which record applies.
					Result.Status = DmarcStatus.TempError;
					return false;
				} else {
					for (int i = 0; i < records.Count; i++) {
						if (records[i].Domain == organizationalDomain) {
							Result.PolicyDomainSource = DmarcPolicyDomainSource.OrganizationalDomain;
							record = records[i];
							break;
						}

						if (records[i].Record.PublicSuffixDomain == DmarcPublicSuffixDomain.Yes)
							publicSuffixRecord = records[i];
					}

					if (record is null && publicSuffixRecord != null) {
						Result.PolicyDomainSource = DmarcPolicyDomainSource.PublicSuffixDomain;
						record = publicSuffixRecord;
					}
				}

				if (record is null) {
					// No DMARC Policy Record: the DMARC mechanism does not apply.
					return false;
				}

				Result.OrganizationalDomain = organizationalDomain;
				Result.IsTesting = record.Record.Testing;
				Result.PolicyDomain = record.Domain;
				Result.Record = record.Record;

				if (!record.Record.Policy.HasValue || (record.Record.Errors & InvalidPolicyErrors) != 0) {
					// The record does not contain a valid "p" tag or contains an invalid "sp" or "np" tag. If the record
					// has at least one valid rua URI, act as if it were "p=none". Otherwise, apply no DMARC processing
					// to the message.
					Result.Errors |= DmarcErrors.InvalidPolicyRecord;

					if (record.Record.AggregateReportUris.Count == 0)
						return false;

					policy = DmarcPolicy.None;
					return true;
				}

				policy = record.Record.Policy.Value;

				if (Result.PolicyDomainSource != DmarcPolicyDomainSource.AuthorDomain) {
					// The Author Domain is a subdomain of the Policy Domain, so "sp" applies if the Author Domain exists
					// and "np" applies if it does not. Each falls back to "p" ("np" falls back to "sp" first).
					policy = record.Record.SubdomainPolicy ?? policy;

					if (record.Record.NonExistentSubdomainPolicy is DmarcPolicy np && np != policy) {
						nonExistentSubdomainPolicy = np;
						NeedsExistenceCheck = true;
					}
				}

				return true;
			}

			public void SetDkimResults (DkimSignatureValidationResult[] results)
			{
				Result.DkimResults = results;
				dkimResults = results;
			}

			public void SetAuthorDomainExistence (DomainExistence value)
			{
				existence = value;
			}

			// Relaxed alignment of an identifier that is a subdomain of the Author Domain's Organizational Domain
			// requires a Tree Walk to determine the identifier's Organizational Domain.
			bool NeedsTreeWalk (string domain, DmarcAlignmentMode mode)
			{
				return mode == DmarcAlignmentMode.Relaxed && organizationalDomain != null && domain != authorDomain && IsSubdomainOf (domain, organizationalDomain);
			}

			int DkimResultCount {
				get { return Math.Min (dkimResults.Length, maxDkimSignatures); }
			}

			static bool IsRelevant (DkimSignatureStatus status)
			{
				return status == DkimSignatureStatus.Pass || status == DkimSignatureStatus.TempError;
			}

			static bool IsRelevant (SpfStatus status)
			{
				return status == SpfStatus.Pass || status == SpfStatus.TempError;
			}

			public List<string> GetIdentifierDomainsToWalk ()
			{
				var domains = new List<string> ();
				string? domain;

				for (int i = 0; i < DkimResultCount; i++) {
					var dkim = dkimResults[i];

					if (IsRelevant (dkim.Status) && DnsDomainName.TryNormalize (dkim.Domain, out domain) && NeedsTreeWalk (domain, Record.DkimAlignment) && !domains.Contains (domain))
						domains.Add (domain);
				}

				var spf = Result.SpfResult;

				if (spf != null && IsRelevant (spf.Status) && DnsDomainName.TryNormalize (spf.Domain, out domain) && NeedsTreeWalk (domain, Record.SpfAlignment) && !domains.Contains (domain))
					domains.Add (domain);

				return domains;
			}

			// RFC 9989, Sections 3.2.10 and 4.10.2
			Alignment GetAlignment (string? identifier, DmarcAlignmentMode mode)
			{
				if (!DnsDomainName.TryNormalize (identifier, out var domain))
					return Alignment.NotAligned;

				if (domain == authorDomain)
					return Alignment.Aligned;

				if (mode == DmarcAlignmentMode.Strict)
					return Alignment.NotAligned;

				if (organizationalDomain is null)
					return Alignment.Unknown;

				if (domain == organizationalDomain)
					return Alignment.Aligned;

				// The Organizational Domain of the identifier is the identifier itself or one of its parent domains, so
				// it can only match if the identifier is a subdomain of the Author Domain's Organizational Domain.
				if (!IsSubdomainOf (domain, organizationalDomain))
					return Alignment.NotAligned;

				var walk = treeWalks[domain];

				if (walk.Incomplete)
					return Alignment.Unknown;

				return walk.OrganizationalDomain == organizationalDomain ? Alignment.Aligned : Alignment.NotAligned;
			}

			public DmarcValidationResult Complete ()
			{
				var errors = DmarcErrors.None;
				var spf = Result.SpfResult;
				Alignment alignment;

				for (int i = 0; i < DkimResultCount; i++) {
					var dkim = dkimResults[i];

					if (!IsRelevant (dkim.Status))
						continue;

					alignment = GetAlignment (dkim.Domain, Record.DkimAlignment);

					if (dkim.Status == DkimSignatureStatus.Pass) {
						if (alignment == Alignment.Aligned) {
							if (!Result.DkimAligned) {
								Result.AlignedDkimResult = dkim;
								Result.DkimAligned = true;
							}
						} else if (alignment == Alignment.Unknown) {
							errors |= DmarcErrors.DnsTemporaryFailure;
						}
					} else if (alignment != Alignment.NotAligned) {
						errors |= DmarcErrors.DkimTemporaryError;
					}
				}

				if (spf != null && IsRelevant (spf.Status)) {
					alignment = GetAlignment (spf.Domain, Record.SpfAlignment);

					if (spf.Status == SpfStatus.Pass) {
						if (alignment == Alignment.Aligned)
							Result.SpfAligned = true;
						else if (alignment == Alignment.Unknown)
							errors |= DmarcErrors.DnsTemporaryFailure;
					} else if (alignment != Alignment.NotAligned) {
						errors |= DmarcErrors.SpfTemporaryError;
					}
				}

				if (existence == DomainExistence.NonExistent)
					policy = nonExistentSubdomainPolicy;
				else if (existence == DomainExistence.Unknown)
					errors |= DmarcErrors.DnsTemporaryFailure;

				if (Result.DkimAligned || Result.SpfAligned) {
					Result.Status = DmarcStatus.Pass;
				} else if (errors != DmarcErrors.None) {
					// If DNS queries required for DMARC validation did not complete or a potentially aligned
					// identifier had a temporary error, the message can neither pass nor fail (RFC 9989, Section 5.3.6).
					Result.Status = DmarcStatus.TempError;
					Result.Errors |= errors;
				} else {
					Result.Status = DmarcStatus.Fail;
				}

				Result.Policy = Result.IsTesting ? ApplyTestMode (policy) : policy;
				Result.RequestedPolicy = policy;

				return Result;
			}
		}
	}
}
