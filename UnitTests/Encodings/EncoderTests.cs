//
// EncoderTests.cs
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
using System.Reflection;
using System.Security.Cryptography;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

namespace UnitTests.Encodings {
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
			"AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCU" +
			"mJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0" +
			"xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3Bxc" +
			"nN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeY" +
			"mZqbnJ2en6ChoqOkpaanqKmqq6ytrq+wsbKztLW2t7i5uru8vb6" +
			"/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5O" +
			"Xm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/w==",

			"AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSY" +
			"nKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QEFCQ0RFRkdISUpLTE" +
			"1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc" +
			"3R1dnd4eXp7fH1+f4CBgoOEhYaHiImKi4yNjo+QkZKTlJWWl5iZ" +
			"mpucnZ6foKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr/" +
			"AwcLDxMXGx8jJysvMzc7P0NHS09TV1tfY2drb3N3e3+Dh4uPk5e" +
			"bn6Onq6+zt7u/w8fLz9PX29/j5+vv8/f7/AA==",

			"AgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fICEiIyQlJic" +
			"oKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj9AQUJDREVGR0hJSktMTU" +
			"5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzd" +
			"HV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJma" +
			"m5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8D" +
			"BwsPExcbHyMnKy8zNzs/Q0dLT1NXW19jZ2tvc3d7f4OHi4+Tl5u" +
			"fo6err7O3u7/Dx8vP09fb3+Pn6+/z9/v8AAQ=="
		};
		static readonly string[] base64EncodedPatternsExtraPadding = {
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ===",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ====",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=====",
			"VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ======",
		};

		static readonly string dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "encoders");
		static readonly byte[] wikipedia_unix;
		static readonly byte[] wikipedia_dos;
		static readonly byte[] photo;

		static EncoderTests ()
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Dos2UnixFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.txt")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_unix = memory.ToArray ();
			}

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.txt")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_dos = memory.ToArray ();
			}

			photo = File.ReadAllBytes (Path.Combine (dataDir, "photo.jpg"));
		}

		[Test]
		public void TestConstructorArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new Base64Encoder (0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new QuotedPrintableEncoder (0));
		}

		static void TestEncoder (ContentEncoding encoding, byte[] rawData, string encodedFile, int bufferSize)
		{
			int n;

			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead (Path.Combine (dataDir, encodedFile))) {
					using (var filtered = new FilteredStream (original)) {
						filtered.Add (new Dos2UnixFilter ());
						file.CopyTo (filtered, 4096);
						filtered.Flush ();
					}
				}

				using (var encoded = new MemoryStream ()) {
					if (encoding == ContentEncoding.UUEncode) {
						var begin = Encoding.ASCII.GetBytes ("begin 644 photo.jpg\n");
						encoded.Write (begin, 0, begin.Length);
					}

					using (var filtered = new FilteredStream (encoded)) {
						filtered.Add (EncoderFilter.Create (encoding));

						using (var memory = new MemoryStream (rawData, false)) {
							var buffer = new byte[bufferSize];

							while ((n = memory.Read (buffer, 0, bufferSize)) > 0)
								filtered.Write (buffer, 0, n);
						}

						filtered.Flush ();
					}

					if (encoding == ContentEncoding.UUEncode) {
						var end = Encoding.ASCII.GetBytes ("end\n");
						encoded.Write (end, 0, end.Length);
					}

					var buf0 = original.GetBuffer ();
					var buf1 = encoded.GetBuffer ();
					n = (int) original.Length;

					Assert.That (encoded.Length, Is.EqualTo (original.Length), "Encoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.That (buf1[i], Is.EqualTo (buf0[i]), $"The byte at offset {i} does not match.");
				}
			}
		}

		static void TestDecoder (ContentEncoding encoding, byte[] rawData, string encodedFile, int bufferSize, bool unix = false)
		{
			int n;

			using (var decoded = new MemoryStream ()) {
				using (var filtered = new FilteredStream (decoded)) {
					filtered.Add (DecoderFilter.Create (encoding));

					if (unix)
						filtered.Add (new Dos2UnixFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, encodedFile))) {
						var buffer = new byte[bufferSize];

						while ((n = file.Read (buffer, 0, bufferSize)) > 0)
							filtered.Write (buffer, 0, n);
					}

					filtered.Flush ();
				}

				var buf = decoded.GetBuffer ();
				n = rawData.Length;

				Assert.That (decoded.Length, Is.EqualTo (rawData.Length), "Decoded length is incorrect.");

				for (int i = 0; i < n; i++)
					Assert.That (buf[i], Is.EqualTo (rawData[i]), $"The byte at offset {i} does not match.");
			}
		}

		[Test]
		public void TestBase64DecodePatterns ()
		{
			var decoder = new Base64Decoder ();
			var output = new byte[4096];

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Base64));

			for (int i = 0; i < base64EncodedPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedPatterns[i]);
				int n = decoder.Decode (buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (base64DecodedPatterns[i]), $"Failed to decode base64EncodedPatterns[{i}]");
			}

			for (int i = 0; i < base64EncodedLongPatterns.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedLongPatterns[i]);
				int n = decoder.Decode (buf, 0, buf.Length, output);

				for (int j = 0; j < n; j++)
					Assert.That ((byte) (j + i), Is.EqualTo (output[j]), $"Failed to decode base64EncodedLongPatterns[{i}]");
			}

			for (int i = 0; i < base64EncodedPatternsExtraPadding.Length; i++) {
				decoder.Reset ();
				var buf = Encoding.ASCII.GetBytes (base64EncodedPatternsExtraPadding[i]);
				int n = decoder.Decode (buf, 0, buf.Length, output);
				var actual = Encoding.ASCII.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (base64DecodedPatterns[0]), $"Failed to decode base64EncodedPatternsExtraPadding[{i}]");
			}
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestBase64Encode (int bufferSize)
		{
			TestEncoder (ContentEncoding.Base64, photo, "photo.b64", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestBase64Decode (int bufferSize)
		{
			TestDecoder (ContentEncoding.Base64, photo, "photo.b64", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestUUEncode (int bufferSize)
		{
			TestEncoder (ContentEncoding.UUEncode, photo, "photo.uu", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestUUDecode (int bufferSize)
		{
			TestDecoder (ContentEncoding.UUEncode, photo, "photo.uu", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestUUDecodeBeginStateChanges (int bufferSize)
		{
			TestDecoder (ContentEncoding.UUEncode, photo, "photo.uu-states", bufferSize);
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
		public void TestQuotedPrintableDecodePatterns ()
		{
			var decoder = new QuotedPrintableDecoder ();
			var encoding = CharsetUtils.Latin1;
			var output = new byte[4096];
			string actual;
			byte[] buf;
			int n;

			for (int i = 0; i < qpEncodedPatterns.Length; i++) {
				decoder.Reset ();
				buf = encoding.GetBytes (qpEncodedPatterns[i]);
				n = decoder.Decode (buf, 0, buf.Length, output);
				actual = encoding.GetString (output, 0, n);
				Assert.That (actual, Is.EqualTo (qpDecodedPatterns[i]), $"Failed to decode qpEncodedPatterns[{i}]");
			}
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestQuotedPrintableEncodeDos (int bufferSize)
		{
			TestEncoder (ContentEncoding.QuotedPrintable, wikipedia_dos, "wikipedia.qp", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestQuotedPrintableEncodeUnix (int bufferSize)
		{
			TestEncoder (ContentEncoding.QuotedPrintable, wikipedia_unix, "wikipedia.qp", bufferSize);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestQuotedPrintableDecode (int bufferSize)
		{
			TestDecoder (ContentEncoding.QuotedPrintable, wikipedia_unix, "wikipedia.qp", bufferSize, true);
		}

		[Test]
		public void TestQuotedPrintableEncodeSpaceDosLineBreak ()
		{
			const string input = "This line ends with a space \r\nbefore a line break.";
			const string expected = "This line ends with a space=20\nbefore a line break.=\n";
			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			buf = Encoding.ASCII.GetBytes (input);
			n = encoder.Flush (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestQuotedPrintableEncodeSpaceUnixLineBreak ()
		{
			const string input = "This line ends with a space \nbefore a line break.";
			const string expected = "This line ends with a space=20\nbefore a line break.=\n";
			var encoder = new QuotedPrintableEncoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			buf = Encoding.ASCII.GetBytes (input);
			n = encoder.Flush (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestQuotedPrintableEncodeEqualSignAt76 ()
		{
			var text = "<table style=\"width:100%;\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"width:100%;text-align:center;background-color:;\" bgcolor=\"\">Test</td></tr><table>";
			var input = Encoding.ASCII.GetBytes (text);
			var expected = "<table style=3D\"width:100%;\" cellpadding=3D\"0\" cellspacing=3D\"0\" border=3D\"=\n0\"><tr><td style=3D\"width:100%;text-align:center;background-color:;\" bgcolo=\nr=3D\"\">Test</td></tr><table>=\n";
			var encoder = new QuotedPrintableEncoder (76);
			var output = new byte[encoder.EstimateOutputLength (input.Length)];
			var outputLength = encoder.Flush (input, 0, input.Length, output);
			var encoded = Encoding.ASCII.GetString (output, 0, outputLength);

			Assert.That (encoded, Is.EqualTo (expected));

			var decoder = new QuotedPrintableDecoder ();
			var buffer = new byte[decoder.EstimateOutputLength (outputLength)];
			var decodedLength = decoder.Decode (output, 0, outputLength, buffer);
			var decoded = Encoding.ASCII.GetString (buffer, 0, decodedLength);

			Assert.That (decoded, Is.EqualTo (text));
		}

		[Test]
		public void TestQuotedPrintableEncodeFlush ()
		{
			const string input = "This line ends with a space ";
			const string expected = "This line ends with a space=20=\n";
			var encoder = new QuotedPrintableEncoder ();
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			buf = Encoding.ASCII.GetBytes (input);
			n = encoder.Flush (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));

			buf = Encoding.ASCII.GetBytes (expected);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (input));
		}

		[Test]
		public void TestQuotedPrintableDecodeInvalidSoftBreak ()
		{
			const string input = "This is an invalid=\rsoft break.";
			const string expected = "This is an invalid=\rsoft break.";
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[1024];
			string actual;
			byte[] buf;
			int n;

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			buf = Encoding.ASCII.GetBytes (input);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = Encoding.ASCII.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestQuotedPrintableDecode2 ()
		{
			const string input = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).";
			const string expected = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var decoder = new QuotedPrintableDecoder ();
			var output = new byte[4096];
			string actual;
			byte[] buf;
			int n;

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			buf = Encoding.ASCII.GetBytes (input);
			n = decoder.Decode (buf, 0, buf.Length, output);
			actual = encoding.GetString (output, 0, n);
			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestQuotedPrintableEncode2 ()
		{
			const string expected = "This is an ordinary text message in which my name (=ED=E5=EC=F9 =EF=E1 =\n=E9=EC=E8=F4=F0)\nis in Hebrew (=FA=E9=F8=E1=F2).\n";
			const string input = "This is an ordinary text message in which my name (םולש ןב ילטפנ)\nis in Hebrew (תירבע).\n";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var encoder = new QuotedPrintableEncoder (72);
			var output = new byte[1024];

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.QuotedPrintable));

			var buf = encoding.GetBytes (input);
			int n = encoder.Flush (buf, 0, buf.Length, output);
			var actual = Encoding.ASCII.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestHexDecoder ()
		{
			const string input = "This should decode: (%ED%E5%EC%F9 %EF%E1 %E9%EC%E8%F4%F0) while %X1%S1%Z1 should not";
			const string expected = "This should decode: (םולש ןב ילטפנ) while %X1%S1%Z1 should not";
			var encoding = Encoding.GetEncoding ("iso-8859-8");
			var decoder = new HexDecoder ();
			var output = new byte[1024];

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Default));

			var buf = Encoding.ASCII.GetBytes (input);
			int n = decoder.Decode (buf, 0, buf.Length, output);
			var actual = encoding.GetString (output, 0, n);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestPassThroughEncoder ()
		{
			var encoder = new PassThroughEncoder (ContentEncoding.Default);
			var output = new byte[4096];
			var input = new byte[4096];

			for (int i = 0; i < input.Length; i++)
				input[i] = (byte) (i & 0xff);

			AssertArgumentExceptions (encoder);

			int n = encoder.Encode (input, 0, input.Length, output);

			Assert.That (n, Is.EqualTo (input.Length));

			for (int i = 0; i < n; i++)
				Assert.That (output[i], Is.EqualTo (input[i]));

			n = encoder.Flush (input, 0, input.Length, output);

			Assert.That (n, Is.EqualTo (input.Length));

			for (int i = 0; i < n; i++)
				Assert.That (output[i], Is.EqualTo (input[i]));

			encoder.Clone ().Reset ();
		}

		[Test]
		public void TestPassThroughDecoder ()
		{
			var decoder = new PassThroughDecoder (ContentEncoding.Default);
			var output = new byte[4096];
			var input = new byte[4096];

			for (int i = 0; i < input.Length; i++)
				input[i] = (byte) (i & 0xff);

			AssertArgumentExceptions (decoder);

			int n = decoder.Decode (input, 0, input.Length, output);

			Assert.That (n, Is.EqualTo (input.Length));

			for (int i = 0; i < n; i++)
				Assert.That (output[i], Is.EqualTo (input[i]));

			decoder.Clone ().Reset ();
		}

		static void AssertIsEncoderFilter (ContentEncoding encoding, ContentEncoding expected)
		{
			var filter = EncoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<EncoderFilter> (), $"Expected EncoderFilter for ContentEncoding.{encoding}");

			var encoder = (EncoderFilter) filter;

			Assert.That (encoder.Encoding, Is.EqualTo (expected), $"Expected encoder's Encoding to be ContentEncoding.{expected}");
		}

		static void AssertIsEncoderFilter (string encoding, ContentEncoding expected)
		{
			var filter = EncoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<EncoderFilter> (), $"Expected EncoderFilter for \"{encoding}\"");

			var encoder = (EncoderFilter) filter;

			Assert.That (encoder.Encoding, Is.EqualTo (expected), $"Expected encoder's Encoding to be ContentEncoding.{expected}");
		}

		[Test]
		public void TestCreateEncoders ()
		{
			Assert.Throws<ArgumentNullException> (() => EncoderFilter.Create (null));
			Assert.Throws<ArgumentNullException> (() => DecoderFilter.Create (null));

			AssertIsEncoderFilter (ContentEncoding.Base64, ContentEncoding.Base64);
			AssertIsEncoderFilter ("base64", ContentEncoding.Base64);

			Assert.That (EncoderFilter.Create (ContentEncoding.Binary), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("binary"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (EncoderFilter.Create (ContentEncoding.Default), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("x-invalid"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (EncoderFilter.Create (ContentEncoding.EightBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("8bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsEncoderFilter (ContentEncoding.QuotedPrintable, ContentEncoding.QuotedPrintable);
			AssertIsEncoderFilter ("quoted-printable", ContentEncoding.QuotedPrintable);

			Assert.That (EncoderFilter.Create (ContentEncoding.SevenBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("7bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsEncoderFilter (ContentEncoding.UUEncode, ContentEncoding.UUEncode);
			AssertIsEncoderFilter ("x-uuencode", ContentEncoding.UUEncode);
			AssertIsEncoderFilter ("uuencode", ContentEncoding.UUEncode);
		}

		static void AssertIsDecoderFilter (ContentEncoding encoding, ContentEncoding expected)
		{
			var filter = DecoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf <DecoderFilter> (), $"Expected DecoderFilter for ContentEncoding.{encoding}");

			var decoder = (DecoderFilter) filter;

			Assert.That (decoder.Encoding, Is.EqualTo (expected), $"Expected decoder's Encoding to be ContentEncoding.{expected}");
		}

		static void AssertIsDecoderFilter (string encoding, ContentEncoding expected)
		{
			var filter = DecoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<DecoderFilter> (), $"Expected DecoderFilter for \"{encoding}\"");

			var decoder = (DecoderFilter) filter;

			Assert.That (decoder.Encoding, Is.EqualTo (expected), $"Expected decoder's Encoding to be ContentEncoding.{expected}");
		}

		[Test]
		public void TestCreateDecoders ()
		{
			AssertIsDecoderFilter (ContentEncoding.Base64, ContentEncoding.Base64);
			AssertIsDecoderFilter ("base64", ContentEncoding.Base64);

			Assert.That (DecoderFilter.Create (ContentEncoding.Binary), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("binary"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (DecoderFilter.Create (ContentEncoding.Default), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("x-invalid"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (DecoderFilter.Create (ContentEncoding.EightBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("8bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsDecoderFilter (ContentEncoding.QuotedPrintable, ContentEncoding.QuotedPrintable);
			AssertIsDecoderFilter ("quoted-printable", ContentEncoding.QuotedPrintable);

			Assert.That (DecoderFilter.Create (ContentEncoding.SevenBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("7bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsDecoderFilter (ContentEncoding.UUEncode, ContentEncoding.UUEncode);
			AssertIsDecoderFilter ("x-uuencode", ContentEncoding.UUEncode);
			AssertIsDecoderFilter ("uuencode", ContentEncoding.UUEncode);
		}

		static object GetEnumValue (Type type, int value)
		{
			return Enum.Parse (type, value.ToString ());
		}

		static void SetRandomState (object fsm)
		{
			var random = new Random ();

			foreach (var field in fsm.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				if ((field.Attributes & FieldAttributes.InitOnly) != 0)
					continue;

				if (field.FieldType.IsEnum) {
					field.SetValue (fsm, GetEnumValue (field.FieldType, random.Next (1, 255)));
				} else if (field.FieldType == typeof (int)) {
					field.SetValue (fsm, random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (uint)) {
					field.SetValue (fsm, (uint) random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (bool)) {
					field.SetValue (fsm, true);
				} else if (field.FieldType == typeof (byte)) {
					field.SetValue (fsm, (byte) random.Next (1, 255));
				} else if (field.FieldType == typeof (short)) {
					field.SetValue (fsm, (short) random.Next (1, short.MaxValue));
				} else if (field.FieldType == typeof (Crc32)) {
					var crc = (Crc32) field.GetValue (fsm);
					var buf = new byte[100];

					using (var rng = RandomNumberGenerator.Create ())
						rng.GetBytes (buf);

					crc.Update (buf, 0, buf.Length);
				} else {
					Assert.Fail ($"Unknown field type: {fsm.GetType ().Name}.{field.Name}");
				}
			}
		}

		static void AssertArgumentExceptions (IMimeEncoder encoder)
		{
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => encoder.Encode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => encoder.Encode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => encoder.Encode (new byte[1], 0, 1, output));

			Assert.Throws<ArgumentNullException> (() => encoder.Flush (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Flush (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Flush (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => encoder.Flush (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => encoder.Flush (new byte[1], 0, 1, output));
		}

		static void AssertArgumentExceptions (IMimeDecoder decoder)
		{
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => decoder.Decode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Decode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Decode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => decoder.Decode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => decoder.Decode (new byte[1], 0, 1, output));
		}

		static void AssertState (object encoder, object clone)
		{
			foreach (var field in encoder.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				var expected = field.GetValue (encoder);
				var actual = field.GetValue (clone);

				if (expected.GetType () == typeof (Crc32)) {
					var crc0 = (Crc32) expected;
					var crc1 = (Crc32) actual;

					Assert.That (crc1.Checksum, Is.EqualTo (crc0.Checksum), $"The cloned {encoder.GetType ().Name}.{field.Name} does not match.");
				} else {
					Assert.That (actual, Is.EqualTo (expected), $"The cloned {encoder.GetType ().Name}.{field.Name} does not match.");
				}
			}
		}

		static void CloneAndAssert (IMimeEncoder encoder)
		{
			// first, set some random state
			SetRandomState (encoder);

			var clone = encoder.Clone ();

			Assert.That (clone.Encoding, Is.EqualTo (encoder.Encoding));

			AssertState (encoder, clone);

			AssertArgumentExceptions (encoder);
		}

		static void CloneAndAssert (IMimeDecoder decoder)
		{
			// first, set some random state
			SetRandomState (decoder);

			var clone = decoder.Clone ();

			AssertState (decoder, clone);

			AssertArgumentExceptions (decoder);
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new Base64Encoder (true, 76));
			CloneAndAssert (new Base64Encoder (false, 76));
			CloneAndAssert (new HexEncoder ());
			CloneAndAssert (new QEncoder (QEncodeMode.Text));
			CloneAndAssert (new QEncoder (QEncodeMode.Phrase));
			CloneAndAssert (new QuotedPrintableEncoder ());
			CloneAndAssert (new UUEncoder ());
			CloneAndAssert (new YEncoder (76));
			
			CloneAndAssert (new Base64Decoder ());
			CloneAndAssert (new HexDecoder ());
			CloneAndAssert (new QuotedPrintableDecoder (true));
			CloneAndAssert (new QuotedPrintableDecoder (false));
			CloneAndAssert (new UUDecoder (true));
			CloneAndAssert (new UUDecoder (false));
			CloneAndAssert (new YDecoder (true));
			CloneAndAssert (new YDecoder (false));
		}

		static void ResetAndAssert (IMimeEncoder encoder)
		{
			// clone the encoder at it's initial state
			var clone = encoder.Clone ();

			// set some random state on the clone
			SetRandomState (clone);

			// reset the clone and make sure it matches
			clone.Reset ();

			AssertState (encoder, clone);
		}

		static void ResetAndAssert (IMimeDecoder decoder)
		{
			// clone the decoder at it's initial state
			var clone = decoder.Clone ();

			// set some random state on the clone
			SetRandomState (clone);

			// reset the clone and make sure it matches
			clone.Reset ();

			AssertState (decoder, clone);
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new Base64Encoder (true, 76));
			ResetAndAssert (new Base64Encoder (false, 76));
			ResetAndAssert (new HexEncoder ());
			ResetAndAssert (new QEncoder (QEncodeMode.Text));
			ResetAndAssert (new QEncoder (QEncodeMode.Phrase));
			ResetAndAssert (new QuotedPrintableEncoder ());
			ResetAndAssert (new UUEncoder ());
			ResetAndAssert (new YEncoder (76));
			
			ResetAndAssert (new Base64Decoder ());
			ResetAndAssert (new HexDecoder ());
			ResetAndAssert (new QuotedPrintableDecoder (true));
			ResetAndAssert (new QuotedPrintableDecoder (false));
			ResetAndAssert (new UUDecoder (true));
			ResetAndAssert (new UUDecoder (false));
			ResetAndAssert (new YDecoder (true));
			ResetAndAssert (new YDecoder (false));
		}
	}
}
