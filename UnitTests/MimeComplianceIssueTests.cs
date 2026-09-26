//
// MimeComplianceIssueTests.cs
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
	public class MimeComplianceIssueTests
	{
		static IEnumerable<MimeComplianceViolation> AllViolations => Enum.GetValues<MimeComplianceViolation> ();

		[Test]
		public void TestEveryViolationHasADescription ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetDescription().
			foreach (var violation in AllViolations) {
				var description = MimeComplianceIssue.GetDescription (violation);

				Assert.That (description, Is.Not.Null.And.Not.Empty, $"{violation} has no description.");
				Assert.That (description, Does.EndWith ("."), $"{violation} description should be a sentence.");
			}
		}

		[Test]
		public void TestEveryViolationHasRemarks ()
		{
			// Note: This guards against a new MimeComplianceViolation being added without a
			// corresponding entry in MimeComplianceIssue.GetRemarks().
			foreach (var violation in AllViolations) {
				var remarks = MimeComplianceIssue.GetRemarks (violation);

				Assert.That (remarks, Is.Not.Null.And.Not.Empty, $"{violation} has no remarks.");
				Assert.That (remarks, Does.EndWith ("."), $"{violation} remarks should be a sentence.");
			}
		}

		[Test]
		public void TestDescriptionsAreUnique ()
		{
			var seen = new Dictionary<string, MimeComplianceViolation> ();

			foreach (var violation in AllViolations) {
				var description = MimeComplianceIssue.GetDescription (violation);

				// Note: A duplicated description is the signature of a copy/paste error.
				Assert.That (seen.ContainsKey (description), Is.False, $"{violation} and {(seen.TryGetValue (description, out var other) ? other.ToString () : null)} share a description.");
				seen.Add (description, violation);
			}
		}

		[Test]
		public void TestGetDescriptionAndGetRemarksThrowOnInvalidViolation ()
		{
			var invalid = (MimeComplianceViolation) 9999;

			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetDescription (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeComplianceIssue.GetRemarks (invalid));
		}

		[Test]
		public void TestPropertiesMatchStaticMethods ()
		{
			foreach (var violation in AllViolations) {
				var issue = new MimeComplianceIssue (violation, 123, 4);

				Assert.That (issue.Violation, Is.EqualTo (violation));
				Assert.That (issue.StreamOffset, Is.EqualTo (123));
				Assert.That (issue.LineNumber, Is.EqualTo (4));
				Assert.That (issue.ColumnNumber, Is.EqualTo (0), "ColumnNumber should default to 0 (unknown).");
				Assert.That (issue.Description, Is.EqualTo (MimeComplianceIssue.GetDescription (violation)));
				Assert.That (issue.Remarks, Is.EqualTo (MimeComplianceIssue.GetRemarks (violation)));
			}
		}

		[Test]
		public void TestToString ()
		{
			var withoutColumn = new MimeComplianceIssue (MimeComplianceViolation.BareLinefeedInHeader, 42, 7);
			var withColumn = new MimeComplianceIssue (MimeComplianceViolation.BareLinefeedInHeader, 42, 7, 13);

			Assert.That (withoutColumn.ToString (), Is.EqualTo ("BareLinefeedInHeader at line 7 (offset 42)"));
			Assert.That (withColumn.ToString (), Is.EqualTo ("BareLinefeedInHeader at line 7, column 13 (offset 42)"));
		}
	}
}
