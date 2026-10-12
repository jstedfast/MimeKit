//
// AuthenticationResultsTarget.cs
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

using MimeKit.Cryptography;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Parses the input as an Authentication-Results (or ARC-Authentication-Results) header value, then checks
	// that Parse and TryParse agree and that both ToString and the folded header encoding re-parse to the
	// same value.
	sealed class AuthenticationResultsTarget : IFuzzTarget
	{
		const int Padding = 7;

		public string Name => "authentication-results";

		public int MaxLength => 16 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();

			bool parsed = AuthenticationResults.TryParse (input, out var authres);

			CheckParseAgrees (input, parsed, authres);
			CheckSubrange (data, parsed, authres);

			if (!parsed)
				return;

			var text = authres!.ToString ();

			Fuzz.Check (AuthenticationResults.TryParse (Fuzz.Utf8.GetBytes (text), out var reparsed), $"AuthenticationResults failed to parse its own output: \"{Fuzz.Escape (text)}\"");
			Fuzz.Equal (text, reparsed!.ToString (), "AuthenticationResults.ToString round-trip");

			CheckEncode (authres, text, (Fuzz.Selector (data) & 1) != 0 ? 60 : 78);
		}

		static void CheckParseAgrees (byte[] input, bool parsed, AuthenticationResults? authres)
		{
			AuthenticationResults? result = null;

			try {
				result = AuthenticationResults.Parse (input);
			} catch (ParseException) {
				if (parsed)
					Fuzz.Check (false, $"Parse threw a ParseException but TryParse succeeded: \"{Fuzz.Escape (authres!.ToString ())}\"");
				return;
			}

			Fuzz.Check (result != null, "Parse returned null instead of throwing a ParseException");

			if (!parsed)
				Fuzz.Check (false, $"Parse succeeded but TryParse failed: \"{Fuzz.Escape (result!.ToString ())}\"");

			Fuzz.Equal (authres!.ToString (), result!.ToString (), "AuthenticationResults Parse vs TryParse");
		}

		// Parsing a subrange of a larger buffer must give exactly the same result as parsing a copy of it.
		static void CheckSubrange (ReadOnlySpan<byte> data, bool parsed, AuthenticationResults? authres)
		{
			var buffer = new byte[data.Length + Padding * 2];

			buffer.AsSpan (0, Padding).Fill ((byte) ';');
			buffer.AsSpan (Padding + data.Length).Fill ((byte) ';');
			data.CopyTo (buffer.AsSpan (Padding));

			bool parsedRange = AuthenticationResults.TryParse (buffer, Padding, data.Length, out var range);

			Fuzz.Check (parsedRange == parsed, $"TryParse returned {parsed} for the buffer but {parsedRange} for the same bytes in a subrange");

			if (parsed)
				Fuzz.Equal (authres!.ToString (), range!.ToString (), "AuthenticationResults subrange");
		}

		// The folded header value that MimeKit writes when signing (e.g. ARC-Authentication-Results) must parse
		// back to the same value.
		static void CheckEncode (AuthenticationResults authres, string text, int maxLineLength)
		{
			var format = MimeRoundTrip.CreateFormatOptions ();
			var builder = new StringBuilder ();

			format.MaxLineLength = maxLineLength;

			authres.Encode (format, builder, "ARC-Authentication-Results:".Length);

			var encoded = builder.ToString ();

			Fuzz.Check (AuthenticationResults.TryParse (Fuzz.Utf8.GetBytes (encoded), out var reparsed), $"AuthenticationResults failed to parse its own encoded output: \"{Fuzz.Escape (encoded)}\"");
			Fuzz.Equal (text, reparsed!.ToString (), "AuthenticationResults.Encode round-trip");
		}
	}
}
