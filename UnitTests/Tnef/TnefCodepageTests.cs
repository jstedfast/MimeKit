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

		#region [MS-OXTNEF] 2.3.3.2 codepage selection

		const string Cyrillic = "\u041f\u0440\u0438\u0432\u0435\u0442";

		static async Task<TnefMessage> LoadAsync (TnefBuilder builder, bool async, TnefOptions options = null, TestTnefComplianceLogger logger = null)
		{
			using (var reader = new TnefReader (builder.ToStream (), options) { ComplianceLogger = logger })
				return async ? await TnefMessage.LoadAsync (reader) : TnefMessage.Load (reader);
		}

		static TnefBuilder CreateMessage (int? oemCodepage, int internetCodepage, bool bodyFirst = false)
		{
			var cp1251 = System.Text.Encoding.GetEncoding (1251);
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			if (oemCodepage.HasValue)
				builder.WriteOemCodepage (oemCodepage.Value);
			builder.WriteMessageClass ("IPM.Note");

			if (bodyFirst) {
				properties.WriteStringProperty (TnefPropertyTag.BodyA, Cyrillic, cp1251);
				properties.WriteInt32Property (TnefPropertyTag.InternetCodepage, internetCodepage);
				properties.WriteStringProperty (TnefPropertyTag.SubjectA, Cyrillic, cp1251);
			} else {
				properties.WriteInt32Property (TnefPropertyTag.InternetCodepage, internetCodepage);
				properties.WriteStringProperty (TnefPropertyTag.SubjectA, Cyrillic, cp1251);
				properties.WriteStringProperty (TnefPropertyTag.BodyA, Cyrillic, cp1251);
			}

			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder;
		}

		async Task RunTestInternetCodepageIsUsedWithoutOemCodepageAsync (bool async)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var message = await LoadAsync (CreateMessage (null, 1251), async, null, logger)) {
				Assert.That (logger.Issues, Is.Empty, "Issues");
				Assert.That (message.Codepage, Is.EqualTo (1251), "Codepage");
				Assert.That (message.Subject, Is.EqualTo (Cyrillic), "Subject");
				Assert.That (message.TextBody.Encoding.CodePage, Is.EqualTo (1251), "Encoding");
				Assert.That (message.TextBody.GetText (), Is.EqualTo (Cyrillic), "Body");
			}
		}

		[Test]
		public Task TestInternetCodepageIsUsedWithoutOemCodepage () => RunTestInternetCodepageIsUsedWithoutOemCodepageAsync (false);

		[Test]
		public Task TestInternetCodepageIsUsedWithoutOemCodepageAsync () => RunTestInternetCodepageIsUsedWithoutOemCodepageAsync (true);

		async Task RunTestInternetCodepageAppliesToPrecedingBodyAsync (bool async)
		{
			using (var message = await LoadAsync (CreateMessage (null, 1251, bodyFirst: true), async)) {
				Assert.That (message.Codepage, Is.EqualTo (1251), "Codepage");
				Assert.That (message.TextBody.Encoding.CodePage, Is.EqualTo (1251), "Encoding");
				Assert.That (message.TextBody.GetText (), Is.EqualTo (Cyrillic), "Body");

				using (var result = message.ConvertToMime ())
					Assert.That (result.Message.TextBody, Is.EqualTo (Cyrillic), "Converted body");
			}
		}

		[Test]
		public Task TestInternetCodepageAppliesToPrecedingBody () => RunTestInternetCodepageAppliesToPrecedingBodyAsync (false);

		[Test]
		public Task TestInternetCodepageAppliesToPrecedingBodyAsync () => RunTestInternetCodepageAppliesToPrecedingBodyAsync (true);

		async Task RunTestOemCodepageTakesPrecedenceOverInternetCodepageAsync (bool async)
		{
			using (var message = await LoadAsync (CreateMessage (1252, 1251), async)) {
				Assert.That (message.Codepage, Is.EqualTo (1252), "Codepage");
				Assert.That (message.TextBody.Encoding.CodePage, Is.EqualTo (1252), "Encoding");
				Assert.That (message.Subject, Is.Not.EqualTo (Cyrillic), "Subject");
			}
		}

		[Test]
		public Task TestOemCodepageTakesPrecedenceOverInternetCodepage () => RunTestOemCodepageTakesPrecedenceOverInternetCodepageAsync (false);

		[Test]
		public Task TestOemCodepageTakesPrecedenceOverInternetCodepageAsync () => RunTestOemCodepageTakesPrecedenceOverInternetCodepageAsync (true);

		async Task RunTestZeroOemCodepageIsIgnoredAsync (bool async)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var message = await LoadAsync (CreateMessage (0, 1251), async, null, logger)) {
				Assert.That (logger.Issues, Is.Empty, "Issues");
				Assert.That (message.Codepage, Is.EqualTo (1251), "Codepage");
				Assert.That (message.Subject, Is.EqualTo (Cyrillic), "Subject");
			}
		}

		[Test]
		public Task TestZeroOemCodepageIsIgnored () => RunTestZeroOemCodepageIsIgnoredAsync (false);

		[Test]
		public Task TestZeroOemCodepageIsIgnoredAsync () => RunTestZeroOemCodepageIsIgnoredAsync (true);

		async Task RunTestZeroOemCodepageKeepsDefaultCodepageAsync (bool async)
		{
			var options = new TnefOptions { DefaultCodepage = 1250 };
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (0);
			builder.WriteMessageClass ("IPM.Note");

			using (var message = await LoadAsync (builder, async, options, logger)) {
				Assert.That (logger.Issues, Is.Empty, "Issues");
				Assert.That (message.Codepage, Is.EqualTo (1250), "Codepage");
			}
		}

		[Test]
		public Task TestZeroOemCodepageKeepsDefaultCodepage () => RunTestZeroOemCodepageKeepsDefaultCodepageAsync (false);

		[Test]
		public Task TestZeroOemCodepageKeepsDefaultCodepageAsync () => RunTestZeroOemCodepageKeepsDefaultCodepageAsync (true);

		async Task RunTestUnsupportedOemCodepageKeepsDefaultCodepageAsync (bool async)
		{
			var options = new TnefOptions { DefaultCodepage = 1250 };
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);
			builder.WriteMessageClass ("IPM.Note");

			using (var message = await LoadAsync (builder, async, options, logger)) {
				Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidMessageCodepage), "Violation");
				Assert.That (message.Codepage, Is.EqualTo (1250), "Codepage");
			}
		}

		[Test]
		public Task TestUnsupportedOemCodepageKeepsDefaultCodepage () => RunTestUnsupportedOemCodepageKeepsDefaultCodepageAsync (false);

		[Test]
		public Task TestUnsupportedOemCodepageKeepsDefaultCodepageAsync () => RunTestUnsupportedOemCodepageKeepsDefaultCodepageAsync (true);

		[TestCase (1200)]
		[TestCase (1201)]
		[TestCase (12000)]
		[TestCase (12001)]
		[TestCase (UnsupportedCodepage)]
		public async Task TestUnusableInternetCodepageIsIgnored (int internetCodepage)
		{
			foreach (var async in new[] { false, true }) {
				using (var message = await LoadAsync (CreateMessage (null, internetCodepage), async)) {
					Assert.That (message.Codepage, Is.EqualTo (1252), "Codepage");
					Assert.That (message.TextBody.Encoding.CodePage, Is.EqualTo (1252), "Encoding");
				}
			}
		}

		async Task RunTestAttachmentInternetCodepageIsIgnoredAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			properties.WriteInt32Property (TnefPropertyTag.InternetCodepage, 1251);
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Attachments, Has.Count.EqualTo (1), "Attachments");
				Assert.That (message.Codepage, Is.EqualTo (1252), "Codepage");
			}
		}

		[Test]
		public Task TestAttachmentInternetCodepageIsIgnored () => RunTestAttachmentInternetCodepageIsIgnoredAsync (false);

		[Test]
		public Task TestAttachmentInternetCodepageIsIgnoredAsync () => RunTestAttachmentInternetCodepageIsIgnoredAsync (true);

		#endregion
	}
}