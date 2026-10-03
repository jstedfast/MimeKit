//
// TnefNestingTests.cs
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
	public class TnefNestingTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");

		// Builds a TNEF stream containing a single attachment whose PR_ATTACH_DATA_OBJ property is
		// an embedded TNEF message.
		static byte[] BuildEmbeddedMessageBytes (byte[] embedded)
		{
			var value = new byte[16 + embedded.Length];
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			return builder.ToArray ();
		}

		static MemoryStream BuildEmbeddedMessage (byte[] embedded)
		{
			return new MemoryStream (BuildEmbeddedMessageBytes (embedded), false);
		}

		static async Task<bool> ReadAsync (TnefReader reader, bool async)
		{
			return async ? await reader.ReadAsync () : reader.Read ();
		}

		static async Task<bool> ReadNextPropertyAsync (TnefPropertyReader reader, bool async)
		{
			return async ? await reader.ReadNextPropertyAsync () : reader.ReadNextProperty ();
		}

		static async Task<TnefReader> GetEmbeddedMessageReaderAsync (TnefReader reader, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (prop, async)) {
					if (prop.IsEmbeddedMessage)
						return prop.OpenEmbeddedMessage ();
				}
			}

			throw new InvalidOperationException ("Failed to locate the embedded message.");
		}

		static int IndexOf (byte[] buffer, byte[] value)
		{
			for (int i = 0; i <= buffer.Length - value.Length; i++) {
				int j = 0;

				while (j < value.Length && buffer[i + j] == value[j])
					j++;

				if (j == value.Length)
					return i;
			}

			return -1;
		}

		async Task RunDefaultMaxNestingDepthAsync (bool async)
		{
			var inner = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			inner.WriteTnefVersion ();

			using var stream = BuildEmbeddedMessage (inner.ToArray ());
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Options.MaxNestingDepth, Is.EqualTo (TnefOptions.DefaultMaxNestingDepth));

			using var embedded = await GetEmbeddedMessageReaderAsync (reader, async);

			Assert.That (embedded.Options.MaxNestingDepth, Is.EqualTo (TnefOptions.DefaultMaxNestingDepth), "MaxNestingDepth");
			Assert.That (await ReadAsync (embedded, async), Is.True, "Read");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestDefaultMaxNestingDepth ()
		{
			RunDefaultMaxNestingDepthAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestDefaultMaxNestingDepthAsync ()
		{
			await RunDefaultMaxNestingDepthAsync (true);
		}

		async Task RunMaxNestingDepthExceededAsync (bool async)
		{
			var inner = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var options = TnefOptions.Default.Clone ();

			options.MaxNestingDepth = 0;
			inner.WriteTnefVersion ();

			using var stream = BuildEmbeddedMessage (inner.ToArray ());
			using var reader = new TnefReader (stream, options) { ComplianceLogger = logger };
			using var embedded = await GetEmbeddedMessageReaderAsync (reader, async);

			Assert.That (embedded.Depth, Is.EqualTo (reader.Depth + 1), "Depth");
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.NestingTooDeep), "Violation");
			Assert.That (await ReadAsync (embedded, async), Is.False, "Read");
		}

		[Test]
		public void TestMaxNestingDepthExceeded ()
		{
			RunMaxNestingDepthExceededAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMaxNestingDepthExceededAsync ()
		{
			await RunMaxNestingDepthExceededAsync (true);
		}

		async Task RunNestedEmbeddedMessageBeyondMaxNestingDepthReturnsEmptyReaderAsync (bool async)
		{
			var leaf = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var options = TnefOptions.Default.Clone ();

			options.MaxNestingDepth = 1;
			leaf.WriteTnefVersion ();

			var middle = BuildEmbeddedMessageBytes (leaf.ToArray ());
			var outer = BuildEmbeddedMessageBytes (middle);

			using var stream = new MemoryStream (outer, false);
			using var reader = new TnefReader (stream, options) { ComplianceLogger = logger };
			using var child = await GetEmbeddedMessageReaderAsync (reader, async);
			using var grandchild = await GetEmbeddedMessageReaderAsync (child, async);

			Assert.That (child.Depth, Is.EqualTo (1), "child Depth");
			Assert.That (grandchild.Depth, Is.EqualTo (2), "grandchild Depth");
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.NestingTooDeep), "Violation");
			Assert.That (await ReadAsync (grandchild, async), Is.False, "Read");
		}

		[Test]
		public void TestNestedEmbeddedMessageBeyondMaxNestingDepthReturnsEmptyReader ()
		{
			RunNestedEmbeddedMessageBeyondMaxNestingDepthReturnsEmptyReaderAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestNestedEmbeddedMessageBeyondMaxNestingDepthReturnsEmptyReaderAsync ()
		{
			await RunNestedEmbeddedMessageBeyondMaxNestingDepthReturnsEmptyReaderAsync (true);
		}

		async Task RunEmbeddedMessageDepthIncrementsAndStreamOffsetIsAbsoluteAsync (bool async)
		{
			var inner = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			inner.WriteTnefVersion ();
			inner.WriteMessageClass ("IPM.Note");

			var embedded = inner.ToArray ();
			var outer = BuildEmbeddedMessageBytes (embedded);
			int embeddedOffset = IndexOf (outer, embedded);

			Assert.That (embeddedOffset, Is.GreaterThanOrEqualTo (0), "embeddedOffset");

			using var stream = new MemoryStream (outer, false);
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };
			using var child = await GetEmbeddedMessageReaderAsync (reader, async);

			Assert.That (child.Depth, Is.EqualTo (reader.Depth + 1), "Depth");
			Assert.That (await ReadAsync (child, async), Is.True, "Read");
			Assert.That (child.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");
			Assert.That (child.StreamOffset, Is.EqualTo (embeddedOffset + 6), "StreamOffset");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestEmbeddedMessageDepthIncrementsAndStreamOffsetIsAbsolute ()
		{
			RunEmbeddedMessageDepthIncrementsAndStreamOffsetIsAbsoluteAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestEmbeddedMessageDepthIncrementsAndStreamOffsetIsAbsoluteAsync ()
		{
			await RunEmbeddedMessageDepthIncrementsAndStreamOffsetIsAbsoluteAsync (true);
		}

		// Real-world writers emit PR_ATTACH_DATA_OBJ before PR_ATTACH_METHOD, so the reader must identify
		// embedded messages by the object's interface identifier rather than the attach method.
		static byte[] BuildAttachDataBeforeAttachMethodBytes (Guid iid, byte[] data, TnefAttachMethod method)
		{
			var value = new byte[16 + data.Length];
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			iid.ToByteArray ().CopyTo (value, 0);
			data.CopyTo (value, 16);

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) method);

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			return builder.ToArray ();
		}

		static async Task<bool> IsAttachDataEmbeddedMessageAsync (TnefReader reader, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (prop, async)) {
					if (prop.Tag.Id == TnefPropertyId.AttachData)
						return prop.IsEmbeddedMessage;
				}
			}

			throw new InvalidOperationException ("Failed to locate PR_ATTACH_DATA_OBJ.");
		}

		async Task RunEmbeddedMessageDetectedWhenAttachDataPrecedesAttachMethodAsync (bool async)
		{
			var inner = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			inner.WriteTnefVersion ();

			var bytes = BuildAttachDataBeforeAttachMethodBytes (IID_IMessage, inner.ToArray (), TnefAttachMethod.EmbeddedMessage);

			using var stream = new MemoryStream (bytes, false);
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			using var embedded = await GetEmbeddedMessageReaderAsync (reader, async);

			Assert.That (await ReadAsync (embedded, async), Is.True, "Read");
			Assert.That (embedded.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");
			Assert.That (await ReadAsync (embedded, async), Is.False, "End of embedded message");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestEmbeddedMessageDetectedWhenAttachDataPrecedesAttachMethod ()
		{
			RunEmbeddedMessageDetectedWhenAttachDataPrecedesAttachMethodAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestEmbeddedMessageDetectedWhenAttachDataPrecedesAttachMethodAsync ()
		{
			await RunEmbeddedMessageDetectedWhenAttachDataPrecedesAttachMethodAsync (true);
		}

		async Task RunOleObjectNotDetectedAsEmbeddedMessageAsync (bool async)
		{
			var IID_IStorage = new Guid ("0000000b-0000-0000-C000-000000000046");
			var bytes = BuildAttachDataBeforeAttachMethodBytes (IID_IStorage, new byte[32], TnefAttachMethod.Ole);

			using var stream = new MemoryStream (bytes, false);
			using var reader = new TnefReader (stream);

			Assert.That (await IsAttachDataEmbeddedMessageAsync (reader, async), Is.False);
		}

		[Test]
		public void TestOleObjectNotDetectedAsEmbeddedMessage ()
		{
			RunOleObjectNotDetectedAsEmbeddedMessageAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestOleObjectNotDetectedAsEmbeddedMessageAsync ()
		{
			await RunOleObjectNotDetectedAsEmbeddedMessageAsync (true);
		}

		[Test]
		public void TestMaxNestingDepthCannotBeNegative ()
		{
			var options = TnefOptions.Default.Clone ();

			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxNestingDepth = -1);
			Assert.That (options.MaxNestingDepth, Is.EqualTo (TnefOptions.DefaultMaxNestingDepth));
		}
	}
}
