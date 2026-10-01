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
		static TnefComplianceStatus GetComplianceStatus (TnefBuilder builder)
		{
			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ())
						;

					return reader.ComplianceStatus;
				}
			}
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

			Assert.That (GetComplianceStatus (builder), Is.EqualTo (TnefComplianceStatus.Compliant));
		}

		[Test]
		public void TestMessageLevelAttributeAfterAttachmentLevelAttribute ()
		{
			// Once the attachment-level attributes have begun, it is no longer legal to
			// go back to the message level.
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			var status = GetComplianceStatus (builder);

			Assert.That (status & TnefComplianceStatus.InvalidAttributeLevel, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLevel));
		}

		[TestCase (TnefAttributeTag.AttachRenderData)]
		[TestCase (TnefAttributeTag.AttachData)]
		[TestCase (TnefAttributeTag.AttachTitle)]
		[TestCase (TnefAttributeTag.Attachment)]
		public void TestAttachmentAttributeAtMessageLevel (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, tag, new byte[] { 0, 0, 0, 0 });

			var status = GetComplianceStatus (builder);

			Assert.That (status & TnefComplianceStatus.InvalidAttributeLevel, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLevel));
		}

		[TestCase (TnefAttributeTag.Subject)]
		[TestCase (TnefAttributeTag.MessageClass)]
		[TestCase (TnefAttributeTag.MapiProperties)]
		[TestCase (TnefAttributeTag.RecipientTable)]
		public void TestMessageAttributeAtAttachmentLevel (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, tag, new byte[] { 0, 0, 0, 0 });

			var status = GetComplianceStatus (builder);

			Assert.That (status & TnefComplianceStatus.InvalidAttributeLevel, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLevel));
		}

		[Test]
		public void TestInvalidAttributeLevelThrowsInStrictMode ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("This is the subject\0"));

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					var ex = Assert.Throws<TnefException> (() => reader.ReadNextAttribute ());

					Assert.That (ex!.Error, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLevel));
				}
			}
		}
	}
}
