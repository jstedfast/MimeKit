//
// TnefComplianceViolationTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefComplianceViolationTests
	{
		// Note: This table pins the numeric value of every TnefComplianceViolation. The values are
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
		static readonly Dictionary<TnefComplianceViolation, int> ExpectedValues = new () {
			{ TnefComplianceViolation.None, 0 },
			{ TnefComplianceViolation.InvalidSignature, 1 },
			{ TnefComplianceViolation.UnsupportedVersion, 2 },
			{ TnefComplianceViolation.InvalidMessageCodepage, 3 },
			{ TnefComplianceViolation.InvalidMessageClass, 4 },
			{ TnefComplianceViolation.InvalidAttributeLevel, 5 },
			{ TnefComplianceViolation.MessageAttributeAfterAttachment, 6 },
			{ TnefComplianceViolation.AttributeLevelMismatch, 7 },
			{ TnefComplianceViolation.UnknownAttribute, 8 },
			{ TnefComplianceViolation.InvalidAttributeLength, 9 },
			{ TnefComplianceViolation.AttributeChecksumMismatch, 10 },
			{ TnefComplianceViolation.InvalidAttributeValue, 11 },
			{ TnefComplianceViolation.InvalidDate, 12 },
			{ TnefComplianceViolation.TruncatedStream, 13 },
			{ TnefComplianceViolation.InvalidPropertyCount, 14 },
			{ TnefComplianceViolation.InvalidRowCount, 15 },
			{ TnefComplianceViolation.InvalidValueCount, 16 },
			{ TnefComplianceViolation.InvalidPropertyLength, 17 },
			{ TnefComplianceViolation.UnsupportedPropertyType, 18 },
			{ TnefComplianceViolation.InvalidNamedPropertyKind, 19 },
			{ TnefComplianceViolation.NestingTooDeep, 20 },
			{ TnefComplianceViolation.TooManyAttachments, 21 },
			{ TnefComplianceViolation.DataSizeLimitExceeded, 22 },
			{ TnefComplianceViolation.TooManyComplianceIssues, int.MaxValue },
		};

		[Test]
		public void TestNumericValuesAreStable ()
		{
			// Note: Removing a violation does not need to be asserted here; it breaks the compilation
			// of ExpectedValues, which is a louder failure than any assertion.
			foreach (var violation in Enum.GetValues<TnefComplianceViolation> ()) {
				Assert.That (ExpectedValues.ContainsKey (violation), Is.True,
					$"{violation} is missing from the expected value table. If it is a new violation, make sure it took the next unused value and then add it here as {{ TnefComplianceViolation.{violation}, {(int) violation} }}.");

				Assert.That ((int) violation, Is.EqualTo (ExpectedValues[violation]),
					$"The numeric value of {violation} changed from {ExpectedValues[violation]} to {(int) violation}. This is a breaking change for callers compiled against an earlier version of MimeKit and for any persisted value.");
			}
		}

		[Test]
		public void TestValuesAreUnique ()
		{
			var seen = new Dictionary<int, TnefComplianceViolation> ();

			foreach (var violation in Enum.GetValues<TnefComplianceViolation> ()) {
				var value = (int) violation;

				Assert.That (seen.ContainsKey (value), Is.False, $"{violation} and {(seen.TryGetValue (value, out var other) ? other.ToString () : null)} share the value {value}.");
				seen.Add (value, violation);
			}
		}

		[Test]
		public void TestNewViolationsTakeTheNextUnusedValue ()
		{
			// Note: TooManyComplianceIssues is not a TNEF defect and is parked at the end of the range
			// on purpose, so that the defect values stay contiguous and free to grow.
			var values = Enum.GetValues<TnefComplianceViolation> ().Where (v => v != TnefComplianceViolation.TooManyComplianceIssues).Select (v => (int) v).ToList ();

			Assert.That (values.Min (), Is.EqualTo (0), "The first violation should have the value 0.");
			Assert.That (values.Max (), Is.EqualTo (values.Count - 1), "The values should be contiguous, so a new violation must take the next unused value rather than reusing or displacing an existing one.");
		}
	}
}
