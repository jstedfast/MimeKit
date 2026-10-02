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
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		[Test]
		public void TestTnefReaderStream ()
		{
			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				using (var reader = new TnefReader (stream)) {
					var buffer = new byte[1024];

					using (var tnef = new TnefReaderStream (reader, 0, 0)) {
						Assert.That (tnef.CanRead, Is.True);
						Assert.That (tnef.CanWrite, Is.False);
						Assert.That (tnef.CanSeek, Is.False);
						Assert.That (tnef.CanTimeout, Is.False);

						Assert.Throws<ArgumentNullException> (() => tnef.Read (null, 0, buffer.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => tnef.Read (buffer, -1, buffer.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => tnef.Read (buffer, 0, -1));

						Assert.Throws<NotSupportedException> (() => tnef.Write (buffer, 0, buffer.Length));
						Assert.Throws<NotSupportedException> (() => tnef.Seek (0, SeekOrigin.End));
						Assert.Throws<NotSupportedException> (() => tnef.Flush ());
						Assert.Throws<NotSupportedException> (() => tnef.SetLength (1024));

						Assert.Throws<NotSupportedException> (() => { var x = tnef.Position; });
						Assert.Throws<NotSupportedException> (() => { tnef.Position = 0; });
						Assert.Throws<NotSupportedException> (() => { var x = tnef.Length; });
					}
				}
			}
		}

		// Builds a TNEF stream containing a single attMsgProps attribute whose properties are built by the
		// caller. Everything in this fixture reads its value through TnefReaderStream.
		static byte[] BuildPropertyStream (Action<TnefMapiPropertyBuilder> build)
		{
			var properties = new TnefMapiPropertyBuilder ();

			build (properties);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder.ToArray ();
		}

		static byte[] BuildBinaryPropertyStream (byte[] payload)
		{
			return BuildPropertyStream (properties => properties.WriteBinaryProperty (TnefPropertyTag.RecordKey, payload));
		}

		// Advances the reader to the named property and returns a stream over its raw value.
		static Stream OpenValueStream (TnefReader reader, TnefPropertyId id)
		{
			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
					continue;

				var properties = reader.TnefPropertyReader;

				while (properties.ReadNextProperty ()) {
					if (properties.PropertyTag.Id == id)
						return properties.GetRawValueReadStream ();
				}
			}

			Assert.Fail ($"Did not find a {id} property.");

			return null;
		}

		static byte[] CreatePayload (int length)
		{
			var payload = new byte[length];

			for (int i = 0; i < length; i++)
				payload[i] = (byte) (i & 0xFF);

			return payload;
		}

		[TestCase (0)]
		[TestCase (1)]
		[TestCase (3)]
		[TestCase (4)]
		[TestCase (13)]
		[TestCase (1000)]
		public void TestReadDoesNotReturnMoreThanTheValue (int length)
		{
			// A single read asking for far more than the value contains must stop at the end of the value
			// rather than running on into whatever follows it in the attribute.
			var payload = CreatePayload (length);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			var buffer = new byte[8192];
			int nread = value.Read (buffer, 0, buffer.Length);

			Assert.That (nread, Is.EqualTo (length), "nread");
			Assert.That (buffer.AsSpan (0, length).ToArray (), Is.EqualTo (payload), "payload");
			Assert.That (value.Read (buffer, 0, buffer.Length), Is.EqualTo (0), "read at end of stream");
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (3)]
		[TestCase (7)]
		[TestCase (64)]
		[TestCase (999)]
		public void TestPartialReads (int chunkSize)
		{
			// Reading the value a few bytes at a time must produce exactly the same bytes as reading it all
			// at once, and must stop in the same place.
			var payload = CreatePayload (1000);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);
			using var actual = new MemoryStream ();

			var buffer = new byte[chunkSize];
			int nread, total = 0;

			while ((nread = value.Read (buffer, 0, buffer.Length)) > 0) {
				Assert.That (nread, Is.LessThanOrEqualTo (chunkSize), "nread");

				actual.Write (buffer, 0, nread);
				total += nread;

				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read past the end of the value");
			}

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestReadIntoTheMiddleOfABuffer ()
		{
			// The offset argument has to be honoured; nothing outside of [offset, offset + count) may be
			// touched.
			var payload = CreatePayload (64);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			var buffer = new byte[256];

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = 0xAA;

			int nread = value.Read (buffer, 100, 64);

			Assert.That (nread, Is.EqualTo (64), "nread");
			Assert.That (buffer.AsSpan (100, 64).ToArray (), Is.EqualTo (payload), "payload");

			for (int i = 0; i < 100; i++)
				Assert.That (buffer[i], Is.EqualTo (0xAA), $"buffer[{i}] was modified");

			for (int i = 164; i < buffer.Length; i++)
				Assert.That (buffer[i], Is.EqualTo (0xAA), $"buffer[{i}] was modified");
		}

		[Test]
		public void TestReadWithZeroCountDoesNotAdvance ()
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			var buffer = new byte[64];

			Assert.That (value.Read (buffer, 0, 0), Is.EqualTo (0), "zero-length read");
			Assert.That (value.Read (buffer, 0, buffer.Length), Is.EqualTo (32), "nread");
			Assert.That (buffer.AsSpan (0, 32).ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestRepeatedReadsAtEndOfStreamReturnZero ()
		{
			var payload = CreatePayload (16);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			var buffer = new byte[64];

			Assert.That (value.Read (buffer, 0, buffer.Length), Is.EqualTo (16), "nread");

			long offset = reader.StreamOffset;

			for (int i = 0; i < 10; i++)
				Assert.That (value.Read (buffer, 0, buffer.Length), Is.EqualTo (0), $"read {i}");

			Assert.That (reader.StreamOffset, Is.EqualTo (offset), "reading at end of stream advanced the reader");
		}

		[Test]
		public void TestCopyToReadsExactlyTheValue ()
		{
			var payload = CreatePayload (5000);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);
			using var actual = new MemoryStream ();

			value.CopyTo (actual, 137);

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[TestCase (13)]
		[TestCase (14)]
		[TestCase (15)]
		[TestCase (16)]
		public void TestStreamConsumesPaddingSoTheNextPropertyIsReadable (int length)
		{
			// A variable length value is padded out to a 4 byte boundary. Reading one through the stream
			// has to consume that padding, otherwise the property that follows it starts being parsed from
			// the middle of the padding.
			var first = CreatePayload (length);
			var second = CreatePayload (20);

			var raw = BuildPropertyStream (properties => {
				properties.WriteBinaryProperty (TnefPropertyTag.RecordKey, first);
				properties.WriteBinaryProperty (TnefPropertyTag.SearchKey, second);
			});

			using var stream = new MemoryStream (raw, false);
			using var reader = new TnefReader (stream);

			while (reader.ReadNextAttribute () && reader.AttributeTag != TnefAttributeTag.MapiProperties)
				continue;

			Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.MapiProperties), "AttributeTag");

			var properties = reader.TnefPropertyReader;

			Assert.That (properties.ReadNextProperty (), Is.True, "first ReadNextProperty");
			Assert.That (properties.PropertyTag.Id, Is.EqualTo (TnefPropertyId.RecordKey), "first PropertyTag");

			using (var value = properties.GetRawValueReadStream ()) {
				using var actual = new MemoryStream ();

				value.CopyTo (actual);

				Assert.That (actual.ToArray (), Is.EqualTo (first), "first payload");
			}

			Assert.That (properties.ReadNextProperty (), Is.True, "second ReadNextProperty");
			Assert.That (properties.PropertyTag.Id, Is.EqualTo (TnefPropertyId.SearchKey), "second PropertyTag");
			Assert.That (properties.ReadValueAsBytes (), Is.EqualTo (second), "second payload");
			Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
		}

		[Test]
		public void TestReadAfterDisposeThrowsObjectDisposedException ()
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			value.Dispose ();

			var buffer = new byte[64];

			Assert.Throws<ObjectDisposedException> (() => value.Read (buffer, 0, buffer.Length));
		}

		[Test]
		public void TestReadValidatesItsArguments ()
		{
			var payload = CreatePayload (32);

			using var stream = new MemoryStream (BuildBinaryPropertyStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);

			var buffer = new byte[64];

			Assert.Throws<ArgumentNullException> (() => value.Read (null, 0, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, -1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, buffer.Length + 1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 0, -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 0, buffer.Length + 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => value.Read (buffer, 32, 33));
		}

		[Test]
		public void TestTruncatedValueDoesNotReadPastTheEndOfTheStream ()
		{
			// The declared length of the value runs past the end of the data that is actually there, so the
			// stream has to stop at whatever it can get rather than block or loop.
			var payload = CreatePayload (4096);
			var raw = BuildBinaryPropertyStream (payload);
			var truncated = new byte[raw.Length - 2048];

			Buffer.BlockCopy (raw, 0, truncated, 0, truncated.Length);

			using var stream = new MemoryStream (truncated, false);
			using var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose);
			using var value = OpenValueStream (reader, TnefPropertyId.RecordKey);
			using var actual = new MemoryStream ();

			var buffer = new byte[256];
			int nread, total = 0;

			while ((nread = value.Read (buffer, 0, buffer.Length)) > 0) {
				actual.Write (buffer, 0, nread);
				total += nread;

				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read more than the value claimed to hold");
			}

			Assert.That (total, Is.LessThan (payload.Length), "the value should have been truncated");
			Assert.That (actual.ToArray (), Is.EqualTo (payload.AsSpan (0, total).ToArray ()), "payload");
		}

		// When the value being read is a whole attribute rather than a MAPI property, there is no length
		// prefix and no padding, so TnefReaderStream takes a different path through GetRawValueReadStream.
		static byte[] BuildAttributeStream (byte[] payload)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, payload);

			return builder.ToArray ();
		}

		static Stream OpenAttributeValueStream (TnefReader reader, TnefAttributeTag tag)
		{
			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag == tag)
					return reader.TnefPropertyReader.GetRawValueReadStream ();
			}

			Assert.Fail ($"Did not find a {tag} attribute.");

			return null;
		}

		[TestCase (1)]
		[TestCase (3)]
		[TestCase (13)]
		[TestCase (1000)]
		public void TestAttributeValueStreamDoesNotReadPastTheAttribute (int length)
		{
			var payload = CreatePayload (length);

			using var stream = new MemoryStream (BuildAttributeStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenAttributeValueStream (reader, TnefAttributeTag.Body);

			var buffer = new byte[8192];
			int nread = value.Read (buffer, 0, buffer.Length);

			Assert.That (nread, Is.EqualTo (length), "nread");
			Assert.That (buffer.AsSpan (0, length).ToArray (), Is.EqualTo (payload), "payload");
			Assert.That (value.Read (buffer, 0, buffer.Length), Is.EqualTo (0), "read at end of stream");
		}

		[TestCase (1)]
		[TestCase (5)]
		[TestCase (256)]
		public void TestAttributeValueStreamPartialReads (int chunkSize)
		{
			var payload = CreatePayload (2000);

			using var stream = new MemoryStream (BuildAttributeStream (payload), false);
			using var reader = new TnefReader (stream);
			using var value = OpenAttributeValueStream (reader, TnefAttributeTag.Body);
			using var actual = new MemoryStream ();

			var buffer = new byte[chunkSize];
			int nread, total = 0;

			while ((nread = value.Read (buffer, 0, buffer.Length)) > 0) {
				actual.Write (buffer, 0, nread);
				total += nread;

				Assert.That (total, Is.LessThanOrEqualTo (payload.Length), "read past the end of the attribute");
			}

			Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
		}

		[Test]
		public void TestAttributeValueStreamLeavesTheReaderAbleToContinue ()
		{
			// Draining an attribute through the stream must leave the reader positioned so that the next
			// attribute, including its checksum, still parses.
			var payload = CreatePayload (37);
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, payload);
			builder.WriteMessageClass ("IPM.Note");

			using var stream = new MemoryStream (builder.ToArray (), false);
			using var reader = new TnefReader (stream);

			using (var value = OpenAttributeValueStream (reader, TnefAttributeTag.Body)) {
				using var actual = new MemoryStream ();

				value.CopyTo (actual);

				Assert.That (actual.ToArray (), Is.EqualTo (payload), "payload");
			}

			Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");
			Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.MessageClass), "AttributeTag");
			Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
		}
	}
}
