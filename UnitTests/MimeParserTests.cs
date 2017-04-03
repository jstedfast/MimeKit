//
// MimeParserTests.cs
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
using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace UnitTests {
	[TestFixture]
	public class MimeParserTests
	{
		static string MessagesDataDir = Path.Combine ("..", "..", "TestData", "messages");
		static string MboxDataDir = Path.Combine ("..", "..", "TestData", "mbox");
		static FormatOptions UnixFormatOptions;

		[SetUp]
		public void Setup ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		[Test]
		public void TestHeaderParser ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\nHeader-2: value 2\r\nHeader-3: value 3\r\n\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = HeaderList.Load (memory);
					string value;

					Assert.AreEqual (3, headers.Count, "Unexpected header count.");

					value = headers["Header-1"];

					Assert.AreEqual ("value 1", value, "Unexpected header value.");

					value = headers["Header-2"];

					Assert.AreEqual ("value 2", value, "Unexpected header value.");

					value = headers["Header-3"];

					Assert.AreEqual ("value 3", value, "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse headers: {0}", ex);
				}
			}
		}

		[Test]
		public void TestSingleHeaderNoTerminator ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = HeaderList.Load (memory);

					Assert.AreEqual (1, headers.Count, "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.AreEqual ("value 1", value, "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse headers: {0}", ex);
				}
			}
		}

		[Test]
		public void TestEmptyHeaders ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = HeaderList.Load (memory);

					Assert.AreEqual (0, headers.Count, "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse headers: {0}", ex);
				}
			}
		}

		[Test]
		public void TestEmptyMessage ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var message = MimeMessage.Load (memory);

					Assert.AreEqual (0, message.Headers.Count, "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse message: {0}", ex);
				}
			}
		}

		[Test]
		public void TestSimpleMbox ()
		{
			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();
					Multipart multipart;
					MimeEntity entity;

					Assert.IsInstanceOf<Multipart> (message.Body);
					multipart = (Multipart) message.Body;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOf<Multipart> (entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOf<Multipart> (entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOf<TextPart> (entity);

					using (var memory = new MemoryStream ()) {
						entity.WriteTo (UnixFormatOptions, memory);

						var text = Encoding.ASCII.GetString (memory.ToArray ());
						Assert.IsTrue (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), "Headers are not properly terminated.");
					}
				}
			}
		}

		static void DumpMimeTree (StringBuilder builder, MimeEntity entity, int depth)
		{
			if (depth > 0)
				builder.Append (new string (' ', depth * 3));

			builder.AppendFormat ("Content-Type: {0}/{1}", entity.ContentType.MediaType, entity.ContentType.MediaSubtype).Append ('\n');

			if (entity is Multipart) {
				var multipart = (Multipart) entity;
				foreach (var part in multipart)
					DumpMimeTree (builder, part, depth + 1);
			} else if (entity is MessagePart) {
				DumpMimeTree (builder, ((MessagePart) entity).Message.Body, depth + 1);
			}
		}

		static void DumpMimeTree (StringBuilder builder, MimeMessage message)
		{
			using (var iter = new MimeIterator (message)) {
				while (iter.MoveNext ()) {
					var ctype = iter.Current.ContentType;

					if (iter.Depth > 0)
						builder.Append (new string (' ', iter.Depth * 3));

					builder.AppendFormat ("Content-Type: {0}/{1}", ctype.MediaType, ctype.MediaSubtype).Append ('\n');
				}
			}
		}

		[Test]
		public void TestEmptyMultipartAlternative ()
		{
			string expected = @"Content-Type: multipart/mixed
   Content-Type: multipart/alternative
   Content-Type: text/plain
".Replace ("\r\n", "\n");

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "empty-multipart.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();
				var builder = new StringBuilder ();

				DumpMimeTree (builder, message);

				Assert.AreEqual (expected, builder.ToString (), "Unexpected MIME tree structure.");
			}
		}

		[Test]
		public void TestJwzMbox ()
		{
			var summary = File.ReadAllText (Path.Combine (MboxDataDir, "jwz-summary.txt")).Replace ("\r\n", "\n");
			var options = FormatOptions.Default.Clone ();
			var original = new MemoryBlockStream ();
			var output = new MemoryBlockStream ();
			var builder = new StringBuilder ();
			var expected = new byte[4096];
			var buffer = new byte[4096];
			int nx, n;

			options.NewLineFormat = NewLineFormat.Unix;

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);
				int count = 0;

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					var marker = Encoding.UTF8.GetBytes ((count > 0 ? "\n" : string.Empty) + parser.MboxMarker + "\n");
					output.Write (marker, 0, marker.Length);
					message.WriteTo (options, output);
					count++;
				}
			}

			string actual = builder.ToString ();

			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			Assert.AreEqual (summary, actual, "Summaries do not match for jwz.mbox");

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				using (var filtered = new FilteredStream (original)) {
					filtered.Add (new Dos2UnixFilter ());
					stream.CopyTo (filtered);
					filtered.Flush ();
				}
			}

			original.Position = 0;
			output.Position = 0;

			Assert.AreEqual (original.Length, output.Length, "The length of the mbox did not match.");

			do {
				var position = original.Position;

				nx = original.Read (expected, 0, expected.Length);
				n = output.Read (buffer, 0, buffer.Length);

				if (nx == 0)
					break;

				for (int i = 0; i < nx; i++) {
					if (buffer[i] == expected[i])
						continue;

					var strExpected = CharsetUtils.Latin1.GetString (expected, 0, nx);
					var strActual = CharsetUtils.Latin1.GetString (buffer, 0, n);

					Assert.AreEqual (strExpected, strActual, "The mbox differs at position {0}", position + i);
				}
			} while (true);
		}

		[Test]
		public void TestJwzPersistentMbox ()
		{
			var summary = File.ReadAllText (Path.Combine (MboxDataDir, "jwz-summary.txt")).Replace ("\r\n", "\n");
			var builder = new StringBuilder ();

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox, true);

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					// Force the various MimePart objects to write their content streams.
					// The idea is that by forcing the MimeParts to seek in their content,
					// we will test to make sure that the parser correctly deals with it.
					message.WriteTo (Stream.Null);
				}
			}

			string actual = builder.ToString ();

			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			Assert.AreEqual (summary, actual, "Summaries do not match for jwz.mbox");
		}

		[Test]
		public void TestJapaneseMessage ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var message = MimeMessage.Load (stream);

				Assert.AreEqual (subject, message.Subject, "Subject values do not match");
				Assert.AreEqual (body, message.TextBody.Replace ("\r\n", "\n"), "Message text does not match.");
			}
		}

		[Test]
		public void TestUnmungedFromLines ()
		{
			int count = 0;

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "unmunged.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					parser.ParseMessage ();

					var marker = parser.MboxMarker;

					if ((count % 2) == 0) {
						Assert.AreEqual ("From -", marker.TrimEnd (), "Message #{0}", count);
					} else {
						Assert.AreEqual ("From Russia with love", marker.TrimEnd (), "Message #{0}", count);
					}

					count++;
				}
			}

			Assert.AreEqual (4, count, "Expected to find 4 messages.");
		}

		[Test]
		public void TestMultipartEpilogueWithText ()
		{
			const string epilogue = "Peter Urka <pcu@umich.edu>\nDept. of Chemistry, Univ. of Michigan\nNewt-thought is right-thought.  Go Newt!\n\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "epilogue.txt"))) {
				var message = MimeMessage.Load (stream);
				var multipart = message.Body as Multipart;

				Assert.AreEqual (epilogue, multipart.Epilogue.Replace ("\r\n", "\n"), "The epilogue does not match");

				Assert.IsTrue (multipart.RawEpilogue[0] == (byte) '\r' || multipart.RawEpilogue[0] == (byte) '\n',
					"The RawEpilogue does not start with a new-line.");
			}
		}

		[Test]
		public void TestMissingSubtype ()
		{
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "missing-subtype.txt"))) {
				var message = MimeMessage.Load (stream);
				var type = message.Body.ContentType;

				Assert.AreEqual ("application", type.MediaType, "The media type is not the default.");
				Assert.AreEqual ("octet-stream", type.MediaSubtype, "The media subtype is not the default.");
				Assert.AreEqual ("document.xml.gz", type.Name, "The parameters do not seem to have been parsed.");
			}
		}

		[Test]
		public void TestIssue51 ()
		{
			const string text = "Date: Sat, 19 Apr 2014 13:13:23 -0700\r\n" +
				"From: Jeffrey Stedfast <notifications@github.com>\r\n" +
				"Subject: Re: [MimeKit] Allow parsing of message with 0 byte body. (#51)\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				try {
					MimeMessage.Load (stream);
				} catch {
					Assert.Fail ("A message with 0 bytes of content should not fail to parse.");
				}
			}
		}
	}
}
