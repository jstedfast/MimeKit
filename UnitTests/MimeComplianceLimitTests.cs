//
// MimeComplianceLimitTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceLimitTests
	{
		const int FloodCount = 20;

		// Note: Each bare linefeed in the body is a separate violation, so the body below is worth
		// FloodCount issues of a single violation and nothing else.
		static string CreateFloodedMessage (int count = FloodCount)
		{
			var builder = new StringBuilder ();

			builder.Append ("From: sender@example.com\r\n");
			builder.Append ("To: recipient@example.com\r\n");
			builder.Append ("Subject: flood\r\n");
			builder.Append ("\r\n");

			for (int i = 0; i < count; i++)
				builder.Append ("line\n");

			return builder.ToString ();
		}

		static List<MimeComplianceIssue> Read (string text, int limit, MimeFormat format = MimeFormat.Entity)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream, format) {
					ComplianceLogger = logger,
					MaxComplianceIssuesPerViolation = limit
				};

				while (!reader.IsEndOfStream)
					reader.ReadMessage ();
			}

			return logger.Issues;
		}

		static async Task<List<MimeComplianceIssue>> ReadAsync (string text, int limit, MimeFormat format = MimeFormat.Entity)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream, format) {
					ComplianceLogger = logger,
					MaxComplianceIssuesPerViolation = limit
				};

				while (!reader.IsEndOfStream)
					await reader.ReadMessageAsync ();
			}

			return logger.Issues;
		}

		static int Count (List<MimeComplianceIssue> issues, MimeComplianceViolation violation)
		{
			return issues.Count (issue => issue.Violation == violation);
		}

		[Test]
		public void TestDefaultIsNoLimit ()
		{
			using (var stream = new MemoryStream ()) {
				var reader = new MimeReader (stream);

				Assert.That (reader.MaxComplianceIssuesPerViolation, Is.EqualTo (0));
			}

			var issues = Read (CreateFloodedMessage (), 0);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (FloodCount));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (0));
		}

		[Test]
		public async Task TestDefaultIsNoLimitAsync ()
		{
			var issues = await ReadAsync (CreateFloodedMessage (), 0);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (FloodCount));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (0));
		}

		[Test]
		public void TestNegativeLimitThrows ()
		{
			using (var stream = new MemoryStream ()) {
				var reader = new MimeReader (stream);

				Assert.Throws<ArgumentOutOfRangeException> (() => reader.MaxComplianceIssuesPerViolation = -1);
			}
		}

		[Test]
		public void TestLimitSuppressesTheExcess ()
		{
			var issues = Read (CreateFloodedMessage (), 3);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (3));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
		}

		[Test]
		public async Task TestLimitSuppressesTheExcessAsync ()
		{
			var issues = await ReadAsync (CreateFloodedMessage (), 3);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (3));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
		}

		[Test]
		public void TestTruncationIsReportedOnlyOnce ()
		{
			// Note: Two different violations are flooded here, so two budgets run out. The marker says
			// that the report is incomplete, which only needs saying once.
			var builder = new StringBuilder ();

			builder.Append ("From: sender@example.com\r\n");

			for (int i = 0; i < FloodCount; i++)
				builder.Append ("X-Header-" + i + ": value\n");

			builder.Append ("Subject: flood\r\n");
			builder.Append ("\r\n");

			for (int i = 0; i < FloodCount; i++)
				builder.Append ("line\n");

			var issues = Read (builder.ToString (), 2);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInHeader), Is.EqualTo (2));
			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (2));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1));
		}

		[Test]
		public void TestFloodedViolationDoesNotSuppressOthers ()
		{
			// Note: This is the reason the budget is per violation rather than a single total. A flood
			// of one cheap violation must not be able to hide a different violation that comes after
			// it.
			var builder = new StringBuilder ();

			builder.Append ("From: sender@example.com\r\n");
			builder.Append ("To: recipient@example.com\r\n");
			builder.Append ("Subject: flood\r\n");
			builder.Append ("\r\n");

			for (int i = 0; i < FloodCount; i++)
				builder.Append ("line\n");

			builder.Append ("caf\u00e9\r\n");

			var issues = Read (builder.ToString (), 3);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (3));
			Assert.That (Count (issues, MimeComplianceViolation.Unexpected8BitBytesInBody), Is.GreaterThan (0));
		}

		[Test]
		public void TestBudgetIsResetForEachMessage ()
		{
			// Note: A single reader is routinely used to walk an entire mbox. A budget that was not
			// reset would be spent by the first malformed message and leave every message after it
			// unreported.
			var builder = new StringBuilder ();

			for (int i = 0; i < 2; i++) {
				builder.Append ("From mbox@example.com Mon Jan  1 00:00:00 2024\r\n");
				builder.Append ("From: sender@example.com\r\n");
				builder.Append ("To: recipient@example.com\r\n");
				builder.Append ("Subject: flood\r\n");
				builder.Append ("\r\n");

				for (int j = 0; j < FloodCount; j++)
					builder.Append ("line\n");

				builder.Append ("\r\n");
			}

			var issues = Read (builder.ToString (), 3, MimeFormat.Mbox);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (6));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (2));
		}

		[Test]
		public async Task TestBudgetIsResetForEachMessageAsync ()
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < 2; i++) {
				builder.Append ("From mbox@example.com Mon Jan  1 00:00:00 2024\r\n");
				builder.Append ("From: sender@example.com\r\n");
				builder.Append ("To: recipient@example.com\r\n");
				builder.Append ("Subject: flood\r\n");
				builder.Append ("\r\n");

				for (int j = 0; j < FloodCount; j++)
					builder.Append ("line\n");

				builder.Append ("\r\n");
			}

			var issues = await ReadAsync (builder.ToString (), 3, MimeFormat.Mbox);

			Assert.That (Count (issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (6));
			Assert.That (Count (issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (2));
		}

		[Test]
		public void TestClearingTheLimitRestoresTheCallersLogger ()
		{
			var logger = new TestMimeComplianceLogger ();
			var text = CreateFloodedMessage ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream) {
					ComplianceLogger = logger,
					MaxComplianceIssuesPerViolation = 3
				};

				Assert.That (reader.ComplianceLogger, Is.SameAs (logger));

				reader.MaxComplianceIssuesPerViolation = 0;

				reader.ReadMessage ();
			}

			Assert.That (Count (logger.Issues, MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (FloodCount));
			Assert.That (Count (logger.Issues, MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (0));
		}
	}
}
