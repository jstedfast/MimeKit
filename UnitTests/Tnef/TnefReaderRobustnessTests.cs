//
// TnefReaderRobustnessTests.cs
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
	public class TnefReaderRobustnessTests
	{
		static void AssertLogged (TestTnefComplianceLogger logger, TnefComplianceViolation violation)
		{
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (violation), "Violation");
		}

		static async Task DrainAsync (TnefReader reader, bool async)
		{
			if (async) {
				while (await reader.ReadAsync ())
					;
			} else {
				while (reader.Read ())
					;
			}
		}

		static async Task<bool> ReadAsync (TnefReader reader, bool async)
		{
			return async ? await reader.ReadAsync () : reader.Read ();
		}

		static void BuildShortPeekedAttribute (TnefBuilder builder, TnefAttributeTag tag)
		{
			builder.WriteAttribute (TnefAttributeLevel.Message, tag, TnefBuilder.Int32Payload (1252));
		}

		async Task RunAttributeLengthOverflowDoesNotThrowAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 }, int.MaxValue);

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await ReadAsync (reader, async), Is.True, "Read");

			var buffer = new byte[1024];

			if (async) {
				using var value = reader.OpenValueStream ();

				while (await value.ReadAsync (buffer, 0, buffer.Length) > 0)
					;
			} else {
				using var value = reader.OpenValueStream ();

				while (value.Read (buffer, 0, buffer.Length) > 0)
					;
			}

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.TruncatedStream);
		}

		[Test]
		public void TestAttributeLengthOverflowDoesNotThrow ()
		{
			RunAttributeLengthOverflowDoesNotThrowAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestAttributeLengthOverflowDoesNotThrowAsync ()
		{
			await RunAttributeLengthOverflowDoesNotThrowAsync (true);
		}

		async Task RunInvalidSignatureBehavesAsEndOfStreamAsync (bool async)
		{
			var builder = new TnefBuilder (signature: 0x12345678);
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM.Note");

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await ReadAsync (reader, async), Is.False, "Read");
			Assert.That (await ReadAsync (reader, async), Is.False, "Read (again)");
			AssertLogged (logger, TnefComplianceViolation.InvalidSignature);
		}

		[Test]
		public void TestInvalidSignatureBehavesAsEndOfStream ()
		{
			RunInvalidSignatureBehavesAsEndOfStreamAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestInvalidSignatureBehavesAsEndOfStreamAsync ()
		{
			await RunInvalidSignatureBehavesAsEndOfStreamAsync (true);
		}

		async Task RunInvalidSignatureLogsIssueAndDoesNotThrowAsync (bool async)
		{
			var builder = new TnefBuilder (signature: 0x12345678);
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await ReadAsync (reader, async), Is.False, "Read");
			AssertLogged (logger, TnefComplianceViolation.InvalidSignature);
		}

		[Test]
		public void TestInvalidSignatureLogsIssueAndDoesNotThrow ()
		{
			RunInvalidSignatureLogsIssueAndDoesNotThrowAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestInvalidSignatureLogsIssueAndDoesNotThrowAsync ()
		{
			await RunInvalidSignatureLogsIssueAndDoesNotThrowAsync (true);
		}

		async Task RunTruncatedPeekedAttributeDoesNotThrowAsync (TnefAttributeTag tag, bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			BuildShortPeekedAttribute (builder, tag);

			// 6 bytes of header + 1 byte level + 4 byte tag + 4 byte length = 15 bytes.
			using var stream = builder.ToStream (15);
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.TruncatedStream);
		}

		[TestCase (TnefAttributeTag.OemCodepage)]
		[TestCase (TnefAttributeTag.TnefVersion)]
		public void TestTruncatedPeekedAttributeDoesNotThrow (TnefAttributeTag tag)
		{
			RunTruncatedPeekedAttributeDoesNotThrowAsync (tag, false).GetAwaiter ().GetResult ();
		}

		[TestCase (TnefAttributeTag.OemCodepage)]
		[TestCase (TnefAttributeTag.TnefVersion)]
		public async Task TestTruncatedPeekedAttributeDoesNotThrowAsync (TnefAttributeTag tag)
		{
			await RunTruncatedPeekedAttributeDoesNotThrowAsync (tag, true);
		}

		async Task RunShortOemCodepageAttributeDoesNotReadPastItsValueAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage, new byte[] { 0xE4, 0x04 });
			builder.WriteMessageClass ("IPM.Note");

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.InvalidAttributeValue);
		}

		[Test]
		public void TestShortOemCodepageAttributeDoesNotReadPastItsValue ()
		{
			RunShortOemCodepageAttributeDoesNotReadPastItsValueAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestShortOemCodepageAttributeDoesNotReadPastItsValueAsync ()
		{
			await RunShortOemCodepageAttributeDoesNotReadPastItsValueAsync (true);
		}

		async Task RunShortTnefVersionAttributeDoesNotReadPastItsValueAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();
			int version = -1;

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, new byte[] { 0x00, 0x00 });
			builder.WriteMessageClass ("IPM.Note");

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (await ReadAsync (reader, async)) {
				if (reader.Tag == TnefAttributeTag.TnefVersion)
					version = async ? await reader.ReadValueAsInt32Async () : reader.ReadValueAsInt32 ();
			}

			AssertLogged (logger, TnefComplianceViolation.InvalidAttributeValue);
			Assert.That (version, Is.EqualTo (0), "TnefVersion");
		}

		[Test]
		public void TestShortTnefVersionAttributeDoesNotReadPastItsValue ()
		{
			RunShortTnefVersionAttributeDoesNotReadPastItsValueAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestShortTnefVersionAttributeDoesNotReadPastItsValueAsync ()
		{
			await RunShortTnefVersionAttributeDoesNotReadPastItsValueAsync (true);
		}

		async Task RunValidMessageClassIsCompliantAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Schedule.Meeting.Request");

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestValidMessageClassIsCompliant ()
		{
			RunValidMessageClassIsCompliantAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestValidMessageClassIsCompliantAsync ()
		{
			await RunValidMessageClassIsCompliantAsync (true);
		}

		async Task RunEmptyMessageClassIsReportedAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, Array.Empty<byte> ());

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.InvalidMessageClass);
		}

		[Test]
		public void TestEmptyMessageClassIsReported ()
		{
			RunEmptyMessageClassIsReportedAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestEmptyMessageClassIsReportedAsync ()
		{
			await RunEmptyMessageClassIsReportedAsync (true);
		}

		async Task RunNonAsciiMessageClassIsReportedAsync (TnefAttributeTag tag, bool async)
		{
			// PidTagMessageClass is a dot-delimited ASCII string; binary garbage is not one.
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, tag, new byte[] { 0x49, 0x50, 0x4d, 0x01, 0xff, 0x00 });

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.InvalidMessageClass);
		}

		[TestCase (TnefAttributeTag.MessageClass)]
		[TestCase (TnefAttributeTag.OriginalMessageClass)]
		public void TestNonAsciiMessageClassIsReported (TnefAttributeTag tag)
		{
			RunNonAsciiMessageClassIsReportedAsync (tag, false).GetAwaiter ().GetResult ();
		}

		[TestCase (TnefAttributeTag.MessageClass)]
		[TestCase (TnefAttributeTag.OriginalMessageClass)]
		public async Task TestNonAsciiMessageClassIsReportedAsync (TnefAttributeTag tag)
		{
			await RunNonAsciiMessageClassIsReportedAsync (tag, true);
		}

		async Task RunMaximumLengthMessageClassIsCompliantAsync (bool async)
		{
			// [MS-OXCMSG] limits PidTagMessageClass to 255 characters.
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM." + new string ('A', 250));

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestMaximumLengthMessageClassIsCompliant ()
		{
			RunMaximumLengthMessageClassIsCompliantAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMaximumLengthMessageClassIsCompliantAsync ()
		{
			await RunMaximumLengthMessageClassIsCompliantAsync (true);
		}

		async Task RunOversizedMessageClassIsReportedAsync (bool async)
		{
			// A message class that is too large to peek at is itself a violation - PidTagMessageClass
			// is a short, dot-delimited ASCII string.
			var payload = new byte[4096];

			for (int i = 0; i < payload.Length; i++)
				payload[i] = (byte) 'A';

			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, payload);

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.InvalidMessageClass);
		}

		[Test]
		public void TestOversizedMessageClassIsReported ()
		{
			RunOversizedMessageClassIsReportedAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestOversizedMessageClassIsReportedAsync ()
		{
			await RunOversizedMessageClassIsReportedAsync (true);
		}

		async Task RunInvalidMessageClassLogsIssueAndDoesNotThrowAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, new byte[] { 0x00, 0x00, 0x00, 0x00 });

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			await DrainAsync (reader, async);

			AssertLogged (logger, TnefComplianceViolation.InvalidMessageClass);
		}

		[Test]
		public void TestInvalidMessageClassLogsIssueAndDoesNotThrow ()
		{
			RunInvalidMessageClassLogsIssueAndDoesNotThrowAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestInvalidMessageClassLogsIssueAndDoesNotThrowAsync ()
		{
			await RunInvalidMessageClassLogsIssueAndDoesNotThrowAsync (true);
		}

		async Task RunMessageClassValueIsStillReadableAsync (bool async)
		{
			// Validating the message class must not consume any of the attribute's raw value.
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();
			string messageClass = string.Empty;

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM.Note");

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.MessageClass)
					continue;

				var buffer = async ? await reader.ReadValueAsBytesAsync () : reader.ReadValueAsBytes ();

				messageClass = Encoding.ASCII.GetString (buffer, 0, buffer.Length - 1);
			}

			Assert.That (messageClass, Is.EqualTo ("IPM.Note"), "MessageClass");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestMessageClassValueIsStillReadable ()
		{
			RunMessageClassValueIsStillReadableAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMessageClassValueIsStillReadableAsync ()
		{
			await RunMessageClassValueIsStillReadableAsync (true);
		}
	}
}
