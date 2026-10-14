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
			{ MimeComplianceViolation.InvalidContentType, 33 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 34 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 35 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 36 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 37 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 38 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 39 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 40 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 41 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 42 },
			{ MimeComplianceViolation.UnquotedDisplayName, 43 },
			{ MimeComplianceViolation.AddressInDisplayName, 44 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 45 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 46 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 47 },
			{ MimeComplianceViolation.InvalidLocalPart, 48 },
			{ MimeComplianceViolation.MissingAddressSeparator, 49 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 50 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 51 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 52 },
			{ MimeComplianceViolation.AddressWithoutDomain, 53 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 54 },
			{ MimeComplianceViolation.InvalidDomain, 55 },
			{ MimeComplianceViolation.TrailingDotInDomain, 56 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 57 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 58 },
			{ MimeComplianceViolation.Invalid8BitAddress, 59 },
			{ MimeComplianceViolation.MissingGroupTerminator, 60 },
			{ MimeComplianceViolation.NonConformantAddress, 61 },
			{ MimeComplianceViolation.NullByteInAddress, 62 },
			{ MimeComplianceViolation.NullByteInDisplayName, 63 },
			{ MimeComplianceViolation.LineBreakInAddress, 64 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 65 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 66 },
			{ MimeComplianceViolation.EmptyGroupName, 67 },
			{ MimeComplianceViolation.MissingBodySeparator, 68 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 69 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 70 },
			{ MimeComplianceViolation.InvalidBase64Character, 71 },
			{ MimeComplianceViolation.InvalidBase64Padding, 72 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 73 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 74 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 75 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 76 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 77 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 78 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 79 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 80 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 81 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 82 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 83 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 84 },
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
