//
// Program.cs
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

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Exporters;

namespace Benchmarks {
	public class Program
	{
		public static void Main (string[] args)
		{
#if DEBUG
			var config = new DebugInProcessConfig ()
				.WithOptions (ConfigOptions.DisableOptimizationsValidator)
				.AddExporter (MarkdownExporter.GitHub);
#else
			var config = ManualConfig.CreateMinimumViable ();
				//.AddExporter (MarkdownExporter.GitHub);
#endif

#if false
			// Only run benchmarks for the MimeParser, ExperimentalMimeParser, and MimeReader classes
			config.AddFilter (new DisjunctionFilter (
				new NameFilter (name => name.StartsWith ("MimeParser_", StringComparison.Ordinal)),
				new NameFilter (name => name.StartsWith ("ExperimentalMimeParser_", StringComparison.Ordinal)),
				new NameFilter (name => name.StartsWith ("MimeReader_", StringComparison.Ordinal))
			));
#endif

			var summary = BenchmarkRunner.Run (typeof (Program).Assembly, config);
		}
	}
}
