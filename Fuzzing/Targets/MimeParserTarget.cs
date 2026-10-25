//
// MimeParserTarget.cs
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

using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Parses the input with MimeParser and checks that:
	//
	// * the sync and async parsers produce identical messages (compared by their serialized form);
	// * attaching a compliance logger does not change the result;
	// * WriteToAsync produces the same output as WriteTo;
	// * parsing the serialized message and serializing it again is a fixed point.
	sealed class MimeParserTarget : IFuzzTarget
	{
		const int MaxMessages = 10000;

		public string Name => "mime-parser";

		public int MaxLength => 1024 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);
			bool persistent = (selector & 1) != 0;

			CheckEntity (input, persistent);

			if (data.StartsWith ("From "u8))
				CheckMbox (input, persistent);
		}

		static void CheckEntity (byte[] input, bool persistent)
		{
			var sync = ParseEntity (input, persistent, false, null, out var message);
			var async = ParseEntity (input, persistent, true, null, out _);

			Fuzz.Equal (sync, async, "MimeParser Entity sync vs async");

			var checker = new MimeComplianceChecker (MimeComplianceContext.Transport, MimeComplianceOptions.Default.MaxIssuesPerViolation, input.Length);
			var logged = ParseEntity (input, persistent, false, checker, out _);

			Fuzz.Equal (sync, logged, "MimeParser Entity output changed when a compliance logger was attached");

			if (message == null)
				return;

			var written = MimeRoundTrip.Serialize (message);
			var writtenAsync = MimeRoundTrip.SerializeAsync (message);

			Fuzz.Equal (written, writtenAsync, "MimeMessage.WriteTo vs WriteToAsync");

			MimeRoundTrip.Check (message, "MimeParser Entity");
		}

		static string ParseEntity (byte[] input, bool persistent, bool async, MimeComplianceChecker? checker, out MimeMessage? message)
		{
			var parser = new MimeParser (MimeRoundTrip.CreateParserOptions (), new MemoryStream (input, false), MimeFormat.Entity, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory);

			if (checker != null) {
				parser.ComplianceLogger = checker;
				checker.BeginOperation ();
			}

			try {
				message = async ? Fuzz.Wait (parser.ParseMessageAsync ()) : parser.ParseMessage ();
			} catch (FormatException ex) {
				message = null;
				return $"{ex.GetType ().Name}: {ex.Message}";
			}

			return Fuzz.Latin1.GetString (MimeRoundTrip.Serialize (message));
		}

		static void CheckMbox (byte[] input, bool persistent)
		{
			var sync = ParseMbox (input, persistent, false, null);
			var async = ParseMbox (input, persistent, true, null);

			Fuzz.Equal (sync, async, "MimeParser Mbox sync vs async");

			var checker = new MimeComplianceChecker (MimeComplianceContext.Storage, MimeComplianceOptions.Default.MaxIssuesPerViolation, input.Length);
			var logged = ParseMbox (input, persistent, false, checker);

			Fuzz.Equal (sync, logged, "MimeParser Mbox output changed when a compliance logger was attached");
		}

		static string ParseMbox (byte[] input, bool persistent, bool async, MimeComplianceChecker? checker)
		{
			var parser = new MimeParser (MimeRoundTrip.CreateParserOptions (), new MemoryStream (input, false), MimeFormat.Mbox, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory);
			var result = new StringBuilder ();
			long position = parser.Position;
			int count = 0;

			if (checker != null) {
				parser.ComplianceOptions = new MimeComplianceOptions { Context = MimeComplianceContext.Storage };
				parser.ComplianceLogger = checker;
			}

			try {
				while (!parser.IsEndOfStream) {
					Fuzz.Check (++count <= MaxMessages, $"MimeParser parsed more than {MaxMessages} messages from {input.Length} bytes");

					checker?.BeginOperation ();

					var message = async ? Fuzz.Wait (parser.ParseMessageAsync ()) : parser.ParseMessage ();
					long offset = parser.MboxMarkerOffset;

					Fuzz.Check (offset >= 0 && offset <= input.Length, $"MboxMarkerOffset {offset} is outside of the stream [0, {input.Length}]");
					Fuzz.Check (parser.Position > position || parser.IsEndOfStream, $"MimeParser did not advance past offset {position}");
					position = parser.Position;

					result.Append (offset).Append (' ').AppendLine (parser.MboxMarker);
					result.Append (Fuzz.Latin1.GetString (MimeRoundTrip.Serialize (message)));
					result.AppendLine ();
				}
			} catch (FormatException ex) {
				result.Append (ex.GetType ().Name).Append (": ").AppendLine (ex.Message);
			}

			return result.ToString ();
		}
	}
}
