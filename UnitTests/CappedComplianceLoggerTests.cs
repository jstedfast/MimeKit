//
// CappedComplianceLoggerTests.cs
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
	public class CappedComplianceLoggerTests
	{
		[Test]
		public void TestTooManyComplianceIssuesPassesThrough ()
		{
			var inner = new TestMimeComplianceLogger ();
			var capped = new CappedComplianceLogger (inner, 0);

			capped.Log (new MimeComplianceIssue (MimeComplianceContext.Storage, MimeComplianceViolation.TooManyComplianceIssues, 10, 2, 3));
			capped.Log (new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.TooManyComplianceIssues, 20, 4, 5));

			Assert.That (inner.Issues, Has.Count.EqualTo (2));
			Assert.That (inner.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.TooManyComplianceIssues));
			Assert.That (inner.Issues[0].StreamOffset, Is.EqualTo (10));
			Assert.That (inner.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.TooManyComplianceIssues));
			Assert.That (inner.Issues[1].StreamOffset, Is.EqualTo (20));
		}
	}
}
