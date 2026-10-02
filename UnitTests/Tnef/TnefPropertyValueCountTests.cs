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
	/// <summary>
	/// Tests the handling of malformed MAPI property value counts.
	/// </summary>
	/// <remarks>
	/// When <see cref="TnefPropertyReader.ReadNextProperty"/> returns <see langword="true" />, the
	/// caller must always be able to read the value. A property with a value count of zero cannot
	/// satisfy that contract, so it must not be reported as an available property.
	/// </remarks>
	[TestFixture]
	public class TnefPropertyValueCountTests
	{
		static MemoryStream BuildProperty (TnefPropertyType type, int valueCount, int trailing = 32)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (TnefPropertyId.Subject, type));
			properties.WriteValueCount (valueCount);
			properties.WriteRaw (new byte[trailing]);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			return builder.ToStream ();
		}

		static TnefPropertyReader ReadFirstProperty (MemoryStream stream, TnefReader reader, out bool available)
		{
			available = false;

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
					continue;

				var prop = reader.TnefPropertyReader;

				available = prop.ReadNextProperty ();

				return prop;
			}

			throw new InvalidOperationException ("Failed to locate the MAPI properties attribute.");
		}

		[Test]
		public void TestSingleValuedPropertyWithZeroValueCountIsNotReported ()
		{
			using (var stream = BuildProperty (TnefPropertyType.Unicode, 0)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					ReadFirstProperty (stream, reader, out var available);

					Assert.That (available, Is.False, "ReadNextProperty");
					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidPropertyLength,
						Is.EqualTo (TnefComplianceStatus.InvalidPropertyLength), "ComplianceStatus");
				}
			}
		}

		[Test]
		public void TestSingleValuedPropertyWithZeroValueCountThrowsInStrictMode ()
		{
			using (var stream = BuildProperty (TnefPropertyType.Unicode, 0)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					Assert.Throws<TnefException> (() => ReadFirstProperty (stream, reader, out _));
				}
			}
		}

		[Test]
		public void TestSingleValuedPropertyWithExcessiveValueCountIsClamped ()
		{
			using (var stream = BuildProperty (TnefPropertyType.Unicode, 5)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					var prop = ReadFirstProperty (stream, reader, out var available);

					Assert.That (available, Is.True, "ReadNextProperty");
					Assert.That (prop.ValueCount, Is.EqualTo (1), "ValueCount");
					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidPropertyLength,
						Is.EqualTo (TnefComplianceStatus.InvalidPropertyLength), "ComplianceStatus");
				}
			}
		}

		[Test]
		public void TestMultiValuedPropertyWithZeroValueCountIsNotReported ()
		{
			using (var stream = BuildProperty (TnefPropertyType.Unicode | TnefPropertyType.MultiValued, 0)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					ReadFirstProperty (stream, reader, out var available);

					Assert.That (available, Is.False, "ReadNextProperty");
				}
			}
		}

		[Test]
		public void TestSingleValuedPropertyWithOneValueIsUnaffected ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "This is the subject");

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					var prop = ReadFirstProperty (stream, reader, out var available);

					Assert.That (available, Is.True, "ReadNextProperty");
					Assert.That (prop.ValueCount, Is.EqualTo (1), "ValueCount");
					Assert.That (prop.ReadValueAsString (), Is.EqualTo ("This is the subject"), "ReadValueAsString");
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
				}
			}
		}

		/// <summary>
		/// Whenever ReadNextProperty returns true, ReadValue must not throw.
		/// </summary>
		[Test]
		public void TestReadNextPropertyAlwaysYieldsAReadableValue ()
		{
			foreach (var test in TnefFuzzTests.MalformedCases ()) {
				using (var stream = new MemoryStream (test.Data, false)) {
					using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
						try {
							while (reader.ReadNextAttribute ()) {
								var prop = reader.TnefPropertyReader;

								while (prop.ReadNextProperty ()) {
									Assert.That (prop.ValueCount, Is.GreaterThan (0), $"{test.Name}: ValueCount");

									prop.ReadValue ();
								}
							}
						} catch (TnefException) {
							// Expected for malformed input.
						}
					}
				}
			}
		}
	}
}
