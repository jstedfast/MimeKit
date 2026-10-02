//
// TnefCodepageTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefCodepageTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		// A codepage that is not supported even when the CodePagesEncodingProvider has been registered.
		const int UnsupportedCodepage = 65123;

		[Test]
		public void TestUnsupportedDefaultMessageCodepage ()
		{
			// Constructing a TnefReader with a codepage that the host cannot provide must degrade
			// gracefully rather than throwing. On Linux, *most* Windows codepages are unavailable
			// unless the consuming application has registered the CodePagesEncodingProvider.
			var options = new TnefOptions { DefaultCodepage = UnsupportedCodepage };

			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				using (var reader = new TnefReader (stream, options)) {
					Assert.That (reader.Codepage, Is.Not.EqualTo (UnsupportedCodepage), "Codepage");

					while (reader.Read ())
						;
				}
			}
		}

		[Test]
		public async Task TestUnsupportedDefaultMessageCodepageAsync ()
		{
			var options = new TnefOptions { DefaultCodepage = UnsupportedCodepage };

			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				using (var reader = new TnefReader (stream, options)) {
					Assert.That (reader.Codepage, Is.Not.EqualTo (UnsupportedCodepage), "Codepage");

					while (await reader.ReadAsync ())
						;
				}
			}
		}

		[Test]
		public void TestUnsupportedOemCodepageIsRecoverable ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (reader.Read ())
						;

					Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidMessageCodepage), "Violation");
					Assert.That (reader.Codepage, Is.Not.EqualTo (UnsupportedCodepage), "Codepage");
				}
			}
		}

		[Test]
		public async Task TestUnsupportedOemCodepageIsRecoverableAsync ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (await reader.ReadAsync ())
						;

					Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidMessageCodepage), "Violation");
					Assert.That (reader.Codepage, Is.Not.EqualTo (UnsupportedCodepage), "Codepage");
				}
			}
		}

		[Test]
		public void TestUnsupportedOemCodepageDoesNotThrow ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					Assert.DoesNotThrow (() => {
						while (reader.Read ())
							;
					});

					Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidMessageCodepage), "Violation");
				}
			}
		}

		[Test]
		public async Task TestUnsupportedOemCodepageDoesNotThrowAsync ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (await reader.ReadAsync ())
						;

					Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidMessageCodepage), "Violation");
				}
			}
		}

		[Test]
		public void TestMessageCodepageIsAlwaysSupported ()
		{
			// Whatever Codepage ends up reporting, Encoding.GetEncoding() must be able to
			// resolve it; TnefPart relies on this when decoding PidTagBody and PidTagHtml.
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream)) {
					while (reader.Read ())
						;

					Assert.DoesNotThrow (() => System.Text.Encoding.GetEncoding (reader.Codepage));
				}
			}
		}

		[Test]
		public async Task TestMessageCodepageIsAlwaysSupportedAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream)) {
					while (await reader.ReadAsync ())
						;

					Assert.DoesNotThrow (() => System.Text.Encoding.GetEncoding (reader.Codepage));
				}
			}
		}
	}
}