//
// MimeComplianceOptionsTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceOptionsTests
	{
		const string Base64Message =
			"From: sender@example.com\r\n" +
			"To: recipient@example.com\r\n" +
			"Subject: base64\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: application/octet-stream\r\n" +
			"Content-Transfer-Encoding: base64\r\n" +
			"\r\n" +
			"SGVs*bG8=\r\n";

		const string QuotedPrintableMessage =
			"From: sender@example.com\r\n" +
			"To: recipient@example.com\r\n" +
			"Subject: quoted-printable\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: text/plain\r\n" +
			"Content-Transfer-Encoding: quoted-printable\r\n" +
			"\r\n" +
			"Hello =ZZ World\r\n";

		const string UUEncodeMessage =
			"From: sender@example.com\r\n" +
			"To: recipient@example.com\r\n" +
			"Subject: uuencode\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: application/octet-stream\r\n" +
			"Content-Transfer-Encoding: x-uuencode\r\n" +
			"\r\n" +
			"begin 644 file.txt\r\n";

		const string AddressMessage =
			"From: sender@example.com\r\n" +
			"To: <recipient@example.com\r\n" +
			"Subject: address\r\n" +
			"\r\n" +
			"Hello\r\n";

		static IEnumerable<TestCaseData> ValidatorTestCases ()
		{
			yield return new TestCaseData (MimeComplianceValidators.Base64, Base64Message);
			yield return new TestCaseData (MimeComplianceValidators.QuotedPrintable, QuotedPrintableMessage);
			yield return new TestCaseData (MimeComplianceValidators.UUEncode, UUEncodeMessage);
			yield return new TestCaseData (MimeComplianceValidators.Address, AddressMessage);
		}

		static List<MimeComplianceViolation> Read (string text, MimeComplianceOptions options)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream, MimeFormat.Entity) {
					ComplianceLogger = logger,
					ComplianceOptions = options
				};

				reader.ReadMessage ();
			}

			return logger.Issues.Select (issue => issue.Violation).ToList ();
		}

		static async Task<List<MimeComplianceViolation>> ReadAsync (string text, MimeComplianceOptions options)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream, MimeFormat.Entity) {
					ComplianceLogger = logger,
					ComplianceOptions = options
				};

				await reader.ReadMessageAsync ();
			}

			return logger.Issues.Select (issue => issue.Violation).ToList ();
		}

		static MimeComplianceOptions CreateOptions (MimeComplianceValidators validators)
		{
			return new MimeComplianceOptions { EnabledValidators = validators };
		}

		[Test]
		public void TestDefaults ()
		{
			var options = new MimeComplianceOptions ();

			Assert.That (options.Context, Is.EqualTo (MimeComplianceContext.Transport));
			Assert.That (options.EnabledValidators, Is.EqualTo (MimeComplianceValidators.All));
			Assert.That (options.MaxIssuesPerViolation, Is.EqualTo (0));
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var options = new MimeComplianceOptions ();

			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxIssuesPerViolation = -1);
		}

		[Test]
		public void TestClone ()
		{
			var options = new MimeComplianceOptions {
				Context = MimeComplianceContext.Storage,
				EnabledValidators = MimeComplianceValidators.Base64 | MimeComplianceValidators.Address,
				MaxIssuesPerViolation = 7
			};

			var clone = options.Clone ();

			Assert.That (clone, Is.Not.SameAs (options));
			Assert.That (clone.Context, Is.EqualTo (options.Context));
			Assert.That (clone.EnabledValidators, Is.EqualTo (options.EnabledValidators));
			Assert.That (clone.MaxIssuesPerViolation, Is.EqualTo (options.MaxIssuesPerViolation));
		}

		[Test]
		public void TestReaderDoesNotShareTheDefaultOptions ()
		{
			using (var stream = new MemoryStream ()) {
				var reader = new MimeReader (stream);

				Assert.That (reader.ComplianceOptions, Is.Not.Null);
				Assert.That (reader.ComplianceOptions, Is.Not.SameAs (MimeComplianceOptions.Default));

				reader.ComplianceOptions = MimeComplianceOptions.Default;

				Assert.That (reader.ComplianceOptions, Is.Not.SameAs (MimeComplianceOptions.Default));

				var options = new MimeComplianceOptions ();
				reader.ComplianceOptions = options;

				Assert.That (reader.ComplianceOptions, Is.SameAs (options));
			}
		}

		[TestCaseSource (nameof (ValidatorTestCases))]
		public void TestDisableValidator (MimeComplianceValidators validator, string text)
		{
			var none = Read (text, CreateOptions (MimeComplianceValidators.None));
			var all = Read (text, CreateOptions (MimeComplianceValidators.All));
			var only = Read (text, CreateOptions (validator));
			var allButOne = Read (text, CreateOptions (MimeComplianceValidators.All & ~validator));

			Assert.That (all, Has.Count.GreaterThan (none.Count), "The message should trigger the validator.");
			Assert.That (only, Is.EqualTo (all), "Enabling only this validator should report everything that it detects.");
			Assert.That (allButOne, Is.EqualTo (none), "Disabling this validator should suppress everything that it detects.");
		}

		[TestCaseSource (nameof (ValidatorTestCases))]
		public async Task TestDisableValidatorAsync (MimeComplianceValidators validator, string text)
		{
			var none = await ReadAsync (text, CreateOptions (MimeComplianceValidators.None));
			var all = await ReadAsync (text, CreateOptions (MimeComplianceValidators.All));
			var only = await ReadAsync (text, CreateOptions (validator));
			var allButOne = await ReadAsync (text, CreateOptions (MimeComplianceValidators.All & ~validator));

			Assert.That (all, Has.Count.GreaterThan (none.Count), "The message should trigger the validator.");
			Assert.That (only, Is.EqualTo (all), "Enabling only this validator should report everything that it detects.");
			Assert.That (allButOne, Is.EqualTo (none), "Disabling this validator should suppress everything that it detects.");
		}

		[Test]
		public void TestContext ()
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (Base64Message), false)) {
				var reader = new MimeReader (stream, MimeFormat.Entity) {
					ComplianceLogger = logger,
					ComplianceOptions = new MimeComplianceOptions { Context = MimeComplianceContext.Storage }
				};

				reader.ReadMessage ();
			}

			Assert.That (logger.Issues, Is.Not.Empty);
			Assert.That (logger.Issues.All (issue => issue.Context == MimeComplianceContext.Storage), Is.True);
		}

		// Note: The options are captured at the start of each parse operation, so changing them while a
		// message is being parsed must not affect the rest of that parse, but must affect the next one.
		sealed class DisablingComplianceLogger : IMimeComplianceLogger
		{
			public readonly List<MimeComplianceIssue> Issues = new List<MimeComplianceIssue> ();
			public MimeReader Reader;

			public void Log (in MimeComplianceIssue issue)
			{
				Issues.Add (issue);

				Reader.ComplianceOptions.EnabledValidators = MimeComplianceValidators.None;
			}
		}

		static string CreateTwoMessages ()
		{
			const string message =
				"From: sender@example.com\r\n" +
				"To: recipient@example.com\r\n" +
				"Subject: multipart\r\n" +
				"MIME-Version: 1.0\r\n" +
				"Content-Type: multipart/mixed; boundary=\"boundary\"\r\n" +
				"\r\n" +
				"--boundary\r\n" +
				"Content-Type: application/octet-stream\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"\r\n" +
				"SGVs*bG8=\r\n" +
				"--boundary\r\n" +
				"Content-Type: application/octet-stream\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"\r\n" +
				"SGVs*bG8=\r\n" +
				"--boundary--\r\n";

			return "From sender@example.com Mon Jan 1 00:00:00 2024\r\n" + message +
				"From sender@example.com Mon Jan 1 00:00:00 2024\r\n" + message;
		}

		[Test]
		public void TestOptionsAreCapturedPerParse ()
		{
			var logger = new DisablingComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CreateTwoMessages ()), false)) {
				var reader = new MimeReader (stream, MimeFormat.Mbox) {
					ComplianceLogger = logger
				};

				logger.Reader = reader;

				reader.ReadMessage ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.ObsoleteBase64Comment), Is.EqualTo (2), "First message");

				logger.Issues.Clear ();
				reader.ReadMessage ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.ObsoleteBase64Comment), Is.EqualTo (0), "Second message");
			}
		}

		[Test]
		public async Task TestOptionsAreCapturedPerParseAsync ()
		{
			var logger = new DisablingComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CreateTwoMessages ()), false)) {
				var reader = new MimeReader (stream, MimeFormat.Mbox) {
					ComplianceLogger = logger
				};

				logger.Reader = reader;

				await reader.ReadMessageAsync ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.ObsoleteBase64Comment), Is.EqualTo (2), "First message");

				logger.Issues.Clear ();
				await reader.ReadMessageAsync ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.ObsoleteBase64Comment), Is.EqualTo (0), "Second message");
			}
		}

		[Test]
		public void TestChangingTheLimitBetweenParses ()
		{
			var logger = new TestMimeComplianceLogger ();
			var builder = new StringBuilder ();

			for (int n = 0; n < 2; n++) {
				builder.Append ("From sender@example.com Mon Jan 1 00:00:00 2024\r\n");
				builder.Append ("From: sender@example.com\r\n");
				builder.Append ("Subject: flood\r\n");
				builder.Append ("\r\n");

				for (int i = 0; i < 10; i++)
					builder.Append ("line\n");
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (builder.ToString ()), false)) {
				var reader = new MimeReader (stream, MimeFormat.Mbox) {
					ComplianceLogger = logger,
					ComplianceOptions = new MimeComplianceOptions { MaxIssuesPerViolation = 3 }
				};

				reader.ReadMessage ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (3), "First message");
				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1), "First message");

				logger.Issues.Clear ();
				reader.ComplianceOptions.MaxIssuesPerViolation = 5;
				reader.ReadMessage ();

				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.BareLinefeedInBody), Is.EqualTo (5), "Second message");
				Assert.That (logger.Issues.Count (issue => issue.Violation == MimeComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1), "Second message");
			}
		}
	}
}
