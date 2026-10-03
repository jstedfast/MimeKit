//
// TnefComplianceIssueTests.cs
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
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefComplianceIssueTests
	{
		static readonly TnefComplianceViolation[] AllViolations = Enum.GetValues<TnefComplianceViolation> ().Where (v => v != TnefComplianceViolation.None).ToArray ();

		[Test]
		public void TestDefaultInstanceIsDistinguishableFromARealIssue ()
		{
			var issue = default (TnefComplianceIssue);

			Assert.That (issue.Violation, Is.EqualTo (TnefComplianceViolation.None));
		}

		[Test]
		public void TestConstructorRejectsViolationsThatAreNotReportable ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefComplianceIssue (TnefComplianceViolation.None, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefComplianceIssue ((TnefComplianceViolation) (-1), 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefComplianceIssue ((TnefComplianceViolation) 999, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefComplianceIssue (TnefComplianceViolation.None, 0, 0, TnefAttributeTag.Null, TnefPropertyTag.Null));

			foreach (var violation in AllViolations)
				Assert.DoesNotThrow (() => new TnefComplianceIssue (violation, 0), $"{violation} should be constructible.");
		}

		[Test]
		public void TestConstructorRejectsNegativeDepth ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefComplianceIssue (TnefComplianceViolation.TruncatedStream, 0, -1, TnefAttributeTag.Null, TnefPropertyTag.Null));
		}

		[Test]
		public void TestShortConstructorDefaults ()
		{
			var issue = new TnefComplianceIssue (TnefComplianceViolation.InvalidSignature, 0);

			Assert.That (issue.StreamOffset, Is.EqualTo (0), "StreamOffset");
			Assert.That (issue.Depth, Is.EqualTo (0), "Depth");
			Assert.That (issue.AttributeTag, Is.EqualTo (TnefAttributeTag.Null), "AttributeTag");
			Assert.That (issue.PropertyTag, Is.EqualTo (TnefPropertyTag.Null), "PropertyTag");
		}

		[Test]
		public void TestProperties ()
		{
			var issue = new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 1234, 2, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW);

			Assert.That (issue.Violation, Is.EqualTo (TnefComplianceViolation.InvalidPropertyLength), "Violation");
			Assert.That (issue.StreamOffset, Is.EqualTo (1234), "StreamOffset");
			Assert.That (issue.Depth, Is.EqualTo (2), "Depth");
			Assert.That (issue.AttributeTag, Is.EqualTo (TnefAttributeTag.MapiProperties), "AttributeTag");
			Assert.That (issue.PropertyTag, Is.EqualTo (TnefPropertyTag.SubjectW), "PropertyTag");
		}

		[Test]
		public void TestLargestViolationMatchesTheConstructorBound ()
		{
			// Note: The TnefComplianceIssue constructor range check and the TnefCappedComplianceLogger
			// counter array are written in terms of the largest defined violation. If a new violation is
			// appended, both must be updated to match.
			var max = Enum.GetValues<TnefComplianceViolation> ().Where (v => v != TnefComplianceViolation.TooManyComplianceIssues).Max ();

			Assert.That (max, Is.EqualTo (TnefComplianceViolation.DataSizeLimitExceeded));
		}

		[Test]
		public void TestEquality ()
		{
			var issue = new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW);
			var same = new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW);

			Assert.That (issue.Equals (same), Is.True, "Equals");
			Assert.That (issue.Equals ((object) same), Is.True, "Equals(object)");
			Assert.That (issue == same, Is.True, "==");
			Assert.That (issue != same, Is.False, "!=");
			Assert.That (issue.GetHashCode (), Is.EqualTo (same.GetHashCode ()), "GetHashCode");
			Assert.That (issue.Equals ("not an issue"), Is.False, "Equals(string)");

			var different = new[] {
				new TnefComplianceIssue (TnefComplianceViolation.InvalidValueCount, 10, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW),
				new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 11, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW),
				new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 2, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW),
				new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 1, TnefAttributeTag.Attachment, TnefPropertyTag.SubjectW),
				new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.BodyW),
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
				Assert.That (TnefComplianceIssue.GetDescription (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestEveryViolationHasRemarks ()
		{
			foreach (var violation in AllViolations)
				Assert.That (TnefComplianceIssue.GetRemarks (violation), Is.Not.Null.And.Not.Empty, violation.ToString ());
		}

		[Test]
		public void TestDescriptionsAreUnique ()
		{
			var seen = new Dictionary<string, TnefComplianceViolation> ();

			foreach (var violation in AllViolations) {
				var description = TnefComplianceIssue.GetDescription (violation);

				Assert.That (seen.ContainsKey (description), Is.False, $"{violation} and {(seen.TryGetValue (description, out var other) ? other.ToString () : null)} share a description.");
				seen.Add (description, violation);
			}
		}

		[Test]
		public void TestEveryViolationHasASeverity ()
		{
			foreach (var violation in AllViolations)
				Assert.DoesNotThrow (() => TnefComplianceIssue.GetSeverity (violation), violation.ToString ());
		}

		[Test]
		public void TestEveryViolationHasACategory ()
		{
			foreach (var violation in AllViolations)
				Assert.That (TnefComplianceIssue.GetCategories (violation), Is.Not.EqualTo (MimeComplianceCategories.None), violation.ToString ());
		}

		[Test]
		public void TestSeverityAssignments ()
		{
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.UnsupportedVersion), Is.EqualTo (MimeComplianceSeverity.Minor));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.AttributeChecksumMismatch), Is.EqualTo (MimeComplianceSeverity.Minor));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.MessageAttributeAfterAttachment), Is.EqualTo (MimeComplianceSeverity.Critical));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.AttributeLevelMismatch), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.TruncatedStream), Is.EqualTo (MimeComplianceSeverity.Major));

			// Note: A consumer that only looks at Major and above must still be told that the report is
			// incomplete or that some content went unprocessed.
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.NestingTooDeep), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.TooManyAttachments), Is.EqualTo (MimeComplianceSeverity.Major));
			Assert.That (TnefComplianceIssue.GetSeverity (TnefComplianceViolation.DataSizeLimitExceeded), Is.EqualTo (MimeComplianceSeverity.Major));
		}

		[Test]
		public void TestCategoryAssignments ()
		{
			Assert.That (TnefComplianceIssue.GetCategories (TnefComplianceViolation.AttributeChecksumMismatch), Is.EqualTo (MimeComplianceCategories.Interoperability));
			Assert.That (TnefComplianceIssue.GetCategories (TnefComplianceViolation.TruncatedStream), Is.EqualTo (MimeComplianceCategories.DataLoss));
			Assert.That (TnefComplianceIssue.GetCategories (TnefComplianceViolation.MessageAttributeAfterAttachment), Is.EqualTo (MimeComplianceCategories.Interoperability | MimeComplianceCategories.Security));
			Assert.That (TnefComplianceIssue.GetCategories (TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (MimeComplianceCategories.Security));
		}

		[Test]
		public void TestLimitViolationsAreSecurityRelevant ()
		{
			// Note: Content that went unprocessed because of a limit is content that went unscanned.
			var limits = new[] {
				TnefComplianceViolation.NestingTooDeep,
				TnefComplianceViolation.TooManyAttachments,
				TnefComplianceViolation.DataSizeLimitExceeded,
			};

			foreach (var violation in limits) {
				var categories = TnefComplianceIssue.GetCategories (violation);

				Assert.That (categories & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), violation.ToString ());
				Assert.That (categories & MimeComplianceCategories.DataLoss, Is.EqualTo (MimeComplianceCategories.DataLoss), violation.ToString ());
			}
		}

		[Test]
		public void TestCriticalViolationsAreAllSecurityIssues ()
		{
			foreach (var violation in AllViolations) {
				if (TnefComplianceIssue.GetSeverity (violation) != MimeComplianceSeverity.Critical)
					continue;

				Assert.That (TnefComplianceIssue.GetCategories (violation) & MimeComplianceCategories.Security, Is.EqualTo (MimeComplianceCategories.Security), $"{violation} is Critical but is not categorized as a Security issue.");
			}
		}

		[Test]
		public void TestInteroperabilityOnlyViolationsAreNotCritical ()
		{
			foreach (var violation in AllViolations) {
				if (TnefComplianceIssue.GetCategories (violation) != MimeComplianceCategories.Interoperability)
					continue;

				Assert.That (TnefComplianceIssue.GetSeverity (violation), Is.LessThan (MimeComplianceSeverity.Critical), $"{violation} only harms interoperability but is rated Critical.");
			}
		}

		[Test]
		public void TestStaticMethodsThrowOnInvalidViolation ()
		{
			var invalid = (TnefComplianceViolation) 9999;

			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetSeverity (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetCategories (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetDescription (invalid));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetRemarks (invalid));

			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetSeverity (TnefComplianceViolation.None));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetCategories (TnefComplianceViolation.None));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetDescription (TnefComplianceViolation.None));
			Assert.Throws<ArgumentOutOfRangeException> (() => TnefComplianceIssue.GetRemarks (TnefComplianceViolation.None));
		}

		[Test]
		public void TestPropertiesMatchStaticMethods ()
		{
			foreach (var violation in AllViolations) {
				var issue = new TnefComplianceIssue (violation, 0);

				Assert.That (issue.Severity, Is.EqualTo (TnefComplianceIssue.GetSeverity (violation)), $"{violation} Severity");
				Assert.That (issue.Categories, Is.EqualTo (TnefComplianceIssue.GetCategories (violation)), $"{violation} Categories");
				Assert.That (issue.Description, Is.EqualTo (TnefComplianceIssue.GetDescription (violation)), $"{violation} Description");
				Assert.That (issue.Remarks, Is.EqualTo (TnefComplianceIssue.GetRemarks (violation)), $"{violation} Remarks");
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.That (new TnefComplianceIssue (TnefComplianceViolation.InvalidSignature, 0).ToString (), Is.EqualTo ("InvalidSignature at offset 0"));
			Assert.That (new TnefComplianceIssue (TnefComplianceViolation.AttributeChecksumMismatch, 42, 0, TnefAttributeTag.Subject, TnefPropertyTag.Null).ToString (), Is.EqualTo ("AttributeChecksumMismatch at offset 42 in attribute Subject"));
			Assert.That (new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 1234, 2, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW).ToString (), Is.EqualTo ($"InvalidPropertyLength at offset 1234 in attribute MapiProperties in property {TnefPropertyTag.SubjectW} (depth 2)"));
		}
	}
}
