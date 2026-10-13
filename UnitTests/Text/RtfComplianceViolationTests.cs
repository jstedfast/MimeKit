//
// RtfComplianceViolationTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class RtfComplianceViolationTests
	{
		// Note: This table pins the numeric value of every RtfComplianceViolation. The values are public
		// contract (see TnefComplianceViolationTests for the full rationale). A new violation must take the
		// next unused value; an unreleased one may be inserted into its group as long as this table is updated
		// in the same commit.
		static readonly Dictionary<RtfComplianceViolation, int> ExpectedValues = new () {
			{ RtfComplianceViolation.None, 0 },
			{ RtfComplianceViolation.MissingRtfHeader, 1 },
			{ RtfComplianceViolation.UnbalancedGroupEnd, 2 },
			{ RtfComplianceViolation.ContentAfterDocument, 3 },
			{ RtfComplianceViolation.TruncatedStream, 4 },
			{ RtfComplianceViolation.KeywordTooLong, 5 },
			{ RtfComplianceViolation.InvalidParameter, 6 },
			{ RtfComplianceViolation.InvalidHexEscape, 7 },
			{ RtfComplianceViolation.UnescapedEightBitCharacter, 8 },
			{ RtfComplianceViolation.UnpairedSurrogate, 9 },
			{ RtfComplianceViolation.UnsupportedCodePage, 10 },
			{ RtfComplianceViolation.UndefinedFont, 11 },
			{ RtfComplianceViolation.UndefinedColor, 12 },
			{ RtfComplianceViolation.InvalidFontSize, 13 },
			{ RtfComplianceViolation.FieldInstructionTooLong, 14 },
			{ RtfComplianceViolation.FontTableLimitExceeded, 15 },
			{ RtfComplianceViolation.ColorTableLimitExceeded, 16 },
			{ RtfComplianceViolation.GroupDepthLimitExceeded, 17 },
			{ RtfComplianceViolation.TooManyComplianceIssues, int.MaxValue },
		};

		[Test]
		public void TestNumericValuesAreStable ()
		{
			foreach (var violation in Enum.GetValues<RtfComplianceViolation> ()) {
				Assert.That (ExpectedValues.ContainsKey (violation), Is.True,
					$"{violation} is missing from the expected value table. If it is a new violation, make sure it took the next unused value and then add it here as {{ RtfComplianceViolation.{violation}, {(int) violation} }}.");

				Assert.That ((int) violation, Is.EqualTo (ExpectedValues[violation]),
					$"The numeric value of {violation} changed from {ExpectedValues[violation]} to {(int) violation}. This is a breaking change for callers compiled against an earlier version of MimeKit and for any persisted value.");
			}
		}

		[Test]
		public void TestValuesAreUnique ()
		{
			var seen = new Dictionary<int, RtfComplianceViolation> ();

			foreach (var violation in Enum.GetValues<RtfComplianceViolation> ()) {
				var value = (int) violation;

				Assert.That (seen.ContainsKey (value), Is.False, $"{violation} and {(seen.TryGetValue (value, out var other) ? other.ToString () : null)} share the value {value}.");
				seen.Add (value, violation);
			}
		}

		[Test]
		public void TestNewViolationsTakeTheNextUnusedValue ()
		{
			var values = Enum.GetValues<RtfComplianceViolation> ().Where (v => v != RtfComplianceViolation.TooManyComplianceIssues).Select (v => (int) v).ToList ();

			Assert.That (values.Min (), Is.EqualTo (0), "The first violation should have the value 0.");
			Assert.That (values.Max (), Is.EqualTo (values.Count - 1), "The values should be contiguous, so a new violation must take the next unused value rather than reusing or displacing an existing one.");
		}
	}
}