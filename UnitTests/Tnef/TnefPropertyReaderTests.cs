//
// TnefPropertyReaderTests.cs
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

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertyReaderTests
	{
		static byte[] CreateTnefStream (int declaredValueLength, int? declaredAttributeLength = null)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WritePropertyHeader (TnefPropertyTag.AttachDataBin);
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 1, 2, 3, 4 }, declaredValueLength, false);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, length: declaredAttributeLength);

			return builder.ToArray ();
		}

		static TestTnefComplianceLogger ReadMalformedValue (byte[] tnef, out byte[] bytes)
		{
			var logger = new TestTnefComplianceLogger ();

			bytes = Array.Empty<byte> ();

			using var reader = new TnefReader (new MemoryStream (tnef, false)) { ComplianceLogger = logger };

			while (reader.Read ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var prop = reader.GetPropertyReader ();

				while (prop.ReadNextProperty ()) {
					if (prop.ValueCount > 0)
						bytes = prop.ReadValueAsBytes ();
				}
			}

			return logger;
		}

		static async Task<TestTnefComplianceLogger> ReadMalformedValueAsync (byte[] tnef)
		{
			var logger = new TestTnefComplianceLogger ();

			using var reader = new TnefReader (new MemoryStream (tnef, false)) { ComplianceLogger = logger };

			while (await reader.ReadAsync ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await prop.ReadNextPropertyAsync ()) {
					if (prop.ValueCount > 0)
						await prop.ReadValueAsBytesAsync ();
				}
			}

			return logger;
		}

		[TestCase (int.MaxValue)]
		[TestCase (int.MaxValue - 3)]
		[TestCase (0x40000000)]
		[TestCase (1024 * 1024)]
		[TestCase (-1)]
		[TestCase (int.MinValue)]
		public void TestInvalidValueLengthIsRejected (int declaredLength)
		{
			var tnef = CreateTnefStream (declaredLength);
			var logger = ReadMalformedValue (tnef, out var bytes);

			Assert.That (tnef.Length, Is.LessThan (128), "the crafted TNEF stream should be tiny");
			Assert.That (bytes.Length, Is.LessThanOrEqualTo (tnef.Length), "allocated buffer is bounded by input size");
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyLength));
		}

		[TestCase (int.MaxValue)]
		[TestCase (int.MaxValue - 3)]
		[TestCase (0x40000000)]
		[TestCase (1024 * 1024)]
		[TestCase (-1)]
		[TestCase (int.MinValue)]
		public async Task TestInvalidValueLengthIsRejectedAsync (int declaredLength)
		{
			var tnef = CreateTnefStream (declaredLength);
			var logger = await ReadMalformedValueAsync (tnef);

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.InvalidPropertyLength));
		}

		[Test]
		public void TestOversizedAttributeLengthDoesNotAllocate ()
		{
			var tnef = CreateTnefStream (0x7FFFFF00, 0x7FFFFFFF);
			var logger = ReadMalformedValue (tnef, out var bytes);

			Assert.That (bytes.Length, Is.LessThanOrEqualTo (tnef.Length), "allocated buffer is bounded by input size");
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public async Task TestOversizedAttributeLengthDoesNotAllocateAsync ()
		{
			var tnef = CreateTnefStream (0x7FFFFF00, 0x7FFFFFFF);
			var logger = await ReadMalformedValueAsync (tnef);

			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[TestCase (TnefPropertyType.Unicode)]
		[TestCase (TnefPropertyType.String8)]
		public void TestOversizedStringLengthDoesNotAllocate (TnefPropertyType type)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.AttachTransportName, type));
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 0x41, 0x00, 0x42, 0x00 }, 0x7FFFFF00, false);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, length: 0x7FFFFFFF);

			// Note: Lift the limits so that the bogus length is not rejected up front and the chunked read path is exercised.
			var options = new TnefOptions { MaxPropertyValueLength = int.MaxValue, MaxTotalDataBytes = long.MaxValue };
			using var reader = new TnefReader (builder.ToStream (), options) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.True);
			var prop = reader.GetPropertyReader ();
			Assert.That (prop.ReadNextProperty (), Is.True);
			Assert.DoesNotThrow (() => _ = prop.ReadValueAsString ());
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[TestCase (TnefPropertyType.Unicode)]
		[TestCase (TnefPropertyType.String8)]
		public async Task TestOversizedStringLengthDoesNotAllocateAsync (TnefPropertyType type)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.AttachTransportName, type));
			properties.WriteValueCount (1);
			properties.WriteVariableLengthValue (new byte[] { 0x41, 0x00, 0x42, 0x00 }, 0x7FFFFF00, false);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, length: 0x7FFFFFFF);

			// Note: Lift the limits so that the bogus length is not rejected up front and the chunked read path is exercised.
			var options = new TnefOptions { MaxPropertyValueLength = int.MaxValue, MaxTotalDataBytes = long.MaxValue };
			using var reader = new TnefReader (builder.ToStream (), options) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.True);
			var prop = reader.GetPropertyReader ();
			Assert.That (await prop.ReadNextPropertyAsync (), Is.True);
			await prop.ReadValueAsStringAsync ();
			Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public void TestGetPropertyReaderReturnsSameInstance ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True);
			var first = reader.GetPropertyReader ();
			Assert.That (reader.GetPropertyReader (), Is.SameAs (first));
		}

		[Test]
		public async Task TestGetPropertyReaderReturnsSameInstanceAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True);
			var first = reader.GetPropertyReader ();
			Assert.That (reader.GetPropertyReader (), Is.SameAs (first));
		}

		[Test]
		public void TestGetPropertyReaderOnNonPropertyAttributeThrows ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True);
			Assert.Throws<InvalidOperationException> (() => reader.GetPropertyReader ());
		}

		[Test]
		public async Task TestGetPropertyReaderOnNonPropertyAttributeThrowsAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True);
			Assert.Throws<InvalidOperationException> (() => reader.GetPropertyReader ());
		}

		[Test]
		public void TestPropertyReaderThrowsAfterReaderAdvances ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);
			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True);
			var prop = reader.GetPropertyReader ();
			Assert.That (prop.ReadNextProperty (), Is.True);
			Assert.That (reader.Read (), Is.True);

			Assert.Throws<InvalidOperationException> (() => _ = prop.Tag);
			Assert.Throws<InvalidOperationException> (() => _ = prop.PropertyCount);
			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsInt32 ());
			Assert.Throws<InvalidOperationException> (() => prop.ReadNextProperty ());
		}

		[Test]
		public async Task TestPropertyReaderThrowsAfterReaderAdvancesAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteInt32Property (TnefPropertyTag.Importance, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);
			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True);
			var prop = reader.GetPropertyReader ();
			Assert.That (await prop.ReadNextPropertyAsync (), Is.True);
			Assert.That (await reader.ReadAsync (), Is.True);

			Assert.Throws<InvalidOperationException> (() => _ = prop.Tag);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await prop.ReadNextPropertyAsync ());
		}

		[Test]
		public void TestConvertToMessageWithOversizedLength ()
		{
			var tnef = CreateTnefStream (int.MaxValue);
			var part = new TnefPart { Content = new MimeContent (new MemoryStream (tnef, false)) };

			Assert.DoesNotThrow (() => {
				using var message = ConvertToMessage (part);
				Assert.That (message, Is.Not.Null);
			});
		}

		[Test]
		public void TestLoadTnefMessageWithOversizedLength ()
		{
			var tnef = CreateTnefStream (int.MaxValue);
			var part = new TnefPart { Content = new MimeContent (new MemoryStream (tnef, false)) };

			Assert.DoesNotThrow (() => part.LoadTnefMessage ().Dispose ());
		}

		static MimeMessage ConvertToMessage (TnefPart part) => TnefConversionTestHelper.Convert (part);
	}
}