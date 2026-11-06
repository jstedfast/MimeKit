//
// MimeReaderScanContentTests.cs
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
using System.Runtime.Intrinsics;

using MimeKit;

using UnitTests.IO;

namespace UnitTests {
	// Tests for the vectorized "skip lines that cannot be a boundary" fast path used by MimeReader.ScanContent().
	[TestFixture]
	public class MimeReaderScanContentTests
	{
		static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "mbox");

		public enum SkipKernel
		{
			Scalar,
			Vector128,
			Vector256
		}

		unsafe delegate byte* SkipFunc (byte* inptr, byte* inend, byte c1, byte c2, out int lines, out bool dos, out bool unix);

		static unsafe SkipFunc GetKernel (SkipKernel kernel)
		{
			switch (kernel) {
			case SkipKernel.Vector256:
				if (!Vector256.IsHardwareAccelerated)
					Assert.Ignore ("Vector256 is not hardware accelerated on this machine.");
				return MimeReader.SkipNonBoundaryLinesVector256;
			case SkipKernel.Vector128:
				if (!Vector128.IsHardwareAccelerated)
					Assert.Ignore ("Vector128 is not hardware accelerated on this machine.");
				return MimeReader.SkipNonBoundaryLinesVector128;
			default:
				return MimeReader.SkipNonBoundaryLinesScalar;
			}
		}

		// A straight-forward reference implementation of the per-line logic that the kernels must replicate.
		static int Reference (byte[] buffer, int start, int end, byte c1, byte c2, out int lines, out bool dos, out bool unix)
		{
			int resume = start;

			lines = 0;
			dos = unix = false;

			for (int i = start; i < end; i++) {
				if (buffer[i] != (byte) '\n')
					continue;

				if (i > start && buffer[i - 1] == (byte) '\r')
					dos = true;
				else
					unix = true;

				resume = i + 1;
				lines++;

				if (i + 2 < end && ((buffer[i + 1] == (byte) '-' && buffer[i + 2] == (byte) '-') || (buffer[i + 1] == c1 && buffer[i + 2] == c2)))
					break;
			}

			return resume;
		}

		static unsafe void AssertKernel (SkipFunc kernel, byte[] buffer, int start, int end, byte c1, byte c2, string label)
		{
			Assert.That (buffer[end], Is.EqualTo ((byte) '\n'), "sentinel");

			int expected = Reference (buffer, start, end, c1, c2, out int expectedLines, out bool expectedDos, out bool expectedUnix);

			fixed (byte* buf = buffer) {
				byte* resume = kernel (buf + start, buf + end, c1, c2, out int lines, out bool dos, out bool unix);
				int actual = (int) (resume - buf);

				if (actual != expected || lines != expectedLines || dos != expectedDos || unix != expectedUnix) {
					var text = Encoding.Latin1.GetString (buffer, start, end - start).Replace ("\r", "\\r").Replace ("\n", "\\n");

					Assert.Fail ($"{label}: c1c2='{(char) c1}{(char) c2}' input=\"{text}\"\n" +
						$"expected: resume={expected - start} lines={expectedLines} dos={expectedDos} unix={expectedUnix}\n" +
						$"actual:   resume={actual - start} lines={lines} dos={dos} unix={unix}");
				}
			}
		}

		static readonly byte[] FuzzAlphabet = Encoding.ASCII.GetBytes ("\r\n\n--Fraaaaaaaaaaxyz");

		[Test]
		public void TestSkipNonBoundaryLinesFuzz ([Values] SkipKernel kind)
		{
			var kernel = GetKernel (kind);
			var random = new Random (1337);

			for (int iteration = 0; iteration < 50000; iteration++) {
				int prefix = random.Next (0, 4);
				int length = random.Next (0, 300);
				var buffer = new byte[prefix + length + 4];
				int end = prefix + length;

				for (int i = 0; i < buffer.Length; i++)
					buffer[i] = FuzzAlphabet[random.Next (FuzzAlphabet.Length)];

				// make lines longer on average in some iterations so that multiple vectors are scanned between linefeeds
				if ((iteration & 1) == 0) {
					for (int i = prefix; i < end; i++) {
						if (buffer[i] == (byte) '\n' && random.Next (4) != 0)
							buffer[i] = (byte) 'b';
					}
				}

				buffer[end] = (byte) '\n';

				bool mbox = (iteration & 2) != 0;
				byte c1 = mbox ? (byte) 'F' : (byte) '-';
				byte c2 = mbox ? (byte) 'r' : (byte) '-';

				AssertKernel (kernel, buffer, prefix, end, c1, c2, $"iteration {iteration}");
			}
		}

		// Exhaustively place a pair of line endings (each either "\n" or "\r\n") followed by a possible boundary
		// prefix at every combination of offsets so that every lane position (and every crossing of a vector
		// boundary) is covered for both the linefeed and carriage-return masks.
		[Test]
		public void TestSkipNonBoundaryLinesSweep ([Values] SkipKernel kind, [Values ("--", "Fr", "-x", "F-", "-")] string next)
		{
			var kernel = GetKernel (kind);
			var prefixBytes = Encoding.ASCII.GetBytes (next);
			const int Length = 80;

			for (int crlf = 0; crlf < 4; crlf++) {
				for (int first = 0; first < Length; first++) {
					for (int second = first + 1; second <= Length; second++) {
						var buffer = new byte[Length + 4];
						int end = second == Length ? Length : Length - 1;

						buffer.AsSpan ().Fill ((byte) 'a');
						buffer[first] = (byte) '\n';
						if ((crlf & 1) != 0 && first > 0)
							buffer[first - 1] = (byte) '\r';

						if (second < end) {
							buffer[second] = (byte) '\n';
							if ((crlf & 2) != 0 && second - 1 > first)
								buffer[second - 1] = (byte) '\r';

							for (int i = 0; i < prefixBytes.Length && second + 1 + i < end; i++)
								buffer[second + 1 + i] = prefixBytes[i];
						}

						buffer[end] = (byte) '\n';

						AssertKernel (kernel, buffer, 0, end, (byte) '-', (byte) '-', $"first={first} second={second} crlf={crlf}");
						AssertKernel (kernel, buffer, 0, end, (byte) 'F', (byte) 'r', $"first={first} second={second} crlf={crlf}");
					}
				}
			}
		}

		[Test]
		public void TestSkipNonBoundaryLinesEdgeCases ([Values] SkipKernel kind)
		{
			var kernel = GetKernel (kind);
			var inputs = new string[] {
				"",
				"\n",
				"\r\n",
				"abc",
				"abc\n",
				"abc\r\n--",
				"abc\r\n-",
				"abc\r\n--boundary\r\n",
				"\n--",
				"\n\n\n\n",
				"\r\r\r\n",
				"\r",
				"abc\r",
				"line\r\nFrom someone\r\nmore\r\n",
				"line\nFrom someone\nmore\n",
				"line\n---\n",
				"line\n-x\nline\n",
				new string ('a', 31) + "\n--" + new string ('a', 40),
				new string ('a', 32) + "\n--" + new string ('a', 40),
				new string ('a', 15) + "\n--" + new string ('a', 40),
				new string ('a', 30) + "\r\n" + new string ('a', 40) + "\n",
				new string ('a', 31) + "\r\n" + new string ('a', 40) + "\n",
				new string ('a', 15) + "\r\n" + new string ('a', 40) + "\n",
				new string ('a', 100) + "\n",
				new string ('a', 100) + "\n-",
				new string ('a', 100) + "\n--",
				new string ('a', 1000),
				"\n" + new string ('a', 1000),
				new string ('a', 127) + "\r\n--boundary\n" + new string ('a', 40),
				new string ('a', 127) + "\r\n" + new string ('a', 127) + "\n" + new string ('a', 40),
				"short\r\n" + new string ('a', 1024) + "\r\n--boundary\r\n" + new string ('a', 40),
				"short\r\n" + new string ('a', 1024) + "\r\nshort\r\n" + new string ('a', 40),
				"short\r\n" + new string ('a', 1024) + "\r\n" + new string ('a', 1024) + "\nshort\n",
			};

			foreach (var input in inputs) {
				var data = Encoding.ASCII.GetBytes (input);

				for (int prefix = 0; prefix < 3; prefix++) {
					var buffer = new byte[prefix + data.Length + 4];

					// a '\r' immediately before the starting point must not be treated as part of the first line ending
					buffer.AsSpan (0, prefix).Fill ((byte) '\r');
					data.CopyTo (buffer, prefix);
					buffer[prefix + data.Length] = (byte) '\n';
					buffer.AsSpan (prefix + data.Length + 1).Fill ((byte) '-');

					AssertKernel (kernel, buffer, prefix, prefix + data.Length, (byte) '-', (byte) '-', $"prefix={prefix}");
					AssertKernel (kernel, buffer, prefix, prefix + data.Length, (byte) 'F', (byte) 'r', $"prefix={prefix}");
				}
			}
		}

		// Records every callback (with its arguments) so that the fast path can be compared against the per-line path.
		class RecordingReader : MimeReader
		{
			public readonly List<string> Events = new List<string> ();

			public RecordingReader (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			void Add (string name, params object[] args)
			{
				Events.Add (name + " " + string.Join (", ", args.Select (x => x?.ToString () ?? "null")));
			}

			static string Text (byte[] buffer, int startIndex, int count) => Encoding.Latin1.GetString (buffer, startIndex, count);
			static string Type (ContentType contentType) => contentType.MimeType;

			protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken) => Add (nameof (OnMboxMarkerBegin), beginOffset, lineNumber);
			protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => Add (nameof (OnMboxMarkerRead), Text (buffer, startIndex, count));
			protected override void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken) => Add (nameof (OnMboxMarkerEnd), beginOffset, lineNumber, endOffset);
			protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnHeadersBegin), beginOffset, beginLineNumber);
			protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnHeaderRead), header.Field, Encoding.Latin1.GetString (header.RawValue), header.Offset, beginLineNumber);
			protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken) => Add (nameof (OnHeadersEnd), beginOffset, beginLineNumber, endOffset, endLineNumber);
			protected override void OnBodySeparator (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken) => Add (nameof (OnBodySeparator), beginOffset, lineNumber, endOffset);
			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMimeMessageBegin), beginOffset, beginLineNumber);
			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMimeMessageEnd), beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMimePartBegin), Type (contentType), beginOffset, beginLineNumber);
			protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMimePartContentBegin), beginOffset, beginLineNumber);
			protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => Add (nameof (OnMimePartContentRead), Text (buffer, startIndex, count));
			protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken) => Add (nameof (OnMimePartContentEnd), beginOffset, beginLineNumber, endOffset, lines, newLineFormat);
			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMimePartEnd), Type (contentType), beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMessagePartBegin), Type (contentType), beginOffset, beginLineNumber);
			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMessagePartEnd), Type (contentType), beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartBegin), Type (contentType), beginOffset, beginLineNumber);
			protected override void OnMultipartBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartBoundaryBegin), beginOffset, lineNumber);
			protected override void OnMultipartBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartBoundaryRead), Text (buffer, startIndex, count), beginOffset, lineNumber);
			protected override void OnMultipartBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken) => Add (nameof (OnMultipartBoundaryEnd), beginOffset, lineNumber, endOffset);
			protected override void OnMultipartEndBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartEndBoundaryBegin), beginOffset, lineNumber);
			protected override void OnMultipartEndBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartEndBoundaryRead), Text (buffer, startIndex, count), beginOffset, lineNumber);
			protected override void OnMultipartEndBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken) => Add (nameof (OnMultipartEndBoundaryEnd), beginOffset, lineNumber, endOffset);
			protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartPreambleBegin), beginOffset, beginLineNumber);
			protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => Add (nameof (OnMultipartPreambleRead), Text (buffer, startIndex, count));
			protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMultipartPreambleEnd), beginOffset, beginLineNumber, endOffset, lines);
			protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken) => Add (nameof (OnMultipartEpilogueBegin), beginOffset, beginLineNumber);
			protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken) => Add (nameof (OnMultipartEpilogueRead), Text (buffer, startIndex, count));
			protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMultipartEpilogueEnd), beginOffset, beginLineNumber, endOffset, lines);
			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken) => Add (nameof (OnMultipartEnd), Type (contentType), beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
		}

		sealed class NullComplianceLogger : IMimeComplianceLogger
		{
			public void Log (in MimeComplianceIssue issue)
			{
			}
		}

		// Compare the unvalidated fast path with the original, per-line compliance path.
		static List<string> Record (byte[] data, MimeFormat format, int chunkSize, bool fastPath)
		{
			using (var stream = new ChunkedReadStream (data, chunkSize)) {
				var reader = new RecordingReader (stream, format);

				if (!fastPath) {
					reader.ComplianceLogger = new NullComplianceLogger ();
					reader.MaxContentVectorSize = 0;
				}

				try {
					do {
						reader.ReadMessage ();
					} while (format == MimeFormat.Mbox && !reader.IsEndOfStream);
				} catch (Exception ex) {
					reader.Events.Add (ex.GetType ().Name + ": " + ex.Message);
				}

				return reader.Events;
			}
		}

		static async Task<List<string>> RecordAsync (byte[] data, MimeFormat format, int chunkSize, bool fastPath)
		{
			using (var stream = new ChunkedReadStream (data, chunkSize)) {
				var reader = new RecordingReader (stream, format);

				if (!fastPath) {
					reader.ComplianceLogger = new NullComplianceLogger ();
					reader.MaxContentVectorSize = 0;
				}

				try {
					do {
						await reader.ReadMessageAsync ();
					} while (format == MimeFormat.Mbox && !reader.IsEndOfStream);
				} catch (Exception ex) {
					reader.Events.Add (ex.GetType ().Name + ": " + ex.Message);
				}

				return reader.Events;
			}
		}

		static void AssertEventsEqual (List<string> expected, List<string> actual, string label)
		{
			int count = Math.Min (expected.Count, actual.Count);

			for (int i = 0; i < count; i++) {
				if (expected[i] != actual[i])
					Assert.Fail ($"{label}: event #{i} differs\nexpected: {Truncate (expected[i])}\nactual:   {Truncate (actual[i])}");
			}

			Assert.That (actual, Has.Count.EqualTo (expected.Count), $"{label}: event count");
		}

		static string Truncate (string text)
		{
			text = text.Replace ("\r", "\\r").Replace ("\n", "\\n");

			return text.Length > 400 ? text.Substring (0, 400) + "..." : text;
		}

		static IEnumerable<(string Name, MimeFormat Format)> CorpusFiles ()
		{
			foreach (var path in Directory.GetFiles (MessagesDataDir).OrderBy (x => x, StringComparer.Ordinal))
				yield return (path, MimeFormat.Entity);

			foreach (var path in Directory.GetFiles (MboxDataDir).OrderBy (x => x, StringComparer.Ordinal))
				yield return (path, MimeFormat.Mbox);
		}

		[Test]
		public void TestCorpusFastPathMatchesPerLinePath ([Values (1021, 4096)] int chunkSize)
		{
			foreach (var (path, format) in CorpusFiles ()) {
				var data = File.ReadAllBytes (path);
				var expected = Record (data, format, chunkSize, false);
				var actual = Record (data, format, chunkSize, true);

				AssertEventsEqual (expected, actual, Path.GetFileName (path));
			}
		}

		[Test]
		public async Task TestCorpusFastPathMatchesPerLinePathAsync ([Values (1021, 4096)] int chunkSize)
		{
			foreach (var (path, format) in CorpusFiles ()) {
				var data = File.ReadAllBytes (path);
				var expected = await RecordAsync (data, format, chunkSize, false);
				var actual = await RecordAsync (data, format, chunkSize, true);

				AssertEventsEqual (expected, actual, Path.GetFileName (path));
			}
		}

		static readonly string[] BodyLines = {
			"", "a", "-", "--", "---", "-x", "--x", "--b", "--b--", "--b ", "--b--  ", "--bb", "--inner", "--inner--",
			"From ", "From someone@example.com", ">From escaped", "Fr", "F", "From",
			"YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXphYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5emFiY2RlZmdoaWprbG1u",
			"short line of text", new string ('x', 31), new string ('y', 32), new string ('z', 63), new string ('w', 997),
			new string ('v', 1100), "trailing\r", "\r",
		};

		static string NewLine (Random random)
		{
			switch (random.Next (8)) {
			case 0: return "\n";
			default: return "\r\n";
			}
		}

		static void AppendBody (StringBuilder builder, Random random)
		{
			int lines = random.Next (0, 40);

			for (int i = 0; i < lines; i++) {
				if (random.Next (3) == 0)
					builder.Append (BodyLines[random.Next (BodyLines.Length)]);
				else
					builder.Append (BodyLines[20], 0, random.Next (BodyLines[20].Length));

				builder.Append (NewLine (random));
			}
		}

		static string GenerateMessage (Random random, int depth = 0)
		{
			var builder = new StringBuilder ();
			string nl = NewLine (random);

			builder.Append ("From: mimekit@example.org").Append (nl);

			if (depth < 2 && random.Next (3) != 0) {
				string boundary = depth == 0 ? "b" : "inner";

				builder.Append ($"Content-Type: multipart/mixed; boundary=\"{boundary}\"").Append (nl).Append (nl);
				AppendBody (builder, random);

				int parts = random.Next (0, 4);
				for (int i = 0; i < parts; i++) {
					builder.Append ("--").Append (boundary).Append (random.Next (4) == 0 ? "  " : "").Append (NewLine (random));

					if (random.Next (4) == 0) {
						builder.Append ("Content-Type: message/rfc822").Append (nl).Append (nl);
						builder.Append (GenerateMessage (random, depth + 1));
					} else if (random.Next (3) == 0) {
						builder.Append (GenerateMessage (random, depth + 1).Substring ("From: mimekit@example.org".Length + nl.Length));
					} else {
						builder.Append ("Content-Type: text/plain").Append (nl).Append (nl);
						AppendBody (builder, random);
					}
				}

				if (random.Next (4) != 0) {
					builder.Append ("--").Append (boundary).Append ("--").Append (NewLine (random));
					AppendBody (builder, random);
				}
			} else {
				builder.Append ("Content-Type: text/plain").Append (nl).Append (nl);
				AppendBody (builder, random);
			}

			return builder.ToString ();
		}

		static byte[] GenerateMbox (Random random)
		{
			var builder = new StringBuilder ();
			int count = random.Next (1, 4);

			for (int i = 0; i < count; i++) {
				builder.Append ("From mimekit@example.org Fri Jan  1 00:00:00 2026").Append (NewLine (random));
				builder.Append (GenerateMessage (random));
			}

			return Encoding.Latin1.GetBytes (builder.ToString ());
		}

		static readonly int[] ChunkSizes = { 1, 2, 3, 7, 31, 33, 64, 4096 };

		static async Task<(List<string> Events, List<MimeComplianceIssue> Issues)> RecordValidated (byte[] data, MimeFormat format,
			int chunkSize, int vectorSize, int maxIssues, bool doAsync)
		{
			using var stream = new ChunkedReadStream (data, chunkSize);
			var logger = new TestMimeComplianceLogger ();
			var reader = new RecordingReader (stream, format) {
				ComplianceLogger = logger,
				MaxContentVectorSize = (byte) vectorSize
			};
			reader.ComplianceOptions.EnabledValidators = MimeComplianceValidators.All;
			reader.ComplianceOptions.MaxIssuesPerViolation = maxIssues;

			try {
				do {
					if (doAsync)
						await reader.ReadMessageAsync ();
					else
						reader.ReadMessage ();
				} while (format == MimeFormat.Mbox && !reader.IsEndOfStream);
			} catch (FormatException ex) {
				reader.Events.Add (ex.GetType ().Name + ": " + ex.Message);
			}

			return (reader.Events, logger.Issues);
		}

		static async Task AssertValidatedPaths (byte[] data, MimeFormat format, int chunkSize, int maxIssues, bool doAsync, string label)
		{
			var expected = await RecordValidated (data, format, chunkSize, 0, maxIssues, doAsync);

			foreach (int vectorSize in new[] { 16, 32 }) {
				if (vectorSize == 32 ? !Vector256.IsHardwareAccelerated : !Vector128.IsHardwareAccelerated)
					continue;

				var actual = await RecordValidated (data, format, chunkSize, vectorSize, maxIssues, doAsync);
				AssertEventsEqual (expected.Events, actual.Events, label);
				Assert.That (actual.Issues, Is.EqualTo (expected.Issues), $"{label}: vectorSize={vectorSize}, issues");
			}
		}

		[Test]
		public async Task TestValidatedCorpusMatchesPerLinePath ([Values (false, true)] bool doAsync, [Values (0, 1)] int maxIssues)
		{
			foreach (var (path, format) in CorpusFiles ()) {
				var data = File.ReadAllBytes (path);
				await AssertValidatedPaths (data, format, 4096, maxIssues, doAsync, Path.GetFileName (path));
			}
		}

		[Test]
		public async Task TestValidatedGeneratedMessagesMatchPerLinePath ([Values (false, true)] bool doAsync)
		{
			var random = new Random (5271);

			for (int i = 0; i < 300; i++) {
				var format = (i & 1) == 0 ? MimeFormat.Entity : MimeFormat.Mbox;
				var data = format == MimeFormat.Mbox ? GenerateMbox (random) : Encoding.Latin1.GetBytes (GenerateMessage (random));

				if (random.Next (4) == 0)
					data = data.AsSpan (0, random.Next (data.Length + 1)).ToArray ();

				await AssertValidatedPaths (data, format, ChunkSizes[i % ChunkSizes.Length], i % 3 == 0 ? 1 : 0, doAsync, $"message {i}");
			}
		}

		[Test]
		public async Task TestValidatedContentEdges ([Values (false, true)] bool doAsync,
			[Values ("7bit", "8bit", "binary", "base64", "quoted-printable", "x-uuencode")] string encoding)
		{
			string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: text/plain\r\n" +
				"Content-Transfer-Encoding: " + encoding + "\r\n\r\n";
			string prefix = string.Concat (Enumerable.Repeat (new string ('A', 76) + "\r\n", 60));
			var endings = new[] { "\0\r\n", "\u00ff\r\n", "\n", "\r\n--b--\r\n", "\r\n--b--", "\r", "",
				"AA==\r\n", "AB==\r\n", "AAA=\r\n", "AAB=\r\n", "A\r\n", "AA\r\n", "AAA\r\n",
				"AA==\r\nAAAA\r\n", "* comment *\r\n", "=0G\r\n", "=\rX\r\n", "=", "=A" };

			foreach (int padding in new[] { 0, 1, 14, 15, 16, 30, 31, 32, 63, 997, 998, 999, 1000, 4095 }) {
				foreach (string ending in endings) {
					var data = Encoding.Latin1.GetBytes (header + prefix + new string ('A', padding) + ending);
					foreach (int chunkSize in new[] { 31, 64, 1021, 4096 })
						await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, 0, doAsync, $"{encoding}/{padding}/{ending}/{chunkSize}");
				}
			}
		}

		[Test]
		public async Task TestValidatedExaminedTail ([Values (false, true)] bool doAsync,
			[Values ("7bit", "binary", "base64")] string encoding)
		{
			string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: text/plain\r\n" +
				"Content-Transfer-Encoding: " + encoding + "\r\n\r\n";
			string prefix = string.Concat (Enumerable.Repeat (new string ('A', 76) + "\r\n", 60));
			var endings = new[] { "\0\u00ff\r\n", "\u00ff\0\n", "\n", "\r\n", "AB==\r\n", "*\r\n", "", "\r" };

			foreach (int width in new[] { 80, 998, 1001, 8192 }) {
				foreach (string ending in endings) {
					for (int padding = 0; padding < 32; padding++) {
						string body = new string ('A', padding) + "\r\n" + prefix + new string ('A', width) + ending;
						foreach (string boundary in new[] { "", "--b--\r\n" }) {
							var data = Encoding.Latin1.GetBytes (header + body + boundary);
							foreach (int chunkSize in new[] { 64, 1021, 4096 })
								await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, 0, doAsync, $"tail/{encoding}/{width}/{padding}/{ending}/{boundary}/{chunkSize}");
						}
					}
				}
			}
		}

		[Test]
		public async Task TestValidatedPreviousLinePosition ([Values (false, true)] bool doAsync,
			[Values ("7bit", "base64")] string encoding)
		{
			string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: text/plain\r\n" +
				"Content-Transfer-Encoding: " + encoding + "\r\n\r\n";

			foreach (int width in new[] { 0, 1, 14, 15, 16, 30, 31, 32, 76, 997, 998 }) {
				for (int padding = 0; padding < 32; padding++) {
					string body = new string ('A', padding) + "\r\n" +
						string.Concat (Enumerable.Repeat (new string ('A', width) + "\r\n", 12));
					var data = Encoding.ASCII.GetBytes (header + body + "--b--\r\n");

					foreach (int chunkSize in new[] { 33, 64, 1021, 4096 })
						await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, 0, doAsync, $"previous/{encoding}/{width}/{padding}/{chunkSize}");
				}
			}
		}

		[Test]
		public async Task TestValidatedQuotedPrintable ([Values (false, true)] bool doAsync, [Values (0, 1)] int maxIssues)
		{
			const string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: text/plain\r\n" +
				"Content-Transfer-Encoding: quoted-printable\r\n\r\n";
			var lines = new[] {
				new string ('x', 76), "literal=20text=3Dvalue", "=c3=a9=E6=97=A5=E6=9C=AC",
				new string ('x', 72) + "=", "=41=42=43=", "", "=0G", "=\rX"
			};
			var endings = new[] { "=0G\r\n", "=\rX\r\n", "=A\n", "=\n", "=\r", "=", "=A",
				"\0\u00ff\r\n", "\r\n--b--\r\n", "" };

			foreach (string line in lines) {
				string prefix = string.Concat (Enumerable.Repeat (line + "\r\n", 70));
				for (int padding = 0; padding < 32; padding++) {
					foreach (string ending in endings) {
						var data = Encoding.Latin1.GetBytes (header + new string ('x', padding) + prefix + ending);
						foreach (int chunkSize in new[] { 33, 64, 1021, 4096 })
							await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, maxIssues, doAsync, $"qp/{line}/{padding}/{ending}/{chunkSize}");
					}
				}
			}
		}

		[Test]
		public async Task TestValidatedQuotedPrintableEscapeBytes ([Values (false, true)] bool doAsync)
		{
			const string header = "Content-Type: text/plain\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n";

			for (int value = 0; value < 256; value++) {
				for (int padding = 0; padding < 32; padding++) {
					string escapes = "=" + (char) value + "A\r\n=A" + (char) value + "\r\n";
					var data = Encoding.Latin1.GetBytes (header + new string ('x', padding) + "\r\n" + string.Concat (Enumerable.Repeat (escapes, 400)));
					await AssertValidatedPaths (data, MimeFormat.Entity, 4096, 0, doAsync, $"qp-byte/{value}/{padding}");
				}
			}
		}

		[Test]
		public async Task TestValidatedQuotedPrintableBoundaryCandidates ([Values (false, true)] bool doAsync,
			[Values (0, 1)] int maxIssues, [Values ("\r\n", "\n")] string newline)
		{
			const string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: text/plain\r\n" +
				"Content-Transfer-Encoding: quoted-printable\r\n\r\n";
			string clean = string.Concat (Enumerable.Repeat ("text=20=c3=a9=E6=97=A5=\r\n", 12));
			var candidates = new[] { "---\r\n", "--bX=20value\r\n", "--b--X=\r\n", "---=0G\r\n", "---=\rX\r\n",
				"---=A\r\n", "---=A\n", "---\0\u00ff\r\n", "---" + new string ('x', 998) + "\r\n" };

			foreach (string candidate in candidates) {
				for (int padding = 0; padding < 32; padding++) {
					string body = new string ('x', padding) + "\r\n" + clean + candidate + clean + candidate + clean +
						"=0G\r\n---\r\n=\rX\r\n\0\u00ff\n" + clean;
					foreach (string ending in new[] { "--b--\r\n", "---=", "---=\r", "---=A", "--b" }) {
						var data = Encoding.Latin1.GetBytes ((header + body + ending).Replace ("\r\n", newline));
						foreach (int chunkSize in new[] { 1, 33, 64, 1021, 4096 })
							await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, maxIssues, doAsync, $"qp-boundary/{candidate}/{padding}/{ending}/{chunkSize}");
					}
				}
			}
		}

		[Test]
		public async Task TestValidatedUU ([Values (false, true)] bool doAsync, [Values (0, 1)] int maxIssues)
		{
			const string header = "Content-Type: multipart/mixed; boundary=b\r\n\r\n--b\r\nContent-Type: application/octet-stream\r\n" +
				"Content-Transfer-Encoding: x-uuencode\r\n\r\nbegin 644 file.bin\r\n";
			string prefix = string.Concat (Enumerable.Repeat ("M" + new string ('A', 60) + "\r\n", 80));
			var endings = new[] { "`\r\nend\r\n", "`\r\nend", "", "M!!!\r\n", "M" + new string ('A', 61) + "\r\n",
				" \r\n`\r\nend\r\n", "\0\u00ff\r\n", "\r\n--b--\r\n", "`\r\nend\r\n--b--\r\n", "M!!!\n" };

			for (int decoded = 1; decoded <= 45; decoded++) {
				string line = (char) (32 + decoded) + new string ('!', 4 * ((decoded + 2) / 3)) + "\r\n";
				foreach (string ending in endings) {
					var data = Encoding.Latin1.GetBytes (header + prefix + string.Concat (Enumerable.Repeat (line, 80)) + ending);
					foreach (int chunkSize in new[] { 33, 64, 1021, 4096 })
						await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, maxIssues, doAsync, $"uu/{decoded}/{ending}/{chunkSize}");
				}
			}
		}

		[Test]
		public async Task TestValidatedUUPayloadBytes ([Values (false, true)] bool doAsync)
		{
			const string header = "Content-Type: application/octet-stream\r\nContent-Transfer-Encoding: x-uuencode\r\n\r\nbegin 644 file.bin\r\n";
			string prefix = string.Concat (Enumerable.Repeat ("M" + new string ('A', 60) + "\r\n", 80));

			for (int value = 0; value < 256; value++) {
				foreach (int position in new[] { 0, 15, 16, 28, 31, 32, 44, 47, 48, 59 }) {
					string line = "M" + new string ('A', position) + (char) value + new string ('A', 59 - position) + "\r\n";
					var data = Encoding.Latin1.GetBytes (header + prefix + string.Concat (Enumerable.Repeat (line, 80)) + "`\r\nend\r\n");
					await AssertValidatedPaths (data, MimeFormat.Entity, 4096, 0, doAsync, $"uu-byte/{value}/{position}");
				}
			}
		}

		[Test]
		public async Task TestValidatedEncodedMbox ([Values (false, true)] bool doAsync,
			[Values ("quoted-printable", "x-uuencode")] string encoding)
		{
			const string marker = "From mimekit@example.org Fri Jan  1 00:00:00 2026\r\n";
			string header = "From: mimekit@example.org\r\nContent-Type: text/plain\r\nContent-Transfer-Encoding: " + encoding + "\r\n\r\n";
			string body = encoding == "quoted-printable"
				? string.Concat (Enumerable.Repeat ("literal=20text=3Dvalue=\r\n", 300))
				: "begin 644 file.bin\r\n" + string.Concat (Enumerable.Repeat ("M" + new string ('A', 60) + "\r\n", 100)) + "`\r\nend\r\n";

			for (int padding = 0; padding < 32; padding++) {
				var data = Encoding.ASCII.GetBytes (marker + header + new string (' ', padding) + "\r\n" + body + marker + header + body);
				foreach (int chunkSize in new[] { 33, 1021, 4096 })
					await AssertValidatedPaths (data, MimeFormat.Mbox, chunkSize, 0, doAsync, $"encoded-mbox/{encoding}/{padding}/{chunkSize}");
			}
		}

		[Test]
		public async Task TestValidatedBase64Mbox ([Values (false, true)] bool doAsync, [Values (0, 1)] int maxIssues)
		{
			const string marker = "From mimekit@example.org Fri Jan  1 00:00:00 2026\r\n";
			const string header = "From: mimekit@example.org\r\nContent-Type: application/octet-stream\r\nContent-Transfer-Encoding: base64\r\n\r\n";
			string prefix = string.Concat (Enumerable.Repeat (new string ('Z', 76) + "\r\n", 60));

			for (int padding = 0; padding < 64; padding++) {
				var data = Encoding.ASCII.GetBytes (marker + header + prefix + new string ('Z', padding) + "\r\n" +
					marker + header + prefix + "AA==\r\n");

				foreach (int chunkSize in new[] { 33, 1021, 4096 })
					await AssertValidatedPaths (data, MimeFormat.Mbox, chunkSize, maxIssues, doAsync, $"mbox/{padding}/{chunkSize}");
			}
		}

		[Test]
		public async Task TestValidatedQuotedPrintableMboxCandidates ([Values (false, true)] bool doAsync,
			[Values (0, 1)] int maxIssues, [Values ("\r\n", "\n")] string newline)
		{
			const string marker = "From mimekit@example.org Fri Jan  1 00:00:00 2026\r\n";
			const string header = "From: mimekit@example.org\r\nContent-Type: text/plain\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n";
			string clean = string.Concat (Enumerable.Repeat ("literal=20text=3Dvalue=\r\n", 12));
			string body = clean + "Fromage=20value\r\n" + clean + "---=\r\n" + clean +
				"Fro=0G\r\n" + clean + "=\rX\r\n\0\u00ff\n";

			for (int padding = 0; padding < 32; padding++) {
				var data = Encoding.Latin1.GetBytes ((marker + header + new string ('x', padding) + "\r\n" + body + marker + header + body).Replace ("\r\n", newline));
				foreach (int chunkSize in new[] { 1, 33, 64, 1021, 4096 })
					await AssertValidatedPaths (data, MimeFormat.Mbox, chunkSize, maxIssues, doAsync, $"qp-mbox/{padding}/{chunkSize}");
			}
		}

		[Test]
		public async Task TestValidatedBase64PartialQuantum ([Values (false, true)] bool doAsync)
		{
			const string header = "Content-Type: application/octet-stream\r\nContent-Transfer-Encoding: base64\r\n\r\n";
			const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

			foreach (int width in new[] { 1, 2, 3, 15, 16, 31, 32, 75, 76, 77, 998, 999 }) {
				var text = new StringBuilder (header);
				for (int i = 0; i < 100; i++)
					text.Append (new string (alphabet[i % alphabet.Length], width)).Append ("\t \r\n");

				foreach (string suffix in new[] { "=\r\n", "==\r\n", "\r\n", "AA==\r\nA\r\n", "\0\u00ff\r\n", "*\r\n" }) {
					var data = Encoding.Latin1.GetBytes (text.ToString () + suffix);
					foreach (int chunkSize in new[] { 64, 1021, 4096 })
						await AssertValidatedPaths (data, MimeFormat.Entity, chunkSize, 0, doAsync, $"quantum/{width}/{suffix}/{chunkSize}");
				}
			}
		}

		static byte[] CreateByteDetectionMessage (string encoding, int padding, bool nullFirst)
		{
			string prefix = new string ('x', padding);
			string first = nullFirst ? "\0" : "\u00ff";
			string second = nullFirst ? "\u00ff" : "\0";
			string text = "Content-Type: text/plain\r\nContent-Transfer-Encoding: " + encoding + "\r\n\r\n" +
				prefix + "\r\n" + prefix + first + "\r\n" + prefix + second + "\r\n" + first + second;

			return Encoding.Latin1.GetBytes (text);
		}

		static void AssertByteDetections (TestMimeComplianceLogger logger, string encoding, bool nullFirst)
		{
			var nulls = logger.Issues.Where (x => x.Violation == MimeComplianceViolation.UnexpectedNullBytesInBody).ToList ();
			var eightBits = logger.Issues.Where (x => x.Violation == MimeComplianceViolation.Unexpected8BitBytesInBody).ToList ();

			Assert.That (nulls.Count, Is.EqualTo (encoding == "binary" ? 0 : 1), "NUL issue count");
			Assert.That (eightBits.Count, Is.EqualTo (encoding == "7bit" ? 1 : 0), "8-bit issue count");

			if (nulls.Count > 0)
				Assert.That (nulls[0].LineNumber, Is.EqualTo (nullFirst ? 5 : 6), "NUL issue line");
			if (eightBits.Count > 0)
				Assert.That (eightBits[0].LineNumber, Is.EqualTo (nullFirst ? 6 : 5), "8-bit issue line");
		}

		[Test]
		public void TestContentByteDetection ([Values ("7bit", "8bit", "binary")] string encoding,
			[Values (0, 15, 16, 31, 32, 63, 64, 997, 4095)] int padding, [Values] bool nullFirst)
		{
			var data = CreateByteDetectionMessage (encoding, padding, nullFirst);

			foreach (int chunkSize in ChunkSizes.Append (data.Length)) {
				using var stream = new ChunkedReadStream (data, chunkSize);
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();

				AssertByteDetections (logger, encoding, nullFirst);
			}
		}

		[Test]
		public async Task TestContentByteDetectionAsync ([Values ("7bit", "8bit", "binary")] string encoding,
			[Values (0, 15, 16, 31, 32, 63, 64, 997, 4095)] int padding, [Values] bool nullFirst)
		{
			var data = CreateByteDetectionMessage (encoding, padding, nullFirst);

			foreach (int chunkSize in ChunkSizes.Append (data.Length)) {
				using var stream = new ChunkedReadStream (data, chunkSize);
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();

				AssertByteDetections (logger, encoding, nullFirst);
			}
		}

		[Test]
		public void TestGeneratedMessagesFastPathMatchesPerLinePath ([Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			var random = new Random (format == MimeFormat.Mbox ? 4242 : 2424);

			for (int i = 0; i < 300; i++) {
				var data = format == MimeFormat.Mbox ? GenerateMbox (random) : Encoding.Latin1.GetBytes (GenerateMessage (random));

				// randomly truncate some of the messages to exercise the end-of-stream code paths
				if (random.Next (4) == 0)
					data = data.AsSpan (0, random.Next (data.Length + 1)).ToArray ();

				int chunkSize = ChunkSizes[i % ChunkSizes.Length];
				var expected = Record (data, format, chunkSize, false);
				var actual = Record (data, format, chunkSize, true);

				AssertEventsEqual (expected, actual, $"message #{i} (chunkSize={chunkSize})");
			}
		}

		[Test]
		public async Task TestGeneratedMessagesFastPathMatchesPerLinePathAsync ([Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			var random = new Random (format == MimeFormat.Mbox ? 4242 : 2424);

			for (int i = 0; i < 300; i++) {
				var data = format == MimeFormat.Mbox ? GenerateMbox (random) : Encoding.Latin1.GetBytes (GenerateMessage (random));

				if (random.Next (4) == 0)
					data = data.AsSpan (0, random.Next (data.Length + 1)).ToArray ();

				int chunkSize = ChunkSizes[i % ChunkSizes.Length];
				var expected = await RecordAsync (data, format, chunkSize, false);
				var actual = await RecordAsync (data, format, chunkSize, true);

				AssertEventsEqual (expected, actual, $"message #{i} (chunkSize={chunkSize})");
			}
		}
	}
}
