//
// TnefReaderStreamTests.cs
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
	public class TnefReaderStreamTests
	{
		static byte[] CreatePayload (int length)
		{
			var payload = new byte[length];

			for (int i = 0; i < length; i++)
				payload[i] = (byte) (i & 0xFF);

			return payload;
		}

		static byte[] BuildPropertyStream (Action<TnefMapiPropertyBuilder> build)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			build (properties);

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder.ToArray ();
		}

		static byte[] BuildBinaryPropertyStream (byte[] payload)
		{
			return BuildPropertyStream (properties => properties.WriteBinaryProperty (TnefPropertyTag.RecordKey, payload));
		}

		static byte[] BuildAttributeStream (byte[] payload)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, payload);

			return builder.ToArray ();
		}

		static async Task<bool> ReadAsync (TnefReader reader, bool async)
		{
			return async ? await reader.ReadAsync () : reader.Read ();
		}

		static async Task<bool> ReadNextPropertyAsync (TnefPropertyReader reader, bool async)
		{
			return async ? await reader.ReadNextPropertyAsync () : reader.ReadNextProperty ();
		}

		static Task<int> ReadValueStreamAsync (Stream stream, byte[] buffer, int offset, int count, bool async)
		{
			return async ? stream.ReadAsync (buffer, offset, count) : Task.FromResult (stream.Read (buffer, offset, count));
		}

		static async Task<Stream> OpenValueStreamAsync (TnefReader reader, TnefPropertyId id, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var properties = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (properties, async)) {
					if (properties.Tag.Id == id)
						return properties.OpenValueStream ();
				}
			}

			Assert.Fail ($"Did not find a {id} property.");

			return Stream.Null;
		}

		static async Task<Stream> OpenAttributeValueStreamAsync (TnefReader reader, TnefAttributeTag tag, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag == tag)
					return reader.OpenValueStream ();
			}

			Assert.Fail ($"Did not find a {tag} attribute.");

			return Stream.Null;
		}

		async Task RunTnefReaderStreamAsync (bool async)
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);

			var buffer = new byte[64];

			Assert.That (value.CanRead, Is.True, "CanRead");
			Assert.That (value.CanWrite, Is.False, "CanWrite");
			Assert.That (value.CanSeek, Is.False, "CanSeek");
			Assert.That (value.CanTimeout, Is.False, "CanTimeout");
			Assert.Throws<ArgumentNullException> (() => value.Read (null!, 0, buffer.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, -1, buffer.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 0, -1));
			Assert.Throws<NotSupportedException> (() => value.Write (buffer, 0, buffer.Length));
			Assert.Throws<NotSupportedException> (() => value.Seek (0, SeekOrigin.End));
			Assert.Throws<NotSupportedException> (() => value.SetLength (1024));
			Assert.Throws<NotSupportedException> (() => { var unused = value.Position; });
			Assert.Throws<NotSupportedException> (() => { value.Position = 0; });
			Assert.Throws<NotSupportedException> (() => { var unused = value.Length; });
			Assert.DoesNotThrow (() => value.Flush ());
		}

		[Test]
		public void TestTnefReaderStream ()
		{
			RunTnefReaderStreamAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestTnefReaderStreamAsync ()
		{
			await RunTnefReaderStreamAsync (true);
		}

		async Task RunReadDoesNotReturnMoreThanTheValueAsync (int length, bool async)
		{
			var payload = CreatePayload (length);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);

			var buffer = new byte[8192];
			int nread = await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async);

			Assert.That (nread, Is.EqualTo (length), "nread");
			Assert.That (buffer.AsSpan (0, length).ToArray (), Is.EqualTo (payload), "payload");
			Assert.That (await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async), Is.EqualTo (0), "read at end of stream");
		}

		[TestCase (0)]
		[TestCase (1)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (13)]
		[TestCase (1000)]
		public void TestReadDoesNotReturnMoreThanTheValue (int length)
		{
			RunReadDoesNotReturnMoreThanTheValueAsync (length, false).GetAwaiter ().GetResult ();
		}

		[TestCase (0)]
		[TestCase (1)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (13)]
		[TestCase (1000)]
		public async Task TestReadDoesNotReturnMoreThanTheValueAsync (int length)
		{
			await RunReadDoesNotReturnMoreThanTheValueAsync (length, true);
		}

		async Task RunPartialReadsAsync (int chunkSize, bool async)
		{
			var payload = CreatePayload (1000);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);
			using var actual = new MemoryStream ();

			var buffer = new byte[chunkSize];
			int nread, total = 0;

			while ((nread = await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async)) > 0) {
				Assert.That (nread, Is.LessThanOrEqualTo (chunkSize), "nread");
				actual.Write (buffer, 0, nread);
				total += nread;
				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read past the end of the value");
			}

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (7)]
		[TestCase (64)]
		[TestCase (999)]
		public void TestPartialReads (int chunkSize)
		{
			RunPartialReadsAsync (chunkSize, false).GetAwaiter ().GetResult ();
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (7)]
		[TestCase (64)]
		[TestCase (999)]
		public async Task TestPartialReadsAsync (int chunkSize)
		{
			await RunPartialReadsAsync (chunkSize, true);
		}

		async Task RunReadIntoTheMiddleOfABufferAsync (bool async)
		{
			var payload = CreatePayload (64);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);

			var buffer = Enumerable.Repeat ((byte) 0xAA, 256).ToArray ();
			int nread = await ReadValueStreamAsync (value, buffer, 100, 64, async);

			Assert.That (nread, Is.EqualTo (64), "nread");
			Assert.That (buffer.AsSpan (100, 64).ToArray (), Is.EqualTo (payload), "payload");

			for (int i = 0; i < 100; i++)
				Assert.That (buffer[i], Is.EqualTo (0xAA), $"buffer[{i}] was modified");

			for (int i = 164; i < buffer.Length; i++)
				Assert.That (buffer[i], Is.EqualTo (0xAA), $"buffer[{i}] was modified");
		}

		[Test]
		public void TestReadIntoTheMiddleOfABuffer ()
		{
			RunReadIntoTheMiddleOfABufferAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestReadIntoTheMiddleOfABufferAsync ()
		{
			await RunReadIntoTheMiddleOfABufferAsync (true);
		}

		async Task RunReadWithZeroCountDoesNotAdvanceAsync (bool async)
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);

			var buffer = new byte[64];

			Assert.That (await ReadValueStreamAsync (value, buffer, 0, 0, async), Is.EqualTo (0), "zero-length read");
			Assert.That (await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async), Is.EqualTo (32), "nread");
			Assert.That (buffer.AsSpan (0, 32).ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestReadWithZeroCountDoesNotAdvance ()
		{
			RunReadWithZeroCountDoesNotAdvanceAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestReadWithZeroCountDoesNotAdvanceAsync ()
		{
			await RunReadWithZeroCountDoesNotAdvanceAsync (true);
		}

		async Task RunRepeatedReadsAtEndOfStreamReturnZeroAsync (bool async)
		{
			var payload = CreatePayload (16);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);

			var buffer = new byte[64];

			Assert.That (await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async), Is.EqualTo (16), "nread");

			long offset = reader.StreamOffset;

			for (int i = 0; i < 10; i++)
				Assert.That (await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async), Is.EqualTo (0), $"read {i}");

			Assert.That (reader.StreamOffset, Is.EqualTo (offset), "reading at end of stream advanced the reader");
		}

		[Test]
		public void TestRepeatedReadsAtEndOfStreamReturnZero ()
		{
			RunRepeatedReadsAtEndOfStreamReturnZeroAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestRepeatedReadsAtEndOfStreamReturnZeroAsync ()
		{
			await RunRepeatedReadsAtEndOfStreamReturnZeroAsync (true);
		}

		async Task RunCopyToReadsExactlyTheValueAsync (bool async)
		{
			var payload = CreatePayload (5000);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);
			using var actual = new MemoryStream ();

			if (async)
				await value.CopyToAsync (actual, 137);
			else
				value.CopyTo (actual, 137);

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestCopyToReadsExactlyTheValue ()
		{
			RunCopyToReadsExactlyTheValueAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestCopyToReadsExactlyTheValueAsync ()
		{
			await RunCopyToReadsExactlyTheValueAsync (true);
		}

		async Task RunStreamConsumesPaddingSoTheNextPropertyIsReadableAsync (int length, bool async)
		{
			var first = CreatePayload (length);
			var second = CreatePayload (20);
			var logger = new TestTnefComplianceLogger ();

			var raw = BuildPropertyStream (properties => {
				properties.WriteBinaryProperty (TnefPropertyTag.RecordKey, first);
				properties.WriteBinaryProperty (TnefPropertyTag.SearchKey, second);
			});

			using var stream = new MemoryStream (raw, false);
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (await ReadAsync (reader, async) && reader.Tag != TnefAttributeTag.MapiProperties)
				continue;

			var properties = reader.GetPropertyReader ();

			Assert.That (await ReadNextPropertyAsync (properties, async), Is.True, "first ReadNextProperty");
			Assert.That (properties.Tag.Id, Is.EqualTo (TnefPropertyId.RecordKey), "first Tag");

			using (var value = properties.OpenValueStream ()) {
				using var actual = new MemoryStream ();

				if (async)
					await value.CopyToAsync (actual);
				else
					value.CopyTo (actual);

				Assert.That (actual.ToArray (), Is.EqualTo (first), "first payload");
			}

			Assert.That (await ReadNextPropertyAsync (properties, async), Is.True, "second ReadNextProperty");
			Assert.That (properties.Tag.Id, Is.EqualTo (TnefPropertyId.SearchKey), "second Tag");
			Assert.That (async ? await properties.ReadValueAsBytesAsync () : properties.ReadValueAsBytes (), Is.EqualTo (second), "second payload");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestStreamConsumesPaddingSoTheNextPropertyIsReadable (int length)
		{
			RunStreamConsumesPaddingSoTheNextPropertyIsReadableAsync (length, false).GetAwaiter ().GetResult ();
		}

		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public async Task TestStreamConsumesPaddingSoTheNextPropertyIsReadableAsync (int length)
		{
			await RunStreamConsumesPaddingSoTheNextPropertyIsReadableAsync (length, true);
		}

		async Task RunReadAfterReaderAdvancesThrowsInvalidOperationExceptionAsync (bool async)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, CreatePayload (17));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, CreatePayload (23));

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream);

			Assert.That (await ReadAsync (reader, async), Is.True, "Read TnefVersion");
			Assert.That (await ReadAsync (reader, async), Is.True, "Read Body");

			var value = reader.OpenValueStream ();
			var buffer = new byte[64];

			Assert.That (await ReadAsync (reader, async), Is.True, "Read Owner");

			if (async)
				Assert.ThrowsAsync<InvalidOperationException> (async () => await value.ReadAsync (buffer, 0, buffer.Length));
			else
				Assert.Throws<InvalidOperationException> (() => value.Read (buffer, 0, buffer.Length));

			value.Dispose ();
		}

		[Test]
		public void TestReadAfterReaderAdvancesThrowsInvalidOperationException ()
		{
			RunReadAfterReaderAdvancesThrowsInvalidOperationExceptionAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestReadAfterReaderAdvancesThrowsInvalidOperationExceptionAsync ()
		{
			await RunReadAfterReaderAdvancesThrowsInvalidOperationExceptionAsync (true);
		}

		async Task RunReadAfterDisposeThrowsObjectDisposedExceptionAsync (bool async)
		{
			using var stream = new MemoryStream (BuildBinaryPropertyStream (CreatePayload (32)), false);
			using var reader = new TnefReader (stream);
			var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);
			var buffer = new byte[64];

			value.Dispose ();

			if (async)
				Assert.ThrowsAsync<ObjectDisposedException> (async () => await value.ReadAsync (buffer, 0, buffer.Length));
			else
				Assert.Throws<ObjectDisposedException> (() => value.Read (buffer, 0, buffer.Length));
		}

		[Test]
		public void TestReadAfterDisposeThrowsObjectDisposedException ()
		{
			RunReadAfterDisposeThrowsObjectDisposedExceptionAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestReadAfterDisposeThrowsObjectDisposedExceptionAsync ()
		{
			await RunReadAfterDisposeThrowsObjectDisposedExceptionAsync (true);
		}

		[Test]
		public void TestFlushIsANoOp ()
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, false).GetAwaiter ().GetResult ();

			Assert.DoesNotThrow (() => value.Flush (), "Flush");
			Assert.DoesNotThrowAsync (() => value.FlushAsync (), "FlushAsync");

			var buffer = new byte[64];
			int nread = value.Read (buffer, 0, buffer.Length);

			Assert.That (nread, Is.EqualTo (payload.Length), "nread");
			Assert.That (buffer.AsSpan (0, nread).ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestReadValidatesItsArguments ()
		{
			using var stream = new MemoryStream (BuildBinaryPropertyStream (CreatePayload (32)), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, false).GetAwaiter ().GetResult ();
			var buffer = new byte[64];

			Assert.Throws<ArgumentNullException> (() => value.Read (null!, 0, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, -1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, buffer.Length + 1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 0, -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 0, buffer.Length + 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 32, 33));
		}

		[Test]
		public async Task TestReadAsyncValidatesItsArguments ()
		{
			using var stream = new MemoryStream (BuildBinaryPropertyStream (CreatePayload (32)), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, true);
			var buffer = new byte[64];

			Assert.ThrowsAsync<ArgumentNullException> (async () => await value.ReadAsync (null!, 0, 1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await value.ReadAsync (buffer, -1, 1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await value.ReadAsync (buffer, buffer.Length + 1, 1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await value.ReadAsync (buffer, 0, -1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await value.ReadAsync (buffer, 0, buffer.Length + 1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await value.ReadAsync (buffer, 32, 33));
		}

		async Task RunTruncatedValueDoesNotReadPastTheEndOfTheStreamAsync (bool async)
		{
			var payload = CreatePayload (4096);
			var raw = BuildBinaryPropertyStream (payload);
			var truncated = new byte[raw.Length - 2048];

			Buffer.BlockCopy (raw, 0, truncated, 0, truncated.Length);

			using var stream = new MemoryStream (truncated, false);
			using var reader = new TnefReader (stream);
			using var value = await OpenValueStreamAsync (reader, TnefPropertyId.RecordKey, async);
			using var actual = new MemoryStream ();

			var buffer = new byte[256];
			int nread, total = 0;

			while ((nread = await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async)) > 0) {
				actual.Write (buffer, 0, nread);
				total += nread;
				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read more than the value claimed to hold");
			}

			Assert.That (total, Is.LessThan (payload.Length), "the value should have been truncated");
			Assert.That (actual.ToArray (), Is.EqualTo (payload.AsSpan (0, total).ToArray ()), "payload");
		}

		[Test]
		public void TestTruncatedValueDoesNotReadPastTheEndOfTheStream ()
		{
			RunTruncatedValueDoesNotReadPastTheEndOfTheStreamAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestTruncatedValueDoesNotReadPastTheEndOfTheStreamAsync ()
		{
			await RunTruncatedValueDoesNotReadPastTheEndOfTheStreamAsync (true);
		}

		async Task RunAttributeValueStreamDoesNotReadPastTheAttributeAsync (int length, bool async)
		{
			var payload = CreatePayload (length);

			using var stream = new MemoryStream (BuildAttributeStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenAttributeValueStreamAsync (reader, TnefAttributeTag.Body, async);

			var buffer = new byte[8192];
			int nread = await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async);

			Assert.That (nread, Is.EqualTo (length), "nread");
			Assert.That (buffer.AsSpan (0, length).ToArray (), Is.EqualTo (payload), "payload");
			Assert.That (await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async), Is.EqualTo (0), "read at end of stream");
		}

		[TestCase (1)]
		[TestCase (3)]
		[TestCase (13)]
		[TestCase (1000)]
		public void TestAttributeValueStreamDoesNotReadPastTheAttribute (int length)
		{
			RunAttributeValueStreamDoesNotReadPastTheAttributeAsync (length, false).GetAwaiter ().GetResult ();
		}

		[TestCase (1)]
		[TestCase (3)]
		[TestCase (13)]
		[TestCase (1000)]
		public async Task TestAttributeValueStreamDoesNotReadPastTheAttributeAsync (int length)
		{
			await RunAttributeValueStreamDoesNotReadPastTheAttributeAsync (length, true);
		}

		async Task RunAttributeValueStreamPartialReadsAsync (int chunkSize, bool async)
		{
			var payload = CreatePayload (2000);

			using var stream = new MemoryStream (BuildAttributeStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = await OpenAttributeValueStreamAsync (reader, TnefAttributeTag.Body, async);
			using var actual = new MemoryStream ();

			var buffer = new byte[chunkSize];
			int nread, total = 0;

			while ((nread = await ReadValueStreamAsync (value, buffer, 0, buffer.Length, async)) > 0) {
				actual.Write (buffer, 0, nread);
				total += nread;
				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read past the end of the attribute");
			}

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[TestCase (1)]
		[TestCase (5)]
		[TestCase (256)]
		public void TestAttributeValueStreamPartialReads (int chunkSize)
		{
			RunAttributeValueStreamPartialReadsAsync (chunkSize, false).GetAwaiter ().GetResult ();
		}

		[TestCase (1)]
		[TestCase (5)]
		[TestCase (256)]
		public async Task TestAttributeValueStreamPartialReadsAsync (int chunkSize)
		{
			await RunAttributeValueStreamPartialReadsAsync (chunkSize, true);
		}

		async Task RunAttributeValueStreamLeavesTheReaderAbleToContinueAsync (bool async)
		{
			var payload = CreatePayload (37);
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, payload);
			builder.WriteMessageClass ("IPM.Note");

			using var stream = new MemoryStream (builder.ToArray (), false);
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			using (var value = await OpenAttributeValueStreamAsync (reader, TnefAttributeTag.Body, async)) {
				using var actual = new MemoryStream ();

				if (async)
					await value.CopyToAsync (actual);
				else
					value.CopyTo (actual);

				Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
			}

			Assert.That (await ReadAsync (reader, async), Is.True, "Read");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MessageClass), "Tag");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestAttributeValueStreamLeavesTheReaderAbleToContinue ()
		{
			RunAttributeValueStreamLeavesTheReaderAbleToContinueAsync (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestAttributeValueStreamLeavesTheReaderAbleToContinueAsync ()
		{
			await RunAttributeValueStreamLeavesTheReaderAbleToContinueAsync (true);
		}
	}
}
