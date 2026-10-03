//
// TnefPropertyValueCountTests.cs
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
	public class TnefPropertyValueCountTests
	{
		static MemoryStream BuildPropertyStream (TnefMapiPropertyBuilder properties)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder.ToStream ();
		}

		static TnefPropertyReader ReadFirstProperty (MemoryStream stream, TestTnefComplianceLogger logger)
		{
			var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (reader.Read ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var prop = reader.GetPropertyReader ();

				Assert.That (prop.ReadNextProperty (), Is.True, "ReadNextProperty");

				return prop;
			}

			reader.Dispose ();
			throw new InvalidOperationException ("Failed to locate the MAPI properties attribute.");
		}

		static async Task<TnefPropertyReader> ReadFirstPropertyAsync (MemoryStream stream, TestTnefComplianceLogger logger)
		{
			var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (await reader.ReadAsync ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var prop = reader.GetPropertyReader ();

				Assert.That (await prop.ReadNextPropertyAsync (), Is.True, "ReadNextPropertyAsync");

				return prop;
			}

			reader.Dispose ();
			throw new InvalidOperationException ("Failed to locate the MAPI properties attribute.");
		}

		[Test]
		public void TestSingleValuedPropertyWithZeroValueCountIsReportedButHasNoValue ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode));
			properties.WriteValueCount (0);

			using var stream = BuildPropertyStream (properties);
			var prop = ReadFirstProperty (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (0), "ValueCount");
			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsString ());
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidValueCount));
		}

		[Test]
		public async Task TestSingleValuedPropertyWithZeroValueCountIsReportedButHasNoValueAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode));
			properties.WriteValueCount (0);

			using var stream = BuildPropertyStream (properties);
			var prop = await ReadFirstPropertyAsync (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (0), "ValueCount");
			Assert.ThrowsAsync<InvalidOperationException> (async () => await prop.ReadValueAsStringAsync ());
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidValueCount));
		}

		[Test]
		public void TestMultiValuedPropertyWithZeroValueCountDoesNotHideNextProperty ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var multiTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Long | TnefPropertyType.MultiValued);
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (multiTag);
			properties.WriteValueCount (0);
			properties.WriteInt32Property (TnefPropertyTag.Importance, 7);

			using var stream = BuildPropertyStream (properties);
			var prop = ReadFirstProperty (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (0), "ValueCount");
			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt32 ());
			Assert.That (prop.ReadNextProperty (), Is.True, "ReadNextProperty");
			Assert.That (prop.Tag, Is.EqualTo (TnefPropertyTag.Importance), "Tag");
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (7), "Value");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public async Task TestMultiValuedPropertyWithZeroValueCountDoesNotHideNextPropertyAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var multiTag = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Long | TnefPropertyType.MultiValued);
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (multiTag);
			properties.WriteValueCount (0);
			properties.WriteInt32Property (TnefPropertyTag.Importance, 7);

			using var stream = BuildPropertyStream (properties);
			var prop = await ReadFirstPropertyAsync (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (0), "ValueCount");
			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt32 ());
			Assert.That (await prop.ReadNextPropertyAsync (), Is.True, "ReadNextPropertyAsync");
			Assert.That (prop.Tag, Is.EqualTo (TnefPropertyTag.Importance), "Tag");
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (7), "Value");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public void TestSingleValuedPropertyWithOneValueIsUnaffected ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "This is the subject");

			using var stream = BuildPropertyStream (properties);
			var prop = ReadFirstProperty (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (1), "ValueCount");
			Assert.That (prop.ReadValueAsString (), Is.EqualTo ("This is the subject"), "ReadValueAsString");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}

		[Test]
		public async Task TestSingleValuedPropertyWithOneValueIsUnaffectedAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "This is the subject");

			using var stream = BuildPropertyStream (properties);
			var prop = await ReadFirstPropertyAsync (stream, logger);

			Assert.That (prop.ValueCount, Is.EqualTo (1), "ValueCount");
			Assert.That (await prop.ReadValueAsStringAsync (), Is.EqualTo ("This is the subject"), "ReadValueAsStringAsync");
			Assert.That (logger.Issues, Is.Empty, "Issues");
		}
	}
}