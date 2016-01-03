//
// EncoderTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
using MimeKit.Utils;
using MimeKit.IO.Filters;
using MimeKit.Encodings;

namespace UnitTests {
	[TestFixture]
	public class EncoderTests
	{
		static readonly string[] base64EncodedPatterns = {
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCBvbmNlLi4=",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCB0d2ljZQ==",
			"VGhpcyBpcyBhIHRleHQgd2hpY2ggd2lsbCBub3QgYmUgcGFkZGVk",
			" &% VGhp\r\ncyBp\r\ncyB0aGUgcGxhaW4g  \tdGV4dCBtZ?!XNzY*WdlIQ==",

		};
		static readonly string[] base64DecodedPatterns = {
			"This is the plain text message!",
			"This is a text which has to be padded once..",
			"This is a text which has to be padded twice",
			"This is a text which will not be padded",
			"This is the plain text message!"
		};
		static readonly string[] base64EncodedLongPatterns = {
			"AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCU"
			+ "mJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0"
			+ "xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3Bxc"
			+ "nN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeY"
			+ "mZqbnJ2en6ChoqOkpaanqKmqq6ytrq+wsbKztLW2t7i5uru8vb6"
			+ "/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5O"
			+ "Xm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/w==",

			"AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSY"
			+ "nKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QEFCQ0RFRkdISUpLTE"
			+ "1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc"
			+ "3R1dnd4eXp7fH1+f4CBgoOEhYaHiImKi4yNjo+QkZKTlJWWl5iZ"
			+ "mpucnZ6foKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr/"
			+ "AwcLDxMXGx8jJysvMzc7P0NHS09TV1tfY2drb3N3e3+Dh4uPk5e"
			+ "bn6Onq6+zt7u/w8fLz9PX29/j5+vv8/f7/AA==",

			"AgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fICEiIyQlJic"
			+ "oKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj9AQUJDREVGR0hJSktMTU"
			+ "5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzd"
			+ "HV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJma"
			+ "m5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8D"
			+ "BwsPExcbHyMnKy8zNzs/Q0dLT1NXW19jZ2tvc3d7f4OHi4+Tl5u"
			+ "fo6err7O3u7/Dx8vP09fb3+Pn6+/z9/v8AAQ=="
		};

		[Test]
		public void TestBase64Decode ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
						using (var filtered = new FilteredStream (file)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.Base64));

							filtered.CopyTo (decoded, 4096);
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}

			var decoder = new Base64Decoder ();
			var output = new byte[4096];

			Assert.AreEqual (ContentEncoding.Base64, decoder.Encoding);

			for (int i = 0; i < base64EncodedPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedPatterns[i]);
				int n = decoder.Decode (buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.AreEqual (base64DecodedPatterns[i], actual, "Failed to decode base64EncodedPatterns[{0}]", i);
			}

			for (int i = 0; i < base64EncodedLongPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedLongPatterns[i]);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				for (int j = 0; j < n; j++)
					Assert.AreEqual (output[j], (byte) (j + i), "Failed to decode base64EncodedLongPatterns[{0}]", i);
			}
		}

		[Test]
		public void TestBase64Encode ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
					using (var filtered = new FilteredStream (original)) {
						filtered.Add (new Dos2UnixFilter ());
						file.CopyTo (filtered, 4096);
						filtered.Flush ();
					}
				}

				using (var encoded = new MemoryStream ()) {
					using (var filtered = new FilteredStream (encoded)) {
						filtered.Add (EncoderFilter.Create (ContentEncoding.Base64));

						using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
							file.CopyTo (filtered, 4096);

						filtered.Flush ();
					}

					var buf0 = original.GetBuffer ();
					var buf1 = encoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, encoded.Length, "Encoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public void TestUUDecode ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.uu")) {
						using (var filtered = new FilteredStream (file)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.UUEncode));

							filtered.CopyTo (decoded, 4096);
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public void TestUUEncode ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.uu")) {
					using (var filtered = new FilteredStream (original)) {
						filtered.Add (new Dos2UnixFilter ());
						file.CopyTo (filtered, 4096);
						filtered.Flush ();
					}
				}

				using (var encoded = new MemoryStream ()) {
					var begin = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\n");
					var end = Encoding.ASCII.GetBytes ("end\n");

					encoded.Write (begin, 0, begin.Length);

					using (var filtered = new FilteredStream (encoded)) {
						filtered.Add (EncoderFilter.Create (ContentEncoding.UUEncode));

						using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
							file.CopyTo (filtered, 4096);

						filtered.Flush ();
					}

					encoded.Write (end, 0, end.Length);

					var buf0 = original.GetBuffer ();
					var buf1 = encoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, encoded.Length, "Encoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

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
		public void TestQuotedPrintableDecode ()
		{
			const string input = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).";
			const string expected = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[4096];
			string actual;
			byte[] buf;
			int n;

			Assert.AreEqual (ContentEncoding.QuotedPrintable, decoder.Encoding);

			buf = Encoding.ASCII.GetBytes (input);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = encoding.GetString (output, 0, n);
			Assert.AreEqual (expected, actual);

			encoding = CharsetUtils.Latin1;

			for (int i = 0; i < qpEncodedPatterns.Length; i++) {
				decoder.Reset ();
				buf = encoding.GetBytes (qpEncodedPatterns[i]);
				n = decoder.Decode (buf, 0, buf.Length, output);
				actual = encoding.GetString (output, 0, n);
				Assert.AreEqual (qpDecodedPatterns[i], actual, "Failed to decode qpEncodedPatterns[{0}]", i);
			}
		}

		[Test]
		public void TestQuotedPrintableEncode ()
		{
			const string expected = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1=\n =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).\n";
			const string input = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).\n";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[4096];

			Assert.AreEqual (ContentEncoding.QuotedPrintable, encoder.Encoding);

			var buf = encoding.GetBytes (input);
			int n = encoder.Flush (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestPassThroughEncode ()
		{
			var encoder = new PassThroughEncoder (ContentEncoding.Default);
			var output = new byte[4096];
			var input = new byte[4096];

			for (int i = 0; i < input.Length; i++)
				input[i] = (byte) (i & 0xff);

			int n = encoder.Encode (input, 0, input.Length, output);

			Assert.AreEqual (input.Length, n);

			for (int i = 0; i < n; i++)
				Assert.AreEqual (input[i], output[i]);

			n = encoder.Flush (input, 0, input.Length, output);

			Assert.AreEqual (input.Length, n);

			for (int i = 0; i < n; i++)
				Assert.AreEqual (input[i], output[i]);
		}

		[Test]
		public void TestPassThroughDecode ()
		{
			var decoder = new PassThroughDecoder (ContentEncoding.Default);
			var output = new byte[4096];
			var input = new byte[4096];

			for (int i = 0; i < input.Length; i++)
				input[i] = (byte) (i & 0xff);

			int n = decoder.Decode (input, 0, input.Length, output);

			Assert.AreEqual (input.Length, n);

			for (int i = 0; i < n; i++)
				Assert.AreEqual (input[i], output[i]);
		}
	}
}
