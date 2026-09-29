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
			{ MimeComplianceViolation.AddressInGroupDisplayName, 43 },
			{ MimeComplianceViolation.InvalidLocalPart, 44 },
			{ MimeComplianceViolation.MissingAddressSeparator, 45 },
			{ MimeComplianceViolation.AmbiguousMailboxBoundary, 46 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 47 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 48 },
			{ MimeComplianceViolation.AddressWithoutDomain, 49 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 50 },
			{ MimeComplianceViolation.TrailingDotInDomain, 51 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 52 },
			{ MimeComplianceViolation.InvalidCharacterInDomainLiteral, 53 },
			{ MimeComplianceViolation.Invalid8BitAddress, 54 },
			{ MimeComplianceViolation.MissingGroupTerminator, 55 },
			{ MimeComplianceViolation.NonConformantAddress, 56 },
			{ MimeComplianceViolation.NullByteInAddress, 57 },
			{ MimeComplianceViolation.NullByteInDisplayName, 58 },
			{ MimeComplianceViolation.LineBreakInAddress, 59 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 60 },
			{ MimeComplianceViolation.Iso2022SequenceInLocalPart, 61 },
			{ MimeComplianceViolation.EmptyGroupName, 62 },
			{ MimeComplianceViolation.MissingBodySeparator, 63 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 64 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 65 },
			{ MimeComplianceViolation.InvalidBase64Character, 66 },
			{ MimeComplianceViolation.InvalidBase64Padding, 67 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 68 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 69 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 70 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 71 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 72 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 73 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 74 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 75 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 76 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 77 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 78 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 79 },
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
			var values = Enum.GetValues<MimeComplianceViolation> ().Select (v => (int) v).ToList ();

			Assert.That (values.Min (), Is.EqualTo (0), "The first violation should have the value 0.");
			Assert.That (values.Max (), Is.EqualTo (values.Count - 1), "The values should be contiguous, so a new violation must take the next unused value rather than reusing or displacing an existing one.");
		}
	}
}
