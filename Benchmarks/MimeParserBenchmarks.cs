//
// MimeParserBenchmarks.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using BenchmarkDotNet.Attributes;

using MimeKit;

namespace Benchmarks {
    public class MimeParserBenchmarks
    {
		static readonly string MessagesDataDir = Path.Combine (BenchmarkHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (BenchmarkHelper.ProjectDir, "TestData", "mbox");

		static void BenchmarkMimeParser (string fileName, bool persistent = false)
		{
			var path = Path.Combine (MessagesDataDir, fileName);

			using (var stream = File.OpenRead (path)) {
				var parser = new MimeParser (stream, MimeFormat.Entity, persistent);

				for (int i = 0; i < 1000; i++) {
					parser.ParseMessage ();

					stream.Position = 0;
					parser.SetStream (stream, MimeFormat.Entity, persistent);
				}
			}
		}

		[Benchmark]
		public void BenchmarkStarTrekMessage ()
		{
			BenchmarkMimeParser ("startrek.eml");
		}

		[Benchmark]
		public void BenchmarkStarTrekMessagePersistent ()
		{
			BenchmarkMimeParser ("startrek.eml", true);
		}
	}
}
