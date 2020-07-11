//
// MimeParserTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

using Newtonsoft.Json;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace UnitTests {
	[TestFixture]
	public class MimeParserTests
	{
		static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "mbox");
		static FormatOptions UnixFormatOptions;

		public MimeParserTests ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			using (var stream = new MemoryStream ()) {
				var parser = new MimeParser (stream);

				Assert.Throws<ArgumentNullException> (() => new MimeParser (null));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, stream));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (ParserOptions.Default, null));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, stream, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (ParserOptions.Default, null, MimeFormat.Default));

				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, stream));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (ParserOptions.Default, null));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, stream, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (ParserOptions.Default, null, MimeFormat.Default));
			}
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
		public async Task TestHeaderParserAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\nHeader-2: value 2\r\nHeader-3: value 3\r\n\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = await HeaderList.LoadAsync (memory);
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
		public void TestTruncatedHeaderName ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = HeaderList.Load (memory);
					Assert.Fail ("Parsing headers should fail.");
				} catch (FormatException) {
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse headers: {0}", ex);
				}
			}
		}

		[Test]
		public void TestTruncatedHeader ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1");

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
		public async Task TestTruncatedHeaderAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = await HeaderList.LoadAsync (memory);

					Assert.AreEqual (1, headers.Count, "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.AreEqual ("value 1", value, "Unexpected header value.");
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
		public async Task TestSingleHeaderNoTerminatorAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = await HeaderList.LoadAsync (memory);

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
		public async Task TestEmptyHeadersAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = await HeaderList.LoadAsync (memory);

					Assert.AreEqual (0, headers.Count, "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse headers: {0}", ex);
				}
			}
		}

		[Test]
		public void TestPartialByteOrderMarkEOF ()
		{
			var bom = new byte[] { 0xEF, 0xBB/*, 0xBF */ };

			using (var stream = new MemoryStream (bom, false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "ParseMessage");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "ParseMessageAsync");
			}
		}

		[Test]
		public void TestPartialByteOrderMark ()
		{
			var bom = new byte[] { 0xEF, 0xBB/*, 0xBF */ };

			using (var stream = new MemoryStream ()) {
				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "ParseMessage");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "ParseMessageAsync");
			}
		}

		[Test]
		public void TestParsingGarbage ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Mbox);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "Mbox");

				stream.Position = 0;

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "MboxAsync");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "Entity");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "EntityAsync");
			}
		}

		[Test]
		public void TestDoubleMboxMarker ()
		{
			var content = Encoding.ASCII.GetBytes ("From - \r\nFrom -\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");

			using (var stream = new MemoryStream (content, false)) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);
				MimeMessage message;

				message = parser.ParseMessage ();
				Assert.AreEqual (0, message.Headers.Count);

				message = parser.ParseMessage ();
				Assert.AreEqual (3, message.Headers.Count);
			}
		}

		[Test]
		public async Task TestDoubleMboxMarkerAsync ()
		{
			var content = Encoding.ASCII.GetBytes ("From - \r\nFrom -\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");

			using (var stream = new MemoryStream (content, false)) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);
				MimeMessage message;

				message = await parser.ParseMessageAsync ();
				Assert.AreEqual (0, message.Headers.Count);

				message = await parser.ParseMessageAsync ();
				Assert.AreEqual (3, message.Headers.Count);
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
		public async Task TestEmptyMessageAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var message = await MimeMessage.LoadAsync (memory);

					Assert.AreEqual (0, message.Headers.Count, "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ("Failed to parse message: {0}", ex);
				}
			}
		}

		static void AssertSimpleMbox (Stream stream)
		{
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

		[Test]
		public void TestSimpleMbox ()
		{
			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
				AssertSimpleMbox (stream);
		}

		static async Task AssertSimpleMboxAsync (Stream stream)
		{
			var parser = new MimeParser (stream, MimeFormat.Mbox);

			while (!parser.IsEndOfStream) {
				var message = await parser.ParseMessageAsync ();
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
					await entity.WriteToAsync (UnixFormatOptions, memory);

					var text = Encoding.ASCII.GetString (memory.ToArray ());
					Assert.IsTrue (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), "Headers are not properly terminated.");
				}
			}
		}

		[Test]
		public async Task TestSimpleMboxAsync ()
		{
			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
				await AssertSimpleMboxAsync (stream);
		}

		[Test]
		public void TestSimpleMboxWithByteOrderMark ()
		{
			using (var stream = new MemoryStream ()) {
				var bom = new byte[] { 0xEF, 0xBB, 0xBF };

				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				AssertSimpleMbox (stream);
			}
		}

		[Test]
		public async Task TestSimpleMboxWithByteOrderMarkAsync ()
		{
			using (var stream = new MemoryStream ()) {
				var bom = new byte[] { 0xEF, 0xBB, 0xBF };

				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				await AssertSimpleMboxAsync (stream);
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

		static NewLineFormat DetectNewLineFormat (string fileName)
		{
			using (var stream = File.OpenRead (fileName)) {
				var buffer = new byte[1024];

				var nread = stream.Read (buffer, 0, buffer.Length);

				for (int i = 0; i < nread; i++) {
					if (buffer[i] == (byte) '\n') {
						if (i > 0 && buffer[i - 1] == (byte) '\r')
							return NewLineFormat.Dos;

						return NewLineFormat.Unix;
					}
				}
			}

			return NewLineFormat.Dos;
		}

		class MimeOffsets
		{
			[JsonProperty ("mimeType", NullValueHandling = NullValueHandling.Ignore)]
			public string MimeType { get; set; }

			[JsonProperty ("mboxMarkerOffset", NullValueHandling = NullValueHandling.Ignore)]
			public long? MboxMarkerOffset { get; set; }

			[JsonProperty ("lineNumber")]
			public int LineNumber { get; set; }

			[JsonProperty ("beginOffset")]
			public long BeginOffset { get; set; }

			[JsonProperty ("headersEndOffset")]
			public long HeadersEndOffset { get; set; }

			[JsonProperty ("endOffset")]
			public long EndOffset { get; set; }

			[JsonProperty ("message", NullValueHandling = NullValueHandling.Ignore)]
			public MimeOffsets Message { get; set; }

			[JsonProperty ("body", NullValueHandling = NullValueHandling.Ignore)]
			public MimeOffsets Body { get; set; }

			[JsonProperty ("children", NullValueHandling = NullValueHandling.Ignore)]
			public List<MimeOffsets> Children { get; set; }

			[JsonProperty ("octets")]
			public long Octets { get; set; }

			[JsonProperty ("lines", NullValueHandling = NullValueHandling.Ignore)]
			public int? Lines { get; set; }
		}

		static void AssertMimeOffsets (MimeOffsets expected, MimeOffsets actual, int message, string partSpecifier)
		{
			Assert.AreEqual (expected.MimeType, actual.MimeType, $"mime-type differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.MboxMarkerOffset, actual.MboxMarkerOffset, $"mbox marker begin offset differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.BeginOffset, actual.BeginOffset, $"begin offset differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.LineNumber, actual.LineNumber, $"begin line differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.HeadersEndOffset, actual.HeadersEndOffset, $"headers end offset differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.EndOffset, actual.EndOffset, $"end offset differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.Octets, actual.Octets, $"octets differs for message #{message}{partSpecifier}");
			Assert.AreEqual (expected.Lines, actual.Lines, $"lines differs for message #{message}{partSpecifier}");

			if (expected.Message != null) {
				Assert.NotNull (actual.Message, $"message content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Message, actual.Message, message, partSpecifier + "/message");
			} else if (expected.Body != null) {
				Assert.NotNull (actual.Body, $"body content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Body, actual.Body, message, partSpecifier + "/0");
			} else if (expected.Children != null) {
				Assert.AreEqual (expected.Children.Count, actual.Children.Count, $"children count differs for message #{message}{partSpecifier}");
				for (int i = 0; i < expected.Children.Count; i++)
					AssertMimeOffsets (expected.Children[i], actual.Children[i], message, partSpecifier + $".{i}");
			}
		}

		class CustomMimeParser : MimeParser
		{
			readonly Dictionary<MimeMessage, MimeOffsets> messages = new Dictionary<MimeMessage, MimeOffsets> ();
			readonly Dictionary<MimeEntity, MimeOffsets> entities = new Dictionary<MimeEntity, MimeOffsets> ();
			public readonly List<MimeOffsets> Offsets = new List<MimeOffsets> ();
			MimeOffsets body;

			public CustomMimeParser (ParserOptions options, Stream stream, MimeFormat format) : base (options, stream, format)
			{
			}

			public CustomMimeParser (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			protected override void OnMimeMessageBegin (MimeMessageBeginEventArgs args)
			{
				var offsets = new MimeOffsets {
					BeginOffset = args.BeginOffset,
					LineNumber = args.LineNumber
				};

				if (args.Parent != null) {
					if (entities.TryGetValue (args.Parent, out var parentOffsets))
						parentOffsets.Message = offsets;
					else
						Console.WriteLine ("oops?");
				} else {
					offsets.MboxMarkerOffset = MboxMarkerOffset;
					Offsets.Add (offsets);
				}

				messages.Add (args.Message, offsets);

				base.OnMimeMessageBegin (args);
			}

			protected override void OnMimeMessageEnd (MimeMessageEndEventArgs args)
			{
				if (messages.TryGetValue (args.Message, out var offsets)) {
					offsets.Octets = args.EndOffset - args.HeadersEndOffset;
					offsets.HeadersEndOffset = args.HeadersEndOffset;
					offsets.EndOffset = args.EndOffset;
					offsets.Body = body;
				} else {
					Console.WriteLine ("oops?");
				}

				messages.Remove (args.Message);

				base.OnMimeMessageEnd (args);
			}

			protected override void OnMimeEntityBegin (MimeEntityBeginEventArgs args)
			{
				var offsets = new MimeOffsets {
					MimeType = args.Entity.ContentType.MimeType,
					BeginOffset = args.BeginOffset,
					LineNumber = args.LineNumber
				};

				if (args.Parent != null && entities.TryGetValue (args.Parent, out var parentOffsets)) {
					if (parentOffsets.Children == null)
						parentOffsets.Children = new List<MimeOffsets> ();

					parentOffsets.Children.Add (offsets);
				}

				entities.Add (args.Entity, offsets);

				base.OnMimeEntityBegin (args);
			}

			protected override void OnMimeEntityEnd (MimeEntityEndEventArgs args)
			{
				if (entities.TryGetValue (args.Entity, out var offsets)) {
					offsets.Octets = args.EndOffset - args.HeadersEndOffset;
					offsets.HeadersEndOffset = args.HeadersEndOffset;
					offsets.EndOffset = args.EndOffset;
					offsets.Lines = args.Lines;
					body = offsets;
				} else {
					Console.WriteLine ("oops?");
				}

				entities.Remove (args.Entity);

				base.OnMimeEntityEnd (args);
			}
		}

		static void AssertMboxResults (string baseName, string actual, Stream output, List<MimeOffsets> offsets, NewLineFormat newLineFormat)
		{
			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			var path = Path.Combine (MboxDataDir, baseName + "-summary.txt");
			if (!File.Exists (path))
				File.WriteAllText (path, actual);

			var summary = File.ReadAllText (path).Replace ("\r\n", "\n");
			var expected = new byte[4096];
			var buffer = new byte[4096];
			int nx, n;

			Assert.AreEqual (summary, actual, "Summaries do not match for {0}.mbox", baseName);

			using (var original = File.OpenRead (Path.Combine (MboxDataDir, baseName + ".mbox.txt"))) {
				output.Position = 0;

				Assert.AreEqual (original.Length, output.Length, "The length of the mbox did not match.");

				do {
					var position = original.Position;

					nx = original.Read (expected, 0, expected.Length);
					n = output.Read (buffer, 0, nx);

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

			var jsonSerializer = JsonSerializer.CreateDefault ();

			path = Path.Combine (MboxDataDir, baseName + "." + newLineFormat.ToString ().ToLowerInvariant () + "-offsets.json");
			if (!File.Exists (path)) {
				jsonSerializer.Formatting = Formatting.Indented;

				using (var writer = new StreamWriter (path))
					jsonSerializer.Serialize (writer, offsets);
			}

			using (var reader = new StreamReader (path)) {
				var expectedOffsets = (List<MimeOffsets>) jsonSerializer.Deserialize (reader, typeof (List<MimeOffsets>));

				Assert.AreEqual (expectedOffsets.Count, offsets.Count, "message count");

				for (int i = 0; i < expectedOffsets.Count; i++)
					AssertMimeOffsets (expectedOffsets[i], offsets[i], i, string.Empty);
			}
		}

		void TestMbox (ParserOptions options, string baseName)
		{
			var mbox = Path.Combine (MboxDataDir, baseName + ".mbox.txt");
			var output = new MemoryBlockStream ();
			var builder = new StringBuilder ();
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var parser = options != null ? new CustomMimeParser (options, stream, MimeFormat.Mbox) : new CustomMimeParser (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();
				int count = 0;

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

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

					var marker = Encoding.UTF8.GetBytes ((count > 0 ? format.NewLine : string.Empty) + parser.MboxMarker + format.NewLine);
					output.Write (marker, 0, marker.Length);
					message.WriteTo (format, output);
					count++;
				}

				offsets = parser.Offsets;
			}

			AssertMboxResults (baseName, builder.ToString (), output, offsets, newLineFormat);
		}

		async Task TestMboxAsync (ParserOptions options, string baseName)
		{
			var mbox = Path.Combine (MboxDataDir, baseName + ".mbox.txt");
			var output = new MemoryBlockStream ();
			var builder = new StringBuilder ();
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var parser = options != null ? new CustomMimeParser (options, stream, MimeFormat.Mbox) : new CustomMimeParser (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();
				int count = 0;

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

				while (!parser.IsEndOfStream) {
					var message = await parser.ParseMessageAsync ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					var marker = Encoding.UTF8.GetBytes ((count > 0 ? format.NewLine : string.Empty) + parser.MboxMarker + format.NewLine);
					await output.WriteAsync (marker, 0, marker.Length);
					await message.WriteToAsync (format, output);
					count++;
				}

				offsets = parser.Offsets;
			}

			AssertMboxResults (baseName, builder.ToString (), output, offsets, newLineFormat);
		}

		[Test]
		public void TestContentLengthMbox ()
		{
			var options = ParserOptions.Default.Clone ();
			options.RespectContentLength = true;

			TestMbox (options, "content-length");
		}

		[Test]
		public async Task TestContentLengthMboxAsync ()
		{
			var options = ParserOptions.Default.Clone ();
			options.RespectContentLength = true;

			await TestMboxAsync (options, "content-length");
		}

		[Test]
		public void TestJwzMbox ()
		{
			TestMbox (null, "jwz");
		}

		[Test]
		public async Task TestJwzMboxAsync ()
		{
			await TestMboxAsync (null, "jwz");
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
		public async Task TestJwzPersistentMboxAsync ()
		{
			var summary = File.ReadAllText (Path.Combine (MboxDataDir, "jwz-summary.txt")).Replace ("\r\n", "\n");
			var builder = new StringBuilder ();

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox, true);

				while (!parser.IsEndOfStream) {
					var message = await parser.ParseMessageAsync ();

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
					await message.WriteToAsync (Stream.Null);
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
		public async Task TestJapaneseMessageAsync ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var message = await MimeMessage.LoadAsync (stream);

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
		public async Task TestUnmungedFromLinesAsync ()
		{
			int count = 0;

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "unmunged.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					await parser.ParseMessageAsync ();

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
		public async Task TestMultipartEpilogueWithTextAsync ()
		{
			const string epilogue = "Peter Urka <pcu@umich.edu>\nDept. of Chemistry, Univ. of Michigan\nNewt-thought is right-thought.  Go Newt!\n\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "epilogue.txt"))) {
				var message = await MimeMessage.LoadAsync (stream);
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
		public void TestMissingMessageBody ()
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

		[Test]
		public async Task TestMissingMessageBodyAsync ()
		{
			const string text = "Date: Sat, 19 Apr 2014 13:13:23 -0700\r\n" +
				"From: Jeffrey Stedfast <notifications@github.com>\r\n" +
				"Subject: Re: [MimeKit] Allow parsing of message with 0 byte body. (#51)\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				try {
					await MimeMessage.LoadAsync (stream);
				} catch {
					Assert.Fail ("A message with 0 bytes of content should not fail to parse.");
				}
			}
		}

		[Test]
		public void TestIssue358 ()
		{
			// Note: This particular message has a badly folded header value for "x-microsoft-exchange-diagnostics:"
			// which was causing MimeParser.StepHeaders[Async]() to abort because ReadAhead() already had more than
			// ReadAheadSize bytes buffered, so it assumed it had reached EOF when in fact it had not.
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "issue358.txt"))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var message = MimeMessage.Load (filtered);

					// make sure that the top-level MIME part is a multipart/alternative
					Assert.IsInstanceOf (typeof (MultipartAlternative), message.Body);
				}
			}
		}

		[Test]
		public void TestLineCountSingleLine ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.AreEqual (1, lines, "Line count");
			}
		}

		[Test]
		public void TestLineCountSingleLineCRLF ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.AreEqual (1, lines, "Line count");
			}
		}

		[Test]
		public void TestLineCountSingleLineInMultipart ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text
--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-DIsposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.AreEqual (1, lines, "Line count");
			}
		}

		[Test]
		public void TestLineCountOneLineOfTextFollowedByBlankLineInMultipart ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text followed by a blank line

--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-DIsposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.AreEqual (1, lines, "Line count");
			}
		}

		[Test]
		public void TestLineCountNonTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.AreEqual (0, lines, "Line count");
			}
		}

		[Test]
		public void TestLineCountProperlyTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.AreEqual (0, lines, "Line count");
			}
		}
	}
}
