//
// RtfCompressedToRtfTests.cs
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

using System.Buffers.Binary;
using System.Text;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class RtfCompressedToRtfTests
	{
		[Test]
		public void TestRtfCompressedToRtfUnknownCompressionType ()
		{
			var input = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', 0xff, 0xff, 0xff, 0xff };
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);

			Assert.That (outputIndex, Is.EqualTo (16), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (0), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.False, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo ((RtfCompressionMode) 1145258561), "ComnpressionMode");
		}

		[Test]
		public void TestRtfCompressedToRtfInvalidCrc ()
		{
			var input = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, (byte) 'L', (byte) 'Z', (byte) 'F', (byte) 'u', 0xff, 0xff, 0xff, 0xff };
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);

			Assert.That (outputIndex, Is.EqualTo (0), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (0), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.False, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Compressed), "ComnpressionMode");
		}

		[Test]
		public void TestRtfCompressedToRtf ()
		{
			var input = new byte[] { (byte) '-', 0x00, 0x00, 0x00, (byte) '+', 0x00, 0x00, 0x00, (byte) 'L', (byte) 'Z', (byte) 'F', (byte) 'u', 0xf1, 0xc5, 0xc7, 0xa7, 0x03, 0x00, (byte) '\n', 0x00, (byte) 'r', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) 'B', (byte) '2', (byte) '\n', 0xf3, (byte) ' ', (byte) 'h', (byte) 'e', (byte) 'l', (byte) '\t', 0x00, (byte) ' ', (byte) 'b', (byte) 'w', 0x05, 0xb0, (byte) 'l', (byte) 'd', (byte) '}', (byte) '\n', 0x80, 0x0f, 0xa0 };
			const string expected = "{\\rtf1\\ansi\\ansicpg1252\\pard hello world}\r\n";
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);

			Assert.That (outputIndex, Is.EqualTo (0), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (43), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.True, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Compressed), "ComnpressionMode");

			var text = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (text, Is.EqualTo (expected));
		}

		[Test]
		public void TestRtfCompressedToRtfByteByByte ()
		{
			var input = new byte[] { (byte) '-', 0x00, 0x00, 0x00, (byte) '+', 0x00, 0x00, 0x00, (byte) 'L', (byte) 'Z', (byte) 'F', (byte) 'u', 0xf1, 0xc5, 0xc7, 0xa7, 0x03, 0x00, (byte) '\n', 0x00, (byte) 'r', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) 'B', (byte) '2', (byte) '\n', 0xf3, (byte) ' ', (byte) 'h', (byte) 'e', (byte) 'l', (byte) '\t', 0x00, (byte) ' ', (byte) 'b', (byte) 'w', 0x05, 0xb0, (byte) 'l', (byte) 'd', (byte) '}', (byte) '\n', 0x80, 0x0f, 0xa0 };
			const string expected = "{\\rtf1\\ansi\\ansicpg1252\\pard hello world}\r\n";
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			using (var memory = new MemoryStream ()) {
				for (int i = 0; i < input.Length; i++) {
					output = filter.Filter (input, i, 1, out outputIndex, out outputLength);
					memory.Write (output, outputIndex, outputLength);
				}

				output = filter.Flush (input, 0, 0, out outputIndex, out outputLength);
				memory.Write (output, outputIndex, outputLength);

				output = memory.ToArray ();
			}

			Assert.That (output.Length, Is.EqualTo (43), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.True, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Compressed), "ComnpressionMode");

			var text = Encoding.ASCII.GetString (output);

			Assert.That (text, Is.EqualTo (expected));
		}

		[Test]
		public void TestRtfCompressedToRtfRaw ()
		{
			var input = new byte[] { (byte) '.', 0x00, 0x00, 0x00, (byte) '\"', 0x00, 0x00, 0x00, (byte) 'M', (byte) 'E', (byte) 'L', (byte) 'A', 0x00, 0x00, 0x00, 0x00, (byte) '{', (byte) '\\', (byte) 'r', (byte) 't', (byte) 'f', (byte) '1', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) '2', (byte) '\\', (byte) 'p', (byte) 'a', (byte) 'r', (byte) 'd', (byte) ' ', (byte) 't', (byte) 'e', (byte) 's', (byte) 't', (byte) '}' };
			const string expected = "{\\rtf1\\ansi\\ansicpg1252\\pard test}";
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);

			Assert.That (outputIndex, Is.EqualTo (16), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (34), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.True, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Uncompressed), "ComnpressionMode");

			var text = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (text, Is.EqualTo (expected));
		}

		[Test]
		public void TestRtfCompressedToRtfRawByteByByte ()
		{
			var input = new byte[] { (byte) '.', 0x00, 0x00, 0x00, (byte) '\"', 0x00, 0x00, 0x00, (byte) 'M', (byte) 'E', (byte) 'L', (byte) 'A', 0x00, 0x00, 0x00, 0x00, (byte) '{', (byte) '\\', (byte) 'r', (byte) 't', (byte) 'f', (byte) '1', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) '2', (byte) '\\', (byte) 'p', (byte) 'a', (byte) 'r', (byte) 'd', (byte) ' ', (byte) 't', (byte) 'e', (byte) 's', (byte) 't', (byte) '}' };
			const string expected = "{\\rtf1\\ansi\\ansicpg1252\\pard test}";
			var filter = new RtfCompressedToRtf ();
			int outputIndex, outputLength;
			byte[] output;

			using (var memory = new MemoryStream ()) {
				for (int i = 0; i < input.Length; i++) {
					output = filter.Filter (input, i, 1, out outputIndex, out outputLength);
					memory.Write (output, outputIndex, outputLength);
				}

				output = filter.Flush (input, 0, 0, out outputIndex, out outputLength);
				memory.Write (output, outputIndex, outputLength);

				output = memory.ToArray ();
			}

			Assert.That (output.Length, Is.EqualTo (34), "outputLength");
			Assert.That (filter.IsValidCrc32, Is.True, "IsValidCrc32");
			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Uncompressed), "ComnpressionMode");

			var text = Encoding.ASCII.GetString (output);

			Assert.That (text, Is.EqualTo (expected));
		}

		static byte[] CompressedRtfHeader (int compressedSize, int uncompressedSize)
		{
			var header = new byte[16];

			BinaryPrimitives.WriteInt32LittleEndian (header.AsSpan (0, 4), compressedSize);
			BinaryPrimitives.WriteInt32LittleEndian (header.AsSpan (4, 4), uncompressedSize);
			header[8] = (byte) 'L'; header[9] = (byte) 'Z'; header[10] = (byte) 'F'; header[11] = (byte) 'u';

			return header;
		}

		static byte[] UncompressedRtf (byte[] rawData, int crc = 0, int? compressedSize = null)
		{
			var buffer = new byte[16 + rawData.Length];

			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (0, 4), compressedSize ?? (rawData.Length + 12));
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (4, 4), rawData.Length);
			buffer[8] = (byte) 'M'; buffer[9] = (byte) 'E'; buffer[10] = (byte) 'L'; buffer[11] = (byte) 'A';
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (12, 4), crc);
			rawData.CopyTo (buffer.AsSpan (16));

			return buffer;
		}

		[Test]
		public void TestUncompressedRtfWithNonZeroCrcIsInvalid ()
		{
			// [MS-OXRTFCP] requires the CRC field of an UNCOMPRESSED stream to be 0.
			var input = UncompressedRtf (Encoding.ASCII.GetBytes ("{\\rtf1 test}"), 0x12345678);
			var filter = new RtfCompressedToRtf ();

			filter.Flush (input, 0, input.Length, out _, out _);

			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Uncompressed), "CompressionMode");
			Assert.That (filter.IsValidCrc32, Is.False, "IsValidCrc32");
		}

		[Test]
		public void TestUndersizedCompressedSizeDoesNotThrow ()
		{
			// COMPSIZE is defined as the length of the CONTENTS field plus 12, so a value smaller than 12
			// is nonsensical and must not drive the bookkeeping negative.
			var input = UncompressedRtf (Encoding.ASCII.GetBytes ("{\\rtf1 test}"), 0, 3);
			var filter = new RtfCompressedToRtf ();

			byte[] output = null;
			int outputIndex = 0, outputLength = 0;

			Assert.DoesNotThrow (() => output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength));

			Assert.That (output, Is.Not.Null, "output");
			Assert.That (outputLength, Is.EqualTo (0), "outputLength");
		}

		static byte[] CompressedRtf (byte[] contents, int? compressedSize = null, int uncompressedSize = 0, int crc = 0)
		{
			var buffer = new byte[16 + contents.Length];

			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (0, 4), compressedSize ?? (contents.Length + 12));
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (4, 4), uncompressedSize);
			buffer[8] = (byte) 'L'; buffer[9] = (byte) 'Z'; buffer[10] = (byte) 'F'; buffer[11] = (byte) 'u';
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (12, 4), crc);
			contents.CopyTo (buffer.AsSpan (16));

			return buffer;
		}

		// The CONTENTS field of the stream used by TestRtfCompressedToRtf (). The final 3 bytes are the
		// control run that terminates the stream.
		static readonly byte[] CompressedRtfContents = {
			0x03, 0x00, (byte) '\n', 0x00, (byte) 'r', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2',
			(byte) '5', (byte) 'B', (byte) '2', (byte) '\n', 0xf3, (byte) ' ', (byte) 'h', (byte) 'e', (byte) 'l',
			(byte) '\t', 0x00, (byte) ' ', (byte) 'b', (byte) 'w', 0x05, 0xb0, (byte) 'l', (byte) 'd', (byte) '}',
			(byte) '\n', 0x80, 0x0f, 0xa0
		};

		[Test]
		public void TestCompressedRtfStopsAtDeclaredCompressedSize ()
		{
			// [MS-OXRTFCP] defines COMPSIZE as the length of the CONTENTS field plus 12, so only the first
			// 2 bytes of CONTENTS belong to this stream and the rest must not be decompressed.
			var input = CompressedRtf (CompressedRtfContents, 12 + 2);
			var filter = new RtfCompressedToRtf ();

			filter.Flush (input, 0, input.Length, out _, out int outputLength);

			Assert.That (filter.CompressionMode, Is.EqualTo (RtfCompressionMode.Compressed), "CompressionMode");
			Assert.That (outputLength, Is.EqualTo (0), "outputLength");
		}

		[Test]
		public void TestCompressedRtfIgnoresDataBeyondCompressedSize ()
		{
			// Drop the control run that terminates the stream so that only COMPSIZE can tell us where the
			// CONTENTS field ends, then append data that is not part of the stream. The decompressed output
			// and the CRC must both be unaffected by it.
			var contents = new byte[CompressedRtfContents.Length - 3];
			Buffer.BlockCopy (CompressedRtfContents, 0, contents, 0, contents.Length);

			var expected = CompressedRtf (contents);
			var trailing = new byte[expected.Length + 64];
			Buffer.BlockCopy (expected, 0, trailing, 0, expected.Length);

			for (int i = expected.Length; i < trailing.Length; i++)
				trailing[i] = (byte) (i & 0xff);

			var filter = new RtfCompressedToRtf ();
			var output = filter.Flush (expected, 0, expected.Length, out int outputIndex, out int outputLength);
			var expectedOutput = new byte[outputLength];
			Buffer.BlockCopy (output, outputIndex, expectedOutput, 0, outputLength);
			bool expectedCrc = filter.IsValidCrc32;

			filter = new RtfCompressedToRtf ();
			output = filter.Flush (trailing, 0, trailing.Length, out outputIndex, out outputLength);
			var actualOutput = new byte[outputLength];
			Buffer.BlockCopy (output, outputIndex, actualOutput, 0, outputLength);

			Assert.That (actualOutput, Is.EqualTo (expectedOutput), "output");
			Assert.That (filter.IsValidCrc32, Is.EqualTo (expectedCrc), "IsValidCrc32");
		}

		[Test]
		public void TestRtfCompressedToRtfOverflowingUncompressedSize ()
		{
			// The COMPSIZE and RAWSIZE header fields are untrusted; the difference between them must not
			// be allowed to overflow when estimating the size of the output buffer.
			var input = CompressedRtfHeader (12, int.MinValue);
			var filter = new RtfCompressedToRtf ();

			Assert.DoesNotThrow (() => filter.Flush (input, 0, input.Length, out _, out _));
		}

		[Test]
		public void TestRtfCompressedToRtfHugeUncompressedSize ()
		{
			// A RAWSIZE of 1GB must not cause us to allocate a 1GB output buffer up front.
			var input = CompressedRtfHeader (12, 0x40000000);
			var filter = new RtfCompressedToRtf ();

			long before = GC.GetAllocatedBytesForCurrentThread ();

			Assert.DoesNotThrow (() => filter.Flush (input, 0, input.Length, out _, out _));

			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;

			Assert.That (allocated, Is.LessThan (16 * 1024 * 1024), "allocated");
		}
	}
}
