//
// MimeComplianceViolationTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceViolationTests
	{
		// Note: This table pins the numeric value of every MimeComplianceViolation. The values are
		// public contract: C# bakes enum constants into the consuming assembly at compile time, so
		// renumbering silently changes the meaning of code already compiled against an earlier
		// version of MimeKit, and it invalidates any value an application has persisted.
		//
		// A new violation must take the next unused value. It does not have to be declared last --
		// the enumeration groups members by subject matter -- but it must never reuse a value, be
		// given a value in the middle, or cause an existing member to be renumbered.
		//
		// The one exception is a violation that has not yet shipped in a release. Nothing can have
		// been compiled or persisted against a value that was never published, so an unreleased
		// member may be inserted into its subject-matter group and the members after it renumbered
		// to match. Update this table in the same commit; it is what makes such a change deliberate
		// rather than accidental. Once the enumeration ships, the rule above applies without
		// exception and grouping gives way to appending.
		static readonly Dictionary<MimeComplianceViolation, int> ExpectedValues = new () {
			{ MimeComplianceViolation.None, 0 },
			{ MimeComplianceViolation.BareCarriageReturnInHeader, 1 },
			{ MimeComplianceViolation.BareLinefeedInHeader, 2 },
			{ MimeComplianceViolation.BareLinefeedInBody, 3 },
			{ MimeComplianceViolation.OversizedLine, 4 },
			{ MimeComplianceViolation.Unexpected8BitBytesInHeader, 5 },
			{ MimeComplianceViolation.Unexpected8BitBytesInBody, 6 },
			{ MimeComplianceViolation.UnexpectedNullBytesInHeader, 7 },
			{ MimeComplianceViolation.UnexpectedNullBytesInBody, 8 },
			{ MimeComplianceViolation.InvalidHeader, 9 },
			{ MimeComplianceViolation.IncompleteHeader, 10 },
			{ MimeComplianceViolation.BlankHeaderContinuationLine, 11 },
			{ MimeComplianceViolation.RepeatedContentType, 12 },
			{ MimeComplianceViolation.RepeatedContentTransferEncoding, 13 },
			{ MimeComplianceViolation.RepeatedDate, 14 },
			{ MimeComplianceViolation.RepeatedFrom, 15 },
			{ MimeComplianceViolation.RepeatedSender, 16 },
			{ MimeComplianceViolation.RepeatedReplyTo, 17 },
			{ MimeComplianceViolation.RepeatedTo, 18 },
			{ MimeComplianceViolation.RepeatedCc, 19 },
			{ MimeComplianceViolation.RepeatedBcc, 20 },
			{ MimeComplianceViolation.RepeatedMessageId, 21 },
			{ MimeComplianceViolation.RepeatedInReplyTo, 22 },
			{ MimeComplianceViolation.RepeatedReferences, 23 },
			{ MimeComplianceViolation.RepeatedSubject, 24 },
			{ MimeComplianceViolation.RepeatedReturnPath, 25 },
			{ MimeComplianceViolation.RepeatedResentDate, 26 },
			{ MimeComplianceViolation.RepeatedResentFrom, 27 },
			{ MimeComplianceViolation.RepeatedResentSender, 28 },
			{ MimeComplianceViolation.RepeatedResentTo, 29 },
			{ MimeComplianceViolation.RepeatedResentCc, 30 },
			{ MimeComplianceViolation.RepeatedResentBcc, 31 },
			{ MimeComplianceViolation.RepeatedResentMessageId, 32 },
			{ MimeComplianceViolation.MissingDate, 33 },
			{ MimeComplianceViolation.MissingFrom, 34 },
			{ MimeComplianceViolation.MissingMessageId, 35 },
			{ MimeComplianceViolation.MissingMimeVersion, 36 },
			{ MimeComplianceViolation.InvalidContentType, 37 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 38 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 39 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 40 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 41 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 42 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 43 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 44 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 45 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 46 },
			{ MimeComplianceViolation.UnquotedDisplayName, 47 },
			{ MimeComplianceViolation.AddressInDisplayName, 48 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 49 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 50 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 51 },
			{ MimeComplianceViolation.InvalidLocalPart, 52 },
			{ MimeComplianceViolation.MissingAddressSeparator, 53 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 54 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 55 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 56 },
			{ MimeComplianceViolation.AddressWithoutDomain, 57 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 58 },
			{ MimeComplianceViolation.InvalidDomain, 59 },
			{ MimeComplianceViolation.TrailingDotInDomain, 60 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 61 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 62 },
			{ MimeComplianceViolation.Invalid8BitAddress, 63 },
			{ MimeComplianceViolation.MissingGroupTerminator, 64 },
			{ MimeComplianceViolation.NonConformantAddress, 65 },
			{ MimeComplianceViolation.NullByteInAddress, 66 },
			{ MimeComplianceViolation.NullByteInDisplayName, 67 },
			{ MimeComplianceViolation.LineBreakInAddress, 68 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 69 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 70 },
			{ MimeComplianceViolation.EmptyGroupName, 71 },
			{ MimeComplianceViolation.NestedAddressGroup, 72 },
			{ MimeComplianceViolation.MissingBodySeparator, 73 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 74 },
			{ MimeComplianceViolation.MimeDepthLimitExceeded, 75 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 76 },
			{ MimeComplianceViolation.InvalidBase64Character, 77 },
			{ MimeComplianceViolation.InvalidBase64Padding, 78 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 79 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 80 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 81 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 82 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 83 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 84 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 85 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 86 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 87 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 88 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 89 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 90 },
			{ MimeComplianceViolation.TooManyComplianceIssues, int.MaxValue },
		};

		[Test]
		public void TestNumericValuesAreStable ()
		{
			// Note: Removing a violation does not need to be asserted here; it breaks the compilation
			// of ExpectedValues, which is a louder failure than any assertion.
			foreach (var violation in Enum.GetValues<MimeComplianceViolation> ()) {
				Assert.That (ExpectedValues.ContainsKey (violation), Is.True,
					$"{violation} is missing from the expected value table. If it is a new violation, make sure it took the next unused value and then add it here as {{ MimeComplianceViolation.{violation}, {(int) violation} }}.");

				Assert.That ((int) violation, Is.EqualTo (ExpectedValues[violation]),
					$"The numeric value of {violation} changed from {ExpectedValues[violation]} to {(int) violation}. This is a breaking change for callers compiled against an earlier version of MimeKit and for any persisted value.");
			}
		}

		[Test]
		public void TestValuesAreUnique ()
		{
			var seen = new Dictionary<int, MimeComplianceViolation> ();

			foreach (var violation in Enum.GetValues<MimeComplianceViolation> ()) {
				var value = (int) violation;

				Assert.That (seen.ContainsKey (value), Is.False, $"{violation} and {(seen.TryGetValue (value, out var other) ? other.ToString () : null)} share the value {value}.");
				seen.Add (value, violation);
			}
		}

		[Test]
		public void TestNewViolationsTakeTheNextUnusedValue ()
		{
			// Note: A new violation does not have to be declared last -- members are grouped by subject
			// matter so that the enumeration reads well -- but it must take the next unused value.
			// Relax this only if a violation is ever deliberately retired, leaving a permanent gap.
			// TooManyComplianceIssues is not a MIME defect and is parked at the end of the range on
			// purpose, so that the defect values stay contiguous and free to grow.
			var values = Enum.GetValues<MimeComplianceViolation> ().Where (v => v != MimeComplianceViolation.TooManyComplianceIssues).Select (v => (int) v).ToList ();

			Assert.That (values.Min (), Is.EqualTo (0), "The first violation should have the value 0.");
			Assert.That (values.Max (), Is.EqualTo (values.Count - 1), "The values should be contiguous, so a new violation must take the next unused value rather than reusing or displacing an existing one.");
		}
	}
}
