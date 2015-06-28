//
// HtmlEntityDecoderTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using System;
using System.IO;

using NUnit.Framework;

using Newtonsoft.Json;
using MimeKit.Text;

namespace UnitTests {
	[TestFixture]
	public class HtmlEntityDecoderTests
	{
		[Test]
		public void TestDecodeEntities ()
		{
			var path = Path.Combine ("..", "..", "TestData", "html", "HtmlEntities.json");
			var decoder = new HtmlEntityDecoder ();

			using (var json = new JsonTextReader (new StreamReader (path))) {
				while (json.Read ()) {
					string name, value;

					if (json.TokenType == JsonToken.StartObject)
						continue;

					if (json.TokenType != JsonToken.PropertyName)
						break;

					name = (string) json.Value;

					if (!json.Read () || json.TokenType != JsonToken.StartObject)
						break;

					// read to the "codepoints" property
					if (!json.Read () || json.TokenType != JsonToken.PropertyName)
						break;

					// skip the array of integers...
					if (!json.Read () || json.TokenType != JsonToken.StartArray)
						break;

					while (json.Read ()) {
						if (json.TokenType == JsonToken.EndArray)
							break;
					}

					// the property should be "characters" - this is what we want
					if (!json.Read () || json.TokenType != JsonToken.PropertyName)
						break;

					value = json.ReadAsString ();

					if (!json.Read () || json.TokenType != JsonToken.EndObject)
						break;

					for (int i = 0; i < name.Length && name[i] != ';'; i++)
						Assert.IsTrue (decoder.Push (name[i]), "Failed to push char #{0} of \"{1}\".", i, name);

					Assert.AreEqual (value, decoder.GetValue (), "Decoded entity did not match for \"{0}\".", name);

					decoder.Reset ();
				}
			}
		}
	}
}
