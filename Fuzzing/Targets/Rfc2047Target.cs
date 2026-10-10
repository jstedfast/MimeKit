//
// Rfc2047Target.cs
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
	// Decodes the input as rfc2047 text and phrases under each compliance mode, and checks that
	// EncodeText followed by DecodeText reproduces the original text.
	sealed class Rfc2047Target : IFuzzTarget
	{
		static readonly ParserOptions Strict = new ParserOptions { Rfc2047ComplianceMode = RfcComplianceMode.Strict };
		static readonly ParserOptions Loose = new ParserOptions { Rfc2047ComplianceMode = RfcComplianceMode.Loose };

		public string Name => "rfc2047";

		public int MaxLength => 64 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();

			foreach (var options in new[] { Strict, Loose }) {
				var text = Rfc2047.DecodeText (options, input);
				var phrase = Rfc2047.DecodePhrase (options, input);

				Fuzz.Check (text.Length <= input.Length * 3 + 16, $"DecodeText produced {text.Length} chars from {input.Length} bytes");
				Fuzz.Check (phrase.Length <= input.Length * 3 + 16, $"DecodePhrase produced {phrase.Length} chars from {input.Length} bytes");
			}

			var format = MimeRoundTrip.CreateFormatOptions ();
			var original = Fuzz.Utf8.GetString (input);

			if (IsEncodable (original)) {
				var encoded = Rfc2047.EncodeText (format, Fuzz.Utf8, original);
				var decoded = Rfc2047.DecodeText (Strict, encoded);

				// Note: EncodeText intentionally drops leading and trailing whitespace.
				Fuzz.Equal (original.Trim (' '), decoded, "Rfc2047.DecodeText (Rfc2047.EncodeText (x))");
			}

			GC.KeepAlive (Rfc2047.EncodePhrase (format, Fuzz.Utf8, original));
		}

		// Note: EncodeText folds and unfolds whitespace, so the round trip only holds for text without
		// control characters (including CR and LF).
		static bool IsEncodable (string text)
		{
			foreach (var c in text) {
				if (c < 0x20 || c == 0x7f)
					return false;
			}

			return true;
		}
	}
}
