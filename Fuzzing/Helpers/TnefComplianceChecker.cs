//
// TnefComplianceChecker.cs
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

using System.Text;

using MimeKit.Tnef;

namespace MimeKit.Fuzzing.Helpers {
	// An ITnefComplianceLogger that validates every issue it receives and records them into a trace.
	sealed class TnefComplianceChecker : ITnefComplianceLogger
	{
		readonly int[] counts = new int[(int) TnefComplianceViolation.DataSizeLimitExceeded + 1];
		readonly StringBuilder trace = new StringBuilder ();
		readonly long streamLength;
		readonly int maxDepth;
		readonly int cap;
		bool reachedCap;
		bool truncated;

		// cap: the TnefComplianceOptions.MaxIssuesPerViolation in effect for a single budget, or -1 when the
		// issues may come from more than one budget (e.g. TnefMessage loads embedded messages with detached
		// readers that each have their own budget), in which case the cap cannot be verified.
		public TnefComplianceChecker (int cap, int maxDepth, long streamLength)
		{
			this.cap = cap;
			this.maxDepth = maxDepth;
			this.streamLength = streamLength;
		}

		public string Trace => trace.ToString ();

		public void Log (in TnefComplianceIssue issue)
		{
			var violation = issue.Violation;

			Fuzz.Check (Enum.IsDefined (violation), $"Undefined TNEF compliance violation: {(int) violation}");
			Fuzz.Check (issue.StreamOffset >= 0 && issue.StreamOffset <= streamLength, $"{violation}: stream offset {issue.StreamOffset} is outside of the stream [0, {streamLength}]");
			Fuzz.Check (issue.Depth >= 0 && issue.Depth <= maxDepth + 1, $"{violation}: invalid depth {issue.Depth}");
			Fuzz.Check (!string.IsNullOrEmpty (issue.Description), $"{violation}: missing description");
			GC.KeepAlive (issue.Severity);
			GC.KeepAlive (issue.Categories);

			if (cap >= 0) {
				if (violation == TnefComplianceViolation.TooManyComplianceIssues) {
					Fuzz.Check (cap > 0, "TooManyComplianceIssues was reported even though issues are not capped");
					Fuzz.Check (!truncated, "TooManyComplianceIssues was reported more than once");
					Fuzz.Check (reachedCap, "TooManyComplianceIssues was reported before any violation reached the cap");
					truncated = true;
				} else if ((int) violation < counts.Length) {
					int count = ++counts[(int) violation];

					if (cap > 0) {
						Fuzz.Check (count <= cap, $"{violation} was reported {count} times, exceeding the cap of {cap}");

						if (count == cap)
							reachedCap = true;
					}
				}
			}

			trace.Append (violation).Append (' ').Append (issue.StreamOffset).Append (' ').Append (issue.Depth).Append (' ').Append (issue.AttributeTag).Append (' ').Append (issue.PropertyTag).AppendLine ();
		}
	}
}
