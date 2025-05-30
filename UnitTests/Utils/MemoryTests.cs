//
// MemoryTests.cs
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
using System.Buffers;

#if NETCOREAPP
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class MemoryTests
	{
		delegate int IndexOfDelegate (byte[] buffer, int offset, int length, byte value, DetectionOptions options, out DetectionResults detected);

		static readonly byte[] EolnWithNullAnd8Bit = new byte[] { 0x00, 0xFF, (byte) '\r', (byte) '\n' };
		static readonly byte[] EolnWithNull = new byte[] { 0x00, (byte) '\r', (byte) '\n' };
		static readonly byte[] EolnWith8Bit = new byte[] { 0xFF, (byte) '\r', (byte) '\n' };
		static readonly byte[] Eoln = new byte[] { (byte) '\r', (byte) '\n' };

		const int BufferLength = 1000;

		[TestCase ((int) DetectionOptions.None, TestName = "TestIndexOfOptions_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestIndexOfOptions_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestIndexOfOptions_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestIndexOfOptions_DetectBinary")]
		public void TestIndexOfOptions (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (Eoln, options, IndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestIndexOfOptionsWithNull_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestIndexOfOptionsWithNull_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestIndexOfOptionsWithNull_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestIndexOfOptionsWithNull_DetectBinary")]
		public void TestIndexOfOptionsWithNull (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWithNull, options, IndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestIndexOfOptionsWith8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestIndexOfOptionsWith8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestIndexOfOptionsWith8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestIndexOfOptionsWith8Bit_DetectBinary")]
		public void TestIndexOfOptionsWith8Bit (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWith8Bit, options, IndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestIndexOfOptionsWithNullAnd8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestIndexOfOptionsWithNullAnd8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestIndexOfOptionsWithNullAnd8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestIndexOfOptionsWithNullAnd8Bit_DetectBinary")]
		public void TestIndexOfOptionsWithNullAnd8Bit (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWithNullAnd8Bit, options, IndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestIndexOfOptionsUnaligned (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (Eoln, alignmentOffset, IndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestIndexOfOptionsUnalignedWithNull (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWithNull, alignmentOffset, IndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestIndexOfOptionsUnalignedWith8Bit (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWith8Bit, alignmentOffset, IndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestIndexOfOptionsUnalignedWithNullAnd8Bit (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWithNullAnd8Bit, alignmentOffset, IndexOf);
		}

		[Test]
		public unsafe void TestIndexOfOptions_NotFound ()
		{
			var options = DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls;
			var buffer = ArrayPool<byte>.Shared.Rent (BufferLength);

			buffer.AsSpan ().Fill ((byte) 'A');

			try {
				fixed (byte* buf = buffer) {
					// Note: Assume AVX2 is supported. The SSE2 and Vector test cases will cover the other scenarios.
					nint alignment = ((nint) buf) & (Vector256<byte>.Count - 1);
					int alignmentOffset = Vector256<byte>.Count - (int) alignment;
					byte* searchSpace = buf + alignmentOffset;
					DetectionResults results;
					int index;

					index = Memory.IndexOf (searchSpace, Vector256<byte>.Count * 2, (byte) '\n', options, out results);
					Assert.That (index, Is.EqualTo (-1), "index");
					Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "results");

					// Now test with a length that will cause PostSequentialScan() to return -1
					index = Memory.IndexOf (searchSpace, Vector256<byte>.Count * 2 + 3, (byte) '\n', options, out results);
					Assert.That (index, Is.EqualTo (-1), "PostSequentialScan index");
					Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "PostSequentialScan results");
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

#if NETCOREAPP
		[TestCase ((int) DetectionOptions.None, TestName = "TestSwAccelIndexOfOptions_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSwAccelIndexOfOptions_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSwAccelIndexOfOptions_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSwAccelIndexOfOptions_DetectBinary")]
		public void TestSwAccelIndexOfOptions (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (Eoln, options, SwAccelIndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSwAccelIndexOfOptionsWithNull_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSwAccelIndexOfOptionsWithNull_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSwAccelIndexOfOptionsWithNull_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSwAccelIndexOfOptionsWithNull_DetectBinary")]
		public void TestSwAccelIndexOfOptionsWithNull (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWithNull, options, SwAccelIndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSwAccelIndexOfOptionsWith8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSwAccelIndexOfOptionsWith8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSwAccelIndexOfOptionsWith8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSwAccelIndexOfOptionsWith8Bit_DetectBinary")]
		public void TestSwAccelIndexOfOptionsWith8Bit (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWith8Bit, options, SwAccelIndexOf);
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSwAccelIndexOfOptionsWithNullAnd8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSwAccelIndexOfOptionsWithNullAnd8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSwAccelIndexOfOptionsWithNullAnd8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSwAccelIndexOfOptionsWithNullAnd8Bit_DetectBinary")]
		public void TestSwAccelIndexOfOptionsWithNullAnd8Bit (int opts)
		{
			DetectionOptions options = (DetectionOptions) opts;

			VerifyIndexOfOptions (EolnWithNullAnd8Bit, options, SwAccelIndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSwAccelIndexOfOptionsUnaligned (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (Eoln, alignmentOffset, SwAccelIndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSwAccelIndexOfOptionsUnalignedWithNull (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWithNull, alignmentOffset, SwAccelIndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSwAccelIndexOfOptionsUnalignedWith8Bit (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWith8Bit, alignmentOffset, SwAccelIndexOf);
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSwAccelIndexOfOptionsUnalignedWithNullAnd8Bit (int alignmentOffset)
		{
			VerifyIndexOfOptionsUnaligned (EolnWithNullAnd8Bit, alignmentOffset, SwAccelIndexOf);
		}

		[Test]
		public unsafe void TestSwAccelIndexOfOptions_NotFound ()
		{
			var options = DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls;
			var buffer = ArrayPool<byte>.Shared.Rent (BufferLength);

			buffer.AsSpan ().Fill ((byte) 'A');

			try {
				fixed (byte* buf = buffer) {
					nint alignment = ((nint) buf) & (4 - 1);
					int alignmentOffset = 4 - (int) alignment;
					byte* searchSpace = buf + alignmentOffset;
					DetectionResults results;
					int index;

					index = Memory.SwAccelIndexOf (searchSpace, 9, (byte) '\n', options, out results);
					Assert.That (index, Is.EqualTo (-1), "index");
					Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "results");
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSse2IndexOfOptions_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSse2IndexOfOptions_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSse2IndexOfOptions_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSse2IndexOfOptions_DetectBinary")]
		public void TestSse2IndexOfOptions (int opts)
		{
			if (Sse2.IsSupported) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (Eoln, options, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSse2IndexOfOptionsWithNull_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSse2IndexOfOptionsWithNull_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSse2IndexOfOptionsWithNull_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSse2IndexOfOptionsWithNull_DetectBinary")]
		public void TestSse2IndexOfOptionsWithNull (int opts)
		{
			if (Sse2.IsSupported) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWithNull, options, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSse2IndexOfOptionsWith8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSse2IndexOfOptionsWith8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSse2IndexOfOptionsWith8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSse2IndexOfOptionsWith8Bit_DetectBinary")]
		public void TestSse2IndexOfOptionsWith8Bit (int opts)
		{
			if (Sse2.IsSupported) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWith8Bit, options, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestSse2IndexOfOptionsWithNullAnd8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestSse2IndexOfOptionsWithNullAnd8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestSse2IndexOfOptionsWithNullAnd8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestSse2IndexOfOptionsWithNullAnd8Bit_DetectBinary")]
		public void TestSse2IndexOfOptionsWithNullAnd8Bit (int opts)
		{
			if (Sse2.IsSupported) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWithNullAnd8Bit, options, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSse2IndexOfOptionsUnaligned (int alignmentOffset)
		{
			if (Sse2.IsSupported) {
				VerifyIndexOfOptionsUnaligned (Eoln, alignmentOffset, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSse2IndexOfOptionsUnalignedWithNull (int alignmentOffset)
		{
			if (Sse2.IsSupported) {
				VerifyIndexOfOptionsUnaligned (EolnWithNull, alignmentOffset, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSse2IndexOfOptionsUnalignedWith8Bit (int alignmentOffset)
		{
			if (Sse2.IsSupported) {
				VerifyIndexOfOptionsUnaligned (EolnWith8Bit, alignmentOffset, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestSse2IndexOfOptionsUnalignedWithNullAnd8Bit (int alignmentOffset)
		{
			if (Sse2.IsSupported) {
				VerifyIndexOfOptionsUnaligned (EolnWithNullAnd8Bit, alignmentOffset, Sse2IndexOf);
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[Test]
		public unsafe void TestSse2IndexOfOptions_NotFound ()
		{
			if (Sse2.IsSupported) {
				var options = DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls;
				var buffer = ArrayPool<byte>.Shared.Rent (BufferLength);

				buffer.AsSpan ().Fill ((byte) 'A');

				try {
					fixed (byte* buf = buffer) {
						nint alignment = ((nint) buf) & (Vector128<byte>.Count - 1);
						int alignmentOffset = Vector128<byte>.Count - (int) alignment;
						byte* searchSpace = buf + alignmentOffset;
						DetectionResults results;
						int index;

						index = Memory.Sse2IndexOf (searchSpace, Vector128<byte>.Count * 2, (byte) '\n', options, out results);
						Assert.That (index, Is.EqualTo (-1), "index");
						Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "results");

						// Now test with a length that will cause PostSequentialScan() to return -1
						index = Memory.Sse2IndexOf (searchSpace, Vector128<byte>.Count * 2 + 3, (byte) '\n', options, out results);
						Assert.That (index, Is.EqualTo (-1), "PostSequentialScan index");
						Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "PostSequentialScan results");
					}
				} finally {
					ArrayPool<byte>.Shared.Return (buffer);
				}
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestVectorIndexOfOptions_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestVectorIndexOfOptions_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestVectorIndexOfOptions_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestVectorIndexOfOptions_DetectBinary")]
		public void TestVectorIndexOfOptions (int opts)
		{
			if (Vector.IsHardwareAccelerated) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (Eoln, options, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestVectorIndexOfOptionsWithNull_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestVectorIndexOfOptionsWithNull_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestVectorIndexOfOptionsWithNull_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestVectorIndexOfOptionsWithNull_DetectBinary")]
		public void TestVectorIndexOfOptionsWithNull (int opts)
		{
			if (Vector.IsHardwareAccelerated) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWithNull, options, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestVectorIndexOfOptionsWith8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestVectorIndexOfOptionsWith8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestVectorIndexOfOptionsWith8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestVectorIndexOfOptionsWith8Bit_DetectBinary")]
		public void TestVectorIndexOfOptionsWith8Bit (int opts)
		{
			if (Vector.IsHardwareAccelerated) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWith8Bit, options, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase ((int) DetectionOptions.None, TestName = "TestVectorIndexOfOptionsWithNullAnd8Bit_None")]
		[TestCase ((int) DetectionOptions.Detect8Bit, TestName = "TestVectorIndexOfOptionsWithNullAnd8Bit_Detect8Bit")]
		[TestCase ((int) DetectionOptions.DetectNulls, TestName = "TestVectorIndexOfOptionsWithNullAnd8Bit_DetectNulls")]
		[TestCase ((int) (DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls), TestName = "TestVectorIndexOfOptionsWithNullAnd8Bit_DetectBinary")]
		public void TestVectorIndexOfOptionsWithNullAnd8Bit (int opts)
		{
			if (Vector.IsHardwareAccelerated) {
				DetectionOptions options = (DetectionOptions) opts;

				VerifyIndexOfOptions (EolnWithNullAnd8Bit, options, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestVectorIndexOfOptionsUnaligned (int alignmentOffset)
		{
			if (Vector.IsHardwareAccelerated) {
				VerifyIndexOfOptionsUnaligned (Eoln, alignmentOffset, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestVectorIndexOfOptionsUnalignedWithNull (int alignmentOffset)
		{
			if (Vector.IsHardwareAccelerated) {
				VerifyIndexOfOptionsUnaligned (EolnWithNull, alignmentOffset, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestVectorIndexOfOptionsUnalignedWith8Bit (int alignmentOffset)
		{
			if (Vector.IsHardwareAccelerated) {
				VerifyIndexOfOptionsUnaligned (EolnWith8Bit, alignmentOffset, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (5)]
		[TestCase (6)]
		[TestCase (7)]
		[TestCase (8)]
		[TestCase (9)]
		[TestCase (10)]
		[TestCase (11)]
		[TestCase (12)]
		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestVectorIndexOfOptionsUnalignedWithNullAnd8Bit (int alignmentOffset)
		{
			if (Vector.IsHardwareAccelerated) {
				VerifyIndexOfOptionsUnaligned (EolnWithNullAnd8Bit, alignmentOffset, VectorIndexOf);
			} else {
				Assert.Ignore ("Vector acceleration is not supported on this platform.");
			}
		}

		[Test]
		public unsafe void TestVectorIndexOfOptions_NotFound ()
		{
			if (Sse2.IsSupported) {
				var options = DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls;
				var buffer = ArrayPool<byte>.Shared.Rent (BufferLength);

				buffer.AsSpan ().Fill ((byte) 'A');

				try {
					fixed (byte* buf = buffer) {
						nint alignment = ((nint) buf) & (Vector<byte>.Count - 1);
						int alignmentOffset = Vector<byte>.Count - (int) alignment;
						byte* searchSpace = buf + alignmentOffset;
						DetectionResults results;
						int index;

						index = Memory.VectorIndexOf (searchSpace, Vector<byte>.Count * 2, (byte) '\n', options, out results);
						Assert.That (index, Is.EqualTo (-1), "index");
						Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "results");

						// Now test with a length that will cause PostSequentialScan() to return -1
						index = Memory.VectorIndexOf (searchSpace, Vector<byte>.Count * 2 + 3, (byte) '\n', options, out results);
						Assert.That (index, Is.EqualTo (-1), "PostSequentialScan index");
						Assert.That (results, Is.EqualTo (DetectionResults.Nothing), "PostSequentialScan results");
					}
				} finally {
					ArrayPool<byte>.Shared.Return (buffer);
				}
			} else {
				Assert.Ignore ("SSE2 is not supported on this platform.");
			}
		}

		[Test]
		public unsafe void TestAlignmentLogicInPreSequentialScan ()
		{
			// Note: This buffer is specifically designed to only have data in the first 3 bytes and 0's after (that we will detect if we go past the end of bounds)
			byte[] buffer = new byte[] { (byte) 'A', (byte) 'B', (byte) 'C', 0, 0, 0, 0, 0 };
			DetectionResults detected;

			fixed (byte *searchSpace = buffer) {
				// We index into our buffer at offset 1 ('B') so that alignment = 1 and we specify the length as 2 bytes ('B' and 'C').
				// If we detect any null bytes, then it means that the IndexOf() code scanned past the end of the buffer.
				int index = Memory.IndexOf (searchSpace + 1, 2, (byte) '\n', DetectionOptions.DetectNulls, out detected);

				Assert.That (index, Is.EqualTo (-1));
				Assert.That (detected, Is.EqualTo (DetectionResults.Nothing));
			}
		}

		static unsafe int Sse2IndexOf (byte[] buffer, int offset, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			fixed (byte* searchSpace = buffer) {
				DetectionResults mask = (DetectionResults) ((int) options);

				int index = Memory.Sse2IndexOf (searchSpace + offset, length, value, options, out detected);

				detected &= mask;

				if (index >= 0)
					index += offset;

				return index;
			}
		}

		static unsafe int VectorIndexOf (byte[] buffer, int offset, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			fixed (byte* searchSpace = buffer) {
				DetectionResults mask = (DetectionResults) ((int) options);

				int index = Memory.VectorIndexOf (searchSpace + offset, length, value, options, out detected);

				detected &= mask;

				if (index >= 0)
					index += offset;

				return index;
			}
		}
#endif

		static unsafe int SwAccelIndexOf (byte[] buffer, int offset, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			fixed (byte* searchSpace = buffer) {
				DetectionResults mask = (DetectionResults) ((int) options);

				int index = Memory.SwAccelIndexOf (searchSpace + offset, length, value, options, out detected);

				detected &= mask;

				if (index >= 0)
					index += offset;

				return index;
			}
		}

		static unsafe int IndexOf (byte[] buffer, int offset, int length, byte value, DetectionOptions options, out DetectionResults detected)
		{
			fixed (byte* searchSpace = buffer) {
				DetectionResults mask = (DetectionResults) ((int) options);

				int index = Memory.IndexOf (searchSpace + offset, length, value, options, out detected);

				detected &= mask;

				if (index >= 0)
					index += offset;

				return index;
			}
		}

		static void VerifyIndexOfOptions (byte[] eoln, DetectionOptions options, IndexOfDelegate indexOf)
		{
			var buffer = ArrayPool<byte>.Shared.Rent (BufferLength);
			DetectionResults expectedResults;

			buffer.AsSpan ().Fill ((byte) 'A');

			try {
				for (int expectedIndex = 0; expectedIndex < 1000; expectedIndex++) {
					Span<byte> region;

					if (expectedIndex >= eoln.Length) {
						region = buffer.AsSpan ((expectedIndex - eoln.Length) + 1, eoln.Length);
						eoln.CopyTo (region);

						expectedResults = DetectionResults.Nothing;
						for (int i = 0; i < eoln.Length; i++) {
							if (eoln[i] > 127 && (options & DetectionOptions.Detect8Bit) != 0) {
								expectedResults |= DetectionResults.Detected8Bit;
							} else if (eoln[i] == 0 && (options & DetectionOptions.DetectNulls) != 0) {
								expectedResults |= DetectionResults.DetectedNulls;
							}
						}
					} else {
						int eolnOffset = eoln.Length - (expectedIndex + 1);

						region = buffer.AsSpan (0, expectedIndex + 1);
						eoln.AsSpan (eolnOffset).CopyTo (region);

						expectedResults = DetectionResults.Nothing;
						for (int i = 0; i < region.Length; i++) {
							if (region[i] > 127 && (options & DetectionOptions.Detect8Bit) != 0) {
								expectedResults |= DetectionResults.Detected8Bit;
							} else if (region[i] == 0 && (options & DetectionOptions.DetectNulls) != 0) {
								expectedResults |= DetectionResults.DetectedNulls;
							}
						}
					}

					int index = indexOf (buffer, 0, BufferLength, (byte) '\n', options, out var results);
					Assert.That (index, Is.EqualTo (expectedIndex), "index");
					Assert.That (results, Is.EqualTo (expectedResults), $"results for index={index}");

					region.Fill ((byte) 'A');
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		static void VerifyIndexOfOptionsUnaligned (byte[] eoln, int alignmentOffset, IndexOfDelegate indexOf)
		{
			var buffer = ArrayPool<byte>.Shared.Rent (alignmentOffset + BufferLength);
			var options = DetectionOptions.Detect8Bit | DetectionOptions.DetectNulls;
			DetectionResults expectedResults;

			buffer.AsSpan ().Fill ((byte) 'A');

			try {
				// Test every possible index for the eoln (we use the SMTP max line length as our stopping point)
				for (int expectedIndex = 0; expectedIndex < 1000; expectedIndex++) {
					Span<byte> region;

					if (expectedIndex >= eoln.Length) {
						region = buffer.AsSpan (alignmentOffset + (expectedIndex - eoln.Length) + 1, eoln.Length);
						eoln.CopyTo (region);

						expectedResults = DetectionResults.Nothing;
						for (int i = 0; i < eoln.Length; i++) {
							if (eoln[i] > 127 && (options & DetectionOptions.Detect8Bit) != 0) {
								expectedResults |= DetectionResults.Detected8Bit;
							} else if (eoln[i] == 0 && (options & DetectionOptions.DetectNulls) != 0) {
								expectedResults |= DetectionResults.DetectedNulls;
							}
						}
					} else {
						int eolnOffset = eoln.Length - (expectedIndex + 1);

						region = buffer.AsSpan (alignmentOffset, expectedIndex + 1);
						eoln.AsSpan (eolnOffset).CopyTo (region);

						expectedResults = DetectionResults.Nothing;
						for (int i = 0; i < region.Length; i++) {
							if (region[i] > 127 && (options & DetectionOptions.Detect8Bit) != 0) {
								expectedResults |= DetectionResults.Detected8Bit;
							} else if (region[i] == 0 && (options & DetectionOptions.DetectNulls) != 0) {
								expectedResults |= DetectionResults.DetectedNulls;
							}
						}
					}

					int index = indexOf (buffer, alignmentOffset, BufferLength, (byte) '\n', options, out var results);
					Assert.That (index, Is.EqualTo (alignmentOffset + expectedIndex), $"index for alignmentOffset={alignmentOffset}");
					Assert.That (results, Is.EqualTo (expectedResults), $"results for index={index} (alignmentOffset={alignmentOffset})");

					region.Fill ((byte) 'A');
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}
	}
}
