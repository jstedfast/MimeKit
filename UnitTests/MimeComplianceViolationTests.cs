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
			{ MimeComplianceViolation.RepeatedContentType, 11 },
			{ MimeComplianceViolation.RepeatedContentTransferEncoding, 12 },
			{ MimeComplianceViolation.RepeatedDate, 13 },
			{ MimeComplianceViolation.RepeatedFrom, 14 },
			{ MimeComplianceViolation.RepeatedSender, 15 },
			{ MimeComplianceViolation.RepeatedReplyTo, 16 },
			{ MimeComplianceViolation.RepeatedTo, 17 },
			{ MimeComplianceViolation.RepeatedCc, 18 },
			{ MimeComplianceViolation.RepeatedBcc, 19 },
			{ MimeComplianceViolation.RepeatedMessageId, 20 },
			{ MimeComplianceViolation.RepeatedInReplyTo, 21 },
			{ MimeComplianceViolation.RepeatedReferences, 22 },
			{ MimeComplianceViolation.RepeatedSubject, 23 },
			{ MimeComplianceViolation.RepeatedReturnPath, 24 },
			{ MimeComplianceViolation.RepeatedResentDate, 25 },
			{ MimeComplianceViolation.RepeatedResentFrom, 26 },
			{ MimeComplianceViolation.RepeatedResentSender, 27 },
			{ MimeComplianceViolation.RepeatedResentTo, 28 },
			{ MimeComplianceViolation.RepeatedResentCc, 29 },
			{ MimeComplianceViolation.RepeatedResentBcc, 30 },
			{ MimeComplianceViolation.RepeatedResentMessageId, 31 },
			{ MimeComplianceViolation.InvalidContentType, 32 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 33 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 34 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 35 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 36 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 37 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 38 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 39 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 40 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 41 },
			{ MimeComplianceViolation.UnquotedDisplayName, 42 },
			{ MimeComplianceViolation.AddressInDisplayName, 43 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 44 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 45 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 46 },
			{ MimeComplianceViolation.InvalidLocalPart, 47 },
			{ MimeComplianceViolation.MissingAddressSeparator, 48 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 49 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 50 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 51 },
			{ MimeComplianceViolation.AddressWithoutDomain, 52 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 53 },
			{ MimeComplianceViolation.InvalidDomain, 54 },
			{ MimeComplianceViolation.TrailingDotInDomain, 55 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 56 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 57 },
			{ MimeComplianceViolation.Invalid8BitAddress, 58 },
			{ MimeComplianceViolation.MissingGroupTerminator, 59 },
			{ MimeComplianceViolation.NonConformantAddress, 60 },
			{ MimeComplianceViolation.NullByteInAddress, 61 },
			{ MimeComplianceViolation.NullByteInDisplayName, 62 },
			{ MimeComplianceViolation.LineBreakInAddress, 63 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 64 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 65 },
			{ MimeComplianceViolation.EmptyGroupName, 66 },
			{ MimeComplianceViolation.MissingBodySeparator, 67 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 68 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 69 },
			{ MimeComplianceViolation.InvalidBase64Character, 70 },
			{ MimeComplianceViolation.InvalidBase64Padding, 71 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 72 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 73 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 74 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 75 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 76 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 77 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 78 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 79 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 80 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 81 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 82 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 83 },
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
