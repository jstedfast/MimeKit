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
		static MemoryStream BuildMessageProperties (TnefMapiPropertyBuilder properties, int? count = null)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, count);

			return builder.ToStream ();
		}

		static void DrainProperty (TnefPropertyReader prop)
		{
			if (prop.ValueCount == 0)
				return;

			do {
				prop.ReadValue ();
			} while (prop.ReadNextValue ());
		}

		static async Task DrainPropertyAsync (TnefPropertyReader prop)
		{
			if (prop.ValueCount == 0)
				return;

			do {
				await prop.ReadValueAsync ();
			} while (await prop.ReadNextValueAsync ());
		}

		static TestTnefComplianceLogger ReadAllValues (MemoryStream stream)
		{
			var logger = new TestTnefComplianceLogger ();

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (reader.Read ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties && reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (prop.ReadNextProperty ())
					DrainProperty (prop);
			}

			return logger;
		}

		static async Task<TestTnefComplianceLogger> ReadAllValuesAsync (MemoryStream stream)
		{
			var logger = new TestTnefComplianceLogger ();

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (await reader.ReadAsync ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties && reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await prop.ReadNextPropertyAsync ())
					await DrainPropertyAsync (prop);
			}

			return logger;
		}

		[Test]
		public void TestUnspecifiedPropertyType ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unspecified));

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.UnsupportedPropertyType));
		}

		[Test]
		public async Task TestUnspecifiedPropertyTypeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unspecified));

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.UnsupportedPropertyType));
		}

		[Test]
		public void TestNullPropertyType ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Null));

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.No.Member (TnefComplianceViolation.UnsupportedPropertyType));
		}

		[Test]
		public async Task TestNullPropertyTypeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Null));

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.No.Member (TnefComplianceViolation.UnsupportedPropertyType));
		}

		[Test]
		public void TestOutOfRangeAppTime ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteDoubleProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.AppTime), 1e30);

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public async Task TestOutOfRangeAppTimeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteDoubleProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.AppTime), 1e30);

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public void TestOutOfRangeSysTime ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime), long.MaxValue);

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public async Task TestOutOfRangeSysTimeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime), long.MaxValue);

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public void TestNegativeSysTime ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime), -1);

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public async Task TestNegativeSysTimeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt64Property (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.SysTime), -1);

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidDate));
		}

		[Test]
		public void TestTruncatedClassIdValue ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.ClassId), new byte[8]);

			Assert.DoesNotThrow (() => ReadAllValues (BuildMessageProperties (properties)));
		}

		[Test]
		public async Task TestTruncatedClassIdValueAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteProperty (new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.ClassId), new byte[8]);

			await ReadAllValuesAsync (BuildMessageProperties (properties));
		}

		[Test]
		public void TestTruncatedNamedPropertyGuid ()
		{
			var builder = new TnefBuilder ();
			var payload = new byte[] { 1, 0, 0, 0, 0x1f, 0, 0, 0x80, 0, 1, 2, 3 };

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, payload);

			Assert.DoesNotThrow (() => ReadAllValues (builder.ToStream ()));
		}

		[Test]
		public async Task TestTruncatedNamedPropertyGuidAsync ()
		{
			var builder = new TnefBuilder ();
			var payload = new byte[] { 1, 0, 0, 0, 0x1f, 0, 0, 0x80, 0, 1, 2, 3 };

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, payload);

			await ReadAllValuesAsync (builder.ToStream ());
		}

		[Test]
		public void TestTruncatedDateAttribute ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, new byte[] { 0xe9, 0x07, 0x01, 0x00 });
			builder.WriteMessageClass ("IPM.Note");

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.True);
			Assert.That (reader.Read (), Is.True);
			Assert.DoesNotThrow (() => reader.ReadValueAsDateTime ());
			Assert.That (reader.Read (), Is.True, "next attribute");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MessageClass));
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidAttributeValue));
		}

		[Test]
		public async Task TestTruncatedDateAttributeAsync ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, new byte[] { 0xe9, 0x07, 0x01, 0x00 });
			builder.WriteMessageClass ("IPM.Note");

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.True);
			Assert.That (await reader.ReadAsync (), Is.True);
			await reader.ReadValueAsDateTimeAsync ();
			Assert.That (await reader.ReadAsync (), Is.True, "next attribute");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MessageClass));
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidAttributeValue));
		}

		[Test]
		public void TestOverflowingPropertyCount ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			var logger = ReadAllValues (builder.ToStream ());

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyCount));
		}

		[Test]
		public async Task TestOverflowingPropertyCountAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			var logger = await ReadAllValuesAsync (builder.ToStream ());

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyCount));
		}

		[Test]
		public void TestOverflowingValueCount ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unicode | TnefPropertyType.MultiValued);

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (int.MaxValue);

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidValueCount));
		}

		[Test]
		public async Task TestOverflowingValueCountAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unicode | TnefPropertyType.MultiValued);

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (int.MaxValue);

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidValueCount));
		}

		[Test]
		public void TestOverflowingRowCount ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.DoesNotThrow (() => {
				while (reader.Read ()) {
					if (reader.Tag != TnefAttributeTag.RecipientTable)
						continue;

					var prop = reader.GetPropertyReader ();

					while (prop.ReadNextRow ()) {
						while (prop.ReadNextProperty ())
							DrainProperty (prop);
					}
				}
			});

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidRowCount));
		}

		[Test]
		public async Task TestOverflowingRowCountAsync ()
		{
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RecipientTable, new byte[] { 0xff, 0xff, 0xff, 0x7f });

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			while (await reader.ReadAsync ()) {
				if (reader.Tag != TnefAttributeTag.RecipientTable)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await prop.ReadNextRowAsync ()) {
					while (await prop.ReadNextPropertyAsync ())
						await DrainPropertyAsync (prop);
				}
			}

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidRowCount));
		}

		[Test]
		public void TestPropertyValueLongerThanAttribute ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (TnefPropertyTag.AttachDataBin);
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 1, 2, 3, 4 }, 1024);

			var logger = ReadAllValues (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyLength));
		}

		[Test]
		public async Task TestPropertyValueLongerThanAttributeAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (TnefPropertyTag.AttachDataBin);
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 1, 2, 3, 4 }, 1024);

			var logger = await ReadAllValuesAsync (BuildMessageProperties (properties));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyLength));
		}

		[Test]
		public void TestTruncatedPropertyHeaderIsReportedAsTruncated ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, 2);

			var logger = ReadAllValues (builder.ToStream (builder.ToArray ().Length - 2));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public async Task TestTruncatedPropertyHeaderIsReportedAsTruncatedAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, 2);

			var logger = await ReadAllValuesAsync (builder.ToStream (builder.ToArray ().Length - 2));

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public void TestTruncatedMultiValuedPropertyDoesNotThrow ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unicode | TnefPropertyType.MultiValued);

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (2);
			properties.WriteUnicodeValue ("hi");
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			Assert.DoesNotThrow (() => ReadAllValues (builder.ToStream (builder.ToArray ().Length - 2)));
		}

		[Test]
		public async Task TestTruncatedMultiValuedPropertyDoesNotThrowAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unicode | TnefPropertyType.MultiValued);

			properties.WritePropertyHeader (tag);
			properties.WriteValueCount (2);
			properties.WriteUnicodeValue ("hi");
			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			await ReadAllValuesAsync (builder.ToStream (builder.ToArray ().Length - 2));
		}
	}
}