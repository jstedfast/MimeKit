//
// TnefPropertyReaderRobustnessTests.cs
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
	public class TnefPropertyReaderRobustnessTests
	{
		static readonly TnefPropertyTag AppTimeTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.AppTime);
		static readonly TnefPropertyTag SysTimeTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime);
		static readonly TnefPropertyTag ClassIdTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.ClassId);

		static MemoryStream BuildMessageProperties (TnefMapiPropertyBuilder properties, int? count = null, int? length = null)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, count, length);

			return builder.ToStream ();
		}

		static TnefComplianceStatus ReadAllValues (MemoryStream stream)
		{
			using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
				while (reader.ReadNextAttribute ()) {
					if (reader.AttributeTag != TnefAttributeTag.MapiProperties && reader.AttributeTag != TnefAttributeTag.Attachment)
						continue;

					var prop = reader.TnefPropertyReader;

					while (prop.ReadNextProperty ())
						prop.ReadValue ();
				}

				return reader.ComplianceStatus;
			}
		}

		// Enumerates the properties of the first attMsgProps attribute without reading any
		// of the values.
		static TnefComplianceStatus ReadPropertiesWithoutValues (MemoryStream stream)
		{
			using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
				while (reader.ReadNextAttribute ()) {
					if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
						continue;

					var prop = reader.TnefPropertyReader;

					while (prop.ReadNextProperty ())
						;

					break;
				}

				return reader.ComplianceStatus;
			}
		}

		// Reads only the properties of the first attMsgProps attribute and returns the
		// compliance status *before* the reader moves on to the next attribute.
		static TnefComplianceStatus ReadPropertiesOfFirstMapiAttribute (MemoryStream stream)
		{
			using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
				while (reader.ReadNextAttribute ()) {
					if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
						continue;

					var prop = reader.TnefPropertyReader;

					while (prop.ReadNextProperty ())
						prop.ReadValue ();

					break;
				}

				return reader.ComplianceStatus;
			}
		}

		[Test]
		public void TestUnspecifiedPropertyType ()
		{
			// PT_UNSPECIFIED is only meaningful in a property tag that is used to *request* a
			// property - the length of such a value is unknowable, so it cannot be skipped over.
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unspecified);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (tag);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadPropertiesWithoutValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.UnsupportedPropertyType, Is.EqualTo (TnefComplianceStatus.UnsupportedPropertyType), "ComplianceStatus");
		}

		[Test]
		public void TestNullPropertyType ()
		{
			// PT_NULL is a valid property type that simply has no value.
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Null);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (tag);

			TnefComplianceStatus status = TnefComplianceStatus.UnsupportedPropertyType;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.UnsupportedPropertyType, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
		}

		[Test]
		public void TestOutOfRangeAppTime ()
		{
			// DateTime.FromOADate() throws ArgumentException for values outside of the OLE Automation
			// date range; this must be reported as an InvalidDate compliance error instead.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteDoubleProperty (AppTimeTag, 1e30);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.InvalidDate, Is.EqualTo (TnefComplianceStatus.InvalidDate), "ComplianceStatus");
		}

		[Test]
		public void TestOutOfRangeSysTime ()
		{
			// DateTime.FromFileTime() throws ArgumentOutOfRangeException for FILETIME values that do
			// not map onto a valid DateTime.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (SysTimeTag, long.MaxValue);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.InvalidDate, Is.EqualTo (TnefComplianceStatus.InvalidDate), "ComplianceStatus");
		}

		[Test]
		public void TestNegativeSysTime ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (SysTimeTag, -1);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.InvalidDate, Is.EqualTo (TnefComplianceStatus.InvalidDate), "ComplianceStatus");
		}

		[Test]
		public void TestTruncatedClassIdValue ()
		{
			// new Guid (byte[]) throws ArgumentException when ReadBytes() clamps the request to the
			// number of bytes that actually remain in the attribute.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteProperty (ClassIdTag, new byte[8]);

			Assert.DoesNotThrow (() => ReadAllValues (BuildMessageProperties (properties)));
		}

		[Test]
		public void TestTruncatedNamedPropertyGuid ()
		{
			// LoadPropertyName() reads a 16-byte GUID; a truncated attribute must not throw.
			var builder = new TnefBuilder ();
			var payload = new byte[] {
				0x01, 0x00, 0x00, 0x00, // property count = 1
				0x1f, 0x00, 0x00, 0x80, // PT_UNICODE, id = 0x8000 (named)
				0x00, 0x01, 0x02, 0x03  // only 4 bytes of the 16-byte GUID
			};

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, payload);

			using (var stream = builder.ToStream ()) {
				Assert.DoesNotThrow (() => ReadAllValues (stream));
			}
		}

		[Test]
		public void TestTruncatedDateAttribute ()
		{
			// The TNEF date structure is 7 WORDs; a shorter attribute must not cause us to read
			// into the following attribute.
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, new byte[] { 0xe9, 0x07, 0x01, 0x00 });
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.DoesNotThrow (() => {
						while (reader.ReadNextAttribute ()) {
							if (reader.AttributeType == TnefAttributeType.Date)
								reader.TnefPropertyReader.ReadValue ();
						}
					});

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidDate, Is.EqualTo (TnefComplianceStatus.InvalidDate), "ComplianceStatus");
				}
			}
		}

		[Test]
		public void TestOverflowingPropertyCount ()
		{
			// Each property needs at least 4 bytes, so a property count of int.MaxValue cannot possibly
			// be honored. The reader must clamp it rather than spin.
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			using (var stream = builder.ToStream ()) {
				Assert.DoesNotThrow (() => status = ReadAllValues (stream));
			}

			Assert.That (status & TnefComplianceStatus.AttributeOverflow, Is.EqualTo (TnefComplianceStatus.AttributeOverflow), "ComplianceStatus");
		}

		[Test]
		public void TestOverflowingValueCount ()
		{
			// A multi-valued property with a value count of int.MaxValue cannot possibly be honored.
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, (TnefPropertyType) (0x1000 | (int) TnefPropertyType.Unicode));
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (int.MaxValue);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.AttributeOverflow, Is.EqualTo (TnefComplianceStatus.AttributeOverflow), "ComplianceStatus");
		}

		[Test]
		public void TestOverflowingRowCount ()
		{
			// Each recipient table row needs at least a 4-byte property count.
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			using (var stream = builder.ToStream ()) {
				Assert.DoesNotThrow (() => {
					using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
						while (reader.ReadNextAttribute ()) {
							if (reader.AttributeTag != TnefAttributeTag.RecipientTable)
								continue;

							var prop = reader.TnefPropertyReader;

							while (prop.ReadNextRow ()) {
								while (prop.ReadNextProperty ())
									prop.ReadValue ();
							}
						}

						status = reader.ComplianceStatus;
					}
				});
			}

			Assert.That (status & TnefComplianceStatus.AttributeOverflow, Is.EqualTo (TnefComplianceStatus.AttributeOverflow), "ComplianceStatus");
		}

		[Test]
		public void TestPropertyValueLongerThanAttribute ()
		{
			// The declared value length reaches beyond the end of the enclosing attribute, which is a
			// structural containment violation rather than a nonsensical length.
			var tag = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 1, 2, 3, 4 }, 1024);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;

			Assert.DoesNotThrow (() => status = ReadAllValues (BuildMessageProperties (properties)));
			Assert.That (status & TnefComplianceStatus.AttributeOverflow, Is.EqualTo (TnefComplianceStatus.AttributeOverflow), "ComplianceStatus");
		}

		[Test]
		public void TestTruncatedPropertyHeaderIsReportedAsTruncated ()
		{
			// ReadNextProperty() swallowed the EndOfStreamException without recording why it
			// stopped reading properties.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Long), 1);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, 2);

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;
			int length = builder.ToArray ().Length;

			using (var stream = builder.ToStream (length - 2)) {
				Assert.DoesNotThrow (() => status = ReadPropertiesOfFirstMapiAttribute (stream));
			}

			Assert.That (status & TnefComplianceStatus.StreamTruncated, Is.EqualTo (TnefComplianceStatus.StreamTruncated), "ComplianceStatus");
		}

		[Test]
		public void TestTruncatedMultiValuedPropertyDoesNotThrow ()
		{
			// Note: ReadNextValue() intentionally does *not* report StreamTruncated here --
			// hitting the end of the stream while looking for another value is how reading
			// normally terminates for real-world TNEF streams.
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, (TnefPropertyType) (0x1000 | (int) TnefPropertyType.Unicode));
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (2);
			properties.WriteUnicodeValue ("hi");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			int length = builder.ToArray ().Length;

			using (var stream = builder.ToStream (length - 2)) {
				Assert.DoesNotThrow (() => ReadPropertiesOfFirstMapiAttribute (stream));
			}
		}
	}
}
