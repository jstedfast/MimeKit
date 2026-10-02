//
// TnefPropertyReaderCoercionTests.cs
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
	public class TnefPropertyReaderCoercionTests
	{
		static readonly TnefPropertyTag Int16Tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.I2);
		static readonly TnefPropertyTag BooleanTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Boolean);
		static readonly TnefPropertyTag DoubleTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Double);
		static readonly TnefPropertyTag FloatTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.R4);
		static readonly TnefPropertyTag SysTimeTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime);

		/// <summary>
		/// Build a TNEF stream containing a single message-level MAPI property with the specified
		/// 32-bit raw value and return a property reader positioned on that property.
		/// </summary>
		static TnefReader ReadSingleProperty (TnefPropertyTag tag, int rawValue)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (tag, rawValue);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var reader = new TnefReader (builder.ToStream (), 0, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
					continue;

				if (reader.TnefPropertyReader.ReadNextProperty ())
					return reader;
			}

			reader.Dispose ();

			throw new InvalidOperationException ("Failed to locate the property.");
		}

		[Test]
		public void TestReadNegativeInt16AsInt32 ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValueAsInt32 (), Is.EqualTo (-1));
		}

		[Test]
		public void TestReadNegativeInt16AsInt64 ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValueAsInt64 (), Is.EqualTo (-1L));
		}

		[Test]
		public void TestReadNegativeInt16AsInt16 ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValueAsInt16 (), Is.EqualTo ((short) -1));
		}

		[Test]
		public void TestReadNegativeInt16AsDouble ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValueAsDouble (), Is.EqualTo (-1.0));
		}

		[Test]
		public void TestReadNegativeInt16AsFloat ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValueAsFloat (), Is.EqualTo (-1.0f));
		}

		[Test]
		public void TestReadNegativeInt16AsValue ()
		{
			using var reader = ReadSingleProperty (Int16Tag, 0xFFFF);

			Assert.That (reader.TnefPropertyReader.ReadValue (), Is.EqualTo ((short) -1));
		}

		[Test]
		public void TestReadBooleanWithHighByteSet ()
		{
			// PT_BOOLEAN values are 16 bits wide on the wire, so masking off all but the low 8 bits
			// turns a non-zero value such as 0x0100 into false.
			using var reader = ReadSingleProperty (BooleanTag, 0x0100);

			Assert.That (reader.TnefPropertyReader.ReadValueAsBoolean (), Is.True);
		}

		[Test]
		public void TestReadBooleanWithHighByteSetAsInt32 ()
		{
			using var reader = ReadSingleProperty (BooleanTag, 0x0100);

			Assert.That (reader.TnefPropertyReader.ReadValueAsInt32 (), Is.EqualTo (0x0100));
		}

		[Test]
		public void TestReadBooleanWithHighByteSetAsValue ()
		{
			using var reader = ReadSingleProperty (BooleanTag, 0x0100);

			Assert.That (reader.TnefPropertyReader.ReadValue (), Is.EqualTo (true));
		}

		[Test]
		public void TestReadAttributeValueAsBytesThrowsInvalidOperationException ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream (), 0, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.TnefVersion)
					continue;

				var prop = reader.TnefPropertyReader;

				Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsBytes ());
				return;
			}

			Assert.Fail ("Failed to locate the attTnefVersion attribute.");
		}

		static TnefReader ReadSingleProperty (TnefMapiPropertyBuilder properties)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var reader = new TnefReader (builder.ToStream (), 0, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
					continue;

				if (reader.TnefPropertyReader.ReadNextProperty ())
					return reader;
			}

			reader.Dispose ();

			throw new InvalidOperationException ("Failed to locate the property.");
		}

		[Test]
		public void TestReadDoubleValue ()
		{
			// PT_DOUBLE is stored in little-endian byte order on the wire.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteDoubleProperty (DoubleTag, Math.PI);

			using var reader = ReadSingleProperty (properties);

			Assert.That (reader.TnefPropertyReader.ReadValueAsDouble (), Is.EqualTo (Math.PI));
		}

		[Test]
		public void TestReadFloatValue ()
		{
			// PT_R4 is stored in little-endian byte order on the wire.
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteProperty (FloatTag, BitConverter.GetBytes (1.5f));

			using var reader = ReadSingleProperty (properties);

			Assert.That (reader.TnefPropertyReader.ReadValueAsFloat (), Is.EqualTo (1.5f));
		}

		[Test]
		public void TestReadSysTimeValueIsUtc ()
		{
			// PT_SYSTIME is a FILETIME, which [MS-OXCDATA] defines as the number of 100-nanosecond
			// intervals since January 1, 1601 *in Coordinated Universal Time*, so the decoded value
			// must not be shifted into the local timezone of whatever host happens to be parsing
			// the stream.
			var expected = new DateTime (2024, 3, 17, 9, 45, 12, DateTimeKind.Utc);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (SysTimeTag, expected.ToFileTimeUtc ());

			using var reader = ReadSingleProperty (properties);

			var value = reader.TnefPropertyReader.ReadValueAsDateTime ();

			Assert.That (value.Kind, Is.EqualTo (DateTimeKind.Utc), "Kind");
			Assert.That (value, Is.EqualTo (expected), "Value");
		}

		[Test]
		public void TestReadSysTimeValueAsValueIsUtc ()
		{
			var expected = new DateTime (2024, 3, 17, 9, 45, 12, DateTimeKind.Utc);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (SysTimeTag, expected.ToFileTimeUtc ());

			using var reader = ReadSingleProperty (properties);

			var value = reader.TnefPropertyReader.ReadValue ();

			Assert.That (value, Is.InstanceOf<DateTime> ());
			Assert.That (((DateTime) value).Kind, Is.EqualTo (DateTimeKind.Utc), "Kind");
			Assert.That (value, Is.EqualTo (expected), "Value");
		}

		/// <summary>
		/// Build a TNEF stream containing a single message-level attribute with the specified raw
		/// value and return a reader positioned on that attribute.
		/// </summary>
		static TnefReader ReadSingleAttribute (TnefAttributeTag tag, byte[] payload)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, tag, payload);

			var reader = new TnefReader (builder.ToStream (), 0, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag == tag)
					return reader;
			}

			reader.Dispose ();

			throw new InvalidOperationException ("Failed to locate the attribute.");
		}

		/// <summary>
		/// Invoke an accessor on a freshly positioned attribute. An attribute value may only be
		/// read once, so each accessor needs its own reader.
		/// </summary>
		static void WithAttribute (TnefAttributeTag tag, byte[] payload, Action<TnefPropertyReader> action)
		{
			using var reader = ReadSingleAttribute (tag, payload);

			action (reader.TnefPropertyReader);
		}

		static void AssertNotReadableAs (TnefAttributeTag tag, byte[] payload, Action<TnefPropertyReader> action, string message)
		{
			WithAttribute (tag, payload, prop => Assert.Throws<InvalidOperationException> (() => action (prop), message));
		}

		[Test]
		public void TestAttributeShortCoercions ()
		{
			var payload = BitConverter.GetBytes ((short) -3);

			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ValueType, Is.EqualTo (typeof (short)), "ValueType"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsInt16 (), Is.EqualTo (-3), "Int16"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (-3), "Int32"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsInt64 (), Is.EqualTo (-3), "Int64"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsDouble (), Is.EqualTo (-3.0), "Double"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsFloat (), Is.EqualTo (-3.0f), "Float"));
			WithAttribute (TnefAttributeTag.Priority, payload, prop => Assert.That (prop.ReadValueAsBoolean (), Is.True, "Boolean"));

			AssertNotReadableAs (TnefAttributeTag.Priority, payload, prop => prop.ReadValueAsString (), "String");
			AssertNotReadableAs (TnefAttributeTag.Priority, payload, prop => prop.ReadValueAsBytes (), "Bytes");
			AssertNotReadableAs (TnefAttributeTag.Priority, payload, prop => prop.ReadValueAsDateTime (), "DateTime");
			AssertNotReadableAs (TnefAttributeTag.Priority, payload, prop => prop.ReadValueAsGuid (), "Guid");
		}

		[Test]
		public void TestAttributeLongCoercions ()
		{
			var payload = BitConverter.GetBytes (1234567);

			WithAttribute (TnefAttributeTag.AidOwner, payload, prop => Assert.That (prop.ValueType, Is.EqualTo (typeof (int)), "ValueType"));
			WithAttribute (TnefAttributeTag.AidOwner, payload, prop => Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (1234567), "Int32"));
			WithAttribute (TnefAttributeTag.AidOwner, payload, prop => Assert.That (prop.ReadValueAsInt64 (), Is.EqualTo (1234567), "Int64"));
			WithAttribute (TnefAttributeTag.AidOwner, payload, prop => Assert.That (prop.ReadValueAsDouble (), Is.EqualTo (1234567.0), "Double"));

			AssertNotReadableAs (TnefAttributeTag.AidOwner, payload, prop => prop.ReadValueAsString (), "String");
			AssertNotReadableAs (TnefAttributeTag.AidOwner, payload, prop => prop.ReadValueAsBytes (), "Bytes");
			AssertNotReadableAs (TnefAttributeTag.AidOwner, payload, prop => prop.ReadValueAsDateTime (), "DateTime");
			AssertNotReadableAs (TnefAttributeTag.AidOwner, payload, prop => prop.ReadValueAsGuid (), "Guid");
		}

		[Test]
		public void TestAttributeStringCoercions ()
		{
			var payload = "This is the subject"u8.ToArray ();

			WithAttribute (TnefAttributeTag.Subject, payload, prop => Assert.That (prop.ValueType, Is.EqualTo (typeof (string)), "ValueType"));
			WithAttribute (TnefAttributeTag.Subject, payload, prop => Assert.That (prop.ReadValueAsString (), Is.EqualTo ("This is the subject"), "String"));
			WithAttribute (TnefAttributeTag.Subject, payload, prop => Assert.That (prop.ReadValueAsBytes (), Is.EqualTo (payload), "Bytes"));

			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsInt16 (), "Int16");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsInt32 (), "Int32");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsInt64 (), "Int64");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsDouble (), "Double");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsFloat (), "Float");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsBoolean (), "Boolean");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsDateTime (), "DateTime");
			AssertNotReadableAs (TnefAttributeTag.Subject, payload, prop => prop.ReadValueAsGuid (), "Guid");
		}

		[Test]
		public void TestAttributeDateCoercions ()
		{
			var expected = new DateTime (2024, 3, 17, 9, 45, 12, DateTimeKind.Utc);
			var payload = new byte[14];
			int index = 0;

			foreach (var component in new short[] { 2024, 3, 17, 9, 45, 12, (short) expected.DayOfWeek }) {
				BitConverter.GetBytes (component).CopyTo (payload, index);
				index += 2;
			}

			WithAttribute (TnefAttributeTag.DateSent, payload, prop => Assert.That (prop.ValueType, Is.EqualTo (typeof (DateTime)), "ValueType"));
			WithAttribute (TnefAttributeTag.DateSent, payload, prop => Assert.That (prop.ReadValueAsDateTime (), Is.EqualTo (expected), "DateTime"));

			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsInt32 (), "Int32");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsInt64 (), "Int64");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsDouble (), "Double");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsBoolean (), "Boolean");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsString (), "String");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsBytes (), "Bytes");
			AssertNotReadableAs (TnefAttributeTag.DateSent, payload, prop => prop.ReadValueAsGuid (), "Guid");
		}

		[Test]
		public void TestAttributeByteCoercions ()
		{
			// Note: atpByte attributes are opaque blobs with their own internal structure, so they
			// may only be read as a byte array (or as a string, consistent with PT_BINARY).
			var payload = new byte[] { 1, 2, 3, 4 };

			WithAttribute (TnefAttributeTag.Owner, payload, prop => Assert.That (prop.ValueType, Is.EqualTo (typeof (byte[])), "ValueType"));
			WithAttribute (TnefAttributeTag.Owner, payload, prop => Assert.That (prop.ReadValueAsBytes (), Is.EqualTo (payload), "Bytes"));

			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsInt16 (), "Int16");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsInt32 (), "Int32");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsInt64 (), "Int64");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsDouble (), "Double");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsFloat (), "Float");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsBoolean (), "Boolean");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsDateTime (), "DateTime");
			AssertNotReadableAs (TnefAttributeTag.Owner, payload, prop => prop.ReadValueAsGuid (), "Guid");
		}
	}
}
