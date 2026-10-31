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
		const MimeComplianceViolation MissingSegment = MimeComplianceViolation.MissingParameterSegment;
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

		// Continuation-gap cases inspired by https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> ContinuationCases ()
		{
			var cases = new (string Parameters, int Missing, int Repeated)[] {
				("filename*0=a", 0, 0),
				("filename*0=a; filename*1=b", 0, 0),
				("filename*1=b; filename*0=a", 0, 0),
				("filename*2=c; filename*0=a; filename*1=b", 0, 0),
				("filename*1=a; filename*2=b", 1, 0),
				("filename*0=a; filename*2=b", 1, 0),
				("filename*0=a; filename*0=b; filename*1=c", 0, 1),
				("filename*1=a; filename*1=b; filename*3=c", 1, 1),
				("filename*0=a; filename*2147483647=b", 1, 0),
				("filename*2147483647=a; filename*2147483647=b", 1, 1),
				("filename*0000=a; filename*0001=b", 0, 0),
				("filename=fallback; filename*1=b", 1, 0),
				("filename*1=b; filename=fallback", 1, 0),
				("filename*=us-ascii''fallback; filename*1=b", 1, 1),
				("filename*1=b; filename*=us-ascii''fallback", 1, 1),
				("filename*1*=b; filename*0*=us-ascii''a", 0, 0),
				("filename*0*=us-ascii''a; filename*2=b", 1, 0),
				("filename*0=a; filename*0*=b", 0, 1),
				("filename*2147483648=a; filename*1=b", 1, 0),
				("filename*9999999999999999999999=a; filename*0=b", 0, 0),
				("filename*=us-ascii''a", 0, 0),
				("filename*0=a; other*1=b; filename*2=c", 2, 0),
				("filename*2=a; other*1=b; filename*0=c; other*0=d", 1, 0),
				("boundary*0=a; boundary*2=b", 1, 0)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					yield return new TestCaseData ("Content-Disposition: attachment; " + item.Parameters, item.Missing, item.Repeated, chunked);
					yield return new TestCaseData ("Content-Type: text/plain; " + item.Parameters.Replace ("filename", "name"), item.Missing, item.Repeated, chunked);
				}
			}
		}

		static void AssertContinuationIssues (List<MimeComplianceIssue> issues, int missing, int repeated)
		{
			Assert.That (issues.Count (x => x.Violation == MissingSegment), Is.EqualTo (missing));
			Assert.That (issues.Where (x => x.Violation == MissingSegment).All (x => x.PositionKind == MimeCompliancePositionKind.ElementStart), Is.True);
			AssertParameters (issues, Filename, repeated);
		}

		[TestCaseSource (nameof (ContinuationCases))]
		public void TestContinuationSegments (string header, int missing, int repeated, bool chunked)
		{
			AssertContinuationIssues (Read (header + "\r\n\r\nbody\r\n", chunked), missing, repeated);
		}

		[TestCaseSource (nameof (ContinuationCases))]
		public async Task TestContinuationSegmentsAsync (string header, int missing, int repeated, bool chunked)
		{
			AssertContinuationIssues (await ReadAsync (header + "\r\n\r\nbody\r\n", chunked), missing, repeated);
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

		const string FoldedContinuations = "Content-Type: text/plain; z*0=a;\r\n\tname=fallback;\r\n" +
			"\t(\u00e9) name*2=b; z*0=c;\r\n\tother*1=x; name*4=d; other*3=y\r\n\r\nbody\r\n";

		static void AssertContinuationPositions (List<MimeComplianceIssue> issues)
		{
			var missing = issues.Where (x => x.Violation == MissingSegment).OrderBy (x => x.StreamOffset).ToList ();
			Assert.That (missing, Has.Count.EqualTo (2));
			AssertPosition (FoldedContinuations, "name*2=", missing[0], MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedContinuations, "other*1=", missing[1], MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedContinuations, "z*0=c", issues.Single (x => x.Violation == Parameter), MimeCompliancePositionKind.Exact);
		}

		[Test]
		public void TestContinuationPositions ([Values (false, true)] bool chunked)
		{
			AssertContinuationPositions (Read (FoldedContinuations, chunked));
		}

		[Test]
		public async Task TestContinuationPositionsAsync ([Values (false, true)] bool chunked)
		{
			AssertContinuationPositions (await ReadAsync (FoldedContinuations, chunked));
		}

		const string SeparateContinuations = "Content-Type: text/plain; name*0=a\r\n" +
			"Content-Type: text/plain; name*1=b\r\n" +
			"Content-Disposition: attachment; filename*0=a\r\n" +
			"Content-Disposition: attachment; filename*1=b\r\n\r\nbody\r\n";

		static void AssertSeparateContinuations (List<MimeComplianceIssue> issues, bool enabled)
		{
			AssertContinuationIssues (issues, enabled ? 2 : 0, 0);
			Assert.That (issues.Count (x => x.Violation == Disposition), Is.EqualTo (1));
			Assert.That (issues.Count (x => x.Violation == MimeComplianceViolation.RepeatedContentType), Is.EqualTo (1));
			if (enabled) {
				var missing = issues.Where (x => x.Violation == MissingSegment).ToList ();
				AssertPosition (SeparateContinuations, "name*1=b", missing[0], MimeCompliancePositionKind.ElementStart);
				AssertPosition (SeparateContinuations, "filename*1=b", missing[1], MimeCompliancePositionKind.ElementStart);
			}
		}

		[Test]
		public void TestContinuationSegmentsDoNotCrossHeaders ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertSeparateContinuations (Read (SeparateContinuations, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
		}

		[Test]
		public async Task TestContinuationSegmentsDoNotCrossHeadersAsync ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertSeparateContinuations (await ReadAsync (SeparateContinuations, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
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
			using (var stream = CreateStream ("Content-Type: text/plain; x*1=1; x*1=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.Throws<FormatException> (() => reader.ReadMessage ());
				AssertParameters (logger.Issues, None, 0);
				Assert.That (logger.Issues.Any (x => x.Violation == MissingSegment), Is.False);
			}
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesParameterValidationAsync ()
		{
			using (var stream = CreateStream ("Content-Type: text/plain; x*1=1; x*1=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.ThrowsAsync<FormatException> (async () => await reader.ReadMessageAsync ());
				AssertParameters (logger.Issues, None, 0);
				Assert.That (logger.Issues.Any (x => x.Violation == MissingSegment), Is.False);
			}
		}

		const string LimitedParameters = "Content-Type: text/plain; x=1; x=2; x=3; x=4; boundary=a; boundary=b\r\n" +
			"Content-Disposition: attachment; filename=a; filename=b; filename=c; a*1=x; b*3=y\r\n" +
			"Content-Disposition: inline\r\nContent-Disposition: attachment\r\n\r\nbody\r\n";

		static void AssertSeparateBudgets (List<MimeComplianceIssue> issues)
		{
			foreach (var violation in new[] { Boundary, Filename, Parameter, Disposition, MissingSegment, MimeComplianceViolation.TooManyComplianceIssues })
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

		const string ContinuedHeaders = "Content-Type: text/plain\r\n" +
			"Content-Disposition: attachment; filename*0=file.; filename*2=txt\r\n\r\nbody\r\n";

		static byte[] AssertUnchangedMessage (MimeMessage message, bool continued)
		{
			Assert.That (message.Body.ContentType.Name, Is.Null);
			Assert.That (message.Body.ContentDisposition.FileName, Is.EqualTo (continued ? "file.txt" : "a"));
			Assert.That (message.Body, Is.InstanceOf<TextPart> ());
			Assert.That (((TextPart) message.Body).Text, Is.EqualTo ("body\r\n"));
			using (var stream = new MemoryStream ()) {
				message.WriteTo (stream);
				return stream.ToArray ();
			}
		}

		[Test]
		public void TestReportingDoesNotChangeParsing ([Values (RepeatedHeaders, ContinuedHeaders)] string text)
		{
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.All }) {
					using (var stream = CreateStream (text, true)) {
						var parser = new MimeParser (stream);
						var logger = new TestMimeComplianceLogger ();
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = logger;
						using (var message = parser.ParseMessage ()) {
							var actual = AssertUnchangedMessage (message, text == ContinuedHeaders);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							Assert.That (logger.Issues.Count (x => x.Violation == MissingSegment),
								Is.EqualTo (logging && validators != MimeComplianceValidators.None && text == ContinuedHeaders ? 1 : 0));
						}
					}
				}
			}
		}

		[Test]
		public async Task TestReportingDoesNotChangeParsingAsync ([Values (RepeatedHeaders, ContinuedHeaders)] string text)
		{
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.None, MimeComplianceValidators.All }) {
					using (var stream = CreateStream (text, true)) {
						var parser = new MimeParser (stream);
						var logger = new TestMimeComplianceLogger ();
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = logger;
						using (var message = await parser.ParseMessageAsync ()) {
							var actual = AssertUnchangedMessage (message, text == ContinuedHeaders);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							Assert.That (logger.Issues.Count (x => x.Violation == MissingSegment),
								Is.EqualTo (logging && validators != MimeComplianceValidators.None && text == ContinuedHeaders ? 1 : 0));
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
		public void TestManyRepeatedParametersAreBounded ([Values (false, true)] bool continued)
		{
			const int count = 16384;
			string name = continued ? "x*0" : "x";
			var bytes = Encoding.ASCII.GetBytes ($"text/plain; {name}=0" + string.Concat (Enumerable.Repeat ($";\r\n {name}=1", count)));
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

		[Test]
		public void TestContinuationValidatorReuseAndOffsets ()
		{
			const string value = "text/plain; name*2=x;\r\n name*2=y";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "yy");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Storage, HeaderId.ContentType, 100, 5, 7);
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, bytes.Length - 4);
				AssertContinuationIssues (logger.Issues, 1, 1);
				var missing = logger.Issues.Single (x => x.Violation == MissingSegment);
				Assert.That (missing.StreamOffset, Is.EqualTo (100 + value.IndexOf ("name*2")));
				Assert.That (missing.LineNumber, Is.EqualTo (5));
				Assert.That (missing.ColumnNumber, Is.EqualTo (7 + value.IndexOf ("name*2")));
				Assert.That (missing.Context, Is.EqualTo (MimeComplianceContext.Storage));

				logger.Issues.Clear ();
				var complete = Encoding.ASCII.GetBytes ("text/plain; name*0=a; name*1=b");
				validator.Validate (complete, 0, complete.Length);
				Assert.That (logger.Issues, Is.Empty);
			}
		}

		[Test]
		public void TestSegmentTrackingIsBoundedByDeclarationCount ([Values (false, true)] bool sparse)
		{
			const int count = 4096;
			var text = new StringBuilder ("attachment");
			int last = 0;
			for (int i = 0; i < count; i++) {
				last = sparse ? int.MaxValue - i * 9059 : count - i - 1;
				text.Append ("; filename*").Append (last).Append ("=x");
			}
			text.Append ("; filename*").Append (last).Append ("=y");
			var bytes = Encoding.ASCII.GetBytes (text.ToString ());
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentDisposition, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertContinuationIssues (logger.Issues, sparse ? 1 : 0, 1);
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (count * 256L + 65536));
			}
		}
	}
}
