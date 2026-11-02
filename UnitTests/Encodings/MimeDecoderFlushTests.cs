//
// MimeDecoderFlushTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2026 .NET Foundation and Contributors
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

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

using UnitTests.IO;

namespace UnitTests.Encodings {
	[TestFixture]
	public class MimeDecoderFlushTests
	{
		static IMimeDecoder CreateDecoder (string name)
		{
			return name switch {
				"base64" => new Base64Decoder (),
				"qp" => new QuotedPrintableDecoder (),
				"q" => new QuotedPrintableDecoder (true),
				"rfc2231" => new Rfc2231Decoder (),
				"uu" => new UUDecoder (true),
				"uu-framed" => new UUDecoder (),
				"yenc" => new YEncDecoder (true),
				"yenc-framed" => new YEncDecoder (),
				"pass" => new PassThroughDecoder (ContentEncoding.Unspecified),
				_ => throw new ArgumentException ("Unknown decoder.", nameof (name))
			};
		}

		static IEnumerable<TestCaseData> FlushCases ()
		{
			var cases = new (string Decoder, string Input, string Output)[] {
				("base64", "", ""),
				("base64", "T", ""),
				("base64", "TQ", "M"),
				("base64", "TWE", "Ma"),
				("base64", "TQ=", "M"),
				("base64", "TWE=", "Ma"),
				("base64", "TWFu", "Man"),
				("base64", "TWFuTQ", "ManM"),
				("base64", "TQ==TWE", "MMa"),
				("base64", "T W\r\nE", "Ma"),
				("base64", "=Q", ""),
				("base64", "T=", ""),
				("base64", "T=Q", ""),
				("base64", "TW=", "M"),
				("base64", "====TQ", "M"),
				("qp", "", ""),
				("qp", "abc=", "abc="),
				("qp", "abc=4", "abc=4"),
				("qp", "abc=\r", "abc=\r"),
				("qp", "abc=\r\n", "abc"),
				("qp", "==", "=="),
				("qp", "=Q", "=Q"),
				("qp", "abc=4G", "abc=4G"),
				("qp", "=41", "A"),
				("q", "_=4", " =4"),
				("rfc2231", "", ""),
				("rfc2231", "%", "%"),
				("rfc2231", "%4", "%4"),
				("rfc2231", "%q", "%q"),
				("rfc2231", "%4Z", "%4Z"),
				("rfc2231", "%41%A", "A%A"),
				("uu", "", ""),
				("uu", "#3", ""),
				("uu", "#36", "M"),
				("uu", "#36%", "Ma"),
				("uu", "#36%N", "Man"),
				("uu", "!36%", "M"),
				("uu", "\"36%", "Ma"),
				("uu", "`\n", ""),
				("uu-framed", "begin", ""),
				("uu-framed", "begin 644 file\n#36%", "Ma"),
				("yenc", "", ""),
				("yenc", "*=", "\0"),
				("yenc", "=J", "\u00e0"),
				("yenc", "*=\r", "\0"),
				("yenc-framed", "=ybegin line=128 size=1 name=file\n=", ""),
				("yenc-framed", "=ybegin line=128 size=1 name=file\n*=", "\0"),
				("pass", "a\0\r\n=", "a\0\r\n=")
			};

			foreach (var item in cases)
				yield return new TestCaseData (item.Decoder, item.Input, item.Output);
		}

		static byte[] Convert (IMimeDecoder decoder, byte[] input, int index, int length, bool flush)
		{
			int estimate = decoder.EstimateOutputLength (length);
			var output = new byte[estimate + 16];
			Array.Fill (output, (byte) 0xcc);

			int count = flush ? decoder.Flush (input, index, length, output) : decoder.Decode (input, index, length, output);
			Assert.That (count, Is.InRange (0, estimate));
			Assert.That (output.AsSpan (estimate).ToArray (), Is.All.EqualTo (0xcc), "Output estimate");

			Array.Resize (ref output, count);
			return output;
		}

		[TestCaseSource (nameof (FlushCases))]
		public void TestFlush (string name, string text, string expected)
		{
			var input = Encoding.Latin1.GetBytes ("!" + text + "!");
			var decoded = Encoding.Latin1.GetBytes (expected);

			for (int split = 0; split <= text.Length; split++) {
				var decoder = CreateDecoder (name);
				var first = Convert (decoder, input, 1, split, false);
				Assert.That (Convert (decoder, Array.Empty<byte> (), 0, 0, false), Is.Empty, "Empty Decode is not EOF");
				var clone = decoder.Clone ();
				var last = Convert (decoder, input, split + 1, text.Length - split, true);

				Assert.That (first.Concat (last).ToArray (), Is.EqualTo (decoded), $"split={split}");
				Assert.That (Convert (clone, input, split + 1, text.Length - split, true), Is.EqualTo (last), "Clone");
				Assert.That (Convert (decoder, Array.Empty<byte> (), 0, 0, true), Is.Empty, "Repeated flush");

				if (decoder is YEncDecoder yenc) {
					var crc = new Crc32 (-1);
					crc.Update (decoded, 0, decoded.Length);
					Assert.That (yenc.Checksum, Is.EqualTo (crc.Checksum), "Checksum after flush");
				}

				decoder.Reset ();
				Assert.That (Convert (decoder, input, 1, text.Length, true), Is.EqualTo (decoded), "Reset");
			}
		}

		[TestCaseSource (nameof (FlushCases))]
		public void TestFilteredStreamFlush (string name, string text, string expected)
		{
			var input = Encoding.Latin1.GetBytes (text);
			foreach (bool read in new[] { false, true }) {
				using (var source = new ChunkedReadStream (input, 1))
				using (var output = new MemoryStream ())
				using (var filtered = new FilteredStream (read ? source : output)) {
					filtered.Add (new DecoderFilter (CreateDecoder (name)));
					if (read) {
						filtered.CopyTo (output);
						Assert.That (filtered.ReadByte (), Is.EqualTo (-1));
					} else {
						for (int i = 0; i < input.Length; i++)
							filtered.Write (input, i, 1);
						filtered.Flush ();
						filtered.Flush ();
					}
					Assert.That (Encoding.Latin1.GetString (output.ToArray ()), Is.EqualTo (expected), $"read={read}");
				}
			}
		}

		[TestCaseSource (nameof (FlushCases))]
		public async Task TestFilteredStreamFlushAsync (string name, string text, string expected)
		{
			var input = Encoding.Latin1.GetBytes (text);
			foreach (bool read in new[] { false, true }) {
				using (var source = new ChunkedReadStream (input, 1))
				using (var output = new MemoryStream ())
				using (var filtered = new FilteredStream (read ? source : output)) {
					filtered.Add (new DecoderFilter (CreateDecoder (name)));
					if (read) {
						await filtered.CopyToAsync (output);
						Assert.That (await filtered.ReadAsync (new byte[1], 0, 1), Is.Zero);
					} else {
						for (int i = 0; i < input.Length; i++)
							await filtered.WriteAsync (input, i, 1);
						await filtered.FlushAsync ();
						await filtered.FlushAsync ();
					}
					Assert.That (Encoding.Latin1.GetString (output.ToArray ()), Is.EqualTo (expected), $"read={read}");
				}
			}
		}

		[TestCase ("base64", "TQ", "M")]
		[TestCase ("qp", "=4", "=4")]
		[TestCase ("rfc2231", "%4", "%4")]
		[TestCase ("uu", "#36%", "Ma")]
		public void TestInvalidFlushPreservesState (string name, string text, string expected)
		{
			var decoder = CreateDecoder (name);
			var input = Encoding.ASCII.GetBytes (text);

			Assert.That (Convert (decoder, input, 0, input.Length, false), Is.Empty);
			var output = new byte[decoder.EstimateOutputLength (0)];
			Assert.Throws<ArgumentNullException> (() => decoder.Flush (null, 0, 0, output));
			Assert.Throws<ArgumentNullException> (() => decoder.Flush (input, 0, 0, null));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Flush (input, -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Flush (input, 0, -1, output));
			Assert.Throws<ArgumentException> (() => decoder.Flush (input, 0, 0, Array.Empty<byte> ()));
			Assert.That (Convert (decoder, input, 0, 0, true), Is.EqualTo (Encoding.ASCII.GetBytes (expected)));
		}

		[Test]
		public void TestFlushWithExactOutputEstimate ()
		{
			var decoder = new Base64Decoder ();
			var input = Encoding.ASCII.GetBytes ("TWFuTQ");
			var output = new byte[decoder.EstimateOutputLength (3)];

			Assert.That (decoder.Decode (input, 0, 3, output), Is.Zero);
			output = new byte[decoder.EstimateOutputLength (3)];
			int count = decoder.Flush (input, 3, 3, output);
			Assert.That (count, Is.EqualTo (4));
			Assert.That (Encoding.ASCII.GetString (output, 0, count), Is.EqualTo ("ManM"));
		}

		[Test]
		public void TestPointerDecodeIsNotPublic ()
		{
			var types = new[] { typeof (IMimeDecoder), typeof (Base64Decoder), typeof (QuotedPrintableDecoder),
				typeof (UUDecoder), typeof (YEncDecoder), typeof (PassThroughDecoder), typeof (Rfc2231Decoder) };

			foreach (var type in types) {
				foreach (var method in type.GetMethods ()) {
					if (method.Name == nameof (IMimeDecoder.Decode))
						Assert.That (method.GetParameters ().Any (parameter => parameter.ParameterType.IsPointer), Is.False, type.Name);
				}
			}
		}
	}
}
