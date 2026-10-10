//
// MimeReaderTarget.cs
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

namespace MimeKit.Fuzzing.Targets {
	// Drives MimeReader over the input in both Entity and Mbox formats, comparing the callback traces and
	// compliance issues reported by the sync and async code paths.
	sealed class MimeReaderTarget : IFuzzTarget
	{
		const int MaxMessages = 10000;

		public string Name => "mime-reader";

		public int MaxLength => 1024 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);
			int cap = (selector & 3) switch {
				0 => 1,
				1 => 4,
				2 => 0,
				_ => MimeComplianceOptions.Default.MaxIssuesPerViolation,
			};
			var context = (selector & 4) != 0 ? MimeComplianceContext.Storage : MimeComplianceContext.Transport;

			foreach (var format in new[] { MimeFormat.Entity, MimeFormat.Mbox }) {
				var (syncTrace, syncIssues) = Read (input, format, cap, context, false);
				var (asyncTrace, asyncIssues) = Read (input, format, cap, context, true);

				Fuzz.Equal (syncTrace, asyncTrace, $"MimeReader {format} sync vs async callbacks");
				Fuzz.Equal (syncIssues, asyncIssues, $"MimeReader {format} sync vs async compliance issues");
			}
		}

		static ParserOptions CreateOptions ()
		{
			return new ParserOptions {
				MaxMimeDepth = 16,
				MaxAddressGroupDepth = 4,
			};
		}

		static (string Trace, string Issues) Read (byte[] input, MimeFormat format, int cap, MimeComplianceContext context, bool async)
		{
			var stream = new MemoryStream (input, false);
			var reader = new RecordingMimeReader (CreateOptions (), stream, format);
			var checker = new MimeComplianceChecker (context, cap, input.Length);

			reader.ComplianceOptions = new MimeComplianceOptions {
				EnabledValidators = MimeComplianceValidators.All,
				MaxIssuesPerViolation = cap,
				Context = context,
			};
			reader.ComplianceLogger = checker;

			try {
				if (format == MimeFormat.Entity) {
					checker.BeginOperation ();
					ReadMessage (reader, async);
				} else {
					long position = reader.Position;
					int count = 0;

					while (!reader.IsEndOfStream) {
						Fuzz.Check (++count <= MaxMessages, $"MimeReader read more than {MaxMessages} messages from {input.Length} bytes");

						checker.BeginOperation ();
						ReadMessage (reader, async);

						Fuzz.Check (reader.Position > position || reader.IsEndOfStream, $"MimeReader did not advance past offset {position}");
						position = reader.Position;
					}
				}
			} catch (FormatException ex) {
				reader.Append ($"FormatException: {ex.Message}");
			}

			return (reader.Trace, checker.Trace);
		}

		static void ReadMessage (MimeReader reader, bool async)
		{
			if (async)
				Fuzz.Wait (reader.ReadMessageAsync ());
			else
				reader.ReadMessage ();
		}
	}
}
