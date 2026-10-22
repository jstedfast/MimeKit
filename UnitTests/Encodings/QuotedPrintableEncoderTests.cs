//
// QuotedPrintableEncoderTests.cs
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
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class QuotedPrintableEncoderTests : MimeEncoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new QuotedPrintableEncoder (0));

			AssertArgumentExceptions (new QuotedPrintableEncoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new QuotedPrintableEncoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new QuotedPrintableEncoder (76));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new QuotedPrintableEncoder ());
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestEncodeDos (int bufferSize)
		{
			TestEncoder (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_dos, "wikipedia.qp", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestEncodeUnix (int bufferSize)
		{
			TestEncoder (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_unix, "wikipedia.qp", bufferSize);
		}

		[Test]
		public void TestFlushDos ()
		{
			TestEncoderFlush (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_dos, "wikipedia.qp");
		}

		[Test]
		public void TestFlushUnix ()
		{
			TestEncoderFlush (new QuotedPrintableEncoder (), "wikipedia.txt", wikipedia_unix, "wikipedia.qp");
		}

		[Test]
		public void TestEstimateOutputLengthAccountsForPartialLine ([Range (60, 76)] int maxLineLength, [Values (NewLineFormat.Dos, NewLineFormat.Unix)] NewLineFormat newLineFormat)
		{
			const byte guard = 0xCC;
			var patterns = new byte[][] {
				new byte[] { 0xFF },
				new byte[] { (byte) 'a' },
				new byte[] { (byte) ' ' },
				new byte[] { (byte) 'a', 0xFF, (byte) '\t' },
			};

			foreach (var pattern in patterns) {
				for (int prefix = 0; prefix <= 80; prefix++) {
					for (int suffix = 0; suffix <= 80; suffix++) {
						var input = new byte[prefix + suffix];
						var encoder = new QuotedPrintableEncoder (newLineFormat, maxLineLength);

						for (int i = 0; i < input.Length; i++)
							input[i] = pattern[i % pattern.Length];

						int estimate = encoder.EstimateOutputLength (prefix);
						var output = new byte[estimate + 16];
						Array.Fill (output, guard);

						int n = encoder.Encode (input, 0, prefix, output);
						Assert.That (n, Is.LessThanOrEqualTo (estimate), $"Encode: pattern={pattern.Length} prefix={prefix}");
						Assert.That (output.AsSpan (estimate).IndexOfAnyExcept (guard), Is.EqualTo (-1), $"Encode overrun: pattern={pattern.Length} prefix={prefix}");

						estimate = encoder.EstimateOutputLength (suffix);
						output = new byte[estimate + 16];
						Array.Fill (output, guard);

						n = encoder.Flush (input, prefix, suffix, output);
						Assert.That (n, Is.LessThanOrEqualTo (estimate), $"Flush: pattern={pattern.Length} prefix={prefix} suffix={suffix}");
						Assert.That (output.AsSpan (estimate).IndexOfAnyExcept (guard), Is.EqualTo (-1), $"Flush overrun: pattern={pattern.Length} prefix={prefix} suffix={suffix}");
					}
				}
			}
		}

		[Test]
		public void TestEncodeSpaceDosLineBreak ()
		{
			const string input = "This line ends with a space \r\nbefore a line break.";
			const string expected = "This line ends with a space=20\r\nbefore a line break.=\r\n";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var encoder = new QuotedPrintableEncoder ();

			try {
				var buf = Encoding.ASCII.GetBytes (input);
				int n = encoder.Flush (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestEncodeSpaceUnixLineBreak ()
		{
			const string input = "This line ends with a space \nbefore a line break.";
			const string expected = "This line ends with a space=20\r\nbefore a line break.=\r\n";
			var output = ArrayPool<byte>.Shared.Rent (1024);
			var encoder = new QuotedPrintableEncoder ();

			try {
				var buf = Encoding.ASCII.GetBytes (input);
				int n = encoder.Flush (buf, 0, buf.Length, output);

				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (expected));
			} finally {
				ArrayPool<byte>.Shared.Return (output);
			}
		}

		[Test]
		public void TestEncodeEqualSignAt76 ()
		{
			const string expected = "<table style=3D\"width:100%;\" cellpadding=3D\"0\" cellspacing=3D\"0\" border=3D\"=\r\n0\"><tr><td style=3D\"width:100%;text-align:center;background-color:;\" bgcolo=\r\nr=3D\"\">Test</td></tr><table>=\r\n";
			const string text = "<table style=\"width:100%;\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"width:100%;text-align:center;background-color:;\" bgcolor=\"\">Test</td></tr><table>";
			var input = Encoding.ASCII.GetBytes (text);
			var encoder = new QuotedPrintableEncoder (76);
			var output = new byte[encoder.EstimateOutputLength (input.Length)];
			var outputLength = encoder.Flush (input, 0, input.Length, output);
			var encoded = Encoding.ASCII.GetString (output, 0, outputLength);

			Assert.That (encoded, Is.EqualTo (expected));
		}

		[Test]
		public void TestFlush ()
		{
			const string input = "This line ends with a space ";
			const string expected = "This line ends with a space=20=\r\n";
			var encoder = new QuotedPrintableEncoder ();
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			buf = Encoding.ASCII.GetBytes (input);
			n = encoder.Flush (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));

			buf = Encoding.ASCII.GetBytes (expected);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (input));
		}

		static string Encode (QuotedPrintableEncoder encoder, byte[] input, bool oneByteAtATime)
		{
			var builder = new StringBuilder ();
			byte[] output;
			int n;

			if (oneByteAtATime) {
				for (int i = 0; i < input.Length; i++) {
					output = new byte[encoder.EstimateOutputLength (1)];
					n = encoder.Encode (input, i, 1, output);
					builder.Append (Encoding.ASCII.GetString (output, 0, n));
				}

				output = new byte[encoder.EstimateOutputLength (0)];
				n = encoder.Flush (input, input.Length, 0, output);
			} else {
				output = new byte[encoder.EstimateOutputLength (input.Length)];
				n = encoder.Flush (input, 0, input.Length, output);
			}

			builder.Append (Encoding.ASCII.GetString (output, 0, n));

			return builder.ToString ();
		}

		[Test]
		public void TestNewLineFormat ([Values (NewLineFormat.Dos, NewLineFormat.Unix)] NewLineFormat newLineFormat)
		{
			var newLine = newLineFormat == NewLineFormat.Dos ? "\r\n" : "\n";
			var input = Encoding.ASCII.GetBytes (new string ('a', 76) + " \r\nb\nc\rd\t");
			var expected = Encoding.ASCII.GetBytes (new string ('a', 75) + "=" + newLine + "a=20" + newLine + "b" + newLine + "c=0Dd=09=" + newLine);
			var encoder = new QuotedPrintableEncoder (newLineFormat);

			AssertNewLineFormat (encoder, input, expected);
			AssertNewLineFormat (encoder, Array.Empty<byte> (), Array.Empty<byte> ());

			var decoded = new byte[expected.Length];
			int count = new QuotedPrintableDecoder ().Decode (expected, 0, expected.Length, decoded);
			Assert.That (Encoding.ASCII.GetString (decoded, 0, count), Is.EqualTo (new string ('a', 76) + " " + newLine + "b" + newLine + "c\rd\t"));
		}

		static void AssertLineLengths (string encoded, int maxLineLength)
		{
			foreach (var line in encoded.Split ('\n'))
				Assert.That (line.TrimEnd ('\r').Length, Is.LessThanOrEqualTo (maxLineLength), $"Line too long: {line}");
		}

		[TestCase ("\n", false)]
		[TestCase ("\n", true)]
		[TestCase ("\r\n", false)]
		[TestCase ("\r\n", true)]
		public void TestEncodeTripletDoesNotExceedMaxLineLength (string newLine, bool oneByteAtATime)
		{
			var prefix = new string ('a', 74);
			var input = Encoding.Latin1.GetBytes (prefix + "\u00e9b" + newLine + prefix + "\u00e9" + newLine + prefix + " " + newLine + prefix + "\r\u00e9" + newLine);
			var expected = prefix + "=\r\n=E9b\r\n" + prefix + "=\r\n=E9\r\n" + prefix + "=\r\n=20\r\n" + prefix + "=\r\n=0D=E9\r\n";
			var encoded = Encode (new QuotedPrintableEncoder (), input, oneByteAtATime);

			Assert.That (encoded, Is.EqualTo (expected));
			AssertLineLengths (encoded, 76);
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestEncodeTripletDoesNotExceedMaxLineLengthOnFlush (bool oneByteAtATime)
		{
			var input = Encoding.Latin1.GetBytes (new string ('a', 73) + "\u00e9");
			var expected = new string ('a', 73) + "=\r\n=E9=\r\n";
			var encoded = Encode (new QuotedPrintableEncoder (), input, oneByteAtATime);

			Assert.That (encoded, Is.EqualTo (expected));
			AssertLineLengths (encoded, 76);
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestEncodeFullLineFollowedByCrLf (bool oneByteAtATime)
		{
			var line = new string ('a', 73) + "\u00e9";
			var input = Encoding.Latin1.GetBytes (line + "\r\n" + line + "\r");
			var expected = new string ('a', 73) + "=E9\r\n" + new string ('a', 73) + "=\r\n=E9=0D=\r\n";
			var encoded = Encode (new QuotedPrintableEncoder (), input, oneByteAtATime);

			Assert.That (encoded, Is.EqualTo (expected));
			AssertLineLengths (encoded, 76);
		}

		[Test]
		public void TestEncodeAllOctetsDoesNotExceedMaxLineLength ([Range (60, 76)] int maxLineLength, [Values (NewLineFormat.Dos, NewLineFormat.Unix)] NewLineFormat newLineFormat)
		{
			var random = new Random (maxLineLength);
			var input = new byte[16 * 1024];

			random.NextBytes (input);

			var encoder = new QuotedPrintableEncoder (newLineFormat, maxLineLength);
			var encoded = Encode (encoder, input, false);
			var buffer = Encoding.ASCII.GetBytes (encoded);
			var decoded = new byte[buffer.Length];
			int n = new QuotedPrintableDecoder ().Decode (buffer, 0, buffer.Length, decoded);

			AssertLineLengths (encoded, maxLineLength);
			Assert.That (Encode (encoder, input, true), Is.EqualTo (encoded), "one byte at a time");
			Assert.That (decoded.AsSpan (0, n).ToArray (), Is.EqualTo (CanonicalizeLineBreaks (input, newLineFormat)), "round trip");
		}

		static byte[] CanonicalizeLineBreaks (byte[] input, NewLineFormat newLineFormat)
		{
			var output = new List<byte> (input.Length);

			for (int i = 0; i < input.Length; i++) {
				if (input[i] == (byte) '\r' && i + 1 < input.Length && input[i + 1] == (byte) '\n')
					continue;

				if (input[i] == (byte) '\n' && newLineFormat == NewLineFormat.Dos)
					output.Add ((byte) '\r');

				output.Add (input[i]);
			}

			return output.ToArray ();
		}
		[Test]
		public void TestEncodeHebrew ()
		{
			const string expected = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =\r\n=E9=EC=E8=F4=F0)\r\nis in Hebrew (=FA=E9=F8=E1=F2).\r\n";
			const string input = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).\n";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QuotedPrintableEncoder (72);
			var output = new byte[1024];

			var buf = encoding.GetBytes (input);
			int n = encoder.Flush (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[TestCase ("")]
		[TestCase ("abc")]
		[TestCase ("abc ")]
		[TestCase ("abc\r")]
		public void TestEstimateOutputLengthDoesNotOverflow (string pending)
		{
			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[64];
			var input = Encoding.ASCII.GetBytes (pending);

			encoder.Encode (input, 0, input.Length, output);

			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.EstimateOutputLength (int.MaxValue));

			TestHelper.AssertEstimateOutputLengthDoesNotOverflow (encoder.EstimateOutputLength, pending);
		}
	}
}
