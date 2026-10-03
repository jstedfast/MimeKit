//
// TnefCappedComplianceLoggerTests.cs
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
	public class TnefCappedComplianceLoggerTests
	{
		[Test]
		public void TestInnerLogger ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 5);

			Assert.That (capped.InnerLogger, Is.SameAs (inner));
		}

		[Test]
		public void TestLimitSuppressesExcessIssues ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 3);

			for (int i = 0; i < 10; i++)
				capped.Log (new TnefComplianceIssue (TnefComplianceViolation.AttributeChecksumMismatch, i));

			Assert.That (inner.Issues.Count (x => x.Violation == TnefComplianceViolation.AttributeChecksumMismatch), Is.EqualTo (3));
			Assert.That (inner.Issues.Count (x => x.Violation == TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (4));
		}

		[Test]
		public void TestMarkerCarriesTheTriggeringIssuesLocation ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 1);

			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 10, 1, TnefAttributeTag.MapiProperties, TnefPropertyTag.SubjectW));
			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.InvalidPropertyLength, 20, 2, TnefAttributeTag.Attachment, TnefPropertyTag.BodyW));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));

			var marker = inner.Issues[1];

			Assert.That (marker.Violation, Is.EqualTo (TnefComplianceViolation.TooManyComplianceIssues));
			Assert.That (marker.StreamOffset, Is.EqualTo (20));
			Assert.That (marker.Depth, Is.EqualTo (2));
			Assert.That (marker.AttributeTag, Is.EqualTo (TnefAttributeTag.Attachment));
			Assert.That (marker.PropertyTag, Is.EqualTo (TnefPropertyTag.BodyW));
		}

		[Test]
		public void TestMarkerIsEmittedOnlyOnce ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 1);

			for (int i = 0; i < 5; i++) {
				capped.Log (new TnefComplianceIssue (TnefComplianceViolation.AttributeChecksumMismatch, i));
				capped.Log (new TnefComplianceIssue (TnefComplianceViolation.UnknownAttribute, i));
			}

			Assert.That (inner.Issues.Count (x => x.Violation == TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
			Assert.That (inner.Issues, Has.Count.EqualTo (3));
		}

		[Test]
		public void TestFloodDoesNotSuppressOtherViolations ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 2);

			for (int i = 0; i < 1000; i++)
				capped.Log (new TnefComplianceIssue (TnefComplianceViolation.AttributeChecksumMismatch, i));

			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.MessageAttributeAfterAttachment, 1000));

			Assert.That (inner.Issues.Count (x => x.Violation == TnefComplianceViolation.MessageAttributeAfterAttachment), Is.EqualTo (1));
		}

		[Test]
		public void TestEveryViolationHasItsOwnBudget ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 1);
			int count = 0;

			foreach (var violation in Enum.GetValues<TnefComplianceViolation> ()) {
				if (violation == TnefComplianceViolation.None || violation == TnefComplianceViolation.TooManyComplianceIssues)
					continue;

				capped.Log (new TnefComplianceIssue (violation, 0));
				count++;
			}

			Assert.That (inner.Issues, Has.Count.EqualTo (count));
			Assert.That (inner.Issues.Any (x => x.Violation == TnefComplianceViolation.TooManyComplianceIssues), Is.False);
		}

		[Test]
		public void TestResetRestoresTheBudget ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 1);

			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.TruncatedStream, 0));
			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.TruncatedStream, 1));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));

			capped.Reset ();
			inner.Issues.Clear ();

			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.TruncatedStream, 2));
			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.TruncatedStream, 3));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
			Assert.That (inner.Issues[0].Violation, Is.EqualTo (TnefComplianceViolation.TruncatedStream));
			Assert.That (inner.Issues[0].StreamOffset, Is.EqualTo (2));
			Assert.That (inner.Issues[1].Violation, Is.EqualTo (TnefComplianceViolation.TooManyComplianceIssues), "The marker should be emitted again after a reset.");
		}

		[Test]
		public void TestTooManyComplianceIssuesPassesThrough ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 0);

			for (int i = 0; i < 3; i++)
				capped.Log (new TnefComplianceIssue (TnefComplianceViolation.TooManyComplianceIssues, i));

			Assert.That (inner.Issues, Has.Count.EqualTo (3));
		}

		[Test]
		public void TestZeroLimitReportsOnlyTheMarker ()
		{
			var inner = new TestTnefComplianceLogger ();
			var capped = new TnefCappedComplianceLogger (inner, 0);

			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.InvalidSignature, 0));
			capped.Log (new TnefComplianceIssue (TnefComplianceViolation.UnknownAttribute, 5));

			Assert.That (inner.Issues, Has.Count.EqualTo (1));
			Assert.That (inner.Issues[0].Violation, Is.EqualTo (TnefComplianceViolation.TooManyComplianceIssues));
			Assert.That (inner.Issues[0].StreamOffset, Is.EqualTo (0));
		}
	}
}
