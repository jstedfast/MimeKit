//
// MimeComplianceParameterTests.cs
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
	public class MimeComplianceParameterTests
	{
		const MimeComplianceViolation Boundary = MimeComplianceViolation.RepeatedBoundaryParameter;
		const MimeComplianceViolation Filename = MimeComplianceViolation.RepeatedFilenameParameter;
		const MimeComplianceViolation Parameter = MimeComplianceViolation.RepeatedParameter;
		const MimeComplianceViolation Disposition = MimeComplianceViolation.RepeatedContentDisposition;
		const MimeComplianceViolation None = MimeComplianceViolation.None;

		// Regression cases inspired by https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> ParameterCases ()
		{
			var cases = new (string Field, string Value, MimeComplianceViolation Violation, int Count)[] {
				("Content-Type", "multipart/mixed; boundary=a; BOUNDARY=b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=a; boundary=a; boundary=a", Boundary, 2),
				("Content-Type", "multipart/mixed; boundary=a; boundary*=''b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*=''a; boundary=b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*=''a; boundary*=''b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*0=a; boundary*1=b", None, 0),
				("Content-Type", "multipart/mixed; boundary*1=b; boundary*0*=us-ascii''a", None, 0),
				("Content-Type", "multipart/mixed; boundary*0=a; boundary*00*=b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=a; boundary*0=b; boundary*1=c", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*0=a; boundary*1=b; boundary=c", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*=''a; boundary*0=b; boundary*1=c", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary*0=a; boundary*1=b; boundary*=''c", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=a(comment; boundary=x); boundary (x) = b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=\"a\" boundary=b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=a(comment)boundary=b", Boundary, 1),
				("Content-Type", "multipart/mixed; boundary=a boundary=b", None, 0),
				("Content-Type", "text/plain; name=file.png; NAME=file.zip", Filename, 1),
				("Content-Disposition", "attachment; filename=file.png; filename=file.zip", Filename, 1),
				("Content-Disposition", "attachment; filename*=''a; filename*=''b", Filename, 1),
				("Content-Disposition", "attachment; filename=a; filename*=''b", None, 0),
				("Content-Disposition", "attachment; filename*=''a; filename=b", None, 0),
				("Content-Type", "text/plain; name=fallback; name*0*=utf-8''file; name*1=.zip", None, 0),
				("Content-Disposition", "attachment; filename*0=a; filename*1=b; filename=fallback", None, 0),
				("Content-Disposition", "attachment; filename=a; filename*=''b; filename=c", Filename, 1),
				("Content-Disposition", "attachment; filename*=''a; filename=b; filename*=''c", Filename, 1),
				("Content-Disposition", "attachment; filename=a; filename*0=b; filename*0*=c", Filename, 1),
				("Content-Disposition", "attachment; filename*=''a; filename*0=b; filename*1=c", Filename, 1),
				("Content-Disposition", "attachment; filename*0=a; filename*1=b; filename*=''c", Filename, 1),
				("Content-Disposition", "attachment; filename * (x) 0 (x) * =''a; FILENAME*0=b", Filename, 1),
				("Content-Type", "text/plain; charset=utf-8; CHARSET=us-ascii", Parameter, 1),
				("Content-Type", "text/plain; charset=utf-8; charset*=us-ascii''utf-8", Parameter, 1),
				("Content-Type", "text/plain; filename=a; filename=b", Parameter, 1),
				("Content-Disposition", "attachment; name=a; name=b", Parameter, 1),
				("Content-Disposition", "attachment; boundary=a; boundary=b", Parameter, 1),
				("Content-Disposition", "attachment; filename=\"a; filename=b\"", None, 0),
				("Content-Disposition", "attachment; filename=\"a\\\"; filename=b\"; size=1", None, 0),
				("Content-Type", "text/plain; name=a (nested (name=x); name=y)", None, 0),
				("Content-Type", "text/plain; name=a (escaped \\); name=x)", None, 0),
				("Content-Type", "text/plain; name=\"a\\\"; name=x\"; name=b", Filename, 1),
				("Content-Type", "text/plain;; broken; name=a; name=b", Filename, 1),
				("Content-Type", "text/plain; name*999999999999999999=x; name=a", None, 0),
				("Content-Type", "text/plain; name*2147483647=a; name*2147483647=b", Filename, 1),
				("Content-Type", "text/plain; name*0x=a; name=a", None, 0),
				("Content-Type", "text/plain; name=a; name=", Filename, 1),
				("Content-Type", "text/plain; name=\"unterminated; name=b", None, 0),
				("Content-Type", "text/plain; name=a (unterminated; name=b", None, 0),
				("Content-Type", "text/plain", None, 0),
				("X-Parameters", "text/plain; name=a; name=b", None, 0)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true })
					yield return new TestCaseData (item.Field, item.Value, item.Violation, item.Count, chunked);
			}
		}

		static Stream CreateStream (string text, bool chunked)
		{
			var bytes = Encoding.UTF8.GetBytes (text);
			return chunked ? new ChunkedReadStream (bytes, 1) : new MemoryStream (bytes, false);
		}

		static List<MimeComplianceIssue> Read (string text, bool chunked, MimeComplianceOptions options = null)
		{
			var logger = new TestMimeComplianceLogger ();
			using (var stream = CreateStream (text, chunked)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };
				if (options != null)
					reader.ComplianceOptions = options;
				reader.ReadMessage ();
			}
			return logger.Issues;
		}

		static async Task<List<MimeComplianceIssue>> ReadAsync (string text, bool chunked, MimeComplianceOptions options = null)
		{
			var logger = new TestMimeComplianceLogger ();
			using (var stream = CreateStream (text, chunked)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };
				if (options != null)
					reader.ComplianceOptions = options;
				await reader.ReadMessageAsync ();
			}
			return logger.Issues;
		}

		static void AssertParameters (List<MimeComplianceIssue> issues, MimeComplianceViolation expected, int count)
		{
			var parameters = issues.Where (x => x.Violation == Boundary || x.Violation == Filename || x.Violation == Parameter).ToList ();
			Assert.That (parameters.Select (x => x.Violation), Is.EqualTo (Enumerable.Repeat (expected, count)));
			Assert.That (parameters.All (x => x.PositionKind == MimeCompliancePositionKind.Exact), Is.True);
		}

		[TestCaseSource (nameof (ParameterCases))]
		public void TestRepeatedParameters (string field, string value, MimeComplianceViolation expected, int count, bool chunked)
		{
			AssertParameters (Read ($"{field}: {value}\r\n\r\nbody\r\n", chunked), expected, count);
		}

		[TestCaseSource (nameof (ParameterCases))]
		public async Task TestRepeatedParametersAsync (string field, string value, MimeComplianceViolation expected, int count, bool chunked)
		{
			AssertParameters (await ReadAsync ($"{field}: {value}\r\n\r\nbody\r\n", chunked), expected, count);
		}

		static void AssertPosition (string text, string marker, MimeComplianceIssue issue, MimeCompliancePositionKind kind)
		{
			var prefix = Encoding.UTF8.GetBytes (text.Substring (0, text.IndexOf (marker, StringComparison.Ordinal)));
			int line = 1, column = 1;
			foreach (byte c in prefix) {
				if (c == (byte) '\n') {
					line++;
					column = 1;
				} else {
					column++;
				}
			}
			Assert.That (issue.StreamOffset, Is.EqualTo (prefix.Length));
			Assert.That (issue.LineNumber, Is.EqualTo (line));
			Assert.That (issue.ColumnNumber, Is.EqualTo (column));
			Assert.That (issue.PositionKind, Is.EqualTo (kind));
		}

		const string FoldedParameters = "X-Prefix: \u00e9\r\nContent-Type \t: text/plain; charset=utf-8;\r\n" +
			"\t(comment; charset=x)\r\n\t(\u00e9) CHARSET=us-ascii; charset=iso-8859-1\r\n\r\nbody\r\n";

		static void AssertFoldedPositions (List<MimeComplianceIssue> issues)
		{
			var parameters = issues.Where (x => x.Violation == Parameter).ToList ();
			Assert.That (parameters, Has.Count.EqualTo (2));
			AssertPosition (FoldedParameters, "CHARSET=", parameters[0], MimeCompliancePositionKind.Exact);
			AssertPosition (FoldedParameters, "charset=iso", parameters[1], MimeCompliancePositionKind.Exact);
		}

		[Test]
		public void TestFoldedParameterPositions ([Values (false, true)] bool chunked)
		{
			AssertFoldedPositions (Read (FoldedParameters, chunked));
		}

		[Test]
		public async Task TestFoldedParameterPositionsAsync ([Values (false, true)] bool chunked)
		{
			AssertFoldedPositions (await ReadAsync (FoldedParameters, chunked));
		}

		const string RepeatedHeaders = "Content-Type: text/plain\r\n" +
			"Content-Type: text/plain; name=a; name=b\r\n" +
			"Content-Disposition: attachment; filename=a\r\n" +
			"Content-Disposition: inline; filename=b; filename=c\r\n" +
			"Content-Disposition: attachment\r\n\r\nbody\r\n";

		static void AssertRepeatedHeaders (List<MimeComplianceIssue> issues, bool parameters)
		{
			AssertParameters (issues, Filename, parameters ? 2 : 0);
			var repeated = issues.Where (x => x.Violation == Disposition).ToList ();
			Assert.That (repeated, Has.Count.EqualTo (2));
			AssertPosition (RepeatedHeaders, "Content-Disposition: inline", repeated[0], MimeCompliancePositionKind.ElementStart);
			AssertPosition (RepeatedHeaders, "Content-Disposition: attachment\r\n", repeated[1], MimeCompliancePositionKind.ElementStart);
		}

		[Test]
		public void TestEveryHeaderIsChecked ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertRepeatedHeaders (Read (RepeatedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
		}

		[Test]
		public async Task TestEveryHeaderIsCheckedAsync ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertRepeatedHeaders (await ReadAsync (RepeatedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
		}

		const string NestedHeaders = "Content-Type: multipart/mixed; boundary=b\r\nContent-Disposition: inline\r\n\r\n" +
			"--b\r\nContent-Disposition: attachment; filename=a\r\n\r\none\r\n" +
			"--b\r\nContent-Type: message/rfc822\r\nContent-Disposition: inline\r\n\r\n" +
			"Content-Disposition: inline\r\nContent-Disposition: attachment\r\n\r\ntwo\r\n--b--\r\n";

		static void AssertNestedHeaders (List<MimeComplianceIssue> issues)
		{
			var repeated = issues.Where (x => x.Violation == Disposition).ToList ();
			Assert.That (repeated, Has.Count.EqualTo (1));
			AssertPosition (NestedHeaders, "Content-Disposition: attachment\r\n", repeated[0], MimeCompliancePositionKind.ElementStart);
		}

		[Test]
		public void TestDispositionStateResetsForEachHeaderBlock ([Values (false, true)] bool chunked)
		{
			AssertNestedHeaders (Read (NestedHeaders, chunked));
		}

		[Test]
		public async Task TestDispositionStateResetsForEachHeaderBlockAsync ([Values (false, true)] bool chunked)
		{
			AssertNestedHeaders (await ReadAsync (NestedHeaders, chunked));
		}

		const string MboxMessage = "From sender@example.com Mon Jan 1 00:00:00 2024\r\n" +
			"Content-Disposition: inline\r\n\r\nbody\r\n";

		[Test]
		public void TestDispositionStateResetsBetweenMessages ()
		{
			using (var stream = CreateStream (MboxMessage + MboxMessage, true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Mbox) { ComplianceLogger = logger };
				reader.ReadMessage ();
				reader.ReadMessage ();
				Assert.That (logger.Issues.Any (x => x.Violation == Disposition), Is.False);
			}
		}

		[Test]
		public async Task TestDispositionStateResetsBetweenMessagesAsync ()
		{
			using (var stream = CreateStream (MboxMessage + MboxMessage, true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (stream, MimeFormat.Mbox) { ComplianceLogger = logger };
				await reader.ReadMessageAsync ();
				await reader.ReadMessageAsync ();
				Assert.That (logger.Issues.Any (x => x.Violation == Disposition), Is.False);
			}
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesParameterValidation ()
		{
			using (var stream = CreateStream ("Content-Type: text/plain; x=1; x=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.Throws<FormatException> (() => reader.ReadMessage ());
				AssertParameters (logger.Issues, None, 0);
			}
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesParameterValidationAsync ()
		{
			using (var stream = CreateStream ("Content-Type: text/plain; x=1; x=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.ThrowsAsync<FormatException> (async () => await reader.ReadMessageAsync ());
				AssertParameters (logger.Issues, None, 0);
			}
		}

		const string LimitedParameters = "Content-Type: text/plain; x=1; x=2; x=3; x=4; boundary=a; boundary=b\r\n" +
			"Content-Disposition: attachment; filename=a; filename=b; filename=c\r\n" +
			"Content-Disposition: inline\r\nContent-Disposition: attachment\r\n\r\nbody\r\n";

		static void AssertSeparateBudgets (List<MimeComplianceIssue> issues)
		{
			foreach (var violation in new[] { Boundary, Filename, Parameter, Disposition, MimeComplianceViolation.TooManyComplianceIssues })
				Assert.That (issues.Count (x => x.Violation == violation), Is.EqualTo (1), violation.ToString ());
			Assert.That (issues.All (x => x.Context == MimeComplianceContext.Storage), Is.True);
		}

		[Test]
		public void TestSeparateReportingBudgets ()
		{
			AssertSeparateBudgets (Read (LimitedParameters, true, new MimeComplianceOptions { MaxIssuesPerViolation = 1, Context = MimeComplianceContext.Storage }));
		}

		[Test]
		public async Task TestSeparateReportingBudgetsAsync ()
		{
			AssertSeparateBudgets (await ReadAsync (LimitedParameters, true, new MimeComplianceOptions { MaxIssuesPerViolation = 1, Context = MimeComplianceContext.Storage }));
		}

		static byte[] AssertUnchangedMessage (MimeMessage message)
		{
			Assert.That (message.Body.ContentType.Name, Is.Null);
			Assert.That (message.Body.ContentDisposition.FileName, Is.EqualTo ("a"));
			Assert.That (message.Body, Is.InstanceOf<TextPart> ());
			Assert.That (((TextPart) message.Body).Text, Is.EqualTo ("body\r\n"));
			using (var stream = new MemoryStream ()) {
				message.WriteTo (stream);
				return stream.ToArray ();
			}
		}

		[Test]
		public void TestReportingDoesNotChangeParsing ()
		{
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.All }) {
					using (var stream = CreateStream (RepeatedHeaders, true)) {
						var parser = new MimeParser (stream);
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = new TestMimeComplianceLogger ();
						using (var message = parser.ParseMessage ()) {
							var actual = AssertUnchangedMessage (message);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
						}
					}
				}
			}
		}

		[Test]
		public async Task TestReportingDoesNotChangeParsingAsync ()
		{
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.All }) {
					using (var stream = CreateStream (RepeatedHeaders, true)) {
						var parser = new MimeParser (stream);
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = new TestMimeComplianceLogger ();
						using (var message = await parser.ParseMessageAsync ()) {
							var actual = AssertUnchangedMessage (message);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
						}
					}
				}
			}
		}

		[Test]
		public void TestValidatorReuseAndOffsets ()
		{
			const string value = "text/plain; x=1;\r\n x=2; x=3";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "yy");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Storage, HeaderId.ContentType, 100, 5, 7);
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, bytes.Length - 4);
				Assert.That (logger.Issues, Has.Count.EqualTo (2));
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (100 + value.IndexOf ("x=2")));
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (6));
				Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (2));
				Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (7));
			}
		}

		[Test]
		public void TestManyRepeatedParametersAreBounded ()
		{
			const int count = 16384;
			var bytes = Encoding.ASCII.GetBytes ("text/plain; x=0" + string.Concat (Enumerable.Repeat (";\r\n x=1", count)));
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			validator.Validate (bytes, 0, bytes.Length);
			watch.Stop ();
			Assert.That (logger.Issues, Has.Count.EqualTo (count));
			Assert.That (logger.Issues[count - 1].LineNumber, Is.EqualTo (count + 1));
			if (!TestHelper.IsCodeCoverageEnabled)
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
		}
	}
}
