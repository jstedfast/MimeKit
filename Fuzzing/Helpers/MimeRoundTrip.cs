//
// MimeRoundTrip.cs
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

namespace MimeKit.Fuzzing.Helpers {
	static class MimeRoundTrip
	{
		public static FormatOptions CreateFormatOptions ()
		{
			// Note: FormatOptions.Default uses the platform's newline, which would make results differ between
			// Windows and Linux; the fuzzers always serialize with LF.
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;
			return options;
		}

		public static ParserOptions CreateParserOptions ()
		{
			return new ParserOptions {
				MaxMimeDepth = 16,
				MaxAddressGroupDepth = 4,
			};
		}

		public static byte[] Serialize (MimeMessage message, FormatOptions? options = null)
		{
			using var stream = new MemoryStream ();

			message.WriteTo (options ?? CreateFormatOptions (), stream);

			return stream.ToArray ();
		}

		public static byte[] SerializeAsync (MimeMessage message, FormatOptions? options = null)
		{
			using var stream = new MemoryStream ();

			Fuzz.Wait (message.WriteToAsync (options ?? CreateFormatOptions (), stream));

			return stream.ToArray ();
		}

		public static MimeMessage Parse (byte[] data, bool persistent, ParserOptions? options = null)
		{
			var parser = new MimeParser (options ?? CreateParserOptions (), new MemoryStream (data, false), MimeFormat.Entity, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory);

			return parser.ParseMessage ();
		}

		// Serialize a message, then check that parsing and re-serializing the output reproduces it exactly
		// and that the result is stable across another iteration.
		const int MaxCarriageReturnRounds = 16;

		public static void Check (MimeMessage message, string what)
		{
			var first = Serialize (message);
			var options = CreateParserOptions ();

			// Generated headers can exceed the default input limit. Bound reparsing by the output
			// already in memory without relaxing the limits used to parse the original fuzz input.
			options.MaxHeaderLength = Math.Max (options.MaxHeaderLength, first.Length);

			MimeMessage reparsed;

			try {
				reparsed = Parse (first, false, options);
			} catch (FormatException ex) {
				throw new FuzzInvariantException ($"{what}: MimeKit failed to parse its own output: {ex.Message}");
			}

			var second = Serialize (reparsed);

			// Note: Content (or a header) that contains bare CRs before a CRLF (e.g. "\r\r\r\n") is not legal MIME.
			// Dos2UnixFilter converts "\r\r\r\n" into "\r\r\n", which then becomes "\r\n" and finally "\n" on the following
			// round trips, so the output only converges after a few extra iterations. Require that the only differences
			// along the way are CRs and that it eventually becomes stable.
			for (int round = 0; round < MaxCarriageReturnRounds && !first.AsSpan ().SequenceEqual (second) && EqualIgnoringCarriageReturns (first, second); round++) {
				first = second;

				try {
					reparsed = Parse (first, false, options);
				} catch (FormatException ex) {
					throw new FuzzInvariantException ($"{what}: MimeKit failed to parse its own output: {ex.Message}");
				}

				second = Serialize (reparsed);
			}

			// Note: Each round only removes one CR from each run, so long runs of CRs need more rounds than are worth
			// checking. As long as the output keeps shrinking and the only differences are CRs, it will converge.
			if (second.Length < first.Length && EqualIgnoringCarriageReturns (first, second))
				return;

			Fuzz.Equal (first, second, $"{what}: serialize(parse(serialize(x))) != serialize(x)");
		}

		static bool EqualIgnoringCarriageReturns (byte[] a, byte[] b)
		{
			int i = 0, j = 0;

			while (true) {
				while (i < a.Length && a[i] == (byte) '\r')
					i++;
				while (j < b.Length && b[j] == (byte) '\r')
					j++;

				if (i == a.Length || j == b.Length)
					return i == a.Length && j == b.Length;

				if (a[i++] != b[j++])
					return false;
			}
		}
	}
}
