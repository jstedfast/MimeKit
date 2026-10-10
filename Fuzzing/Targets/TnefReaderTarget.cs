//
// TnefReaderTarget.cs
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

using MimeKit.Fuzzing.Helpers;
using MimeKit.Tnef;

namespace MimeKit.Fuzzing.Targets {
	// Walks a TNEF stream with TnefReader/TnefPropertyReader (sync and async), checking that both produce
	// the same trace and compliance issues, that compliance issues are sane and capped, and that
	// re-emitting the stream through TnefWriter is idempotent.
	sealed class TnefReaderTarget : IFuzzTarget
	{
		const int MaxNestingDepth = 4;

		public string Name => "tnef-reader";

		public int MaxLength => 1024 * 1024;

		static TnefOptions CreateOptions ()
		{
			return new TnefOptions {
				MaxNestingDepth = MaxNestingDepth,
				MaxPropertyValueLength = 1024 * 1024,
				MaxTotalDataBytes = 16 * 1024 * 1024,
			};
		}

		static readonly int[] Caps = { 0, 1, 4 };

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			int cap = Caps[Fuzz.Selector (data) % Caps.Length];

			var (syncTrace, syncIssues) = Walk (input, cap, false);
			var (asyncTrace, asyncIssues) = Walk (input, cap, true);

			Fuzz.Equal (syncTrace, asyncTrace, "TnefReader sync vs async trace");
			Fuzz.Equal (syncIssues, asyncIssues, "TnefReader sync vs async compliance issues");

			ReEmitRoundTrip (input);
		}

		static (string Trace, string Issues) Walk (byte[] input, int cap, bool useAsync)
		{
			var checker = new TnefComplianceChecker (cap, MaxNestingDepth, input.Length);
			var walker = new TnefWalker ();

			using (var reader = new TnefReader (new MemoryStream (input, false), CreateOptions (), false)) {
				reader.ComplianceLogger = checker;
				reader.MaxComplianceIssuesPerViolation = cap;

				try {
					if (useAsync)
						Fuzz.Wait (walker.WalkAsync (reader));
					else
						walker.Walk (reader);
				} catch (TnefException ex) when (ex.Violation == TnefComplianceViolation.InvalidSignature) {
					// Documented: the stream does not begin with the TNEF signature.
					return ("invalid signature", checker.Trace);
				}
			}

			return (walker.Trace, checker.Trace);
		}

		static void ReEmitRoundTrip (byte[] input)
		{
			byte[]? first;

			try {
				first = TnefReEmitter.ReEmit (input, CreateOptions ());
			} catch (TnefException ex) when (ex.Violation == TnefComplianceViolation.InvalidSignature) {
				return;
			}

			// The writer rejected something that the reader produced from the fuzzed input; there is nothing to round-trip.
			if (first == null)
				return;

			var second = TnefReEmitter.ReEmit (first, CreateOptions ());

			Fuzz.Check (second != null, "TnefWriter rejected a stream that TnefWriter itself produced");
			Fuzz.Equal (first, second, "TNEF re-emit is not idempotent");
		}
	}
}
