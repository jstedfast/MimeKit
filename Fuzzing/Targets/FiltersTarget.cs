//
// FiltersTarget.cs
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

using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Runs the input through the stateful filter chosen by the first byte (the second byte selects an option and seeds
	// the chunking pattern), checking that:
	//
	// * filtering the input in many (possibly empty) chunks produces the same output and end state as filtering it all
	//   at once;
	// * filtering again after Reset reproduces the output and end state;
	// * writing through a FilteredStream (sync and async, arbitrary write sizes) and reading through one (arbitrary
	//   read sizes) produce the same output;
	// * the output matches a straightforward reference implementation of the filter's documented behavior.
	//
	// The filters covered are Dos2Unix, Unix2Dos, ArmoredFrom, MboxFrom, TrailingWhitespace, the DKIM simple and relaxed
	// body canonicalization filters, BestEncoding, OpenPgpDetection and OpenPgpBlock.
	sealed class FiltersTarget : IFuzzTarget
	{
		static readonly string[][] OpenPgpBlocks = {
			new[] { "-----BEGIN PGP MESSAGE-----", "-----END PGP MESSAGE-----" },
			new[] { "-----BEGIN PGP SIGNED MESSAGE-----", "-----BEGIN PGP SIGNATURE-----", "-----END PGP SIGNATURE-----" },
			new[] { "-----BEGIN PGP PUBLIC KEY BLOCK-----", "-----END PGP PUBLIC KEY BLOCK-----" },
			new[] { "-----BEGIN PGP PRIVATE KEY BLOCK-----", "-----END PGP PRIVATE KEY BLOCK-----" },
		};

		static readonly OpenPgpDataType[] OpenPgpDataTypes = {
			OpenPgpDataType.EncryptedMessage,
			OpenPgpDataType.SignedMessage,
			OpenPgpDataType.PublicKey,
			OpenPgpDataType.PrivateKey
		};

		static readonly int[] LineLengths = { FormatOptions.MinimumLineLength, 72, 78, 998, FormatOptions.MaximumLineLength };

		public string Name => "filters";

		public int MaxLength => 256 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			if (data.Length < 2)
				return;

			int kind = data[0] % 13;
			int option = data[1];
			var input = data.Slice (2).ToArray ();
			var random = new Random ((data[0] << 8) | data[1]);

			switch (kind) {
			case 0:
			case 1: TestDos2Unix (input, kind == 1, random); break;
			case 2:
			case 3: TestUnix2Dos (input, kind == 3, random); break;
			case 4: TestFrom (input, true, random); break;
			case 5: TestFrom (input, false, random); break;
			case 6: TestTrailingWhitespace (input, random); break;
			case 7: TestDkimSimple (input, (option & 1) != 0, random); break;
			case 8: TestDkimRelaxed (input, (option & 1) != 0, random); break;
			case 9: TestBestEncoding (input, random); break;
			case 10: TestOpenPgpDetection (input, random); break;
			default: TestOpenPgpBlock (input, option % OpenPgpBlocks.Length, random); break;
			}
		}

		static void TestDos2Unix (byte[] input, bool ensureNewLine, Random random)
		{
			var name = $"Dos2UnixFilter ({ensureNewLine})";
			var output = TestFilter (() => new Dos2UnixFilter (ensureNewLine), null, name, input, random);
			var expected = new List<byte> (input.Length + 1);

			for (int i = 0; i < input.Length; i++) {
				if (input[i] != (byte) '\r' || i + 1 == input.Length || input[i + 1] != (byte) '\n')
					expected.Add (input[i]);
			}

			if (ensureNewLine) {
				if (expected.Count > 0 && expected[expected.Count - 1] == (byte) '\r')
					expected[expected.Count - 1] = (byte) '\n';
				else if (expected.Count == 0 || expected[expected.Count - 1] != (byte) '\n')
					expected.Add ((byte) '\n');
			}

			Fuzz.Equal (expected.ToArray (), output, $"{name} vs reference");
		}

		static void TestUnix2Dos (byte[] input, bool ensureNewLine, Random random)
		{
			var name = $"Unix2DosFilter ({ensureNewLine})";
			var output = TestFilter (() => new Unix2DosFilter (ensureNewLine), null, name, input, random);
			var expected = new List<byte> (input.Length * 2 + 2);

			for (int i = 0; i < input.Length; i++) {
				if (input[i] == (byte) '\n' && (i == 0 || input[i - 1] != (byte) '\r'))
					expected.Add ((byte) '\r');

				expected.Add (input[i]);
			}

			if (ensureNewLine && (expected.Count == 0 || expected[expected.Count - 1] != (byte) '\n')) {
				if (expected.Count == 0 || expected[expected.Count - 1] != (byte) '\r')
					expected.Add ((byte) '\r');

				expected.Add ((byte) '\n');
			}

			Fuzz.Equal (expected.ToArray (), output, $"{name} vs reference");

			for (int i = 0; i < output.Length; i++)
				Fuzz.Check (output[i] != (byte) '\n' || (i > 0 && output[i - 1] == (byte) '\r'), $"{name}: bare LF at offset {i}");
		}

		static void TestFrom (byte[] input, bool armored, Random random)
		{
			var name = armored ? "ArmoredFromFilter" : "MboxFromFilter";
			var output = TestFilter (() => armored ? new ArmoredFromFilter () : new MboxFromFilter (), null, name, input, random);
			var expected = new List<byte> (input.Length);
			var marker = "From "u8;

			for (int i = 0; i < input.Length; i++) {
				if ((i == 0 || input[i - 1] == (byte) '\n') && input.AsSpan (i).StartsWith (marker)) {
					if (armored) {
						expected.Add ((byte) '=');
						expected.Add ((byte) '4');
						expected.Add ((byte) '6');
						continue;
					}

					expected.Add ((byte) '>');
				}

				expected.Add (input[i]);
			}

			Fuzz.Equal (expected.ToArray (), output, $"{name} vs reference");
		}

		static void TestTrailingWhitespace (byte[] input, Random random)
		{
			const string name = "TrailingWhitespaceFilter";
			var output = TestFilter (() => new TrailingWhitespaceFilter (), null, name, input, random);
			var expected = new List<byte> (input.Length);
			int blanks = -1;

			for (int i = 0; i < input.Length; i++) {
				byte c = input[i];

				if (c.IsBlank ()) {
					if (blanks < 0)
						blanks = i;
				} else {
					if (blanks >= 0 && c != (byte) '\r' && c != (byte) '\n') {
						for (int j = blanks; j < i; j++)
							expected.Add (input[j]);
					}

					expected.Add (c);
					blanks = -1;
				}
			}

			Fuzz.Equal (expected.ToArray (), output, $"{name} vs reference");
		}

		// Strips every CR and then converts every LF into a CRLF so that the input is in the canonical form that the
		// DKIM body filters expect (they are always preceded by a new-line filter).
		static byte[] Canonicalize (byte[] input)
		{
			var output = new List<byte> (input.Length * 2);

			foreach (var c in input) {
				if (c == (byte) '\n')
					output.Add ((byte) '\r');

				if (c != (byte) '\r')
					output.Add (c);
			}

			return output.ToArray ();
		}

		static List<byte[]> SplitCrlfLines (byte[] input, out byte[] tail)
		{
			var lines = new List<byte[]> ();
			int start = 0;

			for (int i = 0; i + 1 < input.Length; i++) {
				if (input[i] == (byte) '\r' && input[i + 1] == (byte) '\n') {
					lines.Add (input.AsSpan (start, i - start).ToArray ());
					start = i + 2;
					i++;
				}
			}

			tail = input.AsSpan (start).ToArray ();

			return lines;
		}

		static byte[] DkimReference (byte[] input, Func<byte[], byte[]> canonicalizeLine)
		{
			var lines = SplitCrlfLines (input, out var tail);
			var output = new List<byte> (input.Length);
			int emptyLines = 0;

			foreach (var raw in lines) {
				var line = canonicalizeLine (raw);

				if (line.Length == 0) {
					emptyLines++;
					continue;
				}

				for (; emptyLines > 0; emptyLines--) {
					output.Add ((byte) '\r');
					output.Add ((byte) '\n');
				}

				output.AddRange (line);
				output.Add ((byte) '\r');
				output.Add ((byte) '\n');
			}

			tail = canonicalizeLine (tail);

			if (tail.Length > 0) {
				for (; emptyLines > 0; emptyLines--) {
					output.Add ((byte) '\r');
					output.Add ((byte) '\n');
				}

				output.AddRange (tail);
			}

			return output.ToArray ();
		}

		// RFC 6376, section 3.4.4: reduce all sequences of WSP within a line to a single SP and ignore all WSP at the
		// end of the line.
		static byte[] RelaxLine (byte[] line)
		{
			var output = new List<byte> (line.Length);
			bool lwsp = false;

			foreach (var c in line) {
				if (c.IsBlank ()) {
					lwsp = true;
				} else {
					if (lwsp)
						output.Add ((byte) ' ');

					output.Add (c);
					lwsp = false;
				}
			}

			return output.ToArray ();
		}

		static void TestDkimSimple (byte[] input, bool canonicalize, Random random)
		{
			const string name = "DkimSimpleBodyFilter";

			if (canonicalize)
				input = Canonicalize (input);

			var output = TestFilter (() => new DkimSimpleBodyFilter (), f => $"LastWasNewLine={((DkimBodyFilter) f).LastWasNewLine}", name, input, random);
			var filter = new DkimSimpleBodyFilter ();

			filter.Flush (input, 0, input.Length, out _, out _);

			// Note: an empty body canonicalizes to a single CRLF, which the caller appends when LastWasNewLine is false.
			Fuzz.Check (filter.LastWasNewLine == (output.Length > 0 && output[output.Length - 1] == (byte) '\n'), $"{name}: LastWasNewLine is inconsistent with the output");

			if (canonicalize)
				Fuzz.Equal (DkimReference (input, line => line), output, $"{name} vs reference");
		}

		static void TestDkimRelaxed (byte[] input, bool canonicalize, Random random)
		{
			const string name = "DkimRelaxedBodyFilter";

			if (canonicalize)
				input = Canonicalize (input);

			var output = TestFilter (() => new DkimRelaxedBodyFilter (), f => $"LastWasNewLine={((DkimBodyFilter) f).LastWasNewLine}", name, input, random);
			var filter = new DkimRelaxedBodyFilter ();

			filter.Flush (input, 0, input.Length, out _, out _);

			// Note: an empty body canonicalizes to an empty string, so LastWasNewLine must be true to prevent the caller
			// from appending a CRLF.
			Fuzz.Check (filter.LastWasNewLine == (output.Length == 0 || output[output.Length - 1] == (byte) '\n'), $"{name}: LastWasNewLine is inconsistent with the output");

			if (canonicalize)
				Fuzz.Equal (DkimReference (input, RelaxLine), output, $"{name} vs reference");
		}

		static string DescribeBestEncoding (IMimeFilter filter)
		{
			var best = (BestEncodingFilter) filter;
			var builder = new System.Text.StringBuilder ();

			foreach (var constraint in new[] { EncodingConstraint.None, EncodingConstraint.EightBit, EncodingConstraint.SevenBit }) {
				foreach (var lineLength in LineLengths)
					builder.Append ($"{constraint}/{lineLength}={best.GetBestEncoding (constraint, lineLength)} ");
			}

			return builder.ToString ();
		}

		static void TestBestEncoding (byte[] input, Random random)
		{
			const string name = "BestEncodingFilter";
			var output = TestFilter (() => new BestEncodingFilter (), DescribeBestEncoding, name, input, random);

			Fuzz.Equal (input, output, $"{name} is a pass-through filter");
		}

		static bool IsMarkerLine (byte[] input, int lineIndex, int lineEnd, string marker)
		{
			// Note: lineEnd is the index of the terminating LF; the marker may be followed by a single CR.
			int length = lineEnd - lineIndex;

			if (length > marker.Length && input[lineEnd - 1] == (byte) '\r')
				length--;

			if (length != marker.Length)
				return false;

			for (int i = 0; i < length; i++) {
				if (input[lineIndex + i] != (byte) marker[i])
					return false;
			}

			return true;
		}

		static void TestOpenPgpDetection (byte[] input, Random random)
		{
			const string name = "OpenPgpDetectionFilter";
			var output = TestFilter (() => new OpenPgpDetectionFilter (), f => {
				var detection = (OpenPgpDetectionFilter) f;
				return $"DataType={detection.DataType} BeginOffset={detection.BeginOffset} EndOffset={detection.EndOffset}";
			}, name, input, random);
			var filter = new OpenPgpDetectionFilter ();
			var dataType = OpenPgpDataType.None;
			int? beginOffset = null, endOffset = null;
			int block = -1, step = 0;
			int lineIndex = 0;

			filter.Flush (input, 0, input.Length, out _, out _);

			for (int i = 0; i < input.Length && endOffset == null; i++) {
				if (input[i] != (byte) '\n')
					continue;

				if (block < 0) {
					for (int j = 0; j < OpenPgpBlocks.Length; j++) {
						if (IsMarkerLine (input, lineIndex, i, OpenPgpBlocks[j][0])) {
							beginOffset = lineIndex;
							block = j;
							step = 1;
							break;
						}
					}
				} else if (IsMarkerLine (input, lineIndex, i, OpenPgpBlocks[block][step])) {
					if (++step == OpenPgpBlocks[block].Length) {
						dataType = OpenPgpDataTypes[block];
						endOffset = i + 1;
					}
				}

				lineIndex = i + 1;
			}

			var expected = beginOffset.HasValue ? input.AsSpan (beginOffset.Value, (endOffset ?? input.Length) - beginOffset.Value) : ReadOnlySpan<byte>.Empty;

			Fuzz.Equal (expected, output, $"{name} vs reference");
			Fuzz.Check (filter.DataType == dataType, $"{name}: DataType is {filter.DataType}, expected {dataType}");
			Fuzz.Check (filter.BeginOffset == beginOffset, $"{name}: BeginOffset is {filter.BeginOffset}, expected {beginOffset}");
			Fuzz.Check (filter.EndOffset == endOffset, $"{name}: EndOffset is {filter.EndOffset}, expected {endOffset}");
		}

		static void TestOpenPgpBlock (byte[] input, int block, Random random)
		{
			var markers = OpenPgpBlocks[block];
			var begin = markers[0];
			var end = markers[markers.Length - 1];
			var name = $"OpenPgpBlockFilter ({begin})";
			var output = TestFilter (() => new OpenPgpBlockFilter (begin, end), null, name, input, random);
			int beginOffset = -1, endOffset = input.Length;
			int lineIndex = 0;

			for (int i = 0; i < input.Length; i++) {
				if (input[i] != (byte) '\n')
					continue;

				if (beginOffset < 0) {
					if (IsMarkerLine (input, lineIndex, i, begin))
						beginOffset = lineIndex;
				} else if (IsMarkerLine (input, lineIndex, i, end)) {
					endOffset = i + 1;
					break;
				}

				lineIndex = i + 1;
			}

			var expected = beginOffset >= 0 ? input.AsSpan (beginOffset, endOffset - beginOffset) : ReadOnlySpan<byte>.Empty;

			Fuzz.Equal (expected, output, $"{name} vs reference");
		}

		static int NextChunkSize (Random random, int remaining)
		{
			switch (random.Next (8)) {
			case 0: return 0;
			case 1:
			case 2: return Math.Min (remaining, random.Next (1, 8));
			case 3:
			case 4: return Math.Min (remaining, random.Next (1, 80));
			default: return Math.Min (remaining, random.Next (1, 4096));
			}
		}

		static void Append (List<byte> output, byte[] buffer, int index, int length)
		{
			for (int i = 0; i < length; i++)
				output.Add (buffer[index + i]);
		}

		static byte[] FilterChunked (IMimeFilter filter, byte[] input, Random random)
		{
			var output = new List<byte> (input.Length);
			int index = 0;
			byte[] buffer;
			int outputIndex, outputLength;

			while (index < input.Length) {
				int n = NextChunkSize (random, input.Length - index);

				// Sometimes finish with a Flush of the final chunk rather than an empty Flush.
				if (index + n == input.Length && n > 0 && random.Next (2) == 0)
					break;

				buffer = filter.Filter (input, index, n, out outputIndex, out outputLength);
				Append (output, buffer, outputIndex, outputLength);
				index += n;
			}

			buffer = filter.Flush (input, index, input.Length - index, out outputIndex, out outputLength);
			Append (output, buffer, outputIndex, outputLength);

			return output.ToArray ();
		}

		static byte[] FilterWhole (IMimeFilter filter, byte[] input)
		{
			var buffer = filter.Flush (input, 0, input.Length, out int outputIndex, out int outputLength);

			return buffer.AsSpan (outputIndex, outputLength).ToArray ();
		}

		static byte[] WriteFiltered (IMimeFilter filter, byte[] input, Random random, bool async)
		{
			using var memory = new MemoryStream ();

			using (var filtered = new FilteredStream (memory)) {
				int index = 0;

				filtered.Add (filter);

				while (index < input.Length) {
					int n = NextChunkSize (random, input.Length - index);

					if (async)
						Fuzz.Wait (filtered.WriteAsync (input, index, n));
					else
						filtered.Write (input, index, n);

					index += n;
				}

				if (async)
					Fuzz.Wait (filtered.FlushAsync ());
				else
					filtered.Flush ();
			}

			return memory.ToArray ();
		}

		static byte[] ReadFiltered (IMimeFilter filter, byte[] input, Random random)
		{
			using var filtered = new FilteredStream (new MemoryStream (input, false));
			var output = new List<byte> (input.Length);
			var buffer = new byte[4096];
			int n;

			filtered.Add (filter);

			do {
				int size = NextChunkSize (random, buffer.Length);

				if (size == 0)
					size = 1;

				if ((n = filtered.Read (buffer, 0, size)) > 0)
					Append (output, buffer, 0, n);
			} while (n > 0);

			return output.ToArray ();
		}

		static byte[] TestFilter (Func<IMimeFilter> create, Func<IMimeFilter, string>? describe, string name, byte[] input, Random random)
		{
			var filter = create ();
			var expected = FilterWhole (filter, input);
			var state = describe?.Invoke (filter);

			void Verify (IMimeFilter f, byte[] actual, string how)
			{
				Fuzz.Equal (expected, actual, $"{name} {how} vs whole");

				if (describe != null)
					Fuzz.Equal (state!, describe (f), $"{name} {how} state vs whole");
			}

			filter.Reset ();
			Verify (filter, FilterChunked (filter, input, random), "chunked");

			filter.Reset ();
			Verify (filter, FilterWhole (filter, input), "after Reset");

			filter = create ();
			Verify (filter, WriteFiltered (filter, input, random, false), "FilteredStream.Write");

			filter = create ();
			Verify (filter, WriteFiltered (filter, input, random, true), "FilteredStream.WriteAsync");

			filter = create ();
			Verify (filter, ReadFiltered (filter, input, random), "FilteredStream.Read");

			return expected;
		}
	}
}
