//
// TnefMessageTarget.cs
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
	// Loads a TnefMessage (sync and async), compares canonical dumps of the two, converts the message to
	// MIME and checks that the resulting MimeMessage survives a serialize/parse/serialize round trip.
	sealed class TnefMessageTarget : IFuzzTarget
	{
		const int MaxNestingDepth = 4;

		public string Name => "tnef-message";

		public int MaxLength => 4 * 1024 * 1024;

		static TnefOptions CreateOptions ()
		{
			return new TnefOptions {
				MaxNestingDepth = MaxNestingDepth,
				MaxPropertyValueLength = 1024 * 1024,
				MaxTotalDataBytes = 16 * 1024 * 1024,
			};
		}

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);

			// Note: TnefMessage loads embedded messages with detached readers that each have their own budget,
			// so the per-violation cap cannot be verified here (it is verified by the tnef-reader target).
			var syncChecker = new TnefComplianceChecker (-1, MaxNestingDepth, input.Length);
			var asyncChecker = new TnefComplianceChecker (-1, MaxNestingDepth, input.Length);
			TnefMessage syncMessage, asyncMessage;

			try {
				syncMessage = Load (input, syncChecker, false);
			} catch (TnefException ex) when (ex.Violation == TnefComplianceViolation.InvalidSignature) {
				// Documented: the stream does not begin with the TNEF signature.
				try {
					Load (input, asyncChecker, true).Dispose ();
				} catch (TnefException ex2) when (ex2.Violation == TnefComplianceViolation.InvalidSignature) {
					return;
				}

				throw new FuzzInvariantException ("TnefMessage.Load failed with an invalid signature but LoadAsync did not");
			}

			using (syncMessage) {
				asyncMessage = Load (input, asyncChecker, true);

				using (asyncMessage) {
					var syncDump = TnefMessageDumper.Dump (syncMessage);
					var asyncDump = Fuzz.Wait (TnefMessageDumper.DumpAsync (asyncMessage));

					Fuzz.Equal (syncDump, asyncDump, "TnefMessage Load vs LoadAsync");
					Fuzz.Equal (syncChecker.Trace, asyncChecker.Trace, "TnefMessage Load vs LoadAsync compliance issues");
				}

				ConvertToMime (syncMessage, selector);
			}
		}

		static TnefMessage Load (byte[] input, TnefComplianceChecker checker, bool useAsync)
		{
			// Note: The reader is not disposed because the message may continue to read values from it lazily.
			var reader = new TnefReader (new MemoryStream (input, false), CreateOptions (), false);

			reader.ComplianceLogger = checker;

			return useAsync ? Fuzz.Wait (TnefMessage.LoadAsync (reader)) : TnefMessage.Load (reader);
		}

		static void ConvertToMime (TnefMessage message, uint selector)
		{
			var options = new TnefConversionOptions {
				ConvertEmbeddedMessages = (selector & 1) != 0,
				GenerateCalendar = (selector & 2) != 0,
				MaxCalendarExceptions = 64,
				MaxCalendarExceptionsSize = 1024 * 1024,
			};

			if ((selector & 4) != 0)
				options.AttachmentPlaceholderCallback = (attachment, entity) => "[attachment]";

			using var result = message.ConvertToMime (options);

			foreach (var loss in result.Losses)
				GC.KeepAlive (loss.ToString ());

			MimeRoundTrip.Check (result.Message, "TNEF conversion result");
		}
	}
}
