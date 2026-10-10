//
// RtfCompressedTarget.cs
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
	// Decompresses RTF ([MS-OXRTFCP]) whole and in two chunks, checking that both produce the same output
	// and that the output is bounded; then compresses the input (both compressed and uncompressed modes)
	// and checks that decompressing the result reproduces it with a valid CRC.
	sealed class RtfCompressedTarget : IFuzzTarget
	{
		public string Name => "rtf-compressed";

		public int MaxLength => 1024 * 1024;

		// A 2-byte dictionary reference expands to at most 17 bytes and every group of 8 tokens has a 1-byte
		// control prefix, so the output can never be more than 9 times the size of the input. (The decoder
		// stops at the end-of-stream reference rather than at RAWSIZE, so RAWSIZE cannot be used as the bound.)
		const int MaxExpansion = 9;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			int split = input.Length == 0 ? 0 : (int) ((long) input.Length * input[0] / 255);

			var whole = Decompress (input, input.Length, out var wholeMode, out var wholeCrc);
			var chunked = Decompress (input, split, out var chunkedMode, out var chunkedCrc);

			Fuzz.Equal (whole, chunked, $"RtfCompressedToRtf whole vs split at {split}");
			Fuzz.Check (wholeMode == chunkedMode, $"RtfCompressedToRtf CompressionMode differs: {wholeMode} vs {chunkedMode}");
			Fuzz.Check (wholeCrc == chunkedCrc, "RtfCompressedToRtf IsValidCrc32 differs between whole and split input");
			Fuzz.Check (whole.Length <= (long) input.Length * MaxExpansion, $"RtfCompressedToRtf produced {whole.Length} bytes from {input.Length} bytes of input");

			RoundTrip (input, split, true);
			RoundTrip (input, split, false);
		}

		internal static byte[] Decompress (byte[] input, int split, out RtfCompressionMode mode, out bool validCrc)
		{
			var filter = new RtfCompressedToRtf ();
			var output = new MemoryStream ();
			byte[] buffer;
			int index, length;

			buffer = filter.Filter (input, 0, split, out index, out length);
			output.Write (buffer, index, length);

			buffer = filter.Flush (input, split, input.Length - split, out index, out length);
			output.Write (buffer, index, length);

			mode = filter.CompressionMode;
			validCrc = filter.IsValidCrc32;

			return output.ToArray ();
		}

		static void RoundTrip (byte[] input, int split, bool compress)
		{
			using var output = new MemoryStream ();
			string? error = null;
			bool closed = false;

			using (var stream = new RtfCompressedWriteStream (output, compress, (s, e) => { closed = true; error = e; })) {
				stream.Write (input, 0, split);
				stream.Write (input, split, input.Length - split);
			}

			Fuzz.Check (closed, "RtfCompressedWriteStream did not invoke its closed callback");
			Fuzz.Check (error == null, $"RtfCompressedWriteStream failed: {error}");

			var encoded = output.ToArray ();
			var decoded = Decompress (encoded, encoded.Length, out var mode, out var validCrc);

			Fuzz.Equal (input, decoded, $"RTF {(compress ? "compression" : "uncompressed encoding")} round trip");
			Fuzz.Check (mode == (compress ? RtfCompressionMode.Compressed : RtfCompressionMode.Uncompressed), $"Round trip CompressionMode is {mode}");

			if (compress)
				Fuzz.Check (validCrc, "Compressed RTF produced by RtfCompressedWriteStream has an invalid CRC");
		}
	}
}
