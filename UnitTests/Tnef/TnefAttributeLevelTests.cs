//
// TnefAttributeLevelTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefAttributeLevelTests
	{
		static List<TnefComplianceIssue> ReadComplianceIssues (TnefBuilder builder)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (reader.Read ())
						;
				}
			}

			return logger.Issues;
		}

		static async Task<List<TnefComplianceIssue>> ReadComplianceIssuesAsync (TnefBuilder builder)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (await reader.ReadAsync ())
						;
				}
			}

			return logger.Issues;
		}

		[Test]
		public void TestValidAttributeLevelsAreCompliant ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, Encoding.ASCII.GetBytes ("file.txt\0"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 });

			Assert.That (ReadComplianceIssues (builder), Is.Empty);
		}

		[Test]
		public async Task TestValidAttributeLevelsAreCompliantAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, Encoding.ASCII.GetBytes ("file.txt\0"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 });

			Assert.That (await ReadComplianceIssuesAsync (builder), Is.Empty);
		}

		[Test]
		public void TestMessageLevelAttributeAfterAttachmentLevelAttribute ()
		{
			// Once the attachment-level attributes have begun, it is no longer legal to
			// go back to the message level.
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			Assert.That (ReadComplianceIssues (builder).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.MessageAttributeAfterAttachment));
		}

		[Test]
		public async Task TestMessageLevelAttributeAfterAttachmentLevelAttributeAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			Assert.That ((await ReadComplianceIssuesAsync (builder)).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.MessageAttributeAfterAttachment));
		}

		[TestCase (TnefAttributeTag.AttachRenderData)]
		[TestCase (TnefAttributeTag.AttachData)]
		[TestCase (TnefAttributeTag.AttachTitle)]
		[TestCase (TnefAttributeTag.Attachment)]
		public void TestAttachmentAttributeAtMessageLevel (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, tag, new byte[] { 0, 0, 0, 0 });

			Assert.That (ReadComplianceIssues (builder).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.AttributeLevelMismatch));
		}

		[TestCase (TnefAttributeTag.AttachRenderData)]
		[TestCase (TnefAttributeTag.AttachData)]
		[TestCase (TnefAttributeTag.AttachTitle)]
		[TestCase (TnefAttributeTag.Attachment)]
		public async Task TestAttachmentAttributeAtMessageLevelAsync (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, tag, new byte[] { 0, 0, 0, 0 });

			Assert.That ((await ReadComplianceIssuesAsync (builder)).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.AttributeLevelMismatch));
		}

		[TestCase (TnefAttributeTag.Subject)]
		[TestCase (TnefAttributeTag.MessageClass)]
		[TestCase (TnefAttributeTag.MapiProperties)]
		[TestCase (TnefAttributeTag.RecipientTable)]
		public void TestMessageAttributeAtAttachmentLevel (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, tag, new byte[] { 0, 0, 0, 0 });

			Assert.That (ReadComplianceIssues (builder).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.AttributeLevelMismatch));
		}

		[TestCase (TnefAttributeTag.Subject)]
		[TestCase (TnefAttributeTag.MessageClass)]
		[TestCase (TnefAttributeTag.MapiProperties)]
		[TestCase (TnefAttributeTag.RecipientTable)]
		public async Task TestMessageAttributeAtAttachmentLevelAsync (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, tag, new byte[] { 0, 0, 0, 0 });

			Assert.That ((await ReadComplianceIssuesAsync (builder)).Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.AttributeLevelMismatch));
		}

		[Test]
		public void TestInvalidAttributeLevelIsLoggedAndDoesNotThrow ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteAttribute ((TnefAttributeLevel) 0x7f, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					Assert.DoesNotThrow (() => {
						while (reader.Read ())
							;
					});
				}
			}

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidAttributeLevel));
		}

		[Test]
		public async Task TestInvalidAttributeLevelIsLoggedAndDoesNotThrowAsync ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteAttribute ((TnefAttributeLevel) 0x7f, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger }) {
					while (await reader.ReadAsync ())
						;
				}
			}

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidAttributeLevel));
		}
	}
}