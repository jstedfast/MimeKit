//
// Unix2DosFilterBenchmarks.cs
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
	public class Unix2DosFilterBenchmarks
	{
		static readonly string TextDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "text");
		static readonly byte[] LoremIpsumDos, LoremIpsumUnix, PathologicalDos, PathologicalUnix;

		static Unix2DosFilterBenchmarks ()
		{
			var path = Path.Combine (TextDataDir, "lorem-ipsum.txt");
			using var stream = File.OpenRead (path);

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());
					stream.CopyTo (filtered);
					filtered.Flush ();

					LoremIpsumDos = memory.ToArray ();
				}
			}

			stream.Position = 0;

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());
					stream.CopyTo (filtered);
					filtered.Flush ();

					LoremIpsumUnix = memory.ToArray ();
				}
			}

			PathologicalDos = new byte[2048];
			for (int i = 0; i < PathologicalDos.Length; ) {
				PathologicalDos[i++] = (byte) '\r';
				PathologicalDos[i++] = (byte) '\n';
			}

			PathologicalUnix = new byte[1024];
			PathologicalUnix.AsSpan ().Fill ((byte) '\n');
		}

		static void FilterInputStream (byte[] data, IMimeFilter filter)
		{
			using var input = new MemoryStream (data, false);
			using var output = new MeasuringStream ();
			using var filtered = new FilteredStream (output);

			filtered.Add (filter);
			input.CopyTo (filtered);
			filtered.Flush ();
		}

		[Benchmark]
		public void Dos2Unix_LoremIpsumDos ()
		{
			FilterInputStream (LoremIpsumDos, new Dos2UnixFilter ());
		}

		[Benchmark]
		public void Dos2Unix_LoremIpsumUnix ()
		{
			FilterInputStream (LoremIpsumUnix, new Dos2UnixFilter ());
		}

		[Benchmark]
		public void Dos2Unix_PathologicalDos ()
		{
			FilterInputStream (PathologicalDos, new Dos2UnixFilter ());
		}

		[Benchmark]
		public void Dos2Unix_PathologicalUnix ()
		{
			FilterInputStream (PathologicalUnix, new Dos2UnixFilter ());
		}

		[Benchmark]
		public void Unix2Dos_LoremIpsumDos ()
		{
			FilterInputStream (LoremIpsumDos, new Unix2DosFilter ());
		}

		[Benchmark]
		public void Unix2Dos_LoremIpsumUnix ()
		{
			FilterInputStream (LoremIpsumUnix, new Unix2DosFilter ());
		}

		[Benchmark]
		public void Unix2Dos_PathologicalDos ()
		{
			FilterInputStream (PathologicalDos, new Unix2DosFilter ());
		}

		[Benchmark]
		public void Unix2Dos_PathologicalUnix ()
		{
			FilterInputStream (PathologicalUnix, new Unix2DosFilter ());
		}
	}
}
