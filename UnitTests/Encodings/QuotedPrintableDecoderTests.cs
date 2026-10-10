//
// QuotedPrintableDecoderTests.cs
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
using System.Buffers;

using MimeKit;
using MimeKit.Utils;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableDecoderTests : MimeDecoderTestsBase
	{
		static readonly string[] qpEncodedPatterns = {
			"=e1=e2=E3=E4\r\n",
			"=e1=g2=E3=E4\r\n",
			"=e1=eg=E3=E4\r\n",
			"   =e1 =e2  =E3\t=E4  \t \t    \r\n",
			"Soft line=\r\n\tHard line\r\n",
			"width==\r\n340 height=3d200\r\n",

		};
		static readonly string[] qpDecodedPatterns = {
			"\u00e1\u00e2\u00e3\u00e4\r\n",
			"\u00e1=g2\u00e3\u00e4\r\n",
			"\u00e1=eg\u00e3\u00e4\r\n",
			"   \u00e1 \u00e2  \u00e3\t\u00e4  \t \t    \r\n",
			"Soft line\tHard line\r\n",
			"width=340 height=200\r\n"
		};

		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new QuotedPrintableDecoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new QuotedPrintableDecoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new QuotedPrintableDecoder (true));
			CloneAndAssert (new QuotedPrintableDecoder (false));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new QuotedPrintableDecoder (true));
			ResetAndAssert (new QuotedPrintableDecoder (false));
		}

		[Test]
		public void TestDecodePatterns ()
		{
			var output = ArrayPool<byte>.Shared.Rent (4096);
			var decoder = new QuotedPrintableDecoder ();
			var encoding = CharsetUtils.Latin1;

			try {
				for (int i = 0; i < qpEncodedPatterns.Length; i++) {
					decoder.Reset ();

					var buf = encoding.GetBytes (qpEncodedPatterns[i]);
					int n = decoder.Decode (buf, 0, buf.Length, output);

					var actual = encoding.GetString (output, 0, n);
					Assert.That (actual, Is.EqualTo (qpDecodedPatterns[i]), $"Failed to decode qpEncodedPatterns[{i}]");
				}
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestDecode (int bufferSize)
		{
			TestDecoder (new QuotedPrintableDecoder (), wikipedia_unix, "wikipedia.qp", bufferSize, true);
		}

		[Test]
		public void TestDecodeEqualSignAt76 ()
		{
			const string encoded = "<table style=3D\"width:100%;\" cellpadding=3D\"0\" cellspacing=3D\"0\" border=3D\"=\n0\"><tr><td style=3D\"width:100%;text-align:center;background-color:;\" bgcolo=\nr=3D\"\">Test</td></tr><table>=\n";
			const string expected = "<table style=\"width:100%;\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"width:100%;text-align:center;background-color:;\" bgcolor=\"\">Test</td></tr><table>";
			var input = Encoding.ASCII.GetBytes (encoded);
			var decoder = new QuotedPrintableDecoder ();

			var output = new byte[decoder.EstimateOutputLength (input.Length)];
			var decodedLength = decoder.Decode (input, 0, input.Length, output);
			var decoded = Encoding.ASCII.GetString (output, 0, decodedLength);

			Assert.That (decoded, Is.EqualTo (expected));
		}

		[Test]
		public void TestDecodeInvalidSoftBreak ()
		{
			const string input = "This is an invalid=\rsoft break.";
			const string expected = "This is an invalid=\rsoft break.";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var decoder = new QuotedPrintableDecoder ();

			try {
				Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				var buf = Encoding.ASCII.GetBytes (input);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestDecodeHebrew ()
		{
			const string input = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).";
			const string expected = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var output = ArrayPool<byte>.Shared.Rent (4096);
			var decoder = new QuotedPrintableDecoder ();

			try {
				Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

				var buf = Encoding.ASCII.GetBytes (input);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				var actual = encoding.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		static byte[] DecodeInChunks (QuotedPrintableDecoder decoder, byte[] input, int chunkSize)
		{
			var output = new byte[decoder.EstimateOutputLength (input.Length) + 2];
			int outputLength = 0;

			for (int index = 0; index < input.Length; index += chunkSize) {
				int length = Math.Min (chunkSize, input.Length - index);
				var chunk = new byte[decoder.EstimateOutputLength (length)];
				int n = decoder.Decode (input, index, length, chunk);

				Buffer.BlockCopy (chunk, 0, output, outputLength, n);
				outputLength += n;
			}

			Array.Resize (ref output, outputLength);

			return output;
		}

		static IEnumerable<byte[]> GetPassThroughRunLengthInputs ()
		{
			// Place '=' sequences at, before and after the scalar scan length (16) as well as
			// much further into the input so that both the scalar and IndexOf() paths are used.
			foreach (var runLength in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 100, 1000 }) {
				var builder = new StringBuilder ();

				for (int i = 0; i < 4; i++) {
					builder.Append ('x', runLength);
					builder.Append ("=C3=A9");
					builder.Append ('y', runLength);
					builder.Append ("=\r\n");
					builder.Append ('z', runLength);
					builder.Append ("=\n");
					builder.Append ('w', runLength);
					builder.Append ("==3D=g=\rX=");
				}

				builder.Append ('v', runLength);

				yield return Encoding.ASCII.GetBytes (builder.ToString ());
			}
		}

		[Test]
		public void TestDecodePassThroughRunLengths ()
		{
			foreach (var input in GetPassThroughRunLengthInputs ()) {
				// Feeding the decoder 1 byte at a time only ever uses the byte-by-byte scan, so it serves as the reference.
				var expected = DecodeInChunks (new QuotedPrintableDecoder (), input, 1);

				foreach (var chunkSize in new[] { 2, 3, 15, 16, 17, 33, 77, input.Length }) {
					var actual = DecodeInChunks (new QuotedPrintableDecoder (), input, chunkSize);

					Assert.That (actual, Is.EqualTo (expected), $"input length = {input.Length}, chunk size = {chunkSize}");
				}
			}
		}

		[Test]
		public void TestDecodeLongPassThroughRun ()
		{
			var text = new string ('a', 1024) + "=3D" + new string ('b', 1024) + "=\r\n" + new string ('c', 1024);
			var expected = new string ('a', 1024) + "=" + new string ('b', 1024) + new string ('c', 1024);
			var input = Encoding.ASCII.GetBytes (text);
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[decoder.EstimateOutputLength (input.Length)];

			int n = decoder.Decode (input, 0, input.Length, output);

			Assert.That (Encoding.ASCII.GetString (output, 0, n), Is.EqualTo (expected));
		}

		[Test]
		public void TestDecodeInPlace ()
		{
			foreach (var input in GetPassThroughRunLengthInputs ()) {
				var expected = DecodeInChunks (new QuotedPrintableDecoder (), input, 1);
				var decoder = new QuotedPrintableDecoder ();
				var buffer = new byte[decoder.EstimateOutputLength (input.Length)];

				Buffer.BlockCopy (input, 0, buffer, 0, input.Length);

				int n = decoder.Decode (buffer, 0, input.Length, buffer);

				Assert.That (buffer.AsSpan (0, n).ToArray (), Is.EqualTo (expected), $"input length = {input.Length}");
			}
		}

		[Test]
		public void TestDecodeRfc2047LongPassThroughRun ()
		{
			var text = new string ('_', 64) + "=C3=A9" + new string ('a', 64) + "_b";
			var expected = new string (' ', 64) + "\u00e9" + new string ('a', 64) + " b";
			var input = Encoding.ASCII.GetBytes (text);
			var decoder = new QuotedPrintableDecoder (true);
			var output = new byte[decoder.EstimateOutputLength (input.Length)];

			int n = decoder.Decode (input, 0, input.Length, output);

			Assert.That (Encoding.UTF8.GetString (output, 0, n), Is.EqualTo (expected));
		}
	}
}
