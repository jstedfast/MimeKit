//
// TnefFuzzTests.cs
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

using System.Diagnostics;
using System.Text;

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	/// <summary>
	/// Asserts the invariants that the TNEF reader must uphold when fed hostile input.
	/// </summary>
	[TestFixture]
	public class TnefFuzzTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");

		static readonly TnefPropertyTag AttachMethodTag = new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long);
		static readonly TnefPropertyTag AttachDataTag = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object);
		static readonly TnefPropertyTag SubjectTag = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);

		const int TimeoutMilliseconds = 30000;

		static bool ContainsProperties (TnefAttributeTag tag)
		{
			switch (tag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.RecipientTable:
				return true;
			default:
				return false;
			}
		}

		static bool IsVariableLength (TnefPropertyType type)
		{
			switch (type) {
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object:
				return true;
			default:
				return false;
			}
		}

		static void DrainPropertyValues (TnefPropertyReader properties)
		{
			if (properties.ValueCount == 0)
				return;

			do {
				if (properties.IsEmbeddedMessage) {
					using (var embedded = properties.OpenEmbeddedMessage ())
						DrainReader (embedded);
				} else if (IsVariableLength (properties.PropertyType)) {
					using (var stream = properties.OpenValueStream ())
						stream.CopyTo (Stream.Null);
				} else {
					GC.KeepAlive (properties.Tag);
				}
			} while (properties.ReadNextValue ());
		}

		static void DrainProperties (TnefPropertyReader properties, bool isTable)
		{
			if (isTable) {
				while (properties.ReadNextRow ()) {
					while (properties.ReadNextProperty ())
						DrainPropertyValues (properties);
				}
			} else {
				while (properties.ReadNextProperty ())
					DrainPropertyValues (properties);
			}
		}

		public static void DrainReader (TnefReader reader)
		{
			while (reader.Read ()) {
				if (ContainsProperties (reader.Tag)) {
					var properties = reader.GetPropertyReader ();

					DrainProperties (properties, reader.Tag == TnefAttributeTag.RecipientTable);
				} else {
					using (var stream = reader.OpenValueStream ())
						stream.CopyTo (Stream.Null);
				}
			}
		}

		static async Task DrainPropertyValuesAsync (TnefPropertyReader properties)
		{
			if (properties.ValueCount == 0)
				return;

			do {
				if (properties.IsEmbeddedMessage) {
					using (var embedded = properties.OpenEmbeddedMessage ())
						await DrainReaderAsync (embedded).ConfigureAwait (false);
				} else if (IsVariableLength (properties.PropertyType)) {
					using (var stream = properties.OpenValueStream ())
						await stream.CopyToAsync (Stream.Null).ConfigureAwait (false);
				} else {
					GC.KeepAlive (properties.Tag);
				}
			} while (await properties.ReadNextValueAsync ().ConfigureAwait (false));
		}

		static async Task DrainPropertiesAsync (TnefPropertyReader properties, bool isTable)
		{
			if (isTable) {
				while (await properties.ReadNextRowAsync ().ConfigureAwait (false)) {
					while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
						await DrainPropertyValuesAsync (properties).ConfigureAwait (false);
				}
			} else {
				while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
					await DrainPropertyValuesAsync (properties).ConfigureAwait (false);
			}
		}

		public static async Task DrainReaderAsync (TnefReader reader)
		{
			while (await reader.ReadAsync ().ConfigureAwait (false)) {
				if (ContainsProperties (reader.Tag)) {
					var properties = reader.GetPropertyReader ();

					await DrainPropertiesAsync (properties, reader.Tag == TnefAttributeTag.RecipientTable).ConfigureAwait (false);
				} else {
					using (var stream = reader.OpenValueStream ())
						await stream.CopyToAsync (Stream.Null).ConfigureAwait (false);
				}
			}
		}

		static MimeMessage ConvertToMime (byte[] data)
		{
			return TnefConversionTestHelper.ConvertArbitrary (data);
		}

		public static void AssertInvariants (byte[] data, string what)
		{
			var allocatedBefore = GC.GetAllocatedBytesForCurrentThread ();
			var stopwatch = Stopwatch.StartNew ();
			Exception exception = null;

			try {
				using var stream = new MemoryStream (data, false);
				using var reader = new TnefReader (stream) { ComplianceLogger = new TestTnefComplianceLogger () };

				DrainReader (reader);
			} catch (Exception ex) {
				exception = ex;
			} finally {
				stopwatch.Stop ();
			}

			Assert.That (exception, Is.Null, what + " threw " + exception?.GetType ().Name + ": " + exception?.Message + Environment.NewLine + exception?.StackTrace);
			Assert.That (stopwatch.ElapsedMilliseconds, Is.LessThan (TimeoutMilliseconds), what + " took too long");

			var allocated = GC.GetAllocatedBytesForCurrentThread () - allocatedBefore;
			var ceiling = (4 * 1024 * 1024) + (64L * Math.Max (data.Length, 1));

			Assert.That (allocated, Is.LessThan (ceiling), $"{what} allocated {allocated} bytes from a {data.Length} byte input");
		}

		public static async Task AssertInvariantsAsync (byte[] data, string what)
		{
			var allocatedBefore = GC.GetAllocatedBytesForCurrentThread ();
			var stopwatch = Stopwatch.StartNew ();
			Exception exception = null;

			try {
				using var stream = new MemoryStream (data, false);
				using var reader = new TnefReader (stream) { ComplianceLogger = new TestTnefComplianceLogger () };

				await DrainReaderAsync (reader).ConfigureAwait (false);
			} catch (Exception ex) {
				exception = ex;
			} finally {
				stopwatch.Stop ();
			}

			Assert.That (exception, Is.Null, what + " threw " + exception?.GetType ().Name + ": " + exception?.Message + Environment.NewLine + exception?.StackTrace);
			Assert.That (stopwatch.ElapsedMilliseconds, Is.LessThan (TimeoutMilliseconds), what + " took too long");

			var allocated = GC.GetAllocatedBytesForCurrentThread () - allocatedBefore;
			var ceiling = (4 * 1024 * 1024) + (64L * Math.Max (data.Length, 1));

			Assert.That (allocated, Is.LessThan (ceiling), $"{what} allocated {allocated} bytes from a {data.Length} byte input");
		}

		public class MalformedCase
		{
			public string Name { get; }
			public byte[] Data { get; }

			public MalformedCase (string name, byte[] data)
			{
				Name = name;
				Data = data;
			}

			public override string ToString () => Name;
		}

		static byte[] WellFormed ()
		{
			var props = new TnefMapiPropertyBuilder ();

			props.WriteStringProperty (SubjectTag, "This is the subject");
			props.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long), 1);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, props);
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, Encoding.ASCII.GetBytes ("file.txt\0"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[256]);

			return builder.ToArray ();
		}

		static byte[] EmbeddedMessage (byte[] embedded, int depth = 1)
		{
			for (int i = 0; i < depth; i++) {
				var value = new byte[16 + embedded.Length];

				IID_IMessage.ToByteArray ().CopyTo (value, 0);
				embedded.CopyTo (value, 16);

				var props = new TnefMapiPropertyBuilder ();

				props.WriteInt32Property (AttachMethodTag, (int) TnefAttachMethod.EmbeddedMessage);
				props.WriteBinaryProperty (AttachDataTag, value);

				var builder = new TnefBuilder ();

				builder.WriteTnefVersion ();
				builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
				builder.WriteMapiProperties (TnefAttributeLevel.Attachment, props);

				embedded = builder.ToArray ();
			}

			return embedded;
		}

		public static IEnumerable<MalformedCase> MalformedCases ()
		{
			var wellFormed = WellFormed ();

			yield return new MalformedCase ("well-formed", wellFormed);
			yield return new MalformedCase ("empty", Array.Empty<byte> ());
			yield return new MalformedCase ("signature-truncated", new byte[] { 0x78, 0x9f });
			yield return new MalformedCase ("bad-signature", new TnefBuilder (0x00000000).WriteTnefVersion ().ToArray ());
			yield return new MalformedCase ("bad-signature-inverted", new TnefBuilder (unchecked ((int) 0x789f3e22)).WriteTnefVersion ().ToArray ());
			yield return new MalformedCase ("header-only", new TnefBuilder ().ToArray ());
			yield return new MalformedCase ("missing-legacy-key", new byte[] { 0x78, 0x9f, 0x3e, 0x22 });

			var small = new TnefBuilder ().WriteTnefVersion ().WriteOemCodepage (1252).WriteMessageClass ("IPM.Note").ToArray ();

			for (int i = 0; i <= small.Length; i++) {
				var truncated = new byte[i];

				Buffer.BlockCopy (small, 0, truncated, 0, i);

				yield return new MalformedCase ($"truncated-at-{i}", truncated);
			}

			yield return new MalformedCase ("bad-checksum",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), checksum: 0x1234).ToArray ());
			yield return new MalformedCase ("negative-length",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), length: -1).ToArray ());
			yield return new MalformedCase ("int-min-length",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), length: int.MinValue).ToArray ());
			yield return new MalformedCase ("huge-length",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), length: int.MaxValue).ToArray ());
			yield return new MalformedCase ("lying-long-length",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), length: 1024).ToArray ());
			yield return new MalformedCase ("zero-length-attribute",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, Array.Empty<byte> ()).ToArray ());
			yield return new MalformedCase ("invalid-level",
				new TnefBuilder ().WriteAttribute ((TnefAttributeLevel) 0x7f, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000)).ToArray ());
			yield return new MalformedCase ("message-level-after-attachment",
				new TnefBuilder ()
					.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 })
					.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, Encoding.ASCII.GetBytes ("IPM.Note\0"))
					.ToArray ());
			yield return new MalformedCase ("unknown-attribute-tag",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, (TnefAttributeTag) 0x7fff, new byte[16]).ToArray ());
			yield return new MalformedCase ("attachment-attribute-at-message-level",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.AttachData, new byte[16]).ToArray ());
			yield return new MalformedCase ("garbage-after-valid-stream",
				new TnefBuilder ().WriteTnefVersion ().WriteRaw (Enumerable.Range (0, 64).Select (i => (byte) i).ToArray ()).ToArray ());
			yield return new MalformedCase ("huge-message-class",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageClass, new byte[64 * 1024]).ToArray ());

			yield return new MalformedCase ("property-count-overflows-attribute",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message, Subject (), count: 1000).ToArray ());
			yield return new MalformedCase ("negative-property-count",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message, Subject (), count: -1).ToArray ());
			yield return new MalformedCase ("int-max-property-count",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message, Subject (), count: int.MaxValue).ToArray ());
			yield return new MalformedCase ("property-value-length-overflows-attribute",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ().WriteBinaryProperty (new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary), new byte[8], length: 1 << 24)).ToArray ());
			yield return new MalformedCase ("negative-property-value-length",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ().WriteBinaryProperty (new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary), new byte[8], length: -1)).ToArray ());
			yield return new MalformedCase ("unpadded-property-value",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WriteBinaryProperty (new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary), new byte[7], pad: false)
						.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long), 1)).ToArray ());
			yield return new MalformedCase ("unknown-property-type",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ().WriteProperty (new TnefPropertyTag (TnefPropertyId.Subject, (TnefPropertyType) 0x0fff), new byte[8])).ToArray ());
			yield return new MalformedCase ("multi-valued-count-exceeds-payload",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode | TnefPropertyType.MultiValued))
						.WriteValueCount (1 << 20)
						.WriteRaw (new byte[8])).ToArray ());
			yield return new MalformedCase ("negative-multi-valued-count",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode | TnefPropertyType.MultiValued))
						.WriteValueCount (-1)
						.WriteRaw (new byte[8])).ToArray ());
			yield return new MalformedCase ("named-property-truncated",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WriteRaw (new byte[] { 0x1f, 0x00, 0x00, 0x80 })
						.WriteRaw (new byte[8]), count: 1).ToArray ());
			yield return new MalformedCase ("named-property-huge-name-length",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WriteRaw (new byte[] { 0x1f, 0x00, 0x00, 0x80 })
						.WriteRaw (Guid.Empty.ToByteArray ())
						.WriteValueCount (0)
						.WriteValueCount (int.MaxValue)
						.WriteRaw (new byte[8]), count: 1).ToArray ());
			yield return new MalformedCase ("row-count-overflows-attribute",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable,
					TnefBuilder.Int32Payload (1 << 20)).ToArray ());
			yield return new MalformedCase ("negative-row-count",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable,
					TnefBuilder.Int32Payload (-1)).ToArray ());

			foreach (var type in new[] { TnefPropertyType.I2, TnefPropertyType.Long, TnefPropertyType.R4, TnefPropertyType.Double,
										 TnefPropertyType.Currency, TnefPropertyType.AppTime, TnefPropertyType.Error,
										 TnefPropertyType.Boolean, TnefPropertyType.I8, TnefPropertyType.SysTime,
										 TnefPropertyType.ClassId, TnefPropertyType.Unspecified, TnefPropertyType.Null }) {
				yield return new MalformedCase ($"truncated-{type}",
					new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
						new TnefMapiPropertyBuilder ().WriteProperty (new TnefPropertyTag (TnefPropertyId.Subject, type), new byte[1])).ToArray ());
			}

			yield return new MalformedCase ("embedded-message", EmbeddedMessage (new TnefBuilder ().WriteTnefVersion ().ToArray ()));
			yield return new MalformedCase ("deeply-nested", EmbeddedMessage (new TnefBuilder ().WriteTnefVersion ().ToArray (), 48));
			yield return new MalformedCase ("embedded-bad-signature", EmbeddedMessage (new TnefBuilder (0).WriteTnefVersion ().ToArray ()));
			yield return new MalformedCase ("embedded-empty", EmbeddedMessage (Array.Empty<byte> ()));
			yield return new MalformedCase ("embedded-truncated", EmbeddedMessage (new byte[] { 0x78, 0x9f, 0x3e }));

			var many = new TnefBuilder ();

			for (int i = 0; i < 2000; i++)
				many.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), checksum: 0);

			yield return new MalformedCase ("thousands-of-bad-checksums", many.ToArray ());
		}

		static TnefMapiPropertyBuilder Subject ()
		{
			return new TnefMapiPropertyBuilder ().WriteStringProperty (SubjectTag, "subject");
		}

		static readonly MalformedCase[] Cases = MalformedCases ().ToArray ();

		public static IEnumerable<TestCaseData> ReaderCases ()
		{
			foreach (var test in Cases)
				yield return new TestCaseData (test).SetArgDisplayNames (test.Name);
		}

		[TestCaseSource (nameof (ReaderCases))]
		public void TestReaderUpholdsInvariants (MalformedCase test)
		{
			AssertInvariants (test.Data, test.Name);
		}

		[TestCaseSource (nameof (ReaderCases))]
		public async Task TestReaderUpholdsInvariantsAsync (MalformedCase test)
		{
			await AssertInvariantsAsync (test.Data, test.Name).ConfigureAwait (false);
		}

		[TestCaseSource (nameof (ReaderCases))]
		public void TestConvertToMimeUpholdsInvariants (MalformedCase test)
		{
			Assert.DoesNotThrow (() => ConvertToMime (test.Data).Dispose (), test.Name);
		}

		[Test]
		public void TestWellFormedStreamIsCompliant ()
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = new MemoryStream (WellFormed (), false)) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
					DrainReader (reader);
			}

			Assert.That (logger.Issues, Is.Empty);
		}

		[Test]
		public async Task TestWellFormedStreamIsCompliantAsync ()
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = new MemoryStream (WellFormed (), false)) {
				using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
					await DrainReaderAsync (reader).ConfigureAwait (false);
			}

			Assert.That (logger.Issues, Is.Empty);
		}
	}
}
