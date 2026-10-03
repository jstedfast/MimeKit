//
// RtfCompressedWriteStreamTests.cs
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

using System.Text;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class RtfCompressedWriteStreamTests
	{
		static byte[] Compress (byte[] rtf, bool compress, int chunkSize = int.MaxValue)
		{
			using var output = new MemoryStream ();
			string error = "not closed";

			using (var stream = new RtfCompressedWriteStream (output, compress, (s, e) => error = e)) {
				for (int index = 0; index < rtf.Length; index += chunkSize)
					stream.Write (rtf, index, Math.Min (chunkSize, rtf.Length - index));
			}

			Assert.That (error, Is.Null, "closed callback error");

			return output.ToArray ();
		}

		static byte[] Decompress (byte[] compressed, RtfCompressionMode expectedMode)
		{
			var filter = new RtfCompressedToRtf ();
			var output = filter.Flush (compressed, 0, compressed.Length, out int outputIndex, out int outputLength);

			Assert.That (filter.CompressionMode, Is.EqualTo (expectedMode), "CompressionMode");
			Assert.That (filter.IsValidCrc32, Is.True, "IsValidCrc32");

			return output.AsSpan (outputIndex, outputLength).ToArray ();
		}

		static void AssertRoundTrip (byte[] rtf, bool compress, int chunkSize = int.MaxValue)
		{
			var compressed = Compress (rtf, compress, chunkSize);
			var mode = compress ? RtfCompressionMode.Compressed : RtfCompressionMode.Uncompressed;

			Assert.That (BitConverter.ToInt32 (compressed, 0), Is.EqualTo (compressed.Length - 4), "COMPSIZE");
			Assert.That (BitConverter.ToInt32 (compressed, 4), Is.EqualTo (rtf.Length), "RAWSIZE");

			var decompressed = Decompress (compressed, mode);

			Assert.That (decompressed, Is.EqualTo (rtf));
		}

		// [MS-OXRTFCP] section 4.1: Compressing Plain Text Without Repetition.
		[Test]
		public void TestSpecExampleWithoutRepetition ()
		{
			var rtf = Encoding.ASCII.GetBytes ("{\\rtf1\\ansi\\ansicpg1252\\pard hello world}\r\n");
			var expected = new byte[] { 0x2d, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x00, 0x00, 0x4c, 0x5a, 0x46, 0x75, 0xf1, 0xc5, 0xc7, 0xa7, 0x03, 0x00, 0x0a, 0x00, 0x72, 0x63, 0x70, 0x67, 0x31, 0x32, 0x35, 0x42, 0x32, 0x0a, 0xf3, 0x20, 0x68, 0x65, 0x6c, 0x09, 0x00, 0x20, 0x62, 0x77, 0x05, 0xb0, 0x6c, 0x64, 0x7d, 0x0a, 0x80, 0x0f, 0xa0 };

			Assert.That (Compress (rtf, true), Is.EqualTo (expected));
		}

		// [MS-OXRTFCP] section 4.2: Compressing Plain Text With Repetition (the match overlaps the write position).
		[Test]
		public void TestSpecExampleWithRepetition ()
		{
			var rtf = Encoding.ASCII.GetBytes ("{\\rtf1 WXYZWXYZWXYZWXYZWXYZ}");

			AssertRoundTrip (rtf, true);

			var compressed = Compress (rtf, true);

			// Note: The overlapping match means that the output is much smaller than the input.
			Assert.That (compressed.Length - 16, Is.LessThan (rtf.Length - 8));
		}

		// [MS-OXRTFCP] section 4.3: Uncompressed RTF.
		[Test]
		public void TestSpecExampleUncompressed ()
		{
			var rtf = Encoding.ASCII.GetBytes ("{\\rtf1\\ansi\\ansicpg1252\\pard test}");
			var compressed = Compress (rtf, false);

			Assert.That (compressed.Length, Is.EqualTo (16 + rtf.Length));
			Assert.That (Encoding.ASCII.GetString (compressed, 8, 4), Is.EqualTo ("MELA"));
			Assert.That (BitConverter.ToInt32 (compressed, 12), Is.EqualTo (0), "CRC");
			AssertRoundTrip (rtf, false);
		}

		[Test]
		public void TestEmpty ()
		{
			AssertRoundTrip (Array.Empty<byte> (), true);
			AssertRoundTrip (Array.Empty<byte> (), false);
		}

		[TestCase (1)]
		[TestCase (7)]
		[TestCase (4096)]
		[TestCase (int.MaxValue)]
		public void TestLargeRepetitiveInput (int chunkSize)
		{
			var builder = new StringBuilder ("{\\rtf1\\ansi\\ansicpg1252\\deff0{\\fonttbl{\\f0\\fswiss Arial;}}\r\n");

			for (int i = 0; i < 2000; i++)
				builder.AppendFormat ("\\pard\\plain\\f0\\fs20 Line {0} of the message body.\\par\r\n", i);

			builder.Append ('}');

			var rtf = Encoding.ASCII.GetBytes (builder.ToString ());

			AssertRoundTrip (rtf, true, chunkSize);

			Assert.That (Compress (rtf, true, chunkSize).Length, Is.LessThan (rtf.Length / 3), "compression ratio");
		}

		[TestCase (0)]
		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		public void TestRandomInput (int seed)
		{
			var random = new Random (seed);
			var rtf = new byte[20000 + seed * 1234];

			// Note: Use a small alphabet for some of the inputs so that there are plenty of short and overlapping matches.
			for (int i = 0; i < rtf.Length; i++)
				rtf[i] = (byte) (seed % 2 == 0 ? random.Next (256) : 'a' + random.Next (3));

			AssertRoundTrip (rtf, true);
			AssertRoundTrip (rtf, true, 13);
			AssertRoundTrip (rtf, false);
		}

		[Test]
		public void TestRunsOfSingleByte ()
		{
			for (int length = 1; length < 64; length++) {
				var rtf = new byte[length];

				rtf.AsSpan ().Fill ((byte) 'x');

				AssertRoundTrip (rtf, true);
			}
		}

		[Test]
		public void TestWritesAtNonZeroOffset ()
		{
			var rtf = Encoding.ASCII.GetBytes ("{\\rtf1 hello hello hello}");

			using var output = new MemoryStream ();

			output.Write (new byte[] { 1, 2, 3 }, 0, 3);

			using (var stream = new RtfCompressedWriteStream (output, true, null))
				stream.Write (rtf, 0, rtf.Length);

			Assert.That (output.Position, Is.EqualTo (output.Length), "position restored to the end");

			var compressed = output.ToArray ().AsSpan (3).ToArray ();

			Assert.That (Decompress (compressed, RtfCompressionMode.Compressed), Is.EqualTo (rtf));
		}

		[Test]
		public async Task TestWriteAsync ()
		{
			var rtf = Encoding.ASCII.GetBytes ("{\\rtf1 hello hello hello}");

			using var output = new MemoryStream ();

			using (var stream = new RtfCompressedWriteStream (output, true, null)) {
				await stream.WriteAsync (rtf, 0, rtf.Length);
				await stream.FlushAsync ();
			}

			Assert.That (Decompress (output.ToArray (), RtfCompressionMode.Compressed), Is.EqualTo (rtf));
		}

		[Test]
		public void TestStreamMembers ()
		{
			using var output = new MemoryStream ();
			var stream = new RtfCompressedWriteStream (output, true, null);

			Assert.That (stream.CanRead, Is.False);
			Assert.That (stream.CanSeek, Is.False);
			Assert.That (stream.CanWrite, Is.True);
			Assert.Throws<NotSupportedException> (() => stream.Read (new byte[1], 0, 1));
			Assert.Throws<NotSupportedException> (() => stream.Seek (0, SeekOrigin.Begin));
			Assert.Throws<NotSupportedException> (() => stream.SetLength (0));
			Assert.Throws<NotSupportedException> (() => stream.Position = 0);

			stream.Write (new byte[] { (byte) '{', (byte) '}' }, 0, 2);
			Assert.That (stream.Length, Is.EqualTo (2));
			Assert.That (stream.Position, Is.EqualTo (2));

			stream.Dispose ();
			stream.Dispose ();

			Assert.That (stream.CanWrite, Is.False);
			Assert.Throws<ObjectDisposedException> (() => stream.Write (new byte[1], 0, 1));
			Assert.Throws<ObjectDisposedException> (() => stream.Flush ());
		}
	}
}
