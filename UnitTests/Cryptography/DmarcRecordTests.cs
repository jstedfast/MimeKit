//
// DmarcRecordTests.cs
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

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DmarcRecordTests
	{
		static DmarcRecord Parse (string text)
		{
			Assert.That (DmarcRecord.TryParse (text, out var record), Is.True, "TryParse");
			Assert.That (record, Is.Not.Null);

			return DmarcRecord.Parse (text);
		}

		static void AssertDefaults (DmarcRecord record)
		{
			Assert.That (record.Policy, Is.Null, "Policy");
			Assert.That (record.SubdomainPolicy, Is.Null, "SubdomainPolicy");
			Assert.That (record.NonExistentSubdomainPolicy, Is.Null, "NonExistentSubdomainPolicy");
			Assert.That (record.PublicSuffixDomain, Is.EqualTo (DmarcPublicSuffixDomain.Unspecified), "PublicSuffixDomain");
			Assert.That (record.Testing, Is.False, "Testing");
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Relaxed), "DkimAlignment");
			Assert.That (record.SpfAlignment, Is.EqualTo (DmarcAlignmentMode.Relaxed), "SpfAlignment");
			Assert.That (record.AggregateReportUris, Is.Empty, "AggregateReportUris");
			Assert.That (record.FailureReportUris, Is.Empty, "FailureReportUris");
			Assert.That (record.FailureReportingOptions, Is.EqualTo (DmarcFailureReportingOptions.AllFail), "FailureReportingOptions");
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => DmarcRecord.Parse (null));
			Assert.That (DmarcRecord.TryParse (null, out var record), Is.False);
			Assert.That (record, Is.Null);
		}

		[TestCase ("v=DMARC1")]
		[TestCase ("v=DMARC1;")]
		[TestCase ("v=DMARC1 ;")]
		[TestCase ("v = DMARC1")]
		[TestCase ("V=DMARC1")]
		[TestCase ("  v=DMARC1  ")]
		[TestCase ("v=\tDMARC1\t;\t")]
		public void TestMinimalRecord (string text)
		{
			var record = Parse (text);

			AssertDefaults (record);
			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
		}

		[TestCase ("")]
		[TestCase ("   ")]
		[TestCase ("p=none; v=DMARC1")]
		[TestCase ("v=dmarc1; p=none")]
		[TestCase ("v=DMARC2; p=none")]
		[TestCase ("v=DMARC; p=none")]
		[TestCase ("v=DMARC10; p=none")]
		[TestCase ("v=DMARC1 p=none")]
		[TestCase ("v DMARC1; p=none")]
		[TestCase ("v=; p=none")]
		[TestCase ("v")]
		[TestCase ("version=DMARC1; p=none")]
		[TestCase ("v=spf1 -all")]
		public void TestNotDmarcRecord (string text)
		{
			Assert.That (DmarcRecord.TryParse (text, out var record), Is.False);
			Assert.That (record, Is.Null);

			var ex = Assert.Throws<ParseException> (() => DmarcRecord.Parse (text));
			Assert.That (ex.TokenIndex, Is.EqualTo (0));
			Assert.That (ex.ErrorIndex, Is.GreaterThanOrEqualTo (0));
		}

		[Test]
		public void TestFullRecord ()
		{
			const string text = "v=DMARC1; p=reject; sp=quarantine; np=none; psd=n; t=y; adkim=s; aspf=s; " +
				"rua=mailto:dmarc@example.com, mailto:reports@example.net; ruf=mailto:failures@example.com; fo=1:d:s";
			var record = Parse (text);

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (record.SubdomainPolicy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (record.NonExistentSubdomainPolicy, Is.EqualTo (DmarcPolicy.None));
			Assert.That (record.PublicSuffixDomain, Is.EqualTo (DmarcPublicSuffixDomain.No));
			Assert.That (record.Testing, Is.True);
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Strict));
			Assert.That (record.SpfAlignment, Is.EqualTo (DmarcAlignmentMode.Strict));
			Assert.That (record.AggregateReportUris, Is.EqualTo (new[] { "mailto:dmarc@example.com", "mailto:reports@example.net" }));
			Assert.That (record.FailureReportUris, Is.EqualTo (new[] { "mailto:failures@example.com" }));
			Assert.That (record.FailureReportingOptions, Is.EqualTo (DmarcFailureReportingOptions.AnyFail | DmarcFailureReportingOptions.Dkim | DmarcFailureReportingOptions.Spf));
		}

		[TestCase ("none", DmarcPolicy.None)]
		[TestCase ("quarantine", DmarcPolicy.Quarantine)]
		[TestCase ("reject", DmarcPolicy.Reject)]
		[TestCase ("REJECT", DmarcPolicy.Reject)]
		[TestCase ("Quarantine", DmarcPolicy.Quarantine)]
		public void TestPolicies (string value, DmarcPolicy expected)
		{
			var record = Parse ($"v=DMARC1; p={value}; sp={value}; np={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (expected));
			Assert.That (record.SubdomainPolicy, Is.EqualTo (expected));
			Assert.That (record.NonExistentSubdomainPolicy, Is.EqualTo (expected));
		}

		[TestCase ("")]
		[TestCase ("bogus")]
		[TestCase ("rejected")]
		[TestCase ("none reject")]
		public void TestInvalidPolicies (string value)
		{
			var record = Parse ($"v=DMARC1; p={value}; sp={value}; np={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidPolicy | DmarcRecordErrors.InvalidSubdomainPolicy | DmarcRecordErrors.InvalidNonExistentSubdomainPolicy));
			AssertDefaults (record);
		}

		[TestCase ("y", DmarcPublicSuffixDomain.Yes)]
		[TestCase ("n", DmarcPublicSuffixDomain.No)]
		[TestCase ("u", DmarcPublicSuffixDomain.Unspecified)]
		[TestCase ("Y", DmarcPublicSuffixDomain.Yes)]
		public void TestPublicSuffixDomain (string value, DmarcPublicSuffixDomain expected)
		{
			var record = Parse ($"v=DMARC1; psd={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.PublicSuffixDomain, Is.EqualTo (expected));
		}

		[TestCase ("y", true)]
		[TestCase ("n", false)]
		[TestCase ("Y", true)]
		public void TestTesting (string value, bool expected)
		{
			var record = Parse ($"v=DMARC1; p=reject; t={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Testing, Is.EqualTo (expected));
		}

		[TestCase ("r", DmarcAlignmentMode.Relaxed)]
		[TestCase ("s", DmarcAlignmentMode.Strict)]
		[TestCase ("S", DmarcAlignmentMode.Strict)]
		public void TestAlignment (string value, DmarcAlignmentMode expected)
		{
			var record = Parse ($"v=DMARC1; adkim={value}; aspf={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.DkimAlignment, Is.EqualTo (expected));
			Assert.That (record.SpfAlignment, Is.EqualTo (expected));
		}

		[Test]
		public void TestInvalidSimpleValues ()
		{
			var record = Parse ("v=DMARC1; psd=x; t=yes; adkim=strict; aspf=; p=none");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidPublicSuffixDomain | DmarcRecordErrors.InvalidTesting |
				DmarcRecordErrors.InvalidDkimAlignment | DmarcRecordErrors.InvalidSpfAlignment));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.None));
			Assert.That (record.PublicSuffixDomain, Is.EqualTo (DmarcPublicSuffixDomain.Unspecified));
			Assert.That (record.Testing, Is.False);
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Relaxed));
			Assert.That (record.SpfAlignment, Is.EqualTo (DmarcAlignmentMode.Relaxed));
		}

		[Test]
		public void TestTagNamesAreCaseInsensitive ()
		{
			var record = Parse ("v=DMARC1; P=reject; SP=none; ADKIM=s; RUA=mailto:a@example.com");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (record.SubdomainPolicy, Is.EqualTo (DmarcPolicy.None));
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Strict));
			Assert.That (record.AggregateReportUris, Is.EqualTo (new[] { "mailto:a@example.com" }));
		}

		[Test]
		public void TestWhiteSpace ()
		{
			var record = Parse ("v=DMARC1 ;\tp = quarantine\t; rua =  mailto:a@example.com ,\tmailto:b@example.com  ; adkim= s ;");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Quarantine));
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Strict));
			Assert.That (record.AggregateReportUris, Is.EqualTo (new[] { "mailto:a@example.com", "mailto:b@example.com" }));
		}

		[Test]
		public void TestUnknownAndHistoricTagsAreIgnored ()
		{
			var record = Parse ("v=DMARC1; pct=50; rf=afrf; ri=86400; foo=bar; x=; p=reject");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
		}

		[Test]
		public void TestEmptyTagsAreIgnored ()
		{
			var record = Parse ("v=DMARC1;; ;p=reject;;");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
		}

		[TestCase ("v=DMARC1; p; sp=reject")]
		[TestCase ("v=DMARC1; =none; sp=reject")]
		[TestCase ("v=DMARC1; p1=none; sp=reject")]
		[TestCase ("v=DMARC1; p_x=none; sp=reject")]
		[TestCase ("v=DMARC1; p x=none; sp=reject")]
		[TestCase ("v=DMARC1; garbage; sp=reject")]
		public void TestMalformedTags (string text)
		{
			var record = Parse (text);

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.MalformedTag));
			Assert.That (record.Policy, Is.Null);
			Assert.That (record.SubdomainPolicy, Is.EqualTo (DmarcPolicy.Reject));
		}

		[Test]
		public void TestDuplicateTags ()
		{
			var record = Parse ("v=DMARC1; p=reject; p=none; adkim=s; adkim=r; v=DMARC1");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.DuplicateTag));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (record.DkimAlignment, Is.EqualTo (DmarcAlignmentMode.Strict));
		}

		[Test]
		public void TestDuplicateInvalidTag ()
		{
			// the first occurrence wins even when it is invalid
			var record = Parse ("v=DMARC1; p=bogus; p=reject");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidPolicy | DmarcRecordErrors.DuplicateTag));
			Assert.That (record.Policy, Is.Null);
		}

		[Test]
		public void TestReportUris ()
		{
			var record = Parse ("v=DMARC1; rua=mailto:a@example.com!10m, https://example.com/dmarc!5 , mailto:b@example.com; ruf=mailto:c@example.com!1g");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.AggregateReportUris, Is.EqualTo (new[] { "mailto:a@example.com", "https://example.com/dmarc", "mailto:b@example.com" }));
			Assert.That (record.FailureReportUris, Is.EqualTo (new[] { "mailto:c@example.com" }));
		}

		[Test]
		public void TestPercentEncodedUri ()
		{
			var record = Parse ("v=DMARC1; rua=mailto:a%2Cb%21c@example.com");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.AggregateReportUris, Is.EqualTo (new[] { "mailto:a%2Cb%21c@example.com" }));
		}

		[TestCase ("rua=", 0)]
		[TestCase ("rua=,", 0)]
		[TestCase ("rua=dmarc@example.com", 0)]
		[TestCase ("rua=/var/dmarc", 0)]
		[TestCase ("rua=1mailto:a@example.com", 0)]
		[TestCase ("rua=mailto:a@example.com,", 1)]
		[TestCase ("rua=mailto:a@example.com,,mailto:b@example.com", 2)]
		[TestCase ("rua=mailto:a@example.com, not a uri", 1)]
		[TestCase ("rua=mailto:a b@example.com", 0)]
		[TestCase ("rua=mailto:dmarc@exämple.com, mailto:b@example.com", 1)]
		[TestCase ("rua=!10m", 0)]
		public void TestInvalidAggregateReportUris (string tag, int validCount)
		{
			var record = Parse ("v=DMARC1; p=none; " + tag);

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidAggregateReportUri));
			Assert.That (record.AggregateReportUris, Has.Count.EqualTo (validCount));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.None));
		}

		[Test]
		public void TestInvalidFailureReportUris ()
		{
			var record = Parse ("v=DMARC1; ruf=bogus, mailto:a@example.com");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidFailureReportUri));
			Assert.That (record.FailureReportUris, Is.EqualTo (new[] { "mailto:a@example.com" }));
		}

		[Test]
		public void TestInvalidPolicyWithAggregateReportUri ()
		{
			// RFC 9989, Section 4.10.1: the verifier treats this as p=none because there is a valid rua URI
			var record = Parse ("v=DMARC1; p=bogus; rua=mailto:a@example.com");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidPolicy));
			Assert.That (record.Policy, Is.Null);
			Assert.That (record.AggregateReportUris, Has.Count.EqualTo (1));
		}

		[TestCase ("0", DmarcFailureReportingOptions.AllFail)]
		[TestCase ("1", DmarcFailureReportingOptions.AnyFail)]
		[TestCase ("d", DmarcFailureReportingOptions.Dkim)]
		[TestCase ("s", DmarcFailureReportingOptions.Spf)]
		[TestCase ("D:S", DmarcFailureReportingOptions.Dkim | DmarcFailureReportingOptions.Spf)]
		[TestCase ("0:d", DmarcFailureReportingOptions.AllFail | DmarcFailureReportingOptions.Dkim)]
		[TestCase ("d:0:s", DmarcFailureReportingOptions.AllFail | DmarcFailureReportingOptions.Dkim | DmarcFailureReportingOptions.Spf)]
		[TestCase ("s:d:1", DmarcFailureReportingOptions.AnyFail | DmarcFailureReportingOptions.Dkim | DmarcFailureReportingOptions.Spf)]
		[TestCase ("1 : d", DmarcFailureReportingOptions.AnyFail | DmarcFailureReportingOptions.Dkim)]
		public void TestFailureReportingOptions (string value, DmarcFailureReportingOptions expected)
		{
			var record = Parse ($"v=DMARC1; ruf=mailto:a@example.com; fo={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.FailureReportingOptions, Is.EqualTo (expected));
		}

		[TestCase ("")]
		[TestCase ("   ")]
		[TestCase ("0:1")]
		[TestCase ("1:d:0")]
		[TestCase ("d:d")]
		[TestCase ("0:0")]
		[TestCase ("2")]
		[TestCase ("ds")]
		[TestCase ("d:")]
		[TestCase (":d")]
		[TestCase ("d::s")]
		[TestCase ("x")]
		public void TestInvalidFailureReportingOptions (string value)
		{
			// the fo tag may appear before the ruf tag
			var record = Parse ($"v=DMARC1; fo={value}; ruf=mailto:a@example.com");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.InvalidFailureReportingOptions));
			Assert.That (record.FailureReportingOptions, Is.EqualTo (DmarcFailureReportingOptions.AllFail));
		}

		[TestCase ("1")]
		[TestCase ("0:1")]
		public void TestFailureReportingOptionsIgnoredWithoutFailureReportUri (string value)
		{
			var record = Parse ($"v=DMARC1; p=none; fo={value}");

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.FailureReportingOptions, Is.EqualTo (DmarcFailureReportingOptions.AllFail));
		}

		[Test]
		public void TestLargeRecord ()
		{
			// a hostile record with a huge number of tags and URIs must parse in linear time
			var builder = new System.Text.StringBuilder ("v=DMARC1; p=reject");

			for (int i = 0; i < 10000; i++)
				builder.Append ("; x=y");

			builder.Append ("; rua=mailto:a@example.com");

			for (int i = 0; i < 10000; i++)
				builder.Append (",mailto:a@example.com");

			var record = Parse (builder.ToString ());

			Assert.That (record.Errors, Is.EqualTo (DmarcRecordErrors.None));
			Assert.That (record.Policy, Is.EqualTo (DmarcPolicy.Reject));
			Assert.That (record.AggregateReportUris, Has.Count.EqualTo (10001));
		}
	}
}
