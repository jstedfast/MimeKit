//
// MimeComplianceChecker.cs
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

namespace MimeKit.Fuzzing.Helpers {
	// An IMimeComplianceLogger that validates every issue it receives and records them into a trace so
	// that the issues reported by different code paths (sync vs async) can be compared.
	sealed class MimeComplianceChecker : IMimeComplianceLogger
	{
		readonly int[] counts = new int[(int) MimeComplianceViolation.IncompleteUUEncodedContent + 1];
		readonly StringBuilder trace = new StringBuilder ();
		readonly MimeComplianceContext context;
		readonly long streamLength;
		readonly int cap;
		bool reachedCap;
		bool truncated;

		public MimeComplianceChecker (MimeComplianceContext context, int maxIssuesPerViolation, long streamLength)
		{
			this.context = context;
			this.cap = maxIssuesPerViolation;
			this.streamLength = streamLength;
		}

		public string Trace => trace.ToString ();

		public int Count { get; private set; }

		// MimeReader and MimeParser reset the per-violation budget at the start of every parse operation.
		public void BeginOperation ()
		{
			Array.Clear (counts);
			reachedCap = false;
			truncated = false;
			trace.AppendLine ("--");
		}

		public void Log (in MimeComplianceIssue issue)
		{
			var violation = issue.Violation;

			Fuzz.Check (Enum.IsDefined (violation), $"Undefined compliance violation: {(int) violation}");
			Fuzz.Check (Enum.IsDefined (issue.PositionKind), $"{violation}: undefined position kind {(int) issue.PositionKind}");
			Fuzz.Check (issue.Context == context, $"{violation}: context {issue.Context} does not match the configured context {context}");
			Fuzz.Check (issue.StreamOffset >= 0 && issue.StreamOffset <= streamLength, $"{violation}: stream offset {issue.StreamOffset} is outside of the stream [0, {streamLength}]");
			Fuzz.Check (issue.LineNumber >= 1, $"{violation}: invalid line number {issue.LineNumber}");
			Fuzz.Check (issue.ColumnNumber >= 0, $"{violation}: invalid column number {issue.ColumnNumber}");
			Fuzz.Check (!string.IsNullOrEmpty (issue.Description), $"{violation}: missing description");
			GC.KeepAlive (issue.Severity);
			GC.KeepAlive (issue.Categories);

			if (violation == MimeComplianceViolation.TooManyComplianceIssues) {
				Fuzz.Check (cap > 0, "TooManyComplianceIssues was reported even though issues are not capped");
				Fuzz.Check (!truncated, "TooManyComplianceIssues was reported more than once in a single operation");
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

			Count++;
			trace.Append (violation).Append (' ').Append (issue.PositionKind).Append (' ').Append (issue.StreamOffset).Append (' ').Append (issue.LineNumber).Append (' ').Append (issue.ColumnNumber).AppendLine ();
		}
	}
}
