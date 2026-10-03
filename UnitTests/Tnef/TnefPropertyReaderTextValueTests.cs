//
// TnefPropertyReaderTextValueTests.cs
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

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertyReaderTextValueTests
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

		[Test]
		public void TestReadValueAsStringUnicodeProperty ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "This is the subject");

			using var reader = ReadSingleProperty (properties);

			Assert.That (reader.GetPropertyReader ().ReadValueAsString (), Is.EqualTo ("This is the subject"));
		}

		[Test]
		public async Task TestReadValueAsStringUnicodePropertyAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode), "This is the subject");

			using var reader = await ReadSinglePropertyAsync (properties);

			Assert.That (await reader.GetPropertyReader ().ReadValueAsStringAsync (), Is.EqualTo ("This is the subject"));
		}

		[Test]
		public void TestReadValueAsStringString8Property ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8), "Caf\u00e9 au lait", Encoding.GetEncoding (1252));

			using var reader = ReadSingleProperty (properties);

			Assert.That (reader.GetPropertyReader ().ReadValueAsString (), Is.EqualTo ("Caf\u00e9 au lait"));
		}

		[Test]
		public async Task TestReadValueAsStringString8PropertyAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8), "Caf\u00e9 au lait", Encoding.GetEncoding (1252));

			using var reader = await ReadSinglePropertyAsync (properties);

			Assert.That (await reader.GetPropertyReader ().ReadValueAsStringAsync (), Is.EqualTo ("Caf\u00e9 au lait"));
		}

		[Test]
		public void TestReadValueAsStringNonTextProperty ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long), 1234);

			using var reader = ReadSingleProperty (properties);
			var prop = reader.GetPropertyReader ();

			Assert.Throws<InvalidOperationException> (() => prop.ReadValueAsString ());
		}

		[Test]
		public async Task TestReadValueAsStringNonTextPropertyAsync ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteInt32Property (new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long), 1234);

			using var reader = await ReadSinglePropertyAsync (properties);
			var prop = reader.GetPropertyReader ();

			Assert.ThrowsAsync<InvalidOperationException> (async () => await prop.ReadValueAsStringAsync ());
		}

		[Test]
		public void TestReadValueAsStringAttribute ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, Encoding.ASCII.GetBytes ("This is the body\0"));

			using var reader = new TnefReader (builder.ToStream ());

			while (reader.Read ()) {
				if (reader.Tag != TnefAttributeTag.Body)
					continue;

				Assert.That (reader.ReadValueAsString (), Is.EqualTo ("This is the body"));
				return;
			}

			Assert.Fail ("Failed to locate the attBody attribute.");
		}

		[Test]
		public async Task TestReadValueAsStringAttributeAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, Encoding.ASCII.GetBytes ("This is the body\0"));

			using var reader = new TnefReader (builder.ToStream ());

			while (await reader.ReadAsync ()) {
				if (reader.Tag != TnefAttributeTag.Body)
					continue;

				Assert.That (await reader.ReadValueAsStringAsync (), Is.EqualTo ("This is the body"));
				return;
			}

			Assert.Fail ("Failed to locate the attBody attribute.");
		}
	}
}