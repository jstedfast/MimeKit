//
// EncoderFilterBenchmarks.cs
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
	public class EncoderFilterBenchmarks : IDisposable
	{
		static readonly string EncoderDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "encoders");
		readonly Stream BinaryData, TextData;

		public EncoderFilterBenchmarks ()
		{
			var path = Path.Combine (EncoderDataDir, "wikipedia.txt");
			var data = File.ReadAllBytes (path);

			TextData = new MemoryStream (data, false);

			path = Path.Combine (EncoderDataDir, "photo.jpg");
			data = File.ReadAllBytes (path);

			BinaryData = new MemoryStream (data, false);
		}

		public void Dispose ()
		{
			BinaryData.Dispose ();
			TextData.Dispose ();

			GC.SuppressFinalize (this);
		}

		static void FilterInputStream (Stream input, IMimeEncoder encoder)
		{
			using var output = new MeasuringStream ();
			using var filtered = new FilteredStream (output);

			filtered.Add (new EncoderFilter (encoder));
			input.Position = 0;
			input.CopyTo (filtered);
			filtered.Flush ();
		}

		[Benchmark]
		public void Base64Encoder ()
		{
			FilterInputStream (BinaryData, new Base64Encoder ());
		}

		[Benchmark]
		public void HexEncoder ()
		{
			FilterInputStream (BinaryData, new HexEncoder ());
		}

		[Benchmark]
		public void QEncoder ()
		{
			FilterInputStream (TextData, new QEncoder (QEncodeMode.Text));
		}

		[Benchmark]
		public void QuotedPrintableEncoder ()
		{
			FilterInputStream (TextData, new QuotedPrintableEncoder ());
		}

		[Benchmark]
		public void UUEncoder ()
		{
			FilterInputStream (BinaryData, new UUEncoder ());
		}
	}
}
