//
// MimeComplianceEncodedWordTests.cs
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
	public class MimeComplianceEncodedWordTests
	{
		const MimeComplianceViolation Invalid = MimeComplianceViolation.InvalidEncodedWord;
		const MimeComplianceViolation Placement = MimeComplianceViolation.InvalidEncodedWordPlacement;

		// Regression cases inspired by https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> SyntaxCases ()
		{
			var cases = new (string Value, int Count)[] {
				("ordinary text", 0),
				("=?utf-8?Q?caf=C3=A9?=", 0),
				("=?UTF-8?q?hello_world?=", 0),
				("=?utf-8?B?YQ==?=", 0),
				("=?utf-8?b?YWI=?=", 0),
				("=?utf-8?B?YWJj?=", 0),
				("=?x-unknown*en?Q?hello?=", 0),
				("=?utf-8*?Q?hello?=", 0),
				("=?utf-8?Q?first?=\r\n\t=?utf-8?Q?second?=", 0),
				("=?utf-8?Q?()\\:.,?=", 0),
				("=?utf-8?Q?=ff=00?=", 0),
				("=?", 1),
				("=?utf-8?Q?unfinished", 1),
				("=??Q?hello?=", 1),
				("=?*en?Q?hello?=", 1),
				("=?utf.8?Q?hello?=", 1),
				("=?utf 8?Q?hello?=", 1),
				("=?utf-8?X?hello?=", 1),
				("=?utf-8?QQ?hello?=", 1),
				("=?utf-8??hello?=", 1),
				("=?utf-8?Q??=", 1),
				("=?utf-8?Q?a b?=", 1),
				("=?utf-8?Q?a\tb?=", 1),
				("=?utf-8?Q?a\r\n b?=", 1),
				("=?utf-8?Q?caf\u00e9?=", 1),
				("=?utf-8?Q?bad=GG?=", 1),
				("=?utf-8?Q?bad=A?=", 1),
				("=?utf-8?Q?bad=?=", 1),
				("=?utf-8?B?YQ?=", 1),
				("=?utf-8?B?YQ=?=", 1),
				("=?utf-8?B?Y===?=", 1),
				("=?utf-8?B?=WJj?=", 1),
				("=?utf-8?B?YWJj!?=", 1),
				("=?utf-8?B?YQ==YWJj?=", 1),
				("prefix=?utf-8?Q?hello?=", 1),
				("=?utf-8?Q?hello?=suffix", 1),
				("=?utf-8?Q?first?==?utf-8?Q?second?=", 2),
				("=?utf-8?Q?" + new string ('a', 63) + "?=", 0),
				("=?utf-8?Q?" + new string ('a', 64) + "?=", 1),
				("=?broken=?utf-8?Q?hello?=", 2),
				("=?utf-8?Q?broken=?utf-8?Q?hello?=", 2),
				("(=?utf-8?Q?hello?=)", 1)
			};

			foreach (var item in cases) {
				foreach (string field in new[] { "Subject", "Comments", "Content-Description", "X-Example" }) {
					foreach (bool chunked in new[] { false, true })
						yield return new TestCaseData (field + ": " + item.Value, item.Count, 0, chunked);
				}
			}
		}

		static IEnumerable<TestCaseData> AddressCases ()
		{
			var cases = new (string Value, int Invalid, int Placement)[] {
				("=?utf-8?Q?hello_world?= <a@example.com>", 0, 0),
				("=?utf-8?B?YQ==?= <a@example.com>", 0, 0),
				("=?utf-8?Q?group?= : a@example.com;", 0, 0),
				("a@example.com (=?utf-8?Q?hello?=)", 0, 0),
				("(outer (=?utf-8?Q?hello?=)) a@example.com", 0, 0),
				("a (=?utf-8?Q?hello?=) @ example.com", 0, 0),
				("=?utf-8?Q?hello?= (=?utf-8?Q?comment?=) <a@example.com>", 0, 0),
				("=?utf-8?Q?bad=GG?= <a@example.com>", 1, 0),
				("=?utf-8?B?YQ?= <a@example.com>", 1, 0),
				("=?utf-8?X?hello?= <a@example.com>", 1, 0),
				("=?utf-8?Q?hello.world?= <a@example.com>", 1, 0),
				("=?utf-8?Q?hello=2Eworld?= <a@example.com>", 0, 0),
				("=?utf-8?Q?group.name?= : a@example.com;", 1, 0),
				("=?utf-8?Q?hello?=<a@example.com>", 1, 0),
				("=?utf-8?Q?group?=: a@example.com;", 1, 0),
				("prefix=?utf-8?Q?hello?= <a@example.com>", 1, 0),
				("a@example.com (=?utf-8?Q?hello.world?=)", 0, 0),
				("a@example.com (=?utf-8?Q?hello(world)?=)", 1, 0),
				("a@example.com (=?utf-8?Q?hello\\world?=)", 1, 0),
				("a@example.com (prefix=?utf-8?Q?hello?=)", 1, 0),
				("a@example.com (=?utf-8?Q?bad=GG?=)", 1, 0),
				("\"=?utf-8?Q?hello?=\" <a@example.com>", 0, 1),
				("\"=?utf-8?Q?bad=GG?=\" <a@example.com>", 1, 1),
				("\"=?utf-8?Q?bad=?=\" <a@example.com>", 1, 1),
				("\"=?utf-8?Q?group?=\" : a@example.com;", 0, 1),
				("=?utf-8?Q?user?=@example.com", 0, 1),
				("\"=?utf-8?Q?user?=\"@example.com", 0, 1),
				("a@=?utf-8?Q?example?=.com", 0, 1),
				("a@=?utf-8?Q?example.com?=", 0, 1),
				("a@[=?utf-8?Q?example?=]", 0, 1),
				("Name <a@=?utf-8?Q?example?=.com>", 0, 1),
				("=?literal@example.com", 0, 0),
				("\"=?literal\"@example.com", 0, 0),
				("a@example.com, =?utf-8?Q?bad=GG?= <b@example.com>", 1, 0),
				("=?utf-8?Q?group?= : =?utf-8?Q?bad=GG?= <a@example.com>;", 1, 0),
				("=?utf-8?Q?first?= <a@example.com>, \"=?utf-8?Q?second?=\" <b@example.com>", 0, 1),
				("(=?utf-8?Q?bad=GG?=) \"=?utf-8?Q?hello?=\" <a@example.com>", 1, 1)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true })
					yield return new TestCaseData ("To: " + item.Value, item.Invalid, item.Placement, chunked);
			}

			foreach (string field in new[] { "From", "Sender", "Reply-To", "Cc", "Bcc", "Resent-From", "Resent-Sender",
				"Resent-Reply-To", "Resent-To", "Resent-Cc", "Resent-Bcc", "Disposition-Notification-To" })
				yield return new TestCaseData (field + ": =?utf-8?Q?bad=GG?= <a@example.com>", 1, 0, false);

			foreach (string field in new[] { "Date", "Message-Id", "Received", "Content-Type", "Content-Disposition", "Return-Path" })
				yield return new TestCaseData (field + ": =?utf-8?Q?bad=GG?=", 0, 0, false);
		}

		static Stream CreateStream (string text, bool chunked)
		{
			var bytes = Encoding.UTF8.GetBytes (text);
			return chunked ? new ChunkedReadStream (bytes, 1) : new MemoryStream (bytes, false);
		}

		static void AssertWords (List<MimeComplianceIssue> issues, int invalid, int placement)
		{
			Assert.That (issues.Count (x => x.Violation == Invalid), Is.EqualTo (invalid), "syntax");
			Assert.That (issues.Count (x => x.Violation == Placement), Is.EqualTo (placement), "placement");
			Assert.That (issues.Where (x => x.Violation == Invalid || x.Violation == Placement)
				.All (x => x.PositionKind == MimeCompliancePositionKind.ElementStart), Is.True);
		}

		[TestCaseSource (nameof (SyntaxCases))]
		[TestCaseSource (nameof (AddressCases))]
		public void TestEncodedWords (string header, int invalid, int placement, bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();
			using (var stream = CreateStream (header + "\r\n\r\nbody\r\n", chunked)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };
				reader.ReadMessage ();
			}
			AssertWords (logger.Issues, invalid, placement);
		}

		[TestCaseSource (nameof (SyntaxCases))]
		[TestCaseSource (nameof (AddressCases))]
		public async Task TestEncodedWordsAsync (string header, int invalid, int placement, bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();
			using (var stream = CreateStream (header + "\r\n\r\nbody\r\n", chunked)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };
				await reader.ReadMessageAsync ();
			}
			AssertWords (logger.Issues, invalid, placement);
		}

		const string Preserved = "Subject: =?utf-8?Q?bad=GG?=\r\n" +
			"To: =?x-unknown?Q?hello?= <a@example.com>, \"=?utf-8?Q?other?=\" <b@example.com>\r\n" +
			"To: =?utf-8?Q?ignored=GG?= <ignored@example.com>\r\n\r\nbody\r\n";

		[Test]
		public async Task TestGatingAndRecovery ([Values (false, true)] bool chunked, [Values (false, true)] bool async)
		{
			byte[] expected = null;
			string expectedTo = null, expectedSubject = null;
			foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.Address,
				MimeComplianceValidators.EncodedWords, MimeComplianceValidators.All,
				MimeComplianceValidators.All & ~MimeComplianceValidators.EncodedWords }) {
				foreach (bool logging in new[] { false, true }) {
					using (var stream = CreateStream (Preserved, chunked)) {
						var logger = new TestMimeComplianceLogger ();
						var parser = new MimeParser (stream) {
							ComplianceLogger = logging ? logger : null,
							ComplianceOptions = new MimeComplianceOptions { EnabledValidators = validators }
						};
						using var message = async ? await parser.ParseMessageAsync () : parser.ParseMessage ();
						expectedTo ??= message.To.ToString ();
						expectedSubject ??= message.Subject;
						Assert.That (message.To.ToString (), Is.EqualTo (expectedTo));
						Assert.That (message.Subject, Is.EqualTo (expectedSubject));
						using var output = new MemoryStream ();
						message.WriteTo (output);
						var actual = output.ToArray ();
						expected ??= actual;
						Assert.That (actual, Is.EqualTo (expected));
						bool enabled = logging && (validators & MimeComplianceValidators.EncodedWords) != 0;
						AssertWords (logger.Issues, enabled ? 2 : 0, enabled ? 1 : 0);
					}
				}
			}
		}

		[Test]
		public async Task TestPositionsAndBudgets ([Values (false, true)] bool chunked, [Values (false, true)] bool async)
		{
			const string header = "X-Prefix: \u00e9\r\nTo: (\u00e9) =?utf-8?Q?bad=GG?= <a@example.com>,\r\n" +
				"\t\"=?utf-8?Q?other?=\" <b@example.com>\r\nSubject: =?utf-8?Q?bad=G?=\r\n\r\nbody\r\n";
			foreach (int budget in new[] { 0, 1 }) {
				var logger = new TestMimeComplianceLogger ();
				using (var stream = CreateStream (header, chunked)) {
					var reader = new MimeReader (stream) {
						ComplianceLogger = logger,
						ComplianceOptions = new MimeComplianceOptions {
							EnabledValidators = MimeComplianceValidators.EncodedWords,
							Context = MimeComplianceContext.Storage,
							MaxIssuesPerViolation = budget
						}
					};
					if (async)
						await reader.ReadMessageAsync ();
					else
						reader.ReadMessage ();
				}
				AssertWords (logger.Issues, budget == 0 ? 2 : 1, 1);
				foreach (var issue in logger.Issues.Where (x => x.Violation == Invalid || x.Violation == Placement)) {
					var bytes = Encoding.UTF8.GetBytes (header);
					int line = 1, column = 1;
					for (int i = 0; i < issue.StreamOffset; i++) {
						if (bytes[i] == '\n') {
							line++;
							column = 1;
						} else {
							column++;
						}
					}
					Assert.That (bytes[issue.StreamOffset], Is.EqualTo ((byte) '='));
					Assert.That (bytes[issue.StreamOffset + 1], Is.EqualTo ((byte) '?'));
					Assert.That (issue.LineNumber, Is.EqualTo (line));
					Assert.That (issue.ColumnNumber, Is.EqualTo (column));
					Assert.That (issue.Context, Is.EqualTo (MimeComplianceContext.Storage));
				}
				Assert.That (logger.Issues.Count (x => x.Violation == MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (budget));
			}
		}

		sealed class CountingLogger : IMimeComplianceLogger
		{
			public int Count;

			public void Log (in MimeComplianceIssue issue)
			{
				Count++;
			}
		}

		[Test]
		public void TestValidatorOffsetsAndReuse ()
		{
			const string value = "=?utf-8?Q?bad=GG?= <a@example.com>,\r\n \"=?utf-8?Q?other?=\" <b@example.com>";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "yy");
			var logger = new TestMimeComplianceLogger ();
			var validator = new AddressValidator (logger, MimeComplianceContext.Storage, 100, 5, 7) {
				ValidateEncodedWords = true, ValidateSyntax = false
			};
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, value.Length);
				AssertWords (logger.Issues, 1, 1);
				var invalid = logger.Issues.Single (x => x.Violation == Invalid);
				var placement = logger.Issues.Single (x => x.Violation == Placement);
				Assert.That (invalid.StreamOffset, Is.EqualTo (100));
				Assert.That (invalid.LineNumber, Is.EqualTo (5));
				Assert.That (invalid.ColumnNumber, Is.EqualTo (7));
				Assert.That (placement.StreamOffset, Is.EqualTo (100 + value.IndexOf ("=?utf-8?Q?other")));
				Assert.That (placement.LineNumber, Is.EqualTo (6));
				Assert.That (placement.ColumnNumber, Is.EqualTo (3));

				logger.Issues.Clear ();
				const string truncated = "=?utf-8?Q?hello?";
				var partial = Encoding.ASCII.GetBytes ("xx" + truncated + "=yy");
				var words = new EncodedWordValidator (logger, MimeComplianceContext.Storage, partial, 2, truncated.Length, 100, 5, 7);
				words.Validate (2 + truncated.Length, EncodedWordContext.Text);
				AssertWords (logger.Issues, 1, 0);
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (100));
			}
		}

		[Test]
		public async Task TestAddressSyntaxGating ([Values (false, true)] bool async)
		{
			foreach (var validators in new[] { MimeComplianceValidators.EncodedWords, MimeComplianceValidators.Address,
				MimeComplianceValidators.EncodedWords | MimeComplianceValidators.Address }) {
				var logger = new TestMimeComplianceLogger ();
				using (var stream = CreateStream ("To: =?utf-8?Q?bad=GG?= <local>\r\n\r\nbody\r\n", true)) {
					var reader = new MimeReader (stream) {
						ComplianceLogger = logger,
						ComplianceOptions = new MimeComplianceOptions { EnabledValidators = validators }
					};
					if (async)
						await reader.ReadMessageAsync ();
					else
						reader.ReadMessage ();
				}
				AssertWords (logger.Issues, (validators & MimeComplianceValidators.EncodedWords) != 0 ? 1 : 0, 0);
				Assert.That (logger.Issues.Any (x => x.Violation == MimeComplianceViolation.AddressWithoutDomain),
					Is.EqualTo ((validators & MimeComplianceValidators.Address) != 0));
			}
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesValidation ()
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream ("Subject: =?utf-8?Q?bad=GG?= " + new string ('a', 256) + "\r\n\r\n", true);
			var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
			Assert.Throws<FormatException> (() => reader.ReadMessage ());
			AssertWords (logger.Issues, 0, 0);
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesValidationAsync ()
		{
			var logger = new TestMimeComplianceLogger ();
			using var stream = CreateStream ("To: =?utf-8?Q?bad=GG?= <a@" + new string ('a', 256) + ">\r\n\r\n", true);
			var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
			Assert.ThrowsAsync<FormatException> (async () => await reader.ReadMessageAsync ());
			AssertWords (logger.Issues, 0, 0);
		}

		[Test]
		public void TestBoundedScan ([Values ("plain", "=?", "=?utf-8?Q?broken", "=?x-unknown?Q?bad=GG?= ")] string fragment,
			[Values (false, true)] bool address)
		{
			var text = new StringBuilder ();
			while (text.Length < 1024 * 1024)
				text.Append (fragment);
			if (address)
				text.Append (" <a@example.com>");
			var bytes = Encoding.ASCII.GetBytes (text.ToString ());
			var logger = new CountingLogger ();
			var validator = new AddressValidator (logger, MimeComplianceContext.Transport, 0, 1, 1) {
				ValidateEncodedWords = true, ValidateSyntax = false
			};
			var words = new EncodedWordValidator (logger, MimeComplianceContext.Transport, bytes, 0, bytes.Length, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			if (address)
				validator.Validate (bytes, 0, bytes.Length);
			else
				words.Validate (bytes.Length, EncodedWordContext.Text);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();
			Assert.That (logger.Count, fragment == "plain" ? Is.EqualTo (0) : Is.GreaterThan (0));
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (65536));
			}
		}
	}
}
