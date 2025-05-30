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
			{ MimeComplianceViolation.BareLinefeedInHeader, 0 },
			{ MimeComplianceViolation.BareLinefeedInBody, 1 },
			{ MimeComplianceViolation.InvalidHeader, 2 },
			{ MimeComplianceViolation.IncompleteHeader, 3 },
			{ MimeComplianceViolation.InvalidContentType, 4 },
			{ MimeComplianceViolation.MultipleContentTypes, 5 },
			{ MimeComplianceViolation.InvalidContentTransferEncoding, 6 },
			{ MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 7 },
			{ MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 8 },
			{ MimeComplianceViolation.MultipleContentTransferEncodings, 9 },
			{ MimeComplianceViolation.InvalidWrapping, 10 },
			{ MimeComplianceViolation.MissingBodySeparator, 11 },
			{ MimeComplianceViolation.MissingMultipartBoundaryParameter, 12 },
			{ MimeComplianceViolation.InvalidMultipartBoundaryParameter, 13 },
			{ MimeComplianceViolation.MissingMultipartBoundary, 14 },
			{ MimeComplianceViolation.Unexpected8BitBytesInHeader, 15 },
			{ MimeComplianceViolation.Unexpected8BitBytesInBody, 16 },
			{ MimeComplianceViolation.UnexpectedNullBytesInHeader, 17 },
			{ MimeComplianceViolation.UnexpectedNullBytesInBody, 18 },
			{ MimeComplianceViolation.IncompleteBase64Quantum, 19 },
			{ MimeComplianceViolation.InvalidBase64Character, 20 },
			{ MimeComplianceViolation.InvalidBase64Padding, 21 },
			{ MimeComplianceViolation.Base64CharactersAfterPadding, 22 },
			{ MimeComplianceViolation.ObsoleteBase64Comment, 23 },
			{ MimeComplianceViolation.InvalidQuotedPrintableEncoding, 24 },
			{ MimeComplianceViolation.InvalidQuotedPrintableSoftBreak, 25 },
			{ MimeComplianceViolation.InvalidUUEncodePretext, 26 },
			{ MimeComplianceViolation.InvalidUUEncodeFileMode, 27 },
			{ MimeComplianceViolation.InvalidUUEncodedContent, 28 },
			{ MimeComplianceViolation.InvalidUUEncodedLineLength, 29 },
			{ MimeComplianceViolation.IncompleteUUEncodedLine, 30 },
			{ MimeComplianceViolation.InvalidUUEncodedLineExtraData, 31 },
			{ MimeComplianceViolation.InvalidUUEncodeEndMarker, 32 },
			{ MimeComplianceViolation.IncompleteUUEncodedContent, 33 }
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
