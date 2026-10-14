//
// HtmlComplianceIssueTests.cs
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
using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlComplianceIssueTests
	{
		static readonly HtmlComplianceViolation[] AllViolations = Enum.GetValues<HtmlComplianceViolation> ().Where (v => v != HtmlComplianceViolation.None).ToArray ();

		[Test]
		public void TestDefaultInstanceIsDistinguishableFromARealIssue ()
		{
			var issue = default (HtmlComplianceIssue);

			Assert.That (issue.Violation, Is.EqualTo (HtmlComplianceViolation.None));
		}

		[Test]
		public void TestConstructorRejectsViolationsThatAreNotReportable ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue (HtmlComplianceViolation.None, 0, 1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue ((HtmlComplianceViolation) (-1), 0, 1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue ((HtmlComplianceViolation) 999, 0, 1, 1));

			foreach (var violation in AllViolations)
				Assert.DoesNotThrow (() => new HtmlComplianceIssue (violation, 0, 1, 1), $"{violation} should be constructible.");
		}

		[Test]
		public void TestConstructorRejectsInvalidLocations ()
		{
			const HtmlComplianceViolation violation = HtmlComplianceViolation.EofInTag;

			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue (violation, -1, 1, 1)).ParamName, Is.EqualTo ("offset"));
			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue (violation, 0, 0, 1)).ParamName, Is.EqualTo ("lineNumber"));
			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new HtmlComplianceIssue (violation, 0, 1, 0)).ParamName, Is.EqualTo ("linePosition"));
		}

		[Test]
		public void TestProperties ()
		{
			var issue = new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 1234, 5, 6);

			Assert.That (issue.Violation, Is.EqualTo (HtmlComplianceViolation.DuplicateAttribute), "Violation");
			Assert.That (issue.Offset, Is.EqualTo (1234), "Offset");
			Assert.That (issue.LineNumber, Is.EqualTo (5), "LineNumber");
			Assert.That (issue.LinePosition, Is.EqualTo (6), "LinePosition");
		}

		[Test]
		public void TestLargestViolationMatchesTheConstructorBound ()
		{
			// Note: The HtmlComplianceIssue constructor range check and the HtmlCappedComplianceLogger counter
			// array are written in terms of the largest defined violation. If a new violation is appended,
			// both must be updated to match.
			var max = Enum.GetValues<HtmlComplianceViolation> ().Where (v => v != HtmlComplianceViolation.TooManyComplianceIssues).Max ();

			Assert.That (max, Is.EqualTo (HtmlComplianceViolation.ElementDepthLimitExceeded));
		}

		[Test]
		public void TestEquality ()
		{
			var issue = new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 10, 2, 3);
			var same = new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 10, 2, 3);

			Assert.That (issue.Equals (same), Is.True, "Equals");
			Assert.That (issue.Equals ((object) same), Is.True, "Equals(object)");
			Assert.That (issue == same, Is.True, "==");
			Assert.That (issue != same, Is.False, "!=");
			Assert.That (issue.GetHashCode (), Is.EqualTo (same.GetHashCode ()), "GetHashCode");
			Assert.That (issue.Equals ("not an issue"), Is.False, "Equals(string)");

			var different = new[] {
				new HtmlComplianceIssue (HtmlComplianceViolation.MissingAttributeValue, 10, 2, 3),
				new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 11, 2, 3),
				new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 10, 3, 3),
				new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 10, 2, 4),
			};

			foreach (var other in different) {
				Assert.That (issue.Equals (other), Is.False, $"Equals {other}");
				Assert.That (issue == other, Is.False, $"== {other}");
				Assert.That (issue != other, Is.True, $"!= {other}");
			}
		}

		[Test]
		public void TestEveryViolationHasADescription ()
		{
			foreach (var violation in AllViolations)
				Assert.That (HtmlComplianceIssue.GetDescription (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestEveryViolationHasRemarks ()
		{
			foreach (var violation in AllViolations)
				Assert.That (HtmlComplianceIssue.GetRemarks (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestDescriptionsAreUnique ()
		{
			var seen = new Dictionary<string, HtmlComplianceViolation> ();

			foreach (var violation in AllViolations) {
				var description = HtmlComplianceIssue.GetDescription (violation);

				Assert.That (seen.ContainsKey (description), Is.False, $"{violation} and {(seen.TryGetValue (description, out var other) ? other.ToString () : null)} share a description.");
				seen.Add (description, violation);
			}
		}

		[Test]
		public void TestEveryViolationHasASeverityAndCategory ()
		{
			foreach (var violation in AllViolations) {
				Assert.DoesNotThrow (() => HtmlComplianceIssue.GetSeverity (violation), violation.ToString ());
				Assert.That (HtmlComplianceIssue.GetCategories (violation), Is.Not.EqualTo (MimeComplianceCategories.None), violation.ToString ());
			}
		}

		[Test]
		public void TestLimitViolationsAreSecurityRelevant ()
		{
			// Note: Content that went unprocessed because of a limit is content that went unscanned.
			var limits = new[] {
				HtmlComplianceViolation.ElementDepthLimitExceeded,
			};

			foreach (var violation in limits) {
				var categories = HtmlComplianceIssue.GetCategories (violation);

				Assert.That (HtmlComplianceIssue.GetSeverity (violation), Is.EqualTo (MimeComplianceSeverity.Major), violation.ToString ());
				Assert.That (categories & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), violation.ToString ());
				Assert.That (categories & MimeComplianceCategories.DataLoss, Is.EqualTo (MimeComplianceCategories.DataLoss), violation.ToString ());
			}

			Assert.That (HtmlComplianceIssue.GetSeverity (HtmlComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (HtmlComplianceIssue.GetCategories (HtmlComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceCategories.Security));
		}

		[Test]
		public void TestCriticalViolationsAreAllSecurityIssues ()
		{
			foreach (var violation in AllViolations) {
				if (HtmlComplianceIssue.GetSeverity (violation) != MimeComplianceSeverity.Critical)
					continue;

				Assert.That (HtmlComplianceIssue.GetCategories (violation) & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), $"{violation} is Critical but is not categorized as a Security issue.");
			}
		}

		[Test]
		public void TestStaticMethodsThrowOnInvalidViolation ()
		{
			foreach (var invalid in new[] { (HtmlComplianceViolation) 9999, HtmlComplianceViolation.None }) {
				Assert.Throws<ArgumentOutOfRangeException> (() => HtmlComplianceIssue.GetSeverity (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => HtmlComplianceIssue.GetCategories (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => HtmlComplianceIssue.GetDescription (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => HtmlComplianceIssue.GetRemarks (invalid));
			}
		}

		[Test]
		public void TestPropertiesMatchStaticMethods ()
		{
			foreach (var violation in AllViolations) {
				var issue = new HtmlComplianceIssue (violation, 0, 1, 1);

				Assert.That (issue.Severity, Is.EqualTo (HtmlComplianceIssue.GetSeverity (violation)), $"{violation} Severity");
				Assert.That (issue.Categories, Is.EqualTo (HtmlComplianceIssue.GetCategories (violation)), $"{violation} Categories");
				Assert.That (issue.Description, Is.EqualTo (HtmlComplianceIssue.GetDescription (violation)), $"{violation} Description");
				Assert.That (issue.Remarks, Is.EqualTo (HtmlComplianceIssue.GetRemarks (violation)), $"{violation} Remarks");
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.That (new HtmlComplianceIssue (HtmlComplianceViolation.EofInTag, 0, 1, 1).ToString (), Is.EqualTo ("EofInTag at line 1, position 1 (offset 0)"));
			Assert.That (new HtmlComplianceIssue (HtmlComplianceViolation.DuplicateAttribute, 1234, 5, 6).ToString (), Is.EqualTo ("DuplicateAttribute at line 5, position 6 (offset 1234)"));
		}
	}
}