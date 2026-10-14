//
// HtmlCappedComplianceLoggerTests.cs
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
	public class HtmlCappedComplianceLoggerTests
	{
		static HtmlComplianceIssue Issue (HtmlComplianceViolation violation, long offset)
		{
			return new HtmlComplianceIssue (violation, offset, 1, (int) offset + 1);
		}

		[Test]
		public void TestInnerLogger ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 5);

			Assert.That (capped.InnerLogger, Is.SameAs (inner));
		}

		[Test]
		public void TestLimitSuppressesExcessIssues ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 3);

			for (int i = 0; i < 10; i++)
				capped.Log (Issue (HtmlComplianceViolation.DuplicateAttribute, i));

			Assert.That (inner.Issues.Count (x => x.Violation == HtmlComplianceViolation.DuplicateAttribute), Is.EqualTo (3));
			Assert.That (inner.Issues.Count (x => x.Violation == HtmlComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (4));
		}

		[Test]
		public void TestMarkerCarriesTheTriggeringIssuesLocation ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 1);

			capped.Log (new HtmlComplianceIssue (HtmlComplianceViolation.NestedComment, 10, 1, 11));
			capped.Log (new HtmlComplianceIssue (HtmlComplianceViolation.NestedComment, 20, 3, 4));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
			Assert.That (inner.Issues[1], Is.EqualTo (new HtmlComplianceIssue (HtmlComplianceViolation.TooManyComplianceIssues, 20, 3, 4)));
		}

		[Test]
		public void TestMarkerIsEmittedOnlyOnce ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 1);

			for (int i = 0; i < 5; i++) {
				capped.Log (Issue (HtmlComplianceViolation.DuplicateAttribute, i));
				capped.Log (Issue (HtmlComplianceViolation.EofInTag, i));
			}

			Assert.That (inner.Issues.Count (x => x.Violation == HtmlComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (3));
		}

		[Test]
		public void TestFloodDoesNotSuppressOtherViolations ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 2);

			for (int i = 0; i < 1000; i++)
				capped.Log (Issue (HtmlComplianceViolation.DuplicateAttribute, i));

			capped.Log (Issue (HtmlComplianceViolation.UnexpectedNullCharacter, 1000));

			Assert.That (inner.Issues.Count (x => x.Violation == HtmlComplianceViolation.UnexpectedNullCharacter), Is.EqualTo (1));
		}

		[Test]
		public void TestEveryViolationHasItsOwnBudget ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 1);
			var violations = Enum.GetValues<HtmlComplianceViolation> ().Where (v => v != HtmlComplianceViolation.None && v != HtmlComplianceViolation.TooManyComplianceIssues).ToArray ();

			foreach (var violation in violations)
				capped.Log (Issue (violation, 0));

			Assert.That (inner.Issues.Select (x => x.Violation), Is.EqualTo (violations));
		}

		[Test]
		public void TestTooManyComplianceIssuesPassesThrough ()
		{
			var inner = new TestHtmlComplianceLogger ();
			var capped = new HtmlCappedComplianceLogger (inner, 0);

			capped.Log (Issue (HtmlComplianceViolation.TooManyComplianceIssues, 0));
			capped.Log (Issue (HtmlComplianceViolation.TooManyComplianceIssues, 1));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
		}
	}
}