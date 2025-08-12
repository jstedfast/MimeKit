//
// ArmoredFromFilterBenchmarks.cs
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

using MimeKit.IO;
using MimeKit.IO.Filters;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.IO.Filters {
	public class ArmoredFromFilterBenchmarks
	{
		static readonly byte[] FromRussiaWithLove;

		static ArmoredFromFilterBenchmarks ()
		{
			var data = "From Russia with love is one of my favorite James Bond files.\n"u8;

			FromRussiaWithLove = new byte[data.Length * 1000];
			var output = FromRussiaWithLove.AsSpan ();

			for (int i = 0; i < 1000; i++) {
				var slice = output.Slice (i * data.Length, data.Length);
				data.CopyTo (slice);
			}
		}

		static void BenchmarkFilter (IMimeFilter filter, byte[] data, int bufferSize)
		{
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (filter);

					using (var inputStream = new MemoryStream (data, false)) {
						var buffer = new byte[bufferSize];
						int nread;

						do {
							nread = inputStream.Read (buffer, 0, buffer.Length);
							if (nread == 0)
								break;

							filtered.Write (buffer, 0, nread);
						} while (true);

						filtered.Flush ();
					}
				}
			}
		}

		[Benchmark]
		public void ArmoredFromFilter_FromRussiaWithLove ()
		{
			BenchmarkFilter (new ArmoredFromFilter (), FromRussiaWithLove, 1024);
		}
	}
}
