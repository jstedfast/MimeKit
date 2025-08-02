//
// YEncodingTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

namespace UnitTests.Encodings {
	[TestFixture]
	public class YEncodingTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "yenc");

		static void TestYEncode (string fileName, byte[] content, int contentLength, uint crc32, MemoryStream expectedOutput)
		{
			using (var encoded = new MemoryStream ()) {
				var ybegin = Encoding.ASCII.GetBytes ($"-- \n=ybegin line=128 size={contentLength} name={fileName} \n");
				var yend = Encoding.ASCII.GetBytes ($"=yend size={contentLength} crc32={crc32:x8} \n");
				var yenc = new YEncoder ();

				encoded.Write (ybegin, 0, ybegin.Length);

				using (var filtered = new FilteredStream (encoded)) {
					filtered.Add (new EncoderFilter (yenc));

					using (var stream = new MemoryStream (content, false))
						stream.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				encoded.Write (yend, 0, yend.Length);

				Assert.That (yenc.Checksum ^ 0xffffffff, Is.EqualTo (crc32), "The encoded checksum does not match.");

				var latin1 = Encoding.GetEncoding ("iso-8859-1");
				var buf = expectedOutput.GetBuffer ();

				var expected = latin1.GetString (buf, 0, (int) expectedOutput.Length);

				buf = encoded.GetBuffer ();

				var actual = latin1.GetString (buf, 0, (int) encoded.Length);

				Assert.That (actual, Is.EqualTo (expected), "Encoded value does not match original.");
			}
		}

		static void TestYEncodeFlush (string fileName, byte[] content, int contentLength, uint crc32, MemoryStream expectedOutput)
		{
			var ybegin = Encoding.ASCII.GetBytes ($"-- \n=ybegin line=128 size={contentLength} name={fileName} \n");
			var yend = Encoding.ASCII.GetBytes ($"=yend size={contentLength} crc32={crc32:x8} \n");
			var yenc = new YEncoder ();

			int needed = yenc.EstimateOutputLength (contentLength) + ybegin.Length + yend.Length;
			byte[] encoded = new byte[needed];

			int encodedLength = yenc.Flush (content, 0, content.Length, encoded);

			if (ybegin.Length > 0) {
				// shift the encoded data to the right to make room for the "begin" line
				Buffer.BlockCopy (encoded, 0, encoded, ybegin.Length, encodedLength);
				Buffer.BlockCopy (ybegin, 0, encoded, 0, ybegin.Length);
				encodedLength += ybegin.Length;
			}

			if (yend.Length > 0) {
				// append the "end" line to the end of the encoded data
				Buffer.BlockCopy (yend, 0, encoded, encodedLength, yend.Length);
				encodedLength += yend.Length;
			}

			Assert.That (yenc.Checksum ^ 0xffffffff, Is.EqualTo (crc32), "The encoded checksum does not match.");

			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var buf = expectedOutput.GetBuffer ();

			var expected = latin1.GetString (buf, 0, (int) expectedOutput.Length);
			var actual = latin1.GetString (buf, 0, encodedLength);

			Assert.That (actual, Is.EqualTo (expected), "Encoded value does not match original.");
		}

		[Test]
		public void TestYDecodeSimpleMessage ()
		{
			using (var file = File.OpenRead (Path.Combine (DataDir, "simple.msg"))) {
				var message = MimeMessage.Load (file);

				using (var decoded = new MemoryStream ()) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));

						((MimePart) message.Body).Content.WriteTo (filtered);
						filtered.Flush ();
					}

					decoded.Position = 0;

					Assert.That (decoded.Length, Is.EqualTo (584), "The decoded size does not match.");
					Assert.That (ydec.Checksum ^ 0xffffffff, Is.EqualTo (0xded29f4f), "The decoded checksum does not match.");

					using (var original = new MemoryStream ()) {
						using (var filtered = new FilteredStream (original)) {
							filtered.Add (new Dos2UnixFilter ());

							((MimePart) message.Body).Content.WriteTo (filtered);
							filtered.Flush ();
						}

						original.Position = 0;

						TestYEncode ("testfile.txt", decoded.GetBuffer (), (int) decoded.Length, 0xded29f4f, original);

						original.Position = 0;

						TestYEncodeFlush ("testfile.txt", decoded.GetBuffer (), (int) decoded.Length, 0xded29f4f, original);
					}
				}
			}
		}

		[Test]
		public void TestYDecodeMultiPart ()
		{
			var expected = File.ReadAllBytes (Path.Combine (DataDir, "joystick.jpg"));

			using (var decoded = new MemoryStream ()) {
				using (var file = File.OpenRead (Path.Combine (DataDir, "00000020.ntx"))) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.That (decoded.Length, Is.EqualTo (11250), "The decoded size does not match (part 1).");
					Assert.That (ydec.Checksum ^ 0xffffffff, Is.EqualTo (0xbfae5c0b), "The decoded checksum does not match (part 1).");
				}

				using (var file = File.OpenRead (Path.Combine (DataDir, "00000021.ntx"))) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.That (decoded.Length, Is.EqualTo (19338), "The decoded size does not match (part 2).");
					Assert.That (ydec.Checksum ^ 0xffffffff, Is.EqualTo (0xaca76043), "The decoded checksum does not match (part 2).");
				}

				var actual = decoded.GetBuffer ();

				for (int i = 0; i < expected.Length; i++)
					Assert.That (actual[i], Is.EqualTo (expected[i]), $"different content at index {i}");
			}
		}

		[Test]
		public void TestYDecodeStateTransitions ()
		{
			using (var file = File.OpenRead (Path.Combine (DataDir, "state-changes.ntx"))) {
				using (var decoded = new MemoryStream ()) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.That (decoded.Length, Is.EqualTo (584), "The decoded size does not match.");
					Assert.That (ydec.Checksum ^ 0xffffffff, Is.EqualTo (0xded29f4f), "The decoded checksum does not match.");
				}
			}
		}

		static readonly string[] YPartTransitionInputs = {
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=xcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=yxcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=ypxcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=ypaxcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=yparxcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=ypartxcontent",
			"=ybegin part=1 line=128 size=19338 name=joystick.jpg\n=ypart begin=1 end=11250\ncontent",
		};

		static readonly string[] YPartTransitionOutputs = {
			"xcontent",
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			"content"
		};

		[Test]
		public void TestYDecodeYPartStateTransitions ()
		{
			var ydec = new YDecoder ();
			var decoded = new byte[1024];

			for (int i = 0; i < YPartTransitionInputs.Length; i++) {
				var input = Encoding.ASCII.GetBytes (YPartTransitionInputs[i]);
				var chars = YPartTransitionOutputs[i].ToCharArray ();

				for (int j = 0; j < chars.Length; j++)
					chars[j] -= (char) 42;

				var expected = new string (chars);

				int n = ydec.Decode (input, 0, input.Length, decoded);
				var actual = Encoding.ASCII.GetString (decoded, 0, n);

				Assert.That (actual, Is.EqualTo (expected), YPartTransitionInputs[i]);

				ydec.Reset ();
			}
		}
	}
}
