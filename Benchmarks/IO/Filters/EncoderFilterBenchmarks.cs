//
// EncoderFilterBenchmarks.cs
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
using System.Buffers.Text;
using System.Runtime.CompilerServices;

using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

using BenchmarkDotNet.Attributes;

namespace Benchmarks.IO.Filters {
	public class EncoderFilterBenchmarks
	{
		readonly byte[] BinaryData, TextData;

		public EncoderFilterBenchmarks ()
		{
			var dataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "encoders");
			var path = Path.Combine (dataDir, "encoders", "wikipedia.txt");
			TextData = File.ReadAllBytes (path);

			path = Path.Combine (dataDir, "encoders", "photo.jpg");
			BinaryData = File.ReadAllBytes (path);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void EncodeStream (byte[] data, IMimeEncoder encoder)
		{
			using var input = new MemoryStream (data, false);
			using var output = new MeasuringStream ();
			using var filtered = new FilteredStream (output);

			filtered.Add (new EncoderFilter (encoder));
			input.CopyTo (filtered);
			filtered.Flush ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void Encode (byte[] data, IMimeEncoder encoder)
		{
			var maxLength = encoder.EstimateOutputLength (data.Length);
			var output = new byte[maxLength];

			encoder.Flush (data, 0, data.Length, output);
		}

		[Benchmark]
		public void HwAccelBase64EncodeStream ()
		{
			EncodeStream (BinaryData, new Base64Encoder ());
		}

		[Benchmark]
		public void Base64EncodeStream ()
		{
			EncodeStream (BinaryData, new Base64Encoder () { EnableHardwareAcceleration = false });
		}

		[Benchmark]
		public void HwAccelBase64Encode ()
		{
			Encode (BinaryData, new Base64Encoder ());
		}

		[Benchmark]
		public void Base64Encode ()
		{
			Encode (BinaryData, new Base64Encoder () { EnableHardwareAcceleration = false });
		}

		[Benchmark]
		public void Base64EncodeToUtf8 ()
		{
			// Note: This benchmark serves as a baseline for optimal performance of a base64 encoder.
			var maxLength = Base64.GetMaxEncodedToUtf8Length (BinaryData.Length);
			var output = new byte[maxLength];

			Base64.EncodeToUtf8 (BinaryData, output, out _, out _, true);
		}

		[Benchmark]
		public void HexEncodeStream ()
		{
			EncodeStream (BinaryData, new HexEncoder ());
		}

		[Benchmark]
		public void QEncodeStream ()
		{
			EncodeStream (TextData, new QEncoder (QEncodeMode.Text));
		}

		[Benchmark]
		public void QuotedPrintableEncodeStream ()
		{
			EncodeStream (TextData, new QuotedPrintableEncoder ());
		}

		[Benchmark]
		public void UUEncodeStream ()
		{
			EncodeStream (BinaryData, new UUEncoder ());
		}
	}
}
