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

			var reader = new TnefReader (builder.ToStream (), 1252, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.MapiProperties)
					continue;

				if (reader.TnefPropertyReader.ReadNextProperty ())
					return reader;
			}

			reader.Dispose ();

			throw new InvalidOperationException ("Failed to locate the property.");
		}

		static string ReadTextValue (TnefPropertyReader prop)
		{
			var buffer = new char[64];
			var text = new StringBuilder ();
			int n;

			while ((n = prop.ReadTextValue (buffer, 0, buffer.Length)) > 0)
				text.Append (buffer, 0, n);

			return text.ToString ();
		}

		[Test]
		public void TestReadTextValueUnicodeProperty ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);

			properties.WriteStringProperty (tag, "This is the subject");

			using var reader = ReadSingleProperty (properties);

			Assert.That (ReadTextValue (reader.TnefPropertyReader), Is.EqualTo ("This is the subject\0"));
		}

		[Test]
		public void TestReadTextValueString8Property ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8);

			properties.WriteStringProperty (tag, "Caf\u00e9 au lait", Encoding.GetEncoding (1252));

			using var reader = ReadSingleProperty (properties);

			Assert.That (ReadTextValue (reader.TnefPropertyReader), Is.EqualTo ("Caf\u00e9 au lait\0"));
		}

		[Test]
		public void TestReadTextValueNonTextProperty ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long);

			properties.WriteInt32Property (tag, 1234);

			using var reader = ReadSingleProperty (properties);
			var prop = reader.TnefPropertyReader;
			var buffer = new char[64];

			Assert.Throws<InvalidOperationException> (() => prop.ReadTextValue (buffer, 0, buffer.Length));
		}

		[Test]
		public void TestReadTextValueAttribute ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, Encoding.ASCII.GetBytes ("This is the body\0"));

			using var reader = new TnefReader (builder.ToStream (), 1252, TnefComplianceMode.Loose);

			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.Body)
					continue;

				Assert.That (ReadTextValue (reader.TnefPropertyReader), Is.EqualTo ("This is the body\0"));
				return;
			}

			Assert.Fail ("Failed to locate the attBody attribute.");
		}

		[Test]
		public void TestReadTextValueArgumentExceptions ()
		{
			var properties = new TnefMapiPropertyBuilder ();
			var tag = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);

			properties.WriteStringProperty (tag, "This is the subject");

			using var reader = ReadSingleProperty (properties);
			var prop = reader.TnefPropertyReader;
			var buffer = new char[64];

			Assert.Throws<ArgumentNullException> (() => prop.ReadTextValue (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (buffer, 0, -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => prop.ReadTextValue (buffer, 0, buffer.Length + 1));
		}
	}
}
