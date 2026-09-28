//
// EncodingValidatorBenchmarks.cs
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
using System.IO;
using System.Runtime.CompilerServices;

using MimeKit;
using MimeKit.Encodings;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.Encodings {
	public class EncodingValidatorBenchmarks
	{
		// Note: The validators are fed content by the parser in chunks as it reads them, so write
		// the content in similarly sized chunks in order to exercise the same state transitions.
		const int ChunkSize = 4096;

		readonly byte[] QuotedPrintableData, Base64Data, UUEncodedData;
		readonly NullMimeComplianceLogger logger = new NullMimeComplianceLogger ();

		public EncodingValidatorBenchmarks ()
		{
			var dataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "encoders");

			Base64Data = File.ReadAllBytes (Path.Combine (dataDir, "photo.b64"));
			UUEncodedData = File.ReadAllBytes (Path.Combine (dataDir, "photo.uu"));

			// Note: wikipedia.qp is only a few hundred bytes, which is far too small to be
			// comparable with the base64 and uuencoded content, so quoted-printable encode a
			// repeated copy of the original text in order to get a similarly sized document.
			QuotedPrintableData = GetQuotedPrintableData (Path.Combine (dataDir, "wikipedia.txt"), 512);
		}

		static byte[] GetQuotedPrintableData (string path, int repeat)
		{
			var text = File.ReadAllBytes (path);
			var input = new byte[text.Length * repeat];

			for (int i = 0; i < repeat; i++)
				Buffer.BlockCopy (text, 0, input, i * text.Length, text.Length);

			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[encoder.EstimateOutputLength (input.Length)];
			int n = encoder.Encode (input, 0, input.Length, output);

			n += encoder.Flush (input, input.Length, 0, output);

			Array.Resize (ref output, n);

			return output;
		}

		// Note: A logger that does nothing keeps the benchmarks focused on the cost of scanning
		// the content rather than on the cost of recording any issues that are found within it.
		class NullMimeComplianceLogger : IMimeComplianceLogger
		{
			public void Log (in MimeComplianceIssue issue)
			{
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void Validate (byte[] data, IEncodingValidator validator)
		{
			int index = 0;

			while (index < data.Length) {
				int n = Math.Min (ChunkSize, data.Length - index);

				validator.Write (data, index, n);
				index += n;
			}

			validator.Flush ();
		}

		[Benchmark]
		public void Base64Validate ()
		{
			Validate (Base64Data, new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1));
		}

		[Benchmark]
		public void QuotedPrintableValidate ()
		{
			Validate (QuotedPrintableData, new QuotedPrintableValidator (logger, MimeComplianceContext.Transport, 0, 1));
		}

		[Benchmark]
		public void UUValidate ()
		{
			Validate (UUEncodedData, new UUValidator (logger, MimeComplianceContext.Transport, 0, 1));
		}
	}
}
