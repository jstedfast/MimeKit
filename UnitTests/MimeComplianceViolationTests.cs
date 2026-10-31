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
			{ MimeComplianceViolation.InvalidParameter, 44 },
			{ MimeComplianceViolation.InvalidRfc2231Parameter, 45 },
			{ MimeComplianceViolation.EncodedWordInParameter, 46 },
			{ MimeComplianceViolation.RepeatedBoundaryParameter, 47 },
			{ MimeComplianceViolation.RepeatedFilenameParameter, 48 },
			{ MimeComplianceViolation.RepeatedParameter, 49 },
			{ MimeComplianceViolation.MissingParameterSegment, 50 },
			{ MimeComplianceViolation.InvalidEncodedWord, 51 },
			{ MimeComplianceViolation.InvalidEncodedWordPlacement, 52 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 53 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 54 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 55 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 56 },
			{ MimeComplianceViolation.UnquotedDisplayName, 57 },
			{ MimeComplianceViolation.AddressInDisplayName, 58 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 59 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 60 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 61 },
			{ MimeComplianceViolation.InvalidLocalPart, 62 },
			{ MimeComplianceViolation.MissingAddressSeparator, 63 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 64 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 65 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 66 },
			{ MimeComplianceViolation.AddressWithoutDomain, 67 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 68 },
			{ MimeComplianceViolation.InvalidDomain, 69 },
			{ MimeComplianceViolation.TrailingDotInDomain, 70 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 71 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 72 },
			{ MimeComplianceViolation.Invalid8BitAddress, 73 },
			{ MimeComplianceViolation.MissingGroupTerminator, 74 },
			{ MimeComplianceViolation.NonConformantAddress, 75 },
			{ MimeComplianceViolation.NullByteInAddress, 76 },
			{ MimeComplianceViolation.NullByteInDisplayName, 77 },
			{ MimeComplianceViolation.LineBreakInAddress, 78 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 79 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 80 },
			{ MimeComplianceViolation.EmptyGroupName, 81 },
			{ MimeComplianceViolation.NestedAddressGroup, 82 },
			{ MimeComplianceViolation.MissingBodySeparator, 83 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 84 },
			{ MimeComplianceViolation.MimeDepthLimitExceeded, 85 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 86 },
			{ MimeComplianceViolation.InvalidBase64Character, 87 },
			{ MimeComplianceViolation.InvalidBase64Padding, 88 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 89 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 90 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 91 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 92 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 93 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 94 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 95 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 96 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 97 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 98 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 99 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 100 },
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
