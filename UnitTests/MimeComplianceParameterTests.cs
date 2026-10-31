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
		const MimeComplianceViolation Invalid = MimeComplianceViolation.InvalidParameter;
		const MimeComplianceViolation InvalidEncoded = MimeComplianceViolation.InvalidRfc2231Parameter;
		const MimeComplianceViolation EncodedWord = MimeComplianceViolation.EncodedWordInParameter;
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

		// Malformed parameter cases inspired by https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> InvalidParameterCases ()
		{
			var cases = new (string Parameters, int Invalid)[] {
				("", 0),
				(" ; ; (comment)", 0),
				("(nested (comment; filename=ignored))", 0),
				("filename=\"\"", 0),
				("filename=(comment) \"file.txt\"", 0),
				("filename=unquoted file.txt", 0),
				("filename=unquoted\r\n\tfile.txt", 0),
				("filename=\"file.txt\" filename=other.txt", 0),
				("filename=file.txt(comment)filename=other.txt", 0),
				("filename=\"file; \\\"name\\\".txt\"; size=1", 0),
				("filename*=''file.txt", 0),
				("filename*=utf-8''%GG", 0),
				("filename*=no-prefix", 0),
				("filename*0=file; filename*2=.txt", 0),
				("filename", 1),
				("filename value", 1),
				("filename (comment)", 1),
				("=file.txt", 1),
				("\"filename\"=file.txt", 1),
				("file/name=file.txt", 1),
				("filenam\u00e9=file.txt", 1),
				("filename*", 1),
				("filename*0", 1),
				("filename*0x=file.txt", 1),
				("filename**=file.txt", 1),
				("filename*2147483648=file.txt", 1),
				("filename=", 1),
				("filename= \t ; size=1", 1),
				("filename=(comment)", 1),
				("filename=(comment); size=1", 1),
				("filename=\"unterminated; size=1", 1),
				("filename=file(comment", 1),
				("filename (comment", 1),
				("(unterminated", 1),
				("filename=\"file\".zip", 1),
				("filename; =bad; size=1", 2),
				("\"bad; ignored=1\"=x; filename=file.txt", 1),
				("broken (comment; ignored=1); filename=a; filename=b", 1),
				("filename=; filename=good; filename=again", 1),
				("filename*0=; filename*2=x", 1)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					yield return new TestCaseData ("Content-Disposition: attachment; " + item.Parameters, item.Invalid, chunked);
					yield return new TestCaseData ("Content-Type: text/plain; " + item.Parameters.Replace ("filename", "name"), item.Invalid, chunked);
				}
			}

			foreach (bool chunked in new[] { false, true }) {
				yield return new TestCaseData ("X-Parameters: attachment; broken", 0, chunked);
				yield return new TestCaseData ("Content-Type: text/plain junk", 0, chunked);
				yield return new TestCaseData ("Content-Disposition: \"attachment; broken\"", 0, chunked);
				yield return new TestCaseData ("Content-Disposition: attachment (comment; broken)", 0, chunked);
				yield return new TestCaseData ("Content-Disposition: attachment (comment; broken); filename", 1, chunked);
			}
		}

		static void AssertInvalidParameters (List<MimeComplianceIssue> issues, int count)
		{
			var invalid = issues.Where (x => x.Violation == Invalid).ToList ();
			Assert.That (invalid, Has.Count.EqualTo (count));
			Assert.That (invalid.All (x => x.PositionKind == MimeCompliancePositionKind.ElementStart), Is.True);
		}

		[TestCaseSource (nameof (InvalidParameterCases))]
		public void TestInvalidParameters (string header, int count, bool chunked)
		{
			AssertInvalidParameters (Read (header + "\r\n\r\nbody\r\n", chunked), count);
		}

		[TestCaseSource (nameof (InvalidParameterCases))]
		public async Task TestInvalidParametersAsync (string header, int count, bool chunked)
		{
			AssertInvalidParameters (await ReadAsync (header + "\r\n\r\nbody\r\n", chunked), count);
		}

		// Regression cases inspired by https://github.com/noxxi/mime-is-broken and RFC 2231 sections 4 and 7.
		static IEnumerable<TestCaseData> EncodedParameterCases ()
		{
			var cases = new (string Parameters, int Invalid)[] {
				("filename*=utf-8''file%20name.txt", 0),
				("filename*=''file.txt", 0),
				("filename*='en'file.txt", 0),
				("filename*=utf-8'en-US'file.txt", 0),
				("filename*=''", 0),
				("filename*=x-unsupported-charset''file.txt", 0),
				("filename*=ISO_8859-1:1987''file.txt", 0),
				("filename*=x-%charset''file.txt", 0),
				("filename*=\"''file%2Etxt\"", 0),
				("filename*=utf-8''caf%c3%a9", 0),
				("filename*=utf-8''%00%FF", 0),
				("filename*0*=utf-8''%C3; filename*1*=%A9.txt", 0),
				("filename*1*=%A9.txt; filename*0*=utf-8''%C3", 0),
				("filename*0*=''file; filename*1=.txt; filename*2*=%21", 0),
				("filename*0=file; filename*1*=%2Etxt", 0),
				("filename*1*=%2Etxt", 0),
				("filename*0*=''; filename*1*=file.txt", 0),
				("filename*0*=''file; filename*1*=\"\"", 0),
				("filename*0*=''file; filename*1*=", 0),
				("filename*0*=''file; filename*1*=; filename*2*=.txt", 0),
				("filename*0*=''file; filename*1*=(comment)", 0),
				("filename*=''file (comment; ignored=%GG)", 0),
				("filename*=''file\r\n\t(comment)", 0),
				("filename=%GG; other*0=%; other*1=GG", 0),
				("filename*=file.txt", 1),
				("filename*=utf-8'file.txt", 1),
				("filename*='file.txt", 1),
				("filename*=\"\"", 1),
				("filename*=utf 8''file.txt", 1),
				("filename*=utf-8'en US'file.txt", 1),
				("filename*=utf-8''file%", 1),
				("filename*=utf-8''file%2", 1),
				("filename*=utf-8''file%GG", 1),
				("filename*=utf-8''file%2G%G2", 1),
				("filename*=utf-8''file%%20", 1),
				("filename*=utf-8''file name.txt", 1),
				("filename*=utf-8''file\r\n\tname.txt", 1),
				("filename*=utf-8''caf\u00e9", 1),
				("filename*=utf-8''file*name", 1),
				("filename*=utf-8''file'name", 1),
				("filename*=\"utf-8''file%GG\"", 1),
				("filename*0*=file; filename*1*=%2Etxt", 1),
				("filename*0*=''file; filename*1*=utf-8''txt", 1),
				("filename*0*=''file%; filename*1*=20name", 1),
				("filename*1*=0name; filename*0*=''file%2", 1),
				("filename*0*=utf-8''caf%C; filename*1*=3%A9.txt", 1),
				("filename*0*=''file%; filename*1=20name", 1),
				("filename*0*=''file%; filename*2*=20name", 1),
				("filename*=''file%; filename*=''other%GG", 2),
				("filename*=bad; other*=bad", 2)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					yield return new TestCaseData ("Content-Disposition: attachment; " + item.Parameters, item.Invalid, chunked);
					yield return new TestCaseData ("Content-Type: text/plain; " + item.Parameters.Replace ("filename", "name"), item.Invalid, chunked);
				}
			}
		}

		static void AssertInvalidEncodedParameters (List<MimeComplianceIssue> issues, int count)
		{
			var invalid = issues.Where (x => x.Violation == InvalidEncoded).ToList ();
			Assert.That (invalid, Has.Count.EqualTo (count));
			Assert.That (invalid.All (x => x.PositionKind == MimeCompliancePositionKind.ElementStart), Is.True);
		}

		[TestCaseSource (nameof (EncodedParameterCases))]
		public void TestEncodedParameters (string header, int count, bool chunked)
		{
			var issues = Read (header + "\r\n\r\nbody\r\n", chunked);
			AssertInvalidEncodedParameters (issues, count);
			AssertInvalidParameters (issues, 0);
		}

		[TestCaseSource (nameof (EncodedParameterCases))]
		public async Task TestEncodedParametersAsync (string header, int count, bool chunked)
		{
			var issues = await ReadAsync (header + "\r\n\r\nbody\r\n", chunked);
			AssertInvalidEncodedParameters (issues, count);
			AssertInvalidParameters (issues, 0);
		}

		// Regression cases inspired by https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> EncodedWordCases ()
		{
			var cases = new (string Parameters, int Count)[] {
				("filename=\"=?utf-8?B?ZmlsZS50eHQ=?=\"", 1),
				("filename==?utf-8?Q?file=2Etxt?=", 1),
				("filename=\"=?UTF-8?q?file_name.txt?=\"", 1),
				("filename=\"=?utf-8?b?ZmlsZS50eHQ=?=\"", 1),
				("filename=\"=?x-unknown?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8*en?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8*?Q?file.txt?=\"", 1),
				("filename=\"prefix=?utf-8?Q?file?=.txt\"", 1),
				("filename=\"=?utf-8?Q?file?= =?utf-8?Q?.txt?=\"", 1),
				("filename=\"=?utf-8?Q?file?=\r\n\t=?utf-8?Q?.txt?=\"", 1),
				("filename=\"=?utf-8?Q?file?=\"; filename=\"=?utf-8?Q?other?=\"", 2),
				("filename=\"=\\?utf-8\\?Q\\?file.txt?\\=\"", 1),
				("filename=\"\\=\\?utf-8\\?Q\\?file.txt\\?\\=\"", 1),
				("filename=\"=?utf-8?Q?file\\_name.txt?=\"", 1),
				("filename=\"=?utf-8?Q?file.txt?=\" filename=plain", 1),
				("filename=(comment) \"=?utf-8?Q?file.txt?=\" (comment)", 1),
				("filename=\"(=?utf-8?Q?file.txt?=)\"", 1),
				("filename=\"=?broken=?utf-8?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8?Q?broken=?utf-8?B?ZmlsZQ==?=\"", 1),
				("filename=\"=?utf-8?Q?=?utf-8?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8?X?bad?==?utf-8?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8?Q??==?utf-8?Q?file.txt?=\"", 1),
				("filename=\"=?utf-8?B?not-base64!?=\"", 1),
				("filename=\"=?utf-8?Q?bad=GG?=\"", 1),
				("filename=\"=?utf-8?Q?" + new string ('a', 100) + "?=\"", 1),
				("filename*0=\"=?utf-8?Q?file?=\"; filename*1=.txt", 1),
				("filename*2=\"=?utf-8?Q?file?=\"", 1),
				("filename*=\"utf-8''=?utf-8?Q?file?=\"", 1),
				("filename*=utf-8''%3D%3Futf-8%3FQ%3Ffile%3F%3D", 0),
				("filename*0=\"=?utf-8?Q?file\"; filename*1=\"?=\"", 0),
				("filename=(=?utf-8?Q?ignored?=) plain", 0),
				("filename=plain (=?utf-8?Q?ignored?=)", 0),
				("(=?utf-8?Q?ignored?=); filename=plain", 0),
				("filename=\"\"", 0),
				("filename=\"=?\"", 0),
				("filename=\"=?utf-8?Q?unfinished\"", 0),
				("filename=\"=?utf-8?Q?unfinished?\"", 0),
				("filename=\"=?utf-8?Q??=\"", 0),
				("filename=\"=??Q?file?=\"", 0),
				("filename=\"=?*en?Q?file?=\"", 0),
				("filename=\"=?utf-8?X?file?=\"", 0),
				("filename=\"=?utf-8?QQ?file?=\"", 0),
				("filename=\"=?utf-8?Qfile?=\"", 0),
				("filename=\"=?utf 8?Q?file?=\"", 0),
				("filename=\"=?utf.8?Q?file?=\"", 0),
				("filename=\"=?utf-8?Q?file name?=\"", 0),
				("filename=\"=?utf-8?Q?file\tname?=\"", 0),
				("filename=\"=?utf-8?Q?caf\u00e9?=\"", 0),
				("filename=\"=?utf-8?Q?file?name?=\"", 0),
				("filename=\"=?utf-8?Q?file\r\n\tname?=\"", 0)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					yield return new TestCaseData ("Content-Disposition: attachment; " + item.Parameters, item.Count, chunked);
					yield return new TestCaseData ("Content-Type: text/plain; " + item.Parameters.Replace ("filename", "name"), item.Count, chunked);
				}
			}

			foreach (bool chunked in new[] { false, true }) {
				yield return new TestCaseData ("Subject: =?utf-8?Q?subject?=", 0, chunked);
				yield return new TestCaseData ("X-Parameters: attachment; filename=\"=?utf-8?Q?file?=\"", 0, chunked);
			}
		}

		static void AssertEncodedWords (List<MimeComplianceIssue> issues, int count)
		{
			var words = issues.Where (x => x.Violation == EncodedWord).ToList ();
			Assert.That (words, Has.Count.EqualTo (count));
			Assert.That (words.All (x => x.PositionKind == MimeCompliancePositionKind.ElementStart), Is.True);
		}

		[TestCaseSource (nameof (EncodedWordCases))]
		public void TestEncodedWordsInParameters (string header, int count, bool chunked)
		{
			AssertEncodedWords (Read (header + "\r\n\r\nbody\r\n", chunked), count);
		}

		[TestCaseSource (nameof (EncodedWordCases))]
		public async Task TestEncodedWordsInParametersAsync (string header, int count, bool chunked)
		{
			AssertEncodedWords (await ReadAsync (header + "\r\n\r\nbody\r\n", chunked), count);
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

		const string MalformedParameters = "X-Prefix: \u00e9\r\nContent-Type: text/plain; z*2=x;\r\n" +
			"\t(\u00e9) name;\r\n\tz*2=y; filename=;\r\n\tsize=\"unterminated\r\n\r\nbody\r\n";

		static void AssertMalformedPositions (List<MimeComplianceIssue> issues)
		{
			AssertInvalidParameters (issues, 3);
			var invalid = issues.Where (x => x.Violation == Invalid).ToList ();
			AssertPosition (MalformedParameters, "name;", invalid[0], MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedParameters, "filename=", invalid[1], MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedParameters, "size=", invalid[2], MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedParameters, "z*2=x", issues.Single (x => x.Violation == MissingSegment), MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedParameters, "z*2=y", issues.Single (x => x.Violation == Parameter), MimeCompliancePositionKind.Exact);
		}

		[Test]
		public void TestMalformedParameterPositions ([Values (false, true)] bool chunked)
		{
			AssertMalformedPositions (Read (MalformedParameters, chunked));
		}

		[Test]
		public async Task TestMalformedParameterPositionsAsync ([Values (false, true)] bool chunked)
		{
			AssertMalformedPositions (await ReadAsync (MalformedParameters, chunked));
		}

		const string MalformedEncodedParameters = "X-Prefix: \u00e9\r\nContent-Type: text/plain; x*2*=%GG;\r\n" +
			"\t(\u00e9) name*=no-prefix;\r\n\tx*2*=%Q0; broken; other*=''bad%\r\n\r\nbody\r\n";

		static void AssertMalformedEncodedPositions (List<MimeComplianceIssue> issues)
		{
			AssertInvalidEncodedParameters (issues, 4);
			var invalid = issues.Where (x => x.Violation == InvalidEncoded).ToList ();
			var fragments = new[] { "x*2*=%GG", "name*=", "x*2*=%Q0", "other*=" };
			for (int i = 0; i < fragments.Length; i++)
				AssertPosition (MalformedEncodedParameters, fragments[i], invalid[i], MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedEncodedParameters, "x*2*=%GG", issues.Single (x => x.Violation == MissingSegment), MimeCompliancePositionKind.ElementStart);
			AssertPosition (MalformedEncodedParameters, "x*2*=%Q0", issues.Single (x => x.Violation == Parameter), MimeCompliancePositionKind.Exact);
			AssertPosition (MalformedEncodedParameters, "broken;", issues.Single (x => x.Violation == Invalid), MimeCompliancePositionKind.ElementStart);
		}

		[Test]
		public void TestMalformedEncodedParameterPositions ([Values (false, true)] bool chunked)
		{
			AssertMalformedEncodedPositions (Read (MalformedEncodedParameters, chunked));
		}

		[Test]
		public async Task TestMalformedEncodedParameterPositionsAsync ([Values (false, true)] bool chunked)
		{
			AssertMalformedEncodedPositions (await ReadAsync (MalformedEncodedParameters, chunked));
		}

		const string IgnoredEncodedHeaders = "Content-Type: text/plain\r\n" +
			"Content-Type: text/html; name*=bad; title=\"=?utf-8?Q?ignored?=\"\r\n" +
			"Content-Disposition: attachment; filename=a\r\n" +
			"Content-Disposition: inline; filename*=utf-8''%GG; info=\"=?utf-8?Q?ignored?=\"\r\n\r\nbody\r\n";

		static void AssertIgnoredEncodedHeaders (List<MimeComplianceIssue> issues, MimeComplianceValidators validators)
		{
			bool enabled = (validators & MimeComplianceValidators.Parameters) != 0;
			AssertInvalidEncodedParameters (issues, enabled ? 2 : 0);
			AssertEncodedWords (issues, enabled ? 2 : 0);
			if (enabled) {
				var invalid = issues.Where (x => x.Violation == InvalidEncoded).ToList ();
				AssertPosition (IgnoredEncodedHeaders, "name*=", invalid[0], MimeCompliancePositionKind.ElementStart);
				AssertPosition (IgnoredEncodedHeaders, "filename*=", invalid[1], MimeCompliancePositionKind.ElementStart);
				var words = issues.Where (x => x.Violation == EncodedWord).ToList ();
				AssertPosition (IgnoredEncodedHeaders, "title=", words[0], MimeCompliancePositionKind.ElementStart);
				AssertPosition (IgnoredEncodedHeaders, "info=", words[1], MimeCompliancePositionKind.ElementStart);
			}
		}

		const string FoldedEncodedWords = "X-Prefix: \u00e9\r\nContent-Type: text/plain;\r\n" +
			"\t(\u00e9) name*2=\"=?utf-8?Q?first?=\";\r\n\tname*2=\"=?utf-8?Q?second?=\";\r\n" +
			"\tbad*=\"utf-8''=?utf-8?Q?third?=\"\r\n\r\nbody\r\n";

		static void AssertEncodedWordPositions (List<MimeComplianceIssue> issues)
		{
			AssertEncodedWords (issues, 3);
			var words = issues.Where (x => x.Violation == EncodedWord).ToList ();
			AssertPosition (FoldedEncodedWords, "name*2=\"=?utf-8?Q?first", words[0], MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedEncodedWords, "name*2=\"=?utf-8?Q?second", words[1], MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedEncodedWords, "bad*=", words[2], MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedEncodedWords, "name*2=\"=?utf-8?Q?first", issues.Single (x => x.Violation == MissingSegment), MimeCompliancePositionKind.ElementStart);
			AssertPosition (FoldedEncodedWords, "name*2=\"=?utf-8?Q?second", issues.Single (x => x.Violation == Filename), MimeCompliancePositionKind.Exact);
			AssertPosition (FoldedEncodedWords, "bad*=", issues.Single (x => x.Violation == InvalidEncoded), MimeCompliancePositionKind.ElementStart);
		}

		[Test]
		public void TestEncodedWordPositions ([Values (false, true)] bool chunked)
		{
			AssertEncodedWordPositions (Read (FoldedEncodedWords, chunked));
		}

		[Test]
		public async Task TestEncodedWordPositionsAsync ([Values (false, true)] bool chunked)
		{
			AssertEncodedWordPositions (await ReadAsync (FoldedEncodedWords, chunked));
		}

		[Test]
		public void TestEncodedParametersInIgnoredHeaders ([Values (false, true)] bool chunked,
			[Values (MimeComplianceValidators.Parameters, MimeComplianceValidators.All & ~MimeComplianceValidators.Parameters)] MimeComplianceValidators validators)
		{
			AssertIgnoredEncodedHeaders (Read (IgnoredEncodedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators);
		}

		[Test]
		public async Task TestEncodedParametersInIgnoredHeadersAsync ([Values (false, true)] bool chunked,
			[Values (MimeComplianceValidators.Parameters, MimeComplianceValidators.All & ~MimeComplianceValidators.Parameters)] MimeComplianceValidators validators)
		{
			AssertIgnoredEncodedHeaders (await ReadAsync (IgnoredEncodedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators);
		}

		const string IgnoredMalformedHeaders = "Content-Type: text/plain\r\n" +
			"Content-Type: text/html; name\r\n" +
			"Content-Disposition: attachment; filename=a\r\n" +
			"Content-Disposition: inline; filename=\r\n\r\nbody\r\n";

		static void AssertIgnoredMalformedHeaders (List<MimeComplianceIssue> issues, bool enabled)
		{
			AssertInvalidParameters (issues, enabled ? 2 : 0);
			if (enabled) {
				var invalid = issues.Where (x => x.Violation == Invalid).ToList ();
				AssertPosition (IgnoredMalformedHeaders, "name\r\n", invalid[0], MimeCompliancePositionKind.ElementStart);
				AssertPosition (IgnoredMalformedHeaders, "filename=\r\n", invalid[1], MimeCompliancePositionKind.ElementStart);
			}
		}

		[Test]
		public void TestMalformedParametersInIgnoredHeaders ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertIgnoredMalformedHeaders (Read (IgnoredMalformedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
		}

		[Test]
		public async Task TestMalformedParametersInIgnoredHeadersAsync ([Values (false, true)] bool chunked, [Values (MimeComplianceValidators.All, MimeComplianceValidators.None)] MimeComplianceValidators validators)
		{
			AssertIgnoredMalformedHeaders (await ReadAsync (IgnoredMalformedHeaders, chunked, new MimeComplianceOptions { EnabledValidators = validators }), validators != MimeComplianceValidators.None);
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
			using (var stream = CreateStream ("Content-Type: text/plain; broken; bad*=missing; word=\"=?utf-8?Q?file?=\"; x*1=1; x*1=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.Throws<FormatException> (() => reader.ReadMessage ());
				AssertParameters (logger.Issues, None, 0);
				Assert.That (logger.Issues.Any (x => x.Violation == MissingSegment), Is.False);
				AssertInvalidParameters (logger.Issues, 0);
				AssertInvalidEncodedParameters (logger.Issues, 0);
				AssertEncodedWords (logger.Issues, 0);
			}
		}

		[Test]
		public void TestHeaderLengthLimitPrecedesParameterValidationAsync ()
		{
			using (var stream = CreateStream ("Content-Type: text/plain; broken; bad*=missing; word=\"=?utf-8?Q?file?=\"; x*1=1; x*1=2; x=" + new string ('a', 256) + "\r\n\r\n", true)) {
				var logger = new TestMimeComplianceLogger ();
				var reader = new MimeReader (new ParserOptions { MaxHeaderLength = 128 }, stream) { ComplianceLogger = logger };
				Assert.ThrowsAsync<FormatException> (async () => await reader.ReadMessageAsync ());
				AssertParameters (logger.Issues, None, 0);
				Assert.That (logger.Issues.Any (x => x.Violation == MissingSegment), Is.False);
				AssertInvalidParameters (logger.Issues, 0);
				AssertInvalidEncodedParameters (logger.Issues, 0);
				AssertEncodedWords (logger.Issues, 0);
			}
		}

		const string LimitedParameters = "Content-Type: text/plain; x=1; x=2; x=3; x=4; boundary=a; boundary=b\r\n" +
			"Content-Disposition: attachment; filename=a; filename=b; filename=c; a*1=x; b*3=y; enc*=bad; other*=bad;\r\n" +
			"\tword=\"=?utf-8?Q?first?=\"; word=\"=?utf-8?Q?second?=\"; bad; broken=\r\n" +
			"Content-Disposition: inline\r\nContent-Disposition: attachment\r\n\r\nbody\r\n";

		static void AssertSeparateBudgets (List<MimeComplianceIssue> issues)
		{
			foreach (var violation in new[] { Boundary, Filename, Parameter, Disposition, MissingSegment, Invalid, InvalidEncoded, EncodedWord, MimeComplianceViolation.TooManyComplianceIssues })
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

		const string InvalidHeaders = "Content-Type: text/plain; name\r\n" +
			"Content-Disposition: attachment; filename=a\r\n\r\nbody\r\n";

		static byte[] AssertUnchangedMessage (MimeMessage message, string filename, string name = null)
		{
			Assert.That (message.Body.ContentType.Name, Is.EqualTo (name));
			Assert.That (message.Body.ContentDisposition.FileName, Is.EqualTo (filename));
			Assert.That (message.Body, Is.InstanceOf<TextPart> ());
			Assert.That (((TextPart) message.Body).Text, Is.EqualTo ("body\r\n"));
			using (var stream = new MemoryStream ()) {
				message.WriteTo (stream);
				return stream.ToArray ();
			}
		}

		[Test]
		public void TestReportingDoesNotChangeParsing ([Values (RepeatedHeaders, ContinuedHeaders, InvalidHeaders, IgnoredMalformedHeaders, IgnoredEncodedHeaders)] string text)
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
							var actual = AssertUnchangedMessage (message, text == ContinuedHeaders ? "file.txt" : "a");
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							Assert.That (logger.Issues.Count (x => x.Violation == MissingSegment),
								Is.EqualTo (logging && validators != MimeComplianceValidators.None && text == ContinuedHeaders ? 1 : 0));
							int invalid = text == InvalidHeaders ? 1 : text == IgnoredMalformedHeaders ? 2 : 0;
							AssertInvalidParameters (logger.Issues, logging && validators != MimeComplianceValidators.None ? invalid : 0);
							AssertInvalidEncodedParameters (logger.Issues, logging && validators != MimeComplianceValidators.None && text == IgnoredEncodedHeaders ? 2 : 0);
							AssertEncodedWords (logger.Issues, logging && validators != MimeComplianceValidators.None && text == IgnoredEncodedHeaders ? 2 : 0);
						}
					}
				}
			}
		}

		[Test]
		public async Task TestReportingDoesNotChangeParsingAsync ([Values (RepeatedHeaders, ContinuedHeaders, InvalidHeaders, IgnoredMalformedHeaders, IgnoredEncodedHeaders)] string text)
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
							var actual = AssertUnchangedMessage (message, text == ContinuedHeaders ? "file.txt" : "a");
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							Assert.That (logger.Issues.Count (x => x.Violation == MissingSegment),
								Is.EqualTo (logging && validators != MimeComplianceValidators.None && text == ContinuedHeaders ? 1 : 0));
							int invalid = text == InvalidHeaders ? 1 : text == IgnoredMalformedHeaders ? 2 : 0;
							AssertInvalidParameters (logger.Issues, logging && validators != MimeComplianceValidators.None ? invalid : 0);
							AssertInvalidEncodedParameters (logger.Issues, logging && validators != MimeComplianceValidators.None && text == IgnoredEncodedHeaders ? 2 : 0);
							AssertEncodedWords (logger.Issues, logging && validators != MimeComplianceValidators.None && text == IgnoredEncodedHeaders ? 2 : 0);
						}
					}
				}
			}
		}

		// Preserve recovery for malformed encodings encountered in https://github.com/noxxi/mime-is-broken.
		static IEnumerable<TestCaseData> EncodedPreservationCases ()
		{
			var cases = new (string Parameters, string Filename, int Invalid, int Words)[] {
				("filename*0*=utf-8''caf%C3; filename*1*=%A9.txt", "caf\u00e9.txt", 0, 0),
				("filename*0*=utf-8''caf%C; filename*1*=3%A9.txt", "caf\u00e9.txt", 1, 0),
				("filename*1*=3%A9.txt; filename*0*=utf-8''caf%C", "caf\u00e9.txt", 1, 0),
				("filename*0*=\"utf-8''caf%\"; filename*1*=\"C3%A9.txt\"", "caf\u00e9.txt", 1, 0),
				("filename*=''file%GG.txt", "file%GG.txt", 1, 0),
				("filename*=utf-8'file.txt", "utf-8'file.txt", 1, 0),
				("filename*=x-unsupported-charset''caf%E9.txt", "caf\u00e9.txt", 0, 0),
				("filename*0=file; filename*1*=%2Etxt", "file.txt", 0, 0),
				("filename=\"=?utf-8?Q?caf=C3=A9.txt?=\"", "caf\u00e9.txt", 0, 1),
				("filename=\"=?utf-8?B?ZmlsZS50eHQ=?=\"", "file.txt", 0, 1),
				("filename=\"=?utf-8?Q?file?=\r\n\t=?utf-8?Q?.txt?=\"", "file.txt", 0, 1),
				("filename=\"=\\?utf-8?Q?file=2Etxt?\\=\"", "file.txt", 0, 1),
				("filename=\"=?x-unknown?Q?file.txt?=\"", "file.txt", 0, 1),
				("filename*0=\"=?utf-8?Q?file?=\"; filename*1=.txt", "=?utf-8?Q?file?=.txt", 0, 1),
				("filename*=utf-8''%3D%3Futf-8%3FQ%3Ffile%3F%3D", "=?utf-8?Q?file?=", 0, 0)
			};

			foreach (var item in cases) {
				foreach (bool chunked in new[] { false, true }) {
					foreach (bool contentType in new[] { false, true }) {
						foreach (var mode in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose })
							yield return new TestCaseData (item.Parameters, item.Filename, item.Invalid, item.Words, chunked, contentType, mode);
					}
				}
			}
		}

		[TestCaseSource (nameof (EncodedPreservationCases))]
		public void TestEncodedReportingDoesNotChangeParsing (string parameters, string filename, int count, int words, bool chunked, bool contentType, RfcComplianceMode mode)
		{
			string text = contentType ?
				"Content-Type: text/plain; " + parameters.Replace ("filename", "name") + "\r\nContent-Disposition: attachment; filename=a\r\n\r\nbody\r\n" :
				"Content-Type: text/plain\r\nContent-Disposition: attachment; " + parameters + "\r\n\r\nbody\r\n";
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.All, MimeComplianceValidators.All & ~MimeComplianceValidators.Parameters }) {
					using (var stream = CreateStream (text, chunked)) {
						var parser = new MimeParser (new ParserOptions { ParameterComplianceMode = mode }, stream);
						var logger = new TestMimeComplianceLogger ();
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = logger;
						using (var message = parser.ParseMessage ()) {
							var actual = AssertUnchangedMessage (message, contentType ? "a" : filename, contentType ? filename : null);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							AssertInvalidEncodedParameters (logger.Issues, logging && (validators & MimeComplianceValidators.Parameters) != 0 ? count : 0);
							AssertEncodedWords (logger.Issues, logging && (validators & MimeComplianceValidators.Parameters) != 0 ? words : 0);
						}
					}
				}
			}
		}

		[TestCaseSource (nameof (EncodedPreservationCases))]
		public async Task TestEncodedReportingDoesNotChangeParsingAsync (string parameters, string filename, int count, int words, bool chunked, bool contentType, RfcComplianceMode mode)
		{
			string text = contentType ?
				"Content-Type: text/plain; " + parameters.Replace ("filename", "name") + "\r\nContent-Disposition: attachment; filename=a\r\n\r\nbody\r\n" :
				"Content-Type: text/plain\r\nContent-Disposition: attachment; " + parameters + "\r\n\r\nbody\r\n";
			byte[] expected = null;
			foreach (bool logging in new[] { false, true }) {
				foreach (var validators in new[] { MimeComplianceValidators.All, MimeComplianceValidators.All & ~MimeComplianceValidators.Parameters }) {
					using (var stream = CreateStream (text, chunked)) {
						var parser = new MimeParser (new ParserOptions { ParameterComplianceMode = mode }, stream);
						var logger = new TestMimeComplianceLogger ();
						parser.ComplianceOptions.EnabledValidators = validators;
						if (logging)
							parser.ComplianceLogger = logger;
						using (var message = await parser.ParseMessageAsync ()) {
							var actual = AssertUnchangedMessage (message, contentType ? "a" : filename, contentType ? filename : null);
							expected ??= actual;
							Assert.That (actual, Is.EqualTo (expected));
							AssertInvalidEncodedParameters (logger.Issues, logging && (validators & MimeComplianceValidators.Parameters) != 0 ? count : 0);
							AssertEncodedWords (logger.Issues, logging && (validators & MimeComplianceValidators.Parameters) != 0 ? words : 0);
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
		public void TestMalformedValidatorReuseAndOffsets ()
		{
			const string value = "text/plain; broken;\r\n x=";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "yy");
			var valid = Encoding.ASCII.GetBytes ("text/plain; x=\"\"");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Storage, HeaderId.ContentType, 100, 5, 7);
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, bytes.Length - 4);
				AssertInvalidParameters (logger.Issues, 2);
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (100 + value.IndexOf ("broken")));
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (5));
				Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (7 + value.IndexOf ("broken")));
				Assert.That (logger.Issues[1].StreamOffset, Is.EqualTo (100 + value.IndexOf ("x=")));
				Assert.That (logger.Issues[1].LineNumber, Is.EqualTo (6));
				Assert.That (logger.Issues[1].ColumnNumber, Is.EqualTo (2));
				Assert.That (logger.Issues.All (x => x.Context == MimeComplianceContext.Storage), Is.True);

				logger.Issues.Clear ();
				validator.Validate (valid, 0, valid.Length);
				Assert.That (logger.Issues, Is.Empty);
			}
		}

		[Test]
		public void TestManyMalformedParametersAreBounded ([Values ("broken", "x=")] string parameter)
		{
			const int count = 16384;
			var bytes = Encoding.ASCII.GetBytes ("text/plain" + string.Concat (Enumerable.Repeat (";\r\n " + parameter, count)));
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertInvalidParameters (logger.Issues, count);
			AssertParameters (logger.Issues, Parameter, parameter == "x=" ? count - 1 : 0);
			Assert.That (logger.Issues[logger.Issues.Count - 1].LineNumber, Is.EqualTo (count + 1));
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (count * 256L + 65536));
			}
		}

		[Test]
		public void TestEncodedValidatorReuseAndOffsets ()
		{
			const string value = "text/plain;\r\n name*=utf-8''file%2";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "0yy");
			var valid = Encoding.ASCII.GetBytes ("text/plain; name*0*=utf-8''%C3; name*1*=%A9");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Storage, HeaderId.ContentType, 100, 5, 7);
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, value.Length);
				AssertInvalidEncodedParameters (logger.Issues, 1);
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (100 + value.IndexOf ("name*=")));
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (6));
				Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (2));
				Assert.That (logger.Issues[0].Context, Is.EqualTo (MimeComplianceContext.Storage));

				logger.Issues.Clear ();
				validator.Validate (valid, 0, valid.Length);
				Assert.That (logger.Issues, Is.Empty);
			}
		}

		[Test]
		public void TestManyMalformedEncodedValuesAreBounded ()
		{
			const int count = 16384;
			var bytes = Encoding.ASCII.GetBytes ("text/plain" + string.Concat (Enumerable.Repeat (";\r\n x*=utf-8''%GG", count)));
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertInvalidEncodedParameters (logger.Issues, count);
			AssertParameters (logger.Issues, Parameter, count - 1);
			Assert.That (logger.Issues[logger.Issues.Count - 1].LineNumber, Is.EqualTo (count + 1));
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (count * 256L + 65536));
			}
		}

		[Test]
		public void TestEncodedValueScanIsBounded ([Values (false, true)] bool prefix)
		{
			var bytes = Encoding.ASCII.GetBytes ("text/plain; name*=" + (prefix ? "utf-8''" : "") + new string ('a', 1024 * 1024) + "%2");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertInvalidEncodedParameters (logger.Issues, 1);
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (65536));
			}
		}

		[Test]
		public void TestEncodedWordValidatorReuseAndOffsets ()
		{
			const string value = "text/plain;\r\n name=\"=\\?utf-8?Q?file.txt?=\"";
			var bytes = Encoding.ASCII.GetBytes ("xx" + value + "yy");
			const string incomplete = "text/plain; name==?utf-8?Q?file?";
			var truncated = Encoding.ASCII.GetBytes (incomplete + "=");
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Storage, HeaderId.ContentType, 100, 5, 7);
			for (int i = 0; i < 2; i++) {
				logger.Issues.Clear ();
				validator.Validate (bytes, 2, value.Length);
				AssertEncodedWords (logger.Issues, 1);
				Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (100 + value.IndexOf ("name=")));
				Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (6));
				Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (2));
				Assert.That (logger.Issues[0].Context, Is.EqualTo (MimeComplianceContext.Storage));

				logger.Issues.Clear ();
				validator.Validate (truncated, 0, incomplete.Length);
				Assert.That (logger.Issues, Is.Empty);
				validator.Validate (truncated, 0, truncated.Length);
				AssertEncodedWords (logger.Issues, 1);
			}
		}

		[Test]
		public void TestManyEncodedWordParametersAreBounded ()
		{
			const int count = 16384;
			var bytes = Encoding.ASCII.GetBytes ("text/plain" + string.Concat (Enumerable.Repeat (";\r\n x=\"=?utf-8?Q?file?=\"", count)));
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertEncodedWords (logger.Issues, count);
			AssertParameters (logger.Issues, Parameter, count - 1);
			Assert.That (logger.Issues[logger.Issues.Count - 1].LineNumber, Is.EqualTo (count + 1));
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (count * 256L + 65536));
			}
		}

		[Test]
		public void TestEncodedWordScanIsBounded ([Values ("plain", "=?", "=?utf-8?Q?payload")] string fragment, [Values (false, true)] bool complete)
		{
			var text = new StringBuilder ("text/plain; name=\"");
			while (text.Length < 1024 * 1024)
				text.Append (fragment);
			if (complete)
				text.Append ("=?utf-8?Q?end?=");
			text.Append ('"');
			var bytes = Encoding.ASCII.GetBytes (text.ToString ());
			var logger = new TestMimeComplianceLogger ();
			var validator = new ParameterValidator (logger, MimeComplianceContext.Transport, HeaderId.ContentType, 0, 1, 1);
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			long before = GC.GetAllocatedBytesForCurrentThread ();
			validator.Validate (bytes, 0, bytes.Length);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;
			watch.Stop ();

			AssertEncodedWords (logger.Issues, complete ? 1 : 0);
			if (!TestHelper.IsCodeCoverageEnabled) {
				Assert.That (watch.Elapsed, Is.LessThan (TimeSpan.FromSeconds (5)));
				Assert.That (allocated, Is.LessThan (65536));
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
