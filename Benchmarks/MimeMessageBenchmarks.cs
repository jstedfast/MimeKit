//
// MimeMessageBenchmarks.cs
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
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit;

namespace Benchmarks {
	public class MimeMessageBenchmarks
	{
		static readonly MimeMessage UnpreparedMessage;

		static MimeMessageBenchmarks ()
		{
			UnpreparedMessage = new MimeMessage ();

			UnpreparedMessage.Body = new Multipart ("mixed") {
				new TextPart ("plain") {
					Text = File.ReadAllText (Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "text", "lorem-ipsum.txt"))
				},
				new MimePart ("application", "octet-stream") {
					Content = new MimeContent (File.OpenRead (Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "images", "girl.jpg")))
				}
			};
		}

		static void MimeMessage_Prepare (EncodingConstraint constraint)
		{
			foreach (var bodyPart in UnpreparedMessage.BodyParts.OfType<MimePart> ())
				bodyPart.ContentTransferEncoding = ContentEncoding.Default;

			UnpreparedMessage.Prepare (constraint);
		}

		[Benchmark]
		public void MimeMessage_Prepare_EncodingConstraint_None ()
		{
			MimeMessage_Prepare (EncodingConstraint.None);
		}

		[Benchmark]
		public void MimeMessage_Prepare_EncodingConstraint_SevenBit ()
		{
			MimeMessage_Prepare (EncodingConstraint.SevenBit);
		}

		[Benchmark]
		public void MimeMessage_Prepare_EncodingConstraint_EightBit ()
		{
			MimeMessage_Prepare (EncodingConstraint.EightBit);
		}
	}
}
