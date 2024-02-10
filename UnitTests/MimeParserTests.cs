//
// MimeParserTests.cs
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
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeFormat.Default));

#pragma warning disable CS0618 // Type or member is obsolete
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, stream));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (ParserOptions.Default, null));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, stream, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (ParserOptions.Default, null, MimeFormat.Default));
#pragma warning restore CS0618 // Type or member is obsolete

				Assert.Throws<ArgumentNullException> (() => parser.Options = null);
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

					Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");

					value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");

					value = headers["Header-2"];

					Assert.That (value, Is.EqualTo ("value 2"), "Unexpected header value.");

					value = headers["Header-3"];

					Assert.That (value, Is.EqualTo ("value 3"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");

					value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");

					value = headers["Header-2"];

					Assert.That (value, Is.EqualTo ("value 2"), "Unexpected header value.");

					value = headers["Header-3"];

					Assert.That (value, Is.EqualTo ("value 3"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestTruncatedHeaderNameAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var headers = await HeaderList.LoadAsync (memory);
					Assert.Fail ("Parsing headers should fail.");
				} catch (FormatException) {
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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

					Assert.That (headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
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
				parser.SetStream (stream, MimeFormat.Mbox);
				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "MboxAsync");

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity);
				Assert.Throws<FormatException> (() => parser.ParseMessage (), "ParseMessage");

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity);
				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "ParseMessageAsync");

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity);
				Assert.Throws<FormatException> (() => parser.ParseEntity (), "ParseEntity");

				stream.Position = 0;
				parser.SetStream (stream, MimeFormat.Entity);
				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseEntityAsync (), "ParseEntityAsync");
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
				Assert.That (message.Headers.Count, Is.EqualTo (0));

				message = parser.ParseMessage ();
				Assert.That (message.Headers.Count, Is.EqualTo (3));
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
				Assert.That (message.Headers.Count, Is.EqualTo (0));

				message = await parser.ParseMessageAsync ();
				Assert.That (message.Headers.Count, Is.EqualTo (3));
			}
		}

		[Test]
		public void TestEmptyMessage ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var message = MimeMessage.Load (memory);

					Assert.That (message.Headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse message: {ex}");
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

					Assert.That (message.Headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse message: {ex}");
				}
			}
		}

		[Test]
		public void TestInvalidContentType ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of recovery from invalid media-type in Content-Type header
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: garbage; charset=utf-8

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestInvalidContentTypeAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of recovery from invalid media-type in Content-Type header
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: garbage; charset=utf-8

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestHeaderFieldNameBeginsWithColon ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a header line starting with ':'
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
: What header is this?

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestHeaderFieldNameBeginsWithColonAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a header line starting with ':'
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
: What header is this?

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMultipartBoundaryWithoutTrailingNewline ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart boundary w/o trailing newline
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMultipartBoundaryWithoutTrailingNewlineAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart boundary w/o trailing newline
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestTruncatedMultipartSubpartHeaders ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public async Task TestTruncatedMultipartSubpartHeadersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public void TestTruncatedMultipartSubpartHeaderFieldName ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart header field name
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
Content-Dis".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public async Task TestTruncatedMultipartSubpartHeaderFieldNameAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart header field name
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
Content-Dis".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public void TestMultipartSubpartHeadersEndWithBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public async Task TestMultipartSubpartHeadersEndWithBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));
			}
		}

		[Test]
		public void TestMultipartSubpartHeadersLineStartsWithDashDash ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\r\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMultipartSubpartHeadersLineStartsWithDashDashyAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\r\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMultipartBoundaryLineWithTrailingSpacesAndThenMoreCharacters ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace and then more characters
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it.
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMultipartBoundaryLineWithTrailingSpacesAndThenMoreCharactersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace and then more characters
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it.
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMultipartDoubleBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is technically the third part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMultipartDoubleBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is technically the third part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMultipartWithoutBoundaryParameter ()
		{
			string text = @"Content-Type: multipart/mixed

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");
			int dashes = text.IndexOf ("--");
			var preamble = text.Substring (dashes);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var multipart = (Multipart) parser.ParseEntity ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble), "Preamble");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var multipart = (Multipart) parser.ParseEntity ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble.Replace ("\n", "\r\n")), "Preamble");
			}
		}

		[Test]
		public async Task TestMultipartWithoutBoundaryParameterAsync ()
		{
			string text = @"Content-Type: multipart/mixed

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");
			int dashes = text.IndexOf ("--");
			var preamble = text.Substring (dashes);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var multipart = (Multipart) await parser.ParseEntityAsync ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble), "Preamble");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var multipart = (Multipart) await parser.ParseEntityAsync ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble.Replace ("\n", "\r\n")), "Preamble");
			}
		}

		[Test]
		public void TestTruncatedImmediatelyAfterMessageRfc822Headers ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part truncated immediately after the MIME headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}
		}

		[Test]
		public async Task TestTruncatedImmediatelyAfterMessageRfc822HeadersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part truncated immediately after the MIME headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}
		}

		[Test]
		public void TestMessageRfc822WithBoundaryBeforeMessage ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}
		}

		[Test]
		public async Task TestMessageRfc822WithBoundaryBeforeMessageAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");
			}
		}

		[Test]
		public void TestMessageRfc822WithFromMarkerBeforeMessage ()
		{
			string text = @"From -
From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

From -
From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the embedded message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMessageRfc822WithFromMarkerBeforeMessageAsync ()
		{
			string text = @"From -
From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

From -
From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the embedded message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMessageRfc822WithMungedFromMarkerBeforeMessage ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996
Return-Path: <blaker@craswell.com>
Received: from maleman.mcom.com ([198.93.92.3]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA19742
          for <jwz@dredd.mcom.com>; Fri, 13 Dec 1996 14:59:31 -0800
Received: from xwing.netscape.com (xwing.mcom.com [205.218.156.54]) by maleman.mcom.com (8.6.9/8.6.9) with ESMTP id OAA23726 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:58:13 -0800
Received: from peapod.deming.com (host20.deming.com [206.63.131.20]) by xwing.netscape.com (8.7.6/8.7.3) with SMTP id OAA00270 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:59:27 -0800 (PST)
Received: by peapod.deming.com from localhost
    (router,SLmail V2.0); Fri, 13 Dec 1996 15:01:48 Pacific Standard Time
Received: by peapod.deming.com from seth
    (206.63.131.30::mail daemon; unverified,SLmail V2.0); Fri, 13 Dec 1996 15:01:02 Pacific Standard Time
Message-Id: <3.0.32.19961213150855.009172e0@mail.craswell.com>
X-Sender: blaker@mail.craswell.com
X-Mailer: Windows Eudora Pro Version 3.0 (32)
Date: Fri, 13 Dec 1996 15:09:42 -0800
To: Jamie Zawinski <jwz@netscape.com>
From: ""Blake Ramsdell"" <blaker@craswell.com>
Subject: Re: can you send me an encrypted message?
MIME-Version: 1.0
Content-Type: application/x-pkcs7-mime; name=""smime.p7m""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""smime.p7m""

MIAGCSqGSIb3DQEHA6CAMIACAQAxgDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcw
FQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAt
IEluZGl2aWR1YWwgU3Vic2NyaWJlcgIQKQ/GF/RumodE+WtXiPJmhDANBgkqhkiG9w0BAQEF
AARAb0tthyav05ce7KBWdlfN1M0R6wLQ2FWPVQynuWo/yHUoo3hiII7j15FXNgnxF7QkY5/p
mZXg0P2eJ1iYQy1vZDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcwFQYDVQQKEw5W
ZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAtIEluZGl2aWR1
YWwgU3Vic2NyaWJlcgIQDOtpec1+JM3EpqAMVqgtjzANBgkqhkiG9w0BAQEFAARAuqnsnz1O
qEdx7NEMJDEdjccjdEuCM8x2euTYlU/GWNY+s2iKVahbT3/R8E8hp3YfrHd2sjvgy6teTOPO
ZI2SxwAAMIAGCSqGSIb3DQEHATAUBggqhkiG9w0DBwQIlhWqtbsElaWggASCAjBooYYTWSBz
7A4l0Aho7mK85zpMyAR0xTKqHXT0zL9XpHbKPAcETaBTh1n7e8aJeQ93ONGAs6tVVlA6bpUN
F3Q5O+ZuNXOMT83HIKRYEO1l8a+CH7XtUiQWtu/aBt12GQDX475WhPULKEJs7kLS2DwToRX/
ctwEPNwc6zfsOZoVTQ5HOwisvDZ2QGwa08Psj38SaQ0Y+ryk5FeiAtKQUZ0uuJWI/rRu64yj
KmVs1DDId18coftA2rv/u2/zABEX8u5ckEkwS7fO7UHv6XMCQ3kqgqIZZE1zIGohfUdtOYYo
M4eki3QDyovHPxEjBbnmpUw2xDN7/DdxYEZ4CteWurQ+VoP0PUM2qwi6EgM6MpVKg8KzOWdb
aV51a1oQKtpJJFZqZtFf9SQ4OW6NKXHsJ2AF8W4OQ+ySWQN43wMk8dGJYlPrREqn5RufPg3k
QM+s4VwTrS2TrU+ELZCYnJFfH+N7tE8ILrFMAteVxtqjat7OJRyDxy0cnBP+oG81Sr0zvbdC
jUPUDFlrPgFjDrswX1UpkEE2OgKWmfc134AbysJFOuCIze2XqKB96rJvxS76ygzVvrU/4sI1
6VDlZUEuUPaBUOimFxRk/rqPJDI1M8rNKykw9qsoWQMRnvrODfzo7iVWQ0TQHiwfoBhs6Dvm
UgrMwopFnzRdSHvT1acSqVfMYWm5nXImvtCuFAavkjDutE9+Y/LLFLBUpAVeu3rwW3wV0Tcv
9I6Afej0ntfbH9vlRwQIl7MeXMqoBV0AAAAAAAAAAAAA
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.DIsposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.DIsposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\r\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");
			}
		}

		[Test]
		public async Task TestMessageRfc822WithMungedFromMarkerBeforeMessageAsync ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996
Return-Path: <blaker@craswell.com>
Received: from maleman.mcom.com ([198.93.92.3]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA19742
          for <jwz@dredd.mcom.com>; Fri, 13 Dec 1996 14:59:31 -0800
Received: from xwing.netscape.com (xwing.mcom.com [205.218.156.54]) by maleman.mcom.com (8.6.9/8.6.9) with ESMTP id OAA23726 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:58:13 -0800
Received: from peapod.deming.com (host20.deming.com [206.63.131.20]) by xwing.netscape.com (8.7.6/8.7.3) with SMTP id OAA00270 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:59:27 -0800 (PST)
Received: by peapod.deming.com from localhost
    (router,SLmail V2.0); Fri, 13 Dec 1996 15:01:48 Pacific Standard Time
Received: by peapod.deming.com from seth
    (206.63.131.30::mail daemon; unverified,SLmail V2.0); Fri, 13 Dec 1996 15:01:02 Pacific Standard Time
Message-Id: <3.0.32.19961213150855.009172e0@mail.craswell.com>
X-Sender: blaker@mail.craswell.com
X-Mailer: Windows Eudora Pro Version 3.0 (32)
Date: Fri, 13 Dec 1996 15:09:42 -0800
To: Jamie Zawinski <jwz@netscape.com>
From: ""Blake Ramsdell"" <blaker@craswell.com>
Subject: Re: can you send me an encrypted message?
MIME-Version: 1.0
Content-Type: application/x-pkcs7-mime; name=""smime.p7m""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""smime.p7m""

MIAGCSqGSIb3DQEHA6CAMIACAQAxgDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcw
FQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAt
IEluZGl2aWR1YWwgU3Vic2NyaWJlcgIQKQ/GF/RumodE+WtXiPJmhDANBgkqhkiG9w0BAQEF
AARAb0tthyav05ce7KBWdlfN1M0R6wLQ2FWPVQynuWo/yHUoo3hiII7j15FXNgnxF7QkY5/p
mZXg0P2eJ1iYQy1vZDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcwFQYDVQQKEw5W
ZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAtIEluZGl2aWR1
YWwgU3Vic2NyaWJlcgIQDOtpec1+JM3EpqAMVqgtjzANBgkqhkiG9w0BAQEFAARAuqnsnz1O
qEdx7NEMJDEdjccjdEuCM8x2euTYlU/GWNY+s2iKVahbT3/R8E8hp3YfrHd2sjvgy6teTOPO
ZI2SxwAAMIAGCSqGSIb3DQEHATAUBggqhkiG9w0DBwQIlhWqtbsElaWggASCAjBooYYTWSBz
7A4l0Aho7mK85zpMyAR0xTKqHXT0zL9XpHbKPAcETaBTh1n7e8aJeQ93ONGAs6tVVlA6bpUN
F3Q5O+ZuNXOMT83HIKRYEO1l8a+CH7XtUiQWtu/aBt12GQDX475WhPULKEJs7kLS2DwToRX/
ctwEPNwc6zfsOZoVTQ5HOwisvDZ2QGwa08Psj38SaQ0Y+ryk5FeiAtKQUZ0uuJWI/rRu64yj
KmVs1DDId18coftA2rv/u2/zABEX8u5ckEkwS7fO7UHv6XMCQ3kqgqIZZE1zIGohfUdtOYYo
M4eki3QDyovHPxEjBbnmpUw2xDN7/DdxYEZ4CteWurQ+VoP0PUM2qwi6EgM6MpVKg8KzOWdb
aV51a1oQKtpJJFZqZtFf9SQ4OW6NKXHsJ2AF8W4OQ+ySWQN43wMk8dGJYlPrREqn5RufPg3k
QM+s4VwTrS2TrU+ELZCYnJFfH+N7tE8ILrFMAteVxtqjat7OJRyDxy0cnBP+oG81Sr0zvbdC
jUPUDFlrPgFjDrswX1UpkEE2OgKWmfc134AbysJFOuCIze2XqKB96rJvxS76ygzVvrU/4sI1
6VDlZUEuUPaBUOimFxRk/rqPJDI1M8rNKykw9qsoWQMRnvrODfzo7iVWQ0TQHiwfoBhs6Dvm
UgrMwopFnzRdSHvT1acSqVfMYWm5nXImvtCuFAavkjDutE9+Y/LLFLBUpAVeu3rwW3wV0Tcv
9I6Afej0ntfbH9vlRwQIl7MeXMqoBV0AAAAAAAAAAAAA
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.DIsposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.DIsposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\r\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");
			}
		}

		[Test]
		public void TestMessageRfc822 ()
		{
			string text = @"Content-Type: message/rfc822

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMessageRfc822Async ()
		{
			string text = @"Content-Type: message/rfc822

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
			}
		}

		[Test]
		public void TestMimePartBasic ()
		{
			string text = @"Content-Type: application/octet-stream; name=rawData.dat
Content-Disposition: inline; filename=rawData.dat
Content-Transfer-Encoding: quoted-printable

This is some raw data.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));
			}
		}

		[Test]
		public async Task TestMimePartBasicAsync ()
		{
			string text = @"Content-Type: application/octet-stream; name=rawData.dat
Content-Disposition: inline; filename=rawData.dat
Content-Transfer-Encoding: quoted-printable

This is some raw data.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));
			}
		}

		static void AssertSimpleMbox (Stream stream)
		{
			var parser = new MimeParser (stream, MimeFormat.Mbox);

			while (!parser.IsEndOfStream) {
				var message = parser.ParseMessage ();
				Multipart multipart;
				MimeEntity entity;

				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				using (var memory = new MemoryStream ()) {
					entity.WriteTo (UnixFormatOptions, memory);

					var text = Encoding.ASCII.GetString (memory.ToArray ());
					Assert.That (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), Is.True, "Headers are not properly terminated.");
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

				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				using (var memory = new MemoryStream ()) {
					await entity.WriteToAsync (UnixFormatOptions, memory);

					var text = Encoding.ASCII.GetString (memory.ToArray ());
					Assert.That (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), Is.True, "Headers are not properly terminated.");
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

			if (entity is Multipart multipart) {
				foreach (var part in multipart)
					DumpMimeTree (builder, part, depth + 1);
			} else if (entity is MessagePart rfc822) {
				DumpMimeTree (builder, rfc822.Message.Body, depth + 1);
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

				Assert.That (builder.ToString (), Is.EqualTo (expected), "Unexpected MIME tree structure.");
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
			Assert.That (actual.MimeType, Is.EqualTo (expected.MimeType), $"mime-type differs for message #{message}{partSpecifier}");
			Assert.That (actual.MboxMarkerOffset, Is.EqualTo (expected.MboxMarkerOffset), $"mbox marker begin offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.BeginOffset, Is.EqualTo (expected.BeginOffset), $"begin offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"begin line differs for message #{message}{partSpecifier}");
			Assert.That (actual.HeadersEndOffset, Is.EqualTo (expected.HeadersEndOffset), $"headers end offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.EndOffset, Is.EqualTo (expected.EndOffset), $"end offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.Octets, Is.EqualTo (expected.Octets), $"octets differs for message #{message}{partSpecifier}");
			Assert.That (actual.Lines, Is.EqualTo (expected.Lines), $"lines differs for message #{message}{partSpecifier}");

			if (expected.Message != null) {
				Assert.That (actual.Message, Is.Not.Null, $"message content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Message, actual.Message, message, partSpecifier + "/message");
			} else if (expected.Body != null) {
				Assert.That (actual.Body, Is.Not.Null, $"body content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Body, actual.Body, message, partSpecifier + "/0");
			} else if (expected.Children != null) {
				Assert.That (actual.Children.Count, Is.EqualTo (expected.Children.Count), $"children count differs for message #{message}{partSpecifier}");
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
					parentOffsets.Children ??= new List<MimeOffsets> ();
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

			Assert.That (actual, Is.EqualTo (summary), $"Summaries do not match for {baseName}.mbox");

			using (var original = File.OpenRead (Path.Combine (MboxDataDir, baseName + ".mbox.txt"))) {
				int lineNumber = 1, columnNumber = 1;

				output.Position = 0;

				Assert.That (output.Length, Is.EqualTo (original.Length), "The length of the mbox did not match.");

				do {
					nx = original.Read (expected, 0, expected.Length);
					n = output.Read (buffer, 0, nx);

					if (nx == 0)
						break;

					for (int i = 0; i < nx; i++) {
						if (buffer[i] == expected[i]) {
							if (expected[i] == (byte) '\n') {
								columnNumber = 1;
								lineNumber++;
							} else {
								columnNumber++;
							}
							continue;
						}

						var strExpected = CharsetUtils.Latin1.GetString (expected, 0, nx);
						var strActual = CharsetUtils.Latin1.GetString (buffer, 0, n);

						Assert.That (strActual, Is.EqualTo (strExpected), $"The mbox differs at on line {lineNumber}, column {columnNumber}");
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

				Assert.That (offsets.Count, Is.EqualTo (expectedOffsets.Count), "message count");

				for (int i = 0; i < expectedOffsets.Count; i++)
					AssertMimeOffsets (expectedOffsets[i], offsets[i], i, string.Empty);
			}
		}

		static void TestMbox (ParserOptions options, string baseName)
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

		static async Task TestMboxAsync (ParserOptions options, string baseName)
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

			Assert.That (actual, Is.EqualTo (summary), "Summaries do not match for jwz.mbox");
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

			Assert.That (actual, Is.EqualTo (summary), "Summaries do not match for jwz.mbox");
		}

		[Test]
		public void TestJapaneseMessage ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var message = MimeMessage.Load (stream);

				Assert.That (message.Subject, Is.EqualTo (subject), "Subject values do not match");
				Assert.That (message.TextBody.Replace ("\r\n", "\n"), Is.EqualTo (body), "Message text does not match.");
			}
		}

		[Test]
		public async Task TestJapaneseMessageAsync ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var message = await MimeMessage.LoadAsync (stream);

				Assert.That (message.Subject, Is.EqualTo (subject), "Subject values do not match");
				Assert.That (message.TextBody.Replace ("\r\n", "\n"), Is.EqualTo (body), "Message text does not match.");
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
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From -"), $"Message #{count}");
					} else {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From Russia with love"), $"Message #{count}");
					}

					count++;
				}
			}

			Assert.That (count, Is.EqualTo (4), "Expected to find 4 messages.");
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
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From -"), $"Message #{count}");
					} else {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From Russia with love"), $"Message #{count}");
					}

					count++;
				}
			}

			Assert.That (count, Is.EqualTo (4), "Expected to find 4 messages.");
		}

		[Test]
		public void TestMultipartEpilogueWithText ()
		{
			const string epilogue = "Peter Urka <pcu@umich.edu>\nDept. of Chemistry, Univ. of Michigan\nNewt-thought is right-thought.  Go Newt!\n\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "epilogue.txt"))) {
				var message = MimeMessage.Load (stream);
				var multipart = message.Body as Multipart;

				Assert.That (multipart.Epilogue.Replace ("\r\n", "\n"), Is.EqualTo (epilogue), "The epilogue does not match");

				Assert.That (multipart.RawEpilogue[0] == (byte) '\r' || multipart.RawEpilogue[0] == (byte) '\n', Is.True,
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

				Assert.That (multipart.Epilogue.Replace ("\r\n", "\n"), Is.EqualTo (epilogue), "The epilogue does not match");

				Assert.That (multipart.RawEpilogue[0] == (byte) '\r' || multipart.RawEpilogue[0] == (byte) '\n', Is.True,
				               "The RawEpilogue does not start with a new-line.");
			}
		}

		[Test]
		public void TestMissingSubtype ()
		{
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "missing-subtype.txt"))) {
				var message = MimeMessage.Load (stream);
				var type = message.Body.ContentType;

				Assert.That (type.MediaType, Is.EqualTo ("application"), "The media type is not the default.");
				Assert.That (type.MediaSubtype, Is.EqualTo ("octet-stream"), "The media subtype is not the default.");
				Assert.That (type.Name, Is.EqualTo ("document.xml.gz"), "The parameters do not seem to have been parsed.");
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
					Assert.That (message.Body, Is.InstanceOf<MultipartAlternative> ());
				}
			}
		}

		[Test]
		public async Task TestIssue358Async ()
		{
			// Note: This particular message has a badly folded header value for "x-microsoft-exchange-diagnostics:"
			// which was causing MimeParser.StepHeaders[Async]() to abort because ReadAhead() already had more than
			// ReadAheadSize bytes buffered, so it assumed it had reached EOF when in fact it had not.
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "issue358.txt"))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var message = await MimeMessage.LoadAsync (filtered);

					// make sure that the top-level MIME part is a multipart/alternative
					Assert.That (message.Body, Is.InstanceOf<MultipartAlternative> ());
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

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineAsync ()
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
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
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

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineCRLFAsync ()
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
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
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

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineInMultipartAsync ()
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
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
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

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountOneLineOfTextFollowedByBlankLineInMultipartAsync ()
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
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
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

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountNonTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
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

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountProperlyTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}
	}
}
