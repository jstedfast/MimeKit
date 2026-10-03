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
			{ MimeComplianceViolation.BareLinefeedInHeader, 1 },
			{ MimeComplianceViolation.BareLinefeedInBody, 2 },
			{ MimeComplianceViolation.OversizedLine, 3 },
			{ MimeComplianceViolation.Unexpected8BitBytesInHeader, 4 },
			{ MimeComplianceViolation.Unexpected8BitBytesInBody, 5 },
			{ MimeComplianceViolation.UnexpectedNullBytesInHeader, 6 },
			{ MimeComplianceViolation.UnexpectedNullBytesInBody, 7 },
			{ MimeComplianceViolation.InvalidHeader, 8 },
			{ MimeComplianceViolation.IncompleteHeader, 9 },
			{ MimeComplianceViolation.RepeatedContentType, 10 },
			{ MimeComplianceViolation.RepeatedContentTransferEncoding, 11 },
			{ MimeComplianceViolation.RepeatedDate, 12 },
			{ MimeComplianceViolation.RepeatedFrom, 13 },
			{ MimeComplianceViolation.RepeatedSender, 14 },
			{ MimeComplianceViolation.RepeatedReplyTo, 15 },
			{ MimeComplianceViolation.RepeatedTo, 16 },
			{ MimeComplianceViolation.RepeatedCc, 17 },
			{ MimeComplianceViolation.RepeatedBcc, 18 },
			{ MimeComplianceViolation.RepeatedMessageId, 19 },
			{ MimeComplianceViolation.RepeatedInReplyTo, 20 },
			{ MimeComplianceViolation.RepeatedReferences, 21 },
			{ MimeComplianceViolation.RepeatedSubject, 22 },
			{ MimeComplianceViolation.RepeatedReturnPath, 23 },
			{ MimeComplianceViolation.RepeatedResentDate, 24 },
			{ MimeComplianceViolation.RepeatedResentFrom, 25 },
			{ MimeComplianceViolation.RepeatedResentSender, 26 },
			{ MimeComplianceViolation.RepeatedResentTo, 27 },
			{ MimeComplianceViolation.RepeatedResentCc, 28 },
			{ MimeComplianceViolation.RepeatedResentBcc, 29 },
			{ MimeComplianceViolation.RepeatedResentMessageId, 30 },
			{ MimeComplianceViolation.InvalidContentType, 31 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 32 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 33 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 34 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 35 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 36 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 37 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 38 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 39 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 40 },
			{ MimeComplianceViolation.UnquotedDisplayName, 41 },
			{ MimeComplianceViolation.AddressInDisplayName, 42 },
			{ MimeComplianceViolation.UnquotedAddressInDisplayName, 43 },
			{ MimeComplianceViolation.AddressInGroupDisplayName, 44 },
			{ MimeComplianceViolation.UnquotedAddressInGroupDisplayName, 45 },
			{ MimeComplianceViolation.InvalidLocalPart, 46 },
			{ MimeComplianceViolation.MissingAddressSeparator, 47 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 48 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 49 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 50 },
			{ MimeComplianceViolation.AddressWithoutDomain, 51 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 52 },
			{ MimeComplianceViolation.InvalidDomain, 53 },
			{ MimeComplianceViolation.TrailingDotInDomain, 54 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 55 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 56 },
			{ MimeComplianceViolation.Invalid8BitAddress, 57 },
			{ MimeComplianceViolation.MissingGroupTerminator, 58 },
			{ MimeComplianceViolation.NonConformantAddress, 59 },
			{ MimeComplianceViolation.NullByteInAddress, 60 },
			{ MimeComplianceViolation.NullByteInDisplayName, 61 },
			{ MimeComplianceViolation.LineBreakInAddress, 62 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 63 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 64 },
			{ MimeComplianceViolation.EmptyGroupName, 65 },
			{ MimeComplianceViolation.MissingBodySeparator, 66 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 67 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 68 },
			{ MimeComplianceViolation.InvalidBase64Character, 69 },
			{ MimeComplianceViolation.InvalidBase64Padding, 70 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 71 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 72 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 73 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 74 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 75 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 76 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 77 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 78 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 79 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 80 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 81 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 82 },
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
