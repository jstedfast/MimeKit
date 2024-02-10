//
// BestEncodingFilterBenchmarks.cs
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
using System.Buffers;

using MimeKit.IO;
using MimeKit.IO.Filters;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.IO.Filters {
	public class BestEncodingFilterBenchmarks : IDisposable
	{
		static readonly string TestDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData");
		readonly Stream LoremIpsum, GirlJpeg;

		public BestEncodingFilterBenchmarks ()
		{
			var path = Path.Combine (TestDataDir, "text", "lorem-ipsum.txt");
			var data = File.ReadAllBytes (path);

			LoremIpsum = new MemoryStream (data, false);

			path = Path.Combine (TestDataDir, "images", "girl.jpg");
			data = File.ReadAllBytes (path);

			GirlJpeg = new MemoryStream (data, false);
		}

		public void Dispose ()
		{
			LoremIpsum.Dispose ();
			GC.SuppressFinalize (this);
		}

		static void FilterInputStream (Stream input, IMimeFilter filter)
		{
			var buffer = ArrayPool<byte>.Shared.Rent (4096);

			try {
				for (int i = 0; i < 100; i++) {
					int n;

					while ((n = input.Read (buffer, 0, 4096)) > 0)
						filter.Filter (buffer, 0, n, out _, out _);
					filter.Flush (buffer, 0, 0, out _, out _);
					input.Position = 0;
					filter.Reset ();
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		[Benchmark]
		public void BestEncoding_LoremIpsum ()
		{
			FilterInputStream (LoremIpsum, new BestEncodingFilter ());
		}

		[Benchmark]
		public void BestEncoding_GirlJpeg ()
		{
			FilterInputStream (GirlJpeg, new BestEncodingFilter ());
		}
	}
}
