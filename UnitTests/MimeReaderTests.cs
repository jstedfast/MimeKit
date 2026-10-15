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

using UnitTests.IO;

namespace UnitTests {
	[TestFixture]
	public class MimeReaderTests
	{
		static readonly string ComplianceDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "compliance");
		//static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "mbox");
		static readonly FormatOptions UnixFormatOptions;

		static MimeReaderTests ()
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
			Assert.Throws<ArgumentNullException> (() => reader.ComplianceOptions = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => reader.ComplianceOptions.MaxIssuesPerViolation = -1);
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

		class CustomMimeReader : MimeReader
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

			protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				mboxMarkerBeginOffset = beginOffset;
				//mboxMarkerLineNumber = lineNumber;

				base.OnMboxMarkerBegin (beginOffset, lineNumber, cancellationToken);
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

		sealed class ContentRegion
		{
			public string Kind;
			public long BeginOffset;
			public long EndOffset;
			public byte[] Content;
		}

		// Records the bytes passed to the content/preamble/epilogue Read callbacks so that they can be compared
		// against the stream offsets reported by the corresponding End callbacks.
		class ContentRecordingReader : MimeReader
		{
			public readonly List<ContentRegion> Regions = new List<ContentRegion> ();
			readonly MemoryStream content = new MemoryStream ();

			public ContentRecordingReader (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			void Begin ()
			{
				content.SetLength (0);
			}

			void End (string kind, long beginOffset, long endOffset)
			{
				Regions.Add (new ContentRegion { Kind = kind, BeginOffset = beginOffset, EndOffset = endOffset, Content = content.ToArray () });
			}

			protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Begin ();
			protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => content.Write (buffer, startIndex, count);
			protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken) => End ("content", beginOffset, endOffset);

			protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Begin ();
			protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => content.Write (buffer, startIndex, count);
			protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken) => End ("preamble", beginOffset, endOffset);

			protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Begin ();
			protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => content.Write (buffer, startIndex, count);
			protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken) => End ("epilogue", beginOffset, endOffset);
		}

		[Test]
		public void TestNextGrowSize ()
		{
			// Grows geometrically (by 1.5x) so that appending to the header buffer is amortized O(n).
			Assert.That (MimeReader.NextGrowSize (512, 513), Is.EqualTo (768));
			Assert.That (MimeReader.NextGrowSize (4096, 4097), Is.EqualTo (6144));

			// Never smaller than what is needed.
			Assert.That (MimeReader.NextGrowSize (512, 5000), Is.EqualTo (5000));
			Assert.That (MimeReader.NextGrowSize (0, 1), Is.EqualTo (1));
			Assert.That (MimeReader.NextGrowSize (1, 2), Is.EqualTo (2));

			// Clamped to the maximum array length without overflowing, but never smaller than what is needed.
			Assert.That (MimeReader.NextGrowSize (0x60000000, 0x60000001), Is.EqualTo (MimeReader.MaxArrayLength));
			Assert.That (MimeReader.NextGrowSize (0x7FFFFF00, 0x7FFFFFF0), Is.EqualTo (0x7FFFFFF0));
		}

		static void AssertContentRegions (byte[] source, List<ContentRegion> regions, int expectedCount, string label)
		{
			if (expectedCount >= 0)
				Assert.That (regions, Has.Count.EqualTo (expectedCount), $"{label}: region count");
			else
				Assert.That (regions, Is.Not.Empty, $"{label}: region count");

			for (int i = 0; i < regions.Count; i++) {
				var region = regions[i];
				var expected = new byte[region.EndOffset - region.BeginOffset];

				Array.Copy (source, region.BeginOffset, expected, 0, expected.Length);

				Assert.That (Encoding.Latin1.GetString (region.Content), Is.EqualTo (Encoding.Latin1.GetString (expected)), $"{label}: {region.Kind} region #{i} @ {region.BeginOffset}");
			}
		}

		static readonly string LongLine = new string ('x', 1500);

		static readonly string[] ContentCallbackMessages = {
			// multipart with a preamble, an empty part, a part that ends with a blank line and an epilogue followed by EOS
			"From: mimekit@example.org\r\nContent-Type: multipart/mixed; boundary=\"b\"\r\n\r\npreamble\r\n--b\r\n\r\n--b\r\nContent-Type: text/plain\r\n\r\nline 1\r\n\r\n--b\r\nContent-Type: text/plain\r\n\r\nline 2\r\n--b--\r\nepilogue\r\n",

			// same as above, but with bare linefeeds
			"From: mimekit@example.org\nContent-Type: multipart/mixed; boundary=\"b\"\n\npreamble\n--b\n\n--b\nContent-Type: text/plain\n\nline 1\n\n--b\nContent-Type: text/plain\n\nline 2\n--b--\nepilogue\n",

			// over-long lines (that get consumed mid-line) immediately followed by a boundary marker
			"From: mimekit@example.org\r\nContent-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\n\r\n" + LongLine + "\r\n--b\r\n\r\n" + LongLine + "\n--b--\r\n",

			// nested multipart where the inner epilogue is terminated by the outer boundary
			"From: mimekit@example.org\r\nContent-Type: multipart/mixed; boundary=\"outer\"\r\n\r\n--outer\r\nContent-Type: multipart/alternative; boundary=\"inner\"\r\n\r\n--inner\r\n\r\ninner\r\n--inner--\r\ninner epilogue\r\n--outer--\r\n",

			// single-part messages terminated by EOS, with and without a trailing newline
			"From: mimekit@example.org\r\n\r\nbody\r\n",
			"From: mimekit@example.org\r\n\r\nbody",
			"From: mimekit@example.org\r\n\r\nbody\r",
		};

		static readonly int[] ContentCallbackExpectedRegions = { 5, 5, 4, 5, 1, 1, 1 };

		[Test]
		public void TestContentCallbacksExcludeBoundaryNewLine ([Values (1, 2, 3, 7, 4096)] int chunkSize)
		{
			for (int i = 0; i < ContentCallbackMessages.Length; i++) {
				var source = Encoding.ASCII.GetBytes (ContentCallbackMessages[i]);

				using (var stream = new ChunkedReadStream (source, chunkSize)) {
					var reader = new ContentRecordingReader (stream, MimeFormat.Entity);

					reader.ReadMessage ();

					AssertContentRegions (source, reader.Regions, ContentCallbackExpectedRegions[i], $"message #{i}");
				}
			}
		}

		[Test]
		public async Task TestContentCallbacksExcludeBoundaryNewLineAsync ([Values (1, 2, 3, 7, 4096)] int chunkSize)
		{
			for (int i = 0; i < ContentCallbackMessages.Length; i++) {
				var source = Encoding.ASCII.GetBytes (ContentCallbackMessages[i]);

				using (var stream = new ChunkedReadStream (source, chunkSize)) {
					var reader = new ContentRecordingReader (stream, MimeFormat.Entity);

					await reader.ReadMessageAsync ();

					AssertContentRegions (source, reader.Regions, ContentCallbackExpectedRegions[i], $"message #{i}");
				}
			}
		}

		[Test]
		public void TestContentCallbacksExcludeBoundaryNewLineMbox ()
		{
			var source = File.ReadAllBytes (Path.Combine (MboxDataDir, "jwz.mbox.txt"));

			using (var stream = new MemoryStream (source, false)) {
				var reader = new ContentRecordingReader (stream, MimeFormat.Mbox);

				while (!reader.IsEndOfStream)
					reader.ReadMessage ();

				AssertContentRegions (source, reader.Regions, -1, "jwz.mbox.txt");
			}
		}

		[Test]
		public async Task TestContentCallbacksExcludeBoundaryNewLineMboxAsync ()
		{
			var source = File.ReadAllBytes (Path.Combine (MboxDataDir, "jwz.mbox.txt"));

			using (var stream = new MemoryStream (source, false)) {
				var reader = new ContentRecordingReader (stream, MimeFormat.Mbox);

				while (!reader.IsEndOfStream)
					await reader.ReadMessageAsync ();

				AssertContentRegions (source, reader.Regions, -1, "jwz.mbox.txt");
			}
		}

		[Test]
		public void TestLineCountSingleLine ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountSingleLineCRLF ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineCRLFAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
".ReplaceLineEndings ("\r\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountSingleLineInMultipart ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
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
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineInMultipartAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
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
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public void TestLineCountOneLineOfTextFollowedByBlankLineInMultipart ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
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
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		[Test]
		public async Task TestLineCountOneLineOfTextFollowedByBlankLineInMultipartAsync ()
		{
			string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
Date: Tue, 12 Nov 2013 09:12:42 -0500
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
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
			}
		}

		static readonly string[] EmptyContainerTypes = { "multipart/alternative; boundary=inner", "message/rfc822" };

		static string CreateEmptyContainerInMultipart (string contentType, string newLine)
		{
			return $"Content-Type: multipart/mixed; boundary=outer{newLine}{newLine}--outer{newLine}Content-Type: {contentType}{newLine}{newLine}--outer--{newLine}";
		}

		[Test]
		public void TestLineCountEmptyContainerInMultipart ([ValueSource (nameof (EmptyContainerTypes))] string contentType, [Values ("\n", "\r\n")] string newLine)
		{
			var text = CreateEmptyContainerInMultipart (contentType, newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				reader.ReadEntity ();

				Assert.That (reader.Offsets[0].Children[0].Lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountEmptyContainerInMultipartAsync ([ValueSource (nameof (EmptyContainerTypes))] string contentType, [Values ("\n", "\r\n")] string newLine)
		{
			var text = CreateEmptyContainerInMultipart (contentType, newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Entity);

				await reader.ReadEntityAsync ();

				Assert.That (reader.Offsets[0].Children[0].Lines, Is.EqualTo (0), "Line count");
			}
		}

		static string CreateEmptyContainerInMbox (string contentType, string newLine)
		{
			return $"From a@example.com{newLine}Content-Type: {contentType}{newLine}{newLine}From b@example.com{newLine}Subject: second{newLine}{newLine}body{newLine}";
		}

		[Test]
		public void TestLineCountEmptyContainerInMbox ([ValueSource (nameof (EmptyContainerTypes))] string contentType, [Values ("\n", "\r\n")] string newLine)
		{
			var text = CreateEmptyContainerInMbox (contentType, newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Mbox);

				reader.ReadMessage ();

				Assert.That (reader.Offsets[0].Body.Lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountEmptyContainerInMboxAsync ([ValueSource (nameof (EmptyContainerTypes))] string contentType, [Values ("\n", "\r\n")] string newLine)
		{
			var text = CreateEmptyContainerInMbox (contentType, newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Mbox);

				await reader.ReadMessageAsync ();

				Assert.That (reader.Offsets[0].Body.Lines, Is.EqualTo (0), "Line count");
			}
		}

		static void AssertEmptyEntityOffsets (MimeOffsets offsets, long expected, string what)
		{
			Assert.That (offsets.BeginOffset, Is.EqualTo (expected), $"{what} BeginOffset");
			Assert.That (offsets.HeadersEndOffset, Is.EqualTo (expected), $"{what} HeadersEndOffset");
			Assert.That (offsets.EndOffset, Is.EqualTo (expected), $"{what} EndOffset");
			Assert.That (offsets.Lines ?? 0, Is.EqualTo (0), $"{what} Lines");
		}

		[Test]
		public void TestOffsetsEmptyMessageInMbox ([Values ("\n", "\r\n")] string newLine)
		{
			var text = $"From -{newLine}From b@example.com{newLine}Subject: second{newLine}{newLine}body{newLine}";
			long expected = "From -".Length + newLine.Length;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Mbox);

				reader.ReadMessage ();

				AssertEmptyEntityOffsets (reader.Offsets[0], expected, "Message");
				AssertEmptyEntityOffsets (reader.Offsets[0].Body, expected, "Body");
			}
		}

		[Test]
		public async Task TestOffsetsEmptyMessageInMboxAsync ([Values ("\n", "\r\n")] string newLine)
		{
			var text = $"From -{newLine}From b@example.com{newLine}Subject: second{newLine}{newLine}body{newLine}";
			long expected = "From -".Length + newLine.Length;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new CustomMimeReader (stream, MimeFormat.Mbox);

				await reader.ReadMessageAsync ();

				AssertEmptyEntityOffsets (reader.Offsets[0], expected, "Message");
				AssertEmptyEntityOffsets (reader.Offsets[0].Body, expected, "Body");
			}
		}

		[Test]
		public void TestLineCountNonTerminatedSingleHeader ()
		{
			const string text = "Date: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\nFrom: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.IncompleteHeader), "Violation");
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (3), "LineNumber");
			}
		}

		[Test]
		public async Task TestLineCountNonTerminatedSingleHeaderAsync ()
		{
			const string text = "Date: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\nFrom: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.IncompleteHeader), "Violation");
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (3), "LineNumber");
			}
		}

		[Test]
		public void TestLineCountProperlyTerminatedSingleHeader ()
		{
			const string text = "Date: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\nFrom: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.MissingBodySeparator), "Violation");
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (4), "LineNumber");
			}
		}

		[Test]
		public async Task TestLineCountProperlyTerminatedSingleHeaderAsync ()
		{
			const string text = "Date: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\nFrom: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new CustomMimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				var lines = reader.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
				Assert.That (logger.Issues.Count, Is.EqualTo (1), "ComplianceViolations");
				Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.MissingBodySeparator), "Violation");
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (text.Length), "StreamOffset");
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (4), "LineNumber");
			}
		}

		// Note: The body of this message ends every other line with a bare linefeed instead of a
		// CRLF sequence. Each of those linefeeds is on a different line of the body, which is what
		// makes this a regression test: MimeReader.ScanContent () does not update inputIndex until
		// it has finished scanning the buffer, so calculating the position of the linefeed from
		// inputIndex (rather than from the current scan pointer) reported the start of the body for
		// every linefeed and, once the line tracking had advanced past that point, a negative column.
		// Note: These are the headers that a top-level message is expected to have, so tests that are
		// not about missing headers can prepend them to keep the compliance logger free of noise.
		const string MessageHeaders = "From: mimekit@example.org\r\nDate: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\nMIME-Version: 1.0\r\n";
		const int MessageHeaderLines = 4;

		const string BareLinefeedsInBodyText = "From: mimekit@example.org\r\n" +
			"To: mimekit@example.org\r\n" +
			"Subject: bare linefeeds in the body\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <123@example.org>\r\n" +
			"\r\n" +
			"This line is properly terminated.\r\n" +
			"This line is not.\n" +
			"Another properly terminated line.\r\n" +
			"And this one is not either.\n";

		static void AssertBareLinefeedsInBody (TestMimeComplianceLogger logger)
		{
			const string firstLine = "This line is not.";
			const string secondLine = "And this one is not either.";

			var firstOffset = BareLinefeedsInBodyText.IndexOf (firstLine, StringComparison.Ordinal) + firstLine.Length;
			var secondOffset = BareLinefeedsInBodyText.IndexOf (secondLine, StringComparison.Ordinal) + secondLine.Length;

			Assert.That (logger.Issues.Count, Is.EqualTo (2), "ComplianceViolations");

			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.BareLinefeedInBody), "Violation #1");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (firstOffset), "StreamOffset #1");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (8), "LineNumber #1");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (firstLine.Length + 1), "ColumnNumber #1");

			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.BareLinefeedInBody), "Violation #2");
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (secondOffset), "StreamOffset #2");
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (10), "LineNumber #2");
			Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (secondLine.Length + 1), "ColumnNumber #2");
		}

		[Test]
		public void TestBareLinefeedsInBodyPositions ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (BareLinefeedsInBodyText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertBareLinefeedsInBody (logger);
			}
		}

		[Test]
		public async Task TestBareLinefeedsInBodyPositionsAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (BareLinefeedsInBodyText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertBareLinefeedsInBody (logger);
			}
		}

		static void AssertInvalidHeaderFieldBytes (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (2), FormatIssues (logger));

			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidHeader), FormatIssues (logger));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (3), "StreamOffset #1");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (1), "LineNumber #1");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (4), "ColumnNumber #1");

			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.UnexpectedNullBytesInHeader), FormatIssues (logger));
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (0), "StreamOffset #2");
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (1), "LineNumber #2");
			Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (1), "ColumnNumber #2");
			Assert.That (logger.Issues[1].PositionKind, Is.EqualTo (MimeCompliancePositionKind.LineStart), "PositionKind #2");
		}

		static string FormatIssues (TestMimeComplianceLogger logger)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < logger.Issues.Count; i++) {
				var issue = logger.Issues[i];

				builder.Append ('#').Append (i).Append (": ")
					.Append (issue.Violation).Append (" offset=").Append (issue.StreamOffset)
					.Append (" line=").Append (issue.LineNumber)
					.Append (" column=").Append (issue.ColumnNumber)
					.Append (" kind=").Append (issue.PositionKind).AppendLine ();
			}

			return builder.ToString ();
		}

		[Test]
		public void TestInvalidHeaderFieldReportsNullByte ()
		{
			var data = new byte[] { (byte) 'B', (byte) 'a', (byte) 'd', 0x00, 0xff, (byte) '\r', (byte) '\n', (byte) '\r', (byte) '\n' };

			using (var stream = new MemoryStream (data, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				var ex = Assert.Throws<FormatException> (() => reader.ReadHeaders ());

				Assert.That (ex.Message, Is.EqualTo ("Failed to parse headers."));
				AssertInvalidHeaderFieldBytes (logger);
			}
		}

		[Test]
		public async Task TestInvalidHeaderFieldReportsNullByteAsync ()
		{
			var data = new byte[] { (byte) 'B', (byte) 'a', (byte) 'd', 0x00, 0xff, (byte) '\r', (byte) '\n', (byte) '\r', (byte) '\n' };

			using (var stream = new MemoryStream (data, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				var ex = Assert.ThrowsAsync<FormatException> (async () => await reader.ReadHeadersAsync ());

				Assert.That (ex.Message, Is.EqualTo ("Failed to parse headers."));
				AssertInvalidHeaderFieldBytes (logger);
			}
		}

		static void AssertBinaryContentBytesAreAccepted (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (0), "ComplianceViolations");
		}

		[Test]
		public void TestBinaryContentTransferEncodingAcceptsNullAnd8BitBytes ()
		{
			var message = Encoding.ASCII.GetBytes (MessageHeaders + "Content-Transfer-Encoding: binary\r\n\r\n");
			var data = new byte[message.Length + 4];

			Buffer.BlockCopy (message, 0, data, 0, message.Length);
			data[message.Length] = 0xff;
			data[message.Length + 1] = 0x00;
			data[message.Length + 2] = (byte) '\r';
			data[message.Length + 3] = (byte) '\n';

			using (var stream = new MemoryStream (data, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertBinaryContentBytesAreAccepted (logger);
			}
		}

		[Test]
		public async Task TestBinaryContentTransferEncodingAcceptsNullAnd8BitBytesAsync ()
		{
			var message = Encoding.ASCII.GetBytes (MessageHeaders + "Content-Transfer-Encoding: binary\r\n\r\n");
			var data = new byte[message.Length + 4];

			Buffer.BlockCopy (message, 0, data, 0, message.Length);
			data[message.Length] = 0xff;
			data[message.Length + 1] = 0x00;
			data[message.Length + 2] = (byte) '\r';
			data[message.Length + 3] = (byte) '\n';

			using (var stream = new MemoryStream (data, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertBinaryContentBytesAreAccepted (logger);
			}
		}

		static byte[] CreateMultipartMessagePartWithInvalidFirstLine ()
		{
			var prefix = Encoding.ASCII.GetBytes (MessageHeaders + "Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\nContent-Type: message/rfc822\r\n\r\n");
			var suffix = Encoding.ASCII.GetBytes ("bad\r\n--b--\r\n");
			var data = new byte[prefix.Length + 2 + suffix.Length];

			Buffer.BlockCopy (prefix, 0, data, 0, prefix.Length);
			data[prefix.Length] = 0xff;
			data[prefix.Length + 1] = 0x00;
			Buffer.BlockCopy (suffix, 0, data, prefix.Length + 2, suffix.Length);

			return data;
		}

		static void AssertMessagePartInvalidFirstLineIssues (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (4), FormatIssues (logger));

			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidHeader), "Violation #1");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (MessageHeaders.Length + 84), "StreamOffset #1");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (MessageHeaderLines + 6), "LineNumber #1");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (1), "ColumnNumber #1");

			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.UnexpectedNullBytesInHeader), "Violation #2");
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (MessageHeaders.Length + 84), "StreamOffset #2");
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (MessageHeaderLines + 6), "LineNumber #2");
			Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (1), "ColumnNumber #2");
			Assert.That (logger.Issues[1].PositionKind, Is.EqualTo (MimeCompliancePositionKind.LineStart), "PositionKind #2");

			Assert.That (logger.Issues[2].Violation, Is.EqualTo (MimeComplianceViolation.Unexpected8BitBytesInHeader), "Violation #3");
			Assert.That (logger.Issues[2].StreamOffset, Is.EqualTo (MessageHeaders.Length + 84), "StreamOffset #3");
			Assert.That (logger.Issues[2].LineNumber, Is.EqualTo (MessageHeaderLines + 6), "LineNumber #3");
			Assert.That (logger.Issues[2].ColumnNumber, Is.EqualTo (1), "ColumnNumber #3");
			Assert.That (logger.Issues[2].PositionKind, Is.EqualTo (MimeCompliancePositionKind.ElementStart), "PositionKind #3");

			Assert.That (logger.Issues[3].Violation, Is.EqualTo (MimeComplianceViolation.MissingBodySeparator), "Violation #4");
			Assert.That (logger.Issues[3].StreamOffset, Is.EqualTo (MessageHeaders.Length + 91), "StreamOffset #4");
			Assert.That (logger.Issues[3].LineNumber, Is.EqualTo (MessageHeaderLines + 7), "LineNumber #4");
			Assert.That (logger.Issues[3].ColumnNumber, Is.EqualTo (1), "ColumnNumber #4");
			Assert.That (logger.Issues[3].PositionKind, Is.EqualTo (MimeCompliancePositionKind.ElementStart), "PositionKind #4");
		}

		[Test]
		public void TestMessagePartInvalidFirstLineReportsHeaderIssues ()
		{
			using (var stream = new MemoryStream (CreateMultipartMessagePartWithInvalidFirstLine (), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMessagePartInvalidFirstLineIssues (logger);
			}
		}

		[Test]
		public async Task TestMessagePartInvalidFirstLineReportsHeaderIssuesAsync ()
		{
			using (var stream = new MemoryStream (CreateMultipartMessagePartWithInvalidFirstLine (), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMessagePartInvalidFirstLineIssues (logger);
			}
		}

		const string EmbeddedUtf8HeaderMessage = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\nContent-Type: message/rfc822\r\n\r\nSubject: caf\u00e9\r\n\r\nbody\r\n--b--\r\n";

		[Test]
		public void TestMessagePartUtf8FirstHeaderIsNotReportedAsBody ()
		{
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (EmbeddedUtf8HeaderMessage), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), FormatIssues (logger));
			}
		}

		[Test]
		public async Task TestMessagePartUtf8FirstHeaderIsNotReportedAsBodyAsync ()
		{
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (EmbeddedUtf8HeaderMessage), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), FormatIssues (logger));
			}
		}

		const string EmbeddedBareLinefeedHeaderMessage = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\nContent-Type: message/rfc822\r\n\r\nSubject: test\nFrom: sender@example.org\r\n\r\nbody\r\n--b--\r\n";

		static void AssertMessagePartBareLinefeedFirstHeader (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (1), FormatIssues (logger));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.BareLinefeedInHeader), "Violation");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (MessageHeaders.Length + 97), "StreamOffset");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (MessageHeaderLines + 6), "LineNumber");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (14), "ColumnNumber");
		}

		[Test]
		public void TestMessagePartBareLinefeedFirstHeaderIsNotReportedAsBody ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (EmbeddedBareLinefeedHeaderMessage), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMessagePartBareLinefeedFirstHeader (logger);
			}
		}

		[Test]
		public async Task TestMessagePartBareLinefeedFirstHeaderIsNotReportedAsBodyAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (EmbeddedBareLinefeedHeaderMessage), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMessagePartBareLinefeedFirstHeader (logger);
			}
		}

		static void AssertInvalidBoundary (TestMimeComplianceLogger logger, long missingBoundaryOffset)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (2), FormatIssues (logger));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidMultipartBoundaryParameter), "Violation #1");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (MessageHeaders.Length), "StreamOffset #1");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (MessageHeaderLines + 1), "LineNumber #1");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (1), "ColumnNumber #1");
			Assert.That (logger.Issues[0].PositionKind, Is.EqualTo (MimeCompliancePositionKind.ElementStart), "PositionKind #1");
			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.MissingMultipartBoundary), "Violation #2");
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (missingBoundaryOffset), "StreamOffset #2");
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (MessageHeaderLines + 4), "LineNumber #2");
			Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (1), "ColumnNumber #2");
		}

		[Test]
		public void TestBoundaryLongerThanRfcLimitIsInvalid ()
		{
			var text = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"" + new string ('x', 71) + "\"\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertInvalidBoundary (logger, MessageHeaders.Length + 123);
			}
		}

		[Test]
		public async Task TestBoundaryLongerThanRfcLimitIsInvalidAsync ()
		{
			var text = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"" + new string ('x', 71) + "\"\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertInvalidBoundary (logger, MessageHeaders.Length + 123);
			}
		}

		[Test]
		public void TestBoundaryEndingWithSpaceIsInvalid ()
		{
			const string text = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"trailing-space \"\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertInvalidBoundary (logger, MessageHeaders.Length + 67);
			}
		}

		[Test]
		public async Task TestBoundaryEndingWithSpaceIsInvalidAsync ()
		{
			const string text = MessageHeaders + "Content-Type: multipart/mixed; boundary=\"trailing-space \"\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertInvalidBoundary (logger, MessageHeaders.Length + 67);
			}
		}

		[Test]
		public void TestReadToEosReturnsBufferedAndUnreadInput ()
		{
			var data = Encoding.ASCII.GetBytes ("Header: value\r\n\r\nbody");

			using (var stream = new MemoryStream (data, false)) {
				var reader = new MimeReader (stream, MimeFormat.Entity);

				reader.ReadHeaders ();

				using var remainder = reader.ReadToEos ();
				using var memory = new MemoryStream ();

				remainder.CopyTo (memory);

				Assert.That (Encoding.ASCII.GetString (memory.ToArray ()), Is.EqualTo ("body"));
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

		static void UpdateStreamOffsets (string path, ExpectedMimeComplianceIssue[] issues, out int bareLineFeeds)
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
					if (issue.LineNumber == lineNumber && issue.ColumnNumber == column) {
						issue.UnixOffset = unixOffset;
						issue.DosOffset = dosOffset;
					}
				}
			}
		}

		static void AssertMimeComplianceViolations (string fileName, ExpectedMimeComplianceIssue[] issues)
		{
			var path = Path.Combine (ComplianceDataDir, fileName);
			var expectedCount = issues.Length;

			UpdateStreamOffsets (path, issues, out var bareLineFeeds);

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Dos2UnixFilter ());

					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (filtered) {
						ComplianceLogger = logger
					};

					reader.ReadMessage ();

					Assert.That (logger.Issues.Count, Is.EqualTo (expectedCount + bareLineFeeds), "ComplianceViolations for Unix format");

					for (int i = 0, v = 0; i < issues.Length && v < logger.Issues.Count; v++) {
						var actual = logger.Issues[v];

						if (actual.Violation == MimeComplianceViolation.BareLinefeedInHeader ||
							actual.Violation == MimeComplianceViolation.BareLinefeedInBody)
							continue;

						var expected = issues[i++];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.ColumnNumber, Is.EqualTo (expected.ColumnNumber), $"ColumnNumber for issue #{i}");
						Assert.That (actual.PositionKind, Is.EqualTo (expected.PositionKind), $"PositionKind for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.UnixOffset), $"StreamOffset for issue #{i}");
					}
				}
			}

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (filtered) {
						ComplianceLogger = logger
					};

					reader.ReadMessage ();

					Assert.That (logger.Issues.Count, Is.EqualTo (expectedCount), "ComplianceViolations for DOS format");

					for (int i = 0, v = 0; i < issues.Length && v < logger.Issues.Count; v++) {
						var actual = logger.Issues[v];
						var expected = issues[i++];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.ColumnNumber, Is.EqualTo (expected.ColumnNumber), $"ColumnNumber for issue #{i}");
						Assert.That (actual.PositionKind, Is.EqualTo (expected.PositionKind), $"PositionKind for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.DosOffset), $"StreamOffset for issue #{i}");
					}
				}
			}
		}

		static async Task AssertMimeComplianceViolationsAsync (string fileName, ExpectedMimeComplianceIssue[] issues)
		{
			var path = Path.Combine (ComplianceDataDir, fileName);
			var expectedCount = issues.Length;

			UpdateStreamOffsets (path, issues, out var bareLineFeeds);

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Dos2UnixFilter ());

					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (filtered) {
						ComplianceLogger = logger
					};

					await reader.ReadMessageAsync ();

					Assert.That (logger.Issues.Count, Is.EqualTo (expectedCount + bareLineFeeds), "ComplianceViolations for Unix format");

					for (int i = 0, v = 0; i < issues.Length && v < logger.Issues.Count; v++) {
						var actual = logger.Issues[v];

						if (actual.Violation == MimeComplianceViolation.BareLinefeedInHeader ||
							actual.Violation == MimeComplianceViolation.BareLinefeedInBody)
							continue;

						var expected = issues[i++];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.ColumnNumber, Is.EqualTo (expected.ColumnNumber), $"ColumnNumber for issue #{i}");
						Assert.That (actual.PositionKind, Is.EqualTo (expected.PositionKind), $"PositionKind for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.UnixOffset), $"StreamOffset for issue #{i}");
					}
				}
			}

			using (var stream = File.OpenRead (path)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (filtered) {
						ComplianceLogger = logger
					};

					await reader.ReadMessageAsync ();

					Assert.That (logger.Issues.Count, Is.EqualTo (expectedCount), "ComplianceViolations for DOS format");

					for (int i = 0; i < issues.Length; i++) {
						var actual = logger.Issues[i];
						var expected = issues[i];

						Assert.That (actual.Violation, Is.EqualTo (expected.Violation), $"Violation for issue #{i}");
						Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"LineNumber for issue #{i}");
						Assert.That (actual.ColumnNumber, Is.EqualTo (expected.ColumnNumber), $"ColumnNumber for issue #{i}");
						Assert.That (actual.PositionKind, Is.EqualTo (expected.PositionKind), $"PositionKind for issue #{i}");
						Assert.That (actual.StreamOffset, Is.EqualTo (expected.DosOffset), $"StreamOffset for issue #{i}");
					}
				}
			}
		}

		[Test]
		public void TestMimeComplianceInvalidHeaderFieldNameWithSpace ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 7, 10),
			};

			AssertMimeComplianceViolations ("invalid-header-field-with-space.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidHeaderFieldNameWithSpaceAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidHeader, 7, 10),
			};

			return AssertMimeComplianceViolationsAsync ("invalid-header-field-with-space.eml", issues);
		}

		// Note: The field names of the last 2 headers in this message are invalid. The first contains
		// a space and the second contains a control character. This is a regression test: the reported
		// position of an InvalidHeader used to always be the start of the header rather than the
		// offending character within the field name.
		const string InvalidHeaderFieldNamesText = "From: mimekit@example.org\r\n" +
			"To: mimekit@example.org\r\n" +
			"Subject: invalid header field names\r\n" +
			"X-Invalid Header: the field name contains a space\r\n" +
			"X-Ctl\u0001Header: the field name contains a control character\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <123@example.org>\r\n" +
			"\r\n" +
			"This is the body.\r\n";

		static void AssertInvalidHeaderFieldNames (TestMimeComplianceLogger logger)
		{
			var spaceOffset = InvalidHeaderFieldNamesText.IndexOf ("X-Invalid Header", StringComparison.Ordinal) + "X-Invalid".Length;
			var controlOffset = InvalidHeaderFieldNamesText.IndexOf ("X-Ctl\u0001Header", StringComparison.Ordinal) + "X-Ctl".Length;

			Assert.That (logger.Issues.Count, Is.EqualTo (2), "ComplianceViolations");

			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.InvalidHeader), "Violation #1");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (spaceOffset), "StreamOffset #1");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (4), "LineNumber #1");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (10), "ColumnNumber #1");
			Assert.That (logger.Issues[0].PositionKind, Is.EqualTo (MimeCompliancePositionKind.Exact), "PositionKind #1");

			Assert.That (logger.Issues[1].Violation, Is.EqualTo (MimeComplianceViolation.InvalidHeader), "Violation #2");
			Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (controlOffset), "StreamOffset #2");
			Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (5), "LineNumber #2");
			Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (6), "ColumnNumber #2");
			Assert.That (logger.Issues[1].PositionKind, Is.EqualTo (MimeCompliancePositionKind.Exact), "PositionKind #2");
		}

		[Test]
		public void TestInvalidHeaderFieldNamePositions ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidHeaderFieldNamesText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertInvalidHeaderFieldNames (logger);
			}
		}

		[Test]
		public async Task TestInvalidHeaderFieldNamePositionsAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidHeaderFieldNamesText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertInvalidHeaderFieldNames (logger);
			}
		}

		[Test]
		public void TestMimeComplianceIncompleteHeader ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IncompleteHeader, 7, 25)
			};

			AssertMimeComplianceViolations ("incomplete-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceIncompleteHeaderAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IncompleteHeader, 7, 25)
			};

			return AssertMimeComplianceViolationsAsync ("incomplete-header.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingBasic ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidContentTransferEncoding, 7, 28)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-basic.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingBasicAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidContentTransferEncoding, 7, 28)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-basic.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingMultipart ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 10, 28)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-multipart.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingMultipartAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IllegalMultipartContentTransferEncoding, 10, 28)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-multipart.eml", issues);
		}

		[Test]
		public void TestMimeComplianceInvalidContentTransferEncodingRfc822 ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 7, 28)
			};

			AssertMimeComplianceViolations ("invalid-content-transfer-encoding-rfc822.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTransferEncodingRfc822Async ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding, 7, 28)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-content-transfer-encoding-rfc822.eml", issues);
		}

		// Note: The Content-Transfer-Encoding value of this message is folded onto the line following
		// the ':'. This is a regression test for the position of the Content-Transfer-Encoding
		// violations, which is the start of the value rather than the start of the header, so skipping
		// the folding whitespace has to keep track of the line number as well as the column.
		const string FoldedContentTransferEncodingText = "From: mimekit@example.org\r\n" +
			"To: mimekit@example.org\r\n" +
			"Subject: folded Content-Transfer-Encoding\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: message/rfc822\r\n" +
			"Content-Transfer-Encoding:\r\n" +
			"\tbase64\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <123@example.org>\r\n" +
			"\r\n" +
			"VGhpcyBpcyB0aGUgcmZjODIyIG1lc3NhZ2UgYm9keS4K\r\n";

		static void AssertFoldedContentTransferEncoding (TestMimeComplianceLogger logger)
		{
			var offset = FoldedContentTransferEncodingText.IndexOf ("base64", StringComparison.Ordinal);

			Assert.That (logger.Issues.Count, Is.EqualTo (1), "ComplianceViolations");

			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding), "Violation");
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (offset), "StreamOffset");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (7), "LineNumber");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (2), "ColumnNumber");
			Assert.That (logger.Issues[0].PositionKind, Is.EqualTo (MimeCompliancePositionKind.Exact), "PositionKind");
		}

		[Test]
		public void TestFoldedContentTransferEncodingPosition ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (FoldedContentTransferEncodingText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertFoldedContentTransferEncoding (logger);
			}
		}

		[Test]
		public async Task TestFoldedContentTransferEncodingPositionAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (FoldedContentTransferEncodingText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertFoldedContentTransferEncoding (logger);
			}
		}

		// Note: A header without a ':' at the end of the stream must not be interpreted as a Content-Transfer-Encoding
		// header (its value would be located beyond the end of the stream).
		const string InvalidContentTransferEncodingAtEndOfStreamText = "From: a@b.c\nContent-Type: text/plain\nContent-Transfer-Encoding";

		static void AssertInvalidContentTransferEncodingAtEndOfStream (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.No.Member (MimeComplianceViolation.InvalidContentTransferEncoding), "Violations");

			foreach (var issue in logger.Issues)
				Assert.That (issue.StreamOffset, Is.InRange (0, InvalidContentTransferEncodingAtEndOfStreamText.Length), $"{issue.Violation} StreamOffset");
		}

		[Test]
		public void TestInvalidContentTransferEncodingHeaderAtEndOfStream ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidContentTransferEncodingAtEndOfStreamText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertInvalidContentTransferEncodingAtEndOfStream (logger);
			}
		}

		[Test]
		public async Task TestInvalidContentTransferEncodingHeaderAtEndOfStreamAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidContentTransferEncodingAtEndOfStreamText), false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertInvalidContentTransferEncodingAtEndOfStream (logger);
			}
		}

		// Note: A header without a ':' that is followed by a newline is never interpreted as a known header, so one
		// at the end of the stream must not be either.
		const string InvalidContentTypeAtEndOfStreamText = "From: a@b.c\nContent-Transfer-Encoding: 7bit\nContent-Type";

		[Test]
		public void TestInvalidContentTypeHeaderAtEndOfStream ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidContentTypeAtEndOfStreamText), false)) {
				var message = MimeMessage.Load (stream);

				Assert.That (message.Body.ContentType.MimeType, Is.EqualTo ("text/plain"));
			}
		}

		[Test]
		public async Task TestInvalidContentTypeHeaderAtEndOfStreamAsync ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (InvalidContentTypeAtEndOfStreamText), false)) {
				var message = await MimeMessage.LoadAsync (stream);

				Assert.That (message.Body.ContentType.MimeType, Is.EqualTo ("text/plain"));
			}
		}

		[Test]
		public void TestMimeComplianceInvalidContentType ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidContentType, 6, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("invalid-content-type.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceInvalidContentTypeAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.InvalidContentType, 6, 1, MimeCompliancePositionKind.ElementStart)
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
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.InvalidMimeVersion, 5, 1)
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
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.InvalidMimeVersion, 5, 1)
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
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.MissingMimeVersion, 1, 1),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.MissingMimeVersion, 15, 1)
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
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.MissingMimeVersion, 1, 1),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceStatus.MissingMimeVersion, 15, 1)
			};

			return AssertMimeComplianceIssuesAsync (text, issues);
		}
#endif

		[Test]
		public void TestMimeComplianceOversizedLine ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.OversizedLine, 7, 1, MimeCompliancePositionKind.LineStart),
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.OversizedLine, 11, 1, MimeCompliancePositionKind.LineStart)
			};

			AssertMimeComplianceViolations ("invalid-wrapping.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceOversizedLineAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.OversizedLine, 7, 1, MimeCompliancePositionKind.LineStart),
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.OversizedLine, 11, 1, MimeCompliancePositionKind.LineStart)
			};

			return AssertMimeComplianceViolationsAsync ("invalid-wrapping.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingBodySeparator ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingBodySeparator, 8, 1),
			};

			AssertMimeComplianceViolations ("missing-body-separator.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingBodySeparatorAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingBodySeparator, 8, 1),
			};

			return AssertMimeComplianceViolationsAsync ("missing-body-separator.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartBoundaryParameter ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundaryParameter, 6, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("missing-multipart-boundary-parameter.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartBoundaryParameterAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundaryParameter, 6, 1, MimeCompliancePositionKind.ElementStart)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-boundary-parameter.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartBoundary ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 19, 1)
			};

			AssertMimeComplianceViolations ("missing-multipart-boundary.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartBoundaryAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 19, 1)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-boundary.eml", issues);
		}

		[Test]
		public void TestMimeComplianceMissingMultipartEndBoundary ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 19, 1)
			};

			AssertMimeComplianceViolations ("missing-multipart-end-boundary.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceMissingMultipartEndBoundaryAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.MissingMultipartBoundary, 19, 1)
			};

			return AssertMimeComplianceViolationsAsync ("missing-multipart-end-boundary.eml", issues);
		}

		[Test]
		public void TestMimeComplianceRepeatedContentTransferEncoding ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.RepeatedContentTransferEncoding, 8, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("multiple-content-transfer-encodings.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceRepeatedContentTransferEncodingAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.RepeatedContentTransferEncoding, 8, 1, MimeCompliancePositionKind.ElementStart)
			};

			return AssertMimeComplianceViolationsAsync ("multiple-content-transfer-encodings.eml", issues);
		}

		[Test]
		public void TestMimeComplianceRepeatedContentType ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.RepeatedContentType, 7, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("multiple-content-types.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceRepeatedContentTypeAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.RepeatedContentType, 7, 1, MimeCompliancePositionKind.ElementStart)
			};

			return AssertMimeComplianceViolationsAsync ("multiple-content-types.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInHeader ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInHeader, 3, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("raw-koi8r-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInHeaderAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInHeader, 3, 1, MimeCompliancePositionKind.ElementStart)
			};

			return AssertMimeComplianceViolationsAsync ("raw-koi8r-header.eml", issues);
		}

		[Test]
		public void TestMimeComplianceValid8BitBytesInHeader ()
		{
			var issues = Array.Empty<ExpectedMimeComplianceIssue> ();

			AssertMimeComplianceViolations ("raw-utf8-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceValid8BitBytesInHeaderAsync ()
		{
			var issues = Array.Empty<ExpectedMimeComplianceIssue> ();

			return AssertMimeComplianceViolationsAsync ("raw-utf8-header.eml", issues);
		}

		// Note: The 8-bit bytes live on a folded continuation line rather than on the line that
		// starts the header. The violation is still attributed to the beginning of the header,
		// because the value can only be validated once it has been unfolded in its entirety.
		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInFoldedHeader ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInHeader, 3, 1, MimeCompliancePositionKind.ElementStart)
			};

			AssertMimeComplianceViolations ("8bit-folded-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInFoldedHeaderAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInHeader, 3, 1, MimeCompliancePositionKind.ElementStart)
			};

			return AssertMimeComplianceViolationsAsync ("8bit-folded-header.eml", issues);
		}

		// Note: UTF-8 is legal in headers per rfc6532, so a folded continuation line containing
		// valid UTF-8 must not be reported even though it is not US-ASCII.
		[Test]
		public void TestMimeComplianceValid8BitBytesInFoldedHeader ()
		{
			var issues = Array.Empty<ExpectedMimeComplianceIssue> ();

			AssertMimeComplianceViolations ("utf8-folded-header.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceValid8BitBytesInFoldedHeaderAsync ()
		{
			var issues = Array.Empty<ExpectedMimeComplianceIssue> ();

			return AssertMimeComplianceViolationsAsync ("utf8-folded-header.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInBody ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 24, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-body.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInBodyAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 24, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-body.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInPreamble ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 11, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-preamble.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInPreambleAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 11, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-preamble.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpected8BitBytesInEpilogue ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 28, 1)
			};

			AssertMimeComplianceViolations ("unexpected-8bit-bytes-in-epilogue.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpected8BitBytesInEpilogueAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 28, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-8bit-bytes-in-epilogue.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpectedNullBytesInHeader ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInHeader, 5, 1, MimeCompliancePositionKind.LineStart)
			};

			AssertMimeComplianceViolations ("unexpected-null-bytes-in-headers.eml", issues);
		}

		static readonly object[] BareCarriageReturnInHeaderCases = {
			// mid-value
			new object[] { "Subject: a\rb\r\n\r\nbody\r\n", new int[] { 10 } },
			// only the first bare CR in a header is reported
			new object[] { "Subject: a\rb\rc\r\n\r\nbody\r\n", new int[] { 10 } },
			// ... but each header gets its own report
			new object[] { "A: x\ry\nB: z\rw\n\nbody\n", new int[] { 4, 11 } },
			// a line made up of nothing but CRs
			new object[] { "Subject: a\n\r\r\nTo: b\n\nbody\n", new int[] { 11 } },
			new object[] { "Subject: a\n\r\r\r\nTo: b\n\nbody\n", new int[] { 11 } },
			// on a folded continuation line
			new object[] { "Subject: a\n b\rc\n\nbody\n", new int[] { 13 } },
			// CRs right before the line terminator
			new object[] { "Subject: a\r\r\n\nbody\n", new int[] { 10 } },
			// at the end of the stream
			new object[] { "Subject: a\r", new int[] { 10 } },
			new object[] { "Subject: a\rb", new int[] { 10 } },
			// in the headers of a MIME part
			new object[] { "Content-Type: multipart/mixed; boundary=x\n\n--x\nA: b\rc\n\nbody\n--x--\n", new int[] { 51 } },
			// not reported: CRLF line terminators, or bare CRs in the body
			new object[] { "Subject: a\r\n\r\nbody\r\n", Array.Empty<int> () },
			new object[] { "Subject: a\n\nb\rc\n", Array.Empty<int> () },
		};

		static IEnumerable<(Stream Stream, string Label)> CreateBareCarriageReturnStreams (byte[] data)
		{
			yield return (new MemoryStream (data, false), "unchunked");

			// Note: Exercise every possible position of each CR relative to the end of the input buffer.
			for (int chunkSize = 1; chunkSize <= data.Length; chunkSize++)
				yield return (new ChunkedReadStream (data, chunkSize), $"chunkSize={chunkSize}");
		}

		static void AssertBareCarriageReturnsInHeader (byte[] data, int[] offsets, TestMimeComplianceLogger logger, string label)
		{
			var issues = logger.Issues.Where (x => x.Violation == MimeComplianceViolation.BareCarriageReturnInHeader).ToList ();

			Assert.That (issues.Select (x => x.StreamOffset), Is.EqualTo (offsets.Select (x => (long) x)), $"{label}: {FormatIssues (logger)}");

			foreach (var issue in issues) {
				int offset = (int) issue.StreamOffset;
				int lineStart = Array.LastIndexOf (data, (byte) '\n', offset - 1) + 1;
				int lineNumber = data.Take (offset).Count (c => c == (byte) '\n') + 1;

				Assert.That (data[offset], Is.EqualTo ((byte) '\r'), $"{label}: byte at StreamOffset");
				Assert.That (issue.LineNumber, Is.EqualTo (lineNumber), $"{label}: LineNumber");
				Assert.That (issue.ColumnNumber, Is.EqualTo (offset - lineStart + 1), $"{label}: ColumnNumber");
				Assert.That (issue.PositionKind, Is.EqualTo (MimeCompliancePositionKind.Exact), $"{label}: PositionKind");
			}
		}

		[TestCaseSource (nameof (BareCarriageReturnInHeaderCases))]
		public void TestMimeComplianceBareCarriageReturnInHeader (string text, int[] offsets)
		{
			var data = Encoding.ASCII.GetBytes (text);

			foreach (var (stream, label) in CreateBareCarriageReturnStreams (data)) {
				using (stream) {
					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (stream) { ComplianceLogger = logger };

					reader.ReadMessage ();

					AssertBareCarriageReturnsInHeader (data, offsets, logger, label);
				}
			}
		}

		[TestCaseSource (nameof (BareCarriageReturnInHeaderCases))]
		public async Task TestMimeComplianceBareCarriageReturnInHeaderAsync (string text, int[] offsets)
		{
			var data = Encoding.ASCII.GetBytes (text);

			foreach (var (stream, label) in CreateBareCarriageReturnStreams (data)) {
				using (stream) {
					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (stream) { ComplianceLogger = logger };

					await reader.ReadMessageAsync ();

					AssertBareCarriageReturnsInHeader (data, offsets, logger, label);
				}
			}
		}

		static readonly object[] BlankHeaderContinuationLineCases = {
			// a single space, a single tab, and a mix of both
			new object[] { "Subject: x\r\n \r\nTo: y\r\n\r\nbody\r\n", new int[] { 12 } },
			new object[] { "Subject: x\n\t\nTo: y\n\nbody\n", new int[] { 11 } },
			new object[] { "Subject: x\n \t \t\nTo: y\n\nbody\n", new int[] { 11 } },
			// each blank line is reported
			new object[] { "Subject: x\n \n \nTo: y\n\nbody\n", new int[] { 11, 13 } },
			new object[] { "A: x\n \nB: y\n\t\n\nbody\n", new int[] { 5, 12 } },
			// between two legitimate folds
			new object[] { "Subject: x\n \n y\n\nbody\n", new int[] { 11 } },
			// right before the blank line that ends the header block
			new object[] { "Subject: x\n \n\nbody\n", new int[] { 11 } },
			// the line that RFC 7103 section 7.3 warns may be mistaken for the end of the header block
			new object[] { "Subject: x\n \nbody\n", new int[] { 11 } },
			// with a bare LF or a bare CR
			new object[] { "Subject: x\r\n \nTo: y\r\n\r\nbody\r\n", new int[] { 12 } },
			new object[] { "Subject: x\n \r \nTo: y\n\nbody\n", new int[] { 11 } },
			new object[] { "Subject: x\n \r\r\nTo: y\n\nbody\n", new int[] { 11 } },
			// at the end of the stream
			new object[] { "Subject: x\n  ", new int[] { 11 } },
			// in the headers of a MIME part
			new object[] { "Content-Type: multipart/mixed; boundary=x\n\n--x\nA: b\n \n\nbody\n--x--\n", new int[] { 52 } },
			// not reported: legitimate folds, a blank first line of a header value, or blank lines in the body
			new object[] { "Subject: x\n y\n\nbody\n", Array.Empty<int> () },
			new object[] { "Subject: x\n  y \n\t z\n\nbody\n", Array.Empty<int> () },
			new object[] { "Subject: \n x\n\nbody\n", Array.Empty<int> () },
			new object[] { "Subject:\n x\n\nbody\n", Array.Empty<int> () },
			new object[] { "Subject: x\n\n \nbody\n \n", Array.Empty<int> () },
		};

		static void AssertBlankHeaderContinuationLines (byte[] data, int[] offsets, TestMimeComplianceLogger logger, string label)
		{
			var issues = logger.Issues.Where (x => x.Violation == MimeComplianceViolation.BlankHeaderContinuationLine).ToList ();

			Assert.That (issues.Select (x => x.StreamOffset), Is.EqualTo (offsets.Select (x => (long) x)), $"{label}: {FormatIssues (logger)}");

			foreach (var issue in issues) {
				int offset = (int) issue.StreamOffset;
				int lineNumber = data.Take (offset).Count (c => c == (byte) '\n') + 1;
				int lineEnd = Array.IndexOf (data, (byte) '\n', offset);

				if (lineEnd == -1)
					lineEnd = data.Length;

				Assert.That (offset == 0 || data[offset - 1] == (byte) '\n', Is.True, $"{label}: StreamOffset is at the start of a line");
				Assert.That (lineEnd, Is.GreaterThan (offset), $"{label}: line is not empty");
				for (int i = offset; i < lineEnd; i++)
					Assert.That (data[i] == (byte) ' ' || data[i] == (byte) '\t' || data[i] == (byte) '\r', Is.True, $"{label}: line is blank");

				Assert.That (issue.Severity, Is.EqualTo (MimeComplianceSeverity.Major), $"{label}: Severity");
				Assert.That (issue.LineNumber, Is.EqualTo (lineNumber), $"{label}: LineNumber");
				Assert.That (issue.ColumnNumber, Is.EqualTo (1), $"{label}: ColumnNumber");
				Assert.That (issue.PositionKind, Is.EqualTo (MimeCompliancePositionKind.LineStart), $"{label}: PositionKind");
			}
		}

		[TestCaseSource (nameof (BlankHeaderContinuationLineCases))]
		public void TestMimeComplianceBlankHeaderContinuationLine (string text, int[] offsets)
		{
			var data = Encoding.ASCII.GetBytes (text);

			foreach (var (stream, label) in CreateBareCarriageReturnStreams (data)) {
				using (stream) {
					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (stream) { ComplianceLogger = logger };

					reader.ReadMessage ();

					AssertBlankHeaderContinuationLines (data, offsets, logger, label);
				}
			}
		}

		[TestCaseSource (nameof (BlankHeaderContinuationLineCases))]
		public async Task TestMimeComplianceBlankHeaderContinuationLineAsync (string text, int[] offsets)
		{
			var data = Encoding.ASCII.GetBytes (text);

			foreach (var (stream, label) in CreateBareCarriageReturnStreams (data)) {
				using (stream) {
					var logger = new TestMimeComplianceLogger ();
					var reader = new MimeReader (stream) { ComplianceLogger = logger };

					await reader.ReadMessageAsync ();

					AssertBlankHeaderContinuationLines (data, offsets, logger, label);
				}
			}
		}

		[Test]
		public Task TestMimeComplianceUnexpectedNullBytesInHeaderAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInHeader, 5, 1, MimeCompliancePositionKind.LineStart)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-null-bytes-in-headers.eml", issues);
		}

		[Test]
		public void TestMimeComplianceUnexpectedNullBytesInBody ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInBody, 18, 1),
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 19, 1)
			};

			AssertMimeComplianceViolations ("unexpected-null-bytes-in-body.eml", issues);
		}

		[Test]
		public Task TestMimeComplianceUnexpectedNullBytesInBodyAsync ()
		{
			var issues = new ExpectedMimeComplianceIssue[] {
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.UnexpectedNullBytesInBody, 18, 1),
				new ExpectedMimeComplianceIssue (MimeComplianceViolation.Unexpected8BitBytesInBody, 19, 1)
			};

			return AssertMimeComplianceViolationsAsync ("unexpected-null-bytes-in-body.eml", issues);
		}

		static Stream CreateMissingHeadersStream (string text, bool chunked)
		{
			var data = Encoding.ASCII.GetBytes (text);

			return chunked ? new ChunkedReadStream (data, 7) : new MemoryStream (data, false);
		}

		static void AssertMissingHeaders (TestMimeComplianceLogger logger, int index, long offset, int lineNumber, int columnNumber, MimeCompliancePositionKind kind, params MimeComplianceViolation[] violations)
		{
			Assert.That (logger.Issues.Count, Is.EqualTo (index + violations.Length), FormatIssues (logger));

			for (int i = 0; i < violations.Length; i++) {
				var issue = logger.Issues[index + i];

				Assert.That (issue.Violation, Is.EqualTo (violations[i]), $"Violation #{i}");
				Assert.That (issue.StreamOffset, Is.EqualTo (offset), $"StreamOffset #{i}");
				Assert.That (issue.LineNumber, Is.EqualTo (lineNumber), $"LineNumber #{i}");
				Assert.That (issue.ColumnNumber, Is.EqualTo (columnNumber), $"ColumnNumber #{i}");
				Assert.That (issue.PositionKind, Is.EqualTo (kind), $"PositionKind #{i}");
			}
		}

		const string MissingAllHeadersText = "Subject: missing headers\r\nContent-Type: text/plain\r\n\r\nbody\r\n";

		static void AssertMissingAllHeaders (TestMimeComplianceLogger logger)
		{
			var offset = MissingAllHeadersText.IndexOf ("\r\n\r\n", StringComparison.Ordinal) + 2;

			AssertMissingHeaders (logger, 0, offset, 3, 1, MimeCompliancePositionKind.LineStart,
				MimeComplianceViolation.MissingDate,
				MimeComplianceViolation.MissingFrom,
				MimeComplianceViolation.MissingMessageId,
				MimeComplianceViolation.MissingMimeVersion);
		}

		[Test]
		public void TestMimeComplianceMissingHeaders ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMissingAllHeaders (logger);
			}
		}

		[Test]
		public async Task TestMimeComplianceMissingHeadersAsync ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMissingAllHeaders (logger);
			}
		}

		const string MissingHeadersMboxText = "From sender@example.org Tue Nov 12 09:12:42 2013\r\nSubject: missing headers\r\n\r\nbody\r\n";

		static void AssertMissingHeadersMbox (TestMimeComplianceLogger logger)
		{
			var offset = MissingHeadersMboxText.IndexOf ("\r\n\r\n", StringComparison.Ordinal) + 2;

			// Note: The mbox marker is not a From header and does not count as one.
			AssertMissingHeaders (logger, 0, offset, 3, 1, MimeCompliancePositionKind.LineStart,
				MimeComplianceViolation.MissingDate,
				MimeComplianceViolation.MissingFrom,
				MimeComplianceViolation.MissingMessageId);
		}

		[Test]
		public void TestMimeComplianceMissingHeadersMbox ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingHeadersMboxText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Mbox) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMissingHeadersMbox (logger);
			}
		}

		[Test]
		public async Task TestMimeComplianceMissingHeadersMboxAsync ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingHeadersMboxText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Mbox) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMissingHeadersMbox (logger);
			}
		}

		const string MissingHeadersIncompleteText = "Subject: missing headers";

		static void AssertMissingHeadersIncomplete (TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues.Count, Is.GreaterThan (0), FormatIssues (logger));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.IncompleteHeader), FormatIssues (logger));

			// Note: When the stream ends in the middle of a header, there is no line that ends the header block,
			// so the issues are reported at the exact position where the stream ended.
			AssertMissingHeaders (logger, 1, MissingHeadersIncompleteText.Length, 1, MissingHeadersIncompleteText.Length + 1, MimeCompliancePositionKind.Exact,
				MimeComplianceViolation.MissingDate,
				MimeComplianceViolation.MissingFrom,
				MimeComplianceViolation.MissingMessageId);
		}

		[Test]
		public void TestMimeComplianceMissingHeadersIncompleteHeader ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingHeadersIncompleteText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMissingHeadersIncomplete (logger);
			}
		}

		[Test]
		public async Task TestMimeComplianceMissingHeadersIncompleteHeaderAsync ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (MissingHeadersIncompleteText, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMissingHeadersIncomplete (logger);
			}
		}

		static readonly object[] MissingMimeVersionCases = {
			new object[] { "Subject: not a MIME message\r\n", false },
			new object[] { "Content-Disposition: inline\r\n", false },
			new object[] { "Content-Type: text/plain\r\n", true },
			new object[] { "Content-Transfer-Encoding: 7bit\r\n", true },
		};

		const string MessageHeadersWithoutMimeVersion = "From: mimekit@example.org\r\nDate: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <123@example.org>\r\n";

		static void AssertMissingMimeVersion (TestMimeComplianceLogger logger, string text, bool expected)
		{
			if (expected) {
				var offset = text.IndexOf ("\r\n\r\n", StringComparison.Ordinal) + 2;

				AssertMissingHeaders (logger, 0, offset, 5, 1, MimeCompliancePositionKind.LineStart, MimeComplianceViolation.MissingMimeVersion);
			} else {
				Assert.That (logger.Issues.Count, Is.EqualTo (0), FormatIssues (logger));
			}
		}

		[TestCaseSource (nameof (MissingMimeVersionCases))]
		public void TestMimeComplianceMissingMimeVersion (string header, bool expected)
		{
			var text = MessageHeadersWithoutMimeVersion + header + "\r\nbody\r\n";

			using (var stream = CreateMissingHeadersStream (text, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertMissingMimeVersion (logger, text, expected);
			}
		}

		[TestCaseSource (nameof (MissingMimeVersionCases))]
		public async Task TestMimeComplianceMissingMimeVersionAsync (string header, bool expected)
		{
			var text = MessageHeadersWithoutMimeVersion + header + "\r\nbody\r\n";

			using (var stream = CreateMissingHeadersStream (text, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertMissingMimeVersion (logger, text, expected);
			}
		}

		// Note: Embedded messages are frequently trimmed copies of the original message, so only the
		// top-level message is checked for missing headers.
		const string EmbeddedMessageWithoutHeaders = MessageHeaders + "Content-Type: message/rfc822\r\n\r\nSubject: embedded\r\nContent-Type: text/plain\r\n\r\nbody\r\n";

		[Test]
		public void TestMimeComplianceMissingHeadersIgnoresEmbeddedMessages ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (EmbeddedMessageWithoutHeaders, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadMessage ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), FormatIssues (logger));
			}
		}

		[Test]
		public async Task TestMimeComplianceMissingHeadersIgnoresEmbeddedMessagesAsync ([Values (false, true)] bool chunked)
		{
			using (var stream = CreateMissingHeadersStream (EmbeddedMessageWithoutHeaders, chunked)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), FormatIssues (logger));
			}
		}

		[Test]
		public void TestMimeComplianceMissingHeadersIgnoresEntitiesAndHeaders ()
		{
			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadEntity ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ReadEntity: " + FormatIssues (logger));
			}

			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				reader.ReadHeaders ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ReadHeaders: " + FormatIssues (logger));
			}
		}

		[Test]
		public async Task TestMimeComplianceMissingHeadersIgnoresEntitiesAndHeadersAsync ()
		{
			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadEntityAsync ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ReadEntityAsync: " + FormatIssues (logger));
			}

			using (var stream = CreateMissingHeadersStream (MissingAllHeadersText, false)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Entity) { ComplianceLogger = logger };

				await reader.ReadHeadersAsync ();

				Assert.That (logger.Issues.Count, Is.EqualTo (0), "ReadHeadersAsync: " + FormatIssues (logger));
			}
		}
	}
}