//
// EncoderTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

namespace UnitTests {
	[TestFixture]
	public class EncoderTests
	{
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

		[Test]
		public void TestQuotedPrintableDecode ()
		{
			const string input = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).";
			const string expected = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[4096];

			var buf = Encoding.ASCII.GetBytes (input);
			int n = decoder.Decode (buf, 0, buf.Length, output);
			var actual = encoding.GetString (output, 0, n);

			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestQuotedPrintableEncode ()
		{
			const string expected = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1=\n =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).\n";
			const string input = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).\n";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[4096];

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
