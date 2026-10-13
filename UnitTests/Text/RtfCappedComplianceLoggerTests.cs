//
// RtfCappedComplianceLoggerTests.cs
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
	public class RtfCappedComplianceLoggerTests
	{
		static RtfComplianceIssue Issue (RtfComplianceViolation violation, long offset, int depth = 0)
		{
			return new RtfComplianceIssue (violation, offset, 1, (int) offset + 1, depth);
		}

		[Test]
		public void TestInnerLogger ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 5);

			Assert.That (capped.InnerLogger, Is.SameAs (inner));
		}

		[Test]
		public void TestLimitSuppressesExcessIssues ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 3);

			for (int i = 0; i < 10; i++)
				capped.Log (Issue (RtfComplianceViolation.InvalidHexEscape, i));

			Assert.That (inner.Issues.Count (x => x.Violation == RtfComplianceViolation.InvalidHexEscape), Is.EqualTo (3));
			Assert.That (inner.Issues.Count (x => x.Violation == RtfComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (4));
		}

		[Test]
		public void TestMarkerCarriesTheTriggeringIssuesLocation ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 1);

			capped.Log (new RtfComplianceIssue (RtfComplianceViolation.UndefinedColor, 10, 1, 11, 1));
			capped.Log (new RtfComplianceIssue (RtfComplianceViolation.UndefinedColor, 20, 3, 4, 2));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
			Assert.That (inner.Issues[1], Is.EqualTo (new RtfComplianceIssue (RtfComplianceViolation.TooManyComplianceIssues, 20, 3, 4, 2)));
		}

		[Test]
		public void TestMarkerIsEmittedOnlyOnce ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 1);

			for (int i = 0; i < 5; i++) {
				capped.Log (Issue (RtfComplianceViolation.InvalidHexEscape, i));
				capped.Log (Issue (RtfComplianceViolation.UndefinedFont, i));
			}

			Assert.That (inner.Issues.Count (x => x.Violation == RtfComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (3));
		}

		[Test]
		public void TestFloodDoesNotSuppressOtherViolations ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 2);

			for (int i = 0; i < 1000; i++)
				capped.Log (Issue (RtfComplianceViolation.InvalidHexEscape, i));

			capped.Log (Issue (RtfComplianceViolation.ContentAfterDocument, 1000));

			Assert.That (inner.Issues.Count (x => x.Violation == RtfComplianceViolation.ContentAfterDocument), Is.EqualTo (1));
		}

		[Test]
		public void TestEveryViolationHasItsOwnBudget ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 1);
			var violations = Enum.GetValues<RtfComplianceViolation> ().Where (v => v != RtfComplianceViolation.None && v != RtfComplianceViolation.TooManyComplianceIssues).ToArray ();

			foreach (var violation in violations)
				capped.Log (Issue (violation, 0));

			Assert.That (inner.Issues.Select (x => x.Violation), Is.EqualTo (violations));
		}

		[Test]
		public void TestTooManyComplianceIssuesPassesThrough ()
		{
			var inner = new TestRtfComplianceLogger ();
			var capped = new RtfCappedComplianceLogger (inner, 0);

			capped.Log (Issue (RtfComplianceViolation.TooManyComplianceIssues, 0));
			capped.Log (Issue (RtfComplianceViolation.TooManyComplianceIssues, 1));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
		}
	}
}