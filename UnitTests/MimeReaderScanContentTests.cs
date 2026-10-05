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

		// Returns at most 'chunkSize' bytes per read to force lines and line endings to be split across reads.
		class ChunkedReadStream : MemoryStream
		{
			readonly int chunkSize;

			public ChunkedReadStream (byte[] buffer, int chunkSize) : base (buffer, false)
			{
				this.chunkSize = chunkSize;
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				return base.Read (buffer, offset, Math.Min (count, chunkSize));
			}

			public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				return base.ReadAsync (buffer, offset, Math.Min (count, chunkSize), cancellationToken);
			}
		}

		sealed class NullComplianceLogger : IMimeComplianceLogger
		{
			public void Log (in MimeComplianceIssue issue)
			{
			}
		}

		// Note: Setting a compliance logger disables the fast path, so the results with and without a logger must be identical.
		static List<string> Record (byte[] data, MimeFormat format, int chunkSize, bool fastPath)
		{
			using (var stream = new ChunkedReadStream (data, chunkSize)) {
				var reader = new RecordingReader (stream, format);

				if (!fastPath)
					reader.ComplianceLogger = new NullComplianceLogger ();

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

				if (!fastPath)
					reader.ComplianceLogger = new NullComplianceLogger ();

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
