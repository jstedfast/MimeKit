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
			{ MimeComplianceViolation.InvalidHeader, 3 },
			{ MimeComplianceViolation.IncompleteHeader, 4 },
			{ MimeComplianceViolation.InvalidContentType, 5 },
			{ MimeComplianceViolation.MultipleContentTypes, 6 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 7 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 8 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 9 },
			{ MimeComplianceViolation.MultipleContentTransferEncodings, 10 },
			{ MimeComplianceViolation.OversizedLine, 11 },
			{ MimeComplianceViolation.MissingBodySeparator, 12 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 13 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 14 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 15 },
			{ MimeComplianceViolation.Unexpected8BitBytesInHeader, 16 },
			{ MimeComplianceViolation.Unexpected8BitBytesInBody, 17 },
			{ MimeComplianceViolation.UnexpectedNullBytesInHeader, 18 },
			{ MimeComplianceViolation.UnexpectedNullBytesInBody, 19 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 20 },
			{ MimeComplianceViolation.InvalidBase64Character, 21 },
			{ MimeComplianceViolation.InvalidBase64Padding, 22 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 23 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 24 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 25 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 26 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 27 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 28 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 29 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 30 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 31 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 32 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 33 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 34 },
			{ MimeComplianceViolation.ExcessiveAngleBracketsInAddress, 35 },
			{ MimeComplianceViolation.UnbalancedAngleBracketsInAddress, 36 },
			{ MimeComplianceViolation.UnbalancedQuotesInAddress, 37 },
			{ MimeComplianceViolation.UnbalancedParenthesesInAddress, 38 },
			{ MimeComplianceViolation.UnquotedDisplayName, 39 },
			{ MimeComplianceViolation.InvalidLocalPart, 40 },
			{ MimeComplianceViolation.MissingAddressSeparator, 41 },
			{ MimeComplianceViolation.ExtraneousCommaInAddressList, 42 },
			{ MimeComplianceViolation.ObsoleteRouteAddress, 43 },
			{ MimeComplianceViolation.AddressWithoutDomain, 44 },
			{ MimeComplianceViolation.ObsoleteDomainSyntax, 45 },
			{ MimeComplianceViolation.TrailingDotInDomain, 46 },
			{ MimeComplianceViolation.WhitespaceInDomainLiteral, 47 },
			{ MimeComplianceViolation.Invalid8BitAddress, 48 },
			{ MimeComplianceViolation.MissingGroupTerminator, 49 },
			{ MimeComplianceViolation.NonConformantAddress, 50 },
			{ MimeComplianceViolation.NullByteInAddress, 51 },
			{ MimeComplianceViolation.LineBreakInAddress, 52 },
			{ MimeComplianceViolation.ControlCharacterInAddress, 53 },
			{ MimeComplianceViolation.EmptyGroupName, 54 }
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
