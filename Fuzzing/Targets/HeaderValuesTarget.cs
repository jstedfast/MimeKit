//
// HeaderValuesTarget.cs
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
using MimeKit.Utils;

namespace MimeKit.Fuzzing.Targets {
	// Parses the input as each kind of structured header value and checks that formatting and re-parsing
	// the result is a fixed point.
	sealed class HeaderValuesTarget : IFuzzTarget
	{
		const int MaxReferences = 100000;

		public string Name => "header-values";

		public int MaxLength => 64 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);
			var options = new ParserOptions {
				ParameterComplianceMode = (selector & 1) != 0 ? RfcComplianceMode.Strict : RfcComplianceMode.Loose,
				Rfc2047ComplianceMode = (selector & 2) != 0 ? RfcComplianceMode.Strict : RfcComplianceMode.Loose,
			};
			var format = MimeRoundTrip.CreateFormatOptions ();

			CheckContentType (options, format, input);
			CheckContentDisposition (options, format, input);
			CheckDate (input);
			CheckMessageIds (input);
			CheckHeader (options, input);
		}

		static string StripPrefix (string value, string prefix)
		{
			Fuzz.Check (value.StartsWith (prefix, StringComparison.Ordinal), $"Expected the formatted value to start with \"{prefix}\": \"{Fuzz.Escape (value)}\"");

			return value.Substring (prefix.Length);
		}

		static void CheckContentType (ParserOptions options, FormatOptions format, byte[] input)
		{
			const string prefix = "Content-Type: ";

			if (!ContentType.TryParse (options, input, out var type))
				return;

			var first = type.ToString (format, Fuzz.Utf8, true);
			GC.KeepAlive (type.ToString (format, Fuzz.Utf8, false));
			GC.KeepAlive (type.MimeType);

			var value = Fuzz.Utf8.GetBytes (StripPrefix (first, prefix));

			Fuzz.Check (ContentType.TryParse (options, value, out var reparsed), $"ContentType failed to parse its own output: \"{Fuzz.Escape (first)}\"");

			CheckConverges (first, reparsed!.ToString (format, Fuzz.Utf8, true), type.Parameters, reparsed.Parameters, "ContentType", text => {
				Fuzz.Check (ContentType.TryParse (options, Fuzz.Utf8.GetBytes (StripPrefix (text, prefix)), out var again), $"ContentType failed to parse its own output: \"{Fuzz.Escape (text)}\"");
				return again!.ToString (format, Fuzz.Utf8, true);
			});
		}

		static void CheckContentDisposition (ParserOptions options, FormatOptions format, byte[] input)
		{
			const string prefix = "Content-Disposition: ";

			if (!ContentDisposition.TryParse (options, input, out var disposition))
				return;

			var first = disposition.ToString (format, Fuzz.Utf8, true);
			GC.KeepAlive (disposition.ToString (format, Fuzz.Utf8, false));
			GC.KeepAlive (disposition.FileName);

			var value = Fuzz.Utf8.GetBytes (StripPrefix (first, prefix));

			Fuzz.Check (ContentDisposition.TryParse (options, value, out var reparsed), $"ContentDisposition failed to parse its own output: \"{Fuzz.Escape (first)}\"");

			CheckConverges (first, reparsed!.ToString (format, Fuzz.Utf8, true), disposition.Parameters, reparsed.Parameters, "ContentDisposition", text => {
				Fuzz.Check (ContentDisposition.TryParse (options, Fuzz.Utf8.GetBytes (StripPrefix (text, prefix)), out var again), $"ContentDisposition failed to parse its own output: \"{Fuzz.Escape (text)}\"");
				return again!.ToString (format, Fuzz.Utf8, true);
			});
		}

		// Note: A parameter that was parsed from rfc2047 encoded-words remembers the charset of the *first* encoded-word
		// so that it can be re-encoded the same way. When that charset cannot represent the whole value, the encoder
		// uses a different charset for some of the words (e.g. "=?utf-8?q?...?= =?iso-8859-1?q?...?="), so the re-parsed
		// parameter remembers a different encoding than the original one did. The parameter values must still be
		// identical, and the output must be stable after that.
		static void CheckConverges (string first, string second, ParameterList original, ParameterList reparsed, string what, Func<string, string> roundTrip)
		{
			if (first != second) {
				Fuzz.Check (original.Count == reparsed.Count, $"{what} parameter count changed from {original.Count} to {reparsed.Count}: \"{Fuzz.Escape (first)}\"");

				for (int i = 0; i < original.Count; i++) {
					Fuzz.Equal (original[i].Name, reparsed[i].Name, $"{what} parameter name");
					Fuzz.Equal (original[i].Value, reparsed[i].Value, $"{what} parameter value");
				}

				first = second;
				second = roundTrip (first);
			}

			Fuzz.Equal (first, second, $"{what} format(parse(format(x)))");
		}

		static void CheckDate (byte[] input)
		{
			if (!DateUtils.TryParse (input, out var date))
				return;

			var formatted = DateUtils.FormatDate (date);

			Fuzz.Check (DateUtils.TryParse (formatted, out var reparsed), $"DateUtils failed to parse its own output: \"{formatted}\"");
			Fuzz.Check (reparsed == date && reparsed.Offset == date.Offset, $"DateUtils round trip changed {date:O} into {reparsed:O} (\"{formatted}\")");
		}

		static bool IsPlausibleMessageId (string id)
		{
			int at = id.IndexOf ('@');

			// Note: Require a single '@' separating a non-empty local-part from a non-empty domain.
			if (at <= 0 || at == id.Length - 1 || id.IndexOf ('@', at + 1) != -1)
				return false;

			foreach (var c in id) {
				if (c <= ' ' || c == '"' || c == '<' || c == '>' || c == '(' || c == ')' || c == '\\' || c == ':' || c == 0x7f)
					return false;
			}

			return true;
		}

		static void CheckMessageIds (byte[] input)
		{
			var id = MimeUtils.ParseMessageId (input, 0, input.Length);

			// Note: The msg-id parser is intentionally lax (see https://github.com/jstedfast/MimeKit/issues/472) and can
			// return garbage for garbage input (e.g. a phrase followed by a msg-id), so only check the round trip for
			// results that could plausibly be a msg-id.
			if (id != null && IsPlausibleMessageId (id)) {
				var reparsed = MimeUtils.ParseMessageId ("<" + id + ">");

				Fuzz.Equal (id, reparsed ?? "(null)", "MimeUtils.ParseMessageId (\"<\" + ParseMessageId (x) + \">\")");
			}

			int count = 0;

			foreach (var reference in MimeUtils.EnumerateReferences (input, 0, input.Length)) {
				Fuzz.Check (reference != null, "EnumerateReferences returned null");
				Fuzz.Check (++count <= MaxReferences, $"EnumerateReferences returned more than {MaxReferences} references");
			}
		}

		static void CheckHeader (ParserOptions options, byte[] input)
		{
			var text = Fuzz.Latin1.GetString (input);
			var unfolded = Header.Unfold (text);

			Fuzz.Equal (unfolded, Header.Unfold (unfolded), "Header.Unfold (Header.Unfold (x))");

			if (Header.TryParse (options, input, out var header)) {
				GC.KeepAlive (header.Value);
				GC.KeepAlive (header.ToString ());
				GC.KeepAlive (header.GetValue (Fuzz.Latin1));
			}
		}
	}
}
