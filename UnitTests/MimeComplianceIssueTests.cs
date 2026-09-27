//
// MimeComplianceIssueTests.cs
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

using System.Linq;
using System.Text;
using MimeKit;
using MimeKit.Encodings;

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceIssueTests
	{
		// Note: None is excluded because it is never reported and has no description, severity or categories.
		static IEnumerable<MimeComplianceViolation> AllViolations => Enum.GetValues<MimeComplianceViolation> ().Where (v => v != MimeComplianceViolation.None);

		[Test]
		public void TestDefaultInstanceIsDistinguishableFromARealIssue ()
		{
			// Note: This is the reason MimeComplianceViolation.None occupies the zero slot. If a real
			// violation were given the value 0, then default(MimeComplianceIssue) would masquerade as
			// a genuine report of that violation.
			var issue = default (MimeComplianceIssue);

			Assert.That (issue.Violation, Is.EqualTo (MimeComplianceViolation.None));
		}

		[Test]
		public void TestConstructorRejectsViolationsThatAreNotReportable ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new MimeComplianceIssue (MimeComplianceViolation.None, 0, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MimeComplianceIssue (MimeComplianceViolation.None, 0, 1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MimeComplianceIssue ((MimeComplianceViolation) (-1), 0, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MimeComplianceIssue ((MimeComplianceViolation) 999, 0, 1));

			foreach (var violation in AllViolations)
				Assert.DoesNotThrow (() => new MimeComplianceIssue (violation, 0, 1), $"{violation} should be constructible.");
		}

		[Test]
		public void TestEmptyGroupNameIsTheLargestViolation ()
		{
			// Note: The MimeComplianceIssue constructor range check is written in terms of the largest
			// defined violation. If a new violation is appended, the check must be updated to match.
			var max = Enum.GetValues<MimeComplianceViolation> ().Max ();

			Assert.That (max, Is.EqualTo (MimeComplianceViolation.EmptyGroupName));
		}

		[Test]
		public void TestColumnNumberIsRelativeToThePhysicalLine ()
		{
			// Note: Unlike the address violations, this one is reported by the reader itself, which
			// derives the column from the offset of the line it is currently scanning.
			var text = "To: a@example.com\nSubject: test\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			var issue = logger.Issues.First (i => i.Violation == MimeComplianceViolation.BareLinefeedInHeader);

			Assert.That (issue.LineNumber, Is.EqualTo (1), "LineNumber");

			// Note: "To: a@example.com" is 17 characters, so the bare linefeed is at column 18.
			Assert.That (issue.ColumnNumber, Is.EqualTo (18), "ColumnNumber");
			Assert.That (text[(int) issue.StreamOffset], Is.EqualTo ('\n'), "StreamOffset");
		}

		[Test]
		public void TestEquality ()
		{
			var issue = new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 100, 5, 3);
			var same = new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 100, 5, 3);

			Assert.That (issue.Equals (same), Is.True, "Equals");
			Assert.That (issue.Equals ((object) same), Is.True, "Equals (object)");
			Assert.That (issue == same, Is.True, "operator ==");
			Assert.That (issue != same, Is.False, "operator !=");
			Assert.That (issue.GetHashCode (), Is.EqualTo (same.GetHashCode ()), "GetHashCode");

			Assert.That (issue.Equals ((object) "not an issue"), Is.False, "Equals (string)");
			Assert.That (issue.Equals ((object) null), Is.False, "Equals (null)");

			// Note: Each of these differs from `issue` in exactly one field.
			var others = new [] {
				new MimeComplianceIssue (MimeComplianceViolation.IncompleteHeader, 100, 5, 3),
				new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 101, 5, 3),
				new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 100, 6, 3),
				new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 100, 5, 4)
			};

			foreach (var other in others) {
				Assert.That (issue.Equals (other), Is.False, $"{other} should not equal {issue}.");
				Assert.That (issue == other, Is.False, $"{other} == {issue}");
				Assert.That (issue != other, Is.True, $"{other} != {issue}");
			}

			// Note: Loggers that de-duplicate issues depend on this.
			var set = new HashSet<MimeComplianceIssue> { issue, same };

			Assert.That (set, Has.Count.EqualTo (1), "HashSet should treat equal issues as duplicates.");
		}

		[Test]
		public void TestEveryViolationHasADescription ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetDescription().
			foreach (var violation in AllViolations) {
				var description = MimeComplianceIssue.GetDescription (violation);

				Assert.That (description, Is.Not.Null.And.Not.Empty, $"{violation} has no description.");
				Assert.That (description, Does.EndWith ("."), $"{violation} description should be a sentence.");
			}
		}

		[Test]
		public void TestEveryViolationHasRemarks ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetRemarks().
			foreach (var violation in AllViolations) {
				var remarks = MimeComplianceIssue.GetRemarks (violation);

				Assert.That (remarks, Is.Not.Null.And.Not.Empty, $"{violation} has no remarks.");
				Assert.That (remarks, Does.EndWith ("."), $"{violation} remarks should be a sentence.");
			}
		}

		[Test]
		public void TestDescriptionsAreUnique ()
		{
			var seen = new Dictionary<string, MimeComplianceViolation> ();

			foreach (var violation in AllViolations) {
				var description = MimeComplianceIssue.GetDescription (violation);

				// Note: A duplicated description is the signature of a copy/paste error.
				Assert.That (seen.ContainsKey (description), Is.False, $"{violation} and {(seen.TryGetValue (description, out var other) ? other.ToString () : null)} share a description.");
				seen.Add (description, violation);
			}
		}

		[Test]
		public void TestEveryViolationHasASeverity ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetSeverity().
			foreach (var violation in AllViolations) {
				foreach (var context in Enum.GetValues<MimeComplianceContext> ()) {
					var severity = MimeComplianceIssue.GetSeverity (violation, context);

					Assert.That (Enum.IsDefined (severity), Is.True, $"{violation} has an undefined severity in {context}.");
				}
			}
		}

		[Test]
		public void TestSeverityAssignments ()
		{
			// Note: These are deliberate judgement calls, pinned so that any future re-rating is a
			// conscious decision rather than an accident.

			// Note: These are requirements of the channel rather than of the message itself, so they
			// are only Minor when the message came from a local message store.
			var channelOnly = new [] {
				MimeComplianceViolation.BareLinefeedInHeader,
				MimeComplianceViolation.BareLinefeedInBody,
				MimeComplianceViolation.OversizedLine
			};

			// Note: 8-bit content is universally tolerated via charset fallback, so it is Minor in
			// both contexts. The address violations listed here are obsolete-but-well-defined
			// syntax or defects that MimeKit normalizes away without changing which mailbox the
			// address denotes.
			var minor = new [] {
				MimeComplianceViolation.Unexpected8BitBytesInHeader,
				MimeComplianceViolation.Unexpected8BitBytesInBody,
				MimeComplianceViolation.ExcessiveAngleBracketsInAddress,
				MimeComplianceViolation.ObsoleteRouteAddress,
				MimeComplianceViolation.ExtraneousCommaInAddressList,
				MimeComplianceViolation.ObsoleteDomainSyntax,
				MimeComplianceViolation.TrailingDotInDomain,
				MimeComplianceViolation.WhitespaceInDomainLiteral,
				MimeComplianceViolation.EmptyGroupName
			};

			// Note: These are the classic MIME content-smuggling vectors, plus the two address defects
			// that work the same way: a null byte terminates the address early for anything that
			// treats it as a C string, and a line break inside an address token is the SMTP header
			// injection primitive. Either way, different components can be made to read different
			// mailboxes out of the same header.
			var critical = new [] {
				MimeComplianceViolation.MultipleContentTypes,
				MimeComplianceViolation.MultipleContentTransferEncodings,
				MimeComplianceViolation.UnexpectedNullBytesInHeader,
				MimeComplianceViolation.UnexpectedNullBytesInBody,
				MimeComplianceViolation.NullByteInAddress,
				MimeComplianceViolation.LineBreakInAddress
			};

			foreach (var violation in channelOnly) {
				Assert.That (MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Transport), Is.EqualTo (MimeComplianceSeverity.Major), $"{violation} (Transport)");
				Assert.That (MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Storage), Is.EqualTo (MimeComplianceSeverity.Minor), $"{violation} (Storage)");
			}

			foreach (var violation in minor)
				Assert.That (MimeComplianceIssue.GetSeverity (violation), Is.EqualTo (MimeComplianceSeverity.Minor), $"{violation}");

			foreach (var violation in critical)
				Assert.That (MimeComplianceIssue.GetSeverity (violation), Is.EqualTo (MimeComplianceSeverity.Critical), $"{violation}");

			foreach (var violation in AllViolations) {
				if (channelOnly.Contains (violation) || minor.Contains (violation) || critical.Contains (violation))
					continue;

				Assert.That (MimeComplianceIssue.GetSeverity (violation), Is.EqualTo (MimeComplianceSeverity.Major), $"{violation}");
			}
		}

		[Test]
		public void TestOnlyChannelViolationsAreContextDependent ()
		{
			// Note: Guards against a new violation being given a context-dependent severity without
			// MimeComplianceContext's documentation (which enumerates them) being updated to match.
			var expected = new [] {
				MimeComplianceViolation.BareLinefeedInHeader,
				MimeComplianceViolation.BareLinefeedInBody,
				MimeComplianceViolation.OversizedLine
			};

			var actual = AllViolations.Where (violation =>
				MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Transport) !=
				MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Storage)).ToArray ();

			Assert.That (actual, Is.EquivalentTo (expected));
		}

		[Test]
		public void TestStorageIsNeverStricterThanTransport ()
		{
			foreach (var violation in AllViolations) {
				var transport = MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Transport);
				var storage = MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Storage);

				Assert.That (storage, Is.LessThanOrEqualTo (transport), $"{violation}");
			}
		}

		[Test]
		public void TestGetSeverityThrowsOnInvalidContext ()
		{
			var invalid = (MimeComplianceContext) 9999;
			var issue = new MimeComplianceIssue (MimeComplianceViolation.BareLinefeedInHeader, 0, 1);

			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetSeverity (MimeComplianceViolation.BareLinefeedInHeader, invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => issue.GetSeverity (invalid));
		}

		[Test]
		public void TestInstanceGetSeverityMatchesStatic ()
		{
			foreach (var violation in AllViolations) {
				var issue = new MimeComplianceIssue (violation, 0, 1);

				foreach (var context in Enum.GetValues<MimeComplianceContext> ())
					Assert.That (issue.GetSeverity (context), Is.EqualTo (MimeComplianceIssue.GetSeverity (violation, context)), $"{violation} ({context})");

				Assert.That (issue.Severity, Is.EqualTo (issue.GetSeverity (MimeComplianceContext.Transport)), $"{violation}");
			}
		}

		[Test]
		public void TestObsoleteBase64CommentIsNotMerelyCosmetic ()
		{
			// Note: The characters making up an RFC 1113 comment are themselves valid base64
			// characters, so MimeKit's own decoder absorbs the comment as content rather than
			// skipping it, corrupting everything that follows. This is why the violation is rated
			// Major rather than Minor.
			const string expected = "This is the plain text message!";
			var clean = Encoding.ASCII.GetBytes ("VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==");
			var commented = Encoding.ASCII.GetBytes ("VGhpcyBpcyB0*comment*aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==");

			Assert.That (Decode (clean), Is.EqualTo (expected), "The control input should decode correctly.");
			Assert.That (Decode (commented), Is.Not.EqualTo (expected), "An RFC 1113 comment should corrupt the decoded content.");

			Assert.That (MimeComplianceIssue.GetSeverity (MimeComplianceViolation.ObsoleteBase64Comment), Is.EqualTo (MimeComplianceSeverity.Major));

			static string Decode (byte[] input)
			{
				var decoder = new Base64Decoder ();
				var output = new byte[decoder.EstimateOutputLength (input.Length)];
				int n = decoder.Decode (input, 0, input.Length, output);

				return Encoding.ASCII.GetString (output, 0, n);
			}
		}

		[Test]
		public void TestSeveritiesAreOrderedByIncreasingSeriousness ()
		{
			// Note: Callers are documented as being able to write `severity >= Major`, so the
			// numeric ordering is part of the public contract.
			Assert.That (MimeComplianceSeverity.Minor, Is.LessThan (MimeComplianceSeverity.Major));
			Assert.That (MimeComplianceSeverity.Major, Is.LessThan (MimeComplianceSeverity.Critical));
		}

		[Test]
		public void TestEveryViolationHasACategory ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetCategories().
			const MimeComplianceCategories all = MimeComplianceCategories.Interoperability |
				MimeComplianceCategories.DataLoss | MimeComplianceCategories.Security;

			foreach (var violation in AllViolations) {
				var categories = MimeComplianceIssue.GetCategories (violation);

				Assert.That (categories, Is.Not.EqualTo (MimeComplianceCategories.None), $"{violation} has no category.");
				Assert.That (categories & ~all, Is.EqualTo (MimeComplianceCategories.None), $"{violation} has an undefined category bit.");
			}
		}

		[Test]
		public void TestEveryCategoryIsUsed ()
		{
			// Note: A category that no violation maps to is dead weight in the public API.
			foreach (var category in Enum.GetValues<MimeComplianceCategories> ()) {
				if (category == MimeComplianceCategories.None)
					continue;

				Assert.That (AllViolations.Any (v => (MimeComplianceIssue.GetCategories (v) & category) != 0), Is.True, $"No violation is categorized as {category}.");
			}
		}

		[Test]
		public void TestCategoryAssignments ()
		{
			// Note: These are deliberate judgement calls, pinned so that any future re-categorization
			// is a conscious decision rather than an accident.
			const MimeComplianceCategories Interop = MimeComplianceCategories.Interoperability;
			const MimeComplianceCategories DataLoss = MimeComplianceCategories.DataLoss;
			const MimeComplianceCategories Security = MimeComplianceCategories.Security;

			var expected = new Dictionary<MimeComplianceViolation, MimeComplianceCategories> {
				{ MimeComplianceViolation.BareLinefeedInHeader, Interop | Security },
				{ MimeComplianceViolation.BareLinefeedInBody, Interop | Security },
				{ MimeComplianceViolation.InvalidHeader, Interop | Security },
				{ MimeComplianceViolation.IncompleteHeader, Interop },
				{ MimeComplianceViolation.InvalidContentType, Interop | Security },
				{ MimeComplianceViolation.MultipleContentTypes, Interop | Security },
				{ MimeComplianceViolation.InvalidContentTransferEncoding, Interop | DataLoss | Security },
				{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, Interop | Security },
				{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, Interop | Security },
				{ MimeComplianceViolation.MultipleContentTransferEncodings, Interop | DataLoss | Security },
				{ MimeComplianceViolation.OversizedLine, Interop | DataLoss },
				{ MimeComplianceViolation.MissingBodySeparator, Interop | Security },
				{ MimeComplianceViolation.MissingMultipartBoundaryParameter, Interop | DataLoss },
				{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, Interop | DataLoss | Security },
				{ MimeComplianceViolation.MissingMultipartBoundary, Interop | DataLoss },
				{ MimeComplianceViolation.Unexpected8BitBytesInHeader, Interop | DataLoss },
				{ MimeComplianceViolation.Unexpected8BitBytesInBody, Interop | DataLoss },
				{ MimeComplianceViolation.UnexpectedNullBytesInHeader, Interop | Security },
				{ MimeComplianceViolation.UnexpectedNullBytesInBody, Interop | Security },
				{ MimeComplianceViolation.IncompleteBase64Quantum, DataLoss },
				{ MimeComplianceViolation.InvalidBase64Character, DataLoss | Security },
				{ MimeComplianceViolation.InvalidBase64Padding, DataLoss },
				{ MimeComplianceViolation.Base64CharactersAfterPadding, DataLoss | Security },
				{ MimeComplianceViolation.ObsoleteBase64Comment, DataLoss | Security },
				{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, DataLoss },
				{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, DataLoss },
				{ MimeComplianceViolation.InvalidUUEncodePretext, Interop },
				{ MimeComplianceViolation.InvalidUUEncodeFileMode, Interop },
				{ MimeComplianceViolation.InvalidUUEncodedContent, DataLoss },
				{ MimeComplianceViolation.InvalidUUEncodedLineLength, DataLoss },
				{ MimeComplianceViolation.IncompleteUUEncodedLine, DataLoss },
				{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, Interop | DataLoss },
				{ MimeComplianceViolation.InvalidUUEncodeEndMarker, Interop },
				{ MimeComplianceViolation.IncompleteUUEncodedContent, DataLoss },
				{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, Interop },
				{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, Interop },
				{ MimeComplianceViolation.UnbalancedQuotesInAddress, Interop | DataLoss | Security },
				{ MimeComplianceViolation.UnbalancedParenthesesInAddress, Interop | DataLoss | Security },
				{ MimeComplianceViolation.UnquotedDisplayName, Interop | DataLoss },
				{ MimeComplianceViolation.InvalidLocalPart, Interop },
				{ MimeComplianceViolation.MissingAddressSeparator, Interop | DataLoss },
				{ MimeComplianceViolation.ExtraneousCommaInAddressList, Interop },
				{ MimeComplianceViolation.ObsoleteRouteAddress, Interop },
				{ MimeComplianceViolation.AddressWithoutDomain, Interop },
				{ MimeComplianceViolation.ObsoleteDomainSyntax, Interop },
				{ MimeComplianceViolation.TrailingDotInDomain, Interop },
				{ MimeComplianceViolation.WhitespaceInDomainLiteral, Interop },
				{ MimeComplianceViolation.Invalid8BitAddress, Interop | DataLoss },
				{ MimeComplianceViolation.MissingGroupTerminator, Interop | DataLoss | Security },
				{ MimeComplianceViolation.NonConformantAddress, Interop },
				{ MimeComplianceViolation.NullByteInAddress, Interop | DataLoss | Security },
				{ MimeComplianceViolation.LineBreakInAddress, Interop | DataLoss | Security },
				{ MimeComplianceViolation.ControlCharacterInAddress, Interop | Security },
				{ MimeComplianceViolation.EmptyGroupName, Interop }
			};

			foreach (var violation in AllViolations) {
				Assert.That (expected.ContainsKey (violation), Is.True, $"{violation} is missing from the expected category table.");
				Assert.That (MimeComplianceIssue.GetCategories (violation), Is.EqualTo (expected[violation]), $"{violation}");
			}
		}

		[Test]
		public void TestCriticalViolationsAreAllSecurityIssues ()
		{
			// Note: The Critical rating exists because of content smuggling, so the two axes must
			// agree about which violations that applies to.
			foreach (var violation in AllViolations) {
				if (MimeComplianceIssue.GetSeverity (violation) != MimeComplianceSeverity.Critical)
					continue;

				Assert.That (MimeComplianceIssue.GetCategories (violation) & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), $"{violation} is Critical but is not categorized as a Security issue.");
			}
		}

		[Test]
		public void TestInteroperabilityOnlyViolationsAreNotCritical ()
		{
			// Note: Critical is reserved for content smuggling, so a violation that loses no content
			// and enables no evasion cannot reasonably earn it.
			foreach (var violation in AllViolations) {
				if (MimeComplianceIssue.GetCategories (violation) != MimeComplianceCategories.Interoperability)
					continue;

				Assert.That (MimeComplianceIssue.GetSeverity (violation), Is.LessThan (MimeComplianceSeverity.Critical), $"{violation} only harms interoperability but is rated Critical.");
			}
		}

		[Test]
		public void TestGetDescriptionAndGetRemarksThrowOnInvalidViolation ()
		{
			var invalid = (MimeComplianceViolation) 9999;

			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetDescription (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetRemarks (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetSeverity (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetCategories (invalid));
		}

		[Test]
		public void TestPropertiesMatchStaticMethods ()
		{
			foreach (var violation in AllViolations) {
				var issue = new MimeComplianceIssue (violation, 123, 4);

				Assert.That (issue.Violation, Is.EqualTo (violation));
				Assert.That (issue.StreamOffset, Is.EqualTo (123));
				Assert.That (issue.LineNumber, Is.EqualTo (4));
				Assert.That (issue.ColumnNumber, Is.EqualTo (0), "ColumnNumber should default to 0 (unknown).");
				Assert.That (issue.Description, Is.EqualTo (MimeComplianceIssue.GetDescription (violation)));
				Assert.That (issue.Remarks, Is.EqualTo (MimeComplianceIssue.GetRemarks (violation)));
				Assert.That (issue.Severity, Is.EqualTo (MimeComplianceIssue.GetSeverity (violation)));
				Assert.That (issue.Categories, Is.EqualTo (MimeComplianceIssue.GetCategories (violation)));
			}
		}

		[Test]
		public void TestToString ()
		{
			var withoutColumn = new MimeComplianceIssue (MimeComplianceViolation.BareLinefeedInHeader, 42, 7);
			var withColumn = new MimeComplianceIssue (MimeComplianceViolation.BareLinefeedInHeader, 42, 7, 13);

			Assert.That (withoutColumn.ToString (), Is.EqualTo ("BareLinefeedInHeader at line 7 (offset 42)"));
			Assert.That (withColumn.ToString (), Is.EqualTo ("BareLinefeedInHeader at line 7, column 13 (offset 42)"));
		}
	}
}
