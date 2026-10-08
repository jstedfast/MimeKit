//
// DmarcValidationResult.cs
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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The result of DMARC validation.
	/// </summary>
	/// <remarks>
	/// <para>The result of DMARC validation, as returned by <see cref="DmarcVerifier"/>.</para>
	/// <para>The result does not decide what to do with the message. The final disposition of a message is a
	/// matter of local policy (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-5.4">RFC 9989, Section 5.4</a>):
	/// a policy of <c>reject</c> should not be the sole reason to reject a message
	/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-7.4">RFC 9989, Section 7.4</a>), and local
	/// overrides such as trusted forwarders or ARC are up to the caller.</para>
	/// <para>The result also carries the information needed to build DMARC aggregate and failure reports.</para>
	/// </remarks>
	public sealed class DmarcValidationResult
	{
		static readonly DkimSignatureValidationResult[] NoDkimResults = Array.Empty<DkimSignatureValidationResult> ();

		internal DmarcValidationResult (DmarcStatus status, DmarcErrors errors = DmarcErrors.None)
		{
			DkimResults = NoDkimResults;
			Status = status;
			Errors = errors;
		}

		/// <summary>
		/// Get the DMARC result.
		/// </summary>
		/// <remarks>
		/// Gets the DMARC result.
		/// </remarks>
		/// <value>The DMARC result.</value>
		public DmarcStatus Status {
			get; internal set;
		}

		/// <summary>
		/// Get the problems that were encountered during validation.
		/// </summary>
		/// <remarks>
		/// Gets the problems that were encountered during validation.
		/// </remarks>
		/// <value>The problems that were encountered.</value>
		public DmarcErrors Errors {
			get; internal set;
		}

		/// <summary>
		/// Get the Author Domain.
		/// </summary>
		/// <remarks>
		/// Gets the domain of the author, extracted from the From header. Internationalized domain names are
		/// converted to A-labels (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-5.3.1">RFC 9989, Section 5.3.1</a>).
		/// </remarks>
		/// <value>The Author Domain or <see langword="null"/> if it could not be determined.</value>
		public string? AuthorDomain {
			get; internal set;
		}

		/// <summary>
		/// Get the DMARC Policy Domain.
		/// </summary>
		/// <remarks>
		/// Gets the domain at which the applicable DMARC Policy Record was discovered.
		/// </remarks>
		/// <value>The DMARC Policy Domain or <see langword="null"/> if no DMARC Policy Record was discovered.</value>
		public string? PolicyDomain {
			get; internal set;
		}

		/// <summary>
		/// Get the location at which the applicable DMARC Policy Record was discovered.
		/// </summary>
		/// <remarks>
		/// Gets the location at which the applicable DMARC Policy Record was discovered.
		/// </remarks>
		/// <value>The location or <see langword="null"/> if no DMARC Policy Record was discovered.</value>
		public DmarcPolicyDomainSource? PolicyDomainSource {
			get; internal set;
		}

		/// <summary>
		/// Get the Organizational Domain of the Author Domain.
		/// </summary>
		/// <remarks>
		/// Gets the Organizational Domain of the Author Domain, as determined by the DNS Tree Walk
		/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.10.2">RFC 9989, Section 4.10.2</a>).
		/// </remarks>
		/// <value>The Organizational Domain or <see langword="null"/> if no DMARC Policy Record was discovered.</value>
		public string? OrganizationalDomain {
			get; internal set;
		}

		/// <summary>
		/// Get the applicable DMARC Policy Record.
		/// </summary>
		/// <remarks>
		/// Gets the applicable DMARC Policy Record (the <c>policy_published</c> element of an aggregate report).
		/// </remarks>
		/// <value>The DMARC Policy Record or <see langword="null"/> if no DMARC Policy Record was discovered.</value>
		public DmarcRecord? Record {
			get; internal set;
		}

		/// <summary>
		/// Get the Domain Owner Assessment Policy requested for the Author Domain.
		/// </summary>
		/// <remarks>
		/// <para>Gets the policy that the Domain Owner requested for the Author Domain, selected from the
		/// <c>p</c>, <c>sp</c> or <c>np</c> tag of the DMARC Policy Record
		/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.10.1">RFC 9989, Section 4.10.1</a>), before
		/// the test mode (<c>t=y</c>) is applied.</para>
		/// </remarks>
		/// <value>The requested policy or <see langword="null"/> if the DMARC mechanism does not apply.</value>
		public DmarcPolicy? RequestedPolicy {
			get; internal set;
		}

		/// <summary>
		/// Get the Domain Owner Assessment Policy to apply to the message if it fails DMARC validation.
		/// </summary>
		/// <remarks>
		/// <para>Gets the <see cref="RequestedPolicy"/> after the test mode (<c>t=y</c>) has been applied. In test mode, the
		/// policy is lowered by one level: <c>reject</c> becomes <c>quarantine</c> and <c>quarantine</c> becomes
		/// <c>none</c>.</para>
		/// <para>This is the value of the <c>policy.dmarc</c> property of the Authentication-Results header.</para>
		/// </remarks>
		/// <value>The policy or <see langword="null"/> if the DMARC mechanism does not apply.</value>
		public DmarcPolicy? Policy {
			get; internal set;
		}

		/// <summary>
		/// Get whether the Domain Owner is testing its policy.
		/// </summary>
		/// <remarks>
		/// Gets whether the DMARC Policy Record has a <c>t=y</c> tag. Receivers that override the requested policy
		/// because of the test mode should report the <c>policy_test_mode</c> override reason.
		/// </remarks>
		/// <value><see langword="true"/> if the Domain Owner is testing its policy; otherwise, <see langword="false"/>.</value>
		public bool IsTesting {
			get; internal set;
		}

		/// <summary>
		/// Get whether a DKIM-Authenticated Identifier is aligned with the Author Domain.
		/// </summary>
		/// <remarks>
		/// Gets whether a DKIM signature that passed verification has a signing domain that is aligned with the
		/// Author Domain.
		/// </remarks>
		/// <value><see langword="true"/> if a DKIM-Authenticated Identifier is aligned; otherwise, <see langword="false"/>.</value>
		public bool DkimAligned {
			get; internal set;
		}

		/// <summary>
		/// Get whether the SPF-Authenticated Identifier is aligned with the Author Domain.
		/// </summary>
		/// <remarks>
		/// Gets whether the SPF check passed for a domain that is aligned with the Author Domain.
		/// </remarks>
		/// <value><see langword="true"/> if the SPF-Authenticated Identifier is aligned; otherwise, <see langword="false"/>.</value>
		public bool SpfAligned {
			get; internal set;
		}

		/// <summary>
		/// Get the DKIM results that were evaluated.
		/// </summary>
		/// <remarks>
		/// Gets the DKIM results that were evaluated. When the <see cref="DmarcVerifier"/> verifies the DKIM
		/// signatures itself, this is empty if DMARC validation finished before the signatures had to be verified.
		/// </remarks>
		/// <value>The DKIM results.</value>
		public IReadOnlyList<DkimSignatureValidationResult> DkimResults {
			get; internal set;
		}

		/// <summary>
		/// Get the DKIM result that is aligned with the Author Domain.
		/// </summary>
		/// <remarks>
		/// Gets the first DKIM result that passed verification and whose signing domain is aligned with the Author Domain.
		/// </remarks>
		/// <value>The aligned DKIM result or <see langword="null"/> if no DKIM result is aligned.</value>
		public DkimSignatureValidationResult? AlignedDkimResult {
			get; internal set;
		}

		/// <summary>
		/// Get the SPF result that was evaluated.
		/// </summary>
		/// <remarks>
		/// Gets the SPF result that was evaluated.
		/// </remarks>
		/// <value>The SPF result or <see langword="null"/> if none was provided.</value>
		public SpfCheckResult? SpfResult {
			get; internal set;
		}

		/// <summary>
		/// Get the results for each Author Domain.
		/// </summary>
		/// <remarks>
		/// <para>When <see cref="DmarcVerifier.MaxAuthorDomains"/> is greater than <c>1</c> and the From header contains
		/// more than one Author Domain, each domain is evaluated separately. In that case, this list contains the result
		/// for each Author Domain (including this one), and this result is the most significant of them: the failure
		/// with the strictest policy, as recommended by
		/// <a href="https://www.rfc-editor.org/rfc/rfc9989#section-11.5">RFC 9989, Section 11.5</a>.</para>
		/// </remarks>
		/// <value>The results for each Author Domain or <see langword="null"/> if there was only one Author Domain.</value>
		public IReadOnlyList<DmarcValidationResult>? AuthorDomainResults {
			get; internal set;
		}

		static string GetStatusValue (DmarcStatus status)
		{
			switch (status) {
			case DmarcStatus.Pass: return "pass";
			case DmarcStatus.Fail: return "fail";
			case DmarcStatus.TempError: return "temperror";
			case DmarcStatus.PermError: return "permerror";
			default: return "none";
			}
		}

		static string GetPolicyValue (DmarcPolicy policy)
		{
			switch (policy) {
			case DmarcPolicy.Reject: return "reject";
			case DmarcPolicy.Quarantine: return "quarantine";
			default: return "none";
			}
		}

		/// <summary>
		/// Create an <see cref="AuthenticationMethodResult"/> for the Authentication-Results header.
		/// </summary>
		/// <remarks>
		/// <para>Creates an <see cref="AuthenticationMethodResult"/> for the <c>dmarc</c> method, suitable for adding
		/// to an Authentication-Results header (<a href="https://www.rfc-editor.org/rfc/rfc8601">RFC 8601</a>).</para>
		/// <para>The <c>header.from</c> property is set to the Author Domain. When the message fails DMARC validation,
		/// the <c>policy.dmarc</c> property is set to the <see cref="Policy"/>
		/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-9.1">RFC 9989, Section 9.1</a>).</para>
		/// </remarks>
		/// <returns>The authentication method result.</returns>
		public AuthenticationMethodResult ToAuthenticationMethodResult ()
		{
			var result = new AuthenticationMethodResult ("dmarc", GetStatusValue (Status));

			if (AuthorDomain != null)
				result.Properties.Add (new AuthenticationMethodProperty ("header", "from", AuthorDomain));

			if (Status == DmarcStatus.Fail && Policy.HasValue)
				result.Properties.Add (new AuthenticationMethodProperty ("policy", "dmarc", GetPolicyValue (Policy.Value)));

			return result;
		}
	}
}
