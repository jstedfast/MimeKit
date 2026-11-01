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
			{ MimeComplianceViolation.RepeatedContentId, 14 },
			{ MimeComplianceViolation.RepeatedContentTransferEncoding, 15 },
			{ MimeComplianceViolation.RepeatedDate, 16 },
			{ MimeComplianceViolation.RepeatedFrom, 17 },
			{ MimeComplianceViolation.RepeatedSender, 18 },
			{ MimeComplianceViolation.RepeatedReplyTo, 19 },
			{ MimeComplianceViolation.RepeatedTo, 20 },
			{ MimeComplianceViolation.RepeatedCc, 21 },
			{ MimeComplianceViolation.RepeatedBcc, 22 },
			{ MimeComplianceViolation.RepeatedMessageId, 23 },
			{ MimeComplianceViolation.RepeatedInReplyTo, 24 },
			{ MimeComplianceViolation.RepeatedReferences, 25 },
			{ MimeComplianceViolation.RepeatedSubject, 26 },
			{ MimeComplianceViolation.RepeatedReturnPath, 27 },
			{ MimeComplianceViolation.RepeatedResentDate, 28 },
			{ MimeComplianceViolation.RepeatedResentFrom, 29 },
			{ MimeComplianceViolation.RepeatedResentSender, 30 },
			{ MimeComplianceViolation.RepeatedResentTo, 31 },
			{ MimeComplianceViolation.RepeatedResentCc, 32 },
			{ MimeComplianceViolation.RepeatedResentBcc, 33 },
			{ MimeComplianceViolation.RepeatedResentMessageId, 34 },
			{ MimeComplianceViolation.MissingDate, 35 },
			{ MimeComplianceViolation.MissingFrom, 36 },
			{ MimeComplianceViolation.MissingMessageId, 37 },
			{ MimeComplianceViolation.MissingMimeVersion, 38 },
			{ MimeComplianceViolation.InvalidContentType, 39 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 40 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 41 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 42 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 43 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 44 },
			{ MimeComplianceViolation.InvalidParameter, 45 },
			{ MimeComplianceViolation.InvalidRfc2231Parameter, 46 },
			{ MimeComplianceViolation.EncodedWordInParameter, 47 },
			{ MimeComplianceViolation.RepeatedBoundaryParameter, 48 },
			{ MimeComplianceViolation.RepeatedFilenameParameter, 49 },
			{ MimeComplianceViolation.RepeatedParameter, 50 },
			{ MimeComplianceViolation.MissingParameterSegment, 51 },
			{ MimeComplianceViolation.InvalidEncodedWord, 52 },
			{ MimeComplianceViolation.InvalidEncodedWordPlacement, 53 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 54 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 55 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 56 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 57 },
			{ MimeComplianceViolation.UnquotedDisplayName, 58 },
			{ MimeComplianceViolation.AddressInDisplayName, 59 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 60 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 61 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 62 },
			{ MimeComplianceViolation.InvalidLocalPart, 63 },
			{ MimeComplianceViolation.MissingAddressSeparator, 64 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 65 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 66 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 67 },
			{ MimeComplianceViolation.AddressWithoutDomain, 68 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 69 },
			{ MimeComplianceViolation.InvalidDomain, 70 },
			{ MimeComplianceViolation.TrailingDotInDomain, 71 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 72 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 73 },
			{ MimeComplianceViolation.Invalid8BitAddress, 74 },
			{ MimeComplianceViolation.MissingGroupTerminator, 75 },
			{ MimeComplianceViolation.NonConformantAddress, 76 },
			{ MimeComplianceViolation.NullByteInAddress, 77 },
			{ MimeComplianceViolation.NullByteInDisplayName, 78 },
			{ MimeComplianceViolation.LineBreakInAddress, 79 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 80 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 81 },
			{ MimeComplianceViolation.EmptyGroupName, 82 },
			{ MimeComplianceViolation.NestedAddressGroup, 83 },
			{ MimeComplianceViolation.MissingBodySeparator, 84 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 85 },
			{ MimeComplianceViolation.MimeDepthLimitExceeded, 86 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 87 },
			{ MimeComplianceViolation.InvalidBase64Character, 88 },
			{ MimeComplianceViolation.InvalidBase64Padding, 89 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 90 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 91 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 92 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 93 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 94 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 95 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 96 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 97 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 98 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 99 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 100 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 101 },
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
