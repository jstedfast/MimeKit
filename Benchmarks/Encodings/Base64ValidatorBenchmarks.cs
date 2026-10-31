//
// Base64ValidatorBenchmarks.cs
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
	public class Base64ValidatorBenchmarks
	{
		const int ChunkSize = 4096;
		readonly CountingLogger logger = new CountingLogger ();
		byte[] input;

		class CountingLogger : IMimeComplianceLogger
		{
			public int Count;

			public void Log (in MimeComplianceIssue issue)
			{
				Count++;
			}
		}

		[Params ("Unwrapped", "MimeCrlf", "MimeLf", "ShortLines", "Garbage", "Small")]
		public string Layout { get; set; }

		[GlobalSetup]
		public void Setup ()
		{
			var data = new byte[Layout == "Small" ? 48 : 65536];
			new Random (12345).NextBytes (data);
			string text = Convert.ToBase64String (data);
			int lineLength;
			string separator;

			switch (Layout) {
			case "MimeCrlf": lineLength = 76; separator = "\r\n"; break;
			case "MimeLf": lineLength = 76; separator = "\n"; break;
			case "ShortLines": lineLength = 7; separator = " \t\r\n"; break;
			case "Garbage": lineLength = 23; separator = "!? \t\r\n"; break;
			default: lineLength = text.Length; separator = string.Empty; break;
			}

			var builder = new StringBuilder ();
			for (int offset = 0; offset < text.Length; offset += lineLength) {
				builder.Append (text, offset, Math.Min (lineLength, text.Length - offset));
				builder.Append (separator);
			}

			input = Encoding.ASCII.GetBytes (builder.ToString ());

			var scalar = new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1) { MaxVectorSize = 0 };
			Validate (scalar);
			int expected = logger.Count;

			if (Validate () != expected)
				throw new InvalidOperationException ("Incorrect compliance issue count.");
		}

		void Validate (Base64Validator validator)
		{
			logger.Count = 0;

			for (int offset = 0; offset < input.Length; offset += ChunkSize)
				validator.Write (input, offset, Math.Min (ChunkSize, input.Length - offset));

			validator.Flush ();
		}

		[Benchmark]
		public int Validate ()
		{
			Validate (new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1));

			return logger.Count;
		}
	}
}
