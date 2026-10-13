//
// TestRtfComplianceLogger.cs
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
	public class RtfConverterComplianceTests
	{
		const string Document = "{\\rtf1{\\fonttbl{\\f0 A;}}\\f9 x\\'zz y\\'zz}junk";

		static readonly string[] DocumentIssues = {
			"UndefinedFont at line 1, position 25 (offset 24) (depth 1)",
			"InvalidHexEscape at line 1, position 30 (offset 29) (depth 1)",
			"InvalidHexEscape at line 1, position 36 (offset 35) (depth 1)",
			"ContentAfterDocument at line 1, position 41 (offset 40)"
		};

		static string[] ToStrings (TestRtfComplianceLogger logger)
		{
			return logger.Issues.Select (issue => issue.ToString ()).ToArray ();
		}

		[Test]
		public void TestRtfToTextReportsEachIssueOnce ()
		{
			var logger = new TestRtfComplianceLogger ();
			var converter = new RtfToText { ComplianceLogger = logger };

			converter.Convert (Document);

			Assert.That (ToStrings (logger), Is.EqualTo (DocumentIssues));
		}

		[Test]
		public void TestRtfToHtmlReportsEachIssueOnce ()
		{
			var logger = new TestRtfComplianceLogger ();
			var converter = new RtfToHtml { ComplianceLogger = logger };

			converter.Convert (Document);

			Assert.That (ToStrings (logger), Is.EqualTo (DocumentIssues));
		}

		[Test]
		public void TestRtfToHtmlReportsIssuesWhenExtractingEncapsulatedHtml ()
		{
			const string rtf = "{\\rtf1\\ansi\\fromhtml1{\\*\\htmltag64 <p>}\\htmlrtf {\\'zz}\\htmlrtf0 x{\\*\\htmltag72 </p>}}}";
			var logger = new TestRtfComplianceLogger ();
			var converter = new RtfToHtml { ComplianceLogger = logger, OutputHtmlFragment = true };

			var html = converter.Convert (rtf);

			Assert.That (html, Does.Contain ("<p>x</p>"));
			Assert.That (ToStrings (logger), Is.EqualTo (new[] {
				"InvalidHexEscape at line 1, position 50 (offset 49) (depth 2)",
				"UnbalancedGroupEnd at line 1, position 86 (offset 85)"
			}));
		}

		static RtfComplianceViolation[] Violations (TestRtfComplianceLogger logger)
		{
			return logger.Issues.Select (issue => issue.Violation).ToArray ();
		}

		static readonly string Flood = "{\\rtf1 " + string.Concat (Enumerable.Repeat ("\\'zz", 5)) + "\\f9 x}";

		static readonly RtfComplianceViolation[] CappedFlood = {
			RtfComplianceViolation.InvalidHexEscape,
			RtfComplianceViolation.InvalidHexEscape,
			RtfComplianceViolation.TooManyComplianceIssues,
			RtfComplianceViolation.UndefinedFont
		};

		[Test]
		public void TestRtfToTextMaxComplianceIssuesPerViolation ()
		{
			var logger = new TestRtfComplianceLogger ();
			var converter = new RtfToText { ComplianceLogger = logger, MaxComplianceIssuesPerViolation = 2 };

			converter.Convert (Flood);
			Assert.That (Violations (logger), Is.EqualTo (CappedFlood), "first document");

			// The budget applies per document.
			logger.Issues.Clear ();
			converter.Convert (Flood);
			Assert.That (Violations (logger), Is.EqualTo (CappedFlood), "second document");

			// 0 means that no limit is applied.
			logger.Issues.Clear ();
			converter.MaxComplianceIssuesPerViolation = 0;
			converter.Convert (Flood);
			Assert.That (logger.Issues.Count (issue => issue.Violation == RtfComplianceViolation.InvalidHexEscape), Is.EqualTo (5), "unlimited");
			Assert.That (logger.Issues.Any (issue => issue.Violation == RtfComplianceViolation.TooManyComplianceIssues), Is.False, "unlimited");
		}

		[Test]
		public void TestRtfToHtmlMaxComplianceIssuesPerViolation ()
		{
			var logger = new TestRtfComplianceLogger ();
			var converter = new RtfToHtml { ComplianceLogger = logger, MaxComplianceIssuesPerViolation = 2 };

			converter.Convert (Flood);
			Assert.That (Violations (logger), Is.EqualTo (CappedFlood), "first document");

			logger.Issues.Clear ();
			converter.Convert (Flood);
			Assert.That (Violations (logger), Is.EqualTo (CappedFlood), "second document");

			logger.Issues.Clear ();
			converter.MaxComplianceIssuesPerViolation = 0;
			converter.Convert (Flood);
			Assert.That (logger.Issues.Count (issue => issue.Violation == RtfComplianceViolation.InvalidHexEscape), Is.EqualTo (5), "unlimited");
			Assert.That (logger.Issues.Any (issue => issue.Violation == RtfComplianceViolation.TooManyComplianceIssues), Is.False, "unlimited");
		}
	}
}