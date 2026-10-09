//
// TnefPropertySetTests.cs
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

using System.Text;

using MimeKit.Tnef;

using UnitTests.IO;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertySetTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");
		static readonly Guid PublicStrings = new Guid ("00020329-0000-0000-C000-000000000046");
		static readonly TnefPropertyTag SubjectA = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8);
		static readonly TnefPropertyTag SubjectW = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);

		#region Helpers

		static Task<bool> ReadAsync (TnefReader reader, bool async)
		{
			return async ? reader.ReadAsync () : Task.FromResult (reader.Read ());
		}

		static Task<bool> ReadNextPropertyAsync (TnefPropertyReader prop, bool async)
		{
			return async ? prop.ReadNextPropertyAsync () : Task.FromResult (prop.ReadNextProperty ());
		}

		static Task<TnefProperty> ReadPropertyAsync (TnefPropertyReader prop, bool async)
		{
			return async ? prop.ReadPropertyAsync () : Task.FromResult (prop.ReadProperty ());
		}

		static Task<TnefPropertySet> ReadPropertySetAsync (TnefPropertyReader prop, bool async)
		{
			return async ? prop.ReadPropertySetAsync () : Task.FromResult (prop.ReadPropertySet ());
		}

		static Task<IReadOnlyList<TnefPropertySet>> ReadRowsAsync (TnefPropertyReader prop, bool async)
		{
			return async ? prop.ReadRowsAsPropertySetsAsync () : Task.FromResult (prop.ReadRowsAsPropertySets ());
		}

		static byte[] Int64Payload (long value)
		{
			var bytes = new byte[8];

			for (int i = 0; i < 8; i++)
				bytes[i] = (byte) ((value >> (i * 8)) & 0xFF);

			return bytes;
		}

		static byte[] Concat (params byte[][] arrays)
		{
			var stream = new MemoryStream ();

			foreach (var array in arrays)
				stream.Write (array, 0, array.Length);

			return stream.ToArray ();
		}

		static byte[] CreatePattern (int length)
		{
			var bytes = new byte[length];

			for (int i = 0; i < length; i++)
				bytes[i] = (byte) (i * 31 + (i >> 8));

			return bytes;
		}

		static Stream CreateStream (TnefBuilder builder, bool seekable)
		{
			var stream = builder.ToStream ();

			return seekable ? stream : new NonSeekableStream (stream);
		}

		// Reads the properties of the first attribute with the specified tag into a property set.
		static async Task<TnefPropertySet> ReadPropertySetAsync (Stream stream, TnefAttributeTag tag, bool async, TnefOptions options = null, TestTnefComplianceLogger logger = null)
		{
			using var reader = new TnefReader (stream, options) { ComplianceLogger = logger };

			while (await ReadAsync (reader, async)) {
				if (reader.Tag == tag)
					return await ReadPropertySetAsync (reader.GetPropertyReader (), async);
			}

			throw new InvalidOperationException ("Failed to locate the attribute.");
		}

		static IEnumerable<TnefComplianceViolation> Violations (TestTnefComplianceLogger logger)
		{
			return logger.Issues.Select (issue => issue.Violation);
		}

		#endregion

		#region Value types

		static TnefMapiPropertyBuilder CreateAllTypes (DateTime sysTime, DateTime appTime, Guid guid)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteProperty (new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.I2), TnefBuilder.Int32Payload (0x1234));
			properties.WriteInt32Property (TnefPropertyTag.Priority, 42);
			properties.WriteProperty (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.R4), BitConverter.GetBytes (1.5f));
			properties.WriteDoubleProperty (new TnefPropertyTag ((TnefPropertyId) 0x6602, TnefPropertyType.Double), 2.25);
			properties.WriteInt64Property (new TnefPropertyTag ((TnefPropertyId) 0x6603, TnefPropertyType.Currency), 123456789);
			properties.WriteDoubleProperty (new TnefPropertyTag ((TnefPropertyId) 0x6604, TnefPropertyType.AppTime), appTime.ToOADate ());
			properties.WriteInt32Property (new TnefPropertyTag ((TnefPropertyId) 0x6605, TnefPropertyType.Error), unchecked ((int) 0x8004010F));
			properties.WriteProperty (new TnefPropertyTag ((TnefPropertyId) 0x6606, TnefPropertyType.Boolean), TnefBuilder.Int32Payload (1));
			properties.WriteInt64Property (new TnefPropertyTag ((TnefPropertyId) 0x6607, TnefPropertyType.I8), 0x0123456789ABCDEF);
			properties.WriteInt64Property (TnefPropertyTag.ClientSubmitTime, sysTime.ToFileTimeUtc ());
			properties.WriteGuidProperty (new TnefPropertyTag ((TnefPropertyId) 0x6608, TnefPropertyType.ClassId), guid);
			properties.WriteStringProperty (SubjectA, "8-bit subject", Encoding.ASCII);
			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Unicode body");
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, new byte[] { 1, 2, 3, 4, 5 });

			return properties;
		}

		async Task RunTestValueTypesAsync (bool async)
		{
			var sysTime = new DateTime (2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
			var appTime = new DateTime (2023, 1, 2, 3, 4, 5);
			var guid = Guid.NewGuid ();
			var builder = new TnefBuilder ();

			builder.WriteMapiProperties (TnefAttributeLevel.Message, CreateAllTypes (sysTime, appTime, guid));

			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async);

			Assert.That (set, Has.Count.EqualTo (14));
			Assert.That (set.Select (p => p.Count), Is.All.EqualTo (1));
			Assert.That (set.Select (p => p.IsMultiValued), Is.All.False);

			Assert.That (set[0].Value, Is.EqualTo ((short) 0x1234));
			Assert.That (set[0].TryGetInt16 (out var i2) && i2 == 0x1234, Is.True);
			Assert.That (set[0].TryGetInt32 (out var i2as32) && i2as32 == 0x1234, Is.True);

			Assert.That (set.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (42));
			Assert.That (set[1].TryGetInt64 (out var l) && l == 42, Is.True);
			Assert.That (set[1].TryGetDouble (out var ld) && ld == 42, Is.True);
			Assert.That (set[1].TryGetBoolean (out var lb) && lb, Is.True);

			Assert.That (set[2].Value, Is.EqualTo (1.5f));
			Assert.That (set[2].TryGetFloat (out var f) && f == 1.5f, Is.True);
			Assert.That (set[2].TryGetDouble (out var fd) && fd == 1.5, Is.True);

			Assert.That (set[3].Value, Is.EqualTo (2.25));
			Assert.That (set[3].TryGetInt32 (out var di) && di == 2, Is.True);

			Assert.That (set[4].Value, Is.EqualTo (12345.6789m));
			Assert.That (set[4].TryGetDouble (out var cd) && cd == 12345.6789, Is.True);
			Assert.That (set[4].TryGetInt64 (out var cl) && cl == 12345, Is.True);

			Assert.That (set[5].TryGetDateTime (out var at), Is.True);
			Assert.That (at, Is.EqualTo (appTime));

			Assert.That (set[6].PropertyType, Is.EqualTo (TnefPropertyType.Error));
			Assert.That (set[6].Value, Is.EqualTo (unchecked ((int) 0x8004010F)));

			Assert.That (set[7].Value, Is.EqualTo (true));
			Assert.That (set[7].TryGetInt32 (out var bi) && bi == 1, Is.True);

			Assert.That (set[8].Value, Is.EqualTo (0x0123456789ABCDEF));

			Assert.That (set.GetDateTime (TnefPropertyTag.ClientSubmitTime), Is.EqualTo (sysTime));
			Assert.That (set.GetDateTime (TnefPropertyTag.ClientSubmitTime).Value.Kind, Is.EqualTo (DateTimeKind.Utc));

			Assert.That (set[10].TryGetGuid (out var g) && g == guid, Is.True);
			Assert.That (set[10].TryGetBytes (out var gb), Is.True);
			Assert.That (gb, Is.EqualTo (guid.ToByteArray ()));

			Assert.That (set.GetString (SubjectW), Is.EqualTo ("8-bit subject"), "String8 by Unicode tag");
			Assert.That (set.GetString (TnefPropertyTag.BodyW), Is.EqualTo ("Unicode body"));
			Assert.That (set.GetBytes (TnefPropertyTag.AttachDataBin), Is.EqualTo (new byte[] { 1, 2, 3, 4, 5 }));
			Assert.That (set[13].TryGetString (out var bs), Is.True, "binary as string");
			Assert.That (bs, Is.EqualTo ("\u0001\u0002\u0003\u0004\u0005"));

			// Mismatched conversions
			Assert.That (set[11].TryGetInt32 (out _), Is.False);
			Assert.That (set[11].TryGetDateTime (out _), Is.False);
			Assert.That (set[11].TryGetGuid (out _), Is.False);
			Assert.That (set[11].TryGetBytes (out _), Is.False);
			Assert.That (set[1].TryGetString (out _), Is.False);
			Assert.That (set.GetBoolean (TnefPropertyTag.BodyW), Is.Null);
			Assert.That (set.GetInt64 (TnefPropertyTag.AttachDataBin), Is.Null);
			Assert.That (set.GetGuid (TnefPropertyTag.Priority), Is.Null);
			Assert.That (set.GetString (TnefPropertyTag.Importance), Is.Null, "missing");
			Assert.That (set.TryGetValue (TnefPropertyTag.DisplayNameW, out _), Is.False);

			Assert.That (set.ToList (), Has.Count.EqualTo (14), "enumerator");
			Assert.That (set[1].ToString (), Does.Contain ("42"));
		}

		[Test]
		public Task TestValueTypes () => RunTestValueTypesAsync (false);

		[Test]
		public Task TestValueTypesAsync () => RunTestValueTypesAsync (true);

		async Task RunTestMultiValuedTypesAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.Long | TnefPropertyType.MultiValued));
			properties.WriteValueCount (3);
			properties.WriteRaw (Concat (TnefBuilder.Int32Payload (1), TnefBuilder.Int32Payload (2), TnefBuilder.Int32Payload (3)));

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6602, TnefPropertyType.I8 | TnefPropertyType.MultiValued));
			properties.WriteValueCount (2);
			properties.WriteRaw (Concat (Int64Payload (-1), Int64Payload (long.MaxValue)));

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6603, TnefPropertyType.Unicode | TnefPropertyType.MultiValued));
			properties.WriteValueCount (3);
			properties.WriteUnicodeValue ("one");
			properties.WriteUnicodeValue ("");
			properties.WriteUnicodeValue ("three");

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6604, TnefPropertyType.Binary | TnefPropertyType.MultiValued));
			properties.WriteValueCount (2);
			properties.WriteVariableLengthValue (new byte[] { 1 });
			properties.WriteVariableLengthValue (new byte[] { 2, 3, 4, 5, 6 });

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6605, TnefPropertyType.Long | TnefPropertyType.MultiValued));
			properties.WriteValueCount (0);

			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async);

			Assert.That (set, Has.Count.EqualTo (5));
			Assert.That (set.Select (p => p.IsMultiValued), Is.All.True);

			Assert.That (set[0].Count, Is.EqualTo (3));
			Assert.That (set[0].Value, Is.TypeOf<int[]> ());
			Assert.That (set[0].TryGetValues<int> (out var ints), Is.True);
			Assert.That (ints, Is.EqualTo (new[] { 1, 2, 3 }));
			Assert.That (set[0].TryGetValues<long> (out _), Is.False);
			Assert.That (set[0].TryGetInt32 (out _), Is.False, "scalar accessors reject multi-valued properties");

			Assert.That (set[1].TryGetValues<long> (out var longs), Is.True);
			Assert.That (longs, Is.EqualTo (new[] { -1L, long.MaxValue }));

			Assert.That (set[2].TryGetValues<string> (out var strings), Is.True);
			Assert.That (strings, Is.EqualTo (new[] { "one", "", "three" }));
			Assert.That (set[2].TryGetString (out _), Is.False);

			Assert.That (set[3].TryGetValues<byte[]> (out var blobs), Is.True);
			Assert.That (blobs, Has.Length.EqualTo (2));
			Assert.That (blobs[0], Is.EqualTo (new byte[] { 1 }));
			Assert.That (blobs[1], Is.EqualTo (new byte[] { 2, 3, 4, 5, 6 }));

			Assert.That (set[4].Count, Is.EqualTo (0));
			Assert.That (set[4].TryGetValues<int> (out var empty), Is.True);
			Assert.That (empty, Is.Empty);
		}

		[Test]
		public Task TestMultiValuedTypes () => RunTestMultiValuedTypesAsync (false);

		[Test]
		public Task TestMultiValuedTypesAsync () => RunTestMultiValuedTypesAsync (true);

		async Task RunTestSingleValuedTryGetValuesAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (SubjectW, "subject");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async);

			Assert.That (set[0].TryGetValues<string> (out var values), Is.True);
			Assert.That (values, Is.EqualTo (new[] { "subject" }));
			Assert.That (set[0].TryGetValues<int> (out _), Is.False);
		}

		[Test]
		public Task TestSingleValuedTryGetValues () => RunTestSingleValuedTryGetValuesAsync (false);

		[Test]
		public Task TestSingleValuedTryGetValuesAsync () => RunTestSingleValuedTryGetValuesAsync (true);

		async Task RunTestTruncatedMultiValuedPropertyIsTrimmedAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var logger = new TestTnefComplianceLogger ();

			// Note: Claim 4 values, but only provide 2 (each value is 8 bytes, so the declared count fits in the attribute).
			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.Unicode | TnefPropertyType.MultiValued));
			properties.WriteValueCount (4);
			properties.WriteUnicodeValue ("a");
			properties.WriteUnicodeValue ("b");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async, logger: logger);

			Assert.That (set, Has.Count.EqualTo (1));
			Assert.That (set[0].Count, Is.EqualTo (2));
			Assert.That (set[0].TryGetValues<string> (out var values), Is.True);
			Assert.That (values, Is.EqualTo (new[] { "a", "b" }));
			Assert.That (logger.Issues, Is.Not.Empty);
		}

		[Test]
		public Task TestTruncatedMultiValuedPropertyIsTrimmed () => RunTestTruncatedMultiValuedPropertyIsTrimmedAsync (false);

		[Test]
		public Task TestTruncatedMultiValuedPropertyIsTrimmedAsync () => RunTestTruncatedMultiValuedPropertyIsTrimmedAsync (true);

		#endregion

		#region Named properties

		async Task RunTestNamedPropertiesAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var guid = new Guid ("00062008-0000-0000-C000-000000000046");

			properties.WritePropertyHeader (new TnefPropertyTag (unchecked ((TnefPropertyId) 0x8001), TnefPropertyType.Unicode | TnefPropertyType.MultiValued), PublicStrings, name: "Keywords");
			properties.WriteValueCount (2);
			properties.WriteUnicodeValue ("red");
			properties.WriteUnicodeValue ("blue");

			properties.WritePropertyHeader (new TnefPropertyTag (unchecked ((TnefPropertyId) 0x8002), TnefPropertyType.Long), guid, nameId: 0x8501);
			properties.WriteRaw (TnefBuilder.Int32Payload (15));

			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async);

			Assert.That (set.TryGetValue (new TnefNameId (PublicStrings, "Keywords"), out var keywords), Is.True);
			Assert.That (keywords.Name, Is.EqualTo (new TnefNameId (PublicStrings, "Keywords")));
			Assert.That (keywords.TryGetValues<string> (out var values), Is.True);
			Assert.That (values, Is.EqualTo (new[] { "red", "blue" }));

			Assert.That (set.TryGetValue (new TnefNameId (guid, 0x8501), out var reminder), Is.True);
			Assert.That (reminder.TryGetInt32 (out var minutes) && minutes == 15, Is.True);
			Assert.That (reminder.ToString (), Does.Contain ("15"));

			Assert.That (set.TryGetValue (TnefNameId.Keywords, out _), Is.True, "TnefNameId.Keywords");
			Assert.That (set.TryGetValue (TnefNameId.ReminderDelta, out _), Is.True, "TnefNameId.ReminderDelta");
			Assert.That (set.TryGetValue (TnefNameId.ReminderTime, out _), Is.False, "TnefNameId.ReminderTime");

			Assert.That (set.TryGetValue (new TnefNameId (guid, 0x8502), out _), Is.False);
			Assert.That (set.TryGetValue (new TnefNameId (PublicStrings, "keywords"), out _), Is.False, "names are case-sensitive");
		}

		[Test]
		public Task TestNamedProperties () => RunTestNamedPropertiesAsync (false);

		[Test]
		public Task TestNamedPropertiesAsync () => RunTestNamedPropertiesAsync (true);

		#endregion

		#region Cursor interaction

		static TnefBuilder CreateSimpleMessage ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (SubjectW, "subject");
			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.Long | TnefPropertyType.MultiValued));
			properties.WriteValueCount (2);
			properties.WriteRaw (Concat (TnefBuilder.Int32Payload (1), TnefBuilder.Int32Payload (2)));
			properties.WriteInt32Property (TnefPropertyTag.Priority, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);
			builder.WriteTnefVersion ();

			return builder;
		}

		async Task RunTestReadPropertyAsync (bool async)
		{
			using var reader = new TnefReader (CreateSimpleMessage ().ToStream ());

			Assert.That (await ReadAsync (reader, async), Is.True);

			var prop = reader.GetPropertyReader ();

			Assert.ThrowsAsync<InvalidOperationException> (() => ReadPropertyAsync (prop, async), "not on a property");

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			var subject = await ReadPropertyAsync (prop, async);
			Assert.That (subject.Tag, Is.EqualTo (SubjectW));
			Assert.That (subject.Value, Is.EqualTo ("subject"));
			Assert.ThrowsAsync<InvalidOperationException> (() => ReadPropertyAsync (prop, async), "already read");

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (prop.ReadValueAsInt32 (), Is.EqualTo (1));
			Assert.That (async ? await prop.ReadNextValueAsync () : prop.ReadNextValue (), Is.True);
			Assert.ThrowsAsync<InvalidOperationException> (() => ReadPropertyAsync (prop, async), "advanced to another value");

			// Note: The remainder of the multi-valued property is skipped.
			var set = await ReadPropertySetAsync (prop, async);
			Assert.That (set, Has.Count.EqualTo (1), "ReadPropertySet excludes the current property");
			Assert.That (set.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (1));

			Assert.That (await ReadAsync (reader, async), Is.True);
			Assert.Throws<InvalidOperationException> (() => prop.ReadProperty (), "stale");
			Assert.Throws<InvalidOperationException> (() => prop.ReadPropertySet (), "stale");
			Assert.Throws<InvalidOperationException> (() => prop.ReadRowsAsPropertySets (), "stale");
			Assert.ThrowsAsync<InvalidOperationException> (() => prop.ReadPropertyAsync (), "stale");
			Assert.ThrowsAsync<InvalidOperationException> (() => prop.ReadPropertySetAsync (), "stale");
			Assert.ThrowsAsync<InvalidOperationException> (() => prop.ReadRowsAsPropertySetsAsync (), "stale");
		}

		[Test]
		public Task TestReadProperty () => RunTestReadPropertyAsync (false);

		[Test]
		public Task TestReadPropertyAsync () => RunTestReadPropertyAsync (true);

		async Task RunTestReadPropertyAfterValueConsumedAsync (bool async)
		{
			using var reader = new TnefReader (CreateSimpleMessage ().ToStream ());

			Assert.That (await ReadAsync (reader, async), Is.True);

			var prop = reader.GetPropertyReader ();

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (prop.ReadValueAsString (), Is.EqualTo ("subject"));
			Assert.ThrowsAsync<InvalidOperationException> (() => ReadPropertyAsync (prop, async));
		}

		[Test]
		public Task TestReadPropertyAfterValueConsumed () => RunTestReadPropertyAfterValueConsumedAsync (false);

		[Test]
		public Task TestReadPropertyAfterValueConsumedAsync () => RunTestReadPropertyAfterValueConsumedAsync (true);

		async Task RunTestReadRowsAsPropertySetsAsync (bool async)
		{
			var row1 = new TnefMapiPropertyBuilder ();
			var row2 = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			row1.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Alice");
			row1.WriteStringProperty (TnefPropertyTag.EmailAddressW, "alice@example.com");
			row1.WriteInt32Property (TnefPropertyTag.RecipientType, 1);
			row2.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Bob");
			row2.WriteInt32Property (TnefPropertyTag.RecipientType, 2);

			builder.WriteRecipientTable (row1, row2);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, new TnefMapiPropertyBuilder ().WriteInt32Property (TnefPropertyTag.Priority, 1));

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await ReadAsync (reader, async), Is.True);
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.RecipientTable));

			var rows = await ReadRowsAsync (reader.GetPropertyReader (), async);

			Assert.That (rows, Has.Count.EqualTo (2));
			Assert.That (rows[0], Has.Count.EqualTo (3));
			Assert.That (rows[0].GetString (TnefPropertyTag.DisplayNameW), Is.EqualTo ("Alice"));
			Assert.That (rows[0].GetString (TnefPropertyTag.EmailAddressW), Is.EqualTo ("alice@example.com"));
			Assert.That (rows[0].GetInt32 (TnefPropertyTag.RecipientType), Is.EqualTo (1));
			Assert.That (rows[1], Has.Count.EqualTo (2));
			Assert.That (rows[1].GetString (TnefPropertyTag.DisplayNameW), Is.EqualTo ("Bob"));
			Assert.That (rows[1].GetString (TnefPropertyTag.EmailAddressW), Is.Null);

			Assert.That (await ReadAsync (reader, async), Is.True);
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MapiProperties));
			Assert.That (await ReadRowsAsync (reader.GetPropertyReader (), async), Is.Empty, "not a table");
		}

		[Test]
		public Task TestReadRowsAsPropertySets () => RunTestReadRowsAsPropertySetsAsync (false);

		[Test]
		public Task TestReadRowsAsPropertySetsAsync () => RunTestReadRowsAsPropertySetsAsync (true);

		async Task RunTestReadRowsSkipsCurrentRowAsync (bool async)
		{
			var builder = new TnefBuilder ();

			builder.WriteRecipientTable (
				new TnefMapiPropertyBuilder ().WriteStringProperty (TnefPropertyTag.DisplayNameW, "Alice").WriteInt32Property (TnefPropertyTag.RecipientType, 1),
				new TnefMapiPropertyBuilder ().WriteStringProperty (TnefPropertyTag.DisplayNameW, "Bob"));

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await ReadAsync (reader, async), Is.True);

			var prop = reader.GetPropertyReader ();

			Assert.That (async ? await prop.ReadNextRowAsync () : prop.ReadNextRow (), Is.True);
			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);

			var rows = await ReadRowsAsync (prop, async);

			Assert.That (rows, Has.Count.EqualTo (1));
			Assert.That (rows[0].GetString (TnefPropertyTag.DisplayNameW), Is.EqualTo ("Bob"));
		}

		[Test]
		public Task TestReadRowsSkipsCurrentRow () => RunTestReadRowsSkipsCurrentRowAsync (false);

		[Test]
		public Task TestReadRowsSkipsCurrentRowAsync () => RunTestReadRowsSkipsCurrentRowAsync (true);

		async Task RunTestReadPropertyWithNoValuesAfterReadingValueAsync (bool async)
		{
			var multiValued = new TnefPropertyTag ((TnefPropertyId) 0x6610, TnefPropertyType.Unicode | TnefPropertyType.MultiValued);
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "body");
			properties.WritePropertyHeader (multiValued).WriteValueCount (0);
			properties.WriteInt32Property (TnefPropertyTag.Priority, 1);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await ReadAsync (reader, async), Is.True);

			var prop = reader.GetPropertyReader ();

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (async ? await prop.ReadValueAsStringAsync () : prop.ReadValueAsString (), Is.EqualTo ("body"));

			// A property with no values must not inherit the "already read" state of the previous property's value.
			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (prop.Tag, Is.EqualTo (multiValued));

			var empty = await ReadPropertyAsync (prop, async);

			Assert.That (empty.Count, Is.EqualTo (0));

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That ((await ReadPropertyAsync (prop, async)).Value, Is.EqualTo (1));
		}

		[Test]
		public Task TestReadPropertyWithNoValuesAfterReadingValue () => RunTestReadPropertyWithNoValuesAfterReadingValueAsync (false);

		[Test]
		public Task TestReadPropertyWithNoValuesAfterReadingValueAsync () => RunTestReadPropertyWithNoValuesAfterReadingValueAsync (true);

		#endregion

		#region Corpus

		static IEnumerable<string> CorpusFiles ()
		{
			return Directory.GetFiles (DataDir, "*.tnef").Select (Path.GetFileName);
		}

		// Reads every property of every attribute (including those of embedded messages) using the cursor API.
		static async Task ReadWithCursorAsync (TnefReader reader, List<object> results, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.MapiProperties && reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (prop, async)) {
					var values = new List<object> ();

					results.Add (prop.Tag);

					if (prop.IsEmbeddedMessage) {
						using var embedded = prop.OpenEmbeddedMessage ();
						await ReadWithCursorAsync (embedded, results, async);
						continue;
					}

					if (prop.ValueCount > 0) {
						do {
							values.Add (async ? await prop.ReadValueAsync () : prop.ReadValue ());
						} while (async ? await prop.ReadNextValueAsync () : prop.ReadNextValue ());
					}

					results.Add (prop.IsMultiValued ? values : values.SingleOrDefault ());
				}
			}
		}

		// Reads every property of every attribute (including those of embedded messages) using property sets.
		static async Task ReadWithPropertySetsAsync (TnefReader reader, List<object> results, bool async)
		{
			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.MapiProperties && reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (prop, async)) {
					results.Add (prop.Tag);

					if (prop.IsEmbeddedMessage) {
						using var embedded = prop.OpenEmbeddedMessage ();
						await ReadWithPropertySetsAsync (embedded, results, async);
						continue;
					}

					var property = await ReadPropertyAsync (prop, async);

					if (property.IsMultiValued)
						results.Add (((System.Collections.IEnumerable) property.Value).Cast<object> ().ToList ());
					else
						results.Add (property.Value);
				}
			}
		}

		async Task RunTestCorpusRoundTripAsync (string fileName, bool async)
		{
			var path = Path.Combine (DataDir, fileName);
			var expected = new List<object> ();
			var actual = new List<object> ();

			using (var reader = new TnefReader (File.OpenRead (path)))
				await ReadWithCursorAsync (reader, expected, async);

			using (var reader = new TnefReader (File.OpenRead (path)))
				await ReadWithPropertySetsAsync (reader, actual, async);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[TestCaseSource (nameof (CorpusFiles))]
		public Task TestCorpusRoundTrip (string fileName) => RunTestCorpusRoundTripAsync (fileName, false);

		[TestCaseSource (nameof (CorpusFiles))]
		public Task TestCorpusRoundTripAsync (string fileName) => RunTestCorpusRoundTripAsync (fileName, true);

		#endregion

		#region Allocation limits

		[Test]
		public void TestOptionsDefaults ()
		{
			var options = new TnefOptions ();

			Assert.That (options.MaxPropertyValueLength, Is.EqualTo (TnefOptions.DefaultMaxPropertyValueLength));
			Assert.That (options.MaxTotalDataBytes, Is.EqualTo (TnefOptions.DefaultMaxTotalDataBytes));
			Assert.That (TnefOptions.DefaultMaxPropertyValueLength, Is.EqualTo (32 * 1024 * 1024));
			Assert.That (TnefOptions.DefaultMaxTotalDataBytes, Is.EqualTo (64L * 1024 * 1024));

			options.MaxPropertyValueLength = 0;
			options.MaxTotalDataBytes = 0;
			Assert.That (options.MaxPropertyValueLength, Is.EqualTo (0));
			Assert.That (options.MaxTotalDataBytes, Is.EqualTo (0));

			options.MaxPropertyValueLength = 1234;
			options.MaxTotalDataBytes = 5678;

			var clone = options.Clone ();

			Assert.That (clone.MaxPropertyValueLength, Is.EqualTo (1234));
			Assert.That (clone.MaxTotalDataBytes, Is.EqualTo (5678));
		}

		async Task RunTestPropertyValueLimitAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, CreatePattern (1000));
			properties.WriteStringProperty (TnefPropertyTag.BodyW, new string ('x', 1000));
			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.Unicode | TnefPropertyType.MultiValued));
			properties.WriteValueCount (2);
			properties.WriteUnicodeValue ("ok");
			properties.WriteUnicodeValue (new string ('y', 1000));
			properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, CreatePattern (100));
			properties.WriteInt32Property (TnefPropertyTag.Priority, 7);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var options = new TnefOptions { MaxPropertyValueLength = 512 };
			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async, options, logger);

			Assert.That (set, Has.Count.EqualTo (5));
			Assert.That (set[0].Count, Is.EqualTo (0), "binary over limit");
			Assert.That (set[0].Value, Is.Null);
			Assert.That (set[1].Count, Is.EqualTo (0), "string over limit");
			Assert.That (set[2].Count, Is.EqualTo (0), "multi-valued string over limit");
			Assert.That (set.GetBytes (TnefPropertyTag.RtfCompressed), Is.EqualTo (CreatePattern (100)));
			Assert.That (set.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (7));

			var issues = logger.Issues.Where (issue => issue.Violation == TnefComplianceViolation.DataSizeLimitExceeded).ToList ();

			Assert.That (issues, Has.Count.EqualTo (3));
			Assert.That (Violations (logger).Distinct (), Is.EqualTo (new[] { TnefComplianceViolation.DataSizeLimitExceeded }));
		}

		[Test]
		public Task TestPropertyValueLimit () => RunTestPropertyValueLimitAsync (false);

		[Test]
		public Task TestPropertyValueLimitAsync () => RunTestPropertyValueLimitAsync (true);

		// Simulates a crafted stream containing many large (LOH-sized) values.
		async Task RunTestTotalDataLimitAsync (bool seekable, bool async)
		{
			const int valueLength = 100 * 1024;
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();
			var value = CreatePattern (valueLength);

			for (int i = 0; i < 50; i++)
				properties.WriteBinaryProperty (new TnefPropertyTag ((TnefPropertyId) (0x6600 + i), TnefPropertyType.Binary), value);
			properties.WriteInt32Property (TnefPropertyTag.Priority, 7);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var options = new TnefOptions { MaxTotalDataBytes = 10 * valueLength + valueLength / 2 };
			var set = await ReadPropertySetAsync (CreateStream (builder, seekable), TnefAttributeTag.MapiProperties, async, options, logger);

			Assert.That (set, Has.Count.EqualTo (51));

			for (int i = 0; i < 10; i++)
				Assert.That (set[i].Value, Is.EqualTo (value), $"value {i}");

			for (int i = 10; i < 50; i++)
				Assert.That (set[i].Count, Is.EqualTo (0), $"value {i}");

			Assert.That (set.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (7), "fixed-width values are not charged against the budget");
			Assert.That (Violations (logger).Count (v => v == TnefComplianceViolation.DataSizeLimitExceeded), Is.EqualTo (40));
		}

		[Test]
		public Task TestTotalDataLimit () => RunTestTotalDataLimitAsync (true, false);

		[Test]
		public Task TestTotalDataLimitAsync () => RunTestTotalDataLimitAsync (true, true);

		[Test]
		public Task TestTotalDataLimitNonSeekable () => RunTestTotalDataLimitAsync (false, false);

		[Test]
		public Task TestTotalDataLimitNonSeekableAsync () => RunTestTotalDataLimitAsync (false, true);

		async Task RunTestTotalDataLimitIsSharedWithEmbeddedMessagesAsync (bool async)
		{
			const int valueLength = 100 * 1024;
			var value = CreatePattern (valueLength);
			var logger = new TestTnefComplianceLogger ();

			var inner = new TnefBuilder ();
			inner.WriteMapiProperties (TnefAttributeLevel.Message, new TnefMapiPropertyBuilder ()
				.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, value)
				.WriteInt32Property (TnefPropertyTag.Priority, 3));

			var embedded = Concat (IID_IMessage.ToByteArray (), inner.ToArray ());
			var outer = new TnefBuilder ();
			outer.WriteMapiProperties (TnefAttributeLevel.Message, new TnefMapiPropertyBuilder ().WriteBinaryProperty (TnefPropertyTag.AttachDataBin, value));
			outer.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			outer.WriteMapiProperties (TnefAttributeLevel.Attachment, new TnefMapiPropertyBuilder ()
				.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage)
				.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, embedded));

			var options = new TnefOptions { MaxTotalDataBytes = valueLength + valueLength / 2 };

			using var reader = new TnefReader (outer.ToStream (), options) { ComplianceLogger = logger };

			Assert.That (await ReadAsync (reader, async), Is.True);
			var set = await ReadPropertySetAsync (reader.GetPropertyReader (), async);
			Assert.That (set[0].Value, Is.EqualTo (value));

			TnefPropertySet embeddedSet = null;

			while (await ReadAsync (reader, async)) {
				if (reader.Tag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.GetPropertyReader ();

				while (await ReadNextPropertyAsync (prop, async)) {
					if (!prop.IsEmbeddedMessage)
						continue;

					using var message = prop.OpenEmbeddedMessage ();

					Assert.That (await ReadAsync (message, async), Is.True);
					embeddedSet = await ReadPropertySetAsync (message.GetPropertyReader (), async);
				}
			}

			Assert.That (embeddedSet, Is.Not.Null);
			Assert.That (embeddedSet[0].Count, Is.EqualTo (0), "the embedded value exceeds the remaining budget");
			Assert.That (embeddedSet.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (3));
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.DataSizeLimitExceeded));
		}

		[Test]
		public Task TestTotalDataLimitIsSharedWithEmbeddedMessages () => RunTestTotalDataLimitIsSharedWithEmbeddedMessagesAsync (false);

		[Test]
		public Task TestTotalDataLimitIsSharedWithEmbeddedMessagesAsync () => RunTestTotalDataLimitIsSharedWithEmbeddedMessagesAsync (true);

		async Task RunTestReadValueOverLimitReturnsEmptyAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, CreatePattern (1000));
			properties.WriteStringProperty (TnefPropertyTag.BodyW, new string ('x', 1000));
			properties.WriteStringProperty (SubjectW, "small");
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var options = new TnefOptions { MaxPropertyValueLength = 512 };

			using var reader = new TnefReader (builder.ToStream (), options) { ComplianceLogger = logger };

			Assert.That (await ReadAsync (reader, async), Is.True);

			var prop = reader.GetPropertyReader ();

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (async ? await prop.ReadValueAsBytesAsync () : prop.ReadValueAsBytes (), Is.Empty);

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (async ? await prop.ReadValueAsStringAsync () : prop.ReadValueAsString (), Is.Empty);

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.True);
			Assert.That (async ? await prop.ReadValueAsStringAsync () : prop.ReadValueAsString (), Is.EqualTo ("small"));

			Assert.That (await ReadNextPropertyAsync (prop, async), Is.False);
			Assert.That (Violations (logger), Is.EqualTo (new[] { TnefComplianceViolation.DataSizeLimitExceeded, TnefComplianceViolation.DataSizeLimitExceeded }));
		}

		[Test]
		public Task TestReadValueOverLimitReturnsEmpty () => RunTestReadValueOverLimitReturnsEmptyAsync (false);

		[Test]
		public Task TestReadValueOverLimitReturnsEmptyAsync () => RunTestReadValueOverLimitReturnsEmptyAsync (true);

		async Task RunTestNamedPropertyNameOverLimitAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag (unchecked ((TnefPropertyId) 0x8001), TnefPropertyType.Long), PublicStrings, name: "AVeryLongPropertyName");
			properties.WriteRaw (TnefBuilder.Int32Payload (5));
			properties.WriteInt32Property (TnefPropertyTag.Priority, 7);
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			var options = new TnefOptions { MaxPropertyValueLength = 16 };
			var set = await ReadPropertySetAsync (builder.ToStream (), TnefAttributeTag.MapiProperties, async, options, logger);

			Assert.That (set, Has.Count.EqualTo (2));
			Assert.That (set[0].Name, Is.EqualTo (new TnefNameId (PublicStrings, string.Empty)));
			Assert.That (set[0].Value, Is.EqualTo (5));
			Assert.That (set.GetInt32 (TnefPropertyTag.Priority), Is.EqualTo (7));
			Assert.That (Violations (logger), Is.EqualTo (new[] { TnefComplianceViolation.DataSizeLimitExceeded }));
		}

		[Test]
		public Task TestNamedPropertyNameOverLimit () => RunTestNamedPropertyNameOverLimitAsync (false);

		[Test]
		public Task TestNamedPropertyNameOverLimitAsync () => RunTestNamedPropertyNameOverLimitAsync (true);

		async Task RunTestLargeValueFromNonSeekableStreamAsync (bool async)
		{
			var value = CreatePattern (300 * 1024 + 3);
			var builder = new TnefBuilder ();

			builder.WriteMapiProperties (TnefAttributeLevel.Message, new TnefMapiPropertyBuilder ()
				.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, value)
				.WriteStringProperty (TnefPropertyTag.BodyW, new string ('z', 100 * 1024)));

			var set = await ReadPropertySetAsync (CreateStream (builder, false), TnefAttributeTag.MapiProperties, async);

			Assert.That (set.GetBytes (TnefPropertyTag.AttachDataBin), Is.EqualTo (value));
			Assert.That (set.GetString (TnefPropertyTag.BodyW), Is.EqualTo (new string ('z', 100 * 1024)));
		}

		[Test]
		public Task TestLargeValueFromNonSeekableStream () => RunTestLargeValueFromNonSeekableStreamAsync (false);

		[Test]
		public Task TestLargeValueFromNonSeekableStreamAsync () => RunTestLargeValueFromNonSeekableStreamAsync (true);

		async Task RunTestTruncatedLargeValueFromNonSeekableStreamAsync (bool async)
		{
			var value = CreatePattern (200 * 1024);
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteMapiProperties (TnefAttributeLevel.Message, new TnefMapiPropertyBuilder ()
				.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, value));

			// Note: Truncate the stream in the middle of the value.
			var truncated = new MemoryStream (builder.ToArray (), 0, 150 * 1024, false);
			var set = await ReadPropertySetAsync (new NonSeekableStream (truncated), TnefAttributeTag.MapiProperties, async, logger: logger);
			var bytes = set.GetBytes (TnefPropertyTag.AttachDataBin);

			Assert.That (bytes, Is.Not.Null);
			Assert.That (bytes.Length, Is.LessThan (150 * 1024));
			Assert.That (bytes, Is.EqualTo (value.Take (bytes.Length).ToArray ()));
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public Task TestTruncatedLargeValueFromNonSeekableStream () => RunTestTruncatedLargeValueFromNonSeekableStreamAsync (false);

		[Test]
		public Task TestTruncatedLargeValueFromNonSeekableStreamAsync () => RunTestTruncatedLargeValueFromNonSeekableStreamAsync (true);

		async Task RunTestBogusMultiValuedCountDoesNotAllocateAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.I8 | TnefPropertyType.MultiValued));
			properties.WriteValueCount (0x08000000);
			properties.WriteRaw (Concat (Int64Payload (1), Int64Payload (2), Int64Payload (3)));
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, length: 0x7FFFFFF0);

			long before = GC.GetAllocatedBytesForCurrentThread ();
			var set = await ReadPropertySetAsync (CreateStream (builder, false), TnefAttributeTag.MapiProperties, async, logger: logger);
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;

			Assert.That (set, Has.Count.EqualTo (1));
			Assert.That (set[0].TryGetValues<long> (out var values), Is.True);
			Assert.That (values, Is.EqualTo (new[] { 1L, 2L, 3L }));
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));

			// Note: Trusting the count would have allocated a 1 GB array. Async continuations may run on other
			// threads, so the allocation can only be measured reliably for the synchronous code path.
			if (!async && !TestHelper.IsCodeCoverageEnabled)
				Assert.That (allocated, Is.LessThan (4 * 1024 * 1024));
		}

		[Test]
		public Task TestBogusMultiValuedCountDoesNotAllocate () => RunTestBogusMultiValuedCountDoesNotAllocateAsync (false);

		[Test]
		public Task TestBogusMultiValuedCountDoesNotAllocateAsync () => RunTestBogusMultiValuedCountDoesNotAllocateAsync (true);

		async Task RunTestManyValuesGrowArrayAsync (bool async)
		{
			const int count = 5000;
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();
			var raw = new MemoryStream ();

			for (int i = 0; i < count; i++)
				raw.Write (TnefBuilder.Int32Payload (i), 0, 4);

			properties.WritePropertyHeader (new TnefPropertyTag ((TnefPropertyId) 0x6601, TnefPropertyType.Long | TnefPropertyType.MultiValued));
			properties.WriteValueCount (count);
			properties.WriteRaw (raw.ToArray ());
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties, length: 0x7FFFFFF0);

			// Note: The stream is not seekable and the attribute length is bogus, so the array of values must grow.
			var set = await ReadPropertySetAsync (CreateStream (builder, false), TnefAttributeTag.MapiProperties, async);

			Assert.That (set[0].TryGetValues<int> (out var values), Is.True);
			Assert.That (values, Is.EqualTo (Enumerable.Range (0, count).ToArray ()));
		}

		[Test]
		public Task TestManyValuesGrowArray () => RunTestManyValuesGrowArrayAsync (false);

		[Test]
		public Task TestManyValuesGrowArrayAsync () => RunTestManyValuesGrowArrayAsync (true);

		#endregion
	}
}
