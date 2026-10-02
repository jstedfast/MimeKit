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
	/// <remarks>
	/// <para>In <see cref="TnefComplianceMode.Loose"/> the reader must never throw anything other
	/// than <see cref="TnefException"/>, must always terminate, and must never allocate an
	/// unbounded amount of memory in response to an attacker-supplied length.</para>
	/// <para>In <see cref="TnefComplianceMode.Strict"/> the reader must also only ever throw
	/// <see cref="TnefException"/>.</para>
	/// </remarks>
	[TestFixture]
	public class TnefFuzzTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020D0B-0000-0000-C000-000000000046");

		static readonly TnefPropertyTag AttachMethodTag = new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long);
		static readonly TnefPropertyTag AttachDataTag = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object);
		static readonly TnefPropertyTag SubjectTag = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);
		static readonly TnefPropertyTag BodyTag = new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.String8);

		// A single read of a malformed stream must never take anywhere near this long.
		const int TimeoutMilliseconds = 30000;

		#region Reader drivers

		// Different ways a caller can drive the reader. Each one reaches different code paths, so
		// every malformed input is run through all of them.
		public enum DriveStrategy
		{
			Attributes,
			RawValues,
			Properties,
			Rows,
			EmbeddedMessages,
			ConvertToMessage
		}

		public static IEnumerable<DriveStrategy> Strategies => Enum.GetValues (typeof (DriveStrategy)).Cast<DriveStrategy> ();

		static void DriveAttributes (TnefReader reader)
		{
			while (reader.ReadNextAttribute ())
				GC.KeepAlive (reader.AttributeTag);
		}

		static void DriveRawValues (TnefReader reader)
		{
			var buffer = new byte[1024];

			while (reader.ReadNextAttribute ()) {
				int n;

				while ((n = reader.ReadAttributeRawValue (buffer, 0, buffer.Length)) > 0)
					GC.KeepAlive (n);
			}
		}

		static void DriveProperties (TnefReader reader)
		{
			while (reader.ReadNextAttribute ()) {
				var prop = reader.TnefPropertyReader;

				while (prop.ReadNextProperty ())
					GC.KeepAlive (prop.ReadValue ());
			}
		}

		static void DriveRows (TnefReader reader)
		{
			while (reader.ReadNextAttribute ()) {
				var prop = reader.TnefPropertyReader;

				while (prop.ReadNextRow ()) {
					while (prop.ReadNextProperty ())
						GC.KeepAlive (prop.ReadValue ());
				}
			}
		}

		static void DriveEmbeddedMessages (TnefReader reader, int depth = 0)
		{
			while (reader.ReadNextAttribute ()) {
				var prop = reader.TnefPropertyReader;

				while (prop.ReadNextProperty ()) {
					if (prop.IsEmbeddedMessage && depth < 16) {
						using (var embedded = prop.GetEmbeddedMessageReader ())
							DriveEmbeddedMessages (embedded, depth + 1);
					} else {
						GC.KeepAlive (prop.ReadValue ());
					}
				}
			}
		}

		static void Drive (DriveStrategy strategy, byte[] data, TnefComplianceMode mode)
		{
			if (strategy == DriveStrategy.ConvertToMessage) {
				var part = new TnefPart { Content = new MimeContent (new MemoryStream (data, false)) };

				GC.KeepAlive (part.ConvertToMessage ());
				return;
			}

			using (var stream = new MemoryStream (data, false)) {
				using (var reader = new TnefReader (stream, 0, mode)) {
					switch (strategy) {
					case DriveStrategy.Attributes: DriveAttributes (reader); break;
					case DriveStrategy.RawValues: DriveRawValues (reader); break;
					case DriveStrategy.Properties: DriveProperties (reader); break;
					case DriveStrategy.Rows: DriveRows (reader); break;
					case DriveStrategy.EmbeddedMessages: DriveEmbeddedMessages (reader); break;
					}
				}
			}
		}

		/// <summary>
		/// Drive the reader and assert that nothing other than a <see cref="TnefException"/> escapes,
		/// that it terminates, and that it does not allocate unreasonably.
		/// </summary>
		/// <remarks>
		/// <para><see cref="TnefPropertyReader.ReadValue"/> and its typed siblings document
		/// <see cref="EndOfStreamException"/> as part of their contract, so the strategies that call them
		/// directly are allowed to raise it. <see cref="TnefPart.ConvertToMessage"/> is held to the
		/// stricter rule, because it is the API a content conversion pipeline actually calls and it has
		/// no documented exception for a truncated stream.</para>
		/// </remarks>
		public static void AssertInvariants (DriveStrategy strategy, byte[] data, TnefComplianceMode mode, string what)
		{
			var allocatedBefore = GC.GetAllocatedBytesForCurrentThread ();
			var stopwatch = Stopwatch.StartNew ();

			try {
				Drive (strategy, data, mode);
			} catch (TnefException) {
				// This is the only exception the reader is allowed to raise.
			} catch (EndOfStreamException) when (strategy != DriveStrategy.ConvertToMessage) {
				// Documented on the TnefPropertyReader value accessors.
			} catch (Exception ex) {
				Assert.Fail ($"{what} ({strategy}, {mode}) threw {ex.GetType ().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
			} finally {
				stopwatch.Stop ();
			}

			Assert.That (stopwatch.ElapsedMilliseconds, Is.LessThan (TimeoutMilliseconds), $"{what} ({strategy}, {mode}) took too long");

			// A malformed stream must not be able to make the reader allocate wildly more than the
			// size of the input itself. The constant floor covers fixed per-reader overhead.
			var allocated = GC.GetAllocatedBytesForCurrentThread () - allocatedBefore;
			var ceiling = (4 * 1024 * 1024) + (64L * Math.Max (data.Length, 1));

			Assert.That (allocated, Is.LessThan (ceiling), $"{what} ({strategy}, {mode}) allocated {allocated} bytes from a {data.Length} byte input");
		}

		#endregion

		#region Malformed stream catalogue

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

			// Sanity: the well-formed stream itself must survive every driver.
			yield return new MalformedCase ("well-formed", wellFormed);

			#region Stream header

			yield return new MalformedCase ("empty", Array.Empty<byte> ());
			yield return new MalformedCase ("signature-truncated", new byte[] { 0x78, 0x9f });
			yield return new MalformedCase ("bad-signature", new TnefBuilder (0x00000000).WriteTnefVersion ().ToArray ());
			yield return new MalformedCase ("bad-signature-inverted", new TnefBuilder (unchecked ((int) 0x789f3e22)).WriteTnefVersion ().ToArray ());
			yield return new MalformedCase ("header-only", new TnefBuilder ().ToArray ());
			yield return new MalformedCase ("missing-legacy-key", new byte[] { 0x78, 0x9f, 0x3e, 0x22 });

			#endregion

			#region Truncation at every byte boundary of a small well-formed stream

			var small = new TnefBuilder ().WriteTnefVersion ().WriteOemCodepage (1252).WriteMessageClass ("IPM.Note").ToArray ();

			for (int i = 0; i <= small.Length; i++) {
				var truncated = new byte[i];

				Buffer.BlockCopy (small, 0, truncated, 0, i);

				yield return new MalformedCase ($"truncated-at-{i}", truncated);
			}

			#endregion

			#region Attribute framing

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

			#endregion

			#region MAPI property framing

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
						.WriteRaw (new byte[] { 0x1f, 0x00, 0x00, 0x80 }) // PT_UNICODE, id 0x8000 (named)
						.WriteRaw (new byte[8]), count: 1).ToArray ());

			yield return new MalformedCase ("named-property-huge-name-length",
				new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
					new TnefMapiPropertyBuilder ()
						.WriteRaw (new byte[] { 0x1f, 0x00, 0x00, 0x80 })
						.WriteRaw (Guid.Empty.ToByteArray ())
						.WriteValueCount (0)              // TnefNameIdKind.Name
						.WriteValueCount (int.MaxValue)   // name length
						.WriteRaw (new byte[8]), count: 1).ToArray ());

			yield return new MalformedCase ("row-count-overflows-attribute",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable,
					TnefBuilder.Int32Payload (1 << 20)).ToArray ());

			yield return new MalformedCase ("negative-row-count",
				new TnefBuilder ().WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable,
					TnefBuilder.Int32Payload (-1)).ToArray ());

			#endregion

			#region Fixed-width property types truncated

			foreach (var type in new[] { TnefPropertyType.I2, TnefPropertyType.Long, TnefPropertyType.R4, TnefPropertyType.Double,
										 TnefPropertyType.Currency, TnefPropertyType.AppTime, TnefPropertyType.Error,
										 TnefPropertyType.Boolean, TnefPropertyType.I8, TnefPropertyType.SysTime,
										 TnefPropertyType.ClassId, TnefPropertyType.Unspecified, TnefPropertyType.Null }) {
				yield return new MalformedCase ($"truncated-{type}",
					new TnefBuilder ().WriteMapiProperties (TnefAttributeLevel.Message,
						new TnefMapiPropertyBuilder ().WriteProperty (new TnefPropertyTag (TnefPropertyId.Subject, type), new byte[1])).ToArray ());
			}

			#endregion

			#region Embedded messages

			yield return new MalformedCase ("embedded-message", EmbeddedMessage (new TnefBuilder ().WriteTnefVersion ().ToArray ()));
			yield return new MalformedCase ("deeply-nested", EmbeddedMessage (new TnefBuilder ().WriteTnefVersion ().ToArray (), 48));
			yield return new MalformedCase ("embedded-bad-signature", EmbeddedMessage (new TnefBuilder (0).WriteTnefVersion ().ToArray ()));
			yield return new MalformedCase ("embedded-empty", EmbeddedMessage (Array.Empty<byte> ()));
			yield return new MalformedCase ("embedded-truncated", EmbeddedMessage (new byte[] { 0x78, 0x9f, 0x3e }));

			#endregion

			#region Degenerate repetition

			var many = new TnefBuilder ();

			for (int i = 0; i < 2000; i++)
				many.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, TnefBuilder.Int32Payload (0x10000), checksum: 0);

			yield return new MalformedCase ("thousands-of-bad-checksums", many.ToArray ());

			#endregion
		}

		static TnefMapiPropertyBuilder Subject ()
		{
			return new TnefMapiPropertyBuilder ().WriteStringProperty (SubjectTag, "subject");
		}

		#endregion

		#region Tests

		static readonly MalformedCase[] Cases = MalformedCases ().ToArray ();

		public static IEnumerable<TestCaseData> LooseCases ()
		{
			foreach (var test in Cases) {
				foreach (var strategy in Strategies)
					yield return new TestCaseData (test, strategy).SetArgDisplayNames (test.Name, strategy.ToString ());
			}
		}

		[TestCaseSource (nameof (LooseCases))]
		public void TestLooseModeUpholdsInvariants (MalformedCase test, DriveStrategy strategy)
		{
			AssertInvariants (strategy, test.Data, TnefComplianceMode.Loose, test.Name);
		}

		public static IEnumerable<TestCaseData> StrictCases ()
		{
			foreach (var test in Cases) {
				// TnefPart always reads in Loose mode, so it is not a Strict mode scenario.
				foreach (var strategy in Strategies.Where (s => s != DriveStrategy.ConvertToMessage))
					yield return new TestCaseData (test, strategy).SetArgDisplayNames (test.Name, strategy.ToString ());
			}
		}

		[TestCaseSource (nameof (StrictCases))]
		public void TestStrictModeUpholdsInvariants (MalformedCase test, DriveStrategy strategy)
		{
			AssertInvariants (strategy, test.Data, TnefComplianceMode.Strict, test.Name);
		}

		[Test]
		public void TestWellFormedStreamIsCompliant ()
		{
			using (var stream = new MemoryStream (WellFormed (), false)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					DriveProperties (reader);

					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant));
				}
			}
		}

		#endregion
	}
}
