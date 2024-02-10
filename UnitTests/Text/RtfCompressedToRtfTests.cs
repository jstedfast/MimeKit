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

namespace UnitTests.Text {
	[TestFixture]
	public class RtfCompressedToRtfTests
	{
		[Test]
		public void TestSimpleCompressedRtfExample ()
		{
			// http://msdn.microsoft.com/en-us/library/ee217938(v=exchg.80).aspx
			var compressedRtfData = new byte[] {
				// header
				0x2d, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x00, 0x00, 0x4c, 0x5a, 0x46, 0x75, 0xf1, 0xc5, 0xc7, 0xa7,

				// data
				0x03, 0x00, 0x0a, 0x00, 0x72, 0x63, 0x70, 0x67, 0x31, 0x32, 0x35, 0x42, 0x32, 0x0a, 0xf3, 0x20,
				0x68, 0x65, 0x6c, 0x09, 0x00, 0x20, 0x62, 0x77, 0x05, 0xb0, 0x6c, 0x64, 0x7d, 0x0a, 0x80, 0x0f,
				0xa0
			};
			var expected = "{\\rtf1\\ansi\\ansicpg1252\\pard hello world}\r\n";

			var converter = new RtfCompressedToRtf ();
			int outputLength, outputIndex;

			var decompressed = converter.Flush (compressedRtfData, 0, compressedRtfData.Length, out outputIndex, out outputLength);
			var text = Encoding.UTF8.GetString (decompressed, outputIndex, outputLength);

			Assert.That (text, Is.EqualTo (expected), "Decompressed RTF data does not match.");
			Assert.That (converter.IsValidCrc32, Is.True, "Invalid CRC32 checksum.");
		}

		[Test]
		public void TestCrossingWritePositionExample ()
		{
			// http://msdn.microsoft.com/en-us/library/ee158471(v=exchg.80).aspx
			var compressedRtfData = new byte[] {
				// header
				0x1a, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x00, 0x00, 0x4c, 0x5a, 0x46, 0x75, 0xe2, 0xd4, 0x4b, 0x51,

				// data
				0x41, 0x00, 0x04, 0x20, 0x57, 0x58, 0x59, 0x5a, 0x0d, 0x6e, 0x7d, 0x01, 0x0e, 0xb0
			};
			var expected = "{\\rtf1 WXYZWXYZWXYZWXYZWXYZ}";

			var converter = new RtfCompressedToRtf ();
			int outputLength, outputIndex;

			var decompressed = converter.Flush (compressedRtfData, 0, compressedRtfData.Length, out outputIndex, out outputLength);
			var text = Encoding.UTF8.GetString (decompressed, outputIndex, outputLength);

			Assert.That (text, Is.EqualTo (expected), "Decompressed RTF data does not match.");
			Assert.That (converter.IsValidCrc32, Is.True, "Invalid CRC32 checksum.");
		}
	}
}
