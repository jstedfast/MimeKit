//
// MimeReaderTests.cs
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

using Newtonsoft.Json;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;

namespace UnitTests {
	[TestFixture]
	public class MimeReaderTests
	{
		static readonly string ComplianceDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "compliance");
		//static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "mbox");
		static FormatOptions UnixFormatOptions;

		public MimeReaderTests ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var reader = new MimeReader (Stream.Null);

			Assert.Throws<ArgumentNullException> (() => new MimeReader (null));
			Assert.Throws<ArgumentNullException> (() => new MimeReader (null, MimeFormat.Default));

			Assert.Throws<ArgumentNullException> (() => new MimeReader (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => new MimeReader (null, Stream.Null, MimeFormat.Default));

			Assert.Throws<ArgumentNullException> (() => new MimeReader (ParserOptions.Default, null));
			Assert.Throws<ArgumentNullException> (() => new MimeReader (ParserOptions.Default, null, MimeFormat.Default));

			Assert.Throws<ArgumentNullException> (() => reader.Options = null);
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

		enum MimeType
		{
			Message,
			MessagePart,
			Multipart,
			MimePart
		}

		class MimeItem
		{
			public readonly MimeOffsets Offsets;
			public readonly MimeType Type;

			public MimeItem (MimeType type, MimeOffsets offsets)
			{
				Offsets = offsets;
				Type = type;
			}
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

		class CustomMimeReader : ComplianceMimeReader
		{
			public readonly List<MimeOffsets> Offsets = new List<MimeOffsets> ();
			public readonly List<MimeItem> stack = new List<MimeItem> ();
			long mboxMarkerBeginOffset = -1;
			//int mboxMarkerLineNumber = -1;

			public CustomMimeReader (ParserOptions options, Stream stream, MimeFormat format = MimeFormat.Default) : base (options, stream, format)
			{
			}

			public CustomMimeReader (Stream stream, MimeFormat format = MimeFormat.Default) : base (stream, format)
			{
			}

			protected override void OnMboxMarkerRead (byte[] marker, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				mboxMarkerBeginOffset = beginOffset;
				//mboxMarkerLineNumber = lineNumber;

				base.OnMboxMarkerRead (marker, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				var offsets = new MimeOffsets {
					BeginOffset = beginOffset,
					LineNumber = beginLineNumber
				};

				if (stack.Count > 0) {
					var parent = stack[stack.Count - 1];
					Assert.That (parent.Type, Is.EqualTo (MimeType.MessagePart));
					parent.Offsets.Message = offsets;
				} else {
					offsets.MboxMarkerOffset = mboxMarkerBeginOffset;
					Offsets.Add (offsets);
				}

				stack.Add (new MimeItem (MimeType.Message, offsets));

				base.OnMimeMessageBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				var current = stack[stack.Count - 1];

				Assert.That (current.Type, Is.EqualTo (MimeType.Message));

				current.Offsets.Octets = endOffset - headersEndOffset;
				current.Offsets.HeadersEndOffset = headersEndOffset;
				current.Offsets.EndOffset = endOffset;

				stack.RemoveAt (stack.Count - 1);

				base.OnMimeMessageEnd (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			void Push (MimeType type, ContentType contentType, long beginOffset, int beginLineNumber)
			{
				var offsets = new MimeOffsets {
					MimeType = contentType.MimeType,
					BeginOffset = beginOffset,
					LineNumber = beginLineNumber
				};

				if (stack.Count > 0) {
					var parent = stack[stack.Count - 1];

					switch (parent.Type) {
					case MimeType.Message:
						parent.Offsets.Body = offsets;
						break;
					case MimeType.Multipart:
						parent.Offsets.Children ??= new List<MimeOffsets> ();
						parent.Offsets.Children.Add (offsets);
						break;
					default:
						Assert.Fail ();
						break;
					}
				} else {
					Offsets.Add (offsets);
				}

				stack.Add (new MimeItem (type, offsets));
			}

			void Pop (MimeType type, ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines)
			{
				var current = stack[stack.Count - 1];

				Assert.That (current.Type, Is.EqualTo (type));

				current.Offsets.Octets = endOffset - headersEndOffset;
				current.Offsets.HeadersEndOffset = headersEndOffset;
				current.Offsets.EndOffset = endOffset;
				current.Offsets.Lines = lines;

				stack.RemoveAt (stack.Count - 1);
			}

			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Push (MimeType.MessagePart, contentType, beginOffset, beginLineNumber);
				base.OnMessagePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Pop (MimeType.MessagePart, contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMessagePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Push (MimeType.MimePart, contentType, beginOffset, beginLineNumber);
				base.OnMimePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Pop (MimeType.MimePart, contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMimePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Push (MimeType.Multipart, contentType, beginOffset, beginLineNumber);
				base.OnMultipartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Pop (MimeType.Multipart, contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMultipartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}
		}

		static void AssertMboxResults (string baseName, List<MimeOffsets> offsets, NewLineFormat newLineFormat)
		{
			var path = Path.Combine (MboxDataDir, baseName + "." + newLineFormat.ToString ().ToLowerInvariant () + "-offsets.json");
			var jsonSerializer = JsonSerializer.CreateDefault ();

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
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var reader = options != null ? new CustomMimeReader (options, stream, MimeFormat.Mbox) : new CustomMimeReader (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

				while (!reader.IsEndOfStream) {
					reader.ReadMessage ();
				}

				offsets = reader.Offsets;
			}

			AssertMboxResults (baseName, offsets, newLineFormat);
		}

		static async Task TestMboxAsync (ParserOptions options, string baseName)
		{
			var mbox = Path.Combine (MboxDataDir, baseName + ".mbox.txt");
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var reader = options != null ? new CustomMimeReader (options, stream, MimeFormat.Mbox) : new CustomMimeReader (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

				while (!reader.IsEndOfStream) {
					await reader.ReadMessageAsync ();
				}

				offsets = reader.Offsets;
			}

			AssertMboxResults (baseName, offsets, newLineFormat);
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
		public void TestIssue1189Mbox ()
		{
			TestMbox (null, "issue1189");
		}

		[Test]
		public async Task TestIssue1189MboxAsync ()
		{
			await TestMboxAsync (null, "issue1189");
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
		public void TestLineCountSingleLine ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountSingleLineCRLF ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineCRLFAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountSingleLineInMultipart ()
		{
			string text = @"From: mimekit@example.org
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
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineInMultipartAsync ()
		{
			string text = @"From: mimekit@example.org
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
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountOneLineOfTextFollowedByBlankLineInMultipart ()
		{
			string text = @"From: mimekit@example.org
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
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountOneLineOfTextFollowedByBlankLineInMultipartAsync ()
		{
			string text = @"From: mimekit@example.org
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
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountNonTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.IncompleteHeader), "Violation");
				Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (1), "LineNumber");
			}
		}

		[Test]
		public async Task TestLineCountNonTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.IncompleteHeader), "Violation");
				Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (1), "LineNumber");
			}
		}

		[Test]
		public void TestLineCountProperlyTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.MissingBodySeparator), "Violation");
				Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (2), "LineNumber");
			}
		}

		[Test]
		public async Task TestLineCountProperlyTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (reader.ComplianceViolations[0].Violation, Is.EqualTo (MimeComplianceViolation.MissingBodySeparator), "Violation");
				Assert.That (reader.ComplianceViolations[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (reader.ComplianceViolations[0].LineNumber, Is.EqualTo (2), "LineNumber");
			}
		}

		static byte[] ReadAllBytes (string path)
		{
			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Dos2UnixFilter ());

					using (var memory = new MemoryStream ()) {
						filtered.CopyTo (memory);
						return memory.ToArray ();
					}
				}
			}
		}

		static void UpdateStreamOffsets (string path, MimeComplianceIssue[] issues, out int bareLineFeeds)
		{
			byte[] rawData = ReadAllBytes (path);
			long unixOffset = 0;
			long dosOffset = 0;
			int lineNumber = 1;
			int column = 1;

			bareLineFeeds = 0;

			for (int i = 0; i < rawData.Length; i++) {
				if (rawData[i] == (byte) '\n') {
					bareLineFeeds++;
					dosOffset += 2;
					unixOffset++;
					lineNumber++;
					column = 1;
				} else {
					unixOffset++;
					dosOffset++;
					column++;
				}

				foreach (var issue in issues) {
					if (issue.LineNumber == lineNumber && issue.Column == column) {
						issue.UnixOffset = unixOffset;
						issue.DosOffset = dosOffset;
					}
				}
			}
		}

		static void AssertMimeComplianceViolations (string fileName, MimeComplianceIssue[] issues)
		{
			var path = Path.Combine (ComplianceDataDir, fileName);
			var expectedCount = issues.Length;

			UpdateStreamOffsets (path, issues, out var bareLineFeeds);

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Dos2UnixFilter ());

					var reader = new ComplianceMimeReader (filtered);

					reader.ReadMessage ();

					Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (expectedCount + bareLineFeeds), "ComplianceViolations for Unix format");

					for (int i = 0, v = 0; i < issues.Length && v < reader.ComplianceViolations.Count; v++) {
						var actual = reader.ComplianceViolations[v];

						if (actual.Violation == MimeComplianceViolation.BareLinefeedInHeader ||
							actual.Violation == MimeComplianceViolation.BareLinefeedInBody)
							continue;

						var expected = issues[i++];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.UnixOffset), $"StreamOffset for issue #{i}");
					}
				}
			}

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var reader = new ComplianceMimeReader (filtered);

					reader.ReadMessage ();

					Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (expectedCount), "ComplianceViolations for DOS format");

					for (int i = 0; i < issues.Length; i++) {
						var actual = reader.ComplianceViolations[i];
						var expected = issues[i];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.DosOffset), $"StreamOffset for issue #{i}");
					}
				}
			}
		}

		static async Task AssertMimeComplianceViolationsAsync (string fileName, MimeComplianceIssue[] issues)
		{
			var path = Path.Combine (ComplianceDataDir, fileName);
			var expectedCount = issues.Length;

			UpdateStreamOffsets (path, issues, out var bareLineFeeds);

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Dos2UnixFilter ());

					var reader = new ComplianceMimeReader (filtered);

					reader.ReadMessage ();

					Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (expectedCount + bareLineFeeds), "ComplianceViolations for Unix format");

					for (int i = 0, v = 0; i < issues.Length && v < reader.ComplianceViolations.Count; v++) {
						var actual = reader.ComplianceViolations[v];

						if (actual.Violation == MimeComplianceViolation.BareLinefeedInHeader ||
							actual.Violation == MimeComplianceViolation.BareLinefeedInBody)
							continue;

						var expected = issues[i++];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.UnixOffset), $"StreamOffset for issue #{i}");
					}
				}
			}

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var reader = new ComplianceMimeReader (filtered);

					await reader.ReadMessageAsync ();

					Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (expectedCount), "ComplianceViolations for DOS format");

					for (int i = 0; i < issues.Length; i++) {
						var actual = reader.ComplianceViolations[i];
						var expected = issues[i];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.DosOffset), $"StreamOffset for issue #{i}");
					}
				}
			}
		}

		[Test]
		public void TestMimeComplianceInvalidHeaderFieldNameWithSpace ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 7, 1),
			};

			AssertMimeComplianceViolations ("invalid-header-field-with-space.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidHeaderFieldNameWithSpaceAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 7, 1),
			};

			return AssertMimeComplianceViolationsAsync ("invalid-header-field-with-space.eml", issues);
		}

		[Test]
		public void TestMimeComplianceIncompleteHeader ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IncompleteHeader, 6, 25)
			};

			AssertMimeComplianceViolations ("incomplete-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceIncompleteHeaderAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IncompleteHeader, 6, 25)
			};

			return AssertMimeComplianceViolationsAsync ("incomplete-header.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingBasic ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidContentTransferEncoding, 7, 1)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-basic.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingBasicAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidContentTransferEncoding, 7, 1)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-basic.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingMultipart ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 10, 1)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-multipart.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingMultipartAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 10, 1)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-multipart.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingRfc822 ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 7, 1)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-rfc822.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingRfc822Async ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 7, 1)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-rfc822.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentType ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidContentType, 6, 1)
			};

			AssertMimeComplianceViolations ("invalid-content-type.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTypeAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidContentType, 6, 1)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-type.eml", issues);
		}

#if CAN_DETECT_MIME_VERSION_ISSUES
		[Test]
		public void TestMimeComplianceInvalidMimeVersion ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a test message
Message-Id: <123@example.org>
MIME-Version: 1.x
Content-Type: text/plain; charset=us-ascii

This is the message body.
";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceStatus.InvalidMimeVersion, 5, 1)
			};

			AssertMimeComplianceIssues (text, issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidMimeVersionAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a test message
Message-Id: <123@example.org>
MIME-Version: 1.x
Content-Type: text/plain; charset=us-ascii

This is the message body.
";
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceStatus.InvalidMimeVersion, 5, 1)
			};

			return AssertMimeComplianceIssuesAsync (text, issues);
		}

				[Test]
		public void TestMimeComplianceMissingMimeVersion ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a test message
Message-Id: <123@example.org>
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is the message body.
--boundary-marker
Content-Type: message/rfc822
Content-Disposition: attachment; filename=""message1.eml""

From: mimekit@example.org
To: mimekit@example.org
Subject: This is the first inner test message
Message-Id: <123@example.org>
Content-Type: text/plain; charset=us-ascii

This is the first inner message body.
--boundary-marker
Content-Type: message/rfc822
Content-Disposition: attachment; filename=""message2.eml""

From: mimekit@example.org
To: mimekit@example.org
Subject: This is the second inner test message
Message-Id: <123@example.org>
Mime-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is the second inner message body.
--boundary-marker--
";
			var issues = new MimeComplianceIssue[] {
				// FIXME: MissingMimeVersion issues are reported with the offset/lineNumber of the start of the message. Should it use a different offset/lineNumber?
				new MimeComplianceIssue (MimeComplianceStatus.MissingMimeVersion, 1, 1),
				new MimeComplianceIssue (MimeComplianceStatus.MissingMimeVersion, 15, 1)
			};

			AssertMimeComplianceIssues (text, issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMimeVersionAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a test message
Message-Id: <123@example.org>
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is the message body.
--boundary-marker
Content-Type: message/rfc822
Content-Disposition: attachment; filename=""message1.eml""

From: mimekit@example.org
To: mimekit@example.org
Subject: This is the first inner test message
Message-Id: <123@example.org>
Content-Type: text/plain; charset=us-ascii

This is the first inner message body.
--boundary-marker
Content-Type: message/rfc822
Content-Disposition: attachment; filename=""message2.eml""

From: mimekit@example.org
To: mimekit@example.org
Subject: This is the second inner test message
Message-Id: <123@example.org>
Mime-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is the second inner message body.
--boundary-marker--
";
			var issues = new MimeComplianceIssue[] {
				// FIXME: MissingMimeVersion issues are reported with the offset/lineNumber of the start of the message. Should it use a different offset/lineNumber?
				new MimeComplianceIssue (MimeComplianceStatus.MissingMimeVersion, 1, 1),
				new MimeComplianceIssue (MimeComplianceStatus.MissingMimeVersion, 15, 1)
			};

			return AssertMimeComplianceIssuesAsync (text, issues);
		}
#endif

		[Test]
		public void TestMimeComplianceInvalidWrapping ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidWrapping, 7, 1),
				new MimeComplianceIssue (MimeComplianceViolation.InvalidWrapping, 10, 1)
			};

			AssertMimeComplianceViolations ("invalid-wrapping.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidWrappingAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.InvalidWrapping, 7, 1),
				new MimeComplianceIssue (MimeComplianceViolation.InvalidWrapping, 10, 1)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-wrapping.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingBodySeparator ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingBodySeparator, 7, 1),
			};

			AssertMimeComplianceViolations ("missing-body-separator.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingBodySeparatorAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingBodySeparator, 7, 1),
			};

			return AssertMimeComplianceViolationsAsync ("missing-body-separator.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartBoundaryParameter ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundaryParameter, 6, 1)
			};

			AssertMimeComplianceViolations ("missing-multipart-boundary-parameter.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartBoundaryParameterAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundaryParameter, 6, 1)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-boundary-parameter.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartBoundary ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 18, 1)
			};

			AssertMimeComplianceViolations ("missing-multipart-boundary.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartBoundaryAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 18, 1)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-boundary.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartEndBoundary ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 18, 1)
			};

			AssertMimeComplianceViolations ("missing-multipart-end-boundary.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartEndBoundaryAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 18, 1)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-end-boundary.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMultipleContentTransferEncodings ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MultipleContentTransferEncodings, 8, 1)
			};

			AssertMimeComplianceViolations ("multiple-content-transfer-encodings.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMultipleContentTransferEncodingsAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MultipleContentTransferEncodings, 8, 1)
			};

			return AssertMimeComplianceViolationsAsync ("multiple-content-transfer-encodings.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMultipleContentTypes ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MultipleContentTypes, 7, 1)
			};

			AssertMimeComplianceViolations ("multiple-content-types.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMultipleContentTypesAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.MultipleContentTypes, 7, 1)
			};

			return AssertMimeComplianceViolationsAsync ("multiple-content-types.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInBody ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 24, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-body.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInBodyAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 24, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-body.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInPreamble ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 11, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-preamble.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInPreambleAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 11, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-preamble.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInEpilogue ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 28, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-epilogue.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInEpilogueAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 28, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-epilogue.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpectedNullBytesInHeaders ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInHeader, 5, 1)
			};

			AssertMimeComplianceViolations ("unexpected-null-bytes-in-headers.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpectedNullBytesInHeadersAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInHeader, 5, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-null-bytes-in-headers.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpectedNullBytesInBody ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInBody, 17, 1),
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 18, 1)
			};

			AssertMimeComplianceViolations ("unexpected-null-bytes-in-body.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpectedNullBytesInBodyAsync ()
		{
			var issues = new MimeComplianceIssue[] {
				new MimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInBody, 17, 1),
				new MimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 18, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-null-bytes-in-body.eml", issues);
		}
	}
}
