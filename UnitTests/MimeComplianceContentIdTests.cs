//
// MimeComplianceContentIdTests.cs
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

using MimeKit;

using UnitTests.IO;

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceContentIdTests
	{
		const MimeComplianceViolation Repeated = MimeComplianceViolation.RepeatedContentId;

		const string Nested = "Content-Type: multipart/mixed; boundary=b\r\n" +
			"Content-Id: <same@example.com>\r\nContent-Id: <outer@example.com>\r\n\r\n" +
			"--b\r\nContent-Id: <same@example.com>\r\n\r\none\r\n" +
			"--b\r\nContent-Type: multipart/alternative; boundary=c\r\nContent-Id: <same@example.com>\r\n\r\n" +
			"--c\r\nContent-Id: <same@example.com>\r\nContent-Id: <child@example.com>\r\n\r\ntwo\r\n--c--\r\n" +
			"--b\r\nContent-Type: message/rfc822\r\nContent-Id: <same@example.com>\r\n\r\n" +
			"Content-Id: <same@example.com>\r\nContent-Id: <inner@example.com>\r\n\r\nthree\r\n" +
			"--b\r\nContent-Id: <same@example.com>\r\n\r\nfour\r\n--b--\r\n";

		static IEnumerable<TestCaseData> HeaderCases ()
		{
			var cases = new (string Text, int[] Lines)[] {
				("Content-Id: <same@example.com>\r\nContent-Id: <same@example.com>\r\n\r\nbody\r\n", new[] { 2 }),
				("X-Prefix: \u00e9\r\nContent-ID:\r\n <first@example.com>\r\ncOnTeNt-Id: <second@example.com>\r\nContent-Id: <third@example.com>\r\n\r\nbody\r\n", new[] { 4, 5 }),
				("Content-Id:\r\nContent-Id: <second@example.com>\r\n\r\nbody\r\n", new[] { 2 }),
				("Content-Id: invalid\r\nContent-Id: <second@example.com>\r\n\r\nbody\r\n", new[] { 2 }),
				("Content-Id: <first@example.com>\r\nContent-Id: invalid", new[] { 2 }),
				("Content-Id: <first@example.com>\r\nContent-Id:\r\n", new[] { 2 }),
				("Content-Id: <first@example.com>\r\nContent-Id", Array.Empty<int> ()),
				("Content-Id: <first@example.com>\r\n\tContent-Id: <second@example.com>\r\n\r\nbody\r\n", Array.Empty<int> ()),
				("Content-Id: <first@example.com>\r\n\r\nbody\r\n", Array.Empty<int> ()),
				(Nested, new[] { 3, 15, 24 })
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					foreach (bool entity in new[] { false, true })
						yield return new TestCaseData (item.Text, item.Lines, chunked, entity);
				}
			}
		}

		static Stream CreateStream (string text, bool chunked)
		{
			var bytes = Encoding.UTF8.GetBytes (text);
			return chunked ? new ChunkedReadStream (bytes, 1) : new MemoryStream (bytes, false);
		}

		static List<MimeComplianceIssue> Read (string text, bool chunked, bool entity, MimeComplianceOptions options = null)
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream (text, chunked);
			var reader = new MimeReader (stream) {
				ComplianceLogger = logger,
				ComplianceOptions = options ?? new MimeComplianceOptions ()
			};
			if (entity)
				reader.ReadEntity ();
			else
				reader.ReadMessage ();
			return logger.Issues;
		}

		static async Task<List<MimeComplianceIssue>> ReadAsync (string text, bool chunked, bool entity, MimeComplianceOptions options = null)
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream (text, chunked);
			var reader = new MimeReader (stream) {
				ComplianceLogger = logger,
				ComplianceOptions = options ?? new MimeComplianceOptions ()
			};
			if (entity)
				await reader.ReadEntityAsync ();
			else
				await reader.ReadMessageAsync ();
			return logger.Issues;
		}

		static void AssertContentIds (string text, List<MimeComplianceIssue> issues, params int[] lines)
		{
			var repeated = issues.Where (issue => issue.Violation == Repeated).ToList ();
			var bytes = Encoding.UTF8.GetBytes (text);
			int offset = 0, line = 1;

			Assert.That (repeated, Has.Count.EqualTo (lines.Length));
			for (int i = 0; i < lines.Length; i++) {
				while (line < lines[i]) {
					if (bytes[offset++] == '\n')
						line++;
				}

				Assert.That (repeated[i].StreamOffset, Is.EqualTo (offset));
				Assert.That (repeated[i].LineNumber, Is.EqualTo (lines[i]));
				Assert.That (repeated[i].ColumnNumber, Is.EqualTo (1));
				Assert.That (repeated[i].PositionKind, Is.EqualTo (MimeCompliancePositionKind.ElementStart));
			}
		}

		[TestCaseSource (nameof (HeaderCases))]
		public void TestRepeatedContentId (string text, int[] lines, bool chunked, bool entity)
		{
			AssertContentIds (text, Read (text, chunked, entity), lines);
		}

		[TestCaseSource (nameof (HeaderCases))]
		public async Task TestRepeatedContentIdAsync (string text, int[] lines, bool chunked, bool entity)
		{
			AssertContentIds (text, await ReadAsync (text, chunked, entity), lines);
		}

		const string MboxMessage = "From sender@example.com Mon Jan 1 00:00:00 2024\r\n" +
			"Content-Id: <same@example.com>\r\n\r\nbody\r\n";

		static async Task AssertStateResetsBetweenMessages (bool chunked, bool async)
		{
			var text = MboxMessage + MboxMessage +
				MboxMessage.Replace ("\r\n\r\n", "\r\nContent-Id: <other@example.com>\r\n\r\n");
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream (text, chunked);
			var reader = new MimeReader (stream, MimeFormat.Mbox) { ComplianceLogger = logger };
			for (int i = 0; i < 3; i++) {
				if (async)
					await reader.ReadMessageAsync ();
				else
					reader.ReadMessage ();
			}
			AssertContentIds (text, logger.Issues, 11);
		}

		[Test]
		public Task TestStateResetsBetweenMessages ([Values (false, true)] bool chunked)
		{
			return AssertStateResetsBetweenMessages (chunked, false);
		}

		[Test]
		public Task TestStateResetsBetweenMessagesAsync ([Values (false, true)] bool chunked)
		{
			return AssertStateResetsBetweenMessages (chunked, true);
		}

		const string Limited = "Content-Id: <first@example.com>\r\nContent-Id: <second@example.com>\r\n" +
			"Content-Id: <third@example.com>\r\nContent-Id: <fourth@example.com>\r\n" +
			"Content-Disposition: inline\r\nContent-Disposition: attachment\r\n\r\nbody\r\n";

		[Test]
		public void TestReadHeaders ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream (Limited, chunked);
			var reader = new MimeReader (stream) { ComplianceLogger = logger };
			reader.ReadHeaders ();
			AssertContentIds (Limited, logger.Issues, 2, 3, 4);
		}

		[Test]
		public async Task TestReadHeadersAsync ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream (Limited, chunked);
			var reader = new MimeReader (stream) { ComplianceLogger = logger };
			await reader.ReadHeadersAsync ();
			AssertContentIds (Limited, logger.Issues, 2, 3, 4);
		}

		static void AssertLimit (List<MimeComplianceIssue> issues)
		{
			AssertContentIds (Limited, issues, 2);
			var truncated = issues.Single (issue => issue.Violation == MimeComplianceViolation.TooManyComplianceIssues);
			Assert.That (truncated.StreamOffset, Is.EqualTo (Limited.IndexOf ("Content-Id: <third", StringComparison.Ordinal)));
			Assert.That (issues.Count (issue => issue.Violation == MimeComplianceViolation.RepeatedContentDisposition), Is.EqualTo (1));
			Assert.That (issues.All (issue => issue.Context == MimeComplianceContext.Storage), Is.True);
		}

		[Test]
		public void TestLimitAndValidatorGating ([Values (false, true)] bool chunked)
		{
			AssertLimit (Read (Limited, chunked, false, new MimeComplianceOptions {
				EnabledValidators = MimeComplianceValidators.None,
				Context = MimeComplianceContext.Storage,
				MaxIssuesPerViolation = 1
			}));
		}

		[Test]
		public async Task TestLimitAndValidatorGatingAsync ([Values (false, true)] bool chunked)
		{
			AssertLimit (await ReadAsync (Limited, chunked, false, new MimeComplianceOptions {
				EnabledValidators = MimeComplianceValidators.None,
				Context = MimeComplianceContext.Storage,
				MaxIssuesPerViolation = 1
			}));
		}

		static async Task AssertParsingIsUnchanged (bool chunked, bool async)
		{
			const string text = "Content-Type: text/plain\r\nContent-Id: <first@example.com>\r\n" +
				"Content-Id: <second@example.com>\r\n\r\nbody\r\n";
			byte[] expected = null;
			foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.All }) {
				foreach (bool logging in new[] { false, true }) {
					var logger = new TestMimeComplianceLogger ();
					using var stream = CreateStream (text, chunked);
					var parser = new MimeParser (stream) {
						ComplianceLogger = logging ? logger : null,
						ComplianceOptions = new MimeComplianceOptions { EnabledValidators = validators }
					};
					using var message = async ? await parser.ParseMessageAsync () : parser.ParseMessage ();
					Assert.That (message.Body.ContentId, Is.EqualTo ("first@example.com"));
					Assert.That (message.Body.Headers.Count (header => header.Id == HeaderId.ContentId), Is.EqualTo (2));
					using var output = new MemoryStream ();
					message.WriteTo (output);
					expected ??= output.ToArray ();
					Assert.That (output.ToArray (), Is.EqualTo (expected));
					AssertContentIds (text, logger.Issues, logging ? new[] { 3 } : Array.Empty<int> ());
				}
			}
		}

		[Test]
		public Task TestParsingIsUnchanged ([Values (false, true)] bool chunked)
		{
			return AssertParsingIsUnchanged (chunked, false);
		}

		[Test]
		public Task TestParsingIsUnchangedAsync ([Values (false, true)] bool chunked)
		{
			return AssertParsingIsUnchanged (chunked, true);
		}
	}
}
