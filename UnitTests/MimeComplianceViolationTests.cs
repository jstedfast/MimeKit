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
			{ MimeComplianceViolation.RepeatedContentDisposition, 13 },
			{ MimeComplianceViolation.RepeatedContentTransferEncoding, 14 },
			{ MimeComplianceViolation.RepeatedDate, 15 },
			{ MimeComplianceViolation.RepeatedFrom, 16 },
			{ MimeComplianceViolation.RepeatedSender, 17 },
			{ MimeComplianceViolation.RepeatedReplyTo, 18 },
			{ MimeComplianceViolation.RepeatedTo, 19 },
			{ MimeComplianceViolation.RepeatedCc, 20 },
			{ MimeComplianceViolation.RepeatedBcc, 21 },
			{ MimeComplianceViolation.RepeatedMessageId, 22 },
			{ MimeComplianceViolation.RepeatedInReplyTo, 23 },
			{ MimeComplianceViolation.RepeatedReferences, 24 },
			{ MimeComplianceViolation.RepeatedSubject, 25 },
			{ MimeComplianceViolation.RepeatedReturnPath, 26 },
			{ MimeComplianceViolation.RepeatedResentDate, 27 },
			{ MimeComplianceViolation.RepeatedResentFrom, 28 },
			{ MimeComplianceViolation.RepeatedResentSender, 29 },
			{ MimeComplianceViolation.RepeatedResentTo, 30 },
			{ MimeComplianceViolation.RepeatedResentCc, 31 },
			{ MimeComplianceViolation.RepeatedResentBcc, 32 },
			{ MimeComplianceViolation.RepeatedResentMessageId, 33 },
			{ MimeComplianceViolation.MissingDate, 34 },
			{ MimeComplianceViolation.MissingFrom, 35 },
			{ MimeComplianceViolation.MissingMessageId, 36 },
			{ MimeComplianceViolation.MissingMimeVersion, 37 },
			{ MimeComplianceViolation.InvalidContentType, 38 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 39 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 40 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 41 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 42 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 43 },
			{ MimeComplianceViolation.RepeatedBoundaryParameter, 44 },
			{ MimeComplianceViolation.RepeatedFilenameParameter, 45 },
			{ MimeComplianceViolation.RepeatedParameter, 46 },
			{ MimeComplianceViolation.MissingParameterSegment, 47 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 48 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 49 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 50 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 51 },
			{ MimeComplianceViolation.UnquotedDisplayName, 52 },
			{ MimeComplianceViolation.AddressInDisplayName, 53 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 54 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 55 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 56 },
			{ MimeComplianceViolation.InvalidLocalPart, 57 },
			{ MimeComplianceViolation.MissingAddressSeparator, 58 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 59 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 60 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 61 },
			{ MimeComplianceViolation.AddressWithoutDomain, 62 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 63 },
			{ MimeComplianceViolation.InvalidDomain, 64 },
			{ MimeComplianceViolation.TrailingDotInDomain, 65 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 66 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 67 },
			{ MimeComplianceViolation.Invalid8BitAddress, 68 },
			{ MimeComplianceViolation.MissingGroupTerminator, 69 },
			{ MimeComplianceViolation.NonConformantAddress, 70 },
			{ MimeComplianceViolation.NullByteInAddress, 71 },
			{ MimeComplianceViolation.NullByteInDisplayName, 72 },
			{ MimeComplianceViolation.LineBreakInAddress, 73 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 74 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 75 },
			{ MimeComplianceViolation.EmptyGroupName, 76 },
			{ MimeComplianceViolation.NestedAddressGroup, 77 },
			{ MimeComplianceViolation.MissingBodySeparator, 78 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 79 },
			{ MimeComplianceViolation.MimeDepthLimitExceeded, 80 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 81 },
			{ MimeComplianceViolation.InvalidBase64Character, 82 },
			{ MimeComplianceViolation.InvalidBase64Padding, 83 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 84 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 85 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 86 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 87 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 88 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 89 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 90 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 91 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 92 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 93 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 94 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 95 },
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
