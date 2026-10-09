//
// DmarcVerifierTests.cs
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DmarcVerifierTests
	{
		static readonly DkimSignatureValidationResult[] NoDkim = Array.Empty<DkimSignatureValidationResult> ();

		static MimeMessage CreateMessage (params string[] from)
		{
			var message = new MimeMessage ();

			foreach (var address in from)
				message.From.Add (MailboxAddress.Parse (address));

			message.To.Add (new MailboxAddress ("", "recipient@example.org"));
			message.Subject = "DMARC test";
			message.Date = new DateTimeOffset (2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
			message.Body = new TextPart ("plain") { Text = "This is the body." };

			return message;
		}

		static DkimSignatureValidationResult Dkim (DkimSignatureStatus status, string domain, string selector = "selector")
		{
			return new DkimSignatureValidationResult (status, domain, selector);
		}

		static void AssertSameResult (DmarcValidationResult expected, DmarcValidationResult actual)
		{
			Assert.That (actual.Status, Is.EqualTo (expected.Status), "Status");
			Assert.That (actual.Errors, Is.EqualTo (expected.Errors), "Errors");
			Assert.That (actual.AuthorDomain, Is.EqualTo (expected.AuthorDomain), "AuthorDomain");
			Assert.That (actual.PolicyDomain, Is.EqualTo (expected.PolicyDomain), "PolicyDomain");
			Assert.That (actual.PolicyDomainSource, Is.EqualTo (expected.PolicyDomainSource), "PolicyDomainSource");
			Assert.That (actual.OrganizationalDomain, Is.EqualTo (expected.OrganizationalDomain), "OrganizationalDomain");
			Assert.That (actual.RequestedPolicy, Is.EqualTo (expected.RequestedPolicy), "RequestedPolicy");
			Assert.That (actual.Policy, Is.EqualTo (expected.Policy), "Policy");
			Assert.That (actual.IsTesting, Is.EqualTo (expected.IsTesting), "IsTesting");
			Assert.That (actual.DkimAligned, Is.EqualTo (expected.DkimAligned), "DkimAligned");
			Assert.That (actual.SpfAligned, Is.EqualTo (expected.SpfAligned), "SpfAligned");
			Assert.That (actual.DkimResults.Count, Is.EqualTo (expected.DkimResults.Count), "DkimResults");
			Assert.That (actual.AuthorDomainResults?.Count, Is.EqualTo (expected.AuthorDomainResults?.Count), "AuthorDomainResults");
		}

		// Verifies the message synchronously and asynchronously, asserting that both produce the same result and make
		// the same DNS queries. After returning, resolver.Queries contains the queries made by a single verification.
		static async Task<DmarcValidationResult> VerifyAsync (MockDnsResolver resolver, MimeMessage message, IEnumerable<DkimSignatureValidationResult> dkimResults, SpfCheckResult spfResult, Action<DmarcVerifier> configure = null)
		{
			var verifier = new DmarcVerifier (resolver);

			configure?.Invoke (verifier);

			resolver.Queries.Clear ();
			var result = verifier.Verify (message, dkimResults, spfResult);
			var queries = resolver.Queries.ToArray ();

			resolver.Queries.Clear ();
			var asyncResult = await verifier.VerifyAsync (message, dkimResults, spfResult);

			AssertSameResult (result, asyncResult);
			Assert.That (resolver.Queries, Is.EqualTo (queries), "Queries");

			return result;
		}

		static Task<DmarcValidationResult> VerifyAsync (MockDnsResolver resolver, string from, params DkimSignatureValidationResult[] dkimResults)
		{
			return VerifyAsync (resolver, from, null, dkimResults);
		}

		static async Task<DmarcValidationResult> VerifyAsync (MockDnsResolver resolver, string from, SpfCheckResult spfResult, params DkimSignatureValidationResult[] dkimResults)
		{
			using var message = CreateMessage (from);

			return await VerifyAsync (resolver, message, dkimResults, spfResult);
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var resolver = new MockDnsResolver ();
			var verifier = new DmarcVerifier (resolver);
			using var message = CreateMessage ("user@example.com");
			var nullElement = new DkimSignatureValidationResult[] { null };

			Assert.Throws<ArgumentNullException> (() => new DmarcVerifier (null));
			Assert.Throws<ArgumentNullException> (() => new DmarcVerifier (null, new DkimVerifier (resolver)));
			Assert.Throws<ArgumentNullException> (() => new DmarcVerifier (resolver, null));

			Assert.Throws<ArgumentOutOfRangeException> (() => verifier.MaxAuthorDomains = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => verifier.MaxDkimSignatures = 0);

			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null, NoDkim, null));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (message, (IEnumerable<DkimSignatureValidationResult>) null, null));
			Assert.Throws<ArgumentException> (() => verifier.Verify (message, nullElement, null));
			Assert.ThrowsAsync<ArgumentNullException> (() => verifier.VerifyAsync (null, NoDkim, null));
			Assert.ThrowsAsync<ArgumentNullException> (() => verifier.VerifyAsync (message, (IEnumerable<DkimSignatureValidationResult>) null, null));
			Assert.ThrowsAsync<ArgumentException> (() => verifier.VerifyAsync (message, nullElement, null));

			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null, message, null));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (FormatOptions.Default, null, null));
			Assert.ThrowsAsync<ArgumentNullException> (() => verifier.VerifyAsync (null, message, null));
			Assert.ThrowsAsync<ArgumentNullException> (() => verifier.VerifyAsync (FormatOptions.Default, null, null));

			Assert.Throws<ArgumentNullException> (() => verifier.Verify ((MimeMessage) null, (SpfCheckResult) null));
			Assert.ThrowsAsync<ArgumentNullException> (() => verifier.VerifyAsync ((MimeMessage) null, (SpfCheckResult) null));

			Assert.Throws<ArgumentNullException> (() => new SpfCheckResult (SpfStatus.Pass, null));
			Assert.Throws<ArgumentOutOfRangeException> (() => new SpfCheckResult ((SpfStatus) 500, "example.com"));

			Assert.Throws<ArgumentNullException> (() => new DkimSignatureValidationResult (DkimSignatureStatus.Pass, null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSignatureValidationResult (DkimSignatureStatus.Pass, "example.com", null));
			Assert.Throws<ArgumentOutOfRangeException> (() => new DkimSignatureValidationResult ((DkimSignatureStatus) 500, "example.com", "selector"));

			// Argument validation happens before any DNS queries are made.
			Assert.That (resolver.Queries, Is.Empty);
		}

		[Test]
		public void TestDefaults ()
		{
			var resolver = new MockDnsResolver ();
			var verifier = new DmarcVerifier (resolver);

			Assert.That (verifier.DnsResolver, Is.SameAs (resolver));
			Assert.That (verifier.DkimVerifier, Is.Not.Null);
			Assert.That (verifier.MaxAuthorDomains, Is.EqualTo (1));
			Assert.That (verifier.MaxDkimSignatures, Is.EqualTo (10));

			var dkimVerifier = new DkimVerifier (resolver);
			verifier = new DmarcVerifier (resolver, dkimVerifier) {
				MaxAuthorDomains = 3,
				MaxDkimSignatures = 2
			};

			Assert.That (verifier.DkimVerifier, Is.SameAs (dkimVerifier));
			Assert.That (verifier.MaxAuthorDomains, Is.EqualTo (3));
			Assert.That (verifier.MaxDkimSignatures, Is.EqualTo (2));
		}

		[Test]
		public void TestSpfCheckResult ()
		{
			var spf = new SpfCheckResult (SpfStatus.SoftFail, "example.com", "reason");

			Assert.That (spf.Status, Is.EqualTo (SpfStatus.SoftFail));
			Assert.That (spf.Domain, Is.EqualTo ("example.com"));
			Assert.That (spf.Reason, Is.EqualTo ("reason"));

			spf = new SpfCheckResult (SpfStatus.Pass, "example.com");
			Assert.That (spf.Reason, Is.Null);
		}

		[Test]
		public void TestDkimSignatureValidationResultConstructor ()
		{
			var dkim = new DkimSignatureValidationResult (DkimSignatureStatus.Pass, "example.com", "selector");

			Assert.That (dkim.Status, Is.EqualTo (DkimSignatureStatus.Pass));
			Assert.That (dkim.Domain, Is.EqualTo ("example.com"));
			Assert.That (dkim.Selector, Is.EqualTo ("selector"));
			Assert.That (dkim.Header, Is.Null);

			var header = new Header (HeaderId.DkimSignature, "v=1; d=example.com; s=selector");
			dkim = new DkimSignatureValidationResult (DkimSignatureStatus.Fail, "example.com", "selector", header);

			Assert.That (dkim.Status, Is.EqualTo (DkimSignatureStatus.Fail));
			Assert.That (dkim.Header, Is.SameAs (header));

			dkim.SignatureAlgorithm = DkimSignatureAlgorithm.Ed25519Sha256;
			Assert.That (dkim.ToAuthenticationMethodResult ().Properties[2].Value, Is.EqualTo ("ed25519-sha256"));
		}

		[Test]
		public async Task TestNoFromHeader ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ("user@example.com");

			message.Headers.RemoveAll (HeaderId.From);

			var result = await VerifyAsync (resolver, message, NoDkim, new SpfCheckResult (SpfStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.NoFromHeader));
			Assert.That (result.AuthorDomain, Is.Null);
			Assert.That (result.SpfResult, Is.Not.Null);
			Assert.That (resolver.Queries, Is.Empty);

			var authResult = result.ToAuthenticationMethodResult ();
			Assert.That (authResult.Method, Is.EqualTo ("dmarc"));
			Assert.That (authResult.Result, Is.EqualTo ("permerror"));
			Assert.That (authResult.Properties, Is.Empty);
		}

		[Test]
		public async Task TestMultipleFromHeaders ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ("user@example.com");

			message.Headers.Add (HeaderId.From, "other@example.com");

			var result = await VerifyAsync (resolver, message, NoDkim, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.MultipleFromHeaders));
			Assert.That (resolver.Queries, Is.Empty);
		}

		[Test]
		public async Task TestNoAuthorDomain ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ();

			message.From.Add (new MailboxAddress ("", "user"));

			var result = await VerifyAsync (resolver, message, NoDkim, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.NoAuthorDomain));
			Assert.That (resolver.Queries, Is.Empty);

			// An empty group has no mailboxes.
			message.From.Clear ();
			message.From.Add (new GroupAddress ("undisclosed-recipients"));

			result = await VerifyAsync (resolver, message, NoDkim, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.NoAuthorDomain));
		}

		[Test]
		public async Task TestInvalidAuthorDomain ()
		{
			var resolver = new MockDnsResolver ();

			var result = await VerifyAsync (resolver, "user@[127.0.0.1]");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.InvalidAuthorDomain));
			Assert.That (resolver.Queries, Is.Empty);
		}

		[Test]
		public async Task TestGroupedAuthor ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			message.From.Add (new GroupAddress ("Group", new InternetAddress[] { new MailboxAddress ("", "user@Example.COM") }));

			var result = await VerifyAsync (resolver, message, new[] { Dkim (DkimSignatureStatus.Pass, "example.com") }, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.AuthorDomain, Is.EqualTo ("example.com"));
		}

		[Test]
		public async Task TestInternationalizedAuthorDomain ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.xn--bcher-kva.example", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@bücher.example", Dkim (DkimSignatureStatus.Pass, "xn--bcher-kva.example"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.AuthorDomain, Is.EqualTo ("xn--bcher-kva.example"));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.xn--bcher-kva.example", "_dmarc.example" }));
		}

		[Test]
		public async Task TestMultipleAuthorDomains ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ("a@example.com", "b@example.net");

			var result = await VerifyAsync (resolver, message, NoDkim, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.MultipleAuthorDomains));
			Assert.That (resolver.Queries, Is.Empty);

			// Multiple mailboxes with the same domain (ignoring case) have a single Author Domain.
			using var sameDomain = CreateMessage ("a@example.com", "b@EXAMPLE.com");

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			result = await VerifyAsync (resolver, sameDomain, new[] { Dkim (DkimSignatureStatus.Pass, "example.com") }, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.AuthorDomainResults, Is.Null);
		}

		[Test]
		public async Task TestMultipleAuthorDomainsEvaluated ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ("a@example.com", "b@example.net", "c@example.org");
			var dkim = new[] { Dkim (DkimSignatureStatus.Pass, "example.com") };

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			resolver.Add ("_dmarc.example.net", "v=DMARC1; p=quarantine");
			resolver.Add ("_dmarc.example.org", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, message, dkim, null, verifier => verifier.MaxAuthorDomains = 2);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.PermError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.MultipleAuthorDomains));

			result = await VerifyAsync (resolver, message, dkim, null, verifier => verifier.MaxAuthorDomains = 3);

			// The failure with the strictest policy is the primary result.
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.AuthorDomain, Is.EqualTo ("example.org"));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (result.AuthorDomainResults, Has.Count.EqualTo (3));
			Assert.That (result.AuthorDomainResults[0].AuthorDomain, Is.EqualTo ("example.com"));
			Assert.That (result.AuthorDomainResults[0].Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.AuthorDomainResults[1].AuthorDomain, Is.EqualTo ("example.net"));
			Assert.That (result.AuthorDomainResults[1].Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.AuthorDomainResults[2], Is.SameAs (result));

			// The shared ".com", ".net" and ".org" queries are not repeated.
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com", "_dmarc.example.net", "_dmarc.net", "_dmarc.example.org", "_dmarc.org" }));

			// A temporary error is more significant than a pass.
			resolver.AddFailure ("_dmarc.example.net", DnsQueryStatus.TemporaryFailure);
			using var twoDomains = CreateMessage ("a@example.com", "b@example.net");

			result = await VerifyAsync (resolver, twoDomains, dkim, null, verifier => verifier.MaxAuthorDomains = 2);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.AuthorDomain, Is.EqualTo ("example.net"));
		}

		[Test]
		public async Task TestMaximumLengthAuthorDomainSkipsUnqueryablePolicyRecordName ()
		{
			var resolver = new MockDnsResolver ();
			var label63 = new string ('a', 63);
			var label61 = new string ('a', 61);
			var domain = string.Join (".", label63, label63, label63, label61);

			var result = await VerifyAsync (resolver, "user@" + domain);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
			Assert.That (result.AuthorDomain, Is.EqualTo (domain));

			// "_dmarc." + the 253-character Author Domain exceeds the maximum DNS name length, so the Tree Walk
			// skips that query and continues with the parent domains.
			Assert.That (resolver.Queries, Is.EqualTo (new[] {
				"_dmarc." + string.Join (".", label63, label63, label61),
				"_dmarc." + string.Join (".", label63, label61),
				"_dmarc." + label61
			}));
		}

		[Test]
		public async Task TestNoPolicyRecord ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=spf1 -all");

			var result = await VerifyAsync (resolver, "user@mail.example.com", Dkim (DkimSignatureStatus.Fail, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
			Assert.That (result.AuthorDomain, Is.EqualTo ("mail.example.com"));
			Assert.That (result.Record, Is.Null);
			Assert.That (result.PolicyDomain, Is.Null);
			Assert.That (result.PolicyDomainSource, Is.Null);
			Assert.That (result.OrganizationalDomain, Is.Null);
			Assert.That (result.Policy, Is.Null);
			Assert.That (result.RequestedPolicy, Is.Null);
			Assert.That (result.DkimResults, Has.Count.EqualTo (1));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com" }));

			var authResult = result.ToAuthenticationMethodResult ();
			Assert.That (authResult.Result, Is.EqualTo ("none"));
			Assert.That (authResult.Properties, Has.Count.EqualTo (1));
		}

		[Test]
		public async Task TestDkimAligned ()
		{
			var resolver = new MockDnsResolver ();
			var aligned = Dkim (DkimSignatureStatus.Pass, "EXAMPLE.com", "second");
			var dkim = new[] {
				Dkim (DkimSignatureStatus.Fail, "example.com", "failed"),
				Dkim (DkimSignatureStatus.Pass, "example.net"),
				aligned,
				Dkim (DkimSignatureStatus.Pass, "example.com", "third")
			};

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject; rua=mailto:dmarc@example.com");

			var result = await VerifyAsync (resolver, "user@example.com", dkim);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (result.SpfAligned, Is.False);
			Assert.That (result.AlignedDkimResult, Is.SameAs (aligned));
			Assert.That (result.DkimResults, Has.Count.EqualTo (4));
			Assert.That (result.PolicyDomain, Is.EqualTo ("example.com"));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.AuthorDomain));
			Assert.That (result.OrganizationalDomain, Is.EqualTo ("example.com"));
			Assert.That (result.Record.AggregateReportUris, Has.Count.EqualTo (1));
			Assert.That (result.RequestedPolicy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));

			// The unrelated "example.net" domain is not looked up.
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com" }));

			var authResult = result.ToAuthenticationMethodResult ();
			Assert.That (authResult.Method, Is.EqualTo ("dmarc"));
			Assert.That (authResult.Result, Is.EqualTo ("pass"));
			Assert.That (authResult.Properties, Has.Count.EqualTo (1));
			Assert.That (authResult.Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authResult.Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authResult.Properties[0].Value, Is.EqualTo ("example.com"));
		}

		[Test]
		public async Task TestRepeatedIdentifierTreeWalkUsesCache ()
		{
			var resolver = new MockDnsResolver ();
			var identifier = "bounces.example.com";

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, identifier), Dkim (DkimSignatureStatus.Pass, identifier));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (result.SpfAligned, Is.True);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com", "_dmarc.bounces.example.com" }));
		}

		[Test]
		public async Task TestInvalidSpfIdentifierIsNotAligned ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "bad/domain"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.SpfAligned, Is.False);
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
		}

		[Test]
		public async Task TestSpfPassWithUnknownAlignmentReportsTemporaryFailure ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			resolver.AddFailure ("_dmarc.bounces.example.com", DnsQueryStatus.TemporaryFailure);

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "bounces.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
			Assert.That (result.SpfAligned, Is.False);
		}

		[Test]
		public async Task TestSpfAligned ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "bounces.example.com"), Dkim (DkimSignatureStatus.Fail, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.SpfAligned, Is.True);
			Assert.That (result.DkimAligned, Is.False);
			Assert.That (result.AlignedDkimResult, Is.Null);
			Assert.That (result.SpfResult.Domain, Is.EqualTo ("bounces.example.com"));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com", "_dmarc.bounces.example.com" }));

			// SPF results other than pass are not aligned.
			result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.SoftFail, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.SpfAligned, Is.False);
		}

		[Test]
		public async Task TestFail ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=quarantine");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "example.net"), Dkim (DkimSignatureStatus.Pass, "example.org"), Dkim (DkimSignatureStatus.PermError, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));

			var authResult = result.ToAuthenticationMethodResult ();
			Assert.That (authResult.Result, Is.EqualTo ("fail"));
			Assert.That (authResult.Properties, Has.Count.EqualTo (2));
			Assert.That (authResult.Properties[0].Value, Is.EqualTo ("example.com"));
			Assert.That (authResult.Properties[1].PropertyType, Is.EqualTo ("policy"));
			Assert.That (authResult.Properties[1].Property, Is.EqualTo ("dmarc"));
			Assert.That (authResult.Properties[1].Value, Is.EqualTo ("quarantine"));
		}

		[Test]
		public async Task TestRelaxedAlignment ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			// The Author Domain is a subdomain of the Organizational Domain.
			var result = await VerifyAsync (resolver, "user@news.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.PolicyDomain, Is.EqualTo ("example.com"));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.OrganizationalDomain));
			Assert.That (result.OrganizationalDomain, Is.EqualTo ("example.com"));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.news.example.com", "_dmarc.example.com", "_dmarc.com" }));

			// The DKIM signing domain is a different subdomain of the Organizational Domain.
			result = await VerifyAsync (resolver, "user@news.example.com", Dkim (DkimSignatureStatus.Pass, "mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.news.example.com", "_dmarc.example.com", "_dmarc.com", "_dmarc.mail.example.com" }));

			// The DKIM signing domain is a subdomain with its own Organizational Domain.
			resolver.Add ("_dmarc.mail.example.com", "v=DMARC1; p=none; psd=n");
			result = await VerifyAsync (resolver, "user@news.example.com", Dkim (DkimSignatureStatus.Pass, "a.mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.news.example.com", "_dmarc.example.com", "_dmarc.com", "_dmarc.a.mail.example.com", "_dmarc.mail.example.com" }));
		}

		[Test]
		public async Task TestStrictAlignment ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject; adkim=s; aspf=s");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "bounces.example.com"), Dkim (DkimSignatureStatus.Pass, "mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.DkimAligned, Is.False);
			Assert.That (result.SpfAligned, Is.False);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com" }));

			result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "example.com"), Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (result.SpfAligned, Is.True);
		}

		[Test]
		public async Task TestPublicSuffixDomainRecord ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example", "v=DMARC1; p=reject; sp=quarantine; psd=y");

			var result = await VerifyAsync (resolver, "user@mail.foo.example", Dkim (DkimSignatureStatus.Pass, "foo.example"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.PolicyDomain, Is.EqualTo ("example"));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.PublicSuffixDomain));
			Assert.That (result.OrganizationalDomain, Is.EqualTo ("foo.example"));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.foo.example", "_dmarc.foo.example", "_dmarc.example" }));

			// A signing domain under a different Organizational Domain within the same PSD is not aligned.
			result = await VerifyAsync (resolver, "user@mail.foo.example", Dkim (DkimSignatureStatus.Pass, "bar.example"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));

			// A record at the Organizational Domain takes precedence over the PSD record.
			resolver.Add ("_dmarc.foo.example", "v=DMARC1; p=none");
			result = await VerifyAsync (resolver, "user@mail.foo.example", Dkim (DkimSignatureStatus.Pass, "bar.example"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.PolicyDomain, Is.EqualTo ("foo.example"));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.OrganizationalDomain));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));
		}

		[Test]
		public async Task TestPublicSuffixDomainRecordAtAuthorDomainIsAuthorDomainPolicy ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example", "v=DMARC1; p=reject; psd=y");

			var result = await VerifyAsync (resolver, "user@example");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.PolicyDomain, Is.EqualTo ("example"));
			// RFC 9989, Section 4.10.1: a record published at the Author Domain is that domain's policy, even if it has psd=y.
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.AuthorDomain));
			Assert.That (result.OrganizationalDomain, Is.EqualTo ("example"));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (result.ToAuthenticationMethodResult ().Properties[1].Value, Is.EqualTo ("reject"));
		}

		[Test]
		public async Task TestPublicSuffixDomainNo ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.b.example.com", "v=DMARC1; p=quarantine; psd=n");
			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@a.b.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.PolicyDomain, Is.EqualTo ("b.example.com"));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.OrganizationalDomain));
			Assert.That (result.OrganizationalDomain, Is.EqualTo ("b.example.com"));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.a.b.example.com", "_dmarc.b.example.com" }));
		}

		[Test]
		public async Task TestTreeWalkLongDomain ()
		{
			var resolver = new MockDnsResolver ();

			var result = await VerifyAsync (resolver, "user@a.b.c.d.e.f.g.h.i.j");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (resolver.Queries, Is.EqualTo (new[] {
				"_dmarc.a.b.c.d.e.f.g.h.i.j",
				"_dmarc.d.e.f.g.h.i.j",
				"_dmarc.e.f.g.h.i.j",
				"_dmarc.f.g.h.i.j",
				"_dmarc.g.h.i.j",
				"_dmarc.h.i.j",
				"_dmarc.i.j",
				"_dmarc.j"
			}));

			result = await VerifyAsync (resolver, "user@b.c.d.e.f.g.h.i");

			Assert.That (resolver.Queries, Has.Count.EqualTo (8));
			Assert.That (resolver.Queries[1], Is.EqualTo ("_dmarc.c.d.e.f.g.h.i"));
		}

		[Test]
		public async Task TestMultipleRecords ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.mail.example.com", "v=DMARC1; p=none");
			resolver.Add ("_dmarc.mail.example.com", "v=DMARC1; p=quarantine");
			resolver.Add ("_dmarc.example.com", "v=spf1 -all");
			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@mail.example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.MultipleRecords));
			Assert.That (result.PolicyDomain, Is.EqualTo ("example.com"));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));
		}

		[Test]
		public async Task TestAuthorTreeWalkTemporaryFailure ()
		{
			var resolver = new MockDnsResolver ();

			resolver.AddFailure ("_dmarc.example.com", DnsQueryStatus.TemporaryFailure);

			var result = await VerifyAsync (resolver, "user@mail.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
			Assert.That (result.Record, Is.Null);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com" }));
			Assert.That (result.ToAuthenticationMethodResult ().Result, Is.EqualTo ("temperror"));

			// Resolver exceptions are treated as temporary failures.
			resolver = new MockDnsResolver ();
			resolver.AddException ("_dmarc.example.com", new IOException ("Network failure"));

			result = await VerifyAsync (resolver, "user@example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
		}

		[Test]
		public async Task TestAuthorRecordWithIncompleteTreeWalk ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.mail.example.com", "v=DMARC1; p=reject");
			resolver.AddFailure ("_dmarc.example.com", DnsQueryStatus.TemporaryFailure);

			// Identifiers identical to the Author Domain can be aligned without the Organizational Domain.
			var result = await VerifyAsync (resolver, "user@mail.example.com", Dkim (DkimSignatureStatus.Pass, "mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
			Assert.That (result.PolicyDomainSource, Is.EqualTo (DmarcPolicyDomainSource.AuthorDomain));
			Assert.That (result.OrganizationalDomain, Is.Null);

			// Relaxed alignment cannot be determined.
			result = await VerifyAsync (resolver, "user@mail.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));

			// Strict alignment can be determined.
			resolver = new MockDnsResolver ();
			resolver.Add ("_dmarc.mail.example.com", "v=DMARC1; p=reject; adkim=s");
			resolver.AddFailure ("_dmarc.example.com", DnsQueryStatus.TemporaryFailure);

			result = await VerifyAsync (resolver, "user@mail.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
		}

		[Test]
		public async Task TestIdentifierTreeWalkTemporaryFailure ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			resolver.AddFailure ("_dmarc.mail.example.com", DnsQueryStatus.TemporaryFailure);

			var result = await VerifyAsync (resolver, "user@example.com", Dkim (DkimSignatureStatus.Pass, "mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));

			// Another aligned identifier passes regardless.
			result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.Pass, "example.com"), Dkim (DkimSignatureStatus.Pass, "mail.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
		}

		[Test]
		public async Task TestDkimTemporaryError ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@example.com", Dkim (DkimSignatureStatus.TempError, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DkimTemporaryError));

			// A temporary error for an unaligned identifier cannot change the outcome.
			result = await VerifyAsync (resolver, "user@example.com", Dkim (DkimSignatureStatus.TempError, "example.net"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));

			// ...nor can a temporary error when another identifier is aligned.
			result = await VerifyAsync (resolver, "user@example.com", Dkim (DkimSignatureStatus.TempError, "example.com"), Dkim (DkimSignatureStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
		}

		[Test]
		public async Task TestSpfTemporaryError ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.TempError, "bounces.example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.SpfTemporaryError));

			result = await VerifyAsync (resolver, "user@example.com", new SpfCheckResult (SpfStatus.TempError, "example.net"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));
		}

		[Test]
		public async Task TestInvalidPolicy ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=bogus");

			var result = await VerifyAsync (resolver, "user@example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.InvalidPolicyRecord));
			Assert.That (result.Record, Is.Not.Null);
			Assert.That (result.PolicyDomain, Is.EqualTo ("example.com"));
			Assert.That (result.Policy, Is.Null);

			// With a valid rua tag, the record is treated as "p=none".
			resolver = new MockDnsResolver ();
			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject; sp=bogus; rua=mailto:dmarc@example.com");

			result = await VerifyAsync (resolver, "user@example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.InvalidPolicyRecord));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));
		}

		[Test]
		public async Task TestMissingPolicy ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; sp=reject");

			var result = await VerifyAsync (resolver, "user@mail.example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.InvalidPolicyRecord));

			resolver = new MockDnsResolver ();
			resolver.Add ("_dmarc.example.com", "v=DMARC1; sp=reject; rua=mailto:dmarc@example.com");

			result = await VerifyAsync (resolver, "user@mail.example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.InvalidPolicyRecord));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));
		}

		[Test]
		public async Task TestSubdomainPolicy ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject; sp=quarantine");

			var result = await VerifyAsync (resolver, "user@example.com");
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));

			result = await VerifyAsync (resolver, "user@mail.example.com");
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));

			// No existence check is needed without an "np" tag.
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com" }));

			// "np" falls back to "sp", so no existence check is needed when they are the same.
			resolver = new MockDnsResolver ();
			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=none; sp=reject; np=reject");

			result = await VerifyAsync (resolver, "user@mail.example.com");
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com" }));
		}

		[Test]
		public async Task TestNonExistentSubdomainPolicy ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=none; np=reject");
			resolver.Add ("exists.example.com", "v=spf1 -all");
			resolver.AddFailure ("unknown.example.com", DnsQueryStatus.TemporaryFailure);

			// "np" does not apply to the Organizational Domain itself.
			var result = await VerifyAsync (resolver, "user@example.com");
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.example.com", "_dmarc.com" }));

			result = await VerifyAsync (resolver, "user@missing.example.com");
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.missing.example.com", "_dmarc.example.com", "_dmarc.com", "missing.example.com" }));

			result = await VerifyAsync (resolver, "user@exists.example.com");
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));

			// If the existence of the Author Domain cannot be determined, neither can the outcome of a failure...
			result = await VerifyAsync (resolver, "user@unknown.example.com");
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));

			// ...but a pass is still a pass.
			result = await VerifyAsync (resolver, "user@unknown.example.com", Dkim (DkimSignatureStatus.Pass, "example.com"));
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.None));

			// Resolver exceptions are treated as temporary failures.
			resolver.AddException ("broken.example.com", new IOException ("Network failure"));
			result = await VerifyAsync (resolver, "user@broken.example.com");
			Assert.That (result.Status, Is.EqualTo (DmarcStatus.TempError));
			Assert.That (result.Errors, Is.EqualTo (DmarcErrors.DnsTemporaryFailure));
		}

		[Test]
		public async Task TestTestingMode ()
		{
			var resolver = new MockDnsResolver ();

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject; sp=quarantine; t=y");

			var result = await VerifyAsync (resolver, "user@example.com");

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.IsTesting, Is.True);
			Assert.That (result.RequestedPolicy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (result.ToAuthenticationMethodResult ().Properties[1].Value, Is.EqualTo ("quarantine"));

			result = await VerifyAsync (resolver, "user@mail.example.com");

			Assert.That (result.IsTesting, Is.True);
			Assert.That (result.RequestedPolicy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (result.Policy, Is.EqualTo (DmarcPolicy.None));
		}

		[Test]
		public async Task TestMaxDkimSignatures ()
		{
			var resolver = new MockDnsResolver ();
			using var message = CreateMessage ("user@example.com");
			var dkim = new[] {
				Dkim (DkimSignatureStatus.Pass, "example.net"),
				Dkim (DkimSignatureStatus.Pass, "example.org"),
				Dkim (DkimSignatureStatus.Pass, "example.com")
			};

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");

			var result = await VerifyAsync (resolver, message, dkim, null, verifier => verifier.MaxDkimSignatures = 2);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Fail));
			Assert.That (result.DkimResults, Has.Count.EqualTo (3));

			result = await VerifyAsync (resolver, message, dkim, null, verifier => verifier.MaxDkimSignatures = 3);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.AlignedDkimResult, Is.SameAs (dkim[2]));
		}

		[Test]
		public void TestCancellation ()
		{
			var resolver = new MockDnsResolver ();
			var verifier = new DmarcVerifier (resolver);
			using var message = CreateMessage ("user@example.com");
			using var cts = new CancellationTokenSource ();

			cts.Cancel ();

			Assert.Throws<OperationCanceledException> (() => verifier.Verify (message, NoDkim, null, cts.Token));
			Assert.ThrowsAsync<OperationCanceledException> (() => verifier.VerifyAsync (message, NoDkim, null, cts.Token));
			Assert.Throws<OperationCanceledException> (() => verifier.Verify (message, null, cts.Token));
			Assert.ThrowsAsync<OperationCanceledException> (() => verifier.VerifyAsync (message, null, cts.Token));

			// An OperationCanceledException thrown by the resolver during the existence check is not swallowed.
			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=none; np=reject");
			resolver.AddException ("mail.example.com", new OperationCanceledException ());

			using var subdomain = CreateMessage ("user@mail.example.com");

			Assert.Throws<OperationCanceledException> (() => verifier.Verify (subdomain, NoDkim, null));
			Assert.ThrowsAsync<OperationCanceledException> (() => verifier.VerifyAsync (subdomain, NoDkim, null));
		}

		static AsymmetricCipherKeyPair LoadDkimKeys ()
		{
			using var reader = new PemReader (new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem")));

			return (AsymmetricCipherKeyPair) reader.ReadObject ();
		}

		static MimeMessage CreateSignedMessage (MockDnsResolver resolver)
		{
			var signer = new DkimSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"), "example.com", "1433868189.example") {
				HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
				BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
				SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256
			};
			var publicKey = Convert.ToBase64String (SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo (LoadDkimKeys ().Public).GetEncoded ());
			var message = CreateMessage ("user@mail.example.com");

			signer.Sign (message, new HeaderId[] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date });
			resolver.Add ("1433868189.example._domainkey.example.com", "v=DKIM1; k=rsa; p=" + publicKey);

			return message;
		}

		[Test]
		public void TestVerifyWithDkimVerifier ()
		{
			var resolver = new MockDnsResolver ();
			var verifier = new DmarcVerifier (resolver);
			using var message = CreateSignedMessage (resolver);

			// Without a DMARC Policy Record, the DKIM signatures are not verified.
			var result = verifier.Verify (message, new SpfCheckResult (SpfStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.DkimResults, Is.Empty);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com" }));

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			resolver.Queries.Clear ();

			result = verifier.Verify (message, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (result.DkimResults, Has.Count.EqualTo (1));
			Assert.That (result.AlignedDkimResult.Header, Is.Not.Null);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com", "1433868189.example._domainkey.example.com" }));

			// The DKIM signatures are verified once, even when there are multiple Author Domains.
			message.From.Add (new MailboxAddress ("", "other@example.com"));
			verifier.MaxAuthorDomains = 2;
			resolver.Queries.Clear ();

			result = verifier.Verify (FormatOptions.Default, message, null);

			Assert.That (result.AuthorDomainResults, Has.Count.EqualTo (2));
			Assert.That (resolver.Queries.Count (query => query.EndsWith ("._domainkey.example.com", StringComparison.Ordinal)), Is.EqualTo (1));
		}

		[Test]
		public async Task TestVerifyWithDkimVerifierAsync ()
		{
			var resolver = new MockDnsResolver ();
			var verifier = new DmarcVerifier (resolver);
			using var message = CreateSignedMessage (resolver);

			// Without a DMARC Policy Record, the DKIM signatures are not verified.
			var result = await verifier.VerifyAsync (message, new SpfCheckResult (SpfStatus.Pass, "example.com"));

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.None));
			Assert.That (result.DkimResults, Is.Empty);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com" }));

			resolver.Add ("_dmarc.example.com", "v=DMARC1; p=reject");
			resolver.Queries.Clear ();

			result = await verifier.VerifyAsync (message, null);

			Assert.That (result.Status, Is.EqualTo (DmarcStatus.Pass));
			Assert.That (result.DkimAligned, Is.True);
			Assert.That (result.DkimResults, Has.Count.EqualTo (1));
			Assert.That (result.AlignedDkimResult.Header, Is.Not.Null);
			Assert.That (resolver.Queries, Is.EqualTo (new[] { "_dmarc.mail.example.com", "_dmarc.example.com", "_dmarc.com", "1433868189.example._domainkey.example.com" }));

			// The DKIM signatures are verified once, even when there are multiple Author Domains.
			message.From.Add (new MailboxAddress ("", "other@example.com"));
			verifier.MaxAuthorDomains = 2;
			resolver.Queries.Clear ();

			result = await verifier.VerifyAsync (FormatOptions.Default, message, null);

			Assert.That (result.AuthorDomainResults, Has.Count.EqualTo (2));
			Assert.That (resolver.Queries.Count (query => query.EndsWith ("._domainkey.example.com", StringComparison.Ordinal)), Is.EqualTo (1));
		}
	}
}
