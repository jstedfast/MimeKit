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
	}
}
