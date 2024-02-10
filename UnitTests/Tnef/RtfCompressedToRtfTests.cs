//
// RtfCompressedToRtfTests.cs
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
			var input = new byte[] { (byte) '.', 0x00, 0x00, 0x00, (byte) '\"', 0x00, 0x00, 0x00, (byte) 'M', (byte) 'E', (byte) 'L', (byte) 'A', (byte) ' ', 0xdf, 0x12, 0xce, (byte) '{', (byte) '\\', (byte) 'r', (byte) 't', (byte) 'f', (byte) '1', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) '2', (byte) '\\', (byte) 'p', (byte) 'a', (byte) 'r', (byte) 'd', (byte) ' ', (byte) 't', (byte) 'e', (byte) 's', (byte) 't', (byte) '}' };
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
			var input = new byte[] { (byte) '.', 0x00, 0x00, 0x00, (byte) '\"', 0x00, 0x00, 0x00, (byte) 'M', (byte) 'E', (byte) 'L', (byte) 'A', (byte) ' ', 0xdf, 0x12, 0xce, (byte) '{', (byte) '\\', (byte) 'r', (byte) 't', (byte) 'f', (byte) '1', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) '\\', (byte) 'a', (byte) 'n', (byte) 's', (byte) 'i', (byte) 'c', (byte) 'p', (byte) 'g', (byte) '1', (byte) '2', (byte) '5', (byte) '2', (byte) '\\', (byte) 'p', (byte) 'a', (byte) 'r', (byte) 'd', (byte) ' ', (byte) 't', (byte) 'e', (byte) 's', (byte) 't', (byte) '}' };
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
	}
}
