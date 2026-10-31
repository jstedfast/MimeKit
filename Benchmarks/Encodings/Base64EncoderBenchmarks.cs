//
// Base64EncoderBenchmarks.cs
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

using System;
using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit;
using MimeKit.Encodings;

namespace Benchmarks.Encodings {
	[MemoryDiagnoser]
	public class Base64EncoderBenchmarks
	{
		Base64Encoder encoder;
		byte[] input;
		byte[] output;
		int chunkSize;

		[Params ("MimeCrlf", "MimeLf", "Width60", "Width64", "Width68", "Width72", "SmallChunks", "Bulk", "SingleLine", "Small", "Tiny")]
		public string Layout { get; set; }

		[GlobalSetup]
		public void Setup ()
		{
			int length = Layout == "Tiny" ? 1 : Layout == "Small" ? 48 : Layout == "SingleLine" ? 57 : 65536;
			int lineLength = Layout.StartsWith ("Width", StringComparison.Ordinal) ? int.Parse (Layout.AsSpan (5)) : 76;
			var newLineFormat = Layout == "MimeLf" ? NewLineFormat.Unix : NewLineFormat.Dos;
			string newLine = newLineFormat == NewLineFormat.Dos ? "\r\n" : "\n";

			chunkSize = Layout == "SmallChunks" ? 17 : Layout == "Bulk" ? length : 4096;
			encoder = new Base64Encoder (newLineFormat, lineLength);
			input = new byte[length];
			new Random (12345).NextBytes (input);
			output = new byte[encoder.EstimateOutputLength (chunkSize)];

			string base64 = Convert.ToBase64String (input);
			var builder = new StringBuilder ();
			for (int offset = 0; offset < base64.Length; offset += lineLength) {
				builder.Append (base64, offset, Math.Min (lineLength, base64.Length - offset));
				builder.Append (newLine);
			}
			var expected = Encoding.ASCII.GetBytes (builder.ToString ());
			int encoded = 0;
			for (int offset = 0; offset < input.Length; offset += chunkSize) {
				int count = encoder.Encode (input, offset, Math.Min (chunkSize, input.Length - offset), output);
				if (!output.AsSpan (0, count).SequenceEqual (expected.AsSpan (encoded, count)))
					throw new InvalidOperationException ("Incorrect encoded output.");
				encoded += count;
			}
			int flushed = encoder.Flush (Array.Empty<byte> (), 0, 0, output);
			if (encoded + flushed != expected.Length || !output.AsSpan (0, flushed).SequenceEqual (expected.AsSpan (encoded)))
				throw new InvalidOperationException ("Incorrect flushed output.");
		}

		[Benchmark]
		public int Encode ()
		{
			encoder.Reset ();
			int encoded = 0;

			for (int offset = 0; offset < input.Length; offset += chunkSize)
				encoded += encoder.Encode (input, offset, Math.Min (chunkSize, input.Length - offset), output);

			return encoded + encoder.Flush (Array.Empty<byte> (), 0, 0, output);
		}
	}
}
