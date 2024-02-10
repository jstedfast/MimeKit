//
// DecoderFilterBenchmarks.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.IO.Filters {
	public class DecoderFilterBenchmarks : IDisposable
	{
		static readonly string EncoderDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "encoders");
		readonly Stream QuotedPrintableData, Base64Data, UUEncodedData;

		public DecoderFilterBenchmarks ()
		{
			var path = Path.Combine (EncoderDataDir, "wikipedia.qp");
			var data = File.ReadAllBytes (path);

			QuotedPrintableData = new MemoryStream (data, false);

			path = Path.Combine (EncoderDataDir, "photo.b64");
			data = File.ReadAllBytes (path);

			Base64Data = new MemoryStream (data, false);

			path = Path.Combine (EncoderDataDir, "photo.uu");
			data = File.ReadAllBytes (path);

			UUEncodedData = new MemoryStream (data, false);
		}

		public void Dispose ()
		{
			QuotedPrintableData.Dispose ();
			UUEncodedData.Dispose ();
			Base64Data.Dispose ();

			GC.SuppressFinalize (this);
		}

		static void FilterInputStream (Stream input, IMimeDecoder decoder)
		{
			using var output = new MeasuringStream ();
			using var filtered = new FilteredStream (output);

			filtered.Add (new DecoderFilter (decoder));
			input.Position = 0;
			input.CopyTo (filtered);
			filtered.Flush ();
		}

		[Benchmark]
		public void Base64Decoder ()
		{
			FilterInputStream (Base64Data, new Base64Decoder ());
		}

		[Benchmark]
		public void QuotedPrintableDecoder ()
		{
			FilterInputStream (QuotedPrintableData, new QuotedPrintableDecoder ());
		}

		[Benchmark]
		public void UUDecoder ()
		{
			FilterInputStream (UUEncodedData, new UUDecoder ());
		}
	}
}
