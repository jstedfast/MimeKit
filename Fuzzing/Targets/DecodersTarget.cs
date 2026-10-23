//
// DecodersTarget.cs
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

using MimeKit.Encodings;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Decodes the input with the decoder chosen by the first byte, checking that:
	//
	// * decoding in two chunks produces the same output as decoding all at once;
	// * a decoder cloned mid-stream behaves identically to the original;
	// * a decoder never writes more than EstimateOutputLength bytes (guard bytes must survive the scalar code path);
	// * the hardware-accelerated and scalar code paths produce identical output;
	// * decoding again after Reset reproduces the output.
	//
	// It also checks that each encoder respects its EstimateOutputLength and that encoding the input and
	// decoding the result reproduces it.
	sealed class DecodersTarget : IFuzzTarget
	{
		const int GuardLength = 64;
		const byte Guard = 0xCC;

		public string Name => "decoders";

		public int MaxLength => 1024 * 1024;

		static IMimeDecoder CreateDecoder (int kind)
		{
			return kind switch {
				0 => new Base64Decoder (),
				1 => new QuotedPrintableDecoder (false),
				2 => new QuotedPrintableDecoder (true),
				3 => new UUDecoder (false),
				4 => new UUDecoder (true),
				5 => new YEncDecoder (false),
				6 => new YEncDecoder (true),
				_ => new Rfc2231Decoder (),
			};
		}

		public void Run (ReadOnlySpan<byte> data)
		{
			if (data.Length == 0)
				return;

			int kind = data[0] & 7;
			var input = data.Slice (1).ToArray ();
			int split = input.Length == 0 ? 0 : (data[0] >> 3) * input.Length / 31;

			var decoder = CreateDecoder (kind);
			var name = decoder.GetType ().Name + (kind is 2 or 4 or 6 ? " (variant)" : string.Empty);

			var whole = Decode (decoder, input, 0, input.Length, name);
			decoder.Reset ();
			var again = Decode (decoder, input, 0, input.Length, name);

			Fuzz.Equal (whole, again, $"{name} after Reset");

			decoder.Reset ();
			var first = Decode (decoder, input, 0, split, name);
			var clone = decoder.Clone ();
			var second = Decode (decoder, input, split, input.Length - split, name);
			var cloned = Decode (clone, input, split, input.Length - split, name);

			Fuzz.Equal (second, cloned, $"{name} clone vs original after {split} bytes");

			var chunked = new byte[first.Length + second.Length];
			first.CopyTo (chunked, 0);
			second.CopyTo (chunked, first.Length);

			Fuzz.Equal (whole, chunked, $"{name} whole vs split at {split}");

			RoundTrip (new Base64Encoder (), new Base64Decoder (), input, split);
			RoundTrip (new UUEncoder (), new UUDecoder (true), input, split);
			RoundTrip (new YEncEncoder (), new YEncDecoder (true), input, split);
			RoundTrip (new QuotedPrintableEncoder (60 + (input.Length % 17)), new QuotedPrintableDecoder (false), input, split);
		}

		// Note: The array-based Decode() methods of some decoders (e.g. Base64Decoder and UUDecoder) use SIMD kernels
		// that are allowed to use the entire output array as scratch space, so the guard bytes are only checked when
		// decoding via the (scalar) pointer-based API using a clone of the decoder (which also verifies that the
		// hardware-accelerated and scalar code paths agree).
		static unsafe byte[] Decode (IMimeDecoder decoder, byte[] input, int startIndex, int length, string name)
		{
			int estimate = decoder.EstimateOutputLength (length);
			var scalar = decoder.Clone ();
			var output = new byte[estimate];

			int n = decoder.Decode (input, startIndex, length, output);

			Fuzz.Check (n >= 0 && n <= estimate, $"{name}.Decode returned {n} bytes but EstimateOutputLength ({length}) was {estimate}");

			var guarded = new byte[estimate + GuardLength];
			int m;

			Array.Fill (guarded, Guard);

			fixed (byte* inptr = input, outptr = guarded)
				m = scalar.Decode (inptr + startIndex, length, outptr);

			Fuzz.Check (m == n, $"{name}.Decode (byte*) returned {m} bytes but Decode (byte[]) returned {n} bytes");
			CheckGuard (guarded, estimate, $"{name}.Decode overran its output estimate of {estimate} bytes for {length} bytes of input");

			var result = output.AsSpan (0, n).ToArray ();

			Fuzz.Equal (result, guarded.AsSpan (0, m), $"{name}.Decode (byte*) vs Decode (byte[])");

			return result;
		}

		static void CheckGuard (byte[] output, int estimate, string message)
		{
			for (int i = estimate; i < output.Length; i++)
				Fuzz.Check (output[i] == Guard, message);
		}

		static void RoundTrip (IMimeEncoder encoder, IMimeDecoder decoder, byte[] input, int split)
		{
			var name = encoder.GetType ().Name;
			var encoded = new MemoryStream ();

			int estimate = encoder.EstimateOutputLength (split);
			var output = new byte[estimate + GuardLength];
			Array.Fill (output, Guard);

			int n = encoder.Encode (input, 0, split, output);
			Fuzz.Check (n >= 0 && n <= estimate, $"{name}.Encode returned {n} bytes but EstimateOutputLength ({split}) was {estimate}");
			CheckGuard (output, estimate, $"{name}.Encode overran its output estimate of {estimate} bytes");
			encoded.Write (output, 0, n);

			int remaining = input.Length - split;
			estimate = encoder.EstimateOutputLength (remaining);
			output = new byte[estimate + GuardLength];
			Array.Fill (output, Guard);

			n = encoder.Flush (input, split, remaining, output);
			Fuzz.Check (n >= 0 && n <= estimate, $"{name}.Flush returned {n} bytes but EstimateOutputLength ({remaining}) was {estimate}");
			CheckGuard (output, estimate, $"{name}.Flush overran its output estimate of {estimate} bytes");
			encoded.Write (output, 0, n);

			var bytes = encoded.ToArray ();
			var decoded = Decode (decoder, bytes, 0, bytes.Length, decoder.GetType ().Name);

			// The quoted-printable encoder treats CRLF as a hard line break and always emits it as a bare LF.
			var expected = encoder is QuotedPrintableEncoder ? CanonicalizeLineBreaks (input) : input;

			Fuzz.Equal (expected, decoded, $"{decoder.GetType ().Name} (Encode ({name}, x))");
		}

		static byte[] CanonicalizeLineBreaks (byte[] input)
		{
			var output = new MemoryStream (input.Length);

			for (int i = 0; i < input.Length; i++) {
				if (input[i] == (byte) '\r' && i + 1 < input.Length && input[i + 1] == (byte) '\n')
					continue;

				output.WriteByte (input[i]);
			}

			return output.ToArray ();
		}
	}
}
