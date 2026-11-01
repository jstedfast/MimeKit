//
// MimeComplianceValidatorBenchmarks.cs
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
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit;

namespace Benchmarks {
	[MemoryDiagnoser]
	public class MimeComplianceValidatorBenchmarks
	{
		sealed class CountingLogger : IMimeComplianceLogger
		{
			public int Count;

			public void Log (in MimeComplianceIssue issue)
			{
				Count++;
			}
		}

		readonly CountingLogger logger = new CountingLogger ();
		EncodedWordContext context;
		HeaderId header;
		byte[] data;
		bool parameters;

		[Params ("PlainSubject", "LongPlainSubject", "ShortQ", "ShortB", "FoldedQ", "FoldedB",
			"PlainDisplayName", "EncodedDisplayNames", "Comments", "QuotedWord", "MalformedQ",
			"MalformedB", "Unterminated", "SparseWords", "ParameterNone", "ParameterCharset",
			"ParameterFilename", "ParameterPair", "ParameterContinuation", "ParameterRepeated",
			"ParameterManyNames", "ParameterManySegments")]
		public string Scenario { get; set; }

		[GlobalSetup]
		public void Setup ()
		{
			context = EncodedWordContext.Text;
			header = HeaderId.ContentType;
			parameters = Scenario.StartsWith ("Parameter", StringComparison.Ordinal);
			string text;

			switch (Scenario) {
			case "PlainSubject":
				text = " Monthly report\r\n";
				break;
			case "LongPlainSubject":
				text = string.Concat (Enumerable.Repeat (" Ordinary folded header text without encoded words.\r\n", 80));
				break;
			case "ShortQ":
				text = " =?utf-8?Q?caf=C3=A9?=\r\n";
				break;
			case "ShortB":
				text = " =?utf-8?B?Y2Fmw6k=?=\r\n";
				break;
			case "FoldedQ":
				text = string.Concat (Enumerable.Repeat (" =?utf-8?Q?" + new string ('a', 50) + "=20?=\r\n", 64));
				break;
			case "FoldedB":
				text = string.Concat (Enumerable.Repeat (" =?utf-8?B?" + Convert.ToBase64String (new byte[42]) + "?=\r\n", 64));
				break;
			case "PlainDisplayName":
				context = EncodedWordContext.Phrase;
				text = " Alice Example <alice@example.com>\r\n";
				break;
			case "EncodedDisplayNames":
				context = EncodedWordContext.Phrase;
				text = string.Join (",\r\n ", Enumerable.Repeat ("=?utf-8?Q?Alice_Example?= <alice@example.com>", 20));
				break;
			case "Comments":
				context = EncodedWordContext.Phrase;
				text = " (outer (=?utf-8?Q?caf=C3=A9?=)) Alice <alice@example.com>\r\n";
				break;
			case "QuotedWord":
				context = EncodedWordContext.Phrase;
				text = " \"=?utf-8?Q?Alice_Example?=\" <alice@example.com>\r\n";
				break;
			case "MalformedQ":
				text = " =?utf-8?Q?bad=GG?=\r\n";
				break;
			case "MalformedB":
				text = " =?utf-8?B?YQ?=\r\n";
				break;
			case "Unterminated":
				text = string.Concat (Enumerable.Repeat ("=?utf-8?Q?unfinished", 128));
				break;
			case "SparseWords":
				text = new string ('a', 2048) + "\r\n =?utf-8?Q?valid?=\r\n " +
					new string ('a', 2048) + "\r\n =?utf-8?Q?bad=GG?=\r\n";
				break;
			case "ParameterNone":
				text = " text/plain\r\n";
				break;
			case "ParameterCharset":
				text = " text/plain; charset=utf-8\r\n";
				break;
			case "ParameterFilename":
				header = HeaderId.ContentDisposition;
				text = " attachment; filename=\"report.pdf\"\r\n";
				break;
			case "ParameterPair":
				text = " text/plain; charset=utf-8; format=flowed\r\n";
				break;
			case "ParameterContinuation":
				header = HeaderId.ContentDisposition;
				text = " attachment; filename*0*=utf-8''monthly%20;\r\n filename*1*=report.pdf\r\n";
				break;
			case "ParameterRepeated":
				text = " multipart/mixed; boundary=one; boundary=two\r\n";
				break;
			case "ParameterManyNames":
				text = " text/plain" + string.Concat (Enumerable.Range (0, 100).Select (i => $"; x{i}=value")) + "\r\n";
				break;
			case "ParameterManySegments":
				header = HeaderId.ContentDisposition;
				text = " attachment; filename*0*=utf-8''a" +
					string.Concat (Enumerable.Range (1, 100).Select (i => $"; filename*{i}*=a")) + "\r\n";
				break;
			default:
				throw new InvalidOperationException ("Unknown validator scenario.");
			}

			data = Encoding.ASCII.GetBytes (text);
		}

		[Benchmark]
		public int Validate ()
		{
			logger.Count = 0;

			if (parameters) {
				var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, header, 0, 1, 1);
				validator.Validate (data, 0, data.Length);
			} else {
				var validator = new EncodedWordValidator (logger, MimeComplianceContext.Transport, data, 0, data.Length, 0, 1, 1);
				validator.Validate (data.Length, context);
			}

			return logger.Count;
		}
	}
}
