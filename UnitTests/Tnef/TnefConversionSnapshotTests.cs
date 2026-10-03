//
// TnefConversionSnapshotTests.cs
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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using MimeKit;
using MimeKit.Tnef;
using MimeKit.Utils;

namespace UnitTests.Tnef {
	/// <summary>
	/// Pins the complete result of converting each TNEF corpus file to MIME.
	/// </summary>
	/// <remarks>
	/// <para>The <c>.list</c> files beside the corpus only record attachment file names. These snapshots
	/// also cover the MIME structure, every header, the transfer encoding, the disposition and the exact
	/// decoded content of every part, so that rewriting the conversion cannot silently change any of it.</para>
	/// <para>A snapshot records a hash of each part's content rather than the content itself, because the
	/// corpus contains several hundred kilobytes of attachments. The text of a text part is previewed so
	/// that a mismatch is readable without having to reproduce it.</para>
	/// <para>To regenerate the snapshots after an intentional change, set the
	/// <c>MIMEKIT_UPDATE_TNEF_SNAPSHOTS</c> environment variable to <c>1</c>, run this fixture, and review
	/// the resulting diff before committing it.</para>
	/// </remarks>
	[TestFixture]
	public class TnefConversionSnapshotTests
	{
		const int TextPreviewLength = 256;

		static readonly Regex GeneratedMessageIdLocalPart = new Regex ("^[0-9A-Z]+\\.[0-9A-Z]+@", RegexOptions.CultureInvariant);

		static string CorpusDirectory => Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		static bool UpdateSnapshots => Environment.GetEnvironmentVariable ("MIMEKIT_UPDATE_TNEF_SNAPSHOTS") == "1";

		public static IEnumerable<TestCaseData> CorpusCases ()
		{
			foreach (var path in Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal))
				yield return new TestCaseData (Path.GetFileName (path)).SetArgDisplayNames (Path.GetFileName (path));
		}

		/// <summary>
		/// Convert a raw TNEF stream to MIME using the public conversion API.
		/// </summary>
		internal static MimeMessage Convert (byte[] tnef)
		{
			var part = new TnefPart {
				Content = new MimeContent (new MemoryStream (tnef, false))
			};

			return ConvertToMime (part);
		}

		static MimeMessage ConvertToMime (TnefPart part)
		{
			return TnefConversionTestHelper.Convert (part);
		}

		/// <summary>
		/// Render a deterministic, machine-independent description of a converted message.
		/// </summary>
		/// <remarks>
		/// Multipart boundaries and the generated Message-Id are random, and dates are rendered in the
		/// local time zone, so all three are normalized before being recorded.
		/// </remarks>
		internal static string Describe (MimeMessage message)
		{
			var builder = new StringBuilder ();
			int boundary = 0;

			NormalizeBoundaries (message.Body, ref boundary);
			DescribeMessage (builder, message, 0);

			return builder.ToString ();
		}

		static void NormalizeBoundaries (MimeEntity entity, ref int boundary)
		{
			if (entity is Multipart multipart) {
				multipart.Boundary = "=-snapshot-boundary-" + (boundary++).ToString (CultureInfo.InvariantCulture);

				foreach (var child in multipart)
					NormalizeBoundaries (child, ref boundary);
			} else if (entity is MessagePart rfc822 && rfc822.Message != null) {
				NormalizeBoundaries (rfc822.Message.Body, ref boundary);
			}
		}

		static void Indent (StringBuilder builder, int depth)
		{
			builder.Append (' ', depth * 2);
		}

		static string Escape (string value)
		{
			var builder = new StringBuilder (value.Length);

			foreach (var c in value) {
				switch (c) {
				case '\\': builder.Append ("\\\\"); break;
				case '\r': builder.Append ("\\r"); break;
				case '\n': builder.Append ("\\n"); break;
				case '\t': builder.Append ("\\t"); break;
				default:
					if (c < 0x20 || c == 0x7f)
						builder.AppendFormat (CultureInfo.InvariantCulture, "\\u{0:x4}", (int) c);
					else
						builder.Append (c);
					break;
				}
			}

			return builder.ToString ();
		}

		static string FormatDate (DateTimeOffset? date)
		{
			if (date == null)
				return "(null)";

			return date.Value.ToUniversalTime ().ToString ("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
		}

		static bool IsGeneratedMessageId (string messageId)
		{
			// MimeMessage generates a Message-Id of the form <TICKS.RANDOM@hostname>, both halves in upper-case
			// base36, when the TNEF stream did not supply one. The random part makes it unsuitable for a
			// snapshot. Both the shape and the host name are checked so that a genuine Message-Id from the
			// corpus is never mistaken for a generated one.
			if (messageId == null)
				return false;

			var generated = MimeUtils.GenerateMessageId ();
			var domain = generated.Substring (generated.IndexOf ('@'));

			return messageId.EndsWith (domain, StringComparison.Ordinal) && GeneratedMessageIdLocalPart.IsMatch (messageId);
		}

		static void DescribeHeaders (StringBuilder builder, HeaderList headers, int depth)
		{
			foreach (var header in headers) {
				string value;

				switch (header.Id) {
				case HeaderId.Date:
					value = DateUtils.TryParse (header.Value, out var date) ? FormatDate (date) : header.Value;
					break;
				case HeaderId.MessageId:
					value = IsGeneratedMessageId (MimeUtils.EnumerateReferences (header.Value).FirstOrDefault ()) ? "(generated)" : header.Value;
					break;
				case HeaderId.ContentDisposition:
					// The disposition's date parameters are written in the local time zone.
					if (ContentDisposition.TryParse (header.RawValue, out var disposition)) {
						var parts = new List<string> { disposition.Disposition };

						foreach (var parameter in disposition.Parameters) {
							switch (parameter.Name.ToLowerInvariant ()) {
							case "creation-date": parts.Add ("creation-date=" + FormatDate (disposition.CreationDate)); break;
							case "modification-date": parts.Add ("modification-date=" + FormatDate (disposition.ModificationDate)); break;
							case "read-date": parts.Add ("read-date=" + FormatDate (disposition.ReadDate)); break;
							default: parts.Add (parameter.Name + "=" + parameter.Value); break;
							}
						}

						value = string.Join ("; ", parts);
					} else {
						value = header.Value;
					}
					break;
				default:
					value = header.Value;
					break;
				}

				Indent (builder, depth);
				builder.Append (header.Field).Append (": ").AppendLine (Escape (value));
			}
		}

		static void DescribeMessage (StringBuilder builder, MimeMessage message, int depth)
		{
			Indent (builder, depth);
			builder.AppendLine ("message");
			DescribeHeaders (builder, message.Headers, depth + 1);
			DescribeEntity (builder, message.Body, depth + 1);
		}

		static void DescribeEntity (StringBuilder builder, MimeEntity entity, int depth)
		{
			Indent (builder, depth);

			if (entity == null) {
				builder.AppendLine ("(no body)");
				return;
			}

			builder.Append (entity.GetType ().Name).Append (' ').AppendLine (entity.ContentType.MimeType);
			DescribeHeaders (builder, entity.Headers, depth + 1);

			switch (entity) {
			case Multipart multipart:
				foreach (var child in multipart)
					DescribeEntity (builder, child, depth + 1);
				break;
			case MessagePart rfc822:
				if (rfc822.Message != null)
					DescribeMessage (builder, rfc822.Message, depth + 1);
				break;
			case MimePart part:
				DescribeContent (builder, part, depth + 1);
				break;
			}
		}

		static void DescribeContent (StringBuilder builder, MimePart part, int depth)
		{
			Indent (builder, depth);

			if (part.Content == null) {
				builder.AppendLine ("content: (null)");
				return;
			}

			byte[] decoded;

			using (var memory = new MemoryStream ()) {
				part.Content.DecodeTo (memory);
				decoded = memory.ToArray ();
			}

			string hash;

			using (var sha256 = SHA256.Create ())
				hash = BitConverter.ToString (sha256.ComputeHash (decoded)).Replace ("-", string.Empty).ToLowerInvariant ();

			builder.AppendFormat (CultureInfo.InvariantCulture, "content: encoding={0} length={1} sha256={2}", part.Content.Encoding, decoded.Length, hash).AppendLine ();

			if (part is TextPart text) {
				var value = text.Text;

				if (value.Length > TextPreviewLength)
					value = value.Substring (0, TextPreviewLength) + "...";

				Indent (builder, depth);
				builder.Append ("text: ").AppendLine (Escape (value));
			}
		}

		static string Normalize (string text)
		{
			return text.Replace ("\r\n", "\n");
		}

		[Test]
		public void TestCorpusSnapshotsArePresent ()
		{
			var cases = CorpusCases ().ToList ();

			Assert.That (cases, Is.Not.Empty);

			foreach (var path in Directory.EnumerateFiles (CorpusDirectory, "*.tnef"))
				Assert.That (File.Exists (Path.ChangeExtension (path, ".snapshot")), Is.True, Path.GetFileName (path) + " has no snapshot");
		}

		[TestCaseSource (nameof (CorpusCases))]
		public void TestConversionMatchesSnapshot (string fileName)
		{
			var path = Path.Combine (CorpusDirectory, fileName);
			var snapshotPath = Path.ChangeExtension (path, ".snapshot");
			string actual;

			using (var message = Convert (File.ReadAllBytes (path)))
				actual = Describe (message);

			if (UpdateSnapshots) {
				File.WriteAllText (snapshotPath, actual, new UTF8Encoding (false));
				Assert.Inconclusive ("Snapshot written: " + snapshotPath);
			}

			Assert.That (File.Exists (snapshotPath), Is.True, "Missing snapshot. Set MIMEKIT_UPDATE_TNEF_SNAPSHOTS=1 to create it.");

			var expected = File.ReadAllText (snapshotPath, Encoding.UTF8);

			Assert.That (Normalize (actual), Is.EqualTo (Normalize (expected)), fileName);
		}

		[Test]
		public void TestDescribeIsDeterministic ()
		{
			// Two independent conversions of the same input must describe identically, otherwise the
			// snapshots would be flaky.
			var data = File.ReadAllBytes (Path.Combine (CorpusDirectory, "winmail.tnef"));
			string first, second;

			using (var message = Convert (data))
				first = Describe (message);

			using (var message = Convert (data))
				second = Describe (message);

			Assert.That (second, Is.EqualTo (first));
		}

		/// <summary>
		/// Dump descriptions of the corpus and of a deterministic set of mutated inputs to a directory.
		/// </summary>
		/// <remarks>
		/// This exists to diff the conversion before and after a rewrite on malformed input, where a
		/// committed snapshot would be too large and too brittle. Set <c>MIMEKIT_TNEF_SNAPSHOT_DIR</c> to
		/// the output directory, run this test, repeat with the new code, and diff the two directories.
		/// </remarks>
		[Test]
		[Explicit ("Writes conversion snapshots to MIMEKIT_TNEF_SNAPSHOT_DIR for manual diffing.")]
		public void TestDumpConversionSnapshots ()
		{
			const int MutationsPerSeed = 64;

			var directory = Environment.GetEnvironmentVariable ("MIMEKIT_TNEF_SNAPSHOT_DIR");

			if (string.IsNullOrEmpty (directory))
				Assert.Ignore ("MIMEKIT_TNEF_SNAPSHOT_DIR is not set.");

			Directory.CreateDirectory (directory);

			var mutators = TnefCorpusFuzzTests.Mutators.ToArray ();

			foreach (var path in Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal)) {
				var fileName = Path.GetFileName (path);
				var seed = File.ReadAllBytes (path);
				var random = new Random (fileName.Aggregate (0x5A17, (hash, c) => (hash * 31) + c));

				File.WriteAllText (Path.Combine (directory, fileName + ".txt"), DescribeOrError (seed), new UTF8Encoding (false));

				for (int i = 0; i < MutationsPerSeed; i++) {
					var mutator = mutators[i % mutators.Length];
					var data = TnefCorpusFuzzTests.Mutate (mutator, seed, random);
					var name = string.Format (CultureInfo.InvariantCulture, "{0}.{1:D3}.{2}.txt", fileName, i, mutator);

					File.WriteAllText (Path.Combine (directory, name), DescribeOrError (data), new UTF8Encoding (false));
				}
			}
		}

		static string DescribeOrError (byte[] data)
		{
			try {
				using var message = Convert (data);

				return Describe (message);
			} catch (Exception ex) {
				return "exception: " + ex.GetType ().FullName + Environment.NewLine;
			}
		}
	}
}
