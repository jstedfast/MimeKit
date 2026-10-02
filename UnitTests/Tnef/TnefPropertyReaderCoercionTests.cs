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
		static TnefReader ReadSingleProperty (TnefMapiPropertyBuilder properties)
		{
			var builder = new TnefBuilder ();
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var reader = new TnefReader (builder.ToStream ());

			while (reader.Read ()) {
				if (reader.Tag == TnefAttributeTag.MapiProperties && reader.GetPropertyReader ().ReadNextProperty ())
					return reader;
			}

			reader.Dispose ();
			throw new InvalidOperationException ("Failed to locate the property.");
		}

		static async Task<TnefReader> ReadSinglePropertyAsync (TnefMapiPropertyBuilder properties)
		{
			var builder = new TnefBuilder ();
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var reader = new TnefReader (builder.ToStream ());

			while (await reader.ReadAsync ()) {
				if (reader.Tag == TnefAttributeTag.MapiProperties && await reader.GetPropertyReader ().ReadNextPropertyAsync ())
					return reader;
			}

			reader.Dispose ();
			throw new InvalidOperationException ("Failed to locate the property.");
		}

		static TnefReader ReadInt32Property (TnefPropertyTag tag, int value)
		{
			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (tag, value);
			return ReadSingleProperty (properties);
		}

		[Test]
		public void TestReadNegativeInt16Coercions ()
		{
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.I2);

			using var reader = ReadInt32Property (tag, 0xFFFF);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsInt16 (), Is.EqualTo ((short) -1));
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (-1));
			Assert.That (prop.ReadValueAsInt64 (), Is.EqualTo (-1L));
			Assert.That (prop.ReadValueAsDouble (), Is.EqualTo (-1.0));
			Assert.That (prop.ReadValueAsFloat (), Is.EqualTo (-1.0f));
			Assert.That (prop.ReadValue (), Is.EqualTo ((short) -1));
		}

		[Test]
		public async Task TestReadNegativeInt16CoercionsAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.I2);

			properties.WriteInt32Property (tag, 0xFFFF);

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (-1));
			Assert.That (await prop.ReadValueAsync (), Is.EqualTo ((short) -1));
		}

		[Test]
		public void TestReadBooleanWithHighByteSet ()
		{
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Boolean);

			using var reader = ReadInt32Property (tag, 0x0100);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsBoolean (), Is.True);
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (0x0100));
			Assert.That (prop.ReadValue (), Is.EqualTo (true));
		}

		[Test]
		public async Task TestReadBooleanWithHighByteSetAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Boolean);

			properties.WriteInt32Property (tag, 0x0100);

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsBoolean (), Is.True);
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (0x0100));
			Assert.That (await prop.ReadValueAsync (), Is.EqualTo (true));
		}

		[Test]
		public void TestReadFloatingPointValues ()
		{
			var doubleProperties = new TnefMapiPropertyBuilder ();
			var floatProperties = new TnefMapiPropertyBuilder ();

			doubleProperties.WriteDoubleProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Double), Math.PI);
			floatProperties.WriteProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.R4), BitConverter.GetBytes (1.5f));

			using var doubleReader = ReadSingleProperty (doubleProperties);
			using var floatReader = ReadSingleProperty (floatProperties);

			Assert.That (doubleReader.GetPropertyReader ().ReadValueAsDouble (), Is.EqualTo (Math.PI));
			Assert.That (floatReader.GetPropertyReader ().ReadValueAsFloat (), Is.EqualTo (1.5f));
		}

		[Test]
		public async Task TestReadFloatingPointValuesAsync ()
		{
			var doubleProperties = new TnefMapiPropertyBuilder ();
			var floatProperties = new TnefMapiPropertyBuilder ();

			doubleProperties.WriteDoubleProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Double), Math.PI);
			floatProperties.WriteProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.R4), BitConverter.GetBytes (1.5f));

			using var doubleReader = await ReadSinglePropertyAsync (doubleProperties);
			using var floatReader = await ReadSinglePropertyAsync (floatProperties);

			Assert.That (doubleReader.GetPropertyReader ().ReadValueAsDouble (), Is.EqualTo (Math.PI));
			Assert.That (floatReader.GetPropertyReader ().ReadValueAsFloat (), Is.EqualTo (1.5f));
		}

		[Test]
		public void TestReadSysTimeValueIsUtc ()
		{
			var expected = new DateTime (2024, 3, 17, 9, 45, 12, DateTimeKind.Utc);
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (tag, expected.ToFileTimeUtc ());

			using var reader = ReadSingleProperty (properties);
			var prop = reader.GetPropertyReader ();
			var value = prop.ReadValueAsDateTime ();

			Assert.That (value.Kind, Is.EqualTo (DateTimeKind.Utc), "Kind");
			Assert.That (value, Is.EqualTo (expected), "Value");
			Assert.That (prop.ReadValue (), Is.EqualTo (expected), "ReadValue");
		}

		[Test]
		public async Task TestReadSysTimeValueIsUtcAsync ()
		{
			var expected = new DateTime (2024, 3, 17, 9, 45, 12, DateTimeKind.Utc);
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime);
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (tag, expected.ToFileTimeUtc ());

			using var reader = await ReadSinglePropertyAsync (properties);
			var value = await reader.GetPropertyReader ().ReadValueAsync ();

			Assert.That (value, Is.InstanceOf<DateTime> ());
			Assert.That (((DateTime) value).Kind, Is.EqualTo (DateTimeKind.Utc), "Kind");
			Assert.That (value, Is.EqualTo (expected), "Value");
		}

		static void WithCurrency (long rawValue, Action<TnefPropertyReader> action)
		{
			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Currency), rawValue);

			using var reader = ReadSingleProperty (properties);
			action (reader.GetPropertyReader ());
		}

		[Test]
		public void TestReadCurrencyValue ()
		{
			WithCurrency (123456789, prop => Assert.That (prop.ReadValue (), Is.EqualTo (12345.6789m), "ReadValue"));
			WithCurrency (123456789, prop => Assert.That (prop.ReadValueAsDouble (), Is.EqualTo (12345.6789).Within (0.00001), "Double"));
			WithCurrency (123456789, prop => Assert.That (prop.ReadValueAsFloat (), Is.EqualTo (12345.6789f).Within (0.01f), "Float"));
			WithCurrency (123456789, prop => Assert.That (prop.ReadValueAsInt64 (), Is.EqualTo (12345), "Int64"));
			WithCurrency (-25000, prop => Assert.That (prop.ReadValue (), Is.EqualTo (-2.5m), "Negative"));
			WithCurrency (0, prop => Assert.That (prop.ReadValueAsBoolean (), Is.False, "Zero"));
		}

		[Test]
		public async Task TestReadCurrencyValueAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Currency), 123456789);

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (await prop.ReadValueAsync (), Is.EqualTo (12345.6789m), "ReadValueAsync");
		}

		[Test]
		public void TestVariableLengthValueCanOnlyBeReadOnce ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, new byte[] { 1, 2, 3, 4 });

			using var reader = ReadSingleProperty (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsBytes (), Is.EqualTo (new byte[] { 1, 2, 3, 4 }));
			Assert.That (prop.IsValueConsumed, Is.True);
			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsString ());
		}

		[Test]
		public async Task TestVariableLengthValueCanOnlyBeReadOnceAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, new byte[] { 1, 2, 3, 4 });

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (await prop.ReadValueAsBytesAsync (), Is.EqualTo (new byte[] { 1, 2, 3, 4 }));
			Assert.That (prop.IsValueConsumed, Is.True);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await prop.ReadValueAsStringAsync ());
		}

		[Test]
		public void TestFixedWidthValueCanBeReadRepeatedly ()
		{
			using var reader = ReadInt32Property (TnefPropertyTag.Importance, 7);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (7));
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (7));
			Assert.That (prop.IsValueConsumed, Is.False);
		}

		[Test]
		public async Task TestFixedWidthValueCanBeReadRepeatedlyAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 7);

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (7));
			Assert.That (await prop.ReadValueAsync (), Is.EqualTo (7));
			Assert.That (prop.IsValueConsumed, Is.False);
		}
	}
}