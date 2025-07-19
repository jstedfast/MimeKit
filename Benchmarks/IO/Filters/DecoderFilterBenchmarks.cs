//
// DecoderFilterBenchmarks.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.IO;
using System.Runtime.CompilerServices;

using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.IO.Filters {
	public class DecoderFilterBenchmarks
	{
		readonly byte[] QuotedPrintableData, Base64Data, UUEncodedData;

		public DecoderFilterBenchmarks ()
		{
			var dataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "encoders");
			var path = Path.Combine (dataDir, "wikipedia.qp");
			QuotedPrintableData = File.ReadAllBytes (path);

			path = Path.Combine (dataDir, "photo.b64");
			Base64Data = File.ReadAllBytes (path);

			path = Path.Combine (dataDir, "photo.uu");
			UUEncodedData = File.ReadAllBytes (path);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void Decode (byte[] data, IMimeDecoder decoder)
		{
			var maxLength = decoder.EstimateOutputLength (data.Length);
			var output = new byte[maxLength];

			decoder.Decode (data, 0, data.Length, output);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void DecodeStream (byte[] data, IMimeDecoder decoder)
		{
			using var input = new MemoryStream (data, false);
			using var output = new MeasuringStream ();
			using var filtered = new FilteredStream (output);

			filtered.Add (new DecoderFilter (decoder));
			input.CopyTo (filtered);
			filtered.Flush ();
		}

		[Benchmark]
		public void Base64Decode ()
		{
			Decode (Base64Data, new Base64Decoder ());
		}

		[Benchmark]
		public void Base64DecodeStream ()
		{
			DecodeStream (Base64Data, new Base64Decoder ());
		}

		[Benchmark]
		public void QuotedPrintableDecode ()
		{
			Decode (QuotedPrintableData, new QuotedPrintableDecoder ());
		}

		[Benchmark]
		public void QuotedPrintableDecodeStream ()
		{
			DecodeStream (QuotedPrintableData, new QuotedPrintableDecoder ());
		}

		[Benchmark]
		public void UUDecode ()
		{
			Decode (UUEncodedData, new UUDecoder ());
		}

		[Benchmark]
		public void UUDecodeStream ()
		{
			DecodeStream (UUEncodedData, new UUDecoder ());
		}
	}
}
