//
// YEncodingTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class YEncodingTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new YEncoder (59));
		}

		[Test]
		public void TestYDecodeSimpleMessage ()
		{
			using (var file = File.OpenRead ("../../TestData/yenc/simple.msg")) {
				var message = MimeMessage.Load (file);

				using (var decoded = new MemoryStream ()) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));

						((MimePart) message.Body).Content.WriteTo (filtered);
						filtered.Flush ();
					}

					decoded.Position = 0;

					Assert.AreEqual (584, decoded.Length, "The decoded size does not match.");
					Assert.AreEqual (0xded29f4f, ydec.Checksum ^ 0xffffffff, "The decoded checksum does not match.");

					// now re-encode it
					using (var encoded = new MemoryStream ()) {
						var ybegin = Encoding.ASCII.GetBytes ("-- \n=ybegin line=128 size=584 name=testfile.txt \n");
						var yend = Encoding.ASCII.GetBytes ("=yend size=584 crc32=ded29f4f \n");
						var yenc = new YEncoder ();

						encoded.Write (ybegin, 0, ybegin.Length);

						using (var filtered = new FilteredStream (encoded)) {
							filtered.Add (new EncoderFilter (yenc));

							decoded.CopyTo (filtered, 4096);
							filtered.Flush ();
						}

						encoded.Write (yend, 0, yend.Length);

						Assert.AreEqual (0xded29f4f, yenc.Checksum ^ 0xffffffff, "The encoded checksum does not match.");

						using (var original = new MemoryStream ()) {
							using (var filtered = new FilteredStream (original)) {
								filtered.Add (new Dos2UnixFilter ());

								((MimePart) message.Body).Content.WriteTo (filtered);
								filtered.Flush ();
							}

							var latin1 = Encoding.GetEncoding ("iso-8859-1");
							var buf = original.GetBuffer ();

							var expected = latin1.GetString (buf, 0, (int) original.Length);

							buf = encoded.GetBuffer ();

							var actual = latin1.GetString (buf, 0, (int) encoded.Length);

							Assert.AreEqual (expected, actual, "Encoded value does not match original.");
						}
					}
				}
			}
		}

		[Test]
		public void TestYDecodeMultiPart ()
		{
			var expected = File.ReadAllBytes ("../../TestData/yenc/joystick.jpg");

			using (var decoded = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/yenc/00000020.ntx")) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.AreEqual (11250, decoded.Length, "The decoded size does not match (part 1).");
					Assert.AreEqual (0xbfae5c0b, ydec.Checksum ^ 0xffffffff, "The decoded checksum does not match (part 1).");
				}

				using (var file = File.OpenRead ("../../TestData/yenc/00000021.ntx")) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.AreEqual (19338, decoded.Length, "The decoded size does not match (part 2).");
					Assert.AreEqual (0xaca76043, ydec.Checksum ^ 0xffffffff, "The decoded checksum does not match (part 2).");
				}

				var actual = decoded.GetBuffer ();

				for (int i = 0; i < expected.Length; i++)
					Assert.AreEqual (expected[i], actual[i], "different content at index {0}", i);
			}
		}

		[Test]
		public void TestYDecodeStateTransitions ()
		{
			using (var file = File.OpenRead ("../../TestData/yenc/state-changes.ntx")) {
				using (var decoded = new MemoryStream ()) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));
						file.CopyTo (filtered, 1);
						filtered.Flush ();
					}

					Assert.AreEqual (584, decoded.Length, "The decoded size does not match.");
					Assert.AreEqual (0xded29f4f, ydec.Checksum ^ 0xffffffff, "The decoded checksum does not match.");
				}
			}
		}
	}
}
