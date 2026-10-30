//
// EncodersTarget.cs
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

using System.Runtime.Intrinsics.X86;

using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Encodes the input with the encoder chosen by the first byte (the second byte selects the line length/newline format
	// and seeds the chunking pattern), checking that:
	//
	// * encoding the input in many (possibly empty) chunks produces the same output as encoding it all at once;
	// * an encoder cloned mid-stream behaves identically to the original;
	// * encoding again after Reset (or after Flush, which resets the encoder) reproduces the output;
	// * an encoder never writes more than EstimateOutputLength bytes;
	// * the scalar and every supported hardware-accelerated code path produce identical output;
	// * the output only uses the encoding's alphabet and respects its line-length limit;
	// * base64 output matches Convert.ToBase64String and the yEnc checksum matches a reference CRC-32;
	// * encoding through an EncoderFilter (with arbitrary write sizes) matches the encoder itself;
	// * decoding the output (directly and through a DecoderFilter with arbitrary read sizes) reproduces the input.
	//
	// The one-shot rfc2047 'B' and 'Q' encoders and the rfc2231 hex encoder are checked for their estimates, alphabets
	// and round-trips as well.
	sealed class EncodersTarget : IFuzzTarget
	{
		const int GuardLength = 64;
		const byte Guard = 0xCC;

		public string Name => "encoders";

		public int MaxLength => 256 * 1024;

		delegate int EncodeFunc (byte[] input, int startIndex, int length, byte[] output, bool flush);

		public void Run (ReadOnlySpan<byte> data)
		{
			if (data.Length < 2)
				return;

			int kind = data[0] % 9;
			int option = data[1];
			var newLineFormat = (option & 0x80) != 0 ? NewLineFormat.Unix : NewLineFormat.Dos;
			var input = data.Slice (2).ToArray ();
			var random = new Random ((data[0] << 8) | data[1]);

			switch (kind) {
			case 0: TestBase64 (input, newLineFormat, 60 + (option % 17), random); break;
			case 1: TestQuotedPrintable (input, newLineFormat, 60 + (option * 4 % 939), random); break;
			case 2: TestUU (input, newLineFormat, random); break;
			case 3: TestYEnc (input, newLineFormat, 60 + (option * 4 % 939), random); break;
			case 4: TestPassThrough (input, (option & 1) != 0 ? ContentEncoding.Binary : ContentEncoding.EightBit, random); break;
			case 5: TestRfc2047Base64 (input); break;
			case 6: TestRfc2047QuotedPrintable (input, QEncodeMode.Phrase); break;
			case 7: TestRfc2047QuotedPrintable (input, QEncodeMode.Text); break;
			default: TestRfc2231 (input); break;
			}
		}

		static void TestBase64 (byte[] input, NewLineFormat newLineFormat, int maxLineLength, Random random)
		{
			var encoder = new Base64Encoder (newLineFormat, maxLineLength);
			var name = $"Base64Encoder ({newLineFormat}, {maxLineLength})";
			var output = TestStreaming (encoder, name, input, random, true);
			var normalized = NormalizeNewLines (output, newLineFormat, name);

			CheckBase64Lines (normalized, Math.Min (maxLineLength, 76) / 4 * 4, name);

			var expected = Convert.ToBase64String (input);
			var actual = Fuzz.Latin1.GetString (normalized).Replace ("\n", string.Empty);

			Fuzz.Equal (expected, actual, $"{name} vs Convert.ToBase64String");

			var paths = new List<(string, EncodeFunc)> {
				("scalar", ((Base64Encoder) encoder.Clone ()).ScalarEncode)
			};

			if (Ssse3.IsSupported)
				paths.Add (("SSSE3", ((Base64Encoder) encoder.Clone ()).Ssse3Encode));

			if (Avx2.IsSupported)
				paths.Add (("AVX2", ((Base64Encoder) encoder.Clone ()).Avx2Encode));

			if (System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported && Avx2.IsSupported)
				paths.Add (("AVX512", ((Base64Encoder) encoder.Clone ()).Avx512Encode));

			foreach (var (path, encode) in paths)
				Fuzz.Equal (output, EncodeChunked (encoder, encode, input, random, $"{name} {path}"), $"{name} {path} vs default code path");

			RoundTrip (output, new Base64Decoder (), input, name);
			FilterRoundTrip (encoder, input, output, DecoderFilter.Create (ContentEncoding.Base64), input, random, name);
		}

		static void TestQuotedPrintable (byte[] input, NewLineFormat newLineFormat, int maxLineLength, Random random)
		{
			var encoder = new QuotedPrintableEncoder (newLineFormat, maxLineLength);
			var name = $"QuotedPrintableEncoder ({newLineFormat}, {maxLineLength})";
			var output = TestStreaming (encoder, name, input, random, true);
			var expected = Fuzz.CanonicalizeLineBreaks (input, newLineFormat);

			CheckQuotedPrintableLines (NormalizeNewLines (output, newLineFormat, name), Math.Min (maxLineLength, 76), name);
			RoundTrip (output, new QuotedPrintableDecoder (false), expected, name);
			FilterRoundTrip (encoder, input, output, DecoderFilter.Create (ContentEncoding.QuotedPrintable), expected, random, name);
		}

		static void TestUU (byte[] input, NewLineFormat newLineFormat, Random random)
		{
			var encoder = new UUEncoder (newLineFormat);
			var name = $"UUEncoder ({newLineFormat})";
			var output = TestStreaming (encoder, name, input, random, true);

			CheckUULines (NormalizeNewLines (output, newLineFormat, name), input.Length, name);

			var paths = new List<(string, EncodeFunc)> {
				("scalar", ((UUEncoder) encoder.Clone ()).ScalarEncode)
			};

			if (Ssse3.IsSupported)
				paths.Add (("SSSE3", ((UUEncoder) encoder.Clone ()).Ssse3Encode));

			if (Avx2.IsSupported)
				paths.Add (("AVX2", ((UUEncoder) encoder.Clone ()).Avx2Encode));

			foreach (var (path, encode) in paths)
				Fuzz.Equal (output, EncodeChunked (encoder, encode, input, random, $"{name} {path}"), $"{name} {path} vs default code path");

			RoundTrip (output, new UUDecoder (true), input, name);
			FilterRoundTrip (encoder, input, output, new DecoderFilter (new UUDecoder (true)), input, random, name);
		}

		static void TestYEnc (byte[] input, NewLineFormat newLineFormat, int maxLineLength, Random random)
		{
			var encoder = new YEncEncoder (newLineFormat, maxLineLength);
			var name = $"YEncEncoder ({newLineFormat}, {maxLineLength})";
			var output = TestStreaming (encoder, name, input, random, false);
			uint crc = Crc32 (input);

			// Note: TestStreaming resets the encoder when it is done, so recompute the checksum. The Checksum property
			// does not apply the final XOR, so callers (and the unit tests) need to complement it themselves.
			encoder.Flush (input, 0, input.Length, new byte[encoder.EstimateOutputLength (input.Length)]);

			Fuzz.Check (((uint) encoder.Checksum ^ 0xFFFFFFFF) == crc, $"{name}.Checksum was 0x{(uint) encoder.Checksum ^ 0xFFFFFFFF:X8} but expected 0x{crc:X8}");

			CheckYEncLines (NormalizeNewLines (output, newLineFormat, name), maxLineLength, name);
			RoundTrip (output, new YEncDecoder (true), input, name);
			FilterRoundTrip (encoder, input, output, new DecoderFilter (new YEncDecoder (true)), input, random, name);
		}

		static void TestPassThrough (byte[] input, ContentEncoding encoding, Random random)
		{
			var encoder = new PassThroughEncoder (encoding);
			var name = $"PassThroughEncoder ({encoding})";
			var output = TestStreaming (encoder, name, input, random, true);

			Fuzz.Equal (input, output, name);
			FilterRoundTrip (encoder, input, output, DecoderFilter.Create (encoding), input, random, name);
		}

		// Encodes the input all at once, again after Reset, again after the implicit reset done by Flush, in many chunks
		// and in many chunks using a clone taken mid-stream, verifying that all of the outputs match.
		static byte[] TestStreaming (IMimeEncoder encoder, string name, byte[] input, Random random, bool flushResets)
		{
			var whole = EncodeChunk (encoder, input, 0, input.Length, true, name);

			if (flushResets) {
				var again = EncodeChunk (encoder, input, 0, input.Length, true, name);

				Fuzz.Equal (whole, again, $"{name} after Flush");
			}

			encoder.Reset ();

			var chunked = new MemoryStream ();
			var cloned = new MemoryStream ();
			int cloneAt = input.Length == 0 ? 0 : random.Next (input.Length + 1);
			IMimeEncoder? clone = null;
			int index = 0;

			while (true) {
				if (clone is null && index >= cloneAt)
					clone = encoder.Clone ();

				if (index >= input.Length)
					break;

				int length = NextChunkLength (random, input.Length - index);
				var output = EncodeChunk (encoder, input, index, length, false, name);

				chunked.Write (output);

				if (clone is not null)
					cloned.Write (EncodeChunk (clone, input, index, length, false, $"{name} clone"));

				index += length;
			}

			chunked.Write (EncodeChunk (encoder, input, index, 0, true, name));
			cloned.Write (EncodeChunk (clone!, input, index, 0, true, $"{name} clone"));

			Fuzz.Equal (whole, chunked.ToArray (), $"{name} whole vs chunked");

			var tail = chunked.ToArray ().AsSpan ((int) (chunked.Length - cloned.Length));

			Fuzz.Check (chunked.Length >= cloned.Length, $"{name} clone produced more output than the original");
			Fuzz.Equal (tail, cloned.ToArray (), $"{name} clone vs original after {cloneAt} bytes");

			encoder.Reset ();

			return whole;
		}

		static int NextChunkLength (Random random, int remaining)
		{
			int length = random.Next (8) switch {
				0 => 0,
				1 => 1,
				2 => random.Next (1, 4),
				3 => random.Next (1, 80),
				4 => random.Next (1, 4096),
				_ => random.Next (1, 512),
			};

			return Math.Min (length, remaining);
		}

		static byte[] EncodeChunk (IMimeEncoder encoder, byte[] input, int startIndex, int length, bool flush, string name)
		{
			var method = flush ? "Flush" : "Encode";
			int estimate = encoder.EstimateOutputLength (length);
			var output = new byte[estimate + GuardLength];

			Array.Fill (output, Guard);

			int n = flush ? encoder.Flush (input, startIndex, length, output) : encoder.Encode (input, startIndex, length, output);

			Fuzz.Check (n >= 0 && n <= estimate, $"{name}.{method} returned {n} bytes but EstimateOutputLength ({length}) was {estimate}");
			CheckGuard (output, estimate, $"{name}.{method} overran its output estimate of {estimate} bytes for {length} bytes of input");

			return output.AsSpan (0, n).ToArray ();
		}

		// Encodes the input in chunks using one of the internal code-path-specific entry points of a clone of the encoder.
		static byte[] EncodeChunked (IMimeEncoder encoder, EncodeFunc encode, byte[] input, Random random, string name)
		{
			var result = new MemoryStream ();
			int index = 0;
			bool flush;

			do {
				int length = NextChunkLength (random, input.Length - index);

				flush = index + length == input.Length;

				int estimate = encoder.EstimateOutputLength (length);
				var output = new byte[estimate + GuardLength];

				Array.Fill (output, Guard);

				int n = encode (input, index, length, output, flush);

				Fuzz.Check (n >= 0 && n <= estimate, $"{name} returned {n} bytes but EstimateOutputLength ({length}) was {estimate}");
				CheckGuard (output, estimate, $"{name} overran its output estimate of {estimate} bytes for {length} bytes of input");

				result.Write (output, 0, n);
				index += length;
			} while (!flush);

			return result.ToArray ();
		}

		static void CheckGuard (byte[] output, int estimate, string message)
		{
			for (int i = estimate; i < output.Length; i++)
				Fuzz.Check (output[i] == Guard, message);
		}

		static void RoundTrip (byte[] encoded, IMimeDecoder decoder, byte[] expected, string name)
		{
			var output = new byte[decoder.EstimateOutputLength (encoded.Length)];
			int n = decoder.Decode (encoded, 0, encoded.Length, output);

			Fuzz.Equal (expected, output.AsSpan (0, n), $"{decoder.GetType ().Name} ({name} (x))");
		}

		// Encodes the input through an EncoderFilter using arbitrary write sizes (which must match the encoder's own
		// output) and then decodes it again through a DecoderFilter using arbitrary read sizes.
		static void FilterRoundTrip (IMimeEncoder encoder, byte[] input, byte[] expectedEncoding, IMimeFilter decoderFilter, byte[] expected, Random random, string name)
		{
			var encoded = new MemoryStream ();

			using (var filtered = new FilteredStream (encoded)) {
				int index = 0;

				filtered.Add (new EncoderFilter (encoder.Clone ()));

				while (index < input.Length) {
					int length = NextChunkLength (random, input.Length - index);

					filtered.Write (input, index, length);
					index += length;
				}

				filtered.Flush ();
			}

			Fuzz.Equal (expectedEncoding, encoded.ToArray (), $"EncoderFilter ({name}) vs {name}");

			var decoded = new MemoryStream ();

			encoded.Position = 0;

			using (var filtered = new FilteredStream (encoded)) {
				var buffer = new byte[4096];
				int n;

				filtered.Add (decoderFilter);

				do {
					int length = Math.Max (1, NextChunkLength (random, buffer.Length));

					n = filtered.Read (buffer, 0, length);
					decoded.Write (buffer, 0, n);
				} while (n > 0);
			}

			Fuzz.Equal (expected, decoded.ToArray (), $"DecoderFilter (EncoderFilter ({name}))");
		}

		static void CheckBase64Lines (byte[] output, int lineLength, string name)
		{
			int lineStart = 0;

			for (int i = 0; i < output.Length; i++) {
				byte c = output[i];

				if (c == (byte) '\n') {
					int length = i - lineStart;
					bool last = i + 1 == output.Length;

					Fuzz.Check (length > 0 && (last ? length <= lineLength : length == lineLength), $"{name} emitted a {length}-character line (expected {lineLength})");
					Fuzz.Check (length % 4 == 0, $"{name} emitted a line that is not a multiple of 4 characters");

					lineStart = i + 1;
				} else if (c == (byte) '=') {
					Fuzz.Check (i + 1 < output.Length && (output[i + 1] == (byte) '=' || output[i + 1] == (byte) '\n') && i + 2 >= output.Length - 1, $"{name} emitted padding before the end of the output at offset {i}");
				} else {
					Fuzz.Check (c is (>= (byte) 'A' and <= (byte) 'Z') or (>= (byte) 'a' and <= (byte) 'z') or (>= (byte) '0' and <= (byte) '9') or (byte) '+' or (byte) '/', $"{name} emitted 0x{c:X2} at offset {i}");
				}
			}

			Fuzz.Check (lineStart == output.Length, $"{name} output does not end with a newline");
		}

		static bool IsHexDigit (byte c)
		{
			return c is (>= (byte) '0' and <= (byte) '9') or (>= (byte) 'A' and <= (byte) 'F');
		}

		static void CheckQuotedPrintableLines (byte[] output, int maxLineLength, string name)
		{
			int lineStart = 0;

			for (int i = 0; i < output.Length; i++) {
				byte c = output[i];

				if (c == (byte) '\n') {
					int length = i - lineStart;

					Fuzz.Check (length <= maxLineLength, $"{name} emitted a {length}-character line (limit {maxLineLength}) at offset {lineStart}");
					Fuzz.Check (length == 0 || (output[i - 1] != (byte) ' ' && output[i - 1] != (byte) '\t'), $"{name} emitted trailing whitespace at offset {i - 1}");

					lineStart = i + 1;
				} else if (c == (byte) '=') {
					bool softBreak = i + 1 < output.Length && output[i + 1] == (byte) '\n';
					bool escape = i + 2 < output.Length && IsHexDigit (output[i + 1]) && IsHexDigit (output[i + 2]);

					Fuzz.Check (softBreak || escape, $"{name} emitted an invalid '=' sequence at offset {i}");
				} else {
					Fuzz.Check (c == (byte) ' ' || c == (byte) '\t' || (c >= 33 && c <= 126), $"{name} emitted 0x{c:X2} at offset {i}");
				}
			}

			Fuzz.Check (lineStart == output.Length, $"{name} output does not end with a newline");
		}

		static void CheckUULines (byte[] output, int inputLength, string name)
		{
			int lineStart = 0;
			int total = 0;
			bool ended = false;

			for (int i = 0; i < output.Length; i++) {
				byte c = output[i];

				if (c == (byte) '\n') {
					Fuzz.Check (!ended, $"{name} emitted data after the terminating line");
					Fuzz.Check (i > lineStart, $"{name} emitted an empty line at offset {i}");

					int count = (output[lineStart] - 0x20) & 0x3F;
					int length = i - lineStart - 1;

					Fuzz.Check (count <= 45, $"{name} emitted a line with a length of {count}");
					Fuzz.Check (length == (count + 2) / 3 * 4, $"{name} emitted {length} characters for a line of {count} bytes at offset {lineStart}");

					if (count == 0)
						ended = true;
					else if (total + count < inputLength)
						Fuzz.Check (count == 45, $"{name} emitted a short line of {count} bytes before the end of the input");

					total += count;
					lineStart = i + 1;
				} else {
					Fuzz.Check (c >= 0x21 && c <= 0x60, $"{name} emitted 0x{c:X2} at offset {i}");
				}
			}

			Fuzz.Check (ended && lineStart == output.Length, $"{name} output does not end with a terminating line");
			Fuzz.Check (total == inputLength, $"{name} line lengths add up to {total} but the input was {inputLength} bytes");
		}

		static void CheckYEncLines (byte[] output, int maxLineLength, string name)
		{
			int lineStart = 0;

			for (int i = 0; i < output.Length; i++) {
				byte c = output[i];

				if (c == (byte) '\n') {
					int length = i - lineStart;

					// Note: an escape sequence may straddle the line-length limit.
					Fuzz.Check (length > 0 && length <= maxLineLength + 1, $"{name} emitted a {length}-character line (limit {maxLineLength})");

					lineStart = i + 1;
				} else {
					Fuzz.Check (c != 0 && c != (byte) '\r', $"{name} emitted 0x{c:X2} at offset {i}");
				}
			}

			Fuzz.Check (lineStart == output.Length, $"{name} output does not end with a newline");
		}

		static uint Crc32 (byte[] input)
		{
			uint crc = 0xFFFFFFFF;

			foreach (var b in input) {
				crc ^= b;

				for (int i = 0; i < 8; i++)
					crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
			}

			return ~crc;
		}

		static byte[] EncodeOneShot (IRfc2047Encoder encoder, byte[] input, string name)
		{
			int estimate = encoder.EstimateOutputLength (input.Length);
			var output = new byte[estimate + GuardLength];

			Array.Fill (output, Guard);

			int n = encoder.Encode (input, 0, input.Length, output);

			Fuzz.Check (n >= 0 && n <= estimate, $"{name}.Encode returned {n} bytes but EstimateOutputLength ({input.Length}) was {estimate}");
			CheckGuard (output, estimate, $"{name}.Encode overran its output estimate of {estimate} bytes");

			return output.AsSpan (0, n).ToArray ();
		}

		static void TestRfc2047Base64 (byte[] input)
		{
			var output = EncodeOneShot (new Rfc2047Base64Encoder (), input, "Rfc2047Base64Encoder");

			Fuzz.Equal (Convert.ToBase64String (input), Fuzz.Latin1.GetString (output), "Rfc2047Base64Encoder vs Convert.ToBase64String");
		}

		static void TestRfc2047QuotedPrintable (byte[] input, QEncodeMode mode)
		{
			var name = $"Rfc2047QuotedPrintableEncoder ({mode})";
			var output = EncodeOneShot (new Rfc2047QuotedPrintableEncoder (mode), input, name);

			for (int i = 0; i < output.Length; i++) {
				byte c = output[i];

				if (c == (byte) '=') {
					Fuzz.Check (i + 2 < output.Length && IsHexDigit (output[i + 1]) && IsHexDigit (output[i + 2]), $"{name} emitted an invalid '=' sequence at offset {i}");
					i += 2;
				} else if (mode == QEncodeMode.Phrase) {
					// rfc2047, section 5 (3): an encoded-word in a phrase may only contain upper and lower case letters,
					// decimal digits, "!", "*", "+", "-", "/", "=", and "_".
					Fuzz.Check (c is (>= (byte) 'A' and <= (byte) 'Z') or (>= (byte) 'a' and <= (byte) 'z') or (>= (byte) '0' and <= (byte) '9') or (byte) '!' or (byte) '*' or (byte) '+' or (byte) '-' or (byte) '/' or (byte) '_', $"{name} emitted 0x{c:X2} at offset {i}");
				} else {
					// rfc2047, section 4.2: printable ASCII except "=", "?" and "_" (and SPACE which is encoded as "_").
					Fuzz.Check (c > 0x20 && c < 0x7F && c != (byte) '?', $"{name} emitted 0x{c:X2} at offset {i}");
				}
			}

			RoundTrip (output, new QuotedPrintableDecoder (true), input, name);
		}

		static void TestRfc2231 (byte[] input)
		{
			int estimate = Rfc2231Encoder.EstimateOutputLength (input.Length);
			var output = new byte[estimate + GuardLength];

			Array.Fill (output, Guard);

			int n = Rfc2231Encoder.Encode (input, 0, input.Length, output);

			Fuzz.Check (n >= 0 && n <= estimate, $"Rfc2231Encoder.Encode returned {n} bytes but EstimateOutputLength ({input.Length}) was {estimate}");
			CheckGuard (output, estimate, $"Rfc2231Encoder.Encode overran its output estimate of {estimate} bytes");

			var encoded = output.AsSpan (0, n).ToArray ();

			for (int i = 0; i < encoded.Length; i++) {
				byte c = encoded[i];

				if (c == (byte) '%') {
					Fuzz.Check (i + 2 < encoded.Length && IsHexDigit (encoded[i + 1]) && IsHexDigit (encoded[i + 2]), $"Rfc2231Encoder emitted an invalid '%' sequence at offset {i}");
					i += 2;
				} else {
					// rfc2231 attribute-char: any (US-ASCII) CHAR except SPACE, CTLs, "*", "'", "%", or tspecials.
					Fuzz.Check (c > 0x20 && c < 0x7F && "*'%()<>@,;:\\\"/[]?=".IndexOf ((char) c) < 0, $"Rfc2231Encoder emitted 0x{c:X2} at offset {i}");
				}
			}

			RoundTrip (encoded, new Rfc2231Decoder (), input, "Rfc2231Encoder");
		}

		static byte[] NormalizeNewLines (byte[] output, NewLineFormat newLineFormat, string name)
		{
			// Validate the requested format before normalizing for the line/alphabet checks.
			for (int i = 0; i < output.Length; i++) {
				if (output[i] == (byte) '\r')
					Fuzz.Check (newLineFormat == NewLineFormat.Dos && i + 1 < output.Length && output[i + 1] == (byte) '\n', $"{name} emitted an unexpected CR at offset {i}");
				else if (output[i] == (byte) '\n' && newLineFormat == NewLineFormat.Dos)
					Fuzz.Check (i > 0 && output[i - 1] == (byte) '\r', $"{name} emitted a bare LF at offset {i}");
			}

			return newLineFormat == NewLineFormat.Unix ? output : Fuzz.CanonicalizeLineBreaks (output, NewLineFormat.Unix);
		}
	}
}
