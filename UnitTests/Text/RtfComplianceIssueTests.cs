//
// RtfComplianceIssueTests.cs
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
	public class RtfComplianceIssueTests
	{
		static readonly RtfComplianceViolation[] AllViolations = Enum.GetValues<RtfComplianceViolation> ().Where (v => v != RtfComplianceViolation.None).ToArray ();

		[Test]
		public void TestDefaultInstanceIsDistinguishableFromARealIssue ()
		{
			var issue = default (RtfComplianceIssue);

			Assert.That (issue.Violation, Is.EqualTo (RtfComplianceViolation.None));
		}

		[Test]
		public void TestConstructorRejectsViolationsThatAreNotReportable ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue (RtfComplianceViolation.None, 0, 1, 1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue ((RtfComplianceViolation) (-1), 0, 1, 1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue ((RtfComplianceViolation) 999, 0, 1, 1, 0));

			foreach (var violation in AllViolations)
				Assert.DoesNotThrow (() => new RtfComplianceIssue (violation, 0, 1, 1, 0), $"{violation} should be constructible.");
		}

		[Test]
		public void TestConstructorRejectsInvalidLocations ()
		{
			const RtfComplianceViolation violation = RtfComplianceViolation.TruncatedStream;

			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue (violation, -1, 1, 1, 0)).ParamName, Is.EqualTo ("offset"));
			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue (violation, 0, 0, 1, 0)).ParamName, Is.EqualTo ("lineNumber"));
			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue (violation, 0, 1, 0, 0)).ParamName, Is.EqualTo ("linePosition"));
			Assert.That (Assert.Throws<ArgumentOutOfRangeException> (() => new RtfComplianceIssue (violation, 0, 1, 1, -1)).ParamName, Is.EqualTo ("depth"));
		}

		[Test]
		public void TestProperties ()
		{
			var issue = new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 1234, 5, 6, 2);

			Assert.That (issue.Violation, Is.EqualTo (RtfComplianceViolation.InvalidHexEscape), "Violation");
			Assert.That (issue.Offset, Is.EqualTo (1234), "Offset");
			Assert.That (issue.LineNumber, Is.EqualTo (5), "LineNumber");
			Assert.That (issue.LinePosition, Is.EqualTo (6), "LinePosition");
			Assert.That (issue.Depth, Is.EqualTo (2), "Depth");
		}

		[Test]
		public void TestLargestViolationMatchesTheConstructorBound ()
		{
			// Note: The RtfComplianceIssue constructor range check and the RtfCappedComplianceLogger counter
			// array are written in terms of the largest defined violation. If a new violation is appended,
			// both must be updated to match.
			var max = Enum.GetValues<RtfComplianceViolation> ().Where (v => v != RtfComplianceViolation.TooManyComplianceIssues).Max ();

			Assert.That (max, Is.EqualTo (RtfComplianceViolation.GroupDepthLimitExceeded));
		}

		[Test]
		public void TestEquality ()
		{
			var issue = new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 10, 2, 3, 1);
			var same = new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 10, 2, 3, 1);

			Assert.That (issue.Equals (same), Is.True, "Equals");
			Assert.That (issue.Equals ((object) same), Is.True, "Equals(object)");
			Assert.That (issue == same, Is.True, "==");
			Assert.That (issue != same, Is.False, "!=");
			Assert.That (issue.GetHashCode (), Is.EqualTo (same.GetHashCode ()), "GetHashCode");
			Assert.That (issue.Equals ("not an issue"), Is.False, "Equals(string)");

			var different = new[] {
				new RtfComplianceIssue (RtfComplianceViolation.InvalidParameter, 10, 2, 3, 1),
				new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 11, 2, 3, 1),
				new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 10, 3, 3, 1),
				new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 10, 2, 4, 1),
				new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 10, 2, 3, 2),
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
				Assert.That (RtfComplianceIssue.GetDescription (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestEveryViolationHasRemarks ()
		{
			foreach (var violation in AllViolations)
				Assert.That (RtfComplianceIssue.GetRemarks (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestDescriptionsAreUnique ()
		{
			var seen = new Dictionary<string, RtfComplianceViolation> ();

			foreach (var violation in AllViolations) {
				var description = RtfComplianceIssue.GetDescription (violation);

				Assert.That (seen.ContainsKey (description), Is.False, $"{violation} and {(seen.TryGetValue (description, out var other) ? other.ToString () : null)} share a description.");
				seen.Add (description, violation);
			}
		}

		[Test]
		public void TestEveryViolationHasASeverityAndCategory ()
		{
			foreach (var violation in AllViolations) {
				Assert.DoesNotThrow (() => RtfComplianceIssue.GetSeverity (violation), violation.ToString ());
				Assert.That (RtfComplianceIssue.GetCategories (violation), Is.Not.EqualTo (MimeComplianceCategories.None), violation.ToString ());
			}
		}

		[Test]
		public void TestLimitViolationsAreSecurityRelevant ()
		{
			// Note: Content that went unprocessed because of a limit is content that went unscanned.
			var limits = new[] {
				RtfComplianceViolation.FontTableLimitExceeded,
				RtfComplianceViolation.ColorTableLimitExceeded,
				RtfComplianceViolation.GroupDepthLimitExceeded,
			};

			foreach (var violation in limits) {
				var categories = RtfComplianceIssue.GetCategories (violation);

				Assert.That (RtfComplianceIssue.GetSeverity (violation), Is.EqualTo (MimeComplianceSeverity.Major), violation.ToString ());
				Assert.That (categories & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), violation.ToString ());
				Assert.That (categories & MimeComplianceCategories.DataLoss, Is.EqualTo (MimeComplianceCategories.DataLoss), violation.ToString ());
			}

			Assert.That (RtfComplianceIssue.GetSeverity (RtfComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (RtfComplianceIssue.GetCategories (RtfComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceCategories.Security));
		}

		[Test]
		public void TestCriticalViolationsAreAllSecurityIssues ()
		{
			foreach (var violation in AllViolations) {
				if (RtfComplianceIssue.GetSeverity (violation) != MimeComplianceSeverity.Critical)
					continue;

				Assert.That (RtfComplianceIssue.GetCategories (violation) & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), $"{violation} is Critical but is not categorized as a Security issue.");
			}
		}

		[Test]
		public void TestStaticMethodsThrowOnInvalidViolation ()
		{
			foreach (var invalid in new[] { (RtfComplianceViolation) 9999, RtfComplianceViolation.None }) {
				Assert.Throws<ArgumentOutOfRangeException> (() => RtfComplianceIssue.GetSeverity (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => RtfComplianceIssue.GetCategories (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => RtfComplianceIssue.GetDescription (invalid));
				Assert.Throws<ArgumentOutOfRangeException> (() => RtfComplianceIssue.GetRemarks (invalid));
			}
		}

		[Test]
		public void TestPropertiesMatchStaticMethods ()
		{
			foreach (var violation in AllViolations) {
				var issue = new RtfComplianceIssue (violation, 0, 1, 1, 0);

				Assert.That (issue.Severity, Is.EqualTo (RtfComplianceIssue.GetSeverity (violation)), $"{violation} Severity");
				Assert.That (issue.Categories, Is.EqualTo (RtfComplianceIssue.GetCategories (violation)), $"{violation} Categories");
				Assert.That (issue.Description, Is.EqualTo (RtfComplianceIssue.GetDescription (violation)), $"{violation} Description");
				Assert.That (issue.Remarks, Is.EqualTo (RtfComplianceIssue.GetRemarks (violation)), $"{violation} Remarks");
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.That (new RtfComplianceIssue (RtfComplianceViolation.MissingRtfHeader, 0, 1, 1, 0).ToString (), Is.EqualTo ("MissingRtfHeader at line 1, position 1 (offset 0)"));
			Assert.That (new RtfComplianceIssue (RtfComplianceViolation.InvalidHexEscape, 1234, 5, 6, 2).ToString (), Is.EqualTo ("InvalidHexEscape at line 5, position 6 (offset 1234) (depth 2)"));
		}
	}
}